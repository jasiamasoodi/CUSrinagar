using GeneralModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CUSrinagar.Models;
using CUSrinagar.BusinessManagers;
using CUSrinagar.Extensions;
using CUSrinagar.Enums;

namespace CUSrinagar.WebApp.Controllers
{
    public class CourseController : Controller
    {
        public ActionResult Index()
        {
            List<ADMCollegeMaster> listColleges = new List<ADMCollegeMaster>();
            listColleges = new CollegeManager().GetCollegeList() ?? new List<ADMCollegeMaster>();
            foreach (ADMCollegeMaster college in listColleges)
            {
                college.Courses = new CourseManager().GetAllCourseList(college.College_ID) ?? new List<ADMCourseMaster>();
            }
            return View(listColleges);
        }
        public ActionResult GetCombinations()
        {
            FillViewBags();
            return View();

        }
        public ActionResult _CombinationList(Parameters parameter)
        {
            if (parameter == null)
                return View();

            ADMCourseMaster _ADMCourseMaster = new ADMCourseMaster();
            _ADMCourseMaster = new CourseManager().GetItem(parameter);
            return PartialView(_ADMCourseMaster);

        }


        [HttpGet]
        public ActionResult CUSCourses()
        {
            List<ADMCourseMaster> _model = new CourseManager().GetOfferedCoursesForDisplay();
            return View(_model);
        }

        [HttpGet]
        public ActionResult UGCourses()
        {
            Parameters param = new Parameters();
            param = new Parameters()
            {
                Filters = new List<SearchFilter>() { new SearchFilter() { Column = "Programme", Value = ((short)Programme.UG).ToString() } , new SearchFilter() { Column = "PrintProgramme", Value = ((short)PrintProgramme.UG).ToString() } },
                PageInfo = new Paging() { DefaultOrderByColumn = "CollegeFullName,CourseFullName", PageNumber = -1, PageSize = -1 }
            };
            List<CourseList> list = new CourseManager().List(param);
            return View(list);
        }


        private void FillViewBags()
        {
            ViewBag.CollegeList = new CollegeManager().GetADMCollegeMasterList();
            ViewBag.CourseList = new List<SelectListItem>();
            ViewBag.SemesterList = Helper.GetSelectList<Enums.Semester>();

        }
        public PartialViewResult _GetChildDDL(Guid CollegeID)
        {
            ViewBag.CourseList = new CourseManager().GetCourseList(CollegeID);
            return PartialView();
        }


        public ActionResult CourseStructure()
        {
            return View();
        }

        [HttpGet]
        public ActionResult FeeStructure()
        {
            return View();
        }

        [HttpGet]
        public ActionResult UniversitySubjects()
        {
            return View();
        }

    }
}