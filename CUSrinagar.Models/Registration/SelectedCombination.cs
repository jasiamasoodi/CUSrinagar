using CUSrinagar.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUSrinagar.Models
{

    public class AssignCombinationViewModel
    {
        public Guid Student_ID { get; set; }
        public string StudentFormNo { get; set; }
        public string CUSRegistrationNo { get; set; }
        public string FullName { get; set; }
        public PrintProgramme PrintProgramme { get; set; }
        public short CurrentSemesterOrYear { get; set; }
        public string CourseFullName { get; set; }
        public Guid Course_ID { get; set; }
        public string CollegeFullName { get; set; }
        public Guid AcceptByCollege_ID { get; set; }
        public short Batch { get; set; }
        public ARGSelectedCombination PreviousSemesterCombination { get; set; }
        public short SetCombinationForSemester { get; set; }
        public ARGSelectedCombination CurrentSemesterCombination { get; set; }

        public Dictionary<string, List<ADMSubjectMaster>> CoreSubjectOptionList { get; set; }
        public Dictionary<string, List<ADMSubjectMaster>> OptionalCoreSubjectOptionList { get; set; }
        public Dictionary<string, List<ADMSubjectMaster>> AESubjectOptionList { get; set; }
        public Dictionary<string, List<ADMSubjectMaster>> SECSubjectOptionList { get; set; }
        public Dictionary<string, List<ADMSubjectMaster>> DSESubjectOptionList { get; set; }
        public Dictionary<string, List<ADMSubjectMaster>> DCESubjectOptionList { get; set; }
        public Dictionary<string, List<ADMSubjectMaster>> GE_OESubjectOptionList { get; set; }
        public Dictionary<string, List<ADMSubjectMaster>> MILSubjectOptionList { get; set; }
        public Dictionary<string, List<ADMSubjectMaster>> AdditionalSubjectOptionList { get; set; }
        public Dictionary<string, List<ADMSubjectMaster>> MajorSubjectOptionList { get; set; }
        public Dictionary<string, List<ADMSubjectMaster>> MIDSubjectOptionList { get; set; }
        public Dictionary<string, List<ADMSubjectMaster>> MVocSubjectOptionList { get; set; }
        public Dictionary<string, List<ADMSubjectMaster>> MDiscSubjectOptionList { get; set; }
        public Dictionary<string, List<ADMSubjectMaster>> ResearchSubjectOptionList { get; set; }
        public Dictionary<string, List<ADMSubjectMaster>> InternshipSubjectOptionList { get; set; }
        public Dictionary<string, List<ADMSubjectMaster>> SeminarSubjectOptionList { get; set; }
        public Dictionary<string, List<ADMSubjectMaster>> VACSubjectOptionList { get; set; }

        public AssignCombinationViewModel()
        {
            CoreSubjectOptionList = new Dictionary<string, List<ADMSubjectMaster>>();
            OptionalCoreSubjectOptionList = new Dictionary<string, List<ADMSubjectMaster>>();
            AESubjectOptionList = new Dictionary<string, List<ADMSubjectMaster>>();
            SECSubjectOptionList = new Dictionary<string, List<ADMSubjectMaster>>();
            DSESubjectOptionList = new Dictionary<string, List<ADMSubjectMaster>>();
            DCESubjectOptionList = new Dictionary<string, List<ADMSubjectMaster>>();
            GE_OESubjectOptionList = new Dictionary<string, List<ADMSubjectMaster>>();
            MILSubjectOptionList = new Dictionary<string, List<ADMSubjectMaster>>();
            AdditionalSubjectOptionList = new Dictionary<string, List<ADMSubjectMaster>>();
            MajorSubjectOptionList = new Dictionary<string, List<ADMSubjectMaster>>();
            MIDSubjectOptionList = new Dictionary<string, List<ADMSubjectMaster>>();
            MVocSubjectOptionList = new Dictionary<string, List<ADMSubjectMaster>>();
            MDiscSubjectOptionList = new Dictionary<string, List<ADMSubjectMaster>>();
            ResearchSubjectOptionList = new Dictionary<string, List<ADMSubjectMaster>>();
            InternshipSubjectOptionList = new Dictionary<string, List<ADMSubjectMaster>>();
            SeminarSubjectOptionList = new Dictionary<string, List<ADMSubjectMaster>>();
            VACSubjectOptionList = new Dictionary<string, List<ADMSubjectMaster>>();
        }
    }



    public class ARGSelectedCombination : BaseWorkFlow
    {
        public Guid SelectedCombination_ID { get; set; }
        public Guid Student_ID { get; set; }
        public short Semester { get; set; }
        public Guid Combination_ID { get; set; }
        [IgnoreDBWriter]
        public string CombinationID { get; set; }
        //public short PreferenceOrder { get; set; }
        [IgnoreDBWriter]
        public Guid College_ID { get; set; }
        [IgnoreDBWriter]
        public string CollegeID { get; set; }
        [IgnoreDBWriter]
        public Guid Course_ID { get; set; }
        [IgnoreDBWriter]
        public string CourseID { get; set; }
        [IgnoreDBWriter]
        public string CourseCode { get; set; }
        [IgnoreDBWriter]
        public PrintProgramme PrintProgramme { get; set; }
        [IgnoreDBWriter]
        public string CombinationSubjects { get; set; }
        [IgnoreDBRead]
        [IgnoreDBWriter]
        public List<ADMSubjectMaster> Subjects { get; set; } = new List<ADMSubjectMaster>();
        public bool IsVerified { get; set; }
        public short SemesterBatch { get; set; }
    }

    public class ARGSelectedCombinationSubject
    {
        //incase u need more properties ... add here ...
        public Guid Student_ID { get; set; }
        public Guid Combination_ID { get; set; }
        public Guid Course_ID { get; set; }
        public string CombinationSubjects { get; set; }
        [IgnoreDBRead]
        [IgnoreDBWriter]
        public List<ADMSubjectMaster> Subjects { get; set; } = new List<ADMSubjectMaster>();
    }

    public class CombinationsCount
    {
        public string CombinationCode { get; set; }
        public string CombinationSubjects { get; set; }
        public int AssignedStudentCount { get; set; }
        public int TotalRow_Count { get; set; }
        [IgnoreDBRead]
        [IgnoreDBWriter]
        public List<ADMSubjectMaster> SubjectsDetails { get; set; } = new List<ADMSubjectMaster>();
    }
    public class SubjectsCount
    {
        public Guid Subject_ID { get; set; }
        public SubjectType SubjectType { get; set; }
        public string SubjectFullName { get; set; }
        public string CourseFullName { get; set; }
        public int Semester { get; set; }
        public int AssignedStudentCount { get; set; }
        public Guid Course { get; set; }
        public int SemesterBatch { get; set; }
        public Guid AcceptCollege_ID { get; set; }
        public string Gender { get; set; }
        public bool CategoryWise { get; set; }
        public string Category { get; set; }
    }
    public class ReplaceSubject
    {
        public short Semester { get; set; }
        public List<Programme> Programme { get; set; }
        public List<short> Batch { get; set; }
        public Guid? FindSubject_ID { get; set; }
        public Guid? ReplaceWithSubject_ID { get; set; }
    }
    public class BatchUpdateCombinationMaster
    {
        public short Semester { get; set; }
        public PrintProgramme printProgramme { get; set; }
        public List<short> Batch { get; set; }
        public List<Guid> Combination_IDs { get; set; }
        public Guid? FindSubject_ID { get; set; }
        public Guid? ReplaceWithSubject_ID { get; set; }
        public bool? ReplaceSubjectInResult { get; set; }
        public bool? InActiveFindedSubject_ID { get; set; }
        public bool? RemoveFindedSubject { get; set; }
        public bool? ReplaceSubjectInSyllabus { get; set; }
        public bool? ReplaceSubjectInTranscript { get; set; }
    }
    public class CombinationBatchUpdate
    {
        public Guid? OldCombination_ID { get; set; }
        public Guid NewCombination_ID { get; set; }
        public short Semester { get; set; }
        public short SemesterBatch { get; set; }
    }

    public class StudentSelectedSubject : BaseWorkFlow
    {
        public Guid? Student_ID { get; set; }
        public Guid? Course_ID { get; set; }
        public short? Semester { get; set; }
        public string CoreSubject { get; set; }
        public string SkillEnhancementSubjects { get; set; }
    }



}
