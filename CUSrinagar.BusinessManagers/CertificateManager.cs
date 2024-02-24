using CUSrinagar.DataManagers;
using CUSrinagar.Enums;
using CUSrinagar.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace CUSrinagar.BusinessManagers
{
    public class CertificateManager
    {
        public bool SaveAttemptCertificate(AttemptCertificate attemptCertificate)
        {
            using (var ts = new TransactionScope())
            {
                if (new CertificateDB().Add(attemptCertificate))
                {
                    foreach (var item in attemptCertificate.AttemptCertificateDetailsList)
                    {
                        item.Details_ID = Guid.NewGuid();
                        item.Certificate_ID = attemptCertificate.Certificate_ID;
                        new CertificateDB().Add(item);

                    }
                    ts.Complete();
                    return true;
                }
            }
            return false;
        }

        internal bool UpdateCertificateStatus(Guid Certificate_ID, PaymentDetails aRGPaymentDetails)
        {
            bool result = false;
            using (var ts = new TransactionScope())
            {
                result = new PaymentDB().SavePaymentDetails(aRGPaymentDetails, aRGPaymentDetails.PrintProgramme) > 0;
                if (result)
                    result = new CertificateDB().UpdateCertificateStatus(Certificate_ID);
                ts.Complete();
            }
            return result;
        }

        public AttemptCertificate Get(Guid Certificate_Id)
        {
            AttemptCertificate attemptCertificate = new CertificateDB().Get(Certificate_Id);
            attemptCertificate.AttemptCertificateDetailsList = new CertificateDB().GetList(Certificate_Id);
            return attemptCertificate;
        }
        public AttemptCertificate GetByStudentID(Guid Student_ID)
        {
            AttemptCertificate attemptCertificate = new CertificateDB().GetByStudentID(Student_ID);
            return attemptCertificate;
        }

        public PaymentDetails GetPayment(AttemptCertificate attemptCertificate)
        {
            return new CertificateDB().GetPayment(attemptCertificate);
        }

        public bool EditAttemptCertificate(AttemptCertificate attemptCertificate)
        {
            using (var ts = new TransactionScope())
            {
                if (new CertificateDB().Edit(attemptCertificate))
                {
                    foreach (var item in attemptCertificate.AttemptCertificateDetailsList)
                    {
                        new CertificateDB().Edit(item);

                    }
                    ts.Complete();
                    return true;
                }
            }
            return false;
        }
    }
}
