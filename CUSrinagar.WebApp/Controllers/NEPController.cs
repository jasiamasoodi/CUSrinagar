using CaptchaMvc.HtmlHelpers;
using CUSrinagar.BusinessManagers;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using CUSrinagar.Models;
using CUSrinagar.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;


namespace CUSrinagar.WebApp.Controllers
{
    public class NEPController : WebAppBaseController
    {
        string errorMsg = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>##</a></div>";
        List<CertificateType> CertificatesToBeUploaded { get { return new List<CertificateType>() { CertificateType.MarksCard, CertificateType.Provisional, CertificateType.Category }; } }
        short MaximumNumber_Of_Course_DDL_Options { get { return 10; } }

        [HttpPost]
        public ActionResult Direction(PrintProgramme? p)
        {
            switch (p)
            {
                case PrintProgramme.IH:
                    return RedirectToAction("PersonalInfo", "NEPIG", new { area = "" });
                case PrintProgramme.UG:
                    return RedirectToAction("PersonalInfo", "NEP", new { area = "" });
                case PrintProgramme.PG:
                    return RedirectToAction("PersonalInfo", "NEPPG", new { area = "" });
                default:
                    return RedirectToAction("Index", "NEP", new { area = "" });
            }
        }


        [HttpGet]
        public ActionResult Index()
        {
            try
            {
                CourseManager courseManager = new CourseManager();
                ViewBag.AppliedProgrammes = Helper.GetSelectList<NEPProgrammesAdmTitle>();

                List<PrintProgramme> CoursesRegOpen = courseManager.GetOpenResistrationProgrammeCategories();
                IEnumerable<SelectListItem> SelectListItemOptions = Helper.GetSelectForAdmission(CoursesRegOpen)
                                                                    ?.DistinctBy(x => new { x.Value })?.ToList();

                foreach (var item in SelectListItemOptions ?? new List<SelectListItem>())
                {
                    if (item.Value == ((short)PrintProgramme.IH).ToString())
                    {
                        new RegistrationManager().CloseNewAdmission(PrintProgramme.IH);
                        item.Text = NEPProgrammesAdmTitle.Year_5_Integrated_PG_Programmes.GetEnumDescription();
                    }
                    else if (item.Value == ((short)PrintProgramme.UG).ToString())
                    {
                        new RegistrationManager().CloseNewAdmission(PrintProgramme.UG);
                        item.Text = NEPProgrammesAdmTitle.Year_4_UG_Honors_Programmes.GetEnumDescription();
                    }
                    else if (item.Value == ((short)PrintProgramme.PG).ToString())
                    {
                        new RegistrationManager().CloseNewAdmission(PrintProgramme.PG);
                        item.Text = NEPProgrammesAdmTitle.Post_Graduation_Programmes.GetEnumDescription();
                    }
                }
                ViewBag.PrintProgrammeOption = SelectListItemOptions;
            }
            catch (Exception) { }
            ARGReprint aRGReprint = new ARGReprint();

            //delete cookie here 
            new NEPManager().DeleteCookie(PrintProgramme.IH);
            new NEPManager().DeleteCookie(PrintProgramme.UG);
            new NEPManager().DeleteCookie(PrintProgramme.PG);

            return View(aRGReprint);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(ARGReprint aRGReprint)
        {
            ViewBag.AppliedProgrammes = Helper.GetSelectList<NEPProgrammesAdmTitle>();

            if (!(this?.IsCaptchaValid("") ?? false))
            {
                TempData["ErrorMessage"] = errorMsg.Replace("##", "Invalid Verification Code");
                return RedirectToAction("Index");
            }
            else
            {
                if (aRGReprint != null && aRGReprint.FormNo != null && aRGReprint.PrintProgrammeOption != 0)
                {
                    Guid _Student_ID = new NEPManager().GetStudentID(aRGReprint);
                    if (_Student_ID == Guid.Empty)
                    {
                        TempData["ErrorMessage"] = errorMsg.Replace("##", "No record found in current session");
                        return RedirectToAction("Index");
                    }
                    string encryptStudent_ID = _Student_ID.ToString().EncryptCookieAndURLSafe();

                    #region Block Edit
                    bool blockEdit = new NEPManager().BlockEditAfterPayment(_Student_ID, aRGReprint.PrintProgrammeOption);
                    if (blockEdit)
                    {
                        if (aRGReprint.PrintProgrammeOption == PrintProgramme.IH)
                        {
                            return RedirectToAction("Print", "NEPIG", new { r = encryptStudent_ID });
                        }
                        else if (aRGReprint.PrintProgrammeOption == PrintProgramme.PG)
                        {
                            return RedirectToAction("Print", "NEPPG", new { r = encryptStudent_ID });
                        }
                        else
                        {
                            return RedirectToAction("Print", "NEP", new { r = encryptStudent_ID });
                        }
                    }
                    #endregion

                    if (aRGReprint.PrintProgrammeOption == PrintProgramme.IH)
                    {
                        //create cookie
                        new NEPManager().CreateCookie(PrintProgramme.IH, encryptStudent_ID);
                        return RedirectToAction("PersonalInfo", "NEPIG", new { r = encryptStudent_ID });
                    }
                    else if (aRGReprint.PrintProgrammeOption == PrintProgramme.PG)
                    {
                        //create cookie
                        new NEPManager().CreateCookie(PrintProgramme.PG, encryptStudent_ID);
                        return RedirectToAction("PersonalInfo", "NEPPG", new { r = encryptStudent_ID });
                    }
                    else
                    {
                        //create cookie
                        new NEPManager().CreateCookie(PrintProgramme.UG, encryptStudent_ID);
                        return RedirectToAction("PersonalInfo", "NEP", new { r = encryptStudent_ID });
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = errorMsg.Replace("##", "No record found"); ;
                    return RedirectToAction("Index");
                }
            }
        }

        #region PersonalInfo

        [HttpGet]
        public ActionResult PersonalInfo(string r)
        {
            ARGPersonalInformation model = null;
            Guid _Student_ID = Guid.Empty;

            if (!r.IsNullOrEmpty() && Guid.TryParse(r.DecryptCookieAndURLSafe(), out _Student_ID))
                model = new RegistrationManager().GetStudentPersonalInfoOnly(Guid.Parse(r.DecryptCookieAndURLSafe()), PrintProgramme.UG, true);
            if ((r + "").Length > 0 && _Student_ID == Guid.Empty)
            {
                return RedirectToAction("Index");
            }

            if (_Student_ID != Guid.Empty)
            {
                if (new NEPManager().HasCookiePresent(PrintProgramme.UG, _Student_ID.ToString().EncryptCookieAndURLSafe()) == false)
                {
                    return RedirectToAction("Index");
                }
            }

            model = FillPersonalInfo_ViewBags(model) ?? new ARGPersonalInformation();

            var RegistrationProcessClosed = new NEPManager().GetCourseListForRegistrationByGroup(PrintProgramme.UG, _Student_ID).IsNullOrEmpty();
            if (RegistrationProcessClosed)
            {
                var result = new NEPManager().GetNEPPersonalInfo(model.Student_ID, PrintProgramme.UG);
                if (result.Item1)
                    return RedirectToAction("Print", new { r = model.Student_ID.ToString().EncryptCookieAndURLSafe() });
                else
                {
                    TempData["ErrorMessage"] = errorMsg.Replace("##", result.Item2);
                    return RedirectToAction("Index");
                }
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PersonalInfo(ARGPersonalInformation model)
        {
            #region validation
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
                    Tuple<bool, string> _result = new NEPManager().Save(model, PrintProgramme.UG);

                    if (_result.Item1)
                    {
                        new NEPManager().CreateCookie(PrintProgramme.UG, model.Student_ID.ToString().EncryptCookieAndURLSafe());
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
                return RedirectToAction("Index");
            }

            if (r.IsNullOrEmpty()
                || !Guid.TryParse(r.DecryptCookieAndURLSafe(), out Guid _Student_ID)
                || _Student_ID == Guid.Empty)
            {
                TempData["ErrorMessage"] = errorMsg.Replace("##", "Invalid request. Please try again.");
                return RedirectToAction("Index");
            }

            if (new NEPManager().HasCookiePresent(PrintProgramme.UG, _Student_ID.ToString().EncryptCookieAndURLSafe()) == false)
            {
                return RedirectToAction("Index");
            }

            ARGStudentPreviousQualifications previousQualification;
            try
            {
                previousQualification = (ARGStudentPreviousQualifications)TempData["model"] ??
                         new RegistrationManager().GetStudentAcademicDetails(_Student_ID, PrintProgramme.UG)?.FirstOrDefault();
            }
            catch (Exception)
            {
                previousQualification = new RegistrationManager().GetStudentAcademicDetails(_Student_ID, PrintProgramme.UG)?.FirstOrDefault();
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
                    new NEPManager().DeletePreviousQualifications(model.Student_ID, PrintProgramme.UG);
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
                Tuple<bool, string> _result = new NEPManager().Save(model, PrintProgramme.UG);
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
                return RedirectToAction("Index");
            }

            if (r.IsNullOrEmpty() || !Guid.TryParse(r.DecryptCookieAndURLSafe(), out Guid _Student_ID)
                || _Student_ID == Guid.Empty)
            {
                TempData["ErrorMessage"] = errorMsg.Replace("##", "Invalid request. Please try again.");
                return RedirectToAction("Index");
            }
            if (new NEPManager().HasCookiePresent(PrintProgramme.UG, _Student_ID.ToString().EncryptCookieAndURLSafe()) == false)
            {
                return RedirectToAction("Index");
            }

            ARGStudentPreviousQualifications previousQualification = new RegistrationManager().GetStudentAcademicDetails(_Student_ID, PrintProgramme.UG)?.FirstOrDefault();

            //if (previousQualification == null)
            //{
            //    TempData["ErrorMessage"] = errorMsg.Replace("##", "Fill qualification details first then try next step.");
            //    return RedirectToAction("SubjectPreference", new { r = _Student_ID.ToString().EncryptCookieAndURLSafe() });
            //}

            List<ARGCoursesApplied> listOfCoursesOpted = (List<ARGCoursesApplied>)TempData["model"] ?? new RegistrationManager().GetStudentCourses(_Student_ID, PrintProgramme.UG);

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
                    Tuple<bool, string> _result = new NEPManager().Save(model, PrintProgramme.UG);
                    if (_result.Item1)
                    {
                        TempData["SuccessMessage"] = "Details saved successfully.";
                        return RedirectToAction("CollegePreference", new { r = model.First().Student_ID.ToString().EncryptCookieAndURLSafe() });
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
            return RedirectToAction("SubjectPreference", new { r = model.First().Student_ID.ToString().EncryptCookieAndURLSafe() });
        }
        #endregion

        #region CollegePreference
        [HttpGet]
        public ActionResult CollegePreference(string r)
        {
            List<PrintProgramme> CoursesRegOpen = new CourseManager().GetOpenResistrationProgrammeCategories();

            if (CoursesRegOpen.IsNullOrEmpty())
            {
                TempData["ErrorMessage"] = errorMsg.Replace("##", "Invalid request. Please try again.");
                return RedirectToAction("Index");
            }

            if (r.IsNullOrEmpty() || !Guid.TryParse(r.DecryptCookieAndURLSafe(), out Guid _Student_ID)
                || _Student_ID == Guid.Empty)
            {
                TempData["ErrorMessage"] = errorMsg.Replace("##", "Invalid request. Please try again.");
                return RedirectToAction("Index");
            }
            if (new NEPManager().HasCookiePresent(PrintProgramme.UG, _Student_ID.ToString().EncryptCookieAndURLSafe()) == false)
            {
                return RedirectToAction("Index");
            }

            List<NEPCollegePreference> listOfCollegesOpted = (List<NEPCollegePreference>)TempData["model"] ?? new NEPManager().GetCollegePreferences(_Student_ID, PrintProgramme.UG);
            ViewBag.HasDBCollegePreference = (listOfCollegesOpted != null && (List<NEPCollegePreference>)TempData["model"] == null);
            ViewBag.Student_ID = _Student_ID;
            List<NEPCollegePreference> model = FillCollegePreference_ViewBags(_Student_ID, listOfCollegesOpted);
            return View("CollegePreference", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CollegePreference(List<NEPCollegePreference> model)
        {
            model = model.Where(x => x.College_ID != Guid.Empty).ToList();
            if (!model.IsNullOrEmpty())
            {
                if (ModelState.IsValid)
                {
                    short _preference = 1;
                    model.OrderBy(x => x.PreferenceNo).ToList().ForEach(x => { x.PreferenceNo = _preference++; });

                    Tuple<bool, string> _result = new NEPManager().Save(model, PrintProgramme.UG);
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
            return RedirectToAction("CollegePreference", new { r = model.First().Student_ID.ToString().EncryptCookieAndURLSafe() });
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
                return RedirectToAction("Index");
            }

            if (r.IsNullOrEmpty() || !Guid.TryParse(r.DecryptCookieAndURLSafe(), out Guid _Student_ID)
                || _Student_ID == Guid.Empty)
            {
                TempData["ErrorMessage"] = errorMsg.Replace("##", "Invalid request. Please try again.");
                return RedirectToAction("Index");
            }

            if (new NEPManager().HasCookiePresent(PrintProgramme.UG, _Student_ID.ToString().EncryptCookieAndURLSafe()) == false)
            {
                return RedirectToAction("Index");
            }

            NEPDocuments nepDocuments = new NEPManager().GetDocuments(_Student_ID, PrintProgramme.UG);
            ViewBag.HasDocuments = nepDocuments != null;
            NEPDocuments model = FillDocumentPreference_ViewBags(_Student_ID, nepDocuments, PrintProgramme.UG);
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
                Tuple<bool, string> _result = new NEPManager().Save(model, PrintProgramme.UG);
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
                return RedirectToAction("Index");
            }

            if (r.IsNullOrEmpty() || !Guid.TryParse(r.DecryptCookieAndURLSafe(), out Guid _Student_ID)
                || _Student_ID == Guid.Empty)
            {
                TempData["ErrorMessage"] = errorMsg.Replace("##", "Invalid request. Please try again.");
                return RedirectToAction("Index");
            }
            if (new NEPManager().HasCookiePresent(PrintProgramme.UG, _Student_ID.ToString().EncryptCookieAndURLSafe()) == false)
            {
                return RedirectToAction("Index");
            }

            string isEligileForPayment = new NEPManager().IsEligibleForPayment(_Student_ID, PrintProgramme.UG);

            if (!string.IsNullOrWhiteSpace(isEligileForPayment))
            {
                TempData["ErrorMessage"] = errorMsg.Replace("##", isEligileForPayment);
                return RedirectToAction("Index");
            }


            ViewBag.PersonalInfoCompact = new StudentManager().GetStudentC(_Student_ID, PrintProgramme.UG);

            PaymentDetails payment = new PaymentManager().GetPaymentDetails(_Student_ID, PaymentModuleType.Admission, PrintProgramme.UG)?.FirstOrDefault();
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
                var result = new NEPManager().Save(billdeskResponse.Item3, PrintProgramme.UG);
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
                return RedirectToAction("Index");
            }

            #region Block Edit
            bool blockEdit = new NEPManager().BlockEditAfterPayment(_Student_ID, PrintProgramme.UG);
            if (!blockEdit)
            {
                if (new NEPManager().HasCookiePresent(PrintProgramme.UG, _Student_ID.ToString().EncryptCookieAndURLSafe()) == false
                    && !(Request?.UrlReferrer + "").ToLower().Contains("custudentzone"))
                {
                    return RedirectToAction("Index");
                }
            }
            #endregion
            var result = await new NEPManager().GetNEPPersonalInfoAsync(_Student_ID, PrintProgramme.UG);

            if (!result.Item1)
            {
                {
                    TempData["ErrorMessage"] = errorMsg.Replace("##", result.Item2);
                    return RedirectToAction("Index");
                }
            }
            ViewBag.courseApplied = NEPProgrammesAdmTitle.Year_4_UG_Honors_Programmes.GetEnumDescription();
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
                    BoardRegistrationNo = "JKBOSE-" + DateTime.Now.Ticks.ToString(),
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
                    ReadOnly = true,
                    IsProvisional = false,
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
                    model.Add(new ARGCoursesApplied() { Student_ID = _Student_ID, Preference = index, IsActive = true, PrintProgramme = PrintProgramme.UG, Programme = Programme.UG });
            }
            else
            {
                model = listOfCoursesOpted;
                var number_Of_DDL_Options = MaximumNumber_Of_Course_DDL_Options - listOfCoursesOpted.Count;
                if (number_Of_DDL_Options > 0)
                {
                    var maxPrefenceOrder = listOfCoursesOpted.Max(x => x.Preference);
                    for (int index = 1; index <= number_Of_DDL_Options; index++)
                        model.Add(new ARGCoursesApplied() { Student_ID = _Student_ID, Preference = (short)(maxPrefenceOrder + index), IsActive = true, PrintProgramme = PrintProgramme.UG, Programme = Programme.UG });
                }
                model = model.OrderBy(x => x.Preference)?.ToList();
            }

            List<DropDownOptLabelGeneral> CoursesDDL = new NEPManager().GetCourseListForRegistrationByGroup(PrintProgramme.UG, _Student_ID);

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
                    int indexOfComputerApp = CoursesDDL.FindIndex(x => Guid.Parse(x.Value.ToString()) == Guid.Parse("FFC176D2-5C33-4B77-9F11-7D0F8EFE3BC6"));/*Computer Application*/
                    if (indexOfComputerApp != -1)
                        CoursesDDL.RemoveAt(indexOfComputerApp);

                    int indexOfIT = CoursesDDL.FindIndex(x => Guid.Parse(x.Value.ToString()) == Guid.Parse("707294CF-B480-48C8-AFBB-07324A95A523")); /*IT*/
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
            List<ARGCoursesApplied> listOfCoursesOpted = new RegistrationManager().GetStudentCourses(_Student_ID, PrintProgramme.UG);
            if (listOfCoursesOpted.IsNullOrEmpty())
            {
                ViewBag.ErrorMessage = "No course/main subject opted";
                return null;
            }

            List<SelectListItem> listOfCollegePreferences = new CollegeManager().GetCollegePreference(listOfCoursesOpted.Select(x => x.Course_ID).ToList());
            ARGPersonalInformation stdInfo = new RegistrationManager().GetStudentPersonalInfoOnly(_Student_ID, PrintProgramme.UG);
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
            ARGPersonalInformation stdInfo = new RegistrationManager().GetStudentPersonalInfoOnly(_Student_ID, PrintProgramme.UG, true);
            var Amount = new RegistrationManager().GetFormNoMaster(PrintProgramme.UG).BasicFee;
            PaymentDetails model = payment ?? new PaymentDetails()
            {
                TxnAmount = Amount,
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
            var Amount = new RegistrationManager().GetFormNoMaster(PrintProgramme.UG).BasicFee;
            BillDeskRequest billDeskRequest = new BillDeskRequest()
            {
                Entity_ID = payment.Student_ID,
                Email = payment.Email,
                PhoneNo = payment.PhoneNumber,
                TotalFee = Amount,
                CustomerID = DateTime.Now.Ticks.ToString(),
                PrintProgramme = PrintProgramme.UG,
                Student_ID = payment.Student_ID,
                Semester = "0",
                ReturnURL = "NEP/PaymentResponse",
                AdditionalInfo = payment.AdditionalInfo,
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