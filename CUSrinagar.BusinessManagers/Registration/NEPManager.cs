using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CUSrinagar.Extensions;
using CUSrinagar.Enums;
using CUSrinagar.Models;
using System.Data.SqlClient;
using CUSrinagar.DataManagers;
using System.Web.Mvc;
using Terex;
using System.Transactions;
using System.Web.UI.WebControls;
using System.IO;
using System.Web;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace CUSrinagar.BusinessManagers
{
    public class NEPManager
    {
        public Tuple<bool, string> Save(ARGPersonalInformation aRGPersonalInformation, PrintProgramme printProgramme)
        {
            if (new CategoryManager().GetCategoryList().FindIndex(x => x.Value.Trim().ToLower() == aRGPersonalInformation.Category.Trim().ToLower()) == -1)
                return Tuple.Create(false, "Invalid Category");

            RegistrationManager registrationManager = new RegistrationManager();
            RegistrationDB registrationDB = new RegistrationDB();
            NEPDB _NEPDB = new NEPDB();

            printProgramme = registrationManager.MappingTable(printProgramme);

            bool studentExits = aRGPersonalInformation.Student_ID == Guid.Empty ? false : _NEPDB.FormExists(aRGPersonalInformation.Student_ID, printProgramme);

            if (aRGPersonalInformation.Student_ID != Guid.Empty)
            {
                if (!studentExits)
                {
                    return Tuple.Create(false, "Form does not exist");
                }
            }

            PersonalInformationCompact personalInformationCompact = studentExits ? new StudentManager().GetStudentC(aRGPersonalInformation.Student_ID, printProgramme) : new PersonalInformationCompact
            {
                FormStatus = FormStatus.InProcess,
            };

            //Fill address

            if (!studentExits)
            {
                aRGPersonalInformation.Student_ID = Guid.NewGuid();
            }

            string[] disttAndState = aRGPersonalInformation.StudentAddress.District.Split('|');
            aRGPersonalInformation.StudentAddress.District = disttAndState[0];
            aRGPersonalInformation.StudentAddress.State = disttAndState[1];

            string[] assembly = aRGPersonalInformation.StudentAddress.AssemblyConstituency.Split('|');
            aRGPersonalInformation.StudentAddress.AssemblyConstituency = assembly[0];
            aRGPersonalInformation.StudentAddress.ParliamentaryConstituency = assembly[1];

            int result = 0;

            try
            {
                ARGFormNoMaster aRGFormNoMaster = registrationDB.GetFromNoMaster(printProgramme);

                aRGPersonalInformation.DOB = registrationManager.ConvertEnterdDOBToDateTime(aRGPersonalInformation.EnteredDOB);

                aRGPersonalInformation.BoardRegistrationNo = studentExits
                                                                ? personalInformationCompact.BoardRegistrationNo.ToUpper().Trim()
                                                                : aRGPersonalInformation.BoardRegistrationNo.ToUpper().Trim();

                aRGPersonalInformation.FormStatus = personalInformationCompact.FormStatus;

                aRGPersonalInformation.TotalFee = aRGFormNoMaster.BasicFee;
                aRGPersonalInformation.IsLateralEntry = aRGFormNoMaster.ShowLateralEntry;
                aRGPersonalInformation.CurrentSemesterOrYear = 0;
                aRGPersonalInformation.IsProvisional = false;
                aRGPersonalInformation.UploadedOnNadDigilocker = false;
                aRGPersonalInformation.IsProvisional = true;

                aRGPersonalInformation.StudentFormNo = studentExits ? personalInformationCompact.StudentFormNo : ($"{aRGFormNoMaster.FormPrefix}{aRGFormNoMaster.FormNoCount}").ToUpper().Trim();
                aRGPersonalInformation.Batch = aRGFormNoMaster.BatchToSet;

                //CUET ENTRANCE ONLY
                if (printProgramme == PrintProgramme.PG)
                {
                    aRGPersonalInformation.CATEntrancePoints = 0;
                }


                if (printProgramme == PrintProgramme.UG && aRGPersonalInformation.Gender.Trim().ToLower() == "male")
                {
                    if (studentExits)
                    {
                        string oldGender = _NEPDB.GetStudentGender(printProgramme, aRGPersonalInformation.Student_ID);
                        if (oldGender.ToLower().Trim() != aRGPersonalInformation.Gender.ToLower().Trim() && personalInformationCompact.FormStatus == FormStatus.InProcess)
                        {
                            _NEPDB.DeleteCourseApplied(aRGPersonalInformation.Student_ID, printProgramme);
                            _NEPDB.DeleteNEPCollegePreferences(aRGPersonalInformation.Student_ID);
                        }
                        if (_NEPDB.CheckForWomensCollege(aRGPersonalInformation.Student_ID))
                        {
                            return Tuple.Create(false, $"Please change <br/> Step-1 : Major Subject Preferences and<br/>Step-2: College Preferences,<br/>before making changes to Gender");
                        }
                    }
                }

                //--------------- if category changed del old category  -------------------
                if (aRGPersonalInformation.Category.ToLower().Trim() == "om")
                {
                    List<Certificate> certificates = new StudentDB().GetUploadedCertificates(aRGPersonalInformation.Student_ID);
                    if (certificates.IsNotNullOrEmpty())
                    {
                        string oldPath = certificates.FirstOrDefault(x => x.CertificateType == CertificateType.Category)?.CertificateUrl ?? "/nothing.ss";
                        if (File.Exists(HostingEnvironment.MapPath("~" + oldPath)))
                        {
                            File.Delete(HostingEnvironment.MapPath("~" + oldPath));
                        }
                        _NEPDB.DeleteCertificate(aRGPersonalInformation.Student_ID, CertificateType.Category);
                    }
                }

                using (var ts = new TransactionScope())
                {
                    if (!studentExits)
                    {
                        #region  Save address and presonal info
                        if (registrationManager.CheckBoardRegNoExists(aRGPersonalInformation.BoardRegistrationNo, printProgramme) && !studentExits)
                        {
                            ts.Complete();
                            return Tuple.Create(false, $"{aRGPersonalInformation.BoardRegistrationNo} already submitted form. Indeed you can edit the form.");
                        }

                        aRGPersonalInformation.CreatedOn = DateTime.UtcNow;
                        result = registrationDB.Save(aRGPersonalInformation, printProgramme);

                        if (result > 0)
                        {
                            aRGPersonalInformation.StudentAddress.Student_ID = aRGPersonalInformation.Student_ID;
                            aRGPersonalInformation.StudentAddress.CreatedOn = aRGPersonalInformation.CreatedOn;
                            aRGPersonalInformation.StudentAddress.Address_ID = Guid.NewGuid();

                            registrationDB.Save(aRGPersonalInformation.StudentAddress, printProgramme);
                        }
                        else
                        {
                            throw new TransactionAbortedException();
                        }
                        registrationDB.UpdateFromNoMaster(printProgramme);

                        //send email
                        registrationManager.SendEmailAndSMSAsync(aRGPersonalInformation, printProgramme);
                        #endregion
                    }
                    else
                    {
                        #region  Edit address and presonal info

                        aRGPersonalInformation.UpdatedOn = DateTime.Now;
                        aRGPersonalInformation.Student_ID = personalInformationCompact.Student_ID;

                        if (aRGPersonalInformation.CUETApplicationNo == null || aRGPersonalInformation.CUETEntranceRollNo == null)
                        {
                            ARGPersonalInformation personalInformationCUET =
                                registrationDB.GetCUETDetails(personalInformationCompact.Student_ID, printProgramme);
                            if (personalInformationCUET != null)
                            {
                                aRGPersonalInformation.CUETEntranceRollNo = personalInformationCUET.CUETEntranceRollNo;
                                aRGPersonalInformation.CUETApplicationNo = personalInformationCUET.CUETApplicationNo;
                            }
                        }

                        result = _NEPDB.UpdatePersonalInfo(aRGPersonalInformation, printProgramme);

                        if (result > 0)
                        {
                            aRGPersonalInformation.StudentAddress.Student_ID = aRGPersonalInformation.Student_ID;
                            aRGPersonalInformation.StudentAddress.CreatedOn = DateTime.Now;
                            aRGPersonalInformation.StudentAddress.Address_ID = Guid.NewGuid();
                            _NEPDB.DeleteAddress(aRGPersonalInformation.Student_ID, printProgramme);
                            registrationDB.Save(aRGPersonalInformation.StudentAddress, printProgramme);
                        }
                        else
                        {
                            throw new TransactionAbortedException();
                        }

                        #endregion
                    }
                    ts.Complete();
                }

            }
            catch (TransactionAbortedException)
            {
                return Tuple.Create(false, "An error occurred. Please try again");
            }
            catch (SqlException SQLError) when (SQLError.Number == 1205)
            {
                return Tuple.Create(false, "An error occurred. Please try again");
            }

            return Tuple.Create(result > 0, "");
        }

        public bool BlockEditAfterPayment(Guid Student_ID, PrintProgramme printProgrammeOption)
        {
            printProgrammeOption = new RegistrationManager().MappingTable(printProgrammeOption);
            ARGFormNoMaster formNoMaster = new RegistrationDB().GetFromNoMaster(printProgrammeOption);

            if (formNoMaster == null)
                return false;

            if (formNoMaster.AllowEditAfterPayment == false)
                return new NEPDB().BlockEditAfterPayment(Student_ID, printProgrammeOption);
            else
                return false;
        }

        public Tuple<bool, string> Save(ARGStudentPreviousQualifications studentPreviousQualifications, PrintProgramme printProgramme)
        {
            if (studentPreviousQualifications == null)
                return Tuple.Create(false, "Invalid details");

            if (studentPreviousQualifications.Student_ID == Guid.Empty)
                return Tuple.Create(false, "Student ID can not be empty");

            RegistrationManager registrationManager = new RegistrationManager();
            RegistrationDB registrationDB = new RegistrationDB();
            NEPDB _NEPDB = new NEPDB();
            printProgramme = registrationManager.MappingTable(printProgramme);

            bool studentExists = _NEPDB.FormExists(studentPreviousQualifications.Student_ID, printProgramme);
            int result = 0;

            if (studentExists)
            {
                try
                {
                    using (var ts = new TransactionScope())
                    {
                        result = registrationDB.DeleteAcademicDetailsAll(studentPreviousQualifications.Student_ID, printProgramme);

                        studentPreviousQualifications.CreatedOn = DateTime.Now;
                        studentPreviousQualifications.Qualification_ID = Guid.NewGuid();
                        result += registrationDB.Save(studentPreviousQualifications, printProgramme);

                        if (result > 0)
                        {
                            registrationDB.UpdateIsProvisional(studentPreviousQualifications.Student_ID, printProgramme, false);
                        }

                        ts.Complete();
                    }
                }
                catch (SqlException SQLError) when (SQLError.Number == 1205)
                {
                    return Tuple.Create(false, "Please check and try again");
                }
            }
            else
            {
                return Tuple.Create(false, "Student does not exist");
            }
            return Tuple.Create(result > 0, "");
        }

        public Tuple<bool, string> DeletePreviousQualifications(Guid Student_ID, PrintProgramme printProgramme)
        {
            if (Student_ID == Guid.Empty)
                return Tuple.Create(false, "Student ID can not be empty");

            RegistrationManager registrationManager = new RegistrationManager();
            RegistrationDB registrationDB = new RegistrationDB();
            NEPDB _NEPDB = new NEPDB();
            printProgramme = registrationManager.MappingTable(printProgramme);

            int result = 0;
            try
            {
                using (var ts = new TransactionScope())
                {
                    result = registrationDB.DeleteAcademicDetailsAll(Student_ID, printProgramme);
                    if (result > 0)
                    {
                        DeleteCertificate(Student_ID, CertificateType.MarksCard);
                        DeleteCertificate(Student_ID, CertificateType.Provisional);
                        registrationDB.UpdateIsProvisional(Student_ID, printProgramme, true);
                    }
                    ts.Complete();
                }
            }
            catch (SqlException SQLError) when (SQLError.Number == 1205)
            {
                return Tuple.Create(false, "Please check and try again");
            }
            return Tuple.Create(result > 0, "");
        }

        public Tuple<bool, string> Save(List<ARGCoursesApplied> SelectedCourses, PrintProgramme printProgramme, bool allowNonCUET = false)
        {
            if (SelectedCourses.IsNullOrEmpty())
                return Tuple.Create(false, "Invalid details");

            if (SelectedCourses.Any(x => x.Preference == null || x.Preference <= 0))
                return Tuple.Create(false, "Please select preference(s) for all your selected course(s) and try again.");

            if (SelectedCourses.Any(x => Convert.ToInt32(x.Preference) > SelectedCourses.Count()))
                return Tuple.Create(false, "Some of your perferences are invalid as per your selected course(s). Please check carefully and try again.");

            if ((SelectedCourses.GroupBy(x => x.Preference).Where(g => g.Count() > 1)
                                       .Select(x => x.Key)?.Count() ?? 1) > 0)
                return Tuple.Create(false, "Dulicate Course Preferences are not allowed. Please check carefully and try again.");

            if ((SelectedCourses.GroupBy(x => x.Course_ID).Where(g => g.Count() > 1)
                                     .Select(x => x.Key)?.Count() ?? 1) > 0)
                return Tuple.Create(false, "Dulicate Course are not allowed. Please check carefully and try again.");


            Guid Student_ID = SelectedCourses.FirstOrDefault(x => x.Student_ID != Guid.Empty)?.Student_ID ?? Guid.Empty;

            if (Student_ID == Guid.Empty)
                return Tuple.Create(false, "Student ID can not be empty");


            RegistrationManager registrationManager = new RegistrationManager();
            RegistrationDB registrationDB = new RegistrationDB();
            NEPDB _NEPDB = new NEPDB();
            printProgramme = registrationManager.MappingTable(printProgramme);

            bool studentExists = _NEPDB.FormExists(Student_ID, printProgramme);
            int result = 0;

            //check for self-finance Quota
            Guid? SF_Payment_ID = null;
            if (printProgramme == PrintProgramme.IH)
            {
                SF_Payment_ID = registrationDB.GetStudentCoursesApplied(Student_ID, printProgramme)?.FirstOrDefault(x => x.SelfFinancedPayment_ID != null)?.SelfFinancedPayment_ID;
                if (SF_Payment_ID == Guid.Empty)
                    SF_Payment_ID = null;
            }

            if (studentExists)
            {
                try
                {
                    using (var ts = new TransactionScope())
                    {
                        result = _NEPDB.DeleteCourseApplied(Student_ID, printProgramme);

                        SelectedCourses.ForEach(x =>
                        {
                            x.Student_ID = Student_ID;
                            x.AppearedInEntrance = false;
                            x.SelfFinancedPayment_ID = SF_Payment_ID;

                            if (allowNonCUET == false && printProgramme == PrintProgramme.PG)
                            {
                                //CUET ENTRANCE ONLY
                                x.SubjectCGPA = x.CUETPercentileScore;
                                x.SubjectEntrancePoints = Convert.ToDecimal(x.CUETEntrancePoints.Value);
                                x.AppearedInEntrance = true;
                            }
                            else
                            {
                                //Non-CUET ENTRANCE ONLY
                                x.SubjectCGPA = null;
                                x.SubjectEntrancePoints = 0;
                                x.AppearedInEntrance = false;
                            }

                            result += registrationDB.Save(x, printProgramme);
                        });

                        ts.Complete();
                    }
                }
                catch (SqlException SQLError) when (SQLError.Number == 1205)
                {
                    return Tuple.Create(false, "Please check and try again");
                }
            }
            else
            {
                return Tuple.Create(false, "Student does not exist");
            }
            return Tuple.Create(result > 0, "");
        }

        public Tuple<bool, string> Save(List<NEPCollegePreference> CollegePreference, PrintProgramme printProgramme)
        {
            if (CollegePreference.IsNullOrEmpty())
                return Tuple.Create(false, "Invalid details");

            if (CollegePreference.Any(x => x.College_ID == Guid.Empty))
                return Tuple.Create(false, "All Colleges are not selected. Please check carefully and try again.");

            if ((CollegePreference.GroupBy(x => x.College_ID).Where(g => g.Count() > 1)
                                      .Select(x => x.Key)?.Count() ?? 1) > 0)
                return Tuple.Create(false, "Dulicate College are not allowed. Please check carefully and try again.");

            if (CollegePreference.Any(x => x.PreferenceNo <= 0))
                return Tuple.Create(false, "Please select preference(s) for all your selected colleges and try again.");

            if (CollegePreference.Any(x => Convert.ToInt32(x.PreferenceNo) > CollegePreference.Count()))
                return Tuple.Create(false, "Some of your perferences are invalid as per your selected colleges. Please check carefully and try again.");

            if ((CollegePreference.GroupBy(x => x.PreferenceNo).Where(g => g.Count() > 1)
                                    .Select(x => x.Key)?.Count() ?? 1) > 0)
                return Tuple.Create(false, "Dulicate College Preferences are not allowed. Please check carefully and try again.");

            if ((CollegePreference.GroupBy(x => x.College_ID).Where(g => g.Count() > 1)
                                  .Select(x => x.Key)?.Count() ?? 1) > 0)
                return Tuple.Create(false, "Dulicate Colleges are not allowed. Please check carefully and try again.");


            Guid Student_ID = CollegePreference.FirstOrDefault(x => x.Student_ID != Guid.Empty)?.Student_ID ?? Guid.Empty;

            if (Student_ID == Guid.Empty)
                return Tuple.Create(false, "Student ID can not be empty");


            RegistrationManager registrationManager = new RegistrationManager();
            RegistrationDB registrationDB = new RegistrationDB();
            NEPDB _NEPDB = new NEPDB();
            printProgramme = registrationManager.MappingTable(printProgramme);

            bool studentExists = _NEPDB.FormExists(Student_ID, printProgramme);
            int result = 0;

            if (studentExists)
            {
                try
                {
                    using (var ts = new TransactionScope())
                    {
                        result = _NEPDB.DeleteNEPCollegePreferences(Student_ID);

                        CollegePreference.ForEach(x =>
                        {
                            x.Student_ID = Student_ID;
                            result += _NEPDB.Save(x);
                        });

                        ts.Complete();
                    }
                }
                catch (SqlException SQLError) when (SQLError.Number == 1205)
                {
                    return Tuple.Create(false, "Please check and try again");
                }
            }
            else
            {
                return Tuple.Create(false, "Student does not exist");
            }
            return Tuple.Create(result > 0, "");
        }

        public Tuple<bool, string> Save(NEPDocuments documents, PrintProgramme printProgramme)
        {
            if (documents == null)
                return Tuple.Create(false, "Invalid details");

            if (documents.Student_ID == Guid.Empty)
                return Tuple.Create(false, "Student ID can not be empty");

            RegistrationManager registrationManager = new RegistrationManager();
            RegistrationDB registrationDB = new RegistrationDB();
            NEPDB _NEPDB = new NEPDB();
            printProgramme = registrationManager.MappingTable(printProgramme);

            bool studentExists = _NEPDB.FormExists(documents.Student_ID, printProgramme);
            if (!studentExists)
                return Tuple.Create(false, "Student does not exist");

            int result = 0;

            //------------ Potogarph upload ---------------------
            string oldPath = "";
            if (documents.PhotographPath != null)
            {
                oldPath = _NEPDB.GetStudentPhotoPath(documents.Student_ID, printProgramme);
                if (File.Exists(HostingEnvironment.MapPath("~" + oldPath)))
                {
                    File.Delete(HostingEnvironment.MapPath("~" + oldPath));
                }

                string photograph = GeneralFunctions.GetStudentPhotoPath(printProgramme)
                    + Guid.NewGuid() + Path.GetExtension(documents.PhotographPath.FileName);

                UploadAsync(documents.PhotographPath, photograph, true, HttpContext.Current.Server);
                result += _NEPDB.UpdatePhotoPath(documents.Student_ID, photograph, printProgramme);
            }

            //-------------- Upload certificates ---------------------------
            if (documents.Documents?.IsNotNullOrEmpty() ?? false)
            {
                if ((documents.Documents.GroupBy(x => x.File.FileName.ToLower().Trim()).Where(g => g.Count() > 1)
                                        .Select(x => x.Key)?.Count() ?? 1) > 0)
                    return Tuple.Create(false, "Dulicate Certificates are not allowed. Please check carefully and try again.");

                List<Certificate> certificates = new StudentDB().GetUploadedCertificates(documents.Student_ID) ?? new List<Certificate>();
                oldPath = "";

                foreach (Certificate item in documents.Documents)
                {
                    oldPath = certificates.FirstOrDefault(x => x.CertificateType == item.CertificateType)?.CertificateUrl ?? "/nothing.ss";
                    if (File.Exists(HostingEnvironment.MapPath("~" + oldPath)))
                    {
                        File.Delete(HostingEnvironment.MapPath("~" + oldPath));
                    }

                    item.Student_ID = documents.Student_ID;
                    result += Save(item, printProgramme);
                }
            }

            return Tuple.Create(result > 0, "");
        }

        public Tuple<bool, string> Save(PaymentDetails paymentDetails, PrintProgramme printProgramme)
        {
            if (paymentDetails == null)
                return Tuple.Create(false, "Invalid details");

            if (paymentDetails.Entity_ID == Guid.Empty)
                return Tuple.Create(false, "Student ID can not be empty");

            RegistrationManager registrationManager = new RegistrationManager();

            NEPDB _NEPDB = new NEPDB();
            printProgramme = registrationManager.MappingTable(printProgramme);

            bool studentExists = _NEPDB.FormExists(paymentDetails.Entity_ID, printProgramme);
            if (!studentExists)
                return Tuple.Create(false, "Student does not exist");

            paymentDetails.ModuleType = PaymentModuleType.Admission;
            paymentDetails.PaymentType = PaymentType.Online;

            int result = 0;
            try
            {
                using (var ts = new TransactionScope())
                {
                    result = new PaymentManager().SavePaymentDetails(paymentDetails, printProgramme);

                    if (result > 0)
                    {
                        result += _NEPDB.UpdateStudentFeeStatus(paymentDetails.Entity_ID, FormStatus.FeePaid, printProgramme);

                        //check for Self-Finance quota
                        if ((printProgramme == PrintProgramme.IH || printProgramme == PrintProgramme.PG) && (paymentDetails.AdditionalInfo + "").Trim() == "SelfFinance")
                        {
                            new SelfFinancedDB().UpdateAppliedForSelfFinancedSeat(paymentDetails.Payment_ID, paymentDetails.Entity_ID, printProgramme);
                        }
                    }
                    ts.Complete();
                }
            }
            catch (SqlException SQLError) when (SQLError.Number == 1205)
            {
                return Tuple.Create(false, "An error occurred. Please try again");
            }

            return Tuple.Create(result > 0, "");
        }

        private int Save(Certificate certificate, PrintProgramme printProgramme)
        {
            NEPDB _NEPDB = new NEPDB();
            int result = 0;
            if (certificate != null)
            {
                string CertificatePath = GeneralFunctions.GetStudentCertificatePath(printProgramme)
                    + Guid.NewGuid() + "_Adm_" + certificate.CertificateType.ToString()
                    + Path.GetExtension(certificate.File.FileName);

                UploadAsync(certificate.File, CertificatePath, false, HttpContext.Current.Server);

                certificate.CertificateUrl = CertificatePath;
                certificate.Certificate_ID = Guid.NewGuid();
                certificate.VerificationStatus = VerificationStatus.Pending;
                certificate.UploadingDate = DateTime.Now;
                certificate.Student_ID = certificate.Student_ID;

                result += _NEPDB.DeleteCertificate(certificate.Student_ID, certificate.CertificateType);

                result += new StudentDB().Save(certificate, printProgramme);
            }

            return result;
        }

        public Task UploadAsync(HttpPostedFileBase fileBase, string TargetPath, bool isPhoto, HttpServerUtility httpServer)
        {
            return new TaskFactory().StartNew(() => Upload(fileBase, TargetPath, isPhoto, httpServer));
        }
        private void Upload(HttpPostedFileBase fileBase, string TargetPath, bool isPhoto, HttpServerUtility httpServer)
        {
            if (isPhoto)
                Helper.CompressAndUploadImage(fileBase.InputStream, 259, 194, TargetPath, httpServer);
            else
                Helper.UploadImage(fileBase.InputStream, TargetPath, httpServer);
        }

        public int DeleteCertificate(Guid Student_ID, CertificateType certificateType)
        {
            return new NEPDB().DeleteCertificate(Student_ID, certificateType);
        }





        public bool FormExists(Guid Student_ID, PrintProgramme printProgramme)
        {
            printProgramme = new RegistrationManager().MappingTable(printProgramme);
            return new NEPDB().FormExists(Student_ID, printProgramme);
        }

        public List<NEPCollegePreference> GetCollegePreferences(Guid Student_ID, PrintProgramme printProgramme)
        {
            printProgramme = new RegistrationManager().MappingTable(printProgramme);
            return new NEPDB().GetCollegePreferences(Student_ID, printProgramme);
        }

        public Guid GetStudentID(ARGReprint reprint)
        {
            ARGFormNoMaster formNoMaster = new RegistrationManager().GetFormNoMaster(reprint.PrintProgrammeOption);
            reprint.Batch = formNoMaster?.BatchToSet ?? (short)DateTime.Now.Year;
            reprint.AllowFormsToBeEditFromDate = formNoMaster.AllowFormsToBeEditFromDate;
            return new NEPDB().GetStudentIDByFormNoAndDOB(reprint);
        }

        public NEPDocuments GetDocuments(Guid Student_ID, PrintProgramme printProgramme)
        {
            printProgramme = new RegistrationManager().MappingTable(printProgramme);

            bool studentExists = new NEPDB().FormExists(Student_ID, printProgramme);
            if (!studentExists)
                return null;

            List<Certificate> certificates = new StudentManager().GetUploadedCertificates(Student_ID);
            // if (certificates.IsNullOrEmpty())
            //     return null;

            string photoPath = new NEPDB().GetStudentPhotoPath(Student_ID, printProgramme);

            return new NEPDocuments
            {
                Documents = certificates,
                PhotographUrl = photoPath,
                Student_ID = Student_ID
            };
        }

        public Task<Tuple<bool, string, NEPPersonalInfo>> GetNEPPersonalInfoAsync(Guid Student_ID, PrintProgramme printProgramme)
        {
            return new TaskFactory().StartNew(() => GetNEPPersonalInfo(Student_ID, printProgramme));
        }

        public Tuple<bool, string, NEPPersonalInfo> GetNEPPersonalInfo(Guid Student_ID, PrintProgramme printProgramme)
        {
            RegistrationManager registrationManager = new RegistrationManager();
            RegistrationDB _RegistrationDB = new RegistrationDB();
            NEPDB _NEPDB = new NEPDB();
            printProgramme = registrationManager.MappingTable(printProgramme);

            bool studentExists = _NEPDB.FormExists(Student_ID, printProgramme);

            if (!studentExists)
                return Tuple.Create<bool, string, NEPPersonalInfo>(false, "Student does not exist", null);

            ARGPersonalInformation _ARGPersonalInformation = _RegistrationDB.GetStudentByID(Student_ID, printProgramme);
            if (_ARGPersonalInformation == null)
                return Tuple.Create<bool, string, NEPPersonalInfo>(false, "Student does not exist", null);

            if (_ARGPersonalInformation.FormStatus == FormStatus.InProcess)
                return Tuple.Create<bool, string, NEPPersonalInfo>(false, "student has not paid fee", null);

            _ARGPersonalInformation.PaymentDetail = new PaymentManager().GetPaymentDetails(Student_ID, PaymentModuleType.Admission, printProgramme);

            if (_ARGPersonalInformation.PaymentDetail == null)
                return Tuple.Create<bool, string, NEPPersonalInfo>(false, "student has not paid fee", null);

            _ARGPersonalInformation.StudentAddress = _RegistrationDB.GetStudentAddress(Student_ID, printProgramme);
            _ARGPersonalInformation.AcademicDetails = _RegistrationDB.GetStudentAcademicDetails(Student_ID, printProgramme);
            _ARGPersonalInformation.CoursesApplied = _RegistrationDB.GetStudentCoursesApplied(Student_ID, printProgramme);

            List<NEPCollegePreference> collegePreference = null;

            if (printProgramme == PrintProgramme.UG)
            {
                collegePreference = GetCollegePreferences(Student_ID, printProgramme);
            }
            List<Certificate> documents = GetDocuments(Student_ID, printProgramme)?.Documents;

            if (documents != null)
            {
                if (_ARGPersonalInformation.Category.ToLower().Trim() != "om" && _ARGPersonalInformation.FormStatus != FormStatus.InProcess)
                {
                    if (!documents.Any(x => x.CertificateType == CertificateType.Category))
                        return Tuple.Create<bool, string, NEPPersonalInfo>(false, "Category Certificate not uploaded. Please upload category certificate before printing form", null);
                }
            }

            NEPPersonalInfo personalInfo = new NEPPersonalInfo
            {
                PersonalInformation = _ARGPersonalInformation,
                CollegePreference = collegePreference,
                Documents = documents,
            };

            return Tuple.Create(true, "", personalInfo);
        }

        public bool HasCategory(Guid Student_ID, PrintProgramme printProgramme)
        {
            printProgramme = new RegistrationManager().MappingTable(printProgramme);
            return new NEPDB().HasCategory(Student_ID, printProgramme);
        }

        public bool IsProvisionalAdm(Guid Student_ID, PrintProgramme printProgramme)
        {
            printProgramme = new RegistrationManager().MappingTable(printProgramme);
            return new NEPDB().IsProvisionalAdm(Student_ID, printProgramme);
        }

        public string IsEligibleForPayment(Guid Student_ID, PrintProgramme printProgramme)
        {
            try
            {
                NEPDB _NEPDB = new NEPDB();
                printProgramme = new RegistrationManager().MappingTable(printProgramme);
                bool check = _NEPDB.FormExists(Student_ID, printProgramme);
                bool isProvisionalAdm = _NEPDB.IsProvisionalAdm(Student_ID, printProgramme);
                bool hasCategory = HasCategory(Student_ID, printProgramme);


                if (!check)
                    return "Student not found";

                check = _NEPDB.SubjectsApplied(Student_ID, printProgramme);

                if (!check)
                    return "Not applied for any Course / Subjects yet. Please complete your registration form before making payment";

                if (!isProvisionalAdm)
                {
                    check = _NEPDB.QualificationsApplied(Student_ID, printProgramme);

                    if (!check)
                        return "Not submitted 12TH details yet. Please complete your registration form before making payment";
                }

                if (printProgramme == PrintProgramme.UG)
                {
                    check = _NEPDB.CollegePreferencesApplied(Student_ID);

                    if (!check)
                        return "Not applied for any college preferences yet. Please complete your registration form before making payment";
                }

                if (!isProvisionalAdm || hasCategory)
                {
                    check = _NEPDB.CertificateApplied(Student_ID);

                    if (!check)
                        return "Not uploaded any certificates yet. Please complete your registration form before making payment";
                }
            }
            catch (SqlException SQLError) when (SQLError.Number == 1205)
            {
                return "Please check your details again before making payment.";
            }
            return null;
        }

        public List<DropDownOptLabelGeneral> GetCourseListForRegistrationByGroup(PrintProgramme printProgramme, Guid? Student_ID)
        {
            printProgramme = new RegistrationManager().MappingTable(printProgramme);
            string gender = null;

            if ((Student_ID ?? Guid.Empty) != Guid.Empty && printProgramme == PrintProgramme.UG)
            {
                gender = new NEPDB().GetStudentGender(printProgramme, (Guid)Student_ID);
                if (gender.ToLower().Trim() == "transgender")
                    gender = null;
            }

            return new NEPDB().GetCourseListForRegistrationByGroup(printProgramme, gender)?.OrderBy(x => x.DataGroupField)?.ThenBy(x => x.Text)?.ToList() ?? new List<DropDownOptLabelGeneral>();
        }

        public bool CreateCookie(PrintProgramme programme, string Student_ID)
        {
            if (HttpContext.Current.Request.Cookies[programme.ToString().EncryptCookieAndURLSafe()] != null)
                return true;

            HttpCookie authCookie = new HttpCookie(programme.ToString().EncryptCookieAndURLSafe())
            {
                HttpOnly = true,
                Value = Student_ID
            };
            HttpContext.Current.Response.Cookies.Add(authCookie);

            return true;
        }

        public bool DeleteCookie(PrintProgramme programme)
        {
            HttpCookie hasCookie = HttpContext.Current.Request.Cookies[programme.ToString().EncryptCookieAndURLSafe()];
            if (hasCookie != null)
            {
                hasCookie.Expires = DateTime.UtcNow.AddDays(-7);
                HttpContext.Current.Response.Cookies.Add(hasCookie);
            }
            return true;
        }

        public bool HasCookiePresent(PrintProgramme programme, string Student_ID)
        {
            HttpCookie hasCookie = HttpContext.Current.Request.Cookies[programme.ToString().EncryptCookieAndURLSafe()];
            if (hasCookie != null)
            {
                return hasCookie.Value == Student_ID;
            }
            return false;
        }
    }
}
