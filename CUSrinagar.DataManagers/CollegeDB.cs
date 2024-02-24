using CUSrinagar.DataManagers.SQLQueries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Terex;
using CUSrinagar.Models;
using System.Data.SqlClient;
using GeneralModels;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;

namespace CUSrinagar.DataManagers
{
    public class CollegeDB
    {
        public static List<SelectListItem> CollegeList()
        {
            return new MSSQLFactory().GetObjectList<SelectListItem>(new CollegeSQLQueries().GetCollegeList);
        }
        public static List<SelectListItem> GetADMCollegeMasterList()
        {
            return new MSSQLFactory().GetObjectList<SelectListItem>(new CollegeSQLQueries().GetADMCollegeMasterList);
        }
        public List<SelectListItem> GetALLCollegeMasterList()
        {
            return new MSSQLFactory().GetObjectList<SelectListItem>(@"SELECT Convert(Varchar(50), College_ID) AS Value, CollegeFullName+' '+Address AS Text  FROM ADMCollegeMaster ORDER BY CollegeCode ASC");
        }

        public ADMCollegeMaster GetItem(Guid College_ID)
        {
            string query = "SELECT * FROM ADMCollegeMaster WHERE College_ID=@College_ID";
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = query;
            cmd.Parameters.AddWithValue("@College_ID", College_ID);
            return new MSSQLFactory().GetObject<ADMCollegeMaster>(cmd);
        }

        public List<ADMCollegeMaster> GetAllCollegeList(Parameters parameter)
        {
            return new MSSQLFactory().GetObjectList<ADMCollegeMaster>(new CollegeSQLQueries().GetAllCollegeList(parameter));
        }

        public static List<CUSCollegeInfoDashboard> CUSCollegeInfoDashboard(string year)
        {
            string query = "select ClgCode,CourseCategory,count(*) as Roll from TempRegistration group by ClgCode,CourseCategory order by ClgCode,CourseCategory";
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = query;
            //cmd.Parameters.AddWithValue("@College_ID", year);
            return new MSSQLFactory().GetObjectList<CUSCollegeInfoDashboard>(cmd);
        }

        public static List<ADMCollegeMaster> GetADMCollegeMasterAllList()
        {
            return new MSSQLFactory().GetObjectList<ADMCollegeMaster>(new CollegeSQLQueries().GetADMCollegeMasterAllList);
        }

        public static List<SelectListItem> GetCollegeWiseNewRegistrations(PrintProgramme programme, short batchToAssign)
        {
            return new MSSQLFactory().GetObjectList<SelectListItem>(new CollegeSQLQueries().GetCollegeWiseNewRegistrations(programme, batchToAssign));
        }
        public List<SelectListItem> GetCollegePreference(List<Guid> Course_IDs)
        {
            var InClause = Course_IDs.ToIN<Guid>();
            string Query = $@"SELECT DISTINCT C.CollegeFullName [Text],CAST(C.College_ID AS NVARCHAR(MAX)) [Value]
                                FROM dbo.ADMCollegeMaster C
                                JOIN dbo.ADMCollegeCourseMapping CM ON CM.College_ID = C.College_ID
                                WHERE CM.Course_ID  IN ({InClause}) ORDER BY C.CollegeFullName";
            return new MSSQLFactory().GetObjectList<SelectListItem>(Query);
        }
        public string GetColleges(string colleges)
        {
            colleges = colleges.Replace(",", "','");
            string query = $@"SELECT Stuff(
                                  (SELECT N', ' + CollegeFullName FROM ADMCollegeMaster  WHERE College_ID in ('{colleges}') FOR XML PATH(''), TYPE)
                                  .value('text()[1]', 'nvarchar(max)'),1,2,N'') ";
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = query;
            return new MSSQLFactory().ExecuteScalar<string>(cmd);
        }

    }
}
