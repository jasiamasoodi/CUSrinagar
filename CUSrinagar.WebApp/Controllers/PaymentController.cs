using CUSrinagar.BusinessManagers;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using CUSrinagar.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CUSrinagar.WebApp.Controllers
{
    public class PaymentController : Controller
    {
        #region Admission and billdesk
        [HttpGet]
        [OutputCache(Duration = 0, Location = System.Web.UI.OutputCacheLocation.None, NoStore = true)]
        public ActionResult Index(string id, string R)
        {
            if (Request.UrlReferrer == null)
            {
                TempData["error"] = $"Refresh is not allowed.";
                return RedirectToAction("Instructions", "Registration", new { area = string.Empty });
            }

            if (!Guid.TryParse(id + "", out Guid result) || !Enum.TryParse((R + "").DecryptCookieAndURLSafe(), out PrintProgramme programme))
            {
                TempData["error"] = $"Details are not valid. Please try again.";
                return RedirectToAction("Instructions", "Registration", new { area = string.Empty });
            }

            ARGPersonalInformation aRGPersonalInformation = new RegistrationManager().GetStudentPersonalInfoOnly(result, programme, true);
            if (aRGPersonalInformation == null)
            {
                TempData["error"] = $"Details are not valid. Please try again.";
                return RedirectToAction("Instructions", "Registration", new { area = string.Empty });
            }
            //Make payment by calling BillDesk gateway
            Enum.TryParse((R + "").DecryptCookieAndURLSafe(), out PrintProgramme programme2);
            BillDeskRequest billDeskRequest = new BillDeskRequest
            {
                Email = aRGPersonalInformation.StudentAddress.Email,
                PhoneNo = aRGPersonalInformation.StudentAddress.Mobile,
                ReturnURL = $"Payment/PaymentSuccess",
                PrintProgramme = programme2,
                Entity_ID = result,
                CustomerID = DateTime.UtcNow.Ticks.ToString() + new Random().Next(1, 99),
                TotalFee = aRGPersonalInformation.TotalFee
            };
            StringBuilder sbHTML = new BillDeskManager().GenerateHTMLForm(new BillDeskManager().GenerateRequestString(billDeskRequest, BillDeskPaymentType.OTH));
            System.Web.HttpContext.Current.Response.Clear();
            System.Web.HttpContext.Current.Response.Write(sbHTML.ToString());
            System.Web.HttpContext.Current.Response.End();
            return new EmptyResult();
        }

        public ActionResult PaymentSuccess()
        {
            if (Request.InputStream == null)
                return RedirectToAction("Instructions", "Registration", new { area = string.Empty });

            Tuple<bool, string, PaymentDetails, Guid, Guid> billDeskResponse = new BillDeskManager().BillDeskResponse(Request.InputStream);
            if (billDeskResponse.Item1)
            {
                int result = new PaymentManager().SaveRegistrationPaymentDetails(billDeskResponse.Item3);
                if (result <= 0)
                {
                    TempData["error"] = $"Something went wrong. If your account is debited, Please visit University I.T Section.";
                    return RedirectToAction("Instructions", "Registration", new { area = string.Empty });
                }
                else
                {
                    new PaymentManager().SendPaymentSMSAndEmailAsyc(billDeskResponse.Item3);
                    TempData["alert"] = $"Payment received successfully under TxnReferenceNo. :{billDeskResponse.Item3.TxnReferenceNo} Amount (Rs): {billDeskResponse.Item3.TxnAmount}";
                    return RedirectToAction("Detail", "Registration", new { id = billDeskResponse.Item3.Entity_ID + "/", R = ((int)billDeskResponse.Item3.PrintProgramme).ToString().EncryptCookieAndURLSafe(), area = string.Empty });
                }
            }
            else
            {
                string ErrorMsg = (string.IsNullOrEmpty(billDeskResponse.Item2) || billDeskResponse.Item2.Trim().ToLower() == "na" || billDeskResponse.Item2.Trim().ToLower() == "na - na") ? "Sorry.  We were unable to process your transaction.  We apologise for the inconvenience and request you to try again later." : billDeskResponse.Item2;
                TempData["error"] = ErrorMsg;
                return RedirectToAction("Instructions", "Registration", new { area = string.Empty });
            }
        }



        [HttpGet]
        [OutputCache(Duration = 0, Location = System.Web.UI.OutputCacheLocation.None, NoStore = true)]
        public ActionResult S2S()
        {
            return View();
        }

        #endregion

        #region Migration

        [HttpGet]
        [OutputCache(Duration = 0, Location = System.Web.UI.OutputCacheLocation.None, NoStore = true)]
        public ActionResult Migration(string id, string PP, string FT, string RS, string NC, string TF, string RK)
        {
            if (Request.UrlReferrer == null)
            {
                TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>Refresh is not allowed.</a></div>";
                return RedirectToAction("Index", "Migration", new { area = string.Empty });
            }

            if (!Guid.TryParse(id + "", out Guid Student_ID) || !Enum.TryParse((PP + "").DecryptCookieAndURLSafe(), out PrintProgramme _PrintProgramme))

            {
                TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'> Payment is either closed or will be allowed soon for {AppUserHelper.OrgPrintProgramme.GetEnumDescription()}</a></div>";
                return RedirectToAction("Index", "Migration", new { area = string.Empty });
            }

            ARGPersonalInformation aRGPersonalInformation = new RegistrationManager().GetStudentPersonalInfoOnly(Student_ID, _PrintProgramme, true);
            if (aRGPersonalInformation == null)
            {
                TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'> Details are not valid. Please try again.</a></div>";
                return RedirectToAction("Index", "Migration", new { area = string.Empty });
            }
            Enum.TryParse((PP + "").DecryptCookieAndURLSafe(), out PrintProgramme programme2);
            Enum.TryParse((FT + "").DecryptCookieAndURLSafe(), out MigrationE formType);
            Int32.TryParse((TF + "").DecryptCookieAndURLSafe(), out int totalFee);
            string College = (NC + "").DecryptCookieAndURLSafe().ToString();
            string Remarks = (RK + "").DecryptCookieAndURLSafe().ToString();
            BillDeskRequest billDeskRequest = new BillDeskRequest
            {
                Email = aRGPersonalInformation.StudentAddress.Email,
                PhoneNo = aRGPersonalInformation.StudentAddress.Mobile,
                ReturnURL = $"Payment/MigrationPaySuccess",
                PrintProgramme = programme2,
                Entity_ID = aRGPersonalInformation.Student_ID,
                CustomerID = DateTime.UtcNow.Ticks.ToString(),
                TotalFee = totalFee,
                AdditionalInfo = (short)formType + "@" + College + "@" + Remarks
            };
            StringBuilder sbHTML = new BillDeskManager().GenerateHTMLForm(new BillDeskManager().GenerateRequestString(billDeskRequest, BillDeskPaymentType.OTH));
            System.Web.HttpContext.Current.Response.Clear();
            System.Web.HttpContext.Current.Response.Write(sbHTML.ToString());
            System.Web.HttpContext.Current.Response.End();
            return new EmptyResult();
        }
        public ActionResult MigrationPaySuccess()
        {

            if (Request.InputStream == null)
                return RedirectToAction("Form", "Migration", new { area = string.Empty });

            Tuple<bool, string, PaymentDetails, Guid, Guid> billDeskResponse = new BillDeskManager().BillDeskResponse(Request.InputStream);
            if (billDeskResponse.Item1)
            {
                //save payment and migration details in DB
                int result = new PaymentManager().SaveMigrationPaymentDetails(billDeskResponse.Item3);
                if (result <= 0)
                {
                    TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>Something went wrong. If your account is debited, Please visit University I.T Section.</a></div>";
                    return RedirectToAction("Form", "Migration", new { area = string.Empty });
                }
                else
                {
                    //   DownLoadFormType ftype = billDeskResponse.Item3.for
                    //new PaymentManager().SendPaymentSMSAndEmailAsyc(billDeskResponse.Item3);
                    TempData["alert"] = $"Payment received successfully under TxnReferenceNo. :{billDeskResponse.Item3.TxnReferenceNo} Amount (Rs): {billDeskResponse.Item3.TxnAmount}";
                    return RedirectToAction("Print", "Migration", new { Student_ID = billDeskResponse.Item3.Entity_ID, TypeIs = billDeskResponse.Item3.ModuleType, PP = billDeskResponse.Item3.PrintProgramme });
                    //  return View("/Migration/" + DownLoadFormType.Inter_Migration.ToString().Replace("_", "") + "Print", studentInfo);
                    //  return RedirectToAction("SaveForm", "Migration", new { id = billDeskResponse.Item3.Entity_ID + "/", R = ((int)billDeskResponse.Item3.PrintProgramme).ToString().EncryptCookieAndURLSafe(), area = "" });
                }
            }
            else
            {
                string ErrorMsg = (string.IsNullOrEmpty(billDeskResponse.Item2) || billDeskResponse.Item2.Trim().ToLower() == "na" || billDeskResponse.Item2.Trim().ToLower() == "na - na") ? "Sorry.  We were unable to process your transaction.  We apologise for the inconvenience and request you to try again later." : billDeskResponse.Item2;
                TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>{ErrorMsg}</a></div>";
                return RedirectToAction("Form", "Migration", new { area = string.Empty });
            }
        }
        #endregion

        #region Certificate

        [HttpGet]
        [OutputCache(Duration = 0, Location = System.Web.UI.OutputCacheLocation.None, NoStore = true)]
        public ActionResult AttemptCertificate(string id, string PP, string TF,string CID)
        {
            if (Request.UrlReferrer == null)
            {
                TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>Refresh is not allowed.</a></div>";
                return RedirectToAction("Index", "AttemptCertificate", new { area = string.Empty });
            }

            if (!Guid.TryParse(id + "", out Guid Student_ID) || !Enum.TryParse((PP + "").DecryptCookieAndURLSafe(), out PrintProgramme _PrintProgramme))

            {
                TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'> Payment is either closed or will be allowed soon for {AppUserHelper.OrgPrintProgramme.GetEnumDescription()}</a></div>";
                return RedirectToAction("Index", "AttemptCertificate", new { area = string.Empty });
            }

            ARGPersonalInformation aRGPersonalInformation = new RegistrationManager().GetStudentPersonalInfoOnly(Student_ID, _PrintProgramme, true);
            if (aRGPersonalInformation == null)
            {
                TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'> Details are not valid. Please try again.</a></div>";
                return RedirectToAction("Index", "AttemptCertificate", new { area = string.Empty });
            }
            Enum.TryParse((PP + "").DecryptCookieAndURLSafe(), out PrintProgramme programme2);
           Int32.TryParse((TF + "").DecryptCookieAndURLSafe(), out int totalFee);
            string Certificate_id = (CID + "").DecryptCookieAndURLSafe().ToString();
            BillDeskRequest billDeskRequest = new BillDeskRequest
            {
                Email = aRGPersonalInformation.StudentAddress.Email,
                PhoneNo = aRGPersonalInformation.StudentAddress.Mobile,
                ReturnURL = $"Payment/AttemptCertificatePaySuccess",
                PrintProgramme = programme2,
                Entity_ID = aRGPersonalInformation.Student_ID,
                CustomerID = DateTime.UtcNow.Ticks.ToString(),
                TotalFee = totalFee,
                 AdditionalInfo = Certificate_id + "@"
             };
            StringBuilder sbHTML = new BillDeskManager().GenerateHTMLForm(new BillDeskManager().GenerateRequestString(billDeskRequest, BillDeskPaymentType.OTH));
            System.Web.HttpContext.Current.Response.Clear();
            System.Web.HttpContext.Current.Response.Write(sbHTML.ToString());
            System.Web.HttpContext.Current.Response.End();
            return new EmptyResult();
        }
        public ActionResult AttemptCertificatePaySuccess()
        {

            if (Request.InputStream == null)
                return RedirectToAction("Index", "AttemptCertificate", new { area = string.Empty });

            Tuple<bool, string, PaymentDetails, Guid, Guid> billDeskResponse = new BillDeskManager().BillDeskResponse(Request.InputStream);
            if (billDeskResponse.Item1)
            {
                //save payment and update status in DB
                int result = new PaymentManager().UpdateAttempCertificatePaymentDetails(billDeskResponse.Item3);
                if (result <= 0)
                {
                    TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>Something went wrong. If your account is debited, Please visit University I.T Section.</a></div>";
                    return RedirectToAction("Index", "AttemptCertificate", new { area = string.Empty });
                }
                else
                {
                    string[] sformtypeAI = billDeskResponse.Item3.AdditionalInfo.Split('@');
                    Guid Certificate_ID = Guid.Parse(sformtypeAI[0]);
                    TempData["alert"] = $"Payment received successfully under TxnReferenceNo. :{billDeskResponse.Item3.TxnReferenceNo} Amount (Rs): {billDeskResponse.Item3.TxnAmount}";
                    return RedirectToAction("Print", "AttemptCertificate", new { Certificate_Id = Certificate_ID, Student_ID = billDeskResponse.Item3.Entity_ID, PP = billDeskResponse.Item3.PrintProgramme });
                     }
            }
            else
            {
                string ErrorMsg = (string.IsNullOrEmpty(billDeskResponse.Item2) || billDeskResponse.Item2.Trim().ToLower() == "na" || billDeskResponse.Item2.Trim().ToLower() == "na - na") ? "Sorry.  We were unable to process your transaction.  We apologise for the inconvenience and request you to try again later." : billDeskResponse.Item2;
                TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>{ErrorMsg}</a></div>";
                return RedirectToAction("Index", "AttemptCertificate", new { area = string.Empty });
            }
        }
        #endregion
    }
}