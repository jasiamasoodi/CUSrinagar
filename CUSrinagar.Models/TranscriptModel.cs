using CUSrinagar.Enums;
using System;
using System.Collections.Generic;

namespace CUSrinagar.Models
{
    public class TranscriptModel
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
        public List<SubjectResult> SubjectResults { get; set; }
    }
    public class TranscriptSubjectModel
    {
        public short Semester { get; set; }
        #region FillModelProperties
        //public void UpdateModelProperties()
        //{
        //    bool InternalApplicable = false; decimal _InternalMaxMarks = 0; decimal _InternalMinPassMarks = 0; decimal? InternalMarksObtained = null;
        //    ResultStatus? _InternalResultStatus;

        //    bool ExternalApplicable = false; decimal _ExternalMaxMarks = 0; decimal _ExternalMinPassMarks = 0; decimal? ExternalMarksObtained = null;
        //    ResultStatus? _ExternalResultStatus;

        //    if (IsInternalMarksApplicable)
        //    {
        //        if (InternalIsPartOf == MarksIsPartOf.Internal)
        //        {
        //            InternalApplicable = true; _InternalMinPassMarks += InternalMinPassMarks; _InternalMaxMarks += InternalMaxMarks;
        //            if (decimal.TryParse(InternalMarks, out decimal _Marks) && _Marks >= 0)
        //            {
        //                InternalMarksObtained = InternalMarksObtained.HasValue ? InternalMarksObtained : 0;
        //                InternalMarksObtained += _Marks;
        //            }
        //        }
        //        else if (InternalIsPartOf == MarksIsPartOf.External)
        //        {
        //            ExternalApplicable = true; _ExternalMinPassMarks += _ExternalMinPassMarks; _ExternalMaxMarks += _ExternalMaxMarks;
        //            if (decimal.TryParse(InternalMarks, out decimal _Marks) && _Marks >= 0)
        //            {
        //                ExternalMarksObtained = ExternalMarksObtained.HasValue ? ExternalMarksObtained : 0;
        //                ExternalMarksObtained += _Marks;
        //            }
        //        }
        //    }

        //    if (IsInternalAttendance_AssessmentApplicable)
        //    {
        //        if (InternalAttendanceIsPartOf == MarksIsPartOf.Internal)
        //        {
        //            InternalApplicable = true; _InternalMinPassMarks += InternalAttendance_AssessmentMinPassMarks; _InternalMaxMarks += InternalAttendance_AssessmentMaxMarks;
        //            if (decimal.TryParse(InternalAttendance_AssessmentMarks, out decimal _Marks) && _Marks >= 0)
        //            {
        //                InternalMarksObtained = InternalMarksObtained.HasValue ? InternalMarksObtained : 0;
        //                InternalMarksObtained += _Marks;
        //            }
        //        }
        //        else if (InternalAttendanceIsPartOf == MarksIsPartOf.External)
        //        {
        //            ExternalApplicable = true; _ExternalMinPassMarks += InternalAttendance_AssessmentMinPassMarks; _ExternalMaxMarks += InternalAttendance_AssessmentMaxMarks;
        //            if (decimal.TryParse(InternalAttendance_AssessmentMarks, out decimal _Marks) && _Marks >= 0)
        //            {
        //                ExternalMarksObtained = ExternalMarksObtained.HasValue ? ExternalMarksObtained : 0;
        //                ExternalMarksObtained += _Marks;
        //            }
        //        }
        //    }

        //    if (IsExternalMarksApplicable)
        //    {
        //        if (ExternalIsPartOf == MarksIsPartOf.Internal)
        //        {
        //            InternalApplicable = true; _InternalMinPassMarks += ExternalMinPassMarks; _InternalMaxMarks += ExternalMaxMarks;
        //            if (decimal.TryParse(ExternalMarks, out decimal _Marks) && _Marks >= 0)
        //            {
        //                InternalMarksObtained = InternalMarksObtained.HasValue ? InternalMarksObtained : 0;
        //                InternalMarksObtained += _Marks;
        //            }
        //        }
        //        else if (ExternalIsPartOf == MarksIsPartOf.External)
        //        {
        //            ExternalApplicable = true; _ExternalMinPassMarks += _ExternalMinPassMarks; _ExternalMaxMarks += _ExternalMaxMarks;
        //            if (decimal.TryParse(ExternalMarks, out decimal _Marks) && _Marks >= 0)
        //            {
        //                ExternalMarksObtained = ExternalMarksObtained.HasValue ? ExternalMarksObtained : 0;
        //                ExternalMarksObtained += _Marks;
        //            }
        //        }
        //    }

        //    if (IsExternalAttendance_AssessmentApplicable)
        //    {
        //        if (ExternalAttendanceIsPartOf == MarksIsPartOf.Internal)
        //        {
        //            InternalApplicable = true; _InternalMinPassMarks += ExternalAttendance_AssessmentMinPassMarks; _InternalMaxMarks += ExternalAttendance_AssessmentMaxMarks;
        //            if (decimal.TryParse(ExternalAttendance_AssessmentMarks, out decimal _Marks) && _Marks >= 0)
        //            {
        //                InternalMarksObtained = InternalMarksObtained.HasValue ? InternalMarksObtained : 0;
        //                InternalMarksObtained += _Marks;
        //            }
        //        }
        //        else if (ExternalAttendanceIsPartOf == MarksIsPartOf.External)
        //        {
        //            ExternalApplicable = true; _ExternalMinPassMarks += ExternalAttendance_AssessmentMinPassMarks; _ExternalMaxMarks += ExternalAttendance_AssessmentMaxMarks;
        //            if (decimal.TryParse(ExternalAttendance_AssessmentMarks, out decimal _Marks) && _Marks >= 0)
        //            {
        //                ExternalMarksObtained = ExternalMarksObtained.HasValue ? ExternalMarksObtained : 0;
        //                ExternalMarksObtained += _Marks;
        //            }
        //        }
        //    }

        //    if (InternalApplicable)
        //    {
        //        if (InternalMarksObtained.HasValue)
        //        {
        //            if (InternalMarksObtained.Value <= _InternalMaxMarks)
        //            {
        //                if (InternalMarksObtained < _InternalMinPassMarks)
        //                    _InternalResultStatus = ResultStatus.Fail;
        //                else
        //                    _InternalResultStatus = ResultStatus.Pass;
        //            }
        //            else
        //                _InternalResultStatus = ResultStatus.Discrepancy;
        //        }
        //        else
        //            _InternalResultStatus = ResultStatus.Discrepancy;
        //    }
        //    else
        //    {
        //        IsInternalMarksApplicable = false;
        //        _InternalResultStatus = ResultStatus.NotApplicable;
        //    }
        //    if (ExternalApplicable)
        //    {
        //        if (ExternalMarksObtained.HasValue)
        //        {
        //            if (ExternalMarksObtained.Value <= _ExternalMaxMarks)
        //            {
        //                if (ExternalMarksObtained < _ExternalMinPassMarks)
        //                    _ExternalResultStatus = ResultStatus.Fail;
        //                else
        //                    _ExternalResultStatus = ResultStatus.Pass;
        //            }
        //            else
        //                _ExternalResultStatus = ResultStatus.Discrepancy;
        //        }
        //        else
        //            _ExternalResultStatus = ResultStatus.Discrepancy;
        //    }
        //    else
        //    {
        //        IsExternalMarksApplicable = false;
        //        _ExternalResultStatus = ResultStatus.NotApplicable;
        //    }

        //    OverallResultStatus = ResultStatus.Fail;
        //    if (InternalApplicable && ExternalApplicable)
        //    {
        //        if (_InternalResultStatus == ResultStatus.Pass && _ExternalResultStatus == ResultStatus.Pass)
        //            OverallResultStatus = ResultStatus.Pass;
        //    }
        //    else if (InternalApplicable)
        //    {
        //        if (_InternalResultStatus == ResultStatus.Pass)
        //            OverallResultStatus = ResultStatus.Pass;
        //    }
        //    else if (ExternalApplicable)
        //    {
        //        if (_ExternalResultStatus == ResultStatus.Pass)
        //            OverallResultStatus = ResultStatus.Pass;
        //    }

        //    TotalInternalMaxMarks = _InternalMaxMarks;
        //    TotalInternalMinPassMarks = _InternalMinPassMarks;
        //    TotalInternalMarksObtained = InternalMarksObtained;
        //    InternalStatus = _InternalResultStatus;

        //    TotalExternalMaxMarks = _ExternalMaxMarks;
        //    TotalExternalMinPassMarks = _ExternalMinPassMarks;
        //    TotalExternalMarksObtained = ExternalMarksObtained;
        //    ExternalStatus = _ExternalResultStatus;
        //}

        #endregion


        public Guid _ID { get; set; }
        public Guid Subject_ID { get; set; }
        public Programme Programme { get; set; }
        public string SubjectFullName { get; set; }
        public SubjectType SubjectType { get; set; }
        public bool IsExternalMarksApplicable { get; set; }
        public MarksIsPartOf ExternalIsPartOf { get; set; }
        public bool IsExternalPassComponent { get; set; }
        public int ExternalMaxMarks { get; set; }
        public int ExternalMinPassMarks { get; set; }
        public string ExternalMarks { get; set; }

        public bool IsExternalAttendance_AssessmentApplicable { get; set; }
        public MarksIsPartOf ExternalAttendanceIsPartOf { get; set; }
        public bool IsExternalAttendance_AssessmentPassComponent { get; set; }
        public int ExternalAttendance_AssessmentMaxMarks { get; set; }
        public int ExternalAttendance_AssessmentMinPassMarks { get; set; }
        public string ExternalAttendance_AssessmentMarks { get; set; }
        public bool ExternalSubmitted { get; set; }

        public bool IsInternalMarksApplicable { get; set; }
        public MarksIsPartOf InternalIsPartOf { get; set; }
        public bool IsInternalPassComponent { get; set; }
        public int InternalMaxMarks { get; set; }
        public int InternalMinPassMarks { get; set; }
        public string InternalMarks { get; set; }

        public bool IsInternalAttendance_AssessmentApplicable { get; set; }
        public MarksIsPartOf InternalAttendanceIsPartOf { get; set; }
        public bool IsInternalAttendance_AssessmentPassComponent { get; set; }
        public int InternalAttendance_AssessmentMaxMarks { get; set; }
        public int InternalAttendance_AssessmentMinPassMarks { get; set; }
        public string InternalAttendance_AssessmentMarks { get; set; }
        public bool InternalSubmitted { get; set; }


        #region DerivedProperties
        [IgnoreDBRead]
        public decimal? TotalInternalMaxMarks
        {
            get
            {
                decimal _InternalMaxMarks = 0;
                if (IsInternalMarksApplicable)
                    if (InternalIsPartOf == MarksIsPartOf.Internal)
                        _InternalMaxMarks += InternalMaxMarks;

                if (IsInternalAttendance_AssessmentApplicable)
                    if (InternalAttendanceIsPartOf == MarksIsPartOf.Internal)
                        _InternalMaxMarks += InternalAttendance_AssessmentMaxMarks;
                if (IsExternalMarksApplicable)
                    if (ExternalIsPartOf == MarksIsPartOf.Internal)
                        _InternalMaxMarks += ExternalMaxMarks;

                if (IsExternalAttendance_AssessmentApplicable)
                    if (ExternalAttendanceIsPartOf == MarksIsPartOf.Internal)
                        _InternalMaxMarks += ExternalAttendance_AssessmentMaxMarks;

                return _InternalMaxMarks;
            }
        }
        [IgnoreDBRead]
        public decimal? TotalInternalMinPassMarks
        {
            get
            {
                decimal _InternalMinPassMarks = 0;
                if (IsInternalMarksApplicable)
                    if (InternalIsPartOf == MarksIsPartOf.Internal)
                        _InternalMinPassMarks += InternalMinPassMarks;
                if (IsInternalAttendance_AssessmentApplicable)
                    if (InternalAttendanceIsPartOf == MarksIsPartOf.Internal)
                        _InternalMinPassMarks += InternalAttendance_AssessmentMinPassMarks;
                if (IsExternalMarksApplicable)
                    if (ExternalIsPartOf == MarksIsPartOf.Internal)
                        _InternalMinPassMarks += ExternalMinPassMarks;

                if (IsExternalAttendance_AssessmentApplicable)
                    if (ExternalAttendanceIsPartOf == MarksIsPartOf.Internal)
                        _InternalMinPassMarks += ExternalAttendance_AssessmentMinPassMarks;
                return _InternalMinPassMarks;
            }
        }
        [IgnoreDBRead]
        public decimal? TotalInternalMarksObtained
        {
            get
            {
                decimal? _InternalMarksObtained = null;
                if (IsInternalMarksApplicable)
                    if (InternalIsPartOf == MarksIsPartOf.Internal)
                        if (decimal.TryParse(InternalMarks, out decimal _Marks))
                        {
                            if (_Marks >= 0)
                            {
                                _InternalMarksObtained = _InternalMarksObtained.HasValue ? _InternalMarksObtained : 0;
                                _InternalMarksObtained += _Marks;
                            }
                            else if (_Marks == -2 || _Marks == -3)
                            {
                                _InternalMarksObtained = _InternalMarksObtained.HasValue ? _InternalMarksObtained : 0;
                                _InternalMarksObtained += 0;
                            }
                        }

                if (IsInternalAttendance_AssessmentApplicable)
                    if (InternalAttendanceIsPartOf == MarksIsPartOf.Internal)
                        if (decimal.TryParse(InternalAttendance_AssessmentMarks, out decimal _Marks) && _Marks >= 0)
                        {
                            _InternalMarksObtained = _InternalMarksObtained.HasValue ? _InternalMarksObtained : 0;
                            _InternalMarksObtained += _Marks;
                        }

                if (IsExternalMarksApplicable)
                    if (ExternalIsPartOf == MarksIsPartOf.Internal)
                        if (decimal.TryParse(ExternalMarks, out decimal _Marks) && _Marks >= 0)
                        {
                            _InternalMarksObtained = _InternalMarksObtained.HasValue ? _InternalMarksObtained : 0;
                            _InternalMarksObtained += _Marks;
                        }

                if (IsExternalAttendance_AssessmentApplicable)
                    if (ExternalAttendanceIsPartOf == MarksIsPartOf.Internal)
                        if (decimal.TryParse(ExternalAttendance_AssessmentMarks, out decimal _Marks) && _Marks >= 0)
                        {
                            if (_Marks >= 0)
                            {
                                _InternalMarksObtained = _InternalMarksObtained.HasValue ? _InternalMarksObtained : 0;
                                _InternalMarksObtained += _Marks;
                            }
                            else if (_Marks == -2 || _Marks == -3)
                            {
                                _InternalMarksObtained = _InternalMarksObtained.HasValue ? _InternalMarksObtained : 0;
                                _InternalMarksObtained += 0;
                            }
                        }
                if (_InternalMarksObtained > TotalInternalMaxMarks)
                    return null;
                return _InternalMarksObtained;
            }
        }
        [IgnoreDBRead]
        public ResultStatus InternalStatus
        {
            get
            {
                bool _InternalApplicable = false;
                bool _IsAbsent = false;
                ResultStatus _InternalResultStatus = ResultStatus.NA;

                if (IsInternalMarksApplicable)
                    if (InternalIsPartOf == MarksIsPartOf.Internal)
                    {
                        _InternalApplicable = true;
                        if (decimal.TryParse(InternalMarks, out decimal _Marks) && _Marks == -2)
                            _IsAbsent = true;
                    }

                if (IsInternalAttendance_AssessmentApplicable)
                    if (InternalAttendanceIsPartOf == MarksIsPartOf.Internal)
                        _InternalApplicable = true;

                if (IsExternalMarksApplicable)
                    if (ExternalIsPartOf == MarksIsPartOf.Internal)
                        _InternalApplicable = true;

                if (IsExternalAttendance_AssessmentApplicable)
                    if (ExternalAttendanceIsPartOf == MarksIsPartOf.Internal)
                    {
                        _InternalApplicable = true;
                        if (decimal.TryParse(ExternalAttendance_AssessmentMarks, out decimal _Marks) && _Marks == -2)
                            _IsAbsent = true;
                    }

                if (_InternalApplicable)
                {
                    if (TotalInternalMarksObtained.HasValue)
                    {
                        if (_IsAbsent && TotalInternalMarksObtained.Value <= 0)
                            _InternalResultStatus = ResultStatus.Absent;
                        else if (TotalInternalMarksObtained.Value <= TotalInternalMaxMarks)
                        {
                            if (TotalInternalMarksObtained.Value < TotalInternalMinPassMarks)
                                _InternalResultStatus = ResultStatus.F;
                            else
                                _InternalResultStatus = ResultStatus.P;
                        }
                        else
                            _InternalResultStatus = ResultStatus.Discrepancy;
                    }
                    else
                        _InternalResultStatus = ResultStatus.NA;
                }
                else
                    _InternalResultStatus = ResultStatus.NotApplicable;

                return _InternalResultStatus;
            }
        }

        [IgnoreDBRead]
        public decimal? TotalExternalMaxMarks
        {
            get
            {
                decimal _ExternalMaxMarks = 0;

                if (IsInternalMarksApplicable)
                    if (InternalIsPartOf == MarksIsPartOf.External)
                        _ExternalMaxMarks += ExternalMaxMarks;

                if (IsInternalAttendance_AssessmentApplicable)
                    if (InternalAttendanceIsPartOf == MarksIsPartOf.External)
                        _ExternalMaxMarks += InternalAttendance_AssessmentMaxMarks;

                if (IsExternalMarksApplicable)
                    if (ExternalIsPartOf == MarksIsPartOf.External)
                        _ExternalMaxMarks += ExternalMaxMarks;

                if (IsExternalAttendance_AssessmentApplicable)
                    if (ExternalAttendanceIsPartOf == MarksIsPartOf.External)
                        _ExternalMaxMarks += ExternalAttendance_AssessmentMaxMarks;

                return _ExternalMaxMarks;
            }
        }
        [IgnoreDBRead]
        public decimal? TotalExternalMinPassMarks
        {
            get
            {
                decimal _ExternalMinPassMarks = 0;
                if (IsInternalMarksApplicable)
                    if (InternalIsPartOf == MarksIsPartOf.External) _ExternalMinPassMarks += _ExternalMinPassMarks;

                if (IsInternalAttendance_AssessmentApplicable)
                    if (InternalAttendanceIsPartOf == MarksIsPartOf.External) _ExternalMinPassMarks += InternalAttendance_AssessmentMinPassMarks;

                if (IsExternalMarksApplicable)
                    if (ExternalIsPartOf == MarksIsPartOf.External) _ExternalMinPassMarks += ExternalMinPassMarks;


                if (IsExternalAttendance_AssessmentApplicable)
                    if (ExternalAttendanceIsPartOf == MarksIsPartOf.External) _ExternalMinPassMarks += ExternalAttendance_AssessmentMinPassMarks;

                return _ExternalMinPassMarks;
            }
        }
        [IgnoreDBRead]
        public decimal? TotalExternalMarksObtained
        {
            get
            {
                decimal? _ExternalMarksObtained = null;

                if (IsInternalMarksApplicable)
                    if (InternalIsPartOf == MarksIsPartOf.External)
                        if (decimal.TryParse(InternalMarks, out decimal _Marks) && _Marks >= 0)
                        {
                            _ExternalMarksObtained = _ExternalMarksObtained.HasValue ? _ExternalMarksObtained : 0;
                            _ExternalMarksObtained += _Marks;
                        }

                if (IsInternalAttendance_AssessmentApplicable)
                    if (InternalAttendanceIsPartOf == MarksIsPartOf.External)
                        if (decimal.TryParse(InternalAttendance_AssessmentMarks, out decimal _Marks) && _Marks >= 0)
                        {
                            _ExternalMarksObtained = _ExternalMarksObtained.HasValue ? _ExternalMarksObtained : 0;
                            _ExternalMarksObtained += _Marks;
                        }
                if (IsExternalMarksApplicable)
                    if (ExternalIsPartOf == MarksIsPartOf.External)
                        if (decimal.TryParse(ExternalMarks, out decimal _Marks))
                        {
                            if (_Marks >= 0)
                            {
                                _ExternalMarksObtained = _ExternalMarksObtained.HasValue ? _ExternalMarksObtained : 0;
                                _ExternalMarksObtained += _Marks;
                            }
                            else if (_Marks == -2 || _Marks == -3)
                            {
                                _ExternalMarksObtained = _ExternalMarksObtained.HasValue ? _ExternalMarksObtained : 0;
                                _ExternalMarksObtained += 0;
                            }
                        }


                if (IsExternalAttendance_AssessmentApplicable)
                    if (ExternalAttendanceIsPartOf == MarksIsPartOf.External)
                        if (decimal.TryParse(ExternalAttendance_AssessmentMarks, out decimal _Marks) && _Marks >= 0)
                        {
                            _ExternalMarksObtained = _ExternalMarksObtained.HasValue ? _ExternalMarksObtained : 0;
                            _ExternalMarksObtained += _Marks;
                        }
                //if (_ExternalMarksObtained > TotalExternalMaxMarks || (InternalStatus != ResultStatus.NotApplicable && InternalStatus != ResultStatus.P)) return null;
                if (_ExternalMarksObtained > TotalExternalMaxMarks || _ExternalMarksObtained == 0) return null;
                if (InternalStatus == ResultStatus.NotApplicable)
                {
                    return _ExternalMarksObtained;
                }
                else
                {
                    if (InternalStatus != ResultStatus.P)
                        return null;
                }

                return _ExternalMarksObtained;
            }
        }
        [IgnoreDBRead]
        public ResultStatus ExternalStatus
        {
            get
            {
                bool _ExternalApplicable = false; ResultStatus _ExternalResultStatus = ResultStatus.NA; bool _IsAbsent = false;
                if (IsInternalMarksApplicable)
                    if (InternalIsPartOf == MarksIsPartOf.External)
                        _ExternalApplicable = true;

                if (IsInternalAttendance_AssessmentApplicable)
                    if (InternalAttendanceIsPartOf == MarksIsPartOf.External)
                        _ExternalApplicable = true;

                if (IsExternalMarksApplicable)
                    if (ExternalIsPartOf == MarksIsPartOf.External)
                    {
                        _ExternalApplicable = true;
                        if (decimal.TryParse(ExternalMarks, out decimal _Marks) && _Marks == -2)
                            _IsAbsent = true;
                    }

                if (IsExternalAttendance_AssessmentApplicable)
                    if (ExternalAttendanceIsPartOf == MarksIsPartOf.External)
                        _ExternalApplicable = true;

                if (_ExternalApplicable)
                {
                    if (TotalExternalMarksObtained.HasValue)
                    {
                        if (_IsAbsent && TotalExternalMarksObtained.Value <= 0)
                            _ExternalResultStatus = ResultStatus.Absent;
                        else if (TotalExternalMarksObtained.Value <= TotalExternalMaxMarks)
                        {
                            if (TotalExternalMarksObtained.Value < TotalExternalMinPassMarks)
                            {
                                if (TotalExternalMarksObtained.Value == decimal.Zero)
                                    _ExternalResultStatus = ResultStatus.NA;
                                else
                                    _ExternalResultStatus = ResultStatus.F;
                            }
                            else
                                _ExternalResultStatus = ResultStatus.P;
                        }
                        else
                            _ExternalResultStatus = ResultStatus.Discrepancy;
                    }
                    else
                        _ExternalResultStatus = ResultStatus.NA;

                    if (InternalStatus != ResultStatus.NotApplicable)
                    {
                        //if (InternalStatus == ResultStatus.P)
                        //{

                        //}
                        //else
                        //{
                        //    _ExternalResultStatus = ResultStatus.F;
                        //}
                        if (InternalStatus == ResultStatus.P && _ExternalResultStatus == ResultStatus.F)
                            _ExternalResultStatus = ResultStatus.F;
                        else if (InternalStatus != ResultStatus.P && _ExternalResultStatus != ResultStatus.Absent && _ExternalResultStatus != ResultStatus.NA)
                            _ExternalResultStatus = ResultStatus.F;
                    }
                }
                else
                    _ExternalResultStatus = ResultStatus.NotApplicable;

                return _ExternalResultStatus;
            }
        }
        [IgnoreDBRead]
        public decimal TotalMaxMarks
        {
            get
            {
                decimal _totalMarks = 0;
                if (InternalStatus != ResultStatus.NotApplicable && TotalInternalMaxMarks.HasValue)
                    _totalMarks = TotalInternalMaxMarks.Value;
                if (ExternalStatus != ResultStatus.NotApplicable && TotalExternalMaxMarks.HasValue)
                    _totalMarks += TotalExternalMaxMarks.Value;
                return _totalMarks;
            }
        }
        [IgnoreDBRead]
        public decimal? TotalMarksObtained
        {
            get
            {
                decimal? _totalMarksObtained = null;
                if (TotalInternalMarksObtained.HasValue)
                    _totalMarksObtained = TotalInternalMarksObtained;
                if (TotalExternalMarksObtained.HasValue)
                {
                    _totalMarksObtained = _totalMarksObtained.HasValue ? _totalMarksObtained : 0;
                    _totalMarksObtained += TotalExternalMarksObtained;
                }
                return _totalMarksObtained;
            }
        }
        [IgnoreDBRead]
        public ResultStatus OverallResultStatus
        {
            get
            {
                var _OverallResultStatus = ExternalStatus;
                if ((InternalStatus != ResultStatus.NotApplicable && InternalStatus == ResultStatus.P) && (ExternalStatus != ResultStatus.NotApplicable && ExternalStatus == ResultStatus.P))
                    _OverallResultStatus = ResultStatus.P;
                else if ((InternalStatus == ResultStatus.NotApplicable) && (ExternalStatus != ResultStatus.NotApplicable && ExternalStatus == ResultStatus.P))
                    _OverallResultStatus = ResultStatus.P;
                else if ((InternalStatus != ResultStatus.NotApplicable && InternalStatus == ResultStatus.P) && (ExternalStatus == ResultStatus.NotApplicable))
                    _OverallResultStatus = ResultStatus.P;

                else if ((InternalStatus != ResultStatus.NotApplicable && InternalStatus == ResultStatus.F) && (ExternalStatus != ResultStatus.NotApplicable && ExternalStatus == ResultStatus.F))
                    _OverallResultStatus = ResultStatus.F;
                else if ((InternalStatus == ResultStatus.NotApplicable) && (ExternalStatus != ResultStatus.NotApplicable && ExternalStatus == ResultStatus.F))
                    _OverallResultStatus = ResultStatus.F;
                else if ((InternalStatus != ResultStatus.NotApplicable && InternalStatus == ResultStatus.F) && (ExternalStatus == ResultStatus.NotApplicable))
                    _OverallResultStatus = ResultStatus.F;

                return _OverallResultStatus;
            }
        }
        #endregion

        public Guid? ResultNotification_ID { get; set; }
        public Guid? ExamForm_ID { get; set; }
        /// <summary>
        /// Exam form number
        /// </summary>
        public string FormNumber { get; set; }
        /// <summary>
        /// Result notification number
        /// </summary>
        public string NotificationNo { get; set; }
        /// <summary>
        /// result notification date
        /// </summary>
        public DateTime NotificationDate { get; set; }
        public string Title { get; set; }
        public string SubjectCode { get; set; }

        public Guid? ParentNotification_ID { get; set; }
        //public bool ImportedToMasterTable { get; set; }
        public RecordState RecordState { get; set; }

        public bool HasResult { get; set; }
        public bool HasExaminationFee { get; set; }
    }


    public class TranscriptNotification : BaseWorkFlow
    {
        public Guid TranscriptGeneration_ID { get; set; }
        public short Batch { get; set; }
        public Guid Course_ID { get; set; }
        [IgnoreDBWriter]
        public string CourseFullName { get; set; }
        public DateTime GeneratedOn { get; set; }
        public string GeneratedBySection { get; set; }
        public string VerifiedBySection { get; set; }
        public string PrintedBySection { get; set; }
        public string Remark { get; set; }


    }
}
