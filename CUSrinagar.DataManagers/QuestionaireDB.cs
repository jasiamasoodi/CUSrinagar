using CUSrinagar.DataManagers.SQLQueries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Terex;
using CUSrinagar.Models;
using System.Data.SqlClient;
using CUSrinagar.Enums;
using GeneralModels;

namespace CUSrinagar.DataManagers
{
    public class QuestionaireDB
    {

        public List<Questionaire> GetQuestionaire(Parameters parameter)
        {

            return new MSSQLFactory().GetObjectList<Questionaire>(new QuestionaireSQLQueries().GetQuestionaire(parameter));
        }

        public bool HaveSubmitted(Semester semester, ARGPersonalInformation student)
        {
            return new MSSQLFactory().GetObjectList<QuestionResponse>(new QuestionaireSQLQueries().CheckQuestionaire(semester, student)) != null;
        }

        public List<QuestionOptions> GetQuestionOptions(Guid Question_Id)
        {
            return new MSSQLFactory().GetObjectList<QuestionOptions>(new QuestionaireSQLQueries().GetQuestionOptions(Question_Id));
        }
        public int SaveQuestionaire(QuestionResponse response)
        {
            return new MSSQLFactory().InsertRecord<QuestionResponse>(response, "QuestionResponse");
        }

        public Questionaire GetQuestion(Guid Question_Id)
        {
            return new MSSQLFactory().GetObject<Questionaire>(new QuestionaireSQLQueries().GetQuestion(Question_Id));
        }

        public int AddQuestion(Questionaire input)
        {

            return new MSSQLFactory().InsertRecord<Questionaire>(input);
        }


        public int EditQuestion(Questionaire input)
        {
            SqlCommand sqlCommand = new MSSQLFactory().UpdateRecord<Questionaire>(input, null, null);
            sqlCommand.CommandText += " WHERE Question_ID=@Question_ID";
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }

        public List<QuestionOptions> GetQuestionOptionsCount(Parameters parameter,Guid Question_Id)
        {
            return new MSSQLFactory().GetObjectList<QuestionOptions>(new QuestionaireSQLQueries().GetQuestionOptionsCount(parameter,Question_Id));
        }

        public int AddQuestionOptions(QuestionOptions input)
        {
            return new MSSQLFactory().InsertRecord<QuestionOptions>(input);
        }

        public int EditQuestionOptions(QuestionOptions input)
        {
            SqlCommand sqlCommand = new MSSQLFactory().UpdateRecord<QuestionOptions>(input, null, null);
            sqlCommand.CommandText += " WHERE Option_Id=@Option_Id";
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }

    }
}
