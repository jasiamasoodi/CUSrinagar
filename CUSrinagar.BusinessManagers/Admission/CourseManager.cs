using System;
using System.Collections.Generic;
using System.Linq;
using CUSrinagar.Models;
using CUSrinagar.DataManagers;
using System.Web.Mvc;
using CUSrinagar.Enums;
using GeneralModels;
using CUSrinagar.Extensions;
using Terex;

namespace CUSrinagar.BusinessManagers
{
    public class CourseManager
    {
        public static PrintProgramme ProgrammeToPrintProgrammeMapping(Programme programme)
        {
            switch (programme)
            {
                case Programme.HS:
                case Programme.IG:
                case Programme.Professional:
                case Programme.Engineering:
                    return PrintProgramme.IH;
                case Programme.UG:
                    return PrintProgramme.UG;
                case Programme.PG:
                    return PrintProgramme.PG;
                default:
                    return PrintProgramme.UG;
            }
        }
        internal Guid GetCourseForIG(PrintProgramme chPrintProgramme, Guid course_ID)
        {
            return new CourseDB().GetCourseForIG(chPrintProgramme, course_ID);
        }
        #region add colleges of course merge
        //public void SetColleges()
        //{
        //    List<ARGCoursesApplied> listis = new CourseDB().GetEnglishStudentLists();
        //    foreach (ARGCoursesApplied ca in listis)
        //    {
        //        NEPPersonalInfo personalInformation = new NEPManager().GetNEPPersonalInfo(ca.Student_ID, PrintProgramme.UG).Item3;

        //        if (personalInformation != null)
        //        {
        //            short maxCollegePred = personalInformation.CollegePreference.Max(x => x.PreferenceNo);

        //            if (personalInformation.PersonalInformation.FormStatus == FormStatus.FeePaid)
        //            {
        //                if (personalInformation.PersonalInformation.Gender != "MALE")
        //                {
        //                    if (!personalInformation.CollegePreference.Any(x => x.College_ID == Guid.Parse("B5E689E6-75FB-44E9-8ED7-3E649C18B659")))//gwc
        //                    {
        //                        maxCollegePred++;
        //                        var v1 = new NEPCollegePreference
        //                        {
        //                            College_ID = Guid.Parse("B5E689E6-75FB-44E9-8ED7-3E649C18B659"),
        //                            PreferenceNo = maxCollegePred,
        //                            Student_ID = personalInformation.PersonalInformation.Student_ID
        //                        };
        //                        new NEPDB().Save(v1);
        //                    }
        //                }
        //                if (!personalInformation.CollegePreference.Any(x => x.College_ID == Guid.Parse("8B585643-CEB5-4C96-8328-862A9911CD51")))//asc
        //                {
        //                    maxCollegePred++;
        //                    var v2 = new NEPCollegePreference
        //                    {
        //                        College_ID = Guid.Parse("8B585643-CEB5-4C96-8328-862A9911CD51"),
        //                        PreferenceNo = maxCollegePred,
        //                        Student_ID = personalInformation.PersonalInformation.Student_ID
        //                    };

        //                    new NEPDB().Save(v2);
        //                }
        //                if (!personalInformation.CollegePreference.Any(x => x.College_ID == Guid.Parse("67C52693-7029-486F-AA46-798F69A95AFB"))) //aaa
        //                {
        //                    maxCollegePred++;
        //                    var v3 = new NEPCollegePreference
        //                    {
        //                        College_ID = Guid.Parse("67C52693-7029-486F-AA46-798F69A95AFB"),
        //                        PreferenceNo = maxCollegePred,
        //                        Student_ID = personalInformation.PersonalInformation.Student_ID
        //                    };

        //                    new NEPDB().Save(v3);
        //                }
        //            }
        //        }
        //        else
        //        {
        //            //kk
        //        }
        //    }
        //}

        //public void SetColleges()
        //{
        //    List<ARGCoursesApplied> listis = new CourseDB().GetEnglishStudentLists();
        //    foreach (ARGCoursesApplied ca in listis)
        //    {
        //        NEPPersonalInfo personalInformation = new NEPManager().GetNEPPersonalInfo(ca.Student_ID, PrintProgramme.UG).Item3;

        //        if (personalInformation != null)
        //        {
        //            short maxCollegePred = personalInformation.CollegePreference.Max(x => x.PreferenceNo);

        //            if (personalInformation.PersonalInformation.FormStatus == FormStatus.FeePaid)
        //            {
        //                if (!personalInformation.CollegePreference.Any(x => x.College_ID == Guid.Parse("AAF597C1-B122-4276-B143-38F698B7C18C")))//hyd
        //                {
        //                    maxCollegePred++;
        //                    var v1 = new NEPCollegePreference
        //                    {
        //                        College_ID = Guid.Parse("AAF597C1-B122-4276-B143-38F698B7C18C"),
        //                        PreferenceNo = maxCollegePred,
        //                        Student_ID = personalInformation.PersonalInformation.Student_ID
        //                    };
        //                    new NEPDB().Save(v1);
        //                }
        //            }
        //        }
        //        else
        //        {
        //            //kk
        //        }
        //    }
        //}
        #endregion
        public string GetPrintProgrammesInClause(Guid course_ID)
        {
            ADMCourseMaster aDMCourseMaster = new CourseManager().GetCourseById(course_ID);
            string printPrg = "";
            switch (aDMCourseMaster.PrintProgramme)
            {
                case PrintProgramme.IH:
                case PrintProgramme.UG:
                    printPrg = ((short)Programme.UG).ToString() + "," + ((short)Programme.IG).ToString() + "," + ((short)Programme.Professional).ToString() + "," + ((short)Programme.Engineering).ToString() + "," + ((short)Programme.HS).ToString();
                    break;
                case PrintProgramme.PG:
                    printPrg = ((short)Programme.PG).ToString();
                    break;
                case PrintProgramme.BED:
                    printPrg = ((short)PrintProgramme.BED).ToString();
                    break;
            }
            return printPrg;
        }
        public string GetPrintProgrammesBySkillProgramme(Guid course_ID)
        {
            ADMCourseMaster aDMCourseMaster = new CourseManager().GetCourseById(course_ID);
            string printPrg = "";
            switch (aDMCourseMaster.PrintProgramme)
            {
                case PrintProgramme.IH:
                    printPrg = ((short)Programme.IG).ToString() + "," + ((short)Programme.Professional).ToString() + "," + ((short)Programme.Engineering).ToString() + "," + ((short)Programme.HS).ToString();
                    break;
                case PrintProgramme.UG:
                    printPrg = ((short)Programme.UG).ToString();
                    break;
                case PrintProgramme.PG:
                    printPrg = ((short)Programme.PG).ToString();
                    break;
                case PrintProgramme.BED:
                    printPrg = ((short)PrintProgramme.BED).ToString();
                    break;
            }
            return printPrg;
        }

        internal Guid GetChildCourse(Guid Course_ID)
        {
            Tuple<string, List<Guid[]>> tuple = new EntranceCentersDB().GetCourseGrouping(PrintProgramme.IH);
            Guid oppcourse = Guid.Empty;
            if (tuple != null && tuple.Item2.Any())
            {
                oppcourse =
                     tuple.Item2.Find(x => x[0] == Course_ID || x[1] == Course_ID).ToList().FindIndex(x => x == Course_ID) == 0
                     ? tuple.Item2.Find(x => x[0] == Course_ID || x[1] == Course_ID).ElementAt(1)
                     : tuple.Item2.Find(x => x[0] == Course_ID || x[1] == Course_ID).ElementAt(0);
            }
            return oppcourse;
        }

        public List<ADMCourseMaster> GetCollegeCourseMapping(Guid College_ID)
        {
            if (College_ID == Guid.Empty)
                return null;
            return new CourseDB().GetCollegeCourseMapping(College_ID);
        }
        public List<ADMCollegeCourseMapping> GetCollegeCourseMappingList(Guid College_ID)
        {
            if (College_ID == Guid.Empty)
                return null;
            return new CourseDB().GetCollegeCourseMappingList(College_ID);
        }

        public List<Guid> GetCourseGuidList(string CourseNameLike)
        {
            return new CourseDB().GetCourseGuidList(CourseNameLike);
        }
        public List<ADMCourseMaster> GetCourseLists(string CourseNameLike)
        {
            return new CourseDB().GetCourseLists(CourseNameLike);
        }
        public List<ADMCourseMaster> GetCourseLists(Programme programme)
        {
            return new CourseDB().GetCourseLists(programme);
        }
        public List<ADMCourseMaster> GetUGCourseLists()
        {
            return new CourseDB().GetUGCourseLists();
        }
        public List<ADMCourseMaster> GetUGCourseListsCollegeWise()
        {
            return new CourseDB().GetUGCourseListsCollegeWise();
        }
        public List<ARGCoursesApplied> GetCoursesAppliedMapping(Guid College_ID)
        {
            if (College_ID == Guid.Empty)
                return null;
            return new CourseDB().GetCoursesAppliedMapping(College_ID);
        }
        public List<ARGCoursesApplied> GetCoursesAppliedMapping(Guid College_ID, PrintProgramme printProgramme)
        {
            if (College_ID == Guid.Empty)
                return null;
            return new CourseDB().GetCoursesAppliedMapping(College_ID, printProgramme);
        }

        public ADMCourseMaster GetCompactItem(Guid Course_ID)
        {
            return new CourseDB().GetItem(Course_ID);
        }
        public Guid GetStudentCourse_ID(string CUSRegistrationNoORStudentFormNo, PrintProgramme programme)
        {
            programme = new RegistrationManager().MappingTable(programme);
            return new CourseDB().GetStudentCourse_ID(CUSRegistrationNoORStudentFormNo + "", programme);
        }
        public ADMCourseMaster GetItem(Guid Course_ID)
        {
            var item = new CourseDB().GetItem(Course_ID);
            if (item == null)
                return null;
            item.Subjects = new SubjectsManager().GetList(Course_ID);
            return item;
        }

        public Programme GetCourseProgramme(Guid course_Id)
        {
            return (new CourseDB().GetCourseProgramme(course_Id) ?? new ADMCourseMaster()).Programme;
        }

        public ADMCourseMaster GetItem(ADMCourseMaster _Search, bool withAllDefaultSubjects, Guid? College_ID = null, string CombinationSemester = null)
        {
            if (_Search == null) return null;
            CombinationManager _combinationManager = new CombinationManager();
            ADMCourseMaster _ADMCourseMaster = new CourseDB().GetItem(_Search.Course_ID);
            if (_ADMCourseMaster == null) return null;
            _ADMCourseMaster.CombinationDetails = _combinationManager.GetList(_Search, College_ID, CombinationSemester);

            if (withAllDefaultSubjects)
                _ADMCourseMaster.Subjects = new SubjectsManager().GetList(_Search.Course_ID);
            return _ADMCourseMaster;
        }


        public ADMCourseMaster GetItem(Parameters parameter)
        {
            if (parameter == null || parameter.Filters == null) return null;
            ADMCourseMaster _Search = new ADMCourseMaster() { Course_ID = new Guid((parameter.Filters.Where(x => x.Column.ToLower() == "course_id").FirstOrDefault().Value)), Status = true };
            CombinationManager _combinationManager = new CombinationManager();
            ADMCourseMaster _ADMCourseMaster = new CourseDB().GetItem(_Search.Course_ID);
            if (_ADMCourseMaster == null) return null;
            _ADMCourseMaster.CombinationDetails = _combinationManager.GetList(_Search, new Guid((parameter.Filters.Where(x => x.Column.ToLower() == "college_id").FirstOrDefault().Value)), parameter.Filters.Where(x => x.Column.ToLower() == "semester").FirstOrDefault().Value)?.OrderBy(x => x.CombinationCode.SortAlphaNumeric())?.ToList();

            _ADMCourseMaster.Subjects = new SubjectsManager().GetList(_Search.Course_ID);
            return _ADMCourseMaster;
        }
        public List<ARGCoursesApplied> GetCourseListForRegistration(PrintProgramme printProgramme)
        {
            return new CourseDB().GetCourseListForRegistration(printProgramme);
        }


        public List<ARGCoursesApplied> RemoveIntMed(List<ARGCoursesApplied> _ARGCoursesApplied, bool IsBEdMEd)
        {
            if (IsBEdMEd)
            {
                ARGCoursesApplied coursesAppliedIntMEd = _ARGCoursesApplied.FirstOrDefault(x => x.Course_ID == new Guid("FC32E138-4EE2-4DA2-9453-5C8368180BC3"));
                if (coursesAppliedIntMEd != null)
                    return new List<ARGCoursesApplied> { coursesAppliedIntMEd };
                else
                    return null;
            }
            else
            {
                ARGCoursesApplied coursesAppliedIntMEd = _ARGCoursesApplied.FirstOrDefault(x => x.Course_ID == new Guid("FC32E138-4EE2-4DA2-9453-5C8368180BC3"));
                if (coursesAppliedIntMEd != null)
                    _ARGCoursesApplied.Remove(coursesAppliedIntMEd);
            }
            return _ARGCoursesApplied;
        }

        public List<ARGCoursesApplied> GetCourseListForAdminRegistration(PrintProgramme printProgramme)
        {
            return new CourseDB().GetCourseListForAdminRegistration(printProgramme);
        }

        public List<ARGCoursesApplied> GetCourseListForAdminRegistrationCollegeWise(PrintProgramme printProgramme, Guid College_ID)
        {
            return new CourseDB().GetCourseListForAdminRegistrationCollegeWise(printProgramme, College_ID);
        }

        public List<PrintProgramme> GetOpenResistrationProgrammeCategories()
        {
            return new CourseDB().GetClosedResistrationProgrammeCategories();
        }
        public List<SelectListItem> GetCourseList(Guid College_ID)
        {
            return new CourseDB().GetCourseList(College_ID);
        }

        public List<SelectListItem> GetCourseListNEP(Guid College_ID)
        {
            return new CourseDB().GetCourseListNEP(College_ID);
        }

        public SelectListItem GetSkillCourse()
        {
            return new CourseDB().GetSkillCourse();
        }
        public List<SelectListItem> GetOtherSubjectCourse()
        {
            return new CourseDB().GetOtherSubjectCourse();
        }

        internal string GetCourse(string courses)
        {
            return new CourseDB().GetCourses(courses);
        }

        public List<SelectListItem> GetCourseList()
        {
            return new CourseDB().GetCourseList();
        }
        public List<SelectListItem> GetAllCourseList()
        {
            return new CourseDB().GetAllCourseList();
        }
        public List<SelectListItem> GetCourseList(Guid College_ID, PrintProgramme programme)
        {
            return new CourseDB().GetCourseList(College_ID, programme);
        }
        public List<SelectListItem> GetCourseList(Guid? College_ID, PrintProgramme? printProgramme, Programme? programme)
        {
            return new CourseDB().GetCourseList(College_ID, printProgramme, programme);
        }

        public List<SelectListItem> GetCollegeList(Guid? Course_ID, PrintProgramme? printProgramme, Programme? programme)
        {
            return new CourseDB().GetCollegeList(Course_ID, printProgramme, programme);
        }
        public List<ADMCourseMaster> GetAllCourseList(Guid College_ID)
        {
            return new CourseDB().GetAllCourseList(College_ID);
        }

        public List<SelectListItem> CoursesSelectListItem()
        {
            return new CourseDB().CoursesSelectListItem();
        }
        public List<SelectListItem> SelectList(Parameters parameter)
        {
            return new CourseDB().SelectList(parameter);
        }

        public List<SelectListItem> SelectListDept(Parameters parameter)
        {
            return new CourseDB().SelectListDept(parameter);
        }

        public List<SelectListItem> GetAllCoursesByProgramme(int programmeId, short? status = null, bool checkHasCom = false, bool checkSelectionlistAllowed = false)
        {
            return new CourseDB().GetAllCoursesByProgramme(programmeId, status, checkHasCom, checkSelectionlistAllowed: checkSelectionlistAllowed);
        }
        public List<SelectListItem> GetAllCoursesByProgramme(Programme Programme)
        {
            return new CourseDB().GetAllCoursesByProgramme(Programme);
        }
        public List<SelectListItem> GetAllCoursesByPrintProgramme(PrintProgramme printProgramme)
        {
            return new CourseDB().GetAllCoursesByPrintProgramme(printProgramme);
        }
        public ADMCourseMaster GetCourseById(Guid course_Id)
        {
            ADMCourseMaster aDMCourseMaster = new CourseDB().GetCourseById(course_Id);
            if (aDMCourseMaster == null)
                return aDMCourseMaster;

            aDMCourseMaster.CourseMappingList = new CourseDB().GetCourseCollege(course_Id);
            //if (aDMCollegeCourseMappinglist?.Count > 0)
            //{
            //    aDMCourseMaster.Colleges = aDMCollegeCourseMappinglist.Select(x => x.College_ID).ToList();
            //    aDMCourseMaster.MeritBasislInTackCapacity = aDMCollegeCourseMappinglist.FirstOrDefault().MeritBasislInTackCapacity;
            //    aDMCourseMaster.SelfFinancedInTackCapacity = aDMCollegeCourseMappinglist.FirstOrDefault().SelfFinancedInTackCapacity;
            //}
            return aDMCourseMaster;
        }


        public List<ADMCourseMaster> GetOfferedCoursesForDisplay()
        {
            return new CourseDB().GetOfferedCoursesForDisplay();
        }

        #region Subject Info Page

        public List<SelectListItem> GetAllCourseListByCollegeID(Guid College_ID)
        {
            return new CourseDB().GetAllCourseListByCollegeID(College_ID);
        }

        public List<CourseCollegeWidget> GetAllCourseListByCollegeID(Guid College_ID, PrintProgramme? printProgramme)
        {
            return new CourseDB().GetAllCourseListByCollegeID(College_ID, printProgramme);
        }

        #endregion
        public List<ADMCourseMaster> GetAllCourseListOfAllColleges(Parameters parameter)
        {
            return new CourseDB().GetAllCourseListOfAllColleges(parameter);
        }

        public List<CourseList> List(Parameters parameter)
        {
            return new CourseDB().List(parameter);
        }

        #region Evalvator       
        public List<SelectListItem> GetAllCourseListOfAllColleges()
        {
            return new CourseDB().GetAllCourseListOfAllColleges();
        }
        #endregion
        public int AddCourse(ADMCourseMaster input)
        {
            int result = 0;
            input.Course_ID = Guid.NewGuid();
            input.Status = true;
            input.SetWorkFlow(RecordState.New);
            result = new CourseDB().AddCourse(input);
            if (result > 0)
            {

                result += new CourseDB().AddCollegeCourse(input);

            }
            return result;
        }

        public List<SelectListItem> GetProgrammes(Guid? college_ID)
        {
            List<SelectListItem> list = null;
            var programmes = new CourseDB().GetProgrammes(college_ID);
            if (programmes != null)
            {
                list = new List<SelectListItem>();
                foreach (var item in programmes)
                    list.Add(new SelectListItem() { Text = ((Programme)item).GetEnumDescription(), Value = item.ToString() });
            }
            return list;
        }

        public int EditCourse(ADMCourseMaster input)
        {
            int result = 0;

            List<string> ignoreQuery = new List<string>() {

                nameof(input.Course_ID),
                nameof(input.CreatedOn),
                nameof(input.CreatedBy),
                nameof(input.Status),
                nameof(input.Course_ID)

            };
            input.SetWorkFlow(RecordState.Old);
            result = new CourseDB().EditCourse(input, ignoreQuery, null);
            if (result > 0)
            {
                new CourseDB().DeleteAlreadyAssignedColleges(input.Course_ID);
                result += new CourseDB().AddCollegeCourse(input);

            }
            return result;
        }



        public List<AwardForSubjects> GetSubjectsForCourseAndCollege(Guid college_ID, Guid course_ID, PrintProgramme printProgramme, short semester, short batch)
        {
            return new CourseDB().GetSubjectsForCourseAndCollege(college_ID, course_ID, printProgramme, semester, batch);
        }
        public PrintProgramme? GetClosedResistrationProgrammeCategoriesIBEdMEd()
        {
            return new CourseDB().GetClosedResistrationProgrammeCategoriesIBEdMEd();
        }

        internal Guid GetCollegefromCourseMapping(Guid Student_ID, Guid Course_Id, PrintProgramme printProgramme)
        {
            return new CourseDB().GetCollegefromCourseMapping(Student_ID, Course_Id, printProgramme);
        }

        public IEnumerable<SelectListItem> GetCoursesForSyllabus(List<Programme> programmes)
        {
            return new CourseDB().GetCoursesForSyllabus(programmes);
        }

        public List<Guid> GetSkillGECourseIDsByProgramme(Programme programme)
        {
            return new CourseDB().GetSkillGECourseIDsByProgramme(programme);
        }
    }
}
