using CUSrinagar.Enums;
using CUSrinagar.Models;
using CUSrinagar.OAuth;
using System;
using CUSrinagar.BusinessManagers;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CUSrinagar.WebApp.CUStudentZone.Controllers
{
    [OAuthorize(AppRoles.Student)]
    public class DashboardController : Controller
    {
        #region Student-Profile

        [HttpGet]
        public ActionResult Index()
        {
            return RedirectToAction("GetInfo", "StudentInfo", new { area = "CUStudentZone" });
            //AppUserHelper.User_ID;//            student_ID
            //AppUserHelper.TableSuffix;//        printprogramme  
            // ADMCourseMaster aDMCourseMaster = new ADMCourseMaster();
            // ARGPersonalInformation aRGPersonalInformation = new RegistrationManager().GetStudentByID(AppUserHelper.User_ID, AppUserHelper.TableSuffix);
            ////retreive combination-Id of a student
            // ADMCombinationMaster aDMCombinationMaster = new StudentProfileManager().GetStudentCombinationByStudentID(AppUserHelper.User_ID, AppUserHelper.TableSuffix);
            // if (aDMCombinationMaster.Combination_ID != Guid.Empty)
            // { 
            //     //retreive all subjectnames by CombinationID
            //     ViewBag.StudentSubjects = new StudentProfileManager().GetSubjectsByCombinationID(aDMCombinationMaster.Combination_ID);
            //     ViewBag.Course_ID = aDMCombinationMaster.Course_ID;
            //     ViewBag.CourseName = (new CourseManager().GetCourseById(ViewBag.Course_ID) ?? new ADMCourseMaster()).CourseFullName;
            // }
            // return View(aRGPersonalInformation); 
        }
        #endregion
    }
}