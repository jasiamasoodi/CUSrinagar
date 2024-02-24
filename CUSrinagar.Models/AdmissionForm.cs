using CUSrinagar.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CUSrinagar.Models
{

    public class AdmissionForm
    {
        public Guid Student_Id { get; set; }
        public Semester Semester { get; set; }
        public DateTime AppliedOn { get; set; }
        public int ModuleType { get; set; }
        public DateTime? PaymentOn { get; set; }
        public List<string> Subjects { get; set; }
        public decimal Amount { get; set; }
        public short SemesterBatch { get; set; }
    }

    public class StudentPayment
    {
        public Guid Student_Id { get; set; }
        public Semester Semester { get; set; }
        public DateTime AppliedOn { get; set; }
        public int ModuleType { get; set; }
        public DateTime? PaymentOn { get; set; }
         public decimal Amount { get; set; }
        public PaymentType PaymentType { get; set; }
        public string TxnReferenceNo { get; set; }
    }

    public class StudentAdmissionCertificate
    {
        public string StudentFormNo { get; set; }
        public string CertificateUrl { get; set; }
        public string FullName { get; set; }
        public string Mobile { get; set; }
        public CertificateType CertificateType { get; set; }
    }
}

