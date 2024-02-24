using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CUSrinagar.DataManagers.SQLQueries;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using CUSrinagar.Models;
using GeneralModels;
using Terex;

namespace CUSrinagar.DataManagers
{
    public class StudentStatisticsDB
    {

        /// <summary>
        /// Universal method actual method main final one only
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public List<StudentsCourseWiseStatistics> GetstudentsCourseWiseStatistics(Parameters parameters, bool CategoryWise = false, bool DistrictWise = false)
        {
            FilterHelper helper;
            if (parameters.Filters == null || parameters.Filters.Where(x => x.Column == "Semester").FirstOrDefault() == null)
            {
                if (parameters.Filters != null && parameters.Filters.Where(x => x.Column == "SemesterBatch").Count() > 0)
                {
                    List<SearchFilter> sflist = parameters.Filters.Where(x => x.Column == "SemesterBatch").ToList();
                    foreach (SearchFilter sf in sflist)
                    { sf.Column = "Batch"; }
                }
                helper = new GeneralFunctions().GetWhereClause<StudentsCourseWiseStatistics>(parameters.Filters);

                helper.Command.CommandText = $@"SELECT CollegeFullName,Batch
                            ,(ISNULL(Male,0)+ISNULL(Female,0)+ISNULL(Other,0))NoOfStudents,ISNULL(Male,0)Male,ISNULL(Female,0)Female,ISNULL(Other,0)Other
                            FROM (
                            SELECT  CollegeFullName,Gender,Batch,COUNT(S.Student_ID)T
                            FROM
                            (
	                            SELECT Student_ID, AcceptCollege_ID,Batch, FormStatus,CurrentSemesterOrYear{(CategoryWise ? ",Category" : "")},(CASE WHEN Gender='Male' THEN 'Male' ELSE CASE WHEN Gender='Female' THEN 'Female' ELSE 'Other' END END )Gender FROM ARGPersonalInformation_UG
	                            UNION ALL
	                            SELECT Student_ID, AcceptCollege_ID,Batch, FormStatus,CurrentSemesterOrYear{(CategoryWise ? ",Category" : "")},(CASE WHEN Gender='Male' THEN 'Male' ELSE CASE WHEN Gender='Female' THEN 'Female' ELSE 'Other' END END )Gender FROM ARGPersonalInformation_PG
	                            UNION ALL
	                            SELECT Student_ID, AcceptCollege_ID,Batch, FormStatus,CurrentSemesterOrYear{(CategoryWise ? ",Category" : "")},(CASE WHEN Gender='Male' THEN 'Male' ELSE CASE WHEN Gender='Female' THEN 'Female' ELSE 'Other' END END )Gender FROM ARGPersonalInformation_IH
                            )S
                           
                            INNER JOIN ADMCollegeMaster College ON College.College_ID = S.AcceptCollege_ID
                             {helper.WhereClause}
                            GROUP BY  CollegeFullName,S.Batch,Gender
                            )P
                            PIVOT(
	                            MAX(T)
	                            FOR Gender IN ([Male],[Female],[Other])
                            )G
                            ORDER BY CollegeFullName,G.Male,G.Female,G.Other";
            }
            else
            {
                helper = new GeneralFunctions().GetWhereClause<StudentsCourseWiseStatistics>(parameters.Filters);

                helper.Command.CommandText = $@"SELECT SemesterBatch,PrintProgramme,Programme,CollegeFullName,Semester,CourseFullName{(CategoryWise ? ",Category" : "")}{(DistrictWise ? ",District" : "")}
                            ,(ISNULL(Male,0)+ISNULL(Female,0)+ISNULL(Other,0))NoOfStudents,ISNULL(Male,0)Male,ISNULL(Female,0)Female,ISNULL(Other,0)Other
                            FROM (
                            SELECT Comb.SemesterBatch,PrintProgramme,Programme, CollegeFullName,Comb.Semester, CourseFullName{(CategoryWise ? ",S.Category" : "")}{(DistrictWise ? ",A.District" : "")},Gender,COUNT(S.Student_ID)T
                            FROM
                            (
	                            SELECT Student_ID, AcceptCollege_ID, FormStatus,CurrentSemesterOrYear{(CategoryWise ? ",Category" : "")},(CASE WHEN Gender='Male' THEN 'Male' ELSE CASE WHEN Gender='Female' THEN 'Female' ELSE 'Other' END END )Gender FROM ARGPersonalInformation_UG
	                            UNION ALL
	                            SELECT Student_ID, AcceptCollege_ID, FormStatus,CurrentSemesterOrYear{(CategoryWise ? ",Category" : "")},(CASE WHEN Gender='Male' THEN 'Male' ELSE CASE WHEN Gender='Female' THEN 'Female' ELSE 'Other' END END )Gender FROM ARGPersonalInformation_PG
	                            UNION ALL
	                            SELECT Student_ID, AcceptCollege_ID, FormStatus,CurrentSemesterOrYear{(CategoryWise ? ",Category" : "")},(CASE WHEN Gender='Male' THEN 'Male' ELSE CASE WHEN Gender='Female' THEN 'Female' ELSE 'Other' END END )Gender FROM ARGPersonalInformation_IH
                            )S
                            INNER JOIN
                            (
	                            SELECT Student_ID, Semester, SemesterBatch,Combination_ID FROM ARGSelectedCombination_UG Where IsVerified=1
	                            UNION ALL
	                            SELECT Student_ID, Semester,SemesterBatch, Combination_ID FROM ARGSelectedCombination_PG Where IsVerified=1
	                            UNION ALL
	                            SELECT Student_ID, Semester,SemesterBatch, Combination_ID FROM ARGSelectedCombination_IH Where IsVerified=1
                            )Comb ON S.Student_ID = Comb.Student_ID AND Comb.Semester <= S.CurrentSemesterOrYear  
                            {(DistrictWise ? @"LEFT JOIN
                            (
	                            SELECT Student_ID,District FROM dbo.ARGStudentAddress_UG 
	                            UNION ALL
	                            SELECT Student_ID,District FROM dbo.ARGStudentAddress_PG 
	                            UNION ALL
	                            SELECT Student_ID,District FROM dbo.ARGStudentAddress_IH 
                            )A ON A.Student_ID = S.Student_ID" : "")}                                                                       
                            INNER JOIN ADMCombinationMaster CM ON Comb.Semester = CM.Semester AND CM.Combination_ID = Comb.Combination_ID 
                            INNER JOIN ADMCollegeMaster College ON College.College_ID = S.AcceptCollege_ID
                            INNER JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = CM.Course_ID
                            {helper.WhereClause}
                            GROUP BY Comb.SemesterBatch,PrintProgramme, Programme, CollegeFullName, CourseFullName,Comb.Semester,Gender{(CategoryWise ? ",Category" : "")}{(DistrictWise ? ",A.District" : "")}
                            )P
                            PIVOT(
	                            MAX(T)
	                            FOR Gender IN ([Male],[Female],[Other])
                            )G
                            ORDER BY SemesterBatch,CollegeFullName,PrintProgramme, Programme,  CourseFullName,Semester{(CategoryWise ? ",Category" : "")}{(DistrictWise ? ",District" : "")},G.Male,G.Female,G.Other";
            }
            return new MSSQLFactory().GetObjectList<StudentsCourseWiseStatistics>(helper.Command);
        }

        public DataTable GetProgrammeCollegeSemesterAdmissionPayments(DateTime fromDate, DateTime toDate, PaymentType? paymentType)
        {
            string pTypes = "";
            if (paymentType != null)
            {
                if (paymentType.Value == PaymentType.Online)
                {
                    pTypes = $" AND PaymentType IN(${(short)PaymentType.Online},{(short)PaymentType.Reconciled},{(short)PaymentType.ReFund}) ";
                }
                else
                    pTypes = $" AND PaymentType IN(${(short)paymentType}) ";
            }

            return new MSSQLFactory().GetDataTable($@"SELECT dt.[Year of Payment],
                                                       dt.Programmes,
                                                       dt.Semester,
                                                       dt.[Students Of College],
                                                       SUM(dt.[Total Students]) [Total Students],
                                                       SUM(dt.[University Component Amount]) [University Component Amount],
                                                       dt.[Payment Type] FROM (SELECT YEAR(TxnDate) [Year of Payment],
                                                       CASE
                                                           WHEN Programme = 1 THEN
                                                               'UG'
                                                           WHEN Programme = 2 THEN
                                                               'PG'
                                                           WHEN Programme = 3 THEN
                                                               'IG'
                                                           WHEN Programme = 4 THEN
                                                               'Honors'
                                                           WHEN Programme = 6 THEN
                                                               'Professional'
                                                           WHEN Programme = 5 THEN
                                                               'Engineering'
                                                       END AS Programmes,
                                                       CASE
                                                           WHEN PaymentDetails_UG.Semester = 1 THEN
                                                               'Semester 1 & 2'
                                                           WHEN PaymentDetails_UG.Semester = 3 THEN
                                                               'Semester 3 & 4'
                                                           WHEN PaymentDetails_UG.Semester = 5 THEN
                                                               'Semester 5 & 6'
                                                           WHEN PaymentDetails_UG.Semester = 7 THEN
                                                               'Semester 7 & 8'
                                                           WHEN PaymentDetails_UG.Semester = 9 THEN
                                                               'Semester 9 & 10'
                                                           WHEN PaymentDetails_UG.Semester = 11 THEN
                                                               'Semester 11 & 12'
                                                           ELSE
                                                               CAST(PaymentDetails_UG.Semester AS VARCHAR(10))
                                                       END Semester,
                                                       CollegeFullName [Students Of College],
                                                       COUNT(Entity_ID) [Total Students],
                                                       SUM(TxnAmount) [University Component Amount],
	                                                   MAX(CASE
                                                           WHEN dbo.PaymentDetails_UG.PaymentType = 1
                                                                OR dbo.PaymentDetails_UG.PaymentType = 3 THEN
                                                               'Online'
                                                           WHEN dbo.PaymentDetails_UG.PaymentType = 2 THEN
                                                               'offline'
                                                           ELSE
                                                               'offline'
                                                       END) [Payment Type]
                                                FROM dbo.ARGPersonalInformation_UG
                                                    JOIN dbo.PaymentDetails_UG
                                                        ON Entity_ID = Student_ID
                                                    JOIN dbo.ARGSelectedCombination_UG
                                                        ON ARGSelectedCombination_UG.Student_ID = ARGPersonalInformation_UG.Student_ID
                                                    JOIN dbo.ADMCombinationMaster
                                                        ON ADMCombinationMaster.Combination_ID = ARGSelectedCombination_UG.Combination_ID
                                                           AND ARGSelectedCombination_UG.Semester = PaymentDetails_UG.Semester
                                                    JOIN dbo.ADMCourseMaster
                                                        ON ADMCourseMaster.Course_ID = ADMCombinationMaster.Course_ID
                                                    JOIN dbo.ADMCollegeMaster
                                                        ON ADMCollegeMaster.College_ID = ADMCombinationMaster.College_ID
                                                WHERE ModuleType = 6 AND (CAST(TxnDate AS DATE) BETWEEN '{fromDate.ToString("yyyy-MM-dd")}' AND  '{toDate.ToString("yyyy-MM-dd")}') {pTypes}
                                                GROUP BY CollegeFullName,
                                                         Programme,
                                                         PaymentDetails_UG.Semester,
                                                         YEAR(TxnDate)
                                                UNION
                                                SELECT YEAR(TxnDate) [Year of Payment],
                                                       CASE
                                                           WHEN Programme = 1 THEN
                                                               'UG'
                                                           WHEN Programme = 2 THEN
                                                               'PG'
                                                           WHEN Programme = 3 THEN
                                                               'IG'
                                                           WHEN Programme = 4 THEN
                                                               'Honors'
                                                           WHEN Programme = 6 THEN
                                                               'Professional'
                                                           WHEN Programme = 5 THEN
                                                               'Engineering'
                                                       END AS Programmes,
                                                       CASE
                                                           WHEN PaymentDetails_IH.Semester = 1 THEN
                                                               'Semester 1 & 2'
                                                           WHEN PaymentDetails_IH.Semester = 3 THEN
                                                               'Semester 3 & 4'
                                                           WHEN PaymentDetails_IH.Semester = 5 THEN
                                                               'Semester 5 & 6'
                                                           WHEN PaymentDetails_IH.Semester = 7 THEN
                                                               'Semester 7 & 8'
                                                           WHEN PaymentDetails_IH.Semester = 9 THEN
                                                               'Semester 9 & 10'
                                                           WHEN PaymentDetails_IH.Semester = 11 THEN
                                                               'Semester 11 & 12'
                                                           ELSE
                                                               CAST(PaymentDetails_IH.Semester AS VARCHAR(10))
                                                       END Semester,
                                                       CollegeFullName [Students Of College],
                                                       COUNT(Entity_ID) [Total Students],
                                                       SUM(TxnAmount) [University Component Amount],
	                                                   MAX(CASE
                                                           WHEN dbo.PaymentDetails_IH.PaymentType = 1
                                                                OR dbo.PaymentDetails_IH.PaymentType = 3 THEN
                                                               'Online'
                                                           WHEN dbo.PaymentDetails_IH.PaymentType = 2 THEN
                                                               'offline'
                                                           ELSE
                                                               'offline'
                                                       END) [Payment Type]
                                                FROM dbo.ARGPersonalInformation_IH
                                                    JOIN dbo.PaymentDetails_IH
                                                        ON Entity_ID = Student_ID
                                                    JOIN dbo.ARGSelectedCombination_IH
                                                        ON ARGSelectedCombination_IH.Student_ID = ARGPersonalInformation_IH.Student_ID
                                                    JOIN dbo.ADMCombinationMaster
                                                        ON ADMCombinationMaster.Combination_ID = ARGSelectedCombination_IH.Combination_ID
                                                           AND ARGSelectedCombination_IH.Semester = PaymentDetails_IH.Semester
                                                    JOIN dbo.ADMCourseMaster
                                                        ON ADMCourseMaster.Course_ID = ADMCombinationMaster.Course_ID
                                                    JOIN dbo.ADMCollegeMaster
                                                        ON ADMCollegeMaster.College_ID = ADMCombinationMaster.College_ID
                                                WHERE ModuleType = 6 AND (CAST(TxnDate AS DATE) BETWEEN '{fromDate.ToString("yyyy-MM-dd")}' AND  '{toDate.ToString("yyyy-MM-dd")}') {pTypes}
                                                GROUP BY CollegeFullName,
                                                         Programme,
                                                         PaymentDetails_IH.Semester,
                                                         YEAR(TxnDate)	 
                                                UNION
                                                SELECT YEAR(TxnDate) [Year of Payment],
                                                       CASE
                                                           WHEN Programme = 1 THEN
                                                               'UG'
                                                           WHEN Programme = 2 THEN
                                                               'PG'
                                                           WHEN Programme = 3 THEN
                                                               'IG'
                                                           WHEN Programme = 4 THEN
                                                               'Honors'
                                                           WHEN Programme = 6 THEN
                                                               'Professional'
                                                           WHEN Programme = 5 THEN
                                                               'Engineering'
                                                       END AS Programmes,
                                                       CASE
                                                           WHEN PaymentDetails_PG.Semester = 1 THEN
                                                               'Semester 1 & 2'
                                                           WHEN PaymentDetails_PG.Semester = 3 THEN
                                                               'Semester 3 & 4'
                                                           WHEN PaymentDetails_PG.Semester = 5 THEN
                                                               'Semester 5 & 6'
                                                           WHEN PaymentDetails_PG.Semester = 7 THEN
                                                               'Semester 7 & 8'
                                                           WHEN PaymentDetails_PG.Semester = 9 THEN
                                                               'Semester 9 & 10'
                                                           WHEN PaymentDetails_PG.Semester = 11 THEN
                                                               'Semester 11 & 12'
                                                           ELSE
                                                               CAST(PaymentDetails_PG.Semester AS VARCHAR(10))
                                                       END Semester,
                                                       CollegeFullName [Students Of College],
                                                       COUNT(Entity_ID) [Total Students],
                                                       SUM(TxnAmount) [University Component Amount],
	                                                   MAX(CASE
                                                           WHEN dbo.PaymentDetails_PG.PaymentType = 1
                                                                OR dbo.PaymentDetails_PG.PaymentType = 3 THEN
                                                               'Online'
                                                           WHEN dbo.PaymentDetails_PG.PaymentType = 2 THEN
                                                               'offline'
                                                           ELSE
                                                               'offline'
                                                       END) [Payment Type]
                                                FROM dbo.ARGPersonalInformation_PG
                                                    JOIN dbo.PaymentDetails_PG
                                                        ON Entity_ID = Student_ID
                                                    JOIN dbo.ARGSelectedCombination_PG
                                                        ON ARGSelectedCombination_PG.Student_ID = ARGPersonalInformation_PG.Student_ID
                                                    JOIN dbo.ADMCombinationMaster
                                                        ON ADMCombinationMaster.Combination_ID = ARGSelectedCombination_PG.Combination_ID
                                                           AND ARGSelectedCombination_PG.Semester = PaymentDetails_PG.Semester
                                                    JOIN dbo.ADMCourseMaster
                                                        ON ADMCourseMaster.Course_ID = ADMCombinationMaster.Course_ID
                                                    JOIN dbo.ADMCollegeMaster
                                                        ON ADMCollegeMaster.College_ID = ADMCombinationMaster.College_ID
                                                WHERE ModuleType = 6 AND (CAST(TxnDate AS DATE) BETWEEN '{fromDate.ToString("yyyy-MM-dd")}' AND  '{toDate.ToString("yyyy-MM-dd")}') {pTypes}
                                                GROUP BY CollegeFullName,
                                                         Programme,
                                                         PaymentDetails_PG.Semester,
                                                         YEAR(TxnDate)) dt
		                                                 GROUP BY  dt.[Students Of College],
                                                         dt.Programmes,
                                                         dt.Semester,
                                                         dt.[Year of Payment],
		                                                 dt.[Payment Type]
                                                         ORDER BY  Programmes DESC,
		                                                dt.[Students Of College],
                                                         Semester,
                                                         dt.[Year of Payment],
		                                                 dt.[Payment Type]
	                                                 ");
        }

        public DataTable GetCourseCollegeSemesterAdmissionPayments(DateTime fromDate, DateTime toDate, PaymentType? paymentType)
        {
            string pTypes = "";
            if (paymentType != null)
            {
                if (paymentType.Value == PaymentType.Online)
                {
                    pTypes = $" AND PaymentType IN(${(short)PaymentType.Online},{(short)PaymentType.Reconciled},{(short)PaymentType.ReFund}) ";
                }
                else
                    pTypes = $" AND PaymentType IN(${(short)paymentType}) ";
            }

            return new MSSQLFactory().GetDataTable($@"SELECT * FROM (SELECT YEAR(TxnDate) [Year Of Payment],
                                               CourseFullName [Course],
                                               CASE
                                                   WHEN PaymentDetails_UG.Semester = 1 THEN
                                                       'Semester 1 & 2'
                                                   WHEN PaymentDetails_UG.Semester = 3 THEN
                                                       'Semester 3 & 4'
                                                   WHEN PaymentDetails_UG.Semester = 5 THEN
                                                       'Semester 5 & 6'
                                                   WHEN PaymentDetails_UG.Semester = 7 THEN
                                                       'Semester 7 & 8'
                                                   WHEN PaymentDetails_UG.Semester = 9 THEN
                                                       'Semester 9 & 10'
                                                   WHEN PaymentDetails_UG.Semester = 11 THEN
                                                       'Semester 11 & 12'
                                                   ELSE
                                                       CAST(PaymentDetails_UG.Semester AS VARCHAR(10))
                                               END Semester,
                                               CollegeFullName [Students Of College],
                                               COUNT(Entity_ID) TotalStudents,
                                               SUM(TxnAmount)  [University Component Amount],
                                               CASE
                                                   WHEN dbo.PaymentDetails_UG.PaymentType = 1
                                                        OR dbo.PaymentDetails_UG.PaymentType = 3 THEN
                                                       'Online'
                                                   WHEN dbo.PaymentDetails_UG.PaymentType = 2 THEN
                                                       'offline'
                                                   ELSE
                                                       'offline'
                                               END [Payment Type]
                                        FROM dbo.ARGPersonalInformation_UG
                                            JOIN dbo.PaymentDetails_UG
                                                ON Entity_ID = Student_ID
                                            JOIN dbo.ARGSelectedCombination_UG
                                                ON ARGSelectedCombination_UG.Student_ID = ARGPersonalInformation_UG.Student_ID
                                            JOIN dbo.ADMCombinationMaster
                                                ON ADMCombinationMaster.Combination_ID = ARGSelectedCombination_UG.Combination_ID
                                                   AND ARGSelectedCombination_UG.Semester = PaymentDetails_UG.Semester
                                            JOIN dbo.ADMCourseMaster
                                                ON ADMCourseMaster.Course_ID = ADMCombinationMaster.Course_ID
                                            JOIN dbo.ADMCollegeMaster
                                                ON ADMCollegeMaster.College_ID = ADMCombinationMaster.College_ID
                                        WHERE ModuleType = 6 AND (CAST(TxnDate AS DATE) BETWEEN '{fromDate.ToString("yyyy-MM-dd")}' AND  '{toDate.ToString("yyyy-MM-dd")}') {pTypes}
                                        GROUP BY CollegeFullName,
                                                 CourseFullName,
                                                 PaymentDetails_UG.Semester,
                                                 PaymentType,
                                                 YEAR(TxnDate)
                                        UNION 
                                        SELECT YEAR(TxnDate) [Year Of Payment],
                                               CourseFullName [Course],
                                               CASE
                                                   WHEN PaymentDetails_IH.Semester = 1 THEN
                                                       'Semester 1 & 2'
                                                   WHEN PaymentDetails_IH.Semester = 3 THEN
                                                       'Semester 3 & 4'
                                                   WHEN PaymentDetails_IH.Semester = 5 THEN
                                                       'Semester 5 & 6'
                                                   WHEN PaymentDetails_IH.Semester = 7 THEN
                                                       'Semester 7 & 8'
                                                   WHEN PaymentDetails_IH.Semester = 9 THEN
                                                       'Semester 9 & 10'
                                                   WHEN PaymentDetails_IH.Semester = 11 THEN
                                                       'Semester 11 & 12'
                                                   ELSE
                                                       CAST(PaymentDetails_IH.Semester AS VARCHAR(10))
                                               END Semester,
                                               CollegeFullName [Students Of College],
                                               COUNT(Entity_ID) TotalStudents,
                                               SUM(TxnAmount)  [University Component Amount],
                                               CASE
                                                   WHEN dbo.PaymentDetails_IH.PaymentType = 1
                                                        OR dbo.PaymentDetails_IH.PaymentType = 3 THEN
                                                       'Online'
                                                   WHEN dbo.PaymentDetails_IH.PaymentType = 2 THEN
                                                       'offline'
                                                   ELSE
                                                       'offline'
                                               END [Payment Type]
                                        FROM dbo.ARGPersonalInformation_IH
                                            JOIN dbo.PaymentDetails_IH
                                                ON Entity_ID = Student_ID
                                            JOIN dbo.ARGSelectedCombination_IH
                                                ON ARGSelectedCombination_IH.Student_ID = ARGPersonalInformation_IH.Student_ID
                                            JOIN dbo.ADMCombinationMaster
                                                ON ADMCombinationMaster.Combination_ID = ARGSelectedCombination_IH.Combination_ID
                                                   AND ARGSelectedCombination_IH.Semester = PaymentDetails_IH.Semester
                                            JOIN dbo.ADMCourseMaster
                                                ON ADMCourseMaster.Course_ID = ADMCombinationMaster.Course_ID
                                            JOIN dbo.ADMCollegeMaster
                                                ON ADMCollegeMaster.College_ID = ADMCombinationMaster.College_ID
                                        WHERE ModuleType = 6 AND (CAST(TxnDate AS DATE) BETWEEN '{fromDate.ToString("yyyy-MM-dd")}' AND  '{toDate.ToString("yyyy-MM-dd")}') {pTypes}
                                        GROUP BY CollegeFullName,
                                                 CourseFullName,
                                                 PaymentDetails_IH.Semester,
                                                 PaymentType,
                                                 YEAR(TxnDate)
                                        UNION
                                        SELECT YEAR(TxnDate) [Year Of Payment],
                                               CourseFullName [Course],
                                               CASE
                                                   WHEN PaymentDetails_PG.Semester = 1 THEN
                                                       'Semester 1 & 2'
                                                   WHEN PaymentDetails_PG.Semester = 3 THEN
                                                       'Semester 3 & 4'
                                                   WHEN PaymentDetails_PG.Semester = 5 THEN
                                                       'Semester 5 & 6'
                                                   WHEN PaymentDetails_PG.Semester = 7 THEN
                                                       'Semester 7 & 8'
                                                   WHEN PaymentDetails_PG.Semester = 9 THEN
                                                       'Semester 9 & 10'
                                                   WHEN PaymentDetails_PG.Semester = 11 THEN
                                                       'Semester 11 & 12'
                                                   ELSE
                                                       CAST(PaymentDetails_PG.Semester AS VARCHAR(10))
                                               END Semester,
                                               CollegeFullName [Students Of College],
                                               COUNT(Entity_ID) TotalStudents,
                                               SUM(TxnAmount)  [University Component Amount],
                                               CASE
                                                   WHEN dbo.PaymentDetails_PG.PaymentType = 1
                                                        OR dbo.PaymentDetails_PG.PaymentType = 3 THEN
                                                       'Online'
                                                   WHEN dbo.PaymentDetails_PG.PaymentType = 2 THEN
                                                       'offline'
                                                   ELSE
                                                       'offline'
                                               END [Payment Type]
                                        FROM dbo.ARGPersonalInformation_PG
                                            JOIN dbo.PaymentDetails_PG
                                                ON Entity_ID = Student_ID
                                            JOIN dbo.ARGSelectedCombination_PG
                                                ON ARGSelectedCombination_PG.Student_ID = ARGPersonalInformation_PG.Student_ID
                                            JOIN dbo.ADMCombinationMaster
                                                ON ADMCombinationMaster.Combination_ID = ARGSelectedCombination_PG.Combination_ID
                                                   AND ARGSelectedCombination_PG.Semester = PaymentDetails_PG.Semester
                                            JOIN dbo.ADMCourseMaster
                                                ON ADMCourseMaster.Course_ID = ADMCombinationMaster.Course_ID
                                            JOIN dbo.ADMCollegeMaster
                                                ON ADMCollegeMaster.College_ID = ADMCombinationMaster.College_ID
                                        WHERE ModuleType = 6 AND (CAST(TxnDate AS DATE) BETWEEN '{fromDate.ToString("yyyy-MM-dd")}' AND  '{toDate.ToString("yyyy-MM-dd")}') {pTypes}
                                        GROUP BY CollegeFullName,
                                                 CourseFullName,
                                                 PaymentDetails_PG.Semester,
                                                 PaymentType,
                                                 YEAR(TxnDate)) dt
		                                         ORDER BY dt.[Students Of College],
                                                 dt.Course,
                                                 dt.Semester,
                                                 dt.[Payment Type],
                                                 dt.[Year Of Payment];");
        }









        //public List<CourseCategoryWiseStatistics> GetStudentStatistics(PrintProgramme printProgramme, short batch, short semester, Guid College_ID)
        //{
        //    printProgramme = new GeneralFunctions().MappingTable(printProgramme);
        //    var Query = $@"SELECT * FROM (
        //                    SELECT CM.Course_ID,CourseFullName,S.Gender,COUNT(S.Student_ID) GenderCount
        //                    FROM ARGPersonalInformation_{printProgramme.ToString()} S
        //                    JOIN ADMCollegeMaster College ON College.College_ID=S.AcceptCollege_ID
        //                    JOIN ARGSelectedCombination_{printProgramme.ToString()} Comb ON Comb.Semester={semester} AND Comb.Student_ID = S.Student_ID AND Comb.IsVerified=1
        //                    JOIN ADMCombinationMaster CM ON CM.Combination_ID = Comb.Combination_ID AND  CM.Semester = Comb.Semester
        //                    JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = CM.Course_ID
        //                    JOIN ARGStudentAddress_{printProgramme.ToString()} A ON A.Student_ID = S.Student_ID
        //                    WHERE S.AcceptCollege_ID='{College_ID}' AND Comb.SemesterBatch={batch} AND Comb.Semester <= S.CurrentSemesterOrYear
        //                    GROUP BY CM.Course_ID,CourseFullName,S.Gender
        //                    ) SourceQuery
        //                    PIVOT
        //                    (
        //                        SUM(GenderCount)
        //                        FOR Gender IN ([MALE], [FEMALE])
        //                    ) FinalQuery
        //                    ORDER BY FinalQuery.CourseFullName;";
        //    return new MSSQLFactory().GetObjectList<CourseCategoryWiseStatistics>(Query);
        //}

        //public List<StatisticsGenderWise> GenderWise(PrintProgramme programme, short batch, short semester)
        //{
        //    string Query = $@"SELECT * FROM (
        //                        SELECT STDINFO.Batch,STDINFO.Gender,COUNT(*) [Total Students]
        //                        FROM 
        //                        (
        //                         SELECT STD.Batch,Student_ID,STD.Gender FROM ARGPersonalInformation_UG STD
        //                         UNION ALL
        //                         SELECT STD.Batch,Student_ID,STD.Gender FROM ARGPersonalInformation_IH STD
        //                         UNION ALL
        //                         SELECT STD.Batch,Student_ID,STD.Gender FROM ARGPersonalInformation_PG STD
        //                        ) STDINFO
        //                        JOIN 
        //                        (
        //                         SELECT Student_ID,Combination_ID from	ARGSelectedCombination_UG COMB WHERE COMB.Semester=1
        //                         UNION ALL
        //                         SELECT Student_ID,Combination_ID from	ARGSelectedCombination_IH COMB WHERE COMB.Semester=1
        //                         UNION ALL
        //                         SELECT Student_ID,Combination_ID from	ARGSelectedCombination_PG COMB WHERE COMB.Semester=1
        //                        )COMB ON COMB.Student_ID = STDINFO.Student_ID
        //                        JOIN ADMCombinationMaster CM ON CM.Combination_ID = COMB.Combination_ID
        //                        JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = CM.Course_ID
        //                        GROUP BY  STDINFO.Batch,STDINFO.Gender
        //                        ) SourceQuery
        //                        PIVOT
        //                        (
        //                            MAX([Total Students]) FOR Gender IN ([Male],[Female])
        //                        ) PivotedData
        //                        Order By Batch";
        //    return new MSSQLFactory().GetObjectList<StatisticsGenderWise>(Query);
        //}

        //public List<StatisticsProgramGenderWise> ProgramGenderWise(PrintProgramme programme, short batch, short semester)
        //{
        //    string Query = $@"SELECT * FROM (
        //                        SELECT STDINFO.Batch,PrintProgramme,Programme,STDINFO.Gender,COUNT(*) [Total Students]
        //                        FROM 
        //                        (
        //                         SELECT STD.Batch,Student_ID,STD.Gender FROM ARGPersonalInformation_UG STD
        //                         UNION ALL
        //                         SELECT STD.Batch,Student_ID,STD.Gender FROM ARGPersonalInformation_IH STD
        //                         UNION ALL
        //                         SELECT STD.Batch,Student_ID,STD.Gender FROM ARGPersonalInformation_PG STD
        //                        ) STDINFO
        //                        JOIN 
        //                        (
        //                         SELECT Student_ID,Combination_ID from	ARGSelectedCombination_UG COMB WHERE COMB.Semester=1
        //                         UNION ALL
        //                         SELECT Student_ID,Combination_ID from	ARGSelectedCombination_IH COMB WHERE COMB.Semester=1
        //                         UNION ALL
        //                         SELECT Student_ID,Combination_ID from	ARGSelectedCombination_PG COMB WHERE COMB.Semester=1
        //                        )COMB ON COMB.Student_ID = STDINFO.Student_ID
        //                        JOIN ADMCombinationMaster CM ON CM.Combination_ID = COMB.Combination_ID
        //                        JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = CM.Course_ID
        //                        GROUP BY  STDINFO.Batch,PrintProgramme,Programme,STDINFO.Gender
        //                        ) SourceQuery
        //                        PIVOT
        //                        (
        //                            MAX([Total Students]) FOR Gender IN ([Male],[Female])
        //                        ) PivotedData
        //                        ORDER BY Batch,PrintProgramme,Programme";
        //    return new MSSQLFactory().GetObjectList<StatisticsProgramGenderWise>(Query);
        //}

        //public List<StatisticsProgramCategoryGenderWise> ProgramCategoryGenderWise(PrintProgramme programme, short batch, short semester)
        //{
        //    string Query = $@"SELECT * FROM (
        //                        SELECT STDINFO.Batch,PrintProgramme,Programme,STDINFO.Gender,STDINFO.CategoryCode,CategoryName,COUNT(*) [Total Students]
        //                        FROM 
        //                        (
        //                            SELECT STD.Batch,Student_ID,STD.Gender,STD.Category CategoryCode FROM ARGPersonalInformation_UG STD
        //                            UNION ALL
        //                            SELECT STD.Batch,Student_ID,STD.Gender,STD.Category CategoryCode FROM ARGPersonalInformation_IH STD
        //                            UNION ALL
        //                            SELECT STD.Batch,Student_ID,STD.Gender,STD.Category CategoryCode FROM ARGPersonalInformation_PG STD
        //                        ) STDINFO
        //                        JOIN 
        //                        (
        //                            SELECT Student_ID,Combination_ID from	ARGSelectedCombination_UG COMB WHERE COMB.Semester=1
        //                            UNION ALL
        //                            SELECT Student_ID,Combination_ID from	ARGSelectedCombination_IH COMB WHERE COMB.Semester=1
        //                            UNION ALL
        //                            SELECT Student_ID,Combination_ID from	ARGSelectedCombination_PG COMB WHERE COMB.Semester=1
        //                        )COMB ON COMB.Student_ID = STDINFO.Student_ID
        //                        JOIN ADMCombinationMaster CM ON CM.Combination_ID = COMB.Combination_ID
        //                        JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = CM.Course_ID
        //                        LEFT JOIN CategoryMaster ON CategoryMaster.CategoryCode=STDINFO.CategoryCode
        //                        GROUP BY  STDINFO.Batch,PrintProgramme,Programme,STDINFO.Gender,STDINFO.CategoryCode,CategoryName
        //                        ) SourceQuery
        //                        PIVOT
        //                        (
        //                            MAX([Total Students]) FOR Gender IN ([Male],[Female])
        //                        ) PivotedData
        //                        ORDER BY Batch,PivotedData.PrintProgramme,PivotedData.Programme,PivotedData.CategoryCode";
        //    return new MSSQLFactory().GetObjectList<StatisticsProgramCategoryGenderWise>(Query);
        //}

        //public List<StatisticsCategoryGenderWise> CategoryGender(PrintProgramme programme, short batch, short semester)
        //{
        //    string Query = $@"SELECT * FROM (
        //                        SELECT STDINFO.Batch,STDINFO.Gender,STDINFO.CategoryCode,CategoryName,COUNT(*) [Total Students]
        //                        FROM 
        //                        (
        //                            SELECT STD.Batch,Student_ID,STD.Gender,STD.Category CategoryCode FROM ARGPersonalInformation_UG STD
        //                            UNION ALL
        //                            SELECT STD.Batch,Student_ID,STD.Gender,STD.Category CategoryCode FROM ARGPersonalInformation_IH STD
        //                            UNION ALL
        //                            SELECT STD.Batch,Student_ID,STD.Gender,STD.Category CategoryCode FROM ARGPersonalInformation_PG STD
        //                        ) STDINFO
        //                        JOIN 
        //                        (
        //                            SELECT Student_ID,Combination_ID from	ARGSelectedCombination_UG COMB WHERE COMB.Semester=1
        //                            UNION ALL
        //                            SELECT Student_ID,Combination_ID from	ARGSelectedCombination_IH COMB WHERE COMB.Semester=1
        //                            UNION ALL
        //                            SELECT Student_ID,Combination_ID from	ARGSelectedCombination_PG COMB WHERE COMB.Semester=1
        //                        )COMB ON COMB.Student_ID = STDINFO.Student_ID
        //                        JOIN ADMCombinationMaster CM ON CM.Combination_ID = COMB.Combination_ID
        //                        JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = CM.Course_ID
        //                        LEFT JOIN CategoryMaster ON CategoryMaster.CategoryCode=STDINFO.CategoryCode
        //                        GROUP BY  STDINFO.Batch,STDINFO.Gender,STDINFO.CategoryCode,CategoryName
        //                        ) SourceQuery
        //                        PIVOT
        //                        (
        //                            MAX([Total Students]) FOR Gender IN ([Male],[Female])
        //                        ) PivotedData
        //                        ORDER BY PivotedData.Batch,PivotedData.CategoryCode";
        //    return new MSSQLFactory().GetObjectList<StatisticsCategoryGenderWise>(Query);
        //}


        //public List<CategoryStatistics> GetCourseCategories(PrintProgramme printProgramme, Guid Course_ID, short semester, short SemesterBatch, Guid College_ID)
        //{
        //    printProgramme = new GeneralFunctions().MappingTable(printProgramme);
        //    var Query = $@"SELECT FinalQuery.Category CategoryName,FinalQuery.FEMALE,FinalQuery.MALE
        //                FROM
        //                (
        //                    SELECT S.Category,S.Gender,COUNT(S.Student_ID) NoOfStudents
        //                    FROM ARGPersonalInformation_{printProgramme.ToString()} S
        //                 JOIN ARGSelectedCombination_{printProgramme.ToString()} Comb ON Comb.Student_ID = S.Student_ID AND COMB.IsVerified=1
        //                 JOIN ADMCombinationMaster CM ON CM.Semester = Comb.Semester AND CM.Combination_ID = Comb.Combination_ID
        //                 WHERE S.AcceptCollege_ID = '{College_ID}' AND  CM.Course_ID = '{Course_ID}'  
        //                 AND  COMB.Semester={semester} AND Comb.SemesterBatch={SemesterBatch}  AND COMB.Semester <= S.CurrentSemesterOrYear
        //                 GROUP BY S.Category,S.Gender
        //                ) SourceQuery
        //                PIVOT
        //                (
        //                    SUM(NoOfStudents)
        //                    FOR Gender IN ([MALE], [FEMALE])
        //                ) FinalQuery
        //                ORDER BY CategoryName;";
        //    return new MSSQLFactory().GetObjectList<CategoryStatistics>(Query);
        //}

        //public List<StudentStatisticsSummary> GetStudentStatistics2(Parameters parameter, string groupBy)
        //{
        //    return new StudentStatisticsSQLQueries().GetStudentStatistics2(parameter, groupBy);
        //}

        //public List<DistrictStatistics> GetCourseDistrictWiseStatistics(PrintProgramme printProgramme, short batch, short semester, Guid College_ID)
        //{
        //    printProgramme = new GeneralFunctions().MappingTable(printProgramme);
        //    var Query = $@"SELECT FinalQuery.Course_ID,FinalQuery.CourseFullName,FinalQuery.District,FinalQuery.MALE,FinalQuery.FEMALE
        //                    FROM
        //                    (
        //                        SELECT ADMCourseMaster.CourseFullName,CM.Course_ID,
        //                         A.District,
        //                        UPPER(S.Gender) Gender,
        //                        COUNT(S.Student_ID) NoOfStudents
        //                     FROM ARGPersonalInformation_{printProgramme} S
        //                     JOIN ARGSelectedCombination_{printProgramme} Comb ON COMB.Student_ID=S.Student_ID AND Comb.IsVerified=1
        //                     JOIN ADMCombinationMaster CM ON Comb.Semester=CM.Semester AND COMB.Combination_ID = CM.Combination_ID
        //                        JOIN ADMCourseMaster  ON ADMCourseMaster.Course_ID = CM.Course_ID    
        //                        INNER JOIN ARGStudentAddress_{printProgramme} A ON A.Student_ID = S.Student_ID
        //                     WHERE S.AcceptCollege_ID ='{College_ID}' AND Comb.SemesterBatch = {batch} 
        //                        AND COMB.Semester = {semester} AND COMB.Semester <= S.CurrentSemesterOrYear
        //                        GROUP BY ADMCourseMaster.CourseFullName,CM.Course_ID,A.District,S.Gender
        //                    ) SourceQuery
        //                    PIVOT
        //                    (
        //                        SUM(NoOfStudents)
        //                        FOR Gender IN ([MALE], [FEMALE])
        //                    ) FinalQuery
        //                    ORDER BY FinalQuery.CourseFullName,FinalQuery.District ";
        //    return new MSSQLFactory().GetObjectList<DistrictStatistics>(Query);
        //}

        //public List<CategoryStatistics> GetCourseDistrictStatistics(PrintProgramme printProgramme, Guid course_ID, string district, short semester, short Batch, Guid College_ID)
        //{
        //    printProgramme = new GeneralFunctions().MappingTable(printProgramme);
        //    var Query = $@" SELECT FinalQuery.CategoryName,FinalQuery.MALE,FinalQuery.FEMALE
        //                FROM
        //                (
        //                    SELECT S.Category AS CategoryName,
        //                            UPPER(S.Gender) Gender,
        //                            COUNT(S.Student_ID) NoOfStudents
        //                        FROM ARGPersonalInformation_{printProgramme.ToString()} S
        //               JOIN ARGSelectedCombination_{printProgramme.ToString()} Comb ON Comb.Student_ID = S.Student_ID AND Comb.IsVerified=1
        //               JOIN ADMCombinationMaster CM ON CM.Semester = Comb.Semester AND CM.Combination_ID = Comb.Combination_ID
        //               JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = CM.Course_ID
        //               JOIN ARGStudentAddress_{printProgramme.ToString()} A ON A.Student_ID = S.Student_ID
        //               WHERE A.District = '{district}' AND CM.Course_ID = '{course_ID}' 
        //               AND S.AcceptCollege_ID = '{College_ID}'  AND Comb.SemesterBatch = {Batch} AND COMB.Semester = {semester} AND COMB.Semester <= S.CurrentSemesterOrYear
        //                    GROUP BY S.Category,S.Gender
        //                ) SourceQuery
        //                PIVOT
        //                (
        //                    SUM(NoOfStudents)
        //                    FOR Gender IN ([MALE], [FEMALE])
        //                ) FinalQuery ";
        //    return new MSSQLFactory().GetObjectList<CategoryStatistics>(Query);
        //}

        public List<ARGPersonalInformation> DownloadPhotographs(short year, PrintProgramme programmeId, Guid courseID, short semester, Guid College_ID)
        {
            programmeId = new GeneralFunctions().MappingTable(programmeId);
            return new MSSQLFactory()
                .GetObjectList<ARGPersonalInformation>($@"SELECT P.Photograph,P.StudentFormNo,ISNULL(CUSRegistrationNo,StudentFormNo) AS CUSRegistrationNo FROM ARGPersonalInformation_{programmeId.ToString()} P
                                                        JOIN ARGSelectedCombination_{programmeId.ToString()} SC ON SC.Student_ID = P.Student_ID AND SC.Semester={semester}
                                                        JOIN ADMCombinationMaster cm ON cm.Combination_ID = SC.Combination_ID AND cm.Semester ={semester}
                                                        WHERE P.AcceptCollege_ID='{College_ID}' 
                                                        AND cm.Course_ID='{courseID}'
                                                        AND P.Batch={year} AND P.FormStatus NOT IN ({(short)FormStatus.InProcess},{(short)FormStatus.Rejected},{(short)FormStatus.Cancelled})");
        }

        public List<ReEvaluationStatistics> GetReEvaluationStatistics(Parameters parameters, PrintProgramme? PrintProgramme)
        {
            var helper = new GeneralFunctions().GetWhereClause<ReEvaluation>(parameters.Filters);
            helper.WhereClause = helper.WhereClause.Replace("WHERE", "AND");

            helper.Command.CommandText = $@"SELECT COUNT(ReEvaluation.Student_ID)NoOfStudents,SubmittedYear,FormType,Semester, SUM(FeeAmount) AmountReceived, MIN(ReEvaluation.CreatedOn) StartDate,
                                                                                       MAX(ReEvaluation.CreatedOn) EndDate, MAX(ReEvaluation.Notification_ID) Notification_ID,Link,Description NotificationID
                                                                                FROM ReEvaluation
                                                                                INNER JOIN ARGPersonalInformation_{(PrintProgramme)PrintProgramme} ON ARGPersonalInformation_{(PrintProgramme)PrintProgramme}.Student_ID = ReEvaluation.Student_ID
                                                                                INNER JOIN Notification ON Notification.Notification_ID = ReEvaluation.Notification_ID
                                                                                WHERE  ReEvaluation.FormStatus = {(short)FormStatus.PaymentSuccessful} {helper.WhereClause}
                                                                                GROUP BY SubmittedYear, Semester,Link,Description,FormType
                                                                                ORDER BY StartDate DESC";

            return new MSSQLFactory().GetObjectList<ReEvaluationStatistics>(helper.Command);
        }

        public List<PaymentStatistics> GetPaymentStatistics(DateTime? dateFrom, DateTime? dateTo, PaymentType? paymentType)
        {
            SqlCommand command = new SqlCommand();
            Parameters parameters = new Parameters() { Filters = new List<SearchFilter>() };
            if (dateFrom != null)
            {
                parameters.Filters.Add(new SearchFilter()
                {
                    Column = nameof(PaymentDetails.TxnDate),
                    Operator = SQLOperator.GreaterThanEqualTo,
                    Value = dateFrom.ToString()
                });
            }

            if (dateTo != null)
            {
                parameters.Filters.Add(new SearchFilter()
                {
                    Column = nameof(PaymentDetails.TxnDate),
                    Operator = SQLOperator.LessThanEqualTo,
                    Value = dateTo.ToString()
                });
            }

            if (paymentType != null)
            {
                parameters.Filters.Add(new SearchFilter()
                {
                    Column = nameof(PaymentDetails.PaymentType),
                    Operator = SQLOperator.EqualTo,
                    Value = ((short)paymentType.Value).ToString()
                });

                if (paymentType.Value == PaymentType.Online)
                {
                    parameters.Filters.Add(new SearchFilter()
                    {
                        Column = nameof(PaymentDetails.PaymentType),
                        Operator = SQLOperator.EqualTo,
                        Value = ((short)PaymentType.Reconciled).ToString(),
                        IsSibling = true,
                        GroupOperation = LogicalOperator.OR
                    });
                    parameters.Filters.Add(new SearchFilter()
                    {
                        Column = nameof(PaymentDetails.PaymentType),
                        Operator = SQLOperator.EqualTo,
                        Value = ((short)PaymentType.ReFund).ToString(),
                        IsSibling = true,
                        GroupOperation = LogicalOperator.OR
                    });
                }
            }

            var helper = new GeneralFunctions().GetWhereClause<PaymentDetails>(parameters.Filters);
            command = helper.Command;
            string query = $@"SELECT PaymentDetails.ModuleType,SUM(TxnAmount) AmountReceived,Count(TxnReferenceNo) as TotalForms
                                FROM 
                                ( 
	                                SELECT TxnReferenceNo,TxnAmount,ModuleType,PaymentType,CAST(TxnDate AS DATE) TxnDate FROM PaymentDetails_UG
	                                UNION 
	                                SELECT TxnReferenceNo,TxnAmount,ModuleType,PaymentType,CAST(TxnDate AS DATE) TxnDate FROM PaymentDetails_IH
	                                UNION 
	                                SELECT TxnReferenceNo,TxnAmount,ModuleType,PaymentType,CAST(TxnDate AS DATE) TxnDate FROM PaymentDetails_PG
                                )
                                PaymentDetails
                                {helper.WhereClause}
                                GROUP BY PaymentDetails.ModuleType
                                ORDER BY PaymentDetails.ModuleType";

            command.CommandText = query;

            return new MSSQLFactory().GetObjectList<PaymentStatistics>(command);
        }

        public List<AdmissionPaymentStatistics> GetAdmissionPaymentStatistics(DateTime? dateFrom, DateTime? dateTo)
        {
            SqlCommand command = new SqlCommand();
            Parameters parameters = new Parameters() { Filters = new List<SearchFilter>() };
            if (dateFrom != null)
            {
                parameters.Filters.Add(new SearchFilter()
                {
                    Column = nameof(PaymentDetails.TxnDate),
                    Operator = SQLOperator.GreaterThanEqualTo,
                    Value = dateFrom.ToString()
                });
            }

            if (dateTo != null)
            {
                parameters.Filters.Add(new SearchFilter()
                {
                    Column = nameof(PaymentDetails.TxnDate),
                    Operator = SQLOperator.LessThanEqualTo,
                    Value = dateTo.ToString()
                });
            }

            var helper = new GeneralFunctions().GetWhereClause<PaymentDetails>(parameters.Filters);
            command = helper.Command;
            string query = $@"SELECT Students.Programme, SUM(TxnAmount) AmountReceived
                                    FROM
                                    (
                                        SELECT Entity_ID, Payment_ID, TxnAmount, ModuleType, CAST(TxnDate AS DATE) TxnDate
                                        FROM PaymentDetails_UG
                                        UNION
                                        SELECT Entity_ID, Payment_ID, TxnAmount, ModuleType, CAST(TxnDate AS DATE) TxnDate
                                        FROM PaymentDetails_IH
                                        UNION
                                        SELECT Entity_ID, Payment_ID, TxnAmount, ModuleType, CAST(TxnDate AS DATE) TxnDate
                                        FROM PaymentDetails_PG
                                    ) PaymentDetails
                                        INNER JOIN
                                        (
                                            SELECT Student_ID, 1 AS Programme
                                            FROM ARGPersonalInformation_UG
                                            UNION
                                            SELECT Student_ID, 2 AS Programme
                                            FROM ARGPersonalInformation_PG
                                            UNION
                                            SELECT Student_ID, 3 AS Programme
                                            FROM ARGPersonalInformation_IH
                                        ) Students
                                            ON Students.Student_ID = PaymentDetails.Entity_ID AND PaymentDetails.ModuleType = {(short)PaymentModuleType.Admission}
                                    {helper.WhereClause}                                    
                                    GROUP BY Students.Programme
                                    ORDER BY Students.Programme";
            command.CommandText = query;

            return new MSSQLFactory().GetObjectList<AdmissionPaymentStatistics>(command);
        }

        public DataTable GetIndividualAdmissionPayments(DateTime fromDate, DateTime toDate, PaymentType? paymentType)
        {
            string pTypes = "";
            if (paymentType != null)
            {
                if (paymentType.Value == PaymentType.Online)
                {
                    pTypes = $" AND PaymentType IN(${(short)PaymentType.Online},{(short)PaymentType.Reconciled},{(short)PaymentType.ReFund}) ";
                }
                else
                    pTypes = $" AND PaymentType IN(${(short)paymentType}) ";
            }

            string SQLQuery = $@"SELECT  * FROM (
                                    SELECT  DISTINCT
                                    CASE WHEN PaymentDetails_IH.Semester=1 THEN
                                    'Semester 1 & 2 Admission'
                                     WHEN PaymentDetails_IH.Semester=3 THEN
                                    'Semester 3 & 4 Admission'
                                     WHEN PaymentDetails_IH.Semester=5 THEN
                                    'Semester 5 & 6 Admission'
                                     WHEN PaymentDetails_IH.Semester=7 THEN
                                    'Semester 7 & 8 Admission'
                                     WHEN PaymentDetails_IH.Semester=9 THEN
                                    'Semester 9 & 10 Admission'
                                    ELSE
                                    CAST(PaymentDetails_IH.Semester AS VARCHAR(10))
                                    END Semester,
                                           SemesterBatch Batch,
	                                       CollegeFullName,
	                                       StudentFormNo,CUSRegistrationNo,FullName,PhoneNumber,
                                           TxnAmount,
	                                       TxnDate,
                                           CourseFullName,
                                           CASE
                                               WHEN dbo.PaymentDetails_IH.PaymentType = 1 or dbo.PaymentDetails_IH.PaymentType = 3 THEN
                                                   'Online'
                                               WHEN dbo.PaymentDetails_IH.PaymentType = 2 THEN
                                                   'offline'
			                                       ELSE
                                                   'offline'
                                           END PaymentType
                                    FROM dbo.ARGPersonalInformation_IH
                                        JOIN dbo.PaymentDetails_IH
                                            ON Entity_ID = Student_ID
                                        LEFT JOIN dbo.ARGSelectedCombination_IH
                                            ON ARGSelectedCombination_IH.Student_ID = ARGPersonalInformation_IH.Student_ID
                                        LEFT JOIN dbo.ADMCombinationMaster
                                            ON ADMCombinationMaster.Combination_ID = ARGSelectedCombination_IH.Combination_ID		
                                        JOIN dbo.ADMCourseMaster
                                            ON ADMCourseMaster.Course_ID = ADMCombinationMaster.Course_ID
		                                    JOIN dbo.ADMCollegeMaster ON ADMCollegeMaster.College_ID = ADMCombinationMaster.College_ID
                                    WHERE ModuleType = {(short)PaymentModuleType.SemesterAdmission}  AND (CAST(TxnDate AS DATE) BETWEEN '{fromDate.ToString("yyyy-MM-dd")}' AND  '{toDate.ToString("yyyy-MM-dd")}') {pTypes}
                            UNION
                            SELECT  DISTINCT
                                    CASE WHEN PaymentDetails_UG.Semester=1 THEN
                                    'Semester 1 & 2 Admission'
                                     WHEN PaymentDetails_UG.Semester=3 THEN
                                    'Semester 3 & 4 Admission'
                                     WHEN PaymentDetails_UG.Semester=5 THEN
                                    'Semester 5 & 6 Admission'
                                     WHEN PaymentDetails_UG.Semester=7 THEN
                                    'Semester 7 & 8 Admission'
                                     WHEN PaymentDetails_UG.Semester=9 THEN
                                    'Semester 9 & 10 Admission'
                                    ELSE
                                    CAST(PaymentDetails_UG.Semester AS VARCHAR(10))
                                    END Semester,
                                           SemesterBatch Batch,
	                                       CollegeFullName,
	                                       StudentFormNo,CUSRegistrationNo,FullName,PhoneNumber,
                                           TxnAmount,
	                                       TxnDate,
                                           CourseFullName,
                                           CASE
                                               WHEN dbo.PaymentDetails_UG.PaymentType = 1 or dbo.PaymentDetails_UG.PaymentType = 3 THEN
                                                   'Online'
                                               WHEN dbo.PaymentDetails_UG.PaymentType = 2 THEN
                                                   'offline'
			                                       ELSE
                                                   'offline'
                                           END PaymentType
                                    FROM dbo.ARGPersonalInformation_UG
                                        JOIN dbo.PaymentDetails_UG
                                            ON Entity_ID = Student_ID
                                        LEFT JOIN dbo.ARGSelectedCombination_UG
                                            ON ARGSelectedCombination_UG.Student_ID = ARGPersonalInformation_UG.Student_ID
                                        LEFT JOIN dbo.ADMCombinationMaster
                                            ON ADMCombinationMaster.Combination_ID = ARGSelectedCombination_UG.Combination_ID		
                                        JOIN dbo.ADMCourseMaster
                                            ON ADMCourseMaster.Course_ID = ADMCombinationMaster.Course_ID
		                                    JOIN dbo.ADMCollegeMaster ON ADMCollegeMaster.College_ID = ADMCombinationMaster.College_ID
                                    WHERE ModuleType = {(short)PaymentModuleType.SemesterAdmission}  AND (CAST(TxnDate AS DATE) BETWEEN '{fromDate.ToString("yyyy-MM-dd")}' AND  '{toDate.ToString("yyyy-MM-dd")}') {pTypes}
                            UNION
                            SELECT  DISTINCT
                                    CASE WHEN PaymentDetails_PG.Semester=1 THEN
                                    'Semester 1 & 2 Admission'
                                     WHEN PaymentDetails_PG.Semester=3 THEN
                                    'Semester 3 & 4 Admission'
                                     WHEN PaymentDetails_PG.Semester=5 THEN
                                    'Semester 5 & 6 Admission'
                                     WHEN PaymentDetails_PG.Semester=7 THEN
                                    'Semester 7 & 8 Admission'
                                     WHEN PaymentDetails_PG.Semester=9 THEN
                                    'Semester 9 & 10 Admission'
                                    ELSE
                                    CAST(PaymentDetails_PG.Semester AS VARCHAR(10))
                                    END Semester,
                                           SemesterBatch Batch,
	                                       CollegeFullName,
	                                       StudentFormNo,CUSRegistrationNo,FullName,PhoneNumber,
                                           TxnAmount,
	                                       TxnDate,
                                           CourseFullName,
                                           CASE
                                               WHEN dbo.PaymentDetails_PG.PaymentType = 1 or dbo.PaymentDetails_PG.PaymentType = 3 THEN
                                                   'Online'
                                               WHEN dbo.PaymentDetails_PG.PaymentType = 2 THEN
                                                   'offline'
			                                       ELSE
                                                   'offline'
                                           END PaymentType
                                    FROM dbo.ARGPersonalInformation_PG
                                        JOIN dbo.PaymentDetails_PG
                                            ON Entity_ID = Student_ID
                                        LEFT JOIN dbo.ARGSelectedCombination_PG
                                            ON ARGSelectedCombination_PG.Student_ID = ARGPersonalInformation_PG.Student_ID
                                        LEFT JOIN dbo.ADMCombinationMaster
                                            ON ADMCombinationMaster.Combination_ID = ARGSelectedCombination_PG.Combination_ID		
                                        JOIN dbo.ADMCourseMaster
                                            ON ADMCourseMaster.Course_ID = ADMCombinationMaster.Course_ID
		                                    JOIN dbo.ADMCollegeMaster ON ADMCollegeMaster.College_ID = ADMCombinationMaster.College_ID
                                    WHERE ModuleType = {(short)PaymentModuleType.SemesterAdmission}  AND (CAST(TxnDate AS DATE) BETWEEN '{fromDate.ToString("yyyy-MM-dd")}' AND  '{toDate.ToString("yyyy-MM-dd")}') {pTypes}
                            ) s
                                    
                            ORDER BY CollegeFullName,CourseFullName,
                            s.Semester,
                            s.Batch,
                            PaymentType,TxnAmount;";

            return new MSSQLFactory().GetDataTable(SQLQuery);
        }
    }
}
