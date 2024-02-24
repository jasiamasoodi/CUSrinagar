using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using CUSrinagar.ValidationAttrs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;

namespace CUSrinagar.Models
{
    public class CertificateCoursePersonalInfo : BaseWorkFlow
    {
        public Guid Student_ID { get; set; }

        [DisplayName("Select Course")]
        [Required(ErrorMessage = " Required")]
        public CertificateCourses AppliedFor { get; set; }

        [DisplayName("Full Name(First Name Middle Name Last Name)")]
        [Required(ErrorMessage = " Required")]
        [MinLength(2, ErrorMessage = " Invalid")]
        [MaxLength(75, ErrorMessage = " Max 75 chars")]
        [RegularExpression(@"^([\s]*?[a-zA-Z]{1,}[ A-Za-z&-.]*)$", ErrorMessage = " Invalid")]
        public string FullName { get; set; }

        [DisplayName("Parentage (First Name Middle Name Last Name)")]
        [Required(ErrorMessage = " Required")]
        [MinLength(2, ErrorMessage = " Invalid")]
        [MaxLength(75, ErrorMessage = " Max 75 chars")]
        [RegularExpression(@"^([\s]*?[a-zA-Z]{1,}[ A-Za-z&-.]*)$", ErrorMessage = " Invalid")]
        public string Parentage { get; set; }

        [Required(ErrorMessage = " Required")]
        [DisplayName("Board Registration No")]
        [MinLength(4, ErrorMessage = " Invalid")]
        [MaxLength(25, ErrorMessage = " Max 25 chars")]
        [RegularExpression(@"^[^\s]+$", ErrorMessage = " Invalid(remove spaces)")]
        [Remote("ValidateBoardRegnNoExists", "CertificateCourses", AreaReference.UseRoot, AdditionalFields = "Batch", ErrorMessage = "Board Registration No. has already submitted the Form for current session (Email or visit University for any queries)")]
        public string BoardRegnNo { get; set; }

        [DisplayName("Date of birth (dd-mm-yyyy) as per 10th Certificate")]
        [Required(ErrorMessage = " Required")]
        [RegularExpression(@"^(?:(?:31(\/|-|\.)(?:0?[13578]|1[02]))\1|(?:(?:29|30)(\/|-|\.)(?:0?[1,3-9]|1[0-2])\2))(?:(?:1[6-9]|[2-9]\d)?\d{2})$|^(?:29(\/|-|\.)0?2\3(?:(?:(?:1[6-9]|[2-9]\d)?(?:0[48]|[2468][048]|[13579][26])|(?:(?:16|[2468][048]|[3579][26])00))))$|^(?:0?[1-9]|1\d|2[0-8])(\/|-|\.)(?:(?:0?[1-9])|(?:1[0-2]))\4(?:(?:1[6-9]|[2-9]\d)?\d{4})$", ErrorMessage = " Invalid (remove spaces or special chars)")]
        [IgnoreDBRead]
        [IgnoreDBWriter]
        public string EnteredDOB { get; set; }
        public DateTime DOB { get; set; }

        public short Batch { get; set; }

        [DisplayName("Mobile No.")]
        [Required(ErrorMessage = "Required")]
        [Range(2000, 99999999999999, ErrorMessage = "Invalid")]
        [RegularExpression(@"\d{10,10}", ErrorMessage = "Invalid")]
        public string MobileNo { get; set; }

        [Required(ErrorMessage = " Required")]
        [DisplayName("Email")]
        [MaxLength(100, ErrorMessage = " Max 100 chars")]
        [RegularExpression(@"^([a-zA-Z0-9_.-])+@([a-zA-Z0-9_.-])+\.([a-zA-Z])+([a-zA-Z])+", ErrorMessage = " Invalid")]
        public string Email { get; set; }

        public FormStatus FormStatus { get; set; }

        public bool IsPassout { get; set; }
        public Guid? AcceptedCollege_ID { get; set; }
        public string Photograph { get; set; } = "/Content/ThemePublic/PrintImages/DefaultStudentPhoto.jpeg";

        [Required(ErrorMessage = " Required")]
        public string Gender { get; set; }

        [Required(ErrorMessage = " Required")]
        [MinLength(4, ErrorMessage = "Min 4 chars")]
        [MaxLength(150, ErrorMessage = "Max 150 chars")]
        public string Address { get; set; }

        public int TotalFee { get; set; }

        [DisplayName("Photograph")]
        [Required(ErrorMessage = " Required")]
        [ValidateFileForm(20, 70, new string[] { ".jpg", ".bmp", ".png", ".jpeg" })]
        [IgnoreDBRead]
        [IgnoreDBWriter]
        public HttpPostedFileBase PhotographPath { get; set; }

        [IgnoreDBRead]
        [IgnoreDBWriter]
        public byte[] QRCode
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(BoardRegnNo))
                {
                    return (BoardRegnNo + " | " + Batch).ToQRCode();
                }
                return null;
            }
        }

        [DisplayName("Basic Qualification")]
        public List<CertificateCoursePrevQualifications> PrevQualifications { get; set; }

        [IgnoreDBRead]
        [IgnoreDBWriter]
        public PaymentDetails PaymentDetail { get; set; }
    }
}
