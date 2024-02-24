using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Newtonsoft.Json;
using CUSrinagar.Models;
using CUSrinagar.BusinessManagers;
using GeneralModels;
using CUSrinagar.Enums;
using CUSrinagar.OAuth;
using CUSrinagar.Extensions;
using System;
using CUSrinagar.DataManagers;
using System.Threading.Tasks;

namespace CUSrinagar.WebApp.CUSrinagarAdminPanel.Controllers
{
    [OAuthorize(AppRoles.University, AppRoles.University_ReEvaluations)]
    public class DeanResultController : UniversityAdminBaseController
    {
        public ActionResult ReEvaluationXerox()
        {
            FillViewBags(null);
            return View();
        }

        [HttpPost]
        public ActionResult ReEvaluationXerox(Parameters parameter, Programme? otherParam1)
        {
            if (otherParam1 == null || otherParam1 == 0)
            {
                return RedirectToAction("ReEvaluationXerox", "DeanResult");
            }

            var value = Request.QueryString["Parameters"];
            if (value.IsNotNullOrEmpty())
            {
                parameter = JsonConvert.DeserializeObject<Parameters>(value);
            }
            FillViewBags(parameter, (Programme)otherParam1);
            List<ReEvaluation> list = null;
            if (otherParam1.HasValue)
            {
                list = new ReEvaluationManager().GetReEvaluationList(parameter, otherParam1.Value);
            }

            return View(list ?? new List<ReEvaluation>());
        }

        [HttpPost]
        public ActionResult ReEvaluationXeroxListPartail(Parameters parameter, Programme? otherParam1)
        {
            var value = Request.QueryString["Parameters"];
            if (value.IsNotNullOrEmpty())
                parameter = JsonConvert.DeserializeObject<Parameters>(value);

            List<ReEvaluation> list = null;
            if (otherParam1.HasValue)
            {
                list = new ReEvaluationManager().GetReEvaluationList(parameter, otherParam1.Value);
            }
            return View(list);
        }

        void FillViewBags(Parameters parameters, Programme? programme = Programme.UG)
        {

            List<SelectListItem> semestersDropDownList = new List<SelectListItem>();
            for (int i = 1; i <= 12; i++)
            {
                semestersDropDownList.Add(new SelectListItem() { Text = $"Semester-{i}", Value = i.ToString() });
            }
            var PrintProgrammeDropDownList = Helper.GetSelectList<Programme>().OrderBy(x => x.Text);

            if (parameters != null)
            {
                semestersDropDownList.First(i => i.Value == parameters.Filters.First(x => x.Column.ToUpper().Equals("SEMESTER")).Value).Selected = true;
                ViewBag.Semester = parameters.Filters.First(x => x.Column.ToUpper().Equals("SEMESTER")).Value;

                var formType = parameters.Filters.FirstOrDefault(x => x.Column.ToUpper().Equals("FORMTYPE"));
                if (formType != null)
                    ViewBag.FormType = parameters.Filters.First(x => x.Column.ToUpper().Equals("FORMTYPE")).Value;
                else
                    ViewBag.FormType = 0;

                PrintProgrammeDropDownList.First(i => i.Value == ((int)programme).ToString()).Selected = true;
            }
            var batches = new List<SelectListItem>();
            for (int year = 2017; year <= DateTime.Now.Year; year++)
            {
                batches.Add(new SelectListItem { Text = year.ToString(), Value = year.ToString() });
            }
            ViewBag.Batches = batches.OrderByDescending(x => x.Value);
            ViewBag.Programme = programme;
            ViewBag.Semesters = semestersDropDownList;
            ViewBag.PrintProgrammeList = PrintProgrammeDropDownList;
            ViewBag.FormTypeList = new List<SelectListItem>() { new SelectListItem() { Text = FormType.ReEvaluation.ToString(), Value = ((short)FormType.ReEvaluation).ToString() }, new SelectListItem() { Text = FormType.Xerox.ToString(), Value = ((short)FormType.Xerox).ToString() } };

        }

        public void ReEvaluationXeroxCSV(Parameters parameter, Programme? printProgramme)
        {
            if (printProgramme.HasValue)
            {
                if (parameter == null)
                    parameter = new Parameters();
                if (parameter.PageInfo == null)
                    parameter.PageInfo = new Paging();
                parameter.PageInfo.PageNumber = parameter.PageInfo.PageSize = -1;
                List<ReEvaluation> list = new ReEvaluationManager().GetReEvaluationList(parameter, printProgramme.Value);
                var reportList = list.Select(x => new
                {
                    x.Semester,
                    Batch = x.SemesterBatch,
                    AppliedOn = x.DateFrom,
                    x.ExamRollNumber,
                    College = x.CollegeFullName,
                    x.FormNumber,
                    x.FullName,
                    x.MobileNo,
                    Course = x.CourseFullName,
                    subject = x.SubjectsForEvaluation == null ? "" : string.Join($", {Environment.NewLine}", x.SubjectsForEvaluation.Select(y => y.SubjectID).ToList()),
                     x.CenterDetail,
                    FormStatus = Helper.GetEnumDescription(x.FormStatus)
                }).OrderBy(x => x.subject).ThenBy(x => x.FormNumber).ToList();
                ExportToCSV(reportList, "ReEvaluations_Xerox List Dwnld on " + DateTime.Now.ToLongDateString());
            }
        }

        #region subject_result_discrepancy
        public void ResultDiscrepancyCSV()
        {
            var dd = new ResultManager().SubjectResultDiscrepancyDataTable();
            DownloadExcel(dd, "Economics List_" + DateTime.Now.ToString("dd-MMM-yy"));

        }
        #endregion

        #region Find Subject Clash for DateSheets

        [HttpGet]
        public ActionResult FindSubjectClashforDateSheets()
        {
            FillSubjectClashViewBags(null, null);
            ViewBag.SubjectsEntered = "";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> FindSubjectClashforDateSheets(Programme programme, short Semester, string Subjects, List<SubjectType> subjectTypes, short examYear)
        {
            FillSubjectClashViewBags(null, null);
            ViewBag.SubjectsEntered = Subjects;

            List<ADMCombinationMaster> aDMCombinationMasters = await new CombinationManager().GetADMCombinationMastersForDateSheetClashAsync(programme, Semester, Subjects, subjectTypes, examYear);
            return View("FindSubjectClashforDateSheets", aDMCombinationMasters);
        }

        private void FillSubjectClashViewBags(Programme? programme, short? Semester)
        {
            IEnumerable<SelectListItem> ProgrammeDropDownList = Helper.GetSelectList(Programme.HS, Programme.Professional);
            ProgrammeDropDownList.ToList().First(x => x.Value == "3").Text = "Integrated / Honor's / Professional";
            List<SelectListItem> semestersDropDownList = new List<SelectListItem>();
            for (int i = 1; i <= 12; i++)
            {
                semestersDropDownList.Add(new SelectListItem() { Text = $"Semester-{i}", Value = i.ToString() });
            }
            if (programme != null && Semester != null)
            {
                semestersDropDownList.First(i => i.Value == Semester.Value.ToString()).Selected = true;
                ViewBag.Semester = Semester;
                ProgrammeDropDownList.First(i => i.Value == ((int)programme).ToString()).Selected = true;
            }
            else
            {
                ViewBag.Programme = Programme.UG;
                ViewBag.Semester = "1";
            }

            ViewBag.ProgrammeList = ProgrammeDropDownList;
            ViewBag.Semesters = semestersDropDownList;
            IEnumerable<SelectListItem> SubjTypes = Helper.GetSelectList(SubjectType.None);

            ViewBag.SubjTypes = SubjTypes;
        }

        #endregion
    }

}
