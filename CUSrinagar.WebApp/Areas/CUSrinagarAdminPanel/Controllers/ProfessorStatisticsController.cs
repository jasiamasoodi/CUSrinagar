using CUSrinagar.BusinessManagers;
using CUSrinagar.Enums;
using CUSrinagar.Models;
using CUSrinagar.OAuth;
using GeneralModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CUSrinagar.WebApp.CUSrinagarAdminPanel.Controllers
{
    [OAuthorize(AppRoles.University)]
    public class ProfessorStatisticsController : Controller
    {
        // GET: CUSrinagarAdminPanel/ProfessorStatistics
        public ActionResult ProfessorList()
        {
            SetViewBags();
            return View();
        }

       // GET: CUSrinagarAdminPanel/Syllabus/Details/5

        public PartialViewResult ProfessorListTable(Parameters parameter)
        {
            List<ProfessorCRClasses> listProfessors = new AssignSubjectManager().GetAllProfessorList(parameter);
            return PartialView(listProfessors);
        }
        private void SetViewBags()
        {
            ViewBag.CollegeList = new CollegeManager().GetADMCollegeMasterList();
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
            if (parameter.Filters == null) parameter = new Parameters() { Filters = new List<SearchFilter>() { new SearchFilter() { Column = "ProfessorCRClasses_ID", Value = otherparam1.ToString(), Operator = SQLOperator.EqualTo, GroupOperation = LogicalOperator.AND } } };
            else
                parameter.Filters.Add(new SearchFilter() { Column = "ProfessorCRClasses_ID", Value = otherparam1.ToString(), Operator = SQLOperator.EqualTo, GroupOperation = LogicalOperator.AND });
            ProfessorCRClasses listDetails = new AssignSubjectManager().GetDetails(parameter, Column);
            return View(listDetails);
        }
    }
}