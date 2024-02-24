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
    [OAuthorize(AppRoles.University,AppRoles.PublishResult)]
    public class ResultBacklogBatchesController : UniversityAdminBaseController
    {
        #region ResultBacklogBatches

        [HttpGet]
        public ActionResult ResultBacklogBatches()
        {
            ViewBag.Programmes = Helper.GetSelectList<Programme>(); 
            ViewBag.Semesters = Helper.GetSelectList<Semester>();
            ViewBag.Batches = new List<SelectListItem>();
           // ViewBag.PrintProgrammes = Helper.GetSelectList<Programme>();
            return View();
        }


        [HttpPost]
        public PartialViewResult ResultBacklogBatchesPartial(Parameters parameter, PrintProgramme otherParam1)
        {
            List<ResultCompact> resultlist = null;
            short? sem = null;
            int? btch = null;
            short? prg = null;           
            var semester = parameter.Filters.FirstOrDefault(x => x.Column.ToLower() == "semester" && !string.IsNullOrEmpty(x.Value))?.Value;
            var batch = parameter.Filters.FirstOrDefault(y => y.Column.ToLower() == "batch" && !string.IsNullOrEmpty(y.Value))?.Value;
            var programe = parameter.Filters.FirstOrDefault(z => z.Column.ToLower() == "programme" && !string.IsNullOrEmpty(z.Value))?.Value;
           
            if (semester != null) sem = short.Parse(semester);
            if (batch != null) btch = int.Parse(batch);
            if (programe != null) prg = short.Parse(programe);
            

            resultlist = new ResultManager().CheckBacklogBatches(otherParam1, sem, parameter, btch, programe);
            return PartialView(resultlist);
        }


        #endregion
    }
}