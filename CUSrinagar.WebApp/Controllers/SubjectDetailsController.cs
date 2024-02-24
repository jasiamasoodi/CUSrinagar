using GeneralModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CUSrinagar.Models;
using CUSrinagar.BusinessManagers;
using CUSrinagar.Extensions;

namespace CUSrinagar.WebApp.Controllers
{
    public class SubjectDetailsController : Controller
    {
        // GET: SubjectDetails
        public ActionResult Index()
        {
            return View();
        }

        #region SubjectInfo

        [HttpGet]
        public ActionResult SubjectInfo()
        {
            ViewBag.College = new CollegeManager().GetADMCollegeMasterList();
            ViewBag.CourseList = new List<SelectListItem>();
            ViewBag.SemesterList = new List<SelectListItem>();

            //List<SubjectDetails> _model = new SubjectsManager().SubjectInfo();
            //return View(_model);
            return View();
        }


        public ActionResult SubjectDetails(Parameters parameter)
        {
            //List<ADMSubjectMaster> listSubject = new List<ADMSubjectMaster>();
            //if (parameter.Filters != null)
            //{
            //    SearchFilter defaultFilter = new SyllabusManager().GetDefaultSearchFilter();
            //    if (parameter.Filters == null)
            //        parameter.Filters = new List<SearchFilter>();
            //    parameter.Filters.Add(defaultFilter);
            //    listSubject = new SubjectsManager().GetAllSubjectDetails(parameter);
            //}
            //return View(listSubject );

            List<SubjectDetails> listSubject = new List<SubjectDetails>();
            if (parameter.Filters != null)
            {
                // SearchFilter defaultFilter = new SyllabusManager().GetDefaultSearchFilter();
                if (parameter.Filters == null)
                    parameter.Filters = new List<SearchFilter>();
                //parameter.Filters.Add(defaultFilter);
                listSubject = new SubjectsManager().GetAllSubjectDetails(parameter);
            }
            return View(listSubject);



            //List<Subject> _model = new SubjectsManager().GetAllSubjectDetails(parameter);
            //return View(_model);

        }
        public PartialViewResult _GetChildDDLL(string id, string Type, string childType, string childSubType)
        {
            ViewBag.Type = Type;
            ViewBag.ChildType = childType;
            ViewBag.ChildSubType = childSubType;
            ViewBag.ChildValues = new SubjectsManager().FetchChildDDlValuesByCollegeID(id, Type);
            return PartialView();
        }
        #endregion


    }
}