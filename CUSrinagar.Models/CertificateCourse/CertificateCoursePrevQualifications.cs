using CUSrinagar.Enums;
using CUSrinagar.ValidationAttrs;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace CUSrinagar.Models
{
    public class CertificateCoursePrevQualifications
    {
        public Guid PrevQualification_ID { get; set; }
        public Guid Student_ID { get; set; }

        [DisplayName("Exam Name")]
        [Required(ErrorMessage = " Required")]
        [MinLength(3, ErrorMessage = "Max 3 chars")]
        [MaxLength(150, ErrorMessage = "Max 150 chars")]
        public string ExamName { get; set; }

        [Required(ErrorMessage = " Required")]
        [MinLength(2, ErrorMessage = "Max 2 chars")]
        [MaxLength(80, ErrorMessage = "Max 80 chars")]
        public string Stream { get; set; }

        [Required(ErrorMessage = " Required")]
        [RegularExpression(@"\d{4,4}", ErrorMessage = "Invalid")]
        [Range(typeof(short), "1950", "9999", ErrorMessage = " Invalid")]
        public short YearOfPassing { get; set; }

        [Required(ErrorMessage = " Required")]
        [DisplayName("Marks Obt")]
        [Range(typeof(double), "1", "9999", ErrorMessage = " Invalid")]
        public double MarksObt { get; set; }

        [Required(ErrorMessage = " Required")]
        [DisplayName("Max Marks")]
        [RegularExpression(@"\d{2,4}", ErrorMessage = "Invalid")]
        public int MaxMarks { get; set; }

        [Required(ErrorMessage = " Required")]
        [DisplayName("Board/University Name")]
        public string ExamBody { get; set; }

        [DisplayName("%tage")]
        [IgnoreDBRead]
        [IgnoreDBWriter]
        public decimal Percentage
        {
            get
            {
                if (MarksObt > 0 && MaxMarks > 0)
                {
                    return Math.Round((decimal)(MarksObt / MaxMarks) * 100m, 2);
                }
                return 0.0m;
            }
        }

        [IgnoreDBRead]
        [IgnoreDBWriter]
        public bool ReadOnly { get; set; }
    }
}
