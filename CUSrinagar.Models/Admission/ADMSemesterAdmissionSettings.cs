using System;

namespace CUSrinagar.Models
{
    public class ADMSemesterAdmissionSettings
    {
        public Guid Course_ID { get; set; }
        public short Semester { get; set; }
        public decimal AdmissionFee { get; set; }
        public short AllowUptoBatch { get; set; }
        public bool Status { get; set; }
        public bool AllowRePrint { get; set; }
        public bool AllowOnlySpecificCUSRegnNos { get; set; }
        public decimal? SelfFinanceFee { get; set; }
        public decimal? CollegeComponent { get; set; }

        /// <summary>
        /// SFStudentsFormNo separated by |
        /// </summary>
        public string SFStudentsFormNo { get; set; }
        public DateTime ClosingDate { get; set; }
    }

    public class AllowInSemesterAdm
    {
        public string CUSRegistrationNoToAllow { get; set; }
        public short AllowForSemesterAdm { get; set; }
        public decimal AdditionalFee { get; set; }
    }
}
