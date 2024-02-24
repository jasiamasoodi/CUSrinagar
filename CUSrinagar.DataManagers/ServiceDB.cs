using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CUSrinagar.Models;
using CUSrinagar.Enums;
using Terex;

namespace CUSrinagar.DataManagers
{
    public class ServiceDB
    {
        public int AutoCancelNotRespondingStudents()
        {
            string query = $@"UPDATE ARGPersonalInformation_UG SET FormStatus={(int)FormStatus.AutoAdmissionCancel},AcceptCollege_ID=NULL WHERE IsPassout=0 AND Student_ID 
                            IN( SELECT Exmfrm.Student_ID  FROM ARGPersonalInformation_UG Pinfo JOIN ARGSelectedCombination_UG  SC ON SC.Student_ID = Pinfo.Student_ID AND Pinfo.CurrentSemesterOrYear=sc.Semester JOIN ARGStudentExamForm_UG Exmfrm ON Exmfrm.Student_ID = Pinfo.Student_ID AND Exmfrm.Semester = SC.Semester JOIN ADMCombinationMaster ON ADMCombinationMaster.Combination_ID = SC.Combination_ID 
                            JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = ADMCombinationMaster.Course_ID WHERE SC.SemesterBatch<=YEAR(DATEADD (yy,-1,getdate ())) AND  Exmfrm.CreatedOn<dateadd (yy,-1,getdate ()) AND Pinfo.FormStatus={(int)FormStatus.Selected}  AND Pinfo.CurrentSemesterOrYear<>Duration
                            AND Exmfrm.Student_ID NOT in
                            (SELECT Exmfrm.Student_ID FROM ARGPersonalInformation_UG Pinfo JOIN ARGStudentExamForm_UG Exmfrm ON Exmfrm.Student_ID = Pinfo.Student_ID 
                            WHERE Batch<=YEAR(DATEADD (yy,-1,getdate ())) AND  Exmfrm.CreatedOn>=dateadd (yy,-1,getdate ()) AND Pinfo.FormStatus={(int)FormStatus.Selected}))

                            UPDATE ARGPersonalInformation_UG SET FormStatus={(int)FormStatus.AutoAdmissionCancel},AcceptCollege_ID=NULL WHERE IsPassout=0 AND Student_ID 
                                                        IN(                                   
                            SELECT Pinfo.Student_ID
                            FROM ARGPersonalInformation_UG Pinfo
                                JOIN ARGSelectedCombination_UG SC
                                    ON SC.Student_ID = Pinfo.Student_ID
                                        AND Pinfo.CurrentSemesterOrYear = SC.Semester
                                Left JOIN ARGStudentExamForm_UG Exmfrm
                                    ON Exmfrm.Student_ID = Pinfo.Student_ID
                                        AND Exmfrm.Semester = SC.Semester
                                JOIN ADMCombinationMaster
                                    ON ADMCombinationMaster.Combination_ID = SC.Combination_ID
                                JOIN ADMCourseMaster
                                    ON ADMCourseMaster.Course_ID = ADMCombinationMaster.Course_ID
                            WHERE  CAST(COALESCE(SC.UpdatedON,SC.CreatedON ) AS date)<= DATEADD(yy, -1, GETDATE()) AND 
                            Exmfrm.StudentExamForm_ID IS NULL
                            AND Pinfo.CurrentSemesterOrYear = SC.Semester  AND Pinfo.FormStatus = {(int)FormStatus.Selected}
                                AND Pinfo.Student_ID NOT IN (
                                                                    SELECT Exmfrm.Student_ID
                                                                    FROM ARGPersonalInformation_UG Pinfo
                                                                        JOIN ARGStudentExamForm_UG Exmfrm
                                                                            ON Exmfrm.Student_ID = Pinfo.Student_ID
                                                                    WHERE Batch <= YEAR(DATEADD(yy, -1, GETDATE()))
                                                                            AND Exmfrm.CreatedOn >= DATEADD(yy, -2, GETDATE())
                                                                            AND Pinfo.FormStatus = {(int)FormStatus.Selected}
                                                                )
								                                )





                            UPDATE ARGPersonalInformation_IH SET FormStatus={(int)FormStatus.AutoAdmissionCancel},AcceptCollege_ID=NULL WHERE IsPassout=0 AND Student_ID 
                            IN( 
                            SELECT Exmfrm.Student_ID  FROM ARGPersonalInformation_IH Pinfo JOIN ARGSelectedCombination_IH  SC ON SC.Student_ID = Pinfo.Student_ID AND Pinfo.CurrentSemesterOrYear =sc.Semester JOIN ADMCombinationMaster ON ADMCombinationMaster.Combination_ID = SC.Combination_ID 
                            JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = ADMCombinationMaster.Course_ID JOIN ARGStudentExamForm_IH Exmfrm ON Exmfrm.Student_ID = Pinfo.Student_ID AND Exmfrm.Semester= SC.Semester WHERE SC.SemesterBatch<=YEAR(DATEADD (yy,-1,getdate ())) AND  Exmfrm.CreatedOn<dateadd (yy,-1,getdate ()) AND Pinfo.FormStatus={(int)FormStatus.Selected}   AND Pinfo.CurrentSemesterOrYear<>Duration
                            AND Exmfrm.Student_ID NOT in
                            (SELECT Exmfrm.Student_ID FROM ARGPersonalInformation_IH Pinfo JOIN ARGStudentExamForm_IH Exmfrm ON Exmfrm.Student_ID = Pinfo.Student_ID 
                            WHERE Batch<=YEAR(DATEADD (yy,-1,getdate ())) AND  Exmfrm.CreatedOn>=dateadd (yy,-1,getdate ()) AND Pinfo.FormStatus={(int)FormStatus.Selected}) )

                          UPDATE ARGPersonalInformation_IH SET FormStatus={(int)FormStatus.AutoAdmissionCancel},AcceptCollege_ID=NULL WHERE IsPassout=0 AND Student_ID 
                                                        IN(                                   
                            SELECT Pinfo.Student_ID
                            FROM ARGPersonalInformation_IH Pinfo
                                JOIN ARGSelectedCombination_IH SC
                                    ON SC.Student_ID = Pinfo.Student_ID
                                        AND Pinfo.CurrentSemesterOrYear = SC.Semester
                                Left JOIN ARGStudentExamForm_IH Exmfrm
                                    ON Exmfrm.Student_ID = Pinfo.Student_ID
                                        AND Exmfrm.Semester = SC.Semester
                                JOIN ADMCombinationMaster
                                    ON ADMCombinationMaster.Combination_ID = SC.Combination_ID
                                JOIN ADMCourseMaster
                                    ON ADMCourseMaster.Course_ID = ADMCombinationMaster.Course_ID
                            WHERE  CAST(COALESCE(SC.UpdatedON,SC.CreatedON ) AS date)<= DATEADD(yy, -1, GETDATE()) AND 
                            Exmfrm.StudentExamForm_ID IS NULL
                            AND Pinfo.CurrentSemesterOrYear = SC.Semester  AND Pinfo.FormStatus = {(int)FormStatus.Selected}
                                AND Pinfo.Student_ID NOT IN (
                                                                    SELECT Exmfrm.Student_ID
                                                                    FROM ARGPersonalInformation_IH Pinfo
                                                                        JOIN ARGStudentExamForm_IH Exmfrm
                                                                            ON Exmfrm.Student_ID = Pinfo.Student_ID
                                                                    WHERE Batch <= YEAR(DATEADD(yy, -1, GETDATE()))
                                                                            AND Exmfrm.CreatedOn >= DATEADD(yy, -2, GETDATE())
                                                                            AND Pinfo.FormStatus = {(int)FormStatus.Selected}
                                                                )
								                                )

                            UPDATE ARGPersonalInformation_PG SET FormStatus={(int)FormStatus.AutoAdmissionCancel},AcceptCollege_ID=NULL WHERE IsPassout=0 AND Student_ID 
                            IN( 
                            SELECT Exmfrm.Student_ID FROM ARGPersonalInformation_PG Pinfo JOIN ARGSelectedCombination_PG  SC ON SC.Student_ID = Pinfo.Student_ID AND Pinfo.CurrentSemesterOrYear=sc.Semester JOIN ADMCombinationMaster ON ADMCombinationMaster.Combination_ID = SC.Combination_ID 
                            JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = ADMCombinationMaster.Course_ID JOIN ARGStudentExamForm_UG Exmfrm ON Exmfrm.Student_ID = Pinfo.Student_ID AND Exmfrm.Semester = SC.Semester WHERE SC.SemesterBatch<=YEAR(DATEADD (yy,-1,getdate ())) AND  Exmfrm.CreatedOn<dateadd (yy,-1,getdate ()) AND Pinfo.FormStatus={(int)FormStatus.Selected} AND Pinfo.CurrentSemesterOrYear<>Duration
                            AND Exmfrm.Student_ID NOT in
                            (SELECT Exmfrm.Student_ID FROM ARGPersonalInformation_PG Pinfo JOIN ARGStudentExamForm_PG Exmfrm ON Exmfrm.Student_ID = Pinfo.Student_ID 
                            WHERE Batch<=YEAR(DATEADD (yy,-1,getdate ())) AND  Exmfrm.CreatedOn>=dateadd (yy,-1,getdate ()) AND Pinfo.FormStatus={(int)FormStatus.Selected})
                            )
                            UPDATE ARGPersonalInformation_PG SET FormStatus={(int)FormStatus.AutoAdmissionCancel},AcceptCollege_ID=NULL WHERE IsPassout=0 AND Student_ID 
                                                        IN(                                   
                            SELECT Pinfo.Student_ID
                            FROM ARGPersonalInformation_PG Pinfo
                                JOIN ARGSelectedCombination_PG SC
                                    ON SC.Student_ID = Pinfo.Student_ID
                                        AND Pinfo.CurrentSemesterOrYear = SC.Semester
                                Left JOIN ARGStudentExamForm_PG Exmfrm
                                    ON Exmfrm.Student_ID = Pinfo.Student_ID
                                        AND Exmfrm.Semester = SC.Semester
                                JOIN ADMCombinationMaster
                                    ON ADMCombinationMaster.Combination_ID = SC.Combination_ID
                                JOIN ADMCourseMaster
                                    ON ADMCourseMaster.Course_ID = ADMCombinationMaster.Course_ID
                            WHERE  CAST(COALESCE(SC.UpdatedON,SC.CreatedON ) AS date)<= DATEADD(yy, -1, GETDATE()) AND 
                            Exmfrm.StudentExamForm_ID IS NULL
                            AND Pinfo.CurrentSemesterOrYear = SC.Semester  AND Pinfo.FormStatus = {(int)FormStatus.Selected}
                                AND Pinfo.Student_ID NOT IN (
                                                                    SELECT Exmfrm.Student_ID
                                                                    FROM ARGPersonalInformation_PG Pinfo
                                                                        JOIN ARGStudentExamForm_PG Exmfrm
                                                                            ON Exmfrm.Student_ID = Pinfo.Student_ID
                                                                    WHERE Batch <= YEAR(DATEADD(yy, -1, GETDATE()))
                                                                            AND Exmfrm.CreatedOn >= DATEADD(yy, -2, GETDATE())
                                                                            AND Pinfo.FormStatus ={(int)FormStatus.Selected}
                                                                )
								                                )

";

            SqlCommand cmd = new SqlCommand
            {
                CommandText = query
            };
            return new MSSQLFactory().ExecuteNonQuery(cmd);
        }
    }
}
