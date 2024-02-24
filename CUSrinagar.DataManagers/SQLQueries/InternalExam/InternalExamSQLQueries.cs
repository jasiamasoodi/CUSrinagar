using CUSrinagar.Enums;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CUSrinagar.Models;
using CUSrinagar.Extensions;
using GeneralModels;

namespace CUSrinagar.DataManagers.SQLQueries
{
    public class InternalExamSQLQueries
    {

        internal SqlCommand GetInternalExamList(Parameters parameters)
        {
            SqlCommand command = new SqlCommand();
            //  command.Parameters.AddWithValue("@Student_ID", ID);
            command.CommandText = $@"SELECT * from InternalExam";
            return command;
        }

        internal SqlCommand GetQuestionList(Parameters parameter)
        {

            string query = $@"SELECT InternalQuestions.*
                                      FROM [InternalQuestions] join InternalExam on InternalQuestions.Exam_Id=InternalExam.Exam_Id";
            SqlCommand command = new SqlCommand();
            FilterHelper helper = new GeneralFunctions().GetWhereClause<InternalExam>(parameter.Filters);
            query += helper.WhereClause;
            query += new GeneralFunctions().GetPagedQuery(query, parameter);
            helper.Command.CommandText = query;
            return helper.Command;
        }

        internal SqlCommand GetMySubjects(Guid Student_ID, int Semester)
        {
            string query = $@"SELECT Subject_ID AS [Value],SubjectFullName AS [Text] FROM VWStudentWithDetail WHERE Student_ID=@Student_ID AND Semester=@Semester";
            SqlCommand command = new SqlCommand();
            command.Parameters.AddWithValue("@Student_ID", Student_ID);
            command.Parameters.AddWithValue("@Semester", Semester);
            return command;
        }

        internal SqlCommand GetOptionList(Guid Question_Id)
        {

            string query = $@"SELECT InternalQuestionOptions.*
                                      FROM  dbo.InternalQuestionOptions
                                     Where InternalQuestionOptions.Question_Id = @Question_Id";
            SqlCommand command = new SqlCommand();
            command.Parameters.AddWithValue("@Question_Id", Question_Id);
            command.CommandText = query;
            return command;
        }

        internal SqlCommand GetSubjectList(Guid exam_Id)
        {
            SqlCommand command = new SqlCommand();
            command.CommandText = $@"SELECT InternalQuestionOptions.*,InternalQuestions.*,ADMSubjectMaster.Subject_ID,SubjectFullName
                                      FROM [InternalQuestions]
                                      JOIN dbo.InternalQuestionOptions
                                      ON InternalQuestionOptions.Question_Id = InternalQuestions.Question_Id
                                    join InternalExam
                                    on InternalExam.Exam_Id=InternalQuestions.Exam_Id
                                      JOIN dbo.ADMSubjectMaster
                                      ON ADMSubjectMaster.Subject_ID = InternalExam.Subject_ID
                                      WHERE  InternalQuestions.Exam_Id=@Exam_Id Order By SubjectFullName";
            command.Parameters.AddWithValue("@Exam_Id", exam_Id);
            return command;
        }

        internal SqlCommand GetInternalExam(Guid exam_Id)
        {
            SqlCommand command = new SqlCommand();
            command.CommandText = $@"SELECT InternalExam.*
                                      FROM InternalExam  WHERE Exam_Id=@Exam_Id";
            command.Parameters.AddWithValue("@Exam_Id", exam_Id);
            return command;
        }

        internal SqlCommand GetExam(Guid subject_Id, int batch)
        {
            SqlCommand command = new SqlCommand();
            command.CommandText = $@"SELECT InternalExam.*,InternalQuestions.*
                                      FROM InternalExam
                                      JOIN dbo.InternalQuestions
                                      ON InternalExam.Exam_Id = InternalQuestions.Exam_Id And Batch=@Batch And InternalExam.Subject_ID=@Subject_ID AND convert(varchar, ExamDate, 110)= convert(varchar, getdate(), 110)";
            command.Parameters.AddWithValue("@Subject_Id", subject_Id);
            command.Parameters.AddWithValue("@Batch", batch);
            return command;
        }
        internal SqlCommand CheckExam(Guid subject_Id, Guid student_id)
        {
            SqlCommand command = new SqlCommand();
            command.CommandText = $@" SELECT  Student_Id FROM dbo.InternalExamAnswers
                                    JOIN dbo.InternalQuestions ON InternalQuestions.Question_Id = InternalExamAnswers.Question_Id
                                    JOIN dbo.InternalExam ON InternalExam.Exam_Id = InternalQuestions.Exam_Id
                                    WHERE Student_Id = @Student_Id AND Subject_ID =@Subject_Id";
            command.Parameters.AddWithValue("@Subject_Id", subject_Id);
            command.Parameters.AddWithValue("@Student_Id", student_id);
            return command;
        }
       
        internal SqlCommand GetQuestion(Guid question_Id)
        {
            string query = $@"SELECT InternalQuestions.*
                                      FROM [InternalQuestions]
                              WHERE Question_Id = @Question_Id";

            SqlCommand command = new SqlCommand();
            command.Parameters.AddWithValue("@Question_Id", question_Id);
            command.CommandText = query;
            return command;
        }
    }
}