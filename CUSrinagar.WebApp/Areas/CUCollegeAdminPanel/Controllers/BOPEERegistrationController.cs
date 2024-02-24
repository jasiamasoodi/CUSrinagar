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
using CUSrinagar.DataManagers;

namespace CUSrinagar.WebApp.CUCollegeAdminPanel.Controllers
{
    [OAuthorize(AppRoles.AllowBOPEERegistration)]
    public class BOPEERegistrationController : Controller
    {
        #region ViewBags
        string errorMsg = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>##</a></div>";

        [NonAction]
        private List<short> SetViewBags(PrintProgramme programme)
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
            ViewBag.PrintProgramme = (int)programme;

            string collegeCode = AppUserHelper.CollegeCode;
            if (collegeCode.ToLower().Trim() == "gcet")
            {
                ViewBag.PrintProgrammeDes = "Engineering Programmes";
            }
            else if (collegeCode.ToLower().Trim() == "iase")
            {
                ViewBag.PrintProgrammeDes = "B.Ed/UG Programmes";
            }
            else
            {
                ViewBag.PrintProgrammeDes = programme.GetEnumDescription().Replace(" / Engineering", "") + " Programmes";
            }
            #region validation
            //List<short> BatchsToAllow = new BOPEERegistrationManager().GetBatches(programme);
            //if (BatchsToAllow.IsNotNullOrEmpty())
            //{
            //    ViewBag.MinBatch = BatchsToAllow[0];
            //    ViewBag.MaxBatch = BatchsToAllow[BatchsToAllow.Count - 1];
            //}
            //else
            //{
            //    TempData["response"] = errorMsg.Replace("##", "New Registration not allowed now. CUSRegistration No has already been alloted to all batchs, please contact University I.T Cell").Replace("<button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button>", "");
            //    ViewBag.MinBatch = -1;
            //    ViewBag.MaxBatch = -1;
            //} 
            #endregion
            ViewBag.MinBatch = DateTime.Now.Year - 1;
            ViewBag.MaxBatch = DateTime.Now.Year + 1;
            List<short> BatchsToAllow = new List<short> { (short)(DateTime.Now.Year - 1), (short)(DateTime.Now.Year), (short)(DateTime.Now.Year + 1) };
            return BatchsToAllow;
        }

        private static ARGPersonalInformation FillAcademicDetails(PrintProgramme printProgramme)
        {
            ARGPersonalInformation aRGPersonalInformation = null;
            if (printProgramme == PrintProgramme.BED)
            {
                aRGPersonalInformation = new ARGPersonalInformation
                {
                    AcademicDetails = new List<ARGStudentPreviousQualifications>
                    {
                        new   ARGStudentPreviousQualifications {
                            ExamName = "GRADUATION",
                            ReadOnly=true
                        }
                    }
                };
            }
            else if (printProgramme == PrintProgramme.UG || printProgramme == PrintProgramme.IH)
            {
                aRGPersonalInformation = new ARGPersonalInformation
                {
                    AcademicDetails = new List<ARGStudentPreviousQualifications>
                    {
                        new   ARGStudentPreviousQualifications {
                            ExamName = "12TH",
                            ReadOnly=true
                        }
                    }
                };
            }
            else if (printProgramme == PrintProgramme.PG)
            {
                aRGPersonalInformation = new ARGPersonalInformation
                {
                    AcademicDetails = new List<ARGStudentPreviousQualifications>
                    {
                        new   ARGStudentPreviousQualifications {
                            ExamName = "12TH",
                            ReadOnly=true
                        },
                        new   ARGStudentPreviousQualifications {
                            ExamName = "GRADUATION",
                            ReadOnly=true
                        }
                    }
                };
            }

            return aRGPersonalInformation;
        }
        #endregion

        [HttpGet]
        public ActionResult AddStudentDetails()
        {
            List<ARGCoursesApplied> coursesAppliedDetails = new List<ARGCoursesApplied>();

            //------------- temp ------------------- 
            string collegeCode = AppUserHelper.CollegeCode;
            if (collegeCode.ToLower().Trim() == "iase" || collegeCode.ToLower().Trim() == "gcet")
            {
                //---- origional line ------------
                coursesAppliedDetails = new CourseManager().GetCoursesAppliedMapping(AppUserHelper.College_ID ?? Guid.Empty);
            }
            else
            {
                coursesAppliedDetails = new CourseManager().GetCoursesAppliedMapping(AppUserHelper.College_ID ?? Guid.Empty, PrintProgramme.PG);
            }

            if (coursesAppliedDetails.IsNullOrEmpty())
            {
                TempData["response"] = errorMsg.Replace("##", "Your college is not allowed for this feature.");
                return RedirectToAction("Index", "Dashboard", new { area = "CUCollegeAdminPanel" });
            }
            PrintProgramme printProgramme = new RegistrationManager().MappingTable(coursesAppliedDetails[0].PrintProgramme);
            SetViewBags(printProgramme);

            ARGPersonalInformation aRGPersonalInformation = FillAcademicDetails(coursesAppliedDetails[0].PrintProgramme);

            aRGPersonalInformation.CoursesApplied = coursesAppliedDetails;
            return View(aRGPersonalInformation);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddStudentDetails(ARGPersonalInformation _ARGPersonalInformation, PrintProgramme programme, string StdBoardRegistrationNo)
        {
            List<short> BatchsToAllow = SetViewBags(programme);
            if (BatchsToAllow.IsNullOrEmpty() || BatchsToAllow.All(x => x != _ARGPersonalInformation.Batch))
            {
                if (BatchsToAllow.IsNotNullOrEmpty())
                {
                    TempData["response"] = errorMsg.Replace("##", "Invalid Batch");
                }
                return RedirectToAction("AddStudentDetails");
            }

            List<ARGCoursesApplied> SelectedCourses = _ARGPersonalInformation.CoursesApplied?.Where(x => x.IsClicked)?.ToList();
            List<ARGCoursesApplied> coursesAppliedDetails = new List<ARGCoursesApplied>();
            //------------- temp ------------------- 
            string collegeCode = AppUserHelper.CollegeCode;
            if (collegeCode.ToLower().Trim() == "iase" || collegeCode.ToLower().Trim() == "gcet")
            {
                //---- origional line ------------
                coursesAppliedDetails = new CourseManager().GetCoursesAppliedMapping(AppUserHelper.College_ID ?? Guid.Empty);
            }
            else
            {
                coursesAppliedDetails = new CourseManager().GetCoursesAppliedMapping(AppUserHelper.College_ID ?? Guid.Empty, PrintProgramme.PG);
            }


            #region Validation section
            if (SelectedCourses.IsNullOrEmpty())
            {
                TempData["response"] = errorMsg.Replace("##", "Please select course(s)");
                return View(_ARGPersonalInformation);
            }
            else
            {
                foreach (var item in SelectedCourses ?? new List<ARGCoursesApplied>())
                {
                    if (!_ARGPersonalInformation.CoursesApplied.Any(x => x.Course_ID == item.Course_ID))
                    {
                        TempData["response"] = errorMsg.Replace("##", "Invalid Course found. Please refresh and try again");
                        return View(_ARGPersonalInformation);
                    }
                }
            }
            if ((!this?.IsCaptchaValid("Captcha is not valid") ?? true))
            {
                TempData["response"] = errorMsg.Replace("##", "Invalid Verification Code, please try again.");
                return View(_ARGPersonalInformation);
            }

            if ((_ARGPersonalInformation.Batch != DateTime.UtcNow.Year - 1) && (_ARGPersonalInformation.Batch != DateTime.UtcNow.Year + 1) && _ARGPersonalInformation.Batch != DateTime.UtcNow.Year)
            {
                TempData["response"] = errorMsg.Replace("##", $"Invalid Batch, should be between {DateTime.UtcNow.Year - 1} - {DateTime.UtcNow.Year + 1}");
                return View(_ARGPersonalInformation);
            }

            if (!StdBoardRegistrationNo.IsBoardRegNoIsValid())
            {
                TempData["response"] = errorMsg.Replace("##", $"Board Regn No is not valid.");
                return View(_ARGPersonalInformation);
            }
            if ((_ARGPersonalInformation?.IsProvisional) ?? false)
            {
                int indexOf12th = _ARGPersonalInformation.AcademicDetails.FindIndex(x => x.ExamName.ToLower() == "12th");
                int indexOfGraduation = _ARGPersonalInformation.AcademicDetails.FindIndex(x => x.ExamName.ToLower() == "graduation");
                if (indexOfGraduation != -1)
                {
                    _ARGPersonalInformation.AcademicDetails.RemoveAt(indexOfGraduation);
                }
                else if (indexOf12th != -1)
                {
                    _ARGPersonalInformation.AcademicDetails.RemoveAt(indexOf12th);
                }
                ModelState.Clear();
                TryValidateModel(_ARGPersonalInformation);
            }
            if (!new RegistrationManager().ValidateDOB(_ARGPersonalInformation.EnteredDOB))
            {
                ModelState.AddModelError("EnteredDOB", "Invalid DOB");
            }
            #endregion

            #region Save section
            ModelState.Remove(nameof(_ARGPersonalInformation.BoardRegistrationNo));
            _ARGPersonalInformation.BoardRegistrationNo = StdBoardRegistrationNo;

            _ARGPersonalInformation.MarksCertificate = null;
            ModelState.Remove(nameof(_ARGPersonalInformation.MarksCertificate));
            _ARGPersonalInformation.CategoryCertificate = null;
            ModelState.Remove(nameof(_ARGPersonalInformation.CategoryCertificate));

            if (!ModelState.IsValid)
            {
                if (_ARGPersonalInformation != null)
                {
                    _ARGPersonalInformation.AcademicDetails = FillAcademicDetails(_ARGPersonalInformation.CoursesApplied[0].PrintProgramme).AcademicDetails;
                }
                return View(_ARGPersonalInformation);
            }
            else
            {
                //--------------- save here ------------------
                _ARGPersonalInformation.CoursesApplied = new List<ARGCoursesApplied>();
                _ARGPersonalInformation.CoursesApplied.AddRange(SelectedCourses);

                if (!string.IsNullOrWhiteSpace(_ARGPersonalInformation.PreviousUniversityRegnNo) && _ARGPersonalInformation.PreviousUniversityRegnNo.ToLower().Trim() == "null")
                    _ARGPersonalInformation.PreviousUniversityRegnNo = null;


                _ARGPersonalInformation.Preference = null;
                int _result = new BOPEERegistrationManager().Save(_ARGPersonalInformation, programme);
                SelectedCourses = null;//dispose
                if (_result > 0)
                {
                    TempData["response"] = errorMsg.Replace("##", $"Form Submitted Successfully. Please note down the Form No. : <strong style='font-size:18px;'>{_ARGPersonalInformation.StudentFormNo}</strong>&nbsp;&nbsp;&nbsp;&nbsp;<a href='/Registration/Detail/{_ARGPersonalInformation.Student_ID}?R={((int)programme).ToString().EncryptCookieAndURLSafe()}' target='_blank'>Print or Download Form</a>").Replace("alert-danger", "alert-success");
                    return RedirectToAction("AddStudentDetails");
                }
                else if (_result == -1)
                {
                    TempData["response"] = errorMsg.Replace("##", $"Board Registration No.: {_ARGPersonalInformation.BoardRegistrationNo} in Batch: {_ARGPersonalInformation.Batch} has already submitted the Form (Email or visit University for any queries).");
                    return RedirectToAction("AddStudentDetails");
                }
                else
                {
                    TempData["response"] = $"<div class='alert-msg error' style='background-color: #F3CBCC !important'>Unable to Submit Form. Please refresh & try again. <a class='close' href='#' title='close'>X</a></div>";
                    return RedirectToAction("AddStudentDetails");
                }
            }
            #endregion
        }

        [HttpPost]
        public JsonResult IsBoardRegnNoPresentInBatch(string StdBoardRegistrationNo, PrintProgramme programme, short Batch)
        {
            return Json(new BOPEERegistrationManager().CheckBoardRegNoExists(StdBoardRegistrationNo, programme, Batch), JsonRequestBehavior.DenyGet);
        }
    }
}