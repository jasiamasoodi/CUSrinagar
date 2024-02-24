using CUSrinagar.Enums;
using CUSrinagar.Models.ValidationAttrs;
using GeneralModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Web;

namespace CUSrinagar.Models
{
    public class ExaminationFormSummary
    {
        public PrintProgramme PrintProgramme { get; set; }

        public string PrintProgrammeDescription { get { return PrintProgramme.ToString(); } }

        public int InProcess { get; set; }

        public int Accepted { get; set; }

        public int NotDownloaded { get; set; }

        public int TotalStudent { get; set; }
    }

    public class StudentExamForm : PersonalInformationCompact
    {
        public short Semester { get; set; }

        public string FormNumber { get; set; }

        public short? Year { get; set; }

        public new string ExamRollNumber { get; set; }

        public decimal? AmountPaid { get; set; }

        public FormStatus Status { get; set; }

        public bool IsRegular { get; set; }

        public bool IsVerified { get; set; }

    }
    public class StudentExamFormList
    {
        [Description("ExaminationForms.")]
        public DateTime CreatedOn { get; set; }

        [Description("ExaminationForms.")]
        public DateTime? UpdatedOn { get; set; }

        public Guid Student_ID { get; set; }
        public Guid AcceptCollege_ID { get; set; }

        public string CUSRegistrationNo { get; set; }

        public string FullName { get; set; }

        [Description("ExaminationForms.")]
        public short Semester { get; set; }

        public string FormNumber { get; set; }

        public short? Year { get; set; }

        public short SemesterBatch { get; set; }

        public string ExamRollNumber { get; set; }

        public string ClassRollNo { get; set; }

        public decimal? AmountPaid { get; set; }

        [Description("ExaminationForms.")]
        public FormStatus Status { get; set; }

        public Guid StudentExamForm_ID { get; set; }

        public bool IsRegular { get; set; }

        public bool IsVerified { get; set; }

        public Guid Course_ID { get; set; }

        public string CourseFullName { get; set; }

        public PrintProgramme Programme { get; set; }

    }

    public class ARGStudentExamForm : BaseWorkFlow
    {
        public Guid StudentExamForm_ID { get; set; }

        public Guid Student_ID { get; set; }

        public short Semester { get; set; }

        public string FormNumber { get; set; }

        public short? Year { get; set; }

        public string ExamRollNumber { get; set; }

        public decimal AmountPaid { get; set; }

        [IgnoreDBRead, IgnoreDBWriter]
        public decimal AmountToBePaid { get; set; }

        public decimal LateFeeAmount { get; set; }

        public FormStatus Status { get; set; }

        public bool IsRegular { get; set; }

        public Guid Notification_ID { get; set; }

        [IgnoreDBRead, IgnoreDBWriter]
        public ARGPersonalInformation StudentInfo { get; set; }

        [IgnoreDBRead, IgnoreDBWriter]
        public RecordState RecordState { get; set; }

        public bool IsDivisionImprovement { get; set; }
        public short PendingFeeAmount { get; set; }
    }


    public class ARGStudentExamFormSubjects : ARGStudentExamForm
    {
        [IgnoreDBRead, IgnoreDBWriter]
        public List<ARGStudentReExamForm> ReAppearSubjects { get; set; } = new List<ARGStudentReExamForm>();

        [IgnoreDBRead, IgnoreDBWriter]
        public List<StudentAdditionalSubject> AdditionalSubjects { get; set; } = new List<StudentAdditionalSubject>();
    }

    public class ARGExamFormDownloadable
    {
        public Guid ARGExamForm_ID { get; set; }

        public ExaminationCourseCategory CourseCategory { get; set; }

        public PrintProgramme PrintProgramme { get; set; }

        public short Semester { get; set; }

        [IgnoreDBWriter]
        public string FormNumberPrefix { get; set; }

        public int FormNumberCount { get; set; }

        public bool AllowDownloadForm { get; set; }

        public short Year { get; set; }

        [IgnoreDBWriter]
        public decimal? TotalFee { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public bool AllowDownloadAdmitCards { get; set; }

        public string Remark { get; set; }

        public int LateFee1 { get; set; }

        public int LateFee2 { get; set; }

        public int GracePeriodDays1 { get; set; }

        public bool IsGracePeriod { get; set; }

        public int GracePeriodLateFeeAmount { get; set; }

        public int GracePeriodDays2 { get; set; }

        public int HardCodeFeeAmount { get; set; }

        public Guid Notification_ID { get; set; }

        public bool IsHardCodeFeeSet { get; set; }

        [Required(ErrorMessage = " FeeStructure Required")]
        public Guid FeeStructure_ID { get; set; }

        public bool AllowInCenterAllotment { get; set; }

        public bool IsBacklogSetting { get; set; }

        [Range(2016, 9999, ErrorMessage = "RegularBatch should be from 2016")]
        public short RegularBatch { get; set; }
        
        public int MinimumFee { get; set; }
        
        public int MaximumFee { get; set; }
        public bool ValidateByMinMaxFee { get; set; }

    }

    public class FeeStructure
    {
        public Guid FeeStructure_ID { get; set; }

        public int FeePerSubject { get; set; }

        public int MinimumFee { get; set; }

        public int OtherCharges { get; set; }

        public int ExaminationFund { get; set; }

        public int ITComponent { get; set; }
        public bool Status { get; set; }

    }

    public class ARGStudentReExamForm : BaseWorkFlow
    {
        public Guid StudentReExamForm_ID { get; set; }
        public Guid StudentExamForm_ID { get; set; }
        public PrintProgramme Programme { get; set; }
        public Guid Subject_ID { get; set; }
        [IgnoreDBWriter]
        public string SubjectFullName { get; set; }

        [IgnoreDBWriter]
        public SubjectType SubjectType { get; set; }

        [IgnoreDBWriter]
        public string SubjectCode { get; set; }

        public ReExamType Type { get; set; }

        [IgnoreDBWriter]
        public bool IsApplied { get; set; }

        [IgnoreDBWriter]
        public RecordState RecordState { get; set; }

        public FormStatus FeeStatus { get; set; }

        [IgnoreDBWriter]
        public int TheoryMarks { get; set; }

        [IgnoreDBWriter]
        public int PracticalAttendance { get; set; }

        [IgnoreDBWriter]
        public int TheoryAttendance { get; set; }

        [IgnoreDBWriter]
        public int PracticalMarks { get; set; }
        [IgnoreDBWriter]
        public bool HasExaminationFee { get; set; }

        [IgnoreDBRead]
        [IgnoreDBWriter]
        public bool HasInternalPassed { get; set; }
    }

    public class InEligibleCandidates
    {
        public Guid InEligibleCandidate_ID { get; set; }

        public Guid Student_ID { get; set; }

        public int Year { get; set; }

        public short Semester { get; set; }

        public string Reason { get; set; }

        public bool BlockFullForm { get; set; }

        public string Subjects { get; set; }
    }


    public class ExaminationFormWidget
    {
        public int TotalNumberOfStudents { get; set; }

        public short Semester { get; set; }

        public int Batch { get; set; }

        public int TotalAmount { get; set; }

        public int TotalRegularStudents { get; set; }

        public int TotalBacklogStudents { get; set; }

        public int TotalForms { get; set; }

        public int TotalSuccessfullPayments { get; set; }

        public int LateFeeAmount { get; set; }

        public List<CourseWiseWidget> CourseWidgets { get; set; }

    }

    public class CourseWiseWidget
    {
        public string CourseFullName { get; set; }

        public int TotalNumberOfStudentsPerCourse { get; set; }

        public int FormApplied { get; set; }

        public int NoOfSuccessfulPayments { get; set; }

        public int TotalAmountReceived { get; set; }

        public int NoOfRegularStudents { get; set; }

        public int NoOfBacklogStudents { get; set; }

        public int LateFeeAmountForRegularStudents { get; set; }

        public int LateFeeAmountForBackLogStudents { get; set; }
    }

    public class AdmissionFormWidget
    {
        public int Batch { get; set; }

        public string Gender { get; set; }

        public int NoOfStudents { get; set; }
    }


    public class NewAdmissionsWidget
    {
        public int Batch { get; set; }

        public string CourseFullName { get; set; }

        public int NoOfStudents { get; set; }
    }

    public class AdmissionFormWidgetCourseWise
    {
        public PrintProgramme PrintProgramme { get; set; }

        public string CourseFullName { get; set; }

        public int NoOfStudents { get; set; }
    }


    public class CourseWiseGenderCountWidget
    {
        public int Batch { get; set; }

        public string Gender { get; set; }

        public int NoOfStudents { get; set; }
    }

    public class ExaminationWhiteListCompact : BaseWorkFlow
    {
        public Guid WhiteList_ID { get; set; }

        [Required(ErrorMessage = "Semester cannot be Empty.")]
        public int Semester { get; set; }

        [Required(ErrorMessage = "Examination Year cannot be Empty.")]
        public int ExaminationYear { get; set; }

        public Guid Student_ID { get; set; }

        public Guid AllowedBy_ID { get; set; }

        public string Remarks { get; set; }

        public bool IsBacklog { get; set; }

        [IgnoreDBWriter]
        public string CUSRegistrationNo { get; set; }

        [IgnoreDBRead]
        [IgnoreDBWriter]
        public HttpPostedFileBase Files { get; set; }

        public string ApplicationPath { get; set; }

    }

    public class ExaminationWhiteList : ExaminationWhiteListCompact
    {
        public string StudentName { get; set; }

        public string AllowedByName { get; set; }

    }


    public class RegularExaminationForm : BaseWorkFlow
    {
        public Guid StudentExamForm_ID { get; set; }

        public Guid Student_ID { get; set; }

        public int Semester { get; set; }

        public string FormNumber { get; set; }

        public int ExaminationYear { get; set; }

        public string ExamRollNumber { get; set; }

        public decimal AmountPaid { get; set; }

        [IgnoreDBRead, IgnoreDBWriter]
        public decimal AmountToBePaid { get; set; }

        public decimal LateFeeAmount { get; set; }

        public FormStatus Status { get; set; }

        public Guid Notification_ID { get; set; }

        [IgnoreDBRead, IgnoreDBWriter]
        public RecordState RecordState { get; set; }

        [IgnoreDBRead, IgnoreDBWriter]
        public ARGExamFormDownloadable ExaminationSettings { get; set; } = new ARGExamFormDownloadable();

        [IgnoreDBRead, IgnoreDBWriter]
        public ARGSelectedCombination SelectedCombination { get; set; } = new ARGSelectedCombination();

        [IgnoreDBRead, IgnoreDBWriter]
        public List<StudentAdditionalSubject> AdditionalSubjects { get; set; } = new List<StudentAdditionalSubject>();

        [IgnoreDBRead, IgnoreDBWriter]
        public ResponseData Response { get; set; }

    }

    public class BacklogExaminationForm : BaseWorkFlow
    {
        public Guid StudentExamForm_ID { get; set; }

        public Guid Student_ID { get; set; }

        public short Semester { get; set; }

        public string FormNumber { get; set; }

        public short ExaminationYear { get; set; }

        public string ExamRollNumber { get; set; }

        public decimal AmountPaid { get; set; }

        [IgnoreDBRead, IgnoreDBWriter]
        public decimal AmountToBePaid { get; set; }

        public decimal LateFeeAmount { get; set; }

        public FormStatus Status { get; set; }

        public Guid Notification_ID { get; set; }

        [IgnoreDBRead, IgnoreDBWriter]
        public RecordState RecordState { get; set; }

        public ARGExamFormDownloadable ExaminationSettings { get; set; } = new ARGExamFormDownloadable();

        public ResultCompact Result { get; set; } = new ResultCompact();

        [IgnoreDBRead, IgnoreDBWriter]
        public List<ARGStudentReExamForm> ReAppearSubjects { get; set; } = new List<ARGStudentReExamForm>();

        [IgnoreDBRead, IgnoreDBWriter]
        public List<StudentAdditionalSubject> AdditionalSubjects { get; set; } = new List<StudentAdditionalSubject>();
    }

    public class RegularExaminationViewModel
    {
        public string FullName { get; set; }

        public Guid Student_ID { get; set; }

        public string RegistrationNumber { get; set; }

        public int CurrentSemester { get; set; }

        public int ApplyingForSemester { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public decimal FeeAmount { get; set; }

        public ARGExamFormDownloadable ExaminationSettings { get; set; } = new ARGExamFormDownloadable();

        public ARGSelectedCombination SelectedCombination { get; set; }

        public List<StudentAdditionalSubject> AdditionalSubjects { get; set; }

    }

}

