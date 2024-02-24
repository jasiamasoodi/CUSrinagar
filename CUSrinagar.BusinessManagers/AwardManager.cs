using CUSrinagar.Models;
using System;
using System.Collections.Generic;
using CUSrinagar.DataManagers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.IO;
using System.Web.Mvc;
using System.Diagnostics;
using System.Web;
using GeneralModels;
using CUSrinagar.Enums;
using System.Transactions;
using Terex;

namespace CUSrinagar.BusinessManagers
{
    public class AwardManager
    {
        public static List<SelectListItem> FillSubjectsAssigned(int semester, Programme programme, int batch, bool isEvaluator)
        {
            List<SelectListItem> subjectsAssigned = new List<SelectListItem>();
            SubjectsManager subjectsManager = new SubjectsManager();
            ADMSubjectMaster aDMSubjectMaster;

            if (programme == Programme.UG && batch > 2021)
            {
                if (isEvaluator)
                {
                    new AwardManager().GetEvalvatorSubjects(programme).Where(x => x.Semester == semester)
                                         .GroupBy(s => s.Subject_ID).Select(g => g.First()).ToList()
                                         .ForEach(x =>
                                         {
                                             aDMSubjectMaster = subjectsManager.Get(x.Subject_ID);
                                             subjectsAssigned.Add(
                                                 new SelectListItem()
                                                 {
                                                     Text = aDMSubjectMaster.SubjectFullName + "-"
                                                    + aDMSubjectMaster.SubjectType
                                                    + " (" + aDMSubjectMaster.DepartmentFullName + ")",
                                                     Value = x.Subject_ID.ToString()
                                                 });
                                         });
                }
                else
                {
                    new AwardManager().GetProfessorSubjects(programme).Where(x => x.Semester == semester)
                                         .GroupBy(s => s.Subject_ID).Select(g => g.First()).ToList()
                                         .ForEach(x =>
                                         {
                                             aDMSubjectMaster = subjectsManager.Get(x.Subject_ID);
                                             subjectsAssigned.Add(
                                                 new SelectListItem()
                                                 {
                                                     Text = aDMSubjectMaster.SubjectFullName + "-"
                                                    + aDMSubjectMaster.SubjectType
                                                    + " (" + aDMSubjectMaster.DepartmentFullName + ")",
                                                     Value = x.Subject_ID.ToString()
                                                 });
                                         });
                }
            }
            else
            {
                if (isEvaluator)
                {
                    new AwardManager().GetEvalvatorSubjects(programme).Where(x => x.Semester == semester)
                                         .GroupBy(s => s.Subject_ID).Select(g => g.First()).ToList()
                                         .ForEach(x =>
                                         {
                                             aDMSubjectMaster = subjectsManager.Get(x.Subject_ID);
                                             subjectsAssigned.Add(
                                                 new SelectListItem()
                                                 {
                                                     Text = aDMSubjectMaster.SubjectFullName + "-"
                                                    + aDMSubjectMaster.SubjectType
                                                    + " (" + new CourseManager().GetCourseById(x.Course_ID).CourseFullName + ")",
                                                     Value = x.Subject_ID.ToString()
                                                 });
                                         });
                }
                else
                {
                    new AwardManager().GetProfessorSubjects(programme).Where(x => x.Semester == semester)
                             .GroupBy(s => s.Subject_ID).Select(g => g.First()).ToList()
                             .ForEach(x =>
                             {
                                 aDMSubjectMaster = subjectsManager.Get(x.Subject_ID);
                                 subjectsAssigned.Add(
                                     new SelectListItem()
                                     {
                                         Text = aDMSubjectMaster.SubjectFullName + "-"
                                        + aDMSubjectMaster.SubjectType
                                           + " (" + new CourseManager().GetCourseById(x.Course_ID).CourseFullName + ")",
                                         Value = x.Subject_ID.ToString()
                                     });
                             });
                }
            }

            return subjectsAssigned;
        }
        public List<AwardModel> GetAllStudentListExt(Parameters parameter, Programme programme, MarksFor marksFor, int Year, string Courses, bool isPrint = false, bool isResultDeclared = false)
        {
            SearchFilter semfilter = parameter.Filters.Where(x => x.Column == "Semester").FirstOrDefault();
            short semester = (short)Convert.ToInt32(semfilter.Value);
            PrintProgramme printProgramme = new GeneralFunctions().GetPrintProgramme(programme);

            SearchFilter subfilter = parameter.Filters.Where(x => x.Column == "CombinationSubjects").FirstOrDefault();
            subfilter.Value = (subfilter.Value).Replace("%", string.Empty);
            Guid SubjectId = new Guid(subfilter.Value);
            ADMSubjectMaster subject = new SubjectsManager().Get(SubjectId);

            List<AwardModel> list = new AwardDB().GetAllRecordsExt(parameter, programme, marksFor, Year, Courses, isPrint, subject.SubjectType, isResultDeclared);
            if (list != null)
            {
                foreach (AwardModel obj in list)
                {

                    // SubjectResult result = new ExaminationFormManager().BlockExternalt(obj.Student_ID, obj.Subject_ID,obj.Batch ,semester, printProgramme);
                    obj.InternalResult = new ResultManager().GetInternalResultStatus(obj.Student_ID, SubjectId, semester);

                    // obj.InternalResult = result.InternalStatus == ResultStatus.P ? true : false;
                    //  obj.OverallResult = result.OverallResultStatus == ResultStatus.P ? true : false;
                }
            }
            return list;
        }

        public List<AwardModel> GetAllStudentList(Parameters parameter, Programme programme, MarksFor marksFor, int batch, bool IsBacklog, string Courses, bool isPrint = false)
        {
            SearchFilter subfilter = parameter.Filters.Where(x => x.Column == "CombinationSubjects").FirstOrDefault();
            subfilter.Value = (subfilter.Value).Replace("%", string.Empty);
            Guid SubjectId = new Guid(subfilter.Value);
            ADMSubjectMaster subject = new SubjectsManager().Get(SubjectId);

            if (parameter == null)
                return null;

            List<AwardModel> list = new AwardDB().GetAllRecords(parameter, programme, marksFor, batch, subject.SubjectType, subject.HasExaminationFee, IsBacklog, Courses, isPrint);
            return list;
        }


        public List<AppUserEvaluatorSubjects> GetEvalvatorSubjects(Programme programme)
        {
            List<AppUserEvaluatorSubjects> list = new AwardDB().GetEvalvatorSubjects(programme, AppUserHelper.User_ID);
            return list ?? new List<AppUserEvaluatorSubjects>();
        }

        public List<AppUserProfessorSubjects> GetProfessorSubjects(Programme programme)
        {
            List<AppUserProfessorSubjects> list = new AwardDB().GetProfessorSubjects(programme, AppUserHelper.User_ID);
            return list ?? new List<AppUserProfessorSubjects>();
        }
        public List<AppUserProfessorSubjects> GetProfessor(Guid Subject_Id)
        {
            List<AppUserProfessorSubjects> list = new AwardDB().GetProfessor(Subject_Id);
            return list ?? new List<AppUserProfessorSubjects>();
        }
        public Parameters GetParameters(Programme programme, bool id3, Guid? id2, string id1, int id)
        {
            Parameters parameters = new Parameters();
            SearchFilter sf;
            List<SearchFilter> filters = new List<SearchFilter>();
            sf = new SearchFilter();
            sf.Column = "IsBacklog";
            sf.Operator = SQLOperator.EqualTo;
            sf.Value = id3.ToString();
            filters.Add(sf);
            sf = new SearchFilter();
            sf.Column = "CombinationSubjects";
            sf.Operator = SQLOperator.EqualTo;
            sf.Value = id2.ToString();
            filters.Add(sf);
            sf = new SearchFilter();
            sf.Column = "Batch";
            sf.Operator = SQLOperator.EqualTo;
            sf.Value = id.ToString();
            filters.Add(sf);
            sf = new SearchFilter();
            sf.Column = "Semester";
            sf.Operator = SQLOperator.EqualTo;
            sf.Value = id1.ToString();
            filters.Add(sf);
            parameters.Filters = filters;
            return parameters;
        }

        public AwardFilterSettings GetAwardFilters(Guid id)
        {
            AwardFilterSettings awardFilterSettings = new AwardDB().GetAwardFilter(id);
            awardFilterSettings.CourseList = string.IsNullOrEmpty(awardFilterSettings.Courses) || awardFilterSettings.Courses == "NULL" ? new List<Guid>() : awardFilterSettings.Courses.Split(',').Select(Guid.Parse).ToList();
            awardFilterSettings.CollegeList = string.IsNullOrEmpty(awardFilterSettings.Colleges) || awardFilterSettings.Colleges == "NULL" ? new List<Guid>() : awardFilterSettings.Colleges.Split(',').Select(Guid.Parse).ToList();
            return awardFilterSettings;
        }

        public bool EditFilter(AwardFilterSettings model)
        {
            return new AwardDB().EditFilter(model, new List<string>(), new List<string>()) > 0;
        }

        public bool ReleaseAward(string id)
        {
            AwardSettingsModel model = new AwardDB().FetchAwardSettings(id);
            if (model.Batch > 0)
            {
                if (ReleaseUserSubjects(model))
                    return new AwardDB().ReleaseAward(id, !model.ISFinalSubmit);
            }
            else
            {
                if (ReleaseUserSubjectsExt(model))
                {
                    return new AwardDB().ReleaseAwardExt(id, !model.ISFinalSubmitTheory);
                }
            }
            return false;
        }
        public bool ReleaseUserSubjects(AwardSettingsModel model)
        {
            Programme programme = new CourseManager().GetCourseById(model.Course_Id).Programme;
            return new AwardDB().ReleaseUserSubjects(model, programme);
        }

        public bool UpdateAwardStatus(AwardFilterSettings model, string type)
        {
            return new AwardDB().UpdateAwardStatus(model, type);
        }

        public bool ReleaseUserSubjectsExt(AwardSettingsModel model)
        {
            Programme programme = new CourseManager().GetCourseById(model.Course_Id).Programme;
            return new AwardDB().ReleaseUserSubjectsExt(model, programme);
        }
        public bool OpenAwardLink(string id, DateTime? dateTime)
        {
            AwardFilterSettings model = new AwardDB().FetchAwardFilterSettings(id);
            if (dateTime == null) dateTime = model.EndDate;
            return new AwardDB().OpenAwardLink(id, !model.IsActive, dateTime.Value);
        }

        public bool UpdateFilter(AwardFilterSettings awardFilterSettings)
        {
            return new AwardDB().OpenAwardLink(awardFilterSettings.AwardFilterSettingsID.ToString(), awardFilterSettings.IsActive, awardFilterSettings.EndDate);
        }

        public bool checkHasDate(AwardFilterSettings awardFilterSettings)
        {
            AwardFilterSettings model = new AwardDB().checkHasDate(awardFilterSettings);
            return model != null;
        }

        public IEnumerable<AwardSettingsModel> GetAllAwardSetting(Parameters parameter)
        {
            return new AwardDB().GetAllAwardSetting(parameter);
        }

        public bool viewHaveAnyIntORExtColumn(ADMSubjectMaster subject, MarksIsPartOf marksIsPartOf, AwardModuleType awardModule)
        {

            bool HaveIntORExtColumn = (subject.InternalIsPartOf == marksIsPartOf && subject.InternalVisibleTo == awardModule && subject.IsInternalMarksApplicable)
                || (subject.ExternalAttendance_AssessmentIsPartOf == marksIsPartOf && subject.ExternalAttendance_AssessmentVisibleTo == awardModule && subject.IsExternalAttendance_AssessmentMarksApplicable)
                || (subject.InternalAttendance_AssessmentIsPartOf == marksIsPartOf && subject.InternalAttendance_AssessmentVisibleTo == awardModule && subject.IsInternalAttendance_AssessmentMarksApplicable)
                || (subject.ExternalIsPartOf == marksIsPartOf && subject.ExternalVisibleTo == awardModule && subject.IsExternalMarksApplicable);
            return HaveIntORExtColumn;
        }
        public int MinMarks(ADMSubjectMaster subject, MarksIsPartOf marksIsPartOf, AwardModuleType awardModule)
        {
            return ((subject.InternalIsPartOf == marksIsPartOf && subject.InternalVisibleTo == awardModule && subject.IsInternalMarksApplicable) ? subject.InternalMinPassMarks : 0)
                  + ((subject.ExternalAttendance_AssessmentIsPartOf == marksIsPartOf && subject.ExternalAttendance_AssessmentVisibleTo == awardModule && subject.IsExternalAttendance_AssessmentMarksApplicable) ? subject.ExternalAttendance_AssessmentMinPassMarks : 0)
                  + ((subject.InternalAttendance_AssessmentIsPartOf == marksIsPartOf && subject.InternalAttendance_AssessmentVisibleTo == awardModule && subject.IsInternalAttendance_AssessmentMarksApplicable) ? subject.InternalAttendance_AssessmentMinPassMarks : 0)
                  + ((subject.ExternalIsPartOf == marksIsPartOf && subject.ExternalVisibleTo == awardModule && subject.IsExternalMarksApplicable) ? subject.ExternalMinPassMarks : 0);
        }

        public IEnumerable<AwardFilterSettings> GetAllAwardFilters(Parameters parameter)
        {
            IEnumerable<AwardFilterSettings> list = new AwardDB().GetAllAwardFilters(parameter) ?? new List<AwardFilterSettings>();
            foreach (AwardFilterSettings item in list)
            {
                if (!string.IsNullOrEmpty(item.Colleges) && item.Colleges != "NULL")
                { item.CollegesName = GetCollegeName(item.Colleges); }
                if (!string.IsNullOrEmpty(item.Courses) && item.Courses != "NULL")
                { item.CoursesName = GetCourseName(item.Courses); }
            }
            return list;
        }

        private string GetCollegeName(string colleges)
        {
            return new CollegeManager().GetColleges(colleges);
        }

        private string GetCourseName(string courses)
        {
            return new CourseManager().GetCourse(courses);
        }

        public int UpdateBacklogsToLateron(Guid id)
        {
            AwardFilterSettings awardFilterSettings = new AwardDB().GetAwardFilter(id);
            SqlCommand cmd = new SqlCommand();
            int returndata = 0;

            string awardtype = awardFilterSettings.AwardType;
            int examYear = awardFilterSettings.FilterValue;
            int semester = Convert.ToInt16(awardFilterSettings.Semester);

            PrintProgramme programme = GeneralFunctions.ProgrammeToPrintProgrammeMapping(awardFilterSettings.Programme);

            if (awardtype == "Theory")
            {
                string TQuery = $@"UPDATe ug SET ug.ExternalMarks=-3
                    FROM dbo.ARGPersonalInformation_{programme} P JOIN dbo.ARGSelectedCombination_{programme} SC 
                    ON SC.Student_ID = P.Student_ID
                    JOIN dbo.ADMCombinationMaster CM ON CM.Combination_ID = SC.Combination_ID
                    JOIN dbo.ADMCourseMaster CMs ON CMs.Course_ID = CM.Course_ID
                    JOIN dbo.{programme}_Semester{semester} UG ON UG.Student_ID = P.Student_ID  AND UG.Student_ID = SC.Student_ID
                    JOIN dbo.ARGStudentExamForm_{programme} Exm ON Exm.Student_ID = SC.Student_ID AND Exm.Semester = CM.Semester AND Exm.IsRegular=0 AND Exm.Status = 4
                    JOIN dbo.ARGStudentReExamForm RE ON RE.StudentExamForm_ID = Exm.StudentExamForm_ID AND RE.Subject_ID = UG.Subject_ID AND Exm.Year='2023'
                    JOIN dbo.ADMSubjectMaster ON ADMSubjectMaster.Subject_ID = UG.Subject_ID						 							
                    AND SC.IsVerified=1 AND Exm.ExamRollNumber IS NOT NULL  AND RE.FeeStatus IN (10,15) 
                    WHERE  CM.Semester={semester} AND RE.FeeStatus=10 AND (UG.ExternalMarks<22 OR UG.ExternalMarks IS NULL)	AND Exm.Year={examYear}
                    AND Exm.ExamRollNumber IS NOT NULL  AND RE.FeeStatus IN (10,15) AND Exm.IsRegular=0 AND Exm.Status = 4 AND SC.IsVerified=1
                    --ORDER BY UG.ExternalMarks,UG.ExternalUpdatedOn DESC";


                cmd.CommandText = TQuery;
                returndata = Convert.ToInt32(new MSSQLFactory().ExecuteNonQuery(cmd));
            }

            if (returndata > 0)
                return new MSSQLFactory().ExecuteNonQuery(cmd);
            else
                return 0;
        }

    }
}


