using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using CUSrinagar.DataManagers;
using CUSrinagar.DataManagers.SQLQueries;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using CUSrinagar.Models;
using Terex;

namespace CUSrinagar.DataManagers
{
    public class NEPDB
    {
        public bool FormExists(Guid Student_ID, PrintProgramme printProgramme)
        {
            return new MSSQLFactory().ExecuteScalar<int>($"SELECT COUNT(Student_ID) FROM dbo.ARGPersonalInformation_{printProgramme.ToString()} WHERE Student_ID='{Student_ID}'") > 0;
        }

        public int UpdatePersonalInfo(ARGPersonalInformation aRGPersonalInformation, PrintProgramme printProgramme)
        {
            string PIQuery = $@"UPDATE ARGPersonalInformation_{printProgramme.ToString()}  
                                SET FullName=@FullName,FathersName=@FathersName,
                                MothersName=@MothersName,Gender=@Gender,Religion=@Religion,
                                DOB=@DOB,UpdatedOn=@UpdatedOn,Category=@Category,
                                IsLateralEntry=@IsLateralEntry,PreviousUniversityRegnNo=@PreviousUniversityRegnNo,
                                CUETApplicationNo=@CUETApplicationNo,CUETEntranceRollNo=@CUETEntranceRollNo ";
            SqlCommand cmd = new SqlCommand();

            cmd.Parameters.AddWithValue("@FullName", aRGPersonalInformation.FullName);
            cmd.Parameters.AddWithValue("@FathersName", aRGPersonalInformation.FathersName);
            cmd.Parameters.AddWithValue("@MothersName", aRGPersonalInformation.MothersName);
            cmd.Parameters.AddWithValue("@Gender", aRGPersonalInformation.Gender);
            cmd.Parameters.AddWithValue("@Religion", aRGPersonalInformation.Religion);
            cmd.Parameters.AddWithValue("@DOB", aRGPersonalInformation.DOB);
            cmd.Parameters.AddWithValue("@UpdatedOn", aRGPersonalInformation.UpdatedOn);
            cmd.Parameters.AddWithValue("@Category", aRGPersonalInformation.Category);
            cmd.Parameters.AddWithValue("@IsLateralEntry", aRGPersonalInformation.IsLateralEntry);
            cmd.Parameters.AddWithValue("@Student_ID", aRGPersonalInformation.Student_ID);

            if (string.IsNullOrWhiteSpace(aRGPersonalInformation.PreviousUniversityRegnNo))
            {
                cmd.Parameters.AddWithValue("@PreviousUniversityRegnNo", DBNull.Value);
            }
            else
            {
                cmd.Parameters.AddWithValue("@PreviousUniversityRegnNo", aRGPersonalInformation.PreviousUniversityRegnNo);
            }

            if (string.IsNullOrWhiteSpace(aRGPersonalInformation.CUETApplicationNo))
            {
                cmd.Parameters.AddWithValue("@CUETApplicationNo", DBNull.Value);
            }
            else
            {
                cmd.Parameters.AddWithValue("@CUETApplicationNo", aRGPersonalInformation.CUETApplicationNo);
            }

            if (string.IsNullOrWhiteSpace(aRGPersonalInformation.CUETEntranceRollNo))
            {
                cmd.Parameters.AddWithValue("@CUETEntranceRollNo", DBNull.Value);
            }
            else
            {
                cmd.Parameters.AddWithValue("@CUETEntranceRollNo", aRGPersonalInformation.CUETEntranceRollNo);
            }

            cmd.CommandText = $"{PIQuery} WHERE Student_ID = @Student_ID";
            return new MSSQLFactory().ExecuteNonQuery(cmd);
        }

        public int DeleteAddress(Guid Student_ID, PrintProgramme printProgramme)
        {
            return new MSSQLFactory().ExecuteNonQuery($"DELETE FROM dbo.ARGStudentAddress_{printProgramme.ToString()} WHERE Student_ID='{Student_ID}'");
        }
        public int DeleteCourseApplied(Guid Student_ID, PrintProgramme printProgramme)
        {
            return new MSSQLFactory().ExecuteNonQuery($"DELETE FROM dbo.ARGCoursesApplied_{printProgramme.ToString()} WHERE Student_ID='{Student_ID}'");
        }


        public int Save(NEPCollegePreference collegePreference)
        {
            return new MSSQLFactory().InsertRecord(collegePreference, "NEPCollegePreferences");
        }
        public int DeleteNEPCollegePreferences(Guid Student_ID)
        {
            return new MSSQLFactory().ExecuteNonQuery($"DELETE FROM dbo.NEPCollegePreferences WHERE Student_ID='{Student_ID}'");
        }

        public int DeleteCourseApplied(Guid Student_ID)
        {
            return new MSSQLFactory().ExecuteNonQuery($"DELETE FROM dbo.NEPCollegePreferences WHERE Student_ID='{Student_ID}'");
        }

        public int UpdatePhotoPath(Guid Student_ID, string PhotoPath, PrintProgramme printProgramme)
        {
            return new MSSQLFactory().ExecuteNonQuery($"UPDATE dbo.ARGPersonalInformation_{printProgramme.ToString()} SET Photograph='{PhotoPath}' WHERE Student_ID='{Student_ID}'");
        }
        public int DeleteCertificate(Guid Student_ID, CertificateType certificateType)
        {
            return new MSSQLFactory().ExecuteNonQuery($"DELETE FROM dbo.Certificate WHERE Student_ID='{Student_ID}' AND CertificateType='{(short)certificateType}'");
        }
        public string GetStudentPhotoPath(Guid Student_ID, PrintProgramme printProgramme)
        {
            return new MSSQLFactory().ExecuteScalar<string>($"SELECT Photograph FROM dbo.ARGPersonalInformation_{printProgramme.ToString()} WHERE Student_ID='{Student_ID}'");
        }

        public List<SelectListItem> GetCollegePreference(List<Guid> Course_IDs)
        {
            var InClause = Course_IDs.ToIN();
            string Query = $@"SELECT DISTINCT C.CollegeFullName [Text],CAST(C.College_ID AS NVARCHAR(MAX)) [Value]
                                FROM dbo.ADMCollegeMaster C
                                JOIN dbo.ADMCollegeCourseMapping CM ON CM.College_ID = C.College_ID
                                WHERE CM.Course_ID  IN {InClause} ORDER BY C.CollegeFullName";
            return new MSSQLFactory().GetObjectList<SelectListItem>(Query);
        }

        public List<NEPCollegePreference> GetCollegePreferences(Guid Student_ID, PrintProgramme printProgramme)
        {
            string Query = $@"SELECT NEPCollegePreferences.*,CollegeFullName FROM NEPCollegePreferences 
                            JOIN dbo.ADMCollegeMaster ON ADMCollegeMaster.College_ID = NEPCollegePreferences.College_ID 
                            WHERE NEPCollegePreferences.Student_ID='{Student_ID}' ORDER BY PreferenceNo ASC";
            return new MSSQLFactory().GetObjectList<NEPCollegePreference>(Query);
        }

        public int UpdateStudentFeeStatus(Guid Student_ID, FormStatus formStatus, PrintProgramme printProgramme)
        {
            return new MSSQLFactory().ExecuteNonQuery($"UPDATE dbo.ARGPersonalInformation_{printProgramme.ToString()} SET FormStatus='{(short)formStatus}' WHERE Student_ID='{Student_ID}'");
        }

        public bool HasCategory(Guid Student_ID, PrintProgramme printProgramme)
        {
            return new MSSQLFactory().ExecuteScalar<int>($"SELECT COUNT(Student_ID) FROM dbo.ARGPersonalInformation_{printProgramme.ToString()} WHERE Student_ID='{Student_ID}' AND Category<>'OM'") > 0;
        }

        public Guid GetStudentIDByFormNoAndDOB(ARGReprint aRGReprint)
        {
            return new MSSQLFactory().ExecuteScalar<Guid>(new RegistrationSQLQueries().GetStudentIDByFormNoAndDOBNEP(aRGReprint));
        }

        public bool SubjectsApplied(Guid Student_ID, PrintProgramme printProgramme)
        {
            return new MSSQLFactory().ExecuteScalar<int>($"SELECT COUNT(Student_ID) FROM dbo.ARGCoursesApplied_{printProgramme.ToString()} WHERE Student_ID='{Student_ID}'") > 0;
        }
        public bool QualificationsApplied(Guid Student_ID, PrintProgramme printProgramme)
        {
            return new MSSQLFactory().ExecuteScalar<int>($"SELECT COUNT(Student_ID) FROM dbo.ARGStudentPreviousQualifications_{printProgramme.ToString()} WHERE Student_ID='{Student_ID}'") > 0;
        }
        public bool CollegePreferencesApplied(Guid Student_ID)
        {
            return new MSSQLFactory().ExecuteScalar<int>($"SELECT COUNT(Student_ID) FROM dbo.NEPCollegePreferences WHERE Student_ID='{Student_ID}'") > 0;
        }
        public bool CertificateApplied(Guid Student_ID)
        {
            return new MSSQLFactory().ExecuteScalar<int>($"SELECT COUNT(Student_ID) FROM dbo.Certificate WHERE Student_ID='{Student_ID}'") > 0;
        }
        public bool CheckForWomensCollege(Guid Student_ID)
        {
            SqlCommand cmd = new SqlCommand($@"SELECT COUNT(p.Student_ID) FROM dbo.ARGPersonalInformation_UG p
                                                         JOIN dbo.NEPCollegePreferences c ON c.Student_ID = p.Student_ID
                                                         WHERE p.Student_ID = '{Student_ID}' AND c.College_ID = 'B5E689E6-75FB-44E9-8ED7-3E649C18B659'");
            return new MSSQLFactory().ExecuteScalar<int>(cmd) > 0;
        }

        public List<DropDownOptLabelGeneral> GetCourseListForRegistrationByGroup(PrintProgramme printProgramme, string gender)
        {
            string genderSql = string.IsNullOrWhiteSpace(gender) ? "" : $@" AND
                                                                              (
                                                                                  ADMCollegeCourseMapping.AllowedGender IS NULL
                                                                                  OR ADMCollegeCourseMapping.AllowedGender = @gender 
                                                                              ) ";
            string Query = $@"SELECT DISTINCT CONVERT(NVARCHAR(50), ADMCourseMaster.Course_ID) AS Value,
                                       CourseFullName AS Text,
                                       Programme,
                                       CourseCode,
                                       '-------------- Stream : ' + GroupName + ' ----------------' AS DataGroupField
                                FROM ADMCourseMaster
                                    LEFT JOIN dbo.ADMCollegeCourseMapping
                                        ON ADMCollegeCourseMapping.Course_ID = ADMCourseMaster.Course_ID
                                WHERE ADMCourseMaster.Status = 1
                                      AND ADMCourseMaster.RegistrationOpen = 1 {genderSql}";
            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.Parameters.AddWithValue("@PrintProgramme", printProgramme);

            if (!string.IsNullOrWhiteSpace(genderSql))
                sqlCommand.Parameters.AddWithValue("@gender", gender);

            Query += $" AND PrintProgramme=@PrintProgramme";
            sqlCommand.CommandText = Query;
            return new MSSQLFactory().GetObjectList<DropDownOptLabelGeneral>(sqlCommand);
        }

        public bool BlockEditAfterPayment(Guid Student_ID, PrintProgramme printProgrammeOption)
        {
            return new MSSQLFactory().ExecuteScalar<short>($@"SELECT FormStatus FROM dbo.ARGPersonalInformation_{printProgrammeOption} WHERE Student_ID='{Student_ID}'") != 1;
        }

        public string GetStudentGender(PrintProgramme printProgramme, Guid student_ID)
        {
            return new MSSQLFactory().ExecuteScalar<string>($@"SELECT Gender FROM dbo.ARGPersonalInformation_{printProgramme} WHERE Student_ID='{student_ID}'") ?? "";

        }

        public bool IsProvisionalAdm(Guid student_ID, PrintProgramme printProgramme)
        {
            return new MSSQLFactory().ExecuteScalar<bool>($@"SELECT IsProvisional FROM dbo.ARGPersonalInformation_{printProgramme} WHERE Student_ID='{student_ID}'");
        }
    }
}
