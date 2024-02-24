using CUSrinagar.BusinessManagers;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using CUSrinagar.Models;
using CUSrinagar.OAuth;
using GeneralModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Nut;
using System.Threading.Tasks;

namespace CUSrinagar.WebApp.CUSrinagarAdminPanel.Controllers
{
    [OAuthorize(AppRoles.University_Evaluator)]

    public class AwardController : Controller
    {
        string errorMsg = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>##</a></div>";

        public ActionResult Award(Programme? programme, string SubjectId = null, string Semester = null, MarksFor MarksFor = MarksFor.Theory, int Batch = 0)
        {
            if (programme == null) return RedirectToAction("Index", "Dashboard");
            SetViewBags(programme.Value, SubjectId, Semester, MarksFor, Batch);

            return View("AwardExt");

        }

        private void SetViewBags(Programme programme, string SubjectId, string Semester, MarksFor MarksFor, int Batch)
        {
            IEnumerable<SelectListItem> SemesterList = new AwardManager().GetEvalvatorSubjects(programme).GroupBy(s => s.Semester).Select(g => g.First()).Select(x =>
                                   new SelectListItem()
                                   {
                                       Text = "Semester" + x.Semester,
                                       Value = x.Semester.ToString()
                                   });

            ViewBag.SemesterList = SemesterList.OrderBy(x => x.Value);
            ViewBag.SubjectsList = new List<SelectListItem>();
            ViewBag.DefaultBatch = Batch;
            ViewBag.DefaultSemester = Semester;
            ViewBag.DefaultSubject = SubjectId;
            ViewBag.DefaultMarksFor = MarksFor;
            if (!string.IsNullOrEmpty(Semester) && !string.IsNullOrEmpty(SubjectId))
            {
                ViewBag.SubjectsList = AwardManager.FillSubjectsAssigned(Convert.ToInt16(Semester), programme, Batch, isEvaluator: true);
            }
            ViewBag.Programme = programme;
        }

        private void SetViewBagsList(Parameters parameter, MarksFor marksFor, Programme programme)
        {
            ViewBag.OFFSET = new ResultManager().GetOFFSet(parameter);
            Guid SubjectId = new Guid((parameter?.Filters?.Where(x => x.Column.Trim().ToLower() == "combinationsubjects").FirstOrDefault()?.Value) ?? Guid.Empty.ToString());
            ViewBag.SubjectID = SubjectId;
            string semester = parameter?.Filters?.Where(x => x.Column.Trim().ToLower() == "semester").FirstOrDefault()?.Value ?? string.Empty;
            ViewBag.SemesterId = semester;
            ViewBag.DefaultMarksFor = marksFor;
            ADMSubjectMaster subject = new SubjectsManager().Get(SubjectId);
            ViewBag.IsSkill = subject.SubjectType;


            SetShowHideColumnViewBag(subject);
        }

        public PartialViewResult _GetChildDDL(int semester, Programme programme)
        {
            AwardFilterSettings awardFilterSettings = new ResultManager().FetchAwardFilterSettings(programme, MarksFor.Practical, Convert.ToInt32(semester));
            int batch = awardFilterSettings.FilterValue;
            ViewBag.ChildList = AwardManager.FillSubjectsAssigned(semester, programme, batch, isEvaluator: true);
            return PartialView();
        }
        [HttpPost]
        public async Task<ActionResult> Edit(List<AwardModel> semesterModelList, string SubjectID, string Semester, Programme programme, MarksFor marksFor, bool IsFinalSubmit = false)
        {
            AwardFilterSettings awardFilterSettings = new ResultManager().FetchAwardFilterSettings(programme, marksFor, Convert.ToInt32(Semester));
            if (awardFilterSettings == null)
                return RedirectToAction("Award", "Award", new { area = "CUSrinagarAdminPanel" });
            int Year = awardFilterSettings.FilterValue;
            int NoOfRowsEffected = 0;

            if (semesterModelList != null)
            {
                if (!IsFinalSubmit)
                {
                    semesterModelList = semesterModelList.Where(x => x.RecordStatus == RecordState.New && (!x.ExternalSubmitted)).ToList();
                }
                else { semesterModelList = semesterModelList.Where(x => (x.RecordStatus == RecordState.New || x.ExternalMarks == 0) && (!x.ExternalSubmitted)).ToList(); }

                NoOfRowsEffected = await new ResultManager().AddResultAsync(semesterModelList, Semester, programme, marksFor);
            }
            if (IsFinalSubmit)
            {
                return Content("Success");
            }

            TempData["response"] = errorMsg.Replace("##", $"{NoOfRowsEffected} record(s) saved successfully. {semesterModelList.Count - NoOfRowsEffected} remained unaffected.").Replace("alert-danger", "alert-success");
            return RedirectToAction("Award", "Award", new { SubjectId = SubjectID, Semester = Semester, Programme = programme, MarksFor = marksFor, Batch = Year });

        }
        //public ActionResult SubmitAwardGet(SubmitAward input)
        //{
        //    if (input == null) return RedirectToAction("Index", "Dashboard");
        //    SetViewBags(input.Programme, input.CombinationSubjects, input.Semester, input.MarksFor, input.Year);
        //    return View("SubmitAward");
        //}

        [HttpPost]
        public ActionResult SubmitAward(SubmitAward input)
        {
            int Semester = Convert.ToInt32(input.Semester);
            AwardFilterSettings awardFilterSettings = new ResultManager().FetchAwardFilterSettings(input.Programme, input.MarksFor, Semester);
            if (awardFilterSettings == null)
                return RedirectToAction("Award", "Award", new { area = "CUSrinagarAdminPanel" });
            input.Year = awardFilterSettings.FilterValue;
            input.Batch = 0;
            if (ModelState.IsValid)
            {
                if (!new ResultManager().CheckAwardExists(input))
                {
                    TempData["response"] = $"<div class='col-sm-1'></div><div class='alert alert-warning col-sm-10'><button type='button' class='close' data-dismiss='alert'><i class='ace-icon fa fa-times'></i></button><strong><i class='ace-icon fa fa-times'></i> Oh snap!</strong> Award Does Not Exist.<br></div><div class='col-sm-1'></div>";

                }

                else
                {
                    new ResultManager().Submit(input);
                    TempData["response"] = $"<div class='col-sm-1'></div><div class='alert alert-success col-sm-10'><button type='button' class='close' data-dismiss='alert'><i class='ace-icon fa fa-times'></i></button><strong><i class='ace-icon fa fa-times'></i> Awesome!</strong> Award submitted successfully.<br></div><div class='col-sm-1'></div>";
                }
            }
            else
                TempData["response"] = $"<div class='col-sm-1'></div><div class='alert alert-warning col-sm-10'><button type='button' class='close' data-dismiss='alert'><i class='ace-icon fa fa-times'></i></button><strong><i class='ace-icon fa fa-times'></i> Oh snap!</strong> Validation Error.<br></div><div class='col-sm-1'></div>";

            return RedirectToAction("Award", "Award", new { SubjectId = input.CombinationSubjects, Semester = input.Semester, Programme = input.Programme, MarksFor = input.MarksFor, Batch = input.Year });

        }

        //public String PostAward(SemesterModel model, PrintProgramme printProgramme, short semester, MarksFor marksFor)
        //{
        //    ResponseData response = new ResultManager().SaveAward(model, printProgramme, semester);
        //    return JsonConvert.SerializeObject(response);
        //}

        //public String PutAward(SemesterModel model, PrintProgramme printProgramme, short semester, MarksFor marksFor)
        //{
        //    ResponseData response = new ResultManager().UpdateAward(model, printProgramme, semester);
        //    return JsonConvert.SerializeObject(response);
        //}


        public ActionResult PrintAward(int? id, string id1, Guid? id2, Programme? programme, MarksFor marksFor = MarksFor.Theory)
        {
            if (id1 == null || id == null || programme == null || id2 == null)
            {
                return RedirectToAction("Award", "Award", new { area = "CUSrinagarAdminPanel" });
            }
            string semester = id1;
            AwardFilterSettings awardFilterSettings = new ResultManager().FetchAwardFilterSettings(programme.Value, marksFor, Convert.ToInt32(semester));
            if (awardFilterSettings == null)
                return RedirectToAction("Award", "Award", new { area = "CUSrinagarAdminPanel" });
            bool isResultDeclared = checkResult((Convert.ToInt32(semester)), programme.Value);
            int Year = awardFilterSettings.FilterValue;
            IEnumerable<AwardModel> listResult = new List<AwardModel>();
            Parameters parameter = new AwardManager().GetParameters(programme.Value, false, id2, id1, id.Value);
            ViewBag.ISAwardSubmitted = new ResultManager().CheckAwardSubmittedEval(parameter, programme.Value, marksFor, Year);
            if (parameter.Filters != null)
                listResult = new AwardManager().GetAllStudentListExt(parameter, programme.Value, marksFor, Year, awardFilterSettings.Courses, true, isResultDeclared);
            ViewBag.Year = Year;
            ADMSubjectMaster subject = new SubjectsManager().Get(id2.Value);
            ViewBag.Subject = subject.SubjectFullName;
            ViewBag.Programme = programme + " Semester-" + id1;
            SetShowHideColumnViewBag(subject);


            return View(listResult);
        }

        private bool checkResult(int semester, Programme programme)
        {
            return new ResultManager().checkIsResultDeclared(semester, programme);
        }

        public void SetShowHideColumnViewBag(ADMSubjectMaster subject)
        {
            ViewData["SubjectData"] = subject;
            bool HaveInternalColumn = new AwardManager().viewHaveAnyIntORExtColumn(subject, MarksIsPartOf.Internal, AwardModuleType.University);
            bool HaveExternalColumn = new AwardManager().viewHaveAnyIntORExtColumn(subject, MarksIsPartOf.External, AwardModuleType.University);
            int MinInternalMarks = new AwardManager().MinMarks(subject, MarksIsPartOf.Internal, AwardModuleType.University);
            int MinExternalMarks = new AwardManager().MinMarks(subject, MarksIsPartOf.External, AwardModuleType.University);
            ViewBag.HaveInternalColumn = HaveInternalColumn;
            ViewBag.HaveExternalColumn = HaveExternalColumn;
            ViewBag.MinInternalMarks = MinInternalMarks;
            ViewBag.MinExternalMarks = MinExternalMarks;

            bool ShowInternalColumn = subject.IsInternalMarksApplicable && subject.InternalVisibleTo == AwardModuleType.University;
            bool ShowExternalColumn = subject.IsExternalMarksApplicable && subject.ExternalVisibleTo == AwardModuleType.University;
            bool ShowExternalAttendanceColumn = subject.IsExternalAttendance_AssessmentMarksApplicable && subject.ExternalAttendance_AssessmentVisibleTo == AwardModuleType.University;
            bool ShowInternalAttendanceColumn = subject.IsInternalAttendance_AssessmentMarksApplicable && subject.InternalAttendance_AssessmentVisibleTo == AwardModuleType.University;

            ViewBag.InternalMaxMarks = subject.InternalMaxMarks;
            ViewBag.ExternalMaxMarks = subject.ExternalMaxMarks;
            ViewBag.ExternalAttendance_AssessmentMaxMarks = subject.ExternalAttendance_AssessmentMaxMarks;
            ViewBag.InternalAttendance_AssessmentMaxMarks = subject.InternalAttendance_AssessmentMaxMarks;
            ViewBag.MinPassMarks = (ShowInternalColumn ? subject.InternalMinPassMarks : 0) + (ShowExternalColumn ? subject.ExternalMinPassMarks : 0) + (ShowExternalAttendanceColumn ? subject.ExternalAttendance_AssessmentMinPassMarks : 0) + (ShowInternalAttendanceColumn ? subject.InternalAttendance_AssessmentMinPassMarks : 0);


            ViewBag.ShowInternalColumn = ShowInternalColumn;
            ViewBag.ShowExternalColumn = ShowExternalColumn;
            ViewBag.ShowExternalAttendanceColumn = ShowExternalAttendanceColumn;
            ViewBag.ShowInternalAttendanceColumn = ShowInternalAttendanceColumn;


            ViewBag.InternalLabel = subject.InternalMarksLabel;
            ViewBag.ExternalLabel = subject.ExternalMarksLabel;
            ViewBag.ExternalAttendanceLabel = subject.ExternalAttendance_AssessmentMarksLabel;
            ViewBag.InternalAttendanceLabel = subject.InternalAttendance_AssessmentMarksLabel;
        }
        public ActionResult AwardList_UG(Parameters parameter, Programme programme, MarksFor marksFor = MarksFor.Theory)
        {
            if (parameter.Filters == null)
                return RedirectToAction("Award", "Award", new { area = "CUSrinagarAdminPanel" });

            string semester = parameter?.Filters?.Where(x => x.Column == "Semester").FirstOrDefault()?.Value ?? string.Empty;
            //string subject = parameter?.Filters?.Where(x => x.Column == "Semester").FirstOrDefault()?.Value ?? string.Empty;
            AwardFilterSettings awardFilterSettings = new ResultManager().FetchAwardFilterSettings(programme, marksFor, Convert.ToInt32(semester));
            SetViewBagsList(parameter, marksFor, programme);
            ViewBag.ISAwardSubmitted = false;
            if (awardFilterSettings == null)
                return View();
            int Year = awardFilterSettings.FilterValue;
            ViewBag.ISAwardSubmitted = new ResultManager().CheckAwardSubmittedEval(parameter, programme, marksFor, Year);
            IEnumerable<AwardModel> listResult = new List<AwardModel>();
            if (ChekAwardFilter(awardFilterSettings, parameter))
            {
                if (!new AwardManager().checkHasDate(awardFilterSettings))
                {
                    awardFilterSettings.IsActive = false;
                    new AwardManager().UpdateFilter(awardFilterSettings);
                }
                else
                {
                    awardFilterSettings.IsActive = true;
                    new AwardManager().UpdateFilter(awardFilterSettings);
                    listResult = new AwardManager().GetAllStudentListExt(parameter, programme, marksFor, Year, awardFilterSettings.Courses);
                }
            }
            return View(listResult);
        }
        public ActionResult AwardList_IH(Parameters parameter, Programme programme, MarksFor marksFor = MarksFor.Theory)
        {
            if (parameter.Filters == null)
                return View();
            string semester = parameter?.Filters?.Where(x => x.Column == "Semester").FirstOrDefault()?.Value ?? string.Empty;

            AwardFilterSettings awardFilterSettings = new ResultManager().FetchAwardFilterSettings(programme, marksFor, Convert.ToInt32(semester));
            SetViewBagsList(parameter, marksFor, programme);
            ViewBag.ISAwardSubmitted = false;
            if (awardFilterSettings == null)
                return View();
            int Year = awardFilterSettings.FilterValue;
            ViewBag.ISAwardSubmitted = new ResultManager().CheckAwardSubmittedEval(parameter, programme, marksFor, Year);
            IEnumerable<AwardModel> listResult = new List<AwardModel>();
            if (ChekAwardFilter(awardFilterSettings, parameter))
            {
                if (awardFilterSettings.IsActive && !new AwardManager().checkHasDate(awardFilterSettings))
                {
                    awardFilterSettings.IsActive = !awardFilterSettings.IsActive;
                    new AwardManager().UpdateFilter(awardFilterSettings);
                }
                else
                {
                    listResult = new AwardManager().GetAllStudentListExt(parameter, programme, marksFor, Year, awardFilterSettings.Courses);
                }
            }
            return View(listResult);
        }

        public ActionResult AwardList_PG(Parameters parameter, Programme programme, MarksFor marksFor = MarksFor.Theory)
        {
            if (parameter.Filters == null)
                return View();
            string semester = parameter?.Filters?.Where(x => x.Column == "Semester").FirstOrDefault()?.Value ?? string.Empty;
            AwardFilterSettings awardFilterSettings = new ResultManager().FetchAwardFilterSettings(programme, marksFor, Convert.ToInt32(semester));
            SetViewBagsList(parameter, marksFor, programme);
            ViewBag.ISAwardSubmitted = false;
            if (awardFilterSettings == null)
                return View();
            int Year = awardFilterSettings.FilterValue;
            ViewBag.ISAwardSubmitted = new ResultManager().CheckAwardSubmitted(parameter, programme, marksFor, Year);
            IEnumerable<AwardModel> listResult = new List<AwardModel>();
            if (ChekAwardFilter(awardFilterSettings, parameter))
            {
                if (awardFilterSettings.IsActive && !new AwardManager().checkHasDate(awardFilterSettings))
                {
                    awardFilterSettings.IsActive = !awardFilterSettings.IsActive;
                    new AwardManager().UpdateFilter(awardFilterSettings);
                }
                else
                {
                    listResult = new AwardManager().GetAllStudentListExt(parameter, programme, marksFor, Year, awardFilterSettings.Courses);
                }
            }
            return View(listResult);
        }
        public string ConvertNumericToWord(int number)
        {
            if (number < 0)
            {
                return ((ResultStatus)number).ToString();
            }
            else
                return number.ToText(Language.English).ToUpper();
        }
        internal bool ChekAwardFilter(AwardFilterSettings awardFilterSettings, Parameters parameter)
        {
            if (parameter.Filters != null && (ViewBag.ISAwardSubmitted || awardFilterSettings.IsActive))
            {
                if (!string.IsNullOrEmpty(awardFilterSettings.Colleges) && awardFilterSettings.Colleges != "NULL")
                {
                    if (awardFilterSettings.Colleges.Contains(AppUserHelper.College_ID.ToString())) return true; else return false;
                }
                if (!string.IsNullOrEmpty(awardFilterSettings.Courses) && awardFilterSettings.Courses != "NULL")
                {
                    string subject = parameter?.Filters?.Where(x => x.Column == "CombinationSubjects").FirstOrDefault()?.Value ?? string.Empty;
                    Guid Course_Id = new SubjectsManager().GetSubjectsCourse(Guid.Parse(subject));
                    if (awardFilterSettings.Courses.Contains(Course_Id.ToString())) return true; else return false;
                }
                return true;
            }
            else
                return false;
        }

    }
}