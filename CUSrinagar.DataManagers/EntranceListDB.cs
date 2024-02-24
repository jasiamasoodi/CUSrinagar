using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Terex;
using CUSrinagar.Models;
using System.Data.SqlClient;
using CUSrinagar.Extensions;
using CUSrinagar.Enums;

namespace CUSrinagar.DataManagers
{
    public class EntranceListDB
    {
        public List<ADMEntranceLists> GetList()
        {
            string TSQL = $@"SELECT EntranceList_ID,
                                ADMEntranceLists.Course_ID,
                                ListType,
                                ListURL,
                                ADMEntranceLists.CreatedOn,
                                ADMEntranceLists.CreatedBy,
                                ADMEntranceLists.UpdatedOn,
                                ADMEntranceLists.UpdatedBy,
                                ADMCourseMaster.Course_ID,
                                CourseFullName
	                            FROM ADMEntranceLists 
                            JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = ADMEntranceLists.Course_ID
                            ORDER BY ISNULL(ADMEntranceLists.UpdatedOn,ADMEntranceLists.CreatedOn) DESC";
            return new MSSQLFactory().GetObjectList<ADMEntranceLists>(TSQL);
        }

        public int Delete(Guid Id)
        {
            return new MSSQLFactory().ExecuteNonQuery($"DELETE FROM ADMEntranceLists WHERE (EntranceList_ID='{Id}' OR Course_ID='{Id}');");
        }
        public int DeleteAll()
        {
            return new MSSQLFactory().ExecuteNonQuery($"DELETE FROM ADMEntranceLists;");
        }

        public int Save(ADMEntranceLists entranceLists)
        {
            return new MSSQLFactory().InsertRecord(entranceLists);
        }
        public int Update(ADMEntranceLists entranceLists)
        {
            string TSQL = "UPDATE ADMEntranceLists SET ListURL=@ListURL,UpdatedOn=@UpdatedOn,UpdatedBy=@UpdatedBy WHERE EntranceList_ID=@EntranceList_ID";
            SqlCommand cmd = new SqlCommand(TSQL);
            cmd.Parameters.AddWithValue("@ListURL", entranceLists.ListURL);
            cmd.Parameters.AddWithValue("@EntranceList_ID", entranceLists.EntranceList_ID);
            cmd.Parameters.AddWithValue("@UpdatedOn", entranceLists.UpdatedOn);
            cmd.Parameters.AddWithValue("@UpdatedBy", entranceLists.UpdatedBy);
            return new MSSQLFactory().ExecuteNonQuery(cmd);
        }
        public ADMEntranceLists GetItem(List<Guid> Course_IDs, EntranceListType entranceListType)
        {
            return new MSSQLFactory().GetObject<ADMEntranceLists>($"SELECT * FROM ADMEntranceLists WHERE Course_ID IN ({Course_IDs.ToIN()}) AND ListType={(short)entranceListType}");
        }
        public ADMEntranceLists GetItem(List<Guid> Course_IDs)
        {
            return new MSSQLFactory().GetObject<ADMEntranceLists>($"SELECT * FROM ADMEntranceLists WHERE Course_ID IN ({Course_IDs.ToIN()})");
        }

        public List<SelectListItem> GetAllCoursesByProgramme(Programme programmeId)
        {
            SqlCommand command = new SqlCommand();
            command.CommandText = $@"SELECT convert(nvarchar(50),[Course_ID]) as [Value],[CourseFullName] as [Text] from [ADMCourseMaster]
                                     WHERE Status=1 AND Programme = @ProgrammeId AND MinCombinationSubjects>0 ";

            if(programmeId==Programme.IG || programmeId==Programme.HS || programmeId == Programme.Professional)
            {
                command.CommandText += @" Union
                                        SELECT convert(nvarchar(50),[Course_ID]) as [Value],[CourseFullName] as [Text] from [ADMCourseMaster]
                                        WHERE Course_ID='1A4A791F-FE3A-4668-A18B-C71403FD258B' ";
            }
            command.CommandText += " order by [CourseFullName]";
            command.Parameters.AddWithValue("@ProgrammeId", (int)programmeId);
            return new MSSQLFactory().GetObjectList<SelectListItem>(command);
        }
    }
}
