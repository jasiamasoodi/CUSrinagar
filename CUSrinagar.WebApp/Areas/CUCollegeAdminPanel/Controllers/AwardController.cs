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
using Microsoft.Ajax.Utilities;

namespace CUSrinagar.WebApp.CUCollegeAdminPanel.Controllers
{
    [OAuthorize(AppRoles.College_AssistantProfessor)]
    public class AwardController : Controller
    {
        string errorMsg = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>##</a></div>";
        // GET: CUCollegeAdminPanel/Award
        public ActionResult Award(Programme? programme, string SubjectId = null, string Semester = null, MarksFor MarksFor = MarksFor.Practical, int Batch = 0)
        {
            if (programme == null) return RedirectToAction("Index", "Dashboard");
            SetViewBags(programme.Value, SubjectId, Semester, MarksFor, Batch);
            return View();
        }

        public ActionResult AwardList(Parameters parameter, Programme? otherParam1)
        {
            Programme programme = otherParam1.Value;
            MarksFor marksFor = MarksFor.Practical;
            if (parameter.Filters == null)
                return RedirectToAction("Award", "Award", new { area = "CUCollegeAdminPanel" });
            string semester = parameter?.Filters?.Where(x => x.Column == "Semester").FirstOrDefault()?.Value ?? string.Empty;
            AwardFilterSettings awardFilterSettings = new ResultManager().FetchAwardFilterSettings(programme, marksFor, Convert.ToInt32(semester));
            SetViewBagsList(parameter, marksFor, programme);
            ViewBag.ISAwardSubmitted = false;
            if (awardFilterSettings == null)
                return View();
            int batch = awardFilterSettings.FilterValue;
            ViewBag.ISAwardSubmitted = new ResultManager().CheckAwardSubmitted(parameter, programme, marksFor, batch);
            List<AwardModel> listResult = new List<AwardModel>();
            if (ChekAwardFilter(awardFilterSettings, parameter))
            {
                if (awardFilterSettings.IsActive && !new AwardManager().checkHasDate(awardFilterSettings))
                {
                    awardFilterSettings.IsActive = !awardFilterSettings.IsActive;
                    new AwardManager().UpdateFilter(awardFilterSettings);
                }
                else
                {
                    listResult = new AwardManager().GetAllStudentList(parameter, programme, marksFor, batch, false, awardFilterSettings.Courses);
                    listResult = listResult == null ? new List<AwardModel>() : listResult;
                    List<AwardModel> backLogList = new AwardManager().GetAllStudentList(parameter, programme, marksFor, batch, true, awardFilterSettings.Courses);
                    backLogList = backLogList == null ? new List<AwardModel>() : backLogList;
                    listResult.AddRange(backLogList);
                }
            }
            return View(listResult);
        }

        public ActionResult PrintAward(int? id, string id1, Guid? id2, bool? id3, Programme? programme, MarksFor marksFor = MarksFor.Practical)
        {
            if (id1 == null || id == null || programme == null || id2 == null || id3 == null)
            {
                return RedirectToAction("Award", "Award", new { area = "CUCollegeAdminPanel" });
            }

            string semester = id1;
            AwardFilterSettings awardFilterSettings = new ResultManager().FetchAwardFilterSettings(programme.Value, marksFor, Convert.ToInt32(semester));
            if (awardFilterSettings == null)
                return RedirectToAction("Award", "Award", new { area = "CUCollegeAdminPanel" });
            int batch = awardFilterSettings.FilterValue;
            List<AwardModel> listResult = new List<AwardModel>();
            Parameters parameter = new AwardManager().GetParameters(programme.Value, id3.Value, id2, id1, id.Value);
            ViewBag.ISAwardSubmitted = new ResultManager().CheckAwardSubmitted(parameter, programme.Value, marksFor, batch);
            if (parameter.Filters != null)
            {
                listResult = new AwardManager().GetAllStudentList(parameter, programme.Value, marksFor, batch, false, awardFilterSettings.Courses, true);
                listResult = listResult == null ? new List<AwardModel>() : listResult;
                List<AwardModel> backLogList = new AwardManager().GetAllStudentList(parameter, programme.Value, marksFor, batch, true, awardFilterSettings.Courses, true);
                backLogList = backLogList == null ? new List<AwardModel>() : backLogList;
                listResult.AddRange(backLogList);
            }
            ViewBag.Batch = id;
            ADMSubjectMaster subjectMaster = new SubjectsManager().Get(id2.Value);
            ViewBag.Subject = subjectMaster.SubjectFullName + "-" + subjectMaster.SubjectType;
            ViewBag.IsSkill = subjectMaster.SubjectType;
            ViewBag.Programme = programme + " Semester-" + id1;

            SetShowHideColumnViewBag(subjectMaster);
            return View(listResult);
        }
        public ActionResult AwardList_UG(Parameters parameter, Programme programme, MarksFor marksFor = MarksFor.Practical)
        {
            if (parameter.Filters == null)
                return RedirectToAction("Award", "Award", new { area = "CUCollegeAdminPanel" });
            string semester = parameter?.Filters?.Where(x => x.Column == "Semester").FirstOrDefault()?.Value ?? string.Empty;
            AwardFilterSettings awardFilterSettings = new ResultManager().FetchAwardFilterSettings(programme, marksFor, Convert.ToInt32(semester));
            SetViewBagsList(parameter, marksFor, programme);
            ViewBag.ISAwardSubmitted = false;
            if (awardFilterSettings == null)
                return View();
            int batch = awardFilterSettings.FilterValue;
            ViewBag.ISAwardSubmitted = new ResultManager().CheckAwardSubmitted(parameter, programme, marksFor, batch);
            List<AwardModel> listResult = new List<AwardModel>();
            if (ChekAwardFilter(awardFilterSettings, parameter))
            {
                if (awardFilterSettings.IsActive && !new AwardManager().checkHasDate(awardFilterSettings))
                {
                    awardFilterSettings.IsActive = !awardFilterSettings.IsActive;
                    new AwardManager().UpdateFilter(awardFilterSettings);
                }
                else
                {
                    listResult = new AwardManager().GetAllStudentList(parameter, programme, marksFor, batch, false, awardFilterSettings.Courses);
                    listResult = listResult == null ? new List<AwardModel>() : listResult;
                    List<AwardModel> backLogList = new AwardManager().GetAllStudentList(parameter, programme, marksFor, batch, true, awardFilterSettings.Courses);
                    backLogList = backLogList == null ? new List<AwardModel>() : backLogList;
                    listResult.AddRange(backLogList);
                }
            }
            else
            { ViewBag.Message = "Last Date for Award Submission has passed."; }
            return View(listResult);
        }


        public ActionResult AwardList_IH(Parameters parameter, Programme programme, MarksFor marksFor = MarksFor.Practical)
        {
            if (parameter.Filters == null)
                return RedirectToAction("Award", "Award", new { area = "CUCollegeAdminPanel" });
            string semester = parameter?.Filters?.Where(x => x.Column == "Semester").FirstOrDefault()?.Value ?? string.Empty;
            AwardFilterSettings awardFilterSettings = new ResultManager().FetchAwardFilterSettings(programme, marksFor, Convert.ToInt32(semester));
            SetViewBagsList(parameter, marksFor, programme);
            ViewBag.ISAwardSubmitted = false;
            if (awardFilterSettings == null)
                return View();
            int batch = awardFilterSettings.FilterValue;
            ViewBag.ISAwardSubmitted = new ResultManager().CheckAwardSubmitted(parameter, programme, marksFor, batch);
            List<AwardModel> listResult = new List<AwardModel>();
            if (ChekAwardFilter(awardFilterSettings, parameter))
            {
                if (awardFilterSettings.IsActive && !new AwardManager().checkHasDate(awardFilterSettings))
                {
                    awardFilterSettings.IsActive = !awardFilterSettings.IsActive;
                    new AwardManager().UpdateFilter(awardFilterSettings);
                }
                else
                {
                    listResult = new AwardManager().GetAllStudentList(parameter, programme, marksFor, batch, false, awardFilterSettings.Courses);
                    listResult = listResult == null ? new List<AwardModel>() : listResult;
                    //if (AppUserHelper.College_ID != Guid.Parse("9D03A374-4398-4A48-BE2A-FD9911EC6F82"))
                    //{
                    List<AwardModel> backLogList = new AwardManager().GetAllStudentList(parameter, programme, marksFor, batch, true, awardFilterSettings.Courses);
                    backLogList = backLogList == null ? new List<AwardModel>() : backLogList;
                    listResult.AddRange(backLogList);
                    //}
                }
            }
            else
            { ViewBag.Message = "Last Date for Award Submission has passed."; }
            return View(listResult);
        }
        public ActionResult AwardList_PG(Parameters parameter, Programme programme, MarksFor marksFor = MarksFor.Practical)
        {
            if (parameter.Filters == null)
                return RedirectToAction("Award", "Award", new { area = "CUCollegeAdminPanel" });
            string semester = parameter?.Filters?.Where(x => x.Column == "Semester").FirstOrDefault()?.Value ?? string.Empty;
            AwardFilterSettings awardFilterSettings = new ResultManager().FetchAwardFilterSettings(programme, marksFor, Convert.ToInt32(semester));
            SetViewBagsList(parameter, marksFor, programme);
            ViewBag.ISAwardSubmitted = false;
            if (awardFilterSettings == null)
                return View();
            int batch = awardFilterSettings.FilterValue;
            ViewBag.ISAwardSubmitted = new ResultManager().CheckAwardSubmitted(parameter, programme, marksFor, batch);
            List<AwardModel> listResult = new List<AwardModel>();
            if (ChekAwardFilter(awardFilterSettings, parameter))
            {
                if (awardFilterSettings.IsActive && !new AwardManager().checkHasDate(awardFilterSettings))
                {
                    awardFilterSettings.IsActive = !awardFilterSettings.IsActive;
                    new AwardManager().UpdateFilter(awardFilterSettings);
                }
                else
                {
                    listResult = new AwardManager().GetAllStudentList(parameter, programme, marksFor, batch, false, awardFilterSettings.Courses);
                    listResult = listResult == null ? new List<AwardModel>() : listResult;
                    List<AwardModel> backLogList = new AwardManager().GetAllStudentList(parameter, programme, marksFor, batch, true, awardFilterSettings.Courses);
                    backLogList = backLogList == null ? new List<AwardModel>() : backLogList;
                    listResult.AddRange(backLogList);
                }
            }
            else
            { ViewBag.Message = "Last Date for Award Submission has passed."; }
            return View(listResult);
        }
        private void SetViewBags(Programme programme, string SubjectId, string Semester, MarksFor MarksFor, int Batch)
        {
            ViewBag.showBacklog = new UserProfileManager().GetUserById(AppUserHelper.User_ID).ShowBacklog;
            IEnumerable<SelectListItem> SemesterList = new AwardManager().GetProfessorSubjects(programme).GroupBy(s => s.Semester).Select(g => g.First()).Select(x =>
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
                ViewBag.SubjectsList = AwardManager.FillSubjectsAssigned(Convert.ToInt16(Semester), programme, Batch,isEvaluator: false);
            }

            ViewBag.Programme = programme;
        }

        private void SetViewBagsList(Parameters parameter, MarksFor marksFor, Programme programme)
        {
            ViewBag.OFFSET = new ResultManager().GetOFFSet(parameter);
            Guid SubjectId = new Guid((parameter?.Filters?.Where(x => x.Column == "CombinationSubjects").FirstOrDefault()?.Value) ?? Guid.Empty.ToString());
            ViewBag.SubjectID = SubjectId;
            string semester = parameter?.Filters?.Where(x => x.Column == "Semester").FirstOrDefault()?.Value ?? string.Empty;

            ViewBag.SemesterId = semester;
            ViewBag.DefaultMarksFor = marksFor;
            ADMSubjectMaster subject = new SubjectsManager().Get(SubjectId);
            ViewBag.IsSkill = subject.SubjectType;

            SetShowHideColumnViewBag(subject);
        }
        public void SetShowHideColumnViewBag(ADMSubjectMaster subject)
        {
            ViewData["SubjectData"] = subject;
            bool HaveInternalColumn = new AwardManager().viewHaveAnyIntORExtColumn(subject, MarksIsPartOf.Internal, AwardModuleType.College);
            bool HaveExternalColumn = new AwardManager().viewHaveAnyIntORExtColumn(subject, MarksIsPartOf.External, AwardModuleType.College);
            int MinInternalMarks = new AwardManager().MinMarks(subject, MarksIsPartOf.Internal, AwardModuleType.College);
            int MinExternalMarks = new AwardManager().MinMarks(subject, MarksIsPartOf.External, AwardModuleType.College);
            ViewBag.HaveInternalColumn = HaveInternalColumn;
            ViewBag.HaveExternalColumn = HaveExternalColumn;
            ViewBag.MinInternalMarks = MinInternalMarks;
            ViewBag.MinExternalMarks = MinExternalMarks;

            bool ShowInternalColumn = subject.IsInternalMarksApplicable && subject.InternalVisibleTo == AwardModuleType.College;
            bool ShowExternalColumn = subject.IsExternalMarksApplicable && subject.ExternalVisibleTo == AwardModuleType.College;
            bool ShowExternalAttendanceColumn = subject.IsExternalAttendance_AssessmentMarksApplicable && subject.ExternalAttendance_AssessmentVisibleTo == AwardModuleType.College;
            bool ShowInternalAttendanceColumn = subject.IsInternalAttendance_AssessmentMarksApplicable && subject.InternalAttendance_AssessmentVisibleTo == AwardModuleType.College;

            ViewBag.InternalMaxMarks = subject.InternalMaxMarks;
            ViewBag.ExternalMaxMarks = subject.ExternalMaxMarks;
            ViewBag.ExternalAttendance_AssessmentMaxMarks = subject.ExternalAttendance_AssessmentMaxMarks;
            ViewBag.InternalAttendance_AssessmentMaxMarks = subject.InternalAttendance_AssessmentMaxMarks;

            ViewBag.ShowInternalColumn = ShowInternalColumn;
            ViewBag.ShowExternalColumn = ShowExternalColumn;
            ViewBag.ShowExternalAttendanceColumn = ShowExternalAttendanceColumn;
            ViewBag.ShowInternalAttendanceColumn = ShowInternalAttendanceColumn;


            ViewBag.InternalLabel = subject.InternalMarksLabel;
            ViewBag.ExternalLabel = subject.ExternalMarksLabel;
            ViewBag.ExternalAttendanceLabel = subject.ExternalAttendance_AssessmentMarksLabel;
            ViewBag.InternalAttendanceLabel = subject.InternalAttendance_AssessmentMarksLabel;
        }
        public PartialViewResult _GetChildDDL(int semester, Programme programme)
        {
            AwardFilterSettings awardFilterSettings = new ResultManager().FetchAwardFilterSettings(programme, MarksFor.Practical, Convert.ToInt32(semester));
            int batch = awardFilterSettings.FilterValue;
            ViewBag.ChildList = AwardManager.FillSubjectsAssigned(semester, programme, batch, isEvaluator: false);
            return PartialView();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(List<AwardModel> semesterModelList, string SubjectID, string Semester, Programme programme, MarksFor marksFor, bool IsFinalSubmit = false)
        {
            int semester = Convert.ToInt32(Semester);
            AwardFilterSettings awardFilterSettings = new ResultManager().FetchAwardFilterSettings(programme, marksFor, semester);
            if (awardFilterSettings == null)
                return RedirectToAction("Award", "Award", new { area = "CUCollegeAdminPanel" });
            int batch = awardFilterSettings.FilterValue;
            int NoOfRowsEffected = 0;
            if (semesterModelList != null)
            {
                if (!IsFinalSubmit)
                {
                    semesterModelList = semesterModelList.Where(x => x.RecordStatus == RecordState.New && (!x.InternalSubmitted || x.IsBacklog)).ToList();
                }
                else { semesterModelList = semesterModelList.Where(x => (x.RecordStatus == RecordState.New || (x.InternalMarks == 0 && x.ExternalAttendance_AssessmentMarks == 0 && x.InternalAttendance_AssessmentMarks == 0)) && (!x.InternalSubmitted)).ToList(); }

                NoOfRowsEffected = await new ResultManager().AddResultAsync(semesterModelList, Semester, programme, marksFor);

            }
            if (IsFinalSubmit)
            {
                return Content("Success");
            }
            TempData["response"] = errorMsg.Replace("##", $"{NoOfRowsEffected} record(s) saved successfully. {semesterModelList.Count - NoOfRowsEffected} remained unaffected.").Replace("alert-danger", "alert-success");
            return RedirectToAction("Award", "Award", new { SubjectId = SubjectID, Semester = Semester, Programme = programme, MarksFor = marksFor, Batch = batch });

        }

        [HttpPost]
        public JsonResult PostResultListItem(SemesterModel model, short Semester, Programme programme)
        {
            ResponseData response = new ResponseData();
            if (!ModelState.IsValid)
            {
                return Json(new ResponseData() { ErrorMessage = string.Join(", ", ModelState.Values.SelectMany(s => s.Errors).Select(y => y.ErrorMessage).ToArray()) });
            }
            else
            {
                if (model.IsBacklog && model.SubjectType != SubjectType.SEC)
                { model.ExternalMarks = 0; model.ExternalSubmitted = false; }
                model.InternalUpdatedBy = AppUserHelper.User_ID;
                model.InternalUpdatedOn = DateTime.Now;
                response = new ResultManager().PostRapidEntry(model, Semester, programme);
                return Json(response);
            }
        }

        //public ActionResult SubmitAwardGet(SubmitAward input)
        //{
        //    if (input == null) return RedirectToAction("Index", "Dashboard");
        //    SetViewBags(input.Programme, input.CombinationSubjects, input.Semester, input.MarksFor, input.Year);
        //    return View("SubmitAward");
        //}

        //[HttpPost]
        //public ActionResult SubmitAward(SubmitAward input)
        //{

        //    int Semester = Convert.ToInt32(input.Semester);
        //    AwardFilterSettings awardFilterSettings = new ResultManager().FetchAwardFilterSettings(input.Programme, input.MarksFor, Semester);
        //    if (awardFilterSettings == null)
        //        return RedirectToAction("Award", "Award", new { area = "CUCollegeAdminPanel" });
        //    input.Batch = awardFilterSettings.FilterValue;
        //    input.Year = 0;

        //    if (ModelState.IsValid)
        //    {
        //        if (!input.IsBacklog && !new ResultManager().CheckAwardExists(input))
        //        {
        //            TempData["response"] = $"<div class='col-sm-1'></div><div class='alert alert-warning col-sm-10'><button type='button' class='close' data-dismiss='alert'><i class='ace-icon fa fa-times'></i></button><strong><i class='ace-icon fa fa-times'></i> Oh snap!</strong> Award Does Not Exist.<br></div><div class='col-sm-1'></div>";

        //        }

        //        else
        //        {
        //            new ResultManager().Submit(input);
        //            TempData["response"] = $"<div class='col-sm-1'></div><div class='alert alert-success col-sm-10'><button type='button' class='close' data-dismiss='alert'><i class='ace-icon fa fa-times'></i></button><strong><i class='ace-icon fa fa-times'></i> Awesome!</strong> Award submitted successfully.<br></div><div class='col-sm-1'></div>";
        //        }
        //    }
        //    else
        //        TempData["response"] = $"<div class='col-sm-1'></div><div class='alert alert-warning col-sm-10'><button type='button' class='close' data-dismiss='alert'><i class='ace-icon fa fa-times'></i></button><strong><i class='ace-icon fa fa-times'></i> Oh snap!</strong> Validation Error.<br></div><div class='col-sm-1'></div>";

        //    return RedirectToAction("Award", "Award", new { SubjectId = input.CombinationSubjects, Semester = input.Semester, Programme = input.Programme, MarksFor = input.MarksFor, Batch = input.Year });

        //}
        public ActionResult FinalSubmit(List<string> list, SubmitAward dt)
        {
            if (list != null && dt != null)
            {
                int Semester = Convert.ToInt32(dt.Semester);
                AwardFilterSettings awardFilterSettings = new ResultManager().FetchAwardFilterSettings(dt.Programme, dt.MarksFor, Semester);
                if (awardFilterSettings == null)
                    return RedirectToAction("Award", "Award", new { area = "CUCollegeAdminPanel" });
                dt.Batch = awardFilterSettings.FilterValue;
                dt.Year = 0;

                if (new ResultManager().FinalSubmit(list, dt) > 0)
                    TempData["response"] = $"<div class='col-sm-1'></div><div class='alert alert-success col-sm-10'><button type='button' class='close' data-dismiss='alert'><i class='ace-icon fa fa-times'></i></button><strong><i class='ace-icon fa fa-times'></i> Awesome!</strong> Award submitted successfully.<br></div><div class='col-sm-1'></div>";
                else
                    TempData["response"] = $"<div class='col-sm-1'></div><div class='alert alert-warning col-sm-10'><button type='button' class='close' data-dismiss='alert'><i class='ace-icon fa fa-times'></i></button><strong><i class='ace-icon fa fa-times'></i> Oh snap!</strong> Validation Error.<br></div><div class='col-sm-1'></div>";
            }
            return Content("Success");
            //return RedirectToAction("Award", "Award", new { SubjectId = dt.CombinationSubjects, Semester = dt.Semester, Programme = dt.Programme, MarksFor = dt.MarksFor, Batch = dt.Year });

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