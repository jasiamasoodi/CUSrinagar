using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUSrinagar.Models
{
    public partial class CUETRegistrationData
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("data")]
        public CUETCandidateDetails CandidateDetails { get; set; }
        public CUETCandidateDocs CandidateDoc { get; set; }
        public CUETError CUETErrors { get; set; }
    }

    public partial class CUETCandidateDetails
    {
        [JsonProperty("application_no")]
        public long ApplicationNo { get; set; }

        public DateTime DOB { get; set; }
        public string BoardRegistrationNo { get; set; }
        public PrintProgramme PrintProgramme { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("gender")]
        public string Gender { get; set; }

        [JsonProperty("nationality")]
        public string Nationality { get; set; }

        [JsonProperty("other_nationality")]
        public object OtherNationality { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("pwd_applicable")]
        public string PwdApplicable { get; set; }

        [JsonProperty("pwd_category")]
        public object PwdCategory { get; set; }

        [JsonProperty("pwd_category_other")]
        public object PwdCategoryOther { get; set; }

        [JsonProperty("pwd_war")]
        public object PwdWar { get; set; }

        [JsonProperty("father_name")]
        public string FatherName { get; set; }

        [JsonProperty("father_qualification")]
        public string FatherQualification { get; set; }

        [JsonProperty("father_occupation")]
        public string FatherOccupation { get; set; }

        [JsonProperty("mother_name")]
        public string MotherName { get; set; }

        [JsonProperty("mother_qualification")]
        public string MotherQualification { get; set; }

        [JsonProperty("mother_occupation")]
        public string MotherOccupation { get; set; }

        [JsonProperty("guardian_name")]
        public string GuardianName { get; set; }

        [JsonProperty("family_income")]
        public string FamilyIncome { get; set; }

        [JsonProperty("twin")]
        public string Twin { get; set; }

        [JsonProperty("twin_name")]
        public object TwinName { get; set; }

        [JsonProperty("domicile_state")]
        public string DomicileState { get; set; }

        [JsonProperty("residence_place")]
        public string ResidencePlace { get; set; }

        [JsonProperty("medium_schooling")]
        public string MediumSchooling { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("add_state")]
        public string AddState { get; set; }

        [JsonProperty("add_district")]
        public string AddDistrict { get; set; }

        [JsonProperty("country_pr")]
        public string CountryPr { get; set; }

        [JsonProperty("add_state_pr")]
        public string AddStatePr { get; set; }

        [JsonProperty("add_district_pr")]
        public string AddDistrictPr { get; set; }

        [JsonProperty("bpl")]
        public string Bpl { get; set; }

        [JsonProperty("kashmiri_migrant")]
        public string KashmiriMigrant { get; set; }

        [JsonProperty("cw_quota")]
        public string CwQuota { get; set; }

        [JsonProperty("ncc_selection")]
        public string NccSelection { get; set; }

        [JsonProperty("sports_selection")]
        public string SportsSelection { get; set; }

        [JsonProperty("sports_achievement")]
        public object SportsAchievement { get; set; }

        [JsonProperty("ward_quota")]
        public string WardQuota { get; set; }

        [JsonProperty("ward_quota_relation_father")]
        public object WardQuotaRelationFather { get; set; }

        [JsonProperty("ward_university_father")]
        public object WardUniversityFather { get; set; }

        [JsonProperty("ward_employee_id_father")]
        public object WardEmployeeIdFather { get; set; }

        [JsonProperty("ward_department_father")]
        public object WardDepartmentFather { get; set; }

        [JsonProperty("ward_employee_designation_father")]
        public object WardEmployeeDesignationFather { get; set; }

        [JsonProperty("ward_employment_status_father")]
        public object WardEmploymentStatusFather { get; set; }

        [JsonProperty("ward_quota_relation_mother")]
        public object WardQuotaRelationMother { get; set; }

        [JsonProperty("ward_university_mother")]
        public object WardUniversityMother { get; set; }

        [JsonProperty("ward_employee_id_mother")]
        public object WardEmployeeIdMother { get; set; }

        [JsonProperty("ward_department_mother")]
        public object WardDepartmentMother { get; set; }

        [JsonProperty("ward_employee_designation_mother")]
        public object WardEmployeeDesignationMother { get; set; }

        [JsonProperty("ward_employment_status_mother")]
        public object WardEmploymentStatusMother { get; set; }

        [JsonProperty("x_result_status")]
        public string XResultStatus { get; set; }

        [JsonProperty("x_passing_year")]
        public long XPassingYear { get; set; }

        [JsonProperty("x_course_name")]
        public string XCourseName { get; set; }

        [JsonProperty("x_schooling_place")]
        public string XSchoolingPlace { get; set; }

        [JsonProperty("x_school_type")]
        public string XSchoolType { get; set; }

        [JsonProperty("x_school_type_other")]
        public object XSchoolTypeOther { get; set; }

        [JsonProperty("x_country")]
        public string XCountry { get; set; }

        [JsonProperty("x_state")]
        public string XState { get; set; }

        [JsonProperty("x_district")]
        public string XDistrict { get; set; }

        [JsonProperty("x_board")]
        public string XBoard { get; set; }

        [JsonProperty("x_board_other")]
        public object XBoardOther { get; set; }

        [JsonProperty("x_result_type")]
        public string XResultType { get; set; }

        [JsonProperty("x_grade_point")]
        public object XGradePoint { get; set; }

        [JsonProperty("x_cgpa_obtained")]
        public object XCgpaObtained { get; set; }

        [JsonProperty("x_cgpa")]
        public object XCgpa { get; set; }

        [JsonProperty("x_maximum_marks")]
        public string XMaximumMarks { get; set; }

        [JsonProperty("x_marks_obtained")]
        public string XMarksObtained { get; set; }

        [JsonProperty("x_percentage")]
        public string XPercentage { get; set; }

        [JsonProperty("xii_result_status")]
        public string XiiResultStatus { get; set; }

        [JsonProperty("xii_passing_year")]
        public int XiiPassingYear { get; set; }

        [JsonProperty("xii_course_name")]
        public string XiiCourseName { get; set; }

        [JsonProperty("xii_schooling_place")]
        public string XiiSchoolingPlace { get; set; }

        [JsonProperty("xii_school_type")]
        public string XiiSchoolType { get; set; }

        [JsonProperty("xii_school_type_other")]
        public object XiiSchoolTypeOther { get; set; }

        [JsonProperty("xii_country")]
        public string XiiCountry { get; set; }

        [JsonProperty("xii_state")]
        public string XiiState { get; set; }

        [JsonProperty("xii_district")]
        public string XiiDistrict { get; set; }

        [JsonProperty("xii_board")]
        public string XiiBoard { get; set; }

        [JsonProperty("xii_board_other")]
        public object XiiBoardOther { get; set; }

        [JsonProperty("xii_result_type")]
        public string XiiResultType { get; set; }

        [JsonProperty("xii_grade_point")]
        public object XiiGradePoint { get; set; }

        [JsonProperty("xii_cgpa_obtained")]
        public object XiiCgpaObtained { get; set; }

        [JsonProperty("xii_cgpa")]
        public object XiiCgpa { get; set; }

        [JsonProperty("xii_maximum_marks")]
        public string XiiMaximumMarks { get; set; }

        [JsonProperty("xii_marks_obtained")]
        public string XiiMarksObtained { get; set; }

        [JsonProperty("xii_percentage")]
        public string XiiPercentage { get; set; }

        [JsonProperty("university_programme_selection")]
        public UniversityProgrammeSelection UniversityProgrammeSelection { get; set; }

        [JsonProperty("test_paper_1")]
        public string TestPaper1 { get; set; }

        [JsonProperty("test_paper_medium_1")]
        public string TestPaperMedium1 { get; set; }

        [JsonProperty("test_paper_2")]
        public string TestPaper2 { get; set; }

        [JsonProperty("test_paper_medium_2")]
        public string TestPaperMedium2 { get; set; }

        [JsonProperty("test_paper_3")]
        public string TestPaper3 { get; set; }

        [JsonProperty("test_paper_medium_3")]
        public string TestPaperMedium3 { get; set; }

        [JsonProperty("test_paper_4")]
        public string TestPaper4 { get; set; }

        [JsonProperty("test_paper_medium_4")]
        public string TestPaperMedium4 { get; set; }

        [JsonProperty("test_paper_5")]
        public string TestPaper5 { get; set; }

        [JsonProperty("test_paper_medium_5")]
        public string TestPaperMedium5 { get; set; }

        [JsonProperty("test_paper_6")]
        public string TestPaper6 { get; set; }

        [JsonProperty("test_paper_medium_6")]
        public string TestPaperMedium6 { get; set; }

        [JsonProperty("test_paper_7")]
        public string TestPaper7 { get; set; }

        [JsonProperty("test_paper_medium_7")]
        public string TestPaperMedium7 { get; set; }

        [JsonProperty("test_paper_8")]
        public string TestPaper8 { get; set; }

        [JsonProperty("test_paper_medium_8")]
        public string TestPaperMedium8 { get; set; }

        [JsonProperty("test_paper_9")]
        public string TestPaper9 { get; set; }

        [JsonProperty("test_paper_medium_9")]
        public string TestPaperMedium9 { get; set; }

        [JsonProperty("test_paper_10")]
        public string TestPaper10 { get; set; }

        [JsonProperty("test_paper_medium_10")]
        public string TestPaperMedium10 { get; set; }

        [JsonProperty("cuet_result")]
        public Dictionary<string, string> CUETResult { get; set; }
    }

    public class UniversityProgrammeSelection
    {
        [JsonProperty("Cluster University of Srinagar")]
        public string SelectedProgramme { get; set; }

        public List<string> SelectedCourses
        {
            get
            {
                if (SelectedProgramme.IsNullOrEmpty())
                    return null;
                return SelectedProgramme.Split(',').ToList();

            }
        }
    }
}
