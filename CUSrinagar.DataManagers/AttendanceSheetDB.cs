using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CUSrinagar.Models;
using Terex;
using System.Data.SqlClient;
using CUSrinagar.DataManagers.SQLQueries;
using System.Web.Mvc;
using GeneralModels;
using CUSrinagar.Enums;

namespace CUSrinagar.DataManagers
{
    public class AttendanceSheetDB
    {

        #region Attendance sheets Related Routines
        public List<SelectListItem> GetAllSemestersByPrintProgramme(PrintProgramme printProgramme)
        {
            string query = @"SELECT DISTINCT CAST(Semester AS NVARCHAR(2)) AS[Value],'Semester-' + CAST((Semester)AS NVARCHAR(2)) AS[Text] FROM ARGExamFormDownloadable
                     WHERE AllowDownloadAdmitCards = 1  AND PrintProgramme = @PrintProgramme ";
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.AddWithValue("@PrintProgramme",printProgramme);
            cmd.CommandText = query;
            return new MSSQLFactory().GetObjectList<SelectListItem>(cmd);
        }

        public List<SelectListItem> GetYearDDL()
        {
            return new MSSQLFactory().GetObjectList<SelectListItem>(@"
                     SELECT DISTINCT CAST((Year) AS NVARCHAR(10)) AS [Text],CAST((Year) AS NVARCHAR(10)) AS [value]   FROM ARGExamFormDownloadable
                     WHERE AllowDownloadAdmitCards=1     
                    ");
        }


        public List<AttendanceSheetForm> GetAllStudentsLibrary(PrintProgramme programme, Parameters parameter)
        {
            return new MSSQLFactory().GetObjectList<AttendanceSheetForm>(new StudentSQLQueries().GetAllStudentsLibrary(programme, parameter));
        }

        public List<SelectListItem> GetSemesterExamCourses(PrintProgramme programme)
        {
            return new MSSQLFactory().GetObjectList<SelectListItem>
                        ($@"SELECT DISTINCT convert(nvarchar(120),ADMCourseMaster.Course_ID) AS Value,CourseFullName AS Text FROM ARGCentersAllotmentMaster
                        JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = ARGCentersAllotmentMaster.Course_ID
                        JOIN ARGEntranceCentersMaster ON ARGEntranceCentersMaster.Center_ID = ARGCentersAllotmentMaster.Center_ID
                        WHERE College_ID='{AppUserHelper.College_ID}' AND ADMCourseMaster.Status=1 AND ADMCourseMaster.PrintProgramme={(short)programme}");
        }

        public List<SelectListItem> GetSemesterExamCourses(PrintProgramme programme, Guid College_ID)
        {
            return new MSSQLFactory().GetObjectList<SelectListItem>
                        ($@"SELECT DISTINCT convert(nvarchar(120),ADMCourseMaster.Course_ID) AS Value,CourseFullName AS Text FROM ARGCentersAllotmentMaster
                        JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = ARGCentersAllotmentMaster.Course_ID
                        JOIN ARGEntranceCentersMaster ON ARGEntranceCentersMaster.Center_ID = ARGCentersAllotmentMaster.Center_ID
                        WHERE College_ID='{College_ID}' AND ADMCourseMaster.Status=1 AND ADMCourseMaster.PrintProgramme={(short)programme}");
        }

        #endregion
    }
}
