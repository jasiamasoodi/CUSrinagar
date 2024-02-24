using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUSrinagar.DataManagers.SQLQueries
{
    public class FeeStructureSQLQueries
    {
        internal SqlCommand FeeStructureAdmissionForIntegratedAndPGCourses(int Programme, int Year)
        {
            string MasterCourseValue = "";

            switch (Programme)
            {
                case 1:
                    MasterCourseValue = "'PG', 'HN', 'IG'";
                    break;

                case 2:
                    MasterCourseValue = "'PROF'";
                    break;

                case 3:
                    MasterCourseValue = "'UG'";
                    break;

                default:
                    break;
            }

            SqlCommand command = new SqlCommand()
            {
                CommandText = $@"SELECT MasterCourseCategory, FeeStructureAdmission.EnrollmentFee,
                                                            FeeStructureAdmission.RegistrationFee, FeeStructureAdmission.UniversityDevFee,
                                                            FeeStructureAdmission.SportsDevFee, FeeStructureAdmission.CorpusFund, FeeStructureAdmission.MagazineFee,
                                                            FeeStructureAdmission.IT_SSFee, FeeStructureAdmission.ResDevFund, FeeStructureAdmission.StudentAidFund,
                                                            FeeStructureAdmission.AlumniFund, FeeStructureAdmission.DIQAFund, FeeStructureAdmission.NSSFund,
                                                            FeeStructureAdmission.CulturalLiteraryFund, FeeStructureAdmission.ICardFee, FeeStructureAdmission.LibraryFee,
                                                            FeeStructureAdmission.AdditionalFee
                                    FROM dbo.FeeStructureMaster
                                        LEFT JOIN dbo.FeeStructureAdmission
                                            ON FeeStructureAdmission.FeeStructure_ID = FeeStructureMaster.FeeStructure_ID
                                    WHERE Semester = 1
                                          AND Year = {Year}
                                          AND ModuleType = 'Admission'
                                          AND MasterCourseCategory IN ({MasterCourseValue}); "
            };

            return command;
        }
    }
}
