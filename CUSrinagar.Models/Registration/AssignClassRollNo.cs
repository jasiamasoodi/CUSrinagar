using CUSrinagar.Enums;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CUSrinagar.Models
{
    public class AssignClassRollNo : BaseWorkFlow
    {
        [Required(ErrorMessage = " Required")]
        [DisplayName("Override Existing Class RollNo")]
        public bool OverrideExistingClassRollNo { get; set; }

        [Required(ErrorMessage = " Required")]
        public PrintProgramme Programme { get; set; }

        [DisplayName("New Class RollNo")]
        [Required(ErrorMessage = " Required")]
        [MaxLength(11, ErrorMessage = " Max 11 chars")]
        [RegularExpression(@"^[a-zA-Z0-9-]*$", ErrorMessage = " Invalid (remove spaces and only numbers,Chars are allowed)")]
        public string ClassRollNo { get; set; }

        [DisplayName("Form Number Or CUS Regn. No.")]
        [Required(ErrorMessage = " Required")]
        [MinLength(4, ErrorMessage = " Invalid")]
        [MaxLength(150, ErrorMessage = " Max 150 chars")]
        [RegularExpression(@"^[^\s]+$", ErrorMessage = " Invalid(remove spaces)")]
        public string StudentFormNoOrRegnNo { get; set; }

        [DisplayName("Batch")]
        [Required(ErrorMessage = " Required")]
        [Range(2015, 3000, ErrorMessage = "Invalid")]
        [RegularExpression(@"\d{4,4}", ErrorMessage = " Invalid (remove spaces and only numbers are allowed)")]
        public int Batch { get; set; }
    }
}
