using CUSrinagar.Enums;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CUSrinagar.Models
{
    public class CUETAPIRequestModel
    {
        [JsonProperty("application_no")]
        public string ApplicationNo { get; set; }

        [JsonProperty("day")]
        public string Day { get; set; }

        [JsonProperty("month")]
        public string Month { get; set; }

        [JsonProperty("year")]
        public string Year { get; set; }
    }

    public class CUSCUETAPIRequest
    {
        [DisplayName("Application No.")]
        [Required(ErrorMessage ="required")]
        [MinLength(7 ,ErrorMessage ="min 7 chars")]
        [MaxLength(15 ,ErrorMessage ="max 15 chars")]
        public string ApplicationNo { get; set; }

        [DisplayName("Date of Birth")]
        [Required(ErrorMessage ="required")]
        public DateTime DOB { get; set; }

        [DisplayName("CUET Selected Programme")]
        [Required(ErrorMessage ="required")]
        public PrintProgramme SelectedPorgramme { get; set; }

        [Required(ErrorMessage = " Required")]
        [DisplayName("Board Registration No")]
        [MinLength(7, ErrorMessage = " Invalid")]
        [MaxLength(40, ErrorMessage = " Max 40 chars")]
        [RegularExpression(@"^[^\s]+$", ErrorMessage = " Invalid(remove spaces)")]
        public string BoardRegistrationNo { get; set; }
    }
}
