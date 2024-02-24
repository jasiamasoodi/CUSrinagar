using CUSrinagar.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUSrinagar.Models
{
    public class BillDeskS2SResponse
    {
        public string RequestType { get; set; }
        public string MerchantID { get; set; }
        public Guid Entity_ID { get; set; }

        /// <summary>
        /// The Merchant Transaction ID that that was
        /// sent in ‘CustomerID’ in the Payment Request
        /// Should be of 15-25 length
        /// </summary>
        public string CustomerID { get; set; }
        public string TxnReferenceNo { get; set; }
        public string BankReferenceNo { get; set; }
        public decimal TxnAmount { get; set; }
        /// <summary>
        /// "0300" Success Successful Transaction
        /// "0399" Invalid Authentication at Bank Failed Transaction
        /// "NA" Invalid Input in the Request Message Cancel Transaction
        /// "0002" BillDesk is waiting for Response from Bank Pending Transaction
        /// "0001" Error at BillDesk Cancel Transaction
        /// </summary>
        public string AuthStatus { get; set; }
        public string Filler1 { get; set; }

        /// <summary>
        /// 0699 – Cancellation
        /// 0799 – Refund
        /// NA – Refund Not Available for this request
        /// </summary>
        public string RefundStatus { get; set; }
        public decimal TotalRefundAmount { get; set; }
        public string LastRefundDate { get; set; }
        public string LastRefundRefNo { get; set; }

        /// <summary>
        /// Y – Request Successfully Processed
        /// N- Invalid Request / Parameters
        /// </summary>
        public char QueryStatus { get; set; }

        public string CheckSum { get; set; }

        public PaymentType PaymentType { get; set; }
        public PrintProgramme PrintProgramme { get; set; }
        public PaymentModuleType ModuleType { get; set; }
    }
}
