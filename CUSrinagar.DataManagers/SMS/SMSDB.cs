using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Terex;
using CUSrinagar.Models;
using System.Data.SqlClient;
using CUSrinagar.Extensions;
using CUSrinagar.Enums;

namespace CUSrinagar.DataManagers
{
    public class SMSDB
    {
        #region CourseDDL
        public List<SelectListItem> GetCoursesDDL(Programme _Programme, short Batch)
        {
            PrintProgramme _PProgramme = GeneralFunctions.ProgrammeToPrintProgrammeMapping(_Programme);
            return new MSSQLFactory().GetObjectList<SelectListItem>
                                        ($@"SELECT DISTINCT Cast(A.Course_ID AS nvarchar(50)) AS [Value],c.CourseFullName AS [Text] FROM ARGPersonalInformation_{_PProgramme.ToString()} P
                                        JOIN ARGCoursesApplied_{_PProgramme.ToString()} A ON A.Student_ID = P.Student_ID
                                        JOIN ADMCourseMaster c ON c.Course_ID = A.Course_ID
                                        WHERE P.Batch={Batch} AND c.Programme={(short)_Programme}");
        }

        public List<SelectListItem> GetCoursesDDL(Programme _Programme, Guid? College_ID)
        {
            string TSQL = $@"SELECT DISTINCT Cast(cm.Course_ID AS nvarchar(50)) AS [Value],cm.CourseFullName AS [Text] FROM ADMCourseMaster cm
                                            JOIN ADMCollegeCourseMapping ccm ON ccm.Course_ID = cm.Course_ID
                                            WHERE ccm.Status=1 AND cm.Status=1 AND Programme={(short)_Programme}";
            if (!College_ID.IsNullOrEmpty())
                TSQL += $" AND ccm.College_ID='{College_ID}'";

            return new MSSQLFactory().GetObjectList<SelectListItem>(TSQL) ?? new List<SelectListItem>();
        }

        public List<SelectListItem> GetCoursesDDL(SMS _SMS)
        {
            PrintProgramme printProgramme = GeneralFunctions.ProgrammeToPrintProgrammeMapping(_SMS.Programme);
            string TSQL = $@"SELECT DISTINCT Cast(cm.Course_ID AS nvarchar(50)) AS [Value],cm.CourseFullName AS [Text] FROM ARGCentersAllotmentMaster ca
                        JOIN ARGEntranceCentersMaster ec ON ec.Center_ID = ca.Center_ID
                        JOIN ADMCollegeMaster c ON c.College_ID=ec.College_ID
                        JOIN ADMCourseMaster cm ON cm.Course_ID = ca.Course_ID
                        JOIN ARGPersonalInformation_{printProgramme.ToString()} p ON p.Student_ID=ca.Entity_ID
                        WHERE cm.Programme={(short)_SMS.Programme} AND p.Batch={_SMS.Batch}";
            if (_SMS?.College_ID != null)
            {
                TSQL += $@" AND ca.Center_ID='{_SMS.College_ID}'";
            }

            return new MSSQLFactory().GetObjectList<SelectListItem>(TSQL) ?? new List<SelectListItem>();
        }

        public List<DropDownOptLabelGeneral> GetCenterList()
        {
            string TSQL = $@"SELECT CONVERT(nvarchar(50), Center_ID) AS Value, CenterCode AS Text, CollegeFullName AS DataGroupField FROM ARGEntranceCentersMaster 
                            JOIN ADMCollegeMaster ON ADMCollegeMaster.College_ID = ARGEntranceCentersMaster.College_ID
                            WHERE ARGEntranceCentersMaster.Status = 1 AND ARGEntranceCentersMaster.IsEntrance=1
                            ORDER BY CollegeCode ASC,CenterCode ASC";
            return new MSSQLFactory().GetObjectList<DropDownOptLabelGeneral>(TSQL);


        }
        #endregion

        public List<string> ComposeSMSForEntranceTest(SMS _SMS)
        {
            PrintProgramme printProgramme = GeneralFunctions.ProgrammeToPrintProgrammeMapping(_SMS.Programme);
            string TSQL = $@"SELECT DISTINCT ad.Mobile FROM ARGPersonalInformation_{printProgramme.ToString()} p
                            JOIN ARGCoursesApplied_{printProgramme.ToString()} a ON a.Student_ID = p.Student_ID
                            JOIN ARGStudentAddress_{printProgramme.ToString()} ad ON ad.Student_ID = p.Student_ID
                            JOIN ADMCourseMaster cm ON cm.Course_ID = a.Course_ID
                            WHERE p.FormStatus={(short)FormStatus.FeePaid} AND P.EntranceRollNo IS NOT NULL AND 
                            cm.Programme={(short)_SMS.Programme} AND p.Batch={_SMS.Batch}";

            if (_SMS.Course_ID.IsNotNullOrEmpty())
            {
                if (_SMS.Course_ID.Contains(new Guid("FC32E138-4EE2-4DA2-9453-5C8368180BC3")) && _SMS.Course_ID.Contains(new Guid("1A4A791F-FE3A-4668-A18B-C71403FD258B")))
                {

                }
                else if (_SMS.Course_ID.Contains(new Guid("1A4A791F-FE3A-4668-A18B-C71403FD258B")))
                {
                    TSQL += $" AND a.Course_ID NOT IN ('FC32E138-4EE2-4DA2-9453-5C8368180BC3')";
                }
                else
                {
                    TSQL += $" AND a.Course_ID IN ({_SMS.Course_ID.ToIN()})";
                }
            }
            return new MSSQLFactory().GetSingleValues<string>(TSQL);
        }

        public List<string> ComposeSMSForEntranceSelection(SMS _SMS)
        {
            PrintProgramme printProgramme = GeneralFunctions.ProgrammeToPrintProgrammeMapping(_SMS.Programme);
            string TSQL = $@"SELECT DISTINCT ad.Mobile FROM ARGPersonalInformation_{printProgramme.ToString()} p
                            JOIN ARGCoursesApplied_{printProgramme.ToString()} a ON a.Student_ID = p.Student_ID
                            JOIN ARGStudentAddress_{printProgramme.ToString()} ad ON ad.Student_ID = p.Student_ID
                            JOIN ADMCourseMaster cm ON cm.Course_ID = a.Course_ID 
                            WHERE a.AppearedInEntrance=1 and a.StudentSelectionStatus={(short)StudentSelectionStatus.Provisional} 
                            AND cm.Programme={(short)_SMS.Programme} AND p.Batch={_SMS.Batch}";

            if (_SMS.Course_ID.IsNotNullOrEmpty())
            {
                TSQL += $" AND a.Course_ID IN ({_SMS.Course_ID.ToIN()})";
            }
            return new MSSQLFactory().GetSingleValues<string>(TSQL);
        }

        public List<string> ComposeSMSForSemester(SMS _SMS)
        {
            PrintProgramme printProgramme = GeneralFunctions.ProgrammeToPrintProgrammeMapping(_SMS.Programme);
            string TSQL = $@"SELECT DISTINCT ad.Mobile FROM VWStudentCourse vw
                        JOIN ARGPersonalInformation_{printProgramme.ToString()} p ON p.Student_ID=vw.Student_ID
                        JOIN ARGStudentAddress_{printProgramme.ToString()} ad ON ad.Student_ID = p.Student_ID
                        JOIN ADMCourseMaster cm ON cm.Course_ID = vw.Course_ID
                        WHERE p.Batch={_SMS.Batch} AND cm.Programme={(short)_SMS.Programme} AND p.FormStatus NOT IN(1,3,5,6,14,13)";

            if (_SMS.Course_ID.IsNotNullOrEmpty())
            {
                TSQL += $" AND vw.Course_ID IN ({_SMS.Course_ID.ToIN()})";
            }

            if (!_SMS.College_ID.IsNullOrEmpty())
            {
                TSQL += $" AND p.AcceptCollege_ID='{_SMS.College_ID}'";
            }
            else
            {
                TSQL += $" AND p.AcceptCollege_ID IS NOT NULL";
            }

            return new MSSQLFactory().GetSingleValues<string>(TSQL);
        }

        public List<string> ComposeSMSForEntranceCenterRelocation(SMS _SMS)
        {
            PrintProgramme printProgramme = GeneralFunctions.ProgrammeToPrintProgrammeMapping(_SMS.Programme);
            string TSQL = $@"SELECT DISTINCT a.Mobile FROM ARGPersonalInformation_{printProgramme.ToString()} p
                            JOIN ARGCentersAllotmentMaster cam ON cam.Entity_ID=p.Student_ID
                            JOIN ARGStudentAddress_{printProgramme.ToString()} a ON a.Student_ID = p.Student_ID
                            WHERE p.Batch={_SMS.Batch}";

            if (!_SMS.College_ID.IsNullOrEmpty())
                TSQL += $" AND cam.Center_ID='{_SMS.College_ID}'";

            if (_SMS.Course_ID.IsNotNullOrEmpty())
                TSQL += $" AND cam.Course_ID IN ({_SMS.Course_ID.ToIN()})";

            return new MSSQLFactory().GetSingleValues<string>(TSQL);
        }

        public List<string> ComposeSMSByEntranceRollNo(SMS _SMS)
        {
            PrintProgramme printProgramme = GeneralFunctions.ProgrammeToPrintProgrammeMapping(_SMS.Programme);
            string TSQL = $@"SELECT DISTINCT a.Mobile FROM ARGPersonalInformation_{printProgramme.ToString()} p
                            JOIN ARGStudentAddress_{printProgramme.ToString()} a ON a.Student_ID = p.Student_ID
                            JOIN ARGCoursesApplied_{printProgramme.ToString()} ca ON ca.Student_ID = p.Student_ID
                            JOIN ADMCourseMaster c ON c.Course_ID = ca.Course_ID
                            WHERE c.Programme={(short)_SMS.Programme} AND p.Batch={_SMS.Batch} AND p.EntranceRollNo IN({_SMS.RollNos})";

            if (_SMS.Course_ID.IsNotNullOrEmpty())
            {
                if (_SMS.Course_ID.Contains(new Guid("FC32E138-4EE2-4DA2-9453-5C8368180BC3")) && _SMS.Course_ID.Contains(new Guid("1A4A791F-FE3A-4668-A18B-C71403FD258B")))
                {

                }
                else if (_SMS.Course_ID.Contains(new Guid("1A4A791F-FE3A-4668-A18B-C71403FD258B")))
                {
                    TSQL += $" AND ca.Course_ID NOT IN ('FC32E138-4EE2-4DA2-9453-5C8368180BC3')";
                }
                else
                {
                    TSQL += $" AND ca.Course_ID IN ({_SMS.Course_ID.ToIN()})";
                }
            }
            return new MSSQLFactory().GetSingleValues<string>(TSQL);
        }
    }
}
