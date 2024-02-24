using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CUSrinagar.Models;
using CUSrinagar.BusinessManagers;
using CUSrinagar.Extensions;
using CUSrinagar.Enums;
using System.Text.RegularExpressions;
using System.IO;
using System.Web.UI;
using CUSrinagar.OAuth;
using GeneralModels;

namespace CUSrinagar.WebApp.CUSrinagarAdminPanel.Controllers
{
    [OAuthorize(AppRoles.University)]
    public class AdmissionSettingsController : Controller
    {
        string errorMsg = $"<div class='alert alert-success alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>##</a></div>";

        #region Regular Admission Settings
        [HttpGet]
        public ActionResult RegularAdmSettings()
        {
            return View(new AdmissionSettingsManager().GetFormNoMasterList());
        }

        [HttpGet]
        public ActionResult EditRegularAdmSettings(PrintProgramme? id)
        {
            if (id == null)
                return RedirectToAction("RegularAdmSettings");
            List<SelectListItem> ADMCourses = new AdmissionSettingsManager().GetCourses((PrintProgramme)id);
            List<SelectListItem> SFCourses = ADMCourses.DeepCloneObject();

            ARGFormNoMasterSettings formNoMasterSettings = new AdmissionSettingsManager().GetFormNoMasterSettings((PrintProgramme)id);

            List<ARGCoursesApplied> CoursesApplied = new CourseManager().GetCourseListForRegistration((PrintProgramme)id);
            if (CoursesApplied.IsNotNullOrEmpty())
            {
                foreach (var item in ADMCourses)
                {
                    if (CoursesApplied.Any(x => x.Course_ID.ToString().ToLower() == item.Value.ToLower()))
                        item.Selected = true;
                }
            }
            if (!string.IsNullOrWhiteSpace(formNoMasterSettings.AllowProgrammesInSF))
            {
                foreach (var item in SFCourses ?? new List<SelectListItem>())
                {
                    if (formNoMasterSettings.AllowProgrammesInSF.ToLower().Contains(item.Value.ToLower()))
                        item.Selected = true;
                }
            }

            ViewBag.ADMCourses = ADMCourses;
            ViewBag.SFCourses = SFCourses;

            return View(formNoMasterSettings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditRegularAdmSettings(ARGFormNoMasterSettings formNoMasterSettings)
        {
            if (formNoMasterSettings == null)
                return RedirectToAction("RegularAdmSettings");
            new AdmissionSettingsManager().UpdateSettings(formNoMasterSettings);
            TempData["response"] = errorMsg.Replace("##", "updated successfully");

            return RedirectToAction("EditRegularAdmSettings", new { id = (short)formNoMasterSettings.PrintProgramme });
        } 
        #endregion
    }
}