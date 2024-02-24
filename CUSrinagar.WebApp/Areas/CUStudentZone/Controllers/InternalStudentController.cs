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

namespace CUSrinagar.WebApp.CUStudentZone.Controllers
{
    [OAuthorize(AppRoles.Student)]
    public class InternalStudentController : Controller
    {  // GET: CUStudentZone/InternalStudent
        public ActionResult ViewExam(string Message = "")
        {
            ViewBag.ErrorMessage = Message;
            SetViewBag();
            return View();
        }
        public void SetViewBag()
        {
            ARGPersonalInformation aRGPersonalInformation = new StudentManager().GetStudent(AppUserHelper.User_ID, AppUserHelper.OrgPrintProgramme, false);
            Guid Student_ID = aRGPersonalInformation.Student_ID;
            int Semester = aRGPersonalInformation.CurrentSemesterOrYear;
            ViewBag.Subjects = new InternalExamManager().GetSubjects(Student_ID, Semester);
        }

        // GET: CUStudentZone/InternalStudent/Details/5
        public ActionResult GetExam(Guid Subject)
        {
            //if( VerifyOTP())
            return RedirectToAction("ExamPaper", new { Subject_Id = Subject });
            //  else  {
            // ViewBag.msg = "Exam Paper Not Available";
            //SetViewBag();
            //return RedirectToAction("ViewExam");
            //   }
        }

        private bool VerifyOTP()
        {
            throw new NotImplementedException();
        }

        //protected void GenerateOTP(object sender, EventArgs e)
        //{
        //    string alphabets = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        //    string small_alphabets = "abcdefghijklmnopqrstuvwxyz";
        //    string numbers = "1234567890";

        //    string characters = numbers;
        //    if (rbType.SelectedItem.Value == "1")
        //    {
        //        characters += alphabets + small_alphabets + numbers;
        //    }
        //    int length = int.Parse(ddlLength.SelectedItem.Value);
        //    string otp = string.Empty;
        //    for (int i = 0; i < length; i++)
        //    {
        //        string character = string.Empty;
        //        do
        //        {
        //            int index = new Random().Next(0, characters.Length);
        //            character = characters.ToCharArray()[index].ToString();
        //        } while (otp.IndexOf(character) != -1);
        //        otp += character;
        //    }
        //    lblOTP.Text = otp;
        //}
        private bool checkIsPaperAvailable(Guid Subject_Id, Guid Student_ID, int Semester, int Batch)
        {
            return new InternalExamManager().GetExam(Subject_Id, Batch);
        }

        // GET: CUStudentZone/InternalStudent/Create
        public ActionResult ExamPaper(Guid Subject_Id)
        {
            ARGPersonalInformation aRGPersonalInformation = new StudentManager().GetStudent(AppUserHelper.User_ID, AppUserHelper.OrgPrintProgramme, false);
            Guid Student_ID = aRGPersonalInformation.Student_ID;
            int Semester = aRGPersonalInformation.CurrentSemesterOrYear;

            bool Available = checkIsPaperAvailable(Subject_Id, Student_ID, Semester, aRGPersonalInformation.Batch);
            bool AlreadySubmitted = checkIsPaperAlreadySubmitted(Subject_Id, Student_ID, Semester, aRGPersonalInformation.Batch);
            if (!Available || AlreadySubmitted)
            {
                string Message = "Exam Paper Not Available";
                SetViewBag();
                return RedirectToAction("ViewExam", new { Message });
            }
            Parameters parameters = createParameters("Subject_Id", Subject_Id.ToString(), "Batch", aRGPersonalInformation.Batch.ToString(), "Semester", Semester.ToString());

            List<InternalQuestions> listQuestions = new InternalExamManager().GetQuestionList(parameters);
            foreach (InternalQuestions question in listQuestions ?? new List<InternalQuestions>())
            {
                question.InternalQuestionOptions = new InternalExamManager().GetOptionList(question.Question_Id);
            }
            return View(listQuestions);
        }

        private bool checkIsPaperAlreadySubmitted(Guid Subject_Id, Guid student_ID, int semester, short batch)
        {
            return new InternalExamManager().CheckExam(Subject_Id, student_ID);
        }

        private Parameters createParameters(string col1, string col1val, string col2, string col2val, string col3, string col3val)
        {
            Parameters parameters = new Parameters() { Filters = new List<SearchFilter>() };
            parameters.Filters.Add(new SearchFilter() { Column = col1, Operator = SQLOperator.EqualTo, Value = col1val, GroupOperation = LogicalOperator.AND });
            if (!string.IsNullOrEmpty(col2))
            { parameters.Filters.Add(new SearchFilter() { Column = col2, Operator = SQLOperator.EqualTo, Value = col2val, GroupOperation = LogicalOperator.AND }); }
            if (!string.IsNullOrEmpty(col3))
            { parameters.Filters.Add(new SearchFilter() { Column = col3, Operator = SQLOperator.EqualTo, Value = col3val, GroupOperation = LogicalOperator.AND }); }
            return parameters;
        }
        // POST: CUStudentZone/InternalStudent/Create
        [HttpPost]
        public ActionResult SaveExam(List<InternalQuestions> collection)
        {

            //if (collection.Any(x => x.IsCheckedOption == Guid.Empty))
            //{ ModelState.AddModelError("AllQuestions", "Answer to all Question"); }
            if (ModelState.IsValid)
            {
                bool isSuccess = new InternalExamManager().SaveExam(collection);
                ViewBag.ErrorMessage = "Saved Succesfully form";
                SetViewBag();
                return View("ViewExam");
            }

            return View("ExamPaper", collection);
        }
    }


}
