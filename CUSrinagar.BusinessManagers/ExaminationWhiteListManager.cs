using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using CUSrinagar.DataManagers;
using CUSrinagar.Models;
using GeneralModels;

namespace CUSrinagar.BusinessManagers
{
    public class ExaminationWhiteListManager
    {
        public List<ExaminationWhiteList> GetAllApplications(Parameters parameters)
        {
            return new ExaminationWhiteListDB().GetAllApplications(parameters);
        }

        public int SaveApplication(ExaminationWhiteListCompact application)
        {
            if (application.Student_ID == Guid.Empty)
                application.Student_ID = new StudentManager().GetStudentCompactByRegistrationNo(application.CUSRegistrationNo + "")?.Student_ID ?? Guid.Empty;
            if (application.Student_ID == Guid.Empty || application.Semester <= 0 || application.ExaminationYear <= 2016 || application.AllowedBy_ID == Guid.Empty)
                return -1;

            application.WhiteList_ID = Guid.NewGuid();
            application.SetWorkFlow(Enums.RecordState.New);

            if (application.Files != null)
            {
                string input_filename = application.Files.FileName;
                string new_filename = Regex.Replace(input_filename, @"[^0-9a-zA-Z.]+", "");
                new_filename = new_filename.Trim();

                string path = HttpRuntime.AppDomainAppPath + "WhiteListApplications\\" + new_filename;

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                path = HttpRuntime.AppDomainAppPath + "FolderManager\\WhiteListApplications";

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                string filePath = "";
                string fileExtension = application.Files.FileName.Substring(application.Files.FileName.LastIndexOf('.'));

                if (application.CUSRegistrationNo != null)
                    filePath = "/FolderManager/WhiteListApplications/" + application.CUSRegistrationNo + "_" + DateTime.Now.Date.ToString("dd-MM-yyyy") + fileExtension;
                else
                    filePath = "/FolderManager/WhiteListApplications/" + application.Student_ID + "_" + DateTime.Now.Date.ToString("dd-MM-yyyy") + fileExtension;

                application.ApplicationPath = filePath;
                application.Files.SaveAs(HttpRuntime.AppDomainAppPath + filePath);

            }

            return new ExaminationWhiteListDB().SaveApplication(application);
        }

        public int EditApplication(ExaminationWhiteListCompact application)
        {
            return new ExaminationWhiteListDB().EditApplication(application);
        }

        public int DeleteApplication(Guid _ID)
        {
            return new ExaminationWhiteListDB().DeleteApplication(_ID);
        }
    }
}
