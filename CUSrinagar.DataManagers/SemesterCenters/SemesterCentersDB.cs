using CUSrinagar.Enums;
using CUSrinagar.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using CUSrinagar.Extensions;
using Terex;
using System.Data;
using System.Web.Mvc;
using CUSrinagar.DataManagers.SQLQueries;
using System.Linq;

namespace CUSrinagar.DataManagers
{
    public class SemesterCentersDB
    {
        #region CenterCRUD
        public List<CenterAllotmentDetail> GetCenterAllotmentDetails(Guid Center_ID)
        {
            SqlCommand cmd = new SqlCommand(@"SELECT COUNT(ARGEntranceCentersMaster.Center_ID) TotalStudentsAssigned,
                                               ARGEntranceCentersMaster.Center_ID,
                                               ADMCourseMaster.Course_ID,
                                               ARGStudentExamForm.Semester,
                                               Notification.Notification_ID,
                                               MAX(YEAR(ARGCentersAllotmentMaster.EntranceDate)) Year,
                                               ARGStudentExamForm.Year EYear,
	                                           MAX(CourseFullName) CourseFullName,
											   MAX(Notification.Description+' Dated :'+cast(Notification.StartDate AS NVARCHAR(11))) NotificationDescription,
											   MAX(Notification.Link) NotificationURL
                                        FROM ARGEntranceCentersMaster
                                            JOIN ARGCentersAllotmentMaster
                                                ON ARGCentersAllotmentMaster.Center_ID = ARGEntranceCentersMaster.Center_ID
                                         JOIN (SELECT StudentExamForm_ID,Year,Semester,Status,Notification_ID FROM ARGStudentExamForm_IH WHERE Status=4
		                                        UNION
		                                        SELECT StudentExamForm_ID,Year,Semester,Status,Notification_ID FROM ARGStudentExamForm_PG WHERE Status=4
		                                        UNION
		                                        SELECT StudentExamForm_ID,Year,Semester,Status,Notification_ID FROM ARGStudentExamForm_UG WHERE Status=4) ARGStudentExamForm 
		                                        ON ARGStudentExamForm.StudentExamForm_ID = ARGCentersAllotmentMaster.Entity_ID 
                                                LEFT JOIN Notification ON Notification.Notification_ID=ARGStudentExamForm.Notification_ID
                                            JOIN ADMCourseMaster
                                                ON ADMCourseMaster.Course_ID = ARGCentersAllotmentMaster.Course_ID
                                        WHERE ARGEntranceCentersMaster.Center_ID = @Center_ID
                                        GROUP BY ARGEntranceCentersMaster.Center_ID,
                                                 ADMCourseMaster.Course_ID,
                                                 ARGStudentExamForm.Semester,Notification.Notification_ID,
                                                 ARGStudentExamForm.Year ORDER BY ARGStudentExamForm.Semester,CourseFullName;");
            cmd.Parameters.AddWithValue("@Center_ID", Center_ID);
            return new MSSQLFactory().GetObjectList<CenterAllotmentDetail>(cmd);
        }

        public List<ARGEntranceCentersMaster> GetSemesterCentersMaster(Guid College_ID, bool IsEntranceCenter)
        {
            string query = $@"SELECT ARGEntranceCentersMaster.*,(CollegeFullName +' , '+Address) CenterName FROM ARGEntranceCentersMaster 
                                JOIN ADMCollegeMaster ON ADMCollegeMaster.College_ID = ARGEntranceCentersMaster.College_ID WHERE IsEntrance={(IsEntranceCenter ? 1 : 0)} ";
            if (College_ID == Guid.Empty)
                return new MSSQLFactory().GetObjectList<ARGEntranceCentersMaster>(query + " ORDER BY CollegeFullName,CenterCode ASC");
            else
                return new MSSQLFactory().GetObjectList<ARGEntranceCentersMaster>(query + $" AND ARGEntranceCentersMaster.College_ID='{College_ID}' ORDER BY CollegeFullName, CenterCode ASC");

        }

        public int ChangeCenterStatus(Guid Center_ID)
        {
            return new MSSQLFactory().ExecuteNonQuery($@"UPDATE ARGEntranceCentersMaster SET Status=~Status FROM ARGEntranceCentersMaster
                                        LEFT JOIN ARGCentersAllotmentMaster ON ARGCentersAllotmentMaster.Center_ID = ARGEntranceCentersMaster.Center_ID
                                        WHERE CenterAllowment_ID IS NULL AND ARGEntranceCentersMaster.Center_ID='{Center_ID}'");
        }

        public int DeleteCenter(Guid Center_ID)
        {
            return new MSSQLFactory().ExecuteNonQuery($@"DELETE ARGEntranceCentersMaster FROM ARGEntranceCentersMaster
                                    LEFT JOIN ARGCentersAllotmentMaster ON ARGCentersAllotmentMaster.Center_ID = ARGEntranceCentersMaster.Center_ID
                                    WHERE CenterAllowment_ID IS NULL AND ARGEntranceCentersMaster.Center_ID='{Center_ID}'");
        }

        public int CreateCenters(ARGEntranceCentersMaster SemesterCenter)
        {
            if (!CenterCodeByCollegeAlreadyExists(SemesterCenter.College_ID, SemesterCenter.CenterCode, SemesterCenter.IsEntrance))
                return new MSSQLFactory().InsertRecord(SemesterCenter);
            else
                return 0;
        }

        public bool CenterCodeByCollegeAlreadyExists(Guid College_ID, string CenterCode, bool IsEntrance)
        {
            SqlCommand command = new SqlCommand("SELECT COUNT(Center_ID) FROM ARGEntranceCentersMaster WHERE College_ID=@College_ID AND CenterCode=@CenterCode AND IsEntrance=@IsEntrance");
            command.Parameters.AddWithValue("@College_ID", College_ID);
            command.Parameters.AddWithValue("@CenterCode", CenterCode);
            command.Parameters.AddWithValue("@IsEntrance", IsEntrance);
            return new MSSQLFactory().ExecuteScalar<int>(command) > 0;
        }
        #endregion

        #region Generate Center Notice
        public List<CenterNotice> GetCenterAndCourseInfo(RePrintCenterNotice printCenterNotice)
        {
            PrintProgramme printProgramme = GeneralFunctions.ProgrammeToPrintProgrammeMapping(printCenterNotice.CourseCategory);
            string AllColleges = printCenterNotice.College_ID == Guid.Empty ? "" : "College_ID=@College_ID AND";

            SqlCommand sqlCommand = new SqlCommand($@"SELECT DISTINCT ARGCentersAllotmentMaster.Center_ID,ARGCentersAllotmentMaster.Course_ID,Programme AS CourseCategory,ARGStudentExamForm_{printProgramme.ToString()}.Semester FROM ARGEntranceCentersMaster
                                                    JOIN ARGCentersAllotmentMaster ON ARGCentersAllotmentMaster.Center_ID = ARGEntranceCentersMaster.Center_ID
                                                    JOIN ARGStudentExamForm_{printProgramme.ToString()} ON ARGStudentExamForm_{printProgramme.ToString()}.StudentExamForm_ID=ARGCentersAllotmentMaster.Entity_ID
                                                    JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = ARGCentersAllotmentMaster.Course_ID
                                                    WHERE {AllColleges} Semester=@Semester AND Programme=@Programme AND ARGEntranceCentersMaster.Status=1");

            if (!string.IsNullOrWhiteSpace(AllColleges))
                sqlCommand.Parameters.AddWithValue("@College_ID", printCenterNotice.College_ID);

            sqlCommand.Parameters.AddWithValue("@Semester", printCenterNotice.Semester);
            sqlCommand.Parameters.AddWithValue("@Programme", (short)printCenterNotice.CourseCategory);
            return new MSSQLFactory().GetObjectList<CenterNotice>(sqlCommand);
        }

        public CenterNotice GetCenterNoticeDetail(CenterNotice _CenterNotice)
        {
            PrintProgramme printProgramme = GeneralFunctions.ProgrammeToPrintProgrammeMapping(_CenterNotice.CourseCategory);
            string query = $@"SELECT MAX(ARGEntranceCentersMaster.College_ID) College_ID
                            ,MAX(ARGCentersAllotmentMaster.Course_ID) Course_ID
                            ,MAX(ARGCentersAllotmentMaster.Center_ID) Center_ID
                            ,MAX(CollegeFullName) CollegeFullName
                            ,MAX(ADMCourseMaster.CourseFullName) CourseFullName
                            ,MAX(ADMCourseMaster.Programme) CourseCategory
                            ,MAX(CenterCode) CenterCode
                            ,MAX(Semester) Semester
                            ,MAX(YEAR(ARGCentersAllotmentMaster.EntranceDate)) ExamYear
                            ,MIN(REPLACE(REPLACE(ExamRollNumber,'N',''),'T','')) +' - '+ MAX(REPLACE(REPLACE(ExamRollNumber,'N',''),'T','')) RegularSeries
                            ,(SELECT MIN(REPLACE(REPLACE(ExamRollNumber,'N',''),'T','')) +' - '+ MAX(REPLACE(REPLACE(ExamRollNumber,'N',''),'T','')) BacklogSeries FROM  ARGStudentExamForm_{printProgramme.ToString()} 
                            JOIN ARGCentersAllotmentMaster ON [Entity_ID]=StudentExamForm_ID
                            JOIN ARGEntranceCentersMaster ON ARGEntranceCentersMaster.Center_ID = ARGCentersAllotmentMaster.Center_ID
                            WHERE ARGCentersAllotmentMaster.Course_ID=@Course_ID AND ARGEntranceCentersMaster.Center_ID=@Center_ID AND IsRegular=0 AND Semester=@Semester) BacklogSeries
                            FROM ARGStudentExamForm_{printProgramme.ToString()} 
                            JOIN ARGCentersAllotmentMaster ON [Entity_ID]=StudentExamForm_ID
                            JOIN ARGEntranceCentersMaster ON ARGEntranceCentersMaster.Center_ID = ARGCentersAllotmentMaster.Center_ID
                            JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = ARGCentersAllotmentMaster.Course_ID
                            JOIN ADMCollegeMaster ON ADMCollegeMaster.College_ID = ARGEntranceCentersMaster.College_ID
                            WHERE ARGCentersAllotmentMaster.Course_ID=@Course_ID AND ARGEntranceCentersMaster.Center_ID=@Center_ID AND IsRegular=1 AND Semester=@Semester";

            SqlCommand sqlCommand = new SqlCommand(query);
            sqlCommand.Parameters.AddWithValue("@Course_ID", _CenterNotice.Course_ID);
            sqlCommand.Parameters.AddWithValue("@Center_ID", _CenterNotice.Center_ID);
            sqlCommand.Parameters.AddWithValue("@Semester", _CenterNotice.Semester);
            return new MSSQLFactory().GetObject<CenterNotice>(sqlCommand);
        }

        public DataTable GetCenterCollegeMapping(RePrintCenterNotice _PrintCenterNotice)
        {
            PrintProgramme printProgramme = GeneralFunctions.ProgrammeToPrintProgrammeMapping(_PrintCenterNotice.CourseCategory);
            string LookInProgramme = $"= {(short)_PrintCenterNotice.CourseCategory} ";

            string query = $@"SELECT DISTINCT col.CollegeFullName [Colleges to assign],cm.CenterCode, coll.CollegeFullName [Center College] FROM dbo.ARGCentersAllotmentMaster ca
                                JOIN dbo.ARGEntranceCentersMaster cm ON cm.Center_ID = ca.Center_ID AND cm.IsEntrance=0 AND cm.Status=1
                                JOIN dbo.ARGStudentExamForm_{printProgramme.ToString()}  ef ON ef.StudentExamForm_ID=ca.Entity_ID AND ef.Semester=@Semester AND ef.Year=@Year
                                JOIN dbo.ARGPersonalInformation_{printProgramme.ToString()} p ON p.Student_ID=ef.Student_ID
                                JOIN dbo.ADMCollegeMaster col ON p.AcceptCollege_ID=col.College_ID
                                JOIN  dbo.ADMCollegeMaster coll ON coll.College_ID = cm.College_ID
                                ORDER BY [Colleges to assign] ASC,cm.CenterCode asc";

            SqlCommand command = new SqlCommand(query);
            command.Parameters.AddWithValue("@Semester", _PrintCenterNotice.Semester);
            command.Parameters.AddWithValue("@Year", _PrintCenterNotice.ExaminationYear);
            return new MSSQLFactory().GetDataTable(command);
        }

        public CenterNotice GetCenterNoticeDetailBackLogOnly(CenterNotice _CenterNotice)
        {
            PrintProgramme printProgramme = GeneralFunctions.ProgrammeToPrintProgrammeMapping(_CenterNotice.CourseCategory);
            string query = $@"SELECT MAX(ARGEntranceCentersMaster.College_ID) College_ID
                            ,MAX(ARGCentersAllotmentMaster.Course_ID) Course_ID
                            ,MAX(ARGCentersAllotmentMaster.Center_ID) Center_ID
                            ,MAX(CollegeFullName) CollegeFullName
                            ,MAX(ADMCourseMaster.CourseFullName) CourseFullName
                            ,MAX(ADMCourseMaster.Programme) CourseCategory
                            ,MAX(CenterCode) CenterCode
                            ,MAX(Semester) Semester
                            ,MAX(YEAR(ARGCentersAllotmentMaster.EntranceDate)) ExamYear
                            ,MIN(REPLACE(REPLACE(ExamRollNumber,'N',''),'T','')) +' - '+ MAX(REPLACE(REPLACE(ExamRollNumber,'N',''),'T','')) BacklogSeries                           
                            FROM ARGStudentExamForm_{printProgramme.ToString()} 
                            JOIN ARGCentersAllotmentMaster ON [Entity_ID]=StudentExamForm_ID
                            JOIN ARGEntranceCentersMaster ON ARGEntranceCentersMaster.Center_ID = ARGCentersAllotmentMaster.Center_ID
                            JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = ARGCentersAllotmentMaster.Course_ID
                            JOIN ADMCollegeMaster ON ADMCollegeMaster.College_ID = ARGEntranceCentersMaster.College_ID
                            WHERE ARGCentersAllotmentMaster.Course_ID=@Course_ID AND ARGEntranceCentersMaster.Center_ID=@Center_ID AND IsRegular=0 AND Semester=@Semester";

            SqlCommand sqlCommand = new SqlCommand(query);
            sqlCommand.Parameters.AddWithValue("@Course_ID", _CenterNotice.Course_ID);
            sqlCommand.Parameters.AddWithValue("@Center_ID", _CenterNotice.Center_ID);
            sqlCommand.Parameters.AddWithValue("@Semester", _CenterNotice.Semester);
            return new MSSQLFactory().GetObject<CenterNotice>(sqlCommand);
        }

        public int GetStudentsAssignedToCenter(CenterNotice _CenterNotice)
        {
            PrintProgramme printProgramme = GeneralFunctions.ProgrammeToPrintProgrammeMapping(_CenterNotice.CourseCategory);
            return new MSSQLFactory().ExecuteScalar<int>($@"SELECT 
                            COUNT(ARGCentersAllotmentMaster.Center_ID) TotalStudentesAssigned
                            FROM ARGStudentExamForm_{printProgramme.ToString()}
                            JOIN ARGCentersAllotmentMaster ON [Entity_ID]=StudentExamForm_ID                            
                            WHERE ARGCentersAllotmentMaster.Course_ID='{_CenterNotice.Course_ID}' AND ARGCentersAllotmentMaster.Center_ID='{_CenterNotice.Center_ID}'
							 AND Semester={_CenterNotice.Semester} AND Status=4");
        }


        #endregion


        #region Archived CNotices

        public int GetArchivedStudentsAssignedToCenter(CenterNotice _CenterNotice)
        {
            PrintProgramme printProgramme = GeneralFunctions.ProgrammeToPrintProgrammeMapping(_CenterNotice.CourseCategory);
            return new MSSQLFactory().ExecuteScalar<int>($@"SELECT 
                            COUNT(ArchivedSemesterCentersAllotmentMaster.Center_ID) TotalStudentesAssigned
                            FROM ARGStudentExamForm_{printProgramme.ToString()}
                            JOIN ArchivedSemesterCentersAllotmentMaster ON [Entity_ID]=StudentExamForm_ID                            
                            WHERE ArchivedSemesterCentersAllotmentMaster.Course_ID='{_CenterNotice.Course_ID}' AND ArchivedSemesterCentersAllotmentMaster.Center_ID='{_CenterNotice.Center_ID}'
							 AND Semester={_CenterNotice.Semester} AND Status=4");
        }

        public CenterNotice GetArchivedCenterNoticeDetailBackLogOnly(CenterNotice _CenterNotice)
        {
            PrintProgramme printProgramme = GeneralFunctions.ProgrammeToPrintProgrammeMapping(_CenterNotice.CourseCategory);
            string query = $@"SELECT MAX(ArchivedSemesterCentersMaster.College_ID) College_ID
                            ,MAX(ArchivedSemesterCentersAllotmentMaster.Course_ID) Course_ID
                            ,MAX(ArchivedSemesterCentersAllotmentMaster.Center_ID) Center_ID
                            ,MAX(CollegeFullName) CollegeFullName
                            ,MAX(ADMCourseMaster.CourseFullName) CourseFullName
                            ,MAX(ADMCourseMaster.Programme) CourseCategory
                            ,MAX(CenterCode) CenterCode
                            ,MAX(Semester) Semester
                            ,MAX(YEAR(ArchivedSemesterCentersAllotmentMaster.EntranceDate)) ExamYear
                            ,MIN(REPLACE(REPLACE(ExamRollNumber,'N',''),'T','')) +' - '+ MAX(REPLACE(REPLACE(ExamRollNumber,'N',''),'T','')) BacklogSeries                           
                            FROM ARGStudentExamForm_{printProgramme.ToString()} 
                            JOIN ArchivedSemesterCentersAllotmentMaster ON [Entity_ID]=StudentExamForm_ID
                            JOIN ArchivedSemesterCentersMaster ON ArchivedSemesterCentersMaster.Center_ID = ArchivedSemesterCentersAllotmentMaster.Center_ID
                            JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = ArchivedSemesterCentersAllotmentMaster.Course_ID
                            JOIN ADMCollegeMaster ON ADMCollegeMaster.College_ID = ArchivedSemesterCentersMaster.College_ID
                            WHERE ArchivedSemesterCentersAllotmentMaster.Course_ID=@Course_ID AND ArchivedSemesterCentersMaster.Center_ID=@Center_ID AND IsRegular=0 AND Semester=@Semester";

            SqlCommand sqlCommand = new SqlCommand(query);
            sqlCommand.Parameters.AddWithValue("@Course_ID", _CenterNotice.Course_ID);
            sqlCommand.Parameters.AddWithValue("@Center_ID", _CenterNotice.Center_ID);
            sqlCommand.Parameters.AddWithValue("@Semester", _CenterNotice.Semester);
            return new MSSQLFactory().GetObject<CenterNotice>(sqlCommand);
        }
        public CenterNotice GetArchivedCenterNoticeDetail(CenterNotice _CenterNotice)
        {
            PrintProgramme printProgramme = GeneralFunctions.ProgrammeToPrintProgrammeMapping(_CenterNotice.CourseCategory);
            string query = $@"SELECT MAX(ArchivedSemesterCentersMaster.College_ID) College_ID
                            ,MAX(ArchivedSemesterCentersAllotmentMaster.Course_ID) Course_ID
                            ,MAX(ArchivedSemesterCentersAllotmentMaster.Center_ID) Center_ID
                            ,MAX(CollegeFullName) CollegeFullName
                            ,MAX(ADMCourseMaster.CourseFullName) CourseFullName
                            ,MAX(ADMCourseMaster.Programme) CourseCategory
                            ,MAX(CenterCode) CenterCode
                            ,MAX(Semester) Semester
                            ,MAX(YEAR(ArchivedSemesterCentersAllotmentMaster.EntranceDate)) ExamYear
                            ,MIN(REPLACE(REPLACE(ExamRollNumber,'N',''),'T','')) +' - '+ MAX(REPLACE(REPLACE(ExamRollNumber,'N',''),'T','')) RegularSeries
                            ,(SELECT MIN(REPLACE(REPLACE(ExamRollNumber,'N',''),'T','')) +' - '+ MAX(REPLACE(REPLACE(ExamRollNumber,'N',''),'T','')) BacklogSeries FROM  ARGStudentExamForm_{printProgramme.ToString()} 
                            JOIN ArchivedSemesterCentersAllotmentMaster ON [Entity_ID]=StudentExamForm_ID
                            JOIN ArchivedSemesterCentersMaster ON ArchivedSemesterCentersMaster.Center_ID = ArchivedSemesterCentersAllotmentMaster.Center_ID
                            WHERE ArchivedSemesterCentersAllotmentMaster.Course_ID=@Course_ID AND ArchivedSemesterCentersMaster.Center_ID=@Center_ID AND IsRegular=0 AND Semester=@Semester) BacklogSeries
                            FROM ARGStudentExamForm_{printProgramme.ToString()} 
                            JOIN ArchivedSemesterCentersAllotmentMaster ON [Entity_ID]=StudentExamForm_ID
                            JOIN ArchivedSemesterCentersMaster ON ArchivedSemesterCentersMaster.Center_ID = ArchivedSemesterCentersAllotmentMaster.Center_ID
                            JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = ArchivedSemesterCentersAllotmentMaster.Course_ID
                            JOIN ADMCollegeMaster ON ADMCollegeMaster.College_ID = ArchivedSemesterCentersMaster.College_ID
                            WHERE ArchivedSemesterCentersAllotmentMaster.Course_ID=@Course_ID AND ArchivedSemesterCentersMaster.Center_ID=@Center_ID AND IsRegular=1 AND Semester=@Semester";

            SqlCommand sqlCommand = new SqlCommand(query);
            sqlCommand.Parameters.AddWithValue("@Course_ID", _CenterNotice.Course_ID);
            sqlCommand.Parameters.AddWithValue("@Center_ID", _CenterNotice.Center_ID);
            sqlCommand.Parameters.AddWithValue("@Semester", _CenterNotice.Semester);
            return new MSSQLFactory().GetObject<CenterNotice>(sqlCommand);
        }
        public List<CenterNotice> GetArchivedCenterAndCourseInfo(RePrintCenterNotice printCenterNotice)
        {
            PrintProgramme printProgramme = GeneralFunctions.ProgrammeToPrintProgrammeMapping(printCenterNotice.CourseCategory);
            string AllColleges = printCenterNotice.College_ID == Guid.Empty ? "" : "College_ID=@College_ID AND";

            SqlCommand sqlCommand = new SqlCommand($@"SELECT DISTINCT ArchivedSemesterCentersAllotmentMaster.Center_ID,ArchivedSemesterCentersAllotmentMaster.Course_ID,Programme AS CourseCategory,ARGStudentExamForm_{printProgramme.ToString()}.Semester FROM ArchivedSemesterCentersMaster
                                                    JOIN ArchivedSemesterCentersAllotmentMaster ON ArchivedSemesterCentersAllotmentMaster.Center_ID = ArchivedSemesterCentersMaster.Center_ID
                                                    JOIN ARGStudentExamForm_{printProgramme.ToString()} ON ARGStudentExamForm_{printProgramme.ToString()}.StudentExamForm_ID=ArchivedSemesterCentersAllotmentMaster.Entity_ID
                                                    JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = ArchivedSemesterCentersAllotmentMaster.Course_ID
                                                    WHERE {AllColleges} Semester=@Semester AND Programme=@Programme AND ArchivedSemesterCentersMaster.Status=1 AND ARGStudentExamForm_{printProgramme.ToString()}.year={printCenterNotice.ExaminationYear}");

            if (!string.IsNullOrWhiteSpace(AllColleges))
                sqlCommand.Parameters.AddWithValue("@College_ID", printCenterNotice.College_ID);

            sqlCommand.Parameters.AddWithValue("@Semester", printCenterNotice.Semester);
            sqlCommand.Parameters.AddWithValue("@Programme", (short)printCenterNotice.CourseCategory);
            return new MSSQLFactory().GetObjectList<CenterNotice>(sqlCommand);
        }
        #endregion



        #region Generate Excels
        public DataTable GetCourseWiseExamFormCount(RePrintCenterNotice _PrintCenterNotice)
        {
            PrintProgramme printProgramme = GeneralFunctions.ProgrammeToPrintProgrammeMapping(_PrintCenterNotice.CourseCategory);
            string LookInProgramme = $"= {(short)_PrintCenterNotice.CourseCategory} ";

            string submittedDateExmFrm = "";
            if (_PrintCenterNotice.ExmFrmSubmittedFrom != null && _PrintCenterNotice.ExmFrmSubmittedFrom != DateTime.MinValue)
            {
                submittedDateExmFrm = $" AND CAST(ISNULL(ARGStudentExamForm_{printProgramme.ToString()}.UpdatedOn, ARGStudentExamForm_{printProgramme.ToString()}.CreatedOn) AS DATE)>='{_PrintCenterNotice.ExmFrmSubmittedFrom.Value.ToString("yyyy-MM-dd")}'";
            }

            string query = $@"SELECT *,(ISNULL([Total Regular Students],0)+ISNULL([Total Backlog Students],0)) Total FROM (SELECT MAX(CollegeFullName) [College FullName],
                                   MAX(CourseFullName) [Course FullName],CASE
									WHEN IsRegular=1 THEN 'Total Regular Students'
									ELSE 'Total Backlog Students'
								   END IsRegular,
                                   COUNT(ARGPersonalInformation_{printProgramme.ToString()}.AcceptCollege_ID) [Total Students]
                            FROM ARGStudentExamForm_{printProgramme.ToString()}
                                JOIN ARGSelectedCombination_{printProgramme.ToString()}
                                    ON ARGSelectedCombination_{printProgramme.ToString()}.Student_ID = ARGStudentExamForm_{printProgramme.ToString()}.Student_ID
                                      AND ARGStudentExamForm_{printProgramme.ToString()}.Status = {(short)FormStatus.Accepted}
                                JOIN ADMCombinationMaster
                                    ON ADMCombinationMaster.Combination_ID = ARGSelectedCombination_{printProgramme.ToString()}.Combination_ID
                                JOIN ARGPersonalInformation_{printProgramme.ToString()}
                                    ON ARGPersonalInformation_{printProgramme.ToString()}.Student_ID = ARGSelectedCombination_{printProgramme.ToString()}.Student_ID
                                JOIN ADMCollegeMaster
                                    ON AcceptCollege_ID = ADMCollegeMaster.College_ID
                                JOIN ADMCourseMaster
                                    ON ADMCourseMaster.Course_ID = ADMCombinationMaster.Course_ID
                            WHERE ARGSelectedCombination_{printProgramme.ToString()}.Semester = @Semester
                                  AND ARGStudentExamForm_{printProgramme.ToString()}.Semester = @Semester
                                  AND ADMCourseMaster.Programme {LookInProgramme}
                                  AND ARGStudentExamForm_{printProgramme.ToString()}.Year = @Year {submittedDateExmFrm}
                            GROUP BY ARGPersonalInformation_{printProgramme.ToString()}.AcceptCollege_ID,
                                     ADMCombinationMaster.Course_ID, IsRegular) t
                            PIVOT(
	                            MAX([Total Students]) 
                                FOR IsRegular 
                                IN([Total Regular Students],[Total Backlog Students])) AS pt
                                ORDER BY PT.[College FullName],PT.[Course FullName]";

            SqlCommand command = new SqlCommand(query);
            command.Parameters.AddWithValue("@Semester", _PrintCenterNotice.Semester);
            command.Parameters.AddWithValue("@Year", _PrintCenterNotice.ExaminationYear);
            return new MSSQLFactory().GetDataTable(command);
        }

        public DataTable GetCenterWiseSubjectCount(RePrintCenterNotice _PrintCenterNotice)
        {
            PrintProgramme printProgramme = GeneralFunctions.ProgrammeToPrintProgrammeMapping(_PrintCenterNotice.CourseCategory);
            string LookInProgramme = $"= {(short)_PrintCenterNotice.CourseCategory}";

            if (_PrintCenterNotice.CourseCategory == Programme.IG)
            {
                LookInProgramme = $"IN ({(short)Programme.IG},{(short)Programme.HS},{(short)Programme.Professional})";
            }
            string query = "";
            string collegeID = "";

            if (_PrintCenterNotice.College_ID != Guid.Empty)
                collegeID = $" AND ADMCollegeMaster.College_ID='{_PrintCenterNotice.College_ID}' ";

            if (_PrintCenterNotice.ListType == ExamFormListType.Regular)
            {
                query = $@"SELECT S.* FROM (
                        SELECT DISTINCT
                            MAX(CollegeFullName) [College FullName],
                            MAX(CourseFullName) [Course FullName],
                            ADMSubjectMaster.SubjectFullName Subject,
                                MAX(dbo.FNSubjectTypeDescription(ADMSubjectMaster.SubjectType)) SubjectType,
                                MAX(DepartmentFullName) Department,
                                MAX(CenterCode) Center,
                                ARGSelectedCombination_{printProgramme.ToString()}.SemesterBatch Batch,
                            COUNT(ARGSelectedCombination_{printProgramme.ToString()}.Student_ID) [No. of Students],
                            MAX('Regular') FormType
                        FROM ARGStudentExamForm_{printProgramme.ToString()}
                            JOIN ARGSelectedCombination_{printProgramme.ToString()}
                                ON ARGSelectedCombination_{printProgramme.ToString()}.Student_ID = ARGStudentExamForm_{printProgramme.ToString()}.Student_ID
                            JOIN VWSCMaster
                                ON VWSCMaster.Combination_ID = ARGSelectedCombination_{printProgramme.ToString()}.Combination_ID
                            JOIN ARGCentersAllotmentMaster
                                ON ARGCentersAllotmentMaster.Course_ID = VWSCMaster.Course_ID
                                    AND ARGCentersAllotmentMaster.Entity_ID = ARGStudentExamForm_{printProgramme.ToString()}.StudentExamForm_ID
		                            JOIN ARGEntranceCentersMaster ON ARGEntranceCentersMaster.Center_ID = ARGCentersAllotmentMaster.Center_ID AND ARGEntranceCentersMaster.Status = 1
		                            JOIN ADMCollegeMaster ON ADMCollegeMaster.College_ID = ARGEntranceCentersMaster.College_ID
		                            JOIN ADMSubjectMaster ON ADMSubjectMaster.Subject_ID = VWSCMaster.Subject_ID
                                    LEFT JOIN dbo.Department ON Department.Department_ID = ADMSubjectMaster.Department_ID
                                    JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = VWSCMaster.Course_ID
                        JOIN ARGPersonalInformation_{printProgramme.ToString()} p ON p.Student_ID = ARGStudentExamForm_{printProgramme.ToString()}.Student_ID
                        WHERE ARGSelectedCombination_{printProgramme.ToString()}.Semester=@Semester AND
                            ARGStudentExamForm_{printProgramme.ToString()}.Status = {(short)FormStatus.Accepted}
                        AND ADMSubjectMaster.SubjectType NOT IN({(int)SubjectType.SEC},{(int)SubjectType.Practical},{(int)SubjectType.Workshop},{(int)SubjectType.Lab})
	                        AND   ARGStudentExamForm_{printProgramme.ToString()}.Year = @Year
                                AND ARGStudentExamForm_{printProgramme.ToString()}.Semester=@Semester
                            AND ADMCourseMaster.Programme {LookInProgramme} AND IsRegular=1 {collegeID}
                        GROUP BY ADMSubjectMaster.SubjectFullName,
                                    ARGCentersAllotmentMaster.Center_ID,
                                    ADMSubjectMaster.Subject_ID,
                                    VWSCMaster.Course_ID,
                                ARGSelectedCombination_{printProgramme.ToString()}.SemesterBatch
                        UNION
                        SELECT DISTINCT
                                MAX(CollegeFullName) [College FullName],
                                MAX(CourseFullName) [Course FullName],
                                ADMSubjectMaster.SubjectFullName Subject,
                                    MAX(dbo.FNSubjectTypeDescription(ADMSubjectMaster.SubjectType)) SubjectType,
                                    MAX(DepartmentFullName) Department,
                                    MAX(CenterCode) Center,
                                   ARGSelectedCombination_{printProgramme.ToString()}.SemesterBatch Batch,
                                COUNT(ARGSelectedCombination_{printProgramme.ToString()}.Student_ID) [No. of Students],
                                MAX('Regular') FormType
                            FROM ARGStudentExamForm_{printProgramme.ToString()}
                                JOIN ARGSelectedCombination_{printProgramme.ToString()}
                                    ON ARGSelectedCombination_{printProgramme.ToString()}.Student_ID = ARGStudentExamForm_{printProgramme.ToString()}.Student_ID
                                JOIN dbo.ARGStudentAdditionalSubjects_{printProgramme.ToString()} ad
                                    ON ad.Student_ID = ARGStudentExamForm_{printProgramme.ToString()}.Student_ID AND ad.Semester=@Semester
                                JOIN ARGCentersAllotmentMaster
                                    ON ARGCentersAllotmentMaster.Course_ID = ad.Course_ID
                                        AND ARGCentersAllotmentMaster.Entity_ID = ARGStudentExamForm_{printProgramme.ToString()}.StudentExamForm_ID
		                                JOIN ARGEntranceCentersMaster ON ARGEntranceCentersMaster.Center_ID = ARGCentersAllotmentMaster.Center_ID AND ARGEntranceCentersMaster.Status = 1
		                                JOIN ADMCollegeMaster ON ADMCollegeMaster.College_ID = ARGEntranceCentersMaster.College_ID
		                                JOIN ADMSubjectMaster ON ADMSubjectMaster.Subject_ID = ad.Subject_ID
                                        LEFT JOIN dbo.Department ON Department.Department_ID = ADMSubjectMaster.Department_ID
                                        JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = ad.Course_ID
                            JOIN ARGPersonalInformation_{printProgramme.ToString()} p ON p.Student_ID = ARGStudentExamForm_{printProgramme.ToString()}.Student_ID
                            WHERE ARGSelectedCombination_{printProgramme.ToString()}.Semester=@Semester AND
                                ARGStudentExamForm_{printProgramme.ToString()}.Status = {(short)FormStatus.Accepted}
                            AND ADMSubjectMaster.SubjectType NOT IN({(int)SubjectType.SEC},{(int)SubjectType.Practical},{(int)SubjectType.Workshop},{(int)SubjectType.Lab})
	                            AND   ARGStudentExamForm_{printProgramme.ToString()}.Year = @Year
                                    AND ARGStudentExamForm_{printProgramme.ToString()}.Semester=@Semester
                                AND ADMCourseMaster.Programme  {LookInProgramme} AND IsRegular=1 {collegeID}
                            GROUP BY ADMSubjectMaster.SubjectFullName,
                                        ARGCentersAllotmentMaster.Center_ID,
                                        ADMSubjectMaster.Subject_ID,
                                        ad.Course_ID,
                                       ARGSelectedCombination_{printProgramme.ToString()}.SemesterBatch) S
                            ORDER BY S.[College FullName],S.[Course FullName] ASC,S.Subject,S.Center,S.[No. of Students];";
            }
            else if (_PrintCenterNotice.ListType == ExamFormListType.Backlog)
            {
                query = $@"SELECT DISTINCT
                            MAX(CollegeFullName) [College FullName],
                            MAX(CC.CourseFullName) [Course FullName],
                            ADMSubjectMaster.SubjectFullName Subject,
                            MAX(dbo.FNSubjectTypeDescription(ADMSubjectMaster.SubjectType)) SubjectType,
                            MAX(DepartmentFullName) Department,
                            MAX(CenterCode) Center,
                            ARGSelectedCombination_{printProgramme.ToString()}.SemesterBatch Batch,
                            COUNT(ARGStudentExamForm_{printProgramme.ToString()}.Student_ID) [No. of Students],
                            MAX('Backlog') FormType
                        FROM ARGStudentExamForm_{printProgramme.ToString()}
                            JOIN ARGPersonalInformation_{printProgramme.ToString()} p ON p.Student_ID = ARGStudentExamForm_{printProgramme.ToString()}.Student_ID
							JOIN dbo.ARGSelectedCombination_{printProgramme.ToString()} sc ON sc.Student_ID = p.Student_ID AND SC.Semester=ARGStudentExamForm_{printProgramme.ToString()}.Semester
							JOIN dbo.ADMCombinationMaster CM ON CM.Combination_ID = sc.Combination_ID AND CM.Semester=SC.Semester
							JOIN dbo.ADMCourseMaster CC ON CC.Course_ID = CM.Course_ID
                            JOIN ARGStudentReExamForm
                                ON ARGStudentReExamForm.StudentExamForm_ID = ARGStudentExamForm_{printProgramme.ToString()}.StudentExamForm_ID
                            JOIN ARGSelectedCombination_{printProgramme.ToString()}
                                ON ARGSelectedCombination_{printProgramme.ToString()}.Student_ID = ARGStudentExamForm_{printProgramme.ToString()}.Student_ID
                                AND ARGSelectedCombination_{printProgramme.ToString()}.Semester= {_PrintCenterNotice.Semester}
                            JOIN ADMSubjectMaster
                                ON ADMSubjectMaster.Subject_ID = ARGStudentReExamForm.Subject_ID
                            LEFT JOIN dbo.Department ON Department.Department_ID = ADMSubjectMaster.Department_ID
                            JOIN ARGCentersAllotmentMaster
                                ON ARGCentersAllotmentMaster.Entity_ID = ARGStudentExamForm_{printProgramme.ToString()}.StudentExamForm_ID
                            JOIN ARGEntranceCentersMaster
                                ON ARGEntranceCentersMaster.Center_ID = ARGCentersAllotmentMaster.Center_ID
                                   AND ARGEntranceCentersMaster.Status = 1
                            JOIN ADMCollegeMaster
                                ON ADMCollegeMaster.College_ID = ARGEntranceCentersMaster.College_ID
                        WHERE IsRegular = 0
                            AND ARGStudentExamForm_{printProgramme.ToString()}.Status = {(short)FormStatus.Accepted}
                            AND ADMSubjectMaster.SubjectType NOT IN({(int)SubjectType.SEC},{(int)SubjectType.Practical},{(int)SubjectType.Workshop},{(int)SubjectType.Lab})
                            AND ARGStudentExamForm_{printProgramme.ToString()}.Year = {_PrintCenterNotice.ExaminationYear}
                            AND ARGStudentExamForm_{printProgramme.ToString()}.Semester = {_PrintCenterNotice.Semester}
                            AND CC.Programme {LookInProgramme} {collegeID}
                        GROUP BY ADMSubjectMaster.SubjectFullName,
                                 ARGCentersAllotmentMaster.Center_ID,
                                 ADMSubjectMaster.Subject_ID,
                                 ADMSubjectMaster.Course_ID,
                                 ARGSelectedCombination_{printProgramme.ToString()}.SemesterBatch
                        ORDER BY [College FullName],
                                 Batch,
                                 Center,
                                 Subject,
                                 [No. of Students];";
            }

            SqlCommand command = new SqlCommand(query);
            command.Parameters.AddWithValue("@Semester", _PrintCenterNotice.Semester);
            command.Parameters.AddWithValue("@Year", _PrintCenterNotice.ExaminationYear);
            return new MSSQLFactory().GetDataTable(command);
        }

        public List<DropDownOptLabelGeneral> GetCenterList(Guid College_ID, Guid Course_ID, PrintProgramme printProgramme, short examYear, short semester)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            return new MSSQLFactory().GetObjectList<DropDownOptLabelGeneral>
                ($@"SELECT DISTINCT CONVERT(NVARCHAR(50), ARGEntranceCentersMaster.Center_ID) AS Value,
                               CenterCode AS Text,
                              '' AS DataGroupField
                        FROM ARGEntranceCentersMaster
                        JOIN dbo.ARGCentersAllotmentMaster 
                        ON ARGCentersAllotmentMaster.Center_ID = ARGEntranceCentersMaster.Center_ID
                        AND ARGCentersAllotmentMaster.Course_ID='{Course_ID}'
		                        JOIN ARGStudentExamForm_{printProgramme} ef ON ef.StudentExamForm_ID=ARGCentersAllotmentMaster.Entity_ID
		                        AND ef.Year={examYear} AND ef.Semester={semester}
                        WHERE ARGEntranceCentersMaster.Status = 1
                              AND ARGEntranceCentersMaster.IsEntrance = 0
                              AND ARGEntranceCentersMaster.College_ID = '{College_ID}'
                        ORDER BY CenterCode ASC;");
        }

        public DataTable GetOverAllExamFormSubjectWiseCount(RePrintCenterNotice _PrintCenterNotice)
        {
            PrintProgramme printProgramme = GeneralFunctions.ProgrammeToPrintProgrammeMapping(_PrintCenterNotice.CourseCategory);
            string LookInProgramme = $"= {(short)_PrintCenterNotice.CourseCategory}";

            if (_PrintCenterNotice.CourseCategory == Programme.IG)
            {
                LookInProgramme = $"IN ({(short)Programme.IG},{(short)Programme.HS},{(short)Programme.Professional})";
            }

            string IncludeCollege = string.Empty, includeAdCollege = string.Empty;
            if (_PrintCenterNotice.College_ID != Guid.Empty)
            {
                IncludeCollege = $" AND VWSCMaster.College_ID='{_PrintCenterNotice.College_ID}'";
                includeAdCollege = $" AND p.AcceptCollege_ID='{_PrintCenterNotice.College_ID}' ";
            }
            string JoinPInfo = "";

            string query = "";
            if (_PrintCenterNotice.ListType == ExamFormListType.Regular)
            {
                query = $@"SELECT S.* FROM (SELECT DISTINCT
                            MAX(CollegeFullName) [College FullName],
                            MAX(CourseFullName) [Course FullName],
                            MAX(ADMSubjectMaster.SubjectFullName) Subject,
                            MAX(dbo.FNSubjectTypeDescription(ADMSubjectMaster.SubjectType)) SubjectType,
                            ARGSelectedCombination_{printProgramme.ToString()}.SemesterBatch AS Batch,
                            COUNT(ARGSelectedCombination_{printProgramme.ToString()}.Student_ID) [No. of Students],
	                        MAX('Regular') FormType
                            FROM ARGStudentExamForm_{printProgramme.ToString()}{JoinPInfo}
                            JOIN ARGSelectedCombination_{printProgramme.ToString()}
                                ON ARGSelectedCombination_{printProgramme.ToString()}.Student_ID = ARGStudentExamForm_{printProgramme.ToString()}.Student_ID
                                AND ARGStudentExamForm_{printProgramme.ToString()}.Semester=@Semester
		                            JOIN dbo.ARGPersonalInformation_{printProgramme.ToString()} p ON p.Student_ID = ARGStudentExamForm_{printProgramme.ToString()}.Student_ID
                            JOIN VWSCMaster
                                ON VWSCMaster.Combination_ID = ARGSelectedCombination_{printProgramme.ToString()}.Combination_ID {IncludeCollege}
		                            JOIN ADMCollegeMaster ON ADMCollegeMaster.College_ID = VWSCMaster.College_ID
		                            JOIN ADMSubjectMaster ON ADMSubjectMaster.Subject_ID = VWSCMaster.Subject_ID
                                    JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = VWSCMaster.Course_ID

                            WHERE ARGSelectedCombination_{printProgramme.ToString()}.Semester=@Semester AND
                            ARGStudentExamForm_{printProgramme.ToString()}.Status = {(short)FormStatus.Accepted}
                            AND IsRegular=1
                            AND ADMSubjectMaster.SubjectType NOT IN({(int)SubjectType.SEC},{(int)SubjectType.Practical},{(int)SubjectType.Workshop},{(int)SubjectType.Lab})
                            AND   ARGStudentExamForm_{printProgramme.ToString()}.Year = @Year
                                AND ARGStudentExamForm_{printProgramme.ToString()}.Semester=@Semester
                            AND ADMCourseMaster.Programme {LookInProgramme} 
                            GROUP BY
                                    VWSCMaster.College_ID,
                                    VWSCMaster.Course_ID,
                                    ADMSubjectMaster.Subject_ID,ARGSelectedCombination_{printProgramme.ToString()}.SemesterBatch
                            UNION

                        SELECT DISTINCT
                            MAX(CollegeFullName) [College FullName],
                            MAX(CourseFullName) [Course FullName],
                            MAX(ADMSubjectMaster.SubjectFullName) Subject,
                            MAX(dbo.FNSubjectTypeDescription(ADMSubjectMaster.SubjectType)) SubjectType,
                            ARGSelectedCombination_{printProgramme.ToString()}.SemesterBatch AS Batch,
                            COUNT(ARGSelectedCombination_{printProgramme.ToString()}.Student_ID) [No. of Students],
	                        MAX('Regular') FormType
                            FROM ARGStudentExamForm_{printProgramme.ToString()}
                            JOIN ARGSelectedCombination_{printProgramme.ToString()}
                                ON ARGSelectedCombination_{printProgramme.ToString()}.Student_ID = ARGStudentExamForm_{printProgramme.ToString()}.Student_ID 
		                        AND ARGStudentExamForm_{printProgramme.ToString()}.Semester=@Semester
		                            JOIN dbo.ARGPersonalInformation_{printProgramme.ToString()} p ON p.Student_ID = ARGStudentExamForm_{printProgramme.ToString()}.Student_ID
                            JOIN dbo.ARGStudentAdditionalSubjects_{printProgramme.ToString()} ad
                                ON ad.Student_ID = ARGStudentExamForm_{printProgramme.ToString()}.Student_ID AND ARGStudentExamForm_{printProgramme.ToString()}.Semester=@Semester
		                            JOIN ADMCollegeMaster ON ADMCollegeMaster.College_ID = p.AcceptCollege_ID
		                            JOIN ADMSubjectMaster ON ADMSubjectMaster.Subject_ID = ad.Subject_ID
                                    JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = ad.Course_ID

                            WHERE ARGSelectedCombination_{printProgramme.ToString()}.Semester=@Semester AND
                            ARGStudentExamForm_{printProgramme.ToString()}.Status = {(short)FormStatus.Accepted}
                            AND IsRegular=1
                            AND ADMSubjectMaster.SubjectType NOT IN({(int)SubjectType.SEC},{(int)SubjectType.Practical},{(int)SubjectType.Workshop},{(int)SubjectType.Lab})
                            AND   ARGStudentExamForm_{printProgramme.ToString()}.Year = @Year
                                AND ARGStudentExamForm_{printProgramme.ToString()}.Semester=@Semester
                            AND ADMCourseMaster.Programme {LookInProgramme} 
                            GROUP BY
                                    p.AcceptCollege_ID,
                                    ad.Course_ID,
                                    ADMSubjectMaster.Subject_ID,ARGSelectedCombination_{printProgramme.ToString()}.SemesterBatch) S
                            ORDER BY S.[College FullName] ASC,S.[Course FullName] ASC,S.Subject ASC,S.[No. of Students];";
            }
            else if (_PrintCenterNotice.ListType == ExamFormListType.Backlog)
            {
                if (_PrintCenterNotice.College_ID != Guid.Empty)
                    IncludeCollege = $" AND p.AcceptCollege_ID='{_PrintCenterNotice.College_ID}'";
                query = $@"SELECT DISTINCT
                        MAX(CollegeFullName) [College FullName],
                        MAX(CourseFullName) [Course FullName],
                        MAX(ADMSubjectMaster.SubjectFullName) Subject,
                        MAX(dbo.FNSubjectTypeDescription(ADMSubjectMaster.SubjectType)) SubjectType,
                        sc.SemesterBatch Batch, COUNT(ARGStudentExamForm_{printProgramme.ToString()}.Student_ID) [No. of Students],
                        MAX('Backlog') FormType
                    FROM ARGStudentExamForm_{printProgramme.ToString()}
                        JOIN ARGPersonalInformation_{printProgramme.ToString()} p ON p.Student_ID = ARGStudentExamForm_{printProgramme.ToString()}.Student_ID
                        JOIN ARGSelectedCombination_{printProgramme.ToString()} sc  ON p.Student_ID = sc.Student_ID AND sc.Semester=@Semester
                        JOIN ARGStudentReExamForm
                            ON ARGStudentReExamForm.StudentExamForm_ID = ARGStudentExamForm_{printProgramme.ToString()}.StudentExamForm_ID
                        JOIN ADMSubjectMaster
                            ON ADMSubjectMaster.Subject_ID = ARGStudentReExamForm.Subject_ID
                        JOIN ADMCollegeMaster
                            ON ADMCollegeMaster.College_ID = p.AcceptCollege_ID
                        JOIN ADMCourseMaster
                            ON ADMCourseMaster.Course_ID = ADMSubjectMaster.Course_ID
                    WHERE IsRegular = 0
                        AND ARGStudentExamForm_{printProgramme.ToString()}.Status =  {(short)FormStatus.Accepted}
                        AND ADMSubjectMaster.SubjectType NOT IN({(int)SubjectType.SEC},{(int)SubjectType.Practical},{(int)SubjectType.Workshop},{(int)SubjectType.Lab})
                        AND ARGStudentExamForm_{printProgramme.ToString()}.Year = @Year
                        AND ARGStudentExamForm_{printProgramme.ToString()}.Semester = @Semester
	                    AND ARGStudentReExamForm.FeeStatus ={(short)FormStatus.FeePaid}
                        AND ADMCourseMaster.Programme {LookInProgramme} {IncludeCollege}
	                    GROUP BY ADMCollegeMaster.College_ID,ADMCourseMaster.Course_ID,ARGStudentReExamForm.Subject_ID,sc.SemesterBatch
                    ORDER BY [College FullName],[Course FullName],Subject,
                                Batch,[No. of Students] desc;";
            }
            SqlCommand command = new SqlCommand(query);
            command.Parameters.AddWithValue("@Semester", _PrintCenterNotice.Semester);
            command.Parameters.AddWithValue("@Year", _PrintCenterNotice.ExaminationYear);
            return new MSSQLFactory().GetDataTable(command);
        }

        public DataTable GetWithoutExamFormSubjectWiseCount(RePrintCenterNotice _PrintCenterNotice)
        {
            PrintProgramme printProgramme = GeneralFunctions.ProgrammeToPrintProgrammeMapping(_PrintCenterNotice.CourseCategory);
            string LookInProgramme = $"= {(short)_PrintCenterNotice.CourseCategory}";

            string IncludeCollege = string.Empty, includeAdCollege = string.Empty;
            if (_PrintCenterNotice.College_ID != Guid.Empty)
            {
                IncludeCollege = $" AND VWSCMaster.College_ID='{_PrintCenterNotice.College_ID}'";
                includeAdCollege = $" AND p.AcceptCollege_ID='{_PrintCenterNotice.College_ID}' ";
            }

            string batchWise = "";
            string perInfobatchWise = "";

            short latestBatch = new MSSQLFactory()
                .ExecuteScalar<short>($@"SELECT TOP 1 Batch FROM dbo.ARGPersonalInformation_{printProgramme} WHERE AcceptCollege_ID IS NOT NULL
                                    AND AcceptCollege_ID NOT IN('F1FC5214-041F-4B5A-92CC-F585F7174527','9D03A374-4398-4A48-BE2A-FD9911EC6F82')
                                    ORDER BY Batch DESC");

            if (_PrintCenterNotice.ListType == ExamFormListType.Backlog)
            {
                batchWise = $" AND ARGSelectedCombination_{printProgramme.ToString()}.SemesterBatch<{latestBatch}";
                perInfobatchWise = $" AND p.Batch<{latestBatch}";
            }
            else if (_PrintCenterNotice.ListType == ExamFormListType.Regular)
            {
                batchWise = $" AND ARGSelectedCombination_{printProgramme.ToString()}.SemesterBatch>={latestBatch}";
                perInfobatchWise = $" AND p.Batch>={latestBatch}";
            }

            string query = $@"SELECT S.* FROM (SELECT DISTINCT
                            MAX(CollegeFullName) [College FullName],
                            MAX(CourseFullName) [Course FullName],
                            MAX(ADMSubjectMaster.SubjectFullName) Subject,
                            MAX(dbo.FNSubjectTypeDescription(ADMSubjectMaster.SubjectType)) SubjectType,
                            MAX(DepartmentFullName) Department,
                            COUNT(ARGSelectedCombination_{printProgramme.ToString()}.Student_ID) [No. of Students]
                            FROM  ARGSelectedCombination_{printProgramme.ToString()}
                            JOIN VWSCMaster
                                ON VWSCMaster.Combination_ID = ARGSelectedCombination_{printProgramme.ToString()}.Combination_ID {IncludeCollege}
		                            JOIN ADMCollegeMaster ON ADMCollegeMaster.College_ID = VWSCMaster.College_ID
		                            JOIN ADMSubjectMaster ON ADMSubjectMaster.Subject_ID = VWSCMaster.Subject_ID
                                    LEFT JOIN dbo.Department ON Department.Department_ID = ADMSubjectMaster.Department_ID
                                    JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = VWSCMaster.Course_ID
                            WHERE ARGSelectedCombination_{printProgramme.ToString()}.Semester=@Semester {batchWise}
                            AND ADMSubjectMaster.SubjectType NOT IN({(int)SubjectType.SEC},{(int)SubjectType.Practical},{(int)SubjectType.Workshop},{(int)SubjectType.Lab})
                            AND ADMCourseMaster.Programme {LookInProgramme} 
                            GROUP BY
                                    VWSCMaster.College_ID,
                                    VWSCMaster.Course_ID,
                                    ADMSubjectMaster.Subject_ID

                            UNION

                            SELECT DISTINCT
                            MAX(CollegeFullName) [College FullName],
                            MAX(CourseFullName) [Course FullName],
                            MAX(ADMSubjectMaster.SubjectFullName) Subject,
                            MAX(dbo.FNSubjectTypeDescription(ADMSubjectMaster.SubjectType)) SubjectType,
                            MAX(DepartmentFullName) Department,
                            COUNT(ad.Student_ID) [No. of Students]
                            FROM   ARGStudentAdditionalSubjects_{printProgramme.ToString()} ad
	                            JOIN dbo.ARGPersonalInformation_{printProgramme.ToString()} p ON p.Student_ID=ad.Student_ID  AND ad.Semester=@Semester {includeAdCollege}
	                            JOIN ADMSubjectMaster ON ADMSubjectMaster.Subject_ID = ad.Subject_ID
                                LEFT JOIN dbo.Department ON Department.Department_ID = ADMSubjectMaster.Department_ID
                                JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = ad.Course_ID
	                            JOIN ADMCollegeMaster ON ADMCollegeMaster.College_ID = p.AcceptCollege_ID
                            WHERE ADMSubjectMaster.SubjectType NOT IN({(int)SubjectType.SEC},{(int)SubjectType.Practical},{(int)SubjectType.Workshop},{(int)SubjectType.Lab})
                            AND ADMCourseMaster.Programme  {LookInProgramme}  {perInfobatchWise}
                            GROUP BY
                                    p.AcceptCollege_ID,
                                    ad.Course_ID,
                                    ADMSubjectMaster.Subject_ID) S
                            ORDER BY S.[College FullName] ASC,S.[Course FullName] ASC,S.Subject ASC,S.[No. of Students];";
            SqlCommand command = new SqlCommand(query);
            command.Parameters.AddWithValue("@Semester", _PrintCenterNotice.Semester);
            return new MSSQLFactory().GetDataTable(command);
        }

        public DataTable GetStudentDetailsWithExamFormSubjects(RePrintCenterNotice _PrintCenterNotice)
        {
            PrintProgramme printProgramme = GeneralFunctions.ProgrammeToPrintProgrammeMapping(_PrintCenterNotice.CourseCategory);
            string LookInProgramme = $"= {(short)_PrintCenterNotice.CourseCategory}";

            if (_PrintCenterNotice.CourseCategory == Programme.IG)
            {
                LookInProgramme = $"IN ({(short)Programme.IG},{(short)Programme.HS},{(short)Programme.Professional})";
            }

            string IncludeCollege = string.Empty, includeAdCollege = string.Empty;
            if (_PrintCenterNotice.College_ID != Guid.Empty)
            {
                IncludeCollege = $" AND VWSCMaster.College_ID='{_PrintCenterNotice.College_ID}'";
                includeAdCollege = $" AND p.AcceptCollege_ID='{_PrintCenterNotice.College_ID}' ";
            }

            string submittedDateExmFrm = "";
            if (_PrintCenterNotice.ExmFrmSubmittedFrom != null && _PrintCenterNotice.ExmFrmSubmittedFrom != DateTime.MinValue)
            {
                submittedDateExmFrm = $" AND CAST(ISNULL(ARGStudentExamForm_{printProgramme.ToString()}.UpdatedOn, ARGStudentExamForm_{printProgramme.ToString()}.CreatedOn) AS DATE)>='{_PrintCenterNotice.ExmFrmSubmittedFrom.Value.ToString("yyyy-MM-dd")}'";
            }


            string query = "";
            if (_PrintCenterNotice.ListType == ExamFormListType.Regular)
            {
                query = $@"SELECT s.* FROM (SELECT DISTINCT
                            ADMCollegeMaster.CollegeFullName [College FullName],
                            dbo.ARGStudentExamForm_{printProgramme.ToString()}.FormNumber,
                            ARGStudentExamForm_{printProgramme.ToString()}.ExamRollNumber,
                            CourseFullName [Course FullName],
                            ClassRollNo [ClassRollNo],
                            FullName [FullName],
                            Mobile,
                            CUSRegistrationNo [CUSRegistrationNo],
                            ARGStudentExamForm_{printProgramme.ToString()}.Semester,
                            ADMSubjectMaster.SubjectFullName Subject,
                            dbo.FNSubjectTypeDescription(ADMSubjectMaster.SubjectType) SubjectType,
                            ARGSelectedCombination_{printProgramme.ToString()}.SemesterBatch,
                            CombinationCode,
                            CASE WHEN IsDivisionImprovement=1 THEN
                            'Division Improvement'
							ELSE
							'Regular'
							END as FormType,
                            ea.CenterCode,
                            cc.CollegeFullName [Center Location],
                            CAST(ISNULL(ARGStudentExamForm_{printProgramme.ToString()}.UpdatedOn, ARGStudentExamForm_{printProgramme.ToString()}.CreatedOn) AS DATE) SubmittedFrmOn
                            FROM ARGStudentExamForm_{printProgramme.ToString()}
                            JOIN ARGSelectedCombination_{printProgramme.ToString()}
                                ON ARGSelectedCombination_{printProgramme.ToString()}.Student_ID = ARGStudentExamForm_{printProgramme.ToString()}.Student_ID
                            JOIN VWSCMaster
                                ON VWSCMaster.Combination_ID = ARGSelectedCombination_{printProgramme.ToString()}.Combination_ID 
	                            {IncludeCollege}
		                            JOIN ADMCollegeMaster ON ADMCollegeMaster.College_ID = VWSCMaster.College_ID
		                            JOIN ADMSubjectMaster ON ADMSubjectMaster.Subject_ID = VWSCMaster.Subject_ID
                                    JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = VWSCMaster.Course_ID
		                            JOIN dbo.ARGPersonalInformation_{printProgramme.ToString()} ON ARGPersonalInformation_{printProgramme.ToString()}.Student_ID=ARGStudentExamForm_{printProgramme.ToString()}.Student_ID
                                    LEFT JOIN dbo.ARGStudentAddress_{printProgramme.ToString()} ON ARGStudentAddress_{printProgramme.ToString()}.Student_ID=ARGPersonalInformation_{printProgramme.ToString()}.Student_ID
                                    LEFT JOIN dbo.ARGCentersAllotmentMaster ca ON ca.Entity_ID=ARGStudentExamForm_{printProgramme.ToString()}.StudentExamForm_ID
                                    LEFT JOIN dbo.ARGEntranceCentersMaster ea ON ea.Center_ID=ca.Center_ID
                                    LEFT JOIN dbo.ADMCollegeMaster cc ON cc.College_ID = ea.College_ID
                            WHERE ARGSelectedCombination_{printProgramme.ToString()}.Semester={_PrintCenterNotice.Semester}
                             AND ARGStudentExamForm_{printProgramme.ToString()}.Status = {(short)FormStatus.Accepted} 
                            AND IsRegular=1
                            AND ADMSubjectMaster.SubjectType NOT IN({(int)SubjectType.Practical},{(int)SubjectType.Workshop},{(int)SubjectType.Lab})
                            AND ARGStudentExamForm_{printProgramme.ToString()}.Year = {_PrintCenterNotice.ExaminationYear}
                            AND ARGStudentExamForm_{printProgramme.ToString()}.Semester={_PrintCenterNotice.Semester}
                            AND ADMCourseMaster.Programme {LookInProgramme} {submittedDateExmFrm}
                            

                            UNION

                            SELECT DISTINCT
                            ADMCollegeMaster.CollegeFullName [College FullName],
                            dbo.ARGStudentExamForm_{printProgramme.ToString()}.FormNumber,
                            ARGStudentExamForm_{printProgramme.ToString()}.ExamRollNumber,
                            CourseFullName [Course FullName],
                            ClassRollNo [ClassRollNo],
                            p.FullName [FullName],
                            Mobile,
                            CUSRegistrationNo [CUSRegistrationNo],
                            ARGStudentExamForm_{printProgramme.ToString()}.Semester,
                            ADMSubjectMaster.SubjectFullName Subject,
                            dbo.FNSubjectTypeDescription(ADMSubjectMaster.SubjectType) SubjectType,
                            ad.SemesterBatch,
                            ADMCombinationMaster.CombinationCode AS CombinationCode,
                            CASE WHEN IsDivisionImprovement=1 THEN
                            'Division Improvement'
                            ELSE
                            'Regular'
                            END as FormType,
                            ea.CenterCode,
                            cc.CollegeFullName [Center Location],
                            CAST(ISNULL(ARGStudentExamForm_{printProgramme.ToString()}.UpdatedOn, ARGStudentExamForm_{printProgramme.ToString()}.CreatedOn) AS DATE) SubmittedFrmOn
                            FROM ARGStudentExamForm_{printProgramme.ToString()} 
                            JOIN ARGSelectedCombination_{printProgramme.ToString()}  sc
                                ON sc.Student_ID = ARGStudentExamForm_{printProgramme.ToString()}.Student_ID
                            JOIN ADMCombinationMaster ON ADMCombinationMaster.Combination_ID=sc.Combination_ID
                            JOIN dbo.ARGStudentAdditionalSubjects_{printProgramme.ToString()}  ad 
		                            ON ad.Student_ID = ARGStudentExamForm_{printProgramme.ToString()}.Student_ID
		                            AND ad.Semester={_PrintCenterNotice.Semester} AND ARGStudentExamForm_{printProgramme.ToString()}.Semester={_PrintCenterNotice.Semester}
		                            JOIN dbo.ARGPersonalInformation_{printProgramme.ToString()}  p ON p.Student_ID=ARGStudentExamForm_{printProgramme.ToString()}.Student_ID {includeAdCollege}
		                            LEFT JOIN dbo.ADMSubjectMaster asj ON asj.Subject_ID=ad.Subject_ID                            
		                            JOIN ADMCollegeMaster ON ADMCollegeMaster.College_ID = p.AcceptCollege_ID
		                            JOIN ADMSubjectMaster ON ADMSubjectMaster.Subject_ID = ad.Subject_ID
                                    JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = ad.Course_ID		                            
                                    LEFT JOIN dbo.ARGStudentAddress_{printProgramme.ToString()}  ON ARGStudentAddress_{printProgramme.ToString()}.Student_ID=p.Student_ID
                                    LEFT JOIN dbo.ARGCentersAllotmentMaster ca ON ca.Entity_ID=ARGStudentExamForm_{printProgramme.ToString()}.StudentExamForm_ID
                                    LEFT JOIN dbo.ARGEntranceCentersMaster ea ON ea.Center_ID=ca.Center_ID
                                    LEFT JOIN dbo.ADMCollegeMaster cc ON cc.College_ID = ea.College_ID
                            WHERE sc.Semester={_PrintCenterNotice.Semester} AND
                            ARGStudentExamForm_{printProgramme.ToString()}.Status = {(short)FormStatus.Accepted}
                            AND IsRegular=1
                            AND ADMSubjectMaster.SubjectType NOT IN({(int)SubjectType.Practical},{(int)SubjectType.Workshop},{(int)SubjectType.Lab})
                            AND ARGStudentExamForm_{printProgramme.ToString()}.Year = {_PrintCenterNotice.ExaminationYear}
                            AND ARGStudentExamForm_{printProgramme.ToString()}.Semester={_PrintCenterNotice.Semester}
                            AND ADMCourseMaster.Programme {LookInProgramme} {submittedDateExmFrm}) s
                            ORDER BY s.[College FullName] ASC,s.[Course FullName] ASC,
                            s.Subject ASC;";
            }
            else if (_PrintCenterNotice.ListType == ExamFormListType.Backlog)
            {
                if (_PrintCenterNotice.College_ID != Guid.Empty)
                    IncludeCollege = $" AND p.AcceptCollege_ID='{_PrintCenterNotice.College_ID}'";
                query = $@"SELECT DISTINCT
                            ADMCollegeMaster.CollegeFullName [College FullName],
	                        ARGStudentExamForm_{printProgramme.ToString()}.FormNumber,
                            ARGStudentExamForm_{printProgramme.ToString()}.ExamRollNumber,
                            ADMCourseMaster.CourseFullName [Course FullName],
	                        p.ClassRollNo,
                            p.FullName,
                            Mobile,
	                        p.CUSRegistrationNo,
                            ARGStudentExamForm_{printProgramme.ToString()}.Semester,
                            ADMSubjectMaster.SubjectFullName Subject,
                            dbo.FNSubjectTypeDescription(ADMSubjectMaster.SubjectType) SubjectType,
                            ARGSelectedCombination_{printProgramme.ToString()}.SemesterBatch,
                            '' CombinationCode,
                            CASE WHEN IsDivisionImprovement=1 THEN
                            'Division Improvement'
							ELSE
							'Backlog'
							END as FormType,
                            ea.CenterCode,
                            cc.CollegeFullName [Center Location],
                            CAST(ISNULL(ARGStudentExamForm_{printProgramme.ToString()}.UpdatedOn, ARGStudentExamForm_{printProgramme.ToString()}.CreatedOn) AS DATE) SubmittedFrmOn
                        FROM ARGStudentExamForm_{printProgramme.ToString()}
                            JOIN ARGPersonalInformation_{printProgramme.ToString()} p ON p.Student_ID = ARGStudentExamForm_{printProgramme.ToString()}.Student_ID
                            JOIN ARGStudentReExamForm
                                ON ARGStudentReExamForm.StudentExamForm_ID = ARGStudentExamForm_{printProgramme.ToString()}.StudentExamForm_ID
                            JOIN ARGSelectedCombination_{printProgramme.ToString()}
                                ON ARGSelectedCombination_{printProgramme.ToString()}.Student_ID = ARGStudentExamForm_{printProgramme.ToString()}.Student_ID
                            AND ARGSelectedCombination_{printProgramme.ToString()}.Semester={_PrintCenterNotice.Semester}
                            JOIN ADMSubjectMaster
                                ON ADMSubjectMaster.Subject_ID = ARGStudentReExamForm.Subject_ID
                            JOIN ADMCollegeMaster
                                ON ADMCollegeMaster.College_ID = p.AcceptCollege_ID
                            JOIN ADMCourseMaster
                                ON ADMCourseMaster.Course_ID = ADMSubjectMaster.Course_ID
                            LEFT JOIN dbo.ARGStudentAddress_{printProgramme.ToString()} ON ARGStudentAddress_{printProgramme.ToString()}.Student_ID=p.Student_ID
                            LEFT JOIN dbo.ARGCentersAllotmentMaster ca ON ca.Entity_ID=ARGStudentExamForm_{printProgramme.ToString()}.StudentExamForm_ID
                            LEFT JOIN dbo.ARGEntranceCentersMaster ea ON ea.Center_ID=ca.Center_ID
                            LEFT JOIN dbo.ADMCollegeMaster cc ON cc.College_ID = ea.College_ID
                        WHERE IsRegular = 0
                            AND ARGStudentExamForm_{printProgramme.ToString()}.Status = {(short)FormStatus.Accepted}
                            AND ADMSubjectMaster.SubjectType NOT IN({(int)SubjectType.Practical},{(int)SubjectType.Workshop},{(int)SubjectType.Lab})
                            AND ARGStudentExamForm_{printProgramme.ToString()}.Year = @Year
                            AND ARGStudentExamForm_{printProgramme.ToString()}.Semester = @Semester
	                        AND ARGStudentReExamForm.FeeStatus ={(short)FormStatus.FeePaid}
                            AND ADMCourseMaster.Programme {LookInProgramme} {IncludeCollege} {submittedDateExmFrm} 
                        ORDER BY ADMCollegeMaster.CollegeFullName,
                                    SemesterBatch,
                                    Subject;";
            }

            SqlCommand command = new SqlCommand(query);
            command.Parameters.AddWithValue("@Semester", _PrintCenterNotice.Semester);
            command.Parameters.AddWithValue("@Year", _PrintCenterNotice.ExaminationYear);
            return new MSSQLFactory().GetDataTable(command);
        }
        #endregion

        #region AssignRollNosBulk
        public List<DropDownOptLabelGeneral> SelectExamFormsSettingsDDL()
        {
            return new MSSQLFactory().GetObjectList<DropDownOptLabelGeneral>(@"SELECT Cast(ARGExamForm_ID AS NVARCHAR(40)) AS Value,
                                                                            (CASE WHEN IsBacklogSetting=1 THEN 'Backlog :  FormPrefix= EXMBK' + CourseCategory+SUBSTRING(CAST(Year AS VARCHAR(4)),3,2)
	                                                                            ELSE 'Regular : FormPrefix= EXM' + CourseCategory+SUBSTRING(CAST(Year AS VARCHAR(4)),3,2) END +
                                                                                ', Semester-' + CAST(Semester AS NVARCHAR(2)) +                                                                          
                                                                            ' , '+ CAST(Year AS NVARCHAR(4))) AS Text,CourseCategory AS DataGroupField
                                                                            FROM ARGExamFormDownloadable WHERE AllowInCenterAllotment=1
                                                                            ORDER BY CourseCategory,IsBacklogSetting ASC,Year DESC,Semester DESC;");
        }

        public bool CheckIfRollnosAreAlreadyAssigned(ARGExamFormDownloadable formDownloadable)
        {
            PrintProgramme printProgramme = new GeneralFunctions().MappingTable(formDownloadable.PrintProgramme);
            return new MSSQLFactory().ExecuteScalar<int>($@"SELECT COUNT(FormNumber) FROM ARGStudentExamForm_{printProgramme.ToString()} 
                                                WHERE FormNumber LIKE '{formDownloadable.FormNumberPrefix}%' AND Semester={formDownloadable.Semester} AND Year={formDownloadable.Year} AND ExamRollNumber IS NOT NULL") > 0;
        }


        public bool CheckRollNosAlreadyAssignedForCourse(ARGExamFormDownloadable formDownloadable, Guid Course_ID, Guid AcceptedCollege_ID)
        {
            PrintProgramme printProgramme = new GeneralFunctions().MappingTable(formDownloadable.PrintProgramme);

            string query = $@"SELECT COUNT(ExamRollNumber) FROM ARGPersonalInformation_{printProgramme.ToString()}
		                        JOIN ARGSelectedCombination_{printProgramme.ToString()} ON ARGSelectedCombination_{printProgramme.ToString()}.Student_ID = ARGPersonalInformation_{printProgramme.ToString()}.Student_ID
		                        JOIN ARGStudentExamForm_{printProgramme.ToString()} ON ARGStudentExamForm_{printProgramme.ToString()}.Student_ID = ARGPersonalInformation_{printProgramme.ToString()}.Student_ID AND ARGStudentExamForm_{printProgramme.ToString()}.Semester=ARGSelectedCombination_{printProgramme.ToString()}.Semester
		                        JOIN PaymentDetails_{printProgramme.ToString()} ON Entity_ID=ARGStudentExamForm_{printProgramme.ToString()}.StudentExamForm_ID
                                JOIN ADMCombinationMaster ON ADMCombinationMaster.Combination_ID = ARGSelectedCombination_{printProgramme.ToString()}.Combination_ID 
		                        JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = ADMCombinationMaster.Course_ID
		                        WHERE ADMCombinationMaster.Course_ID='{Course_ID}' AND AcceptCollege_ID='{AcceptedCollege_ID}'
		                        AND ARGStudentExamForm_{printProgramme.ToString()}.Semester={formDownloadable.Semester} 
                                AND ARGStudentExamForm_{printProgramme.ToString()}.Year={formDownloadable.Year} 
                                AND ARGStudentExamForm_{printProgramme.ToString()}.Status={(short)FormStatus.Accepted}	
                                AND	ARGStudentExamForm_{printProgramme.ToString()}.IsRegular=1 AND ExamRollNumber IS NOT NULL;";

            return new MSSQLFactory().ExecuteScalar<int>(query) <= 0;
        }


        public List<string> AssingBulkRollNosTSQLScript(ARGExamFormDownloadable formDownloadable)
        {
            PrintProgramme printProgramme = new GeneralFunctions().MappingTable(formDownloadable.PrintProgramme);
            string IsForPrintProgramme = $"PrintProgramme ={(short)printProgramme}";

            if (formDownloadable.CourseCategory == ExaminationCourseCategory.ENG)
                IsForPrintProgramme = $"Programme ={(short)Programme.Engineering}";
            if (formDownloadable.CourseCategory == ExaminationCourseCategory.IH)
                IsForPrintProgramme = $"PrintProgramme ={(short)PrintProgramme.IH} AND Programme<>{(short)Programme.Engineering}";
            if (formDownloadable.CourseCategory == ExaminationCourseCategory.BED)
                IsForPrintProgramme = $"PrintProgramme ={(short)PrintProgramme.BED}";

            string query = $@"SELECT 
                        'WITH UpdateData  As
                        (
	                        SELECT StudentFormNo,tab.FormNumber,
	                        ROW_NUMBER() OVER (ORDER BY tab.FormNumber,tab.Status ASC) AS RN from
	                        (
		                        SELECT DISTINCT ARGPersonalInformation_{printProgramme.ToString()}.Student_ID,ADMCombinationMaster.Course_ID,CourseFullName,StudentFormNo,ARGStudentExamForm_{printProgramme.ToString()}.FormNumber,ARGStudentExamForm_{printProgramme.ToString()}.Status FROM ARGPersonalInformation_{printProgramme.ToString()}
		                        JOIN ARGSelectedCombination_{printProgramme.ToString()} ON ARGSelectedCombination_{printProgramme.ToString()}.Student_ID = ARGPersonalInformation_{printProgramme.ToString()}.Student_ID
		                        JOIN ARGStudentExamForm_{printProgramme.ToString()} ON ARGStudentExamForm_{printProgramme.ToString()}.Student_ID = ARGPersonalInformation_{printProgramme.ToString()}.Student_ID AND ARGStudentExamForm_{printProgramme.ToString()}.Semester=ARGSelectedCombination_{printProgramme.ToString()}.Semester
		                        JOIN PaymentDetails_{printProgramme.ToString()} ON Entity_ID=ARGStudentExamForm_{printProgramme.ToString()}.StudentExamForm_ID
                                JOIN ADMCombinationMaster ON ADMCombinationMaster.Combination_ID = ARGSelectedCombination_{printProgramme.ToString()}.Combination_ID 
		                        JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = ADMCombinationMaster.Course_ID
		                        WHERE ADMCombinationMaster.Course_ID=''$'+CAST(ADMCourseMaster.Course_ID AS NVARCHAR(40))+'$'' AND AcceptCollege_ID=''&'+CAST(ADMCollegeCourseMapping.College_ID AS NVARCHAR(40))+'&''
		                        AND ARGStudentExamForm_{printProgramme.ToString()}.Semester={formDownloadable.Semester} AND ARGStudentExamForm_{printProgramme.ToString()}.Year={formDownloadable.Year} AND ARGStudentExamForm_{printProgramme.ToString()}.Status={(short)FormStatus.Accepted}	AND	ARGStudentExamForm_{printProgramme.ToString()}.IsRegular=1
	                        ) 
                        tab)	
                        UPDATE ARGStudentExamForm_{printProgramme.ToString()}
                        SET ARGStudentExamForm_{printProgramme.ToString()}.ExamRollNumber = CAST((## + RN) AS NVARCHAR(10))
                        FROM ARGStudentExamForm_{printProgramme.ToString()} d INNER JOIN UpdateData ON UpdateData.FormNumber =d.FormNumber;'
                        FROM ADMCourseMaster
                         JOIN ADMCollegeCourseMapping ON ADMCollegeCourseMapping.Course_ID = ADMCourseMaster.Course_ID
                        JOIN ADMCollegeMaster ON ADMCollegeMaster.College_ID = ADMCollegeCourseMapping.College_ID
                        WHERE {IsForPrintProgramme} AND ADMCourseMaster.Status=1  AND ADMCollegeMaster.Status=1 ORDER BY ADMCollegeMaster.CollegeFullName ASC";
            return new MSSQLFactory().GetSingleValues<string>(query);
        }

        public int AssingBulkRollNosForReAppear(ARGExamFormDownloadable _ExamSettings)
        {
            PrintProgramme printProgramme = new GeneralFunctions().MappingTable(_ExamSettings.PrintProgramme);
            string query = $@"UPDATE ToUpdateTabe 
                            SET ToUpdateTabe.ExamRollNumber=REPLACE(REPLACE(RTRIM(LTRIM(SourceTable.rollno)),'N',''),'t','')
                            FROM ARGStudentExamForm_{printProgramme.ToString()} ToUpdateTabe
                            JOIN (SELECT DISTINCT t.* FROM (
                            SELECT Student_ID,ARGStudentExamForm_{printProgramme.ToString()}.Semester,ROW_NUMBER()  OVER(PARTITION BY Student_ID ORDER BY ARGStudentExamForm_{printProgramme.ToString()}.Semester DESC) RowNumber,
                            MAX(ExamRollNumber) rollno,MAX(StudentExamForm_ID) StudentExamForm_ID
                            FROM ARGStudentExamForm_{printProgramme.ToString()}
                            WHERE ExamRollNumber IS NOT NULL AND ARGStudentExamForm_{printProgramme.ToString()}.Semester <>{_ExamSettings.Semester} 
                            GROUP BY Student_ID,ARGStudentExamForm_{printProgramme.ToString()}.Semester )  t  WHERE t.RowNumber=1) SourceTable ON SourceTable.Student_ID = ToUpdateTabe.Student_ID
                            WHERE ToUpdateTabe.Semester={_ExamSettings.Semester} AND ToUpdateTabe.FormNumber LIKE '{_ExamSettings.FormNumberPrefix}%' AND ToUpdateTabe.Status={(short)FormStatus.Accepted} AND ToUpdateTabe.Year={_ExamSettings.Year};";
            return new MSSQLFactory().ExecuteNonQuery(query);
        }

        public int CreateNewRollNos(string TSQLCommand)
        {
            return new MSSQLFactory().ExecuteNonQuery(TSQLCommand);
        }

        public short GetBatch(ARGExamFormDownloadable _ExamSettings)
        {
            PrintProgramme printProgramme = new GeneralFunctions().MappingTable(_ExamSettings.PrintProgramme);
            string query = $@"SELECT TOP 1 Batch FROM ARGStudentExamForm_{printProgramme.ToString()}
                            JOIN ARGPersonalInformation_{printProgramme.ToString()} ON ARGPersonalInformation_{printProgramme.ToString()}.Student_ID = ARGStudentExamForm_{printProgramme.ToString()}.Student_ID
                            WHERE FormNumber LIKE '{_ExamSettings.FormNumberPrefix}%' AND Semester={_ExamSettings.Semester} AND Year={_ExamSettings.Year} ORDER BY Batch DESC";
            return new MSSQLFactory().ExecuteScalar<short>(query);
        }

        public string GetMaximumRollNoOfParticularbatch(short Semester, short Batch)
        {
            string query = $@"SELECT MAX(ARGStudentExamForm.ExamRollNumber) FROM 
                                (SELECT REPLACE(REPLACE(RTRIM(LTRIM(ExamRollNumber)),'N',''),'t','') ExamRollNumber FROM ARGStudentExamForm_PG 
                                JOIN ARGPersonalInformation_PG ON ARGPersonalInformation_PG.Student_ID = ARGStudentExamForm_PG.Student_ID
                                WHERE ExamRollNumber IS NOT NULL AND Semester ={Semester} AND Batch={Batch}
                                UNION 
                                SELECT REPLACE(REPLACE(RTRIM(LTRIM(ExamRollNumber)),'N',''),'t','') ExamRollNumber FROM ARGStudentExamForm_UG 
                                JOIN ARGPersonalInformation_UG ON ARGPersonalInformation_UG.Student_ID = ARGStudentExamForm_UG.Student_ID
                                WHERE ExamRollNumber IS NOT NULL AND Semester ={Semester} AND Batch={Batch}
                                UNION 
                                SELECT REPLACE(REPLACE(RTRIM(LTRIM(ExamRollNumber)),'N',''),'t','') ExamRollNumber FROM ARGStudentExamForm_IH 
                                JOIN ARGPersonalInformation_IH ON ARGPersonalInformation_IH.Student_ID = ARGStudentExamForm_IH.Student_ID
                                WHERE ExamRollNumber IS NOT NULL AND Semester ={Semester} AND Batch={Batch}) ARGStudentExamForm ";
            return new MSSQLFactory().ExecuteScalar<string>(query);
        }

        public string GetMaximumRollNoOfParticularbatch(short Batch)
        {
            string query = $@"SELECT MAX(ARGStudentExamForm.rn) FROM
                            (SELECT REPLACE(REPLACE(RTRIM(LTRIM(ExamRollNumber)),'N',''),'t','') rn,ARGStudentExamForm_UG.Student_ID FROM ARGStudentExamForm_UG
                            JOIN ARGPersonalInformation_UG ON ARGPersonalInformation_UG.Student_ID = ARGStudentExamForm_UG.Student_ID AND Batch={Batch}
                            WHERE ExamRollNumber IS NOT NULL 
                            UNION
                            SELECT REPLACE(REPLACE(RTRIM(LTRIM(ExamRollNumber)),'N',''),'t','') rn,ARGStudentExamForm_PG.Student_ID FROM ARGStudentExamForm_PG
                            JOIN ARGPersonalInformation_PG ON ARGPersonalInformation_PG.Student_ID = ARGStudentExamForm_PG.Student_ID AND Batch={Batch}
                            WHERE ExamRollNumber IS NOT NULL 
                            UNION
                            SELECT REPLACE(REPLACE(RTRIM(LTRIM(ExamRollNumber)),'N',''),'t','') rn,ARGStudentExamForm_IH.Student_ID FROM ARGStudentExamForm_IH
                            JOIN ARGPersonalInformation_IH ON ARGPersonalInformation_IH.Student_ID = ARGStudentExamForm_IH.Student_ID AND Batch={Batch}
                            WHERE ExamRollNumber IS NOT NULL) ARGStudentExamForm";
            return new MSSQLFactory().ExecuteScalar<string>(query);
        }

        public List<PendingExamRollNos> GetPendingRollNoCandidates(ARGExamFormDownloadable _ExamSettings)
        {
            PrintProgramme printProgramme = new GeneralFunctions().MappingTable(_ExamSettings.PrintProgramme);
            string query = $@"SELECT  StudentExamForm_ID,ARGStudentExamForm_{printProgramme.ToString()}.Student_ID,Course_ID,Batch,AcceptCollege_ID FROM ARGStudentExamForm_{printProgramme.ToString()} 
                                JOIN VWStudentCourse ON VWStudentCourse.Student_ID = ARGStudentExamForm_{printProgramme.ToString()}.Student_ID
                                JOIN PaymentDetails_{printProgramme.ToString()} ON Entity_ID=ARGStudentExamForm_{printProgramme.ToString()}.StudentExamForm_ID
                                WHERE ExamRollNumber IS NULL AND Status={(short)FormStatus.Accepted} AND ARGStudentExamForm_{printProgramme.ToString()}.Semester={_ExamSettings.Semester} AND FormNumber  LIKE '{_ExamSettings.FormNumberPrefix}%' AND Year={_ExamSettings.Year} ORDER BY AcceptCollege_ID,Course_ID";
            return new MSSQLFactory().GetObjectList<PendingExamRollNos>(query);
        }

        public string GetPendingMAXRollNo(PrintProgramme printProgramme, PendingExamRollNos pendingExamRollNo)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            string query = $@"SELECT  MAX(REPLACE(REPLACE(RTRIM(LTRIM(ExamRollNumber)),'N',''),'t','')) FROM ARGStudentExamForm_{printProgramme.ToString()} 
                            JOIN VWStudentCourse ON VWStudentCourse.Student_ID = ARGStudentExamForm_{printProgramme.ToString()}.Student_ID
                            WHERE ExamRollNumber IS NOT NULL AND Course_ID='{pendingExamRollNo.Course_ID}' AND 
                            AcceptCollege_ID='{pendingExamRollNo.AcceptCollege_ID}' AND Batch={pendingExamRollNo.Batch}";

            if (pendingExamRollNo.Batch == 2017)
            {
                query = $@"SELECT MAX(ARGStudentExamForm.rn) FROM
                        ( SELECT REPLACE(REPLACE(RTRIM(LTRIM(ExamRollNumber)),'N',''),'t','') rn,Student_ID FROM ARGStudentExamForm_UG
                        WHERE ExamRollNumber IS NOT NULL AND REPLACE(REPLACE(RTRIM(LTRIM(ExamRollNumber)),'N',''),'t','') NOT LIKE '18%'
                        UNION
                        SELECT REPLACE(REPLACE(RTRIM(LTRIM(ExamRollNumber)),'N',''),'t','') rn,Student_ID FROM ARGStudentExamForm_PG
                        WHERE ExamRollNumber IS NOT NULL AND REPLACE(REPLACE(RTRIM(LTRIM(ExamRollNumber)),'N',''),'t','') NOT LIKE '18%'
                        UNION
                        SELECT REPLACE(REPLACE(RTRIM(LTRIM(ExamRollNumber)),'N',''),'t','') rn,Student_ID FROM ARGStudentExamForm_IH
                        WHERE ExamRollNumber IS NOT NULL AND REPLACE(REPLACE(RTRIM(LTRIM(ExamRollNumber)),'N',''),'t','') NOT LIKE '18%') ARGStudentExamForm
                        JOIN VWStudentCourse ON VWStudentCourse.Student_ID = ARGStudentExamForm.Student_ID WHERE Batch=2017";
            }

            return new MSSQLFactory().ExecuteScalar<string>(query);
        }


        public int AssignPednignRollNo(PrintProgramme printProgramme, string ExamRollNo, Guid StudentExamForm_ID)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            return new MSSQLFactory().ExecuteNonQuery($"UPDATE ARGStudentExamForm_{printProgramme.ToString()} SET ExamRollNumber='{ExamRollNo}' WHERE StudentExamForm_ID='{StudentExamForm_ID}' AND ExamRollNumber IS NULL");
        }

        public bool CheckIfRollNoAlreadyExists(Guid Student_ID, short Batch, string NewExamRollNo)
        {
            string query = $@"SELECT COUNT(ARGStudentExamForm.rn) FROM
                    (SELECT REPLACE(REPLACE(RTRIM(LTRIM(ExamRollNumber)),'N',''),'t','') rn,ARGStudentExamForm_UG.Student_ID FROM ARGStudentExamForm_UG
                    JOIN ARGPersonalInformation_UG ON ARGPersonalInformation_UG.Student_ID = ARGStudentExamForm_UG.Student_ID
                    WHERE ExamRollNumber IS NOT NULL 
                    UNION
                    SELECT REPLACE(REPLACE(RTRIM(LTRIM(ExamRollNumber)),'N',''),'t','') rn,ARGStudentExamForm_PG.Student_ID FROM ARGStudentExamForm_PG
                    JOIN ARGPersonalInformation_PG ON ARGPersonalInformation_PG.Student_ID = ARGStudentExamForm_PG.Student_ID
                    WHERE ExamRollNumber IS NOT NULL 
                    UNION
                    SELECT REPLACE(REPLACE(RTRIM(LTRIM(ExamRollNumber)),'N',''),'t','') rn,ARGStudentExamForm_IH.Student_ID FROM ARGStudentExamForm_IH
                    JOIN ARGPersonalInformation_IH ON ARGPersonalInformation_IH.Student_ID = ARGStudentExamForm_IH.Student_ID
                    WHERE ExamRollNumber IS NOT NULL) ARGStudentExamForm
                    WHERE ARGStudentExamForm.Student_ID<> '{Student_ID}' AND ARGStudentExamForm.rn='{NewExamRollNo}'";

            return new MSSQLFactory().ExecuteScalar<int>(query) > 0;
        }

        #endregion


        #region AssignCentersInBulk

        public List<SelectListItem> GetCoursesDDL(PrintProgramme programme, Guid studentesOfCollege_ID)
        {
            return new MSSQLFactory().GetObjectList<SelectListItem>($@"SELECT DISTINCT CAST(ADMCollegeCourseMapping.Course_ID AS VARCHAR(50)) AS Value,CourseFullName AS Text FROM ADMCollegeCourseMapping
                                        JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = ADMCollegeCourseMapping.Course_ID AND ADMCollegeCourseMapping.Status=1
                                        WHERE PrintProgramme={(short)programme} AND College_ID='{studentesOfCollege_ID}' AND ADMCourseMaster.Status=1 ORDER BY CourseFullName ASC");
        }

        public List<AssignCentersAs> GetDispalyAvaliableCenters(Guid College_ID)
        {
            return new MSSQLFactory().GetObjectList<AssignCentersAs>($@"SELECT Center_ID,
                                                       CenterCode,
                                                       CASE
                                                           WHEN
                                                           (
                                                               SELECT COUNT(ARGCentersAllotmentMaster.CenterAllowment_ID)
                                                               FROM ARGCentersAllotmentMaster
                                                               WHERE Center_ID = ARGEntranceCentersMaster.Center_ID
                                                           ) > 0 THEN
                                                               1
                                                           ELSE
                                                               0
                                                       END AS HasAlreadySomeStudents
                                                FROM ARGEntranceCentersMaster
                                                WHERE College_ID = '{College_ID}'
                                                      AND Status = 1 AND  ARGEntranceCentersMaster.IsEntrance=0
                                                ORDER BY CenterCode;");
        }


        public int GetExamFormsAvailable(Guid College_ID, short semester, short year, Guid course_ID, PrintProgramme category, SemesterCentersCategory semesterCenters)
        {
            category = new GeneralFunctions().MappingTable(category);
            string ExamCategory = semesterCenters == SemesterCentersCategory.All
               ? ""
               : $" AND ARGStudentExamForm_{category.ToString()}.IsRegular={(semesterCenters == SemesterCentersCategory.RegularOnly ? "1" : "0")}";

            return new MSSQLFactory().ExecuteScalar<int>($@"SELECT COUNT(StudentExamForm_ID) FROM VWStudentCourse
                            JOIN ARGStudentExamForm_{category.ToString()} ON ARGStudentExamForm_{category.ToString()}.Student_ID = VWStudentCourse.Student_ID
                            WHERE AcceptCollege_ID='{College_ID}' AND Course_ID='{course_ID}' 
                            AND ARGStudentExamForm_{category.ToString()}.Semester={semester} AND Year={year} AND ARGStudentExamForm_{category.ToString()}.Status=4
                            {ExamCategory}");
        }

        public bool CheckIfCentersAlreadyAlloted(Guid StudentesOfCollege_ID, short semester, short year, Guid Course_ID, PrintProgramme category, SemesterCentersCategory semesterCenters)
        {
            category = new GeneralFunctions().MappingTable(category);

            string ExamCategory = semesterCenters == SemesterCentersCategory.All
              ? ""
              : $" AND ARGStudentExamForm_{category.ToString()}.IsRegular={(semesterCenters == SemesterCentersCategory.RegularOnly ? "1" : "0")}";

            return new MSSQLFactory().ExecuteScalar<int>($@"SELECT COUNT(CenterAllowment_ID) FROM ARGEntranceCentersMaster
                                                JOIN ARGCentersAllotmentMaster ON ARGCentersAllotmentMaster.Center_ID = ARGEntranceCentersMaster.Center_ID
                                                JOIN ARGStudentExamForm_{category.ToString()} ON ARGStudentExamForm_{category.ToString()}.StudentExamForm_ID=ARGCentersAllotmentMaster.Entity_ID
                                                JOIN ARGPersonalInformation_{category.ToString()} ON ARGPersonalInformation_{category.ToString()}.Student_ID = ARGStudentExamForm_{category.ToString()}.Student_ID
                                                WHERE AcceptCollege_ID='{StudentesOfCollege_ID}' AND Course_ID='{Course_ID}' 
                                                AND Semester={semester} AND Year={year}{ExamCategory}") > 0;
        }

        public int AssignCentersInBulk(AssignCentersInBulk assignCentersInBulk)
        {
            assignCentersInBulk.PrintProgramme = new GeneralFunctions().MappingTable(assignCentersInBulk.PrintProgramme);
            string ExamCategory = assignCentersInBulk.SemesterCenterCategory == SemesterCentersCategory.All
                ? ""
                : $" AND ARGStudentExamForm_{assignCentersInBulk.PrintProgramme.ToString()}.IsRegular={(assignCentersInBulk.SemesterCenterCategory == SemesterCentersCategory.RegularOnly ? "1" : "0")}";


            #region TSql
            string sqlStatement = $@"INSERT INTO ARGCentersAllotmentMaster
                                    (
                                        CenterAllowment_ID,
                                        Entity_ID,
                                        Course_ID,
                                        Center_ID,
                                        EntranceDate,
                                        Time,
                                        Appeared,
                                        CreatedOn,
                                        CreatedBy,
                                        UpdatedOn,
                                        UpdatedBy,
                                        final
                                    ) 
                                    SELECT NEWID(),  ARGStudentExamForm_{assignCentersInBulk.PrintProgramme.ToString()}.StudentExamForm_ID, ADMCombinationMaster.Course_ID,@Center_ID, 
                                    '{DateTime.Now.Date.ToString("yyyy-MM-dd")}', N'2:00 PM', 0, GETDATE(),'{assignCentersInBulk.User_ID}',  NULL, NULL,0 
                                    FROM ARGPersonalInformation_{assignCentersInBulk.PrintProgramme.ToString()}
                                        JOIN ARGSelectedCombination_{assignCentersInBulk.PrintProgramme.ToString()}
                                            ON ARGSelectedCombination_{assignCentersInBulk.PrintProgramme.ToString()}.Student_ID = ARGPersonalInformation_{assignCentersInBulk.PrintProgramme.ToString()}.Student_ID
                                        JOIN ARGStudentExamForm_{assignCentersInBulk.PrintProgramme.ToString()}
                                            ON ARGStudentExamForm_{assignCentersInBulk.PrintProgramme.ToString()}.Student_ID = ARGPersonalInformation_{assignCentersInBulk.PrintProgramme.ToString()}.Student_ID
                                               AND ARGStudentExamForm_{assignCentersInBulk.PrintProgramme.ToString()}.Semester = ARGSelectedCombination_{assignCentersInBulk.PrintProgramme.ToString()}.Semester
                                        JOIN ADMCombinationMaster
                                            ON ADMCombinationMaster.Combination_ID = ARGSelectedCombination_{assignCentersInBulk.PrintProgramme.ToString()}.Combination_ID
                                        JOIN ADMCourseMaster
                                            ON ADMCourseMaster.Course_ID = ADMCombinationMaster.Course_ID
                                    WHERE ADMCombinationMaster.Course_ID = @Course_ID
                                          AND AcceptCollege_ID = @AcceptCollege_ID
                                          AND ARGStudentExamForm_{assignCentersInBulk.PrintProgramme.ToString()}.Semester = @Semester
                                          AND ARGStudentExamForm_{assignCentersInBulk.PrintProgramme.ToString()}.Year = @Year
                                          AND ExamRollNumber IS NOT NULL 
                                          AND ARGStudentExamForm_{assignCentersInBulk.PrintProgramme.ToString()}.Status = 4 
                                          {ExamCategory}
                                    ORDER BY ExamRollNumber ASC,ARGSelectedCombination_{assignCentersInBulk.PrintProgramme.ToString()}.Combination_ID ASC OFFSET {assignCentersInBulk.OFFSETCount} ROWS FETCH NEXT {assignCentersInBulk.FetchNextCount} ROWS ONLY;";
            #endregion

            SqlCommand cmd = new SqlCommand(sqlStatement);
            cmd.Parameters.AddWithValue("@Center_ID", assignCentersInBulk.Center_ID);
            cmd.Parameters.AddWithValue("@Course_ID", assignCentersInBulk.Course_ID);
            cmd.Parameters.AddWithValue("@AcceptCollege_ID", assignCentersInBulk.StudentesOfCollege_ID);
            cmd.Parameters.AddWithValue("@Year", assignCentersInBulk.Year);

            cmd.Parameters.AddWithValue("@Semester", assignCentersInBulk.Semester);
            return new MSSQLFactory().ExecuteNonQuery(cmd);
        }

        public int DeleteDataFromCenterAllotmentSemDB()
        {
            int year = DateTime.Now.Year - 1;
            return new MSSQLFactory().ExecuteNonQuery($@"IF EXISTS(SELECT ARGExamForm_ID FROM ARGExamFormDownloadable WHERE AllowDownloadAdmitCards=1)
                                                        BEGIN
                                                            SELECT -1
                                                        END
                                                        ELSE
                                                        BEGIN
                                                            DELETE ARGCentersAllotmentMaster FROM ARGCentersAllotmentMaster
                                                            JOIN (
                                                            SELECT StudentExamForm_ID FROM ARGStudentExamForm_IH WHERE Year>={year}
                                                            UNION 
                                                            SELECT StudentExamForm_ID FROM ARGStudentExamForm_PG WHERE Year>={year}
                                                            UNION 
                                                            SELECT StudentExamForm_ID FROM ARGStudentExamForm_UG WHERE Year>={year}) ARGStudentExamForm
                                                            ON ARGStudentExamForm.StudentExamForm_ID=Entity_ID
                                                        END");
        }

        public int DeleteDataFromCenterAllotmentEntranceDB()
        {
            return new MSSQLFactory().ExecuteNonQuery($@"IF EXISTS(SELECT PrintProgramme FROM ARGDownloadAdmitCards WHERE AllowDownloadAdmitCards=1)
                                                        BEGIN
                                                            SELECT -1
                                                        END
                                                        ELSE
                                                        BEGIN
                                                            DELETE FROM dbo.ARGCentersAllotmentMaster WHERE Center_ID IN (
                                                            SELECT Center_ID FROM dbo.ARGEntranceCentersMaster WHERE IsEntrance=1)
                                                        END");
        }
        #endregion


        #region IndividualCenterAllotment

        public string GetExamRollNo(Guid Student_ID, PrintProgramme PrintProgramme)
        {
            PrintProgramme = new GeneralFunctions().MappingTable(PrintProgramme);
            string TSQL = $@"SELECT DISTINCT REPLACE(REPLACE(RTRIM(LTRIM(t.ExamRollNo)),'N',''),'t','') ExamRollNo  FROM (
                            SELECT Student_ID,Semester,ROW_NUMBER()  OVER(PARTITION BY Student_ID ORDER BY Semester DESC) RowNumber,
                            MAX(ExamRollNumber) ExamRollNo,MAX(StudentExamForm_ID) StudentExamForm_ID
                            FROM ARGStudentExamForm_{PrintProgramme.ToString()}
                            WHERE Student_ID=@Student_ID AND ExamRollNumber IS NOT NULL
                            GROUP BY Student_ID,Semester ) t  WHERE t.RowNumber=1";
            SqlCommand cmd = new SqlCommand(TSQL);
            cmd.Parameters.AddWithValue("@Student_ID", Student_ID);
            return new MSSQLFactory().ExecuteScalar<string>(cmd);
        }

        public PendingExamRollNos GetStudentInformationByExamFormID(string ExamFormNo, PrintProgramme printProgramme)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            string TSQL = $@"SELECT StudentExamForm_ID,ARGStudentExamForm_{printProgramme.ToString()}.Student_ID,Course_ID,Batch,AcceptCollege_ID,ARGStudentExamForm_{printProgramme.ToString()}.Semester FROM ARGStudentExamForm_{printProgramme.ToString()} 
                                JOIN VWStudentCourse ON VWStudentCourse.Student_ID = ARGStudentExamForm_{printProgramme.ToString()}.Student_ID
                                WHERE FormNumber =@FormNumber";

            SqlCommand cmd = new SqlCommand(TSQL);
            cmd.Parameters.AddWithValue("@FormNumber", ExamFormNo);
            return new MSSQLFactory().GetObject<PendingExamRollNos>(cmd);

        }
        public IndividualSemDetails GetPersonalInformationByExamFormID(string ExamFormNo, PrintProgramme printProgramme)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            string TSQL = $@"SELECT p.FullName,
                                   CASE
                                       WHEN (a.Payment_ID IS NULL) THEN
                                           {(short)FormStatus.InProcess}
                                       ELSE
                                           {(short)FormStatus.Accepted}
                                   END AS FormStatus,
                                   p.CourseFullName,
                                   p.CollegeFullName,
                                   e.StudentExamForm_ID,
                                   p.Course_ID,
                                   e.Student_ID,
                                   a.Payment_ID
                            FROM VWStudentCourse p
                                JOIN ARGStudentExamForm_{printProgramme.ToString()} e
                                    ON e.Student_ID = p.Student_ID
                                LEFT JOIN PaymentDetails_{printProgramme.ToString()} a
                                    ON a.Entity_ID = e.StudentExamForm_ID
                            WHERE FormNumber = @FormNumber
                                  AND e.Status = {(short)FormStatus.Accepted};";

            SqlCommand cmd = new SqlCommand(TSQL);
            cmd.Parameters.AddWithValue("@FormNumber", ExamFormNo);
            return new MSSQLFactory().GetObject<IndividualSemDetails>(cmd);

        }
        public bool CheckIfCenterAlreadyAssigned(Guid StudentExamForm_ID)
        {
            string TSQL = $@"SELECT COUNT(CenterAllowment_ID) FROM ARGCentersAllotmentMaster 
			WHERE Entity_ID='{StudentExamForm_ID}'";

            SqlCommand cmd = new SqlCommand(TSQL);
            return new MSSQLFactory().ExecuteScalar<int>(cmd) > 0;

        }
        public int SaveIndividualCenter(ARGCentersAllotmentMaster aRGCentersAllotmentMaster)
        {
            return new MSSQLFactory().InsertRecord(aRGCentersAllotmentMaster);
        }

        public int UpdateIndividualExamRollNo(string examRollNo, PrintProgramme printProgramme, Guid StudentExamForm_ID)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            return new MSSQLFactory().ExecuteNonQuery($"UPDATE ARGStudentExamForm_{printProgramme.ToString()} SET ExamRollNumber='{examRollNo}' WHERE StudentExamForm_ID='{StudentExamForm_ID}'");
        }

        public int DeleteCenterOfIndividual(string ExamFormNo, PrintProgramme printProgramme)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            SqlCommand cmd = new SqlCommand($@"DELETE c
                                                FROM dbo.ARGCentersAllotmentMaster c
                                                INNER JOIN dbo.ARGStudentExamForm_{printProgramme.ToString()} e
                                                  ON c.Entity_ID=e.StudentExamForm_ID
                                                WHERE e.FormNumber=@FormNumber");
            cmd.Parameters.AddWithValue("@FormNumber", ExamFormNo + "");
            return new MSSQLFactory().ExecuteNonQuery(cmd);
        }

        #endregion

        #region Bulk Admit Card
        public List<AttendanceSheetForm> GetAllStudentsForBulkAdmitCards(PrintProgramme programme, Guid Course_ID, int Batch, int Semester, Guid College_ID)
        {
            return new MSSQLFactory().GetObjectList<AttendanceSheetForm>(GetAllStudentsForBulkAdmitCardsQuery(programme, Course_ID, Batch, Semester, College_ID));
        }

        public SqlCommand GetAllStudentsForBulkAdmitCardsQuery(PrintProgramme programme, Guid Course_ID, int Batch, int Semester, Guid College_ID)
        {
            string tablePostFix = new GeneralFunctions().GetProgrammePostFix(programme);
            string ExamRollNoSQL = string.Empty;
            string IncludeCourseSQL = string.Empty;

            SqlCommand command = new SqlCommand();
            command.Parameters.AddWithValue("@College_ID", College_ID);
            if (Course_ID != Guid.Empty)
            {
                command.Parameters.AddWithValue("@Course_ID", Course_ID);
                IncludeCourseSQL = " AND ADMCourseMaster.Course_ID=@Course_ID ";
            }
            command.Parameters.AddWithValue("@Batch", Batch);
            command.Parameters.AddWithValue("@Semester", Semester);
            command.Parameters.AddWithValue("@Status", (int)FormStatus.Accepted);

            string query = $@"SELECT DISTINCT
                              StudentFormNo,
							  '' AS CollegeFullName,
                              FullName, 
                              FathersName, 
                              MothersName,
                              PINFO.Student_ID, 
                              BoardRegistrationNo,
                              CUSRegistrationNo,
                              Gender, 
                              DOB,
                              Photograph, 
                              IsProvisional, 
                              FormStatus,
                              A.Mobile AS PhoneNo,
                             ADMCombinationMaster.Course_ID,
							   PINFO.AcceptCollege_ID AS College_ID,
                              ARGSelectedCombination{tablePostFix}.Combination_ID,
                              ExamRollNumber,
                              ARGStudentExamForm{tablePostFix}.Year AS Year,
                              ARGStudentExamForm{tablePostFix}.FormNumber,
                              PINFO.Batch,
                              PINFO.CreatedOn,
                                ADMCombinationMaster.CombinationCode,
							  ARGSelectedCombination{tablePostFix}.semester,
							  ADMCourseMaster.CourseFullName	
                              FROM ARGPERSONALINFORMATION{tablePostFix} PINFO 
                             INNER JOIN ARGSelectedCombination{tablePostFix} ON ARGSelectedCombination{tablePostFix}.Student_ID =PINFO.Student_ID 
                              INNER JOIN ARGStudentExamForm{tablePostFix} ON ARGStudentExamForm{tablePostFix}.Student_ID = PINFO.Student_ID AND ARGStudentExamForm{tablePostFix}.Semester = ARGSelectedCombination{tablePostFix}.Semester
                              INNER JOIN ARGStudentAddress{tablePostFix} A ON PINFO.Student_ID=A.Student_ID
                              INNER JOIN ADMCombinationMaster ON ADMCombinationMaster.Combination_ID = ARGSelectedCombination{tablePostFix}.Combination_ID
							  INNER JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = ADMCombinationMaster.Course_ID
							  INNER JOIN ARGCentersAllotmentMaster ON ARGCentersAllotmentMaster.[Entity_ID] = StudentExamForm_ID
							  INNER JOIN ARGEntranceCentersMaster ON ARGEntranceCentersMaster.Center_ID = ARGCentersAllotmentMaster.Center_ID
                              WHERE PINFO.AcceptCollege_ID=@College_ID {IncludeCourseSQL} 
							  AND ARGSelectedCombination{tablePostFix}.SemesterBatch<=@Batch AND ARGStudentExamForm{tablePostFix}.Semester=@Semester AND ARGStudentExamForm{tablePostFix}.Status=@Status 
                              {ExamRollNoSQL} 
                              ORDER BY ExamRollNumber ASC";

            command.CommandText = query;
            return command;
        }



        public AttendanceSheetForm GetStudentAdmitCardAndAttendanceSheet(PrintProgramme programme, string ExamFormNo)
        {
            return new MSSQLFactory().GetObject<AttendanceSheetForm>(GetStudentAdmitCardAndAttendanceSheetQuery(programme, ExamFormNo));
        }

        public SqlCommand GetStudentAdmitCardAndAttendanceSheetQuery(PrintProgramme programme, string ExamFormNo)
        {
            string ExamRollNoSQL = string.Empty;
            string tablePostFix = "_" + programme.ToString();
            SqlCommand command = new SqlCommand();
            command.Parameters.AddWithValue("@ExamFormNo", ExamFormNo);
            command.Parameters.AddWithValue("@Status", (int)FormStatus.Accepted);

            string query = $@"SELECT Top 1
                              StudentFormNo,
							  '' AS CollegeFullName,
                              FullName, 
                              FathersName, 
                              MothersName,
                              PINFO.Student_ID, 
                              BoardRegistrationNo,
                              CUSRegistrationNo,
                              Gender, 
                              DOB,
                              Photograph, 
                              IsProvisional, 
                              FormStatus,
                              A.Mobile AS PhoneNo,
                             ADMCombinationMaster.Course_ID,
							   PINFO.AcceptCollege_ID AS College_ID,
                              ARGSelectedCombination{tablePostFix}.Combination_ID,
                              ExamRollNumber,
                              ARGStudentExamForm{tablePostFix}.Year AS Year,
                              ARGStudentExamForm{tablePostFix}.FormNumber,
                              PINFO.Batch,
                              PINFO.CreatedOn,
                                ADMCombinationMaster.CombinationCode,
							  ARGSelectedCombination{tablePostFix}.semester,
							  ADMCourseMaster.CourseFullName	
                              FROM ARGPERSONALINFORMATION{tablePostFix} PINFO 
                             INNER JOIN ARGSelectedCombination{tablePostFix} ON ARGSelectedCombination{tablePostFix}.Student_ID =PINFO.Student_ID 
                              INNER JOIN ARGStudentExamForm{tablePostFix} ON ARGStudentExamForm{tablePostFix}.Student_ID = PINFO.Student_ID AND ARGStudentExamForm{tablePostFix}.Semester = ARGSelectedCombination{tablePostFix}.Semester
                              INNER JOIN ARGStudentAddress{tablePostFix} A ON PINFO.Student_ID=A.Student_ID
                              INNER JOIN ADMCombinationMaster ON ADMCombinationMaster.Combination_ID = ARGSelectedCombination{tablePostFix}.Combination_ID
							  INNER JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = ADMCombinationMaster.Course_ID
							  INNER JOIN ARGCentersAllotmentMaster ON ARGCentersAllotmentMaster.[Entity_ID] = StudentExamForm_ID
							  INNER JOIN ARGEntranceCentersMaster ON ARGEntranceCentersMaster.Center_ID = ARGCentersAllotmentMaster.Center_ID
                              WHERE ARGStudentExamForm{tablePostFix}.FormNumber=@ExamFormNo AND ARGStudentExamForm{tablePostFix}.Status={(short)FormStatus.Accepted}
                              ORDER BY ARGStudentExamForm{tablePostFix}.CreatedOn DESC;";

            command.CommandText = query;
            return command;
        }


        #endregion

        #region Views
        //CREATE VIEW[dbo].[VWSCMaster]
        //AS
        //    SELECT Programme,CombinationMaster.*,SubjectFullName,SubjectCode,SubjectType,TotalCredit,Department_ID
        //    ,IsExternalMarksApplicable,ExternalMaxMarks,IsExternalAttendance_AssessmentApplicable,ExternalAttendance_AssessmentMaxMarks,ExternalMinPassMarks
        //    ,IsInternalMarksApplicable,InternalMaxMarks,IsInternalAttendance_AssessmentApplicable,InternalAttendance_AssessmentMaxMarks,InternalMinPassMarks
        //    ,IsExternalPassComponent,ExternalAttendance_AssessmentMinPassMarks,IsExternalAttendance_AssessmentPassComponent
        //    ,IsInternalPassComponent,InternalAttendance_AssessmentMinPassMarks,IsInternalAttendance_AssessmentPassComponent
        //    ,[ExternalIsPartOf],[ExternalAttendanceIsPartOf],[InternalIsPartOf],[InternalAttendanceIsPartOf],HasResult,HasExaminationFee
        //    FROM(
        //    SELECT Combination_ID, College_ID, Course_ID, Status, CombinationCode, Semester,
        //    Split.a.value('.', 'UNIQUEIDENTIFIER') AS Subject_ID
        //    FROM(SELECT ADMCombinationMaster.Combination_ID, College_ID, Course_ID , Status, CombinationCode, Semester,
        //                    CAST ('<M>' + REPLACE(ADMCombinationMaster.CombinationSubjects, '|', '</M><M>') + '</M>' AS XML) AS String
        //            FROM ADMCombinationMaster) AS A CROSS APPLY String.nodes('/M') AS Split(a)
        //    )CombinationMaster
        //    JOIN ADMSubjectMaster ON ADMSubjectMaster.Subject_ID = CombinationMaster.Subject_ID AND ADMSubjectMaster.Semester = CombinationMaster.Semester;



        //CREATE VIEW[dbo].[VWStudentCourse]
        //        AS
        //SELECT STDINFO.Student_ID,STDINFO.FullName,STDINFO.CUSRegistrationNo,STDINFO.DOB,STDINFO.CurrentSemesterOrYear,ClassRollNo,STDINFO.Batch,STDCourseInfo.CourseFullName,
        //STDCourseInfo.PrintProgramme,ADMCollegeMaster.CollegeFullName,STDCourseInfo.Course_ID,STDINFO.AcceptCollege_ID,STDCourseInfo.CourseCode
        //FROM(
        //SELECT Student_ID, FullName, CUSRegistrationNo, DOB, AcceptCollege_ID, CurrentSemesterOrYear, Batch, ClassRollNo FROM dbo.ARGPersonalInformation_UG
        //UNION ALL
        //SELECT Student_ID, FullName, CUSRegistrationNo, DOB, AcceptCollege_ID, CurrentSemesterOrYear, Batch, ClassRollNo FROM dbo.ARGPersonalInformation_PG
        //UNION ALL
        //SELECT Student_ID, FullName, CUSRegistrationNo, DOB, AcceptCollege_ID, CurrentSemesterOrYear, Batch, ClassRollNo FROM dbo.ARGPersonalInformation_IH
        //)STDINFO
        //LEFT JOIN(
        //SELECT DISTINCT STDCOMB.Student_ID, ADMCourseMaster.Course_ID, ADMCourseMaster.CourseFullName, ADMCourseMaster.PrintProgramme, ADMCourseMaster.CourseCode FROM (
        //SELECT DISTINCT Student_ID, Combination_ID FROM ARGSelectedCombination_UG
        //UNION ALL
        //SELECT DISTINCT Student_ID, Combination_ID FROM ARGSelectedCombination_IH
        //UNION ALL
        //SELECT DISTINCT Student_ID, Combination_ID FROM ARGSelectedCombination_PG
        //)STDCOMB
        //LEFT JOIN ADMCombinationMaster ON ADMCombinationMaster.Combination_ID = STDCOMB.Combination_ID
        //LEFT JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = ADMCombinationMaster.Course_ID
        //)STDCourseInfo ON STDCourseInfo.Student_ID = STDINFO.Student_ID
        //JOIN ADMCollegeMaster ON STDINFO.AcceptCollege_ID=ADMCollegeMaster.College_ID
        #endregion

        #region RelocateCenter
        public List<DropDownOptLabelGeneral> GetRelocateCenterList()
        {
            return new MSSQLFactory().GetObjectList<DropDownOptLabelGeneral>
                ($@"SELECT DISTINCT ecm.Center_ID AS [Value],ecm.CenterCode AS [Text],cm.CollegeFullName AS DataGroupField FROM dbo.ARGCentersAllotmentMaster cam
                    JOIN dbo.ARGEntranceCentersMaster ecm ON ecm.Center_ID = cam.Center_ID
                    JOIN dbo.ADMCollegeMaster cm ON cm.College_ID = ecm.College_ID
                    WHERE ecm.IsEntrance=0 AND ecm.Status=1 ORDER BY cm.CollegeFullName ASC,ecm.CenterCode ASC");
        }
        public List<SelectListItem> GetRelocateCenterCourseIds(Guid Center_ID)
        {
            return new MSSQLFactory().GetObjectList<SelectListItem>
                ($@"SELECT DISTINCT CAST(cam.Course_ID AS VARCHAR(50))
                    AS [Value],cm.CourseFullName AS [Text],cm.Programme FROM dbo.ARGCentersAllotmentMaster cam
                    JOIN dbo.ADMCourseMaster cm ON cm.Course_ID = cam.Course_ID
                    WHERE cam.Center_ID='{Center_ID}'
                    ORDER BY cm.Programme,cm.CourseFullName");
        }

        public List<SelectListItem> GetArchiveCenterCourseIds()
        {
            return new MSSQLFactory().GetObjectList<SelectListItem>
                ($@"SELECT DISTINCT CAST(cam.Course_ID AS VARCHAR(50))
                    AS [Value],cm.CourseFullName AS [Text],cm.Programme FROM dbo.ARGCentersAllotmentMaster cam
					JOIN dbo.ARGEntranceCentersMaster ON ARGEntranceCentersMaster.Center_ID = cam.Center_ID AND IsEntrance=0
                    JOIN dbo.ADMCourseMaster cm ON cm.Course_ID = cam.Course_ID
                    ORDER BY cm.Programme,cm.CourseFullName");
        }


        public int RelocateCenter(RelocateCenter relocateCenter)
        {
            return new MSSQLFactory().ExecuteNonQuery($"UPDATE dbo.ARGCentersAllotmentMaster SET Center_ID='{relocateCenter.ToCenter_ID}' WHERE Center_ID='{relocateCenter.FromCenter_ID}' AND Course_ID='{relocateCenter.FromCourse_ID}';");
        }

        public int ArchiveCenters(ArchiveCenter archiveCenters)
        {
            PrintProgramme printProgramme = new GeneralFunctions().MappingTable(new CourseDB().GetItem(archiveCenters.Course_ID).PrintProgramme);
            return new MSSQLFactory().ExecuteNonQuery($@"DELETE cm FROM dbo.ARGCentersAllotmentMaster cm
                                                        JOIN dbo.ARGStudentExamForm_{printProgramme.ToString()} EF ON EF.StudentExamForm_ID=cm.Entity_ID AND EF.Semester={archiveCenters.Semester}
                                                        WHERE cm.Course_ID='{archiveCenters.Course_ID}'");
        }
        public int ArchiveCentersWithNoAllotment()
        {
            return new MSSQLFactory().ExecuteNonQuery($@"DELETE ca FROM dbo.ARGEntranceCentersMaster ca
                                                        LEFT JOIN dbo.ARGCentersAllotmentMaster em ON ca.Center_ID = em.Center_ID
                                                        WHERE em.CenterAllowment_ID IS NULL AND ca.IsEntrance=0");
        }

        public List<Guid> GetNotDuplicateStudentIDs(PrintProgramme printProgramme, short semester)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            List<SelectListItem> items = new MSSQLFactory().GetObjectList<SelectListItem>($@"
                                                    SELECT CAST(pif.Student_ID  AS VARCHAR(100)) Text,
                                                           CAST(COUNT(pif.Student_ID) AS VARCHAR(10)) Value
                                                        FROM
                                                        (
                                                            SELECT DISTINCT
                                                                   Student_ID,
                                                                   ExamRollNumber
                                                            FROM ARGStudentExamForm_{printProgramme}
                                                            WHERE ExamRollNumber IS NOT NULL
                                                        ) pif
                                                        GROUP BY Student_ID
                                                        HAVING COUNT(pif.Student_ID) > 1;");
            if (items.IsNotNullOrEmpty())
                return items.Select(x => Guid.Parse(x.Text)).ToList();

            return null;
        }

        public int UpdateToCorrectRollNos(List<Guid> studentIDs, PrintProgramme printProgramme)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            List<string> queries = new MSSQLFactory().GetSingleValues<string>
                ($@"SELECT 'UPDATE ARGStudentExamForm_{printProgramme} SET ExamRollNumber=''' + S.ExamRollNumber + ''' WHERE Student_ID='''
                           + CAST(S.Student_ID AS VARCHAR(800)) + ''' AND ExamRollNumber IS NOT NULL AND ExamRollNumber<>'''
                           + S.ExamRollNumber + ''';'
                    FROM
                    (
                        SELECT ROW_NUMBER() OVER (PARTITION BY Student_ID
                                                  ORDER BY NoofTimes DESC,c.Year ASC,c.IsRegular asc
                                                 ) SNO,
                               Student_ID,
                               ExamRollNumber
                        FROM ( SELECT COUNT(ExamRollNumber) NoofTimes, ExamRollNumber,
                               MAX(Student_ID) Student_ID, MIN(year) [Year], MIN(CAST(IsRegular AS SMALLINT)) IsRegular 
                        FROM ARGStudentExamForm_{printProgramme}
                        WHERE ExamRollNumber IS NOT NULL
                              AND Student_ID IN ( {studentIDs.ToIN()} )
		                      GROUP BY ExamRollNumber) c 
                    ) S
                    WHERE S.SNO = 1;");

            if (queries.IsNotNullOrEmpty())
            {
                return new MSSQLFactory().ExecuteNonQuery(string.Join(Environment.NewLine, queries));
            }
            return 0;
        }
        #endregion
    }
}
