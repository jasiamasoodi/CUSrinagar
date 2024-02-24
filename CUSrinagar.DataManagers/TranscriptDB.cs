using CUSrinagar.DataManagers.SQLQueries;
using CUSrinagar.Enums;
using CUSrinagar.Models;
using GeneralModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Terex;

namespace CUSrinagar.DataManagers
{
    public class TranscriptDB
    {

        public List<Transcript> List(PrintProgramme printProgramme, Parameters parameter, Guid Student_ID)
        {
            int loop = 0;
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);

        Again:
            var Query = $@"SELECT * from (SELECT C.PrintProgramme, S.IsLateralEntry,C.SchoolFullName,S.FullName,S.FathersName,S.CUSRegistrationNo,E.ExamRollNumber,C.CourseFullName,C.DegreeCourseTitle,S.Batch,Col.CollegeFullName,col.Address CollegeAddress,TCourse_ID Course_ID,AcceptCollege_ID
                   ,(SELECT MAX(SemesterBatch) FROM ARGSelectedCombination_{printProgramme} SSC WHERE SSC.Student_ID=S.Student_ID) SemesterBatch,MSCGPA.*
                            FROM MSCGPA 
                            JOIN ARGPersonalInformation_{printProgramme.ToString()} S ON S.Student_ID = MSCGPA.Student_ID AND MSCGPA.SGPAType=1
                            JOIN (
                                    SELECT DISTINCT
                                                EXM.Student_ID,
                                                ExamRollNumber
                                            FROM ARGStudentExamForm_{printProgramme} EXM
                                                JOIN VWStudentDetailforMS
                                                    ON VWStudentDetailforMS.Student_ID = EXM.Student_ID
                                            WHERE EXM.Semester =  (CASE
                                                          WHEN (VWStudentDetailforMS.LateralEntryStartingSemester > 0 AND VWStudentDetailforMS.IsLateralEntry=1)  THEN
                                                               VWStudentDetailforMS.LateralEntryStartingSemester
                                                          ELSE
                                                             1
                                                      END
                                                     )
                                                  AND ExamRollNumber IS NOT NULL

                                    ) E ON E.Student_ID=S.Student_ID
                            JOIN ADMCourseMaster C ON C.Course_ID = TCourse_ID
                            JOIN ADMCollegeMaster Col ON Col.College_ID = S.AcceptCollege_ID ) VW";
            var Helper = new GeneralFunctions().GetWhereClause<FilterModels>(parameter.Filters);
            Helper.Command.CommandText = Query + Helper.WhereClause + new GeneralFunctions().GetPagedQuery(Query, parameter);
            var list = new MSSQLFactory().GetObjectList<Transcript>(Helper.Command);
            if (list == null && printProgramme == PrintProgramme.PG & loop == 0)
            {
                loop++;
                printProgramme = new GeneralFunctions().MappingTable(PrintProgramme.IH);
                goto Again;
            }
            if (list != null && list.Count > 0)
            {
                var duplicates = list.GroupBy(x => x.Student_ID).ToList();
                if (duplicates != null && duplicates.Count > 0)
                    foreach (var item in duplicates)
                    {
                        Guid student_ID = item.Key;
                        var t_item = item.OrderBy(x => x.ExamRollNumber).Last();
                        list.RemoveAll(x => x.Student_ID == item.Key);
                        list.Add(t_item);
                    }
            }
            return list;
        }



        public string GetGrade(decimal cGPA, int GS)
        {
            decimal percentage = cGPA * 10;
            var Query = $"SELECT LetterGrade FROM MSGrades WHERE GradeSystem={GS} AND {percentage}>=FourtyPassPercentageFrom AND {percentage}<FourtyPassPercentageTo ORDER BY GradePoint DESC";
            return new MSSQLFactory().ExecuteScalar<string>(Query);
        }


        public List<Transcript> MarksSheetList(PrintProgramme printProgramme, Parameters parameter)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            var Query = $@"SELECT S.Student_ID,S.FullName,S.FathersName,S.CUSRegistrationNo,E.ExamRollNumber,C.CourseFullName,S.Batch,Col.CollegeFullName,col.Address CollegeAddress,TCourse_ID Course_ID,AcceptCollege_ID
                   ,(SELECT MAX(SemesterBatch) FROM ARGSelectedCombination_UG WHERE Student_ID='{parameter.Filters.Where(x => x.Column == "Student_ID")?.FirstOrDefault()?.Value ?? Guid.Empty.ToString()}') SemesterBatch
                            FROM MSCGPA 
                            JOIN ARGPersonalInformation_{printProgramme.ToString()} S ON S.Student_ID = MSCGPA.Student_ID AND MSCGPA.SGPAType=1
                            JOIN (SELECT DISTINCT Student_ID,ExamRollNumber FROM ARGStudentExamForm_{printProgramme.ToString()} WHERE Semester=1 AND ExamRollNumber IS NOT NULL) E ON E.Student_ID=S.Student_ID
                            JOIN ADMCourseMaster C ON C.Course_ID = TCourse_ID
                            JOIN ADMCollegeMaster Col ON Col.College_ID = S.AcceptCollege_ID ";
            var Helper = new GeneralFunctions().GetWhereClause<FilterModels>(parameter.Filters);
            Helper.Command.CommandText = Query + Helper.WhereClause;// + new GeneralFunctions().GetPagedQuery(Query, parameter);
            var list = new MSSQLFactory().GetObjectList<Transcript>(Helper.Command);
            if (list != null && list.Count > 0)
            {
                var duplicates = list.GroupBy(x => x.Student_ID).ToList();
                if (duplicates != null && duplicates.Count > 0)
                    foreach (var item in duplicates)
                    {
                        Guid student_ID = item.Key;
                        var t_item = item.OrderBy(x => x.ExamRollNumber).Last();
                        list.RemoveAll(x => x.Student_ID == item.Key);
                        list.Add(t_item);
                    }
            }
            return list;
        }

        public List<SelectListItem> GetSemesterToList()
        {
            var Query = @"SELECT distinct Convert(Varchar(10),SemesterTo) AS Value,'Semester '+ Convert(Varchar(10),SemesterTo) AS Text  FROM MSTranscriptDegreeSettings  ORDER BY Value ASC";
            return new MSSQLFactory().GetObjectList<SelectListItem>(Query);
        }
        public List<SelectListItem> GetSemesterFromList()
        {
            var Query = @"SELECT distinct Convert(Varchar(10),SemesterFrom) AS Value,'Semester '+ Convert(Varchar(10),SemesterFrom) AS Text  FROM MSTranscriptDegreeSettings  ORDER BY Value ASC";
            return new MSSQLFactory().GetObjectList<SelectListItem>(Query);
        }
        public int SaveSettings(MSTranscriptDegreeSettings model)
        {
            return new MSSQLFactory().InsertRecord(model);
        }
        public int EditSettings(MSTranscriptDegreeSettings model)
        {
            SqlCommand sqlCommand = new MSSQLFactory().UpdateRecord(model, new List<string>(), new List<string>());
            sqlCommand.CommandText += " WHERE Setting_Id=@Setting_Id";
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }

        public List<MSTranscriptDegreeSettings> GetSettingsList(Parameters parameter)
        {
            var Query = $@"Select MSTranscriptDegreeSettings.*,CourseFullName from MSTranscriptDegreeSettings  join AdmCourseMaster cm on MSTranscriptDegreeSettings.Course_ID=cm.Course_ID";
            var Helper = new GeneralFunctions().GetWhereClause<MSTranscriptDegreeSettings>(parameter.Filters);
            Helper.Command.CommandText = Query + Helper.WhereClause + new GeneralFunctions().GetPagedQuery(Query, parameter);
            return new MSSQLFactory().GetObjectList<MSTranscriptDegreeSettings>(Helper.Command);
        }

        public MSTranscriptDegreeSettings GetSettings(Guid setting_ID)
        {
            var Query = $@"Select sett.*,Programme from
                            MSTranscriptDegreeSettings sett join AdmCourseMaster cm on sett.Course_ID=cm.Course_ID
                            Where Setting_ID='{setting_ID}'";
            return new MSSQLFactory().GetObject<MSTranscriptDegreeSettings>(Query);
        }

        public DataTable NadCertificateData(PrintProgramme printProgramme, Parameters parameter)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            string otherUnivrsityMigrationStatus = "";

            if (parameter.Filters.Find(x => x.Column.ToLower() == "course_id") != null)
            {
                parameter.Filters.Find(x => x.Column.ToLower() == "course_id").Column = "TCourse_ID";
                parameter.Filters.Find(x => x.Column.ToLower() == "tcourse_id").TableAlias = "T";
            }

            if (printProgramme == PrintProgramme.PG)
            {
                otherUnivrsityMigrationStatus = " AND (s.OtherUnivMigrationReceivedOn IS NOT NULL OR s.PreviousUniversityRegnNo NOT LIKE 'CUS-%') ";
            }
            else if (printProgramme == PrintProgramme.IH)
            {
                //IG bed med course and college GCET
                otherUnivrsityMigrationStatus = @" AND
                                                  (
                                                      (S.OtherUnivMigrationReceivedOn IS NOT NULL AND s.IsLateralEntry=1) 
		                                              OR S.PreviousUniversityRegnNo LIKE 'CUS-%' 
		                                              OR S.IsLateralEntry=0 
		                                              OR s.AcceptCollege_ID='9D03A374-4398-4A48-BE2A-FD9911EC6F82'
                                                      OR (c.Course_ID='FC32E138-4EE2-4DA2-9453-5C8368180BC3' AND S.OtherUnivMigrationReceivedOn IS NOT NULL)
                                                  ) ";
            }
            else if (printProgramme == PrintProgramme.UG)
            {
                //b.ed course
                otherUnivrsityMigrationStatus = @"  AND
                                                  (
                                                      (S.OtherUnivMigrationReceivedOn IS NOT NULL AND c.Course_ID='48887D19-F0C3-41CB-AC7A-22EA7B65494A') 
		                                              OR S.PreviousUniversityRegnNo LIKE 'CUS-%' 
		                                              OR c.Course_ID <>'48887D19-F0C3-41CB-AC7A-22EA7B65494A' 
                                                  ) ";

            }

            var Query = $@"SELECT DISTINCT 
                            FORMAT(T.CreatedOn,'dd-MMM-yyyy')T_CreatedOn,FORMAT(T.PrintedOn,'dd-MMM-yyyy')T_PrintedOn,FORMAT(T.HandedOverOn,'dd-MMM-yyyy')T_HandedOverOn,CollegeCode,CourseFullName Course,Batch,'##' [END],
                            'CUS' ORG_CODE,'Cluster University, Srinagar'	ORG_NAME,'' ORG_NAME_L,'Gogji-Bagh'	ORG_ADDRESS,'Srinagar'	ORG_CITY,'Jammu and Kashmir'	ORG_STATE,
                            '190008'	ORG_PIN,C.CourseCode	ACADEMIC_COURSE_ID
                            ,CASE WHEN T.TCourse_ID='15D3B6D6-575C-457B-BB25-2B310C1D1BAF' 
								THEN 
									CASE WHEN Q.Qualification_ID IS NULL THEN 'Master of Arts in Geography' ELSE 'Master of Science in Geography' END
								ELSE 
							        CASE WHEN SGPAType=2 AND T.TCourse_ID='48887D19-F0C3-41CB-AC7A-22EA7B65494A' THEN
                                        'Practice of Teaching (B.Ed)'
                                    ELSE
                                         DS.AwardedDegreeTitle
                                    END
							 END
                             COURSE_NAME
                            ,''	COURSE_NAME_L,''	COURSE_SUBTITLE,
                            ''	STREAM,''	STREAM_L,''	STREAM_SECOND,''	STREAM_SECOND_L,YEAR(T.DateofDeclaration) SESSION,S.CUSRegistrationNo	REGN_NO,E.ExamRollNumber	RROLL,SUBSTRING(S.ABCID ,1,4)+'-'+SUBSTRING(S.ABCID,5,4)+'-'+SUBSTRING(S.ABCID,9,8)  AS ABC_ACCOUNT_ID,
                            ''	LOCKER_ID,Trim(S.FullName)	CNAME,dbo.fngender(gender) GENDER, CAST(FORMAT(S.DOB,'dd/MM/yyyy') AS NVARCHAR(max)) DOB,''	BLOOD_GROUP, dbo.fncategory(S.Category) CASTE,
                            dbo.FNReligion(S.Religion) RELIGION, 'IN'	NATIONALITY, (CASE WHEN SP.Student_ID IS NULL THEN 'N' ELSE 'Y' END)	PH,
                            (CASE WHEN Trim(A.Mobile) IS NULL	OR LEN(TRIM(A.Mobile))<10 THEN '' ELSE TRIM(A.Mobile) END)	MOBILE,
                            Trim(A.Email)EMAIL,	Trim(S.FathersName)	FNAME,Trim(S.MothersName)	MNAME,''	GNAME,Trim(A.PermanentAddress)	STUDENT_ADDRESS,
                            S.CUSRegistrationNo	PHOTO,'O' MRKS_REC_STATUS,'QUALIFIED' RESULT,YEAR(T.DateofDeclaration)	YEAR,
                            FORMAT(T.DateofDeclaration,'MMMM') [Month],''	DIVISION,''	GRADE,''	[PERCENT],''	DOR,''	DOI,''	DOV,
                            ''	CERT_NO,'Regular'	EXAM_TYPE,T.CGPA	CGPA,T.CGPA	OGPA,''	THESIS, dbo.FN_GRADUATION_LEVEL(C.PrintProgramme,C.Programme)	GRADUATION_LEVEL,
                            'Degree completion date:'+FORMAT(T.DateofDeclaration,'dd-MM-yyyy')	REMARKS
                            FROM  dbo.ARGPersonalInformation_{printProgramme} S
                            JOIN dbo.ARGSelectedCombination_{printProgramme} Comb ON Comb.Student_ID = S.Student_ID AND (S.UploadedOnNadDigilocker IS NULL OR S.UploadedOnNadDigilocker=0)
                            JOIN dbo.ARGStudentExamForm_{printProgramme} E ON E.Student_ID = S.Student_ID AND e.ExamRollNumber IS NOT NULL AND ISNUMERIC(E.ExamRollNumber)=1
                            JOIN ARGStudentAddress_{printProgramme} A ON A.Student_ID=S.Student_ID
                            JOIN dbo.ADMCombinationMaster CM ON CM.Combination_ID = Comb.Combination_ID
                            JOIN dbo.MSCGPA T ON T.Student_ID = S.Student_ID AND T.PrintedOn IS NOT NULL
                            JOIN dbo.ADMCourseMaster C ON C.Course_ID = T.TCourse_ID
                            JOIN dbo.ADMCollegeMaster College ON College.College_ID = S.AcceptCollege_ID 
	                        JOIN dbo.MSTranscriptDegreeSettings DS ON DS.Course_ID = C.Course_ID                           
                            LEFT JOIN SpeciallyAbledStudents SP ON SP.Student_ID=S.Student_ID
                            LEFT JOIN ARGStudentPreviousQualifications_PG Q ON Q.Student_ID = S.Student_ID AND C.COURSE_ID='15D3B6D6-575C-457B-BB25-2B310C1D1BAF'
							AND LOWER(ExamName)=LOWER('Graduation') AND (LOWER(REPLACE(stream, '.', '')) LIKE '%med%' OR LOWER(REPLACE(stream, '.', '')) LIKE '%bsc%' OR LOWER(REPLACE(stream, '.', '')) LIKE '%sci%')";
            var Helper = new GeneralFunctions().GetWhereClause<FilterModels>(parameter.Filters);
            Helper.Command.CommandText = Query + Helper.WhereClause + $" {otherUnivrsityMigrationStatus} ORDER BY [SESSION],[year],course_name,S.CUSRegistrationNo";
            var list = new MSSQLFactory().GetDataTable(Helper.Command);
            return list;
        }

        public List<SubjectMarks> GetSubject(Guid Student_ID)
        {
            var Query = $"Select * from MSStudentMarks WHERE Student_ID='{Student_ID}' ORDER By Semester,SGPAType,SubjectType,SubjectTitle";
            return new MSSQLFactory().GetObjectList<SubjectMarks>(Query);
        }

        public List<MSSGPA> GetSGPA(Guid Student_ID)
        {
            var Query = $"Select * from MSSGPA WHERE Student_ID='{Student_ID}' ORDER By Semester,SGPAType,CreditPoints";
            return new MSSQLFactory().GetObjectList<MSSGPA>(Query);
        }
        public List<MSCGPA> GetCGPA(Guid Student_ID,int SemesterTo)
        {
            var Query = $@"Select MSCGPA.*,CASE WHEN DC.DegreeCertificate_ID IS NULL THEN 0 ELSE 1 end DegreeCertificateGenerated 
                        FROM MSCGPA
                        LEFT JOIN dbo.DegreeCertificate DC ON DC.Student_ID = MSCGPA.Student_ID and DC.SemesterTo={SemesterTo}
                        WHERE MSCGPA.Student_ID='{Student_ID}' and MSCGPA.SemesterTo={SemesterTo}
                        ORDER By TotalCreditPoints";
            return new MSSQLFactory().GetObjectList<MSCGPA>(Query);
        }

        public int Validated(Guid student_ID)
        {
            var Query = $"UPDATE MSCGPA SET ValidatedOn=COALESCE(ValidatedOn,GETDATE()),ValidatedBy=COALESCE(ValidatedBy,'{AppUserHelper.User_ID}') WHERE Student_ID='{student_ID}'";
            return new MSSQLFactory().ExecuteNonQuery(Query);
        }
        public int Printed(Guid student_ID)
        {
            var Query = $@"UPDATE MSCGPA SET ValidatedOn=COALESCE(ValidatedOn,GETDATE()),ValidatedBy=COALESCE(ValidatedBy,'{AppUserHelper.User_ID}'),
                                             PrintedOn=COALESCE(PrintedOn,GETDATE()) ,PrintedBy=COALESCE(PrintedBy,'{AppUserHelper.User_ID}')
                            WHERE Student_ID='{student_ID}' AND (PrintedOn IS NULL OR ValidatedOn IS NULL OR ValidatedOn<=PrintedOn)";
            return new MSSQLFactory().ExecuteNonQuery(Query);
        }
        public int HandedOver(Guid student_ID)
        {
            var Query = $@"UPDATE MSCGPA SET ValidatedOn=COALESCE(ValidatedOn,GETDATE()),ValidatedBy=COALESCE(ValidatedBy,'{AppUserHelper.User_ID}'),
                                             PrintedOn=COALESCE(PrintedOn,GETDATE()) ,PrintedBy=COALESCE(PrintedBy,'{AppUserHelper.User_ID}'),
                                            HandedOverOn=COALESCE(HandedOverOn,GETDATE()) ,HandedOverBy_ID=COALESCE(HandedOverBy_ID,'{AppUserHelper.User_ID}')
                            WHERE Student_ID='{student_ID}' AND (HandedOverOn IS NULL OR HandedOverBy_ID IS NULL)";
            return new MSSQLFactory().ExecuteNonQuery(Query);
        }
        #region newtrancriptView
        public List<SubjectMarks> GetSubject(Guid Student_ID, int SemesterFrom, int SemesterTo, Guid Course_ID)
        {
            string squery = (Course_ID == Guid.Empty || Course_ID == null) ? "" : $" AND Tcourse_ID='{Course_ID}'";
            var Query = $"Select * from MSStudentMarks WHERE Student_ID='{Student_ID}' and Semester >={SemesterFrom} and Semester <={SemesterTo} {squery} ORDER By Semester,SGPAType,SubjectType,SubjectTitle";
            return new MSSQLFactory().GetObjectList<SubjectMarks>(Query);
        }

        public List<MSSGPA> GetSGPA(Guid Student_ID, int SemesterFrom, int SemesterTo, Guid Course_ID)
        {
            string squery = (Course_ID == Guid.Empty || Course_ID == null) ? "" : $" AND Tcourse_ID='{Course_ID}'";
            var Query = $"Select * from MSSGPA WHERE Student_ID='{Student_ID}'  and Semester >={SemesterFrom} and Semester <={SemesterTo} {squery} ORDER By Semester,SGPAType,CreditPoints";
            return new MSSQLFactory().GetObjectList<MSSGPA>(Query);
        }
        public List<MSCGPA> GetCGPA(Guid Student_ID, int SemesterTo, Guid Course_ID)
        {
            string squery = (Course_ID == Guid.Empty || Course_ID == null) ? "" : $" AND Tcourse_ID='{Course_ID}'";
            var Query = $@"Select MSCGPA.*,CASE WHEN DC.DegreeCertificate_ID IS NULL THEN 0 ELSE 1 end DegreeCertificateGenerated 
                        FROM MSCGPA
                        LEFT JOIN dbo.DegreeCertificate DC ON DC.Student_ID = MSCGPA.Student_ID 
                        WHERE MSCGPA.Student_ID='{Student_ID}'   and MSCGPA.SemesterTo ={SemesterTo} {squery}
                        ORDER By TotalCreditPoints";
            return new MSSQLFactory().GetObjectList<MSCGPA>(Query);
        }

        public int Validated(Guid student_ID, int SemesterTo, Guid Course_ID)
        {
            string squery = (Course_ID == Guid.Empty || Course_ID == null) ? "" : $" AND Tcourse_ID='{Course_ID}'";
            string sbquery = (SemesterTo == 0) ? "" : $"  and SemesterTo ={SemesterTo}";
            var Query = $"UPDATE MSCGPA SET ValidatedOn=COALESCE(ValidatedOn,GETDATE()),ValidatedBy=COALESCE(ValidatedBy,'{AppUserHelper.User_ID}') WHERE Student_ID='{student_ID}' {sbquery}  {squery}";
            return new MSSQLFactory().ExecuteNonQuery(Query);
        }
        public int Printed(Guid student_ID, int SemesterTo, Guid Course_ID)
        {
            string squery = (Course_ID == Guid.Empty || Course_ID == null) ? "" : $" AND Tcourse_ID='{Course_ID}'";
            string sbquery = (SemesterTo == 0) ? "" : $"  and SemesterTo ={SemesterTo}";
            var Query = $@"UPDATE MSCGPA SET ValidatedOn=COALESCE(ValidatedOn,GETDATE()),ValidatedBy=COALESCE(ValidatedBy,'{AppUserHelper.User_ID}'),
                                             PrintedOn=COALESCE(PrintedOn,GETDATE()) ,PrintedBy=COALESCE(PrintedBy,'{AppUserHelper.User_ID}')
                            WHERE Student_ID='{student_ID}' {sbquery}  {squery} AND (PrintedOn IS NULL OR ValidatedOn IS NULL OR ValidatedOn<=PrintedOn)";
            return new MSSQLFactory().ExecuteNonQuery(Query);
        }
        public int HandedOver(Guid student_ID, int SemesterTo, Guid Course_ID)
        {
            string sbquery = (SemesterTo == 0) ? "" : $"  and SemesterTo ={SemesterTo}";
            string squery = (Course_ID == Guid.Empty || Course_ID == null) ? "" : $" AND Tcourse_ID='{Course_ID}'";
            var Query = $@"UPDATE MSCGPA SET ValidatedOn=COALESCE(ValidatedOn,GETDATE()),ValidatedBy=COALESCE(ValidatedBy,'{AppUserHelper.User_ID}'),
                                             PrintedOn=COALESCE(PrintedOn,GETDATE()) ,PrintedBy=COALESCE(PrintedBy,'{AppUserHelper.User_ID}'),
                                            HandedOverOn=COALESCE(HandedOverOn,GETDATE()) ,HandedOverBy_ID=COALESCE(HandedOverBy_ID,'{AppUserHelper.User_ID}')
                            WHERE Student_ID='{student_ID}'  {sbquery} {squery} AND (HandedOverOn IS NULL OR HandedOverBy_ID IS NULL)";
            return new MSSQLFactory().ExecuteNonQuery(Query);
        }
        #endregion
        #region TranscriptGeneration

        public List<TranscriptNotification> TranscriptGenerationList(Parameters parameter)
        {
            var Query = $@"Select Notification.*,C.CourseFullName from
                            TranscriptNotification Notification
                            JOIN ADMCourseMaster C ON Notification.Course_ID=C.Course_ID";
            var Helper = new GeneralFunctions().GetWhereClause<ResultCompact>(parameter.Filters);
            Helper.Command.CommandText = Query + Helper.WhereClause + new GeneralFunctions().GetPagedQuery(Query, parameter);
            return new MSSQLFactory().GetObjectList<TranscriptNotification>(Helper.Command);
        }

        public TranscriptNotification TranscriptGeneration(Guid TranscriptGeneration_ID)
        {
            var Query = $@"Select Notification.*,C.CourseFullName from
                            TranscriptNotification Notification
                            JOIN ADMCourseMaster C ON Notification.Course_ID=C.Course_ID
                            Where TranscriptGeneration_ID='{TranscriptGeneration_ID}'";
            return new MSSQLFactory().GetObject<TranscriptNotification>(Query);
        }

        public DataSet ResultStatisticsDataTable(short Batch, Guid Course_ID)
        {
            PrintProgramme printProgramme = new CourseDB().GetCourseById(Course_ID).PrintProgramme;
            var Query = $@"SELECT Sub.CourseFullName,C.Semester,dbo.FNSubjectTypeDescription(Sub.SubjectType)SubjectType,Sub.TotalCredit,Sub.SubjectFullName,COUNT(*) [Count]
                        FROM ARGPersonalInformation_{printProgramme} S
                        JOIN ARGSelectedCombination_{printProgramme} C ON C.Student_ID = S.Student_ID
                        JOIN VWSCMasterCompact Sub ON Sub.Combination_ID = C.Combination_ID
                        WHERE S.Batch=2017 AND Sub.Course_ID='{Course_ID}'
                        GROUP BY Sub.CourseFullName,C.Semester,dbo.FNSubjectTypeDescription(Sub.SubjectType),Sub.TotalCredit,Sub.SubjectFullName
                        ORDER BY C.Semester,dbo.FNSubjectTypeDescription(Sub.SubjectType),Sub.TotalCredit,Sub.SubjectFullName";
            return new MSSQLFactory().GetDataSet(Query);
        }

        public int ReplaceSubject(PrintProgramme printProgramme, Guid combination_ID, Guid FindSubject_ID, Guid ReplaceSubject_ID)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            var query = $@"UPDATE  MSStudentMarks SET Subject_ID='{ReplaceSubject_ID}',SubjectTitle=S.SubjectFullName
                            FROM MSStudentMarks R
                            JOIN ARGSelectedCombination_{printProgramme.ToString()} C ON C.Student_ID = R.Student_Id
                            JOIN ADMCombinationMaster CM ON CM.Combination_ID = C.Combination_ID
                            JOIN ADMSubjectMaster S ON S.Subject_ID='{ReplaceSubject_ID}'
                            WHERE C.Combination_ID='combination_ID' AND CM.CombinationSubjects LIKE '%{ReplaceSubject_ID}%' 
                            AND R.Subject_ID='{FindSubject_ID}'";
            return new MSSQLFactory().ExecuteNonQuery(query);
        }

        public bool HasTranscript(Guid student_ID)
        {
            var query = $@"Select TOP 1 1 FROM dbo.MSCGPA WHERE Student_ID='{student_ID}'";
            return new MSSQLFactory().ExecuteScalar<int>(query) > 0;
        }

        #endregion

    }
}

