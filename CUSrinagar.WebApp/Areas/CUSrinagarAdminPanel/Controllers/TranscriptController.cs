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
using CUSrinagar.DataManagers;
using System.Net.Http.Headers;

namespace CUSrinagar.WebApp.CUSrinagarAdminPanel.Controllers
{

    public class TranscriptController : AdminController
    {
        #region GenerateTranscript

        //[OAuthorize(AppRoles.University)]
        //public ActionResult StudentMarks()
        //{
        //    FillViewBag_PrintProgrammes();
        //    FillViewBag_Courses(PrintProgramme.UG);
        //    return View();
        //}
        //[OAuthorize(AppRoles.University)]
        //public ActionResult GetStudentMarksold(Parameters parameter)
        //{
        //    string RegNo = string.Empty;
        //    if (parameter.Filters.Where(x => x.Column == "CUSRegistrationNo").Count() > 0)
        //    { RegNo = parameter.Filters.Where(x => x.Column == "CUSRegistrationNo").FirstOrDefault().Value; }
        //    parameter.PageInfo.PageNumber = -1;
        //    parameter.PageInfo.PageSize = -1;
        //    GenerateTransscripts(parameter.Filters.Where(x => x.Column == "SemesterBatch").FirstOrDefault().Value, parameter.Filters.Where(x => x.Column == "Course_ID").FirstOrDefault().Value, parameter.Filters.Where(x => x.Column == "PrintProgramme").FirstOrDefault().Value
        //            , RegNo, parameter.Filters.Where(x => x.Column == "HaveGap").FirstOrDefault().Value, parameter.Filters.Where(x => x.Column == "AllowExtraCredits").FirstOrDefault().Value,
        //            parameter.Filters.Where(x => x.Column == "IsLateralEntry").FirstOrDefault().Value, parameter.Filters.Where(x => x.Column == "IsDivisionImprovement").FirstOrDefault().Value);
        //    parameter.PageInfo.PageNumber = 1;
        //    parameter.PageInfo.PageSize = 15;

        //    List<MSStudentMarks> listResultMS = new MarksCardManager().GetStudentMarks(parameter);
        //    return View(listResultMS);
        //}


        //[OAuthorize(AppRoles.University)]
        //public void GenerateTransscripts(string SemesterBatchIs, string course_idIs, string PrintProgrammeIs, string CUSRegistrationNo, string HaveGap, string AllowExtraCredits, string IsLateralEntry, string isDivisionImprovement)
        //{
        //    int Batch = int.Parse(SemesterBatchIs);
        //    Guid course_id = Guid.Parse(course_idIs);
        //    PrintProgramme PrintProgramme = (PrintProgramme)int.Parse(PrintProgrammeIs);
        //    new MarksCardManager().GenerateTransscriptsOld(PrintProgramme, Batch, course_id, CUSRegistrationNo, HaveGap, AllowExtraCredits, IsLateralEntry, isDivisionImprovement);
        //}
        //[OAuthorize(AppRoles.University)]
        //public ActionResult GetStudentMarks(Parameters parameter)
        //{
        //    Guid Course_ID = Guid.Parse(parameter.Filters.Where(x => x.Column == "Course_ID").FirstOrDefault().Value);
        //    bool IsGap = Convert.ToBoolean(parameter.Filters.Where(x => x.Column == "HaveGap").First().Value);
        //    ADMCourseMaster courseMaster = new CourseManager().GetCourseById(Course_ID);
        //    if (courseMaster.CourseFullName.Contains("Integrated") && !IsGap)
        //    { new MarksCardManager().GenerateTransscriptsIG(parameter); }
        //    else
        //    { new MarksCardManager().GenerateTransscripts(parameter); }
        //    List<MSStudentMarks> listResultMS = new MarksCardManager().GetStudentMarks(parameter);
        //    return View(listResultMS);

        //}

        #endregion


        #region TranscriptList
        public void TranscriptCSV(Parameters parameter, PrintProgramme? printProgramme)
        {
            List<Transcript> transcripts = new TranscriptManager().TranscriptList(printProgramme.Value, parameter);
            transcripts = transcripts?.Where(x => x.Subject.IsNotNullOrEmpty() && x.SGPA.IsNotNullOrEmpty() && x.CGPA.IsNotNullOrEmpty())?.ToList() ?? new List<Transcript>() { };
            var list = transcripts.Select(
                col => new
                {
                    col.Batch,
                    col.CGPA.First().MarksSheetNo,
                    col.CollegeFullName,
                    col.CourseFullName,
                    col.CUSRegistrationNo,
                    col.ExamRollNumber,
                    col.FullName,
                    col.FathersName,
                    col.CGPA.First().TotalCreditsEarned,
                    col.CGPA.First().TotalCreditPoints,
                    col.CGPA.First().CGPA,
                    col.CGPA.First().Percentage,
                    DateOfDeclaration = col.CGPA.First().DateofDeclaration.ToString("dd-MMM-yyyy"),
                    col.CGPA.First().NotificationNo,
                    ValidatedOn = col.CGPA.First().IsValidated ? col.CGPA.First().ValidatedOn.Value.ToString("dd-MMM-yyyy") : "Not Validated",
                    PrintedOn = col.CGPA.First().IsPrinted ? col.CGPA.First().PrintedOn.Value.ToString("dd-MMM-yyyy") : "Not Printed",
                }).ToList();
            ExportToCSV(list, $"Transcript_{DateTime.Now.ToString("dd-MMM-yyyy")}");
        }

        public ActionResult NadDegreeCertificateCSV(Parameters parameter, PrintProgramme? printProgramme)
        {
            SearchFilter searchFilterSemesterFrom = parameter.Filters.First(x => x.Column.ToLower().Trim().Contains("semesterfrom"));
            if (searchFilterSemesterFrom != null)
            {
                parameter.Filters.Remove(searchFilterSemesterFrom);
            }

            parameter.Filters.First(x => x.Column.ToLower().Trim().Contains("semesterto")).TableAlias = "DS";

            DataTable _DataTable = new TranscriptManager().NadCertificateData(printProgramme.Value, parameter);
            return DownloadExcel(_DataTable ?? new DataTable(), $"NadDegreeCertificateData_{DateTime.Now.ToString("dd-MMM-yyyy")}");
        }


        [OAuthorize(AppRoles.University_TransScript, AppRoles.University, AppRoles.View_TransScript)]
        public ActionResult PrintTranscript(int? Batch, int? SemesterBatch, int? SemesterTo, PrintProgramme PrintProgramme, Guid? Course_ID, Guid? AcceptCollege_ID, string CUSRegistrationNo
       , string GreaterThanDate, string LessThanDate, short? PrintedOn, short? ValidatedOn, short? HandedOverOn)
        {
            Parameters parameters = new Parameters() { Filters = new List<SearchFilter>(), PageInfo = new Paging() { DefaultOrderByColumn = "ExamRollNumber", PageNumber = -1, PageSize = -1 }, SortInfo = new Sort { ColumnName = "ExamRollNumber" } };
            parameters.Filters.Add(new SearchFilter() { Column = "PrintProgramme", Value = ((short)PrintProgramme).ToString() });
            if (AcceptCollege_ID.HasValue)
                parameters.Filters.Add(new SearchFilter() { Column = "AcceptCollege_ID", Value = AcceptCollege_ID.ToString() });
            if (Course_ID.HasValue)
                parameters.Filters.Add(new SearchFilter() { Column = "Course_ID", Value = Course_ID.ToString(), TableAlias = "VW" });
            if (Batch.HasValue)
                parameters.Filters.Add(new SearchFilter() { Column = "Batch", Value = Batch.ToString() });
            if (SemesterBatch.HasValue)
                parameters.Filters.Add(new SearchFilter() { Column = "SemesterBatch", Value = SemesterBatch.ToString() });
            if (SemesterTo.HasValue)
                parameters.Filters.Add(new SearchFilter() { Column = "SemesterTo", Value = SemesterTo.ToString() });
            if (!string.IsNullOrEmpty(GreaterThanDate) && DateTime.TryParse(GreaterThanDate, out DateTime date1))
                parameters.Filters.Add(new SearchFilter() { Column = "CreatedOn", Value = date1.ToString(), TableAlias = "VW", Operator = SQLOperator.GreaterThanEqualTo });
            if (!string.IsNullOrEmpty(LessThanDate) && DateTime.TryParse(LessThanDate, out DateTime date2))
                parameters.Filters.Add(new SearchFilter() { Column = "CreatedOn", Value = date2.ToString(), TableAlias = "VW", Operator = SQLOperator.LessThanEqualTo });
            if (ValidatedOn.HasValue)
                parameters.Filters.Add(new SearchFilter() { Column = "ValidatedOn", Operator = (ValidatedOn.Value == 1 ? SQLOperator.ISNotNULL : SQLOperator.ISNULL) });
            if (PrintedOn.HasValue)
                parameters.Filters.Add(new SearchFilter() { Column = "PrintedOn", Operator = (PrintedOn.Value == 1 ? SQLOperator.ISNotNULL : SQLOperator.ISNULL) });
            if (HandedOverOn.HasValue)
                parameters.Filters.Add(new SearchFilter() { Column = "ValidatedOn", Operator = (ValidatedOn.Value == 1 ? SQLOperator.ISNotNULL : SQLOperator.ISNULL) });
            if (!string.IsNullOrEmpty(CUSRegistrationNo))
                parameters.Filters.Add(new SearchFilter() { Column = "CUSRegistrationNo", Value = CUSRegistrationNo });
            List<Transcript> transcripts;
            transcripts = new TranscriptManager().TranscriptList(PrintProgramme, parameters);
            if (Course_ID.HasValue || transcripts != null)
            {
                Programme programme = new CourseManager().GetCourseProgramme(Course_ID.HasValue ? Course_ID.Value : transcripts.FirstOrDefault().Course_ID);
                if (programme == Programme.Engineering)
                { return View("PrintTranscript_Eng", transcripts); }
            }

            return View("PrintTranscripts", transcripts);
        }


        #endregion


        #region ProgramStructure
        [OAuthorize(AppRoles.University_TransScript, AppRoles.University, AppRoles.View_TransScript)]
        public ActionResult ProgramStructure()
        {
            return View();
        }

        [OAuthorize(AppRoles.University_TransScript, AppRoles.University, AppRoles.View_TransScript)]
        public PartialViewResult ProgramStructureList(Parameters parameter)
        {
            List<NotificationCompactList> list = new NotificationManager().CompactList(parameter);
            return PartialView(list);
        }
        #endregion

        [OAuthorize(AppRoles.University_TransScript, AppRoles.University)]
        public ActionResult Detail(Guid id)
        {
            var model = new TranscriptManager().TranscriptGeneration(id);
            ViewBag.TranscriptNotification = model;
            DataSet ds = new TranscriptManager().ResultStatisticsDataTable(model.Batch, model.Course_ID);
            return View(ds);
        }


        #region BackUp

        private void RemoveFailedStudents(List<TranscriptModel> listResult)
        {
            var courseMaster = new CourseManager().GetCourseById(listResult.First().Course_ID);
            List<Guid> failedsubjects = new List<Guid>();
            List<Guid> failedstudents = new List<Guid>();
            List<MarksSheet> _listResult = new List<MarksSheet>();
            foreach (var std in listResult)
            {
                var hasFailsubject = false;
                foreach (var item in std.SubjectResults)
                {
                    if (item.OverallResultStatus != ResultStatus.P)
                    {
                        hasFailsubject = true;
                        failedsubjects.Add(item._ID);
                    }
                }
                if (hasFailsubject || std.SubjectResults.Max(x => x.Semester) != courseMaster.Duration)
                    failedstudents.Add(std.Student_ID);
            }
            if (failedstudents.IsNotNullOrEmpty())
                listResult.RemoveAll(x => failedstudents.Contains(x.Student_ID));

            string guidids = string.Join("','", failedsubjects);
            string listofStudents = string.Join("','", listResult.Select(x => x.Student_ID));
        }
        #endregion
        #region Settings
        [OAuthorize(AppRoles.University_TransScript, AppRoles.University)]
        public ActionResult TranscriptDegreeSettingsSearch()
        {
            FillViewBag_Programmes();
            FillViewBag_Courses(PrintProgramme.UG);
            return View();

        }
        [OAuthorize(AppRoles.University_TransScript, AppRoles.University)]
        public ActionResult TranscriptDegreeSettingsList(Parameters parameter)
        {
            List<MSTranscriptDegreeSettings> list = new TranscriptManager().GetSettingsList(parameter);
            return PartialView(list);

        }
        [OAuthorize(AppRoles.University_TransScript, AppRoles.University)]
        public ActionResult AddTranscriptDegreeSettings()
        {
            FillViewBag_Programmes();
            FillViewBag_Courses(PrintProgramme.UG);
            return View("TranscriptDegreeSetting", new MSTranscriptDegreeSettings());
        }
        [OAuthorize(AppRoles.University_TransScript, AppRoles.University)]
        public ActionResult EditTranscriptDegreeSettings(Guid Setting_ID)
        {
            MSTranscriptDegreeSettings model = new TranscriptManager().GetSettings(Setting_ID);
            FillViewBag_Courses(model.Programme);
            FillViewBag_Programmes();
            return View("TranscriptDegreeSetting", model);
        }
        [OAuthorize(AppRoles.University_TransScript, AppRoles.University)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveTranscriptDegreeSettings(MSTranscriptDegreeSettings model)
        {
            if (ModelState.IsValid)
            {
                if (model.Setting_ID == null || model.Setting_ID == Guid.Empty)
                    new TranscriptManager().SaveSettings(model);
                else
                    new TranscriptManager().EditSettings(model);
                return RedirectToAction("TranscriptDegreeSettingsSearch");
            }
            FillViewBag_Programmes();
            FillViewBag_Courses(PrintProgramme.UG);
            return View("TranscriptDegreeSetting", model);


        }
        #endregion
        #region Generate Transcript
        [OAuthorize(AppRoles.University_TransScript, AppRoles.University)]
        public ActionResult SetTranscriptParameter()
        {
            FillViewBag_PrintProgrammes();
            FillViewBag_Courses(PrintProgramme.UG);
            ViewBag.SemestersFrom = new TranscriptManager().GetSemesterFromList() ?? new List<SelectListItem>();
            ViewBag.SemestersTo = new TranscriptManager().GetSemesterToList() ?? new List<SelectListItem>();
            IEnumerable<SelectListItem> list = Helper.GetSelectList<TranscriptFilters>();
            ViewBag.Filters = list ?? new List<SelectListItem>();
            return View();
        }
        [OAuthorize(AppRoles.University_TransScript, AppRoles.University)]
        public ActionResult GenerateTranscripts(Parameters parameter)
        {
            int Batch = Convert.ToInt32(parameter.Filters.Where(x => x.Column == "SemesterBatch").First().Value);
            Tuple<List<Guid>, string> result = Tuple.Create(new List<Guid>(), "Settings Does Not Exist");
            MSTranscriptDegreeSettings settings = new MarksCardManager().GetTranscriptSetting(parameter)?.Where(x => Batch >= x.BatchFrom && Batch <= x.BatchTo)?.FirstOrDefault();
            if (settings != null)
            {
                result = new MarksCardManager().GenerateTranscripts(settings, parameter);
            }
            ViewBag.MessageIs = result.Item2;
            Enum.TryParse(parameter.Filters.Where(x => x.Column == "PrintProgramme").First().Value, out PrintProgramme printProgramme);
            List<Transcript> list = new TranscriptManager().TranscriptList(printProgramme, parameter);
            return View(list);
        }

        [OAuthorize(AppRoles.University_TransScript, AppRoles.University, AppRoles.View_TransScript)]
        public ActionResult TranscriptSearch()
        {
            FillViewBag_College();
            FillViewBag_PrintProgrammes();
            FillViewBag_Course(Programme.UG);
            ViewBag.Batches = Helper.GetYearsDDL().OrderByDescending(x => x.Value);
            ViewBag.SemestersFrom = new TranscriptManager().GetSemesterFromList() ?? new List<SelectListItem>();
            ViewBag.SemestersTo = new TranscriptManager().GetSemesterToList() ?? new List<SelectListItem>();
            ViewData["PageSize"] = -1;
            return View();
        }

        [OAuthorize(AppRoles.University_TransScript, AppRoles.University, AppRoles.View_TransScript)]
        public PartialViewResult TranscriptsList(Parameters parameter, PrintProgramme? otherParam1)
        {
            List<Transcript> list = new TranscriptManager().TranscriptList(otherParam1.Value, parameter);
            ViewBag.PrintProgramme = otherParam1;
            return PartialView(list);
        }
        [OAuthorize(AppRoles.University_TransScript, AppRoles.University)]
        [HttpPost]
        public JsonResult Validated(TranscriptUpdate TranscriptUpdate)
        {
            ResponseData response = new TranscriptManager().Validated(TranscriptUpdate.Student_ID, TranscriptUpdate.SemesterTo, TranscriptUpdate.Course_ID);
            return Json(response);
        }
        [OAuthorize(AppRoles.University_TransScript, AppRoles.University)]
        [HttpPost]
        public JsonResult Printed(TranscriptUpdate TranscriptUpdate)
        {
            ResponseData response = new TranscriptManager().Printed(TranscriptUpdate.Student_ID, TranscriptUpdate.SemesterTo, TranscriptUpdate.Course_ID);
            return Json(response);
        }
        [OAuthorize(AppRoles.University_TransScript, AppRoles.University)]
        [HttpPost]
        public JsonResult HandedOver(TranscriptUpdate TranscriptUpdate)
        {
            ResponseData response = new TranscriptManager().HandedOver(TranscriptUpdate.Student_ID, TranscriptUpdate.SemesterTo, TranscriptUpdate.Course_ID);
            return Json(response);
        }
        #endregion
    }
}
