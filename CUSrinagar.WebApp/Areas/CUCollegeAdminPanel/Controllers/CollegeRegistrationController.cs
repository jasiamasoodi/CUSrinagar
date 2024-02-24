using CUSrinagar.Extensions;
using CUSrinagar.OAuth;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;
using CUSrinagar.Enums;
using CUSrinagar.BusinessManagers;
using CUSrinagar.Models;
using System.Linq;
using CaptchaMvc.HtmlHelpers;

namespace CUSrinagar.WebApp.CUCollegeAdminPanel.Controllers
{
    [OAuthorize(AppRoles.College_Clerk)]
    public class CollegeRegistrationController : Controller
    {
        #region ViewBags
        [NonAction]
        private void SetViewBags(PrintProgramme programme)
        {
            ViewBag.GenderDDL = Helper.GenderDDL();
            ViewBag.ReligionDDL = Helper.ReligionDDL();
            ViewBag.CategoryDDL = new CategoryManager().GetCategoryList();
            ViewBag.DistrictDDL = new GeneralDDLManager().GetDistrictList();
            ViewBag.Provisional = Helper.ProvisionalDDL();
            ViewBag.Session = Helper.SessionDDL();
            ViewBag.Boards = Helper.BoardsDDL();
            ViewBag.AssemblyDDL = new GeneralDDLManager().GetAssemblyList();
            ViewBag.UniversitiesDDL = Helper.UniversitiesDDL();
            ViewBag.Preference = Helper.PreferenceDDL();
            ViewBag.OldCourses = new RegistrationManager().GetCollegeCourses();
            ViewBag.FormNoMater = new RegistrationManager().GetFormNoMaster(programme);
        }
        #endregion

        #region Graduation
        [HttpGet]
        [OutputCache(Duration = 0, Location = OutputCacheLocation.None, NoStore = true)]
        public ActionResult Graduation()
        {
            SetViewBags(PrintProgramme.UG);

            ARGPersonalInformation aRGPersonalInformation = new ARGPersonalInformation
            {
                AcademicDetails = new List<ARGStudentPreviousQualifications>
                {
                    new   ARGStudentPreviousQualifications {
                        ExamName = "12TH",
                        ReadOnly=true
                        }
                }
            };
            aRGPersonalInformation.CoursesApplied = new CourseManager().GetCourseListForAdminRegistrationCollegeWise(PrintProgramme.UG, AppUserHelper.College_ID ?? Guid.Empty) ?? new List<ARGCoursesApplied>();
            return View(aRGPersonalInformation);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [OutputCache(Duration = 0, Location = OutputCacheLocation.None, NoStore = true)]
        public ActionResult Graduation(ARGPersonalInformation _ARGPersonalInformation)
        {
            SetViewBags(PrintProgramme.UG);
            List<ARGCoursesApplied> SelectedCourses = _ARGPersonalInformation.CoursesApplied?.Where(x => x.IsClicked)?.ToList();
            _ARGPersonalInformation.CoursesApplied = new CourseManager().GetCourseListForAdminRegistrationCollegeWise(PrintProgramme.UG, AppUserHelper.College_ID ?? Guid.Empty) ?? new List<ARGCoursesApplied>();

            #region Validation section
            if (SelectedCourses.IsNullOrEmpty())
            {
                TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button><strong>Oh snap!</strong> <a href='#' class='alert-link'>Please select course(s)</a></div>";
                return View(_ARGPersonalInformation);
            }
            else
            {
                foreach (var item in SelectedCourses ?? new List<ARGCoursesApplied>())
                {
                    if (!_ARGPersonalInformation.CoursesApplied.Any(x => x.Course_ID == item.Course_ID))
                    {
                        TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button><strong>Oh snap!</strong> <a href='#' class='alert-link'>Invalid Course found. Please refresh and try again.</a></div>";
                        return View(_ARGPersonalInformation);
                    }
                }
            }
            if ((!this?.IsCaptchaValid("Captcha is not valid") ?? true))
            {
                TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button><strong>Oh snap!</strong> <a href='#' class='alert-link'>Invalid Verification Code, please try again.</a></div>";
                return View(_ARGPersonalInformation);
            }
            if ((_ARGPersonalInformation?.IsProvisional) ?? false)
            {
                int indexOf12th = _ARGPersonalInformation.AcademicDetails.FindIndex(x => x.ExamName.ToLower() == "12th");
                if (indexOf12th != -1)
                {
                    _ARGPersonalInformation.AcademicDetails.RemoveAt(indexOf12th);
                    ModelState.Clear();
                    this.TryValidateModel(_ARGPersonalInformation);
                }
            }
            if (!new RegistrationManager().ValidateDOB(_ARGPersonalInformation.EnteredDOB))
            {
                ModelState.AddModelError("EnteredDOB", "Invalid DOB");
            }

            _ARGPersonalInformation.MarksCertificate = null;
            ModelState.Remove(nameof(_ARGPersonalInformation.MarksCertificate));
            _ARGPersonalInformation.CategoryCertificate = null;
            ModelState.Remove(nameof(_ARGPersonalInformation.CategoryCertificate));
            #endregion

            #region Save section
            if (!ModelState.IsValid)
            {
                if (_ARGPersonalInformation != null)
                {
                    if (_ARGPersonalInformation.AcademicDetails.Any(x => x.ExamName.ToLower().Trim() != "12th"))
                    {
                        _ARGPersonalInformation.AcademicDetails.Add(new ARGStudentPreviousQualifications
                        {
                            ExamName = "12TH",
                            ReadOnly = true
                        });
                    }
                }
                return View(_ARGPersonalInformation);
            }
            else
            {
                //--------------- save here ------------------
                _ARGPersonalInformation.CoursesApplied = new List<ARGCoursesApplied>();
                _ARGPersonalInformation.CoursesApplied.AddRange(SelectedCourses);
                _ARGPersonalInformation.PreviousUniversityRegnNo = null;
                _ARGPersonalInformation.Preference = null;
                int _result = new RegistrationManager().Save(_ARGPersonalInformation, PrintProgramme.UG, AppUserHelper.User_ID);
                SelectedCourses = null;//dispose
                if (_result > 0)
                {
                    new RegistrationManager().SendEmailAndSMSAsync(_ARGPersonalInformation, PrintProgramme.UG);
                    return RedirectToAction("Index", "Payment", new { id = _ARGPersonalInformation.Student_ID + "/", R = Convert.ToString(PrintProgramme.UG).EncryptCookieAndURLSafe(), area = string.Empty });
                }
                else if (_result == -1)
                {
                    TempData["response"] = $"<div class='alert-msg error' style='background-color: #F3CBCC !important'>Board Registration No. has already submitted the Form (Email or visit University for any queries). <a class='close' href='#' title='close'>X</a></div>";
                    return RedirectToAction("Graduation", "CollegeRegistration", new { area = "CUCollegeAdminPanel" });
                }
                else
                {
                    TempData["response"] = $"<div class='alert-msg error' style='background-color: #F3CBCC !important'>Unable to Submit Form. Please refresh & try again. <a class='close' href='#' title='close'>X</a></div>";
                    return RedirectToAction("Graduation", "CollegeRegistration", new { area = "CUCollegeAdminPanel" });
                }
            }
            #endregion
        }

        [HttpGet]
        public ActionResult GraduationPreview()
        {
            TempData["response"] = "Refresh is not allowed";
            return RedirectToAction("Graduation", "CollegeRegistration", new { area = "CUCollegeAdminPanel" });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [OutputCache(Duration = 0, Location = System.Web.UI.OutputCacheLocation.None, NoStore = true)]
        public ActionResult GraduationPreview(ARGPersonalInformation _ARGPersonalInformation)
        {
            SetViewBags(PrintProgramme.UG);
            List<ARGCoursesApplied> SelectedCourses = _ARGPersonalInformation.CoursesApplied?.Where(x => x.IsClicked)?.ToList();
            var _RegistrationManager = new RegistrationManager();
            _ARGPersonalInformation.CoursesApplied = new CourseManager().GetCourseListForAdminRegistration(PrintProgramme.UG) ?? new List<ARGCoursesApplied>();

            #region Validation section
            if (SelectedCourses.IsNullOrEmpty())
            {
                TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'></button> <a href='#' class='alert-link'>Please select course(s)</a></div>";
                return View("~/Views/Registration/IntegratedHonorsPreview.cshtml");
            }
            else
            {
                foreach (var item in SelectedCourses ?? new List<ARGCoursesApplied>())
                {
                    if (!_ARGPersonalInformation.CoursesApplied.Any(x => x.Course_ID == item.Course_ID))
                    {
                        TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'></button> <a href='#' class='alert-link'>Invalid Course found. Please refresh and try again.</a></div>";
                        return View("~/Views/Registration/IntegratedHonorsPreview.cshtml");
                    }
                }
            }
            if ((_ARGPersonalInformation?.IsProvisional) ?? false)
            {
                int indexOf12th = _ARGPersonalInformation.AcademicDetails.FindIndex(x => x.ExamName.ToLower() == "12th");
                if (indexOf12th != -1)
                {
                    _ARGPersonalInformation.AcademicDetails.RemoveAt(indexOf12th);
                    ModelState.Clear();
                    this.TryValidateModel(_ARGPersonalInformation);
                }
            }
            if (_RegistrationManager.CheckBoardRegNoExists(_ARGPersonalInformation.BoardRegistrationNo, PrintProgramme.UG))
            {
                ModelState.AddModelError("BoardRegistrationNo", "Invalid or Board Registration No. has already submitted the Form  for current session (Email or visit University for any queries)");
            }
            if (!_RegistrationManager.ValidateDOB(_ARGPersonalInformation.EnteredDOB))
            {
                ModelState.AddModelError("EnteredDOB", "Invalid DOB");
            }

            ModelState.Remove(nameof(_ARGPersonalInformation.CategoryCertificate));
            ModelState.Remove(nameof(_ARGPersonalInformation.MarksCertificate));
            #endregion

            #region Preview section
            if (!ModelState.IsValid)
            {
                if (_ARGPersonalInformation != null)
                {
                    _ARGPersonalInformation.AcademicDetails.Add(new ARGStudentPreviousQualifications
                    {
                        ExamName = "12TH",
                        ReadOnly = true
                    });
                }
                TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'></button> <a href='#' class='alert-link'> Data submitted is not valid. Please check carefully and try again.</a></div>";
                return View("~/Views/Registration/IntegratedHonorsPreview.cshtml");
            }
            else
            {
                //--------------- save here ------------------
                _ARGPersonalInformation.CoursesApplied = new List<ARGCoursesApplied>();
                _ARGPersonalInformation.CoursesApplied.AddRange(SelectedCourses);

                ViewBag.BytePhotograph = Helper.CompressImage(_ARGPersonalInformation.PhotographPath.InputStream, 259, 194);
                ViewBag.PhotoType = _ARGPersonalInformation.PhotographPath.ContentType;

                _ARGPersonalInformation.PhotographPath = null;
                _ARGPersonalInformation.PreviousUniversityRegnNo = null;
                SelectedCourses = null;

                string[] disttAndState = _ARGPersonalInformation.StudentAddress.District.Split('|');
                _ARGPersonalInformation.StudentAddress.District = disttAndState[0];
                _ARGPersonalInformation.StudentAddress.State = disttAndState[1];
                string[] assembly = _ARGPersonalInformation.StudentAddress.AssemblyConstituency.Split('|');
                _ARGPersonalInformation.StudentAddress.AssemblyConstituency = assembly[0];
                _ARGPersonalInformation.StudentAddress.ParliamentaryConstituency = assembly[1];

                _ARGPersonalInformation.StudentFormNo = "Preview";
                _ARGPersonalInformation.Preference = null;
                _ARGPersonalInformation.DOB = _RegistrationManager.ConvertEnterdDOBToDateTime(_ARGPersonalInformation.EnteredDOB);
                ViewBag.AppliedFor = PrintProgramme.UG.GetEnumDescription();
                return View("~/Views/Registration/Detail.cshtml", _ARGPersonalInformation);
            }
            #endregion
        }
        #endregion
    }
}