using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CUSrinagar.Models;
using CUSrinagar.DataManagers;
using CUSrinagar.Enums;
using System.Transactions;
using GeneralModels;
using System.Data.SqlClient;
using CUSrinagar.Extensions;

namespace CUSrinagar.BusinessManagers
{
    public class PaymentManager
    {
        public int SavePaymentDetails(PaymentDetails payment, PrintProgramme printProgramme)
        {
            return new PaymentDB().SavePaymentDetails(payment, printProgramme);
        }

        #region Examination

        public ResponseData SavePaymentForExamination(Tuple<bool, string, PaymentDetails, Guid, Guid> billdeskResponse, PrintProgramme printProgramme, bool isPaymentSuccessful)
        {
            ResponseData response = new ResponseData();
            PaymentDetails paymentDetails = billdeskResponse.Item3;
            int NoOfRecords = 0;

            using (TransactionScope transaction = new TransactionScope())
            {
                if (isPaymentSuccessful)
                {
                    response.IsSuccess = true;
                    response.SuccessMessage = billdeskResponse.Item2;
                    // if ((new ARGStudentExamForm().UpdateReEvaluationFormStatus(paymentDetails.EntityID, FormStatus.PaymentSuccessful)) > 0)
                    {
                        paymentDetails.ModuleType = PaymentModuleType.ReEvaluation;
                        paymentDetails.PrintProgramme = new RegistrationManager().MappingTable(printProgramme);
                        NoOfRecords = new PaymentDB().SavePayment(paymentDetails, printProgramme, PaymentModuleType.Examination);
                        if (NoOfRecords > 0)
                        {
                            SendEmailandSMStoStudentAsync(paymentDetails);
                        }
                    }
                }
                else
                {
                    new ReEvaluationDB().DeleteReEvaluationForm(billdeskResponse.Item5, billdeskResponse.Item4);
                    response.IsSuccess = false;
                    response.ErrorMessage = billdeskResponse.Item2;
                }
                transaction.Complete();
            }

            return response;
        }


        #endregion

        #region  ReEvaluation
        public ResponseData SavePayment(Tuple<bool, string, PaymentDetails, Guid, Guid> billdeskResponse, PrintProgramme printProgramme, bool paymentSuccessful, PaymentModuleType moduleType)
        {
            PaymentDetails paymentDetails = billdeskResponse.Item3;
            ResponseData response = new ResponseData();
            int NoOfRecords = 0;
            if (paymentSuccessful)
            {
                response.IsSuccess = true;
                response.SuccessMessage = billdeskResponse.Item2;

                switch (moduleType)
                {
                    case PaymentModuleType.ReEvaluation:
                        if (new ReEvaluationDB().UpdateReEvaluationFormStatus(paymentDetails.AdditionalInfo, FormStatus.PaymentSuccessful) > 0)
                            NoOfRecords = new PaymentDB().SavePayment(paymentDetails, printProgramme, moduleType);
                        break;
                    case PaymentModuleType.Xerox:
                        if (new ReEvaluationDB().UpdateReEvaluationFormStatus(paymentDetails.AdditionalInfo, FormStatus.PaymentSuccessful) > 0)
                            NoOfRecords = new PaymentDB().SavePayment(paymentDetails, printProgramme, moduleType);
                        break;
                    case PaymentModuleType.Examination:
                        NoOfRecords = new PaymentDB().SavePayment(paymentDetails, printProgramme, moduleType);
                        if (NoOfRecords > 0)
                            new ExaminationFormManager().UpdateExaminationFormManually(paymentDetails.TxnAmount, paymentDetails.Entity_ID, printProgramme, paymentDetails.AdditionalInfo, true);
                        break;
                    case PaymentModuleType.SemesterAdmission:
                        Guid Course_ID = paymentDetails.Entity_ID;
                        paymentDetails.Entity_ID = paymentDetails.Student_ID;
                        NoOfRecords = new PaymentDB().SavePayment(paymentDetails, printProgramme, moduleType);
                        if (NoOfRecords > 0 && paymentDetails.Semester == 1)
                        {
                            int res = new StudentManager().SaveSemesterAdmission(paymentDetails, printProgramme, moduleType, Course_ID, paymentDetails.AdditionalInfo);
                            if (res == -3)
                            {
                                response.IsSuccess = false;
                                response.ErrorMessage = "It seems that you have already applied for UG. Please visit University I.T Cell for cancellation";
                                new PaymentDB().Delete(paymentDetails.Payment_ID, printProgramme);
                                return response;
                            }
                            else if (res == 0)
                            {
                                new PaymentDB().Delete(paymentDetails.Payment_ID, printProgramme);
                                NoOfRecords = 0;
                                response.IsSuccess = false;
                                response.ErrorMessage = "It seems that you are not selected in UG";
                                return response;
                            }
                        }
                        break;
                }

                if (NoOfRecords > 0)
                {
                    paymentDetails.ModuleType = moduleType;
                    paymentDetails.PrintProgramme = new RegistrationManager().MappingTable(printProgramme);
                    if (NoOfRecords > 0)
                    {
                        paymentDetails.AdditionalInfo = new ExaminationFormManager().GetStudentNameOnly(paymentDetails.PrintProgramme, paymentDetails.Entity_ID);
                        SendEmailandSMStoStudentAsync(paymentDetails);
                    }
                }
            }
            else
            {
                switch (moduleType)
                {
                    case PaymentModuleType.None:
                        break;
                    case PaymentModuleType.Admission:
                        break;
                    case PaymentModuleType.ReEvaluation:
                        new ReEvaluationDB().DeleteReEvaluationForm(billdeskResponse.Item5, billdeskResponse.Item4);
                        break;
                    case PaymentModuleType.Examination:
                        new ExaminationFormDB().UpdateExaminationFormStatus(paymentDetails?.TxnAmount ?? 0, 0, billdeskResponse.Item4, printProgramme, paymentSuccessful);
                        break;
                    case PaymentModuleType.Xerox:
                        break;
                    case PaymentModuleType.SelfFinanced:
                        break;
                    case PaymentModuleType.SemesterAdmission:
                        break;
                    case PaymentModuleType.CancelRegistration:
                        break;
                    case PaymentModuleType.Migration:
                        break;
                    default:
                        break;
                }
                response.IsSuccess = false;
                response.ErrorMessage = billdeskResponse.Item2;
            }

            response.ResponseObject = paymentDetails;
            return response;
        }
        private void SendEmailandSMStoStudentAsync(PaymentDetails paymentDetails)
        {
#if (!DEBUG)
                new TaskFactory().StartNew(() => SendEmailandSMStoStudent(paymentDetails));
#endif
        }

        private void SendEmailandSMStoStudent(PaymentDetails paymentDetails)
        {
            string smsMessage = "We have received your payment of Rs. " + paymentDetails.TxnAmount + $" against name "
                                   + paymentDetails.AdditionalInfo + " on " + paymentDetails.TxnDate.ToShortDateString() + " and your transaction reference number is " + paymentDetails.TxnReferenceNo;

            new EmailSystem().AppliedSuccessfullyMail(paymentDetails, paymentDetails.Email, paymentDetails.PhoneNumber);
            new SMSHttpPostClient().SendSMS(new SMSModel() { Message = smsMessage, MobileNos = paymentDetails.PhoneNumber }, SMSServiceType.singlemsg);

        }


        public PaymentDetails GetPaymentDetail(Guid Entity_ID, PaymentModuleType _ModuleType, PrintProgramme printProgramme)
        {
            printProgramme = new RegistrationManager().MappingTable(printProgramme);
            return new PaymentDB().GetPaymentDetail(Entity_ID, _ModuleType, printProgramme);
        }
        public PaymentDetails GetPaymentDetail(Guid Entity_ID, short Semester, PaymentModuleType _ModuleType, PrintProgramme printProgramme)
        {
            return new PaymentDB().GetPaymentDetail(Entity_ID, Semester, _ModuleType, printProgramme);
        }
        public List<PaymentDetails> GetPaymentDetails(Guid Entity_ID, PaymentModuleType _ModuleType, PrintProgramme printProgramme)
        {
            printProgramme = new RegistrationManager().MappingTable(printProgramme);
            return new PaymentDB().GetPaymentDetails(Entity_ID, _ModuleType, printProgramme);
        }

        public List<PaymentDetails> GetPaymentDetails(Guid Entity_ID, PaymentModuleType _ModuleType, PrintProgramme printProgramme, short Semester)
        {
            printProgramme = new RegistrationManager().MappingTable(printProgramme);
            return new PaymentDB().GetPaymentDetails(Entity_ID, _ModuleType, printProgramme, Semester);
        }

        public List<PaymentDetails> GetPaymentDetails(Guid student_ID, PrintProgramme printProgramme)
        {
            printProgramme = new RegistrationManager().MappingTable(printProgramme);
            return new PaymentDB().GetPaymentDetails(student_ID, printProgramme);
        }


        #endregion

        #region Registration Payment Details

        public int SaveRegistrationPaymentDetails(PaymentDetails aRGPaymentDetails)
        {
            int result = 0;
            var RegistrationSQL = new RegistrationDB();
            aRGPaymentDetails.PrintProgramme = new RegistrationManager().MappingTable(aRGPaymentDetails.PrintProgramme);
            aRGPaymentDetails.ModuleType = PaymentModuleType.Admission;
            try
            {
                using (var ts = new TransactionScope())
                {
                    result = new PaymentDB().SavePaymentDetails(aRGPaymentDetails, aRGPaymentDetails.PrintProgramme);
                    if (result > 0)
                        RegistrationSQL.UpdateFormStatus(FormStatus.FeePaid, aRGPaymentDetails.Entity_ID, aRGPaymentDetails.PrintProgramme);
                    ts.Complete();
                }
            }
            catch (SqlException)
            {
                System.Threading.Thread.Sleep(millisecondsTimeout: 600);
                using (var ts = new TransactionScope())
                {
                    result = new PaymentDB().SavePaymentDetails(aRGPaymentDetails, aRGPaymentDetails.PrintProgramme);
                    if (result > 0)
                        RegistrationSQL.UpdateFormStatus(FormStatus.FeePaid, aRGPaymentDetails.Entity_ID, aRGPaymentDetails.PrintProgramme);
                    ts.Complete();
                }
            }
            return result;
        }

        public void SendPaymentSMSAndEmailAsyc(PaymentDetails billDeskResponse)
            => new TaskFactory().StartNew(() => SendPaymentSMSAndEmail(billDeskResponse));
        private void SendPaymentSMSAndEmail(PaymentDetails billDeskResponse)
        {
#if (!DEBUG)
            SMSModel sms = new SMSModel { MobileNos = billDeskResponse.PhoneNumber };
            sms.Message = $"Payment of Rs: {billDeskResponse.TxnAmount} Received Successfully for {billDeskResponse.PrintProgramme.ToString()} Admission under{Environment.NewLine}TxnReferenceNo :  {billDeskResponse.TxnReferenceNo}{Environment.NewLine}Dated: {billDeskResponse.TxnDate.ToString("dd-MM-yyy")}";
            new SMSHttpPostClient().SendSMS(sms, SMSServiceType.singlemsg);
            new EmailSystem().AdmissionOnlinePaymentMail(billDeskResponse);
#endif
        }
        #endregion

        #region SelfFinanced
        public int SaveSelfFinancedPaymentDetails(PaymentDetails aRGPaymentDetails, bool savePayment)
        {
            int result = 0;
            using (var ts = new TransactionScope())
            {
                var RegistrationSQL = new RegistrationDB();
                aRGPaymentDetails.PrintProgramme = new RegistrationManager().MappingTable(aRGPaymentDetails.PrintProgramme);
                aRGPaymentDetails.ModuleType = PaymentModuleType.SelfFinanced;
                result = savePayment ? new PaymentDB().SavePaymentDetails(aRGPaymentDetails, aRGPaymentDetails.PrintProgramme) : 1;
                if (result > 0)
                {
                    new SelfFinancedDB().UpdateAppliedForSelfFinancedSeat(aRGPaymentDetails.Payment_ID, AppUserHelper.User_ID, new SelfFinancedDB().GetCourse_IDs(aRGPaymentDetails.AdditionalInfo.ToLists<string>('@')), aRGPaymentDetails.PrintProgramme);
                }
                ts.Complete();
            }
            return result;
        }
        #endregion

        #region NextSemesterAdmission

        public PaymentDetails GetPaymentDetail(Guid Entity_ID, short Semester, PrintProgramme? printProgramme = null)
        {
            if (printProgramme == null)
                printProgramme = AppUserHelper.TableSuffix;

            return new PaymentDB().GetPaymentDetail(Entity_ID, Semester, (PrintProgramme)printProgramme);
        }

        public PaymentDetails GetPaymentDetail(Guid Entity_ID, short Semester, PrintProgramme printProgramme)
        {
            return new PaymentDB().GetPaymentDetail(Entity_ID, Semester, printProgramme);
        }

        public int SaveSemesterAdmissionPaymentDetails(PaymentDetails aRGPaymentDetails)
        {
            int result = 0;
            using (var ts = new TransactionScope())
            {
                var RegistrationSQL = new RegistrationDB();
                aRGPaymentDetails.PrintProgramme = new RegistrationManager().MappingTable(aRGPaymentDetails.PrintProgramme);
                aRGPaymentDetails.ModuleType = PaymentModuleType.SemesterAdmission;
                result = new PaymentDB().SavePaymentDetails(aRGPaymentDetails, aRGPaymentDetails.PrintProgramme);
                if (result > 0)
                {
                    //new StudentManager().UpdateCurrentSemOrYear((short)aRGPaymentDetails.Semester);
#if !DEBUG
                    SendPaymentSMSAndEmailAsyc(aRGPaymentDetails);
#endif
                }
                ts.Complete();
            }
            return result;
        }
        #endregion
        #region Migration  Payment Details

        public int SaveMigrationPaymentDetails(PaymentDetails aRGPaymentDetails)
        {
            string[] sformtypeAI = aRGPaymentDetails.AdditionalInfo.Split('@');
            Enum.TryParse(sformtypeAI[0], out MigrationE formType);
            Guid Form_Id = Guid.Parse(sformtypeAI[2]);
            int result = 0;
            using (var ts = new TransactionScope())
            {
                var RegistrationSQL = new RegistrationDB();
                aRGPaymentDetails.PrintProgramme = new RegistrationManager().MappingTable(aRGPaymentDetails.PrintProgramme);
                aRGPaymentDetails.ModuleType = PaymentModuleType.Migration;
                result = new PaymentDB().SavePaymentDetails(aRGPaymentDetails, aRGPaymentDetails.PrintProgramme);
                if (result > 0)
                    new MigrationManager().UpdateDownloadForm(Form_Id);
                ts.Complete();
            }
            return result;
        }
        #endregion

        #region AttempCertificate Payment Details

        public int UpdateAttempCertificatePaymentDetails(PaymentDetails aRGPaymentDetails)
        {
            string[] sformtypeAI = aRGPaymentDetails.AdditionalInfo.Split('@');
            Guid Certificate_ID = Guid.Parse(sformtypeAI[0]);
            int result = 0;

            var RegistrationSQL = new RegistrationDB();
            aRGPaymentDetails.PrintProgramme = new RegistrationManager().MappingTable(aRGPaymentDetails.PrintProgramme);
            aRGPaymentDetails.ModuleType = PaymentModuleType.AttemptCertificate;

            result =
                new CertificateManager().UpdateCertificateStatus(Certificate_ID, aRGPaymentDetails) ? 1 : 0;

            return result;
        }

        public bool PaymentExistsUG(Guid user_ID, int semester)
        {
            return new PaymentDB().GetPaymentExistsUG(user_ID, semester);
        }


        #endregion

        #region AdmMoreCourses
        public void UpdateAlreadyAppliedCoursePref(List<ARGCoursesApplied> alreadyCourses)
        {
            AdminRegistrationDB adminRegistrationDB = new AdminRegistrationDB();
            foreach (ARGCoursesApplied item in alreadyCourses ?? new List<ARGCoursesApplied>())
            {
                if (alreadyCourses.Count(x => x.GroupName.ToLower().Trim() == item.GroupName.ToLower().Trim()) <= 1)
                {
                    item.Preference = 1;
                }
                adminRegistrationDB.UpdateAlreadyAppliedCoursePref(item);
            }
        }

        public int SaveAdmMoreCoursesPaymentDetails(PaymentDetails aRGPaymentDetails, bool savePayment = true)
        {
            int result = 0;
            using (var ts = new TransactionScope())
            {
                Guid Student_ID = aRGPaymentDetails.Entity_ID;
                PrintProgramme printProgramme = aRGPaymentDetails.PrintProgramme;

                List<ARGCoursesApplied> aRGCoursesApplieds = null;

                var RegistrationSQL = new RegistrationDB();
                aRGPaymentDetails.ModuleType = PaymentModuleType.Admission;
                AdminRegistrationDB adminRegistrationDB = new AdminRegistrationDB();

                bool hasPayments = new PaymentDB().GetPaymentDetail(Student_ID, PaymentModuleType.Admission, printProgramme) != null;

                if (savePayment)
                    result = new PaymentDB().SavePaymentDetails(aRGPaymentDetails, printProgramme);
                else
                    result = 1;

                if (result > 0)
                {
                    if (savePayment)
                        new PaymentDB().UpdatePersonalInfoAddMoreCourses(Student_ID, aRGPaymentDetails.TxnAmount, printProgramme, hasPayments);

                    if (!hasPayments)
                    {
                        adminRegistrationDB.DeleteCourse(Student_ID, printProgramme);
                    }
                    else
                    {
                        aRGCoursesApplieds = adminRegistrationDB.GetStudentCoursesAppliedIDs(Student_ID, printProgramme);
                    }

                    List<string> courseCodes = aRGPaymentDetails.AdditionalInfo.ToLists<string>('@') ?? new List<string>();
                    List<string> courseCodesforGuid = new List<string>();
                    List<ARGCoursesApplied> _CoursesApplied;

                    if (courseCodes.Any(x => x.Contains("_")))
                    {
                        courseCodes.ForEach(x =>
                        {
                            courseCodesforGuid.Add(x.Substring(0, x.IndexOf("_")));
                        });
                        _CoursesApplied = new SelfFinancedDB().GetCoursePrefIDs(courseCodesforGuid);
                    }
                    else
                    {
                        _CoursesApplied = new SelfFinancedDB().GetCoursePrefIDs(courseCodes);
                    }

                    short pref = 0;

                    foreach (var item in _CoursesApplied)
                    {
                        if (!(aRGCoursesApplieds?.Exists(x => x.Course_ID == item.Course_ID) ?? false))
                        {
                            if (courseCodes.Any(x => x.Contains("_")))
                            {
                                pref = Convert.ToInt16(courseCodes.FirstOrDefault(x => x.ToLower().Trim().Contains(item.CourseCode.ToLower().Trim())).Split('_')[1]);

                                adminRegistrationDB.Save(new ARGCoursesApplied
                                {
                                    Course_ID = item.Course_ID,
                                    Student_ID = Student_ID,
                                    Preference = pref,
                                    SubjectEntrancePoints = 0,
                                    AppearedInEntrance = false,
                                }, printProgramme);
                            }
                            else
                            {
                                adminRegistrationDB.Save(new ARGCoursesApplied
                                {
                                    Course_ID = item.Course_ID,
                                    Student_ID = Student_ID,
                                    Preference = 0,
                                    SubjectEntrancePoints = 0,
                                    AppearedInEntrance = false,
                                }, printProgramme);
                            }
                        }
                    }
                }
                aRGCoursesApplieds = null;
                ts.Complete();
            }
            return result;
        }
        #endregion

        #region CertificateCourses
        public int SavePaymentForCertificateCourses(PaymentDetails aRGPaymentDetails)
        {
            int result = 0;
            using (var ts = new TransactionScope())
            {
                aRGPaymentDetails.ModuleType = PaymentModuleType.CertificateCoursesAdmission;
                result = new PaymentDB().SavePaymentForCertificateCourses(aRGPaymentDetails);
                if (result > 0)
                {
                    new CertificateCoursesManager().UpdateFormStatus(aRGPaymentDetails.Entity_ID);
                }
                ts.Complete();
            }
            if (result > 0)
            {
                string msg = $"Payment received successfully.  Under Name: {aRGPaymentDetails.AdditionalInfo} TxnRefNo: {aRGPaymentDetails.TxnReferenceNo} Amount: {aRGPaymentDetails.TxnAmount} dated: {aRGPaymentDetails.TxnDate}";
                new CertificateCoursesManager().SendSMSAsync(msg, aRGPaymentDetails.PhoneNumber);
            }
            return result;
        }
        #endregion
    }
}
