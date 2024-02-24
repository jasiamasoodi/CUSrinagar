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
using System.Threading.Tasks;

namespace CUSrinagar.WebApp.CUSrinagarAdminPanel.Controllers
{
    [OAuthorize(AppRoles.University, AppRoles.University_Registrar)]
    public class AdmissionController : AdminController
    {
        #region IHSection
        public ActionResult UploadedCertificateIH()
        {
            FillViewBag_College();
            FillViewBag_Programmes();
            FillViewBag_Course(Programme.IG);
            FillViewBag_SelectionStatus();
            return View();
        }
        public PartialViewResult UploadCertificateIHList(Parameters parameter, PrintProgramme? otherParam1)
        {
            List<SelectedStudent> list = new StudentManager().GetSelectedStudentsList(parameter, otherParam1.Value);
            return PartialView(list);
        }
        public void UploadCertificateIHCSV(Parameters parameter, PrintProgramme? printProgramme)
        {
            List<SelectedStudent> list = new StudentManager().GetSelectedStudentsList(parameter, printProgramme.Value);
            var reportList = list.Select(x => new
            {
                x.CollegeFullName,
                Programe = Helper.GetEnumDescription(x.Programme),
                x.CourseFullName,
                x.TotalPoints,
                x.EntranceRollNo,
                x.StudentFormNo,
                x.FullName,
                x.Parentage,
                x.Category,
                x.Gender,
                Status = Helper.GetEnumDescription<StudentSelectionStatus>(x.StudentSelectionStatus),
                x.NumberOfCertificate
            }).ToList();
            ExportToCSV(reportList, "Selection List" + printProgramme.ToString() + DateTime.Now.ToString("dd/mm/yyyy"));
        }

        [HttpGet]
        public ActionResult UploadedCertificate(Guid Student_ID, Guid Course_ID, PrintProgramme printProgramme)
        {
            FillViewBag_SelectionStatus();
            FillViewBag_VerificationStatusOfCertificates();
            var certificates = new StudentManager().GetUploadedCertificates(Student_ID);
            Tuple<bool, ARGPersonalInformation, ADMSemesterAdmissionSettings> ValidateRequest = ValidateRequest_Student(Student_ID, Course_ID, printProgramme);
            var studentPersonalInfo = ValidateRequest.Item2;
            ViewBag.ARGPersonalInformation = studentPersonalInfo;
            GetResponseViewBags();
            return View(certificates);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadedCertificate(List<Certificate> model, Guid Course_ID, StudentSelectionStatus StudentSelectionStatus, PrintProgramme printProgramme)
        {
            ResponseData response = new ResponseData();
            response = new StudentManager().SaveUpdate(model, printProgramme, Course_ID, StudentSelectionStatus);

            SetResponseViewBags(response);
            return RedirectToAction("UploadedCertificate", new { Student_ID = model.First().Student_ID, Course_ID = Course_ID, printProgramme = printProgramme });
        }
        Tuple<bool, ARGPersonalInformation, ADMSemesterAdmissionSettings> ValidateRequest_Student(Guid Student_ID, Guid Course_ID, PrintProgramme printProgramme)
        {
            ARGPersonalInformation _S = new RegistrationManager().GetStudent(Student_ID, printProgramme, true, true, true, true);
            _S.CoursesApplied = _S.CoursesApplied.Where(x => (x.Course_ID == Course_ID) && x.PrintProgramme == printProgramme).ToList();
            if (_S.CoursesApplied.Any(x => x.StudentSelectionStatus == StudentSelectionStatus.Provisional || x.StudentSelectionStatus == StudentSelectionStatus.Provisional_SF
            || x.StudentSelectionStatus == StudentSelectionStatus.CertificatesUploaded || x.StudentSelectionStatus == StudentSelectionStatus.CertificatesUploaded_SF
            || x.StudentSelectionStatus == StudentSelectionStatus.Verified_MakePayment || x.StudentSelectionStatus == StudentSelectionStatus.Verified_MakePayment_SF))
            {
                var courseSetting = new StudentManager().GetSemesterAdmissionSettings(Course_ID, 1);
                decimal amount = courseSetting.AdmissionFee;
                if (courseSetting.AllowOnlySpecificCUSRegnNos)
                    amount += new StudentManager().GetAdditionalFeeSemesterAdm(_S.StudentFormNo, 1);
                if (_S.CoursesApplied.Any(x => x.StudentSelectionStatus == StudentSelectionStatus.Provisional_SF || x.StudentSelectionStatus == StudentSelectionStatus.CertificatesUploaded_SF || x.StudentSelectionStatus == StudentSelectionStatus.Verified_MakePayment_SF) && courseSetting.SelfFinanceFee.HasValue)
                    amount += courseSetting.SelfFinanceFee.Value;
                ViewBag.AmountToPay = amount;
            }
            return Tuple.Create<bool, ARGPersonalInformation, ADMSemesterAdmissionSettings>(false, _S, null);
        }

        #endregion


        #region UGSection
        public ActionResult CollegePreference()
        {
            List<StudentCollegeCourseCount> Count = new StudentManager().StudentCollegeCourseCount(null);
            ViewBag.StudentCollegeCourseCount = Count;
            FillViewBag_College();
            FillViewBag_Programmes();
            FillViewBag_Course(Programme.UG);
            FillViewBag_SelectionStatus();
            return View();
        }
        public PartialViewResult CollegePreferenceListPartial(Parameters parameter, PrintProgramme? otherParam1)
        {
            List<StudentCollegePreferenceList> list = new StudentManager().GetCollegePreference(parameter);
            return PartialView(list);
        }
        public void CollegePreferenceCSV(Parameters parameter, PrintProgramme? printProgramme)
        {
            List<StudentCollegePreferenceList> list = new StudentManager().GetCollegePreference(parameter);
            var reportList = list.Select(x => new
            {
                x.Batch,
                x.CollegeFullName,
                Allowed = x.IsAllowed ? "Added" : "Not Added",
                x.CourseFullName,
                x.Category,
                x.StudentFormNo,
                x.FullName,
                x.Gender,
                x.PreferenceOrder,
                x.EntranceRollNo,
                x.CATEntrancePoints,
                x.QualificationPoints,
                x.TotalPoints
            }).ToList();
            ExportToCSV(reportList, "UG Admission List" + DateTime.Now.ToString("dd/MMM/yyyy"));
        }
        [HttpGet]
        public ActionResult StudentCollegePreference(Guid Student_ID, Guid Course_ID, PrintProgramme printProgramme)
        {
            List<StudentCollegePreference> collegePref = new StudentManager().GetCollegePreference(Student_ID);

            Tuple<bool, ARGPersonalInformation, ADMSemesterAdmissionSettings> ValidateRequest = ValidateRequest_UG(Student_ID, null, PrintProgramme.IH);

            ViewBag.ARGPersonalInformation = ValidateRequest.Item2;
            GetResponseViewBags();
            return View(collegePref);
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult SetCollegePreference(Guid College_ID, Guid Student_ID, Guid Course_ID, StudentSelectionStatus StudentSelectionStatus, PrintProgramme printProgramme)
        {
            ResponseData response = new ResponseData();
            int numberofrecordseffected = 0;
            numberofrecordseffected += new StudentManager().UpdateCollegePreference(College_ID, Student_ID, Course_ID, true);
            numberofrecordseffected += new StudentManager().UpdateSelectionStatus(Student_ID, Guid.Parse("A3EE7F98-7B82-4D95-A2C0-FABA7A18240E"), StudentSelectionStatus.Verified_MakePayment);
            if (numberofrecordseffected > 0)
            {
                response.IsSuccess = true; response.SuccessMessage = "Status updated successfully.";
            }
            SetResponseViewBags(response);
            return RedirectToAction("StudentCollegePreference", new { Student_ID = Student_ID, Course_ID = Course_ID, printProgramme = printProgramme });
        }

        public JsonResult BatchUpdateStudentSelectionStatus(BatchUpdateSelectionStatus model)
        {
            ResponseData response = new ResponseData();
            var _stdManager = new StudentManager();
            foreach (var item in model.Student_ID)
            {
                _stdManager.UpdateCollegePreference(model.College_ID, item, model.Course_ID, true);
                response.NumberOfRecordsEffected += _stdManager.UpdateSelectionStatus(item, Guid.Parse("A3EE7F98-7B82-4D95-A2C0-FABA7A18240E"), model.StudentSelectionStatus);
            }
            response.IsSuccess = response.NumberOfRecordsEffected > 1;
            return Json(response);
        }

        public JsonResult StudentCollegeCourseCount(Guid? College_ID)
        {
            List<StudentCollegeCourseCount> Count = new StudentManager().StudentCollegeCourseCount(College_ID);
            return Json(Count);
        }

        Tuple<bool, ARGPersonalInformation, ADMSemesterAdmissionSettings> ValidateRequest_UG(Guid Student_ID, Guid? Course_ID, PrintProgramme printProgramme)
        {
            List<StudentCollegePreference> collegePref = new StudentManager().GetCollegePreference(Student_ID);
            if (collegePref.Any(x => x.OldStudent_ID.HasValue && x.OldStudent_ID != Guid.Empty))
            {
                Student_ID = collegePref.First().Student_ID;
                printProgramme = PrintProgramme.UG; //that is student has been imported 
            }
            ARGPersonalInformation studentPersonalInfo = new RegistrationManager().GetStudent(Student_ID, printProgramme, true, true, true, true);
            Course_ID = Course_ID.HasValue ? Course_ID.Value : Guid.Parse("A3EE7F98-7B82-4D95-A2C0-FABA7A18240E");
            studentPersonalInfo.CoursesApplied = studentPersonalInfo.CoursesApplied.Where(x => x.Course_ID == Guid.Parse("A3EE7F98-7B82-4D95-A2C0-FABA7A18240E")).ToList();
            ARGStudentPreviousQualifications I2thDetails = studentPersonalInfo.AcademicDetails?.FirstOrDefault(x => x.ExamName.Trim().ToLower() == "12th");
            if (I2thDetails != null)
            {
                studentPersonalInfo.CoursesApplied.First().SubjectEntrancePoints = Convert.ToDecimal(((I2thDetails.MarksObt / I2thDetails.MaxMarks * 100) * 60) / 100);
            }
            if (collegePref.IsNotNullOrEmpty())
            {
                var course = collegePref.FirstOrDefault(x => x.IsActive)?.CourseFullName;
                studentPersonalInfo.CoursesApplied.First().CourseName = studentPersonalInfo.CoursesApplied.First().CourseName + "/" + course;
            }
            return Tuple.Create<bool, ARGPersonalInformation, ADMSemesterAdmissionSettings>(false, studentPersonalInfo, null);
        }

        #endregion

        #region PG Section
        [HttpGet]
        public ActionResult PGCertificateCheck()
        {
            ViewBag.Programme = Helper.GetSelectList<PrintProgramme>();
            return View();
        }

        public PartialViewResult StudentCertificates(Parameters parameter)
        {
            List<StudentAdmissionCertificate> studentAdmissionCertificate = new StudentManager().GetCertificateDetails(parameter);
            return PartialView(studentAdmissionCertificate);
        }
        #endregion

        #region Register of Students
        [HttpGet]
        public ActionResult StudentsRegister()
        {
            ViewBag.ProgrammeList = Helper.GetSelectList<PrintProgramme>().OrderBy(x => x.Value);
            ViewBag.CourseList = new List<SelectListItem>();
            ViewBag.CombinationList = new List<SelectListItem>();
            ViewBag.Colleges = new CollegeManager().GetADMCollegeMasterList();
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> StudentsRegisterPreview(string id, Guid? id1, PrintProgramme? programme, string College_ID, bool iLEO=false)
        {
            if (!(Guid.TryParse(Convert.ToString(id1), out Guid Course_ID) || !int.TryParse(id, out int Batch)) || !Enum.TryParse(programme?.ToString(), out PrintProgramme Pprogramme))
                return RedirectToAction("StudentsRegister", "Admission", new { area = "CUSrinagarAdminPanel" });

            ADMCollegeMaster college = new CollegeManager().GetItem(Guid.Parse(College_ID ?? Guid.Empty.ToString()));

            ViewBag.College = college.CollegeFullName;
            ViewBag.CollegeCode = college.CollegeCode;

            Parameters parameter = new Parameters();
            var RRManager = new RRManager();

            parameter = RRManager.AddPageFilters("Course_ID", Course_ID.ToString(), parameter);
            parameter = RRManager.AddPageFilters("College_ID", College_ID ?? Guid.Empty.ToString(), parameter);
            parameter = RRManager.AddPageFilters("SemesterBatch", id, parameter);
            if (iLEO)
            {
                parameter = RRManager.AddPageFilters("IsLateralEntry", "1", parameter);
            }
            ADMCourseMaster course = new CourseManager().GetItem(Course_ID);
            ViewBag.CourseFullName = course.CourseFullName;
            ViewBag.Programme = course.Programme;
            List<StudentRegister> list = await RRManager.GetStudentsAsync(Pprogramme, parameter);
            return View(list);
        }


        public PartialViewResult _GetRRDDL(string College_ID, string id, string Type, string childType, string childSubType, string Semester)
        {
            ViewBag.Type = Type;
            ViewBag.ChildType = childType;
            ViewBag.ChildSubType = childSubType;
            ViewBag.ChildValues = FetchChildDDlValues(College_ID, id, Type, Semester);
            return PartialView();
        }

        private List<SelectListItem> FetchChildDDlValues(string College_ID, string id, string Type, string Semester)
        {
            List<SelectListItem> childDDL = new List<SelectListItem>();
            switch (Type)
            {
                case "Course":
                    {
                        PrintProgramme programme = (PrintProgramme)Convert.ToInt32(id);
                        childDDL = new CourseManager().GetCourseList(Guid.Parse(College_ID ?? Guid.Empty.ToString()), programme);
                        break;
                    }
                case "Semester":
                    {
                        Guid CourseId = new Guid(id);
                        childDDL = new LibraryManager().GetAllSemesters(CourseId);
                        break;
                    }
            }

            return childDDL;
        }
        #endregion
    }

}
