using CUSrinagar.DataManagers;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using CUSrinagar.Models;
using GeneralModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace CUSrinagar.BusinessManagers
{
    public class MarksCardManager
    {


        public void GenerateTransscriptsOld(PrintProgramme PrintProgramme, int Batch, Guid Course_ID, string CusRegistrationNo, string HaveGap, string AllowExtraCredits, string IsLateralEntry, string isDivisionImprovement)
        {

            bool IsGap = Convert.ToBoolean(HaveGap);
            bool AllowExtraCredit = Convert.ToBoolean(AllowExtraCredits);
            bool isLateralEntry = Convert.ToBoolean(IsLateralEntry);
            bool IsDivisionImprovement = Convert.ToBoolean(isDivisionImprovement);
            MSCGPA msCGPA = new MSCGPA();
            Guid Student_id = Guid.Empty;
            if (IsDivisionImprovement && (!string.IsNullOrEmpty(CusRegistrationNo)))
            {
                Student_id = new StudentManager().GetStudentIDByRegistrationNo(CusRegistrationNo);
                msCGPA = GetPreviousTranscript(Student_id);
                DeletePreviousTranscript(msCGPA.Student_ID);
            }

            string msg = "", submsg = "";
            List<Guid> StudentsGuid = new List<Guid>();
            int NoofSems = 0;
            int Credit = 0;
            bool IsInserted = true;
            ADMCourseMaster courseMaster = new CourseManager().GetCourseById(Course_ID);
            NoofSems = courseMaster.Duration;

            if (IsGap && !string.IsNullOrEmpty(CusRegistrationNo))
            {
                Guid CCourse_ID = new CourseManager().GetChildCourse(Course_ID);
                ADMCourseMaster CcourseMaster = new CourseManager().GetCourseById(CCourse_ID);
                int CNoofSems = CcourseMaster.Duration;
                if (CNoofSems > NoofSems)
                {
                    msg = "No Child Course found";
                    return;
                }
                Credit = isLateralEntry ? CcourseMaster.TotalCreditsForLateralEntry : CcourseMaster.TotalCredits;
                StudentsGuid = new MarksDB().GenerateStudentCreditInClause(Course_ID, Batch, Credit, PrintProgramme, CNoofSems, AllowExtraCredit, isLateralEntry, Student_id).ToList();
                //Get total credits of students
                string StudentsGuidinclause = "  IN ('" + string.Join("','", StudentsGuid) + "')";
                StudentsGuid = new MarksDB().GenerateFinalStudentCreditInClause(StudentsGuidinclause, Credit, PrintProgramme, CNoofSems, AllowExtraCredit, Student_id).ToList();

                //Additional Subjects
                List<Guid> AddStudentsGuid = new MarksDB().GenerateStudentCreditInClause(Course_ID, Batch, (Credit + (NoofSems * 6)), PrintProgramme, CNoofSems, AllowExtraCredit, isLateralEntry, Student_id).ToList();
                StudentsGuid.AddRange(AddStudentsGuid);
                Course_ID = CCourse_ID;
                NoofSems = CNoofSems;
            }
            else
            {
                Credit = isLateralEntry ? courseMaster.TotalCreditsForLateralEntry : courseMaster.TotalCredits;
                StudentsGuid = new MarksDB().GenerateStudentCreditInClause(Course_ID, Batch, Credit, PrintProgramme, NoofSems, AllowExtraCredit, isLateralEntry, Student_id).ToList();
                //Get total credits of students
                string StudentsGuidinclause = "  IN ('" + string.Join("','", StudentsGuid) + "')";
                StudentsGuid = new MarksDB().GenerateFinalStudentCreditInClause(StudentsGuidinclause, Credit, PrintProgramme, NoofSems, AllowExtraCredit, Student_id).ToList();

                //Additional Subjects
                List<Guid> AddStudentsGuid = new MarksDB().GenerateStudentCreditInClause(Course_ID, Batch, (Credit + (NoofSems * 6)), PrintProgramme, NoofSems, AllowExtraCredit, isLateralEntry, Student_id).ToList();
                StudentsGuid.AddRange(AddStudentsGuid);

            }

            string studentinclause = "  IN ('" + string.Join("','", StudentsGuid) + "')";
            if (StudentsGuid.Count <= 0)
            { IsInserted = false; }
            else
            {
                using (TransactionScope transaction = new TransactionScope())
                {
                    //else
                    {
                        // 1.insert into student marks data
                        //////////////////a) insert student subjects detail
                        //////////////////b) insert student marks detail
                        //////////////////c) insert student result  detail
                        //insert data semester wise of course one by one
                        for (int Semestr = 1; Semestr <= NoofSems; Semestr++)
                        {
                            if (IsInserted)
                            {
                                try
                                {
                                    //insert marks semster wise
                                    IsInserted = new MarksDB().GenerateMarks(PrintProgramme, Batch, Course_ID, Semestr, studentinclause);
                                }
                                catch (Exception e)
                                {
                                    submsg = " with Student Marks " + e.Message;
                                    IsInserted = false;
                                    break;
                                }
                            }
                        }

                        if (IsInserted)
                        //.sgpa of above students only
                        {
                            try
                            {
                                IsInserted = new MarksDB().GenerateSGPA(Course_ID, studentinclause);
                            }
                            catch (Exception e)
                            {
                                submsg = " with SGPA " + e.Message;
                                IsInserted = false;
                            }
                        }

                        if (IsInserted)
                        {
                            try
                            {
                                //3.calculate cgpa
                                IsInserted = new MarksDB().GenerateCGPA(Course_ID, studentinclause);
                            }
                            catch (Exception e)
                            {
                                submsg = " with CGPA " + e.Message;
                                IsInserted = false;
                            }
                        }

                        if (IsInserted)
                        //3.assign serial
                        {
                            try
                            {
                                int LastCount = new MarksDB().GetLastCount(PrintProgramme);
                                if (LastCount > 0 || DateTime.Now.Month <= 2)
                                {
                                    IsInserted = new MarksDB().GenerateSerialNoandOthers(studentinclause, LastCount, PrintProgramme, Guid.Empty);
                                    new MarksDB().UpdateDate(studentinclause, PrintProgramme, NoofSems, Guid.Empty);
                                    if (IsDivisionImprovement && (!string.IsNullOrEmpty(CusRegistrationNo)))
                                    {
                                        new MarksDB().UpdateDateForDivImp(studentinclause, PrintProgramme, msCGPA);
                                    }
                                }
                                else
                                {
                                    IsInserted = false;
                                    submsg = " with serial number";
                                }
                            }
                            catch (Exception e)
                            {
                                submsg = " with CGPA " + e.Message;
                                IsInserted = false;
                            }
                        }
                    }
                    if (IsInserted)
                    {
                        try
                        {

                            new StudentDB().UpdatePassout(studentinclause, PrintProgramme);
                        }
                        catch (Exception)
                        {
                        }
                        msg = "save successfully";
                        transaction.Complete();
                    }
                    else
                    {
                        msg = "Some error occured" + submsg;
                    }
                }
            }
        }
        public void GenerateTransscripts(Parameters parameter)
        {

            bool IsGap = Convert.ToBoolean(parameter.Filters.Where(x => x.Column == "HaveGap").First().Value);
            bool AllowExtraCredit = Convert.ToBoolean(parameter.Filters.Where(x => x.Column == "AllowExtraCredits").First().Value);
            bool isLateralEntry = Convert.ToBoolean(parameter.Filters.Where(x => x.Column == "IsLateralEntry").First().Value);
            bool IsDivisionImprovement = Convert.ToBoolean(parameter.Filters.Where(x => x.Column == "IsDivisionImprovement").First().Value);
            string CusRegistrationNo = parameter.Filters.Where(x => x.Column == "CUSRegistrationNo")?.FirstOrDefault()?.Value;
            Guid Course_ID = Guid.Parse(parameter.Filters.Where(x => x.Column == "Course_ID").FirstOrDefault().Value);
            int CurrentBatch = Convert.ToInt32(parameter.Filters.Where(x => x.Column == "SemesterBatch").FirstOrDefault().Value);
            PrintProgramme PrintProgramme = (PrintProgramme)int.Parse(parameter.Filters.Where(x => x.Column == "PrintProgramme").FirstOrDefault().Value);


            MSCGPA msCGPA = new MSCGPA();
            string msg = "", submsg = "";
            List<Guid> StudentsGuid = new List<Guid>();
            int Credit = 0;
            bool IsInserted = true;
            Guid Student_ID = Guid.Empty;
            ADMCourseMaster courseMaster = new CourseManager().GetCourseById(Course_ID);
            int NoofSem = courseMaster.Duration;
            Guid CCourse_ID = Guid.Empty;

            if (!string.IsNullOrEmpty(CusRegistrationNo))//For One Student only
            {
                Student_ID = new StudentManager().GetStudentIDByRegistrationNo(CusRegistrationNo);
                if (Student_ID == null || Student_ID == Guid.Empty)
                { return; }
                if (IsDivisionImprovement)
                {
                    msCGPA = SetDivisionImprovementSettings(Student_ID);

                }
                if (IsGap)
                {
                    CCourse_ID = new CourseManager().GetChildCourse(Course_ID);
                    ADMCourseMaster CcourseMaster = new CourseManager().GetCourseById(CCourse_ID);
                    int CNoofSems = CcourseMaster.Duration;
                    if (CNoofSems > NoofSem)
                    {
                        msg = "No Child Course found";
                        return;
                    }
                    Credit = isLateralEntry ? CcourseMaster.TotalCreditsForLateralEntry : CcourseMaster.TotalCredits;
                    NoofSem = CNoofSems;

                }

            }
            if (!IsGap) { Credit = isLateralEntry ? courseMaster.TotalCreditsForLateralEntry : courseMaster.TotalCredits; }
            if (Credit == 0)
            { return; }
            StudentsGuid = new MarksDB().GenerateStudentCreditInClause(Course_ID, CurrentBatch, Credit, PrintProgramme, NoofSem, AllowExtraCredit, isLateralEntry, Student_ID).ToList();
            if (StudentsGuid.Count == 0)
            { return; }
            //Get total credits of students
            string StudentsGuidinclause = "  IN ('" + string.Join("','", StudentsGuid) + "')";
            StudentsGuid = new MarksDB().GenerateFinalStudentCreditInClause(StudentsGuidinclause, Credit, PrintProgramme, NoofSem, AllowExtraCredit, Student_ID).ToList();

            //Additional Subjects
            List<Guid> AddStudentsGuid = new MarksDB().GenerateStudentCreditInClause(Course_ID, CurrentBatch, (Credit + (NoofSem * 6)), PrintProgramme, NoofSem, AllowExtraCredit, isLateralEntry, Student_ID).ToList();
            StudentsGuid.AddRange(AddStudentsGuid);


            if (IsGap)
            {
                Course_ID = CCourse_ID;

            }

            string studentinclause = "  IN ('" + string.Join("','", StudentsGuid) + "')";
            if (StudentsGuid.Count <= 0)
            { IsInserted = false; }
            else
            {
                using (TransactionScope transaction = new TransactionScope())
                {
                    //else
                    {
                        // 1.insert into student marks data
                        //////////////////a) insert student subjects detail
                        //////////////////b) insert student marks detail
                        //////////////////c) insert student result  detail
                        //insert data semester wise of course one by one
                        for (int Semestr = 1; Semestr <= NoofSem; Semestr++)
                        {
                            if (IsInserted)
                            {
                                try
                                {
                                    //insert marks semster wise
                                    IsInserted = new MarksDB().GenerateMarks(PrintProgramme, CurrentBatch, Course_ID, Semestr, studentinclause);
                                }
                                catch (Exception e)
                                {
                                    submsg = " with Student Marks " + e.Message;
                                    IsInserted = false;
                                    break;
                                }
                            }
                        }

                        if (IsInserted)
                        //.sgpa of above students only
                        {
                            try
                            {
                                IsInserted = new MarksDB().GenerateSGPA(Course_ID, studentinclause);
                            }
                            catch (Exception e)
                            {
                                submsg = " with SGPA " + e.Message;
                                IsInserted = false;
                            }
                        }

                        if (IsInserted)
                        {
                            try
                            {
                                //3.calculate cgpa
                                IsInserted = new MarksDB().GenerateCGPA(Course_ID, studentinclause);
                            }
                            catch (Exception e)
                            {
                                submsg = " with CGPA " + e.Message;
                                IsInserted = false;
                            }
                        }

                        if (IsInserted)
                        //3.assign serial
                        {
                            try
                            {
                                int LastCount = new MarksDB().GetLastCount(PrintProgramme);
                                if (LastCount > 0 || DateTime.Now.Month <= 2)
                                {
                                    IsInserted = new MarksDB().GenerateSerialNoandOthers(studentinclause, LastCount, PrintProgramme, Guid.Empty);
                                    new MarksDB().UpdateDate(studentinclause, PrintProgramme, NoofSem, Guid.Empty);
                                    if (IsDivisionImprovement && (!string.IsNullOrEmpty(CusRegistrationNo)))
                                    {
                                        new MarksDB().UpdateDateForDivImp(studentinclause, PrintProgramme, msCGPA);
                                    }
                                }
                                else
                                {
                                    IsInserted = false;
                                    submsg = " with serial number";
                                }
                            }
                            catch (Exception e)
                            {
                                submsg = " with CGPA " + e.Message;
                                IsInserted = false;
                            }
                        }
                    }
                    if (IsInserted)
                    {
                        try
                        {

                            new StudentDB().UpdatePassout(studentinclause, PrintProgramme);
                        }
                        catch (Exception)
                        {
                        }
                        msg = "save successfully";
                        transaction.Complete();
                    }
                    else
                    {
                        msg = "Some error occured" + submsg;
                    }
                }
            }
        }

        public List<MSTranscriptDegreeSettings> GetTranscriptSetting(Parameters parameter)
        {
            return new MarksDB().GetTranscriptSetting(parameter);

        }

        public void GenerateTransscriptsIG(Parameters parameter)
        {
            bool IsGap = Convert.ToBoolean(parameter.Filters.Where(x => x.Column == "HaveGap").First().Value);
            bool AllowExtraCredit = Convert.ToBoolean(parameter.Filters.Where(x => x.Column == "AllowExtraCredits").First().Value);
            bool isLateralEntry = Convert.ToBoolean(parameter.Filters.Where(x => x.Column == "IsLateralEntry").First().Value);
            bool IsDivisionImprovement = Convert.ToBoolean(parameter.Filters.Where(x => x.Column == "IsDivisionImprovement").First().Value);
            string CusRegistrationNo = parameter.Filters.Where(x => x.Column == "CUSRegistrationNo")?.FirstOrDefault()?.Value;
            Guid Course_ID = Guid.Parse(parameter.Filters.Where(x => x.Column == "Course_ID").FirstOrDefault().Value);
            int CurrentBatch = Convert.ToInt32(parameter.Filters.Where(x => x.Column == "SemesterBatch").FirstOrDefault().Value);
            PrintProgramme PrintProgramme = (PrintProgramme)int.Parse(parameter.Filters.Where(x => x.Column == "PrintProgramme").FirstOrDefault().Value);


            MSCGPA msCGPA = new MSCGPA();
            string msg = "", submsg = "";
            List<Guid> StudentsGuid = new List<Guid>();
            int Credit = 0;
            bool IsInserted = true;
            Guid Student_ID = Guid.Empty;
            ADMCourseMaster courseMaster = new CourseManager().GetCourseById(Course_ID);
            int NoofSem = courseMaster.Duration;
            Guid CCourse_ID = Guid.Empty;

            if (!string.IsNullOrEmpty(CusRegistrationNo))//For One Student only
            {

                Student_ID = new StudentManager().GetStudentIDByRegistrationNo(CusRegistrationNo, PrintProgramme);
                if (IsDivisionImprovement)
                {
                    msCGPA = SetDivisionImprovementSettings(Student_ID);

                }
                if (IsGap)
                {
                    CCourse_ID = new CourseManager().GetChildCourse(Course_ID);
                    ADMCourseMaster CcourseMaster = new CourseManager().GetCourseById(CCourse_ID);
                    int CNoofSems = CcourseMaster.Duration;
                    if (CNoofSems > NoofSem)
                    {
                        msg = "No Child Course found";
                        return;
                    }
                    Credit = isLateralEntry ? CcourseMaster.TotalCreditsForLateralEntry : CcourseMaster.TotalCredits;
                    NoofSem = CNoofSems;

                }

            }
            if (!IsGap) { Credit = isLateralEntry ? courseMaster.TotalCreditsForLateralEntry : courseMaster.TotalCredits; }

            StudentsGuid = new MarksDB().GenerateStudentCreditInClause(Course_ID, CurrentBatch, Credit, PrintProgramme, NoofSem, AllowExtraCredit, isLateralEntry, Student_ID).ToList();
            //Get total credits of students
            string StudentsGuidinclause = "  IN ('" + string.Join("','", StudentsGuid) + "')";
            StudentsGuid = new MarksDB().GenerateFinalStudentCreditInClause(StudentsGuidinclause, Credit, PrintProgramme, NoofSem, AllowExtraCredit, Student_ID).ToList();

            //Additional Subjects
            List<Guid> AddStudentsGuid = new MarksDB().GenerateStudentCreditInClause(Course_ID, CurrentBatch, (Credit + (NoofSem * 6)), PrintProgramme, NoofSem, AllowExtraCredit, isLateralEntry, Student_ID).ToList();
            StudentsGuid.AddRange(AddStudentsGuid);


            if (IsGap)
            {
                Course_ID = CCourse_ID;

            }


            string studentinclause = "  IN ('" + string.Join("','", StudentsGuid) + "')";
            if (StudentsGuid.Count <= 0)
            { IsInserted = false; }
            else
            {
                using (TransactionScope transaction = new TransactionScope())
                {
                    int loop = 0;

                CreateTranscript:
                    {
                        PrintProgramme ChPrintProgramme = (loop == 0) ? PrintProgramme.IH : PrintProgramme.PG;
                        Guid ChCourse_ID = new CourseManager().GetCourseForIG(ChPrintProgramme, Course_ID);
                        if (courseMaster.Course_ID == Guid.Parse("32996CE4-5613-4A2D-A565-030390D7F496"))
                        {
                            if (ChPrintProgramme == PrintProgramme.PG)
                            { ChCourse_ID = Guid.Parse("5C1695B0-5485-4ACA-B6C4-FA264BF34303"); }
                            else
                            { ChCourse_ID = Guid.Parse("9B15F7E4-967D-453F-BEE8-8B624FE6D0DF"); }
                        }
                        if (courseMaster.Course_ID == Guid.Parse("DC9595E4-B4DC-41E3-AEB9-446C0B990BCF"))
                        {
                            if (ChPrintProgramme == PrintProgramme.PG)
                            { ChCourse_ID = Guid.Parse("880B538E-3BD0-4CB0-ACD2-7E4D0AF2B4E8"); }
                            else
                            { ChCourse_ID = Guid.Parse("4E7A4B78-B87C-4666-A517-069656D55281"); }
                        }
                        int semesterstart = (loop == 0) ? 1 : 7; int semesterend = (loop == 0) ? 6 : 10; int credit = new CourseManager().GetCourseById(ChCourse_ID).TotalCredits;

                        // 1.insert into student marks data
                        //////////////////a) insert student subjects detail
                        //////////////////b) insert student marks detail
                        //////////////////c) insert student result  detail
                        //insert data semester wise of course one by one
                        for (int Semestr = semesterstart; Semestr <= semesterend; Semestr++)
                        {
                            int tblSem = Semestr > 6 ? Semestr - 6 : Semestr;
                            if (IsInserted)
                            {
                                try
                                {
                                    //insert marks semster wise
                                    IsInserted = new MarksDB().GenerateMarks(ChPrintProgramme, CurrentBatch, ChCourse_ID, Semestr, studentinclause, tblSem);
                                }
                                catch (Exception e)
                                {
                                    submsg = " with Student Marks " + e.Message;
                                    IsInserted = false;
                                    break;
                                }
                            }
                        }

                        if (IsInserted)
                        //.sgpa of above students only
                        {
                            try
                            {
                                IsInserted = new MarksDB().GenerateSGPA(ChCourse_ID, studentinclause);
                            }
                            catch (Exception e)
                            {
                                submsg = " with SGPA " + e.Message;
                                IsInserted = false;
                            }
                        }

                        if (IsInserted)
                        {
                            try
                            {
                                //3.calculate cgpa
                                IsInserted = new MarksDB().GenerateCGPA(ChCourse_ID, studentinclause);
                            }
                            catch (Exception e)
                            {
                                submsg = " with CGPA " + e.Message;
                                IsInserted = false;
                            }
                        }

                        if (IsInserted)
                        //3.assign serial
                        {
                            try
                            {
                                int LastCount = new MarksDB().GetLastCount(ChPrintProgramme);
                                if (LastCount > 0 || DateTime.Now.Month <= 2)
                                {
                                    IsInserted = new MarksDB().GenerateSerialNoandOthers(studentinclause, LastCount, ChPrintProgramme, ChCourse_ID);
                                    new MarksDB().UpdateDate(studentinclause, PrintProgramme, semesterend, ChCourse_ID);
                                    if (IsDivisionImprovement && (!string.IsNullOrEmpty(CusRegistrationNo)))
                                    {
                                        new MarksDB().UpdateDateForDivImp(studentinclause, ChPrintProgramme, msCGPA);
                                    }
                                }
                                else
                                {
                                    IsInserted = false;
                                    submsg = " with serial number";
                                }
                            }
                            catch (Exception e)
                            {
                                submsg = " with CGPA " + e.Message;
                                IsInserted = false;
                            }
                        }
                    }
                    loop++;
                    if (loop == 1 && IsInserted == true)
                    {
                        goto CreateTranscript;
                    }
                    if (IsInserted)
                    {
                        try
                        {

                            new StudentDB().UpdatePassout(studentinclause, PrintProgramme);
                        }
                        catch (Exception)
                        {
                        }
                        msg = "save successfully";
                        transaction.Complete();
                    }
                    else
                    {
                        msg = "Some error occured" + submsg;
                    }
                }
            }
        }
        public Tuple<List<Guid>,string>  GenerateTranscripts(MSTranscriptDegreeSettings mSTranscriptDegreeSettings, Parameters parameter)
        {

            PrintProgramme PrintProgramme = (PrintProgramme)int.Parse(parameter.Filters.Where(x => x.Column == "PrintProgramme").FirstOrDefault().Value);
            string CusRegistrationNo = parameter.Filters.Where(x => x.Column == "CUSRegistrationNo")?.FirstOrDefault()?.Value;
            int CurrentBatch = Convert.ToInt32(parameter.Filters.Where(x => x.Column == "SemesterBatch").FirstOrDefault().Value);
            Enum.TryParse((parameter.Filters.Where(x => x.Column == "Filter").First().Value).ToString(), out TranscriptFilters Type);
            bool setPassout = Convert.ToBoolean(parameter.Filters.Where(x => x.Column == "IsPassOut").FirstOrDefault().Value);
            List<Guid> StudentsGuid = new List<Guid>();


            if (Type == TranscriptFilters.IsLateralEntry)
                mSTranscriptDegreeSettings.TotalCredits = mSTranscriptDegreeSettings.LateralEntryTotalCredits;
            if (Type != TranscriptFilters.None)
            {
                if (string.IsNullOrEmpty(CusRegistrationNo))
                { return Tuple.Create(StudentsGuid,"Registration No is required"); }
            }


            MSCGPA msCGPA = new MSCGPA();
            string msg = "", submsg = "";
            bool IsInserted = true;
            Guid Student_ID = Guid.Empty;

           
            if (!string.IsNullOrEmpty(CusRegistrationNo))//For One Student only
            {
                Student_ID = new StudentManager().GetStudentIDByRegistrationNo(CusRegistrationNo);
                if (Student_ID == null || Student_ID == Guid.Empty)
                { return Tuple.Create(StudentsGuid, "Registration No Not Found"); }

            }
            if (Type == TranscriptFilters.IsDivisionImprovement || Type == TranscriptFilters.IsReevaluation)
            {
                msCGPA = GetAndRemovePreviousTranscript(Student_ID, mSTranscriptDegreeSettings, PrintProgramme);

            }
            //check always from semester first
            StudentsGuid = new MarksDB().GetPassedStudentList(mSTranscriptDegreeSettings.Course_ID, CurrentBatch, mSTranscriptDegreeSettings.TotalCredits, PrintProgramme,1, mSTranscriptDegreeSettings.SemesterTo, Type, Student_ID).ToList();
            //Get total credits of students
            if (StudentsGuid.Count == 0)
            { return Tuple.Create(StudentsGuid, "No Transcript Generated ,either already generated or result not clear"); }

            string StudentsGuidinclause = "  IN ('" + string.Join("','", StudentsGuid.Distinct()) + "')";
            StudentsGuid = new MarksDB().GetFinalPassedStudentList(StudentsGuidinclause, mSTranscriptDegreeSettings.TotalCredits, PrintProgramme, 1, mSTranscriptDegreeSettings.SemesterTo, Type).ToList();
            if (StudentsGuid.Count == 0)
            { return Tuple.Create(StudentsGuid, "No Transcript Generated ,either already generated or result not clear"); }

            //Additional Subjects
            List<Guid> AddStudentsGuid = new MarksDB().GetPassedStudentList(mSTranscriptDegreeSettings.Course_ID, CurrentBatch, mSTranscriptDegreeSettings.TotalCredits + (mSTranscriptDegreeSettings.SemesterTo * 6), PrintProgramme, 1, mSTranscriptDegreeSettings.SemesterTo, Type, Student_ID).ToList();
            StudentsGuid.AddRange(AddStudentsGuid);




            string studentinclause = "  IN ('" + string.Join("','", StudentsGuid.Distinct()) + "')";
            if (StudentsGuid.Count <= 0)
            { IsInserted = false; }
            else
            {
                using (TransactionScope transaction = new TransactionScope())
                {
                    //else
                    {
                        // 1.insert into student marks data
                        //////////////////a) insert student subjects detail
                        //////////////////b) insert student marks detail
                        //////////////////c) insert student result  detail
                        //insert data semester wise of course one by one
                        for (int Semestr = mSTranscriptDegreeSettings.SemesterFrom; Semestr <= mSTranscriptDegreeSettings.SemesterTo; Semestr++)
                        {
                            if (IsInserted)
                            {
                                try
                                {
                                    //insert marks semster wise
                                    IsInserted = new MarksDB().GenerateMarks(PrintProgramme, CurrentBatch, mSTranscriptDegreeSettings.Course_ID, Semestr, studentinclause);
                                }
                                catch (Exception e)
                                {
                                    submsg = " with Student Marks " + e.Message;
                                    IsInserted = false;
                                    break;
                                }
                            }
                        }

                        if (IsInserted)
                        //.sgpa of above students only
                        {
                            try
                            {
                                IsInserted = new MarksDB().GenerateSGPA(mSTranscriptDegreeSettings.Course_ID, studentinclause);
                            }
                            catch (Exception e)
                            {
                                submsg = " with SGPA " + e.Message;
                                IsInserted = false;
                            }
                        }

                        if (IsInserted)
                        {
                            try
                            {
                                //3.calculate cgpa
                                IsInserted = new MarksDB().GenerateCGPA(mSTranscriptDegreeSettings.Course_ID, mSTranscriptDegreeSettings.SemesterFrom, mSTranscriptDegreeSettings.SemesterTo, studentinclause);
                            }
                            catch (Exception e)
                            {
                                submsg = " with CGPA " + e.Message;
                                IsInserted = false;
                            }
                        }

                        if (IsInserted)
                        //3.assign serial
                        {
                            try
                            {
                                int LastCount = new MarksDB().GetLastCount(PrintProgramme);
                                if (LastCount > 0 || DateTime.Now.Month <= 2)
                                {
                                    IsInserted = new MarksDB().GenerateSerialNo(studentinclause, LastCount, PrintProgramme, mSTranscriptDegreeSettings.Course_ID, mSTranscriptDegreeSettings.SemesterTo);
                                    new MarksDB().UpdateDateAndNotificationNo(studentinclause, PrintProgramme, mSTranscriptDegreeSettings.SemesterTo, mSTranscriptDegreeSettings.Course_ID);
                                    if (Type == TranscriptFilters.IsDivisionImprovement)
                                    {
                                        new MarksDB().UpdateDateForDivImp(studentinclause, PrintProgramme, msCGPA, mSTranscriptDegreeSettings.SemesterTo, mSTranscriptDegreeSettings.Course_ID);
                                    }
                                }
                                else
                                {
                                    IsInserted = false;
                                    submsg = " with serial number";
                                }
                            }
                            catch (Exception e)
                            {
                                submsg = " with CGPA " + e.Message;
                                IsInserted = false;
                            }
                        }
                    }
                    if (IsInserted)
                    {
                        try
                        {
                            if (setPassout || Type == TranscriptFilters.IsExit)
                            { new StudentDB().UpdatePassout(studentinclause, PrintProgramme); }
                        }
                        catch (Exception)
                        {
                        }
                        msg = $"Transcript Generated successfully for {StudentsGuid.Count} students";
                        transaction.Complete();
                    }
                    else
                    {
                        msg = "Some error occured" + submsg;
                    }
                }
            }
            return Tuple.Create(StudentsGuid, msg);
        }
        private MSCGPA GetAndRemovePreviousTranscript(Guid Student_ID, MSTranscriptDegreeSettings mSTranscriptDegreeSettings, PrintProgramme printProgramme)
        {
            MSCGPA mSCGPA = GetPreviousTranscript(Student_ID, mSTranscriptDegreeSettings);
            DeletePreviousTranscript(Student_ID, mSTranscriptDegreeSettings, printProgramme);
            return mSCGPA;

        }

        private void DeletePreviousTranscript(Guid student_ID, MSTranscriptDegreeSettings mSTranscriptDegreeSettings, PrintProgramme printProgramme)
        {
            new StudentDB().DeletePreviousTranscript(student_ID, mSTranscriptDegreeSettings, printProgramme);
        }

        private MSCGPA GetPreviousTranscript(Guid student_id, MSTranscriptDegreeSettings mSTranscriptDegreeSettings)
        {
            return new StudentDB().GetPreviousTranscript(student_id, mSTranscriptDegreeSettings);
        }

        private MSCGPA SetDivisionImprovementSettings(Guid Student_ID)
        {
            MSCGPA mSCGPA = GetPreviousTranscript(Student_ID);
            DeletePreviousTranscript(Student_ID);
            return mSCGPA;

        }

        private void DeletePreviousTranscript(Guid student_ID)
        {
            new StudentDB().DeletePreviousTranscript(student_ID);
        }

        private MSCGPA GetPreviousTranscript(Guid student_id)
        {
            return new StudentDB().GetPreviousTranscript(student_id);
        }

        public List<MSStudentMarks> GetStudentMarks(Parameters parameter)
        {
            List<MSStudentMarks> list = new MarksDB().GetStudentMarks(parameter);
            return list;

        }
    }
}
