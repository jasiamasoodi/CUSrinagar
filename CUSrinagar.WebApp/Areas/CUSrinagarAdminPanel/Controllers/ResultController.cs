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
using CUSrinagar.DataManagers;
using System.Web.UI.WebControls;
using System.Reflection;
using System.Security.Cryptography;

namespace CUSrinagar.WebApp.CUSrinagarAdminPanel.Controllers
{
    [OAuthorize(AppRoles.University, AppRoles.University_Evaluator, AppRoles.University_Results, AppRoles.View_TransScript)]
    public class ResultController : AdminController
    {
        #region ResultListSection
        public ActionResult Result()
        {
            FillViewBag_College();
            FillViewBag_Programmes();
            FillViewBag_Course(Programme.UG);
            FillViewBag_Semesters();
            return View();
        }
        public PartialViewResult ResultListPartial(Parameters parameter, PrintProgramme? otherParam1)
        {
            List<ResultCompact> resultlist = null;
            short? sem = null;
            var semester = parameter.Filters.FirstOrDefault(x => x.Column.ToLower() == "semester" && !string.IsNullOrEmpty(x.Value))?.Value;
            if (semester != null) sem = short.Parse(semester);

            resultlist = new ResultManager().Get(otherParam1.Value, sem, parameter, IsEditable: true);
            return PartialView(resultlist);
        }


        [HttpGet]
        public ActionResult PrintMarksSheet(PrintProgramme PrintProgramme, Guid Student_ID, short Semester)
        {

            Parameters parameters = new Parameters() { Filters = new List<SearchFilter>(), PageInfo = new Paging() { DefaultOrderByColumn = "CreatedOn", PageNumber = -1, PageSize = -1 }, SortInfo = new Sort { ColumnName = "CUSRegistrationNo" } };
            parameters.Filters.Add(new SearchFilter() { Column = "Student_ID", Value = Student_ID.ToString(), TableAlias = "S" });
            parameters.Filters.Add(new SearchFilter() { Column = "Semester", Value = Semester.ToString(), TableAlias = "Comb", Operator = SQLOperator.LessThanEqualTo });
            if (Semester > 6)
            {
                parameters.SortInfo = new Sort() { ColumnName = "CreatedOn", OrderBy = System.Data.SqlClient.SortOrder.Descending };
                List<ResultCompact> model = new ResultManager().GetDetails(PrintProgramme, Semester, parameters, true);
                return View("PrintEngMarksSheet", model);
            }
            else
            {
                List<ResultCompact> model = new ResultManager().Get(PrintProgramme, null, parameters, true);
                return View(model);
            }

        }

        #region PrintAllMarksSheets
        [HttpPost]
        public ActionResult PrintAllMarksSheets(Parameters parameter, PrintProgramme? printProgramme)//Download all Provisional Marks-Cards of all Courses
        {
            short? sem = null;
            var semester = parameter.Filters.FirstOrDefault(x => x.Column.ToLower() == "semester" && !string.IsNullOrEmpty(x.Value))?.Value;
            if (semester != null) sem = short.Parse(semester);

            if (parameter.Filters.Any(x => x.Column == "Semester"))
            {
                parameter.Filters.Remove(parameter.Filters.First(x => x.Column == "Semester"));
            }

            List<ResultCompact> model = new List<ResultCompact>();
            if (sem == 6)
            {
                parameter.Filters.Add(new SearchFilter() { Column = "Semester", Value = semester.ToString(), TableAlias = "Comb", Operator = SQLOperator.LessThanEqualTo });
                model = new ResultManager().Get(printProgramme.Value, null, parameter, true);
            }
            return View(model);
        }
        #endregion

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

        //private void CalculateAverage(PrintProgramme printProgramme, short _semester, Parameters parameters)
        //{
        //    parameters.PageInfo = new Paging() { DefaultOrderByColumn = "Student_ID", PageNumber = 1, PageSize = 10 };
        //    List<ResultCompact> collection = new ResultManager().GetResultTest(printProgramme, _semester, parameters);
        //    ResponseData response = new ResponseData();
        //    bool Has3Core = false;
        //    if (parameters.Filters.Any(x => x.Column == "Course_ID" && "03FFF89D-3055-4F0A-A18C-185BEB7512DE,B18A12B2-9025-4057-8E38-DF723649D941,4D8ADDCB-DB0B-4E39-ABC4-AB2FFF15289D".Contains(x.Value.ToUpper())))
        //    {
        //        Has3Core = true;
        //    }
        //    foreach (var item in collection)
        //    {
        //        if (item.SubjectResults.IsNotNullOrEmpty())
        //        {
        //            string Query = "INSERT INTO TempAverageResult(_ID,Student_ID,Semester,Subject_ID,InternalMarksObtained,ExternalMarksObtained,[GROUP]) Values ";
        //            var SubjectQuery = "";

        //            var semSubjects = item.SubjectResults.Where(x => x.Semester == 1 && x.SubjectType == SubjectType.Core).OrderBy(x => x.SubjectFullName).ToList();

        //            Guid core1BaseSubject_ID = semSubjects[0].Subject_ID;
        //            Guid core2BaseSubject_ID = semSubjects[1].Subject_ID;
        //            Guid core3BaseSubject_ID = Guid.Empty;


        //            SubjectQuery += $"(NEWID(),'{item.Student_ID}',1,'{semSubjects[0].Subject_ID}',{(semSubjects[0].TotalInternalMarksObtained.HasValue ? semSubjects[0].TotalInternalMarksObtained.Value : decimal.Zero)},{(semSubjects[0].TotalExternalMarksObtained.HasValue ? semSubjects[0].TotalExternalMarksObtained.Value : decimal.Zero)},1),";
        //            SubjectQuery += $"(NEWID(),'{item.Student_ID}',1,'{semSubjects[1].Subject_ID}',{(semSubjects[1].TotalInternalMarksObtained.HasValue ? semSubjects[1].TotalInternalMarksObtained.Value : decimal.Zero)},{(semSubjects[1].TotalExternalMarksObtained.HasValue ? semSubjects[1].TotalExternalMarksObtained.Value : decimal.Zero)},2),";
        //            if (Has3Core)
        //            {
        //                SubjectQuery += $"(NEWID(),'{item.Student_ID}',1,'{semSubjects[2].Subject_ID}',{(semSubjects[2].TotalInternalMarksObtained.HasValue ? semSubjects[2].TotalInternalMarksObtained.Value : decimal.Zero)},{(semSubjects[2].TotalExternalMarksObtained.HasValue ? semSubjects[2].TotalExternalMarksObtained.Value : decimal.Zero)},3),";
        //                core3BaseSubject_ID = semSubjects[2].Subject_ID;
        //            }

        //            for (int semester = 2; semester <= 6; semester++)
        //            {
        //                var semesterValue = semester;
        //                jump:
        //                //either 2 core or 2
        //                for (int subjectCount = 0; subjectCount < (Has3Core ? 3 : 2); subjectCount++)
        //                {
        //                    var Parameters = new Parameters() { Filters = new List<SearchFilter>() { new SearchFilter() { Column = "BaseSubject_ID", Value = semSubjects[subjectCount].Subject_ID.ToString() } }, PageInfo = new Paging() { DefaultOrderByColumn = "BaseSubject_ID" } };
        //                    var subjectCombinationSettings = new CombinationSettingManager().GetSubjectCombinationSettings(Parameters)?.Where(x => x.DependentSubject_ID != null)?.OrderBy(x => x.DependentSubjectFullName)?.ToList();
        //                    if (subjectCombinationSettings == null)
        //                    {
        //                        semesterValue = semesterValue - 1;
        //                        semSubjects = item.SubjectResults.Where(x => x.Semester == semesterValue && (x.SubjectType == SubjectType.Core || x.SubjectType == SubjectType.DCE || x.SubjectType == SubjectType.DSE)).ToList();
        //                        goto jump;
        //                    }
        //                    var studentSubject = item.SubjectResults.FirstOrDefault(x => x.Semester == semester && subjectCombinationSettings.Any(y => y.DependentSubject_ID == x.Subject_ID));
        //                    if (studentSubject == null)
        //                    {
        //                        studentSubject = item.SubjectResults.FirstOrDefault(x => x.Semester == semester && subjectCombinationSettings.Any(y => y.DependentSubject_ID != null && x.SubjectFullName.Contains(y.BaseSubjectFullName)));
        //                        if (studentSubject == null)
        //                            studentSubject = item.SubjectResults.OrderBy(x => x.SubjectFullName).First();
        //                    }
        //                    if (subjectCombinationSettings.Any(x => x.BaseSubject_ID == core1BaseSubject_ID))
        //                    {

        //                        SubjectQuery += $"(NEWID(),'{item.Student_ID}',{semester},'{studentSubject.Subject_ID}',{(studentSubject.TotalInternalMarksObtained.HasValue ? studentSubject.TotalInternalMarksObtained.Value : decimal.Zero)},{(studentSubject.TotalExternalMarksObtained.HasValue ? studentSubject.TotalExternalMarksObtained.Value : decimal.Zero)},1),";
        //                        if (semester != 5)
        //                            core1BaseSubject_ID = studentSubject.Subject_ID;
        //                    }
        //                    else if (subjectCombinationSettings.Any(x => x.BaseSubject_ID == core2BaseSubject_ID))
        //                    {
        //                        SubjectQuery += $"(NEWID(),'{item.Student_ID}',{semester},'{studentSubject.Subject_ID}',{(studentSubject.TotalInternalMarksObtained.HasValue ? studentSubject.TotalInternalMarksObtained.Value : decimal.Zero)},{(studentSubject.TotalExternalMarksObtained.HasValue ? studentSubject.TotalExternalMarksObtained.Value : decimal.Zero)},2),";
        //                        if (semester != 5)
        //                            core2BaseSubject_ID = studentSubject.Subject_ID;
        //                    }
        //                    else if (subjectCombinationSettings.Any(x => x.BaseSubject_ID == core3BaseSubject_ID))
        //                    {
        //                        SubjectQuery += $"(NEWID(),'{item.Student_ID}',{semester},'{studentSubject.Subject_ID}',{(studentSubject.TotalInternalMarksObtained.HasValue ? studentSubject.TotalInternalMarksObtained.Value : decimal.Zero)},{(studentSubject.TotalExternalMarksObtained.HasValue ? studentSubject.TotalExternalMarksObtained.Value : decimal.Zero)},3),";
        //                        if (semester != 5)
        //                            core3BaseSubject_ID = studentSubject.Subject_ID;
        //                    }
        //                }
        //                semSubjects = item.SubjectResults.Where(x => x.Semester == semester && (x.SubjectType == SubjectType.Core || x.SubjectType == SubjectType.DCE || x.SubjectType == SubjectType.DSE)).OrderBy(x => x.SubjectFullName).ToList();
        //            }
        //            semSubjects = item.SubjectResults.Where(x => x.Semester > 2 && (x.SubjectType == SubjectType.SEC)).ToList();
        //            foreach (var item2 in semSubjects)
        //                SubjectQuery += $"(NEWID(),'{item.Student_ID}',{item2.Semester},'{item2.Subject_ID}',{(item2.TotalInternalMarksObtained.HasValue ? item2.TotalInternalMarksObtained.Value : decimal.Zero)},{(item2.TotalExternalMarksObtained.HasValue ? item2.TotalExternalMarksObtained.Value : decimal.Zero)},9),";

        //            if (!Has3Core)
        //            {
        //                semSubjects = item.SubjectResults.Where(x => x.Semester > 4 && (x.SubjectType == SubjectType.GE || x.SubjectType == SubjectType.OE)).ToList();
        //                foreach (var item2 in semSubjects)
        //                    SubjectQuery += $"(NEWID(),'{item.Student_ID}',{item2.Semester},'{item2.Subject_ID}',{(item2.TotalInternalMarksObtained.HasValue ? item2.TotalInternalMarksObtained.Value : decimal.Zero)},{(item2.TotalExternalMarksObtained.HasValue ? item2.TotalExternalMarksObtained.Value : decimal.Zero)},7),";
        //            }

        //            Query += SubjectQuery.TrimEnd(',');
        //            response.NumberOfRecordsEffected += new ResultManager().InsertTempAverageResult(Query);
        //        }

        //    }

        //}

        //private List<string> ListOfEligibleStudents()
        //{
        //    List<string> listOfEligibleStudents = new List<string>();
        //    #region InBlock
        //    listOfEligibleStudents.Add("CUS-17-GWC-10392");
        //    listOfEligibleStudents.Add("CUS-17-SPC-10638");
        //    #endregion
        //    return listOfEligibleStudents;
        //}

        [OAuthorize(AppRoles.University)]
        [HttpGet]
        public ActionResult Edit(PrintProgramme PrintProgramme, short Semester, Guid Student_ID)
        {
            //var dfd = new ResultManager().PassPercentage(PrintProgramme, Student_ID, Semester, 50, true);

            var studentResult = new ResultManager().GetResult(PrintProgramme, Semester, Student_ID, true);

            foreach (SubjectResult result in studentResult.SubjectResults ?? new List<SubjectResult>())
            {
                result.IsCancelled = new ResultManager().CheckResultCancelled(result._ID, result.ExamForm_ID);
            }
            GetResponseViewBags();

            return View(studentResult);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ResultCompact model)
        {
            ResponseData response = new ResponseData();
            if (ModelState.IsValid)
            {
                response = new ResultManager().SaveUpdate(model);
                if (response.IsSuccess)
                    model = (ResultCompact)response.ResponseObject;
            }
            else
                response.ErrorMessage = string.Join(", ", ModelState.Values.SelectMany(s => s.Errors).Select(y => y.ErrorMessage).ToArray());
            SetResponseViewBags(response);
            //TempData["model"] = model;
            return RedirectToAction("Edit", new { PrintProgramme = model.PrintProgramme, Semester = model.Semester, Student_ID = model.Student_ID });
        }
        [HttpGet]
        public ActionResult ResultAnomaly(Guid Subject_Id, Guid Student_ID, Guid ExamForm_ID, Guid? ResultNotification_ID, Guid _ID, PrintProgramme PrintProgramme, int Semester)
        {
            ResultAnomalies model = new ResultAnomalies()
            {
                Subject_Id = Subject_Id,
                Student_ID = Student_ID,
                ExamForm_Id = ExamForm_ID,
                ResultNotification_Id = ResultNotification_ID,
                Result_Id = _ID,
                PrintProgramme = PrintProgramme,
                Semester = Semester
            };
            FillViewBag_ResultAnomaly();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResultAnomaly(ResultAnomalies model)
        {
            ResponseData response = new ResponseData();

            if (new ResultManager().ResultAnomalyExistsAlready(model.Subject_Id, model.Student_ID, model.ExamForm_Id))
            { response.ErrorMessage = "Anomaly Already exist"; }
            else
            {

                if (ModelState.IsValid)
                {
                    model.ResultAnomalies_Id = Guid.NewGuid();
                    model.CreatedOn = DateTime.Now;
                    if (new ResultManager().SaveAnomaly(model) > 0)
                    {
                        new ResultManager().RemoveResut(model);
                        //model = (ResultAnomalies)response.ResponseObject;
                    }
                }
                else
                { response.ErrorMessage = string.Join(", ", ModelState.Values.SelectMany(s => s.Errors).Select(y => y.ErrorMessage).ToArray()); }
            }
            FillViewBag_ResultAnomaly();
            return RedirectToAction("ResultAnomaly", new { Subject_Id = model.Subject_Id, Student_ID = model.Student_ID, ExamForm_Id = model.ExamForm_Id, ResultNotification_Id = model.ResultNotification_Id, _ID = model.Result_Id, PrintProgramme = model.PrintProgramme, Semester = model.Semester });
        }
        public ActionResult ResultAnomalyList(Parameters parameter)
        {
            if (parameter == null)
            {
                return RedirectToAction("Result");
            }

            List<ResultAnomalies> list = new ResultManager().GetALLResultAnomalies(parameter);
            return View(list);
        }
        #endregion


        //#region SubjectWise
        //public ActionResult SubjectWise()
        //{
        //    FillViewBag_College();
        //    FillViewBag_Programmes();
        //    FillViewBag_Course(Programme.UG);
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

        ////public void ResultCSVSubjectWise(Parameters parameter, PrintProgramme? printProgramme)
        ////{
        ////    short.TryParse(parameter.Filters?.FirstOrDefault(x => x.Column == "Semester")?.Value, out short semester);
        ////    if (semester <= 0 || !printProgramme.HasValue)
        ////        return;
        ////    List<ResultList> list = new ResultManager().List(printProgramme.Value, parameter, semester);
        ////    var showList = list?.Select(x => new
        ////    {
        ////        x.SemesterBatch,
        ////        x.Semester,
        ////        x.CollegeFullName,
        ////        x.CourseFullName,
        ////        x.CUSRegistrationNo,
        ////        x.ExamRollNumber,
        ////        x.FullName,
        ////        x.SubjectFullName,
        ////        SubjectType = x.SubjectType.ToString(),
        ////        ExternalAttendance_AssessmentMarks = x.IsInternalMarksApplicable ? x.InternalMarks.ToString() : "",
        ////        InternalMarks = x.IsInternalMarksApplicable ? x.InternalMarks.ToString() : "",
        ////        InternalAttendance_AssessmentMarks = x.IsInternalAttendance_AssessmentApplicable ? x.InternalAttendance_AssessmentMarks.ToString() : "",
        ////        ExternalMarks = x.IsExternalMarksApplicable ? x.ExternalMarks.ToString() : "",
        ////        x.TotalInternalMaxMarks,
        ////        x.TotalInternalMinPassMarks,
        ////        TotalInternalMarksObtained = (x.TotalInternalMarksObtained.HasValue && (x.InternalStatus == ResultStatus.P || x.InternalStatus == ResultStatus.F)) ? x.TotalInternalMarksObtained.ToString() : "",
        ////        x.TotalExternalMaxMarks,
        ////        x.TotalExternalMinPassMarks,
        ////        TotalExternalMarksObtained = (x.TotalExternalMarksObtained.HasValue && (x.ExternalStatus == ResultStatus.P || x.ExternalStatus == ResultStatus.F)) ? x.TotalExternalMarksObtained.ToString() : "",
        ////        InternalStatus = x.InternalStatus.ToString(),
        ////        ExternalStatus = x.ExternalStatus.ToString(),
        ////        x.TotalMaxMarks,
        ////        ResultStatus = x.OverallResultStatus,
        ////        x.NotificationNo,
        ////        Dated = x.NotificationDate == DateTime.MinValue ? "" : x.NotificationDate.ToString("d")
        ////    })?.ToList();
        ////    if (showList != null)
        ////        ExportToCSV(showList, $"{printProgramme.ToString()}-Sem-{semester}-ResultReport_Subject_Wise{DateTime.Now.ToShortDateString()}");
        ////    else
        ////        ExportToCSV(new List<ResultList>(), $"{printProgramme.ToString()}-Sem-{semester}-ResultReport Subject Wise");
        ////}
        //#endregion

        #region statistics  
        public ActionResult Statistics()
        {
            FillViewBag_College();
            FillViewBag_Programmes();
            FillViewBag_Course(Programme.UG);
            FillViewBag_Semesters();
            FillViewBag_Departments();
            return View();
        }
        public PartialViewResult StatisticsListPartial(Parameters parameter, PrintProgramme? otherParam1)
        {
            ViewBag.StatisticsType = parameter.Filters.First(x => x.Column == "ShowSubjectWiseStatistics").Value;
            parameter.Filters.Remove(parameter.Filters.First(x => x.Column == "ShowSubjectWiseStatistics"));

            short? sem = null;
            var semester = parameter.Filters.FirstOrDefault(x => x.Column.ToLower() == "semester" && !string.IsNullOrEmpty(x.Value))?.Value;
            if (semester != null) sem = short.Parse(semester);
            parameter.PageInfo = new Paging() { PageNumber = -1, PageSize = -1, DefaultOrderByColumn = "Batch" };
            List<ResultCompact> resultlist = new ResultManager().Get(otherParam1.Value, sem, parameter, true, true);

            return PartialView(resultlist);
        }

        //public ActionResult Statistics()
        //{
        //    FillViewBag_College();
        //    FillViewBag_Programmes();
        //    FillViewBag_Course(Programme.UG);
        //    FillViewBag_Semesters();
        //    ViewData["PageSize"] = -1;
        //    return View();

        //}
        //[HttpPost]
        //public ActionResult StatisticsListPartial(Parameters parameter, PrintProgramme? otherParam1)
        //{
        //    parameter.PageInfo = new Paging() { DefaultOrderByColumn = "CUSRegistrationNo", PageNumber = -1, PageSize = -1 };
        //    List<ResultCompact> list = new ResultManager().Get(otherParam1.Value, null, parameter, false, true);
        //    return PartialView(list);
        //}

        //public ActionResult CourseWiseStatistics()
        //{
        //    FillViewBag_College();
        //    FillViewBag_Programmes();
        //    FillViewBag_Course(Programme.UG);
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
        //    List<ResultList> list = new ResultManager().List(otherParam1.Value, parameter, semester);
        //    return PartialView(list);

        //}

        //public void CSVCourseWiseStatistics(Parameters parameter, PrintProgramme? printProgramme)
        //{
        //    short.TryParse(parameter.Filters?.FirstOrDefault(x => x.Column == "Semester")?.Value, out short semester);
        //    parameter.PageInfo = new Paging() { DefaultOrderByColumn = "ExamRollNumber", PageNumber = -1, PageSize = -1 };
        //    List<ResultList> list = new ResultManager().List(printProgramme.Value, parameter, semester);
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
        //    FillViewBag_College();
        //    FillViewBag_Programmes();
        //    FillViewBag_Course(Programme.UG);
        //    FillViewBag_Semesters();
        //    ViewData["PageSize"] = -1;
        //    return View();

        //}

        //public ActionResult SubjectWiseStatisticsListPartial(Parameters parameter, PrintProgramme? otherParam1)
        //{
        //    short.TryParse(parameter.Filters?.FirstOrDefault(x => x.Column == "Semester")?.Value, out short semester);
        //    if (semester <= 0 || !otherParam1.HasValue)
        //        return null;
        //    List<ResultList> list = new ResultManager().List(otherParam1.Value, parameter, semester);
        //    return PartialView(list);
        //}
        //public void CSVSubjectWiseStatistics(Parameters parameter, PrintProgramme? printProgramme)
        //{
        //    short.TryParse(parameter.Filters?.FirstOrDefault(x => x.Column == "Semester")?.Value, out short semester);
        //    if (semester <= 0 || !printProgramme.HasValue)
        //        return;
        //    List<ResultList> list = new ResultManager().List(printProgramme.Value, parameter, semester);

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

        #region CreateResult
        public ActionResult CreateResult()
        {
            FillViewBag_College();
            FillViewBag_Programmes();
            FillViewBag_Course(Programme.UG);
            FillViewBag_Semesters();
            return View();
        }
        public PartialViewResult CreateResultListPartial(Parameters parameter, PrintProgramme? otherParam1)
        {
            short.TryParse(parameter.Filters?.FirstOrDefault(x => x.Column == "Semester")?.Value, out short semester);
            List<ResultCompact> resultlist = null;
            if (semester > 0 && otherParam1.HasValue)
            {
                resultlist = new ResultManager().Get(otherParam1.Value, semester, parameter);
            }
            return PartialView(resultlist);
        }

        [OAuthorize(AppRoles.University, AppRoles.RapidEntry)]
        public ActionResult RapidEntry()
        {
            FillViewBag_College();
            FillViewBag_Programmes();
            FillViewBag_Course(Programme.UG);
            FillViewBag_Semesters();
            ViewBag.Batches = Helper.GetYearsDDL().OrderByDescending(x => x.Value);
            return View();
        }
        public PartialViewResult RapidEntryListPartial(Parameters parameter, PrintProgramme? otherParam1)
        {
            short.TryParse(parameter.Filters?.FirstOrDefault(x => x.Column == "Semester")?.Value, out short semester);
            if (semester <= 0 || !otherParam1.HasValue)
                return null;
            List<ResultList> list = new ResultManager().List(otherParam1.Value, parameter, semester, true, IsPassOut: true);
            return PartialView(list);
        }

        public void RapidEntryListCSV(Parameters parameter, PrintProgramme? printProgramme)
        {
            short.TryParse(parameter.Filters?.FirstOrDefault(x => x.Column == "Semester")?.Value, out short semester);
            if (semester <= 0 || !printProgramme.HasValue)
                return;
            List<ResultList> list = new ResultManager().List(printProgramme.Value, parameter, semester, true);
            var showList = list?.Select(x => new
            {
                x.SemesterBatch,
                x.ClassRollNo,
                x.Semester,
                x.CollegeFullName,
                x.CourseFullName,
                x.CUSRegistrationNo,
                x.ExamRollNumber,
                x.FullName,
                x.SubjectFullName,
                SubjectType = x.SubjectType.ToString(),
                x.ExternalAttendance_AssessmentMarks,
                x.InternalMarks,
                x.InternalAttendance_AssessmentMarks,
                x.ExternalMarks,
                x.TotalInternalMaxMarks,
                x.TotalInternalMinPassMarks,
                x.TotalInternalMarksObtained,
                x.TotalExternalMaxMarks,
                x.TotalExternalMinPassMarks,
                x.TotalExternalMarksObtained,
                x.TotalMaxMarks,
                x.TotalMarksObtained,
                ResultStatus = x.OverallResultStatus,
                x.NotificationNo,
                Dated = x.NotificationDate == DateTime.MinValue ? "" : x.NotificationDate.Value.ToString("d")
            })?.ToList();
            if (showList != null)
                ExportToCSV(showList, $"{printProgramme.ToString()}-Sem-{semester}-ResultReport_Subject_Wise{DateTime.Now.ToShortDateString()}");
            else
                ExportToCSV(new List<ResultList>(), $"{printProgramme.ToString()}-Sem-{semester}-ResultReport Subject Wise");
        }

        [HttpPost]
        public JsonResult PostResultListItem(ResultList model)
        {
            ResponseData response = new ResponseData();
            if (ModelState.IsValid)
                response = new ResultManager().SaveUpdate(model);
            else
                response.ErrorMessage = string.Join(", ", ModelState.Values.SelectMany(s => s.Errors).Select(y => y.ErrorMessage).ToArray());
            return Json(response);
        }


        [HttpPost]
        public JsonResult PostRevaluationListItem(ResultList model)
        {
            ResponseData response = new ResponseData();
            if (ModelState.IsValid)
                response = new ResultManager().RevaluationSaveUpdate(model);
            else
                response.ErrorMessage = string.Join(", ", ModelState.Values.SelectMany(s => s.Errors).Select(y => y.ErrorMessage).ToArray());
            return Json(response);
        }





        #region IrolXL
        //[HttpPost]
        //public FilePathResult FromFileUpdator(ResultFile model)
        //{
        //    ResponseData response = new ResponseData();
        //    HttpPostedFileBase File = model.File;
        //    string filePath = Server.MapPath("~/FolderManager/Xerox/") + File.FileName;
        //    if (System.IO.File.Exists(filePath))
        //        System.IO.File.Delete(filePath);
        //    File.SaveAs(filePath);

        //    WorkBook workbook = WorkBook.Load(filePath);
        //    WorkSheet sheet = workbook.DefaultWorkSheet;

        //    Tuple<bool, string> ValidateRow = ValidateColumns(sheet.Rows.FirstOrDefault());
        //    if (!ValidateRow.Item1)
        //    {
        //        System.IO.File.Delete(filePath);
        //        filePath = CreateErrorFile(ValidateRow.Item2);
        //        return new FilePathResult(filePath, "text/plain");
        //    }
        //    for (var i = 1; i < sheet.RowCount; i++)
        //    {
        //        Tuple<bool, string, ResultList> itemResponse = FillResultItem(sheet.Rows[i], model);
        //        if (!itemResponse.Item1)
        //        {
        //            sheet[$"J{i + 1}"].Value = itemResponse.Item2;
        //            continue;
        //        }
        //        response = new ResponseData();// new ResultManager().SaveUpdate(itemResponse.Item3);
        //        if (response.IsSuccess)
        //            sheet[$"J{i + 1}"].Value = "Done";
        //        else
        //            sheet[$"J{i + 1}"].Value = response.ErrorMessage;
        //    }

        //    System.IO.File.Delete(filePath);
        //    workbook.SaveAs(filePath);
        //    new System.Threading.Tasks.TaskFactory().StartNew(() =>
        //    {
        //        System.Threading.Thread.Sleep(540000);
        //        System.IO.File.Delete(filePath);
        //    });
        //    return new FilePathResult(filePath, File.ContentType);
        //}

        //private string CreateErrorFile(string errorMessage)
        //{
        //    string fileName = Server.MapPath("~/FolderManager/Xerox/file-error.txt");
        //    if (System.IO.File.Exists(fileName))
        //        System.IO.File.Delete(fileName);

        //    // Create a new file     
        //    using (StreamWriter sw = System.IO.File.CreateText(fileName))
        //    {
        //        foreach (var item in errorMessage.Split('#'))
        //        {
        //            sw.WriteLine(item);
        //        }
        //    }
        //    return fileName;
        //}

        //private Tuple<bool, string, ResultList> FillResultItem(RangeRow rangeRow, ResultFile resultFile)
        //{
        //    bool IsValid = true;
        //    string ErrorMessage = "";
        //    ResultList item = new ResultList();

        //    if (rangeRow.IsNullOrEmpty() || rangeRow.Columns.IsNullOrEmpty() || rangeRow.Columns.Length < 10)
        //        return new Tuple<bool, string, ResultList>(false, "Miss match in number of rows", null);
        //    var Columns = rangeRow.Columns.Select(x => x.StringValue.ToLower()).ToList();
        //    if (!string.IsNullOrEmpty(Columns[9]) && Columns[9].Contains("done"))
        //        return new Tuple<bool, string, ResultList>(false, "Done", null);
        //    item.CUSRegistrationNo = Columns[1];
        //    item.ExamRollNumber = Columns[2];
        //    item.ClassRollNo = Columns[3];
        //    item.SubjectFullName = Columns[8];
        //    Parameters parameter = new Parameters()
        //    {
        //        Filters = new List<SearchFilter>(),
        //        PageInfo = new Paging() { DefaultOrderByColumn = "ExamRollNumber", PageNumber = -1, PageSize = -1 }
        //    };
        //    if (!string.IsNullOrEmpty(item.CUSRegistrationNo))
        //        parameter.Filters.Add(new SearchFilter() { Column = "CUSRegistrationNo", Value = item.CUSRegistrationNo.Trim() });
        //    if (!string.IsNullOrEmpty(item.ExamRollNumber))
        //        parameter.Filters.Add(new SearchFilter() { Column = "ExamRollNumber", Value = item.ExamRollNumber.Trim() });
        //    if (!string.IsNullOrEmpty(item.ClassRollNo))
        //        parameter.Filters.Add(new SearchFilter() { Column = "ClassRollNo", Value = item.ClassRollNo.Trim() });
        //    if (resultFile.FileOfSubject_ID.HasValue == false && !string.IsNullOrEmpty(item.SubjectFullName.Trim()))
        //        parameter.Filters.Add(new SearchFilter() { Column = "SubjectFullName", Value = item.SubjectFullName.Trim(), Operator = SQLOperator.Contains });
        //    if (resultFile.FileOfBatch.HasValue)
        //        parameter.Filters.Add(new SearchFilter() { Column = "Batch", Value = resultFile.FileOfBatch.Value.ToString().Trim(), TableAlias = "STDINFO" });
        //    if (resultFile.FileOfCollege_ID.HasValue)
        //        parameter.Filters.Add(new SearchFilter() { Column = "AcceptCollege_ID", Value = resultFile.FileOfCollege_ID.Value.ToString() });
        //    if (resultFile.FileOfCourse_ID.HasValue)
        //        parameter.Filters.Add(new SearchFilter() { Column = "Course_ID", Value = resultFile.FileOfCourse_ID.Value.ToString(), TableAlias = "VWCombinationMaster" });
        //    if (resultFile.FileOfSubject_ID.HasValue)
        //        parameter.Filters.Add(new SearchFilter() { Column = "Subject_ID", Value = resultFile.FileOfSubject_ID.Value.ToString(), TableAlias = "VWCombinationMaster" });

        //    List<ResultList> list = parameter.Filters.IsNullOrEmpty() ? null : new List<ResultList>();// new ResultManager().GetResultSubjectWize(resultFile.FileOfPrintProgramme, parameter);
        //    if (list.IsNullOrEmpty())
        //        ErrorMessage = "No record found";
        //    else if (list.Count != 1)
        //        ErrorMessage = "More than 1 records mapped";

        //    if (!string.IsNullOrEmpty(ErrorMessage))
        //    {
        //        return new Tuple<bool, string, ResultList>(false, ErrorMessage, null);
        //    }
        //    item = list.First();
        //    //if (resultFile.UpdateField == "External")
        //    //{
        //    //    item.ExternalMarks = Columns[4];
        //    //}
        //    //else if (resultFile.UpdateField == "Internal")
        //    //{
        //    //    item.InternalMarks = Columns[5];
        //    //    item.InternalAttendance_AssessmentMarks = Columns[6];
        //    //    item.ExternalAttendance_AssessmentMarks = Columns[7];
        //    //}
        //    //else if (resultFile.UpdateField == "Both")
        //    //{
        //    //    item.ExternalMarks = Columns[4];
        //    //    item.InternalMarks = Columns[5];
        //    //    item.InternalAttendance_AssessmentMarks = Columns[6];
        //    //    item.ExternalAttendance_AssessmentMarks = Columns[7];
        //    //}
        //    item.RecordState = RecordState.Dirty;
        //    return new Tuple<bool, string, ResultList>(IsValid, ErrorMessage, item);
        //}

        //private Tuple<bool, string> ValidateColumns(RangeRow rangeRow)
        //{
        //    try
        //    {
        //        string InvalidColumns = string.Empty;
        //        if (rangeRow.IsNullOrEmpty() || rangeRow.Columns.IsNullOrEmpty() || rangeRow.Columns.Length < 10)
        //            return new Tuple<bool, string>(false, "Miss match in number of columns. Please check header row of the sheet");
        //        var Columns = rangeRow.Columns.Select(x => ((string)x.Value).ToLower()).ToList();
        //        if (Columns[0] != "s.no") InvalidColumns += $"S.NO not {Columns[0]}#";
        //        if (Columns[1] != "cusregistrationno") InvalidColumns += $"CUSRegistrationNo not {Columns[1]}#";
        //        if (Columns[2] != "examrollno") InvalidColumns += $"ExamRollNo not {Columns[2]}#";
        //        if (Columns[3] != "classrollno") InvalidColumns += $"ClassRollNo not {Columns[3]}#";
        //        if (Columns[4] != "externalmarks") InvalidColumns += $"ExternalMarks not {Columns[4]}#";
        //        if (Columns[5] != "internalmarks") InvalidColumns += $"InternalMarks not {Columns[5]}#";
        //        if (Columns[6] != "internalattend_assesment") InvalidColumns += $"InternalAttend_Assesment not {Columns[6]}#";
        //        if (Columns[7] != "externalattend_assesment") InvalidColumns += $"ExternalAttend_Assesment not {Columns[7]}#";
        //        if (Columns[8] != "subjectfullname") InvalidColumns += $"SubjectFullName not {Columns[8]}#";
        //        if (Columns[9] != "status") InvalidColumns += $"Status not {Columns[9]}";
        //        var IsValidRow = string.IsNullOrEmpty(InvalidColumns);
        //        return new Tuple<bool, string>(IsValidRow, InvalidColumns);
        //    }
        //    catch (Exception e)
        //    {
        //        return new Tuple<bool, string>(false, e.Message);
        //    }
        //}


        #endregion

        //[HttpPost]
        //public FilePathResult FromFileUpdator(ResultFile model)
        //{
        //    ResponseData response = new ResponseData();
        //    Microsoft.Office.Interop.Excel.Application xlApp = null;
        //    Microsoft.Office.Interop.Excel.Workbook xlWorkbook = null;
        //    Microsoft.Office.Interop.Excel._Worksheet xlWorksheet = null;
        //    Microsoft.Office.Interop.Excel.Range xlRange = null;
        //    string filePath = "";
        //    HttpPostedFileBase _File = model.File;
        //    try
        //    {
        //        filePath = Server.MapPath("~/FolderManager/Xerox/") + _File.FileName;
        //        if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
        //        _File.SaveAs(filePath);

        //        xlApp = new Microsoft.Office.Interop.Excel.Application();
        //        xlWorkbook = xlApp.Workbooks.Open(filePath);
        //        xlWorksheet = xlWorkbook.Sheets[1];
        //        xlRange = xlWorksheet.UsedRange;

        //        //Microsoft.Office.Interop.Excel.Range excelRange = xlWorksheet.UsedRange;
        //        //object[,] valueArray = (object[,])excelRange.get_Value(
        //        //    Microsoft.Office.Interop.Excel.XlRangeValueDataType.xlRangeValueDefault);

        //        //dkdkfkd(valueArray[1])
        //        //ws.Cells.set_Item(rowsOffset - 2, colsOffset - 1, "PARAMETERLIST");
        //        // excelSheetAll.Cells[row, 1] = "Process Name";


        //        Tuple<bool, string> ValidateRow = ValidateColumns(xlRange.Rows[1]);
        //        if (!ValidateRow.Item1)
        //        {
        //            System.IO.File.Delete(filePath);
        //            filePath = CreateErrorFile(ValidateRow.Item2);
        //            return new FilePathResult(filePath, "text/plain");

        //        }
        //        for (int i = 2; i <= xlRange.Rows.Count; i++)
        //        {
        //            Microsoft.Office.Interop.Excel.Range _row = xlRange.Rows[i];
        //            Tuple<bool, string, ResultList> itemResponse = FillResultItem(xlRange.Rows[i], model);
        //            if (!itemResponse.Item1)
        //            {
        //                xlWorksheet.Cells[$"{i},10"].Value = itemResponse.Item2;
        //                continue;
        //            }
        //            response = new ResultManager().SaveUpdate(itemResponse.Item3);
        //            if (response.IsSuccess)
        //                xlWorksheet.Cells[$"{i},10"].Value = "Done";
        //            else
        //                xlWorksheet.Cells[$"{i},10"].Value = response.ErrorMessage;
        //        }
        //    }
        //    catch (Exception)
        //    {

        //    }
        //    finally
        //    {
        //        GC.Collect();
        //        GC.WaitForPendingFinalizers();

        //        //rule of thumb for releasing com objects:
        //        //  never use two dots, all COM objects must be referenced and released individually
        //        //  ex: [somthing].[something].[something] is bad

        //        //release com objects to fully kill excel process from running in the background
        //        System.Runtime.InteropServices.Marshal.ReleaseComObject(xlRange);
        //        System.Runtime.InteropServices.Marshal.ReleaseComObject(xlWorksheet);

        //        //close and release
        //        xlWorkbook.Close();
        //        System.Runtime.InteropServices.Marshal.ReleaseComObject(xlWorkbook);

        //        //quit and release
        //        xlApp.Quit();
        //        System.Runtime.InteropServices.Marshal.ReleaseComObject(xlApp);
        //        Dispose(true);
        //        new System.Threading.Tasks.TaskFactory().StartNew(() =>
        //        {
        //            System.Threading.Thread.Sleep(540000);
        //            System.IO.File.Delete(filePath);
        //        });
        //    }


        //    //cleanup



        //    //WorkBook workbook = WorkBook.Load(filePath);
        //    //WorkSheet sheet = workbook.DefaultWorkSheet;

        //    //Tuple<bool, string> ValidateRow = ValidateColumns(sheet.Rows.FirstOrDefault());
        //    //if (!ValidateRow.Item1)
        //    //{
        //    //    System.IO.File.Delete(filePath);
        //    //    filePath = CreateErrorFile(ValidateRow.Item2);
        //    //    return new FilePathResult(filePath, "text/plain");

        //    //}
        //    //for (var i = 1; i < sheet.RowCount; i++)
        //    //{
        //    //    Tuple<bool, string, ResultList> itemResponse = FillResultItem(sheet.Rows[i], model);
        //    //    if (!itemResponse.Item1)
        //    //    {
        //    //        sheet[$"J{i + 1}"].Value = itemResponse.Item2;
        //    //        continue;
        //    //    }
        //    //    response = new ResultManager().SaveUpdate(itemResponse.Item3);
        //    //    if (response.IsSuccess)
        //    //        sheet[$"J{i + 1}"].Value = "Done";
        //    //    else
        //    //        sheet[$"J{i + 1}"].Value = response.ErrorMessage;
        //    //}

        //    //System.IO.File.Delete(filePath);
        //    //workbook.SaveAs(filePath);
        //    //new System.Threading.Tasks.TaskFactory().StartNew(() =>
        //    //{
        //    //    System.Threading.Thread.Sleep(540000);
        //    //    System.IO.File.Delete(filePath);
        //    //});
        //    return new FilePathResult(filePath, _File.ContentType);
        //}

        //private string CreateErrorFile(string errorMessage)
        //{
        //    string fileName = Server.MapPath("~/FolderManager/Xerox/file-error.txt");
        //    if (System.IO.File.Exists(fileName))
        //        System.IO.File.Delete(fileName);

        //    // Create a new file     
        //    using (StreamWriter sw = System.IO.File.CreateText(fileName))
        //    {
        //        foreach (var item in errorMessage.Split('#'))
        //        {
        //            sw.WriteLine(item);
        //        }
        //    }
        //    return fileName;
        //}

        //private Tuple<bool, string, ResultList> FillResultItem(Microsoft.Office.Interop.Excel.Range _rangeRow, ResultFile resultFile)
        //{
        //    bool IsValid = true;
        //    string ErrorMessage = "";
        //    ResultList item = new ResultList();

        //    if (_rangeRow == null || _rangeRow.Columns.Count < 10)
        //        return new Tuple<bool, string, ResultList>(false, "Miss match in number of rows", null);

        //    if (_rangeRow.Cells[1, 1].Value2 != null && _rangeRow.Cells[1, 1].Value2.ToString().ToLower().Contains("done"))
        //        return new Tuple<bool, string, ResultList>(false, "Done", null);
        //    item.CUSRegistrationNo = _rangeRow.Cells[1, 2].Value2;
        //    item.ExamRollNumber = _rangeRow.Cells[1, 3].Value2;
        //    item.ClassRollNo = _rangeRow.Cells[1, 4].Value2;
        //    item.SubjectFullName = _rangeRow.Cells[1, 9].Value2;
        //    Parameters parameter = new Parameters()
        //    {
        //        Filters = new List<SearchFilter>(),
        //        PageInfo = new Paging() { DefaultOrderByColumn = "ExamRollNumber", PageNumber = -1, PageSize = -1 }
        //    };
        //    if (!string.IsNullOrEmpty(item.CUSRegistrationNo))
        //        parameter.Filters.Add(new SearchFilter() { Column = "CUSRegistrationNo", Value = item.CUSRegistrationNo.Trim() });
        //    if (!string.IsNullOrEmpty(item.ExamRollNumber))
        //        parameter.Filters.Add(new SearchFilter() { Column = "ExamRollNumber", Value = item.ExamRollNumber.Trim() });
        //    if (!string.IsNullOrEmpty(item.ClassRollNo))
        //        parameter.Filters.Add(new SearchFilter() { Column = "ClassRollNo", Value = item.ClassRollNo.Trim() });
        //    if (resultFile.FileOfSubject_ID.HasValue == false && !string.IsNullOrEmpty(item.SubjectFullName.Trim()))
        //        parameter.Filters.Add(new SearchFilter() { Column = "SubjectFullName", Value = item.SubjectFullName.Trim(), Operator = SQLOperator.Contains });
        //    if (resultFile.FileOfBatch.HasValue)
        //        parameter.Filters.Add(new SearchFilter() { Column = "Batch", Value = resultFile.FileOfBatch.Value.ToString().Trim(), TableAlias = "STDINFO" });
        //    if (resultFile.FileOfCollege_ID.HasValue)
        //        parameter.Filters.Add(new SearchFilter() { Column = "AcceptCollege_ID", Value = resultFile.FileOfCollege_ID.Value.ToString() });
        //    if (resultFile.FileOfCourse_ID.HasValue)
        //        parameter.Filters.Add(new SearchFilter() { Column = "Course_ID", Value = resultFile.FileOfCourse_ID.Value.ToString(), TableAlias = "VWCombinationMaster" });
        //    if (resultFile.FileOfSubject_ID.HasValue)
        //        parameter.Filters.Add(new SearchFilter() { Column = "Subject_ID", Value = resultFile.FileOfSubject_ID.Value.ToString(), TableAlias = "VWCombinationMaster" });

        //    List<ResultList> list = parameter.Filters.IsNullOrEmpty() ? null : new ResultManager().SubjectWizeEditable(resultFile.FileOfPrintProgramme, resultFile.FileOfSemester, parameter);
        //    if (list.IsNullOrEmpty())
        //        ErrorMessage = "No record found";
        //    else if (list.Count != 1)
        //        ErrorMessage = "More than 1 records mapped";

        //    if (!string.IsNullOrEmpty(ErrorMessage))
        //    {
        //        return new Tuple<bool, string, ResultList>(false, ErrorMessage, null);
        //    }
        //    item = list.First();
        //    if (resultFile.UpdateField == "External")
        //    {
        //        item.ExternalMarks = _rangeRow.Cells[1, 5].Value2 != null && !string.IsNullOrEmpty((string)_rangeRow.Cells[1, 5].Value2) ? (string)_rangeRow.Cells[1, 5].Value2 : null;
        //    }
        //    else if (resultFile.UpdateField == "Internal")
        //    {
        //        item.InternalMarks = _rangeRow.Cells[1,6].Value2 != null && !string.IsNullOrEmpty((string)_rangeRow.Cells[1, 6].Value2) ? (string)_rangeRow.Cells[1, 6].Value2 : null;
        //        item.InternalAttendance_AssessmentMarks = _rangeRow.Cells[1, 7].Value2 != null && !string.IsNullOrEmpty((string)_rangeRow.Cells[1, 7].Value2) ? (string)_rangeRow.Cells[1, 7].Value2 : null;
        //        item.ExternalAttendance_AssessmentMarks = _rangeRow.Cells[1, 8].Value2 != null && !string.IsNullOrEmpty((string)_rangeRow.Cells[1, 8].Value2) ? (string)_rangeRow.Cells[1, 8].Value2 : null;
        //    }
        //    else if (resultFile.UpdateField == "Both")
        //    {
        //        item.ExternalMarks = _rangeRow.Cells[1, 5].Value2 != null && !string.IsNullOrEmpty((string)_rangeRow.Cells[1, 5].Value2) ? (string)_rangeRow.Cells[1, 5].Value2 : null;
        //        item.InternalMarks = _rangeRow.Cells[1, 6].Value2 != null && !string.IsNullOrEmpty((string)_rangeRow.Cells[1, 6].Value2) ? (string)_rangeRow.Cells[1, 6].Value2 : null;
        //        item.InternalAttendance_AssessmentMarks = _rangeRow.Cells[1, 7].Value2 != null && !string.IsNullOrEmpty((string)_rangeRow.Cells[1, 7].Value2) ? (string)_rangeRow.Cells[1, 7].Value2 : null;
        //        item.ExternalAttendance_AssessmentMarks = _rangeRow.Cells[1, 8].Value2 != null && !string.IsNullOrEmpty((string)_rangeRow.Cells[1, 8].Value2) ? (string)_rangeRow.Cells[1, 8].Value2 : null;
        //    }
        //    item.RecordState = RecordState.Dirty;
        //    return new Tuple<bool, string, ResultList>(IsValid, ErrorMessage, item);
        //}

        //private Tuple<bool, string> ValidateColumns(Microsoft.Office.Interop.Excel.Range _row)
        //{
        //    try
        //    {
        //        string InvalidColumns = string.Empty;
        //        if (_row == null || _row.Columns.Count < 10)
        //            return new Tuple<bool, string>(false, "Miss match in number of columns. Please check header row of the sheet");

        //        if (_row.Cells[1, 1].Value2 != null && _row.Cells[1, 1].Value2.ToString().ToLower() != "s.no") InvalidColumns += $"S.NO not {_row.Cells[1, 1].Value2}#";
        //        if (_row.Cells[1, 2].Value2 != null && _row.Cells[1, 2].Value2.ToString().ToLower() != "cusregistrationno") InvalidColumns += $"cusregistrationno not {_row.Cells[1, 2].Value2}#";
        //        if (_row.Cells[1, 3].Value2 != null && _row.Cells[1, 3].Value2.ToString().ToLower() != "examrollno") InvalidColumns += $"examrollno not {_row.Cells[1, 3].Value2}#";
        //        if (_row.Cells[1, 4].Value2 != null && _row.Cells[1, 4].Value2.ToString().ToLower() != "classrollno") InvalidColumns += $"classrollno not {_row.Cells[1, 4].Value2}#";
        //        if (_row.Cells[1, 5].Value2 != null && _row.Cells[1, 5].Value2.ToString().ToLower() != "externalmarks") InvalidColumns += $"externalmarks not {_row.Cells[1, 5].Value2}#";
        //        if (_row.Cells[1, 6].Value2 != null && _row.Cells[1, 6].Value2.ToString().ToLower() != "internalmarks") InvalidColumns += $"internalmarks not {_row.Cells[1, 6].Value2}#";
        //        if (_row.Cells[1, 7].Value2 != null && _row.Cells[1, 7].Value2.ToString().ToLower() != "internalattend_assesment") InvalidColumns += $"internalattend_assesment not {_row.Cells[1, 7].Value2}#";
        //        if (_row.Cells[1, 8].Value2 != null && _row.Cells[1, 8].Value2.ToString().ToLower() != "externalattend_assesment") InvalidColumns += $"externalattend_assesment not {_row.Cells[1, 8].Value2}#";
        //        if (_row.Cells[1, 9].Value2 != null && _row.Cells[1, 9].Value2.ToString().ToLower() != "subjectfullname") InvalidColumns += $"subjectfullname not {_row.Cells[1, 9].Value2}#";
        //        if (_row.Cells[1, 10].Value2 != null && _row.Cells[1, 10].Value2.ToString().ToLower() != "status") InvalidColumns += $"status not {_row.Cells[1, 10].Value2}#";
        //        var IsValidRow = string.IsNullOrEmpty(InvalidColumns);
        //        return new Tuple<bool, string>(IsValidRow, InvalidColumns);
        //    }
        //    catch (Exception e)
        //    {
        //        return new Tuple<bool, string>(false, e.Message);
        //    }
        //}

        //public class ResultFile
        //{
        //    public PrintProgramme FileOfPrintProgramme { get; set; }
        //    public short FileOfSemester { get; set; }
        //    public short? FileOfBatch { get; set; }
        //    public Guid? FileOfCollege_ID { get; set; }
        //    public Guid? FileOfCourse_ID { get; set; }
        //    public Guid? FileOfSubject_ID { get; set; }
        //    public string UpdateField { get; set; }
        //    public HttpPostedFileBase File { get; set; }

        //}

        #endregion

        public async Task<ActionResult> UploadExamResult(UploadResult uploadResult, HttpPostedFileBase CSVResultFile)
        {
            //ModelState.Remove(nameof(_RePrintCenterNotice.PipeSeparatedCenters));

            if (!ModelState.IsValid)
            {
                return RedirectToAction("Result");
            }
            Tuple<bool, string> response = await new ResultManager().UploadExamResultAsync(uploadResult, CSVResultFile);
            if (response.Item1)
            {
                TempData["response"] = $"<div class='alert alert-success alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> {response.Item2}</div>";
            }
            else
            {
                TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> {response.Item2}</div>";
            }
            return RedirectToAction("Result");
        }
    }

}
