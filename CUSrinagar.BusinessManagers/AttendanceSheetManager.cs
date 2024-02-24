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
using CUSrinagar.Extensions;

namespace CUSrinagar.BusinessManagers
{
    public class AttendanceSheetManager
    {
        public List<SelectListItem> GetPrintProgrammeDDL()
        {
            List<SelectListItem> childDDL = new List<SelectListItem>();
            foreach (var item in new ExaminationFormManager().GetAdmitPrintProgrammeDDL() ?? new List<PrintProgramme>())
            {
                childDDL.Add(new SelectListItem { Value = ((short)item).ToString(), Text = item.GetEnumDescription() });
            }
            return childDDL.Any() ? childDDL : null;
        }

        public List<SelectListItem> FetchChildDDlValues(string id, string Type)
        {
            List<SelectListItem> childDDL = new List<SelectListItem>();
            switch (Type)
            {
                case "Year":
                    {
                        childDDL = new AttendanceSheetManager().GetYearDDL();
                        break;
                    }
                case "Course":
                    {
                        PrintProgramme programme = (PrintProgramme)Convert.ToInt32(id);
                        //childDDL = new CourseManager().GetCourseList(AppUserHelper.College_ID.Value, programme);
                        childDDL = new AttendanceSheetDB().GetSemesterExamCourses(programme);
                        break;
                    }
                case "Semester":
                    {
                        PrintProgramme programme = new CourseManager().GetItem(Guid.Parse(id)).PrintProgramme;
                        childDDL = new AttendanceSheetManager().GetAllSemestersByPrintProgramme(programme);
                        break;
                    }
            }

            return childDDL;

        }

        public List<SelectListItem> FetchChildDDlValues2(Guid College_ID, string id, string Type)
        {
            List<SelectListItem> childDDL = new List<SelectListItem>();
            switch (Type)
            {
                case "Year":
                    {
                        childDDL = new AttendanceSheetManager().GetYearDDL();
                        break;
                    }
                case "Course":
                    {
                        PrintProgramme programme = (PrintProgramme)Convert.ToInt32(id);
                        //childDDL = new CourseManager().GetCourseList(AppUserHelper.College_ID.Value, programme);
                        childDDL = new AttendanceSheetDB().GetSemesterExamCourses(programme, College_ID);
                        break;
                    }
                case "Semester":
                    {
                        PrintProgramme programme = new CourseManager().GetItem(Guid.Parse(id)).PrintProgramme;
                        childDDL = new AttendanceSheetManager().GetAllSemestersByPrintProgramme(programme);
                        break;
                    }
            }

            return childDDL;

        }

        public List<SelectListItem> GetAllSemestersByPrintProgramme(PrintProgramme programme)
            => new AttendanceSheetDB().GetAllSemestersByPrintProgramme(programme);

        public List<SelectListItem> GetYearDDL() => new AttendanceSheetDB().GetYearDDL();

        public Task<List<AttendanceSheetForm>> GetAttendanceSheetsAsync(PrintProgramme programme, Guid Course_ID, int Batch, int ExamSemester, string ExamRollNo, Guid? Center_ID, Guid? CollegeID)
        {
            Guid College_ID = CollegeID == null ? (AppUserHelper.College_ID ?? Guid.Empty) : (CollegeID ?? Guid.Empty);
            string CollegeFullName = new CollegeDB().GetItem(College_ID)?.CollegeFullName ?? "";
            return new TaskFactory().StartNew(() => GetAttendanceSheets(programme, Course_ID, Batch, ExamSemester, ExamRollNo, College_ID, CollegeFullName, Center_ID));
        }

        public List<AttendanceSheetForm> GetAttendanceSheets(PrintProgramme programme, Guid Course_ID, int Batch, int ExamSemester, string ExamRollNo, Guid College_ID, string CollegeFullName, Guid? Center_ID)
        {
            StudentDB studentDB = new StudentDB();//
            programme = new RegistrationManager().MappingTable(programme);
            List<AttendanceSheetForm> attendanceForms = new List<AttendanceSheetForm>();

            ExamRollNo = Convert.ToString(ExamRollNo).Trim();

            attendanceForms = studentDB.GetAllStudentsAttendanceSheet(programme, Course_ID, Batch, ExamSemester, ExamRollNo, College_ID, Center_ID);
            List<ADMSubjectMaster> Subjects = null;
            List<StudentAdditionalSubject> _StudentAdditionalSubject = null;
            List<StudentAdditionalSubject> _StudentAdditionalSubjectTemp = null;

            ARGStudentExamForm _StudentExamForm = null;

            string CourseName = new CourseDB().GetItem(Course_ID)?.CourseFullName ?? string.Empty;
            Tuple<bool, string, List<ADMSubjectMaster>> _BlockSemesterAdmitCardOrNot;

            foreach (AttendanceSheetForm form in attendanceForms ?? new List<AttendanceSheetForm>())
            {
                //Get Center No.
                _StudentExamForm = new ExaminationFormDB().GetExamFormDetails(form.Student_ID, (short)ExamSemester, programme);
                if (_StudentExamForm != null)
                {
                    form.SemesterExamCenterDetail = new ExaminationFormManager().GetSemesterCenterAllotmentDetails(_StudentExamForm.StudentExamForm_ID);
                    if (_StudentExamForm.IsRegular)
                    {
                        //Is Regular
                        Subjects = new SubjectDB().GetList(studentDB.GetSelectedCombination(form.Combination_ID)?.CombinationSubjects.ToGuidList());
                        //add additional subjects
                        _StudentAdditionalSubject = new StudentManager().GetStudentAdditionalSubjects(form.Student_ID, (short)ExamSemester, programme);
                        if (_StudentAdditionalSubject.IsNotNullOrEmpty())
                        {
                            _StudentAdditionalSubjectTemp = _StudentAdditionalSubject.Where(s => s.Subject != null)?.ToList();
                            if (_StudentAdditionalSubjectTemp.IsNotNullOrEmpty())
                            {
                                Subjects.AddRange(_StudentAdditionalSubjectTemp.Select(x => x.Subject));
                            }
                        }

                        //Block Subjects
                        _BlockSemesterAdmitCardOrNot = new ExaminationFormManager().BlockSemesterAdmitCardOrNot(form.Student_ID, ExamSemester, programme, Batch, Subjects, CourseName, true);
                        form.Include = !_BlockSemesterAdmitCardOrNot.Item1;

                        if ((short)ExamSemester == 1)
                            form.StudentSubjects = _BlockSemesterAdmitCardOrNot.Item3?.Where(s => s.SubjectType != SubjectType.FirstSemesterExclusion)
                                .Select(x => x.SubjectFullName + "(" + x.SubjectType.GetEnumDescription() + ")").ToSingleStrings(" ,, ");
                        else
                            form.StudentSubjects = _BlockSemesterAdmitCardOrNot.Item3
                                ?.Select(x => x.SubjectFullName + "(" + x.SubjectType.GetEnumDescription() + ")").ToSingleStrings(" ,, ");
                    }
                    else
                    {
                        //Re-appear Student
                        form.Include = true;
                        Subjects = new ExaminationFormManager().GetAppliedReAppearSubjects(_StudentExamForm.StudentExamForm_ID);
                        if (Subjects.All(x => x.HasExaminationFee == false))
                        {
                            form.SemesterExamCenterDetail = null;
                        }
                        form.StudentSubjects = Subjects
                            ?.Select(x => x.SubjectFullName + "(" + x.SubjectType.GetEnumDescription() + ")").ToSingleStrings(" ,, ")
                            ?? "Your Subjects cannot be found. Please contact I.T CELL.";
                    }
                }
            }

            //dispose
            Subjects = null;
            _StudentAdditionalSubject = null;
            _StudentAdditionalSubjectTemp = null;
            return attendanceForms.IsNullOrEmpty() ? null : attendanceForms.Where(a => a.Include && a.SemesterExamCenterDetail != null).ToList();
        }

    }
}