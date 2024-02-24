using CUSrinagar.Enums;
using CUSrinagar.Models;
using System.Data.SqlClient;
using CUSrinagar.DataManagers;
using System.Web.Mvc;
using Terex;
using System.Transactions;
using System.Web.UI.WebControls;
using System.IO;
using System.Web;
using System.Collections.Generic;
using GeneralModels;
using System;

namespace CUSrinagar.BusinessManagers
{

    public class InternalExamManager
    {
        public List<InternalExam> GetInternalExamList(Parameters parameters)
        {
            return new InternalExamDB().GetInternalExamList(parameters);
        }

        public List<SelectListItem> GetSubjects(Guid Student_ID, int Semester)
        {
            return new InternalExamDB().GetSubjects(Student_ID, Semester);
        }
        public int Save(InternalExam internalExam)
        {
            return new InternalExamDB().Save(internalExam);

        }

        public List<InternalQuestions> GetQuestionList(Parameters parameters)
        {
            return new InternalExamDB().GetQuestionList(parameters);
        }
        public List<InternalQuestionOptions> GetOptionList(Guid Question_Id)
        {
            return new InternalExamDB().GetOptionList(Question_Id);
        }

        public List<SelectListItem> GetSubjectList(Guid exam_Id)
        {
            return new InternalExamDB().GetSubjectList(exam_Id);
        }

        public int Save(InternalQuestions model)
        {
            return new InternalExamDB().Save(model);
        }

        public int Save(InternalQuestionOptions opt)
        {
            return new InternalExamDB().Save(opt);
        }

        public InternalQuestions GetQuestion(Guid question_Id)
        {
            return new InternalExamDB().GetQuestion(question_Id);
        }

        public int Edit(InternalQuestions model)
        {
            SqlCommand sqlCommand = new InternalExamDB().Edit(model);
            sqlCommand.CommandText += " WHERE Question_Id=@Question_Id";
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }

        public int Edit(InternalExam model)
        {
            SqlCommand sqlCommand = new InternalExamDB().Edit(model);
            sqlCommand.CommandText += " WHERE Exam_Id=@Exam_Id";
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
    }

        public InternalExam GetInternalExam(Guid exam_Id)
        {
            return new InternalExamDB().GetInternalExam(exam_Id);
        }

        public bool GetExam(Guid Subject_Id, int Batch)
        {
            return new InternalExamDB().GetExam(Subject_Id, Batch);
        }

        public int Delete(Guid Option_Id)
        {
            return new InternalExamDB().Delete(Option_Id);
        }

        public bool SaveExam(List<InternalQuestions> collection)
        {
            foreach (InternalQuestions question in collection)
            {
                InternalExamAnswers response = CreateResponse(question);
                new InternalExamDB().SaveExam(response);
            }
            return true;
        }
        private InternalExamAnswers CreateResponse(InternalQuestions question)
        {
            ARGPersonalInformation student = new StudentDB().GetStudent(AppUserHelper.User_ID, AppUserHelper.OrgPrintProgramme);
            InternalExamAnswers response = new InternalExamAnswers()
            {
                InternalExamAnswer_Id = Guid.NewGuid(),
                Student_Id = AppUserHelper.User_ID,
                Question_Id = question.Question_Id,
                Option_Id = question.IsCheckedOption
            };
            return response;
        }

        public bool CheckExam(Guid subject_Id, Guid student_ID)
        {
            return new InternalExamDB().CheckExam(subject_Id, student_ID);
        }
    }
}
