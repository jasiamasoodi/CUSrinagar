using CUSrinagar.Models;
using GeneralModels;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using CUSrinagar.Enums;
using System.Security.Principal;

namespace CUSrinagar.DataManagers.SQLQueries
{
    public class AssignSubjectSQLQueries
    {

        internal SqlCommand GetAllProfessorSubjects(Parameters parameter, string Column)
        {
            List<SearchFilter> searchFilters = parameter.Filters?.Where(x => x.Column == "ProfessorResponseOn").ToList();
            parameter.Filters?.RemoveAll(i => searchFilters.Contains(i));
            string query = $@"SELECT ProfessorCRClasses_ID,Batch,CourseFullName,ProfessorCRClasses.Semester,SubjectFullName ,
                        (SELECT FullName FROM AppUsers WHERE User_ID=Professor_ID) Professor,
                        (SELECT FullName FROM AppUsers WHERE User_ID=CR_ID) CR
                        FROM ProfessorCRClasses 
                        JOIN ADMSubjectMaster
                        ON ADMSubjectMaster.Subject_ID = ProfessorCRClasses.Subject_ID
                        JOIN ADMCourseMaster
                        ON ADMCourseMaster.Course_ID = ADMSubjectMaster.Course_ID";
            FilterHelper helper = new GeneralFunctions().GetWhereClause<ProfessorCRClasses>(parameter.Filters);
            query += helper.WhereClause;
            if (AppUserHelper.AppUsercompact.UserRoles.Any(x => x == AppRoles.College_ClassRepresentative) || AppUserHelper.AppUsercompact.UserRoles.Any(x => x == AppRoles.College_AssistantProfessor))
            {
                query += $" AND {Column} = '{AppUserHelper.User_ID}'";
            }
            query += " Order By ProfessorCRClasses.Semester,SubjectFullname";
            helper.Command.CommandText = query;
            return helper.Command;
        }

        internal SqlCommand GetDetail(Parameters parameter)
        {
            string query = $@"SELECT ProfessorCRClasses_ID,ProfessorCRClassDetails_ID, ProfessorStatus,ProfessorRemarks,ClassDatenTimeByProfessor,
                               DurationByProfessor,ProfessorResponseOn,CRStatus,CRRemarks,
                               ClassDatenTimeByCR,DurationByCR,CRResponseOn  FROM ProfessorCRClassDetails";
            FilterHelper helper = new GeneralFunctions().GetWhereClause<ProfessorCRClasses>(parameter.Filters);
            query += (helper.WhereClause);
            query += " Order By ProfessorResponseOn Desc";
            helper.Command.CommandText = query;
            return helper.Command;
        }

        internal SqlCommand GetAllProfessorList(Parameters parameter)
        {
            string query = $@" Select * from (SELECT PC.ProfessorCRClasses_ID,FullName Professor,ADMCollegeMaster.College_ID,
                        (SELECT FullName FROM AppUsers WHERE User_ID=CR_ID) CR,SubjectFullName,PC.Semester,Batch
						,COUNT(ADMSubjectMaster.Subject_ID) ClassCount
						,(SELECT COUNT(CD.ProfessorCRClasses_ID) FROM ProfessorCRClassDetails CD WHERE CD.ProfessorCRClasses_ID=PC.ProfessorCRClasses_ID
						AND(CD.ProfessorStatus<> CD.CRStatus OR CD.DurationByProfessor <> CD.DurationByCR )) ObjectionCount
                       ,(SELECT COUNT(CD.ProfessorCRClasses_ID) FROM ProfessorCRClassDetails CD WHERE CD.ProfessorCRClasses_ID=PC.ProfessorCRClasses_ID
						AND CD.ProfessorStatus={(int)ProfessorStatus.Class_Engaged} ) ClassesEngaged
                         FROM ProfessorCRClassDetails PCD
							   Join ProfessorCRClasses  PC
							   ON PC.ProfessorCRClasses_ID = PCD.ProfessorCRClasses_ID
                        JOIN ADMSubjectMaster
                        ON ADMSubjectMaster.Subject_ID = PC.Subject_ID
						JOIN AppUsers ON User_ID=Professor_ID
						JOIN ADMCollegeMaster ON AppUsers.College_ID=ADMCollegeMaster.College_ID
						GROUP BY PC.ProfessorCRClasses_ID,ADMCollegeMaster.College_ID,FullName ,CR_ID,SubjectFullName,PC.Semester,Batch)t";
            FilterHelper helper = new GeneralFunctions().GetWhereClause<ProfessorCRClasses>(parameter.Filters);
            query += (helper.WhereClause);
            query += " Order By t.Professor,t.Semester  Desc";
            helper.Command.CommandText = query;
            return helper.Command;
        }
    }
}
