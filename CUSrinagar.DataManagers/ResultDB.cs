using CUSrinagar.DataManagers.SQLQueries;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using CUSrinagar.Models;
using GeneralModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Terex;

namespace CUSrinagar.DataManagers
{
    public class ResultDB
    {
        public List<ResultCompact> StudentInfo(PrintProgramme printProgramme, short? semester, Parameters parameter, bool IsEditable = false)
        {
            var Query = new ResultSQLQueries().StudentInfo(printProgramme, semester, IsEditable);
            var Helper = new GeneralFunctions().GetWhereClause<FilterModels>(parameter.Filters);
            Helper.Command.CommandText = Query.Replace("_WHERE_CLAUSE_", Helper.WhereClause) + new GeneralFunctions().GetPagedQuery(Query, parameter);

            var dt = new MSSQLFactory().GetDataTable(Helper.Command);
            List<ResultCompact> list = parseDataTableToResultCompactList(dt);

            if (list.IsNotNullOrEmpty())
            {
                if (parameter.PageInfo.PageSize >= 0 && !parameter.Filters.Any(x => x.Column == "Student_ID"))
                {
                    var studentListInClause = string.Join(",", list.Select(x => x.Student_ID));
                    parameter.Filters.Add(new SearchFilter() { Column = "Student_ID", TableAlias = "S", Operator = SQLOperator.In, Value = studentListInClause });
                }
                if (parameter.Filters.Any(x => x.Column == "ExamRollNumber"))
                {
                    //parameter.Filters.Remove(parameter.Filters.First(x => x.Column == "ExamRollNumber"));
                }
                if (parameter.Filters.Any(x => x.Column == "CombinationSubjects"))
                {
                    parameter.Filters.Add(new SearchFilter() { Column = "CM.Subject_ID", Value = parameter.Filters.First(x => x.Column == "CombinationSubjects").Value });
                    parameter.Filters.Remove(parameter.Filters.First(x => x.Column == "CombinationSubjects"));
                }
                var subjectResultList = SubjectResultList(printProgramme, semester, parameter, IsEditable);
                if (subjectResultList.IsNotNullOrEmpty())
                    list.ForEach(x => x.SubjectResults = subjectResultList.Where(y => y.Student_ID == x.Student_ID)?.ToList());
            }
            return list;
        }

        public List<ResultCompact> StudentInfoDetails(PrintProgramme printProgramme, short? semester, Parameters parameter, bool IsEditable = false)
        {
            var Query = new ResultSQLQueries().StudentInfoDetails(printProgramme, semester, IsEditable);
            var Helper = new GeneralFunctions().GetWhereClause<FilterModels>(parameter.Filters);
            Helper.Command.CommandText = Query.Replace("_WHERE_CLAUSE_", Helper.WhereClause) + new GeneralFunctions().GetPagedQuery(Query, parameter);

            var dt = new MSSQLFactory().GetDataTable(Helper.Command);
            List<ResultCompact> list = parseDataTableToResultCompactListEng(dt);

            if (list.IsNotNullOrEmpty())
            {
                if (parameter.PageInfo.PageSize >= 0 && !parameter.Filters.Any(x => x.Column == "Student_ID"))
                {
                    var studentListInClause = string.Join(",", list.Select(x => x.Student_ID));
                    parameter.Filters.Add(new SearchFilter() { Column = "Student_ID", TableAlias = "S", Operator = SQLOperator.In, Value = studentListInClause });
                }
                if (parameter.Filters.Any(x => x.Column == "ExamRollNumber"))
                {
                    //parameter.Filters.Remove(parameter.Filters.First(x => x.Column == "ExamRollNumber"));
                }
                if (parameter.Filters.Any(x => x.Column == "CombinationSubjects"))
                {
                    parameter.Filters.Add(new SearchFilter() { Column = "CM.Subject_ID", Value = parameter.Filters.First(x => x.Column == "CombinationSubjects").Value });
                    parameter.Filters.Remove(parameter.Filters.First(x => x.Column == "CombinationSubjects"));
                }

                parameter.Filters.Remove(parameter.Filters.FirstOrDefault(x => x.Column.ToLower().Trim() == "semester" && x.TableAlias.ToLower().Trim() == "comb") ?? new SearchFilter());

                var subjectResultList = SubjectResultList(printProgramme, semester, parameter, IsEditable);
                if (subjectResultList.IsNotNullOrEmpty())
                    list.ForEach(x => x.SubjectResults = subjectResultList.Where(y => y.Student_ID == x.Student_ID)?.ToList());
            }
            return list;
        }

        public List<ResultCompact> StudentInfoDetailsFullGazette(PrintProgramme printProgramme, short? semester, Parameters parameter, bool IsEditable = false)
        {
            var Query = new ResultSQLQueries().StudentInfoDetailsForEngFullGazette(printProgramme, semester, IsEditable);
            var Helper = new GeneralFunctions().GetWhereClause<FilterModels>(parameter.Filters);
            Helper.Command.CommandText = Query.Replace("_WHERE_CLAUSE_", Helper.WhereClause) + new GeneralFunctions().GetPagedQuery(Query, parameter);

            var dt = new MSSQLFactory().GetDataTable(Helper.Command);
            List<ResultCompact> list = parseDataTableToResultCompactListEng(dt);

            if (list.IsNotNullOrEmpty())
            {
                if (parameter.PageInfo.PageSize >= 0 && !parameter.Filters.Any(x => x.Column == "Student_ID"))
                {
                    var studentListInClause = string.Join(",", list.Select(x => x.Student_ID));
                    parameter.Filters.Add(new SearchFilter() { Column = "Student_ID", TableAlias = "S", Operator = SQLOperator.In, Value = studentListInClause });
                }
                if (parameter.Filters.Any(x => x.Column == "ExamRollNumber"))
                {
                    //parameter.Filters.Remove(parameter.Filters.First(x => x.Column == "ExamRollNumber"));
                }
                if (parameter.Filters.Any(x => x.Column == "CombinationSubjects"))
                {
                    parameter.Filters.Add(new SearchFilter() { Column = "CM.Subject_ID", Value = parameter.Filters.First(x => x.Column == "CombinationSubjects").Value });
                    parameter.Filters.Remove(parameter.Filters.First(x => x.Column == "CombinationSubjects"));
                }

                parameter.Filters.Remove(parameter.Filters.FirstOrDefault(x => x.Column.ToLower().Trim() == "semester" && x.TableAlias.ToLower().Trim() == "comb") ?? new SearchFilter());

                var subjectResultList = SubjectResultListDetailsFullGazette(printProgramme, semester, parameter, IsEditable);
                if (subjectResultList.IsNotNullOrEmpty())
                    list.ForEach(x => x.SubjectResults = subjectResultList.Where(y => y.Student_ID == x.Student_ID)?.ToList());
            }
            return list;
        }


        public List<ResultCompact> GetFinalData(Parameters parameter, PrintProgramme printProgramme, short? semester, bool IsEditable = false)
        {
            var Query = new ResultSQLQueries().GetFinalData(parameter, printProgramme, semester, IsEditable);

            var dt = new MSSQLFactory().GetDataTable(Query);
            List<ResultCompact> list = parseDataTableToResultCompactListEng(dt);

            if (list.IsNotNullOrEmpty())
            {
                if (parameter.PageInfo.PageSize >= 0 && !parameter.Filters.Any(x => x.Column == "Student_ID"))
                {
                    var studentListInClause = string.Join(",", list.Select(x => x.Student_ID));
                    parameter.Filters.Add(new SearchFilter() { Column = "Student_ID", TableAlias = "S", Operator = SQLOperator.In, Value = studentListInClause });
                }
                if (parameter.Filters.Any(x => x.Column == "ExamRollNumber"))
                {
                    //parameter.Filters.Remove(parameter.Filters.First(x => x.Column == "ExamRollNumber"));
                }
                if (parameter.Filters.Any(x => x.Column == "CombinationSubjects"))
                {
                    parameter.Filters.Add(new SearchFilter() { Column = "CM.Subject_ID", Value = parameter.Filters.First(x => x.Column == "CombinationSubjects").Value });
                    parameter.Filters.Remove(parameter.Filters.First(x => x.Column == "CombinationSubjects"));
                }

                parameter.Filters.Remove(parameter.Filters.FirstOrDefault(x => x.Column.ToLower().Trim() == "semester" && x.TableAlias.ToLower().Trim() == "comb") ?? new SearchFilter());

                //var val = "1";
                //parameter.Filters.Add(new SearchFilter() { Column = "IsInternalPassed", TableAlias = "R", Operator = SQLOperator.EqualTo, Value = val });
                //parameter.Filters.Add(new SearchFilter() { Column = "IsExternalPassed", TableAlias = "R", Operator = SQLOperator.EqualTo, Value = val });               


                var subjectResultList = AllSubjectDetails(printProgramme, semester, parameter, IsEditable);
                if (subjectResultList.IsNotNullOrEmpty())
                    list.ForEach(x => x.SubjectResults = subjectResultList.Where(y => y.Student_ID == x.Student_ID)?.ToList());
            }
            return list;
        }

        public int Printed(Guid student_ID)
        {
            var Query = $@"UPDATE dbo.MSStatus SET PrintedOn=COALESCE(PrintedOn,GETDATE()) ,PrintedBy=COALESCE(PrintedBy,'{AppUserHelper.User_ID}')
                            WHERE Student_ID='{student_ID}' AND (PrintedOn IS NULL)";
            return new MSSQLFactory().ExecuteNonQuery(Query);
        }

        public List<ResultCompact> StudentInfoDetailsForEngForInsertion(Parameters parameter, PrintProgramme printProgramme, short? semester, bool IsEditable = false)
        {
            var Query = new ResultSQLQueries().StudentInfoDetailsForEngForInsertion(parameter, printProgramme, semester, IsEditable);

            var dt = new MSSQLFactory().GetDataTable(Query);
            List<ResultCompact> list = parseDataTableToResultCompactListEng(dt);

            if (list.IsNotNullOrEmpty())
            {
                if (parameter.PageInfo.PageSize >= 0 && !parameter.Filters.Any(x => x.Column == "Student_ID"))
                {
                    var studentListInClause = string.Join(",", list.Select(x => x.Student_ID));
                    parameter.Filters.Add(new SearchFilter() { Column = "Student_ID", TableAlias = "S", Operator = SQLOperator.In, Value = studentListInClause });
                }
                if (parameter.Filters.Any(x => x.Column == "ExamRollNumber"))
                {
                    //parameter.Filters.Remove(parameter.Filters.First(x => x.Column == "ExamRollNumber"));
                }
                if (parameter.Filters.Any(x => x.Column == "CombinationSubjects"))
                {
                    parameter.Filters.Add(new SearchFilter() { Column = "CM.Subject_ID", Value = parameter.Filters.First(x => x.Column == "CombinationSubjects").Value });
                    parameter.Filters.Remove(parameter.Filters.First(x => x.Column == "CombinationSubjects"));
                }

                parameter.Filters.Remove(parameter.Filters.FirstOrDefault(x => x.Column.ToLower().Trim() == "semester" && x.TableAlias.ToLower().Trim() == "comb") ?? new SearchFilter());

                //var val = "1";
                //parameter.Filters.Add(new SearchFilter() { Column = "IsInternalPassed", TableAlias = "R", Operator = SQLOperator.EqualTo, Value = val });
                //parameter.Filters.Add(new SearchFilter() { Column = "IsExternalPassed", TableAlias = "R", Operator = SQLOperator.EqualTo, Value = val });               


                var subjectResultList = AllSubjectDetails(printProgramme, semester, parameter, IsEditable);
                if (subjectResultList.IsNotNullOrEmpty())
                    list.ForEach(x => x.SubjectResults = subjectResultList.Where(y => y.Student_ID == x.Student_ID)?.ToList());
            }
            return list;
        }




        public List<ResultCompact> FinalData(PrintProgramme printProgramme, Parameters parameter)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);



            var Query = $@"SELECT DISTINCT S.Student_ID,S.FullName,S.FathersName,S.CUSRegistrationNo,E.ExamRollNumber,C.CourseFullName,S.Batch,Col.CollegeFullName,col.Address CollegeAddress,TCourse_ID Course_ID,AcceptCollege_ID
                            ,c.DegreeCourseTitle,
                            (CASE WHEN CAST(MS.PrintedOn AS DATE) IS NULL THEN 0 ELSE 1 end) PrintedOn,
                            Col.Address,(SELECT MAX(SemesterBatch) FROM ARGSelectedCombination_UG WHERE Student_ID='{parameter.Filters.Where(x => x.Column == "Student_ID")?.FirstOrDefault()?.Value ?? Guid.Empty.ToString()}') SemesterBatch
                            FROM dbo.MSFullDetails FD 
                            JOIN ARGPersonalInformation_{printProgramme.ToString()} S ON S.Student_ID = FD.Student_ID 
                            JOIN  dbo.MSStatus MS ON MS.Student_ID = FD.Student_Id AND MS.Student_ID = S.Student_ID
                            JOIN (SELECT DISTINCT Student_ID,ExamRollNumber FROM ARGStudentExamForm_{printProgramme.ToString()} WHERE Semester=1 AND ExamRollNumber IS NOT NULL) E ON E.Student_ID=S.Student_ID
                            JOIN ADMCourseMaster C ON C.Course_ID =  MS.TCourse_ID
                            JOIN ADMCollegeMaster Col ON Col.College_ID = S.AcceptCollege_ID ";
            var Helper = new GeneralFunctions().GetWhereClause<FilterModels>(parameter.Filters);
            Helper.Command.CommandText = Query + Helper.WhereClause + new GeneralFunctions().GetPagedQuery(Query, parameter);
            var list = new MSSQLFactory().GetObjectList<ResultCompact>(Helper.Command);
            if (list != null && list.Count > 0)
            {
                var duplicates = list.GroupBy(x => x.Student_ID).ToList();
                if (duplicates != null && duplicates.Count > 0)
                    foreach (var item in duplicates)
                    {
                        Guid student_ID = item.Key;
                        var t_item = item.OrderBy(x => x.ExamRollNumber).Last();
                        list.RemoveAll(x => x.Student_ID == item.Key);
                        list.Add(t_item);
                    }
            }
            return list;
        }


        public List<SubjectResult> AllDataByStudentIDs(List<Guid> StudentIDs, PrintProgramme printProgramme)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            //string query = $@"SELECT * FROM dbo.MSFullDetails WHERE Student_Id IN ({StudentIDs.ToIN()})";
            string query = $@"
                           
                        SELECT 
                               P.Student_ID,
                               SemesterBatch,
                               R.semester,
                               SM.Subject_ID,
                               CMS.Programme,
                               MS.TotalCredit,       
                               SM.SubjectFullName,
                               SM.SubjectType,
                               SM.SubjectCode,
                               SM.HasResult,
                               SM.HasExaminationFee,
	                           MS.IsInternalMarksApplicable,
                               CAST(MS.InternalMaxMarks AS DECIMAL(5, 2)) InternalMaxMarks,
                               MS.InternalIsPartOf,
                               CAST(MS.InternalMinPassMarks AS DECIMAL(5, 2)) InternalMinPassMarks,
                               MS.IsInternalAttendance_AssessmentMarksApplicable,
                               CAST(MS.InternalAttendance_AssessmentMaxMarks AS DECIMAL(5, 2)) InternalAttendance_AssessmentMaxMarks,
                               CAST(MS.InternalAttendance_AssessmentMinPassMarks AS DECIMAL(5, 2)) InternalAttendance_AssessmentMinPassMarks,
                               MS.InternalAttendance_AssessmentIsPartOf,
                               MS.IsExternalAttendance_AssessmentMarksApplicable,
                               CAST(MS.ExternalAttendance_AssessmentMaxMarks AS DECIMAL(5, 2)) ExternalAttendance_AssessmentMaxMarks,
                               CAST(MS.ExternalAttendance_AssessmentMinPassMarks AS DECIMAL(5, 2)) ExternalAttendance_AssessmentMinPassMarks,
                               MS.ExternalAttendance_AssessmentIsPartOf,
                               MS.IsExternalMarksApplicable,
                               CAST(MS.ExternalMaxMarks AS DECIMAL(5, 2)) ExternalMaxMarks,
                               CAST(MS.ExternalMinPassMarks AS DECIMAL(5, 2)) ExternalMinPassMarks,
                               MS.ExternalIsPartOf,
	                           CAST(R.InternalMarks AS DECIMAL(5, 2)) InternalMarks,
                               CAST(R.InternalAttendance_AssessmentMarks AS DECIMAL(5, 2)) InternalAttendance_AssessmentMarks,
                               CAST(R.ExternalAttendance_AssessmentMarks AS DECIMAL(5, 2)) ExternalAttendance_AssessmentMarks,
                               CAST(R.ExternalMarks AS DECIMAL(5, 2)) ExternalMarks,
                               R.IsInternalPassed,
                               R.IsExternalPassed,
                               R.ExternalSubmitted,
                               R.InternalSubmitted,
                               R.ResultNotification_ID,
                               R.ExamForm_ID,
                               RN.ParentNotification_ID,
                               ST.NotificationNo,
                               RN.Dated NotificationDate,
                               MS.CreditWeightage,
                               SM.IsLocked,
                               FD.SubjectTitle

                        FROM dbo.ARGPersonalInformation_{printProgramme.ToString()} P
                        JOIN dbo.ARGSelectedCombination_{printProgramme.ToString()} SC ON SC.Student_ID = P.Student_ID AND SC.IsVerified=1 
                        JOIN dbo.ADMCombinationMaster CM ON CM.Combination_ID = SC.Combination_ID    
                        JOIN dbo.{printProgramme.ToString()}_SemesterAll R ON R.Student_ID = P.Student_ID AND R.Student_ID = SC.Student_ID 
                        JOIN dbo.ADMCourseMaster CMS ON CMS.Course_ID = CM.Course_ID 
                        JOIN dbo.ADMSubjectMaster SM ON SM.Semester = SC.semester AND SM.Subject_ID = R.Subject_ID AND SM.HasResult = 1
                        JOIN dbo.MSSubjectMarksStructure MS ON MS.SubjectMarksStructure_ID = SM.SubjectMarksStructure_ID
                        JOIN dbo.ResultNotification RN ON RN.ResultNotification_ID = R.ResultNotification_ID 
                        JOIN dbo.MSFullDetails FD ON FD.Student_Id = P.Student_ID AND FD.Student_Id = R.Student_ID  AND FD.Subject_ID = R.Subject_ID
                        JOIN dbo.MSStatus ST ON ST.Student_ID = FD.Student_Id AND ST.Student_ID = P.Student_ID
                        WHERE                        
	                           P.Student_Id IN ({StudentIDs.ToIN()})
                               /*IN ('08204886-35cd-4570-80b8-0f40a16b44cc','46fba5f0-9856-4019-9a54-2b4806a8b848')*/
                         ORDER BY RN.NotificationNo DESC /*R.Semester,SM.SubjectType,SM.SubjectFullName  */         

                        ";

            SqlCommand command = new SqlCommand();
            command.CommandText = query;
            return new MSSQLFactory().GetObjectList<SubjectResult>(query);
        }


        public List<MarksSheet> MarksSheetList(PrintProgramme printProgramme, Parameters parameter)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            var Query = $@"SELECT S.Student_ID,S.FullName,S.FathersName,S.CUSRegistrationNo,E.ExamRollNumber,C.CourseFullName,S.Batch,Col.CollegeFullName,col.Address CollegeAddress,TCourse_ID Course_ID,AcceptCollege_ID
                   ,(SELECT MAX(SemesterBatch) FROM ARGSelectedCombination_UG WHERE Student_ID='{parameter.Filters.Where(x => x.Column == "Student_ID")?.FirstOrDefault()?.Value ?? Guid.Empty.ToString()}') SemesterBatch
                            FROM MSCGPA 
                            JOIN ARGPersonalInformation_{printProgramme.ToString()} S ON S.Student_ID = MSCGPA.Student_ID AND MSCGPA.SGPAType=1
                            JOIN (SELECT DISTINCT Student_ID,ExamRollNumber FROM ARGStudentExamForm_{printProgramme.ToString()} WHERE Semester=1 AND ExamRollNumber IS NOT NULL) E ON E.Student_ID=S.Student_ID
                            JOIN ADMCourseMaster C ON C.Course_ID = TCourse_ID
                            JOIN ADMCollegeMaster Col ON Col.College_ID = S.AcceptCollege_ID ";
            var Helper = new GeneralFunctions().GetWhereClause<FilterModels>(parameter.Filters);
            Helper.Command.CommandText = Query + Helper.WhereClause;// + new GeneralFunctions().GetPagedQuery(Query, parameter);
            var list = new MSSQLFactory().GetObjectList<MarksSheet>(Helper.Command);
            if (list != null && list.Count > 0)
            {
                var duplicates = list.GroupBy(x => x.Student_ID).ToList();
                if (duplicates != null && duplicates.Count > 0)
                    foreach (var item in duplicates)
                    {
                        Guid student_ID = item.Key;
                        var t_item = item.OrderBy(x => x.ExamRollNumber).Last();
                        list.RemoveAll(x => x.Student_ID == item.Key);
                        list.Add(t_item);
                    }
            }
            return list;
        }


        public List<MarksSheet> MarksSheetStatus(PrintProgramme printProgramme, Parameters parameter)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            //var Query = $@"SELECT S.Student_ID,S.FullName,S.FathersName,S.CUSRegistrationNo,E.ExamRollNumber,C.CourseFullName,S.Batch,Col.CollegeFullName,col.Address CollegeAddress,TCourse_ID Course_ID,AcceptCollege_ID
            //       ,(SELECT MAX(SemesterBatch) FROM ARGSelectedCombination_{printProgramme.ToString()} WHERE Student_ID='{parameter.Filters.Where(x => x.Column == "Student_ID")?.FirstOrDefault()?.Value ?? Guid.Empty.ToString()}') SemesterBatch
            //                FROM MSStatus ST 
            //                JOIN ARGPersonalInformation_{printProgramme.ToString()} S ON S.Student_ID = ST.Student_ID  
            //                JOIN (SELECT DISTINCT Student_ID,ExamRollNumber FROM ARGStudentExamForm_{printProgramme.ToString()} WHERE Semester=1 AND ExamRollNumber IS NOT NULL) E ON E.Student_ID=S.Student_ID
            //                JOIN ADMCourseMaster C ON C.Course_ID = TCourse_ID
            //                JOIN ADMCollegeMaster Col ON Col.College_ID = S.AcceptCollege_ID ";

            var Query = $@"SELECT ST.Student_ID,P.CUSRegistrationNo,E.ExamRollNumber,ST.MaxMarks,ST.DateofDeclaration,ST.CreatedOn,ST.ValidatedOn
							,ST.PrintedOn,ST.HandedOverOn,C.Course_ID,P.Batch,P.AcceptCollege_ID
							 FROM dbo.MSStatus ST JOIN dbo.ARGPersonalInformation_{printProgramme.ToString()} P
							ON P.Student_ID = ST.Student_ID	
						   JOIN dbo.ARGStudentExamForm_{printProgramme.ToString()} E ON E.Student_ID = P.Student_ID AND E.Student_ID = ST.Student_ID	 AND E.Semester=8
						   JOIN dbo.ARGSelectedCombination_{printProgramme.ToString()} SC ON SC.Student_ID = ST.Student_ID AND  SC.IsVerified=1
						   JOIN dbo.ADMCombinationMaster CM ON CM.Combination_ID = SC.Combination_ID AND CM.Semester=8
						   JOIN dbo.ADMCourseMaster C ON C.Course_ID = CM.Course_ID ";


            var Helper = new GeneralFunctions().GetWhereClause<FilterModels>(parameter.Filters);
            Helper.Command.CommandText = Query + Helper.WhereClause;// + new GeneralFunctions().GetPagedQuery(Query, parameter);
            var list = new MSSQLFactory().GetObjectList<MarksSheet>(Helper.Command);
            if (list != null && list.Count > 0)
            {
                var duplicates = list.GroupBy(x => x.Student_ID).ToList();
                if (duplicates != null && duplicates.Count > 0)
                    foreach (var item in duplicates)
                    {
                        Guid student_ID = item.Key;
                        var t_item = item.OrderBy(x => x.ExamRollNumber).Last();
                        list.RemoveAll(x => x.Student_ID == item.Key);
                        list.Add(t_item);
                    }
            }
            return list;
        }



        public List<SubjectResult> GetSubject(Guid Student_ID)
        {
            var Query = $"Select * from MSFullDetails WHERE Student_ID='{Student_ID}' ORDER By Semester,SubjectType,SubjectTitle";
            return new MSSQLFactory().GetObjectList<SubjectResult>(Query);
        }



        public List<ResultCompact> StudentInfoDetailsForEng(PrintProgramme printProgramme, short? semester, Parameters parameter, bool IsEditable = false)
        {
            var Query = new ResultSQLQueries().StudentInfoDetailsForEng(printProgramme, semester, IsEditable);
            var Helper = new GeneralFunctions().GetWhereClause<FilterModels>(parameter.Filters);
            Helper.Command.CommandText = Query.Replace("_WHERE_CLAUSE_", Helper.WhereClause) + new GeneralFunctions().GetPagedQuery(Query, parameter);

            var dt = new MSSQLFactory().GetDataTable(Helper.Command);
            List<ResultCompact> list = parseDataTableToResultCompactListEng(dt);

            if (list.IsNotNullOrEmpty())
            {
                if (parameter.PageInfo.PageSize >= 0 && !parameter.Filters.Any(x => x.Column == "Student_ID"))
                {
                    var studentListInClause = string.Join(",", list.Select(x => x.Student_ID));
                    parameter.Filters.Add(new SearchFilter() { Column = "Student_ID", TableAlias = "S", Operator = SQLOperator.In, Value = studentListInClause });
                }
                if (parameter.Filters.Any(x => x.Column == "ExamRollNumber"))
                {
                    //parameter.Filters.Remove(parameter.Filters.First(x => x.Column == "ExamRollNumber"));
                }
                if (parameter.Filters.Any(x => x.Column == "CombinationSubjects"))
                {
                    parameter.Filters.Add(new SearchFilter() { Column = "CM.Subject_ID", Value = parameter.Filters.First(x => x.Column == "CombinationSubjects").Value });
                    parameter.Filters.Remove(parameter.Filters.First(x => x.Column == "CombinationSubjects"));
                }

                parameter.Filters.Remove(parameter.Filters.FirstOrDefault(x => x.Column.ToLower().Trim() == "semester" && x.TableAlias.ToLower().Trim() == "comb") ?? new SearchFilter());

                //var val = "1";
                //parameter.Filters.Add(new SearchFilter() { Column = "IsInternalPassed", TableAlias = "R", Operator = SQLOperator.EqualTo, Value = val });
                //parameter.Filters.Add(new SearchFilter() { Column = "IsExternalPassed", TableAlias = "R", Operator = SQLOperator.EqualTo, Value = val });

                var subjectResultList = SubjectResultListDetails(printProgramme, semester, parameter, IsEditable);
                if (subjectResultList.IsNotNullOrEmpty())
                    list.ForEach(x => x.SubjectResults = subjectResultList.Where(y => y.Student_ID == x.Student_ID)?.ToList());
            }
            return list;
        }

        public List<ResultList> ResultList_SubjectResult(PrintProgramme printProgramme, Parameters Parameter, short? Semester, bool Editable = false, bool IsPassOut = false)
        {
            try
            {

                string SubQuery = string.Empty;
                if (IsPassOut == true)
                {
                    SubQuery = "AND S.IsPassout<>1";
                }

                string Query = new ResultSQLQueries().ResultList_SubjectResult_Query(true, printProgramme, Semester, Editable, IsPassOut);
                var Helper = new GeneralFunctions().GetWhereClause<FilterModels>(Parameter.Filters);
                Helper.Command.CommandText = Query + Helper.WhereClause + SubQuery + new GeneralFunctions().GetPagedQuery(Query, Parameter);
                var dt = new MSSQLFactory().GetDataTable(Helper.Command);
                List<ResultList> list = parseDataTableToResultList(dt);
                return list;
            }
            catch (SqlException ex) when (ex.Number == 208)  /*Invalid object name*/
            {
                return null;
            }
        }
        public List<ResultAnomalies> GetALLResultAnomalies(Parameters Parameter)
        {
            try
            {
                string Query = "SELECT * FROM ResultAnomalies ";
                var Helper = new GeneralFunctions().GetWhereClause<ResultAnomalies>(Parameter.Filters);
                Helper.Command.CommandText = Query + Helper.WhereClause + new GeneralFunctions().GetPagedQuery(Query, Parameter);
                return new MSSQLFactory().GetObjectList<ResultAnomalies>(Helper.Command);

            }
            catch (SqlException ex) when (ex.Number == 208)  /*Invalid object name*/
            {
                return null;
            }
        }

        public List<ResultAnomalies> ResultAnomalyExistsAlready(Guid subject_Id, Guid student_ID, Guid examForm_ID)
        {
            string Query = $"SELECT * FROM ResultAnomalies Where subject_Id='{subject_Id}' AND  student_ID='{student_ID}' AND  examForm_ID='{examForm_ID}' ";
            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.CommandText = Query;
            return new MSSQLFactory().GetObjectList<ResultAnomalies>(sqlCommand);


        }

        public List<ResultList> RevaluationList(PrintProgramme printProgramme, Parameters Parameter, short? Semester, bool Editable = false)
        {
            string Query = new ResultSQLQueries().RevaluationList_Query(printProgramme, Parameter, Semester, Editable);
            var Helper = new GeneralFunctions().GetWhereClause<FilterModels>(Parameter.Filters);
            Helper.Command.CommandText = Query + Helper.WhereClause + new GeneralFunctions().GetPagedQuery(Query, Parameter);
            return new MSSQLFactory().GetObjectList<ResultList>(Helper.Command);

        }

        public List<SubjectResult> SubjectResult(PrintProgramme printProgramme, Guid Student_ID, short SemesterBatch, short? semester = null, bool Editable = false, bool transcriptInfo = false, Guid? Subject_ID = null)
        {
            string Query = new ResultSQLQueries().ResultList_SubjectResult_Query(false, printProgramme, semester, Editable, transcriptInfo);
            Query += $@" WHERE Comb.Student_ID='{Student_ID}' {(semester.HasValue ? $" AND Comb.Semester={semester}" : "")} AND  Comb.SemesterBatch<={SemesterBatch} {(Subject_ID.HasValue ? $" AND Comb.Subject_ID='{Subject_ID}'" : "")} ORDER BY Comb.Semester,dbo.FNSubjectTypeDescription(SubjectType),SubjectFullName";
            return new MSSQLFactory().GetObjectList<SubjectResult>(Query);
        }

        //public List<ResultList> ResultList_SubjectResultTable(PrintProgramme printProgramme, Parameters Parameter, short? Semester, bool Editable = false)
        //{
        //    string Query = new ResultSQLQueries().ResultListQuery(true, printProgramme, Semester, Editable, true);
        //    var Helper = new GeneralFunctions().GetWhereClause<FilterModels>(Parameter.Filters);
        //    Helper.Command.CommandText = Query + Helper.WhereClause + new GeneralFunctions().GetPagedQuery(Query, Parameter);
        //    return new MSSQLFactory().GetDataTableAsList<ResultList>(Helper.Command);
        //}

        public List<ResultList> ResultHistory_SubjectResult(PrintProgramme printProgramme, Parameters Parameter, short? Semester, bool Editable = false)
        {
            string Query = new ResultSQLQueries().ResultHistoryQuery(true, printProgramme, Semester, Editable);
            var Helper = new GeneralFunctions().GetWhereClause<FilterModels>(Parameter.Filters);
            Helper.Command.CommandText = Query + Helper.WhereClause + new GeneralFunctions().GetPagedQuery(Query, Parameter);
            return new MSSQLFactory().GetObjectList<ResultList>(Helper.Command);
        }

        #region Result MeritList
        public List<ResultList> ResultMeritList(PrintProgramme printProgramme, Parameters Parameter, bool Editable = false)
        {

            if (printProgramme == PrintProgramme.IH)
            {
                Parameter.Filters.Remove(Parameter.Filters.First(x => x.Column == "Programme"));
            }

            string Query = new ResultSQLQueries().ResulMeritListQuery(printProgramme, Parameter, Editable);
            var Helper = new GeneralFunctions().GetWhereClause<FilterModels>(Parameter.Filters);
            Helper.Command.CommandText = Query + Helper.WhereClause + new GeneralFunctions().GetPagedQuery(Query, Parameter);
            return new MSSQLFactory().GetObjectList<ResultList>(Helper.Command);
        }

        #endregion

        #region Result PendingList Regular,Backlog Both

        public DataTable ResultPendingList(Parameters parameter, PrintProgramme printProgramme, short Semester, int batch, Programme programe, Guid CourseId)
        {
            string Query = new ResultSQLQueries().ResulPendingListQuery(parameter, printProgramme, Semester, batch, programe, CourseId);
            return new MSSQLFactory().GetDataTable(Query);
        }
        #endregion

        #region Result DataList  
        public DataTable ResultDataList(Parameters parameter, PrintProgramme printProgramme, short Semester, int batch, Programme programe, Guid CourseId, Guid AcceptCollege_ID, bool value)
        {
            string Query = new ResultSQLQueries().ResulDataListQuery(parameter, printProgramme, Semester, batch, programe, CourseId, AcceptCollege_ID, value);
            return new MSSQLFactory().GetDataTable(Query);
        }
        #endregion

        #region NewResultSection
        public Guid? FindStudent(ResultNotification resultNotification, string ExamRollNumberOrRegistrationNo)
        {
            var printProgramme = new GeneralFunctions().MappingTable(resultNotification.PrintProgramme);
            var Query = $@"SELECT S.Student_ID FROM ARGPersonalInformation_{printProgramme.ToString()} S
                    JOIN ARGSelectedCombination_{printProgramme.ToString()} C ON C.Student_ID = S.Student_ID
                    JOIN ARGStudentExamForm_{printProgramme.ToString()} E ON E.Student_ID = S.Student_ID AND E.Semester = C.Semester
                    WHERE E.ExamRollNumber IS NOT NULL AND C.Semester={resultNotification.Semester} AND C.SemesterBatch={resultNotification.Batch} AND ";
            SqlCommand sqlCommant = new SqlCommand();
            if (ExamRollNumberOrRegistrationNo.Length > 10)
            {
                Query += " CUSRegistrationNo=@CUSRegistrationNo";
                sqlCommant.Parameters.AddWithValue("@CUSRegistrationNo", ExamRollNumberOrRegistrationNo);
            }
            else
            {
                Query += " ExamRollNumber=@ExamRollNumber";
                sqlCommant.Parameters.AddWithValue("@ExamRollNumber", ExamRollNumberOrRegistrationNo);
            }
            sqlCommant.CommandText = Query;
            return new MSSQLFactory().GetObject<ResultCompact>(sqlCommant)?.Student_ID;
        }

        public ResultNotification GetResultNotification(string resultNotification)
        {
            string query = "SELECT * FROM ResultNotification WHERE NotificationNo = @ResultNotification";
            SqlCommand command = new SqlCommand();
            command.Parameters.AddWithValue("@ResultNotification", resultNotification.Trim());
            command.CommandText = query;
            return new MSSQLFactory().GetObject<ResultNotification>(command);
        }



        public List<ResultCompact> ChkDataMSFullDetails(List<Guid> StudentIdS)
        {

            string query = $@"SELECT * FROM dbo.MSFullDetails WHERE Student_Id IN ({StudentIdS.ToIN()})";
            SqlCommand command = new SqlCommand();
            command.CommandText = query;
            return new MSSQLFactory().GetObjectList<ResultCompact>(query);
        }

        public List<ResultCompact> ChkDatastudIDList()
        {

            string query = $@"SELECT DISTINCT Student_Id FROM dbo.MSFullDetails";
            SqlCommand command = new SqlCommand();
            command.CommandText = query;
            return new MSSQLFactory().GetObjectList<ResultCompact>(query);
        }

        public List<ResultCompact> CheckBacklogBatches(PrintProgramme printProgramme, short? semester, Parameters parameters, int? batch, string programe)
        {
            string query = $@"SELECT  SC.SemesterBatch,CMs.CourseFullName,Exm.ExamRollNumber,COUNT(P.Student_ID) AS NoofStudents  
								FROM dbo.ARGPersonalInformation_{printProgramme.ToString()} P JOIN dbo.ARGSelectedCombination_{printProgramme.ToString()} SC 
								ON SC.Student_ID = P.Student_ID
								JOIN dbo.ADMCombinationMaster CM ON CM.Combination_ID = SC.Combination_ID
								JOIN dbo.ADMCourseMaster CMs ON CMs.Course_ID = CM.Course_ID
								JOIN dbo.{printProgramme}_Semester{semester} PG ON PG.Student_ID = P.Student_ID  AND PG.Student_ID = SC.Student_ID
								JOIN dbo.ARGStudentExamForm_{printProgramme.ToString()} Exm ON Exm.Student_ID = SC.Student_ID AND Exm.Semester = CM.Semester AND Exm.IsRegular=0 AND Exm.Status = 4
								JOIN dbo.ARGStudentReExamForm RE ON RE.StudentExamForm_ID = Exm.StudentExamForm_ID AND RE.Subject_ID = PG.Subject_ID AND Exm.Year='{batch}'								
								AND SC.IsVerified=1 AND Exm.ExamRollNumber IS NOT NULL --AND PG.ResultNotification_ID IS NULL
								WHERE  CM.Semester={semester}	AND RE.FeeStatus=10		/*SC.SemesterBatch<2021 AND*/			
								GROUP BY SC.SemesterBatch,CMs.CourseFullName,Exm.ExamRollNumber";
            SqlCommand command = new SqlCommand();
            command.CommandText = query;
            return new MSSQLFactory().GetObjectList<ResultCompact>(query);
        }

        public List<ResultAnomalies> CheckResultCancelled(Guid iD, Guid? examForm_ID)
        {
            string Query = $"SELECT * FROM ResultAnomalies Where Result_Id='{iD}' AND  examForm_ID='{examForm_ID}' ";
            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.CommandText = Query;
            return new MSSQLFactory().GetObjectList<ResultAnomalies>(sqlCommand);


        }


        //public ResultNotification GetResultNotification(Guid ResultNotification_Id)
        //{
        //    string query = "SELECT * FROM ResultNotification WHERE ResultNotification_Id = @ResultNotification_Id";
        //    SqlCommand command = new SqlCommand();
        //    command.Parameters.AddWithValue("@ResultNotification_Id", ResultNotification_Id);
        //    command.CommandText = query;
        //    return new MSSQLFactory().GetObject<ResultNotification>(command);
        //}
        //public ResultCompact StudentInfoBacklogonly(PrintProgramme printProgramme, short semester, Guid Student_ID)
        //{
        //    var Query = new ResultSQLQueries().StudentInfoBacklogonly(printProgramme, semester) + $" Where STDINFO.Student_ID='{Student_ID}'";
        //    return new MSSQLFactory().GetObject<ResultCompact>(Query);
        //}

        //public int Delete(Guid resultNotification_ID)
        //{
        //    string query = $@"DELETE FROM ResultNotification WHERE ResultNotification_ID = '{resultNotification_ID}'";
        //    return new MSSQLFactory().ExecuteNonQuery(query);
        //}

        // public MarksSheet StudentInfoOnMarksSheet(PrintProgramme printProgramme, short semester, Guid Student_ID)
        // {
        //     printProgramme = new GeneralFunctions().MappingTable(printProgramme);
        //     var Query = $@"SELECT OriginalIssued,ProvisionalIssued,DateOfIssue,DateOfDeclaration,SerialNumber,STDINFO.Student_ID,STDINFO.FullName,STDINFO.CUSRegistrationNo,STDINFO.FathersName,EXMFRM.ExamRollNumber,ADMCombinationMaster.Course_ID,CourseFullName,STDINFO.Batch
        //                     FROM ARGPersonalInformation_{printProgramme.ToString()} STDINFO
        //                     JOIN ARGSelectedCombination_{printProgramme.ToString()} SelectedComb ON SelectedComb.Student_ID = STDINFO.Student_ID AND SelectedComb.Semester={semester}
        //                     JOIN ADMCombinationMaster ON ADMCombinationMaster.Combination_ID = SelectedComb.Combination_ID AND ADMCombinationMaster.Semester = SelectedComb.Semester
        //JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = ADMCombinationMaster.Course_ID
        //                     LEFT JOIN MarksSheetSerials ON MarksSheetSerials.Student_ID = STDINFO.Student_ID
        //                     JOIN (Select Distinct ExamRollNumber,Student_ID FROM ARGStudentExamForm_{printProgramme.ToString()} WHERE ExamRollNumber Is NOT NULL)EXMFRM ON EXMFRM.Student_ID=STDINFO.Student_ID
        //                     Where STDINFO.Student_ID='{Student_ID}'";
        //     return new MSSQLFactory().GetObject<MarksSheet>(Query);
        // }



        // public List<ResultCompact> StudentInfoTest(PrintProgramme printProgramme, short semester, Parameters parameter)
        // {
        //     var Query = $@"SELECT STDINFO.Student_ID FROM ARGPersonalInformation_{printProgramme.ToString()} STDINFO
        //                     JOIN ARGSelectedCombination_{printProgramme.ToString()} SelectedComb ON SelectedComb.Student_ID = STDINFO.Student_ID AND SelectedComb.Semester={semester}
        //                     JOIN ADMCombinationMaster ON ADMCombinationMaster.Combination_ID = SelectedComb.Combination_ID AND ADMCombinationMaster.Semester = SelectedComb.Semester
        //JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = ADMCombinationMaster.Course_ID
        //                     JOIN (SELECT DISTINCT ExamRollNumber,Student_ID FROM ARGStudentExamForm_UG WHERE ExamRollNumber IS NOT NULL) E ON E.Student_ID=STDINFO.Student_ID";
        //     var Helper = new GeneralFunctions().GetWhereClause<ResultCompact>(parameter.Filters);
        //     Helper.Command.CommandText = Query + Helper.WhereClause + new GeneralFunctions().GetPagedQuery(Query, parameter);
        //     return new MSSQLFactory().GetObjectList<ResultCompact>(Helper.Command);
        // }

        //public List<SubjectResult> SubjectResultListTempAverage(PrintProgramme printProgramme, Guid Student_ID)
        //{
        //    printProgramme = new GeneralFunctions().MappingTable(printProgramme);
        //    string Query = $@"PR_{printProgramme}_AllSemResult";
        //    SqlCommand sqlCommand = new SqlCommand(Query);
        //    sqlCommand.CommandType = CommandType.StoredProcedure;
        //    sqlCommand.Parameters.AddWithValue("@Student_ID", Student_ID);
        //    return new MSSQLFactory().GetObjectList<SubjectResult>(sqlCommand);
        //}
        //public List<SubjectResult> SubjectResultList(PrintProgramme printProgramme, short semester, Guid Student_ID, bool Editable = false)
        //{
        //    printProgramme = new GeneralFunctions().MappingTable(printProgramme);
        //    string Query = $@"SELECT DISTINCT {semester} AS Semester,Title,NotificationDate,VWSCMaster.Subject_ID,HasResult,HasExaminationFee,SubjectType,VWSCMaster.Programme,SubjectFullName,NotificationNo,ParentNotification_ID,_ID,ResultNotification_ID,ExamForm_ID,FormNumber
        //                ,IsExternalMarksApplicable,ExternalMaxMarks,IsExternalAttendance_AssessmentApplicable,ExternalAttendance_AssessmentMaxMarks,ExternalMinPassMarks
        //                ,IsExternalPassComponent,ExternalAttendance_AssessmentMinPassMarks,IsExternalAttendance_AssessmentPassComponent,[ExternalIsPartOf],[ExternalAttendanceIsPartOf],[InternalIsPartOf],[InternalAttendanceIsPartOf],ExternalAttendance_AssessmentMinPassMarks
        //          ,ExternalMarks,ExternalAttendance_AssessmentMarks,ExternalSubmitted
        //          ,IsInternalMarksApplicable,InternalMaxMarks,IsInternalAttendance_AssessmentApplicable,InternalAttendance_AssessmentMaxMarks,InternalMinPassMarks
        //                ,IsInternalPassComponent,InternalAttendance_AssessmentMinPassMarks,IsInternalAttendance_AssessmentPassComponent
        //          ,InternalMarks,InternalAttendance_AssessmentMarks,InternalSubmitted 
        //                FROM ARGSelectedCombination_{printProgramme.ToString()} COMB
        //                JOIN VWSCMaster ON VWSCMaster.Combination_ID = COMB.Combination_ID AND VWSCMaster.Semester = COMB.Semester
        //                LEFT JOIN(
        //                    SELECT _ID,R.Student_ID,Subject_ID,ExamForm_ID,EXMForm.FormNumber
        //                    ,ExternalMarks,ExternalAttendance_AssessmentMarks,ExternalSubmitted
        //                    ,InternalMarks,InternalAttendance_AssessmentMarks,InternalSubmitted
        //  ,Title, Dated NotificationDate,NotificationNo,ParentNotification_ID,R.ResultNotification_ID
        //                    FROM {printProgramme.ToString()}_Semester{semester} R
        //  {(Editable ? "LEFT " : "")} JOIN ResultNotification ON ResultNotification.ResultNotification_ID = R.ResultNotification_ID AND ResultNotification.IsActive=1
        //                    {(Editable ? "LEFT " : "")}  JOIN ARGStudentExamForm_{printProgramme.ToString()} EXMForm ON EXMForm.StudentExamForm_ID = R.ExamForm_ID
        //  WHERE R.Student_ID='{Student_ID}' 
        //                )Result ON Result.Student_ID = COMB.Student_ID AND Result.Subject_ID = VWSCMaster.Subject_ID
        //                WHERE COMB.Student_ID='{Student_ID}' AND COMB.Semester={semester}
        //                UNION ALL
        //                SELECT {semester} AS Semester,Title,NotificationDate,ADMSubjectMaster.Subject_ID,HasResult,HasExaminationFee,SubjectType,ADMSubjectMaster.Programme,SubjectFullName,NotificationNo,ParentNotification_ID,_ID,ResultNotification_ID,ExamForm_ID,FormNumber
        //                ,IsExternalMarksApplicable,ExternalMaxMarks,IsExternalAttendance_AssessmentApplicable,ExternalAttendance_AssessmentMaxMarks,ExternalMinPassMarks
        //                ,IsExternalPassComponent,ExternalAttendance_AssessmentMinPassMarks,IsExternalAttendance_AssessmentPassComponent,[ExternalIsPartOf],[ExternalAttendanceIsPartOf],[InternalIsPartOf],[InternalAttendanceIsPartOf],ExternalAttendance_AssessmentMinPassMarks
        //          ,ExternalMarks,ExternalAttendance_AssessmentMarks,ExternalSubmitted
        //          ,IsInternalMarksApplicable,InternalMaxMarks,IsInternalAttendance_AssessmentApplicable,InternalAttendance_AssessmentMaxMarks,InternalMinPassMarks
        //                ,IsInternalPassComponent,InternalAttendance_AssessmentMinPassMarks,IsInternalAttendance_AssessmentPassComponent
        //          ,InternalMarks,InternalAttendance_AssessmentMarks,InternalSubmitted 
        //                FROM ARGStudentAdditionalSubjects_{printProgramme.ToString()} STDAdditionalSubject
        //                JOIN ADMSubjectMaster ON  ADMSubjectMaster.Subject_ID = STDAdditionalSubject.Subject_ID AND ADMSubjectMaster.Semester={semester}
        //                {(Editable ? "LEFT " : "")} JOIN(
        //                    SELECT R.Student_ID,R.ResultNotification_ID,Subject_ID,_ID,ExamForm_ID,EXMForm.FormNumber
        //                    ,ExternalMarks,ExternalAttendance_AssessmentMarks,ExternalSubmitted
        //                    ,InternalMarks,InternalAttendance_AssessmentMarks,InternalSubmitted
        //  ,Title, Dated NotificationDate,NotificationNo,ParentNotification_ID
        //                    FROM {printProgramme.ToString()}_Semester{semester} R
        //  {(Editable ? "LEFT " : "")} JOIN ResultNotification ON ResultNotification.ResultNotification_ID = R.ResultNotification_ID AND ResultNotification.IsActive=1
        //                    {(Editable ? "LEFT " : "")}  JOIN ARGStudentExamForm_{printProgramme.ToString()} EXMForm ON EXMForm.StudentExamForm_ID = R.ExamForm_ID
        //  WHERE R.Student_ID='{Student_ID}'
        //                )Result ON Result.Student_ID = STDAdditionalSubject.Student_ID AND Result.Subject_ID = STDAdditionalSubject.Subject_ID 
        //                WHERE STDAdditionalSubject.Student_ID='{Student_ID}' AND STDAdditionalSubject.Semester={semester}";
        //    return new MSSQLFactory().GetObjectList<SubjectResult>(Query);
        //}

        /// <summary>
        /// action used in public view of result
        /// </summary>
        /// <param name="IsEditable">In case need all rows irrespective of result like backlog</param>
        /// <returns></returns>
        //public List<SubjectResult> SubjectResultCompactList_Public(PrintProgramme printProgramme, short semester, Guid Student_ID, bool IsEditable)
        //{
        //    printProgramme = new GeneralFunctions().MappingTable(printProgramme);
        //    var Query = $@"SELECT DISTINCT Title,NotificationDate,VWSCMaster.Subject_ID,HasResult,HasExaminationFee,SubjectType,VWSCMaster.Programme,SubjectFullName,NotificationNo,ParentNotification_ID,_ID,ResultNotification_ID,ExamForm_ID,SubjectCode
        //                ,IsExternalMarksApplicable,ExternalMaxMarks,IsExternalAttendance_AssessmentMarksApplicable,ExternalAttendance_AssessmentMaxMarks,ExternalMinPassMarks
        //                ,IsExternalPassComponent,ExternalAttendance_AssessmentMinPassMarks,IsExternalAttendance_AssessmentPassComponent,[ExternalIsPartOf],[ExternalAttendance_AssessmentIsPartOf],[InternalIsPartOf],[InternalAttendance_AssessmentIsPartOf],ExternalAttendance_AssessmentMinPassMarks
        //          ,ExternalMarks,ExternalAttendance_AssessmentMarks,ExternalSubmitted
        //          ,IsInternalMarksApplicable,InternalMaxMarks,IsInternalAttendance_AssessmentMarksApplicable,InternalAttendance_AssessmentMaxMarks,InternalMinPassMarks
        //                ,IsInternalPassComponent,InternalAttendance_AssessmentMinPassMarks,IsInternalAttendance_AssessmentPassComponent
        //          ,InternalMarks,InternalAttendance_AssessmentMarks,InternalSubmitted 
        //                FROM ARGSelectedCombination_{printProgramme.ToString()} COMB
        //                {(IsEditable ? " LEFT " : "")} JOIN ARGStudentExamForm_{printProgramme.ToString()} EXMFORM ON EXMFORM.Student_ID = COMB.Student_ID AND EXMFORM.Semester = COMB.Semester
        //                JOIN VWSCMaster ON VWSCMaster.Combination_ID = COMB.Combination_ID AND VWSCMaster.Semester = COMB.Semester
        //                LEFT JOIN(
        //                    SELECT _ID,Student_ID,Subject_ID,ExamForm_ID
        //                    ,ExternalMarks,ExternalAttendance_AssessmentMarks,ExternalSubmitted
        //                    ,InternalMarks,InternalAttendance_AssessmentMarks,InternalSubmitted
        //  ,Title, Dated NotificationDate,NotificationNo,ParentNotification_ID,R.ResultNotification_ID
        //                    FROM {printProgramme.ToString()}_Semester{semester} R
        //  {(IsEditable ? " LEFT " : "")} JOIN ResultNotification ON ResultNotification.ResultNotification_ID = R.ResultNotification_ID
        //  WHERE Student_ID='{Student_ID}'
        //                )Result ON Result.Student_ID = COMB.Student_ID AND Result.Subject_ID = VWSCMaster.Subject_ID
        //                WHERE COMB.Student_ID='{Student_ID}' AND COMB.Semester={semester}
        //                UNION ALL
        //                SELECT Title,NotificationDate,S.Subject_ID,HasResult,HasExaminationFee,SubjectType,S.Programme,SubjectFullName,NotificationNo,ParentNotification_ID,_ID,ResultNotification_ID,ExamForm_ID,SubjectCode
        //                ,IsExternalMarksApplicable,ExternalMaxMarks,IsExternalAttendance_AssessmentMarksApplicable,ExternalAttendance_AssessmentMaxMarks,ExternalMinPassMarks
        //                ,IsExternalPassComponent,ExternalAttendance_AssessmentMinPassMarks,IsExternalAttendance_AssessmentPassComponent,[ExternalIsPartOf],[ExternalAttendance_AssessmentIsPartOf],[InternalIsPartOf],InternalAttendance_AssessmentIsPartOf,ExternalAttendance_AssessmentMinPassMarks
        //          ,ExternalMarks,ExternalAttendance_AssessmentMarks,ExternalSubmitted
        //          ,IsInternalMarksApplicable,InternalMaxMarks,IsInternalAttendance_AssessmentMarksApplicable,InternalAttendance_AssessmentMaxMarks,InternalMinPassMarks
        //                ,IsInternalPassComponent,InternalAttendance_AssessmentMinPassMarks,IsInternalAttendance_AssessmentPassComponent
        //          ,InternalMarks,InternalAttendance_AssessmentMarks,InternalSubmitted 
        //                FROM ARGStudentAdditionalSubjects_{printProgramme.ToString()} STDAdditionalSubject
        //                JOIN VW_SubjectWithStructure S ON  S.Subject_ID = STDAdditionalSubject.Subject_ID AND S.Semester={semester}
        //                LEFT JOIN(
        //                    SELECT Student_ID,R.ResultNotification_ID,Subject_ID,_ID,ExamForm_ID
        //                    ,ExternalMarks,ExternalAttendance_AssessmentMarks,ExternalSubmitted
        //                    ,InternalMarks,InternalAttendance_AssessmentMarks,InternalSubmitted
        //  ,Title, Dated NotificationDate,NotificationNo,ParentNotification_ID
        //                    FROM {printProgramme.ToString()}_Semester{semester} R
        //   {(IsEditable ? " LEFT " : "")} JOIN ResultNotification ON ResultNotification.ResultNotification_ID = R.ResultNotification_ID
        //  WHERE Student_ID='{Student_ID}' AND ResultNotification.ResultNotification_ID IS NOT NULL
        //                )Result ON Result.Student_ID = STDAdditionalSubject.Student_ID AND Result.Subject_ID = STDAdditionalSubject.Subject_ID 
        //                WHERE STDAdditionalSubject.Student_ID='{Student_ID}' AND STDAdditionalSubject.Semester={semester}";
        //    return new MSSQLFactory().GetObjectList<SubjectResult>(Query);
        //}

        #region Upload result
        public List<Guid> ISSubjectAssigned(ExamResult examResult)
        {
            PrintProgramme printProgramme = GeneralFunctions.ProgrammeToPrintProgrammeMapping(examResult.Programme);
            string TSQL = "";
            TSQL = $@"Select  E.Student_id
                        FROM ARGStudentExamForm_{printProgramme.ToString()} E
                        JOIN ARGSelectedCombination_{printProgramme.ToString()} SC ON SC.Student_ID = E.Student_ID  AND SC.Semester={examResult.Semester} AND E.Semester={examResult.Semester}
                        JOIN ADMCombinationMaster C ON SC.Combination_id = C.Combination_id
                        WHERE ExamRollNumber='{examResult.ExamRollNo}' AND CombinationSubjects Like '%{examResult.Subject_ID}%'  AND E.Status={(int)FormStatus.Accepted}";

            SqlCommand command = new SqlCommand
            {
                CommandText = TSQL,
                CommandTimeout = 5000
            };
            return new MSSQLFactory().GetSingleValues<Guid>(command);
        }
        public bool HasSubjectAssigned(ExamResult examResult)
        {
            return (ISSubjectAssigned(examResult) ?? new List<Guid>()).Count() > 0;
        }


        public List<Guid> CheckRecordExist(ExamResult examResult)
        {

            PrintProgramme printProgramme = GeneralFunctions.ProgrammeToPrintProgrammeMapping(examResult.Programme);
            string TSQL = "";
            TSQL = $@"Select  R.Student_id
                        FROM {printProgramme.ToString()}_Semester{examResult.Semester} R
                        JOIN ARGStudentExamForm_{printProgramme.ToString()} E ON R.Student_ID = E.Student_ID 
                        WHERE ExamRollNumber='{examResult.ExamRollNo}' AND Subject_id='{examResult.Subject_ID}'";

            SqlCommand command = new SqlCommand
            {
                CommandText = TSQL,
                CommandTimeout = 5000
            };
            return new MSSQLFactory().GetSingleValues<Guid>(command);
        }
        public bool CheckRecordExists(ExamResult examResult)
        {
            return (CheckRecordExist(examResult) ?? new List<Guid>()).Count() > 0;
        }
        public string GetInsertExamResultTSQL(ExamResult examResult)
        {
            PrintProgramme printProgramme = GeneralFunctions.ProgrammeToPrintProgrammeMapping(examResult.Programme);
            string TSQL = "";
            TSQL = $@"INSERT INTO {printProgramme.ToString()}_Semester{examResult.Semester}
                        (
                            _ID,
                            Student_ID,
                            Subject_ID,
                            ExternalMarks,
                            ExternalSubmitted,
                            ExternalUpdatedOn,
                            ExternalUpdatedBy
                        )
                        VALUES
                        (   NEWID(),      
                            (SELECT Student_id FROM  ARGStudentExamForm_{printProgramme.ToString()} E WHERE E.ExamRollNumber='{examResult.ExamRollNo}' AND E.Status={(int)FormStatus.Accepted} AND E.Semester={examResult.Semester} ),      
                            '{examResult.Subject_ID}',      
                            {examResult.ExternalMarks},      
                            0,        
                            GETDATE(), 
                            NULL
                         )";

            return TSQL;
        }
        public string GetUploadExamResultTSQL(ExamResult examResult)
        {
            PrintProgramme printProgramme = GeneralFunctions.ProgrammeToPrintProgrammeMapping(examResult.Programme);
            //Tuple<string, List<Guid[]>> tuple = GetCourseGrouping();
            //string Course_IDs = $"{examResult.Course_ID}";

            string TSQL = "";
            TSQL = $@"UPDATE R SET R.ExternalMarks={examResult.ExternalMarks}
                        FROM {printProgramme.ToString()}_Semester{examResult.Semester} R
                        JOIN ARGStudentExamForm_{printProgramme.ToString()} E ON R.Student_ID = E.Student_ID
                        WHERE Semester={examResult.Semester} AND ExamRollNumber='{examResult.ExamRollNo}' AND Subject_id='{examResult.Subject_ID}' and ResultNotification_id IS NULL";

            return TSQL;
        }

        public int UploadExamResult(string TSQLScript)
        {
            SqlCommand command = new SqlCommand
            {
                CommandText = TSQLScript,
                CommandTimeout = 5000
            };
            return new MSSQLFactory().ExecuteNonQuery(command);
        }

        #endregion
        //public List<ResultList> CompactList_Admin(Parameters Parameter, PrintProgramme printProgramme, short semester)
        //{
        //    printProgramme = new GeneralFunctions().MappingTable(printProgramme);
        //    var Helper = new GeneralFunctions().GetWhereClause<ResultCompact>(Parameter.Filters);
        //    var Query = $@"SELECT * FROM ( SELECT DISTINCT STDINFO.Student_ID,STDINFO.CUSRegistrationNo,STDINFO.FullName,STDINFO.FathersName,EXMFORM.ExamRollNumber,STDINFO.AcceptCollege_ID,CollegeFullName,ADMCourseMaster.Course_ID,ADMCourseMaster.CourseFullName,COMB.Semester,STDINFO.Batch
        //                ,VWSCMaster.Subject_ID,HasResult,HasExaminationFee,SubjectType,VWSCMaster.Programme,SubjectFullName,[ExternalIsPartOf],[ExternalAttendanceIsPartOf],[InternalIsPartOf],[InternalAttendanceIsPartOf]
        //                ,IsExternalMarksApplicable,ExternalMaxMarks,IsExternalAttendance_AssessmentApplicable,ExternalAttendance_AssessmentMaxMarks,ExternalMinPassMarks,IsExternalPassComponent
        //                ,ExternalAttendance_AssessmentMinPassMarks,IsExternalAttendance_AssessmentPassComponent
        //                ,ExternalMarks,ExternalAttendance_AssessmentMarks,ExternalSubmitted
        //                ,IsInternalMarksApplicable,InternalMaxMarks,IsInternalAttendance_AssessmentApplicable,InternalAttendance_AssessmentMaxMarks,InternalMinPassMarks
        //                ,IsInternalPassComponent,InternalAttendance_AssessmentMinPassMarks,IsInternalAttendance_AssessmentPassComponent
        //                ,InternalMarks,InternalAttendance_AssessmentMarks,InternalSubmitted
        //                ,Result._ID,Result.ResultNotification_ID,Result.ExamForm_ID,Result.Dated NotificationDate,NotificationNo,Title,ParentNotification_ID
        //                FROM
        //                ARGPersonalInformation_{printProgramme.ToString()} STDINFO 
        //                JOIN ADMCollegeMaster ON ADMCollegeMaster.College_ID=STDINFO.AcceptCollege_ID
        //                JOIN ARGSelectedCombination_{printProgramme.ToString()} COMB ON COMB.Student_ID = STDINFO.Student_ID AND COMB.Semester={semester.ToString()}
        //                JOIN ARGStudentExamForm_{printProgramme.ToString()} EXMFORM ON EXMFORM.Student_ID = STDINFO.Student_ID AND EXMFORM.Semester={semester.ToString()}
        //                JOIN VWSCMaster ON VWSCMaster.Combination_ID = COMB.Combination_ID AND VWSCMaster.Semester={semester.ToString()}
        //                JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID=VWSCMaster.Course_ID
        //                JOIN(
        //                    SELECT _ID,Student_ID,Subject_ID,ExamForm_ID,ExternalMarks,ExternalAttendance_AssessmentMarks,ExternalSubmitted,InternalMarks,InternalAttendance_AssessmentMarks,InternalSubmitted
        //  ,Title, Dated,NotificationNo,ParentNotification_ID,R.ResultNotification_ID
        //                    FROM {printProgramme.ToString()}_Semester{semester} R
        //  JOIN ResultNotification ON ResultNotification.ResultNotification_ID = R.ResultNotification_ID
        //                )Result ON Result.Student_ID = COMB.Student_ID AND Result.Subject_ID = VWSCMaster.Subject_ID
        //                UNION ALL 
        //                SELECT DISTINCT STDINFO.Student_ID,STDINFO.CUSRegistrationNo,STDINFO.FullName,STDINFO.FathersName,EXMFORM.ExamRollNumber,STDINFO.AcceptCollege_ID,CollegeFullName,ADMCourseMaster.Course_ID,ADMCourseMaster.CourseFullName
        //                ,ADMSubjectMaster.Semester,STDINFO.Batch
        //                ,ADDSubject.Subject_ID,HasResult,HasExaminationFee,SubjectType,ADMSubjectMaster.Programme,SubjectFullName,[ExternalIsPartOf],[ExternalAttendanceIsPartOf],[InternalIsPartOf],[InternalAttendanceIsPartOf]
        //                ,IsExternalMarksApplicable,ExternalMaxMarks,IsExternalAttendance_AssessmentApplicable,ExternalAttendance_AssessmentMaxMarks,ExternalMinPassMarks,IsExternalPassComponent
        //                ,ExternalAttendance_AssessmentMinPassMarks,IsExternalAttendance_AssessmentPassComponent
        //                ,ExternalMarks,ExternalAttendance_AssessmentMarks,ExternalSubmitted
        //                ,IsInternalMarksApplicable,InternalMaxMarks,IsInternalAttendance_AssessmentApplicable,InternalAttendance_AssessmentMaxMarks,InternalMinPassMarks
        //                ,IsInternalPassComponent,InternalAttendance_AssessmentMinPassMarks,IsInternalAttendance_AssessmentPassComponent
        //                ,InternalMarks,InternalAttendance_AssessmentMarks,InternalSubmitted
        //                ,Result._ID,Result.ResultNotification_ID,Result.ExamForm_ID,Result.Dated NotificationDate, NotificationNo, Title, ParentNotification_ID
        //                FROM
        //                ARGPersonalInformation_{printProgramme.ToString()} STDINFO
        //                JOIN ADMCollegeMaster ON ADMCollegeMaster.College_ID = STDINFO.AcceptCollege_ID
        //                JOIN ARGStudentAdditionalSubjects_{printProgramme.ToString()} ADDSubject ON STDINFO.Student_ID = ADDSubject.Student_ID AND ADDSubject.Semester = {semester.ToString()}
        //                JOIN ARGStudentExamForm_{printProgramme.ToString()} EXMFORM ON EXMFORM.Student_ID = STDINFO.Student_ID AND EXMFORM.Semester = {semester.ToString()}
        //                JOIN ADMSubjectMaster ON ADMSubjectMaster.Subject_ID = ADDSubject.Subject_ID AND ADMSubjectMaster.Semester = {semester.ToString()}
        //                LEFT JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = ADMSubjectMaster.Course_ID
        //                JOIN(
        //                    SELECT _ID,Student_ID,Subject_ID,ExamForm_ID,ExternalMarks,ExternalAttendance_AssessmentMarks,ExternalSubmitted,InternalMarks,InternalAttendance_AssessmentMarks,InternalSubmitted
        //  ,Title, Dated,NotificationNo,ParentNotification_ID,R.ResultNotification_ID
        //                    FROM {printProgramme.ToString()}_Semester{semester} R
        //  JOIN ResultNotification ON ResultNotification.ResultNotification_ID = R.ResultNotification_ID
        //                )Result ON Result.Student_ID = ADDSubject.Student_ID AND Result.Subject_ID = ADMSubjectMaster.Subject_ID) Result";
        //    Helper.Command.CommandText = Query + Helper.WhereClause + new GeneralFunctions().GetPagedQuery(Query, Parameter);
        //    return new MSSQLFactory().GetObjectList<ResultList>(Helper.Command);
        //}


        /// <summary>
        /// fetch result whose notification is already available
        /// </summary>
        public List<ResultList> CompactList(Parameters Parameter, PrintProgramme printProgramme, short semester)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            var Helper = new GeneralFunctions().GetWhereClause<ResultCompact>(Parameter.Filters);
            var Query = $@"SELECT * FROM ( SELECT STDINFO.Student_ID,STDINFO.CUSRegistrationNo,STDINFO.FullName,STDINFO.FathersName,ExamRollNumber,STDINFO.AcceptCollege_ID,CollegeFullName,ADMCourseMaster.Course_ID,ADMCourseMaster.CourseFullName,COMB.Semester,STDINFO.Batch
                        ,VWSCMaster.Subject_ID,HasResult,HasExaminationFee,SubjectType,VWSCMaster.Programme,SubjectCode,SubjectFullName,[ExternalIsPartOf],[ExternalAttendance_AssessmentIsPartOf],[InternalIsPartOf],[InternalAttendance_AssessmentIsPartOf]
                        ,IsExternalMarksApplicable,ExternalMaxMarks,IsExternalAttendance_AssessmentMarksApplicable,ExternalAttendance_AssessmentMaxMarks,ExternalMinPassMarks,IsExternalPassComponent
                        ,ExternalAttendance_AssessmentMinPassMarks,IsExternalAttendance_AssessmentPassComponent
                        ,ExternalMarks,ExternalAttendance_AssessmentMarks,ExternalSubmitted
                        ,IsInternalMarksApplicable,InternalMaxMarks,IsInternalAttendance_AssessmentMarksApplicable,InternalAttendance_AssessmentMaxMarks,InternalMinPassMarks
                        ,IsInternalPassComponent,InternalAttendance_AssessmentMinPassMarks,IsInternalAttendance_AssessmentPassComponent
                        ,InternalMarks,InternalAttendance_AssessmentMarks,InternalSubmitted
                        ,Result._ID,Result.ResultNotification_ID,Result.ExamForm_ID,Result.Dated NotificationDate,NotificationNo,Title,ParentNotification_ID
                        FROM
                        ARGPersonalInformation_{printProgramme.ToString()} STDINFO 
                        LEFT JOIN ADMCollegeMaster ON ADMCollegeMaster.College_ID=STDINFO.AcceptCollege_ID
                        JOIN ARGSelectedCombination_{printProgramme.ToString()} COMB ON COMB.Student_ID = STDINFO.Student_ID AND COMB.Semester={semester.ToString()}
                        --JOIN ARGStudentExamForm_{printProgramme.ToString()} EXMFORM ON EXMFORM.Student_ID = STDINFO.Student_ID AND EXMFORM.Semester={semester.ToString()}
                        JOIN VWSCMaster ON VWSCMaster.Combination_ID = COMB.Combination_ID AND VWSCMaster.Semester={semester.ToString()}
                        JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID=VWSCMaster.Course_ID
                        LEFT JOIN(
                            SELECT _ID,R.Student_ID,EXMFORM.ExamRollNumber,Subject_ID,ExamForm_ID,ExternalMarks,ExternalAttendance_AssessmentMarks,ExternalSubmitted,InternalMarks,InternalAttendance_AssessmentMarks,InternalSubmitted
						    ,Title, Dated,NotificationNo,ParentNotification_ID,R.ResultNotification_ID
                            FROM {printProgramme.ToString()}_Semester{semester} R
                            JOIN ARGStudentExamForm_UG EXMFORM ON EXMFORM.StudentExamForm_ID = R.ExamForm_ID
						    JOIN ResultNotification ON ResultNotification.ResultNotification_ID = R.ResultNotification_ID AND ResultNotification.IsActive=1
						    --WHERE ExternalSubmitted=1
                        )Result ON Result.Student_ID = COMB.Student_ID AND Result.Subject_ID = VWSCMaster.Subject_ID
                        UNION ALL 
                        SELECT STDINFO.Student_ID,STDINFO.CUSRegistrationNo,STDINFO.FullName,STDINFO.FathersName,ExamRollNumber,STDINFO.AcceptCollege_ID,CollegeFullName,ADMCourseMaster.Course_ID,ADMCourseMaster.CourseFullName
                        ,S.Semester,STDINFO.Batch
                        ,ADDSubject.Subject_ID,HasResult,HasExaminationFee,SubjectType,S.Programme,SubjectCode,SubjectFullName,[ExternalIsPartOf],[ExternalAttendance_AssessmentIsPartOf],[InternalIsPartOf],[InternalAttendance_AssessmentIsPartOf]
                        ,IsExternalMarksApplicable,ExternalMaxMarks,IsExternalAttendance_AssessmentMarksApplicable,ExternalAttendance_AssessmentMaxMarks,ExternalMinPassMarks,IsExternalPassComponent
                        ,ExternalAttendance_AssessmentMinPassMarks,IsExternalAttendance_AssessmentPassComponent
                        ,ExternalMarks,ExternalAttendance_AssessmentMarks,ExternalSubmitted
                        ,IsInternalMarksApplicable,InternalMaxMarks,IsInternalAttendance_AssessmentMarksApplicable,InternalAttendance_AssessmentMaxMarks,InternalMinPassMarks
                        ,IsInternalPassComponent,InternalAttendance_AssessmentMinPassMarks,IsInternalAttendance_AssessmentPassComponent
                        ,InternalMarks,InternalAttendance_AssessmentMarks,InternalSubmitted
                        ,Result._ID,Result.ResultNotification_ID,Result.ExamForm_ID,Result.Dated NotificationDate, NotificationNo, Title, ParentNotification_ID
                        FROM
                        ARGPersonalInformation_{printProgramme.ToString()} STDINFO
                        LEFT JOIN ADMCollegeMaster ON ADMCollegeMaster.College_ID = STDINFO.AcceptCollege_ID
                        JOIN ARGStudentAdditionalSubjects_{printProgramme.ToString()} ADDSubject ON STDINFO.Student_ID = ADDSubject.Student_ID AND ADDSubject.Semester = {semester.ToString()}
                        --JOIN ARGStudentExamForm_{printProgramme.ToString()} EXMFORM ON EXMFORM.Student_ID = STDINFO.Student_ID AND EXMFORM.Semester = {semester.ToString()}
                        JOIN VW_SubjectWithStructure S ON S.Subject_ID = ADDSubject.Subject_ID AND S.Semester = {semester.ToString()}
                        LEFT JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = S.Course_ID
                        LEFT JOIN(
                            SELECT _ID,R.Student_ID,EXMFORM.ExamRollNumber,Subject_ID,ExamForm_ID,ExternalMarks,ExternalAttendance_AssessmentMarks,ExternalSubmitted,InternalMarks,InternalAttendance_AssessmentMarks,InternalSubmitted
						    ,Title, Dated,NotificationNo,ParentNotification_ID,R.ResultNotification_ID
                            FROM {printProgramme.ToString()}_Semester{semester} R
JOIN ARGStudentExamForm_UG EXMFORM ON EXMFORM.StudentExamForm_ID = R.ExamForm_ID
						    JOIN ResultNotification ON ResultNotification.ResultNotification_ID = R.ResultNotification_ID  AND ResultNotification.IsActive=1
						    --WHERE ExternalSubmitted=1
                        )Result ON Result.Student_ID = ADDSubject.Student_ID AND Result.Subject_ID = S.Subject_ID) Result";
            Helper.Command.CommandText = Query + Helper.WhereClause + new GeneralFunctions().GetPagedQuery(Query, Parameter);
            return new MSSQLFactory().GetObjectList<ResultList>(Helper.Command);
        }

        public int SaveAnomaly(ResultAnomalies model)
        {
            int I = new MSSQLFactory().InsertRecord<ResultAnomalies>(model, $"ResultAnomalies");
            return I;
        }
        public int RemoveResut(ResultAnomalies model)
        {

            string Query = $@"Update {model.PrintProgramme.GetTablePFix()}_Semester{model.Semester.ToString()} Set ExternalMarks=0 , ExternalUpdatedOn='{DateTime.Now}',ExternalUpdatedBy='{AppUserHelper.User_ID}' Where _ID='{model.Result_Id}'";
            return new MSSQLFactory().ExecuteNonQuery(Query);
        }


        public int Save(ResultErrorLog model)
        {
            return new MSSQLFactory().InsertRecord<ResultErrorLog>(model, $"ResultErrorLog");
        }

        public bool ResultCreated(PrintProgramme printProgramme, short Semester, Guid Student_ID)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            var Query = $@"SELECT  COUNT(1) FROM {printProgramme.ToString()}_Semester{Semester} WHERE Student_ID='{Student_ID}' AND ResultNotification_ID IS NOT NULL AND ExamForm_ID IS NOT NULL";
            return new MSSQLFactory().ExecuteScalar<int>(Query) > 0;
        }

        public int UpdateImportStatus(Programme programme, short Semester, Guid _ID, DateTime? UpdatedOn = null)
        {
            PrintProgramme printProgramme = GeneralFunctions.ProgrammeToPrintProgrammeMapping(programme);
            string Query = $@"Update {printProgramme.ToString()}_Semester{Semester.ToString()} Set UpdatedOn='{(UpdatedOn.HasValue ? UpdatedOn.Value : DateTime.Now)}',UpdatedBy='{AppUserHelper.User_ID}' Where _ID='{_ID}'";
            return new MSSQLFactory().ExecuteNonQuery(Query);
        }

        #endregion

        #region MasterResult



        public MasterResultRow GetMasterResult(Programme programme, Guid student_ID, short semester, Guid subject_ID)
        {
            PrintProgramme printProgramme = GeneralFunctions.ProgrammeToPrintProgrammeMapping(programme);
            string query = $@"SELECT *,{(short)programme} AS Programme FROM ResultMaster_{printProgramme.ToString()} WHERE Student_ID='{student_ID}' AND Semester={semester} AND Subject_ID='{subject_ID}' ";
            return new MSSQLFactory().GetObject<MasterResultRow>(query);
        }

        public int Save(MasterResultRow model, Programme programme)
        {
            PrintProgramme printProgramme = GeneralFunctions.ProgrammeToPrintProgrammeMapping(programme);
            List<string> ignoreList = new List<string>() { "UpdatedOn", "UpdatedBy" };
            return new MSSQLFactory().InsertRecord(model, $"ResultMaster_{printProgramme.ToString()}", ignoreList);
        }

        public int Update(MasterResultRow model, Programme programme)
        {
            PrintProgramme printProgramme = GeneralFunctions.ProgrammeToPrintProgrammeMapping(programme);
            List<string> ignoreList = new List<string>() { "_ID" };
            var sqlCommand = new MSSQLFactory().UpdateRecord(model, ignoreList, ignoreList, $"ResultMaster_{printProgramme.ToString()}");
            sqlCommand.CommandText = sqlCommand.CommandText + " WHERE _ID=@_ID";
            sqlCommand.Parameters.AddWithValue("@_ID", model._ID);
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }
        #endregion



        public int EditResult(SemesterModel semester, string semesterName, Programme programme, List<string> ignoreQuery, List<string> ignoreParameter)
        {
            string tableName = new GeneralFunctions().GetSemesterTableName(semesterName, programme);
            SqlCommand sqlCommand = new MSSQLFactory().UpdateRecord(semester, ignoreQuery, ignoreParameter, tableName);
            sqlCommand.CommandText += " WHERE _ID=@_ID";
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }

        public SemesterModel GetSemesterByRegistrationNoAndSubject(Guid subject_ID, string registrationNo, Guid StudentId, string semesterName, Programme programme)
        {
            return new MSSQLFactory().GetObject<SemesterModel>(new StudentSQLQueries().GetSemesterByRegistrationNoAndSubject(subject_ID, registrationNo, StudentId, semesterName, programme, "Subject_ID"));

        }

        public List<SemesterModel> GetSemesterResultByStudentID(Guid Student_ID, string tableName, string joinColumn, bool CheckResultDeleared, PrintProgramme programme)
        {
            string postfix = new GeneralFunctions().GetPrintProgrammeFix(programme);
            string query = $@"SELECT Semester.*,ADMSubjectMaster.SubjectFullName,ADMSubjectMaster.SubjectType,Year,ExamRollNumber
                              FROM [{tableName}] Semester
                              JOIN ADMSubjectMaster ON ADMSubjectMaster.Subject_ID = Semester.{joinColumn}
                                       LEFT JOIN ARGStudentExamForm_{postfix} EF ON EF.StudentExamForm_ID = Semester.ExamForm_ID
                              WHERE Semester.Student_ID=@Student_ID " + (CheckResultDeleared ? "AND ResultNotification_ID IS NOT NULL" : "");


            SqlCommand sqlCommand = new SqlCommand(query);
            sqlCommand.Parameters.AddWithValue("@Student_ID", Student_ID);
            return new MSSQLFactory().GetObjectList<SemesterModel>(sqlCommand);
        }
        public int AddResult(SemesterModel semester, string semesterName, Programme programme, bool isHistory = false)
        {
            string tableName = new GeneralFunctions().GetSemesterTableName(semesterName, programme, isHistory);
            List<string> ignoreQuery = null;

            return new MSSQLFactory().InsertRecord(semester, tableName, ignoreQuery);

        }
        //public int AddResultHistory(ResultHistory resultHistory)
        //{
        //    return new MSSQLFactory().InsertRecord(resultHistory);

        //}
        public int UpdateAward(SemesterModel model, PrintProgramme printProgramme, short semester)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            List<string> ignoreProperties = new List<string>() { nameof(model._ID) };
            SqlCommand sqlCommand = new MSSQLFactory().UpdateRecord(model, ignoreProperties, null, $"{printProgramme.ToString()}_Semester{semester}");
            sqlCommand.CommandText += " WHERE _ID=@_ID";
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }



        public int ReplaceSubjectInResult(Programme programme, short semester, Guid _Combination_ID, Guid _FindSubject, Guid _ReplaceWithSubject)
        {
            string prg = new GeneralFunctions().GetProgrammePFix(programme);
            var query = $@"Update {prg}_Semester{semester} set Subject_ID='{_ReplaceWithSubject}'
                            from ARGSelectedCombination_{prg} SC
                            join ADMCombinationMaster CM  on CM.Combination_ID = SC.Combination_ID 
                            join {prg}_Semester{semester} R on R.Student_ID = SC.Student_ID 
                            where 
                             SC.Combination_ID='{_Combination_ID}' and SC.Semester={semester}  
                             and CM.CombinationSubjects like '%{_ReplaceWithSubject}%' 
                             and R.Subject_ID = '{_FindSubject}'";
            return new MSSQLFactory().ExecuteNonQuery(query);
        }


        public int InsertAward(SemesterModel model, PrintProgramme printProgramme, short semester)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            return new MSSQLFactory().InsertRecord(model, $"{printProgramme.ToString()}_Semester{semester}");
        }


        public SemesterModel GetSemesterResultBy_ID(Guid _ID, PrintProgramme printProgramme, short Semester)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            return new MSSQLFactory().GetObject<SemesterModel>(new ResultSQLQueries().GetSemesterResultBy_ID(_ID, printProgramme, Semester));
        }

        public int UpdateSubmitAward(SubmitAward input, string column, bool IsSkillORGE)
        {
            if (IsSkillORGE)
            {
                SubmitAward award = new SubmitAward() { CombinationSubjects = input.CombinationSubjects, MarksFor = input.MarksFor, Programme = Programme.IG, Semester = input.Semester, Year = input.Year };
                new MSSQLFactory().ExecuteNonQuery(new AwardSQLQueries().UpdateSubmitAward(award, column));
            }
            return new MSSQLFactory().ExecuteNonQuery(new AwardSQLQueries().UpdateSubmitAward(input, column));
        }
        public int UpdateAward(SubmitAward input, string column, List<string> strings, bool IsSkillORGE)
        {
            if (IsSkillORGE)
            {
                SubmitAward award = new SubmitAward() { CombinationSubjects = input.CombinationSubjects, MarksFor = input.MarksFor, Programme = Programme.IG, Semester = input.Semester, Year = input.Year };
                new MSSQLFactory().ExecuteNonQuery(new AwardSQLQueries().UpdateAward(award, strings, column));
            }
            return new MSSQLFactory().ExecuteNonQuery(new AwardSQLQueries().UpdateAward(input, strings, column));
        }
        public bool CheckAwardExists(SubmitAward input)
        {
            return new MSSQLFactory().ExecuteScalar<int>(new AwardSQLQueries().CheckAwardExists(input)) > 0;
        }
        public bool CheckAwardSubmitted(Parameters parameter, Programme programme, MarksFor marksFor, int batchorYear)
        {
            return new MSSQLFactory().ExecuteScalar<bool>(new AwardSQLQueries().CheckAwardSubmitted(batchorYear, parameter, programme, marksFor));
        }
        public bool checkIsResultDeclared(int Semester, Programme programme)
        {
            return new MSSQLFactory().ExecuteScalar<int>(new AwardSQLQueries().checkIsResultDeclared(Semester, programme)) > 0;
        }
        public AwardFilterSettings FetchAwardFilterSettings(Programme programme, MarksFor marksFor, int semester)
        {
            return new MSSQLFactory().GetObject<AwardFilterSettings>(new AwardSQLQueries().GetAwardFilterSettings(programme, marksFor, semester));
        }
        public AwardSettingsModel FetchAwardSettings(SubmitAward input)
        {
            return new MSSQLFactory().GetObject<AwardSettingsModel>(new AwardSQLQueries().FetchAwardSettings(input));
        }


        public List<ARGStudentReExamForm> GetAppliedReAppearSubjects(Guid student_ID, PrintProgramme printProgramme, short semester, int Year)
        {
            return new MSSQLFactory().GetObjectList<ARGStudentReExamForm>(new ResultSQLQueries().GetAppliedReAppearSubjects(student_ID, printProgramme, semester, Year));
        }

        public List<ARGStudentReExamForm> GetAppliedReAppearSubjects(Guid studentExamForm_ID, PrintProgramme printProgramme)
        {
            return new MSSQLFactory().GetObjectList<ARGStudentReExamForm>(new ResultSQLQueries().GetAppliedReAppearSubjects(studentExamForm_ID, printProgramme));
        }


        public int Save(ResultNotification model)
        {
            return new MSSQLFactory().InsertRecord(model);
        }


        public int Save(List<Guid> model)
        {
            string TSQL = $@"INSERT INTO dbo.MSFullDetails
                                SELECT NEWID(),ADMSubjectMaster.Subject_ID,ADMSubjectMaster.SubjectCode,ADMSubjectMaster.SubjectFullName,ADMSubjectMaster.SubjectType,IH_SemesterAll.Semester,TotalCredit,CreditWeightage,ARGPersonalInformation_IH.Student_ID,
                                       InternalMinPassMarks,InternalMaxMarks,InternalMarks,IsInternalPassed,ExternalMinPassMarks,ExternalMaxMarks,
                                       ExternalMarks,IsExternalPassed ,1 FROM dbo.ARGPersonalInformation_IH JOIN dbo.IH_SemesterAll
									   ON IH_SemesterAll.Student_ID = ARGPersonalInformation_IH.Student_ID
									   JOIN dbo.ADMSubjectMaster ON ADMSubjectMaster.Subject_ID = IH_SemesterAll.Subject_ID
									   JOIN dbo.MSSubjectMarksStructure ON MSSubjectMarksStructure.SubjectMarksStructure_ID = ADMSubjectMaster.SubjectMarksStructure_ID
									   WHERE ARGPersonalInformation_IH.Student_ID IN({model.ToIN()})";
            string SQL = "\r\n" + $@"INSERT INTO dbo.MSStatus 
                                        
                                        SELECT NEWID() AS ID,       
       P.Student_ID,
       MAX(RN.Dated) AS DateOfDeclaration,
       MAX(RN.NotificationNo) AS NotificationNO,
       --ROW_NUMBER() OVER (ORDER BY P.Student_ID) AS MarksSheetNO,
        RAND() AS MarksSheetNO,
       NULL AS IsLocked,
       GETDATE() AS CreatedOn,
       NULL AS PrintedOn,
       NULL AS Printedby,
       NULL AS validatedon,
       NULL AS Validatedby,
       VW.Course_ID,
       NULL AS HandedOverOn,
       NULL AS HandedOverBy_ID,
       CAST(SUM(COALESCE(COALESCE(S.InternalMarks, NULL, 0), -2, 0)
                + COALESCE(COALESCE(S.InternalAttendance_AssessmentMarks, NULL, 0), -2, 0)
                + COALESCE(COALESCE(S.ExternalAttendance_AssessmentMarks, NULL, 0), -2, 0)
                + COALESCE(COALESCE(S.ExternalMarks, NULL, 0), -2, 0)
               ) AS BIGINT) AS MarksObt,
       CAST(SUM(COALESCE(COALESCE(MS.InternalMaxMarks, NULL, 0), -2, 0)
                + COALESCE(COALESCE(MS.InternalAttendance_AssessmentMaxMarks, NULL, 0), -2, 0)
                + COALESCE(COALESCE(MS.ExternalAttendance_AssessmentMaxMarks, NULL, 0), -2, 0)
                + COALESCE(COALESCE(MS.ExternalMaxMarks, NULL, 0), -2, 0)
               ) AS BIGINT) AS MaxMarks,
       CAST((SUM((COALESCE(COALESCE(S.InternalMarks, NULL, 0), -2, 0)
                  + COALESCE(COALESCE(S.InternalAttendance_AssessmentMarks, NULL, 0), -2, 0)
                  + COALESCE(COALESCE(S.ExternalAttendance_AssessmentMarks, NULL, 0), -2, 0)
                  + COALESCE(COALESCE(S.ExternalMarks, NULL, 0), -2, 0)
                 )
                )
             / SUM(COALESCE(COALESCE(MS.InternalMaxMarks, NULL, 0), -2, 0)
                   + COALESCE(COALESCE(MS.InternalAttendance_AssessmentMaxMarks, NULL, 0), -2, 0)
                   + COALESCE(COALESCE(MS.ExternalAttendance_AssessmentMaxMarks, NULL, 0), -2, 0)
                   + COALESCE(COALESCE(MS.ExternalMaxMarks, NULL, 0), -2, 0)
                  ) * 100
            ) AS DECIMAL(4, 2)) AS Percentage
FROM dbo.IH_SemesterAll S
    JOIN dbo.ARGPersonalInformation_IH P
        ON P.Student_ID = S.Student_ID
    JOIN dbo.VWStudentCourse VW
        ON VW.Student_ID = P.Student_ID
           AND VW.Student_ID = S.Student_ID
    JOIN dbo.ADMSubjectMaster SM
        ON SM.Subject_ID = S.Subject_ID
    JOIN dbo.MSSubjectMarksStructure MS
        ON MS.SubjectMarksStructure_ID = SM.SubjectMarksStructure_ID
    JOIN
    (
        SELECT DISTINCT
            T.NotificationNo,
            T.Student_ID,
            T.Dated
        FROM
        (
            SELECT ROW_NUMBER() OVER (PARTITION BY P.Student_ID ORDER BY P.Student_ID, RN.Dated DESC) SNO,
                   P.Student_ID,
                   RN.NotificationNo,
                   RN.Dated
            FROM dbo.IH_SemesterAll S
                JOIN dbo.ARGPersonalInformation_IH P
                    ON P.Student_ID = S.Student_ID
                JOIN dbo.ResultNotification RN
                    ON RN.ResultNotification_ID = S.ResultNotification_ID
            WHERE P.Student_ID IN ({model.ToIN()})
            GROUP BY P.Student_ID,
                     RN.NotificationNo,
                     RN.Dated
        ) T
        WHERE SNO = 1
    ) RN
        ON RN.Student_ID = P.Student_ID
           AND RN.Student_ID = S.Student_ID
WHERE P.Student_ID IN ({model.ToIN()})
GROUP BY P.Student_ID,        
         VW.Course_ID

                                    ";
            TSQL = TSQL + SQL;

            return new MSSQLFactory().ExecuteNonQuery(TSQL);
        }
        //public bool HasResult(Guid Student_ID, PrintProgramme printProgramme, short semester)
        //{
        //    printProgramme = new GeneralFunctions().MappingTable(printProgramme);
        //    var Query = $"SELECT COUNT(1) FROM {printProgramme}_Semester{semester} WHERE ResultNotification_ID IS NOT NULL AND Student_ID='{Student_ID}'";
        //    return new MSSQLFactory().ExecuteScalar<int>(Query) > 0;
        //}

        public int AddAwardSettings(AwardSettingsModel input)
        {
            return new MSSQLFactory().InsertRecord(input, "AwardSettings");
        }

        public int EditAwardSettings(AwardSettingsModel input)
        {

            int result = 0;


            List<string> ignoreQuery = new List<string>() {

                nameof(input.SUBJECT_ID),
                nameof(input.CreatedOn),
                nameof(input.CreatedBy),
                nameof(input.Status),
                nameof(input.Semester),
                nameof(input.USER_ID)

            };

            result = new AwardDB().EditAwardSettings(input, ignoreQuery, null);

            return result;
        }

        #region ResultDiscrepancy

        public int Save(SubjectResultDiscrepancy model)
        {
            return new MSSQLFactory().InsertRecord(model);
        }

        public SubjectResultDiscrepancy SubjectResultDiscrepancy(Guid Student_ID)
        {
            var Query = $@"Select SubjectResultDiscrepancy.*,Semester,SubjectFullName from SubjectResultDiscrepancy
                        JOIN ADMSubjectMaster ON ADMSubjectMaster.Subject_ID = SubjectResultDiscrepancy.Subject_ID where Student_ID='{Student_ID}'";
            return new MSSQLFactory().GetObject<SubjectResultDiscrepancy>(Query);
        }

        public DataTable SubjectResultDiscrepancyDataTable()
        {
            var Query = $@"Select S.CUSRegistrationNo,E.ExamRollNumber,S.ClassRollNo,S.FullName,S.FathersName,A.Mobile,A.Email,SubjectFullName,Remark
                            FROM SubjectResultDiscrepancy SD
                            JOIN ARGPersonalInformation_UG S ON S.Student_ID = SD.Student_ID
                            JOIN (SELECT DISTINCT ExamRollNumber,Student_ID FROM ARGStudentExamForm_UG WHERE ExamRollNumber IS NOT NULL) E ON E.Student_ID = S.Student_ID
                            JOIN ARGStudentAddress_UG A ON A.Student_ID = S.Student_ID
                            JOIN ADMSubjectMaster ON ADMSubjectMaster.Subject_ID = SD.Subject_ID
                            ORDER BY SubjectFullName,E.ExamRollNumber";
            return new MSSQLFactory().GetDataTable(Query);
        }
        #endregion

        #region TempAverage


        //public int InsertTempAverageResult(string Query)
        //{
        //    return new MSSQLFactory().ExecuteNonQuery(Query);
        //}

        #endregion

        public SubjectResult SubjectResult(PrintProgramme printProgramme, Guid Subject_ID, Guid Student_ID, short semester)
        {
            Parameters parameter = new Parameters();
            parameter.Filters = new List<SearchFilter>();
            parameter.Filters.Add(
                    new SearchFilter()
                    {
                        Column = "Subject_ID",
                        Value = Subject_ID.ToString(),
                        TableAlias = "COMB",
                    }
                );
            parameter.Filters.Add(
                new SearchFilter()
                {
                    Column = "Student_ID",
                    TableAlias = "S",
                    Operator = SQLOperator.EqualTo,
                    Value = Student_ID.ToString()
                });

            string Query = new ResultSQLQueries().ResultList_SubjectResult_Query(false, printProgramme, semester, false);
            var Helper = new GeneralFunctions().GetWhereClause<FilterModels>(parameter.Filters);
            Helper.Command.CommandText = Query + Helper.WhereClause;
            var dt = new MSSQLFactory().GetDataTable(Helper.Command);
            return parseDataTableToSubjectResultList(dt)?.FirstOrDefault();
        }

        #region Private Methods
        private List<SubjectResult> SubjectResultList(PrintProgramme printProgramme, short? semester, Parameters parameter, bool IsEditable = false)
        {

            string Query = new ResultSQLQueries().ResultList_SubjectResult_Query(false, printProgramme, semester, IsEditable);
            var Helper = new GeneralFunctions().GetWhereClause<FilterModels>(parameter.Filters);
            Helper.Command.CommandText = Query + Helper.WhereClause; //+ new GeneralFunctions().GetPagedQuery(Query, parameter);
            var dt = new MSSQLFactory().GetDataTable(Helper.Command);
            List<SubjectResult> list = parseDataTableToSubjectResultList(dt);
            return list;
        }

        private List<SubjectResult> AllSubjectDetails(PrintProgramme printProgramme, short? semester, Parameters parameter, bool IsEditable = false)
        {
            string Query = new ResultSQLQueries().AllSubjectDetailsQuery(false, printProgramme, semester, IsEditable);
            var Helper = new GeneralFunctions().GetWhereClause<FilterModels>(parameter.Filters);

            SearchFilter progfilter = parameter.Filters.Where(x => x.Column == "SemesterBatch").FirstOrDefault();
            string SemesterBatch = string.Empty;
            if (progfilter != null)
            {
                SemesterBatch = progfilter.Value;
            }
            SearchFilter batchfilter = parameter.Filters.Where(x => x.Column == "Programme").FirstOrDefault();
            string Programme = string.Empty;
            if (batchfilter != null)
            {
                Programme = batchfilter.Value;
            }
            SearchFilter coursefilter = parameter.Filters.Where(x => x.Column == "Course_ID").FirstOrDefault();
            string Course_ID = string.Empty;
            if (coursefilter != null)
            {
                Course_ID = coursefilter.Value;
            }
            Helper.Command.CommandText = Query + $" WHERE  ( SemesterBatch =   {SemesterBatch}) AND ( course.Programme = {Programme} ) AND ( Course.Course_ID = '{Course_ID}')  AND ( R.IsInternalPassed = 1 ) and (( R.IsExternalPassed = 1  and MS.IsExternalMarksApplicable=1) OR (MS.IsExternalMarksApplicable=0))ORDER BY COMB.semester";
            var dt = new MSSQLFactory().GetDataTable(Helper.Command);
            List<SubjectResult> list = parseDataTableToSubjectResultList(dt);
            return list;
        }


        private List<SubjectResult> SubjectResultListDetails(PrintProgramme printProgramme, short? semester, Parameters parameter, bool IsEditable = false)
        {
            string Query = new ResultSQLQueries().ResultList_SubjectResult_QueryDetails(false, printProgramme, semester, IsEditable);
            var Helper = new GeneralFunctions().GetWhereClause<FilterModels>(parameter.Filters);

            SearchFilter progfilter = parameter.Filters.Where(x => x.Column == "SemesterBatch").FirstOrDefault();
            string SemesterBatch = string.Empty;
            if (progfilter != null)
            {
                SemesterBatch = progfilter.Value;
            }
            SearchFilter batchfilter = parameter.Filters.Where(x => x.Column == "Programme").FirstOrDefault();
            string Programme = string.Empty;
            if (batchfilter != null)
            {
                Programme = batchfilter.Value;
            }
            SearchFilter coursefilter = parameter.Filters.Where(x => x.Column == "Course_ID").FirstOrDefault();
            string Course_ID = string.Empty;
            if (coursefilter != null)
            {
                Course_ID = coursefilter.Value;
            }
            Helper.Command.CommandText = Query + $" WHERE  ( SemesterBatch =   {SemesterBatch}) AND ( course.Programme = {Programme} ) AND ( Course.Course_ID = '{Course_ID}')  AND ( R.IsInternalPassed = 1 ) and (( R.IsExternalPassed = 1  and MS.IsExternalMarksApplicable=1) OR (MS.IsExternalMarksApplicable=0))ORDER BY COMB.semester";
            var dt = new MSSQLFactory().GetDataTable(Helper.Command);
            List<SubjectResult> list = parseDataTableToSubjectResultList(dt);
            return list;
        }



        private List<SubjectResult> SubjectResultListDetailsFullGazette(PrintProgramme printProgramme, short? semester, Parameters parameter, bool IsEditable = false)
        {
            string Query = new ResultSQLQueries().ResultList_SubjectResult_QueryDetails(false, printProgramme, semester, IsEditable);
            var Helper = new GeneralFunctions().GetWhereClause<FilterModels>(parameter.Filters);

            SearchFilter progfilter = parameter.Filters.Where(x => x.Column == "Batch").FirstOrDefault();
            string SemesterBatch = string.Empty;
            if (progfilter != null)
            {
                SemesterBatch = progfilter.Value;
            }
            SearchFilter batchfilter = parameter.Filters.Where(x => x.Column == "Programme").FirstOrDefault();
            string Programme = string.Empty;
            if (batchfilter != null)
            {
                Programme = batchfilter.Value;
            }
            SearchFilter coursefilter = parameter.Filters.Where(x => x.Column == "Course_ID").FirstOrDefault();
            string Course_ID = string.Empty;
            if (coursefilter != null)
            {
                Course_ID = coursefilter.Value;
            }
            Helper.Command.CommandText = Query + $" WHERE  ( SemesterBatch =   {SemesterBatch}) AND ( course.Programme = {Programme} ) AND ( Course.Course_ID = '{Course_ID}') ORDER BY COMB.semester";
            var dt = new MSSQLFactory().GetDataTable(Helper.Command);
            List<SubjectResult> list = parseDataTableToSubjectResultListEng(dt);
            return list;
        }







        public List<ResultCompact> StudentInfoDetailsForEngFullGazette(PrintProgramme printProgramme, short? semester, Parameters parameter, bool IsEditable = false)
        {
            var Query = new ResultSQLQueries().StudentInfoDetailsForEng(printProgramme, semester, IsEditable);
            var Helper = new GeneralFunctions().GetWhereClause<FilterModels>(parameter.Filters);
            Helper.Command.CommandText = Query.Replace("_WHERE_CLAUSE_", Helper.WhereClause) + new GeneralFunctions().GetPagedQuery(Query, parameter);

            var dt = new MSSQLFactory().GetDataTable(Helper.Command);
            List<ResultCompact> list = parseDataTableToResultCompactListEng(dt);

            if (list.IsNotNullOrEmpty())
            {
                if (parameter.PageInfo.PageSize >= 0 && !parameter.Filters.Any(x => x.Column == "Student_ID"))
                {
                    var studentListInClause = string.Join(",", list.Select(x => x.Student_ID));
                    parameter.Filters.Add(new SearchFilter() { Column = "Student_ID", TableAlias = "S", Operator = SQLOperator.In, Value = studentListInClause });
                }
                if (parameter.Filters.Any(x => x.Column == "ExamRollNumber"))
                {
                    //parameter.Filters.Remove(parameter.Filters.First(x => x.Column == "ExamRollNumber"));
                }
                if (parameter.Filters.Any(x => x.Column == "CombinationSubjects"))
                {
                    parameter.Filters.Add(new SearchFilter() { Column = "CM.Subject_ID", Value = parameter.Filters.First(x => x.Column == "CombinationSubjects").Value });
                    parameter.Filters.Remove(parameter.Filters.First(x => x.Column == "CombinationSubjects"));
                }

                parameter.Filters.Remove(parameter.Filters.FirstOrDefault(x => x.Column.ToLower().Trim() == "semester" && x.TableAlias.ToLower().Trim() == "comb") ?? new SearchFilter());

                var subjectResultList = SubjectResultListDetails(printProgramme, semester, parameter, IsEditable);
                if (subjectResultList.IsNotNullOrEmpty())
                    list.ForEach(x => x.SubjectResults = subjectResultList.Where(y => y.Student_ID == x.Student_ID)?.ToList());
            }
            return list;
        }


        private List<ResultCompact> parseDataTableToResultCompactListEng(DataTable dataTable)
        {
            if (dataTable == null || dataTable.Rows.Count == 0) return null;
            var hasSemesterColumn = dataTable.Columns.Contains("Semester");
            var hasSemesterBatchColumn = dataTable.Columns.Contains("SemesterBatch");
            var hasCombination_IDColumn = dataTable.Columns.Contains("Combination_ID");
            #region testBlock
            try
            {
                var model = new ResultCompact();
                foreach (var col in dataTable.AsEnumerable())
                {
                    model.Student_ID = col.Field<Guid>("Student_ID");
                    model.CUSRegistrationNo = col.Field<string>("CUSRegistrationNo");
                    model.Gender = col.Field<string>("Gender");
                    model.Combination_ID = hasCombination_IDColumn ? col.Field<Guid?>("Combination_ID") : null;
                    model.Category = col.Field<string>("Category");
                    model.ClassRollNo = col.Field<string>("ClassRollNo");
                    model.FullName = col.Field<string>("FullName");
                    model.FathersName = col.Field<string>("FathersName");
                    model.ExamRollNumber = col.Field<string>("ExamRollNumber");
                    model.Course_ID = col.Field<Guid>("Course_ID");
                    model.CourseFullName = col.Field<string>("CourseFullName");
                    model.Batch = col.Field<short>("Batch");
                    model.SemesterBatch = hasSemesterBatchColumn ? col.Field<short>("SemesterBatch") : short.Parse("0");
                    model.Semester = hasSemesterColumn ? col.Field<short>("Semester") : short.Parse("0");
                    model.PrintProgramme = Extensions.Helper.ToEnum<PrintProgramme>(col.Field<object>("PrintProgramme").ToString());
                    model.Programme = Extensions.Helper.ToEnum<Programme>(col.Field<object>("Programme").ToString());
                    model.AcceptCollege_ID = col.Field<Guid>("AcceptCollege_ID");
                    model.CollegeFullName = col.Field<string>("CollegeFullName");
                    model.HasTranscript = col.Field<string>("HasTranscript") == "1";
                    model.TranscriptPrinted = col.Field<string>("TranscriptPrinted") == "1";
                    model.NotificationNo = col.Field<string>("NotificationNo");
                    model.Address = col.Field<string>("Address");
                }
            }
            catch (Exception)
            {

            }
            #endregion



            var list = dataTable.AsEnumerable().Select(col => new ResultCompact()
            {
                Student_ID = col.Field<Guid>("Student_ID"),
                CUSRegistrationNo = col.Field<string>("CUSRegistrationNo"),
                Gender = col.Field<string>("Gender"),
                DegreeCourseTitle = col.Field<string>("DegreeCourseTitle"),
                Combination_ID = hasCombination_IDColumn ? col.Field<Guid?>("Combination_ID") : null,
                Category = col.Field<string>("Category"),
                ClassRollNo = col.Field<string>("ClassRollNo"),
                FullName = col.Field<string>("FullName"),
                FathersName = col.Field<string>("FathersName"),
                ExamRollNumber = col.Field<string>("ExamRollNumber"),
                Course_ID = col.Field<Guid>("Course_ID"),
                CourseFullName = col.Field<string>("CourseFullName"),
                Batch = col.Field<short>("Batch"),
                SemesterBatch = hasSemesterBatchColumn ? col.Field<short>("SemesterBatch") : short.Parse("0"),
                Semester = hasSemesterColumn ? col.Field<short>("Semester") : short.Parse("0"),
                PrintProgramme = Extensions.Helper.ToEnum<PrintProgramme>(col.Field<object>("PrintProgramme").ToString()),
                Programme = Extensions.Helper.ToEnum<Programme>(col.Field<object>("Programme").ToString()),
                AcceptCollege_ID = col.Field<Guid>("AcceptCollege_ID"),
                CollegeFullName = col.Field<string>("CollegeFullName"),
                HasTranscript = col.Field<bool>("HasTranscript"),
                TranscriptPrinted = col.Field<bool>("TranscriptPrinted"),
                //NotificationNo = col.Field<string>("NotificationNo"),
                Address = col.Field<string>("Address")
            }).Distinct().ToList();
            return list;
        }



        private List<ResultCompact> parseDataTableToResultCompactList(DataTable dataTable)
        {
            if (dataTable == null || dataTable.Rows.Count == 0) return null;
            var hasSemesterColumn = dataTable.Columns.Contains("Semester");
            var hasSemesterBatchColumn = dataTable.Columns.Contains("SemesterBatch");
            var hasCombination_IDColumn = dataTable.Columns.Contains("Combination_ID");
            #region testBlock
            try
            {
                var model = new ResultCompact();
                foreach (var col in dataTable.AsEnumerable())
                {
                    model.Student_ID = col.Field<Guid>("Student_ID");
                    model.CUSRegistrationNo = col.Field<string>("CUSRegistrationNo");
                    model.Gender = col.Field<string>("Gender");
                    model.Combination_ID = hasCombination_IDColumn ? col.Field<Guid?>("Combination_ID") : null;
                    model.Category = col.Field<string>("Category");
                    model.ClassRollNo = col.Field<string>("ClassRollNo");
                    model.FullName = col.Field<string>("FullName");
                    model.FathersName = col.Field<string>("FathersName");
                    model.ExamRollNumber = col.Field<string>("ExamRollNumber");
                    model.Course_ID = col.Field<Guid>("Course_ID");
                    model.CourseFullName = col.Field<string>("CourseFullName");
                    model.Batch = col.Field<short>("Batch");
                    model.SemesterBatch = hasSemesterBatchColumn ? col.Field<short>("SemesterBatch") : short.Parse("0");
                    model.Semester = hasSemesterColumn ? col.Field<short>("Semester") : short.Parse("0");
                    model.PrintProgramme = Extensions.Helper.ToEnum<PrintProgramme>(col.Field<object>("PrintProgramme").ToString());
                    model.Programme = Extensions.Helper.ToEnum<Programme>(col.Field<object>("Programme").ToString());
                    model.AcceptCollege_ID = col.Field<Guid>("AcceptCollege_ID");
                    model.CollegeFullName = col.Field<string>("CollegeFullName");
                    model.HasTranscript = col.Field<string>("HasTranscript") == "1";
                    model.TranscriptPrinted = col.Field<string>("TranscriptPrinted") == "1";
                }
            }
            catch (Exception)
            {

            }
            #endregion



            var list = dataTable.AsEnumerable().Select(col => new ResultCompact()
            {
                Student_ID = col.Field<Guid>("Student_ID"),
                CUSRegistrationNo = col.Field<string>("CUSRegistrationNo"),
                Gender = col.Field<string>("Gender"),
                DegreeCourseTitle = col.Field<string>("DegreeCourseTitle"),
                Combination_ID = hasCombination_IDColumn ? col.Field<Guid?>("Combination_ID") : null,
                Category = col.Field<string>("Category"),
                ClassRollNo = col.Field<string>("ClassRollNo"),
                FullName = col.Field<string>("FullName"),
                FathersName = col.Field<string>("FathersName"),
                ExamRollNumber = col.Field<string>("ExamRollNumber"),
                Course_ID = col.Field<Guid>("Course_ID"),
                CourseFullName = col.Field<string>("CourseFullName"),
                Batch = col.Field<short>("Batch"),
                SemesterBatch = hasSemesterBatchColumn ? col.Field<short>("SemesterBatch") : short.Parse("0"),
                Semester = hasSemesterColumn ? col.Field<short>("Semester") : short.Parse("0"),
                PrintProgramme = Extensions.Helper.ToEnum<PrintProgramme>(col.Field<object>("PrintProgramme").ToString()),
                Programme = Extensions.Helper.ToEnum<Programme>(col.Field<object>("Programme").ToString()),
                AcceptCollege_ID = col.Field<Guid>("AcceptCollege_ID"),
                CollegeFullName = col.Field<string>("CollegeFullName"),
                HasTranscript = col.Field<bool>("HasTranscript"),
                TranscriptPrinted = col.Field<bool>("TranscriptPrinted")
            }).Distinct().ToList();
            return list;
        }
        private List<SubjectResult> parseDataTableToSubjectResultList(DataTable dt)
        {
            if (dt == null || dt.Rows.Count == 0) return null;
            #region testblock
            //SubjectResult model = new SubjectResult();
            //foreach (var col in dt.AsEnumerable())
            //{
            //    model.Student_ID = col.Field<Guid>("Student_ID");
            //    model.Semester = col.Field<short>("Semester");
            //    model.SemesterBatch = col.Field<short>("SemesterBatch");
            //    model.Programme = Extensions.Helper.ToEnum<Programme>(col.Field<object>("Programme").ToString());
            //    model.TotalCredit = int.Parse(col.Field<object>("TotalCredit").ToString());
            //    model._ID = col.Field<Guid?>("_ID") ?? Guid.Empty;
            //    model.Subject_ID = col.Field<Guid?>("Subject_ID") ?? Guid.Empty;
            //    model.SubjectFullName = col.Field<string>("SubjectFullName");
            //    model.SubjectType = Extensions.Helper.ToEnum<SubjectType>((col.Field<object>("SubjectType") ?? 0).ToString());
            //    model.SubjectCode = col.Field<string>("SubjectCode");
            //    model.HasResult = col.Field<bool?>("HasResult") ?? false;
            //    model.HasExaminationFee = col.Field<bool?>("HasExaminationFee") ?? false;
            //    model.IsInternalMarksApplicable = col.Field<bool?>("IsInternalMarksApplicable") ?? false;
            //    model.InternalMaxMarks = col.Field<int?>("InternalMaxMarks") ?? 0;
            //    model.InternalIsPartOf = Extensions.Helper.ToEnum<MarksIsPartOf>((col.Field<object>("InternalIsPartOf") ?? 0).ToString());
            //    model.InternalMinPassMarks = col.Field<int?>("InternalMinPassMarks") ?? 0;
            //    model.IsInternalAttendance_AssessmentMarksApplicable = col.Field<bool?>("IsInternalAttendance_AssessmentMarksApplicable") ?? false;
            //    model.InternalAttendance_AssessmentMaxMarks = col.Field<int?>("InternalAttendance_AssessmentMaxMarks") ?? 0;
            //    model.InternalAttendance_AssessmentMinPassMarks = col.Field<int?>("InternalAttendance_AssessmentMinPassMarks") ?? 0;
            //    model.InternalAttendance_AssessmentIsPartOf = Extensions.Helper.ToEnum<MarksIsPartOf>((col.Field<object>("InternalAttendance_AssessmentIsPartOf") ?? 0).ToString());
            //    model.IsExternalAttendance_AssessmentMarksApplicable = col.Field<bool?>("IsExternalAttendance_AssessmentMarksApplicable") ?? false;
            //    model.ExternalAttendance_AssessmentMaxMarks = col.Field<int?>("ExternalAttendance_AssessmentMaxMarks") ?? 0;
            //    model.ExternalAttendance_AssessmentMinPassMarks = col.Field<int?>("ExternalAttendance_AssessmentMinPassMarks") ?? 0;
            //    model.ExternalAttendance_AssessmentIsPartOf = Extensions.Helper.ToEnum<MarksIsPartOf>((col.Field<object>("ExternalAttendance_AssessmentIsPartOf") ?? 0).ToString());
            //    model.IsExternalMarksApplicable = col.Field<bool?>("IsExternalMarksApplicable") ?? false;
            //    model.ExternalMaxMarks = col.Field<int?>("ExternalMaxMarks") ?? 0;
            //    model.ExternalMinPassMarks = col.Field<int?>("ExternalMinPassMarks") ?? 0;
            //    model.ExternalIsPartOf = Extensions.Helper.ToEnum<MarksIsPartOf>((col.Field<object>("ExternalIsPartOf") ?? 0).ToString());
            //    model.InternalMarks = col.Field<decimal?>("InternalMarks");
            //    model.InternalAttendance_AssessmentMarks = col.Field<decimal?>("InternalAttendance_AssessmentMarks");
            //    model.ExternalAttendance_AssessmentMarks = col.Field<decimal?>("ExternalAttendance_AssessmentMarks");
            //    model.ExternalMarks = col.Field<decimal?>("ExternalMarks");
            //    model.IsInternalPassed = col.Field<bool?>("IsInternalPassed") ?? false;
            //    model.IsExternalPassed = col.Field<bool?>("IsExternalPassed") ?? false;
            //    model.ExternalSubmitted = col.Field<bool?>("ExternalSubmitted") ?? false;
            //    model.InternalSubmitted = col.Field<bool?>("InternalSubmitted") ?? false;
            //    model.ResultNotification_ID = col.Field<Guid?>("ResultNotification_ID");
            //    model.ExamForm_ID = col.Field<Guid?>("ExamForm_ID");
            //    model.ParentNotification_ID = col.Field<Guid?>("ParentNotification_ID");
            //    model.NotificationNo = col.Field<string>("NotificationNo");
            //    model.NotificationDate = col.Field<DateTime?>("NotificationDate");
            //}
            #endregion

            List<SubjectResult> list = dt.AsEnumerable().Select(col => new SubjectResult()
            {
                Student_ID = col.Field<Guid>("Student_ID"),
                Semester = col.Field<short>("Semester"),
                SemesterBatch = col.Field<short>("SemesterBatch"),
                Programme = Extensions.Helper.ToEnum<Programme>(col.Field<object>("Programme").ToString()),
                TotalCredit = decimal.Parse(col.Field<object>("TotalCredit").ToString()),
                _ID = col.Field<Guid?>("_ID") ?? Guid.Empty,
                Subject_ID = col.Field<Guid?>("Subject_ID") ?? Guid.Empty,
                SubjectFullName = col.Field<string>("SubjectFullName"),
                SubjectType = Extensions.Helper.ToEnum<SubjectType>((col.Field<object>("SubjectType") ?? 0).ToString()),
                SubjectCode = col.Field<string>("SubjectCode"),
                HasResult = col.Field<bool?>("HasResult") ?? false,
                HasExaminationFee = col.Field<bool?>("HasExaminationFee") ?? false,
                IsInternalMarksApplicable = col.Field<bool?>("IsInternalMarksApplicable") ?? false,
                InternalMaxMarks = col.Field<decimal?>("InternalMaxMarks") ?? 0,
                InternalIsPartOf = Extensions.Helper.ToEnum<MarksIsPartOf>((col.Field<object>("InternalIsPartOf") ?? 0).ToString()),
                InternalMinPassMarks = col.Field<decimal?>("InternalMinPassMarks") ?? 0,
                IsInternalAttendance_AssessmentMarksApplicable = col.Field<bool?>("IsInternalAttendance_AssessmentMarksApplicable") ?? false,
                InternalAttendance_AssessmentMaxMarks = col.Field<decimal?>("InternalAttendance_AssessmentMaxMarks") ?? 0,
                InternalAttendance_AssessmentMinPassMarks = col.Field<decimal?>("InternalAttendance_AssessmentMinPassMarks") ?? 0,
                InternalAttendance_AssessmentIsPartOf = Extensions.Helper.ToEnum<MarksIsPartOf>((col.Field<object>("InternalAttendance_AssessmentIsPartOf") ?? 0).ToString()),
                IsExternalAttendance_AssessmentMarksApplicable = col.Field<bool?>("IsExternalAttendance_AssessmentMarksApplicable") ?? false,
                ExternalAttendance_AssessmentMaxMarks = col.Field<decimal?>("ExternalAttendance_AssessmentMaxMarks") ?? 0,
                ExternalAttendance_AssessmentMinPassMarks = col.Field<decimal?>("ExternalAttendance_AssessmentMinPassMarks") ?? 0,
                ExternalAttendance_AssessmentIsPartOf = Extensions.Helper.ToEnum<MarksIsPartOf>((col.Field<object>("ExternalAttendance_AssessmentIsPartOf") ?? 0).ToString()),
                IsExternalMarksApplicable = col.Field<bool?>("IsExternalMarksApplicable") ?? false,
                ExternalMaxMarks = col.Field<decimal?>("ExternalMaxMarks") ?? 0,
                ExternalMinPassMarks = col.Field<decimal?>("ExternalMinPassMarks") ?? 0,
                ExternalIsPartOf = Extensions.Helper.ToEnum<MarksIsPartOf>((col.Field<object>("ExternalIsPartOf") ?? 0).ToString()),
                InternalMarks = col.Field<decimal?>("InternalMarks"),
                InternalAttendance_AssessmentMarks = col.Field<decimal?>("InternalAttendance_AssessmentMarks"),
                ExternalAttendance_AssessmentMarks = col.Field<decimal?>("ExternalAttendance_AssessmentMarks"),
                ExternalMarks = col.Field<decimal?>("ExternalMarks"),
                IsInternalPassed = col.Field<bool?>("IsInternalPassed") ?? false,
                IsExternalPassed = col.Field<bool?>("IsExternalPassed") ?? false,
                ExternalSubmitted = col.Field<bool?>("ExternalSubmitted") ?? false,
                InternalSubmitted = col.Field<bool?>("InternalSubmitted") ?? false,
                ResultNotification_ID = col.Field<Guid?>("ResultNotification_ID"),
                ExamForm_ID = col.Field<Guid?>("ExamForm_ID"),
                ParentNotification_ID = col.Field<Guid?>("ParentNotification_ID"),
                NotificationNo = col.Field<string>("NotificationNo"),
                NotificationDate = col.Field<DateTime?>("NotificationDate"),
                ResultAnomalies_Id = col.Field<Guid?>("ResultAnomalies_Id"),
            })?.ToList();
            return list;
        }

        private List<SubjectResult> parseDataTableToSubjectResultListEng(DataTable dt)
        {
            if (dt == null || dt.Rows.Count == 0) return null;
            
            List<SubjectResult> list = dt.AsEnumerable().Select(col => new SubjectResult()
            {
                Student_ID = col.Field<Guid>("Student_ID"),
                Semester = col.Field<short>("Semester"),
                SemesterBatch = col.Field<short>("SemesterBatch"),
                Programme = Extensions.Helper.ToEnum<Programme>(col.Field<object>("Programme").ToString()),
                TotalCredit = decimal.Parse(col.Field<object>("TotalCredit").ToString()),
                _ID = col.Field<Guid?>("_ID") ?? Guid.Empty,
                Subject_ID = col.Field<Guid?>("Subject_ID") ?? Guid.Empty,
                SubjectFullName = col.Field<string>("SubjectFullName"),
                SubjectType = Extensions.Helper.ToEnum<SubjectType>((col.Field<object>("SubjectType") ?? 0).ToString()),
                SubjectCode = col.Field<string>("SubjectCode"),
                HasResult = col.Field<bool?>("HasResult") ?? false,
                HasExaminationFee = col.Field<bool?>("HasExaminationFee") ?? false,
                IsInternalMarksApplicable = col.Field<bool?>("IsInternalMarksApplicable") ?? false,
                InternalMaxMarks = col.Field<decimal?>("InternalMaxMarks") ?? 0,
                InternalIsPartOf = Extensions.Helper.ToEnum<MarksIsPartOf>((col.Field<object>("InternalIsPartOf") ?? 0).ToString()),
                InternalMinPassMarks = col.Field<decimal?>("InternalMinPassMarks") ?? 0,
                IsInternalAttendance_AssessmentMarksApplicable = col.Field<bool?>("IsInternalAttendance_AssessmentMarksApplicable") ?? false,
                InternalAttendance_AssessmentMaxMarks = col.Field<decimal?>("InternalAttendance_AssessmentMaxMarks") ?? 0,
                InternalAttendance_AssessmentMinPassMarks = col.Field<decimal?>("InternalAttendance_AssessmentMinPassMarks") ?? 0,
                InternalAttendance_AssessmentIsPartOf = Extensions.Helper.ToEnum<MarksIsPartOf>((col.Field<object>("InternalAttendance_AssessmentIsPartOf") ?? 0).ToString()),
                IsExternalAttendance_AssessmentMarksApplicable = col.Field<bool?>("IsExternalAttendance_AssessmentMarksApplicable") ?? false,
                ExternalAttendance_AssessmentMaxMarks = col.Field<decimal?>("ExternalAttendance_AssessmentMaxMarks") ?? 0,
                ExternalAttendance_AssessmentMinPassMarks = col.Field<decimal?>("ExternalAttendance_AssessmentMinPassMarks") ?? 0,
                ExternalAttendance_AssessmentIsPartOf = Extensions.Helper.ToEnum<MarksIsPartOf>((col.Field<object>("ExternalAttendance_AssessmentIsPartOf") ?? 0).ToString()),
                IsExternalMarksApplicable = col.Field<bool?>("IsExternalMarksApplicable") ?? false,
                ExternalMaxMarks = col.Field<decimal?>("ExternalMaxMarks") ?? 0,
                ExternalMinPassMarks = col.Field<decimal?>("ExternalMinPassMarks") ?? 0,
                ExternalIsPartOf = Extensions.Helper.ToEnum<MarksIsPartOf>((col.Field<object>("ExternalIsPartOf") ?? 0).ToString()),
                InternalMarks = col.Field<decimal?>("InternalMarks"),
                InternalAttendance_AssessmentMarks = col.Field<decimal?>("InternalAttendance_AssessmentMarks"),
                ExternalAttendance_AssessmentMarks = col.Field<decimal?>("ExternalAttendance_AssessmentMarks"),
                ExternalMarks = col.Field<decimal?>("ExternalMarks"),
                IsInternalPassed = col.Field<bool?>("IsInternalPassed") ?? false,
                IsExternalPassed = col.Field<bool?>("IsExternalPassed") ?? false,
                ExternalSubmitted = col.Field<bool?>("ExternalSubmitted") ?? false,
                InternalSubmitted = col.Field<bool?>("InternalSubmitted") ?? false,
                ResultNotification_ID = col.Field<Guid?>("ResultNotification_ID"),
                ExamForm_ID = col.Field<Guid?>("ExamForm_ID"),
               // ParentNotification_ID = col.Field<Guid?>("ParentNotification_ID"),
                //NotificationNo = col.Field<string>("NotificationNo"),
                //NotificationDate = col.Field<DateTime?>("NotificationDate"),
                ResultAnomalies_Id = col.Field<Guid?>("ResultAnomalies_Id"),
            })?.ToList();
            return list;
        }

        private List<ResultList> parseDataTableToResultList(DataTable dt)
        {
            if (dt == null || dt.Rows.Count == 0) return null;
            #region testblock
            //ResultList model = new ResultList();
            //string errorMessage = "";
            //foreach (var col in dt.AsEnumerable())
            //{
            //    try
            //    {
            //        model.Student_ID = col.Field<Guid>("Student_ID");
            //        model.CUSRegistrationNo = col.Field<string>("CUSRegistrationNo");
            //        model.Gender = col.Field<string>("Gender");
            //        //model.Combination_ID = col.ItemArray.Contains("Combination_ID") ? col.Field<Guid?>("Combination_ID") ?? Guid.Empty : Guid.Empty;
            //        model.Category = col.Field<string>("Category");
            //        model.ClassRollNo = col.Field<string>("ClassRollNo");
            //        model.FullName = col.Field<string>("FullName");
            //        model.FathersName = col.Field<string>("FathersName");
            //        model.ExamRollNumber = col.Field<string>("ExamRollNumber");
            //        model.Course_ID = col.Field<Guid>("Course_ID");
            //        model.CourseFullName = col.Field<string>("CourseFullName");
            //        model.Batch = col.Field<short>("Batch");
            //        model.SemesterBatch =  col.Field<short>("SemesterBatch");
            //        model.Semester = col.Field<short>("Semester");
            //        model.PrintProgramme = Extensions.Helper.ToEnum<PrintProgramme>(col.Field<object>("PrintProgramme").ToString());
            //        model.Programme = Extensions.Helper.ToEnum<Programme>(col.Field<object>("Programme").ToString());
            //        model.AcceptCollege_ID = col.Field<Guid>("AcceptCollege_ID");
            //        model.CollegeFullName = col.Field<string>("CollegeFullName");
            //        //model.TranscriptPrinted = col.ItemArray.Contains("TranscriptPrinted")?  col.Field<object>("TranscriptPrinted") == "1";
            //        //model.UpdatedON = col.ItemArray.Contains("UpdatedOn") ? col.Field<DateTime?>("SemesterBatch") ?? DateTime.Now : DateTime.Now;
            //        //model.UserName = col.ItemArray.Contains("UserName") ? col.Field<string>("UserName") ?? "" : "";
            //        model.TotalCredit = int.Parse(col.Field<object>("TotalCredit").ToString());
            //        model._ID = col.Field<Guid?>("_ID") ?? Guid.Empty;
            //        model.Subject_ID = col.Field<Guid?>("Subject_ID") ?? Guid.Empty;
            //        model.SubjectFullName = col.Field<string>("SubjectFullName");
            //        model.SubjectType = Extensions.Helper.ToEnum<SubjectType>((col.Field<object>("SubjectType") ?? 0).ToString());
            //        model.SubjectCode = col.Field<string>("SubjectCode");
            //        model.HasResult = col.Field<bool?>("HasResult") ?? false;
            //        model.HasExaminationFee = col.Field<bool?>("HasExaminationFee") ?? false;
            //        model.IsInternalMarksApplicable = col.Field<bool?>("IsInternalMarksApplicable") ?? false;
            //        model.InternalMaxMarks = col.Field<decimal?>("InternalMaxMarks") ?? 0;
            //        model.InternalIsPartOf = Extensions.Helper.ToEnum<MarksIsPartOf>((col.Field<object>("InternalIsPartOf") ?? 0).ToString());
            //        model.InternalMinPassMarks = col.Field<decimal?>("InternalMinPassMarks") ?? 0;
            //        model.IsInternalAttendance_AssessmentMarksApplicable = col.Field<bool?>("IsInternalAttendance_AssessmentMarksApplicable") ?? false;
            //        model.InternalAttendance_AssessmentMaxMarks = col.Field<decimal?>("InternalAttendance_AssessmentMaxMarks") ?? 0;
            //        model.InternalAttendance_AssessmentMinPassMarks = col.Field<decimal?>("InternalAttendance_AssessmentMinPassMarks") ?? 0;
            //        model.InternalAttendance_AssessmentIsPartOf = Extensions.Helper.ToEnum<MarksIsPartOf>((col.Field<object>("InternalAttendance_AssessmentIsPartOf") ?? 0).ToString());
            //        model.IsExternalAttendance_AssessmentMarksApplicable = col.Field<bool?>("IsExternalAttendance_AssessmentMarksApplicable") ?? false;
            //        model.ExternalAttendance_AssessmentMaxMarks = col.Field<decimal?>("ExternalAttendance_AssessmentMaxMarks") ?? 0;
            //        model.ExternalAttendance_AssessmentMinPassMarks = col.Field<decimal?>("ExternalAttendance_AssessmentMinPassMarks") ?? 0;
            //        model.ExternalAttendance_AssessmentIsPartOf = Extensions.Helper.ToEnum<MarksIsPartOf>((col.Field<object>("ExternalAttendance_AssessmentIsPartOf") ?? 0).ToString());
            //        model.IsExternalMarksApplicable = col.Field<bool?>("IsExternalMarksApplicable") ?? false;
            //        model.ExternalMaxMarks = col.Field<decimal?>("ExternalMaxMarks") ?? 0;
            //        model.ExternalMinPassMarks = col.Field<decimal?>("ExternalMinPassMarks") ?? 0;
            //        model.ExternalIsPartOf = Extensions.Helper.ToEnum<MarksIsPartOf>((col.Field<object>("ExternalIsPartOf") ?? 0).ToString());
            //        model.InternalMarks = col.Field<decimal?>("InternalMarks");
            //        model.InternalAttendance_AssessmentMarks = col.Field<decimal?>("InternalAttendance_AssessmentMarks");
            //        model.ExternalAttendance_AssessmentMarks = col.Field<decimal?>("ExternalAttendance_AssessmentMarks");
            //        model.ExternalMarks = col.Field<decimal?>("ExternalMarks");
            //        model.IsInternalPassed = col.Field<bool?>("IsInternalPassed") ?? false;
            //        model.IsExternalPassed = col.Field<bool?>("IsExternalPassed") ?? false;
            //        model.ExternalSubmitted = col.Field<bool?>("ExternalSubmitted") ?? false;
            //        model.InternalSubmitted = col.Field<bool?>("InternalSubmitted") ?? false;
            //        model.ResultNotification_ID = col.Field<Guid?>("ResultNotification_ID");
            //        model.ExamForm_ID = col.Field<Guid?>("ExamForm_ID");
            //        model.ParentNotification_ID = col.Field<Guid?>("ParentNotification_ID");
            //        model.NotificationNo = col.Field<string>("NotificationNo");
            //        model.NotificationDate = col.Field<DateTime?>("NotificationDate");
            //    }
            //    catch (Exception e)
            //    {

            //        errorMessage+=e.Message;
            //    }
            //}
            #endregion

            List<ResultList> list = dt.AsEnumerable().Select(col => new ResultList()
            {
                Student_ID = col.Field<Guid>("Student_ID"),
                CUSRegistrationNo = col.Field<string>("CUSRegistrationNo"),
                Gender = col.Field<string>("Gender"),
                //Combination_ID = col.ItemArray.Contains("Combination_ID") ? col.Field<Guid?>("Combination_ID") ?? Guid.Empty : Guid.Empty,
                Category = col.Field<string>("Category"),
                ClassRollNo = col.Field<string>("ClassRollNo"),
                FullName = col.Field<string>("FullName"),
                FathersName = col.Field<string>("FathersName"),
                ExamRollNumber = col.Field<string>("ExamRollNumber"),
                Course_ID = col.Field<Guid>("Course_ID"),
                CourseFullName = col.Field<string>("CourseFullName"),
                Batch = col.Field<short>("Batch"),
                SemesterBatch = col.Field<short>("SemesterBatch"),
                Semester = col.Field<short>("Semester"),
                PrintProgramme = Extensions.Helper.ToEnum<PrintProgramme>(col.Field<object>("PrintProgramme").ToString()),
                Programme = Extensions.Helper.ToEnum<Programme>(col.Field<object>("Programme").ToString()),
                AcceptCollege_ID = col.Field<Guid>("AcceptCollege_ID"),
                CollegeFullName = col.Field<string>("CollegeFullName"),
                //TranscriptPrinted = col.Field<bool>("TranscriptPrinted"),
                //UpdatedON = col.ItemArray.Contains("UpdatedOn") ? col.Field<DateTime?>("SemesterBatch") ?? DateTime.Now : DateTime.Now,
                //UserName = col.ItemArray.Contains("UserName") ? col.Field<string>("UserName") ?? "" : "",
                TotalCredit = decimal.Parse(col.Field<object>("TotalCredit").ToString()),
                _ID = col.Field<Guid?>("_ID") ?? Guid.Empty,
                Subject_ID = col.Field<Guid?>("Subject_ID") ?? Guid.Empty,
                SubjectFullName = col.Field<string>("SubjectFullName"),
                SubjectType = Extensions.Helper.ToEnum<SubjectType>((col.Field<object>("SubjectType") ?? 0).ToString()),
                SubjectCode = col.Field<string>("SubjectCode"),
                HasResult = col.Field<bool?>("HasResult") ?? false,
                HasExaminationFee = col.Field<bool?>("HasExaminationFee") ?? false,
                IsInternalMarksApplicable = col.Field<bool?>("IsInternalMarksApplicable") ?? false,
                InternalMaxMarks = col.Field<decimal?>("InternalMaxMarks") ?? 0,
                InternalIsPartOf = Extensions.Helper.ToEnum<MarksIsPartOf>((col.Field<object>("InternalIsPartOf") ?? 0).ToString()),
                InternalMinPassMarks = col.Field<decimal?>("InternalMinPassMarks") ?? 0,
                IsInternalAttendance_AssessmentMarksApplicable = col.Field<bool?>("IsInternalAttendance_AssessmentMarksApplicable") ?? false,
                InternalAttendance_AssessmentMaxMarks = col.Field<decimal?>("InternalAttendance_AssessmentMaxMarks") ?? 0,
                InternalAttendance_AssessmentMinPassMarks = col.Field<decimal?>("InternalAttendance_AssessmentMinPassMarks") ?? 0,
                InternalAttendance_AssessmentIsPartOf = Extensions.Helper.ToEnum<MarksIsPartOf>((col.Field<object>("InternalAttendance_AssessmentIsPartOf") ?? 0).ToString()),
                IsExternalAttendance_AssessmentMarksApplicable = col.Field<bool?>("IsExternalAttendance_AssessmentMarksApplicable") ?? false,
                ExternalAttendance_AssessmentMaxMarks = col.Field<decimal?>("ExternalAttendance_AssessmentMaxMarks") ?? 0,
                ExternalAttendance_AssessmentMinPassMarks = col.Field<decimal?>("ExternalAttendance_AssessmentMinPassMarks") ?? 0,
                ExternalAttendance_AssessmentIsPartOf = Extensions.Helper.ToEnum<MarksIsPartOf>((col.Field<object>("ExternalAttendance_AssessmentIsPartOf") ?? 0).ToString()),
                IsExternalMarksApplicable = col.Field<bool?>("IsExternalMarksApplicable") ?? false,
                ExternalMaxMarks = col.Field<decimal?>("ExternalMaxMarks") ?? 0,
                ExternalMinPassMarks = col.Field<decimal?>("ExternalMinPassMarks") ?? 0,
                ExternalIsPartOf = Extensions.Helper.ToEnum<MarksIsPartOf>((col.Field<object>("ExternalIsPartOf") ?? 0).ToString()),
                InternalMarks = col.Field<decimal?>("InternalMarks"),
                InternalAttendance_AssessmentMarks = col.Field<decimal?>("InternalAttendance_AssessmentMarks"),
                ExternalAttendance_AssessmentMarks = col.Field<decimal?>("ExternalAttendance_AssessmentMarks"),
                ExternalMarks = col.Field<decimal?>("ExternalMarks"),
                IsInternalPassed = col.Field<bool?>("IsInternalPassed") ?? false,
                IsExternalPassed = col.Field<bool?>("IsExternalPassed") ?? false,
                ExternalSubmitted = col.Field<bool?>("ExternalSubmitted") ?? false,
                InternalSubmitted = col.Field<bool?>("InternalSubmitted") ?? false,
                ResultNotification_ID = col.Field<Guid?>("ResultNotification_ID"),
                ExamForm_ID = col.Field<Guid?>("ExamForm_ID"),
                ParentNotification_ID = col.Field<Guid?>("ParentNotification_ID"),
                NotificationNo = col.Field<string>("NotificationNo"),
                NotificationDate = col.Field<DateTime?>("NotificationDate")


            })?.ToList();

            return list;
        }

        public bool GetInternalResultStatus(Guid student_ID, Guid subject_ID, short semester)
        {

            string Query = "SELECT dbo.GetInternalResult(SubjectMarksStructure_ID,VW.Student_ID,VW.Semester,VW.Subject_ID) FROM  dbo.VWStudentResultAllSemesters VW JOIN dbo.ADMSubjectMaster ON ADMSubjectMaster.Subject_ID = VW.Subject_ID AND ADMSubjectMaster.Semester = VW.Semester WHERE VW.Semester= @semester AND VW.Subject_ID=@Subject_ID AND VW.Student_ID=@Student_ID";

            SqlCommand command = new SqlCommand();
            command.Parameters.AddWithValue("@student_ID", student_ID);
            command.Parameters.AddWithValue("@subject_ID", subject_ID);
            command.Parameters.AddWithValue("@semester", semester);
            command.CommandText = Query;
            return new MSSQLFactory().ExecuteScalar<bool>(command);

        }

        #endregion


    }
}

