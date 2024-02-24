using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CUSrinagar.DataManagers.SQLQueries;
using CUSrinagar.Enums;
using CUSrinagar.Models;
using GeneralModels;
using Terex;

namespace CUSrinagar.DataManagers
{
    public class DashboardDB
    {
        public CourseSummaryWidget GetCourseSummaryWidget(PrintProgramme printProgramme)
        {
            return new MSSQLFactory().GetObject<CourseSummaryWidget>(new WidgetsSQLQueries().GetCourseSummaryWidget(printProgramme));
        }

        public GrievanceCategoryCount GetGrievanceCategoryCount()
        {
            return new MSSQLFactory().GetObject<GrievanceCategoryCount>(new WidgetsSQLQueries().GetGrievanceCategoryCount());
        }

        public GrievanceWidget GetGrievanceWidget()
        {
            return new MSSQLFactory().GetObject<GrievanceWidget>(new WidgetsSQLQueries().GetGrievanceWidget());
        }

        public List<MenuList> GetAllMenuItems()
        {
            return new MSSQLFactory().GetObjectList<MenuList>(@"SELECT * FROM MenuList WHERE ReturnType = 'ActionResult' OR ReturnType = 'ViewResult' AND Attributes LIKE 'HttpGet' OR Attributes = ''");
        }

        public List<ProgammeDashboard> GetProgrammeWiseCount()
        {
            var list = new StudentStatisticsDB().GetstudentsCourseWiseStatistics(new Parameters()
            {
                Filters = new List<SearchFilter>(){
                    new SearchFilter()
                    {
                        Column = "SemesterBatch",
                        Operator = SQLOperator.EqualTo,
                        Value = DateTime.Now.Year.ToString()
                    },new SearchFilter()
                    {
                        Column = "Semester",
                        Operator = SQLOperator.EqualTo,
                        Value = "1",TableAlias="Comb"
                    }
                }
            }) ?? new List<StudentsCourseWiseStatistics>();

            var programmes = list.GroupBy(i => i.Programme);

            List<ProgammeDashboard> programmesCount = new List<ProgammeDashboard>();

            foreach (var programme in programmes)
            {
                programmesCount.Add(new ProgammeDashboard()
                {
                    Programme = programme.Key.ToString(),
                    NoOfStudents = programme.Sum(i => i.NoOfStudents),
                    Batch = programme.Max(i => i.SemesterBatch)
                });
            }
            return programmesCount;
        }

        public List<CoursesDashboard> GetPassingCourses()
        {
            return new MSSQLFactory().GetObjectList<CoursesDashboard>(@"SELECT TOP 5 CourseFullName FROM IH_Semester1 
                                                                                INNER JOIN VW_SubjectWithStructure S ON S.Subject_ID = IH_Semester1.Subject_ID
                                                                                INNER JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = S.Course_ID
                                                                                WHERE ExternalMarks > '21' AND CourseFullName NOT LIKE '%Generic%'
                                                                                GROUP BY CourseFullName
                                                                                ORDER BY COUNT(*) DESC");
        }

        public List<CoursesDashboard> GetFailingCourses()
        {
            return new MSSQLFactory().GetObjectList<CoursesDashboard>(@"SELECT TOP 5 CourseFullName FROM IH_Semester1 
                                                                                INNER JOIN VW_SubjectWithStructure S ON S.Subject_ID = IH_Semester1.Subject_ID
                                                                                INNER JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = S.Course_ID
                                                                                WHERE ExternalMarks < '22' AND CourseFullName NOT LIKE '%Generic%'
                                                                                GROUP BY CourseFullName
                                                                                ORDER BY COUNT(*) ASC");
        }

        public List<ExaminationDashboard> GetOpenExaminations()
        {
            return new MSSQLFactory().GetObjectList<ExaminationDashboard>(@"SELECT CourseCategory + CASE WHEN IsBacklogSetting=0 THEN '-(Regular)' ELSE '-(BackLog)' END Course, Semester, StartDate, EndDate
                                                                                    FROM ARGExamformdownloadable
                                                                                    WHERE allowDownloadForm = 1
                                                                                    ORDER BY CourseCategory, Semester;");
        }

        public List<ExaminationDashboard> GetOpenAwardLinks()
        {
            return new MSSQLFactory().GetObjectList<ExaminationDashboard>(@"SELECT dbo.FNProgrammeDescription(Programme) AS Course,Semester,AwardType,StartDate,EndDate,FilterValue AS Year FROM  dbo.AwardFilterSettings WHERE ISActive=1 AND EndDate>=GETDATE()
             ORDER BY AwardType,Programme,Semester;");
        }

        public List<ExaminationDashboard> CurrentRunningSemesters()
        {
            return new MSSQLFactory().GetObjectList<ExaminationDashboard>(@"
                 SELECT DISTINCT SC.SemesterBatch,dbo.FNProgrammeDescription(Programme) AS Course,MAX(CurrentSemesterOrYear) AS CurrentSemesterOrYear  FROM dbo.ARGPersonalInformation_UG  P 
                 JOIN dbo.ARGSelectedCombination_UG SC ON SC.Student_ID = P.Student_ID
                 JOIN dbo.ADMCombinationMaster CS ON CS.Combination_ID = SC.Combination_ID
                 JOIN dbo.ADMCourseMaster CM ON CM.Course_ID = CS.Course_ID 
                 where SC.SemesterBatch<=YEAR(GETDATE()) AND CM.Programme=1 AND P.IsLateralEntry=0
                 GROUP BY SC.SemesterBatch,CM.Programme
                UNION
                 SELECT DISTINCT SC.SemesterBatch,dbo.FNProgrammeDescription(Programme) AS Course,MAX(CurrentSemesterOrYear) AS CurrentSemesterOrYear  FROM dbo.ARGPersonalInformation_IH  P 
                 JOIN dbo.ARGSelectedCombination_IH SC ON SC.Student_ID = P.Student_ID
                 JOIN dbo.ADMCombinationMaster CS ON CS.Combination_ID = SC.Combination_ID
                 JOIN dbo.ADMCourseMaster CM ON CM.Course_ID = CS.Course_ID 
                 where SC.SemesterBatch<=YEAR(GETDATE()) AND CM.Programme=3 AND P.IsLateralEntry=0
                 GROUP BY SC.SemesterBatch,CM.Programme
                 UNION
                 SELECT DISTINCT SC.SemesterBatch,REPLACE(
                        dbo.FNProgrammeDescription(Programme), 
                        'UG', 
                        'B.Ed'
                    ) Course,MAX(CurrentSemesterOrYear) AS CurrentSemesterOrYear  FROM dbo.ARGPersonalInformation_UG  P 
                 JOIN dbo.ARGSelectedCombination_UG SC ON SC.Student_ID = P.Student_ID
                 JOIN dbo.ADMCombinationMaster CS ON CS.Combination_ID = SC.Combination_ID
                 JOIN dbo.ADMCourseMaster CM ON CM.Course_ID = CS.Course_ID 
                 where SC.SemesterBatch<=YEAR(GETDATE()) AND CM.PrintProgramme=5 AND P.IsLateralEntry=0
                 GROUP BY SC.SemesterBatch,CM.Programme
                 union
                 SELECT DISTINCT SC.SemesterBatch,dbo.FNProgrammeDescription(Programme) AS Course,MAX(CurrentSemesterOrYear) AS CurrentSemesterOrYear  FROM dbo.ARGPersonalInformation_PG  P 
                 JOIN dbo.ARGSelectedCombination_PG SC ON SC.Student_ID = P.Student_ID
                 JOIN dbo.ADMCombinationMaster CS ON CS.Combination_ID = SC.Combination_ID
                 JOIN dbo.ADMCourseMaster CM ON CM.Course_ID = CS.Course_ID 
                 where SC.SemesterBatch<=YEAR(GETDATE()) AND CM.Programme=2 AND P.IsLateralEntry=0
                 GROUP BY SC.SemesterBatch,CM.Programme
                 UNION
                 SELECT DISTINCT SC.SemesterBatch,dbo.FNProgrammeDescription(Programme) AS Course,MAX(CurrentSemesterOrYear) AS CurrentSemesterOrYear  FROM dbo.ARGPersonalInformation_IH  P 
                 JOIN dbo.ARGSelectedCombination_IH SC ON SC.Student_ID = P.Student_ID
                 JOIN dbo.ADMCombinationMaster CS ON CS.Combination_ID = SC.Combination_ID
                 JOIN dbo.ADMCourseMaster CM ON CM.Course_ID = CS.Course_ID 
                 where SC.SemesterBatch<=YEAR(GETDATE()) AND CM.Programme=5 AND P.IsLateralEntry=0
                 GROUP BY SC.SemesterBatch,CM.Programme
                 ORDER BY  Course DESC,SC.SemesterBatch,CurrentSemesterOrYear
            ");
        }
    }
}
