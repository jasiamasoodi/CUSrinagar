using CUSrinagar.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUSrinagar.Models
{
    public class MSCGPA
    {
        public Guid ID { get; set; }
        public Guid Student_ID { get; set; }
        public decimal TotalCreditsEarned { get; set; }
        public decimal CGPA { get; set; }
        public DateTime DateofDeclaration { get; set; }
        public decimal TotalCreditPoints { get; set; }
        public decimal Percentage { get; set; }
        public string NotificationNo { get; set; }
        public int MarksSheetNo { get; set; }
        public int SemesterTo { get; set; }
        public Guid TCourse_ID { get; set; }
        public SGPAType SGPAType { get; set; }
        [IgnoreDBWriter]
        public DateTime CreatedOn { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime? PrintedOn { get; set; }
        public Guid? PrintedBy { get; set; }
        public DateTime? ValidatedOn { get; set; }
        public Guid? ValidatedBy { get; set; }
        public DateTime? HandedOverOn { get; set; }
        public Guid? HandedOverBy_ID { get; set; }
        public string DivImpNotificationNo { get; set; }
        public DateTime DivImpDateofDeclaration { get; set; }

        [IgnoreDBRead, IgnoreDBWriter]
        public bool IsValidated { get { return ValidatedOn.HasValue && ValidatedOn.Value != DateTime.Now && ValidatedBy.HasValue; } }
        [IgnoreDBRead, IgnoreDBWriter]
        public bool IsPrinted { get { return IsValidated && PrintedOn.HasValue && PrintedOn.Value != DateTime.Now && PrintedBy.HasValue; } }
        [IgnoreDBRead, IgnoreDBWriter]
        public bool IsHandedOver { get { return IsPrinted && HandedOverOn.HasValue && ValidatedOn.Value != DateTime.Now && HandedOverBy_ID.HasValue; } }
        [IgnoreDBWriter]
        public bool DegreeCertificateGenerated { get; set; }
    }
}
