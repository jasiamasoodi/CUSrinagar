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

namespace CUSrinagar.WebApp.CUCollegeAdminPanel.Controllers
{
    [OAuthorize(AppRoles.College_AssistantProfessor, AppRoles.College_ClassRepresentative)]
    public class AssignSubjectController : Controller
    {
        // GET: CUCollegeAdminPanel/AssignSubject
        public ActionResult ProfessorSubjectList()
        {
            ViewBag.Semesters = Helper.GetSelectList<Semester>();
            return View();
        }

        //List all users under Role Assistant Professor
        public ActionResult ProfessorSubjectListTable(Parameters parameter)
        {
            List<ProfessorCRClasses> listUsers = new AssignSubjectManager().GetAllProfessorSubjects(parameter);
            return View(listUsers);
        }
        public ActionResult CRSubjectList()
        {
            ViewBag.Semesters = Helper.GetSelectList<Semester>();
            return View();
        }

        //List all users under Role Assistant Professor
        public ActionResult CRSubjectListTable(Parameters parameter)
        {
            List<ProfessorCRClasses> listUsers = new AssignSubjectManager().GetAllCRSubjects(parameter);
            return View(listUsers);
        }
        public ActionResult SubjectClassDetailList(Guid ProfessorCRClasses_ID)
        {
            ViewBag.ProfessorCRClasses_ID = ProfessorCRClasses_ID;
            return View();
        }

        //List all users under Role Assistant Professor
        public ActionResult SubjectClassDetailTable(Parameters parameter, Guid? otherparam1)
        {
            String Column = "Professor_ID";
            if (AppUserHelper.AppUsercompact.UserRoles.Any(x => x == AppRoles.College_ClassRepresentative))
            { Column = "CR_ID"; }
            if (parameter.Filters == null) parameter = new Parameters() { Filters = new List<SearchFilter>() { new SearchFilter() { Column = "ProfessorCRClasses_ID", Value = otherparam1.ToString(), Operator = SQLOperator.EqualTo, GroupOperation = LogicalOperator.AND } } };
            else
                parameter.Filters.Add(new SearchFilter() { Column = "ProfessorCRClasses_ID", Value = otherparam1.ToString(), Operator = SQLOperator.EqualTo, GroupOperation = LogicalOperator.AND } );
            ProfessorCRClasses listDetails = new AssignSubjectManager().GetDetails(parameter, Column);
            SetViewBag();
            return View(listDetails);
        }

        private void SetViewBag()
        {
            IEnumerable<SelectListItem> list = Helper.GetSelectList<ProfessorStatus>();
            ViewBag.ProfessorStatusDDLList = list ?? new List<SelectListItem>();
            IEnumerable<SelectListItem> crlist = Helper.GetSelectList<CRStatus>();
            ViewBag.CRStatusDDLList = crlist ?? new List<SelectListItem>();
        }

        [HttpPost]
        public ActionResult AddOrEdit(ProfessorCRClasses ProfessorCRClasses)
        {
            if (AppUserHelper.AppUsercompact.UserRoles.Any(x => x == AppRoles.College_ClassRepresentative))
            { ProfessorCRClasses.ProfessorCRClassDetails.FirstOrDefault().CRResponseOn = DateTime.Now; }
            new AssignSubjectManager().AddOrEdit(ProfessorCRClasses.ProfessorCRClassDetails.FirstOrDefault());
            return RedirectToAction("SubjectClassDetailList", new { ProfessorCRClasses_ID = ProfessorCRClasses.ProfessorCRClassDetails.FirstOrDefault().ProfessorCRClasses_ID });
        }
    }
}