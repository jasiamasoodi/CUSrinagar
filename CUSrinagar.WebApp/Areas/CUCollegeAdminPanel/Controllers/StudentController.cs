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
    [OAuthorize(AppRoles.College_EditStudent)]
    public class StudentController : Controller
    {
        #region ViewBagsAndVariables
        private string errorHtml = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'></button> <a href='#' class='alert-link'>#msg</a></div>";

        [NonAction]
        private void SetViewBags(bool All = false)
        {
            ViewBag.PrintProgrammeOption = Helper.GetSelectList<PrintProgramme>().OrderBy(x => x.Value);
            if (All)
            {
                ViewBag.GenderDDL = Helper.GenderDDL();
                ViewBag.ReligionDDL = Helper.ReligionDDL();
                ViewBag.DistrictDDL = new GeneralDDLManager().GetDistrictList();
                ViewBag.AssemblyDDL = new GeneralDDLManager().GetAssemblyList();
                ViewBag.CategoryDDL = new CategoryManager().GetCategoryListALL();
                ViewBag.UniversitiesDDL = Helper.UniversitiesDDL();
                ViewBag.Boards = Helper.BoardsDDL();
                ViewBag.Provisional = Helper.ProvisionalDDL();
                ViewBag.Session = Helper.SessionDDL();
            }
        }
        #endregion

        #region SearchStudentToEdit
        [HttpGet]
        public ActionResult Index()
        {
            if (AppUserHelper.College_ID.IsNullOrEmpty())
            {
                TempData["response"] = errorHtml.Replace("#msg", "College not found.");
                return RedirectToAction("Index", "Dashboard", new { area = "CUCollegeAdminPanel" });
            }
            SetViewBags();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(ARGReprint SearchObj)
        {
            SetViewBags();
            if (!ModelState.IsValid)
                return View("Index", SearchObj);

            PrintProgramme printProgramme = SearchObj.PrintProgrammeOption;
            Guid ID = new AdminRegistrationManager().GetStudentIDByFormNo(SearchObj);
            if ((ID == null) || (ID == Guid.Empty))
            {
                TempData["response"] = errorHtml.Replace("#msg", $"No details found for {SearchObj.FormNo}");
                return RedirectToAction("Index");
            }
            else
                return RedirectToAction("Edit", "Student", new { id = ID + "/", R = ((int)printProgramme).ToString().EncryptCookieAndURLSafe(), area = "CUCollegeAdminPanel" });
        }
        #endregion

        #region UpdateStudents
        [HttpGet]
        public ActionResult Edit(string id, string R)
        {
            SetViewBags(true);
            if (!Guid.TryParse(id + "", out Guid result) || !Enum.TryParse((R + "").DecryptCookieAndURLSafe(), out PrintProgramme programme))
            {
                TempData["response"] = errorHtml.Replace("#msg", $"Invalid details");
                return RedirectToAction("Index");
            }
            if (!new AdminRegistrationManager().CheckStudentBelongsToCollege(result, programme))
            {
                TempData["response"] = errorHtml.Replace("#msg", $"Student does not belong to your college.");
                return RedirectToAction("Index");
            }
            if(new ApplicationFormsManager().DegreeAlreadyPrinted(result))
            {
                TempData["response"] = errorHtml.Replace("#msg", $"It seems that Student Degree Certificate has been printed, hence details cannot be edited.");
                return RedirectToAction("Index");
            }

            Guid Course_ID = new StudentManager().GetStudentCurrentCourse(programme, result);
            if (Course_ID == Guid.Parse("48887D19-F0C3-41CB-AC7A-22EA7B65494A"))
            {
                programme = PrintProgramme.BED;
            }

            RegistrationManager _RegistrationManager = new RegistrationManager();
            ARGPersonalInformation _PersonalInformation = _RegistrationManager.GetStudentPersonalInfoOnly(result, _RegistrationManager.MappingTable(programme), true);

            if (!(_PersonalInformation.IsProvisional ?? true))
                _PersonalInformation.AcademicDetails = new RegistrationDB().GetStudentAcademicDetails(result, _RegistrationManager.MappingTable(programme))?.OrderBy(x => x.Year).ToList();
            if (_PersonalInformation.AcademicDetails.IsNullOrEmpty() || (_PersonalInformation.IsProvisional ?? true))
            {
                if (programme == PrintProgramme.IH || programme == PrintProgramme.UG)
                {
                    _PersonalInformation.AcademicDetails = new List<ARGStudentPreviousQualifications>();
                    _PersonalInformation.AcademicDetails.Add(new ARGStudentPreviousQualifications
                    {
                        ExamName = "12TH",
                        ReadOnly = true
                    });
                }
                else if (programme == PrintProgramme.BED)
                {
                    _PersonalInformation.AcademicDetails = new List<ARGStudentPreviousQualifications>();
                    _PersonalInformation.AcademicDetails.Add(new ARGStudentPreviousQualifications
                    {
                        ExamName = "GRADUATION",
                        ReadOnly = true
                    });
                }
                else if (programme == PrintProgramme.PG)
                {
                    _PersonalInformation.AcademicDetails = new RegistrationDB().GetStudentAcademicDetails(result, _RegistrationManager.MappingTable(programme))?.OrderBy(x => x.Year).ToList();
                    if (_PersonalInformation.AcademicDetails.IsNotNullOrEmpty())
                    {
                        _PersonalInformation.AcademicDetails.Add(new ARGStudentPreviousQualifications
                        {
                            ExamName = "GRADUATION",
                            ReadOnly = true
                        });
                    }
                    else
                    {
                        _PersonalInformation.AcademicDetails = new List<ARGStudentPreviousQualifications>();
                        _PersonalInformation.AcademicDetails.AddRange(
                            new List<ARGStudentPreviousQualifications>{
                            new ARGStudentPreviousQualifications
                                {
                                    ExamName = "12TH",
                                    ReadOnly = true
                                },
                                new ARGStudentPreviousQualifications
                                {
                                    ExamName = "GRADUATION",
                                    ReadOnly = true
                                }
                        }
                        );
                    }
                }
            }

            _PersonalInformation.StudentAddress.AssemblyConstituency = _PersonalInformation.StudentAddress.AssemblyConstituency + "|" + _PersonalInformation.StudentAddress.ParliamentaryConstituency;
            _PersonalInformation.StudentAddress.District = _PersonalInformation.StudentAddress.District + "|" + _PersonalInformation.StudentAddress.State;
            _PersonalInformation.EnteredDOB = _PersonalInformation.DOB.ToString("dd-MM-yyyy");

            ViewBag.Programme = programme;
            ViewBag.AppliedForDesc = programme.GetEnumDescription();

            return View(_PersonalInformation);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ARGPersonalInformation StdPersonalInformation, PrintProgramme programme, string NewBoardRegistrationNo, Guid? RemoveQ_ID = null)
        {
            SetViewBags(true);
            ViewBag.Programme = programme;
            ViewBag.AppliedForDesc = programme.GetEnumDescription();


            if (StdPersonalInformation != null && (StdPersonalInformation.IsProvisional ?? false))
            {
                if (StdPersonalInformation.IsLateralEntry)
                {
                    StdPersonalInformation.AcademicDetails = null;
                }
                else
                {
                    int Index = StdPersonalInformation.AcademicDetails.FindIndex(x => x.Qualification_ID == Guid.Empty);
                    if (Index != -1)
                    {
                        StdPersonalInformation.AcademicDetails.RemoveAt(Index);
                    }
                }
                ModelState.Clear();
                TryValidateModel(StdPersonalInformation);
            }
            //remove model error for required photograph
            if (ModelState.ContainsKey(nameof(StdPersonalInformation.PhotographPath)))
                ModelState[nameof(StdPersonalInformation.PhotographPath)].Errors.Clear();
            if (ModelState.ContainsKey(nameof(StdPersonalInformation.BoardRegistrationNo)))
                ModelState[nameof(StdPersonalInformation.BoardRegistrationNo)].Errors.Clear();

            ModelState.Remove(nameof(StdPersonalInformation.CategoryCertificate));
            ModelState.Remove(nameof(StdPersonalInformation.MarksCertificate));

            if (!ModelState.IsValid)
            {
                TempData["response"] = errorHtml.Replace("#msg", $"Data Send is not valid. Please try again.");
                return RedirectToAction("Index");
            }
            if ((!this?.IsCaptchaValid("Captcha is not valid") ?? true))
            {
                TempData["response"] = errorHtml.Replace("#msg", $"Invalid Verification Code, please try again.");
                return RedirectToAction("Index");
            }
            StdPersonalInformation.BoardRegistrationNo = NewBoardRegistrationNo;
            if (new RegistrationManager().ConvertEnterdDOBToDateTime(StdPersonalInformation.EnteredDOB) == DateTime.MinValue)
            {
                TempData["response"] = errorHtml.Replace("#msg", $"Invalid DOB {StdPersonalInformation.EnteredDOB}");
                return RedirectToAction("Index");
            }

            int Result = new AdminRegistrationManager().EditStudentDetails(StdPersonalInformation, programme, RemoveQ_ID);
            if (Result == -1)
            {
                TempData["response"] = errorHtml.Replace("#msg", $"Board Registration No. not provided OR already exists in Batch {StdPersonalInformation.Batch}");
                return RedirectToAction("Index");
            }
            else if (Result > 0)
            {
                TempData["response"] = errorHtml.Replace("#msg", $"Form Number / CUS Registration No.  {StdPersonalInformation.StudentFormNo} Updated Successfully").Replace("alert-danger", "alert-success");
                return RedirectToAction("Index");
            }
            TempData["response"] = errorHtml.Replace("#msg", $"Something went wrong. Please try again.");
            return RedirectToAction("Index");
        }
        #endregion

        #region SemesterAdmission

        [HttpGet]
        public ActionResult SemesterAdmForm(short? AdmSemester, PrintProgramme? printProgramme, Guid? Student_ID)
        {
            if (AdmSemester == null || (AdmSemester ?? 0) <= 0 || printProgramme == null || Student_ID == null)
            {
                TempData["response"] = errorHtml.Replace("#msg", $"Invalid details.");
                return RedirectToAction("ViewFormDetail", "AssignCombination");
            }

            PaymentDetails _PrevPaymentDetails = new PaymentManager().GetPaymentDetail((Guid)Student_ID, (short)AdmSemester, printProgramme);

            if (_PrevPaymentDetails == null)
            {
                TempData["response"] = errorHtml.Replace("#msg", $"Please Sumbit University Semester Admission Fee before Printing/downloading the Form.");
                return RedirectToAction("ViewFormDetail", "AssignCombination");
            }
            Tuple<string, string, List<ADMSubjectMaster>, ARGPersonalInformation, short, string, CombinationSetting> SemesterAdmFormDetails =
                                             new StudentManager().GetSemesterAdmissionDetails((Guid)Student_ID, (PrintProgramme)printProgramme, (short)AdmSemester);

            if (SemesterAdmFormDetails.Item4.Student_ID == Guid.Empty && !string.IsNullOrWhiteSpace(SemesterAdmFormDetails.Item1))
            {
                TempData["response"] = errorHtml.Replace("#msg", SemesterAdmFormDetails.Item1);
                return RedirectToAction("ViewFormDetail", "AssignCombination");
            }
            else if (SemesterAdmFormDetails.Item4.Student_ID == Guid.Empty)
            {
                TempData["response"] = errorHtml.Replace("#msg", $"Previous semester detials not found");
                return RedirectToAction("ViewFormDetail", "AssignCombination");
            }
            SemesterAdmFormDetails.Item4.PaymentDetail = new List<PaymentDetails> { _PrevPaymentDetails };


            List<StudentAdditionalSubject> studentAddionalSubjects = new StudentManager().GetStudentAdditionalSubjects(SemesterAdmFormDetails.Item4.Student_ID, (short)AdmSemester, (PrintProgramme)printProgramme);
            ViewBag.StudentAddionalSubjects = studentAddionalSubjects;

            ViewBag.AdmSemester = AdmSemester;
            ViewBag.SemesterAdmFormDetails = SemesterAdmFormDetails;
            return View();
        }
        #endregion
    }
}