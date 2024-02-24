using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using System;
using System.Collections.Generic;

namespace CUSrinagar.Models
{
    public class CourseWiseList
    {
        public Guid Student_ID { get; set; }
        public string CUSRegistrationNo { get; set; }
        public string StudentFormNo { get; set; }
        public Int64? EntranceRollNo { get; set; }
        public string BoardRegistrationNo { get; set; }
        public string FullName { get; set; }
        public string FathersName { get; set; }
        public Double? Percentage { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string Category { get; set; }
        public string CombinationCode { get; set; }
        public string ClassRollNo { get; set; }
        public short SemesterBatch { get; set; }
        public Guid? AcceptCollege_ID { get; set; }
        public short Semester { get; set; }
        public DateTime DOB { get; set; }
        public bool IsVerified { get; set; }

        public decimal TxnAmount { get; set; }
        public DateTime? TxnDate { get; set; }
        public PaymentModuleType ModuleType { get; set; }
        public PaymentType PaymentType { get; set; }
        public SubjectType SubjectType { get; set; }

        /// <summary>
        /// using this for filter
        /// </summary>
        public short Batch { get; set; }
        public string Subject_ID { get; set; }
        public Guid? Combination_ID { get; set; }
        public Guid? Course_ID { get; set; }
        public string CourseFullName { get; set; }
        public FormStatus FormStatus { get; set; }
        public Programme Programme { get; set; }
        public string ExamName { get; set; }
        public string ExamRollNo { get; set; }
        public string Gender { get; set; }
        public string Religion { get; set; }
    }
    public class LibraryForm
    {
        public string StudentFormNo { get; set; }

        public string FullName { get; set; }

        public short SemesterBatch { get; set; }

        public string PhoneNo { get; set; }

        public string FathersName { get; set; }

        public string MothersName { get; set; }
        public string Category { get; set; }


        public Guid Student_ID { get; set; }

        public string BoardRegistrationNo { get; set; }

        public string CUSRegistrationNo { get; set; }
        public string Gender { get; set; }
        public DateTime DOB { get; set; }


        public string Photograph { get; set; }

        public bool? IsProvisional { get; set; }
        public FormStatus FormStatus { get; set; }

        public Guid Combination_ID { get; set; }
        public ARGStudentAddress StudentAddress { get; set; }


        public string StudentSubjects { get; set; }
        public Guid Course_ID { get; set; }
        public Guid College_ID { get; set; }

        public int year { get; set; }


        public string ClassRollNo { get; set; }
        public int Semester { get; set; }
        public bool IsLateralEntry { get; set; }
        public string QualifyingExam { get; set; }
        public string Religion { get; set; }
        public string ABCID { get; set; }
    }


    public class AttendanceSheetForm
    {
        public string StudentFormNo { get; set; }

        [IgnoreDBRead]
        [IgnoreDBWriter]
        public byte[] QRCode
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(StudentFormNo))
                {
                    return StudentFormNo.ToQRCode();
                }
                return null;
            }
        }

        public string FullName { get; set; }
        public short Batch { get; set; }
        public string PhoneNo { get; set; }
        public string FathersName { get; set; }
        public string MothersName { get; set; }
        public Guid Student_ID { get; set; }
        public string BoardRegistrationNo { get; set; }
        public string CUSRegistrationNo { get; set; }
        public string Gender { get; set; }
        public DateTime DOB { get; set; }
        public string Photograph { get; set; }
        public bool? IsProvisional { get; set; }
        public FormStatus FormStatus { get; set; }
        public Guid Combination_ID { get; set; }
        public string CombinationCode { get; set; }
        public string StudentSubjects { get; set; }
        public Guid Course_ID { get; set; }
        public Guid College_ID { get; set; }
        public int year { get; set; }
        public string ExamRollNumber { get; set; }
        public string FormNumber { get; set; }
        public int Semester { get; set; }
        public string CourseFullName { get; set; }
        public Guid AcceptCollege_ID { get; set; }
        public SemesterExamCenterDetails SemesterExamCenterDetail { get; set; }

        [IgnoreDBRead]
        [IgnoreDBWriter]
        public bool Include { get; set; }
    }


    public class StudentRegister
    {
        public string StudentFormNo { get; set; }

        public string FullName { get; set; }

        public short SemesterBatch { get; set; }

        public string PhoneNo { get; set; }

        public string FathersName { get; set; }

        public string MothersName { get; set; }
        public string Category { get; set; }


        public Guid Student_ID { get; set; }

        public string BoardRegistrationNo { get; set; }

        public string CUSRegistrationNo { get; set; }
        public string Gender { get; set; }
        public DateTime DOB { get; set; }


        public string Photograph { get; set; }

        public bool? IsProvisional { get; set; }
        public FormStatus FormStatus { get; set; }

        public Guid Combination_ID { get; set; }
        public ARGStudentAddress StudentAddress { get; set; }


        public Guid Course_ID { get; set; }
        public Guid College_ID { get; set; }

        public int year { get; set; }


        public string ClassRollNo { get; set; }
        public bool IsLateralEntry { get; set; }
        public bool IsPassout { get; set; }
        public MigrationIssueStatus MigrationIssued { get; set; }
        public decimal? CGPA { get; set; }
        public List<SemesterCombination> SubjectsExams { get; set; }
    }

    public class SemesterCombination
    {
        public string StudentSubjects { get; set; }
        public int Semester { get; set; }
        public string ExamRollNo { get; set; }
        public string ExamYear { get; set; }
        public DateTime? DateOfPassing { get; set; }
        public bool IsVerifiedByCollege { get; set; }
        public decimal? SGPA { get; set; }
        public DateTime AdmissionDate { get; set; }
    }
}