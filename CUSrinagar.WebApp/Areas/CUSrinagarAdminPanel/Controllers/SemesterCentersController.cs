using CUSrinagar.BusinessManagers;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using CUSrinagar.Models;
using CUSrinagar.OAuth;
using GeneralModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CUSrinagar.WebApp.CUSrinagarAdminPanel.Controllers
{
    [OAuthorize(AppRoles.University, AppRoles.University_EController)]
    public class SemesterCentersController : UniversityAdminBaseController
    {
        string errorMsg = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>##</a></div>";

        #region viewbags
        private void SetViewBags()
        {
            List<SelectListItem> Colleges = new CollegeManager().GetADMCollegeMasterList();

            List<SelectListItem> CreateCenters = new List<SelectListItem>();
            CreateCenters.AddRange(Colleges);
            ViewBag.CreateCenters = CreateCenters;

            Colleges.Insert(0, new SelectListItem { Text = "All", Value = Guid.Empty.ToString() });
            ViewBag.Colleges = Colleges;

            ViewBag.Programmes = Helper.GetSelectList<Programme>();
            ViewBag.ProgrammesCountDDL = Helper.GetSelectList<Programme>();

            IEnumerable<SelectListItem> ProgrammesMerge = Helper.GetSelectList(Programme.HS, Programme.Professional);
            ProgrammesMerge.First(x => x.Value == "3").Text = "Integrated / Honor's / Professional";
            ViewBag.ProgrammesMergeDDL = ProgrammesMerge;

            ViewBag.ExamFormListTypeDDL = Helper.GetSelectList<ExamFormListType>();

            ViewBag.ExamFormsSettingsDDL = new SemesterCentersManager().SelectExamFormsSettingsDDL();
        }

        private void SetCenterViewBags()
        {
            List<SelectListItem> Colleges = new CollegeManager().GetADMCollegeMasterList();
            ViewBag.CenterLocation = Colleges;

            ViewBag.ExamFormsSettingsDDL = new SemesterCentersManager().SelectExamFormsSettingsDDL();

            ViewBag.Courses = new List<SelectListItem>();
            ViewBag.SemesterCenterCategory = Helper.GetSelectList<SemesterCentersCategory>();
        }

        private void SetCenterViewBagsReLocate()
        {
            ViewBag.FromCCenterList = new SemesterCentersManager().GetRelocateCenterList();
            ViewBag.ToCCenterList = new EntranceCentersManager().GetCenterList(false);
            ViewBag.CCourses = new List<SelectListItem>();
        }
        #endregion

        #region CenterCRUD       

        [HttpGet]
        public ActionResult Centers()
        {
            SetViewBags();
            ViewBag.Data = new SemesterCentersManager().GetSemesterCentersMaster(Guid.Empty);
            return View();
        }

        [HttpPost]
        public PartialViewResult _DisplayCenters(Guid College_ID)
        {
            return PartialView("_DisplayCenters", new SemesterCentersManager().GetSemesterCentersMaster(College_ID));
        }

        [HttpPost]
        public JsonResult ChangeCenterStatus(Guid Center_ID) =>
            Json(new SemesterCentersManager().ChangeCenterStatus(Center_ID) > 0 ? true : false, JsonRequestBehavior.DenyGet);

        [HttpPost]
        public JsonResult DeleteCenter(Guid Center_ID) =>
            Json(new SemesterCentersManager().DeleteCenter(Center_ID) > 0 ? true : false, JsonRequestBehavior.DenyGet);

        [HttpPost]
        public JsonResult Create(SemesterCenters centersToCreate)
        {
            if (!ModelState.IsValid)
                return Json(false, JsonRequestBehavior.DenyGet);
            else
                return Json(new SemesterCentersManager().CreateCenters(centersToCreate) > 0 ? true : false, JsonRequestBehavior.DenyGet);
        }
        #endregion


        #region GenerateCenterNotice
        [HttpGet]
        public ActionResult DownloadCenterNotices()
        {
            return RedirectToAction("Centers");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DownloadCenterNotices(SemesterCenters _RePrintCenterNotice)
        {
            ModelState.Remove(nameof(_RePrintCenterNotice.PipeSeparatedCenters));
            ModelState.Remove(nameof(_RePrintCenterNotice.College_ID));
            if (!ModelState.IsValid)
                return RedirectToAction("Centers");

            List<CenterNotice> list = new SemesterCentersManager().GetCenterNoticeDetails(_RePrintCenterNotice.PrintCenterNotice);
            TempData["response"] = null;
            if (list.IsNullOrEmpty())
            {
                TempData["response"] = @"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> No details were found.</div>";
                return RedirectToAction("Centers");
            }
            return View(list);
        }


        [HttpGet]
        public ActionResult DownloadArchivedCenterNotices()
        {
            return RedirectToAction("Centers");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DownloadArchivedCenterNotices(SemesterCenters _RePrintCenterNotice)
        {
            ModelState.Remove(nameof(_RePrintCenterNotice.PipeSeparatedCenters));
            ModelState.Remove(nameof(_RePrintCenterNotice.College_ID));
            if (!ModelState.IsValid)
                return RedirectToAction("Centers");

            List<CenterNotice> list = new SemesterCentersManager().GetArchivedCenterNoticeDetails(_RePrintCenterNotice.PrintCenterNotice);
            TempData["response"] = null;
            if (list.IsNullOrEmpty())
            {
                TempData["response"] = @"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> No details were found.</div>";
                return RedirectToAction("Centers");
            }
            return View("DownloadCenterNotices", list);
        }

        #endregion


        #region Generate Excels
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DownloadCenterWiseSubjectCount(SemesterCenters _RePrintCenterNotice)
        {
            ModelState.Remove(nameof(_RePrintCenterNotice.PipeSeparatedCenters));
            ModelState.Remove(nameof(_RePrintCenterNotice.College_ID));
            if (!ModelState.IsValid)
                return RedirectToAction("Centers");
            DataTable CenterWiseSubjectCount = await new SemesterCentersManager().GetCenterWiseSubjectCountAsync(_RePrintCenterNotice.PrintCenterNotice);

            if (CenterWiseSubjectCount == null || CenterWiseSubjectCount?.Rows.Count <= 0)
            {
                TempData["response"] = @"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> No details were found.</div>";
                return RedirectToAction("Centers");
            }

            return DownloadExcel(CenterWiseSubjectCount, $"{_RePrintCenterNotice.PrintCenterNotice.ListType.ToString()}_CenterWiseSubjectCount{_RePrintCenterNotice.PrintCenterNotice.CourseCategory.ToString()}_Sem-{_RePrintCenterNotice.PrintCenterNotice.Semester}_ExamYear-{_RePrintCenterNotice.PrintCenterNotice.ExaminationYear}");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DownloadOverAllExamFormSubjectWiseCount(SemesterCenters _RePrintCenterNotice)
        {
            ModelState.Remove(nameof(_RePrintCenterNotice.PipeSeparatedCenters));
            ModelState.Remove(nameof(_RePrintCenterNotice.College_ID));

            if (!ModelState.IsValid)
                return RedirectToAction("Centers");
            DataTable CenterWiseSubjectCount = await new SemesterCentersManager().GetOverAllExamFormSubjectWiseCountAsync(_RePrintCenterNotice.PrintCenterNotice);

            if (CenterWiseSubjectCount == null || CenterWiseSubjectCount?.Rows.Count <= 0)
            {
                TempData["response"] = @"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> No details were found.</div>";
                return RedirectToAction("Centers");
            }
            return DownloadExcel(CenterWiseSubjectCount, $"{_RePrintCenterNotice.PrintCenterNotice.ListType.ToString()}_CollegeWiseAndSubjectWiseExamFormCount{_RePrintCenterNotice.PrintCenterNotice.CourseCategory.ToString()}_Sem-{_RePrintCenterNotice.PrintCenterNotice.Semester}_ExamYear-{_RePrintCenterNotice.PrintCenterNotice.ExaminationYear}");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DownloadCenterWiseExamFormCount(SemesterCenters _RePrintCenterNotice)
        {
            ModelState.Remove(nameof(_RePrintCenterNotice.PipeSeparatedCenters));
            ModelState.Remove(nameof(_RePrintCenterNotice.College_ID));
            if (!ModelState.IsValid)
                return RedirectToAction("Centers");
            DataTable CenterWiseExamFormCount = await new SemesterCentersManager().GetCourseWiseExamFormCountAsync(_RePrintCenterNotice.PrintCenterNotice);

            if (CenterWiseExamFormCount == null || CenterWiseExamFormCount?.Rows.Count <= 0)
            {
                TempData["response"] = @"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> No details were found.</div>";
                return RedirectToAction("Centers");
            }

            DataTable newTable = CenterWiseExamFormCount.Clone();//get headers only
            int TotalBacklogStudents = 0;
            int TotalRegularStudents = 0;
            int Total = 0;
            foreach (var item in CenterWiseExamFormCount.AsEnumerable().GroupBy(x => x["College FullName"]))
            {
                TotalBacklogStudents = 0;
                TotalRegularStudents = 0;
                Total = 0;
                foreach (var item2 in item)
                {
                    newTable.ImportRow(item2);
                    TotalRegularStudents += Convert.ToInt32("0" + item2.ItemArray[2]);
                    TotalBacklogStudents += Convert.ToInt32("0" + item2.ItemArray[3]);
                    Total += Convert.ToInt32("0" + item2.ItemArray[4]);
                }
                DataRow row = newTable.NewRow();
                row[0] = "";
                row[1] = "TOTAL";
                row[2] = TotalRegularStudents;
                row[3] = TotalBacklogStudents;
                row[4] = Total;
                newTable.Rows.Add(row);
                newTable.Rows.Add(newTable.NewRow());
            }
            CenterWiseExamFormCount.Dispose();

            return DownloadExcel(newTable, $"CourseWiseExamFormCount_{_RePrintCenterNotice.PrintCenterNotice.CourseCategory.ToString()}_Sem-{_RePrintCenterNotice.PrintCenterNotice.Semester}_ExamYear-{_RePrintCenterNotice.PrintCenterNotice.ExaminationYear}");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DownloadWithoutExamFormSubjectWiseCount(SemesterCenters _RePrintCenterNotice)
        {
            ModelState.Remove(nameof(_RePrintCenterNotice.PipeSeparatedCenters));
            ModelState.Remove(nameof(_RePrintCenterNotice.College_ID));
            ModelState.Remove("PrintCenterNotice.ExaminationYear");

            if (!ModelState.IsValid)
                return RedirectToAction("Centers");
            DataTable CenterWiseSubjectCount = await new SemesterCentersManager().GetWithoutExamFormSubjectWiseCountAsync(_RePrintCenterNotice.PrintCenterNotice);

            if (CenterWiseSubjectCount == null || CenterWiseSubjectCount?.Rows.Count <= 0)
            {
                TempData["response"] = @"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> No details were found.</div>";
                return RedirectToAction("Centers");
            }
            return DownloadExcel(CenterWiseSubjectCount, $"WithoutExamFormSubjectWiseCount{_RePrintCenterNotice.PrintCenterNotice.CourseCategory.ToString()}_Sem-{_RePrintCenterNotice.PrintCenterNotice.Semester}");
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DownloadStudentDetailsWithExamFormSubjects(SemesterCenters _RePrintCenterNotice)
        {
            ModelState.Remove(nameof(_RePrintCenterNotice.PipeSeparatedCenters));
            ModelState.Remove(nameof(_RePrintCenterNotice.College_ID));

            if (!ModelState.IsValid)
                return RedirectToAction("Centers");
            DataTable StudentDetilsbyExamForm = await new SemesterCentersManager().GetStudentDetailsWithExamFormSubjectsAsync(_RePrintCenterNotice.PrintCenterNotice);

            if (StudentDetilsbyExamForm == null || StudentDetilsbyExamForm?.Rows.Count <= 0)
            {
                TempData["response"] = @"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> No details were found.</div>";
                return RedirectToAction("Centers");
            }
            string fileName = $"{_RePrintCenterNotice.PrintCenterNotice.ListType.ToString()}_StudentDetailsWithExamFormSubs{_RePrintCenterNotice.PrintCenterNotice.CourseCategory.ToString()}_Sem-{_RePrintCenterNotice.PrintCenterNotice.Semester}_ExamYear-{_RePrintCenterNotice.PrintCenterNotice.ExaminationYear}";

            DataRow dr = StudentDetilsbyExamForm.NewRow();
            dr[0] = "Downloaded On " + DateTime.Now;
            StudentDetilsbyExamForm.Rows.InsertAt(dr, 0);

            var dataRows = StudentDetilsbyExamForm.AsEnumerable().ToList().Select(x => new
            {
                CollegeFullName = x["College FullName"].ToString(),
                FormNumber = x["FormNumber"].ToString(),
                ExamRollNumber = x["ExamRollNumber"].ToString(),
                CourseFullName = x["Course FullName"].ToString(),
                ClassRollNo = x["ClassRollNo"].ToString(),
                FullName = x["FullName"].ToString(),
                Mobile = x["Mobile"].ToString(),
                CUSRegistrationNo = x["CUSRegistrationNo"].ToString(),
                Semester = x["Semester"].ToString(),
                Subject = x["Subject"].ToString(),
                SubjectType = x["SubjectType"].ToString(),
                Batch = x["SemesterBatch"].ToString(),
                CombinationCode = x["CombinationCode"].ToString(),
                FormType = x["FormType"].ToString(),
                CenterCode = x["CenterCode"].ToString(),
                CenterLocation = x["Center Location"].ToString(),
                SubmittedFrmOn = x["SubmittedFrmOn"].ToString(),
            }).ToList();

            ExportToCSV(dataRows, fileName);
            return new EmptyResult();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DownloadCenterCollegeMapping(SemesterCenters _RePrintCenterNotice)
        {
            ModelState.Remove(nameof(_RePrintCenterNotice.PipeSeparatedCenters));
            ModelState.Remove(nameof(_RePrintCenterNotice.College_ID));
            if (!ModelState.IsValid)
                return RedirectToAction("Centers");
            DataTable CenterCollegeMapping = await new SemesterCentersManager().GetCenterCollegeMappingAsync(_RePrintCenterNotice.PrintCenterNotice);

            if (CenterCollegeMapping == null || CenterCollegeMapping?.Rows.Count <= 0)
            {
                TempData["response"] = @"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> No details were found.</div>";
                return RedirectToAction("Centers");
            }

            return DownloadExcel(CenterCollegeMapping, $"CenterCollegeMapping_{_RePrintCenterNotice.PrintCenterNotice.CourseCategory.ToString()}_Sem-{_RePrintCenterNotice.PrintCenterNotice.Semester}_ExamYear-{_RePrintCenterNotice.PrintCenterNotice.ExaminationYear}");
        }

        #endregion


        #region AssignRollNosBulk

        [HttpPost]
        public async Task<JsonResult> AssignRollNosInBulk(Guid ExamSetting_ID)
        {
            if (ExamSetting_ID == Guid.Empty)
                return Json(false, JsonRequestBehavior.DenyGet);

            var response = await new SemesterCentersManager().AssignRollNosInBulkAsync(ExamSetting_ID);
            return Json(new { result = response.Item1, msg = response.Item2 }, JsonRequestBehavior.DenyGet);
        }

        #endregion


        #region AssignCentersBulk

        [HttpGet]
        public ActionResult AssignCentersInBulk()
        {
            SetCenterViewBags();
            SetCenterViewBagsReLocate();
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> AssignCentersInBulk(AssignSemesterCenters CentersToAssign)
        {
            if (!ModelState.IsValid)
            {
                return Json(errorMsg.Replace("##", "Invalid Details"), JsonRequestBehavior.DenyGet);
            }

            Tuple<bool, string> response =
                await new SemesterCentersManager().AssignCentersInBulkAsync(CentersToAssign);

            if (response.Item1)
                return Json(errorMsg.Replace("##", response.Item2).Replace("alert-danger", "alert-success"), JsonRequestBehavior.DenyGet);
            else
            {
                return Json(errorMsg.Replace("##", response.Item2), JsonRequestBehavior.DenyGet);
            }
        }


        [HttpPost]
        public PartialViewResult _GetCoursesDDL(Guid Id, Guid Id1)
        {
            return PartialView(new SemesterCentersManager().GetCoursesDDL(Id, Id1));
        }

        [HttpPost]
        public PartialViewResult _GetCoursesDDLForBulkAssign(Guid Id, Guid Id1)
        {
            return PartialView(new SemesterCentersManager().GetCoursesDDL(Id, Id1));
        }

        [HttpPost]
        public PartialViewResult _DispalyAvaliableCenters(AssignSemesterCenters CentersToAssign)
        {
            if (!ModelState.IsValid)
                return PartialView(new AssignSemesterCenters());

            return PartialView(new SemesterCentersManager().GetDispalyAvaliableCenters(CentersToAssign));
        }


        [HttpPost]
        public JsonResult DeleteDataFromCenterAllotment()
        {
            int result = new SemesterCentersManager().DeleteDataFromCenterAllotment(true);

            if (result == -1)
                return Json("Some courses are still having Download Admit Cards Open. Cannot Delete", JsonRequestBehavior.DenyGet);
            if (result == 0)
                return Json("Already deleted", JsonRequestBehavior.DenyGet);
            else
                return Json(true, JsonRequestBehavior.DenyGet);
        }
        #endregion


        #region IndividualCenterAllotment
        private void SetIndividualViewBags()
        {
            ViewBag.PirntProgramme = Helper.GetSelectList<PrintProgramme>();
            ViewBag.CenterList = new EntranceCentersManager().GetCenterList(false);
        }

        [HttpGet]
        public ActionResult Individual()
        {
            SetIndividualViewBags();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Individual(string ExamFormNo, PrintProgramme PrintProgramme, string command)
        {
            SetIndividualViewBags();
            if ((command + "").Trim().ToLower() == "delete")
            {
                Tuple<bool, string> FormDetail = new SemesterCentersManager().DeleteCenterOfIndividual(ExamFormNo, PrintProgramme);
                if (FormDetail.Item1)
                {
                    ViewBag.FormDetail = null;
                    TempData["response"] = errorMsg.Replace("##", FormDetail.Item2).Replace("alert-danger", "alert-success");
                }
                else
                {
                    ViewBag.FormDetail = null;
                    TempData["response"] = errorMsg.Replace("##", FormDetail.Item2);
                }
            }
            else
            {
                Tuple<bool, string, IndividualSemDetails> FormDetail = new SemesterCentersManager().SearchIndividual(ExamFormNo, PrintProgramme);
                if (FormDetail.Item1)
                {
                    ViewBag.FormDetail = FormDetail;
                }
                else
                {
                    ViewBag.FormDetail = null;
                    TempData["response"] = errorMsg.Replace("##", FormDetail.Item2);
                }
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveIndividualCenter(string ExamFormNo, PrintProgramme printProgramme, Guid CenterToAssign, string ExamRollNoToSet)
        {
            Tuple<bool, string> FormDetail = new SemesterCentersManager().SaveIndividualCenter(ExamFormNo, printProgramme, CenterToAssign, ExamRollNoToSet);

            if (FormDetail.Item1)
                TempData["response"] = errorMsg.Replace("alert-danger", "alert-success").Replace("##", FormDetail.Item2);
            else
                TempData["response"] = errorMsg.Replace("##", FormDetail.Item2);

            return RedirectToAction("Individual");
        }
        #endregion

        #region Bulk Admit Cards

        [HttpGet]
        public ActionResult BulkAdmitCards()
        {
            ViewBag.PrintProgrammeOption = new AttendanceSheetManager().GetPrintProgrammeDDL();
            ViewBag.Colleges = new CollegeManager().GetADMCollegeMasterList();
            ViewBag.CourseList = new List<SelectListItem>();
            ViewBag.Centers = new List<SelectListItem>();
            ViewBag.SemesterList = new List<SelectListItem> {
                new SelectListItem{ Text="Semester-1",Value="1"},
                new SelectListItem{ Text="Semester-2",Value="2"},
                new SelectListItem{ Text="Semester-3",Value="3"},
                new SelectListItem{ Text="Semester-4",Value="4"},
                new SelectListItem{ Text="Semester-5",Value="5"},
                new SelectListItem{ Text="Semester-6",Value="6"},
                new SelectListItem{ Text="Semester-7",Value="7"},
                new SelectListItem{ Text="Semester-8",Value="8"},
                new SelectListItem{ Text="Semester-9",Value="9"},
                new SelectListItem{ Text="Semester-10",Value="10"},
            };
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> DownloadAdmitCardsPreview(string id, Guid? id1, string sem, string College_ID, PrintProgramme? programme)
        {
            Guid Course_ID = id1 ?? Guid.Empty;
            if (!int.TryParse(id, out int Batch) || !int.TryParse(sem, out int Semester) || !Guid.TryParse(College_ID, out Guid College_Id) || programme == null)
                return RedirectToAction("BulkAdmitCards", "SemesterCenters", new { area = "CUSrinagarAdminPanel" });

            List<AttendanceSheetForm> AttendanceSheets = await new SemesterCentersManager().GetAdmitCardsAsync((PrintProgramme)programme, Course_ID, Batch, Semester, College_Id);
            return View(AttendanceSheets);
        }



        [HttpGet]
        public async Task<ActionResult> AttendanceSheetsPreview(string y, Guid? course_id, string sem, string College_ID, PrintProgramme? programme, Guid? CC)
        {
            Guid Course_ID = course_id ?? Guid.Empty;
            Guid CenterCode = CC ?? Guid.Empty;

            if (!int.TryParse(y, out int Batch) || !int.TryParse(sem, out int Semester) || !Guid.TryParse(College_ID, out Guid College_Id) || programme == null || programme == 0)
                return RedirectToAction("BulkAdmitCards", "SemesterCenters", new { area = "CUSrinagarAdminPanel" });

            List<AttendanceSheetForm> AttendanceSheets = await new AttendanceSheetManager().GetAttendanceSheetsAsync((PrintProgramme)programme, Course_ID, Batch, Semester, "", CenterCode, College_Id);

            if (AttendanceSheets.IsNotNullOrEmpty())
                ViewBag.CollegeCode = new CollegeManager().GetItem(College_Id)?.CollegeCode ?? "";

            return View(AttendanceSheets);
        }

        public PartialViewResult _GetChildDDL(Guid College_ID, string id, string Type, string childType, string childSubType)
        {
            ViewBag.Type = Type;
            ViewBag.ChildType = childType;
            ViewBag.ChildSubType = childSubType;
            ViewBag.ChildValues = new AttendanceSheetManager().FetchChildDDlValues2(College_ID, id, Type);
            return PartialView();
        }

        public PartialViewResult _GetCenterList(Guid College_ID, Guid Course_ID, PrintProgramme printProgramme, short examYear, short semester)
        {
            if (College_ID == Guid.Empty || Course_ID == Guid.Empty || printProgramme == 0 || examYear < 2017 || semester < 1)
                ViewBag.CenterList = new List<SelectListItem>();
            else
            {
                ViewBag.CenterList = new SemesterCentersManager().GetCenterList(College_ID, Course_ID, printProgramme, examYear, semester);
            }
            return PartialView();
        }





        [HttpGet]
        public async Task<ActionResult> DownloadAdmitCardAndAttendanceSheet(string ExamFormNo, PrintProgramme? printProgramme)
        {
            AttendanceSheetForm attendance = null;

            if (printProgramme != null)
                attendance = await new SemesterCentersManager().GetStudentAdmitCardAndAttendanceSheetAsync((PrintProgramme)printProgramme, ExamFormNo);

            return View(attendance);
        }
        #endregion

        #region RelocateCenter
        [HttpPost]
        public PartialViewResult _GetCoursesRCDDL(Guid Id)
        {
            return PartialView(new SemesterCentersManager().GetRelocateCenterCourseIds(Id) ?? new List<SelectListItem>());
        }


        [HttpPost]
        public PartialViewResult _GetCoursesACDDL()
        {
            return PartialView(new SemesterCentersManager().GetArchiveCenterCourseIds() ?? new List<SelectListItem>());
        }


        [HttpPost]
        public JsonResult RelocateCenter(RelocateCenter relocateCenter)
        {
            if (ModelState.IsValid)
            {
                Tuple<bool, string> response = new SemesterCentersManager().RelocateCenter(relocateCenter);
                return Json(new { Issucces = response.Item1, msg = response.Item2 }, JsonRequestBehavior.DenyGet);
            }
            else
            {
                return Json(new { Issucces = false, msg = "Invalid Details Found" }, JsonRequestBehavior.DenyGet);
            }
        }

        [HttpPost]
        public JsonResult ArchiveCenters(ArchiveCenter archiveCenters)
        {
            if (ModelState.IsValid)
            {
                Tuple<bool, string> response = new SemesterCentersManager().ArchiveCenters(archiveCenters);
                return Json(new { Issucces = response.Item1, msg = response.Item2 }, JsonRequestBehavior.DenyGet);
            }
            else
            {
                return Json(new { Issucces = false, msg = "Invalid Details Found" }, JsonRequestBehavior.DenyGet);
            }
        }
        #endregion

        #region Download Subject Enrollments

        [HttpGet]
        public PartialViewResult _DDLCourse(Guid? College_ID, Programme? programme)
        {
            PrintProgramme printProgramme = CourseManager.ProgrammeToPrintProgrammeMapping(programme ?? Programme.UG);
            List<SelectListItem> list = new CourseManager().GetCourseList(College_ID ?? Guid.Empty, printProgramme, programme) ?? new List<SelectListItem>();
            return PartialView(list);
        }

        [HttpGet]
        public PartialViewResult _DDLProgramme(Guid? College_ID)
        {
            IEnumerable<SelectListItem> list = new CourseManager().GetProgrammes(College_ID ?? Guid.Empty) ?? Helper.GetSelectList<Programme>();
            return PartialView(list);
        }

        public ActionResult SubjectsEnrollment()
        {
            ViewBag.Programmes = Helper.GetSelectList<Programme>();
            ViewBag.Courses = new List<SelectListItem>();
            ViewBag.Semesters = Helper.GetSelectList<Semester>();
            ViewBag.Colleges = new CollegeManager().GetADMCollegeMasterList();
            ViewBag.Gender = Helper.GenderDDL();
            return View();
        }

        public ActionResult _SubjectCountList(Parameters parameter)
        {
            var programme = parameter.Filters.FirstOrDefault(filter => filter.Column.ToUpper().Equals("PROGRAMME"));
            if (parameter.SortInfo.ColumnName == null)
            {
                parameter.SortInfo = new Sort() { ColumnName = "AssignedStudentCount", OrderBy = System.Data.SqlClient.SortOrder.Descending };
            }
            if (programme == null)
                programme.Value = "IG";
            List<SubjectsCount> list = new AssignCombinationManager().GetSubjectsCount(parameter);
            ViewBag.Programme = programme.Value;
            ViewBag.RecordCount = ((parameter.PageInfo.PageNumber) * parameter.PageInfo.PageSize);
            return View(list);
        }

        #endregion
    }
}