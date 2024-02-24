using Newtonsoft.Json;
using System;
using System.Net;

namespace CUSrinagar.Models
{
    public static class ABCAPIUrls
    {
        public static string AccessTokenURL => "https://nadapi.digilocker.gov.in/v1/oauth";
        public static string AccountsBasicDetailsURL => "https://nadapi.digilocker.gov.in/v1/AbcAccountsBasicDetails";
    }
    public class ABCRequestToken
    {
        [JsonProperty("customer_id")]
        public string CustomerID => "in.edu.cusrinagar";

        [JsonProperty("customer_secret_key")]
        public string CustomerSecretKey => "qvjHetcKSQkElD8oZTWiIguarMhXF4JN";
    }

    public class ABCResponseToken
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("expires_in")]
        public string ExpiresIn { get; set; }
    }

    public class ValidateABCID
    {
        [JsonProperty("abc_account_id")]
        public string ABCID { get; set; }
    }
    public class ValidateABCIDResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("statuscode")]
        public string Statuscode { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("ABC_ACCOUNT_ID")]
        public string ABCID { get; set; }

        [JsonProperty("CNAME")]
        public string Cname { get; set; }

        [JsonProperty("GENDER")]
        public string Gender { get; set; }

        [JsonProperty("DOB")]
        public string Dob { get; set; }

        [JsonProperty("CREDIT_POINTS")]
        public string CreditPoints { get; set; }

        [JsonProperty("CREATED_DATE")]
        public string CreatedDate { get; set; }

        [JsonProperty("UNIVERSITY_NAME")]
        public string UniversityName { get; set; }

        [JsonProperty("COURSE_NAME")]
        public string CourseName { get; set; }

        [JsonProperty("PROGRAM_NAME")]
        public string ProgramName { get; set; }

        [JsonProperty("ENROLLMENT_NO")]
        public string EnrollmentNo { get; set; }
    }

}
