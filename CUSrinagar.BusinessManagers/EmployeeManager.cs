using CUSrinagar.DataManagers;
using CUSrinagar.Models;
using GeneralModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Mvc;

namespace CUSrinagar.BusinessManagers
{
    public class EmployeeManager
    {

        public List<Employee> List(Parameters parameters)
        {
            return new EmployeeDB().GetEmployeeList(parameters);
        }
        public List<SelectListItem> GetDepartments()
        {
            return new EmployeeDB().GetDepartments();
        }

        public ResponseData Create(Employee employee)
        {
            ResponseData response = new ResponseData();
            int NoOfRecordsEffected = 0;
            using (TransactionScope transaction = new TransactionScope())
            {
                employee.Employee_ID = Guid.NewGuid();
                employee.EmployeeID = new Random().Next(1000, 9999).ToString();
               
                NoOfRecordsEffected = new EmployeeDB().CreateEmployee(employee);
                transaction.Complete();
            }
            if(NoOfRecordsEffected > 0)
            {
                response.IsSuccess = true;
                response.SuccessMessage = " Employee Saved Successfully.";
            }
            else
            {
                response.IsSuccess = false;
                response.SuccessMessage = " Employee could not be Saved Successfully.";
            }
            return response;
        }

        public int Update(Employee employee)
        {
            return new EmployeeDB().UpdateEmployee(employee);
        }

        public List<SelectListItem> GetEmployeeDepartments(Parameters parameters)
        {
            return new EmployeeDB().GetEmployeeDepartments(parameters);
        }

       
    }
}
