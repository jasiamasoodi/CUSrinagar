using CUSrinagar.Enums;
using CUSrinagar.Models;
using GeneralModels;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUSrinagar.DataManagers.SQLQueries
{
    public class MigrationSQLQueries
    {
        //      internal string GetMigrationFormList = @"SELECT * FROM  FormsToDownload INNER JOIN ARGPersonalInformation_UG
        //      ON ARGPersonalInformation_UG.Student_ID = FormsToDownload.Student_ID
        //INNER JOIN ARGCoursesApplied_UG ON ARGCoursesApplied_UG.Student_ID = ARGPersonalInformation_UG.Student_ID
        //INNER JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = ARGCoursesApplied_UG.Course_ID  ";

        //internal string GetAppUsers = @"SELECT DISTINCT AppUsers.User_ID from AppUsers 
        //                                JOIN AppUserRoles ON AppUserRoles.User_ID = AppUsers.User_ID ";
        internal SqlCommand GetALLMigrationFormList(Parameters parameter)
        {
            Programme programme = (Programme)Convert.ToInt32(parameter.Filters.Where(x => x.Column == "Programme").FirstOrDefault().Value);
            string prog = new GeneralFunctions().GetProgrammePostFix(programme);
            string query = string.Empty;

            query = $@"  SELECT Form_ID,MI.Student_ID,MI.ForwardedToCollege,MI.REMARKSBYCOLLEGE,FormType,MI.Programme ,MI.FormStatus ,AcceptedBy ,Pinfo.FullName
                ,Pinfo.CusRegistrationNo ,FathersName,MothersName,
				CASE when TRY_CONVERT(UNIQUEIDENTIFIER, NewCollege ) IS NOT NULL
				THEN (SELECT Collegefullname  FROM admcollegemaster WHERE college_id=NewCollege)
				ELSE
                NewCollege
				END AS NewCollege,CollegeFullName,Remarks,CourseFullName ,Pinfo.CurrentSemesterOrYear ,Pinfo.Batch FROM  MigrationDetail MI
                  join ARGPersonalInformation{prog} Pinfo ON Pinfo.Student_ID = MI.Student_ID 
                 left JOIN VWStudentCourse ON VWStudentCourse.Student_ID = MI.Student_ID  ";
            FilterHelper helper = new GeneralFunctions().GetWhereClause<MigrationPI>(parameter.Filters);
            query += helper.WhereClause;
            query += new GeneralFunctions().GetPagedQuery(query, parameter);
            helper.Command.CommandText = query;
            return helper.Command;
        }
        internal SqlCommand GetMigrationFormList(Parameters parameter)
        {
            string query = string.Empty;

            query = $@" SELECT Form_ID,CUSRegistrationNo,MI.Student_ID,MI.ForwardedToCollege,MI.REMARKSBYCOLLEGE,FormType,MI.Programme 
               ,MI.FormStatus ,AcceptedBy ,FullName,CusRegistrationNo ,FullName ,CourseFullName ,CurrentSemesterOrYear ,Batch FROM 
                MigrationDetail MI INNER JOIN VWStudentCourse ON VWStudentCourse.Student_ID = MI.Student_ID ";
            FilterHelper helper = new GeneralFunctions().GetWhereClause<MigrationPI>(parameter.Filters);
            if (parameter.Filters != null && parameter.Filters.Count() > 0)
            {
                query += helper.WhereClause;
            }
            if (Guid.Empty == AppUserHelper.College_ID)
                query += $"  AND  (FormStatus = {(int)FormStatus.Rejected } OR FormStatus = {(int)FormStatus.Accepted } OR FormStatus = {(int)FormStatus.Submitted } )";
            else
                query += $"  AND AcceptCollege_ID = @College_ID AND (FormStatus = {(int)FormStatus.InProcess } OR FormStatus = {(int)FormStatus.Submitted } )";
            helper.Command.Parameters.AddWithValue("@College_ID", AppUserHelper.College_ID);
            query += new GeneralFunctions().GetPagedQuery(query, parameter);

            helper.Command.CommandText = query;
            return helper.Command;
        }



        internal SqlCommand GetMigration(Parameters parameter)
        {
            string query = string.Empty;
            FilterHelper helper = new GeneralFunctions().GetWhereClause<MigrationPI>(parameter.Filters);
            query += helper.WhereClause;

            query = $@"SELECT Form_ID,FormsToDownload.Student_ID,FormType,FormsToDownload.Programme ,AcceptedBy ,FullName ,FullName ,CourseFullName 
                   ,CurrentSemesterOrYear ,Batch FROM  MigrationDetail MI INNER JOIN  VWStudentCourse ON VWStudentCourse.Student_ID = MI.Student_ID ";
            helper.Command.CommandText = query;
            return helper.Command;
        }

        internal SqlCommand GetMigration(Guid Student_ID, string MigrationType, PrintProgramme printProgramme)
        {
            string prog = new GeneralFunctions().GetProgrammePostFix(printProgramme);
            SqlCommand command = new SqlCommand();
            string query = string.Empty;
            query = $"SELECT Form_ID,TotalFeeM,MI.NewCollege,Remarks,FormType,MI.FormStatus,TxnDate TransactionDate,MI.SerialNo,MI.NewSubjects,MI.Email,MI.Mobile,TxnReferenceNo TransactionNo From  MigrationDetail MI Left JOIN PaymentDetails{prog} PD ON entity_id=student_ID AND ModuleType={(int)PaymentModuleType.Migration}  Where Student_ID ='{Student_ID}' " +
                $" ORDER BY CreatedOn desc";
            command.CommandText = query;
            return command;
        }
    }
}



