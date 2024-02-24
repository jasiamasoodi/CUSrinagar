using CUSrinagar.Enums;
using CUSrinagar.OAuth;
using GeneralModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CUSrinagar.DataManagers;
using CUSrinagar.BusinessManagers;
using CUSrinagar.Models;
using CUSrinagar.Extensions;

namespace CUSrinagar.WebApp.CUCollegeAdminPanel.Controllers
{
    [OAuthorize(AppRoles.College_Principal)]
    public class UserProfileController : CollegeAdminBaseController
    {
        #region Principal Assign Subjects + RollNo to AP's
        [NonAction]
        private void SetViewBags(int track, AppUsers appUsers = null)
        {
            ViewBag.RolesList = Helper.GetSelectForAdmission(new List<AppRoles> { AppRoles.College_AssistantProfessor }) ?? new List<SelectListItem>();
            ViewBag.LoggedCollege_ID = AppUserHelper.College_ID;
            List<SelectListItem> courseList = new CourseManager().GetCourseListNEP(ViewBag.LoggedCollege_ID) ?? new List<SelectListItem>();
            courseList.AddRange(new CourseManager().GetOtherSubjectCourse() ?? new List<SelectListItem>());
            ViewBag.CourseDDLList = courseList;
            ViewData["SubjectDDLList" + track] = ViewData["SemesterDDLList" + track] = new List<SelectListItem>();
            FillViewBag_Semesters();
            if (appUsers != null)
            {
                if (appUsers.ProfessorSubjects != null)
                {
                    appUsers.ProfessorSubjects = appUsers.ProfessorSubjects.ToList();
                    foreach (var obj in appUsers.ProfessorSubjects)
                    {
                        ViewData["SubjectDDLList" + track] = new UserProfileManager().FetchChildDDlValues(obj.Course_ID.ToString(), "Subject", true, obj.Semester) ?? new List<SelectListItem>();
                        //ViewData["SemesterDDLList" + track] =semesterd new UserProfileManager().FetchChildDDlValues(obj.Course_ID.ToString(), "Semester",true) ?? new List<SelectListItem>();

                        track++;
                    }
                }

            }
        }

        public ActionResult UserList()
        {
            return View();
        }

        //List all users under Role Assistant Professor
        public ActionResult UserListTable(Parameters parameter)
        {
            List<AppUsers> listUsers = new UserProfileManager().GetAllAppUsersAP(parameter);
            return View(listUsers);
        }
        public ActionResult Create()
        {
            SetViewBags(0);
            return View(new AppUsers());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(AppUsers input, List<int> UserRoles_IDs)
        {
            input.College_ID = AppUserHelper.College_ID;
            List<AppUserRoles> roles = new List<AppUserRoles>();
            if (UserRoles_IDs.IsNullOrEmpty())
            {
                ModelState.AddModelError("UserRoles", "Role is Required");
            }
            else if (UserRoles_IDs.Where(x => x == (int)AppRoles.College).Count() > 0 && !(input.College_ID.HasValue))
            {
                ModelState.AddModelError("College_ID", "College is Required");
            }
            if (new UserProfileManager().checkdataExists("UserName", input.UserName, null))
            {
                ModelState.AddModelError("UserName", "UserName Already Exists");
            }
            if (new UserProfileManager().checkdataExists("Email", input.Email, null))
            {
                ModelState.AddModelError("Email", "Email Already Exists");
            }
            //if (input != null && input.ProfessorSubjects.IsNotNullOrEmpty())
            //    for (int i = 0; i < input.ProfessorSubjects.Count; i++)
            //    {

            //        ModelState.Remove($"ProfessorSubjects[{i}].Course_ID");
            //    }
            if (ModelState.IsValid)
            {
                Tuple<int, string> response = new UserProfileManager().AddUser(input, UserRoles_IDs, true);
                if (response.Item1 > 0)
                {
                    new EmailSystem().APCredentialsMail(input);
                    return RedirectToAction("UserList");
                }
                else
                {
                    ModelState.AddModelError("ErrorMessage", response.Item2);
                }
            }

            SetViewBags(0, input);

            return View(input);
        }


        public ActionResult Edit(Guid? id)
        {
            if (!Guid.TryParse(id + "", out Guid Id))
                return RedirectToAction("UserList");

            AppUsers model = new UserProfileManager().GetAPUserInfoById(Id);
            List<AppUserProfessorSubjects> appUserProfessorSubjects = new UserProfileManager().GetAllAPSubjectsByUserID(Id);
            model.ProfessorSubjects = appUserProfessorSubjects;
            SetViewBags(0, model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(AppUsers input, List<int> UserRoles_IDs)
        {

            if (UserRoles_IDs.IsNullOrEmpty())
            {
                ModelState.AddModelError("UserRoles", "Role is Required");
            }
            else if (UserRoles_IDs.Where(x => x == (int)AppRoles.College).Count() > 0 && !(input.College_ID.HasValue))
            {
                ModelState.AddModelError("College_ID", "College is Required");
            }
            if (new UserProfileManager().checkdataExists("UserName", input.UserName, input.User_ID))
            {
                ModelState.AddModelError("UserName", "UserName Already Exists");
            }
            if (new UserProfileManager().checkdataExists("Email", input.Email, input.User_ID))
            {
                ModelState.AddModelError("Email", "Email Already Exists");
            }

            //if(input!=null && input.ProfessorSubjects.IsNotNullOrEmpty())
            //    for (int i=0;i<input.ProfessorSubjects.Count;i++) {

            //        ModelState.Remove($"ProfessorSubjects[{i}].Course_ID");
            //    }
            ModelState.Remove(nameof(input.Password));

            if (ModelState.IsValid)
            {
                Tuple<int, string> response = new UserProfileManager().EditUser(input, UserRoles_IDs, true);
                if (response.Item1 > 0)
                    return RedirectToAction("UserList");
                else
                {
                    ModelState.AddModelError("ErrorMessage", response.Item2);
                }
            }
            string error = string.Join(", ", ModelState.Values.SelectMany(s => s.Errors).Select(y => y.ErrorMessage).ToArray());
            SetViewBags(0, input);
            return View(input);

        }

        public PartialViewResult _GetChildDDL(string id, string Type, int track, int Semester)
        {
            ViewBag.Type = Type;
            ViewBag.Track = track;
            ViewBag.ChildValues = new UserProfileManager().FetchChildDDlValues(id, Type, true, Semester);
            return PartialView();
        }
        public PartialViewResult SubjectSection(int track)
        {
            ViewBag.Track = track;
            SetViewBags(track + 1);
            return PartialView();
        }

        public string ChangeStatus(Guid id)
        {
            string msg = string.Empty;
            msg = new UserProfileManager().ChangeStatus(id);
            return msg;
        }
        public ActionResult Delete(Guid User_id, Guid id)
        {
            new AppUsersDB().DeleteAPUserSubject(id);
            return RedirectToAction("Edit", new { id = User_id });
        }


        #endregion

        #region Instructions

        [HttpGet]
        public ActionResult Instructions(string cell = "")
        {
            ViewBag.SelectedTab = cell.ToLower();
            return View();
        }
        #endregion

        #region Min-Max Range For Roll No.'s
        [HttpGet]
        public JsonResult GetMinMaxRollNo(Guid CourseId, Guid SubjectId, int Semester, string Column)/*, PrintProgramme programme*/
        {
            PrintProgramme pp = (PrintProgramme)Enum.Parse(typeof(PrintProgramme), Column);
            return Json(new UserProfileManager().GetMinMaxRollNo(CourseId, SubjectId, Semester, pp), JsonRequestBehavior.AllowGet);
        }
        #endregion


    }
}