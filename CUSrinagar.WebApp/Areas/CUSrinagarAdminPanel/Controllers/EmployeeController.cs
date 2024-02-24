using CUSrinagar.BusinessManagers;
using CUSrinagar.Enums;
using CUSrinagar.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CUSrinagar.Models;
using GeneralModels;
using CUSrinagar.Extensions;

namespace CUSrinagar.WebApp.CUSrinagarAdminPanel.Controllers
{
    [OAuthorize(AppRoles.University,AppRoles.University_Salary, AppRoles.University_CAO)]
    public class EmployeeController : Controller
    {

        // GET: CUSrinagarAdminPanel/Employee
        public ActionResult List()
        {
            ViewBag.Departments = new EmployeeManager().GetEmployeeDepartments(null);
            return View();
        }


        // GET: CUSrinagarAdminPanel/Employee
        public ActionResult EmployeeListPartial(Parameters parameter)
        {
            return View( new EmployeeManager().List(parameter));
        }

        // GET: CUSrinagarAdminPanel/Employee/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: CUSrinagarAdminPanel/Employee/Create
        public ActionResult Create()
        {
            ViewBag.EmployeeTypes= Helper.GetSelectList<EmployeeType>();
            ViewBag.Departments = new EmployeeManager().GetEmployeeDepartments(null);
            return View();
        }

        // POST: CUSrinagarAdminPanel/Employee/Create
        [HttpPost]
        public ActionResult Create(Employee employee)
        {
            ResponseData response = new ResponseData();            
            if (!string.IsNullOrWhiteSpace(employee.EnteredDOB))
            {
                employee.DOB = new RegistrationManager().ConvertEnterdDOBToDateTime(employee.EnteredDOB);
                TryUpdateModel(employee);
            }

            if (ModelState.IsValid)
                response = new EmployeeManager().Create(employee);
            else
                response.IsSuccess = false;

            ViewBag.Departments = new EmployeeManager().GetEmployeeDepartments(null);
            ViewBag.EmployeeTypes = Helper.GetSelectList<EmployeeType>();
            ViewBag.Response = response;
            return View();
        }

        // GET: CUSrinagarAdminPanel/Employee/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: CUSrinagarAdminPanel/Employee/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: CUSrinagarAdminPanel/Employee/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: CUSrinagarAdminPanel/Employee/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
