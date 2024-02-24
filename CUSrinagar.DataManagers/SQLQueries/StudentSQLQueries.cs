using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CUSrinagar.Models;
using GeneralModels;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;

namespace CUSrinagar.DataManagers.SQLQueries
{
    public class StudentSQLQueries
    {
        public static string GetAllStudents = "SELECT * FROM STUDENTPROFILE ";

        public static string GetResultQuery = @"  SELECT
	                                                    UG_Semester1.*
                                                       ,Subjects.Name
                                                       ,MinMarks
                                                       ,MaxMarks
                                                       ,NEWID() AS Course_ID
                                                       ,StudentProfile.RegistrationNumber AS RegistrationNumber
                                                    FROM (SELECT
		                                                    SubjectCombinations.Subject1_ID AS Subject_ID
	                                                    FROM SubjectCombinations
	                                                    INNER JOIN Subjects
		                                                    ON SubjectCombinations.Subject1_ID = Subjects.Subject_ID
	                                                    UNION
	                                                    SELECT
		                                                    SubjectCombinations.Subject2_ID AS Subject_ID
	                                                    FROM SubjectCombinations
	                                                    INNER JOIN Subjects
		                                                    ON SubjectCombinations.Subject2_ID = Subjects.Subject_ID
	                                                    UNION
	                                                    SELECT
		                                                    SubjectCombinations.Subject3_ID AS Subject_ID
	                                                    FROM SubjectCombinations
	                                                    INNER JOIN Subjects
		                                                    ON SubjectCombinations.Subject3_ID = Subjects.Subject_ID
	                                                    UNION
	                                                    SELECT
		                                                    SubjectCombinations.Subject4_ID AS Subject_ID
	                                                    FROM SubjectCombinations
	                                                    INNER JOIN Subjects
		                                                    ON SubjectCombinations.Subject2_ID = Subjects.Subject_ID
	                                                    UNION
	                                                    SELECT
		                                                    SubjectCombinations.Subject5_ID AS Subject_ID
	                                                    FROM SubjectCombinations
	                                                    INNER JOIN Subjects
		                                                    ON SubjectCombinations.Subject2_ID = Subjects.Subject_ID
	                                                    UNION
	                                                    SELECT
		                                                    SubjectCombinations.Subject6_ID AS Subject_ID
	                                                    FROM SubjectCombinations

	                                                    INNER JOIN Subjects
		                                                    ON SubjectCombinations.Subject2_ID = Subjects.Subject_ID
	                                                    UNION

	                                                    SELECT
		                                                    SubjectCombinations.Subject7_ID AS Subject_ID
	                                                    FROM SubjectCombinations
	                                                    INNER JOIN Subjects
		                                                    ON SubjectCombinations.Subject2_ID = Subjects.Subject_ID) SubjectCombinations
                                                    INNER JOIN UG_Semester1
	                                                    ON SubjectCombinations.Subject_ID = UG_Semester1.Subject_ID
                                                    LEFT JOIN Subjects
	                                                    ON UG_Semester1.Subject_ID = Subjects.Subject_ID
                                                    LEFT JOIN StudentProfile
	                                                    ON StudentProfile.RegistrationNumber = UG_Semester1.RegistrationNo ";

        internal SqlCommand GetStudentIDByCourseAndBatch(int batch, Guid course_ID)
        {
            SqlCommand command = new SqlCommand();
            command.CommandText = $@" SELECT student_ID FROM vwstudentcourse WHERE Course_ID = '{course_ID}' AND batch = {batch}";
            return command;
        }

        internal SqlCommand GetStudentPaymentHistory(Guid student_Id, PrintProgramme printProgramme)
        {
            string postfix = new GeneralFunctions().GetProgrammePostFix(printProgramme);
            SqlCommand command = new SqlCommand();
            command.CommandText = $@" SELECT * FROM (
                                        SELECT Semester,TxnAmount as Amount,TxnDate as PaymentOn,Student_ID,ModuleType,PaymentType,TxnReferenceNo FROM dbo.PaymentDetails{postfix}
                                        JOIN dbo.ARGPersonalInformation{postfix}
                                        ON Entity_ID=Student_ID
                                        WHERE  Student_ID=@student_ID
                                        union
                                        SELECT PaymentDetails{postfix}.Semester,TxnAmount as Amount,TxnDate  as PaymentOn,ARGPersonalInformation{postfix}.Student_ID,ModuleType,PaymentType,TxnReferenceNo FROM dbo.PaymentDetails{postfix}
                                        JOIN dbo.ARGStudentExamForm{postfix}
                                        ON PaymentDetails{postfix}.Entity_ID=ARGStudentExamForm{postfix}.StudentExamForm_ID
                                        JOIN dbo.ARGPersonalInformation{postfix}
                                        ON ARGPersonalInformation{postfix}.Student_ID = ARGStudentExamForm{postfix}.Student_ID
                                        WHERE ARGPersonalInformation{postfix}.Student_ID=@student_ID
                                        union
                                        SELECT ReEvaluation.Semester,TxnAmount as Amount,TxnDate  as PaymentOn,ReEvaluation.Student_ID,ModuleType,PaymentType,TxnReferenceNo  FROM dbo.ReEvaluation
                                        JOIN dbo.PaymentDetails{postfix}
                                        on Entity_ID=ReEvaluation_ID
                                        WHERE ReEvaluation.Student_ID=@student_ID
                                        ) p WHERE p.ModuleType <>{(short)PaymentModuleType.None}";
            command.Parameters.AddWithValue("@student_ID", student_Id);
            return command;
        }

        internal string GetStudentsForFeeStructure(int _Programme, int year)
        {
            string MasterCourseValue = "";

            switch (_Programme)
            {
                case 1:
                    MasterCourseValue = $"({(short)Programme.PG},{(short)Programme.IG},{(short)Programme.HS})";
                    break;

                case 2:
                    MasterCourseValue = $"({(short)Programme.Professional})";
                    break;

                case 3:
                    MasterCourseValue = $"({(short)Programme.UG})";
                    break;

                default:
                    break;
            }

            return $@"SELECT COUNT(*)
                                FROM
                                (
                                    SELECT CreatedOn, Student_ID, FormStatus
                                    FROM dbo.ARGPersonalInformation_UG
                                    UNION
                                    SELECT CreatedOn, Student_ID, FormStatus
                                    FROM dbo.ARGPersonalInformation_IH
                                    UNION
                                    SELECT CreatedOn, Student_ID, FormStatus
                                    FROM dbo.ARGPersonalInformation_PG
                                ) AllStudents
                                    INNER JOIN
                                    (
                                        SELECT Student_ID, Combination_ID
                                        FROM dbo.ARGSelectedCombination_UG
                                        WHERE Semester = 1
                                        UNION
                                        SELECT Student_ID, Combination_ID
                                        FROM dbo.ARGSelectedCombination_IH
                                        WHERE Semester = 1
                                        UNION
                                        SELECT Student_ID, Combination_ID
                                        FROM dbo.ARGSelectedCombination_PG
                                        WHERE Semester = 1
                                    ) ARGSelectedCombination
                                        ON ARGSelectedCombination.Student_ID = AllStudents.Student_ID
                                    INNER JOIN dbo.ADMCombinationMaster
                                        ON ARGSelectedCombination.Combination_ID = ADMCombinationMaster.Combination_ID
                                    INNER JOIN dbo.ADMCourseMaster
                                        ON ADMCourseMaster.Course_ID = ADMCombinationMaster.Course_ID
                                    INNER JOIN
                                    (
                                        SELECT Entity_ID,TxnAmount,TxnDate
                                        FROM dbo.PaymentDetails_UG
                                        WHERE ModuleType IN ( {(short)PaymentModuleType.Admission},{(short)PaymentModuleType.SemesterAdmission}, {(short)PaymentModuleType.CertificateCoursesAdmission} )
                                        UNION
                                        SELECT Entity_ID,TxnAmount,TxnDate
                                        FROM dbo.PaymentDetails_IH
                                        WHERE ModuleType IN ( {(short)PaymentModuleType.Admission}, {(short)PaymentModuleType.SemesterAdmission}, {(short)PaymentModuleType.CertificateCoursesAdmission} )
                                        UNION
                                        SELECT Entity_ID,TxnAmount,TxnDate
                                        FROM dbo.PaymentDetails_PG
                                        WHERE ModuleType IN ( {(short)PaymentModuleType.Admission}, {(short)PaymentModuleType.SemesterAdmission}, {(short)PaymentModuleType.CertificateCoursesAdmission} )
                                    ) Payments
                                        ON Payments.Entity_ID = AllStudents.Student_ID
                                                                WHERE DATEPART(YEAR, Payments.TxnDate) = {year}
                                                                        AND Programme IN {MasterCourseValue}
                                                                        AND FormStatus <> {(short)FormStatus.InProcess};";
        }

        public SqlCommand GetSeatCountWherenotComplete(int batch, Programme Programme)
        {
            string query;
            query = $@"  SELECT Course_Id,PrintProgramme,SUM(AllocatedSeats) AllocatedSeats,SUM(NoofSeats) NoofSeats FROM   SeatCountAgainstCourse SC 
	                             GROUP BY Course_Id,PrintProgramme  HAVING PrintProgramme={(int)Programme} AND SUM(AllocatedSeats)<SUM(NoofSeats) 
                           AND (SELECT COUNT(CA.Student_ID) FROM ARGPersonalInformation_{Programme.GetTablePFix()} PINFO JOIN ARGCoursesApplied_{Programme.GetTablePFix()} CA 
                            ON CA.Student_ID = PINFO.Student_ID AND IsEligible=1 AND Course_ID=SC.Course_Id AND PINFO.FormStatus=10   AND IsLateralEntry=1
                             JOIN ARGStudentPreviousQualifications_{Programme.GetTablePFix()} ON ARGStudentPreviousQualifications_{Programme.GetTablePFix()}.Student_ID = PINFO.Student_ID
                            LEFT JOIN SeatAllocationMatrix_{Programme.GetTablePFix()} SM ON SM.Student_Id = PINFO.Student_ID 
                           WHERE SeatAllocationMatrix_Id IS NULL AND AcceptCollege_ID IS NULL AND Batch={batch}  AND PINFO.FormStatus=10 AND CA.SelfFinancedPayment_ID IS null  )>0";

            if (Programme == Programme.PG)
            {
                query = $@"  SELECT Course_Id,PrintProgramme,SUM(AllocatedSeats) AllocatedSeats,SUM(NoofSeats) NoofSeats FROM   SeatCountAgainstCourse SC 
	                             GROUP BY Course_Id,PrintProgramme  HAVING PrintProgramme={(int)PrintProgramme.PG} AND SUM(AllocatedSeats)<SUM(NoofSeats) 
                           AND (SELECT COUNT(CA.Student_ID) FROM ARGPersonalInformation_{Programme.GetTablePFix()} PINFO JOIN ARGCoursesApplied_{Programme.GetTablePFix()} CA 
                            ON CA.Student_ID = PINFO.Student_ID AND IsEligible=1 AND Course_ID=SC.Course_Id AND PINFO.FormStatus=10   AND IsLateralEntry=0
                             JOIN ARGStudentPreviousQualifications_{Programme.GetTablePFix()} ON ARGStudentPreviousQualifications_{Programme.GetTablePFix()}.Student_ID = PINFO.Student_ID
                            LEFT JOIN SeatAllocationMatrix_{Programme.GetTablePFix()} SM ON SM.Student_Id = PINFO.Student_ID 
                           WHERE SeatAllocationMatrix_Id IS NULL AND AcceptCollege_ID IS NULL AND Batch={batch}  AND PINFO.FormStatus=10
                            AND CA.SelfFinancedPayment_ID IS null  )>0";
            }

            if (Programme == Programme.UG)
            {

                query = $@"SELECT Course_Id,
                               PrintProgramme,
                               SUM(AllocatedSeats) AllocatedSeats,
                               SUM(NoofSeats) NoofSeats
                        FROM SeatCountAgainstCourse SC
                        GROUP BY Course_Id,
                                 PrintProgramme,
                                 SC.College_Id
                        HAVING PrintProgramme = {(int)PrintProgramme.UG}
                               AND SUM(AllocatedSeats) < SUM(NoofSeats)
                               AND
                               (
                                   SELECT COUNT(CA.Student_ID)
                                   FROM ARGPersonalInformation_{Programme.GetTablePFix()} PINFO
                                       JOIN ARGCoursesApplied_{Programme.GetTablePFix()} CA
                                           ON CA.Student_ID = PINFO.Student_ID
                                              AND IsEligible = 1
                                              AND CA.Course_ID = SC.Course_Id
                                              AND PINFO.FormStatus = {(int)FormStatus.FeePaid}
                                       JOIN ARGStudentPreviousQualifications_{Programme.GetTablePFix()}
                                           ON ARGStudentPreviousQualifications_{Programme.GetTablePFix()}.Student_ID = PINFO.Student_ID
                                       JOIN ADMCollegeCourseMapping
                                           ON ADMCollegeCourseMapping.Course_ID = CA.Course_ID
                                       LEFT JOIN SeatAllocationMatrix_{Programme.GetTablePFix()} SM
                                           ON SM.Student_Id = PINFO.Student_ID
                                   WHERE SeatAllocationMatrix_Id IS NULL
                                         AND AcceptCollege_ID IS NULL
                                         AND Batch = {batch}
                                         AND PINFO.FormStatus = {(int)FormStatus.FeePaid}
                               ) > 0
                               AND SC.College_Id <> 'B5E689E6-75FB-44E9-8ED7-3E649C18B659'--GWC
                        UNION
                        SELECT Course_Id,
                               PrintProgramme,
                               SUM(AllocatedSeats) AllocatedSeats,
                               SUM(NoofSeats) NoofSeats
                        FROM SeatCountAgainstCourse SC
                        GROUP BY Course_Id,
                                 PrintProgramme,
                                 SC.College_Id
                        HAVING PrintProgramme = {(int)PrintProgramme.UG}
                               AND SUM(AllocatedSeats) < SUM(NoofSeats)
                               AND
                               (
                                   SELECT COUNT(CA.Student_ID)
                                   FROM ARGPersonalInformation_{Programme.GetTablePFix()} PINFO
                                       JOIN ARGCoursesApplied_{Programme.GetTablePFix()} CA
                                           ON CA.Student_ID = PINFO.Student_ID
                                              AND IsEligible = 1
                                              AND Course_ID = SC.Course_Id
                                              AND PINFO.FormStatus = {(int)FormStatus.FeePaid}
                                       JOIN ADMCollegeCourseMapping
                                           ON ADMCollegeCourseMapping.Course_ID = CA.Course_ID
                                       JOIN ARGStudentPreviousQualifications_{Programme.GetTablePFix()}
                                           ON ARGStudentPreviousQualifications_{Programme.GetTablePFix()}.Student_ID = PINFO.Student_ID
                                       JOIN NEPCollegePreferences
                                           ON NEPCollegePreferences.College_ID = ADMCollegeCourseMapping.College_ID
                                              AND NEPCollegePreferences.Student_ID = PINFO.Student_ID
                                       LEFT JOIN SeatAllocationMatrix_{Programme.GetTablePFix()} SM
                                           ON SM.Student_Id = PINFO.Student_ID
                                   WHERE SeatAllocationMatrix_Id IS NULL
                                         AND AcceptCollege_ID IS NULL
                                         AND Batch = {batch}
                                         AND PINFO.FormStatus = {(int)FormStatus.FeePaid}
                                         AND PINFO.Gender <> 'MALE'
                               ) > 0
                               AND SC.College_Id = 'B5E689E6-75FB-44E9-8ED7-3E649C18B659';--Gwc";
            }


            SqlCommand command = new SqlCommand();
            command.CommandText = query;
            return command;
        }
        public SqlCommand GetSeatCountWhetherToContinue(int batch, PrintProgramme printProgramme)
        {
            string query = $@"  SELECT SC.Course_Id FROM SeatCountAgainstCourse SC
                                        WHERE PrintProgramme={(int)printProgramme} AND SC.AllocatedSeats<CAST(SC.NoofSeats as int) AND (SELECT COUNT(CA.Student_ID) FROM  ARGPersonalInformation_{printProgramme.GetTablePFix()} PINFO JOIN ARGCoursesApplied_{printProgramme.GetTablePFix()} CA ON
CA.Student_ID = PINFO.Student_ID AND IsEligible=1 AND batch = {batch} AND FormStatus = 10 AND AcceptCollege_ID IS NULL LEFT JOIN SeatAllocationMatrix_{printProgramme.GetTablePFix()} SM ON SM.Student_Id = CA.Student_ID
                                        WHERE SeatAllocationMatrix_Id IS NULL AND Course_ID = Sc.Course_Id AND Category = SC.Category)>0";
            SqlCommand command = new SqlCommand();
            command.CommandText = query;
            return command;
        }
        public SqlCommand GetSeatCountWhetherToContinueForCAT(int batch, PrintProgramme printProgramme)
        {
            string query = $@"  SELECT SC.Course_Id FROM SeatCountAgainstCourse SC
                                        WHERE PrintProgramme={(int)printProgramme} AND SC.AllocatedSeats<SC.NoofSeats AND (SELECT COUNT(CA.Student_ID) FROM  ARGPersonalInformation_{printProgramme.GetTablePFix()}  JOIN ARGCoursesApplied_{printProgramme.GetTablePFix()} CA ON CA.Student_ID = PINFO.Student_ID AND batch = {batch} 
AND IsEligible=1 AND FormStatus = 10 AND AcceptCollege_ID IS NULL LEFT JOIN SeatAllocationMatrix_{printProgramme.GetTablePFix()} SM ON SM.Student_Id = CA.Student_ID
                                        WHERE SeatAllocationMatrix_Id IS NULL AND Course_ID = Sc.Course_Id AND Category = SC.Category)>0";
            SqlCommand command = new SqlCommand();
            command.CommandText = query;
            return command;
        }
        public SqlCommand GetSeatCountByCourse(Guid Course_ID)
        {
            string query = $@"SELECT SeatCountAgainstCourse.*,Percentage FROM SeatCountAgainstCourse JOIN AdmissionSelectionCriteria ON CategoryCode=Category 
                              WHERE Course_Id='{Course_ID}' ORDER BY ActualNoofSeats desc";
            SqlCommand command = new SqlCommand();
            command.CommandText = query;
            return command;
        }

        public SqlCommand GetSeatAllocation(Guid Student_ID, PrintProgramme printProgramme)
        {
            string query = $@"SELECT *  FROM SeatAllocationMatrix_{printProgramme.GetTablePFix()}  WHERE Student_Id='{Student_ID}' and IsFinallySelected=1";
            SqlCommand command = new SqlCommand();
            command.CommandText = query;
            return command;
        }

        public SqlCommand GetCatHavSeatCountZero(Guid Course_ID)
        {
            string query = $@"SELECT * FROM  SeatCountAgainstCourse WHERE Course_Id='{Course_ID}' AND NoofSeats=0  AND Category <>'EWS'
                               ORDER BY ActualNoofSeats desc";
            SqlCommand command = new SqlCommand();
            command.CommandText = query;
            return command;
        }

        internal SqlCommand GetStudentsNEduP(Programme Programme, int batch)
        {
            string InClause = string.Empty;
            string query = $@"  SELECT Distinct PINFO.Student_ID,Category,DOB,SubjectEntrancePoints,
               CAST(((MarksObt/MaxMarks)*100) AS DECIMAL(5,2))
                 AS [Percentage] FROM ARGPersonalInformation_{Programme.GetTablePFix()}  PINFO
                JOIN ARGStudentPreviousQualifications_{Programme.GetTablePFix()} QUAL ON PINFO.Student_ID = QUAL.Student_ID
               JOIN ARGCoursesApplied_{Programme.GetTablePFix()} CAPP  ON CAPP.Student_ID = PINFO.Student_ID
				JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = CAPP.Course_ID AND Programme={(int)Programme}
                 WHERE batch={batch} AND IsLateralEntry=1 AND FormStatus={(int)FormStatus.FeePaid}  AND PINFO.Student_ID NOT IN (SELECT DISTINCT Student_ID FROM ARGCoursesApplied_{Programme.GetTablePFix()} WHERE SelfFinancedPayment_ID IS NOT NULL)";
            if (Programme == Programme.UG)
            { InClause = $" AND PINFO.Student_ID NOT IN  ( SELECT Student_Id FROM SeatAllocationMatrix_{Programme.GetTablePFix()}  WHERE  ISFinallySelected=0 OR ( PreferenceOfCourse=1 AND PreferenceOfCollege=1 AND AssignedUnderCategory=0))"; }
            else
            { InClause = $" AND PINFO.Student_ID NOT IN  ( SELECT Student_Id FROM SeatAllocationMatrix_{Programme.GetTablePFix()}  WHERE  ISFinallySelected=0 OR ( PreferenceOfCourse=1 AND AssignedUnderCategory=0))"; }
            query = query + InClause;
            // ORDER BY [Percentage] DESC , DOB ASC";
            SqlCommand command = new SqlCommand();
            command.CommandText = query;
            return command;
        }

        internal SqlCommand GetStudentsPG(Programme Programme, int batch)
        {
            string InClause = string.Empty;
            string query = $@"  SELECT Distinct PINFO.Student_ID,Category,DOB,SubjectEntrancePoints,
                
               CAST(((MarksObt/MaxMarks)*100) AS DECIMAL(5,2))
                 AS [Percentage] FROM ARGPersonalInformation_{Programme.GetTablePFix()}  PINFO
                JOIN ARGStudentPreviousQualifications_{Programme.GetTablePFix()} QUAL ON PINFO.Student_ID = QUAL.Student_ID
               JOIN ARGCoursesApplied_{Programme.GetTablePFix()} CAPP  ON CAPP.Student_ID = PINFO.Student_ID
				JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = CAPP.Course_ID AND Programme={(int)Programme}
                 WHERE batch={batch} AND IsLateralEntry=0 AND FormStatus={(int)FormStatus.FeePaid}  AND PINFO.Student_ID NOT IN (SELECT DISTINCT Student_ID FROM ARGCoursesApplied_{Programme.GetTablePFix()} WHERE SelfFinancedPayment_ID IS NOT NULL)";
            InClause = $" AND PINFO.Student_ID NOT IN  ( SELECT Student_Id FROM SeatAllocationMatrix_{Programme.GetTablePFix()}  WHERE  ISFinallySelected=0 OR ( PreferenceOfCourse=1 AND AssignedUnderCategory=0))";
            query = query + InClause+"ORDER BY [Percentage] DESC , DOB ASC";
            SqlCommand command = new SqlCommand();
            command.CommandText = query;
            return command;
        }

        internal SqlCommand GetCategoryStudentsNEduP(string category, PrintProgramme printProgramme, int batch)
        {
            string query = $@" SELECT PINFO.Student_ID,Category,DOB,CAST(((MarksObt/MaxMarks)*100) AS DECIMAL(5,2))  AS [Percentage] FROM ARGPersonalInformation_{printProgramme.GetTablePFix()}  PINFO
                JOIN ARGStudentPreviousQualifications_{printProgramme.GetTablePFix()}  QUAL ON PINFO.Student_ID = QUAL.Student_ID
                AND Category<>'{category}' AND PINFO.Student_ID NOT IN (SELECT PI.Student_ID FROM  SeatAllocationMatrix_{printProgramme.GetTablePFix()} SM JOIN
ARGPersonalInformation_{printProgramme.GetTablePFix()} PI ON PI.Student_ID = SM.Student_Id WHERE NOT (AssignedUnderCategory=0 AND  Category<>'OM')) AND batch={batch} AND FormStatus=10  ";
            SqlCommand command = new SqlCommand();
            command.CommandText = query;
            return command;
        }
        internal SqlCommand GetFractionStudentsNEduP(Guid Course_Id, PrintProgramme printProgramme, int batch)
        {
            string query = $@" SELECT TOP(SELECT CONVERT(INT, (SELECT CEILING((SELECT SUM(NoofSeats%1) FROM SeatCountAgainstCourse  WHERE Course_Id = '{Course_Id}' AND Category<>'OM' AND (AllocatedSeats>1 OR NoofSeats<1))))) ) PINFO.Student_ID,Category,ISNULL(CAST(CAST(((CAST(((MarksObt/MaxMarks)*100) AS DECIMAL(5,2)))) AS DECIMAL(5,2)) AS VARCHAR(10)),0)
                     AS Percentage FROM ARGPersonalInformation_{printProgramme.GetTablePFix()} PINFO
                    JOIN ARGStudentPreviousQualifications_{printProgramme.GetTablePFix()} PQ ON PQ.Student_ID = PINFO.Student_ID
                         JOIN ARGCoursesApplied_{printProgramme.GetTablePFix()} CA
                                ON CA.Student_ID =PINFO.Student_ID AND CA.Course_ID= '{Course_Id}'  
                    WHERE PINFO.Student_ID NOT IN (SELECT Student_Id FROM  SeatAllocationMatrix_{printProgramme.GetTablePFix()}) AND batch = {batch} AND FormStatus = 10 AND Category<>'OM'ORDER BY CAST(ISNULL(CAST(CAST(((CAST(((MarksObt/MaxMarks)*100) AS DECIMAL(5,2)))) AS DECIMAL(5,2)) AS VARCHAR(10)),0) AS DECIMAL(5,2)) DESC , DOB ASC";
            SqlCommand command = new SqlCommand();
            command.CommandText = query;
            return command;
        }
        internal SqlCommand GetSelectedStudentsFORCourse(Guid Course_ID, int CurrentList, PrintProgramme printProgramme, int batch)
        {
            // string prgPostFix = "_" + PrintProgramme.IH.GetTablePFix();
            string query = $@" SELECT distinct PINFO.CUETApplicationNo,PINFO.CUETEntranceRollNo,PINFO.Student_ID,PINFO.IsProvisional,PINFO.FullName,PINFO.FathersName,StudentFormNo,CA.Preference,ISFinallySelected,SelectionListNo AS SelectionAgaintListNo,
                  Course_IdAssigned AS Course_ID,CollegeFullName,AssignedUnderCategory,Category,EntranceRollNo,ExamBody,SubjectEntrancePoints, CAST(ISNULL(CAST(CAST(((CAST(((MarksObt/MaxMarks)*100) AS DECIMAL(5,2)))) AS DECIMAL(5,2)) AS VARCHAR(10)),0) AS DECIMAL(5,2)) Percentage FROM ARGPersonalInformation_{printProgramme.GetTablePFix()} PINFO
                                JOIN ARGCoursesApplied_{printProgramme.GetTablePFix()} CA
                                ON CA.Student_ID =PINFO.Student_ID
                                JOIN ADMCourseMaster
                                ON ADMCourseMaster.Course_ID = CA.Course_ID
                                Left JOIN ARGStudentPreviousQualifications_{printProgramme.GetTablePFix()} PQ ON PQ.Student_ID = PINFO.Student_ID 
								INNER JOIN SeatAllocationMatrix_{printProgramme.GetTablePFix()} SM ON SM.Student_Id = PINFO.Student_ID AND (SelectionListNo={CurrentList}OR(ISFinallySelected=1 AND 
                                  SelectionListNo<{CurrentList}))
                                Left JOIN ADMCollegeMaster on College_Id=College_IdAssigned
                               WHERE PINFO.Batch={batch}  AND Course_IdAssigned='{Course_ID}' AND Course_IdAssigned=CA.Course_Id ";
            SqlCommand command = new SqlCommand();
            command.CommandText = query + (printProgramme == PrintProgramme.PG ?
                " ORDER BY SM.AssignedUnderCategory, CA.SubjectEntrancePoints DESC"
                : "  ORDER BY SM.AssignedUnderCategory, CAST(ISNULL(CAST(CAST(((CAST(((MarksObt/MaxMarks)*100) AS DECIMAL(5,2)))) AS DECIMAL(5,2)) AS VARCHAR(10)),0) AS DECIMAL(5,2)) DESC,Category ");
            return command;
        }
        internal SqlCommand GetSelectedStudentsFORCourse(Guid Course_ID, int CurrentList, PrintProgramme printProgramme, int batch, Guid College_ID)
        {
            // string prgPostFix = "_" + PrintProgramme.IH.GetTablePFix();
            string query = $@" SELECT distinct PINFO.CUETApplicationNo,PINFO.CUETEntranceRollNo, PINFO.Student_ID,PINFO.IsProvisional,PINFO.FullName,PINFO.FathersName,StudentFormNo,CA.Preference,ISFinallySelected,
                  Course_IdAssigned AS Course_ID,CollegeFullName,AssignedUnderCategory,Category,EntranceRollNo, CAST(ISNULL(CAST(CAST(((CAST(((MarksObt/MaxMarks)*100) AS DECIMAL(5,2)))) AS DECIMAL(5,2)) AS VARCHAR(10)),0) AS DECIMAL(5,2)) Percentage FROM ARGPersonalInformation_{printProgramme.GetTablePFix()} PINFO
                                JOIN ARGCoursesApplied_{printProgramme.GetTablePFix()} CA
                                ON CA.Student_ID =PINFO.Student_ID
                                JOIN ADMCourseMaster
                                ON ADMCourseMaster.Course_ID = CA.Course_ID
                                Left JOIN ARGStudentPreviousQualifications_{printProgramme.GetTablePFix()} PQ ON PQ.Student_ID = PINFO.Student_ID 
								INNER JOIN SeatAllocationMatrix_{printProgramme.GetTablePFix()} SM ON SM.Student_Id = PINFO.Student_ID AND (SelectionListNo={CurrentList}OR(ISFinallySelected=1 AND 
                                  SelectionListNo<{CurrentList}))
                                Left JOIN ADMCollegeMaster on College_Id=College_IdAssigned
                               WHERE PINFO.Batch={batch}  AND Course_IdAssigned='{Course_ID}' AND Course_IdAssigned=CA.Course_Id  AND College_IdAssigned='{College_ID}'";
            SqlCommand command = new SqlCommand();
            command.CommandText = query + "  ORDER BY CAST(ISNULL(CAST(CAST(((CAST(((MarksObt/MaxMarks)*100) AS DECIMAL(5,2)))) AS DECIMAL(5,2)) AS VARCHAR(10)),0) AS DECIMAL(5,2)) DESC,Category ";
            return command;
        }
        internal SqlCommand GetSeatCountAgainstCourse(Guid Course_ID)
        {
            string prgPostFix = "_" + PrintProgramme.IH.GetTablePFix();
            string query = $@" SELECT * FROM SeatCountAgainstCourse WHERE Course_ID='{Course_ID}' ";
            SqlCommand command = new SqlCommand();
            command.CommandText = query + "  ORDER BY AllocatedSeats DESC ";
            return command;
        }


        public string GetExtraClause(string course_Id, string additionalcourse, string prgPostFix)
        {
            string query = string.Empty;
            query = $" AND ARGCoursesApplied{prgPostFix}.Course_Id IN ('{course_Id + "','" + additionalcourse}') and IsActive=0 ";
            return query;
        }
        public string GetAdditionalCourse(string course_Id)
        {
            switch (course_Id)
            {
                //case "653AA86C-4900-484E-B566-EF49FD4E8313":
                //    return "A7CB49E3-6E17-41D5-A696-62D8C5A8AF1B";
                //case "9702E8EC-5E1E-4E46-8FCB-26F996C898D0":
                //    return "8368EEEF-8F08-4077-AABE-48B348C1F85B";
                //case "C490C5AF-95B6-407A-AE85-668A5FD9E277":
                //    return "58144C9C-319B-4AA1-B335-BF77B0FD1C24";
                //case "9B15F7E4-967D-453F-BEE8-8B624FE6D0DF":
                //    return "32996CE4-5613-4A2D-A565-030390D7F496";
                //case "4E7A4B78-B87C-4666-A517-069656D55281":
                //    return "DC9595E4-B4DC-41E3-AEB9-446C0B990BCF";
                default:
                    return string.Empty;
            }
        }
        public string GetHonorsClause(string CourseID, string prgPostFix)
        {
            Tuple<string, List<Guid[]>> tuple = new EntranceCentersDB().GetCourseGrouping(PrintProgramme.IH);
            Guid oppcourse;
            Guid Course_ID = Guid.Parse(CourseID);
            if (tuple != null && tuple.Item2.Any())
            {
                oppcourse =
                     tuple.Item2.Find(x => x[0] == Course_ID || x[1] == Course_ID).ToList().FindIndex(x => x == Course_ID) == 0
                     ? tuple.Item2.Find(x => x[0] == Course_ID || x[1] == Course_ID).ElementAt(1)
                     : tuple.Item2.Find(x => x[0] == Course_ID || x[1] == Course_ID).ElementAt(0);
                return $" AND  ARGCoursesApplied{prgPostFix}.Student_Id Not in (SELECT Student_ID FROM dbo.ARGCoursesApplied{prgPostFix} WHERE Course_ID ='{oppcourse}' AND StudentSelectionStatus  IN ({(int)StudentSelectionStatus.Joined},{(int)StudentSelectionStatus.Provisional})) ";

            }

            return string.Empty;
        }



        internal SqlCommand GetTieList(decimal? totalPoints, string CategoryClause, Parameters parameter, string inClause)
        {
            SearchFilter progfilter = parameter.Filters.Where(x => x.Column == "Programme").FirstOrDefault();
            string programme = progfilter.Value;
            SearchFilter batchfilter = parameter.Filters.Where(x => x.Column == "Batch").FirstOrDefault();
            string batch = batchfilter.Value;
            SearchFilter coursefilter = parameter.Filters.Where(x => x.Column.ToLower() == "course_id").FirstOrDefault();
            string course = coursefilter.Value;
            string examname = "12TH";
            Programme programmeType;
            Enum.TryParse<Programme>(programme, out programmeType);
            if (programmeType == Programme.PG || course == "FC32E138-4EE2-4DA2-9453-5C8368180BC3")
            {
                examname = "Post-Graduation";
            }
            string prgPostFix = new GeneralFunctions().GetProgrammePostFix(programmeType);
            string query = $@" SELECT Distinct PINFO.Student_ID,PINFO.FullName,PINFO.IsProvisional,PINFO.FathersName,
                     CAST('{course}' AS UNIQUEIDENTIFIER) AS    Course_ID,
                           1 isTie,";



            if (!string.IsNullOrEmpty(GetAdditionalCourse(course)))
            {
                query = query + $@" (SELECT TOP 1 StudentSelectionStatus FROM ARGCoursesApplied{prgPostFix} ca WHERE ca.Student_ID=Student_ID AND ca.Course_ID ='{course}' ) StudentSelectionStatus,";
            }
            else
            {
                query = query + "StudentSelectionStatus,";
            }
            query = query + $@" 
                                   Category,EntranceRollNo,ISNULL(CAST(CAST(((CAST(((MarksObt/MaxMarks)*100) AS DECIMAL(5,2))*40)/100) AS DECIMAL(5,2)) AS VARCHAR(10)) ,0)AcademicPoints,ISNULL(CAST(CAST(((CAST(((MarksObt/MaxMarks)*100) AS DECIMAL(5,2))*40)/100) AS DECIMAL(5,2)) AS VARCHAR(10)),0)+SubjectEntrancePoints TotalPoints,SubjectEntrancePoints,(MarksObt/MaxMarks*100) Percentage FROM ARGPersonalInformation{prgPostFix} PINFO
                                JOIN ARGCoursesApplied{prgPostFix}
                                ON ARGCoursesApplied{prgPostFix}.Student_ID =PINFO.Student_ID
                                JOIN ADMCourseMaster
                                ON ADMCourseMaster.Course_ID = ARGCoursesApplied{prgPostFix}.Course_ID
                               Left JOIN ARGStudentPreviousQualifications_IH ON ARGStudentPreviousQualifications_IH.Student_ID = PINFO.Student_ID AND ARGStudentPreviousQualifications_IH.ExamName='{examname}'
                                WHERE PINFO.Batch={batch} AND AppearedInEntrance=1 AND IsProvisional=0 ";
            if (!string.IsNullOrEmpty(GetAdditionalCourse(course)))
            {
                query = query + GetExtraClause(course, GetAdditionalCourse(course), prgPostFix);
            }
            else
            {
                query = query + $@"   AND ARGCoursesApplied{prgPostFix}.course_Id='{course}' ";
            }
            if (programmeType == Programme.IG)
            {
                if (IsIntegratedCourse(course))
                {
                    query = query + $@"    AND ARGCoursesApplied{prgPostFix}.Preference = 1 ";
                }
                else
                {
                    query = query + GetHonorsClause(course, prgPostFix);
                }
            }
            query = query + $@"   AND ISNULL(CAST(CAST(((CAST(((MarksObt/MaxMarks)*100) AS DECIMAL(5,2))*40)/100) AS DECIMAL(5,2)) AS VARCHAR(10)),0)+SubjectEntrancePoints={totalPoints}  AND StudentSelectionStatus!={(int)StudentSelectionStatus.Rejected}";
            if (inClause != string.Empty)
            { query = query + $@"  AND PINFO.Student_ID" + inClause; }
            query = query + $"  AND Category {CategoryClause} ";
            SqlCommand command = new SqlCommand();
            command.CommandText = query + "  ORDER BY ISNULL(CAST(CAST(((CAST(((MarksObt/MaxMarks)*100) AS DECIMAL(5,2))*40)/100) AS DECIMAL(5,2)) AS VARCHAR(10)),0)+SubjectEntrancePoints DESC,SubjectEntrancePoints DESC,(MarksObt/MaxMarks*100) DESC"; //AND AcceptCollege_ID IS  NULL
            return command;
        }

        internal SqlCommand GetSelectedStudentsNEP(decimal count, string CategoryClause, Guid Course_ID, string inClause, int preference)
        {
            //SearchFilter progfilter = parameter.Filters.Where(x => x.Column == "Programme").FirstOrDefault();
            //string programme = progfilter.Value;
            //SearchFilter batchfilter = parameter.Filters.Where(x => x.Column == "Batch").FirstOrDefault();
            //string batch = batchfilter.Value;
            //SearchFilter coursefilter = parameter.Filters.Where(x => x.Column.ToLower() == "course_id").FirstOrDefault();
            //string course = coursefilter.Value;
            int batch = 2022;
            string course = Course_ID.ToString();
            string examname = "12TH";
            //Programme programmeType;
            //Enum.TryParse<Programme>(programme, out programmeType);
            //if (programmeType == Programme.PG || course == "FC32E138-4EE2-4DA2-9453-5C8368180BC3")
            //{
            //    examname = "Post-Graduation";
            //}
            string prgPostFix = "_" + PrintProgramme.IH.GetTablePFix();
            string query = $@" SELECT Distinct TOP {count} PINFO.Student_ID,PINFO.IsProvisional,PINFO.FullName,PINFO.FathersName,
                  CAST('{course}' AS UNIQUEIDENTIFIER) AS    Course_ID, ";



            if (!string.IsNullOrEmpty(GetAdditionalCourse(course)))
            {
                query = query + $@" (SELECT TOP 1 StudentSelectionStatus FROM ARGCoursesApplied{prgPostFix} ca WHERE ca.Student_ID=Student_ID AND ca.Course_ID ='{course}' ) StudentSelectionStatus,";
            }
            else
            {
                query = query + "StudentSelectionStatus,";
            }
            query = query + $@"  Category,EntranceRollNo,ISNULL(CAST(CAST(((CAST(((MarksObt/MaxMarks)*100) AS DECIMAL(5,2))*40)/100) AS DECIMAL(5,2)) AS VARCHAR(10)),0)+SubjectEntrancePoints TotalPoints,SubjectEntrancePoints,CAST(CAST(((CAST(((MarksObt/MaxMarks)*100) AS DECIMAL(5,2))*40)/100) AS DECIMAL(5,2)) AS VARCHAR(10)) AcademicPoints,(MarksObt/MaxMarks*100) Percentage FROM ARGPersonalInformation{prgPostFix} PINFO
                                JOIN ARGCoursesApplied{prgPostFix}
                                ON ARGCoursesApplied{prgPostFix}.Student_ID =PINFO.Student_ID
                                JOIN ADMCourseMaster
                                ON ADMCourseMaster.Course_ID = ARGCoursesApplied{prgPostFix}.Course_ID
                                Left JOIN ARGStudentPreviousQualifications{prgPostFix} ON ARGStudentPreviousQualifications{prgPostFix}.Student_ID = PINFO.Student_ID AND ARGStudentPreviousQualifications{prgPostFix}.ExamName='{examname}'
                               WHERE PINFO.Batch={batch} AND ARGCoursesApplied{prgPostFix}.Preference=1 AND IsProvisional=0  ";
            if (!string.IsNullOrEmpty(GetAdditionalCourse(course)))
            {
                query = query + GetExtraClause(course, GetAdditionalCourse(course), prgPostFix);
            }
            else
            {
                query = query + $@"   AND ARGCoursesApplied{prgPostFix}.course_Id='{course}' ";
            }
            //if (programmeType == Programme.IG)
            //{
            //    if (IsIntegratedCourse(course))
            //    {
            //        query = query + $@"    AND ARGCoursesApplied{prgPostFix}.Preference = 1 ";
            //    }
            //    else
            //    {
            //        query = query + GetHonorsClause(course, prgPostFix);
            //    }
            //}
            query = query + $@" AND StudentSelectionStatus!={(int)StudentSelectionStatus.Rejected}";
            if (inClause != string.Empty)
            { query = query + $@"  AND PINFO.Student_ID" + inClause; }
            query = query + $"  AND Category {CategoryClause} ";
            SqlCommand command = new SqlCommand();
            command.CommandText = query + "  ORDER BY ISNULL(CAST(CAST(((CAST(((MarksObt/MaxMarks)*100) AS DECIMAL(5,2))*40)/100) AS DECIMAL(5,2)) AS VARCHAR(10)),0)+SubjectEntrancePoints DESC,SubjectEntrancePoints DESC,(MarksObt/MaxMarks*100) DESC"; //AND AcceptCollege_ID IS  NULL
            return command;
        }
        internal SqlCommand GetStudentsWithNextPrefNEP(string CategoryClause, Guid course_ID, string inClause, int preference, decimal percentage)
        {
            string programme = PrintProgramme.IH.ToString();
            string batch = "2022";
            string course = course_ID.ToString();
            string examname = "12TH";
            Programme programmeType;
            Enum.TryParse<Programme>(programme, out programmeType);
            if (programmeType == Programme.PG || course == "FC32E138-4EE2-4DA2-9453-5C8368180BC3")
            {
                examname = "Post-Graduation";
            }
            string prgPostFix = new GeneralFunctions().GetProgrammePostFix(programmeType);
            string query = $@" SELECT Distinct TOP 1 PINFO.Student_ID,PINFO.IsProvisional,PINFO.FullName,PINFO.FathersName,
                  CAST('{course}' AS UNIQUEIDENTIFIER) AS    Course_ID, ";



            if (!string.IsNullOrEmpty(GetAdditionalCourse(course)))
            {
                query = query + $@" (SELECT TOP 1 StudentSelectionStatus FROM ARGCoursesApplied{prgPostFix} ca WHERE ca.Student_ID=Student_ID AND ca.Course_ID ='{course}' ) StudentSelectionStatus,";
            }
            else
            {
                query = query + "StudentSelectionStatus,";
            }
            query = query + $@"  Category,EntranceRollNo,ISNULL(CAST(CAST(((CAST(((MarksObt/MaxMarks)*100) AS DECIMAL(5,2))*40)/100) AS DECIMAL(5,2)) AS VARCHAR(10)),0)+SubjectEntrancePoints TotalPoints,SubjectEntrancePoints,CAST(CAST(((CAST(((MarksObt/MaxMarks)*100) AS DECIMAL(5,2))*40)/100) AS DECIMAL(5,2)) AS VARCHAR(10)) AcademicPoints,(MarksObt/MaxMarks*100) Percentage FROM ARGPersonalInformation{prgPostFix} PINFO
                                JOIN ARGCoursesApplied{prgPostFix}
                                ON ARGCoursesApplied{prgPostFix}.Student_ID =PINFO.Student_ID
                                JOIN ADMCourseMaster
                                ON ADMCourseMaster.Course_ID = ARGCoursesApplied{prgPostFix}.Course_ID
                                Left JOIN ARGStudentPreviousQualifications{prgPostFix} ON ARGStudentPreviousQualifications{prgPostFix}.Student_ID = PINFO.Student_ID AND ARGStudentPreviousQualifications{prgPostFix}.ExamName='{examname}'
                               WHERE PINFO.Batch={batch} AND ARGCoursesApplied{prgPostFix}.Preference={preference} AND IsProvisional=0  ";
            if (!string.IsNullOrEmpty(GetAdditionalCourse(course)))
            {
                query = query + GetExtraClause(course, GetAdditionalCourse(course), prgPostFix);
            }
            else
            {
                query = query + $@"   AND ARGCoursesApplied{prgPostFix}.course_Id='{course}' ";
            }
            if (programmeType == Programme.IG)
            {
                if (IsIntegratedCourse(course))
                {
                    query = query + $@"    AND ARGCoursesApplied{prgPostFix}.Preference = 1 ";
                }
                else
                {
                    query = query + GetHonorsClause(course, prgPostFix);
                }
            }
            query = query + $@" AND StudentSelectionStatus!={(int)StudentSelectionStatus.Rejected}";
            if (inClause != string.Empty)
            { query = query + $@"  AND PINFO.Student_ID" + inClause; }
            query = query + $"  AND Category {CategoryClause} AND (MarksObt/MaxMarks*100)>{percentage} ";
            SqlCommand command = new SqlCommand();
            command.CommandText = query + "  ORDER BY ISNULL(CAST(CAST(((CAST(((MarksObt/MaxMarks)*100) AS DECIMAL(5,2))*40)/100) AS DECIMAL(5,2)) AS VARCHAR(10)),0)+SubjectEntrancePoints DESC,SubjectEntrancePoints DESC,(MarksObt/MaxMarks*100) DESC"; //AND AcceptCollege_ID IS  NULL
            return command;
        }

        internal SqlCommand GetSelectedStudents(decimal count, string CategoryClause, Parameters parameter, string inClause)
        {
            SearchFilter progfilter = parameter.Filters.Where(x => x.Column == "Programme").FirstOrDefault();
            string programme = progfilter.Value;
            SearchFilter batchfilter = parameter.Filters.Where(x => x.Column == "Batch").FirstOrDefault();
            string batch = batchfilter.Value;
            SearchFilter coursefilter = parameter.Filters.Where(x => x.Column.ToLower() == "course_id").FirstOrDefault();
            string course = coursefilter.Value;
            string examname = "12TH";
            Programme programmeType;
            Enum.TryParse<Programme>(programme, out programmeType);
            if (programmeType == Programme.PG || course == "FC32E138-4EE2-4DA2-9453-5C8368180BC3")
            {
                examname = "Post-Graduation";
            }
            string prgPostFix = new GeneralFunctions().GetProgrammePostFix(programmeType);
            string query = $@" SELECT Distinct TOP {count} PINFO.Student_ID,PINFO.IsProvisional,PINFO.FullName,PINFO.FathersName,
                  CAST('{course}' AS UNIQUEIDENTIFIER) AS    Course_ID, ";



            if (!string.IsNullOrEmpty(GetAdditionalCourse(course)))
            {
                query = query + $@" (SELECT TOP 1 StudentSelectionStatus FROM ARGCoursesApplied{prgPostFix} ca WHERE ca.Student_ID=Student_ID AND ca.Course_ID ='{course}' ) StudentSelectionStatus,";
            }
            else
            {
                query = query + "StudentSelectionStatus,";
            }
            query = query + $@"  Category,EntranceRollNo,ISNULL(CAST(CAST(((CAST(((MarksObt/MaxMarks)*100) AS DECIMAL(5,2))*40)/100) AS DECIMAL(5,2)) AS VARCHAR(10)),0)+SubjectEntrancePoints TotalPoints,SubjectEntrancePoints,CAST(CAST(((CAST(((MarksObt/MaxMarks)*100) AS DECIMAL(5,2))*40)/100) AS DECIMAL(5,2)) AS VARCHAR(10)) AcademicPoints,(MarksObt/MaxMarks*100) Percentage FROM ARGPersonalInformation{prgPostFix} PINFO
                                JOIN ARGCoursesApplied{prgPostFix}
                                ON ARGCoursesApplied{prgPostFix}.Student_ID =PINFO.Student_ID
                                JOIN ADMCourseMaster
                                ON ADMCourseMaster.Course_ID = ARGCoursesApplied{prgPostFix}.Course_ID
                                Left JOIN ARGStudentPreviousQualifications{prgPostFix} ON ARGStudentPreviousQualifications{prgPostFix}.Student_ID = PINFO.Student_ID AND ARGStudentPreviousQualifications{prgPostFix}.ExamName='{examname}'
                               WHERE PINFO.Batch={batch} AND AppearedInEntrance=1 AND IsProvisional=0  ";
            if (!string.IsNullOrEmpty(GetAdditionalCourse(course)))
            {
                query = query + GetExtraClause(course, GetAdditionalCourse(course), prgPostFix);
            }
            else
            {
                query = query + $@"   AND ARGCoursesApplied{prgPostFix}.course_Id='{course}' ";
            }
            if (programmeType == Programme.IG)
            {
                if (IsIntegratedCourse(course))
                {
                    query = query + $@"    AND ARGCoursesApplied{prgPostFix}.Preference = 1 ";
                }
                else
                {
                    query = query + GetHonorsClause(course, prgPostFix);
                }
            }
            query = query + $@" AND StudentSelectionStatus!={(int)StudentSelectionStatus.Rejected}";
            if (inClause != string.Empty)
            { query = query + $@"  AND PINFO.Student_ID" + inClause; }
            query = query + $"  AND Category {CategoryClause} ";
            SqlCommand command = new SqlCommand();
            command.CommandText = query + "  ORDER BY ISNULL(CAST(CAST(((CAST(((MarksObt/MaxMarks)*100) AS DECIMAL(5,2))*40)/100) AS DECIMAL(5,2)) AS VARCHAR(10)),0)+SubjectEntrancePoints DESC,SubjectEntrancePoints DESC,(MarksObt/MaxMarks*100) DESC"; //AND AcceptCollege_ID IS  NULL
            return command;
        }

        private bool IsIntegratedCourse(string course)
        {
            ADMCourseMaster aDMCourseMaster = new CourseDB().GetItem(Guid.Parse(course));
            if (aDMCourseMaster.CourseFullName.Contains("Integrated"))
                return true;
            else
                return false;
        }

        internal SqlCommand GetFreeSeatsCount(string course, string batch, string programme)
        {
            Programme programmeType;
            Enum.TryParse<Programme>(programme, out programmeType);
            string prgPostFix = new GeneralFunctions().GetProgrammePostFix(programmeType);
            SqlCommand command = new SqlCommand();
            string query = $@"SELECT COUNT(Distinct ARGCoursesApplied{prgPostFix}.Student_ID) FROM ARGCoursesApplied{prgPostFix}
                                    JOIN ARGPersonalInformation{prgPostFix}
                                    ON  ARGPersonalInformation{prgPostFix}.Student_ID = ARGCoursesApplied{prgPostFix}.Student_ID
                                    WHERE 1=1";
            if (!string.IsNullOrEmpty(GetAdditionalCourse(course)))
            {
                query = query + GetExtraClause(course, GetAdditionalCourse(course), prgPostFix);
            }
            else
            {
                query = query + $@"   AND ARGCoursesApplied{prgPostFix}.course_Id='{course}' ";
            }
            if (programmeType == Programme.IG)
            {
                if (IsIntegratedCourse(course))
                {
                    query = query + $@"    AND ARGCoursesApplied{prgPostFix}.Preference =1 ";
                }
                else
                {
                    query = query + GetHonorsClause(course, prgPostFix);
                }
            }
            query = query + $@" AND Batch={batch} AND AppearedInEntrance=1  AND IsProvisional=0    
                                   AND StudentSelectionStatus={(int)StudentSelectionStatus.Joined}  ";
            command.CommandText = query;
            return command;
        }

        //        internal SqlCommand GetAlreadySelectedStudents(string programme, string batch, string course)
        //        {
        //            Programme programmeType;
        //            Enum.TryParse<Programme>(programme, out programmeType);
        //            string prgPostFix = new GeneralFunctions().GetProgrammePostFix(programmeType);
        //            SqlCommand command = new SqlCommand();
        //            string query = $@"SELECT PINFO.Student_ID,CollegeFullName,PINFO.FullName,PINFO.FathersName,CATEntrancePoints,'{course}' Course_ID,
        //ADMCourseMaster.CourseFullName,StudentSelectionStatus
        //                            ,VWStudentCourse.CourseFullName AlreadyCourse,
        //                               CASE 
        //                                WHEN StudentSelectionStatus = 0 
        //                                   THEN 1 
        //                                   ELSE 0 
        //                                    END as  isNew,";



        //            if (!string.IsNullOrEmpty(GetAdditionalCourse(course)))
        //            {
        //                query = query + $@" (SELECT TOP 1 StudentSelectionStatus FROM ARGCoursesApplied_IH ca WHERE ca.Student_ID=Student_ID AND ca.Course_ID ='{course}' ) StudentSelectionStatus,";
        //            }
        //            else
        //            {
        //                query = query + "StudentSelectionStatus,";
        //            }
        //            query = query + $@"  Category,EntranceRollNo,CATEntrancePoints+SubjectEntrancePoints TotalPoints,SubjectEntrancePoints  FROM ARGCoursesApplied{prgPostFix}
        //                                    JOIN ARGPersonalInformation{prgPostFix} PINFO
        //                                    ON  PINFO.Student_ID = ARGCoursesApplied{prgPostFix}.Student_ID
        //                                    JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = ARGCoursesApplied{prgPostFix}.Course_ID
        //                                       Left JOIN VWStudentCourse ON VWStudentCourse.Student_ID = PINFO.Student_ID
        //                                   WHERE 1=1 ";
        //            if (!string.IsNullOrEmpty(GetAdditionalCourse(course)))
        //            {
        //                query = query + GetExtraClause(course, GetAdditionalCourse(course),prgPostFix);
        //            }
        //            else
        //            {
        //                query = query + $@" AND  Course_ID='{course}'  ";
        //            }
        //            query = query + GetHonorsClause(course, prgPostFix);
        //            query = query + $@" AND PINFO.Batch={batch} AND AppearedInEntrance=1  AND IsProvisional=0 
        //                                   AND StudentSelectionStatus={(int)StudentSelectionStatus.Joined}  ";
        //            command.CommandText = query + "  ORDER BY CATEntrancePoints+SubjectEntrancePoints DESC,SubjectEntrancePoints DESC"; //AND AcceptCollege_ID IS  NULL
        //            return command;
        //        }

        internal SqlCommand ExistStudentForCategory(string categoryCode, Parameters parameter, string insubClause)
        {
            SearchFilter progfilter = parameter.Filters.Where(x => x.Column == "Programme").FirstOrDefault();
            string programme = progfilter.Value;
            SearchFilter batchfilter = parameter.Filters.Where(x => x.Column == "Batch").FirstOrDefault();
            string batch = batchfilter.Value;
            SearchFilter coursefilter = parameter.Filters.Where(x => x.Column.ToLower() == "course_id").FirstOrDefault();
            string course = coursefilter.Value;
            Programme programmeType;
            Enum.TryParse<Programme>(programme, out programmeType);
            string prgPostFix = new GeneralFunctions().GetProgrammePostFix(programmeType);
            SqlCommand command = new SqlCommand();
            string query = $@"SELECT COUNT(Distinct ARGCoursesApplied{prgPostFix}.Student_ID) FROM ARGCoursesApplied{prgPostFix}
                                    JOIN ARGPersonalInformation{prgPostFix}
                                    ON  ARGPersonalInformation{prgPostFix}.Student_ID = ARGCoursesApplied{prgPostFix}.Student_ID
                                    WHERE 1=1 ";
            if (!string.IsNullOrEmpty(GetAdditionalCourse(course)))
            {
                query = query + GetExtraClause(course, GetAdditionalCourse(course), prgPostFix);
            }
            else
            {
                query = query + $@"  AND  Course_ID='{course}'  ";
            }
            if (programmeType == Programme.IG)
            {
                if (IsIntegratedCourse(course))
                {
                    query = query + $@"    AND ARGCoursesApplied{prgPostFix}.Preference = 1";
                }
                else
                {
                    query = query + GetHonorsClause(course, prgPostFix);
                }
            }
            query = query + $@" AND Batch={batch}  AND AppearedInEntrance=1   AND IsProvisional=0  
                                    AND Category='{categoryCode}' AND StudentSelectionStatus!={(int)StudentSelectionStatus.Rejected}  "; //AND AcceptCollege_ID IS  NULL
            if (insubClause != string.Empty)
            { query = query + $@"  AND ARGPersonalInformation{prgPostFix}.Student_ID" + insubClause; }
            command.CommandText = query;
            return command;
        }

        internal SqlCommand GetAdmissionSelectionCriteria()
        {
            SqlCommand command = new SqlCommand();
            command.CommandText = $@" SELECT * FROM AdmissionSelectionCriteria";
            return command;
        }


        internal SqlCommand GetStudentAdmissionFormHistory(Guid student_Id, PrintProgramme printProgramme)
        {
            string postfix = new GeneralFunctions().GetProgrammePostFix(printProgramme);
            SqlCommand command = new SqlCommand();
            command.CommandText = $@" SELECT AdmissionDetail.Student_ID,AdmissionDetail.Semester,AppliedOn,ModuleType,Amount,PaymentOn,SemesterBatch  FROM 
                                        (
                                         SELECT Distinct Student_ID,sc.SemesterBatch,sc.Semester,ISNULL(sc.UpdatedOn,sc.CreatedOn) AS AppliedOn,ModuleType,TxnAmount as Amount,TxnDate as PaymentOn FROM ARGSelectedCombination{postfix} sc
										JOIN VWSCMaster
										ON VWSCMaster.Combination_ID = sc.Combination_ID
										LEFT JOIN PaymentDetails{postfix}
										ON Entity_ID=Student_ID AND ModuleType={(int)PaymentModuleType.SemesterAdmission} AND PaymentDetails{postfix}.Semester = sc.Semester
                                        WHERE Student_ID=@student_ID 
                                        )AdmissionDetail Order by Semester";
            command.Parameters.AddWithValue("@student_ID", student_Id);
            return command;
        }
        internal SqlCommand GetStudentSemesters(Guid student_Id, PrintProgramme printProgramme)
        {
            string postfix = new GeneralFunctions().GetProgrammePostFix(printProgramme);
            SqlCommand command = new SqlCommand();
            command.CommandText = $@" SELECT DISTINCT(Semester) PSemester FROM ARGStudentExamForm{postfix}
                                        WHERE Status = 4 AND Student_ID = @student_ID
                                        ORDER BY Semester";
            command.Parameters.AddWithValue("@student_ID", student_Id);
            return command;
        }


        internal SqlCommand GetStudentExaminationHistory(Guid student_Id, PrintProgramme printProgramme, Semester semester)
        {
            string postfix = new GeneralFunctions().GetProgrammePostFix(printProgramme);
            SqlCommand command = new SqlCommand();
            command.CommandText = $@"SELECT ROW_NUMBER() OVER (ORDER BY Year) AS ExamFormNo,StudentExamForm_ID,Student_ID,ExamRollNumber As ExamRollNo,FormNumber,Semester,Year,AmountPaid,CreatedOn SubmittedOn
                                     FROM ARGStudentExamForm{postfix}
                                     WHERE Status=4 AND  Student_ID=@student_ID AND Semester=@Semester";
            command.Parameters.AddWithValue("@student_ID", student_Id);
            command.Parameters.AddWithValue("@Semester", semester);
            return command;
        }
        internal SqlCommand GetStudentRecentExaminationForm(Guid student_Id, PrintProgramme printProgramme)
        {
            string postfix = new GeneralFunctions().GetProgrammePostFix(printProgramme);
            SqlCommand command = new SqlCommand();
            command.CommandText = $@"SELECT TOP 1 StudentExamForm_ID,Student_ID,ExamRollNumber As ExamRollNo,FormNumber,Semester,Year,AmountPaid,CreatedOn SubmittedOn
                                     FROM ARGStudentExamForm{postfix}
                                     WHERE ExamRollNumber IS NOT NULL AND  Student_ID=@student_ID ORDER BY CreatedOn  desc";
            command.Parameters.AddWithValue("@student_ID", student_Id);
            return command;
        }
        internal SqlCommand GetStudentAllSubjects(Guid student_Id, PrintProgramme printProgramme, Semester semester)
        {
            string postfix = new GeneralFunctions().GetProgrammePostFix(printProgramme);
            SqlCommand command = new SqlCommand();
            command.CommandText = $@"SELECT Subject_ID FROM ARGSelectedCombination{postfix}
                                    JOIN VWSCMaster ON VWSCMaster.Combination_ID = ARGSelectedCombination{postfix}.Combination_ID
                                    AND VWSCMaster.semester = @Semester AND Student_ID = @student_ID
                                    union
                                    SELECT Subject_ID FROM ARGStudentAdditionalSubjects{postfix}
                                    WHERE semester = @Semester AND Student_ID = @student_ID";
            command.Parameters.AddWithValue("@student_ID", student_Id);
            command.Parameters.AddWithValue("@Semester", semester);
            return command;
        }

        internal SqlCommand GetStudentResult(Guid student_Id, PrintProgramme printProgramme, Semester semester, string join, int ExamFormNo, Guid Subject_ID, bool checkresultdeclared)
        {
            string postfix = new GeneralFunctions().GetProgrammePostFix(printProgramme);
            string ResultTable = new GeneralFunctions().GetTableName(((int)semester).ToString(), printProgramme);
            SqlCommand command = new SqlCommand();
            string query = string.Empty;
            if (checkresultdeclared)
            {
                query = $" AND {ResultTable}.ResultNotification_ID IS NOT NULL ";
            }
            if (join.Equals(string.Empty))
            {
                command.CommandText = $@"SELECT  SubjectFullName ,SubjectType,IsInternalPassed,IsExternalPassed,
                                    {ResultTable}.ExternalMarks , {ResultTable}.ExternalAttendance_AssessmentMarks ,
                                    {ResultTable}.InternalAttendance_AssessmentMarks , {ResultTable}.InternalMarks 
                                    FROM {ResultTable} JOIN  VW_SubjectWithStructure S
                                    ON S.Subject_ID = {ResultTable}.Subject_ID
                                    WHERE Student_ID =   @student_ID  and  {ResultTable}.Subject_ID = @Subject_ID {query}";
            }
            else
            {
                command.CommandText = $@"SELECT * FROM(
                                    SELECT ROW_NUMBER() OVER (ORDER BY S.CreatedOn) AS ExamFormNo,SubjectFullName ,SubjectType,
                                    {ResultTable}.ExternalMarks FinalExternalMarks, {ResultTable}.ExternalAttendance_AssessmentMarks FinalExternalAttendance_AssessmentMarks,
                                    {ResultTable}.InternalAttendance_AssessmentMarks FinalInternalAttendance_AssessmentMarks, {ResultTable}.InternalMarks FinalInternalMarks,
                                    ResultHistory.ExternalMarks ,ResultHistory.ExternalAttendance_AssessmentMarks,ResultHistory.InternalAttendance_AssessmentMarks,ResultHistory.InternalMarks
                                    FROM {ResultTable} JOIN  VW_SubjectWithStructure S
                                    ON S.Subject_ID = {ResultTable}.Subject_ID
                                    {join} ResultHistory ON Semester_ID = _ID
                                    WHERE Student_ID =   @student_ID  and  {ResultTable}.Subject_ID = @Subject_ID {query}
                                    ) Result WHERE Result.ExamFormNo=@ExamFormNo";
                command.Parameters.AddWithValue("@ExamFormNo", ExamFormNo);
            }
            command.Parameters.AddWithValue("@student_ID", student_Id);
            command.Parameters.AddWithValue("@Subject_ID", Subject_ID);

            return command;
        }

        internal string GetStudentFailedAdditionalSubjects(Guid student_ID, short semester, PrintProgramme printProgramme)
        {
            return $@"SELECT 
                                ExamForm.StudentExamForm_ID,ExamForm.StudentReExamForm_ID,ExamForm.Subject_ID,SubjectFullName,SubjectType,SubjectCode,ExamForm.IsApplied,ExamForm.FeeStatus,Result.ExternalMarks,Result.InternalAttendance_AssessmentMarks,Result.ExternalAttendance_AssessmentMarks,result.InternalMarks
                                FROM ARGStudentAdditionalSubjects_{printProgramme.ToString()}
                                    INNER JOIN VW_SubjectWithStructure S
                                        ON S.Subject_ID = ARGStudentAdditionalSubjects_{printProgramme.ToString()}.Subject_ID 
                                        AND S.Semester = ARGStudentAdditionalSubjects_{printProgramme.ToString()}.Semester 
                                    LEFT JOIN
                                    (
                                        SELECT Student_ID, ARGStudentExamForm_{printProgramme.ToString()}.StudentExamForm_ID, Subject_ID, StudentReExamForm_ID, FeeStatus , CASE WHEN FeeStatus = 10 THEN 1 ELSE 0 END IsApplied,Semester
                                        FROM ARGStudentExamForm_{printProgramme.ToString()}
                                            INNER JOIN ARGStudentReExamForm
                                                ON ARGStudentReExamForm.StudentExamForm_ID = ARGStudentExamForm_{printProgramme.ToString()}.StudentExamForm_ID
                                    ) ExamForm
                                        ON ExamForm.Student_ID = ARGStudentAdditionalSubjects_{printProgramme.ToString()}.Student_ID
                                           AND ExamForm.Subject_ID = ARGStudentAdditionalSubjects_{printProgramme.ToString()}.Subject_ID
                                    LEFT JOIN UG_Semester2 Result
                                        ON Result.Student_ID = ARGStudentAdditionalSubjects_{printProgramme.ToString()}.Student_ID
                                           AND Result.Subject_ID = ARGStudentAdditionalSubjects_{printProgramme.ToString()}.Subject_ID 
                                        WHERE ARGStudentAdditionalSubjects_{printProgramme.ToString()}.Student_ID = '{student_ID}' AND ARGStudentAdditionalSubjects_{printProgramme.ToString()}.Semester = {semester}  ";

        }

        internal SqlCommand StudentExists(Guid student_ID)
        {
            SqlCommand command = new SqlCommand();
            command.CommandText = @" SELECT Batch
                                            FROM
                                            (
                                               SELECT Batch,
                                                       Student_ID
                                                FROM ARGPersonalInformation_IH
                                                UNION
                                                SELECT Batch,
                                                       Student_ID
                                                FROM ARGPersonalInformation_UG
                                                UNION
                                                SELECT Batch,
                                                       Student_ID
                                                FROM ARGPersonalInformation_PG
                                            ) AllStudents WHERE Student_ID = @student_ID ";
            command.Parameters.AddWithValue("@student_ID", student_ID);
            return command;
        }

        internal SqlCommand GetRegistrationNumberFromStudentIdentifier(Guid student_ID)
        {
            SqlCommand command = new SqlCommand();
            command.CommandText = $"SELECT CUSRegistrationNo FROM ARGPersonalInformation_{AppUserHelper.TableSuffix.ToString()} WHERE Student_ID = @student_ID ";

            command.Parameters.AddWithValue("@student_ID", student_ID);

            return command;
        }

        internal SqlCommand GetStudentAdditionalSubjects(Guid Student_ID, short semester, PrintProgramme printProgramme)
        {
            SqlCommand command = new SqlCommand();
            command.CommandText = $@"SELECT * FROM ARGStudentAdditionalSubjects_{printProgramme.ToString()} WHERE Student_ID=@Student_ID AND Semester=@Semester";
            command.Parameters.AddWithValue("@Student_ID", Student_ID);
            command.Parameters.AddWithValue("@Semester", semester);
            return command;
        }

        internal SqlCommand GetStudentAdditionalSubjectsDetails(Guid Student_ID, short semester, PrintProgramme printProgramme)
        {
            SqlCommand command = new SqlCommand();
            command.CommandText = $@"SELECT DISTINCT a.Subject_ID, sm.SubjectFullName, sm.SubjectType FROM dbo.ARGStudentAdditionalSubjects_{printProgramme} a
                                    JOIN dbo.ADMSubjectMaster sm ON sm.Subject_ID = a.Subject_ID
                                    WHERE a.Student_ID=@Student_ID AND a.Semester=@Semester";
            command.Parameters.AddWithValue("@Student_ID", Student_ID);
            command.Parameters.AddWithValue("@Semester", semester);
            return command;

        }

        internal SqlCommand GetResultReportList(Parameters parameter)
        {
            //string query = @" SELECT DISTINCT TempRegistration.* FROM TempRegistration LEFT JOIN UG_Semester1 ON UG_Semester1.RegistrationNo = TempRegistration.RegistrationNo ";
            string query = @" SELECT DISTINCT CAST( '00000000-0000-0000-0000-000000000000' as uniqueidentifier)  as Student_ID,[Name],FathersName,TempRegistration.RegistrationNo AS RegistrationNumber 
                                ,DOB AS DateofBirth,MobileNo AS MobileNumber,EMAIL,CASE WHEN GENDER='F' THEN 2 ELSE 1 END GENDER,SubjCombCode AS CombinationCode,RollNo as RollNumber
                                ,ClgCode AS CollegeCode,CAST( '00000000-0000-0000-0000-000000000000' as uniqueidentifier)  AS Course_ID,CourseCode
                                FROM TempRegistration
                                LEFT JOIN UG_Semester1 ON UG_Semester1.RegistrationNo = TempRegistration.RegistrationNo  ";
            SqlCommand command = new SqlCommand();
            FilterHelper helper = new GeneralFunctions().GetWhereClause<StudentProfile>(parameter.Filters);
            query += helper.WhereClause;
            query += new GeneralFunctions().GetPagedQuery(query, parameter);
            helper.Command.CommandText = query;
            return helper.Command;
        }

        internal SqlCommand GetSemesterByRegistrationNoAndSubject(Guid subject_ID, string registrationNo, Guid StudentId, string semesterName, Programme programme, string SubjectIdCol)
        {
            string tableName = new GeneralFunctions().GetSemesterTableName(semesterName, programme);
            SqlCommand command = new SqlCommand();
            string query = $@"SELECT * FROM [{tableName}]";
            query += " WHERE Student_Id = @StudentId";
            query += $" AND {SubjectIdCol}=@SubjectID";
            command.CommandText = query;
            //if (registrationNo != null) { command.Parameters.AddWithValue("@RegistrationNumber", registrationNo); }
            //else
            { command.Parameters.AddWithValue("@StudentId", StudentId); }
            command.Parameters.AddWithValue("@SubjectID", subject_ID);
            return command;
        }

        internal SqlCommand GetStudentSubjects(Guid student_ID, PrintProgramme printProgramme, Semester semester)
        {
            string postfix = new GeneralFunctions().GetProgrammePostFix(printProgramme);
            SqlCommand command = new SqlCommand();
            command.CommandText = $@"SELECT SubjectFullName,Subject_ID,SubjectType FROM
                                        (SELECT VWSCMaster.SubjectFullName,
                                                VWSCMaster.Subject_ID,
                                                VWSCMaster.SubjectType 
                                        FROM  ARGSelectedCombination{postfix}
                                        JOIN VWSCMaster
                                        ON VWSCMaster.Combination_ID = ARGSelectedCombination{postfix}.Combination_ID
                                        WHERE Student_ID=@student_ID AND ARGSelectedCombination{postfix}.Semester=@semester
                                        union
                                        SELECT S.SubjectFullName,
                                               S.Subject_ID,
                                               S.SubjectType
                                               FROM ARGStudentAdditionalSubjects{postfix}
                                        JOIN VW_SubjectWithStructure S
                                        ON S.Subject_ID = ARGStudentAdditionalSubjects{postfix}.Subject_ID
                                        WHERE Student_ID=@student_ID AND ARGStudentAdditionalSubjects{postfix}.Semester=@semester
                                        ) Subjects ORDER BY Subjects.SubjectType DESC,Subjects.SubjectFullName;";
            command.Parameters.AddWithValue("@student_ID", student_ID);
            command.Parameters.AddWithValue("@semester", (int)semester);
            return command;
        }


        public static string GetResultQueryBySemester(string SemesterName)
        {
            string query = $@"  SELECT {SemesterName}.*
                                                       ,ISNULL((SELECT [FullName] FROM [AppUsers] WHERE [User_ID] = UG_Semester1.UpdatedBy),'') AS UpdatedByUserName
                                                       ,Subjects.Name
                                                       ,MinMarks
                                                       ,MaxMarks
                                                       ,NEWID() AS Course_ID
                                                       ,StudentProfile.RegistrationNumber AS RegistrationNum
                                                    FROM (SELECT
		                                                    SubjectCombinations.Subject1_ID AS Subject_ID
	                                                    FROM SubjectCombinations
	                                                    INNER JOIN Subjects
		                                                    ON SubjectCombinations.Subject1_ID = Subjects.Subject_ID
	                                                    UNION
	                                                    SELECT
		                                                    SubjectCombinations.Subject2_ID AS Subject_ID
	                                                    FROM SubjectCombinations
	                                                    INNER JOIN Subjects
		                                                    ON SubjectCombinations.Subject2_ID = Subjects.Subject_ID
	                                                    UNION
	                                                    SELECT
		                                                    SubjectCombinations.Subject3_ID AS Subject_ID
	                                                    FROM SubjectCombinations
	                                                    INNER JOIN Subjects
		                                                    ON SubjectCombinations.Subject3_ID = Subjects.Subject_ID
	                                                    UNION
	                                                    SELECT
		                                                    SubjectCombinations.Subject4_ID AS Subject_ID
	                                                    FROM SubjectCombinations
	                                                    INNER JOIN Subjects
		                                                    ON SubjectCombinations.Subject2_ID = Subjects.Subject_ID
	                                                    UNION
	                                                    SELECT
		                                                    SubjectCombinations.Subject5_ID AS Subject_ID
	                                                    FROM SubjectCombinations
	                                                    INNER JOIN Subjects
		                                                    ON SubjectCombinations.Subject2_ID = Subjects.Subject_ID
	                                                    UNION
	                                                    SELECT
		                                                    SubjectCombinations.Subject6_ID AS Subject_ID
	                                                    FROM SubjectCombinations

	                                                    INNER JOIN Subjects
		                                                    ON SubjectCombinations.Subject2_ID = Subjects.Subject_ID
	                                                    UNION

	                                                    SELECT
		                                                    SubjectCombinations.Subject7_ID AS Subject_ID
	                                                    FROM SubjectCombinations
	                                                    INNER JOIN Subjects
		                                                    ON SubjectCombinations.Subject2_ID = Subjects.Subject_ID) SubjectCombinations
                                                    INNER JOIN {SemesterName}
	                                                    ON SubjectCombinations.Subject_ID = {SemesterName}.Subject_ID
                                                    LEFT JOIN Subjects
	                                                    ON  {SemesterName}.Subject_ID = Subjects.Subject_ID
                                                    LEFT JOIN StudentProfile
	                                                    ON StudentProfile.RegistrationNumber = {SemesterName}.RegistrationNo ";
            return query;
        }

        internal string GetStudentSubjectAwards(string subjects, string registrationNumber, PrintProgramme programme)
        {
            string query = string.Empty;
            if (programme == PrintProgramme.UG || programme == PrintProgramme.IH)
                query = $" SELECT * FROM UG_Semester1 INNER JOIN Subjects ON Subjects.Subject_ID = UG_Semester1.Subject_ID WHERE Subjects.Subject_ID IN ({subjects}) AND RegistrationNo = '{registrationNumber}' ";
            else if (programme == PrintProgramme.PG)
                query = $" SELECT * FROM PG_Semester1 INNER JOIN VW_SubjectWithStructure S ON S.Subject_ID = PG_Semester1.Subject_ID WHERE S.Subject_ID IN ({subjects}) AND RegistrationNo = '{registrationNumber}' ";
            return query;
        }

        internal string GetStudentSubjectAwards(string registrationNumber, PrintProgramme printProgramme, short semester = 1)
        {
            return $"  SELECT *,SubjectFullName [Name],TheoryMinPassMarks [MinMarks],TheoryMaxMarks [MaxMarks]  FROM {printProgramme.ToString()}_Semester{semester} INNER JOIN VW_SubjectWithStructure S ON {printProgramme.ToString()}_Semester{semester.ToString()}.Subject_ID = S.Subject_ID WHERE RegistrationNo = '{registrationNumber}' ";
        }

        internal string GetStudentSubjectsFromCombination(string combinationCode)
        {
            return $" SELECT Subjects FROM SubjectCombinations WHERE FullCode = '{combinationCode}'";
        }

        internal string GetStudentSubjectsByCombination(string subjects)
        {
            return $" SELECT * FROM Subjects WHERE SUBJECT_ID IN ({subjects}) ";
        }


        internal static SqlCommand GetAllStudentsBySubjectAndCollege(Parameters parameter, string semesterName, Programme programme)
        {
            string tableName = new GeneralFunctions().GetSemesterTableName(semesterName, programme);

            SearchFilter subjectSearchFilter = parameter.Filters.Where(x => x.Column == "Subject_ID").FirstOrDefault();
            var SubjectId = subjectSearchFilter.Value;
            string query = $@" Select StudentProfile.* from ( {GetResultQueryBySemester(tableName)} ) tempSubject

                                                        inner join StudentProfile
                                                        On StudentProfile.RegistrationNumber = tempSubject.RegistrationNo";
            parameter.Filters.Remove(subjectSearchFilter);
            FilterHelper helper = new GeneralFunctions().GetWhereClause<StudentProfile>(parameter.Filters);
            query += helper.WhereClause;
            query += $"AND ClgCode = '{AppUserHelper.CollegeCode}' AND Subject_ID = '{SubjectId}'";
            query += new GeneralFunctions().GetPagedQuery(query, parameter);
            helper.Command.CommandText = query;
            return helper.Command;
        }

        internal string GetStudentSkillEnhancementSubjects(string skillEnhancementSubjects)
        {
            List<Guid> subjects = skillEnhancementSubjects.ToGuidList();
            var inSubjects = subjects.ToIN();
            return $" SELECT * FROM  VW_SubjectWithStructure S WHERE Subject_ID IN ({inSubjects})";
        }

        internal SqlCommand GetAllResultRecords(Parameters parameter)
        {
            string query = " SELECT * FROM STUDENTPROFILE ";
            SqlCommand command = new SqlCommand();
            FilterHelper helper = new GeneralFunctions().GetWhereClause<StudentProfile>(parameter.Filters);
            query += helper.WhereClause;
            query += new GeneralFunctions().GetPagedQuery(query, parameter);
            helper.Command.CommandText = query;
            return helper.Command;
        }

        public SqlCommand GetAllStudentsResult(Parameters parameters)
        {
            string query = " SELECT * FROM STUDENTPROFILE ";

            FilterHelper helper = new GeneralFunctions().GetWhereClause<StudentProfile>(parameters.Filters);

            query += helper.WhereClause;
            query += new GeneralFunctions().GetPagedQuery(query, parameters);

            helper.Command.CommandText = query;

            return helper.Command;
        }

        internal SqlCommand GetStudentRegistrationNumberByRollNumber(string ExamRollNumber)
        {
            SqlCommand command = new SqlCommand();
            command.Parameters.AddWithValue("@ExamRollNumber", ExamRollNumber);
            command.Parameters.AddWithValue("@RegistrationNumber", ExamRollNumber);
            command.CommandText = " SELECT RegistrationNo FROM UG_Semester1 WHERE ExamRollNumber = @ExamRollNumber OR RegistrationNo = @RegistrationNumber ";
            return command;

        }

        internal SqlCommand GetStudentByName(string Name)
        {
            SqlCommand command = new SqlCommand();
            command.Parameters.AddWithValue("@Name", "%" + Name?.Trim() + "%");
            command.CommandText = GetAllStudents + " WHERE Name LIKE @Name";
            return command;
        }

        internal SqlCommand GetStudentByRegistrationNumber(string RegistrationNumber)
        {
            SqlCommand command = new SqlCommand();
            //command.Parameters.AddWithValue("@RegistrationNo", RegistrationNumber.Trim());
            //command.CommandText = GetAllStudents + " WHERE RegistrationNumber = '@RegistrationNo'";
            command.Parameters.AddWithValue("@RegistrationNumber", RegistrationNumber.Trim());
            command.Parameters.AddWithValue("@RollNumber", RegistrationNumber.Trim());
            command.CommandText = @"SELECT Student_ID,
                                               Name,
                                               FathersName,
                                               RegistrationNumber,
                                               RollNumber,
                                               Course_ID,
                                               StudentImage,
                                               DateOfBirth,
                                               Address_ID,
                                               MobileNumber,
                                               StudentProfile.Email,
                                               Gender,
                                               StudentProfile.ClgCode,
                                               CombinationCode,
                                               ClgName AS CollegeCode,
                                               Address,
                                               Phone,
                                               Fax,
                                               Pin,
                                               Colleges.Email,
                                               Website FROM StudentProfile
                                        INNER JOIN Colleges ON Colleges.ClgCode = StudentProfile.ClgCode WHERE RegistrationNumber = @RegistrationNumber OR RollNumber=@RollNumber";
            return command;
        }

        internal SqlCommand GetStudentsByCollege(string CollegeCode, Parameters parameter)
        {
            string query = GetAllStudents + " WHERE StudentProfile.ClgCode = @CollegeCode ";
            SqlCommand command = new SqlCommand();
            command.Parameters.AddWithValue("@CollegeCode", CollegeCode);
            query = new GeneralFunctions().GetPagedQuery(query, parameter);
            command.CommandText = query;
            return command;
        }

        internal string GetSubjectCombinations()
        {
            return "SELECT * FROM SubjectCombinations ";
        }

        internal SqlCommand GetStudentSubjects(string RegistrationNumber)
        {
            SqlCommand command = new SqlCommand();
            command.Parameters.AddWithValue("@RegistrationNumber", RegistrationNumber?.Trim());
            command.CommandText = GetResultQuery + " WHERE StudentProfile.RegistrationNumber = @RegistrationNumber ";
            return command;
        }
        internal SqlCommand GetStudentsBySubjectsAndCollege(string RegistrationNumber)
        {
            SqlCommand command = new SqlCommand();
            command.Parameters.AddWithValue("@RegistrationNumber", RegistrationNumber?.Trim());
            command.CommandText = GetResultQuery + $" WHERE StudentProfile.RegistrationNumber = @RegistrationNumber AND ClgCode ='{AppUserHelper.CollegeCode}'";
            return command;
        }
        internal SqlCommand GetStudentsBySubjectsAndCollege(string RegistrationNumber, string semesterName, Programme programme)
        {
            string tableName = new GeneralFunctions().GetSemesterTableName(semesterName, programme);
            SqlCommand command = new SqlCommand();
            command.Parameters.AddWithValue("@RegistrationNumber", RegistrationNumber?.Trim());
            command.CommandText = GetResultQueryBySemester(tableName) + $" WHERE StudentProfile.RegistrationNumber = @RegistrationNumber AND ClgCode ='{AppUserHelper.CollegeCode}'";
            return command;
        }
        internal SqlCommand GetResultByCollege(string collegeCode)
        {
            SqlCommand command = new SqlCommand();
            command.Parameters.AddWithValue("@CollegeCode", collegeCode);
            command.CommandText = GetResultQuery + " WHERE StudentProfile.ClgCode = @CollegeCode ";
            return command;
        }

        internal SqlCommand GetResultByStudentRegistration(string RegistrationNumber)
        {
            SqlCommand command = new SqlCommand();
            command.Parameters.AddWithValue("@RegistrationNumber", RegistrationNumber.Trim());
            command.CommandText = GetResultQuery + " WHERE StudentProfile.RegistrationNumber = @RegistrationNumber ";
            return command;
        }
        internal SqlCommand DeleteSubjectResult(Guid id)
        {
            SqlCommand command = new SqlCommand();
            command.Parameters.AddWithValue("@ID", id);
            command.CommandText = "Delete From UG_Semester1 " + " WHERE _ID = @ID ";
            return command;
        }
        public SqlCommand GetAllStudentsLibrary(PrintProgramme programme, Parameters parameter)
        {
            string tablePostFix = new GeneralFunctions().GetProgrammePostFix(programme);
            string query = $@"Select * from(SELECT Distinct
                              StudentFormNo,
                              FullName, 
                              FathersName, 
                              MothersName,
                              PINFO.Student_ID, 
                              BoardRegistrationNo,
                              CUSRegistrationNo,
                              Gender, 
                              Category,
                              DOB,
                              Photograph,
                              IsProvisional, 
                              FormStatus,
                              Course_ID,
							   PINFO.AcceptCollege_ID AS College_ID,
                              ARGSelectedCombination{tablePostFix}.Combination_ID,
                              ClassRollNo,
                              ARGSelectedCombination{tablePostFix}.SemesterBatch AS Year,
                              ARGSelectedCombination{tablePostFix}.SemesterBatch,
                              PINFO.CreatedOn,
                              ARGSelectedCombination{tablePostFix}.Semester
                              FROM ARGPERSONALINFORMATION{tablePostFix} PINFO 
                              INNER JOIN ARGSelectedCombination{tablePostFix} 
                              INNER JOIN ADMCombinationMaster ON ADMCombinationMaster.Combination_ID = ARGSelectedCombination{tablePostFix}.Combination_ID
                              ON ARGSelectedCombination{tablePostFix}.Student_ID = PINFO.Student_ID 
                              ) temp";

            SqlCommand command = new SqlCommand();
            FilterHelper helper = new GeneralFunctions().GetWhereClause<LibraryForm>(parameter.Filters);
            query += helper.WhereClause;
            query += $" AND FormStatus IN ({(int)FormStatus.Accepted},{(int)FormStatus.Selected},{(int)FormStatus.Submitted})";
            query += new GeneralFunctions().GetPagedQuery(query, parameter);
            helper.Command.CommandText = query;
            return helper.Command;
        }

        public SqlCommand GetAllStudentsRR(PrintProgramme programme, Parameters parameter)
        {
            string tablePostFix = new GeneralFunctions().GetProgrammePostFix(programme);
            string query = $@"Select * from(SELECT Distinct
                              StudentFormNo,
                              FullName, 
                              FathersName, 
                              MothersName,
                              PINFO.Student_ID, 
                              BoardRegistrationNo,
                              CUSRegistrationNo,
                              Gender, 
                              Category,
                              DOB,
                              Photograph,
                              IsProvisional, 
                              FormStatus,
                              Religion,
                              Course_ID,
							   PINFO.AcceptCollege_ID AS College_ID,
                              COMB.Combination_ID,
                              ClassRollNo,
                              PINFO.Batch AS Year,
                              COMB.SemesterBatch,
                              PINFO.CreatedOn,
                              COMB.Semester,
                              PINFO.ABCID,
                              PINFO.IsLateralEntry
                              FROM ARGPERSONALINFORMATION{tablePostFix} PINFO 
                              INNER JOIN ARGSelectedCombination{tablePostFix} COMB  ON COMB.Student_ID = PINFO.Student_ID AND COMB.Semester<=PINFO.CurrentSemesterOrYear 
                              AND COMB.IsVerified=1
                              INNER JOIN ADMCombinationMaster ON ADMCombinationMaster.Combination_ID = COMB.Combination_ID
                              ) temp";

            SqlCommand command = new SqlCommand();
            FilterHelper helper = new GeneralFunctions().GetWhereClause<LibraryForm>(parameter.Filters);
            query += helper.WhereClause;
            query += $" AND FormStatus IN ({(int)FormStatus.Accepted},{(int)FormStatus.Selected},{(int)FormStatus.Submitted})";
            query += new GeneralFunctions().GetPagedQuery(query, parameter);
            helper.Command.CommandText = query;
            return helper.Command;
        }


        public SqlCommand GetAllStudentsAttendanceSheet(PrintProgramme programme, Guid Course_ID, int Batch, int Semester, string ExamRollNo, Guid College_ID, Guid? Center_ID)
        {
            string tablePostFix = new GeneralFunctions().GetProgrammePostFix(programme);
            string ExamRollNoSQL = string.Empty, CenterID = String.Empty;

            SqlCommand command = new SqlCommand();
            command.Parameters.AddWithValue("@College_ID", College_ID);
            command.Parameters.AddWithValue("@Course_ID", Course_ID);
            command.Parameters.AddWithValue("@Batch", Batch);
            command.Parameters.AddWithValue("@Semester", Semester);
            command.Parameters.AddWithValue("@Status", (int)FormStatus.Accepted);
            if (!string.IsNullOrWhiteSpace(ExamRollNo))
            {
                command.Parameters.AddWithValue("@ExamRollNo", ExamRollNo);
                ExamRollNoSQL = " AND ExamRollNumber=@ExamRollNo";
            }
            else
            {
                ExamRollNoSQL = " AND ExamRollNumber IS NOT NULL ";
            }

            if ((Center_ID ?? Guid.Empty) != Guid.Empty)
            {
                command.Parameters.AddWithValue("@Center_ID", Center_ID);
                CenterID = " AND ARGCentersAllotmentMaster.Center_ID=@Center_ID";
            }

            string query = $@"SELECT DISTINCT
                              StudentFormNo,
							  '' AS CollegeFullName,
                              FullName, 
                              FathersName, 
                              MothersName,
                              PINFO.Student_ID, 
                              PINFO.AcceptCollege_ID,
                              BoardRegistrationNo,
                              CUSRegistrationNo,
                              Gender, 
                              DOB,
                              Photograph, 
                              IsProvisional, 
                              FormStatus,
                             ADMCombinationMaster.Course_ID,
							   PINFO.AcceptCollege_ID AS College_ID,
                              ARGSelectedCombination{tablePostFix}.Combination_ID,
                              ExamRollNumber,
                              PINFO.Batch AS Year,
                              ARGSelectedCombination{tablePostFix}.SemesterBatch AS Batch,
                              PINFO.CreatedOn,
							  ARGSelectedCombination{tablePostFix}.semester,
                              ARGStudentExamForm{tablePostFix}.FormNumber,
							  ADMCourseMaster.CourseFullName	
                              FROM ARGPERSONALINFORMATION{tablePostFix} PINFO 
                              INNER JOIN ARGSelectedCombination{tablePostFix} ON ARGSelectedCombination{tablePostFix}.Student_ID =PINFO.Student_ID 
                              INNER JOIN ARGStudentExamForm{tablePostFix} ON ARGStudentExamForm{tablePostFix}.Student_ID = PINFO.Student_ID AND ARGStudentExamForm{tablePostFix}.Semester = ARGSelectedCombination{tablePostFix}.Semester
                              INNER JOIN ADMCombinationMaster ON ADMCombinationMaster.Combination_ID = ARGSelectedCombination{tablePostFix}.Combination_ID
							  INNER JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = ADMCombinationMaster.Course_ID
							  INNER JOIN ARGCentersAllotmentMaster ON ARGCentersAllotmentMaster.[Entity_ID] = StudentExamForm_ID
							  INNER JOIN ARGEntranceCentersMaster ON ARGEntranceCentersMaster.Center_ID = ARGCentersAllotmentMaster.Center_ID
                              WHERE ARGEntranceCentersMaster.College_ID=@College_ID AND ADMCourseMaster.Course_ID=@Course_ID  
							  AND ARGSelectedCombination{tablePostFix}.SemesterBatch<=@Batch AND ARGStudentExamForm{tablePostFix}.Semester=@Semester AND ARGStudentExamForm{tablePostFix}.Status=@Status 
                              {ExamRollNoSQL} {CenterID}
                              ORDER BY ExamRollNumber ASC";

            command.CommandText = query;
            return command;
        }


        public SqlCommand GetStudentAddress(PrintProgramme programme, Guid StudentID)
        {
            string tablePostFix = new GeneralFunctions().GetProgrammePostFix(programme);
            SqlCommand command = new SqlCommand();
            string query = $@"SELECT  *
                              FROM ARGStudentAddress{tablePostFix} 
                              WHERE Student_ID='{StudentID}'
                              ";

            command.CommandText = query;
            return command;
        }
        public SqlCommand GetStudentSubjects(Guid CombinationId)
        {
            SqlCommand command = new SqlCommand();
            string query = $@"SELECT
                              *
                              FROM ADMCombinationMaster  
                               WHERE Combination_ID='{CombinationId}'
                              ";
            command.CommandText = query;
            return command;
        }
        public SqlCommand GetSubject(Guid SubjectID)
        {
            SqlCommand command = new SqlCommand();
            string query = $@"SELECT * FROM VW_SubjectWithStructure SM JOIN Department D on SM.Department_ID= D.Department_ID WHERE SM.Subject_ID='{SubjectID}'";

            command.CommandText = query;
            return command;
        }

        public SqlCommand GetStudentCombinationByStudentID(Guid Student_ID, PrintProgramme programme)
        {

            SqlCommand command = new SqlCommand();
            string tablePostFix = new GeneralFunctions().GetProgrammePostFix(programme);
            string query = $@" SELECT TOP 1 
                              StudentFormNo,
                              FullName, 
                              FathersName, 
                              MothersName,
                              STDINFO.Student_ID, 
                              BoardRegistrationNo,
                              CUSRegistrationNo, 
                              Gender, 
                              DOB,
                              Photograph,
                              IsProvisional, 
                              FormStatus,
                              ADMCombinationMaster.Course_ID,
							  ADMCombinationMaster.College_ID,
                              ADMCombinationMaster.Combination_ID,
                              STDINFO.CreatedOn
                              FROM ARGPERSONALINFORMATION{tablePostFix} STDINFO 
                              LEFT JOIN ARGSelectedCombination{tablePostFix} COMB ON COMB.Student_ID = STDINFO.Student_ID
							  LEFT JOIN	ADMCombinationMaster ON ADMCombinationMaster.Combination_ID = COMB.Combination_ID
						      WHERE STDINFO.Student_ID='{Student_ID}'  ORDER BY COMB.Semester";
            command.CommandText = query;
            return command;
        }


        /// <summary>
        /// Gives Student personal information
        /// </summary>
        /// <param name="formNumberOrRegistrationNo"></param>
        /// <param name="printProgramme"></param>
        /// <returns></returns>
        internal SqlCommand GetStudent(string formNumberOrRegistrationNo, PrintProgramme printProgramme)
        {
            SqlCommand command = new SqlCommand();
            command.Parameters.AddWithValue("@FormNumber", formNumberOrRegistrationNo.Trim());
            command.CommandText = $@"SELECT Top(1) ARGPersonalInformation_{printProgramme.ToString()}.*,ADMCollegeMaster.CollegeFullName AS AcceptCollegeID 
                                    FROM ARGPersonalInformation_{printProgramme.ToString()}
                                    LEFT JOIN ADMCollegeMaster ON ADMCollegeMaster.College_ID = ARGPersonalInformation_{printProgramme.ToString()}.AcceptCollege_ID  
                                    WHERE (ARGPersonalInformation_{printProgramme.ToString()}.CUSRegistrationNo = @FormNumber 
                                            OR ARGPersonalInformation_{printProgramme.ToString()}.BoardRegistrationNo = @FormNumber 
                                            OR ARGPersonalInformation_{printProgramme.ToString()}.StudentFormNo = @FormNumber)
order by ARGPersonalInformation_{printProgramme.ToString()}.Batch DESC";
            return command;
        }
        internal SqlCommand GetStudent(string formNumberOrRegistrationNo, DateTime DOB, PrintProgramme printProgramme)
        {
            SqlCommand command = new SqlCommand();
            command.Parameters.AddWithValue("@FormNumber", formNumberOrRegistrationNo.Trim());
            command.Parameters.AddWithValue("@DOB", DOB);
            command.CommandText = $@"SELECT ARGPersonalInformation_{printProgramme.ToString()}.*,ADMCollegeMaster.CollegeFullName AS AcceptCollegeID 
                                    FROM ARGPersonalInformation_{printProgramme.ToString()}
                                    LEFT JOIN ADMCollegeMaster ON ADMCollegeMaster.College_ID = ARGPersonalInformation_{printProgramme.ToString()}.AcceptCollege_ID  
                                    WHERE (ARGPersonalInformation_{printProgramme.ToString()}.CUSRegistrationNo = @FormNumber 
                                            OR ARGPersonalInformation_{printProgramme.ToString()}.BoardRegistrationNo = @FormNumber 
                                            OR ARGPersonalInformation_{printProgramme.ToString()}.StudentFormNo = @FormNumber) AND DOB=@DOB AND AcceptCollege_ID IS NOT NULL";
            return command;
        }
        internal SqlCommand GetStudentAddress(Guid student_ID, PrintProgramme printProgramme)
        {
            SqlCommand command = new SqlCommand();
            command.Parameters.AddWithValue("@Student_ID", student_ID);
            command.CommandText = $@"SELECT * FROM ARGStudentAddress_{printProgramme.ToString()} WHERE Student_ID = @Student_ID";
            return command;
        }

        internal string StudentListCompact(PrintProgramme printProgramme)
        {
            var compactQuery = PersonalInformationCompactQuery(printProgramme);
            string query = $@"SELECT * FROM {compactQuery} STDINFO";
            return query;
        }
        internal string PersonalInformationCompactQuery(PrintProgramme printProgramme)
        {
            string query = $@" (SELECT std.[Student_ID],[BoardRegistrationNo],[CUSRegistrationNo],[FullName],[FathersName],[Gender],[DOB],[Category],[AcceptCollege_ID],CollegeFullName
                           ,[CurrentSemesterOrYear],[Batch],[Address_ID],ADR.Mobile,ADR.Email,STDCOURSE.Course_ID,CourseCode,CourseFullName,PrintProgramme,ClassRollNo
                              FROM [ARGPersonalInformation_{printProgramme.ToString()}] std
                              LEFT JOIN ADMCollegeMaster ON ADMCollegeMaster.College_ID=std.AcceptCollege_ID
                              LEFT JOIN ARGStudentAddress_{printProgramme.ToString()} ADR ON ADR.Student_ID = std.Student_ID
                              LEFT JOIN ARGCoursesApplied_{printProgramme.ToString()} STDCOURSE ON STDCOURSE.Student_ID = std.Student_ID
                              LEFT JOIN ADMCourseMaster  ON ADMCourseMaster.Course_ID = STDCOURSE.Course_ID WHERE std.FormStatus=9)";
            return query;
        }
        internal SqlCommand GetStudentAcademicDetails(Guid student_ID, PrintProgramme printProgramme)
        {
            SqlCommand command = new SqlCommand();
            command.Parameters.AddWithValue("@Student_ID", student_ID);
            command.CommandText = $@"SELECT * FROM ARGStudentPreviousQualifications_{printProgramme.ToString()} WHERE Student_ID = @Student_ID";
            return command;
        }
        internal SqlCommand GetStudentCoursesApplied(Guid student_ID, PrintProgramme printProgramme)
        {
            SqlCommand command = new SqlCommand();
            command.Parameters.AddWithValue("@Student_ID", student_ID);
            command.CommandText = $@"SELECT ARGCoursesApplied_{printProgramme.ToString()}.*,
                                            ADMCourseMaster.CourseFullName AS CourseName,
                                            ADMCourseMaster.CourseCode
                                            FROM ARGCoursesApplied_{printProgramme.ToString()}
                                            INNER JOIN ADMCourseMaster ON ARGCoursesApplied_{printProgramme.ToString()}.Course_ID = ADMCourseMaster.Course_ID
                                            WHERE ARGCoursesApplied_{printProgramme.ToString()}.Student_ID = @Student_ID";
            return command;
        }
        internal SqlCommand GetSelectedCombinations(Guid student_ID, PrintProgramme printProgramme)
        {
            string query = $@"SELECT Combination.*,CombinationCode AS CombinationID,ADMCombinationMaster.College_ID,ADMCourseMaster.PrintProgramme,
                            ADMCombinationMaster.Course_ID,ADMCollegeMaster.CollegeFullName AS CollegeID,ADMCourseMaster.CourseFullName AS CourseID,ADMCourseMaster.CourseCode ,CombinationSubjects
                            FROM ARGSelectedCombination_{printProgramme.ToString()} Combination
                            JOIN ADMCombinationMaster ON ADMCombinationMaster.Combination_ID = Combination.Combination_ID 
                            JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = ADMCombinationMaster.Course_ID
                            JOIN ADMCollegeMaster ON ADMCollegeMaster.College_ID = ADMCombinationMaster.College_ID
                            WHERE Combination.Student_ID='{student_ID}' ORDER BY Combination.Semester";
            SqlCommand command = new SqlCommand();
            command.CommandText = query;
            command.Parameters.AddWithValue("@Student_ID", student_ID);
            return command;
        }
        internal SqlCommand GetSelectedCombinations(Guid student_ID, PrintProgramme printProgramme, short semester)
        {
            string query = $@"SELECT Combination.*,CombinationCode AS CombinationID,ADMCombinationMaster.College_ID,ADMCourseMaster.PrintProgramme,
                            ADMCombinationMaster.Course_ID,ADMCollegeMaster.CollegeFullName AS CollegeID,ADMCourseMaster.CourseFullName AS CourseID,ADMCourseMaster.CourseCode ,CombinationSubjects
                            FROM ARGSelectedCombination_{printProgramme.ToString()} Combination
                            JOIN ADMCombinationMaster ON ADMCombinationMaster.Combination_ID = Combination.Combination_ID 
                            JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = ADMCombinationMaster.Course_ID
                            JOIN ADMCollegeMaster ON ADMCollegeMaster.College_ID = ADMCombinationMaster.College_ID
                            WHERE Combination.Student_ID='{student_ID}' AND Combination.Semester={semester}";
            SqlCommand command = new SqlCommand();
            command.CommandText = query;
            return command;
        }

        internal SqlCommand GetSelectedCombinations(Guid student_ID, short Semester, PrintProgramme printProgramme)
        {
            string query = $@"SELECT ARGSelectedCombination_{printProgramme.ToString()}.*,CombinationCode AS CombinationID,ADMCombinationMaster.College_ID,PrintProgramme,
                            ADMCombinationMaster.Course_ID,ADMCollegeMaster.CollegeFullName AS CollegeID,ADMCourseMaster.CourseFullName AS CourseID,CombinationSubjects
                            FROM ARGSelectedCombination_{printProgramme.ToString()}
                            JOIN ADMCombinationMaster ON ADMCombinationMaster.Combination_ID = ARGSelectedCombination_{printProgramme.ToString()}.Combination_ID
                            JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = ADMCombinationMaster.Course_ID
                            JOIN ADMCollegeMaster ON ADMCollegeMaster.College_ID = ADMCombinationMaster.College_ID
                            WHERE	ARGSelectedCombination_{printProgramme.ToString()}.Student_ID=@Student_ID AND ARGSelectedCombination_{printProgramme.ToString()}.Semester=@Semester";
            SqlCommand command = new SqlCommand();
            command.CommandText = query;
            command.Parameters.AddWithValue("@Student_ID", student_ID);
            command.Parameters.AddWithValue("@Semester", Semester);
            return command;
        }


        public SqlCommand GetAllStudentsRegister(PrintProgramme programme, Parameters parameter)
        {
            string tablePostFix = new GeneralFunctions().GetProgrammePostFix(programme);
            string query = $@"SELECT * FROM (SELECT ROW_NUMBER() OVER(PARTITION BY COMB.Student_ID ORDER BY COMB.Student_ID ASC,ADMCombinationMaster.Semester DESC) sno,
                              StudentFormNo,
                              FullName, 
                              FathersName, 
                              MothersName,
                              PINFO.Student_ID, 
                              BoardRegistrationNo,
                              CUSRegistrationNo,
                              Gender, 
                              Category,
                              DOB,
                              Photograph,
                              IsProvisional, 
                              FormStatus,
                              IsPassout,
                              MigrationIssued,
                              Course_ID,
							  PINFO.AcceptCollege_ID AS College_ID,
                              COMB.Combination_ID,
                              ClassRollNo,
                              PINFO.Batch AS Year,
                              COMB.SemesterBatch,
                              PINFO.CreatedOn,
                              COMB.Semester,
                              PINFO.IsLateralEntry
                              FROM ARGPERSONALINFORMATION{tablePostFix} PINFO 
                              INNER JOIN ARGSelectedCombination{tablePostFix} COMB  ON COMB.Student_ID = PINFO.Student_ID AND COMB.Semester<=PINFO.CurrentSemesterOrYear 
                              INNER JOIN ADMCombinationMaster ON ADMCombinationMaster.Combination_ID = COMB.Combination_ID
                              WHERE COMB.SemesterBatch = @SemesterBatch AND PINFO.AcceptedBy_ID IS NOT NULL
                              ) temp";

            SqlCommand command = new SqlCommand();
            FilterHelper helper = new GeneralFunctions().GetWhereClause<LibraryForm>(parameter.Filters);
            query += helper.WhereClause;
            query += $" AND temp.IsPassout=1 AND temp.sno=1 AND FormStatus IN ({(int)FormStatus.Accepted},{(int)FormStatus.Selected},{(int)FormStatus.Submitted})" + "ORDER BY temp.CUSRegistrationNo ASC";
            query += new GeneralFunctions().GetPagedQuery(query, parameter);
            helper.Command.CommandText = query;
            return helper.Command;
        }
    }
}
