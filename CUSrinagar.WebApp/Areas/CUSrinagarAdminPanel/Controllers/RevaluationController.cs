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
    [OAuthorize(AppRoles.University, AppRoles.ResultRevaluation)]
    public class RevaluationController : AdminController
    {

        public ActionResult CreateRevaluationtNotification()
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




            //FillViewBag_College();
            //FillViewBag_Programmes();            
            //FillViewBag_Semesters();
            //ViewBag.Batchs = Helper.GetYearsDDL();
            //return View();

        }

        [HttpPost]
        public ActionResult CreateRevaluation(ResultNotification revaluationNotification)
        {
            if (checkNotificationNo(revaluationNotification.NotificationNo))
            { ModelState.AddModelError("NotificationNo", "Notification No Already Exists"); }
            ModelState.Remove("ResultNotificationID");
            if (ModelState.IsValid)
            {
                revaluationNotification.ResultNotification_ID = Guid.NewGuid();
                revaluationNotification.CreatedOn = DateTime.Now;
                revaluationNotification.CreatedBy = AppUserHelper.User_ID;
                new ResultManager().Save(revaluationNotification);
                TempData["Message"] = "Notification Created Successfully";
                TempData["RevaluationNotification"] = revaluationNotification.ResultNotification_ID;


            }
            return RedirectToAction("CreateRevaluationtNotification", "Revaluation");


        }

        private bool checkNotificationNo(string notificationNo)
        {
            return new ResultManager().GetResultNotification(notificationNo) != null;
        }


        public ActionResult UpdateRevaluationtResult()
        {

            FillViewBag_College();
            FillViewBag_Programmes();
            FillViewBag_Course(Programme.UG);
            FillViewBag_Semesters();
            ViewBag.Batchs = Helper.GetYearsDDL();
            return View();

        }

        public PartialViewResult UpdateRevaluationtResultPartial(Parameters parameter, PrintProgramme? otherParam1)
        {
            short.TryParse(parameter.Filters?.FirstOrDefault(x => x.Column == "Semester")?.Value, out short semester);
            if (semester <= 0 || !otherParam1.HasValue)
                return null;
            //List<ResultList> list = new ResultManager().List(otherParam1.Value, parameter, semester, true);
            List<ResultList> list = new ResultManager().RevaluationList(otherParam1.Value, parameter, semester, true);
            return PartialView(list);
        }
    }
}

