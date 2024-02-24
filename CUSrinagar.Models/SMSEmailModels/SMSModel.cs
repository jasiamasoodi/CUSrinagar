using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CUSrinagar.Extensions;
using System.Security.Cryptography;

namespace CUSrinagar.Models
{
    public class SMSModel
    {
        public string Username { get { return ConfigurationManager.AppSettings["Username"].ToString().Trim(); } }
        public string Password { get { return ConfigurationManager.AppSettings["SMSPassword"].ToString().Trim().SMSEncryptedPasswod(); } }
        public string Senderid { get { return ConfigurationManager.AppSettings["Senderid"].ToString().Trim(); } }
        public string SecureKey { get { return ConfigurationManager.AppSettings["SecureKey"].ToString().Trim(); } }

        /// <summary>
        /// For bulk SMS should be separated by comma
        /// </summary>
        public string MobileNos { get; set; }
        public string Message { get; set; }
        public string NewSecureKey
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                StringBuilder sb1 = new StringBuilder();
                if (!string.IsNullOrWhiteSpace(Message))
                {
                    sb.Append(Username).Append(Senderid).Append(Message).Append(SecureKey);
                    byte[] genkey = Encoding.UTF8.GetBytes(sb.ToString());
                    HashAlgorithm sha1 = HashAlgorithm.Create("SHA512");
                    byte[] sec_key = sha1.ComputeHash(genkey);

                    for (int i = 0; i < sec_key.Length; i++)
                    {
                        sb1.Append(sec_key[i].ToString("x2"));
                    }
                }
                return sb1.ToString();
            }
        }
    }
}
