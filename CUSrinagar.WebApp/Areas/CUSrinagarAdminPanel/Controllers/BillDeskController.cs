using CUSrinagar.BusinessManagers;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using CUSrinagar.Models;
using CUSrinagar.OAuth;
using GeneralModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;

namespace CUSrinagar.WebApp.CUSrinagarAdminPanel.Controllers
{
    [OAuthorize(AppRoles.University_PaymentReconcile)]
    public class BillDeskController : Controller
    {
        #region ViewBags
        private void SetViewBags()
        {
            ViewBag.FeeType = Helper.GetSelectList(PaymentModuleType.None, PaymentModuleType.FirstSemesterAdmissionReconcileOnly);
            List<SelectListItem> Courses = Helper.GetSelectList<PrintProgramme>().ToList();
            foreach (SelectListItem x in Courses)
            {
                if (x.Value == "3")
                {
                    x.Text += " / Cert. in Arabic";
                    break;
                }

            }
            ViewBag.course = Courses;

            ViewBag.StoredRequests = new List<BillDeskStoredRequest>();
            ViewBag.FeeTypeSelected = "";
        }
        #endregion


        #region Check Payment & Status
        [HttpGet]
        public ActionResult SearchPayments()
        {
            SetViewBags();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SearchPayments(S2SSearch _S2SSearch)
        {
            SetViewBags();
            ViewBag.FeeTypeSelected = _S2SSearch.FeeType.ToString();
            if (!ModelState.IsValid)
                return View(_S2SSearch);
            ViewBag.StoredRequests = new BillDeskManager().GetRequests(_S2SSearch) ?? new List<BillDeskStoredRequest>();
            return View();
        }

        [HttpPost]
        public JsonResult CheckPaymentStatus(string Id)
        {
            if (string.IsNullOrWhiteSpace(Id))
            {
                return Json("<td colspan='7'>Customer ID not provided.</td>", JsonRequestBehavior.DenyGet);
            }
            else
            {
                BillDeskManager billDesk = new BillDeskManager();
                BillDeskS2SRequest billDeskS2SRequest = new BillDeskS2SRequest
                {
                    CustomerID = Id
                };

                //call S2S API
                string RequestMsg = billDesk.GenerateRequestString(billDeskS2SRequest);
                var BillDeskResponse = APIHelper.PostAsync("?msg=" + RequestMsg);

                Tuple<bool, string, PaymentDetails, BillDeskS2SResponse, Guid> DisplayResponse = null;

                if (BillDeskResponse != string.Empty)
                {
                    DisplayResponse = billDesk.BillDeskS2SResponse(BillDeskResponse);
                }
                bool PaymentExistInDB = billDesk.PaymentExistsInDB(DisplayResponse.Item3?.TxnReferenceNo);

                //create visual display response
                RequestMsg = $"<td>{DisplayResponse.Item2}</td>";
                RequestMsg += PaymentExistInDB ? "<th style='color:green'>Payment exist in DB</th>" : "<th style='color:red'>Payment does not exist in DB</th>";
                RequestMsg += $"<td>{DisplayResponse.Item3?.TxnReferenceNo}</td>";
                RequestMsg += $"<td><i class='fa fa-rupee'></i> {DisplayResponse.Item3?.TxnAmount}</td>";
                RequestMsg += $"<td>{DisplayResponse.Item3?.TxnDate.ToString("dd MMMM,yyyy hh:mm tt")}</td>";
                RequestMsg += $"<td>{DisplayResponse.Item3?.AdditionalInfo}</td>";

                string copyforRefund = @"<table border=/""1/"">
                                                <tr style=/""color:white;background-color:#375b30/"">
	                                                <th>Biller ID</th>
	                                                <th>PG Ref No.</th>
	                                                <th>Date of Txn (YYYYMMDD)</th>
	                                                <th>REF 1 (Cust ID)</th>
	                                                <th>Txn Amt</th>
	                                                <th>Refund amt</th>
                                                </tr>";
                copyforRefund += $@"<tr>
	                                    <td>{BillDeskSettings.MerchantID}</td>
	                                    <td>{DisplayResponse.Item3?.TxnReferenceNo}</td>
	                                    <td>{DisplayResponse.Item3?.TxnDate.ToString("yyyyMMdd")}</td>
	                                    <td>_{Id}</td>
	                                    <td>{DisplayResponse.Item3?.TxnAmount}</td>
	                                    <td>{DisplayResponse.Item3?.TxnAmount}</td>
                                    </tr>
                                    </table>";

                if (DisplayResponse.Item2.Trim().ToLower().Contains("payment status (0300) is success and is currently not refunded or cancelled") && !PaymentExistInDB)
                {
                    RequestMsg += $"<th><button type='button' class='Reconcile quadraText' data-BillDeskResponse='{BillDeskResponse.EncryptCookieAndURLSafe()}'>Reconcile</button><br/><br/>";
                    RequestMsg += $"<a href='javascript:void(0);' class='copytorefund' data-copy='{copyforRefund.Replace("\r\n", "").Replace("\n", "")}'>Copy for ReFund</a><br/><br/>";
                    RequestMsg += $"<a class='closetr' data-Id='{Id}' href='javascript:void(0);'>x Close</a></th>";
                }
                else if(PaymentExistInDB)
                {
                    RequestMsg += $"<th><a href='javascript:void(0);' class='copytorefund' data-copy='{copyforRefund.Replace("\r\n", "").Replace("\n", "")}'>Copy for ReFund</a><br/><br/>";
                    RequestMsg += $"<a class='closetr' data-Id='{Id}' href='javascript:void(0);'>x Close</a></th>";
                }
                else
                {
                    RequestMsg += $"<th><a class='closetr' data-Id='{Id}' href='javascript:void(0);'>x Close</a></th>";
                }
                return Json(RequestMsg, JsonRequestBehavior.DenyGet);
            }
        }
        #endregion


        #region Reconcilation Process
        [HttpPost]
        public JsonResult Reconcile(string BillDeskResponse, PaymentModuleType FeeType)
        {
            Tuple<bool, string> result = null;
            try
            {
                if (string.IsNullOrWhiteSpace(BillDeskResponse) || FeeType == PaymentModuleType.None)
                {
                    return Json("Invalid Data suppllied. Refresh and try again", JsonRequestBehavior.DenyGet);
                }
                BillDeskResponse = BillDeskResponse.DecryptCookieAndURLSafe();

                #region Start Reconcilation Process
                result = new BillDeskManager().DoReconcilation(BillDeskResponse, FeeType);

                if (result.Item1)
                    return Json(result.Item1, JsonRequestBehavior.DenyGet);
                else
                    return Json(result.Item2, JsonRequestBehavior.DenyGet);
                #endregion
            }
            catch (CryptographicException)
            {
                return Json("Invalid Data suppllied. Refresh and try again", JsonRequestBehavior.DenyGet);
            }
        }
        #endregion

    }
}