using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CUSrinagar.Models;
using Terex;
using CUSrinagar.DataManagers.SQLQueries;
using GeneralModels;
using CUSrinagar.Enums;

namespace CUSrinagar.DataManagers
{
    public class GrievanceDB
    {
        public List<GrievanceList> GetGrievanceListCompact(Parameters Parameter)
        {
            var Query = new GrievanceSQLQueries().GetGrievanceLisQuerytCompact();

            FilterHelper Helper = new GeneralFunctions().GetWhereClause<Grievance>(Parameter.Filters);
            Query += Helper.WhereClause;
            Query += new GeneralFunctions().GetPagedQuery(Query, Parameter);
            Helper.Command.CommandText = Query;
            return new MSSQLFactory().GetObjectList<GrievanceList>(Helper.Command);
        }

        public List<GrievanceList> GetTop10GrievanceListCompact()
        {
            var Query = $@"SELECT TOP 4 Grievance_ID,GrievanceID,FullName,Subject,Message,Category,Date,Status  FROM Grievance
                            WHERE IsNumberVerified=1 AND Status <> {(short)GrievanceStatus.Resolved}
                            ORDER BY Date DESC";
            return new MSSQLFactory().GetObjectList<GrievanceList>(Query);
        }

        public int SaveGrievance(Grievance grievance)
        {
            return new MSSQLFactory().InsertRecord<Grievance>(grievance);
        }

        public int SaveGrievance(GrievanceReply grievanceReply)
        {
            return new MSSQLFactory().InsertRecord<GrievanceReply>(grievanceReply);
        }

        public Grievance GetGrievance(Guid grievance_ID)
        {
            return new MSSQLFactory().GetObject<Grievance>(new GrievanceSQLQueries().GetGrievance(grievance_ID));
        }

        public int UpdateGrievanceCount()
        {
            return new MSSQLFactory().ExecuteNonQuery(new GrievanceSQLQueries().UpdateGrievanceCount());
        }

        public List<GrievanceReply> GetGrievanceReplies(Guid grievance_ID)
        {
            return new MSSQLFactory().GetObjectList<GrievanceReply>(new GrievanceSQLQueries().GetGrievanceReplies(grievance_ID));
        }

        public int UpdateGrievance(Grievance grievance)
        {
            List<string> ignoreList = new List<string>() { "Grievance_ID", "CreatedOn", "CreatedBy" };
            var sqlCommand = new MSSQLFactory().UpdateRecord(grievance, ignoreList, ignoreList, "Grievance");
            sqlCommand.CommandText = sqlCommand.CommandText + " WHERE Grievance_ID =@Grievance_ID";
            sqlCommand.Parameters.AddWithValue("@Grievance_ID", grievance.Grievance_ID);
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }

        public List<GrievanceWidgetSummary> GetGrievanceWidgetsData(Guid? college_ID, GrievanceCategory? grievanceCategory)
        {
            return new MSSQLFactory().GetObjectList<GrievanceWidgetSummary>($"SELECT Category,COUNT(*) NoOfQueries FROM Grievance {GetWhereClause(college_ID, grievanceCategory)} GROUP BY Category");
        }

        public GrievanceResolvedWidgetSummary GetGrievanceWidgetsResolved(Guid? college_ID, GrievanceCategory? grievanceCategory)
        {
            string whereClause = GetWhereClause(college_ID, grievanceCategory);

            string query = $@"SELECT COUNT(*)Resolved,(SELECT COUNT(*) FROM Grievance {whereClause} AND Status<>3)UnResolved FROM Grievance {whereClause} AND Status=3";

            return new MSSQLFactory().GetObject<GrievanceResolvedWidgetSummary>(query);
        }


        public GrievanceVerifiedWidgetSummary GetGrievanceWidgetsVerified(Guid? college_ID, GrievanceCategory? grievanceCategory)
        {
            string whereClause = GetWhereClause(college_ID, grievanceCategory);

            string query = $@"SELECT FinalQuery.[1] AS Verified, FinalQuery.[0] UnVerified
                                FROM
                                (
                                    SELECT IsNumberVerified
                                    FROM Grievance                                        
                                        {whereClause}
                                ) SourceQuery
                                PIVOT
                                (
                                    COUNT(IsNumberVerified)
                                    FOR IsNumberVerified IN ([0], [1])
                                ) FinalQuery;";

            return new MSSQLFactory().GetObject<GrievanceVerifiedWidgetSummary>(query);
        }

        public GrievanceGeneralWidgetSummary GetGrievanceWidgetsGeneral(Guid? college_ID, GrievanceCategory? grievanceCategory)
        {
            string whereClause = GetWhereClause(college_ID, grievanceCategory);

            string query = $@"SELECT FinalQuery.[1] AS AllowedForPublic, FinalQuery.[0] NotAllowedForPublic
                                FROM
                                (
                                    SELECT AllowViewPublic FROM Grievance {whereClause} 
                                ) SourceQuery
                                PIVOT
                                (
                                    COUNT(AllowViewPublic)
                                    FOR AllowViewPublic IN ([0], [1])
                                ) FinalQuery;";

            return new MSSQLFactory().GetObject<GrievanceGeneralWidgetSummary>(query);
        }
        private static string GetWhereClause(Guid? college_ID, GrievanceCategory? grievanceCategory)
        {
            string whereClause = " WHERE 1 = 1 ";

            if (college_ID != null)
                whereClause += $" AND College_ID = '{college_ID}' ";

            if (grievanceCategory != null)
                whereClause += $" AND Category = {(short)grievanceCategory} ";

            return whereClause;
        }

        public GrievanceAssignedWidgetSummary GetGrievanceWidgetsAssigned(Guid? college_ID, GrievanceCategory? grievanceCategory)
        {
            string whereClause = GetWhereClause(college_ID, grievanceCategory);

            string query = $@"SELECT COUNT(Grievance_ID) AS UnAssigned,(SELECT COUNT(Grievance_ID) AS Assigned FROM Grievance 
                                {whereClause} AND UserAssigned_ID IS NOT NULL ) Assigned FROM Grievance {whereClause} AND UserAssigned_ID IS NULL  ";


            return new MSSQLFactory().GetObject<GrievanceAssignedWidgetSummary>(query);
        }
        public int SaveUpdateAlomini(Alomini alomini)
        {
            new MSSQLFactory().ExecuteNonQuery($"DELETE FROM StudentEmploymentStatus WHERE Student_ID='{alomini.Student_ID}'");
            return new MSSQLFactory().InsertRecord(alomini, TableName: "StudentEmploymentStatus");
        }
        public Alomini GetAlomini(Guid Student_ID)
        {
            return new MSSQLFactory().GetObject<Alomini>($"SELECT TOP 1 * FROM StudentEmploymentStatus WHERE Student_ID='{Student_ID}'");
        }
    }
}
