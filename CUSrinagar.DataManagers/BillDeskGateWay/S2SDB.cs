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
    public class S2SDB
    {
        private List<BillDeskStoredRequest> CustomerID(S2SSearch _S2SSearch)
        {
            if (_S2SSearch.SearchQuery == Guid.Empty.ToString())
                return null;
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = $"SELECT * FROM BillDeskRequest WHERE Request LIKE @SearchQuery ";
            cmd.Parameters.AddWithValue("@SearchQuery", _S2SSearch.SearchQuery.ToLike());

            if (!string.IsNullOrWhiteSpace(_S2SSearch.TxnDate))
            {
                cmd.CommandText += "AND CAST(CreatedOn AS Date) >=@CreatedOn";
                string[] txndate = _S2SSearch.TxnDate.SplitDate();
                cmd.Parameters.AddWithValue("@CreatedOn", $"{txndate[2]}-{txndate[1]}-{txndate[0]}");
            }
            cmd.CommandText += " ORDER BY CreatedOn DESC";
            return new MSSQLFactory().GetObjectList<BillDeskStoredRequest>(cmd);
        }
        public List<BillDeskStoredRequest> GetCustomerID(S2SSearch _S2SSearch)
        {
            if (_S2SSearch.FeeType == PaymentModuleType.ReEvaluation || _S2SSearch.FeeType == PaymentModuleType.Examination || _S2SSearch.FeeType == PaymentModuleType.Xerox)
                return CustomerID(_S2SSearch);
            else
            {
                _S2SSearch.Course = new GeneralFunctions().MappingTable(_S2SSearch.Course);
                _S2SSearch.SearchQuery = GetStudentID(_S2SSearch) + string.Empty;
                return CustomerID(_S2SSearch);
            }
        }

        public Guid GetStudentID(S2SSearch _S2SSearch)
        {
            string query = "";
            SqlCommand command = new SqlCommand();
            if (_S2SSearch.FeeType != PaymentModuleType.CertificateCoursesAdmission)
            {
                query = $@"SELECT TOP 1 Student_ID FROM ARGPersonalInformation_{_S2SSearch.Course} 
                            WHERE (BoardRegistrationNo=@BoardRegistrationNo OR CUSRegistrationNo=@CUSRegistrationNo OR StudentFormNo=@StudentFormNo)";
                command.Parameters.AddWithValue("@BoardRegistrationNo", _S2SSearch.SearchQuery);
                command.Parameters.AddWithValue("@CUSRegistrationNo", _S2SSearch.SearchQuery);
                command.Parameters.AddWithValue("@StudentFormNo", _S2SSearch.SearchQuery);
            }
            else
            {
                query = $@"SELECT TOP 1 Student_ID FROM CertificateCoursePersonalInfo WHERE BoardRegnNo=@BoardRegistrationNo";
                command.Parameters.AddWithValue("@BoardRegistrationNo", _S2SSearch.SearchQuery);
            }

            if (!string.IsNullOrWhiteSpace(_S2SSearch.Batch))
            {
                query += " AND Batch=@Batch";
                command.Parameters.AddWithValue("@Batch", _S2SSearch.Batch);
            }
            query += " ORDER BY Batch DESC";

            command.CommandText = query;
            return new MSSQLFactory().ExecuteScalar<Guid>(command);
        }

        public bool PaymentExistsInDB(string TxnReferenceNo)
        {
            SqlCommand cmd = new SqlCommand($@"SELECT COUNT(s.TxnReferenceNo) FROM(
                                                SELECT TxnReferenceNo
                                                FROM dbo.PaymentDetails_UG
                                                UNION
                                                SELECT TxnReferenceNo
                                                FROM dbo.PaymentDetails_IH
                                                UNION
                                                SELECT TxnReferenceNo
                                                FROM dbo.PaymentDetails_PG
                                            ) s WHERE s.TxnReferenceNo = @TxnReferenceNo");
            cmd.Parameters.AddWithValue("@TxnReferenceNo", TxnReferenceNo ?? string.Empty);
            return new MSSQLFactory().ExecuteScalar<int>(cmd) > 0;
        }

        public bool IsReconcilationPossibleForExaminationForm(PaymentDetails details)
        {
            try
            {
                SqlCommand cmd = new SqlCommand($@"SELECT COUNT(Payment_ID) FROM ARGStudentExamForm_{details.PrintProgramme.ToString()} 
                                            JOIN PaymentDetails_{details.PrintProgramme.ToString()}  ON PaymentDetails_{details.PrintProgramme.ToString()}.Entity_ID = ARGStudentExamForm_{details.PrintProgramme.ToString()}.StudentExamForm_ID
                                            WHERE FormNumber = @FormNumber AND ModuleType = @ModuleType AND ARGStudentExamForm_{details.PrintProgramme.ToString()}.Semester = @Semester AND PaymentType<>{(short)PaymentType.ReFund}");
                cmd.Parameters.AddWithValue("@FormNumber", details.AdditionalInfo);
                cmd.Parameters.AddWithValue("@ModuleType", (short)PaymentModuleType.Examination);
                cmd.Parameters.AddWithValue("@Semester", details.Semester);
                return new MSSQLFactory().ExecuteScalar<int>(cmd) < 1;
            }
            catch (SqlException)
            {
                return false;
            }
        }
        public bool IsReconcilationPossibleForSemester_I_Admission(PaymentDetails details)
        {
            try
            {
                SqlCommand cmd = new SqlCommand($@"SELECT COUNT(TxnReferenceNo) FROM dbo.PaymentDetails_{details.PrintProgramme.ToString()} WHERE Entity_ID='{details.Student_ID}' AND ModuleType={(short)PaymentModuleType.SemesterAdmission} AND Semester=1 AND PaymentType<>{(short)PaymentType.ReFund}");
                return new MSSQLFactory().ExecuteScalar<int>(cmd) < 1;
            }
            catch (SqlException)
            {
                return false;
            }
        }

        public bool IsReconcilationPossibleForREandXeroxForm(PaymentDetails details)
        {
            try
            {
                string formnos = (details.AdditionalInfo + "").Split(',').ToList().First(x => !(string.IsNullOrWhiteSpace(x)));
                SqlCommand cmd = new SqlCommand($@"SELECT COUNT(Payment_ID) FROM ReEvaluation 
                                    JOIN PaymentDetails_{details.PrintProgramme.ToString()} ON PaymentDetails_{details.PrintProgramme.ToString()}.Entity_ID = ReEvaluation.ReEvaluation_ID
                                    WHERE FormNumber=@FormNumber AND ModuleType IN (4,2)");
                cmd.Parameters.AddWithValue("@FormNumber", formnos);

                if (details.Semester != null)
                {
                    cmd.CommandText += " AND ReEvaluation.Semester=@Semester";
                    cmd.Parameters.AddWithValue("@Semester", details.Semester);
                }
                return new MSSQLFactory().ExecuteScalar<int>(cmd) < 1;
            }
            catch (SqlException)
            {
                return false;
            }
        }
    }
}
