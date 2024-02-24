using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using CUSrinagar.Models;
using CUSrinagar.BusinessManagers;
using GeneralModels;
using CUSrinagar.Enums;
using CUSrinagar.OAuth;
using CUSrinagar.Extensions;
using System.Data;
using System.Threading.Tasks;

namespace CUSrinagar.WebApp.CUSrinagarAdminPanel.Controllers
{
    [OAuthorize(AppRoles.DegreeCertificate, AppRoles.University)]
    public class DegreeCertificateController : AdminController
    {
        public ActionResult DegreeCertificate()
        {
            FillViewBag_College();
            FillViewBag_PrintProgrammes();
            FillViewBag_Course(Programme.UG);
            ViewBag.SemestersFrom = new TranscriptManager().GetSemesterFromList() ?? new List<SelectListItem>();
            ViewBag.SemestersTo = new TranscriptManager().GetSemesterToList() ?? new List<SelectListItem>();
            ViewBag.Batches = Helper.GetYearsDDL().OrderByDescending(x => x.Value);
            ViewData["PageSize"] = -1;
            return View();
        }

        public PartialViewResult DegreeCertificateList(Parameters parameter, PrintProgramme? otherparam1)
        {
            List<DegreeCertificate> list = new DegreeCertificateManager().List(parameter, otherparam1.Value);

            ViewBag.PrintProgramme = otherparam1;
            return PartialView(list);
        }
        public void DegreeCertificateCSV(Parameters parameter, PrintProgramme? printProgramme)
        {
            List<DegreeCertificate> DegreeCertificates = new DegreeCertificateManager().List(parameter, printProgramme.Value);
            var list = DegreeCertificates.Select(
                col => new
                {
                    col.DegreeCertificate_ID
                }).ToList();
            ExportToCSV(list, $"DegreeCertificate_{DateTime.Now.ToString("dd-MMM-yyyy")}");
        }

       
        public async Task<JsonResult> CreateALL(List<string> Student_ID, int SemesterFrom, int SemesterTo, int Batch, PrintProgramme PrintProgramme)
        {
            Guid User_ID = AppUserHelper.User_ID;
            ResponseData response = await new DegreeCertificateManager().SaveAsync(Student_ID, SemesterFrom, SemesterTo, Batch, PrintProgramme, User_ID);
            return Json(response);
        }
        public JsonResult Validated(Guid DegreeCertificate_ID)
        {
            ResponseData response = new DegreeCertificateManager().Validated(DegreeCertificate_ID);
            return Json(response);
        }
        public JsonResult Printed(Guid DegreeCertificate_ID)
        {
            ResponseData response = new DegreeCertificateManager().Printed(DegreeCertificate_ID);
            return Json(response);
        }

        [HttpPost]
        public JsonResult Duplicate(Guid DegreeCertificate_ID, string DuplicateLbl)
        {
            ResponseData response = new DegreeCertificateManager().Duplicate(DegreeCertificate_ID, DuplicateLbl);
            return Json(response);
        }

        public JsonResult HandedOver(Guid DegreeCertificate_ID)
        {
            ResponseData response = new DegreeCertificateManager().HandedOverOn(DegreeCertificate_ID);
            return Json(response);
        }

        public ActionResult PrintDegreeCertificate(int? Batch, int SemesterTo, PrintProgramme PrintProgramme, Guid? Course_ID, Guid? AcceptCollege_ID, string CUSRegistrationNo
        , string GreaterThanDate, string LessThanDate, short? PrintedOn, short? ValidatedOn, short? HandedOver)
        {
            Parameters parameters = new Parameters() { Filters = new List<SearchFilter>(), PageInfo = new Paging() { DefaultOrderByColumn = "ExamRollNumber", PageNumber = -1, PageSize = -1 }, SortInfo = new Sort { ColumnName = "ExamRollNumber" } };
            parameters.Filters.Add(new SearchFilter() { Column = "PrintProgramme", Value = ((short)PrintProgramme).ToString() });

            ViewBag.PrintProgramme = PrintProgramme;

            parameters.Filters.Add(new SearchFilter() { Column = "SemesterTo", Value = SemesterTo.ToString(), TableAlias = "DC" });
          
            if (AcceptCollege_ID.HasValue)
                parameters.Filters.Add(new SearchFilter() { Column = "AcceptCollege_ID", Value = AcceptCollege_ID.Value.ToString() });
            if (Course_ID.HasValue)
                parameters.Filters.Add(new SearchFilter() { Column = "Course_ID", Value = Course_ID.ToString(), TableAlias = "C" });
            if (Batch.HasValue)
                parameters.Filters.Add(new SearchFilter() { Column = "Batch", Value = Batch.ToString() });
            if (!string.IsNullOrEmpty(GreaterThanDate) && DateTime.TryParse(GreaterThanDate, out DateTime date1))
                parameters.Filters.Add(new SearchFilter() { Column = "CreatedOn", Value = date1.ToString(), TableAlias = "DC", Operator = SQLOperator.GreaterThanEqualTo });
            if (!string.IsNullOrEmpty(LessThanDate) && DateTime.TryParse(LessThanDate, out DateTime date2))
                parameters.Filters.Add(new SearchFilter() { Column = "CreatedOn", Value = date2.ToString(), TableAlias = "DC", Operator = SQLOperator.LessThanEqualTo });
            else
                parameters.Filters.Add(new SearchFilter() { Column = "CreatedOn", TableAlias = "DC", Operator = SQLOperator.ISNotNULL });
            if (ValidatedOn.HasValue)
                parameters.Filters.Add(new SearchFilter() { Column = "ValidatedOn", TableAlias = "DC", Operator = (ValidatedOn.Value == 1 ? SQLOperator.ISNotNULL : SQLOperator.ISNULL) });
            if (PrintedOn.HasValue)
                parameters.Filters.Add(new SearchFilter() { Column = "PrintedOn", TableAlias = "DC", Operator = (PrintedOn.Value == 1 ? SQLOperator.ISNotNULL : SQLOperator.ISNULL) });
            if (HandedOver.HasValue)
                parameters.Filters.Add(new SearchFilter() { Column = "HandedOverOn", TableAlias = "DC", Operator = (ValidatedOn.Value == 1 ? SQLOperator.ISNotNULL : SQLOperator.ISNULL) });
            if (!string.IsNullOrEmpty(CUSRegistrationNo))
                parameters.Filters.Add(new SearchFilter() { Column = "CUSRegistrationNo", Value = CUSRegistrationNo });
            List<DegreeCertificate> DegreeCertificates = new DegreeCertificateManager().List(parameters, PrintProgramme) ?? new List<DegreeCertificate>();

            foreach (var item in DegreeCertificates?.Where(x => x.DegreeCourseTitle.ToLower().Contains("geography")))
            {
                if (item.DegreeCourseTitle.ToLower().Contains("master"))
                    item.DegreeCourseTitle = new StudentManager().GetCoursePrefixDegree(item.DegreeCourseTitle, PrintProgramme, item.Student_ID, "Graduation");
                else
                    item.DegreeCourseTitle = new StudentManager().GetCoursePrefixDegree(item.DegreeCourseTitle, PrintProgramme, item.Student_ID, "12th");
            }
            if (PrintProgramme == PrintProgramme.PG)
                return View("PrintDegreeCertificate_PG", DegreeCertificates);
            return View(DegreeCertificates);
        }

        [HttpPost]
        public JsonResult MarkPrintedAll(List<Guid> DegreeCertificate_IDs)
        {
            ResponseData response = new DegreeCertificateManager().MarkPrintedAll(DegreeCertificate_IDs);
            return Json(response);
        }

        [HttpPost]
        public JsonResult MarkHandedOverAll(List<Guid> DegreeCertificate_IDs)
        {
            ResponseData response = new DegreeCertificateManager().MarkHandedOverAll(DegreeCertificate_IDs);
            return Json(response);
        }

        [HttpPost]
        public JsonResult MarkAllGood(List<Guid> DegreeCertificate_IDs)
        {
            ResponseData response = new DegreeCertificateManager().MarkAllGood(DegreeCertificate_IDs);
            return Json(response);
        }
    }
}
