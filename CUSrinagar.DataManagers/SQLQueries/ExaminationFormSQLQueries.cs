using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CUSrinagar.Enums;
using CUSrinagar.Models;
using GeneralModels;
using Terex;

namespace CUSrinagar.DataManagers.SQLQueries
{
    public class ExaminationFormSQLQueries
    {
        internal SqlCommand GetRegularExamFormDownloadableWithoutDateValidation(ExaminationCourseCategory CourseCategory, short semester)
        {
            SqlCommand command = new SqlCommand
            {
                CommandText = @"SELECT * FROM ARGExamFormDownloadable WHERE CourseCategory = @CourseCategory AND Semester=@Semester AND IsBacklogSetting = 0"
            };
            command.Parameters.AddWithValue("@CourseCategory", CourseCategory.ToString());
            command.Parameters.AddWithValue("@Semester", semester);
            return command;
        }

        internal SqlCommand GetBacklogExamFormDownloadableWithoutDateValidation(ExaminationCourseCategory CourseCategory, short semester)
        {
            SqlCommand command = new SqlCommand
            {
                CommandText = @"SELECT * FROM ARGExamFormDownloadable WHERE CourseCategory = @CourseCategory AND Semester=@Semester AND IsBackLogSetting = 1"
            };
            command.Parameters.AddWithValue("@CourseCategory", CourseCategory.ToString());
            command.Parameters.AddWithValue("@Semester", semester);
            return command;
        }

        internal string GetRegularExamFormDownloadable(ExaminationCourseCategory courseCategory, short semester) => $@"SELECT * FROM ARGExamFormDownloadable WHERE CourseCategory = '{courseCategory}' AND Semester = {semester} AND IsBacklogSetting = 0 AND SYSDATETIME() >= StartDate
                                                                                                                                  AND
                                                                                                                                  (
                                                                                                                                      SYSDATETIME() <= EndDate
                                                                                                                                      OR (SYSDATETIME() <= DATEADD(DAY, GracePeriodDays1, EndDate))
                                                                                                                                  )";

        internal string GetBacklogExamFormDownloadable(ExaminationCourseCategory courseCategory, short semester) => $@"SELECT * FROM ARGExamFormDownloadable WHERE CourseCategory = '{courseCategory}' AND Semester = {semester} AND IsBacklogSetting = 1 AND SYSDATETIME() >= StartDate
                                                                                                                                      AND
                                                                                                                                      (
                                                                                                                                          SYSDATETIME() <= EndDate
                                                                                                                                          OR (SYSDATETIME() <= DATEADD(DAY, GracePeriodDays1, EndDate))
                                                                                                                                      )";

        internal SqlCommand GetStudentExamForm(Guid Student_ID, ARGExamFormDownloadable settingsForm, PrintProgramme printProgramme, bool isRegular)
        {
            printProgramme = printProgramme == PrintProgramme.BED ? PrintProgramme.UG : printProgramme;
            SqlCommand command = new SqlCommand
            {
                CommandText = $@"SELECT * FROM ARGStudentExamForm_{printProgramme.ToString()} WHERE Student_ID = @Student_ID AND Semester=@Semester AND Year=@Year AND IsRegular=@IsRegular"
            };
            command.Parameters.AddWithValue("@Student_ID", Student_ID);
            command.Parameters.AddWithValue("@Semester", settingsForm.Semester);
            command.Parameters.AddWithValue("@Year", settingsForm.Year);
            command.Parameters.AddWithValue("@IsRegular", isRegular);
            return command;
        }

        internal SqlCommand GetFeeStructure(Guid feeStructure_ID)
        {
            SqlCommand command = new SqlCommand
            {
                CommandText = $@"SELECT * FROM FeeStructure WHERE FeeStructure_ID = @feeStructure_ID "
            };
            command.Parameters.AddWithValue("@feeStructure_ID", feeStructure_ID);
            return command;
        }

        internal SqlCommand GetStudentExamForm(Guid examForm_ID, PrintProgramme printProgramme)
        {
            SqlCommand command = new SqlCommand
            {
                CommandText = $@"SELECT * FROM ARGStudentExamForm_{printProgramme.ToString()}
                                     WHERE StudentExamForm_ID = @examForm_ID "
            };
            command.Parameters.AddWithValue("@examForm_ID", examForm_ID);
            return command;
        }

        internal SqlCommand ExaminationAllowedForStudentSemester(ExaminationCourseCategory courseCategory, short semester)
        {
            SqlCommand command = new SqlCommand
            {
                CommandText = @" SELECT AllowDownloadForm FROM ARGExamFormDownloadable WHERE CourseCategory = @courseCategory AND Semester=@semester "
            };
            command.Parameters.AddWithValue("@courseCategory", courseCategory.ToString());
            command.Parameters.AddWithValue("@semester", semester);
            return command;
        }

        internal SqlCommand ExaminationAllowedForStudentCourse(Guid user_ID, PrintProgramme printProgramme, short semester)
        {
            SqlCommand command = new SqlCommand
            {
                CommandText = $@" SELECT AllowExaminationForm FROM ARGSelectedCombination_{printProgramme.ToString()}
                                        INNER JOIN ADMCombinationMaster 
                                        ON ADMCombinationMaster.Combination_ID = ARGSelectedCombination_{printProgramme.ToString()}.Combination_ID
                                        INNER JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = ADMCombinationMaster.Course_ID
                                        WHERE Student_ID = @Student_ID "
            };
            command.Parameters.AddWithValue("@Student_ID", user_ID);
            return command;
        }

        internal SqlCommand GetExaminationFormList(short semester, PrintProgramme printProgramme, bool getUnpaidStudents)
        {
            string status = getUnpaidStudents ? "1" : "4";

            SqlCommand command = new SqlCommand();
            command.CommandText = $@"SELECT
                                            ARGPersonalInformation_IH.Student_ID,CUSRegistrationNo, FullName,Semester, FormNumber, Year,ExamRollNumber, AmountPaid,Status,StudentExamForm_ID, IsRegular	
	                                        FROM ARGStudentExamForm_IH
                                            INNER JOIN ARGPersonalInformation_IH
                                                ON ARGPersonalInformation_IH.Student_ID = ARGStudentExamForm_IH.Student_ID
                                        WHERE ARGStudentExamForm_IH.Semester = @semester
                                              AND ARGStudentExamForm_IH.Status = {status}";
            command.Parameters.AddWithValue("@semester", semester);
            return command;
        }

        internal SqlCommand GetInEligibleCandiates(Guid student_ID, short Semester)
        {
            SqlCommand command = new SqlCommand();
            command.CommandText = $"SELECT * FROM InEligibleCandidates WHERE Student_ID = @student_ID AND Semester=@Semester";
            command.Parameters.AddWithValue("@student_ID", student_ID);
            command.Parameters.AddWithValue("@Semester", Semester);
            return command;
        }

        internal SqlCommand GetStudentExamFormByStudent(Guid student_ID, PrintProgramme printProgramme)
        {
            SqlCommand command = new SqlCommand();
            command.CommandText = $@"SELECT * FROM ARGStudentExamForm_{printProgramme.ToString()}
                                     WHERE Student_ID = @Student_ID ";
            command.Parameters.AddWithValue("@Student_ID", student_ID);
            return command;
        }


        internal string GetExaminationFormsQuery(Parameters parameters, PrintProgramme printProgramme)
        {
            //string StdInfoQuery = new StudentSQLQueries().StudentListCompact(printProgramme);
            string stdCompactQuery = new StudentSQLQueries().PersonalInformationCompactQuery(printProgramme);

            string Query = $@"SELECT STDINFO.*,Semester,FormNumber,year,ExamRollNumber,AmountPaid,EXMFRM.Status 
                            FROM {stdCompactQuery} STDINFO
                            LEFT JOIN ARGStudentExamForm_{printProgramme.ToString()} EXMFRM ON EXMFRM.Student_ID = STDINFO.Student_ID";
            return Query;
        }

        internal string GetExaminationFormsListQuery(PrintProgramme printProgramme, string Columns)
        {

            if (string.IsNullOrWhiteSpace(Columns))
            {
                Columns = @"CUSRegistrationNo, FullName, ExaminationForms.Semester, Batch, FormNumber,
                                      IsVerified, Year, ExamRollNumber,AmountPaid, ExaminationForms.Status, 
									  StudentExamForm_ID, IsRegular,ADMCourseMaster.Course_ID,CourseFullName,
									  ExaminationForms.CreatedOn ";
            }
            var tableAlias = new GeneralFunctions().MappingTable(printProgramme);
            string Query = $@" SELECT ARGPersonalInformation_{tableAlias}.Student_ID,
		                             { Columns}
                                FROM ARGPersonalInformation_{tableAlias}                                   
                                INNER JOIN ARGStudentExamForm_{tableAlias} ExaminationForms ON ExaminationForms.Student_ID = ARGPersonalInformation_{tableAlias}.Student_ID 
                                INNER JOIN ARGSelectedCombination_{tableAlias}
                                        ON ARGSelectedCombination_{tableAlias}.Student_ID = ARGPersonalInformation_{tableAlias}.Student_ID   
                                        AND ARGSelectedCombination_{tableAlias}.Semester = ExaminationForms.Semester
                                INNER JOIN ADMCombinationMaster
                                    ON ADMCombinationMaster.Combination_ID = ARGSelectedCombination_{tableAlias}.Combination_ID  
                                    AND ADMCombinationMaster.Semester = ARGSelectedCombination_{tableAlias}.Semester
                                INNER JOIN ADMCourseMaster
                                    ON ADMCourseMaster.Course_ID = ADMCombinationMaster.Course_ID AND ADMCourseMaster.PrintProgramme = {(short)printProgramme} ";
            return Query;
        }

        internal string SetLateFeeZero(Guid StudentExamForm_ID, string tableName)
        {
            return $" UPDATE {tableName} SET LateFeeAmount = 0 WHERE StudentExamForm_ID='{StudentExamForm_ID}'";
        }

        internal string GetExaminationFormsCSVListQuery(PrintProgramme printProgramme)
        {
            string Query = $@" SELECT STDINFO.Student_ID, CUSRegistrationNo, FullName, CurrentSemesterOrYear, SemesterBatch, ClassRollNo,
                            FormNumber, CATEntrancePoints, SelectedCombination_ID, COMB.Semester, IsVerified, Year,
                            ExamRollNumber, AmountPaid, ExaminationForms.Status, StudentExamForm_ID, IsRegular,CourseFullName,ExaminationForms.Semester
                            FROM ARGPersonalInformation_{printProgramme.ToString()} STDINFO
                            INNER JOIN ARGSelectedCombination_{printProgramme.ToString()} COMB ON COMB.Student_ID = STDINFO.Student_ID
                            INNER JOIN ADMCombinationMaster ON ADMCombinationMaster.Combination_ID = COMB.Combination_ID  AND ADMCombinationMaster.Semester = COMB.Semester
                            INNER JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = ADMCombinationMaster.Course_ID
                            LEFT JOIN ARGStudentExamForm_{printProgramme.ToString()} ExaminationForms ON ExaminationForms.Student_ID = STDINFO.Student_ID AND ExaminationForms.Semester = COMB.Semester AND ExaminationForms.Status={(short)FormStatus.Accepted}";
            return Query;
        }


        internal SqlCommand GetStudentExamForm(ARGStudentExamForm studentExamForm, PrintProgramme printProgramme)
        {
            SqlCommand command = new SqlCommand
            {
                CommandText = $@"SELECT * FROM ARGStudentExamForm_{printProgramme.ToString()}
                                     WHERE FormNumber = @FormNumber AND Semester=@Semester AND Year=@Year"
            };
            command.Parameters.AddWithValue("@FormNumber", studentExamForm.FormNumber.Trim());
            command.Parameters.AddWithValue("@Semester", studentExamForm.Semester);
            command.Parameters.AddWithValue("@Year", studentExamForm.Year);
            return command;
        }

        internal SqlCommand UpdateExaminationFormStatus(decimal Amount, int LateFeeAmount, string tableName, Guid StudentExamForm_ID, bool paymentSuccessful)
        {
            SqlCommand command = new SqlCommand();
            var formStatus = paymentSuccessful ? FormStatus.Accepted : FormStatus.PaymentUnsuccessful;

            string query = $@"UPDATE {tableName} SET Status = {(short)formStatus},AMOUNTPAID = (SELECT SUM(ISNULL(TxnAmount,0)) FROM {tableName.ToUpper().Replace("ARGSTUDENTEXAMFORM", "PAYMENTDETAILS")} WHERE ENTITY_ID=@StudentExamForm_ID),UpdatedOn = @UpdatedOn ,UpdatedBy=@updatedby_ID ";

            if (Amount == 0)
            {
                query = $@"UPDATE {tableName} SET Status = {(short)formStatus},AMOUNTPAID = 0,UpdatedOn = @UpdatedOn ,UpdatedBy=@updatedby_ID ";
            }

            string whereClause = "WHERE StudentExamForm_ID = @StudentExamForm_ID";

            if (paymentSuccessful)
            {
                command.Parameters.AddWithValue("@StudentExamForm_ID", StudentExamForm_ID);
                command.Parameters.AddWithValue("@UpdatedOn", DateTime.Now);
                command.Parameters.AddWithValue("@updatedby_ID", AppUserHelper.User_ID);
                command.Parameters.AddWithValue("@Amount", Amount);
            }

            if (LateFeeAmount > 0)
            {
                query += ",LateFeeAmount=@LateFeeAmount ";
                command.Parameters.AddWithValue("@LateFeeAmount", LateFeeAmount);
            }

            command.CommandText = query + whereClause;

            return command;
        }

        private void DeleteExamFormSubjects(Guid studentExamForm_ID)
        {
            SqlCommand command = new SqlCommand();

            new MSSQLFactory().ExecuteNonQuery(command);
        }

        internal SqlCommand GetAppliedBacklogSubjectsForStudent(Guid studentExamForm_ID, PrintProgramme printProgramme, short semester)
        {
            SqlCommand command = new SqlCommand();
            command.CommandText = $@"SELECT *,1 AS IsApplied,1 AS RecordState,Name AS SubjectFullName FROM ARGStudentReExamForm                                    
                                    INNER JOIN Subjects ON ARGStudentReExamForm.Subject_ID = Subjects.Subject_ID
                                    WHERE StudentExamForm_ID = @StudentExamForm_ID ";
            command.Parameters.AddWithValue("@StudentExamForm_ID", studentExamForm_ID);
            return command;
        }

        internal SqlCommand GetExaminationFormSubjects(Guid studentExamForm_ID)
        {
            SqlCommand command = new SqlCommand();
            command.CommandText = $@"SELECT *,1 AS IsApplied FROM ARGStudentReExamForm WHERE studentExamForm_ID = @studentExamForm_ID ";
            command.Parameters.AddWithValue("@studentExamForm_ID", studentExamForm_ID);

            return command;
        }

        internal SqlCommand GetCourseWidgetsQuery(PrintProgramme programme, int batch, int semester)
        {
            SqlCommand command = new SqlCommand
            {
                CommandText = $@" SELECT CourseFullName,
                                               SUM(AmountPaid) TotalAmountReceived,COUNT(IsRegular) NoOfRegularStudents,SUM(ISNULL(LateFeeAmount,0)) LateFeeAmountForCourse
                                        FROM ARGStudentExamForm_{programme.ToString()}
                                            INNER JOIN ARGPERSONALINFORMATION_{programme.ToString()} 
                                            ON ARGPERSONALINFORMATION_{programme.ToString()}.Student_ID = ARGSTUDENTExamForm_{programme.ToString()}.Student_ID
                                            INNER JOIN ARGSelectedCombination_{programme.ToString()}
                                                ON ARGSelectedCombination_{programme.ToString()}.Student_ID = ARGStudentExamForm_{programme.ToString()}.Student_ID
                                                   AND ARGSelectedCombination_{programme.ToString()}.Semester = @Semester
                                            INNER JOIN ADMCombinationMaster
                                                ON ADMCombinationMaster.Combination_ID = ARGSelectedCombination_{programme.ToString()}.Combination_ID
                                            INNER JOIN ADMCourseMaster
                                                ON ADMCourseMaster.Course_ID = ADMCombinationMaster.Course_ID
                                        WHERE ARGStudentExamForm_{programme.ToString()}.Status = 4
                                              AND ARGStudentExamForm_{programme.ToString()}.Semester = @Semester                                              
	                                          AND IsRegular = 1 AND Batch = @batch
                                        GROUP BY CourseFullName "
            };
            command.Parameters.AddWithValue("@Semester", semester);
            command.Parameters.AddWithValue("@batch", batch);
            return command;
        }

        internal SqlCommand GetNoOfBacklogStudents(PrintProgramme programme, int batch, int semester, string coursefullName)
        {
            SqlCommand command = new SqlCommand
            {
                CommandText = $@" SELECT COUNT(IsRegular) NoOfBacklogStudents
                                        FROM ARGStudentExamForm_{programme.ToString()}
                                        INNER JOIN ARGPersonalInformation_{programme.ToString()}
                                                ON ARGPersonalInformation_{programme.ToString()}.Student_ID = ARGStudentExamForm_{programme.ToString()}.Student_ID
                                            INNER JOIN ARGSelectedCombination_{programme.ToString()}
                                                ON ARGSelectedCombination_{programme.ToString()}.Student_ID = ARGStudentExamForm_{programme.ToString()}.Student_ID
                                                   AND ARGSelectedCombination_{programme.ToString()}.Semester = @Semester
                                            INNER JOIN ADMCombinationMaster
                                                ON ADMCombinationMaster.Combination_ID = ARGSelectedCombination_{programme.ToString()}.Combination_ID
                                            INNER JOIN ADMCourseMaster
                                                ON ADMCourseMaster.Course_ID = ADMCombinationMaster.Course_ID
                                        WHERE ARGStudentExamForm_{programme.ToString()}.Status = 4
                                              AND ARGStudentExamForm_{programme.ToString()}.Semester = @Semester
                                             AND Batch = @batch
	                                          AND IsRegular = 0 AND CourseFullName = @CourseFullName "
            };
            command.Parameters.AddWithValue("@Semester", semester);
            command.Parameters.AddWithValue("@CourseFullName", coursefullName);
            command.Parameters.AddWithValue("@batch", batch);
            return command;
        }

        internal SqlCommand GetNoOfFormApplied(PrintProgramme programme, int batch, int semester, string coursefullName)
        {
            SqlCommand command = new SqlCommand
            {
                CommandText = $@" SELECT COUNT(ARGStudentExamForm_{programme.ToString()}.Student_ID) FormApplied
                                        FROM ARGStudentExamForm_{programme.ToString()}
                                       INNER JOIN ARGPersonalInformation_{programme.ToString()}
                                                ON ARGPersonalInformation_{programme.ToString()}.Student_ID = ARGStudentExamForm_{programme.ToString()}.Student_ID
                                            INNER JOIN ARGSelectedCombination_{programme.ToString()}
                                                ON ARGSelectedCombination_{programme.ToString()}.Student_ID = ARGStudentExamForm_{programme.ToString()}.Student_ID
                                                   AND ARGSelectedCombination_{programme.ToString()}.Semester = @Semester
                                            INNER JOIN ADMCombinationMaster
                                                ON ADMCombinationMaster.Combination_ID = ARGSelectedCombination_{programme.ToString()}.Combination_ID
                                            INNER JOIN ADMCourseMaster
                                                ON ADMCourseMaster.Course_ID = ADMCombinationMaster.Course_ID
                                        WHERE 
                                              ARGStudentExamForm_{programme.ToString()}.Semester = @Semester
                                              AND Batch = @batch
	                                          AND CourseFullName = @CourseFullName "
            };
            command.Parameters.AddWithValue("@Semester", semester);
            command.Parameters.AddWithValue("@batch", batch);
            command.Parameters.AddWithValue("@CourseFullName", coursefullName);
            return command;
        }

        internal SqlCommand GetTotalNumberOfStudentsPerCourse(PrintProgramme programme, int batch, int semester, string courseFullName)
        {
            SqlCommand command = new SqlCommand
            {
                CommandText = $@" SELECT COUNT(ARGPersonalInformation_{programme.ToString()}.Student_ID) TotalNumberOfStudentsPerCourse
                                        FROM ARGPersonalInformation_{programme.ToString()}                                            
                                            INNER JOIN ARGSelectedCombination_{programme.ToString()}
                                                ON ARGSelectedCombination_{programme.ToString()}.Student_ID = ARGPersonalInformation_{programme.ToString()}.Student_ID
                                                   AND ARGSelectedCombination_{programme.ToString()}.Semester = @Semester                                            
                                            INNER JOIN ADMCombinationMaster
                                                ON ADMCombinationMaster.Combination_ID = ARGSelectedCombination_{programme.ToString()}.Combination_ID
                                            INNER JOIN ADMCourseMaster
                                                ON ADMCourseMaster.Course_ID = ADMCombinationMaster.Course_ID
                                        WHERE 
                                              ARGSelectedCombination_{programme.ToString()}.Semester = @Semester and FormStatus={(short)FormStatus.Selected}                                               
                                              AND CourseFullName = @CourseFullName AND Batch = @batch"
            };
            command.Parameters.AddWithValue("@Semester", semester);
            command.Parameters.AddWithValue("@batch", batch);
            command.Parameters.AddWithValue("@CourseFullName", courseFullName);
            return command;
        }

        internal SqlCommand GetLate1FeeAmountForWidget(PrintProgramme programme, int year, short semester)
        {
            SqlCommand command = new SqlCommand
            {
                CommandText = $@" SELECT COUNT(*) * ISNULL(LateFee1,0)
                                        FROM ARGExamFormDownloadable
                                            INNER JOIN ARGStudentExamForm_{programme.ToString()}
                                                ON ARGExamFormDownloadable.Notification_ID = ARGStudentExamForm_{programme.ToString()}.Notification_ID
                                                   AND ARGStudentExamForm_{programme.ToString()}.Semester = ARGExamFormDownloadable.Semester
                                        WHERE ARGStudentExamForm_{programme.ToString()}.Notification_ID IS NOT NULL
                                              AND CreatedOn > EndDate
                                              AND CreatedOn < DATEADD(DAY, GracePeriodDays1, EndDate)
                                              AND CourseCategory = 'IH'
	                                          GROUP BY LateFee1 "
            };
            command.Parameters.AddWithValue("@Semester", semester);
            command.Parameters.AddWithValue("@Year", year);
            command.Parameters.AddWithValue("@CourseCategory", new GeneralFunctions().GetExaminationCourseCategory(programme).ToString());
            return command;
        }

        internal SqlCommand GetLate2FeeAmountForWidget(PrintProgramme programme, int year, short semester)
        {
            SqlCommand command = new SqlCommand
            {
                CommandText = $@" SELECT COUNT(*) * ISNULL(LateFee2,0)
                                        FROM ARGExamFormDownloadable
                                            INNER JOIN ARGStudentExamForm_{programme.ToString()}
                                                ON ARGExamFormDownloadable.Notification_ID = ARGStudentExamForm_{programme.ToString()}.Notification_ID
                                                   AND ARGStudentExamForm_{programme.ToString()}.Semester = ARGExamFormDownloadable.Semester
                                        WHERE ARGStudentExamForm_{programme.ToString()}.Notification_ID IS NOT NULL
                                              AND CreatedOn > DATEADD(DAY, GracePeriodDays1, EndDate)
                                              AND CourseCategory = 'IH'
	                                          GROUP BY LateFee2 "
            };
            command.Parameters.AddWithValue("@Semester", semester);
            command.Parameters.AddWithValue("@Year", year);
            command.Parameters.AddWithValue("@CourseCategory", new GeneralFunctions().GetExaminationCourseCategory(programme).ToString());
            return command;
        }

        internal SqlCommand GetLateFeeAmountForBackLogStudents(PrintProgramme programme, int batch, int semester, string courseFullName)
        {
            SqlCommand command = new SqlCommand
            {
                CommandText = $@" SELECT SUM(ISNULL(LatefeeAmount,0)) LateFeeAmountForBackLogStudents
                                        FROM ARGStudentExamForm_{programme.ToString()}
                                       INNER JOIN ARGPersonalInformation_{programme.ToString()}
                                                ON ARGPersonalInformation_{programme.ToString()}.Student_ID = ARGStudentExamForm_{programme.ToString()}.Student_ID
                                            INNER JOIN ARGSelectedCombination_{programme.ToString()}
                                                ON ARGSelectedCombination_{programme.ToString()}.Student_ID = ARGStudentExamForm_{programme.ToString()}.Student_ID
                                                   AND ARGSelectedCombination_{programme.ToString()}.Semester = @Semester
                                            INNER JOIN ADMCombinationMaster
                                                ON ADMCombinationMaster.Combination_ID = ARGSelectedCombination_{programme.ToString()}.Combination_ID
                                            INNER JOIN ADMCourseMaster
                                                ON ADMCourseMaster.Course_ID = ADMCombinationMaster.Course_ID
                                        WHERE 
                                              ARGStudentExamForm_{programme.ToString()}.Semester = @Semester
                                              AND Batch = @batch AND ARGStudentExamForm_{programme.ToString()}.IsRegular = 0
	                                          AND CourseFullName = @CourseFullName "
            };
            command.Parameters.AddWithValue("@Semester", semester);
            command.Parameters.AddWithValue("@batch", batch);
            command.Parameters.AddWithValue("@CourseFullName", courseFullName);
            return command;
        }

        internal SqlCommand GetLateFeeAmountForRegularStudents(PrintProgramme programme, int batch, int semester, string courseFullName)
        {
            SqlCommand command = new SqlCommand
            {
                CommandText = $@" SELECT SUM(ISNULL(LatefeeAmount,0)) LateFeeAmountForRegularStudents
                                        FROM ARGStudentExamForm_{programme.ToString()}
                                       INNER JOIN ARGPersonalInformation_{programme.ToString()}
                                                ON ARGPersonalInformation_{programme.ToString()}.Student_ID = ARGStudentExamForm_{programme.ToString()}.Student_ID
                                            INNER JOIN ARGSelectedCombination_{programme.ToString()}
                                                ON ARGSelectedCombination_{programme.ToString()}.Student_ID = ARGStudentExamForm_{programme.ToString()}.Student_ID
                                                   AND ARGSelectedCombination_{programme.ToString()}.Semester = @Semester
                                            INNER JOIN ADMCombinationMaster
                                                ON ADMCombinationMaster.Combination_ID = ARGSelectedCombination_{programme.ToString()}.Combination_ID
                                            INNER JOIN ADMCourseMaster
                                                ON ADMCourseMaster.Course_ID = ADMCombinationMaster.Course_ID
                                        WHERE 
                                              ARGStudentExamForm_{programme.ToString()}.Semester = @Semester
                                              AND Batch = @batch AND ARGStudentExamForm_{programme.ToString()}.IsRegular = 1
	                                          AND CourseFullName = @CourseFullName "
            };
            command.Parameters.AddWithValue("@Semester", semester);
            command.Parameters.AddWithValue("@batch", batch);
            command.Parameters.AddWithValue("@CourseFullName", courseFullName);
            return command;
        }
    }
}
