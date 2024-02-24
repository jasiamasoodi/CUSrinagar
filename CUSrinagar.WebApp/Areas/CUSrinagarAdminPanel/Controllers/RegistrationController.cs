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
using System.Web;
using System.Web.Mvc;


namespace CUSrinagar.WebApp.CUSrinagarAdminPanel.Controllers
{
    [OAuthorize(AppRoles.University)]
    public class RegistrationController : Controller
    {
        #region ViewBags
        [NonAction]
        private void SetViewBags(Guid id, PrintProgramme ProgrammeOption)
        {
            ViewBag.PreferenceList = Helper.PreferenceDDL();
            ViewBag.SavedCourses = new AdminRegistrationManager().GetStudentCoursesAppliedIDs(id, ProgrammeOption);
            ViewBag.FormNoMaster = new RegistrationManager().GetFormNoMaster(ProgrammeOption);
        }

        [NonAction]
        private void SetDetailViewBags()
        {
            ViewBag.PrintProgrammeOption = Helper.GetSelectList<PrintProgramme>().OrderBy(x => x.Value);
        }

        [NonAction]
        private void SetSelectedCourses(ref ARGPersonalInformation aRGPersonalInformation, PrintProgramme programmeOption)
        {
            if (aRGPersonalInformation == null)
                return;
            List<ARGCoursesApplied> SavedCourses = new AdminRegistrationManager().GetStudentCoursesAppliedIDs(aRGPersonalInformation.Student_ID, programmeOption);

            int LookedCount = 1;
            foreach (var item in aRGPersonalInformation.CoursesApplied)
            {
                if (SavedCourses.IsNullOrEmpty())
                    break;
                if (LookedCount > SavedCourses?.Count())
                    break;
                if (SavedCourses.Any(x => x.Course_ID == item.Course_ID))
                {
                    item.IsClicked = true;
                    item.Preference = SavedCourses.First(x => x.Course_ID == item.Course_ID).Preference ?? 0;
                    LookedCount++;
                }
            }
        }

        [NonAction]
        private IEnumerable<SelectListItem> SetPreferenceCount(int TotalCourses)
        {
            for (int i = 1; i <= TotalCourses; i++)
                yield return new SelectListItem { Value = i.ToString(), Text = i.ToString() };
        }
        #endregion

        #region AssigningRegistrationNumbers
        [HttpGet]
        public ActionResult AssignRegistrationNumbers(PrintProgramme? Programme)
        {
            GetSetResponseViewBags(Programme ?? PrintProgramme.UG);
            ViewBag.Response = TempData["Response"];
            return View();
        }

        [HttpPost]
        public ActionResult AssignRegistrationNumbers(Guid? college_ID, PrintProgramme ProgrammeDropDownList, int GapBetweenCourses)
        {
            GetSetResponseViewBags(ProgrammeDropDownList);
            ResponseData response = new RegistrationManager().AssignRegistrationNumbers(college_ID ?? Guid.Empty, ProgrammeDropDownList, GapBetweenCourses);
            TempData["Response"] = response;
            return RedirectToAction("AssignRegistrationNumbers", new { @Programme = ProgrammeDropDownList });
        }

        private void GetSetResponseViewBags(PrintProgramme? programme)
        {
            short batchToAssign = new RegistrationManager().GetFormNoMaster(programme ?? PrintProgramme.UG).BatchToSet;
            ViewBag.Colleges = new CollegeManager().GetCollegeWiseNewRegistrations(programme ?? PrintProgramme.UG, batchToAssign);
            ViewBag.Batch = batchToAssign;
            ViewBag.Programmes = Helper.GetSelectList<PrintProgramme>().ToList();
            ViewBag.SelectedProgramme = programme;
        }

        #endregion


        #region Integrated - Honors - Professional

        [HttpGet]
        [OutputCache(Duration = 0, Location = System.Web.UI.OutputCacheLocation.None, NoStore = true)]
        public ActionResult IntegratedHonors(Guid? id, PrintProgramme? ProgrammeOption)
        {
            if (!Guid.TryParse(Convert.ToString(id), out Guid ID) || ProgrammeOption == null)
                return RedirectToAction("StudentDetail", "Registration", new { area = "CUSrinagarAdminPanel" });

            SetViewBags(ID, (PrintProgramme)ProgrammeOption);
            ARGPersonalInformation aRGPersonalInformation = new AdminRegistrationManager().GetStudentByID(ID, (PrintProgramme)ProgrammeOption);
            aRGPersonalInformation.CoursesApplied = new CourseManager().GetCourseListForAdminRegistration((PrintProgramme)ProgrammeOption)?.OrderByDescending(x => x.Programme).ToList() ?? new List<ARGCoursesApplied>();

            if (aRGPersonalInformation.StudentAddress.District.ToLower().Trim() == "other")
            {
                aRGPersonalInformation.TotalFee -= ((ARGFormNoMaster)ViewBag.FormNoMaster).AdditionalFeeForNonJK;
            }

            ARGCoursesApplied HasIntBedMed = new AdminRegistrationManager().GetStudentCoursesAppliedIDs(ID, (PrintProgramme)ProgrammeOption).FirstOrDefault(x => x.Course_ID == new Guid("FC32E138-4EE2-4DA2-9453-5C8368180BC3"));

            if (HasIntBedMed != null)
            {
                aRGPersonalInformation.CoursesApplied = new List<ARGCoursesApplied> { HasIntBedMed };
            }
            else
            {
                HasIntBedMed = aRGPersonalInformation.CoursesApplied.FirstOrDefault(x => x.Course_ID == new Guid("FC32E138-4EE2-4DA2-9453-5C8368180BC3"));
                aRGPersonalInformation.CoursesApplied.Remove(HasIntBedMed);
            }

            SetSelectedCourses(ref aRGPersonalInformation, (PrintProgramme)ProgrammeOption);
            ViewBag.PreferenceCount = SetPreferenceCount(aRGPersonalInformation?.CoursesApplied?.Count ?? 0);

            return View(aRGPersonalInformation);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [OutputCache(Duration = 0, Location = System.Web.UI.OutputCacheLocation.None, NoStore = true)]
        public ActionResult IntegratedHonors(ARGPersonalInformation _ARGPersonalInformation)
        {
            PrintProgramme ProgrammeOption = PrintProgramme.IH;
            SetViewBags(_ARGPersonalInformation.Student_ID, ProgrammeOption);

            new RegistrationManager().AddGraduationCourse(_ARGPersonalInformation);
            List<ARGCoursesApplied> SelectedCourses = _ARGPersonalInformation.CoursesApplied?.Where(x => x.IsClicked)?.ToList();
            List<ARGCoursesApplied> _ARGCoursesApplied = new CourseManager().GetCourseListForAdminRegistration(ProgrammeOption)?.OrderByDescending(x => x.Programme).ToList() ?? new List<ARGCoursesApplied>();

            ViewBag.PreferenceCount = SetPreferenceCount(_ARGPersonalInformation?.CoursesApplied?.Count ?? 0);

            #region Validation section            
            if (_ARGPersonalInformation.PhotographPath != null && (!ModelState.IsValidField(nameof(_ARGPersonalInformation.PhotographPath))))
            {
                string errorMessage = ModelState.First(x => x.Key == nameof(_ARGPersonalInformation.PhotographPath)).Value.Errors[0].ErrorMessage;
                TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button><strong>Oh snap!</strong> <a href='#' class='alert-link'> {errorMessage}</a></div>";
                return RedirectToAction("StudentDetail", "Registration", new { area = "CUSrinagarAdminPanel" });
            }

            ARGCoursesApplied HasIntBedMed = SelectedCourses.FirstOrDefault(x => x.Course_ID == new Guid("FC32E138-4EE2-4DA2-9453-5C8368180BC3"));

            if (HasIntBedMed != null)
                _ARGPersonalInformation.CoursesApplied = new List<ARGCoursesApplied> { HasIntBedMed };
            else
            {
                HasIntBedMed = _ARGCoursesApplied.FirstOrDefault(x => x.Course_ID == new Guid("FC32E138-4EE2-4DA2-9453-5C8368180BC3"));
                _ARGCoursesApplied.Remove(HasIntBedMed);
                _ARGPersonalInformation.CoursesApplied = _ARGCoursesApplied;
            }

            if (SelectedCourses.IsNullOrEmpty())
            {
                TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button><strong>Oh snap!</strong> <a href='#' class='alert-link'>Please select course(s)</a></div>";
                return RedirectToAction("StudentDetail", "Registration", new { area = "CUSrinagarAdminPanel" });
            }
            else
            {
                foreach (var item in SelectedCourses ?? new List<ARGCoursesApplied>())
                {
                    if (!_ARGPersonalInformation.CoursesApplied.Any(x => x.Course_ID == item.Course_ID))
                    {
                        TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button><strong>Oh snap!</strong> <a href='#' class='alert-link'>Invalid Course found. Please refresh and try again.</a></div>";
                        return RedirectToAction("StudentDetail", "Registration", new { area = "CUSrinagarAdminPanel" });
                    }
                    // remove if preferenc is open
                    if(!_ARGPersonalInformation.IsLateralEntry)
                    item.Preference = 0;
                }

                #region PreferenceCode
                if (_ARGPersonalInformation.IsLateralEntry)
                {
                    if (SelectedCourses.Any(x => x.Preference == null || x.Preference <= 0))
                    {
                        TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button><strong>Oh snap!</strong> <a href='#' class='alert-link'> Please select preference(s) for all your selected course(s) and try again.</a></div>";
                        return RedirectToAction("StudentDetail", "Registration", new { area = "CUSrinagarAdminPanel" });
                    }
                    if (SelectedCourses.Any(x => Convert.ToInt32(x.Preference) > SelectedCourses.Count()))
                    {
                        TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button><strong>Oh snap!</strong> <a href='#' class='alert-link'> Some of your perferences are invalid as per your selected course(s). Please check carefully and try again.</a></div>";
                        return RedirectToAction("StudentDetail", "Registration", new { area = "CUSrinagarAdminPanel" });
                    }
                    if (SelectedCourses.GroupBy(x => x.Preference).Where(g => g.Count() > 1).Select(y => y.Key)?.Any() ?? false)
                    {
                        TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button><strong>Oh snap!</strong> <a href='#' class='alert-link'> Duplicate Course preferences found. Please check carefully and try again.</a></div>";
                        return RedirectToAction("StudentDetail", "Registration", new { area = "CUSrinagarAdminPanel" });
                    }
                }
                    #endregion
            }
            #endregion

            #region Save section
            //--------------- save here ------------------
            _ARGPersonalInformation.CoursesApplied = new List<ARGCoursesApplied>();
            _ARGPersonalInformation.CoursesApplied.AddRange(SelectedCourses);
            int _result = new AdminRegistrationManager().EditCourses(_ARGPersonalInformation, ProgrammeOption);
            SelectedCourses = null;//dispose
            SetDetailViewBags();
            if (_result > 0)
            {
                string msg = "Saved Succesfully.";
                if (_ARGPersonalInformation.FormStatus != FormStatus.InProcess)
                {
                    msg += " But Fee already received therefore courses cannot be changed.";
                }
                TempData["response"] = $"<div class='alert alert-block alert-success'>{msg}<a class='close' href='#' title='close'>X</a></div>";
                return RedirectToAction("StudentDetail", "Registration", new { area = "CUSrinagarAdminPanel" });
            }
            else
            {
                TempData["response"] = $"<div class='alert alert-block alert-danger'>Unable to Save Form.  <a class='close' href='#' title='close'>X</a></div>";
                return RedirectToAction("StudentDetail", "Registration", new { area = "CUSrinagarAdminPanel" });
            }

            #endregion
        }

        #endregion


        #region Graduation

        [HttpGet]
        [OutputCache(Duration = 0, Location = System.Web.UI.OutputCacheLocation.None, NoStore = true)]
        public ActionResult Graduation(Guid? id, PrintProgramme? ProgrammeOption)
        {
            if (!Guid.TryParse(Convert.ToString(id), out Guid ID) || ProgrammeOption == null)
                return RedirectToAction("StudentDetail", "Registration", new { area = "CUSrinagarAdminPanel" });

            SetViewBags(ID, (PrintProgramme)ProgrammeOption);
            ARGPersonalInformation aRGPersonalInformation = new AdminRegistrationManager().GetStudentByID(ID, (PrintProgramme)ProgrammeOption);
            aRGPersonalInformation.CoursesApplied = new CourseManager().GetCourseListForAdminRegistration((PrintProgramme)ProgrammeOption);
            SetSelectedCourses(ref aRGPersonalInformation, (PrintProgramme)ProgrammeOption);
            return View(aRGPersonalInformation);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [OutputCache(Duration = 0, Location = System.Web.UI.OutputCacheLocation.None, NoStore = true)]
        public ActionResult Graduation(ARGPersonalInformation _ARGPersonalInformation)
        {
            PrintProgramme ProgrammeOption = PrintProgramme.UG;
            SetViewBags(_ARGPersonalInformation.Student_ID, ProgrammeOption);

            List<ARGCoursesApplied> SelectedCourses = _ARGPersonalInformation.CoursesApplied?.Where(x => x.IsClicked)?.ToList();
            List<ARGCoursesApplied> _ARGCoursesApplied = new CourseManager().GetCourseListForAdminRegistration(ProgrammeOption) ?? new List<ARGCoursesApplied>();

            #region Validation section
            if (_ARGPersonalInformation.PhotographPath != null && (!ModelState.IsValidField(nameof(_ARGPersonalInformation.PhotographPath))))
            {
                string errorMessage = ModelState.First(x => x.Key == nameof(_ARGPersonalInformation.PhotographPath)).Value.Errors[0].ErrorMessage;
                TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button><strong>Oh snap!</strong> <a href='#' class='alert-link'> {errorMessage}</a></div>";
                return RedirectToAction("StudentDetail", "Registration", new { area = "CUSrinagarAdminPanel" });
            }
            _ARGPersonalInformation.CoursesApplied = _ARGCoursesApplied;

            if (_ARGPersonalInformation.Student_ID == Guid.Empty || (_ARGPersonalInformation.TotalFee == 0 && _ARGPersonalInformation.Batch > 2017))
            {
                TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button><strong>Oh snap!</strong> <a href='#' class='alert-link'>Data supplied was invalid</a></div>";
                return RedirectToAction("StudentDetail", "Registration", new { area = "CUSrinagarAdminPanel" });
            }
            else if (SelectedCourses.IsNullOrEmpty())
            {
                TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button><strong>Oh snap!</strong> <a href='#' class='alert-link'>Please select course(s)</a></div>";
                return RedirectToAction("StudentDetail", "Registration", new { area = "CUSrinagarAdminPanel" });
            }
            else
            {
                foreach (var item in SelectedCourses ?? new List<ARGCoursesApplied>())
                {
                    if (!_ARGPersonalInformation.CoursesApplied.Any(x => x.Course_ID == item.Course_ID))
                    {
                        TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button><strong>Oh snap!</strong> <a href='#' class='alert-link'>Invalid Course found. Please refresh and try again.</a></div>";
                        return RedirectToAction("StudentDetail", "Registration", new { area = "CUSrinagarAdminPanel" });
                    }
                }
            }
            #endregion

            #region Save section
            //--------------- save here ------------------
            _ARGPersonalInformation.CoursesApplied = new List<ARGCoursesApplied>();
            _ARGPersonalInformation.CoursesApplied.AddRange(SelectedCourses);
            int _result = new AdminRegistrationManager().EditCourses(_ARGPersonalInformation, ProgrammeOption);
            SelectedCourses = null;//dispose
            SetDetailViewBags();
            if (_result > 0)
            {
                string msg = "Saved Succesfully.";
                if (_ARGPersonalInformation.FormStatus != FormStatus.InProcess)
                {
                    msg += " But Fee already received therefore courses cannot be changed.";
                }
                TempData["response"] = $"<div class='alert alert-block alert-success'>{msg}<a class='close' href='#' title='close'>X</a></div>";
                return RedirectToAction("StudentDetail", "Registration", new { area = "CUSrinagarAdminPanel" });
            }
            else
            {
                TempData["response"] = $"<div class='alert alert-block alert-danger'>Unable to Save Form.  <a class='close' href='#' title='close'>X</a></div>";
                return RedirectToAction("StudentDetail", "Registration", new { area = "CUSrinagarAdminPanel" });
            }
            #endregion
        }

        #endregion


        #region Post-Graduation

        [HttpGet]
        [OutputCache(Duration = 0, Location = System.Web.UI.OutputCacheLocation.None, NoStore = true)]
        public ActionResult PostGraduation(Guid? id, PrintProgramme? ProgrammeOption)
        {
            if (!Guid.TryParse(Convert.ToString(id), out Guid ID) || ProgrammeOption == null)
                return RedirectToAction("StudentDetail", "Registration", new { area = "CUSrinagarAdminPanel" });

            SetViewBags(ID, (PrintProgramme)ProgrammeOption);
            ARGPersonalInformation aRGPersonalInformation = new AdminRegistrationManager().GetStudentByID(ID, (PrintProgramme)ProgrammeOption);
            aRGPersonalInformation.CoursesApplied = new CourseManager().GetCourseListForAdminRegistration((PrintProgramme)ProgrammeOption)?.OrderByDescending(x => x.Programme).ToList() ?? new List<ARGCoursesApplied>();
            SetSelectedCourses(ref aRGPersonalInformation, (PrintProgramme)ProgrammeOption);
            ViewBag.PreferenceCount = SetPreferenceCount(aRGPersonalInformation?.CoursesApplied?.Count ?? 0);
            if (aRGPersonalInformation.StudentAddress.District.ToLower().Trim() == "other")
            {
                aRGPersonalInformation.TotalFee -= ((ARGFormNoMaster)ViewBag.FormNoMaster).AdditionalFeeForNonJK;
            }
            return View(aRGPersonalInformation);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [OutputCache(Duration = 0, Location = System.Web.UI.OutputCacheLocation.None, NoStore = true)]
        public ActionResult PostGraduation(ARGPersonalInformation _ARGPersonalInformation)
        {
            PrintProgramme ProgrammeOption = PrintProgramme.PG;
            SetViewBags(_ARGPersonalInformation.Student_ID, ProgrammeOption);
            List<ARGCoursesApplied> SelectedCourses = _ARGPersonalInformation.CoursesApplied?.Where(x => x.IsClicked)?.ToList();
            List<ARGCoursesApplied> _ARGCoursesApplied = new CourseManager().GetCourseListForAdminRegistration(ProgrammeOption)?.OrderByDescending(x => x.Programme).ToList() ?? new List<ARGCoursesApplied>();

            ViewBag.PreferenceCount = SetPreferenceCount(_ARGPersonalInformation?.CoursesApplied?.Count ?? 0);

            #region Validation section
            if (_ARGPersonalInformation.PhotographPath != null && (!ModelState.IsValidField(nameof(_ARGPersonalInformation.PhotographPath))))
            {
                string errorMessage = ModelState.First(x => x.Key == nameof(_ARGPersonalInformation.PhotographPath)).Value.Errors[0].ErrorMessage;
                TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button><strong>Oh snap!</strong> <a href='#' class='alert-link'> {errorMessage}</a></div>";
                return RedirectToAction("StudentDetail", "Registration", new { area = "CUSrinagarAdminPanel" });
            }
            _ARGPersonalInformation.CoursesApplied = _ARGCoursesApplied;

            if (SelectedCourses.IsNullOrEmpty())
            {
                TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button><strong>Oh snap!</strong> <a href='#' class='alert-link'>Please select course(s)</a></div>";
                return RedirectToAction("StudentDetail", "Registration", new { area = "CUSrinagarAdminPanel" });
            }
            else
            {
                foreach (var item in SelectedCourses ?? new List<ARGCoursesApplied>())
                {
                    if (!_ARGPersonalInformation.CoursesApplied.Any(x => x.Course_ID == item.Course_ID))
                    {
                        TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button><strong>Oh snap!</strong> <a href='#' class='alert-link'>Invalid Course found. Please refresh and try again.</a></div>";
                        return RedirectToAction("StudentDetail", "Registration", new { area = "CUSrinagarAdminPanel" });
                    }
                    //add preference
                    item.Preference = 0;
                }

                #region PreferenceCode
                //if (SelectedCourses.Any(x => x.Preference == null || x.Preference <= 0))
                //{
                //    TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button><strong>Oh snap!</strong> <a href='#' class='alert-link'> Please select preference(s) for all your selected course(s) and try again.</a></div>";
                //    return RedirectToAction("StudentDetail", "Registration", new { area = "CUSrinagarAdminPanel" });
                //}
                //if (SelectedCourses.Any(x => Convert.ToInt32(x.Preference) > SelectedCourses.Count()))
                //{
                //    TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button><strong>Oh snap!</strong> <a href='#' class='alert-link'> Some of your perferences are invalid as per your selected course(s). Please check carefully and try again.</a></div>";
                //    return RedirectToAction("StudentDetail", "Registration", new { area = "CUSrinagarAdminPanel" });
                //}
                //if (SelectedCourses.GroupBy(x => x.Preference).Where(g => g.Count() > 1).Select(y => y.Key)?.Any() ?? false)
                //{
                //    TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button><strong>Oh snap!</strong> <a href='#' class='alert-link'> Duplicate Course preferences found. Please check carefully and try again.</a></div>";
                //    return RedirectToAction("StudentDetail", "Registration", new { area = "CUSrinagarAdminPanel" });
                //} 
                #endregion
            }
            #endregion

            #region Save section
            //--------------- save here ------------------
            _ARGPersonalInformation.CoursesApplied = new List<ARGCoursesApplied>();
            _ARGPersonalInformation.CoursesApplied.AddRange(SelectedCourses);
            int _result = new AdminRegistrationManager().EditCourses(_ARGPersonalInformation, ProgrammeOption);
            SelectedCourses = null;//dispose
            SetDetailViewBags();
            if (_result > 0)
            {
                string msg = "Saved Succesfully.";
                if (_ARGPersonalInformation.FormStatus != FormStatus.InProcess)
                {
                    msg += " But Fee already received therefore courses cannot be changed.";
                }
                TempData["response"] = $"<div class='alert alert-block alert-success'>{msg} <a class='close' href='#' title='close'>X</a></div>";
                return RedirectToAction("StudentDetail", "Registration", new { area = "CUSrinagarAdminPanel" });
            }
            else
            {
                TempData["response"] = $"<div class='alert alert-block alert-danger'>Unable to Save Form.  <a class='close' href='#' title='close'>X</a></div>";
                return RedirectToAction("StudentDetail", "Registration", new { area = "CUSrinagarAdminPanel" });
            }

            #endregion
        }

        #endregion


        #region Detail Section
        public ActionResult StudentDetail()
        {
            SetDetailViewBags();
            return View();

        }
        #endregion

        #region Edit Section
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditStudent(ARGReprint studentDetail)
        {
            if (!ModelState.IsValid)
            {
                SetDetailViewBags();
                return View("StudentDetail", studentDetail);
            }

            Guid id = new AdminRegistrationManager().GetStudentIDByFormNo(studentDetail);
            if ((id == null) || (id == Guid.Empty))
            {
                ModelState.AddModelError("FormNo", "Invalid  Form No");
            }

            if (ModelState.IsValid)
            {
                switch (studentDetail.PrintProgrammeOption)
                {
                    case PrintProgramme.IH:
                        return RedirectToAction("IntegratedHonors", "Registration", new { id = id, ProgrammeOption = studentDetail.PrintProgrammeOption });
                    case PrintProgramme.UG:
                        return RedirectToAction("Graduation", "Registration", new { id = id, ProgrammeOption = studentDetail.PrintProgrammeOption });
                    case PrintProgramme.PG:
                        return RedirectToAction("PostGraduation", "Registration", new { id = id, ProgrammeOption = studentDetail.PrintProgrammeOption });
                    case PrintProgramme.BED:
                        return RedirectToAction("BED", "Registration", new { id = id, ProgrammeOption = studentDetail.PrintProgrammeOption });
                }
            }
            SetDetailViewBags();
            return View("StudentDetail", studentDetail);

        }
        [HttpGet]
        public ActionResult EditStudentDetail(Guid studentId, PrintProgramme programme)
        {

            switch (programme)
            {
                case PrintProgramme.IH:
                    return RedirectToAction("IntegratedHonors", "Registration", new { id = studentId, ProgrammeOption = programme });
                case PrintProgramme.UG:
                    return RedirectToAction("Graduation", "Registration", new { id = studentId, ProgrammeOption = programme });
                case PrintProgramme.PG:
                    return RedirectToAction("PostGraduation", "Registration", new { id = studentId, ProgrammeOption = programme });
                case PrintProgramme.BED:
                    return RedirectToAction("BED", "Registration", new { id = studentId, ProgrammeOption = programme });
            }

            return RedirectToAction("SearchStudent", "StudentInfo");

        }
        #endregion
    }
}