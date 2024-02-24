using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using CUSrinagar.Models;
using System.Web.Compilation;
using System.IO;
using CUSrinagar.BusinessManagers;
using GeneralModels;
using CUSrinagar.Enums;
using CUSrinagar.OAuth;
using CUSrinagar.Extensions;

namespace CUSrinagar.WebApp.CUSrinagarAdminPanel.Controllers
{
    [OAuthorize(AppRoles.University)]
    public class CollegeController : Controller
    {
        // GET: CUSrinagarAdminPanel/College
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult CollegeList()
        {
            return View();
        }

        public ActionResult CollegeListTable(Parameters parameter)
        {
            List<ADMCollegeMaster> collegeList = new CollegeManager().GetCollegeList(parameter)?.Where(c => c.Status).ToList();
            return View(collegeList);
        }

        [HttpGet]
        public ActionResult Create()
        {
            SetViewBags();
            return View();
        }

        [NonAction]
        private void SetViewBags(AppUsers appUsers = null)
        {
            //ViewBag.RolesList = Helper.GetSelectForAdmission(new List<AppRoles> { AppRoles.College_AssistantProfessor }) ?? new List<SelectListItem>();
            //ViewBag.LoggedCollege_ID = AppUserHelper.College_ID;
            //ViewBag.CourseDDLList = new CourseManager().GetCourseList(ViewBag.LoggedCollege_ID) ?? new List<SelectListItem>();
            //ViewBag.SubjectDDLList0 = ViewBag.SemesterDDLList0 = new List<SelectListItem>();

            //if (appUsers != null)
            //{
            //    int track = 0;
            //    if (appUsers.ProfessorSubjects != null)
            //    {
            //        appUsers.ProfessorSubjects = appUsers.ProfessorSubjects.Where(x => x.Course_ID != null && x.Course_ID != Guid.Empty).ToList();
            //        foreach (var obj in appUsers.ProfessorSubjects)
            //        {
            //            ViewData["SubjectDDLList" + track] = new UserProfileManager().FetchChildDDlValues(obj.Course_ID.ToString(), "Subject") ?? new List<SelectListItem>();
            //            ViewData["SemesterDDLList" + track] = new UserProfileManager().FetchChildDDlValues(obj.Course_ID.ToString(), "Semester") ?? new List<SelectListItem>();
            //            track++;
            //        }
            //    }

            //}
        }

    }
}