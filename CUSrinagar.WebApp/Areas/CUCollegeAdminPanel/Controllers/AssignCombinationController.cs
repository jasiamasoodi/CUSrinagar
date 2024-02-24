using CUSrinagar.BusinessManagers;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using CUSrinagar.Models;
using CUSrinagar.OAuth;
using GeneralModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Data;
using CUSrinagar.DataManagers;

namespace CUSrinagar.WebApp.CUCollegeAdminPanel.Controllers
{
    [OAuthorize(AppRoles.College_AssignCombination)]
    public class AssignCombinationController : CollegeAdminBaseController
    {
        #region ChangeCourse
        public ActionResult ChangeCourse(string Form_RegistrationNumber)
        {
            ARGPersonalInformation stdinfo = null;
            if (!string.IsNullOrEmpty(Form_RegistrationNumber))
            {
                stdinfo = new StudentManager().GetStudentWithCombination(Form_RegistrationNumber, PrintProgramme.IH, true, 3);
                ViewBag.Form_RegistrationNumber = Form_RegistrationNumber;
                if (stdinfo == null || stdinfo.CurrentSemesterOrYear < 2)
                {
                    ViewBag.ErrorMessage = "Student Not found.Please check search filters like program, semester (or try to import if it is new admission)";
                    return View();
                }
                ViewBag.IstYearCourse = new CombinationManager().GetSelectedCombination(stdinfo.Student_ID, PrintProgramme.IH, 1).CourseID;
                Parameters param = new Parameters() { Filters = new List<SearchFilter>() { new SearchFilter() { TableAlias = "ADMCollegeMaster", Column = "College_ID", Value = AppUserHelper.College_ID.Value.ToString() }, new SearchFilter() { Column = "PrintProgramme", Value = ((short)PrintProgramme.IH).ToString() } }, PageInfo = new Paging() { DefaultOrderByColumn = "CourseFullName", PageNumber = -1, PageSize = -1 } };
                var courselist = new CourseManager().List(param)?.Select(x => new SelectListItem { Value = x.Course_ID.ToString(), Text = x.CourseFullName })?.ToList();
                ViewBag.Courses = courselist;
                if (stdinfo.SelectedCombinations.IsNullOrEmpty() || stdinfo.CurrentSemesterOrYear <= 0)
                    stdinfo.SelectedCombinations = new List<ARGSelectedCombination>() { new ARGSelectedCombination()
                    {
                        PrintProgramme = PrintProgramme.IH,
                        College_ID = AppUserHelper.College_ID.Value,
                        Semester = 3,
                        CollegeID = AppUserHelper.CollegeCode,
                        Student_ID = stdinfo.Student_ID,
                        Course_ID = Guid.Parse(courselist.First().Value),
                        SemesterBatch = stdinfo.Batch
                    }};
                ViewBag.CourseSemesterCombinations = new CombinationManager().GetCombinationsDDL(AppUserHelper.College_ID.Value, stdinfo.SelectedCombinations.First().Course_ID, stdinfo.SelectedCombinations.First().Semester);
            }
            GetResponseViewBags();
            return View(stdinfo);
        }

        public ActionResult PostChangeCourse(ARGSelectedCombination model, string ClassRollNo, bool? OverrideClassRollNumber, bool? fromAcceptExamFormScreen, decimal? OfflinePaymentAmount, DateTime? BankPaymentDate, bool? NewAdmission)
        {
            ResponseData response = new ResponseData();
            ARGPersonalInformation stdinfo = null;
            if (ModelState.IsValid)
            {
                response = new AssignCombinationManager().ChangeCourse(model, OfflinePaymentAmount, BankPaymentDate);
                if (!string.IsNullOrWhiteSpace(ClassRollNo))
                {
                    stdinfo = new StudentManager().GetStudent(model.Student_ID, model.PrintProgramme, false);
                    AssignClassRollNo assignClassRollNo = new AssignClassRollNo() { Batch = stdinfo.Batch, ClassRollNo = ClassRollNo, OverrideExistingClassRollNo = OverrideClassRollNumber.Value, Programme = model.PrintProgramme, StudentFormNoOrRegnNo = stdinfo.StudentFormNo };
                    Tuple<int, string> responseRollnumber = new AssignClassRollNoManager().Update(assignClassRollNo);
                    if (responseRollnumber.Item1 <= 0)
                        response.ErrorMessage += responseRollnumber.Item2;
                }
                SetResponseViewBags(response);
            }
            else
                response.ErrorMessage = string.Join(", ", ModelState.Values.SelectMany(s => s.Errors).Select(y => y.ErrorMessage).ToArray());
            return RedirectToAction("ChangeCourse", new { @Form_RegistrationNumber = stdinfo.StudentFormNo });
        }



        #endregion

        #region Lateral_Entry
        [HttpGet]
        public ActionResult GetLateralEntry(short? SemesterBatch, PrintProgramme? PrintProgramme, short? Semester, string StudentFormNo)
        {
            ARGPersonalInformation stdinfo = null;
            if (SemesterBatch.HasValue && PrintProgramme.HasValue && Semester.HasValue && !string.IsNullOrEmpty(StudentFormNo))
            {
                stdinfo = new StudentManager().GetStudentWithCombination(StudentFormNo, PrintProgramme.Value, true, Semester);
                FillViewBags(StudentFormNo, PrintProgramme, Semester);
                if (stdinfo == null)
                {
                    ViewBag.ErrorMessage = "Student Not found.Please check search filters like program, semester (or try to import if it is new admission)";
                    return View();
                }
                if (SetFormDetailViewBags(stdinfo, PrintProgramme.Value, Semester.Value, true) == 0)
                {
                    ViewBag.ErrorMessage = "No combination found for the said course. Please create atleast one combination then proceed";
                    return View();
                }
                if (stdinfo.SelectedCombinations.First().SelectedCombination_ID == Guid.Empty)
                    stdinfo.SelectedCombinations.First().SemesterBatch = SemesterBatch.Value;
            }
            GetResponseViewBags();
            return View(stdinfo);
        }

        [HttpPost]
        public JsonResult ImportFromPGtoIH(string cusformno)
        {
            Tuple<bool, string> response = new AssignCombinationManager().ImportPGtoIH(cusformno);
            return Json(new { IsSuccess = response.Item1, message = response.Item2 }, JsonRequestBehavior.DenyGet);
        }

        public ActionResult InsertUpdateLateralEntryCombination(ARGSelectedCombination model, string ClassRollNo, bool? OverrideClassRollNumber, bool? fromAcceptExamFormScreen, decimal? OfflinePaymentAmount, DateTime? BankPaymentDate, bool? NewAdmission)
        {
            ResponseData response = new ResponseData();
            var stdinfo = new StudentManager().GetStudent(model.Student_ID, model.PrintProgramme, false);
            if (ModelState.IsValid)
            {
                response = new AssignCombinationManager().SaveUpdateCombinationLateralEntry(model, OfflinePaymentAmount, BankPaymentDate);
                if (!string.IsNullOrWhiteSpace(ClassRollNo))
                {
                    AssignClassRollNo assignClassRollNo = new AssignClassRollNo() { Batch = stdinfo.Batch, ClassRollNo = ClassRollNo, OverrideExistingClassRollNo = OverrideClassRollNumber.Value, Programme = model.PrintProgramme, StudentFormNoOrRegnNo = stdinfo.StudentFormNo };
                    Tuple<int, string> responseRollnumber = new AssignClassRollNoManager().Update(assignClassRollNo);
                    if (responseRollnumber.Item1 <= 0)
                        response.ErrorMessage += responseRollnumber.Item2;
                }
                SetResponseViewBags(response);
            }
            else
                response.ErrorMessage = string.Join(", ", ModelState.Values.SelectMany(s => s.Errors).Select(y => y.ErrorMessage).ToArray());
            return RedirectToAction("GetLateralEntry", new { @SemesterBatch = model.SemesterBatch, @PrintProgramme = model.PrintProgramme, @Semester = model.Semester, @StudentFormNo = stdinfo.StudentFormNo });
        }

        #endregion

        public ActionResult AssignCombination(string Form_RegistrationNumber, PrintProgramme? printProg, short? sem, bool? RedirectedFromViewDetail, bool? fromAcceptExamFormScreen, PrintProgramme? printProg1)
        {
            printProg = printProg == null ? printProg1 : printProg;
            FillViewBags(Form_RegistrationNumber, printProg, sem);

            if (Form_RegistrationNumber == null || (printProg == null))
                return View("ViewFormDetail");

            ARGPersonalInformation studentInfo = new StudentDB().GetStudent(Form_RegistrationNumber, printProg.Value);
            if (studentInfo == null) return View("ViewFormDetail");

            if (studentInfo.IsPassout)
            {
                TempData["ErrorMessage"] = "Student is already passout. Hence, no changes are allowed";
                return RedirectToAction("AssignCombination");
            }

            int Batch = studentInfo.Batch;
            int Semester = sem.Value;
            ARGPersonalInformation stdinfo;
            switch (Semester)
            {
                case 1:
                    {
                        if (Batch == 2022 && printProg == PrintProgramme.UG)
                        {
                            stdinfo = ViewFormDetailNEP(Form_RegistrationNumber, printProg, sem, RedirectedFromViewDetail, fromAcceptExamFormScreen, printProg1);
                            return View("ViewFormDetailNEP", stdinfo);
                        }
                        if (Batch >= 2023 && printProg == PrintProgramme.UG)
                        {
                            stdinfo = ViewFormDetailNEPCUET(Form_RegistrationNumber, printProg, sem, RedirectedFromViewDetail, fromAcceptExamFormScreen, printProg1);
                            return View("ViewFormDetailNEPCUET", stdinfo);
                        }
                        if (printProg == PrintProgramme.PG)
                        {
                            stdinfo = ViewFormDetail(Form_RegistrationNumber, printProg, sem, printProg1);


                            if (SetFormDetailViewBags(stdinfo, printProg.Value, sem.Value) == 0)
                            {
                                TempData["ErrorMessage"] = "No combination found for the said course. Please create atleast one combination then proceed";
                                return RedirectToAction("AssignCombination");
                            }
                            ViewBag.Course = ((List<SelectListItem>)(ViewBag.Courses)).First().Value;
                            if (stdinfo.AcceptCollege_ID != null)
                            {
                                ViewBag.Course = new StudentManager().GetStudentCurrentCourse(PrintProgramme.PG, stdinfo.Student_ID);
                            }
                            return View("ViewFormDetailPG", stdinfo);
                        }
                        else
                        {
                            stdinfo = ViewFormDetail(Form_RegistrationNumber, printProg, sem, printProg1);

                            if (new ApplicationFormsManager().DegreeAlreadyPrinted(stdinfo.Student_ID))
                            {
                                TempData["ErrorMessage"] = "It seems that degree certificate has been printed for the student. Hence, no changes are allowed";
                                return RedirectToAction("AssignCombination");
                            }
                            else if (SetFormDetailViewBags(stdinfo, printProg.Value, sem.Value) == 0)
                            {
                                TempData["ErrorMessage"] = "No combination found for the said course. Please create atleast one combination then proceed";
                                return RedirectToAction("AssignCombination");
                            }
                            return View("ViewFormDetail", stdinfo);
                        }
                    }
                default:
                    {
                        stdinfo = CombinationRollNumber(Form_RegistrationNumber, printProg, sem, RedirectedFromViewDetail, fromAcceptExamFormScreen, printProg1);
                        return View("CombinationRollNumber", stdinfo);
                    }
            }

        }
        public PartialViewResult GetPGView(Guid Course_ID, int semester, string Form_RegistrationNumber)
        {
            ARGPersonalInformation stdinfo = PGCombinationRollNumber(Form_RegistrationNumber, Course_ID, PrintProgramme.PG, (short)semester);
            return PartialView("PGSubjectsRegion", stdinfo);
        }

        public ARGPersonalInformation ViewFormDetail(string Form_RegistrationNumber, PrintProgramme? printProg, short? sem, PrintProgramme? printProg1)
        {
            ARGPersonalInformation stdinfo = null;
            if (printProg1.HasValue) printProg = printProg1;
            if (Form_RegistrationNumber.IsNotNullOrEmpty() && printProg.HasValue && sem.HasValue)
                stdinfo = new StudentManager().GetStudentWithCombination(Form_RegistrationNumber, printProg.Value, true, sem);
            FillViewBags(Form_RegistrationNumber, printProg, sem);

            GetResponseViewBags();
            return stdinfo;
        }


        #region NEP CUET
        public ARGPersonalInformation ViewFormDetailNEPCUET(string Form_RegistrationNumber, PrintProgramme? printProg, short? sem, bool? RedirectedFromViewDetail, bool? fromAcceptExamFormScreen, PrintProgramme? printProg1)
        {
            ViewBag.fromAcceptExamFormScreen = fromAcceptExamFormScreen;
            if (printProg1.HasValue) printProg = printProg1.Value;
            if (Form_RegistrationNumber.IsNotNullOrEmpty() && fromAcceptExamFormScreen.HasValue && fromAcceptExamFormScreen.Value && Form_RegistrationNumber.ToLower().Contains("exm"))
                Form_RegistrationNumber = new ExaminationFormManager().GetRegistrationNoOrFormNumber(printProg.Value, Form_RegistrationNumber);

            FillViewBags(Form_RegistrationNumber, printProg, sem ?? 0);
            ARGPersonalInformation STDINFO = null;
            GetResponseViewBags();

            if (Form_RegistrationNumber.IsNotNullOrEmpty())
            {
                STDINFO = new StudentManager().GetStudentWithSelectionDetail(Form_RegistrationNumber, printProg.Value, false, sem);
                if (STDINFO != null)
                {
                    Guid Course_ID; short SemesterBatch;
                    STDINFO.AssignedCollege_ID = AppUserHelper.College_ID;

                    Course_ID = STDINFO.AssignedCourse_ID;
                    SemesterBatch = STDINFO.Batch;
                    var _CSManager = new CombinationSettingManager();

                    STDINFO.CoursesApplied = new RegistrationDB().GetStudentCoursesApplied(STDINFO.Student_ID, printProg.Value);
                    CombinationSetting _CombinationSetting;

                    Guid CurrentCourse_ID = new StudentManager().GetStudentCurrentCourse(printProg.Value, STDINFO.Student_ID);

                    if (CurrentCourse_ID != Guid.Empty)
                    {
                        _CombinationSetting = _CSManager.GetCombinationSetting(CurrentCourse_ID, sem ?? 0, SemesterBatch);
                    }
                    else
                    {
                        _CombinationSetting = _CSManager.GetCombinationSetting(STDINFO.CoursesApplied.First().Course_ID, sem ?? 0, SemesterBatch);
                    }
                    ViewBag.CourseName = new CourseDB().GetItem(CurrentCourse_ID)?.CourseFullName ?? "";

                    if (_CombinationSetting != null && _CombinationSetting.CombinationSettingStructure != null)
                    {
                        ViewBag.CombinationSetting = _CombinationSetting;
                        var response = new AssignCombinationManager().GetSubjectListOptionsNEP(_CombinationSetting, STDINFO, sem ?? 1);
                        if (response.Item3.IsSuccess)
                        {
                            ViewBag.AssignCombinationViewModel = new AssignCombinationManager().GetSubjectListOptionsNEP(_CombinationSetting, STDINFO, sem ?? 1).Item2;
                            if (_CombinationSetting.CombinationSettingStructure.AllowInterCollegeElective && _CombinationSetting.CombinationSettingStructure.GE_OE_Count > 0)
                            {
                                ViewBag.Programme = new CourseManager().GetPrintProgrammesInClause(Course_ID);
                                FillViewBag_College();
                            }
                            ViewBag.StudentAddionalSubjects = new StudentManager().GetStudentAdditionalSubjects(STDINFO.Student_ID, sem ?? 1, printProg.Value);
                            return STDINFO;
                        }
                        else
                        {
                            ViewBag.ErrorMessage = response.Item3.ErrorMessage;

                        }
                    }
                    else
                        ViewBag.ErrorMessage = $"Combination Setting not available. Please contact Cluster University I.T Section.";

                }
            }

            return STDINFO;
        }
        public ActionResult PostSubjectCombinationNEPCUET(Guid? Student_ID, short? Semester, PrintProgramme? PrintProgramme, List<Guid> subject,
       List<Guid> additionalSubject, string ClassRollNo, bool? OverrideClassRollNumber, string Form_RegistrationNumber, short? SemesterBatch,
       bool? fromAcceptExamFormScreen, short? Batch, Decimal? OfflinePaymentAmount, DateTime? BankPaymentDate)
        {
            List<ADMSubjectMaster> subjectmasterlist = new List<ADMSubjectMaster>();
            List<ADMSubjectMaster> addionalSubjectmasterlist = new List<ADMSubjectMaster>();

            AssignCombinationManager assignClassRollNoManager = new AssignCombinationManager();

            if (subject.IsNotNullOrEmpty()) foreach (var item in subject.Where(x => x != Guid.Empty)) subjectmasterlist.Add(new ADMSubjectMaster() { Subject_ID = item });
            if (additionalSubject.IsNotNullOrEmpty()) foreach (var item in additionalSubject.Where(x => x != Guid.Empty)) addionalSubjectmasterlist.Add(new ADMSubjectMaster() { Subject_ID = item });

            //get course ID by major subject
            Guid? ACourse_ID = assignClassRollNoManager
                .GetCourseIDByMajorSubjectSemester1(subjectmasterlist.Select(x => x.Subject_ID).ToList());

            var response = assignClassRollNoManager.SaveUpdateStudentCombinationNEPCUET(PrintProgramme ?? Enums.PrintProgramme.UG, Student_ID ?? Guid.Empty, Semester ?? 0, subjectmasterlist, SemesterBatch ?? 0, ACourse_ID ?? Guid.Empty, OfflinePaymentAmount, BankPaymentDate, addionalSubjectmasterlist);
            if (!string.IsNullOrWhiteSpace(ClassRollNo) && Form_RegistrationNumber.IsNotNullOrEmpty())
            {
                AssignClassRollNo assignClassRollNo = new AssignClassRollNo() { Batch = Batch ?? 0, ClassRollNo = ClassRollNo, OverrideExistingClassRollNo = OverrideClassRollNumber.Value, Programme = PrintProgramme ?? Enums.PrintProgramme.UG, StudentFormNoOrRegnNo = Form_RegistrationNumber.Trim() };
                Tuple<int, string> responseRollnumber = new AssignClassRollNoManager().Update(assignClassRollNo);
                if (responseRollnumber.Item1 <= 0)
                    response.ErrorMessage += responseRollnumber.Item2;
            }

            SetResponseViewBags(response);
            return RedirectToAction("AssignCombination", new
            {
                Form_RegistrationNumber = Form_RegistrationNumber,
                sem = Semester,
                RedirectedFromViewDetail = false,
                printProg = PrintProgramme,
                fromAcceptExamFormScreen = fromAcceptExamFormScreen
            });
        }
        #endregion


        #region NEP
        public ARGPersonalInformation ViewFormDetailNEP(string Form_RegistrationNumber, PrintProgramme? printProg, short? sem, bool? RedirectedFromViewDetail, bool? fromAcceptExamFormScreen, PrintProgramme? printProg1)
        {
            ViewBag.fromAcceptExamFormScreen = fromAcceptExamFormScreen;
            if (printProg1.HasValue) printProg = printProg1.Value;
            if (Form_RegistrationNumber.IsNotNullOrEmpty() && fromAcceptExamFormScreen.HasValue && fromAcceptExamFormScreen.Value && Form_RegistrationNumber.ToLower().Contains("exm"))
                Form_RegistrationNumber = new ExaminationFormManager().GetRegistrationNoOrFormNumber(printProg.Value, Form_RegistrationNumber);

            FillViewBags(Form_RegistrationNumber, printProg, sem ?? 0);
            ARGPersonalInformation STDINFO = null;
            GetResponseViewBags();
            if (Form_RegistrationNumber.IsNotNullOrEmpty())
            {
                STDINFO = new StudentManager().GetStudentWithSelectionDetail(Form_RegistrationNumber, printProg.Value, false, sem);
                if (STDINFO != null && STDINFO.AssignedCourse_ID != null)
                {
                    Guid Course_ID; short SemesterBatch;
                    if (STDINFO.AssignedCollege_ID != null && STDINFO.AssignedCollege_ID != AppUserHelper.College_ID)
                    {
                        ViewBag.ErrorMessage = $"Students final selection not in this college.";
                    }
                    Course_ID = STDINFO.AssignedCourse_ID; SemesterBatch = STDINFO.Batch;
                    var _CSManager = new CombinationSettingManager();
                    var _CombinationSetting = _CSManager.GetCombinationSetting(Course_ID, sem ?? 0, SemesterBatch);

                    if (_CombinationSetting != null && _CombinationSetting.CombinationSettingStructure != null)
                    {
                        ViewBag.CombinationSetting = _CombinationSetting;
                        var response = new AssignCombinationManager().GetSubjectListOptionsNEP(_CombinationSetting, STDINFO, sem ?? 1);
                        if (response.Item3.IsSuccess)
                        {
                            ViewBag.AssignCombinationViewModel = new AssignCombinationManager().GetSubjectListOptionsNEP(_CombinationSetting, STDINFO, sem ?? 1).Item2;
                            if (_CombinationSetting.CombinationSettingStructure.AllowInterCollegeElective && _CombinationSetting.CombinationSettingStructure.GE_OE_Count > 0)
                            {
                                ViewBag.Programme = new CourseManager().GetPrintProgrammesInClause(Course_ID);
                                FillViewBag_College();
                            }
                            ViewBag.StudentAddionalSubjects = new StudentManager().GetStudentAdditionalSubjects(STDINFO.Student_ID, sem ?? 1, printProg.Value);
                            return STDINFO;
                        }
                        else
                        {
                            ViewBag.ErrorMessage = response.Item3.ErrorMessage;

                        }
                    }
                    else
                        ViewBag.ErrorMessage = $"Combination Setting not available. Please contact Cluster University I.T Section.";

                }
            }

            return STDINFO;
        }
        public ActionResult PostSubjectCombinationNEP(Guid? Student_ID, short? Semester, PrintProgramme? PrintProgramme, List<Guid> subject,
       List<Guid> additionalSubject, string ClassRollNo, bool? OverrideClassRollNumber, string Form_RegistrationNumber, short? SemesterBatch,
       bool? fromAcceptExamFormScreen, Guid? ACourse_ID, short? Batch, Decimal? OfflinePaymentAmount, DateTime? BankPaymentDate)
        {
            List<ADMSubjectMaster> subjectmasterlist = new List<ADMSubjectMaster>(); List<ADMSubjectMaster> addionalSubjectmasterlist = new List<ADMSubjectMaster>();
            if (subject.IsNotNullOrEmpty()) foreach (var item in subject.Where(x => x != Guid.Empty)) subjectmasterlist.Add(new ADMSubjectMaster() { Subject_ID = item });
            if (additionalSubject.IsNotNullOrEmpty()) foreach (var item in additionalSubject.Where(x => x != Guid.Empty)) addionalSubjectmasterlist.Add(new ADMSubjectMaster() { Subject_ID = item });

            var response = new AssignCombinationManager().SaveUpdateStudentCombinationNEP(PrintProgramme ?? Enums.PrintProgramme.UG, Student_ID ?? Guid.Empty, Semester ?? 0, subjectmasterlist, SemesterBatch ?? 0, ACourse_ID ?? Guid.Empty, OfflinePaymentAmount, BankPaymentDate, addionalSubjectmasterlist);
            if (!string.IsNullOrWhiteSpace(ClassRollNo) && Form_RegistrationNumber.IsNotNullOrEmpty())
            {
                AssignClassRollNo assignClassRollNo = new AssignClassRollNo() { Batch = Batch ?? 0, ClassRollNo = ClassRollNo, OverrideExistingClassRollNo = OverrideClassRollNumber.Value, Programme = PrintProgramme ?? Enums.PrintProgramme.UG, StudentFormNoOrRegnNo = Form_RegistrationNumber.Trim() };
                Tuple<int, string> responseRollnumber = new AssignClassRollNoManager().Update(assignClassRollNo);
                if (responseRollnumber.Item1 <= 0)
                    response.ErrorMessage += responseRollnumber.Item2;
            }

            SetResponseViewBags(response);
            return RedirectToAction("AssignCombination", new
            {
                Form_RegistrationNumber = Form_RegistrationNumber,
                sem = Semester,
                RedirectedFromViewDetail = false,
                printProg = PrintProgramme,
                fromAcceptExamFormScreen = fromAcceptExamFormScreen
            });
        }
        #endregion
        public ARGPersonalInformation PGCombinationRollNumber(string Form_RegistrationNumber, Guid Course_Id, PrintProgramme? printProg, short? sem)
        {
            ViewBag.fromAcceptExamFormScreen = false;

            FillViewBags(Form_RegistrationNumber, printProg, sem ?? 0);
            ARGPersonalInformation STDINFO = null;
            GetResponseViewBags();
            if (Form_RegistrationNumber.IsNotNullOrEmpty())
            {
                STDINFO = new StudentManager().GetStudentWithCombination(Form_RegistrationNumber, printProg.Value, false);
                if (STDINFO != null)
                {
                    Guid Course_ID; short SemesterBatch;

                    Course_ID = Course_Id; SemesterBatch = STDINFO.Batch;
                    var _CSManager = new CombinationSettingManager();
                    var _CombinationSetting = _CSManager.GetCombinationSetting(Course_ID, sem ?? 0, SemesterBatch);
                    if (_CombinationSetting != null && _CombinationSetting.CombinationSettingStructure != null)
                    {
                        ViewBag.CombinationSetting = _CombinationSetting;
                        var response = new AssignCombinationManager().GetSubjectListOptions(_CombinationSetting, STDINFO, sem ?? 0);
                        if (response.Item3.IsSuccess)
                        {
                            ViewBag.AssignCombinationViewModel = response.Item2;
                            if (_CombinationSetting.CombinationSettingStructure.AllowInterCollegeElective && _CombinationSetting.CombinationSettingStructure.GE_OE_Count > 0)
                            {
                                ViewBag.Programme = new CourseManager().GetPrintProgrammesInClause(Course_ID);
                                FillViewBag_College();
                            }

                            List<StudentAdditionalSubject> additionalSubjects =
                                new StudentManager().GetStudentAdditionalSubjects(STDINFO.Student_ID, sem ?? 0, printProg.Value);

                            if (additionalSubjects.IsNotNullOrEmpty())
                            {
                                foreach (ARGSelectedCombination item in STDINFO.SelectedCombinations ?? new List<ARGSelectedCombination>())
                                {
                                    item.Subjects?.AddRange(additionalSubjects?.Where(x => x.Semester == item.Semester)
                                        ?.ToList()?.Select(x => x.Subject) ?? new List<ADMSubjectMaster>());
                                }
                            }
                            ViewBag.StudentAddionalSubjects = additionalSubjects;

                            return STDINFO;
                        }
                        else
                        {
                            ViewBag.ErrorMessage = response.Item3.ErrorMessage;
                        }
                    }
                    else
                        ViewBag.ErrorMessage = $"Combination Setting not available. Please contact Cluster University I.T Section.";

                }
            }

            return STDINFO;
        }

        #region Semester1_Onwards_AssignCombination

        public ARGPersonalInformation CombinationRollNumber(string Form_RegistrationNumber, PrintProgramme? printProg, short? sem, bool? RedirectedFromViewDetail, bool? fromAcceptExamFormScreen, PrintProgramme? printProg1)
        {
            ViewBag.fromAcceptExamFormScreen = fromAcceptExamFormScreen;
            if (printProg1.HasValue) printProg = printProg1.Value;
            if (Form_RegistrationNumber.IsNotNullOrEmpty() && fromAcceptExamFormScreen.HasValue && fromAcceptExamFormScreen.Value && Form_RegistrationNumber.ToLower().Contains("exm"))
                Form_RegistrationNumber = new ExaminationFormManager().GetRegistrationNoOrFormNumber(printProg.Value, Form_RegistrationNumber);

            FillViewBags(Form_RegistrationNumber, printProg, sem ?? 0);
            ARGPersonalInformation STDINFO = null;
            GetResponseViewBags();
            if (Form_RegistrationNumber.IsNotNullOrEmpty())
            {
                STDINFO = new StudentManager().GetStudentWithCombination(Form_RegistrationNumber, printProg.Value, false);
                if (STDINFO != null && STDINFO.SelectedCombinations.IsNotNullOrEmpty())
                {
                    Guid Course_ID; short SemesterBatch;
                    ARGSelectedCombination _Comb = STDINFO.SelectedCombinations.FirstOrDefault(x => x.Semester == (sem ?? 0));
                    if (_Comb == null)
                        _Comb = STDINFO.SelectedCombinations.OrderByDescending(x => x.Semester).First();
                    if (_Comb != null)
                    {
                        Course_ID = _Comb.Course_ID; SemesterBatch = _Comb.SemesterBatch;
                        var _CSManager = new CombinationSettingManager();
                        var _CombinationSetting = _CSManager.GetCombinationSetting(Course_ID, sem ?? 0, SemesterBatch);
                        if (_CombinationSetting != null && _CombinationSetting.CombinationSettingStructure != null)
                        {
                            ViewBag.CombinationSetting = _CombinationSetting;
                            var response = new AssignCombinationManager().GetSubjectListOptions(_CombinationSetting, STDINFO, sem ?? 0);
                            if (response.Item3.IsSuccess)
                            {
                                ViewBag.AssignCombinationViewModel = response.Item2;
                                if (_CombinationSetting.CombinationSettingStructure.AllowInterCollegeElective && _CombinationSetting.CombinationSettingStructure.GE_OE_Count > 0)
                                {
                                    ViewBag.Programme = new CourseManager().GetPrintProgrammesInClause(_Comb.Course_ID);
                                    FillViewBag_College();
                                }

                                List<StudentAdditionalSubject> additionalSubjects =
                                    new StudentManager().GetStudentAdditionalSubjects(STDINFO.Student_ID, sem ?? 0, printProg.Value);

                                if (additionalSubjects.IsNotNullOrEmpty())
                                {
                                    foreach (ARGSelectedCombination item in STDINFO.SelectedCombinations ?? new List<ARGSelectedCombination>())
                                    {
                                        item.Subjects?.AddRange(additionalSubjects?.Where(x => x.Semester == item.Semester)
                                            ?.ToList()?.Select(x => x.Subject) ?? new List<ADMSubjectMaster>());
                                    }
                                }
                                ViewBag.StudentAddionalSubjects = additionalSubjects;

                                return STDINFO;
                            }
                            else
                            {
                                ViewBag.ErrorMessage = response.Item3.ErrorMessage;
                            }
                        }
                        else
                            ViewBag.ErrorMessage = $"Combination Setting not available. Please contact Cluster University I.T Section.";
                    }
                    else
                        ViewBag.ErrorMessage = $"Previous Semester Combination not found.";
                }
            }

            return STDINFO;
        }
        public ActionResult PostSubjectCombinationRollNumberPG(Guid? Course_ID, Guid? Student_ID, short? Semester, PrintProgramme? PrintProgramme, List<Guid> subject,
           List<Guid> additionalSubject, string ClassRollNo, bool? OverrideClassRollNumber, string Form_RegistrationNumber, short? SemesterBatch,
           bool? fromAcceptExamFormScreen, short? Batch, Decimal? OfflinePaymentAmount, DateTime? BankPaymentDate)
        {

            List<ADMSubjectMaster> subjectmasterlist = new List<ADMSubjectMaster>(); List<ADMSubjectMaster> addionalSubjectmasterlist = new List<ADMSubjectMaster>();
            if (subject.IsNotNullOrEmpty()) foreach (var item in subject.Where(x => x != Guid.Empty)) subjectmasterlist.Add(new ADMSubjectMaster() { Subject_ID = item });
            if (additionalSubject.IsNotNullOrEmpty()) foreach (var item in additionalSubject.Where(x => x != Guid.Empty)) addionalSubjectmasterlist.Add(new ADMSubjectMaster() { Subject_ID = item });

            var response = new AssignCombinationManager().SaveUpdateStudentCombinationPG(Course_ID ?? Guid.Empty, PrintProgramme ?? Enums.PrintProgramme.UG, Student_ID ?? Guid.Empty, Semester ?? 0, subjectmasterlist, SemesterBatch ?? 0, OfflinePaymentAmount, BankPaymentDate, addionalSubjectmasterlist);
            if (!string.IsNullOrWhiteSpace(ClassRollNo) && Form_RegistrationNumber.IsNotNullOrEmpty())
            {
                AssignClassRollNo assignClassRollNo = new AssignClassRollNo() { Batch = Batch ?? 0, ClassRollNo = ClassRollNo, OverrideExistingClassRollNo = OverrideClassRollNumber.Value, Programme = PrintProgramme ?? Enums.PrintProgramme.UG, StudentFormNoOrRegnNo = Form_RegistrationNumber.Trim() };
                Tuple<int, string> responseRollnumber = new AssignClassRollNoManager().Update(assignClassRollNo);
                if (responseRollnumber.Item1 <= 0)
                    response.ErrorMessage += responseRollnumber.Item2;
            }

            SetResponseViewBags(response);


            SetResponseViewBags(response);
            return RedirectToAction("AssignCombination", new
            {
                Form_RegistrationNumber = Form_RegistrationNumber,
                sem = Semester,
                RedirectedFromViewDetail = false,
                printProg = PrintProgramme,
                fromAcceptExamFormScreen = fromAcceptExamFormScreen
            });
        }

        public ActionResult PostSubjectCombinationRollNumber(Guid? Student_ID, short? Semester, PrintProgramme? PrintProgramme, List<Guid> subject,
            List<Guid> additionalSubject, string ClassRollNo, bool? OverrideClassRollNumber, string Form_RegistrationNumber, short? SemesterBatch,
            bool? fromAcceptExamFormScreen, short? Batch)
        {
            List<ADMSubjectMaster> subjectmasterlist = new List<ADMSubjectMaster>(); List<ADMSubjectMaster> addionalSubjectmasterlist = new List<ADMSubjectMaster>();
            if (subject.IsNotNullOrEmpty()) foreach (var item in subject.Where(x => x != Guid.Empty)) subjectmasterlist.Add(new ADMSubjectMaster() { Subject_ID = item });
            if (additionalSubject.IsNotNullOrEmpty()) foreach (var item in additionalSubject.Where(x => x != Guid.Empty)) addionalSubjectmasterlist.Add(new ADMSubjectMaster() { Subject_ID = item });

            var response = new AssignCombinationManager().SaveUpdateStudentCombination(PrintProgramme ?? Enums.PrintProgramme.UG, Student_ID ?? Guid.Empty, Semester ?? 0, subjectmasterlist, SemesterBatch ?? 0, addionalSubjectmasterlist);
            if (!string.IsNullOrWhiteSpace(ClassRollNo) && Form_RegistrationNumber.IsNotNullOrEmpty())
            {
                AssignClassRollNo assignClassRollNo = new AssignClassRollNo() { Batch = Batch ?? 0, ClassRollNo = ClassRollNo, OverrideExistingClassRollNo = OverrideClassRollNumber.Value, Programme = PrintProgramme ?? Enums.PrintProgramme.UG, StudentFormNoOrRegnNo = Form_RegistrationNumber.Trim() };
                Tuple<int, string> responseRollnumber = new AssignClassRollNoManager().Update(assignClassRollNo);
                if (responseRollnumber.Item1 <= 0)
                    response.ErrorMessage += responseRollnumber.Item2;
            }
            SetResponseViewBags(response);
            return RedirectToAction("AssignCombination", new
            {
                Form_RegistrationNumber = Form_RegistrationNumber,
                sem = Semester,
                RedirectedFromViewDetail = false,
                printProg = PrintProgramme,
                fromAcceptExamFormScreen = fromAcceptExamFormScreen
            });
        }

        #endregion

        #region Semester1_Assign_Combination
        public ActionResult InsertUpdateCombination(ARGSelectedCombination model, string ClassRollNo, bool? OverrideClassRollNumber, bool? fromAcceptExamFormScreen, decimal? OfflinePaymentAmount, DateTime? BankPaymentDate)
        {
            ResponseData response = new ResponseData();
            var stdinfo = new StudentManager().GetStudent(model.Student_ID, model.PrintProgramme, false);
            string Form_RegistrationNumber = stdinfo.CUSRegistrationNo.IsNotNullOrEmpty() ? stdinfo.CUSRegistrationNo : stdinfo.StudentFormNo;
            if (ModelState.IsValid)
            {
                response = new AssignCombinationManager().SaveUpdateCombinationToSemester1(model, OfflinePaymentAmount, BankPaymentDate);
                if (!string.IsNullOrWhiteSpace(ClassRollNo) && response.IsSuccess)
                {
                    AssignClassRollNo assignClassRollNo = new AssignClassRollNo() { Batch = stdinfo.Batch, ClassRollNo = ClassRollNo, OverrideExistingClassRollNo = OverrideClassRollNumber.Value, Programme = model.PrintProgramme, StudentFormNoOrRegnNo = stdinfo.StudentFormNo };
                    Tuple<int, string> responseRollnumber = new AssignClassRollNoManager().Update(assignClassRollNo);
                    if (responseRollnumber.Item1 <= 0)
                        response.ErrorMessage += responseRollnumber.Item2;
                }
                SetResponseViewBags(response);
            }
            else
                response.ErrorMessage = string.Join(", ", ModelState.Values.SelectMany(s => s.Errors).Select(y => y.ErrorMessage).ToArray());
            return RedirectToAction("AssignCombination", new { @Form_RegistrationNumber = Form_RegistrationNumber, printProg = model.PrintProgramme, @sem = model.Semester, @fromAcceptExamFormScreen = fromAcceptExamFormScreen, Batch = model.SemesterBatch });
        }


        #endregion

        #region BatchUpdateCombination
        //public JsonResult BatchUpdateSemesterCombination(BatchUpdatePersonalInfo model, PrintProgramme? printProgramme)
        //{
        //    ResponseData response = new AssignCombinationManager().BatchUpdateStudentCombination(model, printProgramme.Value);
        //    return Json(response);
        //}

        public JsonResult BatchUpdateNextSemesterCombination(BatchUpdatePersonalInfo model, PrintProgramme? printProgramme)
        {
            ResponseData response = new AssignCombinationManager().BatchUpdateNextSemesterStudentCombination(model, printProgramme.Value);
            return Json(response);
        }

        public JsonResult ToggleVerifyCombination(List<Guid> Student_IDs, PrintProgramme printProgramme, short Semester)
        {
            ResponseData response = new AssignCombinationManager().ToggleVerifyCombination(Student_IDs, printProgramme, Semester);
            return Json(response);
        }


        #endregion

        #region StudentList
        public ActionResult Forms()
        {
            FillViewBag_Programmes(AppUserHelper.College_ID);
            FillViewBag_Course(AppUserHelper.College_ID, null, Programme.UG);
            FillViewBag_Semesters();
            return View();
        }

        public ActionResult FormListPartial(Parameters parameter, PrintProgramme? otherParam1)
        {
            ViewBag.PrintProgramme = otherParam1;
            if (parameter == null || otherParam1 == null || otherParam1 == 0)
            {
                return View(new List<CourseWiseList>());
            }
            else
            {
                List<CourseWiseList> list = new AssignCombinationManager().GetStudentDetails(parameter, otherParam1.Value)?.DistinctBy(x => x.Student_ID)?.ToList();
                return View(list);
            }
        }

        public void StudentListCSV(Parameters parameter, PrintProgramme? printProgramme)
        {
            string Subject_ID = parameter.Filters.FirstOrDefault(x => x.Column.ToLower().Trim() == "subject_id")?.Value;

            bool hasSubjectSelected = !Subject_ID.IsNotNullOrEmpty();

            List<CourseWiseList> list = new AssignCombinationManager().GetStudentDetails(parameter, printProgramme.Value)?.DistinctBy(x => x.Student_ID)?.ToList();
            var reportList = list.Select(x => new
            {
                Programe = Helper.GetEnumDescription(x.Programme),
                x.CourseFullName,
                x.ClassRollNo,
                x.CUSRegistrationNo,
                x.FullName,
                x.Mobile,
                x.Email,
                x.Gender,
                x.Religion,
                x.CombinationCode,
                x.IsVerified,
                x.Semester,
                x.FathersName,
                x.StudentFormNo,
                x.EntranceRollNo,
                x.Category,
                Status = Helper.GetEnumDescription(x.FormStatus),
                PaymentDate = x.TxnDate.HasValue ? x.TxnDate.Value.ToString("dd-MMM-yyyy") : "",
                AmountPaid = x.TxnAmount,
                ModuleType = x.ModuleType.ToString(),
                PaymentType = x.PaymentType.ToString(),
            }).ToList();

            if (Guid.TryParse(Subject_ID + "", out Guid Subject_Id))
            {
                Subject_ID = "Studentlist";
                ADMSubjectMaster subject = new SubjectsManager().Get(Subject_Id);
                if (subject != null && !string.IsNullOrWhiteSpace(subject.SubjectFullName))
                {
                    Subject_ID = subject.SubjectFullName;
                    Subject_ID += $"({list.First().SubjectType.GetEnumDescription()})".Replace("<", "")
                                                        .Replace(">", "")
                                                        .Replace(":", "")
                                                        .Replace("\"", "")
                                                        .Replace("/", "")
                                                        .Replace("\\", "")
                                                        .Replace("|", "")
                                                        .Replace("?", "")
                                                        .Replace("*", "")
                                                        .Replace("'", "")
                                                        .Replace("[", "")
                                                        .Replace("]", "")
                                                        .Replace("|", "")
                                                        .Replace(".", "")
                                                        .Replace(",", "")
                                                        .Replace("+", "")
                                                        .Replace("/", "")
                                                        .Replace("%", "")
                                                        .Replace("`", "")
                                                        .Replace("~", "")
                                                        .Replace("&", "");
                }
            }
            else
            {
                Subject_ID = "StudentList (ALL Subjects)";
            }

            ExportToCSV(reportList, Subject_ID + printProgramme.ToString() + DateTime.Now.ToString("dd_mm_yy"));
        }

        #endregion

        public JsonResult ReleaseStudent(Guid Student_ID, PrintProgramme printProgramme, short Semester)
        {
            ResponseData response = new AssignCombinationManager().DeleteStudentCombinations(Student_ID, printProgramme, Semester);
            return Json(response);
        }
        public JsonResult SubjectCombinationDDL(Guid? course_id)
        {
            if (course_id.HasValue)
            {
                var list = new CombinationManager().GetCombinationsDDL(AppUserHelper.College_ID.Value, course_id.Value);
                return Json(list);
            }
            return null;
        }

        public JsonResult GetSubjectCombinationCount(Guid? Combination_ID, short? Semester, short? Batch, PrintProgramme? printProgramme)
        {
            if (Combination_ID.HasValue && Semester.HasValue && printProgramme.HasValue && Batch.HasValue)
            {
                int count = new AssignCombinationManager().GetCombinationCount(Combination_ID.Value, Semester.Value, Batch.Value, printProgramme.Value);
                return Json(count);
            }
            return null;
        }
        public JsonResult GetCoursePoints(Guid? Course_ID, Guid Student_ID)
        {
            if (Course_ID.HasValue)
            {
                var course = new StudentDB().GetStudentCoursesApplied(Student_ID, PrintProgramme.IH)?.FirstOrDefault(x => x.Course_ID == Course_ID);
                if (course == null) return null;
                return Json(course);
            }
            return null;
        }
        public JsonResult GetCoursePointsPG(Guid? Course_ID, Guid Student_ID)
        {
            if (Course_ID.HasValue)
            {
                var course = new StudentDB().GetStudentCoursesApplied(Student_ID, PrintProgramme.PG)?.FirstOrDefault(x => x.Course_ID == Course_ID);
                if (course == null) return null;
                return Json(course);
            }
            return null;
        }
        public JsonResult GetStudentRegisteredCount(Guid? SubjectCombination_ID)
        {
            //if (SubjectCombination_ID.HasValue)
            //{
            //    int count = new AssignCombinationManager().GetActualCombinationCount(SubjectCombination_ID.Value, AppUserHelper.College_ID.Value);
            //    return Json(count);
            //}
            return null;
        }


        [HttpPost]
        public ActionResult DownLoadExcel(Parameters parameter, PrintProgramme? printProgramme)
        {
            DataTable _DataTable = new AssignCombinationManager().GetStudentsListAsDataTable(parameter);
            if (_DataTable != null)
            {
                _DataTable.Columns.Remove("Student_ID");
                return DownloadExcel(_DataTable, new AssignCombinationManager().GetFileNameForCombination(parameter.Filters));
            }
            else
                return DownloadExcel<object>(null, null, null);
        }
        [HttpPost]
        public ActionResult DownLoadExcelS(Parameters parameter, PrintProgramme? printProgramme)
        {
            DataTable _DataTable = new AssignCombinationManager().GetSubjectsCountE(parameter);
            if (_DataTable != null)
            {
                return DownloadExcel(_DataTable, new AssignCombinationManager().GetFileNameForCombination(parameter.Filters));
            }
            else
                return DownloadExcel<object>(null, null, null);
        }
        public PartialViewResult _GetChildDDL(string ProgrammeId, string CourseId, string Type, int Semester, string childType, string childSubType)
        {
            ViewBag.ReturnPath = Request.UrlReferrer.ToString();
            PrintProgramme programme = (PrintProgramme)Convert.ToInt32(ProgrammeId);
            ViewBag.Type = Type;
            ViewBag.ChildType = childType;
            ViewBag.ChildSubType = childSubType;
            if (Guid.TryParse(CourseId, out Guid result) || (Type.ToUpper().Equals("COURSE")))
            {

                List<SelectListItem> courseList = new CourseManager().GetCourseList(AppUserHelper.College_ID.Value, programme);
                if (Type.ToUpper().Equals("COURSE"))
                { ViewBag.ChildValues = courseList; }
                else if (Type.ToUpper().Equals("SEMESTER"))
                { ViewBag.ChildValues = new LibraryManager().GetAllSemesters(new Guid(CourseId)); }
                else if (Type.ToUpper().Equals("SUBJECT"))
                {
                    List<SelectListItem> Subjects = new UserProfileManager().FetchChildDDlValues(CourseId, "Subject", true, Semester) ?? new List<SelectListItem>();
                    List<SelectListItem> skillSubjects = new SubjectsManager().GetAllSubjects(SubjectType.SEC.ToString(), AppUserHelper.College_ID.Value);
                    if (skillSubjects != null)
                    { Subjects.AddRange(skillSubjects); }
                    ViewBag.ChildValues = Subjects.OrderBy(x => x.Text.Trim());
                }
                else
                {
                    ViewBag.ChildValues = new CombinationManager().GetCombinationsDDL(AppUserHelper.College_ID.Value, Guid.Parse(CourseId), Semester);
                }


            }

            else
            {

                ViewBag.ChildValues = new List<SelectListItem>();

            }
            return PartialView("_GetChildDDL");


        }
        public ActionResult StudentDetail(string formNumber, PrintProgramme? programme)
        {
            ARGPersonalInformation result = new ARGPersonalInformation();
            if (formNumber.IsNotNullOrEmpty() && programme != null)
                result = new AssignCombinationManager().GetStudentDetails(formNumber, programme.Value) ?? new ARGPersonalInformation();
            ViewData.Model = result;
            return View();
        }

        #region ViewBags

        void FillViewBags(string Form_RegistrationNumber, PrintProgramme? printProg, short? sem)
        {
            ViewBag.Form_RegistrationNumber = Form_RegistrationNumber;
            ViewBag.Semester = sem;
            ViewBag.PrintProgramme = printProg ?? PrintProgramme.UG;
            ViewBag.PrintProgrammeList = Helper.GetSelectList<PrintProgramme>().OrderBy(x => x.Value);
            ViewBag.Batches = Helper.GetYearsDDL().OrderByDescending(x => x.Value);
            //ViewBag.Semesters = new List<SelectListItem>() { new SelectListItem() { Text = "Semester-I (New)", Value = "1" }, new SelectListItem() { Text = "Semester-II", Value = "2" }, new SelectListItem() { Text = "Semester-III", Value = "3" }, new SelectListItem() { Text = "Semester-IV", Value = "4" }, new SelectListItem() { Text = "Semester-V", Value = "5" } };
            FillViewBag_Semesters();
        }

        int SetFormDetailViewBags(ARGPersonalInformation formDetail, PrintProgramme printProgramme, short semester, bool? HasLateralEntry = null)
        {
            Parameters param = new Parameters() { Filters = new List<SearchFilter>() { new SearchFilter() { TableAlias = "ADMCollegeMaster", Column = "College_ID", Value = AppUserHelper.College_ID.Value.ToString() }, new SearchFilter() { Column = "PrintProgramme", Value = ((short)printProgramme).ToString() } }, PageInfo = new Paging() { DefaultOrderByColumn = "CourseFullName", PageNumber = -1, PageSize = -1 } };
            if (HasLateralEntry == true)
                param.Filters.Add(new SearchFilter() { Column = "HasLateralEntry", Value = "1" });
            List<SelectListItem> _CollegeCourseList = new CourseManager().List(param)?.Select(x => new SelectListItem { Value = x.Course_ID.ToString(), Text = x.CourseFullName })?.ToList();
            if (_CollegeCourseList == null) return 0;
            //var _CollegeCourseList = new CourseManager().GetCourseList(AppUserHelper.College_ID.Value, printProgramme);
            List<SelectListItem> courselist = new List<SelectListItem>();
            if ((printProgramme == PrintProgramme.IH || printProgramme == PrintProgramme.PG) && formDetail.CoursesApplied.IsNotNullOrEmpty())
            {
                foreach (var item in formDetail.CoursesApplied)
                {
                    if (_CollegeCourseList.Any(x => x.Value.ToLower() == item.Course_ID.ToString().ToLower()))
                        courselist.Add(new SelectListItem() { Text = item.CourseName, Value = item.Course_ID.ToString() });
                }
            }
            else
            {
                courselist = _CollegeCourseList;
            }
            if (courselist.IsNullOrEmpty()) return 0;
            ViewBag.Courses = courselist;

            if (formDetail.SelectedCombinations.IsNullOrEmpty() || formDetail.CurrentSemesterOrYear <= 0)
            {
                ARGSelectedCombination _SC = new ARGSelectedCombination()
                {
                    PrintProgramme = printProgramme,
                    College_ID = AppUserHelper.College_ID.Value,
                    Semester = semester,
                    CollegeID = AppUserHelper.CollegeCode,
                    Student_ID = formDetail.Student_ID,
                    Course_ID = formDetail.CoursesApplied == null || formDetail.CoursesApplied.Where(x => courselist.Any(y => y.Value == x.Course_ID.ToString())).IsNullOrEmpty()
                                ? Guid.Parse(courselist.First().Value) : formDetail.CoursesApplied.First(x => courselist.Any(y => y.Value == x.Course_ID.ToString())).Course_ID,
                    SemesterBatch = formDetail.Batch
                };
                if (printProgramme == PrintProgramme.IH)
                    _SC.Course_ID = Guid.Parse(courselist.First().Value);
                else if (printProgramme == PrintProgramme.UG)
                    _SC.Course_ID = Guid.Parse(courselist.First().Value);
                //_SC.Course_ID = formDetail.CoursesApplied == null 
                //    ? Guid.Parse(courselist.First().Value)
                //    : formDetail.CoursesApplied.FirstOrDefault(x => x.Course_ID != Guid.Parse("a3ee7f98-7b82-4d95-a2c0-faba7a18240e"))?.Course_ID 
                //    ?? formDetail.CoursesApplied.First().Course_ID;

                formDetail.SelectedCombinations = new List<ARGSelectedCombination>();
                formDetail.SelectedCombinations.Add(_SC);
            }
            var comb = new CombinationManager().GetCombinationsDDL(AppUserHelper.College_ID.Value, formDetail.SelectedCombinations.First().Course_ID, semester);
            ViewBag.CourseSemesterCombinations = comb;
            return 1;
        }

        [NonAction]

        private void SetFormDetailViewBags(List<ARGCoursesApplied> coursesApplied, List<ARGSelectedCombination> selectedCombinations, List<SelectListItem> courseList)
        {
            ViewBag.Courses = courseList;
            if (selectedCombinations.IsNotNullOrEmpty())
            {
                selectedCombinations = selectedCombinations.OrderBy(x => x.Semester).ToList();
                foreach (var selectedCombination in selectedCombinations)
                {
                    var _subjectCombinations = new CombinationManager().GetCombinationsDDL(AppUserHelper.College_ID.Value, selectedCombination.Course_ID);
                    ViewData[selectedCombination.CourseID + "Comb"] = _subjectCombinations;

                }
            }
            else
            {
                if (courseList != null)
                {
                    ViewBag.SubjectCombination = new CombinationManager().GetCombinationsDDL(AppUserHelper.College_ID.Value, Guid.Parse(courseList.First().Value));
                }
            }

        }


        private void SetViewBags(List<ARGSelectedCombination> selectedCombinations)
        {
            ViewBag.Programmes = Helper.GetSelectList<Programme>().OrderBy(x => x.Text);
            var courseList = new CourseManager().GetCourseList(AppUserHelper.College_ID.Value);
            ViewBag.Courses = courseList;
            if (selectedCombinations.IsNotNullOrEmpty())
            {
                selectedCombinations = selectedCombinations.OrderBy(x => x.Semester).ToList();
                foreach (var selectedCombination in selectedCombinations)
                {

                    var _subjectCombinations = new CombinationManager().GetCombinationsDDL(AppUserHelper.College_ID.Value, selectedCombination.Course_ID);
                    ViewData[selectedCombination.CourseID + "Comb"] = _subjectCombinations;

                }
            }
            else
            {
                if (courseList != null)
                {
                    ViewBag.SubjectCombination = new CombinationManager().GetCombinationsDDL(AppUserHelper.College_ID.Value, Guid.Parse(courseList.First().Value));
                }
            }


        }

        private Programme GetProgramFromProgramme(PrintProgramme programme)
        {
            if (programme == PrintProgramme.BED)
                return Programme.UG;
            else if (programme == PrintProgramme.PG)
                return Programme.PG;
            else if (programme == PrintProgramme.IH)
                return Programme.IG;
            return Programme.UG;
        }

        private string GetPrintProgrammes(Guid course_ID)
        {
            ADMCourseMaster aDMCourseMaster = new CourseManager().GetCourseById(course_ID);
            string printPrg = "";
            switch (aDMCourseMaster.PrintProgramme)
            {
                case PrintProgramme.IH:
                case PrintProgramme.UG:
                    printPrg = ((short)Programme.UG).ToString() + "," + ((short)Programme.IG).ToString() + "," + ((short)Programme.Professional).ToString() + "," + ((short)Programme.HS).ToString();
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


        #endregion

        public ActionResult GetCombinationsCount()
        {
            //ViewBag.ProgrammeList = Helper.GetSelectList<PrintProgramme>().OrderBy(x => x.Value);
            //ViewBag.CourseList = new List<SelectListItem>();
            //ViewBag.SemesterList = new List<SelectListItem>();
            FillViewBag_Programmes(AppUserHelper.College_ID);
            FillViewBag_Course(AppUserHelper.College_ID, null, Programme.UG);
            FillViewBag_Semesters();
            return View();
        }
        public ActionResult _CombinationCountList(Parameters parameter)
        {

            var programme = parameter.Filters.FirstOrDefault(filter => filter.Column.ToUpper().Equals("PROGRAMME"));
            if (parameter.SortInfo.ColumnName == null)
            {
                parameter.SortInfo = new Sort() { ColumnName = "AssignedStudentCount", OrderBy = System.Data.SqlClient.SortOrder.Descending };
            }
            if (programme == null)
                programme.Value = "IG";
            List<CombinationsCount> list = new AssignCombinationManager().GetCombinationsCount(parameter) ?? new List<CombinationsCount>();
            ViewBag.Programme = programme.Value;
            ViewBag.RecordCount = ((parameter.PageInfo.PageNumber) * parameter.PageInfo.PageSize);
            return View(list);

        }

        public void GetCombinationsCountCSV(Parameters parameter)
        {

            var programme = parameter.Filters.FirstOrDefault(filter => filter.Column.ToUpper().Equals("PROGRAMME"));
            if (parameter.SortInfo.ColumnName == null)
            {
                parameter.SortInfo = new Sort() { ColumnName = "AssignedStudentCount", OrderBy = System.Data.SqlClient.SortOrder.Descending };
            }
            if (programme == null)
                programme.Value = "IG";
            List<CombinationsCount> list = new AssignCombinationManager().GetCombinationsCount(parameter) ?? new List<CombinationsCount>();

            var reportList = list.Select(x => new
            {
                Programe = x.CombinationCode,
                CombinationSubjects = (x.SubjectsDetails != null ? string.Join(" | ", x.SubjectsDetails.OrderBy(y => y.SubjectFullName).Select(y => y.SubjectFullName)) : ""),
                x.AssignedStudentCount
            }).ToList();
            ExportToCSV(reportList, "Combination Count List" + DateTime.Now.ToString("dd/MM/yyyy"));

        }

        public ActionResult GetSubjectsCount()
        {
            //ViewBag.ProgrammeList = Helper.GetSelectList<PrintProgramme>().OrderBy(x => x.Value);
            //ViewBag.CourseList = new List<SelectListItem>();
            //ViewBag.SemesterList = new List<SelectListItem>();
            FillViewBag_Programmes(AppUserHelper.College_ID);
            FillViewBag_Course(AppUserHelper.College_ID, null, Programme.UG);
            FillViewBag_Semesters();
            ViewBag.Gender = Helper.GenderDDL();
            ViewBag.SubjectTypesDDL = Helper.GetSelectList(SubjectType.None, SubjectType.FirstSemesterExclusion).OrderBy(x => x.Text);
            return View();
        }
        public ActionResult _SubjectCountList(Parameters parameter)
        {
            var programme = parameter.Filters.FirstOrDefault(filter => filter.Column.ToUpper().Equals("PROGRAMME"));

            if (parameter.SortInfo.ColumnName == null)
            {
                parameter.SortInfo = new Sort() { ColumnName = "AssignedStudentCount", OrderBy = System.Data.SqlClient.SortOrder.Descending };
            }

            if (programme == null)
                programme.Value = "IG";

            List<SubjectsCount> list = new AssignCombinationManager().GetSubjectsCount(parameter);
            ViewBag.Programme = programme.Value;
            ViewBag.RecordCount = ((parameter.PageInfo.PageNumber) * parameter.PageInfo.PageSize);
            return View(list);

        }

        [HttpPost]
        public JsonResult ImportFromIHtoUG(string cusformno)
        {
            //Tuple<bool, string> response = new AssignCombinationManager().ImportIHtoUG(cusformno);
            return Json(new { IsSuccess = true, message = "SUCCESS" }, JsonRequestBehavior.DenyGet);
        }

        public JsonResult Get(Guid? Course_ID, short? Semester, short? Batch)
        {
            if (Course_ID.HasValue && Semester.HasValue && Batch.HasValue)
            {
                var model = new CombinationSettingManager().GetCombinationSetting(Course_ID.Value, Semester.Value, Batch.Value);
                if (model != null) return Json(model);
            }
            return null;
        }

    }
}