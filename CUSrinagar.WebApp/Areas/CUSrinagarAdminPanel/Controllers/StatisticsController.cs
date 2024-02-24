using CUSrinagar.BusinessManagers;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using CUSrinagar.Models;
using CUSrinagar.OAuth;
using GeneralModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using System.Xml.Linq;

namespace CUSrinagar.WebApp.CUSrinagarAdminPanel.Controllers
{
    public class StatisticsController : AdminController
    {
        #region Statistics
        [OAuthorize(AppRoles.University_Statistics, AppRoles.University, AppRoles.University_Dean)]
        [HttpGet]
        public ActionResult Enrollment()
        {
            ViewData["PageSize"] = -1;
            FillViewBag_Batches();
            FillViewBag_College();
            FillViewBag_Programmes();
            FillViewBag_Course(Programme.UG);
            FillViewBag_Semesters();
            return View();
        }

        [OAuthorize(AppRoles.University_Statistics, AppRoles.University, AppRoles.University_Dean)]
        [HttpPost]
        public ActionResult EnrollmentList(Parameters parameter)
        {
            var list = new StatisticsManager().GetstudentsCourseWiseStatistics(parameter);
            return PartialView(list);
        }

        [OAuthorize(AppRoles.University_Statistics, AppRoles.University, AppRoles.University_Dean)]
        [HttpGet]
        public ActionResult CategoryWise()
        {
            ViewData["PageSize"] = -1;
            FillViewBag_Batches();
            FillViewBag_College();
            FillViewBag_Programmes();
            FillViewBag_Course(Programme.UG);
            FillViewBag_Semesters();
            return View();
        }

        [OAuthorize(AppRoles.University, AppRoles.University_Statistics, AppRoles.University_Dean)]
        public ActionResult CategoryWiseListPartial(Parameters parameter, PrintProgramme? otherparam1)
        {
            var list = new StatisticsManager().GetstudentsCourseWiseStatistics(parameter, true);
            return PartialView(list);
        }
        [OAuthorize(AppRoles.University, AppRoles.University_Statistics, AppRoles.University_Dean)]
        public ActionResult DistrictWise()
        {
            ViewData["PageSize"] = -1;
            FillViewBag_Batches();
            FillViewBag_College();
            FillViewBag_Programmes();
            FillViewBag_Course(Programme.UG);
            FillViewBag_Semesters();
            return View();
        }

        [OAuthorize(AppRoles.University, AppRoles.University_Statistics, AppRoles.University_Dean)]
        public ActionResult DistrictListPartial(Parameters parameter, PrintProgramme? otherparam1)
        {
            var list = new StatisticsManager().GetstudentsCourseWiseStatistics(parameter, true, true);
            return PartialView(list);
        }

        #endregion

        #region Examination Widgets
        [HttpGet]
        [OAuthorize(AppRoles.University, AppRoles.University_Statistics, AppRoles.University_CAO)]
        public ActionResult ExaminationFormWigets()
        {
            ViewBag.PrintProgrammes = Helper.GetSelectList<PrintProgramme>();
            GetResponseViewBags();
            GetResponseViewBagsForExaminationWidget();
            return View();
        }

        [HttpPost]
        [OAuthorize(AppRoles.University, AppRoles.University_Statistics, AppRoles.University_CAO)]
        public ActionResult ExaminationFormWigets(PrintProgramme programme, int batch, short semester)
        {
            GetResponseViewBagsForExaminationWidget();
            ResponseData responseData = new ExaminationFormManager().GetExaminationFormWidget(programme, batch, semester);
            ViewBag.Programme = programme;
            ViewBag.SelectedBatch = batch;
            ViewBag.Sem = semester;
            ViewBag.ErrorMsg = responseData.ErrorMessage;
            return View(responseData.ResponseObject == null ? null : ((ExaminationFormWidget)responseData.ResponseObject));
        }

        private void GetResponseViewBagsForExaminationWidget()
        {
            ViewBag.PrintProgrammes = Helper.GetSelectList<PrintProgramme>();
            ViewBag.Semesters = new SelectList(
                                                new List<SelectListItem>
                                                {
                                                    new SelectListItem { Selected = false, Text = "Semester I", Value = 1.ToString()},
                                                    new SelectListItem { Selected = false, Text = "Semester II", Value = 2.ToString()},
                                                    new SelectListItem { Selected = false, Text = "Semester III", Value = 3.ToString()},
                                                    new SelectListItem { Selected = false, Text = "Semester IV", Value = 4.ToString()},
                                                    new SelectListItem { Selected = false, Text = "Semester V", Value = 5.ToString()},
                                                    new SelectListItem { Selected = false, Text = "Semester VI", Value = 6.ToString()},
                                                    new SelectListItem { Selected = false, Text = "Semester VII", Value = 7.ToString()},
                                                    new SelectListItem { Selected = false, Text = "Semester VIII", Value = 8.ToString()},

                                                }, "Value", "Text", 1);

            ViewBag.Batches = Helper.GetYearsDDL();
        }
        #endregion

        #region Admission Widgets

        [HttpGet]
        [OAuthorize(AppRoles.University, AppRoles.University_Statistics, AppRoles.University_Dean)]
        public ActionResult AdmissionFormWigets()
        {
            ViewBag.PrintProgrammes = Helper.GetSelectList<PrintProgramme>();
            GetResponseViewBagsForAdmission();
            //List<AdmissionFormWidget> admissionFormWidgets = new RegistrationManager().GetAdmissionFormMainWidget(null, null, null);
            return View();
        }

        [HttpPost]
        [OAuthorize(AppRoles.University, AppRoles.University_Statistics, AppRoles.University_Dean)]
        public ActionResult AdmissionFormWigets(PrintProgramme? programme, int? Batch, Guid? College_ID)
        {
            GetResponseViewBagsForAdmission();

            ViewBag.Programme = programme;
            ViewBag.ExamYear = Batch;
            ViewBag.College_ID = College_ID;
            return View();
        }

        [OAuthorize(AppRoles.University, AppRoles.University_Statistics, AppRoles.University_Dean)]
        public ActionResult NewAdmissionStatistics()
        {
            var items = Helper.GetSelectList<PrintProgramme>().ToList();
            items.Remove(items.FirstOrDefault(i => i.Value == "5"));
            ViewBag.PrintProgrammes = items;
            return View();
        }

        [OAuthorize(AppRoles.University, AppRoles.University_Statistics, AppRoles.University_Dean)]
        public ActionResult NewAdmissionStatisticsList(Parameters parameter, PrintProgramme otherparam1)
        {
            var items = Helper.GetSelectList<PrintProgramme>().ToList();
            items.Remove(items.FirstOrDefault(i => i.Value == "5"));
            ViewBag.PrintProgrammes = items;
            parameter.Filters = null;
            parameter.PageInfo.PageNumber = -1;
            parameter.PageInfo.PageSize = -1;
            List<NewAdmissionsWidget> newAdmissionWidgets = new RegistrationManager().GetNewAdmissionsStatistics(parameter, otherparam1);
            ViewBag.NoOfStudentsWithFees = new RegistrationManager().GetNewAdmissionsStudentCountWithFees();
            ViewBag.NoOfStudentsWithoutFees = new RegistrationManager().GetNewAdmissionsStudentCountWithoutFees();
            ViewBag.GraduationOnly = new RegistrationManager().GetGraduationOnlyNewAdmissionsStudentCountWithFees();
            //newAdmissionWidgets.Where(i => i.CourseFullName == "B.Sc. Medical").First().NoOfStudents = newAdmissionWidgets.Where(i => i.CourseFullName == "B.Sc. Medical").First().NoOfStudents;
            return View(newAdmissionWidgets);
        }


        [HttpGet]
        [OAuthorize(AppRoles.University, AppRoles.University_Statistics, AppRoles.University_Dean)]
        public ActionResult GetCourseWiseCountForProgramme(PrintProgramme? programme, int? Batch, Guid? College_ID)
        {
            List<AdmissionFormWidgetCourseWise> courseWiseData = new RegistrationManager().GetCourseWiseCountForProgramme(programme, Batch, College_ID);

            var totals = new RegistrationManager().GetCourseWiseCountForProgramme(null, Batch, null);
            ViewBag.Totals = totals.Sum(i => i.NoOfStudents);

            return PartialView(courseWiseData);
        }


        //[HttpGet]
        //[OAuthorize(AppRoles.University, AppRoles.University_Statistics)]
        public string GetProgrammeWiseCount(PrintProgramme programme, int? Batch)
        {
            return (new RegistrationManager()?.GetProgrammeWiseCount(programme, Batch)?.Where(x => x.PrintProgramme == programme)?.Sum(i => i.NoOfStudents) ?? 0).ToString();
        }


        [OAuthorize(AppRoles.University, AppRoles.University_Statistics, AppRoles.University_Dean)]
        public ActionResult GetCourseWiseGenderCount(PrintProgramme? programme, int? Batch, Guid College_ID, string CourseFullName)
        {
            List<CourseWiseGenderCountWidget> courseWiseData = new RegistrationManager().GetCourseWiseGenderCount(programme, Batch, College_ID, CourseFullName);
            return PartialView(courseWiseData);
        }
        private void GetResponseViewBagsForAdmission()
        {
            var collegeList = new CollegeManager().GetCollegeList().Select(college => new SelectListItem { Text = college.CollegeFullName, Value = college.College_ID.ToString() }).ToList();
            collegeList.Insert(0, new SelectListItem() { Selected = true, Text = " Select College " });
            ViewBag.Colleges = collegeList;

            var selectListItems = Helper.GetSelectList<PrintProgramme>().ToList();
            selectListItems.Insert(0, new SelectListItem() { Selected = true, Text = " Select Programme " });
            ViewBag.PrintProgrammes = selectListItems;

            List<SelectListItem> _batchs = new List<SelectListItem> {
                new SelectListItem { Selected = true, Text = "All Batches",Value="" }
            };

            int _Year = DateTime.Now.Year;
            do
            {
                _batchs.Add(new SelectListItem { Selected = false, Text = _Year.ToString(), Value = _Year.ToString() });
                _Year--;
            } while (_Year > 2016);


            List<SelectListItem> _semesters = new List<SelectListItem> {
                new SelectListItem { Selected = true, Text = "Semester-1",Value="1" }
            };
            int semester = 1;
            do
            {
                semester++;
                _semesters.Add(new SelectListItem { Selected = false, Text = ("Semester-" + semester), Value = semester.ToString() });

            } while (semester <= 8);

            ViewBag.Semesters = _semesters;
            ViewBag.ExaminationYears = new SelectList(_batchs, "Value", "Text", 1);
        }

        #endregion

        #region Grievance Widgets

        [HttpGet]
        [OAuthorize(AppRoles.University, AppRoles.University_Statistics)]
        public ActionResult GrievanceWidgets()
        {
            ViewBag.PrintProgrammes = Helper.GetSelectList<PrintProgramme>();
            GetResponseViewBagsForGrievance();
            List<GrievanceWidgetSummary> widgetSummary = new GrievanceManager().GetGrievanceWidgetsData(null, null);
            return View(widgetSummary);
        }

        [HttpPost]
        [OAuthorize(AppRoles.University, AppRoles.University_Statistics)]
        public ActionResult GrievanceWidgets(Guid? college_ID, GrievanceCategory? grievanceCategory)
        {
            ViewBag.PrintProgrammes = Helper.GetSelectList<PrintProgramme>();
            GetResponseViewBagsForGrievance();
            List<GrievanceWidgetSummary> widgetSummary = new GrievanceManager().GetGrievanceWidgetsData(college_ID, grievanceCategory);
            return View(widgetSummary);
        }

        [OAuthorize(AppRoles.University, AppRoles.University_Statistics)]
        public JsonResult GrievanceWidgetsResolved(Guid? college_ID, GrievanceCategory? grievanceCategory)
        {
            var grievanceWidgetsResolved = new GrievanceManager().GetGrievanceWidgetsResolved(college_ID, grievanceCategory) ?? new GrievanceResolvedWidgetSummary();
            return Json(grievanceWidgetsResolved);
        }

        [OAuthorize(AppRoles.University, AppRoles.University_Statistics)]
        public JsonResult GrievanceWidgetsVerified(Guid? college_ID, GrievanceCategory? grievanceCategory)
        {
            var GrievanceWidgetsVerified = new GrievanceManager().GetGrievanceWidgetsVerified(college_ID, grievanceCategory) ?? new GrievanceVerifiedWidgetSummary();
            return Json(GrievanceWidgetsVerified);
        }

        [OAuthorize(AppRoles.University, AppRoles.University_Statistics)]
        public JsonResult GrievanceWidgetsAssigned(Guid? college_ID, GrievanceCategory? grievanceCategory)
        {
            var GrievanceWidgetsVerified = new GrievanceManager().GetGrievanceWidgetsAssigned(college_ID, grievanceCategory) ?? new GrievanceAssignedWidgetSummary();
            return Json(GrievanceWidgetsVerified);
        }

        [OAuthorize(AppRoles.University, AppRoles.University_Statistics)]
        public JsonResult GrievanceWidgetsGeneral(Guid? college_ID, GrievanceCategory? grievanceCategory)
        {
            var GrievanceWidgetsGeneral = new GrievanceManager().GetGrievanceWidgetsGeneral(college_ID, grievanceCategory) ?? new GrievanceGeneralWidgetSummary();
            return Json(GrievanceWidgetsGeneral);
        }

        private void GetResponseViewBagsForGrievance()
        {
            var collegeList = new CollegeManager().GetCollegeList().Select(college => new SelectListItem { Text = college.CollegeFullName, Value = college.College_ID.ToString() }).ToList();
            collegeList.Insert(0, new SelectListItem() { Selected = true, Text = " Select College " });
            ViewBag.Colleges = collegeList;

            var selectListItems = Helper.GetSelectList<GrievanceCategory>().ToList();
            selectListItems.Insert(0, new SelectListItem() { Selected = true, Text = " Select Category " });
            ViewBag.GrievanceCategory = selectListItems;

        }

        #endregion

        #region AwardsStatistics


        [HttpGet]
        [OAuthorize(AppRoles.University, AppRoles.University_Statistics)]
        public ActionResult AwardStatistics()
        {
            ViewBag.PrintProgrammes = Helper.GetSelectList<PrintProgramme>();
            GetResponseViewBagsForAwards();
            return View();
        }

        [HttpGet]
        [OAuthorize(AppRoles.University, AppRoles.University_Statistics)]
        public ActionResult GetCoursesForCollege(Guid college_ID, PrintProgramme? printProgramme, string Batch, string Semester)
        {
            List<CourseCollegeWidget> courses = new CourseManager().GetAllCourseListByCollegeID(college_ID, printProgramme);
            ViewBag.PrintProgramme = printProgramme;
            ViewBag.College_ID = college_ID;
            ViewBag.Batch = Batch;
            ViewBag.Semester = Semester;
            return PartialView(courses);
        }

        [OAuthorize(AppRoles.University, AppRoles.University_Statistics)]
        public ActionResult GetSubjectsForCourse(Guid college_ID, Guid course_ID, short semester, PrintProgramme printProgramme, short batch)
        {
            ViewBag.CollegeFullName = new CollegeManager().GetItem(college_ID).CollegeFullName;
            ViewBag.CourseFullName = new CourseManager().GetCompactItem(course_ID).CourseFullName;
            ViewBag.Semester = semester;
            ViewBag.PrintProgramme = Helper.GetEnumDescription(printProgramme);
            ViewBag.Batch = batch;

            List<AwardForSubjects> subjects = new CourseManager().GetSubjectsForCourseAndCollege(college_ID, course_ID, printProgramme, semester, batch);
            return View(subjects);
        }


        private void GetResponseViewBagsForAwards()
        {
            ViewBag.Colleges = new CollegeManager().GetCollegeList().Select(college => new SelectListItem { Text = college.CollegeFullName, Value = college.College_ID.ToString() }).ToList();
            ViewBag.Programme = Helper.GetSelectList<PrintProgramme>().ToList();
            ViewBag.Semesters = Helper.GetSelectList<Semester>();
            ViewBag.Batchs = Helper.GetYearsDDL();
        }

        #endregion

        #region ReEvaluationStatistics
        [HttpGet]
        [OAuthorize(AppRoles.University)]
        public ActionResult ReEvaluationStatistics()
        {
            FillReEvaluationStatisticsViewBags();
            return View(new List<ReEvaluationStatistics>());
        }

        [HttpPost]
        [OAuthorize(AppRoles.University)]
        public ActionResult ReEvaluationStatistics(int? Semester, PrintProgramme? PrintProgramme, int? SubmittedYear, FormType? Type)
        {
            ViewBag.Semester = Semester;
            ViewBag.PrintProgramme = PrintProgramme;
            ViewBag.Year = SubmittedYear;
            ViewBag.Type = Type;
            FillReEvaluationStatisticsViewBags();
            var reEvaluations = new StatisticsManager().ReEvaluationStatistics(Semester, PrintProgramme, SubmittedYear, Type);
            return View(reEvaluations ?? new List<ReEvaluationStatistics>());
        }

        void FillReEvaluationStatisticsViewBags()
        {
            ViewBag.Semesters = new List<SelectListItem>() { new SelectListItem() { Text = "Semester-I", Value = "1", Selected = true }, new SelectListItem() { Text = "Semester-II", Value = "2" }, new SelectListItem() { Text = "Semester-III", Value = "3" }, new SelectListItem() { Text = "Semester-IV", Value = "4" } };
            ViewBag.PrintProgrammeList = Helper.GetSelectList(PrintProgramme.BED).ToList();
            ViewBag.SubmittedYear = new List<SelectListItem>() { new SelectListItem() { Text = "2019", Value = "2019", Selected = true }, new SelectListItem() { Text = "2018", Value = "2018" }, new SelectListItem() { Text = "2017", Value = "2017" } };
            ViewBag.FormTypeList = new List<SelectListItem>() { new SelectListItem() { Text = FormType.ReEvaluation.ToString(), Value = ((short)FormType.ReEvaluation).ToString() }, new SelectListItem() { Text = FormType.Xerox.ToString(), Value = ((short)FormType.Xerox).ToString() } };
        }


        #endregion

        #region PaymentStatistics

        [OAuthorize(AppRoles.University_CAO, AppRoles.University, AppRoles.University_Dean)]
        [HttpGet]
        public ActionResult PaymentStatistics()
        {
            //financial year
            int year = DateTime.Now.Year;
            DateTime fromDate = new DateTime((year - 1), 04, 01);
            DateTime toDate = new DateTime(year, 03, 31);

            ViewBag.DateFrom = fromDate;
            ViewBag.DateTo = toDate;

            ViewBag.PaymentType = Helper.GetSelectList(PaymentType.ReFund, PaymentType.Reconciled);
            ViewBag.paymentTypeDesc = "ALL";

            var paymentsList = new StatisticsManager().GetPaymentStatistics(fromDate, toDate, null);
            ViewBag.AdmissionPaymentStatistics = new StatisticsManager().GetAdmissionPaymentStatistics(fromDate, toDate);
            return View(paymentsList);
        }

        [OAuthorize(AppRoles.University_CAO, AppRoles.University, AppRoles.University_Dean)]
        [HttpPost]
        public ActionResult PaymentStatistics(DateTime? DateFrom, DateTime? DateTo, PaymentType? paymentType)
        {
            ViewBag.DateFrom = DateFrom;
            ViewBag.DateTo = DateTo;
            ViewBag.paymentTypeVal = paymentType;
            ViewBag.paymentTypeDesc = paymentType?.GetEnumDescription() ?? "ALL";

            ViewBag.PaymentType = Helper.GetSelectList(PaymentType.ReFund, PaymentType.Reconciled);

            var paymentsList = new StatisticsManager().GetPaymentStatistics(DateFrom, DateTo, paymentType);
            ViewBag.AdmissionPaymentStatistics = new StatisticsManager().GetAdmissionPaymentStatistics(DateFrom, DateTo);
            return View(paymentsList);
        }

        public ActionResult PrintProgrammeWise(Parameters parameter, PrintProgramme? printProgramme)
        {
            var list = new StatisticsManager().GetstudentsCourseWiseStatistics(parameter);

            var collegeFilter = parameter.Filters?.FirstOrDefault(i => i.Column.ToUpper() == "ACCEPTCOLLEGE_ID");
            if (collegeFilter != null)
            {
                collegeFilter.Column = "College";
                collegeFilter.Value = new CollegeManager().GetItem(Guid.Parse(collegeFilter.Value)).CollegeCode;
            }

            var courseFilter = parameter.Filters?.FirstOrDefault(i => i.Column.ToUpper() == "COURSE_ID");
            if (courseFilter != null)
            {
                courseFilter.Column = "Course";
                courseFilter.Value = new CourseManager().GetItem(Guid.Parse(courseFilter.Value)).CourseFullName;
            }

            if (parameter.Filters != null)
                ViewBag.Filters = string.Join(",", parameter.Filters.Select(i => " " + i.Column + " = " + i.Value));

            return View(list);
        }

        public ActionResult CourseWiseCSV(Parameters parameters)
        {
            List<StudentsCourseWiseStatistics> listResult = null;
            listResult = new StatisticsManager().GetstudentsCourseWiseStatistics(parameters);
            var list = listResult.Select(
                col => new
                {
                    Batch = col.SemesterBatch,
                    col.CollegeFullName,
                    col.CourseFullName,
                    col.Semester,
                    col.NoOfStudents
                }).ToList();
            ExportToCSV(list, $"Statistics Report");
            return View();
        }
        #endregion

        #region Commented code

        //[OAuthorize(AppRoles.University, AppRoles.University_Statistics, AppRoles.University_Dean)]
        //public ActionResult Student()
        //{
        //    ViewBag.Colleges = new CollegeManager().GetADMCollegeMasterList();
        //    ViewBag.Districts = new GeneralDDLManager().GetDistrictList();
        //    ViewBag.Gender = Helper.GetSelectList<Gender>();
        //    ViewBag.PrintProgrammes = Helper.GetSelectList<PrintProgramme>();
        //    GetResponseViewBags();
        //    return View();
        //}

        //[OAuthorize(AppRoles.University, AppRoles.University_Statistics, AppRoles.University_Dean)]
        //public ActionResult StudentListPartial(Parameters parameter, List<string> otherparam1)
        //{
        //    if (otherparam1.IsNullOrEmpty() || string.IsNullOrEmpty(string.Join("", otherparam1).Trim()))
        //    {
        //        otherparam1.Add("Batch");
        //    }
        //    var groupBy = string.Join(", ", otherparam1);
        //    ViewBag.GroupByClause = otherparam1 ?? new List<string>();
        //    List<StudentStatisticsSummary> list = new StatisticsManager().GetStudentStatistics2(parameter, groupBy);
        //    return PartialView(list);
        //}

        //[OAuthorize(AppRoles.University, AppRoles.University_Statistics, AppRoles.University_Dean)]
        //public ActionResult CourseCategoryWise()
        //{
        //    ViewData["PageSize"] = -1;
        //    //ViewBag.Colleges = new CollegeManager().GetADMCollegeMasterList();
        //    ViewBag.PrintProgrammes = Helper.GetSelectList<PrintProgramme>();
        //    GetResponseViewBags();
        //    return View();
        //}

        //[OAuthorize(AppRoles.University, AppRoles.University_Statistics, AppRoles.University_Dean)]
        //public ActionResult CourseCategoryWiseListPartial(Parameters parameter, PrintProgramme? otherparam1)
        //{
        //    ViewBag.Category = new CategoryManager().GetCategoryList();
        //    short semester, batch;
        //    List<CollegeCourseCategoryStatistics> list = null;
        //    if (otherparam1.HasValue && short.TryParse(parameter.Filters?.FirstOrDefault(x => x.Column == "Semester")?.Value, out semester) && short.TryParse(parameter.Filters?.FirstOrDefault(x => x.Column == "SemesterBatch")?.Value, out batch))
        //        list = new StatisticsManager().ByCollegeCourseCategory(otherparam1.Value, batch, semester);
        //    var delete_it = new StatisticsManager().GetstudentsCourseWiseStatistics(parameter);
        //    return PartialView(list);
        //}

        //[OAuthorize(AppRoles.University, AppRoles.University_Statistics, AppRoles.University_Dean)]
        //public ActionResult PrintCourseCategoryWise(Parameters parameter, PrintProgramme? printProgramme)
        //{
        //    ViewBag.Category = new CategoryManager().GetCategoryList(new Parameters());
        //    short semester = 0, batch = 0;
        //    List<CollegeCourseCategoryStatistics> list = null;
        //    if (printProgramme.HasValue & short.TryParse(parameter.Filters?.FirstOrDefault(x => x.Column == "Semester")?.Value, out semester) && short.TryParse(parameter.Filters?.FirstOrDefault(x => x.Column == "SemesterBatch")?.Value, out batch))
        //        list = new StatisticsManager().ByCollegeCourseCategory(printProgramme.Value, batch, semester);
        //    ViewBag.Programme = printProgramme.HasValue ? Helper.GetEnumDescription(printProgramme.Value) : "";
        //    ViewBag.Batch = batch;
        //    ViewBag.Semester = semester;
        //    return View(list);
        //}

        //#region GenderWise
        //[OAuthorize(AppRoles.University, AppRoles.University_Statistics, AppRoles.University_Dean)]
        //public ActionResult GenderWise()
        //{
        //    ViewBag.PrintProgrammes = Helper.GetSelectList<PrintProgramme>();
        //    return View();
        //}

        //[OAuthorize(AppRoles.University, AppRoles.University_Statistics, AppRoles.University_Dean)]
        //public ActionResult GenderWiseListPartial(Parameters parameter, PrintProgramme? otherparam1)
        //{
        //    List<StatisticsGenderWise> list = new StatisticsManager().GenderWise(PrintProgramme.UG, 000, 0);
        //    return PartialView(list);
        //}
        //public void GenderWiseCSV(PrintProgramme? printProgramme, Parameters parameter, string otherparam1)
        //{
        //    List<StatisticsGenderWise> list = new StatisticsManager().GenderWise(PrintProgramme.UG, 000, 0);
        //    var reportList = list.Select(x => new
        //    {
        //        x.Batch,
        //        x.Female,
        //        x.Male,
        //        Total = x.Female + x.Male,
        //    }).ToList();
        //    ExportToCSV(reportList, "Statistics-Gender-Wise");
        //}
        //#endregion

        //#region ProgrammGenderWise
        //[OAuthorize(AppRoles.University, AppRoles.University_Statistics, AppRoles.University_Dean)]
        //public ActionResult ProgramGenderWise()
        //{
        //    ViewBag.PrintProgrammes = Helper.GetSelectList<PrintProgramme>();
        //    return View();
        //}

        //[OAuthorize(AppRoles.University, AppRoles.University_Statistics, AppRoles.University_Dean)]
        //public ActionResult ProgramGenderWiseListPartial(Parameters parameter, PrintProgramme? otherparam1)
        //{
        //    List<StatisticsProgramGenderWise> list = new StatisticsManager().ProgramGenderWise(PrintProgramme.UG, 0, 0);
        //    return View(list);
        //}
        //public void ProgramGenderWiseCSV(PrintProgramme? printProgramme, Parameters parameter, string otherparam1)
        //{
        //    List<StatisticsProgramGenderWise> list = new StatisticsManager().ProgramGenderWise(PrintProgramme.UG, 000, 0);
        //    var reportList = list.Select(x => new
        //    {
        //        x.Batch,
        //        Programme = x.PrintProgramme == PrintProgramme.BED ? x.PrintProgramme.ToString() : x.Programme.ToString(),
        //        x.Female,
        //        x.Male,
        //        Total = x.Female + x.Male,
        //    }).ToList();
        //    ExportToCSV(reportList, "Statistics-Programme-Gender-Wise");
        //}
        //#endregion

        //#region CategoryGenderWise
        //[OAuthorize(AppRoles.University, AppRoles.University_Statistics, AppRoles.University_Dean)]
        //public ActionResult CategoryGenderWise()
        //{
        //    ViewBag.PrintProgrammes = Helper.GetSelectList<PrintProgramme>();
        //    return View();
        //}
        //[OAuthorize(AppRoles.University, AppRoles.University_Statistics, AppRoles.University_Dean)]
        //public ActionResult CategoryGenderWiseListPartial(Parameters parameter, PrintProgramme? otherparam1)
        //{
        //    List<StatisticsCategoryGenderWise> list = new StatisticsManager().CategoryGender(otherparam1.Value, 000, 0);
        //    return PartialView(list);
        //}
        //public void CategoryGenderWiseCSV(PrintProgramme? printProgramme, Parameters parameter, string otherparam1)
        //{
        //    List<StatisticsCategoryGenderWise> list = new StatisticsManager().CategoryGender(PrintProgramme.UG, 000, 0);
        //    var reportList = list.Select(x => new
        //    {
        //        x.Batch,
        //        x.CategoryCode,
        //        x.CategoryName,
        //        x.Female,
        //        x.Male,
        //        Total = x.Female + x.Male,
        //    }).ToList();
        //    ExportToCSV(reportList, "Statistics-Category-Gender-Wise");
        //}
        //#endregion



        //#region ProgrammCategoryGenderWise
        //[OAuthorize(AppRoles.University, AppRoles.University_Statistics, AppRoles.University_Dean)]
        //public ActionResult ProgramCategoryGenderWise()
        //{
        //    ViewBag.PrintProgrammes = Helper.GetSelectList<PrintProgramme>();
        //    return View();
        //}

        //[OAuthorize(AppRoles.University, AppRoles.University_Statistics, AppRoles.University_Dean)]
        //public ActionResult ProgramCategoryGenderWiseListPartial(Parameters parameter, PrintProgramme? otherparam1)
        //{
        //    List<StatisticsProgramCategoryGenderWise> list = new StatisticsManager().ProgramCategoryGenderWise(PrintProgramme.UG, 000, 0);
        //    return View(list);
        //}
        //public void ProgramCategoryGenderWiseCSV(PrintProgramme? printProgramme, Parameters parameter, string otherparam1)
        //{
        //    List<StatisticsProgramCategoryGenderWise> list = new StatisticsManager().ProgramCategoryGenderWise(PrintProgramme.UG, 000, 0);
        //    var reportList = list.Select(x => new
        //    {
        //        x.Batch,
        //        Programme = x.PrintProgramme == PrintProgramme.BED ? x.PrintProgramme.ToString() : x.Programme.ToString(),
        //        x.CategoryCode,
        //        x.CategoryName,
        //        x.Female,
        //        x.Male,
        //        Total = x.Female + x.Male,
        //    }).ToList();
        //    ExportToCSV(reportList, "Statistics-Prog-Category-Gender-Wise");
        //}
        //#endregion


        //[OAuthorize(AppRoles.University, AppRoles.University_Statistics, AppRoles.University_Dean)]
        //public ActionResult CourseDistrictCategoryWise()
        //{
        //    ViewBag.PrintProgrammes = Helper.GetSelectList<PrintProgramme>();
        //    return View();
        //}

        //[OAuthorize(AppRoles.University, AppRoles.University_Statistics, AppRoles.University_Dean)]
        //public ActionResult CourseDistrictCategoryListPartial(Parameters parameter, PrintProgramme? otherparam1)
        //{
        //    ViewBag.Category = new CategoryManager().GetCategoryList(new Parameters());
        //    short semester, batch;
        //    List<CollegeCourseDistrictCategoryStatistics> list = null;
        //    if (otherparam1.HasValue && short.TryParse(parameter.Filters?.FirstOrDefault(x => x.Column == "Semester")?.Value, out semester) && short.TryParse(parameter.Filters?.FirstOrDefault(x => x.Column == "SemesterBatch")?.Value, out batch))
        //        list = new StatisticsManager().ByCollegeCourseDistrictCategory(otherparam1.Value, batch, semester);
        //    return PartialView(list);
        //}

        //[OAuthorize(AppRoles.University, AppRoles.University_Statistics, AppRoles.University_Dean)]
        //public ActionResult PrintCourseDistrictCategory(Parameters parameter, PrintProgramme? printProgramme)
        //{
        //    ViewBag.Category = new CategoryManager().GetCategoryList();
        //    ViewBag.College = AppUserHelper.AppUsercompact.FullName;
        //    short semester = 0, batch = 0;
        //    List<CollegeCourseDistrictCategoryStatistics> list = null;
        //    if (printProgramme.HasValue && short.TryParse(parameter.Filters?.FirstOrDefault(x => x.Column == "Semester")?.Value, out semester) && short.TryParse(parameter.Filters?.FirstOrDefault(x => x.Column == "SemesterBatch")?.Value, out batch))
        //        list = new StatisticsManager().ByCollegeCourseDistrictCategory(printProgramme.Value, batch, semester);
        //    ViewBag.Programme = printProgramme.HasValue ? Helper.GetEnumDescription(printProgramme.Value) : "";
        //    ViewBag.Batch = batch;
        //    ViewBag.Semester = semester;
        //    return View(list);
        //}

        //public void StatisticCSV(Parameters parameter, string otherparam1)
        //{
        //    if (parameter == null)
        //        parameter = new Parameters();
        //    parameter.PageInfo = new Paging() { PageNumber = -1, PageSize = -1 };
        //    if (otherparam1.IsNullOrEmpty() || string.IsNullOrEmpty(otherparam1))
        //        otherparam1 = "[\"Batch\"]";

        //    List<string> showColumns = JsonConvert.DeserializeObject<List<string>>(otherparam1);
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

        #endregion

        #region SemesterEnrollmentPayments

        [OAuthorize(AppRoles.University_CAO, AppRoles.University, AppRoles.University_Dean)]
        [HttpPost]
        public ActionResult ProgrammeCollegeSemesterAdmissionPayments(DateTime? DateFrom, DateTime? DateTo, PaymentType? paymentType)
        {
            //financial year
            int year = DateTime.Now.Year;
            DateTime fromDate = DateFrom ?? new DateTime((year - 1), 04, 01);
            DateTime toDate = DateTo ?? new DateTime(year, 03, 31);

            DataTable dt = new StatisticsManager().GetProgrammeCollegeSemesterAdmissionPayments(fromDate, toDate, paymentType) ?? new DataTable();
            return DownloadExcel(dt, $"SemAdmPaymentDetails_from_{fromDate.ToString("dd-MMMM-yyyy")} to {toDate.ToString("dd-MMMM-yyyy")}");
        }

        [OAuthorize(AppRoles.University_CAO, AppRoles.University, AppRoles.University_Dean)]
        [HttpPost]
        public ActionResult CourseCollegeSemesterAdmissionPayments(DateTime? DateFrom, DateTime? DateTo, PaymentType? paymentType)
        {
            //financial year
            int year = DateTime.Now.Year;
            DateTime fromDate = DateFrom ?? new DateTime((year - 1), 04, 01);
            DateTime toDate = DateTo ?? new DateTime(year, 03, 31);

            DataTable dt = new StatisticsManager().GetCourseCollegeSemesterAdmissionPayments(fromDate, toDate, paymentType) ?? new DataTable();
            return DownloadExcel(dt, $"CourseWiseSemAdmPaymentDetails_from_{fromDate.ToString("dd-MMMM-yyyy")} to {toDate.ToString("dd-MMMM-yyyy")}");
        }

        [OAuthorize(AppRoles.University_CAO, AppRoles.University, AppRoles.University_Dean)]
        [HttpPost]
        public ActionResult IndividualAdmissionPayments(DateTime? DateFrom, DateTime? DateTo, PaymentType? paymentType)
        {
            //financial year
            int year = DateTime.Now.Year;
            DateTime fromDate = DateFrom ?? new DateTime((year - 1), 04, 01);
            DateTime toDate = DateTo ?? new DateTime(year, 03, 31);

            DataTable dt = new StatisticsManager().GetIndividualAdmissionPayments(fromDate, toDate, paymentType) ?? new DataTable();
            return DownloadExcel(dt, $"IndividualAdmissionPayments_from_{fromDate.ToString("dd-MMMM-yyyy")} to {toDate.ToString("dd-MMMM-yyyy")}");
        }
        #endregion
    }
}