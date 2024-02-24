using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CUSrinagar.Models;
using GeneralModels;

namespace CUSrinagar.DataManagers.SQLQueries
{
    public class SyllabusSQLQueries
    {

        internal SqlCommand GetAllSyllabusRecords(Parameters parameter)
        {
            string query = $@" SELECT
                                     *
                                    FROM
                                    (   SELECT
                                         Syllabus_ID
                                        ,ADMCourseMaster.Programme AS ProgrammeId
		                                ,ADMCourseMaster.Course_ID AS Course_ID
		                                ,ADMSubjectMaster.Subject_ID AS Subject_ID
		                                ,Syllabus.Semester
                                        ,ADMSubjectMaster.SubjectFullName  AS SubjectName
                                        ,ADMCourseMaster.CourseFullName AS CourseName
                                        ,[Session]
		                                ,SyllabusFileName
                                        ,Syllabus.Status
                                        ,SubjectType
                                        ,DepartmentFullName AS Department
		                                 FROM Syllabus 
		                                left join ADMSubjectMaster ON Syllabus.Subject_ID=ADMSubjectMaster.Subject_ID
		                                join ADMCourseMaster  ON ADMSubjectMaster.Course_ID=ADMCourseMaster.Course_ID OR ADMCourseMaster.Course_ID = Syllabus.Course_ID
	                                    LEFT JOIN Department ON Department.Department_ID = ADMSubjectMaster.Department_ID
	                                ) AS tempSyllabus ";
            SqlCommand command = new SqlCommand();
            FilterHelper helper = new GeneralFunctions().GetWhereClause<Syllabus>(parameter.Filters);
            query += helper.WhereClause;
            query += new GeneralFunctions().GetPagedQuery(query, parameter);
            helper.Command.CommandText = query;
            return helper.Command;
        }
        internal SqlCommand GetAllSyllabus(Guid course_ID)
        {
            SqlCommand command = new SqlCommand();
            command.CommandText = @"SELECT * from ADMCourseMaster where Course_ID=@course_ID";
            return command;
        }

        internal SqlCommand GetALLSyllabusByFilter(string filters)
        {
            FilterHelper filterHelper = new SerializeFilter().GetWhereClauseFromFilter(filters);
            SqlCommand command = filterHelper.Command;
            command.CommandText = $@" SELECT
                                     *
                                    FROM
                                    (   SELECT
                                         Syllabus_ID
                                        ,ADMCourseMaster.Programme AS ProgrammeId
		                                ,ADMCourseMaster.Course_ID AS CourseId
		                                ,ADMSubjectMaster.Subject_ID AS SubjectId
		                                ,Syllabus.Semester
                                        ,ADMSubjectMaster.SubjectFullName  AS SubjectName
                                        ,ADMCourseMaster.CourseFullName AS CourseName
                                        ,[Session]
		                                ,SyllabusFileName
		                                 FROM Syllabus 
		                                inner join ADMSubjectMaster
		                                ON Syllabus.Subject_ID=ADMSubjectMaster.Subject_ID
		                                inner join ADMCourseMaster
		                                ON ADMSubjectMaster.Course_ID=ADMCourseMaster.Course_ID
	                                ) AS tempSyllabus  {filterHelper.WhereClause}";
            return command;
        }


        internal SqlCommand ExistSyllabusBySubjectName(string session, short? semester, Guid? subject_ID, Guid? course_id)
        {
            SqlCommand command = new SqlCommand();
            command.CommandText = $@"SELECT * FROM [Syllabus] WHERE {(subject_ID.HasValue ? $"Subject_ID='{subject_ID}'" : $"Course_ID='{course_id}'")} AND Session = @Session  {(semester.HasValue ? "and semester="+semester : "")}";
            command.Parameters.AddWithValue("@Session", session);
            return command;
        }


        internal SqlCommand GetSyllabusById(Guid syllabus_Id)
        {
            SqlCommand command = new SqlCommand();
            command.CommandText = @"SELECT * FROM [Syllabus]
                                     WHERE Syllabus_ID = @syllabus_Id";

            command.Parameters.AddWithValue("@Syllabus_ID", syllabus_Id);
            return command;
        }


        internal SqlCommand GetAllSkillCourses(Parameters parameter)
        {
            string query = $@"SELECT ADMCourseMaster.Programme,ADMCourseMaster.CourseFullName,Syllabus.Semester,Syllabus.SyllabusFileName,ADMSubjectMaster.SubjectFullName as SubjectName from Syllabus inner join ADMSubjectMaster on Syllabus.Subject_ID=ADMSubjectMaster.Subject_ID
                                inner join ADMCourseMaster on ADMCourseMaster.Course_ID=ADMSubjectMaster.Course_ID
                                where ADMSubjectMaster.subjectType=3 AND ADMCourseMaster.Course_ID='4A583C48-313C-4FEC-80B6-2BEC220E71E6' 
								ORDER by Semester,SubjectFullName";
            SqlCommand command = new SqlCommand();
            FilterHelper helper = new GeneralFunctions().GetWhereClause<Syllabus>(parameter.Filters);
            query += helper.WhereClause;
            query += new GeneralFunctions().GetPagedQuery(query, parameter);
            helper.Command.CommandText = query;
            return helper.Command;
        }
        
        internal SqlCommand ListOfSkillCourses(Parameters parameter)
        {
            string query = $@"SELECT ADMCourseMaster.Programme,ADMCourseMaster.CourseFullName,Syllabus.Semester,Syllabus.SyllabusFileName,ADMSubjectMaster.SubjectFullName as SubjectName from Syllabus inner join ADMSubjectMaster on Syllabus.Subject_ID=ADMSubjectMaster.Subject_ID
                                inner join ADMCourseMaster on ADMCourseMaster.Course_ID=ADMSubjectMaster.Course_ID
                                where ADMSubjectMaster.subjectType=3 order by Programme,CourseFullName,Syllabus.Semester,SubjectFullName";
            SqlCommand command = new SqlCommand();
            FilterHelper helper = new GeneralFunctions().GetWhereClause<Syllabus>(parameter.Filters);
            query += helper.WhereClause;
            query += new GeneralFunctions().GetPagedQuery(query, parameter);
            helper.Command.CommandText = query;
            return helper.Command;
        }

     

    }
}
