using System.Data.SqlClient;
using CUSrinagar.Models;
using GeneralModels;

namespace CUSrinagar.DataManagers.SQLQueries
{
    public class EmployeeSQLQueries
    {
        internal SqlCommand GetEmployeeList(Parameters parameters)
        {
            FilterHelper helper = null;
            string selectColumns = new GeneralFunctions().GetColumnsForSelectList<Employee>("Employees");

            SqlCommand command = new SqlCommand($@" SELECT Employees.Employee_ID, Employees.EmployeeID, Employees.EmployeeFullName, Employees.Phone, Employees.Email, EmployeeDepartment.Department_ID, Employees.DOB, Employees.Status, Employees.FathersName, Employees.MothersName,
                                                            Employees.PermanentAddress, Employees.PinCode, Employees.PresentAddress, Employees.EmployeeAttendance_ID,EmployeeDepartment.DepartmentName FROM Employees
                                                        Left JOIN EmployeeDepartment ON EmployeeDepartment.Department_ID = Employees.Department_ID ");

            if (parameters != null && parameters.Filters != null)
            {
                helper = new GeneralFunctions().GetWhereClause<Employee>(parameters.Filters);
                helper.Command.CommandText = command.CommandText + helper.WhereClause;
                command = helper.Command;
            }

            return command;
        }

        internal SqlCommand GetEmployeeDepartments(Parameters parameters)
        {
            SqlCommand command = new SqlCommand(@" SELECT CAST(Department_ID AS VARCHAR(40)) Value, DepartmentName Text FROM EmployeeDepartment");
            if (parameters != null && parameters.Filters != null)
                command.CommandText += new GeneralFunctions().GetWhereClause<Employee>(parameters.Filters).WhereClause;
            return command;

        }
        internal SqlCommand GetDepartments()
        {
            SqlCommand command = new SqlCommand(@" SELECT DepartmentName Value, DepartmentName Text FROM EmployeeDepartment");           
            return command;

        }
    }
}
