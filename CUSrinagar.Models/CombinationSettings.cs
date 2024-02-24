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
    public class CombinationSetting : BaseWorkFlow
    {
        [Required]
        public Guid CombinationSetting_ID { get; set; }
        [Required]
        public short Batch { get; set; }
        [Required]
        public Guid Course_ID { get; set; }
        [IgnoreDBWriter]
        public string CourseFullName { get; set; }
        [Required]
        public short Semester { get; set; }
        public string CompulsarySubjects { get; set; }
        [IgnoreDBRead, IgnoreDBWriter]
        public List<ADMSubjectMaster> CompulsarySubject { get; set; }
        public bool AllowCollegeChangeCombination { get; set; }
        public bool AllowStudentChangeCombination { get; set; }
        public bool AllowDeleteCombination { get; set; }
        public bool CheckAdmissionForm { get; set; }
        public bool CheckExaminationForm { get; set; }
        public bool CheckResult { get; set; }
        public bool CheckCourseApplied { get; set; }
        public string Description { get; set; }
        public Guid CombinationSettingStructure_ID { get; set; }
        public bool AllowChangeCourse { get; set; }
        [IgnoreDBRead, IgnoreDBWriter]
        public CombinationSettingStructure CombinationSettingStructure { get; set; }
        public bool ValidateByIntakeCapacity { get; set; }
        public DateTime? DefaultPaymentDate { get; set; }

        [IgnoreDBRead, IgnoreDBWriter]
        public string GetDefaultPaymentDate
        {
            get
            {
                var _date = DefaultPaymentDate.HasValue ? DefaultPaymentDate.Value : DateTime.Now;
                string parsedDate = "";
                if (DefaultPaymentDate.HasValue)
                {
                    parsedDate = (_date.Year + "-" + (_date.Month.ToString().Length == 1 ? "0" + _date.Month : _date.Month.ToString()) + "-" + (_date.Day.ToString().Length == 1 ? "0" + _date.Day : _date.Day.ToString())).Trim();
                }
                return parsedDate;
            }
        }
        public int? DefaultPaymentAmount { get; set; }


        public bool CheckInSelectionList { get; set; }
        public bool CheckSelectionListCollege { get; set; }
    }


    public class SubjectCombinationSetting
    {
        public Guid _ID { get; set; }
        //public short Batch { get; set; }
        [IgnoreDBWriter]
        public Guid? Course_ID { get; set; }
        [IgnoreDBWriter]
        public string CourseFullName { get; set; }
        [IgnoreDBWriter]
        public short BaseSemester { get; set; }
        public short ForSemester { get; set; }
        public Guid BaseSubject_ID { get; set; }
        [IgnoreDBWriter]
        public string BaseSubjectFullName { get; set; }
        //[IgnoreDBWriter]
        //public short DependentSemester { get; set; }
        public Guid? DependentSubject_ID { get; set; }
        [IgnoreDBWriter]
        public string DependentSubjectFullName { get; set; }
        public bool IsActive { get; set; }
        public DateTime Dated { get; set; }
        [IgnoreDBWriter]
        public RecordState RecordState { get; set; }
        [IgnoreDBWriter]
        public Programme Programme { get; set; }
    }

    public class CombinationSettingStructure
    {
        public Guid CombinationSettingStructure_ID { get; set; }
        public int TotalSubjectCredits { get; set; }
        public bool ValidateByTotalSubjectCredits { get; set; }
        public int TotalResultCredits { get; set; }
        public bool ValidateByTotalResultCredits { get; set; }
        public int TotalNumberOfSubjects { get; set; }
        public bool ValidateByTotalNumberOfSubjects { get; set; }
        public int TotalResultNumberOfSubjects { get; set; }
        public bool ValidateByTotalResultNumberOfSubjects { get; set; }
        public bool Show_Core { get; set; }
        public int Core_Count { get; set; }
        public bool ValidateByCore_Count { get; set; }
        public int Core_Credit { get; set; }
        public bool ValidateByCore_Credit { get; set; }
        public bool CoreDependOnPrevSem { get; set; }
        public bool MajorDependOnPrevSem { get; set; }
        public bool MIDDependOnPrevSem { get; set; }
        public bool MDiscDependOnPrevSem { get; set; }
        public bool Show_DCE_DSE { get; set; }
        public int DCE_DSE_Count { get; set; }
        public bool ValidateByDCE_DSE_Count { get; set; }
        public int DCE_DSE_Credit { get; set; }
        public bool ValidateByDCE_DSE_Credit { get; set; }
        public bool DCEByCourse { get; set; }
        public bool DCEBySubject { get; set; }
        public int BaseSemester { get; set; }
        public bool ShowGE_OE { get; set; }
        public int GE_OE_Count { get; set; }
        public bool ValidateByGE_OE_Count { get; set; }
        public int GE_OE_Credit { get; set; }
        public bool ValidateByGE_OE_Credit { get; set; }
        public bool AllowInterCollegeElective { get; set; }
        public bool Show_SEC { get; set; }
        public int SEC_Count { get; set; }
        public bool ValidateBySEC_Count { get; set; }
        public int SEC_Credit { get; set; }
        public bool ValidateBySEC_Credit { get; set; }
        public bool Show_MIL { get; set; }
        public int MIL_Count { get; set; }
        public bool ValidateByMIL_Count { get; set; }
        public int MIL_Credit { get; set; }
        public bool ValidateByMIL_Credit { get; set; }
        public bool MILDependOnPrevSem { get; set; }
        public bool Show_AE { get; set; }
        public int AE_Count { get; set; }
        public bool ValidateByAE_Count { get; set; }
        public int AE_Credit { get; set; }
        public bool ValidateByAE_Credit { get; set; }

        public bool Show_Major { get; set; }
        public int Major_Count { get; set; }
        public bool ValidateByMajor_Count { get; set; }
        public int Major_Credit { get; set; }
        public bool ValidateByMajor_Credit { get; set; }
        public bool Show_MinorInterDisciplinary { get; set; }
        public int MinorInterDisciplinary_Count { get; set; }
        public bool ValidateByMinorInterDisciplinary_Count { get; set; }
        public int MinorInterDisciplinary_Credit { get; set; }
        public bool ValidateByMinorInterDisciplinary_Credit { get; set; }
        public bool Show_MinorVocational { get; set; }
        public int MinorVocational_Count { get; set; }
        public bool ValidateByMinorVocational_Count { get; set; }
        public int MinorVocational_Credit { get; set; }
        public bool ValidateByMinorVocational_Credit { get; set; }
        public bool Show_MultiDisciplinary { get; set; }
        public int MultiDisciplinary_Count { get; set; }
        public bool ValidateByMultiDisciplinary_Count { get; set; }
        public int MultiDisciplinary_Credit { get; set; }
        public bool ValidateByMultiDisciplinary_Credit { get; set; }
        public bool Show_Internship { get; set; }
        public int Internship_Count { get; set; }
        public bool ValidateByInternship_Count { get; set; }
        public int Internship_Credit { get; set; }
        public bool ValidateByInternship_Credit { get; set; }
        public bool Show_Seminar { get; set; }
        public int Seminar_Count { get; set; }
        public bool ValidateBySeminar_Count { get; set; }
        public int Seminar_Credit { get; set; }
        public bool ValidateBySeminar_Credit { get; set; }
        public bool Show_Research { get; set; }
        public int Research_Count { get; set; }
        public bool ValidateByResearch_Count { get; set; }
        public int Research_Credit { get; set; }
        public bool ValidateByResearch_Credit { get; set; }
        public bool Show_VAC { get; set; }
        public int VAC_Count { get; set; }
        public bool ValidateByVAC_Count { get; set; }
        public int VAC_Credit { get; set; }
        public bool ValidateByVAC_Credit { get; set; }


        public int AdditionalCourseCount { get; set; }
        public int OC_Count { get; set; }
        public int Other_Credit { get; set; }
        public string Remark { get; set; }
        public DateTime CreatedOn { get; set; }
        public Guid CreatedBy { get; set; }
        public bool AllowInterProgrammeSkill { get; set; }
    }

}
