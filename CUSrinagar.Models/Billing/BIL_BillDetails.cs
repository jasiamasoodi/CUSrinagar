using CUSrinagar.Enums;
using CUSrinagar.ValidationAttrs;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace CUSrinagar.Models
{
    public class BIL_BillDetails
    {
        public Guid Bill_ID { get; set; }

        [MaxLength(22)]
        [DisplayName("Bill No")]
        public string BillNo { get; set; }

        public Guid User_ID { get; set; }

        [DisplayName("Sample Paper")]
        [Required(ErrorMessage = "Sample Paper is required")]
        public Guid SamplePaper_ID { get; set; }


        [DisplayName("Paper Pattern")]
        [Required(ErrorMessage = "Paper Pattern is required")]
        public Guid PaperPattern_ID { get; set; }

        [DisplayName("Subject")]
        [Required(ErrorMessage = "Subject is required")]
        public Guid Subject_ID { get; set; }


        [MaxLength(100)]
        [MinLength(4)]
        [Required(ErrorMessage = "Batch is required")]
        public string Batch { get; set; }

        [IgnoreDBWriter]
        [Required(ErrorMessage = "Semester is required")]
        public short Semester { get; set; }

        [IgnoreDBWriter]
        [Required(ErrorMessage = "Subject Dept is required")]
        [DisplayName("Department")]
        public Guid Department_Id { get; set; }

        [IgnoreDBWriter]
        public string DepartmentFullName { get; set; }

        [MaxLength(100)]
        [MinLength(4)]
        [DisplayName("Examination/Programme")]
        [Required(ErrorMessage = "Examination/Programme is required")]
        public string Examination { get; set; }

        [MaxLength(50)]
        [MinLength(4)]
        [Required(ErrorMessage = "Session is required")]
        public string Session { get; set; }

        [Required(ErrorMessage = "Amount Per Set is required")]
        [DisplayName("Amount Per Set")]
        public short AmountPerSet { get; set; }

        [Required(ErrorMessage = "No Of Sets is required")]
        [DisplayName("No Of Sets")]
        public short NoOfSets { get; set; }

        [Required(ErrorMessage = "Total Assignment Completion Days is required")]
        [DisplayName("Total Assignment Completion Days")]
        public short TotalAssignmentCompletionDays { get; set; }

        [MaxLength(100)]
        [Required(ErrorMessage = "Paper Receiver Email is required")]
        [DisplayName("Paper Receiver Email")]
        public string PaperReceiverEmail { get; set; }

        [MaxLength(2000)]
        [DisplayName("Syllabus Links (Separate by | )")]
        public string SyllabusLink { get; set; }

        [DisplayName("Bill Status")]
        public BillStatus BillStatus { get; set; }

        [DisplayName("Bill Type")]
        public BillType BillType { get; set; }

        public string Institute { get; set; }

        [IgnoreDBWriter]
        public short RevenueStampAmount { get; set; }

        [DisplayName("Conveyance Charges (if applicable)")]
        public short? ConveyanceCharges { get; set; }

        public DateTime? PaymentDate { get; set; }

        [MaxLength(16)]
        public string PaymentAccount { get; set; }

        [MaxLength(100)]
        public string PaymentBranch { get; set; }

        [MaxLength(100)]
        public string PaymentBank { get; set; }

        [MaxLength(11)]
        public string PaymentIFSCode { get; set; }

        [IgnoreDBWriter]
        public string PhoneNumber { get; set; }

        public DateTime CreatedOn { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public Guid? UpdatedBy { get; set; }


        /// <summary>
        /// User Full Name
        /// </summary>
        [IgnoreDBWriter]
        [DisplayName("Setter Name")]
        public string FullName { get; set; }

        [IgnoreDBWriter]
        public string SubjectFullName { get; set; }

        [IgnoreDBWriter]
        public SubjectType SubjectType { get; set; }

        [IgnoreDBWriter]
        public Programme Programme { get; set; }

        [IgnoreDBWriter]
        public string UserName { get; set; }

        [IgnoreDBRead]
        [IgnoreDBWriter]
        public int TotalAmount => (AmountPerSet * NoOfSets) + (ConveyanceCharges ?? 0);
    }

    public class ExcelBillDetailsEvaluator
    {
        public short SNo { get; set; }
        public string BillNo { get; set; }
        public string Bill_Type { get; set; }
        public string Name { get; set; }
        public short Semester { get; set; }
        public string Bank_Name { get; set; }
        public string Bank_Branch { get; set; }
        public string AccountNo { get; set; }
        public string BillStatus { get; set; }
        public short? Conveyance_Charges { get; set; }
        public int Gross_Amount { get; set; }
        public int Amount_Deducted_For_RevenueStamp { get; set; }
        public int Net_Amount { get; set; }
        public string Remarks { get; set; }
        public string Subjects { get; set; }
        public string Examination { get; set; }
        public string Institute { get; set; }
    }
    public class ExcelBillDetailsPaperSetter
    {
        public short SNo { get; set; }
        public string BillNo { get; set; }
        public string Bill_Type { get; set; }
        public string Name { get; set; }
        public short Semester { get; set; }
        public string Bank_Name { get; set; }
        public string Bank_Branch { get; set; }
        public string AccountNo { get; set; }
        public string BillStatus { get; set; }
        public int Gross_Amount { get; set; }
        public int Amount_Deducted_For_RevenueStamp { get; set; }
        public int Net_Amount { get; set; }
        public string Remarks { get; set; }
        public string Subjects { get; set; }
        public string Examination { get; set; }
        public string Institute { get; set; }
    }


    public class BulkBillTransactionStatus
    {
        public BillStatus BillStatus => BillStatus.Paid;

        [DisplayName("Transaction Dated")]
        [Required(ErrorMessage = "required")]
        public DateTime TransactionDate { get; set; }

        [DisplayName("CSV File only")]
        [Required(ErrorMessage = "required")]
        [ValidateFileForm(0, 9000, new string[] { ".csv", ".CSV" })]
        public HttpPostedFileBase CSVFile { get; set; }
    }

    public class BillStatistics
    {
        public BillStatus BillStatus { get; set; }
        public long TotalBills { get; set; }
    }
}
