using CUSrinagar.BusinessManagers;
using CUSrinagar.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using GeneralModels;
using Filter = CUSrinagar.Models.Filter;

namespace CUSrinagar.WebApp.Controllers
{
    public class SyllabusController : WebAppBaseController
    {
        #region Syllabus
        public ActionResult Index(bool id = false, Programme pp = Programme.UG)
        {
            FillViewBag_SubjectTypesOrCourses(id, pp);
            ViewBag.isNEP = id;
            ViewBag.Programme = pp;
            FillViewBag_Course(Programme.UG);
            FillViewBag_Departments();
            FillViewBag_Semesters();
            ViewBag.Year = Helper.GetYearsDDL().OrderByDescending(x => x.Value);
            return View();
        }

        public ActionResult SyllabusListPartial(Parameters parameter)
        {
            bool IsNEP = parameter.Filters.Any(x => x.Column.ToLower().Trim() == "isnep" && x.Value.ToLower().Trim() == "true");
            try
            {
                SearchFilter searchToRemove = parameter.Filters.First(x => x.Column.ToLower().Trim() == "isnep");
                parameter.Filters.Remove(searchToRemove);
            }
            catch (Exception) { }

            if (parameter.Filters.Any(x => x.Column.ToLower().Trim() == "programme" && x.Value.ToLower().Trim() == "6"))
            {
                SearchFilter filter = new SearchFilter
                {
                    Column = "Programme",
                    Value = ((short)Programme.Engineering).ToString(),
                    GroupOperation = LogicalOperator.OR,
                    IsSibling = true,
                    TableAlias = "C"
                };

                int index = parameter.Filters.FindIndex(x => x.Column.ToLower().Trim() == "programme" && x.Value.ToLower().Trim() == "6");
                parameter.Filters.Insert(index + 1, filter);
            }

            List<Syllabus> listSyllabus = new SyllabusManager().List(parameter);

            if (listSyllabus.IsNotNullOrEmpty() && IsNEP)
            {
                listSyllabus.ForEach(x =>
                {
                    x.IsNEP = true;
                });
            }

            return View(listSyllabus);
        }

        #endregion

        #region  Commented


        //[HttpPost]
        //public JsonResult List(string sidx, string sord, bool _search, string filters, int page, int rows)
        //{
        //    SearchFilter defaultFilter = new SyllabusManager().GetDefaultSearchFilter();
        //    //if (filters == null)
        //    //filters = new List<SearchFilter>();
        //    //filters.Add(defaultFilter);
        //    return Json(new SyllabusManager().GetAllSyllabus(sidx, sord, _search, filters, page, rows), JsonRequestBehavior.DenyGet);
        //}
        //public void SetViewBags(Syllabus syllabus = null)
        //{
        //    IEnumerable<SelectListItem> ProgrammeDDL = new List<SelectListItem>();

        //    ProgrammeDDL = Helper.GetSelectList<Programme>();
        //    List<SelectListItem> CourseDDL = new List<SelectListItem>();
        //    List<SelectListItem> SemesterDDL = new List<SelectListItem>();
        //    List<SelectListItem> SubjectDDL = new List<SelectListItem>();
        //    if (syllabus != null)
        //    {
        //        int ProgrammeId = Convert.ToInt32(ProgrammeDDL.FirstOrDefault().Value);
        //        CourseDDL = new CourseManager().GetAllCoursesByProgramme(ProgrammeId,1);
        //        SemesterDDL = new SyllabusManager().GetAllSemesters(syllabus.Course_ID.Value);
        //        SubjectDDL = new SubjectsManager().GetAllSubjects(syllabus.Course_ID.Value);

        //    }
        //    ViewBag.ProgrammeDDLList = ProgrammeDDL;
        //    ViewBag.CourseDDLList = CourseDDL;
        //    FillViewBag_Semesters();
        //    ViewBag.SemesterDDLList = SemesterDDL;
        //    ViewBag.SubjectDDLList = SubjectDDL;


        //}

        /// <summary>
        /// Partial view for child dropdown
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// 
        //public PartialViewResult _GetChildDDL(string id, string Type, string childType, string childSubType, int Semester=0)

        //{
        //    ViewBag.Type = Type;
        //    ViewBag.ChildType = childType;
        //    ViewBag.ChildSubType = childSubType;
        //    ViewBag.ChildValues = new SyllabusManager().FetchChildDDlValues(id, Semester, Type,false);
        //    return PartialView();
        //}
        #endregion

        #region Skill Courses
        [HttpGet]
        public ActionResult SkillCourses(Parameters parameter)
        {
            List<Syllabus> listforms = new SyllabusManager().GetAllSkillCourses(parameter);
            return View(listforms);
            //return View();
        }


        [HttpGet]
        public ActionResult ListOfSkillCourses(Parameters parameter)
        {
            IEnumerable<SelectListItem> CollegeDDL = new List<SelectListItem>();
            CollegeDDL = new CollegeManager().GetADMCollegeMasterList();
            ViewBag.CollegeDDLList = CollegeDDL == null ? new List<SelectListItem>() : CollegeDDL;
            return View();
        }

        public ActionResult GetSkillSubjectsByCollege(Guid? Id)
        {
            List<ADMSubjectMaster> listSubjects = new List<ADMSubjectMaster>();
            if (Id != null)
            {
                listSubjects = new SubjectsManager().GetSkillSubjectsByCollege(Id.Value);
            }
            return View(listSubjects);
        }
        #endregion

    }
}
