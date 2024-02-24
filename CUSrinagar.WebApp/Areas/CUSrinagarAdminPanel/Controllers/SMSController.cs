using CUSrinagar.BusinessManagers;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using CUSrinagar.Models;
using CUSrinagar.OAuth;
using GeneralModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CUSrinagar.WebApp.CUSrinagarAdminPanel.Controllers
{
    [OAuthorize(AppRoles.University)]
    public class SMSController : Controller
    {
        string errorMsg = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>##</a></div>";
        private void ViewBags()
        {
            ViewBag.SMSRegarding = Helper.GetSelectList(SMSRegarding.None).OrderBy(x => x.Value);
            ViewBag.Programme = Helper.GetSelectList<Programme>().OrderBy(x => x.Value);
            ViewBag.Colleges = new List<SelectListItem>();
            ViewBag.Courses = new List<SelectListItem>();
        }

        [HttpGet]
        public ActionResult Send()
        {
            ViewBags();
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Send(SMS _SMS)
        {
            ViewBags();
            if (_SMS?.SMSRegarding == SMSRegarding.Other)
            {
                ModelState.Remove(nameof(_SMS.Programme));
                ModelState.Remove(nameof(_SMS.Batch));
                ModelState.Remove(nameof(_SMS.College_ID));
            }

            if (!ModelState.IsValid)
            {
                StringBuilder errors = new StringBuilder();
                foreach (ModelState modelState in ViewData.ModelState.Values.Where(x => x.Errors.Count > 0))
                {
                    foreach (ModelError error in modelState.Errors)
                        errors.Append(error.ErrorMessage + "<br/>");
                }

                TempData["response"] = errorMsg.Replace("##", $"Data send was not valid,<br/> {errors.ToString()}");
                return RedirectToAction("Send");
            }
            Tuple<bool, string> response = await new SMSHttpPostClient().ComposeSMSAsync(_SMS);
            TempData["response"] = response.Item1
                ? errorMsg.Replace("##", response.Item2).Replace("alert-danger", "alert-success")
                : errorMsg.Replace("##", $"SMS was not send. <strong>{response.Item2}</strong>.");

            return RedirectToAction("Send");
        }

        [HttpPost]
        public PartialViewResult _GetCoursesSMSDDL(SMS _SMS)
        {
            return PartialView(new SMSHttpPostClient().GetCoursesDDL(_SMS.SMSRegarding, _SMS.Programme, _SMS.Batch, _SMS.College_ID));
        }


        [HttpPost]
        public PartialViewResult _GetCollegeCenterSMSDDL(SMS _SMS)
        {
            if (_SMS?.SMSRegarding == SMSRegarding.RelocateEntranceCenter)
            {
                return PartialView(new SMSHttpPostClient().GetCenterList());
            }
            else
            {
                return PartialView(new CollegeManager().GetADMCollegeMasterList());
            }
        }
    }
}