using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using CUSrinagar.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terex;

namespace CUSrinagar.DataManagers
{
    public class CertificateDB
    {
        public bool Add(AttemptCertificate attemptCertificate)
        {
            return new MSSQLFactory().InsertRecord<AttemptCertificate>(attemptCertificate) >= 1;
        }

        public bool Add(AttemptCertificateDetails item)
        {
            return new MSSQLFactory().InsertRecord<AttemptCertificateDetails>(item) >= 1;

        }

        public bool UpdateCertificateStatus(Guid Certificate_ID)
        {
            string query = $"UPDATE AttemptCertificate SET FeeStatus={(int)FormStatus.FeePaid} WHERE Certificate_ID=@Certificate_ID";
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = query;
            cmd.Parameters.AddWithValue("@Certificate_ID", Certificate_ID);
            return !new MSSQLFactory().ExecuteScalar<bool>(cmd);

        }

        public AttemptCertificate Get(Guid Certificate_ID)
        {
            string query = $"Select * from AttemptCertificate  WHERE Certificate_ID=@Certificate_ID";
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = query;
            cmd.Parameters.AddWithValue("@Certificate_ID", Certificate_ID);
            return new MSSQLFactory().GetObject<AttemptCertificate>(cmd);
        }
        public List<AttemptCertificateDetails> GetList(Guid Certificate_ID)
        {
            string query = $"Select * from AttemptCertificateDetails  WHERE Certificate_ID=@Certificate_ID";
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = query;
            cmd.Parameters.AddWithValue("@Certificate_ID", Certificate_ID);
            return new MSSQLFactory().GetObjectList<AttemptCertificateDetails>(cmd);
        }

        public PaymentDetails GetPayment(AttemptCertificate attemptCertificate)
        {
            SqlCommand command = new SqlCommand();
            string query = string.Empty;
            query = $"SELECT PD.* From  AttemptCertificate AC Left JOIN PaymentDetails_{attemptCertificate.personalInformationCompact.PrintProgramme.GetTablePFix()} PD ON entity_id=student_ID AND ModuleType={(int)PaymentModuleType.AttemptCertificate}  Where Student_ID ='{attemptCertificate.personalInformationCompact.Student_ID}' " +
                $" ORDER BY CreatedOn desc";
            command.CommandText = query;
            return new MSSQLFactory().GetObject<PaymentDetails>(command);
        }

        public bool Edit(AttemptCertificate attemptCertificate)
        {
            List<string> ignoreQuery = new List<string>() { nameof(attemptCertificate.Certificate_ID), nameof(attemptCertificate.CreatedOn), nameof(attemptCertificate.TotalFee) };
            SqlCommand sqlCommand = new MSSQLFactory().UpdateRecord<AttemptCertificate>(attemptCertificate, ignoreQuery, ignoreQuery);
            sqlCommand.CommandText += " WHERE Certificate_ID=@Certificate_ID";
            sqlCommand.Parameters.AddWithValue("@Certificate_ID", attemptCertificate.Certificate_ID);
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand) >= 1;

        }
        public bool Edit(AttemptCertificateDetails attemptCertificateDetails)
        {
            List<string> ignoreQuery = new List<string>() { nameof(attemptCertificateDetails.Details_ID), nameof(attemptCertificateDetails.Certificate_ID), nameof(attemptCertificateDetails.Semester) };
            SqlCommand sqlCommand = new MSSQLFactory().UpdateRecord<AttemptCertificateDetails>(attemptCertificateDetails, ignoreQuery, ignoreQuery);
            sqlCommand.CommandText += " WHERE Details_ID=@Details_ID";
            sqlCommand.Parameters.AddWithValue("@Details_ID", attemptCertificateDetails.Details_ID);
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand) >= 1;

        }
        public AttemptCertificate GetByStudentID(Guid Student_ID)
        {
            string query = $"Select * from AttemptCertificate  WHERE Student_ID=@Student_ID";
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = query;
            cmd.Parameters.AddWithValue("@Student_ID", Student_ID);
            return new MSSQLFactory().GetObject<AttemptCertificate>(cmd);
        }
    }
}
