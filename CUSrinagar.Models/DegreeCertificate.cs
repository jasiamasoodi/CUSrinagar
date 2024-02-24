using CUSrinagar.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUSrinagar.Models
{
    public class DegreeCertificate
    {
        public Guid DegreeCertificate_ID { get; set; }
        public Guid Student_ID { get; set; }

        [IgnoreDBWriter]
        public string Photograph { get; set; }

        public string DispatchNumber { get; set; }
        /// <summary>
        /// Stamp number
        /// </summary>
        public int IssueNumber { get; set; }

        [IgnoreDBWriter]
        public int SerialNumber { get; set; }

        [IgnoreDBWriter]
        public string ExamRollNumber { get; set; }

        [IgnoreDBWriter]
        public string CUSRegistrationNo { get; set; }

        [IgnoreDBWriter]
        public string FullName { get; set; }

        [IgnoreDBWriter]
        public string AwardedDegreeTitle { get; set; }
        [IgnoreDBWriter]
        public string DegreeCourseTitle { get; set; }
        [IgnoreDBWriter]
        public Guid Course_Id { get; set; }
        [IgnoreDBWriter]
        public string SchoolOf { get; set; }
        public int SemesterTo { get; set; }
        [IgnoreDBWriter]
        public int SemesterFrom { get; set; }
        [IgnoreDBWriter]
        public decimal CGPA_Type1 { get; set; }

        [IgnoreDBWriter]
        public string CGPA_Type1_Grade { get; set; }

        [IgnoreDBRead]
        [IgnoreDBWriter]
        public string CGPA_Type1_Division
        {
            get
            {
                var percentage = CGPA_Type1 * 10;
                if (percentage > 60)
                    return "First Division";
                else return "Second Division";

            }
        }
        [IgnoreDBRead]
        [IgnoreDBWriter]
        public decimal? CGPA_Type2 { get; set; }

        [IgnoreDBWriter]
        public string CGPA_Type2_Grade { get; set; }

        [IgnoreDBRead]
        [IgnoreDBWriter]
        public string CGPA_Type2_Division
        {
            get
            {
                if (!CGPA_Type2.HasValue || CGPA_Type2.Value == 0) return null;
                return (CGPA_Type2 * 10 > 60 ? "First Division" : "Second Division");
            }
        }

        public string Eng_Division
        {
            get
            {
                if (Percentage >= 60) return "First Division";
                else if (Percentage < 50) return "Second Division";
                else if (Percentage < 45) return "Third Division";
                else return "Fail";
            }
        }

        [IgnoreDBWriter]
        public decimal? Percentage { get; set; }

        [IgnoreDBWriter]
        [IgnoreDBRead]
        public string Mode { get { return "Regular"; } }

        [IgnoreDBWriter]
        public string CollegeFullName { get; set; }

        [IgnoreDBWriter]
        public string CollegeAddress { get; set; }

        [IgnoreDBWriter]
        public DateTime DegreeCompletionDate { get; set; }

        [IgnoreDBWriter]
        public DateTime? CreatedOn { get; set; }

        public Guid? CreatedBy_ID { get; set; }
        public DateTime? ValidatedOn { get; set; }
        public Guid? ValidatedBy_ID { get; set; }
        public DateTime? PrintedOn { get; set; }
        public Guid? PrintedBy_ID { get; set; }
        public DateTime? HandedOverOn { get; set; }
        public Guid? HandedOverBy_ID { get; set; }

        [IgnoreDBRead]
        [IgnoreDBWriter]
        public bool HasDegreeCertificate { get { return CreatedOn.HasValue && CreatedBy_ID.HasValue && !string.IsNullOrEmpty(DispatchNumber) && SerialNumber > 0; } }
        [IgnoreDBRead]
        [IgnoreDBWriter]
        public bool IsValidated { get { return HasDegreeCertificate && ValidatedOn.HasValue && ValidatedBy_ID.HasValue; } }
        [IgnoreDBRead]
        [IgnoreDBWriter]
        public bool IsPrinted { get { return IsValidated && PrintedOn.HasValue && PrintedBy_ID.HasValue; } }
        [IgnoreDBRead]
        [IgnoreDBWriter]
        public bool IsHandedOver { get { return IsPrinted && HandedOverOn.HasValue && HandedOverBy_ID.HasValue; } }
        [IgnoreDBRead]
        [IgnoreDBWriter]
        public byte[] QRCode
        {
            get
            {
                string _qrLabel = "";
                if (!string.IsNullOrEmpty(CGPA_Type2_Division))
                    _qrLabel = $"{FullName}/RegistrationNo:{CUSRegistrationNo}/Degree:{DegreeCourseTitle}/CGPA_Theory:{CGPA_Type1}/CGPA_PracticeOfTeaching:{CGPA_Type2}/Mode:{Mode}/DateofPassing:{DegreeCompletionDate.ToString("dd-MMM-yyyy")}";
                else
                    _qrLabel = $"{FullName}/RegistrationNo:{CUSRegistrationNo}/Degree:{DegreeCourseTitle}/CGPA:{CGPA_Type1}/Mode:{Mode}/DateofPassing:{DegreeCompletionDate.ToString("dd-MMM-yyyy")}";
                return _qrLabel.ToQRCode();
            }
        }
        [IgnoreDBWriter]
        public string Remarks { get; set; }
        [IgnoreDBWriter]
        public string CourseFullName { get; set; }
        [IgnoreDBWriter]
        public string Batch { get; set; }

        [IgnoreDBWriter]
        public short TotalDuplicatesIssued { get; set; }

        [IgnoreDBWriter]
        public string DuplicateIssuanceDetails { get; set; }

        [IgnoreDBWriter]
        public bool IsLateralEntry { get; set; }

        [IgnoreDBWriter]
        public string PreviousUniversityRegnNo { get; set; }

        [IgnoreDBWriter]
        public DateTime? OtherUnivMigrationReceivedOn { get; set; }

    }
}
