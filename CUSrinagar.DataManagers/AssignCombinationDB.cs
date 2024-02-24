using CUSrinagar.DataManagers.SQLQueries;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using CUSrinagar.Models;
using GeneralModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Terex;

namespace CUSrinagar.DataManagers
{
    public class AssignCombinationDB
    {
        public List<CourseWiseList> GetStudentsList(Parameters parameters, PrintProgramme printProgramme)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);

            string Query = $@" SELECT DISTINCT AcceptCollege_ID,STDINFO.CUSRegistrationNo ,STDINFO.StudentFormNo ,BoardRegistrationNo,STDINFO.Student_ID,STDINFO.FullName,STDINFO.EntranceRollNo
                             ,STDINFO.FathersName, STDINFO.Gender,STDINFO.Religion,
                             Mobile,Addres.Email,Category,STDINFO.ClassRollNo,STDINFO.FormStatus
                             ,CombinationCode,COMB.Semester,Comb.IsVerified
                             ,CourseFullName,Programme
                             ,PermanentAddress,District,PinCode,DOB,VWSCMasterCompact.SubjectType
                             ,(Select Top 1  ExmFrm.ExamRollNumber  from  dbo.ARGStudentExamForm_{printProgramme.ToString()} ExmFrm where ExmFrm.Student_ID = STDINFO.Student_ID and ExmFrm.semester=COMB.Semester AND ExmFrm.ExamRollNumber IS NOT NULL ) ExamRollNo
                             ,P.TxnAmount,P.TxnDate,P.ModuleType,P.PaymentType
                             FROM ARGPersonalInformation_{printProgramme.ToString()} STDINFO  
                             INNER JOIN ARGStudentAddress_{printProgramme.ToString()} Addres ON Addres.Student_ID = STDINFO.Student_ID
                             INNER JOIN dbo.ARGSelectedCombination_{printProgramme.ToString()} COMB ON COMB.Student_ID = STDINFO.Student_ID AND COMB.Semester<=STDINFO.CurrentSemesterOrYear
                             INNER JOIN	VWSCMasterCompact ON VWSCMasterCompact.Semester = COMB.Semester AND VWSCMasterCompact.Combination_ID = COMB.Combination_ID
                             LEFT JOIN PaymentDetails_{printProgramme.ToString()} P ON P.Semester = COMB.Semester AND P.Entity_ID=STDINFO.Student_ID AND ModuleType = 6";
            var helper = new GeneralFunctions().GetWhereClause<CourseWiseList>(parameters.Filters);
            if ((parameters.SortInfo.ColumnName + "").ToLower().Contains("classrollno"))
            {
                if (parameters.PageInfo.PageSize == -1)
                {
                    Query = "SELECT * FROM (" + Query + (helper.WhereClause + @") s ORDER BY
                                                                            LEFT(s.ClassRollNo, PATINDEX('%[0-9]%', s.ClassRollNo) - 1),
                                                                    CONVERT(INT, SUBSTRING(s.ClassRollNo, PATINDEX('%[0-9]%', s.ClassRollNo), LEN(s.ClassRollNo)))");
                }
                else
                {

                    Query = "SELECT * FROM (" + Query + (helper.WhereClause + new GeneralFunctions().GetPagedQuery(Query, parameters).Replace(";", "") + @") s ORDER BY
                                                                                                                                                                                                                                                                        LEFT(s.ClassRollNo, PATINDEX('%[0-9]%', s.ClassRollNo) - 1),
                                                                                                                                                                                                                                                                      CONVERT(INT, SUBSTRING(s.ClassRollNo, PATINDEX('%[0-9]%', s.ClassRollNo), LEN(s.ClassRollNo)))");
                }
            }
            else
            {
                Query += helper.WhereClause + new GeneralFunctions().GetPagedQuery(Query, parameters);
            }
            helper.Command.CommandText = Query;
            helper.Command.CommandTimeout = 120;
            return new MSSQLFactory().GetObjectList<CourseWiseList>(helper.Command);
        }


        public DataTable GetStudentsListAsDataTable(Parameters parameters)
        {
            return new MSSQLFactory().GetDataTable(AssignCombinationSQLQueries.GetStudentsList(parameters));
        }
        public List<CombinationsCount> GetCombinationsCount(Parameters parameters)
        {
            return new MSSQLFactory().GetObjectList<CombinationsCount>(AssignCombinationSQLQueries.GetCombinationsCount(parameters));
        }
        public List<SubjectsCount> GetSubjectsCount(Parameters parameters)
        {
            return new MSSQLFactory().GetObjectList<SubjectsCount>(AssignCombinationSQLQueries.GetSubjectsCount(parameters));
        }
        public DataTable GetSubjectsCountE(Parameters parameters)
        {
            return new MSSQLFactory().GetDataTable(AssignCombinationSQLQueries.GetSubjectsCount(parameters));
        }
        public ARGPersonalInformation GetStudentDetails(string formNumber, PrintProgramme programme)
        {
            return new MSSQLFactory().GetObject<ARGPersonalInformation>(AssignCombinationSQLQueries.GetStudentDetails(formNumber, programme));
        }

        public int Save(ARGSelectedCombination SelectedCombination, PrintProgramme printProgramme)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            return new MSSQLFactory().InsertRecord(SelectedCombination, $"ARGSelectedCombination_{printProgramme.ToString()}");
        }

        public int Update(ARGSelectedCombination studentCombination, PrintProgramme printProgramme)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            List<string> ignoreList = new List<string>() { "SelectedCombination_ID", "CreatedOn", "CreatedBy" };
            var sqlCommand = new MSSQLFactory().UpdateRecord(studentCombination, ignoreList, ignoreList, $"ARGSelectedCombination_{printProgramme.ToString()}");
            sqlCommand.CommandText += " WHERE SelectedCombination_ID =@SelectedCombination_ID";
            sqlCommand.Parameters.AddWithValue("@SelectedCombination_ID", studentCombination.SelectedCombination_ID);
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }

        public int GetCombinationCount(Guid Combination_ID, short Semester, short Batch, PrintProgramme printProgramme)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            var Query = $@"SELECT COUNT(STDCOMB.Student_ID) 
                                FROM ARGSelectedCombination_{printProgramme.ToString()} STDCOMB 
                                JOIN ARGPersonalInformation_{printProgramme.ToString()} STD ON	STDCOMB.Student_ID = STD.Student_ID
                                WHERE STDCOMB.Combination_ID='{Combination_ID}' AND STD.Batch={Batch} AND STDCOMB.Semester={Semester}";
            return new MSSQLFactory().ExecuteScalar<int>(Query);
        }

        //public int GetActualCombinationCount(Guid subjectCombination_ID, Guid College_ID)
        //{
        //    return new MSSQLFactory().ExecuteScalar<int>(AssignCombinationSQLQueries.GetActualCombinationCount(subjectCombination_ID, College_ID));
        //}
        public int DeleteStudentSemester1Award(Guid Student_ID, PrintProgramme printProgramme)
        {
            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.CommandText = $"DELETE FROM {printProgramme.ToString()}_Semester1 WHERE Student_ID='{Student_ID}'";
            sqlCommand.Parameters.AddWithValue("@Student_ID", Student_ID);
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }

        public int DeleteStudentCombinations(Guid Student_ID, short semester, PrintProgramme printProgramme)
        {
            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.CommandText = $"DELETE FROM ARGSelectedCombination_{printProgramme.ToString()} WHERE Student_ID =@Student_ID AND Semester={semester}";
            sqlCommand.Parameters.AddWithValue("@Student_ID", Student_ID);
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);

            //string tableName = "ARGSelectedCombination";

            //switch (programme)
            //{
            //    case Programme.UG:
            //        tableName += "_UG";
            //        break;
            //    case Programme.PG:
            //        tableName += "_PG";
            //        break;
            //    case Programme.IG:
            //        tableName += "_IH";
            //        break;
            //    case Programme.HS:
            //        tableName += "_UG";
            //        break;
            //    default:
            //        break;
            //}
            //return new MSSQLFactory().ExecuteNonQuery(AssignCombinationSQLQueries.Delete(student_ID, college_ID, tableName));
        }

        public ADMCourseMaster GetCourseName(Guid courseID)
        {

            return new MSSQLFactory().GetObject<ADMCourseMaster>(AssignCombinationSQLQueries.GetCourseName(courseID));
        }

        public CombinationsCount GetCombination(Guid combinationId)
        {
            return new MSSQLFactory().GetObject<CombinationsCount>(AssignCombinationSQLQueries.GetCombination(combinationId));
        }

        public int GetSubjectCount(PrintProgramme printProgramme, short semester, Guid subject_ID, SubjectType subjectType, short? Batch = 0, Guid? College_ID = null)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            var Query = $@"SELECT COUNT(1) FROM ARGPersonalInformation_{printProgramme} S
                        JOIN ARGSelectedCombination_{printProgramme} C ON C.Student_ID = S.Student_ID 
                        JOIN ADMCombinationMaster CMaster ON CMaster.Combination_ID = C.Combination_ID AND CMaster.Semester = C.Semester
                        WHERE s.Batch={Batch} AND s.AcceptCollege_ID='{College_ID}' AND c.Semester={semester} AND CMaster.CombinationSubjects LIKE '%{subject_ID}%' ";

            //var Query1 = $@"SELECT COUNT(DISTINCT SelectedCombination_ID) FROM(
            //            SELECT SelectedCombination_ID,Combination_ID,Student_ID FROM ARGSelectedCombination_UG WHERE Semester={semester}
            //            UNION ALL
            //            SELECT SelectedCombination_ID,Combination_ID,Student_ID FROM ARGSelectedCombination_IH WHERE Semester={semester}
            //            )AllSelectedCom
            //            JOIN ARGPersonalInformation_IH STDINFO ON STDINFO.Student_ID = AllSelectedCom.Student_ID
            //            JOIN ADMCombinationMaster ON ADMCombinationMaster.Combination_ID = AllSelectedCom.Combination_ID AND ADMCombinationMaster.Semester={semester}
            //            JOIN ADMCollegeCourseMapping ON ADMCollegeCourseMapping.Course_ID = ADMCombinationMaster.Course_ID
            //            WHERE STDINFO.Batch={Batch} AND ADMCombinationMaster.CombinationSubjects LIKE '%{subject_ID}%' AND ADMCollegeCourseMapping.College_ID='{College_ID}' {(IsCollege?"AND IsVerified=1":"")}";
            return new MSSQLFactory().ExecuteScalar<int>(Query);
        }


        #region Import Ih to UG
        public bool IsAlreadyImported(string CUSFormNo, PrintProgramme programme)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.AddWithValue("@StudentFormNo", CUSFormNo);
            cmd.CommandText = $@"SELECT COUNT(StudentFormNo) FROM dbo.ARGPersonalInformation_{programme.ToString()} WHERE StudentFormNo=@StudentFormNo;";
            return new MSSQLFactory().ExecuteScalar<int>(cmd) > 0;
        }
        public ARGPersonalInformation GetStudentforImport(string CUSFormNo, PrintProgramme programme)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.AddWithValue("@StudentFormNo", CUSFormNo);
            cmd.CommandText = $@"SELECT StudentFormNo,Student_ID,AcceptCollege_ID,FormStatus,CATEntrancePoints,Photograph FROM dbo.ARGPersonalInformation_{programme.ToString()} WHERE StudentFormNo=@StudentFormNo;";
            return new MSSQLFactory().GetObject<ARGPersonalInformation>(cmd);
        }

        public int ImportIHtoUG(Guid Student_ID, Guid NewStudent_ID)
        {
            #region Import Script
            string TSQL = $@"DECLARE @Student_ID UNIQUEIDENTIFIER;
                            SET @Student_ID='{NewStudent_ID}';
                            INSERT INTO dbo.ARGPersonalInformation_UG
                            (
	                               Student_ID,
                                   StudentFormNo,
                                   BoardRegistrationNo,
                                   CUSRegistrationNo,
                                   PreviousUniversityRegnNo,
                                   FullName,
                                   FathersName,
                                   MothersName,
                                   Gender,
                                   DOB,
                                   Religion,
                                   Category,
                                   AcceptCollege_ID,
                                   AcceptedBy_ID,
                                   IsPassout,
                                   Photograph,
                                   IsProvisional,
                                   FormStatus,
                                   CreatedOn,
                                   CreatedBy,
                                   UpdatedOn,
                                   UpdatedBy,
                                   TotalFee,
                                   EntranceRollNo,
                                   CurrentSemesterOrYear,
                                   Batch,
                                   ClassRollNo,
                                   CATEntrancePoints
                            )
                            SELECT @Student_ID,
                                   StudentFormNo,
                                   BoardRegistrationNo,
                                   CUSRegistrationNo,
                                   PreviousUniversityRegnNo,
                                   FullName,
                                   FathersName,
                                   MothersName,
                                   Gender,
                                   DOB,
                                   Religion,
                                   Category,
                                   AcceptCollege_ID,
                                   AcceptedBy_ID,
                                   IsPassout,
                                   REPLACE(Photograph,'/IH/','/UG/'),
                                   IsProvisional,
                                   FormStatus,
                                   CreatedOn,
                                   CreatedBy,
                                   UpdatedOn,
                                   UpdatedBy,
                                   TotalFee,
                                   EntranceRollNo,
                                   CurrentSemesterOrYear,
                                   Batch,
                                   ClassRollNo,
                                   CATEntrancePoints FROM dbo.ARGPersonalInformation_IH WHERE Student_ID='{Student_ID}';
                            INSERT INTO dbo.ARGStudentAddress_UG
                            (
	                               Address_ID,
                                   Student_ID,
                                   PinCode,
                                   Mobile,
                                   Email,
                                   PermanentAddress,
                                   AssemblyConstituency,
                                   ParliamentaryConstituency,
                                   Block,
                                   Tehsil,
                                   District,
                                   State,
                                   CreatedOn,
                                   CreatedBy,
                                   UpdatedOn,
                                   UpdatedBy
                            )
                            SELECT NEWID(),
                                   @Student_ID,
                                   PinCode,
                                   Mobile,
                                   Email,
                                   PermanentAddress,
                                   AssemblyConstituency,
                                   ParliamentaryConstituency,
                                   Block,
                                   Tehsil,
                                   District,
                                   State,
                                   CreatedOn,
                                   CreatedBy,
                                   UpdatedOn,
                                   UpdatedBy FROM dbo.ARGStudentAddress_IH WHERE Student_ID='{Student_ID}';
                            INSERT INTO dbo.ARGStudentPreviousQualifications_UG
                            (
                                Qualification_ID,
                                Student_ID,
                                ExamName,
                                Stream,
                                Subjects,
                                Session,
                                Year,
                                RollNo,
                                MaxMarks,
                                MarksObt,
                                ExamBody,
                                CreatedOn,
                                CreatedBy,
                                UpdatedOn,
                                UpdatedBy
                            )
                            SELECT  NEWID(),
                                @Student_ID,
                                ExamName,
                                Stream,
                                Subjects,
                                Session,
                                Year,
                                RollNo,
                                MaxMarks,
                                MarksObt,
                                ExamBody,
                                CreatedOn,
                                CreatedBy,
                                UpdatedOn,
                                UpdatedBy FROM dbo.ARGStudentPreviousQualifications_IH WHERE Student_ID='{Student_ID}';
                            INSERT INTO dbo.ARGCoursesApplied_UG
                            (
                                Student_ID,
                                Course_ID,
                                Preference,
                                SubjectEntrancePoints,
                                AppearedInEntrance,
                                SelfFinancedPayment_ID,
                                StudentSelectionStatus,
                                SelectionAgaintListNo,
                                IsActive
                            )
                            SELECT TOP 1 @Student_ID,
                                   Course_ID,
                                   Preference,
                                   SubjectEntrancePoints,
                                   AppearedInEntrance,
                                   SelfFinancedPayment_ID,
                                   StudentSelectionStatus,
                                   SelectionAgaintListNo,
                                   IsActive FROM dbo.ARGCoursesApplied_IH WHERE Course_ID='A3EE7F98-7B82-4D95-A2C0-FABA7A18240E' AND Student_ID='{Student_ID}';
                            INSERT INTO dbo.PaymentDetails_UG
                            (
                                Payment_ID,
                                Entity_ID,
                                TxnReferenceNo,
                                BankReferenceNo,
                                TxnAmount,
                                BankID,
                                BankMerchantID,
                                TxnType,
                                CurrencyName,
                                ItemCode,
                                SecurityType,
                                SecurityID,
                                TxnDate,
                                AuthStatus,
                                SettlementType,
                                ErrorStatus,
                                ErrorDescription,
                                ModuleType,
                                PaymentType,
                                Email,
                                PhoneNumber,
                                Semester
                            )
                            SELECT NEWID(),
                                @Student_ID,
                                TxnReferenceNo,
                                BankReferenceNo,
                                TxnAmount,
                                BankID,
                                BankMerchantID,
                                TxnType,
                                CurrencyName,
                                ItemCode,
                                SecurityType,
                                SecurityID,
                                TxnDate,
                                AuthStatus,
                                SettlementType,
                                ErrorStatus,
                                ErrorDescription,
                                ModuleType,
                                PaymentType,
                                Email,
                                PhoneNumber,
                                Semester FROM dbo.PaymentDetails_IH WHERE Entity_ID='{Student_ID}' AND ModuleType IN (1,6);";
            #endregion
            return new MSSQLFactory().ExecuteNonQuery(TSQL);
        }

        public int ToggleVerifyCombination(List<Guid> student_IDs, PrintProgramme printProgramme, short semester)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            string TSQL = $@"UPDATE s SET s.IsVerified = CASE
                                             WHEN s.IsVerified <> 1 THEN
                                                 1
                                             ELSE
                                                 0
                                         END FROM dbo.ARGSelectedCombination_{printProgramme.ToString()} s
				                         LEFT JOIN dbo.DegreeCertificate d ON d.Student_ID = s.Student_ID
                        WHERE s.Semester={semester} AND s.Student_ID IN ({student_IDs.ToIN()}) AND d.PrintedOn IS NULL ";
            return new MSSQLFactory().ExecuteNonQuery(TSQL);
        }

        public int ImportPGtoIH(Guid Student_ID, Guid NewStudent_ID)
        {
            #region Import Script
            string TSQL = $@"DECLARE @Student_ID UNIQUEIDENTIFIER;
                            SET @Student_ID='{NewStudent_ID}';
                            INSERT INTO dbo.ARGPersonalInformation_IH
                            (
	                               Student_ID,
                                   StudentFormNo,
                                   BoardRegistrationNo,
                                   CUSRegistrationNo,
                                   PreviousUniversityRegnNo,
                                   FullName,
                                   FathersName,
                                   MothersName,
                                   Gender,
                                   DOB,
                                   Religion,
                                   Category,
                                   AcceptCollege_ID,
                                   AcceptedBy_ID,
                                   IsPassout,
                                   Photograph,
                                   IsProvisional,
                                   FormStatus,
                                   CreatedOn,
                                   CreatedBy,
                                   UpdatedOn,
                                   UpdatedBy,
                                   TotalFee,
                                   EntranceRollNo,
                                   CurrentSemesterOrYear,
                                   Batch,
                                   ClassRollNo,
                                   CATEntrancePoints,
                                    IsLateralEntry
                            )
                            SELECT @Student_ID,
                                   StudentFormNo,
                                   BoardRegistrationNo,
                                   CUSRegistrationNo,
                                   PreviousUniversityRegnNo,
                                   FullName,
                                   FathersName,
                                   MothersName,
                                   Gender,
                                   DOB,
                                   Religion,
                                   Category,
                                   AcceptCollege_ID,
                                   AcceptedBy_ID,
                                   IsPassout,
                                   REPLACE(Photograph,'/PG/','/IH/'),
                                   IsProvisional,
                                   FormStatus,
                                   CreatedOn,
                                   CreatedBy,
                                   UpdatedOn,
                                   UpdatedBy,
                                   TotalFee,
                                   EntranceRollNo,
                                   CurrentSemesterOrYear,
                                   Batch,
                                   ClassRollNo,
                                   CATEntrancePoints,
                                    1
                                    FROM dbo.ARGPersonalInformation_PG WHERE Student_ID='{Student_ID}';
                            INSERT INTO dbo.ARGStudentAddress_IH
                            (
	                               Address_ID,
                                   Student_ID,
                                   PinCode,
                                   Mobile,
                                   Email,
                                   PermanentAddress,
                                   AssemblyConstituency,
                                   ParliamentaryConstituency,
                                   Block,
                                   Tehsil,
                                   District,
                                   State,
                                   CreatedOn,
                                   CreatedBy,
                                   UpdatedOn,
                                   UpdatedBy
                            )
                            SELECT NEWID(),
                                   @Student_ID,
                                   PinCode,
                                   Mobile,
                                   Email,
                                   PermanentAddress,
                                   AssemblyConstituency,
                                   ParliamentaryConstituency,
                                   Block,
                                   Tehsil,
                                   District,
                                   State,
                                   CreatedOn,
                                   CreatedBy,
                                   UpdatedOn,
                                   UpdatedBy FROM dbo.ARGStudentAddress_PG WHERE Student_ID='{Student_ID}';
                            INSERT INTO dbo.ARGStudentPreviousQualifications_IH
                            (
                                Qualification_ID,
                                Student_ID,
                                ExamName,
                                Stream,
                                Subjects,
                                Session,
                                Year,
                                RollNo,
                                MaxMarks,
                                MarksObt,
                                ExamBody,
                                CreatedOn,
                                CreatedBy,
                                UpdatedOn,
                                UpdatedBy
                            )
                            SELECT  NEWID(),
                                @Student_ID,
                                ExamName,
                                Stream,
                                Subjects,
                                Session,
                                Year,
                                RollNo,
                                MaxMarks,
                                MarksObt,
                                ExamBody,
                                CreatedOn,
                                CreatedBy,
                                UpdatedOn,
                                UpdatedBy FROM dbo.ARGStudentPreviousQualifications_PG WHERE Student_ID='{Student_ID}';
                            INSERT INTO dbo.ARGCoursesApplied_IH
                            (
                                Student_ID,
                                Course_ID,
                                Preference,
                                SubjectEntrancePoints,
                                AppearedInEntrance,
                                SelfFinancedPayment_ID,
                                StudentSelectionStatus,
                                SelectionAgaintListNo,
                                IsActive
                            )
                            SELECT TOP 1 @Student_ID,
                                   Course_ID,
                                   Preference,
                                   SubjectEntrancePoints,
                                   AppearedInEntrance,
                                   SelfFinancedPayment_ID,
                                   StudentSelectionStatus,
                                   SelectionAgaintListNo,
                                   IsActive FROM dbo.ARGCoursesApplied_PG WHERE Course_ID='A3EE7F98-7B82-4D95-A2C0-FABA7A18240E' AND Student_ID='{Student_ID}';
                            INSERT INTO dbo.PaymentDetails_IH
                            (
                                Payment_ID,
                                Entity_ID,
                                TxnReferenceNo,
                                BankReferenceNo,
                                TxnAmount,
                                BankID,
                                BankMerchantID,
                                TxnType,
                                CurrencyName,
                                ItemCode,
                                SecurityType,
                                SecurityID,
                                TxnDate,
                                AuthStatus,
                                SettlementType,
                                ErrorStatus,
                                ErrorDescription,
                                ModuleType,
                                PaymentType,
                                Email,
                                PhoneNumber,
                                Semester
                            )
                            SELECT NEWID(),
                                @Student_ID,
                                TxnReferenceNo,
                                BankReferenceNo,
                                TxnAmount,
                                BankID,
                                BankMerchantID,
                                TxnType,
                                CurrencyName,
                                ItemCode,
                                SecurityType,
                                SecurityID,
                                TxnDate,
                                AuthStatus,
                                SettlementType,
                                ErrorStatus,
                                ErrorDescription,
                                ModuleType,
                                PaymentType,
                                Email,
                                PhoneNumber,
                                Semester FROM dbo.PaymentDetails_PG WHERE Entity_ID='{Student_ID}' AND ModuleType IN (1,6);";
            #endregion
            return new MSSQLFactory().ExecuteNonQuery(TSQL);
        }
        public int DeleteFromIH(Guid Student_ID)
        {
            string TSQL = $@"DELETE FROM dbo.ARGStudentAddress_IH WHERE Student_ID='{Student_ID}';
                        DELETE FROM dbo.ARGStudentPreviousQualifications_IH WHERE Student_ID='{Student_ID}';
                        DELETE FROM dbo.ARGCoursesApplied_IH WHERE Student_ID='{Student_ID}';
                        DELETE FROM dbo.PaymentDetails_IH WHERE Entity_ID='{Student_ID}';
                        DELETE FROM dbo.ARGSelectedCombination_IH WHERE Student_ID='{Student_ID}';
                        DELETE FROM dbo.ARGPersonalInformation_IH WHERE Student_ID='{Student_ID}';";
            return new MSSQLFactory().ExecuteNonQuery(TSQL);
        }
        public int DeleteFromPG(Guid Student_ID)
        {
            string TSQL = $@"DELETE FROM dbo.ARGStudentAddress_PG WHERE Student_ID='{Student_ID}';
                        DELETE FROM dbo.ARGStudentPreviousQualifications_PG WHERE Student_ID='{Student_ID}';
                        DELETE FROM dbo.ARGCoursesApplied_PG WHERE Student_ID='{Student_ID}';
                        DELETE FROM dbo.PaymentDetails_PG WHERE Entity_ID='{Student_ID}';
                        DELETE FROM dbo.ARGSelectedCombination_PG WHERE Student_ID='{Student_ID}';
                        DELETE FROM dbo.ARGPersonalInformation_PG WHERE Student_ID='{Student_ID}';";
            return new MSSQLFactory().ExecuteNonQuery(TSQL);
        }
        public int DeleteGraduationCourseOnlyFromIH(Guid Student_ID)
        {
            string TSQL = $@"DELETE FROM dbo.ARGCoursesApplied_IH WHERE Student_ID='{Student_ID}' AND Course_ID='A3EE7F98-7B82-4D95-A2C0-FABA7A18240E';";
            return new MSSQLFactory().ExecuteNonQuery(TSQL);
        }

        public int DeletePaymentFromIHOnly(Guid Student_ID)
        {
            string TSQL = $"DELETE FROM dbo.PaymentDetails_IH WHERE Entity_ID='{Student_ID}' AND ModuleType IN (1,6);";
            return new MSSQLFactory().ExecuteNonQuery(TSQL);
        }

        public Guid? GetCourseIDByMajorSubjectSemester1(List<Guid> majorSubjectIDs, SubjectType major, int Semester)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = $@"SELECT TOP 1 Course_ID FROM dbo.ADMSubjectMaster
                                WHERE Subject_ID IN ({majorSubjectIDs.ToIN()}) AND Semester={Semester} AND SubjectType={(short)major}
                                ORDER BY FromBatch DESC";
            return new MSSQLFactory().ExecuteScalar<Guid?>(cmd);
        }
        #endregion
    }
}
