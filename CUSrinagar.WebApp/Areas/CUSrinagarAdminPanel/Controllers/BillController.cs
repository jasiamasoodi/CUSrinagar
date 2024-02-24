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
using System.Web.Mvc;

namespace CUSrinagar.WebApp.CUSrinagarAdminPanel.Controllers
{
    [OAuthorize(AppRoles.University_Bills)]
    public class BillController : UniversityAdminBaseController
    {
        string errorMsg = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> ##</div>";

        #region PaperSetter
        public ActionResult SetterSearch()
        {
            return View();
        }

        [HttpPost]
        public PartialViewResult SetterList(Parameters parameter)
        {
            if (parameter.Filters is null) parameter.Filters = new List<SearchFilter>();
            parameter.Filters.Add(new SearchFilter() { Column = "RoleID", Operator = SQLOperator.EqualTo, Value = ((int)AppRoles.PaperSetterBills).ToString() });
            List<AppUsers> list = new UserProfileManager().GetAllAppUsers(parameter);
            BillingManager billManager = new BillingManager();

            foreach (AppUsers userItem in list ?? new List<AppUsers>())
            {
                userItem.BankAccountDetail = billManager.GetBankAccountByUserID(userItem.User_ID);
            }
            return PartialView(list);
        }

        [HttpGet]
        public ActionResult CreateSetter()
        {
            FillViewBag_Setter();
            return View();
        }

        [HttpPost]
        public ActionResult CreateSetter(AppUsers appUsers)
        {
            List<int> UserRoles_IDs = new List<int>();
            BIL_PaperSetterInstitute paperSetterInstitute = CreatePaperSetterInstitute(appUsers.Institute, appUsers.User_ID);
            UserRoles_IDs.Add((int)AppRoles.PaperSetterBills);
            try
            {
                if (new UserProfileManager().checkdataExists("UserName", appUsers.UserName, null))
                {
                    ModelState.AddModelError("UserName", "UserName Already Exists");
                }
                if (new UserProfileManager().checkdataExists("Email", appUsers.Email, null))
                {
                    ModelState.AddModelError("Email", "Email Already Exists");
                }
                ModelState.Remove(nameof(appUsers.Designation));
                if (ModelState.IsValid)
                {
                    new UserProfileManager().AddUser(appUsers, UserRoles_IDs, false);
                    new BillingManager().SaveUpdatePaperSetterInstitute(paperSetterInstitute);
                    new EmailSystem().CredentialsMail(appUsers);
                    return RedirectToAction("SetterSearch");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("ErrorMessage", ex.Message);
            }
            FillViewBag_Setter();
            return View(appUsers);
        }

        private BIL_PaperSetterInstitute CreatePaperSetterInstitute(string institute, Guid User_Id)
        {
            BIL_PaperSetterInstitute paperSetterInstitute = new BIL_PaperSetterInstitute()
            {
                Institute = institute,
                User_ID = User_Id

            };
            return paperSetterInstitute;
        }

        [HttpGet]
        public ActionResult EditSetter(Guid id)
        {
            FillViewBag_Setter();
            AppUsers appUsers = new UserProfileManager().GetUserById(id);
            appUsers.Institute = new BillingManager().GetUserInstitute(id)?.Institute;
            return View(appUsers);
        }

        [HttpPost]
        public ActionResult EditSetter(AppUsers appUsers)
        {
            List<int> UserRoles_IDs = new List<int>();
            BIL_PaperSetterInstitute paperSetterInstitute = CreatePaperSetterInstitute(appUsers.Institute, appUsers.User_ID);
            UserRoles_IDs.Add((int)AppRoles.PaperSetterBills);
            try
            {
                if (new UserProfileManager().checkdataExists("UserName", appUsers.UserName, appUsers.User_ID))
                {
                    ModelState.AddModelError("UserName", "UserName Already Exists");
                }
                if (new UserProfileManager().checkdataExists("Email", appUsers.Email, appUsers.User_ID))
                {
                    ModelState.AddModelError("Email", "Email Already Exists");
                }
                ModelState.Remove(nameof(appUsers.Password));
                ModelState.Remove(nameof(appUsers.Designation));
                if (ModelState.IsValid)
                {
                    new UserProfileManager().EditUser(appUsers);
                    new BillingManager().SaveUpdatePaperSetterInstitute(paperSetterInstitute);

                    if (AppUserHelper.User_ID == appUsers.User_ID)
                    {
                        List<AppRoles> roles = AppUserHelper.AppUsercompact?.UserRoles;
                        if (System.Web.HttpContext.Current.User.Identity.IsAuthenticated)
                        {
                            new AuthenticationManager().SignOut();
                        }
                        TempData["response"] = $"<div class='alert alert-success alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button><strong>Great!</strong> <a href='#' class='alert-link'>User details changed successfully. Please login again for changes to reflect.</div>";
                        return RedirectToAction("SignIn", "Account", new { area = string.Empty });
                    }
                    return RedirectToAction("SetterSearch");
                }
            }

            catch (Exception ex)
            {
                ModelState.AddModelError("ErrorMessage", ex.Message);
            }
            FillViewBag_Setter();
            return View(appUsers);
        }
        public void FillViewBag_Setter()
        {
            ViewBag.Roles = Helper.GetSelectedList(AppRoles.PaperSetterBills);
        }
        public ActionResult AssignPaper(Guid id)
        {
            BIL_BillDetails billDetails = new BIL_BillDetails();
            billDetails.FullName = SetNamewithInstitute(id, new UserProfileManager().GetUserById(id)?.FullName);
            billDetails.User_ID = id;
            FillViewBag_AssignPaper(billDetails);

            return View(billDetails);
        }

        private string SetNamewithInstitute(Guid id, string Name)
        {
            string institute = new BillingManager().GetUserInstitute(id)?.Institute;
            return Name + (institute != null ? ("-" + institute) : "");
        }

        public string ChangeStatus(Guid id)
        {
            string msg = string.Empty;
            msg = new UserProfileManager().ChangeStatus(id);
            return msg;
        }

        [HttpPost]
        public JsonResult RemoveRole(string id, string role)
        {
            List<AppRoles> roles = new List<AppRoles>();
            if (role == "Evaluator")
            {
                roles.Add(AppRoles.EvaluatorBills);
                roles.Add(AppRoles.University_Evaluator);
            }
            else { roles.Add(AppRoles.PaperSetterBills); }
            Guid ID = new Guid(id);
            var response = new BillingManager().DeleteUserRoles(ID, roles);
            return Json(new { status = response.Item1, msg = response.Item2 }, JsonRequestBehavior.DenyGet);
        }

        [HttpPost]
        public JsonResult DeleteSetter(string id)
        {
            Guid ID = new Guid(id);
            var response = new BillingManager().DeleteUser(ID);
            return Json(new { status = response.Item1, msg = response.Item2 }, JsonRequestBehavior.DenyGet);
        }

        [HttpPost]
        public JsonResult AddRole(string id, string role)
        {
            List<AppRoles> roles = new List<AppRoles>();
            if (role == "Evaluator")
            {
                roles.Add(AppRoles.EvaluatorBills);
                roles.Add(AppRoles.University_Evaluator);
            }
            else { roles.Add(AppRoles.PaperSetterBills); }
            Guid ID = new Guid(id);
            var response = new BillingManager().SaveUpdateUserRoles(ID, roles);
            return Json(new { status = response.Item1, msg = response.Item2 }, JsonRequestBehavior.DenyGet);
        }

        public ActionResult EvaluatorSearch()
        {
            return View();
        }

        [HttpPost]
        public PartialViewResult EvaluatorList(Parameters parameter)
        {
            if (parameter.Filters is null) parameter.Filters = new List<SearchFilter>();
            parameter.Filters.Add(new SearchFilter() { Column = "RoleID", Operator = SQLOperator.EqualTo, Value = ((int)AppRoles.University_Evaluator).ToString() });

            if (parameter.SortInfo.ColumnName == null)
            {
                parameter.SortInfo = new Sort
                {
                    ColumnName = "Status",
                    OrderBy = System.Data.SqlClient.SortOrder.Descending
                };
            }


            List<AppUsers> list = new UserProfileManager().GetAllAppUsers(parameter);
            BillingManager billManager = new BillingManager();

            foreach (AppUsers userItem in list ?? new List<AppUsers>())
            {
                if (userItem.UserRoles.Any(x => x.RoleID == AppRoles.PaperSetterBills))
                    continue;

                userItem.BankAccountDetail = billManager.GetBankAccountByUserID(userItem.User_ID);
            }
            return PartialView(list);

        }
        #endregion

        #region Bill

        [ChildActionOnly]
        public string GetPaperSetting(Guid d)
        {
            BIL_PaperSetterSettings paperSetterSettings = new BillingManager().GetPaperSetterSetting(d);
            return paperSetterSettings != null
                ? $@"<a href=""{paperSetterSettings.FilePath}"" target=""_blank"">{paperSetterSettings.Title}</a>"
                : "";
        }

        [HttpGet]
        public ActionResult Bills()
        {
            FillViewBag_Bill();
            return View();
        }

        [HttpPost]
        public PartialViewResult BillList(Parameters parameter)
        {
            #region for notification
            SearchFilter billsType = parameter.Filters.FirstOrDefault(x => x.Column.ToLower().Trim().Contains("billtype"));

            ViewBag.BillsTypeSelected = billsType.Value;

            Parameters parameters = new Parameters
            {
                Filters = new List<SearchFilter> { billsType }
            };

            ViewBag.BillStatistics = new BillingManager().GetBillStatistics(parameters) ?? new List<BillStatistics>();
            #endregion

            List<BIL_BillDetails> list = new BillingManager().GetBills(parameter);

            if (list.IsNotNullOrEmpty())
            {
                BIL_BankAccountDetails bIL_Account;
                foreach (IEnumerable<BIL_BillDetails> billS in list.GroupBy(X => X.BillNo))
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

            ViewBag.BillStatus = Helper.GetSelectList(BillStatus.Accepted, BillStatus.EmailSent, BillStatus.ExternalAwardsUploaded, BillStatus.Paid, BillStatus.Rejected);
            return PartialView(list);
        }
        private void FillViewBag_Bill()
        {
            ViewBag.BillStatus = Helper.GetSelectList<BillStatus>();
            ViewBag.BillType = Helper.GetSelectList<BillType>();
        }
        private void FillViewBag_AssignPaper(BIL_BillDetails bIL_BillDetails)
        {
            Parameters parameters = CreateParameter(bIL_BillDetails);
            BillingManager billingManager = new BillingManager();
            ViewBag.SemestersDDL = Helper.GetSelectList<Semester>();
            ViewBag.SubjectDeptsDDL = billingManager.SubjectDepartmentsDDL();
            if (parameters != null)
            {
                SearchFilter sf = parameters.Filters.Where(x => x.Column == "Department_ID").FirstOrDefault();
                sf.TableAlias = "S";
                ViewBag.SubjectsDDL = new SubjectsManager().SubjectDDLWithDetail(parameters) ?? new List<SelectListItem>();
                sf.TableAlias = "SM";
                ViewBag.BillNosDDL = new BillingManager().BillNoDDL(parameters) ?? new List<SelectListItem>();
            }
            ViewBag.PaperPatternsDDL = billingManager.PaperPatterFileDDL(SetterFileType.PaperPattern);
            ViewBag.SamplePapersDDL = billingManager.PaperPatterFileDDL(SetterFileType.SamplePaper);

        }
        private Parameters CreateParameter(BIL_BillDetails bIL_BillDetails)
        {
            if (bIL_BillDetails.Bill_ID == Guid.Empty || bIL_BillDetails.Bill_ID == null) return null;
            Parameters parameter = new Parameters()
            {
                Filters = new List<SearchFilter>() {
                    new SearchFilter() {Column="User_ID", Operator=SQLOperator.EqualTo,Value=bIL_BillDetails.User_ID.ToString() }
                    ,  new SearchFilter() {Column="Semester", Operator=SQLOperator.EqualTo,Value=bIL_BillDetails.Semester.ToString() }
                    ,  new SearchFilter() {Column="Department_ID", Operator=SQLOperator.EqualTo,Value=bIL_BillDetails.Department_Id.ToString() }
                }
            };
            return parameter;
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveBill(BIL_BillDetails bIL_BillDetails)
        {
            ModelState.Remove("BillNo");
            if (bIL_BillDetails.BillType == BillType.EvaluatorBill)
            {
                ModelState.Remove("PaperReceiverEmail");
            }
            if (ModelState.IsValid)
            {
                Tuple<bool, string> result = bIL_BillDetails.Bill_ID == Guid.Empty
                    ? new BillingManager().Save(bIL_BillDetails)
                    : new BillingManager().Update(bIL_BillDetails);

                if (result.Item1)
                {
                    TempData["Err"] = errorMsg.Replace("##", result.Item2).Replace("alert-danger", "alert-success");
                    return RedirectToAction("Bills");
                }
                TempData["ErrorMessage"] = errorMsg.Replace("##", result.Item2);

            }
            FillViewBag_AssignPaper(bIL_BillDetails);
            bIL_BillDetails.FullName = SetNamewithInstitute(bIL_BillDetails.User_ID, new UserProfileManager().GetUserById(bIL_BillDetails.User_ID)?.FullName);
            return View("AssignPaper", bIL_BillDetails);
        }

        [HttpPost]
        public void BillsExcel(Parameters parameter)
        {
            if (parameter == null)
                parameter = new Parameters();
            if (parameter.PageInfo == null)
                parameter.PageInfo = new Paging();
            parameter.PageInfo.PageNumber = parameter.PageInfo.PageSize = -1;
            try
            {
                List<BIL_BillDetails> billDetails =
                    new BillingManager().GetBills(parameter)
                    ?.OrderByDescending(x => x.CreatedOn)
                    ?.OrderByDescending(x => x.BillNo)
                    ?.OrderBy(x => x.Semester)
                    ?.ToList() ?? new List<BIL_BillDetails>();

                short SNo = 0;
                if (billDetails.First().BillType == BillType.PaperSetterBill)
                {
                    List<ExcelBillDetailsPaperSetter> listforExcel = new List<ExcelBillDetailsPaperSetter>();
                    foreach (IEnumerable<BIL_BillDetails> groupedBills in billDetails.GroupBy(x => x.BillNo))
                    {
                        SNo++;
                        listforExcel.Add(new ExcelBillDetailsPaperSetter
                        {
                            SNo = SNo,
                            BillNo = groupedBills.First().BillNo,
                            Bill_Type = groupedBills.First().BillType.GetEnumDescription(),
                            Name = groupedBills.First().FullName.ToUpper(),
                            Semester = groupedBills.First().Semester,
                            Bank_Name = groupedBills.First().PaymentBank,
                            Bank_Branch = groupedBills.First().PaymentBranch,
                            AccountNo ="AC/No: "+ groupedBills.First().PaymentAccount,
                            BillStatus = groupedBills.First().BillStatus.GetEnumDescription(),
                            Gross_Amount = groupedBills.Sum(x => x.TotalAmount),
                            Amount_Deducted_For_RevenueStamp = groupedBills.First().RevenueStampAmount,
                            Net_Amount = groupedBills.Sum(x => x.TotalAmount) - groupedBills.First().RevenueStampAmount,
                            Examination = groupedBills.First().Examination,
                            Subjects = string.Join(" , " + Environment.NewLine, groupedBills.Select(x => x.SubjectFullName)),
                            Remarks = groupedBills.First().RevenueStampAmount > 0 ? "Without Revenue Stamp" : "",
                            Institute = groupedBills.First().Institute,
                        });
                    }
                    ExportToCSV(listforExcel, $"CUS_{listforExcel?.FirstOrDefault()?.Bill_Type ?? "No bills found"}");
                }
                else
                {
                    List<ExcelBillDetailsEvaluator> listforExcel = new List<ExcelBillDetailsEvaluator>();
                    foreach (IEnumerable<BIL_BillDetails> groupedBills in billDetails.GroupBy(x => x.BillNo))
                    {
                        SNo++;
                        listforExcel.Add(new ExcelBillDetailsEvaluator
                        {
                            SNo = SNo,
                            BillNo = groupedBills.First().BillNo,
                            Bill_Type = groupedBills.First().BillType.GetEnumDescription(),
                            Name = groupedBills.First().FullName.ToUpper(),
                            Semester = groupedBills.First().Semester,
                            Bank_Name = groupedBills.First().PaymentBank,
                            Bank_Branch = groupedBills.First().PaymentBranch,
                            AccountNo = groupedBills.First().PaymentAccount,
                            BillStatus = groupedBills.First().BillStatus.GetEnumDescription(),
                            Conveyance_Charges = (groupedBills.First().ConveyanceCharges ?? 0),
                            Gross_Amount = groupedBills.Sum(x => x.TotalAmount),
                            Amount_Deducted_For_RevenueStamp = groupedBills.First().RevenueStampAmount,
                            Net_Amount = groupedBills.Sum(x => x.TotalAmount) - groupedBills.First().RevenueStampAmount,
                            Examination = groupedBills.First().Examination,
                            Subjects = string.Join(" , " + Environment.NewLine, groupedBills.Select(x => x.SubjectFullName)),
                            Remarks = groupedBills.First().RevenueStampAmount > 0 ? "Without Revenue Stamp" : "",
                            Institute = groupedBills.First().Institute,
                        });
                    }

                    ExportToCSV(listforExcel, $"CUS_{listforExcel?.FirstOrDefault()?.Bill_Type ?? "No bills found"}");
                }
            }
            catch (Exception) { }

        }


        [HttpPost]
        public ActionResult RemoveBill(string id)
        {
            Guid ID = new Guid(id);
            var response = new BillingManager().Delete(ID);
            return Json(new { status = response.Item1, msg = response.Item2 }, JsonRequestBehavior.DenyGet);
        }

        [HttpPost]
        public ActionResult DeleteBill(string BillNo)
        {
            var response = new BillingManager().Delete(BillNo);
            return Json(new { status = response.Item1, msg = response.Item2 }, JsonRequestBehavior.DenyGet);
        }

        [HttpPost]
        public ActionResult ChangeBillStatus(Parameters parameter, string BillStatus)
        {
            BillStatus billStatus = (BillStatus)Enum.Parse(typeof(BillStatus), BillStatus);
            var response = new BillingManager().BulkUpdateBillStatus(parameter, billStatus);
            return Json(new { status = response.Item1, msg = response.Item2 }, JsonRequestBehavior.DenyGet);
        }
        public ActionResult EditBill(Guid id)
        {
            BIL_BillDetails bIL_BillDetails = new BillingManager().GetBillByBillID(id);
            if (bIL_BillDetails == null)
            {
                TempData["Err"] = errorMsg.Replace("##", "Bill not found").Replace("alert-danger", "alert-success");
                return RedirectToAction("Bills");
            }
            if (bIL_BillDetails.BillStatus == BillStatus.Paid)
            {

                TempData["Err"] = errorMsg.Replace("##", $"Bill has already been paid on {bIL_BillDetails.PaymentDate?.ToLongDateString()}, hence cannot be edited").Replace("alert-danger", "alert-success");
                return RedirectToAction("Bills");
            }
            else
            {
                FillViewBag_AssignPaper(bIL_BillDetails);
                bIL_BillDetails.FullName = SetNamewithInstitute(bIL_BillDetails.User_ID, bIL_BillDetails.FullName);
                return View("AssignPaper", bIL_BillDetails);
            }
        }


        [HttpPost]
        public ActionResult ChangeRevenueStampCharges(string BillNo, short NewAmount)
        {
            var response = new BillingManager().UpdateRevenueStampCharges(BillNo, NewAmount);
            return Json(new { status = response.Item1, msg = response.Item2 }, JsonRequestBehavior.DenyGet);
        }

        [HttpPost]
        public ActionResult ChangeBillStatusPaidAndClosed(BulkBillTransactionStatus billTransactionStatus)
        {
            if (ModelState.IsValid)
            {
                Tuple<bool, string> _bankaccount = new BillingManager().BulkUpdateBillStatusToPaid(billTransactionStatus);

                TempData["Err"] = _bankaccount.Item1
                    ? errorMsg.Replace("##", _bankaccount.Item2).Replace("alert-danger", "alert-success")
                    : errorMsg.Replace("##", _bankaccount.Item2);
            }

            return RedirectToAction("Bills");
        }
        #endregion


        #region PaperSetterSetting
        [HttpGet]
        public ActionResult PaperSetterList(Parameters parameter)
        {
            ViewBag.PaperSetter = Helper.GetSelectList<SetterFileType>();
            List<BIL_PaperSetterSettings> listpapersetter = new BillingManager().GetPaperSetterSettings(parameter);
            ViewBag.data = listpapersetter?.OrderBy(x => x.Type)?.OrderByDescending(x => x.CreatedOn);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreatePaperSetterSettings(BIL_PaperSetterSettings papersettersettings)
        {
            if (ModelState.IsValid)
            {
                Tuple<bool, string> _papersetter = new BillingManager().Save(papersettersettings);

                if (_papersetter.Item1)
                {
                    TempData["Err"] = errorMsg.Replace("##", "Saved successfully").Replace("alert-danger", "alert-success");
                }
                else
                {
                    TempData["Err"] = errorMsg.Replace("##", "Unable to Save");
                }
            }
            else
            {
                TempData["Err"] = errorMsg.Replace("##", "Details / File are not valid");
            }
            return RedirectToAction("PaperSetterList", new { id = "" });

        }
        public void FillViewBag_PaperSetter()
        {
            IEnumerable<SelectListItem> list = Helper.GetSelectList<SetterFileType>();
            ViewBag.PaperSetter = list ?? new List<SelectListItem>();
        }
        [HttpPost]
        public JsonResult DeletePaperSetting(Guid id)
        {
            var response = new BillingManager().DeletePaperSetterSetting(id);
            return Json(new { status = response.Item1, msg = response.Item2 }, JsonRequestBehavior.DenyGet);
        }
        [HttpPost]
        public JsonResult UpdatePaperSettingStatus(string id)
        {
            string msg = string.Empty;
            Guid SettingID = Guid.Parse(id);
            bool IsActive = false;
            var obj = new BillingManager().GetPaperSetterSetting(SettingID);
            if (obj.IsActive == false)
            {
                IsActive = true;
            }
            var response = new BillingManager().UpdatePaperSetterSettingStatus(SettingID, IsActive);
            return Json(new { status = response.Item1, msg = response.Item2 }, JsonRequestBehavior.DenyGet);
        }

        #endregion

        #region BankAccount

        [HttpPost]
        public JsonResult UpdateBankAccountStatus(Guid User_ID)
        {
            BillingManager billingManager = new BillingManager();
            BIL_BankAccountDetails bankAccountDetails = billingManager.GetBankAccountByUserID(User_ID);
            var response = billingManager.UpdateBankAccountStatus(User_ID, !(bankAccountDetails?.IsEditable ?? true));
            return Json(new { status = response.Item1, msg = response.Item2 }, JsonRequestBehavior.DenyGet);
        }

        #endregion

    }
}