using CUSrinagar.Enums;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CUSrinagar.Models;
using CUSrinagar.Extensions;
using Terex;

namespace CUSrinagar.DataManagers.SQLQueries
{
    public class RegistrationSQLQueries
    {

        internal SqlCommand GetStudentByID(Guid ID, PrintProgramme printProgramme)
        {
            SqlCommand command = new SqlCommand();
            command.Parameters.AddWithValue("@Student_ID", ID);
            command.CommandText = $@"SELECT ARGPersonalInformation_{printProgramme.ToString()}.*,ADMCollegeMaster.CollegeFullName AS AcceptCollegeID,ADMCollegeMaster.CollegeCode FROM ARGPersonalInformation_{printProgramme.ToString()}
                                    LEFT JOIN ADMCollegeMaster ON ADMCollegeMaster.College_ID = ARGPersonalInformation_{printProgramme.ToString()}.AcceptCollege_ID  
                                    WHERE ARGPersonalInformation_{printProgramme.ToString()}.Student_ID = @Student_ID";
            return command;
        }
        internal SqlCommand GetStudentIDByFormNoAndDOB(ARGReprint aRGReprint)
        {
            SqlCommand command = new SqlCommand();
            command.Parameters.AddWithValue("@StudentFormNo", aRGReprint.FormNo);

            command.Parameters.AddWithValue("@DOB", aRGReprint.DOB.ToString("yyyy-MM-dd"));
            string dobs = " AND DOB=@DOB ";
            command.CommandText = $@"SELECT TOP 1 Student_ID FROM ARGPersonalInformation_{aRGReprint.PrintProgrammeOption.ToString()} 
                                    WHERE (StudentFormNo = @StudentFormNo OR BoardRegistrationNo=@StudentFormNo)
                                   {dobs}ORDER BY CreatedOn DESC";
            return command;
        }

        internal SqlCommand GetStudentIDByFormNoAndDOBNEP(ARGReprint aRGReprint)
        {
            SqlCommand command = new SqlCommand();
            command.Parameters.AddWithValue("@StudentFormNo", aRGReprint.FormNo);

            command.Parameters.AddWithValue("@DOB", aRGReprint.DOB.ToString("yyyy-MM-dd"));

            string dobs = " AND DOB=@DOB ";

            if (aRGReprint.AllowFormsToBeEditFromDate != null)
            {
                dobs += $" AND CreatedOn>'{aRGReprint.AllowFormsToBeEditFromDate.Value.ToString("yyyy-MM-dd")}' ";
            }

            command.CommandText = $@"SELECT TOP 1 Student_ID FROM ARGPersonalInformation_{aRGReprint.PrintProgrammeOption.ToString()} 
                                    WHERE (StudentFormNo = @StudentFormNo OR BoardRegistrationNo=@StudentFormNo) AND Batch>={aRGReprint.Batch} 
                                   {dobs} AND FormStatus<>{(short)FormStatus.CancelRegistration} ORDER BY CreatedOn DESC";
            return command;
        }


        internal SqlCommand GetStudentIDByFormNoAndDOBCUS(ARGReprint aRGReprint)
        {
            SqlCommand command = new SqlCommand();
            command.Parameters.AddWithValue("@StudentFormNo", aRGReprint.FormNo);
            command.Parameters.AddWithValue("@DOB", aRGReprint.DOB.ToString("yyyy-MM-dd"));
            command.CommandText = $@"SELECT TOP 1 Student_ID FROM ARGPersonalInformation_{aRGReprint.PrintProgrammeOption.ToString()} WHERE (StudentFormNo = @StudentFormNo OR CUSRegistrationNo=@StudentFormNo OR BoardRegistrationNo=@StudentFormNo) AND 
                                   DOB=@DOB ORDER BY CreatedOn DESC";
            return command;
        }
        internal SqlCommand GetStudentByRegNOandDOB(string CUSRegistrationNo, DateTime DOB, PrintProgramme printProgramme)
        {
            SqlCommand command = new SqlCommand();
            command.Parameters.AddWithValue("@CUSRegistrationNo", CUSRegistrationNo);
            command.Parameters.AddWithValue("@fs", (short)FormStatus.CancelRegistration);
            command.Parameters.AddWithValue("@DOB", DOB.ToString("yyyy-MM-dd"));
            command.CommandText = $@"SELECT TOP 1 PINFO.*,Course.*,CM.Duration FROM ARGPersonalInformation_{printProgramme.GetTablePFix()} PINFO Left join VWStudentCourse Course ON 
                                      PINFO.Student_ID = Course.Student_ID Left JOIN ADMCourseMaster CM ON CM.Course_ID = Course.Course_ID WHERE ( PINFO.CUSRegistrationNo=@CUSRegistrationNo ) AND 
                                   PINFO.DOB=@DOB  ORDER BY PINFO.CreatedOn DESC";
            return command;
        }
        internal SqlCommand GetStudentAddress(Guid student_ID, PrintProgramme printProgramme)
        {
            SqlCommand command = new SqlCommand();
            command.Parameters.AddWithValue("@Student_ID", student_ID);
            command.CommandText = $@"SELECT * FROM ARGStudentAddress_{printProgramme.ToString()} WHERE Student_ID = @Student_ID";
            return command;
        }

        internal SqlCommand GetStudentAcademicDetails(Guid student_ID, PrintProgramme printProgramme)
        {
            SqlCommand command = new SqlCommand();
            command.Parameters.AddWithValue("@Student_ID", student_ID);
            command.CommandText = $@"SELECT * FROM ARGStudentPreviousQualifications_{printProgramme.ToString()} WHERE Student_ID = @Student_ID ORDER BY [Year] ASC";
            return command;
        }

        internal SqlCommand GetStudentCoursesApplied(Guid student_ID, PrintProgramme printProgramme)
        {
            SqlCommand command = new SqlCommand();
            command.Parameters.AddWithValue("@Student_ID", student_ID);
            command.CommandText = $@"SELECT ARGCoursesApplied_{printProgramme.ToString()}.*,
                                            ADMCourseMaster.CourseFullName AS CourseName,ADMCourseMaster.PrintProgramme,ADMCourseMaster.Programme,
                                            ADMCourseMaster.CourseCode,
                                            ADMCourseMaster.GroupName
                                            FROM ARGCoursesApplied_{printProgramme.ToString()}
                                            INNER JOIN ADMCourseMaster ON ARGCoursesApplied_{printProgramme.ToString()}.Course_ID = ADMCourseMaster.Course_ID
                                            WHERE ARGCoursesApplied_{printProgramme.ToString()}.Student_ID = @Student_ID ORDER BY GroupName,Programme,CourseFullName";
            return command;
        }


        internal SqlCommand GetStudentCoursesApplied(List<Guid> course_Ids)
        {
            SqlCommand command = new SqlCommand();
            command.CommandText = $@"SELECT CourseCode,Course_ID,CourseFullName AS CourseName,Programme FROM dbo.ADMCourseMaster WHERE Course_ID IN ({course_Ids.ToIN()})";
            return command;
        }

        #region Admin
        internal SqlCommand GetStudentIDByFormNo(ARGReprint studentDetail)
        {
            SqlCommand command = new SqlCommand();
            command.Parameters.AddWithValue("@StudentFormNo", studentDetail.FormNo);
            command.Parameters.AddWithValue("@Batch", studentDetail.Batch);
            command.CommandText = $@"SELECT Student_ID FROM ARGPersonalInformation_{studentDetail.PrintProgrammeOption.ToString()} WHERE (StudentFormNo = @StudentFormNo OR BoardRegistrationNo=@StudentFormNo OR CUSRegistrationNo=@StudentFormNo) AND Batch=@Batch";
            return command;
        }
        internal SqlCommand GetStudentCoursesAppliedIDS(Guid student_ID, PrintProgramme printProgramme)
        {
            SqlCommand command = new SqlCommand();
            command.Parameters.AddWithValue("@Student_ID", student_ID);
            command.CommandText = $@"SELECT ARGCoursesApplied_{printProgramme.ToString()}.Course_ID,ARGCoursesApplied_{printProgramme.ToString()}.Preference,ARGCoursesApplied_{printProgramme.ToString()}.SubjectEntrancePoints,ARGCoursesApplied_{printProgramme.ToString()}.AppearedInEntrance
                                        ,ADMCourseMaster.CourseFullName AS CourseName,ADMCourseMaster.Programme
                                            FROM ARGCoursesApplied_{printProgramme.ToString()}
                                            INNER JOIN ADMCourseMaster ON ARGCoursesApplied_{printProgramme.ToString()}.Course_ID = ADMCourseMaster.Course_ID
                                            WHERE ARGCoursesApplied_{printProgramme.ToString()}.Student_ID = @Student_ID";
            return command;
        }

        internal SqlCommand GetSelectedCombinations(Guid student_ID, PrintProgramme programme)
        {
            string query = $@" SELECT *,CourseFullName as CourseID,CollegeFullName AS CollegeID FROM ARGPersonalInformation_{programme.ToString()}                                 
                                 LEFT JOIN ADMCollegeMaster ON ADMCollegeMaster.College_ID = ARGPersonalInformation_{programme.ToString()}.AcceptCollege_ID
								 LEFT JOIN ARGSelectedCombination_{programme.ToString()} ON ARGPersonalInformation_{programme.ToString()}.Student_ID = ARGSelectedCombination_{programme.ToString()}.Student_ID
								 JOIN ADMCombinationMaster ON ADMCombinationMaster.Combination_ID = ARGSelectedCombination_{programme.ToString()}.Combination_ID
								 JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = ADMCombinationMaster.Course_ID
                               WHERE ARGSelectedCombination_{programme.ToString()}.Student_ID = @Student_ID ";

            SqlCommand command = new SqlCommand();
            command.CommandText = query;
            command.Parameters.AddWithValue("@Student_ID", student_ID);
            return command;
        }

        internal int DeleteStudentByID(Guid student_ID, PrintProgramme printProgramme)
        {
            var Query = $"Delete from MigrationDetail WHERE Student_ID ='{student_ID}' and FormStatus!={(int)FormStatus.FeePaid}";
            return new MSSQLFactory().ExecuteNonQuery(Query);
        }
        #endregion
    }
}
