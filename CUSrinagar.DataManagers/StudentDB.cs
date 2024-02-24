using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CUSrinagar.Models;
using Terex;
using CUSrinagar.DataManagers.SQLQueries;
using CUSrinagar.Extensions;
using CUSrinagar.Enums;
using GeneralModels;
using System.Web.Mvc;
using System.Data.SqlClient;
using System.Data;

namespace CUSrinagar.DataManagers
{
    public class StudentDB
    {
        public string GetStudentCourseName(Guid Student_ID)
        {
            return new MSSQLFactory().ExecuteScalar<string>($"SELECT TOP 1 CourseFullName FROM dbo.VWStudentCourse WHERE Student_ID='{Student_ID}'");
        }

        public short GetStudentCurrentSemester(Guid Student_ID, PrintProgramme printProgramme)
        {
            return new MSSQLFactory().ExecuteScalar<short>($"SELECT TOP 1 CurrentSemesterOrYear FROM dbo.ARGPersonalInformation_{printProgramme} WHERE Student_ID='{Student_ID}'");
        }

        public Guid GetStudentID(string cUSRegnNoOrFormNoOrBoardRegnNo, DateTime DOB, PrintProgramme printProgramme, bool ispassout)
        {
            SqlCommand command = new SqlCommand();
            command.Parameters.AddWithValue("@CUSRegistrationNo", cUSRegnNoOrFormNoOrBoardRegnNo);

            command.Parameters.AddWithValue("@DOB", DOB.ToString("yyyy-MM-dd"));
            if (!ispassout)
            {
                command.CommandText = $@"SELECT TOP 1 Student_ID FROM ARGPersonalInformation_{printProgramme} WHERE (DOB=@DOB) AND (CUSRegistrationNo = @CUSRegistrationNo OR StudentFormNo=@CUSRegistrationNo OR BoardRegistrationNo=@CUSRegistrationNo)
                                   AND FormStatus NOT IN('{(short)FormStatus.Rejected}','{(short)FormStatus.Disqualified}','{(short)FormStatus.Cancelled}','{(short)FormStatus.CancelRegistration}') ORDER BY Batch DESC";
            }
            else
            {
                command.CommandText = $@"SELECT TOP 1 P.Student_ID FROM ARGPersonalInformation_{printProgramme} P
                                            INNER JOIN dbo.MSCGPA ON MSCGPA.Student_ID = P.Student_ID
                                            WHERE (P.DOB=@DOB) AND (P.CUSRegistrationNo = @CUSRegistrationNo 
                                            OR P.StudentFormNo=@CUSRegistrationNo OR P.BoardRegistrationNo=@CUSRegistrationNo)
                                   AND P.FormStatus NOT IN('{(short)FormStatus.Rejected}','{(short)FormStatus.Disqualified}',
                                    '{(short)FormStatus.Cancelled}','{(short)FormStatus.CancelRegistration}') AND P.IsPassout=1 ORDER BY Batch DESC";

            }


            Guid Student_ID = new MSSQLFactory().ExecuteScalar<Guid>(command);
            if (Student_ID == Guid.Empty)
            {
                if (cUSRegnNoOrFormNoOrBoardRegnNo.ToLower().Contains("-17-gct-") && printProgramme == PrintProgramme.IH)
                {
                    command.CommandText = $@"SELECT TOP 1 Student_ID FROM ARGPersonalInformation_{printProgramme} WHERE (DOB=@DOB) AND (CUSRegistrationNo = @CUSRegistrationNo OR StudentFormNo=@CUSRegistrationNo OR BoardRegistrationNo=@CUSRegistrationNo)
                                   AND FormStatus NOT IN('{(short)FormStatus.Rejected}','{(short)FormStatus.Disqualified}','{(short)FormStatus.Cancelled}','{(short)FormStatus.CancelRegistration}') ORDER BY Batch DESC";

                    Student_ID = new MSSQLFactory().ExecuteScalar<Guid>(command);
                }
            }

            return Student_ID;
        }

        public List<Enrollments> GetEnrollment(Parameters parameters)
        {
            var Query = $@" SELECT CollegeFullName,Batch,COUNT(Student_ID) Enrollment FROM VWStudentCourse";

            FilterHelper helper = new GeneralFunctions().GetWhereClause<ARGPersonalInformation>(parameters.Filters);

            helper.Command.CommandText = Query + helper.WhereClause + "  GROUP BY CollegeFullName,Batch  ORDER BY CollegeFullName,Batch ";
            return new MSSQLFactory().GetObjectList<Enrollments>(helper.Command);
        }

        public List<ADMSubjectMaster> GetStudentSubjects(string RegistrationNumber)
        {
            return new MSSQLFactory().GetObjectList<ADMSubjectMaster>(new StudentSQLQueries().GetStudentSubjects(RegistrationNumber));
        }
        public List<ADMSubjectMaster> GetStudentsBySubjectsAndCollege(string RegistrationNumber)
        {
            return new MSSQLFactory().GetObjectList<ADMSubjectMaster>(new StudentSQLQueries().GetStudentsBySubjectsAndCollege(RegistrationNumber));
        }

        public int GetStudentsForFeeStructure(int Programme, int year)
        {
            return new MSSQLFactory().ExecuteScalar<int>(new StudentSQLQueries().GetStudentsForFeeStructure(Programme, year));
        }

        public List<ADMSubjectMaster> GetStudentsBySubjectsAndCollege(string RegistrationNumber, string semesterName, Programme programme)
        {
            return new MSSQLFactory().GetObjectList<ADMSubjectMaster>(new StudentSQLQueries().GetStudentsBySubjectsAndCollege(RegistrationNumber, semesterName, programme));
        }

        public List<AdmissionForm> GetStudentIDbyCourseANDBatch(int batch, Guid course_ID)
        {
            return new MSSQLFactory().GetObjectList<AdmissionForm>(new StudentSQLQueries().GetStudentIDByCourseAndBatch(batch, course_ID));
        }

        public short StudentExists(Guid student_ID)
        {
            var batch = new MSSQLFactory().ExecuteScalar<short>(new StudentSQLQueries().StudentExists(student_ID));
            return batch;
        }

        //public List<SubjectCombinations> GetSubjectCombinations()
        //{
        //    return new MSSQLFactory().GetObjectList<SubjectCombinations>(new StudentSQLQueries().GetAdmissionSelectionCriteria());
        //}

        public List<AdmissionSelectionCriteria> GetAdmissionSelectionCriteria()
        {
            return new MSSQLFactory().GetObjectList<AdmissionSelectionCriteria>(new StudentSQLQueries().GetAdmissionSelectionCriteria());
        }

        public List<ADMSubjectMaster> GetStudentSubjectAwards(string subjects, string registrationNumber, PrintProgramme programme)
        {
            return new MSSQLFactory().GetObjectList<ADMSubjectMaster>(new StudentSQLQueries().GetStudentSubjectAwards(subjects, registrationNumber, programme));
        }


        public List<Semesters> GetStudentSemesters(Guid student_Id, PrintProgramme printProgramme)
        {
            return new MSSQLFactory().GetObjectList<Semesters>(new StudentSQLQueries().GetStudentSemesters(student_Id, printProgramme));
        }



        public List<ExamForm> GetStudentExaminationHistory(Guid student_Id, PrintProgramme printProgramme, Semester semester)
        {
            return new MSSQLFactory().GetObjectList<ExamForm>(new StudentSQLQueries().GetStudentExaminationHistory(student_Id, printProgramme, semester));
        }
        public ExamForm GetStudentRecentExaminationForm(Guid student_Id, PrintProgramme printProgramme)
        {
            return new MSSQLFactory().GetObject<ExamForm>(new StudentSQLQueries().GetStudentRecentExaminationForm(student_Id, printProgramme));
        }
        public List<ADMSubjectMaster> GetStudentAllSubjects(Guid student_Id, PrintProgramme printProgramme, Semester semester)
        {
            return new MSSQLFactory().GetObjectList<ADMSubjectMaster>(new StudentSQLQueries().GetStudentAllSubjects(student_Id, printProgramme, semester));
        }

        public int ExistStudentForCategory(string categoryCode, Parameters parameter, string insubClause)
        {
            return new MSSQLFactory().ExecuteScalar<int>(new StudentSQLQueries().ExistStudentForCategory(categoryCode, parameter, insubClause));
        }

        public List<StudentPayment> GetStudentPaymentHistory(Guid student_Id, PrintProgramme printProgramme)
        {
            return new MSSQLFactory().GetObjectList<StudentPayment>(new StudentSQLQueries().GetStudentPaymentHistory(student_Id, printProgramme));
        }

        public List<PersonalInformationCompact> GetStudentsForCourse(Parameters parameters, PrintProgramme programme, short batchToAssign)
        {
            programme = new GeneralFunctions().MappingTable(programme);
            var Query = $@"SELECT ARGPersonalInformation_{programme}.*
                                            FROM ARGPersonalInformation_{programme}
                                                INNER JOIN ARGSelectedCombination_{programme}
                                                    ON ARGSelectedCombination_{programme}.Student_ID = ARGPersonalInformation_{programme}.Student_ID
                                                INNER JOIN ADMCombinationMaster
                                                    ON ADMCombinationMaster.Combination_ID = ARGSelectedCombination_{programme}.Combination_ID
                                                INNER JOIN ADMCourseMaster
                                                    ON ADMCourseMaster.Course_ID = ADMCombinationMaster.Course_ID
                                                INNER JOIN ADMCollegeMaster
                                                    ON AcceptCollege_ID = ADMCollegeMaster.College_ID 
                                                WHERE BATCH ={batchToAssign}";

            FilterHelper helper = new GeneralFunctions().GetWhereClause<ARGPersonalInformation>(parameters.Filters);

            helper.Command.CommandText = Query + helper.WhereClause.Replace("WHERE", "AND") + " ORDER BY CourseFullName,CombinationCode ASC ";
            return new MSSQLFactory().GetObjectList<PersonalInformationCompact>(helper.Command);
        }

        public MigrationPI GetMigrationDetail(Guid student_ID, PrintProgramme programme, string formType)
        {
            return new MSSQLFactory().GetObject<MigrationPI>(new MigrationSQLQueries().GetMigration(student_ID, formType, programme));
        }

        public bool AddSeatCount(SeatCountAgainstCourse seatCountAgainstCourse)
        {
            return new MSSQLFactory().InsertRecord<SeatCountAgainstCourse>(seatCountAgainstCourse) > 0;
        }

        public int DeletePreviousTranscript(Guid Student_Id)
        {
            var Query = $@"Delete from MSCGPA WHERE Student_Id='{Student_Id}'
                           Delete from MSSGPA WHERE Student_Id='{Student_Id}'
                           Delete from MSStudentMarks WHERE Student_Id='{Student_Id}'
                            update ARGPersonalInformation_UG
                            set IsPassout=0
                            where Student_Id='{Student_Id}'";

            SqlCommand sqlCommand = new SqlCommand(Query);
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }

        public MSCGPA GetPreviousTranscript(Guid student_ID)
        {
            var Query = $@"SELECT * from MSCGPA where Student_ID ='{student_ID}'";
            SqlCommand command = new SqlCommand();
            command.CommandText = Query;
            return new MSSQLFactory().GetObject<MSCGPA>(command);
        }
        public int DeletePreviousTranscript(Guid Student_Id, MSTranscriptDegreeSettings mSTranscriptDegreeSettings, PrintProgramme printProgramme)
        {
            var Query = $@"Delete from MSCGPA WHERE Student_Id='{Student_Id}' AND SemesterTo={mSTranscriptDegreeSettings.SemesterTo} AND TCourse_ID='{mSTranscriptDegreeSettings.Course_ID}'
                           Delete from MSSGPA WHERE Student_Id='{Student_Id}' AND Semester >={mSTranscriptDegreeSettings.SemesterFrom}  AND Semester <={mSTranscriptDegreeSettings.SemesterTo} AND TCourse_ID='{mSTranscriptDegreeSettings.Course_ID}'
                           Delete from MSStudentMarks WHERE Student_Id='{Student_Id}' AND Semester >={mSTranscriptDegreeSettings.SemesterFrom}  AND Semester <={mSTranscriptDegreeSettings.SemesterTo} AND TCourse_ID='{mSTranscriptDegreeSettings.Course_ID}'
                            update ARGPersonalInformation_{printProgramme.GetTablePFix()}
                            set IsPassout=0
                            where Student_Id='{Student_Id}'";

            SqlCommand sqlCommand = new SqlCommand(Query);
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }

        public MSCGPA GetPreviousTranscript(Guid student_ID, MSTranscriptDegreeSettings mSTranscriptDegreeSettings)
        {
            var Query = $@"SELECT * from MSCGPA where Student_ID ='{student_ID}' AND SemesterTo={mSTranscriptDegreeSettings.SemesterTo} AND TCourse_ID='{mSTranscriptDegreeSettings.Course_ID}'";
            SqlCommand command = new SqlCommand();
            command.CommandText = Query;
            return new MSSQLFactory().GetObject<MSCGPA>(command);
        }

        public bool AddSeatMatrix(SeatAllocationMatrix seatAllocationMatrix, PrintProgramme printProgramme)
        {
            return new MSSQLFactory().InsertRecord(seatAllocationMatrix, "SeatAllocationMatrix_" + printProgramme.GetTablePFix()) > 0;
        }
        public int UpdateFractionAssignrSeat(Guid course_ID)
        {
            var Query = $@"UPDATE FractionSeats SET
                        AssignedSeats=AssignedSeats+1
                         WHERE  Course_ID=@Course_ID and Category='TOTAL'";

            SqlCommand sqlCommand = new SqlCommand(Query);
            sqlCommand.Parameters.AddWithValue("@Course_ID", course_ID);
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }

        public SeatAllocationMatrix GetStudentFromMatrix(Guid student_ID, PrintProgramme printProgramme)
        {
            var Query = $@"SELECT * from SeatAllocationMatrix_{printProgramme.GetTablePFix()} where Student_ID ='{student_ID}'";
            SqlCommand command = new SqlCommand();
            command.CommandText = Query;
            return new MSSQLFactory().GetObject<SeatAllocationMatrix>(command);
        }
        public int UpdateStatus(Guid Student_Id, PrintProgramme printProgramme)
        {
            var Query = $@"UPDATE  SeatAllocationMatrix_{printProgramme.GetTablePFix()}  SET ISFinallySelected=1 WHERE Student_Id='{Student_Id}'";

            SqlCommand sqlCommand = new SqlCommand(Query);
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }

        public int UpdateSelectionListCount(Programme Programme)
        {
            var Query = $@"
                            UPDATE ADMCourseMaster SET CurrentSelectionListNo=CurrentSelectionListNo+1,AllowNewSelectionList=0
                            WHERE Programme={(int)Programme}";

            SqlCommand sqlCommand = new SqlCommand(Query);
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }
        public int UpdateSeatCount(Programme Programme)
        {
            var Query = $@" UPDATE SeatCountAgainstCourse
                            SET AllocatedSeats= (SELECT COUNT(Student_Id) FROM SeatAllocationMatrix_{Programme.GetTablePFix()} WHERE AssignedUnderCategory=0 AND ISFinallySelected=1  AND PreferenceOfCourse=1 AND Course_IdAssigned=Course_Id)
                            WHERE Category='OM' and PrintProgramme={(int)Programme}

                            UPDATE  SeatCountAgainstCourse
                            SET AllocatedSeats= (SELECT COUNT(Student_Id) FROM SeatAllocationMatrix_{Programme.GetTablePFix()} SM WHERE SM.AssignedUnderCategory=1  AND ISFinallySelected=1  AND PreferenceOfCourse=1  AND SM.Course_IdAssigned=Course_Id AND Category=(SELECT PINFO.Category FROM ARGPersonalInformation_{Programme.GetTablePFix()}  PINFO WHERE PINFO.Student_ID=SM.Student_Id))
                            WHERE Category<>'OM'  and PrintProgramme={(int)Programme}";

            SqlCommand sqlCommand = new SqlCommand(Query);
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }
        public int UpdateNOOFSeats(Guid course_ID, string category, int cnt = 1)
        {
            var Query = $@"UPDATE SeatCountAgainstCourse SET
                        NoofSeats=NoofSeats+{cnt}
                         WHERE  Course_ID='{course_ID}' and Category='{category}'";

            SqlCommand sqlCommand = new SqlCommand(Query);
            //sqlCommand.Parameters.AddWithValue("@Course_ID", course_ID);
            // sqlCommand.Parameters.AddWithValue("@Category", category);
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }

        public int DecreaseNOOFSeats(Guid course_ID, string Category, int cnt = 1)
        {
            var Query = $@"UPDATE SeatCountAgainstCourse SET
                        NoofSeats=NoofSeats-{cnt}
                         WHERE  Course_ID=@Course_ID and Category=@Category";

            SqlCommand sqlCommand = new SqlCommand(Query);
            sqlCommand.Parameters.AddWithValue("@Course_ID", course_ID);
            sqlCommand.Parameters.AddWithValue("@Category", Category);
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }
        public int UpdateSeatCount(Guid course_ID, string category, int cnt = 1)
        {
            var Query = $@"UPDATE SeatCountAgainstCourse SET
                        AllocatedSeats=AllocatedSeats+{cnt}
                         WHERE  Course_ID='{course_ID}' and Category='{category}'";

            SqlCommand sqlCommand = new SqlCommand(Query);
            //sqlCommand.Parameters.AddWithValue("@Course_ID", course_ID);
            // sqlCommand.Parameters.AddWithValue("@Category", category);
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }


        public int DecreaseSeatCount(Guid course_ID, string Category, int cnt = 1)
        {
            var Query = $@"UPDATE SeatCountAgainstCourse SET
                        AllocatedSeats=AllocatedSeats-{cnt}
                         WHERE  Course_ID=@Course_ID and Category=@Category";

            SqlCommand sqlCommand = new SqlCommand(Query);
            sqlCommand.Parameters.AddWithValue("@Course_ID", course_ID);
            sqlCommand.Parameters.AddWithValue("@Category", Category);
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }
        #region ug
        public int UpdateSeatCountUG(PrintProgramme printProgramme)
        {
            var Query = $@" UPDATE SeatCountAgainstCourse
                            SET AllocatedSeats= (SELECT COUNT(Student_Id) FROM SeatAllocationMatrix_{printProgramme.GetTablePFix()} WHERE AssignedUnderCategory=0 AND ISFinallySelected=1  AND PreferenceOfCourse=1 AND PreferenceOfCollege=1 AND Course_IdAssigned=Course_Id AND College_IdAssigned=College_Id)
                            WHERE Category='OM' and PrintProgramme={(int)printProgramme}

                            UPDATE  SeatCountAgainstCourse
                            SET AllocatedSeats=0
                            WHERE Category<>'OM'  and PrintProgramme={(int)printProgramme}";

            SqlCommand sqlCommand = new SqlCommand(Query);
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }
        public int UpdateNOOFSeats(Guid course_ID, Guid College_ID, string category, int cnt = 1)
        {
            var Query = $@"UPDATE SeatCountAgainstCourse SET
                        NoofSeats=NoofSeats+{cnt}
                         WHERE  Course_ID='{course_ID}' and Category='{category}' and College_ID='{College_ID}'";

            SqlCommand sqlCommand = new SqlCommand(Query);
            //sqlCommand.Parameters.AddWithValue("@Course_ID", course_ID);
            // sqlCommand.Parameters.AddWithValue("@Category", category);
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }

        public int DecreaseNOOFSeats(Guid course_ID, Guid College_ID, string Category, int cnt = 1)
        {
            var Query = $@"UPDATE SeatCountAgainstCourse SET
                        NoofSeats=NoofSeats-{cnt}
                         WHERE  Course_ID=@Course_ID and Category=@Category and College_ID=@College_ID";

            SqlCommand sqlCommand = new SqlCommand(Query);
            sqlCommand.Parameters.AddWithValue("@Course_ID", course_ID);
            sqlCommand.Parameters.AddWithValue("@Category", Category);
            sqlCommand.Parameters.AddWithValue("@College_ID", College_ID);
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }

        public int UpdateSeatCount(Guid course_ID, Guid College_ID, string category, int cnt = 1)
        {
            var Query = $@"UPDATE SeatCountAgainstCourse SET
                        AllocatedSeats=AllocatedSeats+{cnt}
                         WHERE  Course_ID='{course_ID}' and College_ID='{College_ID}' and Category='{category}'";

            SqlCommand sqlCommand = new SqlCommand(Query);
            //sqlCommand.Parameters.AddWithValue("@Course_ID", course_ID);
            // sqlCommand.Parameters.AddWithValue("@Category", category);
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }
        public int DecreaseSeatCount(Guid course_ID, Guid College_ID, string Category, int cnt = 1)
        {
            var Query = $@"UPDATE SeatCountAgainstCourse SET
                        AllocatedSeats=AllocatedSeats-{cnt}
                         WHERE  Course_ID=@Course_ID and College_ID='{College_ID}'  and Category=@Category";

            SqlCommand sqlCommand = new SqlCommand(Query);
            sqlCommand.Parameters.AddWithValue("@Course_ID", course_ID);
            sqlCommand.Parameters.AddWithValue("@Category", Category);
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }

        public int UpdateSeatCount(Guid course_ID, Guid College_Id, int TNoofSeats, int currentList, PrintProgramme printProgramme)
        {
            var Query = $@"UPDATE SeatCountAgainstCourse
                    SET NoofSeats = NoofSeats+ISNULL((SELECT SUM(cc.NoofSeats) FROM   SeatCountAgainstCourse cc  WHERE cc.Course_Id='{course_ID}' AND cc.College_ID='{College_Id}'
                     AND (AllocatedSeats=0 AND NoofSeats>1)),0)
                     WHERE  Course_Id ='{course_ID}' AND College_ID='{College_Id}' AND Category = 'OM'

                    UPDATE SeatCountAgainstCourse
                    SET NoofSeats = 0
                    WHERE AllocatedSeats = 0 AND NoofSeats>=1
                    AND Course_Id ='{course_ID}'  AND College_ID='{College_Id}'

                    UPDATE SeatCountAgainstCourse
                    SET NoofSeats = AllocatedSeats
                    WHERE Category!='OM'
                    AND Course_Id =  '{course_ID}'  AND College_ID='{College_Id}'

                                    
                    UPDATE SeatCountAgainstCourse
                    SET NoofSeats = NoofSeats + ({TNoofSeats}-ISNULL((SELECT SUM(NoofSeats) FROM   SeatCountAgainstCourse  WHERE Course_Id='{course_ID}' AND College_ID='{College_Id}'),0))
                    WHERE Category = 'OM'
                    AND Course_Id =  '{course_ID}' AND College_ID='{College_Id}'


                    UPDATE SeatCountAgainstCourse
                            SET AllocatedSeats= (SELECT COUNT(Student_Id) FROM SeatAllocationMatrix_{printProgramme.GetTablePFix()} WHERE AssignedUnderCategory=0 AND PreferenceOfCourse=1 AND PreferenceOfCollege=1 AND ISFinallySelected=1 AND Course_IdAssigned=Course_Id AND College_IdAssigned=College_Id)
                            WHERE Category='OM' AND Course_Id='{course_ID}' AND College_Id='{College_Id}'

                            UPDATE  SeatCountAgainstCourse
                            SET AllocatedSeats= 0
                            WHERE Category<>'OM'  AND Course_Id='{course_ID}'  AND College_Id='{College_Id}'


                    DELETE FROM SeatAllocationMatrix_{printProgramme.GetTablePFix()}
                    WHERE Course_IdAssigned= '{course_ID}' AND College_IdAssigned='{College_Id}' and (ISFinallySelected=0 AND SelectionListNo = {currentList} ) 
                    
                    DELETE FROM FractionSeats";

            SqlCommand sqlCommand = new SqlCommand(Query);
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }


        #endregion
        public int DeleteStudentFromMatrix(Guid student_ID, PrintProgramme printProgramme)
        {
            var Query = string.Empty;
            if (printProgramme == PrintProgramme.IH)
            { Query = $@"DELETE FROM SeatAllocationMatrix_{printProgramme.GetTablePFix()} WHERE (PreferenceOfCourse<>1 OR (PreferenceOfCourse=1 AND AssignedUnderCategory=1)) AND Student_Id=@Student_ID "; }
            else { Query = $@"DELETE FROM SeatAllocationMatrix_{printProgramme.GetTablePFix()} WHERE Student_Id=@Student_ID "; }

            SqlCommand sqlCommand = new SqlCommand(Query);
            sqlCommand.Parameters.AddWithValue("@Student_ID", student_ID);
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }


        public int UpdateSeatCount(Guid course_ID, int TNoofSeats, int currentList, PrintProgramme printProgramme)
        {
            //if(PrintProgramme)
            var Query = $@"UPDATE SeatCountAgainstCourse
                    SET NoofSeats = NoofSeats+ISNULL((SELECT SUM(cc.NoofSeats) FROM  SeatCountAgainstCourse cc  WHERE cc.Course_Id='{course_ID}'
                    AND Category NOT LIKE '%(non-%'
                    AND (AllocatedSeats=0 AND NoofSeats>=1)),0)
                    WHERE  Course_Id ='{course_ID}' AND  Category = 'OM'

                    UPDATE SeatCountAgainstCourse
                    SET NoofSeats = 0
                    WHERE AllocatedSeats = 0 AND NoofSeats>=1
                    AND Course_Id ='{course_ID}' 

                    UPDATE SeatCountAgainstCourse
                    SET NoofSeats = AllocatedSeats
                    WHERE Category!='OM'
                    AND Course_Id =  '{course_ID}'

                                    
                    UPDATE SeatCountAgainstCourse
                    SET NoofSeats = NoofSeats + ({TNoofSeats}-ISNULL((SELECT SUM(NoofSeats) FROM  SeatCountAgainstCourse  WHERE Course_Id='{course_ID}'),0))
                    WHERE Category = 'OM'
                    AND Course_Id =  '{course_ID}'


                    UPDATE SeatCountAgainstCourse
                    SET AllocatedSeats= (SELECT COUNT(Student_Id) FROM SeatAllocationMatrix_{printProgramme.GetTablePFix()}
                    WHERE AssignedUnderCategory=0 AND PreferenceOfCourse=1 AND ISFinallySelected=1 AND Course_IdAssigned=Course_Id)
                    WHERE Category='OM' AND Course_Id='{course_ID}'

                    UPDATE  SeatCountAgainstCourse
                    SET AllocatedSeats= (SELECT COUNT(Student_Id) FROM SeatAllocationMatrix_{printProgramme.GetTablePFix()} SM
                    WHERE SM.AssignedUnderCategory=1  AND PreferenceOfCourse=1  AND ISFinallySelected=1  AND SM.Course_IdAssigned=Course_Id 
                    AND Category=(SELECT PINFO.Category FROM ARGPersonalInformation_{printProgramme.GetTablePFix()}  PINFO WHERE PINFO.Student_ID=SM.Student_Id))
                    WHERE Category<>'OM'  AND Course_Id='{course_ID}'
                    


                    DELETE FROM SeatAllocationMatrix_{printProgramme.GetTablePFix()}
                    WHERE Course_IdAssigned= '{course_ID}' and (ISFinallySelected=0 AND SelectionListNo = {currentList} ) 
                    
                    DELETE FROM FractionSeats";

            SqlCommand sqlCommand = new SqlCommand(Query);
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }

        public int DropSeatCount(PrintProgramme printProgramme)
        {
            var Query = $@" Delete FROM FractionSeats
                            DELETE FROM SeatAllocationMatrix_{printProgramme.GetTablePFix()} WHERE ISFinallySelected=0 
                            DELETE FROM SeatCountAgainstCourse WHERE PrintProgramme={(int)printProgramme}
                            UPDATE ADMCourseMaster SET CurrentSelectionListNo=0,AllowNewSelectionList=1
                            WHERE PrintProgramme={(int)printProgramme}";

            SqlCommand sqlCommand = new SqlCommand(Query);
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }
        public int AssignCollegePreference(Guid College_Id, Guid Student_ID, PrintProgramme printProgramme)
        {
            var Query = $@"UPDATE  SeatAllocationMatrix_{printProgramme.GetTablePFix()}  SET College_IdAssigned='{College_Id}' WHERE Student_Id='{Student_ID}'";

            SqlCommand sqlCommand = new SqlCommand(Query);
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }

        //public List<SelectedStudentInfo> GetAlreadySelectedStudents(string course, string batch, string programme)
        //{
        //    return new MSSQLFactory().GetObjectList<SelectedStudentInfo>(new StudentSQLQueries().GetAlreadySelectedStudents(programme, batch, course));
        //}

        public int GetFreeSeatsCount(string course, string batch, string programme)
        {
            return new MSSQLFactory().ExecuteScalar<int>(new StudentSQLQueries().GetFreeSeatsCount(course, batch, programme));
        }

        public bool SetFractionSeats(Guid course_ID)
        {
            var Query = $@"INSERT INTO FractionSeats
                          VALUES (
                          NEWID(), 'TOTAL', (SELECT CONVERT(INT, (SELECT ROUND((SELECT SUM(NoofSeats % 1) FROM SeatCountAgainstCourse  WHERE Course_Id = '{course_ID}' AND Category <> 'OM' AND(AllocatedSeats > 1 OR NoofSeats < 1)),0))) ),'{course_ID}',0)";

            SqlCommand sqlCommand = new SqlCommand(Query);
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand) > 0;
        }

        public int IsSelected(Guid student_ID, PrintProgramme printProgramme)
        {
            var Query = $@"UPDATE  SeatAllocationMatrix_{printProgramme.GetTablePFix()}  SET ISFinallySelected=1 
                           WHERE Student_Id='{student_ID}'";
            return new MSSQLFactory().ExecuteNonQuery(Query);
        }

        public int CloseSemesterAdm(Guid studentCourse_ID, short admissionSemester)
        {
            var Query = $@"UPDATE dbo.ADMSemesterAdmissionSettings SET Status=0
                           WHERE Course_ID='{studentCourse_ID}' AND Semester={admissionSemester}";
            return new MSSQLFactory().ExecuteNonQuery(Query);
        }

        public IEnumerable<SelectedStudentInfo> GetSelectedStudents(decimal Count, string CategoryClause, Parameters parameters, string inClause)
        {
            return new MSSQLFactory().GetObjectList<SelectedStudentInfo>(new StudentSQLQueries().GetSelectedStudents(Count, CategoryClause, parameters, inClause));
        }
        public SelectedStudentInfo GetStudentsWithNextPrefNEP(string CategoryClause, Guid course_ID, string inClause, int preference, decimal percentage)
        {
            return new MSSQLFactory().GetObject<SelectedStudentInfo>(new StudentSQLQueries().GetStudentsWithNextPrefNEP(CategoryClause, course_ID, inClause, preference, percentage));

        }
        public IEnumerable<SelectedStudentInfo> GetSelectedStudentsNEP(decimal Count, string CategoryClause, Guid _CourseID, string inClause, int preference)
        {
            return new MSSQLFactory().GetObjectList<SelectedStudentInfo>(new StudentSQLQueries().GetSelectedStudentsNEP(Count, CategoryClause, _CourseID, inClause, preference));
        }

        public List<SeatCountAgainstCourse> GetSeatCountWherenotComplete(int batch, Programme Programme)
        {
            return new MSSQLFactory().GetObjectList<SeatCountAgainstCourse>(new StudentSQLQueries().GetSeatCountWherenotComplete(batch, Programme));
        }

        public List<SeatCountAgainstCourse> GetSeatCountWhetherToContinue(int batch, PrintProgramme printProgramme)
        {
            return new MSSQLFactory().GetObjectList<SeatCountAgainstCourse>(new StudentSQLQueries().GetSeatCountWhetherToContinue(batch, printProgramme));
        }
        public List<SeatCountAgainstCourse> GetSeatCountWhetherToContinueCAT(int batch, PrintProgramme printProgramme)
        {
            return new MSSQLFactory().GetObjectList<SeatCountAgainstCourse>(new StudentSQLQueries().GetSeatCountWhetherToContinue(batch, printProgramme));
        }
        public List<SeatCountAgainstCourse> GetSeatCountByCourse(Guid Course_id)
        {
            return new MSSQLFactory().GetObjectList<SeatCountAgainstCourse>(new StudentSQLQueries().GetSeatCountByCourse(Course_id));
        }
        public List<SeatAllocationMatrix> GetSeatAllocation(Guid Student_ID, PrintProgramme printProgramme)
        {
            return new MSSQLFactory().GetObjectList<SeatAllocationMatrix>(new StudentSQLQueries().GetSeatAllocation(Student_ID, printProgramme));
        }
        public IEnumerable<SelectedStudentInfo> GetCategoryStudentsNEduP(String category, PrintProgramme printProgramme, int batch)
        {
            return new MSSQLFactory().GetObjectList<SelectedStudentInfo>(new StudentSQLQueries().GetCategoryStudentsNEduP(category, printProgramme, batch));
        }
        public IEnumerable<SelectedStudentInfo> GetStudentsNEduP(Programme Programme, int batch)
        {
            if (Programme == Programme.PG)
                return new MSSQLFactory().GetObjectList<SelectedStudentInfo>(new StudentSQLQueries().GetStudentsPG(Programme.PG, batch));
            else
                return new MSSQLFactory().GetObjectList<SelectedStudentInfo>(new StudentSQLQueries().GetStudentsNEduP(Programme, batch));
        }

        public IEnumerable<SelectedStudentInfo> GetFractionStudentsNEduP(Guid Course_Id, PrintProgramme printProgramme, int batch)
        {
            return new MSSQLFactory().GetObjectList<SelectedStudentInfo>(new StudentSQLQueries().GetFractionStudentsNEduP(Course_Id, printProgramme, batch));
        }
        public IEnumerable<SelectedStudentInfo> GetSelectedStudentsFORCourse(Guid Course_ID, int CurrentList, PrintProgramme printProgramme, int batch)
        {
            return new MSSQLFactory().GetObjectList<SelectedStudentInfo>(new StudentSQLQueries().GetSelectedStudentsFORCourse(Course_ID, CurrentList, printProgramme, batch));
        }
        public IEnumerable<SelectedStudentInfo> GetSelectedStudentsFORCourse(Guid Course_ID, int CurrentList, PrintProgramme printProgramme, int batch, Guid College_ID)
        {
            return new MSSQLFactory().GetObjectList<SelectedStudentInfo>(new StudentSQLQueries().GetSelectedStudentsFORCourse(Course_ID, CurrentList, printProgramme, batch, College_ID));
        }
        public List<SeatCountAgainstCourse> GetCatHavSeatCountZero(Guid Course_ID)
        {
            return new MSSQLFactory().GetObjectList<SeatCountAgainstCourse>(new StudentSQLQueries().GetCatHavSeatCountZero(Course_ID));
        }
        public IEnumerable<SeatCountAgainstCourse> GetSeatCountAgainstCourse(Guid Course_ID)
        {
            return new MSSQLFactory().GetObjectList<SeatCountAgainstCourse>(new StudentSQLQueries().GetSeatCountAgainstCourse(Course_ID));
        }



        public IEnumerable<SelectedStudentInfo> GetTieList(decimal? totalPoints, string CategoryClause, Parameters parameters, string inClause)
        {
            return new MSSQLFactory().GetObjectList<SelectedStudentInfo>(new StudentSQLQueries().GetTieList(totalPoints, CategoryClause, parameters, inClause));
        }
        public int UpdateGenerateNewListStatus(Guid course_ID, bool status, int listno)
        {
            var Query = $@"UPDATE ADMCourseMaster SET
                        AllowNewSelectionList=@Status,
                        CurrentSelectionListNo=@CurrentSelectionListNo
                        WHERE  Course_ID=@Course_ID";

            SqlCommand sqlCommand = new SqlCommand(Query);
            sqlCommand.Parameters.AddWithValue("@Course_ID", course_ID);
            sqlCommand.Parameters.AddWithValue("@Status", status);
            sqlCommand.Parameters.AddWithValue("@CurrentSelectionListNo", listno);
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }
        public int UpdateNotSelectedStudents(string course, string batch, string programme)
        {
            Programme programmeType;
            Enum.TryParse<Programme>(programme, out programmeType);
            string prgPostFix = new GeneralFunctions().GetProgrammePostFix(programmeType);
            var Query = $@"UPDATE ARGCoursesApplied{prgPostFix} SET
                    StudentSelectionStatus=@StudentSelectionStatus WHERE  Course_ID=@Course_ID
                 AND StudentSelectionStatus={(int)StudentSelectionStatus.Provisional}";

            SqlCommand sqlCommand = new SqlCommand(Query);
            sqlCommand.Parameters.AddWithValue("@StudentSelectionStatus", (int)StudentSelectionStatus.Rejected);
            sqlCommand.Parameters.AddWithValue("@Course_ID", course);
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }
        public int UpdateStudentSelectionStatus(string inClause, StudentSelectionStatus studentSelectionStatus, Parameters parameter, int listno)
        {
            SearchFilter progfilter = parameter.Filters.Where(x => x.Column == "Programme").FirstOrDefault();
            string programme = progfilter.Value;
            SearchFilter courfilter = parameter.Filters.Where(x => x.Column.ToLower() == "course_id").FirstOrDefault();
            string Course_ID = courfilter.Value;
            Programme programmeType;
            Enum.TryParse<Programme>(programme, out programmeType);
            string prgPostFix = new GeneralFunctions().GetProgrammePostFix(programmeType);
            var Query = $@"UPDATE ARGCoursesApplied{prgPostFix} SET
                    StudentSelectionStatus=@StudentSelectionStatus";
            if (listno > 0)
            {
                Query += $@" ,SelectionAgaintListNo={listno}";
            }
            Query += $@" WHERE Student_ID {inClause}
                      AND Course_ID=@Course_ID";
            if (studentSelectionStatus == StudentSelectionStatus.NotSelected)
            {
                Query = Query + $"  AND StudentSelectionStatus!={(int)StudentSelectionStatus.Rejected}";
            }
            SqlCommand sqlCommand = new SqlCommand(Query);
            sqlCommand.Parameters.AddWithValue("@StudentSelectionStatus", studentSelectionStatus);
            sqlCommand.Parameters.AddWithValue("@Course_ID", Course_ID);
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }
        public int UpdateStudentSelectionStatus(UpdateStudentStatus updateStudentStatus)
        {
            string prgPostFix = new GeneralFunctions().GetProgrammePostFix(updateStudentStatus.Programme);
            var Query = $@"UPDATE CourseApplied
                        SET CourseApplied.StudentSelectionStatus=@StudentSelectionStatus
                        FROM ARGPersonalInformation{prgPostFix} PINFO
                        JOIN ARGCoursesApplied{prgPostFix} CourseApplied
                        ON CourseApplied.Student_ID = PINFO.Student_ID AND Batch=@Batch AND EntranceRollNo IN ({updateStudentStatus.RollNos})
                        AND Course_ID=@Course_ID";

            SqlCommand sqlCommand = new SqlCommand(Query);
            sqlCommand.Parameters.AddWithValue("@Batch", updateStudentStatus.Batch);
            sqlCommand.Parameters.AddWithValue("@StudentSelectionStatus", updateStudentStatus.StudentSelectionStatus);
            sqlCommand.Parameters.AddWithValue("@Course_ID", updateStudentStatus.Course_ID);
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }
        public List<StudentResult> GetStudentResult(Guid student_Id, PrintProgramme printProgramme, Semester semester, string join, int Examformno, Guid Subject_ID, bool checkresultdeclared)
        {
            return new MSSQLFactory().GetObjectList<StudentResult>(new StudentSQLQueries().GetStudentResult(student_Id, printProgramme, semester, join, Examformno, Subject_ID, checkresultdeclared));
        }
        public List<AdmissionForm> GetStudentAdmissionFormHistory(Guid student_Id, PrintProgramme printProgramme)
        {
            return new MSSQLFactory().GetObjectList<AdmissionForm>(new StudentSQLQueries().GetStudentAdmissionFormHistory(student_Id, printProgramme));
        }

        public List<ADMSubjectMaster> GetStudentSubjectAwards(string registrationNumber, PrintProgramme printProgramme)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            return new MSSQLFactory().GetObjectList<ADMSubjectMaster>(new StudentSQLQueries().GetStudentSubjectAwards(registrationNumber, printProgramme));
        }

        #region addionalSubject
        public List<StudentAdditionalSubject> GetStudentAdditionalSubjects(Guid student_ID, short semester, PrintProgramme printProgramme)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            List<StudentAdditionalSubject> subjects = new MSSQLFactory().GetObjectList<StudentAdditionalSubject>(new StudentSQLQueries().GetStudentAdditionalSubjects(student_ID, semester, printProgramme));
            if (subjects != null)
            {
                foreach (var subject in subjects)
                {
                    subject.Subject = new MSSQLFactory().GetObject<ADMSubjectMaster>(new StudentSQLQueries().GetSubject(subject.Subject_ID));
                }
            }
            return subjects;
        }
        public List<StudentAdditionalSubject> GetStudentAdditionalSubjectsDetails(Guid student_ID, short semester, PrintProgramme printProgramme)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            List<StudentAdditionalSubject> subjects = new MSSQLFactory().GetObjectList<StudentAdditionalSubject>(new StudentSQLQueries().GetStudentAdditionalSubjectsDetails(student_ID, semester, printProgramme));
            if (subjects != null)
            {
                foreach (var subject in subjects)
                {
                    subject.Subject = new MSSQLFactory().GetObject<ADMSubjectMaster>(new StudentSQLQueries().GetSubject(subject.Subject_ID));
                }
            }
            return subjects;
        }
        public int Save(StudentAdditionalSubject studentAdditionalSubject, PrintProgramme printProgramme)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            return new MSSQLFactory().InsertRecord(studentAdditionalSubject, $"ARGStudentAdditionalSubjects_{printProgramme.ToString()}");
        }


        public int Update(StudentAdditionalSubject studentAdditionalSubject, PrintProgramme printProgramme)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            List<string> ignoreList = new List<string>() { "AdditionalSubject_ID", "CreatedOn", "CreatedBy" };
            var sqlCommand = new MSSQLFactory().UpdateRecord(studentAdditionalSubject, ignoreList, ignoreList, $"ARGStudentAdditionalSubjects_{printProgramme.ToString()}");
            sqlCommand.CommandText += " WHERE AdditionalSubject_ID =@AdditionalSubject_ID";
            sqlCommand.Parameters.AddWithValue("@AdditionalSubject_ID", studentAdditionalSubject.AdditionalSubject_ID);
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }
        public int DeleteStudentAdditionalSubject(Guid Student_ID, short Semester, PrintProgramme printProgramme)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            var Query = $"DELETE FROM ARGStudentAdditionalSubjects_{printProgramme.ToString()} WHERE Student_ID='{Student_ID}' AND Semester={Semester}";
            return new MSSQLFactory().ExecuteNonQuery(Query);
        }

        public int DeleteStudentAdditionalSubject(Guid Student_ID, short Semester, Guid Subject_ID, PrintProgramme printProgramme)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            var Query = $"DELETE FROM ARGStudentAdditionalSubjects_{printProgramme.ToString()} WHERE Student_ID='{Student_ID}' AND Semester={Semester} AND Subject_ID='{Subject_ID}'";
            return new MSSQLFactory().ExecuteNonQuery(Query);
        }
        #endregion
        public ARGPersonalInformation GetStudent(Guid student_ID, PrintProgramme printProgramme)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            return new MSSQLFactory().GetObject<ARGPersonalInformation>(new RegistrationSQLQueries().GetStudentByID(student_ID, printProgramme));
        }
        public PersonalInformationCompact GetStudent(string CUSRegistrationNo, DateTime DOB, PrintProgramme printProgramme)
        {
            return new MSSQLFactory().GetObject<PersonalInformationCompact>(new RegistrationSQLQueries().GetStudentByRegNOandDOB(CUSRegistrationNo, DOB, printProgramme));
        }
        public ARGPersonalInformation GetStudent(string formNumber, PrintProgramme printProgramme)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            return new MSSQLFactory().GetObject<ARGPersonalInformation>(new StudentSQLQueries().GetStudent(formNumber, printProgramme));
        }

        public List<PersonalInformationCompact> GetStudent(string BoardRegistrationNo)
        {
            var Query = $@"SELECT STDINFO.Student_ID,STDINFO.BoardRegistrationNo,STDINFO.FullName,STDINFO.DOB,STDINFO.AcceptCollege_ID,STDINFO.CUSRegistrationNo 
                        ,ADMCombinationMaster.Course_ID,CourseFullName,CollegeFullName,STDINFO.FormStatus,1 AS PrintProgramme
                        FROM 
                        ARGPersonalInformation_UG STDINFO 
                        LEFT JOIN ARGSelectedCombination_UG COMB ON COMB.Student_ID = STDINFO.Student_ID AND COMB.Semester=1
                        LEFT JOIN ADMCombinationMaster ON ADMCombinationMaster.Combination_ID = COMB.Combination_ID
                        LEFT JOIN ADMCourseMaster ON  ADMCourseMaster.Course_ID = ADMCombinationMaster.Course_ID
                        LEFT JOIN ADMCollegeMaster ON ADMCollegeMaster.College_ID = ADMCombinationMaster.College_ID
                        WHERE STDINFO.BoardRegistrationNo='{BoardRegistrationNo}'
                        UNION ALL	
                        SELECT STDINFO.Student_ID,STDINFO.BoardRegistrationNo,STDINFO.FullName,STDINFO.DOB,STDINFO.AcceptCollege_ID,STDINFO.CUSRegistrationNo 
                        ,ADMCombinationMaster.Course_ID,CourseFullName,CollegeFullName,STDINFO.FormStatus,3 AS PrintProgramme
                        FROM 
                        ARGPersonalInformation_IH STDINFO 
                        LEFT JOIN ARGSelectedCombination_IH COMB ON COMB.Student_ID = STDINFO.Student_ID AND COMB.Semester=1
                        LEFT JOIN ADMCombinationMaster ON ADMCombinationMaster.Combination_ID = COMB.Combination_ID
                        LEFT JOIN ADMCourseMaster ON  ADMCourseMaster.Course_ID = ADMCombinationMaster.Course_ID
                        LEFT JOIN ADMCollegeMaster ON ADMCollegeMaster.College_ID = ADMCombinationMaster.College_ID
                        WHERE STDINFO.BoardRegistrationNo='{BoardRegistrationNo}'";
            return new MSSQLFactory().GetObjectList<PersonalInformationCompact>(Query);
        }

        public PersonalInformationCompact GetStudentC(Guid Student_Id, PrintProgramme PP)
        {
            var Query = $@"SELECT STDINFO.*,Duration
                        ,ADMCombinationMaster.Course_ID,CourseFullName,CollegeFullName,{(int)PP} As PrintProgramme
                        FROM 
                        ARGPersonalInformation_{PP.GetTablePFix()} STDINFO 
                        LEFT JOIN ARGSelectedCombination_{PP.GetTablePFix()} COMB ON COMB.Student_ID = STDINFO.Student_ID AND COMB.Semester=1
                        LEFT JOIN ADMCombinationMaster ON ADMCombinationMaster.Combination_ID = COMB.Combination_ID
                        LEFT JOIN ADMCourseMaster ON  ADMCourseMaster.Course_ID = ADMCombinationMaster.Course_ID
                        LEFT JOIN ADMCollegeMaster ON ADMCollegeMaster.College_ID = ADMCombinationMaster.College_ID
                        WHERE STDINFO.Student_Id='{Student_Id}'
                       ";
            return new MSSQLFactory().GetObject<PersonalInformationCompact>(Query);
        }

        public short GetStudentSemesterBatch(Guid student_ID, PrintProgramme tableSuffix, short admissionSemester)
        {
            return new MSSQLFactory().ExecuteScalar<short>($@"SELECT ISNULL(s.SemesterBatch,p.Batch) FROM dbo.ARGSelectedCombination_{tableSuffix} s
                                                        LEFT JOIN dbo.ARGPersonalInformation_{tableSuffix} p ON s.Student_ID = p.Student_ID
                                                         WHERE s.Student_ID = '{student_ID}' AND s.Semester = {admissionSemester}");
        }

        public UpdateCertificates GetCertificateDetails(Guid student_ID)
        {
            string Query = $@"SELECT Certificate_ID,
                           CertificateUrl AS OldMarksCardPath FROM dbo.Certificate WHERE Student_ID ='{student_ID}' AND CertificateType={(short)CertificateType.MarksCard}";
            return new MSSQLFactory().GetObject<UpdateCertificates>(Query);
        }

        public List<StudentAdmissionCertificate> GetCertificateDetails(string formNo, PrintProgramme printProgramme)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            var Query = $@"SELECT p.StudentFormNo,c.CertificateUrl,c.CertificateType,a.Mobile,p.FullName FROM dbo.ARGPersonalInformation_{printProgramme.ToString()} p
                            JOIN dbo.ARGStudentAddress_{printProgramme.ToString()} a ON a.Student_ID = p.Student_ID
                            LEFT JOIN dbo.Certificate c ON c.Student_ID = p.Student_ID
                            WHERE (p.StudentFormNo=@StudentFormNo OR CAST(p.EntranceRollNo AS VARCHAR(MAX))=@StudentFormNo) AND p.FormStatus<>1";

            SqlCommand sqlCommand = new SqlCommand(Query);
            sqlCommand.Parameters.AddWithValue("@StudentFormNo", formNo);
            return new MSSQLFactory().GetObjectList<StudentAdmissionCertificate>(sqlCommand);
        }

        public int GetCurrentSemester(Guid user_ID, PrintProgramme printProgramme)
        {
            var tableName = new GeneralFunctions().GetTableName(printProgramme, Module.PersonalInformation);
            return new MSSQLFactory().ExecuteScalar<int>($"SELECT CURRENTSEMESTERORYEAR FROM {tableName} WHERE STUDENT_ID={user_ID}");
        }



        #region CollegePreference

        public int UpdateSelectionStatus(Guid student_ID, Guid course_id, StudentSelectionStatus status)
        {
            return new MSSQLFactory().ExecuteNonQuery($"Update ARGCoursesApplied_IH Set StudentSelectionStatus={(short)status} where student_id='{student_ID}' and course_id='{course_id}'");
        }

        public int UpdateSelectionStatus(Guid student_ID)
        {
            return new MSSQLFactory().ExecuteNonQuery($"Update ARGCoursesApplied_IH Set StudentSelectionStatus={(short)StudentSelectionStatus.NotSelected} where student_id='{student_ID}'");
        }
        public int DeleteCollegePreference(Guid Student_ID)
        {
            return new MSSQLFactory().ExecuteNonQuery($"Delete from  StudentCollegePreference where student_id='{Student_ID}'");
        }



        public int SaveCollegePreference(StudentCollegePreference model)
        {
            return new MSSQLFactory().InsertRecord(model, "StudentCollegePreference");
        }
        public int UpdateCollegePreference(StudentCollegePreference model)
        {
            List<string> ignoreList = new List<string>() { "Preference_ID", "CreatedOn", "CreatedBy" };
            var sqlCommand = new MSSQLFactory().UpdateRecord(model, ignoreList, ignoreList, $"StudentCollegePreference");
            sqlCommand.CommandText = sqlCommand.CommandText + $" WHERE Preference_ID ='{model.Preference_ID}'";
            sqlCommand.Parameters.AddWithValue("@Preference_ID", model.Preference_ID);
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }


        public int UpdateCollegePreference(Guid College_ID, Guid Student_ID, Guid Course_ID, bool IsAllowed)
        {
            var records = new MSSQLFactory().ExecuteNonQuery($"Update StudentCollegePreference Set IsAllowed='0' WHERE Student_ID ='{Student_ID}'");
            return new MSSQLFactory().ExecuteNonQuery($"Update StudentCollegePreference Set IsAllowed='{(IsAllowed == true ? 1 : 0)}' WHERE Student_ID ='{Student_ID}' AND College_ID='{College_ID}' AND Course_ID='{Course_ID}'");

            //List<string> ignoreList = null;// new List<string>() { "Student_ID", "CreatedOn", "CreatedBy" };
            //var sqlCommand = new MSSQLFactory().UpdateRecord(model, ignoreList, ignoreList, $"StudentCollegePreference");
            //sqlCommand.CommandText = sqlCommand.CommandText + $" WHERE Student_ID ='{model.Student_ID}' AND College_ID='{model.College_ID}' AND Course_ID='{model.Course_ID}'";
            //sqlCommand.Parameters.AddWithValue("@Student_ID", model.Student_ID);
            //return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }
        public int ReplaceStudent_ID(Guid student_ID, Guid NewStudent_ID)
        {
            return new MSSQLFactory().ExecuteNonQuery($"Update StudentCollegePreference Set Student_ID='{NewStudent_ID}',OldStudent_ID='{student_ID}' where student_id='{student_ID}'");
        }


        #endregion


        public bool HasAppliedForCourse(PrintProgramme printProgramme, Guid student_ID, Guid course_ID)
        {
            var printProg = new GeneralFunctions().MappingTable(printProgramme);
            var Query = $"SELECT 1 FROM ARGCoursesApplied_{printProg.ToString()} WHERE Student_ID = '{student_ID}' AND Course_ID = '{course_ID}'";
            return new MSSQLFactory().ExecuteScalar<int>(Query) > 0;

        }
        public bool HasBasicQMed(PrintProgramme printProgramme, Guid student_ID, String DecidingQ)
        {
            var printProg = new GeneralFunctions().MappingTable(printProgramme);
            var Query = $@"SELECT 1 FROM ARGStudentPreviousQualifications_{printProg.ToString()}  WHERE  TRIM(LOWER(ExamName))=TRIM(LOWER('{DecidingQ}'))
                                   AND (TRIM(LOWER(REPLACE(REPLACE(stream, ' ', ''), '.', ''))) LIKE '%med%' OR TRIM(LOWER(REPLACE(REPLACE(stream, ' ', ''), '.', ''))) LIKE '%bsc%' OR TRIM(LOWER(REPLACE(REPLACE(stream, ' ', ''), '.', ''))) LIKE '%sci%') AND Student_ID = '{student_ID}'";
            return new MSSQLFactory().ExecuteScalar<int>(Query) > 0;

        }

        public List<ADMSubjectMaster> GetStudentCourseWithSubjects(Guid student_ID, int semester)
        {
            return new MSSQLFactory().GetObjectList<ADMSubjectMaster>($@"SELECT CourseFullName,SubjectFullName,Subject_ID,Course_ID,SubjectType FROM VWStudentWithDetail 
                                                                            WHERE Student_ID ='{student_ID}' AND Semester={semester}");
        }

        public SelectListItem GetParticularStudentForLogin(Guid College_ID, Guid Course_ID, int? Batch)
        {
            var batchClause = string.Empty;
            if (Batch != null)
            {
                batchClause = $"AND Batch = {Batch}";
            }

            return new MSSQLFactory().GetObject<SelectListItem>($@"SELECT TOP 1 CUSRegistrationNo + '|' + CAST(PrintProgramme AS VARCHAR(1))[Text],CONVERT(varchar(11),DOB,103)  [Value] FROM VWStudentCourse 
                                                                        WHERE CUSRegistrationNo IS NOT NULL AND AcceptCollege_ID = '{College_ID}' AND Course_ID = '{Course_ID}' {batchClause}
                                                                        ORDER BY CollegeFullName,CourseFullName");
        }

        public List<StudentLoginInfo> GetStudentsForLogin(PrintProgramme? programme)
        {
            string WhereClause = programme != null ? $"AND PrintProgramme ={(short)programme}" : "";

            return new MSSQLFactory().GetObjectList<StudentLoginInfo>($@"SELECT  DISTINCT CourseFullName,Course_ID,CollegeFullName,AcceptCollege_ID College_ID
                                                                            FROM VWStudentCourse WHERE CUSRegistrationNo IS NOT NULL {WhereClause}
                                                                            ORDER BY CollegeFullName,CourseFullName");
        }

        public MigrationPI GetStudentMigration(string formNumber, DateTime DOB, PrintProgramme printProgramme)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            MigrationPI Pinfo = new MSSQLFactory().GetObject<MigrationPI>(new StudentSQLQueries().GetStudent(formNumber, DOB, printProgramme));

            return Pinfo;

        }
        public MigrationPI GetStudentMigration(Guid student_ID, PrintProgramme printProgramme, string MigrationType)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            MigrationPI Pinfo = new MSSQLFactory().GetObject<MigrationPI>(new RegistrationSQLQueries().GetStudentByID(student_ID, printProgramme));
            if (Pinfo != null)
            {

                MigrationPI migration = new MSSQLFactory().GetObject<MigrationPI>(new MigrationSQLQueries().GetMigration(student_ID, MigrationType, printProgramme));
                if (migration != null)
                {
                    Guid College_ID;
                    Pinfo.NewCollegeName = migration.NewCollege;
                    Pinfo.Remarks = migration.Remarks;
                    Pinfo.FormType = migration.FormType;
                    Pinfo.TransactionNo = migration.TransactionNo;
                    Pinfo.TransactionDate = migration.TransactionDate;
                    Pinfo.NewSubjects = migration.NewSubjects;
                    Pinfo.Email = migration.Email;
                    Pinfo.Mobile = migration.Mobile;
                    Pinfo.SerialNo = migration.SerialNo;
                    Pinfo.FormStatus = migration.FormStatus;
                    Pinfo.TotalFeeM = migration.TotalFeeM;
                    Pinfo.Programme = printProgramme;
                    if (Guid.TryParse(migration.NewCollege, out College_ID))
                    {
                        Pinfo.NewCollegeName = new CollegeDB().GetItem(College_ID).CollegeFullName;
                    }
                    return Pinfo;
                }

            }

            return null;
        }
        public int DeleteStudentMigration(Guid student_ID, PrintProgramme printProgramme, string MigrationType)
        {
            return new RegistrationSQLQueries().DeleteStudentByID(student_ID, printProgramme);
        }


        public List<ADMSubjectMaster> GetsubjectsforExamForm(Guid StudentExamForm_ID, PrintProgramme printProgramme)
        {
            SqlCommand command = new SqlCommand();
            command.Parameters.AddWithValue("@StudentExamForm_ID", StudentExamForm_ID);
            command.CommandText = $@"SELECT Subject_ID FROM dbo.ARGStudentExamForm_{printProgramme.ToString()} pinfo
                                        JOIN dbo.ARGStudentReExamForm
                                        ON ARGStudentReExamForm.StudentExamForm_ID = pinfo.StudentExamForm_ID
                                        WHERE ARGStudentReExamForm.StudentExamForm_ID=@StudentExamForm_ID";
            return new MSSQLFactory().GetObjectList<ADMSubjectMaster>(command);
        }

        public ARGPersonalInformation GetStudentBasicSubjectInfo(string formNumber, PrintProgramme printProgramme)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            SqlCommand command = new SqlCommand();
            command.Parameters.AddWithValue("@FormNumber", formNumber.Trim());
            command.CommandText = $@"SELECT ARGPersonalInformation_{printProgramme.ToString()}.*,ADMCollegeMaster.CollegeFullName AS AcceptCollegeID 
                                    FROM ARGPersonalInformation_{printProgramme.ToString()}
                                    LEFT JOIN ADMCollegeMaster ON ADMCollegeMaster.College_ID = ARGPersonalInformation_{printProgramme.ToString()}.AcceptCollege_ID  
                                    WHERE (ARGPersonalInformation_{printProgramme.ToString()}.CUSRegistrationNo = @FormNumber 
                                            OR ARGPersonalInformation_{printProgramme.ToString()}.BoardRegistrationNo = @FormNumber 
                                            OR ARGPersonalInformation_{printProgramme.ToString()}.StudentFormNo = @FormNumber)";
            return new MSSQLFactory().GetObject<ARGPersonalInformation>(command);
        }

        public ARGStudentAddress GetStudentAddress(Guid student_ID, PrintProgramme printProgramme)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            return new MSSQLFactory().GetObject<ARGStudentAddress>(new StudentSQLQueries().GetStudentAddress(student_ID, printProgramme));
        }

        public List<PersonalInformationCompact> GetStudentListCompact(Parameters Parameter, PrintProgramme printProgramme)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            var Query = new StudentSQLQueries().StudentListCompact(printProgramme);
            FilterHelper helper = new GeneralFunctions().GetWhereClause<ARGPersonalInformation>(Parameter.Filters);
            helper.Command.CommandText = Query + helper.WhereClause + new GeneralFunctions().GetPagedQuery(Query, Parameter);
            return new MSSQLFactory().GetObjectList<PersonalInformationCompact>(helper.Command);
        }



        public List<ARGStudentPreviousQualifications> GetStudentAcademicDetails(Guid student_ID, PrintProgramme printProgramme)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            return new MSSQLFactory().GetObjectList<ARGStudentPreviousQualifications>(new StudentSQLQueries().GetStudentAcademicDetails(student_ID, printProgramme));
        }

        public List<ARGCoursesApplied> GetStudentCoursesApplied(Guid student_ID, PrintProgramme printProgramme)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            string Query = $@"SELECT CA.*,PrintProgramme,Programme,ADMCourseMaster.CourseFullName AS CourseName,ADMCourseMaster.CourseCode,Duration
                            FROM ARGCoursesApplied_{printProgramme.ToString()} CA
                            INNER JOIN ADMCourseMaster ON CA.Course_ID = ADMCourseMaster.Course_ID
                            WHERE Student_ID = '{student_ID}'";
            return new MSSQLFactory().GetObjectList<ARGCoursesApplied>(Query);
            //return new MSSQLFactory().GetObjectList<ARGCoursesApplied>(new StudentSQLQueries().GetStudentCoursesApplied(student_ID, printProgramme));
        }
        public List<SeatCountAgainstCourse> GetCoursesApplied(Guid student_ID, PrintProgramme printProgramme)
        {
            string Query = $@"SELECT *
                            FROM ARGCoursesApplied_{printProgramme.GetTablePFix()} CA
                            left JOIN SeatCountAgainstCourse SC ON CA.Course_ID = SC.Course_ID
                            WHERE Student_ID ='{student_ID}' AND ISEligible=1 order by Preference asc,CA.Course_ID,NoofSeats desc";

            return new MSSQLFactory().GetObjectList<SeatCountAgainstCourse>(Query);
        }
        public List<SeatCountAgainstCourse> GetCoursesAppliedWithColg(Guid student_ID, PrintProgramme printProgramme)
        {
            string Query = $@" SELECT *
                            FROM ARGCoursesApplied_ug CA
                            INNER JOIN NEPCollegePreferences ON  NEPCollegePreferences.Student_ID = CA.Student_ID
                            LEFT JOIN SeatCountAgainstCourse ON SeatCountAgainstCourse.Course_Id = CA.Course_ID AND SeatCountAgainstCourse.College_Id = NEPCollegePreferences.College_ID
                            WHERE CA.Student_ID ='{student_ID}' AND ISEligible=1  AND NoofSeats >0  order by Preference asc,PreferenceNo asc,NoofSeats desc";
            return new MSSQLFactory().GetObjectList<SeatCountAgainstCourse>(Query);
        }

        public List<SeatCountAgainstCourse> GetCoursesAppliedWithFractionCount(Guid student_ID, string category, PrintProgramme printProgramme)
        {
            string Query = $@"SELECT *
                            FROM ARGCoursesApplied_{printProgramme.GetTablePFix()} CA
                             JOIN FractionSeats ON FractionSeats.Course_ID = CA.Course_Id AND FractionSeats.Category = 'TOTAL'
                            JOIN SeatCountAgainstCourse ON SeatCountAgainstCourse.Course_Id = CA.Course_ID AND SeatCountAgainstCourse.Category = '{category}'
                            WHERE Student_ID ='{student_ID}'  AND ISEligible=1 order by Preference asc,CA.Course_ID desc";
            return new MSSQLFactory().GetObjectList<SeatCountAgainstCourse>(Query);
        }
        public List<SeatCountAgainstCourse> SeatCountAgainstCourse(Programme Programme)
        {
            string Query = $@"SELECT *  FROM SeatCountAgainstCourse SC WHERE PrintProgramme={(int)Programme} ";
            return new MSSQLFactory().GetObjectList<SeatCountAgainstCourse>(Query);
        }
        public List<ARGCoursesApplied> GetStudentCoursesApplied(List<Guid> course_Ids)
        {
            return new MSSQLFactory().GetObjectList<ARGCoursesApplied>(new RegistrationSQLQueries().GetStudentCoursesApplied(course_Ids));
        }
        public List<ARGSelectedCombination> GetSelectedCombinations(Guid student_ID, PrintProgramme printProgramme)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            var Query = $@"SELECT Combination.*,CombinationCode AS CombinationID,ADMCombinationMaster.College_ID,ADMCourseMaster.PrintProgramme,
                            ADMCombinationMaster.Course_ID,ADMCollegeMaster.CollegeFullName AS CollegeID,ADMCourseMaster.CourseFullName AS CourseID,ADMCourseMaster.CourseCode ,CombinationSubjects
                            FROM ARGSelectedCombination_{printProgramme.ToString()} Combination
                            JOIN ADMCombinationMaster ON ADMCombinationMaster.Combination_ID = Combination.Combination_ID 
                            JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = ADMCombinationMaster.Course_ID
                            JOIN ADMCollegeMaster ON ADMCollegeMaster.College_ID = ADMCombinationMaster.College_ID
                            WHERE Combination.Student_ID='{student_ID}' ORDER BY Combination.Semester";
            return new MSSQLFactory().GetObjectList<ARGSelectedCombination>(Query);
        }

        public decimal? GetCGPAOnly(Guid Student_ID)
        {
            return new MSSQLFactory().ExecuteScalar<decimal?>($"SELECT CGPA FROM dbo.MSCGPA WHERE Student_ID='{Student_ID}';");
        }
        public DateTime? GetDateOfSemesterPassing(Guid student_ID, PrintProgramme printProgramme, short semester)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            return new MSSQLFactory().ExecuteScalar<DateTime?>($@"SELECT MAX(Dated) FROM dbo.{printProgramme.ToString()}_Semester{semester} s
                                                            LEFT JOIN dbo.ResultNotification ON ResultNotification.ResultNotification_ID = s.ResultNotification_ID
                                                            WHERE Student_ID = '{student_ID}';");
        }

        public ARGSelectedCombination GetSelectedCombination(Guid student_ID, PrintProgramme printProgramme, short semester)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            return new MSSQLFactory().GetObject<ARGSelectedCombination>(new StudentSQLQueries().GetSelectedCombinations(student_ID, printProgramme, semester));
        }
        public ARGSelectedCombination GetSelectedCombinations(Guid student_ID, short Semester, PrintProgramme printProgramme)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            var COMB = new MSSQLFactory().GetObject<ARGSelectedCombination>(new StudentSQLQueries().GetSelectedCombinations(student_ID, Semester, printProgramme));
            if (COMB != null)
                COMB.Subjects = new CombinationDB().GetCombinationSubjects(COMB.Combination_ID);
            return COMB;
        }

        public List<ADMSubjectMaster> GetStudentSubjects(Guid student_ID, PrintProgramme printProgramme, Semester semester)
        {
            return new MSSQLFactory().GetObjectList<ADMSubjectMaster>(new StudentSQLQueries().GetStudentSubjects(student_ID, printProgramme, semester));
        }

        //public List<ADMSubjectMaster> GetStudentSubjectsByCombination(string combinationCode)
        //{
        //    List<ADMSubjectMaster> subjectList = null;
        //    var subjects = new MSSQLFactory().ExecuteScalar<string>(new StudentSQLQueries().GetStudentSubjectsFromCombination(combinationCode));

        //    if (subjects != null || subjects != "")
        //    {
        //        subjectList = new MSSQLFactory().GetObjectList<ADMSubjectMaster>(new StudentSQLQueries().GetStudentSubjectsByCombination(subjects));
        //        subjectList.ForEach(i => i.Subject_ID = subjects);
        //    }
        //    return subjectList;
        //}

        public List<StudentRegister> GetAllStudentsRegister(PrintProgramme programme, Parameters parameter)
        {
            SqlCommand cmd = new StudentSQLQueries().GetAllStudentsRegister(programme, parameter);
            return new MSSQLFactory().GetObjectList<StudentRegister>(cmd);
        }
        public SemesterCombination GetStudentExamDetails(PrintProgramme programme, int Semester, Guid Student_ID)
        {
            programme = new GeneralFunctions().MappingTable(programme);
            string Sql = $@"SELECT *,SGPA FROM (SELECT ROW_NUMBER() OVER (PARTITION BY Student_ID ORDER BY Year DESC, IsRegular ASC) Sno,Student_ID,
                                   Year AS ExamYear,
                                   ExamRollNumber as ExamRollNo
                            FROM dbo.ARGStudentExamForm_{programme.ToString()}
                            WHERE Semester = {Semester}
                                  AND Status = 4
	                              AND Student_ID='{Student_ID}'
	                              ) s
                            LEFT JOIN dbo.MSSGPA ON MSSGPA.Student_ID=s.Student_ID AND MSSGPA.Semester={Semester}
                            WHERE s.Sno=1 AND MSSGPA.Semester={Semester}";
            return new MSSQLFactory().GetObject<SemesterCombination>(Sql);
        }

        #region Library
        public List<LibraryForm> GetAllStudentsLibrary(PrintProgramme programme, Parameters parameter, bool IsRR)
        {
            SqlCommand cmd = IsRR ? new StudentSQLQueries().GetAllStudentsRR(programme, parameter) : new StudentSQLQueries().GetAllStudentsLibrary(programme, parameter);
            return new MSSQLFactory().GetObjectList<LibraryForm>(cmd);
        }

        public List<AttendanceSheetForm> GetAllStudentsAttendanceSheet(PrintProgramme programme, Guid Course_ID, int Batch, int Semester, string ExamRollNo, Guid College_ID, Guid? Center_ID)
        {
            return new MSSQLFactory().GetObjectList<AttendanceSheetForm>(new StudentSQLQueries().GetAllStudentsAttendanceSheet(programme, Course_ID, Batch, Semester, ExamRollNo, College_ID, Center_ID));
        }
        public ARGStudentAddress GetStudentAddress(PrintProgramme programme, Guid Student_ID)
        {
            return new MSSQLFactory().GetObject<ARGStudentAddress>(new StudentSQLQueries().GetStudentAddress(programme, Student_ID));
        }

        public List<ARGStudentReExamForm> GetStudentFailedAdditionalSubjects(Guid student_ID, short semester, PrintProgramme printProgramme)
        {
            return new MSSQLFactory().GetObjectList<ARGStudentReExamForm>(new StudentSQLQueries().GetStudentFailedAdditionalSubjects(student_ID, semester, printProgramme));
        }

        public ADMCombinationMaster GetSelectedCombination(Guid Combination_ID)
        {
            return new MSSQLFactory().GetObject<ADMCombinationMaster>(new StudentSQLQueries().GetStudentSubjects(Combination_ID));
        }
        public ADMSubjectMaster GetSubject(Guid Subject_ID)
        {
            return new MSSQLFactory().GetObject<ADMSubjectMaster>(new StudentSQLQueries().GetSubject(Subject_ID));
        }

        public int Update(ARGPersonalInformation studentInfo, PrintProgramme printProgramme)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            List<string> ignoreList = new List<string>() { "Student_ID", "CreatedOn", "CreatedBy" };
            if (string.IsNullOrEmpty(studentInfo.MothersName))
            { ignoreList.Add("MothersName"); }
            if (studentInfo.FormStatus == FormStatus.CancelRegistration && studentInfo.IsPassout)
            {
                studentInfo.IsPassout = false;
            }

            var sqlCommand = new MSSQLFactory().UpdateRecord(studentInfo, ignoreList, ignoreList, $"ARGPersonalInformation_{printProgramme.ToString()}");
            sqlCommand.CommandText = sqlCommand.CommandText + " WHERE Student_ID =@Student_ID ";
            sqlCommand.Parameters.AddWithValue("@Student_ID", studentInfo.Student_ID);
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }
        public int UpdateStudentCombinationInfo(ARGPersonalInformation studentInfo, PrintProgramme printProgramme)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            var Query = $@"UPDATE ARGPersonalInformation_{printProgramme} SET
                    FormStatus=@FormStatus, AcceptCollege_ID=@AcceptCollege_ID,                    AcceptedBy_ID=@AcceptedBy_ID,                    CurrentSemesterOrYear=@CurrentSemesterOrYear,
                    ClassRollNo=@ClassRollNo,                    UpdatedBy=@UpdatedBy,UpdatedOn=@UpdatedOn WHERE Student_ID=@Student_ID";
            SqlCommand sqlCommand = new SqlCommand(Query);
            sqlCommand.Parameters.Add(MSSQLFactory.GetSqlParameter("FormStatus", (short)studentInfo.FormStatus));
            sqlCommand.Parameters.Add(MSSQLFactory.GetSqlParameter("AcceptedBy_ID", studentInfo.AcceptedBy_ID));
            sqlCommand.Parameters.Add(MSSQLFactory.GetSqlParameter("AcceptCollege_ID", studentInfo.AcceptCollege_ID));
            sqlCommand.Parameters.Add(MSSQLFactory.GetSqlParameter("CurrentSemesterOrYear", studentInfo.CurrentSemesterOrYear));
            sqlCommand.Parameters.Add(MSSQLFactory.GetSqlParameter("ClassRollNo", studentInfo.ClassRollNo));
            sqlCommand.Parameters.Add(MSSQLFactory.GetSqlParameter("UpdatedBy", studentInfo.UpdatedBy));
            sqlCommand.Parameters.Add(MSSQLFactory.GetSqlParameter("UpdatedOn", studentInfo.UpdatedOn));
            sqlCommand.Parameters.Add(MSSQLFactory.GetSqlParameter("Student_ID", studentInfo.Student_ID));
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
            //var Query = $@"UPDATE ARGPersonalInformation_{printProgramme} SET
            //        FormStatus={(short)studentInfo.FormStatus},
            //        AcceptCollege_ID='{studentInfo.AcceptCollege_ID}',
            //        AcceptedBy_ID='{AppUserHelper.User_ID}',
            //        CurrentSemesterOrYear={studentInfo.CurrentSemesterOrYear},ClassRollNo='{studentInfo.ClassRollNo}',
            //        UpdatedBy='{studentInfo.UpdatedBy}',UpdatedOn='{studentInfo.UpdatedOn}' WHERE Student_ID='{studentInfo.Student_ID}'";
            //return new MSSQLFactory().ExecuteNonQuery(Query);
        }

        public PersonalInformationCompact GetStudentCompactByRegistrationNo(string CUSRegistrationNo)
        {
            var Query = $@"SELECT * FROM (
                            SELECT [Student_ID],'{PrintProgramme.UG}' AS PrintProgramme,[BoardRegistrationNo],[CUSRegistrationNo],[FullName],[FathersName],[Gender],[DOB],[Category],[AcceptCollege_ID],[CurrentSemesterOrYear],[Batch],[StudentFormNo] FROM [ARGPersonalInformation_UG] WHERE IsPassout=0 AND FormStatus NOT IN('{(short)FormStatus.Rejected}','{(short)FormStatus.Disqualified}','{(short)FormStatus.Cancelled}','{(short)FormStatus.CancelRegistration}') AND (CUSRegistrationNo=@CUSRegistrationNO OR StudentFormNo=@CUSRegistrationNO)
                            UNION 
                            SELECT [Student_ID],'{PrintProgramme.IH}' AS PrintProgramme,[BoardRegistrationNo],[CUSRegistrationNo],[FullName],[FathersName],[Gender],[DOB],[Category],[AcceptCollege_ID],[CurrentSemesterOrYear],[Batch],[StudentFormNo] FROM [ARGPersonalInformation_IH] WHERE IsPassout=0 AND FormStatus NOT IN('{(short)FormStatus.Rejected}','{(short)FormStatus.Disqualified}','{(short)FormStatus.Cancelled}','{(short)FormStatus.CancelRegistration}') AND (CUSRegistrationNo=@CUSRegistrationNO OR StudentFormNo=@CUSRegistrationNO)
                            UNION 
                            SELECT [Student_ID],'{PrintProgramme.PG}' AS PrintProgramme,[BoardRegistrationNo],[CUSRegistrationNo],[FullName],[FathersName],[Gender],[DOB],[Category],[AcceptCollege_ID],[CurrentSemesterOrYear],[Batch],[StudentFormNo] FROM [ARGPersonalInformation_PG] WHERE IsPassout=0 AND FormStatus NOT IN('{(short)FormStatus.Rejected}','{(short)FormStatus.Disqualified}','{(short)FormStatus.Cancelled}','{(short)FormStatus.CancelRegistration}') AND (CUSRegistrationNo=@CUSRegistrationNO OR StudentFormNo=@CUSRegistrationNO)
                            )STDINFO";
            SqlCommand sqlCommand = new SqlCommand(Query);
            sqlCommand.Parameters.AddWithValue("@CUSRegistrationNO", CUSRegistrationNo);
            return new MSSQLFactory().GetObject<PersonalInformationCompact>(sqlCommand);
        }
        public Guid GetStudentIDByRegistrationNo(string CUSRegistrationNo)
        {
            var Query = $@"SELECT * FROM (
                            SELECT [Student_ID],'{PrintProgramme.UG}' AS PrintProgramme,[BoardRegistrationNo],[CUSRegistrationNo],[FullName],[FathersName],[Gender],[DOB],[Category],[AcceptCollege_ID],[CurrentSemesterOrYear],[Batch],[StudentFormNo] FROM [ARGPersonalInformation_UG] WHERE (CUSRegistrationNo=@CUSRegistrationNO OR StudentFormNo=@CUSRegistrationNO)
                            UNION 
                            SELECT [Student_ID],'{PrintProgramme.IH}' AS PrintProgramme,[BoardRegistrationNo],[CUSRegistrationNo],[FullName],[FathersName],[Gender],[DOB],[Category],[AcceptCollege_ID],[CurrentSemesterOrYear],[Batch],[StudentFormNo] FROM [ARGPersonalInformation_IH] WHERE (CUSRegistrationNo=@CUSRegistrationNO OR StudentFormNo=@CUSRegistrationNO)
                            UNION 
                            SELECT [Student_ID],'{PrintProgramme.PG}' AS PrintProgramme,[BoardRegistrationNo],[CUSRegistrationNo],[FullName],[FathersName],[Gender],[DOB],[Category],[AcceptCollege_ID],[CurrentSemesterOrYear],[Batch],[StudentFormNo] FROM [ARGPersonalInformation_PG] WHERE (CUSRegistrationNo=@CUSRegistrationNO OR StudentFormNo=@CUSRegistrationNO)
                            )STDINFO";
            SqlCommand sqlCommand = new SqlCommand(Query);
            sqlCommand.Parameters.AddWithValue("@CUSRegistrationNO", CUSRegistrationNo);
            return new MSSQLFactory().ExecuteScalar<Guid>(sqlCommand);
        }
        public Guid GetStudentIDByRegistrationNo(string CUSRegistrationNo, PrintProgramme printProgramme)
        {
            var tfix = printProgramme.GetTablePFix();
            var Query = $@"
                            SELECT [Student_ID],'{printProgramme}' AS PrintProgramme,[BoardRegistrationNo],[CUSRegistrationNo],[FullName],[FathersName],[Gender],[DOB],[Category],[AcceptCollege_ID],[CurrentSemesterOrYear],[Batch],[StudentFormNo] FROM ARGPersonalInformation_{tfix} STDINFO
                            WHERE (CUSRegistrationNo=@CUSRegistrationNO OR StudentFormNo=@CUSRegistrationNO)";
            SqlCommand sqlCommand = new SqlCommand(Query);
            sqlCommand.Parameters.AddWithValue("@CUSRegistrationNO", CUSRegistrationNo);
            return new MSSQLFactory().ExecuteScalar<Guid>(sqlCommand);
        }
        public List<PersonalInformationCompact> GetStudentInfoBySearchQuery(string SearchQuery)
        {
            string LikeQuery = SearchQuery + "%";
            var Query = $@"SELECT * FROM (
                            SELECT ARGPersonalInformation_UG.Student_ID,'{PrintProgramme.UG}' AS PrintProgramme,[BoardRegistrationNo],ClassRollNo,[CUSRegistrationNo],[FullName],[FathersName],[Gender],[DOB],[Category],[AcceptCollege_ID],[CurrentSemesterOrYear],[Batch],[StudentFormNo],ExamRollNumber FROM [ARGPersonalInformation_UG] 
							LEFT JOIN (select Distinct Student_ID,ExamRollNumber from ARGStudentExamForm_UG WHERE ExamRollNumber IS NOT NULL) ExmFORM on  ExmFORM.Student_ID = ARGPersonalInformation_UG.Student_ID 
						     UNION 
                            SELECT ARGPersonalInformation_IH.Student_ID,'{PrintProgramme.IH}' AS PrintProgramme,[BoardRegistrationNo],ClassRollNo,[CUSRegistrationNo],[FullName],[FathersName],[Gender],[DOB],[Category],[AcceptCollege_ID],[CurrentSemesterOrYear],[Batch],[StudentFormNo],ExamRollNumber FROM [ARGPersonalInformation_IH] 
                           LEFT JOIN (select Distinct Student_ID,ExamRollNumber from ARGStudentExamForm_IH WHERE ExamRollNumber IS NOT NULL) ExmFORM on  ExmFORM.Student_ID = ARGPersonalInformation_IH.Student_ID 
						    UNION 
                            SELECT ARGPersonalInformation_PG.Student_ID,'{PrintProgramme.PG}' AS PrintProgramme,[BoardRegistrationNo],ClassRollNo,[CUSRegistrationNo],[FullName],[FathersName],[Gender],[DOB],[Category],[AcceptCollege_ID],[CurrentSemesterOrYear],[Batch],[StudentFormNo],ExamRollNumber FROM [ARGPersonalInformation_PG] 
                            LEFT JOIN (select Distinct Student_ID,ExamRollNumber from ARGStudentExamForm_PG WHERE ExamRollNumber IS NOT NULL) ExmFORM on  ExmFORM.Student_ID = ARGPersonalInformation_PG.Student_ID 
						    )STDINFO WHERE (CUSRegistrationNo=@SearchQuery OR StudentFormNo=@SearchQuery OR classrollno=@SearchQuery OR ExamRollNumber =@SearchQuery OR BoardRegistrationNo =@SearchQuery OR FullName LIKE @LikeQuery)";
            SqlCommand sqlCommand = new SqlCommand(Query);
            sqlCommand.Parameters.AddWithValue("@SearchQuery", SearchQuery);
            sqlCommand.Parameters.AddWithValue("@LikeQuery", LikeQuery);
            return new MSSQLFactory().GetObjectList<PersonalInformationCompact>(sqlCommand);
        }
        public ADMCombinationMaster GetStudentCombinationByStudentID(Guid Student_ID, PrintProgramme programme)
        {
            return new MSSQLFactory().GetObject<ADMCombinationMaster>(new StudentSQLQueries().GetStudentCombinationByStudentID(Student_ID, programme));
        }


        #endregion

        #region Tests
        public List<CertificateCoursePersonalInfo> Helloworld()
        {
            string query = @"SELECT Student_ID,Photograph,PhotoType FROM CertificateCoursePersonalInfo";
            return new MSSQLFactory().GetObjectList<CertificateCoursePersonalInfo>(query);
        }
        #endregion


        #region NextSemesterAdmission



        public int UpdateCurrentSemOrYear(Guid Student_ID, short newSemester, PrintProgramme printProgramme)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            return new MSSQLFactory().ExecuteNonQuery($"UPDATE ARGPersonalInformation_{printProgramme.ToString()} SET CurrentSemesterOrYear ={newSemester},updatedOn=GETDATE(),updatedby='{AppUserHelper.User_ID}' WHERE Student_ID ='{Student_ID}'");
        }
        public ADMSemesterAdmissionSettings GetSemesterAdmissionSettings(Guid Course_ID, short Semester)
        {
            return new MSSQLFactory().GetObject<ADMSemesterAdmissionSettings>($"SELECT * FROM ADMSemesterAdmissionSettings WHERE Semester={Semester} AND Course_ID='{Course_ID}'");
        }
        public short GetAdmissionSemester(Guid CurrentCourse_ID, short CurrentSemester)
        {
            string query = $@"SELECT top 1 Semester FROM ADMSemesterAdmissionSettings
                            WHERE Status=1 AND Course_ID='{CurrentCourse_ID}' and Semester<={CurrentSemester + 2} ORDER BY Semester DESC";
            return new MSSQLFactory().ExecuteScalar<short>(query);
        }

        public short GetAdmissionSemesterRePrint(Guid CurrentCourse_ID, short CurrentSemester)
        {
            string query = $@"SELECT top 1 Semester FROM ADMSemesterAdmissionSettings
                            WHERE AllowRePrint=1 AND Course_ID='{CurrentCourse_ID}' and Semester>={CurrentSemester} and Semester<={CurrentSemester + 2} ORDER BY Semester DESC";
            return new MSSQLFactory().ExecuteScalar<short>(query);
        }


        public Guid GetStudentCurrentCourse()
        {
            string query = $@"SELECT Course_ID FROM ADMCombinationMaster WHERE Combination_ID = (
                            SELECT TOP 1 Combination_ID FROM ARGSelectedCombination_{AppUserHelper.TableSuffix.ToString()}
                            WHERE Student_ID = '{AppUserHelper.User_ID}' ORDER BY Semester DESC)";
            return new MSSQLFactory().ExecuteScalar<Guid>(query);
        }
        public Guid GetStudentCurrentCourse(PrintProgramme printProgramme, Guid student_ID)
        {
            string query = $@"SELECT Course_ID FROM ADMCombinationMaster WHERE Combination_ID = (
                            SELECT TOP 1 Combination_ID FROM ARGSelectedCombination_{printProgramme.GetTablePFix()}
                            WHERE Student_ID = '{student_ID}' ORDER BY Semester DESC)";
            return new MSSQLFactory().ExecuteScalar<Guid>(query);
        }

        public ARGSelectedCombination GetPreviousSelectedCombination(Guid Student_ID, short CurrentSemester)
        {
            string query = $@"SELECT TOP 1 * FROM ARGSelectedCombination_{AppUserHelper.TableSuffix}
                            WHERE Semester<{CurrentSemester} AND Student_ID='{Student_ID}' ORDER BY Semester DESC";
            return new MSSQLFactory().GetObject<ARGSelectedCombination>(query);
        }

        public int SaveSelectedCombination(StudentSelectedSubject selectedsubjects) => new MSSQLFactory().InsertRecord(selectedsubjects, "StudentSelectedSubjects");


        public bool IsStudentAllowedInSemesterAdm(string CUSRegistrationNoToAllow, short AllowForSemesterAdm)
        {
            SqlCommand command = new SqlCommand(@"SELECT COUNT(CUSRegistrationNoToAllow) FROM AllowInSemesterAdm
                                                WHERE CUSRegistrationNoToAllow = @CUSRegistrationNoToAllow AND AllowForSemesterAdm = @AllowForSemesterAdm");
            command.Parameters.AddWithValue("@CUSRegistrationNoToAllow", CUSRegistrationNoToAllow);
            command.Parameters.AddWithValue("@AllowForSemesterAdm", AllowForSemesterAdm);
            return new MSSQLFactory().ExecuteScalar<int>(command) > 0;
        }

        public bool ExcludeFromEligibilityCheck(string CUSRegistrationNoToAllow, short AllowForSemesterAdm)
        {
            SqlCommand command = new SqlCommand(@"SELECT TOP 1 ISNULL(ExcludeFromEligibilityCheck,0) FROM AllowInSemesterAdm
                                                WHERE CUSRegistrationNoToAllow = @CUSRegistrationNoToAllow AND AllowForSemesterAdm = @AllowForSemesterAdm ORDER BY ExcludeFromEligibilityCheck DESC");
            command.Parameters.AddWithValue("@CUSRegistrationNoToAllow", CUSRegistrationNoToAllow);
            command.Parameters.AddWithValue("@AllowForSemesterAdm", AllowForSemesterAdm);
            return new MSSQLFactory().ExecuteScalar<bool>(command);
        }

        public decimal GetAdditionalFeeSemesterAdm(string CUSRegistrationNoToAllow, short AllowForSemesterAdm)
        {
            SqlCommand command = new SqlCommand(@"SELECT TOP 1 AdditionalFee FROM AllowInSemesterAdm
                                                WHERE CUSRegistrationNoToAllow = @CUSRegistrationNoToAllow AND AllowForSemesterAdm = @AllowForSemesterAdm");
            command.Parameters.AddWithValue("@CUSRegistrationNoToAllow", CUSRegistrationNoToAllow);
            command.Parameters.AddWithValue("@AllowForSemesterAdm", AllowForSemesterAdm);
            return new MSSQLFactory().ExecuteScalar<decimal>(command);
        }


        #region For offline payment mode only     

        public int UpdateSelectedCombination(StudentSelectedSubject selectedsubjects)
        {
            List<string> ignoreList = new List<string>() { "Student_ID", "Semester", "CreatedOn", "CreatedBy" };
            var sqlCommand = new MSSQLFactory().UpdateRecord(selectedsubjects, ignoreList, ignoreList, $"StudentSelectedSubjects");
            sqlCommand.CommandText = sqlCommand.CommandText + " WHERE Student_ID =@Student_ID AND Semester=@Semester";
            sqlCommand.Parameters.AddWithValue("@Student_ID", selectedsubjects.Student_ID);
            sqlCommand.Parameters.AddWithValue("@Semester", selectedsubjects.Semester);
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }

        public StudentSelectedSubject GetStudentSelectedSubject(Guid student_ID, short semester)
        {
            var Query = $@"SELECT * FROM StudentSelectedSubjects WHERE Student_ID='{student_ID}' AND Semester={semester}";
            return new MSSQLFactory().GetObject<StudentSelectedSubject>(Query);
        }

        public List<ADMSubjectMaster> GetStudentSkillEnhancementSubjects(Guid student_ID, short semester)
        {
            List<ADMSubjectMaster> subjects = null;
            string skillEnhancementSubjects = new MSSQLFactory().ExecuteScalar<string>($"SELECT SkillEnhancementSubjects FROM StudentSelectedSubjects WHERE Student_ID = '{student_ID}' AND Semester={semester}");
            if (skillEnhancementSubjects != null)
                subjects = new MSSQLFactory().GetObjectList<ADMSubjectMaster>(new StudentSQLQueries().GetStudentSkillEnhancementSubjects(skillEnhancementSubjects));
            return subjects;
        }
        public List<StudentSelectedSubject> GetStudentSelectedSubjects(Parameters parameters)
        {
            var Query = $@"SELECT * FROM VWSSS ";
            var GeneralFunction = new GeneralFunctions();
            FilterHelper filterHelper = GeneralFunction.GetWhereClause<StudentSelectedSubject>(parameters.Filters);
            Query += filterHelper.WhereClause;
            Query += GeneralFunction.GetPagedQuery(Query, parameters);
            return new MSSQLFactory().GetObjectList<StudentSelectedSubject>(Query);
        }
        #endregion
        #endregion


        #region CollegePreference
        public List<StudentCollegePreferenceList> GetCollegePreference(Parameters parameters)
        {
            var Query = $@"Select CP.College_ID,ADMCollegeMaster.CollegeFullName,CP.Course_ID,CP.PreferenceOrder,CP.IsActive, ADMCourseMaster.CourseFullName,CP.Student_ID,FormStatus,StudentSelectionStatus
                            ,StudentFormNo,FullName,Gender,EntranceRollNo,Category,CATEntrancePoints,CP.IsAllowed,
                            CASE WHEN S.IsProvisional = 1 THEN null ELSE  CAST(CAST(((q.MarksObt / q.MaxMarks) * 100) AS DECIMAL(5, 2)) AS VARCHAR(7)) END AS [QualificationPercentage],
                            CASE WHEN S.IsProvisional = 1 THEN NULL ELSE  CAST(CAST(((CAST(((q.MarksObt / q.MaxMarks) * 100) AS DECIMAL(5, 2)) * 60)/100) AS DECIMAL(5,2)) AS VARCHAR(7))END AS [QualificationPoints],
                            CASE WHEN S.IsProvisional = 1 THEN S.CATEntrancePoints ELSE CAST(CAST(((CAST(((q.MarksObt / q.MaxMarks) * 100) AS DECIMAL(5, 2)) * 60)/100) AS DECIMAL(5,2))+S.CATEntrancePoints AS VARCHAR(7))END AS [TotalPoints]
                            FROM 
                            (select Student_ID,StudentFormNo,FullName,Gender,EntranceRollNo,Category,CATEntrancePoints,FormStatus,Batch,IsProvisional from ARGPersonalInformation_IH 
                            union all 
                            select Student_ID,StudentFormNo,FullName,Gender,EntranceRollNo,Category,CATEntrancePoints,FormStatus,Batch,IsProvisional from ARGPersonalInformation_UG
                            )S
                            JOIN (
                            SELECT Student_ID,Course_ID,StudentSelectionStatus FROM ARGCoursesApplied_IH
                            UNION All SELECT Student_ID,Course_ID,StudentSelectionStatus FROM ARGCoursesApplied_UG
                            )CA ON CA.Student_ID = S.Student_ID AND CA.Course_ID='a3ee7f98-7b82-4d95-a2c0-faba7a18240e' 
                            LEFT JOIN ARGStudentPreviousQualifications_IH Q ON Q.Student_ID = S.Student_ID
                            LEFT JOIN StudentCollegePreference CP ON CP.Student_ID = S.Student_ID
                            LEFT JOIN ADMCollegeMaster on ADMCollegeMaster.College_ID = CP.College_ID
                            LEFT JOIN ADMCourseMaster on ADMCourseMaster.Course_ID = CP.Course_ID";
            FilterHelper helper = new GeneralFunctions().GetWhereClause<StudentCollegePreferenceList>(parameters.Filters);
            helper.Command.CommandText = Query + helper.WhereClause + new GeneralFunctions().GetPagedQuery(Query, parameters);
            return new MSSQLFactory().GetObjectList<StudentCollegePreferenceList>(helper.Command);
        }

        public DataTable NewUGAdmissionFormsCSV(Parameters parameters)
        {
            var Query = $@"SELECT * FROM (
                            SELECT DISTINCT [CollegeFullName], CourseFullName,StudentFormNo,FullName,FathersName,Gender,EntranceRollNo,ARGStudentAddress_UG.PermanentAddress,
                            ARGStudentAddress_UG.District,ARGStudentAddress_UG.Mobile,ARGStudentAddress_UG.Email,TxnDate,
                            ARGStudentPreviousQualifications_UG.ExamName,ARGStudentPreviousQualifications_UG.Stream,ARGStudentPreviousQualifications_UG.Subjects,ARGStudentPreviousQualifications_UG.Session,
                            ARGStudentPreviousQualifications_UG.Year,ARGStudentPreviousQualifications_UG.RollNo,ARGStudentPreviousQualifications_UG.MaxMarks,ARGStudentPreviousQualifications_UG.MarksObt,
                            ARGStudentPreviousQualifications_UG.ExamBody,AssemblyConstituency,ParliamentaryConstituency,Pref.CreatedOn,Pref.College_ID
                            FROM ARGPersonalInformation_UG
                            JOIN  dbo.StudentCollegePreference Pref ON ARGPersonalInformation_UG.Student_ID = Pref.Student_ID
                            JOIN dbo.ARGStudentAddress_UG ON ARGStudentAddress_UG.Student_ID = ARGPersonalInformation_UG.Student_ID
                            JOIN dbo.ADMCollegeMaster ON ADMCollegeMaster.College_ID = Pref.College_ID
                            JOIN dbo.ADMCourseMaster ON ADMCourseMaster.Course_ID = Pref.Course_ID
                            JOIN dbo.PaymentDetails_UG ON Entity_ID=ARGStudentAddress_UG.Student_ID AND Semester=1 AND ModuleType={(int)PaymentModuleType.SemesterAdmission}
                            LEFT JOIN dbo.ARGStudentPreviousQualifications_UG ON ARGStudentPreviousQualifications_UG.Student_ID = ARGPersonalInformation_UG.Student_ID
                            WHERE Batch={DateTime.Now.Year} AND EntranceRollNo IS NOT NULL
                            )S";
            FilterHelper helper = new GeneralFunctions().GetWhereClause<StudentCollegePreferenceList>(parameters.Filters);
            helper.Command.CommandText = Query + helper.WhereClause + new GeneralFunctions().GetPagedQuery(Query, parameters);
            return new MSSQLFactory().GetDataTable(helper.Command);
        }
        public List<StudentCollegeCourseCount> CollegeCourseCount(Guid? College_ID)
        {
            var Query = $@"SELECT {(College_ID.HasValue ? "CollegeFullName,CollegeCode," : "")} Course.CourseFullName,SUM(Mapping.MeritBasislInTackCapacity) AS IntakeCapacity
                            FROM ADMCollegeMaster
                            JOIN ADMCollegeCourseMapping Mapping ON Mapping.College_ID = ADMCollegeMaster.College_ID
                            JOIN ADMCourseMaster Course ON Course.Course_ID = Mapping.Course_ID
                            WHERE Course.PrintProgramme=1 AND Course.Programme=1 {(College_ID.HasValue ? "AND ADMCollegeMaster.College_ID='" + College_ID + "'" : "")}
                            GROUP BY {(College_ID.HasValue ? "CollegeFullName,CollegeCode," : "")} Course.CourseFullName
                            ORDER BY Course.CourseFullName";
            return new MSSQLFactory().GetObjectList<StudentCollegeCourseCount>(Query);
        }
        public List<StudentCollegeCourseCount> StudentCollegeCourseCount(Guid? College_ID)
        {
            var Query = $@"SELECT {(College_ID.HasValue ? "CollegeFullName,CollegeCode," : "")} Course.CourseFullName,COUNT(Distinct Student_ID)[Count]
                            FROM ADMCollegeMaster
                            JOIN ADMCollegeCourseMapping Mapping ON Mapping.College_ID = ADMCollegeMaster.College_ID
                            JOIN ADMCourseMaster Course ON Course.Course_ID = Mapping.Course_ID
                            LEFT JOIN StudentCollegePreference ON StudentCollegePreference.College_ID = ADMCollegeMaster.College_ID AND StudentCollegePreference.Course_ID = Course.Course_ID
                            WHERE Course.PrintProgramme=1 AND Course.Programme=1 {(College_ID.HasValue ? "AND ADMCollegeMaster.College_ID='" + College_ID + "'" : "")}
                            GROUP BY {(College_ID.HasValue ? "CollegeFullName,CollegeCode," : "")} Course.CourseFullName
                            ORDER BY Course.CourseFullName";
            return new MSSQLFactory().GetObjectList<StudentCollegeCourseCount>(Query);
        }
        public List<StudentCollegePreference> GetCollegePreference(Guid student_ID)
        {
            var Query = $@"SELECT StudentCollegePreference.*,ADMCollegeMaster.College_ID,CollegeFullName,CourseFullName
                            FROM StudentCollegePreference
                            join ADMCollegeMaster on ADMCollegeMaster.College_ID = StudentCollegePreference.College_ID
                            join ADMCourseMaster on ADMCourseMaster.Course_ID = StudentCollegePreference.Course_ID
                            where (Student_ID = '{student_ID}' OR OldStudent_ID='{student_ID}') and StudentCollegePreference.IsActive=1 order by PreferenceOrder ";
            return new MSSQLFactory().GetObjectList<StudentCollegePreference>(Query);
        }
        public StudentCollegePreference GetPreference(Guid Preference_ID)
        {
            var Query = $@"SELECT *
                            FROM StudentCollegePreference where Preference_ID = '{Preference_ID}'";
            return new MSSQLFactory().GetObject<StudentCollegePreference>(Query);
        }
        #endregion

        #region UploadCertificates
        public int Save(Certificate _Certificate, PrintProgramme printProgramme)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            return new MSSQLFactory().InsertRecord(_Certificate, $"Certificate");
        }

        public int Update(Certificate _Certificate, PrintProgramme printProgramme)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            List<string> ignoreList = new List<string>() { "Certificate_ID" };
            var sqlCommand = new MSSQLFactory().UpdateRecord(_Certificate, ignoreList, ignoreList, $"Certificate");
            sqlCommand.CommandText += " WHERE Certificate_ID =@Certificate_ID";
            sqlCommand.Parameters.AddWithValue("@Certificate_ID", _Certificate.Certificate_ID);
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }

        public List<Certificate> GetUploadedCertificates(Guid Student_ID)
        {
            var Query = $@"Select * from Certificate where Student_ID='{Student_ID}'";
            return new MSSQLFactory().GetObjectList<Certificate>(Query);
        }

        public List<Certificate> GetUploadedCertificates(Parameters Parameter, PrintProgramme printProgramme)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            var Query = $@"Select * from Certificate";
            FilterHelper helper = new GeneralFunctions().GetWhereClause<Certificate>(Parameter.Filters);
            helper.Command.CommandText = Query + helper.WhereClause + new GeneralFunctions().GetPagedQuery(Query, Parameter);
            return new MSSQLFactory().GetObjectList<Certificate>(helper.Command);
        }

        public List<SelectedStudent> GetSelectedStudentsList(Parameters parameters, PrintProgramme printProgramme)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            FilterHelper helper = new GeneralFunctions().GetWhereClause<SelectedStudent>(parameters.Filters);
            var Query = $@"SELECT CC.College_ID,CollegeFullName,C.Course_ID,ADMCourseMaster.CourseFullName, S.Student_ID,S.StudentFormNo,S.FullName,S.FathersName,S.Category,S.Gender,S.EntranceRollNo
                        ,C.StudentSelectionStatus,ADMCourseMaster.PrintProgramme,ADMCourseMaster.Programme
                        ,COUNT(*) NumberOfCertificate,MAX(ISNULL(S.CATEntrancePoints,0))+MAX(C.SubjectEntrancePoints) AS TotalPoints
                        FROM ARGPersonalInformation_{printProgramme.ToString()} S
                        INNER JOIN ARGCoursesApplied_{printProgramme.ToString()} C ON C.Student_ID = S.Student_ID
                        INNER JOIN ADMCourseMaster ADMCourseMaster ON ADMCourseMaster.Course_ID = C.Course_ID
                        JOIN ADMCollegeCourseMapping CC ON CC.Course_ID = ADMCourseMaster.Course_ID
                        JOIN ADMCollegeMaster ON ADMCollegeMaster.College_ID = CC.College_ID
                        LEFT JOIN [Certificate] Certif ON Certif.Student_ID = C.Student_ID
                        {helper.WhereClause}
                        GROUP BY S.Student_ID,S.StudentFormNo,S.FullName,S.FathersName,S.Category,S.Gender,S.EntranceRollNo,
                        ADMCourseMaster.CourseFullName,C.Course_ID,C.StudentSelectionStatus,ADMCourseMaster.PrintProgramme,ADMCourseMaster.Programme,CC.College_ID,CollegeFullName";
            helper.Command.CommandText = Query + new GeneralFunctions().GetPagedQuery(Query, parameters);
            return new MSSQLFactory().GetObjectList<SelectedStudent>(helper.Command);
        }

        #endregion

        #region AllowInSemester
        public int Save(AllowInSemesterAdm item)
        {
            return new MSSQLFactory().InsertRecord(item, $"AllowInSemesterAdm");
        }
        public int DeleteAllowInSemesterAdm(string FormNumber_RegistrationNo, short Semester)
        {
            return new MSSQLFactory().ExecuteNonQuery($"Delete FROM AllowInSemesterAdm where CUSRegistrationNoToAllow='{FormNumber_RegistrationNo}' AND AllowForSemesterAdm={Semester}");
        }

        public int IsSelectedInLoggedInCollege(Guid student_ID, Guid college_ID, PrintProgramme printProgramme)
        {
            return new MSSQLFactory()
                .ExecuteScalar<int>
                ($"SELECT COUNT(SeatAllocationMatrix_Id) FROM dbo.SeatAllocationMatrix_{printProgramme} WHERE ISFinallySelected=1" +
                $" AND Student_Id='{student_ID}' AND College_IdAssigned='{college_ID}'");
        }

        public int IsSelectedInCourse(Guid student_ID, Guid course_ID, PrintProgramme printProgramme)
        {
            return new MSSQLFactory()
                .ExecuteScalar<int>
                ($"SELECT COUNT(SeatAllocationMatrix_Id) FROM dbo.SeatAllocationMatrix_{printProgramme} WHERE ISFinallySelected=1 AND " +
                $"Student_Id='{student_ID}' AND Course_IdAssigned='{course_ID}'");
        }

        public int UpdateStudentCombinationInfo(Guid student_ID, PrintProgramme printProgramme)
        {
            return new MSSQLFactory()
                .ExecuteNonQuery
                ($@"UPDATE dbo.ARGPersonalInformation_{printProgramme} SET CurrentSemesterOrYear=(CurrentSemesterOrYear-1)
                    WHERE Student_ID='{student_ID}'");
        }

        public int checkMerit(int batch, int preference, decimal subjectEntrancePoints, Guid course_Id, PrintProgramme printProgramme)
        {
            return new MSSQLFactory()
                .ExecuteScalar<int>
                ($@"SELECT COUNT(c.Student_ID) FROM dbo.ARGCoursesApplied_{printProgramme} c
                    JOIN dbo.ARGPersonalInformation_{printProgramme} p ON p.Student_ID = c.Student_ID
                    LEFT JOIN dbo.SeatAllocationMatrix_{printProgramme} s ON s.Student_Id = p.Student_ID
                    WHERE p.Batch={batch} AND p.FormStatus=10  AND c.Preference<={preference} AND c.SubjectEntrancePoints>{subjectEntrancePoints}
                    AND c.Course_ID='{course_Id}'
                    AND s.SeatAllocationMatrix_Id IS NULL");
        }

        public int ReceiveMigration(Guid student_ID, PrintProgramme printProgramme)
        {
            SqlCommand sqlCommand = new SqlCommand($@"UPDATE dbo.ARGPersonalInformation_{printProgramme} 
                                                    SET OtherUnivMigrationReceivedOn=@OtherUnivMigrationReceivedOn
                                                    WHERE Student_ID='{student_ID}'");

            sqlCommand.Parameters.AddWithValue("@OtherUnivMigrationReceivedOn", DateTime.Now);
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }
        #endregion

        public int UpdatePassout(string Student_Id, PrintProgramme printProgramme)
        {
            var Query = $@"UPDATE  ARGPersonalInformation_{printProgramme.GetTablePFix()}  SET ISPassout=1 WHERE Student_Id  {Student_Id}";

            SqlCommand sqlCommand = new SqlCommand(Query);
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }

        public string GetStudentABCID(Guid student_ID, PrintProgramme printProgramme)
        {
            return new MSSQLFactory().ExecuteScalar<string>($@"SELECT ABCID FROM dbo.ARGPersonalInformation_{printProgramme} WHERE Student_ID='{student_ID}'");
        }
    }
}
