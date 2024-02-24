using CUSrinagar.Enums;
using GeneralModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CUSrinagar.Models.ValidationAttrs;

namespace CUSrinagar.Models
{
    public class StudentProfile : Paging
    {
        public Guid Student_ID { get; set; }

        public string Name { get; set; }

        public string FathersName { get; set; }

        public string RegistrationNumber { get; set; }

        public string RollNumber { get; set; }

        public string ImagePath { get; set; }

        public DateTime DateofBirth { get; set; }

        public string Address { get; set; }

        public string MobileNumber { get; set; }

        public string Email { get; set; }

        public Gender Gender { get; set; }

        public string CombinationCode { get; set; }

        public List<ADMSubjectMaster> Subjects { get; set; }

        [DBColumnName("ClgCode")]
        public string CollegeCode { get; set; }

        public Guid Course_ID { get; set; }

        public string CourseCode { get; set; }
    }

    public class StudentResultDiscrepancy
    {

        public string Name { get; set; }

        public string  RegistrationNumber { get; set; }

        public string ExamRollNumber { get; set; }

        public List<ADMSubjectMaster> SubjectCombinationsChosen { get; set; }

        public List<ADMSubjectMaster> SubjectResultsAvailable { get; set; }

    }


    public class ResultGazette
    {
        public string ExamRollNumber { get; set; }

        public string RegistrationNo { get; set; }

        public string FullName { get; set; }

        public string SubjectFullName { get; set; }

        public int TotalTheory { get; set; }

        public int TotalPractical { get; set; }

        public int Total { get; set; }

        public string Remarks { get; set; }
    }
}
