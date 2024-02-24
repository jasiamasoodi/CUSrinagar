using CUSrinagar.Enums;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUSrinagar.DataManagers.SQLQueries
{
    public class RegularExaminationsSQLQueries
    {
        internal static string GetAllowedSemesterExaminations = null;

        internal static string GetExaminationFormViewModel(PrintProgramme printProgramme, Guid user_ID)
        {
            var personalInformationtableName = new GeneralFunctions().GetTableName(printProgramme, Module.PersonalInformation);

            var addresstableName = new GeneralFunctions().GetTableName(printProgramme, Module.Address);

            string query = $@"SELECT FullName,CUSRegistrationNo RegistrationNumber,CurrentSemesterOrYear CurrentSemester,Email,Mobile FROM {personalInformationtableName} 
                                INNER JOIN {addresstableName} ON {addresstableName}.Student_ID = {personalInformationtableName}.Student_ID WHERE {personalInformationtableName}.Student_ID = '{user_ID}'";

            return query;
        }

        internal static SqlCommand ExaminationAllowedForStudentCourse(Guid user_ID, PrintProgramme printProgramme, int semester)
        {
            SqlCommand command = new SqlCommand
            {
                CommandText = $@" SELECT AllowExaminationForm FROM ARGSelectedCombination_{printProgramme.ToString()}
                                        INNER JOIN ADMCombinationMaster ON ADMCombinationMaster.Combination_ID = ARGSelectedCombination_{printProgramme.ToString()}.Combination_ID
                                        INNER JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = ADMCombinationMaster.Course_ID
                                        WHERE Student_ID = @Student_ID "
            };
            command.Parameters.AddWithValue("@Student_ID", user_ID);
            return command;
        }

        internal static SqlCommand GetRegularExamFormDownloadableWithoutDateValidation(ExaminationCourseCategory CourseCategory, int semester)
        {
            SqlCommand command = new SqlCommand
            {
                CommandText = @"SELECT * FROM ARGExamFormDownloadable WHERE CourseCategory = @CourseCategory AND Semester=@Semester AND IsBacklogSetting = 0"
            };
            command.Parameters.AddWithValue("@CourseCategory", CourseCategory.ToString());
            command.Parameters.AddWithValue("@Semester", semester);
            return command;
        }

    }
}
