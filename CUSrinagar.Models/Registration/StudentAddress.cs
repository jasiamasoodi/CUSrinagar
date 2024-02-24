using CUSrinagar.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CUSrinagar.Models
{
    public class ARGStudentAddress : BaseWorkFlow
    {
        public Guid Address_ID { get; set; }
        public Guid Student_ID { get; set; }

        [DisplayName("Pin Code")]
        [Required(ErrorMessage = " Required")]
        [RegularExpression(@"\d{6,6}", ErrorMessage = "Invalid")]
        public string PinCode { get; set; }
        

        [DisplayName("Mobile No.")]
        [Required(ErrorMessage = "Required")]       
        [RegularExpression(@"^(?:(?:\+|0{0,2})(\s*[\-]\s*)?|[0]?)?[6789]\d{9,9}$", ErrorMessage = "Invalid")]
        [Remote("MobileNoExists", "Registration", AreaReference.UseRoot, AdditionalFields = "PProgramme,Student_ID", ErrorMessage = "Mobile No. already exists")]
        public string Mobile { get; set; }

        [Required(ErrorMessage = " Required")]
        [DisplayName("Email")]
        [MaxLength(70, ErrorMessage = " Max 70 chars")]
        [RegularExpression(@"^([a-zA-Z0-9_.-])+@([a-zA-Z0-9_.-])+\.([a-zA-Z])+([a-zA-Z])+", ErrorMessage = " Invalid")]
        [Remote("EmailExists", "Registration", AreaReference.UseRoot, AdditionalFields = "PProgramme,Student_ID", ErrorMessage = "Email already exists")]
        public string Email { get; set; }

        [DisplayName("Permanent Address")]
        [Required(ErrorMessage = " Required")]
        [MinLength(4, ErrorMessage = "Min 4 chars")]
        [MaxLength(140, ErrorMessage = "Max 140 chars")]
        public string PermanentAddress { get; set; }

        [Required(ErrorMessage = " Required")]
        public string District { get; set; }
        public string State { get; set; }

        [Required(ErrorMessage = " Required")]
        [MinLength(2, ErrorMessage = "Min 2 chars")]
        [MaxLength(35, ErrorMessage = "Max 35 chars")]
        public string Tehsil { get; set; }

        [Required(ErrorMessage = " Required")]
        [MinLength(2, ErrorMessage = "Min 2 chars")]
        [MaxLength(55, ErrorMessage = "Max 55 chars")]
        public string Block { get; set; }

        [Required(ErrorMessage = " Required")]
        [DisplayName(" Assembly Constituency")]
        public string AssemblyConstituency { get; set; }
        
        public string ParliamentaryConstituency { get; set; }

        [IgnoreDBRead]
        [IgnoreDBWriter]
        public PrintProgramme PProgramme {  get; set; }
    }

}
