using CUSrinagar.DataManagers.SQLQueries;
using CUSrinagar.Enums;
using CUSrinagar.Models;
using GeneralModels;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terex;
using System.Web.Mvc;
using CUSrinagar.Extensions;

namespace CUSrinagar.DataManagers
{
    public class MarksDB
    {
        public bool GenerateMarks(PrintProgramme Prog, int Batch, Guid Course_ID, int Semestr, string studentclause, int tblsem = 0)
        {
            Programme programme = tblsem == 0 ? new GeneralFunctions().GetProgramme(Prog) : Programme.IG;
            string ResultTbl = new GeneralFunctions().GetTableName(Semestr.ToString(), programme);
            string mainSubjectQuery = $@" SELECT DISTINCT fullname
                                                   ,Marks_ID 
                                                   ,SDetail.[SubjectFULLNAME] SubjectTitle
		                                           ,SDetail.[Subject_ID]
                                                   ,SDetail.[SubjectType]
		                                           ,Subjct.[SubjectCode]
                                                   ,SDetail.[Semester]
                                                   ,'{Course_ID}' Course_Id
                                                   ,Subjct.TotalCredit Credit
                                                   ,SDetail.[Student_Id]
                                                   ,Subjct.CreditWeightage
                                                   ,Subjct.SGPAType
		                                           ,dbo.GetInternalMinMarks(Subjct.SubjectMarksStructure_ID) InternalMinMarks
                                                   , dbo.GetInternalMaxMarks(Subjct.SubjectMarksStructure_ID) InternalMaxMarks
                                                   ,(
                                                            SELECT (CASE WHEN S.internalispartof = 1 AND IsInternalMarksApplicable=1 AND CAST(Res.InternalMarks AS DECIMAL)>0 AND InternalMarks IS NOT NULL
                                                              THEN CAST(Res.InternalMarks AS DECIMAL)

                                                             ELSE 0 
					                                        END +
					                                        CASE WHEN S.externalispartof=1  AND IsExternalMarksApplicable=1 AND CAST(Res.ExternalMarks AS DECIMAL)>0 AND ExternalMarks IS NOT NULL
                                                             THEN CAST(ExternalMarks AS DECIMAL)

                                                             ELSE 0 
					                                        END+
					                                        CASE WHEN S.InternalAttendance_Assessmentispartof=1 AND IsInternalAttendance_AssessmentMarksApplicable=1  AND CAST(InternalAttendance_AssessmentMarks AS DECIMAL)>0 AND InternalAttendance_AssessmentMarks IS NOT NULL
                                                             THEN CAST(InternalAttendance_AssessmentMarks AS DECIMAL)

                                                             ELSE 0 
					                                        END+
					                                        CASE WHEN S.ExternalAttendance_Assessmentispartof=1 AND IsExternalAttendance_AssessmentMarksApplicable=1  AND CAST(ExternalAttendance_AssessmentMarks AS DECIMAL)>0 AND ExternalAttendance_AssessmentMarks IS NOT NULL
                                                             THEN CAST(ExternalAttendance_AssessmentMarks AS DECIMAL)

                                                             ELSE 0 
					                                        END) FROM VW_SubjectWithStructure S
                                                            join {ResultTbl} RES
                                                             on S.sUBJECT_iD=RES.Subject_ID WHERE Res.Subject_id= SDetail.Subject_ID AND Res.Student_ID = SDetail.Student_ID
                                                   ) InternalMarksObt
                                                  ,dbo.GetExternalMinMarks(Subjct.SubjectMarksStructure_ID) ExternalMinMarks
                                                   , dbo.GetExternalMaxMarks(Subjct.SubjectMarksStructure_ID) ExternalMaxMarks
                                                   ,(
                                                                   SELECT (CASE WHEN S.internalispartof = 2 AND IsInternalMarksApplicable=1 AND CAST(Res.InternalMarks AS DECIMAL)>0 AND InternalMarks IS NOT NULL
                                                              THEN CAST(Res.InternalMarks AS DECIMAL)
                                                             ELSE 0 
					                                        END +
					                                        CASE WHEN S.externalispartof=2 AND IsExternalMarksApplicable=1  AND CAST(Res.ExternalMarks AS DECIMAL)>0AND ExternalMarks IS NOT NULL
                                                             THEN CAST(ExternalMarks AS DECIMAL)
                                                             ELSE 0 
					                                        END+
					                                        CASE WHEN S.InternalAttendance_Assessmentispartof=2  AND IsInternalAttendance_AssessmentMarksApplicable=1 AND CAST(InternalAttendance_AssessmentMarks AS DECIMAL)>0 AND InternalAttendance_AssessmentMarks IS NOT NULL
                                                             THEN CAST(InternalAttendance_AssessmentMarks AS DECIMAL)
                                                             ELSE 0 
					                                        END+
					                                        CASE WHEN S.ExternalAttendance_Assessmentispartof=2 AND IsExternalAttendance_AssessmentMarksApplicable=1  AND CAST(ExternalAttendance_AssessmentMarks AS DECIMAL)>0 AND ExternalAttendance_AssessmentMarks IS NOT NULL
                                                             THEN CAST(ExternalAttendance_AssessmentMarks AS DECIMAL)
                                                             ELSE 0 
					                                        END) FROM VW_SubjectWithStructure S
                                                            join {ResultTbl} RES
                                                             on S.sUBJECT_iD=RES.Subject_ID WHERE Res.Subject_id= SDetail.Subject_ID AND Res.Student_ID = SDetail.Student_ID
                                                   ) ExternalMarksObt

                                        FROM VWStudentDetailforMS SDetail
                                        JOIN VW_SubjectWithStructure Subjct ON Subjct.subject_id =SDetail.subject_id and HasResult=1 
                                       LEFT JOIN MSStudentMarks Marks on Marks.Student_Id=SDetail.Student_ID and Marks.Subject_ID=SDetail.Subject_ID  ";
            string addSubjectQuery = $@" SELECT fullname
                                                   ,Marks_ID 
                                                   ,Subjct.[SubjectFULLNAME] SubjectTitle
		                                           ,Subjct.[Subject_ID]
                                                   ,Subjct.[SubjectType]
		                                           ,Subjct.[SubjectCode]
                                                   ,Subjct.[Semester]
                                                   ,'{Course_ID}' Course_Id
                                                   ,Subjct.[TotalCredit] Credit
                                                   , CDetail.Student_ID
                                                   ,Subjct.CreditWeightage
                                                   ,Subjct.SGPAType
		                                           ,dbo.GetInternalMinMarks(Subjct.SubjectMarksStructure_ID) InternalMinMarks
                                                   , dbo.GetInternalMaxMarks(Subjct.SubjectMarksStructure_ID) InternalMaxMarks
                                                   ,(
                                                            SELECT (CASE WHEN SM.internalispartof = 1 AND IsInternalMarksApplicable=1 AND CAST(Res.InternalMarks AS DECIMAL)>0 AND InternalMarks IS NOT NULL
                                                              THEN CAST(Res.InternalMarks AS DECIMAL)

                                                             ELSE 0 
					                                        END +
					                                        CASE WHEN SM.externalispartof=1 AND IsExternalMarksApplicable=1  AND CAST(Res.ExternalMarks AS DECIMAL)>0 AND ExternalMarks IS NOT NULL
                                                             THEN CAST(ExternalMarks AS DECIMAL)

                                                             ELSE 0 
					                                        END+
					                                        CASE WHEN SM.InternalAttendance_Assessmentispartof=1  AND IsInternalAttendance_AssessmentMarksApplicable=1 AND CAST(InternalAttendance_AssessmentMarks AS DECIMAL)>0 AND InternalAttendance_AssessmentMarks IS NOT NULL
                                                             THEN CAST(InternalAttendance_AssessmentMarks AS DECIMAL)

                                                             ELSE 0 
					                                        END+
					                                        CASE WHEN SM.ExternalAttendance_Assessmentispartof=1 AND IsExternalAttendance_AssessmentMarksApplicable=1 AND CAST(ExternalAttendance_AssessmentMarks AS DECIMAL)>0 AND ExternalAttendance_AssessmentMarks IS NOT NULL
                                                             THEN CAST(ExternalAttendance_AssessmentMarks AS DECIMAL)

                                                             ELSE 0 
					                                        END) FROM VW_SubjectWithStructure SM
                                                            join {ResultTbl} RES
                                                             on sm.sUBJECT_iD= RES.Subject_ID WHERE Res.Subject_id= SDetail.Subject_ID AND Student_ID = SDetail.Student_ID
                                                   ) InternalMarksObt
                                                  ,dbo.GetExternalMinMarks(Subjct.SubjectMarksStructure_ID) ExternalMinMarks
                                                   , dbo.GetExternalMaxMarks(Subjct.SubjectMarksStructure_ID) ExternalMaxMarks
                                                   ,(
                                                                   SELECT (CASE WHEN SM.internalispartof = 2 AND IsInternalMarksApplicable=1 AND CAST(Res.InternalMarks AS DECIMAL)>0 AND InternalMarks IS NOT NULL
                                                              THEN CAST(Res.InternalMarks AS DECIMAL)
                                                             ELSE 0 
					                                        END +
					                                        CASE WHEN SM.externalispartof=2 AND IsExternalMarksApplicable=1 AND CAST(Res.ExternalMarks AS DECIMAL)>0AND ExternalMarks IS NOT NULL
                                                             THEN CAST(ExternalMarks AS DECIMAL)
                                                             ELSE 0 
					                                        END+
					                                        CASE WHEN SM.InternalAttendance_Assessmentispartof=2 AND IsInternalAttendance_AssessmentMarksApplicable=1 AND CAST(InternalAttendance_AssessmentMarks AS DECIMAL)>0 AND InternalAttendance_AssessmentMarks IS NOT NULL
                                                             THEN CAST(InternalAttendance_AssessmentMarks AS DECIMAL)
                                                             ELSE 0 
					                                        END+
					                                        CASE WHEN SM.ExternalAttendance_Assessmentispartof=2 AND IsExternalAttendance_AssessmentMarksApplicable=1  AND CAST(ExternalAttendance_AssessmentMarks AS DECIMAL)>0 AND ExternalAttendance_AssessmentMarks IS NOT NULL
                                                             THEN CAST(ExternalAttendance_AssessmentMarks AS DECIMAL)
                                                             ELSE 0 
					                                        END) FROM VW_SubjectWithStructure SM
                                                            join {ResultTbl} RES
                                                             on sm.sUBJECT_iD= RES.Subject_ID WHERE Res.Subject_id= SDetail.Subject_ID AND Student_ID = SDetail.Student_ID
                                                   ) ExternalMarksObt
                          FROM dbo.ARGStudentAdditionalSubjects_{programme.GetTablePFix()} SDetail

                           JOIN dbo.VWStudentResultAllSemesters CDetail
                           ON CDetail.Student_ID = SDetail.Student_ID AND CDetail.Subject_ID = SDetail.Subject_ID

                        JOIN dbo.ARGPersonalInformation_{programme.GetTablePFix()} Detail
                        ON Detail.Student_ID = SDetail.Student_ID
                     
					    JOIN VW_SubjectWithStructure Subjct ON Subjct.Subject_ID = SDetail.Subject_ID  
                       LEFT JOIN MSStudentMarks Marks on Marks.Student_Id=SDetail.Student_ID and Marks.Subject_ID=SDetail.Subject_ID ";
            string query = $@"	
                                        IF OBJECT_ID('tempdb..#TempStudentMarks') IS NOT null
                                        DROP TABLE #TempStudentMarks
                                         SELECT * 
                                           INTO #TempStudentMarks   
                                          FROM ({mainSubjectQuery} UNION {addSubjectQuery}) AllDetail
                                        WHERE AllDetail.Semester={Semestr} AND student_Id {studentclause} AND Marks_ID IS NULL 
                                        ORDER BY AllDetail.student_Id, AllDetail.semester , AllDetail.SubjectTitle


                                        INSERT INTO[dbo].[MSStudentMarks]
                                                   (Marks_ID
                                                   ,Subject_ID
                                                   ,SubjectCode
                                                   ,SubjectTitle
                                                   ,SubjectType
                                                   ,Semester
                                                   ,Credit
                                                   ,CreditWeightage
                                                   ,SGPAType
                                                   ,Student_Id
                                                   ,InternalMinMarks
                                                   ,InternalMaxMarks
                                                   ,InternalMarksObt
                                                   ,InternalResultStatus
                                                   ,ExternalMinMarks
                                                   ,ExternalMaxMarks
                                                   ,ExternalMarksObt
                                                   ,ExternalResultStatus
                                                   ,GradeLetter
                                                   ,GradePoints
                                                   ,Tcourse_ID
                                                 )
                                        SELECT   NEWID() AS Marks_ID
                                                   , Subject_ID
                                                   , SubjectCode
                                                   , SubjectTitle
                                                   , SubjectType
                                                   , Semester
                                                   , Credit
                                                   , CreditWeightage
                                                   , ISNull(SGPAType,1)
                                                   , Student_Id
                                                   , InternalMinMarks
                                                   , InternalMaxMarks
                                                   , ISNULL(InternalMarksObt,0) InternalMarksObt
                                                  ,(CASE WHEN ISNULL(InternalMarksObt,0)>=InternalMinMarks
                                                     THEN 1

                                                             ELSE 0 
					 	                                        END	   ) InternalResultStatus
                                                   ,ExternalMinMarks
                                                   ,ExternalMaxMarks
                                                   , ISNULL(ExternalMarksObt,0) ExternalMarksObt
		                                           ,(CASE WHEN ISNULL(ExternalMarksObt,0)>=ExternalMinMarks
                                                     THEN 1

                                                             ELSE 0 
					                                         END		   ) ExternalResultStatus
                                                 ,
                                                (SELECT TOP 1 LetterGrade FROM MSGrades
						                        join MSCourseGradeSystem on MSCourseGradeSystem.GradeSystem = MSGrades.GradeSystem and MSCourseGradeSystem.Course_ID='{Course_ID}'
						                        where 
						                        (((ISNULL(ExternalMarksObt,0)+ISNULL(InternalMarksObt,0))/(ExternalMaxMarks+InternalMaxMarks)*Cast(100 AS Decimal)))>=FourtyPassPercentageFrom 
						                        and (((ISNULL(ExternalMarksObt,0)+ISNULL(InternalMarksObt,0))/(ExternalMaxMarks+InternalMaxMarks)*Cast(100 AS Decimal)))<FourtyPassPercentageTo)
						                        GradeLetter
                                                 
                                                ,(SELECT TOP 1 GradePoint FROM MSGrades 
						                        join MSCourseGradeSystem on MSCourseGradeSystem.GradeSystem = MSGrades.GradeSystem and MSCourseGradeSystem.Course_ID='{Course_ID}'
						                        where 
						                        (((ISNULL(ExternalMarksObt,0)+ISNULL(InternalMarksObt,0))/(ExternalMaxMarks+InternalMaxMarks)*Cast(100 AS Decimal)))>=FourtyPassPercentageFrom 
						                        and (((ISNULL(ExternalMarksObt,0)+ISNULL(InternalMarksObt,0))/(ExternalMaxMarks+InternalMaxMarks)*Cast(100 AS Decimal)))<FourtyPassPercentageTo) 
						                        GradePoints
                                                ,'{Course_ID}'
                                                  -- ,[ResultNotificationNo]
                                                FROM #TempStudentMarks
		
                                        ";

            ///additional subjects need tobe added
            ///,(SELECT TOP 1 LetterGrade FROM MSGrades WHERE ROUND(((ISNULL(ExternalMarksObt,0)+ISNULL(InternalMarksObt,0))/(ExternalMaxMarks+InternalMaxMarks)*Cast(100 AS Decimal)),0)>=FourtyPassPercentageFrom AND ROUND(((ISNULL(ExternalMarksObt,0)+ISNULL(InternalMarksObt,0))/(ExternalMaxMarks+InternalMaxMarks)*Cast(100 AS Decimal)),0)<=FourtyPassPercentageTo) GradeLetter
            //,(SELECT TOP 1 GradePoint FROM MSGrades WHERE ROUND(((ISNULL(ExternalMarksObt, 0) + ISNULL(InternalMarksObt, 0)) / (ExternalMaxMarks + InternalMaxMarks) * Cast(100 AS Decimal)), 0) >= FourtyPassPercentageFrom AND ROUND(((ISNULL(ExternalMarksObt,0)+ISNULL(InternalMarksObt, 0))/ (ExternalMaxMarks + InternalMaxMarks) * Cast(100 AS Decimal)),0)<= FourtyPassPercentageTo) GradePoints

            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = query;
            return !new MSSQLFactory().ExecuteScalar<bool>(cmd);
        }

        public List<MSTranscriptDegreeSettings> GetTranscriptSetting(Parameters parameter)
        {
            string query = "select * from MSTranscriptDegreeSettings ";
            FilterHelper helper = new GeneralFunctions().GetWhereClause<MSTranscriptDegreeSettings>(parameter.Filters);
            query += helper.WhereClause;
            helper.Command.CommandText = query;
            return new MSSQLFactory().GetObjectList<MSTranscriptDegreeSettings>(helper.Command);
        }

        public void UpdateDate(string studentinclause, PrintProgramme printProgramme, int semester, Guid Course_ID)
        {
            string cstring = Course_ID == Guid.Empty ? "" : $@" Where TCourse_Id='{Course_ID}'";
            string Query = $@" UPDATE MSCGPA SET NotificationNo = REPLACE(ResultNotification.NotificationNo,' ',''), DateofDeclaration = ResultNotification.Dated
                                from
                                (SELECt student_ID, Max(Dated) Dated FROM dbo.VWResult
                                JOIN dbo.ResultNotification ON ResultNotification.ResultNotification_ID = VWResult.ResultNotification_ID
                                WHERE Student_ID {studentinclause} 
                                 AND ResultNotification.Semester<={semester} GROUP BY student_ID) t 
                                JOIN ResultNotification ON ResultNotification.Dated = t.Dated AND ResultNotification.PrintProgramme={(int)printProgramme}
                                JOIN MSCGPA ON MSCGPA.Student_ID = t.Student_ID    {cstring} ";
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = Query;
            new MSSQLFactory().ExecuteScalar<bool>(cmd);
        }
        public void UpdateDateForDivImp(string studentinclause, PrintProgramme printProgramme, MSCGPA mSCGPA)
        {
            string Query = $@" UPDATE MSCGPA SET DivImpNotificationNo = NotificationNo, DivImpDateofDeclaration = DateofDeclaration
                                from MSCGPA WHERE Student_ID {studentinclause} 
                                 UPDATE MSCGPA SET NotificationNo = '{mSCGPA.NotificationNo}', DateofDeclaration ='{mSCGPA.DateofDeclaration}'
                                from MSCGPA WHERE Student_ID  {studentinclause}";
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = Query;
            new MSSQLFactory().ExecuteScalar<bool>(cmd);
        }


        public List<Guid> GenerateFinalStudentCreditInClause(string studentsGuidinclause, int TotalCredit, PrintProgramme printProgramme, int semester, bool AllowExtraCredit, Guid Student_ID)
        {
            string substring = AllowExtraCredit ? ">=" : "=";
            string studentIDdtring = (Student_ID == Guid.Empty) ? "" : $" AND Details.Student_ID='{Student_ID}'";
            string postfix = new GeneralFunctions().GetProgrammePostFix(printProgramme);
            string Query = $@" SELECT Details.Student_ID  FROM   
                                   (SELECT  CDetail.Student_ID,CDetail.Subject_ID,batch,SDetail.Course_ID,CDetail.TotalInternalMarks,CDetail.TotalExternalMarks  FROM dbo.VWStudentDetailforMS SDetail
                                    JOIN dbo.VWStudentResultAllSemesters CDetail
                                    ON CDetail.Student_ID = SDetail.Student_ID AND CDetail.Subject_ID = SDetail.Subject_ID
                       	            UNION 
					                SELECT Detail.Student_ID ,CDetail.Subject_ID,batch,ADMCombinationMaster.Course_ID,CDetail.TotalInternalMarks,CDetail.TotalExternalMarks FROM dbo.ARGStudentAdditionalSubjects{postfix} SDetail
						            JOIN dbo.VWStudentResultAllSemesters CDetail
						            ON CDetail.Student_ID = SDetail.Student_ID AND CDetail.Subject_ID = SDetail.Subject_ID
						            JOIN dbo.ARGPersonalInformation{postfix}  Detail
			                        ON Detail.Student_ID = SDetail.Student_ID 
						            JOIN dbo.ARGSelectedCombination{postfix}  ON ARGSelectedCombination{postfix} .Student_ID = CDetail.Student_ID
						            JOIN dbo.ADMCombinationMaster ON ADMCombinationMaster.Combination_ID = ARGSelectedCombination{postfix} .Combination_ID)
                        Details
						JOIN        dbo.ADMSubjectMaster Subjct
                                  ON Subjct.Subject_ID = Details.Subject_ID
						  JOIN dbo.MSSubjectMarksStructure MS
                        ON MS.SubjectMarksStructure_ID = Subjct.SubjectMarksStructure_ID
					    WHERE Details.Student_Id {studentsGuidinclause}    and  Subjct.Semester<={semester} {studentIDdtring}    GROUP BY Details.Student_ID
                            HAVING SUM(MS.TotalCredit) {substring} {TotalCredit}";

            return new MSSQLFactory().GetSingleValues<Guid>(Query) ?? new List<Guid>();
        }

        public List<Guid> GetStudentIDs(Parameters parameter)
        {
            return new MSSQLFactory().GetSingleValues<Guid>(GetStudentMarksIDQ(parameter));
        }

        private SqlCommand GetStudentMarksIDQ(Parameters parameter)
        {
            string query = "select Distinct Detail.Student_ID  from  (SELECT DISTINCT Student_ID,FULLNAME,COURSE_ID,BATCH,pRINTPROGRAMME,CUSRegistrationNo FROM VWStudentDetailforMS) Detail JOIN MSStudentMarks Marks   on  Detail.Student_ID=Marks.Student_ID ";
            SqlCommand command = new SqlCommand();
            FilterHelper helper = new GeneralFunctions().GetWhereClause<MSStudentMarks>(parameter.Filters);
            query += helper.WhereClause;
            //      query += new GeneralFunctions().GetPagedQuery(query, parameter);
            helper.Command.CommandText = query;
            return helper.Command;
        }

        public List<Guid> GenerateStudentCreditInClause(Guid Course_ID, int SemesterBatch, int TotalCredit, PrintProgramme printProgramme, int semester, bool AllowExtraCredit
            , bool isLateralEntry, Guid Student_ID)
        {
            string substring = AllowExtraCredit ? ">=" : "=";
            string studentIDdtring = (Student_ID == Guid.Empty) ? "" : $" AND Details.Student_ID='{Student_ID}'";
            string lateralEntry1Sql = isLateralEntry ? " WHERE CDetail.Semester>=SDetail.LateralEntryStartingSemester " : "";
            string lateralEntry2Sql = isLateralEntry ? @" JOIN dbo.ADMCourseMaster ON ADMCourseMaster.Course_ID = ADMCombinationMaster.Course_ID
			                                            WHERE CDetail.Semester>ADMCourseMaster.LateralEntryStartingSemester  " : "";

            string postfix = new GeneralFunctions().GetProgrammePostFix(printProgramme);
            string Query = $@" SELECT Details.Student_ID  FROM   
                                   (SELECT  CDetail.Student_ID,CDetail.Subject_ID,SDetail.Batch,SDetail.Course_ID,CDetail.TotalInternalMarks,CDetail.TotalExternalMarks,Resultnotification_Id  FROM dbo.VWStudentDetailforMS SDetail
                                    JOIN dbo.VWStudentResultAllSemesters CDetail
                                    ON CDetail.Student_ID = SDetail.Student_ID AND CDetail.Subject_ID = SDetail.Subject_ID
                                     {lateralEntry1Sql}
                       	            UNION 
					                SELECT Detail.Student_ID ,CDetail.Subject_ID,SC.SemesterBatch Batch,ADMCombinationMaster.Course_ID,CDetail.TotalInternalMarks,CDetail.TotalExternalMarks,Resultnotification_Id FROM dbo.ARGStudentAdditionalSubjects{postfix} SDetail
						            JOIN dbo.VWStudentResultAllSemesters CDetail
						            ON CDetail.Student_ID = SDetail.Student_ID AND CDetail.Subject_ID = SDetail.Subject_ID
						            JOIN dbo.ARGPersonalInformation{postfix}  Detail
			                        ON Detail.Student_ID = SDetail.Student_ID 
						            JOIN dbo.ARGSelectedCombination{postfix} SC  ON SC.Student_ID = CDetail.Student_ID
						            JOIN dbo.ADMCombinationMaster ON ADMCombinationMaster.Combination_ID = SC.Combination_ID
			                        {lateralEntry2Sql}
                                    )
                        Details
						JOIN        dbo.ADMSubjectMaster Subjct
                                  ON Subjct.Subject_ID = Details.Subject_ID
						  JOIN dbo.MSSubjectMarksStructure MS
                        ON MS.SubjectMarksStructure_ID = Subjct.SubjectMarksStructure_ID
                        LEFT JOIN  MSCGPA  ON Details.Student_ID=MSCGPA.Student_ID
					    WHERE Batch <= {SemesterBatch} AND Details.Course_Id= '{Course_ID}' AND MSCGPA.ID IS NULL AND TotalInternalMarks>=dbo.GetInternalMinMarks(MS.SubjectMarksStructure_ID)
                        AND TotalExternalMarks>=dbo.GetExternalMinMarks(MS.SubjectMarksStructure_ID) AND Resultnotification_Id IS NOT null  AND Subjct.Semester<={semester} {studentIDdtring}
                        GROUP BY Details.Student_ID
                            HAVING SUM(MS.TotalCredit) {substring} {TotalCredit}";

            return new MSSQLFactory().GetSingleValues<Guid>(Query) ?? new List<Guid>();
        }

        public List<MSStudentMarks> GetStudentMarks(Parameters parameter)
        {
            return new MSSQLFactory().GetObjectList<MSStudentMarks>(GetStudentMarksQ(parameter));
        }
        internal SqlCommand GetStudentMarksQ(Parameters parameter)
        {
            string query = "select Marks.*, FullName ,Batch ,PrintProgramme   from  (SELECT DISTINCT Student_ID,FULLNAME,COURSE_ID,BATCH,pRINTPROGRAMME,CUSRegistrationNo FROM VWStudentDetailforMS) Detail JOIN MSStudentMarks Marks   on  Detail.Student_ID=Marks.Student_ID ";
            SqlCommand command = new SqlCommand();
            FilterHelper helper = new GeneralFunctions().GetWhereClause<MSStudentMarks>(parameter.Filters);
            query += helper.WhereClause;
            query += new GeneralFunctions().GetPagedQuery(query, parameter);
            helper.Command.CommandText = query.Replace("ORDER BY FULLNAME", "ORDER BY FULLNAME,Semester,SubjectTitle");
            return helper.Command;
        }
        public bool GenerateSGPA(Guid Course_ID, string StudentInClause)
        {
            //Create sgpa on subjecttype basis
            string query = $@"INSERT INTO[dbo].[MSSGPA]
                                (
                                ID,
                                Student_ID,
                                Semester,
                                SGPA,
                                Formula,
                                Credits,
                                CreditPoints,
                                SGPAType,
                                TCourse_ID
                                )

                                 select TSGPA.* from ( SELECT NEWID() ID,
                                       student_id,
	                                   semester,
	                                   ROUND(SUM(credit*gradepoints)/SUM(credit),2) SGPA,
	                                   'Sum(credit*GradePoints)/sum(Credit)' Formula,
	                                   SUM(Credit) Credits,
	                                   SUM(Credit*GradePoints) GradePoints,
                                       ISNull(SGPAType,1) SGPAType,
                                       '{Course_ID}' TCourse_ID
                                FROM  [MSStudentMarks]
                                WHERE Student_ID {StudentInClause} and TCourse_ID= '{Course_ID}'
                                GROUP BY student_id,semester,SGPAType
                                ) TSGPA Left join MSSGPA MSG on  TSGPA.Student_Id=MSG.Student_ID AND TSGPA.Semester=MSG.semester 
								Where MSG.ID is null
								 ORDER BY TSGPA.student_id,TSGPA.Semester
                                ";
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = query;
            return !new MSSQLFactory().ExecuteScalar<bool>(cmd);
        }



        public bool GenerateCGPA(Guid Course_Id, string StudentInClause)
        {
            string query = $@"INSERT INTO[dbo].[MSCGPA]
                    (
                        ID,
                        Student_ID,
                        TCourse_ID,
                        TotalCreditsEarned,
                        CGPA,
                        DateofDeclaration,
                        TotalCreditPoints,
                        [Percentage],
                        NotificationNo,
                        SGPAType
                    )
                    SELECT NEWID(),
                           student_id,
                           '{Course_Id}',
	                       SUM(Credits) TotalCreditsEarned,
	                       ROUND(CAST(SUM(CreditPoints) AS DECIMAL)/CAST(SUM(CAST(credits AS DECIMAL)) AS DECIMAL),2) CGPA,
	                       GETDATE(),
	                       SUM(CreditPoints) TotalCreditPoints,
	                       ROUND(CAST(SUM(CreditPoints) AS DECIMAL)/CAST(SUM(CAST(credits AS DECIMAL)) AS DECIMAL),2)*10,
	                       '1',
                           SGPAType
                    FROM MSSGPA  WHERE Student_ID {StudentInClause} AND TCourse_ID='{Course_Id}'
                    GROUP BY student_id,SGPATYPE
                    ORDER BY student_id";
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = query;
            new MSSQLFactory().ExecuteNonQuery($@"IF  EXISTS (SELECT * FROM sys.objects WHERE name LIKE '%UN_SerialNumber%')
                                                    ALTER TABLE[dbo].[MSCGPA] DROP CONSTRAINT[UN_SerialNumber]");
            return !new MSSQLFactory().ExecuteScalar<bool>(cmd);
        }

        public bool GenerateSerialNoandOthers(string StudentInClause, int LastCount, PrintProgramme printProgramme, Guid Course_Id)
        {
            string cstring = Course_Id == Guid.Empty ? "" : $@" And TCourse_Id='{Course_Id}'";
            string cwstring = Course_Id == Guid.Empty ? "" : $@" Where TCourse_Id='{Course_Id}'";
            string year = DateTime.Now.Year.ToString();
            string Format = year.Substring(2) + GetFormat(printProgramme);
            string query = $@"IF OBJECT_ID('tempdb..#TempStudentSerial') IS NOT null
                               DROP TABLE #TempStudentSerial
                                  Select  '{Format}'+CONVERT(VARCHAR(50),(FORMAT ({LastCount}+ROW_NUMBER() OVER (ORDER BY student_id), '0000') )) SerialNo

                                ,'Test Notif' NotificationNo 
                                ,GETDATE() DateofDeclaration 
                               ,c.Student_ID INTO #TempStudentSerial
                                FROM dbo.MSCGPA c
                                WHERE c.SGPAType=1 AND c.Student_ID {StudentInClause} {cstring}
                             UPDATE C 
                                    SET C.NotificationNo=S.NotificationNo 
                                    ,C.DateofDeclaration=S.DateofDeclaration
                                    ,C.MarksSheetNo=S.SerialNo
                                    from dbo.MSCGPA C JOIN  #TempStudentSerial S
                                    ON  S.Student_ID = C.Student_ID {cwstring}";
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = query;
            var returnValue = !new MSSQLFactory().ExecuteScalar<bool>(cmd);
            new MSSQLFactory().ExecuteNonQuery("ALTER TABLE MSCGPA ADD CONSTRAINT UN_SerialNumber UNIQUE (MarksSheetNo,SGPAType)");
            return returnValue;
        }

        private int GetFormat(PrintProgramme printProgramme)
        {
            switch (printProgramme)
            {
                case PrintProgramme.UG:
                case PrintProgramme.BED:
                    return 1;
                case PrintProgramme.IH:
                    return 3;
                case PrintProgramme.PG:
                    return 2;
                default:
                    return 0;
            }

        }

        public int GetLastCount(PrintProgramme printProgramme)
        {
            //printProgramme = (printProgramme == PrintProgramme.BED) ? PrintProgramme.UG : printProgramme;
            string year = DateTime.Now.Year.ToString();
            string Format = year.Substring(2) + GetFormat(printProgramme);
            string Query = $@"select CONVERT(INT,SUBSTRING(CONVERT(VARCHAR(50),MAX(MarksSheetNo)),4,LEN(Max(MarksSheetNo)))) 
                                FROM dbo.MSCGPA 
                                WHERE MarksSheetNo like '{Format}%'";
            //SqlCommand cmd = new SqlCommand();
            //cmd.CommandText = Query;
            return new MSSQLFactory().ExecuteScalar<int>(Query);
        }

        public Transcript GetBasicInfo(PrintProgramme printProgramme, Guid Student_ID)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            var Query = $@"select top 1 S.FullName,S.FathersName,S.MothersName,S.CUSRegistrationNo,CourseFullName
                        ,CollegeFullName,ADMCollegeMaster.Address CollegeAddress,E.ExamRollNumber,S.Batch
                        from ARGPersonalInformation_{printProgramme} S
                        join ARGSelectedCombination_{printProgramme} C on C.Student_ID = S.Student_ID
                        join ADMCombinationMaster on ADMCombinationMaster.Combination_ID = C.Combination_ID
                        join ADMCourseMaster on ADMCourseMaster.Course_ID = ADMCombinationMaster.Course_ID
                        join ARGStudentExamForm_{printProgramme} E on E.Student_ID = S.Student_ID and E.ExamRollNumber is not null
                        join ADMCollegeMaster on ADMCollegeMaster.College_ID = S.AcceptCollege_ID
                        Where C.Semester = 1 AND S.Student_ID ='{Student_ID}'";
            return new MSSQLFactory().GetObject<Transcript>(Query);
        }
        #region Transcript Dynamic


        public bool GenerateCGPA(Guid Course_Id, int SemesterFrom, int SemesterTo, string StudentInClause)
        {
            string query = $@"INSERT INTO[dbo].[MSCGPA]
                    (
                        ID,
                        Student_ID,
                        TCourse_ID,
                        TotalCreditsEarned,
                        CGPA,
                        DateofDeclaration,
                        TotalCreditPoints,
                        [Percentage],
                        NotificationNo,
                        SGPAType,
                        SemesterTo
                    )
                     Select TCGPA.* from(    SELECT NEWID() ID,
                           student_id,
                           '{Course_Id}' TCourse_ID,
	                       SUM(Credits) TotalCreditsEarned,
	                       ROUND(CAST(SUM(CreditPoints) AS DECIMAL)/CAST(SUM(CAST(credits AS DECIMAL)) AS DECIMAL),2) CGPA,
	                       GETDATE() DateofDeclaration,
	                       SUM(CreditPoints) TotalCreditPoints,
	                       ROUND(CAST(SUM(CreditPoints) AS DECIMAL)/CAST(SUM(CAST(credits AS DECIMAL)) AS DECIMAL),2)*10 [Percentage],
	                       '1' NotificationNo,
                           SGPAType,
                           {SemesterTo} SemesterTo
                    FROM MSSGPA  WHERE Semester>={SemesterFrom} and Semester<={SemesterTo} and Student_ID {StudentInClause} AND TCourse_ID='{Course_Id}' 
                    GROUP BY student_id,SGPATYPE) TCGPA Left join MSCGPA CG on  TCGPA.Student_ID=CG.Student_ID and TCGPA.TCourse_ID=CG.TCourse_ID and TCGPA.SemesterTo=CG.SemesterTo 
                    Where CG.ID is null
                    ORDER BY TCGPA.Student_Id";
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = query;
            new MSSQLFactory().ExecuteNonQuery($@"IF  EXISTS (SELECT * FROM sys.objects WHERE name LIKE '%UN_SerialNumber%')   ALTER TABLE[dbo].[MSCGPA] DROP CONSTRAINT[UN_SerialNumber]");
            return !new MSSQLFactory().ExecuteScalar<bool>(cmd);
        }

        public bool GenerateSerialNo(string StudentInClause, int LastCount, PrintProgramme printProgramme, Guid Course_Id, int SemesterTo)
        {
            string cstring = Course_Id == Guid.Empty ? "" : $@" And TCourse_Id='{Course_Id}'";
            string cwstring = Course_Id == Guid.Empty ? "" : $@" Where TCourse_Id='{Course_Id}'";
            string year = DateTime.Now.Year.ToString();
            string Format = year.Substring(2) + GetFormat(printProgramme);
            string query = $@"IF OBJECT_ID('tempdb..#TempStudentSerial') IS NOT null
                               DROP TABLE #TempStudentSerial
                                  Select  '{Format}'+CONVERT(VARCHAR(50),(FORMAT ({LastCount}+ROW_NUMBER() OVER (ORDER BY student_id), '0000') )) SerialNo

                                ,'Test Notif' NotificationNo 
                                ,GETDATE() DateofDeclaration 
                               ,c.Student_ID INTO #TempStudentSerial
                                FROM dbo.MSCGPA c
                                WHERE c.SGPAType=1 AND c.Student_ID {StudentInClause} {cstring} AND SemesterTo={SemesterTo}
                             UPDATE C 
                                    SET C.NotificationNo=S.NotificationNo 
                                    ,C.DateofDeclaration=S.DateofDeclaration
                                    ,C.MarksSheetNo=S.SerialNo
                                    from dbo.MSCGPA C JOIN  #TempStudentSerial S
                                    ON  S.Student_ID = C.Student_ID {cwstring}  AND SemesterTo={SemesterTo}";
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = query;
            var returnValue = !new MSSQLFactory().ExecuteScalar<bool>(cmd);
            new MSSQLFactory().ExecuteNonQuery("ALTER TABLE MSCGPA ADD CONSTRAINT UN_SerialNumber UNIQUE (MarksSheetNo,SGPAType)");
            return returnValue;
        }



        public void UpdateDateAndNotificationNo(string studentinclause, PrintProgramme printProgramme, int SemesterTo, Guid Course_ID)
        {
            string cstring = Course_ID == Guid.Empty ? "" : $@" Where TCourse_Id='{Course_ID}'";
            string Query = $@" UPDATE MSCGPA SET NotificationNo = REPLACE(t.NotificationNo,' ',''), DateofDeclaration = t.Dated
                                from
                                (select  * from (Select Rn.NotificationNo,Student_ID,Dated,RankIs=ROW_NUMBER() Over (Partition  By student_ID order by Dated Desc)     from ResultNotification RN join VWResult VW on Rn.ResultNotification_ID=vw.ResultNotification_ID 
                                WHERE Student_ID
                                {studentinclause} AND Rn.Semester<={SemesterTo} )TTTT  where RankIs=1) t 
                                JOIN MSCGPA ON MSCGPA.Student_ID = t.Student_ID  {cstring}  AND SemesterTo={SemesterTo}";
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = Query;
            new MSSQLFactory().ExecuteScalar<bool>(cmd);
        }
        public void UpdateDateForDivImp(string studentinclause, PrintProgramme printProgramme, MSCGPA mSCGPA, int semester, Guid Course_ID)
        {
            string Query = $@" UPDATE MSCGPA SET DivImpNotificationNo = NotificationNo, DivImpDateofDeclaration = DateofDeclaration
                                from MSCGPA WHERE Student_ID {studentinclause} AND TCourse_Id='{Course_ID}' AND SemesterTo={semester}
                                 UPDATE MSCGPA SET NotificationNo = '{mSCGPA.NotificationNo}', DateofDeclaration ='{mSCGPA.DateofDeclaration}'
                                from MSCGPA WHERE Student_ID  {studentinclause} AND TCourse_Id='{Course_ID}' AND SemesterTo={semester}";
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = Query;
            new MSSQLFactory().ExecuteScalar<bool>(cmd);
        }
        public List<Guid> GetPassedStudentList(Guid Course_ID, int SemesterBatch, int TotalCredits, PrintProgramme printProgramme, int SemesterFrom, int SemesterTo, TranscriptFilters FilterType, Guid Student_ID)
        {
            string substring = FilterType == TranscriptFilters.IsExtraCredit ? ">=" : "=";
            string studentIDdtring = (Student_ID == Guid.Empty) ? "" : $" AND Details.Student_ID='{Student_ID}'";
            string lateralEntry1Sql = FilterType == TranscriptFilters.IsLateralEntry ? " WHERE CDetail.Semester>=SDetail.LateralEntryStartingSemester " : "";
            string lateralEntry2Sql = FilterType == TranscriptFilters.IsLateralEntry ? @" JOIN dbo.ADMCourseMaster ON ADMCourseMaster.Course_ID = ADMCombinationMaster.Course_ID
			                                            WHERE CDetail.Semester>ADMCourseMaster.LateralEntryStartingSemester  " : "";

            string postfix = new GeneralFunctions().GetProgrammePostFix(printProgramme);
            string Query = $@" SELECT Details.Student_ID  FROM   
                                   (SELECT  CDetail.Student_ID,CDetail.Subject_ID,SDetail.Batch,SDetail.Course_ID,CDetail.TotalInternalMarks,CDetail.TotalExternalMarks,Resultnotification_Id  FROM dbo.VWStudentDetailforMS SDetail
                                    JOIN dbo.VWStudentResultAllSemesters CDetail
                                    ON CDetail.Student_ID = SDetail.Student_ID AND CDetail.Subject_ID = SDetail.Subject_ID
                                     {lateralEntry1Sql}
                       	            UNION 
					                SELECT Detail.Student_ID ,CDetail.Subject_ID,SC.SemesterBatch Batch,ADMCombinationMaster.Course_ID,CDetail.TotalInternalMarks,CDetail.TotalExternalMarks,Resultnotification_Id FROM dbo.ARGStudentAdditionalSubjects{postfix} SDetail
						            JOIN dbo.VWStudentResultAllSemesters CDetail
						            ON CDetail.Student_ID = SDetail.Student_ID AND CDetail.Subject_ID = SDetail.Subject_ID
						            JOIN dbo.ARGPersonalInformation{postfix}  Detail
			                        ON Detail.Student_ID = SDetail.Student_ID 
						            JOIN dbo.ARGSelectedCombination{postfix} SC  ON SC.Student_ID = CDetail.Student_ID
						            JOIN dbo.ADMCombinationMaster ON ADMCombinationMaster.Combination_ID = SC.Combination_ID
			                        {lateralEntry2Sql}
                                    )
                        Details
						JOIN        dbo.ADMSubjectMaster Subjct
                                  ON Subjct.Subject_ID = Details.Subject_ID
						  JOIN dbo.MSSubjectMarksStructure MS
                        ON MS.SubjectMarksStructure_ID = Subjct.SubjectMarksStructure_ID
                        LEFT JOIN  MSCGPA  ON Details.Student_ID=MSCGPA.Student_ID AND TCourse_Id='{Course_ID}'  AND SemesterTo={SemesterTo} 
					    WHERE Batch = {SemesterBatch} AND Details.Course_Id= '{Course_ID}' AND MSCGPA.ID IS NULL AND TotalInternalMarks>=dbo.GetInternalMinMarks(MS.SubjectMarksStructure_ID)
                        AND TotalExternalMarks>=dbo.GetExternalMinMarks(MS.SubjectMarksStructure_ID) AND Resultnotification_Id IS NOT null  AND Subjct.Semester>={SemesterFrom}  AND Subjct.Semester<={SemesterTo} {studentIDdtring}
                        GROUP BY Details.Student_ID
                            HAVING SUM(MS.TotalCredit) {substring} {TotalCredits}";

            return new MSSQLFactory().GetSingleValues<Guid>(Query) ?? new List<Guid>();
        }
        public List<Guid> GetFinalPassedStudentList(string studentsGuidinclause, int TotalCredits, PrintProgramme printProgramme, int SemesterFrom, int
            SemesterTo, TranscriptFilters transcriptFilters)
        {
            string substring = transcriptFilters == TranscriptFilters.IsExtraCredit ? ">=" : "=";
            string postfix = new GeneralFunctions().GetProgrammePostFix(printProgramme);
            string Query = $@" SELECT Details.Student_ID  FROM   
                                   (SELECT  CDetail.Student_ID,CDetail.Subject_ID,batch,SDetail.Course_ID,CDetail.TotalInternalMarks,CDetail.TotalExternalMarks  FROM dbo.VWStudentDetailforMS SDetail
                                    JOIN dbo.VWStudentResultAllSemesters CDetail
                                    ON CDetail.Student_ID = SDetail.Student_ID AND CDetail.Subject_ID = SDetail.Subject_ID
                       	            UNION 
					                SELECT Detail.Student_ID ,CDetail.Subject_ID,batch,ADMCombinationMaster.Course_ID,CDetail.TotalInternalMarks,CDetail.TotalExternalMarks FROM dbo.ARGStudentAdditionalSubjects{postfix} SDetail
						            JOIN dbo.VWStudentResultAllSemesters CDetail
						            ON CDetail.Student_ID = SDetail.Student_ID AND CDetail.Subject_ID = SDetail.Subject_ID
						            JOIN dbo.ARGPersonalInformation{postfix}  Detail
			                        ON Detail.Student_ID = SDetail.Student_ID 
						            JOIN dbo.ARGSelectedCombination{postfix}  ON ARGSelectedCombination{postfix} .Student_ID = CDetail.Student_ID
						            JOIN dbo.ADMCombinationMaster ON ADMCombinationMaster.Combination_ID = ARGSelectedCombination{postfix} .Combination_ID)
                        Details
						JOIN        dbo.ADMSubjectMaster Subjct
                                  ON Subjct.Subject_ID = Details.Subject_ID
						  JOIN dbo.MSSubjectMarksStructure MS
                        ON MS.SubjectMarksStructure_ID = Subjct.SubjectMarksStructure_ID
					    WHERE Details.Student_Id {studentsGuidinclause}    AND Subjct.Semester>={SemesterFrom}  AND Subjct.Semester<={SemesterTo}    GROUP BY Details.Student_ID
                            HAVING SUM(MS.TotalCredit) {substring} {TotalCredits}";

            return new MSSQLFactory().GetSingleValues<Guid>(Query) ?? new List<Guid>();
        }

        #endregion
    }
}