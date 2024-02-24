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
    public class ARGCoursesApplied
    {
        [IgnoreDBRead]
        [IgnoreDBWriter]
        public bool IsClicked { get; set; }

        public Guid Student_ID { get; set; }
        public Guid Course_ID { get; set; }

        [IgnoreDBWriter]
        public string CourseName { get; set; }

        [IgnoreDBWriter]
        public short Duration { get; set; }
        [IgnoreDBWriter]
        public string CourseCode { get; set; }

        [IgnoreDBWriter]
        public Programme Programme { get; set; }

        [IgnoreDBWriter]
        public PrintProgramme PrintProgramme { get; set; }

        [IgnoreDBWriter]
        [IgnoreDBRead]
        public ARGCentersAllotmentMaster EntranceCentersAllotment { get; set; }

        public short? Preference { get; set; }
        public decimal SubjectEntrancePoints { get; set; }

        public bool AppearedInEntrance { get; set; }

        public StudentSelectionStatus StudentSelectionStatus { get; set; }
        public int SelectionAgaintListNo { get; set; }
        
        public Guid? SelfFinancedPayment_ID { get; set; }

        public bool IsActive { get; set; } = true;

        [IgnoreDBWriter]
        [IgnoreDBRead]
        public PaymentDetails PaymentDetail { get; set; }

        [IgnoreDBWriter]
        public string GroupName { get; set; }

        [IgnoreDBWriter]
        [IgnoreDBRead]
        public bool AlreadyAppliedWithFee { get; set; }

        [DisplayName("Over All CGPA")]
        [Range(typeof(double), "24", "200", ErrorMessage = "min 24credits")]
        public double? SubjectCGPA { get; set; }

        [IgnoreDBWriter]
        [IgnoreDBRead]
        [DisplayName("CUET Percentile Score")]
        [Range(typeof(double), "0", "100", ErrorMessage = "invalid")]
        public double? CUETPercentileScore { get; set; }

        [IgnoreDBWriter]
        [IgnoreDBRead]
        [DisplayName("CUET Entrance Points (In Figure)")]
        [Range(typeof(double), "0", "400", ErrorMessage = "Allowed only 0-400")]
        public double? CUETEntrancePoints { get; set; }
    }

    public class UpdateCertificates
    {
        [DisplayName("Marks Card")]
        [ValidateFileForm(100, 500, new string[] { ".jpg", ".bmp", ".png", ".jpeg" })]
        [IgnoreDBRead]
        [IgnoreDBWriter]
        public HttpPostedFileBase CertificateMarksCard { get; set; }

        public string OldMarksCardPath { get; set; }
        public Guid Certificate_ID { get; set; }
        public List<ARGCoursesApplied> coursesApplied { get; set; }
    }
}
