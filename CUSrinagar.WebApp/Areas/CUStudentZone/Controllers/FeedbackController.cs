using CUSrinagar.Enums;
using CUSrinagar.Models;
using CUSrinagar.OAuth;
using System;
using CUSrinagar.BusinessManagers;
using System.Collections.Generic;
using CUSrinagar.Extensions;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GeneralModels;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CUSrinagar.WebApp.CUStudentZone.Controllers
{
    [OAuthorize(AppRoles.Student)]
    public class FeedbackController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            DDLViewBags();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(DailyLectureFeedBack dailyLectureFeedBack)
        {
            dailyLectureFeedBack.Student_ID = AppUserHelper.User_ID;
            ModelState.Remove("Student_ID");
            DDLViewBags();
            if (ModelState.IsValid)
            {
                Tuple<bool, string> list = new FeedbackManager().Save(dailyLectureFeedBack, AppUserHelper.OrgPrintProgramme);
                if (list.Item1)
                {
                    TempData["response"] = "<div class='alert alert-success alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>" + list.Item2 + "</a></div>";
                }
                else
                {
                    TempData["response"] = "<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>" + list.Item2 + "</a></div>";
                    return View("Index", dailyLectureFeedBack);
                }
            }
            return View();
        }

        [NonAction]
        private void DDLViewBags()
        {
            Guid Student_ID = AppUserHelper.User_ID;
            PrintProgramme printProgramme = AppUserHelper.OrgPrintProgramme;
            ViewBag.studentBatch = AppUserHelper.AppUsercompact.OrgBatch;

            StudentManager stdManager = new StudentManager();
            short _CurrentSemester = stdManager.GetStudentCurrentSemester(Student_ID, printProgramme);
            ViewBag.CurrentSemester = _CurrentSemester;
            ViewBag.ABCIDOfCurrentStd = stdManager.GetStudentABCID(Student_ID, printProgramme) ?? "";

            ViewBag.FacultyTypeList = Helper.GetSelectList<FacultyTypes>();
            ViewBag.ModeOfTeachingList = Helper.GetSelectList<ModeOfTeachings>();
            ViewBag.RatingList = Helper.GetSelectList<Rating>();
            ViewBag.MaterialProvidedList = Helper.GetSelectList<MaterialType>();

            ViewBag.YesNo = new List<SelectListItem> {
                 new SelectListItem
                 {
                      Text="Yes",
                      Value="True"
                 }
                 ,new SelectListItem
                 {
                      Text="No",
                      Value="False"
                 }
            };

            List<ADMSubjectMaster> subjectsList = stdManager.GetStudentCurrentSemSubjects(Student_ID, _CurrentSemester, printProgramme);
            ViewBag.SubjectList = subjectsList?.Select(s =>
                                 new SelectListItem { Text = s.SubjectFullName + "-" + s.SubjectType, Value = s.Subject_ID.ToString() })
                                ?.ToList() ?? new List<SelectListItem>();
        }

        public ActionResult FeedbackList(Parameters parameter)
        {
            if (parameter.Filters == null)
            {
                parameter.Filters = new List<SearchFilter>();
            }
            SearchFilter filter = new SearchFilter() { Column = "Student_Id", Operator = SQLOperator.EqualTo, Value = AppUserHelper.User_ID.ToString(), TableAlias = "p" };
            parameter.Filters.Add(filter);

            List<DailyLectureFeedBack> list = new FeedbackManager().GetDailyLectureFeedBacks(parameter, AppUserHelper.OrgPrintProgramme);
            return PartialView(list);
        }

        #region RemoteValidation
        public async Task<JsonResult> ValidateABCID(string ABCID)
        {
            if (Regex.IsMatch(ABCID + "", "^([0-9]*){12}$"))
            {
                if ((ABCID + "").Length >= 12)
                {
                    string result = await new FeedbackManager().ValidateABCID(ABCID);
                    if (string.IsNullOrWhiteSpace(result))
                        return Json(true, JsonRequestBehavior.AllowGet);
                    else
                        return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}