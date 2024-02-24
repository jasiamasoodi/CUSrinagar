﻿using CUSrinagar.BusinessManagers;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using CUSrinagar.Models;
using CUSrinagar.Models.ValidationAttrs;
using GeneralModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CUSrinagar.WebApp.CUSrinagarAdminPanel.Controllers
{
    public class AdminController : Controller
    {
        #region ResultSection   
        public string InternalExternalSubjectsWithStatus(List<SubjectResult> subjectResults, bool GetTotalInternalMarks = false, bool GetTotalExternalMarks = false)
        {
            var str = "";
            decimal? _TotalInternalMarks = null; decimal? _TotalExternalMarks = null;
            if (subjectResults.IsNotNullOrEmpty())
            {
                foreach (SubjectResult item in subjectResults)
                {
                    bool showOnPublicView = item.ResultNotification_ID.HasValue && item.ExamForm_ID.HasValue && item.HasResult;
                    str += item.SubjectFullName + "(";
                    if (item.InternalStatus != ResultStatus.NotApplicable && showOnPublicView)
                    {
                        if (item.TotalInternalMarksObtained.HasValue)
                        {
                            _TotalInternalMarks = _TotalInternalMarks.HasValue ? _TotalInternalMarks : 0;
                            _TotalInternalMarks += item.TotalInternalMarksObtained.Value;
                            str += item.TotalInternalMarksObtained.Value.ToString("f0") + $"{item.InternalStatus.ToString()},";
                        }
                        else
                            str += $"{item.InternalStatus.ToString()},";
                    }

                    if (item.ExternalStatus != ResultStatus.NotApplicable && showOnPublicView)
                    {
                        if (item.TotalExternalMarksObtained.HasValue)
                        {
                            _TotalExternalMarks = _TotalExternalMarks.HasValue ? _TotalExternalMarks : 0;
                            _TotalExternalMarks += item.TotalExternalMarksObtained.Value;
                            str += item.TotalExternalMarksObtained.Value.ToString("f0") + $"{item.ExternalStatus.ToString()}";
                        }
                        else
                            str += $"{item.ExternalStatus.ToString()}";
                        str += ") | ";
                    }
                }
            }
            if (GetTotalInternalMarks)
                return _TotalInternalMarks.HasValue ? _TotalInternalMarks.Value.ToString() : "N.A";
            if (GetTotalExternalMarks)
                return _TotalExternalMarks.HasValue ? _TotalExternalMarks.Value.ToString() : "N.A";
            return str.TrimEnd(' ').TrimEnd('|').TrimEnd(' ');
        }
        //public string InternalSubjectMarksWithStatus(SubjectResultCompact item, bool WithStatusAsSuffix = true)
        //{
        //    var _ResultStatus = "";

        //    if (item != null)
        //    {
        //        bool InternalApplicable = false; decimal InternalMaxMarks = 0; decimal InternalMinPassMarks = 0; decimal? InternalMarksObtained = null; var InternalMarksTitle = "";

        //        if (item.IsInternalMarksApplicable)
        //        {
        //            if (item.InternalIsPartOf == MarksIsPartOf.Internal)
        //            {
        //                InternalApplicable = true; InternalMinPassMarks += item.InternalMinPassMarks; InternalMaxMarks += item.InternalMaxMarks;
        //                if (item.InternalSubmitted)
        //                {
        //                    if (decimal.TryParse(item.InternalMarks, out decimal _Marks) && _Marks >= 0)
        //                    {
        //                        InternalMarksObtained = InternalMarksObtained.HasValue ? InternalMarksObtained : 0;
        //                        InternalMarksObtained += _Marks;
        //                    }
        //                    else
        //                    {
        //                        InternalMarksTitle += item.InternalMarks + "#";
        //                    }
        //                }
        //            }
        //        }
        //        if (item.IsInternalAttendance_AssessmentApplicable)
        //        {
        //            if (item.InternalAttendanceIsPartOf == MarksIsPartOf.Internal)
        //            {
        //                InternalApplicable = true; InternalMinPassMarks += item.InternalAttendance_AssessmentMinPassMarks; InternalMaxMarks += item.InternalAttendance_AssessmentMaxMarks;
        //                if (item.InternalSubmitted)
        //                {
        //                    if (decimal.TryParse(item.InternalAttendance_AssessmentMarks, out decimal _Marks) && _Marks >= 0)
        //                    {
        //                        InternalMarksObtained = InternalMarksObtained.HasValue ? InternalMarksObtained : 0;
        //                        InternalMarksObtained += _Marks;
        //                    }
        //                    else
        //                    {
        //                        InternalMarksTitle += item.InternalAttendance_AssessmentMarks + "#";
        //                    }
        //                }
        //            }
        //        }
        //        if (item.IsExternalMarksApplicable)
        //        {
        //            if (item.ExternalIsPartOf == MarksIsPartOf.Internal)
        //            {
        //                InternalApplicable = true; InternalMinPassMarks += item.ExternalMinPassMarks; InternalMaxMarks += item.ExternalMaxMarks;
        //                if (item.InternalSubmitted)
        //                {
        //                    if (decimal.TryParse(item.ExternalMarks, out decimal _Marks) && _Marks >= 0)
        //                    {
        //                        InternalMarksObtained = InternalMarksObtained.HasValue ? InternalMarksObtained : 0;
        //                        InternalMarksObtained += _Marks;
        //                    }
        //                    else
        //                    {
        //                        InternalMarksTitle += item.ExternalMarks + "#";
        //                    }
        //                }
        //            }
        //        }
        //        if (item.IsExternalAttendance_AssessmentApplicable)
        //        {
        //            if (item.ExternalAttendanceIsPartOf == MarksIsPartOf.Internal)
        //            {
        //                InternalApplicable = true; InternalMinPassMarks += item.ExternalAttendance_AssessmentMinPassMarks; InternalMaxMarks += item.ExternalAttendance_AssessmentMaxMarks;
        //                if (item.InternalSubmitted)
        //                {
        //                    if (decimal.TryParse(item.ExternalAttendance_AssessmentMarks, out decimal _Marks) && _Marks >= 0)
        //                    {
        //                        InternalMarksObtained = InternalMarksObtained.HasValue ? InternalMarksObtained : 0;
        //                        InternalMarksObtained += _Marks;
        //                    }
        //                    else
        //                    {
        //                        InternalMarksTitle += item.ExternalAttendance_AssessmentMarks + "#";
        //                    }
        //                }
        //            }
        //        }

        //        _ResultStatus += "";
        //        if (InternalApplicable)
        //        {
        //            if (InternalMarksObtained.HasValue)
        //            {
        //                if (InternalMarksObtained < InternalMinPassMarks)
        //                {
        //                    _ResultStatus = WithStatusAsSuffix ? InternalMarksObtained.Value.ToString("f0") + "F" : InternalMarksObtained.Value.ToString("f0");
        //                }
        //                else
        //                {
        //                    _ResultStatus = WithStatusAsSuffix ? InternalMarksObtained.Value.ToString("f0") + "P" : InternalMarksObtained.Value.ToString("f0");
        //                }
        //            }
        //            else
        //            {
        //                _ResultStatus = "N.A";
        //            }
        //        }
        //        else
        //        {
        //            _ResultStatus = "N.A";
        //        }
        //    }
        //    return _ResultStatus;
        //}
        //public string ExternalSubjectMarksWithStatus(SubjectResultCompact item, bool WithStatusAsSuffix = true)
        //{
        //    var _ResultStatus = "";

        //    if (item != null)
        //    {
        //        bool ExternalApplicable = false; decimal ExternalMaxMarks = 0; decimal ExternalMinPassMarks = 0; decimal? ExternalMarksObtained = null; var ExternalMarksTitle = "";

        //        if (item.IsInternalMarksApplicable)
        //        {
        //            if (item.InternalIsPartOf == MarksIsPartOf.External)
        //            {
        //                ExternalApplicable = true; ExternalMinPassMarks += item.InternalMinPassMarks; ExternalMaxMarks += item.InternalMaxMarks;
        //                if (item.ExternalSubmitted)
        //                {
        //                    if (decimal.TryParse(item.InternalMarks, out decimal _Marks) && _Marks >= 0)
        //                    {
        //                        ExternalMarksObtained = ExternalMarksObtained.HasValue ? ExternalMarksObtained : 0;
        //                        ExternalMarksObtained += _Marks;
        //                    }
        //                    else
        //                    {
        //                        ExternalMarksTitle += item.InternalMarks + "#";
        //                    }
        //                }
        //            }
        //        }
        //        if (item.IsInternalAttendance_AssessmentApplicable)
        //        {
        //            if (item.InternalAttendanceIsPartOf == MarksIsPartOf.External)
        //            {
        //                ExternalApplicable = true; ExternalMinPassMarks += item.InternalAttendance_AssessmentMinPassMarks; ExternalMaxMarks += item.InternalAttendance_AssessmentMaxMarks;
        //                if (item.ExternalSubmitted)
        //                {
        //                    if (decimal.TryParse(item.InternalAttendance_AssessmentMarks, out decimal _Marks) && _Marks >= 0)
        //                    {
        //                        ExternalMarksObtained = ExternalMarksObtained.HasValue ? ExternalMarksObtained : 0;
        //                        ExternalMarksObtained += _Marks;
        //                    }
        //                    else
        //                    {
        //                        ExternalMarksTitle += item.InternalAttendance_AssessmentMarks + "#";
        //                    }

        //                }
        //            }
        //        }
        //        if (item.IsExternalMarksApplicable)
        //        {
        //            if (item.ExternalIsPartOf == MarksIsPartOf.External)
        //            {
        //                ExternalApplicable = true; ExternalMinPassMarks += item.ExternalMinPassMarks; ExternalMaxMarks += item.ExternalMaxMarks;
        //                if (item.ExternalSubmitted)
        //                {
        //                    if (decimal.TryParse(item.ExternalMarks, out decimal _Marks) && _Marks >= 0)
        //                    {
        //                        ExternalMarksObtained = ExternalMarksObtained.HasValue ? ExternalMarksObtained : 0;
        //                        ExternalMarksObtained += _Marks;
        //                    }
        //                    else
        //                    {
        //                        ExternalMarksTitle += item.ExternalMarks + "#";
        //                    }

        //                }
        //            }
        //        }
        //        if (item.IsExternalAttendance_AssessmentApplicable)
        //        {
        //            if (item.ExternalAttendanceIsPartOf == MarksIsPartOf.External)
        //            {
        //                ExternalApplicable = true; ExternalMinPassMarks += item.ExternalAttendance_AssessmentMinPassMarks; ExternalMaxMarks += item.ExternalAttendance_AssessmentMaxMarks;
        //                if (item.ExternalSubmitted)
        //                {
        //                    if (decimal.TryParse(item.ExternalAttendance_AssessmentMarks, out decimal _Marks) && _Marks >= 0)
        //                    {
        //                        ExternalMarksObtained = ExternalMarksObtained.HasValue ? ExternalMarksObtained : 0;
        //                        ExternalMarksObtained += _Marks;
        //                    }
        //                    else
        //                    {
        //                        ExternalMarksTitle += item.ExternalAttendance_AssessmentMarks + "#";
        //                    }
        //                }
        //            }
        //        }

        //        _ResultStatus += "";
        //        if (ExternalApplicable)
        //        {
        //            if (ExternalMarksObtained.HasValue)
        //            {
        //                if (ExternalMarksObtained < ExternalMinPassMarks)
        //                {
        //                    _ResultStatus = WithStatusAsSuffix ? ExternalMarksObtained.Value.ToString("f0") + "F" : ExternalMarksObtained.Value.ToString("f0");
        //                }
        //                else
        //                {
        //                    _ResultStatus = WithStatusAsSuffix ? ExternalMarksObtained.Value.ToString("f0") + "P" : ExternalMarksObtained.Value.ToString("f0");
        //                }
        //            }
        //            else
        //            {
        //                _ResultStatus = "N.A";
        //            }
        //        }
        //        else
        //        {
        //            _ResultStatus = "N.A";
        //        }
        //    }
        //    return _ResultStatus;
        //}

        #endregion
        public void FillViewBag_ResultNotifications(Parameters parameters)
        {
            List<SelectListItem> list = new ResultNotificationManager().DDL(parameters) ?? new List<SelectListItem>();
            ViewBag.ResultNotifications = list ?? new List<SelectListItem>();
        }
        public void FillViewBag_ExaminationCourseCategories()
        {
            IEnumerable<SelectListItem> list = Helper.GetSelectList<ExaminationCourseCategory>();
            ViewBag.ExaminationCourseCategories = list ?? new List<SelectListItem>();
        }
        public void FillViewBag_VerificationStatusOfCertificates()
        {
            IEnumerable<SelectListItem> list = Helper.GetSelectList<VerificationStatus>();
            ViewBag.VerificationStatusOfCertificates = list ?? new List<SelectListItem>();
        }
        public void FillViewBag_SelectionStatus()
        {
            IEnumerable<SelectListItem> list = Helper.GetSelectList<StudentSelectionStatus>();
            ViewBag.SelectionStatus = list ?? new List<SelectListItem>();
        }
        public void FillViewBag_ResultAnomaly()
        {
            IEnumerable<SelectListItem> list = Helper.GetSelectList<ResultAnomalyType>();
            ViewBag.ResultAnomalys = list ?? new List<SelectListItem>();
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

        public void ExportToCSV<T>(List<T> list, string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                fileName = "List" + DateTime.Now.ToString();

            string csv = GetCSV(list);
            HttpContext.Response.Clear();
            HttpContext.Response.AddHeader("content-disposition", string.Format("attachment; filename={0}.csv", fileName));
            HttpContext.Response.ContentType = "text/csv";
            HttpContext.Response.AddHeader("CU", "public");
            HttpContext.Response.Write(csv);
            HttpContext.Response.End();
        }

        string GetCSV<T>(List<T> list)
        {
            StringBuilder sb = new StringBuilder();
            //Get the properties for type T for the headers
            PropertyInfo[] propInfos = typeof(T).GetProperties();
            for (int i = 0; i <= propInfos.Length - 1; i++)
            {
                sb.Append(propInfos[i].Name);

                if (i < propInfos.Length - 1)
                {
                    sb.Append(",");
                }
            }
            sb.AppendLine();
            //Loop through the collection, then the properties and add the values
            for (int i = 0; i <= list.Count - 1; i++)
            {
                T item = list[i];
                for (int j = 0; j <= propInfos.Length - 1; j++)
                {
                    object o = item.GetType().GetProperty(propInfos[j].Name).GetValue(item, null);
                    if (o != null)
                    {
                        string value = o.ToString();
                        //Check if the value contans a comma and place it in quotes if so
                        //if (value.Contains(","))
                        //{
                        //    value = string.Concat("\"", value, "\"");
                        //}
                        if (value.Contains(","))
                        {
                            value.Replace("\"", "\"\"");
                            value = string.Concat("\"", value, "\"");
                        }
                        //Replace any \r or \n special characters from a new line with a space
                        if (value.Contains("\r"))
                        {
                            value = value.Replace("\r", " ");
                        }
                        if (value.Contains("\n"))
                        {
                            value = value.Replace("\n", " ");
                        }
                        sb.Append(value);
                    }
                    if (j < propInfos.Length - 1)
                    {
                        sb.Append(",");
                    }
                }
                sb.AppendLine();
            }
            return sb.ToString();
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



        #region FillViewBags
        public void FillViewBag_Programmes(Guid? College_ID)
        {
            List<SelectListItem> list = new CourseManager().GetProgrammes(College_ID);
            ViewBag.Programmes = list ?? new List<SelectListItem>();

        }

        public void FillViewBag_Columns<T>()
        {
            var model = (T)Activator.CreateInstance(typeof(T));
            var Properties = typeof(T).GetProperties();

            List<SelectListItem> Columns = new List<SelectListItem>();
            foreach (var Property in Properties)
            {
                SelectListItem item = new SelectListItem()
                {
                    Text = Property.Name
                };
                var DbColumnAttribute = Property.GetCustomAttributes(typeof(DBColumnNameAttribute), false).Cast<DBColumnNameAttribute>().FirstOrDefault();
                item.Value = DbColumnAttribute == null ? Property.Name : DbColumnAttribute.name;
                Columns.Add(item);
            }
            Columns.RemoveAll(i => i.Text.Contains("ID"));
            ViewBag.Columns = Columns == null ? new List<SelectListItem>() : Columns;
        }
        public void FillViewBag_Columns(Parameters parameter)
        {
            var ColumnsFilter = parameter.Filters.FirstOrDefault(i => i.Column.ToUpper() == "COLUMNS");
            ViewBag.Columns = ColumnsFilter?.Value;
        }

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
        public void FillViewBag_SGPATypes()
        {
            IEnumerable<SelectListItem> list = Helper.GetSelectList<SGPAType>();
            ViewBag.SGPATypes = list ?? new List<SelectListItem>();
        }
        public void FillViewBag_MarksIsPartOfs()
        {
            IEnumerable<SelectListItem> list = Helper.GetSelectList<MarksIsPartOf>();
            ViewBag.MarksIsPartOfs = list ?? new List<SelectListItem>();
        }
        public void FillViewBag_AwardModuleTypes()
        {
            IEnumerable<SelectListItem> list = Helper.GetSelectList<AwardModuleType>();
            ViewBag.AwardModuleTypes = list ?? new List<SelectListItem>();
        }
        public void FillViewBag_Semesters()
        {
            IEnumerable<SelectListItem> list = Helper.GetSelectList<Semester>();
            ViewBag.Semesters = list ?? new List<SelectListItem>();
        }
        public void FillViewBag_Batches()
        {
            IEnumerable<SelectListItem> list = Helper.GetYearsDDL();// new List<SelectListItem>() { new SelectListItem() { Text = "2020", Value = "2020" }, new SelectListItem() { Text = "2019", Value = "2019" }, new SelectListItem() { Text = "2018", Value = "2018" }, new SelectListItem() { Text = "2017", Value = "2017" } };
            ViewBag.Batches = list ?? new List<SelectListItem>();
        }
        public void FillViewBag_Courses(PrintProgramme printProgramme)
        {
            List<SelectListItem> list = new CourseManager().GetAllCoursesByPrintProgramme(printProgramme);
            ViewBag.Courses = list ?? new List<SelectListItem>();
        }
        public void FillViewBag_Courses(Programme Programme)
        {
            List<SelectListItem> list = new CourseManager().GetAllCoursesByProgramme(Programme);
            ViewBag.Courses = list ?? new List<SelectListItem>();
        }
        public void FillViewBag_Course(Parameters parameter)
        {
            List<SelectListItem> list = new CourseManager().SelectList(parameter);
            ViewBag.Courses = list ?? new List<SelectListItem>();
        }

        public void FillViewBag_Departments(Parameters parameter)
        {
            List<SelectListItem> list = new CourseManager().SelectListDept(parameter);
            ViewBag.Departments = list ?? new List<SelectListItem>();
        }
        public void FillViewBag_Course(Programme programme)
        {
            List<SelectListItem> list = new CourseManager().GetAllCoursesByProgramme((int)programme);
            ViewBag.Courses = list ?? new List<SelectListItem>();
        }
        public void FillViewBag_Course(Guid? Course_ID)
        {
            Programme programme = Programme.UG;
            if (Course_ID.HasValue && Course_ID != Guid.Empty)
                programme = new CourseManager().GetCourseById(Course_ID.Value).Programme;
            FillViewBag_Course(programme);
        }
        public void FillViewBag_Departments(string SchoolFullName = null)
        {
            List<SelectListItem> list = new SubjectsManager().DepartmentDataList(SchoolFullName);
            ViewBag.Departments = list ?? new List<SelectListItem>();
        }
        public void FillViewBag_SubjectMarksStructures()
        {
            Parameters parameter = new Parameters() { PageInfo = new Paging() { DefaultOrderByColumn = "TotalCredit,ExternalMaxMarks,InternalMaxMarks,InternalAttendance_AssessmentMaxMarks,ExternalAttendance_AssessmentMaxMarks", PageNumber = -1, PageSize = -1 } };
            List<SelectListItem> list = new SubjectMarksStructureManager().SelectList(parameter);
            ViewBag.SubjectMarksStructures = list ?? new List<SelectListItem>();
        }
        public void FillViewBag_Schools(Guid? College_ID)
        {
            List<SelectListItem> list = new SubjectsManager().SchoolSelectList(College_ID);
            ViewBag.Schools = list ?? new List<SelectListItem>();
        }
        public void FillViewBag_Subjects(Guid Course_Id, Semester semester)
        {
            SelectList list = new SubjectsManager().GetListDDL(Course_Id, (int)semester, true);
            ViewBag.Subjects = list ?? new SelectList(new List<SelectListItem>());
        }
        public void FillViewBag_CombinationSettingStructure(Parameters parameter)
        {
            if (parameter == null)
                parameter = new Parameters() { PageInfo = new Paging() { DefaultOrderByColumn = "Remark" } };
            var list = new CombinationSettingManager().GetCombinationSettingStructureSelectList(parameter);
            ViewBag.CombinationSettingStructure = list ?? new List<SelectListItem>();
        }
        #endregion
    }
}