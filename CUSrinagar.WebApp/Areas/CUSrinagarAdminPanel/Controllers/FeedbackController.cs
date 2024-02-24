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
using CUSrinagar.DataManagers;

namespace CUSrinagar.WebApp.CUSrinagarAdminPanel.Controllers
{
    [OAuthorize(AppRoles.Feedback)]
    public class FeedbackController : AdminController
    {
        [HttpGet]
        public ActionResult DailyLectureFeedBacks()
        {
            DDLViewBags();
            return View();
        }

        private void DDLViewBags()
        {
            FillViewBag_Programmes();
            FillViewBag_College();
            FillViewBag_Course(Programme.UG);
            FillViewBag_Semesters();
            ViewBag.RatingList = Helper.GetSelectList<Rating>();
            ViewBag.FacultyType = Helper.GetSelectList<FacultyTypes>();
        }


        public ActionResult DailyLectureFeedBacksList(Parameters parameter)
        {
            if (parameter == null || (parameter?.Filters.IsNullOrEmpty() ?? true))
            {
                return RedirectToAction("DailyLectureFeedBacks");
            }

            PrintProgramme programme = 0;
            SearchFilter sfilter = parameter?.Filters?.Where(x => x.Column == "Programme").FirstOrDefault();
            if (sfilter != null)
            {
                programme =  GeneralFunctions.ProgrammeToPrintProgrammeMapping((Programme)Enum.Parse(typeof(Programme),sfilter.Value));
            }
            parameter?.Filters?.Remove(sfilter);
            List<DailyLectureFeedBack> list = new FeedbackManager().GetDailyLectureFeedBacks(parameter, programme);
            return View(list);
        }
    }
}