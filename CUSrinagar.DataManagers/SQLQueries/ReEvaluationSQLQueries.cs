using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using GeneralModels;
using System.Data.SqlClient;
using Terex;
using CUSrinagar.Models;

namespace CUSrinagar.DataManagers.SQLQueries
{
    public class ReEvaluationSQLQueries
    {
        public string GetAvailableSemestersForDropdown => @"SELECT 'Semester - '  + CAST(Semester AS VARCHAR(25)) [Text],Semester[Value]  FROM ReEvaluationSettings WHERE AllowDownloadForm = 1 AND 
                                                                SYSDATETIME() >= StartDate AND SYSDATETIME() <= EndDate
                                                            AND CourseCategory = '" + new GeneralFunctions().GetExaminationCourseCategory(AppUserHelper.OrgPrintProgramme).ToString() + "' ORDER BY Semester";

        internal string GetFormNumberCount(bool IsReEvaluation)
        {
            string query = $" SELECT FormNumberCount FROM ReEvaluationSetting WHERE IsReEvaluation ={(IsReEvaluation ? "1" : "0")} ";
            return query;
        }

        internal string GetSubjectReEvaluationFee(FormType formType)
        {
            string query = string.Empty;
            switch (formType)
            {
                case FormType.ReEvaluation:
                    query = $" SELECT FeePerSubject FROM ReEvaluationSetting WHERE IsReEvaluation = 1 ";
                    break;
                case FormType.Xerox:
                    query = $" SELECT FeePerSubject FROM ReEvaluationSetting WHERE IsReEvaluation = 0 ";
                    break;

                default:
                    break;
            }
            return query;
        }

        internal string UpdateReEvaluationFormStatus(string formNumbers, FormStatus status)
        {
            return $" UPDATE ReEvaluation SET FormStatus = " + ((int)(status)).ToString() + $",UpdatedOn=@UpdatedOn,UpdatedBy='{AppUserHelper.User_ID}' WHERE formNumber IN (" + formNumbers.ToListOfStrings(',').ToIN() + ")";
        }

        internal string DeleteReEvaluationForm(Guid student_ID, Guid reEvaluation_ID)
        {
            return $@" DELETE FROM ReEvaluationStudentSubjects WHERE ReEvaluationStudentSubjects.Student_ID = '{student_ID}' AND ReEvaluation_ID='{reEvaluation_ID}';
                            DELETE FROM ReEvaluation WHERE Student_ID = '{student_ID}' AND reEvaluation_ID='{reEvaluation_ID}'";
        }

        internal string GetReEvaluationsForStudent(Guid student_ID, short Semester)
        {
            var personalInformationTable = new GeneralFunctions().GetTableName(AppUserHelper.OrgPrintProgramme, Module.PersonalInformation);

            return $@" SELECT ReEvaluation.*,FullName AS StudentName FROM ReEvaluation 
                        INNER JOIN {personalInformationTable} ON {personalInformationTable}.Student_ID = ReEvaluation.Student_ID WHERE ReEvaluation.Student_ID = '{student_ID}' AND ReEvaluation.FormStatus = 11 AND ReEvaluation.Semester={Semester}";
        }

        internal string GetReEvaluationsForStudentByReEvaluation_ID(Guid reEvaluation_ID)
        {
            return $@" SELECT ReEvaluation.*,Name AS StudentName FROM ReEvaluation 
                        INNER JOIN StudentProfile ON StudentProfile.Student_ID = ReEvaluation.Student_ID WHERE ReEvaluation.ReEvaluation_ID = '{reEvaluation_ID}' AND FormStatus = 11 ";
        }

        internal SqlCommand GetReEvaluationList(Parameters parameters, Programme programme)
        {
            PrintProgramme printProgramme = GeneralFunctions.ProgrammeToPrintProgrammeMapping(programme);
            parameters.Filters.Add(
                    new SearchFilter
                    {
                        Column = "Semester",
                        TableAlias = "c",
                        Value = parameters.Filters.First(x => x.Column.ToLower().Trim() == "semester").Value,
                        Operator = SQLOperator.EqualTo,
                    }
                );
            string tableName = new GeneralFunctions().GetTableName(printProgramme, Module.ReEvaluation);
            string Query = $@"SELECT * FROM (
                                    SELECT DISTINCT dense_rank() OVER (PARTITION BY ReEvaluation.FormNumber ORDER BY Year DESC) AS SNo,
                                    ReEvaluation.*, dbo.PaymentDetails_{printProgramme}.PhoneNumber AS MobileNo,
                                STDINFO.FullName,STDINFO.CUSRegistrationNo,TxnDate DateFrom,ExamRollNumber,c.SemesterBatch,
                                dbo.ADMCourseMaster.CourseFullName,col.CollegeFullName,
	                               CASE WHEN ece.Center_ID IS NOT NULL THEN
	                               ece.CenterCode + ' ('+cmm2.CollegeFullName+')'
	                               WHEN cc.Center_ID IS NOT NULL THEN
	                               ec.CenterCode + ' ('+cmm.CollegeFullName+')'
	                               ELSE
                                   ''
	                               END CenterDetail 
                             FROM ReEvaluation
                             INNER JOIN ARGPersonalInformation_{printProgramme.ToString()} STDINFO ON STDINFO.Student_ID = ReEvaluation.Student_ID 
	                          INNER JOIN dbo.ARGSelectedCombination_{printProgramme.ToString()} c ON c.Student_ID = STDINFO.Student_ID
	                            INNER JOIN dbo.ADMCombinationMaster cm ON cm.Combination_ID = c.Combination_ID
                                INNER JOIN dbo.ADMCourseMaster ON ADMCourseMaster.Course_ID = cm.Course_ID
                                INNER JOIN dbo.ADMCollegeMaster col ON col.College_ID=STDINFO.AcceptCollege_ID
                             INNER JOIN ARGStudentExamForm_{printProgramme.ToString()} ON ARGStudentExamForm_{printProgramme.ToString()}.Student_ID = ReEvaluation.Student_ID 
                                AND ARGStudentExamForm_{printProgramme.ToString()}.Semester = ReEvaluation.Semester AND ExamRollNumber IS NOT NULL
                                LEFT JOIN {tableName} ON {tableName}.Entity_ID = ReEvaluation.ReEvaluation_ID 

	                            LEFT JOIN dbo.ArchivedSemesterCentersAllotmentMaster aca ON aca.Entity_ID=StudentExamForm_ID 
	                            LEFT JOIN dbo.ARGCentersAllotmentMaster cc ON cc.Entity_ID=StudentExamForm_ID

	                            LEFT JOIN dbo.ArchivedSemesterCentersMaster ece ON ece.Center_ID=aca.Center_ID AND ece.Center_ID IS NOT NULL
	                            LEFT JOIN dbo.ARGEntranceCentersMaster ec ON ec.Center_ID=cc.Center_ID AND ec.Center_ID IS NOT NULL

	                            LEFT JOIN dbo.ADMCollegeMaster cmm2 ON cmm2.College_ID=ece.College_ID
	                            LEFT JOIN dbo.ADMCollegeMaster cmm ON cmm.College_ID=ec.College_ID ";

            FilterHelper helper = new GeneralFunctions().GetWhereClause<ReEvaluation>(parameters.Filters);
            Query += helper.WhereClause;
            Query += @") AS TEMP WHERE TEMP.SNo=1 ";
            string sortColumn = parameters.SortInfo?.ColumnName;
            Query += $" ORDER BY TEMP.{(sortColumn.IsNullOrEmpty() ? "FormNumber" : sortColumn)} " + new GeneralFunctions().GetPagedQuery(Query, parameters);
            helper.Command.CommandText = Query;
            return helper.Command;
        }

        internal string IsReEvaluationOpen(FormType formType)
        {
            string query = " SELECT AllowDownloadForm FROM ReEvaluationSetting WHERE ";

            switch (formType)
            {
                case FormType.ReEvaluation:
                    query += "IsReEvaluation = 1 ";
                    break;
                case FormType.Xerox:
                    query += "IsReEvaluation = 0  ";
                    break;
                default:
                    break;
            }
            
            return query + $" AND AllowDownloadForm = 1 AND CourseCategory ={(short)new GeneralFunctions().GetExaminationCourseCategory(AppUserHelper.OrgPrintProgramme)} AND " +
                            " SYSDATETIME() >= StartDate AND SYSDATETIME() <= EndDate ";
        }

        internal SqlCommand GetReEvaluationCompactLists(Parameters parameters)
        {
            string query = @"SELECT 
	                               FullName, ReEvaluation.ReEvaluation_ID, ReEvaluation.Student_ID, ReEvaluation.FormStatus, FormNumber,
                                   Notification_ID, Semester, ReEvaluation.CreatedOn, ReEvaluationStudentSubjects.ReEvaluationSubject_ID,
                                   ReEvaluationStudentSubjects.Subject_ID
                            FROM ReEvaluation
                                INNER JOIN ReEvaluationStudentSubjects
                                    INNER JOIN ARGPersonalInformation_UG
                                        ON ARGPersonalInformation_UG.Student_ID = ReEvaluationStudentSubjects.Student_ID
                                    ON ReEvaluationStudentSubjects.ReEvaluation_ID = ReEvaluation.ReEvaluation_ID";

            FilterHelper helper = new GeneralFunctions().GetWhereClause<ReEvaluationCompactList>(parameters.Filters);
            helper.Command.CommandText = query + helper.WhereClause;

            return helper.Command;
        }

        internal string GetSubjectsForEvaluation(Guid reEvaluation_ID, Guid student_ID)
        {
            return $@"  SELECT ReEvaluationStudentSubjects.ReEvaluationSubject_ID,FormNumber,SubjectFullName SubjectID,
                               ReEvaluationStudentSubjects.ReEvaluation_ID,
                               ADMSubjectMaster.Subject_ID,
                               CASE
                                   WHEN FormType = 2 THEN
                                     CAST(1 AS BIT)
                                   ELSE
                                       CAST(0 AS BIT)
                               END AS OptForXerox,
                               CASE
                                   WHEN FormType = 1 THEN
                                      CAST(1 AS BIT)
                                   ELSE
                                       CAST(0 AS BIT)
                               END AS OptForReEvaluation
                        FROM ReEvaluation
                         INNER  JOIN ReEvaluationStudentSubjects
                                ON ReEvaluationStudentSubjects.Student_ID = ReEvaluation.Student_ID
                                   AND ReEvaluationStudentSubjects.ReEvaluation_ID = ReEvaluation.ReEvaluation_ID
						LEFT JOIN ADMSubjectMaster ON ADMSubjectMaster.Subject_ID = ReEvaluationStudentSubjects.Subject_ID 
                        WHERE FormStatus = {(short)(FormStatus.PaymentSuccessful)} AND ReEvaluation.Student_ID = '{student_ID}' AND ReEvaluation.ReEvaluation_ID = '{reEvaluation_ID}' ";
        }

        internal string GetSubjectsForEvaluation(Guid reEvaluation_ID, Guid student_ID, PrintProgramme programme)
        {
            string tableName = new GeneralFunctions().GetTableName(programme, Module.ReEvaluation);
            return $@"  SELECT ReEvaluationStudentSubjects.ReEvaluationSubject_ID,FormNumber,SubjectFullName AS SubjectID,
                               ReEvaluationStudentSubjects.ReEvaluation_ID,
                               ReEvaluationStudentSubjects.Subject_ID,
                               ReEvaluationStudentSubjects.Student_ID,
                               CASE
                                   WHEN FormType = 2 THEN
                                     CAST(1 AS BIT)
                                   ELSE
                                       CAST(0 AS BIT)
                               END AS OptForXerox,
                               CASE
                                   WHEN FormType = 1 THEN
                                      CAST(1 AS BIT)
                                   ELSE
                                       CAST(0 AS BIT)
                               END AS OptForReEvaluation,
                                ADMSubjectMaster.SubjectType
                        FROM ReEvaluation
                         INNER  JOIN ReEvaluationStudentSubjects
                                ON ReEvaluationStudentSubjects.Student_ID = ReEvaluation.Student_ID
                                   AND ReEvaluationStudentSubjects.ReEvaluation_ID = ReEvaluation.ReEvaluation_ID
                        INNER JOIN ADMSubjectMaster ON ADMSubjectMaster.Subject_ID = ReEvaluationStudentSubjects.Subject_ID
                        LEFT JOIN {tableName} ON {tableName}.Entity_ID = ReEvaluation.ReEvaluation_ID
                        WHERE FormStatus = {(short)(FormStatus.PaymentSuccessful)} AND ReEvaluation.Student_ID = '{student_ID}' AND ReEvaluation.ReEvaluation_ID = '{reEvaluation_ID}'";
        }

        internal string GetFormPrefix(FormType formType)
        {
            string query = string.Empty;

            switch (formType)
            {
                case FormType.ReEvaluation:
                    query = $" SELECT FormNumberPrefix FROM ReEvaluationSetting WHERE IsReEvaluation = 1 ";
                    break;
                case FormType.Xerox:
                    query = $" SELECT FormNumberPrefix FROM ReEvaluationSetting WHERE IsReEvaluation = 0 ";
                    break;

                default:
                    break;
            }
            return query;
        }

        internal string UpdateReEvaluationFormCount(int formCount, bool IsReEvaluation)
        {
            string query = $" Update ReEvaluationSetting SET FormNumberCount ={formCount} WHERE IsReEvaluation = {(IsReEvaluation ? 1 : 0)} ";
            return query;
        }
    }
}
