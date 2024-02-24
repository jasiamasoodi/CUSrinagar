using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using System.Data.SqlClient;
using CUSrinagar.DataManagers;
using System.Web.Mvc;
using Terex;
using System.Transactions;
using System.Web.UI.WebControls;
using System.Reflection;
using System.Data;
using GeneralModels;
using CUSrinagar.Models;

namespace CUSrinagar.BusinessManagers
{
    public class DegreeCertificateManager
    {
        public List<DegreeCertificate> List(Parameters parameter, PrintProgramme printProgramme)
        {
            var _list = new DegreeCertificateDB().List(parameter, printProgramme)?.OrderBy(x => x.OtherUnivMigrationReceivedOn).ThenBy(x => x.ExamRollNumber)?.ToList();
            if(_list == null && printProgramme==PrintProgramme.PG) _list = new DegreeCertificateDB().List(parameter, PrintProgramme.IH)?.OrderBy(x => x.OtherUnivMigrationReceivedOn).ThenBy(x => x.ExamRollNumber)?.ToList();
            var _list_type2 = _list?.Where(x => x.DegreeCourseTitle?.ToLower()?.Contains("education") ?? false)?.ToList();
            if (_list_type2.IsNotNullOrEmpty())
            {
                foreach (var item in _list_type2)
                {
                    item.CGPA_Type2 = new TranscriptDB().GetCGPA(item.Student_ID,4)?.FirstOrDefault(x => x.SGPAType == SGPAType.Type2)?.CGPA ?? 0;
                    item.CGPA_Type1_Grade = GetGrade(item.CGPA_Type1, 1);
                    item.CGPA_Type2_Grade = GetGrade(item.CGPA_Type2.Value, 1);
                }
            }
            return _list;
        }
        public List<DegreeCertificate> ListEng(Parameters parameter, PrintProgramme printProgramme)
        {
            var _list = new DegreeCertificateDB().ListEng(parameter, printProgramme)?.OrderBy(x => x.OtherUnivMigrationReceivedOn).ThenBy(x => x.ExamRollNumber)?.ToList();
            var _list_type2 = _list?.Where(x => x.DegreeCourseTitle?.ToLower()?.Contains("education") ?? false)?.ToList();
            if (_list_type2.IsNotNullOrEmpty())
            {
                foreach (var item in _list_type2)
                {
                    item.CGPA_Type2 = new TranscriptDB().GetCGPA(item.Student_ID,8)?.FirstOrDefault(x => x.SGPAType == SGPAType.Type2)?.CGPA ?? 0;
                    item.CGPA_Type1_Grade = GetGrade(item.CGPA_Type1, 1);
                    item.CGPA_Type2_Grade = GetGrade(item.CGPA_Type2.Value, 1);
                }
            }
            return _list;
        }


        private string GetGrade(decimal cGPA, int GradeSystem)
        {
            return new TranscriptDB().GetGrade(cGPA, GradeSystem);
        }

        public ResponseData Save(List<Guid> Student_ID,int SemesterFrom, int SemesterTo, string CommonDispatchNumber, int StartingIssueNumber, PrintProgramme PrintProgramme)
        {
            if (string.IsNullOrEmpty(CommonDispatchNumber) || Student_ID.IsNullOrEmpty() || PrintProgramme == 0 || StartingIssueNumber <= 0) return new ResponseData() { ErrorMessage = "Invalid input parameters" };
            var response = new ResponseData() { };
            var settingdb = new DegreeCertificateDB();
            Parameters param = new Parameters() { PageInfo = new Paging() { DefaultOrderByColumn = "DegreeCertificate_ID" } };
            List<DegreeCertificate> dc_list = null;
            foreach (var s in Student_ID)
            {
                List<MSCGPA> mSCGPA = new TranscriptDB().GetCGPA(s,SemesterTo);
                #region Validation
                if (mSCGPA.IsNullOrEmpty())
                {
                    response.ErrorMessage += "Transcript not generated";
                    continue;
                }
                if (mSCGPA.Any(x => !x.IsValidated))
                {
                    response.ErrorMessage += "Transcript not validated";
                    continue;
                }
                if (mSCGPA.Any(x => !x.IsPrinted))
                {
                    response.ErrorMessage += "Transcript not printed";
                    continue;
                }
                if (mSCGPA.Any(x => !x.IsHandedOver))
                {
                    response.ErrorMessage += "Transcript not handed Over";
                    continue;
                }

                param.Filters = new List<SearchFilter>() { new SearchFilter() { Column = "Student_ID", TableAlias = "DC", Operator = SQLOperator.EqualTo, Value = s.ToString() } };
                dc_list = settingdb.List(param, PrintProgramme);
                if (dc_list.IsNotNullOrEmpty())
                {
                    response.ErrorMessage += $"Degree certficate already generated.";
                    continue;
                }

                param.Filters = new List<SearchFilter>() { new SearchFilter() { Column = "IssueNumber", Operator = SQLOperator.Contains, Value = (StartingIssueNumber).ToString() } };
                dc_list = settingdb.List(param, PrintProgramme);
                if (dc_list.IsNotNullOrEmpty())
                {
                    response.ErrorMessage += $"Degree certficate already generated with this issue number={StartingIssueNumber} to {dc_list.First().CUSRegistrationNo}.";
                    continue;
                }
                #endregion

                DegreeCertificate model = new DegreeCertificate()
                {
                    DegreeCertificate_ID = Guid.NewGuid(),
                    Student_ID = s,
                    DispatchNumber = CommonDispatchNumber,
                    IssueNumber = StartingIssueNumber,
                    CreatedBy_ID = AppUserHelper.User_ID,
                    SemesterTo=SemesterTo,
                };

                if (settingdb.Save(model) == 1)
                {
                    StartingIssueNumber++;
                    response.NumberOfRecordsEffected++;
                }
            }
            if (response.NumberOfRecordsEffected > 0) { response.SuccessMessage = "Records saved sucessfully."; response.IsSuccess = true; }
            return response;
        }
        public Task<ResponseData> SaveAsync(List<string> Student_ID,int SemesterFrom, int SemesterTo, int Batch, PrintProgramme PrintProgramme, Guid User_ID)
        {
            return new TaskFactory().StartNew(() => Save(Student_ID, SemesterFrom, SemesterTo, Batch, PrintProgramme, User_ID));
        }
        public Task<ResponseData> SaveAsyncEng(List<string> Student_ID, int Batch, PrintProgramme PrintProgramme, Guid User_ID)
        {
            return new TaskFactory().StartNew(() => SaveEng(Student_ID, Batch, PrintProgramme, User_ID));
        }

        public ResponseData SaveEng(List<string> Student_ID, int Batch, PrintProgramme PrintProgramme, Guid User_ID)
        {
            string CommonDispatchNumber = $"CUS/EXM/DC/{PrintProgramme}/{Batch}";
            int StartingIssueNumber = new DegreeCertificateDB().GetMaxIssueNumber() + 1;
            if (string.IsNullOrEmpty(CommonDispatchNumber) || Student_ID.IsNullOrEmpty() || PrintProgramme == 0 || StartingIssueNumber <= 0) return new ResponseData() { ErrorMessage = "Invalid input parameters" };
            var response = new ResponseData() { };
            var settingdb = new DegreeCertificateDB();
            Parameters param = new Parameters() { PageInfo = new Paging() { DefaultOrderByColumn = "DegreeCertificate_ID" } };
            List<DegreeCertificate> dc_list = null;
            foreach (var s in Student_ID)
            {
                //List<MSCGPA> mSCGPA = new TranscriptDB().GetCGPA(Guid.Parse(s));
                //#region Validation
                //if (mSCGPA.IsNullOrEmpty())
                //{
                //    response.ErrorMessage += "Transcript not generated";
                //    continue;
                //}
                //if (mSCGPA.Any(x => !x.IsValidated))
                //{
                //    response.ErrorMessage += "Transcript not validated";
                //    continue;
                //}
                //if (mSCGPA.Any(x => !x.IsPrinted))
                //{
                //    response.ErrorMessage += "Transcript not printed";
                //    continue;
                //}
                //if (mSCGPA.Any(x => !x.IsHandedOver))
                //{
                //    response.ErrorMessage += "Transcript not handed Over";
                //    continue;
                //}

                param.Filters = new List<SearchFilter>() { new SearchFilter() { Column = "Student_ID", TableAlias = "DC", Operator = SQLOperator.EqualTo, Value = s.ToString() } };
                dc_list = settingdb.ListEng(param, PrintProgramme);
                if (dc_list.IsNotNullOrEmpty())
                {
                    response.ErrorMessage += $"Degree certficate already generated.";
                    continue;
                }

                param.Filters = new List<SearchFilter>() { new SearchFilter() { Column = "IssueNumber", Operator = SQLOperator.Contains, Value = (StartingIssueNumber).ToString() } };
                dc_list = settingdb.ListEng(param, PrintProgramme);
                if (dc_list.IsNotNullOrEmpty())
                {
                    response.ErrorMessage += $"Degree certficate already generated with this issue number={StartingIssueNumber} to {dc_list.First().CUSRegistrationNo}.";
                    continue;
                }
                //#endregion

                DegreeCertificate model = new DegreeCertificate()
                {
                    DegreeCertificate_ID = Guid.NewGuid(),
                    Student_ID = Guid.Parse(s),
                    DispatchNumber = CommonDispatchNumber,
                    IssueNumber = StartingIssueNumber,
                    CreatedBy_ID = User_ID,
                  };

                if (settingdb.Save(model) == 1)
                {
                    StartingIssueNumber++;
                    response.NumberOfRecordsEffected++;
                }
            }
            if (response.NumberOfRecordsEffected > 0) { response.SuccessMessage = response.NumberOfRecordsEffected + " Records saved sucessfully."; response.IsSuccess = true; }
            return response;
        }

        public ResponseData Save(List<string> Student_ID, int SemesterFrom, int SemesterTo, int Batch, PrintProgramme PrintProgramme, Guid User_ID)
        {
            string CommonDispatchNumber = $"CUS/EXM/DC/{PrintProgramme}/{Batch}";
            int StartingIssueNumber = new DegreeCertificateDB().GetMaxIssueNumber() + 1;
            if (string.IsNullOrEmpty(CommonDispatchNumber) || Student_ID.IsNullOrEmpty() || PrintProgramme == 0 || StartingIssueNumber <= 0) return new ResponseData() { ErrorMessage = "Invalid input parameters" };
            var response = new ResponseData() { };
            var settingdb = new DegreeCertificateDB();
            Parameters param = new Parameters() { PageInfo = new Paging() { DefaultOrderByColumn = "DegreeCertificate_ID" } };
            List<DegreeCertificate> dc_list = null;
            foreach (var s in Student_ID)
            {
                List<MSCGPA> mSCGPA = new TranscriptDB().GetCGPA(Guid.Parse(s),SemesterTo);
                #region Validation
                if (mSCGPA.IsNullOrEmpty())
                {
                    response.ErrorMessage += "Transcript not generated";
                    continue;
                }
                if (mSCGPA.Any(x => !x.IsValidated))
                {
                    response.ErrorMessage += "Transcript not validated";
                    continue;
                }
                if (mSCGPA.Any(x => !x.IsPrinted))
                {
                    response.ErrorMessage += "Transcript not printed";
                    continue;
                }
                if (mSCGPA.Any(x => !x.IsHandedOver))
                {
                    response.ErrorMessage += "Transcript not handed Over";
                    continue;
                }

                param.Filters = new List<SearchFilter>() { new SearchFilter() { Column = "Student_ID", TableAlias = "DC", Operator = SQLOperator.EqualTo, Value = s.ToString() } ,
                                                                                new SearchFilter() { Column = "SemesterTo", TableAlias = "DC", Operator = SQLOperator.EqualTo, Value = SemesterTo.ToString() }};
                dc_list = settingdb.List(param, PrintProgramme);
                if (dc_list.IsNotNullOrEmpty())
                {
                    response.ErrorMessage += $"Degree certficate already generated.";
                    continue;
                }

                param.Filters = new List<SearchFilter>() { new SearchFilter() { Column = "IssueNumber", Operator = SQLOperator.Contains, Value = (StartingIssueNumber).ToString() } };
                dc_list = settingdb.List(param, PrintProgramme);
                if (dc_list.IsNotNullOrEmpty())
                {
                    response.ErrorMessage += $"Degree certficate already generated with this issue number={StartingIssueNumber} to {dc_list.First().CUSRegistrationNo}.";
                    continue;
                }
                #endregion

                DegreeCertificate model = new DegreeCertificate()
                {
                    DegreeCertificate_ID = Guid.NewGuid(),
                    Student_ID = Guid.Parse(s),
                    DispatchNumber = CommonDispatchNumber,
                    IssueNumber = StartingIssueNumber,
                    CreatedBy_ID = User_ID,
                    SemesterTo = SemesterTo
                };

                if (settingdb.Save(model) == 1)
                {
                    StartingIssueNumber++;
                    response.NumberOfRecordsEffected++;
                }
            }
            if (response.NumberOfRecordsEffected > 0) { response.SuccessMessage = response.NumberOfRecordsEffected + " Records saved sucessfully."; response.IsSuccess = true; }
            return response;
        }


        public ResponseData Update(DegreeCertificate setting, PrintProgramme printProgramme)
        {
            var settingdb = new DegreeCertificateDB();
            var response = new ResponseData() { ErrorMessage = "Failed to save record" };
            response.NumberOfRecordsEffected = settingdb.Update(setting);
            if (response.NumberOfRecordsEffected > 0)
            {
                response.SuccessMessage = "Record saved successfully";
                response.IsSuccess = true;
                response.ErrorMessage = "";
            }
            return response;
        }
        public ResponseData Delete(Guid DegreeCertificate_ID)
        {
            var response = new ResponseData() { ErrorMessage = "Failed to save record" };
            response.NumberOfRecordsEffected = new DegreeCertificateDB().Delete(DegreeCertificate_ID);
            if (response.NumberOfRecordsEffected > 0)
            {
                response.SuccessMessage = "Record deleted successfully";
                response.IsSuccess = true;
            }
            return response;
        }
        public ResponseData Validated(Guid DegreeCertificate_ID)
        {
            var response = new ResponseData();
            response.NumberOfRecordsEffected = new DegreeCertificateDB().Validated(DegreeCertificate_ID);
            response.IsSuccess = response.NumberOfRecordsEffected > 0;
            return response;
        }
        public ResponseData Printed(Guid DegreeCertificate_ID)
        {
            var response = new ResponseData();
            response.NumberOfRecordsEffected = new DegreeCertificateDB().Printed(DegreeCertificate_ID);
            response.IsSuccess = response.NumberOfRecordsEffected > 0;
            return response;
        }

        public ResponseData Duplicate(Guid DegreeCertificate_ID, string DuplicateLbl)
        {
            var response = new ResponseData();
            response.NumberOfRecordsEffected = new DegreeCertificateDB().Duplicate(DegreeCertificate_ID, DuplicateLbl);
            response.IsSuccess = response.NumberOfRecordsEffected > 0;
            return response;
        }
        public ResponseData HandedOverOn(Guid DegreeCertificate_ID)
        {
            var response = new ResponseData();
            response.NumberOfRecordsEffected = new DegreeCertificateDB().HandedOverOn(DegreeCertificate_ID);
            response.IsSuccess = response.NumberOfRecordsEffected > 0;
            return response;
        }

        public ResponseData MarkPrintedAll(List<Guid> DegreeCertificate_IDs)
        {
            var response = new ResponseData();
            if (DegreeCertificate_IDs.IsNotNullOrEmpty())
            {
                response.NumberOfRecordsEffected = new DegreeCertificateDB().MarkPrintedAll(DegreeCertificate_IDs);
                response.IsSuccess = response.NumberOfRecordsEffected > 0;
            }
            else
            {
                response.NumberOfRecordsEffected = 0;
                response.IsSuccess = true;
            }
            return response;
        }

        public ResponseData MarkHandedOverAll(List<Guid> DegreeCertificate_IDs)
        {
            var response = new ResponseData();
            if (DegreeCertificate_IDs.IsNotNullOrEmpty())
            {
                response.NumberOfRecordsEffected = new DegreeCertificateDB().MarkHandedOverAll(DegreeCertificate_IDs);
                response.IsSuccess = response.NumberOfRecordsEffected > 0;
            }
            else
            {
                response.NumberOfRecordsEffected = 0;
                response.IsSuccess = true;
            }
            return response;
        }

        public ResponseData MarkAllGood(List<Guid> DegreeCertificate_IDs)
        {
            var response = new ResponseData();
            if (DegreeCertificate_IDs.IsNotNullOrEmpty())
            {
                response.NumberOfRecordsEffected = new DegreeCertificateDB().MarkAllGood(DegreeCertificate_IDs);
                response.IsSuccess = response.NumberOfRecordsEffected > 0;
            }
            else
            {
                response.NumberOfRecordsEffected = 0;
                response.IsSuccess = true;
            }
            return response;
        }
    }
}
