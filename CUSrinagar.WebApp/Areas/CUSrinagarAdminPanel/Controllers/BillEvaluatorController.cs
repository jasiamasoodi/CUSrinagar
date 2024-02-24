using CUSrinagar.BusinessManagers;
using CUSrinagar.Enums;
using CUSrinagar.Models;
using CUSrinagar.OAuth;
using System;
using System.Web.Mvc;
using CUSrinagar.Extensions;
using GeneralModels;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using System.Data.Common;
using CUSrinagar.DataManagers;
using Microsoft.Ajax.Utilities;

namespace CUSrinagar.WebApp.CUSrinagarAdminPanel.Controllers
{
    [OAuthorize(AppRoles.EvaluatorBills)]
    public class BillEvaluatorController : Controller
    {
        string errorMsg = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> ##</div>";

        #region Bills
        [HttpGet]
        public ActionResult Bills()
        {
            ViewBag.BillType = BillType.EvaluatorBill;

            #region for Statistics

            Parameters parameter = new Parameters
            {
                Filters = new List<SearchFilter>()
            };
            parameter?.Filters?.Add(new SearchFilter
            {
                Column = "BillType",
                Value = ((short)BillType.EvaluatorBill).ToString()
            });

            parameter?.Filters?.Add(new SearchFilter
            {
                Column = "User_ID",
                Value = AppUserHelper.User_ID.ToString()
            });

            ViewBag.BillStatistics = new BillingManager().GetBillStatistics(parameter) ?? new List<BillStatistics>();
            #endregion

            List<SelectListItem> bstatus
            = Helper.GetSelectList(BillStatus.InProcess, BillStatus.Assigned, BillStatus.EmailSent, BillStatus.Accepted, BillStatus.Rejected, BillStatus.ExternalAwardsUploaded).ToList();
            bstatus.Insert(0, Helper.GetSelectedList(BillStatus.ExternalAwardsUploaded).First());

            ViewBag.BillStatus = bstatus;

            return View();
        }


        [HttpPost]
        public PartialViewResult EvaluatorList(Parameters parameter)
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
            return PartialView(list);
        }
        #endregion

        #region GenerateBill

        public ActionResult GenerateEvaluatorBill(string Subject_ID)
        {
            ADMSubjectMaster subjectMaster = new SubjectsManager().Get(Guid.Parse(Subject_ID));
            if (!subjectMaster.HasExaminationFee)
            {
                return Json(new { Item1 = false, Item2 = $"Bill cannot be generated for subject: {subjectMaster.SubjectFullName}, has no University Examinaton Fee" }, JsonRequestBehavior.DenyGet);
            }

            Guid User_ID = AppUserHelper.User_ID;
            BIL_BankAccountDetails bankAccountDetails = new BillingManager().GetBankAccountByUserID(User_ID);
            if (bankAccountDetails == null)
            {
                return Json(new
                {
                    Item1 = false,
                    Item2 = $"Before Generating bill please enter your Bank Account Details under Bank Account Option"
                },
                JsonRequestBehavior.DenyGet);
            }

            AwardFilterSettings awardFilterSettings = new ResultManager().FetchAwardFilterSettings(subjectMaster.Programme, MarksFor.Theory, (int)subjectMaster.Semester);

            int Year = awardFilterSettings.FilterValue;
            int Batch = new ResultManager().FetchAwardFilterSettings(subjectMaster.Programme, MarksFor.Practical, (int)subjectMaster.Semester).FilterValue;
            if (BillExists(subjectMaster.Subject_ID, Batch, BillType.EvaluatorBill, User_ID))
            {
                return Json(new { Item1 = false, Item2 = "Bill Already Exists" }, JsonRequestBehavior.DenyGet);
            }
            BIL_BillDetails bIL_BillDetails = createEvaluatorBill(subjectMaster, Year, Batch);
            Tuple<bool, string> result = new BillingManager().Save(bIL_BillDetails);

            return Json(result, JsonRequestBehavior.DenyGet);

        }


        private BIL_BillDetails createEvaluatorBill(ADMSubjectMaster subjectMaster, int Year, int Batch)
        {
            BIL_BillDetails bIL_BillDetails = new BIL_BillDetails();
            bIL_BillDetails.BillType = BillType.EvaluatorBill;
            bIL_BillDetails.BillStatus = BillStatus.ExternalAwardsUploaded;
            bIL_BillDetails.Session = Year.ToString();
            bIL_BillDetails.Examination = subjectMaster.Programme.GetEnumDescription();
            bIL_BillDetails.NoOfSets = (short)GetExternalAwardCount(subjectMaster.Programme, subjectMaster.Subject_ID, subjectMaster.Semester, AppUserHelper.User_ID, Year);

            if (bIL_BillDetails.NoOfSets <= 0)
                return null;

            bIL_BillDetails.AmountPerSet = GetAmountPerPaper(subjectMaster.Programme, subjectMaster.Semester);
            bIL_BillDetails.Batch = Batch + " & previous Backlogs";
            bIL_BillDetails.User_ID = AppUserHelper.User_ID;
            bIL_BillDetails.Subject_ID = subjectMaster.Subject_ID;
            bIL_BillDetails.Semester = (short)subjectMaster.Semester;
            bIL_BillDetails.TotalAssignmentCompletionDays = 0;
            return bIL_BillDetails;
        }

        private int GetExternalAwardCount(Programme programme, Guid subject_ID, Semester semester, Guid user_ID, int year)
        {
            bool isResultDeclared = new ResultManager().checkIsResultDeclared((int)semester, programme);
            Parameters parameters = new Parameters();
            parameters.Filters = new List<SearchFilter>
            {
                new SearchFilter
                {
                    Column = "Semester",
                    Value = ((short)semester).ToString(),
                },
                new SearchFilter
                {
                    Column = "CombinationSubjects",
                    Value = subject_ID.ToString(),
                }
            };

            return new AwardDB()
                .GetAllRecordsExt(parameters, programme, MarksFor.Theory, year, null, true, SubjectType.Core, isResultDeclared)
                ?.Where(x => x.ExternalMarks >= 0)?.Count() ?? 0;
        }

        private bool BillExists(Guid Subject_ID, int Batch, BillType BillType, Guid User_ID)
        {
            return new BillingManager().CheckBillExists(Subject_ID, Batch, BillType, User_ID);
        }
        private short GetAmountPerPaper(Programme programme, Semester semester)
        {
            switch (programme)
            {
                case Programme.PG:
                    return 30;
                case Programme.IG:
                    if ((int)semester <= 6) { return 25; }
                    else if ((int)semester >= 7) { return 30; }
                    break;
                default:
                    return 25;
            }
            return 0;
        }
        #endregion
    }
}