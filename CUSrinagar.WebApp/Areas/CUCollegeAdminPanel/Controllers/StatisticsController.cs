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
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CUSrinagar.WebApp.CUCollegeAdminPanel.Controllers
{
    [OAuthorize(AppRoles.College_Statistics)]
    public class StatisticsController : CollegeAdminBaseController
    {

        #region Statistics
        [HttpGet]
        public ActionResult Enrollment()
        {
            ViewData["PageSize"] = -1;
            FillViewBag_Batches();
            FillViewBag_Programmes();
            FillViewBag_Course(Programme.UG);
            FillViewBag_Semesters();
            return View();
        }
        [HttpPost]
        public ActionResult EnrollmentList(Parameters parameter)
        {
            var list = new StatisticsManager().GetstudentsCourseWiseStatistics(parameter);
            return PartialView(list);
        }

        [HttpGet]
        public ActionResult CategoryWise()
        {
            ViewData["PageSize"] = -1;
            FillViewBag_Batches();
            FillViewBag_Programmes();
            FillViewBag_Course(Programme.UG);
            FillViewBag_Semesters();
            return View();
        }

        public ActionResult CategoryWiseListPartial(Parameters parameter, PrintProgramme? otherparam1)
        {
            var list = new StatisticsManager().GetstudentsCourseWiseStatistics(parameter, true);
            return PartialView(list);
        }

        public ActionResult DistrictWise()
        {
            ViewData["PageSize"] = -1;
            FillViewBag_Batches();
            FillViewBag_Programmes();
            FillViewBag_Course(Programme.UG);
            FillViewBag_Semesters();
            return View();
        }
        public ActionResult DistrictListPartial(Parameters parameter, PrintProgramme? otherparam1)
        {
            var list = new StatisticsManager().GetstudentsCourseWiseStatistics(parameter, true, true);
            return PartialView(list);
        }
        #endregion
             
        //#region Statistics
        //public ActionResult Student()
        //{
        //    //ViewBag.Colleges = new CollegeManager().GetADMCollegeMasterList();
        //    ViewBag.College_ID = AppUserHelper.College_ID;
        //    ViewBag.Districts = new GeneralDDLManager().GetDistrictList();
        //    ViewBag.Gender = Helper.GetSelectList<Gender>();
        //    ViewBag.PrintProgrammes = Helper.GetSelectList<PrintProgramme>();
        //    GetResponseViewBags();
        //    return View();
        //}

        //public PartialViewResult StudentListPartial(Parameters parameter, List<string> otherParam1)
        //{
        //    if (otherParam1.IsNullOrEmpty() || string.IsNullOrEmpty(string.Join("", otherParam1).Trim()))
        //    {
        //        otherParam1.Add("Batch");
        //    }
        //    var groupBy = string.Join(", ", otherParam1);
        //    ViewBag.GroupByClause = otherParam1 ?? new List<string>();
        //    List<StudentStatisticsSummary> list = new StatisticsManager().GetStudentStatistics2(parameter, groupBy);
        //    return PartialView(list);
        //}

        //// lklklklkl
        //public ActionResult CourseCategoryWise()
        //{
        //    FillViewBag_Semesters();
        //    ViewBag.PrintProgrammes = Helper.GetSelectList<PrintProgramme>();
        //    return View();
        //}

        //public ActionResult CourseWiseListPartial(Parameters parameter, PrintProgramme? otherParam1)
        //{
        //    ViewBag.Category = new CategoryManager().GetCategoryList();
        //    short semester, batch;
        //    List<CourseCategoryWiseStatistics> list = null;
        //    if (otherParam1.HasValue && short.TryParse(parameter.Filters?.FirstOrDefault(x => x.Column == "Semester")?.Value, out semester) && short.TryParse(parameter.Filters?.FirstOrDefault(x => x.Column == "SemesterBatch")?.Value, out batch))
        //        list = new StatisticsManager().GetStudentStatisticsCourseWise(otherParam1.Value, batch, semester, AppUserHelper.College_ID.Value, true);
        //    return PartialView(list);
        //}

        //public ActionResult PrintCourseCategoryWise(Parameters parameter, PrintProgramme? printProgramme)
        //{
        //    ViewBag.Category = new CategoryManager().GetCategoryList(new Parameters());
        //    ViewBag.College = AppUserHelper.AppUsercompact.FullName;

        //    short semester = 0, batch = 0;
        //    List<CourseCategoryWiseStatistics> list = null;
        //    if (printProgramme.HasValue && short.TryParse(parameter.Filters?.FirstOrDefault(x => x.Column == "Semester")?.Value, out semester) && short.TryParse(parameter.Filters?.FirstOrDefault(x => x.Column == "SemesterBatch")?.Value, out batch))
        //        list = new StatisticsManager().GetStudentStatisticsCourseWise(printProgramme.Value, batch, semester, AppUserHelper.College_ID.Value, true);
        //    ViewBag.Programme = printProgramme.HasValue ? Helper.GetEnumDescription(printProgramme.Value) : "";
        //    ViewBag.Batch = batch;
        //    ViewBag.Semester = semester;
        //    return View(list);
        //}
        //// kjlkjlkj


        //public ActionResult CourseDistrictCategoryWise()
        //{
        //    ViewBag.PrintProgrammes = Helper.GetSelectList<PrintProgramme>();
        //    return View();
        //}

        //public ActionResult CourseDistrictCategoryListPartial(Parameters parameter, PrintProgramme? otherParam1)
        //{
        //    ViewBag.Category = new CategoryManager().GetCategoryList(new Parameters());
        //    short semester, batch;
        //    List<CourseDistrictWiseStatistics> list = null;
        //    if (otherParam1.HasValue && short.TryParse(parameter.Filters?.FirstOrDefault(x => x.Column == "Semester")?.Value, out semester) && short.TryParse(parameter.Filters?.FirstOrDefault(x => x.Column == "SemesterBatch")?.Value, out batch))
        //        list = new StatisticsManager().GetCourseDistrictCategoryWise(otherParam1.Value, batch, semester);
        //    return PartialView(list);
        //}

        //public ActionResult PrintCourseDistrictCategory(Parameters parameter, PrintProgramme? printProgramme)
        //{
        //    ViewBag.Category = new CategoryManager().GetCategoryList();
        //    ViewBag.College = AppUserHelper.AppUsercompact.FullName;
        //    short semester = 0, batch = 0;
        //    CollegeCourseDistrictCategoryStatistics _CollegeCourseDistrictCategoryStatistics = new CollegeCourseDistrictCategoryStatistics() { CollegeFullName = AppUserHelper.AppUsercompact.FullName };
        //    if (printProgramme.HasValue && short.TryParse(parameter.Filters?.FirstOrDefault(x => x.Column == "Semester")?.Value, out semester) && short.TryParse(parameter.Filters?.FirstOrDefault(x => x.Column == "SemesterBatch")?.Value, out batch))
        //        _CollegeCourseDistrictCategoryStatistics = new StatisticsManager().ByCollegeCourseDistrictCategory(printProgramme.Value, batch, semester, AppUserHelper.College_ID.Value);
        //    ViewBag.Programme = printProgramme.HasValue ? Helper.GetEnumDescription(printProgramme.Value) : "";
        //    ViewBag.Batch = batch;
        //    ViewBag.Semester = semester;
        //    return View(_CollegeCourseDistrictCategoryStatistics);
        //}


        //public void StatisticCSV(Parameters parameter, string otherParam1)
        //{
        //    if (parameter == null) parameter = new Parameters();
        //    parameter.PageInfo = new Paging() { PageNumber = -1, PageSize = -1 };
        //    if (otherParam1.IsNullOrEmpty() || string.IsNullOrEmpty(otherParam1))
        //        otherParam1 = "[\"Batch\"]";

        //    List<string> showColumns = JsonConvert.DeserializeObject<List<string>>(otherParam1);
        //    var groupBy = string.Join(", ", showColumns);
        //    List<StudentStatisticsSummary> list = new StatisticsManager().GetStudentStatistics2(parameter, groupBy);

        //    //var reportList = listResult.Select(x => new
        //    //{
        //    //    x.RegistrationNumber,
        //    //    x.RollNumber,
        //    //    x.Name,
        //    //    x.FathersName,
        //    //    DOB = x.DateofBirth.ToString("d"),
        //    //    x.Email,
        //    //    x.MobileNumber,
        //    //    x.CollegeCode,
        //    //    x.CourseCode,
        //    //    x.CombinationCode,
        //    //    Subjects = x.Subjects == null ? null : string.Join(",", x.Subjects.Select(y => y.Name)),
        //    //    MaxMarks = x.Subjects == null ? 0 : x.Subjects?.Sum(y => y.MaxMarks),
        //    //    MarksObtained = x.Subjects == null ? 0 : x.Subjects?.Sum(y => y.PracticalMarks + y.TheoryMarks)
        //    //}).ToList();
        //    ExportToCSV(list, "ResultReport-Semester-1");
        //}
        //#endregion

        #region Photographs ZIP
        private void SetViewBags()
        {
            ViewBag.ProgrammeList = Helper.GetSelectList<PrintProgramme>().OrderBy(x => x.Value);
            ViewBag.CourseList = new List<SelectListItem>();
            ViewBag.CombinationList = new List<SelectListItem>();
            ViewBag.SemesterList = new List<SelectListItem>();
            ViewBag.SearchedQuery = "";
        }

        [HttpGet]
        public ActionResult DownloadPhotographs()
        {
            SetViewBags();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DownloadPhotographs(short? Year, PrintProgramme? ProgrammeId, Guid? CourseID, short? Semester)
        {
            SetViewBags();

            Guid Course_ID = Guid.Empty;
            short semester = 1;
            if (!short.TryParse(Year + "", out short year) || !short.TryParse(Semester + "", out semester)
                || !Guid.TryParse(CourseID + "", out Course_ID) || ProgrammeId == null)
            {
                TempData["response"] = @"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button>All fields are required</div>";
                return View();
            }
            string CourseName = new CourseManager().GetCourseById(CourseID ?? Guid.Empty)?.CourseFullName;
            ViewBag.SearchedQuery = $"Batch_{Year}_{CourseName.Replace("'", "").Replace(".", "").Replace("/", "").Replace("\\", "").Replace(":", "")}_Sem-{semester}";

            List<ARGPersonalInformation> _ARGPersonalInformation = await new StatisticsManager().DownloadPhotographsAsync(year, (PrintProgramme)ProgrammeId, Course_ID, semester);
            return View(_ARGPersonalInformation);
        }
        #endregion

    }
}