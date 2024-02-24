using CUSrinagar.BusinessManagers;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using GeneralModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CUSrinagar.WebApp.Controllers
{
    public class WebAppBaseController : Controller
    {
        public void FillViewBag_SubjectTypesOrCourses(bool isNEP, Programme programme)
        {

            IEnumerable<SelectListItem> list =
                isNEP ?
                Helper.GetSelectList(SubjectType.None, SubjectType.FirstSemesterExclusion,
                SubjectType.Core, SubjectType.GE, SubjectType.MIL, SubjectType.OE, SubjectType.OC, SubjectType.Lab, SubjectType.Workshop,
                SubjectType.BSC, SubjectType.ESC, SubjectType.HSMC, SubjectType.DCE, SubjectType.Practical
                )
                : programme == Programme.Professional ?
                new CourseManager().GetCoursesForSyllabus(new List<Programme> { programme, Programme.Engineering })
                : new CourseManager().GetCoursesForSyllabus(new List<Programme> { programme }); 

            ViewBag.SubjectTypesOrCourses = list?.OrderBy(x => x.Value)?.ToList() ?? new List<SelectListItem>();
        }
        public void FillViewBag_Course(Programme programme)
        {
            List<SelectListItem> list = new CourseManager().GetAllCoursesByProgramme((int)programme);
            ViewBag.Courses = list ?? new List<SelectListItem>();
        }
        public void FillViewBag_Programmes()
        {
            IEnumerable<SelectListItem> list = Helper.GetSelectList<Programme>();
            ViewBag.Programmes = list ?? new List<SelectListItem>();
        }
        public void FillViewBag_Semesters()
        {
            IEnumerable<SelectListItem> list = Helper.GetSelectList<Semester>();
            ViewBag.Semesters = list ?? new List<SelectListItem>();
        }
        public void GetResponseViewBags()
        {
            ViewBag.ErrorMessage = TempData["ErrorMessage"];
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
        }

        public void SetResponseViewBags(ResponseData responseData)
        {
            TempData["ErrorMessage"] = responseData.ErrorMessage;
            TempData["SuccessMessage"] = responseData.SuccessMessage;
        }

        public string AbsoluteUrl(string ActionUrl, string Query = null)
        {
            var urlBuilder = new UriBuilder(Request.Url.AbsoluteUri)
            {
                Path = ActionUrl,
                Query = Query
            };
            Uri uri = urlBuilder.Uri;
            string url = urlBuilder.ToString();
            return url;
        }

        public void FillViewBag_Departments(string SchoolFullName = null)
        {
            List<SelectListItem> list = new SubjectsManager().DepartmentDataList(SchoolFullName);
            ViewBag.Departments = list ?? new List<SelectListItem>();
        }



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
    }
}