using CUSrinagar.Extensions;
using Newtonsoft.Json;
using System.Linq;

namespace CUSrinagar.Models
{
    public class CUETDocuments
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("data")]
        public CUETCandidateDocs CandidateDoc { get; set; }

        public CUETError CUETErrors { get; set; }
    }

    public class CUETCandidateDocs
    {
        [JsonProperty("application_no")]
        public long ApplicationNo { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("gender")]
        public string Gender { get; set; }

        [JsonProperty("photo")]
        public string Photo
        {
            get;
            set;
        }
        public string PhotoURL
        {
            get
            {
                if (Photo.IsNullOrEmpty()) return null;
                return Photo.Split('?').FirstOrDefault();
            }
        }
        public string PhotoExtension
        {
            get
            {
                if (Photo.IsNullOrEmpty()) return null;
                return Photo.Split('?').FirstOrDefault()?.Substring(Photo.Split('?').FirstOrDefault()?.LastIndexOf(".") ?? 0);
            }
        }


        [JsonProperty("signature")]
        public string Signature
        {
            get;
            set;
        }
        public string SignatureURL
        {
            get
            {
                if (Signature.IsNullOrEmpty()) return null;
                return Signature.Split('?').FirstOrDefault();
            }
        }

        [JsonProperty("category_certificate")]
        public object CategoryCertificate { get; set; }

        [JsonProperty("km_certificate")]
        public object KmCertificate { get; set; }

        [JsonProperty("bpl_certificate")]
        public object BplCertificate { get; set; }

        [JsonProperty("cw_certificate")]
        public object CwCertificate { get; set; }

        [JsonProperty("pwd_certificate")]
        public object PwdCertificate { get; set; }

    }
}
