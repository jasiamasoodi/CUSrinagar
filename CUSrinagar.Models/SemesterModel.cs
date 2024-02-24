using CUSrinagar.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CUSrinagar.Models
{
    #region MasterResult
    public class ResultErrorLog
    {
        public Guid ResultErrorLog_ID { get; set; }
        public PrintProgramme PrintProgramme { get; set; }
        public Guid _ID { get; set; }
        public short Semester { get; set; }
        public string Message { get; set; }
        public DateTime Dated { get; set; }
    }
    public class MasterResultRow : MasterResultSubject
    {
        public Guid Student_ID { get; set; }
        [IgnoreDBWriter]
        public Guid Combination_ID { get; set; }
        [IgnoreDBWriter]
        public string CUSRegistrationNo { get; set; }
        [IgnoreDBWriter]
        public string ClassRollNo { get; set; }
        [IgnoreDBWriter]
        public string FullName { get; set; }
        [IgnoreDBWriter]
        public string FathersName { get; set; }
        [IgnoreDBWriter]
        public string ExamRollNumber { get; set; }
        [IgnoreDBWriter]
        public Guid Course_ID { get; set; }
        [IgnoreDBWriter]
        public string CourseFullName { get; set; }
        [IgnoreDBWriter]
        public int Batch { get; set; }

        public short Semester { get; set; }
        [IgnoreDBWriter]
        public PrintProgramme PrintProgramme { get; set; }

        //below properties just using for where clause filters --
        [IgnoreDBWriter]
        public Guid AcceptCollege_ID { get; set; }
        [IgnoreDBWriter]
        public string CollegeFullName { get; set; }
        [IgnoreDBRead]
        public string CombinationSubjects { get; set; }
    }
    public class MasterResult
    {
        public Guid Student_ID { get; set; }
        public Guid Combination_ID { get; set; }
        public string CUSRegistrationNo { get; set; }
        public string ClassRollNo { get; set; }
        public string FullName { get; set; }
        public string FathersName { get; set; }
        public string ExamRollNumber { get; set; }
        public Guid Course_ID { get; set; }
        public string CourseFullName { get; set; }
        public int Batch { get; set; }
        public short Semester { get; set; }
        public PrintProgramme PrintProgramme { get; set; }
        public List<MasterResultSubject> MasterResultSubject { get; set; }
        //below properties just using for where clause filters --        
        public Guid AcceptCollege_ID { get; set; }
        public string CollegeFullName { get; set; }
        public string CombinationSubjects { get; set; }
    }
    public class MasterResultSubject : BaseWorkFlow
    {
        [IgnoreDBWriter]
        public Programme Programme { get; set; }
        public Guid _ID { get; set; }
        public Guid Subject_ID { get; set; }
        [IgnoreDBWriter]
        public string SubjectFullName { get; set; }
        [IgnoreDBWriter]
        public SubjectType SubjectType { get; set; }
        public short? ExternalMaxMarks { get; set; }
        public short? ExternalMinPassMarks { get; set; }
        public short? ExternalMarks { get; set; }
        public ResultStatus? ExternalStatus { get; set; }

        public short? InternalMaxMarks { get; set; }
        public short? InternalMinPassMarks { get; set; }
        public short? InternalMarks { get; set; }
        public ResultStatus? InternalStatus { get; set; }


        [IgnoreDBRead]
        public short TotalMaxMarks
        {
            get
            {
                short _totalMarks = 0;
                if (InternalStatus != ResultStatus.NA && InternalMaxMarks.HasValue)
                    _totalMarks = InternalMaxMarks.Value;
                if (ExternalStatus != ResultStatus.NA && ExternalMaxMarks.HasValue)
                    _totalMarks += ExternalMaxMarks.Value;
                return _totalMarks;
            }
        }
        [IgnoreDBRead]
        public short TotalMarksObtained
        {
            get
            {
                short _totalMarksObtained = 0;
                if (InternalStatus != ResultStatus.NA && InternalMarks.HasValue)
                    _totalMarksObtained = InternalMarks.Value;
                if (ExternalStatus != ResultStatus.NA && ExternalMarks.HasValue && InternalStatus == ResultStatus.P)
                    _totalMarksObtained += ExternalMarks.Value;
                return _totalMarksObtained;
            }
        }
        [IgnoreDBRead]
        public ResultStatus ResultStatus
        {
            get
            {
                var _OverallResultStatus = ResultStatus.F;
                if (InternalStatus != ResultStatus.NotApplicable && ExternalStatus != ResultStatus.NotApplicable)
                {
                    if (InternalStatus == ResultStatus.P && ExternalStatus == ResultStatus.P)
                        _OverallResultStatus = ResultStatus.P;
                }
                else if (InternalStatus != ResultStatus.NotApplicable)
                {
                    if (InternalStatus == ResultStatus.P)
                        _OverallResultStatus = ResultStatus.P;
                }
                else if (ExternalStatus != ResultStatus.NotApplicable)
                {
                    if (ExternalStatus == ResultStatus.P)
                        _OverallResultStatus = ResultStatus.P;
                }
                return _OverallResultStatus;
            }
        }

        public Guid ResultNotification_ID { get; set; }
        public Guid ExamForm_ID { get; set; }
        public string Remark { get; set; }

        [IgnoreDBWriter]
        public string NotificationNo { get; set; }
        [IgnoreDBWriter]
        public DateTime NotificationDate { get; set; }
        [IgnoreDBWriter]
        public string Title { get; set; }
    }
    #endregion

    public class SemesterModel
    {
        public Guid _ID { get; set; }
        [IgnoreDBWriter]
        public string CUSRegistrationNo { get; set; }
        [IgnoreDBWriter]
        public string ExamRollNumber { get; set; }
        [IgnoreDBWriter]
        public int Year { get; set; }
        public Guid Subject_ID { get; set; }
        [IgnoreDBWriter]
        public string SubjectFullName { get; set; }
        public decimal? ExternalMarks { get; set; }
        public decimal? ExternalAttendance_AssessmentMarks { get; set; }
        public decimal? InternalMarks { get; set; }
        public decimal? InternalAttendance_AssessmentMarks { get; set; }
        //[IgnoreDBWriter]
        //public short? ResultStatus { get; set; }
        public bool ExternalSubmitted { get; set; }
        public bool InternalSubmitted { get; set; }
        //[IgnoreDBWriter]
        //public int Session { get; set; }
        public Guid? ResultNotification_ID { get; set; }
        public Guid? ExamForm_ID { get; set; }
        public Guid Student_ID { get; set; }
        public DateTime InternalUpdatedOn { get; set; }
        public Guid? InternalUpdatedBy { get; set; }
        public DateTime? ExternalUpdatedOn { get; set; }
        public Guid? ExternalUpdatedBy { get; set; }
        [IgnoreDBWriter]
        public SubjectType SubjectType { get; set; }
        [IgnoreDBWriter]
        public bool IsBacklog { get; set; }
    }



    public class SubjectResult
    {
        public short SemesterBatch { get; set; }
        public short Semester { get; set; }
        public Guid _ID { get; set; }
        public virtual Guid Student_ID { get; set; }
        public Guid Subject_ID { get; set; }
        public Programme Programme { get; set; }
        public string SubjectFullName { get; set; }
        public SubjectType SubjectType { get; set; }

        [IgnoreDBRead]
        [IgnoreDBWriter]
        public short SortOrder
        {
            get
            {
                switch (SubjectType)
                {
                    case SubjectType.Core:
                    case SubjectType.Major:
                        return 1;
                    case SubjectType.DSE:
                    case SubjectType.MID:
                        return 2;
                    case SubjectType.DCE:
                    case SubjectType.MDisc:
                        return 3;
                    case SubjectType.GE:
                    case SubjectType.VAC:
                        return 4;
                    case SubjectType.OE:
                    case SubjectType.AE:
                    case SubjectType.BSC:
                        return 5;
                    case SubjectType.MIL:
                    case SubjectType.ESC:
                    case SubjectType.MVoc:
                        return 6;
                    case SubjectType.HSMC:
                    case SubjectType.CourseType_01:
                        return 7;
                    case SubjectType.CourseType_02:
                        return 8;
                    case SubjectType.CourseType_03:
                        return 9;
                    case SubjectType.OC:
                        return 10;
                    case SubjectType.Lab:
                        return 11;
                    case SubjectType.Practical:
                        return 12;
                    case SubjectType.Workshop:
                        return 13;
                    case SubjectType.Internship:
                        return 14;
                    case SubjectType.Research:
                        return 15;
                    case SubjectType.Seminar:
                        return 16;
                    case SubjectType.FirstSemesterExclusion:
                        return 17;
                    case SubjectType.SEC:
                        return 18;
                    default:
                        return 100;
                }
            }
        }

        public decimal TotalCredit { get; set; }

        public bool IsInternalMarksApplicable { get; set; }
        public decimal InternalMaxMarks { get; set; }
        public decimal InternalMinPassMarks { get; set; }
        public MarksIsPartOf InternalIsPartOf { get; set; }
        public decimal? InternalMarks { get; set; }

        public bool IsInternalAttendance_AssessmentMarksApplicable { get; set; }
        public decimal InternalAttendance_AssessmentMaxMarks { get; set; }
        public decimal InternalAttendance_AssessmentMinPassMarks { get; set; }
        public MarksIsPartOf InternalAttendance_AssessmentIsPartOf { get; set; }
        public decimal? InternalAttendance_AssessmentMarks { get; set; }

        public bool IsExternalAttendance_AssessmentMarksApplicable { get; set; }
        public decimal ExternalAttendance_AssessmentMaxMarks { get; set; }
        public decimal ExternalAttendance_AssessmentMinPassMarks { get; set; }
        public MarksIsPartOf ExternalAttendance_AssessmentIsPartOf { get; set; }
        public decimal? ExternalAttendance_AssessmentMarks { get; set; }


        public bool IsExternalMarksApplicable { get; set; }
        public decimal ExternalMaxMarks { get; set; }
        public decimal ExternalMinPassMarks { get; set; }
        public MarksIsPartOf ExternalIsPartOf { get; set; }
        public decimal? ExternalMarks { get; set; }


        public bool InternalSubmitted { get; set; }
        public bool ExternalSubmitted { get; set; }

        public string SubjectTitle { get; set; }

        #region DerivedProperties
        [IgnoreDBRead]
        public bool HasInternalComponent
        {
            get
            {
                if ((IsExternalMarksApplicable == true && ExternalIsPartOf == MarksIsPartOf.Internal) || (IsExternalAttendance_AssessmentMarksApplicable == true && ExternalAttendance_AssessmentIsPartOf == MarksIsPartOf.Internal)
                 || (IsInternalMarksApplicable == true && InternalIsPartOf == MarksIsPartOf.Internal) || (IsInternalAttendance_AssessmentMarksApplicable == true && InternalAttendance_AssessmentIsPartOf == MarksIsPartOf.Internal))
                    return true;
                return false;
            }
        }
        [IgnoreDBRead]
        public decimal? TotalInternalMaxMarks
        {
            get
            {
                decimal _total = decimal.Zero;
                if (IsExternalMarksApplicable == true && ExternalIsPartOf == MarksIsPartOf.Internal && ExternalMaxMarks > 0)
                    _total += ExternalMaxMarks;
                if (IsExternalAttendance_AssessmentMarksApplicable == true && ExternalAttendance_AssessmentIsPartOf == MarksIsPartOf.Internal && ExternalAttendance_AssessmentMaxMarks > 0)
                    _total += ExternalAttendance_AssessmentMaxMarks;
                if (IsInternalMarksApplicable == true && InternalIsPartOf == MarksIsPartOf.Internal && InternalMaxMarks > 0)
                    _total += InternalMaxMarks;
                if (IsInternalAttendance_AssessmentMarksApplicable == true && InternalAttendance_AssessmentIsPartOf == MarksIsPartOf.Internal && InternalAttendance_AssessmentMaxMarks > 0)
                    _total += InternalAttendance_AssessmentMaxMarks;
                if (_total == decimal.Zero) return null;
                return _total;

            }
        }
        [IgnoreDBRead]
        public decimal? TotalInternalMinPassMarks
        {
            get
            {
                decimal _total = decimal.Zero;
                if (IsExternalMarksApplicable == true && ExternalIsPartOf == MarksIsPartOf.Internal && ExternalMinPassMarks > 0)
                    _total += ExternalMinPassMarks;
                if (IsExternalAttendance_AssessmentMarksApplicable == true && ExternalAttendance_AssessmentIsPartOf == MarksIsPartOf.Internal && ExternalAttendance_AssessmentMinPassMarks > 0)
                    _total += ExternalAttendance_AssessmentMinPassMarks;
                if (IsInternalMarksApplicable == true && InternalIsPartOf == MarksIsPartOf.Internal && InternalMinPassMarks > 0)
                    _total += InternalMinPassMarks;
                if (IsInternalAttendance_AssessmentMarksApplicable == true && InternalAttendance_AssessmentIsPartOf == MarksIsPartOf.Internal && InternalAttendance_AssessmentMinPassMarks > 0)
                    _total += InternalAttendance_AssessmentMinPassMarks;
                if (_total == decimal.Zero) return null;
                return _total;

            }
        }
        [IgnoreDBRead]
        public decimal? TotalInternalMarksObtained
        {
            get
            {
                decimal? _total = null;
                if (IsExternalMarksApplicable == true && ExternalIsPartOf == MarksIsPartOf.Internal && ExternalMarks.HasValue && ExternalMarks.Value >= 0)
                    _total = (_total ?? 0) + ExternalMarks.Value;

                if (IsExternalAttendance_AssessmentMarksApplicable == true && ExternalAttendance_AssessmentIsPartOf == MarksIsPartOf.Internal
                    && ExternalAttendance_AssessmentMarks.HasValue && ExternalAttendance_AssessmentMarks.Value > 0)
                    _total = (_total ?? 0) + ExternalAttendance_AssessmentMarks.Value;

                if (IsInternalMarksApplicable == true && InternalIsPartOf == MarksIsPartOf.Internal && InternalMarks.HasValue && InternalMarks.Value >= 0)
                    _total = (_total ?? 0) + InternalMarks.Value;

                if (IsInternalAttendance_AssessmentMarksApplicable == true && InternalAttendance_AssessmentIsPartOf == MarksIsPartOf.Internal
                    && InternalAttendance_AssessmentMarks.HasValue && InternalAttendance_AssessmentMarks.Value >= 0)
                    _total = (_total ?? 0) + InternalAttendance_AssessmentMarks.Value;
                return _total;

            }
        }

        [IgnoreDBRead]
        public ResultStatus? InternalStatus
        {
            get
            {
                if (HasInternalComponent)
                {
                    bool _isAbsent = true;
                    bool _isNotAvailable = true;

                    if (IsExternalMarksApplicable == true && ExternalIsPartOf == MarksIsPartOf.Internal && ExternalMarks.HasValue)
                    {
                        if (_isAbsent && ExternalMarks.Value == -2)
                        {
                            _isAbsent = true;
                            _isNotAvailable = false;
                        }
                        else if (_isNotAvailable && ExternalMarks.Value == -1)
                        {
                            _isNotAvailable = true;
                            _isAbsent = false;
                        }
                        else
                        {
                            _isAbsent = _isNotAvailable = false;
                        }
                    }
                    if (IsExternalAttendance_AssessmentMarksApplicable == true && ExternalAttendance_AssessmentIsPartOf == MarksIsPartOf.Internal && ExternalAttendance_AssessmentMarks.HasValue)
                    {
                        if (_isAbsent && ExternalAttendance_AssessmentMarks.Value == -2)
                        {
                            _isAbsent = true;
                            _isNotAvailable = false;
                        }
                        else if (_isNotAvailable && ExternalAttendance_AssessmentMarks.Value == -1)
                        {
                            _isNotAvailable = true;
                            _isAbsent = false;
                        }
                        else
                        {
                            _isAbsent = _isNotAvailable = false;
                        }
                    }

                    if (IsInternalMarksApplicable == true && InternalIsPartOf == MarksIsPartOf.Internal && InternalMarks.HasValue)
                    {
                        if (_isAbsent && InternalMarks.Value == -2)
                        {
                            _isAbsent = true;
                            _isNotAvailable = false;
                        }
                        else if (_isNotAvailable && InternalMarks.Value == -1)
                        {
                            _isNotAvailable = true;
                            _isAbsent = false;
                        }
                        else
                        {
                            _isAbsent = _isNotAvailable = false;
                        }
                    }

                    if (IsInternalAttendance_AssessmentMarksApplicable == true && InternalAttendance_AssessmentIsPartOf == MarksIsPartOf.Internal && InternalAttendance_AssessmentMarks.HasValue)
                    {
                        if (_isAbsent && InternalAttendance_AssessmentMarks.Value == -2)
                        {
                            _isAbsent = true;
                            _isNotAvailable = false;
                        }
                        else if (_isNotAvailable && InternalAttendance_AssessmentMarks.Value == -1)
                        {
                            _isNotAvailable = true;
                            _isAbsent = false;
                        }
                        else
                        {
                            _isAbsent = _isNotAvailable = false;
                        }
                    }
                    if (_isAbsent && _isNotAvailable) return ResultStatus.NA;

                    if (_isAbsent) return ResultStatus.Absent;

                    if (_isNotAvailable) return ResultStatus.NA;






                    if (!TotalInternalMarksObtained.HasValue)
                        return ResultStatus.NA;

                    if (TotalInternalMarksObtained.Value > TotalInternalMaxMarks)
                        return ResultStatus.Discrepancy;

                    if (TotalInternalMarksObtained.Value < TotalInternalMinPassMarks)
                        return ResultStatus.F;

                    if (TotalInternalMarksObtained.Value >= TotalInternalMinPassMarks && TotalInternalMarksObtained.Value <= TotalInternalMaxMarks)
                        return ResultStatus.P;

                    return ResultStatus.NA;
                }
                return ResultStatus.NotApplicable;
            }
        }

        public bool HasExternalComponent
        {
            get
            {
                if ((IsExternalMarksApplicable == true && ExternalIsPartOf == MarksIsPartOf.External) || (IsExternalAttendance_AssessmentMarksApplicable == true && ExternalAttendance_AssessmentIsPartOf == MarksIsPartOf.External)
                 || (IsInternalMarksApplicable == true && InternalIsPartOf == MarksIsPartOf.External) || (IsInternalAttendance_AssessmentMarksApplicable == true && InternalAttendance_AssessmentIsPartOf == MarksIsPartOf.External))
                    return true;
                return false;
            }
        }
        [IgnoreDBRead]
        public decimal? TotalExternalMaxMarks
        {
            get
            {
                if (!HasExternalComponent) return null;
                decimal _total = decimal.Zero;
                if (IsExternalMarksApplicable == true && ExternalIsPartOf == MarksIsPartOf.External && ExternalMaxMarks > 0)
                    _total += ExternalMaxMarks;
                if (IsExternalAttendance_AssessmentMarksApplicable == true && ExternalAttendance_AssessmentIsPartOf == MarksIsPartOf.External && ExternalAttendance_AssessmentMaxMarks > 0)
                    _total += ExternalAttendance_AssessmentMaxMarks;
                if (IsInternalMarksApplicable == true && InternalIsPartOf == MarksIsPartOf.External && InternalMaxMarks > 0)
                    _total += InternalMaxMarks;
                if (IsInternalAttendance_AssessmentMarksApplicable == true && InternalAttendance_AssessmentIsPartOf == MarksIsPartOf.External && InternalAttendance_AssessmentMaxMarks > 0)
                    _total += InternalAttendance_AssessmentMaxMarks;
                if (_total == decimal.Zero) return null;
                return _total;

            }
        }
        [IgnoreDBRead]
        public decimal? TotalExternalMinPassMarks
        {
            get
            {
                if (!HasExternalComponent) return null;
                decimal? _total = null;
                if (IsExternalMarksApplicable == true && ExternalIsPartOf == MarksIsPartOf.External && ExternalMinPassMarks > 0)
                    _total = (_total ?? 0) + ExternalMinPassMarks;
                if (IsExternalAttendance_AssessmentMarksApplicable == true && ExternalAttendance_AssessmentIsPartOf == MarksIsPartOf.External && ExternalAttendance_AssessmentMinPassMarks > 0)
                    _total = (_total ?? 0) + ExternalAttendance_AssessmentMinPassMarks;
                if (IsInternalMarksApplicable == true && InternalIsPartOf == MarksIsPartOf.External && InternalMinPassMarks > 0)
                    _total += InternalMinPassMarks;
                if (IsInternalAttendance_AssessmentMarksApplicable == true && InternalAttendance_AssessmentIsPartOf == MarksIsPartOf.External && InternalAttendance_AssessmentMinPassMarks > 0)
                    _total = (_total ?? 0) + InternalAttendance_AssessmentMinPassMarks;

                return _total;

            }
        }
        [IgnoreDBRead]
        public decimal? TotalExternalMarksObtained
        {
            get
            {
                if (!HasExternalComponent) return null;
                if (!ResultNotification_ID.HasValue || !ExamForm_ID.HasValue) return null;
                decimal? _total = null;
                if (IsExternalMarksApplicable == true && ExternalIsPartOf == MarksIsPartOf.External && ExternalMarks.HasValue && ExternalMarks.Value >= 0)
                    _total = ExternalMarks.Value;

                if (IsExternalAttendance_AssessmentMarksApplicable == true && ExternalAttendance_AssessmentIsPartOf == MarksIsPartOf.External
                    && ExternalAttendance_AssessmentMarks.HasValue && ExternalAttendance_AssessmentMarks.Value >= 0)
                    _total += ExternalAttendance_AssessmentMarks.Value;

                if (IsInternalMarksApplicable == true && InternalIsPartOf == MarksIsPartOf.External && InternalMarks.HasValue && InternalMarks.Value >= 0)
                    _total += InternalMarks.Value;

                if (IsInternalAttendance_AssessmentMarksApplicable == true && InternalAttendance_AssessmentIsPartOf == MarksIsPartOf.External
                    && InternalAttendance_AssessmentMarks.HasValue && InternalAttendance_AssessmentMarks.Value >= 0)
                    _total += InternalAttendance_AssessmentMarks.Value;

                return _total;
            }
        }


        [IgnoreDBRead]
        public ResultStatus? ExternalStatus
        {
            get
            {
                if (!ResultNotification_ID.HasValue || !ExamForm_ID.HasValue) return ResultStatus.NA;
                bool _isAbsent = true;
                bool _isNotAvailable = true;

                if (IsExternalMarksApplicable == true && ExternalIsPartOf == MarksIsPartOf.External && ExternalMarks.HasValue)
                {
                    if (_isAbsent && ExternalMarks.Value == -2)
                    {
                        _isAbsent = true;
                        _isNotAvailable = false;
                    }
                    else if (_isNotAvailable && ExternalMarks.Value == -1)
                    {
                        _isNotAvailable = true;
                        _isAbsent = false;
                    }
                    else
                    {
                        _isAbsent = _isNotAvailable = false;
                    }
                }
                if (IsExternalAttendance_AssessmentMarksApplicable == true && ExternalAttendance_AssessmentIsPartOf == MarksIsPartOf.External && ExternalAttendance_AssessmentMarks.HasValue)
                {
                    if (_isAbsent && ExternalAttendance_AssessmentMarks.Value == -2)
                    {
                        _isAbsent = true;
                        _isNotAvailable = false;
                    }
                    else if (_isNotAvailable && ExternalAttendance_AssessmentMarks.Value == -1)
                    {
                        _isNotAvailable = true;
                        _isAbsent = false;
                    }
                    else
                    {
                        _isAbsent = _isNotAvailable = false;
                    }
                }

                if (IsInternalMarksApplicable == true && InternalIsPartOf == MarksIsPartOf.External && InternalMarks.HasValue)
                {
                    if (_isAbsent && InternalMarks.Value == -2)
                    {
                        _isAbsent = true;
                        _isNotAvailable = false;
                    }
                    else if (_isNotAvailable && InternalMarks.Value == -1)
                    {
                        _isNotAvailable = true;
                        _isAbsent = false;
                    }
                    else
                    {
                        _isAbsent = _isNotAvailable = false;
                    }
                }

                if (IsInternalAttendance_AssessmentMarksApplicable == true && InternalAttendance_AssessmentIsPartOf == MarksIsPartOf.External && InternalAttendance_AssessmentMarks.HasValue)
                {
                    if (_isAbsent && InternalAttendance_AssessmentMarks.Value == -2)
                    {
                        _isAbsent = true;
                        _isNotAvailable = false;
                    }
                    else if (_isNotAvailable && InternalAttendance_AssessmentMarks.Value == -1)
                    {
                        _isNotAvailable = true;
                        _isAbsent = false;
                    }
                    else
                    {
                        _isAbsent = _isNotAvailable = false;
                    }
                }

                if (ExternalMarks==null|| ExternalMarks.Value == -3)
                    return ResultStatus.LO;

                if (_isAbsent && _isNotAvailable) return ResultStatus.NA;

                if (_isAbsent) return ResultStatus.Absent;

                if (_isNotAvailable) return ResultStatus.NA;


                if (!HasExternalComponent) return null;

                if (HasInternalComponent && InternalStatus != ResultStatus.P && InternalStatus != ResultStatus.P)
                    return ResultStatus.NA;

                if (HasInternalComponent && InternalStatus != ResultStatus.P)
                    return ResultStatus.F;

                if (!TotalExternalMarksObtained.HasValue)
                    return ResultStatus.NA;

                if (TotalExternalMarksObtained.Value > TotalExternalMaxMarks)
                    return ResultStatus.Discrepancy;

                if (TotalExternalMarksObtained.Value < TotalExternalMinPassMarks)
                    return ResultStatus.F;

                if (TotalExternalMarksObtained.Value >= TotalExternalMinPassMarks && TotalExternalMarksObtained <= TotalExternalMaxMarks)
                    return ResultStatus.P;

                return ResultStatus.NA;
            }
        }
        [IgnoreDBRead]
        public decimal TotalMaxMarks
        {
            get
            {
                decimal _totalMarks = 0;
                if (HasInternalComponent && TotalInternalMaxMarks.HasValue)
                    _totalMarks = TotalInternalMaxMarks.Value;
                if (HasExternalComponent && TotalExternalMaxMarks.HasValue)
                    _totalMarks += TotalExternalMaxMarks.Value;
                return _totalMarks;
            }
        }
        [IgnoreDBRead]
        public decimal? TotalMarksObtained
        {
            get
            {
                if (HasInternalComponent && InternalStatus != ResultStatus.P)
                    return null;
                decimal? _total = null;
                if (HasInternalComponent && TotalInternalMarksObtained.HasValue && TotalInternalMarksObtained.Value > decimal.Zero && (InternalStatus == ResultStatus.P || InternalStatus == ResultStatus.F))
                    _total = (_total ?? 0) + TotalInternalMarksObtained.Value;
                if (HasExternalComponent && TotalExternalMarksObtained.HasValue && TotalExternalMarksObtained.Value > decimal.Zero && (ExternalStatus == ResultStatus.P || ExternalStatus == ResultStatus.F))
                    _total = _total.HasValue ? _total.Value + TotalExternalMarksObtained.Value : TotalExternalMarksObtained.Value;
                return _total;
            }
        }
        [IgnoreDBRead]
        public ResultStatus OverallResultStatus
        {
            get
            {
                if (HasExternalComponent && ExternalStatus == ResultStatus.P && HasInternalComponent && InternalStatus == ResultStatus.P)
                    return ResultStatus.P;

                if (!HasExternalComponent && HasInternalComponent && InternalStatus == ResultStatus.P)
                    return ResultStatus.P;

                if (!HasInternalComponent && HasExternalComponent && ExternalStatus == ResultStatus.P)
                    return ResultStatus.P;

                return ResultStatus.F;
            }
        }

        public bool ShowPublic
        {
            get
            {
                if (ResultNotification_ID.HasValue && ExamForm_ID.HasValue && HasResult) return true;
                return false;
            }
        }

        #endregion
        public Guid? ResultNotificationID { get; set; }//For Result-Revaluation Controller
        public Guid? ResultNotification_ID { get; set; }
        public Guid? ExamForm_ID { get; set; }
        public Guid? ParentNotification_ID { get; set; }
        public string NotificationNo { get; set; }
        public DateTime? NotificationDate { get; set; }
        public string SubjectCode { get; set; }

        [IgnoreDBRead]
        public RecordState RecordState { get; set; }
        public bool HasResult { get; set; }
        public bool HasExaminationFee { get; set; }


        public bool IsInternalPassed { get; set; }
        public bool IsExternalPassed { get; set; }
        //#region Transcript
        //public string T_SubjectTitle { get; set; }
        //public SubjectType? T_SubjectType { get; set; }
        //public short? T_Credit { get; set; }
        //public short? T_InternalMinMarks { get; set; }
        //public short? T_InternalMaxMarks { get; set; }
        //public short? T_InternalMarksObt { get; set; }
        //public ResultStatus? T_InternalResultStatus { get; set; }
        //public short? T_ExternalMinMarks { get; set; }
        //public short? T_ExternalMaxMarks { get; set; }
        //public short? T_ExternalMarksObt { get; set; }
        //public ResultStatus? T_ExternalResultStatus { get; set; }
        //public string T_GradeLetter { get; set; }
        //public short? T_GradePoints { get; set; }
        //public SGPAType? T_SGPAType { get; set; }
        //#endregion

        public Guid? ResultAnomalies_Id { get; set; }//For Showing Cancel Result for Students

        public bool IsCancelled { get; set; }

        public Guid? Marks_ID { get; set; }
    }



    public class ResultList : SubjectResult
    {
        //public Guid Combination_ID { get; set; }
        public string Category { get; set; }
        public string FathersName { get; set; }
        public short Batch { get; set; }
        //public short Semester { get; set; }
        public PrintProgramme PrintProgramme { get; set; }
        //public Programme Programme { get; set; }

        //public List<MSSGPA> SGPA { get; set; }
        //[IgnoreDBRead]
        //public List<MSCGPA> CGPA { get; set; }

        public bool TranscriptPrinted { get; set; }


        public override Guid Student_ID { get; set; }
        public string Gender { get; set; }
        public string CUSRegistrationNo { get; set; }
        public string ClassRollNo { get; set; }
        public string FullName { get; set; }
        public string ExamRollNumber { get; set; }
        public Guid AcceptCollege_ID { get; set; }
        public string CollegeFullName { get; set; }
        public Guid Course_ID { get; set; }
        public string CourseFullName { get; set; }
        //public short SemesterBatch { get; set; }
        public DateTime UpdatedON { get; set; }

        public string UserName { get; set; }

        public decimal CGPA { get; set; }

        // For ResultHistory Controller
        public DateTime InternalUpdatedOn { get; set; }
        public DateTime ExternalUpdatedOn { get; set; }
        public string InternalUserName { get; set; }
        public string ExternalUserName { get; set; }

        public DateTime? CreatedOn { get; set; }




    }

    public class MarksSheet
    {
        public Guid Student_ID { get; set; }
        public string CUSRegistrationNo { get; set; }
        public string FullName { get; set; }
        public string FathersName { get; set; }
        public string ExamRollNumber { get; set; }
        public Guid Course_ID { get; set; }
        public string CourseFullName { get; set; }
        public int Batch { get; set; }
        public string SerialNumber { get; set; }
        public DateTime DateOfDeclaration { get; set; }
        public DateTime DateOfIssue { get; set; }
        public bool ProvisionalIssued { get; set; }
        public bool OriginalIssued { get; set; }

        public Guid AcceptCollege_ID { get; set; }
        public string CollegeFullName { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? PrintedOn { get; set; }
        public short maxMarks { get; set; }
        public List<SubjectResult> SubjectResults { get; set; }
    }

    //public class MSFullDetails
    //{
    //    public Guid Marks_ID { get; set; }
    //    public Guid Subject_ID { get; set; }
    //    public string SubjectCode { get; set; }
    //    public string SubjectTitle { get; set; }
    //    public short SubjectType { get; set; }
    //    public short Semester { get; set; }
    //    public short Credit { get; set; }
    //    public short CreditWeightage { get; set; }
    //    public Guid Student_Id { get; set; }
    //    public short InternalMinMarks { get; set; }
    //    public short InternalMaxMarks { get; set; }
    //    public short InternalMarksObt { get; set; }
    //    public bool InternalResultStatus { get; set; }
    //    public short ExternalMinMarks { get; set; }
    //    public short ExternalMaxMarks { get; set; }
    //    public short ExternalMarksObt { get; set; }
    //    public bool ExternalResultStatus { get; set; }
    //    public bool IsLocked { get; set; }

    //}
    public class ResultCompact : IEquatable<ResultCompact>
    {

        public bool Equals(ResultCompact other) => this.Student_ID.Equals(other.Student_ID);
        public override int GetHashCode() => this.Student_ID.GetHashCode();
        public Guid Student_ID { get; set; }
        public string Gender { get; set; }
        public Guid? Combination_ID { get; set; }
        public string CUSRegistrationNo { get; set; }
        public string Category { get; set; }
        public string ClassRollNo { get; set; }
        public string FullName { get; set; }
        public string FathersName { get; set; }
        public string ExamRollNumber { get; set; }
        public Guid Course_ID { get; set; }
        public Guid Department_ID { get; set; }
        public string CourseFullName { get; set; }
        public string DegreeCourseTitle { get; set; }
        public short Batch { get; set; }
        public short SemesterBatch { get; set; }
        public short Semester { get; set; }
        public PrintProgramme PrintProgramme { get; set; }
        public Programme Programme { get; set; }
        [IgnoreDBRead]
        public List<SubjectResult> SubjectResults { get; set; }
        //below properties just using for where clause filters --       

        public Guid AcceptCollege_ID { get; set; }
        public string CollegeFullName { get; set; }
        //[IgnoreDBRead]
        //public string CombinationSubjects { get; set; }

        public bool HasTranscript { get; set; }
        public bool TranscriptPrinted { get; set; }


        [IgnoreDBRead]
        public Transcript Transcript { get; set; }

        public List<ADMSubjectMaster> SubjectList { get; set; }
        public List<ResultCompact> ResultDetailList { get; set; }
        public int NoOfStudents { get; set; }//For ResultBacklog Batches 

        public string NotificationNo { get; set; }//For Engineering Candidates
        public string Address { get; set; }//For Engineering Candidates College-Address
        public bool PrintedOn { get; set; }//For Engineering Candidates IsMarksSheet Printed

    }


    public class ResultSummary
    {
        public int TotalStudent { get; set; }
        public int TotalAppeared { get; set; }
        public int Other { get { return TotalStudent - TotalAppeared; } }
    }

    public class QuickResult
    {
        public string CUSRegistrationNo { get; set; }
        public int ExamRollNumber { get; set; }
        public short Semester { get; set; }
        public string JsonResult { get; set; }
    }

    public class SubjectResultDiscrepancy
    {
        public Guid SubjectResultDiscrepancy_ID { get; set; }
        public Guid Student_ID { get; set; }
        public Guid Subject_ID { get; set; }
        public DateTime Dated { get; set; }
        public string Remark { get; set; }

        [IgnoreDBWriter]
        public short Semester { get; set; }
        [IgnoreDBWriter]
        public string SubjectFullName { get; set; }
    }
    public class UploadResult
    {
        [DisplayName("Semester")]
        [Required(ErrorMessage = " Required")]
        [Range(typeof(short), "1", "16", ErrorMessage = " Invalid Semester")]
        public short Semester { get; set; }

        [DisplayName("Programme")]
        [Required(ErrorMessage = " Required")]
        public Programme Programme { get; set; }

        [DisplayName("Course")]
        [Required(ErrorMessage = " Required")]
        public Guid Course { get; set; }

        [DisplayName("Subject")]
        public Guid Subject { get; set; }
    }
    public class ExamResult
    {
        public long ExamRollNo { get; set; }
        public int ExternalMarks { get; set; }
        public Guid Subject_ID { get; set; }
        public short Semester { get; set; }
        public Programme Programme { get; set; }
    }
}
