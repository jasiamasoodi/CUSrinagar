using System.ComponentModel;

namespace CUSrinagar.Enums
{
    public enum Semester
    {
        [Description("Semester-I")]
        Semester1 = 1,
        [Description("Semester-II")]
        Semester2 = 2,
        [Description("Semester-III")]
        Semester3 = 3,
        [Description("Semester-IV")]
        Semester4 = 4,
        [Description("Semester-V")]
        Semester5 = 5,
        [Description("Semester-VI")]
        Semester6 = 6,
        [Description("Semester-VII")]
        Semester7 = 7,
        [Description("Semester-VIII")]
        Semester8 = 8,
        [Description("Semester-IX")]
        Semester9 = 9,
        [Description("Semester-X")]
        Semester10 = 10
    }
    public enum Gender
    {
        [Description("Male")]
        Male = 1,
        [Description("Female")]
        Female = 2,
        [Description("Others")]
        Other = 3
    }
    /// <summary>
    /// University Category
    /// </summary>
    public enum Programme
    {
        [Description("Graduation")]
        UG = 1,
        [Description("Post-Graduation")]
        PG = 2,
        [Description("Integrated")]
        IG = 3,
        [Description("Honours")]
        HS = 4,
        [Description("Engineering")]
        Engineering = 5,
        [Description("Professional")]
        Professional = 6
    }
    public enum AwardType
    {
        [Description("Practical")]
        Practical,
        [Description("Theory")]
        Theory

    }

    /// <summary>
    /// Backend/Tables category.
    /// </summary>
    public enum PrintProgramme
    {
        //4 digit is reserved
        [Description("Graduation")]
        UG = 1,
        [Description("Post-Graduation")]
        PG = 2,
        [Description("Integrated / Honours / Professional / Engineering")]
        IH = 3,
        [Description("B.Ed")]
        BED = 5,
    }

    public enum Module
    {
        PersonalInformation,
        Address,
        ExamForm,
        SelectedCombination,
        AdditionalSubjects,
        PreviousQualification,
        CoursesApplied,
        ReEvaluation,
        Grievance,
        ExaminationPayment,
        Result
    }

    public enum ResultStatus
    {
        [Description("Later On")]
        LO = -3,
        [Description("Absent")]
        Absent = -2,
        [Description("Not Available")]
        NA = -1,
        [Description("-")]
        NULL,
        [Description("Fail")]
        F = 1,
        [Description("Pass")]
        P = 2,
        [Description("Re-Appear")]
        ReAppear = 3,
        [Description("Division Improve")]
        DivisionImprove = 4,
        [Description("Not Applicable")]
        NotApplicable = 5,
        [Description("Marks Not Assigned")]
        NotAssigned = 6,
        //possibility:min/max marks not well defined
        [Description("Discrepancy")]
        Discrepancy = 7

    }

    public enum Scheme
    {
        Semester = 1,
        Year = 2
    }

    public enum LogicalOperator
    {
        [Description(" AND ")]
        AND = 1,
        [Description(" OR ")]
        OR = 2
    }

    public enum CourseCategory
    {
        Professional = 1,
        [Description("Non-Professional")]
        NonProfessional = 2
    }

    public enum SQLOperator
    {
        [Description(" = ")]
        EqualTo,
        [Description(" <> ")]
        NotEqualTo,
        [Description(" LIKE ")]
        Like,
        [Description(" IN ")]
        In,
        [Description(" NOT IN ")]
        NotIn,
        [Description(" < ")]
        LessThan,
        [Description(" > ")]
        GreaterThan,
        [Description(" <= ")]
        LessThanEqualTo,
        [Description(" >= ")]
        GreaterThanEqualTo,
        [Description(" LIKE ")]
        Contains,
        [Description(" IS NULL ")]
        ISNULL,
        [Description(" IS NOT NULL ")]
        ISNotNULL,
    }

    public enum RecordState
    {
        New,
        Old,
        Dirty
    }

    public enum Status
    {
        [Description("In-Active")]
        InActive = 0,
        Active = 1
    }
    public enum StudentSelectionStatus
    {
        NotSelected = 0,
        Joined = 1,
        Rejected = 2,
        Provisional = 3,
        CertificatesUploaded = 4,
        Verified_MakePayment = 5,
        CollegePreferenceUpdated = 6,
        AllowedForCounseling = 7,
        AppliedForCounseling = 8,
        [Description("Provisional(Self Finance)")]
        Provisional_SF = 9,
        [Description("CertificatesUploaded(Self Finance)")]
        CertificatesUploaded_SF = 10,
        [Description("Verified_MakePayment(Self Finance)")]
        Verified_MakePayment_SF = 11,
        [Description("Joined(Self Finance)")]
        Joined_SF = 12,
    }

    public enum FormStatus
    {
        [Description("In Process")]
        InProcess = 1,
        [Description("Submitted")]
        Submitted = 2,
        [Description("Rejected")]
        Rejected = 3,
        [Description("Accepted")]
        Accepted = 4,
        [Description("Cancelled")]
        Cancelled = 5,
        [Description("Disqualified")]
        Disqualified = 6,
        [Description("Absent")]
        Absent = 7,
        [Description("Appeared in Entrance")]
        AppearedInEntrance = 8,
        [Description("Selected")]
        Selected = 9,
        [Description("Fee Paid")]
        FeePaid = 10,
        PaymentSuccessful = 11,
        PaymentUnsuccessful = 12,
        [Description("Cancel Admission on students request")]
        CancelRegistration = 13,
        [Description("Not appeared in internals")]
        InEligible = 14,
        UpdatedManually = 15,
        BlockSemesterAdmitCard = 16,
        Refunded = 17,
        [Description("Admission cancelled due to continuous absentee")]
        AutoAdmissionCancel = 18,

    }

    public enum FormType
    {
        [Description("ReEvaluation")]
        ReEvaluation = 1,
        [Description("Xerox")]
        Xerox = 2
    }

    public enum NotificationType
    {
        Admission = 1,
        Jobs = 2,
        Tenders = 3,
        Examination = 4,
        General = 5,
        Result = 6,
        Datesheet = 7,
        Circulars = 8,
        Orders = 9
    }
    public enum SMSServiceType
    {
        singlemsg,
        bulkmsg,
        unicodemsg,
        otpmsg,
        unicodeotpmsg
    }

    public enum SMSRegarding
    {
        None = 0,
        [Description("Entrance Test")]
        EntranceTest = 1,
        [Description("Relocate Entrance Center")]
        RelocateEntranceCenter = 2,
        [Description("Entrance Selection List")]
        EntranceSelectionList = 3,
        [Description("Semester (Registered Students)")]
        Semester = 4,
        [Description("Entrance (By Entrance RollNo)")]
        ByEntranceRollNo = 5,
        Other = 7
    }

    public enum MarksFor
    {
        Theory = 1,
        Practical = 2
    }
    public enum PaymentType
    {
        Online = 1,
        Offline = 2,
        Reconciled = 3,
        ReFund = 4
    }
    public enum PaymentModuleType
    {
        None = -1,
        [Description("Registration Fee(Admission)")]
        Admission = 1,
        [Description("ReEvaluation Forms")]
        ReEvaluation = 2,
        [Description("Examination Forms")]
        Examination = 3,
        [Description("Xerox Forms")]
        Xerox = 4,
        [Description("Registration Fee(Self Finance)")]
        SelfFinanced = 5,
        [Description("Semester Admission")]
        SemesterAdmission = 6,
        [Description("Registration Cancelled")]
        CancelRegistration = 7,
        [Description("Migration ")]
        Migration = 8,
        [Description("Add More Admission Courses")]
        AddMoreAdmCourses = 9,
        [Description("Certificate Courses Admission")]
        CertificateCoursesAdmission = 10,
        [Description("Semester I-II Admission (For Reconcile Purpose only)")]
        FirstSemesterAdmissionReconcileOnly = 11,
        AttemptCertificate = 12,
        CertificateVerification = 13,
        [Description("NEP Registration Fee(Admission)")]
        NEPAdmission = 14,
        [Description("Issuing of Degree Certificate")]
        IssuingOfDegreeCertificate = 15,
        [Description("Correction Form")]
        CorrectionForm = 16,
    }
    public enum MigrationE
    {
        [Description("Before Completion of Degree")]
        CancelRegistration = 1,
        [Description("Intra College Migration")]
        IntraCollegeMigration = 2,
        [Description("After Completion of Degree")]
        PassoutMigration = 3
    }
    public enum GrievanceStatus
    {
        [Description("Received/Pending")]
        Received = 1,
        [Description("Forwarded to Concerned Person")]
        Forwarded = 2,
        [Description("Resolved")]
        Resolved = 3
    }


    public enum GrievanceCategory
    {
        [Description("Teaching/Class Work")]
        TeachingClassWork = 1,
        [Description("Admission")]
        Admission = 2,
        [Description("Syllabus")]
        Syllabus = 3,
        [Description("Examination")]
        Examination = 4,
        [Description("Result")]
        Result = 5,
        [Description("Other")]
        Other = 6
    }
    public enum SubjectType
    {
        [Description("None")]
        None = 0,
        [Description("Core")]
        Core = 1,
        [Description("Skill Enhancement")]
        SEC = 3,
        [Description("Generic Elective")]
        GE = 4,
        [Description("MIL")]
        MIL = 5,
        [Description("Ability Enhancement")]
        AE = 6,
        [Description("Open Elective")]
        OE = 7,
        [Description("Discipline Specific Elective")]
        DSE = 8,
        FirstSemesterExclusion = 9,
        [Description("Discipline Centric Elective")]
        DCE = 10,
        [Description("Optional Core")]
        OC = 11,
        [Description("Lab")]
        Lab = 12,
        [Description("Practical")]
        Practical = 13,
        [Description("Workshop")]
        Workshop = 14,
        [Description("Major")]
        Major = 15,
        [Description("Minor Inter Disciplinary")]
        MID = 16,
        [Description("Minor Vocational")]
        MVoc = 17,
        [Description("Multi Disciplinary")]
        MDisc = 18,
        [Description("Internship")]
        Internship = 19,
        [Description("Seminar")]
        Seminar = 20,
        [Description("Research")]
        Research = 21,
        [Description("Value Added Courses")]
        VAC = 22,
        [Description("Basic Science Course")]
        BSC = 30,
        [Description("Engineering Science Course")]
        ESC = 31,
        [Description("Humanity Science and Management Course")]
        HSMC = 32,
        [Description("Course Type-01")]
        CourseType_01 = 33,
        [Description("Course Type-02")]
        CourseType_02 = 34,
        [Description("Course Type-03")]
        CourseType_03 = 35


    }
    public enum NomenclatureSubjectType
    {
        [Description("Core")]
        C,
        [Description("Skill Enhancement")]
        S,
        [Description("Generic Elective")]
        G,
        [Description("Open Elective")]
        O,
        [Description("Descipline Specific Elective")]
        D
    }
    public enum ReExamType
    {
        Backlog = 1,
        DivisionImprovement = 2
    }

    public enum MarksType
    {
        TheoryMarks = 1,
        TheoryAttendance = 2,
        PracticalMarks = 3,
        PracticalAttendance = 4,
        PracticalInternalMarks = 5,
        PracticalExternalMarks = 6
    }

    public enum ExaminationCourseCategory
    {
        //[Description("None")]
        //None = 0,
        [Description("Under Graduate")]
        UG = 1,
        [Description("B.Ed")]
        BED = 2,
        [Description("Post Graduation")]
        PG = 3,
        [Description("M.Ed")]
        MED = 4,
        [Description("Integrated/Honors/Professional")]
        IH = 5,
        [Description("Engineering")]
        ENG = 6,
        [Description("Integrated B.Ed-M.Ed")]
        IBM = 7
    }
    public enum AwardReceivedStatus
    {
        [Description("Received")]
        Received,
        [Description("Not Received")]
        NotReceived,
        [Description("Partitally Received")]
        PartitallyReceived
    }

    public enum SelfFinancedDownloadType
    {
        [Description("By Course")]
        ByCourse,
        [Description("By Individual")]
        ByIndividual
    }

    public enum EntranceListType
    {
        Key = 3,
        [Description("Revised Key")]
        RevisedKey = 6,
        [Description("Subject Points")]
        SubjectPoints = 9,
        [Description("Combined Points")]
        CombinedPoints = 12,
        [Description("Selection List")]
        SelectionList = 15
    }
    public enum MarksIsPartOf
    {
        [Description("Internal")]
        Internal = 1,
        [Description("External")]
        External = 2,
        [Description("Independent")]
        Independent = 3,
    }
    public enum AwardModuleType
    {
        [Description("College")]
        College = 1,
        [Description("University")]
        University = 2,
    }

    public enum CertificateCourses
    {
        [Description("Certificate Course in Arabic")]
        CCA = 1,
        [Description("Diploma Course in Arabic")]
        DA = 2
    }
    public enum ProfessorStatus
    {
        [Description("No-Response")]
        No_Response = 0,
        [Description("Class-Engaged")]
        Class_Engaged = 1,
        [Description("Class-NotEngaged")]
        Class_NotEngaged = 2
    }
    public enum CRStatus
    {
        [Description("No-Response")]
        No_Response = 0,
        [Description("Class-Engaged")]
        Class_Engaged = 1,
        [Description("Class-NotEngaged")]
        Class_NotEngaged = 2
    }

    public enum EmployeeStatus
    {
        [Description("Active")]
        Active,
        [Description("InActive")]
        InActive,
        [Description("Transferred")]
        Transferred,
    }
    public enum FTFileStatus
    {
        Created = 0,
        Closed = 1,
        ReOpened = 2,
        Forwarded = 3,
        Received = 4,
        Reverted = 5,
        TrackAllYourFiles = 6
    }
    public enum EmployeeType
    {
        Gazetted = 1,
        Non_Gazetted = 2,
        Class_4 = 3,
        Contractual = 4,
        Need_Basis = 5
    }
    public enum SalaryCalculationColumn
    {
        Allowance = 1,
        Deduction = 2
    }

    public enum SemesterCentersCategory
    {
        All = 0,
        [Description("Regular only")]
        RegularOnly = 1,
        [Description("Backlog only")]
        BacklogsOnly = 2
    }

    public enum MasterCourseCategory
    {
        [Description("None")]
        None = 0,
        [Description("Under Graduate")]
        UG = 1,
        [Description("BED")]
        BED = 2,
        [Description("Post Graduation")]
        PG = 3,
        [Description("MED")]
        MED = 4,
        [Description("Integrated")]
        IG = 5,
        [Description("Engineering")]
        ENG = 6,
        [Description("Honors")]
        HN = 7,
        [Description("Professional")]
        PROF = 8,

    }

    public enum EntranceExcelListType
    {
        [Description("Merit List")]
        MeritList = 1,
        [Description("Merit List (With Academics)")]
        MeritListAcademic = 2,
    }

    public enum EntrancePDFListType
    {
        [Description("General Merit List")]
        GeneralMeritList = 1,
        [Description("Combined Merit List (With CAT)")]
        CombinedMeritList = 2,
        [Description("Combined Merit List (With Academics)")]
        CombinedMeritListAcademics = 3
    }

    public enum CertificateType
    {
        Provisional = 1,
        Character = 2,
        DOB = 3,
        MarksCard = 4,
        Category = 5
    }

    public enum VerificationStatus
    {
        Pending = 1,
        Verified = 2,
        Rejected = 3
    }

    public enum SGPAType
    {
        Type1 = 1,
        Type2 = 2
    }

    /// <summary>
    /// Do not add or change values to this enum
    /// </summary>
    public enum BillDeskPaymentType
    {
        ADM,
        EXM,
        OTH
    }

    public enum ExamFormListType
    {
        All = -1,
        [Description("Backlog & Division Improvement")]
        Backlog = 0,
        Regular = 1
    }
    public enum ResultAnomalyType
    {
        UnFair = 1
    }
    public enum NEPProgrammesAdmTitle
    {
        [Description("4-Year (3+1) UG (Honor’s) Programmes")]
        Year_4_UG_Honors_Programmes = 1,
        [Description("5-Year Bachelor's/Master's Integrated PG Programmes")]
        Year_5_Integrated_PG_Programmes = 3,
        [Description("Post-Graduation Programmes")]
        Post_Graduation_Programmes = 2
    }

    public enum MigrationIssueStatus
    {
        NotIssued = 0,
        [Description("Issued")]
        Issued = 1,
        [Description("Duplicate Issued")]
        DuplicateIssued = 2
    }
    public enum TranscriptFilters
    {
        None = 0,
        [Description("Extra Credits")]
        IsExtraCredit = 1,
        [Description("Exit")]
        IsExit = 2,
        [Description("Lateral Entry")]
        IsLateralEntry = 3,
        [Description("Re Evaluation")]
        IsReevaluation = 4,
        [Description("Division Improvement")]
        IsDivisionImprovement = 5
    }
    #region Daily Lecture Feedback
    public enum FacultyTypes
    {
        [Description("Academic Arrangement")]
        AcademicArrangement = 1,
        [Description("Permanent")]
        Permanent = 2
    }
    public enum ModeOfTeachings
    {
        Online = 1,
        Offline = 2
    }

    public enum Rating
    {
        [Description("Extremely Poor")]
        ExtremelyPoor = 1,
        [Description("Very Poor")]
        VeryPoor = 2,
        Poor = 3,
        [Description("Somehow Tolerable")]
        SomehowTolerable = 4,
        Moderate = 5,
        [Description("Moderately Good")]
        ModeratelyGood = 6,
        Good = 7,
        [Description("Very Good")]
        VeryGood = 8,
        [Description("Extremely Good")]
        ExtremelyGood = 9,
    }
    public enum MaterialType
    {
        No = 1,
        PPT = 2,
        PhotoState = 3,
        PDF = 4,
        DOC = 5,
        Excel = 6,
        Images = 7,
        [Description("Written Notes")]
        WrittenNotes = 8,
    }

    public enum BillStatus
    {
        InProcess = 0,
        Assigned = 1,
        Accepted = 2,
        Rejected = 3,
        [Description("Email Sent with Papers")]
        EmailSent = 4,

        [Description("Under Verification")]
        UnderVerification = 5,

        Verified = 6,

        [Description("Dispatched to Accounts Section")]
        DispatchedToAccountsSection = 7,

        [Description("Error in Bill")]
        ErrorInBill = 8,

        [Description("External Awards Uploaded")]
        ExternalAwardsUploaded = 9,

        [Description("Paid and Closed")]
        Paid = 10,
    }

    public enum SetterFileType
    {
        [Description("Paper Pattern")]
        PaperPattern = 0,

        [Description("Sample Paper")]
        SamplePaper = 1
    }

    public enum BillType
    {
        [Description("Paper Setter Bill")]
        PaperSetterBill = 0,

        [Description("Evaluator Bill")]
        EvaluatorBill = 1
    }
    #endregion
}
