using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CUSrinagar.Models;
using Terex;
using System.Data.SqlClient;
using CUSrinagar.DataManagers.SQLQueries;
using System.Web.Mvc;
using GeneralModels;

namespace CUSrinagar.DataManagers
{

    public class SyllabusDB
    {



        public List<Syllabus> GetALLSyllabusByFilter(string filters)
        {
            return new MSSQLFactory().GetObjectList<Syllabus>(new SyllabusSQLQueries().GetALLSyllabusByFilter(filters));
        }

        public int AddSyllabus(Syllabus input)
        {
            return new MSSQLFactory().InsertRecord<Syllabus>(input, "Syllabus");
        }

        public int EditSyllabus(Syllabus input, List<string> ignoreQuery, List<string> ignoreParameter)
        {
            SqlCommand sqlCommand = new MSSQLFactory().UpdateRecord<Syllabus>(input, ignoreQuery, ignoreParameter);
            sqlCommand.CommandText += " WHERE Syllabus_ID=@Syllabus_ID";
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);

        }



        public Syllabus GetSyllabusById(Guid syllabus_Id)
        {
            return new MSSQLFactory().GetObject<Syllabus>(new SyllabusSQLQueries().GetSyllabusById(syllabus_Id));
        }


        //public List<Syllabus> GetAllSyllabusRecords(Parameters parameter)
        //{
        //    return new MSSQLFactory().GetObjectList<Syllabus>(new SyllabusSQLQueries().GetAllSyllabusRecords(parameter));
        //}
        public List<Syllabus> List(Parameters parameter)
        {
            string query = $@"SELECT Syllabus.*,C.Programme,S.Semester,S.SubjectFullName,C.CourseFullName,S.SubjectType,D.DepartmentFullName,S.SubjectCode
                                FROM Syllabus 
                                LEFT JOIN ADMSubjectMaster S ON Syllabus.Subject_ID=S.Subject_ID
                                LEFT JOIN ADMCourseMaster C  ON S.Course_ID=C.Course_ID OR C.Course_ID = Syllabus.Course_ID
                                LEFT JOIN Department D ON D.Department_ID = S.Department_ID";
            SqlCommand command = new SqlCommand();
            FilterHelper helper = new GeneralFunctions().GetWhereClause<Syllabus>(parameter.Filters);
            query += helper.WhereClause;
            query += new GeneralFunctions().GetPagedQuery(query, parameter);
            helper.Command.CommandText = query;
            return new MSSQLFactory().GetObjectList<Syllabus>(helper.Command);
        }

        public List<Syllabus> GetAllSkillCourses(Parameters parameter)
        {
            return new MSSQLFactory().GetObjectList<Syllabus>(new SyllabusSQLQueries().GetAllSkillCourses(parameter));
        }


        public List<ADMSubjectMaster> ExistSyllabusBySubjectName(string session, short? semester, Guid? subject_ID, Guid? course_id )
        {
            return new MSSQLFactory().GetObjectList<ADMSubjectMaster>(new SyllabusSQLQueries().ExistSyllabusBySubjectName( session,  semester, subject_ID, course_id ));
        }
        public int Delete(Guid id)
        {
            SqlCommand cmd = new SqlCommand();
            string Query = $"Delete from Syllabus";
            cmd.Parameters.AddWithValue("@Syllabus_ID", id);
            cmd.CommandText = Query + " Where Syllabus_ID = @Syllabus_ID";
            return new MSSQLFactory().ExecuteNonQuery(cmd);
        }
        public List<Syllabus> ListOfSkillCourses(Parameters parameter)
        {
            return new MSSQLFactory().GetObjectList<Syllabus>(new SyllabusSQLQueries().ListOfSkillCourses(parameter));
        }

        public int ReplaceSubject(Guid findSubject_ID, Guid replaceWithSubject_ID)
        {
            var query = $@"Update Syllabus SET Subject_ID='{replaceWithSubject_ID}' Where Subject_ID='{findSubject_ID}'";
            return new MSSQLFactory().ExecuteNonQuery(query);
        }
    }

}
