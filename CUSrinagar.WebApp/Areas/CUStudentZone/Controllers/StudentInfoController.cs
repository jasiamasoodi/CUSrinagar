using CUSrinagar.Enums;
using CUSrinagar.Models;
using CUSrinagar.OAuth;
using System;
using CUSrinagar.BusinessManagers;
using System.Collections.Generic;
using System.Web.Mvc;

namespace CUSrinagar.WebApp.CUStudentZone.Controllers
{
    [OAuthorize(AppRoles.Student, AppRoles.University_StudentProfile)]
    public class StudentInfoController : StudentBaseController
    {
        public ActionResult GetInfo()
        {
            ARGPersonalInformation aRGPersonal = new StudentManager().GetStudent(AppUserHelper.User_ID, AppUserHelper.OrgPrintProgramme);

            if (aRGPersonal != null)
            {
                ViewBag.PrintProgramme = AppUserHelper.TableSuffix;
                ViewBag.Student_ID = aRGPersonal.Student_ID;
                return View();
            }
            return RedirectToAction("Index", "DashBoard");
        }
        public PartialViewResult PersonalDetail(Guid? student_Id, PrintProgramme? printProgramme)
        {
            if (printProgramme == null || student_Id == null)
                return PartialView(new ARGPersonalInformation());

            ARGPersonalInformation pinfo = new StudentManager().GetStudent(student_Id.Value, printProgramme.Value);
            return PartialView(pinfo);
        }
        public PartialViewResult ExaminationDetail(Guid? student_Id, PrintProgramme? printProgramme)
        {
            if (printProgramme == null || student_Id == null)
                return PartialView(new List<Semesters>());

            List<Semesters> ExamForms = new StudentManager().GetStudentExaminationHistory(student_Id.Value, printProgramme.Value, true);
            return PartialView(ExamForms);
        }

        public PartialViewResult AdmissionDetail(Guid? student_Id, PrintProgramme? printProgramme)
        {
            if (printProgramme == null || student_Id == null)
                return PartialView(new List<AdmissionForm>());

            List<AdmissionForm> AdmissionForms = new StudentManager().GetStudentAdmissionFormHistory(student_Id.Value, printProgramme.Value);
            return PartialView(AdmissionForms);
        }
    }
}