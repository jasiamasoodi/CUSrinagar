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
    [OAuthorize(AppRoles.University, AppRoles.ExtPendingList)]
    public class PendingListsController : UniversityAdminBaseController
    {
        #region PendingListSection

        [HttpGet]
        public ActionResult ResultPendingLists()
        {

            List<SelectListItem> Colleges = new CollegeManager().GetADMCollegeMasterList();
            Colleges.Insert(0, new SelectListItem { Value = Guid.Empty.ToString() });
            ViewBag.Colleges = Colleges;

            ViewBag.Programmes = Helper.GetSelectList<Programme>();
            ViewBag.Courses = new List<SelectListItem>();

            ViewBag.Semesters = Helper.GetSelectList<Semester>();
            return View();
        }


        [HttpPost]
        public ActionResult ExportToExcel(Parameters parameter, PrintProgramme printProgramme)
        {
            short.TryParse(parameter?.Filters?.FirstOrDefault(x => x.Column == "Semester")?.Value ?? "0", out short semester);
            if (semester <= 0 || printProgramme == 0)
                return null;
            int.TryParse(parameter?.Filters?.FirstOrDefault(x => x.Column == "Batch")?.Value ?? "0", out int batch);

            Guid.TryParse(parameter?.Filters?.FirstOrDefault(x => x.Column == "Course_ID")?.Value ?? "0", out Guid CourseId);

            SearchFilter prgFilter = parameter?.Filters?.FirstOrDefault(x => x.Column == "Programme");

            DataTable _DataTable = new ResultManager().ResultPendingList(parameter, printProgramme, semester, batch, (Programme)Enum.Parse(typeof(Programme), prgFilter.Value), CourseId);
            if (_DataTable != null)
            {
                return DownloadExcel(_DataTable, "ExtPendingList");
            }
            else
                return DownloadExcel<object>(null, null, null);
        }

        #endregion


    }
}