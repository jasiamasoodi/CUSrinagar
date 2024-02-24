using CUSrinagar.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUSrinagar.Models
{
    public class BillDeskS2SRequest
    {
        public string RequestType { get; set; } = "0122";
        public string MerchantID
        {
            get
            {
                return BillDeskSettings.MerchantID;
            }
        }
        /// <summary>
        /// The Merchant Transaction ID that that was
        /// sent in ‘CustomerID’ in the Payment Request
        /// Should be of 15-25 length
        /// </summary>
        public string CustomerID { get; set; }

        public string CurrentDate
        {
            get
            {
                return DateTime.Now.ToString("yyyyMMddHHmmss");
            }
        }
        public string CheckSum { get; set; }

    }
}
