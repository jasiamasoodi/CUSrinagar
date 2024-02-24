using System;
using System.Collections.Generic;
using System.Linq;
using CUSrinagar.Models;
using Terex;
using System.Data.SqlClient;
using CUSrinagar.DataManagers.SQLQueries;
using System.Web.Mvc;
using GeneralModels;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using CUSrinagar.Models.ValidationAttrs;

namespace CUSrinagar.DataManagers
{
    public class ExaminationFormDB
    {
        public bool HasAlreadyAppliedForDivisionImprovement(Guid Student_ID, PrintProgramme printProgramme)
        {
            DivImprovementResponse maxAllowedTwoSemesters =
                                   new MSSQLFactory().GetObject<DivImprovementResponse>
                                   ($@"SELECT COUNT(e.Student_ID) AS TotalForms,ISNULL(MAX(e.Semester),0) AS SemesterApplied  FROM (
                                        SELECT DISTINCT e.Student_ID,e.Semester
                                        FROM dbo.ARGStudentExamForm_{printProgramme} e
                                        LEFT JOIN ARGStudentReExamForm r ON r.StudentExamForm_ID = e.StudentExamForm_ID
                                        WHERE Student_ID='{Student_ID}' AND Status={(short)FormStatus.Accepted} 
                                        AND IsDivisionImprovement=1) e;") ?? new DivImprovementResponse();
            return maxAllowedTwoSemesters.TotalForms > 0;
        }

        public DivImprovementResponse HasAlreadyAppliedForDivisionImprovement(Guid Student_ID, PrintProgramme printProgramme, short semester, int totalSubjects)
        {
            DivImprovementResponse improvementResponse =
                                    new MSSQLFactory().GetObject<DivImprovementResponse>
                                    ($@"SELECT COUNT(e.Student_ID) AS TotalForms,ISNULL(MAX(e.Semester),0) AS SemesterApplied  FROM (
                                        SELECT DISTINCT e.Student_ID,e.Semester
                                        FROM dbo.ARGStudentExamForm_{printProgramme} e
                                        LEFT JOIN ARGStudentReExamForm r ON r.StudentExamForm_ID = e.StudentExamForm_ID
                                        WHERE Student_ID='{Student_ID}' AND Status={(short)FormStatus.Accepted} 
                                        AND IsDivisionImprovement=1 AND Semester={semester}) e;") ?? new DivImprovementResponse();

            DivImprovementResponse maxAllowedTwoSemesters =
                                    new MSSQLFactory().GetObject<DivImprovementResponse>
                                    ($@"SELECT COUNT(e.Student_ID) AS TotalForms,ISNULL(MAX(e.Semester),0) AS SemesterApplied  FROM (
                                        SELECT DISTINCT e.Student_ID,e.Semester
                                        FROM dbo.ARGStudentExamForm_{printProgramme} e
                                        LEFT JOIN ARGStudentReExamForm r ON r.StudentExamForm_ID = e.StudentExamForm_ID
                                        WHERE Student_ID='{Student_ID}' AND Status={(short)FormStatus.Accepted} 
                                        AND IsDivisionImprovement=1) e;") ?? new DivImprovementResponse();

            improvementResponse.hasAlreadyApplied = improvementResponse.TotalForms > 0 || maxAllowedTwoSemesters.TotalForms > 1;
            return improvementResponse;
        }

        public ARGExamFormDownloadable GetRegularExamFormDownloadableWithoutDateValidation(ExaminationCourseCategory CourseCategory, short semester)
        {
            ARGExamFormDownloadable examFormDownloadable =
                 new MSSQLFactory().GetObject<ARGExamFormDownloadable>
                 (new ExaminationFormSQLQueries().GetRegularExamFormDownloadableWithoutDateValidation(CourseCategory, semester));

            if (examFormDownloadable?.AllowDownloadForm == false && AppUserHelper.CollegeCode.ToUpper().Equals("IASE") && CourseCategory == ExaminationCourseCategory.BED)
            {
                examFormDownloadable =
                                 new MSSQLFactory().GetObject<ARGExamFormDownloadable>
                                 (new ExaminationFormSQLQueries().GetRegularExamFormDownloadableWithoutDateValidation(ExaminationCourseCategory.UG, semester));
            }

            return examFormDownloadable;
        }

        public ARGExamFormDownloadable GetBacklogExamFormDownloadableWithoutDateValidation(ExaminationCourseCategory CourseCategory, short semester)
        {
            ARGExamFormDownloadable examFormDownloadable =
                new MSSQLFactory().GetObject<ARGExamFormDownloadable>
                (new ExaminationFormSQLQueries().GetBacklogExamFormDownloadableWithoutDateValidation(CourseCategory, semester));

            if (examFormDownloadable?.AllowDownloadForm == false && AppUserHelper.CollegeCode.ToUpper().Equals("IASE") && CourseCategory == ExaminationCourseCategory.BED)
            {
                examFormDownloadable =
                                 new MSSQLFactory().GetObject<ARGExamFormDownloadable>
                                 (new ExaminationFormSQLQueries().GetBacklogExamFormDownloadableWithoutDateValidation(ExaminationCourseCategory.UG, semester));
            }

            return examFormDownloadable;
        }

        public ARGExamFormDownloadable GetRegularExamFormDownloadable(ExaminationCourseCategory courseCategory, short semester)
        {
            return new MSSQLFactory().GetObject<ARGExamFormDownloadable>(new ExaminationFormSQLQueries().GetRegularExamFormDownloadable(courseCategory, semester));
        }

        public ARGExamFormDownloadable GetBacklogExamFormDownloadable(ExaminationCourseCategory courseCategory, short semester)
        {
            return new MSSQLFactory().GetObject<ARGExamFormDownloadable>(new ExaminationFormSQLQueries().GetBacklogExamFormDownloadable(courseCategory, semester));
        }


        public List<SelectListItem> RegularExamForSemester(PrintProgramme programme)
        {
            ExaminationCourseCategory courseCategory = new GeneralFunctions().GetExaminationCourseCategory(programme);
            var query = $@"Select Distinct Semester AS [Value], Semester AS [Text] FROM ARGExamFormDownloadable WHERE CourseCategory='{courseCategory}' AND AllowDownloadForm=1 AND IsBacklogSetting = 0
                           AND DATEADD(day,(GracePeriodDays1+GracePeriodDays2),EndDate)>=SYSDATETIME() AND (StartDate<>SYSDATETIME() AND StartDate <= SYSDATETIME()) ORDER BY SEMESTER DESC ";
            List<SelectListItem> ddlList = new MSSQLFactory().GetObjectList<SelectListItem>(query);

            if (AppUserHelper.CollegeCode.ToUpper().Equals("IASE") && programme == PrintProgramme.UG)
            {
                if (ddlList.IsNullOrEmpty())
                    ddlList = new List<SelectListItem>();
                query = $@"Select Distinct Semester AS [Value], Semester AS [Text] FROM ARGExamFormDownloadable WHERE CourseCategory='{ExaminationCourseCategory.UG}' AND AllowDownloadForm=1 AND IsBacklogSetting = 0
                           AND DATEADD(day,(GracePeriodDays1+GracePeriodDays2),EndDate)>=SYSDATETIME() AND (StartDate<>SYSDATETIME() AND StartDate <= SYSDATETIME()) ORDER BY SEMESTER DESC ";
                ddlList.AddRange(new MSSQLFactory().GetObjectList<SelectListItem>(query) ?? new List<SelectListItem>());
            }
            return ddlList.IsNullOrEmpty() ? null : ddlList;
        }

        public List<SelectListItem> BacklogExamForSemester(PrintProgramme programme)
        {
            ExaminationCourseCategory courseCategory = new GeneralFunctions().GetExaminationCourseCategory(programme);
            var query = $@"Select Distinct Semester AS [Value], Semester AS [Text] FROM ARGExamFormDownloadable WHERE CourseCategory='{courseCategory}' AND AllowDownloadForm=1 AND IsBacklogSetting = 1 
                           AND DATEADD(day,(GracePeriodDays1+GracePeriodDays2),EndDate)>=SYSDATETIME() AND (StartDate<>SYSDATETIME() AND StartDate <= SYSDATETIME()) ORDER BY SEMESTER DESC ";

            List<SelectListItem> ddlList = new MSSQLFactory().GetObjectList<SelectListItem>(query);

            if (AppUserHelper.CollegeCode.ToUpper().Equals("IASE") && programme == PrintProgramme.UG)
            {
                if (ddlList.IsNullOrEmpty())
                    ddlList = new List<SelectListItem>();
                query = $@"Select Distinct Semester AS [Value], Semester AS [Text] FROM ARGExamFormDownloadable WHERE CourseCategory='{ExaminationCourseCategory.UG}' AND AllowDownloadForm=1 AND IsBacklogSetting = 1 
                           AND DATEADD(day,(GracePeriodDays1+GracePeriodDays2),EndDate)>=SYSDATETIME() AND (StartDate<>SYSDATETIME() AND StartDate <= SYSDATETIME()) ORDER BY SEMESTER DESC ";

                ddlList.AddRange(new MSSQLFactory().GetObjectList<SelectListItem>(query) ?? new List<SelectListItem>());
            }
            return ddlList.IsNullOrEmpty() ? null : ddlList;
        }

        public ARGStudentExamFormSubjects GetStudentExamForm(Guid student_ID, ARGExamFormDownloadable frm, PrintProgramme printProgramme, bool isRegular)
        {
            return new MSSQLFactory().GetObject<ARGStudentExamFormSubjects>(new ExaminationFormSQLQueries().GetStudentExamForm(student_ID, frm, printProgramme, isRegular));
        }
        public short GetStudentCurrentBatch(Guid student_ID, int Semester, PrintProgramme printProgramme)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            return new MSSQLFactory().ExecuteScalar<short>($@"SELECT ISNULL(ISNULL(MAX(sc.SemesterBatch),MAX(p.Batch)),0) RegularBatch FROM dbo.ARGSelectedCombination_{printProgramme.ToString()} sc
                                                            JOIN dbo.ARGPersonalInformation_{printProgramme.ToString()} p ON p.Student_ID = sc.Student_ID
                                                            WHERE sc.Semester<={Semester} and sc.Student_ID='{student_ID}'");
        }


        public ARGStudentExamFormSubjects GetStudentExamForm(Guid Student_ID, short Semester, PrintProgramme printProgramme)
        {
            printProgramme = printProgramme == PrintProgramme.BED ? PrintProgramme.UG : printProgramme;
            SqlCommand command = new SqlCommand
            {
                CommandText = $@"SELECT * FROM ARGStudentExamForm_{printProgramme.ToString()} WHERE Student_ID = @Student_ID AND Semester=@Semester AND IsRegular=1"
            };
            command.Parameters.AddWithValue("@Student_ID", Student_ID);
            command.Parameters.AddWithValue("@Semester", Semester);
            return new MSSQLFactory().GetObject<ARGStudentExamFormSubjects>(command);
        }

        public bool ExamFormSubmitted(Guid Student_ID, short Semester, short year, PrintProgramme printProgramme)
        {
            SqlCommand command = new SqlCommand();
            command.CommandText = $@"SELECT Count(1) FROM ARGStudentExamForm_{printProgramme.ToString()}
                                     WHERE Status={(short)FormStatus.Accepted} AND Student_ID = @Student_ID AND Semester=@Semester AND Year=@Year AND IsRegular=1";
            command.Parameters.AddWithValue("@Student_ID", Student_ID);
            command.Parameters.AddWithValue("@Semester", Semester);
            command.Parameters.AddWithValue("@Year", year);
            return new MSSQLFactory().ExecuteScalar<int>(command) > 0;
        }
        public FeeStructure GetFeeStructure(Guid feeStructure_ID)
        {
            return new MSSQLFactory().GetObject<FeeStructure>(new ExaminationFormSQLQueries().GetFeeStructure(feeStructure_ID));
        }

        public ARGStudentExamFormSubjects GetStudentExamForm(Guid examForm_ID, PrintProgramme printProgramme)
        {
            return new MSSQLFactory().GetObject<ARGStudentExamFormSubjects>(new ExaminationFormSQLQueries().GetStudentExamForm(examForm_ID, printProgramme));
        }

        public bool ExaminationAllowedForStudentCourse(Guid user_ID, PrintProgramme printProgramme, short semester)
        {
            return new MSSQLFactory().ExecuteScalar<bool>(new ExaminationFormSQLQueries().ExaminationAllowedForStudentCourse(user_ID, printProgramme, semester));
        }

        public List<StudentExamFormList> GetExaminationFormList(short semester, PrintProgramme printProgramme, bool getUnpaidStudents)
        {
            return new MSSQLFactory().GetObjectList<StudentExamFormList>(new ExaminationFormSQLQueries().GetExaminationFormList(semester, printProgramme, getUnpaidStudents));
        }


        public ARGStudentExamFormSubjects GetStudentExamFormByStudent_ID(Guid student_ID, PrintProgramme printProgramme)
        {
            return new MSSQLFactory().GetObject<ARGStudentExamFormSubjects>(new ExaminationFormSQLQueries().GetStudentExamForm(student_ID, printProgramme));
        }

        public int UpdateExamFormDownloadable(Guid ARGExamForm_ID, int FormNumberCount)
        {
            string query = @"UPDATE ARGExamFormDownloadable Set FormNumberCount=@FormNumberCount Where ARGExamForm_ID = @ARGExamForm_ID";
            SqlCommand sqlCommand = new SqlCommand(query);
            sqlCommand.Parameters.AddWithValue("@ARGExamForm_ID", ARGExamForm_ID);
            sqlCommand.Parameters.AddWithValue("@FormNumberCount", FormNumberCount);
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }

        public int InsertStudentExamForm(ARGStudentExamForm studentexamForm, PrintProgramme printProgramme)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            return new MSSQLFactory().InsertRecord(studentexamForm, $"ARGStudentExamForm_{printProgramme.ToString()}");
        }

        public List<StudentExamFormList> GetExaminationForms(Parameters Parameter, PrintProgramme printProgramme)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            var Query = new ExaminationFormSQLQueries().GetExaminationFormsCSVListQuery(printProgramme);

            FilterHelper Helper = new GeneralFunctions().GetWhereClause<StudentExamFormList>(Parameter.Filters);

            Query += Helper.WhereClause;
            Query += new GeneralFunctions().GetPagedQuery(Query, Parameter);

            Helper.Command.CommandText = Query;

            return new MSSQLFactory().GetObjectList<StudentExamFormList>(Helper.Command);
        }


        public List<StudentExamFormList> GetExaminationFormsList(Parameters Parameter, PrintProgramme printProgramme, string Columns)
        {
            var Query = new ExaminationFormSQLQueries().GetExaminationFormsListQuery(printProgramme, Columns);

            FilterHelper Helper = new GeneralFunctions().GetWhereClause<StudentExamFormList>(Parameter.Filters);

            Query += Helper.WhereClause;

            if (Parameter.SortInfo.ColumnName.IsNotNullOrEmpty())
            {
                var DbColumnAttribute = typeof(StudentExamFormList).GetProperties().ToList().FirstOrDefault(i => i.Name == Parameter.SortInfo?.ColumnName);

                if (DbColumnAttribute != null)
                    DbColumnAttribute.GetCustomAttributes(typeof(DBColumnNameAttribute), false).Cast<DBColumnNameAttribute>().FirstOrDefault();

                Parameter.SortInfo.ColumnName = DbColumnAttribute == null ? Parameter.SortInfo.ColumnName : DbColumnAttribute.Name;
            }
            Query += new GeneralFunctions().GetPagedQuery(Query, Parameter);

            Helper.Command.CommandText = Query;

            return new MSSQLFactory().GetObjectList<StudentExamFormList>(Helper.Command);
        }


        public int UpdateFeeStatusForBackLogSubjects(Guid studentExamForm_ID, List<Guid> Subject_IDs, bool isBillDeskPayment)
        {
            FormStatus status = isBillDeskPayment ? FormStatus.FeePaid : FormStatus.UpdatedManually;

            var inClause = Subject_IDs.ToIN();

            return new MSSQLFactory().ExecuteNonQuery($"UPDATE ARGStudentReExamForm SET FeeStatus = {(short)status} WHERE StudentExamForm_ID = '{studentExamForm_ID}' AND Subject_ID IN ({inClause})");
        }

        public int SaveExaminationSubjects(ARGStudentReExamForm reAppearSubject) => new MSSQLFactory().InsertRecord(reAppearSubject);

        public int UpdateExaminationFormStatus(decimal Amount, int LateFeeAmount, Guid StudentExamForm_ID, PrintProgramme printProgramme, bool paymentSuccessful)
        {
            int NoOfRecords = 0;
            if (paymentSuccessful)
            {
                string tableName = new GeneralFunctions().GetTableName(printProgramme, Module.ExamForm);
                NoOfRecords = new MSSQLFactory().ExecuteNonQuery(new ExaminationFormSQLQueries().UpdateExaminationFormStatus(Amount, LateFeeAmount, tableName, StudentExamForm_ID, paymentSuccessful));
            }
            return NoOfRecords;
        }

        public int CloseExaminationForm()
        {
            return new MSSQLFactory().ExecuteNonQuery("UPDATE dbo.ARGExamFormDownloadable SET AllowDownloadForm=0 WHERE DATEADD(DAY,(GracePeriodDays1+GracePeriodDays2),EndDate)<SYSDATETIME() AND AllowDownloadForm=1");
        }

        public List<StudentExamFormList> GetExaminationFormList(Parameters Parameter, PrintProgramme printProgramme)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);

            var Query = new ExaminationFormSQLQueries().GetExaminationFormsCSVListQuery(printProgramme);

            FilterHelper Helper = new GeneralFunctions().GetWhereClause<StudentExamFormList>(Parameter.Filters);

            Query += Helper.WhereClause;
            Query += new GeneralFunctions().GetPagedQuery(Query, Parameter);
            Helper.Command.CommandText = Query;

            return new MSSQLFactory().GetObjectList<StudentExamFormList>(Helper.Command);
        }

        public int DeleteOldRecords(Guid studentExamForm_ID)
        {
            return new MSSQLFactory().ExecuteNonQuery($"DELETE FROM ARGStudentReExamForm WHERE StudentExamForm_ID = '{studentExamForm_ID}'");
        }

        public int UpdateExaminationFormAmount(Guid entity_ID, PrintProgramme printProgramme)
        {
            SqlCommand command = new SqlCommand()
            {
                CommandText = $@"UPDATE ARGSTUDENTEXAMFORM_{printProgramme} SET AMOUNTPAID = (SELECT SUM(TxnAmount) FROM PAYMENTDETAILS_{printProgramme} WHERE ENTITY_ID=@StudentExamForm_ID)"
            };
            command.Parameters.AddWithValue("@StudentExamForm_ID", entity_ID);

            return new MSSQLFactory().ExecuteNonQuery(command);
        }

        public ARGStudentExamFormSubjects GetExaminationFormForPrint(Guid student_ID, int semester)
        {
            var query = $@"SELECT Top 1 StudentExamForm_ID, Student_ID, Semester, FormNumber, Year, ExamRollNumber, AmountPaid, Status, CreatedOn,IsDivisionImprovement,
                            UpdatedOn, CreatedBy, UpdatedBy, IsRegular, StudentCode, Notification_ID, LateFeeAmount FROM ARGStudentExamForm_{AppUserHelper.TableSuffix} WHERE Student_ID ='{student_ID}' AND Semester = {semester} AND IsRegular=1 ORDER BY CreatedOn DESC";

            return new MSSQLFactory().GetObject<ARGStudentExamFormSubjects>(query);
        }

        public bool IsSpecialledAbledStudent(Guid student_ID)
        {
            return new MSSQLFactory().ExecuteScalar<int>($"SELECT COUNT(Student_ID) FROM SpeciallyAbledStudents WHERE Student_ID = '{student_ID}'") > 0;
        }

        public ARGStudentExamFormSubjects GetBacklogExaminationFormForPrint(Guid student_ID, int semester)
        {
            var programme = new GeneralFunctions().MappingTable(AppUserHelper.OrgPrintProgramme);

            var query = $@"SELECT TOP 1 StudentExamForm_ID, Student_ID, Semester, FormNumber, Year, ExamRollNumber, AmountPaid, Status, CreatedOn,IsDivisionImprovement,
                            UpdatedOn, CreatedBy, UpdatedBy, IsRegular, StudentCode, Notification_ID, LateFeeAmount, IsDivisionImprovement FROM ARGStudentExamForm_{programme} WHERE Student_ID ='{student_ID}' AND Semester = {semester} AND IsRegular=0 ORDER BY CreatedOn DESC";

            return new MSSQLFactory().GetObject<ARGStudentExamFormSubjects>(query);
        }


        public ExaminationWhiteListCompact CheckIfStudentIsWhiteListed(Guid student_ID, short semester, short year)
        {
            return new MSSQLFactory().GetObject<ExaminationWhiteListCompact>($@"SELECT * FROM ExaminationWhiteList WHERE Student_ID = '{student_ID}' AND Semester = {semester} AND ExaminationYear = {year}");
        }

        public List<ADMSubjectMaster> GetAppliedReAppearSubjectsNewTables(Guid studentExamForm_ID)
        {
            return new MSSQLFactory().GetObjectList<ADMSubjectMaster>
                                                ($@"SELECT ADMSubjectMaster.HasExaminationFee,ADMSubjectMaster.SubjectFullName+'-(Reappear) ' AS SubjectFullName,ADMSubjectMaster.Subject_ID,ADMSubjectMaster.HasResult,ADMSubjectMaster.SubjectType FROM ARGStudentReExamForm 
                                                JOIN ADMSubjectMaster ON ADMSubjectMaster.Subject_ID = ARGStudentReExamForm.Subject_ID
                                                WHERE (ARGStudentReExamForm.FeeStatus={(short)FormStatus.FeePaid} OR ARGStudentReExamForm.FeeStatus={(short)FormStatus.UpdatedManually}  ) AND StudentExamForm_ID = '{studentExamForm_ID}'");
        }


        public List<ARGStudentReExamForm> GetSubjectsAppliedForBacklogFormNewTables(Guid studentExamForm_ID)
        {
            return new MSSQLFactory().GetObjectList<ARGStudentReExamForm>
                                                ($@"SELECT StudentReExamForm_ID, StudentExamForm_ID, Programme, ARGStudentReExamForm.Subject_ID, [Type] , 
                                                                                ADMSubjectMaster.Subject_ID,SubjectFullName  FROM ARGStudentReExamForm 
                                                JOIN ADMSubjectMaster ON ADMSubjectMaster.Subject_ID = ARGStudentReExamForm.Subject_ID
                                                WHERE StudentExamForm_ID = '{studentExamForm_ID}'");
        }


        public ExaminationFormSummary GetExaminationFormSummary(Parameters Parameter, PrintProgramme printProgramme)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            FilterHelper Helper = new FilterHelper();

            Helper = new GeneralFunctions().GetWhereClause<StudentExamForm>(Parameter.Filters);

            var Query = $@"WITH ExamFormSummary (PrintProgramme,ExamFormStatus, Total)  
                        AS  
                        (  
                        SELECT 1 AS [PrintProgramme],ISNULL(EXMFRM.[Status],0) ExamFormStatus,COUNT(*) Total
                        FROM dbo.ARGPersonalInformation_{printProgramme.ToString()} STDINFO 
                        JOIN ARGSelectedCombination_{printProgramme.ToString()} Comb ON Comb.Student_ID = STDINFO.Student_ID
                        INNER JOIN ADMCombinationMaster ON ADMCombinationMaster.Combination_ID = COMB.Combination_ID  AND ADMCombinationMaster.Semester = COMB.Semester
                        LEFT JOIN ARGStudentExamForm_{printProgramme.ToString()} EXMFRM ON EXMFRM.Student_ID = STDINFO.Student_ID AND EXMFRM.Status={(short)FormStatus.Accepted} AND EXMFRM.Semester=Comb.Semester							
                        {Helper.WhereClause}
                        GROUP BY EXMFRM.[Status]
                        )  
                        SELECT   PrintProgramme,[0] AS NotDownloaded, [4] AS Accepted
                        FROM   ExamFormSummary 
                        PIVOT
                        (
                            SUM(Total)
                            FOR ExamFormStatus IN ([0], [1], [4])
                        ) AS ExamFormSummaryPivot";
            Helper.Command.CommandText = Query;
            var summary = new MSSQLFactory().GetObject<ExaminationFormSummary>(Helper.Command);
            if (summary != null)
                summary.TotalStudent = summary.InProcess + summary.NotDownloaded + summary.Accepted;
            return summary;
        }

        public ARGStudentReExamForm GetARGReExamForm(Guid studentExamForm_ID)
        {
            return new MSSQLFactory().GetObject<ARGStudentReExamForm>($@"SELECT StudentReExamForm_ID, StudentExamForm_ID, Programme, Subject_ID, [Type], CreatedOn, UpdatedOn, CreatedBy,
                                                                                UpdatedBy, FeeStatus FROM ARGStudentReExamForm WHERE StudentExamForm_ID = '{studentExamForm_ID}'");
        }

        public List<ARGStudentReExamForm> GetExaminationFormSubjects(Guid studentExamForm_ID)
        {
            return new MSSQLFactory().GetObjectList<ARGStudentReExamForm>(new ExaminationFormSQLQueries().GetExaminationFormSubjects(studentExamForm_ID));
        }

        public List<ARGStudentReExamForm> GetAppliedBacklogSubjectsForStudent(Guid studentExamForm_ID, PrintProgramme printProgramme, short semster)
        {
            return new MSSQLFactory().GetObjectList<ARGStudentReExamForm>(new ExaminationFormSQLQueries().GetAppliedBacklogSubjectsForStudent(studentExamForm_ID, printProgramme, semster));
        }

        public ARGStudentExamForm GetStudentExamForm(ARGStudentExamForm studentExamForm, PrintProgramme printProgramme)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            return new MSSQLFactory().GetObject<ARGStudentExamForm>(new ExaminationFormSQLQueries().GetStudentExamForm(studentExamForm, printProgramme));
        }

        public int UpdateStudentExamForm(ARGStudentExamForm studentExamForm, PrintProgramme printProgramme)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            string query = $@"UPDATE ARGStudentExamForm_{printProgramme.ToString()} Set ExamRollNumber=@ExamRollNumber,Status=@Status Where Student_ID = @Student_ID AND @FormNumber=FormNumber AND Semester=@Semester AND Year=@Year";
            SqlCommand sqlCommand = new SqlCommand(query);
            sqlCommand.Parameters.AddWithValue("@ExamRollNumber", string.IsNullOrEmpty(studentExamForm.ExamRollNumber) ? (object)DBNull.Value : studentExamForm.ExamRollNumber);
            sqlCommand.Parameters.AddWithValue("@Status", (short)studentExamForm.Status);
            sqlCommand.Parameters.AddWithValue("@Student_ID", studentExamForm.Student_ID);
            sqlCommand.Parameters.AddWithValue("@FormNumber", studentExamForm.FormNumber);
            sqlCommand.Parameters.AddWithValue("@Semester", studentExamForm.Semester);
            sqlCommand.Parameters.AddWithValue("@Year", studentExamForm.Year);
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }

        #region Semester AdmitCard Related Routines
        public List<SelectListItem> GetSemesterDDL(PrintProgramme programme, Guid Student_ID)
        {
            PrintProgramme programmeTable = new GeneralFunctions().MappingTable(programme);
            return new MSSQLFactory().GetObjectList<SelectListItem>($@"
                    SELECT DISTINCT CAST(Semester AS NVARCHAR(2)) AS [Value],'Semester-'+CAST((Semester) AS NVARCHAR(2)) AS [Text] FROM ARGExamFormDownloadable
                    WHERE AllowDownloadAdmitCards=1 AND PrintProgramme={(int)programme} 
                    AND Semester<=(SELECT CurrentSemesterOrYear FROM dbo.ARGPersonalInformation_{programmeTable.ToString()} WHERE Student_ID='{Student_ID}')");
        }
        public List<short> GetAdmitPrintProgrammeDDL()
        {
            return new MSSQLFactory().GetSingleValues<short>($@"
                                   SELECT DISTINCT PrintProgramme FROM ARGExamFormDownloadable
                                    WHERE AllowDownloadAdmitCards=1     
                                    ");
        }

        public List<short> GetAdmitSemesterDDL()
        {
            return new MSSQLFactory().GetSingleValues<short>(@"
                                   SELECT DISTINCT Semester FROM ARGExamFormDownloadable
                                    WHERE AllowDownloadAdmitCards=1           
                                    ");
        }

        public Guid GetStudentID()
        {
            PrintProgramme tablePrefix = AppUserHelper.TableSuffix;
            string query = $@"SELECT DISTINCT p.Student_ID FROM ARGPersonalInformation_{tablePrefix.ToString()} p
                            JOIN ARGStudentExamForm_{tablePrefix.ToString()} e ON e.Student_ID = p.Student_ID
                            WHERE p.Student_ID=@Student_ID AND CurrentSemesterOrYear>0 AND (e.Status={(short)FormStatus.Accepted} OR e.Status={(short)FormStatus.BlockSemesterAdmitCard})";
            SqlCommand command = new SqlCommand();
            command.Parameters.AddWithValue("@Student_ID", AppUserHelper.User_ID);
            command.CommandText = query;
            return new MSSQLFactory().ExecuteScalar<Guid>(command);
        }
        public ADMCombinationMaster GetStudentSemensterSubjectsAndCourse(Guid Student_ID, short Semester, PrintProgramme printProgramme)
        {
            string query = $@"SELECT c.Course_ID,c.CombinationSubjects,c.CombinationCode FROM ARGSelectedCombination_{printProgramme.ToString()} s
                            JOIN ADMCombinationMaster c ON c.Combination_ID = s.Combination_ID
                            WHERE Student_ID=@Student_ID AND s.Semester=@Semester";
            SqlCommand command = new SqlCommand();
            command.Parameters.AddWithValue("@Student_ID", Student_ID);
            command.Parameters.AddWithValue("@Semester", Semester);
            command.CommandText = query;
            return new MSSQLFactory().GetObject<ADMCombinationMaster>(command);
        }
        public ARGStudentExamForm GetExamFormDetails(Guid Student_ID, short Semester, PrintProgramme printProgramme)
        {
            string query = $@"SELECT TOP 1 * FROM ARGStudentExamForm_{printProgramme.ToString()} WHERE Student_ID=@Student_ID AND Semester=@Semester AND Status IN(@Status1,@Status2) ORDER BY CreatedOn DESC,Year DESC";
            SqlCommand command = new SqlCommand();
            command.Parameters.AddWithValue("@Student_ID", Student_ID);
            command.Parameters.AddWithValue("@Semester", Semester);
            command.Parameters.AddWithValue("@Status1", FormStatus.Accepted);
            command.Parameters.AddWithValue("@Status2", FormStatus.BlockSemesterAdmitCard);
            command.CommandText = query;
            return new MSSQLFactory().GetObject<ARGStudentExamForm>(command);
        }

        public StudentExamForm GetExamFormDetailsWithOutAccepted(Guid Student_ID, PrintProgramme printProgramme)
        {
            string query = $@"SELECT TOP 1 * FROM ARGStudentExamForm_{printProgramme.ToString()} WHERE Student_ID=@Student_ID ORDER BY Semester DESC";
            SqlCommand command = new SqlCommand();
            command.Parameters.AddWithValue("@Student_ID", Student_ID);
            command.CommandText = query;
            return new MSSQLFactory().GetObject<StudentExamForm>(command);
        }

        public SemesterExamCenterDetails GetSemesterCenterAllotmentDetails(Guid StudentExamForm_ID)
        {
            string TSQL = @"SELECT CenterCode,CollegeCode,CollegeFullName,[Address],District,PinCode,ContactNo1,Email,EntranceDate,[Time] FROM ARGCentersAllotmentMaster
                            JOIN ARGEntranceCentersMaster ON ARGEntranceCentersMaster.Center_ID = ARGCentersAllotmentMaster.Center_ID
                            JOIN ADMCollegeMaster ON ADMCollegeMaster.College_ID = ARGEntranceCentersMaster.College_ID
                            JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = ARGCentersAllotmentMaster.Course_ID WHERE [Entity_ID] = @StudentExamForm_ID";
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.AddWithValue("@StudentExamForm_ID", StudentExamForm_ID);
            cmd.CommandText = TSQL;
            return new MSSQLFactory().GetObject<SemesterExamCenterDetails>(cmd);
        }



        public ARGExamFormDownloadable AllowSemesterAdmitCards(PrintProgramme printProgramme, short semester)
        {
            SqlCommand command = new SqlCommand();
            command.CommandText = $@"SELECT TOP 1 * FROM ARGExamFormDownloadable WHERE PrintProgramme=@PrintProgramme AND Semester=@Semester AND AllowDownloadAdmitCards=1 ORDER BY [Year] DESC";
            command.Parameters.AddWithValue("@PrintProgramme", printProgramme);
            command.Parameters.AddWithValue("@Semester", semester);
            return new MSSQLFactory().GetObject<ARGExamFormDownloadable>(command);
        }

        public List<CourseWiseWidget> GetCourseWidgetForProgramme(PrintProgramme programme, int batch, int semester)
        {

            List<CourseWiseWidget> coursewidgets = new MSSQLFactory().GetObjectList<CourseWiseWidget>(new ExaminationFormSQLQueries().GetCourseWidgetsQuery(programme, batch, semester));

            if (coursewidgets != null)
            {
                foreach (CourseWiseWidget courseWiseWidget in coursewidgets ?? new List<CourseWiseWidget>())
                {
                    courseWiseWidget.TotalNumberOfStudentsPerCourse = new MSSQLFactory().ExecuteScalar<int>(new ExaminationFormSQLQueries().GetTotalNumberOfStudentsPerCourse(programme, batch, semester, courseWiseWidget.CourseFullName));
                    courseWiseWidget.NoOfBacklogStudents = new MSSQLFactory().ExecuteScalar<int>(new ExaminationFormSQLQueries().GetNoOfBacklogStudents(programme, batch, semester, courseWiseWidget.CourseFullName));
                    courseWiseWidget.FormApplied = new MSSQLFactory().ExecuteScalar<int>(new ExaminationFormSQLQueries().GetNoOfFormApplied(programme, batch, semester, courseWiseWidget.CourseFullName));
                    courseWiseWidget.LateFeeAmountForBackLogStudents = new MSSQLFactory().ExecuteScalar<int>(new ExaminationFormSQLQueries().GetLateFeeAmountForBackLogStudents(programme, batch, semester, courseWiseWidget.CourseFullName));
                    courseWiseWidget.LateFeeAmountForRegularStudents = new MSSQLFactory().ExecuteScalar<int>(new ExaminationFormSQLQueries().GetLateFeeAmountForRegularStudents(programme, batch, semester, courseWiseWidget.CourseFullName));
                }
            }

            return coursewidgets;
        }

        public int GetLateFee1AmountForWidget(PrintProgramme programme, int year, short semester) => new MSSQLFactory().ExecuteScalar<int>(new ExaminationFormSQLQueries().GetLate1FeeAmountForWidget(programme, year, semester));

        public int GetLateFee2AmountForWidget(PrintProgramme programme, int year, short semester) => new MSSQLFactory().ExecuteScalar<int>(new ExaminationFormSQLQueries().GetLate2FeeAmountForWidget(programme, year, semester));

        #endregion


        public ARGExamFormDownloadable GetExamFormDownloadable(Guid ARGExamForm_ID)
        {
            return new MSSQLFactory().GetObject<ARGExamFormDownloadable>($"SELECT * FROM ARGExamFormDownloadable WHERE ARGExamForm_ID='{ARGExamForm_ID}'");
        }

        public string GetRegistrationNoOrFormNumber(PrintProgramme printProgramme, string FormNumber)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            string query = $@"SELECT COALESCE(S.CUSRegistrationNo,s.StudentFormNo)
                                FROM ARGPersonalInformation_{printProgramme.ToString()} S ON S.Student_ID = E.Student_ID
                                WHERE FormNumber=@FormNumber";
            SqlCommand sqlCommand = new SqlCommand(query);
            sqlCommand.Parameters.AddWithValue("@FormNumber", FormNumber);
            return new MSSQLFactory().ExecuteScalar<string>(sqlCommand);
        }
        public ARGStudentExamForm GetRegularExamFormOnlyAccepted(Guid student_ID, int semester, PrintProgramme printProgramme)
        {
            var query = $@"SELECT TOP 1 * FROM dbo.ARGStudentExamForm_UG WHERE Student_ID='{student_ID}' AND Semester={semester} AND IsRegular=1 AND Status={(short)FormStatus.Accepted} AND Year={DateTime.Now.Year} ORDER BY CreatedOn DESC";
            return new MSSQLFactory().GetObject<ARGStudentExamForm>(query);
        }

        public bool Get(PrintProgramme printProgramme, Guid student_ID, short semester, short submittedyear)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            var query = $@"SELECT COUNT(Student_ID) FROM dbo.ARGStudentExamForm_{printProgramme} WHERE Student_ID='{student_ID}' 
                AND Semester={semester} AND Status={(short)FormStatus.Accepted}";
            return new MSSQLFactory().ExecuteScalar<int>(query) > 0;
        }

        public string GetStudentNameOnly(PrintProgramme printProgramme, Guid ExamForm_ID)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            var query = $@"SELECT FullName FROM dbo.ARGPersonalInformation_{printProgramme.ToString()} WHERE Student_ID
                            =(SELECT TOP 1 Student_ID FROM dbo.ARGStudentExamForm_{printProgramme.ToString()} WHERE StudentExamForm_ID='{ExamForm_ID}')";
            return new MSSQLFactory().ExecuteScalar<string>(query);
        }

        public IEnumerable<SelectListItem> GetExamFeeStructure()
        {
            return new MSSQLFactory().GetObjectList<SelectListItem>($@"SELECT CAST(FeeStructure_ID AS VARCHAR(100)) AS Value,
                                               ' Per Subject: ' + CAST(FeePerSubject AS VARCHAR(10)) + '       Hot & Cold Charges: '
                                               + CAST((MinimumFee + OtherCharges + ExaminationFund + ITComponent) AS VARCHAR(10)) AS Text
                                        FROM dbo.FeeStructure
                                        WHERE Status = 1  ORDER BY ExaminationFund DESC;");
        }
    }
}
