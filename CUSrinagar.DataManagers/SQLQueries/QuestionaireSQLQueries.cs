using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CUSrinagar.Enums;
using CUSrinagar.Models;
using GeneralModels;

namespace CUSrinagar.DataManagers.SQLQueries
{
    public class QuestionaireSQLQueries
    {
        internal SqlCommand GetQuestionaire(Parameters parameter)
        {
            string query = @"SELECT * FROM [Questionaire]";
            SqlCommand command = new SqlCommand();
            FilterHelper helper = new GeneralFunctions().GetWhereClause<Questionaire>(parameter?.Filters);
            if (parameter != null)
            {
                query += helper.WhereClause;
                query += new GeneralFunctions().GetPagedQuery(query, parameter);
            }
            helper.Command.CommandText = query;
            return helper.Command;

        }
        internal SqlCommand GetQuestionOptions(Guid Question_Id)
        {
            SqlCommand command = new SqlCommand();
            command.CommandText = @"SELECT * FROM [QuestionOptions] where Question_Id=@Question_Id";
            command.Parameters.AddWithValue("@Question_Id", Question_Id);
            return command;
        }

        internal SqlCommand CheckQuestionaire(Semester semester, ARGPersonalInformation student)
        {
            SqlCommand command = new SqlCommand();
            command.CommandText = $@"SELECT * FROM [QuestionResponse] where Student_Id=@Student_Id and Semester=@Semester AND Year={DateTime.Now.Year}";
            command.Parameters.AddWithValue("@Student_Id", student.Student_ID);
            command.Parameters.AddWithValue("@Semester", (int)semester);
            return command;
        }

        internal SqlCommand GetQuestion(Guid Question_Id)
        {
            SqlCommand command = new SqlCommand();
            command.CommandText = @"SELECT * FROM [Questionaire] where Question_Id=@Question_Id";
            command.Parameters.AddWithValue("@Question_Id", Question_Id);
            return command;
        }

        internal SqlCommand GetQuestionOptionsCount(Parameters parameter, Guid Question_Id)
        {
            string query = @"SELECT OptionLabel, COUNT(QuestionResponse.Student_Id) StudentCount
                                        FROM QuestionResponse
                                        JOIN QuestionOptions
                                        ON QuestionOptions.Option_Id = QuestionResponse.Option_Id
                                        JOIN Questionaire ON Questionaire.Question_Id = QuestionResponse.Question_Id
                                        JOIN VWStudentCourse ON VWStudentCourse.Student_ID = QuestionResponse.Student_Id";
            SqlCommand command = new SqlCommand();
            FilterHelper helper = new GeneralFunctions().GetWhereClause<ARGPersonalInformation>(parameter?.Filters);

            query += helper.WhereClause;
            query += @" AND Questionaire.Question_Id =@Question_Id GROUP BY OptionLabel
                                        ORDER BY COUNT(QuestionResponse.Student_Id) DESC";
            helper.Command.Parameters.AddWithValue("@Question_Id", Question_Id);
            helper.Command.CommandText = query;
            return helper.Command;
        }


    }
}
