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
    [OAuthorize(AppRoles.University,AppRoles.PublishResult)]
    public class ResultNotificationController : AdminController
    {
        public ActionResult ResultNotification()
        {
            FillViewBag_Programmes();
            FillViewBag_Semesters();
            ViewBag.Batchs = Helper.GetYearsDDL();
            return View();
        }
        public PartialViewResult ResultNotificationListPartial(Parameters parameter, PrintProgramme? otherParam1)
        {
            var resultNotificationList = new ResultNotificationManager().ListCompact(parameter);
            return PartialView(resultNotificationList);
        }

        public void CSV(Parameters parameter, PrintProgramme? printProgramme)
        {
            List<ResultNotificationList> list = new ResultNotificationManager().ListCompact(parameter) ?? new List<ResultNotificationList>();
            var CSVlist = list.Select(
                col => new
                {
                    col.Batch,
                    col.Semester,
                    Date = col.Dated.ToString("dd-MMMM-yyyy"),
                    col.Title,
                    col.ImportedToMasterTable
                }).ToList();
            ExportToCSV(CSVlist, $"Result-notifiction-list");
        }

        public JsonResult ImportToMasterTable(Guid ResultNotification_ID)
        {
            ResponseData response = new ResultManager().ImportResultToMasterTable(ResultNotification_ID);
            return Json(response);
        }

        public ActionResult CreateResultNotification()
        {
            SetViewBags(new ResultNotification());
            return View(new ResultNotification() { Dated = DateTime.Now.Date });
        }

        private void SetViewBags(ResultNotification resultNotification)
        {
            int semester = 0;
            ViewBag.ParentNotifications = new ResultManager().GetResultNotifications(semester);
            List<int> notifications = new List<int>();
            notifications.Add((int)NotificationType.Examination);
            notifications.Add((int)NotificationType.Result);
            ViewBag.GeneralNotifications = new NotificationManager().GetNotificationSLByType(notifications);
            FillViewBag_PrintProgrammes();
            FillViewBag_Programmes();
            FillViewBag_Semesters();
            FillViewBag_Courses(resultNotification.PrintProgramme != 0 ? resultNotification.PrintProgramme : PrintProgramme.UG);
            FillViewBag_College();
            ViewBag.Batchs = Helper.GetYearsDDL();
            FillViewBag_Subjects(resultNotification.Course_ID, (Semester)resultNotification.Semester);
        }


        [HttpPost]
        public ActionResult CreateResultNotification(ResultNotification resultNotification)
        {
            if (checkNotificationNo(resultNotification.NotificationNo))
            { ModelState.AddModelError("NotificationNo", "Notification No Already Exists"); }
            if (ModelState.IsValid)
            {
                resultNotification.ResultNotification_ID = Guid.NewGuid();
                resultNotification.CreatedOn = DateTime.Now;
                resultNotification.CreatedBy = AppUserHelper.User_ID;
                new ResultManager().Save(resultNotification);
                return RedirectToAction("DeclareResult", resultNotification);
            }
            SetViewBags(resultNotification);
            return View(resultNotification);
        }

        private bool checkNotificationNo(string notificationNo)
        {
            return new ResultManager().GetResultNotification(notificationNo) != null;
        }

        public ActionResult DeclareResult(ResultNotification resultNotification)
        {
            SetViewBags(resultNotification);
            return View(resultNotification);
        }
        [HttpPost]
        public ActionResult DeclareResults(ResultNotification resultNotification)
        {
            if (resultNotification.CourseIds != null)
            {
                resultNotification.CourseIds.RemoveAll(x => x == Guid.Empty);
                if (new ResultManager().DeclareResult(resultNotification) > 0)
                {
                    TempData["response"] = $"<div class='col-sm-1'></div><div class='alert alert-success col-sm-10'><button type='button' class='close' data-dismiss='alert'><i class='ace-icon fa fa-times'></i></button><strong><i class='ace-icon fa fa-times'></i> Awesome!</strong> Result declared successfully.<br></div><div class='col-sm-1'></div>";
                }
                else
                {
                    TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'> <strong>OH!</strong> No Record effected.</a></div>";
                }
            }
            else
            {
                TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'> <strong>Select Course.</strong></a></div>";
            }
            SetViewBags(resultNotification);
            return View("DeclareResult", resultNotification);
        }
        public ActionResult ValidateResult(ResultNotification resultNotification)
        {
            SetViewBags(resultNotification);
            return View(resultNotification);
        }
        [HttpPost]
        public ActionResult ValidateResults(ResultNotification resultNotification)
        {
            resultNotification.AwardCounts = new ResultNotificationManager().ValidateResult(resultNotification);

            SetViewBags(resultNotification);
            return View("ValidateResult", resultNotification);
        }
    }

}
