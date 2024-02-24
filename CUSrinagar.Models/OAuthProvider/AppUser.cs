using CUSrinagar.Enums;
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
    public class AppUsers : BaseWorkFlow
    {
        public Guid User_ID { get; set; }

        [Required(ErrorMessage = " Required")]
        [DisplayName("UserName ")]
        [MinLength(6, ErrorMessage = " Min 6 chars")]
        [MaxLength(30, ErrorMessage = " Max 30 chars")]
        [RegularExpression(@"^([a-zA-Z0-9_]*){8,30}$", ErrorMessage = " Invalid (Remove Space and Special chars)")]
        public string UserName { get; set; }

        [DisplayName("College Name ")]
        public Guid? College_ID { get; set; }

        [Required(ErrorMessage = " Required")]
        [DisplayName("FullName ")]
        public string FullName { get; set; }

        [Required(ErrorMessage = " Required")]
        [DisplayName("Email")]
        [MaxLength(75, ErrorMessage = " Max 75 chars")]
        [RegularExpression(@"^([a-zA-Z0-9_.-])+@([a-zA-Z0-9_.-])+\.([a-zA-Z])+([a-zA-Z])+", ErrorMessage = " Invalid")]
        public string Email { get; set; }

        public bool EmailConfirmed { get; set; }

        [Required(ErrorMessage = " Required")]
        [DisplayName("Password")]
        [MinLength(8, ErrorMessage = " Min 8 chars")]
        [MaxLength(30, ErrorMessage = " Max 30 chars")]
        [RegularExpression(@"((?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[\W]).{8,30})", ErrorMessage = " Invalid")]
        public string Password { get; set; }

        [Required(ErrorMessage = " Required")]
        [DisplayName("Phone Number")]
        public string PhoneNumber { get; set; }
        public int AccessFailedCount { get; set; }
        public DateTime? LockoutEndDateUtc { get; set; }
        public bool LockoutEnabled { get; set; }
        public Guid SecurityStamp { get; set; }
        public bool Status { get; set; }

        [Required(ErrorMessage = " Required")]
        [DisplayName("Designation")]
        [MinLength(4, ErrorMessage = " Min 4 chars")]
        [MaxLength(30, ErrorMessage = " Max 30 chars")]
        public string Designation { get; set; }
        public byte[] Photograph { get; set; }
        public string PhotographType { get; set; }
        public string PasswordResetToken { get; set; }
        public DateTime? TokenIssuedOn { get; set; }
        public DateTime LastPasswordResetDate { get; set; }

        [DisplayName("Evaluator ID")]
        public string Evaluator_ID { get; set; }
        [IgnoreDBWriter]
        [DisplayName("Department Name ")]
        public Guid? Department_ID { get; set; }
        [IgnoreDBRead]
        [IgnoreDBWriter]
        public HttpPostedFileBase UploadPhotograph { get; set; }

        [DisplayName("Roles")]
        public List<AppUserRoles> UserRoles { get; set; }

        public List<AppUserProfessorSubjects> ProfessorSubjects { get; set; }

        public List<AppUserEvaluatorSubjects> EvalvatorSubjects { get; set; }
        [IgnoreDBWriter]
        public bool ShowBacklog { get; set; }
        [IgnoreDBWriter]
        public string Institute { get; set; }

        [IgnoreDBRead]
        [IgnoreDBWriter]
        public BIL_BankAccountDetails BankAccountDetail { get; set; }

    }

    public class AppUserCompact
    {
        public Guid User_ID { get; set; }
        public Guid? College_ID { get; set; }
        public string FullName { get; set; }
        public string CollegeCode { get; set; }
        public Guid SecurityStamp { get; set; }
        public List<AppRoles> UserRoles { get; set; }
        public string EmailId { get; set; }
        public string Designation { get; set; }
        public PrintProgramme TableSuffix { get; set; }
        public PrintProgramme OrgPrintProgramme { get; set; }
        public DateTime LastPasswordResetDate { get; set; }
        public short OrgBatch { get; set; }
    }
}
