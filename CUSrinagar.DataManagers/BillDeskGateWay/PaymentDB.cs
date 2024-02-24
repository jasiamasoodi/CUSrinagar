using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Terex;
using CUSrinagar.Models;
using System.Data.SqlClient;
using CUSrinagar.Extensions;
using CUSrinagar.Enums;

namespace CUSrinagar.DataManagers
{
    public class PaymentDB
    {
        public void SaveBDRequest(string msg, string oAuthToken, Guid Student_ID)
        {
            SqlCommand command = new SqlCommand();
            command.Parameters.AddWithValue("@Request", msg);
            command.Parameters.AddWithValue("@CreatedOn", DateTime.Now);
            command.Parameters.AddWithValue("@Student_ID", Student_ID);
            if (!string.IsNullOrWhiteSpace(oAuthToken))
            {
                command.Parameters.AddWithValue("@oAuthToken", oAuthToken);
                command.CommandText = "INSERT INTO BillDeskRequest (Request,CreatedOn,oAuthToken,Student_ID) VALUES(@Request,@CreatedOn,@oAuthToken,@Student_ID);";
            }
            else
            {
                command.CommandText = "INSERT INTO BillDeskRequest (Request,CreatedOn,oAuthToken,Student_ID) VALUES(@Request,@CreatedOn,NULL,@Student_ID);";
            }
            new MSSQLFactory().ExecuteNonQuery(command);
        }

        public Guid GetStudentIDFromBillDeskRequest(string CustomerID)
        {
            CustomerID = CustomerID?.Replace("--", "")?.Replace(";", "")?.Replace("'", "");

            if (string.IsNullOrWhiteSpace(CustomerID))
                CustomerID = "xxxx";

            string SqlQuery = $"SELECT Student_ID FROM dbo.BillDeskRequest WHERE Request LIKE '{BillDeskSettings.MerchantID}|{CustomerID}|%'";
            return new MSSQLFactory().ExecuteScalar<Guid>(SqlQuery);
        }

        public List<short> GetPaymentCategory()
        {
            return new MSSQLFactory().GetSingleValues<short>("SELECT DISTINCT PrintProgramme FROM [ARGFormNoMaster] WHERE AllowOnlinePayment=1");
        }
        public int Delete(Guid Payment_ID, PrintProgramme programme)
        {
            return new MSSQLFactory().ExecuteNonQuery($"DELETE FROM dbo.PaymentDetails_{programme.ToString()} WHERE Payment_ID='{Payment_ID}';");
        }

        public int SavePayment(PaymentDetails paymentDetails, PrintProgramme programme, PaymentModuleType moduleType)
        {
            paymentDetails.ModuleType = moduleType;
            Module module = Module.ReEvaluation;
            switch (moduleType)
            {
                case PaymentModuleType.None:
                    break;
                case PaymentModuleType.Admission:
                    break;
                case PaymentModuleType.ReEvaluation:
                    module = Module.ReEvaluation;
                    break;
                case PaymentModuleType.Examination:
                    module = Module.ExaminationPayment;
                    break;
                case PaymentModuleType.Xerox:
                    break;
                case PaymentModuleType.SelfFinanced:
                    break;
                case PaymentModuleType.SemesterAdmission:
                    break;
                case PaymentModuleType.CancelRegistration:
                    break;
                case PaymentModuleType.Migration:
                    break;
                default:
                    break;
            }
            var tableName = new GeneralFunctions().GetTableName(programme, module);
            return new MSSQLFactory().InsertRecord(paymentDetails, tableName);
        }

        public List<short> GetPaymentCategory(Guid Student_ID, PrintProgramme printProgramme)
        {
            return new MSSQLFactory().GetSingleValues<short>("SELECT DISTINCT PrintProgramme FROM [ARGFormNoMaster] WHERE AllowOnlinePayment=1");
        }

        public int SavePaymentDetails(PaymentDetails aRGPaymentDetails, PrintProgramme printProgramme)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            return new MSSQLFactory().InsertRecord(aRGPaymentDetails, "PaymentDetails_" + printProgramme.ToString());
        }

        public int Update(PaymentDetails model, PrintProgramme printProgramme)
        {
            List<string> ignoreList = new List<string>() { "Payment_ID" };
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            var sqlCommand = new MSSQLFactory().UpdateRecord(model, ignoreList, ignoreList, $"PaymentDetails_{printProgramme.ToString()}");
            sqlCommand.CommandText = sqlCommand.CommandText + " WHERE Payment_ID=@Payment_ID";
            sqlCommand.Parameters.AddWithValue("@Payment_ID", model.Payment_ID);
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }

        public PaymentDetails GetPaymentDetail(Guid Entity_ID, PaymentModuleType _ModuleType, PrintProgramme printProgramme)
        {
            string query = $@"SELECT * FROM PaymentDetails_{printProgramme.ToString()} WHERE Entity_ID=@Entity_ID AND ModuleType=@ModuleType ORDER BY TxnDate DESC";
            SqlCommand command = new SqlCommand();
            command.Parameters.AddWithValue("@Entity_ID", Entity_ID);
            command.Parameters.AddWithValue("@ModuleType", _ModuleType);
            command.CommandText = query;
            return new MSSQLFactory().GetObject<PaymentDetails>(command);
        }

        public PaymentDetails GetPaymentDetail(Guid Entity_ID, short Semester, PaymentModuleType _ModuleType, PrintProgramme printProgramme)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            string query = $@"SELECT * FROM PaymentDetails_{printProgramme.ToString()} 
                            WHERE Entity_ID='{Entity_ID}' AND ModuleType={(short)_ModuleType} AND Semester={Semester}  ORDER BY TxnDate DESC";
            return new MSSQLFactory().GetObject<PaymentDetails>(query);
        }

        public List<PaymentDetails> GetPaymentDetails(Guid Entity_ID, PaymentModuleType _ModuleType, PrintProgramme printProgramme)
        {
            string query = $@"SELECT * FROM PaymentDetails_{printProgramme.ToString()} WHERE Entity_ID='{Entity_ID}' AND ModuleType={(short)_ModuleType} ORDER BY TxnDate DESC";
            //SqlCommand command = new SqlCommand();
            //command.Parameters.AddWithValue("@Entity_ID", Entity_ID);
            //command.Parameters.AddWithValue("@ModuleType", _ModuleType);
            //command.CommandText = query;
            return new MSSQLFactory().GetObjectList<PaymentDetails>(query)?.DistinctBy(x => x.TxnReferenceNo)?.ToList();
        }
        public List<PaymentDetails> GetPaymentDetails(Guid Entity_ID, PaymentModuleType _ModuleType, PrintProgramme printProgramme, short Semester)
        {
            string query = $@"SELECT * FROM PaymentDetails_{printProgramme.ToString()} WHERE Entity_ID='{Entity_ID}' AND ModuleType={(short)_ModuleType} AND Semester={Semester} ORDER BY TxnDate DESC";
            return new MSSQLFactory().GetObjectList<PaymentDetails>(query);
        }


        public PaymentDetails GetPaymentDetail(Guid Entity_ID, short Semester, PrintProgramme printProgramme)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            string query = $@"SELECT * FROM PaymentDetails_{printProgramme} WHERE Entity_ID=@Entity_ID AND ModuleType=@ModuleType AND Semester=@Semester ORDER BY TxnDate DESC";
            SqlCommand command = new SqlCommand();
            command.Parameters.AddWithValue("@Entity_ID", Entity_ID);
            command.Parameters.AddWithValue("@Semester", Semester);
            command.Parameters.AddWithValue("@ModuleType", PaymentModuleType.SemesterAdmission);
            command.CommandText = query;
            return new MSSQLFactory().GetObject<PaymentDetails>(command);
        }

        public PaymentDetails GetPaymentDetail(Guid Entity_ID, short Semester)
        {
            string query = $@"SELECT * FROM PaymentDetails_{AppUserHelper.TableSuffix.ToString()} WHERE Entity_ID=@Entity_ID AND ModuleType=@ModuleType AND Semester=@Semester ORDER BY TxnDate DESC";
            SqlCommand command = new SqlCommand();
            command.Parameters.AddWithValue("@Entity_ID", Entity_ID);
            command.Parameters.AddWithValue("@Semester", Semester);
            command.Parameters.AddWithValue("@ModuleType", PaymentModuleType.SemesterAdmission);
            command.CommandText = query;
            return new MSSQLFactory().GetObject<PaymentDetails>(command);
        }

        public List<PaymentDetails> GetPaymentDetails(Guid student_ID, PrintProgramme printProgramme)
        {
            string query = $@"SELECT *,FormNumber EntityID FROM dbo.PaymentDetails_{printProgramme.ToString()}
                                INNER JOIN dbo.ReEvaluation ON Entity_ID = ReEvaluation_ID
                                WHERE Student_ID = @student_ID ";
            SqlCommand command = new SqlCommand();
            command.Parameters.AddWithValue("@student_ID", student_ID);
            command.CommandText = query;
            return new MSSQLFactory().GetObjectList<PaymentDetails>(command);
        }

        public bool GetPaymentExistsUG(Guid user_ID, int semester)
        {
            string query = $@"SELECT COUNT(*) FROM PaymentDetails_UG WHERE Entity_ID = @user_ID AND Semester = @semester ";

            SqlCommand command = new SqlCommand();
            command.Parameters.AddWithValue("@user_ID", user_ID);
            command.Parameters.AddWithValue("@semester", semester);
            command.CommandText = query;

            return new MSSQLFactory().ExecuteScalar<int>(command) > 0;
        }

        public bool GetPaymentExists(Guid user_ID, PaymentModuleType paymentModuleType, PrintProgramme printProgramme, int? semester)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            SqlCommand command = new SqlCommand();
            command.Parameters.AddWithValue("@user_ID", user_ID);
            string query = $@"SELECT COUNT(*) FROM PaymentDetails_{printProgramme} WHERE Entity_ID = @user_ID AND ModuleType={(short)paymentModuleType} AND Semester IS NULL";

            if (semester != null)
            {
                command.Parameters.AddWithValue("@semester", semester);
                query = $@"SELECT COUNT(*) FROM PaymentDetails_{printProgramme} WHERE Entity_ID = @user_ID AND ModuleType={(short)paymentModuleType} AND Semester = @semester ";
            }
            command.CommandText = query;
            return new MSSQLFactory().ExecuteScalar<int>(command) > 0;
        }


        public int UpdatePersonalInfoAddMoreCourses(Guid student_ID, decimal TotalFee, PrintProgramme printProgramme, bool hasPayment)
        {
            string query = hasPayment ?
                $@"UPDATE ARGPersonalInformation_{printProgramme.ToString()} SET TotalFee=(TotalFee+{TotalFee}) WHERE Student_ID='{student_ID}'"
                :
                $@"UPDATE ARGPersonalInformation_{printProgramme.ToString()} SET FormStatus={(short)FormStatus.FeePaid},TotalFee={TotalFee} WHERE Student_ID='{student_ID}'";
            return new MSSQLFactory().ExecuteNonQuery(query);
        }


        public int SavePaymentForCertificateCourses(PaymentDetails aRGPaymentDetails)
        {
            return new MSSQLFactory().InsertRecord(aRGPaymentDetails, "PaymentDetails_IH");
        }

        public int Delete(Guid student_ID, int semester, PaymentModuleType paymentModule, PrintProgramme printProgramme)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            return new MSSQLFactory().ExecuteNonQuery($"Delete from paymentdetails_{printProgramme.ToString()} WHERE Entity_ID='{student_ID}' AND Semester={semester} AND ModuleType={(short)paymentModule}");
        }


        #region OAuthTokenValueRoutines
        public string GetoAuthTokenValue(string BDCustomerID)
        {
            BDCustomerID = $"|{BDCustomerID}|";
            string query = $"SELECT TOP 1 oAuthToken FROM dbo.BillDeskRequest WHERE Request LIKE '{BDCustomerID.ToLike()}'";
            try
            {
                return new MSSQLFactory().ExecuteScalar<string>(query);
            }
            catch (SqlException SQLError) when (SQLError.Number == 1205)
            {
                System.Threading.Thread.Sleep(millisecondsTimeout: 630);
                return new MSSQLFactory().ExecuteScalar<string>(query);
            }
        }
        public int DeleteoAuthTokenValue(string BDCustomerID)
        {
            BDCustomerID = $"|{BDCustomerID}|";
            string query = $"UPDATE dbo.BillDeskRequest SET oAuthToken=NULL WHERE Request LIKE '{BDCustomerID.ToLike()}'";
            try
            {
                return new MSSQLFactory().ExecuteNonQuery(query);
            }
            catch (SqlException SQLError) when (SQLError.Number == 1205)
            {
                System.Threading.Thread.Sleep(millisecondsTimeout: 520);
                return new MSSQLFactory().ExecuteNonQuery(query);
            }
        }
        #endregion

    }
}
