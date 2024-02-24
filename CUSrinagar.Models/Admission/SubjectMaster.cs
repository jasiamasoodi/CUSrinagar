using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUSrinagar.Models
{
    public class MSSubjectMarksStructure : BaseWorkFlow
    {
        public Guid SubjectMarksStructure_ID { get; set; }
        public SGPAType SGPAType { get; set; }
        public decimal TotalCredit { get; set; }
        public decimal CreditWeightage { get; set; }

        public bool IsExternalMarksApplicable { get; set; }
        public short ExternalMinPassMarks { get; set; }
        public short ExternalMaxMarks { get; set; }
        public bool IsExternalPassComponent { get; set; }
        public string ExternalMarksLabel { get; set; }
        public AwardModuleType ExternalVisibleTo { get; set; }
        public MarksIsPartOf ExternalIsPartOf { get; set; }

        public bool IsExternalAttendance_AssessmentMarksApplicable { get; set; }
        public short ExternalAttendance_AssessmentMinPassMarks { get; set; }
        public short ExternalAttendance_AssessmentMaxMarks { get; set; }
        public bool IsExternalAttendance_AssessmentPassComponent { get; set; }
        public string ExternalAttendance_AssessmentMarksLabel { get; set; }
        public AwardModuleType ExternalAttendance_AssessmentVisibleTo { get; set; }
        public MarksIsPartOf ExternalAttendance_AssessmentIsPartOf { get; set; }

        public bool IsInternalMarksApplicable { get; set; }
        public short InternalMinPassMarks { get; set; }
        public short InternalMaxMarks { get; set; }
        public bool IsInternalPassComponent { get; set; }
        public string InternalMarksLabel { get; set; }
        public AwardModuleType InternalVisibleTo { get; set; }
        public MarksIsPartOf InternalIsPartOf { get; set; }

        public bool IsInternalAttendance_AssessmentMarksApplicable { get; set; }
        public short InternalAttendance_AssessmentMinPassMarks { get; set; }
        public short InternalAttendance_AssessmentMaxMarks { get; set; }
        public bool IsInternalAttendance_AssessmentPassComponent { get; set; }
        public string InternalAttendance_AssessmentMarksLabel { get; set; }
        public AwardModuleType InternalAttendance_AssessmentVisibleTo { get; set; }
        public MarksIsPartOf InternalAttendance_AssessmentIsPartOf { get; set; }
        public bool IsLocked { get; set; }
        public string Remarks { get; set; }

        #region DerivedProperties
        [IgnoreDBRead, IgnoreDBWriter]
        public bool HasInternalComponent
        {
            get {
                if ((IsExternalMarksApplicable && ExternalIsPartOf == MarksIsPartOf.Internal) || (IsExternalAttendance_AssessmentMarksApplicable && ExternalAttendance_AssessmentIsPartOf == MarksIsPartOf.Internal)
                 || (IsInternalMarksApplicable == true && InternalIsPartOf == MarksIsPartOf.Internal) || (IsInternalAttendance_AssessmentMarksApplicable && InternalAttendance_AssessmentIsPartOf == MarksIsPartOf.Internal))
                    return true;
                return false;
            }
        }
        [IgnoreDBRead, IgnoreDBWriter]
        public short? TotalInternalMaxMarks
        {
            get {
                short? _total = 0;
                if (HasInternalComponent)
                {
                    if (IsExternalMarksApplicable && ExternalIsPartOf == MarksIsPartOf.Internal && ExternalMaxMarks > 0)
                        _total += ExternalMaxMarks;
                    if (IsExternalAttendance_AssessmentMarksApplicable && ExternalAttendance_AssessmentIsPartOf == MarksIsPartOf.Internal && ExternalAttendance_AssessmentMaxMarks > 0)
                        _total += ExternalAttendance_AssessmentMaxMarks;
                    if (IsInternalMarksApplicable && InternalIsPartOf == MarksIsPartOf.Internal && InternalMaxMarks > 0)
                        _total += InternalMaxMarks;
                    if (IsInternalAttendance_AssessmentMarksApplicable && InternalAttendance_AssessmentIsPartOf == MarksIsPartOf.Internal && InternalAttendance_AssessmentMaxMarks > 0)
                        _total += InternalAttendance_AssessmentMaxMarks;
                }
                return _total <= 0 ? null : _total;

            }
        }
        [IgnoreDBRead, IgnoreDBWriter]
        public short? TotalInternalMinPassMarks
        {
            get {
                short? _total = 0;
                if (HasInternalComponent)
                {
                    if (IsExternalMarksApplicable && ExternalIsPartOf == MarksIsPartOf.Internal && ExternalMinPassMarks > 0)
                        _total += ExternalMinPassMarks;
                    if (IsExternalAttendance_AssessmentMarksApplicable && ExternalAttendance_AssessmentIsPartOf == MarksIsPartOf.Internal && ExternalAttendance_AssessmentMinPassMarks > 0)
                        _total += ExternalAttendance_AssessmentMinPassMarks;
                    if (IsInternalMarksApplicable && InternalIsPartOf == MarksIsPartOf.Internal && InternalMinPassMarks > 0)
                        _total += InternalMinPassMarks;
                    if (IsInternalAttendance_AssessmentMarksApplicable && InternalAttendance_AssessmentIsPartOf == MarksIsPartOf.Internal && InternalAttendance_AssessmentMinPassMarks > 0)
                        _total += InternalAttendance_AssessmentMinPassMarks;
                }
                if (_total == decimal.Zero) return null;
                return _total;

            }
        }
        [IgnoreDBRead, IgnoreDBWriter]
        public bool HasExternalComponent
        {
            get {
                if ((IsExternalMarksApplicable && ExternalIsPartOf == MarksIsPartOf.External) || (IsExternalAttendance_AssessmentMarksApplicable && ExternalAttendance_AssessmentIsPartOf == MarksIsPartOf.External)
                 || (IsInternalMarksApplicable == true && InternalIsPartOf == MarksIsPartOf.External) || (IsInternalAttendance_AssessmentMarksApplicable && InternalAttendance_AssessmentIsPartOf == MarksIsPartOf.External))
                    return true;
                return false;
            }
        }
        [IgnoreDBRead, IgnoreDBWriter]
        public short? TotalExternalMaxMarks
        {
            get {
                short? _total = 0;
                if (HasExternalComponent)
                {
                    if (IsExternalMarksApplicable && ExternalIsPartOf == MarksIsPartOf.External && ExternalMaxMarks > 0)
                        _total += ExternalMaxMarks;
                    if (IsExternalAttendance_AssessmentMarksApplicable && ExternalAttendance_AssessmentIsPartOf == MarksIsPartOf.External && ExternalAttendance_AssessmentMaxMarks > 0)
                        _total += ExternalAttendance_AssessmentMaxMarks;
                    if (IsInternalMarksApplicable && InternalIsPartOf == MarksIsPartOf.External && InternalMaxMarks > 0)
                        _total += InternalMaxMarks;
                    if (IsInternalAttendance_AssessmentMarksApplicable && InternalAttendance_AssessmentIsPartOf == MarksIsPartOf.External && InternalAttendance_AssessmentMaxMarks > 0)
                        _total += InternalAttendance_AssessmentMaxMarks;
                }
                return _total <= 0 ? null : _total;

            }
        }
        [IgnoreDBRead, IgnoreDBWriter]
        public short? TotalExternalMinPassMarks
        {
            get {
                short? _total = 0;
                if (HasExternalComponent)
                {
                    if (IsExternalMarksApplicable && ExternalIsPartOf == MarksIsPartOf.External && ExternalMinPassMarks > 0)
                        _total += ExternalMinPassMarks;
                    if (IsExternalAttendance_AssessmentMarksApplicable && ExternalAttendance_AssessmentIsPartOf == MarksIsPartOf.External && ExternalAttendance_AssessmentMinPassMarks > 0)
                        _total += ExternalAttendance_AssessmentMinPassMarks;
                    if (IsInternalMarksApplicable && InternalIsPartOf == MarksIsPartOf.External && InternalMinPassMarks > 0)
                        _total += InternalMinPassMarks;
                    if (IsInternalAttendance_AssessmentMarksApplicable && InternalAttendance_AssessmentIsPartOf == MarksIsPartOf.External && InternalAttendance_AssessmentMinPassMarks > 0)
                        _total += InternalAttendance_AssessmentMinPassMarks;
                }
                if (_total == decimal.Zero) return null;
                return _total;

            }
        }
        /// <summary>
        /// total internal marks plus total external marks
        /// </summary>
        [IgnoreDBRead, IgnoreDBWriter]
        public decimal? TotalMaxMarks
        {
            get {
                decimal _totalMarksByCreditWeitage = TotalCredit * CreditWeightage;
                int _totalMarksByFields = 0;
                if (HasInternalComponent && TotalInternalMaxMarks.HasValue)
                    _totalMarksByFields += TotalInternalMaxMarks.Value;
                if (HasExternalComponent && TotalExternalMaxMarks.HasValue)
                    _totalMarksByFields += TotalExternalMaxMarks.Value;
                if (_totalMarksByFields != _totalMarksByCreditWeitage) return null;
                return decimal.Parse(_totalMarksByCreditWeitage.ToString());
            }
        }
        #endregion
    }


    public class ADMSubjectMasterCompact
    {
        public Programme Programme { get; set; }
        public Guid Course_ID { get; set; }
        public string CourseFullName { get; set; }
        public Guid Subject_ID { get; set; }
        public string SubjectFullName { get; set; }
        public string SubjectCode { get; set; }
        public string DepartmentFullName { get; set; }
        public string CollegeCode { get; set; }
        public SubjectType SubjectType { get; set; }
        public short Semester { get; set; }
        public bool Status { get; set; }
        [IgnoreDBWriter]
        public short TotalCredit { get; set; }

    }
    public class ADMSubjectMaster : MSSubjectMarksStructure
    {
        public Guid Subject_ID { get; set; }
        [Required]
        public Guid? Course_ID { get; set; }

        [Required]
        public string SubjectCode { get; set; }

        [Required]
        public string SubjectFullName { get; set; }

        [IgnoreDBWriter]
        public string DepartmentFullName { get; set; }

        //[IgnoreDBWriter]
        //public string SchoolFullName { get; set; }
        [Required(ErrorMessage = "Department is required")]
        public Guid? Department_ID { get; set; }

        [Required(ErrorMessage = "Semester is required")]
        public Semester Semester { get; set; }

        [Required(ErrorMessage = "Subject number is required")]
        public int SubjectNumber { get; set; }

        [Display(Name = "Is Compulsory")]
        public bool IsCompulsory { get; set; }

        public bool Status { get; set; }

        [Required(ErrorMessage = "Subject type is required")]
        public SubjectType SubjectType { get; set; }


        public short? FromBatch { get; set; }
        public short? ToBatch { get; set; }

        public int? SubjectCapacity { get; set; }
        public bool SubjectCapacitySet { get; set; }
        public Programme Programme { get; set; }
        /// <summary>
        /// External Section Start
        /// </summary>
        //[Required(ErrorMessage = "Check theory marks is applicable or not")]

        public Guid? College_ID { get; set; }
        [IgnoreDBWriter]
        public string CollegeFullName { get; set; }
        [IgnoreDBWriter]
        public string CourseCode { get; set; }

        [IgnoreDBWriter]
        public string CourseFullName { get; set; }



        public bool HasResult { get; set; }
        public bool HasExaminationFee { get; set; }


        /// <summary>
        /// just using PrintProgramme to create filter
        /// </summary>
        [IgnoreDBWriter]
        public PrintProgramme PrintProgramme { get; set; }
        [IgnoreDBWriter, IgnoreDBRead]
        public Guid Combination_ID { get; set; }
        [IgnoreDBWriter]
        public Guid? Marks_ID { get; set; }
        
    }

    public class SubjectCompact
    {
        public Guid Subject_ID { get; set; }
        public string SubjectFullName { get; set; }
        public SubjectType SubjectType { get; set; }
        public string SubjectTypeDescription { get; set; }
        public Semester Semester { get; set; }
        public string DepartmentFullName { get; set; }
        public short TotalCredit { get; set; }
        public short? FromBatch { get; set; }
        public short? ToBatch { get; set; }
        public string CollegeCode { get; set; }
        public string CourseFullName { get; set; }
    }
    //public class ADMSubjectMasterold : BaseWorkFlow
    //{
    //    public Guid Subject_ID { get; set; }
    //    [Required(ErrorMessage = "Course required")]
    //    [Display(Name = "Course")]
    //    public Guid Course_ID { get; set; }

    //    [Required(ErrorMessage = "Subject Code required")]
    //    [Display(Name = "Subject Code")]
    //    public string SubjectCode { get; set; }

    //    [Required(ErrorMessage = "Subject Name required")]
    //    [Display(Name = "Subject Name")]
    //    public string SubjectFullName { get; set; }

    //    [Required(ErrorMessage = "Theory Min Pass Marks required")]
    //    [Display(Name = "Theory Min Pass Marks")]
    //    public short TheoryMinPassMarks { get; set; }

    //    [Required(ErrorMessage = "Theory Max Marks required")]
    //    [Display(Name = "Theory Max Marks")]
    //    public short TheoryMaxMarks { get; set; }

    //    [Display(Name = "Practical Min Pass Marks")]
    //    [Required(ErrorMessage = "Practical Min Marks required")]
    //    public int PracticalMinPassMarks { get; set; }


    //    [Display(Name = "Practical Max Marks")]
    //    [Required(ErrorMessage = "Practical Marks required")]
    //    public int PracticalMaxMarks { get; set; }


    //    [DisplayName("Theory Attendence / Theory Credit")]
    //    [Required(ErrorMessage = "Theory Attendance required")]
    //    public int TheoryAttendance { get; set; }

    //    [DisplayName("Practical Attendence / Practical Credit")]
    //    [Required(ErrorMessage = "Practical Attendence  required")]
    //    public int PracticalAttendence { get; set; }

    //    [Display(Name = "Is Compulsory")]
    //    public bool IsCompulsory { get; set; }

    //    [Display(Name = "Is Core")]
    //    public bool IsCore { get; set; }

    //    public bool Status { get; set; }

    //    public SubjectType SubjectType { get; set; }


    //    [IgnoreDBRead]
    //    [IgnoreDBWriter]
    //    [Display(Name = "Programme")]
    //    public Programme ProgrammeId { get; set; }

    //    [Display(Name = "College")]
    //    public Guid? College_ID { get; set; }

    //    [Display(Name = "College")]
    //    [IgnoreDBWriter]
    //    public string CollegeName { get; set; }


    //    [IgnoreDBWriter]
    //    public string CourseCode { get; set; }

    //    [IgnoreDBWriter]
    //    public string CoursefullName { get; set; }

    //    [IgnoreDBWriter]
    //    public string SemesterName { get; set; }

    //    [DisplayName("Department Name")]
    //    [Required(ErrorMessage = "Department Name required")]
    //    public string Department { get; set; }

    //    [DisplayName("Semester ")]
    //    [Required(ErrorMessage = "Semester  required")]
    //    public Semester Semester { get; set; }

    //    [DisplayName("Subject Number")]
    //    [Required(ErrorMessage = "Subject Number required")]
    //    public int SubjectNumber { get; set; }

    //    /// <summary>
    //    /// just using PrintProgramme to create filter
    //    /// </summary>
    //    [IgnoreDBRead, IgnoreDBWriter]
    //    public PrintProgramme PrintProgramme { get; set; }
    //    ///// <summary>
    //    ///// Represents how many subject to show for student while assigning subjects as per college
    //    ///// </summary>
    //    //public int? Credit { get; set; }

    //    public int SubjectCapacity { get; set; }
    //    public bool SubjectCapacitySet { get; set; }
    //}

    public class AwardForSubjects
    {
        public string SubjectFullName { get; set; }

        public int TotalNumberOfStudents { get; set; }

        public int ExternalSubmitted { get; set; }

        public int InternalSubmitted { get; set; }

        public int ExternalNotSubmitted { get; set; }

        public int InternalNotSubmitted { get; set; }
        public int InternalRecieved { get; set; }

        public int ExternalRecieved { get; set; }

    }


    #region DepartmentSection
    public class Department
    {
        public Guid Department_ID { get; set; }

        public string SchoolFullName { get; set; }

        public string DepartmentFullName { get; set; }
    }
    #endregion
}
