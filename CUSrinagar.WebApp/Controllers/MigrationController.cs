using CUSrinagar.BusinessManagers;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using CUSrinagar.Models;
using CUSrinagar.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;



namespace CUSrinagar.WebApp.Controllers
{
    public class MigrationController : Controller
    {
        public ActionResult Form()
        {
            //  return new EmptyResult();
            FillViewBags();
            return View();
        }


        void FillViewBags()
        {
            ViewBag.FormTypeList = Helper.GetSelectedList(MigrationE.CancelRegistration, MigrationE.PassoutMigration).OrderBy(x => x.Value);
            ViewBag.PrintProgrammeList = Helper.GetSelectList<PrintProgramme>().OrderBy(x => x.Value);
        }
        [HttpGet]
        public ActionResult Migration()
        {
            return RedirectToAction("Form");
        }

        [HttpPost]
        public ActionResult Migration(MigrationE formType, PrintProgramme Programme)
        {
            //return new EmptyResult();
            SetViewBags(Guid.Empty, Programme, formType);
            return View();
        }
        private void SetViewBags(Guid StudentCollege_ID, PrintProgramme programme, MigrationE formType)
        {
            ViewBag.CollegeList = new CollegeManager().GetADMCollegeMasterList().Where(x => x.Value != StudentCollege_ID.ToString());
            ViewBag.PrintProgrammeIs = programme;
            ViewBag.FormTypeIs = formType;
        }
        private MigrationPI FillDetail(string CusRegistrationNo, DateTime DOB, PrintProgramme programme)
        {
            MigrationPI studentInfo = new MigrationPI();
            if (!string.IsNullOrEmpty(CusRegistrationNo))
                studentInfo = new MigrationManager().GetMigrationStudent(CusRegistrationNo, DOB, programme);

            if (studentInfo != null && studentInfo.SelectedCombinations != null)
            {
                ViewBag.CourseName = studentInfo.SelectedCombinations.First().CourseID;
                Tuple<List<SemesterModel>, int> response = new StudentManager().GetLastSemesterResultDetails(studentInfo, programme);
                studentInfo.SemesterModel = response.Item1;
                ViewBag.LastSemester = response.Item2;
            }
            return studentInfo;

        }
        
        #region old code
        //public ActionResult GetStudentDetail(string CusRegistrationNo, DateTime? DOB, PrintProgramme? programme, MigrationE? formType)
        //{
        //    var studentInfo = new MigrationPI();
        //    if (string.IsNullOrEmpty(CusRegistrationNo))
        //    {
        //        ModelState.AddModelError("CusRegistrationNo", "CusRegistrationNo Required");
        //    }
        //    else if (DOB == null)
        //    {
        //        ModelState.AddModelError("DOB", "DOB Required");
        //    }
        //    else if (programme == null || programme <= 0)
        //    {
        //        ModelState.AddModelError("PrintProgramme", "Programme invalid");
        //    }
        //    else if (formType == null || formType <= 0)
        //    {
        //        ModelState.AddModelError("MigrationE", "Form Type is invalid");
        //    }
        //    else
        //    {
        //        studentInfo = FillDetail(CusRegistrationNo, DOB.Value, programme.Value);

        //        if (studentInfo != null)
        //        {
        //            MigrationPI migrationPI = new MigrationManager().GetMigrationStudent(studentInfo.Student_ID, programme.Value, formType.ToString(), true);
        //            if (migrationPI != null)
        //            {
        //                if (migrationPI.FormStatus == FormStatus.FeePaid)
        //                {
        //                    ModelState.AddModelError("CusRegistrationNo", "CusRegistrationNo Already Applied");
        //                    return Content("RePrint=" + studentInfo.Student_ID);
        //                }
        //                else
        //                {
        //                    new MigrationManager().DeleteMigrationStudent(studentInfo.Student_ID, programme.Value, formType.ToString());

        //                }
        //            }

        //        }
        //        else
        //        {
        //            ModelState.AddModelError("CusRegistrationNo", "Student Not Found");
        //        }
        //    }
        //    SetViewBags(studentInfo, programme.Value, formType.Value);
        //    if (studentInfo == null)
        //    { return Content("<h3 style='color: red'>Student Not Found</h3>"); }
        //    return PartialView("_SubViewMigration", studentInfo);
        //} 
        #endregion

        public ActionResult GetStudentDetail(string CusRegistrationNo, DateTime? DOB, PrintProgramme? programme, MigrationE? formType)
        {
            var studentInfo = new PersonalInformationCompact();
            MigrationPI migrationPI = new MigrationPI();
            if (string.IsNullOrEmpty(CusRegistrationNo))
            {
                ModelState.AddModelError("CusRegistrationNo", "CusRegistrationNo Required");
            }
            else if (DOB == null)
            {
                ModelState.AddModelError("DOB", "DOB Required");
            }
            else if (programme == null || programme <= 0)
            {
                ModelState.AddModelError("PrintProgramme", "Programme invalid");
            }
            else if (formType == null || formType <= 0)
            {
                ModelState.AddModelError("MigrationE", "Form Type is invalid");
            }
            else
            {
                studentInfo = new StudentManager().GetStudent(CusRegistrationNo, DOB.Value, programme.Value, true);

                if (studentInfo != null)
                {
                    migrationPI = new MigrationManager().GetMigrationDetail(studentInfo.Student_ID, programme.Value, formType.ToString());
                    if (migrationPI != null)
                    {
                        OMapping(studentInfo, programme.Value, migrationPI);
                        if (migrationPI.FormStatus == FormStatus.FeePaid)
                        {
                            ModelState.AddModelError("CusRegistrationNo", "CusRegistrationNo Already Applied");
                            return Content("RePrint=" + studentInfo.Student_ID);
                        }
                        else if (!string.IsNullOrEmpty(migrationPI.TransactionNo) && (migrationPI.TransactionDate != null))
                        {
                            if (new MigrationManager().UpdateDownloadForm(migrationPI.Form_ID))
                            {
                                return Content("RePrint=" + studentInfo.Student_ID);
                            }
                        }
                        else
                        {
                            new MigrationManager().DeleteMigrationStudent(studentInfo.Student_ID, programme.Value, formType.ToString());

                        }
                    }
                    else { migrationPI = OMapping(studentInfo, programme.Value, new MigrationPI()); }

                }
                else
                {
                    ModelState.AddModelError("CusRegistrationNo", "Student Not Found");
                }
            }
            SetViewBags(studentInfo?.AcceptCollege_ID ?? Guid.Empty, programme.Value, formType.Value);
            if (studentInfo?.Student_ID == null || studentInfo?.Student_ID == Guid.Empty  || !ModelState.IsValid)
            { return Content("<h3 style='color: red'>Student Not Found</h3>"); }
            return PartialView("_SubViewMigration", migrationPI);
        }

        private MigrationPI OMapping(PersonalInformationCompact studentInfo, PrintProgramme printProgramme, MigrationPI migrationPI)
        {
            migrationPI.Student_ID = studentInfo.Student_ID;
            migrationPI.CurrentSemesterOrYear = studentInfo.CurrentSemesterOrYear;
            migrationPI.CUSRegistrationNo = studentInfo.CUSRegistrationNo;
            migrationPI.AcceptCollege_ID = studentInfo.AcceptCollege_ID==null?Guid.Empty: studentInfo.AcceptCollege_ID.Value;
            migrationPI.Batch = studentInfo.Batch;
            migrationPI.Email = studentInfo.Email;
            migrationPI.Mobile = studentInfo.Mobile;
            migrationPI.ClassRollNo = studentInfo.ClassRollNo;
            migrationPI.FullName = studentInfo.FullName;
            migrationPI.Gender = studentInfo.Gender;
            migrationPI.FathersName = studentInfo.FathersName;
            migrationPI.MothersName = studentInfo.MothersName;
            migrationPI.DOB = studentInfo.DOB;
            migrationPI.Semester = studentInfo.CurrentSemesterOrYear;
            migrationPI.CourseFullName = studentInfo.CourseFullName;
            migrationPI.StudentAddress = new DataManagers.StudentDB().GetStudentAddress(studentInfo.Student_ID, printProgramme);
            Tuple<List<SemesterModel>, int> response = new StudentManager().GetLastSemesterResultDetails(migrationPI, printProgramme);
            migrationPI.SemesterModel = response.Item1;
            ViewBag.LastSemester = response.Item2;
            return migrationPI;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [OutputCache(Duration = 0, Location = System.Web.UI.OutputCacheLocation.None, NoStore = true)]
        public ActionResult MakePayment(MigrationPI studentInfo)
        {
            if (ModelState.ContainsKey("TotalFeeM"))
                ModelState["TotalFeeM"].Errors.Clear();
            if (ModelState.IsValid)
            {
                studentInfo.TotalFeeM = 300;
                Guid id = studentInfo.Student_ID;
                if ((id == null) || (id == Guid.Empty))
                {
                    TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'> Form does not exist.</a></div>";
                    ViewBag.PrintProgrammeIs = studentInfo.Programme;
                    return PartialView("_SubViewMigration", studentInfo);
                }
                int SerialNo = new MigrationManager().GetLastSerialNo();
                studentInfo.Form_ID = Guid.NewGuid();
                studentInfo.SerialNo = SerialNo + 1;
                studentInfo.CreatedBy = id;
                studentInfo.AcceptedBy = id;
                studentInfo.CreatedOn = DateTime.Now;
                studentInfo.FormStatus = FormStatus.InProcess;
                if (new MigrationManager().SaveDownloadForm(studentInfo))
                { return RedirectToAction("Migration", "Payment", new { id = id + "/", PP = ((int)studentInfo.Programme).ToString().EncryptCookieAndURLSafe(), FT = ((int)studentInfo.FormType).ToString().EncryptCookieAndURLSafe(), RS = ((int)studentInfo.ResultStatus).ToString().EncryptCookieAndURLSafe(), NC = studentInfo.NewCollege.EncryptCookieAndURLSafe(), TF = studentInfo.TotalFeeM.ToString().EncryptCookieAndURLSafe(), RK = studentInfo.Form_ID.ToString().EncryptCookieAndURLSafe(), area = string.Empty }); }

            }
            SetViewBags(studentInfo.AcceptCollege_ID, studentInfo.Programme, studentInfo.FormType);
            studentInfo = FillDetail(studentInfo.CUSRegistrationNo, studentInfo.DOB, studentInfo.Programme);
            return View("Migration", studentInfo);
        }

        public void RePrint(string CusRegistrationNo, DateTime DOB, PrintProgramme programme, MigrationE formType)
        {
            var studentInfo = new MigrationManager().GetMigrationStudent(CusRegistrationNo, DOB, programme);

            if (studentInfo != null && studentInfo.SelectedCombinations != null)
            {
                RedirectToAction("Print", new { Student_Id = studentInfo.Student_ID, TypeIs = formType.ToString(), PP = programme });
            }

        }
        public ActionResult Print(Guid Student_Id, string TypeIs, PrintProgramme PP)
        {
            string formType = TypeIs.ToString().Replace("_", "");
            MigrationPI studentInfo = new MigrationManager().GetMigrationStudent(Student_Id, PP, TypeIs, true);
            if (studentInfo != null && studentInfo.FormStatus != FormStatus.FeePaid)
            { return new EmptyResult(); }
            if (studentInfo != null && studentInfo.SelectedCombinations != null)
            {
                ViewBag.CourseName = ((PP == PrintProgramme.PG) && studentInfo.SelectedCombinations.First().CourseID.ToLower().Contains("geography") ? new StudentManager().GetCoursePrefix(studentInfo.SelectedCombinations.First().CourseID, PP, Student_Id, "Graduation") : studentInfo.SelectedCombinations.First().CourseID);
                Tuple<List<SemesterModel>, int> response = new StudentManager().GetLastSemesterResultDetails(studentInfo, PP);
                studentInfo.SemesterModel = response.Item1;
                ViewBag.LastSemester = response?.Item2;


            }
            SetViewBags(studentInfo?.AcceptCollege_ID ?? Guid.Empty, PP, studentInfo.FormType);
            return View("MigrationPrint", studentInfo);
        }

    }
}