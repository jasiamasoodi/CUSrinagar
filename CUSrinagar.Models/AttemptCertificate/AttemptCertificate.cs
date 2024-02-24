using CUSrinagar.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUSrinagar.Models
{
    public class AttemptCertificate
    {
        public Guid Certificate_ID { get; set; }
        public Guid Student_Id { get; set; }
        public FormStatus FeeStatus { get; set; }
        public int TotalFee { get; set; }
        [Required]
        public string ReasonforIssuance { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public List<AttemptCertificateDetails> AttemptCertificateDetailsList { get; set; }
        [IgnoreDBWriter]
        public PersonalInformationCompact personalInformationCompact { get; set; }
        [IgnoreDBWriter]
        public string TransactionNo { get; set; }
        [IgnoreDBWriter]
        public DateTime TransactionDate { get; set; }
     
    }
}
