using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace CUSrinagar.Models
{
    public static class BillDeskSettings
    {
        public static string MerchantID { get { return ConfigurationManager.AppSettings["MERCHANTID"]; } }
        public static string SecurityID { get { return ConfigurationManager.AppSettings["SECURITYID"]; } }
        public static string CUSBASEURL { get { return ConfigurationManager.AppSettings["CUS_BASE_URL"]; } }
        public static string BillDeskBaseUrl { get { return ConfigurationManager.AppSettings["BILLDESK_BASE_URL"]; } }
        public static string BillDeskBaseUrlS2S { get { return ConfigurationManager.AppSettings["BILLDESK_BASE_URL_S2S"]; } }
        public static string CheckSumKey { get { return ConfigurationManager.AppSettings["CHECKSUMKEY"]; } }
        public static string FormMethod { get { return "POST"; } }
        public static string FormName { get { return "form1"; } }


        public static string GenerateCheckSum(this string DataString, string CheckSumKeyVal = null)
        {
            CheckSumKeyVal = CheckSumKeyVal ?? CheckSumKey;
            UTF8Encoding encoder = new UTF8Encoding();
            byte[] hashValue;
            byte[] keybyt = encoder.GetBytes(CheckSumKeyVal);
            byte[] message = encoder.GetBytes(DataString);

            HMACSHA256 hashString = new HMACSHA256(keybyt);
            string hex = "";

            hashValue = hashString.ComputeHash(message);
            foreach (byte x in hashValue)
            {
                hex += string.Format("{0:x2}", x);
            }
            return hex.ToUpper().Trim();
        }
    }
}
