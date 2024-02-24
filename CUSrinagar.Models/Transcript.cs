using CUSrinagar.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUSrinagar.Models
{
    public class Transcript
    {
        public Guid Student_ID { get; set; }
        public string FullName { get; set; }
        public string FathersName { get; set; }
        public string CUSRegistrationNo { get; set; }
        public string ExamRollNumber { get; set; }
        public Guid Course_ID { get; set; }
        public string CourseFullName { get; set; }
        public string Batch { get; set; }
        public string SemesterBatch { get; set; }
        public Guid AcceptCollege_ID { get; set; }
        public string CollegeFullName { get; set; }
        public string SchoolFullName { get; set; }
        public string CollegeAddress { get; set; }
        public int SemesterTo { get; set; }

        [IgnoreDBRead]
        public string Mode { get { return "Regular"; } }

        public bool IsLateralEntry { get; set; }

        [IgnoreDBRead]
        public List<SubjectMarks> Subject { get; set; }
        [IgnoreDBRead]
        public List<MSSGPA> SGPA { get; set; }
        [IgnoreDBRead]
        public List<MSCGPA> CGPA { get; set; }
        [IgnoreDBRead]
        public MSTranscriptDegreeSettings TranscriptDegreeSettings { get; set; }
        public PrintProgramme printProgramme { get; set; }
        public string DegreeCourseTitle { get; set; }
    }
    public class TranscriptUpdate
    {
        public Guid Student_ID { get; set; }
        public int SemesterTo { get; set; }
        public Guid Course_ID { get; set; }
    }
}
