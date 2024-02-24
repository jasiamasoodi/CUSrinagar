using CaptchaMvc.HtmlHelpers;
using CUSrinagar.BusinessManagers;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using CUSrinagar.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace CUSrinagar.WebApp.Controllers
{
    public class ApplicationFormsController : Controller
    {
        #region DepartmentalCertificateVerfication

        [HttpGet]
        public ActionResult Search()
        {
            ViewBag.PrintProgrammeList = Helper.GetSelectList<PrintProgramme>().OrderBy(x => x.Value);
            return View();
        }

        [HttpPost]
        public ActionResult Search(AttemptCertificate studentDetailsEntered)
        {
            ModelState.Remove("ReasonforIssuance");
            studentDetailsEntered.personalInformationCompact.CUSRegistrationNo.Replace(" ", "").Trim();
            if (string.IsNullOrWhiteSpace(studentDetailsEntered.personalInformationCompact.CUSRegistrationNo))
            {
                ModelState.AddModelError("personalInformationCompact.CusRegistrationNo", "Registration No. Required");
            }
            else if (studentDetailsEntered.personalInformationCompact.DOB == DateTime.MinValue)
            {
                ModelState.AddModelError("personalInformationCompact.DOB", "DOB Required");
            }
            else
            {
                studentDetailsEntered.personalInformationCompact = new StudentManager().GetStudent(studentDetailsEntered.personalInformationCompact.CUSRegistrationNo, studentDetailsEntered.personalInformationCompact.DOB, studentDetailsEntered.personalInformationCompact.PrintProgramme);

                if (studentDetailsEntered.personalInformationCompact != null)
                {
                    CertificateVerification certificateVerification = new ApplicationFormsManager().GetByStudentID(studentDetailsEntered.personalInformationCompact.Student_ID, studentDetailsEntered.personalInformationCompact.PrintProgramme) ?? new CertificateVerification();

                    certificateVerification.FullName = studentDetailsEntered.personalInformationCompact.FullName;
                    certificateVerification.FathersName = studentDetailsEntered.personalInformationCompact.FathersName;
                    certificateVerification.OrgProgramme = studentDetailsEntered.personalInformationCompact.PrintProgramme;
                    certificateVerification.Student_ID = studentDetailsEntered.personalInformationCompact.Student_ID;
                    certificateVerification.CUSRegistrationNo = studentDetailsEntered.personalInformationCompact.CUSRegistrationNo;
                    certificateVerification.CollegeFullName = studentDetailsEntered.personalInformationCompact.CollegeFullName;
                    certificateVerification.DOB = studentDetailsEntered.personalInformationCompact.DOB;

                    if (certificateVerification.FeePaid)
                    {
                        return RedirectToAction("Print", new { id = certificateVerification.Student_ID, p = studentDetailsEntered.personalInformationCompact.PrintProgramme.ToString().EncryptCookieAndURLSafe() });
                    }
                    else
                    {
                        return View("CertificateVerfication", certificateVerification);
                    }
                }
                else
                {
                    TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'> Student not found</a></div>"; ;
                }
            }

            ViewBag.PrintProgrammeList = Helper.GetSelectList<PrintProgramme>().OrderBy(x => x.Value);
            return View();
        }


        [HttpGet]
        public ActionResult CertificateVerfication()
        {
            return RedirectToAction("Search");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [OutputCache(Duration = 0, Location = System.Web.UI.OutputCacheLocation.None, NoStore = true)]
        public ActionResult CertificateVerfication(CertificateVerification certificateVerification)
        {
            if (ModelState.IsValid)
            {
                PersonalInformationCompact personalInformationCompact = new StudentManager().GetStudent(certificateVerification.CUSRegistrationNo, certificateVerification.DOB, certificateVerification.OrgProgramme);

                if (personalInformationCompact == null || personalInformationCompact?.Student_ID == Guid.Empty)
                {
                    TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'> Student not found.</a></div>";
                    return RedirectToAction("Search");
                }

                ARGStudentAddress studentAddress = new StudentManager().GetStudentAddress(personalInformationCompact.Student_ID, personalInformationCompact.PrintProgramme);
                certificateVerification.Student_ID = personalInformationCompact.Student_ID;
                certificateVerification.AppliedOn = DateTime.Now;
                certificateVerification.FeePaid = false;
                int result = new ApplicationFormsManager().Save(certificateVerification);
                if (result > 0)
                {
                    BillDeskRequest billDeskRequest = new BillDeskRequest
                    {
                        Email = certificateVerification.OrgEmail,
                        PhoneNo = studentAddress.Mobile,
                        ReturnURL = $"ApplicationForms/CVPayment",
                        PrintProgramme = certificateVerification.OrgProgramme,
                        Entity_ID = certificateVerification.Student_ID,
                        CustomerID = DateTime.UtcNow.Ticks.ToString(),
                        TotalFee = 300
                    };
                    StringBuilder sbHTML = new BillDeskManager().GenerateHTMLForm(new BillDeskManager().GenerateRequestString(billDeskRequest, BillDeskPaymentType.OTH));
                    System.Web.HttpContext.Current.Response.Clear();
                    System.Web.HttpContext.Current.Response.Write(sbHTML.ToString());
                    System.Web.HttpContext.Current.Response.End();
                    return new EmptyResult();
                }

            }
            return View("Search", certificateVerification);
        }

        public ActionResult CVPayment()
        {
            Tuple<bool, string, PaymentDetails, Guid, Guid> billDeskResponse = new BillDeskManager().BillDeskResponse(Request.InputStream);

            if (billDeskResponse.Item1)
            {
                //save payment and migration details in DB
                bool result = new ApplicationFormsManager().UpdatePayment(billDeskResponse.Item3);
                if (!result)
                {
                    TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>Something went wrong. If your account is debited, Please visit University I.T Section.</a></div>";
                    return RedirectToAction("Search", "ApplicationForms", new { area = string.Empty });
                }
                else
                {
                    TempData["alert"] = $"Payment received successfully under TxnReferenceNo. :{billDeskResponse.Item3.TxnReferenceNo} Amount (Rs): {billDeskResponse.Item3.TxnAmount}";
                    return RedirectToAction("Print", "ApplicationForms", new { id = billDeskResponse.Item3.Entity_ID, p = billDeskResponse.Item3.PrintProgramme.ToString().EncryptCookieAndURLSafe() });
                }
            }
            else
            {
                string ErrorMsg = (string.IsNullOrEmpty(billDeskResponse.Item2) || billDeskResponse.Item2.Trim().ToLower() == "na" || billDeskResponse.Item2.Trim().ToLower() == "na - na") ? "Sorry.  We were unable to process your transaction.  We apologise for the inconvenience and request you to try again later." : billDeskResponse.Item2;
                TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>{ErrorMsg}</a></div>";
                return RedirectToAction("search", "ApplicationForms", new { area = string.Empty });
            }
        }

        [HttpGet]
        public ActionResult Print(string id, string p)
        {
            string printProg = null;

            PrintProgramme printProgramme = PrintProgramme.UG;
            try
            {
                printProgramme = (PrintProgramme)Enum.Parse(typeof(PrintProgramme), (p + "").DecryptCookieAndURLSafe());
                printProg = "";
            }
            catch (Exception)
            {
                printProg = null;
            }
            if (!Guid.TryParse(id + "", out Guid Student_ID) || printProg == null)
            {
                return RedirectToAction("search", "ApplicationForms", new { area = string.Empty });
            }

            CertificateVerification certificateVerification = new ApplicationFormsManager().GetByStudentID(Student_ID, printProgramme) ?? new CertificateVerification();

            if (certificateVerification == null)
                return RedirectToAction("search", "ApplicationForms", new { area = string.Empty });

            if (!certificateVerification.FeePaid || certificateVerification.PaymentDetail == null)
                return RedirectToAction("search", "ApplicationForms", new { area = string.Empty });

            return View(certificateVerification);
        }
        #endregion

        #region Issuing of Degree Certificate
        [HttpGet]
        public ActionResult SearchStudent()
        {
            ViewBag.PrintProgrammeList = Helper.GetSelectList<PrintProgramme>().OrderBy(x => x.Value);
            return View();
        }

        [HttpPost]
        public ActionResult SearchStudent(AttemptCertificate studentDetailsEntered)
        {
            ModelState.Remove("ReasonforIssuance");
            studentDetailsEntered.personalInformationCompact.CUSRegistrationNo.Replace(" ", "").Trim();
            if (!this?.IsCaptchaValid("Captcha is not valid") ?? true)
            {
                TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>Invalid Verification Code, please try again.</a></div>"; ;
                return RedirectToAction("SearchStudent");
            }
            else
            {
                if (string.IsNullOrWhiteSpace(studentDetailsEntered.personalInformationCompact.CUSRegistrationNo))
                {
                    ModelState.AddModelError("personalInformationCompact.CusRegistrationNo", "Registration No. Required");
                }
                else if (studentDetailsEntered.personalInformationCompact.DOB == DateTime.MinValue)
                {
                    ModelState.AddModelError("personalInformationCompact.DOB", "DOB Required");
                }
                else
                {
                    Guid Student_ID = new StudentManager()
                        .GetStudentID(studentDetailsEntered.personalInformationCompact.CUSRegistrationNo,
                        studentDetailsEntered.personalInformationCompact.DOB,
                        studentDetailsEntered.personalInformationCompact.PrintProgramme,
                        true);

                    if (Student_ID != Guid.Empty)
                    {
                        return RedirectToAction("IssuanceOfDegreeCertificateForm", "ApplicationForms",
                            new { s = Student_ID, p = studentDetailsEntered.personalInformationCompact.PrintProgramme.ToString().EncryptCookieAndURLSafe() });
                    }
                    else
                    {
                        TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'> Student not found or has not been passed out yet.</a></div>"; ;
                    }
                }
                ViewBag.PrintProgrammeList = Helper.GetSelectList<PrintProgramme>().OrderBy(x => x.Value);
                return View();
            }
        }

        [HttpGet]
        public ActionResult IssuanceOfDegreeCertificateForm(string s, string p)
        {
            string printProg = null;
            ViewBags();

            PrintProgramme printProgramme = PrintProgramme.UG;
            try
            {
                printProgramme = (PrintProgramme)Enum.Parse(typeof(PrintProgramme), (p + "").DecryptCookieAndURLSafe());
                printProg = "";
            }
            catch (Exception)
            {
                printProg = null;
            }
            if (!Guid.TryParse(s + "", out Guid Student_ID) || printProg == null)
            {
                return RedirectToAction("SearchStudent", "ApplicationForms", new { area = string.Empty });
            }
            ApplicationFormsManager applicationFormsManager = new ApplicationFormsManager();

            IssuingOfDegreeCertificate issuanceOfDegreeCertificateFormDetails =
                                                        applicationFormsManager
                                                        .GetForIssuingOfDegreeCertificate(Student_ID, printProgramme);

            if (issuanceOfDegreeCertificateFormDetails == null)
            {
                TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'> Student not found or has not been passed out yet.</a></div>"; ;
                return RedirectToAction("SearchStudent", "ApplicationForms", new { area = string.Empty });
            }
            else if (issuanceOfDegreeCertificateFormDetails.PaymentStatus == FormStatus.FeePaid && issuanceOfDegreeCertificateFormDetails.PaymentDetail != null)
            {
                return RedirectToAction("PrintIssuanceOfDCForm", "ApplicationForms", new { s = Student_ID, p = printProgramme.ToString().EncryptCookieAndURLSafe() });
            }
            //else if (applicationFormsManager.DegreeAlreadyHandedOverOn(Student_ID))
            //{
            //    TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>It seems that you have already applied for advance degree certificate.</a></div>"; ;
            //    return RedirectToAction("SearchStudent", "ApplicationForms", new { area = string.Empty });
            //}

            List<SelectListItem> ExamNamesDDL = applicationFormsManager.GetQualifyingExamNames(issuanceOfDegreeCertificateFormDetails.Course_ID, printProgramme);
            if (printProgramme == PrintProgramme.PG || issuanceOfDegreeCertificateFormDetails.Course_ID == Guid.Parse("48887d19-f0c3-41cb-ac7a-22ea7b65494a")) //B.Ed course
            {
                ExamNamesDDL = ExamNamesDDL.Where(x => !x.Value.ToLower().Contains("12th")).ToList();
            }
            if (printProgramme == PrintProgramme.PG && issuanceOfDegreeCertificateFormDetails.Course_ID == Guid.Parse("c23dd7f4-a933-4deb-ba8e-f933830bccf8")) //M.Ed course
            {
                ExamNamesDDL = ExamNamesDDL.Where(x => !x.Value.ToLower().Contains("graduation")).ToList();
            }
            if (printProgramme == PrintProgramme.IH && issuanceOfDegreeCertificateFormDetails.Course_ID == Guid.Parse("FC32E138-4EE2-4DA2-9453-5C8368180BC3")) //Int. B.Ed-M.Ed course
            {
                ExamNamesDDL = ExamNamesDDL.Where(x => !x.Value.ToLower().Contains("graduation") || !x.Value.ToLower().Contains("12th")).ToList();
            }
            ViewBag.ExamNamesDDL = ExamNamesDDL;
            return View(issuanceOfDegreeCertificateFormDetails);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult IssuanceOfDegreeCertificateForm(IssuingOfDegreeCertificate issuingOfDegreeCertificate)
        {
            if (!this?.IsCaptchaValid("Captcha is not valid") ?? true)
            {
                TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>Invalid Verification Code, please try again.</a></div>"; ;
                return RedirectToAction("IssuanceOfDegreeCertificateForm", "ApplicationForms",
                new
                {
                    s = issuingOfDegreeCertificate.Student_ID,
                    p = issuingOfDegreeCertificate.PrintProgramme.ToString().EncryptCookieAndURLSafe()
                });
            }
            else
            {
                ModelState.Remove("StudentAddress.PinCode");
                ModelState.Remove("StudentAddress.PermanentAddress");
                ModelState.Remove("StudentAddress.District");
                ModelState.Remove("StudentAddress.Tehsil");
                ModelState.Remove("StudentAddress.Block");
                ModelState.Remove("StudentAddress.AssemblyConstituency");

                if (ModelState.IsValid)
                {
                    // Save here
                    Tuple<bool, string> response = new ApplicationFormsManager().Save(issuingOfDegreeCertificate);
                    if (response.Item1)
                    {
                        //send for payment
                        BillDeskRequest billDeskRequest = new BillDeskRequest
                        {
                            Email = issuingOfDegreeCertificate.StudentAddress.Email,
                            PhoneNo = issuingOfDegreeCertificate.StudentAddress.Mobile,
                            ReturnURL = $"ApplicationForms/IDCPaymentResponse",
                            PrintProgramme = issuingOfDegreeCertificate.PrintProgramme,
                            Entity_ID = issuingOfDegreeCertificate.Student_ID,
                            CustomerID = DateTime.UtcNow.Ticks.ToString(),
                            TotalFee = 400
                        };
                        StringBuilder sbHTML = new BillDeskManager().GenerateHTMLForm(new BillDeskManager().GenerateRequestString(billDeskRequest, BillDeskPaymentType.OTH));
                        System.Web.HttpContext.Current.Response.Clear();
                        System.Web.HttpContext.Current.Response.Write(sbHTML.ToString());
                        System.Web.HttpContext.Current.Response.End();
                        return new EmptyResult();
                    }
                    else
                    {
                        TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>Could not Save your details. Please check your entered details and try again.</a></div>"; ;
                    }
                }
                else
                {
                    TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>Invalid details entered. Please check your entered details and try again.</a></div>"; ;
                }

                return RedirectToAction("IssuanceOfDegreeCertificateForm", "ApplicationForms",
                    new
                    {
                        s = issuingOfDegreeCertificate.Student_ID,
                        p = issuingOfDegreeCertificate.PrintProgramme.ToString().EncryptCookieAndURLSafe()
                    });
            }
        }

        public ActionResult IDCPaymentResponse()
        {
            Tuple<bool, string, PaymentDetails, Guid, Guid> billDeskResponse = new BillDeskManager().BillDeskResponse(Request.InputStream);

            if (billDeskResponse.Item1)
            {
                //save payment and migration details in DB
                bool result = new ApplicationFormsManager().UpdateDegreeIssuingFormPayment(billDeskResponse.Item3);
                if (!result)
                {
                    TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>Something went wrong." +
                        $" If your amount is not deducted, then try to contact your Bank or use other online methods of banking like internet banking<br/>" +
                        $" If amount is deducted , Please visit University I.T Section by or before last date.</a></div>";
                    return RedirectToAction("SearchStudent", "ApplicationForms", new { area = string.Empty });
                }
                else
                {
                    return RedirectToAction("PrintIssuanceOfDCForm", "ApplicationForms", new { s = billDeskResponse.Item3.Entity_ID, p = billDeskResponse.Item3.PrintProgramme.ToString().EncryptCookieAndURLSafe() });
                }
            }
            else
            {
                TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>Something went wrong." +
                       $" If your amount is not deducted, then try to contact your Bank or use other online methods of banking like internet banking<br/>" +
                       $" If amount is deducted , Please visit University I.T Section by or before last date.</a></div>";
                return RedirectToAction("SearchStudent", "ApplicationForms", new { area = string.Empty });
            }
        }

        [HttpGet]
        public ActionResult PrintIssuanceOfDCForm(string s, string p)
        {
            string printProg = null;
            ViewBags();

            PrintProgramme printProgramme = PrintProgramme.UG;
            try
            {
                printProgramme = (PrintProgramme)Enum.Parse(typeof(PrintProgramme), (p + "").DecryptCookieAndURLSafe());
                printProg = "";
            }
            catch (Exception)
            {
                printProg = null;
            }
            if (!Guid.TryParse(s + "", out Guid Student_ID) || printProg == null)
            {
                return RedirectToAction("SearchStudent", "ApplicationForms", new { area = string.Empty });
            }
            ApplicationFormsManager applicationFormsManager = new ApplicationFormsManager();

            IssuingOfDegreeCertificate issuanceOfDegreeCertificateFormDetails =
                                                        applicationFormsManager
                                                        .GetForIssuingOfDegreeCertificate(Student_ID, printProgramme);

            if (issuanceOfDegreeCertificateFormDetails == null)
            {
                TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'> Student not found or has not been passed out yet.</a></div>"; ;
                return RedirectToAction("SearchStudent", "ApplicationForms", new { area = string.Empty });
            }
            else if (issuanceOfDegreeCertificateFormDetails.PaymentStatus != FormStatus.FeePaid || issuanceOfDegreeCertificateFormDetails.PaymentDetail == null)
            {
                TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>Please make payment before printing the form.</a></div>"; ;
                return RedirectToAction("SearchStudent", "ApplicationForms", new { area = string.Empty });
            }
            return View(issuanceOfDegreeCertificateFormDetails);
        }

        [HttpPost]
        public JsonResult GetStudentPreviousQualification(string s, string p, string e)
        {
            string printProg = null;
            ViewBags();

            PrintProgramme printProgramme = PrintProgramme.UG;
            try
            {
                printProgramme = (PrintProgramme)Enum.Parse(typeof(PrintProgramme), (p + ""));
                printProg = "";
            }
            catch (Exception)
            {
                printProg = null;
            }
            if (!Guid.TryParse(s + "", out Guid Student_ID) || printProg == null || string.IsNullOrWhiteSpace(e))
            {
                return Json(new
                {
                    error = "An error found in provided details",
                    qualifyingexam = ""
                }, JsonRequestBehavior.DenyGet);
            }
            ARGStudentPreviousQualifications qualifyingExam = new ApplicationFormsManager().GetStudentPreviousQualification(Student_ID, e, printProgramme) ?? new ARGStudentPreviousQualifications();

            return Json(new
            {
                error = "",
                qualifyingexam = qualifyingExam
            }, JsonRequestBehavior.DenyGet);
        }

        [NonAction]
        private void ViewBags()
        {
            ViewBag.EmploymentStatus = Helper.EmploymentStatusDDL();
            List<SelectListItem> Boards = Helper.BoardsDDL();
            Boards.RemoveAt(3);
            Boards.AddRange(Helper.UniversitiesDDL());
            ViewBag.Boards = Boards;
            ViewBag.Session = Helper.SessionDDL();
            ViewBag.I2thStreamDDL = Helper.I2thStreamDDL();
        }

        #endregion

        #region Application form for CORRECTION
        [HttpGet]
        public ActionResult SearchForCorrection()
        {
            ViewBag.PrintProgrammeList = Helper.GetSelectList<PrintProgramme>().OrderBy(x => x.Value);
            return View();
        }

        [HttpPost]
        public ActionResult SearchForCorrection(AttemptCertificate studentDetailsEntered)
        {
            ModelState.Remove("ReasonforIssuance");
            if (!this?.IsCaptchaValid("Captcha is not valid") ?? true)
            {
                TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>Invalid Verification Code, please try again.</a></div>"; ;
                return RedirectToAction("SearchForCorrection");
            }
            else
            {
                if (string.IsNullOrWhiteSpace(studentDetailsEntered.personalInformationCompact.CUSRegistrationNo))
                {
                    ModelState.AddModelError("personalInformationCompact.CusRegistrationNo", "Registration No. Required");
                }
                else if (studentDetailsEntered.personalInformationCompact.DOB == DateTime.MinValue)
                {
                    ModelState.AddModelError("personalInformationCompact.DOB", "DOB Required");
                }
                else
                {
                    Guid Student_ID = new StudentManager()
                        .GetStudent(studentDetailsEntered.personalInformationCompact.CUSRegistrationNo,
                        studentDetailsEntered.personalInformationCompact.DOB,
                        studentDetailsEntered.personalInformationCompact.PrintProgramme)?.Student_ID ?? Guid.Empty;

                    if (Student_ID != Guid.Empty)
                    {
                        return RedirectToAction("CorrectionForm", "ApplicationForms",
                            new { s = Student_ID, p = studentDetailsEntered.personalInformationCompact.PrintProgramme.ToString().EncryptCookieAndURLSafe() });
                    }
                    else
                    {
                        TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'> Student not found.</a></div>"; ;
                    }
                }
                ViewBag.PrintProgrammeList = Helper.GetSelectList<PrintProgramme>().OrderBy(x => x.Value);
                return View();
            }
        }

        [HttpGet]
        public ActionResult CorrectionForm(string s, string p)
        {
            string printProg = null;

            PrintProgramme printProgramme = PrintProgramme.UG;
            try
            {
                printProgramme = (PrintProgramme)Enum.Parse(typeof(PrintProgramme), (p + "").DecryptCookieAndURLSafe());
                printProg = "";
            }
            catch (Exception)
            {
                printProg = null;
            }
            if (!Guid.TryParse(s + "", out Guid Student_ID) || printProg == null)
            {
                return RedirectToAction("SearchForCorrection", "ApplicationForms", new { area = string.Empty });
            }

            ApplicationFormsManager applicationFormsManager = new ApplicationFormsManager();
            CorrectionForm correctionForm = applicationFormsManager.GetCorrectionForm(Student_ID, printProgramme);

            if ((correctionForm?.Student_ID ?? Guid.Empty) == Guid.Empty)
            {
                TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'> Student not found.</a></div>"; ;
                return RedirectToAction("SearchForCorrection", "ApplicationForms", new { area = string.Empty });
            }
            else if (correctionForm.PaymentStatus == FormStatus.FeePaid && correctionForm.PaymentDetail != null)
            {
                return RedirectToAction("PrintCorrectionForm", "ApplicationForms", new { s = Student_ID, p = printProgramme.ToString().EncryptCookieAndURLSafe() });
            }
            ViewBag.AmountPayable = applicationFormsManager.CalculateCorrectionFormFee(correctionForm);
            return View(correctionForm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CorrectionForm(CorrectionForm correctionForm)
        {
            if (!this?.IsCaptchaValid("Captcha is not valid") ?? true)
            {
                TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>Invalid Verification Code, please try again.</a></div>"; ;
                return RedirectToAction("CorrectionForm", "ApplicationForms",
                new
                {
                    s = correctionForm.Student_ID,
                    p = correctionForm.PrintProgramme.ToString().EncryptCookieAndURLSafe()
                });
            }
            else
            {
                if (ModelState.IsValid)
                {
                    ApplicationFormsManager applicationFormsManager = new ApplicationFormsManager();
                    CorrectionForm correctionFormDB =
                        applicationFormsManager.GetCorrectionForm(correctionForm.Student_ID, correctionForm.PrintProgramme);

                    if ((correctionFormDB?.Student_ID ?? Guid.Empty) == Guid.Empty)
                    {
                        TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'> Student not found or has not been passed out yet.</a></div>"; ;
                        return RedirectToAction("SearchForCorrection", "ApplicationForms", new { area = string.Empty });
                    }

                    // Save here
                    correctionFormDB.NewDOB = correctionForm.NewDOB;
                    correctionFormDB.NewFullName = correctionForm.NewFullName.ToUpper();
                    correctionFormDB.NewFathersName = correctionForm.NewFathersName.ToUpper();
                    correctionFormDB.AppliedOn = DateTime.Now;
                    correctionFormDB.PaymentStatus = FormStatus.InProcess;
                    correctionFormDB.ReasonAndDocumentaryProof = correctionForm.ReasonAndDocumentaryProof;

                    Tuple<bool, string> response = applicationFormsManager.SaveCorrectionForm(correctionFormDB);

                    if (response.Item1)
                    {
                        //send for payment
                        ARGStudentAddress studentAddress = new StudentManager().GetStudentAddress(correctionFormDB.Student_ID, correctionFormDB.PrintProgramme);
                        BillDeskRequest billDeskRequest = new BillDeskRequest
                        {
                            Email = studentAddress.Email,
                            PhoneNo = studentAddress.Mobile,
                            ReturnURL = $"ApplicationForms/CFPaymentResponse",
                            PrintProgramme = correctionFormDB.PrintProgramme,
                            Entity_ID = correctionFormDB.Student_ID,
                            CustomerID = DateTime.UtcNow.Ticks.ToString(),
                            TotalFee = applicationFormsManager.CalculateCorrectionFormFee(correctionFormDB)
                        };
                        StringBuilder sbHTML = new BillDeskManager().GenerateHTMLForm(new BillDeskManager().GenerateRequestString(billDeskRequest, BillDeskPaymentType.OTH));
                        System.Web.HttpContext.Current.Response.Clear();
                        System.Web.HttpContext.Current.Response.Write(sbHTML.ToString());
                        System.Web.HttpContext.Current.Response.End();
                        return new EmptyResult();
                    }
                    else
                    {
                        TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>Could not Save your details. Please check your entered details and try again.</a></div>"; ;
                    }
                }
                else
                {
                    TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>Invalid details entered. Please check your entered details and try again.</a></div>"; ;
                }

                return RedirectToAction("SearchForCorrection", "ApplicationForms",
                    new
                    {
                        s = correctionForm.Student_ID,
                        p = correctionForm.PrintProgramme.ToString().EncryptCookieAndURLSafe()
                    });
            }
        }

        public ActionResult CFPaymentResponse()
        {
            Tuple<bool, string, PaymentDetails, Guid, Guid> billDeskResponse = new BillDeskManager().BillDeskResponse(Request.InputStream);

            if (billDeskResponse.Item1)
            {
                //save payment and migration details in DB
                bool result = new ApplicationFormsManager().UpdateCorrectionFormPayment(billDeskResponse.Item3);
                if (!result)
                {
                    TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>Something went wrong." +
                        $" If your amount is not deducted, then try to contact your Bank or use other online methods of banking like internet banking<br/>" +
                        $" If amount is deducted , Please visit University I.T Section by or before last date.</a></div>";
                    return RedirectToAction("SearchForCorrection", "ApplicationForms", new { area = string.Empty });
                }
                else
                {
                    return RedirectToAction("PrintCorrectionForm", "ApplicationForms", new { s = billDeskResponse.Item3.Entity_ID, p = billDeskResponse.Item3.PrintProgramme.ToString().EncryptCookieAndURLSafe() });
                }
            }
            else
            {
                TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>Something went wrong." +
                       $" If your amount is not deducted, then try to contact your Bank or use other online methods of banking like internet banking<br/>" +
                       $" If amount is deducted , Please visit University I.T Section by or before last date.</a></div>";
                return RedirectToAction("SearchForCorrection", "ApplicationForms", new { area = string.Empty });
            }
        }

        [HttpGet]
        public ActionResult PrintCorrectionForm(string s, string p)
        {
            string printProg = null;

            PrintProgramme printProgramme = PrintProgramme.UG;
            try
            {
                printProgramme = (PrintProgramme)Enum.Parse(typeof(PrintProgramme), (p + "").DecryptCookieAndURLSafe());
                printProg = "";
            }
            catch (Exception)
            {
                printProg = null;
            }
            if (!Guid.TryParse(s + "", out Guid Student_ID) || printProg == null)
            {
                return RedirectToAction("SearchForCorrection", "ApplicationForms", new { area = string.Empty });
            }
            ApplicationFormsManager applicationFormsManager = new ApplicationFormsManager();

            CorrectionForm correctionForm = applicationFormsManager.GetCorrectionForm(Student_ID, printProgramme);

            if ((correctionForm?.Student_ID ?? Guid.Empty) == Guid.Empty)
            {
                TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'> Student not found.</a></div>"; ;
                return RedirectToAction("SearchForCorrection", "ApplicationForms", new { area = string.Empty });
            }
            else if (correctionForm.PaymentStatus != FormStatus.FeePaid || correctionForm.PaymentDetail == null)
            {
                TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>Please make payment before printing the form.</a></div>"; ;
                return RedirectToAction("SearchForCorrection", "ApplicationForms", new { area = string.Empty });
            }
            return View(correctionForm);
        }
        #endregion
    }
}