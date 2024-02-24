using CUSrinagar.Enums;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CUSrinagar.Models
{
    public class CertificateVerification
    {
        public Guid Student_ID { get; set; }
        public bool FeePaid { get; set; }

        [DisplayName("Reason for verification of certificates")]
        [Required(ErrorMessage = " Required")]
        [MinLength(10, ErrorMessage = "Min 10 chars")]
        [MaxLength(300, ErrorMessage = "Max 300 chars")]
        public string ReasonForVerification { get; set; }

        [DisplayName("Department / Organisation Name and Postal Address where from verification is sought")]
        [Required(ErrorMessage = " Required")]
        [MinLength(10, ErrorMessage = "Min 10 chars")]
        [MaxLength(300, ErrorMessage = "Max 300 chars")]
        public string OrgNamePostalAddress { get; set; }

        [Required(ErrorMessage = " Required")]
        [DisplayName("Department / Organisation  Email")]
        [MaxLength(100, ErrorMessage = " Max 100 chars")]
        [RegularExpression(@"^([a-zA-Z0-9_.-])+@([a-zA-Z0-9_.-])+\.([a-zA-Z])+([a-zA-Z])+", ErrorMessage = " Invalid")]
        public string OrgEmail { get; set; }

        [DisplayName("Documents attached for verification")]
        [Required(ErrorMessage = " Required")]
        [MinLength(2, ErrorMessage = "Min 2 chars")]
        [MaxLength(500, ErrorMessage = "Max 500 chars")]
        public string DocsAttachedForVerification { get; set; }

        public DateTime AppliedOn { get; set; }

        [IgnoreDBWriter]
        public string CUSRegistrationNo { get; set; }

        [IgnoreDBWriter]
        public string CollegeFullName { get; set; }

        [IgnoreDBWriter]
        public string FullName { get; set; }

        [IgnoreDBRead]
        [IgnoreDBWriter]
        public DateTime DOB { get; set; }

        [IgnoreDBWriter]
        public string FathersName { get; set; }

        [IgnoreDBRead]
        [IgnoreDBWriter]
        public PrintProgramme OrgProgramme { get; set; }

        [IgnoreDBWriter]
        public PaymentDetails PaymentDetail { get; set; }
    }
}
