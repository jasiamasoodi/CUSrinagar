using CUSrinagar.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUSrinagar.Models
{
    public class StatisticsGenderWise
    {
        public short Batch { get; set; }
        public short Male { get; set; }
        public short Female { get; set; }

    }

    public class StatisticsProgramGenderWise
    {
        public short Batch { get; set; }
        public PrintProgramme PrintProgramme { get; set; }
        public Programme Programme { get; set; }
        public short Male { get; set; }
        public short Female { get; set; }
    }

    public class StatisticsProgramCategoryGenderWise
    {
        public short Batch { get; set; }
        public PrintProgramme PrintProgramme { get; set; }
        public Programme Programme { get; set; }
        public short Male { get; set; }
        public short Female { get; set; }
        public string CategoryCode { get; set; }
        public string CategoryName { get; set; }
    }

    public class StatisticsCategoryGenderWise
    {
        public short Batch { get; set; }
        public short Male { get; set; }
        public short Female { get; set; }
        public string CategoryCode { get; set; }
        public string CategoryName { get; set; }
    }

    //public class StudentStatisticsFilter
    //{
    //    public bool IsYearSet { get; set; }
    //    public bool IsCollegeSet { get; set; }
    //    public bool IsCourseSetSet { get; set; }
    //    public bool IsDistictSet { get; set; }
    //    public bool IsGenderSet { get; set; }
    //    public bool IsProgrammeSet { get; set; }
    //}
    public class StudentStatisticsSummary
    {
        public short? Batch { get; set; }
        public Guid? AcceptCollege_ID { get; set; }
        public string CollegeFullName { get; set; }
        public Guid? Course_ID { get; set; }
        public string CourseFullName { get; set; }
        public string District { get; set; }
        public string Gender { get; set; }
        public Programme? Programme { get; set; }
        public int Count { get; set; }
    }
    //public class StudentStatisticsFilter
    //{
    //    public short Year { get; set; }
    //    public bool IsYearSet { get; set; }
    //    public String CollegeFullName { get; set; }
    //    public bool IsCollegeSet { get; set; }
    //    public String CourseFullName { get; set; }
    //    public bool IsCourseSetSet { get; set; }
    //    public String District { get; set; }
    //    public bool IsDistictSet { get; set; }
    //    public String Gender { get; set; }
    //    public bool IsGenderSet { get; set; }
    //    public Programme Programme { get; set; }
    //    public bool IsProgrammeSet { get; set; }
    //}

    public class CollegeCourseCategoryStatistics
    {
        public Guid College_ID { get; set; }
        public string CollegeFullName { get; set; }
        public List<CourseCategoryWiseStatistics> CourseCategory { get; set; }
    }

    public class CourseCategoryWiseStatistics
    {
        public string CourseFullName { get; set; }

        public Guid Course_ID { get; set; }

        public int Male { get; set; }

        public int Female { get; set; }

        [IgnoreDBWriter]
        public int Total { get { return Male + Female; } }
        [IgnoreDBRead]
        public List<CategoryStatistics> Categories { get; set; }
    }

    public class CategoryStatistics
    {
        public string CategoryName { get; set; }

        public int Male { get; set; }

        public int Female { get; set; }

    }

    public class CollegeCourseDistrictCategoryStatistics
    {
        public Guid College_ID { get; set; }
        public string CollegeFullName { get; set; }
        public List<CourseDistrictWiseStatistics> CourseDistrictCategory { get; set; }
    }

    public class CourseDistrictWiseStatistics
    {
        public string CourseFullName { get; set; }
        public List<DistrictStatistics> Districts { get; set; }
    }

    public class DistrictStatistics
    {
        public string CourseFullName { get; set; }

        public Guid Course_ID { get; set; }

        public string District { get; set; }

        public int Male { get; set; }

        public int Female { get; set; }

        public List<CategoryStatistics> Categories { get; set; }
    }

    public class ReEvaluationStatistics
    {
        public int SubmittedYear { get; set; }

        public string Course { get; set; }

        public int Semester { get; set; }

        public FormType FormType { get; set; }

        public int AmountReceived { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public Guid Notification_ID { get; set; }

        public string Link { get; set; }

        public string NotificationID { get; set; }

        public int NoOfStudents { get; set; }
    }

    public class PaymentStatistics
    {
        public decimal AmountReceived { get; set; }
        public int TotalForms { get; set; }

        public PaymentModuleType ModuleType { get; set; }

    }

    public class AdmissionPaymentStatistics
    {
        public decimal AmountReceived { get; set; }

        public Programme Programme { get; set; }

    }

    public class StudentsCourseWiseStatistics
    {
        public int SemesterBatch { get; set; }

        public Programme Programme { get; set; }

        public PrintProgramme PrintProgramme { get; set; }

        public string CollegeFullName { get; set; }

        public string CourseFullName { get; set; }

        public string Semester { get; set; }

        public FormStatus FormStatus { get; set; }
        [IgnoreDBRead, IgnoreDBWriter]
        public Guid AcceptCollege_ID { get; set; }

        [IgnoreDBRead, IgnoreDBWriter]
        public Guid Course_ID { get; set; }
        [IgnoreDBRead, IgnoreDBWriter]
        public int Batch { get; set; }
        public string Category { get; set; }
        public string District { get; set; }

        public int NoOfStudents { get; set; }
        public int Male { get; set; }
        public int Female { get; set; }
        public int Other { get; set; }
    }



}
