using CaptchaMvc.HtmlHelpers;
using CUSrinagar.BusinessManagers;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using CUSrinagar.Models;
using CUSrinagar.OAuth;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace CUSrinagar.WebApp.CUStudentZone.Controllers
{
    [OAuthorize(AppRoles.Student)]
    public class SelfFinancedController : Controller
    {
        private void SetViewBags()
        {
            var _SelfFinancedManager = new SelfFinancedManager();
            var FormMasterSF = _SelfFinancedManager.GetApplyProgramme();
            ViewBag.PrintProgrammeOption = FormMasterSF != null;
            ViewBag.SFAmount = FormMasterSF?.SelfFinancedApplicationFee ?? 0;
            ViewBag.EligibleCourses = FormMasterSF?.AllowProgrammesInSelfFinance ?? "";
            ViewBag.CoursesApplied = _SelfFinancedManager.GetCoursesApplied();
            if (AppUserHelper.TableSuffix == PrintProgramme.IH)
                ViewBag.CATPoints = new RegistrationManager().GetStudentPersonalInfoOnly(AppUserHelper.User_ID, AppUserHelper.TableSuffix)?.CATEntrancePoints?.ToString() ?? "Not appeared in CAT";
            else
                ViewBag.CATPoints = null;
        }

        [HttpGet]
        public ActionResult Apply()
        {
            SetViewBags();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Apply(List<Guid> CoruseAppied_ID)
        {
            SetViewBags();
            if (CoruseAppied_ID.IsNullOrEmpty())
            {
                TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>Please select atleast one course</a></div>"; ;
                return RedirectToAction("Apply");
            }

            //make payment and redirect to receipt
            Tuple<bool, string, StringBuilder> result = new SelfFinancedManager().Apply(CoruseAppied_ID);
            if (!result.Item1)
            {
                TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>{result.Item2}</a></div>"; ;
                return RedirectToAction("Apply");
            }
            if (result.Item1 && result.Item3 != null)
            {
                System.Web.HttpContext.Current.Response.Clear();
                System.Web.HttpContext.Current.Response.Write(result.Item3.ToString());
                System.Web.HttpContext.Current.Response.End();
                return new EmptyResult();
            }
            else if (result.Item1 && result.Item3 == null)
            {
                TempData["alert"] = $"Applied successfully";
                return RedirectToAction("PrintReceipt", "SelfFinanced", new { area = "CUStudentZone" });
            }
            return RedirectToAction("Apply");
        }

        public ActionResult Success()
        {
            if (Request.InputStream == null)
                return RedirectToAction("Apply", "SelfFinanced", new { area = "CUStudentZone" });

            Tuple<bool, string, PaymentDetails, Guid, Guid> billDeskResponse = new BillDeskManager().BillDeskResponse(Request.InputStream);
            if (billDeskResponse.Item1)
            {
                int result = new PaymentManager().SaveSelfFinancedPaymentDetails(billDeskResponse.Item3, true);
                if (result <= 0)
                {
                    TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>Something went wrong. If your account is debited, Please visit University I.T Section.</a></div>";
                    return RedirectToAction("Apply", "SelfFinanced", new { area = "CUStudentZone" });
                }
                else
                {
                    new SelfFinancedManager().SendPaymentSMSAsync(billDeskResponse.Item3);
                    TempData["alert"] = $"Payment received successfully under TxnReferenceNo. :{billDeskResponse.Item3.TxnReferenceNo} Amount (Rs): {billDeskResponse.Item3.TxnAmount}";
                    return RedirectToAction("PrintReceipt", "SelfFinanced", new { area = "CUStudentZone" });
                }
            }
            else
            {
                TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>{billDeskResponse.Item2}</a></div>";
                return RedirectToAction("Apply", "SelfFinanced", new { area = "CUStudentZone" });
            }
        }

        [HttpGet]
        public ActionResult PrintReceipt()
        {
            ViewBag.AppliedFor = AppUserHelper.OrgPrintProgramme.GetEnumDescription();
            ARGPersonalInformation studentPersonalInfo = new RegistrationManager().GetStudentByID(AppUserHelper.User_ID, AppUserHelper.TableSuffix);

            if (AppUserHelper.OrgPrintProgramme == PrintProgramme.IH && (studentPersonalInfo?.Batch ?? 0) == 2022)
            {
                ARGStudentPreviousQualifications previousQualification = studentPersonalInfo.AcademicDetails.FirstOrDefault(x => x.ExamName.ToLower().Trim() == "12th");
                if (previousQualification != null)
                {
                    studentPersonalInfo.CATEntrancePoints = Convert.ToDecimal((previousQualification.MarksObt / previousQualification.MaxMarks) * 100);
                }
            }
            studentPersonalInfo.CoursesApplied = new SelfFinancedManager().GetCoursesApplied();
            if (studentPersonalInfo.CoursesApplied.All(x => x.SelfFinancedPayment_ID == null))
            {
                TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>Please apply first before printing receipt</a></div>";
                return RedirectToAction("Apply", "SelfFinanced", new { area = "CUStudentZone" });
            }
            foreach (var item in studentPersonalInfo.CoursesApplied.Where(x => x.SelfFinancedPayment_ID != null))
            {
                item.PaymentDetail = new SelfFinancedManager().GetAppliedSelfFinancedPayment((Guid)item.SelfFinancedPayment_ID);
            }
            return View(studentPersonalInfo);
        }
    }
}