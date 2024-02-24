using CUSrinagar.Enums;
using CUSrinagar.ValidationAttrs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CUSrinagar.Models
{
    public class BatchUpdateSelectionStatus
    {
        public List<Guid> Student_ID { get; set; }
        public Guid College_ID { get; set; }
        public Guid Course_ID { get; set; }
        public StudentSelectionStatus StudentSelectionStatus { get; set; }


    }
    public class StudentCollegePreference : BaseWorkFlow
    {
        public Guid Preference_ID { get; set; }
        public Guid College_ID { get; set; }
        [IgnoreDBWriter]
        public string CollegeFullName { get; set; }
        public Guid Student_ID { get; set; }
        public Guid? OldStudent_ID { get; set; }
        public Guid Course_ID { get; set; }
        [IgnoreDBWriter]
        public string CourseFullName { get; set; }
        public short PreferenceOrder { get; set; }
        public bool IsActive { get; set; }
        public bool IsAllowed { get; set; }
    }

    public class StudentCollegePreferenceList
    {
        public short? Batch { get; set; }
        public Guid College_ID { get; set; }
        public string CollegeFullName { get; set; }
        public Guid Course_ID { get; set; }
        public string CourseFullName { get; set; }
        public Guid Student_ID { get; set; }
        public string StudentFormNo { get; set; }
        public string FullName { get; set; }
        public string Gender { get; set; }
        public string EntranceRollNo { get; set; }
        public string Category { get; set; }
        public decimal? CATEntrancePoints { get; set; }
        public decimal? QualificationPoints { get; set; }
        public decimal? QualificationPercentage { get; set; }
        public decimal? TotalPoints { get; set; }
        public short PreferenceOrder { get; set; }
        public bool IsActive { get; set; }
        public FormStatus FormStatus { get; set; }
        public StudentSelectionStatus StudentSelectionStatus { get; set; }
        public bool IsAllowed { get; set; }
        public DateTime CreatedOn { get; set; }
        [IgnoreDBWriter]
        public DateTime TxnDate { get; set; }


    }

    public class StudentCollegeCourseCount
    {
        public string CollegeFullName { get; set; }
        public string CollegeCode { get; set; }
        public string CourseFullName { get; set; }
        public int IntakeCapacity { get; set; }
        public int Count { get; set; }
    }

    public class Certificate
    {
        public Guid Certificate_ID { get; set; }
        public Guid Student_ID { get; set; }
        public CertificateType CertificateType { get; set; }

        [IgnoreDBRead]
        [IgnoreDBWriter]
        [ValidateFileForm(70, 200, new string[] { ".jpg", ".bmp", ".png", ".jpeg" })]
        [Required(ErrorMessage = " Required")]
        public HttpPostedFileBase File { get; set; }

        public string CertificateUrl { get; set; }
        public VerificationStatus VerificationStatus { get; set; }
        public DateTime? UploadingDate { get; set; }
        public DateTime? VerificationDate { get; set; }
        public Guid? VerifiedBy { get; set; }
        public string Remark { get; set; }

        [IgnoreDBWriter]
        [IgnoreDBRead]
        public RecordState RecordState { get; set; }
    }

}
