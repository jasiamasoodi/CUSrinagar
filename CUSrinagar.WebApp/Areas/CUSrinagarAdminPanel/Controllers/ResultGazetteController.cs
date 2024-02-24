using CUSrinagar.OAuth;
using CUSrinagar.Extensions;
using IronXL;
using System.Data;
using System.Threading.Tasks;
using CUSrinagar.DataManagers;
using System.Web.UI.WebControls;
using System.Reflection;
using System.Security.Cryptography;
using CUSrinagar.Enums;
using System.Web.Mvc;
using CUSrinagar.BusinessManagers;
using CUSrinagar.Models;
using GeneralModels;
using System.Collections.Generic;
using System.Linq;

namespace CUSrinagar.WebApp.CUSrinagarAdminPanel.Controllers
{
    [OAuthorize(AppRoles.ResultGazette,AppRoles.University)]
    public class ResultGazetteController : AdminController
    {
        #region ResultGazette
        public ActionResult ResultGazette()
        {
            FillViewBag_College();
            FillViewBag_Programmes();
            FillViewBag_Course(Programme.UG);
            FillViewBag_Semesters();
            return View();
        }

        [HttpPost]
        public ActionResult PrintGazette(Parameters parameter, PrintProgramme? printProgramme, bool printGazette = false, bool printStatistics = false)
        {
            ViewBag.printGazette = printGazette;
            ViewBag.printStatistics = printStatistics;
            ViewBag.param = parameter;
            parameter.PageInfo = new Paging() { PageNumber = -1, PageSize = -1, DefaultOrderByColumn = "CUSRegistrationNo" };
            short? sem = null;
            var semester = parameter.Filters.FirstOrDefault(x => x.Column.ToLower() == "semester" && !string.IsNullOrEmpty(x.Value))?.Value;
            if (semester != null) sem = short.Parse(semester);

            List<ResultCompact> resultlist = new ResultManager().Get(printProgramme.Value, sem, parameter, false, true);
            return View(resultlist);
        }

        #endregion
    }
}