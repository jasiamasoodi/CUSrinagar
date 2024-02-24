using CUSrinagar.BusinessManagers;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using CUSrinagar.Models;
using CUSrinagar.OAuth;
using GeneralModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CUSrinagar.WebApp.CUSrinagarAdminPanel.Controllers
{
    [OAuthorize(AppRoles.College,AppRoles.University)]
    public class AssignRollNumberController : Controller
    {
        public ActionResult ExaminationForm(PrintProgramme? programme)
        {
            FillViewBags();
            return View();
        }
        public ActionResult ExaminationFormListPartail(Parameters parameter, PrintProgramme? otherParam1)
        {
            List<StudentExamFormList> list = null;
            if (otherParam1.HasValue)
            {
                list = new ExaminationFormManager().GetExaminationFormList(1, PrintProgramme.IH, false);
            }
            return View(list);
        }                     

        void FillViewBags()
        {
            ViewBag.PrintProgrammeList = Helper.GetSelectList(PrintProgramme.BED).OrderBy(x => x.Text);
        }
    }
}