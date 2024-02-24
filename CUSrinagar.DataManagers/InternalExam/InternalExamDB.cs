using CUSrinagar.DataManagers.SQLQueries;
using CUSrinagar.Enums;
using CUSrinagar.Models;
using GeneralModels;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Terex;

namespace CUSrinagar.DataManagers
{

    public class InternalExamDB
    {
        public List<InternalExam> GetInternalExamList(Parameters parameters)
        {
            return new MSSQLFactory().GetObjectList<InternalExam>(new InternalExamSQLQueries().GetInternalExamList(parameters));
        }



        public int Save(InternalExam internalExam)
        {
            return new MSSQLFactory().InsertRecord(internalExam);
        }

        public List<SelectListItem> GetSubjects(Guid Student_ID, int Semester)
        {
            SqlCommand command = new SqlCommand();
            command.CommandText = $@"SELECT CAST(Subject_ID AS NVARCHAR(100)) AS [Value],SubjectFullName AS [Text] FROM VWStudentWithDetail WHERE Student_ID=@Student_ID AND Semester=@Semester";
            command.Parameters.AddWithValue("@Student_ID", Student_ID);
            command.Parameters.AddWithValue("@Semester", Semester);
            return new MSSQLFactory().GetObjectList<SelectListItem>(command);
        }
        public List<InternalQuestions> GetQuestionList(Parameters parameters)
        {
            return new MSSQLFactory().GetObjectList<InternalQuestions>(new InternalExamSQLQueries().GetQuestionList(parameters));
        }
        public List<InternalQuestionOptions> GetOptionList(Guid Question_Id)
        {
            return new MSSQLFactory().GetObjectList<InternalQuestionOptions>(new InternalExamSQLQueries().GetOptionList(Question_Id));
        }
        public List<SelectListItem> GetSubjectList(Guid exam_Id)
        {
            return new MSSQLFactory().GetObjectList<SelectListItem>(new InternalExamSQLQueries().GetSubjectList(exam_Id));
        }

        public int Save(InternalQuestions model)
        {
            return new MSSQLFactory().InsertRecord(model);
        }

        public int Save(InternalQuestionOptions opt)
        {
            return new MSSQLFactory().InsertRecord(opt);
        }

        public InternalExam GetInternalExam(Guid exam_Id)
        {
            return (new MSSQLFactory().GetObject<InternalExam>(new InternalExamSQLQueries().GetInternalExam(exam_Id)));
        }

        public bool GetExam(Guid subject_Id, int batch)
        {
            return (new MSSQLFactory().GetObjectList<InternalExam>(new InternalExamSQLQueries().GetExam(subject_Id, batch)) ?? new List<InternalExam>()).Count() > 0;
        }

        public InternalQuestions GetQuestion(Guid question_Id)
        {
            return new MSSQLFactory().GetObject<InternalQuestions>(new InternalExamSQLQueries().GetQuestion(question_Id));
        }

        public int SaveExam(InternalExamAnswers response)
        {

            return new MSSQLFactory().InsertRecord(response, "InternalExamAnswers");
        }

        public SqlCommand Edit(InternalQuestions model)
        {
            return new MSSQLFactory().UpdateRecord(model, null, null);
        }
        public SqlCommand Edit(InternalExam model)
        {
            return new MSSQLFactory().UpdateRecord(model, null, null);
        }
        public int Delete(Guid Option_Id)
        {
            string query = $@"DELETE FROM InternalQuestionOptions WHERE Option_Id = '{Option_Id}'";
            return new MSSQLFactory().ExecuteNonQuery(query);
        }

        public bool CheckExam(Guid subject_Id, Guid student_ID)
        {
            return (new MSSQLFactory().GetObjectList<InternalExamAnswers>(new InternalExamSQLQueries().CheckExam(subject_Id, student_ID)) ?? new List<InternalExamAnswers>()).Count() > 0;
        }
    }
}
