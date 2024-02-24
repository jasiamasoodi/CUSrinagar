using CUSrinagar.BusinessManagers;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using GeneralModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CUSrinagar.WebApp.CUStudentZone.Controllers
{
    public class StudentBaseController : Controller
    {
        protected EmptyResult DownloadExcel<T>(List<T> _dataSource, List<string> _modelPropertiesAsColumns, string fileName = null)
        {
            try
            {
                var resultsExcel = _dataSource.DownloadExcel(_modelPropertiesAsColumns, fileName);
                Response.ClearContent();
                Response.AddHeader("content-disposition", "attachment; filename=" + resultsExcel.Item2 + ".xls");
                Response.ContentType = "application/ms-excel";
                StringWriter sw = new StringWriter();
                HtmlTextWriter htw = new HtmlTextWriter(sw);
                resultsExcel.Item1.HeaderRow.Style.Add("background-color", "#438EB9");
                resultsExcel.Item1.HeaderRow.Style.Add("color", "white");
                Response.Charset = "";
                resultsExcel.Item1.RenderControl(htw);

                Response.Output.Write(sw.ToString());
                Response.Flush();
                Response.End();
            }
            catch (NullReferenceException) { }
            return new EmptyResult();
        }
        protected EmptyResult DownloadExcel(DataTable dataTable, string fileName = null)
        {
            try
            {
                GridView gridView = new GridView();
                gridView.DataSource = dataTable;
                gridView.DataBind();
                dataTable.Dispose();
                Response.ClearContent();
                Response.AddHeader("content-disposition", "attachment; filename=" + fileName + ".xls");
                Response.ContentType = "application/ms-excel";
                StringWriter sw = new StringWriter();
                HtmlTextWriter htw = new HtmlTextWriter(sw);
                gridView.HeaderRow.Style.Add("background-color", "#438EB9");
                gridView.HeaderRow.Style.Add("color", "white");
                Response.Charset = "";
                gridView.RenderControl(htw);

                Response.Output.Write(sw.ToString());
                Response.Flush();
                Response.End();
            }
            catch (NullReferenceException) { }
            return new EmptyResult();
        }

        public void GetResponseViewBags()
        {
            ViewBag.ErrorMessage = TempData["ErrorMessage"];
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
        }
        public void SetResponseViewBags(ResponseData responseData)
        {
            TempData["ErrorMessage"] = responseData?.ErrorMessage;
            TempData["SuccessMessage"] = responseData?.SuccessMessage;
        }


        #region FillViewBags
        public void FillViewBag_College()
        {
            IEnumerable<SelectListItem> list = new CollegeManager().GetADMCollegeMasterList();
            ViewBag.College = list ?? new List<SelectListItem>();
        }
        public void FillViewBag_PrintProgrammes()
        {
            IEnumerable<SelectListItem> list = Helper.GetSelectList<PrintProgramme>();
            ViewBag.PrintProgrammes = list ?? new List<SelectListItem>();
        }
        public void FillViewBag_Programmes()
        {
            IEnumerable<SelectListItem> list = Helper.GetSelectList<Programme>();
            ViewBag.Programmes = list ?? new List<SelectListItem>();
        }
        public void FillViewBag_SubjectTypes()
        {
            IEnumerable<SelectListItem> list = Helper.GetSelectList<SubjectType>();
            ViewBag.SubjectTypes = list ?? new List<SelectListItem>();
        }
        public void FillViewBag_Semesters()
        {
            IEnumerable<SelectListItem> list = Helper.GetSelectList<Semester>();
            ViewBag.Semesters = list ?? new List<SelectListItem>();
        }
        public void FillViewBag_Course(PrintProgramme printProgramme)
        {
            List<SelectListItem> list = new CourseManager().GetAllCoursesByPrintProgramme(printProgramme);
            ViewBag.Courses = list ?? new List<SelectListItem>();
        }
        public void FillViewBag_Course(Programme programme)
        {
            List<SelectListItem> list = new CourseManager().GetAllCoursesByProgramme((int)programme);
            ViewBag.Courses = list ?? new List<SelectListItem>();
        }
        public void FillViewBag_Course(Guid? College_ID, PrintProgramme? printProgramme, Programme? programme)
        {

            List<SelectListItem> list = new CourseManager().GetCourseList(College_ID, printProgramme, programme) ?? new List<SelectListItem>();
            ViewBag.Courses = list ?? new List<SelectListItem>();
        }
        public void FillViewBag_Course(Guid? Course_ID)
        {
            Programme programme = Programme.UG;
            if (Course_ID.HasValue && Course_ID != Guid.Empty)
                programme = (Programme)new CourseManager().GetCourseById(Course_ID.Value).Programme;
            FillViewBag_Course(programme);
        }
        public void FillViewBag_Programmes(Guid? College_ID)
        {
            List<SelectListItem> list = new CourseManager().GetProgrammes(College_ID);
            ViewBag.Programmes = list ?? new List<SelectListItem>();

        }
        //public void FillViewBag_Departments(string SchoolFullName = null)
        //{
        //    List<SelectListItem> list = new SubjectsManager().DepartmentDataList(SchoolFullName);
        //    ViewBag.Departments = list ?? new List<SelectListItem>();
        //}
        public void FillViewBag_Schools(Guid? College_ID)
        {
            List<SelectListItem> list = new SubjectsManager().SchoolSelectList(College_ID);
            ViewBag.Schools = list ?? new List<SelectListItem>();
        }
        #endregion
    }
}