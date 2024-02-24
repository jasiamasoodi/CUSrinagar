using CUSrinagar.Enums;
using CUSrinagar.Models;
using CUSrinagar.OAuth;
using System;
using CUSrinagar.BusinessManagers;

using System.Collections.Generic;
using CUSrinagar.Extensions;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace CUSrinagar.WebApp.CUStudentZone.Controllers
{
    [OAuthorize(AppRoles.Student)]
    public class QuestionaireController : Controller
    {
        // GET: CUStudentZone/Questionaire
        public ActionResult Questionaire(Semester? Semester)
        {
            if (Semester == null)
            {
                return RedirectToAction("Examination", "Examination", new { @area = "CUStudentZone" });
            }

            //ViewBag.Semester = Semester;
            //List<Questionaire> questionaire = new List<Questionaire>();
            //questionaire = new QuestionaireManager().GetQuestionaire(Semester.Value);
            //if (questionaire == null)
            //{
            FillViewBag(Semester.Value);
            return RedirectToAction("ExaminationForm", "Examination",
                new { @area = "CUStudentZone", @semester = (short)Semester, @RedirectedFormExaminantionForm = true });
            //}
            //questionaire.ToList().ForEach(q => q.Semester = Semester.Value);
            //return View(questionaire);
        }

        [HttpPost]
        public ActionResult SaveQuestionaire(List<Questionaire> questionaires)
        {
            if (questionaires.Any(x => x.IsCheckedOption == Guid.Empty))
            { ModelState.AddModelError("AllQuestions", "Answer to all Question"); }
            if (ModelState.IsValid)
            {
                bool isSuccess = new QuestionaireManager().SaveQuestionaire(questionaires);
                FillViewBag(questionaires.FirstOrDefault().Semester);
                return RedirectToAction("ExaminationForm", "Examination",
                    new { @area = "CUStudentZone", @semester = (short?)questionaires?.FirstOrDefault()?.Semester, @RedirectedFormExaminantionForm = true });
            }
            return View("Questionaire", questionaires);
        }

        private void FillViewBag(Semester semester)
        {
            List<SelectListItem> StudentExamFormsCurrentSem = null;
            var CurrentSemester = new StudentManager().GetStudent(AppUserHelper.User_ID, AppUserHelper.TableSuffix, false).CurrentSemesterOrYear;
            StudentExamFormsCurrentSem = new ExaminationFormManager().ExamForSemester(AppUserHelper.OrgPrintProgramme, true);
            ViewBag.CurrentSemester = CurrentSemester;
            if (StudentExamFormsCurrentSem != null)
                ViewBag.ExamForSemester = StudentExamFormsCurrentSem.Distinct().ToList();
            ViewBag.SelectedSemester = semester;
        }
    }
}