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
    public class SubjectMarksStructureDB
    {
        public List<MSSubjectMarksStructure> List(Parameters parameters)
        {
            SqlCommand cmd = new SqlCommand();
            string Query = $@"SELECT * FROM MSSubjectMarksStructure";
            var helper = new GeneralFunctions().GetWhereClause<MSSubjectMarksStructure>(parameters.Filters);
            Query += helper.WhereClause + new GeneralFunctions().GetPagedQuery(Query, parameters);
            helper.Command.CommandText = Query;
            return new MSSQLFactory().GetObjectList<MSSubjectMarksStructure>(helper.Command);
        }

        public MSSubjectMarksStructure Get(Guid SubjectMarksStructure_ID)
        {
            SqlCommand cmd = new SqlCommand();
            string TQuery = $@"SELECT * FROM MSSubjectMarksStructure WHERE  SubjectMarksStructure_ID='{SubjectMarksStructure_ID}'";
            cmd.CommandText = TQuery;
            return new MSSQLFactory().GetObject<MSSubjectMarksStructure>(TQuery);
        }

        public bool MSSubjectMarksStructureExists(MSSubjectMarksStructure model)
        {
            SqlCommand cmd = new SqlCommand();
            string TQuery = $@"SELECT 1 FROM MSSubjectMarksStructure WHERE 
                SGPAType={((short)model.SGPAType).ToString()} AND 
                TotalCredit={model.TotalCredit}  AND 
                CreditWeightage={model.CreditWeightage}  AND 
                IsExternalMarksApplicable={(model.IsExternalMarksApplicable ? "1" : "0")} AND 
                ExternalMaxMarks={ model.ExternalMaxMarks.ToString() } AND 
                ExternalMinPassMarks={ model.ExternalMinPassMarks.ToString()} AND 
                ExternalVisibleTo={((short)model.ExternalVisibleTo).ToString() } AND 
                ExternalIsPartOf={ ((short)model.ExternalIsPartOf).ToString() } AND 
                IsExternalPassComponent={(model.IsExternalPassComponent ? "1" : "0") } AND 
                IsExternalAttendance_AssessmentMarksApplicable={(model.IsExternalAttendance_AssessmentMarksApplicable ? "1" : "0")} AND 
                ExternalAttendance_AssessmentMaxMarks={ model.ExternalAttendance_AssessmentMaxMarks.ToString()} AND 
                ExternalAttendance_AssessmentMinPassMarks={ model.ExternalAttendance_AssessmentMinPassMarks.ToString() } AND 
                ExternalAttendance_AssessmentVisibleTo={((short)model.ExternalAttendance_AssessmentVisibleTo).ToString()} AND 
                ExternalAttendance_AssessmentIsPartOf={((short)model.ExternalAttendance_AssessmentIsPartOf).ToString() } AND 
                IsExternalAttendance_AssessmentPassComponent={ (model.IsExternalAttendance_AssessmentPassComponent? "1" : "0") } AND 
                IsInternalMarksApplicable={(model.IsInternalMarksApplicable ? "1" : "0")} AND 
                InternalMaxMarks={ model.InternalMaxMarks.ToString() } AND 
                InternalMinPassMarks={model.InternalMinPassMarks.ToString()} AND 
                InternalVisibleTo={((short)model.InternalVisibleTo).ToString()} AND 
                InternalIsPartOf={ ((short)model.InternalIsPartOf).ToString() } AND 
                IsInternalPassComponent={ (model.IsInternalPassComponent? "1" : "0")} AND 
                IsInternalAttendance_AssessmentMarksApplicable={(model.IsInternalAttendance_AssessmentMarksApplicable ? "1" : "0")} AND 
                InternalAttendance_AssessmentMaxMarks={model.InternalAttendance_AssessmentMaxMarks.ToString()} AND 
                InternalAttendance_AssessmentMinPassMarks={model.InternalAttendance_AssessmentMinPassMarks.ToString()} AND 
                InternalAttendance_AssessmentVisibleTo={((short)model.InternalAttendance_AssessmentVisibleTo).ToString() } AND 
                InternalAttendance_AssessmentIsPartOf={ ((short)model.InternalAttendance_AssessmentIsPartOf).ToString()} AND 
                IsInternalAttendance_AssessmentPassComponent={ (model.IsInternalAttendance_AssessmentPassComponent ? "1" : "0") }";
            cmd.CommandText = TQuery;
            return new MSSQLFactory().ExecuteScalar<int>(cmd) > 0;
        }

        public int Save(MSSubjectMarksStructure setting)
        {
            return new MSSQLFactory().InsertRecord<MSSubjectMarksStructure>(setting, "MSSubjectMarksStructure");
        }

        public int Update(MSSubjectMarksStructure setting)
        {
            List<string> ignoreList = new List<string>() { "SubjectMarksStructure_ID" };
            var sqlCommand = new MSSQLFactory().UpdateRecord(setting, ignoreList, ignoreList, "MSSubjectMarksStructure");
            sqlCommand.CommandText = sqlCommand.CommandText + " WHERE SubjectMarksStructure_ID =@SubjectMarksStructure_ID";
            sqlCommand.Parameters.AddWithValue("@SubjectMarksStructure_ID", setting.SubjectMarksStructure_ID);
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }

        public int Delete(Guid SubjectMarksStructure_ID)
        {
            var Query = $"Delete from MSSubjectMarksStructure WHERE SubjectMarksStructure_ID ='{SubjectMarksStructure_ID}'";
            return new MSSQLFactory().ExecuteNonQuery(Query);
        }

        public int ChangeStatus(Guid SubjectMarksStructure_ID)
        {
            var Query = $"Update MSSubjectMarksStructure Set AllowDownloadForm=case when AllowDownloadForm=1 then 0 else 1 end WHERE SubjectMarksStructure_ID ='{SubjectMarksStructure_ID}'";
            return new MSSQLFactory().ExecuteNonQuery(Query);
        }


    }
}
