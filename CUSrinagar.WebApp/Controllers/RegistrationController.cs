using CaptchaMvc.HtmlHelpers;
using CUSrinagar.BusinessManagers;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using CUSrinagar.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;


namespace CUSrinagar.WebApp.Controllers
{
    public class RegistrationController : Controller
    {
        #region ViewBags
        [NonAction]
        private void SetViewBagsFormNo(PrintProgramme programme)
        {
            ViewBag.FormNoMater = new RegistrationManager().GetFormNoMaster(programme);
            ViewBag.IHPreferences = SetPreferenceCount(2);
            ViewBag.EngPreferences = SetPreferenceCount(3);
        }

        [NonAction]
        private void SetViewBags(string R = null)
        {
            ViewBag.I2thStreamDDL = Helper.I2thStreamDDL();
            ViewBag.GenderDDL = Helper.GenderDDL();
            ViewBag.ReligionDDL = Helper.ReligionDDL();
            ViewBag.CategoryDDL = new CategoryManager().GetCategoryList();
            ViewBag.DistrictDDL = new GeneralDDLManager().GetDistrictList();
            ViewBag.Provisional = Helper.ProvisionalDDL();
            ViewBag.Session = Helper.SessionDDL();

            if (R.IsBEdMEdAdm())
            {
                ViewBag.Boards = Helper.UniversitiesDDL();
            }
            else
            {
                ViewBag.Boards = Helper.BoardsDDL();
            }

            ViewBag.IsBEdMEdAdm = R.IsBEdMEdAdm();

            ViewBag.AssemblyDDL = new GeneralDDLManager().GetAssemblyList();
            ViewBag.UniversitiesDDL = Helper.UniversitiesDDL();
            ViewBag.Preference = Helper.PreferenceDDL();
            ViewBag.OldCourses = new RegistrationManager().GetCollegeCourses();
        }


        [NonAction]
        private void SetInstructionViewBags()
        {
            CourseManager courseManager = new CourseManager();
            List<PrintProgramme> CoursesRegOpen = courseManager.GetOpenResistrationProgrammeCategories();
            ViewBag.PrintProgrammeOption = Helper.GetSelectList<PrintProgramme>().OrderBy(x => x.Value);
            ProceedOptions proceed = new ProceedOptions
            {
                SelectListItemOptions = Helper.GetSelectForAdmission(CoursesRegOpen)
            };

            // ------lateral entry ----------
            ARGFormNoMaster formNoMaster = null;
            if (CoursesRegOpen?.Any(x => x == PrintProgramme.IH) ?? false)
            {
                formNoMaster = new RegistrationManager().GetFormNoMaster(PrintProgramme.IH); //lateral entry
            }

            new RegistrationManager().GetDDLForIntMed(ref proceed);

            foreach (var item in proceed.SelectListItemOptions ?? new List<SelectListItem>())
            {
                if (item.Value == ((short)PrintProgramme.IH).ToString())
                {
                    if (formNoMaster?.ShowLateralEntry ?? false)
                    {
                        item.Text = "Lateral Entry Programmes";
                    }
                    else
                    {
                        item.Text = item.Text.Replace(" / Engineering", "");
                    }
                    break;
                }
            }

            ViewBag.ProceedProgrammeOption = proceed;
            ViewBag.showProceed = !proceed.SelectListItemOptions.IsNullOrEmpty();

            #region Get Notifications
            List<Notification> ADMNotifications = new NotificationManager().GetAllNotificationList(new GeneralModels.Parameters
            {
                Filters = new List<GeneralModels.SearchFilter> {
                       new GeneralModels.SearchFilter
                       {
                            Column="Status",
                             Operator=SQLOperator.EqualTo,
                              GroupOperation= LogicalOperator.AND,
                              Value="1"
                       }, new GeneralModels.SearchFilter
                       {
                            Column="NotificationType",
                             Operator=SQLOperator.EqualTo,
                              GroupOperation= LogicalOperator.AND,
                              Value="1"
                       }
                 }
            });
            #endregion
            ViewBag.ADMNotifications = ADMNotifications.IsNotNullOrEmpty() ? ADMNotifications.Where(d => d.CreatedOn >= DateTime.Now.AddMonths(-2)).OrderByDescending(x => x.CreatedOn).Take(5).ToList() : null;
        }

        [NonAction]
        private IEnumerable<SelectListItem> SetPreferenceCount(int TotalCourses)
        {
            for (int i = 1; i <= TotalCourses; i++)
                yield return new SelectListItem { Value = i.ToString(), Text = i.ToString() };
        }
        #endregion


        #region Integrated - Honors

        [HttpGet]
        [OutputCache(Duration = 0, Location = System.Web.UI.OutputCacheLocation.None, NoStore = true)]
        public ActionResult IntegratedHonors(string R)
        {
            if (R != null && !(R?.IsBEdMEdAdm() ?? false))
            {
                return RedirectToAction("Instructions", "Registration", new { area = string.Empty });
            }

            SetViewBags(R);
            SetViewBagsFormNo(PrintProgramme.IH);

            string _ExamName = R.IsBEdMEdAdm() ? "Post-Graduation" : "12TH";

            ARGPersonalInformation aRGPersonalInformation = new ARGPersonalInformation();
            aRGPersonalInformation.CoursesApplied = new CourseManager().GetCourseListForRegistration(PrintProgramme.IH)?.ToList();

            if (((ARGFormNoMaster)ViewBag.FormNoMater).ShowLateralEntry)
            {
                if (aRGPersonalInformation.CoursesApplied.All(x => x.Programme == Programme.Engineering))
                {
                    _ExamName = "Diploma/B.Sc";
                    ((List<SelectListItem>)ViewBag.Boards).AddRange(Helper.UniversitiesDDL());
                }
                else
                {
                    _ExamName = "GRADUATION";
                }
            }

            aRGPersonalInformation.AcademicDetails = new List<ARGStudentPreviousQualifications>
                {
                    new ARGStudentPreviousQualifications
                    {
                        ExamName = _ExamName,
                        ReadOnly=true
                 }
                };

            if (aRGPersonalInformation.CoursesApplied.IsNullOrEmpty())
                return RedirectToAction("Instructions", "Registration", new { area = string.Empty });

            aRGPersonalInformation.CoursesApplied = new CourseManager().RemoveIntMed(aRGPersonalInformation.CoursesApplied, R.IsBEdMEdAdm());

            if (aRGPersonalInformation.CoursesApplied.IsNullOrEmpty())
                return RedirectToAction("Instructions", "Registration", new { area = string.Empty });


            ViewBag.PreferenceCount = SetPreferenceCount(aRGPersonalInformation.CoursesApplied?.Count ?? 0);
            return View(aRGPersonalInformation);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [OutputCache(Duration = 0, Location = System.Web.UI.OutputCacheLocation.None, NoStore = true)]
        public ActionResult IntegratedHonors(ARGPersonalInformation _ARGPersonalInformation, bool IsBEdMEdAdm)
        {
            if (IsBEdMEdAdm)
            {
                SetViewBags(("IBMEd").EncryptCookieAndURLSafe());
            }
            else
            {
                SetViewBags();
            }

            SetViewBagsFormNo(PrintProgramme.IH);
            string _ExamName = IsBEdMEdAdm ? "Post-Graduation" : "12TH";

            List<ARGCoursesApplied> SelectedCourses = _ARGPersonalInformation.CoursesApplied?.Where(x => x.IsClicked)?.ToList();
            _ARGPersonalInformation.CoursesApplied = new CourseManager().GetCourseListForRegistration(PrintProgramme.IH)?.ToList() ?? new List<ARGCoursesApplied>();

            if (((ARGFormNoMaster)ViewBag.FormNoMater).ShowLateralEntry)
            {
                if (_ARGPersonalInformation.CoursesApplied.All(x => x.Programme == Programme.Engineering))
                {
                    _ExamName = "Diploma/B.Sc";
                    ((List<SelectListItem>)ViewBag.Boards).AddRange(Helper.UniversitiesDDL());
                }
                else
                {
                    _ExamName = "GRADUATION";
                    if (SelectedCourses.Any(x => (x.SubjectCGPA ?? 0) <= 0))
                    {
                        TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'></button> <a href='#' class='alert-link'> Please Enter Subject CGPA Points for all your selected course(s). Please check carefully and try again.</a></div>";
                        return View(_ARGPersonalInformation);
                    }
                    //if (_ARGPersonalInformation.AcademicDetails.Any(x => (x.OverAllCGPA ?? 0) <= 0))
                    //{
                    //    TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'></button> <a href='#' class='alert-link'> Please Enter Over All CGPA Points for all your selected course(s). Please check carefully and try again.</a></div>";
                    //    return View(_ARGPersonalInformation);
                    //}

                    if (SelectedCourses.Count > 2)
                    {
                        TempData["error2"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button><strong>Oh snap!</strong> <a href='#' class='alert-link'>Only two courses are allowed</a></div>";
                        return RedirectToAction("Instructions");
                    }
                }
            }

            var _RegistrationManager = new RegistrationManager();
            _RegistrationManager.AddGraduationCourse(_ARGPersonalInformation);


            ViewBag.PreferenceCount = SetPreferenceCount(_ARGPersonalInformation?.CoursesApplied?.Count ?? 0);

            #region Validation section
            if (SelectedCourses.IsNullOrEmpty())
            {
                TempData["error2"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button><strong>Oh snap!</strong> <a href='#' class='alert-link'>Please select course(s)</a></div>";
                return RedirectToAction("Instructions");
            }
            else
            {
                foreach (var item in SelectedCourses ?? new List<ARGCoursesApplied>())
                {
                    if (!_ARGPersonalInformation.CoursesApplied.Any(x => x.Course_ID == item.Course_ID))
                    {
                        TempData["error2"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button><strong>Oh snap!</strong> <a href='#' class='alert-link'>Invalid Course found. Please refresh and try again.</a></div>";
                        return RedirectToAction("Instructions");
                    }
                    else if (!IsBEdMEdAdm && _ARGPersonalInformation.CoursesApplied.All(x => x.Programme == Programme.IG || x.Programme == Programme.Engineering) && _ARGPersonalInformation.IsLateralEntry)
                    {
                        if (SelectedCourses.Count(x => x.GroupName == item.GroupName) <= 1)
                        {
                            item.Preference = 1;
                        }
                    }
                    else
                        item.Preference = 0;
                }

                #region PreferenceCode
                if (!IsBEdMEdAdm && _ARGPersonalInformation.CoursesApplied.All(x => x.Programme == Programme.IG || x.Programme == Programme.Engineering) && _ARGPersonalInformation.IsLateralEntry)
                {
                    if (SelectedCourses.Any(x => x.Preference == null || x.Preference <= 0))
                    {
                        TempData["error2"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button><strong>Oh snap!</strong> <a href='#' class='alert-link'> Please select preference(s) for all your selected course(s) and try again.</a></div>";
                        return View(_ARGPersonalInformation);
                    }
                    if (SelectedCourses.Any(x => Convert.ToInt32(x.Preference) > SelectedCourses.Count()))
                    {
                        TempData["error2"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button><strong>Oh snap!</strong> <a href='#' class='alert-link'> Some of your perferences are invalid as per your selected course(s). Please check carefully and try again.</a></div>";
                        return View(_ARGPersonalInformation);
                    }
                    if ((SelectedCourses.GroupBy(x => x.Preference).Where(g => g.Count() > 1).Select(y => y.Key)?.Any() ?? false) && _ARGPersonalInformation.IsLateralEntry)
                    {
                        TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'></button> <a href='#' class='alert-link'> Duplicate Course preferences found. Please check carefully and try again.</a></div>";
                        return View();
                    }
                    else if (SelectedCourses.Any(x => x.Preference > 2) && _ARGPersonalInformation.CoursesApplied.All(x => x.Programme == Programme.IG))
                    {
                        TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'></button> <a href='#' class='alert-link'> Invalid Course preferences found. Please check carefully and try again.</a></div>";
                        return View();
                    }
                    else if (SelectedCourses.Any(x => x.Preference > 3) && _ARGPersonalInformation.CoursesApplied.All(x => x.Programme == Programme.Engineering))
                    {
                        TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'></button> <a href='#' class='alert-link'> Invalid Course preferences found. Please check carefully and try again.</a></div>";
                        return View();
                    }
                }
                #endregion
            }
            if ((_ARGPersonalInformation?.IsProvisional) ?? false)
            {
                int indexOf12th = _ARGPersonalInformation.AcademicDetails.FindIndex(x => x.ExamName.Trim().ToLower() == _ExamName.ToLower());
                if (indexOf12th != -1)
                {
                    _ARGPersonalInformation.AcademicDetails.RemoveAt(indexOf12th);
                    ModelState.Clear();
                    TryValidateModel(_ARGPersonalInformation);
                }
            }
            if (!_RegistrationManager.ValidateDOB(_ARGPersonalInformation.EnteredDOB))
            {
                ModelState.AddModelError("EnteredDOB", "Invalid DOB");
            }

            if ((!this?.IsCaptchaValid("Captcha is not valid") ?? true))
            {
                TempData["error2"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button><strong>Oh snap!</strong> <a href='#' class='alert-link'>Invalid Verification Code, please try again.</a></div>";
                return RedirectToAction("Instructions");
            }

            if (_ARGPersonalInformation.Category.Trim().ToUpper() == "OM")
            {
                _ARGPersonalInformation.CategoryCertificate = null;
                ModelState.Remove(nameof(_ARGPersonalInformation.CategoryCertificate));
            }
            if ((_ARGPersonalInformation.IsProvisional ?? false) || _ARGPersonalInformation.MarksCertificate == null)
            {
                _ARGPersonalInformation.MarksCertificate = null;
                ModelState.Remove(nameof(_ARGPersonalInformation.MarksCertificate));
            }

            #endregion

            #region Save section
            if (!ModelState.IsValid)
            {
                if (_ARGPersonalInformation != null)
                {
                    if (_ARGPersonalInformation.AcademicDetails.Any(x => x.ExamName.ToLower().Trim() != _ExamName.ToLower()))
                    {
                        _ARGPersonalInformation.AcademicDetails.Add(new ARGStudentPreviousQualifications
                        {
                            ExamName = _ExamName,
                            ReadOnly = true
                        });
                    }
                }
                TempData["error2"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button><strong>Oh snap!</strong> <a href='#' class='alert-link'>Invalid data send, please try again.</a></div>";
                return RedirectToAction("Instructions");
            }
            else
            {
                //--------------- save here ------------------
                _ARGPersonalInformation.CoursesApplied = new List<ARGCoursesApplied>();
                _ARGPersonalInformation.CoursesApplied.AddRange(SelectedCourses);
                _ARGPersonalInformation.PreviousUniversityRegnNo = _ARGPersonalInformation.PreviousUniversityRegnNo.ToLower().Trim() == "null" ? null : _ARGPersonalInformation.PreviousUniversityRegnNo;

                int _result = _RegistrationManager.Save(_ARGPersonalInformation, PrintProgramme.IH);
                SelectedCourses = null;//dispose
                if (_result > 0)
                {
                    _RegistrationManager.SendEmailAndSMSAsync(_ARGPersonalInformation, PrintProgramme.IH);
                    return RedirectToAction("Submitted", "Registration", new { id = _ARGPersonalInformation.Student_ID + "/", R = ((int)PrintProgramme.IH).ToString().EncryptCookieAndURLSafe(), area = string.Empty });
                }
                else if (_result == -1)
                {
                    TempData["error2"] = $"<div class='alert-msg error' style='background-color: #F3CBCC !important'>Board Registration No. has already submitted the Form (Email or visit University for any queries). <a class='close' href='#' title='close'>X</a></div>";
                    return RedirectToAction("Instructions");
                }
                else
                {
                    TempData["error2"] = $"<div class='alert-msg error' style='background-color: #F3CBCC !important'>Unable to Submit Form. Please refresh & try again. <a class='close' href='#' title='close'>X</a></div>";
                    return RedirectToAction("Instructions");
                }
            }
            #endregion
        }

        [HttpGet]
        public ActionResult IntegratedHonorsPreview()
        {
            TempData["error"] = "Refresh is not allowed";
            return RedirectToAction("Instructions", "Registration", new { area = string.Empty });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [OutputCache(Duration = 0, Location = System.Web.UI.OutputCacheLocation.None, NoStore = true)]
        public ActionResult IntegratedHonorsPreview(ARGPersonalInformation _ARGPersonalInformation, bool IsBEdMEdAdm)
        {
            if (IsBEdMEdAdm)
            {
                SetViewBags(("IBMEd").EncryptCookieAndURLSafe());
            }
            else
            {
                SetViewBags();
            }

            string _ExamName = IsBEdMEdAdm ? "Post-Graduation" : "12TH";


            List<ARGCoursesApplied> SelectedCourses = _ARGPersonalInformation.CoursesApplied?.Where(x => x.IsClicked)?.ToList();
            _ARGPersonalInformation.CoursesApplied = new CourseManager().GetCourseListForRegistration(PrintProgramme.IH) ?? new List<ARGCoursesApplied>();
            if (_ARGPersonalInformation.IsLateralEntry)
            {
                if (_ARGPersonalInformation.CoursesApplied.All(x => x.Programme == Programme.Engineering))
                {
                    _ExamName = "Diploma/B.Sc";
                    ((List<SelectListItem>)ViewBag.Boards).AddRange(Helper.UniversitiesDDL());
                }
                else
                {
                    _ExamName = "GRADUATION";
                    if (SelectedCourses.Any(x => (x.SubjectCGPA ?? 0) <= 0))
                    {
                        TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'></button> <a href='#' class='alert-link'> Please Enter Subject CGPA Points for all your selected course(s). Please check carefully and try again.</a></div>";
                        return View();
                    }
                    //if (_ARGPersonalInformation.AcademicDetails.Any(x => (x.OverAllCGPA ?? 0) <= 0))
                    //{
                    //    TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'></button> <a href='#' class='alert-link'> Please Enter Over All CGPA Points for all your selected course(s). Please check carefully and try again.</a></div>";
                    //    return View();
                    //}
                }
            }

            var _RegistrationManager = new RegistrationManager();
            _RegistrationManager.AddGraduationCourse(_ARGPersonalInformation);

            ViewBag.PreferenceCount = SetPreferenceCount(_ARGPersonalInformation?.CoursesApplied?.Count ?? 0);

            #region Validation section
            if (SelectedCourses.IsNullOrEmpty())
            {
                TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'></button> <a href='#' class='alert-link'>Please select course(s)</a></div>";
                return View();
            }
            else
            {
                foreach (var item in SelectedCourses ?? new List<ARGCoursesApplied>())
                {
                    if (!_ARGPersonalInformation.CoursesApplied.Any(x => x.Course_ID == item.Course_ID))
                    {
                        TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'></button> <a href='#' class='alert-link'>Invalid Course found. Please refresh and try again.</a></div>";
                        return View();
                    }
                    else if (!IsBEdMEdAdm && _ARGPersonalInformation.CoursesApplied.All(x => x.Programme == Programme.IG || x.Programme==Programme.Engineering) && _ARGPersonalInformation.IsLateralEntry)
                    {
                        if (SelectedCourses.Count(x => x.GroupName == item.GroupName) <= 1)
                        {
                            item.Preference = 1;
                        }
                    }
                    else
                        item.Preference = 0;
                }

                #region PreferenceCode
                if (!IsBEdMEdAdm && _ARGPersonalInformation.CoursesApplied.All(x => x.Programme == Programme.IG || x.Programme == Programme.Engineering) && _ARGPersonalInformation.IsLateralEntry)
                {
                    if (SelectedCourses.Any(x => x.Preference == null || x.Preference <= 0))
                    {
                        TempData["error2"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button><strong>Oh snap!</strong> <a href='#' class='alert-link'> Please select preference(s) for all your selected course(s) and try again.</a></div>";
                        return View(_ARGPersonalInformation);
                    }
                    if (SelectedCourses.Any(x => Convert.ToInt32(x.Preference) > SelectedCourses.Count()))
                    {
                        TempData["error2"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button><strong>Oh snap!</strong> <a href='#' class='alert-link'> Some of your perferences are invalid as per your selected course(s). Please check carefully and try again.</a></div>";
                        return View(_ARGPersonalInformation);
                    }
                    if ((SelectedCourses.GroupBy(x => x.Preference).Where(g => g.Count() > 1).Select(y => y.Key)?.Any() ?? false) && _ARGPersonalInformation.IsLateralEntry)
                    {
                        TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'></button> <a href='#' class='alert-link'> Duplicate Course preferences found. Please check carefully and try again.</a></div>";
                        return View();
                    }
                    else if (SelectedCourses.Any(x => x.Preference > 2) && _ARGPersonalInformation.CoursesApplied.All(x => x.Programme == Programme.IG))
                    {
                        TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'></button> <a href='#' class='alert-link'> Invalid Course preferences found. Please check carefully and try again.</a></div>";
                        return View();
                    }
                    else if (SelectedCourses.Any(x => x.Preference > 3) && _ARGPersonalInformation.CoursesApplied.All(x => x.Programme == Programme.Engineering))
                    {
                        TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'></button> <a href='#' class='alert-link'> Invalid Course preferences found. Please check carefully and try again.</a></div>";
                        return View();
                    }
                }
                #endregion
            }
            if ((_ARGPersonalInformation?.IsProvisional) ?? false)
            {
                int indexOf12th = _ARGPersonalInformation.AcademicDetails.FindIndex(x => x.ExamName.Trim().ToLower() == _ExamName.ToLower());
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
            _ARGPersonalInformation.CategoryCertificate = null;
            ModelState.Remove(nameof(_ARGPersonalInformation.MarksCertificate));
            ModelState.Remove(nameof(_ARGPersonalInformation.CategoryCertificate));
            #endregion

            #region Preview section
            if (!ModelState.IsValid)
            {
                if (_ARGPersonalInformation != null)
                {
                    _ARGPersonalInformation.AcademicDetails.Add(new ARGStudentPreviousQualifications
                    {
                        ExamName = _ExamName,
                        ReadOnly = true
                    });
                }
                TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'></button> <a href='#' class='alert-link'> Data submitted is not valid. Please check carefully and try again.</a></div>";
                return View();
            }
            else
            {
                //--------------- save here ------------------
                _ARGPersonalInformation.CoursesApplied = new List<ARGCoursesApplied>();
                _ARGPersonalInformation.CoursesApplied.AddRange(SelectedCourses);
                new RegistrationManager().AddCommonCourses(ref _ARGPersonalInformation);

                SelectedCourses = null;//dispose
                ViewBag.BytePhotograph = Helper.CompressImage(_ARGPersonalInformation.PhotographPath.InputStream, 259, 194);
                ViewBag.PhotoType = _ARGPersonalInformation.PhotographPath.ContentType;

                _ARGPersonalInformation.PhotographPath = null;
                _ARGPersonalInformation.PreviousUniversityRegnNo = _ARGPersonalInformation.PreviousUniversityRegnNo == "null" ? null : _ARGPersonalInformation.PreviousUniversityRegnNo;

                string[] disttAndState = _ARGPersonalInformation.StudentAddress.District.Split('|');
                _ARGPersonalInformation.StudentAddress.District = disttAndState[0];
                _ARGPersonalInformation.StudentAddress.State = disttAndState[1];
                string[] assembly = _ARGPersonalInformation.StudentAddress.AssemblyConstituency.Split('|');
                _ARGPersonalInformation.StudentAddress.AssemblyConstituency = assembly[0];
                _ARGPersonalInformation.StudentAddress.ParliamentaryConstituency = assembly[1];

                _ARGPersonalInformation.StudentFormNo = "Preview";
                _ARGPersonalInformation.DOB = _RegistrationManager.ConvertEnterdDOBToDateTime(_ARGPersonalInformation.EnteredDOB);
                ViewBag.AppliedFor = PrintProgramme.IH.GetEnumDescription();
                return View("Detail", _ARGPersonalInformation);

            }
            #endregion
        }

        #endregion



        #region Graduation

        [HttpGet]
        [OutputCache(Duration = 0, Location = System.Web.UI.OutputCacheLocation.None, NoStore = true)]
        public ActionResult Graduation()
        {
            SetViewBags();
            SetViewBagsFormNo(PrintProgramme.UG);
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
            aRGPersonalInformation.CoursesApplied = new CourseManager().GetCourseListForRegistration(PrintProgramme.UG);
            if (aRGPersonalInformation.CoursesApplied.IsNullOrEmpty())
                return RedirectToAction("Instructions", "Registration", new { area = string.Empty });

            return View(aRGPersonalInformation);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [OutputCache(Duration = 0, Location = System.Web.UI.OutputCacheLocation.None, NoStore = true)]
        public ActionResult Graduation(ARGPersonalInformation _ARGPersonalInformation)
        {
            SetViewBags();
            SetViewBagsFormNo(PrintProgramme.UG);

            List<ARGCoursesApplied> SelectedCourses = _ARGPersonalInformation.CoursesApplied?.Where(x => x.IsClicked)?.ToList();
            _ARGPersonalInformation.CoursesApplied = new CourseManager().GetCourseListForRegistration(PrintProgramme.UG) ?? new List<ARGCoursesApplied>();

            _ARGPersonalInformation.IsProvisional = false;
            ModelState.Remove(nameof(_ARGPersonalInformation.IsProvisional));

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
                int indexOf12th = _ARGPersonalInformation.AcademicDetails.FindIndex(x => x.ExamName.Trim().ToLower() == "12th");
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
                int _result = new RegistrationManager().Save(_ARGPersonalInformation, PrintProgramme.UG);
                SelectedCourses = null;//dispose
                if (_result > 0)
                {
                    new RegistrationManager().SendEmailAndSMSAsync(_ARGPersonalInformation, PrintProgramme.UG);
                    return RedirectToAction("Submitted", "Registration", new { id = _ARGPersonalInformation.Student_ID + "/", R = ((int)PrintProgramme.UG).ToString().EncryptCookieAndURLSafe(), area = string.Empty });
                }
                else if (_result == -1)
                {
                    TempData["response"] = $"<div class='alert-msg error' style='background-color: #F3CBCC !important'>Board Registration No. has already submitted the Form (Email or visit University for any queries). <a class='close' href='#' title='close'>X</a></div>";
                    return RedirectToAction("Graduation", "Registration", new { area = string.Empty });
                }
                else
                {
                    TempData["response"] = $"<div class='alert-msg error' style='background-color: #F3CBCC !important'>Unable to Submit Form. Please refresh & try again. <a class='close' href='#' title='close'>X</a></div>";
                    return RedirectToAction("Graduation", "Registration", new { area = string.Empty });
                }
            }
            #endregion
        }

        [HttpGet]
        public ActionResult GraduationPreview()
        {
            TempData["error"] = "Refresh is not allowed";
            return RedirectToAction("Instructions", "Registration", new { area = string.Empty });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [OutputCache(Duration = 0, Location = System.Web.UI.OutputCacheLocation.None, NoStore = true)]
        public ActionResult GraduationPreview(ARGPersonalInformation _ARGPersonalInformation)
        {
            SetViewBags();
            List<ARGCoursesApplied> SelectedCourses = _ARGPersonalInformation.CoursesApplied?.Where(x => x.IsClicked)?.ToList();
            var _RegistrationManager = new RegistrationManager();
            _ARGPersonalInformation.CoursesApplied = new CourseManager().GetCourseListForRegistration(PrintProgramme.UG) ?? new List<ARGCoursesApplied>();

            _ARGPersonalInformation.IsProvisional = false;
            ModelState.Remove(nameof(_ARGPersonalInformation.IsProvisional));

            #region Validation section
            if (SelectedCourses.IsNullOrEmpty())
            {
                TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'></button> <a href='#' class='alert-link'>Please select course(s)</a></div>";
                return View("IntegratedHonorsPreview");
            }
            else
            {
                foreach (var item in SelectedCourses ?? new List<ARGCoursesApplied>())
                {
                    if (!_ARGPersonalInformation.CoursesApplied.Any(x => x.Course_ID == item.Course_ID))
                    {
                        TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'></button> <a href='#' class='alert-link'>Invalid Course found. Please refresh and try again.</a></div>";
                        return View("IntegratedHonorsPreview");
                    }
                }
            }
            if ((_ARGPersonalInformation?.IsProvisional) ?? false)
            {
                int indexOf12th = _ARGPersonalInformation.AcademicDetails.FindIndex(x => x.ExamName.Trim().ToLower() == "12th");
                if (indexOf12th != -1)
                {
                    _ARGPersonalInformation.AcademicDetails.RemoveAt(indexOf12th);
                    ModelState.Clear();
                    this.TryValidateModel(_ARGPersonalInformation);
                }
            }
            if (_RegistrationManager.CheckBoardRegNoExists(_ARGPersonalInformation.BoardRegistrationNo, PrintProgramme.UG))
            {
                ModelState.AddModelError("BoardRegistrationNo", "Invalid or Board Registration No. has already submitted the Form (Email or visit University for any queries)");
            }
            if (!_RegistrationManager.ValidateDOB(_ARGPersonalInformation.EnteredDOB))
            {
                ModelState.AddModelError("EnteredDOB", "Invalid DOB");
            }

            _ARGPersonalInformation.MarksCertificate = null;
            ModelState.Remove(nameof(_ARGPersonalInformation.MarksCertificate));
            _ARGPersonalInformation.CategoryCertificate = null;
            ModelState.Remove(nameof(_ARGPersonalInformation.CategoryCertificate));
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
                return View("IntegratedHonorsPreview");
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
                return View("Detail", _ARGPersonalInformation);
            }
            #endregion
        }

        [HttpPost]
        public JsonResult GetColleges(string CourseID)
        {
            try
            {
                Guid Course_ID = Guid.Empty;
                bool validate = Guid.TryParse(CourseID, out Course_ID);
                if (string.IsNullOrWhiteSpace(CourseID) && !validate)
                    return Json(string.Empty, JsonRequestBehavior.DenyGet);
                return Json(new RegistrationManager().GetColleges(Course_ID), JsonRequestBehavior.DenyGet);
            }
            catch (Exception)
            {
                return Json(string.Empty, JsonRequestBehavior.DenyGet);
            }
        }

        #endregion



        #region Post-Graduation
        private List<ARGStudentPreviousQualifications> PrevQualificationPGTable()
        {
            return new List<ARGStudentPreviousQualifications>
                {
                    new ARGStudentPreviousQualifications {
                        ExamName = "GRADUATION",
                        ReadOnly=true
                        },
                     new ARGStudentPreviousQualifications {
                        ExamName = "PG",
                        ReadOnly=true
                        },
                     new ARGStudentPreviousQualifications {
                        ExamName = "B.Ed",
                        Stream="B.Ed",
                        ReadOnly=true
                        }
                };

        }

        [HttpGet]
        [OutputCache(Duration = 0, Location = System.Web.UI.OutputCacheLocation.None, NoStore = true)]
        public ActionResult PostGraduation()
        {
            SetViewBags();
            SetViewBagsFormNo(PrintProgramme.PG);
            ARGPersonalInformation aRGPersonalInformation = new ARGPersonalInformation
            {
                AcademicDetails = PrevQualificationPGTable()
            };

            aRGPersonalInformation.CoursesApplied = new CourseManager().GetCourseListForRegistration(PrintProgramme.PG);
            if (aRGPersonalInformation.CoursesApplied.IsNullOrEmpty())
                return RedirectToAction("Instructions", "Registration", new { area = string.Empty });
            ViewBag.PreferenceCount = SetPreferenceCount(aRGPersonalInformation.CoursesApplied.Count);
            return View(aRGPersonalInformation);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [OutputCache(Duration = 0, Location = System.Web.UI.OutputCacheLocation.None, NoStore = true)]
        public ActionResult PostGraduation(ARGPersonalInformation _ARGPersonalInformation)
        {
            SetViewBags();
            SetViewBagsFormNo(PrintProgramme.PG);
            List<ARGCoursesApplied> SelectedCourses = _ARGPersonalInformation.CoursesApplied?.Where(x => x.IsClicked)?.ToList();
            _ARGPersonalInformation.CoursesApplied = new CourseManager().GetCourseListForRegistration(PrintProgramme.PG)?.OrderByDescending(x => x.Programme).ToList() ?? new List<ARGCoursesApplied>();
            RegistrationManager registrationManager = new RegistrationManager();

            _ARGPersonalInformation.IsProvisional = false;


            ViewBag.PreferenceCount = SetPreferenceCount(_ARGPersonalInformation?.CoursesApplied?.Count ?? 0);

            #region Validation section
            if ((!this?.IsCaptchaValid("Captcha is not valid") ?? true))
            {
                TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button><strong>Oh snap!</strong> <a href='#' class='alert-link'>Invalid Verification Code, please try again.</a></div>";
                return View(_ARGPersonalInformation);
            }

            if (SelectedCourses.IsNullOrEmpty())
            {
                TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button><strong>Oh snap!</strong> <a href='#' class='alert-link'>Please select course(s)</a></div>";
                return View(_ARGPersonalInformation);
            }
            else
            {
                Tuple<bool, ARGStudentPreviousQualifications> result;
                List<ARGStudentPreviousQualifications> finalAcademicDetails = new List<ARGStudentPreviousQualifications>();

                foreach (var item in SelectedCourses ?? new List<ARGCoursesApplied>())
                {
                    if (!_ARGPersonalInformation.CoursesApplied.Any(x => x.Course_ID == item.Course_ID))
                    {
                        TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button><strong>Oh snap!</strong> <a href='#' class='alert-link'>Invalid Course found. Please refresh and try again.</a></div>";
                        return View(_ARGPersonalInformation);
                    }
                    //remove if preference is open
                    item.Preference = 0;

                    if (!(_ARGPersonalInformation?.IsProvisional) ?? false)
                    {
                        result = registrationManager.ValidatePGFormAcademicDetails(item.Course_ID, _ARGPersonalInformation.AcademicDetails);
                        if (result.Item1)
                        {
                            if (!finalAcademicDetails.Any(x => x.ExamName == result.Item2.ExamName))
                                finalAcademicDetails.Add(result.Item2);
                        }
                        else
                        {
                            TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button><strong>Oh snap!</strong> <a href='#' class='alert-link'>Invalid Academic details entered. Please refresh and try again.</a></div>";
                            return View(_ARGPersonalInformation);
                        }
                    }
                }

                #region PreferenceCode
                //if (SelectedCourses.Any(x => x.Preference == null || x.Preference <= 0))
                //{
                //    TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button><strong>Oh snap!</strong> <a href='#' class='alert-link'> Please select preference(s) for all your selected course(s) and try again.</a></div>";
                //    return View(_ARGPersonalInformation);
                //}
                //if (SelectedCourses.Any(x => Convert.ToInt32(x.Preference) > SelectedCourses.Count()))
                //{
                //    TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button><strong>Oh snap!</strong> <a href='#' class='alert-link'> Some of your perferences are invalid as per your selected course(s). Please check carefully and try again.</a></div>";
                //    return View(_ARGPersonalInformation);
                //}
                //if (SelectedCourses.GroupBy(x => x.Preference).Where(g => g.Count() > 1).Select(y => y.Key)?.Any() ?? false)
                //{
                //    TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button><strong>Oh snap!</strong> <a href='#' class='alert-link'> Duplicate Course preferences found. Please check carefully and try again.</a></div>";
                //    return View(_ARGPersonalInformation);
                //}
                #endregion

                if (finalAcademicDetails.IsNotNullOrEmpty())
                {
                    _ARGPersonalInformation.AcademicDetails = finalAcademicDetails;
                    ModelState.Clear();
                    TryValidateModel(_ARGPersonalInformation);
                }
            }

            if ((_ARGPersonalInformation?.IsProvisional) ?? false)
            {
                _ARGPersonalInformation.AcademicDetails = new List<ARGStudentPreviousQualifications>();
                ModelState.Clear();
                TryValidateModel(_ARGPersonalInformation);
            }

            if (!new RegistrationManager().ValidateDOB(_ARGPersonalInformation.EnteredDOB))
            {
                ModelState.AddModelError("EnteredDOB", "Invalid DOB");
            }

            //certificates upload
            if (_ARGPersonalInformation.Category.Trim().ToUpper() == "OM")
            {
                _ARGPersonalInformation.CategoryCertificate = null;
                ModelState.Remove(nameof(_ARGPersonalInformation.CategoryCertificate));
            }
            if ((_ARGPersonalInformation.IsProvisional ?? false) || _ARGPersonalInformation.MarksCertificate == null)
            {
                _ARGPersonalInformation.MarksCertificate = null;
                ModelState.Remove(nameof(_ARGPersonalInformation.MarksCertificate));
            }

            #endregion

            #region Save section
            ModelState.Remove("TotalFee");
            if (!ModelState.IsValid)
            {
                return View(_ARGPersonalInformation);
            }
            else
            {
                //--------------- save here ------------------
                _ARGPersonalInformation.CoursesApplied = new List<ARGCoursesApplied>();
                _ARGPersonalInformation.CoursesApplied.AddRange(SelectedCourses);
                _ARGPersonalInformation.Preference = null;
                int _result = registrationManager.Save(_ARGPersonalInformation, PrintProgramme.PG);
                SelectedCourses = null;//dispose

                if (_result > 0)
                {
                    registrationManager.SendEmailAndSMSAsync(_ARGPersonalInformation, PrintProgramme.PG);
                    return RedirectToAction("Submitted", "Registration", new { id = _ARGPersonalInformation.Student_ID + "/", R = ((int)PrintProgramme.PG).ToString().EncryptCookieAndURLSafe(), area = string.Empty });
                }
                else if (_result == -1)
                {
                    TempData["response"] = $"<div class='alert-msg error' style='background-color: #F3CBCC !important'>Board Registration No. has already submitted the Form (Email or visit University for any queries). <a class='close' href='#' title='close'>X</a></div>";
                    return RedirectToAction("PostGraduation", "Registration", new { area = string.Empty });
                }
                else
                {
                    TempData["response"] = $"<div class='alert-msg error' style='background-color: #F3CBCC !important'>Unable to Submit Form. Please refresh & try again. <a class='close' href='#' title='close'>X</a></div>";
                    return RedirectToAction("PostGraduation", "Registration", new { area = string.Empty });
                }
            }
            #endregion
        }

        [HttpGet]
        public ActionResult PostGraduationPreview()
        {
            TempData["error"] = "Refresh is not allowed";
            return RedirectToAction("Instructions", "Registration", new { area = string.Empty });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [OutputCache(Duration = 0, Location = System.Web.UI.OutputCacheLocation.None, NoStore = true)]
        public ActionResult PostGraduationPreview(ARGPersonalInformation _ARGPersonalInformation)
        {
            SetViewBags();
            var _RegistrationManager = new RegistrationManager();
            List<ARGCoursesApplied> SelectedCourses = _ARGPersonalInformation.CoursesApplied?.Where(x => x.IsClicked)?.ToList();
            _ARGPersonalInformation.CoursesApplied = new CourseManager().GetCourseListForRegistration(PrintProgramme.PG) ?? new List<ARGCoursesApplied>();


            _ARGPersonalInformation.IsProvisional = false;

            #region Validation section
            if (SelectedCourses.IsNullOrEmpty())
            {
                TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button><strong>Oh snap!</strong> <a href='#' class='alert-link'>Please select course(s)</a></div>";
                return View("IntegratedHonorsPreview");
            }
            else
            {
                Tuple<bool, ARGStudentPreviousQualifications> result;
                List<ARGStudentPreviousQualifications> finalAcademicDetails = new List<ARGStudentPreviousQualifications>();
                foreach (var item in SelectedCourses ?? new List<ARGCoursesApplied>())
                {
                    if (!_ARGPersonalInformation.CoursesApplied.Any(x => x.Course_ID == item.Course_ID))
                    {
                        TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button><strong>Oh snap!</strong> <a href='#' class='alert-link'>Invalid Course found. Please refresh and try again.</a></div>";
                        return View("IntegratedHonorsPreview");
                    }
                    //remove if preference is open
                    item.Preference = 0;
                    if (!(_ARGPersonalInformation?.IsProvisional) ?? false)
                    {
                        result = _RegistrationManager.ValidatePGFormAcademicDetails(item.Course_ID, _ARGPersonalInformation.AcademicDetails);
                        if (result.Item1)
                        {
                            if (!finalAcademicDetails.Any(x => x.ExamName == result.Item2.ExamName))
                                finalAcademicDetails.Add(result.Item2);
                        }
                        else
                        {
                            TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button><strong>Oh snap!</strong> <a href='#' class='alert-link'>Invalid Academic details entered. Please refresh and try again.</a></div>";
                            return View("IntegratedHonorsPreview");
                        }
                    }
                }
                if (finalAcademicDetails.IsNotNullOrEmpty())
                {
                    _ARGPersonalInformation.AcademicDetails = finalAcademicDetails;
                    ModelState.Clear();
                    TryValidateModel(_ARGPersonalInformation);
                }

                #region PreferenceCode
                //if (SelectedCourses.Any(x => x.Preference == null || x.Preference <= 0))
                //{
                //    TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button><strong>Oh snap!</strong> <a href='#' class='alert-link'> Please select preference(s) for all your selected course(s) and try again.</a></div>";
                //    return View("IntegratedHonorsPreview");
                //}
                //if (SelectedCourses.Any(x => Convert.ToInt32(x.Preference) > SelectedCourses.Count()))
                //{
                //    TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button><strong>Oh snap!</strong> <a href='#' class='alert-link'> Some of your perferences are invalid as per your selected course(s). Please check carefully and try again.</a></div>";
                //    return View("IntegratedHonorsPreview");
                //}
                //if (SelectedCourses.GroupBy(x => x.Preference).Where(g => g.Count() > 1).Select(y => y.Key)?.Any() ?? false)
                //{
                //    TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button><strong>Oh snap!</strong> <a href='#' class='alert-link'> Duplicate Course preferences found. Please check carefully and try again.</a></div>";
                //    return View("IntegratedHonorsPreview");
                //} 
                #endregion
            }
            if ((_ARGPersonalInformation?.IsProvisional) ?? false)
            {
                _ARGPersonalInformation.AcademicDetails = new List<ARGStudentPreviousQualifications>();
                ModelState.Clear();
                TryValidateModel(_ARGPersonalInformation);
            }

            if (!_RegistrationManager.ValidateDOB(_ARGPersonalInformation.EnteredDOB))
            {
                ModelState.AddModelError("EnteredDOB", "Invalid DOB");
            }

            _ARGPersonalInformation.MarksCertificate = null;
            ModelState.Remove(nameof(_ARGPersonalInformation.MarksCertificate));
            _ARGPersonalInformation.CategoryCertificate = null;
            ModelState.Remove(nameof(_ARGPersonalInformation.CategoryCertificate));
            #endregion

            #region Preview section
            if (!ModelState.IsValid)
            {
                TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'></button> <a href='#' class='alert-link'> Data submitted is not valid. Please check carefully and try again.</a></div>";
                return View("IntegratedHonorsPreview");
            }
            else
            {
                //--------------- save here ------------------
                _ARGPersonalInformation.CoursesApplied = new List<ARGCoursesApplied>();
                _ARGPersonalInformation.CoursesApplied.AddRange(SelectedCourses);

                ViewBag.BytePhotograph = Helper.CompressImage(_ARGPersonalInformation.PhotographPath.InputStream, 259, 194);
                ViewBag.PhotoType = _ARGPersonalInformation.PhotographPath.ContentType;

                _ARGPersonalInformation.PhotographPath = null;

                string[] disttAndState = _ARGPersonalInformation.StudentAddress.District.Split('|');
                _ARGPersonalInformation.StudentAddress.District = disttAndState[0];
                _ARGPersonalInformation.StudentAddress.State = disttAndState[1];
                string[] assembly = _ARGPersonalInformation.StudentAddress.AssemblyConstituency.Split('|');
                _ARGPersonalInformation.StudentAddress.AssemblyConstituency = assembly[0];
                _ARGPersonalInformation.StudentAddress.ParliamentaryConstituency = assembly[1];
                _ARGPersonalInformation.StudentFormNo = "Preview";
                _ARGPersonalInformation.Preference = null;
                SelectedCourses = null;
                _ARGPersonalInformation.DOB = _RegistrationManager.ConvertEnterdDOBToDateTime(_ARGPersonalInformation.EnteredDOB);
                ViewBag.AppliedFor = PrintProgramme.PG.GetEnumDescription();
                return View("Detail", _ARGPersonalInformation);

            }
            #endregion
        }
        #endregion


        #region Instructions - Reprint

        [HttpGet]
        public ActionResult Detail(string id, string R)
        {
            if (!Guid.TryParse(id + "", out Guid result) || !Enum.TryParse((R + "").DecryptCookieAndURLSafe(), out PrintProgramme programme))
            {
                return RedirectToAction("Instructions", "Registration", new { area = string.Empty });
            }
            ARGPersonalInformation studentPersonalInfo = new RegistrationManager().GetStudentByID(result, programme);
            ViewBag.AppliedFor = programme.GetEnumDescription();
            if (studentPersonalInfo != null)
            {
                if ((programme == PrintProgramme.IH || programme == PrintProgramme.UG) && studentPersonalInfo.Batch <= 2018)
                { }
                else if (studentPersonalInfo.FormStatus == FormStatus.InProcess)
                {
                    TempData["error2"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'> Please Pay Fee online by Signing-In to <a href='/Account/StudentZone/'>Student Zone</a>, before printing your Form.</a></div>";
                    return RedirectToAction("Instructions", "Registration", new { area = string.Empty });
                }
                else if (studentPersonalInfo.TotalFee > 0 && studentPersonalInfo.PaymentDetail.IsNullOrEmpty())
                {
                    TempData["error2"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'> Please Pay Fee online by Signing-In to <a href='/Account/StudentZone/'>Student Zone</a>, before printing your Form.</a></div>";
                    return RedirectToAction("Instructions", "Registration", new { area = string.Empty });
                }

                if (studentPersonalInfo.CoursesApplied.IsNullOrEmpty())
                    studentPersonalInfo.CoursesApplied = new List<ARGCoursesApplied>();

                return View(studentPersonalInfo);
            }

            return RedirectToAction("Instructions", "Registration", new { area = string.Empty });
        }

        [HttpGet]
        public ActionResult Instructions(string cell = "")
        {
            ViewBag.SelectedTab = cell.ToLower();

            #region Close New Admission
            List<PrintProgramme> _CoursesRegOpen = new CourseManager().GetOpenResistrationProgrammeCategories();
            RegistrationManager registrationManager = new RegistrationManager();

            foreach (PrintProgramme item in _CoursesRegOpen ?? new List<PrintProgramme>())
            {
                registrationManager.CloseNewAdmission(item);
            }
            _CoursesRegOpen = null;//dispose
            #endregion

            SetInstructionViewBags();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Instructions(ARGReprint SearchDetails)
        {
            PrintProgramme programme = SearchDetails.PrintProgrammeOption;
            ViewBag.SelectedTab = "";
            SetInstructionViewBags();
            ViewBag.PrintProgrammeOption = Helper.GetSelectList<PrintProgramme>();
            ViewBag.CurrentTab = "PrintTab";
            if (!ModelState.IsValid)
                return View(SearchDetails);

            if (!new RegistrationManager().ValidateDOB(SearchDetails.EnteredDOB))
            {
                ModelState.AddModelError("EnteredDOB", "Invalid DOB");
            }
            Guid id = new RegistrationManager().GetStudentIDByFormNoAndDOBCUS(SearchDetails);
            if ((id == null) || (id == Guid.Empty))
            {
                ModelState.AddModelError("InvalidFormNo", "Invalid  Form No");
                TempData["error2"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'> Form does not exist.</a></div>";
            }

            if (ModelState.IsValid)
            {
                ARGPersonalInformation studentPersonalInfo = new RegistrationManager().GetStudentPersonalInfoOnly(id, SearchDetails.PrintProgrammeOption);
                if (studentPersonalInfo == null)
                {
                    TempData["error2"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'> Form does not exit.</a></div>";
                    return RedirectToAction("Instructions", "Registration", new { area = string.Empty });
                }
                return RedirectToAction("Detail", "Registration", new { id = id + "/", R = ((int)SearchDetails.PrintProgrammeOption).ToString().EncryptCookieAndURLSafe(), area = string.Empty });
            }
            return View();
        }
        #endregion



        #region Board Data
        [HttpPost]
        public JsonResult BoardData(string BoardRegistrationNo, PrintProgramme programme)
        {
            var _RegistrationManager = new RegistrationManager();
            if (_RegistrationManager.CheckBoardRegNoExists(BoardRegistrationNo, programme))
                BoardRegistrationNo = string.Empty;
            return Json(_RegistrationManager.GetBoardRegistrationData(BoardRegistrationNo), JsonRequestBehavior.DenyGet);
        }
        #endregion



        #region RemoteValidation
        public async Task<JsonResult> BoardRegNoExists(string BoardRegistrationNo, PrintProgramme programme)
        {
            if (string.IsNullOrWhiteSpace(BoardRegistrationNo))
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            string FirstChar = BoardRegistrationNo?.Substring(0, 1) + string.Empty;
            string LastChar = BoardRegistrationNo.Substring(BoardRegistrationNo.Length - 1, 1) + string.Empty;

            if (!(new Regex("^[a-zA-Z0-9]*$").IsMatch(FirstChar)) || !(new Regex("^[a-zA-Z0-9]*$").IsMatch(LastChar)))
            {
                return Json("Invalid Board Regn. No.", JsonRequestBehavior.AllowGet);
            }
            return Json(!(await new RegistrationManager().CheckBoardRegNoExistsAsync(BoardRegistrationNo, programme)), JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> MobileNoExists(ARGStudentAddress StudentAddress)
        {
            bool degreeIssuingForm = Request?.UrlReferrer?.AbsoluteUri?.ToLower()?.Contains("/issuanceofdegreecertificateform?s=") ?? false;
            if (string.IsNullOrWhiteSpace(StudentAddress.Mobile))
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            if (!new Regex(@"^(?:(?:\+|0{0,2})(\s*[\-]\s*)?|[0]?)?[6789]\d{9,9}$").IsMatch(StudentAddress.Mobile)) //fully validate number 
            {
                return Json("Invalid Mobile No", JsonRequestBehavior.AllowGet);
            }
            if (new Regex(@"([0-9])(\1){4}").IsMatch(StudentAddress.Mobile)) //block contineous repeatation of digit in a number
            {
                return Json("Invalid Mobile No", JsonRequestBehavior.AllowGet);
            }
            return Json(!await new RegistrationManager().CheckMobileExistsAsync(StudentAddress.Mobile, StudentAddress.Student_ID, StudentAddress.PProgramme, degreeIssuingForm), JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> EmailExists(ARGStudentAddress StudentAddress)
        {
            bool degreeIssuingForm = Request?.UrlReferrer?.AbsoluteUri?.ToLower()?.Contains("/issuanceofdegreecertificateform?s=") ?? false;
            if (string.IsNullOrWhiteSpace(StudentAddress.Email))
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            if (!new Regex(@"^([a-zA-Z0-9_.-])+@([a-zA-Z0-9_.-])+\.([a-zA-Z])+([a-zA-Z])+").IsMatch(StudentAddress.Email))
            {
                return Json("Invalid Email", JsonRequestBehavior.AllowGet);
            }
            string domain = StudentAddress.Email.ToLower().Split('@')[1];
            if (domain.Contains("test") || domain.Contains("dummy") || domain.Contains("demo")
                || domain.Contains("fake") || domain.Contains("abc") || domain.Contains("xyz") || domain.Contains("temp"))
            {
                return Json("Invalid Email", JsonRequestBehavior.AllowGet);
            }
            return Json(!await new RegistrationManager().CheckEmailExistsAsync(StudentAddress.Email, StudentAddress.Student_ID, StudentAddress.PProgramme, degreeIssuingForm), JsonRequestBehavior.AllowGet);
        }

        public JsonResult ValidateDOB(string EnteredDOB)
        {
            if (string.IsNullOrWhiteSpace(EnteredDOB))
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }

            if (!new Regex(@"^(?:(?:31(\/|-|\.)(?:0?[13578]|1[02]))\1|(?:(?:29|30)(\/|-|\.)(?:0?[1,3-9]|1[0-2])\2))(?:(?:1[6-9]|[2-9]\d)?\d{2})$|^(?:29(\/|-|\.)0?2\3(?:(?:(?:1[6-9]|[2-9]\d)?(?:0[48]|[2468][048]|[13579][26])|(?:(?:16|[2468][048]|[3579][26])00))))$|^(?:0?[1-9]|1\d|2[0-8])(\/|-|\.)(?:(?:0?[1-9])|(?:1[0-2]))\4(?:(?:1[6-9]|[2-9]\d)?\d{4})$").IsMatch(EnteredDOB))
            {
                return Json("Invalid DoB", JsonRequestBehavior.AllowGet);
            }
            else
            {
                DateTime DOB = new RegistrationManager().ConvertEnterdDOBToDateTime(EnteredDOB);
                if (DOB == DateTime.MinValue)
                {
                    return Json("Invalid DoB", JsonRequestBehavior.AllowGet);
                }
                else if (DOB.Date < DateTime.Now.AddYears(-60).Date || DOB.Date > DateTime.Now.AddYears(-10).Date)
                {
                    return Json("Invalid DoB", JsonRequestBehavior.AllowGet);
                }
                return Json(true, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion



        #region Proceed Section
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Proceed(ProceedOptions proceedOptions)
        {
            if (ModelState.IsValid)
            {
                switch (proceedOptions.ProceedProgrammeOption)
                {
                    case PrintProgramme.IH:
                        return RedirectToAction("IntegratedHonors", "Registration", new { area = string.Empty });
                    case PrintProgramme.UG:
                        return RedirectToAction("Graduation", "Registration", new { area = string.Empty });
                    case PrintProgramme.PG:
                        return RedirectToAction("PostGraduation", "Registration", new { area = string.Empty });
                    case PrintProgramme.BED:
                        return RedirectToAction("IntegratedHonors", "Registration", new { R = ("IBMEd").EncryptCookieAndURLSafe(), area = string.Empty });
                }
            }
            return RedirectToAction("Instructions", "Registration", new { area = string.Empty });

        }
        #endregion


        #region AdmitCards
        private void setAdmitCardViewBags(List<PrintProgramme> _PrintProgrammeOption)
        {
            IEnumerable<SelectListItem> ProgrammeOption = Helper.GetSelectForAdmission(_PrintProgrammeOption).OrderBy(x => x.Text);
            foreach (var item in ProgrammeOption)
            {
                if (item.Value == ((short)PrintProgramme.IH).ToString())
                {
                    item.Text = new RegistrationManager().IsEngLateralEntry(PrintProgramme.IH) ? "Engineering (Lateral Entry)" : item.Text.Replace(" / Engineering", "");
                    break;
                }
            }
            ViewBag.PrintProgrammeOption = ProgrammeOption;
        }

        [HttpGet]
        [OutputCache(Duration = 0, Location = System.Web.UI.OutputCacheLocation.None, NoStore = true)]
        public ActionResult AdmitCard()
        {
            List<PrintProgramme> _PrintProgrammeOption = new RegistrationManager().GetAdmitCardCategory();
            if (_PrintProgrammeOption.IsNullOrEmpty())
            {
                ViewBag.PrintProgrammeOption = null;
                return View();
            }
            setAdmitCardViewBags(_PrintProgrammeOption);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [OutputCache(Duration = 0, Location = System.Web.UI.OutputCacheLocation.None, NoStore = true)]
        public ActionResult AdmitCard(ARGReprint _admitCardInfo)
        {
            List<PrintProgramme> printProgrammeOption = new RegistrationManager().GetAdmitCardCategory();
            setAdmitCardViewBags(printProgrammeOption);

            if (!(printProgrammeOption?.Any(x => x == _admitCardInfo.PrintProgrammeOption) ?? false))
                ModelState.AddModelError(nameof(_admitCardInfo.PrintProgrammeOption), "Invalid");

            ModelState.Remove("Batch");
            if (!ModelState.IsValid)
                return View(_admitCardInfo);

            Guid id = new RegistrationManager().GetStudentIDByFormNoAndDOB(_admitCardInfo);
            if ((id == null) || (id == Guid.Empty))
            {
                TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>Admit Card for Form No. {_admitCardInfo.FormNo} does not exist</a></div>";
                return View();
            }
            if (ModelState.IsValid)
            {
                return RedirectToAction("PrintAdmitCard", "Registration", new { id = id + "/", R = ((int)_admitCardInfo.PrintProgrammeOption).ToString().EncryptCookieAndURLSafe(), area = string.Empty });
            }
            return View();
        }


        [HttpGet]
        public ActionResult PrintAdmitCard(string id, string R)
        {
            if (!Guid.TryParse(id + "", out Guid result) || !Enum.TryParse((R + "").DecryptCookieAndURLSafe(), out PrintProgramme programme))
            {
                return RedirectToAction("AdmitCard", "Registration", new { area = string.Empty });
            }

            List<PrintProgramme> printProgrammeOption = new RegistrationManager().GetAdmitCardCategory();
            if (!(printProgrammeOption?.Any(x => x == programme) ?? false))
            {
                TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>Data submitted is invalid.</a></div>";
                return RedirectToAction("AdmitCard", "Registration", new { area = string.Empty });
            }
            ARGPersonalInformation studentPersonalInfo = new RegistrationManager().GetStudentByID(result, programme);
            if (studentPersonalInfo == null)
            {
                TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>Form not found</a></div>";
                return RedirectToAction("AdmitCard", "Registration", new { area = string.Empty });
            }

            if (studentPersonalInfo.FormStatus != FormStatus.FeePaid)
            {
                if (studentPersonalInfo.FormStatus == FormStatus.InProcess)
                    TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>Form No. {studentPersonalInfo.StudentFormNo} didn't Paid Registration Fee.</a></div>";

                else
                    TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>Form No. {studentPersonalInfo.StudentFormNo} has been {studentPersonalInfo.FormStatus.GetEnumDescription()}.</a></div>";

                return RedirectToAction("AdmitCard", "Registration", new { area = string.Empty });
            }
            else if (studentPersonalInfo.Batch < new RegistrationManager().GetAdmitCardCategoryYear(programme))
            {
                TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>Forms of Admission Session {studentPersonalInfo.CreatedOn.Year} are not allowed to download Admit Cards.</a></div>";
                return RedirectToAction("AdmitCard", "Registration", new { area = string.Empty });
            }

            ViewBag.AppliedFor = programme.GetEnumDescription();
            if (studentPersonalInfo != null)
            {
                new EntranceCentersManager().GetCenterDetails(ref studentPersonalInfo, programme);
                if (studentPersonalInfo.CoursesApplied.All(x => x.EntranceCentersAllotment == null))
                {
                    TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>Center are not alloted yet. Contact I.T Section</a></div>";
                    return RedirectToAction("AdmitCard", "Registration", new { area = string.Empty });
                }
                return View(studentPersonalInfo);
            }
            return RedirectToAction("AdmitCard", "Registration", new { area = string.Empty });
        }
        #endregion


        #region FormSubmittedSuccessfully - Make Payment 

        [HttpGet]
        [OutputCache(Duration = 0, Location = System.Web.UI.OutputCacheLocation.None, NoStore = true)]
        public ActionResult Submitted(string id, string R)
        {
            if (!Guid.TryParse(id + "", out Guid result) || !Enum.TryParse((R + "").DecryptCookieAndURLSafe(), out PrintProgramme programme))
            {
                return RedirectToAction("Instructions", "Registration", new { area = string.Empty });
            }
            if (new CourseManager().GetCourseListForRegistration(programme).IsNullOrEmpty())
            {
                //check for closing date
                return RedirectToAction("Instructions", "Registration", new { area = string.Empty });
            }
            ARGPersonalInformation studentPersonalInfo = new RegistrationManager().GetStudentPersonalInfoOnly(result, programme);
            if (studentPersonalInfo == null)
                return RedirectToAction("Instructions", "Registration", new { area = string.Empty });
            if (studentPersonalInfo.FormStatus != FormStatus.InProcess)
            {
                TempData["error"] = $"It seems that you have already paid fee. Note : You should be able to print your Form.";
                return RedirectToAction("Instructions", "Registration", new { area = string.Empty });
            }
            ViewBag.AppliedFor = programme.GetEnumDescription();
            ViewBag.EncryptedAppliedFor = R;
            return View(studentPersonalInfo);
        }


        #region payment
        [HttpGet]
        [OutputCache(Duration = 0, Location = System.Web.UI.OutputCacheLocation.None, NoStore = true)]
        public ActionResult MakePayment()
        {
            //List<PrintProgramme> _PrintProgrammeOption = new BillDeskManager().GetPaymentCategory();
            //if (_PrintProgrammeOption.IsNullOrEmpty())
            //{
            //    ViewBag.PrintProgrammeOption = null;
            //    return View();
            //}
            ViewBag.PrintProgrammeOption = null;//Helper.GetSelectForAdmission(_PrintProgrammeOption).OrderBy(x => x.Text);
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [OutputCache(Duration = 0, Location = System.Web.UI.OutputCacheLocation.None, NoStore = true)]
        public ActionResult MakePayment(ARGReprint SearchDetails)
        {
            List<PrintProgramme> _PrintProgrammeOption = new BillDeskManager().GetPaymentCategory();
            ViewBag.PrintProgrammeOption = Helper.GetSelectForAdmission(_PrintProgrammeOption).OrderBy(x => x.Text);

            if (!ModelState.IsValid)
                return View(SearchDetails);

            if (!new RegistrationManager().ValidateDOB(SearchDetails.EnteredDOB))
            {
                ModelState.AddModelError("EnteredDOB", "Invalid DOB");
                return View(SearchDetails);
            }
            Guid id = new RegistrationManager().GetStudentIDByFormNoAndDOBCUS(SearchDetails);
            if ((id == null) || (id == Guid.Empty))
            {
                TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'> Form does not exist.</a></div>";
                return View();
            }

            ARGPersonalInformation aRGPersonalInformation = new RegistrationManager().GetStudentPersonalInfoOnly(id, SearchDetails.PrintProgrammeOption);

            //fee already submitted
            if (aRGPersonalInformation.FormStatus != FormStatus.InProcess)
            {
                TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'> It seems that you have already paid fee. Note : You should be able to print your Form.</a> <a href='/Registration/Instructions?cell=reprint' target='_blank'>Reprint Here</a></div>";
                return View();
            }
            return RedirectToAction("Index", "Payment", new { id = id + "/", R = ((int)SearchDetails.PrintProgrammeOption).ToString().EncryptCookieAndURLSafe(), area = string.Empty });
        }
        #endregion
        #endregion
    }
}