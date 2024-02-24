using CUSrinagar.BusinessManagers;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using CUSrinagar.Models;
using CUSrinagar.OAuth;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CUSrinagar.WebApp.CUCollegeAdminPanel.Controllers
{
    [OAuthorize(AppRoles.College)]
    public class SemesterCentersController : CollegeAdminBaseController
    {

        #region DisplayDetails
        private void SetViewBags()
        {
            List<SelectListItem> Colleges = new CollegeManager().GetADMCollegeMasterList();

            List<SelectListItem> CreateCenters = new List<SelectListItem>();
            CreateCenters.AddRange(Colleges);
            ViewBag.CreateCenters = CreateCenters;

            Colleges.Insert(0, new SelectListItem { Text = "All", Value = Guid.Empty.ToString() });
            ViewBag.Colleges = Colleges;

            ViewBag.Programmes = Helper.GetSelectList<Programme>();
            ViewBag.ProgrammesCountDDL = Helper.GetSelectList<Programme>();

            IEnumerable<SelectListItem> ProgrammesMerge = Helper.GetSelectList(Programme.HS, Programme.Professional);
            ProgrammesMerge.First(x => x.Value == "3").Text = "Integrated / Honor's / Professional";
            ViewBag.ProgrammesMergeDDL = ProgrammesMerge;

            ViewBag.ExamFormListTypeDDL = Helper.GetSelectList<ExamFormListType>();
        }

        [HttpGet]
        public ActionResult Centers()
        {
            SetViewBags();
            ViewBag.Data = new SemesterCentersManager().GetSemesterCentersMaster((Guid)AppUserHelper.College_ID);
            return View();
        }
        #endregion


        #region GenerateCenterNotice
        [HttpGet]
        public ActionResult DownloadCenterNotices()
        {
            return RedirectToAction("Centers");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DownloadCenterNotices(SemesterCenters _RePrintCenterNotice)
        {
            ModelState.Remove(nameof(_RePrintCenterNotice.PipeSeparatedCenters));
            ModelState.Remove(nameof(_RePrintCenterNotice.College_ID));
            ModelState.Remove(nameof(_RePrintCenterNotice.PrintCenterNotice.College_ID));
            if (!ModelState.IsValid)
                return RedirectToAction("Centers");
            _RePrintCenterNotice.PrintCenterNotice.College_ID = (Guid)AppUserHelper.College_ID;
            List<CenterNotice> list = new SemesterCentersManager().GetCenterNoticeDetails(_RePrintCenterNotice.PrintCenterNotice);
            TempData["response"] = null;
            if (list.IsNullOrEmpty())
            {
                TempData["response"] = @"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> No details were found.</div>";
                return RedirectToAction("Centers");
            }
            return View(list);
        }
        #endregion

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DownloadCenterWiseSubjectCount(SemesterCenters _RePrintCenterNotice)
        {
            ModelState.Remove(nameof(_RePrintCenterNotice.PipeSeparatedCenters));
            ModelState.Remove(nameof(_RePrintCenterNotice.College_ID));
            if (!ModelState.IsValid)
                return RedirectToAction("Centers");

            _RePrintCenterNotice.College_ID = AppUserHelper.College_ID.Value;
            _RePrintCenterNotice.PrintCenterNotice.College_ID = _RePrintCenterNotice.College_ID;
            DataTable CenterWiseSubjectCount = await new SemesterCentersManager().GetCenterWiseSubjectCountAsync(_RePrintCenterNotice.PrintCenterNotice);

            if (CenterWiseSubjectCount == null || CenterWiseSubjectCount?.Rows.Count <= 0)
            {
                TempData["response"] = @"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> No details were found.</div>";
                return RedirectToAction("Centers");
            }

            return DownloadExcel(CenterWiseSubjectCount, $"{_RePrintCenterNotice.PrintCenterNotice.ListType.ToString()}_CenterWiseSubjectCount{_RePrintCenterNotice.PrintCenterNotice.CourseCategory.ToString()}_Sem-{_RePrintCenterNotice.PrintCenterNotice.Semester}_ExamYear-{_RePrintCenterNotice.PrintCenterNotice.ExaminationYear}");
        }

    }
}