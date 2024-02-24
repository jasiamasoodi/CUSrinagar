using CUSrinagar.DataManagers;
using CUSrinagar.Enums;
using CUSrinagar.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Mvc;

namespace CUSrinagar.BusinessManagers
{
    public class ApplicationFormsManager
    {
        #region Departmental Certificate Verfication

        public CertificateVerification GetByStudentID(Guid student_ID, PrintProgramme printProgramme)
        {
            if (student_ID == Guid.Empty)
                return null;
            CertificateVerification certificateVerification = new ApplicationFormsDB().GetByStudentID(student_ID);

            if (certificateVerification != null)
            {
                //allow again after 4 months
                if (certificateVerification.AppliedOn.Date.AddMonths(4) <= DateTime.Now.Date)
                {
                    return null;
                }
            }

            if (certificateVerification?.FeePaid ?? false)
            {
                certificateVerification.PaymentDetail = new PaymentManager().GetPaymentDetail(student_ID, PaymentModuleType.CertificateVerification, printProgramme);
                PersonalInformationCompact personalInformation = new StudentManager().GetStudentC(student_ID, printProgramme);
                certificateVerification.CollegeFullName = personalInformation.CollegeFullName;
                certificateVerification.CUSRegistrationNo = personalInformation.CUSRegistrationNo;
                certificateVerification.FullName = personalInformation.FullName;
                certificateVerification.FathersName = personalInformation.FathersName;
            }
            return certificateVerification;
        }

        public int Save(CertificateVerification certificateVerification)
        {
            if (certificateVerification == null)
                return 0;

            new ApplicationFormsDB().DeletePreviousCV(certificateVerification.Student_ID);

            return new ApplicationFormsDB().Save(certificateVerification);
        }

        public bool UpdatePayment(PaymentDetails paymentDetails)
        {
            if (paymentDetails == null)
                return false;
            paymentDetails.ModuleType = PaymentModuleType.CertificateVerification;

            if (new PaymentDB().SavePaymentDetails(paymentDetails, paymentDetails.PrintProgramme) > 0)
            {
                return new ApplicationFormsDB().UpdateFeePaidInCV(paymentDetails.Entity_ID) > 0;
            }
            return false;
        }

        #endregion

        #region MyRegion

        public IssuingOfDegreeCertificate GetForIssuingOfDegreeCertificate(Guid student_ID, PrintProgramme printProgramme)
        {
            if (student_ID == Guid.Empty)
                return null;

            printProgramme = new GeneralFunctions().MappingTable(printProgramme);

            IssuingOfDegreeCertificate issuingOfDegreeCertificate = new ApplicationFormsDB().GetForIssuingOfDegreeCertificate(student_ID, printProgramme);
            if (issuingOfDegreeCertificate == null)
                return null;

            issuingOfDegreeCertificate.PrintProgramme = printProgramme;

            if (issuingOfDegreeCertificate.PaymentStatus == FormStatus.FeePaid)
            {
                issuingOfDegreeCertificate.PaymentDetail = new PaymentManager().GetPaymentDetail(student_ID, PaymentModuleType.IssuingOfDegreeCertificate, printProgramme);
                if (issuingOfDegreeCertificate.PaymentDetail == null)
                {
                    issuingOfDegreeCertificate.PaymentStatus = FormStatus.InProcess;
                }
                else
                {
                    issuingOfDegreeCertificate.StudentPreviousQualification =
                        new RegistrationDB().GetStudentAcademicDetails(student_ID, printProgramme)
                        ?.OrderByDescending(x => x.UpdatedOn == null ? x.CreatedOn : x.UpdatedOn)?.FirstOrDefault() ?? new ARGStudentPreviousQualifications();

                    issuingOfDegreeCertificate.ExamRollNumber = new ApplicationFormsDB().GetCUSExamRollNo(student_ID, printProgramme);
                }
            }
            else if (issuingOfDegreeCertificate.PaymentStatus == 0)
            {
                issuingOfDegreeCertificate.PaymentStatus = FormStatus.InProcess;
            }

            if (issuingOfDegreeCertificate.StudentPreviousQualification == null)
                issuingOfDegreeCertificate.StudentPreviousQualification = new ARGStudentPreviousQualifications();

            issuingOfDegreeCertificate.StudentAddress = new RegistrationDB().GetStudentAddress(student_ID, printProgramme);

            return issuingOfDegreeCertificate;
        }

        public List<SelectListItem> GetQualifyingExamNames(Guid course_ID, PrintProgramme printProgramme)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            return new ApplicationFormsDB().GetQualifyingExamNames(course_ID, printProgramme) ?? new List<SelectListItem>();
        }

        public bool UpdateDegreeIssuingFormPayment(PaymentDetails paymentDetails)
        {
            if (paymentDetails == null)
                return false;
            paymentDetails.ModuleType = PaymentModuleType.IssuingOfDegreeCertificate;

            bool paymentExists =
                new PaymentDB().GetPaymentExists(paymentDetails.Entity_ID, paymentDetails.ModuleType, paymentDetails.PrintProgramme, null);

            if (!paymentExists)
            {
                if (new PaymentDB().SavePaymentDetails(paymentDetails, paymentDetails.PrintProgramme) > 0)
                {
                    return new ApplicationFormsDB().UpdateIssuingDegreeForm(paymentDetails.Entity_ID) > 0;
                }
            }
            else
            {
                return paymentExists;
            }
            return false;
        }

        public Tuple<bool, string> Save(IssuingOfDegreeCertificate issuingOfDegreeCertificate)
        {
            if (issuingOfDegreeCertificate == null)
                return Tuple.Create(false, "Invalid details");

            if (issuingOfDegreeCertificate.StudentPreviousQualification == null)
                return Tuple.Create(false, "Qualifying Examination is required");

            if (string.IsNullOrWhiteSpace(issuingOfDegreeCertificate.EmploymentStatus))
                return Tuple.Create(false, "Invalid details");

            if (issuingOfDegreeCertificate.Student_ID == Guid.Empty)
                return Tuple.Create(false, "Invalid details");

            if (issuingOfDegreeCertificate.StudentAddress == null || issuingOfDegreeCertificate.StudentAddress?.Address_ID == Guid.Empty)
                return Tuple.Create(false, "Invalid details");

            if (issuingOfDegreeCertificate.PrintProgramme == 0)
                return Tuple.Create(false, "Invalid details");

            issuingOfDegreeCertificate.PrintProgramme = new GeneralFunctions().MappingTable(issuingOfDegreeCertificate.PrintProgramme);

            //Remove possibility of duplicates
            Guid Qualification_ID = GetStudentPreviousQualification(issuingOfDegreeCertificate.Student_ID,
                                    issuingOfDegreeCertificate.StudentPreviousQualification.ExamName,
                                    issuingOfDegreeCertificate.PrintProgramme)?.Qualification_ID ?? Guid.Empty;

            if (Qualification_ID != Guid.Empty)
            {
                issuingOfDegreeCertificate.StudentPreviousQualification.Qualification_ID = Qualification_ID;
                issuingOfDegreeCertificate.StudentPreviousQualification.Student_ID = issuingOfDegreeCertificate.Student_ID;
            }

            //save IssuingOfDegreeCertificate
            issuingOfDegreeCertificate.AppliedOn = DateTime.Now;
            issuingOfDegreeCertificate.PaymentStatus = FormStatus.InProcess;
            issuingOfDegreeCertificate.Specializations = issuingOfDegreeCertificate.Specializations?.ToUpper();
            issuingOfDegreeCertificate.PerusingHigherEduInAndProgramme = issuingOfDegreeCertificate.PerusingHigherEduInAndProgramme?.ToUpper();


            int result = new ApplicationFormsDB().Save(issuingOfDegreeCertificate);
            if (result <= 0)
                return Tuple.Create(false, "Could not save");

            //update or save qualification & update personal info isprovision adm
            issuingOfDegreeCertificate.StudentPreviousQualification.Student_ID = issuingOfDegreeCertificate.Student_ID;
            issuingOfDegreeCertificate.StudentPreviousQualification.Subjects = issuingOfDegreeCertificate.StudentPreviousQualification.Subjects.ToUpper();
            issuingOfDegreeCertificate.StudentPreviousQualification.Stream = issuingOfDegreeCertificate.StudentPreviousQualification.Stream.ToUpper();

            Tuple<bool, string> response = new RegnCoursesManager().AddEditQualificationDetails(issuingOfDegreeCertificate.StudentPreviousQualification, issuingOfDegreeCertificate.PrintProgramme);

            //update Mobile and Email
            if (!response.Item1)
                return response;

            result = new ApplicationFormsDB().UpdateContactDetails(issuingOfDegreeCertificate.StudentAddress.Address_ID, issuingOfDegreeCertificate.PrintProgramme,
                issuingOfDegreeCertificate.StudentAddress.Mobile, issuingOfDegreeCertificate.StudentAddress.Email, issuingOfDegreeCertificate.Student_ID);

            return Tuple.Create(result > 0, result > 0 ? "Saved" : "");
        }

        public ARGStudentPreviousQualifications GetStudentPreviousQualification(Guid Student_ID, string examName, PrintProgramme printProgramme)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);

            if (Student_ID == Guid.Empty || string.IsNullOrWhiteSpace(examName))
                return new ARGStudentPreviousQualifications();

            return new ApplicationFormsDB().GetStudentPreviousQualification(Student_ID, examName, printProgramme);
        }

        public bool DegreeAlreadyHandedOverOn(Guid Student_ID)
        {
            return new ApplicationFormsDB().DegreeAlreadyHandedOverOn(Student_ID);
        }
        public bool DegreeAlreadyPrinted(Guid Student_ID)
        {
            return new ApplicationFormsDB().DegreeAlreadyPrinted(Student_ID);
        }

        public CorrectionForm GetCorrectionForm(Guid Student_ID, PrintProgramme printProgramme)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            CorrectionForm correctionForm = new ApplicationFormsDB().GetCorrectionForm(Student_ID, printProgramme);
            if ((correctionForm?.Student_ID ?? Guid.Empty) != Guid.Empty)
            {
                if (correctionForm.PaymentStatus == FormStatus.FeePaid)
                {
                    correctionForm.PaymentDetail = new PaymentDB().GetPaymentDetail(Student_ID, PaymentModuleType.CorrectionForm, printProgramme);
                }
            }
            return correctionForm;
        }

        public Tuple<bool, string> SaveCorrectionForm(CorrectionForm correctionForm)
        {
            int result = new ApplicationFormsDB().SaveCorrectionForm(correctionForm);
            return Tuple.Create(result > 0, "");
        }

        public int CalculateCorrectionFormFee(CorrectionForm correctionForm)
        {
            if (!correctionForm.HasDegreeCertificatePrinted && correctionForm.HasTranscriptPrinted)
                return 600;

            if (correctionForm.HasDegreeCertificatePrinted && correctionForm.HasTranscriptPrinted)
                return 1600;

            return 300;
        }

        public bool UpdateCorrectionFormPayment(PaymentDetails paymentDetails)
        {
            if (paymentDetails == null)
                return false;

            paymentDetails.ModuleType = PaymentModuleType.CorrectionForm;

            bool paymentExists =
                new PaymentDB().GetPaymentExists(paymentDetails.Entity_ID, paymentDetails.ModuleType, paymentDetails.PrintProgramme, null);

            if (!paymentExists)
            {
                if (new PaymentDB().SavePaymentDetails(paymentDetails, paymentDetails.PrintProgramme) > 0)
                {
                    return new ApplicationFormsDB().UpdateCorrectionForm(paymentDetails.Entity_ID) > 0;
                }
            }
            else
            {
                return paymentExists;
            }
            return false;
        }
        #endregion
    }
}
