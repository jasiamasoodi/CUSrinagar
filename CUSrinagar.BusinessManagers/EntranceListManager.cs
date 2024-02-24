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
using CUSrinagar.DataManagers;
using System.IO;
using System.Web;

namespace CUSrinagar.BusinessManagers
{
    public class EntranceListManager
    {
        public List<ADMEntranceLists> GetList()
        {
            return new EntranceListDB().GetList();
        }

        public int Delete(Guid Id, string FileName)
        {
            int result = new EntranceListDB().Delete(Id);
            if (result > 0)
            {
                if (File.Exists(HttpContext.Current.Server.MapPath(ADMEntranceLists.DirectoryPath + "/" + FileName)))
                {
                    File.Delete(HttpContext.Current.Server.MapPath(ADMEntranceLists.DirectoryPath + "/" + FileName));
                }
            }
            return result;
        }
        public int DeleteAll()
        {
            int result = new EntranceListDB().DeleteAll();
            if (result > 0)
            {
                Directory.Delete(HttpContext.Current.Server.MapPath(ADMEntranceLists.DirectoryPath), true);
                Directory.CreateDirectory(HttpContext.Current.Server.MapPath(ADMEntranceLists.DirectoryPath));
            }
            return result;
        }

        public int UploadAndSave(ADMEntranceLists entranceLists)
        {
            if (entranceLists == null)
                return 0;
            Tuple<string, List<Guid[]>> CourseGrouping = new EntranceCentersDB().GetCourseGrouping(GeneralFunctions.ProgrammeToPrintProgrammeMapping(entranceLists.Programme));

            List<Guid> Course_IDs = new List<Guid> { entranceLists.Course_ID };
            if (CourseGrouping?.Item2?.Any(x => x.Contains(entranceLists.Course_ID)) ?? false)
            {
                Course_IDs = CourseGrouping.Item2.Find(x => (x[0] == entranceLists.Course_ID || x[1] == entranceLists.Course_ID)).ToList();
            }

            EntranceListDB entranceListDB = new EntranceListDB();
            ADMEntranceLists EntranceList = entranceListDB.GetItem(Course_IDs, entranceLists.ListType);

            int result = 0;
            string fileName = DateTime.Now.Ticks + "_" + Path.GetFileName(entranceLists.File.FileName);
            if (EntranceList != null)
            {
                //Update
                entranceLists.EntranceList_ID = EntranceList.EntranceList_ID;
                entranceLists.SetWorkFlow(RecordState.Old);
                entranceLists.ListURL = fileName;
                result = entranceListDB.Update(entranceLists);

                if (result > 0)
                {
                    //delete old
                    if (File.Exists(HttpContext.Current.Server.MapPath(ADMEntranceLists.DirectoryPath + "/" + EntranceList.ListURL)))
                    {
                        File.Delete(HttpContext.Current.Server.MapPath(ADMEntranceLists.DirectoryPath + "/" + EntranceList.ListURL));
                    }
                }
            }
            else
            {
                //Insert
                EntranceList = entranceListDB.GetItem(Course_IDs);
                if (EntranceList != null)
                    entranceLists.Course_ID = EntranceList.Course_ID;

                entranceLists.EntranceList_ID = Guid.NewGuid();
                entranceLists.SetWorkFlow(RecordState.New);
                entranceLists.ListURL = fileName;
                result = entranceListDB.Save(entranceLists);

                #region Push Notification
                if (result > 0)
                {
                    PushNotification pushNotification = new PushNotification
                    {
                        notification = new BasePushNotification
                        {
                            title = $"Cluster University Srinagar",
                            body = $"{entranceLists.ListType.GetEnumDescription()} - Regarding Entrance {DateTime.UtcNow.Year}"
                        }
                    };
                    pushNotification.notification.click_action = "http://www.cusrinagar.edu.in/Notification/EntranceExamNotification";

                    new PushNotificationManager().SendAsync(pushNotification);
                }
                #endregion

            }
            if (result > 0)
            {
                //upload new
                entranceLists.File.SaveAs(HttpContext.Current.Server.MapPath(ADMEntranceLists.DirectoryPath + "/" + fileName));
            }

            return result;
        }

        public List<SelectListItem> GetAllCoursesByProgramme(Programme programmeId)
        {
            return new EntranceListDB().GetAllCoursesByProgramme(programmeId);
        }
    }
}
