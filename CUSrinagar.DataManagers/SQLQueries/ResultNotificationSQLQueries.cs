using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CUSrinagar.Models;
using GeneralModels;
using CUSrinagar.Enums;

namespace CUSrinagar.DataManagers.SQLQueries
{
    public class ResultNotificationSQLQueries
    {
        internal string GetListQuery()
        {
            return $@"SELECT * FROM ResultNotification";
        }
        internal string GetItemQuery(Guid _ID)
        {
            return $@"SELECT * FROM ResultNotification Where ResultNotification_ID='{_ID}'";
        }


        internal string CheckAnyBacklog(Guid id, String Searchtext,PrintProgramme printProgramme, short semester)
        {
            return $@"SELECT RN.Remark FROM dbo.ResultNotification RN JOIN dbo.{printProgramme}_Semester{semester} R
                       ON R.ResultNotification_ID = RN.ResultNotification_ID
                        JOIN dbo.VWStudentWithDetail VW ON VW.Student_ID = R.Student_ID AND VW.Subject_ID = R.Subject_ID 
                        WHERE ParentNotification_ID='{id}' AND (CUSRegistrationNo='{Searchtext}' OR VW.ExamRollNumber ='{Searchtext}')";
        }


    }
}
