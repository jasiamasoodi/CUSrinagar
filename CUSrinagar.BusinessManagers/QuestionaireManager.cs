using CUSrinagar.DataManagers;
using CUSrinagar.DataManagers.SQLQueries;
using CUSrinagar.Enums;
using CUSrinagar.Models;
using GeneralModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Terex;

namespace CUSrinagar.BusinessManagers
{
    public class QuestionaireManager
    {
        public List<Questionaire> GetQuestionaire(Semester semester)
        {

            ///check has questionaire
            ARGPersonalInformation student = new StudentDB().GetStudent(AppUserHelper.User_ID, AppUserHelper.OrgPrintProgramme);
            bool HaveSubmitted = new QuestionaireDB().HaveSubmitted(semester, student);
            if (HaveSubmitted)
            { return null; }
            else
            {
                List<Questionaire> list = new QuestionaireDB().GetQuestionaire(null);
                foreach (Questionaire question in list)
                {
                    question.QuestionOptions = GetQuestionOptions(question.Question_Id);
                }
                return list;
            }
        }

        public List<Questionaire> GetAllQuestionaires(Parameters parameter)
        {
            List<Questionaire> list = new QuestionaireDB().GetQuestionaire(parameter);
            return list;
        }
        public List<Questionaire> GetAllQuestionairesCount(Parameters parameter)
        {
            List<Questionaire> list = new QuestionaireDB().GetQuestionaire(null);
            foreach (Questionaire questionaire in list)
            {
                questionaire.QuestionOptions = new QuestionaireDB().GetQuestionOptionsCount(parameter, questionaire.Question_Id);
                questionaire.TotalStudents = questionaire?.QuestionOptions?.Sum(x => x.StudentCount)??0;
            }
            return list;
        }
        public int AddQuestion(Questionaire input)
        {
            int result = 0;
            input.Question_Id = Guid.NewGuid();
            result = new QuestionaireDB().AddQuestion(input);
            if (result > 0)
            {
                foreach (QuestionOptions questionOptions in input.QuestionOptions)
                {
                    questionOptions.Question_Id = input.Question_Id;
                    questionOptions.Option_Id = Guid.NewGuid();
                    new QuestionaireDB().AddQuestionOptions(questionOptions);
                }
            }
            return result;

        }
        public int EditQuestion(Questionaire input)
        {
            int result = 0;
            result = new QuestionaireDB().EditQuestion(input);
            if (result > 0)
            {
                foreach (QuestionOptions questionOptions in input.QuestionOptions)
                {
                    if (questionOptions.Option_Id == null || questionOptions.Option_Id == Guid.Empty)
                    {
                        questionOptions.Question_Id = input.Question_Id;
                        questionOptions.Option_Id = Guid.NewGuid();
                        new QuestionaireDB().AddQuestionOptions(questionOptions);
                    }
                    else { new QuestionaireDB().EditQuestionOptions(questionOptions); }
                }
            }
            return result;

        }
        public Questionaire GetQuestion(Guid Question_Id)
        {
            Questionaire questionaires = new QuestionaireDB().GetQuestion(Question_Id);
            questionaires.QuestionOptions = new QuestionaireDB().GetQuestionOptions(Question_Id);
            return questionaires;
        }

        public List<QuestionOptions> GetQuestionOptions(Guid Question_Id)
        {
            return new QuestionaireDB().GetQuestionOptions(Question_Id);
        }

        public bool SaveQuestionaire(List<Questionaire> questionaires)
        {
            foreach (Questionaire question in questionaires)
            {
                QuestionResponse response = CreateResponse(question);
                new QuestionaireDB().SaveQuestionaire(response);
            }
            return true;
        }

        private QuestionResponse CreateResponse(Questionaire question)
        {
            ARGPersonalInformation student = new StudentDB().GetStudent(AppUserHelper.User_ID, AppUserHelper.OrgPrintProgramme);
            QuestionResponse response = new QuestionResponse()
            {
                Response_Id = Guid.NewGuid(),
                Option_Id = question.IsCheckedOption,
                Semester = (int)question.Semester,
                Student_Id = student.Student_ID,
                Year = DateTime.Now.Year,
                Question_Id = question.Question_Id

            };
            return response;
        }
    }
}
