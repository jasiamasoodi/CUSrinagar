using CUSrinagar.Models;
using System;
using System.Collections.Generic;
using CUSrinagar.DataManagers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.IO;
using System.Web;
using GeneralModels;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using System.Transactions;
using System.Text.RegularExpressions;

namespace CUSrinagar.BusinessManagers
{
    public class CertificateCoursesManager
    {
        public bool ValidateBoardRegnNoExists(string BoardRegnNo, short Batch)
        {
            if (string.IsNullOrWhiteSpace(BoardRegnNo) || !new Regex(@"^[^\s]+$").IsMatch(BoardRegnNo + ""))
                return true;

            string FirstChar = BoardRegnNo?.Substring(0, 1) + string.Empty;
            string LastChar = BoardRegnNo.Substring(BoardRegnNo.Length - 1, 1) + string.Empty;

            if (!new Regex("^[a-zA-Z0-9]*$").IsMatch(FirstChar) || !new Regex("^[a-zA-Z0-9]*$").IsMatch(LastChar))
            {
                return true;
            }

            return new CertificateCoursesDB().ValidateBoardRegnNoExists(BoardRegnNo, Batch);
        }

        public Tuple<bool, string, Guid> Save(CertificateCoursePersonalInfo certificateCoursePersonalInfo)
        {
            if (certificateCoursePersonalInfo == null || certificateCoursePersonalInfo.PrevQualifications.IsNullOrEmpty() || certificateCoursePersonalInfo.TotalFee <= 0)
                return Tuple.Create(false, "Invalid Data submitted. Please check your details and try again.", Guid.Empty);
            if (ValidateBoardRegnNoExists(certificateCoursePersonalInfo.BoardRegnNo, certificateCoursePersonalInfo.Batch))
                return Tuple.Create(false, $"Board Regn No already exists for batch {certificateCoursePersonalInfo.Batch}. Please check your details and try again.", Guid.Empty);

            certificateCoursePersonalInfo.DOB = new RegistrationManager().ConvertEnterdDOBToDateTime(certificateCoursePersonalInfo.EnteredDOB);
            if (certificateCoursePersonalInfo.DOB == DateTime.MinValue)
                return Tuple.Create(false, $"DOB Entered is not valid. Please check your details and try again.", Guid.Empty);

            int result = 0;
            try
            {
                using (TransactionScope ts = new TransactionScope())
                {
                    CertificateCoursesDB certificateCoursesDB = new CertificateCoursesDB();
                    certificateCoursePersonalInfo.Student_ID = Guid.NewGuid();
                    certificateCoursePersonalInfo.CreatedOn = DateTime.UtcNow;
                    certificateCoursePersonalInfo.FormStatus = FormStatus.InProcess;
                    certificateCoursePersonalInfo.Photograph = GeneralFunctions.GetStudentPhotoPathForCertification() + certificateCoursePersonalInfo.Student_ID + Path.GetExtension(certificateCoursePersonalInfo.PhotographPath.FileName);

                    result = certificateCoursesDB.Save(certificateCoursePersonalInfo);

                    if (result > 0)
                    {
                        foreach (CertificateCoursePrevQualifications item in certificateCoursePersonalInfo.PrevQualifications)
                        {
                            item.Student_ID = certificateCoursePersonalInfo.Student_ID;
                            item.PrevQualification_ID = Guid.NewGuid();
                            certificateCoursesDB.Save(item);
                        }
                        Helper.CompressAndUploadImage(certificateCoursePersonalInfo.PhotographPath.InputStream, 259, 194, certificateCoursePersonalInfo.Photograph);
                    }
                    ts.Complete();
                }
            }
            catch (SqlException SQLError) when (SQLError.Number == 1205)
            {
                result = 0;
            }
            return Tuple.Create(result > 0, "Unknown error occurred", certificateCoursePersonalInfo.Student_ID);
        }

        public CertificateCoursePersonalInfo GetItem(Guid Student_ID, bool IsCompact = true)
        {
            CertificateCoursePersonalInfo certificateCoursePersonalInfo = new CertificateCoursesDB().GetItem(Student_ID);
            if (certificateCoursePersonalInfo != null && !IsCompact)
            {
                certificateCoursePersonalInfo.PrevQualifications = new CertificateCoursesDB().GetCertificateCoursePrevQualifications(Student_ID);
                certificateCoursePersonalInfo.PaymentDetail = new PaymentManager().GetPaymentDetail(Student_ID, PaymentModuleType.CertificateCoursesAdmission, PrintProgramme.IH);
            }
            return certificateCoursePersonalInfo;
        }

        public CertificateCoursePersonalInfo GetItem(string BoardRegnNo, short Batch, bool IsCompact = true)
        {
            CertificateCoursePersonalInfo certificateCoursePersonalInfo = new CertificateCoursesDB().GetItem(BoardRegnNo + "", Batch);
            if (certificateCoursePersonalInfo != null && !IsCompact)
            {
                certificateCoursePersonalInfo.PrevQualifications = new CertificateCoursesDB().GetCertificateCoursePrevQualifications(certificateCoursePersonalInfo.Student_ID);
                certificateCoursePersonalInfo.PaymentDetail = new PaymentManager().GetPaymentDetail(certificateCoursePersonalInfo.Student_ID, PaymentModuleType.CertificateCoursesAdmission, PrintProgramme.IH);
            }
            return certificateCoursePersonalInfo;
        }
        public int UpdateFormStatus(Guid Student_ID)
        {
            return new CertificateCoursesDB().UpdateFormStatus(Student_ID);
        }


        public void SendSMSAsync(string Msg, string MobileNo)
        {
            new TaskFactory().StartNew(() => SendSMS(Msg, MobileNo));
        }
        public void SendSMS(string Msg, string MobileNo)
        {
#if (!DEBUG)
            SMSModel sms = new SMSModel { MobileNos = MobileNo };
            sms.Message = Msg;
            new SMSHttpPostClient().SendSMS(sms, SMSServiceType.singlemsg);
#endif
        }
    }
}
