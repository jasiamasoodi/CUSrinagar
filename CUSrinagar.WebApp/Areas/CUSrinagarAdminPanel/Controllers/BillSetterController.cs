using BitMiracle.LibTiff.Classic;
using CUSrinagar.BusinessManagers;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using CUSrinagar.Models;
using CUSrinagar.OAuth;
using GeneralModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace CUSrinagar.WebApp.CUSrinagarAdminPanel.Controllers
{
    [OAuthorize(AppRoles.PaperSetterBills)]
    public class BillSetterController : Controller
    {
        #region Setter Bills
        [HttpGet]
        public ActionResult Bills()
        {
            ViewBag.BillType = BillType.PaperSetterBill;

            #region for Statistics

            Parameters parameters = new Parameters
            {
                Filters = new List<SearchFilter>()
            };
            parameters?.Filters?.Add(new SearchFilter
            {
                Column = "BillType",
                Value = ((short)BillType.PaperSetterBill).ToString()
            });

            parameters?.Filters?.Add(new SearchFilter
            {
                Column = "User_ID",
                Value = AppUserHelper.User_ID.ToString()
            });

            ViewBag.BillStatistics = new BillingManager().GetBillStatistics(parameters) ?? new List<BillStatistics>();
            #endregion

            ViewBag.BillStatus = Helper.GetSelectList(BillStatus.InProcess, BillStatus.ExternalAwardsUploaded);
            return View();
        }

        [HttpPost]
        public PartialViewResult SetterList(Parameters parameter)
        {
            parameter?.Filters?.Add(new SearchFilter
            {
                Column = "BillType",
                TableAlias = "bd",
                Value = ((short)BillType.PaperSetterBill).ToString()
            });

            parameter?.Filters?.Add(new SearchFilter
            {
                Column = "User_ID",
                TableAlias = "bd",
                Value = AppUserHelper.User_ID.ToString()
            });

            List<BIL_BillDetails> list = new BillingManager().GetBills(parameter);

            ViewBag.BillStatusPartial = new List<SelectListItem>();

            if (list.IsNotNullOrEmpty())
            {
                if (list.All(x => x.BillStatus == BillStatus.UnderVerification || x.BillStatus == BillStatus.Verified || x.BillStatus == BillStatus.DispatchedToAccountsSection || x.BillStatus == BillStatus.Paid))
                { }
                else
                {
                    if (list.All(x => x.BillStatus == BillStatus.Assigned) || list.All(x => x.BillStatus == BillStatus.Rejected))
                    {
                        ViewBag.BillStatusPartial = Helper.GetSelectList(BillStatus.ExternalAwardsUploaded, BillStatus.Paid,
                            BillStatus.ErrorInBill, BillStatus.UnderVerification, BillStatus.Verified, BillStatus.InProcess, BillStatus.Assigned, BillStatus.DispatchedToAccountsSection, BillStatus.EmailSent);
                    }
                    else
                    {
                        ViewBag.BillStatusPartial = Helper.GetSelectList(BillStatus.ExternalAwardsUploaded, BillStatus.Paid,
                           BillStatus.ErrorInBill, BillStatus.UnderVerification, BillStatus.Verified, BillStatus.InProcess, BillStatus.Assigned, BillStatus.DispatchedToAccountsSection, BillStatus.Rejected, BillStatus.Accepted);
                    }
                }
            }
            return PartialView(list);
        }


        [HttpPost]
        public ActionResult ChangeBillStatus(Parameters parameter, string BillStatus)
        {
            parameter?.Filters?.Add(new SearchFilter
            {
                Column = "BillType",
                TableAlias = "bd",
                Value = ((short)BillType.PaperSetterBill).ToString()
            });

            Guid user_ID = AppUserHelper.User_ID;
            parameter?.Filters?.Add(new SearchFilter
            {
                Column = "User_ID",
                TableAlias = "bd",
                Value = user_ID.ToString()
            });

            BillStatus billStatus = (BillStatus)Enum.Parse(typeof(BillStatus), BillStatus);

            if (billStatus == Enums.BillStatus.Accepted)
            {
                BIL_BankAccountDetails bankAccountDetails = new BillingManager().GetBankAccountByUserID(user_ID);
                if (bankAccountDetails == null)
                    return Json(new { status = false, msg = "Bank Account Details not mentioned yet. Please enter your Bank Account Details by going to option Bank Account in the Menu" }, JsonRequestBehavior.DenyGet);
            }

            var response = new BillingManager().BulkUpdateBillStatus(parameter, billStatus);
            return Json(new { status = response.Item1, msg = response.Item2 }, JsonRequestBehavior.DenyGet);
        }

        [HttpGet]
        public ActionResult Certificate(string r)
        {
            List<BIL_BillDetails> bIL_BillDetails = new BillingManager().GetBill((r ?? "").DecryptCookieAndURLSafe());

            if (bIL_BillDetails != null)
            {
                var commingForURL = Request.UrlReferrer;
                if (commingForURL == null)
                {
                    return RedirectToAction("Index", "Dashboard");
                }
                if (commingForURL?.AbsolutePath?.ToLower()?.Contains("/billsetter/") ?? false)
                {
                    if (bIL_BillDetails.First().User_ID == AppUserHelper.User_ID)
                    {
                        return View("Certificate", bIL_BillDetails);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Dashboard");
                    }
                }
                else
                {
                    return View("Certificate", bIL_BillDetails);
                }
            }
            else
                return RedirectToAction("Index", "Dashboard");
        }


        [HttpGet]
        public ActionResult AppointmentLetter(string r)
        {
            List<BIL_BillDetails> bIL_BillDetails = new BillingManager().GetBill((r ?? "").DecryptCookieAndURLSafe());

            if (bIL_BillDetails != null)
            {
                var commingForURL = Request.UrlReferrer;
                if (commingForURL == null)
                {
                    return RedirectToAction("Index", "Dashboard");
                }
                if (commingForURL?.AbsolutePath?.ToLower()?.Contains("/billsetter/") ?? false)
                {
                    if (bIL_BillDetails.First().User_ID == AppUserHelper.User_ID)
                    {
                        return View("AppointmentLetter", bIL_BillDetails);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Dashboard");
                    }
                }
                else
                {
                    return View("AppointmentLetter", bIL_BillDetails);
                }
            }
            else
                return RedirectToAction("Index", "Dashboard");
        }

        [ChildActionOnly]
        public string GetPaperSetting(Guid d)
        {
            BIL_PaperSetterSettings paperSetterSettings = new BillingManager().GetPaperSetterSetting(d);
            return paperSetterSettings != null
                ? $@"<a href=""{paperSetterSettings.FilePath}"" target=""_blank"">{paperSetterSettings.Title}</a>"
                : "";
        }
        #endregion


    }
}