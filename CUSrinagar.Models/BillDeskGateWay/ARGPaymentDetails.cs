using CUSrinagar.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUSrinagar.Models
{
    public class PaymentDetails
    {
        public Guid Payment_ID { get; set; }
        public Guid Entity_ID { get; set; }
        [IgnoreDBWriter]
        public string EntityID { get; set; }
        public string TxnReferenceNo { get; set; }
        public string BankReferenceNo { get; set; }
        public decimal TxnAmount { get; set; }
        public string BankID { get; set; }
        public string BankMerchantID { get; set; }
        public string TxnType { get; set; }
        public string CurrencyName { get; set; }
        public string ItemCode { get; set; }
        public string SecurityType { get; set; }
        public string SecurityID { get; set; }
        public DateTime TxnDate { get; set; }
        public string AuthStatus { get; set; }
        public string SettlementType { get; set; }
        public string ErrorStatus { get; set; }
        public string ErrorDescription { get; set; }
        public PaymentType PaymentType { get; set; }

        [IgnoreDBRead, IgnoreDBWriter]
        public PrintProgramme PrintProgramme { get; set; }
        public PaymentModuleType ModuleType { get; set; }       
        public string Email { get; set; }        
        public string PhoneNumber { get; set; }

        [IgnoreDBRead, IgnoreDBWriter]
        public Guid Student_ID { get; set; }

        public short? Semester { get; set; }

        [IgnoreDBRead, IgnoreDBWriter]
        public string AdditionalInfo { get; set; }
    }

}
