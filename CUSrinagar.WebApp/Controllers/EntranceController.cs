using CaptchaMvc.HtmlHelpers;
using CUSrinagar.BusinessManagers;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using CUSrinagar.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace CUSrinagar.WebApp.Controllers
{
    public class EntranceController : Controller
    {
        [HttpGet]
        public ActionResult ProvAdmForm()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProvAdmForm(ARGReprint SearchDetails)
        {
            if (!ModelState.IsValid)
                return View(SearchDetails);

            if (!new RegistrationManager().ValidateDOB(SearchDetails.EnteredDOB))
            {
                ModelState.AddModelError("EnteredDOB", "Invalid DOB");
            }
            PrintProgramme origionalPrintProgram = SearchDetails.PrintProgrammeOption;
            if (SearchDetails.PrintProgrammeOption == PrintProgramme.UG)
            {
                SearchDetails.PrintProgrammeOption = PrintProgramme.IH;
            }

            Guid id = new RegistrationManager().GetStudentIDByFormNoEntranceRollNoAndDOBCUS(SearchDetails);
            if ((id == null) || (id == Guid.Empty))
            {
                ModelState.AddModelError("InvalidFormNo", "Invalid  Form No");
                TempData["error2"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'> Form does not exist.</a></div>";
            }
            ARGPersonalInformation studentPersonalInfo = new RegistrationManager().GetStudentByID(id, SearchDetails.PrintProgrammeOption) ?? new ARGPersonalInformation();

            if (studentPersonalInfo.Batch != SearchDetails.Batch)
            {
                TempData["error2"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'> It seems that your batch is not allowed.</a></div>";
                return RedirectToAction("ProvAdmForm", "Entrance", new { area = string.Empty });
            }

            if (ModelState.IsValid)
            {
                return RedirectToAction("ProvAdmFormPrint", "Entrance", new { id = id + "/", R = ((int)origionalPrintProgram).ToString().EncryptCookieAndURLSafe(), area = string.Empty });
            }
            return View();
        }

        [HttpGet]
        public ActionResult ProvAdmFormPrint(string id, string R)
        {
            if (!Guid.TryParse(id + "", out Guid result) || !Enum.TryParse((R + "").DecryptCookieAndURLSafe(), out PrintProgramme programme))
            {
                return RedirectToAction("ProvAdmForm", "Entrance", new { area = string.Empty });
            }
            PrintProgramme origionalPrintProgram = programme;
            if (programme == PrintProgramme.UG)
            {
                programme = PrintProgramme.IH;
            }
            ViewBag.UGConsolidatedPoinsts = null;
            ARGPersonalInformation studentPersonalInfo = new RegistrationManager().GetStudentByID(result, programme);
            ViewBag.AppliedFor = origionalPrintProgram.GetEnumDescription().Replace(" / Engineering", "");

            if (studentPersonalInfo != null)
            {
                if (studentPersonalInfo.FormStatus == FormStatus.InProcess)
                {
                    TempData["error2"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'> It seems that you have not paid registration fee , also not appeared in entrance test.</a></div>";
                    return RedirectToAction("Instructions", "Registration", new { area = string.Empty });
                }
                if (studentPersonalInfo.CoursesApplied?.All(x => x.AppearedInEntrance == false) ?? true)
                {
                    TempData["error2"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>It seems that you have not appeared in Entrance Test.</a></div>";
                    return RedirectToAction("ProvAdmForm", "Entrance", new { area = string.Empty });
                }
                else if ((studentPersonalInfo.CoursesApplied?.All(x => x.StudentSelectionStatus != StudentSelectionStatus.Provisional) ?? true
                           || (studentPersonalInfo.CoursesApplied?.All(x => x.StudentSelectionStatus != StudentSelectionStatus.Joined)
                           ?? true)))
                {
                    TempData["error2"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>It seems that you are not selected in the entrance.</a></div>";
                    return RedirectToAction("ProvAdmForm", "Entrance", new { area = string.Empty });
                }
                if (programme == PrintProgramme.IH && studentPersonalInfo.CATEntrancePoints == null)
                {
                    TempData["error2"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>It seems that you have not appeared in Common Aptitude Test.</a></div>";
                    return RedirectToAction("ProvAdmForm", "Entrance", new { area = string.Empty });
                }

                if (studentPersonalInfo.CoursesApplied.IsNullOrEmpty())
                    studentPersonalInfo.CoursesApplied = new List<ARGCoursesApplied>();
                else
                {
                    if (origionalPrintProgram == PrintProgramme.UG)
                    {
                        studentPersonalInfo.CoursesApplied = studentPersonalInfo.CoursesApplied
                            .Where(x => x.Course_ID == Guid.Parse("A3EE7F98-7B82-4D95-A2C0-FABA7A18240E")
                            && (x.StudentSelectionStatus == StudentSelectionStatus.Joined || x.StudentSelectionStatus == StudentSelectionStatus.Provisional))?.ToList();

                        if (studentPersonalInfo.CoursesApplied.IsNullOrEmpty())
                        {
                            TempData["error2"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>It seems that you are not selected for Graduation.</a></div>";
                            return RedirectToAction("ProvAdmForm", "Entrance", new { area = string.Empty });
                        }

                        ARGStudentPreviousQualifications I2thDetails = studentPersonalInfo.AcademicDetails.FirstOrDefault(x => x.ExamName.Trim().ToLower() == "12th");
                        if (I2thDetails != null)
                        {
                            ViewBag.UGConsolidatedPoinsts = Convert.ToDecimal(((I2thDetails.MarksObt / I2thDetails.MaxMarks * 100) * 60) / 100) + (studentPersonalInfo.CATEntrancePoints ?? 0);
                            studentPersonalInfo.CoursesApplied.First(x => x.Course_ID == Guid.Parse("A3EE7F98-7B82-4D95-A2C0-FABA7A18240E")).SubjectEntrancePoints = ViewBag.UGConsolidatedPoinsts;
                        }
                    }
                    else
                    {
                        if (studentPersonalInfo.CoursesApplied.FirstOrDefault(x => x.Course_ID == Guid.Parse("A3EE7F98-7B82-4D95-A2C0-FABA7A18240E")) != null)
                            studentPersonalInfo.CoursesApplied.First(x => x.Course_ID == Guid.Parse("A3EE7F98-7B82-4D95-A2C0-FABA7A18240E")).AppearedInEntrance = false;
                    }
                }

                return View(studentPersonalInfo);
            }

            return RedirectToAction("ProvAdmForm", "Entrance", new { area = string.Empty });
        }
    }
}