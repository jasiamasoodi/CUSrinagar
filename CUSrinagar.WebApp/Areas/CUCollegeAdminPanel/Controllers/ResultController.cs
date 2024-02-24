using CUSrinagar.BusinessManagers;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using CUSrinagar.Models;
using CUSrinagar.OAuth;
using GeneralModels;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CUSrinagar.WebApp.CUCollegeAdminPanel.Controllers
{
    [OAuthorize(AppRoles.College)]
    public class ResultController : CollegeAdminBaseController
    {
        public ActionResult Statistics()
        {
            FillViewBag_Programmes(AppUserHelper.College_ID);
            FillViewBag_Course(AppUserHelper.College_ID, null, Programme.UG);
            FillViewBag_Semesters();
            FillViewBag_Departments();
            return View();
        }
        public PartialViewResult StatisticsListPartial(Parameters parameter, PrintProgramme? otherParam1)
        {
            ViewBag.StatisticsType = parameter.Filters.First(x => x.Column == "ShowSubjectWiseStatistics").Value;
            parameter.Filters.Remove(parameter.Filters.First(x => x.Column == "ShowSubjectWiseStatistics"));
            ViewBag.subjectIDSelected = parameter.Filters.FirstOrDefault(x => x.Column == "CombinationSubjects")?.Value ?? Guid.Empty.ToString();

            short? sem = null;
            var semester = parameter.Filters.FirstOrDefault(x => x.Column.ToLower() == "semester" && !string.IsNullOrEmpty(x.Value))?.Value;
            if (semester != null) sem = short.Parse(semester);
            parameter.PageInfo = new Paging() { PageNumber = -1, PageSize = -1, DefaultOrderByColumn = "Batch" };
            List<ResultCompact> resultlist = new ResultManager().Get(otherParam1.Value, sem, parameter, true, true);


            return PartialView(resultlist);
        }

        #region Commented Code
        //public ActionResult Result()
        //{
        //    FillViewBag_Programmes(AppUserHelper.College_ID);
        //    FillViewBag_Course(AppUserHelper.College_ID, null, Programme.UG);
        //    FillViewBag_Semesters();
        //    return View();
        //}
        //public PartialViewResult ResultListPartial(Parameters parameter, PrintProgramme? otherParam1)
        //{
        //    short.TryParse(parameter.Filters?.FirstOrDefault(x => x.Column == "Semester")?.Value, out short semester);
        //    List<ResultCompact> resultlist = null;
        //    if (semester > 0 && otherParam1.HasValue)
        //    {
        //        resultlist = new ResultManager().Get(otherParam1.Value, semester, parameter, false);
        //    }
        //    return PartialView(resultlist);
        //}
        //[HttpGet]
        //public ActionResult PrintMarksSheet(PrintProgramme PrintProgramme, Guid Student_ID)
        //{
        //    Parameters parameters = new Parameters() { Filters = new List<SearchFilter>(), PageInfo = new Paging() { DefaultOrderByColumn = "CUSRegistrationNo", PageNumber = -1, PageSize = -1 }, SortInfo = new Sort { ColumnName = "CUSRegistrationNo" } };
        //    parameters.Filters.Add(new SearchFilter() { Column = "Student_ID", Value = Student_ID.ToString(), TableAlias = "S" });
        //    List<ResultCompact> model = new ResultManager().Get(PrintProgramme, null, parameters);
        //    return View(model);
        //}
        //[HttpPost]
        //public ActionResult PrintMarksSheet(Parameters parameter, PrintProgramme? printProgramme)
        //{
        //    parameter.PageInfo = new Paging() { PageNumber = -1, PageSize = -1, DefaultOrderByColumn = "CUSRegistrationNo" };
        //    List<ResultCompact> model = new ResultManager().Get(printProgramme.Value, null, parameter);
        //    return View(model);
        //}
        //[HttpPost]
        //public ActionResult PrintGazette(Parameters parameter, PrintProgramme? printProgramme, bool printGazetteOnly = false)
        //{
        //    ViewBag.printGazetteOnly = printGazetteOnly;
        //    parameter.PageInfo = new Paging() { PageNumber = -1, PageSize = -1, DefaultOrderByColumn = "CUSRegistrationNo" };
        //    List<ResultCompact> resultlist = new ResultManager().Get(printProgramme.Value, null, parameter, false, true);
        //    return View(resultlist);
        //}
        //public void ResultCSV(Parameters parameter, PrintProgramme? printProgramme)
        //{
        //    short semester = 0;
        //    short.TryParse(parameter.Filters.FirstOrDefault(x => x.Column == "Semester")?.Value, out semester);
        //    List<ResultCompact> listResult = null;
        //    if (semester > 0 && printProgramme.HasValue)
        //    {
        //        listResult = new ResultManager().Get(printProgramme.Value, semester, parameter) ?? new List<ResultCompact>();
        //    }

        //    var list = listResult.Select(
        //        col => new
        //        {
        //            col.Batch,
        //            col.CourseFullName,
        //            col.ClassRollNo,
        //            col.CUSRegistrationNo,
        //            col.ExamRollNumber,
        //            col.FullName,
        //            col.Semester,
        //            Subject = InternalExternalSubjectsWithStatus(col.SubjectResults),
        //            Status = col.SubjectResults.All(x => x.OverallResultStatus == ResultStatus.P) ? "Pass" : "Fail"
        //        }).ToList();
        //    ExportToCSV(list, $"{printProgramme.ToString()}-Sem-{semester}-ResultReport_{DateTime.Now.ToShortDateString()}");
        //}



        //public ActionResult SubjectWise()
        //{
        //    FillViewBag_Programmes(AppUserHelper.College_ID);
        //    FillViewBag_Course(AppUserHelper.College_ID, null, Programme.UG);
        //    FillViewBag_Semesters();
        //    return View();

        //}
        //public ActionResult SubjectWiseListPartial(Parameters parameter, PrintProgramme? otherParam1)
        //{
        //    short.TryParse(parameter.Filters?.FirstOrDefault(x => x.Column == "Semester")?.Value, out short semester);
        //    if (semester <= 0 || !otherParam1.HasValue)
        //        return null;
        //    List<ResultList> list = new ResultManager().List(otherParam1.Value, parameter, semester);
        //    return PartialView(list);
        //}

        //[HttpPost]
        //public ActionResult PrintGazetteSubjectWise(Parameters parameter, PrintProgramme? printProgramme, bool printGazetteOnly = false, bool uptoSemester = false)
        //{
        //    ViewBag.printGazetteOnly = printGazetteOnly;
        //    short.TryParse(parameter.Filters?.FirstOrDefault(x => x.Column == "Semester")?.Value, out short semester);
        //    parameter.PageInfo = new Paging() { PageNumber = -1, PageSize = -1, DefaultOrderByColumn = "CUSRegistrationNo" };
        //    List<ResultList> list = new ResultManager().List(printProgramme.Value, parameter, (uptoSemester ? (short)0 : semester));
        //    return View(list);
        //}



        //[HttpPost]
        //public ActionResult PrintStatistics(Parameters parameter, PrintProgramme? printProgramme)
        //{
        //    parameter.PageInfo = new Paging() { DefaultOrderByColumn = "CUSRegistrationNo", PageNumber = -1, PageSize = -1 };
        //    List<ResultCompact> list = new ResultManager().Get(printProgramme.Value, null, parameter, false, true);
        //    return PartialView(list);
        //}


        //public void ResultCSV(Parameters parameter, PrintProgramme? printProgramme)
        //{
        //    short semester = 0;
        //    short.TryParse(parameter.Filters.FirstOrDefault(x => x.Column == "Semester")?.Value, out semester);
        //    List<ResultCompact> listResult = null;
        //    if (semester > 0 && printProgramme.HasValue)
        //    {
        //        listResult = new ResultManager().Get(printProgramme.Value, semester, parameter) ?? new List<ResultCompact>();
        //    }

        //    var list = listResult.Select(
        //        col => new
        //        {
        //            col.Batch,
        //            col.CourseFullName,
        //            col.ClassRollNo,
        //            col.CUSRegistrationNo,
        //            col.ExamRollNumber,
        //            col.FullName,
        //            col.Semester,
        //            Subject = InternalExternalSubjectsWithStatus(col.SubjectResults),
        //            Status = col.SubjectResults.All(x => x.OverallResultStatus == ResultStatus.P) ? "Pass" : "Fail"
        //        }).ToList();
        //    ExportToCSV(list, $"{printProgramme.ToString()}-Sem-{semester}-ResultReport_{DateTime.Now.ToShortDateString()}");
        //}

        //#region ResultListSection
        //public ActionResult Result()
        //{
        //    FillViewBag_Programmes(AppUserHelper.College_ID);
        //    FillViewBag_Course(AppUserHelper.College_ID, null, Programme.UG);
        //    FillViewBag_Semesters();
        //    return View();
        //}
        //public PartialViewResult ResultListPartial(Parameters parameter, PrintProgramme? otherParam1)
        //{
        //    short.TryParse(parameter.Filters?.FirstOrDefault(x => x.Column == "Semester")?.Value, out short semester);
        //    List<ResultCompact> resultlist = null;
        //    if (semester > 0 && otherParam1.HasValue)
        //    {
        //        resultlist = new ResultManager().GetResult(otherParam1.Value, semester, parameter);
        //    }
        //    return PartialView(resultlist);
        //}

        //public void ResultCSV(Parameters parameter, PrintProgramme? printProgramme)
        //{
        //    short semester = 0;
        //    short.TryParse(parameter.Filters.FirstOrDefault(x => x.Column == "Semester")?.Value, out semester);
        //    List<ResultCompact> listResult = null;
        //    if (semester > 0 && printProgramme.HasValue)
        //    {
        //        listResult = new ResultManager().GetResult(printProgramme.Value, semester, parameter) ?? new List<ResultCompact>();
        //    }

        //    var list = listResult.Select(
        //        col => new
        //        {
        //            col.Batch,
        //            col.CourseFullName,
        //            col.ClassRollNo,
        //            col.CUSRegistrationNo,
        //            col.ExamRollNumber,
        //            col.FullName,
        //            col.Semester,
        //            Subject = InternalExternalSubjectsWithStatus(col.SubjectResults),
        //            Status = col.SubjectResults.IsNotNullOrEmpty() && col.SubjectResults.All(x => x.OverallResultStatus == ResultStatus.P) ? "Pass" : "Fail"
        //        }).ToList();
        //    ExportToCSV(list, $"{printProgramme.ToString()}-Sem-{semester}-ResultReport_{DateTime.Now.ToShortDateString()}");
        //} 
        #endregion




        public ActionResult PrintGazette(Parameters parameter, PrintProgramme? printProgramme)
        {
            short semester = 0;
            short.TryParse(parameter.Filters?.FirstOrDefault(x => x.Column == "Semester")?.Value, out semester);
            List<ResultCompact> resultlist = null;
            if (semester > 0 && printProgramme.HasValue)
            {
                parameter.PageInfo = new Paging() { PageNumber = -1, PageSize = -1, DefaultOrderByColumn = "ExamRollNumber" };
                resultlist = new ResultManager().Get(printProgramme.Value, semester, parameter, false, true);
                var college_id = parameter.Filters?.FirstOrDefault(x => x.Column == "AcceptCollege_ID")?.Value;
                ViewBag.Batch = parameter.Filters?.FirstOrDefault(x => x.Column == "Batch")?.Value;
                if (college_id != null)
                    ViewBag.CollegeFullName = new CollegeManager().GetItem(Guid.Parse(college_id)).CollegeFullName;
                var course_Id = parameter.Filters?.FirstOrDefault(x => x.Column == "Course_ID")?.Value;
                if (course_Id.IsNotNullOrEmpty())
                    ViewBag.CourseFullName = new CourseManager().GetCompactItem(Guid.Parse(course_Id)).CourseFullName;
                ViewBag.Semester = semester;
                ViewBag.PrintProgramme = Helper.GetEnumDescription(printProgramme.Value);
            }
            return View(resultlist);
        }


        public ActionResult PrintFullGazette(Parameters parameter, PrintProgramme? printProgramme)
        {
            short semester = 0;
            short.TryParse(parameter.Filters?.FirstOrDefault(x => x.Column == "Semester")?.Value, out semester);
            List<ResultCompact> resultlist = null;
            if (semester > 0 && printProgramme.HasValue)
            {
                parameter.PageInfo = new Paging() { PageNumber = -1, PageSize = -1, DefaultOrderByColumn = "ExamRollNumber" };
                resultlist = new ResultManager().GetDetailsFullGazette(printProgramme.Value, semester, parameter, false, true);
                var college_id = parameter.Filters?.FirstOrDefault(x => x.Column == "AcceptCollege_ID")?.Value;
                ViewBag.Batch = parameter.Filters?.FirstOrDefault(x => x.Column == "Batch")?.Value;
                if (college_id != null)
                    ViewBag.CollegeFullName = new CollegeManager().GetItem(Guid.Parse(college_id)).CollegeFullName;
                var course_Id = parameter.Filters?.FirstOrDefault(x => x.Column == "Course_ID")?.Value;
                if (course_Id.IsNotNullOrEmpty())
                    ViewBag.CourseFullName = new CourseManager().GetCompactItem(Guid.Parse(course_Id)).CourseFullName;
                ViewBag.Semester = semester;
                ViewBag.PrintProgramme = Helper.GetEnumDescription(printProgramme.Value);
            }
            return View(resultlist);
        }


        #region SubjectWise
        //public ActionResult SubjectWise()
        //{
        //    FillViewBag_Programmes(AppUserHelper.College_ID);
        //    FillViewBag_Course(AppUserHelper.College_ID, null, Programme.UG);
        //    FillViewBag_Semesters();
        //    return View();

        //}
        //public ActionResult SubjectWiseListPartial(Parameters parameter, PrintProgramme? otherParam1)
        //{
        //    short.TryParse(parameter.Filters?.FirstOrDefault(x => x.Column == "Semester")?.Value, out short semester);
        //    if (semester <= 0 || !otherParam1.HasValue)
        //        return null;
        //    List<ResultList> list = new ResultManager().GetResultSubjectWize(otherParam1.Value, parameter);
        //    return PartialView(list);
        //}
        //public void ResultCSVSubjectWise(Parameters parameter, PrintProgramme? printProgramme)
        //{
        //    short.TryParse(parameter.Filters?.FirstOrDefault(x => x.Column == "Semester")?.Value, out short semester);
        //    if (semester <= 0 || !printProgramme.HasValue)
        //        return;
        //    List<ResultList> list = new ResultManager().GetResultSubjectWize(printProgramme.Value, parameter);
        //    var showList = list?.Select(x => new
        //    {
        //        x.SemesterBatch,
        //        x.Semester,
        //        x.CollegeFullName,
        //        x.CourseFullName,
        //        x.CUSRegistrationNo,
        //        x.ExamRollNumber,
        //        x.FullName,
        //        x.SubjectFullName,
        //        SubjectType = x.SubjectType.ToString(),
        //        TotalInternalMarksObtained = (x.HasInternalComponent && (x.InternalStatus == ResultStatus.P || x.InternalStatus == ResultStatus.F)) ? x.TotalInternalMarksObtained.Value.ToString("f0") : "",
        //        TotalExternalMarksObtained = x.HasExternalComponent ? (x.HasInternalComponent ? x.InternalStatus == ResultStatus.P && x.TotalExternalMarksObtained.HasValue
        //                                    && (x.ExternalStatus == ResultStatus.P || x.ExternalStatus == ResultStatus.F) ? x.TotalExternalMarksObtained.Value.ToString("f0") : x.ExternalStatus.ToString()
        //                                    : x.TotalExternalMarksObtained.HasValue && (x.ExternalStatus == ResultStatus.P || x.ExternalStatus == ResultStatus.F) ? x.TotalExternalMarksObtained.Value.ToString("f0") : x.ExternalStatus.ToString()) : "",
        //        InternalStatus = x.InternalStatus.ToString(),
        //        ExternalStatus = x.ExternalStatus.ToString(),
        //        ResultStatus = x.OverallResultStatus,
        //        x.NotificationNo,
        //        Dated = x.NotificationDate == DateTime.MinValue ? "" : x.NotificationDate.ToString("d")
        //    })?.ToList();
        //    if (showList != null)
        //        ExportToCSV(showList, $"{printProgramme.ToString()}-Sem-{semester}-ResultReport_Subject_Wise{DateTime.Now.ToShortDateString()}");
        //    else
        //        ExportToCSV(new List<ResultList>(), $"{printProgramme.ToString()}-Sem-{semester}-ResultReport Subject Wise");
        //}
        #endregion

        #region statistics

        //public ActionResult CourseWiseStatistics()
        //{
        //    FillViewBag_Programmes(AppUserHelper.College_ID);
        //    FillViewBag_Course(AppUserHelper.College_ID, null, Programme.UG);
        //    FillViewBag_Semesters();
        //    ViewData["PageSize"] = -1;
        //    return View();

        //}
        //public ActionResult CourseWiseStatisticsListPartial(Parameters parameter, PrintProgramme? otherParam1)
        //{
        //    short.TryParse(parameter.Filters?.FirstOrDefault(x => x.Column == "Semester")?.Value, out short semester);
        //    if (semester <= 0 || !otherParam1.HasValue)
        //        return null;
        //    parameter.PageInfo = new Paging() { DefaultOrderByColumn = "ExamRollNumber", PageNumber = -1, PageSize = -1 };
        //    List<ResultList> list = new ResultManager().GetResultSubjectWize(otherParam1.Value, parameter);
        //    return PartialView(list);

        //}

        //public void CSVCourseWiseStatistics(Parameters parameter, PrintProgramme? printProgramme)
        //{
        //    short.TryParse(parameter.Filters?.FirstOrDefault(x => x.Column == "Semester")?.Value, out short semester);
        //    parameter.PageInfo = new Paging() { DefaultOrderByColumn = "ExamRollNumber", PageNumber = -1, PageSize = -1 };
        //    List<ResultList> list = new ResultManager().GetResultSubjectWize(printProgramme.Value, parameter);
        //    if (list.IsNotNullOrEmpty())
        //    {
        //        var _list = list.Where(x => x.CourseFullName == "").Select(x => new
        //        {
        //            CourseFullName = "",
        //            Total = 0,
        //            TotalMale = 0,
        //            TotalFemale = 0,
        //            Passed = 0,
        //            MalePassed = 0,
        //            FemalePassed = 0,
        //            Failed = 0,
        //            MaleFailed = 0,
        //            FemaleFailed = 0,
        //            InternalPassed = 0,
        //            InternalFailed = 0,
        //            ExternalPassed = 0,
        //            ExternalFailed = 0
        //        }).ToList();
        //        var _ListOfCourses = list.Select(x => x.Course_ID).Distinct().ToList();
        //        for (int i = 0; i < _ListOfCourses.Count; i++)
        //        {
        //            Guid Course_ID = _ListOfCourses[i];
        //            var _rows = list.Where(x => x.Course_ID == Course_ID && x.HasResult);
        //            var _failedStudents = _rows.Where(x => x.OverallResultStatus != ResultStatus.P).ToList();
        //            var _passedStudents = _rows.Where(x => x.OverallResultStatus == ResultStatus.P && !_failedStudents.Any(y => y.Student_ID == x.Student_ID)).ToList();

        //            var _internalFailed = _rows.Where(x => x.InternalStatus != ResultStatus.P).ToList();
        //            var _internalPassed = _rows.Where(x => x.InternalStatus == ResultStatus.P && !_internalFailed.Any(y => y.Student_ID == x.Student_ID)).ToList();

        //            var _externalFailed = _rows.Where(x => x.ExternalStatus != ResultStatus.P).ToList();
        //            var _externalPassed = _rows.Where(x => x.ExternalStatus == ResultStatus.P && !_externalFailed.Any(y => y.Student_ID == x.Student_ID)).ToList();

        //            _list.Insert(i, new
        //            {
        //                CourseFullName = _rows.First().CourseFullName,
        //                Total = _rows.Select(x => x.Student_ID).Distinct().Count(),
        //                TotalMale = _rows.Where(x => x.Gender.ToLower() == "male").Select(x => x.Student_ID).Distinct().Count(),
        //                TotalFemale = _rows.Where(x => x.Gender.ToLower() == "female").Select(x => x.Student_ID).Distinct().Count(),
        //                Passed = _passedStudents.Select(x => x.Student_ID).Distinct().Count(),
        //                MalePassed = _passedStudents.Where(x => x.Gender.ToLower() == "male").Select(x => x.Student_ID).Distinct().Count(),
        //                FemalePassed = _passedStudents.Where(x => x.Gender.ToLower() == "female").Select(x => x.Student_ID).Distinct().Count(),
        //                Failed = _failedStudents.Select(x => x.Student_ID).Distinct().Count(),
        //                MaleFailed = _failedStudents.Where(x => x.Gender.ToLower() == "male").Select(x => x.Student_ID).Distinct().Count(),
        //                FemaleFailed = _failedStudents.Where(x => x.Gender.ToLower() == "female").Select(x => x.Student_ID).Distinct().Count(),
        //                InternalPassed = _internalPassed.Select(x => x.Student_ID).Distinct().Count(),
        //                InternalFailed = _internalFailed.Select(x => x.Student_ID).Distinct().Count(),
        //                ExternalPassed = _externalPassed.Select(x => x.Student_ID).Distinct().Count(),
        //                ExternalFailed = _externalFailed.Select(x => x.Student_ID).Distinct().Count()
        //            });
        //        }
        //        ExportToCSV(_list, $"{printProgramme.ToString()}-Sem({semester}) ResultReport_{DateTime.Now.ToShortDateString()}");
        //    }
        //    ExportToCSV(new List<ResultList>(), $"{printProgramme.ToString()}-Sem({semester})-ResultReport_{DateTime.Now.ToShortDateString()}");
        //}

        //public ActionResult SubjectWiseStatistics()
        //{
        //    FillViewBag_Programmes(AppUserHelper.College_ID);
        //    FillViewBag_Course(AppUserHelper.College_ID, null, Programme.UG);
        //    FillViewBag_Semesters();
        //    ViewData["PageSize"] = -1;
        //    return View();

        //}
        //public ActionResult SubjectWiseStatisticsListPartial(Parameters parameter, PrintProgramme? otherParam1)
        //{
        //    short.TryParse(parameter.Filters?.FirstOrDefault(x => x.Column == "Semester")?.Value, out short semester);
        //    if (semester <= 0 || !otherParam1.HasValue)
        //        return null;
        //    List<ResultList> list = new ResultManager().GetResultSubjectWize(otherParam1.Value, parameter);
        //    return PartialView(list);
        //}
        //public void CSVSubjectWiseStatistics(Parameters parameter, PrintProgramme? printProgramme)
        //{
        //    short.TryParse(parameter.Filters?.FirstOrDefault(x => x.Column == "Semester")?.Value, out short semester);
        //    if (semester <= 0 || !printProgramme.HasValue)
        //        return;
        //    List<ResultList> list = new ResultManager().GetResultSubjectWize(printProgramme.Value, parameter);

        //    var _ListOfSubjects = list.Where(x => x.HasResult).GroupBy(x => x.Subject_ID).ToList();
        //    var showList = _ListOfSubjects?.Select(x => new
        //    {
        //        x.First().SemesterBatch,
        //        x.First().CourseFullName,
        //        x.First().SubjectFullName,
        //        SubjectType = x.First().SubjectType.ToString(),
        //        Total = x.Count(),
        //        TotalPassed = x.Where(y => y.OverallResultStatus == ResultStatus.P).Count(),
        //        TotalFailed = x.Where(y => y.OverallResultStatus != ResultStatus.P).Count(),

        //        InternalPassed = x.Where(y => y.InternalStatus == ResultStatus.P).Count(),
        //        Internal_NotPassed = x.Where(y => y.InternalStatus != ResultStatus.P).Count(),

        //        ExternalPass = x.Where(y => y.ExternalStatus == ResultStatus.P).Count(),
        //        ExternalNotPassed = x.Where(y => y.ExternalStatus != ResultStatus.P).Count()
        //    })?.ToList();
        //    if (showList != null)
        //        ExportToCSV(showList, $"{printProgramme.ToString()}-Sem({semester}) ResultReport");
        //    else
        //        ExportToCSV(new List<ResultList>(), $"{printProgramme.ToString()}-Sem({semester})-ResultReport_{DateTime.Now.ToShortDateString()}");
        //}
        #endregion


        #region Result Percentage Statistics

        [HttpGet]
        public ActionResult ResultData()
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
        public ActionResult ExportToExcel(Parameters parameter, PrintProgramme printProgramme, bool? value)
        {
            short.TryParse(parameter?.Filters?.FirstOrDefault(x => x.Column == "Semester")?.Value ?? "0", out short semester);
            if (semester <= 0 || printProgramme == 0)
            {
                //Does not return Null         

            }

            int.TryParse(parameter?.Filters?.FirstOrDefault(x => x.Column == "Batch")?.Value ?? "0", out int batch);

            Guid.TryParse(parameter?.Filters?.FirstOrDefault(x => x.Column == "Course_ID")?.Value ?? "0", out Guid CourseId);

            //Guid.TryParse(parameter?.Filters?.FirstOrDefault(x => x.Column == "AcceptCollege_ID")?.Value ?? "0", out Guid AcceptCollege_ID);

            SearchFilter prgFilter = parameter?.Filters?.FirstOrDefault(x => x.Column == "Programme");

            DataTable _DataTable = new ResultManager().ResultDataList(parameter, printProgramme, semester, batch, (Programme)Enum.Parse(typeof(Programme), prgFilter.Value), CourseId, AppUserHelper.College_ID.Value, false);

            if (_DataTable != null)
            {
                return DownloadExcel(_DataTable, "SubjectWiseResultData");
            }
            else
                return DownloadExcel<object>(null, null, null);
        }



        [HttpPost]
        public ActionResult ExportToExcelCourseWise(Parameters parameter, PrintProgramme printProgramme, bool? value)
        {
            short.TryParse(parameter?.Filters?.FirstOrDefault(x => x.Column == "Semester")?.Value ?? "0", out short semester);

            int.TryParse(parameter?.Filters?.FirstOrDefault(x => x.Column == "Batch")?.Value ?? "0", out int batch);

            Guid.TryParse(parameter?.Filters?.FirstOrDefault(x => x.Column == "Course_ID")?.Value ?? "0", out Guid CourseId);

            SearchFilter prgFilter = parameter?.Filters?.FirstOrDefault(x => x.Column == "Programme");

            DataTable _DataTable = new ResultManager().ResultDataList(parameter, printProgramme, semester, batch, (Programme)Enum.Parse(typeof(Programme), prgFilter.Value), CourseId, AppUserHelper.College_ID.Value, true);

            if (_DataTable != null)
            {
                return DownloadExcel(_DataTable, "CourseWiseResultData");
            }
            else
                return DownloadExcel<object>(null, null, null);
        }



        #endregion
    }
}
