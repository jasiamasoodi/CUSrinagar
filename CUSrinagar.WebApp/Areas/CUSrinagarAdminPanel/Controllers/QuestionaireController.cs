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
    [OAuthorize(AppRoles.University,AppRoles.University_Dean)]
    public class QuestionaireController : Controller
    {
        // GET: CUSrinagarAdminPanel/Questionaire

        public ActionResult QuestionaireList()
        {
            return View();
        }
        public ActionResult QuestionaireListTable(Parameters parameter)
        {
            List<Questionaire> listQuestionaires = new QuestionaireManager().GetAllQuestionaires(parameter);
            return View(listQuestionaires);
        }


        public ActionResult Create()
        {
            return View(new Questionaire());
        }
        [HttpPost]
        public ActionResult Create(Questionaire questionaires)
        {
            //  if (ModelState.IsValid)
            { new QuestionaireManager().AddQuestion(questionaires); }
            return RedirectToAction("QuestionaireList");
            // return View(questionaires);
        }
        public ActionResult Edit(Guid _ID)
        {
            Questionaire questionaires = new QuestionaireManager().GetQuestion(_ID);
            return View(questionaires);
        }
        [HttpPost]
        public ActionResult Edit(Questionaire questionaires)
        {
            //  if (ModelState.IsValid)
            { new QuestionaireManager().EditQuestion(questionaires); }
            return RedirectToAction("QuestionaireList");
            //return View(questionaires);
        }
        public ActionResult _OptionsView(int Count)
        {
            ViewData["Count"] = Count;
            return PartialView(new Questionaire());
        }
        public ActionResult QuestionaireCountList()
        {
            SetViewBags();
            return View();
        }

        private void SetViewBags()
        {
            ViewBag.Colleges = new CollegeManager().GetADMCollegeMasterList();
            ViewBag.PrintProgramme = Helper.GetSelectList<PrintProgramme>();
            ViewBag.Courses = new CourseManager().GetAllCoursesByPrintProgramme(PrintProgramme.UG);
            ViewBag.Semesters = Helper.GetSelectList<Semester>();
        }

        public ActionResult QuestionaireCountListTable(Parameters parameter)
        {
            List<Questionaire> listQuestionaires = new QuestionaireManager().GetAllQuestionairesCount(parameter);
            return View(listQuestionaires);
        }
        public PartialViewResult _GetChildDDL(Guid College_ID, PrintProgramme PrintProgramme)
        {
            ViewBag.Type = "Course_ID";
            ViewBag.ChildType = "";
            ViewBag.ChildSubType = "";
            ViewBag.ChildValues = new List<SelectListItem>();
            ViewBag.ChildValues = new CourseManager().GetCourseList(College_ID, PrintProgramme);
            return PartialView();
        }


    }
}