using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using CUSrinagar.Models;
using GeneralModels;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace CUSrinagar.DataManagers.SQLQueries
{
    public class ResultSQLQueries
    {
        internal string StudentInfo(PrintProgramme printProgramme, short? semester, bool IsEditable = false)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            var Query = $@"SELECT  AcceptCollege_ID,S.Student_ID,Gender,CUSRegistrationNo,Category,ClassRollNo,ExamRollNumber,FullName,FathersName,S.Batch,CollegeFullName
	                        {(semester.HasValue ? ",Comb.Semester,SemesterBatch" : "")},Course.Programme,Course.PrintProgramme,Course.Course_ID,Course.CourseFullName
	                        ,Cast((CASE WHEN t.Student_ID IS NULL THEN 0 ELSE 1 END) AS BIT)HasTranscript,CAST((CASE WHEN t.PrintedOn IS NULL THEN 0 ELSE 1 END) AS BIT)TranscriptPrinted,DegreeCourseTitle
                        FROM dbo.ARGPersonalInformation_{printProgramme.ToString()} S
                        JOIN  ARGSelectedCombination_{printProgramme.ToString()} Comb ON  {(semester.HasValue ? $" Comb.Semester={semester} AND" : "")} Comb.IsVerified=1 AND Comb.Student_ID = S.Student_ID AND Comb.Semester <= S.CurrentSemesterOrYear
                        JOIN ADMCombinationMaster CM ON Comb.Semester=CM.Semester AND CM.Combination_ID = Comb.Combination_ID
                        JOIN ADMCourseMaster Course  ON Course.Course_ID = CM.Course_ID
                        JOIN ADMCollegeMaster ON ADMCollegeMaster.College_ID=S.AcceptCollege_ID
                        {(IsEditable ? "LEFT" : "")} JOIN ARGStudentExamForm_{printProgramme.ToString()} E ON ExamRollNumber IS NOT NULL AND E.Student_ID = S.Student_ID AND {(semester.HasValue ? "E.Semester=Comb.Semester" : "E.Semester=1")}
                        {(IsEditable ? "LEFT" : "")} JOIN dbo.{printProgramme.ToString()}_Semester{(semester.HasValue && semester.Value > 0 ? semester.Value.ToString() : "All")} R ON R.Student_ID = S.Student_ID
                        {(IsEditable ? "LEFT" : "")} JOIN ADMSubjectMaster Subj on  Subj.Subject_ID=R.Subject_ID
                        {(IsEditable ? "LEFT" : "")} JOIN dbo.ResultNotification RN ON RN.ResultNotification_ID=R.ResultNotification_ID
                        LEFT JOIN dbo.MSCGPA T ON T.Student_ID = S.Student_ID
                        _WHERE_CLAUSE_
                        GROUP BY AcceptCollege_ID,S.Student_ID,S.Gender,S.CUSRegistrationNo,S.Category,S.ClassRollNo,E.ExamRollNumber,S.FullName,s.FathersName,S.Batch,CollegeFullName
	                        {(semester.HasValue ? ",Comb.Semester,Comb.SemesterBatch" : "")},Course.Programme,Course.PrintProgramme,Course.Course_ID,Course.CourseFullName,DegreeCourseTitle
	                        ,(CASE WHEN t.Student_ID IS NULL THEN 0 ELSE 1 END),(CASE WHEN t.PrintedOn IS NULL THEN 0 ELSE 1 END)";
            return Query;
        }

        internal string StudentInfoDetails(PrintProgramme printProgramme, short? semester, bool IsEditable = false)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            var Query = $@"SELECT top(1)  AcceptCollege_ID,S.Student_ID,Gender,CUSRegistrationNo,Category,ClassRollNo,ExamRollNumber,FullName,FathersName,S.Batch,CollegeFullName
	                        {(semester.HasValue ? ",Comb.Semester,SemesterBatch" : "")},Course.Programme,Course.PrintProgramme,Course.Course_ID,Course.CourseFullName
	                        ,Cast((CASE WHEN t.Student_ID IS NULL THEN 0 ELSE 1 END) AS BIT)HasTranscript,CAST((CASE WHEN t.PrintedOn IS NULL THEN 0 ELSE 1 END) AS BIT)TranscriptPrinted,DegreeCourseTitle,RN.NotificationNo,RN.CreatedOn,Address
                        FROM dbo.ARGPersonalInformation_{printProgramme.ToString()} S
                        JOIN  ARGSelectedCombination_{printProgramme.ToString()} Comb ON  {(semester.HasValue ? $" Comb.Semester={semester} AND" : "")} Comb.IsVerified=1 AND Comb.Student_ID = S.Student_ID AND Comb.Semester <= S.CurrentSemesterOrYear
                        JOIN ADMCombinationMaster CM ON Comb.Semester=CM.Semester AND CM.Combination_ID = Comb.Combination_ID
                        JOIN ADMCourseMaster Course  ON Course.Course_ID = CM.Course_ID
                        JOIN ADMCollegeMaster ON ADMCollegeMaster.College_ID=S.AcceptCollege_ID
                        {(IsEditable ? "LEFT" : "")} JOIN ARGStudentExamForm_{printProgramme.ToString()} E ON ExamRollNumber IS NOT NULL AND E.Student_ID = S.Student_ID AND {(semester.HasValue ? "E.Semester=Comb.Semester" : "E.Semester=1")}
                        {(IsEditable ? "LEFT" : "")} JOIN dbo.{printProgramme.ToString()}_Semester{(semester.HasValue && semester.Value > 0 ? semester.Value.ToString() : "All")} R ON R.Student_ID = S.Student_ID
                        {(IsEditable ? "LEFT" : "")} JOIN dbo.ResultNotification RN ON RN.ResultNotification_ID=R.ResultNotification_ID
                        LEFT JOIN dbo.MSCGPA T ON T.Student_ID = S.Student_ID
                        _WHERE_CLAUSE_
                        GROUP BY AcceptCollege_ID,S.Student_ID,S.Gender,S.CUSRegistrationNo,S.Category,S.ClassRollNo,E.ExamRollNumber,S.FullName,s.FathersName,S.Batch,CollegeFullName
	                        {(semester.HasValue ? ",Comb.Semester,Comb.SemesterBatch" : "")},Course.Programme,Course.PrintProgramme,Course.Course_ID,Course.CourseFullName,DegreeCourseTitle,RN.NotificationNo,RN.CreatedOn,Address
	                        ,(CASE WHEN t.Student_ID IS NULL THEN 0 ELSE 1 END),(CASE WHEN t.PrintedOn IS NULL THEN 0 ELSE 1 END)";
            return Query;
        }


        internal string StudentInfoDetailsForEngFullGazette(PrintProgramme printProgramme, short? semester, bool IsEditable = false)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            var Query = $@"SELECT  AcceptCollege_ID,S.Student_ID,Gender,CUSRegistrationNo,Category,ClassRollNo,ExamRollNumber,FullName,FathersName,S.Batch,CollegeFullName
                         {(semester.HasValue ? ",Comb.Semester,SemesterBatch" : "")},Course.Programme,Course.PrintProgramme,Course.Course_ID,Course.CourseFullName
                         ,Cast((CASE WHEN t.Student_ID IS NULL THEN 0 ELSE 1 END) AS BIT)HasTranscript,CAST((CASE WHEN t.PrintedOn IS NULL THEN 0 ELSE 1 END) AS BIT)TranscriptPrinted,DegreeCourseTitle,Address
                        FROM dbo.ARGPersonalInformation_{printProgramme.ToString()} S
                        JOIN  ARGSelectedCombination_{printProgramme.ToString()} Comb ON  {(semester.HasValue ? $" Comb.Semester={semester} AND" : "")} Comb.IsVerified=1 AND Comb.Student_ID = S.Student_ID AND Comb.Semester <= S.CurrentSemesterOrYear
                        JOIN ADMCombinationMaster CM ON Comb.Semester=CM.Semester AND CM.Combination_ID = Comb.Combination_ID
                        JOIN ADMCourseMaster Course  ON Course.Course_ID = CM.Course_ID
                        JOIN ADMCollegeMaster ON ADMCollegeMaster.College_ID=S.AcceptCollege_ID
                        {(IsEditable ? "LEFT" : "")} JOIN ARGStudentExamForm_{printProgramme.ToString()} E ON ExamRollNumber IS NOT NULL AND E.Student_ID = S.Student_ID AND {(semester.HasValue ? "E.Semester=Comb.Semester" : "E.Semester=1")}
                        {(IsEditable ? "LEFT" : "")} JOIN dbo.{printProgramme.ToString()}_Semester{(semester.HasValue && semester.Value > 0 ? semester.Value.ToString() : "All")} R ON R.Student_ID = S.Student_ID
                        --{(IsEditable ? "LEFT" : "")} JOIN dbo.ResultNotification RN ON RN.ResultNotification_ID=R.ResultNotification_ID
                        LEFT JOIN dbo.MSCGPA T ON T.Student_ID = S.Student_ID
                        _WHERE_CLAUSE_
                        GROUP BY AcceptCollege_ID,S.Student_ID,S.Gender,S.CUSRegistrationNo,S.Category,S.ClassRollNo,E.ExamRollNumber,S.FullName,s.FathersName,S.Batch,CollegeFullName
                         {(semester.HasValue ? ",Comb.Semester,Comb.SemesterBatch" : "")},Course.Programme,Course.PrintProgramme,Course.Course_ID,Course.CourseFullName,DegreeCourseTitle,Address
                         ,(CASE WHEN t.Student_ID IS NULL THEN 0 ELSE 1 END),(CASE WHEN t.PrintedOn IS NULL THEN 0 ELSE 1 END)";
            return Query;
        }


        internal string StudentInfoDetailsForEng(PrintProgramme printProgramme, short? semester, bool IsEditable = false)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            var Query = $@"SELECT  AcceptCollege_ID,S.Student_ID,Gender,CUSRegistrationNo,Category,ClassRollNo,ExamRollNumber,FullName,FathersName,S.Batch,CollegeFullName
                         {(semester.HasValue ? ",Comb.Semester,SemesterBatch" : "")},Course.Programme,Course.PrintProgramme,Course.Course_ID,Course.CourseFullName
                         ,Cast((CASE WHEN t.Student_ID IS NULL THEN 0 ELSE 1 END) AS BIT)HasTranscript,CAST((CASE WHEN t.PrintedOn IS NULL THEN 0 ELSE 1 END) AS BIT)TranscriptPrinted,DegreeCourseTitle,RN.NotificationNo,RN.CreatedOn,Address
                        FROM dbo.ARGPersonalInformation_{printProgramme.ToString()} S
                        JOIN  ARGSelectedCombination_{printProgramme.ToString()} Comb ON  {(semester.HasValue ? $" Comb.Semester={semester} AND" : "")} Comb.IsVerified=1 AND Comb.Student_ID = S.Student_ID AND Comb.Semester <= S.CurrentSemesterOrYear
                        JOIN ADMCombinationMaster CM ON Comb.Semester=CM.Semester AND CM.Combination_ID = Comb.Combination_ID
                        JOIN ADMCourseMaster Course  ON Course.Course_ID = CM.Course_ID
                        JOIN ADMCollegeMaster ON ADMCollegeMaster.College_ID=S.AcceptCollege_ID
                        {(IsEditable ? "LEFT" : "")} JOIN ARGStudentExamForm_{printProgramme.ToString()} E ON ExamRollNumber IS NOT NULL AND E.Student_ID = S.Student_ID AND {(semester.HasValue ? "E.Semester=Comb.Semester" : "E.Semester=1")}
                        {(IsEditable ? "LEFT" : "")} JOIN dbo.{printProgramme.ToString()}_Semester{(semester.HasValue && semester.Value > 0 ? semester.Value.ToString() : "All")} R ON R.Student_ID = S.Student_ID
                        {(IsEditable ? "LEFT" : "")} JOIN dbo.ResultNotification RN ON RN.ResultNotification_ID=R.ResultNotification_ID
                        LEFT JOIN dbo.MSCGPA T ON T.Student_ID = S.Student_ID
                        _WHERE_CLAUSE_
                        GROUP BY AcceptCollege_ID,S.Student_ID,S.Gender,S.CUSRegistrationNo,S.Category,S.ClassRollNo,E.ExamRollNumber,S.FullName,s.FathersName,S.Batch,CollegeFullName
                         {(semester.HasValue ? ",Comb.Semester,Comb.SemesterBatch" : "")},Course.Programme,Course.PrintProgramme,Course.Course_ID,Course.CourseFullName,DegreeCourseTitle,RN.NotificationNo,RN.CreatedOn,Address
                         ,(CASE WHEN t.Student_ID IS NULL THEN 0 ELSE 1 END),(CASE WHEN t.PrintedOn IS NULL THEN 0 ELSE 1 END)";
            return Query;
        }



        internal string GetFinalData(Parameters Parameter, PrintProgramme printProgramme, short? semester, bool IsEditable = false)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            SearchFilter programefilter = Parameter.Filters.Where(x => x.Column == "Programme").FirstOrDefault();
            string Programme = string.Empty;
            if (programefilter != null)
            {
                Programme = programefilter.Value;
            }

            SearchFilter batchfilter = Parameter.Filters.Where(x => x.Column == "SemesterBatch").FirstOrDefault();
            string batch = string.Empty;
            if (batchfilter != null)
            {
                batch = batchfilter.Value;
            }
            SearchFilter coursefilter = Parameter.Filters.Where(x => x.Column == "Course_ID").FirstOrDefault();
            string CourseId = string.Empty;
            if (coursefilter != null)
            {
                CourseId = coursefilter.Value;
            }
            SearchFilter collegefilter = Parameter.Filters.Where(x => x.Column == "AcceptCollege_ID").FirstOrDefault();
            string AcceptCollege_ID = string.Empty;
            if (collegefilter != null)
            {
                AcceptCollege_ID = collegefilter.Value;
            }


            //var Query = $@"SELECT  AcceptCollege_ID,S.Student_ID,Gender,CUSRegistrationNo,Category,ClassRollNo,ExamRollNumber,FullName,FathersName,S.Batch,CollegeFullName
            //             {(semester.HasValue ? ",Comb.Semester,SemesterBatch" : "")},Course.Programme,Course.PrintProgramme,Course.Course_ID,Course.CourseFullName
            //             ,Cast((CASE WHEN t.Student_ID IS NULL THEN 0 ELSE 1 END) AS BIT)HasTranscript,CAST((CASE WHEN t.PrintedOn IS NULL THEN 0 ELSE 1 END) AS BIT)TranscriptPrinted,DegreeCourseTitle,RN.NotificationNo,RN.CreatedOn,Address
            //            FROM dbo.ARGPersonalInformation_{printProgramme.ToString()} S
            //            JOIN  ARGSelectedCombination_{printProgramme.ToString()} Comb ON  {(semester.HasValue ? $" Comb.Semester={semester} AND" : "")} Comb.IsVerified=1 AND Comb.Student_ID = S.Student_ID AND Comb.Semester <= S.CurrentSemesterOrYear
            //            JOIN ADMCombinationMaster CM ON Comb.Semester=CM.Semester AND CM.Combination_ID = Comb.Combination_ID
            //            JOIN ADMCourseMaster Course  ON Course.Course_ID = CM.Course_ID
            //            JOIN ADMCollegeMaster ON ADMCollegeMaster.College_ID=S.AcceptCollege_ID
            //            {(IsEditable ? "LEFT" : "")} JOIN ARGStudentExamForm_{printProgramme.ToString()} E ON ExamRollNumber IS NOT NULL AND E.Student_ID = S.Student_ID AND {(semester.HasValue ? "E.Semester=Comb.Semester" : "E.Semester=1")}
            //            {(IsEditable ? "LEFT" : "")} JOIN dbo.{printProgramme.ToString()}_Semester{(semester.HasValue && semester.Value > 0 ? semester.Value.ToString() : "All")} R ON R.Student_ID = S.Student_ID
            //            {(IsEditable ? "LEFT" : "")} JOIN dbo.ResultNotification RN ON RN.ResultNotification_ID=R.ResultNotification_ID
            //            LEFT JOIN dbo.MSCGPA T ON T.Student_ID = S.Student_ID
            //            WHERE (R.ResultNotification_ID IS NOT NULL  OR R.ExamForm_ID IS NOT NULL ) AND (SemesterBatch = {batch}) AND (AcceptCollege_ID = '{AcceptCollege_ID}')
            //            AND (Course.Programme ={Programme}) AND (Course.Course_ID = '{CourseId}') AND (Comb.Semester = {semester})
            //             AND S.Student_ID NOT IN (
            //                      SELECT DISTINCT
            //                          P.Student_ID
            //                      FROM dbo.ARGPersonalInformation_IH P
            //                          JOIN dbo.IH_SemesterAll R
            //                              ON R.Student_ID = P.Student_ID
            //                          LEFT JOIN dbo.ResultNotification RN
            //                              ON RN.ResultNotification_ID = R.ResultNotification_ID
            //                          JOIN dbo.ARGSelectedCombination_IH SC
            //                              ON SC.Student_ID = P.Student_ID
            //                          JOIN dbo.ADMCombinationMaster CM
            //                              ON CM.Combination_ID = SC.Combination_ID
            //                          JOIN dbo.ADMCourseMaster CMs
            //                              ON CMs.Course_ID = CM.Course_ID
            //                      WHERE CMs.Programme = {Programme}
            //                            AND CM.Course_ID = '{CourseId}'
            //                            AND SemesterBatch = {batch}
            //                            AND R.ResultNotification_ID IS NULL
            //                  )

            //            GROUP BY AcceptCollege_ID,S.Student_ID,S.Gender,S.CUSRegistrationNo,S.Category,S.ClassRollNo,E.ExamRollNumber,S.FullName,s.FathersName,S.Batch,CollegeFullName
            //             {(semester.HasValue ? ",Comb.Semester,Comb.SemesterBatch" : "")},Course.Programme,Course.PrintProgramme,Course.Course_ID,Course.CourseFullName,DegreeCourseTitle,RN.NotificationNo,RN.CreatedOn,Address
            //             ,(CASE WHEN t.Student_ID IS NULL THEN 0 ELSE 1 END),(CASE WHEN t.PrintedOn IS NULL THEN 0 ELSE 1 END)";





            var Query = $@"SELECT  DISTINCT AcceptCollege_ID,FD.Student_Id,Gender,P.CUSRegistrationNo,P.Category,P.ClassRollNo,E.ExamRollNumber,P.FullName,P.FathersName,P.Batch,CMt.CollegeFullName
	                        {(semester.HasValue ? ",Comb.Semester,SemesterBatch" : "")},CMs.Programme,CMs.PrintProgramme,ST.TCourse_ID,CMs.CourseFullName
	                        
                        FROM dbo.ARGPersonalInformation_{printProgramme.ToString()} S
                        JOIN  ARGSelectedCombination_{printProgramme.ToString()} Comb ON  {(semester.HasValue ? $" Comb.Semester={semester} AND" : "")} Comb.IsVerified=1 AND Comb.Student_ID = S.Student_ID AND Comb.Semester <= S.CurrentSemesterOrYear
                        JOIN ADMCombinationMaster CM ON Comb.Semester=CM.Semester AND CM.Combination_ID = Comb.Combination_ID
                        JOIN ADMCourseMaster Course  ON Course.Course_ID = CM.Course_ID
                        JOIN ADMCollegeMaster ON ADMCollegeMaster.College_ID=S.AcceptCollege_ID
                        {(IsEditable ? "LEFT" : "")} JOIN ARGStudentExamForm_{printProgramme.ToString()} E ON ExamRollNumber IS NOT NULL AND E.Student_ID = S.Student_ID AND {(semester.HasValue ? "E.Semester=Comb.Semester" : "E.Semester=1")}
                        {(IsEditable ? "LEFT" : "")} JOIN dbo.{printProgramme.ToString()}_Semester{(semester.HasValue && semester.Value > 0 ? semester.Value.ToString() : "All")} R ON R.Student_ID = S.Student_ID
                        {(IsEditable ? "LEFT" : "")} JOIN dbo.ResultNotification RN ON RN.ResultNotification_ID=R.ResultNotification_ID
                        LEFT JOIN dbo.MSCGPA T ON T.Student_ID = S.Student_ID
                        WHERE (R.ResultNotification_ID IS NOT NULL  OR R.ExamForm_ID IS NOT NULL ) AND (SemesterBatch = {batch}) AND (AcceptCollege_ID = '{AcceptCollege_ID}')
                        AND (Course.Programme ={Programme}) AND (Course.Course_ID = '{CourseId}') AND (Comb.Semester = {semester})
                         AND S.Student_ID NOT IN (
                                  SELECT DISTINCT
                                      P.Student_ID
                                  FROM dbo.ARGPersonalInformation_IH P
                                      JOIN dbo.IH_SemesterAll R
                                          ON R.Student_ID = P.Student_ID
                                      LEFT JOIN dbo.ResultNotification RN
                                          ON RN.ResultNotification_ID = R.ResultNotification_ID
                                      JOIN dbo.ARGSelectedCombination_IH SC
                                          ON SC.Student_ID = P.Student_ID
                                      JOIN dbo.ADMCombinationMaster CM
                                          ON CM.Combination_ID = SC.Combination_ID
                                      JOIN dbo.ADMCourseMaster CMs
                                          ON CMs.Course_ID = CM.Course_ID
                                  WHERE CMs.Programme = {Programme}
                                        AND CM.Course_ID = '{CourseId}'
                                        AND SemesterBatch = {batch}
                                        AND R.ResultNotification_ID IS NULL
                              )

                        GROUP BY AcceptCollege_ID,S.Student_ID,S.Gender,S.CUSRegistrationNo,S.Category,S.ClassRollNo,E.ExamRollNumber,S.FullName,s.FathersName,S.Batch,CollegeFullName
	                        {(semester.HasValue ? ",Comb.Semester,Comb.SemesterBatch" : "")},Course.Programme,Course.PrintProgramme,Course.Course_ID,Course.CourseFullName,DegreeCourseTitle,RN.NotificationNo,RN.CreatedOn,Address
	                        ,(CASE WHEN t.Student_ID IS NULL THEN 0 ELSE 1 END),(CASE WHEN t.PrintedOn IS NULL THEN 0 ELSE 1 END)";

















            return Query;
        }


        internal string StudentInfoDetailsForEngForInsertion(Parameters Parameter, PrintProgramme printProgramme, short? semester, bool IsEditable = false)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            SearchFilter programefilter = Parameter.Filters.Where(x => x.Column == "Programme").FirstOrDefault();
            string Programme = string.Empty;
            if (programefilter != null)
            {
                Programme = programefilter.Value;
            }

            SearchFilter batchfilter = Parameter.Filters.Where(x => x.Column == "SemesterBatch").FirstOrDefault();
            string batch = string.Empty;
            if (batchfilter != null)
            {
                batch = batchfilter.Value;
            }
            SearchFilter coursefilter = Parameter.Filters.Where(x => x.Column == "Course_ID").FirstOrDefault();
            string CourseId = string.Empty;
            if (coursefilter != null)
            {
                CourseId = coursefilter.Value;
            }
            SearchFilter collegefilter = Parameter.Filters.Where(x => x.Column == "AcceptCollege_ID").FirstOrDefault();
            string AcceptCollege_ID = string.Empty;
            if (collegefilter != null)
            {
                AcceptCollege_ID = collegefilter.Value;
            }


            var Query = $@"SELECT  AcceptCollege_ID,S.Student_ID,Gender,CUSRegistrationNo,Category,ClassRollNo,ExamRollNumber,FullName,FathersName,S.Batch,CollegeFullName
	                        {(semester.HasValue ? ",Comb.Semester,SemesterBatch" : "")},Course.Programme,Course.PrintProgramme,Course.Course_ID,Course.CourseFullName
	                        ,Cast((CASE WHEN t.Student_ID IS NULL THEN 0 ELSE 1 END) AS BIT)HasTranscript,CAST((CASE WHEN t.PrintedOn IS NULL THEN 0 ELSE 1 END) AS BIT)TranscriptPrinted,DegreeCourseTitle,RN.NotificationNo,RN.CreatedOn,Address
                        FROM dbo.ARGPersonalInformation_{printProgramme.ToString()} S
                        JOIN  ARGSelectedCombination_{printProgramme.ToString()} Comb ON  {(semester.HasValue ? $" Comb.Semester={semester} AND" : "")} Comb.IsVerified=1 AND Comb.Student_ID = S.Student_ID AND Comb.Semester <= S.CurrentSemesterOrYear
                        JOIN ADMCombinationMaster CM ON Comb.Semester=CM.Semester AND CM.Combination_ID = Comb.Combination_ID
                        JOIN ADMCourseMaster Course  ON Course.Course_ID = CM.Course_ID
                        JOIN ADMCollegeMaster ON ADMCollegeMaster.College_ID=S.AcceptCollege_ID
                        {(IsEditable ? "LEFT" : "")} JOIN ARGStudentExamForm_{printProgramme.ToString()} E ON ExamRollNumber IS NOT NULL AND E.Student_ID = S.Student_ID AND {(semester.HasValue ? "E.Semester=Comb.Semester" : "E.Semester=1")}
                        {(IsEditable ? "LEFT" : "")} JOIN dbo.{printProgramme.ToString()}_Semester{(semester.HasValue && semester.Value > 0 ? semester.Value.ToString() : "All")} R ON R.Student_ID = S.Student_ID
                        {(IsEditable ? "LEFT" : "")} JOIN dbo.ResultNotification RN ON RN.ResultNotification_ID=R.ResultNotification_ID
                        LEFT JOIN dbo.MSCGPA T ON T.Student_ID = S.Student_ID
                        WHERE (R.ResultNotification_ID IS NOT NULL  OR R.ExamForm_ID IS NOT NULL ) AND (SemesterBatch = {batch}) AND (AcceptCollege_ID = '{AcceptCollege_ID}')
                        AND (Course.Programme ={Programme}) AND (Course.Course_ID = '{CourseId}') AND (Comb.Semester = {semester})
                         AND S.Student_ID NOT IN (
                                  SELECT DISTINCT
                                      P.Student_ID
                                  FROM dbo.ARGPersonalInformation_IH P
                                      JOIN dbo.IH_SemesterAll R
                                          ON R.Student_ID = P.Student_ID
                                      LEFT JOIN dbo.ResultNotification RN
                                          ON RN.ResultNotification_ID = R.ResultNotification_ID
                                      JOIN dbo.ARGSelectedCombination_IH SC
                                          ON SC.Student_ID = P.Student_ID
                                      JOIN dbo.ADMCombinationMaster CM
                                          ON CM.Combination_ID = SC.Combination_ID
                                      JOIN dbo.ADMCourseMaster CMs
                                          ON CMs.Course_ID = CM.Course_ID
                                  WHERE CMs.Programme = {Programme}
                                        AND CM.Course_ID = '{CourseId}'
                                        AND SemesterBatch = {batch}
                                        AND R.ResultNotification_ID IS NULL
                              )

                        GROUP BY AcceptCollege_ID,S.Student_ID,S.Gender,S.CUSRegistrationNo,S.Category,S.ClassRollNo,E.ExamRollNumber,S.FullName,s.FathersName,S.Batch,CollegeFullName
	                        {(semester.HasValue ? ",Comb.Semester,Comb.SemesterBatch" : "")},Course.Programme,Course.PrintProgramme,Course.Course_ID,Course.CourseFullName,DegreeCourseTitle,RN.NotificationNo,RN.CreatedOn,Address
	                        ,(CASE WHEN t.Student_ID IS NULL THEN 0 ELSE 1 END),(CASE WHEN t.PrintedOn IS NULL THEN 0 ELSE 1 END)";
            return Query;
        }



        internal string SubjectResult(PrintProgramme printProgramme, short? semester, bool Editable = false)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            string Query = $@"SELECT 
                            Comb.Student_ID,Comb.Semester,Comb.SemesterBatch,Course.Programme,Course.PrintProgramme,MS.TotalCredit
                            ,R._ID,CM.Subject_ID
                            ,SM.SubjectFullName,SM.SubjectType,SM.SubjectCode,SM.HasResult,SM.HasExaminationFee
                            ,IsInternalMarksApplicable ,CAST(MS.InternalMaxMarks AS INT)InternalMaxMarks,MS.InternalIsPartOf,Cast(MS.InternalMinPassMarks as INT)InternalMinPassMarks
                            ,IsInternalAttendance_AssessmentMarksApplicable,cast(MS.InternalAttendance_AssessmentMaxMarks as int)InternalAttendance_AssessmentMaxMarks,Cast(MS.InternalAttendance_AssessmentMinPassMarks AS INT)InternalAttendance_AssessmentMinPassMarks
                            ,MS.InternalAttendance_AssessmentIsPartOf
                            ,MS.IsExternalAttendance_AssessmentMarksApplicable,CAST(MS.ExternalAttendance_AssessmentMaxMarks AS INT)ExternalAttendance_AssessmentMaxMarks,
                            CAST(MS.ExternalAttendance_AssessmentMinPassMarks AS INT)ExternalAttendance_AssessmentMinPassMarks,MS.ExternalAttendance_AssessmentIsPartOf
                            ,MS.IsExternalMarksApplicable,CAST(MS.ExternalMaxMarks AS INT)ExternalMaxMarks,CAST(MS.ExternalMinPassMarks AS INT)ExternalMinPassMarks,MS.ExternalIsPartOf
                            ,CAST(R.InternalMarks AS DECIMAL(5,2))InternalMarks,CAST(R.InternalAttendance_AssessmentMarks AS DECIMAL(5,2))InternalAttendance_AssessmentMarks
                            ,CAST(R.ExternalAttendance_AssessmentMarks AS DECIMAL(5,2))ExternalAttendance_AssessmentMarks,CAST(R.ExternalMarks AS DECIMAL(5,2))ExternalMarks
                            ,R.IsInternalPassed,R.IsExternalPassed
                            ,R.ExternalSubmitted,R.InternalSubmitted
                            ,R.ResultNotification_ID,R.ExamForm_ID
                            ,RN.ParentNotification_ID,RN.NotificationNo,RN.Dated NotificationDate								                                
                            FROM dbo.ARGPersonalInformation_{printProgramme.ToString()} S
	                        JOIN ARGSelectedCombination_{printProgramme.ToString()} Comb ON Comb.Student_ID = S.Student_ID AND Comb.Semester <= S.CurrentSemesterOrYear
                            JOIN VWCombinationMaster CM ON {(semester.HasValue && semester.Value > 0 ? "Comb.Semester=" + semester + " AND" : "")} CM.Combination_ID = Comb.Combination_ID AND Comb.IsVerified=1
                            JOIN dbo.ADMSubjectMaster SM ON Comb.Semester=SM.Semester AND SM.Subject_ID = CM.Subject_ID
	                        JOIN dbo.MSSubjectMarksStructure MS ON MS.SubjectMarksStructure_ID = SM.SubjectMarksStructure_ID
                            JOIN ADMCourseMaster Course ON Course.Course_ID = CM.Course_ID
                            LEFT JOIN {printProgramme.ToString()}_Semester{(semester.HasValue && semester.Value > 0 ? semester.Value.ToString() : "All")} R ON R.Student_ID = S.Student_ID AND R.Subject_ID=CM.Subject_ID 
                            LEFT JOIN ResultNotification RN ON RN.ResultNotification_ID = R.ResultNotification_ID";
            return Query;
        }


        internal string ResultList_SubjectResult_Query(bool isList, PrintProgramme printProgramme, short? semester, bool Editable = false, bool transcriptInfo = false)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            string Query = $@"SELECT 
                                {(isList ? @"S.Gender,s.CUSRegistrationNo,ClassRollNo,FullName,ExamRollNumber,AcceptCollege_ID,CollegeFullName
                                ,S.Category,S.FathersName,S.Batch,Course.PrintProgramme
                                ,Comb.Course_ID,Course.CourseFullName," : "")}
                                R._ID,S.Student_ID,SemesterBatch,Comb.Semester,COMB.Subject_ID,Course.Programme,MS.TotalCredit,RA.ResultAnomalies_Id
                                ,SM.SubjectFullName,SM.SubjectType,SM.SubjectCode,SM.HasResult,SM.HasExaminationFee
                                ,MS.IsInternalMarksApplicable,CAST(MS.InternalMaxMarks AS DECIMAL(5,2))InternalMaxMarks,MS.InternalIsPartOf,CAST(MS.InternalMinPassMarks AS DECIMAL(5,2))InternalMinPassMarks
                                ,MS.IsInternalAttendance_AssessmentMarksApplicable,CAST(MS.InternalAttendance_AssessmentMaxMarks AS DECIMAL(5,2))InternalAttendance_AssessmentMaxMarks 
                                ,CAST(MS.InternalAttendance_AssessmentMinPassMarks AS DECIMAL(5,2))InternalAttendance_AssessmentMinPassMarks,MS.InternalAttendance_AssessmentIsPartOf
                                ,MS.IsExternalAttendance_AssessmentMarksApplicable,CAST(MS.ExternalAttendance_AssessmentMaxMarks AS DECIMAL(5,2))ExternalAttendance_AssessmentMaxMarks
                                ,CAST(MS.ExternalAttendance_AssessmentMinPassMarks AS DECIMAL(5,2))ExternalAttendance_AssessmentMinPassMarks,MS.ExternalAttendance_AssessmentIsPartOf
                                ,MS.IsExternalMarksApplicable,CAST(MS.ExternalMaxMarks AS DECIMAL(5,2))ExternalMaxMarks,CAST(MS.ExternalMinPassMarks AS DECIMAL(5,2))ExternalMinPassMarks,MS.ExternalIsPartOf
                                ,CAST(R.InternalMarks AS DECIMAL(5,2))InternalMarks,CAST(R.InternalAttendance_AssessmentMarks AS DECIMAL(5,2))InternalAttendance_AssessmentMarks
                                ,CAST(R.ExternalAttendance_AssessmentMarks AS DECIMAL(5,2))ExternalAttendance_AssessmentMarks,CAST(R.ExternalMarks AS DECIMAL(5,2))ExternalMarks
                                ,R.IsInternalPassed,R.IsExternalPassed
                                ,R.ExternalSubmitted,R.InternalSubmitted
                                ,R.ResultNotification_ID,R.ExamForm_ID
                                ,RN.ParentNotification_ID,RN.NotificationNo,RN.Dated NotificationDate
                                
                                FROM(
                                    SELECT Comb.Student_ID,CM.Semester,CM.Subject_ID,CM.Course_ID,SemesterBatch 
                                    FROM ARGSelectedCombination_{printProgramme.ToString()} Comb
                                    JOIN VWCombinationMaster CM ON CM.Semester=Comb.Semester AND CM.Combination_ID = Comb.Combination_ID
                                    Where  Comb.IsVerified=1 {(semester.HasValue && semester.Value > 0 ? "AND Comb.Semester=" + semester : "")}
                                    UNION 
                                    SELECT A.Student_ID,A.Semester,A.Subject_ID,A.Course_ID,SemesterBatch FROM ARGStudentAdditionalSubjects_{printProgramme.ToString()} A 
                                )COMB
                                JOIN dbo.ARGPersonalInformation_{printProgramme.ToString()} S ON S.Student_ID = Comb.Student_ID AND Comb.Semester <= S.CurrentSemesterOrYear 
                                JOIN dbo.ADMCollegeMaster ON ADMCollegeMaster.College_ID = S.AcceptCollege_ID
                                {(Editable ? "LEFT" : "")} JOIN (SELECT DISTINCT Student_ID,ExamRollNumber FROM ARGStudentExamForm_{printProgramme.ToString()} WHERE Status=4 AND ExamRollNumber IS NOT NULL) E ON E.Student_ID = Comb.Student_ID
                                JOIN dbo.ADMSubjectMaster SM ON SM.Semester=Comb.semester AND SM.Subject_ID = Comb.Subject_ID AND SM.HasResult=1
                                JOIN dbo.MSSubjectMarksStructure MS ON MS.SubjectMarksStructure_ID = SM.SubjectMarksStructure_ID
                                JOIN ADMCourseMaster Course ON Course.Course_ID = Comb.Course_ID
                                {(Editable ? "LEFT" : "")} JOIN {printProgramme.ToString()}_Semester{(semester.HasValue && semester.Value > 0 ? semester.Value.ToString() : "All")} R ON R.Student_ID = Comb.Student_ID AND R.Subject_ID=COMB.Subject_ID {(Editable ? "" : " AND R.ResultNotification_ID IS NOT NULL AND R.ExamForm_ID IS NOT NULL")}
                                 LEFT JOIN dbo.ResultAnomalies RA ON   RA.ExamForm_Id=R.ExamForm_ID AND RA.Result_Id = R._ID
                                LEFT JOIN ResultNotification RN ON RN.ResultNotification_ID = R.ResultNotification_ID";
            string Query1 = $@"SELECT 
                                {(isList ? @"S.Student_ID,S.Gender,s.CUSRegistrationNo,s.ClassRollNo,s.FullName,E.ExamRollNumber,S.AcceptCollege_ID,CollegeFullName
                                ,S.Category,S.FathersName,S.Batch
                                ,Comb.Course_ID,Course.CourseFullName,Comb.Semester,Comb.SemesterBatch," : "")}
                                R._ID,Comb.Semester,SS.Subject_ID,Course.Programme,Course.PrintProgramme,SS.TotalCredit,RA.ResultAnomalies_Id
                                ,SS.SubjectFullName,SS.SubjectType,SS.SubjectCode,SS.HasResult,SS.HasExaminationFee
                                ,SS.IsInternalMarksApplicable,SS.InternalMaxMarks,SS.InternalIsPartOf,SS.InternalMinPassMarks
                                ,SS.IsInternalAttendance_AssessmentMarksApplicable,SS.InternalAttendance_AssessmentMaxMarks,SS.InternalAttendance_AssessmentMinPassMarks,SS.InternalAttendance_AssessmentIsPartOf
                                ,SS.IsExternalAttendance_AssessmentMarksApplicable,SS.ExternalAttendance_AssessmentMaxMarks,SS.ExternalAttendance_AssessmentMinPassMarks,SS.ExternalAttendance_AssessmentIsPartOf
                                ,SS.IsExternalMarksApplicable,SS.ExternalMaxMarks,SS.ExternalMinPassMarks,SS.ExternalIsPartOf
                                ,R.InternalMarks,R.InternalAttendance_AssessmentMarks,R.ExternalAttendance_AssessmentMarks,R.ExternalMarks
                                ,R.IsInternalPassed,R.IsExternalPassed
                                ,R.ExternalSubmitted,R.InternalSubmitted
                                ,R.ResultNotification_ID,R.ExamForm_ID
                                ,RN.ParentNotification_ID,RN.NotificationNo,RN.Dated NotificationDate
                                {(transcriptInfo ? @",T.SubjectTitle T_SubjectTitle,T.SubjectType T_SubjectType,T.Credit T_Credit
                                , T.InternalMinMarks T_InternalMinMarks, T.InternalMaxMarks T_InternalMaxMarks, T.InternalMarksObt T_InternalMarksObt, T.InternalResultStatus T_InternalResultStatus
                                , T.ExternalMinMarks T_ExternalMinMarks, T.ExternalMaxMarks T_ExternalMaxMarks, T.ExternalMarksObt T_ExternalMarksObt, T.ExternalResultStatus T_ExternalResultStatus
                                , T.GradeLetter T_GradeLetter, T.GradePoints T_GradePoints, T.SGPAType T_SGPAType,(CASE WHEN MSCGPA.PrintedOn IS NULL THEN 'False' ELSE 'True' END)TranscriptPrinted" : "")}
                                FROM(
                                    SELECT Comb.Student_ID,CM.Semester,CM.Subject_ID,CM.Course_ID,SemesterBatch 
                                    FROM ARGSelectedCombination_{printProgramme.ToString()} Comb
                                    JOIN VWCombinationMaster CM ON CM.Semester=Comb.Semester AND CM.Combination_ID = Comb.Combination_ID
                                    Where  Comb.IsVerified=1 {(semester.HasValue && semester.Value > 0 ? "AND Comb.Semester=" + semester : "")}
                                    UNION 
                                    SELECT A.Student_ID,A.Semester,A.Subject_ID,A.Course_ID,SemesterBatch FROM ARGStudentAdditionalSubjects_{printProgramme.ToString()} A 
                                )Comb

                                {(isList ? $@"JOIN dbo.ARGPersonalInformation_{printProgramme.ToString()} S ON S.Student_ID = Comb.Student_ID AND Comb.Semester <= S.CurrentSemesterOrYear 
                                JOIN dbo.ADMCollegeMaster ON ADMCollegeMaster.College_ID = S.AcceptCollege_ID
                                {(Editable ? "LEFT" : "")} JOIN (SELECT DISTINCT Student_ID,ExamRollNumber FROM ARGStudentExamForm_{printProgramme.ToString()} WHERE Status=4 AND ExamRollNumber IS NOT NULL AND Semester=1) E ON E.Student_ID = Comb.Student_ID" : "")}

                                JOIN VW_SubjectWithStructure SS ON Comb.Semester=SS.Semester AND SS.HasResult=1 AND SS.Subject_ID = Comb.Subject_ID AND HasResult=1
                                JOIN ADMCourseMaster Course ON Course.Course_ID = Comb.Course_ID
                                LEFT JOIN {printProgramme.ToString()}_Semester{(semester.HasValue && semester.Value > 0 ? semester.Value.ToString() : "All")} R ON R.Student_ID = Comb.Student_ID AND R.Subject_ID=SS.Subject_ID {(Editable ? "" : " AND R.ResultNotification_ID IS NOT NULL AND r.ExamForm_ID IS NOT NULL")}
                                LEFT JOIN ResultNotification RN ON RN.ResultNotification_ID = R.ResultNotification_ID
                                LEFT JOIN dbo.ResultAnomalies RA ON   RA.ExamForm_Id=R.ExamForm_ID AND RA.Result_Id = R._ID
                                {(transcriptInfo ? "LEFT JOIN dbo.MSStudentMarks T ON T.Student_Id=Comb.Student_ID AND T.Semester=Comb.Semester AND T.Subject_ID=Comb.Subject_ID LEFT JOIN dbo.MSCGPA ON MSCGPA.Student_ID=T.Student_Id" : "")}";
            return Query;
        }

        internal string AllSubjectDetailsQuery(bool isList, PrintProgramme printProgramme, short? semester, bool Editable = false, bool transcriptInfo = false)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            string Query = $@"SELECT 
                                {(isList ? @"S.Gender,s.CUSRegistrationNo,ClassRollNo,FullName,ExamRollNumber,AcceptCollege_ID,CollegeFullName,DegreeCourseTitle
                                ,S.Category,S.FathersName,S.Batch,Course.PrintProgramme
                                ,Comb.Course_ID,Course.CourseFullName," : "")}
                                R._ID,S.Student_ID,SemesterBatch,Comb.Semester,COMB.Subject_ID,Course.Programme,MS.TotalCredit,RA.ResultAnomalies_Id
                                ,SM.SubjectFullName,SM.SubjectType,SM.SubjectCode,SM.HasResult,SM.HasExaminationFee
                                ,MS.IsInternalMarksApplicable,CAST(MS.InternalMaxMarks AS DECIMAL(5,2))InternalMaxMarks,MS.InternalIsPartOf,CAST(MS.InternalMinPassMarks AS DECIMAL(5,2))InternalMinPassMarks
                                ,MS.IsInternalAttendance_AssessmentMarksApplicable,CAST(MS.InternalAttendance_AssessmentMaxMarks AS DECIMAL(5,2))InternalAttendance_AssessmentMaxMarks 
                                ,CAST(MS.InternalAttendance_AssessmentMinPassMarks AS DECIMAL(5,2))InternalAttendance_AssessmentMinPassMarks,MS.InternalAttendance_AssessmentIsPartOf
                                ,MS.IsExternalAttendance_AssessmentMarksApplicable,CAST(MS.ExternalAttendance_AssessmentMaxMarks AS DECIMAL(5,2))ExternalAttendance_AssessmentMaxMarks
                                ,CAST(MS.ExternalAttendance_AssessmentMinPassMarks AS DECIMAL(5,2))ExternalAttendance_AssessmentMinPassMarks,MS.ExternalAttendance_AssessmentIsPartOf
                                ,MS.IsExternalMarksApplicable,CAST(MS.ExternalMaxMarks AS DECIMAL(5,2))ExternalMaxMarks,CAST(MS.ExternalMinPassMarks AS DECIMAL(5,2))ExternalMinPassMarks,MS.ExternalIsPartOf
                                ,CAST(R.InternalMarks AS DECIMAL(5,2))InternalMarks,CAST(R.InternalAttendance_AssessmentMarks AS DECIMAL(5,2))InternalAttendance_AssessmentMarks
                                ,CAST(R.ExternalAttendance_AssessmentMarks AS DECIMAL(5,2))ExternalAttendance_AssessmentMarks,CAST(R.ExternalMarks AS DECIMAL(5,2))ExternalMarks
                                ,R.IsInternalPassed,R.IsExternalPassed
                                ,R.ExternalSubmitted,R.InternalSubmitted
                                ,R.ResultNotification_ID,R.ExamForm_ID
                                ,RN.ParentNotification_ID,RN.NotificationNo,RN.Dated NotificationDate,MS.CreditWeightage,SM.IsLocked
                                
                                FROM(
                                    SELECT Comb.Student_ID,CM.Semester,CM.Subject_ID,CM.Course_ID,SemesterBatch 
                                    FROM ARGSelectedCombination_{printProgramme.ToString()} Comb
                                    JOIN VWCombinationMaster CM ON CM.Semester=Comb.Semester AND CM.Combination_ID = Comb.Combination_ID
                                    Where  Comb.IsVerified=1 /*{(semester.HasValue && semester.Value > 0 ? "AND Comb.Semester<=" + semester : "")}*/
                                    UNION 
                                    SELECT A.Student_ID,A.Semester,A.Subject_ID,A.Course_ID,SemesterBatch FROM ARGStudentAdditionalSubjects_{printProgramme.ToString()} A 
                                )COMB
                                JOIN dbo.ARGPersonalInformation_{printProgramme.ToString()} S ON S.Student_ID = Comb.Student_ID AND Comb.Semester <= S.CurrentSemesterOrYear
                                JOIN dbo.ADMCollegeMaster ON ADMCollegeMaster.College_ID = S.AcceptCollege_ID
                                {(Editable ? "LEFT" : "")} JOIN (SELECT DISTINCT Student_ID,ExamRollNumber FROM ARGStudentExamForm_{printProgramme.ToString()} WHERE Status=4 AND ExamRollNumber IS NOT NULL) E ON E.Student_ID = Comb.Student_ID
                                JOIN dbo.ADMSubjectMaster SM ON SM.Semester=Comb.semester AND SM.Subject_ID = Comb.Subject_ID AND SM.HasResult=1
                                JOIN dbo.MSSubjectMarksStructure MS ON MS.SubjectMarksStructure_ID = SM.SubjectMarksStructure_ID
                                JOIN ADMCourseMaster Course ON Course.Course_ID = Comb.Course_ID
                                {(Editable ? "LEFT" : "")} JOIN IH_SemesterAll R ON R.Student_ID = Comb.Student_ID AND R.Subject_ID=COMB.Subject_ID {(Editable ? "" : " AND R.ResultNotification_ID IS NOT NULL AND R.ExamForm_ID IS NOT NULL")}
                                 LEFT JOIN dbo.ResultAnomalies RA ON   RA.ExamForm_Id=R.ExamForm_ID AND RA.Result_Id = R._ID                                
                                LEFT JOIN ResultNotification RN ON RN.ResultNotification_ID = R.ResultNotification_ID";
            string Query1 = $@"SELECT 
                                {(isList ? @"S.Student_ID,S.Gender,s.CUSRegistrationNo,s.ClassRollNo,s.FullName,E.ExamRollNumber,S.AcceptCollege_ID,CollegeFullName,DegreeCourseTitle
                                ,S.Category,S.FathersName,S.Batch
                                ,Comb.Course_ID,Course.CourseFullName,Comb.Semester,Comb.SemesterBatch," : "")}
                                R._ID,Comb.Semester,SS.Subject_ID,Course.Programme,Course.PrintProgramme,SS.TotalCredit,RA.ResultAnomalies_Id
                                ,SS.SubjectFullName,SS.SubjectType,SS.SubjectCode,SS.HasResult,SS.HasExaminationFee
                                ,SS.IsInternalMarksApplicable,SS.InternalMaxMarks,SS.InternalIsPartOf,SS.InternalMinPassMarks
                                ,SS.IsInternalAttendance_AssessmentMarksApplicable,SS.InternalAttendance_AssessmentMaxMarks,SS.InternalAttendance_AssessmentMinPassMarks,SS.InternalAttendance_AssessmentIsPartOf
                                ,SS.IsExternalAttendance_AssessmentMarksApplicable,SS.ExternalAttendance_AssessmentMaxMarks,SS.ExternalAttendance_AssessmentMinPassMarks,SS.ExternalAttendance_AssessmentIsPartOf
                                ,SS.IsExternalMarksApplicable,SS.ExternalMaxMarks,SS.ExternalMinPassMarks,SS.ExternalIsPartOf
                                ,R.InternalMarks,R.InternalAttendance_AssessmentMarks,R.ExternalAttendance_AssessmentMarks,R.ExternalMarks
                                ,R.IsInternalPassed,R.IsExternalPassed
                                ,R.ExternalSubmitted,R.InternalSubmitted
                                ,R.ResultNotification_ID,R.ExamForm_ID
                                ,RN.ParentNotification_ID,RN.NotificationNo,RN.Dated NotificationDate,MS.CreditWeightage,SM.IsLocked
                                {(transcriptInfo ? @",T.SubjectTitle T_SubjectTitle,T.SubjectType T_SubjectType,T.Credit T_Credit
                                , T.InternalMinMarks T_InternalMinMarks, T.InternalMaxMarks T_InternalMaxMarks, T.InternalMarksObt T_InternalMarksObt, T.InternalResultStatus T_InternalResultStatus
                                , T.ExternalMinMarks T_ExternalMinMarks, T.ExternalMaxMarks T_ExternalMaxMarks, T.ExternalMarksObt T_ExternalMarksObt, T.ExternalResultStatus T_ExternalResultStatus
                                , T.GradeLetter T_GradeLetter, T.GradePoints T_GradePoints, T.SGPAType T_SGPAType,(CASE WHEN MSCGPA.PrintedOn IS NULL THEN 'False' ELSE 'True' END)TranscriptPrinted" : "")}
                                FROM(
                                    SELECT Comb.Student_ID,CM.Semester,CM.Subject_ID,CM.Course_ID,SemesterBatch 
                                    FROM ARGSelectedCombination_{printProgramme.ToString()} Comb
                                    JOIN VWCombinationMaster CM ON CM.Semester=Comb.Semester AND CM.Combination_ID = Comb.Combination_ID
                                    Where  Comb.IsVerified=1 /*{(semester.HasValue && semester.Value > 0 ? "AND Comb.Semester=" + semester : "")}*/
                                    UNION 
                                    SELECT A.Student_ID,A.Semester,A.Subject_ID,A.Course_ID,SemesterBatch FROM ARGStudentAdditionalSubjects_{printProgramme.ToString()} A 
                                )Comb

                                {(isList ? $@"JOIN dbo.ARGPersonalInformation_{printProgramme.ToString()} S ON S.Student_ID = Comb.Student_ID AND Comb.Semester <= S.CurrentSemesterOrYear
                                JOIN dbo.ADMCollegeMaster ON ADMCollegeMaster.College_ID = S.AcceptCollege_ID
                                {(Editable ? "LEFT" : "")} JOIN (SELECT DISTINCT Student_ID,ExamRollNumber FROM ARGStudentExamForm_{printProgramme.ToString()} WHERE Status=4 AND ExamRollNumber IS NOT NULL AND Semester=1) E ON E.Student_ID = Comb.Student_ID" : "")}

                                JOIN VW_SubjectWithStructure SS ON Comb.Semester=SS.Semester AND SS.HasResult=1 AND SS.Subject_ID = Comb.Subject_ID AND HasResult=1
                                JOIN ADMCourseMaster Course ON Course.Course_ID = Comb.Course_ID
                                LEFT JOIN dbo.IH_SemesterAll R ON R.Student_ID = Comb.Student_ID AND R.Subject_ID=SS.Subject_ID {(Editable ? "" : " AND R.ResultNotification_ID IS NOT NULL AND r.ExamForm_ID IS NOT NULL")}
                                LEFT JOIN ResultNotification RN ON RN.ResultNotification_ID = R.ResultNotification_ID
                                LEFT JOIN dbo.ResultAnomalies RA ON   RA.ExamForm_Id=R.ExamForm_ID AND RA.Result_Id = R._ID                                 
                                {(transcriptInfo ? "LEFT JOIN dbo.MSStudentMarks T ON T.Student_Id=Comb.Student_ID AND T.Semester=Comb.Semester AND T.Subject_ID=Comb.Subject_ID LEFT JOIN dbo.MSCGPA ON MSCGPA.Student_ID=T.Student_Id" : "")}";

            return Query;
        }

        internal string ResultList_SubjectResult_QueryDetails(bool isList, PrintProgramme printProgramme, short? semester, bool Editable = false, bool transcriptInfo = false)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            string Query = $@"SELECT 
                                {(isList ? @"S.Gender,s.CUSRegistrationNo,ClassRollNo,FullName,ExamRollNumber,AcceptCollege_ID,CollegeFullName,DegreeCourseTitle
                                ,S.Category,S.FathersName,S.Batch,Course.PrintProgramme
                                ,Comb.Course_ID,Course.CourseFullName," : "")}
                                R._ID,S.Student_ID,SemesterBatch,Comb.Semester,COMB.Subject_ID,Course.Programme,MS.TotalCredit,RA.ResultAnomalies_Id
                                ,SM.SubjectFullName,SM.SubjectType,SM.SubjectCode,SM.HasResult,SM.HasExaminationFee
                                ,MS.IsInternalMarksApplicable,CAST(MS.InternalMaxMarks AS DECIMAL(5,2))InternalMaxMarks,MS.InternalIsPartOf,CAST(MS.InternalMinPassMarks AS DECIMAL(5,2))InternalMinPassMarks
                                ,MS.IsInternalAttendance_AssessmentMarksApplicable,CAST(MS.InternalAttendance_AssessmentMaxMarks AS DECIMAL(5,2))InternalAttendance_AssessmentMaxMarks 
                                ,CAST(MS.InternalAttendance_AssessmentMinPassMarks AS DECIMAL(5,2))InternalAttendance_AssessmentMinPassMarks,MS.InternalAttendance_AssessmentIsPartOf
                                ,MS.IsExternalAttendance_AssessmentMarksApplicable,CAST(MS.ExternalAttendance_AssessmentMaxMarks AS DECIMAL(5,2))ExternalAttendance_AssessmentMaxMarks
                                ,CAST(MS.ExternalAttendance_AssessmentMinPassMarks AS DECIMAL(5,2))ExternalAttendance_AssessmentMinPassMarks,MS.ExternalAttendance_AssessmentIsPartOf
                                ,MS.IsExternalMarksApplicable,CAST(MS.ExternalMaxMarks AS DECIMAL(5,2))ExternalMaxMarks,CAST(MS.ExternalMinPassMarks AS DECIMAL(5,2))ExternalMinPassMarks,MS.ExternalIsPartOf
                                ,CAST(R.InternalMarks AS DECIMAL(5,2))InternalMarks,CAST(R.InternalAttendance_AssessmentMarks AS DECIMAL(5,2))InternalAttendance_AssessmentMarks
                                ,CAST(R.ExternalAttendance_AssessmentMarks AS DECIMAL(5,2))ExternalAttendance_AssessmentMarks,CAST(R.ExternalMarks AS DECIMAL(5,2))ExternalMarks
                                ,R.IsInternalPassed,R.IsExternalPassed
                                ,R.ExternalSubmitted,R.InternalSubmitted
                                ,R.ResultNotification_ID,R.ExamForm_ID
                                --,RN.ParentNotification_ID,RN.NotificationNo,RN.Dated NotificationDate,MS.CreditWeightage,SM.IsLocked
                                
                                FROM(
                                    SELECT Comb.Student_ID,CM.Semester,CM.Subject_ID,CM.Course_ID,SemesterBatch 
                                    FROM ARGSelectedCombination_{printProgramme.ToString()} Comb
                                    JOIN VWCombinationMaster CM ON CM.Semester=Comb.Semester AND CM.Combination_ID = Comb.Combination_ID
                                    Where  Comb.IsVerified=1 /*{(semester.HasValue && semester.Value > 0 ? "AND Comb.Semester<=" + semester : "")}*/
                                    UNION 
                                    SELECT A.Student_ID,A.Semester,A.Subject_ID,A.Course_ID,SemesterBatch FROM ARGStudentAdditionalSubjects_{printProgramme.ToString()} A 
                                )COMB
                                JOIN dbo.ARGPersonalInformation_{printProgramme.ToString()} S ON S.Student_ID = Comb.Student_ID AND Comb.Semester <= S.CurrentSemesterOrYear
                                JOIN dbo.ADMCollegeMaster ON ADMCollegeMaster.College_ID = S.AcceptCollege_ID
                                {(Editable ? "LEFT" : "")} JOIN (SELECT DISTINCT Student_ID,ExamRollNumber FROM ARGStudentExamForm_{printProgramme.ToString()} WHERE Status=4 AND ExamRollNumber IS NOT NULL) E ON E.Student_ID = Comb.Student_ID
                                JOIN dbo.ADMSubjectMaster SM ON SM.Semester=Comb.semester AND SM.Subject_ID = Comb.Subject_ID AND SM.HasResult=1
                                JOIN dbo.MSSubjectMarksStructure MS ON MS.SubjectMarksStructure_ID = SM.SubjectMarksStructure_ID
                                JOIN ADMCourseMaster Course ON Course.Course_ID = Comb.Course_ID
                                {(Editable ? "LEFT" : "")} JOIN IH_SemesterAll R ON R.Student_ID = Comb.Student_ID AND R.Subject_ID=COMB.Subject_ID {(Editable ? "" : " AND R.ResultNotification_ID IS NOT NULL AND R.ExamForm_ID IS NOT NULL")}
                                 LEFT JOIN dbo.ResultAnomalies RA ON   RA.ExamForm_Id=R.ExamForm_ID AND RA.Result_Id = R._ID
                                --LEFT JOIN dbo.MSFullDetails FD ON FD.Student_Id = R.Student_ID AND FD.Student_Id = E.Student_ID
                                LEFT JOIN ResultNotification RN ON RN.ResultNotification_ID = R.ResultNotification_ID";
            string Query1 = $@"SELECT 
                                {(isList ? @"S.Student_ID,S.Gender,s.CUSRegistrationNo,s.ClassRollNo,s.FullName,E.ExamRollNumber,S.AcceptCollege_ID,CollegeFullName,DegreeCourseTitle
                                ,S.Category,S.FathersName,S.Batch
                                ,Comb.Course_ID,Course.CourseFullName,Comb.Semester,Comb.SemesterBatch," : "")}
                                R._ID,FD.Marks_ID,Comb.Semester,SS.Subject_ID,Course.Programme,Course.PrintProgramme,SS.TotalCredit,RA.ResultAnomalies_Id
                                ,SS.SubjectFullName,SS.SubjectType,SS.SubjectCode,SS.HasResult,SS.HasExaminationFee
                                ,SS.IsInternalMarksApplicable,SS.InternalMaxMarks,SS.InternalIsPartOf,SS.InternalMinPassMarks
                                ,SS.IsInternalAttendance_AssessmentMarksApplicable,SS.InternalAttendance_AssessmentMaxMarks,SS.InternalAttendance_AssessmentMinPassMarks,SS.InternalAttendance_AssessmentIsPartOf
                                ,SS.IsExternalAttendance_AssessmentMarksApplicable,SS.ExternalAttendance_AssessmentMaxMarks,SS.ExternalAttendance_AssessmentMinPassMarks,SS.ExternalAttendance_AssessmentIsPartOf
                                ,SS.IsExternalMarksApplicable,SS.ExternalMaxMarks,SS.ExternalMinPassMarks,SS.ExternalIsPartOf
                                ,R.InternalMarks,R.InternalAttendance_AssessmentMarks,R.ExternalAttendance_AssessmentMarks,R.ExternalMarks
                                ,R.IsInternalPassed,R.IsExternalPassed
                                ,R.ExternalSubmitted,R.InternalSubmitted
                                ,R.ResultNotification_ID,R.ExamForm_ID
                                ,RN.ParentNotification_ID,RN.NotificationNo,RN.Dated NotificationDate,MS.CreditWeightage,SM.IsLocked
                                {(transcriptInfo ? @",T.SubjectTitle T_SubjectTitle,T.SubjectType T_SubjectType,T.Credit T_Credit
                                , T.InternalMinMarks T_InternalMinMarks, T.InternalMaxMarks T_InternalMaxMarks, T.InternalMarksObt T_InternalMarksObt, T.InternalResultStatus T_InternalResultStatus
                                , T.ExternalMinMarks T_ExternalMinMarks, T.ExternalMaxMarks T_ExternalMaxMarks, T.ExternalMarksObt T_ExternalMarksObt, T.ExternalResultStatus T_ExternalResultStatus
                                , T.GradeLetter T_GradeLetter, T.GradePoints T_GradePoints, T.SGPAType T_SGPAType,(CASE WHEN MSCGPA.PrintedOn IS NULL THEN 'False' ELSE 'True' END)TranscriptPrinted" : "")}
                                FROM(
                                    SELECT Comb.Student_ID,CM.Semester,CM.Subject_ID,CM.Course_ID,SemesterBatch 
                                    FROM ARGSelectedCombination_{printProgramme.ToString()} Comb
                                    JOIN VWCombinationMaster CM ON CM.Semester=Comb.Semester AND CM.Combination_ID = Comb.Combination_ID
                                    Where  Comb.IsVerified=1 /*{(semester.HasValue && semester.Value > 0 ? "AND Comb.Semester=" + semester : "")}*/
                                    UNION 
                                    SELECT A.Student_ID,A.Semester,A.Subject_ID,A.Course_ID,SemesterBatch FROM ARGStudentAdditionalSubjects_{printProgramme.ToString()} A 
                                )Comb

                                {(isList ? $@"JOIN dbo.ARGPersonalInformation_{printProgramme.ToString()} S ON S.Student_ID = Comb.Student_ID AND Comb.Semester <= S.CurrentSemesterOrYear
                                JOIN dbo.ADMCollegeMaster ON ADMCollegeMaster.College_ID = S.AcceptCollege_ID
                                {(Editable ? "LEFT" : "")} JOIN (SELECT DISTINCT Student_ID,ExamRollNumber FROM ARGStudentExamForm_{printProgramme.ToString()} WHERE Status=4 AND ExamRollNumber IS NOT NULL AND Semester=1) E ON E.Student_ID = Comb.Student_ID" : "")}

                                JOIN VW_SubjectWithStructure SS ON Comb.Semester=SS.Semester AND SS.HasResult=1 AND SS.Subject_ID = Comb.Subject_ID AND HasResult=1
                                JOIN ADMCourseMaster Course ON Course.Course_ID = Comb.Course_ID
                                LEFT JOIN dbo.IH_SemesterAll R ON R.Student_ID = Comb.Student_ID AND R.Subject_ID=SS.Subject_ID {(Editable ? "" : " AND R.ResultNotification_ID IS NOT NULL AND r.ExamForm_ID IS NOT NULL")}
                                LEFT JOIN ResultNotification RN ON RN.ResultNotification_ID = R.ResultNotification_ID
                                LEFT JOIN dbo.ResultAnomalies RA ON   RA.ExamForm_Id=R.ExamForm_ID AND RA.Result_Id = R._ID  
                               -- LEFT JOIN dbo.MSFullDetails FD ON FD.Student_Id = R.Student_ID AND FD.Student_Id = E.Student_ID
                                {(transcriptInfo ? "LEFT JOIN dbo.MSStudentMarks T ON T.Student_Id=Comb.Student_ID AND T.Semester=Comb.Semester AND T.Subject_ID=Comb.Subject_ID LEFT JOIN dbo.MSCGPA ON MSCGPA.Student_ID=T.Student_Id" : "")}";

            return Query;
        }

        internal string RevaluationList_Query(PrintProgramme printProgramme, Parameters Parameter, short? semester, bool transcriptInfo = false)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);

            string Query = $@" SELECT distinct STDINFO.FullName,STDINFO.CUSRegistrationNo,EXM.ExamRollNumber,STDINFO.Batch,crm.CourseFullName,Comb.SemesterBatch,
								 col.CollegeFullName,STDINFO.ClassRollNo ,R.InternalMarks,R.InternalAttendance_AssessmentMarks,R.ExternalAttendance_AssessmentMarks,R.ExternalMarks
							    ,STDINFO.AcceptCollege_ID,crm.Programme,crm.Course_ID,EXM.Semester,R.Subject_ID,SM.SubjectFullName,R._ID,R.ResultNotification_ID,R.ExamForm_ID,Comb.Student_ID,R.InternalSubmitted,R.ExternalSubmitted
							  FROM {printProgramme.ToString()}_Semester{(semester.HasValue && semester.Value > 0 ? semester.Value.ToString() : "All")} R INNER JOIN dbo.ReEvaluation REV ON REV.Student_ID = R.Student_ID
								 INNER JOIN dbo.ADMSubjectMaster SM ON SM.Subject_ID = R.Subject_ID								 
								 INNER JOIN dbo.ReEvaluationStudentSubjects RSS ON RSS.ReEvaluation_ID = REV.ReEvaluation_ID AND RSS.Subject_ID = R.Subject_ID
								 INNER JOIN dbo.ARGPersonalInformation_{printProgramme.ToString()} STDINFO ON STDINFO.Student_ID = STDINFO.Student_ID AND STDINFO.Student_ID = RSS.Student_ID
								 INNER JOIN dbo.ADMCourseMaster crm ON crm.Course_ID = SM.Course_ID 
								 INNER JOIN dbo.ARGStudentExamForm_{printProgramme.ToString()} EXM ON  EXM.Student_ID = REV.Student_ID
                                 INNER JOIN dbo.ARGSelectedCombination_{printProgramme.ToString()} Comb ON Comb.Student_ID = R.Student_ID AND Comb.Student_ID = RSS.Student_ID
								 INNER JOIN dbo.ADMCollegeMaster col ON col.College_ID=STDINFO.AcceptCollege_ID
                                 INNER JOIN dbo.PaymentDetails_{printProgramme.ToString()} P ON P.Entity_ID=REV.ReEvaluation_ID AND P.ModuleType=2";

            return Query;
        }




        //internal string SubjectWiseResultListQuery(PrintProgramme printProgramme, short? semester, string ListOfColumns, bool Editable = false)
        //{
        //    printProgramme = new GeneralFunctions().MappingTable(printProgramme);
        //    string Query = $@"SELECT S.Student_ID,S.Gender,s.CUSRegistrationNo,s.ClassRollNo,s.FullName,E.ExamRollNumber,S.AcceptCollege_ID,CollegeFullName
        //                    ,Comb.Course_ID,Course.CourseFullName,Comb.Semester,Comb.SemesterBatch
        //                    ,R._ID,Comb.Semester,Comb.Subject_ID,Course.Programme
        //                    ,SS.SubjectFullName,SS.SubjectType,SS.SubjectCode,SS.HasResult,SS.HasExaminationFee
        //                    ,SS.IsInternalMarksApplicable,SS.InternalMaxMarks,SS.InternalIsPartOf,SS.InternalMinPassMarks
        //                    ,SS.IsInternalAttendance_AssessmentApplicable,SS.InternalAttendance_AssessmentMaxMarks,SS.InternalAttendance_AssessmentMinPassMarks,SS.InternalAttendanceIsPartOf
        //                    ,SS.IsExternalAttendance_AssessmentApplicable,SS.ExternalAttendance_AssessmentMaxMarks,SS.ExternalAttendance_AssessmentMinPassMarks,SS.ExternalAttendanceIsPartOf
        //                    ,SS.IsExternalMarksApplicable,SS.ExternalMaxMarks,SS.ExternalMinPassMarks,SS.ExternalIsPartOf
        //                    ,R.InternalMarks,R.InternalAttendance_AssessmentMarks,R.ExternalAttendance_AssessmentMarks,R.ExternalMarks
        //                    ,R.ExternalSubmitted,R.InternalSubmitted
        //                    ,R.ResultNotification_ID,R.ExamForm_ID
        //                    ,RN.ParentNotification_ID,RN.NotificationNo,RN.Dated NotificationDate
        //                    FROM ARGPersonalInformation_{printProgramme.ToString()} S
        //                    JOIN ADMCollegeMaster ON ADMCollegeMaster.College_ID = S.AcceptCollege_ID
        //                    JOIN (
        //                        SELECT Comb.Student_ID,CM.Semester,CM.Subject_ID,CM.Course_ID,Comb.SemesterBatch 
        //                        FROM ARGSelectedCombination_{printProgramme.ToString()} Comb
        //                        JOIN VWCombinationMaster CM ON CM.Semester=Comb.Semester AND CM.Combination_ID = Comb.Combination_ID
        //                        {(semester.HasValue && semester.Value > 0 ? "Where Comb.Semester=" + semester : "")}
        //                        UNION 
        //                        SELECT A.Student_ID,A.Semester,A.Subject_ID,A.Course_ID,null SemesterBatch 
        //                        FROM ARGStudentAdditionalSubjects_{printProgramme.ToString()} A 
        //                    )Comb ON Comb.Student_ID = S.Student_ID
        //                    JOIN ADMCourseMaster Course ON Course.Course_ID = Comb.Course_ID
        //                    {(Editable ? "LEFT" : "")} JOIN (SELECT DISTINCT Student_ID,ExamRollNumber FROM ARGStudentExamForm_{printProgramme.ToString()} WHERE Status={(short)FormStatus.Accepted} AND ExamRollNumber IS NOT NULL {(semester.HasValue && semester.Value > 0 ? "AND Semester=" + semester : "")}) E ON E.Student_ID = Comb.Student_ID
        //                    JOIN ADMSubjectMaster SS ON Comb.Semester=SS.Semester AND SS.HasResult=1 AND SS.Subject_ID = Comb.Subject_ID 
        //                    LEFT JOIN {printProgramme.ToString()}_Semester{(semester.HasValue && semester.Value > 0 ? semester.Value.ToString() : "All")} R ON R.Student_ID = Comb.Student_ID AND R.Subject_ID=SS.Subject_ID {(Editable ? "" : " AND R.ResultNotification_ID IS NOT NULL AND r.ExamForm_ID IS NOT NULL")}
        //                    LEFT JOIN ResultNotification RN ON RN.ResultNotification_ID = R.ResultNotification_ID";
        //    return Query;
        //}

        internal string ResultHistoryQuery(bool isList, PrintProgramme printProgramme, short? semester, bool Editable = false, bool transcriptInfo = false)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            string Query = $@"SELECT distinct
                                {(isList ? @"S.Student_ID,S.Gender,s.CUSRegistrationNo,s.ClassRollNo,s.FullName,E.ExamRollNumber,S.AcceptCollege_ID,CollegeFullName
                                ,Comb.Course_ID,Course.CourseFullName,Comb.Semester,Comb.SemesterBatch," : "")}
                                R._ID,Comb.Semester,SS.Subject_ID,Course.Programme,SS.TotalCredit
                                ,SS.SubjectFullName,SS.SubjectType,SS.SubjectCode,SS.HasResult,SS.HasExaminationFee
                                ,SS.IsInternalMarksApplicable,SS.InternalMaxMarks,SS.InternalIsPartOf,SS.InternalMinPassMarks
                                ,SS.IsInternalAttendance_AssessmentMarksApplicable,SS.InternalAttendance_AssessmentMaxMarks,SS.InternalAttendance_AssessmentMinPassMarks,SS.InternalAttendance_AssessmentIsPartOf
                                ,SS.IsExternalAttendance_AssessmentMarksApplicable,SS.ExternalAttendance_AssessmentMaxMarks,SS.ExternalAttendance_AssessmentMinPassMarks,SS.ExternalAttendance_AssessmentIsPartOf
                                ,SS.IsExternalMarksApplicable,SS.ExternalMaxMarks,SS.ExternalMinPassMarks,SS.ExternalIsPartOf
                                ,R.InternalMarks,R.InternalAttendance_AssessmentMarks,R.ExternalAttendance_AssessmentMarks,R.ExternalMarks
                                ,R.ExternalSubmitted,R.InternalSubmitted,R.InternalUpdatedOn,R.ExternalUpdatedOn,
                                 (SELECT UserName FROM dbo.AppUsers WHERE User_ID=R.InternalUpdatedBy)  InternalUserName,
							    (SELECT UserName FROM dbo.AppUsers WHERE User_ID=R.ExternalUpdatedBy)  ExternalUserName
                                ,R.ResultNotification_ID,R.ExamForm_ID
                                ,RN.ParentNotification_ID,RN.NotificationNo,RN.Dated NotificationDate
                                {(transcriptInfo ? @",T.SubjectTitle T_SubjectTitle,T.SubjectType T_SubjectType,T.Credit T_Credit
                                , T.InternalMinMarks T_InternalMinMarks, T.InternalMaxMarks T_InternalMaxMarks, T.InternalMarksObt T_InternalMarksObt, T.InternalResultStatus T_InternalResultStatus
                                , T.ExternalMinMarks T_ExternalMinMarks, T.ExternalMaxMarks T_ExternalMaxMarks, T.ExternalMarksObt T_ExternalMarksObt, T.ExternalResultStatus T_ExternalResultStatus
                                , T.GradeLetter T_GradeLetter, T.GradePoints T_GradePoints, T.SGPAType T_SGPAType" : "")}
                                FROM(
                                    SELECT Comb.Student_ID,CM.Semester,CM.Subject_ID,CM.Course_ID,SemesterBatch FROM ARGSelectedCombination_{printProgramme.ToString()} Comb
                                    JOIN VWCombinationMaster CM ON CM.Semester=Comb.Semester AND CM.Combination_ID = Comb.Combination_ID
                                    Where  Comb.IsVerified=1 {(semester.HasValue && semester.Value > 0 ? "AND Comb.Semester=" + semester : "")}
                                    UNION 
                                    SELECT A.Student_ID,A.Semester,A.Subject_ID,A.Course_ID,SemesterBatch FROM ARGStudentAdditionalSubjects_{printProgramme.ToString()} A 
                                )Comb

                                {(isList ? $@"JOIN dbo.ARGPersonalInformation_{printProgramme.ToString()} S ON S.Student_ID = Comb.Student_ID AND Comb.Semester <= S.CurrentSemesterOrYear
                                JOIN dbo.ADMCollegeMaster ON ADMCollegeMaster.College_ID = S.AcceptCollege_ID
                                {(Editable ? "LEFT" : "")} JOIN (SELECT DISTINCT Student_ID,ExamRollNumber FROM ARGStudentExamForm_{printProgramme.ToString()} WHERE Status=4 AND ExamRollNumber IS NOT NULL AND Semester=1) E ON E.Student_ID = Comb.Student_ID" : "")}

                                JOIN VW_SubjectWithStructure SS ON Comb.Semester=SS.Semester AND SS.HasResult=1 AND SS.Subject_ID = Comb.Subject_ID AND HasResult=1
                                JOIN ADMCourseMaster Course ON Course.Course_ID = Comb.Course_ID
                                LEFT JOIN {printProgramme.ToString()}_Semester{(semester.HasValue && semester.Value > 0 ? semester.Value.ToString() : "All")}_History R ON R.Student_ID = Comb.Student_ID AND R.Subject_ID=SS.Subject_ID {(Editable ? "" : " AND R.ResultNotification_ID IS NOT NULL AND r.ExamForm_ID IS NOT NULL")}
                                LEFT JOIN AppUsers Users ON Users.User_ID = R.InternalUpdatedBy OR Users.User_ID=R.ExternalUpdatedBy
                                LEFT JOIN ResultNotification RN ON RN.ResultNotification_ID = R.ResultNotification_ID
                                {(transcriptInfo ? "LEFT JOIN dbo.MSStudentMarks T ON T.Student_Id=Comb.Student_ID AND T.Semester=Comb.Semester AND T.Subject_ID=Comb.Subject_ID" : "")}";
            return Query;
        }


        #region Result MeritList  
        internal string ResulMeritListQuery(PrintProgramme printProgramme, Parameters Parameter, bool Editable = false)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);

            string Query = $@"	SELECT DISTINCT  CGPA,MSCGPA.Student_ID,STDINFO.FullName,Comb.SemesterBatch,STDDETAIL.PrintProgramme,STDINFO.Batch,CM.CourseFullName,MSCGPA.TCourse_ID,STDDETAIL.AcceptCollege_ID,STDINFO.FullName,STDDETAIL.ExamRollNumber,STDINFO.CUSRegistrationNo 
								FROM dbo.MSCGPA
								 JOIN dbo.MSStudentMarks ON  MSStudentMarks.Student_Id = MSCGPA.Student_ID 
								JOIN ARGPersonalInformation_{printProgramme.ToString()} STDINFO ON STDINFO.Student_ID = MSCGPA.Student_ID AND STDINFO.Student_ID = MSStudentMarks.Student_Id
								JOIN VWStudentWithDetail STDDETAIL ON STDDETAIL.Student_ID = STDINFO.Student_ID AND STDDETAIL.Student_ID = MSStudentMarks.Student_Id
                                JOIN dbo.ARGSelectedCombination_{printProgramme.ToString()} Comb ON Comb.Combination_ID = STDDETAIL.Combination_ID AND Comb.Student_ID = MSCGPA.Student_ID
                                JOIN dbo.ADMCourseMaster CM ON CM.Course_ID =  MSCGPA.TCourse_ID";

            return Query;
        }

        #endregion


        #region Result External PendingList  Regular,Backlog Both    

        internal string ResulPendingListQuery(Parameters Parameter, PrintProgramme printProgramme, short semester, int batch, Programme programe, Guid CourseId)
        {

            SqlCommand command = new SqlCommand();
            var Query = $@"SELECT * FROM (select  S.CollegeFullName,S.CourseFullName,SUB.SubjectFullName, ISNULL(COUNT(E.Student_ID),0) NoOfStudentsPending,Comb.Semester,'Regular' [Types]  from VWStudentWithDetail S
                            join dbo.ARGStudentExamForm_{printProgramme.ToString()} E on E.Student_ID = S.Student_ID and E.Semester={semester} and E.IsRegular=1 and E.Status=4
                           join ADMSubjectMaster SUB on SUB.Subject_ID = S.Subject_ID
                            join dbo.{printProgramme}_Semester{semester} R on R.Student_ID = S.Student_ID and R.Subject_ID = S.Subject_ID
                            JOIN dbo.ARGSelectedCombination_{printProgramme.ToString()} Comb ON Comb.Combination_ID = S.Combination_ID  AND Comb.Student_ID = R.Student_ID
                            JOIN dbo.ADMCourseMaster CM ON CM.Course_ID = S.Course_ID							
							WHERE Comb.Semester={semester}   AND Comb.SemesterBatch={batch}   AND  (R.ExternalMarks is null or R.ExternalMarks='0' or R.ExternalMarks='-2' OR R.ExternalMarks='0.00') 
                            AND (CM.PrintProgramme={(short)programe} OR CM.Course_ID='{CourseId}')    
							GROUP BY S.CollegeFullName,S.CourseFullName,SUB.SubjectFullName,Comb.Semester 				
							UNION
                           select  S.CollegeFullName,S.CourseFullName,SUB.SubjectFullName,ISNULL(COUNT(E.Student_ID),0) NoOfStudentsPending,Comb.Semester,'Backlog' [Types]  from VWStudentWithDetail S                       
							join ARGStudentExamForm_{printProgramme.ToString()} E ON E.Student_ID = S.Student_ID AND E.Semester = S.Semester AND E.IsRegular=0 AND e.Status=4 
							JOIN ARGStudentReExamForm RE ON RE.StudentExamForm_ID = E.StudentExamForm_ID AND RE.Subject_ID = S.Subject_ID AND E.Year='2022'
                           join ADMSubjectMaster SUB on SUB.Subject_ID = S.Subject_ID
                            join dbo.{printProgramme}_Semester{semester} R on R.Student_ID = S.Student_ID and R.Subject_ID = S.Subject_ID
                            JOIN dbo.ARGSelectedCombination_{printProgramme.ToString()} Comb ON Comb.Combination_ID = S.Combination_ID  AND Comb.Student_ID = R.Student_ID
                            JOIN dbo.ADMCourseMaster CM ON CM.Course_ID = S.Course_ID							
							WHERE Comb.Semester={semester}  AND CM.PrintProgramme={(short)programe} AND Comb.SemesterBatch<{batch}   AND  (R.ExternalMarks is null or R.ExternalMarks='0' or R.ExternalMarks='-2' OR R.ExternalMarks='0.00') 
							AND  (CM.PrintProgramme={(short)programe} OR CM.Course_ID='{CourseId}') 
                            GROUP BY S.CollegeFullName,S.CourseFullName,SUB.SubjectFullName,Comb.Semester  )  o
							PIVOT
							(
							MAX(NoOfStudentsPending) FOR Types in(Regular,Backlog)
							)AS PIVOTTABLE
                            ORDER by PIVOTTABLE.CollegeFullName,PIVOTTABLE.CourseFullName,PIVOTTABLE.SubjectFullName";

            return Query;
        }

        #endregion

        #region Result DataList   
        internal string ResulDataListQuery(Parameters Parameter, PrintProgramme printProgramme, short? semester, int batch, Programme programe, Guid CourseId, Guid AcceptCollege_ID, bool value)
        {
            var Query = string.Empty;
            var Subquery = semester == 0 ? " SC.Semester Is Not Null" : $" SC.Semester={semester}";
            var Subquery2 = semester == 0 ? "ef.Semester Is Not Null" : $" ef.Semester={semester}";
            SqlCommand command = new SqlCommand();

            if (value == true)
            {

                Query = $@" SELECT temp.FullName,
                            temp.FathersName,
                            temp.Category,
                            temp.Gender,
                            temp.ABCID,
                            temp.CUSRegistrationNo,
                            temp.DOB,
                            temp.ClassRollNo,
                            temp.Batch,
                            temp.CourseFullName,
                            temp.CollegeFullName,
                            temp.TotalCreditsEarned,
                            temp.CGPA,
                            temp.[CGPT OUT OF],
                            temp.Percentage,
                            temp.Semester,
                            temp.ExamRollNumber FROM (
                            SELECT 
                           v.FullName,
                           FathersName,
	                       p.Category,
	                       p.Gender,
	                       p.ABCID,
                           v.CUSRegistrationNo,
                           v.DOB,
                           v.ClassRollNo,
                           v.Batch,
                           TDS.AwardedDegreeTitle CourseFullName,
                           CollegeFullName,
                           TotalCreditsEarned,
                           CGPA,
	                       '10.0' [CGPT OUT OF],
                           Percentage,
                           MSCGPA.Semesterto AS Semester,						   
						   (SELECT TOP 1 ExamRollNumber FROM dbo.ARGStudentExamForm_{printProgramme} WHERE Student_ID IN(dbo.MSCGPA.Student_ID) AND ExamRollNumber IS NOT NULL) ExamRollNumber,
				           ROW_NUMBER() OVER (PARTITION BY p.CUSRegistrationNo 
				           ORDER BY  dbo.MSCGPA.SemesterTo DESC )AS row_num				   
				            FROM dbo.VWStudentCourse v
                    JOIN dbo.MSCGPA ON MSCGPA.Student_ID = v.Student_ID
                    JOIN dbo.ARGStudentExamForm_{printProgramme} ef ON ef.Student_ID = v.Student_ID
                    JOIN dbo.ARGPersonalInformation_{printProgramme} p ON p.Student_ID = ef.Student_ID
			        JOIN dbo.MSTranscriptDegreeSettings TDS ON TDS.Course_ID = v.Course_ID AND TDS.SemesterTo = MSCGPA.SemesterTo
                    WHERE v.Batch={batch}  AND v.AcceptCollege_ID='{AcceptCollege_ID}' 
                     ) temp WHERE temp.row_num=1
			         ORDER BY temp.CourseFullName,temp.FullName,temp.ExamRollNumber,temp.Gender";

            }
            else
            {


                Query = $@" SELECT DISTINCT
                    CMR.CollegeFullName,
                     SC.SemesterBatch,
                    CourseFullName,
                    P.FullName,
                    P.ABCID,
                    P.CUSRegistrationNo,
                    Exm.ExamRollNumber,
                    SC.Semester,
                    P.Gender,
                    P.Category,
                    SM.SubjectFullName,
                    dbo.FNSubjectTypeDescription(SM.SubjectType) SubjectType,
                    (COALESCE(COALESCE(S.InternalMarks, NULL, 0), -2, 0)
                     + COALESCE(COALESCE(S.InternalAttendance_AssessmentMarks, NULL, 0), -2, 0)
                     + COALESCE(COALESCE(S.ExternalAttendance_AssessmentMarks, NULL, 0), -2, 0)
                     + COALESCE(COALESCE(S.ExternalMarks, NULL, 0), -2, 0)
                    ) AS MarksObt,
                    (COALESCE(COALESCE(MS.InternalMaxMarks, NULL, 0), -2, 0)
                     + COALESCE(COALESCE(MS.InternalAttendance_AssessmentMaxMarks, NULL, 0), -2, 0)
                     + COALESCE(COALESCE(MS.ExternalAttendance_AssessmentMaxMarks, NULL, 0), -2, 0)
                     + COALESCE(COALESCE(MS.ExternalMaxMarks, NULL, 0), -2, 0)
                    ) AS maxmarks,
                    CAST(((COALESCE(COALESCE(S.InternalMarks, NULL, 0), -2, 0)
                           + COALESCE(COALESCE(S.InternalAttendance_AssessmentMarks, NULL, 0), -2, 0)
                           + COALESCE(COALESCE(S.ExternalAttendance_AssessmentMarks, NULL, 0), -2, 0)
                           + COALESCE(COALESCE(S.ExternalMarks, NULL, 0), -2, 0)
                          )
                          / (COALESCE(COALESCE(MS.InternalMaxMarks, NULL, 0), -2, 0)
                             + COALESCE(COALESCE(MS.InternalAttendance_AssessmentMaxMarks, NULL, 0), -2, 0)
                             + COALESCE(COALESCE(MS.ExternalAttendance_AssessmentMaxMarks, NULL, 0), -2, 0)
                             + COALESCE(COALESCE(MS.ExternalMaxMarks, NULL, 0), -2, 0)
                            ) * 100
                         ) AS DECIMAL(5, 2)) AS PerCentage
                ,RN.Dated AS DateOfDeclaration
                FROM dbo.ARGPersonalInformation_{printProgramme} P
                    JOIN dbo.{printProgramme}_Semester{(semester.HasValue && semester.Value > 0 ? semester.Value.ToString() : "All")} S
                        ON S.Student_ID = P.Student_ID
                    JOIN dbo.ResultNotification RN
                        ON RN.ResultNotification_ID = S.ResultNotification_ID --AND RN.ResultNotification_ID = MAX(RN.Dated)			
                    JOIN dbo.ARGStudentExamForm_{printProgramme} EXM ON EXM.Student_ID = P.Student_ID AND EXM.Student_ID = S.Student_ID and
				          S.ExamForm_ID=EXM.StudentExamForm_ID
                    JOIN dbo.ADMSubjectMaster SM
                        ON SM.Subject_ID = S.Subject_ID
                    JOIN dbo.ADMCourseMaster CMs
                        ON CMs.Course_ID = SM.Course_ID
                    JOIN dbo.ADMCollegeMaster CMR
                        ON CMR.College_ID = P.AcceptCollege_ID
                    JOIN dbo.MSSubjectMarksStructure MS
                        ON MS.SubjectMarksStructure_ID = SM.SubjectMarksStructure_ID
                    JOIN dbo.ARGSelectedCombination_{printProgramme} SC
                        ON   SC.Student_ID = P.Student_ID AND SC.Student_ID = S.Student_ID  AND SC.Semester = SM.Semester AND SC.IsVerified=1
                    JOIN dbo.ADMCombinationMaster CM
                        ON CM.Combination_ID = SC.Combination_ID
                   WHERE CMs.PrintProgramme={(short)printProgramme} AND SC.SemesterBatch={batch}  AND CM.Course_ID='{CourseId}' AND P.AcceptCollege_ID='{AcceptCollege_ID}'
                    AND Exm.ExamRollNumber IS NOT NULL and {Subquery}  ORDER BY CourseFullName, EXM.ExamRollNumber,SC.Semester,SM.SubjectFullName";

            }

            return Query;
        }

        #endregion



        #region NewResultSection

        internal string StudentInfoBacklogonly(PrintProgramme printProgramme, short semester)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            var Query = $@"SELECT STDINFO.MarksCardSerialNo,STDINFO.Student_ID,STDINFO.FullName,STDINFO.CUSRegistrationNo,SelectedComb.Combination_ID,STDINFO.FathersName,STDINFO.ClassRollNo,STDINFO.AcceptCollege_ID
                           ,EXMFRM.ExamRollNumber,{semester} AS Semester,'{printProgramme}' AS PrintProgramme,Programme,ADMCombinationMaster.Course_ID,CourseFullName,STDINFO.Batch
                            FROM ARGPersonalInformation_{printProgramme.ToString()} STDINFO
                            JOIN ARGSelectedCombination_{printProgramme.ToString()} SelectedComb ON SelectedComb.Student_ID = STDINFO.Student_ID AND SelectedComb.Semester={semester}
                            JOIN ADMCombinationMaster ON ADMCombinationMaster.Combination_ID = SelectedComb.Combination_ID AND ADMCombinationMaster.Semester = SelectedComb.Semester
							JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = ADMCombinationMaster.Course_ID
                            LEFT JOIN (Select Distinct ExamRollNumber,Student_ID FROM ARGStudentExamForm_{printProgramme.ToString()} WHERE ExamRollNumber Is NOT NULL)EXMFRM ON EXMFRM.Student_ID=STDINFO.Student_ID";
            return Query;
        }

        #endregion

        internal string StudentReEvaluationQuery(PrintProgramme printProgramme, short semester)
        {
            var query = $@"SELECT STDINFO.Student_ID,STDINFO.FullName,STDINFO.CUSRegistrationNo,
                            VWSCMaster.Subject_ID,VWSCMaster.SubjectFullName,SubjectType,
                            VWSCMaster.TheoryMinPassMarks,VWSCMaster.PracticalMinPassMarks,
                            Result._ID,Result.ExamRollNumber
                            ,Result.TheorySubmitted,Result.TheoryMarks TheoryMarksObtained
                            ,Result.PracticalSubmitted,( (CASE WHEN Result.PracticalMarks<0 THEN 0 ELSE	Result.PracticalMarks END) +(CASE WHEN Result.PracticalAttendance<0 THEN 0 ELSE	Result.PracticalAttendance END)+ (CASE WHEN Result.TheoryAttendance<0 THEN 0 ELSE	Result.TheoryAttendance END)) PracticalMarksObtained
                            ,Result.PracticalInternalMarks PracticalInternalMarksObtained,Result.PracticalExternalMarks PracticalExternalMarksObtained,Result.PracticalExternalMarksSubmitted,Result.PracticalInternalMarksSubmitted
                            FROM ARGPersonalInformation_{printProgramme.ToString()} STDINFO
                            JOIN ARGSelectedCombination_{printProgramme.ToString()} SelectedComb ON SelectedComb.Student_ID = STDINFO.Student_ID AND SelectedComb.Semester={semester}
                            JOIN VWSCMaster ON VWSCMaster.Combination_ID = SelectedComb.Combination_ID
                            JOIN {printProgramme.ToString()}_Semester{semester} Result ON Result.Student_ID = STDINFO.Student_ID AND VWSCMaster.Subject_ID=Result.Subject_ID 
                            LEFT JOIN ResultHistory ON ResultHistory.Entity_ID = _ID                            
                            LEFT JOIN ResultInternalsHistory ON ResultInternalsHistory.Entity_ID = _ID ";
            return query;
        }

        internal string GetSemesterResultBy_ID(Guid _ID, PrintProgramme printProgramme, short semester)
        {
            var Query = $@"SELECT S.Student_ID, S.CUSRegistrationNo,FullName,ClassRollNo ,_ID,R.Subject_ID,SubjectFullName,SubjectType,
                        ExternalMarks,ExternalAttendance_AssessmentMarks,InternalMarks,InternalAttendance_AssessmentMarks,ExternalSubmitted,InternalSubmitted,
                        ResultNotification_ID,ExamForm_ID,{semester} SemesterName,R.InternalUpdatedOn,R.InternalUpdatedBy,R.ExternalUpdatedOn,R.ExternalUpdatedBy
                        FROM dbo.{printProgramme.ToString()}_Semester{semester} R
                        JOIN dbo.ARGPersonalInformation_{printProgramme.ToString()} S ON S.Student_ID = R.Student_ID
                        JOIN dbo.ADMSubjectMaster Sub ON Sub.Subject_ID = R.Subject_ID
                        WHERE R._ID='{_ID.ToString()}'";
            return Query;
        }

        internal string GetAppliedReAppearSubjects(Guid student_ID, PrintProgramme printProgramme, short semester, int Year)
        {
            printProgramme = printProgramme == PrintProgramme.BED ? PrintProgramme.UG : printProgramme;

            var query = $@"SELECT *,CASE WHEN FeeStatus = {(short)FormStatus.FeePaid} OR FeeStatus = {(short)FormStatus.UpdatedManually} THEN 1 ELSE 0 END IsApplied  
				                    FROM ARGStudentExamForm_{printProgramme.ToString()}                                                    
                                    INNER JOIN ARGStudentReExamForm ON ARGStudentReExamForm.StudentExamForm_ID = ARGStudentExamForm_{printProgramme.ToString()}.StudentExamForm_ID			                                                            
                            WHERE ARGStudentExamForm_{printProgramme.ToString()}.Student_ID = '{student_ID}' AND Semester={semester} AND IsRegular = 0 AND YEAR = {Year}";
            return query;
        }

        internal string GetAppliedReAppearSubjects(Guid studentExamForm_ID, PrintProgramme printProgramme)
        {
            printProgramme = printProgramme == PrintProgramme.BED ? PrintProgramme.UG : printProgramme;

            var query = $@"SELECT *,CASE WHEN FeeStatus = {(short)FormStatus.FeePaid} OR FeeStatus = {(short)FormStatus.UpdatedManually} THEN 1 ELSE 0 END IsApplied  
				                    FROM ARGStudentExamForm_{printProgramme.ToString()}                                                    
                                    INNER JOIN ARGStudentReExamForm ON ARGStudentReExamForm.StudentExamForm_ID = ARGStudentExamForm_{printProgramme.ToString()}.StudentExamForm_ID	
INNER JOIN ADMSubjectMaster ON ADMSubjectMaster.Subject_ID = ARGStudentReExamForm.Subject_ID
                            WHERE ARGStudentExamForm_{printProgramme.ToString()}.StudentExamForm_ID = '{studentExamForm_ID}'";
            return query;
        }
    }
}
