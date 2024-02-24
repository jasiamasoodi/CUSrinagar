// ***********************************************************************
// Assembly         : MSSQLFactory
// Author           : Shahid Ali Wani
// Created          : March-2017
//
// Last Modified By : Shahid Ali Wani
// Last Modified On : 6-March-2017
// ***********************************************************************
// <copyright file="MSSQLFactory.cs" company="KampusSide">
//     Copyright (c) KampusSide Corporation Inc. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using CUSrinagar.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;


namespace Terex
{
    public static class CUSDBConnection
    {
        public static string DBConnectionString { get { return ConfigurationManager.ConnectionStrings["CUDefaultConnection"].ConnectionString; } }
    }
    /// <summary>
    /// Terex.MSSQLFactory is responsible all MSSQL things
    /// </summary>
    public class MSSQLFactory
    {
        public int ExecuteNonQuery(string TSQLQueryText)
        {
            var queryText = new SqlCommand(TSQLQueryText);
            queryText.CommandType = CommandType.Text;
            return ExecuteNonQuery(queryText);
        }
        public int ExecuteNonQuery(SqlCommand TSQLQuery)
        {
            int _result = 0;
            using (SqlConnection con = new SqlConnection(CUSDBConnection.DBConnectionString))
            {
                TSQLQuery.Connection = con;
                con.Open();
                _result = TSQLQuery.ExecuteNonQuery();
                TSQLQuery.Dispose();
            }
            return _result;
        }

        public T ExecuteScalar<T>(string TSQLQueryText)
        {
            SqlCommand TSQLQuery = new SqlCommand(TSQLQueryText);
            TSQLQuery.CommandType = CommandType.Text;
            return ExecuteScalar<T>(TSQLQuery);
        }
        public T ExecuteScalar<T>(SqlCommand TSQLQuery)
        {
            T value = default(T);
            using (SqlConnection con = new SqlConnection(CUSDBConnection.DBConnectionString))
            {
                TSQLQuery.Connection = con;
                con.Open();
                var val = TSQLQuery.ExecuteScalar();
                if (val != DBNull.Value && val != null)
                {
                    value = (T)val;
                }
                TSQLQuery.Dispose();
            }
            return value;
        }


        public T GetObject<T>(string TSQLQueryText) where T : class
        {
            SqlCommand TSQLQuery = new SqlCommand(TSQLQueryText);
            TSQLQuery.CommandType = CommandType.Text;
            return GetObject<T>(TSQLQuery);
        }

        public T GetObject<T>(SqlCommand TSQLQuery) where T : class
        {
            T model = null;
            using (SqlConnection con = new SqlConnection(CUSDBConnection.DBConnectionString))
            {
                TSQLQuery.Connection = con;
                con.Open();
                using (SqlDataReader _dataReader = TSQLQuery.ExecuteReader())
                {
                    if (_dataReader.HasRows)
                    {
                        _dataReader.Read();
                        PropertyInfo[] _properties = GetModelProperties(typeof(T)).Where(x => x.IsDefined(typeof(IgnoreDBReadAttribute)) == false).ToArray();
                        var modelPropertiesToBeFilled = _properties.Where(property => GetDataReaderColumns(_dataReader).Contains(property.Name.ToLower())).ToList();
                        model = FillObject<T>(_dataReader, modelPropertiesToBeFilled);
                    }
                }
            }
            return model;
        }


        public List<T> GetObjectList<T>(string TSQLQueryText) where T : class
        {
            SqlCommand TSQLQuery = new SqlCommand(TSQLQueryText);
            TSQLQuery.CommandType = CommandType.Text;
            return GetObjectList<T>(TSQLQuery);
        }


        public List<T> GetObjectList<T>(SqlCommand TSQLQuery) where T : class
        {
            List<T> modelList = null;
            using (SqlConnection con = new SqlConnection(CUSDBConnection.DBConnectionString))
            {
                TSQLQuery.Connection = con;
                con.Open();
                using (SqlDataReader _dataReader = TSQLQuery.ExecuteReader())
                {
                    if (_dataReader.HasRows)
                    {
                        var columnsInDataReader = GetDataReaderColumns(_dataReader);
                        modelList = (List<T>)Activator.CreateInstance(typeof(List<T>));

                        PropertyInfo[] _properties = GetModelProperties(typeof(T)).Where(x => x.IsDefined(typeof(IgnoreDBReadAttribute)) == false).ToArray();
                        var modelPropertiesToBeFilled = _properties.Where(property => columnsInDataReader.Contains(property.Name.ToLower())).ToList();

                        while (_dataReader.Read())
                        {
                            modelList.Add(FillObject<T>(_dataReader, modelPropertiesToBeFilled));
                        }
                    }
                }
            }
            return modelList;
        }


        public List<T> GetSingleValues<T>(string TSQLQueryText)
        {
            SqlCommand TSQLQuery = new SqlCommand(TSQLQueryText);
            TSQLQuery.CommandType = CommandType.Text;
            return GetSingleValues<T>(TSQLQuery);
        }
        public List<T> GetSingleValues<T>(SqlCommand TSQLQuery)
        {
            if (!(typeof(T).FullName.Contains(nameof(System))) && !(typeof(T).FullName.ToLower().StartsWith("cusrinagar.enums")))
                throw new InvalidConstraintException();

            List<T> model = null;
            using (SqlConnection con = new SqlConnection(CUSDBConnection.DBConnectionString))
            {
                TSQLQuery.Connection = con;
                con.Open();
                using (SqlDataReader _dataReader = TSQLQuery.ExecuteReader())
                {
                    if (_dataReader.HasRows)
                    {
                        model = (List<T>)Activator.CreateInstance(typeof(List<T>));
                        while (_dataReader.Read())
                            model.Add((T)(_dataReader[0]));
                    }
                }
            }
            return model;
        }


        public DataTable GetDataTable(string TSQLQueryText)
        {
            SqlCommand TSQLQuery = new SqlCommand(TSQLQueryText);
            TSQLQuery.CommandType = CommandType.Text;
            return GetDataTable(TSQLQuery);
        }
        public DataTable GetDataTable(SqlCommand TSQLQuery)
        {
            DataTable dataTable = new DataTable();
            using (SqlConnection con = new SqlConnection(CUSDBConnection.DBConnectionString))
            {
                using (SqlDataAdapter _dataAdapter = new SqlDataAdapter(TSQLQuery))
                {
                    TSQLQuery.Connection = con;
                    _dataAdapter.Fill(dataTable);
                    TSQLQuery.Dispose();
                }
            }

            if (dataTable.Rows.Count > 0)
                return dataTable;
            dataTable.Dispose();
            return null;
        }

        public List<T> GetDataTableAsList<T>(SqlCommand TSQLQuery) where T : class
        {
            DataTable dataTable = GetDataTable(TSQLQuery);
            if (dataTable == null) return null;
            return DataTableToList<T>(dataTable);
        }

        List<T> DataTableToList<T>(DataTable dataTable) where T : class
        {
            List<T> modelList = (List<T>)Activator.CreateInstance(typeof(List<T>)); ;
            T model = (T)Activator.CreateInstance(typeof(T));
            Type type = model.GetType();

            foreach (DataRow row in dataTable.Rows)
            {
                model = (T)Activator.CreateInstance(typeof(T));
                foreach (DataColumn column in row.Table.Columns)
                {
                    PropertyInfo property = type.GetProperty(column.ColumnName);
                    if (property != null)
                    {
                        property.SetValue(model, null, null);
                        try
                        {
                            if (!(row[column.ColumnName] == DBNull.Value))
                                property.SetValue(model, row[column.ColumnName]);
                        }
                        catch (ArgumentException)
                        {
                            //find the property type
                            Type propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

                            //Returns an System.Object with the specified System.Type and whose value is
                            //equivalent to the specified object.
                            object val;
                            if (propertyType.BaseType == typeof(Enum))
                                val = Enum.Parse(propertyType, row[column.ColumnName].ToString());
                            else
                                val = Convert.ChangeType(row[column.ColumnName], propertyType);

                            //Set the value of the property
                            property.SetValue(model, val, null);
                        }
                    }
                }
                modelList.Add(model);
            }
            dataTable.Dispose();
            return modelList;
        }

        public DataSet GetDataSet(string TSQLQueryText, List<string> tableNames = null)
        {
            SqlCommand TSQLQuery = new SqlCommand(TSQLQueryText);
            TSQLQuery.CommandType = CommandType.Text;
            return GetDataSet(TSQLQuery, tableNames);
        }
        public DataSet GetDataSet(SqlCommand TSQLQuery, List<string> tableNames = null)
        {
            DataSet dataSet = new DataSet();
            using (SqlConnection con = new SqlConnection(CUSDBConnection.DBConnectionString))
            {
                using (SqlDataAdapter _dataAdapter = new SqlDataAdapter(TSQLQuery))
                {
                    TSQLQuery.Connection = con;
                    _dataAdapter.Fill(dataSet);
                    TSQLQuery.Dispose();
                }
            }

            if (dataSet.Tables.Count > 0)
            {
                if (tableNames?.Count() > 0)
                {
                    for (int index = 0; index < tableNames.Count; index++)
                    {
                        dataSet.Tables[index].TableName = tableNames[index];
                    }
                }
                return dataSet;
            }
            dataSet.Dispose();
            return null;
        }



        public int InsertRecord<T>(T model, string TableName = null, List<string> IgnoreList = null) where T : class
        {
            Tuple<string, string, SqlParameter[]> _tuple = CreateSQLParameters(model, TableName, IgnoreList);
            TableName = string.IsNullOrWhiteSpace(TableName) ? typeof(T).Name : TableName;
            string queryInsert = $"INSERT INTO {TableName} ({_tuple.Item1}) VALUES({_tuple.Item2});";
            SqlCommand _sqlCommand = new SqlCommand(queryInsert);
            _sqlCommand.Parameters.AddRange(_tuple.Item3);
            _sqlCommand.CommandType = CommandType.Text;
            return ExecuteNonQuery(_sqlCommand);
        }
        public Tuple<string, string, SqlParameter[]> CreateSQLParameters<T>(T model, string TableName = null, List<string> IgnoreList = null) where T : class
        {
            TableName = string.IsNullOrWhiteSpace(TableName) ? typeof(T).Name : TableName;
            PropertyInfo[] Properties = GetModelProperties(typeof(T)).Where(x => x.IsDefined(typeof(IgnoreDBWriterAttribute)) == false).ToArray();
            StringBuilder stringSqlColumnNames = new StringBuilder();
            StringBuilder stringSqlColumnValues = new StringBuilder();
            List<SqlParameter> SqlParameters = new List<SqlParameter>();
            object value;
            foreach (PropertyInfo property in Properties)
            {
                if (IgnoreList?.Any(x => x.ToLower().Trim() == property.Name.ToLower().Trim()) ?? false)
                    continue;

                //get property value
                value = property.PropertyType == typeof(bool) ? (bool)property.GetValue(model) ? 1 : 0 : property.GetValue(model);

                if (value == null || string.IsNullOrEmpty(value.ToString()) || (value is DateTime && ((DateTime)value).Date == DateTime.MinValue.Date))
                    value = DBNull.Value;

                SqlParameters.Add(GetSqlParameter(TableName, property.Name, value));
                stringSqlColumnNames.Append($",[{property.Name}]");
                stringSqlColumnValues.Append($",@{property.Name}");
            }
            return Tuple.Create(stringSqlColumnNames.ToString().Substring(1), stringSqlColumnValues.ToString().Substring(1), SqlParameters.ToArray());
        }




        public SqlCommand UpdateRecord<T>(T model, List<string> IgnorePropertiesInQuery, List<string> IgnorePropertiesInParameters, string TableName = null) where T : class
        {
            Tuple<string, SqlParameter[]> _tuple = CreateSQLParameters(model, IgnorePropertiesInQuery, IgnorePropertiesInParameters, TableName);
            TableName = string.IsNullOrWhiteSpace(TableName) ? typeof(T).Name : TableName;
            string queryInsert = $"UPDATE {TableName} SET {_tuple.Item1}";
            SqlCommand _sqlCommand = new SqlCommand(queryInsert);
            _sqlCommand.Parameters.AddRange(_tuple.Item2);
            _sqlCommand.CommandType = CommandType.Text;
            return _sqlCommand;
        }
        public Tuple<string, SqlParameter[]> CreateSQLParameters<T>(T model, List<string> IgnoreInQuery, List<string> IgnoreInParameters, string TableName = null) where T : class
        {
            TableName = string.IsNullOrWhiteSpace(TableName) ? typeof(T).Name : TableName;
            PropertyInfo[] Properties = GetModelProperties(typeof(T)).Where(x => x.IsDefined(typeof(IgnoreDBWriterAttribute)) == false).ToArray();
            StringBuilder stringSqlParameters = new StringBuilder();
            List<SqlParameter> SqlParameters = new List<SqlParameter>();
            object value;
            IgnoreInQuery = IgnoreInQuery?.ConvertAll(x => x.ToLower());
            IgnoreInParameters = IgnoreInParameters?.ConvertAll(x => x.ToLower());
            foreach (PropertyInfo property in Properties)
            {
                //get property value
                value = property.PropertyType == typeof(bool) ? (bool)property.GetValue(model) ? 1 : 0 : property.GetValue(model);

                if (value == null || string.IsNullOrEmpty(value.ToString()) || (value is DateTime && ((DateTime)value).Date == DateTime.MinValue.Date))
                    value = DBNull.Value;

                if (!(IgnoreInParameters?.Contains(property.Name.ToLower())) ?? true)
                {
                    SqlParameters.Add(GetSqlParameter(TableName, property.Name, value));
                }

                if (!(IgnoreInQuery?.Contains(property.Name.ToLower())) ?? true)
                {
                    stringSqlParameters.Append($",[{property.Name}]=@{property.Name}");
                }
            }
            return Tuple.Create(stringSqlParameters.ToString().Substring(1), SqlParameters.ToArray());
        }

        private static T FillObject<T>(SqlDataReader _dataReader, List<PropertyInfo> modelPropertiesToBeFilled) where T : class
        {
            T model = (T)Activator.CreateInstance(typeof(T));

            foreach (var property in modelPropertiesToBeFilled)
            {
                property.SetValue(model, null, null);
                try
                {
                    if (!(_dataReader[property.Name] == DBNull.Value))
                        property.SetValue(model, _dataReader[property.Name]);
                }
                catch (ArgumentException)
                {
                    //find the property type
                    Type propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

                    //Returns an System.Object with the specified System.Type and whose value is
                    //equivalent to the specified object.
                    object val;
                    if (propertyType.BaseType == typeof(Enum))
                        val = Enum.Parse(propertyType, _dataReader[property.Name].ToString());
                    else
                        val = Convert.ChangeType(_dataReader[property.Name], propertyType);

                    //Set the value of the property
                    property.SetValue(model, val, null);
                }

            }
            return model;
        }
        private static bool IsNullableType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>));
        }

        private static List<string> GetDataReaderColumns(SqlDataReader _dataReader)
        {
            return _dataReader.GetSchemaTable().AsEnumerable().Select(x => x["ColumnName"].ToString().ToLower()).ToList();
        }

        private static SqlParameter GetSqlParameter(string tableName, string parameterName, object Value)
        {
            DataRow dr = SchemaDataRows.FirstOrDefault(r => string.Equals(r["TABLE_NAME"].ToString(), tableName, StringComparison.CurrentCultureIgnoreCase)
                                                         && string.Equals(r["COLUMN_NAME"].ToString(), parameterName, StringComparison.CurrentCultureIgnoreCase));
            SqlDbType SqlDbType = (SqlDbType)Enum.Parse(typeof(SqlDbType), dr["DATA_TYPE"].ToString(), true);
            SqlParameter sqlParameter = new SqlParameter($"@{parameterName}", SqlDbType);
            sqlParameter.Value = Value;
            sqlParameter.IsNullable = (dr["IS_NULLABLE"].ToString() != "NO");
            dr = null;
            return sqlParameter;
        }
        public static SqlParameter GetSqlParameter(string parameterName, object value)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()) || (value is DateTime && ((DateTime)value).Date == DateTime.MinValue.Date))
                value = DBNull.Value;
            SqlParameter sqlParameter = new SqlParameter($"@{parameterName}", value);
            return sqlParameter;
        }
        private static PropertyInfo[] GetModelProperties(Type type)
        {
            return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                   .Where(x => !x.PropertyType.FullName.Contains(nameof(System.Collections)) && x.CanWrite).ToArray();
        }

        private static EnumerableRowCollection<DataRow> SchemaDataRows => GetSchemaDataRows();

        private static EnumerableRowCollection<DataRow> GetSchemaDataRows()
        {
            string query = "SELECT TABLE_NAME, COLUMN_NAME, DATA_TYPE, IS_NULLABLE FROM INFORMATION_SCHEMA.COLUMNS;";
            SqlDataAdapter dataAdapter = new SqlDataAdapter(query, CUSDBConnection.DBConnectionString);
            DataTable dt = new DataTable();
            dataAdapter.Fill(dt);
            dataAdapter.Dispose();
            return dt.AsEnumerable();
        }
    }
    public enum SqlOperation
    {
        Insert = 0,
        Update = 1,
        Delete = 2,
        Select = 3,
        Truncate = 4
    }

}