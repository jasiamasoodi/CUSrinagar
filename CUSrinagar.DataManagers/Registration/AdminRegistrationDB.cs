using CUSrinagar.DataManagers.SQLQueries;
using CUSrinagar.Enums;
using CUSrinagar.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Terex;

namespace CUSrinagar.DataManagers
{
    public class AdminRegistrationDB
    {
        public Guid GetStudentIDByFormNo(ARGReprint studentDetail)
        {
            return new MSSQLFactory().ExecuteScalar<Guid>(new RegistrationSQLQueries().GetStudentIDByFormNo(studentDetail));
        }

        public int Save(ARGCoursesApplied aRGCoursesApplied, PrintProgramme printProgramme)
        {
            return new MSSQLFactory().InsertRecord(aRGCoursesApplied, $"ARGCoursesApplied_{printProgramme.ToString()}");
        }

        public int DeleteCourse(Guid Student_ID, PrintProgramme printProgramme)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = $"Delete From ARGCoursesApplied_{printProgramme.ToString()} Where Student_ID=@Student_ID";
            cmd.Parameters.AddWithValue("@Student_ID", Student_ID);
            return new MSSQLFactory().ExecuteNonQuery(cmd);
        }

        public List<ARGCoursesApplied> GetStudentCoursesApplied(Guid student_ID, PrintProgramme printProgramme)
        {
            return new MSSQLFactory().GetObjectList<ARGCoursesApplied>(new RegistrationSQLQueries().GetStudentCoursesApplied(student_ID, printProgramme));
        }
        public List<ARGCoursesApplied> GetStudentCoursesAppliedIDs(Guid student_ID, PrintProgramme printProgramme)
        {
            return new MSSQLFactory().GetObjectList<ARGCoursesApplied>(new RegistrationSQLQueries().GetStudentCoursesAppliedIDS(student_ID, printProgramme));
        }
        public int UpdateFeeANDPhoto(ARGPersonalInformation aRGPersonalInformation, PrintProgramme printProgramme, bool UpdatePhoto, bool UpdateCourse)
        {
            SqlCommand cmd = new SqlCommand();
            string Query = $"Update ARGPersonalInformation_{printProgramme.ToString()} SET ";
            if (UpdateCourse)
            {
                Query = Query + $"TotalFee = @TotalFee ";
                cmd.Parameters.AddWithValue("@TotalFee", aRGPersonalInformation.TotalFee);
            }
            if (UpdatePhoto)
            {
                Query = Query + (UpdateCourse ? "," : "") + $"Photograph = @Photograph";
                cmd.Parameters.AddWithValue("@Photograph", aRGPersonalInformation.Photograph);
            }

            cmd.Parameters.AddWithValue("@Student_ID", aRGPersonalInformation.Student_ID);
            cmd.CommandText = Query + " Where Student_ID = @Student_ID";
            return new MSSQLFactory().ExecuteNonQuery(cmd);
        }

        public bool CheckStudentBelongsToCollege(Guid Student_ID, PrintProgramme printProgramme)
        {
            return new MSSQLFactory().ExecuteScalar<int>($@"SELECT COUNT(Student_ID) AS [Value] FROM ARGPersonalInformation_{printProgramme.ToString()}
                                                        WHERE AcceptCollege_ID='{AppUserHelper.College_ID}' AND Student_ID='{Student_ID}';") > 0;
        }

        public int UpdateStudentDetails(ARGPersonalInformation aRGPersonalInformation, PrintProgramme printProgramme)
        {
            string PIQuery = $@"UPDATE ARGPersonalInformation_{printProgramme.ToString()}  
                            SET FullName=@FullName,FathersName=@FathersName,MothersName=@MothersName,Gender=@Gender,Religion=@Religion,DOB=@DOB,UpdatedOn=@UpdatedOn,UpdatedBy=@UpdatedBy,BoardRegistrationNo=@BoardRegistrationNo,Category=@Category,IsProvisional=@IsProvisional,IsLateralEntry=@IsLateralEntry";
            SqlCommand cmd = new SqlCommand();

            cmd.Parameters.AddWithValue("@FullName", aRGPersonalInformation.FullName);
            cmd.Parameters.AddWithValue("@FathersName", aRGPersonalInformation.FathersName);
            cmd.Parameters.AddWithValue("@MothersName", aRGPersonalInformation.MothersName);
            cmd.Parameters.AddWithValue("@Gender", aRGPersonalInformation.Gender);
            cmd.Parameters.AddWithValue("@Religion", aRGPersonalInformation.Religion);
            cmd.Parameters.AddWithValue("@DOB", aRGPersonalInformation.DOB);
            cmd.Parameters.AddWithValue("@UpdatedOn", aRGPersonalInformation.UpdatedOn);
            cmd.Parameters.AddWithValue("@UpdatedBy", aRGPersonalInformation.UpdatedBy);
            cmd.Parameters.AddWithValue("@BoardRegistrationNo", aRGPersonalInformation.BoardRegistrationNo);
            cmd.Parameters.AddWithValue("@Category", aRGPersonalInformation.Category);
            cmd.Parameters.AddWithValue("@IsProvisional", aRGPersonalInformation.IsProvisional);
            cmd.Parameters.AddWithValue("@IsLateralEntry", aRGPersonalInformation.IsLateralEntry);
            if (aRGPersonalInformation.Photograph != null)
            {
                cmd.Parameters.AddWithValue("@Photograph", aRGPersonalInformation.Photograph);
                PIQuery += ",Photograph = @Photograph";
            }
            if (!string.IsNullOrWhiteSpace(aRGPersonalInformation.PreviousUniversityRegnNo))
            {
                cmd.Parameters.AddWithValue("@PreviousUniversityRegnNo", aRGPersonalInformation.PreviousUniversityRegnNo);
                PIQuery += ",PreviousUniversityRegnNo=@PreviousUniversityRegnNo";
            }

            cmd.Parameters.AddWithValue("@Student_ID", aRGPersonalInformation.Student_ID);

            cmd.CommandText = $"{PIQuery} WHERE Student_ID = @Student_ID";
            return new MSSQLFactory().ExecuteNonQuery(cmd);
        }

        public int UpdateStudentAddress(ARGStudentAddress Address, PrintProgramme printProgramme)
        {
            List<string> IgnoreInQuery = new List<string> {
                nameof(Address.Student_ID),
                nameof(Address.Address_ID),
                //nameof(Address.Email),
                //nameof(Address.Mobile),
                nameof(Address.CreatedBy),
                nameof(Address.CreatedOn)
            };

            List<string> IgnoreInParamaters = new List<string> {
                nameof(Address.Address_ID),
                //nameof(Address.Email),
                //nameof(Address.Mobile),
                nameof(Address.CreatedBy),
                nameof(Address.CreatedOn)
            };
            SqlCommand command = new MSSQLFactory().UpdateRecord(Address, IgnoreInQuery, IgnoreInParamaters, $"ARGStudentAddress_{printProgramme.ToString()}");
            command.CommandText += $" WHERE Student_ID=@Student_ID;";
            return new MSSQLFactory().ExecuteNonQuery(command);
        }

        public int UpdateStudentQualification(ARGStudentPreviousQualifications rGStudentPreviousQualifications, PrintProgramme printProgramme)
        {
            List<string> IgnoreInQuery = new List<string> {
                nameof(rGStudentPreviousQualifications.Student_ID),
                nameof(rGStudentPreviousQualifications.Qualification_ID),
                nameof(rGStudentPreviousQualifications.CreatedBy),
                nameof(rGStudentPreviousQualifications.CreatedOn)
            };

            List<string> IgnoreInParamaters = new List<string> {
                nameof(rGStudentPreviousQualifications.Student_ID),
                nameof(rGStudentPreviousQualifications.CreatedBy),
                nameof(rGStudentPreviousQualifications.CreatedOn)
            };
            SqlCommand command = new MSSQLFactory().UpdateRecord(rGStudentPreviousQualifications, IgnoreInQuery, IgnoreInParamaters, $"ARGStudentPreviousQualifications_{printProgramme.ToString()}");
            command.CommandText += $" WHERE Qualification_ID=@Qualification_ID;";
            return new MSSQLFactory().ExecuteNonQuery(command);
        }
        public int DeleteStudentQualification(Guid Qualification_ID, PrintProgramme printProgramme)
        {
            return new MSSQLFactory().ExecuteNonQuery($"DELETE FROM ARGStudentPreviousQualifications_{printProgramme.ToString()} WHERE Qualification_ID='{Qualification_ID}'");
        }

        public DataTable GetSemesterAdmDetails(SemesterAdmissionFilter semesterAdmissionFilter)
        {
            //string getSubjects = "";
            short CombinationSemester = 1;

            if (semesterAdmissionFilter.Semester == 3)
            {
                //getSubjects = ",dbo.getSubjectFullNamesByCombination_ID(cm.Combination_ID) AS SubjectsOpted";
                CombinationSemester = semesterAdmissionFilter.Semester;
            }
            string tsql = $@"SELECT DISTINCT
                               ROW_NUMBER() OVER (PARTITION BY S.CollegeFullName,
                                                               S.CourseFullName
                                                  ORDER BY S.CollegeFullName,
                                                           S.CourseFullName
                                                 ) [sno],
                               CollegeFullName,S.CourseFullName,i.StudentFormNo,i.CUSRegistrationNo,
                               i.DOB,i.ClassRollNo,i.FullName,i.FathersName,p.Semester [Applied for Admission],
                               i.CurrentSemesterOrYear,p.Email,p.PhoneNumber,
                               dbo.getSubjectFullNamesByCombination_ID(cm.Combination_ID) AS SubjectsOpted
                        FROM dbo.ARGPersonalInformation_{semesterAdmissionFilter.SearchInPrintProgramme.ToString()} i
                            JOIN dbo.PaymentDetails_{semesterAdmissionFilter.SearchInPrintProgramme.ToString()} p
                                ON p.Entity_ID = i.Student_ID
                                   AND p.ModuleType = {(short)PaymentModuleType.SemesterAdmission}
	                        JOIN VWStudentCourse S ON S.Student_ID = i.Student_ID
                            LEFT JOIN dbo.ARGSelectedCombination_{semesterAdmissionFilter.SearchInPrintProgramme.ToString()} c
                                ON c.Student_ID = i.Student_ID AND c.Semester = {semesterAdmissionFilter.Semester}
                            LEFT JOIN dbo.ADMCombinationMaster cm
                                ON cm.Combination_ID = c.Combination_ID
                        Where P.Semester =  {semesterAdmissionFilter.Semester} AND i.FormStatus NOT IN ( 3, 5, 6, 13 )
                                   AND CAST(p.TxnDate AS DATE) >= '{semesterAdmissionFilter.Dated.ToString("yyyy-MM-dd")}'
                                   AND i.AcceptCollege_ID = '{semesterAdmissionFilter.College_ID}'
                        ORDER BY CollegeFullName,S.CourseFullName,sno;";
            return new MSSQLFactory().GetDataTable(tsql);
        }

        public int UpdateAlreadyAppliedCoursePref(ARGCoursesApplied item)
        {
            return new MSSQLFactory().ExecuteNonQuery($@"UPDATE dbo.ARGCoursesApplied_IH SET Preference={item.Preference}
                                                       WHERE Course_ID='{item.Course_ID}' AND Student_ID='{AppUserHelper.User_ID}'");
        }


        #region SQL Function

        //Create FUNCTION getSubjectFullNamesByCombination_ID
        //    (
        //        @Combination_ID UNIQUEIDENTIFIER
        //    )
        //    RETURNS VARCHAR(MAX)
        //    AS
        //    BEGIN
        //        DECLARE @subjectFullNames AS VARCHAR(MAX);
        //            SELECT @subjectFullNames = STRING_AGG(cm.SubjectFullName, ', ')
        //        FROM dbo.ADMSubjectMaster cm
        //        WHERE cm.Subject_ID IN
        //              (
        //                  SELECT Split.a.value('.', 'UNIQUEIDENTIFIER') AS Subject_ID
        //                  FROM
        //                  (
        //                      SELECT CAST('<M>' + REPLACE(CombinationSubjects, '|', '</M><M>') + '</M>' AS XML) AS String
        //                      FROM ADMCombinationMaster
        //                      WHERE Combination_ID = @Combination_ID
        //                  ) AS A
        //                  CROSS APPLY String.nodes('/M') AS Split(a)
        //              );
        //        RETURN @subjectFullNames;
        //    END;

        #endregion
    }
}
