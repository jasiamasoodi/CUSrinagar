using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using CUSrinagar.DataManagers.SQLQueries;
using CUSrinagar.Enums;
using CUSrinagar.Models;
using Terex;

namespace CUSrinagar.DataManagers
{
    public class RegularExaminationsDB
    {
        public List<SelectListItem> GetAllowedSemesterExaminations()
        {
            return new MSSQLFactory().GetObjectList<SelectListItem>(SQLQueries.RegularExaminationsSQLQueries.GetAllowedSemesterExaminations);
        }

        public RegularExaminationViewModel GetExaminationFormViewModel(Guid user_ID)
        {
            return new MSSQLFactory().GetObject<RegularExaminationViewModel>(SQLQueries.RegularExaminationsSQLQueries.GetExaminationFormViewModel(AppUserHelper.OrgPrintProgramme, user_ID));
        }

        public bool ExaminationAllowedForStudentCourse(Guid user_ID, PrintProgramme printProgramme, int semester)
        {
            return new MSSQLFactory().ExecuteScalar<bool>(SQLQueries.RegularExaminationsSQLQueries.ExaminationAllowedForStudentCourse(user_ID, printProgramme, semester));
        }

        public ARGExamFormDownloadable GetRegularExamFormDownloadableWithoutDateValidation(ExaminationCourseCategory CourseCategory, int semester)
            => new MSSQLFactory().GetObject<ARGExamFormDownloadable>(SQLQueries.RegularExaminationsSQLQueries.GetRegularExamFormDownloadableWithoutDateValidation(CourseCategory, semester));

        public int InsertStudentExamForm(RegularExaminationForm studentexamForm, PrintProgramme printProgramme)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            return new MSSQLFactory().InsertRecord(studentexamForm, $"ARGStudentExamForm_{printProgramme.ToString()}");
        }

        public int UpdateExamFormDownloadable(Guid aRGExamForm_ID, int formNumberCount)
        {
            string query = @"UPDATE ARGExamFormDownloadable Set FormNumberCount=@FormNumberCount Where ARGExamForm_ID = @ARGExamForm_ID";
            SqlCommand sqlCommand = new SqlCommand(query);
            sqlCommand.Parameters.AddWithValue("@ARGExamForm_ID", aRGExamForm_ID);
            sqlCommand.Parameters.AddWithValue("@FormNumberCount", formNumberCount);
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }

        public RegularExaminationForm GetExaminationForm(Guid user_ID, int applyingForSemester)
        {
            string query = $@"SELECT StudentExamForm_ID, Student_ID, Semester, FormNumber, Year, ExamRollNumber, AmountPaid, Status, CreatedOn,
                                    UpdatedOn, CreatedBy, UpdatedBy, IsRegular, StudentCode, Notification_ID, LateFeeAmount FROM ARGStudentExamForm_UG 
                                    WHERE IsRegular =1 AND Student_ID = '{user_ID}' AND Semester = {applyingForSemester}";

            return new MSSQLFactory().GetObject<RegularExaminationForm>(query);
        }
    }
}
