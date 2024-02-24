using CUSrinagar.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUSrinagar.Models
{
    public class ARGStudentPreviousQualifications : BaseWorkFlow
    {
        public Guid Qualification_ID { get; set; }
        public Guid Student_ID { get; set; }

        [DisplayName("Exam Name")]
        [Required(ErrorMessage = " Required")]
        [MinLength(2, ErrorMessage = "Min 2 chars")]
        [MaxLength(15, ErrorMessage = "Max 15 chars")]
        public string ExamName { get; set; }

        [Required(ErrorMessage = " Required")]
        [StringLength(75, ErrorMessage = "Min 2 chars")]
        public string Stream { get; set; }

        [Required(ErrorMessage = " Required")]
        [MinLength(3, ErrorMessage = "Max 3 chars")]
        [MaxLength(240, ErrorMessage = "Max 240 chars")]
        public string Subjects { get; set; }

        [Required(ErrorMessage = " Required")]
        public string Session { get; set; }

        [Required(ErrorMessage = " Required")]
        [RegularExpression(@"\d{4,4}", ErrorMessage = "Invalid")]
        [Range(typeof(int), "1950", "9999", ErrorMessage = " Invalid")]
        public int Year { get; set; }

        [Required(ErrorMessage = " Required")]
        [DisplayName("Roll No")]
        [MinLength(1, ErrorMessage = "Max 1 chars")]
        [MaxLength(20, ErrorMessage = "Max 20 chars")]
        public string RollNo { get; set; }

        [Required(ErrorMessage = " Required")]
        [DisplayName("Marks Obt")]
        [Range(typeof(double), "1", "9999", ErrorMessage = " Invalid")]
        public double MarksObt { get; set; }

        [Required(ErrorMessage = " Required")]
        [DisplayName("Max Marks")]
        [Range(typeof(double), "1", "9999", ErrorMessage = " Invalid")]
        public double MaxMarks { get; set; }

        [Required(ErrorMessage = " Required")]
        [DisplayName("Board/University Name")]
        [MaxLength(30, ErrorMessage = "Max 30 chars")]
        public string ExamBody { get; set; }


        [DisplayName("Over All CGPA")]
        [Range(typeof(double), "1", "9999", ErrorMessage = " Invalid")]
        public double? OverAllCGPA { get; set; }

        [DisplayName("%tage")]
        [IgnoreDBRead]
        [IgnoreDBWriter]
        public decimal Percentage
        {
            get
            {
                if (MarksObt > 0 && MaxMarks > 0)
                {
                    return Math.Round(((decimal)MarksObt / (decimal)MaxMarks) * 100m, 2);
                }
                return 0.0m;
            }
        }

        [IgnoreDBRead]
        [IgnoreDBWriter]
        public bool ReadOnly { get; set; }

        [DisplayName("12th Result")]
        [IgnoreDBWriter]
        public bool? IsProvisional { get; set; }

    }

}
