using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using CUSrinagar.Models;
using GeneralModels;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CUSrinagar.Models.ValidationAttrs;
using Terex;
using CUSrinagar.DataManagers.SQLQueries;

namespace CUSrinagar.DataManagers
{
    public class GeneralFunctions
    {
        public static string GetStudentCertificatePath(PrintProgramme programme)
        {
            return $"/FolderManager/StudentCertificates/{programme.ToString()}/";
        }
        public static string GetStudentPhotoPath(PrintProgramme programme)
        {
            return $"/FolderManager/StudentPhotos/{programme.ToString()}/";
        }
        public static string GetStudentPhotoPathForCertification()
        {
            return $"/FolderManager/StudentPhotos/Arabic/";
        }
        public static string GetBillingPDFFolderPath()
        {
            return $"/FolderManager/BillingPDF/";
        }

        public static PrintProgramme ProgrammeToPrintProgrammeMapping(Programme programme)
        {
            switch (programme)
            {
                case Programme.HS:
                case Programme.IG:
                case Programme.Professional:
                case Programme.Engineering:
                    return PrintProgramme.IH;
                case Programme.UG:
                    return PrintProgramme.UG;
                case Programme.PG:
                    return PrintProgramme.PG;
                default:
                    return PrintProgramme.UG;
            }
        }
        public PrintProgramme MappingTable(PrintProgramme programme)
        {
            switch (programme)
            {
                case PrintProgramme.BED:
                    programme = PrintProgramme.UG;
                    break;
            }
            return programme;
        }

        public Programme MappingTable(Programme programme)
        {
            switch (programme)
            {
                case Programme.HS:
                case Programme.Engineering:
                case Programme.Professional:
                    programme = Programme.IG;
                    break;
            }
            return programme;
        }

        public string GetPagedQuery(string query, Parameters parameter)
        {
            var paging = parameter.PageInfo;

            string queryWithOrderbyClause = string.Empty;
            if (paging == null)
            {
                if (parameter.SortInfo != null)
                {
                    if (Regex.IsMatch(query.ToUpper(), @"ORDER\s*BY"))
                        return string.Empty;
                    else
                    {
                        if (parameter.SortInfo.ColumnName != null)
                        {
                            if (parameter.SortInfo.IsAlphaNumeric)
                                queryWithOrderbyClause = SortAplhaNumeric(parameter.SortInfo.ColumnName, parameter.SortInfo.OrderBy);
                            else
                                queryWithOrderbyClause = " ORDER BY " + parameter.SortInfo.ColumnName + " " + (parameter.SortInfo.OrderBy == SortOrder.Descending ? "DESC" : "ASC");
                        }
                        else
                        {
                            if (parameter.SortInfo.IsAlphaNumeric)
                                queryWithOrderbyClause = SortAplhaNumeric(parameter.SortInfo.ColumnName, parameter.SortInfo.OrderBy);
                            else
                                queryWithOrderbyClause = " ORDER BY " + paging.DefaultOrderByColumn;
                        }
                    }
                }
                return queryWithOrderbyClause.Replace(";", "").Replace("--", "");
            }



            //if (paging.PageNumber < 0)
            //    return string.Empty;
            //else if (paging.PageSize < 0)
            //    return string.Empty;

            //else
            queryWithOrderbyClause = ParseQueryForOrderBy(query, parameter);

            return queryWithOrderbyClause.Replace(";", "").Replace("--", "");
        }
        public string SortAplhaNumeric(string ColName, SortOrder ColOrder)
        {
            string query = $@" ORDER BY LEFT( {ColName},(CASE WHEN (PATINDEX(' %[0 - 9] % ', { ColName})=0) THEN 1 ELSE PATINDEX('%[0-9]%', {ColName}) END)-1)  {(ColOrder == SortOrder.Descending ? "DESC" : "ASC")},
                      CONVERT(INT, (
                          CASE WHEN (PATINDEX('%[^0-9]%', SUBSTRING({ ColName}, PATINDEX('%[0-9]%', {ColName}), LEN({ColName})))>0)
						       THEN 0
							   ELSE SUBSTRING({ColName}, PATINDEX('%[0-9]%', {ColName}), LEN({ ColName}))
						  END
					    )
				       )  {(ColOrder == SortOrder.Descending ? "DESC" : "ASC")}";
            return query;
        }
        public bool CheckEntityExists<T>(string selectColumn, string whereColumn)
        {
            var result = new MSSQLFactory().ExecuteScalar<T>($"SELECT {selectColumn} FROM {typeof(T).Name} WHERE {whereColumn}='{whereColumn}'");

            return (result == null);
        }

        public Settings GetSettings(Enums.Module grievance)
        {
            return new MSSQLFactory().GetObject<Settings>(new GeneralSQLQueries().GetSettings(grievance));
        }

        private string ParseQueryForOrderBy(string query, Parameters parameter)
        {
            string querySegment = string.Empty;
            var paging = parameter.PageInfo;
            if (paging == null)
                paging = new Paging() { PageSize = 10, PageNumber = 1 };

            if (Regex.IsMatch(query.ToUpper(), @"ORDER\s*BY"))
            { }//    return string.Empty;
            else
            {
                if (parameter.SortInfo != null && parameter.SortInfo.ColumnName != null)
                {
                    if (parameter.SortInfo.IsAlphaNumeric)
                        querySegment += SortAplhaNumeric(parameter.SortInfo.ColumnName, parameter.SortInfo.OrderBy);
                    else
                        querySegment += " ORDER BY " + parameter.SortInfo.ColumnName + " " + (parameter.SortInfo.OrderBy == SortOrder.Descending ? "DESC" : "ASC");
                }
                else
                    querySegment += " ORDER BY " + paging.DefaultOrderByColumn;
            }

            paging.PageNumber = paging.PageNumber - 1;
            if (paging.PageSize == -1)
            {
                paging.PageSize = paging.PageSize;
                paging.PageNumber = paging.PageNumber;
            }
            if (paging.PageSize >= 0 && paging.PageNumber >= 0)
            {
                querySegment += " OFFSET " + (paging.PageNumber * paging.PageSize).ToString() + " ROWS";
                querySegment += " FETCH NEXT " + (paging.PageSize).ToString() + " ROWS ONLY";
            }
            return querySegment.Replace(";", "").Replace("--", "").ToLower().Replace("waitfor", "").Replace("delay", "");
        }

        public int UpdateReEvaluationFormCount(int formCount, bool IsReEvaluation)
        {
            return new MSSQLFactory().ExecuteNonQuery(new ReEvaluationSQLQueries().UpdateReEvaluationFormCount(formCount, IsReEvaluation));
        }

        public FilterHelper GetWhereClause<T>(List<SearchFilter> filters)
        {
            SqlCommand command = new SqlCommand(); string whereClause = string.Empty; string whereSegment = string.Empty; string endSiblingSegment = string.Empty;
            string startSiblingSegment = string.Empty; bool firstFilter = true; var modelProperties = typeof(T).GetProperties().ToList(); List<string> filtersToBeCreated = new List<string>();
            List<PropertyInfo> dependentModelProperties = new List<PropertyInfo>();

            foreach (PropertyInfo property in modelProperties)
            {
                if (property.PropertyType.IsGenericType)
                {
                    Type t = property.PropertyType;
                    Type[] typeParameters = t.GetGenericArguments();

                    foreach (Type tParam in typeParameters)
                        dependentModelProperties.AddRange(tParam.GetProperties().ToList());
                }

            }
            if (filters.IsNotNullOrEmpty())
            {
                int Count = 0;
                whereClause = " WHERE ";
                foreach (var filter in filters)
                {
                    var modelPropertyMatch = modelProperties.FirstOrDefault(propertyName => propertyName.Name.ToUpper() == filter.Column.ToUpper());

                    if (modelPropertyMatch == null)
                    {
                        if (dependentModelProperties != null)
                        {
                            modelPropertyMatch = dependentModelProperties.FirstOrDefault(propertyName => propertyName.Name.ToUpper() == filter.Column.ToUpper());
                            if (modelPropertyMatch != null)
                            {
                                if (filtersToBeCreated.Contains(filter.Column.Trim().ToLower()))
                                { Count++; }
                                string countStr = Count > 0 ? Count.ToString() : "";
                                filtersToBeCreated.Add(filter.Column.Trim().ToLower());

                                CreateSegment(filter, ref whereSegment, ref whereClause, modelPropertyMatch, ref command, ref firstFilter, countStr);

                            }
                        }
                        else
                            continue;
                    }
                    else
                    {
                        if (filtersToBeCreated.Contains(filter.Column.Trim().ToLower()))
                        { Count++; }
                        filtersToBeCreated.Add(filter.Column.Trim().ToLower());
                        string countStr = Count > 0 ? Count.ToString() : "";
                        filtersToBeCreated?.Add(filter.Column.Trim().ToLower());
                        CreateSegment(filter, ref whereSegment, ref whereClause, modelPropertyMatch, ref command, ref firstFilter, countStr);

                    }
                }
            }
            return new FilterHelper() { Command = command, WhereClause = whereClause + whereSegment };
        }

        private void CreateSegment(SearchFilter filter, ref string whereSegment, ref string whereClause, PropertyInfo modelPropertyMatch, ref SqlCommand command, ref bool firstFilter, string countStr)
        {
            bool isDateValue = false;

            var DbColumnAttribute = modelPropertyMatch.GetCustomAttributes(typeof(DBColumnNameAttribute), false).Cast<DBColumnNameAttribute>().FirstOrDefault();

            string columnName = DbColumnAttribute == null ? modelPropertyMatch.Name : DbColumnAttribute.name;

            if (!string.IsNullOrWhiteSpace(filter.TableAlias))
                columnName = filter.TableAlias + "." + columnName;

            if (modelPropertyMatch != null && modelPropertyMatch.PropertyType == typeof(DateTime))
            {
                isDateValue = true;
                DateTime date = DateTime.MinValue;
                if (DateTime.TryParse(filter.Value, out date))
                    filter.Value = date.ToString("yyyy/MM/dd");
            }

            switch (filter.Operator)
            {
                case SQLOperator.EqualTo:
                case SQLOperator.NotEqualTo:
                case SQLOperator.GreaterThan:
                case SQLOperator.LessThan:
                case SQLOperator.LessThanEqualTo:
                case SQLOperator.GreaterThanEqualTo:
                    filter.Value = (isDateValue == false) ? filter.Value : filter.Value;
                    break;

                case SQLOperator.Like:
                    filter.Value = filter.Value.Replace(";", "").Replace("--", "").ToLower().Replace("waitfor", "").Replace("delay","") + "%";
                    break;

                case SQLOperator.In:
                case SQLOperator.NotIn:
                    filter.Value = " ( " + filter.Value.ToQuotedCSV().Replace(";", "").Replace("--", "").ToLower().Replace("waitfor", "").Replace("delay", "") + " ) ";
                    break;
                case SQLOperator.Contains:
                    filter.Value = "%" + filter.Value.Replace(";", "").Replace("--", "").ToLower().Replace("waitfor", "").Replace("delay", "") + "%";
                    break;
                case SQLOperator.ISNULL:
                    filter.Value = "NULL Value";
                    break;
                case SQLOperator.ISNotNULL:
                    filter.Value = "NULL Value";
                    break;
                default:
                    break;
            }

            if (filter.Operator == SQLOperator.In || filter.Operator == SQLOperator.NotIn)
                whereSegment += $"{filter.GroupOperation.GetEnumDescription()} {columnName} {filter.Operator.GetEnumDescription()} { filter.Value} ";
            else
            {
                command.Parameters.AddWithValue("@" + (columnName + countStr).Replace(".", ""), filter.Value);

                if (firstFilter)
                    whereSegment += " ";
                else
                    whereClause += filter.GroupOperation.GetEnumDescription();

                var condition = "";
                if (isDateValue)
                {
                    condition = "( " + $"CAST({columnName} AS DATE)" + filter.Operator.GetEnumDescription();
                }
                else
                    condition = "( " + columnName + filter.Operator.GetEnumDescription();
                if (filter.Value != "NULL Value")
                {
                    condition = condition + "@" + (columnName + countStr).Replace(".", "");
                }
                condition = condition + " )";
                if (filter.IsSibling)
                {
                    var regex = new Regex(Regex.Escape("("));
                    condition = regex.Replace(condition, " ", 1);

                    var regex2 = new Regex(Regex.Escape(")"));
                    whereClause = regex2.Replace(whereClause, " ", 1, whereClause.Length - 6);
                }
                whereClause += condition;
                firstFilter = false;
            }
        }

        public string GetSemesterTableName(string semesterName, Programme programme, bool isHistory = false)
        {
            string pprogramme = programme.ToString();
            string semester = semesterName.Replace(pprogramme, "");
            switch (programme)
            {

                case Programme.IG:
                case Programme.HS:
                case Programme.Engineering:
                case Programme.Professional:
                    pprogramme = "IH";
                    break;

            }
            string tableName = $"{pprogramme}_Semester{semester}";

            return isHistory ? tableName + "_History" : tableName;
        }
        public Programme GetProgramme(PrintProgramme printProgramme)
        {
            Programme programme = Programme.UG;
            switch (printProgramme)
            {
                case PrintProgramme.BED:
                case PrintProgramme.UG:
                    programme = Programme.UG;
                    break;
                case PrintProgramme.PG:
                    programme = Programme.PG;
                    break;
                case PrintProgramme.IH:
                    programme = Programme.IG;
                    break;

            }
            return programme;
        }
        public PrintProgramme GetPrintProgramme(Programme programme)
        {
            PrintProgramme pprogramme = PrintProgramme.UG;
            switch (programme)
            {
                case Programme.UG:
                    pprogramme = PrintProgramme.UG;
                    break;
                case Programme.PG:
                    pprogramme = PrintProgramme.PG;
                    break;
                default:
                    pprogramme = PrintProgramme.IH;
                    break;
            }
            return pprogramme;
        }
        public string GetProgrammePostFix(PrintProgramme printProgramme)
        {
            string postfix = "_UG";
            switch (printProgramme)
            {
                case PrintProgramme.BED:
                case PrintProgramme.UG:
                    postfix = "_UG";
                    break;
                case PrintProgramme.PG:
                    postfix = "_PG";
                    break;
                case PrintProgramme.IH:
                    postfix = "_IH";
                    break;

            }
            return postfix;
        }
        public string GetProgrammePFix(Programme Programme)
        {
            string postfix = "UG";
            switch (Programme)
            {
                case Programme.UG:
                    postfix = "UG";
                    break;
                case Programme.PG:
                    postfix = "PG";
                    break;
                case Programme.HS:
                case Programme.IG:
                case Programme.Engineering:
                case Programme.Professional:
                    postfix = "IH";
                    break;

            }
            return postfix;
        }
        public string GetProgrammePostFix(Programme Programme)
        {
            string postfix = "_UG";
            switch (Programme)
            {
                case Programme.UG:
                    postfix = "_UG";
                    break;
                case Programme.PG:
                    postfix = "_PG";
                    break;
                case Programme.HS:
                case Programme.IG:
                case Programme.Engineering:
                case Programme.Professional:
                    postfix = "_IH";
                    break;

            }
            return postfix;
        }
        public string GetPrintProgrammeFix(PrintProgramme PProgramme)
        {
            string postfix = "UG";
            switch (PProgramme)
            {
                case PrintProgramme.UG:
                case PrintProgramme.BED:
                    postfix = "UG";
                    break;
                case PrintProgramme.PG:
                    postfix = "PG";
                    break;
                case PrintProgramme.IH:
                    postfix = "IH";
                    break;

            }
            return postfix;
        }
        public string GetTableName(string semester, Programme programme)
        {
            string pprogramme = programme.ToString();
            switch (programme)
            {

                case Programme.IG:
                case Programme.HS:
                case Programme.Engineering:
                case Programme.Professional:
                    pprogramme = "IH";
                    break;

            }
            string tableName = $"{pprogramme.ToString()}_Semester{semester}";
            return tableName;
        }
        public string GetTableName(string semester, PrintProgramme programme)
        {
            string pprogramme = programme.ToString();
            switch (programme)
            {

                case PrintProgramme.BED:
                    pprogramme = "UG";
                    break;
            }
            string tableName = $"{pprogramme.ToString()}_Semester{semester}";
            return tableName;
        }
        public string GetTableName(PrintProgramme printProgramme, Enums.Module tableType)
        {
            string moduleTableName = string.Empty;
            string tableSuffix = string.Empty;

            switch (tableType)
            {
                case Enums.Module.AdditionalSubjects:
                    moduleTableName = "ARGStudentAdditionalSubjects";
                    break;

                case Enums.Module.PersonalInformation:
                    moduleTableName = "ARGPersonalInformation";
                    break;

                case Enums.Module.ExamForm:
                    moduleTableName = "ARGStudentExamForm";
                    break;

                case Enums.Module.Address:
                    moduleTableName = "ARGStudentAddress";
                    break;

                case Enums.Module.PreviousQualification:
                    moduleTableName = "ARGStudentPreviousQualifications";
                    break;

                case Enums.Module.SelectedCombination:
                    moduleTableName = "ARGSelectedCombination";
                    break;

                case Enums.Module.CoursesApplied:
                    moduleTableName = "ARGCoursesApplied";
                    break;

                case Enums.Module.Result:
                    moduleTableName = "Semester";
                    break;

                case Enums.Module.ReEvaluation:
                case Enums.Module.ExaminationPayment:
                    moduleTableName = "PaymentDetails";
                    break;
            }

            switch (printProgramme)
            {
                case PrintProgramme.BED:
                    tableSuffix = "_UG";
                    break;
                case PrintProgramme.IH:
                    tableSuffix = "_IH";
                    break;

                case PrintProgramme.UG:
                    tableSuffix = "_UG";
                    break;

                case PrintProgramme.PG:
                    tableSuffix = "_PG";
                    break;
            }

            return tableType == Enums.Module.Result ? tableSuffix.Replace("_", "") + "_" + moduleTableName : moduleTableName + tableSuffix;
        }

        public static string GetStringValue(Enum value)
        {
            string output = null;
            Type type = value.GetType();
            FieldInfo fi = type.GetField(value.ToString());
            StringValueAttribute[] attrs =
               fi.GetCustomAttributes(typeof(StringValueAttribute),
                                       false) as StringValueAttribute[];
            if (attrs.Length > 0)
                output = attrs[0].Value;

            return output;
        }

        public ExaminationCourseCategory GetExaminationCourseCategory(PrintProgramme printProgramme)
        {
            ExaminationCourseCategory courseCategory = ExaminationCourseCategory.UG;

            switch (printProgramme)
            {
                case PrintProgramme.UG:
                    if (AppUserHelper.CollegeCode.ToUpper().Equals("IASE"))
                        courseCategory = ExaminationCourseCategory.BED;
                    else
                        courseCategory = ExaminationCourseCategory.UG;
                    break;
                case PrintProgramme.PG:
                    if (AppUserHelper.CollegeCode.ToUpper().Equals("IASE"))
                    {
                        if (new StudentDB().GetStudentCurrentCourse().Equals(Guid.Parse("CBCED3EF-AFE6-4C23-BD67-6C0A78B1EEA1")))
                            courseCategory = ExaminationCourseCategory.PG;
                        else
                            courseCategory = ExaminationCourseCategory.MED;
                    }
                    else
                        courseCategory = ExaminationCourseCategory.PG;
                    break;
                case PrintProgramme.IH:
                    if (AppUserHelper.CollegeCode.ToUpper().Equals("GCET"))
                        courseCategory = ExaminationCourseCategory.ENG;
                    else if (AppUserHelper.CollegeCode.ToUpper().Equals("IASE"))
                        courseCategory = ExaminationCourseCategory.IBM;
                    else
                        courseCategory = ExaminationCourseCategory.IH;
                    break;
                case PrintProgramme.BED:
                    courseCategory = ExaminationCourseCategory.BED;
                    break;
                default:
                    break;
            }
            return courseCategory;
        }

        public PrintProgramme GetPrintProgrammeForExaminationCourseCategory(ExaminationCourseCategory courseCategory)
        {
            PrintProgramme programme = PrintProgramme.UG;

            switch (courseCategory)
            {
                case ExaminationCourseCategory.UG:
                    programme = PrintProgramme.UG;
                    break;
                case ExaminationCourseCategory.BED:
                    programme = PrintProgramme.BED;
                    break;
                case ExaminationCourseCategory.PG:
                    programme = PrintProgramme.PG;
                    break;
                case ExaminationCourseCategory.MED:
                    programme = PrintProgramme.PG;
                    break;
                case ExaminationCourseCategory.IH:
                    programme = PrintProgramme.IH;
                    break;
                case ExaminationCourseCategory.ENG:
                    programme = PrintProgramme.IH;
                    break;
                default:
                    break;
            }

            return programme;
        }

        public string GetColumnsForSelectList<T>(string TableAlias = null)
        {
            var modelProperties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();

            return string.Join(",", modelProperties.Where(property => property.GetCustomAttributes(typeof(IgnoreDBReadAttribute), false).Count() == 0 ||
                                    property.GetCustomAttributes(typeof(IgnoreDBWriterAttribute), false).Count() == 0).ToList().Select(p => TableAlias.IsNullOrEmpty() ? "" : TableAlias + "." + p.Name)
                                    );
        }

        public static string GetSubjectTypeCode(SubjectType subjectType)
        {
            switch (subjectType)
            {
                case SubjectType.Core:
                    return "C";
                case SubjectType.SEC:
                    return "SEC";
                case SubjectType.GE:
                    return "GE";
                case SubjectType.MIL:
                    return "";
                case SubjectType.AE:
                    return "";
                case SubjectType.OE:
                    return "OE";
                case SubjectType.DSE:
                    return "DSE";
                case SubjectType.DCE:
                    return "DCE";
                case SubjectType.OC:
                    return "C";
                case SubjectType.Lab:
                    return "";
                case SubjectType.Practical:
                    return "";
                case SubjectType.Workshop:
                    return "";
                default:
                    return "";
            }
        }

    }
}
