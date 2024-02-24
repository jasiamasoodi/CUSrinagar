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

namespace CUSrinagar.WebApp.CUStudentZone.Controllers
{
    [OAuthorize(AppRoles.Student)]
    public class ReEvaluationController : StudentBaseController
    {
        #region ReEvaluation
        [HttpGet]
        public ActionResult ReEvaluation()
        {
            var list = new ReEvaluationSettingManager().List(AppUserHelper.OrgPrintProgramme, true);
            ViewBag.ReEvaluationSettings = list;
            ViewBag.CurrentSemester = new StudentManager().GetStudentCurrentSemester(AppUserHelper.User_ID, AppUserHelper.TableSuffix);
            GetResponseViewBags();
            return View();
        }

        [HttpGet]
        public ActionResult ReEvaluationForm(short? ApplyForSemester, short? SubmittedYear)
        {
            if (!short.TryParse(ApplyForSemester + "".ToString(), out short ApplyForSemesterVal) || !short.TryParse(SubmittedYear + "".ToString(), out short SubmittedYearVal))
                return RedirectToAction("ReEvaluation");

            var response = Validate(ApplyForSemesterVal, SubmittedYearVal, true);
            if (!response.IsSuccess)
            {
                SetResponseViewBags(response);
                GetResponseViewBags();
                return View();
            }
            ResultCompact studentResult = (ResultCompact)response.ResponseObject;
            List<ReEvaluationPayment> ReEvaluationsForStudent = new ReEvaluationManager().GetReEvaluationsForStudent(studentResult.Student_ID, ApplyForSemesterVal, SubmittedYearVal);
            if (ReEvaluationsForStudent != null)
            {
                List<ReEvaluationStudentSubject> ReEvaluatedSubjects = new List<ReEvaluationStudentSubject>();
                foreach (var rev in ReEvaluationsForStudent)
                {
                    if (rev.SubjectsForEvaluation != null)
                        ReEvaluatedSubjects.AddRange(rev.SubjectsForEvaluation);
                }
                ViewBag.ReEvaluatedSubjects = ReEvaluatedSubjects;
            }
            SetViewBags(ApplyForSemester.Value, SubmittedYear.Value);
            GetResponseViewBags();
            return View(studentResult);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ReEvaluationForm(ReEvaluation Model, string Email, string MobileNumber, short ApplyForSemester, short SubmittedYear)
        {
            ResponseData response = Validate(ApplyForSemester, SubmittedYear, true, false, Email, MobileNumber);
            if (!response.IsSuccess)
            {
                SetResponseViewBags(response); GetResponseViewBags();
                return RedirectToAction("ReEvaluationForm", new { ApplyForSemester, SubmittedYear });
            }

            response = new ReEvaluationManager().AddEvaluationForm(Model, SubmittedYear);
            if (!response.IsSuccess)
            {
                SetResponseViewBags(new ResponseData() { ErrorMessage = string.IsNullOrEmpty(response.ErrorMessage) ? "Something went wrong. Please try again later" : response.ErrorMessage });
                return RedirectToAction("ReEvaluationForm", new { ApplyForSemester, SubmittedYear });
            }
            var reEvaluation = (ReEvaluation)response?.ResponseObject;
            if (response.IsSuccess)
            {
                BillDeskRequest billDeskRequest = new BillDeskRequest()
                {
                    Entity_ID = Model.ReEvaluation_ID,
                    Email = Email,
                    PhoneNo = MobileNumber,
                    CustomerID = DateTime.Now.Ticks.ToString(),
                    AdditionalInfo = reEvaluation.FormNumber,
                    TotalFee = (int)reEvaluation.FeeAmount,
                    PrintProgramme = AppUserHelper.TableSuffix,
                    Student_ID = reEvaluation.Student_ID,
                    ReturnURL = $"CUStudentZone/ReEvaluation/ReEvaluationPaymentResponse?ApplyForSemester={ApplyForSemester}&SubmittedYear={SubmittedYear}",
                    Semester = Model.Semester.ToString()
                };
                var request = new BillDeskManager().GenerateRequestString(billDeskRequest, BillDeskPaymentType.EXM);
                var htmlForm = new BillDeskManager().GenerateHTMLForm(request);

                System.Web.HttpContext.Current.Response.Clear();
                System.Web.HttpContext.Current.Response.Write(htmlForm.ToString());
                System.Web.HttpContext.Current.Response.End();
                return new EmptyResult();
            }
            else
            {
                response.IsSuccess = false;
                response.ErrorMessage = string.IsNullOrWhiteSpace(response.ErrorMessage) ? " Something went Wrong. Please contact I.T Section Cluster University ,  , Srinagar." : response.ErrorMessage;
            }
            SetResponseViewBags(response);

            return RedirectToAction("ReEvaluationForm", new { ApplyForSemester, SubmittedYear });
        }

        public ActionResult ReEvaluationPaymentResponse(string ApplyForSemester, string SubmittedYear)
        {
            ResponseData response = new ResponseData();
            System.Web.HttpContext.Current.Response.Cache.SetNoStore();
            Tuple<bool, string, PaymentDetails, Guid, Guid> billdeskResponse = new BillDeskManager().BillDeskResponse(Request.InputStream);

            short _ApplyForSemester = short.TryParse(ApplyForSemester + "", out short _s) ? _s : billdeskResponse.Item3.Semester.Value;
            short _SubmittedYear = short.TryParse(SubmittedYear + "", out short sy) ? sy : billdeskResponse.Item3.Semester.Value;

            response = new PaymentManager().SavePayment(billdeskResponse, AppUserHelper.TableSuffix, billdeskResponse.Item1, PaymentModuleType.ReEvaluation);
            if (!response.IsSuccess)
            {
                ViewBag.PaymentFailureMessage = "Information: In-case Amount is not deducted try to enable E-COM in your card using your bank App or ATM like M-Pay, or try to use other methods of online banking";
                return View();
            }
            return RedirectToAction("PrintReEvaluationForm", "ReEvaluation", new { @area = "CUStudentZone", @ApplyForSemester = _ApplyForSemester, _SubmittedYear, @SubmittedYear = _SubmittedYear });
        }

        public ActionResult PrintReEvaluationForm(short? ApplyForSemester, short? SubmittedYear)
        {
            StudentProfile result = new ResultManager().GetResultByStudentRegistrationNumber(ApplyForSemester ?? 0);
            if (result == null)
            {
                SetResponseViewBags(new ResponseData() { ErrorMessage = " Result not found. Please contact I.T Section Cluster University Srinagar." });
                return RedirectToAction("ReEvaluation");
            }

            List<ReEvaluationPayment> ReEvaluationsForStudent = new ReEvaluationManager().GetReEvaluationsForStudent(AppUserHelper.User_ID, ApplyForSemester ?? 0, SubmittedYear ?? 0);
            if (ReEvaluationsForStudent != null)
            {
                List<PaymentDetails> paymentDetailsDB = new List<PaymentDetails>();
                List<ReEvaluationStudentSubject> ReEvaluatedSubjects = new List<ReEvaluationStudentSubject>();

                foreach (var rev in ReEvaluationsForStudent)
                {
                    if (rev.SubjectsForEvaluation != null)
                        ReEvaluatedSubjects.AddRange(rev.SubjectsForEvaluation);
                    var pa = new PaymentManager().GetPaymentDetails(rev.ReEvaluation_ID, PaymentModuleType.ReEvaluation, AppUserHelper.TableSuffix);

                    if (pa != null)
                        paymentDetailsDB.AddRange(pa);
                }

                if (paymentDetailsDB.IsNotNullOrEmpty() && ReEvaluatedSubjects.IsNotNullOrEmpty())
                {
                    ViewBag.ReEvaluatedSubjects = ReEvaluatedSubjects;
                    ViewBag.Semester = paymentDetailsDB.FirstOrDefault().Semester;
                    ViewBag.PaymentDetails = paymentDetailsDB;
                    return View(result);
                }
            }
            SetResponseViewBags(new ResponseData() { ErrorMessage = $" Not applied for any reEvaluation for semester-{ApplyForSemester} for Year {SubmittedYear}." });
            return RedirectToAction("ReEvaluation");
        }
        #endregion

        #region Xerox
        [HttpGet]
        public ActionResult Xerox()
        {
            var list = new ReEvaluationSettingManager().List(AppUserHelper.OrgPrintProgramme, false);
            ViewBag.ReEvaluationSettings = list;
            GetResponseViewBags();
            return View();
        }

        [HttpGet]
        public ActionResult XeroxForm(short? ApplyForSemester, short? SubmittedYear)
        {
            var response = Validate(ApplyForSemester ?? 0, SubmittedYear ?? 0, false);
            if (!response.IsSuccess)
            {
                SetResponseViewBags(response);
                return RedirectToAction("Xerox");
            }
            ResultCompact studentResult = (ResultCompact)response.ResponseObject;
            var ReEvaluationsForStudent = new ReEvaluationManager().GetReEvaluationsForStudent(studentResult.Student_ID, ApplyForSemester ?? 0, SubmittedYear ?? 0);
            if (ReEvaluationsForStudent != null)
            {
                List<ReEvaluationStudentSubject> ReEvaluatedSubjects = new List<ReEvaluationStudentSubject>();
                foreach (var rev in ReEvaluationsForStudent)
                {
                    if (rev.SubjectsForEvaluation != null)
                        ReEvaluatedSubjects.AddRange(rev.SubjectsForEvaluation);
                }
                ViewBag.ReEvaluatedSubjects = ReEvaluatedSubjects;
            }
            SetViewBags(ApplyForSemester ?? 0, SubmittedYear ?? 0);
            GetResponseViewBags();
            return View(studentResult);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult XeroxForm(ReEvaluation Model, string Email, string MobileNumber, short ApplyForSemester, short SubmittedYear)
        {
            ResponseData response = Validate(ApplyForSemester, SubmittedYear, false, true, Email, MobileNumber);
            if (!response.IsSuccess)
            {
                SetResponseViewBags(response); GetResponseViewBags();
                return RedirectToAction("XeroxForm", new { ApplyForSemester, SubmittedYear });
            }
            response = new ReEvaluationManager().AddEvaluationForm(Model, SubmittedYear);
            if (!response.IsSuccess)
            {
                SetResponseViewBags(new ResponseData() { ErrorMessage = string.IsNullOrEmpty(response.ErrorMessage) ? "Something went wrong. Please try again later" : response.ErrorMessage });
                return RedirectToAction("XeroxForm", new { ApplyForSemester, SubmittedYear });
            }

            var reEvaluation = (ReEvaluation)response?.ResponseObject;
            if (response.IsSuccess)
            {
                BillDeskRequest billDeskRequest = new BillDeskRequest()
                {
                    Entity_ID = Model.ReEvaluation_ID,
                    Email = Email,
                    PhoneNo = MobileNumber,
                    CustomerID = DateTime.Now.Ticks.ToString(),
                    AdditionalInfo = reEvaluation.FormNumber,
                    TotalFee = (int)reEvaluation.FeeAmount,
                    PrintProgramme = AppUserHelper.TableSuffix,
                    Student_ID = reEvaluation.Student_ID,
                    ReturnURL = $"CUStudentZone/ReEvaluation/XeroxPaymentResponse?ApplyForSemester={ApplyForSemester}&SubmittedYear={SubmittedYear}",
                    Semester = Model.Semester.ToString()
                };
                var request = new BillDeskManager().GenerateRequestString(billDeskRequest, BillDeskPaymentType.EXM);
                var htmlForm = new BillDeskManager().GenerateHTMLForm(request);

                System.Web.HttpContext.Current.Response.Clear();
                System.Web.HttpContext.Current.Response.Write(htmlForm.ToString());
                System.Web.HttpContext.Current.Response.End();
                return new EmptyResult();
            }
            else
            {
                response.IsSuccess = false;
                response.ErrorMessage = string.IsNullOrWhiteSpace(response.ErrorMessage) ? " Something went Wrong.Please contact I.T Section Cluster University ,  , Srinagar." : response.ErrorMessage;
            }
            SetResponseViewBags(response);
            return RedirectToAction("XeroxForm", new { ApplyForSemester = Model.Semester });
        }

        public ActionResult XeroxPaymentResponse(string ApplyForSemester, string SubmittedYear)
        {
            ResponseData response = new ResponseData();
            System.Web.HttpContext.Current.Response.Cache.SetNoStore();
            Tuple<bool, string, PaymentDetails, Guid, Guid> billdeskResponse = new BillDeskManager().BillDeskResponse(Request.InputStream);

            short _ApplyForSemester = (!string.IsNullOrEmpty(ApplyForSemester) && short.TryParse(ApplyForSemester + "", out short _s)) ? short.Parse(ApplyForSemester) : billdeskResponse.Item3.Semester.Value;
            short _SubmittedYear = (!string.IsNullOrEmpty(SubmittedYear) && short.TryParse(SubmittedYear + "", out short _y)) ? short.Parse(SubmittedYear) : billdeskResponse.Item3.Semester.Value;

            response = new PaymentManager().SavePayment(billdeskResponse, AppUserHelper.TableSuffix, billdeskResponse.Item1, PaymentModuleType.Xerox);
            if (!response.IsSuccess)
            {
                ViewBag.PaymentFailureMessage = "Information: In-case Amount is not deducted try to enable E-COM in your card using your bank App or ATM like M-Pay, or try to use other methods of online banking";
                return View();
            }
            return RedirectToAction("PrintXeroxForm", "ReEvaluation", new { @area = "CUStudentZone", @ApplyForSemester = _ApplyForSemester, _SubmittedYear, @SubmittedYear = _SubmittedYear });
        }
        public ActionResult PrintXeroxForm(short? ApplyForSemester, short? SubmittedYear)
        {
            StudentProfile result = new ResultManager().GetResultByStudentRegistrationNumber(ApplyForSemester ?? 0);

            if (result == null)
            {
                SetResponseViewBags(new ResponseData() { ErrorMessage = " Student not found. Please contact I.T Section Cluster University Srinagar." });
                return RedirectToAction("Xerox");
            }

            List<ReEvaluationPayment> ReEvaluationsForStudent = new ReEvaluationManager().GetReEvaluationsForStudent(AppUserHelper.User_ID, ApplyForSemester ?? 0, SubmittedYear ?? 0);
            if (ReEvaluationsForStudent != null)
            {
                List<PaymentDetails> paymentDetailsDB = new List<PaymentDetails>();
                List<ReEvaluationStudentSubject> ReEvaluatedSubjects = new List<ReEvaluationStudentSubject>();

                foreach (var rev in ReEvaluationsForStudent)
                {
                    if (rev.SubjectsForEvaluation != null)
                        ReEvaluatedSubjects.AddRange(rev.SubjectsForEvaluation);
                    var pa = new PaymentManager().GetPaymentDetails(rev.ReEvaluation_ID, PaymentModuleType.Xerox, AppUserHelper.TableSuffix);

                    if (pa != null)
                        paymentDetailsDB.AddRange(pa);
                }
                if (paymentDetailsDB.IsNotNullOrEmpty() && ReEvaluatedSubjects.IsNotNullOrEmpty())
                {
                    ViewBag.ReEvaluatedSubjects = ReEvaluatedSubjects;
                    ViewBag.Semester = ReEvaluationsForStudent.First().Semester;
                    ViewBag.PaymentDetails = paymentDetailsDB;
                    return View(result);
                }
            }
            SetResponseViewBags(new ResponseData() { ErrorMessage = $" Not applied for any xerox for semester-{ApplyForSemester} for Year {SubmittedYear}." });
            return RedirectToAction("Xerox");
        }
        #endregion

        ResponseData Validate(short ApplyForSemester, short SubmittedYear, bool IsReEvaluation, bool payment = false, string Email = null, string MobileNumber = null)
        {
            var list = new ReEvaluationSettingManager().List(AppUserHelper.OrgPrintProgramme, IsReEvaluation, SubmittedYear, true);
            if (list.IsNullOrEmpty() || !list.Any(x => x.Semester == ApplyForSemester))
            {
                return new ResponseData() { ErrorMessage = $"{(IsReEvaluation ? "Re-Evaluation" : "Xerox")} process hasn't started yet or the date for Application has ended." };
            }
            if (list.Where(x => x.Semester == ApplyForSemester).Count() > 1)
            {
                return new ResponseData() { ErrorMessage = "There is some discrepancy. Please contact I.T Section" };
            }
            list = list.Where(x => x.Semester == ApplyForSemester).ToList();
            if (list.First().ValidateByExaminationForm)
            {
                bool hasExamForms = new ExaminationFormManager().Get(AppUserHelper.OrgPrintProgramme, AppUserHelper.User_ID, ApplyForSemester, SubmittedYear);
                if (!hasExamForms)
                {
                    return new ResponseData() { ErrorMessage = $"Examination form not available in semester-{ApplyForSemester} for Year-{SubmittedYear}" };
                }
            }

            ResultCompact studentResult = new ResultManager().GetResult(AppUserHelper.OrgPrintProgramme, ApplyForSemester, AppUserHelper.User_ID, false);
            if (studentResult == null || studentResult.SubjectResults.IsNullOrEmpty())
            {
                return new ResponseData() { ErrorMessage = "Result not found." };
            }
            if (list.First().ValidateByResultNotificationIDs)
            {
                if (list.First().ResultNotification_IDs.IsNotNullOrEmpty())
                {
                    studentResult.SubjectResults = studentResult.SubjectResults.Where(x => x.ResultNotification_ID.HasValue && list.First().ResultNotification_IDs.Contains(x.ResultNotification_ID.Value.ToString()))?.ToList();
                    if (studentResult.SubjectResults.IsNullOrEmpty())
                    {
                        return new ResponseData() { ErrorMessage = "Result not found against current notification." };
                    }
                }
            }
            if (payment)
            {
                if (Email.IsNullOrEmpty() || MobileNumber.IsNullOrEmpty())
                {
                    return new ResponseData() { ErrorMessage = "invalid details, Email Or Mobile Number not provided." };
                }
            }
            return new ResponseData() { IsSuccess = true, ResponseObject = studentResult };
        }
        void SetViewBags(short ApplyForSemester, short SubmittedYear)
        {
            ViewBag.ApplyForSemester = ApplyForSemester;
            ViewBag.SubmittedYear = SubmittedYear;
            var std = new StudentManager().GetStudentFixed(AppUserHelper.User_ID, AppUserHelper.OrgPrintProgramme, true);
            ViewBag.Email = std.StudentAddress.Email;
            ViewBag.Mobile = std.StudentAddress.Mobile;
        }
    }
}