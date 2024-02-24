using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CUSrinagar.Enums;
using CUSrinagar.Models;

namespace CUSrinagar.DataManagers.SQLQueries
{
    public class WidgetsSQLQueries
    {
        internal string GetCourseSummaryWidget(PrintProgramme printProgramme)
        {
            string tableName = new GeneralFunctions().GetTableName(printProgramme, Module.PersonalInformation);
            string joinName = new GeneralFunctions().GetTableName(printProgramme, Module.SelectedCombination);
            string programmePart = string.Empty;
            switch (printProgramme)
            {
                case PrintProgramme.UG:
                    programmePart = " INNER JOIN ARGSelectedCombination_UG ON ARGSelectedCombination_UG.Student_ID = ARGPersonalInformation.Student_ID ";
                    break;
                case PrintProgramme.PG:
                    programmePart = " INNER JOIN ARGSelectedCombination_PG ON ARGSelectedCombination_PG.Student_ID = ARGPersonalInformation.Student_ID ";
                    break;
                case PrintProgramme.IH:
                    programmePart = " INNER JOIN ARGSelectedCombination_IH ON ARGSelectedCombination_IH.Student_ID = ARGPersonalInformation.Student_ID ";
                    break;
                case PrintProgramme.BED:
                    break;
                default:
                    break;
            }

            return $@" SELECT *
                            FROM
                            (
                                SELECT CourseFullName,
                                       UPPER(ARGPersonalInformation.Gender) Gender,
                                       COUNT(ARGPersonalInformation.Student_ID) Students
                                FROM {tableName}
                                    {programmePart}
                                    INNER JOIN ADMCombinationMaster
                                        ON ADMCombinationMaster.Combination_ID = {joinName}.Combination_ID
                                    INNER JOIN ADMCourseMaster
                                        ON ADMCourseMaster.Course_ID = ADMCombinationMaster.Course_ID
                                WHERE AcceptCollege_ID = '{AppUserHelper.College_ID}'
                                GROUP BY CourseFullName,
                                         ARGPersonalInformation.Gender
                            ) SourceQuery
                            PIVOT
                            (
                                SUM(Students)
                                FOR SourceQuery.Gender IN ([Male], [Female])
                            ) FinalQuery; ";
        }

        internal string GetGrievanceWidget()
        {
            return @" SELECT ISNULL(FinalQuery.[3],0) Resolved,
                                       ISNULL(FinalQuery.[2],0) Forwarded,
                                       ISNULL(FinalQuery.[1],0) Received
                                FROM
                                (
                                    SELECT Status,
                                           COUNT(Status) GrievanceStatus
                                    FROM Grievance
	                                WHERE IsNumberVerified = 1
                                    GROUP BY Status
                                ) SourceQuery
                                PIVOT
                                (
                                    MAX(GrievanceStatus)
                                    FOR Status IN ([1], [2], [3])
                                ) FinalQuery;  ";
        }

        internal string GetGrievanceCategoryCount()
        {
            return @"SELECT Final.[6] Other,
                           Final.[5] Result,
                           Final.[4] Examination,
                           Final.[3] Syllabus,
                           Final.[2] Admission,
                           Final.[1] TeachingClassWork
                    FROM
                    (
                        SELECT Category,
                               COUNT(Category) Grievances
                        FROM Grievance
                        WHERE IsNumberVerified = 1
                        GROUP BY Category
                    ) SourceQuery
                    PIVOT
                    (
                        SUM(Grievances)
                        FOR SourceQuery.Category IN ([1], [2], [3], [4], [5], [6])
                    ) Final";
        }
    }
}
