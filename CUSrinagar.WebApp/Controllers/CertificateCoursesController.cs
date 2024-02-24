using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using CaptchaMvc.HtmlHelpers;
using CUSrinagar.BusinessManagers;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using CUSrinagar.Models;

namespace CUSrinagar.WebApp.Controllers
{
    public class CertificateCoursesController : Controller
    {
        #region setViewBags
        string errorMsg = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>##</a></div>";
        private void SetViewBags()
        {
            ViewBag.GenderDDL = Helper.GenderDDL();
            List<SelectListItem> Exambodies = Helper.BoardsDDL();
            Exambodies.Remove(Exambodies.First(x => x.Value == "OTHER"));
            Exambodies.AddRange(Helper.UniversitiesDDL());
            ViewBag.Exambodies = Exambodies;
            ViewBag.CertificateCourses = Helper.GetSelectList<CertificateCourses>();
        }

        private void SetAdmNotifications()
        {
            ViewBag.Options = new List<SelectListItem>
            {
                new SelectListItem{ Text="Re-Print",Value="Re-Print"},
                new SelectListItem{ Text="Make Payment",Value="MakePayment" }
            };
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
            ViewBag.ADMNotifications = ADMNotifications.IsNotNullOrEmpty() ? ADMNotifications.OrderByDescending(x => x.CreatedOn).Take(5).ToList() : null;
        }
        #endregion

        #region Instructions & Reprint

        [HttpGet]
        public ActionResult Instructions()
        {
            SetAdmNotifications();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Instructions(ARGReprint reprint, string Options)
        {
            SetAdmNotifications();

            ModelState.Remove(nameof(reprint.EnteredDOB));
            ModelState.Remove(nameof(reprint.PrintProgrammeOption));
            ViewBag.SelectedTab = "reprint";
            ViewBag.CurrentTab = "PrintTab";

            if (!ModelState.IsValid)
            {
                return View(reprint);
            }
            else
            {
                CertificateCoursePersonalInfo certificateCoursePersonalInfo = null;
                switch (Options)
                {
                    case "Re-Print":
                        certificateCoursePersonalInfo = new CertificateCoursesManager().GetItem(reprint.FormNo, reprint.Batch);
                        if (certificateCoursePersonalInfo == null)
                        {
                            TempData["response"] = errorMsg.Replace("##", "No details found.");
                            return View();
                        }
                        else
                            return RedirectToAction("PrintPreview", new { id = certificateCoursePersonalInfo.Student_ID });
                    case "MakePayment":
                        certificateCoursePersonalInfo = new CertificateCoursesManager().GetItem(reprint.FormNo, reprint.Batch);
                        if (certificateCoursePersonalInfo == null)
                        {
                            TempData["response"] = errorMsg.Replace("##", "No details found.");
                            return View();
                        }
                        else
                            return RedirectToAction("DoPayment", new { id = certificateCoursePersonalInfo.Student_ID });
                    default:
                        TempData["response"] = errorMsg.Replace("##", "Invalid details");
                        return View();
                }
            }
        }

        #endregion

        #region Apply Fresh
        [HttpGet]
        public ActionResult Apply()
        {
            SetViewBags();
            CertificateCoursePersonalInfo certificateCoursePersonalInfo = new CertificateCoursePersonalInfo
            {
                PrevQualifications = new List<CertificateCoursePrevQualifications>
                {
                     new CertificateCoursePrevQualifications {
                          ExamName="12TH",ReadOnly=true
                     }
                }
            };

            return View(certificateCoursePersonalInfo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Apply(CertificateCoursePersonalInfo certificateCoursePersonalInfo, FormCollection formCollection)
        {
            SetViewBags();

            if ((!this?.IsCaptchaValid("Captcha is not valid") ?? true))
            {
                TempData["response"] = errorMsg.Replace("##", "Invalid Verification Code, please try again.");
                return RedirectToAction("Instructions");
            }

            FormCollection _formCollection = new DynamicControls().FillModel(formCollection, nameof(certificateCoursePersonalInfo.PrevQualifications));
            if (_formCollection != null)
            {
                certificateCoursePersonalInfo.PrevQualifications = new List<CertificateCoursePrevQualifications>();
                TryUpdateModel(certificateCoursePersonalInfo, _formCollection.ToValueProvider());
            }

            certificateCoursePersonalInfo.TotalFee = Convert.ToInt32(Convert.ToString(formCollection["ToTalFees"]).DecryptCookieAndURLSafe() ?? "0");
            if (!ModelState.IsValid
                || certificateCoursePersonalInfo.TotalFee <= 0
                || ((List<SelectListItem>)ViewBag.CertificateCourses).All(x => x.Value != ((int)certificateCoursePersonalInfo.AppliedFor).ToString()))
            {
                TempData["response"] = errorMsg.Replace("##", "Invalid Data submitted. Please check your details and try again.");
                return RedirectToAction("Instructions");
            }

            Tuple<bool, string, Guid> response = new CertificateCoursesManager().Save(certificateCoursePersonalInfo);
            if (response.Item1)
            {
                return RedirectToAction("DoPayment", new { id = response.Item3 });
            }
            else
            {
                TempData["response"] = errorMsg.Replace("##", response.Item2);
                return RedirectToAction("Instructions");
            }
        }
        #endregion

        #region Payments

        [HttpGet]
        public ActionResult DoPayment(string id)
        {
            if (!Guid.TryParse(id + "", out Guid Student_ID))
            {
                TempData["response"] = errorMsg.Replace("##", "Invalid data submitted, please try again.");
                return RedirectToAction("Instructions");
            }

            CertificateCoursePersonalInfo certificateCoursePersonalInfo = new CertificateCoursesManager().GetItem(Student_ID);
            if (certificateCoursePersonalInfo == null)
            {
                TempData["response"] = errorMsg.Replace("##", "Invalid data submitted, please try again.");
                return RedirectToAction("Instructions");
            }
            if (certificateCoursePersonalInfo.FormStatus != FormStatus.InProcess)
            {
                TempData["response"] = errorMsg.Replace("##", "Payment already received. Indeed you can reprint your form.");
                return RedirectToAction("Instructions");
            }

            BillDeskRequest billDeskRequest = new BillDeskRequest
            {
                Email = certificateCoursePersonalInfo.Email,
                PhoneNo = certificateCoursePersonalInfo.MobileNo,
                ReturnURL = $"CertificateCourses/PaymentSuccess",
                PrintProgramme = PrintProgramme.IH,
                Entity_ID = certificateCoursePersonalInfo.Student_ID,
                CustomerID = DateTime.UtcNow.Ticks.ToString() + new Random().Next(1, 99),
                TotalFee = certificateCoursePersonalInfo.TotalFee,
                AdditionalInfo = certificateCoursePersonalInfo.FullName.Replace(".", "")
            };
            StringBuilder builder = new BillDeskManager().GenerateHTMLForm(new BillDeskManager().GenerateRequestString(billDeskRequest, BillDeskPaymentType.OTH));
            System.Web.HttpContext.Current.Response.Clear();
            System.Web.HttpContext.Current.Response.Write(builder.ToString());
            System.Web.HttpContext.Current.Response.End();
            return new EmptyResult();
        }

        public ActionResult PaymentSuccess()
        {
            if (Request.InputStream == null)
                return RedirectToAction("Instructions");

            Tuple<bool, string, PaymentDetails, Guid, Guid> billDeskResponse = new BillDeskManager().BillDeskResponse(Request.InputStream);
            if (billDeskResponse.Item1)
            {
                int result = new PaymentManager().SavePaymentForCertificateCourses(billDeskResponse.Item3);
                if (result <= 0)
                {
                    TempData["response"] = errorMsg.Replace("##", "Something went wrong. If your account is debited, Please visit University I.T Section.");
                    return RedirectToAction("Instructions");
                }
                else
                {
                    TempData["alert"] = $"Payment received successfully. Under Name : {billDeskResponse.Item3.AdditionalInfo} TxnRefNo: {billDeskResponse.Item3.TxnReferenceNo} Amount: {billDeskResponse.Item3.TxnAmount} dated: {billDeskResponse.Item3.TxnDate}";
                    return RedirectToAction("PrintPreview", new { id = billDeskResponse.Item3.Entity_ID });
                }
            }
            else
            {
                TempData["response"] = errorMsg.Replace("##", billDeskResponse.Item2);
                return RedirectToAction("Instructions");
            }
        }

        #endregion

        #region Prints

        [HttpGet]
        public ActionResult PrintPreview(string id)
        {
            if (!Guid.TryParse(id + "", out Guid Student_ID))
            {
                TempData["response"] = errorMsg.Replace("##", "Invalid data submitted, please try again.");
                return RedirectToAction("Instructions");
            }

            CertificateCoursePersonalInfo certificateCoursePersonalInfo = new CertificateCoursesManager().GetItem(Student_ID, false);
            if (certificateCoursePersonalInfo == null)
            {
                TempData["response"] = errorMsg.Replace("##", "Invalid data submitted, please try again.");
                return RedirectToAction("Instructions");
            }
            if (certificateCoursePersonalInfo.FormStatus == FormStatus.InProcess)
            {
                TempData["response"] = errorMsg.Replace("##", "Please make online Fee payment before print your form.");
                return RedirectToAction("Instructions");
            }
            return View(certificateCoursePersonalInfo);
        }

        #endregion

        #region Remote Validation
        public JsonResult ValidateBoardRegnNoExists(string BoardRegnNo, short Batch)
        {
            if (string.IsNullOrWhiteSpace(BoardRegnNo))
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            string FirstChar = BoardRegnNo?.Substring(0, 1) + string.Empty;
            string LastChar = BoardRegnNo.Substring(BoardRegnNo.Length - 1, 1) + string.Empty;

            if (!(new Regex("^[a-zA-Z0-9]*$").IsMatch(FirstChar)) || !(new Regex("^[a-zA-Z0-9]*$").IsMatch(LastChar)))
            {
                return Json("Invalid Board Regn. No.", JsonRequestBehavior.AllowGet);
            }
            return Json(!new CertificateCoursesManager().ValidateBoardRegnNoExists(BoardRegnNo, Batch), JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}