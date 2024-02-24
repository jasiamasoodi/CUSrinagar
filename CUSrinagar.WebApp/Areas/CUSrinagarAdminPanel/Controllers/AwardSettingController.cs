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
using System.Web;
using System.Web.Mvc;
using Nut;
using System.Threading.Tasks;

namespace CUSrinagar.WebApp.CUSrinagarAdminPanel.Controllers
{
    [OAuthorize(AppRoles.University, AppRoles.AwardSettings)]
    public class AwardSettingController : AdminController
    {
        // GET: CUSrinagarAdminPanel/AwardSetting
        public ActionResult Index()
        {
            FillViewBag();
            return View();
        }

        private void FillViewBag()
        {
            List<SelectListItem> CollegeList = new CollegeManager().GetADMCollegeMasterList();
            ViewBag.Colleges = CollegeList;
            SelectList courseList = new GeneralDDLManager().GetCourseOptLabelDDL(true, new Guid(CollegeList.FirstOrDefault().Value));
            courseList.ToList().AddRange(new CourseManager().GetOtherSubjectCourse() ?? new List<SelectListItem>());
            ViewBag.Courses = courseList;
            IEnumerable<SelectListItem> list = Helper.GetSelectList<AwardType>();
            ViewBag.AwardTypes = list ?? new List<SelectListItem>();
            ViewBag.Semesters = Helper.GetSelectList<Semester>();
        }
        public ActionResult AwardSettingList(Parameters parameter)
        {

            IEnumerable<AwardSettingsModel> listResult = new List<AwardSettingsModel>();
            if (parameter.Filters != null)
            {
                SearchFilter awadty = parameter.Filters.Where(x => x.Column == "AwardType").FirstOrDefault();
                SearchFilter batchf = parameter.Filters.Where(x => x.Column == "Batch").FirstOrDefault();
                if (awadty.Value == ((int)AwardType.Theory).ToString())
                {
                    parameter.Filters.Remove(batchf);
                    parameter.Filters.Add(new SearchFilter() { Column = "Year", Value = batchf.Value.ToString(), Operator = SQLOperator.EqualTo });
                }
                listResult = new AwardManager().GetAllAwardSetting(parameter);
            }
            ViewBag.AwardTypeIs = parameter.Filters.Where(x => x.Column == "AwardType").FirstOrDefault().Value;
            return View(listResult);
        }
        public PartialViewResult _GetChildDDL(string id, string Type)
        {
            try
            {
                Guid.Parse(id);

                ViewBag.Type = Type;
                ViewBag.ChildValues = new List<SelectListItem>();
                if ((Type.ToUpper() == "COURSE" && int.TryParse(id, out int resultid)) || Guid.TryParse(id, out Guid result))
                { ViewBag.ChildValues = new SubjectsManager().FetchChildDDlValuesByCollegeID(id, Type); }
            }
            catch (Exception) { }
            return PartialView();
        }
        public string ReleaseAward(string id)
        {
            string msg = string.Empty;
            msg = new AwardManager().ReleaseAward(id) ? "Success" : "Fail";
            return msg;
        }

        public ActionResult AwardFilters()
        {
            FillViewBag_College();
            FillViewBag_Programmes();
            FillViewBag_Course(Programme.UG);
            FillViewBag_Semesters();
            IEnumerable<SelectListItem> list = Helper.GetSelectList<AwardType>();
            ViewBag.AwardTypes = list ?? new List<SelectListItem>();
            return View();
        }
        public ActionResult AwardFiltersList(Parameters parameter)
        {

            IEnumerable<AwardFilterSettings> listResult = new List<AwardFilterSettings>();
            if (parameter.Filters == null)
                parameter.Filters = new List<SearchFilter>() { new SearchFilter() { Column = "AwardType", Value = AwardType.Practical.ToString(), Operator = SQLOperator.EqualTo } };
            listResult = new AwardManager().GetAllAwardFilters(parameter);
            return View(listResult);
        }
        public ActionResult EditFilters(Guid id)
        {
            AwardFilterSettings model = new AwardManager().GetAwardFilters(id);
            FillViewBag_College();
            FillViewBag_Course(model.Programme);
            return View(model);
        }
        [HttpPost]
        public ActionResult EditFilters(AwardFilterSettings model)
        {

            if (model != null)
            {
                model.Courses = model.CourseList != null ? string.Join(",", model.CourseList) : null;
                model.Colleges = model.CollegeList != null ? string.Join(",", model.CollegeList) : null; ;
                if (new AwardManager().EditFilter(model))
                {
                    if (new EmailSystem().AwardMail(model))
                    {
                        return RedirectToAction("AwardFilters");
                    }
                }
            }
            FillViewBag_College();
            FillViewBag_Course(model.Programme);
            return View(model);
        }
        public string OpenAwardLink(string id, DateTime? EndDate, string RecieverMail)
        {
            string msg = string.Empty;
            if (new AwardManager().OpenAwardLink(id, EndDate))
            {
                AwardFilterSettings model = new AwardManager().GetAwardFilters(Guid.Parse(id));
                model.RecieverMail = RecieverMail;
                msg = new EmailSystem().AwardMail(model) ? "Success" : "Fail";
            }
            return msg;
        }
        public string UpdateAwardStatus(Guid id, string Type)
        {
            string msg = "Some Error Occured";

            AwardFilterSettings model = new AwardManager().GetAwardFilters(id);
            if (new AwardManager().UpdateAwardStatus(model, Type))
            {
                msg = "Updated Successfully";
            }
            return msg;
        }

        //Update all backlogs to Lateron Status so that the previos marks will be erased

        public int UpdateBacklogsToLateron(Guid Awardfilter_Id)
        {
            int returnvalue=new AwardManager().UpdateBacklogsToLateron(Awardfilter_Id);                     
            return returnvalue;
        }
    }
}