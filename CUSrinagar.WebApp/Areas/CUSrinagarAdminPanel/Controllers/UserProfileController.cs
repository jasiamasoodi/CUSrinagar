using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using CUSrinagar.Models;
using System.Web.Compilation;
using System.IO;
using CUSrinagar.BusinessManagers;
using GeneralModels;
using CUSrinagar.Enums;
using CUSrinagar.OAuth;
using CUSrinagar.Extensions;
using CUSrinagar.DataManagers;

namespace CUSrinagar.WebApp.CUSrinagarAdminPanel.Controllers
{
    [OAuthorize(AppRoles.University, AppRoles.University_DeputyController, AppRoles.University_Coordinator)]
    public class UserProfileController : Controller
    {
        string errorMsg = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>##</a></div>";

        #region UserProfile
        [OAuthorize(AppRoles.University, AppRoles.University_DeputyController)]
        public ActionResult UserList()
        {
            return View();
        }

        [OAuthorize(AppRoles.University, AppRoles.University_DeputyController)]
        public ActionResult UserListTable(Parameters parameter)
        {
            var obj = System.Web.HttpContext.Current.User;

            List<AppUsers> listUsers = new UserProfileManager().GetAllAppUsers(parameter, obj);
            return View(listUsers);
        }

        [OAuthorize(AppRoles.University, AppRoles.University_DeputyController)]
        public ActionResult Create()
        {
            SetViewBags();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(AppUsers input, List<int> UserRoles_IDs)
        {
            List<AppUserRoles> roles = new List<AppUserRoles>();
            try
            {
                if (UserRoles_IDs.IsNullOrEmpty())
                {
                    ModelState.AddModelError("UserRoles", "Role is Required");
                }
                else if (UserRoles_IDs.Where(x => x == (int)AppRoles.College).Count() > 0 && !(input.College_ID.HasValue))
                {
                    ModelState.AddModelError("College_ID", "College is Required");
                }
                else if (UserRoles_IDs.Where(x => x == (int)AppRoles.University_FileTracking).Count() > 0 && !(input.Department_ID.HasValue))
                {
                    ModelState.AddModelError("Department_ID", "Department is Required");
                }
                if (new UserProfileManager().checkdataExists("UserName", input.UserName, null))
                {
                    ModelState.AddModelError("UserName", "UserName Already Exists");
                }
                if (new UserProfileManager().checkdataExists("Email", input.Email, null))
                {
                    ModelState.AddModelError("Email", "Email Already Exists");
                }

                if (ModelState.IsValid)
                {
                    new UserProfileManager().AddUser(input, UserRoles_IDs, false);
                    if (UserRoles_IDs.Where(x => x == (int)AppRoles.University_FileTracking).Count() > 0 && (input.Department_ID.HasValue))
                    { new UserProfileManager().AddDepartment(input); }
                    new EmailSystem().CredentialsMail(input);
                    return RedirectToAction("UserList");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("ErrorMessage", ex.Message);
            }
            SetViewBags();

            return View(input);
        }

        [OAuthorize(AppRoles.University, AppRoles.University_DeputyController)]
        public ActionResult Edit(Guid id)
        {
            AppUsers model = new UserProfileManager().GetUserById(id);
            model.Department_ID = new UserProfileManager().GetDepartment(id);
            model.Department_ID = model.Department_ID != Guid.Empty ? model.Department_ID : null;
            SetViewBags();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(AppUsers input, List<int> UserRoles_IDs)
        {
            try
            {
                if (UserRoles_IDs.IsNullOrEmpty())
                {
                    ModelState.AddModelError("UserRoles", "Role is Required");
                }
                else if (UserRoles_IDs.Where(x => x == (int)AppRoles.College).Count() > 0 && !(input.College_ID.HasValue))
                {
                    ModelState.AddModelError("College_ID", "College is Required");
                }
                else if (UserRoles_IDs.Where(x => x == (int)AppRoles.University_FileTracking).Count() > 0 && !(input.Department_ID.HasValue))
                {
                    ModelState.AddModelError("Department_ID", "Department is Required");
                }
                if (new UserProfileManager().checkdataExists("UserName", input.UserName, input.User_ID))
                {
                    ModelState.AddModelError("UserName", "UserName Already Exists");
                }
                if (new UserProfileManager().checkdataExists("Email", input.Email, input.User_ID))
                {
                    ModelState.AddModelError("Email", "Email Already Exists");
                }
                ModelState.Remove(nameof(input.Password));
                if (ModelState.IsValid)
                {
                    new UserProfileManager().EditUser(input, UserRoles_IDs, false);
                    new UserProfileManager().DeleteDepartment(input);
                    if (UserRoles_IDs.Where(x => x == (int)AppRoles.University_FileTracking).Count() > 0 && (input.Department_ID.HasValue))
                    { new UserProfileManager().AddDepartment(input); }

                    if (AppUserHelper.User_ID == input.User_ID)
                    {
                        List<AppRoles> roles = AppUserHelper.AppUsercompact?.UserRoles;
                        if (System.Web.HttpContext.Current.User.Identity.IsAuthenticated)
                        {
                            new AuthenticationManager().SignOut();
                        }
                        TempData["response"] = $"<div class='alert alert-success alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button><strong>Great!</strong> <a href='#' class='alert-link'>User details changed successfully. Please login again for changes to reflect.</div>";
                        return RedirectToAction("SignIn", "Account", new { area = string.Empty });
                    }
                    return RedirectToAction("UserList");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("ErrorMessage", ex.Message);
            }
            SetViewBags();
            return View(input);
        }
        public ActionResult DeleteEval(Guid User_id, Guid id)
        {
            new AppUsersDB().DeleteEvalUserSubject(id);
            return RedirectToAction("EditEvalvator", new { id = User_id });
        }
        public ActionResult DeleteaLL(Guid User_id, string id)
        {
            string[] arrayDocs = JsonConvert.DeserializeObject<string[]>(id);

            string instring = arrayDocs.AsEnumerable().ToIN();
            new AppUsersDB().DeleteEvalUserSubject(instring);
            return RedirectToAction("EditEvalvator", new { id = User_id });
        }
        [HttpPost]
        public string Delete(string id)
        {
            Guid ID = new Guid(id);
            bool IsDeleted = new UserProfileManager().DeleteUser(ID);
            return IsDeleted ? "Deleted Successfully" : "Some error occured";
        }
        public string ChangeStatus(Guid id)
        {
            string msg = string.Empty;
            msg = new UserProfileManager().ChangeStatus(id);
            return msg;
        }
        public void SetViewBags()
        {
            if (System.Web.HttpContext.Current.User.IsInRole(CUSrinagar.Enums.AppRoles.University_DeputyController.ToString()))
            {
                ViewBag.RolesList = Helper.GetSelectForAdmission(new List<AppRoles> { AppRoles.University_Coordinator }) ?? new List<SelectListItem>();
                //Load all courses of all colleges.           
            }
            else
            {
                ViewBag.RolesList = Helper.GetSelectList(AppRoles.EvaluatorBills, AppRoles.PaperSetterBills, AppRoles.Student, AppRoles.University_Evaluator, AppRoles.University_Coordinator).OrderBy(x => x.Text);
            }
            ViewBag.CollegeList = new CollegeManager().GetCollegeList();
            ViewBag.DepartmentList = new EmployeeManager().GetEmployeeDepartments(null);
        }

        #endregion

        #region Evalvator Assign Subjects + RollNo to AP's
        [NonAction]
        private void SetViewBagsEvalvator(int track, AppUsers appUsers = null)
        {
            ViewBag.RolesList = Helper.GetSelectForAdmission(new List<AppRoles> { AppRoles.University_Evaluator }) ?? new List<SelectListItem>();
            //Load all courses of all colleges.           
            List<SelectListItem> courseList = new CourseManager().GetAllCourseListOfAllColleges() ?? new List<SelectListItem>();
            courseList.AddRange(new CourseManager().GetOtherSubjectCourse() ?? new List<SelectListItem>());
            ViewBag.CourseDDLList = courseList;
            ViewData["SubjectDDLList" + track] = ViewData["SemesterDDLList" + track] = new List<SelectListItem>();
            ViewData["CollegesDDLList" + track] = new CollegeManager().GetADMCollegeMasterList();
            if (appUsers != null)
            {
                if (appUsers.EvalvatorSubjects != null)
                {
                    appUsers.EvalvatorSubjects = appUsers.EvalvatorSubjects.Where(x => x.Course_ID != null && x.Course_ID != Guid.Empty).ToList();
                    List<SelectListItem> ADMCollegeMasterListDDl;
                    foreach (var obj in appUsers.EvalvatorSubjects)
                    {
                        ADMCollegeMasterListDDl = null;
                        ViewData["SubjectDDLList" + track] = new UserProfileManager().FetchChildDDlValues(obj.Course_ID.ToString(), "Subject", false, obj.Semester) ?? new List<SelectListItem>();
                        ViewData["SemesterDDLList" + track] = new UserProfileManager().FetchChildDDlValues(obj.Course_ID.ToString(), "Semester", false) ?? new List<SelectListItem>();

                        ADMCollegeMasterListDDl = new CollegeManager().GetADMCollegeMasterList();
                        foreach (var item in ADMCollegeMasterListDDl ?? new List<SelectListItem>())
                        {
                            if (obj.Colleges == null)
                                break;

                            if (obj.Colleges?.ToLower()?.Contains(item.Value.ToLower()) ?? false)
                                item.Selected = true;
                        }

                        ViewData["CollegesDDLList" + track] = ADMCollegeMasterListDDl;
                        track++;
                    }

                }

            }
        }

        [OAuthorize(AppRoles.University_Coordinator)]
        public ActionResult UserListEvalvator(Parameters parameter)
        {
            return View();
        }

        [OAuthorize(AppRoles.University_Coordinator)]
        //List all users under Role Evalvator
        public ActionResult UserListTableEvalvator(Parameters parameter)
        {
            List<AppUsers> listUsers = new UserProfileManager().GetAllAppEvalvators(parameter);
            return View(listUsers);
        }

        [OAuthorize(AppRoles.University_Coordinator)]
        public ActionResult CreateEvalvator()
        {
            SetViewBagsEvalvator(0);
            return View(new AppUsers());
        }

        [OAuthorize(AppRoles.University_Coordinator)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateEvalvator(AppUsers input, List<int> UserRoles_IDs)
        {
            input.College_ID = new UserProfileManager().GetUserById(AppUserHelper.User_ID).College_ID;
            List<AppUserRoles> roles = new List<AppUserRoles>();
            if (UserRoles_IDs.IsNullOrEmpty())
            {
                ModelState.AddModelError("UserRoles", "Role is Required");
            }

            if (new UserProfileManager().checkdataExists("UserName", input.UserName, null))
            {
                ModelState.AddModelError("UserName", "UserName Already Exists");
            }
            if (new UserProfileManager().checkdataExists("Email", input.Email, null))
            {
                ModelState.AddModelError("Email", "Email Already Exists");
            }
            if (string.IsNullOrEmpty(input.Evaluator_ID))
            {
                ModelState.AddModelError("Evaluator_ID", "Evaluator ID Required");
            }
            if (ModelState.IsValid)
            {

                Tuple<int, string> response = new UserProfileManager().AddUserEvalvator(input, UserRoles_IDs, true);
                if (response.Item1 > 0)
                {
                    new BillingManager().SaveUpdatePaperSetterInstitute(CreatePaperSetterInstitute(input.Institute, input.User_ID));
                    new EmailSystem().APCredentialsMailEvalvator(input);
                    return RedirectToAction("UserListEvalvator");
                }
                else
                {
                    ModelState.AddModelError("ErrorMessage", response.Item2);
                }
            }

            SetViewBagsEvalvator(0, input);

            return View(input);
        }

        [OAuthorize(AppRoles.University_Coordinator)]
        public ActionResult EditEvalvator(Guid id)
        {
            AppUsers model = new UserProfileManager().GetAPUserInfoById(id);
            model.Institute = new BillingManager().GetUserInstitute(id)?.Institute;
            List<AppUserEvaluatorSubjects> AppUserEvaluatorSubjects = new UserProfileManager().GetEvalvatorSubjectsByUserID(id);
            model.EvalvatorSubjects = AppUserEvaluatorSubjects;
            SetViewBagsEvalvator(0, model);

            return View(model);

        }

        [OAuthorize(AppRoles.University_Coordinator)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditEvalvator(AppUsers input, List<int> UserRoles_IDs)
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
            if (string.IsNullOrEmpty(input.Evaluator_ID))
            {
                ModelState.AddModelError("Evaluator_ID", "Evaluator ID Already Exist");
            }
            ModelState.Remove("Password");
            if (ModelState.IsValid)
            {
                Tuple<int, string> response = new UserProfileManager().EditUserEvalvator(input, UserRoles_IDs, true);
                if (response.Item1 > 0)
                {
                    new BillingManager().SaveUpdatePaperSetterInstitute(CreatePaperSetterInstitute(input.Institute, input.User_ID));
                    return RedirectToAction("UserListEvalvator");
                }
                else
                {
                    ModelState.AddModelError("ErrorMessage", response.Item2);
                }
            }
            SetViewBagsEvalvator(0, input);
            return View(input);

        }

        private BIL_PaperSetterInstitute CreatePaperSetterInstitute(string institute, Guid User_Id)
        {
            BIL_PaperSetterInstitute paperSetterInstitute = new BIL_PaperSetterInstitute()
            {
                Institute = institute,
                User_ID = User_Id

            };
            return paperSetterInstitute;
        }

        public PartialViewResult SubjectSectionEvalvator(int track)
        {
            ViewBag.Track = track;
            SetViewBagsEvalvator(track + 1);
            return PartialView();
        }

        [OAuthorize(AppRoles.University_Coordinator)]
        public string ChangeStatusEvalvator(Guid id)
        {
            string msg = string.Empty;
            msg = new UserProfileManager().ChangeStatus(id);
            return msg;
        }
        public PartialViewResult _GetChildDDLEvalvator(string id, string Type, int track, int Semester)
        {
            ViewBag.Type = Type;
            ViewBag.Track = track;
            ViewBag.ChildValues = new UserProfileManager().FetchChildDDlValuesEvalvator(id, Type, Semester, false);
            return PartialView();
        }
        #endregion

        #region Min-Max Range For Roll No.'s
        [HttpGet]
        public JsonResult GetMinMaxRollNoEvalvator(Guid CourseId, Guid SubjectId, int Semester, string Column)/*, PrintProgramme programme*/
        {
            EvalvatorSubjectsBaseObject EvalvatorSubjectsBaseObject;
            if (Column == "StudentCode")
            { EvalvatorSubjectsBaseObject = new UserProfileManager().GetMinMaxStudentCode(CourseId, SubjectId, Semester); }
            else
            { EvalvatorSubjectsBaseObject = new UserProfileManager().GetMinMaxRollNoEvalvator(CourseId, SubjectId, Semester); }
            return Json(EvalvatorSubjectsBaseObject ?? new EvalvatorSubjectsBaseObject() { MaxRollNo = "0", MaxStudentCode = "0", MinRollNo = "0", MinStudentCode = "0", NoofStudents = "0" }, JsonRequestBehavior.AllowGet);
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
        public ActionResult AssignProfessorCRClasses()
        {
            ProfessorCRClasses professorCRClasses = new ProfessorCRClasses();
            SetViewBagsForAssignProfessor(professorCRClasses);
            return View(professorCRClasses);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AssignProfessorCRClasses(ProfessorCRClasses professorCRClasses)
        {
            if (ModelState.IsValid)
            {
                if (professorCRClasses.ProfessorCRClasses_ID == Guid.Empty)
                    new UserProfileManager().AssignProfessorCRClasses(professorCRClasses);
                else
                    new UserProfileManager().EditProfessorCRClasses(professorCRClasses);
                TempData["response"] = errorMsg.Replace("##", $"Saved successfully.").Replace("alert-danger", "alert-success");

            }
            else
            {
                TempData["response"] = errorMsg.Replace("##", $"Some Error Occured.");
            }
            SetViewBagsForAssignProfessor(professorCRClasses);
            return View(professorCRClasses);
        }
        public void SetViewBagsForAssignProfessor(ProfessorCRClasses professorCRClasses)
        {
            IEnumerable<SelectListItem> ProgrammeDDL = new List<SelectListItem>();
            ProgrammeDDL = Helper.GetSelectList<Programme>();
            IEnumerable<SelectListItem> SemesterDDL = new List<SelectListItem>();
            SemesterDDL = Helper.GetSelectList<Semester>();
            IEnumerable<SelectListItem> SubjectDDL = new List<SelectListItem>();
            IEnumerable<SelectListItem> ProfessorDDL = new List<SelectListItem>();
            IEnumerable<SelectListItem> CRDDL = new List<SelectListItem>();
            List<SelectListItem> CourseDDL = new List<SelectListItem>();
            if (professorCRClasses != null && professorCRClasses.Course_ID != null)
            {

                int ProgrammeId = Convert.ToInt32(professorCRClasses.Programme);
                CourseDDL = new CourseManager().GetAllCoursesByProgramme(ProgrammeId > 0 ? ProgrammeId : Convert.ToInt32(ProgrammeDDL.FirstOrDefault().Value));
                Parameters parameters = createParameters("College_Id", professorCRClasses.College_ID.ToString(), string.Empty, string.Empty);
                CRDDL = new UserProfileManager().GetUserList(AppRoles.College_ClassRepresentative, parameters) ?? new List<SelectListItem>();
                ProfessorDDL = new UserProfileManager().GetUserList(AppRoles.College_AssistantProfessor, parameters) ?? new List<SelectListItem>();
                parameters = createParameters("Course_ID", professorCRClasses.Course_ID.ToString(), "Semester", ((int)professorCRClasses.Semester).ToString());
                SubjectDDL = new SubjectsManager().SubjectDDLWithDetail(parameters) ?? new List<SelectListItem>();
                ViewBag.CourseName = (new CourseManager().GetCourseById(professorCRClasses.Course_ID) ?? new ADMCourseMaster()).CourseFullName;
                ViewBag.Semester = professorCRClasses.Semester;
            }
            ViewBag.ProgrammeDDLList = ProgrammeDDL == null ? new List<SelectListItem>() : ProgrammeDDL;
            ViewBag.CourseDDLList = CourseDDL == null ? new List<SelectListItem>() : CourseDDL;
            ViewBag.SemesterDDLList = SemesterDDL == null ? new List<SelectListItem>() : SemesterDDL;
            ViewBag.CollegeDDLList = new CollegeManager().GetADMCollegeMasterList();
            ViewBag.CRDDLList = CRDDL;
            ViewBag.SubjectDDLList = SubjectDDL;
            ViewBag.ProfessorDDLList = ProfessorDDL;


        }

        private Parameters createParameters(string col1, string col1val, string col2, string col2val)
        {
            Parameters parameters = new Parameters() { Filters = new List<SearchFilter>() };
            parameters.Filters.Add(new SearchFilter() { Column = col1, Operator = SQLOperator.EqualTo, Value = col1val, GroupOperation = LogicalOperator.AND });
            if (!string.IsNullOrEmpty(col2))
            { parameters.Filters.Add(new SearchFilter() { Column = col2, Operator = SQLOperator.EqualTo, Value = col2val, GroupOperation = LogicalOperator.AND }); }
            return parameters;
        }
        [HttpPost]
        public JsonResult CheckExistSubject(ProfessorCRClasses professorCRClasses)
        {
            professorCRClasses = new UserProfileManager().CheckExistSubject(professorCRClasses);
            if (professorCRClasses != null)
                return Json(new { Professor = professorCRClasses.Professor_ID, CR = professorCRClasses.CR_ID, PC_ID = professorCRClasses.ProfessorCRClasses_ID }, JsonRequestBehavior.AllowGet);
            else
                return Json(new { });
        }
    }
}

