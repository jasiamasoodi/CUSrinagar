using CUSrinagar.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using CUSrinagar.Extensions;
using Terex;
using CUSrinagar.Enums;
using System.Web.Mvc;
using CUSrinagar.DataManagers.SQLQueries;
using GeneralModels;

namespace CUSrinagar.DataManagers
{
    public class DegreeCertificateDB
    {
        public List<DegreeCertificate> List(Parameters parameters, PrintProgramme printProgramme)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            SqlCommand cmd = new SqlCommand();
            string Query = $@"SELECT Distinct AwardedDegreeTitle DegreeCourseTitle,T.CGPA CGPA_Type1,C.Course_ID Course_Id,
                             CASE WHEN C.Programme=2 THEN C.SchoolFullName ELSE NULL END [SchoolOf]
                            ,CollegeFullName,ADMCollegeMaster.Address CollegeAddress
                            ,T.Student_ID,CUSRegistrationNo,E.ExamRollNumber,FullName,AwardedDegreeTitle,SemesterFrom,T.SemesterTo
                            ,DateofDeclaration DegreeCompletionDate
                            ,DC.DegreeCertificate_ID,DC.DispatchNumber,DC.SerialNumber,DC.CreatedOn,DC.CreatedBy_ID,DC.ValidatedOn,DC.ValidatedBy_ID,
                            DC.PrintedOn,DC.PrintedBy_ID,DC.HandedOverOn,DC.HandedOverBy_ID,S.Photograph,DC.IssueNumber,DC.Remarks,C.CourseFullName,
                            S.Batch,DC.TotalDuplicatesIssued,DC.DuplicateIssuanceDetails,S.IsLateralEntry,S.PreviousUniversityRegnNo,S.OtherUnivMigrationReceivedOn 
                            FROM  dbo.MSCGPA T
                            JOIN dbo.ARGPersonalInformation_{printProgramme.ToString()} S ON S.Student_ID = T.Student_ID AND T.SGPAType=1
                           join MSTranscriptDegreeSettings Sett on t.TCourse_ID=sett.Course_ID and T.SemesterTo=Sett.SemesterTo and  S.Batch >= sett.BatchFrom  and S.Batch <= sett.BatchTo
                            JOIN dbo.ADMCourseMaster C ON C.Course_ID = T.TCourse_ID
                            JOIN dbo.ADMCollegeMaster ON ADMCollegeMaster.College_ID = S.AcceptCollege_ID
                            JOIN (SELECT DISTINCT Student_ID,examrollnumber FROM dbo.ARGStudentExamForm_{printProgramme.ToString()}  WHERE ExamRollNumber IS NOT NULL AND Semester=4 AND Status=4) E ON E.Student_ID = T.Student_ID
                            LEFT JOIN DegreeCertificate DC ON DC.Student_ID = T.Student_ID AND DC.SemesterTo=T.SemesterTo";
            var helper = new GeneralFunctions().GetWhereClause<FilterModels>(parameters.Filters);
            Query += helper.WhereClause + new GeneralFunctions().GetPagedQuery(Query, parameters);
            helper.Command.CommandText = Query;
            return new MSSQLFactory().GetObjectList<DegreeCertificate>(helper.Command);
        }

        public List<DegreeCertificate> ListEng(Parameters parameters, PrintProgramme printProgramme)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            SqlCommand cmd = new SqlCommand();
            string Query = $@"SELECT DISTINCT 
		                    ISNULL(C.DegreeCourseTitle,c.CourseFullName)DegreeCourseTitle,
		                    ST.Percentage,ST.DateofDeclaration DegreeCompletionDate,ST.NotificationNo                
                            ,CollegeFullName,ADMCollegeMaster.Address CollegeAddress
                            ,T.Student_ID,CUSRegistrationNo,E.ExamRollNumber,FullName,                            
                            DC.DegreeCertificate_ID,DC.DispatchNumber,DC.SerialNumber,DC.CreatedOn,DC.CreatedBy_ID,DC.ValidatedOn,DC.ValidatedBy_ID,
                            DC.PrintedOn,DC.PrintedBy_ID,DC.HandedOverOn,DC.HandedOverBy_ID,S.Photograph,DC.IssueNumber,DC.Remarks,
                            S.Batch,DC.TotalDuplicatesIssued,DC.DuplicateIssuanceDetails,S.IsLateralEntry,S.PreviousUniversityRegnNo,S.OtherUnivMigrationReceivedOn 
                            FROM  dbo.IH_SemesterAll T
                            JOIN dbo.ARGPersonalInformation_{printProgramme} S ON S.Student_ID = T.Student_ID 
							JOIN dbo.ADMSubjectMaster SM ON SM.Subject_ID = T.Subject_ID						
                            JOIN dbo.ARGSelectedCombination_{printProgramme} SC ON SC.Student_ID = S.Student_ID AND SC.Student_ID = T.Student_ID
							JOIN dbo.ADMCombinationMaster CM ON CM.Combination_ID = SC.Combination_ID					
                            JOIN dbo.ADMCourseMaster C ON C.Course_ID = CM.Course_ID 
                            JOIN dbo.ADMCollegeMaster ON ADMCollegeMaster.College_ID = S.AcceptCollege_ID
							JOIN dbo.MSStatus ST ON ST.Student_ID = T.Student_ID
                            JOIN (SELECT DISTINCT Student_ID,examrollnumber FROM dbo.ARGStudentExamForm_{printProgramme}  WHERE ExamRollNumber IS NOT NULL AND Semester=8 AND Status=4) E ON E.Student_ID = T.Student_ID
                            LEFT JOIN DegreeCertificate DC ON DC.Student_ID = T.Student_ID";
            var helper = new GeneralFunctions().GetWhereClause<FilterModels>(parameters.Filters);
            Query += helper.WhereClause + new GeneralFunctions().GetPagedQuery(Query, parameters);
            helper.Command.CommandText = Query;
            return new MSSQLFactory().GetObjectList<DegreeCertificate>(helper.Command);
        }

        public int GetMaxSerialNumber()
        {
            return new MSSQLFactory().ExecuteScalar<int>("SELECT MAX(SerialNumber) FROM dbo.DegreeCertificate");
        }

        public int Save(DegreeCertificate model)
        {
            return new MSSQLFactory().InsertRecord<DegreeCertificate>(model, "DegreeCertificate");
        }

        public int Update(DegreeCertificate model)
        {
            List<string> ignoreList = new List<string>() { "DegreeCertificate_ID", "CreatedOn", "CreatedBy" };
            var sqlCommand = new MSSQLFactory().UpdateRecord(model, ignoreList, ignoreList, "DegreeCertificate");
            sqlCommand.CommandText = sqlCommand.CommandText + " WHERE DegreeCertificate_ID =@DegreeCertificate_ID";
            sqlCommand.Parameters.AddWithValue("@DegreeCertificate_ID", model.DegreeCertificate_ID);
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }

        public int Delete(Guid DegreeCertificate_ID)
        {
            var Query = $"Delete from DegreeCertificate WHERE WHERE DegreeCertificate_ID ='{DegreeCertificate_ID}'";
            return new MSSQLFactory().ExecuteNonQuery(Query);
        }
        public int Validated(Guid DegreeCertificate_ID)
        {
            var Query = $@"UPDATE DegreeCertificate SET ValidatedOn=COALESCE(ValidatedOn,GETDATE()),ValidatedBy_ID=COALESCE(ValidatedBy_ID,'{AppUserHelper.User_ID}')
                            WHERE DegreeCertificate_ID='{DegreeCertificate_ID}' AND (ValidatedOn IS NULL OR ValidatedBy_ID IS NULL)";
            return new MSSQLFactory().ExecuteNonQuery(Query);
        }
        public int Printed(Guid DegreeCertificate_ID)
        {
            var Query = $@"UPDATE DegreeCertificate SET ValidatedOn=COALESCE(ValidatedOn,GETDATE()),ValidatedBy_ID=COALESCE(ValidatedBy_ID,'{AppUserHelper.User_ID}'),
                                             PrintedOn=COALESCE(PrintedOn,GETDATE()) ,PrintedBy_ID=COALESCE(PrintedBy_ID,'{AppUserHelper.User_ID}')
                            WHERE DegreeCertificate_ID='{DegreeCertificate_ID}' AND (PrintedOn IS NULL OR ValidatedOn IS NULL OR ValidatedOn<=PrintedOn)";
            return new MSSQLFactory().ExecuteNonQuery(Query);
        }

        public int Duplicate(Guid DegreeCertificate_ID, string DuplicateLbl)
        {
            DuplicateLbl = DuplicateLbl?.Replace("--", "").Replace(";", "");
            var Query = $@"UPDATE dbo.DegreeCertificate SET PrintedOn=NULL,TotalDuplicatesIssued=TotalDuplicatesIssued+1,HandedOverOn=NULL,HandedOverBy_ID=NULL, DuplicateIssuanceDetails=DuplicateIssuanceDetails+' (PrintedOn '+ CONVERT(varchar, PrintedOn, 103)+'),{DuplicateLbl} Duplicate Generated '
                           WHERE DegreeCertificate_ID='{DegreeCertificate_ID}'";
            SqlCommand cmd = new SqlCommand();

            if (DuplicateLbl.Trim().ToLower() == "1st")
            {
                Query = $@"UPDATE dbo.DegreeCertificate SET PrintedOn=NULL,TotalDuplicatesIssued=TotalDuplicatesIssued+1,HandedOverOn=NULL,HandedOverBy_ID=NULL,DuplicateIssuanceDetails='Original Generated (PrintedOn '+ CONVERT(varchar, PrintedOn, 103)+'), {DuplicateLbl} Duplicate Generated'
                           WHERE DegreeCertificate_ID='{DegreeCertificate_ID}'";

            }

            cmd.CommandText = Query;


            return new MSSQLFactory().ExecuteNonQuery(Query);
        }

        public int HandedOverOn(Guid DegreeCertificate_ID)
        {
            var Query = $@"UPDATE DegreeCertificate SET ValidatedOn=COALESCE(ValidatedOn,GETDATE()),ValidatedBy_ID=COALESCE(ValidatedBy_ID,'{AppUserHelper.User_ID}'),
                            PrintedOn=COALESCE(PrintedOn,GETDATE()) ,PrintedBy_ID=COALESCE(PrintedBy_ID,'{AppUserHelper.User_ID}'),
                            HandedOverOn=COALESCE(HandedOverOn,GETDATE()) ,HandedOverBy_ID=COALESCE(HandedOverBy_ID,'{AppUserHelper.User_ID}')
                            WHERE DegreeCertificate_ID='{DegreeCertificate_ID}' AND (PrintedOn IS NULL OR ValidatedOn IS NULL OR HandedOverBy_ID IS NULL)";
            return new MSSQLFactory().ExecuteNonQuery(Query);
        }

        public int GetMaxIssueNumber()
        {
            return new MSSQLFactory().ExecuteScalar<int>("SELECT MAX(IssueNumber) FROM dbo.DegreeCertificate");
        }

        public int MarkPrintedAll(List<Guid> DegreeCertificate_IDs)
        {
            var Query = $@"UPDATE dbo.DegreeCertificate SET PrintedOn=GETDATE(),
                            PrintedBy_ID='{AppUserHelper.User_ID}' WHERE DegreeCertificate_ID IN ({DegreeCertificate_IDs.ToIN()}) AND PrintedOn IS NULL
                            AND ValidatedOn IS NOT NULL;";
            return new MSSQLFactory().ExecuteNonQuery(Query);
        }

        public int MarkHandedOverAll(List<Guid> DegreeCertificate_IDs)
        {
            var Query = $@"UPDATE dbo.DegreeCertificate SET HandedOverOn=GETDATE(),
                            HandedOverBy_ID='{AppUserHelper.User_ID}' WHERE DegreeCertificate_ID IN ({DegreeCertificate_IDs.ToIN()}) AND PrintedOn IS NOT NULL
                            AND ValidatedOn IS NOT NULL AND HandedOverOn IS NULL;";
            return new MSSQLFactory().ExecuteNonQuery(Query);
        }

        public int MarkAllGood(List<Guid> DegreeCertificate_IDs)
        {
            var Query = $@"UPDATE dbo.DegreeCertificate SET ValidatedOn=GETDATE(),
                            ValidatedBy_ID='{AppUserHelper.User_ID}' WHERE DegreeCertificate_ID IN ({DegreeCertificate_IDs.ToIN()}) 
                            AND ValidatedOn IS NULL;";
            return new MSSQLFactory().ExecuteNonQuery(Query);
        }
    }
}
