using CUSrinagar.DataManagers;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using CUSrinagar.Models;
using GeneralModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace CUSrinagar.BusinessManagers
{
    public class BillingManager
    {
        #region Billing Details
        public List<BIL_BillDetails> GetBills(Parameters parameter)
        {
            if (parameter == null)
                return null;

            return new BillingDB().GetBills(parameter);
        }
        public List<BIL_BillDetails> GetBills(Guid User_ID, BillType billType)
        {
            if (User_ID == Guid.Empty)
                return null;

            return new BillingDB().GetBills(User_ID, billType);
        }
        public BIL_BillDetails GetBill(Guid User_ID)
        {
            if (User_ID == Guid.Empty)
                return null;

            return new BillingDB().GetBill(User_ID);
        }

        public List<BIL_BillDetails> GetBill(string BillNo)
        {
            if (string.IsNullOrWhiteSpace(BillNo))
                return null;

            return new BillingDB().GetBill(BillNo);
        }
        public BIL_BillDetails GetBillByBillID(Guid Bill_ID)
        {
            if (Bill_ID == Guid.Empty)
                return null;

            return new BillingDB().GetBillByBillID(Bill_ID);
        }

        public bool CheckBillExists(Guid Subject_ID, int Batch, BillType BillType,Guid User_ID)
        {
            BIL_BillDetails BIL_BillDetails = new BillingDB().GetBill(Subject_ID,  Batch,  BillType, User_ID);

            if (BIL_BillDetails == null)
                return false;
            return true;
        }
        public Tuple<bool, string> Save(BIL_BillDetails billDetails)
        {
            if (billDetails == null)
                return Tuple.Create(false, "Invalid Details");

            if (billDetails.User_ID == Guid.Empty || billDetails.Subject_ID == Guid.Empty || billDetails.NoOfSets <= 0)
                return Tuple.Create(false, "Invalid Details");

            BIL_PaperSetterInstitute existingPaperSetterInstitute = GetUserInstitute(billDetails.User_ID);

            if (existingPaperSetterInstitute == null)
                return Tuple.Create(false, "Paper Setter Institute not mentioned");

            if (string.IsNullOrWhiteSpace(billDetails.BillNo) || billDetails.BillNo == "undefined")
                billDetails.BillNo = (billDetails.BillType == BillType.PaperSetterBill ? "S-" : "E-") + DateTime.UtcNow.Ticks.ToString();

            billDetails.Bill_ID = Guid.NewGuid();
            billDetails.BillStatus = billDetails.BillType == BillType.EvaluatorBill ? BillStatus.ExternalAwardsUploaded : BillStatus.InProcess;
            billDetails.CreatedBy = AppUserHelper.User_ID;
            billDetails.CreatedOn = DateTime.Now;
            billDetails.Institute = existingPaperSetterInstitute.Institute;

            if (billDetails.BillType == BillType.PaperSetterBill)
            {
                billDetails.ConveyanceCharges = null;
            }
            else if (billDetails.BillType == BillType.EvaluatorBill)
            {
                billDetails.PaperReceiverEmail = " ";
                billDetails.SyllabusLink = null;
                billDetails.PaperPattern_ID = Guid.Empty;
                billDetails.SamplePaper_ID = Guid.Empty;
            }

            int result = new BillingDB().Save(billDetails);

            if (result <= 0)
                return Tuple.Create(false, "Bill not Saved. Please try again");

            return Tuple.Create(true, $"Bill Saved Succesfully with Bill No: {billDetails.BillNo}");
        }

      

        public Tuple<bool, string> Update(BIL_BillDetails billDetails)
        {
            if (billDetails == null)
                return Tuple.Create(false, "Invalid Details");

            if (billDetails.Bill_ID == Guid.Empty || billDetails.User_ID == Guid.Empty || billDetails.Subject_ID == Guid.Empty || billDetails.NoOfSets <= 0 || billDetails.Bill_ID == Guid.Empty)
                return Tuple.Create(false, "Invalid Details");

            BIL_BillDetails existingBill = GetBillByBillID(billDetails.Bill_ID);

            if (existingBill == null)
                return Tuple.Create(false, "Bill does not exist");

            if (existingBill.BillStatus == BillStatus.Paid)
                return Tuple.Create(false, "No allowed to update paid bills");

            BIL_PaperSetterInstitute existingPaperSetterInstitute = GetUserInstitute(billDetails.User_ID);

            if (existingPaperSetterInstitute == null)
                return Tuple.Create(false, "Paper Setter Institute not mentioned");

            if (string.IsNullOrWhiteSpace(billDetails.BillNo))
                billDetails.BillNo = DateTime.UtcNow.Ticks.ToString();

            billDetails.BillStatus = existingBill.BillStatus;
            billDetails.BillType = existingBill.BillType;
            billDetails.UpdatedBy = AppUserHelper.User_ID;
            billDetails.UpdatedOn = DateTime.Now;
            billDetails.Institute = existingPaperSetterInstitute.Institute;
            if (existingBill.BillType == BillType.PaperSetterBill)
            {
                billDetails.ConveyanceCharges = null;
            }
            else if (existingBill.BillType == BillType.EvaluatorBill)
            {
                billDetails.SyllabusLink = "";
                billDetails.PaperPattern_ID = Guid.Empty;
                billDetails.SamplePaper_ID = Guid.Empty;
                billDetails.PaperReceiverEmail = "";
            }

            int result = new BillingDB().Update(billDetails);

            if (result <= 0)
                return Tuple.Create(false, "Bill not Updated. Please try again");

            return Tuple.Create(true, $"Bill Updated Succesfully with Bill No: {billDetails.BillNo}");
        }
        public Tuple<bool, string> UpdateBillStatus(string BillNo, BillStatus billStatus)
        {
            if (string.IsNullOrWhiteSpace(BillNo))
                return Tuple.Create(false, "invalid details");

            List<BIL_BillDetails> bills = new BillingDB().GetBill(BillNo);
            if (bills == null)
                return Tuple.Create(false, "Bill not found");

            if (bills.Any(x => x.BillStatus == BillStatus.Paid))
                return Tuple.Create(false, "Bill has already been paid");

            if (billStatus == BillStatus.DispatchedToAccountsSection)
            {
                BIL_BankAccountDetails bankDetails = new BillingDB().GetBankAccountByUserID(bills.First().User_ID);
                if (bankDetails == null)
                    return Tuple.Create(false, "Bank Account Details not found");
            }

            new BillingDB().UpdateBillStatus(BillNo, billStatus);

            if (billStatus == BillStatus.DispatchedToAccountsSection)
            {
                new BillingDB().SaveUpdateBankAccountInBillDetails(BillNo);
                UpdateBankAccountStatus(bills.First().User_ID, false);
            }

            return Tuple.Create(false, $"Bill Status Changed Succesfully to {billStatus.GetEnumDescription()}");
        }
        public Tuple<bool, string> UpdateRevenueStampCharges(string BillNo, short Amount)
        {
            if (string.IsNullOrWhiteSpace(BillNo))
                return Tuple.Create(false, "invalid details");

            int result = new BillingDB().UpdateRevenueStampCharges(BillNo, Amount);

            if (result <= 0)
                return Tuple.Create(false, "Changes not updated. Please try again");

            return Tuple.Create(false, $"Changes updated Succesfully");
        }

        public Tuple<bool, string> BulkUpdateBillStatus(Parameters parameter, BillStatus billStatus)
        {
            if (parameter == null)
                return Tuple.Create(false, "invalid details");

            if (parameter.PageInfo == null)
                parameter.PageInfo = new Paging();

            parameter.PageInfo.PageNumber
                = parameter.PageInfo.PageSize = -1;

            List<BIL_BillDetails> bills = new BillingDB().GetBills(parameter);
            if (bills == null)
                return Tuple.Create(false, "Bills not found");

            if (bills.All(x => x.BillStatus == BillStatus.Paid))
                return Tuple.Create(false, "Bills has already been paid");

            foreach (IEnumerable<BIL_BillDetails> bill in bills.OrderByDescending(x => x.CreatedOn).ThenBy(x => x.BillNo).GroupBy(x => x.BillNo))
            {
                if (bill.First().BillStatus == BillStatus.Paid)
                    continue;

                if (billStatus == BillStatus.DispatchedToAccountsSection)
                {
                    BIL_BankAccountDetails bankDetails = new BillingDB().GetBankAccountByUserID(bill.First().User_ID);
                    if (bankDetails == null)
                        continue;
                }

               int s= new BillingDB().UpdateBillStatus(bill.First().BillNo, billStatus);

                if (billStatus == BillStatus.DispatchedToAccountsSection)
                {
                   int ss= new BillingDB().SaveUpdateBankAccountInBillDetails(bill.First().BillNo);
                   UpdateBankAccountStatus(bill.First().User_ID, false);
                }
            }

            return Tuple.Create(false, $"Bill Status Changed Succesfully to {billStatus.GetEnumDescription()}");
        }
        public Tuple<bool, string> BulkUpdateBillStatusToPaid(BulkBillTransactionStatus bulkBillTransactionStatus)
        {
            if (bulkBillTransactionStatus == null)
                return Tuple.Create(false, "invalid details");

            if (bulkBillTransactionStatus.CSVFile == null || bulkBillTransactionStatus.CSVFile?.ContentLength <= 0
                || Path.GetExtension(bulkBillTransactionStatus.CSVFile?.FileName).ToLower().Trim() != ".csv")
            {
                return Tuple.Create(false, "Please choose valid file");
            }

            List<string> billNoList = new List<string>();


            #region CSVParsing Work
            Regex CSVParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
            using (StreamReader csvreader = new StreamReader(bulkBillTransactionStatus.CSVFile.InputStream))
            {
                while (!csvreader.EndOfStream)
                {
                    string[] Fields = CSVParser.Split(csvreader.ReadLine());
                    if (Fields.Length != 1)
                    {
                        bulkBillTransactionStatus.CSVFile = null;//dispose
                        return Tuple.Create(false, $"Some row are not valid. Please check again.<br/> Operation aborted.");
                    }

                    if (string.IsNullOrWhiteSpace(Fields[0]))
                    {
                        continue;
                    }

                    if (!billNoList.Contains(Fields[0]))
                    {
                        billNoList.Add(Fields[0].Trim().Replace(" ", ""));
                    }
                }
            }
            #endregion


            int result =
                billNoList.IsNotNullOrEmpty() ?
                new BillingDB().UpdateBillStatusPaid(billNoList, bulkBillTransactionStatus.TransactionDate)
                : 0;

            if (result == 0)
                return Tuple.Create(false, $"Changes not save. Please make sure bills status is Dispatched to Accounts Section and are already not Paid.");

            return Tuple.Create(true, $"Bill Status Changed Succesfully to {BillStatus.Paid.GetEnumDescription()} for {result} bills");
        }


        public Tuple<bool, string> Delete(Guid Bill_ID)
        {
            if (Bill_ID == Guid.Empty)
                return Tuple.Create(false, "invalid details");

            BIL_BillDetails existingBill = GetBillByBillID(Bill_ID);

            if (existingBill == null)
                return Tuple.Create(false, "Bill does not exist");

            if (existingBill.BillStatus == BillStatus.Paid)
                return Tuple.Create(false, "No allowed to delete paid bills");

            int result = new BillingDB().Delete(Bill_ID);

            if (result <= 0)
                return Tuple.Create(false, "Bill not deleted. Please try again");

            return Tuple.Create(false, $"Bill Deleted Succesfully");
        }
        public Tuple<bool, string> Delete(string BillNo)
        {
            if (string.IsNullOrWhiteSpace(BillNo))
                return Tuple.Create(false, "invalid details");

            List<BIL_BillDetails> existingBill = GetBill(BillNo);

            if (existingBill == null)
                return Tuple.Create(false, "Bill does not exist");

            if (existingBill.Any(x => x.BillStatus == BillStatus.Paid))
                return Tuple.Create(false, "No allowed to delete paid bills");

            int result = new BillingDB().Delete(BillNo);

            if (result <= 0)
                return Tuple.Create(false, "Bill not deleted. Please try again");

            return Tuple.Create(false, $"Bill Deleted Succesfully");
        }
        #endregion

        #region BankAccount Details
        public BIL_BankAccountDetails GetBankAccountByUserID(Guid User_ID)
        {
            if (User_ID == Guid.Empty)
                return null;

            return new BillingDB().GetBankAccountByUserID(User_ID);
        }

        public Tuple<bool, string> SaveUpdateBankAccount(BIL_BankAccountDetails bankAccountDetails)
        {
            if (bankAccountDetails == null)
                return Tuple.Create(false, "Invalid Details");

            if (bankAccountDetails.User_ID == Guid.Empty)
                return Tuple.Create(false, "Invalid Details");

            BIL_BankAccountDetails existingBankAccount = GetBankAccountByUserID(bankAccountDetails.User_ID);

            int result = 0;
            if (existingBankAccount == null)
            {
                bankAccountDetails.CreatedBy = AppUserHelper.User_ID;
                bankAccountDetails.CreatedOn = DateTime.Now;
                bankAccountDetails.UpdatedOn = null;
                bankAccountDetails.Account_ID = Guid.NewGuid();
                bankAccountDetails.IFSCode = bankAccountDetails.IFSCode.ToUpper();
                bankAccountDetails.IsEditable = true;
                bankAccountDetails.BankName = bankAccountDetails.BankName.ToUpper();
                bankAccountDetails.Branch = bankAccountDetails.Branch.ToUpper();

                result = new BillingDB().Save(bankAccountDetails);
            }
            else
            {
                if (existingBankAccount.IsEditable)
                {
                    bankAccountDetails.UpdatedOn = DateTime.Now;
                    bankAccountDetails.Account_ID = existingBankAccount.Account_ID;
                    bankAccountDetails.IFSCode = bankAccountDetails.IFSCode.ToUpper();
                    bankAccountDetails.IsEditable = true;
                    bankAccountDetails.BankName = bankAccountDetails.BankName.ToUpper();
                    bankAccountDetails.Branch = bankAccountDetails.Branch.ToUpper();

                    result = new BillingDB().Update(bankAccountDetails);
                }
                else
                    return Tuple.Create(false, "Editing not Allowed. For any queries, please contact University Examination Section.");
            }

            if (result <= 0)
                return Tuple.Create(false, "Changes not Saved. Please try again");

            return Tuple.Create(true, $"Changes Saved Successfully");
        }
        public Tuple<bool, string> UpdateBankAccountStatus(Guid User_ID, bool isEditable)
        {
            if (User_ID == Guid.Empty)
                return Tuple.Create(false, "Invalid Details");

            int result = new BillingDB().UpdateBankAccountStatus(User_ID, isEditable);

            if (result <= 0)
                return Tuple.Create(false, "Changes not Saved. Please try again");

            return Tuple.Create(false, $"Changes Saved Successfully");
        }

        #endregion

        #region PaperSetterInstitute

        public BIL_PaperSetterInstitute GetUserInstitute(Guid User_ID)
        {
            if (User_ID == Guid.Empty)
                return null;

            return new BillingDB().GetUserInstitute(User_ID);
        }
        public Tuple<bool, string> SaveUpdatePaperSetterInstitute(BIL_PaperSetterInstitute paperSetterInstitute)
        {
            if (paperSetterInstitute == null)
                return Tuple.Create(false, "Invalid Details");

            if (paperSetterInstitute.User_ID == Guid.Empty)
                return Tuple.Create(false, "Invalid Details");

            int result = new BillingDB().Save(paperSetterInstitute);

            if (result <= 0)
                return Tuple.Create(false, "Changes not Saved. Please try again");

            return Tuple.Create(false, $"Changes Saved Successfully");
        }
        #endregion

        #region PaperSetterSettings

        public Tuple<bool, string> Save(BIL_PaperSetterSettings paperSetterSettings)
        {
            if (paperSetterSettings == null)
                return Tuple.Create(false, "Invalid Details");

            if (paperSetterSettings.FileToUpload == null)
                return Tuple.Create(false, "File is required");

            paperSetterSettings.Setting_ID = Guid.NewGuid();
            paperSetterSettings.CreatedBy = AppUserHelper.User_ID;
            paperSetterSettings.CreatedOn = DateTime.Now;


            paperSetterSettings.FilePath = GeneralFunctions.GetBillingPDFFolderPath();
            if (!Directory.Exists(HostingEnvironment.MapPath("~" + paperSetterSettings.FilePath)))
            {
                Directory.CreateDirectory(HostingEnvironment.MapPath("~" + paperSetterSettings.FilePath));
            }

            paperSetterSettings.FilePath +=
                $"{paperSetterSettings.Type.GetEnumDescription()}_" + Guid.NewGuid() + Path.GetExtension(paperSetterSettings.FileToUpload.FileName);

            paperSetterSettings.FileToUpload.SaveAs(HostingEnvironment.MapPath("~" + paperSetterSettings.FilePath));

            int result = new BillingDB().Save(paperSetterSettings);

            if (result <= 0)
                return Tuple.Create(false, "Setting not saved. Please try again");

            return Tuple.Create(true, $"Setting Saved Succesfully");
        }
        public Tuple<bool, string> UpdatePaperSetterSettingStatus(Guid Setting_ID, bool isActive)
        {
            if (Setting_ID == Guid.Empty)
                return Tuple.Create(false, "invalid details");

            int result = new BillingDB().UpdatePaperSetterSettingStatus(Setting_ID, isActive);

            if (result <= 0)
                return Tuple.Create(false, "Status not changed. Please try again");

            return Tuple.Create(true, $"Status Changed Succesfully");
        }
        public Tuple<bool, string> DeletePaperSetterSetting(Guid Setting_ID)
        {
            if (Setting_ID == Guid.Empty)
                return Tuple.Create(false, "invalid details");

            BillingDB billingDB = new BillingDB();
            bool isSettingUsed = billingDB.IsSettingUsed(Setting_ID);

            if (isSettingUsed)
                return Tuple.Create(false, "Setting is already in used, however can be made inActive");

            BIL_PaperSetterSettings paperSetterSettings = billingDB.GetPaperSetterSetting(Setting_ID);
            int result = billingDB.DeletePaperSetterSetting(Setting_ID);

            if (result <= 0)
                return Tuple.Create(false, "Setting not deleted. Please try again");

            if (paperSetterSettings != null)
            {
                if (!File.Exists(HostingEnvironment.MapPath("~" + paperSetterSettings.FilePath)))
                {
                    File.Delete(HostingEnvironment.MapPath("~" + paperSetterSettings.FilePath));
                }
            }
            return Tuple.Create(true, $"Setting Deleted Succesfully");
        }

        public List<BIL_PaperSetterSettings> GetPaperSetterSettings(Parameters parameter)
        {
            if (parameter == null)
                return null;

            return new BillingDB().GetPaperSetterSettings(parameter);
        }
        public BIL_PaperSetterSettings GetPaperSetterSetting(Guid Setting_ID)
        {
            if (Setting_ID == Guid.Empty)
                return null;

            return new BillingDB().GetPaperSetterSetting(Setting_ID);
        }


        public IEnumerable<SelectListItem> PaperPatterFileDDL(SetterFileType setterFileType)
        {
            IEnumerable<SelectListItem> list = new BillingDB().PaperPatterFileDDL(setterFileType);
            return list == null ? new List<SelectListItem>() : list;
        }
        public IEnumerable<SelectListItem> BillNoDDL(Parameters parameter)
        {
            return new BillingDB().BillNoDDL(parameter)?.DistinctBy(x => x.Value);
        }
        public IEnumerable<SelectListItem> SubjectDepartmentsDDL()
        {
            return new BillingDB().SubjectDepartmentsDDL();
        }

        #endregion

        #region Billing Authorization Roles
        public Tuple<bool, string> SaveUpdateUserRoles(Guid User_ID, List<AppRoles> userRoles)
        {
            if (userRoles.IsNullOrEmpty() || User_ID == Guid.Empty)
                return Tuple.Create(false, "Invalid Details");

            AppUsersDB appUsersDB = new AppUsersDB();

            AppUsers appUser = appUsersDB.GetUserById(User_ID);
            if (appUser == null)
                return Tuple.Create(false, "User does not exist");

            int result = 0;
            new BillingDB().DeleteUserRoles(User_ID, userRoles);

            foreach (AppRoles roleID in userRoles)
            {
                AppUserRoles role = new AppUserRoles()
                {
                    RoleID = roleID,
                    RoleName = roleID.ToString(),
                    User_ID = User_ID
                };

                role.SetWorkFlow(RecordState.New);
                result += appUsersDB.AddUserRole(role);
            }

            if (result <= 0)
                return Tuple.Create(false, "Roles not updated. Please try again");

            return Tuple.Create(true, $"Roles added Succesfully");
        }

        public Tuple<bool, string> DeleteUserRoles(Guid User_ID, List<AppRoles> userRoles)
        {
            if (User_ID == Guid.Empty)
                return Tuple.Create(false, "Invalid Details");

            AppUsers appUser = new AppUsersDB().GetUserById(User_ID);
            if (appUser == null)
                return Tuple.Create(false, "User does not exist");

            if (!appUser.Status)
                return Tuple.Create(false, "User is in-Active");

            int result = new BillingDB().DeleteUserRoles(User_ID, userRoles);

            if (result <= 0)
                return Tuple.Create(false, "Possible reason: bill is associated with user, hence cannot remove the role. Delete bill first then try again.");

            return Tuple.Create(false, $"Roles deleted Succesfully");
        }

        public Tuple<bool, string> DeleteUser(Guid User_ID)
        {
            if (User_ID == Guid.Empty)
                return Tuple.Create(false, "Invalid Details");

            BIL_BillDetails bill = new BillingDB().GetBill(User_ID);

            if (bill != null)
                return Tuple.Create(false, "User cannot be deleted because some bills are associated with it.");


            int result = new BillingDB().DeleteUser(User_ID);

            if (result <= 0)
                return Tuple.Create(false, "Possible reason: bill is associated with user, hence cannot remove the role. Delete bill first then try again");

            return Tuple.Create(false, $"User deleted Succesfully");
        }

        public List<BillStatistics> GetBillStatistics(Parameters parameters)
        {
            return new BillingDB().GetBillStatistics(parameters);
        }

        #endregion
    }
}
