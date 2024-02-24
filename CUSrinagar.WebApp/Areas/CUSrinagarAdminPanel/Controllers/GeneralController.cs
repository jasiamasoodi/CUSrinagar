using CUSrinagar.BusinessManagers;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using CUSrinagar.Models;
using GeneralModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CUSrinagar.WebApp.CUSrinagarAdminPanel.Controllers
{
    public class GeneralController : AdminController
    {
        public JsonResult GetCourse(Guid Course_ID)
        {
            var list = new CourseManager().GetCompactItem(Course_ID) ?? new ADMCourseMaster();
            return Json(list);
        }
        //public JsonResult DepartmentDataList(string SchoolFullName = null)
        //{
        //    var list = new SubjectsManager().DepartmentDataList(SchoolFullName) ?? new List<SelectListItem>();
        //    return Json(list);
        //}
        public JsonResult GetSchools(Guid? College_ID)
        {
            var list = new SubjectsManager().SchoolSelectList(College_ID) ?? new List<SelectListItem>();
            return Json(list);
        }
        public JsonResult GetCourseList(Guid? College_ID, PrintProgramme? printProgramme, Programme? programme)
        {
            List<SelectListItem> list = new CourseManager().GetCourseList(College_ID, printProgramme, programme) ?? new List<SelectListItem>();
            return Json(list);
        }

        public JsonResult GetCollegeList(Guid? Course_ID, PrintProgramme? printProgramme, Programme? programme)
        {
            List<SelectListItem> list = new CourseManager().GetCollegeList(Course_ID, printProgramme, programme) ?? new List<SelectListItem>();
            return Json(list);
        }

        public JsonResult SelectListAppUser(Parameters parameter)
        {
            var list = new UserProfileManager().SelectListAppUser(parameter) ?? new List<SelectListItem>();
            return Json(list);
        }

        public JsonResult SelectListProgrammesByCollege(Guid? College_ID)
        {
            List<SelectListItem> list = new CourseManager().GetProgrammes(College_ID);
            return Json(list);
        }

        public JsonResult CollegePrintProgramCourses(Guid? College_ID, PrintProgramme? printProgramme)
        {
            List<SelectListItem> list = null;
            if (College_ID.HasValue && printProgramme.HasValue)
            {
                list = new CourseManager().GetCourseList(College_ID.Value, printProgramme.Value);
            }
            else if (College_ID.HasValue)
            {
                list = new CourseManager().GetCourseList(College_ID.Value);
            }
            else if (printProgramme.HasValue)
            {
                list = new CourseManager().GetAllCoursesByPrintProgramme(printProgramme.Value);
            }
            return Json(list);
        }

        public JsonResult CoursesByProgramme(Guid? College_ID, Programme? programme)
        {
            List<SelectListItem> list = null;
            if (College_ID.HasValue && programme.HasValue)
            {
                //list = new CourseManager().GetCourseList(College_ID.Value, programme.Value);
            }
            else if (College_ID.HasValue)
            {
                list = new CourseManager().GetCourseList(College_ID.Value);
            }
            else if (programme.HasValue)
            {
                list = new CourseManager().GetAllCoursesByProgramme((int)programme.Value);
            }
            return Json(list ?? new List<SelectListItem>());
        }

        public JsonResult CourseSubjects(Guid? Course_ID)
        {
            List<SelectListItem> list = null;
            if (Course_ID.HasValue)
            {
                list = new SubjectsManager().GetAllSubjects(Course_ID.Value);
            }
            return Json(list);
        }
        public JsonResult SubjectDDL(Parameters parameters)
        {
            List<SelectListItem> list = null;
            list = new SubjectsManager().SubjectDDL(parameters) ?? new List<SelectListItem>();
            return Json(list);
        }

        public JsonResult SubjectListCompact(Parameters parameters)
        {
            List<SubjectCompact> list = new SubjectsManager().SubjectListCompact(parameters) ?? new List<SubjectCompact>();
            return Json(list);
        }

        public JsonResult SubjectDDLWithDetail(Parameters parameters)
        {
            List<SelectListItem> list = new SubjectsManager().SubjectDDLWithDetail(parameters) ?? new List<SelectListItem>();
            return Json(list);
        }
        public JsonResult BillDDL(Parameters parameters)
        {
            IEnumerable<SelectListItem> list = new BillingManager().BillNoDDL(parameters) ?? new List<SelectListItem>();
            return Json(list);
        }

        public JsonResult ResultNotificationDDL(Parameters parameters)
        {
            List<SelectListItem> list = new ResultNotificationManager().DDL(parameters) ?? new List<SelectListItem>();
            return Json(list);
        }

        public JsonResult ProfessorDDL(Parameters parameters)
        {
            List<SelectListItem> list = new UserProfileManager().GetUserList(AppRoles.College_AssistantProfessor, parameters) ?? new List<SelectListItem>();
            return Json(list);
        }
        public JsonResult CRDDL(Parameters parameters)
        {
            List<SelectListItem> list = new UserProfileManager().GetUserList(AppRoles.College_ClassRepresentative, parameters) ?? new List<SelectListItem>();
            return Json(list);
        }
        public JsonResult VWSCSubjectDDLWithDetail(Parameters parameters)
        {
            List<SelectListItem> list = new SubjectsManager().VWSCSubjectDDLWithDetail(parameters) ?? new List<SelectListItem>();
            return Json(list);
        }

        //public JsonResult CombinationDDL(Guid? College_ID, Guid? Course_ID, short? semester)
        //{
        //    List<SelectListItem> list = new List<SelectListItem>();
        //    if (College_ID.HasValue && Course_ID.HasValue & semester.HasValue)
        //        list = new CombinationManager().GetCombinationsDDL(College_ID.Value, Course_ID.Value, semester.Value);
        //    return Json(list);
        //}

        public JsonResult CombinationDDL(Parameters Parameter)
        {
            List<SelectListItem> list = new CombinationManager().GetCombinationsDDL(Parameter) ?? new List<SelectListItem>();
            return Json(list);
        }


        public JsonResult DepartmentDDL(string SchoolFullName = null)
        {
            List<SelectListItem> list = new SubjectsManager().DepartmentDataList(SchoolFullName) ?? new List<SelectListItem>();
            return Json(list);
        }



        //public JsonResult OldCollgeCourses(string CollegeCode, string Category)
        //{
        //    List<SelectListItem> list = null;
        //    if (!string.IsNullOrEmpty(CollegeCode) && !string.IsNullOrEmpty(Category))
        //    {
        //        list = new CourseManager().OldCourseList( CollegeCode, Category);
        //    }
        //    return Json(list);
        //}
        //public JsonResult OldCollegeCourseSubjects(string CollegeCode, string Category,string CourseCode)
        //{
        //    List<SelectListItem> list = null;
        //    if (!string.IsNullOrEmpty(CollegeCode) && !string.IsNullOrEmpty(CollegeCode) && !string.IsNullOrEmpty(CourseCode))
        //    {
        //        list = new CourseManager().OldCollegeCourseSubjectList( CollegeCode, Category, CourseCode);
        //    }
        //    return Json(list);
        //}
    }
}