using CUSrinagar.DataManagers;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using CUSrinagar.Models;
using GeneralModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace CUSrinagar.BusinessManagers
{
    public class LibraryManager
    {
        public Task<List<LibraryForm>> GetStudentsAsync(PrintProgramme programme, Parameters parameter, bool IsRR = false)
        {
            return new TaskFactory().StartNew(() => GetStudents(programme, parameter, IsRR));
        }
        public List<LibraryForm> GetStudents(PrintProgramme programme, Parameters parameter, bool IsRR)
        {
            StudentDB studentDB = new StudentDB();
            programme = new RegistrationManager().MappingTable(programme);
            List<LibraryForm> libraryForms = studentDB.GetAllStudentsLibrary(programme, parameter, IsRR);
            string StudentsSubjects = string.Empty;
            List<ADMSubjectMaster> Subjects = null;
            List<StudentAdditionalSubject> _StudentAdditionalSubject = null;
            List<StudentAdditionalSubject> StudentAdditionalSubjectTemp = null;
            RegistrationDB _RegistrationDB = new RegistrationDB();

            short Semester = Convert.ToInt16(parameter.Filters.First(x => x.Column == "Semester").Value ?? "0");

            foreach (LibraryForm form in libraryForms ?? new List<LibraryForm>())
            {
                form.StudentAddress = studentDB.GetStudentAddress(programme, form.Student_ID);
                Subjects = new SubjectDB().GetList(studentDB.GetSelectedCombination(form.Combination_ID)?.CombinationSubjects.ToGuidList());
                //add additional subjects
                _StudentAdditionalSubject = new StudentManager().GetStudentAdditionalSubjects(form.Student_ID, Semester, programme);
                if (_StudentAdditionalSubject.IsNotNullOrEmpty())
                {
                    StudentAdditionalSubjectTemp = _StudentAdditionalSubject.Where(s => s.Subject != null)?.ToList();
                    if (StudentAdditionalSubjectTemp.IsNotNullOrEmpty())
                        Subjects.AddRange(StudentAdditionalSubjectTemp.Select(x => x.Subject));
                }

                form.StudentSubjects = Subjects?.OrderBy(x=>x.SubjectType)?.Select(x => x.SubjectFullName+ $" - ({x.SubjectType.GetEnumDescription()})").ToSingleStrings(" , ");
                form.QualifyingExam = string.Join($" , {Environment.NewLine}", _RegistrationDB.GetStudentAcademicDetails(form.Student_ID, programme)
                    ?.Select(x => x.ExamName + " - " + x.ExamBody)?.ToList() ?? new List<string>());
            }
            //dispose
            Subjects = null;
            _StudentAdditionalSubject = null;
            StudentAdditionalSubjectTemp = null;

            return libraryForms.IsNotNullOrEmpty() ? libraryForms.OrderBy(x => x.ClassRollNo.SortAlphaNumeric()).ToList() : libraryForms;

        }


        public ARGStudentAddress GetStudentAddress(PrintProgramme programme, Guid StudentID)
        {
            return new StudentDB().GetStudentAddress(programme, StudentID);
        }

        public Parameters AddPageFilters(string columnName, string columnValue, Parameters parameter)
        {
            parameter.Filters = parameter.Filters ?? new List<SearchFilter>();
            parameter.Filters.Add(new SearchFilter() { Column = columnName, Value = columnValue, GroupOperation = LogicalOperator.AND });
            return parameter;
        }

        public ADMCombinationMaster GetStudentSubjects(Guid CombinationID)
        {
            return new StudentDB().GetSelectedCombination(CombinationID);
        }

        public List<SelectListItem> FetchChildDDlValues(string id, string Type, string Semester)
        {
            List<SelectListItem> childDDL = new List<SelectListItem>();
            switch (Type)
            {
                case "Course":
                    {
                        PrintProgramme programme = (PrintProgramme)Convert.ToInt32(id);
                        childDDL = new CourseManager().GetCourseList(AppUserHelper.College_ID.Value, programme);
                        break;
                    }
                case "Combination":
                    {
                        childDDL = new CombinationManager().GetCombinationsDDL(AppUserHelper.College_ID.Value, Guid.Parse(id), Convert.ToInt32(Semester + ""));
                        break;
                    }
                case "Semester":
                    {
                        Guid CourseId = new Guid(id);
                        childDDL = GetAllSemesters(CourseId);
                        break;
                    }
            }

            return childDDL;
        }
        public List<SelectListItem> GetAllSemesters(Guid Course_Id)
        {
            ADMCourseMaster course = new CourseManager().GetCourseById(Course_Id);
            List<SelectListItem> listSemesters = new List<SelectListItem>();
            if (course != null)
            {
                string scheme = course.Scheme.ToString();
                for (int i = 1; i <= course.Duration; i++)
                {
                    listSemesters.Add(new SelectListItem() { Value = i.ToString(), Text = scheme + " " + i });
                }

            }
            return listSemesters;
        }
        public ADMSubjectMaster GetSubject(Guid SubjectID)
        {
            return new StudentDB().GetSubject(SubjectID);
        }

        public Parameters GetDefaultParameter(Parameters parameter)
        {
            parameter.PageInfo = new Paging() { DefaultOrderByColumn = "CreatedOn" };
            parameter.SortInfo = new Sort() { ColumnName = "ClassRollNo", OrderBy = SortOrder.Ascending };
            return parameter;
        }
    }
}
