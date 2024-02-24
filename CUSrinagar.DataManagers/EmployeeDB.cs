using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using CUSrinagar.DataManagers.SQLQueries;
using CUSrinagar.Models;
using GeneralModels;
using Terex;

namespace CUSrinagar.DataManagers
{
    public class EmployeeDB
    {
        public List<Employee> GetEmployeeList(Parameters parameters)
        {
            return new MSSQLFactory().GetObjectList<Employee>(new EmployeeSQLQueries().GetEmployeeList(parameters));
        }

        public int CreateEmployee(Employee employee)
        {
            return new MSSQLFactory().InsertRecord(employee, "Employees", null);
        }

        public int UpdateEmployee(Employee employee)
        {
            return new MSSQLFactory().ExecuteNonQuery(new MSSQLFactory().UpdateRecord(employee, null, null, "Employees"));
        }

        public int SaveEmployeeDepartment(Guid employee_id, Guid department)
        {
            return new MSSQLFactory().ExecuteNonQuery($"INSERT into EmployeeDepartment Values ('{employee_id}','{department}')");
        }

        public List<SelectListItem> GetEmployeeDepartments(Parameters parameters)
        {
            return new MSSQLFactory().GetObjectList<SelectListItem>(new EmployeeSQLQueries().GetEmployeeDepartments(parameters));
        }
        public List<SelectListItem> GetDepartments()
        {
            return new MSSQLFactory().GetObjectList<SelectListItem>(new EmployeeSQLQueries().GetDepartments());
        }
    }
}
