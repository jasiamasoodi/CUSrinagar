using CUSrinagar.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;


namespace GeneralModels
{
    public class FilterModels
    {
        public short SemesterBatch { get; set; }
        public PrintProgramme PrintProgramme { get; set; }
        public string AcceptCollege_ID { get; set; }
        public Guid Course_ID { get; set; }
        public Guid Student_ID { get; set; }
        public string CUSRegistrationNo { get; set; }
        public short Batch { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime PrintedOn { get; set; }
        public DateTime ValidatedOn { get; set; }
        public DateTime HandedOverOn { get; set; }
        public int IssueNumber { get; set; }
        public string ClassRollNo { get; set; }
        public string FullName { get; set; }
        public string FathersName { get; set; }
        public string ExamRollNumber { get; set; }
        public string CourseFullName { get; set; }
        public short Semester { get; set; }
        public Programme Programme { get; set; }
        public string CombinationSubjects { get; set; }
        public DateTime Dated { get; set; }
        public Guid ResultNotification_ID { get; set; }
        public Guid Subject_ID { get; set; }
        public Guid Department_ID { get; set; }
        public Guid _ID { get; set; }
        public short SemesterFrom { get; set; }
        public short SemesterTo { get; set; }
        public bool IsInternalPassed { get; set; }//For Engineering Marks-Sheet
        public bool IsExternalPassed { get; set; }//For Engineering Marks-Sheet
        public bool IsExternalMarksApplicable { get; set; }//For Engineering Marks-Sheet
        public Guid TCourse_ID { get; set; }
        public string NotificationNo { get; set; }
    }
    public class ResponseData
    {
        public bool IsSuccess { get; set; }
        public object ResponseObject { get; set; }
        public object SecondaryResponseObject { get; set; }
        public int NumberOfRecordsEffected { get; set; }
        public string SuccessMessage { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class FilterHelper
    {
        public FilterHelper()
        {
            Command = new SqlCommand();
        }

        public string WhereClause { get; set; }

        public SqlCommand Command { get; set; }
    }

    public class Paging
    {
        public int PageSize { get; set; }

        public int PageNumber { get; set; }

        public string DefaultOrderByColumn { get; set; }
    }

    public class Parameters
    {
        public List<SearchFilter> Filters { get; set; }

        public Paging PageInfo { get; set; }

        public Sort SortInfo { get; set; }

    }

    public class Sort
    {
        public string ColumnName { get; set; }
        public bool IsAlphaNumeric { get; set; }

        public SortOrder OrderBy { get; set; }
    }

    public class SearchFilter
    {
        public string Column { get; set; }
        public string TableAlias { get; set; }
        public SQLOperator Operator { get; set; }
        public string Value { get; set; }
        public LogicalOperator GroupOperation { get; set; } = LogicalOperator.AND;
        public bool IsSibling { get; set; }
    }
    public class ExcelFilterCombination
    {
        [Required(ErrorMessage = "Required")]
        public string Programme { get; set; }
        public string Combination { get; set; }
        [Required(ErrorMessage = "Required")]
        public string Course { get; set; }
        [Required(ErrorMessage = "Required")]
        public string Batch { get; set; }
        public string Semester { get; set; }
    }
    public class SubmitAward
    {
        [Required(ErrorMessage = "Required")]
        public string Semester { get; set; }
        [Required(ErrorMessage = "Required")]
        public Programme Programme { get; set; }
        [Required(ErrorMessage = "Required")]
        public MarksFor MarksFor { get; set; }
        [Required(ErrorMessage = "Required")]
        public string CombinationSubjects { get; set; }

        public int Year { get; set; }
        public int Batch { get; set; }
        public bool IsBacklog { get; set; }
    }

    public class Settings
    {
        public Guid Settings_ID { get; set; }

        public Module Module { get; set; }

        public PrintProgramme Programme { get; set; }

        public short Semester { get; set; }

        public string Prefix { get; set; }

        public long Count { get; set; }

        public string Suffix { get; set; }
    }

    public class MenuList
    {
        public Guid Menu_ID { get; set; }
        public string Controller { get; set; }
        public string ActionName { get; set; }
        public string ReturnType { get; set; }
        public string Attributes { get; set; }
    }
    public class KeyValue
    {
        public int Key { get; set; }

        public string Value { get; set; }
    }


    public class StudentLoginInfo
    {
        public Guid College_ID { get; set; }

        public Guid Course_ID { get; set; }

        public string CourseFullName { get; set; }

        public string CollegeFullName { get; set; }
    }


    public class ExaminationDashboard
    {
        public string Course { get; set; }

        public int Semester { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string AwardType { get; set; }//For Current Award Links

        public int Batch { get; set; }//For Current Running Semesters
        public int CurrentSemesterOrYear { get; set; }//For Current Running Semesters

        public int SemesterBatch { get; set; }//For Current Running Semesters
        public int PassOut { get; set; }//For Current Running Semesters

        public int Year { get; set; }//For Exam year that is Filter value
    }

    public class ProgammeDashboard
    {
        public string Programme { get; set; }

        public int NoOfStudents { get; set; }

        public int Batch { get; set; }
    }

    public class CoursesDashboard
    {
        public string CourseFullName { get; set; }
    }

    public class DivImprovementResponse
    {
        public short TotalForms { get; set; }
        public bool hasAlreadyApplied { get; set; }
        public short SemesterApplied { get; set; }
    }

}

