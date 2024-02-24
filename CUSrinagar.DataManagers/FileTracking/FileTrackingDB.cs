using CUSrinagar.Enums;
using CUSrinagar.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using CUSrinagar.Extensions;
using Terex;
using System.Data;
using System.Web.Mvc;

namespace CUSrinagar.DataManagers
{
    public class FileTrackingDB
    {
        public int CreateNewFile(FTFileTracking fileTracking)
        {
            try
            {
                return new MSSQLFactory().InsertRecord(fileTracking);
            }
            catch (SqlException SQLError) when (SQLError.Number == 2627)
            {
                return -2;
            }
        }
        public int CreateFileHistory(FTFileTrackingHistory fileTrackingHistory)
        {
            return new MSSQLFactory().InsertRecord(fileTrackingHistory);
        }

        public List<FTFileTracking> GetFiles(FTFilters filters)
        {
            string TSQL = $@"SELECT FTFileTracking.File_ID,
                               FTFileTracking.FileNo,
                               FTFileTracking.Subject,
                               COALESCE(Remarks,Description) AS Description,
                               COALESCE(FTFileTrackingHistory.Section,FTFileTracking.Section) AS Section,
                               FTFileTracking.CurrentlyWithUser_ID,FTFileTrackingHistory.Dated AS CreatedOn FROM FTFileTracking 
                            LEFT JOIN FTFileTrackingHistory ON FTFileTrackingHistory.File_ID = FTFileTracking.File_ID AND FileStatus={(short)FTFileStatus.Created}
                            WHERE CurrentlyWithUser_ID='{AppUserHelper.User_ID}'";

            if (filters == null)
            {
                filters = new FTFilters();
            }
            if (!string.IsNullOrWhiteSpace(filters.SearchQuery))
            {
                filters.SearchQuery = filters.SearchQuery.Replace("'", "").Replace("--", "").Replace(";", "");
                TSQL += $@" AND (FileNo LIKE '%{filters.SearchQuery}%' OR Subject LIKE '%{filters.SearchQuery}%' OR FTFileTracking.Section LIKE '%{filters.SearchQuery}%' OR FTFileTrackingHistory.Section LIKE '%{filters.SearchQuery}%')";
            }

            if (filters.From != null && filters.From != DateTime.MinValue)
            {
                if (filters.To == null || filters?.To == DateTime.MinValue)
                {
                    filters.To = filters.From;
                }
            }

            if (filters.To != null && filters.To != DateTime.MinValue)
            {
                if (filters.From == null || filters?.From == DateTime.MinValue)
                {
                    filters.From = filters.To;
                }
            }

            if (filters.To != null && filters.To != DateTime.MinValue)
            {
                TSQL += $@" AND CAST(FTFileTrackingHistory.Dated AS DATE) BETWEEN '{((DateTime)filters.From).ToString("yyyy-MM-dd")}' AND '{((DateTime)filters.To).ToString("yyyy-MM-dd")}'";
            }
            TSQL += $@" ORDER BY FTFileTrackingHistory.Dated DESC";
            return new MSSQLFactory().GetObjectList<FTFileTracking>(TSQL);
        }

        public FTFileStatus GetCurrentFileStatus(Guid File_ID)
        {
            short val = new MSSQLFactory().ExecuteScalar<short>($"SELECT TOP 1 FileStatus FROM FTFileTrackingHistory WHERE File_ID='{File_ID}' ORDER BY Dated DESC;");
            return (FTFileStatus)Enum.Parse(typeof(FTFileStatus), val.ToString());
        }

        public List<FTFileTracking> GetForwardedFiles(FTFilters filters)
        {
            string TSQL = $@"WITH FileTrackingForwardedHistory AS
                            (
	                            SELECT *,ROW_NUMBER() OVER (PARTITION BY File_ID ORDER BY Dated DESC) RowNumber FROM FTFileTrackingHistory	
	                            WHERE FileStatus={(short)FTFileStatus.Forwarded} #B#
	                            AND User_ID='{AppUserHelper.User_ID}'
                            )
                            SELECT FileTrackingForwardedHistory.File_ID,
                               FileTrackingForwardedHistory.FileStatus AS CurrentFileStatus,
                               FileTrackingForwardedHistory.Dated AS CreatedOn,
                               COALESCE(FileTrackingForwardedHistory.Remarks,FTFileTracking.Description) AS Description,
                               FileTrackingForwardedHistory.Section,
                               FileNo,
                               Subject,
                               CurrentlyWithUser_ID,
	                        FullName+' ('+Designation+')' AS UserID FROM FileTrackingForwardedHistory 
                            JOIN FTFileTracking ON FTFileTracking.File_ID = FileTrackingForwardedHistory.File_ID
                            JOIN AppUsers ON AppUsers.User_ID = CurrentlyWithUser_ID
                            WHERE FileTrackingForwardedHistory.RowNumber=1 #L# ORDER BY FileTrackingForwardedHistory.Dated DESC";
            if (filters != null)
            {
                if (!string.IsNullOrWhiteSpace(filters.SearchQuery))
                {
                    filters.SearchQuery = filters.SearchQuery.Replace("'", "").Replace("--", "").Replace(";", "");
                    TSQL = TSQL.Replace("#L#", $@" AND (FileNo LIKE '%{filters.SearchQuery}%' OR Remarks LIKE '%{filters.SearchQuery}%' 
                                    OR FileTrackingForwardedHistory.Section LIKE '%{filters.SearchQuery}%' OR Subject LIKE '%{filters.SearchQuery}%')");
                }
                else
                {
                    TSQL = TSQL.Replace("#L#", "");
                }

                if (filters.From != null && filters.From != DateTime.MinValue)
                {
                    if (filters.To == null || filters?.To == DateTime.MinValue)
                    {
                        filters.To = filters.From;
                    }
                }

                if (filters.To != null && filters.To != DateTime.MinValue)
                {
                    if (filters.From == null || filters?.From == DateTime.MinValue)
                    {
                        filters.From = filters.To;
                    }
                }

                if (filters.To != null && filters.To != DateTime.MinValue)
                {
                    TSQL = TSQL.Replace("#B#", $@" AND CAST(Dated AS DATE) BETWEEN '{((DateTime)filters.From).ToString("yyyy-MM-dd")}' AND '{((DateTime)filters.To).ToString("yyyy-MM-dd")}'");
                }
                else
                {
                    TSQL = TSQL.Replace("#B#", "");
                }
            }
            else
            {
                TSQL = TSQL.Replace("#L#", "").Replace("#B#", "");
            }
            return new MSSQLFactory().GetObjectList<FTFileTracking>(TSQL);
        }

        public int Delete(Guid File_ID)
        {
            string TSQL = $@"DELETE FROM FTFileTracking WHERE File_ID='{File_ID}';
                    DELETE FROM FTFileTrackingHistory WHERE File_ID='{File_ID}';";
            return new MSSQLFactory().ExecuteNonQuery(TSQL);
        }

        public bool IsItWithCurrentUser(Guid File_ID, Guid User_ID)
        {
            return new MSSQLFactory().ExecuteScalar<int>($"SELECT COUNT(File_ID) FROM FTFileTracking WHERE File_ID='{File_ID}' AND CurrentlyWithUser_ID='{User_ID}'") > 0;
        }

        public List<SelectListItem> GetFileTrackingActiveUsers()
        {
            return new MSSQLFactory().GetObjectList<SelectListItem>($@"SELECT CAST(AppUserRoles.User_ID AS VARCHAR(50)) AS Value,FullName+' ('+DepartmentName+')' AS Text FROM AppUsers
                                                                        JOIN AppUserRoles ON AppUserRoles.User_ID = AppUsers.User_ID
                                                                        JOIN FTUserSectionMapping ON FTUserSectionMapping.User_ID = AppUsers.User_ID
                                                                        JOIN EmployeeDepartment ON EmployeeDepartment.Department_ID = FTUserSectionMapping.Department_ID
                                                                        WHERE RoleID={(short)AppRoles.University_FileTracking} AND Status=1 AND AppUsers.User_ID<>'{AppUserHelper.User_ID}' ORDER BY FullName ASC");
        }

        public Guid GetForwardedUser_ID(Guid File_ID)
        {
            return new MSSQLFactory().ExecuteScalar<Guid>($@"SELECT TOP 1 User_ID FROM FTFileTrackingHistory WHERE File_ID='{File_ID}' ORDER BY Dated DESC;");
        }

        public int UpdateFile(Guid File_ID, Guid User_ID)
        {
            return new MSSQLFactory().ExecuteNonQuery($@"UPDATE FTFileTracking SET CurrentlyWithUser_ID='{User_ID}' WHERE File_ID='{File_ID}';");
        }

        public FTFileTracking GetFile(Guid File_ID)
        {
            return new MSSQLFactory().GetObject<FTFileTracking>($@"SELECT FTFileTracking.*,FullName+ ' ('+DepartmentName +')' AS UserID FROM FTFileTracking
                                                                    JOIN AppUsers ON CurrentlyWithUser_ID = AppUsers.User_ID 
                                                                    JOIN FTUserSectionMapping ON FTUserSectionMapping.User_ID = AppUsers.User_ID
                                                                    JOIN EmployeeDepartment ON EmployeeDepartment.Department_ID = FTUserSectionMapping.Department_ID 
                                                                    WHERE File_ID='{File_ID}';");
        }
        public List<FTFileTrackingHistory> GetFileHistory(Guid File_ID)
        {
            return new MSSQLFactory().GetObjectList<FTFileTrackingHistory>($@"SELECT FTFileTrackingHistory.*,FullName+ ' ('+Designation +')' AS UserID FROM FTFileTrackingHistory
                                                                            JOIN AppUsers ON FTFileTrackingHistory.User_ID = AppUsers.User_ID WHERE File_ID='{File_ID}' ORDER BY Dated ASC;");
        }

        public List<FTFileTracking> GetTrackAllYourFiles(FTFilters filters)
        {
            string FilterSQL = "";
            string TSQL = $@"SELECT * FROM (SELECT DISTINCT FTFileTracking.File_ID,
                                   FTFileTracking.FileNo,
                                   FTFileTracking.Subject,
                                   FTFileTracking.Description,
                                   FTFileTracking.Section,
                                   FTFileTracking.CurrentlyWithUser_ID,FullName+ ' ('+Designation +')' AS UserID,
	                               {(short)FTFileStatus.TrackAllYourFiles} AS CurrentFileStatus,FTFileTrackingHistory.Dated AS CreatedOn FROM FTFileTracking
                            JOIN FTFileTrackingHistory ON FTFileTrackingHistory.File_ID = FTFileTracking.File_ID
                            JOIN AppUsers ON AppUsers.User_ID = CurrentlyWithUser_ID
                            WHERE CurrentlyWithUser_ID <>'{AppUserHelper.User_ID}'
                            AND FTFileTrackingHistory.USER_ID ='{AppUserHelper.User_ID}' AND FileStatus<>{(short)FTFileStatus.Forwarded} ##) t
                            ORDER BY t.CreatedOn DESC";

            if (filters == null)
            {
                filters = new FTFilters();
            }
            if (!string.IsNullOrWhiteSpace(filters.SearchQuery))
            {
                filters.SearchQuery = filters.SearchQuery.Replace("'", "").Replace("--", "").Replace(";", "");
                FilterSQL = $@" AND (FileNo LIKE '%{filters.SearchQuery}%' OR Subject LIKE '%{filters.SearchQuery}%' OR FTFileTracking.Section LIKE '%{filters.SearchQuery}%' OR FTFileTrackingHistory.Section LIKE '%{filters.SearchQuery}%')";
            }

            if (filters.From != null && filters.From != DateTime.MinValue)
            {
                if (filters.To == null || filters?.To == DateTime.MinValue)
                {
                    filters.To = filters.From;
                }
            }

            if (filters.To != null && filters.To != DateTime.MinValue)
            {
                if (filters.From == null || filters?.From == DateTime.MinValue)
                {
                    filters.From = filters.To;
                }
            }

            if (filters.To != null && filters.To != DateTime.MinValue)
            {
                FilterSQL += $@" AND CAST(FTFileTrackingHistory.Dated AS DATE) BETWEEN '{((DateTime)filters.From).ToString("yyyy-MM-dd")}' AND '{((DateTime)filters.To).ToString("yyyy-MM-dd")}'";
            }
            TSQL = TSQL.Replace("##", FilterSQL);
            return new MSSQLFactory().GetObjectList<FTFileTracking>(TSQL);
        }

        public string GetUserSection(Guid User_ID)
        {
            return new MSSQLFactory().ExecuteScalar<string>($@"SELECT TOP 1 DepartmentName FROM FTUserSectionMapping
                                                    JOIN EmployeeDepartment ON EmployeeDepartment.Department_ID = FTUserSectionMapping.Department_ID
                                                    WHERE FTUserSectionMapping.User_ID='{User_ID}'");
        }
    }
}
