using CUSrinagar.DataManagers;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using CUSrinagar.Models;
using GeneralModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Transactions;
using System.Web.Mvc;
using System.Data;
using System.Web;
using System.Text.RegularExpressions;
using System.IO;
using System.Web.UI.WebControls;
using System.Data.Common;
using System.Activities.Statements;
using TransactionScope = System.Transactions.TransactionScope;

namespace CUSrinagar.BusinessManagers
{
    public class ResultManager
    {
        public List<ResultCompact> Get(PrintProgramme printProgramme, short? semester, Parameters parameters, bool IsEditable = false, bool transcriptInfo = false)
        {
            List<ResultCompact> list1 = null;
            try
            {
                list1 = new ResultDB().StudentInfo(printProgramme, semester, parameters, IsEditable);
            }
            catch (SqlException ex) when (ex.Number == 208) { /*Invalid object name*/ }

            if (list1.IsNotNullOrEmpty())
            {
                //foreach (var item in list1)
                //{
                //    if (!IsEditable)
                //    {
                //        var _Semesters = item.SubjectResults.Where(x => x.ResultNotification_ID.HasValue && x.ResultNotification_ID != Guid.Empty).Select(x => x.Semester).Distinct().ToList();
                //        item.SubjectResults = item.SubjectResults.Where(x => _Semesters.Any(y => y == x.Semester)).ToList();
                //    }
                //}

                if (transcriptInfo)
                {
                    foreach (var item in list1)
                    {
                        if (item.HasTranscript)
                        {
                            item.Transcript = new Transcript()
                            {
                                Subject = new TranscriptDB().GetSubject(item.Student_ID),
                                SGPA = new TranscriptDB().GetSGPA(item.Student_ID),
                                CGPA = new TranscriptDB().GetCGPA(item.Student_ID,6)
                            };
                        }
                    }
                }
            }


            //List<ResultList> listTesttable = new ResultDB().ResultList_SubjectResultTable(printProgramme, parameters, semester, IsEditable);
            //List<ResultCompact> listCompacttable = fillcompactList(listTesttable);
            //var newendtable = DateTime.Now;
            //var newtimetakentalbe = (newendtable - newstarttable).TotalSeconds;
            //#endregion

            //List<ResultCompact> list = new ResultDB().StudentInfo(printProgramme, semester, parameters, IsEditable);
            //if (list.IsNotNullOrEmpty())
            //{
            //    foreach (var item in list)
            //    {
            //        item.SubjectResults = new ResultDB().SubjectResult(printProgramme, item.Student_ID, (short)item.SemesterBatch, semester, IsEditable, transcriptInfo);
            //        if (item.SubjectResults.IsNotNullOrEmpty())
            //        {
            //            if (!IsEditable)
            //            {
            //                var _Semesters = item.SubjectResults.Where(x => x.ResultNotification_ID.HasValue && x.ResultNotification_ID != Guid.Empty).Select(x => x.Semester).Distinct().ToList();
            //                item.SubjectResults = item.SubjectResults.Where(x => _Semesters.Any(y => y == x.Semester)).ToList();
            //            }
            //            //if (transcriptInfo && item.SubjectResults.Any(x => !string.IsNullOrEmpty(x.T_GradeLetter)))
            //            //{
            //            //    item.SGPA = new TranscriptDB().GetSGPA(item.Student_ID);
            //            //    item.CGPA = new TranscriptDB().GetCGPA(item.Student_ID);
            //            //}
            //        }
            //    }
            //}
            return list1;
        }


        //List<ResultCompact> fillcompactList(List<ResultList> list)
        //{
        //    List<ResultCompact> listCompact = list.Select(x => new ResultCompact() { Student_ID = x.Student_ID, CUSRegistrationNo = x.CUSRegistrationNo, FullName = x.FullName }).ToList<ResultCompact>().Distinct().ToList();
        //    listCompact.ForEach(x => { x.SubjectResults = list.Where(y => y.Student_ID == x.Student_ID).Select(z => new SubjectResult() { Subject_ID = z.Subject_ID, SubjectFullName = z.SubjectFullName }).ToList(); });
        //    return listCompact;
        //}

        public List<ResultCompact> GetDetails(PrintProgramme printProgramme, short Semester, Parameters parameters, bool IsEditable = false, bool transcriptInfo = false)
        {
            List<ResultCompact> list1 = null;
            try
            {
                list1 = new ResultDB().StudentInfoDetails(printProgramme, Semester, parameters, IsEditable);
            }
            catch (SqlException ex) when (ex.Number == 208) { /*Invalid object name*/ }

            if (list1.IsNotNullOrEmpty())
            {

                if (transcriptInfo)
                {
                    foreach (var item in list1)
                    {
                        if (item.HasTranscript)
                        {
                            item.Transcript = new Transcript()
                            {
                                Subject = new TranscriptDB().GetSubject(item.Student_ID),
                                SGPA = new TranscriptDB().GetSGPA(item.Student_ID),
                                CGPA = new TranscriptDB().GetCGPA(item.Student_ID,Semester)
                            };
                        }
                    }
                }
            }

            return list1;
        }

        public List<ResultCompact> GetDetailsFullGazette(PrintProgramme printProgramme, short? semester, Parameters parameters, bool IsEditable = false, bool transcriptInfo = false)
        {
            List<ResultCompact> list1 = null;
            try
            {
                list1 = new ResultDB().StudentInfoDetailsFullGazette(printProgramme, semester, parameters, IsEditable);
            }
            catch (SqlException ex) when (ex.Number == 208) { /*Invalid object name*/ }

            if (list1.IsNotNullOrEmpty())
            {

                if (transcriptInfo)
                {
                    foreach (var item in list1)
                    {
                        if (item.HasTranscript)
                        {
                            item.Transcript = new Transcript()
                            {
                                Subject = new TranscriptDB().GetSubject(item.Student_ID),
                                SGPA = new TranscriptDB().GetSGPA(item.Student_ID),
                                CGPA = new TranscriptDB().GetCGPA(item.Student_ID,6)
                            };
                        }
                    }
                }
            }

            return list1;
        }

        public List<ResultCompact> ChkDataMSFullDetails(List<Guid> StudentIDs)
        {
            return new ResultDB().ChkDataMSFullDetails(StudentIDs);
        }

        public List<ResultCompact> ChkDatastudIDList()
        {
            return new ResultDB().ChkDatastudIDList();
        }




        public List<ResultCompact> GetFinalData(Parameters parameter, PrintProgramme printProgramme, short? semester, bool transcriptInfo = false)
        {
            List<ResultCompact> list1 = null;
            try
            {
                bool IsEditable = false;
                list1 = new ResultDB().GetFinalData(parameter, printProgramme, semester, IsEditable);
            }
            catch (SqlException ex) when (ex.Number == 208) { /*Invalid object name*/ }


            return list1;
        }






        public List<ResultCompact> GetDetailsForEngForInsertion(Parameters parameter, PrintProgramme printProgramme, short? semester, bool transcriptInfo = false)
        {
            List<ResultCompact> list1 = null;
            try
            {
                bool IsEditable = false;
                list1 = new ResultDB().StudentInfoDetailsForEngForInsertion(parameter, printProgramme, semester, IsEditable);
            }
            catch (SqlException ex) when (ex.Number == 208) { /*Invalid object name*/ }



            return list1;
        }


        public List<ResultCompact> FinalData(PrintProgramme PrintProgramme, Parameters parameters)
        {
            List<ResultCompact> MarksSheetList = new ResultDB().FinalData(PrintProgramme, parameters);

            List<SubjectResult> DetailsList = new List<SubjectResult>();

            if (MarksSheetList.IsNotNullOrEmpty())
            {
                List<Guid> StudentIDs = MarksSheetList.Select(x => x.Student_ID).Distinct().ToList();

                DetailsList = new ResultDB().AllDataByStudentIDs(StudentIDs, PrintProgramme);
                //if (subjectResultList.IsNotNullOrEmpty())
                //    list.ForEach(x => x.SubjectResults = subjectResultList.Where(y => y.Student_ID == x.Student_ID)?.ToList());


                //var subjectResultList = AllSubjectDetails(printProgramme, semester, parameter, IsEditable);

                //var subjectResultList = AllDataByStudentIDs(StudentIDs, PrintProgramme);

                if (DetailsList.IsNotNullOrEmpty())
                    MarksSheetList.ForEach(x => x.SubjectResults = DetailsList.Where(y => y.Student_ID == x.Student_ID)?.ToList());

            }
            return MarksSheetList;


        }
        public List<ResultCompact> GetDetailsForEng(PrintProgramme printProgramme, short? semester, Parameters parameters, bool IsEditable = false, bool transcriptInfo = false)
        {
            List<ResultCompact> list1 = null;
            try
            {
                list1 = new ResultDB().StudentInfoDetailsForEng(printProgramme, semester, parameters, IsEditable);
            }
            catch (SqlException ex) when (ex.Number == 208) { /*Invalid object name*/ }

            if (list1.IsNotNullOrEmpty())
            {

                if (transcriptInfo)
                {
                    foreach (var item in list1)
                    {
                        if (item.HasTranscript)
                        {
                            item.Transcript = new Transcript()
                            {
                                Subject = new TranscriptDB().GetSubject(item.Student_ID),
                                SGPA = new TranscriptDB().GetSGPA(item.Student_ID),
                                CGPA = new TranscriptDB().GetCGPA(item.Student_ID,6)
                            };
                        }
                    }
                }
            }

            return list1;
        }

        public List<MarksSheet> MarksSheetList(PrintProgramme PrintProgramme, Parameters parameters)
        {
            List<MarksSheet> marksSheets = new ResultDB().MarksSheetList(PrintProgramme, parameters);
            if (marksSheets.IsNotNullOrEmpty())
                foreach (var item in marksSheets)
                {
                    if (item.CourseFullName.ToLower().Contains("geography") && PrintProgramme == PrintProgramme.PG)
                    {
                        item.CourseFullName = new StudentManager().GetCoursePrefix(item.CourseFullName, PrintProgramme, item.Student_ID, "Graduation");
                    }
                    item.SubjectResults = new ResultDB().GetSubject(item.Student_ID);

                }
            return marksSheets;
        }

        public List<MarksSheet> MarksSheetStatus(PrintProgramme PrintProgramme, Parameters parameters)
        {
            List<MarksSheet> marksSheets = new ResultDB().MarksSheetStatus(PrintProgramme, parameters);
            if (marksSheets.IsNotNullOrEmpty())
                foreach (var item in marksSheets)
                {
                    item.SubjectResults = new ResultDB().GetSubject(item.Student_ID);
                }
            return marksSheets;
        }



        public Task<ResultCompact> GetResultAsync(string SearchText, ResultNotification resultNotification)
        {
            return new TaskFactory().StartNew(() => GetResult(SearchText, resultNotification));
        }

        public ResultCompact GetResult(string SearchText, ResultNotification resultNotification)
        {
            ResultCompact studentDetail = null;
            if (resultNotification != null)
            {
                var findedStudent_ID = new ResultDB().FindStudent(resultNotification, SearchText);
                if (findedStudent_ID.HasValue)
                {
                    studentDetail = GetResult(resultNotification.PrintProgramme, resultNotification.Semester, findedStudent_ID.Value);
                }
            }
            return studentDetail;
        }

        public ResultCompact GetResult(PrintProgramme printProgramme, short? semester, Guid Student_ID, bool IsEditable = false)
        {
            Parameters param = new Parameters() { Filters = new List<SearchFilter>() { new SearchFilter() { Column = "Student_ID", Value = Student_ID.ToString(), TableAlias = "S" } }, PageInfo = new Paging() { DefaultOrderByColumn = "CUSRegistrationNo", PageNumber = 1, PageSize = 1 } };
            if (semester.HasValue)
                param.Filters.Add(new SearchFilter() { Column = "Semester", Value = semester.ToString(), TableAlias = "Comb" });
            return Get(printProgramme, semester, param, IsEditable)?.FirstOrDefault();
            //ResultCompact item = new ResultDB().StudentInfo(printProgramme, semester, Student_ID, IsEditable);
            //if (item != null)
            //{
            //    item.SubjectResults = new ResultDB().SubjectResultList(printProgramme, item.Student_ID, semester, IsEditable);
            //    if (item.SubjectResults.IsNotNullOrEmpty() && !IsEditable)
            //    {
            //        var _Semesters = item.SubjectResults.Where(x => x.ResultNotification_ID.HasValue && x.ResultNotification_ID != Guid.Empty).Select(x => x.Semester).Distinct().OrderBy(x => x).ToList();
            //        item.SubjectResults = item.SubjectResults.Where(x => _Semesters.Any(y => y == x.Semester)).ToList();
            //    }
            //}
            //return item;
        }

        #region NewResultSection





        public List<SelectListItem> GetResultNotifications(int semester)
        {
            return new ResultNotificationDB().GetResultNotifications(semester);
        }
        //public ResultNotification GetResultNotification(Guid ResultNotification_Id)
        //{
        //    return new ResultDB().GetResultNotification(ResultNotification_Id);
        //}
        public ResultNotification GetResultNotification(string ResultNotification)
        {
            return new ResultDB().GetResultNotification(ResultNotification);
        }


        public List<ResultCompact> CheckBacklogBatches(PrintProgramme printProgramme, short? semester, Parameters parameters, int? batch, string programe)
        {
            return new ResultDB().CheckBacklogBatches(printProgramme, semester, parameters, batch, programe);
        }
        public int Save(ResultNotification resultNotification)
        {
            return new ResultDB().Save(resultNotification);
        }

        public int Save(List<Guid> StudentIDs)
        {
            return new ResultDB().Save(StudentIDs);
        }

        public int DeclareResult(ResultNotification resultNotification)
        {
            try
            {
                return new ResultNotificationDB().DeclareResult(resultNotification);
            }
            catch { return 0; }
        }


        //public int Delete(Guid ResultNotification_ID)
        //{
        //    return new ResultDB().Delete(ResultNotification_ID);

        //}

        //public MarksSheet GetMarksSheet(PrintProgramme printProgramme, short semester, Guid Student_ID)
        //{
        //    MarksSheet markssheet = new ResultDB().StudentInfoOnMarksSheet(printProgramme, semester, Student_ID);
        //    if (markssheet != null)
        //    {
        //        ADMCourseMaster courseMaster = new CourseManager().GetCourseById(markssheet.Course_ID);
        //        markssheet.SubjectResults = new List<SubjectResult>();
        //        //for (short sem = 1; sem <= courseMaster.Duration; sem++)
        //        {
        //            var _subjectresult = new ResultDB().SubjectResultList(printProgramme, markssheet.Student_ID,(short)markssheet.Batch);
        //            //_subjectresult = _subjectresult.IsNotNullOrEmpty() && _subjectresult.Any(x => x.ResultNotification_ID.HasValue && x.ResultNotification_ID != Guid.Empty) ? _subjectresult : null;
        //            if (_subjectresult.IsNotNullOrEmpty())
        //            {
        //                markssheet.SubjectResults.AddRange(_subjectresult);
        //            }
        //        }
        //    }
        //    return markssheet;
        //}

        //public List<ResultCompact> GetResultTest(PrintProgramme printProgramme, short semester, Parameters parameters)
        //{
        //    List<ResultCompact> list = new ResultDB().StudentInfoTest(printProgramme, semester, parameters);
        //    if (list.IsNotNullOrEmpty())
        //    {
        //        foreach (var item in list)
        //            item.SubjectResults = new ResultDB().SubjectResultListTempAverage(printProgramme, item.Student_ID);
        //    }
        //    return list;
        //}


        public List<ResultList> List(PrintProgramme printProgramme, Parameters parameter, short? semester, bool IsEditable = false, bool IsPassOut = false)
        {
            return new ResultDB().ResultList_SubjectResult(printProgramme, parameter, semester, IsEditable, IsPassOut);
        }

        public List<ResultList> RevaluationList(PrintProgramme printProgramme, Parameters parameter, short? semester, bool IsEditable = false)
        {
            return new ResultDB().RevaluationList(printProgramme, parameter, semester, IsEditable);
        }
        public List<ResultList> History(PrintProgramme printProgramme, Parameters parameter, short? semester, bool IsEditable = false)
        {
            return new ResultDB().ResultHistory_SubjectResult(printProgramme, parameter, semester, IsEditable);
        }

        #region  Result MeritList

        public List<ResultList> ResultMeritList(PrintProgramme printProgramme, Parameters parameter, bool IsEditable = false)
        {
            return new ResultDB().ResultMeritList(printProgramme, parameter, IsEditable);
        }

        #endregion

        #region Result PendingList
        public DataTable ResultPendingList(Parameters parameter, PrintProgramme printProgramme, short Semester, int batch, Programme programe, Guid CourseId)
        {
            return new ResultDB().ResultPendingList(parameter, printProgramme, Semester, batch, programe, CourseId);
        }
        #endregion

        #region Result DataList
        public DataTable ResultDataList(Parameters parameter, PrintProgramme printProgramme, short Semester, int batch, Programme programe, Guid CourseId, Guid AcceptCollege_ID, bool value)
        {
            return new ResultDB().ResultDataList(parameter, printProgramme, Semester, batch, programe, CourseId, AcceptCollege_ID, value);
        }
        #endregion




        /// <summary>
        /// method used in view  like marks card, backform
        /// </summary>
        /// <param name="IsEditable">set true in case to get all rows irrespective of result</param>
        /// <returns></returns>
        //public ResultCompact GetStudentResult(PrintProgramme PrintProgramme, short Semester, Guid Student_ID, bool IsEditable)
        //{
        //    ResultCompact studentDetail = studentDetail = new ResultDB().StudentInfoBacklogonly(PrintProgramme, Semester, Student_ID);
        //    if (studentDetail != null)
        //    {
        //        studentDetail.SubjectResults = new ResultDB().SubjectResultCompactList_Public(PrintProgramme, Semester, studentDetail.Student_ID, IsEditable);
        //        if (studentDetail.SubjectResults.IsNotNullOrEmpty())
        //        {
        //            studentDetail.SubjectResults = studentDetail.SubjectResults.Where(x => !(x._ID == Guid.Empty)).ToList();
        //        }

        //        if (!IsEditable)
        //            studentDetail.SubjectResults = studentDetail.SubjectResults != null && studentDetail.SubjectResults.Any(x => !string.IsNullOrEmpty(x.NotificationNo)) ? studentDetail.SubjectResults : null;


        //    }

        //    return studentDetail;
        //}

        public ResponseData SaveUpdate(ResultList _ResultListItem)
        {
            PrintProgramme printProgramme = GeneralFunctions.ProgrammeToPrintProgrammeMapping(_ResultListItem.Programme); var response = new ResponseData(); var RESULTDB = new ResultDB();
            if (_ResultListItem != null)
            {
                if (_ResultListItem._ID == Guid.Empty)
                {
                    SemesterModel _SemesterModel = new SemesterModel()
                    {
                        _ID = Guid.NewGuid(),
                        Student_ID = _ResultListItem.Student_ID,
                        Subject_ID = _ResultListItem.Subject_ID,
                        InternalMarks = _ResultListItem.InternalMarks,
                        InternalAttendance_AssessmentMarks = _ResultListItem.InternalAttendance_AssessmentMarks,
                        ExternalMarks = _ResultListItem.ExternalMarks,
                        ExternalAttendance_AssessmentMarks = _ResultListItem.ExternalAttendance_AssessmentMarks,
                        ExternalSubmitted = _ResultListItem.ExternalSubmitted,
                        InternalSubmitted = _ResultListItem.InternalSubmitted,
                        ResultNotification_ID = _ResultListItem.ResultNotificationID,
                        ExamForm_ID = _ResultListItem.ExamForm_ID
                    };
                    _SemesterModel.ExternalUpdatedOn = _SemesterModel.InternalUpdatedOn = DateTime.Now;
                    _SemesterModel.ExternalUpdatedBy = _SemesterModel.InternalUpdatedBy = AppUserHelper.User_ID;
                    if (_ResultListItem.ResultNotification_ID.HasValue) _SemesterModel.InternalSubmitted = _SemesterModel.ExternalSubmitted = true;
                    response.NumberOfRecordsEffected += RESULTDB.InsertAward(_SemesterModel, printProgramme, _ResultListItem.Semester);
                    _ResultListItem._ID = _SemesterModel._ID;
                }
                else if (_ResultListItem.RecordState == RecordState.Dirty)
                {
                    var _SemesterModelDB = RESULTDB.GetSemesterResultBy_ID(_ResultListItem._ID, printProgramme, _ResultListItem.Semester);
                    _SemesterModelDB.ExternalUpdatedOn = _SemesterModelDB.ExternalMarks!=_ResultListItem.ExternalMarks ? DateTime.Now : _SemesterModelDB.ExternalUpdatedOn;
                    _SemesterModelDB.InternalUpdatedOn = (_SemesterModelDB.InternalMarks != _ResultListItem.InternalMarks|| _SemesterModelDB.InternalAttendance_AssessmentMarks != _ResultListItem.InternalAttendance_AssessmentMarks|| _SemesterModelDB.ExternalAttendance_AssessmentMarks != _ResultListItem.ExternalAttendance_AssessmentMarks) ? DateTime.Now : _SemesterModelDB.InternalUpdatedOn;
                    _SemesterModelDB.ExternalUpdatedBy = _SemesterModelDB.ExternalMarks != _ResultListItem.ExternalMarks ? AppUserHelper.User_ID : _SemesterModelDB.ExternalUpdatedBy;
                    _SemesterModelDB.InternalUpdatedBy = (_SemesterModelDB.InternalMarks != _ResultListItem.InternalMarks || _SemesterModelDB.InternalAttendance_AssessmentMarks != _ResultListItem.InternalAttendance_AssessmentMarks || _SemesterModelDB.ExternalAttendance_AssessmentMarks != _ResultListItem.ExternalAttendance_AssessmentMarks) ? AppUserHelper.User_ID : _SemesterModelDB.InternalUpdatedBy;

                    _SemesterModelDB.InternalMarks = _ResultListItem.InternalMarks;
                    _SemesterModelDB.InternalAttendance_AssessmentMarks = _ResultListItem.InternalAttendance_AssessmentMarks;
                    _SemesterModelDB.ExternalMarks = _ResultListItem.ExternalMarks;
                    _SemesterModelDB.ExternalAttendance_AssessmentMarks = _ResultListItem.ExternalAttendance_AssessmentMarks;
                    _SemesterModelDB.ExternalSubmitted = _ResultListItem.ExternalSubmitted;
                    _SemesterModelDB.InternalSubmitted = _ResultListItem.InternalSubmitted;
                    _SemesterModelDB.ResultNotification_ID = _ResultListItem.ResultNotification_ID;
                    _SemesterModelDB.ExamForm_ID = _ResultListItem.ExamForm_ID;
                    if (_ResultListItem.ResultNotification_ID.HasValue) _SemesterModelDB.InternalSubmitted = _SemesterModelDB.ExternalSubmitted = true;
                    response.NumberOfRecordsEffected = RESULTDB.UpdateAward(_SemesterModelDB, printProgramme, _ResultListItem.Semester);
                    response.NumberOfRecordsEffected = response.NumberOfRecordsEffected == 2 ? 1 : response.NumberOfRecordsEffected;
                }
            }
            if (response.NumberOfRecordsEffected > 0)
            {
                response.ErrorMessage = "";
                response.SuccessMessage = response.NumberOfRecordsEffected + " Record saved successfully.";
                response.IsSuccess = true;
                Parameters parameter = new Parameters() { Filters = new List<SearchFilter>() { new SearchFilter() { Column = "_ID", Operator = SQLOperator.EqualTo, Value = _ResultListItem._ID.ToString() }, new SearchFilter() { TableAlias = "Comb", Column = "Semester", Operator = SQLOperator.EqualTo, Value = _ResultListItem.Semester.ToString() } } };
                response.ResponseObject = RESULTDB.ResultList_SubjectResult(GeneralFunctions.ProgrammeToPrintProgrammeMapping(_ResultListItem.Programme), parameter, _ResultListItem.Semester, true)?.FirstOrDefault();
            }
            return response;
        }

        public bool CheckResultCancelled(Guid iD, Guid? examForm_ID)
        {
            return (new ResultDB().CheckResultCancelled(iD, examForm_ID) ?? new List<ResultAnomalies>()).Count > 0;
        }

        public bool ResultAnomalyExistsAlready(Guid subject_Id, Guid student_ID, Guid examForm_ID)
        {
            return (new ResultDB().ResultAnomalyExistsAlready(subject_Id, student_ID, examForm_ID) ?? new List<ResultAnomalies>()).Count > 0;
        }

        public int RemoveResut(ResultAnomalies model)
        {
            return new ResultDB().RemoveResut(model);
        }

        public int SaveAnomaly(ResultAnomalies model)
        {
            return new ResultDB().SaveAnomaly(model);
        }

        public ResponseData SaveUpdate(ResultCompact _ResultCompact)
        {
            if (!AppUserHelper.AppUsercompact.UserRoles.Any(x => x == AppRoles.University))
                return new ResponseData() { ErrorMessage = "You are not authorized to update records." };

            var response = new ResponseData() { ErrorMessage = "No, Record Updated" };
            var RESULTDB = new ResultDB();
            var STDINFO = new StudentManager().GetStudentWithCombination(_ResultCompact.Student_ID, _ResultCompact.PrintProgramme, false, _ResultCompact.Semester);
            if (STDINFO == null)
                return new ResponseData() { ErrorMessage = "Student not found." };
            if (STDINFO.SelectedCombinations.IsNullOrEmpty())
                return new ResponseData() { ErrorMessage = "Student subjects not found." };

            var _NewDirtyResultRecord = _ResultCompact?.SubjectResults?.Where(x => x.RecordState == RecordState.New || x.RecordState == RecordState.Dirty)?.ToList();
            if (_NewDirtyResultRecord.IsNotNullOrEmpty())
            {
                var IsValidRow = false;
                foreach (var item in _NewDirtyResultRecord)
                {
                    if (STDINFO.SelectedCombinations.First().Subjects.Any(x => x.Subject_ID == item.Subject_ID))
                        IsValidRow = true;
                    else
                    {
                        List<StudentAdditionalSubject> studentAddionalSubjects = new StudentManager().GetStudentAdditionalSubjects(STDINFO.Student_ID, _ResultCompact.Semester, _ResultCompact.PrintProgramme);
                        if (studentAddionalSubjects.IsNotNullOrEmpty() && studentAddionalSubjects.Any(x => x.Subject_ID == item.Subject_ID))
                            IsValidRow = true;
                    }
                    if (IsValidRow)
                    {
                        if (item._ID == Guid.Empty)
                        {
                            SemesterModel _SemesterModel = new SemesterModel()
                            {
                                _ID = Guid.NewGuid(),
                                Student_ID = _ResultCompact.Student_ID,
                                Subject_ID = item.Subject_ID,
                                CUSRegistrationNo = _ResultCompact.CUSRegistrationNo,
                                ExamRollNumber = _ResultCompact.ExamRollNumber,
                                InternalMarks = item.InternalMarks,
                                InternalAttendance_AssessmentMarks = item.InternalAttendance_AssessmentMarks,
                                ExternalMarks = item.ExternalMarks,
                                ExternalAttendance_AssessmentMarks = item.ExternalAttendance_AssessmentMarks,
                                ExternalSubmitted = item.ExternalSubmitted,
                                InternalSubmitted = item.InternalSubmitted,
                                ResultNotification_ID = item.ResultNotification_ID,
                                ExamForm_ID = item.ExamForm_ID
                            };
                            _SemesterModel.ExternalUpdatedOn = _SemesterModel.InternalUpdatedOn = DateTime.Now;
                            _SemesterModel.ExternalUpdatedBy = _SemesterModel.InternalUpdatedBy = AppUserHelper.User_ID;
                            if (item.ResultNotification_ID.HasValue) _SemesterModel.InternalSubmitted = _SemesterModel.ExternalSubmitted = true;
                            response.NumberOfRecordsEffected += RESULTDB.InsertAward(_SemesterModel, _ResultCompact.PrintProgramme, _ResultCompact.Semester);
                        }
                        else if (item.RecordState == RecordState.Dirty)
                        {
                            var _SemesterModelDB = RESULTDB.GetSemesterResultBy_ID(item._ID, _ResultCompact.PrintProgramme, _ResultCompact.Semester);
                            _SemesterModelDB.ExternalUpdatedOn = _SemesterModelDB.InternalUpdatedOn = DateTime.Now;
                            _SemesterModelDB.ExternalUpdatedBy = _SemesterModelDB.InternalUpdatedBy = AppUserHelper.User_ID;
                            _SemesterModelDB.InternalMarks = item.InternalMarks;
                            _SemesterModelDB.InternalAttendance_AssessmentMarks = item.InternalAttendance_AssessmentMarks;
                            _SemesterModelDB.ExternalMarks = item.ExternalMarks;
                            _SemesterModelDB.ExternalAttendance_AssessmentMarks = item.ExternalAttendance_AssessmentMarks;
                            _SemesterModelDB.ExternalSubmitted = item.ExternalSubmitted;
                            _SemesterModelDB.InternalSubmitted = item.InternalSubmitted;
                            _SemesterModelDB.ResultNotification_ID = item.ResultNotification_ID;
                            _SemesterModelDB.ExamForm_ID = item.ExamForm_ID;
                            if (item.ResultNotification_ID.HasValue) _SemesterModelDB.InternalSubmitted = _SemesterModelDB.ExternalSubmitted = true;
                            response.NumberOfRecordsEffected += RESULTDB.UpdateAward(_SemesterModelDB, _ResultCompact.PrintProgramme, _ResultCompact.Semester);
                        }
                    }
                }
            }

            if (response.NumberOfRecordsEffected > 0)
            {
                response.ErrorMessage = "";
                response.SuccessMessage = response.NumberOfRecordsEffected + " Record saved successfully.";
                response.IsSuccess = true;
                response.ResponseObject = GetResult(_ResultCompact.PrintProgramme, _ResultCompact.Semester, _ResultCompact.Student_ID, true);
            }
            return response;
        }

        public List<ResultAnomalies> GetALLResultAnomalies(Parameters parameter)
        {
            return new ResultDB().GetALLResultAnomalies(parameter);
        }


        /// <summary>
        /// Used in updated student combination to check whether to allow update student subject combination or not
        /// </summary>
        /// <param name="printProgramme"></param>
        /// <param name="Semester"></param>
        /// <param name="Student_ID"></param>
        /// <returns></returns>
        public bool ResultCreated(PrintProgramme printProgramme, short Semester, Guid Student_ID)
        {
            return new ResultDB().ResultCreated(printProgramme, Semester, Student_ID);
        }

        public int FinalSubmit(List<string> student_Ids, SubmitAward input)
        {
            ADMSubjectMaster subject = new SubjectsManager().Get(new Guid(input.CombinationSubjects));
            bool IsSkillORGE = (subject.SubjectType == SubjectType.SEC) || (subject.SubjectType == SubjectType.GE);
            AwardSettingsModel model = new ResultDB().FetchAwardSettings(input);
            bool settingsaved = false;
            if (model == null)
            {
                bool ISFinalSubmit = false;
                bool ISFinalSubmitTheory = false;
                if (input.MarksFor == MarksFor.Practical)
                { ISFinalSubmit = true; }
                else { ISFinalSubmitTheory = true; }
                model = new AwardSettingsModel() { AwardSettings_ID = Guid.NewGuid(), Batch = input.Batch, Year = input.Year, SUBJECT_ID = new Guid(input.CombinationSubjects), ISFinalSubmit = ISFinalSubmit, ISFinalSubmitTheory = ISFinalSubmitTheory, Semester = Convert.ToInt32(input.Semester), USER_ID = AppUserHelper.User_ID };
                model.SetWorkFlow(RecordState.New);
                settingsaved = new ResultDB().AddAwardSettings(model) > 0;
            }
            else
            {
                if (input.MarksFor == MarksFor.Practical)
                {
                    model.ISFinalSubmit = true;
                    model.Batch = input.Batch;
                }
                else
                {
                    model.ISFinalSubmitTheory = true;
                    model.Year = input.Year;
                }
                model.SetWorkFlow(RecordState.Old);
                settingsaved = new ResultDB().EditAwardSettings(model) > 0;
            }


            if (settingsaved) return new ResultDB().UpdateAward(input, "Subject_ID", student_Ids, IsSkillORGE);
            return 0;

        }

        public int ReplaceSubjectInResult(Programme programme, short semester, Guid _Combination_ID, Guid _FindSubject, Guid _ReplaceWithSubject)
        {
            return new ResultDB().ReplaceSubjectInResult(programme, semester, _Combination_ID, _FindSubject, _ReplaceWithSubject);
        }


        #endregion

        #region MasterResultSection


        public ResponseData ImportResultToMasterTable(Guid _ResultNotification_ID)
        {
            ResponseData response = new ResponseData() { ErrorMessage = "No change" }; ResultDB _RESULTDB = new ResultDB(); var _CreatedOn = DateTime.Now; MasterResultRow _masterResult = null; MasterResultRow _dbMasterResult = null; bool _MasterRowSaved = false, _MasterRowUpdated = false;
            ResultNotification resultNotification = new ResultNotificationManager().Get(_ResultNotification_ID);
            Parameters parameter = new Parameters() { Filters = new List<SearchFilter>() { new SearchFilter() { Column = "ResultNotification_ID", Operator = SQLOperator.EqualTo, Value = resultNotification.ResultNotification_ID.ToString() }, new SearchFilter() { Column = "ParentNotification_ID", Operator = SQLOperator.EqualTo, IsSibling = true, Value = resultNotification.ResultNotification_ID.ToString() }, new SearchFilter() { Column = "ImportedToMasterTable", Operator = SQLOperator.NotEqualTo, Value = "1" } }, PageInfo = new Paging() { DefaultOrderByColumn = "Student_ID", PageNumber = -1, PageSize = -1 } };
            // parameter.PageInfo = new Paging() { DefaultOrderByColumn = "Student_ID", PageNumber = 1, PageSize = 10 };
            List<ResultList> list = new ResultDB().CompactList(parameter, resultNotification.PrintProgramme, resultNotification.Semester);
            if (list.IsNotNullOrEmpty())
            {
                foreach (var item in list)
                {
                    try
                    {
                        _MasterRowSaved = _MasterRowUpdated = false;
                        _dbMasterResult = _RESULTDB.GetMasterResult(item.Programme, item.Student_ID, item.Semester, item.Subject_ID);
                        _masterResult = FillResultRow(item);
                        if (_dbMasterResult == null)
                        {
                            _masterResult.CreatedOn = _CreatedOn;
                            _MasterRowSaved = _RESULTDB.Save(_masterResult, item.Programme) >= 1;
                        }
                        else
                        {
                            bool IsSame = CompareMasterResult(_dbMasterResult, _masterResult);
                            if (IsSame) { _MasterRowUpdated = true; }
                            else
                            {
                                _masterResult.CreatedOn = _CreatedOn;
                                _masterResult._ID = _dbMasterResult._ID;
                                _MasterRowUpdated = _RESULTDB.Update(_masterResult, _masterResult.Programme) >= 1;
                            }
                        }
                        if ((_MasterRowSaved || _MasterRowUpdated) && _RESULTDB.UpdateImportStatus(item.Programme, item.Semester, item._ID, _CreatedOn) >= 1)
                            response.NumberOfRecordsEffected += 1;
                        else
                            _RESULTDB.Save(new ResultErrorLog() { Message = "!Atomicity", _ID = item._ID, PrintProgramme = resultNotification.PrintProgramme, Semester = item.Semester, ResultErrorLog_ID = Guid.NewGuid(), Dated = _CreatedOn });
                    }
                    catch (Exception e)
                    {
                        _RESULTDB.Save(new ResultErrorLog() { Message = e.Message, _ID = item._ID, PrintProgramme = resultNotification.PrintProgramme, Semester = item.Semester, ResultErrorLog_ID = Guid.NewGuid(), Dated = _CreatedOn });
                    }
                }
                response.ErrorMessage = response.NumberOfRecordsEffected != list.Count ? "Total records=" + list.Count + " from which" + (list.Count - response.NumberOfRecordsEffected).ToString() + " records failed" : "";
                response.IsSuccess = response.NumberOfRecordsEffected > 0;
                if (response.IsSuccess) response.SuccessMessage = $"Out of {list.Count} , {response.NumberOfRecordsEffected} updated successfully.";
                if (response.NumberOfRecordsEffected == list.Count)
                    new ResultNotificationDB().UpdateImportStatus(_ResultNotification_ID);
            }
            return response;
        }

        private bool CompareMasterResult(MasterResultRow dbMasterResult, MasterResultRow masterResult)
        {
            bool IsSame = true;
            //IsSame = dbMasterResult.Equals(masterResult);
            if (dbMasterResult.ResultNotification_ID != masterResult.ResultNotification_ID) return false;
            if (dbMasterResult.ExamForm_ID != masterResult.ExamForm_ID) return false;
            //if (dbMasterResult.ExternalApplicable != masterResult.ExternalApplicable) return false;
            if (dbMasterResult.ExternalMarks != masterResult.ExternalMarks) return false;
            if (dbMasterResult.ExternalMaxMarks != masterResult.ExternalMaxMarks) return false;
            if (dbMasterResult.ExternalMinPassMarks != masterResult.ExternalMinPassMarks) return false;
            if (dbMasterResult.ExternalStatus != masterResult.ExternalStatus) return false;
            //if (dbMasterResult.InternalApplicable != masterResult.InternalApplicable) return false;
            if (dbMasterResult.InternalMarks != masterResult.InternalMarks) return false;
            if (dbMasterResult.InternalMaxMarks != masterResult.InternalMaxMarks) return false;
            if (dbMasterResult.InternalMinPassMarks != masterResult.InternalMinPassMarks) return false;
            if (dbMasterResult.InternalStatus != masterResult.InternalStatus) return false;
            if (dbMasterResult.Programme != masterResult.Programme) return false;
            if (dbMasterResult.Remark != masterResult.Remark) return false;
            if (dbMasterResult.ResultStatus != masterResult.ResultStatus) return false;
            if (dbMasterResult.Semester != masterResult.Semester) return false;
            if (dbMasterResult.Student_ID != masterResult.Student_ID) return false;
            if (dbMasterResult.Subject_ID != masterResult.Subject_ID) return false;
            return IsSame;
        }

        private MasterResultRow FillResultRow(ResultList item)
        {
            MasterResultRow _masterResult = new MasterResultRow()
            {
                _ID = item._ID,
                Student_ID = item.Student_ID,
                Semester = item.Semester,
                Subject_ID = item.Subject_ID,
                ResultNotification_ID = item.ResultNotification_ID.Value,
                ExamForm_ID = item.ExamForm_ID.Value,
                Programme = item.Programme,
                CreatedBy = AppUserHelper.User_ID
            };
            _masterResult.InternalStatus = item.InternalStatus;
            if (_masterResult.InternalStatus != ResultStatus.NotApplicable)
            {
                _masterResult.InternalMaxMarks = short.Parse(item.TotalInternalMaxMarks.Value.ToString("f0"));
                _masterResult.InternalMinPassMarks = short.Parse(item.TotalInternalMinPassMarks.Value.ToString("f0"));
                if (item.TotalInternalMarksObtained.HasValue) _masterResult.InternalMarks = short.Parse(item.TotalInternalMarksObtained.Value.ToString("f0"));
            }
            else
                _masterResult.InternalMaxMarks = _masterResult.InternalMinPassMarks = _masterResult.InternalMarks = null;


            _masterResult.ExternalStatus = item.ExternalStatus;
            if (_masterResult.ExternalStatus != ResultStatus.NotApplicable)
            {
                _masterResult.ExternalMaxMarks = short.Parse(item.TotalExternalMaxMarks.Value.ToString("f0"));
                _masterResult.ExternalMinPassMarks = short.Parse(item.TotalExternalMinPassMarks.Value.ToString("f0"));
                if (item.TotalExternalMarksObtained.HasValue) _masterResult.ExternalMarks = short.Parse(item.TotalExternalMarksObtained.Value.ToString("f0"));
            }
            else
                _masterResult.ExternalMaxMarks = _masterResult.ExternalMinPassMarks = _masterResult.ExternalMarks = null;

            //_masterResult.Remark = $@"RolNo:{item.ExamRollNumber}#Sub:{item.SubjectFullName}#{item.SubjectType.ToString()}#{(item.IsExternalMarksApplicable ? "Ext:" + item.ExternalMarks + "/" + item.ExternalMaxMarks : "")}#{(item.IsExternalAttendance_AssessmentApplicable ? "ExtAsgn:" + item.ExternalAttendance_AssessmentMarks + "/" + item.ExternalAttendance_AssessmentMaxMarks : "")}#{(item.IsInternalMarksApplicable ? "Int:" + item.InternalMarks + "/" + item.InternalMaxMarks : "")}#{(item.IsInternalAttendance_AssessmentApplicable ? "IntAsgn:" + item.InternalAttendance_AssessmentMarks + "/" + item.InternalAttendance_AssessmentMaxMarks : "")}";
            return _masterResult;
        }


        internal Tuple<List<SubjectResult>, List<SubjectResult>> GetPassed_FailedSubjects(List<SubjectResult> subjectResults, short Semester, bool CheckResultDeclared = true)
        {
            List<SubjectResult> PassedSubjects = new List<SubjectResult>();
            List<SubjectResult> FailedSubjects = new List<SubjectResult>();
            if (subjectResults.IsNotNullOrEmpty())
                foreach (var item in subjectResults)
                {
                    if (!item.HasResult) { continue; }

                    if (item.OverallResultStatus == ResultStatus.P)
                    {
                        PassedSubjects.Add(item);
                    }
                    else
                    {
                        FailedSubjects.Add(item);
                    }
                }
            return Tuple.Create(PassedSubjects, FailedSubjects);
        }

        #endregion



        public int GetOFFSet(Parameters parameter)
        {
            int offset = 0;
            if (parameter != null && parameter.PageInfo != null)
            {
                offset = (parameter.PageInfo.PageNumber - 1) * parameter.PageInfo.PageSize;

            }

            return offset;
        }

        public Task<int> AddResultAsync(List<AwardModel> semesterModelList, string semesterName, Programme programme, MarksFor marksFor)
        {
            Guid User_ID = AppUserHelper.User_ID;
            return new TaskFactory().StartNew(() => AddResult(semesterModelList, semesterName, programme, User_ID, marksFor));
        }

        private int AddResult(List<AwardModel> semesterModelList, string semesterName, Programme programme, Guid User_ID, MarksFor marksFor)
        {
            MSSubjectMarksStructure ss = new SubjectsManager().Get((semesterModelList ?? new List<AwardModel>()).FirstOrDefault()?.Subject_ID ?? Guid.Empty);
            if (ss == null)
                return 0;

            SemesterModel semester = null;
            int numberOfRecordsEffected = 0;
            int totalrecordssaved = 0;
            foreach (AwardModel award in semesterModelList ?? new List<AwardModel>())
            {
                semester = new SemesterModel()
                {
                    _ID = award._ID,
                    CUSRegistrationNo = award.CUSRegistrationNo,
                    ExamRollNumber = award.ExamRollNumber,
                    Subject_ID = award.Subject_ID,
                    ExternalMarks = award.ExternalMarks,
                    ExternalAttendance_AssessmentMarks = award.ExternalAttendance_AssessmentMarks,
                    InternalMarks = award.InternalMarks,
                    InternalAttendance_AssessmentMarks = award.InternalAttendance_AssessmentMarks,
                    ExternalSubmitted = award.ExternalSubmitted,
                    InternalSubmitted = award.InternalSubmitted,
                    Student_ID = award.Student_ID,
                    ExamForm_ID = award.ExamForm_ID,
                    IsBacklog = award.IsBacklog
                };
                ADMSubjectMaster subj = new SubjectsManager().Get(award.Subject_ID);
                SubjectType subjectType = subj != null ? subj.SubjectType : SubjectType.None;
                bool updateexternalforI = false;


                if (marksFor == MarksFor.Practical)
                {
                    if (award.IsBacklog && ss.ExternalVisibleTo == AwardModuleType.University)
                    { semester.ExternalMarks = 0; semester.ExternalSubmitted = false; updateexternalforI = true; }
                    semester.InternalUpdatedOn = DateTime.Now;
                    semester.InternalUpdatedBy = User_ID;
                }
                else
                {
                    semester.ExternalUpdatedOn = DateTime.Now;
                    semester.ExternalUpdatedBy = User_ID;
                }
                ADMSubjectMaster subject = new SubjectsManager().Get(award.Subject_ID);
                if (subject != null)
                {
                    bool IsSkillORGE = (subject.SubjectType == SubjectType.SEC) || (subject.SubjectType == SubjectType.GE);
                    if (IsSkillORGE)
                    { programme = GetStudentProgramme(award.Student_ID, programme); }
                }

                using (TransactionScope transaction = new TransactionScope())
                {
                    try
                    {
                        //  numberOfRecordsEffected += AddPreviousResult(semester, semesterName, programme);
                        numberOfRecordsEffected += AddOrEditResult(semester, semesterName, programme, marksFor, ss, updateexternalforI);
                        totalrecordssaved++;
                    }
                    catch (SqlException ex) when (ex.Number == 1205 || ex.Number == 2627) //unique key
                    { }
                    catch (SqlException SQLError) when (SQLError.Number == 1205) //The transaction has aborted
                    { }
                    finally { transaction.Complete(); }
                }
            }

            semesterModelList = null;//Dispose
            return totalrecordssaved;
        }

        public ResponseData PostRapidEntry(SemesterModel model, short semester, Programme programme)
        {
            MSSubjectMarksStructure ss = new SubjectsManager().Get(model.Subject_ID);

            ResponseData response = new ResponseData();
            //   model.SetWorkFlow(RecordState.Old);
            bool isIntercourseSubject = (model.SubjectType == SubjectType.SEC) || (model.SubjectType == SubjectType.GE);
            if (isIntercourseSubject)
            { programme = GetStudentProgramme(model.Student_ID, programme); }
            response.NumberOfRecordsEffected = AddOrEditResult(model, semester.ToString(), programme, MarksFor.Practical, ss);
            if (response.NumberOfRecordsEffected > 0)
            {
                response.IsSuccess = true;
                response.SuccessMessage = "Record saved successfully.";
                var printProgramme = new GeneralFunctions().GetPrintProgramme(programme);
                var _model = new ResultDB().GetSemesterResultBy_ID(model._ID, printProgramme, semester);
                _model.IsBacklog = model.IsBacklog;
                response.ResponseObject = _model;
            }
            return response;
        }


        private Programme GetStudentProgramme(Guid student_ID, Programme programme)
        {
            if (new StudentManager().GetStudent(student_ID, PrintProgramme.UG, false) != null)
            { return Programme.UG; }
            else if (new StudentManager().GetStudent(student_ID, PrintProgramme.IH, false) != null)
            {
                return Programme.IG;
            }
            else return programme;
        }


        //public int AddPreviousResult(SemesterModel semester, string semesterName, Programme programme)
        //{
        //    SemesterModel semesterModel = GetSemesterByRegistrationNoAndSubject(semester.Subject_ID, semester.CUSRegistrationNo, semester.Student_ID, semesterName, programme);
        //    if (semesterModel != null)
        //    {
        //        if (semesterModel.ResultNotification_ID != null || semester.IsBacklog)
        //        {
        //            //save previous result
        //            ResultHistory resultHistory = GetResultHistory(semesterModel, programme, Convert.ToInt32(semesterName));
        //            resultHistory.CreatedOn = DateTime.UtcNow;
        //            //    resultHistory.CreatedBy = semester.CreatedBy;

        //            return new ResultDB().AddResultHistory(resultHistory);

        //        }
        //        //else
        //        //{
        //        //    //save Simple Edit in history
        //        //    semesterModel.CreatedOn = DateTime.UtcNow;
        //        //    semesterModel.CreatedBy = semester.CreatedBy;

        //        //    string _id = semesterModel._ID.ToString();
        //        //    semester._ID = Guid.Parse(_id);
        //        //    return new ResultDB().AddResult(semesterModel, semesterName, programme, true);
        //        //}
        //    }

        //    return 1;
        //}

        //private ResultHistory GetResultHistory(SemesterModel semesterModel, Programme programme, int semester)
        //{
        //    ResultHistory resultHistory = new ResultHistory()
        //    {
        //        ResultHistory_ID = Guid.NewGuid(),
        //        Semester_ID = semesterModel._ID,
        //        Programme = programme,
        //        Semester = (Semester)semester,
        //        ResultNotification_ID = semesterModel.ResultNotification_ID,
        //        ExternalAttendance_AssessmentMarks = semesterModel.ExternalAttendance_AssessmentMarks,
        //        InternalAttendance_AssessmentMarks = semesterModel.InternalAttendance_AssessmentMarks,
        //        ExternalMarks = semesterModel.ExternalMarks,
        //        InternalMarks = semesterModel.InternalMarks,
        //        ExamForm_ID = semesterModel.ExamForm_ID
        //        // PracticalExternalMarks= semesterModel.PracticalExternalMarks,
        //        //                PracticalInternalMarks = semesterModel.PracticalExternalMarks
        //    };
        //    return resultHistory;

        //}

        //private SemesterModel GetSemesterByRegistrationNoAndSubject(Guid subject_ID, string registrationNo, Guid StudentId, string semesterName, Programme programme)
        //{
        //    return new ResultDB().GetSemesterByRegistrationNoAndSubject(subject_ID, registrationNo, StudentId, semesterName, programme);
        //}

        public int AddOrEditResult(SemesterModel semester, string semesterName, Programme programme, MarksFor marksFor, MSSubjectMarksStructure ss, bool updateexternalforI = false)
        {
            if (semester._ID != null && semester._ID != Guid.Empty)
            {
                //edit result
                List<string> ignoreQuery = new List<string>() {
                 nameof(semester._ID),
                 nameof(semester.Student_ID),
                nameof(semester.Subject_ID)
                              };
                List<string> externalcolumns = new List<string>();
                List<string> internalcolumns = new List<string>();
                if (ss.InternalAttendance_AssessmentVisibleTo == AwardModuleType.College) { internalcolumns.Add(nameof(semester.InternalAttendance_AssessmentMarks)); } else { externalcolumns.Add(nameof(semester.InternalAttendance_AssessmentMarks)); }
                if (ss.InternalVisibleTo == AwardModuleType.College) { internalcolumns.Add(nameof(semester.InternalMarks)); } else { externalcolumns.Add(nameof(semester.InternalMarks)); }
                if (ss.ExternalAttendance_AssessmentVisibleTo == AwardModuleType.College) { internalcolumns.Add(nameof(semester.ExternalAttendance_AssessmentMarks)); } else { externalcolumns.Add(nameof(semester.ExternalAttendance_AssessmentMarks)); }
                if (ss.ExternalVisibleTo == AwardModuleType.College) { internalcolumns.Add(nameof(semester.ExternalMarks)); } else { externalcolumns.Add(nameof(semester.ExternalMarks)); }

                if (marksFor == MarksFor.Practical)
                {
                    ignoreQuery.AddRange(new List<string>() {
                 nameof(semester._ID),
                 nameof(semester.Student_ID),
                nameof(semester.Subject_ID),
                nameof(semester.ExternalUpdatedOn),
                nameof(semester.ExternalUpdatedBy),
                              });
                    if (!updateexternalforI)
                    { ignoreQuery.AddRange(externalcolumns); }
                }
                else
                {
                    ignoreQuery.AddRange(new List<string>() {
                 nameof(semester._ID),
                 nameof(semester.Student_ID),
                nameof(semester.Subject_ID),
                nameof(semester.InternalUpdatedOn),
                nameof(semester.InternalUpdatedBy),
                              });
                    ignoreQuery.AddRange(internalcolumns);
                }
                return new ResultDB().EditResult(semester, semesterName, programme, ignoreQuery, null);
            }
            else
            {
                //add result
                semester._ID = Guid.NewGuid();
                return new ResultDB().AddResult(semester, semesterName, programme);
            }
        }

        public List<SemesterModel> GetSemesterResultByStudentID(Guid Student_ID, string table, string joinColumn, bool checkResultDecleared, PrintProgramme programme)
        {
            return new ResultDB().GetSemesterResultByStudentID(Student_ID, table, joinColumn, checkResultDecleared, programme);
        }

        public bool CheckAwardExists(SubmitAward input)
        {
            Guid SubjectId = new Guid(input.CombinationSubjects);
            ADMSubjectMaster subject = new SubjectsManager().Get(SubjectId);
            bool IsSkillORGE = (subject?.SubjectType == SubjectType.SEC) || (subject?.SubjectType == SubjectType.GE);
            if (new ResultDB().CheckAwardExists(input))
            {

                return true;
            }
            else if (IsSkillORGE)
            {
                input.Programme = Programme.IG;
                return new ResultDB().CheckAwardExists(input);
            }

            return false;
        }
        public bool CheckAwardSubmitted(Parameters parameter, Programme programme, MarksFor marksFor, int batch)
        {
            return new ResultDB().CheckAwardSubmitted(parameter, programme, marksFor, batch);
        }
        public AwardFilterSettings FetchAwardFilterSettings(Programme programme, MarksFor marksFor, int semester)
        {
            return new ResultDB().FetchAwardFilterSettings(programme, marksFor, semester);
        }
        public bool checkIsResultDeclared(int Semester, Programme programme)
        {
            return new ResultDB().checkIsResultDeclared(Semester, programme);
        }
        public bool CheckAwardSubmittedEval(Parameters parameter, Programme programme, MarksFor marksFor, int Year)
        {

            return new ResultDB().CheckAwardSubmitted(parameter, programme, marksFor, Year);
        }
        public int Submit(SubmitAward input)
        {
            ADMSubjectMaster subject = new SubjectsManager().Get(new Guid(input.CombinationSubjects));
            bool IsSkillORGE = (subject.SubjectType == SubjectType.SEC) || (subject.SubjectType == SubjectType.GE);
            AwardSettingsModel model = new ResultDB().FetchAwardSettings(input);
            if (model == null)
            {
                bool ISFinalSubmit = false;
                bool ISFinalSubmitTheory = false;
                if (input.MarksFor == MarksFor.Practical)
                { ISFinalSubmit = true; }
                else { ISFinalSubmitTheory = true; }
                model = new AwardSettingsModel() { AwardSettings_ID = Guid.NewGuid(), Batch = input.Batch, Year = input.Year, SUBJECT_ID = new Guid(input.CombinationSubjects), ISFinalSubmit = ISFinalSubmit, ISFinalSubmitTheory = ISFinalSubmitTheory, Semester = Convert.ToInt32(input.Semester), USER_ID = AppUserHelper.User_ID };
                model.SetWorkFlow(RecordState.New);
                new ResultDB().AddAwardSettings(model);
            }
            else
            {
                if (input.MarksFor == MarksFor.Practical)
                {
                    model.ISFinalSubmit = true;
                    model.Batch = input.Batch;
                }
                else
                {
                    model.ISFinalSubmitTheory = true;
                    model.Year = input.Year;
                }
                model.SetWorkFlow(RecordState.Old);
                new ResultDB().EditAwardSettings(model);
            }


            return new ResultDB().UpdateSubmitAward(input, "Subject_ID", IsSkillORGE);

        }



        public ResponseData Printed(Guid student_ID)
        {
            var response = new ResponseData();
            response.NumberOfRecordsEffected = new ResultDB().Printed(student_ID);
            response.IsSuccess = response.NumberOfRecordsEffected > 0;
            return response;
        }



        public StudentProfile GetResultByStudentRegistrationNumber(short ApplyForSemester)
        {
            //ResultCompact _ResultListCompact = new ResultDB().StudentInfo(AppUserHelper.TableSuffix, ApplyForSemester, AppUserHelper.User_ID);

            Parameters param = new Parameters()
            {
                Filters = new List<SearchFilter>() { new SearchFilter() {
                Column = "Student_ID", Value = AppUserHelper.User_ID.ToString(), TableAlias = "S" },
                new SearchFilter() { Column = "Semester", Value = ApplyForSemester.ToString(), TableAlias = "Comb" } },
                PageInfo = new Paging() { DefaultOrderByColumn = "CUSRegistrationNo", PageNumber = -1, PageSize = -1 }
            };

            ResultCompact _ResultListCompact = Get(AppUserHelper.TableSuffix, ApplyForSemester, param)?.FirstOrDefault();

            if (_ResultListCompact == null)
                return null;

            StudentProfile student = new StudentProfile
            {
                Name = _ResultListCompact.FullName,
                RegistrationNumber = _ResultListCompact.CUSRegistrationNo,
                RollNumber = _ResultListCompact.ExamRollNumber,
                Student_ID = _ResultListCompact.Student_ID,
                FathersName = _ResultListCompact.FathersName,
                Subjects = new List<ADMSubjectMaster>()
            };
            return student;
        }


        public ResponseData RevaluationSaveUpdate(ResultList _ResultListItem)
        {
            PrintProgramme printProgramme = GeneralFunctions.ProgrammeToPrintProgrammeMapping(_ResultListItem.Programme); var response = new ResponseData();
            var RESULTDB = new ResultDB();
            if (_ResultListItem != null && _ResultListItem._ID != Guid.Empty)
            {
                if (_ResultListItem.RecordState == RecordState.Dirty)
                {
                    var _SemesterModelDB = RESULTDB.GetSemesterResultBy_ID(_ResultListItem._ID, printProgramme, _ResultListItem.Semester);

                    if (_ResultListItem.ResultNotificationID.HasValue)
                    {
                        _SemesterModelDB.ExternalMarks = _ResultListItem.ExternalMarks;
                        _SemesterModelDB.ResultNotification_ID = _ResultListItem.ResultNotificationID;
                        _SemesterModelDB.InternalSubmitted = _SemesterModelDB.ExternalSubmitted = true;
                    }
                    response.NumberOfRecordsEffected = RESULTDB.UpdateAward(_SemesterModelDB, printProgramme, _ResultListItem.Semester);
                    response.NumberOfRecordsEffected = response.NumberOfRecordsEffected == 2 ? 1 : response.NumberOfRecordsEffected;
                }
            }
            if (response.NumberOfRecordsEffected > 0)
            {
                response.ErrorMessage = "";
                response.SuccessMessage = response.NumberOfRecordsEffected + " Record saved successfully.";
                response.IsSuccess = true;
                Parameters parameter = new Parameters() { Filters = new List<SearchFilter>() { new SearchFilter() { Column = "_ID", Operator = SQLOperator.EqualTo, Value = _ResultListItem._ID.ToString() }, new SearchFilter() { TableAlias = "Comb", Column = "Semester", Operator = SQLOperator.EqualTo, Value = _ResultListItem.Semester.ToString() } } };
                response.ResponseObject = RESULTDB.ResultList_SubjectResult(GeneralFunctions.ProgrammeToPrintProgrammeMapping(_ResultListItem.Programme), parameter, _ResultListItem.Semester, true)?.FirstOrDefault();
            }
            return response;
        }




        /// <summary>
        /// This method should be used for Admit Cards & Attendance Sheets only
        /// </summary>
        /// <param name="subjectResults"></param>
        /// <param name="Semester"></param>
        /// <returns></returns>
        //        public List<ADMSubjectMaster> GetInternalPassedSubjects(ResultCompact subjectResults, short Semester)
        //        {
        //            List<ADMSubjectMaster> passedSubjects = new List<ADMSubjectMaster>();
        //            if (subjectResults != null)
        //            {
        //                foreach (SubjectResult item in subjectResults.SubjectResults)
        //                {
        //#pragma warning disable CS0219 // Variable is assigned but its value is never used
        //                    bool InternalApplicable = false; decimal InternalMaxMarks = 0; decimal InternalMinPassMarks = 0; decimal? InternalMarksObtained = null;
        //#pragma warning restore CS0219 // Variable is assigned but its value is never used

        //                    //if (item.IsInternalMarksApplicable)
        //                    //{
        //                    //    if (item.InternalIsPartOf == MarksIsPartOf.Internal)
        //                    //    {
        //                    //        InternalApplicable = true; InternalMinPassMarks += item.InternalMinPassMarks; InternalMaxMarks += item.InternalMaxMarks;

        //                    //        if (decimal.TryParse(item.InternalMarks, out decimal _Marks) && _Marks >= 0)
        //                    //        {
        //                    //            InternalMarksObtained = InternalMarksObtained.HasValue ? InternalMarksObtained : 0;
        //                    //            InternalMarksObtained += _Marks;
        //                    //        }
        //                    //    }
        //                    //}
        //                    //if (item.IsInternalAttendance_AssessmentApplicable)
        //                    //{
        //                    //    if (item.InternalAttendanceIsPartOf == MarksIsPartOf.Internal)
        //                    //    {
        //                    //        InternalApplicable = true; InternalMinPassMarks += item.InternalAttendance_AssessmentMinPassMarks; InternalMaxMarks += item.InternalAttendance_AssessmentMaxMarks;

        //                    //        if (decimal.TryParse(item.InternalAttendance_AssessmentMarks, out decimal _Marks) && _Marks >= 0)
        //                    //        {
        //                    //            InternalMarksObtained = InternalMarksObtained.HasValue ? InternalMarksObtained : 0;
        //                    //            InternalMarksObtained += _Marks;
        //                    //        }

        //                    //    }
        //                    //}
        //                    //if (item.IsExternalMarksApplicable)
        //                    //{
        //                    //    if (item.ExternalIsPartOf == MarksIsPartOf.Internal)
        //                    //    {
        //                    //        InternalApplicable = true; InternalMinPassMarks += item.ExternalMinPassMarks; InternalMaxMarks += item.ExternalMaxMarks;

        //                    //        if (decimal.TryParse(item.ExternalMarks, out decimal _Marks) && _Marks >= 0)
        //                    //        {
        //                    //            InternalMarksObtained = InternalMarksObtained.HasValue ? InternalMarksObtained : 0;
        //                    //            InternalMarksObtained += _Marks;
        //                    //        }
        //                    //    }
        //                    //}
        //                    //if (item.IsExternalAttendance_AssessmentApplicable)
        //                    //{
        //                    //    if (item.ExternalAttendanceIsPartOf == MarksIsPartOf.Internal)
        //                    //    {
        //                    //        InternalApplicable = true; InternalMinPassMarks += item.ExternalAttendance_AssessmentMinPassMarks; InternalMaxMarks += item.ExternalAttendance_AssessmentMaxMarks;

        //                    //        if (decimal.TryParse(item.ExternalAttendance_AssessmentMarks, out decimal _Marks) && _Marks >= 0)
        //                    //        {
        //                    //            InternalMarksObtained = InternalMarksObtained.HasValue ? InternalMarksObtained : 0;
        //                    //            InternalMarksObtained += _Marks;
        //                    //        }
        //                    //    }
        //                    //}

        //                    //if (InternalApplicable)
        //                    //{
        //                    //    if (InternalMarksObtained.HasValue)
        //                    //    {
        //                    //        if (InternalMarksObtained < InternalMinPassMarks)
        //                    //        {
        //                    //            //failed block
        //                    //        }
        //                    //        else
        //                    //        {
        //                    //            passedSubjects.Add(new ADMSubjectMaster
        //                    //            {
        //                    //                SubjectFullName = item.SubjectFullName,
        //                    //                SubjectType = item.SubjectType,
        //                    //                Subject_ID = item.Subject_ID
        //                    //            });
        //                    //        }
        //                    //    }
        //                    //}
        //                }
        //            }
        //            return passedSubjects;
        //        }

        #region ResultDiscrepancy

        public ResponseData Save(SubjectResultDiscrepancy model)
        {
            ResponseData response = new ResponseData() { };
            model.SubjectResultDiscrepancy_ID = Guid.NewGuid();
            response.NumberOfRecordsEffected = new ResultDB().Save(model);
            if (response.NumberOfRecordsEffected > 0)
            {
                response.IsSuccess = true;
                response.SuccessMessage = "Record saved successfully.";
                response.ResponseObject = new ResultManager().SubjectResultDiscrepancy(model.Student_ID);
            }
            return response;
        }

        public SubjectResultDiscrepancy SubjectResultDiscrepancy(Guid Student_ID)
        {
            return new ResultDB().SubjectResultDiscrepancy(Student_ID);
        }
        public DataTable SubjectResultDiscrepancyDataTable()
        {
            return new ResultDB().SubjectResultDiscrepancyDataTable();
        }
        #endregion


        //public int InsertTempAverageResult(string Query)
        //{
        //    return new ResultDB().InsertTempAverageResult(Query);
        //}

        #region Upload result
        public Task<Tuple<bool, string>> UploadExamResultAsync(UploadResult uploadResult, HttpPostedFileBase CSVResultFile)
        {
            return new TaskFactory().StartNew(() => UploadExamResult(uploadResult, CSVResultFile));
        }


        public Tuple<bool, string> UploadExamResult(UploadResult uploadResult, HttpPostedFileBase _CSVResultFile)
        {
            if (_CSVResultFile == null || _CSVResultFile?.ContentLength <= 0 || System.IO.Path.GetExtension(_CSVResultFile?.FileName).ToLower().Trim() != ".csv")
            {
                return Tuple.Create(false, "Please choose valid result file");
            }

            ExamResult examResults = null;
            List<string> ScriptToUploadList = new List<string>();
            StringBuilder ScriptToUpload = new StringBuilder();
            var _ResultDB = new ResultDB();
            int Counter = 0;
            bool isExisting = false;
            int uploadcnt = 0, insertcnt = 0;
            #region CSVParsing Work
            Regex CSVParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
            using (StreamReader csvreader = new StreamReader(_CSVResultFile.InputStream))
            {
                while (!csvreader.EndOfStream)
                {
                    string[] Fields = CSVParser.Split(csvreader.ReadLine());
                    if (Fields.Length != 2)
                    {
                        _CSVResultFile = null;//dispose
                        return Tuple.Create(false, $"Some row are not valid. Please check again.<br/> Operation aborted.");
                    }

                    if (!long.TryParse(Fields[0], out long _ExamRollNo))
                    {
                        _CSVResultFile = null;//dispose
                        return Tuple.Create(false, $"Some Roll Nos are not valid (Entrance RollNo Should be a number). Please check again.<br/> Operation aborted.");
                    }

                    if (!decimal.TryParse(Fields[1], out decimal _ExternalMarks))
                    {
                        _CSVResultFile = null;//dispose
                        return Tuple.Create(false, $"Some Entrace points are not valid (Entrance Points Should be a fraction/number). Please check again.<br/> Operation aborted.");
                    }

                    examResults = new ExamResult
                    {
                        Subject_ID = uploadResult.Subject,
                        Semester = uploadResult.Semester,
                        ExamRollNo = _ExamRollNo,
                        ExternalMarks = (int)Math.Round(_ExternalMarks, MidpointRounding.AwayFromZero),
                        Programme = uploadResult.Programme
                    };
                    isExisting = _ResultDB.CheckRecordExists(examResults);
                    if (isExisting)
                    {
                        ScriptToUpload.Append(_ResultDB.GetUploadExamResultTSQL(examResults));
                        uploadcnt++;
                    }
                    else
                    {
                        if (_ResultDB.HasSubjectAssigned(examResults))
                        {
                            ScriptToUpload.Append(_ResultDB.GetInsertExamResultTSQL(examResults));
                            insertcnt++;
                        }
                    }
                    ScriptToUpload.Append(Environment.NewLine);
                    Counter++;

                    if (Counter >= 1000)
                    {
                        ScriptToUploadList.Add(ScriptToUpload.ToString());
                        ScriptToUpload.Clear();
                        Counter = 0;
                    }
                }
                if (Counter > 0)
                {
                    ScriptToUploadList.Add(ScriptToUpload.ToString());
                }
            }
            #endregion


            _CSVResultFile = null;//dispose
            int result = 0;
            using (var ts = new TransactionScope())
            {
                foreach (var itemTSQL in ScriptToUploadList)
                {
                    result += _ResultDB.UploadExamResult(itemTSQL);
                }
                ts.Complete();
            }
            examResults = null;//despose
            return Tuple.Create(result > 0, result > 0 ? $"{uploadcnt} Results Uploaded and {insertcnt} inserted Successfully" : "Failed to upload result. Please check details and try again.");
        }

        /// <summary>
        /// right now working good for ug and IG only
        /// </summary>
        /// <param name="printProgramme"></param>
        /// <param name="Student_ID"></param>
        /// <param name="UptoSemester"></param>
        /// <param name="PassPercentage"></param>
        /// <param name="validateByCoreOnly"></param>
        /// <returns></returns>
        public ResponseData PassPercentage(PrintProgramme printProgramme, Guid Student_ID, short UptoSemester, decimal PassPercentage, bool validateByCoreOnly)
        {
            var std_result = GetResult(printProgramme, null, Student_ID, true);
            if (std_result == null || std_result.SubjectResults.IsNullOrEmpty())
                return new ResponseData() { ErrorMessage = $"Failed to pass the {PassPercentage}% of core subjects.", ResponseObject = decimal.Zero };

            bool ExcludeFromEligibilityCheck = new StudentDB().ExcludeFromEligibilityCheck(std_result.CUSRegistrationNo, UptoSemester >= 3 ? (short)(UptoSemester + 2) : UptoSemester);

            if (ExcludeFromEligibilityCheck)
                return new ResponseData() { IsSuccess = true, SuccessMessage = "Passed", ResponseObject = decimal.Zero };

            var parameter = new Parameters() { Filters = new List<SearchFilter>() { new SearchFilter() { Column = "Course_ID", Value = std_result.Course_ID.ToString(), TableAlias = "CombinationSetting" }, new SearchFilter() { Column = "Batch", Value = std_result.SemesterBatch == 0 ? std_result.Batch.ToString() : std_result.SemesterBatch.ToString(), TableAlias = "" }, new SearchFilter() { Operator = SQLOperator.LessThanEqualTo, Column = "Semester", Value = UptoSemester.ToString(), TableAlias = "" } }, PageInfo = new Paging() { DefaultOrderByColumn = "Semester", PageNumber = -1, PageSize = -1 } };
            var settings = new CombinationSettingManager().GetCombinationSettings(parameter)?.Where(x => x.CombinationSettingStructure != null)?.ToList();
            if (settings.IsNullOrEmpty()) return new ResponseData() { ErrorMessage = "Missing settings. Please contact cluster university I.T Section." };

            decimal percentage, totalsubjects, subjectsPassed;
            std_result.SubjectResults = std_result.SubjectResults.Where(x => x.Semester <= UptoSemester).ToList();

            if (validateByCoreOnly)
            {
                totalsubjects = settings.Sum(x => x.CombinationSettingStructure.Core_Count + x.CombinationSettingStructure.MIL_Count);
                decimal numberofcoresubjects = std_result.SubjectResults.Where(x => (x.SubjectType == SubjectType.Core || x.SubjectType == SubjectType.MIL)).Count();
                if (totalsubjects < numberofcoresubjects)
                    totalsubjects = numberofcoresubjects;

                subjectsPassed = std_result.SubjectResults.Where(x => (x.SubjectType == SubjectType.Core || x.SubjectType == SubjectType.MIL) && x.OverallResultStatus == ResultStatus.P && x.ResultNotification_ID != null && x.ExamForm_ID != null)?.Count() ?? 0;
                percentage = (subjectsPassed / totalsubjects) * 100;
            }
            else
            {
                subjectsPassed = std_result.SubjectResults.Where(x => x.OverallResultStatus == ResultStatus.P && x.ResultNotification_ID != null && x.ExamForm_ID != null)?.Count() ?? 0;
                totalsubjects = settings.Sum(x => x.CombinationSettingStructure.TotalResultNumberOfSubjects);
                decimal numberofcoresubjects = std_result.SubjectResults.Count();
                if (totalsubjects < numberofcoresubjects)
                    totalsubjects = numberofcoresubjects;
                percentage = (subjectsPassed / totalsubjects) * 100;
            }
            if (percentage > 100) return new ResponseData() { ErrorMessage = "Missing settings. Please contact cluster university I.T Section." };

            if (percentage < PassPercentage)
                return new ResponseData() { ErrorMessage = $"Not eligible. You have not passed the {PassPercentage}% of Subjects in Previous Semesters.", ResponseObject = percentage.ToString("F2") };
            else
                return new ResponseData() { IsSuccess = true, SuccessMessage = "Passed", ResponseObject = percentage.ToString("F2") };
        }

        public bool GetInternalResultStatus(Guid student_ID, Guid subject_ID, short semester)
        {
            return new ResultDB().GetInternalResultStatus(student_ID, subject_ID, semester);
        }



        #endregion
    }
}
