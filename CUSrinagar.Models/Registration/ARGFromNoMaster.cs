using CUSrinagar.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUSrinagar.Models
{
    public class ARGFormNoMaster
    {
        public string FormPrefix { get; set; }
        public int FormNoCount { get; set; }
        public short BatchToSet { get; set; }
        public bool AllowApplyForSelfFinancedSeat { get; set; }
        public int SelfFinancedApplicationFee { get; set; }
        public bool AllowOnlinePayment { get; set; }
        /// <summary>
        /// Should be separated by |
        /// </summary>
        public string AllowProgrammesInSelfFinance { get; set; }
        public PrintProgramme PrintProgramme { get; set; }

        public int BasicFee { get; set; }
        public int PerCourse { get; set; }
        public int AdditionalFeeForNonJK { get; set; }
        public int FeeForAlreadyInIH { get; set; }
        public DateTime ClosingDate { get; set; }
        public bool ValidatePaymentByCourse { get; set; }
        public bool ShowLateralEntry { get; set; }
        public bool AllowEditAfterPayment { get; set; }
        public bool AllowNonCUET { get; set; }
        public DateTime? AllowFormsToBeEditFromDate { get; set; }
    }

    public class ARGFormNoMasterSettings
    {
        public string FormPrefix { get; set; }
        public int FormNoCount { get; set; }
        public short BatchToSet { get; set; }
        public bool AllowApplyForSelfFinancedSeat { get; set; }
        public bool ResetFormCount { get; set; }
        public int SelfFinancedApplicationFee { get; set; }
        public bool AllowOnlinePayment { get; set; }
        public List<Guid> AllowProgrammesInSelfFinance { get; set; }
        public string AllowProgrammesInSF { get; set; }
        public List<Guid> OpenRegistrationCourses { get; set; }
        public PrintProgramme PrintProgramme { get; set; }
        public int BasicFee { get; set; }
        public int PerCourse { get; set; }
        public int AdditionalFeeForNonJK { get; set; }
        public int FeeForAlreadyInIH { get; set; }
        public DateTime ClosingDate { get; set; }
        public bool ValidatePaymentByCourse { get; set; }
        public bool ShowLateralEntry { get; set; }
        public bool DownloadAdmitCards { get; set; }
        public bool AllowEditAfterPayment { get; set; }
        public bool AllowNonCUET { get; set; }
        public DateTime? AllowFormsToBeEditFromDate { get; set; }
    }
}
