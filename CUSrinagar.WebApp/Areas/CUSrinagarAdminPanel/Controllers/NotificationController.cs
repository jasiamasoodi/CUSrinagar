using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using CUSrinagar.Models;
using System.Web.Compilation;
using System.IO;
using CUSrinagar.Enums;
using CUSrinagar.BusinessManagers;
using CUSrinagar.Extensions;
using CUSrinagar.OAuth;
using System.Threading.Tasks;

namespace CUSrinagar.WebApp.CUSrinagarAdminPanel.Controllers
{
    [OAuthorize(AppRoles.University, AppRoles.University_Registrar, AppRoles.University_Notification)]
    public class NotificationController : Controller
    {
        string errorMsg = $"<div class='alert alert-success alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>##</a></div>";
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult NotificationListPartial(GeneralModels.Parameters parameter)
        {
            bool setsortinfo = (parameter.SortInfo == null || parameter.SortInfo.ColumnName == null);
            parameter = new NotificationManager().GetDefaultParameter(parameter, false, false, setsortinfo);
            List<Notification> listNotification = new NotificationManager().GetAllNotificationList(parameter);
            return View(listNotification);
        }

        public ActionResult _DetailedNotification(Guid Notification_ID)
        {
            Notification Notification = new NotificationManager().GetNotificationById(Notification_ID);
            return View(Notification);
        }

        public ActionResult Create()
        {
            SetViewBags();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Notification input)
        {
            try
            {
                if (input.IsLink && string.IsNullOrEmpty(input.Link))
                {
                    ModelState.AddModelError("Link", "Link required");
                }
                if (input.Files != null && (new NotificationManager().checkFileExists(input)))
                {
                    ModelState.AddModelError("Files", "File Name Already Exist");
                }
                if (ModelState.IsValid)
                {

                    new NotificationManager().AddNotification(input);


                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("ErrorMessage", ex.Message);
            }
            SetViewBags();

            return View(input);
        }


        public ActionResult Edit(Guid id)
        {
            Notification model = new NotificationManager().GetNotificationById(id);
            SetViewBags();
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(Notification input)
        {
            try
            {
                if (input.IsLink && string.IsNullOrEmpty(input.Link))
                {
                    ModelState.AddModelError("Link", "Link required");
                }
                if (input.Files != null && (new NotificationManager().checkFileExists(input)))
                {
                    ModelState.AddModelError("Files", "File Name Already Exist");
                }
                if (ModelState.IsValid)
                {

                    new NotificationManager().EditNotification(input);
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("ErrorMessage", ex.Message);
            }
            SetViewBags();
            return View(input);
        }




        public ActionResult Delete(int id)
        {
            return View();
        }

        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
        public string ChangeStatus(Guid id)
        {
            string msg = string.Empty;
            msg = new NotificationManager().ChangeStatus(id);
            return msg;
        }
        public void SetViewBags(Syllabus syllabus = null)
        {
            IEnumerable<SelectListItem> NotificationTypesDDL = new List<SelectListItem>();
            NotificationTypesDDL = Helper.GetSelectList<NotificationType>();

            ViewBag.NotificationTypesList = NotificationTypesDDL == null ? new List<SelectListItem>() : NotificationTypesDDL;
        }

        [HttpGet]
        public ActionResult MarqueeText()
        {
            ViewBag.MarqueeText = new NotificationManager().GetMarqueeText();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult MarqueeText(string MarqueeText)
        {
            int result = new NotificationManager().SaveUpdateMarqueeText(MarqueeText);
            TempData["response"] = result > 0 ?
                errorMsg.Replace("##", "Saved Successfully") :
                errorMsg.Replace("##", "An Error occurred");
            ViewBag.MarqueeText = new NotificationManager().GetMarqueeText();
            return View();
        }
    }
}
