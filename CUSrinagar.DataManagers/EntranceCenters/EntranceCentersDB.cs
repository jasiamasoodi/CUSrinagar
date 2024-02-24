using CUSrinagar.Enums;
using CUSrinagar.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using CUSrinagar.Extensions;
using Terex;
using Newtonsoft.Json;
using System.Linq;

namespace CUSrinagar.DataManagers
{
    public class EntranceCentersDB
    {
        #region DisplayCenterDetails
        public List<DisplayEntranceCenterAllotmentDetail> GetCenterAllotmentDetails(Guid Center_ID)
        {
            SqlCommand cmd = new SqlCommand(@"SELECT 
                                                MAX(CollegeFullName) CollegeFullName,
                                                MAX(CenterCode) CenterCode,
                                                COUNT(ARGCentersAllotmentMaster.Center_ID) TotalStudentsAssigned,
                                                MAX(CourseFullName) CourseFullName,
                                                EntranceDate 
                                                FROM ARGCentersAllotmentMaster
                                                JOIN ARGEntranceCentersMaster ON ARGEntranceCentersMaster.Center_ID=ARGCentersAllotmentMaster.Center_ID
                                                JOIN ADMCollegeMaster ON ADMCollegeMaster.College_ID = ARGEntranceCentersMaster.College_ID
                                                JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = ARGCentersAllotmentMaster.Course_ID
                                                JOIN
                                                (
	                                                SELECT Student_ID FROM ARGPersonalInformation_IH WHERE EntranceRollNo IS NOT NULL
	                                                UNION
	                                                SELECT Student_ID FROM ARGPersonalInformation_PG WHERE EntranceRollNo IS NOT NULL
	                                                UNION
	                                                SELECT Student_ID FROM ARGPersonalInformation_UG WHERE EntranceRollNo IS NOT NULL
                                                ) p ON ARGCentersAllotmentMaster.Entity_ID=p.Student_ID
                                                WHERE ARGEntranceCentersMaster.Center_ID=@Center_ID
                                                GROUP BY ARGCentersAllotmentMaster.Center_ID,ARGCentersAllotmentMaster.Course_ID,EntranceDate
                                                ORDER BY CollegeFullName,CenterCode,CourseFullName,EntranceDate DESC");
            cmd.Parameters.AddWithValue("@Center_ID", Center_ID);
            return new MSSQLFactory().GetObjectList<DisplayEntranceCenterAllotmentDetail>(cmd);
        }

        #endregion

        #region AssignRollNosInBulk
        public bool BatchExists(PrintProgramme printProgramme, int Batch)
        {
            return new MSSQLFactory().ExecuteScalar<int>($"SELECT COUNT(Student_ID) FROM  ARGPersonalInformation_{printProgramme.ToString()} WHERE Batch={Batch}") > 0;
        }
        public bool IsAdmissionStillOpen(PrintProgramme printProgramme)
        {
            return new MSSQLFactory().ExecuteScalar<int>($"SELECT COUNT(Course_ID) FROM ADMCourseMaster WHERE PrintProgramme={(short)printProgramme} AND RegistrationOpen=1") > 0;
        }
        public bool CheckIfRollnosAreAlreadyAssigned(PrintProgramme printProgramme, int Batch)
        {
            return new MSSQLFactory().ExecuteScalar<int>($"SELECT COUNT(Student_ID) FROM ARGPersonalInformation_{printProgramme.ToString()} WHERE Batch={Batch} AND EntranceRollNo IS NOT NULL") > 0;
        }

        public int AssignBulkRollNos(PrintProgramme printProgramme, int Batch, Guid Course_ID, int StartRollNos)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            return new MSSQLFactory().ExecuteNonQuery($@" WITH UpdateData  AS(
	                                                     SELECT
	                                                     A.Student_ID,ROW_NUMBER() OVER(ORDER BY aa.District) AS RN 
	                                                     FROM ARGCoursesApplied_{printProgramme.ToString()} A
	                                                     JOIN ARGPersonalInformation_{printProgramme.ToString()} P ON P.Student_ID = A.Student_ID
                                                         JOIN dbo.ARGStudentAddress_{printProgramme.ToString()} aa ON aa.Student_ID = P.Student_ID
	                                                     WHERE P.Batch={Batch} AND P.FormStatus={(short)FormStatus.FeePaid} AND EntranceRollNo IS NULL 
                                                         AND A.Course_ID='{Course_ID}'
                                                     )
                                                     UPDATE P SET P.EntranceRollNo=({StartRollNos}+UpdateData.RN) FROM ARGPersonalInformation_{printProgramme.ToString()} P
                                                     JOIN UpdateData ON UpdateData.Student_ID = P.Student_ID");
        }

        public List<EntranceFormsCourseCount> GetEntranceFormsCourseCounts(PrintProgramme printProgramme, int Batch)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            return new MSSQLFactory().GetObjectList<EntranceFormsCourseCount>($@"SELECT COUNT(Course_ID) TotalForms,Course_ID FROM ARGCoursesApplied_{printProgramme.ToString()} P
			                                                        JOIN ARGPersonalInformation_{printProgramme.ToString()} A ON P.Student_ID = A.Student_ID
			                                                        WHERE batch={Batch} AND FormStatus={(short)FormStatus.FeePaid} 
			                                                        GROUP BY Course_ID");
        }

        public int GetMaxEntranceRollNo(int Batch, PrintProgramme printProgramme)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            return new MSSQLFactory().ExecuteScalar<int>($"SELECT CAST(MAX(EntranceRollNo) AS INT) FROM dbo.ARGPersonalInformation_{printProgramme.ToString()} WHERE Batch={Batch} AND EntranceRollNo IS NOT NULL;");
        }
        #endregion


        #region IndividualCenterAssignment And AdmitCards

        public List<ARGEntranceCentersMaster> GetCenters()
        {
            return new MSSQLFactory().GetObjectList<ARGEntranceCentersMaster>("SELECT * FROM ARGEntranceCentersMaster WHERE [Status]=1 AND IsEntrance=1");
        }
        public List<DropDownOptLabelGeneral> GetCenterList(bool IsEntrance)
        {
            return new MSSQLFactory().GetObjectList<DropDownOptLabelGeneral>($@"SELECT CONVERT(nvarchar(50), Center_ID) AS Value, CenterCode AS Text, CollegeFullName AS DataGroupField FROM ARGEntranceCentersMaster 
                                                                            JOIN ADMCollegeMaster ON ADMCollegeMaster.College_ID = ARGEntranceCentersMaster.College_ID
                                                                            WHERE ARGEntranceCentersMaster.Status = 1 AND ARGEntranceCentersMaster.IsEntrance={(IsEntrance ? 1 : 0)}  ORDER BY CollegeCode ASC,CenterCode ASC");
        }
        public ARGCentersAllotmentMaster GetCentersAllotment(Guid Student_ID, Guid Course_ID)
        {
            string Qurery = @"SELECT * FROM ARGCentersAllotmentMaster
                            WHERE Entity_ID = @Student_ID AND Course_ID = @Course_ID ORDER BY EntranceDate ASC";
            SqlCommand sqlCommand = new SqlCommand
            {
                CommandText = Qurery
            };
            sqlCommand.Parameters.AddWithValue("@Course_ID", Course_ID);
            sqlCommand.Parameters.AddWithValue("@Student_ID", Student_ID);
            return new MSSQLFactory().GetObject<ARGCentersAllotmentMaster>(sqlCommand);
        }

        public int Save(ARGCentersAllotmentMaster aRGCentersAllotmentMaster)
        {
            return new MSSQLFactory().InsertRecord(aRGCentersAllotmentMaster);
        }

        public int UpdateEntranceRollNoAndFormStatus(Guid Student_ID, FormStatus _FormStatus, PrintProgramme printProgramme)
        {
            string query = $@"UPDATE ARGPersonalInformation_{printProgramme.ToString()} SET 
                            EntranceRollNo = (SELECT ISNULL(MAX(EntranceRollNo),'{DateTime.Now.Year.ToString().Substring(2, 2)}0000') + 1 FROM ARGPersonalInformation_{printProgramme.ToString()} 
                            WHERE EntranceRollNo IS NOT NULL),
                            FormStatus = @FormStatus
                            WHERE Student_ID = @Student_ID";
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.AddWithValue("@Student_ID", Student_ID);
            cmd.Parameters.AddWithValue("@FormStatus", _FormStatus);
            cmd.CommandText = query;
            return new MSSQLFactory().ExecuteNonQuery(cmd);
        }

        public bool CheckIfAlreadyHasEntranceRollNo(Guid Student_ID, PrintProgramme printProgramme)
        {
            return new MSSQLFactory().ExecuteScalar<int>($"SELECT COUNT(Student_ID) FROM ARGPersonalInformation_{printProgramme.ToString()} WHERE Student_ID='{Student_ID}' AND EntranceRollNo IS NULL") > 0;
        }

        public bool CheckIfEntranceCenterAlreadyAssinged(Guid Student_ID)
        {
            return new MSSQLFactory().ExecuteScalar<int>($"SELECT COUNT(CenterAllowment_ID) FROM ARGCentersAllotmentMaster WHERE Entity_ID ='{Student_ID}'") > 0;
        }

        public int DeleteCentersForIndividual(Guid Student_ID)
        {
            return new MSSQLFactory().ExecuteNonQuery($"DELETE FROM dbo.ARGCentersAllotmentMaster WHERE Entity_ID='{Student_ID}'");
        }
        #endregion

        #region AssignCenterInBulk       

        public List<AssignEntranceCentersAs> GetDispalyAvaliableCenters()
        {
            return new MSSQLFactory().GetObjectList<AssignEntranceCentersAs>($@"SELECT Center_ID,
                                                                CenterCode,CollegeFullname+', '+ADMCollegeMaster.Address AS CollegeFullname,
                                                                CASE
                                                                    WHEN
                                                                    (
                                                                        SELECT COUNT(ARGCentersAllotmentMaster.CenterAllowment_ID)
                                                                        FROM ARGCentersAllotmentMaster
                                                                        WHERE Center_ID = ARGEntranceCentersMaster.Center_ID
                                                                    ) > 0 THEN
                                                                        1
                                                                    ELSE
                                                                        0
                                                                END AS HasAlreadySomeStudents
                                                                FROM ARGEntranceCentersMaster
                                                                JOIN ADMCollegeMaster ON ADMCollegeMaster.College_ID = ARGEntranceCentersMaster.College_ID
                                                                WHERE ARGEntranceCentersMaster.Status = 1 AND  ARGEntranceCentersMaster.IsEntrance=1
                                                                ORDER BY CollegeFullName,CenterCode;");
        }

        public int GetTotalEntranceFormsAvailable(short Batch, Guid Course_ID, PrintProgramme category)
        {
            category = new GeneralFunctions().MappingTable(category);
            Guid GeneralAppitudeCourse_ID =
                         Guid.Parse(Convert.ToString(ConfigurationManager.AppSettings["GeneralApitudeCourse_ID"]));



            Tuple<string, List<Guid[]>> tuple = GetCourseGrouping(PrintProgramme.IH);

            string Course_IDs = $"{Course_ID}";
            if (tuple.Item1.ToLower().Contains(Course_ID.ToString().ToLower()))
            {
                Course_IDs = tuple.Item2.Find(x => (x[0] == Course_ID || x[1] == Course_ID))?.ToSingleString().Replace("|", "','");
            }


            string TSQL = $@"SELECT COUNT(S.Student_ID) from ( SELECT DISTINCT P.Student_ID FROM ARGPersonalInformation_{category.ToString()} P
                                                    JOIN ARGCoursesApplied_{category.ToString()} A ON A.Student_ID = P.Student_ID
                                                    LEFT JOIN dbo.ARGCentersAllotmentMaster m ON m.Entity_ID = A.Student_ID AND m.Course_ID=A.Course_ID
                                                    WHERE P.Batch={Batch} AND A.Course_ID IN('{Course_IDs}') AND P.FormStatus={(short)FormStatus.FeePaid} AND P.EntranceRollNo IS NOT NULL 
                                                    AND m.CenterAllowment_ID IS NULL) s";
            if (GeneralAppitudeCourse_ID == Course_ID)
                TSQL = $@"SELECT COUNT(T.Total) FROM (
                            SELECT COUNT(P.Student_ID) Total FROM ARGPersonalInformation_{category.ToString()} P
                            JOIN ARGCoursesApplied_{category.ToString()} A ON A.Student_ID = P.Student_ID
                            WHERE P.Batch = {Batch} AND A.Course_ID <> 'FC32E138-4EE2-4DA2-9453-5C8368180BC3' AND P.FormStatus = {(short)FormStatus.FeePaid}
                            AND P.EntranceRollNo IS NOT NULL
                            GROUP BY P.Student_ID) T";


            return new MSSQLFactory().ExecuteScalar<int>(TSQL);
        }

        public List<SelectListItem> GetCoursesDDL(PrintProgramme Id, short Id1)
        {
            Id = new GeneralFunctions().MappingTable(Id);
            return new MSSQLFactory().GetObjectList<SelectListItem>
                                        ($@"SELECT s.[Value],
                                                   s.[Text]
                                            FROM
                                            (
                                                SELECT DISTINCT
                                                       CAST(A.Course_ID AS NVARCHAR(50)) AS [Value],
                                                       c.CourseFullName AS [Text],
                                                       c.GroupName,
                                                       c.Programme,
                                                       c.CourseFullName
                                                FROM ARGPersonalInformation_{Id.ToString()} P
                                                    JOIN ARGCoursesApplied_{Id.ToString()} A
                                                        ON A.Student_ID = P.Student_ID
                                                    JOIN ADMCourseMaster c
                                                        ON c.Course_ID = A.Course_ID
                                                WHERE P.Batch = {Id1}
                                            ) s
                                            ORDER BY s.GroupName,
                                                     s.Programme,
                                                     s.CourseFullName;");
        }

        public List<SelectListItem> GetCoursesDDL(Programme Id, short Id1)
        {
            PrintProgramme _PrintProgramme = GeneralFunctions.ProgrammeToPrintProgrammeMapping(Id);
            return new MSSQLFactory().GetObjectList<SelectListItem>
                                        ($@"SELECT DISTINCT Cast(A.Course_ID AS nvarchar(50)) AS [Value],c.CourseFullName AS [Text] FROM ARGPersonalInformation_{_PrintProgramme.ToString()} P
                                        JOIN ARGCoursesApplied_{_PrintProgramme.ToString()} A ON A.Student_ID = P.Student_ID
                                        JOIN ADMCourseMaster c ON c.Course_ID = A.Course_ID
                                        WHERE P.Batch={Id1} AND c.Programme={(short)Id}");
        }

        public int AssignEntranceCenterInBulk(AssignCentersInBulk assignCentersInBulk)
        {
            assignCentersInBulk.PrintProgramme = new GeneralFunctions().MappingTable(assignCentersInBulk.PrintProgramme);

            Guid GeneralAppitudeCourse_ID =
                           Guid.Parse(Convert.ToString(ConfigurationManager.AppSettings["GeneralApitudeCourse_ID"]));

            string TSQL = "";
            if (GeneralAppitudeCourse_ID == assignCentersInBulk.Course_ID)
            {
                string districts = assignCentersInBulk.Districts.IsNullOrEmpty() ? "" : $"AND a.District IN ({assignCentersInBulk.Districts.ToIN()}) ";
                TSQL = $@"INSERT INTO ARGCentersAllotmentMaster
                            (
                                CenterAllowment_ID,
                                Entity_ID,
                                Course_ID,
                                Center_ID,
                                EntranceDate,
                                Time,
                                Appeared,
                                CreatedOn,
                                CreatedBy,
                                UpdatedOn,
                                UpdatedBy,
                                final)
                        SELECT NEWID(),P.Student_ID,
                        '{assignCentersInBulk.Course_ID}',
                        '{assignCentersInBulk.Center_ID}',
                        '{assignCentersInBulk.ExaminationDate.ToString("yyyy-MM-dd")}',
                        '{assignCentersInBulk.ExaminationTime.ToUpper()}',
                        0,
                        GETDATE(),
                        '{assignCentersInBulk.User_ID}',
                        NULL,
                        NULL,
                        0
	                    FROM ARGPersonalInformation_{assignCentersInBulk.PrintProgramme.ToString()} P
                        JOIN dbo.ARGStudentAddress_{assignCentersInBulk.PrintProgramme.ToString()} a ON a.Student_ID = P.Student_ID
						LEFT JOIN dbo.ARGCentersAllotmentMaster c ON p.Student_ID=c.Entity_ID AND C.Course_ID='{GeneralAppitudeCourse_ID}'
                        LEFT JOIN (
	                    SELECT  PP.Student_ID FROM ARGPersonalInformation_{assignCentersInBulk.PrintProgramme.ToString()} PP 
                        JOIN ARGCoursesApplied_{assignCentersInBulk.PrintProgramme.ToString()} AA ON AA.Student_ID = PP.Student_ID
	                    WHERE FormStatus=10 AND AA.Course_ID='FC32E138-4EE2-4DA2-9453-5C8368180BC3' AND PP.Batch={assignCentersInBulk.Year}
                        ) MED ON  MED.Student_ID = P.Student_ID
                            WHERE c.CenterAllowment_ID IS NULL AND EntranceRollNo IS NOT NULL AND FormStatus=10 AND P.Batch={assignCentersInBulk.Year} AND MED.Student_ID IS NULL {districts}
                            ORDER BY EntranceRollNo ASC OFFSET 0 ROWS FETCH NEXT {assignCentersInBulk.FetchNextCount} ROWS ONLY;";
            }
            else
            {
                TSQL = $@"INSERT INTO ARGCentersAllotmentMaster
                            (
                                CenterAllowment_ID,
                                Entity_ID,
                                Course_ID,
                                Center_ID,
                                EntranceDate,
                                Time,
                                Appeared,
                                CreatedOn,
                                CreatedBy,
                                UpdatedOn,
                                UpdatedBy,
                                final)
                                SELECT NEWID(),P.Student_ID,
                                   '{assignCentersInBulk.Course_ID}',
                                   '{assignCentersInBulk.Center_ID}',
                                   '{assignCentersInBulk.ExaminationDate.ToString("yyyy-MM-dd")}',
                                   '{assignCentersInBulk.ExaminationTime.ToUpper()}',
                                   0,
                                   GETDATE(),
                                   '{assignCentersInBulk.User_ID}',
                                   NULL,
                                   NULL,
                                   0
	                               FROM ARGPersonalInformation_{assignCentersInBulk.PrintProgramme.ToString()} P
                            JOIN ARGCoursesApplied_{assignCentersInBulk.PrintProgramme.ToString()} A ON A.Student_ID = P.Student_ID
                            LEFT JOIN dbo.ARGCentersAllotmentMaster m ON m.Entity_ID = A.Student_ID AND m.Course_ID=A.Course_ID
                            WHERE EntranceRollNo IS NOT NULL AND FormStatus=10 AND A.Course_ID='{assignCentersInBulk.Course_ID}' AND P.Batch={assignCentersInBulk.Year}
                                  AND m.CenterAllowment_ID IS NULL 
                            ORDER BY EntranceRollNo ASC OFFSET {assignCentersInBulk.OFFSETCountForEntrance} ROWS FETCH NEXT {assignCentersInBulk.FetchNextCount} ROWS ONLY;";
            }

            return new MSSQLFactory().ExecuteNonQuery(TSQL);
        }


        public int AssignEntranceCenterInBulkForGroupCourses(AssignCentersInBulk assignCentersInBulk, Guid newGroupCourse_ID)
        {
            assignCentersInBulk.PrintProgramme = new GeneralFunctions().MappingTable(assignCentersInBulk.PrintProgramme);

            string TSQL = $@"INSERT INTO ARGCentersAllotmentMaster
                            (CenterAllowment_ID,
                                Entity_ID,
                                Course_ID,
                                Center_ID,
                                EntranceDate,
                                Time,
                                Appeared,
                                CreatedOn,
                                CreatedBy,
                                UpdatedOn,
                                UpdatedBy,
                                final)
                                SELECT NEWID(),
                                       a.Student_ID,
                                       a.Course_ID,
                                       '{assignCentersInBulk.Center_ID}',
                                       '{assignCentersInBulk.ExaminationDate.ToString("yyyy-MM-dd")}',
                                       '{assignCentersInBulk.ExaminationTime.ToUpper()}',
                                       0,
                                       GETDATE(),
                                       '{assignCentersInBulk.User_ID}',
                                       NULL,
                                       NULL,
                                       0
                                FROM dbo.ARGCoursesApplied_{assignCentersInBulk.PrintProgramme.ToString()}  a
                                    JOIN
                                    (
                                        SELECT DISTINCT
                                               m.Entity_ID
                                        FROM dbo.ARGCentersAllotmentMaster m
                                        WHERE m.Course_ID = '{assignCentersInBulk.Course_ID}'
                                              AND m.Course_ID <> '{newGroupCourse_ID}'
                                              AND m.Center_ID = '{assignCentersInBulk.Center_ID}'
                                    ) s
                                        ON a.Student_ID = s.Entity_ID
                                           AND a.Course_ID = '{newGroupCourse_ID}'
                                LEFT JOIN dbo.ARGCentersAllotmentMaster m ON m.Course_ID='{newGroupCourse_ID}' AND m.Entity_ID=a.Student_ID 
								WHERE m.CenterAllowment_ID IS NULL";

            return new MSSQLFactory().ExecuteNonQuery(TSQL);
        }

        public bool CheckIfCentersAlreadyAlloted(PrintProgramme printProgramme, int Batch, Guid Course_ID)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            return new MSSQLFactory().GetObjectList<ARGCentersAllotmentMaster>($@"SELECT ISNULL(m.CenterAllowment_ID,'{Guid.Empty.ToString()}') FROM 
                                                            dbo.ARGPersonalInformation_{printProgramme.ToString()} P 
                                                            LEFT JOIN dbo.ARGCentersAllotmentMaster m ON m.Entity_ID = P.Student_ID  AND P.FormStatus=10 AND P.Batch = {Batch}
                                                            AND m.Course_ID='{Course_ID}'
                                                            WHERE m.CenterAllowment_ID IS NULL")?.All(x => x.CenterAllowment_ID != Guid.Empty) ?? false;
        }

        public List<ADMCourseMaster> GetCourseGroups(PrintProgramme printProgramme)
        {
            return new MSSQLFactory().GetObjectList<ADMCourseMaster>($@"SELECT Course_ID,GroupName FROM dbo.ADMCourseMaster 
                                                            WHERE GroupName IS NOT NULL AND PrintProgramme={(short)printProgramme}
                                                            ORDER BY GroupName");
        }
        #endregion

        #region Generate Center Notice
        public List<CenterNotice> GetCenterAndCourseInfo(RePrintCenterNotice printCenterNotice)
        {
            PrintProgramme printProgramme = GeneralFunctions.ProgrammeToPrintProgrammeMapping(printCenterNotice.CourseCategory);
            string AllColleges = printCenterNotice.College_ID == Guid.Empty ? "" : "ARGEntranceCentersMaster.College_ID=@College_ID AND";

            string SQL = $@"SELECT DISTINCT 
                        ARGEntranceCentersMaster.Center_ID,
                        ARGCentersAllotmentMaster.Course_ID,
                        Programme AS CourseCategory,
                        {printCenterNotice.ExaminationYear} ExamYear
                            FROM ARGCentersAllotmentMaster
                        JOIN ARGEntranceCentersMaster ON ARGEntranceCentersMaster.Center_ID=ARGCentersAllotmentMaster.Center_ID
                        JOIN ADMCollegeMaster ON ADMCollegeMaster.College_ID = ARGEntranceCentersMaster.College_ID
                        JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = ARGCentersAllotmentMaster.Course_ID
                        JOIN ARGPersonalInformation_{printProgramme.ToString()} p ON ARGCentersAllotmentMaster.Entity_ID=p.Student_ID
                        WHERE {AllColleges} PrintProgramme=@Programme AND ARGEntranceCentersMaster.Status=1 AND p.Batch={printCenterNotice.ExaminationYear}";

            SqlCommand sqlCommand = new SqlCommand(SQL);

            sqlCommand.Parameters.AddWithValue("@Programme", (short)printCenterNotice.CourseCategory);
            if (printCenterNotice.College_ID != Guid.Empty)
                sqlCommand.Parameters.AddWithValue("@College_Id", printCenterNotice.College_ID);
            return new MSSQLFactory().GetObjectList<CenterNotice>(sqlCommand);
        }

        public CenterNotice GetCenterNoticeDetail(CenterNotice _CenterNotice)
        {
            PrintProgramme printProgramme = GeneralFunctions.ProgrammeToPrintProgrammeMapping(_CenterNotice.CourseCategory);

            Tuple<string, List<Guid[]>> tuple = GetCourseGrouping(PrintProgramme.IH);

            string TSQL = string.Empty;

            string Course_IDs = $"{_CenterNotice.Course_ID}";
            if (tuple.Item1.ToLower().Contains(_CenterNotice.Course_ID.ToString().ToLower()))
            {
                Course_IDs = tuple.Item2.Find(x => (x[0] == _CenterNotice.Course_ID || x[1] == _CenterNotice.Course_ID))?.ToSingleString().Replace("|", "','");
            }

            string query = $@"SELECT 
                                MAX(ARGEntranceCentersMaster.College_ID) College_ID
                                                            ,MAX(ARGCentersAllotmentMaster.Course_ID) Course_ID
                                                            ,MAX(ARGCentersAllotmentMaster.Center_ID) Center_ID
                                                            ,MAX(CollegeFullName+', '+ADMCollegeMaster.Address) CollegeFullName
                                                            ,MAX(ADMCourseMaster.CourseFullName) CourseFullName
                                                            ,MAX(ADMCourseMaster.PrintProgramme) CourseCategory
                                                            ,MAX(CenterCode) CenterCode
                                                            ,MAX(YEAR(ARGCentersAllotmentMaster.EntranceDate)) ExamYear,
							                                (MIN(REPLACE(p.EntranceRollNo,'','')) +' - '+ MAX(REPLACE(p.EntranceRollNo,'',''))) RegularSeries
                                FROM ARGCentersAllotmentMaster
                                JOIN ARGEntranceCentersMaster ON ARGEntranceCentersMaster.Center_ID=ARGCentersAllotmentMaster.Center_ID
                                JOIN ADMCollegeMaster ON ADMCollegeMaster.College_ID = ARGEntranceCentersMaster.College_ID
                                JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = ARGCentersAllotmentMaster.Course_ID
                                JOIN ARGPersonalInformation_{printProgramme.ToString()} p ON ARGCentersAllotmentMaster.Entity_ID=p.Student_ID
                                WHERE p.EntranceRollNo IS NOT NULL AND p.FormStatus=10 
                                AND ARGCentersAllotmentMaster.Course_ID IN ('{Course_IDs}') 
                                AND ARGCentersAllotmentMaster.Center_ID=@Center_ID AND p.Batch=@Batch";

            SqlCommand sqlCommand = new SqlCommand(query);
            sqlCommand.Parameters.AddWithValue("@Center_ID", _CenterNotice.Center_ID);
            sqlCommand.Parameters.AddWithValue("@Batch", _CenterNotice.ExamYear);
            return new MSSQLFactory().GetObject<CenterNotice>(sqlCommand);
        }


        public int GetStudentsAssignedToCenter(CenterNotice _CenterNotice)
        {
            PrintProgramme printProgramme = GeneralFunctions.ProgrammeToPrintProgrammeMapping(_CenterNotice.CourseCategory);
            string SQL = $@"SELECT COUNT(ARGCentersAllotmentMaster.Center_ID) TotalStudentesAssigned FROM 
                                    ARGCentersAllotmentMaster JOIN ARGPersonalInformation_{printProgramme.ToString()} p ON Entity_ID=p.Student_ID
                                    WHERE ARGCentersAllotmentMaster.Course_ID='{_CenterNotice.Course_ID}' AND ARGCentersAllotmentMaster.Center_ID='{_CenterNotice.Center_ID}'
							 AND p.Batch={_CenterNotice.ExamYear} AND p.FormStatus=10";

            Tuple<string, List<Guid[]>> tuple = GetCourseGrouping(printProgramme);

            if (tuple.Item1.ToLower().Contains(_CenterNotice.Course_ID.ToString().ToLower()))
            {
                string Course_IDs = tuple.Item2.Find(x => (x[0] == _CenterNotice.Course_ID || x[1] == _CenterNotice.Course_ID))?.ToSingleString().Replace("|", "','");

                StringBuilder TSQL = new StringBuilder();
                TSQL.Append($@"SELECT T.[Total Students] FROM (");
                TSQL.Append($@"SELECT MAX(CollegeFullName) CollegeFullName,MAX(CenterCode) CenterCode,
                                        MAX(REPLACE(REPLACE(CourseFullName,'Integrated',''),'Honor''s','')) CourseFullName,
                                         COUNT(FinalTable.PNo) AS [Total Students]
                                        FROM (
                                        SELECT COUNT(A.Entity_ID) OVER(PARTITION BY A.Entity_ID ORDER BY cm.CourseFullName ASC) PNo,cm.CourseFullName,CollegeFullName,CenterCode,A.Center_ID
                                        FROM ARGCentersAllotmentMaster A
                                        JOIN ARGPersonalInformation_{printProgramme.ToString()} P ON P.Student_ID = A.Entity_ID
                                        JOIN ARGEntranceCentersMaster ON ARGEntranceCentersMaster.Center_ID = A.Center_ID
                                        JOIN ADMCollegeMaster ON ADMCollegeMaster.College_ID = ARGEntranceCentersMaster.College_ID
                                        JOIN ADMCourseMaster cm ON cm.Course_ID = A.Course_ID 
                                        WHERE 
                                        CM.Course_ID IN ('{Course_IDs}') AND P.FormStatus=10 AND P.Batch={_CenterNotice.ExamYear}
                                         AND A.Center_ID='{_CenterNotice.Center_ID}'
                                        ) FinalTable WHERE FinalTable.PNo=1
                                        GROUP BY FinalTable.PNo,FinalTable.Center_ID");


                TSQL.Append($@") T");
                SQL = TSQL.ToString();
            }


            return new MSSQLFactory().ExecuteScalar<int>(SQL);
        }


        #endregion

        #region GenerateExcel
        public Tuple<string, List<Guid[]>> GetCourseGrouping(PrintProgramme printProgramme)
        {
            List<ADMCourseMaster> CourseGroups = GetCourseGroups(printProgramme);
            List<Guid[]> CourseGrouping = new List<Guid[]>();

            StringBuilder NotInCourses = new StringBuilder();

            if (CourseGroups.IsNotNullOrEmpty())
            {
                foreach (var item in CourseGroups.GroupBy(x => x.GroupName.ToLower().Trim()))
                {
                    CourseGrouping.Add(item.Where(x => x.GroupName.ToLower().Trim() == item.Key.ToLower().Trim()).Select(x => x.Course_ID).ToArray());
                }
                foreach (var item in CourseGrouping)
                    NotInCourses.Append($"'{string.Join("','", item)}',");
                return Tuple.Create(NotInCourses.ToString().Substring(0, NotInCourses.ToString().Length - 1), CourseGrouping);
            }
            return Tuple.Create("", new List<Guid[]>());
        }

        public DataTable GetCenterWiseCourseCount(RePrintCenterNotice _PrintCenterNotice)
        {
            PrintProgramme printProgramme = GeneralFunctions.ProgrammeToPrintProgrammeMapping(_PrintCenterNotice.CourseCategory);
            if (printProgramme == PrintProgramme.PG || printProgramme == PrintProgramme.UG)
            {
                return new MSSQLFactory().GetDataTable($@"SELECT MAX(CollegeFullName) CollegeFullName,MAX(CenterCode) CenterCode,MAX(CourseFullName) CourseFullName,
                                                    COUNT(ARGCentersAllotmentMaster.Center_ID) [Total Students] FROM ARGCentersAllotmentMaster
                                                    JOIN ARGEntranceCentersMaster ON ARGEntranceCentersMaster.Center_ID=ARGCentersAllotmentMaster.Center_ID
                                                    JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = ARGCentersAllotmentMaster.Course_ID
                                                    JOIN ADMCollegeMaster ON ADMCollegeMaster.College_ID = ARGEntranceCentersMaster.College_ID
                                                    JOIN ARGPersonalInformation_{printProgramme.ToString()} p ON ARGCentersAllotmentMaster.Entity_ID=p.student_ID
                                                    WHERE PrintProgramme={(short)printProgramme} AND p.Batch={_PrintCenterNotice.ExaminationYear}
                                                    GROUP BY ARGCentersAllotmentMaster.Center_ID
                                                    ORDER BY CollegeFullName,CenterCode,CourseFullName,[Total Students]");
            }
            else
            {
                Tuple<string, List<Guid[]>> CourseGrouping = GetCourseGrouping(PrintProgramme.IH);
                StringBuilder TSQL = new StringBuilder();

                TSQL.Append($@"SELECT MAX(CollegeFullName) CollegeFullName,MAX(CenterCode) CenterCode,MAX(CourseFullName) CourseFullName,
                                             COUNT(A.Entity_ID) AS [Total Students] FROM ARGCentersAllotmentMaster A
                                            JOIN ARGPersonalInformation_{printProgramme.ToString()} P ON P.Student_ID = A.Entity_ID
                                            JOIN ADMCourseMaster CM ON CM.Course_ID = A.Course_ID
                                            JOIN ARGEntranceCentersMaster ON ARGEntranceCentersMaster.Center_ID = A.Center_ID
                                            JOIN ADMCollegeMaster ON ADMCollegeMaster.College_ID = ARGEntranceCentersMaster.College_ID
                                            WHERE 
                                            CM.Course_ID NOT IN({CourseGrouping.Item1.ToString()}) AND P.FormStatus=10 AND P.Batch={_PrintCenterNotice.ExaminationYear}
                                            GROUP BY A.Course_ID,A.Center_ID");

                foreach (var item in CourseGrouping.Item2)
                {
                    TSQL.Append(Environment.NewLine + "UNION" + Environment.NewLine);
                    TSQL.Append($@"SELECT MAX(CollegeFullName) CollegeFullName,MAX(CenterCode) CenterCode,
                                        MAX(REPLACE(REPLACE(CourseFullName,'Integrated',''),'Honor''s','')) CourseFullName,
                                         COUNT(FinalTable.PNo) AS [Total Students]
                                        FROM (
                                        SELECT COUNT(A.Entity_ID) OVER(PARTITION BY A.Entity_ID ORDER BY cm.CourseFullName ASC) PNo,cm.CourseFullName,CollegeFullName,CenterCode,A.Center_ID
                                        FROM ARGCentersAllotmentMaster A
                                        JOIN ARGPersonalInformation_{printProgramme.ToString()} P ON P.Student_ID = A.Entity_ID
                                        JOIN ARGEntranceCentersMaster ON ARGEntranceCentersMaster.Center_ID = A.Center_ID
                                        JOIN ADMCollegeMaster ON ADMCollegeMaster.College_ID = ARGEntranceCentersMaster.College_ID
                                        JOIN ADMCourseMaster cm ON cm.Course_ID = A.Course_ID 
                                        WHERE 
                                        CM.Course_ID IN ('{string.Join("','", item)}') AND P.FormStatus=10 AND P.Batch={_PrintCenterNotice.ExaminationYear}
                                        ) FinalTable WHERE FinalTable.PNo=1
                                        GROUP BY FinalTable.PNo,FinalTable.Center_ID");
                }
                return new MSSQLFactory().GetDataTable(TSQL.ToString());
            }
        }


        public DataTable GetAdmissionFormCount(RePrintCenterNotice printCenterNotice)
        {
            PrintProgramme printProgramme = GeneralFunctions.ProgrammeToPrintProgrammeMapping(printCenterNotice.CourseCategory);
            string totalApplicants = $@"{Environment.NewLine}UNION{Environment.NewLine} SELECT ('Total Applicants') AS CourseFullName, COUNT(P.Student_ID) AS [Total Forms] 
                            FROM ARGPersonalInformation_{printProgramme.ToString()}
                            P WHERE P.Batch = {printCenterNotice.ExaminationYear} AND P.FormStatus = 10  ORDER BY[Total Forms] ASC";

            if (printProgramme == PrintProgramme.PG || printProgramme == PrintProgramme.UG)
            {
                string CAT = $@"SELECT MAX(CourseFullName) CourseFullName , COUNT(a.Course_ID) [Total Forms] FROM ARGPersonalInformation_{printProgramme.ToString()} P
                                                    JOIN ARGCoursesApplied_{printProgramme.ToString()} A ON  A.Student_ID = P.Student_ID
                                                    JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = A.Course_ID
                                                    WHERE PrintProgramme = {(short)printProgramme} AND FormStatus = 10 AND Batch = {printCenterNotice.ExaminationYear}
                                                    GROUP BY A.Course_ID ";

                CAT += totalApplicants;

                return new MSSQLFactory().GetDataTable(CAT);
            }
            else
            {
                Tuple<string, List<Guid[]>> CourseGrouping = GetCourseGrouping(printProgramme);
                StringBuilder TSQL = new StringBuilder();

                string isLateralEntry = printCenterNotice.LateralEntryOnly ? $" AND P.IsLateralEntry=1 " : " AND P.IsLateralEntry=0";
                string courseLbl = printCenterNotice.LateralEntryOnly ? $"+' (Lateral Entry)'" : "";

                TSQL.Append($@"SELECT MAX(CM.CourseFullName){courseLbl} CourseFullName, COUNT(A.Course_ID) AS [Total Forms] FROM ARGCoursesApplied_{printProgramme.ToString()} A
                                        JOIN ARGPersonalInformation_{printProgramme.ToString()} P ON P.Student_ID = A.Student_ID
                                        JOIN ADMCourseMaster CM ON CM.Course_ID = A.Course_ID
                                        WHERE 
                                        CM.Course_ID NOT IN({CourseGrouping.Item1.ToString()}) AND P.FormStatus=10 AND P.Batch={printCenterNotice.ExaminationYear}
                                        {isLateralEntry}
                                        GROUP BY A.Course_ID");
                foreach (var item in CourseGrouping.Item2)
                {
                    TSQL.Append(Environment.NewLine + "UNION" + Environment.NewLine);
                    TSQL.Append($@"SELECT MAX(REPLACE(REPLACE(CourseFullName,'Integrated',''),'Honor''s','')){courseLbl} CourseFullName,COUNT(FinalTable.PNo) [Total Forms] FROM (
                                SELECT COUNT(A.Student_ID) OVER(PARTITION BY A.Student_ID ORDER BY cm.CourseFullName ASC) PNo,cm.CourseFullName FROM ARGCoursesApplied_{printProgramme.ToString()} A
                                JOIN ARGPersonalInformation_{printProgramme.ToString()} P ON P.Student_ID = A.Student_ID
                                JOIN ADMCourseMaster cm ON cm.Course_ID = A.Course_ID 
                                WHERE 
                                CM.Course_ID IN ('{string.Join("','", item)}') AND P.FormStatus=10 AND P.Batch={printCenterNotice.ExaminationYear} {isLateralEntry}
                                ) FinalTable WHERE FinalTable.PNo=1
                                GROUP BY FinalTable.PNo");
                }

                TSQL.Append(totalApplicants);

                return new MSSQLFactory().GetDataTable(TSQL.ToString());
            }

        }

        public DataTable GetOMRMasterFile(RePrintCenterNotice printCenterNotice)
        {
            PrintProgramme printProgramme = GeneralFunctions.ProgrammeToPrintProgrammeMapping(printCenterNotice.CourseCategory);
            Tuple<string, List<Guid[]>> tuple = GetCourseGrouping(printProgramme);

            string TSQL = string.Empty;

            Guid GeneralAppitudeCourse_ID =
                           Guid.Parse(Convert.ToString(ConfigurationManager.AppSettings["GeneralApitudeCourse_ID"]));

            if (GeneralAppitudeCourse_ID == printCenterNotice.College_ID)
            {
                TSQL = $@"SELECT DISTINCT P.EntranceRollNo,P.FullName FROM ARGPersonalInformation_{printProgramme.ToString()} P  
                        JOIN ARGCoursesApplied_{printProgramme.ToString()} A ON A.Student_ID = P.Student_ID
                        WHERE P.FormStatus={(short)FormStatus.FeePaid} AND P.Batch={printCenterNotice.ExaminationYear}
                        AND A.Course_ID<>'FC32E138-4EE2-4DA2-9453-5C8368180BC3' AND P.EntranceRollNo IS NOT NULL
                        ORDER BY P.EntranceRollNo ASC";
            }
            else
            {
                string Course_IDs = $"{printCenterNotice.College_ID}";
                if (tuple.Item1.ToLower().Contains(printCenterNotice.College_ID.ToString().ToLower()))
                {
                    Course_IDs = tuple.Item2.Find(x => (x[0] == printCenterNotice.College_ID || x[1] == printCenterNotice.College_ID))?.ToSingleString().Replace("|", "','");
                }

                TSQL = $@"SELECT DISTINCT P.EntranceRollNo,P.FullName FROM ARGPersonalInformation_{printProgramme.ToString()} P
                                                        JOIN ARGCoursesApplied_{printProgramme.ToString()} A ON A.Student_ID = P.Student_ID
                                                        WHERE P.FormStatus={(short)FormStatus.FeePaid} AND P.Batch={printCenterNotice.ExaminationYear}
                                                        AND A.Course_ID IN ('{Course_IDs}') AND P.EntranceRollNo IS NOT NULL
                                                        ORDER BY P.EntranceRollNo ASC";
            }
            return new MSSQLFactory().GetDataTable(TSQL);

        }

        public DataTable GetAttendanceSheetFile(RePrintCenterNotice printCenterNotice)
        {
            PrintProgramme printProgramme = GeneralFunctions.ProgrammeToPrintProgrammeMapping(printCenterNotice.CourseCategory);
            Tuple<string, List<Guid[]>> tuple = GetCourseGrouping(printProgramme);

            string TSQL = string.Empty;

            Guid GeneralAppitudeCourse_ID =
                           Guid.Parse(Convert.ToString(ConfigurationManager.AppSettings["GeneralApitudeCourse_ID"]));

            if (GeneralAppitudeCourse_ID == printCenterNotice.College_ID)
            {
                TSQL = $@"SELECT DISTINCT ROW_NUMBER() OVER(PARTITION BY EC.CenterCode ORDER BY EC.CenterCode,P.EntranceRollNo ASC) [SNo], P.EntranceRollNo,P.FullName,P.FathersName,EC.CenterCode,'' AS [OMR Sheet No.],'' AS [Question Booklet Series],'' AS [Applicant Signature] FROM ARGPersonalInformation_{printProgramme.ToString()} P
                            JOIN ARGCentersAllotmentMaster CA ON CA.Entity_ID=P.Student_ID 
                            JOIN ARGEntranceCentersMaster EC ON EC.Center_ID=CA.Center_ID
                            WHERE P.FormStatus={(short)FormStatus.FeePaid} AND P.Batch={printCenterNotice.ExaminationYear} AND P.EntranceRollNo IS NOT NULL
                            AND CA.Course_ID='{GeneralAppitudeCourse_ID}'
                            ORDER BY EC.CenterCode,P.EntranceRollNo ASC";
            }
            else if (new CourseDB().GetItem(printCenterNotice.College_ID).Programme == Programme.Engineering)
            {
                TSQL = $@"SELECT ROW_NUMBER() OVER(PARTITION BY TEMP.CenterCode ORDER BY TEMP.CenterCode,TEMP.EntranceRollNo ASC) [SNo],* FROM (SELECT DISTINCT P.EntranceRollNo,P.FullName,P.FathersName,EC.CenterCode,'' AS [OMR Sheet No.],'' AS [Question Booklet Series],'' AS [Applicant Signature] FROM ARGPersonalInformation_{printProgramme.ToString()} P
                            JOIN ARGCentersAllotmentMaster CA ON CA.Entity_ID=P.Student_ID 
                            JOIN ARGEntranceCentersMaster EC ON EC.Center_ID=CA.Center_ID
                            WHERE P.FormStatus={(short)FormStatus.FeePaid} AND P.Batch={printCenterNotice.ExaminationYear} AND P.EntranceRollNo IS NOT NULL AND IsLateralEntry=1)
                            TEMP ORDER BY TEMP.CenterCode,TEMP.EntranceRollNo ASC";
            }
            else
            {
                string Course_IDs = $"{printCenterNotice.College_ID}";
                if (tuple.Item1.ToLower().Contains(printCenterNotice.College_ID.ToString().ToLower()))
                {
                    Course_IDs = tuple.Item2.Find(x => (x[0] == printCenterNotice.College_ID || x[1] == printCenterNotice.College_ID))?.ToSingleString().Replace("|", "','");
                }
                TSQL = $@"SELECT ROW_NUMBER() OVER (PARTITION BY s.CenterCode
                                                  ORDER BY s.CenterCode,
                                                           s.EntranceRollNo ASC
                                                 ) [SNo],
                               s.EntranceRollNo,
                               s.FullName,
                               s.FathersName,
                               s.CenterCode,
                               '' AS [OMR Sheet No.],
                               '' AS [Question Booklet Series],
                               '' AS [Applicant Signature]
                        FROM
                        (
                            SELECT DISTINCT
                                   P.EntranceRollNo,
                                   P.FullName,
                                   P.FathersName,
                                   EC.CenterCode
                            FROM ARGPersonalInformation_{printProgramme.ToString()} P
                                JOIN ARGCoursesApplied_{printProgramme.ToString()} A
                                    ON A.Student_ID = P.Student_ID
                                JOIN ARGCentersAllotmentMaster CA
                                    ON CA.Entity_ID = P.Student_ID
                                       AND CA.Course_ID = A.Course_ID
                                JOIN ARGEntranceCentersMaster EC
                                    ON EC.Center_ID = CA.Center_ID
                            WHERE P.FormStatus = {(short)FormStatus.FeePaid}
                                  AND P.Batch = {printCenterNotice.ExaminationYear}
                                  AND A.Course_ID IN ( '{Course_IDs}' )
                                  AND P.EntranceRollNo IS NOT NULL
                        ) s
                        ORDER BY s.CenterCode,
                                 s.EntranceRollNo ASC;";

            }

            return new MSSQLFactory().GetDataTable(TSQL);
        }


        public DataTable GetGeneralMeritList(RePrintCenterNotice printCenterNotice)
        {
            Guid GeneralAppitudeCourse_ID =
                           Guid.Parse(Convert.ToString(ConfigurationManager.AppSettings["GeneralApitudeCourse_ID"]));

            PrintProgramme printProgramme = GeneralFunctions.ProgrammeToPrintProgrammeMapping(printCenterNotice.CourseCategory);
            string TSQL = "";
            if (printProgramme == PrintProgramme.UG)
            {
                if (printCenterNotice.College_ID != GeneralAppitudeCourse_ID)
                {
                    TSQL = $@"SELECT DISTINCT
                            P.StudentFormNo,
                            P.EntranceRollNo,
                            P.FullName,P.FathersName,P.Gender,ad.Mobile,
                            P.Category AS StudentCategory,
                            q.Stream AS [12th Stream],
                            P.CATEntrancePoints,
                            P.CUETApplicationNo,
                            P.CUETEntranceRollNo,
                            CASE
                                WHEN P.IsProvisional = 1 THEN
                                    'Provisional Adm'
                                ELSE
                            (CAST(q.MarksObt AS VARCHAR(10)) + '/' + CAST(q.MaxMarks AS VARCHAR(10)))
                            END AS [MarksObtained in 12th],
                            CASE
                                WHEN P.IsProvisional = 1 THEN
                                    'Provisional Adm'
                                ELSE
                                    CAST(CAST(((q.MarksObt / q.MaxMarks) * 40) AS DECIMAL(5, 2)) AS VARCHAR(10))
                            END AS [40% of 12th Percentage]
                            ,CASE
                                    WHEN P.IsProvisional = 1 THEN
                                        P.CATEntrancePoints
                                    ELSE
                                        CAST(CAST(((q.MarksObt / q.MaxMarks) * 40) AS DECIMAL(5, 2)) + ISNULL(P.CATEntrancePoints,0) AS VARCHAR(6))
                                END AS TotalPoints
                    FROM dbo.ARGPersonalInformation_UG P
                        JOIN dbo.ARGStudentAddress_UG ad ON ad.Student_ID=P.Student_ID
                        JOIN dbo.ARGStudentPreviousQualifications_UG q
                            ON q.Student_ID = p.Student_ID AND q.ExamName = '12th'
                       JOIN dbo.ARGCoursesApplied_UG ca 
                            ON ca.Student_ID = ad.Student_ID AND ca.Course_ID='{printCenterNotice.College_ID}'
                    WHERE  P.Batch = {printCenterNotice.ExaminationYear}
                            AND P.FormStatus=10 AND P.EntranceRollNo IS NOT NULL AND P.CATEntrancePoints IS NOT NULL
                    ORDER BY TotalPoints DESC,
                                    P.CATEntrancePoints DESC;";
                }
                else
                {
                    TSQL = $@"SELECT DISTINCT
                            P.StudentFormNo,
                            P.EntranceRollNo,
                            P.FullName,P.FathersName,P.Gender,ad.Mobile,
                            P.Category AS StudentCategory,
                            q.Stream AS [12th Stream],
                            P.CATEntrancePoints,
                            P.CUETApplicationNo,
                            P.CUETEntranceRollNo,
                            CASE
                                WHEN P.IsProvisional = 1 THEN
                                    'Provisional Adm'
                                ELSE
                            (CAST(q.MarksObt AS VARCHAR(10)) + '/' + CAST(q.MaxMarks AS VARCHAR(10)))
                            END AS [MarksObtained in 12th],
                            CASE
                                WHEN P.IsProvisional = 1 THEN
                                    'Provisional Adm'
                                ELSE
                                    CAST(CAST(((q.MarksObt / q.MaxMarks) * 40) AS DECIMAL(5, 2)) AS VARCHAR(10))
                            END AS [40% of 12th Percentage]
                            ,CASE
                                    WHEN P.IsProvisional = 1 THEN
                                        P.CATEntrancePoints
                                    ELSE
                                        CAST(CAST(((q.MarksObt / q.MaxMarks) * 40) AS DECIMAL(5, 2)) + ISNULL(P.CATEntrancePoints,0) AS VARCHAR(6))
                                END AS TotalPoints
                    FROM dbo.ARGPersonalInformation_UG P
                        JOIN dbo.ARGStudentAddress_UG ad ON ad.Student_ID=P.Student_ID
                        JOIN dbo.ARGStudentPreviousQualifications_UG q
                            ON q.Student_ID = p.Student_ID AND q.ExamName = '12th'
                    WHERE  P.Batch = {printCenterNotice.ExaminationYear}
                            AND P.FormStatus=10 AND P.EntranceRollNo IS NOT NULL AND P.CATEntrancePoints IS NOT NULL
                    ORDER BY TotalPoints DESC,
                                    P.CATEntrancePoints DESC;";
                }
            }
            else if (printCenterNotice.EntranceExcelListType == EntranceExcelListType.MeritListAcademic)
            {
                TSQL = $@"SELECT DISTINCT
                               P.StudentFormNo,
                               P.EntranceRollNo,
                               P.FullName,P.FathersName,ad.Mobile,
                               P.Category AS StudentCategory,
                               a.Preference,
                               a.SubjectEntrancePoints,
                                P.CUETApplicationNo,
                                P.CUETEntranceRollNo,
                               CASE
                                   WHEN P.IsProvisional = 1 THEN
                                       'Provisional Adm'
                                   ELSE
                               (CAST(q.MarksObt AS VARCHAR(10)) + '/' + CAST(q.MaxMarks AS VARCHAR(10)))
                               END AS [MarksObtained in 12th],
                               CASE
                                   WHEN P.IsProvisional = 1 THEN
                                       'Provisional Adm'
                                   ELSE
                                       CAST(CAST(((CAST(((q.MarksObt / q.MaxMarks) * 100) AS DECIMAL(5, 2)) * 40) / 100) AS DECIMAL(5, 2)) AS VARCHAR(10))
                               END AS [40% of 12th Percentage]
                               ,CASE
                                   WHEN P.IsProvisional = 1 THEN
                                       a.SubjectEntrancePoints
                                   ELSE
                                      CAST(((CAST(((q.MarksObt / q.MaxMarks) * 100) AS DECIMAL(5, 2)) * 40)/100) AS DECIMAL(5,2))+ a.SubjectEntrancePoints
                               END AS TotalPoints
                        FROM dbo.ARGPersonalInformation_{printProgramme.ToString()} P
                            JOIN dbo.ARGCoursesApplied_{printProgramme.ToString()} a
                                ON a.Student_ID = P.Student_ID AND a.Course_ID ='{printCenterNotice.College_ID}'
                            JOIN dbo.ARGStudentAddress_{printProgramme.ToString()} ad ON ad.Student_ID=P.Student_ID
                            LEFT JOIN dbo.ARGStudentPreviousQualifications_{printProgramme.ToString()} q
                                ON q.Student_ID = p.Student_ID 
                        WHERE  P.Batch = {printCenterNotice.ExaminationYear}
                              AND a.AppearedInEntrance = 1 AND P.EntranceRollNo IS NOT NULL
                        ORDER BY TotalPoints DESC,
                                 a.SubjectEntrancePoints DESC;";
            }
            else
            {
                Dictionary<PrintProgramme, bool> IncludeGeneralAppitude =
                       JsonConvert.DeserializeObject<Dictionary<PrintProgramme, bool>>(
                       Convert.ToString(ConfigurationManager.AppSettings["IncludeGeneralApitude"]));
                IncludeGeneralAppitude.TryGetValue(printProgramme, out bool HasCAT);

                if (HasCAT)
                {
                    TSQL = $@"SELECT DISTINCT StudentFormNo, EntranceRollNo AS [Entrance RollNo],FullName,P.PreviousUniversityRegnNo,FathersName,Ad.Mobile,Category,SubjectEntrancePoints,P.CATEntrancePoints, (SubjectEntrancePoints+ ISNULL(P.CATEntrancePoints,0)) AS TotalPoints
                         , P.CUETApplicationNo,
                           P.CUETEntranceRollNo 
                        FROM ARGPersonalInformation_{printProgramme.ToString()} P
                        JOIN ARGCoursesApplied_{printProgramme.ToString()} A ON A.Student_ID = P.Student_ID
                        JOIN ARGStudentAddress_{printProgramme.ToString()} Ad ON Ad.Student_ID = P.Student_ID
                        WHERE AppearedInEntrance=1 AND EntranceRollNo IS NOT NULL AND Course_ID='{printCenterNotice.College_ID}' AND Batch={printCenterNotice.ExaminationYear}
                        ORDER BY TotalPoints DESC,SubjectEntrancePoints DESC,P.CATEntrancePoints DESC,Category ASC";

                    if (GeneralAppitudeCourse_ID == printCenterNotice.College_ID)
                    {
                        TSQL = $@"SELECT DISTINCT StudentFormNo, EntranceRollNo AS [Entrance RollNo],FullName,FathersName,Ad.Mobile,Category,CATEntrancePoints,
                            P.CUETApplicationNo,
                            P.CUETEntranceRollNo 
                            FROM ARGPersonalInformation_{printProgramme.ToString()} P
                            JOIN ARGCoursesApplied_{printProgramme.ToString()} A ON A.Student_ID = P.Student_ID
                            JOIN ARGStudentAddress_{printProgramme.ToString()} Ad ON Ad.Student_ID = P.Student_ID
                            WHERE P.CATEntrancePoints IS NOT NULL AND EntranceRollNo IS NOT NULL AND Batch={printCenterNotice.ExaminationYear}
                            AND A.Course_ID<>'FC32E138-4EE2-4DA2-9453-5C8368180BC3'
                            ORDER BY P.CATEntrancePoints DESC,Category ASC";
                    }
                }
                else
                {
                    TSQL = $@"SELECT DISTINCT StudentFormNo, EntranceRollNo AS [Entrance RollNo],FullName,FathersName,Ad.Mobile,Category,SubjectEntrancePoints,
                            P.CUETApplicationNo,
                            P.CUETEntranceRollNo 
                        FROM ARGPersonalInformation_{printProgramme.ToString()} P
                        JOIN ARGCoursesApplied_{printProgramme.ToString()} A ON A.Student_ID = P.Student_ID
                        JOIN ARGStudentAddress_{printProgramme.ToString()} Ad ON Ad.Student_ID = P.Student_ID
                        WHERE AppearedInEntrance=1 AND EntranceRollNo IS NOT NULL AND Course_ID='{printCenterNotice.College_ID}' AND Batch={printCenterNotice.ExaminationYear}
                        ORDER BY SubjectEntrancePoints DESC,Category ASC";
                }
                if (printCenterNotice.College_ID == Guid.Parse("A3EE7F98-7B82-4D95-A2C0-FABA7A18240E"))
                {
                    TSQL = $@"SELECT DISTINCT
                           StudentFormNo,EntranceRollNo AS [Entrance RollNo],
                           FullName,
                           P.Gender,
                           Ad.Mobile,
                            P.CUETApplicationNo,
                            P.CUETEntranceRollNo,
                           CASE
                               WHEN P.IsProvisional = 1 THEN
                                   'Provisional Adm'
                               ELSE
                           (CAST(q.MarksObt AS VARCHAR(20)) + '/' + CAST(q.MaxMarks AS VARCHAR(20)))
                           END AS [12th MarksObt],
                           CASE
                               WHEN P.IsProvisional = 1 THEN
                                   'Provisional Adm'
                               ELSE
                                   CAST(CAST(((q.MarksObt / q.MaxMarks) * 100) AS DECIMAL(5, 2)) AS VARCHAR(7))
                           END AS [12th Percentage],
	                        CASE
                               WHEN P.IsProvisional = 1 THEN
                                   'Provisional Adm'
                               ELSE
                                   CAST(CAST(((CAST(((q.MarksObt / q.MaxMarks) * 100) AS DECIMAL(5, 2)) * 60)/100) AS DECIMAL(5,2)) AS VARCHAR(7))
                           END AS [Academic Merit 60% of 12th],
                           P.CATEntrancePoints,
	                       CASE
                               WHEN P.IsProvisional = 1 THEN
                                   p.CATEntrancePoints
                               ELSE
                                   CAST(CAST(((CAST(((q.MarksObt / q.MaxMarks) * 100) AS DECIMAL(5, 2)) * 60)/100) AS DECIMAL(5,2))+p.CATEntrancePoints AS VARCHAR(7))
                           END AS [Total Points]
                        FROM ARGPersonalInformation_{printProgramme.ToString()}  P
                            JOIN ARGCoursesApplied_{printProgramme.ToString()}  A
                                ON A.Student_ID = P.Student_ID
                            JOIN ARGStudentAddress_{printProgramme.ToString()}  Ad
                                ON Ad.Student_ID = P.Student_ID
		                        LEFT JOIN dbo.ARGStudentPreviousQualifications_{printProgramme.ToString()} q ON q.Student_ID=p.Student_ID
                        WHERE EntranceRollNo IS NOT NULL AND P.CATEntrancePoints IS NOT NULL
                                AND Course_ID = '{printCenterNotice.College_ID}'
                                AND Batch = {printCenterNotice.ExaminationYear}
                        ORDER BY [Total Points] DESC;";
                }
            }

            return new MSSQLFactory().GetDataTable(TSQL);
        }

        public DataTable GetCategoryMeritList(RePrintCenterNotice printCenterNotice)
        {
            Guid GeneralAppitudeCourse_ID =
                            Guid.Parse(Convert.ToString(ConfigurationManager.AppSettings["GeneralApitudeCourse_ID"]));

            PrintProgramme printProgramme = GeneralFunctions.ProgrammeToPrintProgrammeMapping(printCenterNotice.CourseCategory);
            string TSQL = "";

            if (printProgramme == PrintProgramme.UG)
            {
                if (printCenterNotice.College_ID != GeneralAppitudeCourse_ID)
                {
                    TSQL = $@"SELECT DISTINCT
                            P.StudentFormNo,
                            P.EntranceRollNo,
                            P.FullName,P.FathersName,P.Gender,ad.Mobile,
                            P.Category AS StudentCategory,
                            q.Stream AS [12th Stream],
                            P.CATEntrancePoints,
                            P.CUETApplicationNo,
                            P.CUETEntranceRollNo,
                            CASE
                                WHEN P.IsProvisional = 1 THEN
                                    'Provisional Adm'
                                ELSE
                            (CAST(q.MarksObt AS VARCHAR(10)) + '/' + CAST(q.MaxMarks AS VARCHAR(10)))
                            END AS [MarksObtained in 12th],
                            CASE
                                WHEN P.IsProvisional = 1 THEN
                                    'Provisional Adm'
                                ELSE
                                    CAST(CAST(((q.MarksObt / q.MaxMarks) * 40) AS DECIMAL(5, 2)) AS VARCHAR(10))
                            END AS [40% of 12th Percentage]
                            ,CASE
                                   WHEN P.IsProvisional = 1 THEN
                                       P.CATEntrancePoints
                                   ELSE
                                      CAST(CAST(((q.MarksObt / q.MaxMarks) * 40) AS DECIMAL(5, 2)) + ISNULL(P.CATEntrancePoints,0) AS VARCHAR(6))
                               END AS TotalPoints
                    FROM dbo.ARGPersonalInformation_UG P
                        JOIN dbo.ARGStudentAddress_UG ad ON ad.Student_ID=P.Student_ID
                        JOIN dbo.ARGStudentPreviousQualifications_UG q
                            ON q.Student_ID = p.Student_ID AND q.ExamName = '12th'
                       JOIN dbo.ARGCoursesApplied_UG ca 
                            ON ca.Student_ID = ad.Student_ID AND ca.Course_ID='{printCenterNotice.College_ID}'
                    WHERE  P.Batch = {printCenterNotice.ExaminationYear}
                            AND P.FormStatus=10 AND P.Category<>'OM' AND P.EntranceRollNo IS NOT NULL AND P.CATEntrancePoints IS NOT NULL
                    ORDER BY P.Category ASC,TotalPoints DESC,
                                 P.CATEntrancePoints DESC;";
                }
                else
                {
                    TSQL = $@"SELECT DISTINCT
                            P.StudentFormNo,
                            P.EntranceRollNo,
                            P.FullName,P.FathersName,P.Gender,ad.Mobile,
                            P.Category AS StudentCategory,
                            q.Stream AS [12th Stream],
                            P.CATEntrancePoints,
                            P.CUETApplicationNo,
                            P.CUETEntranceRollNo,
                            CASE
                                WHEN P.IsProvisional = 1 THEN
                                    'Provisional Adm'
                                ELSE
                            (CAST(q.MarksObt AS VARCHAR(10)) + '/' + CAST(q.MaxMarks AS VARCHAR(10)))
                            END AS [MarksObtained in 12th],
                            CASE
                                WHEN P.IsProvisional = 1 THEN
                                    'Provisional Adm'
                                ELSE
                                    CAST(CAST(((q.MarksObt / q.MaxMarks) * 40) AS DECIMAL(5, 2)) AS VARCHAR(10))
                            END AS [40% of 12th Percentage]
                            ,CASE
                                   WHEN P.IsProvisional = 1 THEN
                                       P.CATEntrancePoints
                                   ELSE
                                      CAST(CAST(((q.MarksObt / q.MaxMarks) * 40) AS DECIMAL(5, 2)) + ISNULL(P.CATEntrancePoints,0) AS VARCHAR(6))
                               END AS TotalPoints
                    FROM dbo.ARGPersonalInformation_UG P
                        JOIN dbo.ARGStudentAddress_UG ad ON ad.Student_ID=P.Student_ID
                        JOIN dbo.ARGStudentPreviousQualifications_UG q
                            ON q.Student_ID = p.Student_ID AND q.ExamName = '12th'
                    WHERE  P.Batch = {printCenterNotice.ExaminationYear}
                            AND P.FormStatus=10 AND P.Category<>'OM' AND P.EntranceRollNo IS NOT NULL AND P.CATEntrancePoints IS NOT NULL
                    ORDER BY P.Category ASC,TotalPoints DESC,
                                 P.CATEntrancePoints DESC;";
                }
            }
            else if (printCenterNotice.EntranceExcelListType == EntranceExcelListType.MeritListAcademic)
            {
                TSQL = $@"SELECT DISTINCT
                               P.StudentFormNo,
                               P.EntranceRollNo,
                               P.FullName,P.FathersName,ad.Mobile,
                               P.Category AS StudentCategory,
                               a.Preference,
                               a.SubjectEntrancePoints,
                                P.CUETApplicationNo,
                                P.CUETEntranceRollNo,
                               CASE
                                   WHEN P.IsProvisional = 1 THEN
                                       'Provisional Adm'
                                   ELSE
                               (CAST(q.MarksObt AS VARCHAR(10)) + '/' + CAST(q.MaxMarks AS VARCHAR(10)))
                               END AS [MarksObtained in 12th],
                               CASE
                                   WHEN P.IsProvisional = 1 THEN
                                       'Provisional Adm'
                                   ELSE
                                       CAST(CAST(((CAST(((q.MarksObt / q.MaxMarks) * 100) AS DECIMAL(5, 2)) * 40) / 100) AS DECIMAL(5, 2)) AS VARCHAR(10))
                               END AS [40% of 12th Percentage]
                               ,CASE
                                   WHEN P.IsProvisional = 1 THEN
                                       a.SubjectEntrancePoints
                                   ELSE
                                      CAST(((CAST(((q.MarksObt / q.MaxMarks) * 100) AS DECIMAL(5, 2)) * 40)/100) AS DECIMAL(5,2))+ a.SubjectEntrancePoints
                               END AS TotalPoints
                        FROM dbo.ARGPersonalInformation_{printProgramme.ToString()} P
                            JOIN dbo.ARGCoursesApplied_{printProgramme.ToString()} a
                                ON a.Student_ID = P.Student_ID AND a.Course_ID ='{printCenterNotice.College_ID}'
                            JOIN dbo.ARGStudentAddress_{printProgramme.ToString()} ad ON ad.Student_ID=P.Student_ID
                            LEFT JOIN dbo.ARGStudentPreviousQualifications_{printProgramme.ToString()} q
                                ON q.Student_ID = p.Student_ID 
                        WHERE  P.Batch = {printCenterNotice.ExaminationYear}
                              AND a.AppearedInEntrance = 1 AND P.EntranceRollNo IS NOT NULL  AND P.Category<>'OM'
                        ORDER BY  P.Category ASC,TotalPoints DESC,
                                 a.SubjectEntrancePoints DESC";
            }
            else
            {
                Dictionary<PrintProgramme, bool> IncludeGeneralAppitude =
                      JsonConvert.DeserializeObject<Dictionary<PrintProgramme, bool>>(
                      Convert.ToString(ConfigurationManager.AppSettings["IncludeGeneralApitude"]));
                IncludeGeneralAppitude.TryGetValue(printProgramme, out bool HasCAT);

                if (HasCAT)
                {
                    TSQL = $@"SELECT DISTINCT StudentFormNo, EntranceRollNo AS [Entrance RollNo],FullName,FathersName,P.PreviousUniversityRegnNo,Ad.Mobile,Category,SubjectEntrancePoints,P.CATEntrancePoints, (SubjectEntrancePoints+ISNULL(P.CATEntrancePoints,0)) AS TotalPoints,
                            P.CUETApplicationNo,
                            P.CUETEntranceRollNo
                         FROM ARGPersonalInformation_{printProgramme.ToString()} P
                        JOIN ARGCoursesApplied_{printProgramme.ToString()} A ON A.Student_ID = P.Student_ID
                        JOIN ARGStudentAddress_{printProgramme.ToString()} Ad ON Ad.Student_ID = P.Student_ID
                        WHERE AppearedInEntrance=1 AND EntranceRollNo IS NOT NULL AND Course_ID='{printCenterNotice.College_ID}' AND Batch={printCenterNotice.ExaminationYear} 
                        AND Category<>'OM'
                        ORDER BY Category ASC, TotalPoints DESC,SubjectEntrancePoints DESC,P.CATEntrancePoints DESC";

                    if (GeneralAppitudeCourse_ID == printCenterNotice.College_ID)
                    {
                        TSQL = $@"SELECT DISTINCT StudentFormNo, EntranceRollNo AS [Entrance RollNo],FullName,FathersName,Ad.Mobile,Category,CATEntrancePoints,
                            P.CUETApplicationNo,
                            P.CUETEntranceRollNo
                            FROM ARGPersonalInformation_{printProgramme.ToString()} P
                            JOIN ARGCoursesApplied_{printProgramme.ToString()} A ON A.Student_ID = P.Student_ID
                            JOIN ARGStudentAddress_{printProgramme.ToString()} Ad ON Ad.Student_ID = P.Student_ID
                            WHERE P.CATEntrancePoints IS NOT NULL AND EntranceRollNo IS NOT NULL AND Batch={printCenterNotice.ExaminationYear} AND Category<>'OM'
                            AND A.Course_ID<>'FC32E138-4EE2-4DA2-9453-5C8368180BC3'
                            ORDER BY Category ASC, P.CATEntrancePoints DESC";
                    }
                }
                else
                {
                    TSQL = $@"SELECT DISTINCT StudentFormNo, EntranceRollNo AS [Entrance RollNo],FullName,FathersName,Ad.Mobile,Category,SubjectEntrancePoints,
                            P.CUETApplicationNo,
                            P.CUETEntranceRollNo
                            FROM ARGPersonalInformation_{printProgramme.ToString()} P
                        JOIN ARGCoursesApplied_{printProgramme.ToString()} A ON A.Student_ID = P.Student_ID
                        JOIN ARGStudentAddress_{printProgramme.ToString()} Ad ON Ad.Student_ID = P.Student_ID
                        WHERE AppearedInEntrance=1 AND EntranceRollNo IS NOT NULL AND Course_ID='{printCenterNotice.College_ID}' AND Batch={printCenterNotice.ExaminationYear} 
                        AND Category<>'OM'
                        ORDER BY Category ASC,SubjectEntrancePoints DESC";
                }

                if (printCenterNotice.College_ID == Guid.Parse("A3EE7F98-7B82-4D95-A2C0-FABA7A18240E"))
                {
                    TSQL = $@"SELECT DISTINCT
                           StudentFormNo, EntranceRollNo AS [Entrance RollNo],
                           FullName,
                           P.Gender,
                           Ad.Mobile,
                            P.CUETApplicationNo,
                            P.CUETEntranceRollNo,
                           CASE
                               WHEN P.IsProvisional = 1 THEN
                                   'Provisional Adm'
                               ELSE
                           (CAST(q.MarksObt AS VARCHAR(20)) + '/' + CAST(q.MaxMarks AS VARCHAR(20)))
                           END AS [12th MarksObt],
                           CASE
                               WHEN P.IsProvisional = 1 THEN
                                   'Provisional Adm'
                               ELSE
                                   CAST(CAST(((q.MarksObt / q.MaxMarks) * 100) AS DECIMAL(5, 2)) AS VARCHAR(7))
                           END AS [12th Percentage],
	                        CASE
                               WHEN P.IsProvisional = 1 THEN
                                   'Provisional Adm'
                               ELSE
                                   CAST(CAST(((CAST(((q.MarksObt / q.MaxMarks) * 100) AS DECIMAL(5, 2)) * 60)/100) AS DECIMAL(5,2)) AS VARCHAR(7))
                           END AS [Academic Merit 60% of 12th],
                           P.CATEntrancePoints,
	                       CASE
                               WHEN P.IsProvisional = 1 THEN
                                   p.CATEntrancePoints
                               ELSE
                                   CAST(CAST(((CAST(((q.MarksObt / q.MaxMarks) * 100) AS DECIMAL(5, 2)) * 60)/100) AS DECIMAL(5,2))+p.CATEntrancePoints AS VARCHAR(7))
                           END AS [Total Points]
                        FROM ARGPersonalInformation_{printProgramme.ToString()}  P
                            JOIN ARGCoursesApplied_{printProgramme.ToString()}  A
                                ON A.Student_ID = P.Student_ID
                            JOIN ARGStudentAddress_{printProgramme.ToString()}  Ad
                                ON Ad.Student_ID = P.Student_ID
		                        LEFT JOIN dbo.ARGStudentPreviousQualifications_{printProgramme.ToString()} q ON q.Student_ID=p.Student_ID
                        WHERE EntranceRollNo IS NOT NULL AND P.CATEntrancePoints IS NOT NULL AND Category<>'OM'
                                AND Course_ID = '{printCenterNotice.College_ID}'
                                AND Batch = {printCenterNotice.ExaminationYear}
                        ORDER BY [Total Points] DESC;";
                }
            }
            return new MSSQLFactory().GetDataTable(TSQL);
        }


        public DataTable GetDateSheet(RePrintCenterNotice printCenterNotice)
        {
            PrintProgramme printProgramme = GeneralFunctions.ProgrammeToPrintProgrammeMapping(printCenterNotice.CourseCategory);
            return new MSSQLFactory().GetDataTable($@"SELECT DISTINCT CA.EntranceDate,CA.Time,CM.CourseFullName FROM ARGCentersAllotmentMaster CA
                                                        JOIN ARGPersonalInformation_{printProgramme.ToString()} P ON CA.Entity_ID=P.Student_ID
                                                        JOIN ADMCourseMaster CM ON CM.Course_ID = CA.Course_ID
                                                        WHERE P.Batch={printCenterNotice.ExaminationYear} ORDER BY
                                                        CM.CourseFullName, CA.EntranceDate ASC");
        }

        public DataTable GetDistrictWiseCount(RePrintCenterNotice printCenterNotice)
        {
            PrintProgramme printProgramme = GeneralFunctions.ProgrammeToPrintProgrammeMapping(printCenterNotice.CourseCategory);
            return new MSSQLFactory().GetDataTable($@"SELECT COUNT(p.Student_ID) TotalForms,District FROM dbo.ARGPersonalInformation_{printProgramme.ToString()} p
                                                    JOIN dbo.ARGStudentAddress_{printProgramme.ToString()} a ON a.Student_ID = p.Student_ID
                                                    WHERE Batch={printCenterNotice.ExaminationYear} AND FormStatus={(short)FormStatus.FeePaid} AND P.Student_ID NOT IN(
                                                    SELECT ca.Student_ID FROM dbo.ARGPersonalInformation_{printProgramme.ToString()} pp
                                                    JOIN dbo.ARGCoursesApplied_{printProgramme.ToString()} ca ON ca.Student_ID = pp.Student_ID
                                                    WHERE pp.Batch={printCenterNotice.ExaminationYear} AND ca.Course_ID='FC32E138-4EE2-4DA2-9453-5C8368180BC3'
                                                    )
                                                    GROUP BY District ORDER BY TotalForms DESC");
        }

        public DataTable GetDistrictWiseCenterAllotmentCount(RePrintCenterNotice printCenterNotice)
        {
            PrintProgramme printProgramme = GeneralFunctions.ProgrammeToPrintProgrammeMapping(printCenterNotice.CourseCategory);
            Tuple<string, List<Guid[]>> tuple = GetCourseGrouping(printProgramme);

            string Course_IDs = $"{printCenterNotice.College_ID}";
            if (tuple.Item1.ToLower().Contains(printCenterNotice.College_ID.ToString().ToLower()))
            {
                Course_IDs = tuple.Item2.Find(x => (x[0] == printCenterNotice.College_ID || x[1] == printCenterNotice.College_ID))?.ToSingleString().Replace("|", "','");
            }

            string TSQL = $@"SELECT COUNT(a.Student_ID) TotalStudentsAlloted,
                                       a.District,
                                       a.CenterCode,
                                       MAX(Address) [Address]
                                FROM
                                (
                                    SELECT DISTINCT
                                           a.District,
                                           a.Student_ID,
                                           ARGEntranceCentersMaster.CenterCode,
                                           (ADMCollegeMaster.CollegeFullName + ' ' + ADMCollegeMaster.Address) [Address]
                                    FROM dbo.ARGPersonalInformation_{printProgramme.ToString()} p
                                        JOIN dbo.ARGStudentAddress_{printProgramme.ToString()} a
                                            ON a.Student_ID = p.Student_ID
                                        JOIN dbo.ARGCentersAllotmentMaster
                                            ON Entity_ID = p.Student_ID
                                        JOIN dbo.ARGEntranceCentersMaster
                                            ON ARGEntranceCentersMaster.Center_ID = ARGCentersAllotmentMaster.Center_ID
                                        JOIN dbo.ADMCollegeMaster
                                            ON ADMCollegeMaster.College_ID = ARGEntranceCentersMaster.College_ID
                                    WHERE Course_ID IN ( '{Course_IDs}')
                                          AND p.Batch = 2021
                                ) a
                                GROUP BY a.District,
                                         a.CenterCode
                                ORDER BY a.District ASC,
                                         a.CenterCode;";


            return new MSSQLFactory().GetDataTable(TSQL);
        }

        #endregion

        #region Upload result
        public string GetUploadEntranceResultTSQL(EntranceResult entranceResult)
        {
            PrintProgramme printProgramme = GeneralFunctions.ProgrammeToPrintProgrammeMapping(entranceResult.Programme);
            Tuple<string, List<Guid[]>> tuple = GetCourseGrouping(printProgramme);
            string Course_IDs = $"{entranceResult.Course_ID}";

            string TSQL = "";

            Guid GeneralAppitudeCourse_ID =
                           Guid.Parse(Convert.ToString(ConfigurationManager.AppSettings["GeneralApitudeCourse_ID"]));

            if (GeneralAppitudeCourse_ID == entranceResult.Course_ID)
            {
                TSQL = $@"UPDATE P SET P.CATEntrancePoints={entranceResult.EntracePoints}
                        FROM ARGPersonalInformation_{printProgramme.ToString()} P
                        JOIN ARGCoursesApplied_{printProgramme.ToString()} A ON A.Student_ID = P.Student_ID
                        WHERE Batch={entranceResult.Batch} AND A.Course_ID NOT IN('fc32e138-4ee2-4da2-9453-5c8368180bc3') AND P.EntranceRollNo={entranceResult.EntranceRollNo};";
            }
            else
            {
                Course_IDs = $"{entranceResult.Course_ID}";
                if (tuple.Item1.ToLower().Contains(entranceResult.Course_ID.ToString().ToLower()))
                {
                    Course_IDs = tuple.Item2.Find(x => (x[0] == entranceResult.Course_ID || x[1] == entranceResult.Course_ID))?.ToSingleString().Replace("|", "','");
                }

                TSQL = $@"UPDATE A SET A.AppearedInEntrance=1,A.SubjectEntrancePoints={entranceResult.EntracePoints}
                        FROM ARGCoursesApplied_{printProgramme.ToString()} A
                        JOIN ARGPersonalInformation_{printProgramme.ToString()} P ON P.Student_ID = A.Student_ID
                        WHERE Batch={entranceResult.Batch} AND A.Course_ID IN ('{Course_IDs}') AND P.EntranceRollNo={entranceResult.EntranceRollNo};";

                if (entranceResult.Course_ID == new Guid("FC32E138-4EE2-4DA2-9453-5C8368180BC3"))
                {
                    TSQL += $@"{Environment.NewLine} UPDATE P SET P.CATEntrancePoints=0
                        FROM ARGPersonalInformation_{printProgramme.ToString()} P
                        JOIN ARGCoursesApplied_{printProgramme.ToString()} A ON A.Student_ID = P.Student_ID
                        WHERE Batch={entranceResult.Batch} AND A.Course_ID IN('fc32e138-4ee2-4da2-9453-5c8368180bc3') AND P.EntranceRollNo={entranceResult.EntranceRollNo};";
                }
            }
            return TSQL;
        }

        public int UploadEntranceResult(string TSQLScript)
        {
            SqlCommand command = new SqlCommand
            {
                CommandText = TSQLScript,
                CommandTimeout = 5000
            };
            return new MSSQLFactory().ExecuteNonQuery(command);
        }
        #endregion

        #region SelfFinanced List
        public DataTable GetSelfFinancedListByCourse(SelfFinancedList selfFinancedList)
        {
            PrintProgramme printProgramme = GeneralFunctions.ProgrammeToPrintProgrammeMapping(selfFinancedList.Programme);

            string programmequery = selfFinancedList.Programme == Programme.IG
            ? $" IN({(short)Programme.IG},{(short)Programme.HS},{(short)Programme.Professional}) "
            : $" IN({(short)selfFinancedList.Programme}) ";

            string TSQL = $@"SELECT MAX(CourseFullName) CourseFullName,COUNT(CourseFullName) [Total Students],ARGCoursesApplied_{printProgramme.ToString()}.Course_ID FROM ARGPersonalInformation_{printProgramme.ToString()}
                            JOIN ARGCoursesApplied_{printProgramme.ToString()} ON ARGCoursesApplied_{printProgramme.ToString()}.Student_ID = ARGPersonalInformation_{printProgramme.ToString()}.Student_ID
                            JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = ARGCoursesApplied_{printProgramme.ToString()}.Course_ID
                            WHERE Batch={selfFinancedList.Batch} AND SelfFinancedPayment_ID IS NOT NULL AND Programme={(short)selfFinancedList.Programme}
                            GROUP BY ARGCoursesApplied_{printProgramme.ToString()}.Course_ID";

            if (selfFinancedList.Programme == Programme.IG)
            {
                TSQL = $@"SELECT Max(s.CourseFullName) CourseFullName,COUNT(s.Student_ID) [Total Students]
                    FROM
                    (
                        SELECT DISTINCT
                               REPLACE(REPLACE(CourseFullName, 'Integrated ', ''), ' Honor''s', '') CourseFullName,
                               GroupName,
                               ARGCoursesApplied_{printProgramme.ToString()}.Student_ID
                        FROM ARGPersonalInformation_{printProgramme.ToString()}
                            JOIN ARGCoursesApplied_{printProgramme.ToString()}
                                ON ARGCoursesApplied_{printProgramme.ToString()}.Student_ID = ARGPersonalInformation_{printProgramme.ToString()}.Student_ID
                            JOIN ADMCourseMaster
                                ON ADMCourseMaster.Course_ID = ARGCoursesApplied_{printProgramme.ToString()}.Course_ID
                        WHERE Batch = {selfFinancedList.Batch}
                              AND SelfFinancedPayment_ID IS NOT NULL
                              AND Programme{programmequery}
                    ) s
                    GROUP BY s.GroupName;";
            }
            return new MSSQLFactory().GetDataTable(TSQL);
        }

        public DataTable GetSelfFinancedListByIndividual(SelfFinancedList selfFinancedList)
        {
            PrintProgramme printProgramme = GeneralFunctions.ProgrammeToPrintProgrammeMapping(selfFinancedList.Programme);
            string programmequery = selfFinancedList.Programme == Programme.IG
             ? $" IN({(short)Programme.IG},{(short)Programme.HS},{(short)Programme.Professional}) "
             : $" IN({(short)selfFinancedList.Programme}) ";

            string orderBy = selfFinancedList.Programme == Programme.IG
             ? $" ORDER BY  cm.CourseFullName,[Percentage] DESC"
             : $" ORDER BY cm.CourseFullName,(ISNULL(p.CATEntrancePoints,0)+ca.SubjectEntrancePoints) DESC,ca.SubjectEntrancePoints DESC";

            string TSQL = $@"SELECT p.StudentFormNo,p.EntranceRollNo,cm.CourseFullName,ca.Preference,p.FullName,p.FathersName,ca.SubjectEntrancePoints##,a.Mobile
                            ,CAST(pm.TxnDate AS DATE) AS Dated
                            FROM ARGPersonalInformation_{printProgramme.ToString()} p
                            JOIN ARGCoursesApplied_{printProgramme.ToString()} ca ON ca.Student_ID = p.Student_ID
                            JOIN ADMCourseMaster cm ON cm.Course_ID = ca.Course_ID
                            JOIN ARGStudentAddress_{printProgramme.ToString()} a ON a.Student_ID = p.Student_ID
                            JOIN dbo.ARGStudentPreviousQualifications_{printProgramme.ToString()} q ON q.Student_ID=p.Student_ID
                            JOIN dbo.PaymentDetails_{printProgramme.ToString()} pm ON pm.Payment_ID=ca.SelfFinancedPayment_ID
                            WHERE p.Batch={selfFinancedList.Batch} AND cm.Programme{programmequery} AND ca.SelfFinancedPayment_ID IS NOT NULL AND (q.ExamName='12th' OR q.ExamName='Graduation')
                            {orderBy}";

            TSQL = (selfFinancedList.Programme == Programme.IG || selfFinancedList.Programme == Programme.UG)
                ? TSQL.Replace("##", ",q.MarksObt,q.MaxMarks,CAST(((CAST(((q.MarksObt / q.MaxMarks) * 100) AS DECIMAL(5, 2)) * 100)/100) AS DECIMAL(5,2)) [Percentage]")
                : TSQL.Replace("##", @",CAST(((CAST(((q.MarksObt / q.MaxMarks) * 100) AS DECIMAL(5, 2)) * 40)/100) AS DECIMAL(5,2))+ ca.SubjectEntrancePoints TotalPoints");
            return new MSSQLFactory().GetDataTable(TSQL);
        }
        #endregion

        #region VerifyFirstSemesterRR

        public List<FirstSemesterRR> GetFirstSemesterRR(SearchFirstSemesterRR searchFirstSemesterRR)
        {
            PrintProgramme printProgramme = GeneralFunctions.ProgrammeToPrintProgrammeMapping(searchFirstSemesterRR.Programme);
            string TSQL = $@"SELECT ROW_NUMBER() OVER (ORDER BY EntranceRollNo) AS Sno,
                           StudentFormNo,
                           EntranceRollNo,
                           FullName,
                           FathersName,
                           Category,
                           CourseFullName AS CourseAllotedByCollege,
                           CollegeFullName,
                           Mobile,pp.TxnAmount AS AmountPaid,
                           ISNULL(CATEntrancePoints, 0) AS CATEntrancePoints,
                           ARGPersonalInformation_{printProgramme.ToString()}.Student_ID,
                           ARGPersonalInformation_{printProgramme.ToString()}.UpdatedOn AS AcceptedOn
                    FROM ARGPersonalInformation_{printProgramme.ToString()}
                        JOIN ARGStudentAddress_{printProgramme.ToString()}
                            ON ARGStudentAddress_{printProgramme.ToString()}.Student_ID = ARGPersonalInformation_{printProgramme.ToString()}.Student_ID
                        LEFT JOIN ARGSelectedCombination_{printProgramme.ToString()}
                            ON ARGSelectedCombination_{printProgramme.ToString()}.Student_ID = ARGPersonalInformation_{printProgramme.ToString()}.Student_ID
                        JOIN ADMCombinationMaster
                            ON ADMCombinationMaster.Combination_ID = ARGSelectedCombination_{printProgramme.ToString()}.Combination_ID
                        JOIN ADMCourseMaster
                            ON ADMCourseMaster.Course_ID = ADMCombinationMaster.Course_ID
                        JOIN ADMCollegeMaster
                            ON ADMCollegeMaster.College_ID = ARGPersonalInformation_{printProgramme.ToString()}.AcceptCollege_ID
                        LEFT JOIN dbo.PaymentDetails_{printProgramme.ToString()} pp ON pp.Entity_ID=ARGPersonalInformation_{printProgramme.ToString()}.Student_ID 
										AND pp.Semester=1 AND pp.ModuleType={(short)PaymentModuleType.SemesterAdmission} 
                    WHERE Batch = {searchFirstSemesterRR.Batch}
                          AND ARGSelectedCombination_{printProgramme.ToString()}.Semester = 1
                          AND AcceptCollege_ID='{searchFirstSemesterRR.College_ID}' 
                          ## ORDER BY CourseFullName ASC, pp.TxnAmount ASC";

            if (!string.IsNullOrWhiteSpace(searchFirstSemesterRR.EntranceRollNos))
            {
                TSQL += $" AND EntranceRollNo IN({searchFirstSemesterRR.EntranceRollNos})";
                TSQL = TSQL.Replace("##", "");
            }
            else
            {
                TSQL = TSQL.Replace("##", $" AND ADMCombinationMaster.Course_ID = '{searchFirstSemesterRR.Course_ID}'");
            }

            return new MSSQLFactory().GetObjectList<FirstSemesterRR>(TSQL);
        }

        public List<string> GetCoursesAppliedByStudent(Programme programme, Guid Student_ID, decimal CATPoints)
        {
            PrintProgramme printProgramme = GeneralFunctions.ProgrammeToPrintProgrammeMapping(programme);
            string TSQL = "";
            if (printProgramme == PrintProgramme.IH)
            {
                TSQL = $@"SELECT CourseFullName + '('
                           + CAST((SubjectEntrancePoints + CASE
                                                               WHEN q.Qualification_ID IS NULL THEN
                                                                   0
                                                               ELSE
                                                                   CAST(((q.MarksObt / q.MaxMarks) * 40) AS DECIMAL(5, 2))
                                                           END
                                  ) AS VARCHAR(8)) + ')' AS a
                           FROM ARGCoursesApplied_{printProgramme.ToString()} c
                               JOIN ADMCourseMaster
                                   ON ADMCourseMaster.Course_ID = c.Course_ID
                                 LEFT JOIN dbo.ARGStudentPreviousQualifications_{printProgramme.ToString()} q
                                        ON q.Student_ID = c.Student_ID 
                           WHERE c.Student_ID = '{Student_ID}'";
            }
            else if (printProgramme == PrintProgramme.UG)
            {
                TSQL = $@"SELECT CourseFullName + '('
                           + CAST(({CATPoints} + CASE
                                                    WHEN q.Qualification_ID IS NULL THEN
                                                        0
                                                    ELSE
                                                        CAST(((q.MarksObt / q.MaxMarks) * 40) AS DECIMAL(5, 2))
                                                END
                                  ) AS VARCHAR(8)) + ')' AS a
                           FROM ARGCoursesApplied_{printProgramme.ToString()} c
                               JOIN ADMCourseMaster
                                   ON ADMCourseMaster.Course_ID = c.Course_ID
                                 LEFT JOIN dbo.ARGStudentPreviousQualifications_{printProgramme.ToString()} q
                                        ON q.Student_ID = c.Student_ID 
                           WHERE c.Student_ID = '{Student_ID}'";
            }
            else
            {
                TSQL = $@"SELECT CourseFullName + '(' + CAST((SubjectEntrancePoints+{CATPoints}) AS VARCHAR(8)) + ')' AS a
                           FROM ARGCoursesApplied_{printProgramme.ToString()}
                               JOIN ADMCourseMaster
                                   ON ADMCourseMaster.Course_ID = ARGCoursesApplied_{printProgramme.ToString()}.Course_ID
                           WHERE Student_ID = '{Student_ID}'";
            }
            return new MSSQLFactory().GetSingleValues<string>(TSQL);
        }
        #endregion

        #region DeleteJunkData

        public List<string> GetJunkPhotoPaths(PrintProgramme printProgramme, short BatchToDeleteJunk, bool onlyLateralEntry, DateTime appliedBefore)
        {
            string appliedbfr = (appliedBefore != DateTime.MinValue && appliedBefore != DateTime.MaxValue)
                                ? $" AND CAST(p.CreatedOn AS Date)<'{appliedBefore.ToString("yyyy-MM-dd")}' " : "";

            SqlCommand command = new SqlCommand($@"SELECT TOP 100 Photograph
                                                FROM ARGPersonalInformation_{printProgramme.ToString()} p 
                                                LEFT JOIN {printProgramme.ToString()}_Semester1 s ON s.Student_ID = p.Student_ID
                                                WHERE Batch = {BatchToDeleteJunk} AND AcceptCollege_ID IS NULL 
                                                AND p.IsLateralEntry={(onlyLateralEntry ? "1" : "0")} {appliedbfr}
                                                AND s.Student_ID IS NULL AND CurrentSemesterOrYear<=1 ORDER BY StudentFormNo ASC;");
            return new MSSQLFactory().GetSingleValues<string>(command);
        }
        public List<string> GetJunkCertificatesPaths(PrintProgramme printProgramme, short BatchToDeleteJunk, bool onlyLateralEntry, DateTime appliedBefore)
        {
            string appliedbfr = (appliedBefore != DateTime.MinValue && appliedBefore != DateTime.MaxValue)
                                ? $" AND CAST(p.CreatedOn AS Date)<'{appliedBefore.ToString("yyyy-MM-dd")}' " : "";

            SqlCommand command = new SqlCommand($@"SELECT CertificateUrl FROM dbo.Certificate WHERE Student_ID IN
                                                (SELECT  TOP 100 p.Student_ID FROM ARGPersonalInformation_{printProgramme.ToString()} p 
                                                LEFT JOIN {printProgramme.ToString()}_Semester1 s ON s.Student_ID = p.Student_ID
                                                WHERE Batch = {BatchToDeleteJunk} AND AcceptCollege_ID IS NULL AND p.IsLateralEntry={(onlyLateralEntry ? "1" : "0")}
                                                {appliedbfr}
                                                AND s.Student_ID IS NULL AND CurrentSemesterOrYear<=1 ORDER BY StudentFormNo ASC);");
            return new MSSQLFactory().GetSingleValues<string>(command);
        }

        public int DeleteJunk(PrintProgramme printProgramme, short BatchToDeleteJunk, bool onlyLateralEntry, DateTime appliedBefore)
        {
            string appliedbfr = (appliedBefore != DateTime.MinValue && appliedBefore != DateTime.MaxValue)
                                ? $" AND CAST(p.CreatedOn AS Date)<'{appliedBefore.ToString("yyyy-MM-dd")}' " : "";

            SqlCommand command = new SqlCommand($@"DELETE FROM ARGCoursesApplied_{printProgramme.ToString()} WHERE Student_ID in
                                                    (SELECT  TOP 100 p.Student_ID FROM ARGPersonalInformation_{printProgramme.ToString()} p 
                                                    LEFT JOIN {printProgramme.ToString()}_Semester1 s ON s.Student_ID = p.Student_ID
                                                    WHERE Batch = {BatchToDeleteJunk} AND AcceptCollege_ID IS NULL  AND p.IsLateralEntry={(onlyLateralEntry ? "1" : "0")} 
                                                    {appliedbfr}
                                                    AND s.Student_ID IS NULL AND CurrentSemesterOrYear<=1 ORDER BY StudentFormNo ASC);

                                                    DELETE FROM ARGSelectedCombination_{printProgramme.ToString()} WHERE Student_ID in
                                                    (SELECT  TOP 100 p.Student_ID FROM ARGPersonalInformation_{printProgramme.ToString()} p 
                                                    LEFT JOIN {printProgramme.ToString()}_Semester1 s ON s.Student_ID = p.Student_ID
                                                    WHERE Batch = {BatchToDeleteJunk} AND AcceptCollege_ID IS NULL  AND p.IsLateralEntry={(onlyLateralEntry ? "1" : "0")} 
                                                    {appliedbfr}
                                                    AND s.Student_ID IS NULL AND CurrentSemesterOrYear<=1 ORDER BY StudentFormNo ASC);

                                                    DELETE FROM ARGStudentPreviousQualifications_{printProgramme.ToString()} WHERE Student_ID in
                                                    (SELECT  TOP 100 p.Student_ID FROM ARGPersonalInformation_{printProgramme.ToString()} p 
                                                    LEFT JOIN {printProgramme.ToString()}_Semester1 s ON s.Student_ID = p.Student_ID
                                                    WHERE Batch = {BatchToDeleteJunk} AND AcceptCollege_ID IS NULL  AND p.IsLateralEntry={(onlyLateralEntry ? "1" : "0")} 
                                                    {appliedbfr}
                                                    AND s.Student_ID IS NULL AND CurrentSemesterOrYear<=1 ORDER BY StudentFormNo ASC);

                                                    DELETE FROM ARGStudentAddress_{printProgramme.ToString()} WHERE Student_ID in
                                                    (SELECT  TOP 100 p.Student_ID FROM ARGPersonalInformation_{printProgramme.ToString()} p 
                                                    LEFT JOIN {printProgramme.ToString()}_Semester1 s ON s.Student_ID = p.Student_ID
                                                    WHERE Batch = {BatchToDeleteJunk} AND AcceptCollege_ID IS NULL  AND p.IsLateralEntry={(onlyLateralEntry ? "1" : "0")} 
                                                    {appliedbfr}
                                                    AND s.Student_ID IS NULL AND CurrentSemesterOrYear<=1 ORDER BY StudentFormNo ASC);

                                                    DELETE FROM PaymentDetails_{printProgramme.ToString()} WHERE Entity_ID in
                                                    (SELECT  TOP 100 p.Student_ID FROM ARGPersonalInformation_{printProgramme.ToString()} p 
                                                    LEFT JOIN {printProgramme.ToString()}_Semester1 s ON s.Student_ID = p.Student_ID
                                                    WHERE Batch = {BatchToDeleteJunk} AND AcceptCollege_ID IS NULL  AND p.IsLateralEntry={(onlyLateralEntry ? "1" : "0")} 
                                                    {appliedbfr}
                                                    AND s.Student_ID IS NULL AND CurrentSemesterOrYear<=1 ORDER BY StudentFormNo ASC);

                                                    DELETE FROM StudentCollegePreference WHERE Student_ID IN
                                                    (SELECT  TOP 100 p.Student_ID FROM ARGPersonalInformation_{printProgramme.ToString()} p 
                                                    LEFT JOIN {printProgramme.ToString()}_Semester1 s ON s.Student_ID = p.Student_ID
                                                    WHERE Batch = {BatchToDeleteJunk} AND AcceptCollege_ID IS NULL  AND p.IsLateralEntry={(onlyLateralEntry ? "1" : "0")}
                                                    {appliedbfr}
                                                    AND s.Student_ID IS NULL AND CurrentSemesterOrYear<=1 ORDER BY StudentFormNo ASC);

                                                    DELETE FROM [Certificate] WHERE Student_ID in
                                                     (SELECT  TOP 100 p.Student_ID FROM ARGPersonalInformation_{printProgramme.ToString()} p 
                                                    LEFT JOIN {printProgramme.ToString()}_Semester1 s ON s.Student_ID = p.Student_ID
                                                    WHERE Batch = {BatchToDeleteJunk} AND AcceptCollege_ID IS NULL  AND p.IsLateralEntry={(onlyLateralEntry ? "1" : "0")} 
                                                    {appliedbfr}
                                                    AND s.Student_ID IS NULL AND CurrentSemesterOrYear<=1 ORDER BY StudentFormNo ASC);

                                                    DELETE FROM NEPCollegePreferences WHERE Student_ID IN
                                                    (SELECT  TOP 100 p.Student_ID FROM ARGPersonalInformation_{printProgramme.ToString()} p 
                                                    LEFT JOIN {printProgramme.ToString()}_Semester1 s ON s.Student_ID = p.Student_ID
                                                    WHERE Batch = {BatchToDeleteJunk} AND AcceptCollege_ID IS NULL  AND p.IsLateralEntry={(onlyLateralEntry ? "1" : "0")}  {appliedbfr}
                                                    AND s.Student_ID IS NULL AND CurrentSemesterOrYear<=1 ORDER BY StudentFormNo ASC);

                                                    DELETE FROM ARGPersonalInformation_{printProgramme.ToString()} WHERE Student_ID IN
                                                    (SELECT  TOP 100 p.Student_ID FROM ARGPersonalInformation_{printProgramme.ToString()} p 
                                                    LEFT JOIN {printProgramme.ToString()}_Semester1 s ON s.Student_ID = p.Student_ID
                                                    WHERE Batch = {BatchToDeleteJunk} AND AcceptCollege_ID IS NULL  AND p.IsLateralEntry={(onlyLateralEntry ? "1" : "0")}  {appliedbfr}
                                                    AND s.Student_ID IS NULL AND CurrentSemesterOrYear<=1 ORDER BY StudentFormNo ASC);
                                                    
                                                    
                                                    DELETE FROM dbo.SeatAllocationMatrix_{printProgramme.ToString()} WHERE Student_Id IN
                                                    (SELECT  TOP 100 p.Student_ID FROM ARGPersonalInformation_{printProgramme.ToString()} p 
                                                    LEFT JOIN {printProgramme.ToString()}_Semester1 s ON s.Student_ID = p.Student_ID
                                                    WHERE Batch = {BatchToDeleteJunk} AND AcceptCollege_ID IS NULL  AND p.IsLateralEntry={(onlyLateralEntry ? "1" : "0")}  {appliedbfr}
                                                    AND s.Student_ID IS NULL AND CurrentSemesterOrYear<=1 ORDER BY StudentFormNo ASC);

                                                    ");

            command.CommandTimeout = 9000;
            return new MSSQLFactory().ExecuteNonQuery(command);
        }
        #endregion

        #region DeleteCertificateCoursesJunk
        public List<string> GetJunkPhotoPaths()
        {
            SqlCommand command = new SqlCommand($@"SELECT Photograph FROM CertificateCoursePersonalInfo WHERE FormStatus=1;");
            return new MSSQLFactory().GetSingleValues<string>(command);
        }

        public int DeleteJunk()
        {
            SqlCommand command = new SqlCommand($@"DELETE FROM CertificateCoursePrevQualifications WHERE Student_ID IN
                                                    (SELECT Student_ID FROM CertificateCoursePersonalInfo WHERE FormStatus=1);
                                                    DELETE FROM CertificateCoursePersonalInfo WHERE FormStatus=1;");
            return new MSSQLFactory().ExecuteNonQuery(command);
        }
        #endregion

        #region PDF Merit Lists
        public MeritListsAsPDF PDFMeritLists(PDFMeritListFilter meritLists)
        {
            MeritListsAsPDF meritListsPDF = new MeritListsAsPDF();
            meritLists.PrintProgramme = new GeneralFunctions().MappingTable(meritLists.PrintProgramme);
            meritListsPDF.PrintProgramme = meritLists.PrintProgramme;

            Tuple<string, List<Guid[]>> tuple = GetCourseGrouping(meritLists.PrintProgramme);
            string Course_IDs = $"{meritLists.Course_ID}";

            if (tuple?.Item1?.ToLower().Contains(meritLists.Course_ID.ToString().ToLower()) ?? false)
            {
                Course_IDs = tuple.Item2.Find(x => (x[0] == meritLists.Course_ID || x[1] == meritLists.Course_ID))?.ToSingleString().Replace("|", "','");
            }
            string TSQL = "";
            meritListsPDF.CourseFullNames = new MSSQLFactory().GetSingleValues<string>
                  ($@"SELECT DISTINCT CourseFullName FROM dbo.ADMCourseMaster WHERE Course_ID IN ('{Course_IDs}')");

            if (meritLists.PrintProgramme == PrintProgramme.UG && meritLists.PDFListType == EntrancePDFListType.GeneralMeritList)
            {
                TSQL = $@"SELECT DISTINCT
                               P.EntranceRollNo,
                               P.CATEntrancePoints,
                            P.CUETApplicationNo,
                            P.CUETEntranceRollNo,
                        FROM dbo.ARGPersonalInformation_UG P
                        WHERE  P.Batch = {meritLists.Batch} AND p.FormStatus IN (10)
                        AND P.CATEntrancePoints IS NOT NULL
                        ORDER BY P.EntranceRollNo ASC;";
                meritListsPDF.PDFListType = EntrancePDFListType.GeneralMeritList;
            }
            else if (meritLists.PrintProgramme == PrintProgramme.UG && meritLists.PDFListType == EntrancePDFListType.CombinedMeritList)
            {
                return null;
            }
            else if (meritLists.PrintProgramme == PrintProgramme.UG && meritLists.PDFListType == EntrancePDFListType.CombinedMeritListAcademics)
            {
                if (meritLists.Course_ID != Guid.Parse(Convert.ToString(ConfigurationManager.AppSettings["GeneralApitudeCourse_ID"])))
                {
                    TSQL = $@"SELECT DISTINCT
                            P.EntranceRollNo,
                            P.CATEntrancePoints,
                            P.CUETApplicationNo,
                            P.CUETEntranceRollNo,
                            CASE
                                WHEN P.IsProvisional = 1 THEN
                                    'NA'
                                ELSE
                            (CAST(q.MarksObt AS VARCHAR(10)) + '/' + CAST(q.MaxMarks AS VARCHAR(10)))
                            END AS MarksObtained12th,
                            CASE
                                WHEN P.IsProvisional = 1 THEN
                                    'NA'
                                ELSE
                                    CAST(CAST(((q.MarksObt / q.MaxMarks) * 40) AS DECIMAL(5, 2)) AS VARCHAR(10))
                            END AS PointsFrom12th
                            ,CASE
                                   WHEN P.IsProvisional = 1 THEN
                                       P.CATEntrancePoints
                                   ELSE
                                      CAST(((CAST(((q.MarksObt / q.MaxMarks) * 100) AS DECIMAL(5, 2)) * 40)/100) AS DECIMAL(5,2))+ P.CATEntrancePoints
                               END AS TotalPoints
                    FROM dbo.ARGPersonalInformation_UG P
                        JOIN dbo.ARGStudentAddress_UG ad ON ad.Student_ID=P.Student_ID
                        JOIN dbo.ARGStudentPreviousQualifications_UG q
                            ON q.Student_ID = p.Student_ID 
                                AND q.ExamName = '12th'
								JOIN dbo.ARGCoursesApplied_UG ca ON ca.Student_ID = ad.Student_ID AND ca.Course_ID  IN ('{Course_IDs}')
                    WHERE  P.Batch = {meritLists.Batch} AND P.CATEntrancePoints IS NOT NULL 
                            AND P.FormStatus=10 AND P.EntranceRollNo IS NOT NULL ORDER BY P.EntranceRollNo ASC";
                }
                else
                {
                    TSQL = $@"SELECT DISTINCT
                            P.EntranceRollNo,
                            P.CATEntrancePoints,
                            P.CUETApplicationNo,
                            P.CUETEntranceRollNo,
                            CASE
                                WHEN P.IsProvisional = 1 THEN
                                    'NA'
                                ELSE
                            (CAST(q.MarksObt AS VARCHAR(10)) + '/' + CAST(q.MaxMarks AS VARCHAR(10)))
                            END AS MarksObtained12th,
                            CASE
                                WHEN P.IsProvisional = 1 THEN
                                    'NA'
                                ELSE
                                    CAST(CAST(((q.MarksObt / q.MaxMarks) * 40) AS DECIMAL(5, 2)) AS VARCHAR(10))
                            END AS PointsFrom12th
                            ,CASE
                                   WHEN P.IsProvisional = 1 THEN
                                       P.CATEntrancePoints
                                   ELSE
                                      CAST(((CAST(((q.MarksObt / q.MaxMarks) * 100) AS DECIMAL(5, 2)) * 40)/100) AS DECIMAL(5,2))+ P.CATEntrancePoints
                               END AS TotalPoints
                    FROM dbo.ARGPersonalInformation_UG P
                        JOIN dbo.ARGStudentAddress_UG ad ON ad.Student_ID=P.Student_ID
                        JOIN dbo.ARGStudentPreviousQualifications_UG q
                            ON q.Student_ID = p.Student_ID 
                                AND q.ExamName = '12th'
                    WHERE  P.Batch = {meritLists.Batch} AND P.CATEntrancePoints IS NOT NULL 
                            AND P.FormStatus=10 AND P.EntranceRollNo IS NOT NULL ORDER BY P.EntranceRollNo ASC";
                }

                meritListsPDF.PDFListType = EntrancePDFListType.CombinedMeritListAcademics;
            }
            else if (meritLists.PDFListType == EntrancePDFListType.CombinedMeritListAcademics)
            {
                TSQL = $@"SELECT DISTINCT
                               P.EntranceRollNo,
                                P.CUETApplicationNo,
                                P.CUETEntranceRollNo,
                               P.Category AS StudentCategory,
                               a.SubjectEntrancePoints,
                               CASE
                                   WHEN P.IsProvisional = 1 THEN
                                       'NA'
                                   ELSE
                               (CAST(q.MarksObt AS VARCHAR(10)) + '/' + CAST(q.MaxMarks AS VARCHAR(10)))
                               END AS MarksObtained12th,
                               CASE
                                   WHEN P.IsProvisional = 1 THEN
                                       'NA'
                                   ELSE
                                       CAST(CAST(((CAST(((q.MarksObt / q.MaxMarks) * 100) AS DECIMAL(5, 2)) * 40) / 100) AS DECIMAL(5, 2)) AS VARCHAR(10))
                               END AS PointsFrom12th
                               ,CASE
                                   WHEN P.IsProvisional = 1 THEN
                                       a.SubjectEntrancePoints
                                   ELSE
                                      CAST(((CAST(((q.MarksObt / q.MaxMarks) * 100) AS DECIMAL(5, 2)) * 40)/100) AS DECIMAL(5,2))+ a.SubjectEntrancePoints
                               END AS TotalPoints
                        FROM dbo.ARGPersonalInformation_{meritLists.PrintProgramme.ToString()} P
                            JOIN dbo.ARGCoursesApplied_{meritLists.PrintProgramme.ToString()} a
                                ON a.Student_ID = P.Student_ID AND a.Course_ID IN ('{Course_IDs}')
                            LEFT JOIN dbo.ARGStudentPreviousQualifications_{meritLists.PrintProgramme.ToString()} q
                                ON q.Student_ID = p.Student_ID 
                                   AND q.ExamName = '12th'
                        WHERE  P.Batch = {meritLists.Batch}
                              AND a.AppearedInEntrance = 1 AND P.EntranceRollNo IS NOT NULL
                        ORDER BY P.EntranceRollNo ASC";
                meritListsPDF.PDFListType = meritLists.PDFListType;
            }
            else
            {
                TSQL = $@"SELECT DISTINCT
                               EntranceRollNo,
                                CUETApplicationNo,
                                CUETEntranceRollNo,
                               Category AS StudentCategory,
                               SubjectEntrancePoints,
                               CATEntrancePoints,
                               (SubjectEntrancePoints + ISNULL(CATEntrancePoints, 0)) AS TotalPoints
                        FROM dbo.ARGPersonalInformation_{meritLists.PrintProgramme.ToString()}
                            JOIN dbo.ARGCoursesApplied_{meritLists.PrintProgramme.ToString()}
                                ON ARGCoursesApplied_{meritLists.PrintProgramme.ToString()}.Student_ID = ARGPersonalInformation_{meritLists.PrintProgramme.ToString()}.Student_ID
                        WHERE Course_ID IN ( '{Course_IDs}' )
                              AND Batch = {meritLists.Batch} AND AppearedInEntrance=1 ORDER BY EntranceRollNo ASC;";

                meritListsPDF.PDFListType = meritLists.PDFListType;

                if (meritLists.Course_ID == Guid.Parse("a3ee7f98-7b82-4d95-a2c0-faba7a18240e"))
                {
                    TSQL = $@"SELECT DISTINCT
                               EntranceRollNo,
                                CUETApplicationNo,
                                CUETEntranceRollNo,
                               Category AS StudentCategory,
                               CATEntrancePoints
                        FROM dbo.ARGPersonalInformation_{meritLists.PrintProgramme.ToString()}
                            JOIN dbo.ARGCoursesApplied_{meritLists.PrintProgramme.ToString()}
                                ON ARGCoursesApplied_{meritLists.PrintProgramme.ToString()}.Student_ID = ARGPersonalInformation_{meritLists.PrintProgramme.ToString()}.Student_ID
                        WHERE Course_ID IN ( '{Course_IDs}' )
                              AND Batch = {meritLists.Batch} AND CATEntrancePoints IS NOT NULL ORDER BY EntranceRollNo ASC;";
                    meritListsPDF.PDFListType = EntrancePDFListType.GeneralMeritList;
                }
                else if (meritLists.Course_ID == Guid.Parse("1A4A791F-FE3A-4668-A18B-C71403FD258B"))
                {
                    TSQL = $@"SELECT DISTINCT
                               EntranceRollNo,
                               CUETApplicationNo,
                               CUETEntranceRollNo,
                               Category AS StudentCategory,
                               CATEntrancePoints
                        FROM dbo.ARGPersonalInformation_{meritLists.PrintProgramme.ToString()}
                        WHERE Batch = {meritLists.Batch} AND CATEntrancePoints IS NOT NULL ORDER BY EntranceRollNo ASC;";
                    meritListsPDF.PDFListType = EntrancePDFListType.GeneralMeritList;
                }
            }
            meritListsPDF.MeritListsDetails = new MSSQLFactory().GetObjectList<MeritLists>(TSQL);
            meritListsPDF.Batch = meritLists.Batch;
            meritListsPDF.MSGOnBottomOfPDF = meritLists.MSGOnBottomOfPDF;
            return meritListsPDF;
        }
        #endregion

        #region RelocateCenter
        public List<DropDownOptLabelGeneral> GetRelocateCenterList()
        {
            return new MSSQLFactory().GetObjectList<DropDownOptLabelGeneral>
                ($@"SELECT DISTINCT ecm.Center_ID AS [Value],ecm.CenterCode AS [Text],cm.CollegeFullName AS DataGroupField FROM dbo.ARGCentersAllotmentMaster cam
                    JOIN dbo.ARGEntranceCentersMaster ecm ON ecm.Center_ID = cam.Center_ID
                    JOIN dbo.ADMCollegeMaster cm ON cm.College_ID = ecm.College_ID
                    WHERE ecm.IsEntrance=1 AND ecm.Status=1 ORDER BY cm.CollegeFullName ASC,ecm.CenterCode ASC");
        }
        public List<SelectListItem> GetRelocateCenterCourseIds(Guid Center_ID)
        {
            return new MSSQLFactory().GetObjectList<SelectListItem>
                ($@"SELECT DISTINCT CAST(cam.Course_ID AS VARCHAR(50))
 AS [Value],cm.CourseFullName AS [Text],cm.Programme FROM dbo.ARGCentersAllotmentMaster cam
                    JOIN dbo.ADMCourseMaster cm ON cm.Course_ID = cam.Course_ID
                    WHERE cam.Center_ID='{Center_ID}'
                    ORDER BY cm.Programme,cm.CourseFullName");
        }

        public int RelocateCenter(RelocateCenter relocateCenter)
        {
            return new MSSQLFactory().ExecuteNonQuery($"UPDATE dbo.ARGCentersAllotmentMaster SET Center_ID='{relocateCenter.ToCenter_ID}' WHERE Center_ID='{relocateCenter.FromCenter_ID}' AND Course_ID='{relocateCenter.FromCourse_ID}';");
        }
        #endregion
    }
}
