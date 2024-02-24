using CUSrinagar.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUSrinagar.Models
{
    public class SalaryCalculationColumns
    {
        public Guid SalaryCalculationColumn_ID { get; set; }
        public string SalaryCalculationColumn { get; set; }
        public SalaryCalculationColumn SalaryCalculationColumnType { get; set; }
        public bool IsPercent { get; set; }
        public int ColumnValue { get; set; }

    }

    public class SalaryBill
    {
        public Guid SalaryBill_ID { get; set; }
        public Guid Emp_ID { get; set; }
        [IgnoreDBWriter]
        public bool ISNPS { get; set; }
        [Required]
        [Display(Name = "Pay Scale")]
        public string PayScale { get; set; }
        [Required]
        [Display(Name = "Basic Pay")]
        public int BasicPay { get; set; }
        [Required]
        [Display(Name = "D.A")]
        public int DA { get; set; }
        [Required]
        [Display(Name = "H.R.A")]
        public int HRA { get; set; }
        [Required]
        [Display(Name = "M.A")]
        public int MA { get; set; }
        [Required]
        [Display(Name = "CCA")]
        public int CCA { get; set; }
        [Required]
        [Display(Name = "SPL.PAY")]
        public int SPL_PAY { get; set; }
        [Required]
        [Display(Name = "Charge Allow")]
        public int ChargeAllow { get; set; }
        [Required]
        [Display(Name = "GPF.Sub")]
        public int GPF_Sub { get; set; }
        [Required]
        [Display(Name = "GPF.Ref")]
        public int GPF_ref { get; set; }
        [Required]
        [Display(Name = "Group Ins")]
        public int GroupIns { get; set; }
        [Required]
        [Display(Name = "SLI-I")]
        public int SLI_I { get; set; }
        [Required]
        [Display(Name = "SLI-II")]
        public int SLI_II { get; set; }
        [Required]
        [Display(Name = "I.Tax")]
        public int I_Tax { get; set; }
        [Required]
        [Display(Name = "Recovery Excess")]
        public int RecoveryExcess { get; set; }
        [Required]
        [Display(Name = "NPS Share")]
        public int NPSShare { get; set; }
        [Required]
        [Display(Name = "Net Pay")]
        public int NetPay { get; set; }
        [Required]
        [Display(Name = "Other Allowances")]
        public int OtherAllowances { get; set; }
        [Required]
        [Display(Name = "Remark")]
        public string OtherAllowancesRemark { get; set; }
        [Required]
        [Display(Name = "Other Deductions")]
        public int OtherDeductions { get; set; }
        [Required]
        [Display(Name = "Remark")]
        public string OtherDeductionsRemark { get; set; }
        [IgnoreDBWriter]
        public string EmployeeFullName { get; set; }
        [IgnoreDBWriter]
        public EmployeeType EmployeeType { get; set; }
        [IgnoreDBWriter]
        public string Designation { get; set; }
        [IgnoreDBWriter]
        public int GrossAmount
        {
            get
            {
                return (BasicPay
                        + DA
                        + HRA
                        + MA
                        + CCA
                        + SPL_PAY
                        + ChargeAllow
                        + OtherAllowances);
            }
        }
        [IgnoreDBWriter]
        public int TotalDeduc
        {
            get
            {

                return (GPF_Sub
                    + GPF_ref
                    + GroupIns
                    + SLI_I
                    + SLI_II
                    + I_Tax
                    + RecoveryExcess
                    + NPSShare
                    + OtherDeductions);
            }
        }
    }

    [Table("SalaryBillArchive")]
    public class SalaryBillArchive : SalaryBill
    {
        public DateTime CreatedOn { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
    }

    public class SalaryBillFilter
    {
        [Required]
        public int Year { get; set; }
        [Required]
        public int Month { get; set; }
    }
}
