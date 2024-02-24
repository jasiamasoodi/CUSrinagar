using CaptchaMvc.HtmlHelpers;
using CUSrinagar.BusinessManagers;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using CUSrinagar.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.Mvc;

namespace CUSrinagar.WebApp.Controllers
{
    public class NEPIGController : Controller
    {
        string errorMsg = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>##</a></div>";
        List<CertificateType> CertificatesToBeUploaded { get { return new List<CertificateType>() { CertificateType.MarksCard, CertificateType.Provisional, CertificateType.Category }; } }
        short MaximumNumber_Of_Course_DDL_Options { get { return 2; } }//5

        [HttpGet]
        public ActionResult Index()
        {
            return RedirectToAction("Index", "NEP", new { area = "" });
        }

        #region PersonalInfo

        [HttpGet]
        public ActionResult PersonalInfo(string r)
        {
            ARGPersonalInformation model = null;
            Guid _Student_ID = Guid.Empty;

            if (!r.IsNullOrEmpty() && Guid.TryParse(r.DecryptCookieAndURLSafe(), out _Student_ID))
                model = new RegistrationManager().GetStudentPersonalInfoOnly(Guid.Parse(r.DecryptCookieAndURLSafe()), PrintProgramme.IH, true);
            if ((r + "").Length > 0 && _Student_ID == Guid.Empty)
            {
                return RedirectToAction("Index", "NEP", new { area = "" });
            }

            if (_Student_ID != Guid.Empty)
            {
                if (new NEPManager().HasCookiePresent(PrintProgramme.IH, _Student_ID.ToString().EncryptCookieAndURLSafe()) == false)
                {
                    return RedirectToAction("Index", "NEP", new { area = "" });
                }
            }

            model = FillPersonalInfo_ViewBags(model) ?? new ARGPersonalInformation();

            var RegistrationProcessClosed = new NEPManager().GetCourseListForRegistrationByGroup(PrintProgramme.IH, _Student_ID).IsNullOrEmpty();
            if (RegistrationProcessClosed)
            {
                var result = new NEPManager().GetNEPPersonalInfo(model.Student_ID, PrintProgramme.IH);
                if (result.Item1)
                    return RedirectToAction("Print", new { r = model.Student_ID.ToString().EncryptCookieAndURLSafe() });
                else
                {
                    TempData["ErrorMessage"] = errorMsg.Replace("##", result.Item2);
                    return RedirectToAction("Index", "NEP", new { area = "" });
                }
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PersonalInfo(ARGPersonalInformation model)
        {
            #region Validation
            model.MarksCertificate = null;
            ModelState.Remove(nameof(model.MarksCertificate));
            model.CategoryCertificate = null;
            ModelState.Remove(nameof(model.CategoryCertificate));
            ModelState.Remove(nameof(model.IsProvisional));
            ModelState.Remove(nameof(model.PreviousUniversityRegnNo));
            ModelState.Remove(nameof(model.PhotographPath));
            if (!new RegistrationManager().ValidateDOB(model.EnteredDOB))
                ModelState.AddModelError("EnteredDOB", "Invalid DOB");
            #endregion

            if ((!this?.IsCaptchaValid("Captcha is not valid") ?? true))
            {
                TempData["ErrorMessage"] = errorMsg.Replace("##", "Invalid Verification Code, please try again.");
            }
            else
            {

                if (ModelState.IsValid)
                {
                    Tuple<bool, string> _result = new NEPManager().Save(model, PrintProgramme.IH);
                    if (_result.Item1)
                    {
                        new NEPManager().CreateCookie(PrintProgramme.IH, model.Student_ID.ToString().EncryptCookieAndURLSafe());
                        TempData["SuccessMessage"] = "Details saved successfully.";
                        return RedirectToAction("PreviousQualification", new { r = model.Student_ID.ToString().EncryptCookieAndURLSafe() });
                    }
                    else
                    {
                        TempData["ErrorMessage"] = errorMsg.Replace("##", _result.Item2);
                    }
                }
                else
                {
                    var ErrorMessage = string.Join(", ", ModelState.Values.SelectMany(s => s.Errors).Select(y => y.ErrorMessage).ToArray());
                    TempData["ErrorMessage"] = errorMsg.Replace("##", ErrorMessage);
                }
            }
            TempData["model"] = model;
            return RedirectToAction("PersonalInfo", new { r = model.Student_ID.ToString().EncryptCookieAndURLSafe() });
        }
        #endregion

        #region AccademicDetail
        [HttpGet]
        public ActionResult PreviousQualification(string r)
        {
            List<PrintProgramme> CoursesRegOpen = new CourseManager().GetOpenResistrationProgrammeCategories();
            ViewBag.Provisional = Helper.ProvisionalDDL();
            if (CoursesRegOpen.IsNullOrEmpty())
            {
                TempData["ErrorMessage"] = errorMsg.Replace("##", "Invalid request. Please try again later.");
                return RedirectToAction("Index", "NEP", new { area = "" });
            }

            if (r.IsNullOrEmpty()
                || !Guid.TryParse(r.DecryptCookieAndURLSafe(), out Guid _Student_ID)
                || _Student_ID == Guid.Empty)
            {
                TempData["ErrorMessage"] = errorMsg.Replace("##", "Invalid request. Please try again.");
                return RedirectToAction("Index", "NEP", new { area = "" });
            }
            if (new NEPManager().HasCookiePresent(PrintProgramme.IH, _Student_ID.ToString().EncryptCookieAndURLSafe()) == false)
            {
                return RedirectToAction("Index", "NEP", new { area = "" });
            }

            ARGStudentPreviousQualifications previousQualification;
            try
            {
                previousQualification = (ARGStudentPreviousQualifications)TempData["model"] ??
                         new RegistrationManager().GetStudentAcademicDetails(_Student_ID, PrintProgramme.IH)?.FirstOrDefault();
            }
            catch (Exception)
            {
                previousQualification = new RegistrationManager().GetStudentAcademicDetails(_Student_ID, PrintProgramme.IH)?.FirstOrDefault();
            }
            ARGStudentPreviousQualifications model = FillPreviousQualification_ViewBags(_Student_ID, previousQualification);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PreviousQualification(ARGStudentPreviousQualifications model, string IsProv)
        {
            try
            {
                if (IsProv?.DecryptCookieAndURLSafe()?.ToLower() == "provisionaladmallowed" && model.IsProvisional == true)
                {
                    new NEPManager().DeletePreviousQualifications(model.Student_ID, PrintProgramme.IH);
                    return RedirectToAction("SubjectPreference", new { r = model.Student_ID.ToString().EncryptCookieAndURLSafe() });
                }
            }
            catch (Exception)
            {
                return RedirectToAction("PreviousQualification", new { r = model.Student_ID.ToString().EncryptCookieAndURLSafe() });
            }

            if (ModelState.IsValid)
            {
                model.IsProvisional = false;
                Tuple<bool, string> _result = new NEPManager().Save(model, PrintProgramme.IH);
                if (_result.Item1)
                {
                    TempData["SuccessMessage"] = "Details saved successfully.";
                    return RedirectToAction("SubjectPreference", new { r = model.Student_ID.ToString().EncryptCookieAndURLSafe() });
                }
                else
                {
                    TempData["ErrorMessage"] = errorMsg.Replace("##", _result.Item2);
                }
            }
            else
            {
                var ErrorMessage = string.Join(", ", ModelState.Values.SelectMany(s => s.Errors).Select(y => y.ErrorMessage).ToArray());
                TempData["ErrorMessage"] = errorMsg.Replace("##", ErrorMessage);
            }
            TempData["model"] = model;
            return RedirectToAction("PreviousQualification", new { r = model.Student_ID.ToString().EncryptCookieAndURLSafe() });
        }
        #endregion

        #region SubjectDetails
        [HttpGet]
        public ActionResult SubjectPreference(string r)
        {
            List<PrintProgramme> CoursesRegOpen = new CourseManager().GetOpenResistrationProgrammeCategories();

            if (CoursesRegOpen.IsNullOrEmpty())
            {
                TempData["ErrorMessage"] = errorMsg.Replace("##", "Invalid request. Please try again.");
                return RedirectToAction("Index", "NEP", new { area = "" });
            }

            if (r.IsNullOrEmpty() || !Guid.TryParse(r.DecryptCookieAndURLSafe(), out Guid _Student_ID)
                || _Student_ID == Guid.Empty)
            {
                TempData["ErrorMessage"] = errorMsg.Replace("##", "Invalid request. Please try again.");
                return RedirectToAction("Index", "NEP", new { area = "" });
            }

            if (new NEPManager().HasCookiePresent(PrintProgramme.IH, _Student_ID.ToString().EncryptCookieAndURLSafe()) == false)
            {
                return RedirectToAction("Index", "NEP", new { area = "" });
            }

            ARGStudentPreviousQualifications previousQualification = new RegistrationManager().GetStudentAcademicDetails(_Student_ID, PrintProgramme.IH)?.FirstOrDefault();

            //if (previousQualification == null)
            //{
            //    TempData["ErrorMessage"] = errorMsg.Replace("##", "Fill qualification details first then try next step.");
            //    return RedirectToAction("SubjectPreference", new { r = _Student_ID.ToString().EncryptCookieAndURLSafe() });
            //}

            List<ARGCoursesApplied> listOfCoursesOpted = (List<ARGCoursesApplied>)TempData["model"] ?? new RegistrationManager().GetStudentCourses(_Student_ID, PrintProgramme.IH);
            ViewBag.HasDBCourse = listOfCoursesOpted != null;
            List<ARGCoursesApplied> model = FillSubjectPreference_ViewBags(_Student_ID, listOfCoursesOpted, previousQualification);
            return View("SubjectPreference", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubjectPreference(List<ARGCoursesApplied> model)
        {
            model = model.Where(x => x.Course_ID != Guid.Empty).ToList();
            if (!model.IsNullOrEmpty())
            {
                model = model.GroupBy(x => x.Course_ID).Select(x => x.First()).ToList();
                ModelState.Clear();
                short _preference = 1;
                model.OrderBy(x => x.Preference).ToList().ForEach(x => { x.Preference = _preference++; });

                if (ModelState.IsValid)
                {
                    Tuple<bool, string> _result = new NEPManager().Save(model, PrintProgramme.IH);
                    if (_result.Item1)
                    {
                        TempData["SuccessMessage"] = "Details saved successfully.";
                        return RedirectToAction("DocumentUpload", new { r = model.First().Student_ID.ToString().EncryptCookieAndURLSafe() });
                    }
                    else
                    {
                        TempData["ErrorMessage"] = errorMsg.Replace("##", _result.Item2);
                    }
                }
                else
                {
                    var ErrorMessage = string.Join(", ", ModelState.Values.SelectMany(s => s.Errors).Select(y => y.ErrorMessage).ToArray());
                    TempData["ErrorMessage"] = errorMsg.Replace("##", ErrorMessage);
                }
            }
            else
            {
                TempData["ErrorMessage"] = errorMsg.Replace("##", "Please choose atleast one option");
            }

            TempData["model"] = model;
            return RedirectToAction("DocumentUpload", new { r = model.First().Student_ID.ToString().EncryptCookieAndURLSafe() });
        }
        #endregion

        #region DocumentUpload
        [HttpGet]
        public ActionResult DocumentUpload(string r)
        {
            List<PrintProgramme> CoursesRegOpen = new CourseManager().GetOpenResistrationProgrammeCategories();

            if (CoursesRegOpen.IsNullOrEmpty())
            {
                TempData["ErrorMessage"] = errorMsg.Replace("##", "Invalid request. Please try again.");
                return RedirectToAction("Index", "NEP", new { area = "" });
            }

            if (r.IsNullOrEmpty() || !Guid.TryParse(r.DecryptCookieAndURLSafe(), out Guid _Student_ID)
                || _Student_ID == Guid.Empty)
            {
                TempData["ErrorMessage"] = errorMsg.Replace("##", "Invalid request. Please try again.");
                return RedirectToAction("Index", "NEP", new { area = "" });
            }

            if (new NEPManager().HasCookiePresent(PrintProgramme.IH, _Student_ID.ToString().EncryptCookieAndURLSafe()) == false)
            {
                return RedirectToAction("Index", "NEP", new { area = "" });
            }

            NEPDocuments nepDocuments = new NEPManager().GetDocuments(_Student_ID, PrintProgramme.IH);
            ViewBag.HasDocuments = nepDocuments != null;
            NEPDocuments model = FillDocumentPreference_ViewBags(_Student_ID, nepDocuments, PrintProgramme.IH);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DocumentUpload(NEPDocuments model)
        {
            if (model != null)
            {
                if (model.Documents.IsNotNullOrEmpty())
                {
                    if (model.Documents.Any(x => x.File == null && string.IsNullOrWhiteSpace(x.CertificateUrl)))
                    {
                        TempData["ErrorMessage"] = errorMsg.Replace("##", $"{string.Join("<br/>", model.Documents.Where(x => x.File == null && string.IsNullOrWhiteSpace(x.CertificateUrl)).Select(x => x.CertificateType.ToString()))}" +
                            $" are missing. Please check and try again");
                        return RedirectToAction("DocumentUpload", new { r = model.Student_ID.ToString().EncryptCookieAndURLSafe() });
                    }
                }
            }
            if (model != null)
            {
                if (model.Documents.IsNotNullOrEmpty())
                {
                    List<Certificate> certificates = model.Documents.Where(x => x.File != null)?.ToList();
                    model.Documents = certificates;
                    ModelState.Clear();
                    TryValidateModel(model);
                }
            }

            if (ModelState.IsValid)
            {
                Tuple<bool, string> _result = new NEPManager().Save(model, PrintProgramme.IH);
                if (_result.Item1)
                {
                    TempData["SuccessMessage"] = "Details saved successfully.";
                    return RedirectToAction("MakePayment", new { r = model.Student_ID.ToString().EncryptCookieAndURLSafe() });
                }
                else
                {
                    TempData["ErrorMessage"] = errorMsg.Replace("##", _result.Item2);
                }
            }
            else
            {
                var ErrorMessage = string.Join(", ", ModelState.Values.SelectMany(s => s.Errors).Select(y => y.ErrorMessage).ToArray());
                TempData["ErrorMessage"] = errorMsg.Replace("##", ErrorMessage);
            }
            return RedirectToAction("DocumentUpload", new { r = model.Student_ID.ToString().EncryptCookieAndURLSafe() });
        }
        #endregion

        #region MakePayment
        [HttpGet]
        public ActionResult MakePayment(string r)
        {
            List<PrintProgramme> CoursesRegOpen = new CourseManager().GetOpenResistrationProgrammeCategories();

            if (CoursesRegOpen.IsNullOrEmpty())
            {
                TempData["ErrorMessage"] = errorMsg.Replace("##", "Invalid request. Please try again.");
                return RedirectToAction("Index", "NEP", new { area = "" });
            }

            if (r.IsNullOrEmpty() || !Guid.TryParse(r.DecryptCookieAndURLSafe(), out Guid _Student_ID)
                || _Student_ID == Guid.Empty)
            {
                TempData["ErrorMessage"] = errorMsg.Replace("##", "Invalid request. Please try again.");
                return RedirectToAction("Index", "NEP", new { area = "" });
            }

            if (new NEPManager().HasCookiePresent(PrintProgramme.IH, _Student_ID.ToString().EncryptCookieAndURLSafe()) == false)
            {
                return RedirectToAction("Index", "NEP", new { area = "" });
            }

            string isEligileForPayment = new NEPManager().IsEligibleForPayment(_Student_ID, PrintProgramme.IH);

            if (!string.IsNullOrWhiteSpace(isEligileForPayment))
            {
                TempData["ErrorMessage"] = errorMsg.Replace("##", isEligileForPayment);
                return RedirectToAction("Index", "NEP", new { area = "" });
            }


            ViewBag.PersonalInfoCompact = new StudentManager().GetStudentC(_Student_ID, PrintProgramme.IH);

            PaymentDetails payment = new PaymentManager().GetPaymentDetails(_Student_ID, PaymentModuleType.Admission, PrintProgramme.IH)?.FirstOrDefault();
            ViewBag.HasDBPayment = payment != null;
            PaymentDetails model = FillPayment_ViewBags(_Student_ID, payment);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MakePayment(PaymentDetails model)
        {
            if (ModelState.IsValid)
            {
                CreateBillDeskRequest(model);
            }
            else
            {
                var ErrorMessage = string.Join(", ", ModelState.Values.SelectMany(s => s.Errors).Select(y => y.ErrorMessage).ToArray());
                TempData["ErrorMessage"] = errorMsg.Replace("##", ErrorMessage);
            }
            return RedirectToAction("MakePayment", new { r = model.Student_ID.ToString().EncryptCookieAndURLSafe() });
        }

        public ActionResult PaymentResponse()
        {
            Tuple<bool, string, PaymentDetails, Guid, Guid> billdeskResponse = new BillDeskManager().BillDeskResponse(Request.InputStream);

            if (billdeskResponse.Item1)
            {
                Guid Student_ID = billdeskResponse.Item3.Entity_ID;
                var result = new NEPManager().Save(billdeskResponse.Item3, PrintProgramme.IH);
                if (result.Item1)
                    return RedirectToAction("Print", new { r = Student_ID.ToString().EncryptCookieAndURLSafe() });
                else
                {
                    TempData["ErrorMessage"] = "Transaction Failed. What you can do <br/><br/>1. In-case Amount is not deducted try to enable E-COM in your card using your bank App or ATM like M-Pay, or try to use other methods of online banking. <br/>"
                        + "<br/>2. In-case amount is deducted contact Cluster University I.T Cell.<br/>";

                    return RedirectToAction("MakePayment", "NEP", new { r = Student_ID.ToString().EncryptCookieAndURLSafe() });
                }
            }
            else
            {

                string msg = "Transaction Failed. What you can do <br/><br/>1. In-case Amount is not deducted try to enable E-COM in your card using your bank App or ATM like M-Pay, or try to use other methods of online banking. <br/>"
                + "<br/>2. In-case amount is deducted contact Cluster University I.T Cell.<br/>";

                TempData["ErrorMessage"] = errorMsg.Replace("##", msg);

                return RedirectToAction("Index", "NEP");
            }
        }

        #endregion

        #region Print
        [HttpGet]
        public async Task<ActionResult> Print(string r)
        {
            if (r.IsNullOrEmpty() || !Guid.TryParse(r.DecryptCookieAndURLSafe(), out Guid _Student_ID)
                || _Student_ID == Guid.Empty)
            {
                TempData["ErrorMessage"] = errorMsg.Replace("##", "Invalid request. Please try again.");
                return RedirectToAction("Index", "NEP", new { area = "" });
            }

            #region Block Edit
            bool blockEdit = new NEPManager().BlockEditAfterPayment(_Student_ID, PrintProgramme.IH);
            if (!blockEdit)
            {
                if (new NEPManager().HasCookiePresent(PrintProgramme.IH, _Student_ID.ToString().EncryptCookieAndURLSafe()) == false
                    && !(Request?.UrlReferrer + "").ToLower().Contains("custudentzone"))
                {
                    return RedirectToAction("Index", "NEP", new { area = "" });
                }
            }
            #endregion
            var result = await new NEPManager().GetNEPPersonalInfoAsync(_Student_ID, PrintProgramme.IH);

            if (!result.Item1)
            {
                TempData["ErrorMessage"] = errorMsg.Replace("##", result.Item2);
                return RedirectToAction("Index", "NEP", new { area = "" });
            }
            else if (result.Item3.PersonalInformation.IsLateralEntry && result.Item3.PersonalInformation.CoursesApplied.All(x => x.CourseName.ToLower().Contains("engineering")))
            {

                return RedirectToAction("Detail", "Registration", new { id = result.Item3.PersonalInformation.Student_ID, R = ((short)PrintProgramme.IH).ToString().EncryptCookieAndURLSafe(), area = "" });
            }

            ViewBag.courseApplied = NEPProgrammesAdmTitle.Year_5_Integrated_PG_Programmes.GetEnumDescription();
            return View(result.Item3);
        }
        #endregion

        #region Private-Methods-ViewBags
        private ARGPersonalInformation FillPersonalInfo_ViewBags(ARGPersonalInformation personalInfo)
        {
            if (personalInfo == null)
            {

#if (DEBUG)
                personalInfo = new ARGPersonalInformation()
                {
                    FullName = "Sajid Rasool",
                    FathersName = "G R Bhat",
                    BoardRegistrationNo = "JKB-" + DateTime.Now.Ticks.ToString(),
                    MothersName = "Suriya Jabeen",
                    Gender = "MALE",
                    Religion = "Islam",
                    Category = "OM",
                    EnteredDOB = "22-05-1990",
                    StudentAddress = new ARGStudentAddress() { PermanentAddress = "Akhoon Mohalla Brane", PinCode = "191121", Mobile = "7006" + DateTime.Now.Ticks.ToString().Substring(12, 6), Email = DateTime.Now.Ticks.ToString() + "@cusrinagar.edu.in", Block = "Ward-2", Tehsil = "Khanyar", District = "Srinagar|Jammu & Kashmir", AssemblyConstituency = "Sonawar|SRINAGAR" }
                };
#endif
            }
            else
            {
                personalInfo.EnteredDOB = personalInfo.DOB != DateTime.MinValue ? personalInfo.DOB.ToString("dd-MM-yyyy") : "";
            }

            if (personalInfo?.StudentAddress != null)
            {
                if (personalInfo.StudentAddress.Address_ID != Guid.Empty)
                {
                    personalInfo.StudentAddress.District = personalInfo.StudentAddress.District + "|" + personalInfo.StudentAddress.State;
                    personalInfo.StudentAddress.AssemblyConstituency = personalInfo.StudentAddress.AssemblyConstituency + "|" + personalInfo.StudentAddress.ParliamentaryConstituency;
                }
            }

            ViewBag.GenderDDL = Helper.GenderDDL();
            ViewBag.ReligionDDL = Helper.ReligionDDL();
            ViewBag.CategoryDDL = new CategoryManager().GetCategoryList();
            ViewBag.AssemblyDDL = new GeneralDDLManager().GetAssemblyList();
            ViewBag.DistrictDDL = new GeneralDDLManager().GetDistrictList();
            ViewBag.isSelfFinanceForm = new RegistrationManager().GetFormNoMaster(PrintProgramme.IH).AllowApplyForSelfFinancedSeat;

            return personalInfo ?? new ARGPersonalInformation { StudentAddress = new ARGStudentAddress() };
        }
        private ARGStudentPreviousQualifications FillPreviousQualification_ViewBags(Guid _Student_ID, ARGStudentPreviousQualifications previousQualification)
        {
            if (previousQualification == null)
            {
                previousQualification = new ARGStudentPreviousQualifications()
                {
                    Student_ID = _Student_ID,
                    ExamName = "12TH",
                    ReadOnly = true
                };
            }

            ViewBag.I2thStreamDDL = Helper.I2thStreamDDL();
            ViewBag.Sessions = Helper.SessionDDL();
            ViewBag.Boards = Helper.BoardsDDL();
            ViewBag.ErrorMessage = TempData["ErrorMessage"];
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            return previousQualification;
        }
        private List<ARGCoursesApplied> FillSubjectPreference_ViewBags(Guid _Student_ID, List<ARGCoursesApplied> listOfCoursesOpted, ARGStudentPreviousQualifications previousQualification)
        {
            var model = new List<ARGCoursesApplied>();
            if (listOfCoursesOpted.IsNullOrEmpty())
            {
                for (short index = 1; index <= MaximumNumber_Of_Course_DDL_Options; index++)
                    model.Add(new ARGCoursesApplied() { Student_ID = _Student_ID, Preference = index, IsActive = true, PrintProgramme = PrintProgramme.IH, Programme = Programme.IG });
            }
            else
            {
                model = listOfCoursesOpted;
                var number_Of_DDL_Options = MaximumNumber_Of_Course_DDL_Options - listOfCoursesOpted.Count;
                if (number_Of_DDL_Options > 0)
                {
                    var maxPrefenceOrder = listOfCoursesOpted.Max(x => x.Preference);
                    for (int index = 1; index <= number_Of_DDL_Options; index++)
                        model.Add(new ARGCoursesApplied() { Student_ID = _Student_ID, Preference = (short)(maxPrefenceOrder + index), IsActive = true, PrintProgramme = PrintProgramme.IH, Programme = Programme.IG });
                }
                model = model.OrderBy(x => x.Preference)?.ToList();
            }

            List<DropDownOptLabelGeneral> CoursesDDL = new NEPManager().GetCourseListForRegistrationByGroup(PrintProgramme.IH, _Student_ID);

            #region remove computer and IT if not math
            bool removeMathEligigleCourses = false;

            if (previousQualification != null)
            {
                removeMathEligigleCourses =
                      previousQualification.Stream.ToUpper().Contains("MEDICAL WITH MATH")
                   || previousQualification.Stream.ToUpper().Contains("ARTS WITH MATH")
                   || previousQualification.Stream.ToUpper().Contains("NON-MEDICAL")
                   || previousQualification.Stream.ToUpper().Contains("MATH");
                if (!removeMathEligigleCourses && CoursesDDL.IsNotNullOrEmpty())
                {
                    //remove math eligible courses
                    int indexOfComputerApp = CoursesDDL.FindIndex(x => Guid.Parse(x.Value.ToString()) == Guid.Parse("32996CE4-5613-4A2D-A565-030390D7F496"));/*IMCA*/
                    if (indexOfComputerApp != -1)
                        CoursesDDL.RemoveAt(indexOfComputerApp);

                    int indexOfIT = CoursesDDL.FindIndex(x => Guid.Parse(x.Value.ToString()) == Guid.Parse("3DF5801E-5707-41CD-B059-86AF8071F3CD")); /*IG-IT*/
                    if (indexOfIT != -1)
                        CoursesDDL.RemoveAt(indexOfIT);
                }
            }
            #endregion

            ViewBag.Courses = CoursesDDL;
            ViewBag.ErrorMessage = TempData["ErrorMessage"];
            ViewBag.SuccessMessage = TempData["SuccessMessage"];

            return model;
        }
        private List<NEPCollegePreference> FillCollegePreference_ViewBags(Guid _Student_ID, List<NEPCollegePreference> listOfCollegesOpted)
        {
            List<ARGCoursesApplied> listOfCoursesOpted = new RegistrationManager().GetStudentCourses(_Student_ID, PrintProgramme.IH);
            if (listOfCoursesOpted.IsNullOrEmpty())
            {
                ViewBag.ErrorMessage = "No course/main subject opted";
                return null;
            }

            List<SelectListItem> listOfCollegePreferences = new CollegeManager().GetCollegePreference(listOfCoursesOpted.Select(x => x.Course_ID).ToList());
            ARGPersonalInformation stdInfo = new RegistrationManager().GetStudentPersonalInfoOnly(_Student_ID, PrintProgramme.IH);
            if (stdInfo.Gender.Trim().ToLower() == "male" && listOfCollegePreferences.Any(x => x.Text.ToLower().Trim().Contains("women")))
            {
                listOfCollegePreferences = listOfCollegePreferences.Where(x => !x.Text.ToLower().Contains("women"))?.ToList();
            }
            if (listOfCollegePreferences.IsNullOrEmpty())
            {
                ViewBag.ErrorMessage = "No college option available.";
                return null;
            }
            int maxNumber_Of_DDL_Options = listOfCollegePreferences.Count();
            var model = new List<NEPCollegePreference>();
            if (listOfCollegesOpted.IsNullOrEmpty())
            {
                for (short index = 1; index <= maxNumber_Of_DDL_Options; index++)
                    model.Add(new NEPCollegePreference() { Student_ID = _Student_ID, PreferenceNo = index });
            }
            else
            {
                model = listOfCollegesOpted;
                var number_Of_DDL_Options = maxNumber_Of_DDL_Options - listOfCollegesOpted.Count;
                if (number_Of_DDL_Options > 0)
                {
                    short maxPrefenceOrder = listOfCollegesOpted.Max(x => x.PreferenceNo);
                    for (short index = 1; index <= number_Of_DDL_Options; index++)
                        model.Add(new NEPCollegePreference() { Student_ID = _Student_ID, PreferenceNo = (short)(maxPrefenceOrder + index) });
                }
                model = model.OrderBy(x => x.PreferenceNo).ToList();
            }

            ViewBag.Colleges = listOfCollegePreferences;
            ViewBag.ErrorMessage = TempData["ErrorMessage"];
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            return model;
        }
        private NEPDocuments FillDocumentPreference_ViewBags(Guid _Student_ID, NEPDocuments nepDocuments, PrintProgramme printProgramme)
        {
            NEPDocuments model = null;
            bool hasCategory = new NEPManager().HasCategory(_Student_ID, printProgramme);
            ViewBag.HasCategory = hasCategory;

            bool ISProvisional = new NEPManager().IsProvisionalAdm(_Student_ID, printProgramme);
            if (nepDocuments == null || nepDocuments.PhotographUrl == null)
            {
                model = new NEPDocuments() { Student_ID = _Student_ID, Documents = new List<Certificate>() };
                CertificatesToBeUploaded.ForEach(cert =>
                {
                    if (cert == CertificateType.Category && !hasCategory)
                    {
                    }
                    else
                    {
                        if (ISProvisional && cert != CertificateType.Category)
                        {

                        }
                        else
                        {
                            model.Documents.Add(new Certificate() { Student_ID = _Student_ID, CertificateType = cert, VerificationStatus = VerificationStatus.Pending, UploadingDate = DateTime.Now });
                        }
                    }
                });
            }
            else
            {
                model = nepDocuments;
                foreach (CertificateType cert in CertificatesToBeUploaded)
                {
                    if (nepDocuments.Documents != null && !nepDocuments.Documents.Any(y => y.CertificateType == cert))
                    {
                        if (cert == CertificateType.Category && !hasCategory)
                        {
                            string oldPath = nepDocuments.Documents
                            .FirstOrDefault(x => x.CertificateType == cert)?.CertificateUrl ?? "/ho.ss";

                            if (System.IO.File.Exists(HostingEnvironment.MapPath("~" + oldPath)))
                            {
                                System.IO.File.Delete(HostingEnvironment.MapPath("~" + oldPath));
                            }
                            new NEPManager().DeleteCertificate(_Student_ID, CertificateType.Category);
                        }
                        else
                        {
                            if (ISProvisional && cert != CertificateType.Category)
                            {

                            }
                            else
                            {
                                model.Documents.Add(new Certificate() { Student_ID = _Student_ID, CertificateType = cert, VerificationStatus = VerificationStatus.Pending, UploadingDate = DateTime.Now });
                            }
                        }
                    }
                    else if (nepDocuments.Documents != null && cert == CertificateType.Category && !hasCategory)
                    {
                        string oldPath = nepDocuments.Documents
                        .FirstOrDefault(x => x.CertificateType == cert)?.CertificateUrl ?? "/ho.ss";

                        if (System.IO.File.Exists(HostingEnvironment.MapPath("~" + oldPath)))
                        {
                            System.IO.File.Delete(HostingEnvironment.MapPath("~" + oldPath));
                        }
                        new NEPManager().DeleteCertificate(_Student_ID, CertificateType.Category);
                        nepDocuments.Documents = nepDocuments.Documents.Where(x => x.CertificateType != CertificateType.Category)?.ToList();
                    }
                }
            }
            if (nepDocuments.Documents == null)
            {
                if (nepDocuments == null)
                {
                    model = new NEPDocuments() { Student_ID = _Student_ID, Documents = new List<Certificate>() };
                }
                else
                {
                    model.Documents = new List<Certificate>();
                    model.Student_ID = _Student_ID;
                }
                CertificatesToBeUploaded.ForEach(cert =>
                {
                    if (cert == CertificateType.Category && !hasCategory)
                    {
                    }
                    else
                    {
                        if (ISProvisional && cert != CertificateType.Category)
                        {

                        }
                        else
                        {
                            model.Documents.Add(new Certificate() { Student_ID = _Student_ID, CertificateType = cert, VerificationStatus = VerificationStatus.Pending, UploadingDate = DateTime.Now });
                        }
                    }
                });
            }
            ViewBag.ErrorMessage = TempData["ErrorMessage"];
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            return model;
        }
        private PaymentDetails FillPayment_ViewBags(Guid _Student_ID, PaymentDetails payment)
        {
            ARGPersonalInformation stdInfo = new RegistrationManager().GetStudentPersonalInfoOnly(_Student_ID, PrintProgramme.IH, true);
            ARGFormNoMaster formNoMaster = new RegistrationManager().GetFormNoMaster(PrintProgramme.IH);
            ViewBag.isSelfFinanceForm = formNoMaster?.AllowApplyForSelfFinancedSeat ?? false;
            PaymentDetails model = payment ?? new PaymentDetails()
            {
                TxnAmount = formNoMaster.BasicFee,
                Entity_ID = _Student_ID,
                Student_ID = _Student_ID,
                Email = stdInfo.StudentAddress.Email,
                PhoneNumber = stdInfo.StudentAddress.Mobile,
                ModuleType = PaymentModuleType.Admission,
                AdditionalInfo = stdInfo.StudentFormNo
            };
            ViewBag.ErrorMessage = TempData["ErrorMessage"];
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            return model;
        }

        private void CreateBillDeskRequest(PaymentDetails payment)
        {
            ARGFormNoMaster formNoMaster = new RegistrationManager().GetFormNoMaster(PrintProgramme.IH) ?? new ARGFormNoMaster();
            BillDeskRequest billDeskRequest = new BillDeskRequest()
            {
                Entity_ID = payment.Student_ID,
                Email = payment.Email,
                PhoneNo = payment.PhoneNumber,
                TotalFee = formNoMaster.BasicFee,
                CustomerID = DateTime.Now.Ticks.ToString(),
                PrintProgramme = PrintProgramme.IH,
                Student_ID = payment.Student_ID,
                Semester = "0",
                ReturnURL = "NEPIG/PaymentResponse",
                AdditionalInfo = formNoMaster.AllowApplyForSelfFinancedSeat ? "SelfFinance" : payment.AdditionalInfo,
                NonBillDeskField = payment.AdditionalInfo,
            };

            var request = new BillDeskManager().GenerateRequestString(billDeskRequest, BillDeskPaymentType.ADM);
            var htmlForm = new BillDeskManager().GenerateHTMLForm(request);
            System.Web.HttpContext.Current.Response.Clear();
            System.Web.HttpContext.Current.Response.Write(htmlForm.ToString());
            System.Web.HttpContext.Current.Response.End();
        }

        #endregion
    }
}