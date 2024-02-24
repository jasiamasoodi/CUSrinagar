using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CUSrinagar.Models;
using GeneralModels;

namespace CUSrinagar.DataManagers.SQLQueries
{
    public class SalarySQLQueries
    {
        internal SqlCommand GetEmployeeSalary(Guid employee_Id)
        {
            string query = @" SELECT * from SalaryBill 
                                 Right JOIN Employees
                                ON Employee_ID = Emp_ID WHERE Employee_ID = @Emp_ID";
            SqlCommand command = new SqlCommand();
            command.Parameters.AddWithValue("@Emp_ID", employee_Id);
            command.CommandText = query;
            return command;
        }
        internal SqlCommand GetEmployeeSalaryList()
        {
            SqlCommand command = new SqlCommand();
            string query = @"SELECT * FROM SalaryBill Right JOIN Employees
                                ON Employee_ID = Emp_ID order by BasicPay Desc";
            command.CommandText = query;
            return command;
        }

        internal SqlCommand FetchSalaryBill()
        {
            string query = @" SELECT * from SalaryCalculationColumns";
            SqlCommand command = new SqlCommand();
            command.CommandText = query;
            return command;
        }

        internal SqlCommand FetchArchiveSalaryList(Parameters parameters)
        {
            string query = $@"    SELECT
                                        * from SalaryBillArchive 
                                  JOIN Employees
                                ON Employee_ID = Emp_ID ";
            SqlCommand command = new SqlCommand();
            FilterHelper helper = new GeneralFunctions().GetWhereClause<SalaryBillArchive>(parameters.Filters);
            query += helper.WhereClause;
            query += new GeneralFunctions().GetPagedQuery(query, parameters);
            helper.Command.CommandText = query;
            return helper.Command;
        }

        internal SqlCommand FetchSalaryList(Parameters parameters)
        {

            string query = $@"  SELECT
                                        * from SalaryBill 
                                  JOIN Employees
                                ON Employee_ID = Emp_ID";
            SqlCommand command = new SqlCommand();
            FilterHelper helper = new GeneralFunctions().GetWhereClause<SalaryBill>(parameters.Filters);
            query += helper.WhereClause;
            query += new GeneralFunctions().GetPagedQuery(query, parameters);
            helper.Command.CommandText = query;
            return helper.Command;
        }
    }
}
