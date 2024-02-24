using System;
using System.Collections.Generic;
using System.Web.Mvc;
using CUSrinagar.Models;
using CUSrinagar.BusinessManagers;
using GeneralModels;
using CUSrinagar.Extensions;
using CUSrinagar.Enums;

namespace CUSrinagar.WebApp.Controllers
{
    public class NotificationController : Controller
    {
        public ActionResult Notification(NotificationType? TypeIS)
        {
            ViewBag.NotificationTypeParam = (int)(TypeIS ?? 0);
            IEnumerable<SelectListItem> NotificationTypeDDL = new List<SelectListItem>();
            NotificationTypeDDL = Helper.GetSelectList<NotificationType>();
            ViewBag.NotificationTypeList = NotificationTypeDDL;
            return View();
        }

        public ActionResult NotificationListPartial(Parameters parameter)
        {
            bool setsortinfo = (parameter.SortInfo == null || parameter.SortInfo.ColumnName == null);
            parameter = new NotificationManager().GetDefaultParameter(parameter, true, false, setsortinfo, 15, true);
            List<Notification> listNotification = new NotificationManager().GetAllNotificationList(parameter);
            return View(listNotification);
        }

        public ActionResult _DetailedNotification(Guid Notification_ID)
        {
            Notification Notification = new NotificationManager().GetNotificationById(Notification_ID);
            return View(Notification);
        }

        public ActionResult SelectionList()
        {
            return View();
        }

        [HttpGet]
        public ActionResult EntranceExamNotification()
        {
            return View(new EntranceListManager().GetList());
        }
    }
}