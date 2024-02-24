
using System.ComponentModel;

namespace CUSrinagar.Enums
{
    public enum AppRoles
    {
        University = 1,

        College = 2,

        [Description("College Assistant Professor")]
        College_AssistantProfessor = 3,

        [Description("Registrar")]
        University_Registrar = 4,

        [Description("University Office Assistant")]
        University_OfficeAssistant = 5,

        [Description("College Clerk")]
        College_Clerk = 6,

        Student = 7,

        [Description("Principal")]
        College_Principal = 8,

        [Description("Dean")]
        University_Dean = 9,

        [Description("Deputy Controller")]
        University_DeputyController = 10,

        [Description("Evaluator")]
        University_Evaluator = 11,

        [Description("College - Edit Student")]
        College_EditStudent = 12,

        [Description("Coordinator")]
        University_Coordinator = 13,

        [Description("GrievanceFeedback")]
        University_GrievanceFeedback = 14,

        [Description("University Notifications")]
        University_Notification = 15,

        [Description("University Statistics")]
        University_Statistics = 16,

        [Description("University Examination Controller")]
        University_EController = 17,

        [Description("Allow BOPEE Registration")]
        AllowBOPEERegistration = 18,

        [Description("Class Representative")]
        College_ClassRepresentative = 19,

        [Description("CAO")]
        University_CAO = 20,

        [Description("Re-Evaluations")]
        University_ReEvaluations = 21,

        [Description("File Tracking")]
        University_FileTracking = 22,

        [Description("Salary")]
        University_Salary = 23,

        [Description("Payment Reconcile")]
        University_PaymentReconcile = 24,

        [Description("Results")]
        University_Results = 25,
        [Description("Student Info")]
        University_StudentProfile = 26,
        [Description("Student Info Full")]
        University_StudentInfo = 27,

        [Description("TransScripts")]
        University_TransScript = 28,

        [Description("Degree Certificate")]
        DegreeCertificate = 29,

        [Description("Award Settings")]
        AwardSettings = 30,

        [Description("View-TransScripts")]
        View_TransScript = 31,

        [Description("University Feedback")]
        Feedback = 32,

        [Description("Rapid Entry")]
        RapidEntry = 33,

        [Description("Result Revaluation")]
        ResultRevaluation = 34,

        [Description("College RR")]
        College_RR = 35,

        [Description("Subject List")]
        SubjectList = 36,

        [Description("External Award Pending List")]
        ExtPendingList = 37,

        [Description("Enrollments")]
        Enrollment = 38,

        [Description("MarksSheet List")]
        MarksSheetList = 39,

        [Description("Assign Combination")]
        College_AssignCombination = 40,

        [Description("University Role RR")]
        University_RR = 41,

        [Description("College Statistics")]
        College_Statistics = 42,

        [Description("eGov CUET")]
        eGovCUET = 43,

        [Description("Publish Result")]
        PublishResult = 44,

        [Description("University Setter & Evaluator Bills")]
        University_Bills = 45,

        [Description("Paper Setter Bills")]
        PaperSetterBills = 46,

        [Description("Evaluator Bills")]
        EvaluatorBills = 47,

        [Description("Result Gazette")]
        ResultGazette = 48,

    }
}
