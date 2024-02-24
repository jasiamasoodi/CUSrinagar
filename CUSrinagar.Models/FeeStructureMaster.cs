using CUSrinagar.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUSrinagar.Models
{
    public class FeeStructureAdmission
    {
        public Guid FeeStructure_ID { get; set; }

        public int EnrollmentFee { get; set; }

        public int RegistrationFee { get; set; }

        public int UniversityDevFee { get; set; }

        public int SportsDevFee { get; set; }

        public int CorpusFund { get; set; }

        public int MagazineFee { get; set; }

        public int IT_SSFee { get; set; }

        public int ResDevFund { get; set; }

        public int StudentAidFund { get; set; }

        public int AlumniFund { get; set; }

        public int DIQAFund { get; set; }

        public int NSSFund { get; set; }

        public int CulturalLiteraryFund { get; set; }

        public int ICardFee { get; set; }

        public int LibraryFee { get; set; }

        public MasterCourseCategory MasterCourseCategory { get; set; }

        public int AdditionalFee { get; set; }

        public int TotalFee { get; set; }
    }

    public class FeeStructureMaster
    {
        public Guid Master_ID { get; set; }

        public int Year { get; set; }

        public MasterCourseCategory MasterCourseCategory { get; set; }

        public int Semester { get; set; }

        public PaymentModuleType ModuleType { get; set; }

        public Guid FeeStructure_ID { get; set; }
    }

    public class FeeStructureViewModel
    {
        public MasterCourseCategory MasterCourseCategory { get; set; }

        public string FeeComponent { get; set; }

        public int FeeComponentAmount { get; set; }

        public int NoOfStudents { get; set; }

        public int SubTotal { get; set; }
    }

}
