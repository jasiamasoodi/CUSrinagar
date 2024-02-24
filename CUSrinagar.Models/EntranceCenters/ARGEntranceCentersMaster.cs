using CUSrinagar.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using CUSrinagar.Extensions;
using System.Web.Mvc;

namespace CUSrinagar.Models
{
    public class ARGEntranceCentersMaster : BaseWorkFlow
    {
        public Guid Center_ID { get; set; }
        public string CenterCode { get; set; }

        [IgnoreDBWriter]
        public string CenterName { get; set; }
        public Guid College_ID { get; set; }
        public int Capacity { get; set; }
        public bool Status { get; set; }
        public bool IsEntrance { get; set; }


        [IgnoreDBWriter]
        [IgnoreDBRead]
        public ADMCollegeMaster CenterContactDetails { get; set; }
        public List<CenterAllotmentDetail> CenterAllotmentDetails { get; set; }
        public List<DisplayEntranceCenterAllotmentDetail> DisplayEntranceCenterAllotmentDetail { get; set; }
    }

    public class SemesterExamCenterDetails
    {
        public string CenterCode { get; set; }
        public string CollegeCode { get; set; }
        public string CollegeFullName { get; set; }
        public string Address { get; set; }
        public string District { get; set; }
        public string PinCode { get; set; }
        public string ContactNo1 { get; set; }
        public string Email { get; set; }
        public DateTime EntranceDate { get; set; }
        public string Time { get; set; }
    }

    public class CenterAllotmentDetail
    {
        public string CourseFullName { get; set; }
        public short Semester { get; set; }
        public int TotalStudentsAssigned { get; set; }
        public short Year { get; set; }
        public string NotificationDescription { get; set; }
        public string NotificationURL { get; set; }
    }

    public class SemesterCenters
    {
        [DisplayName("Center College/Location")]
        [Required(ErrorMessage = " Required")]
        public Guid College_ID { get; set; }

        public Guid Center_ID { get; set; }

        [DisplayName("Enter Center Codes Separated by |(Pipe)")]
        [Required(ErrorMessage = " Required")]
        [RegularExpression(@"^[a-zA-Z0-9-|(). ]+$", ErrorMessage = "Only number, alphabets and - (dash) are allowed in Center Code.")]
        public string PipeSeparatedCenters { get; set; }

        public List<string> CenterCodes
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(PipeSeparatedCenters))
                {
                    return PipeSeparatedCenters.ToListOfStrings()?.Where(x => !string.IsNullOrWhiteSpace(x))?.ToList();
                }
                return null;
            }
        }


        [DisplayName("Center Category")]
        [Required(ErrorMessage = " Required")]
        public bool IsEntrance { get; set; }

        [IgnoreDBRead]
        [IgnoreDBWriter]
        public RePrintCenterNotice PrintCenterNotice { get; set; }

        [IgnoreDBRead]
        [IgnoreDBWriter]
        public SelfFinancedList SelfFinancedList { get; set; }

        [IgnoreDBRead]
        [IgnoreDBWriter]
        public PDFMeritListFilter PDFListFilter { get; set; }
    }

    public class CenterNotice
    {
        public Guid College_ID { get; set; }
        public string CollegeFullName { get; set; }

        public Guid Course_ID { get; set; }
        public string CourseFullName { get; set; }
        public string CenterCode { get; set; }
        public Guid Center_ID { get; set; }

        public short Semester { get; set; }
        public short ExamYear { get; set; }

        public Programme CourseCategory { get; set; }
        public string RegularSeries { get; set; }
        public string BacklogSeries { get; set; }
        public int TotalStudentesAssigned { get; set; }
    }

    public class RePrintCenterNotice
    {
        [DisplayName("Center College/Location")]
        [Required(ErrorMessage = " Required")]
        public Guid College_ID { get; set; }

        [DisplayName("Semester-")]
        [Required(ErrorMessage = " Required")]
        [Range(typeof(short), "1", "16", ErrorMessage = " Invalid Semester")]
        public short Semester { get; set; }

        [DisplayName("Programme")]
        [Required(ErrorMessage = " Required")]
        public Programme CourseCategory { get; set; }

        [DisplayName("Examination Year")]
        [Required(ErrorMessage = " Required")]
        [Range(typeof(short), "2016", "9999", ErrorMessage = " Invalid Examination Year")]
        public short ExaminationYear { get; set; }

        [DisplayName("List Type")]
        public ExamFormListType ListType { get; set; }

        [DisplayName("Entrance List Type")]
        public EntranceExcelListType EntranceExcelListType { get; set; }

        [DisplayName("Lateral Entry Only")]
        public bool LateralEntryOnly { get; set; }

        [DisplayName("Exm Frm Submitted from")]
        public DateTime? ExmFrmSubmittedFrom { get; set; }
    }

    public class PendingExamRollNos
    {
        public Guid StudentExamForm_ID { get; set; }
        public Guid Student_ID { get; set; }
        public Guid Course_ID { get; set; }
        public short Batch { get; set; }
        public short Semester { get; set; }
        public Guid AcceptCollege_ID { get; set; }
    }

    public class IndividualSemDetails
    {
        public string FullName { get; set; }
        public string ExamFormNo { get; set; }
        public string CourseFullName { get; set; }
        public string CollegeFullName { get; set; }
        public Guid Course_ID { get; set; }
        public Guid Student_ID { get; set; }
        public FormStatus FormStatus { get; set; }
        public PrintProgramme PrintProgramme { get; set; }
        public string ExamRollNoToSet { get; set; }
        public Guid StudentExamForm_ID { get; set; }
    }

    public class AssignEntranceCenters
    {
        [DisplayName("Programme")]
        [Required(ErrorMessage = " Required")]
        public PrintProgramme PrintProgramme { get; set; }

        [DisplayName("Batch")]
        [Required(ErrorMessage = " Required")]
        [Range(2016, 9999, ErrorMessage = "Should be >= 2016")]
        public short Batch { get; set; }

        [DisplayName("Course")]
        [Required(ErrorMessage = " Required")]
        public Guid Course_ID { get; set; }

        [DisplayName("Entrance Date")]
        [Required(ErrorMessage = " Required")]
        [RegularExpression(@"^(?:(?:31(\/|-|\.)(?:0?[13578]|1[02]))\1|(?:(?:29|30)(\/|-|\.)(?:0?[1,3-9]|1[0-2])\2))(?:(?:1[6-9]|[2-9]\d)?\d{2})$|^(?:29(\/|-|\.)0?2\3(?:(?:(?:1[6-9]|[2-9]\d)?(?:0[48]|[2468][048]|[13579][26])|(?:(?:16|[2468][048]|[3579][26])00))))$|^(?:0?[1-9]|1\d|2[0-8])(\/|-|\.)(?:(?:0?[1-9])|(?:1[0-2]))\4(?:(?:1[6-9]|[2-9]\d)?\d{4})$", ErrorMessage = " Invalid (remove spaces or special chars)")]
        [IgnoreDBRead]
        [IgnoreDBWriter]
        public string EnteredEntranceDate { get; set; }
        public DateTime EntranceDate { get; set; }


        [DisplayName("Time")]
        [Required(ErrorMessage = " Required")]
        [RegularExpression(@"^\d{1,2}:\d{2}\s[AM|PM]{2}", ErrorMessage = "Invalid (AM / PM should be in Upper Case e,g: 12:00 PM)")]
        public string Time { get; set; }

        public int TotalEntranceFormsAvailable { get; set; }

        public List<AssignEntranceCentersAs> AssignEntranceCentersAs { get; set; }

        [IgnoreDBRead]
        [IgnoreDBWriter]
        public SelectList DistrictsDDL { get; set; }

        [IgnoreDBRead]
        [IgnoreDBWriter]
        public RelocateCenter RelocateCenters { get; set; }
    }

    public class AssignEntranceCentersAs
    {
        public bool HasToBeAssigned { get; set; }

        public Guid Center_ID { get; set; }

        public string CenterCode { get; set; }

        public string CollegeFullname { get; set; }

        [DisplayName("Assignment Order")]
        [Range(1, 999, ErrorMessage = "Should be 1-999")]
        public short? AssignmentOrder { get; set; }


        [DisplayName("No. Of Students To Be Assigned Per Center")]
        [Range(1, 99999, ErrorMessage = "Should be 1-99999")]
        public short? NoOfStudentsToBeAssignedPerCenter { get; set; }

        [DisplayName("Has Already Some Students")]
        public bool HasAlreadySomeStudents { get; set; }

        [IgnoreDBRead]
        [IgnoreDBWriter]
        public List<string> Districts { get; set; }
    }

    public class DisplayEntranceCenterAllotmentDetail
    {
        public string CollegeFullName { get; set; }
        public string CenterCode { get; set; }
        public int TotalStudentsAssigned { get; set; }
        public string CourseFullName { get; set; }
        public DateTime EntranceDate { get; set; }
    }
    public class EntranceFormsCourseCount
    {
        public int TotalForms { get; set; }
        public Guid Course_ID { get; set; }
    }
    public class EntranceResult
    {
        public long EntranceRollNo { get; set; }
        public decimal EntracePoints { get; set; }
        public Guid Course_ID { get; set; }
        public short Batch { get; set; }
        public Programme Programme { get; set; }
    }

    public class SelfFinancedList
    {
        [Required(ErrorMessage = " Required")]
        [DisplayName("SelfFinanced Download Type")]
        public SelfFinancedDownloadType SelfFinancedDownloadType { get; set; }

        [Required(ErrorMessage = " Required")]
        public Programme Programme { get; set; }

        [Required(ErrorMessage = " Required")]
        [Range(2017, 9999, ErrorMessage = "Should be 2017-9999")]
        public short Batch { get; set; }
    }

    public class PDFMeritListFilter
    {
        [DisplayName("Programme")]
        [Required(ErrorMessage = " Required")]
        public PrintProgramme PrintProgramme { get; set; }

        [DisplayName("Batch")]
        [Required(ErrorMessage = " Required")]
        [Range(2016, 9999, ErrorMessage = "Should be >= 2016")]
        public short Batch { get; set; }

        [DisplayName("Course")]
        [Required(ErrorMessage = " Required")]
        public Guid Course_ID { get; set; }

        [DisplayName("List Type")]
        [Required(ErrorMessage = " Required")]
        public EntrancePDFListType PDFListType { get; set; }

        [DisplayName("Note at bottom of PDF")]
        public string MSGOnBottomOfPDF { get; set; }
    }

    public class MeritListsAsPDF
    {
        public List<string> CourseFullNames { get; set; }
        public EntrancePDFListType PDFListType { get; set; }
        public int Batch { get; set; }
        public List<MeritLists> MeritListsDetails { get; set; }
        public string MSGOnBottomOfPDF { get; set; }
        public PrintProgramme PrintProgramme { get; set; }
    }

    public class MeritLists
    {
        public long? EntranceRollNo { get; set; }
        public decimal SubjectEntrancePoints { get; set; }
        public decimal? CATEntrancePoints { get; set; }
        public decimal TotalPoints { get; set; }
        public string MarksObtained12th { get; set; }
        public string PointsFrom12th { get; set; }
        public string StudentCategory { get; set; }
        public string FullName { get; set; }
        public string FathersName { get; set; }
    }

    public class RelocateCenter
    {
        [DisplayName("From Center")]
        [Required(ErrorMessage = " Required")]
        public Guid FromCenter_ID { set; get; }

        [DisplayName("Select Course")]
        [Required(ErrorMessage = " Required")]
        public Guid FromCourse_ID { set; get; }

        [DisplayName("To Center")]
        [Required(ErrorMessage = " Required")]
        public Guid ToCenter_ID { set; get; }
    }

    public class ArchiveCenter
    {
        [DisplayName("Select Course")]
        [Required(ErrorMessage = " Required")]
        public Guid Course_ID { set; get; }

        [DisplayName("Semester")]
        [Required(ErrorMessage = " Required")]
        public short Semester { set; get; }
    }
}
