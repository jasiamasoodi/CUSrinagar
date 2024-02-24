using CUSrinagar.BusinessManagers;
using CUSrinagar.DataManagers;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using CUSrinagar.Models;
using CUSrinagar.OAuth;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CUSrinagar.WebApp.CUSrinagarAdminPanel.Controllers
{
    [OAuthorize(AppRoles.University, AppRoles.University_Registrar)]
    public class EntranceController : UniversityAdminBaseController
    {
        string errorMsg = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>##</a></div>";

        #region viewbags
        private void SetViewBags()
        {
            ViewBag.Courses = new List<SelectListItem>();
            List<SelectListItem> Colleges = new CollegeManager().GetAllCollegeMasterList();

            List<SelectListItem> CreateCenters = new List<SelectListItem>();
            CreateCenters.AddRange(Colleges);
            ViewBag.CreateCenters = CreateCenters;

            Colleges.Insert(0, new SelectListItem { Text = "All", Value = Guid.Empty.ToString() });
            ViewBag.Colleges = Colleges;

            ViewBag.Programmes = Helper.GetSelectList(PrintProgramme.BED);
            ViewBag.SelfFinancedDownloadType = Helper.GetSelectList<SelfFinancedDownloadType>();
            ViewBag.Programme = Helper.GetSelectList(Programme.Engineering);

            IEnumerable<SelectListItem> SFProgramme = Helper.GetSelectList(Programme.Engineering, Programme.HS, Programme.Professional);
            SFProgramme.First(x => x.Value == "3").Text = "Integrated / Honors / Professional";
            ViewBag.SFProgramme = SFProgramme;

            ViewBag.ListType = Helper.GetSelectList<EntrancePDFListType>();
            ViewBag.ExcelListType = Helper.GetSelectList<EntranceExcelListType>();
        }

        private void SetCenterViewBags()
        {
            ViewBag.FromCenterList = new EntranceCentersManager().GetRelocateCenterList();
            ViewBag.ToCenterList = new EntranceCentersManager().GetCenterList();
            ViewBag.Courses = new List<SelectListItem>();
            ViewBag.Programmes = Helper.GetSelectList(PrintProgramme.BED);
        }
        #endregion

        #region DisplayCenterDetailsAndCrud

        [HttpGet]
        public ActionResult EntranceCenters()
        {
            SetViewBags();
            ViewBag.Data = new EntranceCentersManager().GetEntranceCentersMaster(Guid.Empty);
            return View();
        }


        [HttpPost]
        public PartialViewResult _DisplayCenters(Guid College_ID)
        {
            return PartialView("_DisplayCenters", new EntranceCentersManager().GetEntranceCentersMaster(College_ID));
        }

        [HttpPost]
        public JsonResult DeleteDataFromCenterAllotment()
        {
            int result = new SemesterCentersManager().DeleteDataFromCenterAllotment(false);

            if (result == -1)
                return Json("Some courses are still having Download Admit Cards Open. Cannot Delete", JsonRequestBehavior.DenyGet);
            if (result == 0)
                return Json("Already deleted", JsonRequestBehavior.DenyGet);
            else
                return Json(true, JsonRequestBehavior.DenyGet);
        }

        #endregion

        #region AssignRollNosBulk

        [HttpPost]
        public async Task<JsonResult> AssignRollNosInBulk(PrintProgramme? _Programme, int? Batch)
        {
            if (_Programme == null || Batch == null)
                return Json(false, JsonRequestBehavior.DenyGet);

            var response = await new EntranceCentersManager().AssignRollNosInBulkAsync((PrintProgramme)_Programme, (int)Batch);
            if (response.Item1 == true)
                return Json(response.Item1, JsonRequestBehavior.DenyGet);
            else
                return Json(response.Item2, JsonRequestBehavior.DenyGet);
        }

        #endregion

        #region AssignCenterToIndividual
        public ActionResult AdmitCardStatus()
        {
            ViewBag.PirntProgramme = Helper.GetSelectList(PrintProgramme.BED);
            return View();
        }

        [HttpPost]
        public ActionResult AdmitCardStatus(string formOrRegNumber, PrintProgramme PrintProgramme, string command)
        {
            if (string.IsNullOrWhiteSpace(formOrRegNumber))
                return View();
            ViewBag.PirntProgramme = Helper.GetSelectList(PrintProgramme.BED);
            if ((command + "").Trim().ToLower() == "delete")
            {
                int result = new EntranceCentersManager().DeleteCentersForIndividual(formOrRegNumber, PrintProgramme);
                if (result > 0)
                {
                    TempData["response"] = $"<div class='col-sm-1'></div><div class='alert alert-success col-sm-10'><button type='button' class='close' data-dismiss='alert'><i class='ace-icon fa fa-times'></i></button><strong><i class='ace-icon fa fa-times'></i></strong> Deleted successfully<br></div><div class='col-sm-1'></div>";
                    return RedirectToAction("AdmitCardStatus");
                }
                else
                {
                    TempData["response"] = $"<div class='col-sm-1'></div><div class='alert alert-danger col-sm-10'><button type='button' class='close' data-dismiss='alert'><i class='ace-icon fa fa-times'></i></button><strong><i class='ace-icon fa fa-times'></i></strong>Admission form not found or centers not alloted yet.<br></div><div class='col-sm-1'></div>";
                    return RedirectToAction("AdmitCardStatus");
                }
            }
            else
            {
                ARGPersonalInformation formDetail = new AssignCombinationManager().GetStudentDetails(formOrRegNumber, PrintProgramme, true);

                if (formDetail == null || formDetail.CoursesApplied.IsNullOrEmpty()) return View();

                if (new EntranceCentersDB().CheckIfEntranceCenterAlreadyAssinged(formDetail.Student_ID))
                {
                    TempData["response"] = $"<div class='col-sm-1'></div><div class='alert alert-danger col-sm-10'><button type='button' class='close' data-dismiss='alert'><i class='ace-icon fa fa-times'></i></button><strong><i class='ace-icon fa fa-times'></i></strong> Center has been already assigned to this Applicant<br></div><div class='col-sm-1'></div>";
                    return RedirectToAction("AdmitCardStatus");
                }

                new EntranceCentersManager().AddGeneralAptitudeCourse(ref formDetail, PrintProgramme);

                if (formDetail.FormStatus == FormStatus.InProcess)
                {
                    TempData["response"] = $"<div class='col-sm-1'></div><div class='alert alert-danger col-sm-10'><button type='button' class='close' data-dismiss='alert'><i class='ace-icon fa fa-times'></i></button><strong><i class='ace-icon fa fa-times'></i></strong> Admission Fee not submitted.<br></div><div class='col-sm-1'></div>";
                    return RedirectToAction("AdmitCardStatus");
                }
                ViewBag.CenterList = new EntranceCentersManager().GetCenterList();
                ViewBag.FormNumber = formOrRegNumber;
                ViewBag.FormDetail = formDetail;
                List<ARGCentersAllotmentMaster> _CenterAllotmentModel = new List<ARGCentersAllotmentMaster>();
                foreach (var item in formDetail.CoursesApplied)
                {
                    if (item.Course_ID == Guid.Parse("a3ee7f98-7b82-4d95-a2c0-faba7a18240e"))//remove graduation if applied
                        continue;

                    _CenterAllotmentModel.Add(new ARGCentersAllotmentMaster
                    {
                        Appeared = false,
                        Course_ID = item.Course_ID,
                        Entity_ID = formDetail.Student_ID,
                        EntranceFormStatus = formDetail.FormStatus,
                        DisplayCourseName = item.CourseName,
                        Programme = PrintProgramme
                    });
                }
                if (PrintProgramme == PrintProgramme.UG && _CenterAllotmentModel.Any(x => x.Course_ID == Guid.Parse("1A4A791F-FE3A-4668-A18B-C71403FD258B")))
                {
                    _CenterAllotmentModel = _CenterAllotmentModel.Where(x => x.Course_ID == Guid.Parse("1A4A791F-FE3A-4668-A18B-C71403FD258B")).ToList();
                }
                return View(new CentersAllotmentMaster { CenterAllotmentMaster = _CenterAllotmentModel });
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AssignCenters(CentersAllotmentMaster _ARGCentersAllotmentMaster)
        {
            ViewBag.FormDetail = null;
            if (!ModelState.IsValid)
            {
                TempData["response"] = $"<div class='col-sm-1'></div><div class='alert alert-danger col-sm-10'><button type='button' class='close' data-dismiss='alert'><i class='ace-icon fa fa-times'></i></button><strong><i class='ace-icon fa fa-times'></i></strong> Data submitted is invalid.<br></div><div class='col-sm-1'></div>";
                return RedirectToAction("AdmitCardStatus");
            }

            int result = new EntranceCentersManager().Save(_ARGCentersAllotmentMaster, _ARGCentersAllotmentMaster.CenterAllotmentMaster[0].Programme);
            if (result > 0)
                TempData["response"] = $"<div class='col-sm-1'></div><div class='alert alert-success col-sm-10'><button type='button' class='close' data-dismiss='alert'><i class='ace-icon fa fa-times'></i></button><strong><i class='ace-icon fa fa-times'></i></strong> Success.<br></div><div class='col-sm-1'></div>";
            else if (result == -1)
                TempData["response"] = $"<div class='col-sm-1'></div><div class='alert alert-danger col-sm-10'><button type='button' class='close' data-dismiss='alert'><i class='ace-icon fa fa-times'></i></button><strong><i class='ace-icon fa fa-times'></i></strong> Center has been already assigned to this Applicant<br></div><div class='col-sm-1'></div>";
            else
                TempData["response"] = $"<div class='col-sm-1'></div><div class='alert alert-danger col-sm-10'><button type='button' class='close' data-dismiss='alert'><i class='ace-icon fa fa-times'></i></button><strong><i class='ace-icon fa fa-times'></i></strong> Some error occurred, please refresh &amp; try again.<br></div><div class='col-sm-1'></div>";
            return RedirectToAction("AdmitCardStatus");
        }
        #endregion

        #region AssignCentersBulk

        [HttpGet]
        public ActionResult AssignCentersInBulk()
        {
            SetCenterViewBags();
            return View(new AssignEntranceCenters { Batch = (short)DateTime.Now.Year });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AssignCentersInBulk(AssignEntranceCenters CentersToAssign)
        {
            SetCenterViewBags();
            if (!ModelState.IsValid)
                return View();

            Tuple<bool, string> response = await new EntranceCentersManager().AssignCentersInBulkAsync(CentersToAssign);

            if (response.Item1)
                TempData["error"] = errorMsg.Replace("##", response.Item2).Replace("alert-danger", "alert-success");
            else
                TempData["error"] = errorMsg.Replace("##", response.Item2);

            return RedirectToAction("AssignCentersInBulk");
        }


        [HttpPost]
        public PartialViewResult _GetCoursesDDL(PrintProgramme? Id, short? Id1)
        {
            return PartialView(new EntranceCentersManager().GetCoursesDDL(Id, Id1));
        }

        [HttpPost]
        public PartialViewResult _GetCoursesOMRDDL(PrintProgramme? Id, short? Id1)
        {
            return PartialView(new EntranceCentersManager().GetCoursesDDL(Id, Id1));
        }

        [HttpPost]
        public PartialViewResult _DispalyAvaliableCenters(AssignEntranceCenters CentersToAssign)
        {
            return PartialView(new EntranceCentersManager().GetDispalyAvaliableCenters(CentersToAssign));
        }
        #endregion

        #region GenerateCenterNotice
        [HttpGet]
        public ActionResult DownloadCenterNotices()
        {
            return RedirectToAction("EntranceCenters");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DownloadCenterNotices(SemesterCenters _RePrintCenterNotice)
        {
            ModelState.Remove(nameof(_RePrintCenterNotice.PipeSeparatedCenters));
            ModelState.Remove(nameof(_RePrintCenterNotice.College_ID));
            ModelState.Remove(nameof(_RePrintCenterNotice.College_ID));
            if (!ModelState.IsValid)
                return RedirectToAction("EntranceCenters");

            List<CenterNotice> list = new EntranceCentersManager().GetCenterNoticeDetails(_RePrintCenterNotice.PrintCenterNotice);
            TempData["response"] = null;
            if (list.IsNullOrEmpty())
            {
                TempData["response"] = @"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> No details were found.</div>";
                return RedirectToAction("EntranceCenters");
            }
            return View(list);
        }
        #endregion

        #region Generate Excels
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DownloadAdmissionFormCount(SemesterCenters _RePrintCenterNotice)
        {
            ModelState.Remove(nameof(_RePrintCenterNotice.PipeSeparatedCenters));
            ModelState.Remove(nameof(_RePrintCenterNotice.College_ID));

            if (!ModelState.IsValid)
                return RedirectToAction("EntranceCenters");
            DataTable AdmissionFormCount = await new EntranceCentersManager().GetAdmissionFormCount(_RePrintCenterNotice.PrintCenterNotice);

            if (AdmissionFormCount == null || AdmissionFormCount?.Rows.Count <= 0)
            {
                TempData["response"] = @"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> No details were found.</div>";
                return RedirectToAction("EntranceCenters");
            }

            string appendLateralEntry = "";
            if (_RePrintCenterNotice?.PrintCenterNotice?.LateralEntryOnly ?? false)
                appendLateralEntry = " (Lateral Entry)";


            //AdmissionFormCount.AsEnumerable().Where(c => Convert.ToString(c["CourseFullName"]).ToLower().Contains("bca"))
            //    .ToList().ForEach(DR => DR.SetField("CourseFullName", $"BCA/IMCA{appendLateralEntry}"));


            //AdmissionFormCount.AsEnumerable().Where(c => Convert.ToString(c["CourseFullName"]).ToLower().Contains("bba"))
            //    .ToList().ForEach(DR => DR.SetField("CourseFullName", $"BBA/IMBA{appendLateralEntry}"));

            return DownloadExcel(AdmissionFormCount, $"{appendLateralEntry} AdmissionFormCount_EntranceYear-{_RePrintCenterNotice.PrintCenterNotice.ExaminationYear}");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DownloadCenterWiseCourseCount(SemesterCenters _RePrintCenterNotice)
        {
            ModelState.Remove(nameof(_RePrintCenterNotice.PipeSeparatedCenters));
            ModelState.Remove(nameof(_RePrintCenterNotice.College_ID));
            if (!ModelState.IsValid)
                return RedirectToAction("EntranceCenters");
            DataTable CenterWiseCourseCount = await new EntranceCentersManager().GetCenterWiseCourseCountAsync(_RePrintCenterNotice.PrintCenterNotice);

            if (CenterWiseCourseCount == null || CenterWiseCourseCount?.Rows.Count <= 0)
            {
                TempData["response"] = @"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> No details were found.</div>";
                return RedirectToAction("EntranceCenters");
            }

            CenterWiseCourseCount.AsEnumerable().Where(c => Convert.ToString(c["CourseFullName"]).ToLower().Contains("bca"))
                .ToList().ForEach(DR => DR.SetField("CourseFullName", "BCA/IMCA"));


            CenterWiseCourseCount.AsEnumerable().Where(c => Convert.ToString(c["CourseFullName"]).ToLower().Contains("bba"))
                .ToList().ForEach(DR => DR.SetField("CourseFullName", "BBA/IMBA"));

            return DownloadExcel(CenterWiseCourseCount, $"CenterWiseCourseCount_EntranceYear-{_RePrintCenterNotice.PrintCenterNotice.ExaminationYear}");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DownloadOMRMasterFile(SemesterCenters _RePrintCenterNotice)
        {
            ModelState.Remove(nameof(_RePrintCenterNotice.PipeSeparatedCenters));
            if (!ModelState.IsValid)
                return RedirectToAction("EntranceCenters");
            _RePrintCenterNotice.PrintCenterNotice.College_ID = _RePrintCenterNotice.College_ID;
            DataTable CenterWiseCourseCount = await new EntranceCentersManager().GetOMRMasterFile(_RePrintCenterNotice.PrintCenterNotice);

            if (CenterWiseCourseCount == null || CenterWiseCourseCount?.Rows.Count <= 0)
            {
                TempData["response"] = @"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> No details were found.</div>";
                return RedirectToAction("EntranceCenters");
            }

            ADMCourseMaster courseMaster = new CourseManager().GetCompactItem(_RePrintCenterNotice.PrintCenterNotice.College_ID);
            string courseFullName = courseMaster.CourseFullName.ToUpper().Trim().Contains("BCA") ? "BCA-IMCA" : courseMaster.CourseFullName.Replace("'", "");
            if (courseFullName != "BCA-IMCA")
                courseFullName = courseMaster.CourseFullName.ToUpper().Trim().Contains("BBA") ? "BBA-IMBA" : courseMaster.CourseFullName.Replace("'", "");

            var dataRows = CenterWiseCourseCount.AsEnumerable().ToList().Select(x => new
            {
                EntranceRollNo = x["EntranceRollNo"].ToString(),
                FullName = x["FullName"].ToString()
            }).ToList();
            ExportToCSV(dataRows, $"{courseFullName.Replace("Integrated", "").Replace("Honors", "").Replace("Honor's", "")}_OMRMaster_Entrance-{_RePrintCenterNotice.PrintCenterNotice.ExaminationYear}");
            return new EmptyResult();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DownloadAttendanceSheetFile(SemesterCenters _RePrintCenterNotice)
        {
            ModelState.Remove(nameof(_RePrintCenterNotice.PipeSeparatedCenters));
            if (!ModelState.IsValid)
                return RedirectToAction("EntranceCenters");
            _RePrintCenterNotice.PrintCenterNotice.College_ID = _RePrintCenterNotice.College_ID;
            DataTable CenterWiseCourseCount = await new EntranceCentersManager().GetAttendanceSheetFileAsync(_RePrintCenterNotice.PrintCenterNotice);

            if (CenterWiseCourseCount == null || CenterWiseCourseCount?.Rows.Count <= 0)
            {
                TempData["response"] = @"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> No details were found.</div>";
                return RedirectToAction("EntranceCenters");
            }

            ADMCourseMaster courseMaster = new CourseManager().GetCompactItem(_RePrintCenterNotice.PrintCenterNotice.College_ID);
            string courseFullName = courseMaster.CourseFullName.ToUpper().Trim().Contains("BCA") ? "BCA-IMCA" : courseMaster.CourseFullName.Replace("'", "");
            if (courseFullName != "BCA-IMCA")
                courseFullName = courseMaster.CourseFullName.ToUpper().Trim().Contains("BBA") ? "BBA-IMBA" : courseMaster.CourseFullName.Replace("'", "");

            return DownloadExcel(CenterWiseCourseCount, $"{courseFullName.Replace("Integrated", "").Replace("Honors", "").Replace("Honor's", "")}_AttendanceSheet_Entrance-{_RePrintCenterNotice.PrintCenterNotice.ExaminationYear}");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DownloadGeneralMeritList(SemesterCenters _RePrintCenterNotice)
        {
            ModelState.Remove(nameof(_RePrintCenterNotice.PipeSeparatedCenters));

            if (!ModelState.IsValid)
                return RedirectToAction("EntranceCenters");
            _RePrintCenterNotice.PrintCenterNotice.College_ID = _RePrintCenterNotice.College_ID;
            DataTable CenterWiseCourseCount = await new EntranceCentersManager().GetGeneralMeritList(_RePrintCenterNotice.PrintCenterNotice);

            if (CenterWiseCourseCount == null || CenterWiseCourseCount?.Rows.Count <= 0)
            {
                TempData["response"] = @"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> No details were found.</div>";
                return RedirectToAction("EntranceCenters");
            }

            string courseFullName = new CourseManager().GetCompactItem(_RePrintCenterNotice.PrintCenterNotice.College_ID).CourseFullName.Replace("'", "");

            return DownloadExcel(CenterWiseCourseCount, $"{courseFullName}_MeritList_Entrance-{_RePrintCenterNotice.PrintCenterNotice.ExaminationYear}");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DownloadCategoryMeritList(SemesterCenters _RePrintCenterNotice)
        {
            ModelState.Remove(nameof(_RePrintCenterNotice.PipeSeparatedCenters));
            if (!ModelState.IsValid)
                return RedirectToAction("EntranceCenters");
            _RePrintCenterNotice.PrintCenterNotice.College_ID = _RePrintCenterNotice.College_ID;
            DataTable CenterWiseCourseCount = await new EntranceCentersManager().GetCategoryMeritList(_RePrintCenterNotice.PrintCenterNotice);

            if (CenterWiseCourseCount == null || CenterWiseCourseCount?.Rows.Count <= 0)
            {
                TempData["response"] = @"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> No details were found.</div>";
                return RedirectToAction("EntranceCenters");
            }

            string courseFullName = new CourseManager().GetCompactItem(_RePrintCenterNotice.PrintCenterNotice.College_ID).CourseFullName.Replace("'", "");
            return DownloadExcel(CenterWiseCourseCount, $"{courseFullName}_MeritList_Entrance-{_RePrintCenterNotice.PrintCenterNotice.ExaminationYear}");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DownloadDateSheet(SemesterCenters _RePrintCenterNotice)
        {
            ModelState.Remove(nameof(_RePrintCenterNotice.PipeSeparatedCenters));
            ModelState.Remove(nameof(_RePrintCenterNotice.College_ID));

            if (!ModelState.IsValid)
                return RedirectToAction("EntranceCenters");
            DataTable AdmissionFormCount = await new EntranceCentersManager().GetDateSheet(_RePrintCenterNotice.PrintCenterNotice);

            if (AdmissionFormCount == null || AdmissionFormCount?.Rows.Count <= 0)
            {
                TempData["response"] = @"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> No details were found.</div>";
                return RedirectToAction("EntranceCenters");
            }

            return DownloadExcel(AdmissionFormCount, $"DateSheet_Entrance-{_RePrintCenterNotice.PrintCenterNotice.ExaminationYear}");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DownloadDistrictWiseCount(SemesterCenters _RePrintCenterNotice)
        {
            ModelState.Remove(nameof(_RePrintCenterNotice.PipeSeparatedCenters));
            ModelState.Remove(nameof(_RePrintCenterNotice.College_ID));

            if (!ModelState.IsValid)
                return RedirectToAction("EntranceCenters");
            DataTable AdmissionFormCount = await new EntranceCentersManager().GetDistrictWiseCount(_RePrintCenterNotice.PrintCenterNotice);

            if (AdmissionFormCount == null || AdmissionFormCount?.Rows.Count <= 0)
            {
                TempData["response"] = @"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> No details were found.</div>";
                return RedirectToAction("EntranceCenters");
            }

            return DownloadExcel(AdmissionFormCount, $"DistrictWiseCount_Entrance-{_RePrintCenterNotice.PrintCenterNotice.ExaminationYear}");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DownloadDistrictWiseCenterAllotmentCount(SemesterCenters _RePrintCenterNotice)
        {
            ModelState.Remove(nameof(_RePrintCenterNotice.PipeSeparatedCenters));
            if (!ModelState.IsValid)
                return RedirectToAction("EntranceCenters");
            _RePrintCenterNotice.PrintCenterNotice.College_ID = _RePrintCenterNotice.College_ID;
            DataTable CenterWiseCourseCount = await new EntranceCentersManager().GetDistrictWiseCenterAllotmentCount(_RePrintCenterNotice.PrintCenterNotice);

            if (CenterWiseCourseCount == null || CenterWiseCourseCount?.Rows.Count <= 0)
            {
                TempData["response"] = @"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> No details were found.</div>";
                return RedirectToAction("EntranceCenters");
            }

            string courseFullName = new CourseManager().GetCompactItem(_RePrintCenterNotice.PrintCenterNotice.College_ID).CourseFullName.Replace("'", "");

            return DownloadExcel(CenterWiseCourseCount, $"{courseFullName.Replace("Integrated", "").Replace("Honors", "").Replace("Honor's", "")}_DistrictCenterAllotment_Entrance-{_RePrintCenterNotice.PrintCenterNotice.ExaminationYear}");
        }
        #endregion

        #region Upload result
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UploadEntranceResult(SemesterCenters _RePrintCenterNotice, HttpPostedFileBase CSVResultFile)
        {
            ModelState.Remove(nameof(_RePrintCenterNotice.PipeSeparatedCenters));

            if (!ModelState.IsValid)
            {
                return RedirectToAction("EntranceCenters");
            }
            _RePrintCenterNotice.PrintCenterNotice.College_ID = _RePrintCenterNotice.College_ID;
            Tuple<bool, string> response = await new EntranceCentersManager().UploadEntranceResultAsync(_RePrintCenterNotice.PrintCenterNotice, CSVResultFile);
            if (response.Item1)
            {
                TempData["response"] = $"<div class='alert alert-success alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> {response.Item2}</div>";
            }
            else
            {
                TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> {response.Item2}</div>";
            }
            return RedirectToAction("EntranceCenters");
        }
        #endregion

        #region SelfFinanced List
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DownloadSelfFinancedList(SemesterCenters _SelfFinancedList)
        {
            ModelState.Remove(nameof(_SelfFinancedList.PipeSeparatedCenters));

            if (!ModelState.IsValid)
                return RedirectToAction("EntranceCenters");

            DataTable SFList = await new EntranceCentersManager().GetSelfFinancedListAsync(_SelfFinancedList.SelfFinancedList);

            if (SFList == null || SFList?.Rows.Count <= 0)
            {
                TempData["response"] = @"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> No details were found.</div>";
                return RedirectToAction("EntranceCenters");
            }

            return DownloadExcel(SFList, $"SelfFinancedList_Of_{_SelfFinancedList.SelfFinancedList.Programme.ToString()}-{_SelfFinancedList.SelfFinancedList.Batch}");
        }
        #endregion

        #region VerifyFirstSemesterRR

        private void RRViewBags()
        {
            ViewBag.Programme = Helper.GetSelectList<Programme>();
            ViewBag.Colleges = new CollegeManager().GetADMCollegeMasterList();
            ViewBag.CourseID = new List<SelectListItem>();
        }

        [HttpPost]
        public PartialViewResult _GetCoursesRRDDL(Programme? Id, short? Id1)
        {
            return PartialView("_GetCoursesDDL", new EntranceCentersManager().GetCoursesDDL(Id, Id1));
        }

        [HttpGet]
        public ActionResult VerifyRR()
        {
            RRViewBags();
            return View();
        }

        [HttpPost]
        public async Task<PartialViewResult> _GetListOfFirstSemesterRRPartial(SearchFirstSemesterRR searchFirstSemesterRR)
        {
            if (!ModelState.IsValid)
                return PartialView(new List<FirstSemesterRR>());
            List<FirstSemesterRR> RRList = await new EntranceCentersManager().GetFirstSemesterRRAsync(searchFirstSemesterRR);
            return PartialView(RRList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DownloadFirstSemesterRR(SearchFirstSemesterRR searchFirstSemesterRR)
        {
            RRViewBags();

            if (!ModelState.IsValid)
                return RedirectToAction("VerifyRR");

            List<FirstSemesterRR> RRList = await new EntranceCentersManager().GetFirstSemesterRRAsync(searchFirstSemesterRR);

            if (RRList.IsNullOrEmpty())
                return RedirectToAction("VerifyRR");

            FirstSemesterRR firstSemesterRR = new FirstSemesterRR();
            List<string> _modelPropertiesAsColumns = new List<string>
                                                    {
                                                        nameof(firstSemesterRR.Sno),
                                                        nameof(firstSemesterRR.StudentFormNo),
                                                        nameof(firstSemesterRR.EntranceRollNo),
                                                        nameof(firstSemesterRR.FullName),
                                                        nameof(firstSemesterRR.FathersName),
                                                        nameof(firstSemesterRR.Category),
                                                        nameof(firstSemesterRR.CollegeFullName),
                                                        nameof(firstSemesterRR.CourseAllotedByCollege),
                                                        nameof(firstSemesterRR.CoursesAppliedByStudent),
                                                        nameof(firstSemesterRR.Mobile),
                                                        nameof(firstSemesterRR.AmountPaid),
                                                        nameof(firstSemesterRR.AcceptedOn),
                                                    };
            return DownloadExcel(RRList, _modelPropertiesAsColumns, $"Verify_1st_Sem_RR_List");
        }

        #endregion

        #region DeleteJunkData
        [HttpGet]
        public ActionResult DeleteJunk()
        {
            ViewBag.Programmes = Helper.GetSelectList(PrintProgramme.BED);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteJunk(PrintProgramme printProgramme, bool IsCertificate, bool onlyLateralEntry, DateTime? appliedBefore)
        {
            ViewBag.Programmes = Helper.GetSelectList(PrintProgramme.BED);
            Tuple<bool, string> response = null;

            if (IsCertificate)
            {
                response = await new EntranceCentersManager().DeleteJunkAsync();
            }
            else
            {
                response = await new EntranceCentersManager().DeleteJunkAsync(printProgramme, onlyLateralEntry,appliedBefore??DateTime.MinValue);
            }

            if (response.Item1)
            {
                TempData["response"] = errorMsg.Replace("##", response.Item2).Replace("alert-danger", "alert-success");
            }
            else
            {
                TempData["response"] = errorMsg.Replace("##", response.Item2);
            }
            return View();
        }
        #endregion

        #region PDF Merit Lists
        [HttpPost]
        public PartialViewResult _GetCoursesPDFDDL(PrintProgramme? Id, short? Id1)
        {
            return PartialView(new EntranceCentersManager().GetCoursesDDL(Id, Id1));
        }

        [HttpGet]
        public async Task<ActionResult> DownloadPDFMeritList(SemesterCenters meritListFilter)
        {
            ModelState.Remove(nameof(meritListFilter.College_ID));
            ModelState.Remove(nameof(meritListFilter.Center_ID));
            ModelState.Remove(nameof(meritListFilter.PipeSeparatedCenters));
            ModelState.Remove(nameof(meritListFilter.IsEntrance));
            if (!ModelState.IsValid)
                return View();

            if (meritListFilter.PDFListFilter == null)
                return View();

            return View(await new EntranceCentersManager().PDFMeritListsAsync(meritListFilter.PDFListFilter));
        }
        #endregion

        #region RelocateCenter
        [HttpPost]
        public PartialViewResult _GetCoursesRCDDL(Guid Id)
        {
            return PartialView(new EntranceCentersManager().GetRelocateCenterCourseIds(Id) ?? new List<SelectListItem>());
        }

        [HttpPost]
        public JsonResult RelocateCenter(RelocateCenter relocateCenter)
        {
            if (ModelState.IsValid)
            {
                Tuple<bool, string> response = new EntranceCentersManager().RelocateCenter(relocateCenter);
                return Json(new { Issucces = response.Item1, msg = response.Item2 }, JsonRequestBehavior.DenyGet);
            }
            else
            {
                return Json(new { Issucces = false, msg = "Invalid Details Found" }, JsonRequestBehavior.DenyGet);
            }
        }
        #endregion
    }
}