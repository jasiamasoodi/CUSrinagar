using CUSrinagar.Enums;
using CUSrinagar.Models;
using CUSrinagar.OAuth;
using System;
using CUSrinagar.BusinessManagers;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CUSrinagar.Extensions;
using GeneralModels;

namespace CUSrinagar.WebApp.CUSrinagarAdminPanel.Controllers
{
    [OAuthorize(AppRoles.University_Dean, AppRoles.University)]
    public class MessageController : AdminController
    {
        public ActionResult Message()
        {
            GetResponseViewBags();
            FillViewBags();
            return View();
        }
        public ActionResult MessageList(Parameters parameter)
        {
            parameter.SortInfo = new Sort() { ColumnName = "Date", OrderBy = System.Data.SqlClient.SortOrder.Descending };
            List<Message> list = new MessageManager().GetMessages(parameter);
            return PartialView("MessageList", list);
        }

        [HttpGet]
        public ActionResult Create()
        {
            Message message = (Message)TempData["model"] ?? new Message();
            message.Date = DateTime.Now;
            ViewBag.Courses = new CourseManager().GetCourseList();
            GetResponseViewBags();
            return View(message);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Message model)
        {
            ResponseData response = new ResponseData();
            if (ModelState.IsValid)
            {
                response = new MessageManager().SaveMessage(model);
            }
            else
            {
                response.ErrorMessage = string.Join(", ", ModelState.Values.SelectMany(s => s.Errors).Select(y => y.ErrorMessage).ToArray());
            }
            SetResponseViewBags(response);
            if (response.IsSuccess)
            {
                return RedirectToAction("Message");
            }
            else
            {
                TempData["model"] = model;
                return RedirectToAction("Create");
            }
        }
        public ActionResult Edit(Guid id)
        {
            var model = TempData["model"] ?? new MessageManager().Get(id);
            ViewBag.Courses = new CourseManager().GetCourseList();
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Message model)
        {
            ResponseData response = new ResponseData();
            if (ModelState.IsValid)
            {
                response = new MessageManager().UpdateMessage(model);
            }
            else
            {
                response.ErrorMessage = string.Join(", ", ModelState.Values.SelectMany(s => s.Errors).Select(y => y.ErrorMessage).ToArray());
            }
            SetResponseViewBags(response);
            if (response.IsSuccess)
            {
                return RedirectToAction("Message");
            }
            else
            {
                TempData["model"] = model;
                return RedirectToAction("Edit", new { @id = model.Message_ID });
            }
        }
        #region HelperMethods
        void FillViewBags()
        {
            
        }
        #endregion


    }
}