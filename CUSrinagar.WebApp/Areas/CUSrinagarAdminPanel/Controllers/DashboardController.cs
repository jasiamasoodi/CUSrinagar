using CUSrinagar.BusinessManagers;
using CUSrinagar.Enums;
using CUSrinagar.Models;
using CUSrinagar.OAuth;
using GeneralModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace CUSrinagar.WebApp.CUSrinagarAdminPanel.Controllers
{
    [OAuthorize]
    public class DashboardController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public JsonResult MenuList()
        {

            var projectName = Assembly.GetExecutingAssembly().FullName.Split(',')[0];

            Assembly asm = Assembly.GetAssembly(typeof(MvcApplication));

            var model = asm.GetTypes().
                SelectMany(t => t.GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public))
                .Where(d => d.ReturnType.Name == "ActionResult").Select(n => new
                {
                    Controller = n.DeclaringType?.Name.Replace("Controller", ""),
                    Action = n.Name,
                    ReturnType = n.ReturnType.Name,
                    Attributes = string.Join(",", n.GetCustomAttributes().Select(a => a.GetType().Name.Replace("Attribute", ""))),
                    Area = n.DeclaringType.Namespace.ToString().Replace(projectName + ".", "").Replace("Areas.", "").Replace(".Controllers", "").Replace("Controllers", "")
                });

            var json = model.ToList();

            var deserializedData = JsonConvert.SerializeObject(json);

            return Json(new DashboardManager().GetAllMenuItems(), JsonRequestBehavior.DenyGet);
        }


        public PartialViewResult GetOpenExaminations()
        {
            return PartialView(new DashboardManager().GetOpenExaminations());
        }


        public PartialViewResult GetOpenAwardLinks()
        {
            return PartialView(new DashboardManager().GetOpenAwardLinks());
        }

        public PartialViewResult CurrentRunningSemesters()
        {
            return PartialView(new DashboardManager().CurrentRunningSemesters());
        }



        public PartialViewResult GetProgrammeWiseCount()
        {
            var parameters = new Parameters() { Filters = new List<SearchFilter>() { new SearchFilter() { Column = "Semester", TableAlias = "Comb", Value = "1" } } };
            return PartialView(new DashboardManager().GetProgrammeWiseCount());
        }

        public PartialViewResult GetPassingCourses()
        {
            return PartialView(new DashboardManager().GetPassingCourses());
        }


        public PartialViewResult GetFailingCourses()
        {
            return PartialView(new DashboardManager().GetFailingCourses());
        }

        public PartialViewResult _CUSCollegeInfoPartialView(string year)
        {
            List<CUSCollegeInfoDashboard> list = new CollegeManager().GetCollegeListForDashboard(year);
            ViewBag.StudentsInCollege = (from college in list group college by college.ClgCode into c select new { Name = c.First().ClgCode, Total = c.Sum(x => x.Roll) }).ToList();
            ViewBag.StudentsByCourse = (from student in list group student by student.CourseCategory into c select new { Name = c.First().CourseCategory, Total = c.Sum(x => x.Roll) }).ToList();
            return PartialView(list);
        }

        [ChildActionOnly]
        public string GetControlPanelURL()
        {
            if (AppUserHelper.AppUsercompact?.UserRoles.Any(x => x.ToString().ToLower().Contains("university")) ?? false)
                return "/CUSrinagarAdminPanel/Dashboard/Index";
            if (AppUserHelper.AppUsercompact?.UserRoles.Any(x => x.ToString().ToLower().Contains("college")) ?? false)
                return "/CUCollegeAdminPanel/Dashboard/Index";
            if (AppUserHelper.AppUsercompact?.UserRoles.Any(x => x == AppRoles.Student) ?? false)
                return "/CUStudentZone/StudentInfo/GetInfo";
            return "";
        }

        #region Univeristy_Dean
        public PartialViewResult GrievanceWizardList(Parameters parameter)
        {
            //if (parameter == null) parameter = new Parameters();
            //if (parameter.Filters == null) parameter.Filters = new List<SearchFilter>();
            //parameter.Filters.Add(new SearchFilter() { Column = "Date", GroupOperation = LogicalOperator.AND, Operator = SQLOperator.LessThanEqualTo, Value = DateTime.Now.ToString() });
            //parameter.Filters.Add(new SearchFilter() { Column = "Status", GroupOperation = LogicalOperator.AND, Operator = SQLOperator.NotEqualTo, Value = ((short)GrievanceStatus.Resolved).ToString() });
            //parameter.SortInfo = new GeneralModels.Sort() { ColumnName = "Date", OrderBy= System.Data.SqlClient.SortOrder.Descending };
            //parameter.PageInfo = new Paging() { DefaultOrderByColumn = "Date", PageNumber = 1, PageSize = 7 };
            //var list = new GrievanceManager().GetGrievanceListCompact(parameter);
            var list = new GrievanceManager().GetTop10GrievanceListCompact();
            return PartialView(list);
        }

        public PartialViewResult GrievanceWizardChart(Parameters parameter)
        {
            var summary = new DashboardManager().GetGrievanceWidget() ?? new GrievanceWidget();
            return PartialView(summary);
        }
        #endregion
    }
}