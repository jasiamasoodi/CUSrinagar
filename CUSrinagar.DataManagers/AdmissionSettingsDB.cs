using CUSrinagar.Enums;
using CUSrinagar.Models;
using GeneralModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;
using Terex;
using CUSrinagar.Extensions;

namespace CUSrinagar.DataManagers
{
    public class AdmissionSettingsDB
    {
        #region Regular Admission Settings
        public List<ARGFormNoMaster> GetFormNoMasterList()
        {
            return new MSSQLFactory().GetObjectList<ARGFormNoMaster>("SELECT * FROM dbo.ARGFormNoMaster");
        }
        public ARGFormNoMasterSettings GetFromNoMaster(PrintProgramme printProgramme)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.AddWithValue("@Programme", printProgramme);
            cmd.CommandText = "SELECT Top 1 *,AllowProgrammesInSelfFinance AS AllowProgrammesInSF FROM ARGFormNoMaster WHERE PrintProgramme=@Programme ORDER BY BatchToSet DESC";
            return new MSSQLFactory().GetObject<ARGFormNoMasterSettings>(cmd);
        }
        public List<SelectListItem> GetCourses(PrintProgramme printProgramme)
        {
            return new MSSQLFactory().GetObjectList<SelectListItem>($@"SELECT CAST(Course_ID AS NVARCHAR(50)) AS [Value],CourseFullName AS [Text] FROM dbo.ADMCourseMaster 
                                                                        WHERE Status = 1 AND MinCombinationSubjects > 0 AND PrintProgramme = {(short)printProgramme}
                                                                        ORDER BY Programme, CourseFullName");
        }
        public int UpdateFromNoMaster(ARGFormNoMaster formNoMaster)
        {
            SqlCommand cmd = new MSSQLFactory().UpdateRecord(formNoMaster, new List<string> { "FormNoCount", "PrintProgramme" }, new List<string> { "FormNoCount" });
            cmd.CommandText += " where PrintProgramme=@PrintProgramme";
            return new MSSQLFactory().ExecuteNonQuery(cmd);
        }
        public int ResetFormNoCount(PrintProgramme printProgramme)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText += $"UPDATE dbo.ARGFormNoMaster SET FormNoCount=1 WHERE PrintProgramme={(short)printProgramme}";
            return new MSSQLFactory().ExecuteNonQuery(cmd);
        }
        public int CloseRegistration(PrintProgramme printProgramme)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText += $@"UPDATE dbo.ADMCourseMaster SET RegistrationOpen=0 WHERE PrintProgramme={(short)printProgramme};";
            return new MSSQLFactory().ExecuteNonQuery(cmd);
        }
        public int OpenRegistration(List<Guid> Courses_ID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText += $@"UPDATE dbo.ADMCourseMaster SET RegistrationOpen=1 WHERE Course_ID IN({Courses_ID.ToIN()})";
            return new MSSQLFactory().ExecuteNonQuery(cmd);
        }
        public int UpdateAllowDownloadEntranceAdmitCards(PrintProgramme printProgramme, bool DownloadAdmitCards,bool IsEngLateralEntry)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText += $"UPDATE dbo.ARGDownloadAdmitCards SET AllowDownloadAdmitCards={(DownloadAdmitCards ? "1" : "0")},IsEngLateralEntry={(IsEngLateralEntry ? "1" : "0")} WHERE PrintProgramme={(short)printProgramme}";
            return new MSSQLFactory().ExecuteNonQuery(cmd);
        }
        public bool GetAllowDownloadEntranceAdmitCards(PrintProgramme printProgramme,short ForAcademicSession)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText += $"SELECT TOP 1 AllowDownloadAdmitCards FROM ARGDownloadAdmitCards WHERE PrintProgramme={(short)printProgramme} AND ForAcademicSession={ForAcademicSession}";
            return new MSSQLFactory().ExecuteScalar<bool>(cmd);
        }
        #endregion
    }
}
