using CUSrinagar.BusinessManagers;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using CUSrinagar.Models;
using CUSrinagar.OAuth;
using GeneralModels;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CUSrinagar.WebApp.CUSrinagarAdminPanel.Controllers
{
    [OAuthorize]
    public class BillAccountController : Controller
    {
        string errorMsg = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>##</a></div>";
        #region AccountDetails 

        [HttpGet]
        [OAuthorize(AppRoles.PaperSetterBills, AppRoles.EvaluatorBills)]
        public ActionResult BillAccountDetails()
        {
            BIL_BankAccountDetails bIL_BankAccount = new BillingManager().GetBankAccountByUserID(AppUserHelper.User_ID);
            return View(bIL_BankAccount);
        }

        [HttpPost]
        [OAuthorize(AppRoles.PaperSetterBills, AppRoles.EvaluatorBills)]
        public ActionResult BillAccountDetails(BIL_BankAccountDetails bil_bankaccount)
        {
            if (ModelState.IsValid)
            {
                bil_bankaccount.User_ID = AppUserHelper.User_ID;
                Tuple<bool, string> _bankaccount = new BillingManager().SaveUpdateBankAccount(bil_bankaccount);

                TempData["Err"] = _bankaccount.Item1
                    ? errorMsg.Replace("##", _bankaccount.Item2).Replace("alert-danger", "alert-success")
                    : errorMsg.Replace("##", _bankaccount.Item2);
            }

            return View();
        }

        #endregion

        #region Bills
        [HttpGet]
        [OAuthorize(AppRoles.PaperSetterBills, AppRoles.EvaluatorBills, AppRoles.University_Bills)]
        public ActionResult ViewBill(string r)
        {
            List<BIL_BillDetails> bIL_BillDetails = new BillingManager().GetBill((r ?? "").DecryptCookieAndURLSafe());

            if (bIL_BillDetails != null)
            {
                var commingForURL = Request.UrlReferrer;
                if (commingForURL == null)
                {
                    return View("PrintBills", null);
                }

                if (string.IsNullOrWhiteSpace(bIL_BillDetails.First().PaymentAccount))
                {
                    BIL_BankAccountDetails bIL_Account = new BillingManager().GetBankAccountByUserID(bIL_BillDetails.First().User_ID);
                    if (bIL_Account != null)
                    {
                        bIL_BillDetails.ForEach(x =>
                        {
                            x.PaymentBank = bIL_Account.BankName;
                            x.PaymentBranch = bIL_Account.Branch;
                            x.PaymentAccount = bIL_Account.AccountNo;
                            x.PaymentIFSCode = bIL_Account.IFSCode;
                        });
                    }
                }


                if ((commingForURL?.AbsolutePath?.ToLower()?.Contains("/billsetter/") ?? false)
                    || (commingForURL?.AbsolutePath?.ToLower()?.Contains("/billevaluator/") ?? false))
                {
                    if (bIL_BillDetails.First().User_ID == AppUserHelper.User_ID)
                    {
                        return View("PrintBills", bIL_BillDetails);
                    }
                    else
                    {
                        return View("PrintBills", null);
                    }
                }
                else
                {
                    return View("PrintBills", bIL_BillDetails);
                }
            }
            else
                return View("PrintBills", null);
        }


        [HttpGet]
        [OAuthorize(AppRoles.PaperSetterBills, AppRoles.EvaluatorBills, AppRoles.University_Bills)]
        public ActionResult PrintBills()
        {
            return RedirectToAction("index", "Dashboard");
        }

        [HttpPost]
        [OAuthorize(AppRoles.PaperSetterBills, AppRoles.EvaluatorBills, AppRoles.University_Bills)]
        public ActionResult PrintBills(Parameters parameter)
        {
            var commingForURL = Request.UrlReferrer;
            if (commingForURL?.AbsolutePath?.ToLower()?.Contains("/billsetter/") ?? false)
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

                parameter?.Filters?.Add(new SearchFilter
                {
                    Column = "BillStatus",
                    TableAlias = "bd",
                    Value = $"{(short)BillStatus.Paid},{(short)BillStatus.DispatchedToAccountsSection},{(short)BillStatus.Verified}",
                    Operator = SQLOperator.In
                });
            }
            else if (commingForURL?.AbsolutePath?.ToLower()?.Contains("/billevaluator/") ?? false)
            {
                parameter?.Filters?.Add(new SearchFilter
                {
                    Column = "BillType",
                    TableAlias = "bd",
                    Value = ((short)BillType.EvaluatorBill).ToString()
                });

                parameter?.Filters?.Add(new SearchFilter
                {
                    Column = "User_ID",
                    TableAlias = "bd",
                    Value = AppUserHelper.User_ID.ToString()
                });

                parameter?.Filters?.Add(new SearchFilter
                {
                    Column = "BillStatus",
                    TableAlias = "bd",
                    Value = $"{(short)BillStatus.Paid},{(short)BillStatus.DispatchedToAccountsSection},{(short)BillStatus.Verified}",
                    Operator = SQLOperator.In
                });
            }

            if (parameter == null)
                parameter = new Parameters();
            if (parameter.PageInfo == null)
                parameter.PageInfo = new Paging();
            parameter.PageInfo.PageNumber = parameter.PageInfo.PageSize = -1;

            List<BIL_BillDetails> billDetails =
                new BillingManager().GetBills(parameter)
                ?.OrderByDescending(x => x.CreatedOn)
                ?.OrderByDescending(x => x.BillNo)
                ?.OrderBy(x => x.Semester)
                ?.ToList() ?? new List<BIL_BillDetails>();

            if (billDetails.IsNotNullOrEmpty())
            {
                BIL_BankAccountDetails bIL_Account;
                foreach (IEnumerable<BIL_BillDetails> billS in billDetails.GroupBy(X => X.BillNo))
                {
                    if (string.IsNullOrWhiteSpace(billS.First().PaymentAccount))
                    {
                        bIL_Account = new BillingManager().GetBankAccountByUserID(billS.First().User_ID);
                        if (bIL_Account != null)
                        {
                            billS.ForEach(x =>
                            {
                                x.PaymentBank = bIL_Account.BankName;
                                x.PaymentBranch = bIL_Account.Branch;
                                x.PaymentAccount = bIL_Account.AccountNo;
                                x.PaymentIFSCode = bIL_Account.IFSCode;
                            });
                        }
                    }
                }
            }

            return View(billDetails);
        }

        #endregion

    }
}