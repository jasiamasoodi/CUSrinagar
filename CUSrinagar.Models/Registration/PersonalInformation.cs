using CUSrinagar.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web;
using CUSrinagar.ValidationAttrs;
using System.Web.Mvc;
using CUSrinagar.Extensions;
using System.Threading;

namespace CUSrinagar.Models
{

    public class BatchUpdatePersonalInfo
    {
        public List<Guid> Student_ID { get; set; }
        public CombinationBatchUpdate Combination { get; set; }
    }
    public class PersonalInformationCompact
    {
        public Guid Student_ID { get; set; }
        public string BoardRegistrationNo { get; set; }
        public string PreviousUniversityRegnNo { get; set; }
        public string CUSRegistrationNo { get; set; }
        public string FullName { get; set; }
        public string FathersName { get; set; }
        public string Gender { get; set; }
        public DateTime DOB { get; set; }
        public Guid? AcceptCollege_ID { get; set; }
        public Guid? AcceptedBy_ID { get; set; }
        public short CurrentSemesterOrYear { get; set; }
        public short Batch { get; set; }
        public Guid Address_ID { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string District { get; set; }
        public Guid Course_ID { get; set; }
        public string CourseCode { get; set; }
        public string CourseFullName { get; set; }
        public PrintProgramme PrintProgramme { get; set; }
        public string CollegeFullName { get; set; }
        public string ClassRollNo { get; set; }
        public FormStatus FormStatus { get; set; }

        [IgnoreDBWriter]
        public string ExamRollNumber { get; set; }

        [IgnoreDBWriter]
        public string StudentFormNo { get; set; }

        [IgnoreDBWriter]
        public int Duration { get; set; }

        [IgnoreDBWriter]
        public string MothersName { get; set; }

        [IgnoreDBWriter]
        public bool IsPassout { get; set; }
    }

    public class PersonalInformationList : BaseWorkFlow
    {
        public string StudentFormNo { get; set; }

        [DisplayName("Full Name(First Name Middle Name Last Name)")]
        [Required(ErrorMessage = " Required")]
        [MinLength(2, ErrorMessage = " Invalid")]
        [MaxLength(40, ErrorMessage = " Max 40 chars")]
        [RegularExpression(@"^([\s]*?[a-zA-Z]{1,}[ A-Za-z&-.]*)$", ErrorMessage = " Invalid")]
        public string FullName { get; set; }

        [DisplayName("Father's Name")]
        [Required(ErrorMessage = " Required")]
        [MinLength(2, ErrorMessage = " Invalid")]
        [MaxLength(40, ErrorMessage = " Max 40 chars")]
        [RegularExpression(@"^([\s]*?[a-zA-Z]{1,}[ A-Za-z&-.]*)$", ErrorMessage = " Invalid")]
        public string FathersName { get; set; }

        [DisplayName("Mother's Name")]
        [Required(ErrorMessage = " Required")]
        [MinLength(2, ErrorMessage = " Invalid")]
        [MaxLength(40, ErrorMessage = " Max 40 chars")]
        [RegularExpression(@"^([\s]*?[a-zA-Z]{1,}[ A-Za-z&-.]*)$", ErrorMessage = " Invalid")]
        public string MothersName { get; set; }

        [IgnoreDBWriter]
        public string AcceptCollegeID { get; set; }
        [IgnoreDBWriter]
        public string AcceptedByID { get; set; }

        [IgnoreDBWriter]
        public string CollegeCode { get; set; }

        public Int64? EntranceRollNo { get; set; }

        public short CurrentSemesterOrYear { get; set; }
        public short Batch { get; set; }
        public string ClassRollNo { get; set; }

        [IgnoreDBWriter]
        public string ABCID { get; set; }

    }


    public class ARGPersonalInformation : PersonalInformationList
    {
        public Guid Student_ID { get; set; }


        [IgnoreDBWriter]
        public Guid? AssignedCollege_ID { get; set; }


        [IgnoreDBWriter]
        public Guid AssignedCourse_ID { get; set; }

        [Required(ErrorMessage = " Required")]
        [DisplayName("Board Registration No")]
        [MinLength(5, ErrorMessage = " Invalid")]
        [MaxLength(40, ErrorMessage = " Max 40 chars")]
        [RegularExpression(@"^[^\s]+$", ErrorMessage = " Invalid(remove spaces)")]
        [Remote("BoardRegNoExists", "Registration", AreaReference.UseRoot, AdditionalFields = "programme", ErrorMessage = "Board Regn No. has already submitted the Form for current session. DO NOT TRY TO SUBMIT ANOTHER FORM WITH WRONG BOARD REGN NO. Otherwise both forms will be cancelled (Email or visit University for any queries)")]
        public string BoardRegistrationNo { get; set; }

        public string CUSRegistrationNo { get; set; }

        [Required(ErrorMessage = " Required")]
        [DisplayName("Previous University Regn. No.")]
        [MinLength(4, ErrorMessage = " Min 4 chars")]
        [MaxLength(30, ErrorMessage = " Max 30 chars")]
        [RegularExpression(@"^[^\s]+$", ErrorMessage = " Invalid(remove spaces)")]
        public string PreviousUniversityRegnNo { get; set; }

        [Required(ErrorMessage = " Required")]
        public string Gender { get; set; }

        [DisplayName("Date of birth (dd-mm-yyyy) as per 10th Certificate")]
        [Required(ErrorMessage = " Required")]
        [RegularExpression(@"^(?:(?:31(\/|-|\.)(?:0?[13578]|1[02]))\1|(?:(?:29|30)(\/|-|\.)(?:0?[1,3-9]|1[0-2])\2))(?:(?:1[6-9]|[2-9]\d)?\d{2})$|^(?:29(\/|-|\.)0?2\3(?:(?:(?:1[6-9]|[2-9]\d)?(?:0[48]|[2468][048]|[13579][26])|(?:(?:16|[2468][048]|[3579][26])00))))$|^(?:0?[1-9]|1\d|2[0-8])(\/|-|\.)(?:(?:0?[1-9])|(?:1[0-2]))\4(?:(?:1[6-9]|[2-9]\d)?\d{4})$", ErrorMessage = " Invalid (remove spaces or special chars)")]
        [IgnoreDBRead]
        [IgnoreDBWriter]
        [Remote("ValidateDOB", "Registration", AreaReference.UseRoot, ErrorMessage = "Invalid DOB")]
        public string EnteredDOB { get; set; }

        public DateTime DOB { get; set; }


        [DisplayName("Religion")]
        [Required(ErrorMessage = " Required")]
        public string Religion { get; set; }

        [Required(ErrorMessage = " Required")]
        public string Category { get; set; }

        public Guid? AcceptCollege_ID { get; set; } = null;

        public Guid? AcceptedBy_ID { get; set; } = null;
        public bool IsPassout { get; set; }
        public bool UploadedOnNadDigilocker { get; set; }

        [DisplayName("Photograph")]
        [Required(ErrorMessage = " Required")]
        [ValidateFileForm(20, 70, new string[] { ".jpg", ".bmp", ".png", ".jpeg" })]
        [IgnoreDBRead]
        [IgnoreDBWriter]
        public HttpPostedFileBase PhotographPath { get; set; }

        [DisplayName("Update Photograph")]
        [ValidateFileFormEdit(20, 70, new string[] { ".jpg", ".bmp", ".png", ".jpeg" })]
        [IgnoreDBRead]
        [IgnoreDBWriter]
        public HttpPostedFileBase EditPhotograph { get; set; }

        [DisplayName("Marks Card")]
        [Required(ErrorMessage = " Required")]
        [ValidateFileForm(100, 500, new string[] { ".jpg", ".bmp", ".png", ".jpeg" })]
        [IgnoreDBRead]
        [IgnoreDBWriter]
        public HttpPostedFileBase MarksCertificate { get; set; }

        [DisplayName("Category Certificate")]
        [Required(ErrorMessage = " Required")]
        [ValidateFileForm(100, 500, new string[] { ".jpg", ".bmp", ".png", ".jpeg" })]
        [IgnoreDBRead]
        [IgnoreDBWriter]
        public HttpPostedFileBase CategoryCertificate { get; set; }

        public string Photograph { get; set; } = "/DefaultStudentPhotoss.jpeg";

        [IgnoreDBRead]
        [IgnoreDBWriter]
        public byte[] QRCode
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(StudentFormNo))
                {
                    return StudentFormNo.ToQRCode();
                }
                return null;
            }
        }


        [DisplayName("12th Result")]
        [Required(ErrorMessage = " Required")]
        public bool? IsProvisional { get; set; }
        public FormStatus FormStatus { get; set; }

        [IgnoreDBWriter]
        public string Preference { get; set; }

        [IgnoreDBRead]
        [IgnoreDBWriter]
        public ARGStudentAddress StudentAddress { get; set; }

        public int TotalFee { get; set; }
        public decimal? CATEntrancePoints { get; set; }


        [DisplayName("Select Course(s)")]
        public List<ARGCoursesApplied> CoursesApplied { get; set; }

        [IgnoreDBWriter]
        [IgnoreDBRead]
        public List<ARGEntranceCentersMaster> EntranceCenters { get; set; }
        = new List<ARGEntranceCentersMaster>();

        public List<ARGStudentPreviousQualifications> AcademicDetails { get; set; }
        = new List<ARGStudentPreviousQualifications>();

        [IgnoreDBRead]
        [IgnoreDBWriter]
        public List<ARGSelectedCombination> SelectedCombinations { get; set; }

        [IgnoreDBRead]
        [IgnoreDBWriter]
        public List<PaymentDetails> PaymentDetail { get; set; }

        [IgnoreDBRead]
        [IgnoreDBWriter]
        public List<SemesterModel> Semester { get; set; }

        [IgnoreDBRead]
        [IgnoreDBWriter]
        public string CourseFullName { get; set; }

        [DisplayName("Is Lateral Entry (Check if yes)")]
        public bool IsLateralEntry { get; set; }
        public MigrationIssueStatus MigrationIssued { get; set; }

        [IgnoreDBRead]
        [IgnoreDBWriter]
        public short Duration { get; set; }

        [IgnoreDBRead]
        [IgnoreDBWriter]
        public string ExamRollNo { get; set; }

        [IgnoreDBWriter]
        public DateTime? OtherUnivMigrationReceivedOn { get; set; }

        [DisplayName("CUET Application No.")]
        [Range(2000, 9999999999999, ErrorMessage = "Invalid")]
        [RegularExpression(@"\d{12,13}", ErrorMessage = "Invalid")]
        public string CUETApplicationNo { get; set; }

        [DisplayName("CUET Application No.")]
        [MinLength(8, ErrorMessage = "Invalid")]
        [MaxLength(13, ErrorMessage = "Invalid")]
        [RegularExpression(@"^[^\s]+$", ErrorMessage = " Invalid(remove spaces)")]
        public string CUETEntranceRollNo { get; set; }
    }
    public class MigrationPI
    {
        public Guid Form_ID { get; set; }
        [DisplayName("Student")]
        [Required(ErrorMessage = "Student Details Required")]
        public Guid Student_ID { get; set; }
        public MigrationE FormType { get; set; }
        public PrintProgramme Programme { get; set; }
        public FormStatus FormStatus { get; set; }
        public int Semester { get; set; }
        public Guid AcceptedBy { get; set; }
        public bool Status { get; set; }
        public DateTime CreatedOn { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
        public Guid UpdatedBy { get; set; }
        [DisplayName("College To Which Transfer Is Sought")]
        [Required(ErrorMessage = " Required")]
        [StringLength(80)]
        public string NewCollege { get; set; }
        public string Description { get; set; }
        [DisplayName("Remarks")]
        [Required(ErrorMessage = " Required")]
        [StringLength(300)]
        public string Remarks { get; set; }
        public bool ForwardedToCollege { get; set; }
        public string RemarksByCollege { get; set; }
        public Guid OldCollege { get; set; }

        [DisplayName("New Subjects")]
        [Required(ErrorMessage = " Required")]
        [StringLength(100)]
        public string NewSubjects { get; set; }
        public int SerialNo { get; set; }

        [DisplayName("Mobile No.")]
        [Required(ErrorMessage = "Required")]
        [Range(2000, 99999999999999, ErrorMessage = "Invalid")]
        [RegularExpression(@"\d{10,10}", ErrorMessage = "Invalid")]
        public string Mobile { get; set; }

        [Required(ErrorMessage = " Required")]
        [DisplayName("Email")]
        [MaxLength(100, ErrorMessage = " Max 100 chars")]
        [RegularExpression(@"^([a-zA-Z0-9_.-])+@([a-zA-Z0-9_.-])+\.([a-zA-Z])+([a-zA-Z])+", ErrorMessage = " Invalid")]
        public string Email { get; set; }

        public int TotalFeeM { get; set; }
        [IgnoreDBWriter]
        public ResultStatus ResultStatus { get; set; }
        [IgnoreDBWriter]
        public string TransactionNo { get; set; }
        [IgnoreDBWriter]
        public DateTime TransactionDate { get; set; }
        [IgnoreDBRead]
        [IgnoreDBWriter]
        public string NewCollegeName { get; set; }
        [IgnoreDBWriter]
        public string ClassRollNo { get; set; }
        [IgnoreDBWriter]
        public int Batch { get; set; }
        [IgnoreDBWriter]
        public int Year { get; set; }
        [IgnoreDBWriter]
        public string FullName { get; set; }
        [IgnoreDBWriter]
        public string Gender { get; set; }
        [IgnoreDBWriter]
        public string FathersName { get; set; }
        [IgnoreDBWriter]
        public string MothersName { get; set; }
        [IgnoreDBWriter]
        public string CUSRegistrationNo { get; set; }
        [IgnoreDBWriter]
        public DateTime DOB { get; set; }
        [IgnoreDBWriter]
        public int CurrentSemesterOrYear { get; set; }
        [IgnoreDBWriter]
        public string CourseFullName { get; set; }
        [DisplayName("College Name")]
        [IgnoreDBWriter]
        public string AcceptCollegeID { get; set; }
        [IgnoreDBWriter]
        public Guid AcceptCollege_ID { get; set; }
        public List<ARGSelectedCombination> SelectedCombinations { get; set; }
        public List<SemesterModel> SemesterModel { get; set; }
        [IgnoreDBWriter]
        public ARGStudentAddress StudentAddress { get; set; }

        [IgnoreDBWriter]
        public bool IsLateralEntry { get; set; }
    }
    public class SelectedStudentDetail
    {
        public List<SelectedStudentInfo> selectedStudentInfo { get; set; }
        public List<SeatCount> seatCount { get; set; }
        public List<SelectedStudentInfo> ALLCatFractionList { get; set; }
        public List<SeatCountAgainstCourse> SeatCountAgainstCourseList { get; set; }
    }
    public class SelectedStudentInfo
    {
        [IgnoreDBWriter]
        public int? Sno { get; set; }

        public Guid Student_ID { get; set; }
        public string StudentFormNo { get; set; }
        public string BoardRegistrationNo { get; set; }
        public string CUSRegistrationNo { get; set; }
        public string FullName { get; set; }
        public string FathersName { get; set; }
        public string Gender { get; set; }
        public DateTime DOB { get; set; }
        public Guid? AcceptCollege_ID { get; set; }
        public Guid? AcceptedBy_ID { get; set; }
        public short Batch { get; set; }
        public Guid Address_ID { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string District { get; set; }
        public Guid Course_ID { get; set; }
        public string CourseCode { get; set; }
        public string CourseFullName { get; set; }
        public PrintProgramme PrintProgramme { get; set; }
        public decimal TotalPoints { get; set; }
        public int EntranceRollNo { get; set; }
        public decimal Percentage { get; set; }
        public string Category { get; set; }
        public StudentSelectionStatus StudentSelectionStatus { get; set; }
        public int SelectionAgaintListNo { get; set; }
        public decimal SubjectEntrancePoints { get; set; }
        public decimal AcademicPoints { get; set; }
        public decimal CATEntrancePoints { get; set; }
        public bool isNew { get; set; }
        public bool isTie { get; set; }
        public bool IsFraction { get; set; }
        public bool IsProvisional { get; set; }
        public int Preference { get; set; }
        public bool AssignedUnderCategory { get; set; }
        public bool ISFinallySelected { get; set; }
        public string CollegeFullName { get; set; }
        public string ExamBody { get; set; }

        [IgnoreDBWriter]
        public string CUETApplicationNo { get; set; }

        [IgnoreDBWriter]
        public string CUETEntranceRollNo { get; set; }

    }
    public class SeatCount
    {
        public string CategoryCode { get; set; }
        public int Percentage { get; set; }
        public decimal SeatsAvailable { get; set; }
        public decimal SeatsAssigned { get; set; }
        public SeatMatrix seatMatrices { get; set; }
    }
    public class SeatMatrix
    {
        public int RollNo { get; set; }
        public string FormNo { get; set; }
        public decimal Points { get; set; }
    }
    public class UpdateStudentStatus
    {


        [Required(ErrorMessage = " Required")]
        public Programme Programme { get; set; }

        [Required(ErrorMessage = " Required")]
        [Range(2016, 9999, ErrorMessage = " Should be > 2015")]
        public short Batch { get; set; }

        [DisplayName("Course")]
        [Required(ErrorMessage = " Required")]
        public Guid Course_ID { get; set; }

        [Required(ErrorMessage = " Required")]
        [DisplayName("RollNo's separated by , (comma)")]
        [RegularExpression(@"^(\d{6},)*\d{6}$", ErrorMessage = "Invalid (RollNo's should be 6 digits or remove comma OR NewLine at the end of list)")]
        public string RollNos { get; set; }

        [Required(ErrorMessage = " Required")]
        [DisplayName("Selection Status")]
        public StudentSelectionStatus StudentSelectionStatus { get; set; }
    }
    public class AdmissionSelectionCriteria
    {
        public string CategoryCode { get; set; }
        public int Percentage { get; set; }
        public bool IsActive { get; set; }
        [IgnoreDBRead]
        [IgnoreDBWriter]
        public decimal SeatCount { get; set; }
        [IgnoreDBRead]
        [IgnoreDBWriter]

        public bool HaveFraction { get; set; }
    }
    public class Enrollments
    {
        public string CollegeFullName { get; set; }
        public int Batch { get; set; }
        public int Enrollment { get; set; }

    }
}
