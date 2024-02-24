
using CaptchaMvc.HtmlHelpers;
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
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;


namespace CUSrinagar.WebApp.CUSrinagarAdminPanel.Controllers
{
    [OAuthorize(AppRoles.University_Registrar, AppRoles.University, AppRoles.University_Statistics)]
    public class SeatAllocationController : AdminController
    {
        public ActionResult GenerateList()
        {
            SetViewBags();
            return View();
        }

        public async Task<ActionResult> FetchList(Parameters parameter)
        {
            //int currentList = 0;
            SelectedStudentDetail SelectedStudentDetail = new SelectedStudentDetail();
            if (parameter == null)
            { return View(new SelectedStudentDetail()); }

            string programme = parameter.Filters.FirstOrDefault(x => x.Column.ToLower() == "programme")?.Value ?? "1";

            Tuple<SelectedStudentDetail, int> reslt;

            if (programme == ((int)PrintProgramme.IH).ToString() || programme == ((int)Programme.Engineering).ToString())
            {
                reslt = (await new StudentManager().GetSelectedStudentsNEdPIHAsync(parameter));
                SelectedStudentDetail = reslt.Item1;
                ViewBag.OMSeats = 30;
            }
            else if (programme == ((int)PrintProgramme.PG).ToString())
            {
                reslt = (await new StudentManager().GetSelectedStudentsPGAsync(parameter));
                SelectedStudentDetail = reslt.Item1;
                ViewBag.OMSeats = 17;
            }
            else
            {
                reslt = (await new StudentManager().GetSelectedStudentsNEdPUGAsync(parameter));
                SelectedStudentDetail = reslt.Item1;
                ViewBag.OMSeats = 30;
            }
            ViewBag.CurrentListNo = reslt.Item2;
            return View(SelectedStudentDetail);
        }


        public void SetViewBags()
        {
            IEnumerable<SelectListItem> ProgrammeDDL = Helper.GetSelectList<Programme>();
            IEnumerable<SelectListItem> CourseDDL = new CourseManager().GetAllCoursesByProgramme(Convert.ToInt32(ProgrammeDDL.FirstOrDefault().Value), 1, true, checkSelectionlistAllowed: true);
            ViewBag.ProgrammeDDLList = ProgrammeDDL == null ? new List<SelectListItem>() : ProgrammeDDL;
            ViewBag.CourseDDLList = CourseDDL == null ? new List<SelectListItem>() : CourseDDL;
            ViewBag.CollegeDDLList = new CollegeManager().GetADMCollegeMasterList();
        }
        public bool ReleaseORAdmitSeat(Programme Programme, Guid Course_ID, int Batch, Guid Student_ID, StudentSelectionStatus StudentSelectionStatus)
        {
            Parameters parameter = new Parameters()
            {
                Filters = new List<SearchFilter>() { new SearchFilter() {  Column= "Programme", Value= Programme.ToString() }
                                                    ,new SearchFilter() {  Column= "Course_ID", Value= Course_ID.ToString() }
                                                    ,new SearchFilter() {  Column= "Batch", Value= Batch.ToString() }
                                                    }
            };
            string inClause = $" IN ('{Student_ID}') ";
            var _return = new StudentManager().UpdateStudentSelectionStatus(inClause, StudentSelectionStatus, parameter) > 0;
            try
            {
                if (StudentSelectionStatus == StudentSelectionStatus.Provisional)
                {
                    new StudentManager().Save(new AllowInSemesterAdm { AdditionalFee = 0, AllowForSemesterAdm = 1, CUSRegistrationNoToAllow = new RegistrationManager().GetStudent(Student_ID, PrintProgramme.IH).StudentFormNo });
                }
                else if (StudentSelectionStatus == StudentSelectionStatus.Rejected)
                {
                    new StudentManager().DeleteAllowInSemesterAdm(new RegistrationManager().GetStudent(Student_ID, PrintProgramme.IH).StudentFormNo, 1);
                }
            }
            catch
            {

            }
            return _return;
        }


        public async Task AllocationListCSV(Parameters parameter)
        {
            List<SelectedStudentInfo> mainList = new List<SelectedStudentInfo>();

            //int currentLists = 0;

            string CourseName = string.Empty;

            if (parameter != null)
            {
                string batch = parameter.Filters.FirstOrDefault(x => x.Column.ToLower().Trim() == "batch")?.Value ?? "0";
                int.TryParse(batch, out int batchToSearch);

                if (batchToSearch >= 2022)
                {
                    string programme = parameter.Filters.FirstOrDefault(x => x.Column.ToLower() == "programme")?.Value ?? "1";

                    if (programme == ((int)PrintProgramme.IH).ToString())
                    {
                        mainList = (await new StudentManager().GetSelectedStudentsNEdPIHAsync(parameter)).Item1?.selectedStudentInfo ?? new List<SelectedStudentInfo>();
                    }
                    else
                    {
                        mainList = (await new StudentManager().GetSelectedStudentsNEdPUGAsync(parameter)).Item1?.selectedStudentInfo ?? new List<SelectedStudentInfo>();
                    }
                    if (mainList.IsNotNullOrEmpty())
                    {
                        var finalList = mainList.Select(col => new
                        {
                            col.StudentFormNo,
                            col.FullName,
                            col.FathersName,
                            col.Category,
                            col.Percentage,
                            col.Preference,
                        }).ToList();

                        if (finalList.IsNotNullOrEmpty())
                        {
                            CourseName = new CourseManager().GetCompactItem(new Guid(parameter?.Filters?.First(x => x.Column.ToLower().Trim() == "course_id").Value ?? Guid.Empty.ToString())).CourseFullName;
                        }

                        ExportToCSV(finalList, $"Selection List of {CourseName.Replace("/", "-")}");
                    }
                }
                else
                {
                    mainList = new StudentManager().GetSelectedStudents(parameter, false, out int TOMSeats, out int currentList).selectedStudentInfo ?? new List<SelectedStudentInfo>();
                    var finalList = mainList.Select(col => new
                    {
                        col.Sno,
                        col.EntranceRollNo,
                        col.FullName,
                        col.Category,
                        col.TotalPoints,
                        col.StudentSelectionStatus
                    }).ToList();

                    if (finalList.IsNotNullOrEmpty())
                    {
                        CourseName = new CourseManager().GetCompactItem(new Guid(parameter?.Filters?.First(x => x.Column.ToLower().Trim() == "course_id").Value ?? Guid.Empty.ToString())).CourseFullName;
                    }

                    ExportToCSV(finalList, $"Selection List of {CourseName.Replace("/", "-")}");
                }
            }
            else
            {
                HttpContext.Response.End();
            }
        }


        public PartialViewResult _GetChildDDL(Programme Programme)
        {
            ViewBag.Type = "Course_ID";
            ViewBag.ChildType = "";
            ViewBag.ChildSubType = "";
            ViewBag.ChildValues = new List<SelectListItem>();
            ViewBag.ChildValues = new CourseManager().GetAllCoursesByProgramme((int)Programme, 1, true, checkSelectionlistAllowed: true);
            return PartialView();
        }


        public ActionResult UpdateStudentStatus()
        {
            ViewBags(null);
            return View();
        }
        [HttpPost]
        public ActionResult UpdateStudentStatus(UpdateStudentStatus updateStudentStatus)
        {
            if (ModelState.IsValid)
            {
                if (new StudentManager().UpdateStudentSelectionStatus(updateStudentStatus) > 0)
                    TempData["response"] = "<div class='alert alert-success alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>Updated Successfully</a></div>";
                else
                    TempData["response"] = "<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>Some Error Occured</a></div>";
            }
            ViewBags(updateStudentStatus);
            return View(updateStudentStatus);
        }
        private void ViewBags(UpdateStudentStatus updateStudentStatus)
        {
            ViewBag.Programme = Helper.GetSelectList<Programme>().OrderBy(x => x.Value);
            ViewBag.SelectionStatus = Helper.GetSelectList<StudentSelectionStatus>().OrderBy(x => x.Value);
            if (updateStudentStatus != null && (int)updateStudentStatus.Programme != 0)
            { ViewBag.Courses = new CourseManager().GetAllCoursesByProgramme((int)updateStudentStatus.Programme); }
            else ViewBag.Courses = new List<SelectListItem>();
        }
        [HttpPost]
        public PartialViewResult _GetCoursesDDL(Programme programme)
        {
            return PartialView(new CourseManager().GetAllCoursesByProgramme((int)programme));
        }

        [HttpPost]
        public int _GetCoursesListNo(Guid Course_ID)
        {
            ViewBag.ListNo = 0;
            return new CourseManager().GetCourseById(Course_ID)?.CurrentSelectionListNo ?? 0;
        }
        public ActionResult UpdateSelectionList()
        {
            ListViewBag(null);
            return View();
        }
        [HttpPost]
        public ActionResult UpdateSelectionList(ADMCourseMaster aDMCourseMaster)
        {
            if (ModelState.IsValid)
            {
                if (new StudentManager().UpdateSelectionList(aDMCourseMaster) > 0)
                    TempData["response"] = "<div class='alert alert-success alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>Updated Successfully</a></div>";
                else
                    TempData["response"] = "<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>Some Error Occured</a></div>";
            }
            ListViewBag(aDMCourseMaster);
            return View(aDMCourseMaster);
        }
        private void ListViewBag(ADMCourseMaster aDMCourseMaster)
        {
            ViewBag.Programme = Helper.GetSelectList<Programme>().OrderBy(x => x.Value);
            if (aDMCourseMaster != null && (int)aDMCourseMaster.Programme != 0)
            {
                ViewBag.Courses = new CourseManager().GetAllCoursesByProgramme((int)aDMCourseMaster.Programme);
                ViewBag.ListNo = new CourseManager().GetCourseById(aDMCourseMaster.Course_ID)?.CurrentSelectionListNo ?? 0;
            }
            else
            {
                ViewBag.Courses = new List<SelectListItem>();
                ViewBag.ListNo = 0;
            }
        }
        public JsonResult IsSelected(Guid Student_ID, PrintProgramme printProgramme)
        {
            ResponseData response = new StudentManager().IsSelected(Student_ID, printProgramme);
            return Json(response, JsonRequestBehavior.AllowGet);
        }


    }
}