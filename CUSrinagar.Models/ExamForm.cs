using CUSrinagar.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CUSrinagar.Models
{
    public class Semesters
    {
        public Semester PSemester { get; set; }
        public List<ExamForm> ExamForms { get; set; }
    }
    public class ExamForm
    {

        public int ExamFormNo { get; set; }
        public string ExamRollNo { get; set; }
        public Guid Student_ID { get; set; }
        public string FormNumber { get; set; }
        public Semester Semester { get; set; }
        public int Year { get; set; }
        public int AmountPaid { get; set; }
        public DateTime SubmittedOn { get; set; }
        public List<StudentResult> StudentResult { get; set; }
        [IgnoreDBWriter]
        public Guid StudentExamForm_ID { get; set; }
    }
    public class StudentResult
    {
        public int ExamFormNo { get; set; }
        public string SubjectFullName { get; set; }
        public SubjectType SubjectType { get; set; }
        public string FinalExternalMarks { get; set; }
        public string FinalExternalAttendance_AssessmentMarks { get; set; }
        public string FinalInternalAttendance_AssessmentMarks { get; set; }
        public string FinalInternalMarks { get; set; }
        public string ExternalMarks { get; set; }
        public string ExternalAttendance_AssessmentMarks { get; set; }
        public string InternalAttendance_AssessmentMarks { get; set; }
        public string InternalMarks { get; set; }
        public bool IsInternalPassed { get; set; }
        public bool IsExternalPassed { get; set; }
        //public List<SubjectResult> SubjectResults { get; set; }
    }

}