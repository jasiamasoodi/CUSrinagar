using CUSrinagar.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using GeneralModels;
using CUSrinagar.Enums;

namespace CUSrinagar.DataManagers
{
    public class SerializeFilter
    {
        public FilterHelper GetWhereClauseFromFilter(string filters)
        {
            FilterHelper helper = new FilterHelper();
            string whereClause = " WHERE  1 = 1 ";
            if (!string.IsNullOrWhiteSpace(filters))
            {
                helper = GetSQLWhereClauseFromJson(filters);

                helper.WhereClause = whereClause+" AND ( " + helper.WhereClause + " ) ";

            }
            return helper;

        }

        /// <summary>
        /// This method returns SQL corresponding to json search filter
        /// </summary>
        /// <param name="jsonString">json search filter</param>
        /// <returns>SQL where clause is returned</returns>
        private FilterHelper GetSQLWhereClauseFromJson(string jsonString)
        {
            FilterHelper helper = new FilterHelper();
            string whereClause = " 1 = 1 ";

            if (!string.IsNullOrWhiteSpace(jsonString))
            {
                string commandParameterName = string.Empty;
                Filters searchFilterObject = JsonConvert.DeserializeObject<Filters>(jsonString);

                if (searchFilterObject != null)
                {
                    LogicalOperator groupOpp = searchFilterObject.GroupOperation;

                    if (searchFilterObject.GroupOperation.ToString().ToLower() == "and")
                        whereClause = " 1 = 1 ";
                    else
                        whereClause = " 1 = 2 ";

                    foreach (var filter in searchFilterObject.FilterList)
                    {
                        if (filter != null)
                        {
                            short index = 1;
                            if (!string.IsNullOrWhiteSpace(filter.Value + ""))
                            {
                                string leftExpression = "";
                                string columnName = "";
                                if (filter.Column.Contains("["))
                                    columnName = " " + filter.Column + " ";
                                //leftexpression = groupOpp + " " + rule.field;

                                else
                                    columnName = " [" + filter.Column + "] ";
                                //leftexpression = groupOpp + " [" + rule.field + "] ";

                                leftExpression = groupOpp + columnName;
                                string rightexpression = commandParameterName;

                                if (IsSQLInjection(filter.Value))
                                    continue;
                                else
                                {
                                    if (filter.Operator.ToLower() == "in" || filter.Operator.ToLower() == "ni")
                                        filter.Value = ((string)filter.Value).Replace(",'", "', '");

                                    commandParameterName = "@" + filter.Column + index;
                                    helper.Command.Parameters.AddWithValue(commandParameterName, filter.Value);
                                }

                                switch (filter.Operator)
                                {

                                    case "eq":

                                        whereClause += leftExpression + " = " + commandParameterName + " ";

                                        break;

                                    case "gt":

                                        whereClause += leftExpression + " > " + commandParameterName + " ";

                                        break;

                                    case "ge":

                                        whereClause += leftExpression + " >= " + commandParameterName + " ";

                                        break;

                                    case "lt":

                                        whereClause += leftExpression + " < " + commandParameterName + " ";
                                        break;

                                    case "le":
                                        whereClause += leftExpression + " <= " + commandParameterName + " ";
                                        break;

                                    //case "cn":
                                    //    whereClause += groupOpp + " [" + rule.field + "] LIKE '%" + rule.data + "%' ";
                                    //    break;

                                    case "nc":

                                        whereClause += leftExpression + " NOT LIKE %" + commandParameterName + " " + "% ";
                                        break;

                                    case "nu":

                                        whereClause += groupOpp + " ( " + columnName + " IS NULL OR LTRIM(RTRIM(" + columnName + ")) = 'NULL' OR LTRIM(RTRIM(" + columnName + ")) = '') ";
                                        break;

                                    case "nn":
                                        //whereClause += leftexpression + " IS NOT NULL ";
                                        whereClause += groupOpp + " NOT( " + columnName + " IS NULL OR LTRIM(RTRIM(" + columnName + ")) = 'NULL' OR LTRIM(RTRIM(" + columnName + ")) = '') ";
                                        break;

                                    case "bw":
                                        whereClause += leftExpression + " LIKE " + commandParameterName + " " + "% ";
                                        break;

                                    case "bn":

                                        whereClause += leftExpression + " NOT LIKE " + commandParameterName + " " + "% ";
                                        break;

                                    case "ew":

                                        whereClause += leftExpression + " LIKE %" + commandParameterName + " " + " ";
                                        break;

                                    case "ne":
                                    case "neq":
                                        whereClause += leftExpression + " !=" + commandParameterName + " " + "";
                                        break;

                                    case "in":
                                        whereClause += leftExpression + " IN (" + commandParameterName + " " + ") ";
                                        break;
                                    case "ni":
                                        string dataNotIn = (string)filter.Value;
                                        dataNotIn = dataNotIn.Replace(",'", "', '");
                                        whereClause += leftExpression + " NOT IN (" + commandParameterName + " " + ") ";
                                        break;
                                    case "cn":
                                    default:
                                        //Searching for contains condition
                                        whereClause += leftExpression + " LIKE %" + commandParameterName + " " + "%";
                                        break;

                                }
                            }
                            index++;
                        }
                    }
                }
            }
            helper.WhereClause = whereClause;
            return helper;
        }

        private bool IsSQLInjection(object data)
        {
            string CheckString = string.Empty;
            bool isSQLInjection = false;

            string[] sqlCheckList = { "--", ";--", ";", "/*", "*/", "@@", "@", "char", "nchar", "varchar", "nvarchar", "alter", "begin", "cast", "create", "cursor", "declare", "delete", "drop", "end", "exec", "execute", "fetch", "insert", "kill", "select", "sys", "sysobjects", "syscolumns", "table", "update" };

            if (data is string)
            {
                CheckString = ((string)data).Replace("'", "''");
                for (int i = 0; i <= sqlCheckList.Length - 1; i++)
                {
                    if ((CheckString.IndexOf(sqlCheckList[i], StringComparison.OrdinalIgnoreCase) >= 0))
                        isSQLInjection = true;
                }
            }
            return isSQLInjection;
        }
    }
}

