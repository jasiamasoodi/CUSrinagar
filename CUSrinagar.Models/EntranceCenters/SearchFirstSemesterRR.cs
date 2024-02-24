using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUSrinagar.Models
{
    public class SearchFirstSemesterRR
    {
        [Required(ErrorMessage = " Required")]
        public Programme Programme { get; set; }

        [Required(ErrorMessage = " Required")]
        [Range(2017, 9999, ErrorMessage = " Should be > 2016")]
        public short Batch { get; set; }

        [DisplayName("Course")]
        [Required(ErrorMessage = " Required")]
        public Guid Course_ID { get; set; }

        [DisplayName("College")]
        [Required(ErrorMessage = " Required")]
        public Guid College_ID { get; set; }

        [DisplayName("RollNo's separated by , (comma)")]
        [RegularExpression(@"^(\d{6},)*\d{6}$", ErrorMessage = "Invalid (RollNo's should be 6 digits or remove comma OR NewLine at the end of list)")]
        public string EntranceRollNos { get; set; }
    }

    public class FirstSemesterRR
    {
        public int Sno { get; set; }
        public string StudentFormNo { get; set; }
        public long EntranceRollNo { get; set; }
        public Guid Student_ID { get; set; }
        public decimal? CATEntrancePoints { get; set; }
        public string FullName { get; set; }
        public string FathersName { get; set; }
        public string Category { get; set; }
        public string CollegeFullName { get; set; }
        public string CourseAllotedByCollege { get; set; }
        public string CoursesAppliedByStudent { get; set; }
        public string Mobile { get; set; }
        public decimal AmountPaid { get; set; }
        public DateTime AcceptedOn { get; set; }
    }
}
