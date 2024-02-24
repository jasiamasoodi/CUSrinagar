using CUSrinagar.DataManagers.SQLQueries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Terex;
using CUSrinagar.Models;
using System.Data.SqlClient;

namespace CUSrinagar.DataManagers
{
    public class EnrollmentDB
    {

        //public List<Enrollment> ListCount()
        //{
        //    return new MSSQLFactory().GetObjectList<Enrollment>(new SubjectSQLQueries().GetSkillSubjectsByCollege(Id));
        //}


        //public List<Enrollment> ListCount()
        //{
        //    // string query = "SELECT * FROM (SELECT Batch,CollegeFullName, COUNT(t.Student_ID)AS Count from (SELECT Student_ID,AcceptCollege_ID,CollegeFullName,Batch FROM ARGPersonalInformation_UG INNER JOIN ADMCollegeMaster ON ADMCollegeMaster.College_ID = ARGPersonalInformation_UG.AcceptCollege_ID WHERE ADMCollegeMaster.Status = 1 AND FormStatus NOT IN(3,5) UNION SELECT Student_ID,AcceptCollege_ID,CollegeFullName,Batch FROM ARGPersonalInformation_IH INNER JOIN ADMCollegeMaster ON ADMCollegeMaster.College_ID = ARGPersonalInformation_IH.AcceptCollege_ID WHERE ADMCollegeMaster.Status = 1 AND FormStatus NOT IN(3,5) UNION SELECT Student_ID,AcceptCollege_ID,CollegeFullName,Batch FROM ARGPersonalInformation_PG INNER JOIN ADMCollegeMaster ON ADMCollegeMaster.College_ID = ARGPersonalInformation_pg.AcceptCollege_ID  WHERE ADMCollegeMaster.Status = 1 AND FormStatus NOT IN(3,5))AS t GROUP BY t.CollegeFullName,t.AcceptCollege_ID,Batch) t PIVOT (MAX(Count) FOR CollegeFullName IN ( [Abdul Ahad Azad Memorial College],[Amar Singh College],[Govt.Women College],[Sri Pratap College],[Institute of Advanced Studies in Education])) AS P";
        //    //string query1 = $@"SELECT * FROM (SELECT Batch,CollegeCode, COUNT(t.Student_ID)AS Count from (
        //    //                SELECT Student_ID,AcceptCollege_ID,CollegeCode,CollegeFullName,Batch FROM ARGPersonalInformation_UG
        //    //                INNER JOIN ADMCollegeMaster ON ADMCollegeMaster.College_ID=ARGPersonalInformation_UG.AcceptCollege_ID WHERE ADMCollegeMaster.Status=1 AND FormStatus  IN ( {(int)Enums.FormStatus.Submitted} ,{(int)Enums.FormStatus.Selected} ,{(int)Enums.FormStatus.Accepted },{(int)Enums.FormStatus.CancelRegistration })
        //    //                UNION 
        //    //                SELECT Student_ID,AcceptCollege_ID,CollegeCode,CollegeFullName,Batch FROM ARGPersonalInformation_IH
        //    //                INNER JOIN ADMCollegeMaster ON ADMCollegeMaster.College_ID=ARGPersonalInformation_IH.AcceptCollege_ID WHERE ADMCollegeMaster.Status=1 AND FormStatus IN ( {(int)Enums.FormStatus.Submitted} ,{(int)Enums.FormStatus.Selected} ,{(int)Enums.FormStatus.Accepted },{(int)Enums.FormStatus.CancelRegistration })
        //    //                UNION  
        //    //                SELECT Student_ID,AcceptCollege_ID,CollegeCode,CollegeFullName,Batch FROM ARGPersonalInformation_PG
        //    //                INNER JOIN ADMCollegeMaster ON ADMCollegeMaster.College_ID=ARGPersonalInformation_pg.AcceptCollege_ID  WHERE ADMCollegeMaster.Status=1 AND FormStatus IN ( {(int)Enums.FormStatus.Submitted} ,{(int)Enums.FormStatus.Selected} ,{(int)Enums.FormStatus.Accepted },{(int)Enums.FormStatus.CancelRegistration })
        //    //                )AS t
        //    //                GROUP BY t.CollegeFullName,CollegeCode,t.AcceptCollege_ID,Batch) t
        //    //                PIVOT
        //    //                (    MAX(Count)
        //    //                     FOR CollegeCode IN
		      //    //                  (
		      //    //                  [AAA],
		      //    //                  [ASC],
		      //    //                  [GWC],
		      //    //                  [SPC],
		      //    //                  [IASE]
		      //    //                  )
        //    //                ) AS P order by Batch desc";
        //    string query = $@"SELECT * FROM(
        //                        SELECT SemesterBatch Batch,College.CollegeCode,COUNT(S.Student_ID)AS Count
        //                        FROM
        //                        (
	       //                         SELECT Student_ID, AcceptCollege_ID, FormStatus FROM ARGPersonalInformation_UG
	       //                         UNION ALL
	       //                         SELECT Student_ID, AcceptCollege_ID, FormStatus FROM ARGPersonalInformation_PG
	       //                         UNION ALL
	       //                         SELECT Student_ID, AcceptCollege_ID, FormStatus FROM ARGPersonalInformation_IH
        //                        )S
        //                        INNER JOIN
        //                        (
	       //                         SELECT Student_ID, Semester, SemesterBatch,Combination_ID FROM ARGSelectedCombination_UG Where IsVerified=1
	       //                         UNION ALL
	       //                         SELECT Student_ID, Semester,SemesterBatch, Combination_ID FROM ARGSelectedCombination_PG Where IsVerified=1
	       //                         UNION
	       //                         SELECT Student_ID, Semester,SemesterBatch, Combination_ID FROM ARGSelectedCombination_IH Where IsVerified=1
        //                        )Comb
        //                        ON S.Student_ID = Comb.Student_ID          AND Comb.Semester <= S.CurrentSemesterOrYear                                            
        //                        INNER JOIN ADMCombinationMaster CM ON Comb.Semester = CM.Semester AND CM.Combination_ID = Comb.Combination_ID 
        //                        INNER JOIN ADMCollegeMaster College ON College.College_ID = S.AcceptCollege_ID
        //                        INNER JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = CM.Course_ID
        //                        WHERE Comb.Semester=1
        //                        GROUP BY College.CollegeFullName,College.CollegeCode,S.AcceptCollege_ID,Comb.SemesterBatch)Temp
        //                        PIVOT
        //                        (    MAX(Count)
        //                                FOR CollegeCode IN
		      //                          (
			     //                           [AAA],
			     //                           [ASC],
			     //                           [GWC],
			     //                           [SPC],
			     //                           [IASE]
		      //                          )
        //                        ) AS P order by Batch DESC";
        //    SqlCommand cmd = new SqlCommand();
        //    cmd.CommandText = query;
        //    return new MSSQLFactory().GetObjectList<Enrollment>(cmd);
        //}
     
        //public List<Enrollment> GetEnrollment(int Year)
        //{
        //    var query = $@"SELECT * FROM (SELECT Batch,CollegeCode, COUNT(t.Student_ID)AS Count from (
        //                    SELECT Student_ID,AcceptCollege_ID,CollegeCode,CollegeFullName,Batch FROM ARGPersonalInformation_UG
        //                    INNER JOIN ADMCollegeMaster ON ADMCollegeMaster.College_ID=ARGPersonalInformation_UG.AcceptCollege_ID WHERE ADMCollegeMaster.Status=1 AND FormStatus  IN ( {(int)Enums.FormStatus.Submitted} ,{(int)Enums.FormStatus.Selected} ,{(int)Enums.FormStatus.Accepted },{(int)Enums.FormStatus.CancelRegistration })
        //                    UNION 
        //                    SELECT Student_ID,AcceptCollege_ID,CollegeCode,CollegeFullName,Batch FROM ARGPersonalInformation_IH
        //                    INNER JOIN ADMCollegeMaster ON ADMCollegeMaster.College_ID=ARGPersonalInformation_IH.AcceptCollege_ID WHERE ADMCollegeMaster.Status=1 AND FormStatus IN ( {(int)Enums.FormStatus.Submitted} ,{(int)Enums.FormStatus.Selected} ,{(int)Enums.FormStatus.Accepted },{(int)Enums.FormStatus.CancelRegistration })
        //                    UNION  
        //                    SELECT Student_ID,AcceptCollege_ID,CollegeCode,CollegeFullName,Batch FROM ARGPersonalInformation_PG
        //                    INNER JOIN ADMCollegeMaster ON ADMCollegeMaster.College_ID=ARGPersonalInformation_pg.AcceptCollege_ID  WHERE ADMCollegeMaster.Status=1 AND FormStatus IN ( {(int)Enums.FormStatus.Submitted} ,{(int)Enums.FormStatus.Selected} ,{(int)Enums.FormStatus.Accepted },{(int)Enums.FormStatus.CancelRegistration })
        //                    )AS t
        //                    GROUP BY t.CollegeFullName,CollegeCode,t.AcceptCollege_ID,Batch) t
        //                    PIVOT
        //                    (    MAX(Count)
        //                         FOR CollegeCode IN
		      //                      (
		      //                      [AAA],
		      //                      [ASC],
		      //                      [GWC],
		      //                      [SPC],
		      //                      [IASE]
		      //                      )
        //                    ) AS P Where Batch={Year} ";
        //    return new MSSQLFactory().GetObjectList<Enrollment>(query);
        //}

    }
}
