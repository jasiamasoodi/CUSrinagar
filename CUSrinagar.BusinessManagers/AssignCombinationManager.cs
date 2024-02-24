using CUSrinagar.DataManagers;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using CUSrinagar.Models;
using GeneralModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace CUSrinagar.BusinessManagers
{
    public class AssignCombinationManager
    {
        #region SaveStudentzSubjects
        public ResponseData SaveUpdateStudentCombination(PrintProgramme printProgramme, Guid student_ID, short _SetCombinationForSemester, List<ADMSubjectMaster> subjectsChoosen, short SemesterBatch, List<ADMSubjectMaster> AdditionalSubjectsChoosen = null)
        {
            var STDINFO = new StudentManager().GetStudentWithCombination(student_ID, printProgramme, true);
            var _ValidateByStudent = ValidateByStudent(STDINFO, _SetCombinationForSemester);
            if (!_ValidateByStudent.IsSuccess) return _ValidateByStudent;

            Guid Course_ID = STDINFO.SelectedCombinations.FirstOrDefault(x => x.Semester == _SetCombinationForSemester)?.Course_ID ?? STDINFO.SelectedCombinations.First().Course_ID;
            var _SC = STDINFO.SelectedCombinations.FirstOrDefault(x => x.Semester == _SetCombinationForSemester);
            var _CS = new CombinationSettingManager().GetCombinationSetting(Course_ID, _SetCombinationForSemester, SemesterBatch);


            var _ValidateByCombinationSetting = ValidateByCombinationSetting(printProgramme, STDINFO, _CS, subjectsChoosen, _SC);
            if (!_ValidateByCombinationSetting.IsSuccess) return _ValidateByCombinationSetting;

            ADMCombinationMaster aDMCombination = GetSetCombinationMaster(new ADMCombinationMaster()
            {
                College_ID = STDINFO.AcceptCollege_ID == null ? AppUserHelper.College_ID.Value : STDINFO.AcceptCollege_ID.Value,
                CombinationCode = "",//STDINFO.SelectedCombinations.FirstOrDefault(x => x.Semester == (short)(_SetCombinationForSemester==1?1: _SetCombinationForSemester - 1))?.CourseCode,
                Course_ID = Course_ID,
                Semester = _SetCombinationForSemester,
                SubjectsDetails = (List<ADMSubjectMaster>)_ValidateByCombinationSetting.ResponseObject
            });
            if (aDMCombination == null)
                return new ResponseData() { ErrorMessage = "An error occurred while saving combination. Please try again later." };

            ResponseData response = SaveUpdateStudentCombination(STDINFO, _SetCombinationForSemester, aDMCombination.Combination_ID, printProgramme, SemesterBatch);
            if (!response.IsSuccess)
                return response;


            if ((STDINFO.CurrentSemesterOrYear < _SetCombinationForSemester && AppUserHelper.AppUsercompact.UserRoles.Any(x => x != AppRoles.Student))
                || (STDINFO.CurrentSemesterOrYear < _SetCombinationForSemester && _SetCombinationForSemester % 2 == 0))
                response.NumberOfRecordsEffected += new StudentDB().UpdateCurrentSemOrYear(STDINFO.Student_ID, _SetCombinationForSemester, printProgramme);

            if (_CS.CombinationSettingStructure.AdditionalCourseCount > 0)
                SaveAdditionalSubject(printProgramme, STDINFO, _CS, AdditionalSubjectsChoosen, _SetCombinationForSemester, aDMCombination, SemesterBatch);

            if (response.NumberOfRecordsEffected > 0)
                return new ResponseData() { SuccessMessage = "Subjects saved successfully.", NumberOfRecordsEffected = 1, IsSuccess = true, ErrorMessage = _ValidateByCombinationSetting.ErrorMessage };
            //, ResponseObject = new StudentManager().GetStudentWithCombination(student_ID, printProgramme, false) };
            else
                return new ResponseData() { ErrorMessage = "Unable to assign combination. Please try again later" };
        }
        public ResponseData SaveUpdateStudentCombinationPG(Guid Course_Id, PrintProgramme printProgramme, Guid student_ID, short _SetCombinationForSemester, List<ADMSubjectMaster> subjectsChoosen, short SemesterBatch, Decimal? OfflinePaymentAmount, DateTime? BankPaymentDate, List<ADMSubjectMaster> AdditionalSubjectsChoosen = null)
        {
            var STDINFO = new StudentManager().GetStudentWithCombination(student_ID, printProgramme, true);
            if (_SetCombinationForSemester > 1)
            {
                var _ValidateByStudent = ValidateByStudent(STDINFO, _SetCombinationForSemester);
                if (!_ValidateByStudent.IsSuccess) return _ValidateByStudent;
            }

            Guid Course_ID;
            if (_SetCombinationForSemester != 1)
            { Course_ID = STDINFO.SelectedCombinations.FirstOrDefault(x => x.Semester == _SetCombinationForSemester)?.Course_ID ?? STDINFO.SelectedCombinations.First().Course_ID; }
            else { Course_ID = new SubjectsManager().GetSubjectsCourse(subjectsChoosen.First().Subject_ID); }
            var _SC = STDINFO.SelectedCombinations?.FirstOrDefault(x => x.Semester == _SetCombinationForSemester);
            var _CS = new CombinationSettingManager().GetCombinationSetting(Course_ID, _SetCombinationForSemester, SemesterBatch);


            var _ValidateByCombinationSetting = ValidateByCombinationSetting(printProgramme, STDINFO, _CS, subjectsChoosen, _SC);
            if (!_ValidateByCombinationSetting.IsSuccess) return _ValidateByCombinationSetting;

            ADMCombinationMaster aDMCombination = GetSetCombinationMaster(new ADMCombinationMaster()
            {
                College_ID = STDINFO.AcceptCollege_ID == null ? AppUserHelper.College_ID.Value : STDINFO.AcceptCollege_ID.Value,
                CombinationCode = "",//STDINFO.SelectedCombinations.FirstOrDefault(x => x.Semester == (short)(_SetCombinationForSemester==1?1: _SetCombinationForSemester - 1))?.CourseCode,
                Course_ID = Course_ID,
                Semester = _SetCombinationForSemester,
                SubjectsDetails = (List<ADMSubjectMaster>)_ValidateByCombinationSetting.ResponseObject
            });
            if (aDMCombination == null)
                return new ResponseData() { ErrorMessage = "An error occurred while saving combination. Please try again later." };

            ResponseData response = SaveUpdateStudentCombination(STDINFO, _SetCombinationForSemester, aDMCombination.Combination_ID, printProgramme, SemesterBatch);
            if (!response.IsSuccess)
                return response;


            if ((STDINFO.CurrentSemesterOrYear < _SetCombinationForSemester && AppUserHelper.AppUsercompact.UserRoles.Any(x => x != AppRoles.Student))
                || (STDINFO.CurrentSemesterOrYear < _SetCombinationForSemester && _SetCombinationForSemester % 2 == 0))
                response.NumberOfRecordsEffected += new StudentDB().UpdateCurrentSemOrYear(STDINFO.Student_ID, _SetCombinationForSemester, printProgramme);

            if (_CS.CombinationSettingStructure.AdditionalCourseCount > 0)
                SaveAdditionalSubject(printProgramme, STDINFO, _CS, AdditionalSubjectsChoosen, _SetCombinationForSemester, aDMCombination, SemesterBatch);


            var Payment = new PaymentManager().GetPaymentDetails(STDINFO.Student_ID, PaymentModuleType.SemesterAdmission, printProgramme, _SetCombinationForSemester)?.FirstOrDefault();
            if (Payment == null)
            {
                PaymentDetails _payment = new PaymentDetails()
                {
                    Payment_ID = Guid.NewGuid(),
                    PaymentType = PaymentType.Offline,
                    Semester = 1,
                    AuthStatus = "0300",
                    BankID = "CUS",
                    BankMerchantID = "CUS",
                    BankReferenceNo = "CUS",
                    CurrencyName = "INR",
                    TxnReferenceNo = STDINFO.StudentFormNo,
                    TxnDate = BankPaymentDate.Value,
                    Entity_ID = STDINFO.Student_ID,
                    ModuleType = PaymentModuleType.SemesterAdmission,
                    TxnAmount = OfflinePaymentAmount.Value
                };
                response.NumberOfRecordsEffected += new PaymentDB().SavePaymentDetails(_payment, printProgramme);
            }
            else if (OfflinePaymentAmount.HasValue && BankPaymentDate.HasValue && (Payment.PaymentType == PaymentType.Offline && Payment.TxnAmount != OfflinePaymentAmount.Value || Payment.TxnDate.Date != BankPaymentDate.Value.Date))
            {
                Payment.TxnDate = BankPaymentDate.Value;
                Payment.TxnAmount = OfflinePaymentAmount.Value;
                response.NumberOfRecordsEffected += new PaymentDB().Update(Payment, printProgramme);
            }

            if (!response.IsSuccess)
                return response;

            if (response.NumberOfRecordsEffected > 0)
            {
                if (STDINFO.FormStatus != FormStatus.Selected || STDINFO.CurrentSemesterOrYear <= 0 || !STDINFO.AcceptCollege_ID.HasValue)
                {
                    STDINFO.FormStatus = FormStatus.Selected;
                    STDINFO.AcceptCollege_ID = AppUserHelper.College_ID;
                    STDINFO.CurrentSemesterOrYear = 1;
                    STDINFO.AcceptedBy_ID = AppUserHelper.User_ID;
                    response.NumberOfRecordsEffected += new StudentManager().UpdateStudentCombinationInfo(STDINFO, printProgramme);
                }
                else
                    response.NumberOfRecordsEffected += 1;
            }
            if (response.NumberOfRecordsEffected > 0)
                return new ResponseData() { SuccessMessage = "Subjects saved successfully.", NumberOfRecordsEffected = 1, IsSuccess = true, ErrorMessage = _ValidateByCombinationSetting.ErrorMessage };
            //, ResponseObject = new StudentManager().GetStudentWithCombination(student_ID, printProgramme, false) };
            else
                return new ResponseData() { ErrorMessage = "Unable to assign combination. Please try again later" };
        }
        public ResponseData SaveUpdateStudentCombinationNEP(PrintProgramme printProgramme, Guid student_ID, short _SetCombinationForSemester, List<ADMSubjectMaster> subjectsChoosen, short SemesterBatch, Guid Course_ID, Decimal? OfflinePaymentAmount, DateTime? BankPaymentDate, List<ADMSubjectMaster> AdditionalSubjectsChoosen = null)
        {
            var STDINFO = new StudentManager().GetStudentWithCombination(student_ID, printProgramme, true);
            var _SC = STDINFO.SelectedCombinations?.FirstOrDefault(x => x.Semester == _SetCombinationForSemester);

            var _CS = new CombinationSettingManager().GetCombinationSetting(Course_ID, _SetCombinationForSemester, SemesterBatch);


            var _ValidateByCombinationSetting = ValidateByCombinationSettingNEP(printProgramme, Course_ID, STDINFO, _CS, subjectsChoosen, _SC);
            if (!_ValidateByCombinationSetting.IsSuccess) return _ValidateByCombinationSetting;

            ADMCombinationMaster aDMCombination = GetSetCombinationMaster(new ADMCombinationMaster()
            {
                College_ID = AppUserHelper.College_ID.Value,
                CombinationCode = "",//STDINFO.SelectedCombinations.FirstOrDefault(x => x.Semester == (short)(_SetCombinationForSemester==1?1: _SetCombinationForSemester - 1))?.CourseCode,
                Course_ID = Course_ID,
                Semester = _SetCombinationForSemester,
                SubjectsDetails = (List<ADMSubjectMaster>)_ValidateByCombinationSetting.ResponseObject
            });
            if (aDMCombination == null)
                return new ResponseData() { ErrorMessage = "An error occurred while saving combination. Please try again later." };

            ResponseData response = SaveUpdateStudentCombination(STDINFO, _SetCombinationForSemester, aDMCombination.Combination_ID, printProgramme, SemesterBatch);
            var Payment = new PaymentManager().GetPaymentDetails(STDINFO.Student_ID, PaymentModuleType.SemesterAdmission, printProgramme, _SetCombinationForSemester)?.FirstOrDefault();
            if (Payment == null)
            {
                PaymentDetails _payment = new PaymentDetails()
                {
                    Payment_ID = Guid.NewGuid(),
                    PaymentType = PaymentType.Offline,
                    Semester = 1,
                    AuthStatus = "0300",
                    BankID = "CUS",
                    BankMerchantID = "CUS",
                    BankReferenceNo = "CUS",
                    CurrencyName = "INR",
                    TxnReferenceNo = STDINFO.StudentFormNo,
                    TxnDate = BankPaymentDate.Value,
                    Entity_ID = STDINFO.Student_ID,
                    ModuleType = PaymentModuleType.SemesterAdmission,
                    TxnAmount = OfflinePaymentAmount.Value
                };
                response.NumberOfRecordsEffected += new PaymentDB().SavePaymentDetails(_payment, printProgramme);
            }
            else if (OfflinePaymentAmount.HasValue && BankPaymentDate.HasValue && (Payment.PaymentType == PaymentType.Offline && Payment.TxnAmount != OfflinePaymentAmount.Value || Payment.TxnDate.Date != BankPaymentDate.Value.Date))
            {
                Payment.TxnDate = BankPaymentDate.Value;
                Payment.TxnAmount = OfflinePaymentAmount.Value;
                response.NumberOfRecordsEffected += new PaymentDB().Update(Payment, printProgramme);
            }

            if (!response.IsSuccess)
                return response;

            if (response.NumberOfRecordsEffected > 0)
            {
                if (STDINFO.FormStatus != FormStatus.Selected || STDINFO.CurrentSemesterOrYear <= 0 || !STDINFO.AcceptCollege_ID.HasValue)
                {
                    STDINFO.FormStatus = FormStatus.Selected;
                    STDINFO.AcceptCollege_ID = AppUserHelper.College_ID;
                    STDINFO.CurrentSemesterOrYear = 1;
                    STDINFO.AcceptedBy_ID = AppUserHelper.User_ID;
                    response.NumberOfRecordsEffected += new StudentManager().UpdateStudentCombinationInfo(STDINFO, printProgramme);
                }
                else
                    response.NumberOfRecordsEffected += 1;
            }
            //if ((STDINFO.CurrentSemesterOrYear < _SetCombinationForSemester && AppUserHelper.AppUsercompact.UserRoles.Any(x => x != AppRoles.Student))
            //    || (STDINFO.CurrentSemesterOrYear < _SetCombinationForSemester && _SetCombinationForSemester % 2 == 0))
            //    response.NumberOfRecordsEffected += new StudentDB().UpdateCurrentSemOrYear(STDINFO.Student_ID, _SetCombinationForSemester, printProgramme);

            if (_CS.CombinationSettingStructure.AdditionalCourseCount > 0)
                SaveAdditionalSubject(printProgramme, STDINFO, _CS, AdditionalSubjectsChoosen, _SetCombinationForSemester, aDMCombination, SemesterBatch);

            if (response.NumberOfRecordsEffected > 0)
                return new ResponseData() { SuccessMessage = "Subjects saved successfully.", NumberOfRecordsEffected = 1, IsSuccess = true, ErrorMessage = _ValidateByCombinationSetting.ErrorMessage };
            //, ResponseObject = new StudentManager().GetStudentWithCombination(student_ID, printProgramme, false) };
            else
                return new ResponseData() { ErrorMessage = "Unable to assign combination. Please try again later" };
        }


        public ResponseData SaveUpdateStudentCombinationNEPCUET(PrintProgramme printProgramme, Guid student_ID, short _SetCombinationForSemester, List<ADMSubjectMaster> subjectsChoosen, short SemesterBatch, Guid Course_ID, Decimal? OfflinePaymentAmount, DateTime? BankPaymentDate, List<ADMSubjectMaster> AdditionalSubjectsChoosen = null)
        {
            var STDINFO = new StudentManager().GetStudentWithCombination(student_ID, printProgramme, true);
            var _SC = STDINFO.SelectedCombinations?.FirstOrDefault(x => x.Semester == _SetCombinationForSemester);

            var _CS = new CombinationSettingManager().GetCombinationSetting(Course_ID, _SetCombinationForSemester, SemesterBatch);


            var _ValidateByCombinationSetting = ValidateByCombinationSettingNEPCUET(printProgramme, Course_ID, STDINFO, _CS, subjectsChoosen, _SC);
            if (!_ValidateByCombinationSetting.IsSuccess) return _ValidateByCombinationSetting;

            ADMCombinationMaster aDMCombination = GetSetCombinationMaster(new ADMCombinationMaster()
            {
                College_ID = AppUserHelper.College_ID.Value,
                CombinationCode = "",//STDINFO.SelectedCombinations.FirstOrDefault(x => x.Semester == (short)(_SetCombinationForSemester==1?1: _SetCombinationForSemester - 1))?.CourseCode,
                Course_ID = Course_ID,
                Semester = _SetCombinationForSemester,
                SubjectsDetails = (List<ADMSubjectMaster>)_ValidateByCombinationSetting.ResponseObject
            });
            if (aDMCombination == null)
                return new ResponseData() { ErrorMessage = "An error occurred while saving combination. Please try again later." };

            ResponseData response = SaveUpdateStudentCombination(STDINFO, _SetCombinationForSemester, aDMCombination.Combination_ID, printProgramme, SemesterBatch);
            var Payment = new PaymentManager().GetPaymentDetails(STDINFO.Student_ID, PaymentModuleType.SemesterAdmission, printProgramme, _SetCombinationForSemester)?.FirstOrDefault();
            if (Payment == null)
            {
                PaymentDetails _payment = new PaymentDetails()
                {
                    Payment_ID = Guid.NewGuid(),
                    PaymentType = PaymentType.Offline,
                    Semester = 1,
                    AuthStatus = "0300",
                    BankID = "CUS",
                    BankMerchantID = "CUS",
                    BankReferenceNo = "CUS",
                    CurrencyName = "INR",
                    TxnReferenceNo = STDINFO.StudentFormNo,
                    TxnDate = BankPaymentDate.Value,
                    Entity_ID = STDINFO.Student_ID,
                    ModuleType = PaymentModuleType.SemesterAdmission,
                    TxnAmount = OfflinePaymentAmount.Value
                };
                response.NumberOfRecordsEffected += new PaymentDB().SavePaymentDetails(_payment, printProgramme);
            }
            else if (OfflinePaymentAmount.HasValue && BankPaymentDate.HasValue && (Payment.PaymentType == PaymentType.Offline && Payment.TxnAmount != OfflinePaymentAmount.Value || Payment.TxnDate.Date != BankPaymentDate.Value.Date))
            {
                Payment.TxnDate = BankPaymentDate.Value;
                Payment.TxnAmount = OfflinePaymentAmount.Value;
                response.NumberOfRecordsEffected += new PaymentDB().Update(Payment, printProgramme);
            }

            if (!response.IsSuccess)
                return response;

            if (response.NumberOfRecordsEffected > 0)
            {
                if (STDINFO.FormStatus != FormStatus.Selected || STDINFO.CurrentSemesterOrYear <= 0 || !STDINFO.AcceptCollege_ID.HasValue)
                {
                    STDINFO.FormStatus = FormStatus.Selected;
                    STDINFO.AcceptCollege_ID = AppUserHelper.College_ID;
                    STDINFO.CurrentSemesterOrYear = 1;
                    STDINFO.AcceptedBy_ID = AppUserHelper.User_ID;
                    response.NumberOfRecordsEffected += new StudentManager().UpdateStudentCombinationInfo(STDINFO, printProgramme);
                }
                else
                    response.NumberOfRecordsEffected += 1;
            }
            //if ((STDINFO.CurrentSemesterOrYear < _SetCombinationForSemester && AppUserHelper.AppUsercompact.UserRoles.Any(x => x != AppRoles.Student))
            //    || (STDINFO.CurrentSemesterOrYear < _SetCombinationForSemester && _SetCombinationForSemester % 2 == 0))
            //    response.NumberOfRecordsEffected += new StudentDB().UpdateCurrentSemOrYear(STDINFO.Student_ID, _SetCombinationForSemester, printProgramme);

            if (_CS.CombinationSettingStructure.AdditionalCourseCount > 0)
                SaveAdditionalSubject(printProgramme, STDINFO, _CS, AdditionalSubjectsChoosen, _SetCombinationForSemester, aDMCombination, SemesterBatch);

            if (response.NumberOfRecordsEffected > 0)
                return new ResponseData() { SuccessMessage = "Subjects saved successfully.", NumberOfRecordsEffected = 1, IsSuccess = true, ErrorMessage = _ValidateByCombinationSetting.ErrorMessage };
            //, ResponseObject = new StudentManager().GetStudentWithCombination(student_ID, printProgramme, false) };
            else
                return new ResponseData() { ErrorMessage = "Unable to assign combination. Please try again later" };
        }


        //public ResponseData BatchUpdateStudentCombination(BatchUpdatePersonalInfo model, PrintProgramme printProgramme)
        //{
        //    ResponseData response = new ResponseData() { IsSuccess = true };
        //    if (model != null && model.Student_ID.IsNotNullOrEmpty() && model.Combination != null)
        //    {
        //        ADMCombinationMaster dbNewComb = new CombinationManager().GetCombinationByID(model.Combination.NewCombination_ID);
        //        if (dbNewComb == null | dbNewComb.College_ID != AppUserHelper.College_ID)
        //            return new ResponseData() { ErrorMessage = "Combination not found" };

        //        foreach (var Student_ID in model.Student_ID)
        //        {
        //            var STDINFO = new StudentManager().GetStudentWithCombination(Student_ID, printProgramme, false, model.Combination.Semester);
        //            var _ValidateByStudent = ValidateByStudent(STDINFO, model.Combination.Semester);
        //            if (!_ValidateByStudent.IsSuccess)
        //            {
        //                response.ErrorMessage += $" #{_ValidateByStudent.ErrorMessage}-{STDINFO.CUSRegistrationNo + "-" + STDINFO.FullName}# ";
        //                continue;
        //            }
        //            var _ValidateByCombinationSetting = ValidateByCombinationSetting(STDINFO, model.Combination.Semester, model.Combination.NewCombination_ID, printProgramme);
        //            if (!_ValidateByCombinationSetting.IsSuccess)
        //            {
        //                response.ErrorMessage += $" #{_ValidateByCombinationSetting.ErrorMessage}-{STDINFO.CUSRegistrationNo + "-" + STDINFO.FullName}# ";
        //                continue;
        //            }

        //            ARGSelectedCombination _PrevSemCombination = null;
        //            if (model.Combination.OldCombination_ID.HasValue)
        //                _PrevSemCombination = STDINFO.SelectedCombinations.First(x => x.Combination_ID == model.Combination.OldCombination_ID && x.Semester == model.Combination.Semester);
        //            else
        //                _PrevSemCombination = STDINFO.SelectedCombinations.First(x => x.Semester == model.Combination.Semester);

        //            if (_PrevSemCombination == null)
        //            {
        //                response.ErrorMessage += $" #Missing previous semester combination-{STDINFO.CUSRegistrationNo + "-" + STDINFO.FullName}# ";
        //                continue;
        //            }

        //            ARGSelectedCombination _CurrentSemComb = STDINFO.SelectedCombinations.FirstOrDefault(x => x.Semester == model.Combination.Semester);
        //            if (dbNewComb.Course_ID != _CurrentSemComb.Course_ID || dbNewComb.Semester != _CurrentSemComb.Semester || dbNewComb.Course_ID != _PrevSemCombination.Course_ID || dbNewComb.Semester != _PrevSemCombination.Semester)
        //            {
        //                response.ErrorMessage += $" #Invalid new combination-{STDINFO.CUSRegistrationNo + "-" + STDINFO.FullName}# ";
        //                continue;
        //            }
        //            var _response = SaveUpdateStudentCombination(STDINFO, model.Combination.Semester, dbNewComb.Combination_ID, printProgramme);
        //            if (!_response.IsSuccess)
        //            {
        //                response.ErrorMessage += $" #{_response.ErrorMessage}-{STDINFO.CUSRegistrationNo + "-" + STDINFO.FullName}# ";
        //                continue;
        //            }
        //        }
        //        var numberOfRecordsFailed = model.Student_ID.Count - response.NumberOfRecordsEffected;
        //        response.IsSuccess = response.NumberOfRecordsEffected > 0;
        //        response.SuccessMessage = response.NumberOfRecordsEffected > 0 ? "Records updated successfully." : "";
        //        response.ErrorMessage += numberOfRecordsFailed > 0 ? numberOfRecordsFailed + " records failed to updated." : "";
        //    }
        //    return response;
        //}

        public ResponseData BatchUpdateNextSemesterStudentCombination(BatchUpdatePersonalInfo model, PrintProgramme printProgramme)
        {
            ResponseData response = new ResponseData() { IsSuccess = true };
            if (model != null && model.Student_ID.IsNotNullOrEmpty() && model.Combination != null)
            {
                ADMCombinationMaster dbNewComb = new CombinationManager().GetCombinationByID(model.Combination.NewCombination_ID);
                if (dbNewComb == null | dbNewComb.College_ID != AppUserHelper.College_ID)
                    return new ResponseData() { ErrorMessage = "Combination not found" };

                ApplicationFormsManager applicationFormsManager = new ApplicationFormsManager();

                var STDINFO = new StudentManager().GetStudentWithCombination(model.Student_ID.First(), printProgramme, false);

                var _ValidateByCombinationSetting = new CombinationSettingManager()
                    .GetCombinationSetting(STDINFO.SelectedCombinations?.OrderByDescending(x => x.Semester)?.Last().Course_ID ?? Guid.Empty,
                    model.Combination.Semester, model.Combination.SemesterBatch);

                if (!_ValidateByCombinationSetting.AllowCollegeChangeCombination)
                {
                    response.ErrorMessage += $"Assigning of Combinations has been closed.";
                    return response;
                }
                foreach (var Student_ID in model.Student_ID)
                {

                    int NumberOfRecordsEffected = 0;
                    STDINFO = new StudentManager().GetStudentWithCombination(Student_ID, printProgramme, false);

                    if (applicationFormsManager.DegreeAlreadyPrinted(Student_ID))
                    {
                        response.ErrorMessage += $"It seems that degree certificate has been printed for the student-{STDINFO.CUSRegistrationNo + "-" + STDINFO.FullName}.Hence, no changes are allowed<br/>";
                        continue;
                    }

                    var _ValidateByStudent = ValidateByStudent(STDINFO, model.Combination.Semester);
                    if (!_ValidateByStudent.IsSuccess)
                    {
                        response.ErrorMessage += $" #{_ValidateByStudent.ErrorMessage}-{STDINFO.CUSRegistrationNo + "-" + STDINFO.FullName}# ";
                        continue;
                    }

                    ARGSelectedCombination _PrevSemCombination = STDINFO.SelectedCombinations.FirstOrDefault(x => x.Semester == ((model.Combination.Semester - 1) <= 0 ? 1 : (model.Combination.Semester - 1)));
                    if (_PrevSemCombination == null)
                    {
                        response.ErrorMessage += $" #Missing previous semester combination-{STDINFO.CUSRegistrationNo + "-" + STDINFO.FullName}# ";
                        continue;
                    }



                    ARGSelectedCombination _CurrentSemComb = STDINFO.SelectedCombinations.FirstOrDefault(x => x.Semester == model.Combination.Semester);
                    if ((_CurrentSemComb != null) && (dbNewComb.Course_ID != _PrevSemCombination.Course_ID || dbNewComb.Semester != _CurrentSemComb.Semester))
                    {
                        response.ErrorMessage += $" #Invalid combination-{STDINFO.CUSRegistrationNo + "-" + STDINFO.FullName}# ";
                        continue;
                    }
                    var _response = SaveUpdateStudentCombination(STDINFO, model.Combination.Semester, dbNewComb.Combination_ID, printProgramme, model.Combination.SemesterBatch);
                    if (!_response.IsSuccess)
                    {
                        response.ErrorMessage += $" #{_response.ErrorMessage}-{STDINFO.CUSRegistrationNo + "-" + STDINFO.FullName}# ";
                        continue;
                    }
                    NumberOfRecordsEffected += _response.NumberOfRecordsEffected;
                    if (STDINFO.CurrentSemesterOrYear >= model.Combination.Semester)
                        NumberOfRecordsEffected += 1;
                    else if (STDINFO.CurrentSemesterOrYear < model.Combination.Semester && AppUserHelper.AppUsercompact.UserRoles.Any(x => x != AppRoles.Student))
                        NumberOfRecordsEffected += new StudentDB().UpdateCurrentSemOrYear(STDINFO.Student_ID, model.Combination.Semester, printProgramme);
                    else if (STDINFO.CurrentSemesterOrYear < model.Combination.Semester && (STDINFO.CurrentSemesterOrYear + 1) % 2 == 0)
                        NumberOfRecordsEffected += new StudentDB().UpdateCurrentSemOrYear(STDINFO.Student_ID, model.Combination.Semester, printProgramme);

                    response.NumberOfRecordsEffected += NumberOfRecordsEffected == 2 ? 1 : 0; //2=i.e both combination is assigned and semester is updated
                }
                var numberOfRecordsFailed = model.Student_ID.Count - response.NumberOfRecordsEffected;
                response.IsSuccess = response.NumberOfRecordsEffected > 0;
                response.SuccessMessage = response.NumberOfRecordsEffected > 0 ? "Records updated successfully." : "";
                response.ErrorMessage += numberOfRecordsFailed > 0 ? numberOfRecordsFailed + " records failed to updated." : "";
            }
            return response;
        }

        public ResponseData ToggleVerifyCombination(List<Guid> Student_IDs, PrintProgramme printProgramme, short Semester)
        {
            ResponseData response = new ResponseData() { IsSuccess = true };
            response.NumberOfRecordsEffected = new AssignCombinationDB().ToggleVerifyCombination(Student_IDs, printProgramme, Semester);
            response.IsSuccess = response.NumberOfRecordsEffected > 0;
            response.SuccessMessage = response.NumberOfRecordsEffected > 0 ? response.NumberOfRecordsEffected + " Records updated successfully." : "";
            response.ErrorMessage = response.NumberOfRecordsEffected == 0 ? Student_IDs?.Count - response.NumberOfRecordsEffected + " records failed to updated." : "";

            return response;
        }


        private void SaveAdditionalSubject(PrintProgramme printProgramme, ARGPersonalInformation STDINFO, CombinationSetting _CS, List<ADMSubjectMaster> AdditionalSubjectsChoosen, short _SetCombinationForSemester, ADMCombinationMaster aDMCombination, short SemesterBatch)
        {
            if (_CS.CombinationSettingStructure.AdditionalCourseCount > 0)
            {
                var dbsubjects = AdditionalSubjectsChoosen == null || AdditionalSubjectsChoosen.Count == 0 ? null : new SubjectsManager().GetList(AdditionalSubjectsChoosen?.Select(x => x.Subject_ID)?.ToList())?.ToList();
                List<StudentAdditionalSubject> alreadyOptedAdditionalSubject = new StudentManager().GetStudentAdditionalSubjects(STDINFO.Student_ID, _SetCombinationForSemester, printProgramme);

                if (alreadyOptedAdditionalSubject.IsNotNullOrEmpty())
                    foreach (var item in alreadyOptedAdditionalSubject)
                    {
                        if (dbsubjects == null)
                            new StudentManager().DeleteStudentAdditionalSubject(STDINFO.Student_ID, _SetCombinationForSemester, printProgramme);
                        else if (dbsubjects.Any(x => x.Subject_ID != item.Subject_ID))
                            new StudentManager().DeleteStudentAdditionalSubject(STDINFO.Student_ID, _SetCombinationForSemester, item.Subject_ID, printProgramme);
                    }
                if (dbsubjects != null)
                {
                    foreach (var subject in dbsubjects)
                    {
                        if (aDMCombination.SubjectsDetails.Any(x => x.Subject_ID == subject.Subject_ID)) return;
                        var dbAdditionalsubject = alreadyOptedAdditionalSubject?.FirstOrDefault(x => x.Subject_ID == subject.Subject_ID);
                        if (dbAdditionalsubject != null)
                        {
                            dbAdditionalsubject.SetWorkFlow(RecordState.Old);
                            dbAdditionalsubject.IsVerified = AppUserHelper.AppUsercompact.UserRoles.Any(x => x != AppRoles.College);
                            new StudentManager().Update(dbAdditionalsubject, printProgramme);
                        }
                        else
                        {
                            StudentAdditionalSubject item = new StudentAdditionalSubject() { AdditionalSubject_ID = Guid.NewGuid(), Student_ID = STDINFO.Student_ID, Semester = _SetCombinationForSemester, Subject_ID = subject.Subject_ID, Course_ID = aDMCombination.Course_ID.Value, SemesterBatch = SemesterBatch };
                            item.SetWorkFlow(RecordState.New);
                            item.IsVerified = AppUserHelper.AppUsercompact.UserRoles.Any(x => x != AppRoles.Student);
                            new StudentManager().Save(item, printProgramme);
                        }
                    }
                }
            }
        }


        private ResponseData SaveUpdateStudentCombination(ARGPersonalInformation STDINFO, short setCombinationForSemester, Guid Combination_ID, PrintProgramme printProgramme, short SemesterBatch)
        {
            var response = new ResponseData() { };
            var dbstudentCurrentCombination = STDINFO.SelectedCombinations?.FirstOrDefault(x => x.Semester == setCombinationForSemester);
            if (dbstudentCurrentCombination == null)
            {
                dbstudentCurrentCombination = new ARGSelectedCombination()
                {
                    Combination_ID = Combination_ID,
                    Semester = setCombinationForSemester,
                    SelectedCombination_ID = Guid.NewGuid(),
                    Student_ID = STDINFO.Student_ID,
                    SemesterBatch = SemesterBatch,
                    IsVerified = AppUserHelper.AppUsercompact.UserRoles.Any(x => x != AppRoles.Student)
                };
                dbstudentCurrentCombination.SetWorkFlow(RecordState.New);
                response.NumberOfRecordsEffected = new AssignCombinationDB().Save(dbstudentCurrentCombination, printProgramme);
            }
            else
            {
                if (dbstudentCurrentCombination.Combination_ID == Combination_ID && dbstudentCurrentCombination.IsVerified && dbstudentCurrentCombination.SemesterBatch >= 2017)
                    response.NumberOfRecordsEffected += 1;
                else if (AppUserHelper.AppUsercompact.UserRoles.Any(x => x == AppRoles.Student) && dbstudentCurrentCombination.Combination_ID != Combination_ID && dbstudentCurrentCombination.IsVerified)
                    return new ResponseData() { ErrorMessage = "Your subjects have been already verified. Please visit college administration section in order to change your subjects.", IsSuccess = true, NumberOfRecordsEffected = 1 };
                else
                {
                    dbstudentCurrentCombination.SemesterBatch = SemesterBatch;
                    dbstudentCurrentCombination.IsVerified = AppUserHelper.AppUsercompact.UserRoles.Any(x => x != AppRoles.Student);
                    dbstudentCurrentCombination.Combination_ID = Combination_ID;
                    dbstudentCurrentCombination.SetWorkFlow(RecordState.Old);
                    response.NumberOfRecordsEffected = new AssignCombinationDB().Update(dbstudentCurrentCombination, printProgramme);
                }
            }
            response.IsSuccess = response.NumberOfRecordsEffected == 1;
            return response;
        }

        #endregion





        public List<CourseWiseList> GetStudentDetails(Parameters parameters, PrintProgramme printProgramme)
        {
            return new AssignCombinationDB().GetStudentsList(parameters, printProgramme);
        }
        public DataTable GetStudentsListAsDataTable(Parameters parameters)
        {
            return new AssignCombinationDB().GetStudentsListAsDataTable(parameters);
        }


        public List<CombinationsCount> GetCombinationsCount(Parameters parameters)
        {
            Guid Course_ID = new Guid((parameters.Filters.Where(x => x.Column.ToLower() == "course_id" || x.Column.ToLower() == "course").FirstOrDefault().Value));

            List<CombinationsCount> list = new AssignCombinationDB().GetCombinationsCount(parameters);
            List<ADMSubjectMaster> listSubjects = null;
            foreach (var item in list ?? new List<CombinationsCount>())
            {
                if (!string.IsNullOrWhiteSpace(item.CombinationSubjects))
                {
                    listSubjects = new SubjectDB().GetList(item.CombinationSubjects.ToGuidList());

                    if (listSubjects.IsNotNullOrEmpty())
                        item.SubjectsDetails.AddRange(listSubjects);
                }
            }
            return list;
        }
        public List<SubjectsCount> GetSubjectsCount(Parameters parameters)
        {
            List<SubjectsCount> list = new AssignCombinationDB().GetSubjectsCount(parameters);
            return list;
        }
        public DataTable GetSubjectsCountE(Parameters parameters)
        {
            return new AssignCombinationDB().GetSubjectsCountE(parameters);
        }

        //public List<SelectListItem> GetCombinationsDDL(Guid College_ID, Guid Course_ID)
        //{
        //    return new CombinationManager().GetCombinationsDDL(College_ID, Course_ID);
        //}

        public ARGPersonalInformation GetStudentDetails(string formNumber, PrintProgramme programme, bool PersonalInfoAndCoursesAppliedOnly = false)
        {
            var RManager = new RegistrationManager();
            programme = RManager.MappingTable(programme);
            var studentInfo = new AssignCombinationDB().GetStudentDetails(formNumber, programme);
            if (studentInfo != null)
            {
                studentInfo.CoursesApplied = RManager.GetStudentCourses(studentInfo.Student_ID, programme);
                if (!PersonalInfoAndCoursesAppliedOnly)
                {
                    studentInfo.StudentAddress = RManager.GetStudentAddress(studentInfo.Student_ID, programme);
                    studentInfo.AcademicDetails = RManager.GetStudentAcademicDetails(studentInfo.Student_ID, programme);

                    studentInfo.SelectedCombinations = RManager.GetSelectedCombinations(studentInfo.Student_ID, programme);
                }
            }
            return studentInfo;
        }

        //public ARGSelectedCombination GetSelectedCombinations(Guid student_ID, short Semester, PrintProgramme programme)
        //{
        //    return new StudentDB().GetSelectedCombinations(student_ID, Semester, programme);
        //}

        /// <summary>
        /// This method is meant for semester 1 students only
        /// </summary>
        /// <param name="NewSelectedCombination"></param>
        /// <returns></returns>
        public ResponseData SaveUpdateCombinationToSemester1(ARGSelectedCombination NewSelectedCombination, Decimal? OfflinePaymentAmount, DateTime? BankPaymentDate)
        {
            NewSelectedCombination.Semester = 1;

            ResponseData response = new ResponseData() { ErrorMessage = "Failed to save record!" };
            var studentInfo = new StudentManager().GetStudent(NewSelectedCombination.Student_ID, NewSelectedCombination.PrintProgramme, true);
            if (studentInfo.FormStatus == FormStatus.InProcess)
                return new ResponseData() { ErrorMessage = "Student has not paid university admission fee yet." };



            ARGSelectedCombination dbSelectedCombination = new StudentDB().GetSelectedCombinations(NewSelectedCombination.Student_ID, NewSelectedCombination.Semester, NewSelectedCombination.PrintProgramme);

            var _CS = new CombinationSettingManager().GetCombinationSetting(NewSelectedCombination.Course_ID, NewSelectedCombination.Semester, NewSelectedCombination.SemesterBatch);

            List<ADMCollegeCourseMapping> collegeCourses = new CourseManager().GetCollegeCourseMappingList(AppUserHelper.College_ID.Value);

            if (!(collegeCourses?.FirstOrDefault(x => x.Course_ID == _CS.Course_ID)?.AllowBOPEERegistration ?? false))
            {
                if (studentInfo.CATEntrancePoints == null && studentInfo.Batch > 2023)
                    return new ResponseData() { ErrorMessage = "Student has not appeared in entrance." };
            }

            var CombinationMaster = new CombinationManager().GetCombinationByID(NewSelectedCombination.Combination_ID);
            NewSelectedCombination.Subjects = CombinationMaster.SubjectsDetails;
            var _ValidateByCombinationSetting = ValidateByCombinationSetting(NewSelectedCombination.PrintProgramme, studentInfo, _CS, NewSelectedCombination.Subjects, NewSelectedCombination);
            if (!_ValidateByCombinationSetting.IsSuccess) return _ValidateByCombinationSetting;



            #region check for intack capacity
            if (dbSelectedCombination == null || dbSelectedCombination.Course_ID != NewSelectedCombination.Course_ID)
            {
                if ((!OfflinePaymentAmount.HasValue || OfflinePaymentAmount < 100))
                    return new ResponseData() { ErrorMessage = "Offline payment amount is invalid" };

                #region Check_College_In_Take_Capacity
                //ADMCollegeCourseMapping aDMCollegeCourseMapping = new CourseDB().GetCourseCollege(NewSelectedCombination.Course_ID)?.FirstOrDefault(x => x.College_ID == AppUserHelper.College_ID);
                //if (aDMCollegeCourseMapping == null)
                //    return new ResponseData() { ErrorMessage = "Course not found in your college." };
                //int totalIntackCapacity = aDMCollegeCourseMapping.MeritBasislInTackCapacity + aDMCollegeCourseMapping.SelfFinancedInTackCapacity;
                //int totalAcceptedFormForCourse = new CombinationSettingManager().GetTotalStudentsAlloted(NewSelectedCombination.Course_ID, (Guid)AppUserHelper.College_ID, studentInfo.Batch, NewSelectedCombination.PrintProgramme);

                //if (totalIntackCapacity > 0 && totalAcceptedFormForCourse >= totalIntackCapacity)
                //    return new ResponseData() { ErrorMessage = $"Intake Capacity ({totalIntackCapacity}) reached for this course" };

                #endregion

                if (_CS.CheckCourseApplied)
                {
                    #region Check_Colleg_Preference
                    // if (NewSelectedCombination.PrintProgramme == PrintProgramme.UG)
                    // {
                    //Parameters _param = new Parameters() { Filters = new List<SearchFilter>() { new SearchFilter() { Column = "StudentFormNo", Value = studentInfo.StudentFormNo } }, PageInfo = new Paging() { DefaultOrderByColumn = "StudentFormNo" } };
                    //     collegePref = new StudentManager().GetCollegePreference(NewSelectedCombination.Student_ID);
                    //    if (collegePref.IsNullOrEmpty() || !collegePref.Any(x => x.Course_ID == NewSelectedCombination.Course_ID && x.College_ID == NewSelectedCombination.College_ID && x.IsAllowed == true && x.IsActive == true))
                    //        return new ResponseData() { ErrorMessage = "Student has not applied/allowed for this course/college." };
                    //}
                    //else  
                    #endregion
                    if (!new StudentManager().HasAppliedForCourse(NewSelectedCombination.PrintProgramme, NewSelectedCombination.Student_ID, NewSelectedCombination.Course_ID))
                        return new ResponseData() { ErrorMessage = "Student has not applied for this course." };
                }

            }
            #endregion

            if (NewSelectedCombination.SelectedCombination_ID == Guid.Empty && NewSelectedCombination.PrintProgramme != PrintProgramme.PG)
            {
                var studentOnBoardNo = new StudentManager().GetStudent(studentInfo.BoardRegistrationNo)?.Where(x => !x.AcceptCollege_ID.IsNullOrEmpty() && x.FormStatus != FormStatus.CancelRegistration)?.ToList();
                if (studentOnBoardNo.IsNotNullOrEmpty() && !new TranscriptDB().HasTranscript(studentOnBoardNo.First().Student_ID))
                {
                    var varyingInfo = studentOnBoardNo.FirstOrDefault(x => x.Course_ID != NewSelectedCombination.Course_ID);
                    return new ResponseData() { ErrorMessage = $"The board registration no {studentInfo.BoardRegistrationNo} is registered in College:{(varyingInfo == null ? studentOnBoardNo.First().CollegeFullName : varyingInfo.CollegeFullName)} under course {(varyingInfo == null ? studentOnBoardNo.First().CourseFullName : varyingInfo.CourseFullName)}" };
                }
            }
            if (!studentInfo.AcceptCollege_ID.IsNullOrEmpty() && studentInfo.AcceptCollege_ID != AppUserHelper.College_ID)
                return new ResponseData() { ErrorMessage = "Student belongs to " + studentInfo.AcceptCollegeID };

            studentInfo.SelectedCombinations = new List<ARGSelectedCombination>() { NewSelectedCombination };

            using (var transcope = new TransactionScope())
            {
                if (dbSelectedCombination == null)
                {
                    dbSelectedCombination = NewSelectedCombination;
                    dbSelectedCombination.IsVerified = AppUserHelper.Designation != "Student";
                    NewSelectedCombination.SelectedCombination_ID = Guid.NewGuid();
                    NewSelectedCombination.SetWorkFlow(RecordState.New);
                    response.NumberOfRecordsEffected = new AssignCombinationDB().Save(dbSelectedCombination, dbSelectedCombination.PrintProgramme);
                }
                else
                {
                    if (dbSelectedCombination.Combination_ID == NewSelectedCombination.Combination_ID && dbSelectedCombination.Semester == NewSelectedCombination.Semester && dbSelectedCombination.IsVerified)
                        response.NumberOfRecordsEffected += 1;
                    else
                    {
                        dbSelectedCombination.Combination_ID = NewSelectedCombination.Combination_ID;
                        dbSelectedCombination.IsVerified = AppUserHelper.AppUsercompact.UserRoles.Any(x => x != AppRoles.Student);
                        dbSelectedCombination.SetWorkFlow(RecordState.Old);
                        response.NumberOfRecordsEffected = new AssignCombinationDB().Update(dbSelectedCombination, dbSelectedCombination.PrintProgramme);
                    }
                }

                var Payment = new PaymentManager().GetPaymentDetails(studentInfo.Student_ID, PaymentModuleType.SemesterAdmission, NewSelectedCombination.PrintProgramme, NewSelectedCombination.Semester)?.FirstOrDefault();
                if (Payment == null)
                {
                    PaymentDetails _payment = new PaymentDetails()
                    {
                        Payment_ID = Guid.NewGuid(),
                        PaymentType = PaymentType.Offline,
                        Semester = 1,
                        AuthStatus = "0300",
                        BankID = "CUS",
                        BankMerchantID = "CUS",
                        BankReferenceNo = "CUS",
                        CurrencyName = "INR",
                        TxnReferenceNo = studentInfo.StudentFormNo,
                        TxnDate = BankPaymentDate.Value,
                        Entity_ID = studentInfo.Student_ID,
                        ModuleType = PaymentModuleType.SemesterAdmission,
                        TxnAmount = OfflinePaymentAmount.Value
                    };
                    response.NumberOfRecordsEffected += new PaymentDB().SavePaymentDetails(_payment, NewSelectedCombination.PrintProgramme);
                }
                else if (OfflinePaymentAmount.HasValue && BankPaymentDate.HasValue && (Payment.PaymentType == PaymentType.Offline && Payment.TxnAmount != OfflinePaymentAmount.Value || Payment.TxnDate.Date != BankPaymentDate.Value.Date))
                {
                    Payment.TxnDate = BankPaymentDate.Value;
                    Payment.TxnAmount = OfflinePaymentAmount.Value;
                    response.NumberOfRecordsEffected += new PaymentDB().Update(Payment, NewSelectedCombination.PrintProgramme);
                }



                if (response.NumberOfRecordsEffected > 0)
                {
                    if (studentInfo.FormStatus != FormStatus.Selected || studentInfo.CurrentSemesterOrYear <= 0 || !studentInfo.AcceptCollege_ID.HasValue)
                    {
                        studentInfo.FormStatus = FormStatus.Selected;
                        studentInfo.AcceptCollege_ID = AppUserHelper.College_ID;
                        studentInfo.CurrentSemesterOrYear = 1;
                        studentInfo.AcceptedBy_ID = AppUserHelper.User_ID;
                        response.NumberOfRecordsEffected += new StudentManager().UpdateStudentCombinationInfo(studentInfo, dbSelectedCombination.PrintProgramme);
                    }
                    else
                        response.NumberOfRecordsEffected += 1;
                }
                transcope.Complete();
            }

            if (response.NumberOfRecordsEffected > 0)
                return new ResponseData() { NumberOfRecordsEffected = 1, IsSuccess = true, SuccessMessage = "Record Saved successfully" };
            else
                return new ResponseData() { ErrorMessage = "Failed to save record" };
        }



        //public ResponseData SaveUpdate(List<ARGSelectedCombination> SelectedCombinations, PrintProgramme printProgramme)
        //{
        //    ResponseData response = new ResponseData() { IsSuccess = true };
        //    if (SelectedCombinations != null)
        //    {
        //        var studentInfo = new StudentManager().GetStudent(SelectedCombinations.First().Student_ID, printProgramme, false);
        //        if (studentInfo.AcceptCollege_ID.HasValue && studentInfo.AcceptCollege_ID.Value != Guid.Empty && studentInfo.AcceptCollege_ID != AppUserHelper.College_ID)
        //        {
        //            response.ErrorMessage = "Student belongs to " + studentInfo.AcceptCollegeID;
        //            response.IsSuccess = false;
        //            return response;
        //        }
        //        //if (SelectedCombinations.FirstOrDefault(x => x.Semester < studentInfo.CurrentSemesterOrYear) != null)
        //        //{
        //        //    response.ErrorMessage = "Previous combinations of a student whose result has been created cannot be changed. Please send details to cluster university in order to change it subject combination.";
        //        //    response.IsSuccess = false;
        //        //    return response;
        //        //}
        //        if (response.IsSuccess)
        //        {
        //            try
        //            {
        //                using (TransactionScope transaction = new TransactionScope())
        //                {
        //                    foreach (var comb in SelectedCombinations)
        //                    {
        //                        ARGSelectedCombination studentCombination = new StudentDB().GetSelectedCombinations(comb.Student_ID, comb.Semester, printProgramme);
        //                        if (studentCombination == null)
        //                        {
        //                            studentCombination = comb;
        //                            studentCombination.SelectedCombination_ID = Guid.NewGuid();
        //                            studentCombination.SetWorkFlow(RecordState.New);
        //                            response.NumberOfRecordsEffected += new AssignCombinationDB().Save(studentCombination, printProgramme);
        //                        }
        //                        else
        //                        {
        //                            //var param = new Parameters() {Filters=new List<SearchFilter>() { new SearchFilter() { Column=""} } };
        //                            //var dfd = new ExaminationFormManager().GetExaminationFormList(param, printProgramme);
        //                            if (comb.Semester < studentInfo.CurrentSemesterOrYear)
        //                            {
        //                                response.ErrorMessage = "Previous combinations of a student cannot be changed. Please send details to cluster university in order to change it subject combination.";
        //                                response.IsSuccess = false;
        //                                return response;
        //                                //continue; //previous combination can not be changed
        //                            }
        //                            if (studentCombination.Combination_ID == comb.Combination_ID && studentCombination.Semester == comb.Semester)
        //                                response.NumberOfRecordsEffected += 1;
        //                            else
        //                            {
        //                                studentCombination.Combination_ID = comb.Combination_ID;
        //                                studentCombination.SetWorkFlow(RecordState.Old);
        //                                response.NumberOfRecordsEffected += new AssignCombinationDB().Update(studentCombination, printProgramme);
        //                            }
        //                        }
        //                    }
        //                    int studentUpdateStatus = 0;
        //                    if (response.NumberOfRecordsEffected == SelectedCombinations.Count)
        //                    {
        //                        if (studentInfo.FormStatus == FormStatus.Selected && studentInfo.CurrentSemesterOrYear > 0)
        //                        {
        //                            studentUpdateStatus = 1;
        //                        }
        //                        else
        //                        {
        //                            studentInfo.FormStatus = FormStatus.Selected;
        //                            studentInfo.AcceptCollege_ID = AppUserHelper.College_ID;
        //                            studentInfo.AcceptedBy_ID = AppUserHelper.User_ID;
        //                            studentInfo.MothersName = studentInfo.MothersName.IsNotNullOrEmpty() ? studentInfo.MothersName : " ";
        //                            studentInfo.CurrentSemesterOrYear = studentInfo.CurrentSemesterOrYear == 0 ? (short)1 : studentInfo.CurrentSemesterOrYear;
        //                            studentUpdateStatus = new StudentManager().Update(studentInfo, printProgramme).NumberOfRecordsEffected;
        //                        }
        //                    }
        //                    if (response.NumberOfRecordsEffected != SelectedCombinations.Count || studentUpdateStatus != 1)
        //                    {
        //                        response.IsSuccess = false;
        //                        response.ErrorMessage = "Something wend wrong...";
        //                        response.ResponseObject = null;
        //                        throw new TransactionAbortedException();
        //                    }
        //                    response.SuccessMessage = "Record saved successfully";
        //                    //response.ResponseObject=  new StudentManager().GetStudentWithCombination(studentInfo.CUSRegistrationNo, printProgramme);
        //                    transaction.Complete();
        //                }
        //            }
        //            catch (TransactionAbortedException) { }
        //        }
        //    }
        //    return response;
        //}


        /// <summary>
        /// Saves the combinations for all students other than first semester
        /// </summary>
        /// <param name="student_ID"></param>
        /// <param name="_SetCombinationForSemester"></param>
        /// <param name="printProgramme"></param>
        /// <param name="subjectsChoosen">New subjects to be assigned</param>
        /// <returns></returns>


        ResponseData ValidateByStudent(ARGPersonalInformation STDINFO, short _SetCombinationForSemester)
        {
            if (STDINFO == null)
                return new ResponseData() { ErrorMessage = $"Student not found." };

            if (STDINFO.IsPassout)
                return new ResponseData() { ErrorMessage = $"Student already passout. Hence no changes are allowed for {STDINFO.CUSRegistrationNo}" };

            if (STDINFO.AcceptCollege_ID.IsNullOrEmpty())
                return new ResponseData() { ErrorMessage = $"The student is not registered under any college." };
            if (AppUserHelper.AppUsercompact.UserRoles.Any(x => x != AppRoles.University))
                if (AppUserHelper.AppUsercompact.UserRoles.Any(x => x == AppRoles.College))
                    if (STDINFO.AcceptCollege_ID != AppUserHelper.College_ID)
                        return new ResponseData() { ErrorMessage = "Student belongs to " + STDINFO.AcceptCollegeID };

            if (STDINFO.FormStatus != FormStatus.Selected)
                return new ResponseData() { ErrorMessage = $"Status of the student is {Helper.GetEnumDescription(STDINFO.FormStatus)}" };

            if (STDINFO.SelectedCombinations.IsNullOrEmpty())
                return new ResponseData() { ErrorMessage = $"Subject Combination not found. Please visit Cluster University I.T. Cell." };

            ARGSelectedCombination _PrevSemCombination = STDINFO.SelectedCombinations.FirstOrDefault(x => x.Semester == (short)((_SetCombinationForSemester - 1) <= 0 ? 1 : (_SetCombinationForSemester - 1)));
            _PrevSemCombination = STDINFO.SelectedCombinations.FirstOrDefault();
            if (_PrevSemCombination == null || _PrevSemCombination.Subjects.IsNullOrEmpty())
                return new ResponseData() { ErrorMessage = $"SPrevios semester subjects not found." };
            return new ResponseData() { IsSuccess = true };

        }
        //ResponseData ValidateByCombination(ARGPersonalInformation STDINFO, short _SetCombinationForSemester, PrintProgramme printProgramme, List<ADMSubjectMaster> subjectsChoosen)
        //{
        //    List<ADMSubjectMaster> dbCurrentSemsubjects = new List<ADMSubjectMaster>(); Parameters param = null;

        //    var combinationSetting = new CombinationSettingManager().GetCombinationSetting(STDINFO.SelectedCombinations.First().Course_ID, _SetCombinationForSemester, STDINFO.Batch);
        //    if (combinationSetting == null)
        //        return new ResponseData() { ErrorMessage = "Combination setting not available. Please contact cluster university I.T. Cell" };

        //    if (subjectsChoosen.IsNotNullOrEmpty())
        //    {
        //        param = new Parameters() { Filters = new List<SearchFilter>() { new SearchFilter() { Column = "Semester", Operator = SQLOperator.EqualTo, Value = _SetCombinationForSemester.ToString() }, new SearchFilter() { Column = "Subject_ID", Value = string.Join(",", subjectsChoosen.Select(x => x.Subject_ID).ToList()), Operator = SQLOperator.In } } };
        //        var dbsubjects = new SubjectsManager().GetAllSubjects(param);
        //        if (dbsubjects.IsNullOrEmpty() || subjectsChoosen.Count != dbsubjects?.Count)
        //            return new ResponseData() { ErrorMessage = "Miss match in total number of subjects" };
        //        dbCurrentSemsubjects.AddRange(dbsubjects);
        //    }
        //    if (combinationSetting.CompulsarySubject.IsNotNullOrEmpty())
        //    {
        //        param = new Parameters() { Filters = new List<SearchFilter>() { new SearchFilter() { Column = "Semester", Operator = SQLOperator.EqualTo, Value = _SetCombinationForSemester.ToString() }, new SearchFilter() { Column = "Subject_ID", Value = string.Join(",", combinationSetting.CompulsarySubject.Select(x => x.Subject_ID).ToList()), Operator = SQLOperator.In } } };
        //        var dbsubjects = new SubjectsManager().GetAllSubjects(param);
        //        if (dbsubjects.IsNullOrEmpty() || combinationSetting.CompulsarySubject.Count != dbsubjects?.Count)
        //            return new ResponseData() { ErrorMessage = "Miss match in total number of subjects" };
        //        dbCurrentSemsubjects.AddRange(dbsubjects);
        //    }

        //    ARGSelectedCombination _PrevSemCombination = STDINFO.SelectedCombinations.FirstOrDefault(x => x.Semester == (short)(_SetCombinationForSemester - 1));
        //    if (_PrevSemCombination == null) _PrevSemCombination = STDINFO.SelectedCombinations.FirstOrDefault();
        //    ADMCombinationMaster aDMCombination = GetSetCombinationMaster(_PrevSemCombination, dbCurrentSemsubjects, _SetCombinationForSemester);
        //    if (aDMCombination == null)
        //        return new ResponseData() { ErrorMessage = "An error occurred while saving combination. Please try again later." };

        //    return new ResponseData() { IsSuccess = true, ResponseObject = aDMCombination };
        //}

        public ResponseData ValidateByCombinationSetting(PrintProgramme printProgramme, ARGPersonalInformation STDINFO, CombinationSetting _CS
            , List<ADMSubjectMaster> _SUB, ARGSelectedCombination NewSelectedCombination = null)
        {
            string ErrorMessage = "";
            if (_CS == null)
                return new ResponseData() { ErrorMessage = "Combination setting not found" };
            if (STDINFO == null)
                return new ResponseData() { ErrorMessage = "Student not found" };

            if (!AppUserHelper.AppUsercompact.UserRoles.Any(x => x == AppRoles.College) && _CS.Semester == 1)
                return new ResponseData() { ErrorMessage = "Only college can assign combination for lateral entry." };

            //ARGSelectedCombination studentCurrentCombination = STDINFO.SelectedCombinations?.FirstOrDefault(x => x.Semester == _CS.CombinationSettingStructure.Semester);
            if (AppUserHelper.AppUsercompact.UserRoles.Any(x => x == AppRoles.Student))
            {
                if (!_CS.AllowStudentChangeCombination)
                {
                    if (STDINFO.SelectedCombinations.Where(x => x.Semester == _CS.Semester).IsNotNullOrEmpty())
                    {
                        return new ResponseData()
                        {
                            ErrorMessage = "The process of changing combination for student has been closed. " +
                            "Contact college administration section if change in subjects.",
                            IsSuccess = true,
                            ResponseObject = new StudentManager().GetStudentWithCombination(STDINFO.Student_ID, printProgramme, false, _CS.Semester).SelectedCombinations.First().Subjects
                        };
                    }
                    return new ResponseData() { ErrorMessage = "The process of changing combination for student has been closed. Contact college administration section if change in subjects.", IsSuccess = false };

                }
            }
            else if (AppUserHelper.AppUsercompact.UserRoles.Any(x => x == AppRoles.College))
            {
                if (!_CS.AllowCollegeChangeCombination)
                    return new ResponseData() { ErrorMessage = "The process of changing the combination has been closed. Contact cluster university I.T. Cell.", IsSuccess = false };
            }

            if (_CS.CheckAdmissionForm)
            {
                var paymentInfo = new PaymentManager().GetPaymentDetail(STDINFO.Student_ID, _CS.Semester, printProgramme);
                if (paymentInfo == null)
                    return new ResponseData() { ErrorMessage = $"Either admission form not found or fee not paid." };
            }

            bool _ExaminationFormSubmitted = false;
            if (_CS.CheckExaminationForm)
            {
                ExaminationCourseCategory courseCategory = new GeneralFunctions().GetExaminationCourseCategory(printProgramme);
                var settingForm = new ExaminationFormDB().GetRegularExamFormDownloadable(courseCategory, _CS.Semester);
                if (settingForm == null)
                    return new ResponseData() { ErrorMessage = $"Examination form setting not found. Contact Cluster University I.T Cell." };
                _ExaminationFormSubmitted = new ExaminationFormManager().ExamFormSubmitted(STDINFO.Student_ID, _CS.Semester, settingForm.Year, printProgramme);
                if (AppUserHelper.AppUsercompact.UserRoles.Any(x => x != AppRoles.Student) && !_ExaminationFormSubmitted)
                    return new ResponseData() { ErrorMessage = $"Either examination form not found or fee not paid." };
            }

            if (_CS.CheckSelectionListCollege)
            {
                if (!new StudentManager().IsSelectedInLoggedInCollege(STDINFO.Student_ID, AppUserHelper.College_ID.Value, printProgramme))
                {
                    return new ResponseData() { ErrorMessage = $"Student is not selected/joined in your college." };
                }
            }

            if (_CS.CheckInSelectionList)
            {
                if (!new StudentManager().IsSelectedInCourse(STDINFO.Student_ID, _CS.Course_ID, printProgramme))
                {
                    return new ResponseData() { ErrorMessage = $"Student is not selected/joined in {_CS.CourseFullName}." };
                }
            }

            if (_CS.CheckCourseApplied)
            {
                if ((NewSelectedCombination?.PrintProgramme == null ? printProgramme : NewSelectedCombination.PrintProgramme) == PrintProgramme.IH || (NewSelectedCombination?.PrintProgramme == null ? printProgramme : NewSelectedCombination.PrintProgramme) == PrintProgramme.PG)
                {
                    if (!STDINFO.CoursesApplied.Any(x => x.AppearedInEntrance && x.Course_ID == NewSelectedCombination.Course_ID) && STDINFO.Batch > 2023)
                        return new ResponseData() { ErrorMessage = $"Student has not appeared in entrance for this course : ${_CS.CourseFullName}." };
                }
                else if ((NewSelectedCombination?.PrintProgramme == null ? printProgramme : NewSelectedCombination.PrintProgramme) == PrintProgramme.UG)
                {

                    if (!STDINFO.CoursesApplied.Any(x => x.Course_ID == NewSelectedCombination.Course_ID) || !STDINFO.CATEntrancePoints.HasValue)
                        return new ResponseData() { ErrorMessage = $"Student has not applied for course : {_CS.CourseFullName}." };

                }

            }



            if (_CS.CheckResult)
            {
                if (new ResultManager().ResultCreated(printProgramme, _CS.Semester, STDINFO.Student_ID))
                    return new ResponseData() { ErrorMessage = $"Result already created" };
            }

            #region check for intack capacity
            if (NewSelectedCombination != null && _CS.ValidateByIntakeCapacity)
            {
                ADMCollegeCourseMapping aDMCollegeCourseMapping = new CourseDB().GetCourseCollege(NewSelectedCombination.Course_ID)?.FirstOrDefault(x => x.College_ID == AppUserHelper.College_ID);
                if (aDMCollegeCourseMapping == null)
                    return new ResponseData() { ErrorMessage = "Course not found in your college." };

                int totalIntackCapacity = aDMCollegeCourseMapping.MeritBasislInTackCapacity + aDMCollegeCourseMapping.SelfFinancedInTackCapacity;
                int totalAcceptedFormForCourse = new CombinationSettingManager().GetTotalStudentsAlloted(NewSelectedCombination.Course_ID, (Guid)AppUserHelper.College_ID, STDINFO.Batch, NewSelectedCombination.PrintProgramme);
                if (totalIntackCapacity > 0 && totalAcceptedFormForCourse >= totalIntackCapacity)
                    return new ResponseData() { ErrorMessage = $"Intake Capacity ({totalIntackCapacity}) reached for this course" };

                if (!new CombinationManager().CombinationExistsInCollege(AppUserHelper.College_ID.Value, NewSelectedCombination.Combination_ID, NewSelectedCombination.Course_ID, NewSelectedCombination.Semester))
                    return new ResponseData() { ErrorMessage = "Subject combination does not exist in this college." };

                var _Comb = new CombinationManager().GetCombinationByID(NewSelectedCombination.Combination_ID);
                if (_Comb == null || _Comb.SubjectsDetails.IsNullOrEmpty())
                    return new ResponseData() { ErrorMessage = "Subject combination not found." };
                NewSelectedCombination.Subjects = _Comb.SubjectsDetails;
                _SUB = _SUB.IsNullOrEmpty() ? NewSelectedCombination.Subjects : _SUB;
            }
            #endregion

            if (!STDINFO.AcceptCollege_ID.IsNullOrEmpty() && STDINFO.AcceptCollege_ID != AppUserHelper.College_ID)
                return new ResponseData() { ErrorMessage = "Student belongs to " + STDINFO.AcceptCollegeID };

            var param = new Parameters()
            {
                Filters = new List<SearchFilter>()
            { new SearchFilter() {
                Column = "Semester", Operator = SQLOperator.EqualTo, Value = _CS.Semester.ToString() },
                new SearchFilter() { Column = "Subject_ID",
                    Value = string.Join(",", _SUB.Select(x => x.Subject_ID).ToList()),
                    Operator = SQLOperator.In } }
            };

            _SUB = new SubjectsManager().GetAllSubjects(param);
            if (_SUB.IsNullOrEmpty())
                return new ResponseData() { ErrorMessage = $"Combination / Subjects not found" };

            if (STDINFO.AcceptCollege_ID == Guid.Parse("9D03A374-4398-4A48-BE2A-FD9911EC6F82"))
            {
                List<Guid> courses = new List<Guid>();
                if (STDINFO.SelectedCombinations.IsNotNullOrEmpty())
                {
                    if (STDINFO.SelectedCombinations.Where(x => x.Semester <= 2).IsNotNullOrEmpty() && STDINFO.SelectedCombinations.Where(x => x.Semester <= 2).Select(x => x.Course_ID).Distinct().Count() > 1)
                        return new ResponseData() { ErrorMessage = $"Miss match in course combination" };
                    if (STDINFO.SelectedCombinations.Where(x => x.Semester >= 3).IsNotNullOrEmpty() && STDINFO.SelectedCombinations.Where(x => x.Semester >= 3).Select(x => x.Course_ID).Distinct().Count() > 1)
                        return new ResponseData() { ErrorMessage = $"Miss match in course combination" };
                }

            }
            else if (STDINFO.SelectedCombinations.IsNotNullOrEmpty() && STDINFO.SelectedCombinations.Select(x => x.Course_ID).Distinct().Count() > 1)
                return new ResponseData() { ErrorMessage = $"Miss match in course combination" };

            if (_CS.CombinationSettingStructure.ValidateByTotalNumberOfSubjects && _SUB.Count != _CS.CombinationSettingStructure.TotalNumberOfSubjects)
                return new ResponseData() { ErrorMessage = $"Miss match in total number of subjects. Total number of subjects should be {_CS.CombinationSettingStructure.TotalNumberOfSubjects}" };

            if (_CS.CombinationSettingStructure.ValidateByTotalSubjectCredits && _CS.CombinationSettingStructure.TotalSubjectCredits != _SUB.Sum(x => x.TotalCredit))
                return new ResponseData() { ErrorMessage = $"Miss match in total number of credits. Total number of credits should be {_CS.CombinationSettingStructure.TotalSubjectCredits}" };

            List<ADMSubjectMaster> _S;

            #region Core
            if (_CS.CombinationSettingStructure.ValidateByCore_Credit)
                if (_SUB.IsNullOrEmpty() || _SUB.Where(x => x.SubjectType == SubjectType.Core)?.Sum(x => x.TotalCredit) != _CS.CombinationSettingStructure.Core_Credit)
                    return new ResponseData() { ErrorMessage = $"Miss match in total number of {SubjectType.Core.ToString()} credits. Total number of {SubjectType.Core.ToString()} credits should be {_CS.CombinationSettingStructure.Core_Credit}" };

            if (_CS.CombinationSettingStructure.ValidateByCore_Count)
                if (_SUB.IsNullOrEmpty() || _SUB.Where(x => x.SubjectType == SubjectType.Core)?.Count() != _CS.CombinationSettingStructure.Core_Count)
                    return new ResponseData() { ErrorMessage = $"Miss match in total number of {SubjectType.Core.ToString()} subjects. Total number of {SubjectType.Core.ToString()} should be {_CS.CombinationSettingStructure.Core_Count}" };

            #endregion

            #region DCE_DSE
            if (_CS.CombinationSettingStructure.ValidateByDCE_DSE_Credit)
                if (_SUB.IsNullOrEmpty() || _SUB.Where(x => x.SubjectType == SubjectType.DCE || x.SubjectType == SubjectType.DSE)?.Sum(x => x.TotalCredit) != _CS.CombinationSettingStructure.DCE_DSE_Credit)
                    return new ResponseData() { ErrorMessage = $"Miss match in total number of {SubjectType.DCE.ToString()}/{SubjectType.DSE.ToString()} credits. Total number of {SubjectType.DCE.ToString()}/{SubjectType.DSE.ToString()} credits should be {_CS.CombinationSettingStructure.DCE_DSE_Credit}" };

            if (_CS.CombinationSettingStructure.ValidateByDCE_DSE_Count)
                if (_SUB.IsNullOrEmpty() || _SUB.Where(x => x.SubjectType == SubjectType.DCE || x.SubjectType == SubjectType.DSE)?.Count() != _CS.CombinationSettingStructure.DCE_DSE_Count)
                    return new ResponseData() { ErrorMessage = $"Miss match in total number of {SubjectType.DCE.ToString()}/{SubjectType.DSE.ToString()} subjects. Total number of {SubjectType.DCE.ToString()}/{SubjectType.DSE.ToString()} subjects should be {_CS.CombinationSettingStructure.DCE_DSE_Count}" };
            #endregion

            #region GE_OE
            if (_CS.CombinationSettingStructure.ValidateByGE_OE_Credit)
                if (_SUB.IsNullOrEmpty() || _SUB.Where(x => x.SubjectType == SubjectType.GE || x.SubjectType == SubjectType.OE)?.Sum(x => x.TotalCredit) != _CS.CombinationSettingStructure.GE_OE_Credit)
                    return new ResponseData() { ErrorMessage = $"Miss match in total number of {SubjectType.GE.ToString()}/{SubjectType.OE.ToString()} credits. Total number of {SubjectType.GE.ToString()}/{SubjectType.OE.ToString()} credits should be {_CS.CombinationSettingStructure.GE_OE_Credit}" };

            if (_CS.CombinationSettingStructure.ValidateByGE_OE_Count)
                if (_SUB.IsNullOrEmpty() || _SUB.Where(x => x.SubjectType == SubjectType.GE || x.SubjectType == SubjectType.OE)?.Count() != _CS.CombinationSettingStructure.GE_OE_Count)
                    return new ResponseData() { ErrorMessage = $"Miss match in total number of {SubjectType.GE.ToString()}/{SubjectType.OE.ToString()} subjects. Total number of {SubjectType.GE.ToString()}/{SubjectType.OE.ToString()} subjects should be {_CS.CombinationSettingStructure.GE_OE_Count}" };

            _S = _SUB?.Where(x => x.SubjectType == SubjectType.GE || x.SubjectType == SubjectType.OE)?.ToList();
            if (_S.IsNotNullOrEmpty())
                foreach (var _GE_OE in _S)
                {
                    if (_GE_OE.SubjectCapacitySet)
                    {
                        int NumberOfGE = new AssignCombinationManager().GetSubjectCount(printProgramme, _CS.Semester, _GE_OE.Subject_ID, SubjectType.GE, STDINFO?.SelectedCombinations?.FirstOrDefault()?.SemesterBatch ?? 0, STDINFO.AcceptCollege_ID ?? Guid.Empty);
                        if (NumberOfGE >= _GE_OE.SubjectCapacity)
                        {
                            if (AppUserHelper.AppUsercompact.UserRoles.Any(x => x != AppRoles.Student))
                                ErrorMessage += $"Maximum number of Generic Elective Subject has been reached - {NumberOfGE}. Please choose another subject.";
                            else
                                return new ResponseData() { ErrorMessage = $"Maximum number of Generic Elective Subject has been reached. Please choose another subject." };
                        }
                    }
                }
            #endregion

            #region SEC
            if (_CS.CombinationSettingStructure.ValidateBySEC_Credit)
                if (_SUB.IsNullOrEmpty() || _SUB.Where(x => x.SubjectType == SubjectType.SEC)?.Sum(x => x.TotalCredit) != _CS.CombinationSettingStructure.SEC_Credit)
                    return new ResponseData() { ErrorMessage = $"Miss match in total number of {SubjectType.SEC.ToString()} credits. Total number of {SubjectType.SEC.ToString()} credits should be {_CS.CombinationSettingStructure.SEC_Credit}" };

            if (_CS.CombinationSettingStructure.ValidateBySEC_Count)
                if (_SUB.IsNullOrEmpty() || _SUB.Where(x => x.SubjectType == SubjectType.SEC)?.Count() != _CS.CombinationSettingStructure.SEC_Count)
                    return new ResponseData() { ErrorMessage = $"Miss match in total number of {SubjectType.SEC.ToString()} subjects. Total number of {SubjectType.SEC.ToString()} should be {_CS.CombinationSettingStructure.SEC_Count}" };

            _S = _SUB?.Where(x => x.SubjectType == SubjectType.SEC)?.ToList();
            if (_S.IsNotNullOrEmpty())
                foreach (var _SEC in _S)
                {
                    if (_SEC.SubjectCapacitySet)
                    {
                        int NumberOfGE = new AssignCombinationManager().GetSubjectCount(printProgramme, _CS.Semester, _SEC.Subject_ID, SubjectType.GE, STDINFO.SelectedCombinations.First().SemesterBatch, STDINFO.AcceptCollege_ID);
                        if (NumberOfGE >= _SEC.SubjectCapacity)
                        {
                            if (AppUserHelper.AppUsercompact.UserRoles.Any(x => x != AppRoles.Student))
                                ErrorMessage += $"Maximum number of Skill Subject has been reached - {NumberOfGE}. Please choose another subject.";
                            else
                                return new ResponseData() { ErrorMessage = $"Maximum number of {SubjectType.SEC.ToString()} Subject has been reached. Please choose another subject." };
                        }
                    }
                }
            #endregion

            #region MIL
            if (_CS.CombinationSettingStructure.ValidateByMIL_Credit)
                if (_SUB.IsNullOrEmpty() || _SUB.Where(x => x.SubjectType == SubjectType.MIL)?.Sum(x => x.TotalCredit) != _CS.CombinationSettingStructure.MIL_Credit)
                    return new ResponseData() { ErrorMessage = $"Miss match in total number of {SubjectType.MIL.ToString()} credits. Total number of {SubjectType.MIL.ToString()} credits should be {_CS.CombinationSettingStructure.MIL_Credit}" };

            if (_CS.CombinationSettingStructure.ValidateByMIL_Count)
                if (_SUB.IsNullOrEmpty() || _SUB.Where(x => x.SubjectType == SubjectType.MIL)?.Count() != _CS.CombinationSettingStructure.MIL_Count)
                    return new ResponseData() { ErrorMessage = $"Miss match in total number of {SubjectType.MIL.ToString()} subjects. Total number of {SubjectType.MIL.ToString()} subjects should be {_CS.CombinationSettingStructure.MIL_Count}" };

            #endregion

            #region AE
            if (_CS.CombinationSettingStructure.ValidateByAE_Credit)
                if (_SUB.IsNullOrEmpty() || _SUB.Where(x => x.SubjectType == SubjectType.AE)?.Sum(x => x.TotalCredit) != _CS.CombinationSettingStructure.AE_Credit)
                    return new ResponseData() { ErrorMessage = $"Miss match in total number of {SubjectType.AE.ToString()} credits. Total number of {SubjectType.AE.ToString()} credits should be {_CS.CombinationSettingStructure.AE_Credit}" };

            if (_CS.CombinationSettingStructure.ValidateByAE_Count)
                if (_SUB.IsNullOrEmpty() || _SUB.Where(x => x.SubjectType == SubjectType.AE)?.Count() != _CS.CombinationSettingStructure.AE_Count)
                    return new ResponseData() { ErrorMessage = $"Miss match in total number of {SubjectType.AE.ToString()} subjects. Total number of {SubjectType.AE.ToString()} subjects should be {_CS.CombinationSettingStructure.AE_Count}" };

            #endregion

            #region ValidateEachSubject
            List<ADMSubjectMaster> _NewSubjectOptions = GetSubjectListOptions(_CS, STDINFO, _CS.Semester)?.Item1;
            if (_NewSubjectOptions != null)
            {
                List<ADMSubjectMaster> InvalidSubjects = new List<ADMSubjectMaster>();
                foreach (var item in _SUB)
                    if (_NewSubjectOptions.Any(x => x.Subject_ID == item.Subject_ID)) { } else InvalidSubjects.Add(item);
                if (InvalidSubjects.IsNotNullOrEmpty())
                {
                    string errorMessage = "";
                    foreach (var item in InvalidSubjects)
                    {
                        errorMessage += $"Invalid subject added: {item.CourseFullName}-{item.SubjectFullName}#";
                    }
                    return new ResponseData() { ErrorMessage = errorMessage };
                }
            }
            #endregion

            return new ResponseData() { IsSuccess = true, ResponseObject = _SUB, ErrorMessage = ErrorMessage };
        }
        public ResponseData ValidateByCombinationSettingNEP(PrintProgramme printProgramme, Guid? AssignedCourse_ID, ARGPersonalInformation STDINFO, CombinationSetting _CS
         , List<ADMSubjectMaster> _SUB, ARGSelectedCombination NewSelectedCombination = null)
        {
            string ErrorMessage = "";
            if (_CS == null)
                return new ResponseData() { ErrorMessage = "Combination setting not found" };
            if (STDINFO == null)
                return new ResponseData() { ErrorMessage = "Student not found" };

            if (!AppUserHelper.AppUsercompact.UserRoles.Any(x => x == AppRoles.College) && _CS.Semester == 1)
                return new ResponseData() { ErrorMessage = "Only college can assign combination for lateral entry." };

            //ARGSelectedCombination studentCurrentCombination = STDINFO.SelectedCombinations?.FirstOrDefault(x => x.Semester == _CS.CombinationSettingStructure.Semester);
            if (AppUserHelper.AppUsercompact.UserRoles.Any(x => x == AppRoles.Student))
            {
                if (!_CS.AllowStudentChangeCombination)
                {
                    if (STDINFO.SelectedCombinations.Where(x => x.Semester == _CS.Semester).IsNotNullOrEmpty())
                    {
                        return new ResponseData()
                        {
                            ErrorMessage = "The process of changing combination for student has been closed. " +
                            "Contact college administration section if change in subjects.",
                            IsSuccess = true,
                            ResponseObject = new StudentManager().GetStudentWithCombination(STDINFO.Student_ID, printProgramme, false, _CS.Semester).SelectedCombinations.First().Subjects
                        };
                    }
                    return new ResponseData() { ErrorMessage = "The process of changing combination for student has been closed. Contact college administration section if change in subjects.", IsSuccess = false };

                }
            }
            else if (AppUserHelper.AppUsercompact.UserRoles.Any(x => x == AppRoles.College))
            {
                if (!_CS.AllowCollegeChangeCombination)
                    return new ResponseData() { ErrorMessage = "The process of changing the combination has been closed. Contact cluster university I.T. Cell.", IsSuccess = false };
            }

            if (_CS.CheckAdmissionForm)
            {
                var paymentInfo = new PaymentManager().GetPaymentDetail(STDINFO.Student_ID, _CS.Semester, printProgramme);
                if (paymentInfo == null)
                    return new ResponseData() { ErrorMessage = $"Either admission form not found or fee not paid." };
            }


            if (_CS.CheckSelectionListCollege)
            {
                if (!new StudentManager().IsSelectedInLoggedInCollege(STDINFO.Student_ID, AppUserHelper.College_ID.Value, printProgramme))
                {
                    return new ResponseData() { ErrorMessage = $"Student is not selected/joined in your college." };
                }
            }

            if (_CS.CheckInSelectionList)
            {
                if (!new StudentManager().IsSelectedInCourse(STDINFO.Student_ID, _CS.Course_ID, printProgramme))
                {
                    return new ResponseData() { ErrorMessage = $"Student is not selected/joined in {_CS.CourseFullName}." };
                }
            }

            bool _ExaminationFormSubmitted = false;
            if (_CS.CheckExaminationForm)
            {
                ExaminationCourseCategory courseCategory = new GeneralFunctions().GetExaminationCourseCategory(printProgramme);
                var settingForm = new ExaminationFormDB().GetRegularExamFormDownloadable(courseCategory, _CS.Semester);
                if (settingForm == null)
                    return new ResponseData() { ErrorMessage = $"Examination form setting not found. Contact Cluster University I.T Cell." };
                _ExaminationFormSubmitted = new ExaminationFormManager().ExamFormSubmitted(STDINFO.Student_ID, _CS.Semester, settingForm.Year, printProgramme);
                if (AppUserHelper.AppUsercompact.UserRoles.Any(x => x != AppRoles.Student) && !_ExaminationFormSubmitted)
                    return new ResponseData() { ErrorMessage = $"Either examination form not found or fee not paid." };
            }
            if (_CS.CheckCourseApplied)
            {
                if ((NewSelectedCombination?.PrintProgramme == null ? printProgramme : NewSelectedCombination.PrintProgramme) == PrintProgramme.IH || (NewSelectedCombination?.PrintProgramme == null ? printProgramme : NewSelectedCombination.PrintProgramme) == PrintProgramme.PG)
                {
                    if (!STDINFO.CoursesApplied.Any(x => x.AppearedInEntrance && x.Course_ID == NewSelectedCombination.Course_ID))
                        return new ResponseData() { ErrorMessage = $"Student has not appeared in entrance for this course : ${NewSelectedCombination.CourseCode}." };
                }
                else if ((NewSelectedCombination?.PrintProgramme == null ? printProgramme : NewSelectedCombination.PrintProgramme) == PrintProgramme.UG)
                {
                    Guid Course_IDD = (NewSelectedCombination?.Course_ID == null ? AssignedCourse_ID.Value : NewSelectedCombination.Course_ID);
                    if (!STDINFO.CoursesApplied.Any(x => x.Course_ID == Course_IDD))//|| !STDINFO.CATEntrancePoints.HasValue)
                        return new ResponseData() { ErrorMessage = $"Student has not applied for course : {NewSelectedCombination?.CourseCode}." };
                }

            }

            if (_CS.CheckResult)
            {
                if (new ResultManager().ResultCreated(printProgramme, _CS.Semester, STDINFO.Student_ID))
                    return new ResponseData() { ErrorMessage = $"Result already created" };
            }

            #region check for intack capacity
            if (NewSelectedCombination != null && _CS.ValidateByIntakeCapacity)
            {
                ADMCollegeCourseMapping aDMCollegeCourseMapping = new CourseDB().GetCourseCollege(NewSelectedCombination.Course_ID)?.FirstOrDefault(x => x.College_ID == AppUserHelper.College_ID);
                if (aDMCollegeCourseMapping == null)
                    return new ResponseData() { ErrorMessage = "Course not found in your college." };

                int totalIntackCapacity = aDMCollegeCourseMapping.MeritBasislInTackCapacity + aDMCollegeCourseMapping.SelfFinancedInTackCapacity;
                int totalAcceptedFormForCourse = new CombinationSettingManager().GetTotalStudentsAlloted(NewSelectedCombination.Course_ID, (Guid)AppUserHelper.College_ID, STDINFO.Batch, NewSelectedCombination.PrintProgramme);
                if (totalIntackCapacity > 0 && totalAcceptedFormForCourse >= totalIntackCapacity)
                    return new ResponseData() { ErrorMessage = $"Intake Capacity ({totalIntackCapacity}) reached for this course" };

                if (!new CombinationManager().CombinationExistsInCollege(AppUserHelper.College_ID.Value, NewSelectedCombination.Combination_ID, NewSelectedCombination.Course_ID, NewSelectedCombination.Semester))
                    return new ResponseData() { ErrorMessage = "Subject combination does not exist in this college." };

                var _Comb = new CombinationManager().GetCombinationByID(NewSelectedCombination.Combination_ID);
                if (_Comb == null || _Comb.SubjectsDetails.IsNullOrEmpty())
                    return new ResponseData() { ErrorMessage = "Subject combination not found." };
                NewSelectedCombination.Subjects = _Comb.SubjectsDetails;
                _SUB = _SUB.IsNullOrEmpty() ? NewSelectedCombination.Subjects : _SUB;
            }
            #endregion

            if (!STDINFO.AcceptCollege_ID.IsNullOrEmpty() && STDINFO.AcceptCollege_ID != AppUserHelper.College_ID)
                return new ResponseData() { ErrorMessage = "Student belongs to " + STDINFO.AcceptCollegeID };

            var param = new Parameters() { Filters = new List<SearchFilter>() { new SearchFilter() { Column = "Semester", Operator = SQLOperator.EqualTo, Value = _CS.Semester.ToString() }, new SearchFilter() { Column = "Subject_ID", Value = string.Join(",", _SUB.Select(x => x.Subject_ID).ToList()), Operator = SQLOperator.In } } };
            _SUB = new SubjectsManager().GetAllSubjects(param);
            if (_SUB.IsNullOrEmpty())
                return new ResponseData() { ErrorMessage = $"Combination / Subjects not found" };

            if (STDINFO.AcceptCollege_ID == Guid.Parse("9D03A374-4398-4A48-BE2A-FD9911EC6F82"))
            {
                List<Guid> courses = new List<Guid>();
                if (STDINFO.SelectedCombinations.IsNotNullOrEmpty())
                {
                    if (STDINFO.SelectedCombinations.Where(x => x.Semester <= 2).IsNotNullOrEmpty() && STDINFO.SelectedCombinations.Where(x => x.Semester <= 2).Select(x => x.Course_ID).Distinct().Count() > 1)
                        return new ResponseData() { ErrorMessage = $"Miss match in course combination" };
                    if (STDINFO.SelectedCombinations.Where(x => x.Semester >= 3).IsNotNullOrEmpty() && STDINFO.SelectedCombinations.Where(x => x.Semester >= 3).Select(x => x.Course_ID).Distinct().Count() > 1)
                        return new ResponseData() { ErrorMessage = $"Miss match in course combination" };
                }

            }
            else if (STDINFO.SelectedCombinations.IsNotNullOrEmpty() && STDINFO.SelectedCombinations.Select(x => x.Course_ID).Distinct().Count() > 1)
                return new ResponseData() { ErrorMessage = $"Miss match in course combination" };

            if (_CS.CombinationSettingStructure.ValidateByTotalNumberOfSubjects && _SUB.Count != _CS.CombinationSettingStructure.TotalNumberOfSubjects)
                return new ResponseData() { ErrorMessage = $"Miss match in total number of subjects. Total number of subjects should be {_CS.CombinationSettingStructure.TotalNumberOfSubjects}" };

            if (_CS.CombinationSettingStructure.ValidateByTotalSubjectCredits && _CS.CombinationSettingStructure.TotalSubjectCredits != _SUB.Sum(x => x.TotalCredit))
                return new ResponseData() { ErrorMessage = $"Miss match in total number of credits. Total number of credits should be {_CS.CombinationSettingStructure.TotalSubjectCredits}" };

            List<ADMSubjectMaster> _S;

            #region Core
            if (_CS.CombinationSettingStructure.ValidateByCore_Credit)
                if (_SUB.IsNullOrEmpty() || _SUB.Where(x => x.SubjectType == SubjectType.Core)?.Sum(x => x.TotalCredit) != _CS.CombinationSettingStructure.Core_Credit)
                    return new ResponseData() { ErrorMessage = $"Miss match in total number of {SubjectType.Core.ToString()} credits. Total number of {SubjectType.Core.ToString()} credits should be {_CS.CombinationSettingStructure.Core_Credit}" };

            if (_CS.CombinationSettingStructure.ValidateByCore_Count)
                if (_SUB.IsNullOrEmpty() || _SUB.Where(x => x.SubjectType == SubjectType.Core)?.Count() != _CS.CombinationSettingStructure.Core_Count)
                    return new ResponseData() { ErrorMessage = $"Miss match in total number of {SubjectType.Core.ToString()} subjects. Total number of {SubjectType.Core.ToString()} should be {_CS.CombinationSettingStructure.Core_Count}" };

            #endregion

            #region DCE_DSE
            if (_CS.CombinationSettingStructure.ValidateByDCE_DSE_Credit)
                if (_SUB.IsNullOrEmpty() || _SUB.Where(x => x.SubjectType == SubjectType.DCE || x.SubjectType == SubjectType.DSE)?.Sum(x => x.TotalCredit) != _CS.CombinationSettingStructure.DCE_DSE_Credit)
                    return new ResponseData() { ErrorMessage = $"Miss match in total number of {SubjectType.DCE.ToString()}/{SubjectType.DSE.ToString()} credits. Total number of {SubjectType.DCE.ToString()}/{SubjectType.DSE.ToString()} credits should be {_CS.CombinationSettingStructure.DCE_DSE_Credit}" };

            if (_CS.CombinationSettingStructure.ValidateByDCE_DSE_Count)
                if (_SUB.IsNullOrEmpty() || _SUB.Where(x => x.SubjectType == SubjectType.DCE || x.SubjectType == SubjectType.DSE)?.Count() != _CS.CombinationSettingStructure.DCE_DSE_Count)
                    return new ResponseData() { ErrorMessage = $"Miss match in total number of {SubjectType.DCE.ToString()}/{SubjectType.DSE.ToString()} subjects. Total number of {SubjectType.DCE.ToString()}/{SubjectType.DSE.ToString()} subjects should be {_CS.CombinationSettingStructure.DCE_DSE_Count}" };
            #endregion

            #region GE_OE
            if (_CS.CombinationSettingStructure.ValidateByGE_OE_Credit)
                if (_SUB.IsNullOrEmpty() || _SUB.Where(x => x.SubjectType == SubjectType.GE || x.SubjectType == SubjectType.OE)?.Sum(x => x.TotalCredit) != _CS.CombinationSettingStructure.GE_OE_Credit)
                    return new ResponseData() { ErrorMessage = $"Miss match in total number of {SubjectType.GE.ToString()}/{SubjectType.OE.ToString()} credits. Total number of {SubjectType.GE.ToString()}/{SubjectType.OE.ToString()} credits should be {_CS.CombinationSettingStructure.GE_OE_Credit}" };

            if (_CS.CombinationSettingStructure.ValidateByGE_OE_Count)
                if (_SUB.IsNullOrEmpty() || _SUB.Where(x => x.SubjectType == SubjectType.GE || x.SubjectType == SubjectType.OE)?.Count() != _CS.CombinationSettingStructure.GE_OE_Count)
                    return new ResponseData() { ErrorMessage = $"Miss match in total number of {SubjectType.GE.ToString()}/{SubjectType.OE.ToString()} subjects. Total number of {SubjectType.GE.ToString()}/{SubjectType.OE.ToString()} subjects should be {_CS.CombinationSettingStructure.GE_OE_Count}" };

            _S = _SUB?.Where(x => x.SubjectType == SubjectType.GE || x.SubjectType == SubjectType.OE)?.ToList();
            if (_S.IsNotNullOrEmpty())
                foreach (var _GE_OE in _S)
                {
                    if (_GE_OE.SubjectCapacitySet)
                    {
                        int NumberOfGE = new AssignCombinationManager().GetSubjectCount(printProgramme, _CS.Semester, _GE_OE.Subject_ID, SubjectType.GE, STDINFO?.SelectedCombinations?.FirstOrDefault()?.SemesterBatch ?? 0, STDINFO.AcceptCollege_ID ?? Guid.Empty);
                        if (NumberOfGE >= _GE_OE.SubjectCapacity)
                        {
                            if (AppUserHelper.AppUsercompact.UserRoles.Any(x => x != AppRoles.Student))
                                ErrorMessage += $"Maximum number of Generic Elective Subject has been reached - {NumberOfGE}. Please choose another subject.";
                            else
                                return new ResponseData() { ErrorMessage = $"Maximum number of Generic Elective Subject has been reached. Please choose another subject." };
                        }
                    }
                }
            #endregion

            #region SEC
            if (_CS.CombinationSettingStructure.ValidateBySEC_Credit)
                if (_SUB.IsNullOrEmpty() || _SUB.Where(x => x.SubjectType == SubjectType.SEC)?.Sum(x => x.TotalCredit) != _CS.CombinationSettingStructure.SEC_Credit)
                    return new ResponseData() { ErrorMessage = $"Miss match in total number of {SubjectType.SEC.ToString()} credits. Total number of {SubjectType.SEC.ToString()} credits should be {_CS.CombinationSettingStructure.SEC_Credit}" };

            if (_CS.CombinationSettingStructure.ValidateBySEC_Count)
                if (_SUB.IsNullOrEmpty() || _SUB.Where(x => x.SubjectType == SubjectType.SEC)?.Count() != _CS.CombinationSettingStructure.SEC_Count)
                    return new ResponseData() { ErrorMessage = $"Miss match in total number of {SubjectType.SEC.ToString()} subjects. Total number of {SubjectType.SEC.ToString()} should be {_CS.CombinationSettingStructure.SEC_Count}" };

            _S = _SUB?.Where(x => x.SubjectType == SubjectType.SEC)?.ToList();
            if (_S.IsNotNullOrEmpty())
                foreach (var _SEC in _S)
                {
                    if (_SEC.SubjectCapacitySet)
                    {
                        int NumberOfGE = new AssignCombinationManager().GetSubjectCount(printProgramme, _CS.Semester, _SEC.Subject_ID, SubjectType.GE, STDINFO.SelectedCombinations.First().SemesterBatch, STDINFO.AcceptCollege_ID);
                        if (NumberOfGE >= _SEC.SubjectCapacity)
                        {
                            if (AppUserHelper.AppUsercompact.UserRoles.Any(x => x != AppRoles.Student))
                                ErrorMessage += $"Maximum number of Skill Subject has been reached - {NumberOfGE}. Please choose another subject.";
                            else
                                return new ResponseData() { ErrorMessage = $"Maximum number of {SubjectType.SEC.ToString()} Subject has been reached. Please choose another subject." };
                        }
                    }
                }
            #endregion

            #region MIL
            if (_CS.CombinationSettingStructure.ValidateByMIL_Credit)
                if (_SUB.IsNullOrEmpty() || _SUB.Where(x => x.SubjectType == SubjectType.MIL)?.Sum(x => x.TotalCredit) != _CS.CombinationSettingStructure.MIL_Credit)
                    return new ResponseData() { ErrorMessage = $"Miss match in total number of {SubjectType.MIL.ToString()} credits. Total number of {SubjectType.MIL.ToString()} credits should be {_CS.CombinationSettingStructure.MIL_Credit}" };

            if (_CS.CombinationSettingStructure.ValidateByMIL_Count)
                if (_SUB.IsNullOrEmpty() || _SUB.Where(x => x.SubjectType == SubjectType.MIL)?.Count() != _CS.CombinationSettingStructure.MIL_Count)
                    return new ResponseData() { ErrorMessage = $"Miss match in total number of {SubjectType.MIL.ToString()} subjects. Total number of {SubjectType.MIL.ToString()} subjects should be {_CS.CombinationSettingStructure.MIL_Count}" };

            #endregion

            #region AE
            if (_CS.CombinationSettingStructure.ValidateByAE_Credit)
                if (_SUB.IsNullOrEmpty() || _SUB.Where(x => x.SubjectType == SubjectType.AE)?.Sum(x => x.TotalCredit) != _CS.CombinationSettingStructure.AE_Credit)
                    return new ResponseData() { ErrorMessage = $"Miss match in total number of {SubjectType.AE.ToString()} credits. Total number of {SubjectType.AE.ToString()} credits should be {_CS.CombinationSettingStructure.AE_Credit}" };

            if (_CS.CombinationSettingStructure.ValidateByAE_Count)
                if (_SUB.IsNullOrEmpty() || _SUB.Where(x => x.SubjectType == SubjectType.AE)?.Count() != _CS.CombinationSettingStructure.AE_Count)
                    return new ResponseData() { ErrorMessage = $"Miss match in total number of {SubjectType.AE.ToString()} subjects. Total number of {SubjectType.AE.ToString()} subjects should be {_CS.CombinationSettingStructure.AE_Count}" };

            #endregion
            #region Major
            if (_CS.CombinationSettingStructure.ValidateByMajor_Credit)
                if (_SUB.IsNullOrEmpty() || _SUB.Where(x => x.SubjectType == SubjectType.Major)?.Sum(x => x.TotalCredit) != _CS.CombinationSettingStructure.Major_Credit)
                    return new ResponseData() { ErrorMessage = $"Miss match in total number of {SubjectType.Major.ToString()} credits. Total number of {SubjectType.Major.ToString()} credits should be {_CS.CombinationSettingStructure.Major_Credit}" };

            if (_CS.CombinationSettingStructure.ValidateByMajor_Count)
                if (_SUB.IsNullOrEmpty() || _SUB.Where(x => x.SubjectType == SubjectType.Major)?.Count() != _CS.CombinationSettingStructure.Major_Count)
                    return new ResponseData() { ErrorMessage = $"Miss match in total number of {SubjectType.Major.ToString()} subjects. Total number of {SubjectType.Major.ToString()} subjects should be {_CS.CombinationSettingStructure.Major_Count}" };
            #endregion
            #region MID
            if (_CS.CombinationSettingStructure.ValidateByMinorInterDisciplinary_Credit)
                if (_SUB.IsNullOrEmpty() || _SUB.Where(x => x.SubjectType == SubjectType.MID)?.Sum(x => x.TotalCredit) != _CS.CombinationSettingStructure.MinorInterDisciplinary_Credit)
                    return new ResponseData() { ErrorMessage = $"Miss match in total number of {SubjectType.MID.ToString()} credits. Total number of {SubjectType.MID.ToString()} credits should be {_CS.CombinationSettingStructure.MinorInterDisciplinary_Credit}" };

            if (_CS.CombinationSettingStructure.ValidateByMinorInterDisciplinary_Count)
                if (_SUB.IsNullOrEmpty() || _SUB.Where(x => x.SubjectType == SubjectType.MID)?.Count() != _CS.CombinationSettingStructure.MinorInterDisciplinary_Count)
                    return new ResponseData() { ErrorMessage = $"Miss match in total number of {SubjectType.MID.ToString()} subjects. Total number of {SubjectType.MID.ToString()} subjects should be {_CS.CombinationSettingStructure.MinorInterDisciplinary_Count}" };
            #endregion
            #region MinorVocational
            if (_CS.CombinationSettingStructure.ValidateByMinorVocational_Credit)
                if (_SUB.IsNullOrEmpty() || _SUB.Where(x => x.SubjectType == SubjectType.MVoc)?.Sum(x => x.TotalCredit) != _CS.CombinationSettingStructure.MinorVocational_Credit)
                    return new ResponseData() { ErrorMessage = $"Miss match in total number of {SubjectType.MVoc.ToString()} credits. Total number of {SubjectType.MVoc.ToString()} credits should be {_CS.CombinationSettingStructure.MinorVocational_Credit}" };

            if (_CS.CombinationSettingStructure.ValidateByMinorVocational_Count)
                if (_SUB.IsNullOrEmpty() || _SUB.Where(x => x.SubjectType == SubjectType.MVoc)?.Count() != _CS.CombinationSettingStructure.MinorVocational_Count)
                    return new ResponseData() { ErrorMessage = $"Miss match in total number of {SubjectType.MVoc.ToString()} subjects. Total number of {SubjectType.MVoc.ToString()} subjects should be {_CS.CombinationSettingStructure.MinorVocational_Count}" };
            #endregion
            #region MDisc
            if (_CS.CombinationSettingStructure.ValidateByMultiDisciplinary_Credit)
                if (_SUB.IsNullOrEmpty() || _SUB.Where(x => x.SubjectType == SubjectType.MDisc)?.Sum(x => x.TotalCredit) != _CS.CombinationSettingStructure.MultiDisciplinary_Credit)
                    return new ResponseData() { ErrorMessage = $"Miss match in total number of {SubjectType.Major.ToString()} credits. Total number of {SubjectType.MDisc.ToString()} credits should be {_CS.CombinationSettingStructure.MultiDisciplinary_Credit}" };

            if (_CS.CombinationSettingStructure.ValidateByMultiDisciplinary_Count)
                if (_SUB.IsNullOrEmpty() || _SUB.Where(x => x.SubjectType == SubjectType.MDisc)?.Count() != _CS.CombinationSettingStructure.MultiDisciplinary_Count)
                    return new ResponseData() { ErrorMessage = $"Miss match in total number of {SubjectType.Major.ToString()} subjects. Total number of {SubjectType.MDisc.ToString()} subjects should be {_CS.CombinationSettingStructure.MultiDisciplinary_Count}" };
            #endregion
            #region Internship
            if (_CS.CombinationSettingStructure.ValidateByInternship_Credit)
                if (_SUB.IsNullOrEmpty() || _SUB.Where(x => x.SubjectType == SubjectType.Internship)?.Sum(x => x.TotalCredit) != _CS.CombinationSettingStructure.Internship_Credit)
                    return new ResponseData() { ErrorMessage = $"Miss match in total number of {SubjectType.Major.ToString()} credits. Total number of {SubjectType.MDisc.ToString()} credits should be {_CS.CombinationSettingStructure.Internship_Credit}" };

            if (_CS.CombinationSettingStructure.ValidateByInternship_Count)
                if (_SUB.IsNullOrEmpty() || _SUB.Where(x => x.SubjectType == SubjectType.Internship)?.Count() != _CS.CombinationSettingStructure.Internship_Count)
                    return new ResponseData() { ErrorMessage = $"Miss match in total number of {SubjectType.Major.ToString()} subjects. Total number of {SubjectType.MDisc.ToString()} subjects should be {_CS.CombinationSettingStructure.Internship_Count}" };
            #endregion
            #region Research
            if (_CS.CombinationSettingStructure.ValidateByResearch_Credit)
                if (_SUB.IsNullOrEmpty() || _SUB.Where(x => x.SubjectType == SubjectType.Research)?.Sum(x => x.TotalCredit) != _CS.CombinationSettingStructure.Research_Credit)
                    return new ResponseData() { ErrorMessage = $"Miss match in total number of {SubjectType.Major.ToString()} credits. Total number of {SubjectType.Research.ToString()} credits should be {_CS.CombinationSettingStructure.Research_Credit}" };

            if (_CS.CombinationSettingStructure.ValidateByResearch_Count)
                if (_SUB.IsNullOrEmpty() || _SUB.Where(x => x.SubjectType == SubjectType.Research)?.Count() != _CS.CombinationSettingStructure.Research_Count)
                    return new ResponseData() { ErrorMessage = $"Miss match in total number of {SubjectType.Major.ToString()} subjects. Total number of {SubjectType.Research.ToString()} subjects should be {_CS.CombinationSettingStructure.Research_Count}" };
            #endregion
            #region Seminar
            if (_CS.CombinationSettingStructure.ValidateBySeminar_Credit)
                if (_SUB.IsNullOrEmpty() || _SUB.Where(x => x.SubjectType == SubjectType.Seminar)?.Sum(x => x.TotalCredit) != _CS.CombinationSettingStructure.Seminar_Credit)
                    return new ResponseData() { ErrorMessage = $"Miss match in total number of {SubjectType.Major.ToString()} credits. Total number of {SubjectType.Seminar.ToString()} credits should be {_CS.CombinationSettingStructure.Seminar_Credit}" };

            if (_CS.CombinationSettingStructure.ValidateBySeminar_Count)
                if (_SUB.IsNullOrEmpty() || _SUB.Where(x => x.SubjectType == SubjectType.Seminar)?.Count() != _CS.CombinationSettingStructure.Seminar_Count)
                    return new ResponseData() { ErrorMessage = $"Miss match in total number of {SubjectType.Major.ToString()} subjects. Total number of {SubjectType.Seminar.ToString()} subjects should be {_CS.CombinationSettingStructure.Seminar_Count}" };
            #endregion
            #region VAC
            if (_CS.CombinationSettingStructure.ValidateByVAC_Credit)
                if (_SUB.IsNullOrEmpty() || _SUB.Where(x => x.SubjectType == SubjectType.VAC)?.Sum(x => x.TotalCredit) != _CS.CombinationSettingStructure.VAC_Credit)
                    return new ResponseData() { ErrorMessage = $"Miss match in total number of {SubjectType.Major.ToString()} credits. Total number of {SubjectType.VAC.ToString()} credits should be {_CS.CombinationSettingStructure.VAC_Credit}" };

            if (_CS.CombinationSettingStructure.ValidateByVAC_Count)
                if (_SUB.IsNullOrEmpty() || _SUB.Where(x => x.SubjectType == SubjectType.VAC)?.Count() != _CS.CombinationSettingStructure.VAC_Count)
                    return new ResponseData() { ErrorMessage = $"Miss match in total number of {SubjectType.Major.ToString()} subjects. Total number of {SubjectType.VAC.ToString()} subjects should be {_CS.CombinationSettingStructure.VAC_Count}" };
            #endregion
            #region ValidateEachSubject
            List<ADMSubjectMaster> _NewSubjectOptions = GetSubjectListOptions(_CS, STDINFO, _CS.Semester)?.Item1;
            if (_NewSubjectOptions != null)
            {
                List<ADMSubjectMaster> InvalidSubjects = new List<ADMSubjectMaster>();
                foreach (var item in _SUB)
                    if (_NewSubjectOptions.Any(x => x.Subject_ID == item.Subject_ID)) { } else InvalidSubjects.Add(item);
                if (InvalidSubjects.IsNotNullOrEmpty())
                {
                    string errorMessage = "";
                    foreach (var item in InvalidSubjects)
                    {
                        errorMessage += $"Invalid subject added: {item.CourseFullName}-{item.SubjectFullName}#";
                    }
                    return new ResponseData() { ErrorMessage = errorMessage };
                }
            }
            #endregion

            return new ResponseData() { IsSuccess = true, ResponseObject = _SUB, ErrorMessage = ErrorMessage };
        }



        public ResponseData ValidateByCombinationSettingNEPCUET(PrintProgramme printProgramme, Guid? AssignedCourse_ID, ARGPersonalInformation STDINFO, CombinationSetting _CS
         , List<ADMSubjectMaster> _SUB, ARGSelectedCombination NewSelectedCombination = null)
        {
            string ErrorMessage = "";
            if (_CS == null)
                return new ResponseData() { ErrorMessage = "Combination setting not found" };
            if (STDINFO == null)
                return new ResponseData() { ErrorMessage = "Student not found" };

            //ARGSelectedCombination studentCurrentCombination = STDINFO.SelectedCombinations?.FirstOrDefault(x => x.Semester == _CS.CombinationSettingStructure.Semester);
            if (AppUserHelper.AppUsercompact.UserRoles.Any(x => x == AppRoles.Student))
            {
                if (!_CS.AllowStudentChangeCombination)
                {
                    if (STDINFO.SelectedCombinations.Where(x => x.Semester == _CS.Semester).IsNotNullOrEmpty())
                    {
                        return new ResponseData()
                        {
                            ErrorMessage = "The process of changing combination for student has been closed. " +
                            "Contact college administration section if change in subjects.",
                            IsSuccess = true,
                            ResponseObject = new StudentManager().GetStudentWithCombination(STDINFO.Student_ID, printProgramme, false, _CS.Semester).SelectedCombinations.First().Subjects
                        };
                    }
                    return new ResponseData() { ErrorMessage = "The process of changing combination for student has been closed. Contact college administration section if change in subjects.", IsSuccess = false };

                }
            }
            else if (AppUserHelper.AppUsercompact.UserRoles.Any(x => x == AppRoles.College))
            {
                if (!_CS.AllowCollegeChangeCombination)
                    return new ResponseData() { ErrorMessage = "The process of changing the combination has been closed. Contact cluster university I.T. Cell.", IsSuccess = false };
            }

            if (_CS.CheckAdmissionForm)
            {
                var paymentInfo = new PaymentManager().GetPaymentDetail(STDINFO.Student_ID, _CS.Semester, printProgramme);
                if (paymentInfo == null)
                    return new ResponseData() { ErrorMessage = $"Either admission form not found or fee not paid." };
            }


            bool _ExaminationFormSubmitted = false;
            if (_CS.CheckExaminationForm)
            {
                ExaminationCourseCategory courseCategory = new GeneralFunctions().GetExaminationCourseCategory(printProgramme);
                var settingForm = new ExaminationFormDB().GetRegularExamFormDownloadable(courseCategory, _CS.Semester);
                if (settingForm == null)
                    return new ResponseData() { ErrorMessage = $"Examination form setting not found. Contact Cluster University I.T Cell." };
                _ExaminationFormSubmitted = new ExaminationFormManager().ExamFormSubmitted(STDINFO.Student_ID, _CS.Semester, settingForm.Year, printProgramme);
                if (AppUserHelper.AppUsercompact.UserRoles.Any(x => x != AppRoles.Student) && !_ExaminationFormSubmitted)
                    return new ResponseData() { ErrorMessage = $"Either examination form not found or fee not paid." };
            }
            if (_CS.CheckCourseApplied)
            {
                if ((NewSelectedCombination?.PrintProgramme == null ? printProgramme : NewSelectedCombination.PrintProgramme) == PrintProgramme.IH || (NewSelectedCombination?.PrintProgramme == null ? printProgramme : NewSelectedCombination.PrintProgramme) == PrintProgramme.PG)
                {
                    if (!STDINFO.CoursesApplied.Any(x => x.AppearedInEntrance && x.Course_ID == NewSelectedCombination.Course_ID))
                        return new ResponseData() { ErrorMessage = $"Student has not appeared in entrance for this course : ${NewSelectedCombination.CourseCode}." };
                }
                else if ((NewSelectedCombination?.PrintProgramme == null ? printProgramme : NewSelectedCombination.PrintProgramme) == PrintProgramme.UG)
                {
                    Guid Course_IDD = (NewSelectedCombination?.Course_ID == null ? AssignedCourse_ID.Value : NewSelectedCombination.Course_ID);
                    if (!STDINFO.CoursesApplied.Any(x => x.Course_ID == Course_IDD))//|| !STDINFO.CATEntrancePoints.HasValue)
                        return new ResponseData() { ErrorMessage = $"Student has not applied for course : {NewSelectedCombination?.CourseCode}." };
                }

            }

            if (_CS.CheckResult)
            {
                if (new ResultManager().ResultCreated(printProgramme, _CS.Semester, STDINFO.Student_ID))
                    return new ResponseData() { ErrorMessage = $"Result already created" };
            }

            #region check for intack capacity
            if (NewSelectedCombination != null && _CS.ValidateByIntakeCapacity)
            {
                ADMCollegeCourseMapping aDMCollegeCourseMapping = new CourseDB().GetCourseCollege(NewSelectedCombination.Course_ID)?.FirstOrDefault(x => x.College_ID == AppUserHelper.College_ID);
                if (aDMCollegeCourseMapping == null)
                    return new ResponseData() { ErrorMessage = "Course not found in your college." };

                int totalIntackCapacity = aDMCollegeCourseMapping.MeritBasislInTackCapacity + aDMCollegeCourseMapping.SelfFinancedInTackCapacity;
                int totalAcceptedFormForCourse = new CombinationSettingManager().GetTotalStudentsAlloted(NewSelectedCombination.Course_ID, (Guid)AppUserHelper.College_ID, STDINFO.Batch, NewSelectedCombination.PrintProgramme);
                if (totalIntackCapacity > 0 && totalAcceptedFormForCourse >= totalIntackCapacity)
                    return new ResponseData() { ErrorMessage = $"Intake Capacity ({totalIntackCapacity}) reached for this course" };

                if (!new CombinationManager().CombinationExistsInCollege(AppUserHelper.College_ID.Value, NewSelectedCombination.Combination_ID, NewSelectedCombination.Course_ID, NewSelectedCombination.Semester))
                    return new ResponseData() { ErrorMessage = "Subject combination does not exist in this college." };

                var _Comb = new CombinationManager().GetCombinationByID(NewSelectedCombination.Combination_ID);
                if (_Comb == null || _Comb.SubjectsDetails.IsNullOrEmpty())
                    return new ResponseData() { ErrorMessage = "Subject combination not found." };
                NewSelectedCombination.Subjects = _Comb.SubjectsDetails;
                _SUB = _SUB.IsNullOrEmpty() ? NewSelectedCombination.Subjects : _SUB;
            }
            #endregion

            if (!STDINFO.AcceptCollege_ID.IsNullOrEmpty() && STDINFO.AcceptCollege_ID != AppUserHelper.College_ID)
                return new ResponseData() { ErrorMessage = "Student belongs to " + STDINFO.AcceptCollegeID };

            var param = new Parameters() { Filters = new List<SearchFilter>() { new SearchFilter() { Column = "Semester", Operator = SQLOperator.EqualTo, Value = _CS.Semester.ToString() }, new SearchFilter() { Column = "Subject_ID", Value = string.Join(",", _SUB.Select(x => x.Subject_ID).ToList()), Operator = SQLOperator.In } } };
            _SUB = new SubjectsManager().GetAllSubjects(param);
            if (_SUB.IsNullOrEmpty())
                return new ResponseData() { ErrorMessage = $"Combination / Subjects not found" };

            if (STDINFO.AcceptCollege_ID == Guid.Parse("9D03A374-4398-4A48-BE2A-FD9911EC6F82"))
            {
                List<Guid> courses = new List<Guid>();
                if (STDINFO.SelectedCombinations.IsNotNullOrEmpty())
                {
                    if (STDINFO.SelectedCombinations.Where(x => x.Semester <= 2).IsNotNullOrEmpty() && STDINFO.SelectedCombinations.Where(x => x.Semester <= 2).Select(x => x.Course_ID).Distinct().Count() > 1)
                        return new ResponseData() { ErrorMessage = $"Miss match in course combination" };
                    if (STDINFO.SelectedCombinations.Where(x => x.Semester >= 3).IsNotNullOrEmpty() && STDINFO.SelectedCombinations.Where(x => x.Semester >= 3).Select(x => x.Course_ID).Distinct().Count() > 1)
                        return new ResponseData() { ErrorMessage = $"Miss match in course combination" };
                }

            }
            else if (STDINFO.SelectedCombinations.IsNotNullOrEmpty() && STDINFO.SelectedCombinations.Select(x => x.Course_ID).Distinct().Count() > 1)
                return new ResponseData() { ErrorMessage = $"Miss match in course combination" };

            if (_CS.CombinationSettingStructure.ValidateByTotalNumberOfSubjects && _SUB.Count != _CS.CombinationSettingStructure.TotalNumberOfSubjects)
                return new ResponseData() { ErrorMessage = $"Miss match in total number of subjects. Total number of subjects should be {_CS.CombinationSettingStructure.TotalNumberOfSubjects}" };

            if (_CS.CombinationSettingStructure.ValidateByTotalSubjectCredits && _CS.CombinationSettingStructure.TotalSubjectCredits != _SUB.Sum(x => x.TotalCredit))
                return new ResponseData() { ErrorMessage = $"Miss match in total number of credits. Total number of credits should be {_CS.CombinationSettingStructure.TotalSubjectCredits}" };

            List<ADMSubjectMaster> _S;

            #region Core
            if (_CS.CombinationSettingStructure.ValidateByCore_Credit)
                if (_SUB.IsNullOrEmpty() || _SUB.Where(x => x.SubjectType == SubjectType.Core)?.Sum(x => x.TotalCredit) != _CS.CombinationSettingStructure.Core_Credit)
                    return new ResponseData() { ErrorMessage = $"Miss match in total number of {SubjectType.Core.ToString()} credits. Total number of {SubjectType.Core.ToString()} credits should be {_CS.CombinationSettingStructure.Core_Credit}" };

            if (_CS.CombinationSettingStructure.ValidateByCore_Count)
                if (_SUB.IsNullOrEmpty() || _SUB.Where(x => x.SubjectType == SubjectType.Core)?.Count() != _CS.CombinationSettingStructure.Core_Count)
                    return new ResponseData() { ErrorMessage = $"Miss match in total number of {SubjectType.Core.ToString()} subjects. Total number of {SubjectType.Core.ToString()} should be {_CS.CombinationSettingStructure.Core_Count}" };

            #endregion

            #region DCE_DSE
            if (_CS.CombinationSettingStructure.ValidateByDCE_DSE_Credit)
                if (_SUB.IsNullOrEmpty() || _SUB.Where(x => x.SubjectType == SubjectType.DCE || x.SubjectType == SubjectType.DSE)?.Sum(x => x.TotalCredit) != _CS.CombinationSettingStructure.DCE_DSE_Credit)
                    return new ResponseData() { ErrorMessage = $"Miss match in total number of {SubjectType.DCE.ToString()}/{SubjectType.DSE.ToString()} credits. Total number of {SubjectType.DCE.ToString()}/{SubjectType.DSE.ToString()} credits should be {_CS.CombinationSettingStructure.DCE_DSE_Credit}" };

            if (_CS.CombinationSettingStructure.ValidateByDCE_DSE_Count)
                if (_SUB.IsNullOrEmpty() || _SUB.Where(x => x.SubjectType == SubjectType.DCE || x.SubjectType == SubjectType.DSE)?.Count() != _CS.CombinationSettingStructure.DCE_DSE_Count)
                    return new ResponseData() { ErrorMessage = $"Miss match in total number of {SubjectType.DCE.ToString()}/{SubjectType.DSE.ToString()} subjects. Total number of {SubjectType.DCE.ToString()}/{SubjectType.DSE.ToString()} subjects should be {_CS.CombinationSettingStructure.DCE_DSE_Count}" };
            #endregion

            #region GE_OE
            if (_CS.CombinationSettingStructure.ValidateByGE_OE_Credit)
                if (_SUB.IsNullOrEmpty() || _SUB.Where(x => x.SubjectType == SubjectType.GE || x.SubjectType == SubjectType.OE)?.Sum(x => x.TotalCredit) != _CS.CombinationSettingStructure.GE_OE_Credit)
                    return new ResponseData() { ErrorMessage = $"Miss match in total number of {SubjectType.GE.ToString()}/{SubjectType.OE.ToString()} credits. Total number of {SubjectType.GE.ToString()}/{SubjectType.OE.ToString()} credits should be {_CS.CombinationSettingStructure.GE_OE_Credit}" };

            if (_CS.CombinationSettingStructure.ValidateByGE_OE_Count)
                if (_SUB.IsNullOrEmpty() || _SUB.Where(x => x.SubjectType == SubjectType.GE || x.SubjectType == SubjectType.OE)?.Count() != _CS.CombinationSettingStructure.GE_OE_Count)
                    return new ResponseData() { ErrorMessage = $"Miss match in total number of {SubjectType.GE.ToString()}/{SubjectType.OE.ToString()} subjects. Total number of {SubjectType.GE.ToString()}/{SubjectType.OE.ToString()} subjects should be {_CS.CombinationSettingStructure.GE_OE_Count}" };

            _S = _SUB?.Where(x => x.SubjectType == SubjectType.GE || x.SubjectType == SubjectType.OE)?.ToList();
            if (_S.IsNotNullOrEmpty())
                foreach (var _GE_OE in _S)
                {
                    if (_GE_OE.SubjectCapacitySet && _GE_OE.SubjectCapacity != null)
                    {
                        int NumberOfGE = new AssignCombinationManager().GetSubjectCount(printProgramme, _CS.Semester, _GE_OE.Subject_ID, SubjectType.GE, STDINFO?.SelectedCombinations?.FirstOrDefault()?.SemesterBatch ?? 0, STDINFO.AcceptCollege_ID ?? Guid.Empty);
                        if (NumberOfGE >= _GE_OE.SubjectCapacity)
                        {
                            if (AppUserHelper.AppUsercompact.UserRoles.Any(x => x != AppRoles.Student))
                                ErrorMessage += $"Maximum number of Generic Elective Subject has been reached - {NumberOfGE}. Please choose another subject.";
                            else
                                return new ResponseData() { ErrorMessage = $"Maximum number of Generic Elective Subject has been reached. Please choose another subject." };
                        }
                    }
                }
            #endregion

            #region SEC
            if (_CS.CombinationSettingStructure.ValidateBySEC_Credit)
                if (_SUB.IsNullOrEmpty() || _SUB.Where(x => x.SubjectType == SubjectType.SEC)?.Sum(x => x.TotalCredit) != _CS.CombinationSettingStructure.SEC_Credit)
                    return new ResponseData() { ErrorMessage = $"Miss match in total number of {SubjectType.SEC.ToString()} credits. Total number of {SubjectType.SEC.ToString()} credits should be {_CS.CombinationSettingStructure.SEC_Credit}" };

            if (_CS.CombinationSettingStructure.ValidateBySEC_Count)
                if (_SUB.IsNullOrEmpty() || _SUB.Where(x => x.SubjectType == SubjectType.SEC)?.Count() != _CS.CombinationSettingStructure.SEC_Count)
                    return new ResponseData() { ErrorMessage = $"Miss match in total number of {SubjectType.SEC.ToString()} subjects. Total number of {SubjectType.SEC.ToString()} should be {_CS.CombinationSettingStructure.SEC_Count}" };

            _S = _SUB?.Where(x => x.SubjectType == SubjectType.SEC)?.ToList();
            if (_S.IsNotNullOrEmpty())
                foreach (var _SEC in _S)
                {
                    if (_SEC.SubjectCapacitySet && _SEC.SubjectCapacity != null)
                    {
                        int NumberOfGE = new AssignCombinationManager().GetSubjectCount(printProgramme, _CS.Semester, _SEC.Subject_ID, SubjectType.GE, STDINFO?.SelectedCombinations?.FirstOrDefault()?.SemesterBatch ?? 0, STDINFO.AcceptCollege_ID ?? Guid.Empty);

                        if (NumberOfGE >= _SEC.SubjectCapacity)
                        {
                            if (AppUserHelper.AppUsercompact.UserRoles.Any(x => x != AppRoles.Student))
                                ErrorMessage += $"Maximum number of Skill Subject has been reached - {NumberOfGE}. Please choose another subject.";
                            else
                                return new ResponseData() { ErrorMessage = $"Maximum number of {SubjectType.SEC.ToString()} Subject has been reached. Please choose another subject." };
                        }
                    }
                }
            #endregion

            #region MIL
            if (_CS.CombinationSettingStructure.ValidateByMIL_Credit)
                if (_SUB.IsNullOrEmpty() || _SUB.Where(x => x.SubjectType == SubjectType.MIL)?.Sum(x => x.TotalCredit) != _CS.CombinationSettingStructure.MIL_Credit)
                    return new ResponseData() { ErrorMessage = $"Miss match in total number of {SubjectType.MIL.ToString()} credits. Total number of {SubjectType.MIL.ToString()} credits should be {_CS.CombinationSettingStructure.MIL_Credit}" };

            if (_CS.CombinationSettingStructure.ValidateByMIL_Count)
                if (_SUB.IsNullOrEmpty() || _SUB.Where(x => x.SubjectType == SubjectType.MIL)?.Count() != _CS.CombinationSettingStructure.MIL_Count)
                    return new ResponseData() { ErrorMessage = $"Miss match in total number of {SubjectType.MIL.ToString()} subjects. Total number of {SubjectType.MIL.ToString()} subjects should be {_CS.CombinationSettingStructure.MIL_Count}" };

            #endregion

            #region AE
            if (_CS.CombinationSettingStructure.ValidateByAE_Credit)
                if (_SUB.IsNullOrEmpty() || _SUB.Where(x => x.SubjectType == SubjectType.AE)?.Sum(x => x.TotalCredit) != _CS.CombinationSettingStructure.AE_Credit)
                    return new ResponseData() { ErrorMessage = $"Miss match in total number of {SubjectType.AE.ToString()} credits. Total number of {SubjectType.AE.ToString()} credits should be {_CS.CombinationSettingStructure.AE_Credit}" };

            if (_CS.CombinationSettingStructure.ValidateByAE_Count)
                if (_SUB.IsNullOrEmpty() || _SUB.Where(x => x.SubjectType == SubjectType.AE)?.Count() != _CS.CombinationSettingStructure.AE_Count)
                    return new ResponseData() { ErrorMessage = $"Miss match in total number of {SubjectType.AE.ToString()} subjects. Total number of {SubjectType.AE.ToString()} subjects should be {_CS.CombinationSettingStructure.AE_Count}" };

            #endregion
            #region Major
            if (_CS.CombinationSettingStructure.ValidateByMajor_Credit)
                if (_SUB.IsNullOrEmpty() || _SUB.Where(x => x.SubjectType == SubjectType.Major)?.Sum(x => x.TotalCredit) != _CS.CombinationSettingStructure.Major_Credit)
                    return new ResponseData() { ErrorMessage = $"Miss match in total number of {SubjectType.Major.ToString()} credits. Total number of {SubjectType.Major.ToString()} credits should be {_CS.CombinationSettingStructure.Major_Credit}" };

            if (_CS.CombinationSettingStructure.ValidateByMajor_Count)
                if (_SUB.IsNullOrEmpty() || _SUB.Where(x => x.SubjectType == SubjectType.Major)?.Count() != _CS.CombinationSettingStructure.Major_Count)
                    return new ResponseData() { ErrorMessage = $"Miss match in total number of {SubjectType.Major.ToString()} subjects. Total number of {SubjectType.Major.ToString()} subjects should be {_CS.CombinationSettingStructure.Major_Count}" };
            #endregion
            #region MID
            if (_CS.CombinationSettingStructure.ValidateByMinorInterDisciplinary_Credit)
                if (_SUB.IsNullOrEmpty() || _SUB.Where(x => x.SubjectType == SubjectType.MID)?.Sum(x => x.TotalCredit) != _CS.CombinationSettingStructure.MinorInterDisciplinary_Credit)
                    return new ResponseData() { ErrorMessage = $"Miss match in total number of {SubjectType.MID.ToString()} credits. Total number of {SubjectType.MID.ToString()} credits should be {_CS.CombinationSettingStructure.MinorInterDisciplinary_Credit}" };

            if (_CS.CombinationSettingStructure.ValidateByMinorInterDisciplinary_Count)
                if (_SUB.IsNullOrEmpty() || _SUB.Where(x => x.SubjectType == SubjectType.MID)?.Count() != _CS.CombinationSettingStructure.MinorInterDisciplinary_Count)
                    return new ResponseData() { ErrorMessage = $"Miss match in total number of {SubjectType.MID.ToString()} subjects. Total number of {SubjectType.MID.ToString()} subjects should be {_CS.CombinationSettingStructure.MinorInterDisciplinary_Count}" };
            #endregion
            #region MinorVocational
            if (_CS.CombinationSettingStructure.ValidateByMinorVocational_Credit)
                if (_SUB.IsNullOrEmpty() || _SUB.Where(x => x.SubjectType == SubjectType.MVoc)?.Sum(x => x.TotalCredit) != _CS.CombinationSettingStructure.MinorVocational_Credit)
                    return new ResponseData() { ErrorMessage = $"Miss match in total number of {SubjectType.MVoc.ToString()} credits. Total number of {SubjectType.MVoc.ToString()} credits should be {_CS.CombinationSettingStructure.MinorVocational_Credit}" };

            if (_CS.CombinationSettingStructure.ValidateByMinorVocational_Count)
                if (_SUB.IsNullOrEmpty() || _SUB.Where(x => x.SubjectType == SubjectType.MVoc)?.Count() != _CS.CombinationSettingStructure.MinorVocational_Count)
                    return new ResponseData() { ErrorMessage = $"Miss match in total number of {SubjectType.MVoc.ToString()} subjects. Total number of {SubjectType.MVoc.ToString()} subjects should be {_CS.CombinationSettingStructure.MinorVocational_Count}" };
            #endregion
            #region MDisc
            if (_CS.CombinationSettingStructure.ValidateByMultiDisciplinary_Credit)
                if (_SUB.IsNullOrEmpty() || _SUB.Where(x => x.SubjectType == SubjectType.MDisc)?.Sum(x => x.TotalCredit) != _CS.CombinationSettingStructure.MultiDisciplinary_Credit)
                    return new ResponseData() { ErrorMessage = $"Miss match in total number of {SubjectType.Major.ToString()} credits. Total number of {SubjectType.MDisc.ToString()} credits should be {_CS.CombinationSettingStructure.MultiDisciplinary_Credit}" };

            if (_CS.CombinationSettingStructure.ValidateByMultiDisciplinary_Count)
                if (_SUB.IsNullOrEmpty() || _SUB.Where(x => x.SubjectType == SubjectType.MDisc)?.Count() != _CS.CombinationSettingStructure.MultiDisciplinary_Count)
                    return new ResponseData() { ErrorMessage = $"Miss match in total number of {SubjectType.Major.ToString()} subjects. Total number of {SubjectType.MDisc.ToString()} subjects should be {_CS.CombinationSettingStructure.MultiDisciplinary_Count}" };
            #endregion
            #region Internship
            if (_CS.CombinationSettingStructure.ValidateByInternship_Credit)
                if (_SUB.IsNullOrEmpty() || _SUB.Where(x => x.SubjectType == SubjectType.Internship)?.Sum(x => x.TotalCredit) != _CS.CombinationSettingStructure.Internship_Credit)
                    return new ResponseData() { ErrorMessage = $"Miss match in total number of {SubjectType.Major.ToString()} credits. Total number of {SubjectType.MDisc.ToString()} credits should be {_CS.CombinationSettingStructure.Internship_Credit}" };

            if (_CS.CombinationSettingStructure.ValidateByInternship_Count)
                if (_SUB.IsNullOrEmpty() || _SUB.Where(x => x.SubjectType == SubjectType.Internship)?.Count() != _CS.CombinationSettingStructure.Internship_Count)
                    return new ResponseData() { ErrorMessage = $"Miss match in total number of {SubjectType.Major.ToString()} subjects. Total number of {SubjectType.MDisc.ToString()} subjects should be {_CS.CombinationSettingStructure.Internship_Count}" };
            #endregion
            #region Research
            if (_CS.CombinationSettingStructure.ValidateByResearch_Credit)
                if (_SUB.IsNullOrEmpty() || _SUB.Where(x => x.SubjectType == SubjectType.Research)?.Sum(x => x.TotalCredit) != _CS.CombinationSettingStructure.Research_Credit)
                    return new ResponseData() { ErrorMessage = $"Miss match in total number of {SubjectType.Major.ToString()} credits. Total number of {SubjectType.Research.ToString()} credits should be {_CS.CombinationSettingStructure.Research_Credit}" };

            if (_CS.CombinationSettingStructure.ValidateByResearch_Count)
                if (_SUB.IsNullOrEmpty() || _SUB.Where(x => x.SubjectType == SubjectType.Research)?.Count() != _CS.CombinationSettingStructure.Research_Count)
                    return new ResponseData() { ErrorMessage = $"Miss match in total number of {SubjectType.Major.ToString()} subjects. Total number of {SubjectType.Research.ToString()} subjects should be {_CS.CombinationSettingStructure.Research_Count}" };
            #endregion
            #region Seminar
            if (_CS.CombinationSettingStructure.ValidateBySeminar_Credit)
                if (_SUB.IsNullOrEmpty() || _SUB.Where(x => x.SubjectType == SubjectType.Seminar)?.Sum(x => x.TotalCredit) != _CS.CombinationSettingStructure.Seminar_Credit)
                    return new ResponseData() { ErrorMessage = $"Miss match in total number of {SubjectType.Major.ToString()} credits. Total number of {SubjectType.Seminar.ToString()} credits should be {_CS.CombinationSettingStructure.Seminar_Credit}" };

            if (_CS.CombinationSettingStructure.ValidateBySeminar_Count)
                if (_SUB.IsNullOrEmpty() || _SUB.Where(x => x.SubjectType == SubjectType.Seminar)?.Count() != _CS.CombinationSettingStructure.Seminar_Count)
                    return new ResponseData() { ErrorMessage = $"Miss match in total number of {SubjectType.Major.ToString()} subjects. Total number of {SubjectType.Seminar.ToString()} subjects should be {_CS.CombinationSettingStructure.Seminar_Count}" };
            #endregion
            #region VAC
            if (_CS.CombinationSettingStructure.ValidateByVAC_Credit)
                if (_SUB.IsNullOrEmpty() || _SUB.Where(x => x.SubjectType == SubjectType.VAC)?.Sum(x => x.TotalCredit) != _CS.CombinationSettingStructure.VAC_Credit)
                    return new ResponseData() { ErrorMessage = $"Miss match in total number of {SubjectType.Major.ToString()} credits. Total number of {SubjectType.VAC.ToString()} credits should be {_CS.CombinationSettingStructure.VAC_Credit}" };

            if (_CS.CombinationSettingStructure.ValidateByVAC_Count)
                if (_SUB.IsNullOrEmpty() || _SUB.Where(x => x.SubjectType == SubjectType.VAC)?.Count() != _CS.CombinationSettingStructure.VAC_Count)
                    return new ResponseData() { ErrorMessage = $"Miss match in total number of {SubjectType.Major.ToString()} subjects. Total number of {SubjectType.VAC.ToString()} subjects should be {_CS.CombinationSettingStructure.VAC_Count}" };
            #endregion
            #region ValidateEachSubject
            List<ADMSubjectMaster> _NewSubjectOptions = GetSubjectListOptions(_CS, STDINFO, _CS.Semester)?.Item1;
            if (_NewSubjectOptions != null)
            {
                List<ADMSubjectMaster> InvalidSubjects = new List<ADMSubjectMaster>();
                foreach (var item in _SUB)
                    if (_NewSubjectOptions.Any(x => x.Subject_ID == item.Subject_ID)) { }
                    else InvalidSubjects.Add(item);
                if (InvalidSubjects.IsNotNullOrEmpty())
                {
                    string errorMessage = "";
                    foreach (var item in InvalidSubjects)
                    {
                        errorMessage += $"Invalid subject added: {item.CourseFullName}-{item.SubjectFullName}#";
                    }
                    return new ResponseData() { ErrorMessage = errorMessage };
                }
            }
            #endregion

            return new ResponseData() { IsSuccess = true, ResponseObject = _SUB, ErrorMessage = ErrorMessage };
        }


        //public ResponseData ValidateByCombinationSetting(PrintProgramme printProgramme, Guid student_Id, short batch, Guid course_ID, short semester, List<ADMSubjectMaster> _subjects, bool validateBySubjectOptions = true)
        //{
        //    var _CS = new CombinationSettingManager().GetCombinationSetting(course_ID, semester, batch);
        //    if (_CS == null)
        //        return new ResponseData() { ErrorMessage = "Combination setting not available. Please contact cluster university I.T. Cell" };

        //    #region ValidateStudent
        //    if (!AppUserHelper.AppUsercompact.UserRoles.Any(x => x == AppRoles.Student))
        //    {
        //        if (AppUserHelper.AppUsercompact.UserRoles.Any(x => x == AppRoles.Student))
        //        {
        //            if (!_CS.CombinationSettingStructure.AllowStudentChangeCombination)
        //            {
        //                return new ResponseData() { ErrorMessage = "The process of changing combination for student has been closed. Contact college administration section if change in subjects.", IsSuccess = false };
        //            }
        //        }
        //        else if (!AppUserHelper.AppUsercompact.UserRoles.Any(x => x == AppRoles.University))
        //        {
        //            if (!_CS.CombinationSettingStructure.AllowCollegeChangeCombination)
        //            {
        //                return new ResponseData() { ErrorMessage = "The process of changing the combination has been closed. Contact cluster university I.T. Cell.", IsSuccess = false };
        //            }
        //        }

        //        if (_CS.CombinationSettingStructure.CheckAdmissionForm)
        //        {
        //            var paymentInfo = new PaymentManager().GetPaymentDetail(student_Id, semester, printProgramme);
        //            if (paymentInfo == null)
        //                return new ResponseData() { ErrorMessage = $"Either admission form not found or fee not paid." };
        //        }

        //        bool _ExaminationFormSubmitted = false;
        //        if (_CS.CombinationSettingStructure.CheckExaminationForm)
        //        {
        //            ExaminationCourseCategory courseCategory = new GeneralFunctions().GetExaminationCourseCategory(printProgramme);
        //            var settingForm = new ExaminationFormDB().GetRegularExamFormDownloadable(courseCategory, semester);
        //            if (settingForm == null)
        //                return new ResponseData() { ErrorMessage = $"Examination form setting not found. Contact Cluster University I.T Cell." };
        //            _ExaminationFormSubmitted = new ExaminationFormManager().ExamFormSubmitted(student_Id, semester, settingForm.Year, printProgramme);
        //            if (AppUserHelper.AppUsercompact.UserRoles.Any(x => x != AppRoles.Student) && !_ExaminationFormSubmitted)
        //                return new ResponseData() { ErrorMessage = $"Either examination form not found or fee not paid." };
        //        }

        //        if (_CS.CombinationSettingStructure.CheckResult)
        //        {
        //            if (new ResultManager().ResultCreated(printProgramme, semester, student_Id))
        //                return new ResponseData() { ErrorMessage = $"Result already created" };
        //        }
        //    }
        //    #endregion

        //    #region ValidateCombinationSubjects
        //    if (_subjects.IsNullOrEmpty())
        //        return new ResponseData() { ErrorMessage = $"Combination / Subjects not found" };

        //    List<ADMSubjectMaster> _S;
        //    #region Core
        //    if (_CS.CombinationSettingStructure.Core_Count.HasValue)
        //    {
        //        _S = _subjects?.Where(x => x.SubjectType == SubjectType.Core)?.ToList();
        //        if (_CS.CombinationSettingStructure.Core_Count == 0 && _S.IsNotNullOrEmpty())
        //            return new ResponseData() { ErrorMessage = $"Core cannot be opted in -{semester}." };
        //        if (_CS.CombinationSettingStructure.Core_Count > 0)
        //        {
        //            if (_CS.CombinationSettingStructure.ValidateByCore_Count && _S.IsNullOrEmpty())
        //                return new ResponseData() { ErrorMessage = $"core subjects not found" };

        //            if (_CS.CombinationSettingStructure.ValidateByCore_Count && _S.Count() != _CS.CombinationSettingStructure.Core_Count)
        //                return new ResponseData() { ErrorMessage = $"Number of core subjects in {semester} should be {_CS.CombinationSettingStructure.Core_Count}" };

        //            if (_CS.CombinationSettingStructure.ValidateByCore_Credit && _S.Sum(x => x.TotalCredit) != _CS.CombinationSettingStructure.Core_Credit)
        //                return new ResponseData() { ErrorMessage = $"total core credit in {semester} should be {_CS.CombinationSettingStructure.Core_Credit}" };
        //        }
        //    }
        //    #endregion

        //    #region MIL
        //    if (_CS.CombinationSettingStructure.MIL_Count.HasValue)
        //    {
        //        _S = _subjects?.Where(x => x.SubjectType == SubjectType.MIL)?.ToList();
        //        if (_CS.CombinationSettingStructure.MIL_Count == 0 && _S.IsNotNullOrEmpty())
        //            return new ResponseData() { ErrorMessage = $"MIL cannot be opted in -{semester}." };
        //        if (_CS.CombinationSettingStructure.MIL_Count > 0)
        //        {
        //            if (_CS.CombinationSettingStructure.ValidateByMIL_Count && _S.IsNullOrEmpty())
        //                return new ResponseData() { ErrorMessage = $"MIL subjects not found" };

        //            if (_CS.CombinationSettingStructure.ValidateByMIL_Count && _S.Count() != _CS.CombinationSettingStructure.MIL_Count)
        //                return new ResponseData() { ErrorMessage = $"Number of MIL subjects in {semester} should be {_CS.CombinationSettingStructure.MIL_Count}" };

        //            if (_CS.CombinationSettingStructure.ValidateByMIL_Credit && _S.Sum(x => x.TotalCredit) != _CS.CombinationSettingStructure.MIL_Credit)
        //                return new ResponseData() { ErrorMessage = $"total MIL credit in {semester} should be {_CS.CombinationSettingStructure.MIL_Credit}" };
        //        }
        //    }
        //    #endregion

        //    #region AE
        //    if (_CS.CombinationSettingStructure.AE_Count.HasValue)
        //    {
        //        _S = _subjects?.Where(x => x.SubjectType == SubjectType.AE)?.ToList();
        //        if (_CS.CombinationSettingStructure.AE_Count == 0 && _S.IsNotNullOrEmpty())
        //            return new ResponseData() { ErrorMessage = $"AE cannot be opted in -{semester}." };
        //        if (_CS.CombinationSettingStructure.AE_Count > 0)
        //        {
        //            if (_CS.CombinationSettingStructure.ValidateByAE_Count && _S.IsNullOrEmpty())
        //                return new ResponseData() { ErrorMessage = $"AE subjects not found" };

        //            if (_CS.CombinationSettingStructure.ValidateByAE_Count && _S.Count() != _CS.CombinationSettingStructure.AE_Count)
        //                return new ResponseData() { ErrorMessage = $"Number of AE subjects in {semester} should be {_CS.CombinationSettingStructure.AE_Count}" };

        //            if (_CS.CombinationSettingStructure.ValidateByAE_Credit && _S.Sum(x => x.TotalCredit) != _CS.CombinationSettingStructure.AE_Credit)
        //                return new ResponseData() { ErrorMessage = $"total AE credit in {semester} should be {_CS.CombinationSettingStructure.AE_Credit}" };
        //        }
        //    }
        //    #endregion

        //    #region GE_OE
        //    if (_CS.CombinationSettingStructure.GE_OE_Count.HasValue)
        //    {
        //        _S = _subjects?.Where(x => x.SubjectType == SubjectType.GE || x.SubjectType == SubjectType.OE)?.ToList();
        //        if (_CS.CombinationSettingStructure.GE_OE_Count == 0 && _S.IsNotNullOrEmpty())
        //            return new ResponseData() { ErrorMessage = $"GE/OE cannot be opted in -{semester}." };
        //        if (_CS.CombinationSettingStructure.GE_OE_Count > 0)
        //        {
        //            if (_CS.CombinationSettingStructure.ValidateByGE_OE_Count && _S.IsNullOrEmpty())
        //                return new ResponseData() { ErrorMessage = $"GE/OE subjects not found" };

        //            if (_CS.CombinationSettingStructure.ValidateByGE_OE_Count && _S.Count() != _CS.CombinationSettingStructure.GE_OE_Count)
        //                return new ResponseData() { ErrorMessage = $"Number of GE/OE subjects in {semester} should be {_CS.CombinationSettingStructure.GE_OE_Count}" };

        //            if (_CS.CombinationSettingStructure.ValidateByGE_OE_Credit && _S.Sum(x => x.TotalCredit) != _CS.CombinationSettingStructure.GE_OE_Credit)
        //                return new ResponseData() { ErrorMessage = $"total ge/oe credit in {semester} should be {_CS.CombinationSettingStructure.GE_OE_Credit}" };

        //            foreach (var _GE_OE in _S)
        //            {
        //                if (_GE_OE.SubjectCapacitySet)
        //                {
        //                    int NumberOfGE = new AssignCombinationManager().GetSubjectCount(printProgramme, semester, _GE_OE.Subject_ID, SubjectType.GE, STDINFO.Batch, _Comb.College_ID);
        //                    if (NumberOfGE >= _GE_OE.SubjectCapacity)
        //                        return new ResponseData() { ErrorMessage = $"Maximum number of Generic Elective Subject has been reached. Please choose another subject." };
        //                }
        //            }
        //        }
        //    }
        //    #endregion

        //    #region SEC
        //    if (_CS.CombinationSettingStructure.SEC_Count.HasValue)
        //    {
        //        _S = _subjects?.Where(x => x.SubjectType == SubjectType.SEC)?.ToList();
        //        if (_CS.CombinationSettingStructure.SEC_Count == 0 && _S.IsNotNullOrEmpty())
        //            return new ResponseData() { ErrorMessage = $"Skill cannot be opted in -{semester}." };
        //        if (_CS.CombinationSettingStructure.SEC_Count > 0)
        //        {
        //            if (_CS.CombinationSettingStructure.ValidateBySEC_Count && _S.IsNullOrEmpty())
        //                return new ResponseData() { ErrorMessage = $"Skill subjects not found" };

        //            if (_CS.CombinationSettingStructure.ValidateBySEC_Count && _S.Count() != _CS.CombinationSettingStructure.SEC_Count)
        //                return new ResponseData() { ErrorMessage = $"Number of skill subjects in {semester} should be {_CS.CombinationSettingStructure.SEC_Count}" };

        //            if (_CS.CombinationSettingStructure.ValidateBySEC_Credit && _S.Sum(x => x.TotalCredit) != _CS.CombinationSettingStructure.SEC_Credit)
        //                return new ResponseData() { ErrorMessage = $"total skill credit in {semester} should be {_CS.CombinationSettingStructure.SEC_Credit}" };

        //            foreach (var _SEC in _S)
        //            {
        //                if (_SEC.SubjectCapacitySet)
        //                {
        //                    int NumberOfGE = new AssignCombinationManager().GetSubjectCount(printProgramme, semester, _SEC.Subject_ID, SubjectType.GE, STDINFO.Batch, _Comb.College_ID);
        //                    if (NumberOfGE >= _SEC.SubjectCapacity)
        //                        return new ResponseData() { ErrorMessage = $"Maximum number of Skill Subject has been reached. Please choose another subject." };
        //                }
        //            }
        //        }
        //    }
        //    #endregion

        //    #region DCE_DSE
        //    if (_CS.CombinationSettingStructure.DCE_DSE_Count.HasValue)
        //    {
        //        _S = _subjects?.Where(x => x.SubjectType == SubjectType.DCE || x.SubjectType == SubjectType.DSE)?.ToList();
        //        if (_CS.CombinationSettingStructure.DCE_DSE_Count == 0 && _S.IsNotNullOrEmpty())
        //            return new ResponseData() { ErrorMessage = $"DCE/DSE cannot be opted in -{semester}." };
        //        if (_CS.CombinationSettingStructure.DCE_DSE_Count > 0)
        //        {
        //            if (_CS.CombinationSettingStructure.ValidateByDCE_DSE_Count && _S.IsNullOrEmpty())
        //                return new ResponseData() { ErrorMessage = $"DCE/DSE subjects not found" };

        //            if (_CS.CombinationSettingStructure.ValidateByDCE_DSE_Count && _S.Count() != _CS.CombinationSettingStructure.DCE_DSE_Count)
        //                return new ResponseData() { ErrorMessage = $"Number of DCE/DSE subjects in {semester} should be {_CS.CombinationSettingStructure.DCE_DSE_Count}" };

        //            if (_CS.CombinationSettingStructure.ValidateByDCE_DSE_Credit && _S.Sum(x => x.TotalCredit) != _CS.CombinationSettingStructure.DCE_DSE_Credit)
        //                return new ResponseData() { ErrorMessage = $"Total DCE/DSE credit in {semester} should be {_CS.CombinationSettingStructure.DCE_DSE_Credit}" };
        //        }
        //    }
        //    #endregion


        //    if (_CS.CombinationSettingStructure.ValidateByTotalNumberOfSubject && _subjects.Count != _CS.CombinationSettingStructure.TotalNumberOfSubject)
        //        return new ResponseData() { ErrorMessage = $"Miss match in total number of subjects. Total number of subjects should be {_CS.CombinationSettingStructure.TotalNumberOfSubject}" };

        //    if (_CS.CombinationSettingStructure.ValidateByTotalCredit && _CS.CombinationSettingStructure.TotalCredit != _subjects.Sum(x => x.TotalCredit))
        //        return new ResponseData() { ErrorMessage = $"Miss match in total number of credits. Total number of credits should be {_CS.CombinationSettingStructure.TotalCredit}" };

        //    if (validateBySubjectOptions)
        //    {
        //        var STDINFO = new StudentManager().GetStudentWithCombination(student_Id, printProgramme, false);
        //        List<ADMSubjectMaster> _NewSubjectOptions = GetSubjectListOptions(_CS, STDINFO, semester).Item1;
        //        List<ADMSubjectMaster> InvalidSubjects = new List<ADMSubjectMaster>();
        //        foreach (var item in _subjects)
        //            if (_NewSubjectOptions.Any(x => x.Subject_ID == item.Subject_ID)) { } else InvalidSubjects.Add(item);
        //        if (InvalidSubjects.IsNotNullOrEmpty())
        //        {
        //            string errorMessage = "";
        //            foreach (var item in InvalidSubjects)
        //            {
        //                errorMessage += $"Invalid subject added: {item.CourseFullName}-{item.SubjectFullName}#";
        //            }
        //            return new ResponseData() { ErrorMessage = errorMessage };
        //        }
        //    }

        //    return new ResponseData() { IsSuccess = true };
        //}
        public Tuple<List<ADMSubjectMaster>, AssignCombinationViewModel, ResponseData> GetSubjectListOptionsNEP(CombinationSetting _CombinationSetting, ARGPersonalInformation STDINFO, short _SetCombinationForSemester)
        {
            AssignCombinationViewModel listsub = new AssignCombinationViewModel();
            Parameters parameter;
            var _NewSubjectOptions = new List<ADMSubjectMaster>();
            if (_CombinationSetting.CompulsarySubject.IsNotNullOrEmpty())
            {
                _NewSubjectOptions.AddRange(_CombinationSetting.CompulsarySubject);
            }
            var _CSManager = new CombinationSettingManager();
            if (_CombinationSetting.CombinationSettingStructure.Major_Count > 0)
            {

                parameter = new Parameters() { Filters = new List<SearchFilter>() { new SearchFilter() { TableAlias = "S", Column = "Programme", Value = new CourseManager().GetPrintProgrammesInClause(_CombinationSetting.Course_ID), Operator = SQLOperator.In }, new SearchFilter() { TableAlias = "S", Column = "Status", Value = "1", Operator = SQLOperator.EqualTo }, new SearchFilter() { Column = "Semester", Value = _SetCombinationForSemester.ToString(), Operator = SQLOperator.EqualTo }, new SearchFilter() { Column = "SubjectType", Value = ((short)SubjectType.Major).ToString(), Operator = SQLOperator.EqualTo } }, PageInfo = new Paging() { DefaultOrderByColumn = "SubjectFullName", PageNumber = -1, PageSize = -1 }, SortInfo = new Sort() { ColumnName = "SubjectFullName", OrderBy = SortOrder.Ascending } };
                var _Subjects = new SubjectsManager().GetAllSubjectList(parameter);
                if (_Subjects.IsNotNullOrEmpty())
                {
                    for (int i = 0; i < _CombinationSetting.CombinationSettingStructure.Major_Count; i++)
                    {
                        _NewSubjectOptions.AddRange(_Subjects);
                        listsub.MajorSubjectOptionList[$"{i.ToString()}#{i.ToString()}"] = _Subjects;
                    }
                }
            }
            if (_CombinationSetting.CombinationSettingStructure.MinorInterDisciplinary_Count > 0)
            {

                parameter = new Parameters() { Filters = new List<SearchFilter>() { new SearchFilter() { TableAlias = "S", Column = "Programme", Value = new CourseManager().GetPrintProgrammesInClause(_CombinationSetting.Course_ID), Operator = SQLOperator.In }, new SearchFilter() { TableAlias = "S", Column = "Status", Value = "1", Operator = SQLOperator.EqualTo }, new SearchFilter() { Column = "Semester", Value = _SetCombinationForSemester.ToString(), Operator = SQLOperator.EqualTo }, new SearchFilter() { Column = "SubjectType", Value = ((short)SubjectType.MID).ToString(), Operator = SQLOperator.EqualTo } }, PageInfo = new Paging() { DefaultOrderByColumn = "SubjectFullName", PageNumber = -1, PageSize = -1 }, SortInfo = new Sort() { ColumnName = "SubjectFullName", OrderBy = SortOrder.Ascending } };
                var _Subjects = new SubjectsManager().GetAllSubjectList(parameter);
                if (_Subjects.IsNotNullOrEmpty())
                {
                    for (int i = 0; i < _CombinationSetting.CombinationSettingStructure.MinorInterDisciplinary_Count; i++)
                    {
                        _NewSubjectOptions.AddRange(_Subjects);
                        listsub.MIDSubjectOptionList[$"{i.ToString()}#{i.ToString()}"] = _Subjects;
                    }
                }
            }
            if (_CombinationSetting.CombinationSettingStructure.MinorVocational_Count > 0)
            {

                parameter = new Parameters() { Filters = new List<SearchFilter>() { new SearchFilter() { TableAlias = "S", Column = "Programme", Value = new CourseManager().GetPrintProgrammesInClause(_CombinationSetting.Course_ID), Operator = SQLOperator.In }, new SearchFilter() { TableAlias = "S", Column = "Status", Value = "1", Operator = SQLOperator.EqualTo }, new SearchFilter() { Column = "Semester", Value = _SetCombinationForSemester.ToString(), Operator = SQLOperator.EqualTo }, new SearchFilter() { Column = "SubjectType", Value = ((short)SubjectType.MVoc).ToString(), Operator = SQLOperator.EqualTo } }, PageInfo = new Paging() { DefaultOrderByColumn = "SubjectFullName", PageNumber = -1, PageSize = -1 }, SortInfo = new Sort() { ColumnName = "SubjectFullName", OrderBy = SortOrder.Ascending } };
                var _Subjects = new SubjectsManager().GetAllSubjectList(parameter);
                if (_Subjects.IsNotNullOrEmpty())
                {
                    for (int i = 0; i < _CombinationSetting.CombinationSettingStructure.MinorVocational_Count; i++)
                    {
                        _NewSubjectOptions.AddRange(_Subjects);
                        listsub.MVocSubjectOptionList[$"{i.ToString()}#{i.ToString()}"] = _Subjects;
                    }
                }

            }
            if (_CombinationSetting.CombinationSettingStructure.MultiDisciplinary_Count > 0)
            {

                parameter = new Parameters() { Filters = new List<SearchFilter>() { new SearchFilter() { TableAlias = "S", Column = "Programme", Value = new CourseManager().GetPrintProgrammesInClause(_CombinationSetting.Course_ID), Operator = SQLOperator.In }, new SearchFilter() { TableAlias = "S", Column = "Status", Value = "1", Operator = SQLOperator.EqualTo }, new SearchFilter() { Column = "Semester", Value = _SetCombinationForSemester.ToString(), Operator = SQLOperator.EqualTo }, new SearchFilter() { Column = "SubjectType", Value = ((short)SubjectType.MDisc).ToString(), Operator = SQLOperator.EqualTo } }, PageInfo = new Paging() { DefaultOrderByColumn = "SubjectFullName", PageNumber = -1, PageSize = -1 }, SortInfo = new Sort() { ColumnName = "SubjectFullName", OrderBy = SortOrder.Ascending } };
                var _Subjects = new SubjectsManager().GetAllSubjectList(parameter);
                if (_Subjects.IsNotNullOrEmpty())
                {
                    for (int i = 0; i < _CombinationSetting.CombinationSettingStructure.MultiDisciplinary_Count; i++)
                    {
                        _NewSubjectOptions.AddRange(_Subjects);
                        listsub.MDiscSubjectOptionList[$"{i.ToString()}#{i.ToString()}"] = _Subjects;
                    }
                }
            }
            if (_CombinationSetting.CombinationSettingStructure.Research_Count > 0)
            {

                parameter = new Parameters() { Filters = new List<SearchFilter>() { new SearchFilter() { TableAlias = "S", Column = "Programme", Value = new CourseManager().GetPrintProgrammesInClause(_CombinationSetting.Course_ID), Operator = SQLOperator.In }, new SearchFilter() { TableAlias = "S", Column = "Status", Value = "1", Operator = SQLOperator.EqualTo }, new SearchFilter() { Column = "Semester", Value = _SetCombinationForSemester.ToString(), Operator = SQLOperator.EqualTo }, new SearchFilter() { Column = "SubjectType", Value = ((short)SubjectType.Research).ToString(), Operator = SQLOperator.EqualTo } }, PageInfo = new Paging() { DefaultOrderByColumn = "SubjectFullName", PageNumber = -1, PageSize = -1 }, SortInfo = new Sort() { ColumnName = "SubjectFullName", OrderBy = SortOrder.Ascending } };
                var _Subjects = new SubjectsManager().GetAllSubjectList(parameter);
                if (_Subjects.IsNotNullOrEmpty())
                {
                    for (int i = 0; i < _CombinationSetting.CombinationSettingStructure.Research_Count; i++)
                    {
                        _NewSubjectOptions.AddRange(_Subjects);
                        listsub.ResearchSubjectOptionList[$"{i.ToString()}#{i.ToString()}"] = _Subjects;
                    }
                }
            }
            if (_CombinationSetting.CombinationSettingStructure.Internship_Count > 0)
            {

                parameter = new Parameters() { Filters = new List<SearchFilter>() { new SearchFilter() { TableAlias = "S", Column = "Programme", Value = new CourseManager().GetPrintProgrammesInClause(_CombinationSetting.Course_ID), Operator = SQLOperator.In }, new SearchFilter() { TableAlias = "S", Column = "Status", Value = "1", Operator = SQLOperator.EqualTo }, new SearchFilter() { Column = "Semester", Value = _SetCombinationForSemester.ToString(), Operator = SQLOperator.EqualTo }, new SearchFilter() { Column = "SubjectType", Value = ((short)SubjectType.Internship).ToString(), Operator = SQLOperator.EqualTo } }, PageInfo = new Paging() { DefaultOrderByColumn = "SubjectFullName", PageNumber = -1, PageSize = -1 }, SortInfo = new Sort() { ColumnName = "SubjectFullName", OrderBy = SortOrder.Ascending } };
                var _Subjects = new SubjectsManager().GetAllSubjectList(parameter);
                if (_Subjects.IsNotNullOrEmpty())
                {
                    for (int i = 0; i < _CombinationSetting.CombinationSettingStructure.Internship_Count; i++)
                    {
                        _NewSubjectOptions.AddRange(_Subjects);
                        listsub.InternshipSubjectOptionList[$"{i.ToString()}#{i.ToString()}"] = _Subjects;
                    }
                }
            }
            if (_CombinationSetting.CombinationSettingStructure.Seminar_Count > 0)
            {

                parameter = new Parameters() { Filters = new List<SearchFilter>() { new SearchFilter() { TableAlias = "S", Column = "Programme", Value = new CourseManager().GetPrintProgrammesInClause(_CombinationSetting.Course_ID), Operator = SQLOperator.In }, new SearchFilter() { TableAlias = "S", Column = "Status", Value = "1", Operator = SQLOperator.EqualTo }, new SearchFilter() { Column = "Semester", Value = _SetCombinationForSemester.ToString(), Operator = SQLOperator.EqualTo }, new SearchFilter() { Column = "SubjectType", Value = ((short)SubjectType.Seminar).ToString(), Operator = SQLOperator.EqualTo } }, PageInfo = new Paging() { DefaultOrderByColumn = "SubjectFullName", PageNumber = -1, PageSize = -1 }, SortInfo = new Sort() { ColumnName = "SubjectFullName", OrderBy = SortOrder.Ascending } };
                var _Subjects = new SubjectsManager().GetAllSubjectList(parameter);
                if (_Subjects.IsNotNullOrEmpty())
                {
                    for (int i = 0; i < _CombinationSetting.CombinationSettingStructure.Seminar_Count; i++)
                    {
                        _NewSubjectOptions.AddRange(_Subjects);
                        listsub.SeminarSubjectOptionList[$"{i.ToString()}#{i.ToString()}"] = _Subjects;
                    }
                }
            }
            if (_CombinationSetting.CombinationSettingStructure.AE_Count > 0)
            {

                parameter = new Parameters() { Filters = new List<SearchFilter>() { new SearchFilter() { TableAlias = "S", Column = "Programme", Value = new CourseManager().GetPrintProgrammesInClause(_CombinationSetting.Course_ID), Operator = SQLOperator.In }, new SearchFilter() { TableAlias = "S", Column = "Status", Value = "1", Operator = SQLOperator.EqualTo }, new SearchFilter() { Column = "Semester", Value = _SetCombinationForSemester.ToString(), Operator = SQLOperator.EqualTo }, new SearchFilter() { Column = "SubjectType", Value = ((short)SubjectType.AE).ToString(), Operator = SQLOperator.EqualTo } }, PageInfo = new Paging() { DefaultOrderByColumn = "SubjectFullName", PageNumber = -1, PageSize = -1 }, SortInfo = new Sort() { ColumnName = "SubjectFullName", OrderBy = SortOrder.Ascending } };
                var _Subjects = new SubjectsManager().GetAllSubjectList(parameter);
                if (_Subjects.IsNotNullOrEmpty())
                {
                    for (int i = 0; i < _CombinationSetting.CombinationSettingStructure.AE_Count; i++)
                    {
                        _NewSubjectOptions.AddRange(_Subjects);
                        listsub.AESubjectOptionList[$"{i.ToString()}#{i.ToString()}"] = _Subjects;
                    }
                }
            }
            if (_CombinationSetting.CombinationSettingStructure.VAC_Count > 0)
            {

                parameter = new Parameters() { Filters = new List<SearchFilter>() { new SearchFilter() { TableAlias = "S", Column = "Programme", Value = new CourseManager().GetPrintProgrammesInClause(_CombinationSetting.Course_ID), Operator = SQLOperator.In }, new SearchFilter() { TableAlias = "S", Column = "Status", Value = "1", Operator = SQLOperator.EqualTo }, new SearchFilter() { Column = "Semester", Value = _SetCombinationForSemester.ToString(), Operator = SQLOperator.EqualTo }, new SearchFilter() { Column = "SubjectType", Value = ((short)SubjectType.VAC).ToString(), Operator = SQLOperator.EqualTo } }, PageInfo = new Paging() { DefaultOrderByColumn = "SubjectFullName", PageNumber = -1, PageSize = -1 }, SortInfo = new Sort() { ColumnName = "SubjectFullName", OrderBy = SortOrder.Ascending } };
                var _Subjects = new SubjectsManager().GetAllSubjectList(parameter);
                if (_Subjects.IsNotNullOrEmpty())
                {
                    for (int i = 0; i < _CombinationSetting.CombinationSettingStructure.VAC_Count; i++)
                    {
                        _NewSubjectOptions.AddRange(_Subjects);
                        listsub.VACSubjectOptionList[$"{i.ToString()}#{i.ToString()}"] = _Subjects;
                    }
                }
            }


            if (_CombinationSetting.CombinationSettingStructure.SEC_Count > 0)
            {
                if (_CombinationSetting.CombinationSettingStructure.AllowInterProgrammeSkill)
                {
                    parameter = new Parameters() { Filters = new List<SearchFilter>() { new SearchFilter() { TableAlias = "S", Column = "Programme", Value = new CourseManager().GetPrintProgrammesBySkillProgramme(_CombinationSetting.Course_ID), Operator = SQLOperator.In }, new SearchFilter() { TableAlias = "S", Column = "Status", Value = "1", Operator = SQLOperator.EqualTo }, new SearchFilter() { Column = "Semester", Value = _SetCombinationForSemester.ToString(), Operator = SQLOperator.EqualTo }, new SearchFilter() { TableAlias = "S", Column = "College_ID", Value = STDINFO.AcceptCollege_ID.ToString(), Operator = SQLOperator.EqualTo }, new SearchFilter() { Column = "SubjectType", Value = ((short)SubjectType.SEC).ToString(), Operator = SQLOperator.EqualTo } }, PageInfo = new Paging() { DefaultOrderByColumn = "SubjectFullName", PageNumber = -1, PageSize = -1 }, SortInfo = new Sort() { ColumnName = "SubjectFullName", OrderBy = SortOrder.Ascending } };
                }
                else
                {
                    parameter = new Parameters() { Filters = new List<SearchFilter>() { new SearchFilter() { TableAlias = "S", Column = "Programme", Value = new CourseManager().GetPrintProgrammesInClause(_CombinationSetting.Course_ID), Operator = SQLOperator.In }, new SearchFilter() { TableAlias = "S", Column = "Status", Value = "1", Operator = SQLOperator.EqualTo }, new SearchFilter() { Column = "Semester", Value = _SetCombinationForSemester.ToString(), Operator = SQLOperator.EqualTo }, new SearchFilter() { TableAlias = "S", Column = "College_ID", Value = (STDINFO.AcceptCollege_ID ?? STDINFO.AssignedCollege_ID).ToString(), Operator = SQLOperator.EqualTo }, new SearchFilter() { Column = "SubjectType", Value = ((short)SubjectType.SEC).ToString(), Operator = SQLOperator.EqualTo } }, PageInfo = new Paging() { DefaultOrderByColumn = "SubjectFullName", PageNumber = -1, PageSize = -1 }, SortInfo = new Sort() { ColumnName = "SubjectFullName", OrderBy = SortOrder.Ascending } };
                }
                var _Subjects = new SubjectsManager().GetAllSubjectList(parameter);
                if (_Subjects.IsNotNullOrEmpty())
                {
                    for (int i = 0; i < _CombinationSetting.CombinationSettingStructure.SEC_Count; i++)
                    {
                        _NewSubjectOptions.AddRange(_Subjects);
                        listsub.SECSubjectOptionList[$"{i.ToString()}#{i.ToString()}"] = _Subjects;
                    }
                }
            }


            if (_CombinationSetting.CombinationSettingStructure.AdditionalCourseCount > 0)
            {
                parameter = new Parameters() { Filters = new List<SearchFilter>() { new SearchFilter() { Column = "Programme", Value = new CourseManager().GetPrintProgrammesInClause(_CombinationSetting.Course_ID), Operator = SQLOperator.In, TableAlias = "S", }, new SearchFilter() { TableAlias = "S", Column = "Status", Value = "1", Operator = SQLOperator.EqualTo }, new SearchFilter() { Column = "Semester", Value = _SetCombinationForSemester.ToString(), Operator = SQLOperator.EqualTo }, new SearchFilter() { Column = "SubjectType", Value = $"{((short)SubjectType.Core).ToString()},{((short)SubjectType.OC).ToString()},{((short)SubjectType.DCE).ToString()},{((short)SubjectType.DSE).ToString()}", Operator = SQLOperator.In }, new SearchFilter() { TableAlias = "S", Column = "Course_ID", Value = _CombinationSetting.Course_ID.ToString(), Operator = SQLOperator.EqualTo } }, PageInfo = new Paging() { DefaultOrderByColumn = "SubjectFullName", PageNumber = -1, PageSize = -1 }, SortInfo = new Sort() { ColumnName = "SubjectFullName", OrderBy = SortOrder.Ascending } };
                var _Subjects = new SubjectsManager().GetAllSubjectList(parameter);
                if (_Subjects.IsNotNullOrEmpty())
                {
                    for (int i = 0; i < _CombinationSetting.CombinationSettingStructure.AdditionalCourseCount; i++)
                    {
                        _NewSubjectOptions.AddRange(_Subjects);
                        listsub.AdditionalSubjectOptionList[$"{i.ToString()}#{i.ToString()}"] = _Subjects;
                    }
                }
            }

            return Tuple.Create(_NewSubjectOptions, listsub, new ResponseData() { IsSuccess = true });
        }


        public Tuple<List<ADMSubjectMaster>, AssignCombinationViewModel, ResponseData> GetSubjectListOptions(CombinationSetting _CombinationSetting, ARGPersonalInformation STDINFO, short _SetCombinationForSemester)
        {
            AssignCombinationViewModel listsub = new AssignCombinationViewModel();
            if (STDINFO.SelectedCombinations.IsNullOrEmpty() && _SetCombinationForSemester > 1) return null;
            ARGSelectedCombination _PrevSemCombination = STDINFO.SelectedCombinations?.FirstOrDefault(x => x.Semester == _SetCombinationForSemester - 1) ?? STDINFO.SelectedCombinations?.FirstOrDefault(x => x.Semester == _SetCombinationForSemester);
            if (_PrevSemCombination == null && _SetCombinationForSemester > 1)
            {
                return Tuple.Create<List<ADMSubjectMaster>, AssignCombinationViewModel, ResponseData>(null, null, new ResponseData() { ErrorMessage = "Previous semester combination not assigned." });
            }
            if (_SetCombinationForSemester == 1) { _PrevSemCombination = new ARGSelectedCombination() { Course_ID = _CombinationSetting.Course_ID }; }
            Parameters parameter;
            var _NewSubjectOptions = new List<ADMSubjectMaster>();
            if (_CombinationSetting.CompulsarySubject.IsNotNullOrEmpty())
            {
                _NewSubjectOptions.AddRange(_CombinationSetting.CompulsarySubject);
            }
            var _CSManager = new CombinationSettingManager();
            if (_CombinationSetting.CombinationSettingStructure.Core_Count > 0)
            {
                var _PrevSemCoreSubjects = _PrevSemCombination.Subjects.Where(x => x.SubjectType == SubjectType.Core)?.ToList();
                int loopCount = _CombinationSetting.CombinationSettingStructure.Core_Count > _PrevSemCoreSubjects.Count ? _CombinationSetting.CombinationSettingStructure.Core_Count : _PrevSemCoreSubjects.Count;
                for (int i = 0; i < loopCount; i++)
                {
                    if (_CombinationSetting.CombinationSettingStructure.CoreDependOnPrevSem && i < _PrevSemCoreSubjects.Count)
                    {
                        var _SubjectSettings = _CSManager.SubjectCombinationSettingList(_PrevSemCoreSubjects[i].Subject_ID, _CombinationSetting.Semester)?.Where(x => x.DependentSubject_ID != null)?.Select(x => x.DependentSubject_ID)?.ToIN()?.Replace("'", "");
                        if (!string.IsNullOrEmpty(_SubjectSettings))
                        {
                            parameter = new Parameters() { Filters = new List<SearchFilter>() { new SearchFilter() { Value = ((short)SubjectType.Core).ToString(), Column = "SubjectType", Operator = SQLOperator.EqualTo }, new SearchFilter() { Value = _SubjectSettings, Column = "Subject_ID", Operator = SQLOperator.In }, new SearchFilter() { TableAlias = "S", Column = "Course_ID", Value = _PrevSemCombination.Course_ID.ToString(), Operator = SQLOperator.EqualTo }, new SearchFilter() { TableAlias = "S", Column = "Status", Value = "1", Operator = SQLOperator.EqualTo }, new SearchFilter() { Column = "Semester", Value = _SetCombinationForSemester.ToString(), Operator = SQLOperator.EqualTo } }, PageInfo = new Paging() { DefaultOrderByColumn = "SubjectFullName", PageNumber = -1, PageSize = -1 }, SortInfo = new Sort() { ColumnName = "SubjectFullName", OrderBy = System.Data.SqlClient.SortOrder.Ascending } };
                            var _Subjects = new SubjectsManager().GetAllSubjectList(parameter);
                            if (_Subjects.IsNotNullOrEmpty())
                            {
                                _NewSubjectOptions.AddRange(_Subjects);
                                listsub.CoreSubjectOptionList[$"{_PrevSemCoreSubjects[i].Subject_ID.ToString()}#{_PrevSemCoreSubjects[i].SubjectFullName}"] = _Subjects;
                            }
                        }
                    }
                    else
                    {
                        parameter = new Parameters() { Filters = new List<SearchFilter>() { new SearchFilter() { Value = ((short)SubjectType.Core).ToString(), Column = "SubjectType", Operator = SQLOperator.EqualTo }, new SearchFilter() { TableAlias = "S", Column = "Course_ID", Value = _PrevSemCombination.Course_ID.ToString(), Operator = SQLOperator.EqualTo }, new SearchFilter() { TableAlias = "S", Column = "Status", Value = "1", Operator = SQLOperator.EqualTo }, new SearchFilter() { Column = "Semester", Value = _SetCombinationForSemester.ToString(), Operator = SQLOperator.EqualTo } }, PageInfo = new Paging() { DefaultOrderByColumn = "SubjectFullName", PageNumber = -1, PageSize = -1 }, SortInfo = new Sort() { ColumnName = "SubjectFullName", OrderBy = SortOrder.Ascending } };
                        var _Subjects = new SubjectsManager().GetAllSubjectList(parameter);
                        if (_Subjects.IsNotNullOrEmpty())
                        {
                            _NewSubjectOptions.AddRange(_Subjects);
                            listsub.CoreSubjectOptionList[$"{i.ToString()}#{i.ToString()}"] = _Subjects;
                        }
                    }
                }
            }

            if (_CombinationSetting.CombinationSettingStructure.MIL_Count > 0)
            {
                var _PrevSemMILSubjects = _PrevSemCombination.Subjects.Where(x => x.SubjectType == SubjectType.MIL)?.ToList();
                int loopCount = _CombinationSetting.CombinationSettingStructure.MIL_Count > _PrevSemMILSubjects.Count ? _CombinationSetting.CombinationSettingStructure.MIL_Count : _PrevSemMILSubjects.Count;
                for (int i = 0; i < loopCount; i++)
                {
                    if (_CombinationSetting.CombinationSettingStructure.MILDependOnPrevSem && i < _PrevSemMILSubjects.Count)
                    {
                        var _SubjectSettings = _CSManager.SubjectCombinationSettingList(_PrevSemMILSubjects[i].Subject_ID, _CombinationSetting.Semester)?.Where(x => x.DependentSubject_ID != null)?.Select(x => x.DependentSubject_ID)?.ToIN()?.Replace("'", "");
                        if (!string.IsNullOrEmpty(_SubjectSettings))
                        {
                            parameter = new Parameters() { Filters = new List<SearchFilter>() { new SearchFilter() { Value = ((short)SubjectType.MIL).ToString(), Column = "SubjectType", Operator = SQLOperator.EqualTo }, new SearchFilter() { Value = _SubjectSettings, Column = "Subject_ID", Operator = SQLOperator.In }, new SearchFilter() { TableAlias = "S", Column = "Course_ID", Value = _PrevSemCombination.Course_ID.ToString(), Operator = SQLOperator.EqualTo }, new SearchFilter() { TableAlias = "S", Column = "Status", Value = "1", Operator = SQLOperator.EqualTo }, new SearchFilter() { Column = "Semester", Value = _SetCombinationForSemester.ToString(), Operator = SQLOperator.EqualTo } }, PageInfo = new Paging() { DefaultOrderByColumn = "SubjectFullName", PageNumber = -1, PageSize = -1 }, SortInfo = new Sort() { ColumnName = "SubjectFullName", OrderBy = System.Data.SqlClient.SortOrder.Ascending } };
                            var _Subjects = new SubjectsManager().GetAllSubjectList(parameter);
                            if (_Subjects.IsNotNullOrEmpty())
                            {
                                _NewSubjectOptions.AddRange(_Subjects);
                                listsub.MILSubjectOptionList[$"{_PrevSemMILSubjects[i].Subject_ID.ToString()}#{_PrevSemMILSubjects[i].SubjectFullName}"] = _Subjects;
                            }
                        }
                    }
                    else
                    {
                        parameter = new Parameters() { Filters = new List<SearchFilter>() { new SearchFilter() { Value = ((short)SubjectType.MIL).ToString(), Column = "SubjectType", Operator = SQLOperator.EqualTo }, new SearchFilter() { TableAlias = "S", Column = "Course_ID", Value = _PrevSemCombination.Course_ID.ToString(), Operator = SQLOperator.EqualTo }, new SearchFilter() { TableAlias = "S", Column = "Status", Value = "1", Operator = SQLOperator.EqualTo }, new SearchFilter() { Column = "Semester", Value = _SetCombinationForSemester.ToString(), Operator = SQLOperator.EqualTo } }, PageInfo = new Paging() { DefaultOrderByColumn = "SubjectFullName", PageNumber = -1, PageSize = -1 }, SortInfo = new Sort() { ColumnName = "SubjectFullName", OrderBy = SortOrder.Ascending } };
                        var _Subjects = new SubjectsManager().GetAllSubjectList(parameter);
                        if (_Subjects.IsNotNullOrEmpty())
                        {
                            _NewSubjectOptions.AddRange(_Subjects);
                            listsub.MILSubjectOptionList[$"{i.ToString()}#{i.ToString()}"] = _Subjects;
                        }
                    }
                }
            }

            if (_CombinationSetting.CombinationSettingStructure.OC_Count > 0)
            {
                parameter = new Parameters() { Filters = new List<SearchFilter>() { new SearchFilter() { TableAlias = "S", Column = "Status", Value = "1", Operator = SQLOperator.EqualTo }, new SearchFilter() { Column = "Semester", Value = _SetCombinationForSemester.ToString(), Operator = SQLOperator.EqualTo }, new SearchFilter() { Column = "SubjectType", Value = ((short)SubjectType.OC).ToString(), Operator = SQLOperator.EqualTo }, new SearchFilter() { TableAlias = "S", Column = "Course_ID", Value = _PrevSemCombination.Course_ID.ToString(), Operator = SQLOperator.EqualTo } }, PageInfo = new Paging() { DefaultOrderByColumn = "SubjectFullName", PageNumber = -1, PageSize = -1 }, SortInfo = new Sort() { ColumnName = "SubjectFullName", OrderBy = System.Data.SqlClient.SortOrder.Ascending } };
                var _Subjects = new SubjectsManager().GetAllSubjectList(parameter);
                if (_Subjects.IsNotNullOrEmpty())
                {
                    for (int i = 0; i < _CombinationSetting.CombinationSettingStructure.OC_Count; i++)
                    {
                        _NewSubjectOptions.AddRange(_Subjects);
                        listsub.OptionalCoreSubjectOptionList[$"{i.ToString()}#{i.ToString()}"] = _Subjects;
                    }
                }
            }

            if (_CombinationSetting.CombinationSettingStructure.SEC_Count > 0)
            {
                if (_CombinationSetting.CombinationSettingStructure.AllowInterProgrammeSkill)
                {
                    parameter = new Parameters()
                    {
                        Filters = new List<SearchFilter>() {

                        new SearchFilter() { TableAlias = "S", Column = "Programme",
                            Value = new CourseManager().GetPrintProgrammesBySkillProgramme(_PrevSemCombination.Course_ID), Operator = SQLOperator.In },

                        new SearchFilter() { TableAlias = "S", Column = "Status", Value = "1", Operator = SQLOperator.EqualTo },
                        new SearchFilter() { Column = "Semester", Value = _SetCombinationForSemester.ToString(), Operator = SQLOperator.EqualTo },
                        new SearchFilter() { TableAlias = "S", Column = "College_ID",
                            Value = (STDINFO.AcceptCollege_ID?? AppUserHelper.College_ID).ToString(), Operator = SQLOperator.EqualTo },
                        new SearchFilter() { Column = "SubjectType", Value = ((short)SubjectType.SEC).ToString(), Operator = SQLOperator.EqualTo } },
                        PageInfo = new Paging() { DefaultOrderByColumn = "SubjectFullName", PageNumber = -1, PageSize = -1 },
                        SortInfo = new Sort() { ColumnName = "SubjectFullName", OrderBy = SortOrder.Ascending }
                    };
                }
                else
                {
                    parameter = new Parameters()
                    {
                        Filters = new List<SearchFilter>() {

                        new SearchFilter() { TableAlias = "S", Column = "Programme",
                            Value = new CourseManager().GetPrintProgrammesInClause(_PrevSemCombination.Course_ID), Operator = SQLOperator.In },

                        new SearchFilter() { TableAlias = "S", Column = "Status", Value = "1", Operator = SQLOperator.EqualTo },
                        new SearchFilter() { Column = "Semester", Value = _SetCombinationForSemester.ToString(), Operator = SQLOperator.EqualTo },
                        new SearchFilter() { TableAlias = "S", Column = "College_ID",
                            Value = (STDINFO.AcceptCollege_ID ?? AppUserHelper.College_ID).ToString(),
                            Operator = SQLOperator.EqualTo },
                        new SearchFilter() { Column = "SubjectType", Value = ((short)SubjectType.SEC).ToString(), Operator = SQLOperator.EqualTo } },
                        PageInfo = new Paging() { DefaultOrderByColumn = "SubjectFullName", PageNumber = -1, PageSize = -1 },
                        SortInfo = new Sort() { ColumnName = "SubjectFullName", OrderBy = SortOrder.Ascending }
                    };
                }
                var _Subjects = new SubjectsManager().GetAllSubjectList(parameter);
                if (_Subjects.IsNotNullOrEmpty())
                {
                    for (int i = 0; i < _CombinationSetting.CombinationSettingStructure.SEC_Count; i++)
                    {
                        _NewSubjectOptions.AddRange(_Subjects);
                        listsub.SECSubjectOptionList[$"{i.ToString()}#{i.ToString()}"] = _Subjects;
                    }
                }
            }

            if (_CombinationSetting.CombinationSettingStructure.GE_OE_Count > 0)
            {
                parameter = new Parameters() { Filters = new List<SearchFilter>() { new SearchFilter() { TableAlias = "S", Column = "Programme", Value = new CourseManager().GetPrintProgrammesInClause(_PrevSemCombination.Course_ID), Operator = SQLOperator.In }, new SearchFilter() { TableAlias = "S", Column = "Status", Value = "1", Operator = SQLOperator.EqualTo }, new SearchFilter() { Column = "Semester", Value = _SetCombinationForSemester.ToString(), Operator = SQLOperator.EqualTo }, new SearchFilter() { Column = "SubjectType", Value = ((short)SubjectType.GE).ToString() + "," + ((short)SubjectType.OE).ToString(), Operator = SQLOperator.In } }, PageInfo = new Paging() { DefaultOrderByColumn = "SubjectFullName", PageNumber = -1, PageSize = -1 }, SortInfo = new Sort() { ColumnName = "SubjectFullName", OrderBy = SortOrder.Ascending } };
                if (!_CombinationSetting.CombinationSettingStructure.AllowInterCollegeElective)
                    parameter.Filters.Add(new SearchFilter() { TableAlias = "S", Column = "College_ID", Value = STDINFO.AcceptCollege_ID.HasValue ? STDINFO.AcceptCollege_ID.ToString() : AppUserHelper.College_ID.ToString(), Operator = SQLOperator.EqualTo });
                var _Subjects = new SubjectsManager().GetAllSubjectList(parameter);
                if (_Subjects.IsNotNullOrEmpty())
                {
                    for (int i = 0; i < _CombinationSetting.CombinationSettingStructure.GE_OE_Count; i++)
                    {
                        _NewSubjectOptions.AddRange(_Subjects);
                        listsub.GE_OESubjectOptionList[$"{i.ToString()}#{i.ToString()}"] = _Subjects;
                    }
                }
            }

            if (_CombinationSetting.CombinationSettingStructure.DCE_DSE_Count > 0)
            {
                if (_CombinationSetting.CombinationSettingStructure.DCEBySubject)
                {
                    if (_CombinationSetting.CombinationSettingStructure.BaseSemester > 0)
                    {
                        List<ADMSubjectMaster> _BaseSemCoreSubject = STDINFO.SelectedCombinations.FirstOrDefault(x => x.Semester == (short)_CombinationSetting.CombinationSettingStructure.BaseSemester)?.Subjects?.Where(x => x.SubjectType == SubjectType.Core)?.ToList();
                        int loopCount = _CombinationSetting.CombinationSettingStructure.DCE_DSE_Count > _BaseSemCoreSubject.Count ? _CombinationSetting.CombinationSettingStructure.DCE_DSE_Count : _BaseSemCoreSubject.Count;
                        for (int i = 0; i < loopCount; i++)
                        {
                            if (i < _BaseSemCoreSubject.Count)
                            {
                                var _SubjectSettings = _CSManager.SubjectCombinationSettingList(_BaseSemCoreSubject[i].Subject_ID, _CombinationSetting.Semester)?.Where(x => x.DependentSubject_ID != null)?.Select(x => x.DependentSubject_ID)?.ToIN()?.Replace("'", "");
                                if (!string.IsNullOrEmpty(_SubjectSettings))
                                {
                                    parameter = new Parameters() { Filters = new List<SearchFilter>() { new SearchFilter() { Value = _SubjectSettings, Column = "Subject_ID", Operator = SQLOperator.In }, new SearchFilter() { Column = "SubjectType", Value = ((short)SubjectType.DSE).ToString() + "," + ((short)SubjectType.DCE).ToString(), Operator = SQLOperator.In }, new SearchFilter() { TableAlias = "S", Column = "Course_ID", Value = _PrevSemCombination.Course_ID.ToString(), Operator = SQLOperator.EqualTo }, new SearchFilter() { TableAlias = "S", Column = "Status", Value = "1", Operator = SQLOperator.EqualTo }, new SearchFilter() { Column = "Semester", Value = _CombinationSetting.Semester.ToString(), Operator = SQLOperator.EqualTo } }, PageInfo = new Paging() { DefaultOrderByColumn = "SubjectFullName", PageNumber = -1, PageSize = -1 }, SortInfo = new Sort() { ColumnName = "SubjectFullName", OrderBy = System.Data.SqlClient.SortOrder.Ascending } };
                                    var _Subjects = new SubjectsManager().GetAllSubjectList(parameter);
                                    if (_Subjects.IsNotNullOrEmpty())
                                    {
                                        _NewSubjectOptions.AddRange(_Subjects);
                                        listsub.DCESubjectOptionList[$"{_BaseSemCoreSubject[i].Subject_ID.ToString()}#{_BaseSemCoreSubject[i].SubjectFullName}"] = _Subjects;
                                        listsub.DSESubjectOptionList[$"{_BaseSemCoreSubject[i].Subject_ID.ToString()}#{_BaseSemCoreSubject[i].SubjectFullName}"] = _Subjects;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    parameter = new Parameters() { Filters = new List<SearchFilter>() { new SearchFilter() { TableAlias = "S", Column = "Programme", Value = new CourseManager().GetPrintProgrammesInClause(_PrevSemCombination.Course_ID), Operator = SQLOperator.In }, new SearchFilter() { TableAlias = "S", Column = "Status", Value = "1", Operator = SQLOperator.EqualTo }, new SearchFilter() { Column = "Semester", Value = _SetCombinationForSemester.ToString(), Operator = SQLOperator.EqualTo }, new SearchFilter() { Column = "SubjectType", Value = ((short)SubjectType.DCE).ToString() + "," + ((short)SubjectType.DSE).ToString(), Operator = SQLOperator.In }, new SearchFilter() { TableAlias = "S", Column = "Course_ID", Value = _PrevSemCombination.Course_ID.ToString(), Operator = SQLOperator.EqualTo } }, PageInfo = new Paging() { DefaultOrderByColumn = "SubjectFullName", PageNumber = -1, PageSize = -1 }, SortInfo = new Sort() { ColumnName = "SubjectFullName", OrderBy = SortOrder.Ascending } };
                    var _Subjects = new SubjectsManager().GetAllSubjectList(parameter);
                    if (_Subjects.IsNotNullOrEmpty())
                    {
                        for (int i = 0; i < _CombinationSetting.CombinationSettingStructure.DCE_DSE_Count; i++)
                        {
                            _NewSubjectOptions.AddRange(_Subjects);
                            listsub.DCESubjectOptionList[$"{i.ToString()}#{i.ToString()}"] = _Subjects;
                            listsub.DSESubjectOptionList[$"{i.ToString()}#{i.ToString()}"] = _Subjects;
                        }
                    }
                }
            }
            if (_CombinationSetting.CombinationSettingStructure.Major_Count > 0)
            {
                var _PrevSemMajSubjects = _PrevSemCombination.Subjects.Where(x => x.SubjectType == SubjectType.Major)?.ToList();
                int loopCount = _CombinationSetting.CombinationSettingStructure.Major_Count > _PrevSemMajSubjects.Count ? _CombinationSetting.CombinationSettingStructure.Major_Count : _PrevSemMajSubjects.Count;
                for (int i = 0; i < loopCount; i++)
                {
                    if (_CombinationSetting.CombinationSettingStructure.MajorDependOnPrevSem && i < _PrevSemMajSubjects.Count)
                    {
                        var _SubjectSettings = _CSManager.SubjectCombinationSettingList(_PrevSemMajSubjects[i].Subject_ID, _CombinationSetting.Semester)?.Where(x => x.DependentSubject_ID != null)?.Select(x => x.DependentSubject_ID)?.ToIN()?.Replace("'", "");
                        if (!string.IsNullOrEmpty(_SubjectSettings))
                        {
                            parameter = new Parameters() { Filters = new List<SearchFilter>() { new SearchFilter() { Value = ((short)SubjectType.Major).ToString(), Column = "SubjectType", Operator = SQLOperator.EqualTo }, new SearchFilter() { Value = _SubjectSettings, Column = "Subject_ID", Operator = SQLOperator.In }, new SearchFilter() { TableAlias = "S", Column = "Status", Value = "1", Operator = SQLOperator.EqualTo }, new SearchFilter() { Column = "Semester", Value = _SetCombinationForSemester.ToString(), Operator = SQLOperator.EqualTo } }, PageInfo = new Paging() { DefaultOrderByColumn = "SubjectFullName", PageNumber = -1, PageSize = -1 }, SortInfo = new Sort() { ColumnName = "SubjectFullName", OrderBy = System.Data.SqlClient.SortOrder.Ascending } };
                            var _Subjects = new SubjectsManager().GetAllSubjectList(parameter);
                            if (_Subjects.IsNotNullOrEmpty())
                            {
                                _NewSubjectOptions.AddRange(_Subjects);
                                listsub.MajorSubjectOptionList[$"{_PrevSemMajSubjects[i].Subject_ID.ToString()}#{_PrevSemMajSubjects[i].SubjectFullName}"] = _Subjects;
                            }
                        }
                    }

                    else
                    {
                        parameter = new Parameters() { Filters = new List<SearchFilter>() { new SearchFilter() { TableAlias = "S", Column = "Programme", Value = new CourseManager().GetPrintProgrammesInClause(_PrevSemCombination.Course_ID), Operator = SQLOperator.In }, new SearchFilter() { TableAlias = "S", Column = "Status", Value = "1", Operator = SQLOperator.EqualTo }, new SearchFilter() { Column = "Semester", Value = _SetCombinationForSemester.ToString(), Operator = SQLOperator.EqualTo }, new SearchFilter() { Column = "SubjectType", Value = ((short)SubjectType.Major).ToString(), Operator = SQLOperator.In } }, PageInfo = new Paging() { DefaultOrderByColumn = "SubjectFullName", PageNumber = -1, PageSize = -1 }, SortInfo = new Sort() { ColumnName = "SubjectFullName", OrderBy = SortOrder.Ascending } };
                        var _Subjects = new SubjectsManager().GetAllSubjectList(parameter);
                        if (_Subjects.IsNotNullOrEmpty())
                        {

                            _NewSubjectOptions.AddRange(_Subjects);
                            listsub.MajorSubjectOptionList[$"{i.ToString()}#{i.ToString()}"] = _Subjects;
                        }
                    }
                }
            }

            if (_CombinationSetting.CombinationSettingStructure.MinorInterDisciplinary_Count > 0)
            {
                var _PrevSemMIDSubjects = _PrevSemCombination.Subjects.Where(x => x.SubjectType == SubjectType.MID)?.ToList();
                int loopCount = _CombinationSetting.CombinationSettingStructure.MinorInterDisciplinary_Count > _PrevSemMIDSubjects.Count ? _CombinationSetting.CombinationSettingStructure.MinorInterDisciplinary_Count : _PrevSemMIDSubjects.Count;
                for (int i = 0; i < loopCount; i++)
                {
                    if (_CombinationSetting.CombinationSettingStructure.MIDDependOnPrevSem && i < _PrevSemMIDSubjects.Count)
                    {
                        var _SubjectSettings = _CSManager.SubjectCombinationSettingList(_PrevSemMIDSubjects[i].Subject_ID, _CombinationSetting.Semester)?.Where(x => x.DependentSubject_ID != null)?.Select(x => x.DependentSubject_ID)?.ToIN()?.Replace("'", "");
                        if (!string.IsNullOrEmpty(_SubjectSettings))
                        {
                            parameter = new Parameters() { Filters = new List<SearchFilter>() { new SearchFilter() { Value = ((short)SubjectType.MID).ToString(), Column = "SubjectType", Operator = SQLOperator.EqualTo }, new SearchFilter() { Value = _SubjectSettings, Column = "Subject_ID", Operator = SQLOperator.In }, new SearchFilter() { TableAlias = "S", Column = "Status", Value = "1", Operator = SQLOperator.EqualTo }, new SearchFilter() { Column = "Semester", Value = _SetCombinationForSemester.ToString(), Operator = SQLOperator.EqualTo } }, PageInfo = new Paging() { DefaultOrderByColumn = "SubjectFullName", PageNumber = -1, PageSize = -1 }, SortInfo = new Sort() { ColumnName = "SubjectFullName", OrderBy = System.Data.SqlClient.SortOrder.Ascending } };
                            var _Subjects = new SubjectsManager().GetAllSubjectList(parameter);
                            if (_Subjects.IsNotNullOrEmpty())
                            {
                                _NewSubjectOptions.AddRange(_Subjects);
                                listsub.MIDSubjectOptionList[$"{_PrevSemMIDSubjects[i].Subject_ID.ToString()}#{_PrevSemMIDSubjects[i].SubjectFullName}"] = _Subjects;
                            }
                        }
                    }

                    else
                    {
                        parameter = new Parameters() { Filters = new List<SearchFilter>() { new SearchFilter() { TableAlias = "S", Column = "Programme", Value = new CourseManager().GetPrintProgrammesInClause(_PrevSemCombination.Course_ID), Operator = SQLOperator.In }, new SearchFilter() { TableAlias = "S", Column = "Status", Value = "1", Operator = SQLOperator.EqualTo }, new SearchFilter() { Column = "Semester", Value = _SetCombinationForSemester.ToString(), Operator = SQLOperator.EqualTo }, new SearchFilter() { Column = "SubjectType", Value = ((short)SubjectType.MID).ToString(), Operator = SQLOperator.In } }, PageInfo = new Paging() { DefaultOrderByColumn = "SubjectFullName", PageNumber = -1, PageSize = -1 }, SortInfo = new Sort() { ColumnName = "SubjectFullName", OrderBy = SortOrder.Ascending } };
                        var _Subjects = new SubjectsManager().GetAllSubjectList(parameter);
                        if (_Subjects.IsNotNullOrEmpty())
                        {

                            _NewSubjectOptions.AddRange(_Subjects);
                            listsub.MIDSubjectOptionList[$"{i.ToString()}#{i.ToString()}"] = _Subjects;
                        }
                    }
                }
            }

            if (_CombinationSetting.CombinationSettingStructure.MinorVocational_Count > 0)
            {
                parameter = new Parameters() { Filters = new List<SearchFilter>() { new SearchFilter() { TableAlias = "S", Column = "Programme", Value = new CourseManager().GetPrintProgrammesInClause(_PrevSemCombination.Course_ID), Operator = SQLOperator.In }, new SearchFilter() { TableAlias = "S", Column = "Status", Value = "1", Operator = SQLOperator.EqualTo }, new SearchFilter() { Column = "Semester", Value = _SetCombinationForSemester.ToString(), Operator = SQLOperator.EqualTo }, new SearchFilter() { Column = "SubjectType", Value = ((short)SubjectType.MVoc).ToString(), Operator = SQLOperator.In } }, PageInfo = new Paging() { DefaultOrderByColumn = "SubjectFullName", PageNumber = -1, PageSize = -1 }, SortInfo = new Sort() { ColumnName = "SubjectFullName", OrderBy = SortOrder.Ascending } };
                var _Subjects = new SubjectsManager().GetAllSubjectList(parameter);
                if (_Subjects.IsNotNullOrEmpty())
                {
                    for (int i = 0; i < _CombinationSetting.CombinationSettingStructure.MinorVocational_Count; i++)
                    {
                        _NewSubjectOptions.AddRange(_Subjects);
                        listsub.MVocSubjectOptionList[$"{i.ToString()}#{i.ToString()}"] = _Subjects;
                    }
                }
            }
            if (_CombinationSetting.CombinationSettingStructure.MultiDisciplinary_Count > 0)
            {
                var _PrevSemMDiscSubjects = _PrevSemCombination.Subjects.Where(x => x.SubjectType == SubjectType.MDisc)?.ToList();
                int loopCount = _CombinationSetting.CombinationSettingStructure.MultiDisciplinary_Count > _PrevSemMDiscSubjects.Count ? _CombinationSetting.CombinationSettingStructure.MultiDisciplinary_Count : _PrevSemMDiscSubjects.Count;
                for (int i = 0; i < loopCount; i++)
                {
                    if (_CombinationSetting.CombinationSettingStructure.MDiscDependOnPrevSem && i < _PrevSemMDiscSubjects.Count)
                    {
                        var _SubjectSettings = _CSManager.SubjectCombinationSettingList(_PrevSemMDiscSubjects[i].Subject_ID, _CombinationSetting.Semester)?.Where(x => x.DependentSubject_ID != null)?.Select(x => x.DependentSubject_ID)?.ToIN()?.Replace("'", "");
                        if (!string.IsNullOrEmpty(_SubjectSettings))
                        {
                            parameter = new Parameters() { Filters = new List<SearchFilter>() { new SearchFilter() { Value = ((short)SubjectType.MDisc).ToString(), Column = "SubjectType", Operator = SQLOperator.EqualTo }, new SearchFilter() { Value = _SubjectSettings, Column = "Subject_ID", Operator = SQLOperator.In }, new SearchFilter() { TableAlias = "S", Column = "Status", Value = "1", Operator = SQLOperator.EqualTo }, new SearchFilter() { Column = "Semester", Value = _SetCombinationForSemester.ToString(), Operator = SQLOperator.EqualTo } }, PageInfo = new Paging() { DefaultOrderByColumn = "SubjectFullName", PageNumber = -1, PageSize = -1 }, SortInfo = new Sort() { ColumnName = "SubjectFullName", OrderBy = System.Data.SqlClient.SortOrder.Ascending } };
                            var _Subjects = new SubjectsManager().GetAllSubjectList(parameter);
                            if (_Subjects.IsNotNullOrEmpty())
                            {
                                _NewSubjectOptions.AddRange(_Subjects);
                                listsub.MDiscSubjectOptionList[$"{_PrevSemMDiscSubjects[i].Subject_ID.ToString()}#{_PrevSemMDiscSubjects[i].SubjectFullName}"] = _Subjects;
                            }
                        }
                    }
                    else
                    {
                        parameter = new Parameters() { Filters = new List<SearchFilter>() { new SearchFilter() { TableAlias = "S", Column = "Programme", Value = new CourseManager().GetPrintProgrammesInClause(_PrevSemCombination.Course_ID), Operator = SQLOperator.In }, new SearchFilter() { TableAlias = "S", Column = "Status", Value = "1", Operator = SQLOperator.EqualTo }, new SearchFilter() { Column = "Semester", Value = _SetCombinationForSemester.ToString(), Operator = SQLOperator.EqualTo }, new SearchFilter() { Column = "SubjectType", Value = ((short)SubjectType.MDisc).ToString(), Operator = SQLOperator.In } }, PageInfo = new Paging() { DefaultOrderByColumn = "SubjectFullName", PageNumber = -1, PageSize = -1 }, SortInfo = new Sort() { ColumnName = "SubjectFullName", OrderBy = SortOrder.Ascending } };
                        var _Subjects = new SubjectsManager().GetAllSubjectList(parameter);
                        if (_Subjects.IsNotNullOrEmpty())
                        {
                            _NewSubjectOptions.AddRange(_Subjects);
                            listsub.MDiscSubjectOptionList[$"{i.ToString()}#{i.ToString()}"] = _Subjects;
                        }
                    }
                }
            }
            if (_CombinationSetting.CombinationSettingStructure.Internship_Count > 0)
            {
                parameter = new Parameters() { Filters = new List<SearchFilter>() { new SearchFilter() { TableAlias = "S", Column = "Programme", Value = new CourseManager().GetPrintProgrammesInClause(_PrevSemCombination.Course_ID), Operator = SQLOperator.In }, new SearchFilter() { TableAlias = "S", Column = "Status", Value = "1", Operator = SQLOperator.EqualTo }, new SearchFilter() { Column = "Semester", Value = _SetCombinationForSemester.ToString(), Operator = SQLOperator.EqualTo }, new SearchFilter() { Column = "SubjectType", Value = ((short)SubjectType.Internship).ToString(), Operator = SQLOperator.In } }, PageInfo = new Paging() { DefaultOrderByColumn = "SubjectFullName", PageNumber = -1, PageSize = -1 }, SortInfo = new Sort() { ColumnName = "SubjectFullName", OrderBy = SortOrder.Ascending } };
                var _Subjects = new SubjectsManager().GetAllSubjectList(parameter);
                if (_Subjects.IsNotNullOrEmpty())
                {
                    for (int i = 0; i < _CombinationSetting.CombinationSettingStructure.Internship_Count; i++)
                    {
                        _NewSubjectOptions.AddRange(_Subjects);
                        listsub.InternshipSubjectOptionList[$"{i.ToString()}#{i.ToString()}"] = _Subjects;
                    }
                }
            }
            if (_CombinationSetting.CombinationSettingStructure.Seminar_Count > 0)
            {
                parameter = new Parameters() { Filters = new List<SearchFilter>() { new SearchFilter() { TableAlias = "S", Column = "Programme", Value = new CourseManager().GetPrintProgrammesInClause(_PrevSemCombination.Course_ID), Operator = SQLOperator.In }, new SearchFilter() { TableAlias = "S", Column = "Status", Value = "1", Operator = SQLOperator.EqualTo }, new SearchFilter() { Column = "Semester", Value = _SetCombinationForSemester.ToString(), Operator = SQLOperator.EqualTo }, new SearchFilter() { Column = "SubjectType", Value = ((short)SubjectType.Seminar).ToString(), Operator = SQLOperator.In } }, PageInfo = new Paging() { DefaultOrderByColumn = "SubjectFullName", PageNumber = -1, PageSize = -1 }, SortInfo = new Sort() { ColumnName = "SubjectFullName", OrderBy = SortOrder.Ascending } };
                var _Subjects = new SubjectsManager().GetAllSubjectList(parameter);
                if (_Subjects.IsNotNullOrEmpty())
                {
                    for (int i = 0; i < _CombinationSetting.CombinationSettingStructure.Seminar_Count; i++)
                    {
                        _NewSubjectOptions.AddRange(_Subjects);
                        listsub.SeminarSubjectOptionList[$"{i.ToString()}#{i.ToString()}"] = _Subjects;
                    }
                }
            }
            if (_CombinationSetting.CombinationSettingStructure.Research_Count > 0)
            {
                parameter = new Parameters() { Filters = new List<SearchFilter>() { new SearchFilter() { TableAlias = "S", Column = "Programme", Value = new CourseManager().GetPrintProgrammesInClause(_PrevSemCombination.Course_ID), Operator = SQLOperator.In }, new SearchFilter() { TableAlias = "S", Column = "Status", Value = "1", Operator = SQLOperator.EqualTo }, new SearchFilter() { Column = "Semester", Value = _SetCombinationForSemester.ToString(), Operator = SQLOperator.EqualTo }, new SearchFilter() { Column = "SubjectType", Value = ((short)SubjectType.Research).ToString(), Operator = SQLOperator.In } }, PageInfo = new Paging() { DefaultOrderByColumn = "SubjectFullName", PageNumber = -1, PageSize = -1 }, SortInfo = new Sort() { ColumnName = "SubjectFullName", OrderBy = SortOrder.Ascending } };
                var _Subjects = new SubjectsManager().GetAllSubjectList(parameter);
                if (_Subjects.IsNotNullOrEmpty())
                {
                    for (int i = 0; i < _CombinationSetting.CombinationSettingStructure.Research_Count; i++)
                    {
                        _NewSubjectOptions.AddRange(_Subjects);
                        listsub.ResearchSubjectOptionList[$"{i.ToString()}#{i.ToString()}"] = _Subjects;
                    }
                }
            }
            if (_CombinationSetting.CombinationSettingStructure.VAC_Count > 0)
            {
                parameter = new Parameters() { Filters = new List<SearchFilter>() { new SearchFilter() { TableAlias = "S", Column = "Programme", Value = new CourseManager().GetPrintProgrammesInClause(_PrevSemCombination.Course_ID), Operator = SQLOperator.In }, new SearchFilter() { TableAlias = "S", Column = "Status", Value = "1", Operator = SQLOperator.EqualTo }, new SearchFilter() { Column = "Semester", Value = _SetCombinationForSemester.ToString(), Operator = SQLOperator.EqualTo }, new SearchFilter() { Column = "SubjectType", Value = ((short)SubjectType.VAC).ToString(), Operator = SQLOperator.In } }, PageInfo = new Paging() { DefaultOrderByColumn = "SubjectFullName", PageNumber = -1, PageSize = -1 }, SortInfo = new Sort() { ColumnName = "SubjectFullName", OrderBy = SortOrder.Ascending } };
                var _Subjects = new SubjectsManager().GetAllSubjectList(parameter);
                if (_Subjects.IsNotNullOrEmpty())
                {
                    for (int i = 0; i < _CombinationSetting.CombinationSettingStructure.VAC_Count; i++)
                    {
                        _NewSubjectOptions.AddRange(_Subjects);
                        listsub.VACSubjectOptionList[$"{i.ToString()}#{i.ToString()}"] = _Subjects;
                    }
                }
            }
            if (_CombinationSetting.CombinationSettingStructure.AE_Count > 0)
            {
                parameter = new Parameters() { Filters = new List<SearchFilter>() { new SearchFilter() { TableAlias = "S", Column = "Programme", Value = new CourseManager().GetPrintProgrammesInClause(_PrevSemCombination.Course_ID), Operator = SQLOperator.In }, new SearchFilter() { TableAlias = "S", Column = "Status", Value = "1", Operator = SQLOperator.EqualTo }, new SearchFilter() { Column = "Semester", Value = _SetCombinationForSemester.ToString(), Operator = SQLOperator.EqualTo }, new SearchFilter() { Column = "SubjectType", Value = ((short)SubjectType.AE).ToString(), Operator = SQLOperator.In } }, PageInfo = new Paging() { DefaultOrderByColumn = "SubjectFullName", PageNumber = -1, PageSize = -1 }, SortInfo = new Sort() { ColumnName = "SubjectFullName", OrderBy = SortOrder.Ascending } };
                var _Subjects = new SubjectsManager().GetAllSubjectList(parameter);
                if (_Subjects.IsNotNullOrEmpty())
                {
                    for (int i = 0; i < _CombinationSetting.CombinationSettingStructure.AE_Count; i++)
                    {
                        _NewSubjectOptions.AddRange(_Subjects);
                        listsub.AESubjectOptionList[$"{i.ToString()}#{i.ToString()}"] = _Subjects;
                    }
                }
            }
            if (_CombinationSetting.CombinationSettingStructure.AdditionalCourseCount > 0)
            {
                parameter = new Parameters() { Filters = new List<SearchFilter>() { new SearchFilter() { Column = "Programme", Value = new CourseManager().GetPrintProgrammesInClause(_PrevSemCombination.Course_ID), Operator = SQLOperator.In, TableAlias = "S", }, new SearchFilter() { TableAlias = "S", Column = "Status", Value = "1", Operator = SQLOperator.EqualTo }, new SearchFilter() { Column = "Semester", Value = _SetCombinationForSemester.ToString(), Operator = SQLOperator.EqualTo }, new SearchFilter() { Column = "SubjectType", Value = $"{((short)SubjectType.Core).ToString()},{((short)SubjectType.OC).ToString()},{((short)SubjectType.DCE).ToString()},{((short)SubjectType.DSE).ToString()}", Operator = SQLOperator.In }, new SearchFilter() { TableAlias = "S", Column = "Course_ID", Value = _PrevSemCombination.Course_ID.ToString(), Operator = SQLOperator.EqualTo } }, PageInfo = new Paging() { DefaultOrderByColumn = "SubjectFullName", PageNumber = -1, PageSize = -1 }, SortInfo = new Sort() { ColumnName = "SubjectFullName", OrderBy = SortOrder.Ascending } };
                var _Subjects = new SubjectsManager().GetAllSubjectList(parameter);
                if (_Subjects.IsNotNullOrEmpty())
                {
                    for (int i = 0; i < _CombinationSetting.CombinationSettingStructure.AdditionalCourseCount; i++)
                    {
                        _NewSubjectOptions.AddRange(_Subjects);
                        listsub.AdditionalSubjectOptionList[$"{i.ToString()}#{i.ToString()}"] = _Subjects;
                    }
                }
            }

            return Tuple.Create(_NewSubjectOptions, listsub, new ResponseData() { IsSuccess = true });
        }
        ResponseData ValidateByResult(Guid Student_ID, short Semester, PrintProgramme printProgramme)
        {
            if (new ResultManager().ResultCreated(printProgramme, Semester, Student_ID))
                return new ResponseData() { ErrorMessage = $"Result already created" };
            return new ResponseData() { IsSuccess = true };
        }



        public int GetSubjectCount(PrintProgramme printProgramme, short semester, Guid subject_ID, SubjectType subjectType, short Batch, Guid? College_ID = null)
        {
            return new AssignCombinationDB().GetSubjectCount(printProgramme, semester, subject_ID, subjectType, Batch, College_ID);
        }

        private ADMCombinationMaster GetSetCombinationMaster(ADMCombinationMaster aDMCombination)
        {
            aDMCombination.CombinationHelper = new CombinationHelper() { FinalCombinations = new List<BaseCombinationHelper>() };
            var combinationCode = string.Empty;
            var courseLetters = string.Empty;
            foreach (var subject in aDMCombination.SubjectsDetails ?? new List<ADMSubjectMaster>())
            {
                combinationCode += subject.SubjectFullName.Trim().Substring(0, 1);
                aDMCombination.CombinationHelper.FinalCombinations.Add(new BaseCombinationHelper() { Subject_ID = subject.Subject_ID });
            }
            ADMCourseMaster courseMaster = new CourseManager().GetCourseById(aDMCombination.Course_ID ?? Guid.Empty);

            if (courseMaster != null)
            {
                foreach (var item in courseMaster.CourseFullName.Trim().Split(' '))
                {
                    courseLetters += item.ToUpper().Trim().Substring(0, 1);
                }
            }

            combinationCode = aDMCombination.CombinationCode + combinationCode + courseLetters;
            aDMCombination.CombinationCode = combinationCode;
            var combinationManager = new CombinationManager();
            IEnumerable<Guid?> FinalCombinations = aDMCombination.CombinationHelper.FinalCombinations.Select(i => i.Subject_ID).ToList();
            var _admCombinatioMaster = combinationManager.GetCombinationBySubjects(aDMCombination.College_ID, FinalCombinations, aDMCombination.Course_ID ?? Guid.Empty);
            if (_admCombinatioMaster != null)
                return _admCombinatioMaster;
            var noOfRecordsEffected = 0;
            if (_admCombinatioMaster == null)
            {
                try
                {
                    aDMCombination.CombinationCode = GetCombinationCode(aDMCombination.CombinationCode, aDMCombination.CombinationCode, aDMCombination.Semester.Value, aDMCombination.College_ID);
                    noOfRecordsEffected = combinationManager.Save(aDMCombination);
                }
                catch (Exception)
                { }
            }

            return noOfRecordsEffected == 1 ? new CombinationManager().GetCombinationByID(aDMCombination.Combination_ID) : null;
        }

        string GetCombinationCode(string OCombinationCode, string combinationCode, int suffix, Guid College_ID)
        {
            combinationCode = OCombinationCode + suffix;
            if (!new CombinationManager().CombinationCodeAlreadyExists(combinationCode, College_ID) || suffix > 20)
            {
                return OCombinationCode + suffix;
            }
            return GetCombinationCode(OCombinationCode, combinationCode, ++suffix, College_ID);
        }

        //private void SaveARGSelectedCombination(Guid course_ID, short semester, Guid student_ID, Guid combination_ID, string combinationSubjects, PrintProgramme printProgramme)
        //{

        //    ARGSelectedCombination selectedCombination = new ARGSelectedCombination()
        //    {
        //        College_ID = (Guid)AppUserHelper.College_ID,
        //        Combination_ID = combination_ID,
        //        CombinationSubjects = combinationSubjects,
        //        PrintProgramme = printProgramme,
        //        Semester = semester,
        //        SelectedCombination_ID = Guid.NewGuid(),
        //        Student_ID = student_ID,
        //        Course_ID = course_ID
        //    };

        //    selectedCombination.SetWorkFlow(RecordState.New);

        //    new AssignCombinationManager().SaveUpdate(new List<ARGSelectedCombination>() { selectedCombination }, printProgramme);
        //}

        //private void SaveSelectedCombination(Guid course_ID, short semester, Guid student_ID, List<ADMSubjectMaster> subjects, List<ADMSubjectMaster> skillEnhancementSubjects)
        //{
        //    StudentSelectedSubject selectedsubjects = new StudentDB().GetStudentSelectedSubject(student_ID, semester);
        //    if (selectedsubjects == null)
        //    {
        //        selectedsubjects = new StudentSelectedSubject()
        //        {
        //            Course_ID = course_ID,
        //            Semester = semester,
        //            Student_ID = student_ID,
        //            CoreSubject = string.Join("|", subjects.Select(x => x.Subject_ID).ToList()),
        //        };
        //        selectedsubjects.SetWorkFlow(RecordState.New);
        //        new StudentDB().SaveSelectedCombination(selectedsubjects);
        //    }
        //    else
        //    {
        //        selectedsubjects.Course_ID = course_ID;
        //        selectedsubjects.CoreSubject = string.Join("|", subjects.Select(x => x.Subject_ID).ToList());
        //        selectedsubjects.SetWorkFlow(RecordState.Old);
        //        new StudentDB().UpdateSelectedCombination(selectedsubjects);
        //    }
        //}

        public ResponseData DeleteStudentCombinations(Guid Student_ID, PrintProgramme printProgramme, short Semester)
        {
            ResponseData response = new ResponseData();
            printProgramme = new RegistrationManager().MappingTable(printProgramme);
            var studentInfo = new StudentManager().GetStudentWithCombination(Student_ID, printProgramme, false, Semester);
            if (studentInfo == null)
                return new ResponseData() { ErrorMessage = "Student not found" };

            if (!studentInfo.AcceptCollege_ID.IsNullOrEmpty() && studentInfo.AcceptCollege_ID != AppUserHelper.College_ID)
                return new ResponseData() { ErrorMessage = "Student belongs to " + studentInfo.AcceptCollegeID };
            if (studentInfo.SelectedCombinations.IsNullOrEmpty() || studentInfo.SelectedCombinations.Where(x => x.Semester == Semester).IsNullOrEmpty())
                return new ResponseData() { ErrorMessage = "No, combination has been assigned yet." };

            //if (studentInfo.CUSRegistrationNo.IsNotNullOrEmpty())
            //    return new ResponseData() { ErrorMessage = "Registration number has been allotted to this student, please send its details to cluster university in order to delete him." };

            if (studentInfo.CurrentSemesterOrYear > 7 && studentInfo.IsLateralEntry)
                return new ResponseData() { ErrorMessage = "This student belongs to " + studentInfo.CurrentSemesterOrYear + " semester, please send its details to cluster university in order to delete him." };
            else if (studentInfo.CurrentSemesterOrYear > 1 && Semester == 1)
                return new ResponseData() { ErrorMessage = "This student belongs to " + studentInfo.CurrentSemesterOrYear + " semester, please send its details to cluster university in order to delete him." };
            var batch = studentInfo.Batch;
            if (studentInfo.IsLateralEntry)
                batch = studentInfo.SelectedCombinations.FirstOrDefault(x => x.Semester == Semester).SemesterBatch;

            var _CS = new CombinationSettingManager().GetCombinationSetting(studentInfo.SelectedCombinations.First().Course_ID, Semester, batch);
            if (_CS == null)
                return new ResponseData() { ErrorMessage = "Combination setting not found. Please contact Cluster University I.T Section." };
            if (!_CS.AllowDeleteCombination)
                return new ResponseData() { ErrorMessage = "The process of deletion has been closed." };

            using (TransactionScope transaction = new TransactionScope())
            {
                response.NumberOfRecordsEffected = new AssignCombinationDB().DeleteStudentCombinations(Student_ID, Semester, printProgramme);
                if (response.NumberOfRecordsEffected > 0)
                {
                    int studentUpdateStatus = 0;
                    if (Semester <= 1)
                    {
                        studentInfo.FormStatus = studentInfo.FormStatus == FormStatus.Selected ? FormStatus.FeePaid : FormStatus.InProcess;
                        studentInfo.CurrentSemesterOrYear = 0;
                        studentInfo.AcceptCollege_ID = null;
                        studentInfo.AcceptedBy_ID = null;
                        studentInfo.ClassRollNo = null;
                        // studentInfo.CurrentBatch = null;
                        studentUpdateStatus = new StudentManager().UpdateStudentCombinationInfo(studentInfo, printProgramme);
                        //new AssignCombinationDB().DeleteStudentSemester1Award(Student_ID, printProgramme);
                        if (printProgramme == PrintProgramme.UG)
                            new StudentDB().UpdateSelectionStatus(studentInfo.Student_ID, Guid.Parse("A3EE7F98-7B82-4D95-A2C0-FABA7A18240E"), StudentSelectionStatus.Joined);
                        //else
                        //    response.NumberOfRecordsEffected = new PaymentDB().Delete(studentInfo.Student_ID, 1, PaymentModuleType.SemesterAdmission, printProgramme);
                    }
                    else
                    {
                        studentUpdateStatus = new StudentManager().UpdateStudentCombinationInfo(studentInfo.Student_ID, printProgramme);
                    }
                }
                transaction.Complete();
            }
            if (response.NumberOfRecordsEffected > 0)
            {
                response.IsSuccess = true;
                response.SuccessMessage = "Record deleted successfully.";
            }
            return response;
        }

        /// <summary>
        /// Returns the number of combination assigned to students
        /// </summary>
        /// <param name="Combination_ID"></param>
        /// <returns></returns>
        public int GetCombinationCount(Guid Combination_ID, short Semester, short Batch, PrintProgramme printProgramme)
        {
            return new AssignCombinationDB().GetCombinationCount(Combination_ID, Semester, Batch, printProgramme);
        }

        //public int GetActualCombinationCount(Guid subjectCombination_ID, Guid College_ID)
        //{
        //    return new AssignCombinationDB().GetActualCombinationCount(subjectCombination_ID, College_ID);
        //}


        private string GetCombination(Guid Course_ID, Guid combinationId)
        {
            CombinationsCount CombinationsCount = new AssignCombinationDB().GetCombination(combinationId);
            string combinationName = CombinationsCount.CombinationCode + "=";


            if (!string.IsNullOrWhiteSpace(CombinationsCount.CombinationSubjects))
            {

                List<ADMSubjectMaster> subjects = (Course_ID == null
                         ? new SubjectDB().GetList(CombinationsCount.CombinationSubjects.ToGuidList())
                         : new SubjectDB().GetList(Course_ID, CombinationsCount.CombinationSubjects.ToGuidList()));

                foreach (ADMSubjectMaster subject in subjects)
                {
                    combinationName = combinationName + subject.SubjectFullName + "|";
                }
            }
            return combinationName;
        }

        public string GetFileNameForCombination(List<SearchFilter> filters)
        {
            if (filters.Where(x => x.Column == "Course").Count() == 0)
            { return "SubjectWiseCount"; }
            string coursename = new AssignCombinationDB().GetCourseName(new Guid(filters.Where(x => x.Column == "Course").FirstOrDefault().Value)).CourseFullName;
            if (coursename.IsNullOrEmpty())
            { return "SubjectWiseCount"; }
            string combinationName = "";
            if (filters.Where(x => x.Column == "Combination").Count() > 0)
            {
                combinationName = "(" + GetCombination(new Guid(filters.Where(x => x.Column == "Course").FirstOrDefault().Value), new Guid(filters.Where(x => x.Column == "Combination").FirstOrDefault().Value)) + ")";
            }
            return (coursename + combinationName).Length > 195 ? (coursename + combinationName).Substring(0, 194) : (coursename + combinationName);
        }

        //public Parameters CreateExcelFilter(ExcelFilterCombination filter)
        //{
        //    Parameters parameter = new Parameters();
        //    parameter.PageInfo = null;
        //    List<SearchFilter> filters = new List<SearchFilter>();
        //    parameter.SortInfo = new Sort
        //    {
        //        ColumnName = "Percentage",
        //        OrderBy = SortOrder.Ascending
        //    };
        //    SearchFilter sfilter;
        //    if (!string.IsNullOrEmpty(filter.Combination))
        //    {
        //        sfilter = new SearchFilter();
        //        sfilter.Column = "Combination";
        //        sfilter.Value = filter.Combination;
        //        sfilter.Operator = SQLOperator.EqualTo;
        //        filters.Add(sfilter);
        //    }
        //    sfilter = new SearchFilter();
        //    sfilter.Column = "Batch";
        //    sfilter.Value = filter.Batch;
        //    sfilter.Operator = SQLOperator.EqualTo;
        //    filters.Add(sfilter);
        //    sfilter = new SearchFilter();
        //    sfilter.Column = "Course";
        //    sfilter.Value = filter.Course;
        //    sfilter.Operator = SQLOperator.EqualTo;
        //    filters.Add(sfilter);
        //    sfilter = new SearchFilter();
        //    sfilter.Column = "Programme";
        //    sfilter.Value = filter.Programme;
        //    sfilter.Operator = SQLOperator.EqualTo;
        //    filters.Add(sfilter);
        //    parameter.Filters = filters;
        //    sfilter = new SearchFilter();
        //    sfilter.Column = "Semester";
        //    sfilter.Value = filter.Semester;
        //    sfilter.Operator = SQLOperator.EqualTo;
        //    filters.Add(sfilter);
        //    parameter.Filters = filters;
        //    return parameter;
        //}


        #region Import Ih to UG
        public Tuple<bool, string> ImportIHtoUG(string CUSFormNo)
        {
            #region Validation
            if (string.IsNullOrWhiteSpace(CUSFormNo))
                return Tuple.Create(false, "Please provide Form No.");

            AssignCombinationDB assignCombinationDB = new AssignCombinationDB();

            if (assignCombinationDB.IsAlreadyImported(CUSFormNo, PrintProgramme.UG))
                return Tuple.Create(true, "Already imported");

            ARGPersonalInformation personalInformation = assignCombinationDB.GetStudentforImport(CUSFormNo, PrintProgramme.IH);
            if (personalInformation == null)
                return Tuple.Create(false, "Form not found");

            if (personalInformation.FormStatus != FormStatus.FeePaid)
                return Tuple.Create(false, $"Student is not eligible for Import, because form status is {personalInformation.FormStatus.GetEnumDescription()}");

            if (personalInformation.CATEntrancePoints == null)
                return Tuple.Create(false, "Student not appeared in CAT entrance");

            if (!personalInformation.AcceptCollege_ID.IsNullOrEmpty())
            {
                if (personalInformation.AcceptCollege_ID == AppUserHelper.College_ID)
                    return Tuple.Create(false, "Student already accepted by your college.");
                else
                    return Tuple.Create(false, "Student already accepted by another college.");
            }

            List<ARGCoursesApplied> coursesApplied = new RegistrationDB().GetStudentCoursesApplied(personalInformation.Student_ID, PrintProgramme.IH);
            if (!coursesApplied.Any(x => x.Course_ID == Guid.Parse("A3EE7F98-7B82-4D95-A2C0-FABA7A18240E")))
                return Tuple.Create(false, "Student has not applied for Graduation");

            if (coursesApplied.Where(x => x.Course_ID == Guid.Parse("A3EE7F98-7B82-4D95-A2C0-FABA7A18240E")
                             && x.StudentSelectionStatus != StudentSelectionStatus.Joined)
                            .IsNotNullOrEmpty())
            {
                return Tuple.Create(false, "Student is not selected for Graduation yet or has not submitted University Fee. Please check the details");
            }
            #endregion

            #region Import Operation
            bool OnlyAppliedForGraduation = coursesApplied.All(x => x.Course_ID == Guid.Parse("A3EE7F98-7B82-4D95-A2C0-FABA7A18240E"));
            int result = 0;
            using (var ts = new TransactionScope())
            {
                //Migrate 
                var NewStudent_ID = Guid.NewGuid();
                result = assignCombinationDB.ImportIHtoUG(personalInformation.Student_ID, NewStudent_ID);
                if (result > 0)
                {
                    //Migrate Photograph 
                    bool PhotographExists = File.Exists(HttpContext.Current.Server.MapPath("~" + personalInformation.Photograph));
                    if (PhotographExists)
                    {
                        File.Copy(HttpContext.Current.Server.MapPath("~" + personalInformation.Photograph),
                            HttpContext.Current.Server.MapPath("~" + personalInformation.Photograph.Replace("/IH/", "/UG/")));
                    }

                    //after migrate
                    if (OnlyAppliedForGraduation)
                    {
                        result += assignCombinationDB.DeleteFromIH(personalInformation.Student_ID);
                        //delete photograph from IH 
                        if (PhotographExists)
                        {
                            File.Delete(HttpContext.Current.Server.MapPath("~" + personalInformation.Photograph));
                        }
                    }
                    else
                    {
                        //only delete graduation course
                        result += assignCombinationDB.DeleteGraduationCourseOnlyFromIH(personalInformation.Student_ID);
                        result += assignCombinationDB.DeletePaymentFromIHOnly(personalInformation.Student_ID);
                    }

                    result += new StudentDB().ReplaceStudent_ID(personalInformation.Student_ID, NewStudent_ID);

                }
                ts.Complete();
            }
            #endregion

            if (result > 0)
                return Tuple.Create(true, $"Student with Form no {CUSFormNo} migrated Successfully to UG.");
            else
                return Tuple.Create(false, $"Operation was unsuccessfull. Please contact I.T Cell University;");
        }
        #endregion


        #region LateralEntry

        public ResponseData SaveUpdateCombinationLateralEntry(ARGSelectedCombination NewSelectedCombination, Decimal? OfflinePaymentAmount, DateTime? BankPaymentDate)
        {
            ResponseData response = new ResponseData() { ErrorMessage = "Failed to save record!" };
            var studentInfo = new StudentManager().GetStudent(NewSelectedCombination.Student_ID, NewSelectedCombination.PrintProgramme, false);
            if (studentInfo.FormStatus == FormStatus.InProcess)
                return new ResponseData() { ErrorMessage = "Student has not paid university admission fee yet." };

            ARGSelectedCombination dbSelectedCombination = new StudentDB().GetSelectedCombinations(NewSelectedCombination.Student_ID, NewSelectedCombination.Semester, NewSelectedCombination.PrintProgramme);

            var _CS = new CombinationSettingManager().GetCombinationSetting(NewSelectedCombination.Course_ID, NewSelectedCombination.Semester, NewSelectedCombination.SemesterBatch);
            if (_CS == null || _CS.AllowCollegeChangeCombination == false) return new ResponseData() { ErrorMessage = "Setting not availabe." };
            //var _ValidateByCombinationSetting = ValidateByCombinationSetting(NewSelectedCombination.PrintProgramme, studentInfo, _CS, null, NewSelectedCombination);
            //if (!_ValidateByCombinationSetting.IsSuccess) return _ValidateByCombinationSetting;

            // if (_CS == null)
            //     return new ResponseData() { ErrorMessage = "Combination setting not available. Please contact cluster university I.T. Cell" };

            // if (!AppUserHelper.AppUsercompact.UserRoles.Any(x => x == AppRoles.College))
            //     return new ResponseData() { ErrorMessage = "Only college can assign combination for lateral entry." };

            // if (!_CS.CombinationSettingStructure.AllowCollegeChangeCombination)
            //    return new ResponseData() { ErrorMessage = "The process of changing combination for student has been closed. Contact college administration section if change in subjects." };

            //#region check for intack capacity
            //if (dbSelectedCombination == null || dbSelectedCombination.Course_ID != NewSelectedCombination.Course_ID)
            //{
            //    if ((!OfflinePaymentAmount.HasValue || OfflinePaymentAmount < 100) && NewSelectedCombination.PrintProgramme != PrintProgramme.UG)
            //        return new ResponseData() { ErrorMessage = "Offline payment amount is invalid" };

            //    ADMCollegeCourseMapping aDMCollegeCourseMapping = new CourseDB().GetCourseCollege(NewSelectedCombination.Course_ID)?.FirstOrDefault(x => x.College_ID == AppUserHelper.College_ID);
            //    if (aDMCollegeCourseMapping == null)
            //        return new ResponseData() { ErrorMessage = "Course not found in your college." };

            //    int totalIntackCapacity = aDMCollegeCourseMapping.MeritBasislInTackCapacity + aDMCollegeCourseMapping.SelfFinancedInTackCapacity;
            //    int totalAcceptedFormForCourse = new CombinationSettingManager().GetTotalStudentsAlloted(NewSelectedCombination.Course_ID, (Guid)AppUserHelper.College_ID, studentInfo.Batch, NewSelectedCombination.PrintProgramme);
            //    if (totalIntackCapacity > 0 && totalAcceptedFormForCourse >= totalIntackCapacity)
            //        return new ResponseData() { ErrorMessage = $"Intake Capacity ({totalIntackCapacity}) reached for this course" };
            //}
            //#endregion

            //if (!studentInfo.AcceptCollege_ID.IsNullOrEmpty() && studentInfo.AcceptCollege_ID != AppUserHelper.College_ID)
            //    return new ResponseData() { ErrorMessage = "Student belongs to " + studentInfo.AcceptCollegeID };


            //if (!new CombinationManager().CombinationExistsInCollege(AppUserHelper.College_ID.Value, NewSelectedCombination.Combination_ID, NewSelectedCombination.Course_ID, NewSelectedCombination.Semester))
            //    return new ResponseData() { ErrorMessage = "Subject combination does not exist in this college." };

            //var _Comb = new CombinationManager().GetCombinationByID(NewSelectedCombination.Combination_ID);
            //if (_Comb == null || _Comb.SubjectsDetails.IsNullOrEmpty())
            //    return new ResponseData() { ErrorMessage = "Subject combination not found." };

            //var _ValidateByCombinationSetting = ValidateByCombinationSetting(NewSelectedCombination.PrintProgramme, studentInfo, _CS, _Comb.SubjectsDetails);
            //if (!_ValidateByCombinationSetting.IsSuccess) return _ValidateByCombinationSetting;

            studentInfo.SelectedCombinations = new List<ARGSelectedCombination>() { NewSelectedCombination };
            var Payment = new PaymentManager().GetPaymentDetails(studentInfo.Student_ID, PaymentModuleType.SemesterAdmission, NewSelectedCombination.PrintProgramme, NewSelectedCombination.Semester)?.FirstOrDefault();

            using (var transcope = new TransactionScope())
            {
                if (dbSelectedCombination == null)
                {
                    dbSelectedCombination = NewSelectedCombination;
                    dbSelectedCombination.IsVerified = AppUserHelper.Designation != "Student";
                    NewSelectedCombination.SelectedCombination_ID = Guid.NewGuid();
                    NewSelectedCombination.SetWorkFlow(RecordState.New);
                    response.NumberOfRecordsEffected = new AssignCombinationDB().Save(dbSelectedCombination, dbSelectedCombination.PrintProgramme);

                    if (response.NumberOfRecordsEffected > 0 && Payment == null && OfflinePaymentAmount.HasValue && BankPaymentDate.HasValue && BankPaymentDate.Value > DateTime.MinValue)
                    {
                        PaymentDetails _payment = new PaymentDetails()
                        {
                            Payment_ID = Guid.NewGuid(),
                            PaymentType = PaymentType.Offline,
                            Semester = NewSelectedCombination.Semester,
                            AuthStatus = "0300",
                            BankID = "CUS",
                            BankMerchantID = "CUS",
                            BankReferenceNo = "CUS",
                            CurrencyName = "INR",
                            TxnReferenceNo = studentInfo.StudentFormNo,
                            TxnDate = BankPaymentDate.Value,
                            Entity_ID = studentInfo.Student_ID,
                            ModuleType = PaymentModuleType.SemesterAdmission,
                            TxnAmount = OfflinePaymentAmount.Value
                        };
                        response.NumberOfRecordsEffected += new PaymentDB().SavePaymentDetails(_payment, NewSelectedCombination.PrintProgramme);
                    }



                }
                if (Payment != null && OfflinePaymentAmount.HasValue && BankPaymentDate.HasValue && (Payment.PaymentType == PaymentType.Offline && Payment.TxnAmount != OfflinePaymentAmount.Value || Payment.TxnDate.Date != BankPaymentDate.Value.Date))
                {
                    Payment.TxnDate = BankPaymentDate.Value;
                    Payment.TxnAmount = OfflinePaymentAmount.Value;
                    response.NumberOfRecordsEffected += new PaymentDB().Update(Payment, NewSelectedCombination.PrintProgramme);
                }
                if (dbSelectedCombination.Combination_ID == NewSelectedCombination.Combination_ID && dbSelectedCombination.Semester == NewSelectedCombination.Semester && dbSelectedCombination.IsVerified)
                    response.NumberOfRecordsEffected += 1;
                else
                {
                    dbSelectedCombination.Combination_ID = NewSelectedCombination.Combination_ID;
                    dbSelectedCombination.IsVerified = AppUserHelper.AppUsercompact.UserRoles.Any(x => x != AppRoles.Student);
                    dbSelectedCombination.SetWorkFlow(RecordState.Old);
                    response.NumberOfRecordsEffected = new AssignCombinationDB().Update(dbSelectedCombination, dbSelectedCombination.PrintProgramme);
                }
                if (response.NumberOfRecordsEffected > 0)
                {
                    if (studentInfo.FormStatus != FormStatus.Selected || studentInfo.CurrentSemesterOrYear != NewSelectedCombination.Semester || !studentInfo.AcceptCollege_ID.HasValue)
                    {
                        studentInfo.FormStatus = FormStatus.Selected;
                        studentInfo.AcceptCollege_ID = AppUserHelper.College_ID;
                        studentInfo.CurrentSemesterOrYear = NewSelectedCombination.Semester > studentInfo.CurrentSemesterOrYear ? NewSelectedCombination.Semester : studentInfo.CurrentSemesterOrYear;
                        studentInfo.AcceptedBy_ID = AppUserHelper.User_ID;
                        response.NumberOfRecordsEffected += new StudentManager().UpdateStudentCombinationInfo(studentInfo, dbSelectedCombination.PrintProgramme);
                    }
                    else
                        response.NumberOfRecordsEffected += 1;
                }
                transcope.Complete();
            }

            if (response.NumberOfRecordsEffected > 0)
            {
                response.NumberOfRecordsEffected = 1;
                response.IsSuccess = true;
                response.SuccessMessage = "Record Saved successfully";
                response.ErrorMessage = "";
            }
            else
            {
                response.NumberOfRecordsEffected = 0;
                response.ErrorMessage = "Failed to save record";
            }
            return response;
        }
        public Tuple<bool, string> ImportPGtoIH(string CUSFormNo)
        {
            #region Validation
            if (string.IsNullOrWhiteSpace(CUSFormNo))
                return Tuple.Create(false, "Please provide Form No.");

            AssignCombinationDB assignCombinationDB = new AssignCombinationDB();

            if (assignCombinationDB.IsAlreadyImported(CUSFormNo, PrintProgramme.IH))
                return Tuple.Create(true, "Already imported");

            ARGPersonalInformation personalInformation = assignCombinationDB.GetStudentforImport(CUSFormNo, PrintProgramme.PG);
            if (personalInformation == null)
                return Tuple.Create(false, "Form not found");

            if (personalInformation.FormStatus != FormStatus.FeePaid)
                return Tuple.Create(false, $"Student is not eligible for Import, because form status is {personalInformation.FormStatus.GetEnumDescription()}");

            if (!personalInformation.AcceptCollege_ID.IsNullOrEmpty())
            {
                if (personalInformation.AcceptCollege_ID == AppUserHelper.College_ID)
                    return Tuple.Create(false, "Student already accepted by your college.");
                else
                    return Tuple.Create(false, "Student already accepted by another college.");
            }

            List<ARGCoursesApplied> coursesApplied = new RegistrationDB().GetStudentCoursesApplied(personalInformation.Student_ID, PrintProgramme.PG);
            if (!coursesApplied.Any(x => x.AppearedInEntrance))
                return Tuple.Create(false, "Student has not appeared in Entrance");

            #endregion

            #region Import Operation
            int result = 0;
            bool OnlyAppliedForSingleCourse = coursesApplied.Count == 1;
            using (var ts = new TransactionScope())
            {
                //Migrate 
                var NewStudent_ID = Guid.NewGuid();
                result = assignCombinationDB.ImportPGtoIH(personalInformation.Student_ID, NewStudent_ID);
                if (result > 0)
                {
                    //Migrate Photograph 
                    bool PhotographExists = File.Exists(HttpContext.Current.Server.MapPath("~" + personalInformation.Photograph));
                    if (PhotographExists)
                        File.Copy(HttpContext.Current.Server.MapPath("~" + personalInformation.Photograph), HttpContext.Current.Server.MapPath("~" + personalInformation.Photograph.Replace("/PG/", "/IH/")));

                    //after migrate will we delete it also in pg
                    if (OnlyAppliedForSingleCourse)
                    {
                        result += assignCombinationDB.DeleteFromPG(personalInformation.Student_ID);
                        //delete photograph from IH 
                        if (PhotographExists)
                        {
                            File.Delete(HttpContext.Current.Server.MapPath("~" + personalInformation.Photograph));
                        }
                    }
                    //else
                    //{
                    //    //only delete graduation course
                    //    result += assignCombinationDB.DeleteGraduationCourseOnlyFromIH(personalInformation.Student_ID);
                    //    result += assignCombinationDB.DeletePaymentFromIHOnly(personalInformation.Student_ID);
                    //}
                }
                ts.Complete();
            }
            #endregion

            if (result > 0)
                return Tuple.Create(true, $"Student with Form no {CUSFormNo} migrated Successfully to IH.");
            else
                return Tuple.Create(false, $"Operation was unsuccessfull. Please contact I.T Cell University;");
        }
        #endregion


        //particular for btech students for semester-III
        #region ChangeCourse
        public ResponseData ChangeCourse(ARGSelectedCombination NewSelectedCombination, Decimal? OfflinePaymentAmount, DateTime? BankPaymentDate)
        {
            ResponseData response = new ResponseData() { ErrorMessage = "Failed to save record!" };
            var _S = new StudentManager().GetStudent(NewSelectedCombination.Student_ID, NewSelectedCombination.PrintProgramme, false);
            if (_S.FormStatus == FormStatus.InProcess)
                return new ResponseData() { ErrorMessage = "Student has not paid university admission fee yet." };

            if (_S.AcceptCollege_ID != Guid.Parse("9D03A374-4398-4A48-BE2A-FD9911EC6F82"))
                return new ResponseData() { ErrorMessage = "Changing course not available." };

            if (_S.CurrentSemesterOrYear < 2 || _S.CurrentSemesterOrYear > 3 || NewSelectedCombination.Semester != 3)
                return new ResponseData() { ErrorMessage = $"Student is currently in Semester-{_S.CurrentSemesterOrYear}. Changing course is possible in only Semester-{NewSelectedCombination.Semester}" };

            var _CS = new CombinationSettingManager().GetCombinationSetting(NewSelectedCombination.Course_ID, NewSelectedCombination.Semester, NewSelectedCombination.SemesterBatch);
            if (_CS == null)
                return new ResponseData() { ErrorMessage = "Combination setting not found." };
            if (!_CS.AllowChangeCourse)
                return new ResponseData() { ErrorMessage = "The process of changing course has been closed. Please contact university I.T section." };

            ARGSelectedCombination dbSelectedCombination = new StudentDB().GetSelectedCombinations(NewSelectedCombination.Student_ID, NewSelectedCombination.Semester, NewSelectedCombination.PrintProgramme);

            using (var transcope = new TransactionScope())
            {
                if (dbSelectedCombination == null)
                {
                    dbSelectedCombination = NewSelectedCombination;
                    dbSelectedCombination.IsVerified = AppUserHelper.AppUsercompact.UserRoles.Any(x => x != AppRoles.Student);
                    NewSelectedCombination.SelectedCombination_ID = Guid.NewGuid();
                    NewSelectedCombination.SetWorkFlow(RecordState.New);
                    response.NumberOfRecordsEffected = new AssignCombinationDB().Save(dbSelectedCombination, dbSelectedCombination.PrintProgramme);

                    var Payment = new PaymentManager().GetPaymentDetails(_S.Student_ID, PaymentModuleType.SemesterAdmission, NewSelectedCombination.PrintProgramme, NewSelectedCombination.Semester);
                    if (response.NumberOfRecordsEffected > 0 && Payment == null && OfflinePaymentAmount.HasValue && BankPaymentDate.HasValue && BankPaymentDate.Value > DateTime.MinValue)
                    {
                        PaymentDetails _payment = new PaymentDetails()
                        {
                            Payment_ID = Guid.NewGuid(),
                            PaymentType = PaymentType.Offline,
                            Semester = NewSelectedCombination.Semester,
                            AuthStatus = "0300",
                            BankID = "CUS",
                            BankMerchantID = "CUS",
                            BankReferenceNo = "CUS",
                            CurrencyName = "INR",
                            TxnReferenceNo = _S.StudentFormNo,
                            TxnDate = BankPaymentDate.Value,
                            Entity_ID = _S.Student_ID,
                            ModuleType = PaymentModuleType.SemesterAdmission,
                            TxnAmount = OfflinePaymentAmount.Value
                        };
                        response.NumberOfRecordsEffected += new PaymentDB().SavePaymentDetails(_payment, NewSelectedCombination.PrintProgramme);
                    }
                }
                if (dbSelectedCombination.Combination_ID == NewSelectedCombination.Combination_ID && dbSelectedCombination.Semester == NewSelectedCombination.Semester && dbSelectedCombination.IsVerified)
                    response.NumberOfRecordsEffected += 1;
                else
                {
                    dbSelectedCombination.Combination_ID = NewSelectedCombination.Combination_ID;
                    dbSelectedCombination.IsVerified = AppUserHelper.AppUsercompact.UserRoles.Any(x => x != AppRoles.Student);
                    dbSelectedCombination.SetWorkFlow(RecordState.Old);
                    response.NumberOfRecordsEffected = new AssignCombinationDB().Update(dbSelectedCombination, dbSelectedCombination.PrintProgramme);
                }
                if (response.NumberOfRecordsEffected > 0)
                {
                    if (_S.CurrentSemesterOrYear != NewSelectedCombination.Semester)
                    {
                        _S.CurrentSemesterOrYear = NewSelectedCombination.Semester;
                        response.NumberOfRecordsEffected += new StudentManager().UpdateStudentCombinationInfo(_S, dbSelectedCombination.PrintProgramme);
                    }
                    else
                        response.NumberOfRecordsEffected += 1;
                }
                transcope.Complete();
            }

            if (response.NumberOfRecordsEffected > 0)
            {
                response.NumberOfRecordsEffected = 1;
                response.IsSuccess = true;
                response.SuccessMessage = "Record Saved successfully";
                response.ErrorMessage = "";
            }
            else
            {
                response.NumberOfRecordsEffected = 0;
                response.ErrorMessage = "Failed to save record";
            }
            return response;
        }

        public Guid? GetCourseIDByMajorSubjectSemester1(List<Guid> majorSubjectIDs)
        {
            if (majorSubjectIDs.IsNullOrEmpty())
                return null;

            return new AssignCombinationDB().GetCourseIDByMajorSubjectSemester1(majorSubjectIDs, SubjectType.Major, Semester: 1);
        }
        #endregion
    }


}
