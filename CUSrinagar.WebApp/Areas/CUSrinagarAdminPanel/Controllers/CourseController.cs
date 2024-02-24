
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
    [OAuthorize(AppRoles.University)]
    public class CourseController : Controller
    {
        public ActionResult Courses(string id = null, string id1 = null)
        {
            ViewBag.Programme_Id = id ?? "";
            ViewBag.Course_Id = id1 ?? "";
            ADMCourseMaster aDMCourseMaster = null;
            if (id != null && id1 != null)
            {
                aDMCourseMaster = new ADMCourseMaster { Programme = (Programme)Convert.ToInt32(id) };
            }
            SetViewBags(aDMCourseMaster);
            return View();
        }
        public ActionResult CourseList(Parameters parameter)
        {
            //if (parameter.SortInfo == null || parameter.SortInfo.ColumnName == null)
            //    parameter = new CourseManager().GetDefaultParameter(parameter);
            List<ADMCourseMaster> listCourses = new CourseManager().GetAllCourseListOfAllColleges(parameter);
            return View(listCourses);
        }


        public ActionResult Create()
        {
            ADMCourseMaster aDMCourseMaster = new ADMCourseMaster();
            SetViewBags(aDMCourseMaster);
            return View(aDMCourseMaster);
        }





        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ADMCourseMaster input)
        {
            try
            {

                if (ModelState.IsValid)
                {

                    new CourseManager().AddCourse(input);
                    return RedirectToAction("Courses", new { id = Convert.ToInt32(input.Programme), id1 = input.Course_ID });
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("ErrorMessage", ex.Message);
            }
            SetViewBags();

            return View(input);
        }


        public ActionResult Edit(Guid id)
        {
            ADMCourseMaster model = new CourseManager().GetCourseById(id);
            SetViewBags();
            return View(model);
        }

        public PartialViewResult CourseMapping()
        {
            return PartialView();
        }

        [HttpPost]
        public ActionResult Edit(ADMCourseMaster input)
        {
            try
            {


                if (ModelState.IsValid)
                {

                    new CourseManager().EditCourse(input);
                    return RedirectToAction("Courses", new { id = Convert.ToInt32(input.Programme), id1 = input.Course_ID });
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("ErrorMessage", ex.Message);
            }
            SetViewBags();
            return View(input);
        }
        public string ChangeStatus(Guid id)
        {
            string msg = string.Empty;
            msg = new SubjectsManager().ChangeStatus(id);
            return msg;
        }
        public void SetViewBags(ADMCourseMaster _ADMCourseMaster = null)
        {
            _ADMCourseMaster = _ADMCourseMaster == null ? new ADMCourseMaster() : _ADMCourseMaster;
            _ADMCourseMaster.CourseMappingList = new List<ADMCollegeCourseMapping>() {new ADMCollegeCourseMapping() };
            IEnumerable<SelectListItem> ProgrammeDDL = new List<SelectListItem>();
            ProgrammeDDL = Helper.GetSelectList<Programme>();
            IEnumerable<SelectListItem> PrintProgrammeDDL = new List<SelectListItem>();
            PrintProgrammeDDL = Helper.GetSelectList<PrintProgramme>();
            IEnumerable<SelectListItem> CourseCategoryDDL = new List<SelectListItem>();
            CourseCategoryDDL = Helper.GetSelectList<ExaminationCourseCategory>();
            IEnumerable<SelectListItem> CollegeDDL = new List<SelectListItem>();
            CollegeDDL = new CollegeManager().GetADMCollegeMasterList();
            IEnumerable<SelectListItem> SchemeDDL = new List<SelectListItem>();
            SchemeDDL = Helper.GetSelectList<Scheme>();
            ViewBag.CollegeDDL = CollegeDDL;
            ViewBag.ProgrammeDDLList = ProgrammeDDL == null ? new List<SelectListItem>() : ProgrammeDDL;
            ViewBag.PrintProgrammeDDLList = PrintProgrammeDDL == null ? new List<SelectListItem>() : PrintProgrammeDDL;
            ViewBag.CourseCategoryDDLList = CourseCategoryDDL == null ? new List<SelectListItem>() : CourseCategoryDDL;
            ViewBag.SchemeDDLList = SchemeDDL == null ? new List<SelectListItem>() : SchemeDDL;

        }



    }
}