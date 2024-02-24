using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using CUSrinagar.Models;
using CUSrinagar.DataManagers;
using System.Threading.Tasks;

namespace CUSrinagar.BusinessManagers
{
    public class SelfFinancedManager
    {
        public List<ARGCoursesApplied> GetCoursesApplied()
        {
            List<ARGCoursesApplied> StudentCoursesApplied = new RegistrationDB().GetStudentCoursesApplied(AppUserHelper.User_ID, AppUserHelper.TableSuffix);

            #region remove graduation Course if applied
            ARGCoursesApplied coursesAppliedGraduation = 
                StudentCoursesApplied?.FirstOrDefault(x => x.Course_ID == Guid.Parse("a3ee7f98-7b82-4d95-a2c0-faba7a18240e"));
            if (coursesAppliedGraduation != null)
            {
                StudentCoursesApplied.Remove(coursesAppliedGraduation);
            }
            return StudentCoursesApplied;
            #endregion
        }
        public ARGFormNoMaster GetApplyProgramme()
        {
            return new SelfFinancedDB().GetFromNoMasterForSF(AppUserHelper.OrgPrintProgramme);
        }

        public Tuple<bool, string, StringBuilder> Apply(List<Guid> CourseApplied_ID)
        {
            var RegnManagar = new RegistrationManager();
            PrintProgramme printProgramme = AppUserHelper.TableSuffix;
            ARGPersonalInformation aRGPersonalInformation = RegnManagar.GetStudentByID(AppUserHelper.User_ID, printProgramme);

            List<ARGCoursesApplied> _CoursesApplied = new RegistrationDB().GetCoursesApplied(AppUserHelper.User_ID, CourseApplied_ID, printProgramme);

            if (_CoursesApplied.IsNullOrEmpty() || aRGPersonalInformation.EntranceRollNo == null)
                return Tuple.Create(false, "Sorry! You are not eligible for self financed seat.", new StringBuilder());

            if (!_CoursesApplied.All(x => x.SelfFinancedPayment_ID.IsNullOrEmpty()))
                return Tuple.Create(false, "It seems that you have already applied, Indeed you can reprint receipt(s).", new StringBuilder());
            else
            {
                _CoursesApplied = _CoursesApplied.Where(x => x.SelfFinancedPayment_ID.IsNullOrEmpty() && x.AppearedInEntrance)?.ToList() ?? new List<ARGCoursesApplied>();
            }

            if (_CoursesApplied.IsNullOrEmpty())
                return Tuple.Create(false, "Sorry! You are not eligible for self financed seat. Reason not appeared in Entrance", new StringBuilder());

            if (printProgramme == PrintProgramme.IH && aRGPersonalInformation.CATEntrancePoints == null)
                return Tuple.Create(false, "Sorry! You are not eligible for self financed seat. Reason not appeared in CAT Entrance", new StringBuilder());

            var FromNoMaster = new SelfFinancedDB().GetFromNoMasterForSF(AppUserHelper.OrgPrintProgramme);
            if (FromNoMaster == null || !(aRGPersonalInformation.Batch >= FromNoMaster.BatchToSet))
                return Tuple.Create(false, "Sorry! the batch is not eligible for self financed seat.", new StringBuilder());

            //check for course Eligibility as per notification
            if (!string.IsNullOrWhiteSpace(FromNoMaster.AllowProgrammesInSelfFinance))
            {
                foreach (var item in _CoursesApplied)
                {
                    if (!FromNoMaster.AllowProgrammesInSelfFinance.ToLower().Contains(item.Course_ID.ToString().ToLower()))
                        return Tuple.Create(false, $"Sorry! {item.CourseName} is not eligible for SelfFinanced. Please read notification for eligible courses under SelfFinanced Quota.", new StringBuilder());
                }
            }

            string coursesApplied = string.Join("@", _CoursesApplied.Select(x => x.CourseCode));
            //int amount = FromNoMaster.SelfFinancedApplicationFee * (_CoursesApplied.Count()); // fee for individual courses

            int amount = FromNoMaster.SelfFinancedApplicationFee; // for one only

            //if already applied 
            aRGPersonalInformation.CoursesApplied = new SelfFinancedManager().GetCoursesApplied();
            if (aRGPersonalInformation.CoursesApplied.Any(x => !x.SelfFinancedPayment_ID.IsNullOrEmpty()))
            {
                int result = new PaymentManager().SaveSelfFinancedPaymentDetails(new PaymentDetails
                {
                    Payment_ID = aRGPersonalInformation.CoursesApplied.First(x => !x.SelfFinancedPayment_ID.IsNullOrEmpty()).SelfFinancedPayment_ID.Value,
                    AdditionalInfo = coursesApplied,
                    PrintProgramme = printProgramme
                }, false);

                return Tuple.Create<bool, string, StringBuilder>(true, "", null);
            }
            else
            {
                BillDeskRequest billDeskRequest = new BillDeskRequest
                {
                    Email = aRGPersonalInformation.StudentAddress.Email,
                    PhoneNo = aRGPersonalInformation.StudentAddress.Mobile,
                    ReturnURL = $"CUStudentZone/SelfFinanced/Success",
                    PrintProgramme = printProgramme,
                    Entity_ID = aRGPersonalInformation.Student_ID,
                    CustomerID = DateTime.UtcNow.Ticks.ToString() + new Random().Next(1, 99),
                    TotalFee = amount,
                    AdditionalInfo = coursesApplied

                };
                return Tuple.Create(true, "", new BillDeskManager().GenerateHTMLForm(new BillDeskManager().GenerateRequestString(billDeskRequest, BillDeskPaymentType.OTH)));
            }
        }

        public void SendPaymentSMSAsync(PaymentDetails billDeskResponse)
        {
#if (!DEBUG)
                SMSModel sms = new SMSModel { MobileNos = billDeskResponse.PhoneNumber };
                sms.Message = $"Application Amount of Rs: {billDeskResponse.TxnAmount} Received Successfully for {billDeskResponse.PrintProgramme.ToString()} SelfFinanced Seat{Environment.NewLine}TxnReferenceNo :  {billDeskResponse.TxnReferenceNo}{Environment.NewLine}Dated: {billDeskResponse.TxnDate.ToString("dd-MM-yyy")}";
                new TaskFactory().StartNew(() =>
                {
                    new SMSHttpPostClient().SendOTPMSG(sms);
                    new EmailSystem().SelfFinancedSeatMail(billDeskResponse);
                });
#endif
        }

        public PaymentDetails GetAppliedSelfFinancedPayment(Guid Payment_ID)
        {
            return new SelfFinancedDB().GetAppliedSelfFinancedPayment(AppUserHelper.TableSuffix, Payment_ID);
        }
    }
}
