using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CUSrinagar.Enums;

namespace CUSrinagar.Models
{
    public class MSTranscriptDegreeSettings
    {
        [Key]
        [Required(ErrorMessage = "Setting ID is required")]
        [Display(Name = "Setting ID")]
        public Guid Setting_ID { get; set; }
        [Required(ErrorMessage = "Course is required")]
        [Display(Name = "Course")]
        public Guid Course_ID { get; set; }
        [Required(ErrorMessage = "Batch To is required")]
        [Display(Name = "Batch To")]
        public int BatchTo { get; set; }
        [Required(ErrorMessage = "Batch From is required")]
        [Display(Name = "Batch From")]
        public int BatchFrom { get; set; }
        [Required(ErrorMessage = "Semester To is required")]
        [Display(Name = "Semester To")]
        public int SemesterTo { get; set; }
        [Required(ErrorMessage = "Semester From is required")]
        [Display(Name = "Semester From")]
        public int SemesterFrom { get; set; }
        [Required(ErrorMessage = "Total Credits is required")]
        [Display(Name = "Passing Credits")]
        public int TotalCredits { get; set; }
        [Display(Name = "Passing Credits for Lateral Entry")]
        public int LateralEntryTotalCredits { get; set; }
        [MaxLength(500)]
        [StringLength(500)]
        [Required(ErrorMessage = "Awarded Degree Title is required")]
        [Display(Name = "Awarded Title (Degree)")]
        public string AwardedDegreeTitle { get; set; }
        [MaxLength(500)]
        [StringLength(500)]
        [Display(Name = "Awarded Title (Transcript)")]
        public string AWARDEDTRANSCRIPTTITLE { get; set; }
        [MaxLength(500)]
        [StringLength(500)]
        [Required(ErrorMessage = "Program Title is required")]
        [Display(Name = "Program Title")]
        public string ProgramTitle { get; set; }

        [IgnoreDBWriter]
        public Programme Programme { get; set; }
        [IgnoreDBWriter]
        public string CourseFullName { get; set; }
    }

}
