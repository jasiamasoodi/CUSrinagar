using CaptchaMvc.HtmlHelpers;
using CUSrinagar.BusinessManagers;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using CUSrinagar.Models;
using CUSrinagar.OAuth;
using GeneralModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;


namespace CUSrinagar.WebApp.CUStudentZone.Controllers
{
    [OAuthorize(AppRoles.Student)]
    public class CombinationController : StudentBaseController
    {
        #region UpdateActiveSemesterCombination
        public ActionResult SetCombination(bool hasError = false)
        {
            if (hasError)
            {
                GetResponseViewBags();
                return View();
            }
            var stdinfo = new StudentManager().GetStudentWithCombination(AppUserHelper.User_ID, AppUserHelper.OrgPrintProgramme, false, null);
            var batch = stdinfo?.SelectedCombinations?.Select(x => x.SemesterBatch).OrderByDescending(x => x)?.First() ?? 0;
            if (batch == 2020 && stdinfo.CurrentSemesterOrYear >= 1 && stdinfo.CurrentSemesterOrYear <= 2)
                return RedirectToAction("GetSetCombination", new { @UpdateActiveSemester = "2".Encrypt() });
            else if (batch == 2019 && stdinfo.CurrentSemesterOrYear >= 3 && stdinfo.CurrentSemesterOrYear <= 4)
                return RedirectToAction("GetSetCombination", new { @UpdateActiveSemester = "4".Encrypt() });
            else if (batch == 2018 && stdinfo.CurrentSemesterOrYear >= 5 && stdinfo.CurrentSemesterOrYear <= 6)
                return RedirectToAction("GetSetCombination", new { @UpdateActiveSemester = "6".Encrypt() });
            else if (batch == 2018 && AppUserHelper.OrgPrintProgramme == PrintProgramme.PG && stdinfo.CurrentSemesterOrYear >= 3 && stdinfo.CurrentSemesterOrYear <= 4)
                return RedirectToAction("GetSetCombination", new { @UpdateActiveSemester = "4".Encrypt() });
            else
                ViewBag.ErrorMessage = "There is some issue while updating your subject combination. Please contact on the below emails.";
            return View();
        }
        public ActionResult GetSetCombination(string UpdateActiveSemester)
        {
            PrintProgramme? printProg = AppUserHelper.OrgPrintProgramme;
            PrintProgramme? printProg1 = AppUserHelper.OrgPrintProgramme;
            string Form_RegistrationNumber = new StudentManager().GetStudent(AppUserHelper.User_ID, AppUserHelper.OrgPrintProgramme, false).CUSRegistrationNo;
            short? sem = null;
            if (string.IsNullOrEmpty(UpdateActiveSemester))
            {
                TempData["ErrorMessage"] = "Invalid arguments. Please try again later.";
                return RedirectToAction("SetCombination", new { @hasError = true });
            }
            if (short.TryParse(UpdateActiveSemester.Decrypt(), out short _sem) && short.Parse(UpdateActiveSemester.Decrypt()) > 0)
            {
                sem = short.Parse(UpdateActiveSemester.Decrypt());
            }
            if (sem == null)
            {
                ViewBag.ErrorMessage = "Invalid arguments. Please try again later.";
                return RedirectToAction("SetCombination", new { @hasError = true });
            }
            if (printProg1.HasValue) printProg = printProg1.Value;

            FillViewBags(Form_RegistrationNumber, printProg, sem);
            ARGPersonalInformation STDINFO = null;
            GetResponseViewBags();
            if (Form_RegistrationNumber.IsNotNullOrEmpty())
            {
                STDINFO = new StudentManager().GetStudentWithCombination(Form_RegistrationNumber, printProg.Value, false);
                if (STDINFO != null && STDINFO.SelectedCombinations.IsNotNullOrEmpty())
                {
                    Guid Course_ID; short SemesterBatch;
                    ARGSelectedCombination _Comb = STDINFO.SelectedCombinations.FirstOrDefault(x => x.Semester == sem);
                    if (_Comb == null)
                        _Comb = STDINFO.SelectedCombinations.OrderByDescending(x => x.Semester).First();
                    if (_Comb != null)
                    {
                        Course_ID = _Comb.Course_ID; SemesterBatch = _Comb.SemesterBatch;
                        var _CSManager = new CombinationSettingManager();
                        var _CombinationSetting = _CSManager.GetCombinationSetting(Course_ID, sem.Value, SemesterBatch);
                        if (_CombinationSetting != null)
                        {
                            ViewBag.CombinationSetting = _CombinationSetting;
                            AssignCombinationViewModel _AssignCombinationViewModel = new AssignCombinationManager().GetSubjectListOptions(_CombinationSetting, STDINFO, sem.Value).Item2;
                            ViewBag.AssignCombinationViewModel = _AssignCombinationViewModel;

                            if (_CombinationSetting.CombinationSettingStructure.AllowInterCollegeElective && _CombinationSetting.CombinationSettingStructure.GE_OE_Count > 0)
                            {
                                ViewBag.Programme = new CourseManager().GetPrintProgrammesInClause(_Comb.Course_ID);
                                FillViewBag_College();
                            }
                            return View(STDINFO);
                        }
                        else
                        {
                            TempData["ErrorMessage"] = $"Combination Setting not available. Please contact Cluster University I.T Section.";
                            return RedirectToAction("SetCombination", new { @hasError = true });
                        }
                    }
                    else
                    {
                        ViewBag.ErrorMessage = $"Previous Semester Combination not found.";
                        return RedirectToAction("SetCombination", new { @hasError = true });
                    }
                }
            }
            return RedirectToAction("SetCombination", new { @hasError = true });
        }
        public ActionResult PostSubjectCombinationRollNumber(Guid Student_ID, short Semester, PrintProgramme PrintProgramme, List<Guid> subject,
          List<Guid> additionalSubject, string ClassRollNo, bool? OverrideClassRollNumber, string Form_RegistrationNumber, short SemesterBatch,
          bool? fromAcceptExamFormScreen, short Batch)
        {
            return View();
            //List<ADMSubjectMaster> subjectmasterlist = new List<ADMSubjectMaster>(); List<ADMSubjectMaster> addionalSubjectmasterlist = new List<ADMSubjectMaster>();
            //if (subject.IsNotNullOrEmpty()) foreach (var item in subject.Where(x => x != Guid.Empty)) subjectmasterlist.Add(new ADMSubjectMaster() { Subject_ID = item });
            //if (additionalSubject.IsNotNullOrEmpty()) foreach (var item in additionalSubject.Where(x => x != Guid.Empty)) addionalSubjectmasterlist.Add(new ADMSubjectMaster() { Subject_ID = item });

            //var response = new AssignCombinationManager().SaveUpdateStudentCombination(PrintProgramme, Student_ID, Semester, subjectmasterlist, SemesterBatch, addionalSubjectmasterlist);

            //SetResponseViewBags(response);
            //return RedirectToAction("GetSetCombination", new { @UpdateActiveSemester = Semester.ToString().Encrypt() });
        }
        #endregion

        public ActionResult ChooseSemester()
        {
            return RedirectToAction("ApplySemesterAdm", "Student", new { area = "CUStudentZone" });
            //GetResponseViewBags();
            //return View();
        }

        void FillViewBags(string Form_RegistrationNumber, PrintProgramme? printProg, short? sem)
        {
            ViewBag.Form_RegistrationNumber = Form_RegistrationNumber;
            ViewBag.Semester = sem;
            ViewBag.PrintProgramme = printProg ?? PrintProgramme.UG;
            ViewBag.PrintProgrammeList = Helper.GetSelectList<PrintProgramme>().OrderBy(x => x.Value);
            //ViewBag.Semesters = new List<SelectListItem>() { new SelectListItem() { Text = "Semester-I (New)", Value = "1" }, new SelectListItem() { Text = "Semester-II", Value = "2" }, new SelectListItem() { Text = "Semester-III", Value = "3" }, new SelectListItem() { Text = "Semester-IV", Value = "4" }, new SelectListItem() { Text = "Semester-V", Value = "5" } };
            FillViewBag_Semesters();
        }

        public ActionResult GetCombination(short? semester, bool RedirectedFormExaminantionForm = false)
        {
            ViewBag.RedirectedFormExaminantionForm = RedirectedFormExaminantionForm;
            ViewBag.PaymentAmount = "";
            if (RedirectedFormExaminantionForm)
            {
                if (semester.HasValue && semester.Value == 1)
                    return RedirectToAction("ExaminationForm", "Examination", new { @semester = semester });
                else
                {
                    ARGStudentExamForm _StudentExamForm = new ExaminationFormManager().GetRegularExamFormOnlyAccepted(AppUserHelper.User_ID, semester.Value, AppUserHelper.TableSuffix);
                    if (_StudentExamForm != null)
                    {
                        ViewBag.PaymentAmount = _StudentExamForm.AmountPaid.ToString();
                    }
                }
            }
            else
            {
                if (!semester.HasValue)
                    return RedirectToAction("SemesterAdmForm", "Student", new { area = "CUStudentZone" });

                PaymentDetails paymentDetail = new PaymentManager().GetPaymentDetail(AppUserHelper.User_ID, semester.Value);
                if (paymentDetail == null)
                    return RedirectToAction("SemesterAdmForm", "Student", new { area = "CUStudentZone" });
            }
            GetResponseViewBags();
            ViewBag.SemesterChoosen = semester;
            PrintProgramme? printProg = AppUserHelper.TableSuffix;
            short? sem = semester;
            FillViewBags("", printProg, sem);
            ARGPersonalInformation STDINFO = null;
            GetResponseViewBags();
            if (printProg.HasValue && sem.HasValue)
            {
                STDINFO = new StudentManager().GetStudentWithCombination(AppUserHelper.User_ID, printProg.Value, false);
                if (STDINFO != null)
                {
                    ARGSelectedCombination _PrevSemCombination = STDINFO.SelectedCombinations.FirstOrDefault(x => x.Semester == (short)(sem - 1 > 1 ? sem - 1 : 1));
                    if (_PrevSemCombination != null)
                    {
                        var _CSManager = new CombinationSettingManager();
                        CombinationSetting _CombinationSetting = _CSManager.GetCombinationSetting(_PrevSemCombination.Course_ID, sem.Value, STDINFO.Batch);
                        if (_CombinationSetting != null)
                        {
                            ViewBag.CombinationSetting = _CombinationSetting;
                            AssignCombinationViewModel _AssignCombinationViewModel = new AssignCombinationManager().GetSubjectListOptions(_CombinationSetting, STDINFO, sem.Value).Item2;
                            ViewBag.AssignCombinationViewModel = _AssignCombinationViewModel;

                            if (_CombinationSetting.CombinationSettingStructure.AllowInterCollegeElective && _CombinationSetting.CombinationSettingStructure.GE_OE_Count > 0)
                            {
                                ViewBag.Programme = new CourseManager().GetPrintProgrammesInClause(_PrevSemCombination.Course_ID);
                                FillViewBag_College();
                            }
                            return View(STDINFO);
                        }
                        else
                        {
                            ViewBag.ErrorMessage = $"Combination Setting not available. Please contact Cluster University I.T Section.";
                        }
                    }
                }
            }

            return View(STDINFO);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PostCombination(short? Semester, List<Guid> Subject, List<Guid> AdditionalSubject, short Batch, bool RedirectedFormExaminantionForm = false)
        {
            return View();
            /*ResponseData response = new ResponseData();
            List<ADMSubjectMaster> subjectmasterlist = new List<ADMSubjectMaster>();
            List<ADMSubjectMaster> addionalSubjectmasterlist = new List<ADMSubjectMaster>();
            if (Subject != null)
                foreach (var item in Subject.Where(x => x != Guid.Empty))
                    subjectmasterlist.Add(new ADMSubjectMaster() { Subject_ID = item });
            if (AdditionalSubject != null)
                foreach (var item in AdditionalSubject.Where(x => x != Guid.Empty))
                    addionalSubjectmasterlist.Add(new ADMSubjectMaster() { Subject_ID = item });

            if (subjectmasterlist.IsNotNullOrEmpty() && Semester.HasValue && Semester.Value > 0)
                response = new AssignCombinationManager().SaveUpdateStudentCombination(AppUserHelper.TableSuffix, AppUserHelper.User_ID, Semester.Value, subjectmasterlist, Batch, addionalSubjectmasterlist);

            SetResponseViewBags(response);
            if (response.IsSuccess)
            {
                if (RedirectedFormExaminantionForm)
                {
                    return RedirectToAction("ExaminationForm", "Examination", new { @semester = Semester });
                }
                else
                {
                    return RedirectToAction("SemesterAdmForm", "Student", new { S = Semester.Value.ToString().EncryptCookieAndURLSafe(), area = "CUStudentZone" });
                }
            }
            else
                return RedirectToAction("GetCombination", new { @semester = Semester, @RedirectedFormExaminantionForm = RedirectedFormExaminantionForm });*/
        }

        private string GetPrintProgrammes(Guid course_ID)
        {
            ADMCourseMaster aDMCourseMaster = new CourseManager().GetCourseById(course_ID);
            string printPrg = "";
            switch (aDMCourseMaster.PrintProgramme)
            {
                case PrintProgramme.IH:
                case PrintProgramme.UG:
                    printPrg = ((short)Programme.UG).ToString() + "," + ((short)Programme.IG).ToString() + "," + ((short)Programme.HS).ToString() + "," + ((short)Programme.Professional).ToString();
                    break;
                case PrintProgramme.PG:
                    printPrg = ((short)Programme.PG).ToString();
                    break;
                case PrintProgramme.BED:
                    printPrg = ((short)PrintProgramme.BED).ToString();
                    break;
            }
            return printPrg;
        }
    }
}