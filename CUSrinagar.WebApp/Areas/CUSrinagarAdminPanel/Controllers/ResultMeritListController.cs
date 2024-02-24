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
    public class ResultMeritListController : AdminController
    {
        #region ResultListSection
        public ActionResult ResultMeritList()
        {
            FillViewBag_College();
            FillViewBag_Programmes();
            FillViewBag_Course(Programme.PG);            
            return View();
        }

          public PartialViewResult ResultMeritListPartial(Parameters parameter, PrintProgramme? otherParam1)
        {
            short.TryParse(parameter.Filters?.FirstOrDefault(x => x.Column == "Programme")?.Value, out short Programme);
            if (Programme <= 1 || !otherParam1.HasValue)
                return null;
            List<ResultList> list = new ResultManager().ResultMeritList(otherParam1.Value, parameter, true);
            return PartialView(list);
        }

        #endregion
    }
}