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
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CUSrinagar.BusinessManagers
{
    public class RRManager
    {
        public Parameters AddPageFilters(string columnName, string columnValue, Parameters parameter)
        {
            parameter.Filters = parameter.Filters ?? new List<SearchFilter>();
            parameter.Filters.Add(new SearchFilter() { Column = columnName, Value = columnValue, GroupOperation = LogicalOperator.AND });
            return parameter;
        }
        public Parameters GetDefaultParameter(Parameters parameter)
        {
            parameter.PageInfo = new Paging() { DefaultOrderByColumn = "CreatedOn" };
            parameter.SortInfo = new Sort() { ColumnName = "ClassRollNo", OrderBy = SortOrder.Ascending };
            return parameter;
        }

        public DataTable GetSemesterAdmissionDetails(SemesterAdmissionFilter semesterAdmissionFilter)
        {
            if (semesterAdmissionFilter == null)
                return null;
            semesterAdmissionFilter.SearchInPrintProgramme = new RegistrationManager().MappingTable(semesterAdmissionFilter.SearchInPrintProgramme);
            semesterAdmissionFilter.College_ID = AppUserHelper.College_ID;
            return new AdminRegistrationDB().GetSemesterAdmDetails(semesterAdmissionFilter);
        }


        public Task<List<StudentRegister>> GetStudentsAsync(PrintProgramme programme, Parameters parameter)
        {
            return new TaskFactory().StartNew(() => GetStudents(programme, parameter));
        }
        public List<StudentRegister> GetStudents(PrintProgramme programme, Parameters parameter)
        {
            StudentDB studentDB = new StudentDB();
            programme = new RegistrationManager().MappingTable(programme);
            List<StudentRegister> studentRegisters = studentDB.GetAllStudentsRegister(programme, parameter);
            string StudentsSubjects = string.Empty;
            List<ADMSubjectMaster> Subjects = null;
            List<StudentAdditionalSubject> _StudentAdditionalSubject = null;
            List<StudentAdditionalSubject> StudentAdditionalSubjectTemp = null;
            List<ARGSelectedCombination> SelectedCombinationTemp = null;

            SubjectDB _SubjectDB = new SubjectDB();
            StudentManager _StudentManager = new StudentManager();
            SemesterCombination semesterCombination;

            foreach (StudentRegister form in studentRegisters ?? new List<StudentRegister>())
            {
                form.SubjectsExams = new List<SemesterCombination>();

                SelectedCombinationTemp = studentDB.GetSelectedCombinations(form.Student_ID, programme);
                form.StudentAddress = studentDB.GetStudentAddress(programme, form.Student_ID);
                form.CGPA = studentDB.GetCGPAOnly(form.Student_ID);

                foreach (ARGSelectedCombination item in SelectedCombinationTemp ?? new List<ARGSelectedCombination>())
                {
                    Subjects = _SubjectDB.GetList(studentDB.GetSelectedCombination(item.Combination_ID)?.CombinationSubjects.ToGuidList());

                    //add additional subjects
                    _StudentAdditionalSubject = _StudentManager.GetStudentAdditionalSubjects(form.Student_ID, item.Semester, programme);
                    if (_StudentAdditionalSubject.IsNotNullOrEmpty())
                    {
                        StudentAdditionalSubjectTemp = _StudentAdditionalSubject.Where(s => s.Subject != null)?.ToList();
                        if (StudentAdditionalSubjectTemp.IsNotNullOrEmpty())
                            Subjects.AddRange(StudentAdditionalSubjectTemp.Select(x => x.Subject));
                    }

                    //call exam forms here
                    semesterCombination = studentDB.GetStudentExamDetails(programme, item.Semester, form.Student_ID) ?? new SemesterCombination();
                    semesterCombination.DateOfPassing = studentDB.GetDateOfSemesterPassing(form.Student_ID, programme, item.Semester);
                    //Compile all
                    semesterCombination.StudentSubjects = Subjects?.Select(x => x.SubjectFullName).ToSingleStrings(" , ");
                    semesterCombination.Semester = item.Semester;
                    semesterCombination.AdmissionDate = item.UpdatedOn ?? item.CreatedOn;
                    semesterCombination.IsVerifiedByCollege = item.IsVerified;
                    form.SubjectsExams.Add(semesterCombination);
                }
            }

            //dispose
            Subjects = null;
            _StudentAdditionalSubject = null;
            StudentAdditionalSubjectTemp = null;
            SelectedCombinationTemp = null;

            return studentRegisters.IsNotNullOrEmpty() ? studentRegisters.OrderBy(x => x.ClassRollNo.SortAlphaNumeric()).ToList() : studentRegisters;
        }
    }
}
