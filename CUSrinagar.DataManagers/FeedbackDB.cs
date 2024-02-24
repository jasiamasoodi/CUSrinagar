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
    public class FeedbackDB
    {
        public int Save(DailyLectureFeedBack dailyLectureFeedBack)
        {
            return new MSSQLFactory().InsertRecord(dailyLectureFeedBack);
        }

        public List<DailyLectureFeedBack> GetDailyLectureFeedBacks(Parameters parameters, PrintProgramme printProgramme)
        {
            string TSQL = $@"SELECT d.*,
                                   p.FullName,
                                   p.CUSRegistrationNo,
                                   p.CurrentSemesterOrYear,
                                   cm.CourseFullName,
                                   sm.SubjectFullName,
                                   sm.SubjectType,
                                   s.Semester,
                                   c.College_ID,
                                   cc.CollegeFullName
                            FROM dbo.DailyLectureFeedBack d
                                JOIN dbo.ARGPersonalInformation_{printProgramme} p
                                    ON p.Student_ID = d.Student_ID
                                       AND p.IsPassout = 0
                                       AND p.FormStatus NOT IN ({(short)FormStatus.InProcess}, 
                                        {(short)FormStatus.CancelRegistration}, {(short)FormStatus.FeePaid} ) 
                                JOIN dbo.ADMSubjectMaster sm
                                    ON sm.Subject_ID = d.Subject_ID
                                JOIN dbo.ARGSelectedCombination_{printProgramme} s
                                    ON s.Student_ID = p.Student_ID
                                       AND sm.Semester = s.Semester
                                JOIN dbo.ADMCombinationMaster c
                                    ON c.Combination_ID = s.Combination_ID
                                JOIN dbo.ADMCourseMaster cm
                                    ON cm.Course_ID = c.Course_ID 
                               JOIN dbo.ADMCollegeMaster cc ON cc.College_ID = c.College_ID ";

            FilterHelper helper = new GeneralFunctions().GetWhereClause<DailyLectureFeedBack>(parameters.Filters);
            TSQL += helper.WhereClause;
            helper.Command.CommandText = TSQL;
            return new MSSQLFactory().GetObjectList<DailyLectureFeedBack>(helper.Command);
        }

        public bool ValidateABCID(string aBCID)
        {
            SqlCommand cmd = new SqlCommand(
                $@"SELECT SUM(temp.AbcIDCount) FROM (
                    SELECT COUNT(ABCID) AbcIDCount FROM dbo.ARGPersonalInformation_UG WHERE Batch=@batch AND ABCID=@abcid AND Student_ID<>@Student_ID
                    UNION
                    SELECT COUNT(ABCID) AbcIDCount FROM dbo.ARGPersonalInformation_IH WHERE Batch=@batch AND ABCID=@abcid AND Student_ID<>@Student_ID
                    UNION
                    SELECT COUNT(ABCID) AbcIDCount FROM dbo.ARGPersonalInformation_PG WHERE Batch=@batch AND ABCID=@abcid AND Student_ID<>@Student_ID
                    ) temp"
                );
            AppUserCompact appUserCompact = AppUserHelper.AppUsercompact;
            cmd.Parameters.AddWithValue("@batch", appUserCompact.OrgBatch);
            cmd.Parameters.AddWithValue("@Student_ID", appUserCompact.User_ID);
            cmd.Parameters.AddWithValue("@abcid", aBCID);

            return new MSSQLFactory().ExecuteScalar<int>(cmd) > 0;
        }

        public int UpdateABCID(Guid student_ID, PrintProgramme printProgramme, string ABCID)
        {
            SqlCommand cmd = new SqlCommand($@"UPDATE dbo.ARGPersonalInformation_{printProgramme} SET ABCID=@abcid WHERE Student_ID='{student_ID}'");
            cmd.Parameters.AddWithValue("@abcid", ABCID);
            return new MSSQLFactory().ExecuteNonQuery(cmd);
        }
    }
}
