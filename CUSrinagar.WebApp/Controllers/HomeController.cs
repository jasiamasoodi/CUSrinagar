using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CUSrinagar.Enums;
using CUSrinagar.Models;
using Newtonsoft.Json;
using CUSrinagar.BusinessManagers;
using GeneralModels;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CUSrinagar.Extensions;

namespace CUSrinagar.WebApp.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.MarqueeText = new NotificationManager().GetMarqueeText();
            //new CourseManager().SetColleges();
            return View();
        }

        public ActionResult VCDesk()
        {
            return View();
        }

        public ActionResult RegistrarDesk()
        {
            return View();
        }

        public ActionResult PhysicsWorkshop()
        {
            return View();
        }

        public ActionResult ContractualLecturers()
        {
            return View();
        }

        public string GetQuickNotifications()
        {
            Parameters parameter = new Parameters();
            parameter = new NotificationManager().GetDefaultParameter(parameter, true, true, true, 10);
            //parameter.Filters.Add(new SearchFilter
            //{
            //    Column = "NotificationType",
            //    Value = $"{(short)NotificationType.Jobs}",
            //    Operator = SQLOperator.NotEqualTo
            //});
            List<Notification> listNotification = new NotificationManager().GetAllNotificationList(parameter);
            return JsonConvert.SerializeObject(listNotification);
        }
        public string GetTabContents(NotificationType NotificationTypeIs = NotificationType.General)

        {
            List<Notification> listNotification = new NotificationManager().GetNotificationByType(NotificationTypeIs);
            return JsonConvert.SerializeObject(listNotification);
        }
        public ActionResult Print()
        {

            return View();
        }
        public ActionResult AboutCUS()
        {
            return View();
        }

        public ActionResult Lectures_presentations()
        {
            return View();
        }
        public ActionResult Administration()
        {

            return View();

        }

        public ActionResult AdministrationTemp()
        {
            //return View("~/Views/Shared/_ComingSoon.cshtml");
            return View();

        }


        public ActionResult TheConcept()
        {
            return View();
        }
        public ActionResult ExaminationSchedule()
        {
            return View();
        }
        public ActionResult PaperPattern(Programme Program = Programme.UG)
        {
            if (Program == Programme.PG)
            {
                return View("~/Views/Home/_PGPaper.cshtml");
            }
            else if (Program == Programme.UG)
            {
                return View("~/Views/Home/_UGPaper.cshtml");
            }
            else if (Program == Programme.Engineering)
            {
                return View("~/Views/Home/_Engineering.cshtml");
            }
            return View("~/Views/Home/_UGPaper.cshtml");
        }
        public ActionResult ModelPapers()
        {
            return View();
        }
        public ActionResult FeeStructure()
        {
            return View();
        }
        public ActionResult Gallery()
        {
            return View();
        }

        public ActionResult GalleryAlbum(string Gallery)
        {
            List<string> imageFiles = new List<string>();
            var albumCaption = "";
            try
            {
                var path = "~/Content/ThemePublic/Libraries/images/gallery/";

                if (Gallery.ToLower().Contains("admin"))
                {
                    albumCaption = "Administration";
                    path += "administration/";
                }
                else if (Gallery.ToLower().Contains("sport"))
                {
                    albumCaption = "Maiden Annual Sports Festival";
                    path += "sports_festival/";
                }
                else if (Gallery.ToLower().Contains("prize"))
                {
                    albumCaption = "Prize Distribution";
                    path += "prize_distribution_fest/";
                }
                else if (Gallery.ToLower().Contains("quantum"))
                {
                    albumCaption = "Deformation of Quantum Mechanics";
                    path += "workshop_quantum_mechnics/";
                }
                else if (Gallery.ToLower().Contains("vcwelcome2021"))
                {
                    albumCaption = "Vice Chancellor Welcome";
                    path += "vcwelcome2021/";
                }
                else if (Gallery.ToLower().Contains("constitutionday2021"))
                {
                    albumCaption = "Constitution Day";
                    path += "constitutionday2021/";
                }
                else if (Gallery.ToLower().Contains("annualcollegeroadrace2022"))
                {
                    albumCaption = "Annual College Road Race (Women)-2022";
                    path += "AnnualCollegeRoadRace2022/";
                }
                else if (Gallery.ToLower().Contains("council"))
                {
                    albumCaption = "Academic Council Meetings 2017-onwards";
                    path += "ADMeetings/";
                }
                else if (Gallery.ToLower().Contains("pencaksilatchampionship24"))
                {
                    albumCaption = "All India Inter University Pencak Silat Championship 2023-24";
                    path += "PencakSilatChampionship24/";
                }
                string strimagePath = Server.MapPath(path);
                string[] files = Directory.GetFiles(strimagePath);
                foreach (string filename in files)
                {
                    if (Regex.IsMatch(filename, @".jpg|.png|.jpeg|.png", RegexOptions.IgnoreCase))
                    {
                        var _filename = filename.Split(new Char[] { '\\', '\\' }).ToList().Last();
                        imageFiles.Add(Url.Content(path + _filename));
                    }
                }
            }
            catch { }
            ViewBag.ImageFiles = imageFiles?.OrderBy(x => x)?.ToList();
            ViewBag.AlbumCaption = albumCaption;
            return View();
        }


        public ActionResult Downloads()
        {
            return View();
        }

        public ActionResult BoardOfStudies()
        {
            return View();
        }

        public PartialViewResult DownloadListPartial(Parameters Parameter)
        {
            return PartialView();
        }

        public ActionResult ContactUs()
        {
            return View();
        }

        public ActionResult Contacts()
        {
            return View();
        }


        [HttpPost]
        public ActionResult ContactUsSave(Contact contact)
        {
            ViewBag.Message = "";
            if (ModelState.IsValid)
            {
                string Subject = "Query From Student Email:" + contact.Email + "";
                if (!string.IsNullOrEmpty(contact.Name))
                { Subject += ", Name:" + contact.Name; }
                if (!string.IsNullOrEmpty(contact.Phone))
                { Subject += ", Phone:" + contact.Phone; }
                string Contactmail = System.Web.Configuration.WebConfigurationManager.AppSettings["ContactMsgTo"];
                new TaskFactory().StartNew(() => new EmailSystem().SendMail(Contactmail, Subject, contact.Message, true));
                ViewBag.Message = "Mail Sent Succesfully";
                return View("ContactUs", new Contact());
            }
            return View("ContactUs", contact);
        }

        public ActionResult Enrollments()
        {
            List<StudentsCourseWiseStatistics> listCount = new List<StudentsCourseWiseStatistics>();
            if (listCount != null)
            {
                var parameter = new Parameters() { Filters = new List<SearchFilter>() { new SearchFilter() { Column = "Semester", Value = "1", TableAlias = "Comb" } } };
                listCount = new StatisticsManager().GetstudentsCourseWiseStatistics(parameter);
            }
            return View(listCount);
        }
        public PartialViewResult TotalEnrollments()
        {
            //var enrollment = new EnrollmentManager().GetCurrentYearEnrollment();
            return PartialView("_Enrollments");
        }

        public ActionResult AdmissionQuickLinks()
        {
            return View();
        }


        [HttpPost]
        public async Task<JsonResult> SaveTokenAsync(string token, string DeviceInfo)
        {
            if (string.IsNullOrWhiteSpace(token))
                return Json(1, JsonRequestBehavior.DenyGet);

            PushMessageSubscribers messageSubscribers = new PushMessageSubscribers
            {
                DeviceInfo = DeviceInfo,
                DeviceToken = token,
                CreatedOn = DateTime.Now
            };
            int result = await new PushNotificationManager().SavePushMessageSubscribersAsync(messageSubscribers);
            return Json(result, JsonRequestBehavior.DenyGet);
        }

        [HttpGet]
        public ActionResult CUSApprovedBooksCourses()
        {
            return View();
        }

        [HttpGet]
        public ActionResult DivisionImprovement()
        {
            return View();
        }

        [HttpGet]
        public ActionResult CoordinatorDetails()
        {
            return View();
        }

        [HttpGet]
        public ActionResult SeniorityListAPS()
        {
            return View();
        }

        [HttpGet]
        public ActionResult SeniorityListAssociateProfs()
        {
            return View();
        }

        [HttpGet]
        public ActionResult NEPRegulations()
        {
            return View();
        }

        [HttpGet]
        public ActionResult RTI()
        {
            return View();
        }

        [HttpGet]
        public ActionResult PaperPatternNEP()
        {
            return View();
        }
    }
}