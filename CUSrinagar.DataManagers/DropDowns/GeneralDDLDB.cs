using CUSrinagar.DataManagers.SQLQueries;
using CUSrinagar.Models;
using GeneralModels;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Terex;


namespace CUSrinagar.DataManagers
{
    public class GeneralDDLDB
    {
        public int SavePushMessageSubscribers(PushMessageSubscribers pushMessageSubscribers)
        {
            return new MSSQLFactory().InsertRecord(pushMessageSubscribers);
        }
        public List<string> GetPushMessageSubscribers(int offSet)
        {
            return new MSSQLFactory().GetSingleValues<string>($"SELECT DeviceToken FROM PushMessageSubscribers ORDER BY CreatedOn DESC OFFSET {offSet} ROWS FETCH NEXT 1000 ROWS ONLY;")?? new List<string>();
        }


        public List<DropDown> GetCourseDDL(bool withHasCombination)
        {
            // todo (CourseCode+' - '+CourseFullName)
            string Qurery = "SELECT Course_ID AS [Value],CourseFullName AS [Text] FROM ADMCourseMaster WHERE [Status]=1 ORDER BY CourseFullName";
            if (withHasCombination)
                Qurery = "SELECT Course_ID AS [Value],CourseFullName AS [Text] FROM ADMCourseMaster WHERE [Status]=1 and HasCombination=1 ORDER BY CourseFullName";
            return new MSSQLFactory().GetObjectList<DropDown>(Qurery);
        }
        public List<DropDownOptLabel> GetCourseOptLabelDDL(bool withHasCombination,Guid College_ID)
        {
            // todo (CourseCode+' - '+CourseFullName)
            string Qurery = $@"SELECT ADMCollegeCourseMapping.Course_ID AS [Value],
                                   ADMCourseMaster.CourseFullName AS [Text],
                                   ADMCourseMaster.Programme AS DataGroupFieldEnum
                            FROM ADMCollegeCourseMapping JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = ADMCollegeCourseMapping.Course_ID
                            WHERE ADMCollegeCourseMapping.Status = 1 AND ADMCourseMaster.Status=1 AND ADMCollegeCourseMapping.College_ID='{College_ID}'";
            if (withHasCombination)
                Qurery += " AND HasCombination=1 ";
            Qurery += " ORDER BY CourseFullName;";
            return new MSSQLFactory().GetObjectList<DropDownOptLabel>(Qurery);
        }
        public List<DropDownOptLabel> GetCourseOptLabelDDL(bool withHasCombination)
        {
            // todo (CourseCode+' - '+CourseFullName)
            string Qurery = $@"SELECT ADMCollegeCourseMapping.Course_ID AS [Value],
                                   ADMCourseMaster.CourseFullName AS [Text],
                                   ADMCourseMaster.Programme AS DataGroupFieldEnum
                            FROM ADMCollegeCourseMapping JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = ADMCollegeCourseMapping.Course_ID
                            WHERE ADMCollegeCourseMapping.Status = 1 AND ADMCourseMaster.Status=1 ";
            if (withHasCombination)
                Qurery += " AND HasCombination=1 ";
            Qurery += " ORDER BY CourseFullName ASC;";
            return new MSSQLFactory().GetObjectList<DropDownOptLabel>(Qurery);
        }
        public List<DropDownOptLabelGeneral> GetDistrictList()
        {
            string Qurery = @"SELECT DistrictName+'|'+s.StateName AS [Value],[DistrictName] AS [Text],s.StateName AS DataGroupField FROM [Districts]
                            JOIN States s ON s.StateId = Districts.StateId WHERE Districts.StateId=15";
            return new MSSQLFactory().GetObjectList<DropDownOptLabelGeneral>(Qurery);
        }
        public List<DropDownOptLabelGeneral> GetAssemblyList()
        {
            string Qurery = @"SELECT AssemblyName AS Text,AssemblyName+'|'+ParliamentaryConstituency AS Value,ParliamentaryConstituency AS DataGroupField FROM AssemblyMaster ORDER BY ParliamentaryConstituency ASC";
            return new MSSQLFactory().GetObjectList<DropDownOptLabelGeneral>(Qurery);
        }

        public List<DropDownOptLabelGeneral> GetDistrictListOnly()
        {
            string Qurery = @"SELECT DistrictName AS [Value],[DistrictName] AS [Text],s.StateName AS DataGroupField FROM [Districts]
                            JOIN States s ON s.StateId = Districts.StateId WHERE Districts.StateId=15";
            return new MSSQLFactory().GetObjectList<DropDownOptLabelGeneral>(Qurery);
        }
    }
}
