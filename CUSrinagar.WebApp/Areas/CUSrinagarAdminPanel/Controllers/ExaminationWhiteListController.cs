using CUSrinagar.BusinessManagers;
using CUSrinagar.Enums;
using CUSrinagar.Models;
using CUSrinagar.OAuth;
using GeneralModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CUSrinagar.WebApp.CUSrinagarAdminPanel.Controllers
{
    [OAuthorize(AppRoles.University)]
    public class ExaminationWhiteListController : AdminController
    {
        // GET: CUSrinagarAdminPanel/ExaminationWhiteList
        public ActionResult Applications()
        {
            FillViewBag_College();
            FillViewBag_PrintProgrammes();
            return View();
        }
        public ActionResult ApplicationsList(Parameters parameter)
        {
            var list = new ExaminationWhiteListManager().GetAllApplications(parameter);
            return View(list);
        }

        // GET: CUSrinagarAdminPanel/ExaminationWhiteList/Details/5
        public ActionResult Details(Guid _ID)
        {
            var application = new ExaminationWhiteListManager().GetAllApplications(new Parameters()
            {
                Filters = new List<SearchFilter>() { new SearchFilter()
                {
                    Column = nameof(ExaminationWhiteListCompact.WhiteList_ID),
                    Value = _ID.ToString(),
                    Operator = Enums.SQLOperator.EqualTo
                }
            }
            }).FirstOrDefault();

            return View(application);
        }

        // GET: CUSrinagarAdminPanel/ExaminationWhiteList/Create
        public ActionResult Create()
        {
            ViewBag.Students = new RegistrationManager().GetStudentsSelectListItem().Take(10);
            ViewBag.Users = new UserProfileManager().GetUsersSelectListItem();
            return View();
        }

        [HttpPost]
        public ActionResult Create(ExaminationWhiteListCompact application)
        {
            ViewBag.Users = new UserProfileManager().GetUsersSelectListItem();

            if (ModelState.IsValid)
            {
               int Result=  new ExaminationWhiteListManager().SaveApplication(application);
                if (Result == -1)
                {
                    TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'> Invalid details.</a></div>";
                }
                else
                {
                    TempData["response"] = $"<div class='alert alert-success alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'> Saved successfully</a></div>";
                }
            }
            return View();
        }

        // GET: CUSrinagarAdminPanel/ExaminationWhiteList/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: CUSrinagarAdminPanel/ExaminationWhiteList/Edit/5
        [HttpPost]
        public ActionResult Edit(ExaminationWhiteListCompact application)
        {
            var result = new ExaminationWhiteListManager().EditApplication(application);
            return View();
        }

        // GET: CUSrinagarAdminPanel/ExaminationWhiteList/Delete/5
        public ActionResult Delete(Guid _ID)
        {
            var result = new ExaminationWhiteListManager().DeleteApplication(_ID);

            if (result > 0)
                ViewBag.SuccessMessage = "Application delete successfulyy.";

            return RedirectToAction("Applications");
        }


        public string GetStudent(string Student_ID)
        {
            var studentdetails = new StudentManager().GetStudentInfoBySearchQuery(Student_ID.ToString());
            
            if (studentdetails != null)
                return studentdetails.First().FullName + "," + studentdetails.First().FathersName + "," + studentdetails.First().ClassRollNo;
            else
                return "Not Available  ,Not Available  ,Not Available";
        }
    }
}
