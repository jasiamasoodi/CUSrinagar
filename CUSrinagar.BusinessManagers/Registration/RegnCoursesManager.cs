using CUSrinagar.Enums;
using CUSrinagar.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using CUSrinagar.Extensions;
using System.Threading.Tasks;
using CUSrinagar.DataManagers;
using Terex;
using System.Transactions;
using System.Web.Mvc;
using System.IO;
using System.Web.Hosting;

namespace CUSrinagar.BusinessManagers
{
    public class RegnCoursesManager
    {
        public bool IsRegistrationOpen()
        {
            return new RegnCoursesDB().IsRegistrationOpen(AppUserHelper.TableSuffix);
        }
        public List<ARGCoursesApplied> GetCoursesApplied()
        {
            List<ARGCoursesApplied> _CoursesApplied = new RegistrationDB().GetStudentCoursesApplied(AppUserHelper.User_ID, AppUserHelper.TableSuffix);
            if (_CoursesApplied.IsNullOrEmpty())
                return null;
            _CoursesApplied[0].PaymentDetail = new PaymentDB().GetPaymentDetail(AppUserHelper.User_ID, PaymentModuleType.Admission, AppUserHelper.TableSuffix);
            return _CoursesApplied;
        }

        public int CalculateFeeFor_IHP_Only(List<ARGCoursesApplied> coursesApplied, List<string> excludeGroupInFee, ARGFormNoMaster formNoMaster, bool IsJKResident, bool hasPayment)
        {
            if (hasPayment && coursesApplied.IsNullOrEmpty())
                return 0;

            if (coursesApplied.IsNullOrEmpty())
                throw new Exception("Calculate Fee For IHP Only, Courses Applied should not be null or empty");

            bool hasPayments = new PaymentDB().GetPaymentDetail(AppUserHelper.User_ID, PaymentModuleType.Admission, AppUserHelper.TableSuffix) != null;

            int PercourseFee = formNoMaster.PerCourse;
            int basicFee = hasPayments ? 0 : formNoMaster.BasicFee;

            if (coursesApplied.Any(x => string.IsNullOrWhiteSpace(x.GroupName)))
            {
                coursesApplied.ForEach(x =>
                {
                    if (string.IsNullOrWhiteSpace(x.GroupName))
                    {
                        x.GroupName = x.Course_ID.ToString();
                    }
                });
            }

            int CourseCount = coursesApplied.GroupBy(x => x.GroupName.ToLower().Trim()).Count();

            foreach (string item in excludeGroupInFee)
            {
                if (coursesApplied.Any(x => x.GroupName.ToLower().Trim() == item.ToLower().Trim()))
                    CourseCount--;
            }


            if (coursesApplied.Count == 1 && coursesApplied.Any(x => x.Course_ID == Guid.Parse("A3EE7F98-7B82-4D95-A2C0-FABA7A18240E")))
            {
                return basicFee + 50;
            }
            if (coursesApplied.Count > 1 && coursesApplied.Any(x => x.Course_ID == Guid.Parse("A3EE7F98-7B82-4D95-A2C0-FABA7A18240E")))
            {
                CourseCount--;
            }

            int totalFee = (PercourseFee * CourseCount) + basicFee;


            if (!hasPayments && !IsJKResident)
            {
                totalFee += formNoMaster.AdditionalFeeForNonJK;
            }

            if (hasPayments)
            {
                totalFee -= basicFee;
            }

            return totalFee;
        }

        public int CalculateFeeFor_PG_Only(List<Guid> coursesApplied, ARGFormNoMaster formNoMaster, bool IsJKResident)
        {
            bool hasPayments = new PaymentDB().GetPaymentDetail(AppUserHelper.User_ID, PaymentModuleType.Admission, AppUserHelper.TableSuffix) != null;
            int basicFee = hasPayments ? 0 : formNoMaster.BasicFee;
            int PerSubject = formNoMaster.PerCourse;
            int totalFee = basicFee + (PerSubject * coursesApplied.Count);

            if (totalFee > 0 && !hasPayments && !IsJKResident)
            {
                totalFee += formNoMaster.AdditionalFeeForNonJK;
            }
            return totalFee;
        }

        public int CalculateFeeFor_UG_Only(bool IsJKResident, string BoardRegnNo, short Batch, ARGFormNoMaster formNoMaster, bool hasAlreadyFeePaid, List<Guid> coursesApplied)
        {
            bool hasPayments = new PaymentDB().GetPaymentDetail(AppUserHelper.User_ID, PaymentModuleType.Admission, AppUserHelper.TableSuffix) != null;
            bool hasAlreadyAppliedInIG = formNoMaster.FeeForAlreadyInIH > 0 ? new RegistrationDB().HasAlreadyAppliedInIG(BoardRegnNo, Batch) : false;
            if (hasAlreadyAppliedInIG)
            {
                if (hasPayments)
                    return 0;

                return formNoMaster.FeeForAlreadyInIH;
            }

            int basicFee = hasPayments ? 0 : formNoMaster.BasicFee;
            int PerSubject = formNoMaster.PerCourse;
            int totalFee = basicFee + (PerSubject * coursesApplied.Count);

            if (totalFee > 0 && !hasPayments && !IsJKResident)
            {
                totalFee += formNoMaster.AdditionalFeeForNonJK;
            }
            return totalFee;
        }


        public Tuple<bool, string> AddEditQualificationDetails(ARGStudentPreviousQualifications previousQualifications, PrintProgramme? pprogramme = null)
        {
            int result = 0;
            PrintProgramme programme = pprogramme ?? AppUserHelper.TableSuffix;
            if (previousQualifications != null)
            {
                using (var ts = new TransactionScope())
                {
                    if (previousQualifications.Qualification_ID != Guid.Empty)
                    {
                        previousQualifications.UpdatedOn = DateTime.Now;
                        previousQualifications.UpdatedBy = pprogramme == null ? AppUserHelper.User_ID : previousQualifications.Student_ID;
                        result = new AdminRegistrationDB().UpdateStudentQualification(previousQualifications, programme);
                    }
                    else
                    {
                        previousQualifications.CreatedOn = DateTime.Now;
                        previousQualifications.CreatedBy = pprogramme == null ? AppUserHelper.User_ID : previousQualifications.Student_ID;
                        previousQualifications.Student_ID = previousQualifications.CreatedBy.Value;
                        previousQualifications.Qualification_ID = Guid.NewGuid();
                        result = new RegistrationDB().Save(previousQualifications, programme);
                        if (result > 0)
                            new RegistrationDB().UpdateIsProvisional(previousQualifications.Student_ID, programme);
                    }
                    ts.Complete();
                }
            }
            if (result > 0)
                return Tuple.Create(true, "Saved successfully");
            else
                return Tuple.Create(false, "could not saved.");
        }

        public int UpdateCertificates(UpdateCertificates updateCertificates)
        {
            var _RegnCoursesDB = new RegnCoursesDB();
            int result = 0;
            try
            {
                if (updateCertificates.CertificateMarksCard != null)
                {
                    string oldPath = updateCertificates.OldMarksCardPath;
                    string MarksCardPath = GeneralFunctions.GetStudentCertificatePath(AppUserHelper.TableSuffix) + Guid.NewGuid().ToString() + "_Adm_" + CertificateType.MarksCard.ToString() + "_" + DateTime.Now.Year + System.IO.Path.GetExtension(updateCertificates.CertificateMarksCard.FileName);
                    Helper.UploadImage(updateCertificates.CertificateMarksCard.InputStream, MarksCardPath);
                    updateCertificates.OldMarksCardPath = MarksCardPath;
                    _RegnCoursesDB.UpdateCertificates(updateCertificates);
                    if (File.Exists(HostingEnvironment.MapPath("~" + oldPath)))
                    {
                        File.Delete(HostingEnvironment.MapPath("~" + oldPath));
                    }
                    result = 1;
                }
                PrintProgramme printProgramme = AppUserHelper.TableSuffix;
                foreach (var item in updateCertificates?.coursesApplied ?? new List<ARGCoursesApplied>())
                {
                    result += _RegnCoursesDB.UpdateCourseCGPA(item, printProgramme);
                }
            }
            catch (Exception)
            {
                result = 0;
            }
            return result;
        }
    }
}
