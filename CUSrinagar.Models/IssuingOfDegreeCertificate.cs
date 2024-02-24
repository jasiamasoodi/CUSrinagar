using CUSrinagar.Enums;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CUSrinagar.Models
{
    public class IssuingOfDegreeCertificate
    {
        [IgnoreDBWriter]
        public string Photograph { get; set; }

        [IgnoreDBWriter]
        public string CUSRegistrationNo { get; set; }

        [IgnoreDBWriter]
        public string FullName { get; set; }

        [IgnoreDBWriter]
        public string FathersName { get; set; }

        [IgnoreDBWriter]
        public DateTime DOB { get; set; }

        [IgnoreDBWriter]
        public string CourseFullName { get; set; }

        [IgnoreDBWriter]
        public string CollegeFullName { get; set; }

        [IgnoreDBWriter]
        public PrintProgramme PrintProgramme { get; set; }

        [IgnoreDBWriter]
        public string ExamRollNumber { get; set; }

        [IgnoreDBWriter]
        public decimal CGPA { get; set; }

        [IgnoreDBWriter]
        public Guid Course_ID { get; set; }

        [IgnoreDBWriter]
        public DateTime DateOfDeclaration { get; set; }

        public Guid Student_ID { get; set; }

        [DisplayName("Specializations (if any)")]
        [MinLength(8, ErrorMessage = "Min 8 chars")]
        [MaxLength(300, ErrorMessage = "Max 300 chars")]
        public string Specializations { get; set; }

        [DisplayName("Perusing Higher Education Institute Name, address & Programme (if any)")]
        [MinLength(10, ErrorMessage = "Min 10 chars")]
        [MaxLength(300, ErrorMessage = "Max 300 chars")]
        public string PerusingHigherEduInAndProgramme { get; set; }

        [DisplayName("Present Employment Status")]
        [Required(ErrorMessage = " Required")]
        [MaxLength(20, ErrorMessage = "Max 20 chars")]
        public string EmploymentStatus { get; set; }

        public FormStatus PaymentStatus { get; set; }
        public DateTime AppliedOn { get; set; }

        [IgnoreDBWriter]
        [IgnoreDBRead]
        public ARGStudentPreviousQualifications StudentPreviousQualification { get; set; }

        [IgnoreDBWriter]
        [IgnoreDBRead]
        public ARGStudentAddress StudentAddress { get; set; }

        [IgnoreDBWriter]
        [IgnoreDBRead]
        public PaymentDetails PaymentDetail { get; set; }
    }

    public class CorrectionForm
    {
        public Guid Student_ID { get; set; }

        [IgnoreDBWriter]
        public string Photograph { get; set; }

        [IgnoreDBWriter]
        public string CUSRegistrationNo { get; set; }

        [IgnoreDBWriter]
        public PrintProgramme PrintProgramme { get; set; }

        [IgnoreDBWriter]
        public string PresentFullName { get; set; }

        [IgnoreDBWriter]
        public string PresentFathersName { get; set; }

        [IgnoreDBWriter]
        public DateTime PresentDOB { get; set; }

        [IgnoreDBWriter]
        public bool HasDegreeCertificatePrinted { get; set; }

        [IgnoreDBWriter]
        public bool HasTranscriptPrinted { get; set; }

        [DisplayName("Name (Full)")]
        [Required(ErrorMessage = " Required")]
        [MinLength(2, ErrorMessage = " Invalid")]
        [MaxLength(40, ErrorMessage = " Max 40 chars")]
        [RegularExpression(@"^([\s]*?[a-zA-Z]{1,}[ A-Za-z&-.]*)$", ErrorMessage = " Invalid")]
        public string NewFullName { get; set; }

        [DisplayName("Fathers Name (Full)")]
        [Required(ErrorMessage = " Required")]
        [MinLength(2, ErrorMessage = " Invalid")]
        [MaxLength(40, ErrorMessage = " Max 40 chars")]
        [RegularExpression(@"^([\s]*?[a-zA-Z]{1,}[ A-Za-z&-.]*)$", ErrorMessage = " Invalid")]
        public string NewFathersName { get; set; }

        [DisplayName("Date of birth as per 10th Certificate")]
        [Required(ErrorMessage = " Required")]
        public DateTime NewDOB { get; set; }


        [DisplayName(@"Reasons & Documentary proof in support
                        of effecting the change in the University
                        records")]
        [Required(ErrorMessage = " Required")]
        [MinLength(10, ErrorMessage = "Min 10 chars")]
        [MaxLength(300, ErrorMessage = "Max 300 chars")]
        public string ReasonAndDocumentaryProof { get; set; }    

        public FormStatus PaymentStatus { get; set; }
        public DateTime AppliedOn { get; set; }

        [IgnoreDBWriter]
        [IgnoreDBRead]
        public PaymentDetails PaymentDetail { get; set; }
    }
}
