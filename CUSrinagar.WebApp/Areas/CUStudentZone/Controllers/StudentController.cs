using CUSrinagar.Enums;
using CUSrinagar.Models;
using CUSrinagar.OAuth;
using System;
using CUSrinagar.BusinessManagers;

using System.Collections.Generic;
using CUSrinagar.Extensions;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GeneralModels;

namespace CUSrinagar.WebApp.CUStudentZone.Controllers
{
    [OAuthorize(AppRoles.Student)]
    public class StudentController : StudentBaseController
    {
        private string errorHtml = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'></button> <a href='#' class='alert-link'>#msg</a></div>";

        #region StudentProfileAndExamForms
        public ActionResult StudentProfile()
        {
            var studentInfo = new StudentManager().GetStudent(AppUserHelper.User_ID, AppUserHelper.TableSuffix);
            if (studentInfo != null && studentInfo.SelectedCombinations != null)
            {
                ViewBag.CourseName = studentInfo.SelectedCombinations.First().CourseID;
                ARGExamFormDownloadable frm = new ExaminationFormManager().GetRegularExamFormDownloadable(studentInfo.SelectedCombinations.First().PrintProgramme, studentInfo.CurrentSemesterOrYear);
                ViewBag.DownloadExamForm = frm?.AllowDownloadForm;
            }
            return View(studentInfo);
        }
        public ActionResult ExaminationForm(Guid? id)
        {
            var studentInfo = new StudentManager().GetStudent(AppUserHelper.User_ID, AppUserHelper.TableSuffix);
            if (studentInfo != null && studentInfo.SelectedCombinations != null)
            {
                //var currentSemSelectedComb = studentInfo.SelectedCombinations.First();
                //ViewBag.CourseName = currentSemSelectedComb.CourseID;
                //var printProgramme = currentSemSelectedComb.PrintProgramme;
                //if (currentSemSelectedComb.CourseCode == "MAEDU16")
                //{
                //    printProgramme = PrintProgramme.MED;
                //}
                //ARGExamFormDownloadable frm = new ExaminationFormManager().GetExamFormDownloadable(printProgramme, studentInfo.CurrentSemesterOrYear, 2018);
                //if (frm != null && frm.AllowDownloadForm)
                //{
                //    var responseData = new ExaminationFormManager().GetSetStudentExamForm(studentInfo.Student_ID, frm);
                //    if (responseData.IsSuccess)
                //    {
                //        ARGStudentExamForm examForm = (ARGStudentExamForm)responseData.ResponseObject;
                //        ViewBag.StudentExamFormNumber = examForm?.FormNumber;
                //    }
                //    var additionalSubjects = new StudentManager().GetStudentAdditionalSubjects(studentInfo.Student_ID, 2, printProgramme);
                //    if (additionalSubjects != null)
                //    {
                //        frm.TotalFee += additionalSubjects.Count * 275;
                //    }
                //    ViewBag.TotalExamFee = frm.TotalFee;
                //}
            }
            return View(studentInfo);
        }
        #endregion


        #region NextSemesterAdmission With Online Payment
        [HttpGet]
        public ActionResult ApplySemesterAdm()
        {
            ViewBag.AdmSemesterReprint = 0;
            ViewBag.isSelfFinance = false;
            short AdmissionSemesterReprint = 0;
            var studentManager = new StudentManager();

            Guid Student_ID = AppUserHelper.User_ID;
            PrintProgramme printProgramme = AppUserHelper.TableSuffix;

            ARGPersonalInformation personalInformation = new RegistrationManager().GetStudentPersonalInfoOnly(Student_ID, printProgramme);
            personalInformation.PaymentDetail = new List<PaymentDetails>();

            Guid StudentCourse_ID = studentManager.GetStudentCurrentCourse();
            short AdmissionSemester = 0;
            AdmissionSemester = studentManager.GetAdmissionSemester(personalInformation.CurrentSemesterOrYear);

            if (AdmissionSemester > 1)
                personalInformation.Batch = studentManager.getStudentSemesterBatch(Student_ID, printProgramme, personalInformation.CurrentSemesterOrYear);
            ADMSemesterAdmissionSettings _ADMSemesterAdmissionSettings = new ADMSemesterAdmissionSettings();


            PaymentDetails payment = new PaymentManager().GetPaymentDetail(Student_ID, AdmissionSemester);
            if (payment != null)
                personalInformation.PaymentDetail.Add(payment);

            ViewBag.CourseFullName = new CourseManager().GetItem(StudentCourse_ID)?.CourseFullName ?? AppUserHelper.OrgPrintProgramme.GetEnumDescription();
            ViewBag.AdmSemester = AdmissionSemester;

            _ADMSemesterAdmissionSettings = studentManager.GetSemesterAdmissionSettings(StudentCourse_ID, AdmissionSemester);



            if (_ADMSemesterAdmissionSettings != null)
            {
                //check for closing Date
                if (DateTime.Now > _ADMSemesterAdmissionSettings.ClosingDate)
                {
                    studentManager.CloseSemesterAdm(StudentCourse_ID, AdmissionSemester);
                    return RedirectToAction("ApplySemesterAdm", "Student", new { area = "CUStudentZone" });
                }

                _ADMSemesterAdmissionSettings.AdmissionFee += studentManager.GetAdditionalFeeSemesterAdm(personalInformation.CUSRegistrationNo + string.Empty, AdmissionSemester);

                //reprint
                AdmissionSemesterReprint = studentManager.GetAdmissionSemesterRePrint(personalInformation.CurrentSemesterOrYear);
                if (AdmissionSemesterReprint > 0)
                {
                    ViewBag.AdmSemesterReprint = AdmissionSemesterReprint;
                    PaymentDetails paymentRePrint = new PaymentManager().GetPaymentDetail(AppUserHelper.User_ID, AdmissionSemesterReprint);
                    if (paymentRePrint != null)
                        personalInformation.PaymentDetail.Add(paymentRePrint);
                    ViewBag.AdmSemester = AdmissionSemester = AdmissionSemesterReprint;
                    _ADMSemesterAdmissionSettings = studentManager.GetSemesterAdmissionSettings(StudentCourse_ID, AdmissionSemester);
                }
            }

            else
            {
                //reprint
                AdmissionSemesterReprint = studentManager.GetAdmissionSemesterRePrint(personalInformation.CurrentSemesterOrYear);
                if (AdmissionSemesterReprint > 0)
                {
                    ViewBag.AdmSemesterReprint = AdmissionSemesterReprint;
                    PaymentDetails paymentRePrint = new PaymentManager().GetPaymentDetail(AppUserHelper.User_ID, AdmissionSemesterReprint);
                    if (paymentRePrint != null)
                        personalInformation.PaymentDetail.Add(paymentRePrint);
                    ViewBag.AdmSemester = AdmissionSemester = AdmissionSemesterReprint;
                    _ADMSemesterAdmissionSettings = studentManager.GetSemesterAdmissionSettings(StudentCourse_ID, AdmissionSemester);
                }
            }
            ViewBag.SemesterSettings = _ADMSemesterAdmissionSettings;

            //check for self-finance fee
            if (_ADMSemesterAdmissionSettings != null && _ADMSemesterAdmissionSettings.SelfFinanceFee != 0 && _ADMSemesterAdmissionSettings.SFStudentsFormNo.IsNotNullOrEmpty())
            {
                if (_ADMSemesterAdmissionSettings.SFStudentsFormNo.Trim().ToUpper().Replace(" ", "").Contains(personalInformation.StudentFormNo.ToUpper().Trim()))
                {
                    ViewBag.AdmissionFee = ((_ADMSemesterAdmissionSettings?.AdmissionFee ?? 0) + (_ADMSemesterAdmissionSettings.SelfFinanceFee ?? 0)).ToString().Encrypt();
                    _ADMSemesterAdmissionSettings.AdmissionFee = ((_ADMSemesterAdmissionSettings?.AdmissionFee ?? 0) + (_ADMSemesterAdmissionSettings.SelfFinanceFee ?? 0));
                    ViewBag.isSelfFinance = true;
                }
                else
                {
                    ViewBag.AdmissionFee = (_ADMSemesterAdmissionSettings?.AdmissionFee ?? 0).ToString().Encrypt();
                }
            }
            else
            {
                ViewBag.AdmissionFee = (_ADMSemesterAdmissionSettings?.AdmissionFee ?? 0).ToString().Encrypt();
            }


            if (_ADMSemesterAdmissionSettings != null && _ADMSemesterAdmissionSettings.AllowUptoBatch != personalInformation.Batch)
            {
                //check if student is allowed
                if (!studentManager.IsStudentAllowedInSemesterAdm(personalInformation.CUSRegistrationNo + string.Empty, AdmissionSemester))
                {
                    ViewBag.IsBatchAllowed = false;
                }
                else
                {
                    ViewBag.IsBatchAllowed = true;
                }
            }
            else
            {
                ViewBag.IsBatchAllowed = true;
            }

            //allow only specific Regnnos
            if (personalInformation != null && _ADMSemesterAdmissionSettings != null && _ADMSemesterAdmissionSettings.AllowUptoBatch != personalInformation.Batch)
            {
                if (_ADMSemesterAdmissionSettings != null && studentManager != null && (_ADMSemesterAdmissionSettings?.AllowOnlySpecificCUSRegnNos ?? false) &&
                    !(studentManager.IsStudentAllowedInSemesterAdm(personalInformation.CUSRegistrationNo + string.Empty, AdmissionSemester)))
                {
                    AdmissionSemesterReprint = studentManager.GetAdmissionSemesterRePrint(personalInformation.CurrentSemesterOrYear);
                    if (AdmissionSemesterReprint > 0)
                    {
                        ViewBag.AdmSemesterReprint = AdmissionSemesterReprint;
                        PaymentDetails paymentRePrint = new PaymentManager().GetPaymentDetail(AppUserHelper.User_ID, AdmissionSemesterReprint);
                        if (paymentRePrint != null)
                            personalInformation.PaymentDetail.Add(paymentRePrint);
                        ViewBag.AdmSemester = 0;
                    }
                    else
                    {
                        personalInformation = null;
                        ViewBag.IsBatchAllowed = false;
                    }
                }
            }


            // NEP UG FOR 3 & 4 SEM CHECK ONLY
            if (personalInformation != null)
            {
                if (personalInformation.Batch >= 2022 && printProgramme == PrintProgramme.UG
                    && !studentManager.IsStudentAllowedInSemesterAdm(personalInformation.CUSRegistrationNo + string.Empty, AdmissionSemester)
                    && AdmissionSemester == 3)
                {
                    if (personalInformation.CurrentSemesterOrYear != 2)
                    {
                        ViewBag.IsBatchAllowed = false;
                    }
                }
            }
            return View(personalInformation);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ApplySemesterAdm(string command, string ADMFee)
        {
            //Download PayFee
            ARGPersonalInformation personalInformation = new RegistrationManager().GetStudentPersonalInfoOnly(AppUserHelper.User_ID, AppUserHelper.TableSuffix);
            personalInformation.PaymentDetail = new List<PaymentDetails>();


            short AdmissionSemester = 0;
            AdmissionSemester = new StudentManager().GetAdmissionSemester(personalInformation.CurrentSemesterOrYear);

            if (AdmissionSemester > 1)
                personalInformation.Batch = new StudentManager().getStudentSemesterBatch(AppUserHelper.User_ID, AppUserHelper.TableSuffix, AdmissionSemester);

            PaymentDetails payment = new PaymentManager().GetPaymentDetail(AppUserHelper.User_ID, AdmissionSemester);
            if (payment != null)
                personalInformation.PaymentDetail.Add(payment);

            command = Convert.ToString(command).Trim().ToLower();

            if (command == "download")
            {
                return RedirectToAction("SemesterAdmForm", "Student", new { S = AdmissionSemester.ToString().EncryptCookieAndURLSafe(), area = "CUStudentZone" });
            }

            if (command == "payfee" && decimal.TryParse((ADMFee + "").Decrypt(), out decimal TotalFeeToPay))
            {
                if (AdmissionSemester <= 0)
                    return RedirectToAction("ApplySemesterAdm");

                if (personalInformation.AcceptCollege_ID.IsNullOrEmpty())
                {
                    TempData["response"] = errorHtml.Replace("#msg", "Not eligible because Student does not belong to any affiliated College.");
                    return RedirectToAction("ApplySemesterAdm");
                }


                #region check previous pass percentage

                short UptoSemester = 0;
                decimal PassPercentage = 0;
                bool validateOnlyCore = false;

                switch (AdmissionSemester)//Semester in which to admit
                {
                    case 5:
                        UptoSemester = 3;
                        PassPercentage = 50m;
                        validateOnlyCore = false;
                        break;

                    case 7:
                        UptoSemester = 5;
                        PassPercentage = 75m;
                        validateOnlyCore = false;
                        break;

                    default:
                        UptoSemester = 0;
                        break;
                }
                if (UptoSemester > 0)
                {
                    ResponseData _responseData = new ResultManager().PassPercentage(AppUserHelper.TableSuffix, AppUserHelper.User_ID, UptoSemester, PassPercentage, validateOnlyCore);
                    if (!_responseData.IsSuccess)
                    {
                        TempData["response"] = errorHtml.Replace("#msg", _responseData.ErrorMessage);
                        return RedirectToAction("ApplySemesterAdm");
                    }
                }
                #endregion

                if (personalInformation.PaymentDetail.IsNullOrEmpty())
                {
                    //proceed to payment
                    return RedirectToAction("SemAdmissionFee", "Payment", new { S = AdmissionSemester.ToString().EncryptCookieAndURLSafe(), F = (TotalFeeToPay).ToString().EncryptCookieAndURLSafe(), area = "CUStudentZone" });
                }
                else
                {
                    TempData["response"] = errorHtml.Replace("#msg", $"University Semester Admission Fee already received.");
                    return RedirectToAction("ApplySemesterAdm");
                }
            }

            return RedirectToAction("ApplySemesterAdm");
        }

        [HttpGet]
        public ActionResult SemesterAdmForm(string S)
        {
            S = Convert.ToString(S + "").Trim().DecryptCookieAndURLSafe();
            if (!short.TryParse(S, out short AdmSemester))
            {
                TempData["response"] = errorHtml.Replace("#msg", $"Invalid details.");
                return RedirectToAction("ApplySemesterAdm");
            }
            PaymentDetails _PrevPaymentDetails = new PaymentManager().GetPaymentDetail(AppUserHelper.User_ID, AdmSemester);

            if (_PrevPaymentDetails == null)
            {
                TempData["response"] = errorHtml.Replace("#msg", $"Please Sumbit University Semester Admission Fee before Printing/downloading the Form.");
                return RedirectToAction("ApplySemesterAdm");
            }
            Tuple<string, string, List<ADMSubjectMaster>, ARGPersonalInformation, short, string, CombinationSetting> SemesterAdmFormDetails = new StudentManager().GetSemesterAdmissionDetails(AppUserHelper.User_ID, AppUserHelper.TableSuffix, AdmSemester);
            if (SemesterAdmFormDetails.Item4.Student_ID == Guid.Empty && !string.IsNullOrWhiteSpace(SemesterAdmFormDetails.Item1))
            {
                TempData["response"] = errorHtml.Replace("#msg", SemesterAdmFormDetails.Item1);
                return RedirectToAction("ApplySemesterAdm");
            }
            else if (SemesterAdmFormDetails.Item4.Student_ID == Guid.Empty)
            {
                TempData["response"] = errorHtml.Replace("#msg", $"Previous semester detials not found");
                return RedirectToAction("ApplySemesterAdm");
            }
            SemesterAdmFormDetails.Item4.PaymentDetail = new List<PaymentDetails> { _PrevPaymentDetails };


            List<StudentAdditionalSubject> studentAddionalSubjects = new StudentManager().GetStudentAdditionalSubjects(SemesterAdmFormDetails.Item4.Student_ID, AdmSemester, AppUserHelper.TableSuffix);
            ViewBag.StudentAddionalSubjects = studentAddionalSubjects;

            ViewBag.AdmSemester = AdmSemester;

            //------------------- set college account nos ----------------------
            if (SemesterAdmFormDetails.Item4.AcceptCollege_ID == Guid.Parse("8B585643-CEB5-4C96-8328-862A9911CD51")
                && AppUserHelper.OrgPrintProgramme == PrintProgramme.UG)//ASC college
            {
                ViewBag.AccountNo = "0482040500000005";
            }

            ViewBag.SemesterAdmFormDetails = SemesterAdmFormDetails;
            return View();
        }
        #endregion

    }
}