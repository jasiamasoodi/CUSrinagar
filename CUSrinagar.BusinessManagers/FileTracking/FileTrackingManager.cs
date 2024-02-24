using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using CUSrinagar.Models;
using System.Data.SqlClient;
using CUSrinagar.DataManagers;
using System.Web.Mvc;
using System.Transactions;
using System.Text.RegularExpressions;
using System.Configuration;
using Newtonsoft.Json;
using System.Data;
using System.Web;
using System.IO;

namespace CUSrinagar.BusinessManagers
{
    public class FileTrackingManager
    {
        public List<SelectListItem> GetFileTrackingActiveUsers()
        {
            return new FileTrackingDB().GetFileTrackingActiveUsers() ?? new List<SelectListItem>();
        }
        public int CreateNewFile(FTFileTracking fileTracking)
        {
            if (fileTracking == null)
                return 0;

            int result = 0;
            FileTrackingDB _FileTrackingDB = new FileTrackingDB();

            using (TransactionScope ts = new TransactionScope())
            {
                fileTracking.File_ID = Guid.NewGuid();
                fileTracking.CurrentlyWithUser_ID = AppUserHelper.User_ID;
                fileTracking.Description = string.IsNullOrWhiteSpace(fileTracking.Description) ? null : fileTracking.Description;
                fileTracking.Section = _FileTrackingDB.GetUserSection(fileTracking.CurrentlyWithUser_ID);

                if (!string.IsNullOrWhiteSpace(fileTracking.Section))
                {
                    result = _FileTrackingDB.CreateNewFile(fileTracking);
                    if (result > 0)
                    {
                        FTFileTrackingHistory fileTrackingHistory = new FTFileTrackingHistory
                        {
                            Tracking_ID = Guid.NewGuid(),
                            Section = fileTracking.Section,
                            FileStatus = FTFileStatus.Created,
                            Dated = DateTime.Now,
                            File_ID = fileTracking.File_ID,
                            User_ID = fileTracking.CurrentlyWithUser_ID
                        };
                        _FileTrackingDB.CreateFileHistory(fileTrackingHistory);
                    }
                }
                ts.Complete();
            }
            return result;
        }
        public List<FTFileTracking> GetFilesCompact(FTFilters filter)
        {
            if (filter != null && filter.SearchQuery == "addednew")
                filter = null;

            FileTrackingDB fileTrackingDB = new FileTrackingDB();
            List<FTFileTracking> fileTrackings = fileTrackingDB.GetFiles(filter);

            if (fileTrackings == null)
                fileTrackings = new List<FTFileTracking>();

            fileTrackings.AddRange(fileTrackingDB.GetForwardedFiles(filter) ?? new List<FTFileTracking>());

            List<FTFileTracking> fileTrackingsDistinct = fileTrackings.DistinctBy(x => x.File_ID).ToList();

            foreach (FTFileTracking item in fileTrackingsDistinct ?? new List<FTFileTracking>())
            {
                item.CurrentFileStatus = fileTrackingDB.GetCurrentFileStatus(item.File_ID);
            }
            fileTrackingsDistinct.AddRange((fileTrackingDB.GetTrackAllYourFiles(filter) ?? new List<FTFileTracking>()).DistinctBy(x=>x.File_ID));

            return fileTrackingsDistinct.IsNotNullOrEmpty() ? fileTrackingsDistinct : null;
        }

        public Task<int> DeleteAync(Guid File_ID)
        {
            return new TaskFactory().StartNew(() => Delete(File_ID));
        }

        public int Delete(Guid File_ID)
        {
            if (File_ID == Guid.Empty)
                return 0;

            FileTrackingDB fileTrackingDB = new FileTrackingDB();
            if (fileTrackingDB.GetCurrentFileStatus(File_ID) != FTFileStatus.Created)
                return 0;

            int result = 0;
            using (TransactionScope ts = new TransactionScope())
            {
                result = fileTrackingDB.Delete(File_ID);
                ts.Complete();
            }
            return result;
        }


        public Task<int> CloseFileAsync(FTFileTrackingHistory fileTrackingHistory)
        {
            if (fileTrackingHistory == null || fileTrackingHistory?.File_ID == Guid.Empty)
                return Task.FromResult(0);

            fileTrackingHistory.User_ID = AppUserHelper.User_ID;

            return new TaskFactory().StartNew(() => CloseFile(fileTrackingHistory));
        }
        public int CloseFile(FTFileTrackingHistory fileTrackingHistory)
        {
            if (fileTrackingHistory == null || fileTrackingHistory?.File_ID == Guid.Empty)
                return 0;

            FileTrackingDB fileTrackingDB = new FileTrackingDB();
            FTFileStatus fTFileStatus = fileTrackingDB.GetCurrentFileStatus(fileTrackingHistory.File_ID);

            if (fTFileStatus == FTFileStatus.Forwarded || fTFileStatus == FTFileStatus.Closed || !fileTrackingDB.IsItWithCurrentUser(fileTrackingHistory.File_ID, fileTrackingHistory.User_ID))
                return 0;

            int result = 0;
            fileTrackingHistory.Tracking_ID = Guid.NewGuid();
            fileTrackingHistory.Dated = DateTime.Now;
            fileTrackingHistory.FileStatus = FTFileStatus.Closed;
            fileTrackingHistory.Section = fileTrackingDB.GetUserSection(fileTrackingHistory.User_ID);

            if (string.IsNullOrWhiteSpace(fileTrackingHistory.Section))
                return 0;

            using (TransactionScope ts = new TransactionScope())
            {
                result = fileTrackingDB.CreateFileHistory(fileTrackingHistory);
                ts.Complete();
            }
            return result;

        }


        public Task<int> RevertFileAsync(FTFileTrackingHistory fileTrackingHistory)
        {
            if (fileTrackingHistory == null || fileTrackingHistory?.File_ID == Guid.Empty)
                return Task.FromResult(0);

            fileTrackingHistory.User_ID = AppUserHelper.User_ID;

            return new TaskFactory().StartNew(() => RevertFile(fileTrackingHistory));
        }
        public int RevertFile(FTFileTrackingHistory fileTrackingHistory)
        {
            if (fileTrackingHistory == null || fileTrackingHistory?.File_ID == Guid.Empty)
                return 0;

            FileTrackingDB fileTrackingDB = new FileTrackingDB();
            FTFileStatus fTFileStatus = fileTrackingDB.GetCurrentFileStatus(fileTrackingHistory.File_ID);

            if (fTFileStatus != FTFileStatus.Forwarded || !fileTrackingDB.IsItWithCurrentUser(fileTrackingHistory.File_ID, fileTrackingHistory.User_ID))
                return 0;

            int result = 0;
            fileTrackingHistory.Tracking_ID = Guid.NewGuid();
            fileTrackingHistory.Dated = DateTime.Now;
            fileTrackingHistory.FileStatus = FTFileStatus.Reverted;
            fileTrackingHistory.Section = fileTrackingDB.GetUserSection(fileTrackingHistory.User_ID);

            if (string.IsNullOrWhiteSpace(fileTrackingHistory.Section))
                return 0;

            using (TransactionScope ts = new TransactionScope())
            {
                result = fileTrackingDB.CreateFileHistory(fileTrackingHistory);
                ts.Complete();
            }
            return result;

        }


        public Task<int> ReOpenFileAsync(FTFileTrackingHistory fileTrackingHistory)
        {
            if (fileTrackingHistory == null || fileTrackingHistory?.File_ID == Guid.Empty)
                return Task.FromResult(0);

            fileTrackingHistory.User_ID = AppUserHelper.User_ID;

            return new TaskFactory().StartNew(() => ReOpenFile(fileTrackingHistory));
        }
        public int ReOpenFile(FTFileTrackingHistory fileTrackingHistory)
        {
            if (fileTrackingHistory == null || fileTrackingHistory?.File_ID == Guid.Empty)
                return 0;

            FileTrackingDB fileTrackingDB = new FileTrackingDB();
            FTFileStatus fTFileStatus = fileTrackingDB.GetCurrentFileStatus(fileTrackingHistory.File_ID);

            if (fTFileStatus != FTFileStatus.Closed || !fileTrackingDB.IsItWithCurrentUser(fileTrackingHistory.File_ID, fileTrackingHistory.User_ID))
                return 0;

            int result = 0;
            fileTrackingHistory.Tracking_ID = Guid.NewGuid();
            fileTrackingHistory.Dated = DateTime.Now;
            fileTrackingHistory.FileStatus = FTFileStatus.ReOpened;
            fileTrackingHistory.Section = fileTrackingDB.GetUserSection(fileTrackingHistory.User_ID);

            if (string.IsNullOrWhiteSpace(fileTrackingHistory.Section))
                return 0;

            using (TransactionScope ts = new TransactionScope())
            {
                result = fileTrackingDB.CreateFileHistory(fileTrackingHistory);
                ts.Complete();
            }
            return result;

        }


        public Task<int> ForwardFileAsync(FTFileTrackingHistory fileTrackingHistory)
        {
            if (fileTrackingHistory == null || fileTrackingHistory?.File_ID == Guid.Empty)
                return Task.FromResult(0);

            Guid CurrentUser_ID = AppUserHelper.User_ID;

            return new TaskFactory().StartNew(() => ForwardFile(fileTrackingHistory, CurrentUser_ID));
        }
        public int ForwardFile(FTFileTrackingHistory fileTrackingHistory, Guid CurrentUser_ID)
        {
            if (fileTrackingHistory == null || fileTrackingHistory?.File_ID == Guid.Empty)
                return 0;

            FileTrackingDB fileTrackingDB = new FileTrackingDB();
            FTFileStatus fTFileStatus = fileTrackingDB.GetCurrentFileStatus(fileTrackingHistory.File_ID);

            if (fTFileStatus == FTFileStatus.Forwarded || fTFileStatus == FTFileStatus.Closed
                || !fileTrackingDB.IsItWithCurrentUser(fileTrackingHistory.File_ID, CurrentUser_ID) || CurrentUser_ID == fileTrackingHistory.User_ID)
                return 0;

            int result = 0;
            fileTrackingHistory.Tracking_ID = Guid.NewGuid();
            fileTrackingHistory.Dated = DateTime.Now;
            fileTrackingHistory.FileStatus = FTFileStatus.Forwarded;
            fileTrackingHistory.Section = fileTrackingDB.GetUserSection(CurrentUser_ID);

            if (string.IsNullOrWhiteSpace(fileTrackingHistory.Section))
                return 0;

            using (TransactionScope ts = new TransactionScope())
            {
                result = fileTrackingDB.CreateFileHistory(fileTrackingHistory);
                ts.Complete();
            }
            return result;

        }


        public Task<int> AcceptFileAsync(FTFileTrackingHistory fileTrackingHistory)
        {
            if (fileTrackingHistory == null || fileTrackingHistory?.File_ID == Guid.Empty)
                return Task.FromResult(0);

            fileTrackingHistory.User_ID = AppUserHelper.User_ID;

            return new TaskFactory().StartNew(() => AcceptFile(fileTrackingHistory));
        }
        public int AcceptFile(FTFileTrackingHistory fileTrackingHistory)
        {
            if (fileTrackingHistory == null || fileTrackingHistory?.File_ID == Guid.Empty)
                return 0;

            FileTrackingDB fileTrackingDB = new FileTrackingDB();
            FTFileStatus fTFileStatus = fileTrackingDB.GetCurrentFileStatus(fileTrackingHistory.File_ID);
            Guid ForwardedUser_ID = fileTrackingDB.GetForwardedUser_ID(fileTrackingHistory.File_ID);

            if (fTFileStatus != FTFileStatus.Forwarded || ForwardedUser_ID != fileTrackingHistory.User_ID)
                return 0;

            int result = 0;
            fileTrackingHistory.Tracking_ID = Guid.NewGuid();
            fileTrackingHistory.Dated = DateTime.Now;
            fileTrackingHistory.FileStatus = FTFileStatus.Received;
            fileTrackingHistory.Section = fileTrackingDB.GetUserSection(fileTrackingHistory.User_ID);

            if (string.IsNullOrWhiteSpace(fileTrackingHistory.Section))
                return 0;


            using (TransactionScope ts = new TransactionScope())
            {
                result = fileTrackingDB.CreateFileHistory(fileTrackingHistory);
                if (result > 0)
                {
                    fileTrackingDB.UpdateFile(fileTrackingHistory.File_ID, fileTrackingHistory.User_ID);
                }
                ts.Complete();
            }
            return result;

        }


        public Task<FTFileTracking> GetFileAsync(Guid File_ID)
        {
            if (File_ID == Guid.Empty)
                return Task.FromResult(new FTFileTracking());

            return new TaskFactory().StartNew(() => GetFile(File_ID));
        }
        public FTFileTracking GetFile(Guid File_ID)
        {
            if (File_ID == Guid.Empty)
                return null;

            FTFileTracking fTFileTracking = new FileTrackingDB().GetFile(File_ID);
            if (fTFileTracking != null)
            {
                fTFileTracking.FileTrackingHistoryDetails = new FileTrackingDB().GetFileHistory(File_ID);
            }
            return fTFileTracking;
        }
    }
}
