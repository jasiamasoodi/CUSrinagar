using CaptchaMvc.HtmlHelpers;
using CUSrinagar.BusinessManagers;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using CUSrinagar.Models;
using CUSrinagar.OAuth;
using GeneralModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CUSrinagar.WebApp.CUSrinagarAdminPanel.Controllers
{
    [OAuthorize(AppRoles.University, AppRoles.University_OfficeAssistant)]
    public class InternalAdminController : AdminController
    {
        // GET: CUSrinagarAdminPanel/Default
        public ActionResult InternalExam()
        {
            FillViewBag(Programme.UG, Guid.Empty, 0);
            return View();
        }
        public PartialViewResult InternalExamList(Parameters parameter)
        {
            List<InternalExam> listInternalExam = new InternalExamManager().GetInternalExamList(parameter);
            return PartialView(listInternalExam);
        }
        public ActionResult Create()
        {
            FillViewBag(Programme.UG,Guid.Empty,0);
            return View();
        }
        public void FillViewBag(Programme prog,Guid Course_Id,int semester)
        {
            FillViewBag_Programmes();
            FillViewBag_Course(prog);
            FillViewBag_College();
            FillViewBag_Subjects(Course_Id,(Semester)semester);
            FillViewBag_Semesters();
        }
        [HttpPost]
        public ActionResult Create(InternalExam internalExam)
        {
            internalExam.Exam_Id = Guid.NewGuid();
            if (new InternalExamManager().Save(internalExam) > 0)
                return RedirectToAction("AddQuestions", new { Exam_Id = internalExam.Exam_Id });
            else
                return View();
        }
        public ActionResult Edit(Guid Exam_Id)
        {
           InternalExam internalExam = new InternalExamManager().GetInternalExam(Exam_Id);
            Guid Course_Id =new SubjectsManager().GetSubjectsCourse(internalExam.Subject_ID);
            Programme programme = new CourseManager().GetCourseProgramme(Course_Id);
            FillViewBag(programme,Course_Id, internalExam.Semester);
            internalExam.Course_Id = Course_Id;
            return View(internalExam);
        }
        [HttpPost]
        public ActionResult Edit(InternalExam internalExam)
        {
            if (new InternalExamManager().Edit(internalExam) > 0)
                return RedirectToAction("InternalExam");
            else
                return View();
        }
        public ActionResult ViewQuestions(Guid Exam_Id)
        {
            InternalExam internalExam = new InternalExamManager().GetInternalExam(Exam_Id);
            Guid Course_Id = new SubjectsManager().GetSubjectsCourse(internalExam.Subject_ID);
            Programme programme = new CourseManager().GetCourseProgramme(Course_Id);
            FillViewBag(programme,Course_Id,(int)internalExam.Semester);
            ViewBag.Semester = internalExam.Semester;
            ViewBag.Course = Course_Id;
            ViewBag.programme = programme;
            ViewBag.Subject = internalExam.Subject_ID;
            ViewBag.Exam_Id = Exam_Id;
            return View();
        }
        public PartialViewResult QuestionsList(Parameters parameter)
        {
            List<InternalQuestions> listQuestions = new InternalExamManager().GetQuestionList(parameter);
            foreach (InternalQuestions question in listQuestions ?? new List<InternalQuestions>())
            {
                question.InternalQuestionOptions = new InternalExamManager().GetOptionList(question.Question_Id);
            }
            return PartialView(listQuestions);
        }
        public ActionResult AddQuestions(Guid Exam_Id)
        {
            InternalQuestions questions = new InternalQuestions() { Exam_Id = Exam_Id };
            InternalExam internalExam = new InternalExamManager().GetInternalExam(Exam_Id) ?? new InternalExam();
            ViewBag.Subject = new SubjectsManager().Get(internalExam.Subject_ID).SubjectFullName;
            ViewBag.College = new CollegeManager().GetItem(internalExam.College_Id).CollegeFullName;
            ViewBag.Semester = internalExam.Semester;
            ViewBag.Message = "";
            return View(questions);
        }
        [HttpPost]
        public ActionResult AddQuestions(InternalQuestions model, List<InternalQuestionOptions> option)
        {
            //  string msg = "";
            string Status = "";
            model.Question_Id = Guid.NewGuid();
            if (new InternalExamManager().Save(model) > 0)
            {
                option = option.Where(x => !string.IsNullOrEmpty(x.OptionName)).ToList();
                foreach (var opt in option)
                {
                    opt.Question_Id = model.Question_Id;
                    opt.Option_Id = Guid.NewGuid();
                    new InternalExamManager().Save(opt);
                }
                Status = "Saved";
            }
            else
            {
                Status = "NotSaved";
            }

            //ViewBag.Message = msg;
            //model.InternalQuestionOptions = option;
            return RedirectToAction("EditQuestion", new { model.Question_Id, Status });

        }
        public ActionResult EditQuestion(Guid Question_Id, string Status = "")
        {
            InternalQuestions Question = new InternalExamManager().GetQuestion(Question_Id);
            InternalQuestions questions = new InternalQuestions() { Exam_Id = Question.Exam_Id };
            InternalExam internalExam = new InternalExamManager().GetInternalExam(Question.Exam_Id) ?? new InternalExam();
            ViewBag.Subject = new SubjectsManager().Get(internalExam.Subject_ID).SubjectFullName;
            ViewBag.College = new CollegeManager().GetItem(internalExam.College_Id).CollegeFullName;
            ViewBag.Semester = internalExam.Semester;
            Question.InternalQuestionOptions = new InternalExamManager().GetOptionList(Question.Question_Id);
            string msg = "";
            if (Status == "Saved")
            {
                msg = "<div class='alert alert-success col-sm-10'>Saved successfully</div>";
            }
            else if (Status == "NotSaved")
            {
                msg = "<div class='alert alert-danger col-sm-10'>Not Saved Successfully</div>";
            }
            ViewBag.Message = msg;
            return View(Question);
        }
        [HttpPost]
        public ActionResult EditQuestion(InternalQuestions model, List<InternalQuestionOptions> option)
        {
            string Status = "";
            var optionadd = option.Where(x => !string.IsNullOrEmpty(x.OptionName)).ToList();

            if (new InternalExamManager().Edit(model) > 0)
            {
                foreach (var opt in option.Where(x => x.Option_Id != null))
                {
                    new InternalExamManager().Delete(opt.Option_Id);
                }
                foreach (var opt in optionadd)
                {
                    opt.Question_Id = model.Question_Id;
                    opt.Option_Id = Guid.NewGuid();
                    new InternalExamManager().Save(opt);
                }
                Status = "Saved";
            }
            else
            { Status = "NotSaved"; }
            //ViewBag.Message = msg;
            //model.InternalQuestionOptions = optionadd;
            return RedirectToAction("EditQuestion", new { model.Question_Id, Status });
        }
    }
}
