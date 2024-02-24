using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneralModels;
using CUSrinagar.Models;
using System.Data.SqlClient;
using Terex;
using CUSrinagar.Enums;

namespace CUSrinagar.DataManagers.SQLQueries
{
    public class StudentStatisticsSQLQueries
    {
        string College_IDPart = AppUserHelper.AppUsercompact.UserRoles.Contains(AppRoles.University) ? " AND ARGPersonalInformation.AcceptCollege_ID IS NOT NULL " : $" AND ARGPersonalInformation.AcceptCollege_ID = '{AppUserHelper.College_ID}'";
        
        internal string GetStudentStatistics(PrintProgramme printProgramme, short batch, short semester, bool DistrictWise, Guid College_ID)
        {
            string combinationPart = string.Empty;
            string addressPart = string.Empty;
            string personalInformationPart = string.Empty;
            var Query = "";
            if (DistrictWise)
            {
                
            }
            else
            {
                Query = $@"SELECT * FROM (
                            SELECT CM.Course_ID,CourseFullName,S.Gender,COUNT(S.Student_ID) GenderCount
                            FROM ARGPersonalInformation_{printProgramme.ToString()} S
                            JOIN ADMCollegeMaster College ON College.College_ID=S.AcceptCollege_ID
                            JOIN ARGSelectedCombination_{printProgramme.ToString()} Comb ON Comb.Student_ID = S.Student_ID
                            JOIN ADMCombinationMaster CM ON CM.Combination_ID = Comb.Combination_ID AND  CM.Semester = Comb.Semester
                            JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = CM.Course_ID
                            JOIN ARGStudentAddress_{printProgramme.ToString()} A ON A.Student_ID = S.Student_ID
                            WHERE S.AcceptCollege_ID='{College_ID}' AND Comb.SemesterBatch={batch} AND Comb.Semester={semester}
                            GROUP BY CM.Course_ID,CourseFullName,S.Gender
                            ) SourceQuery
                            PIVOT
                            (
                                SUM(GenderCount)
                                FOR Gender IN ([MALE], [FEMALE])
                            ) FinalQuery
                            ORDER BY FinalQuery.CourseFullName;";
                return Query;
            }

            switch (printProgramme)
            {
                case PrintProgramme.UG:
                case PrintProgramme.BED:
                    combinationPart = " SELECT Student_ID,Semester,Combination_ID FROM ARGSelectedCombination_UG";
                    addressPart = " SELECT * FROM ARGStudentAddress_UG ";
                    personalInformationPart = "    SELECT Gender,Category,Student_ID,Batch,CurrentSemesterOrYear,AcceptCollege_ID FROM ARGPersonalInformation_UG ";
                    break;
                case PrintProgramme.PG:
                    combinationPart = "SELECT Student_ID,Semester,Combination_ID FROM ARGSelectedCombination_PG";
                    addressPart = " SELECT * FROM ARGStudentAddress_PG ";
                    personalInformationPart = "    SELECT Gender,Category,Student_ID,Batch,CurrentSemesterOrYear,AcceptCollege_ID FROM ARGPersonalInformation_PG ";
                    break;
                case PrintProgramme.IH:
                    combinationPart = "SELECT Student_ID,Semester,Combination_ID FROM ARGSelectedCombination_IH";
                    addressPart = " SELECT * FROM ARGStudentAddress_IH ";
                    personalInformationPart = "    SELECT Gender,Category,Student_ID,Batch,CurrentSemesterOrYear,AcceptCollege_ID FROM ARGPersonalInformation_IH ";
                    break;


            }


            return DistrictWise ? GetDistrictWiseQuery(combinationPart, addressPart, personalInformationPart, batch, semester, College_ID) 
                : GetCategoryWiseQuery(combinationPart, addressPart, personalInformationPart, batch, semester,College_ID);

        }

        internal string GetCourseDistrictStatistics(PrintProgramme programme, Guid course_ID, string district, short semester, short Batch,Guid College_ID)
        {
            string combinationPart = string.Empty;
            string addressPart = string.Empty;
            string personalInformationPart = string.Empty;


            switch (programme)
            {
                case PrintProgramme.UG:
                case PrintProgramme.BED:
                    combinationPart = " SELECT Student_ID,Semester,Combination_ID FROM ARGSelectedCombination_UG";
                    addressPart = " SELECT * FROM ARGStudentAddress_UG ";
                    personalInformationPart = "    SELECT Gender,Category,Student_ID,Batch,CurrentSemesterOrYear,AcceptCollege_ID FROM ARGPersonalInformation_UG ";
                    break;
                case PrintProgramme.PG:
                    combinationPart = "SELECT Student_ID,Semester,Combination_ID FROM ARGSelectedCombination_PG";
                    addressPart = " SELECT * FROM ARGStudentAddress_PG ";
                    personalInformationPart = "    SELECT Gender,Category,Student_ID,Batch,CurrentSemesterOrYear,AcceptCollege_ID FROM ARGPersonalInformation_PG ";
                    break;
                case PrintProgramme.IH:
                    combinationPart = "SELECT Student_ID,Semester,Combination_ID FROM ARGSelectedCombination_IH";
                    addressPart = " SELECT * FROM ARGStudentAddress_IH ";
                    personalInformationPart = "    SELECT Gender,Category,Student_ID,Batch,CurrentSemesterOrYear,AcceptCollege_ID FROM ARGPersonalInformation_IH ";
                    break;                              
                
                
            }

            return $@" SELECT *
                            FROM
                            (
                                SELECT ARGPersonalInformation.Category AS CategoryName,
                                       UPPER(ARGPersonalInformation.Gender) Gender,
                                       COUNT(ARGPersonalInformation.Student_ID) NoOfStudents
                                FROM
                                (
                                    {personalInformationPart}
                                ) ARGPersonalInformation
                                    INNER JOIN
                                    (
                                        {addressPart}
                                    ) ARGStudentAddress
                                        ON ARGStudentAddress.Student_ID = ARGPersonalInformation.Student_ID
                                    INNER JOIN
                                    (
                                        {combinationPart}
                                    ) COMB
                                        ON COMB.Student_ID = ARGPersonalInformation.Student_ID               
                                    INNER JOIN ADMCombinationMaster
                                        ON COMB.Combination_ID = ADMCombinationMaster.Combination_ID
                                    INNER JOIN ADMCourseMaster
                                        ON ADMCourseMaster.Course_ID = ADMCombinationMaster.Course_ID    
			                            WHERE ARGStudentAddress.District = '{district}' AND ADMCombinationMaster.Course_ID = '{course_ID}' AND ARGPersonalInformation.AcceptCollege_ID = '{College_ID}'  AND ARGPersonalInformation.Batch = {Batch} AND COMB.Semester = {semester}
                                GROUP BY ARGPersonalInformation.Category,             
                                         ARGPersonalInformation.Gender
                            ) SourceQuery
                            PIVOT
                            (
                                SUM(NoOfStudents)
                                FOR Gender IN ([MALE], [FEMALE])
                            ) FinalQuery ";
        }

        private string GetDistrictWiseQuery(string combinationPart, string addressPart, string personalInformationPart, short batch, short semester,Guid College_ID)
        {
            return $@" SELECT *
                            FROM
                            (
                                SELECT ADMCourseMaster.CourseFullName,ADMCombinationMaster.Course_ID,
                                       ARGStudentAddress.District,
                                       UPPER(ARGPersonalInformation.Gender) Gender,
                                       COUNT(ARGPersonalInformation.Student_ID) NoOfStudents
                                FROM
                                (
                                    {personalInformationPart}
                                ) ARGPersonalInformation
                                    INNER JOIN
                                    (
                                        {addressPart}
                                    ) ARGStudentAddress
                                        ON ARGStudentAddress.Student_ID = ARGPersonalInformation.Student_ID
                                    INNER JOIN
                                    (
                                        {combinationPart}
                                    ) COMB
                                        ON COMB.Student_ID = ARGPersonalInformation.Student_ID               
                                    INNER JOIN ADMCombinationMaster
                                        ON COMB.Combination_ID = ADMCombinationMaster.Combination_ID
                                    INNER JOIN ADMCourseMaster
                                        ON ADMCourseMaster.Course_ID = ADMCombinationMaster.Course_ID    
			                            WHERE ARGPersonalInformation.AcceptCollege_ID ='{College_ID}' AND ARGPersonalInformation.Batch = {batch} AND COMB.Semester = {semester}
                                GROUP BY ADMCourseMaster.CourseFullName,ADMCombinationMaster.Course_ID,
                                         ARGStudentAddress.District,
                                         ARGPersonalInformation.Gender
                            ) SourceQuery
                            PIVOT
                            (
                                SUM(NoOfStudents)
                                FOR Gender IN ([MALE], [FEMALE])
                            ) FinalQuery
                            ORDER BY FinalQuery.CourseFullName,FinalQuery.District ";
        }

        private string GetCategoryWiseQuery(string combinationPart, string addressPart, string personalInformationPart, short batch, short semester,Guid College_ID)
        {
            return $@"SELECT FinalQuery.CourseFullName EntityID,
                              FinalQuery.Course_ID Entity_ID,
                                       CAST(FinalQuery.MALE AS NVARCHAR(15)) MALE,
                                       CAST(FinalQuery.FEMALE AS NVARCHAR(15)) FEMALE,
                                       (ISNULL(FinalQuery.MALE,0) + ISNULL(FinalQuery.FEMALE,0)) Total
                                FROM
                                (
                                    SELECT CourseFullName,ADMCourseMaster.Course_ID,
                                           ISNULL(UPPER(ARGPersonalInformation.Gender), 'GRANDTOTAL') [Gender],
                                           COUNT(ARGPersonalInformation.Student_ID) ACount
                                    FROM
                                    (
                                        {personalInformationPart}
                                    ) ARGPersonalInformation
                                        JOIN ADMCollegeMaster
                                            ON ADMCollegeMaster.College_ID = ARGPersonalInformation.AcceptCollege_ID
                                        LEFT JOIN
                                        (
                                            {addressPart}
                                        ) ARGStudentAddress
                                            ON ARGStudentAddress.Student_ID = ARGPersonalInformation.Student_ID
                                        LEFT JOIN
                                        (
                                            {combinationPart}
                                        ) COMB
                                            ON COMB.Student_ID = ARGPersonalInformation.Student_ID                                               
                                        LEFT JOIN ADMCombinationMaster ON ADMCombinationMaster.Combination_ID = COMB.Combination_ID
                                        INNER JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = ADMCombinationMaster.Course_ID
                                    WHERE ARGPersonalInformation.AcceptCollege_ID = '{College_ID}' AND ARGPersonalInformation.Batch = {batch} AND COMB.Semester = {semester}
                                    GROUP BY ADMCourseMaster.CourseFullName,ADMCourseMaster.Course_ID,
                                             ARGPersonalInformation.Gender
                                ) SourceQuery
                                PIVOT
                                (
                                    SUM(ACount)
                                    FOR Gender IN ([MALE], [FEMALE])
                                ) FinalQuery;";
        }

        internal string GetCourseCategoryStatistics(Guid entity_ID, short semester, short Batch, Guid College_ID)
        {
            return $@"SELECT    
                            FinalQuery.Category CategoryName,
                            FinalQuery.FEMALE,
                            FinalQuery.MALE
                            FROM
                            (
                                SELECT ARGPersonalInformation.Category,
                                       UPPER(ARGPersonalInformation.Gender) Gender,
                                       COUNT(ARGPersonalInformation.Student_ID) NoOfStudents
                                FROM
                                (
                                    SELECT Gender,
                                           Category,
                                           Student_ID,
                                           Batch,
                                           CurrentSemesterOrYear,
                                           AcceptCollege_ID
                                    FROM ARGPersonalInformation_IH
                                    UNION
                                    SELECT Gender,
                                           Category,
                                           Student_ID,
                                           Batch,
                                           CurrentSemesterOrYear,
                                           AcceptCollege_ID
                                    FROM ARGPersonalInformation_PG
                                    UNION
                                    SELECT Gender,
                                           Category,
                                           Student_ID,
                                           Batch,
                                           CurrentSemesterOrYear,
                                           AcceptCollege_ID
                                    FROM ARGPersonalInformation_UG
                                ) ARGPersonalInformation
                                    INNER JOIN
                                    (
                                        SELECT Student_ID,
                                               Semester,
                                               Combination_ID
                                        FROM ARGSelectedCombination_IH
                                        UNION
                                        SELECT Student_ID,
                                               Semester,
                                               Combination_ID
                                        FROM ARGSelectedCombination_UG
                                        UNION
                                        SELECT Student_ID,
                                               Semester,
                                               Combination_ID
                                        FROM ARGSelectedCombination_PG
                                    ) COMB
                                        ON COMB.Student_ID = ARGPersonalInformation.Student_ID
                                    INNER JOIN ADMCombinationMaster
                                        ON COMB.Combination_ID = ADMCombinationMaster.Combination_ID
                                WHERE ADMCombinationMaster.Course_ID = '{entity_ID}'  AND ARGPersonalInformation.AcceptCollege_ID = '{College_ID}' AND COMB.Semester={semester} AND ARGPersonalInformation.Batch={Batch}
                                GROUP BY ARGPersonalInformation.Category,
                                         ARGPersonalInformation.Gender
                            ) SourceQuery
                            PIVOT
                            (
                                SUM(NoOfStudents)
                                FOR Gender IN ([MALE], [FEMALE])
                            ) FinalQuery;";
        }

        internal List<StudentStatisticsSummary> GetStudentStatistics2(Parameters parameter, string groupBy)
        {
            var whereClause = new GeneralFunctions().GetWhereClause<StudentStatisticsSummary>(parameter.Filters);

            string query = $@" SELECT {groupBy},
                                       COUNT(ARGPersonalInformation.Student_ID) [COUNT]
                                FROM
                                    (
                                        SELECT Gender,
                                               Student_ID,
                                               Batch, CurrentSemesterOrYear,
                                               AcceptCollege_ID
                                        FROM ARGPersonalInformation_IH
                                        UNION
                                        SELECT Gender,
                                               Student_ID,
                                               Batch, CurrentSemesterOrYear,
                                               AcceptCollege_ID
                                        FROM ARGPersonalInformation_PG
                                        UNION
                                        SELECT Gender,
                                               Student_ID,
                                               Batch, CurrentSemesterOrYear,
                                               AcceptCollege_ID
                                        FROM ARGPersonalInformation_UG
                                    ) ARGPersonalInformation
                                        JOIN ADMCollegeMaster
                                            ON ADMCollegeMaster.College_ID = ARGPersonalInformation.AcceptCollege_ID
                                        LEFT JOIN
                                        (
                                            SELECT*
                                            FROM ARGStudentAddress_IH
                                            UNION
                                            SELECT*
                                            FROM ARGStudentAddress_UG
                                            UNION
                                            SELECT*
                                            FROM ARGStudentAddress_PG
                                        ) ARGStudentAddress
                                            ON ARGStudentAddress.Student_ID = ARGPersonalInformation.Student_ID
                                        LEFT JOIN
                                        (
                                            SELECT Student_ID, Semester, Combination_ID FROM ARGSelectedCombination_IH
                                            UNION

                                            SELECT Student_ID, Semester, Combination_ID FROM ARGSelectedCombination_UG
                                            UNION

                                            SELECT Student_ID, Semester, Combination_ID FROM ARGSelectedCombination_PG

                                        ) COMB ON COMB.Student_ID = ARGPersonalInformation.Student_ID AND COMB.Semester = ARGPersonalInformation.CurrentSemesterOrYear
                                        LEFT JOIN ADMCombinationMaster ON ADMCombinationMaster.Combination_ID = COMB.Combination_ID
                                        LEFT JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = ADMCombinationMaster.Course_ID

                                {whereClause.WhereClause}
                                GROUP BY {groupBy} ";
            var orderByClause = new GeneralFunctions().GetPagedQuery(query, parameter);
            whereClause.Command.CommandText = query + orderByClause;
            return new MSSQLFactory().GetObjectList<StudentStatisticsSummary>(whereClause.Command);
        }
    }
}
