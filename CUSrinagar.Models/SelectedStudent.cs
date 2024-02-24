using CUSrinagar.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUSrinagar.Models
{
    public class SelectedStudent
    {
        public short? Batch { get; set; }
        public Guid College_ID { get; set; }
        public string CollegeFullName { get; set; }
        public Guid Student_ID { get; set; }
        public string StudentFormNo { get; set; }
        public string EntranceRollNo { get; set; }
        public string FullName { get; set; }
        public string Parentage { get; set; }
        public string Category { get; set; }
        public string Gender { get; set; }
        public string CourseFullName { get; set; }
        public decimal TotalPoints { get; set; }
        public Guid Course_ID { get; set; }
        public StudentSelectionStatus StudentSelectionStatus { get; set; }
        public int NumberOfCertificate { get; set; }
        public Programme Programme { get; set; }
        public PrintProgramme PrintProgramme { get; set; }

    }
}
