using CUSrinagar.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUSrinagar.Models
{
    public class SemesterAdmissionFilter
    {
        [Required(ErrorMessage = " Required")]
        [Display(Name = "Semester")]
        public short Semester { get; set; }
        public Guid? College_ID { get; set; }

        [Required(ErrorMessage = " Required")]
        [Display(Name = "Programme")]
        public PrintProgramme SearchInPrintProgramme { get; set; }

        [Required(ErrorMessage = " Required")]
        [Display(Name = "Fee Submission Date From")]
        public DateTime Dated { get; set; }
    }
}
