using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using CUSrinagar.Models;
using System.IO;
using CUSrinagar.BusinessManagers;
using GeneralModels;
using CUSrinagar.Enums;
using CUSrinagar.OAuth;
using CUSrinagar.Extensions;
using IronXL;
using System.Data;
using System.Threading.Tasks;

namespace CUSrinagar.WebApp.CUSrinagarAdminPanel.Controllers
{
    [OAuthorize(AppRoles.University, AppRoles.University_Evaluator, AppRoles.University_Results, AppRoles.View_TransScript)]
    public class ResultHistoryController : AdminController
    {
        #region ResultListSection
        public ActionResult ResultHistory()
        {
            FillViewBag_College();
            FillViewBag_Programmes();
            FillViewBag_Course(Programme.UG);
            FillViewBag_Semesters();
            return View();
        }
             
        public PartialViewResult ResultHistoryPartial(Parameters parameter, PrintProgramme? otherParam1)
        {
            short.TryParse(parameter.Filters?.FirstOrDefault(x => x.Column == "Semester")?.Value, out short semester);
            if (semester <= 0 || !otherParam1.HasValue)
                return null;
            List<ResultList> list = new ResultManager().History(otherParam1.Value, parameter, semester, true);
            return PartialView(list);
        }


    }
}
#endregion