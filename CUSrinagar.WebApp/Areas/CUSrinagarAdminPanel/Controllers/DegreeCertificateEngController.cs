using CUSrinagar.BusinessManagers;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using CUSrinagar.Models;
using CUSrinagar.OAuth;
using GeneralModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CUSrinagar.WebApp.CUSrinagarAdminPanel.Controllers
{
    [OAuthorize(AppRoles.DegreeCertificate, AppRoles.University)]
    public class DegreeCertificateEngController : AdminController
    {
        public ActionResult DegreeCertificateEng()
        {
            FillViewBag_College();
            FillViewBag_PrintProgrammes();
            FillViewBag_Course(Programme.UG);
            ViewBag.Batch = "2017";
            ViewData["PageSize"] = -1;
            return View();
        }
        

        public PartialViewResult DegreeCertificateListEng(Parameters parameter, PrintProgramme? otherparam1)
        {
            List<DegreeCertificate> list = new DegreeCertificateManager().ListEng(parameter, otherparam1.Value);

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
           // ExportToCSV(list, $"DegreeCertificate_{DateTime.Now.ToString("dd-MMM-yyyy")}");
        }

        public ActionResult PrintDegreeCertificateEng(int? Batch, PrintProgramme PrintProgramme, Guid? Course_ID, Guid? AcceptCollege_ID, string CUSRegistrationNo
       , string GreaterThanDate, string LessThanDate, short? PrintedOn, short? ValidatedOn, short? HandedOver)
        {
            Parameters parameters = new Parameters() { Filters = new List<SearchFilter>(), PageInfo = new Paging() { DefaultOrderByColumn = "ExamRollNumber", PageNumber = -1, PageSize = -1 }, SortInfo = new Sort { ColumnName = "ExamRollNumber" } };
            parameters.Filters.Add(new SearchFilter() { Column = "PrintProgramme", Value = ((short)PrintProgramme).ToString() });

            ViewBag.PrintProgramme = PrintProgramme;

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
            List<DegreeCertificate> DegreeCertificates = new DegreeCertificateManager().ListEng(parameters, PrintProgramme) ?? new List<DegreeCertificate>();

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

        public async Task<JsonResult> CreateALL(List<string> Student_ID, int Batch, PrintProgramme PrintProgramme)
        {
            Guid User_ID = AppUserHelper.User_ID;
            ResponseData response = await new DegreeCertificateManager().SaveAsyncEng(Student_ID, Batch, PrintProgramme, User_ID);
            return Json(response);
        }

        public JsonResult Validated(Guid Student_ID)
        {
            ResponseData response = new DegreeCertificateManager().Validated(Student_ID);
            return Json(response);
        }
        public JsonResult Printed(Guid Student_ID)
        {
            ResponseData response = new DegreeCertificateManager().Printed(Student_ID);
            return Json(response);
        }

        [HttpPost]
        public JsonResult Duplicate(Guid Student_ID, string DuplicateLbl)
        {
            ResponseData response = new DegreeCertificateManager().Duplicate(Student_ID, DuplicateLbl);
            return Json(response);
        }

        public JsonResult HandedOver(Guid Student_ID)
        {
            ResponseData response = new DegreeCertificateManager().HandedOverOn(Student_ID);
            return Json(response);
        }



    }
}