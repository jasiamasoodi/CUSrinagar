using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using CUSrinagar.Models;
using GeneralModels;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using Terex;

namespace CUSrinagar.DataManagers
{
    public class BillingDB
    {
        #region Billing Details
        private string billDetailSQL = $@"SELECT bd.*,au.UserName,
                                            au.FullName,
                                            (sm.SubjectFullName + ' - ' + dbo.FNSubjectTypeDescription(sm.SubjectType)) AS SubjectFullName,
	                                        sm.Semester,sm.Department_ID AS Department_Id,d.DepartmentFullName,a.PhoneNumber,
                                            sm.SubjectType,sm.Programme
                                    FROM dbo.BIL_BillDetails bd
                                        JOIN dbo.AppUsers au
                                            ON au.User_ID = bd.User_ID
                                        JOIN dbo.ADMSubjectMaster sm ON sm.Subject_ID = bd.Subject_ID
	                                    JOIN dbo.Department d ON d.Department_ID = sm.Department_ID
                                        JOIN dbo.AppUsers a ON bd.User_ID=a.User_ID ";

        public List<BIL_BillDetails> GetBills(Parameters parameter)
        {
            GeneralFunctions generalFunctions = new GeneralFunctions();
            string query = billDetailSQL;
            FilterHelper helper = generalFunctions.GetWhereClause<BIL_BillDetails>(parameter.Filters);
            if (string.IsNullOrWhiteSpace(parameter.SortInfo?.ColumnName))
            {
                if (parameter.SortInfo == null)
                {
                    parameter.SortInfo = new Sort();
                }
                parameter.SortInfo.ColumnName = "bd.CreatedOn";
                parameter.SortInfo.OrderBy = SortOrder.Descending;
            }
            string pagedAndOrderByClause = generalFunctions.GetPagedQuery(query, parameter);

            helper.Command.CommandText = $"{query} {helper.WhereClause} {pagedAndOrderByClause}";
            return new MSSQLFactory().GetObjectList<BIL_BillDetails>(helper.Command);
        }

        public List<BIL_BillDetails> GetBills(Guid User_ID, BillType billType)
        {
            return new MSSQLFactory()
                .GetObjectList<BIL_BillDetails>($@"{billDetailSQL} WHERE bd.BillType={(short)billType} AND bd.User_ID='{User_ID}';");
        }
        public BIL_BillDetails GetBill(Guid User_ID)
        {
            return new MSSQLFactory()
                 .GetObject<BIL_BillDetails>($@"{billDetailSQL} WHERE bd.User_ID='{User_ID}';");
        }
        public BIL_BillDetails GetBill(Guid Subject_ID, int Batch, BillType BillType, Guid User_ID)
        {
            return new MSSQLFactory()
                 .GetObject<BIL_BillDetails>($@"{billDetailSQL} WHERE bd.Subject_ID='{Subject_ID}' AND Bd.User_ID='{User_ID}' AND Batch like '{Batch}%' AND BillType={(int)BillType};");
        }
        public List<BIL_BillDetails> GetBill(string BillNo)
        {
            SqlCommand sql = new SqlCommand($@"SELECT bd.*,au.UserName,
                                   au.FullName,
                                   (sm.SubjectFullName + ' - ' + dbo.FNSubjectTypeDescription(sm.SubjectType)) AS SubjectFullName,
	                               sm.Semester,sm.Department_ID AS Department_Id,d.DepartmentFullName,a.PhoneNumber,
                                   sm.SubjectType,sm.Programme
                            FROM dbo.BIL_BillDetails bd
                                JOIN dbo.AppUsers au
                                    ON au.User_ID = bd.User_ID
                                JOIN dbo.ADMSubjectMaster sm ON sm.Subject_ID = bd.Subject_ID
	                            JOIN dbo.Department d ON d.Department_ID = sm.Department_ID
                                JOIN dbo.AppUsers a ON bd.User_ID=a.User_ID
                                     WHERE bd.BillNo=@BillNo order by Examination ASC;");
            sql.Parameters.AddWithValue("@BillNo", BillNo);

            return new MSSQLFactory()
                .GetObjectList<BIL_BillDetails>(sql);
        }
        public BIL_BillDetails GetBillByBillID(Guid Bill_ID)
        {
            SqlCommand sql = new SqlCommand($@"{billDetailSQL} WHERE bd.Bill_ID=@Bill_ID");
            sql.Parameters.AddWithValue("@Bill_ID", Bill_ID);

            return new MSSQLFactory()
                .GetObject<BIL_BillDetails>(sql);
        }

        public int Save(BIL_BillDetails billDetails)
        {
            try
            {
                return new MSSQLFactory().InsertRecord(billDetails);
            }
            catch (SqlException)
            {
                return 0;
            }
        }


        public int Update(BIL_BillDetails billDetails)
        {
            try
            {
                string tSQL = $@"UPDATE dbo.BIL_BillDetails
                                SET 
	                                BillNo=@BillNo,
	                                User_ID=@User_ID,
	                                SamplePaper_ID=@SamplePaper_ID,
	                                PaperPattern_ID=@PaperPattern_ID,
	                                Subject_ID=@Subject_ID,
	                                Batch=@Batch,
	                                Examination=@Examination,
	                                Session=@Session,
	                                AmountPerSet=@AmountPerSet,
	                                ConveyanceCharges=@ConveyanceCharges,
	                                NoOfSets=@NoOfSets,
	                                TotalAssignmentCompletionDays=@TotalAssignmentCompletionDays,
	                                PaperReceiverEmail=@PaperReceiverEmail,
	                                SyllabusLink=@SyllabusLink,
	                                Institute=@Institute,
	                                UpdatedOn=@UpdatedOn,
	                                UpdatedBy=@UpdatedBy
                                    WHERE Bill_ID=@Bill_ID AND BillStatus<>@BillStatus";

                SqlCommand cmd = new SqlCommand(tSQL);

                cmd.Parameters.AddWithValue("@BillNo", billDetails.BillNo);
                cmd.Parameters.AddWithValue("@User_ID", billDetails.User_ID);
                cmd.Parameters.AddWithValue("@SamplePaper_ID", billDetails.SamplePaper_ID);
                cmd.Parameters.AddWithValue("@PaperPattern_ID", billDetails.PaperPattern_ID);
                cmd.Parameters.AddWithValue("@Subject_ID", billDetails.Subject_ID);
                cmd.Parameters.AddWithValue("@Batch", billDetails.Batch);
                cmd.Parameters.AddWithValue("@Examination", billDetails.Examination);
                cmd.Parameters.AddWithValue("@Session", billDetails.Session);
                cmd.Parameters.AddWithValue("@AmountPerSet", billDetails.AmountPerSet);
                cmd.Parameters.AddWithValue("@NoOfSets", billDetails.NoOfSets);
                cmd.Parameters.AddWithValue("@TotalAssignmentCompletionDays", billDetails.TotalAssignmentCompletionDays);
                cmd.Parameters.AddWithValue("@PaperReceiverEmail", billDetails.PaperReceiverEmail);
                cmd.Parameters.AddWithValue("@SyllabusLink", billDetails.SyllabusLink);
                cmd.Parameters.AddWithValue("@Institute", billDetails.Institute?.ToUpper());
                cmd.Parameters.AddWithValue("@UpdatedOn", billDetails.UpdatedOn);
                cmd.Parameters.AddWithValue("@UpdatedBy", billDetails.UpdatedBy);
                if (billDetails.ConveyanceCharges == null)
                {
                    cmd.Parameters.AddWithValue("@ConveyanceCharges", DBNull.Value);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@ConveyanceCharges", billDetails.ConveyanceCharges);
                }

                cmd.Parameters.AddWithValue("@Bill_ID", billDetails.Bill_ID);
                cmd.Parameters.AddWithValue("@BillStatus", (short)BillStatus.Paid);

                return new MSSQLFactory().ExecuteNonQuery(cmd);
            }
            catch (SqlException)
            {
                return 0;
            }
        }
        public int UpdateBillStatus(string BillNo, BillStatus billStatus)
        {
            try
            {
                string tSQL = $@"UPDATE dbo.BIL_BillDetails
                                SET BillStatus = @BillStatusToChange,
	                                UpdatedOn=@UpdatedOn,
	                                UpdatedBy=@UpdatedBy
                                WHERE BillNo = @BillNo
                                      AND BillStatus <> @BillStatus;";

                SqlCommand cmd = new SqlCommand(tSQL);

                cmd.Parameters.AddWithValue("@BillNo", BillNo);
                cmd.Parameters.AddWithValue("@BillStatusToChange", (short)billStatus);
                cmd.Parameters.AddWithValue("@BillStatus", (short)BillStatus.Paid);
                cmd.Parameters.AddWithValue("@UpdatedOn", DateTime.Now);
                cmd.Parameters.AddWithValue("@UpdatedBy", AppUserHelper.User_ID);

                return new MSSQLFactory().ExecuteNonQuery(cmd);
            }
            catch (SqlException)
            {
                return 0;
            }
        }

        public int UpdateBillStatusPaid(List<string> BillNo, DateTime transactionDate)
        {
            try
            {
                string tSQL = $@"UPDATE dbo.BIL_BillDetails
                                SET BillStatus = {(short)BillStatus.Paid},
                                    PaymentDate=@PaymentDate,
	                                UpdatedOn=@UpdatedOn,
	                                UpdatedBy=@UpdatedBy
                                WHERE BillNo IN({BillNo.ToIN()})
                                      AND BillStatus = @BillStatus;";

                SqlCommand cmd = new SqlCommand(tSQL);

                cmd.Parameters.AddWithValue("@PaymentDate", transactionDate);
                cmd.Parameters.AddWithValue("@BillStatus", (short)BillStatus.DispatchedToAccountsSection);
                cmd.Parameters.AddWithValue("@UpdatedOn", DateTime.Now);
                cmd.Parameters.AddWithValue("@UpdatedBy", AppUserHelper.User_ID);

                return new MSSQLFactory().ExecuteNonQuery(cmd);
            }
            catch (SqlException)
            {
                return 0;
            }
        }

        public int UpdateRevenueStampCharges(string BillNo, short amount)
        {
            try
            {
                string tSQL = $@"UPDATE dbo.BIL_BillDetails
                                SET RevenueStampAmount = @RevenueStampAmount,
	                                UpdatedOn=@UpdatedOn,
	                                UpdatedBy=@UpdatedBy
                                WHERE BillNo = @BillNo
                                      AND BillStatus <> @BillStatus;";

                SqlCommand cmd = new SqlCommand(tSQL);

                cmd.Parameters.AddWithValue("@BillNo", BillNo);
                cmd.Parameters.AddWithValue("@RevenueStampAmount", amount);
                cmd.Parameters.AddWithValue("@BillStatus", (short)BillStatus.Paid);
                cmd.Parameters.AddWithValue("@UpdatedOn", DateTime.Now);
                cmd.Parameters.AddWithValue("@UpdatedBy", AppUserHelper.User_ID);

                return new MSSQLFactory().ExecuteNonQuery(cmd);
            }
            catch (SqlException)
            {
                return 0;
            }
        }


        public int Delete(Guid Bill_ID)
        {
            try
            {
                string tSQL = $@"DELETE FROM dbo.BIL_BillDetails
                                WHERE Bill_ID = @Bill_ID
                                        AND BillStatus <> @BillStatus;";

                SqlCommand cmd = new SqlCommand(tSQL);

                cmd.Parameters.AddWithValue("@Bill_ID", Bill_ID);
                cmd.Parameters.AddWithValue("@BillStatus", (short)BillStatus.Paid);

                return new MSSQLFactory().ExecuteNonQuery(cmd);
            }
            catch (SqlException)
            {
                return 0;
            }
        }
        public int Delete(string BillNo)
        {
            try
            {
                string tSQL = $@"DELETE FROM dbo.BIL_BillDetails
                                WHERE BillNo = @BillNo
                                        AND BillStatus <> @BillStatus;";

                SqlCommand cmd = new SqlCommand(tSQL);

                cmd.Parameters.AddWithValue("@BillNo", BillNo);
                cmd.Parameters.AddWithValue("@BillStatus", (short)BillStatus.Paid);

                return new MSSQLFactory().ExecuteNonQuery(cmd);
            }
            catch (SqlException)
            {
                return 0;
            }
        }

        #endregion

        #region BankAccount Details
        public int Save(BIL_BankAccountDetails bankAccountDetails)
        {
            try
            {
                return new MSSQLFactory().InsertRecord(bankAccountDetails);
            }
            catch (SqlException)
            {
                return 0;
            }
        }

        public int Update(BIL_BankAccountDetails bankAccountDetails)
        {
            try
            {
                string tSQL = $@"UPDATE dbo.BIL_BankAccountDetails
                                        SET BankName = @BankName,
                                            Branch = @Branch,
                                            AccountNo = @AccountNo,
                                            IFSCode = @IFSCode,
                                            UpdatedOn = @UpdatedOn
                                        WHERE Account_ID = @Account_ID;";

                SqlCommand cmd = new SqlCommand(tSQL);

                cmd.Parameters.AddWithValue("@BankName", bankAccountDetails.BankName.ToUpper().Trim());
                cmd.Parameters.AddWithValue("@Branch", bankAccountDetails.Branch.ToUpper().Trim());
                cmd.Parameters.AddWithValue("@AccountNo", bankAccountDetails.AccountNo.Trim());
                cmd.Parameters.AddWithValue("@IFSCode", bankAccountDetails.IFSCode.ToUpper().Trim());
                cmd.Parameters.AddWithValue("@UpdatedOn", DateTime.Now);
                cmd.Parameters.AddWithValue("@Account_ID", bankAccountDetails.Account_ID);

                return new MSSQLFactory().ExecuteNonQuery(cmd);
            }
            catch (SqlException)
            {
                return 0;
            }
        }

        public int UpdateBankAccountStatus(Guid user_ID, bool isEditable)
        {
            try
            {
                string tSQL = $@"UPDATE
	                                dbo.BIL_BankAccountDetails
                                SET
	                                IsEditable=@IsEditable,
	                                UpdatedOn=@UpdatedOn
                                WHERE User_ID=@User_ID;";

                SqlCommand cmd = new SqlCommand(tSQL);

                cmd.Parameters.AddWithValue("@IsEditable", isEditable);
                cmd.Parameters.AddWithValue("@UpdatedOn", DateTime.Now);
                cmd.Parameters.AddWithValue("@User_ID", user_ID);

                return new MSSQLFactory().ExecuteNonQuery(cmd);
            }
            catch (SqlException)
            {
                return 0;
            }
        }

        public BIL_BankAccountDetails GetBankAccountByUserID(Guid User_ID)
        {
            return new MSSQLFactory()
                .GetObject<BIL_BankAccountDetails>($@"SELECT * FROM dbo.BIL_BankAccountDetails WHERE User_ID='{User_ID}';");
        }

        #endregion

        #region PaperSetterInstitute

        public int Save(BIL_PaperSetterInstitute paperSetterInstitute)
        {
            try
            {
                MSSQLFactory mSSQLFactory = new MSSQLFactory();
                mSSQLFactory.ExecuteNonQuery($@"DELETE FROM dbo.BIL_PaperSetterInstitute WHERE User_ID='{paperSetterInstitute.User_ID}';");
                return mSSQLFactory.InsertRecord(paperSetterInstitute);
            }
            catch (SqlException)
            {
                return 0;
            }
        }

        public BIL_PaperSetterInstitute GetUserInstitute(Guid User_ID)
        {
            return new MSSQLFactory().GetObject<BIL_PaperSetterInstitute>($@"SELECT * FROM dbo.BIL_PaperSetterInstitute WHERE User_ID='{User_ID}';");
        }

        #endregion

        #region PaperSetterSettings

        public int Save(BIL_PaperSetterSettings paperSetterSettings)
        {
            try
            {
                return new MSSQLFactory().InsertRecord(paperSetterSettings);
            }
            catch (SqlException)
            {
                return 0;
            }
        }

        public List<BIL_PaperSetterSettings> GetPaperSetterSettings(Parameters parameter)
        {
            GeneralFunctions generalFunctions = new GeneralFunctions();
            string query = $@"SELECT * FROM dbo.BIL_PaperSetterSettings";
            FilterHelper helper = generalFunctions.GetWhereClause<BIL_PaperSetterSettings>(parameter.Filters);

            if (string.IsNullOrWhiteSpace(parameter.SortInfo?.ColumnName))
            {
                if (parameter.SortInfo == null)
                { parameter.SortInfo = new Sort(); }

                parameter.SortInfo.ColumnName = "CreatedOn";
                parameter.SortInfo.OrderBy = SortOrder.Descending;
            }

            string pagedAndOrderByClause = generalFunctions.GetPagedQuery(query, parameter);

            helper.Command.CommandText = $"{query} {helper.WhereClause} {pagedAndOrderByClause}";
            return new MSSQLFactory().GetObjectList<BIL_PaperSetterSettings>(helper.Command);
        }

        public BIL_PaperSetterSettings GetPaperSetterSetting(Guid setting_ID)
        {
            return new MSSQLFactory()
                .GetObject<BIL_PaperSetterSettings>($@"SELECT * FROM dbo.BIL_PaperSetterSettings WHERE Setting_ID='{setting_ID}';");
        }

        public bool IsSettingUsed(Guid Setting_ID)
        {
            return new MSSQLFactory()
                .ExecuteScalar<int>($@"SELECT COUNT(Bill_ID) FROM dbo.BIL_BillDetails WHERE (PaperPattern_ID='{Setting_ID}' OR SamplePaper_ID='{Setting_ID}');") > 0;
        }

        public int DeletePaperSetterSetting(Guid setting_ID)
        {
            return new MSSQLFactory().ExecuteNonQuery($@"Delete FROM dbo.BIL_PaperSetterSettings WHERE Setting_ID='{setting_ID}';");
        }

        public int UpdatePaperSetterSettingStatus(Guid setting_ID, bool isActive)
        {
            try
            {
                string tSQL = $@"UPDATE dbo.BIL_PaperSetterSettings 
                                SET IsActive=@IsActive                              
                                 WHERE Setting_ID=@setting_ID;";

                SqlCommand cmd = new SqlCommand(tSQL);

                cmd.Parameters.AddWithValue("@IsActive", isActive);
                cmd.Parameters.AddWithValue("@setting_ID", setting_ID);

                return new MSSQLFactory().ExecuteNonQuery(cmd);
            }
            catch (SqlException)
            {
                return 0;
            }
        }

        public IEnumerable<SelectListItem> PaperPatterFileDDL(SetterFileType setterFileType)
        {
            return new MSSQLFactory()
                .GetObjectList<SelectListItem>($@"SELECT CAST(Setting_ID AS VARCHAR(100)) AS [Value],
                                                       Title AS [Text], CreatedOn
                                                FROM dbo.BIL_PaperSetterSettings
                                                WHERE [Type] = {(short)setterFileType}
                                                      AND IsActive = 1
                                                ORDER BY CreatedOn DESC;");
        }

        public IEnumerable<SelectListItem> BillNoDDL(Parameters parameter)
        {
            GeneralFunctions generalFunctions = new GeneralFunctions();
            string query = $@"SELECT DISTINCT
                                           BillNo AS [Value],
                                           BillNo + ' (' + Examination + ' - ' + [Session] + ')'  AS [Text]
                                           ,BIL_BillDetails.BillNo,BIL_BillDetails.CreatedOn
                                    FROM dbo.BIL_BillDetails 
                                    JOIN dbo.ADMSubjectMaster SM ON SM.Subject_ID = BIL_BillDetails.Subject_ID";
            FilterHelper helper = generalFunctions.GetWhereClause<BIL_BillDetails>(parameter.Filters);
            if (string.IsNullOrWhiteSpace(parameter.SortInfo?.ColumnName))
            {
                if (parameter.SortInfo == null)
                { parameter.SortInfo = new Sort(); }

                parameter.SortInfo.ColumnName = "BIL_BillDetails.CreatedOn";
                parameter.SortInfo.OrderBy = SortOrder.Descending;
            }
            string pagedAndOrderByClause = generalFunctions.GetPagedQuery(query, parameter);

            helper.Command.CommandText = $"{query} {helper.WhereClause} AND BillStatus={(short)BillStatus.InProcess} AND SM.Status=1 AND BillType={(short)BillType.PaperSetterBill} {pagedAndOrderByClause}";
            return new MSSQLFactory().GetObjectList<SelectListItem>(helper.Command);
        }

        public IEnumerable<SelectListItem> SubjectDepartmentsDDL()
        {
            return new MSSQLFactory()
                .GetObjectList<SelectListItem>($@"SELECT DISTINCT
                                                       CAST(D.Department_ID AS VARCHAR(100)) AS [Value],
                                                       D.DepartmentFullName AS [Text]
                                                FROM dbo.Department D
                                                    JOIN dbo.ADMSubjectMaster sm
                                                        ON sm.Department_ID = D.Department_ID
                                                WHERE sm.Status = 1
                                                ORDER BY D.DepartmentFullName ASC;");
        }

        public int DeleteUserRoles(Guid user_ID, List<AppRoles> userRoles)
        {
            if (userRoles.Any(x => x == AppRoles.EvaluatorBills || x == AppRoles.PaperSetterBills))
            {
                return new MSSQLFactory()
                .ExecuteNonQuery($@"DELETE r FROM  dbo.AppUserRoles r
                                    LEFT JOIN dbo.BIL_BillDetails d ON d.User_ID = r.User_ID
                                    WHERE d.User_ID IS NULL AND 
                                    r.User_ID='{user_ID}' AND r.RoleID IN({userRoles.Select(x => (short)x).EnumToIN()});");
            }
            return new MSSQLFactory()
                .ExecuteNonQuery($@"DELETE FROM dbo.AppUserRoles 
                WHERE User_ID='{user_ID}' AND RoleID IN({userRoles.Select(x => (short)x).EnumToIN()});");
        }

        public int DeleteUser(Guid user_ID)
        {
            try
            {
                SqlCommand sql = new SqlCommand($@"DELETE FROM dbo.BIL_PaperSetterInstitute WHERE User_ID=@User_ID;
                                                    DELETE FROM dbo.BIL_BankAccountDetails WHERE User_ID=@User_ID;
                                                    DELETE FROM dbo.AppUserRoles WHERE User_ID=@User_ID;
                                                    DELETE FROM dbo.AppUsers WHERE User_ID=@User_ID;");

                sql.Parameters.AddWithValue("@User_ID", user_ID);
                return new MSSQLFactory().ExecuteNonQuery(sql);
            }
            catch (SqlException)
            {
                return 0;
            }
        }

        public int SaveUpdateBankAccountInBillDetails(string BillNo)
        {
            try
            {
                string tSQL = $@"UPDATE bd SET bd.UpdatedOn=GETDATE(),bd.UpdatedBy=@UpdatedBy,bd.PaymentAccount=a.AccountNo,
                                bd.PaymentIFSCode=a.IFSCode,bd.PaymentBranch=a.Branch,bd.PaymentBank=a.BankName
                                from dbo.BIL_BillDetails bd
                                JOIN dbo.BIL_BankAccountDetails a ON a.User_ID = bd.User_ID
                                WHERE bd.BillNo=@BillNo AND bd.BillStatus=@BillStatus";

                SqlCommand cmd = new SqlCommand(tSQL);

                cmd.Parameters.AddWithValue("@BillStatus", (short)BillStatus.DispatchedToAccountsSection);
                cmd.Parameters.AddWithValue("@BillNo", BillNo);
                cmd.Parameters.AddWithValue("@UpdatedBy", AppUserHelper.User_ID);

                return new MSSQLFactory().ExecuteNonQuery(cmd);
            }
            catch (SqlException)
            {
                return 0;
            }
        }

        public List<BillStatistics> GetBillStatistics(Parameters parameters)
        {
            GeneralFunctions generalFunctions = new GeneralFunctions();
            FilterHelper helper = generalFunctions.GetWhereClause<BIL_BillDetails>(parameters.Filters);

            helper.Command.CommandText = $@"SELECT COUNT(BillStatus) TotalBills,BillStatus FROM (
                                SELECT DISTINCT BillNo,BillStatus FROM dbo.BIL_BillDetails {helper.WhereClause}
                                ) s GROUP BY s.BillStatus ";

            return new MSSQLFactory().GetObjectList<BillStatistics>(helper.Command);
        }

        #endregion
    }
}
