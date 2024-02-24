using CUSrinagar.BusinessManagers;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using CUSrinagar.Models;
using CUSrinagar.OAuth;
using GeneralModels;
using System;
using System.Linq;
using System.Web.Mvc;


namespace CUSrinagar.WebApp.CUCollegeAdminPanel.Controllers
{
    [OAuthorize(AppRoles.College, AppRoles.eGovCUET)]
    public class EGovCUETController : Controller
    {
        [HttpGet]
        public ActionResult Search()
        {
            ViewBag.Programmes = Helper.GetSelectList(PrintProgramme.PG, PrintProgramme.IH, PrintProgramme.BED);
            return View();
        }

        [HttpPost]
        public PartialViewResult SearchList(Parameters parameter)
        {
            var cUETRegistrationData = new CUETRegistrationData();
            if (parameter == null)
            {
                cUETRegistrationData.CUETErrors = new CUETError
                {
                    Error = new BaseError
                    {
                        Message = "All the fields are required",
                        Code = System.Net.HttpStatusCode.BadRequest,
                    }
                };
                return PartialView(cUETRegistrationData);
            }

            string ApplicationNumber = parameter.Filters?.FirstOrDefault(x => x.Column == "ApplicationNo")?.Value;
            string DateOfBirth = parameter.Filters?.FirstOrDefault(x => x.Column == "DOB")?.Value;
            string SelectedPorgramme = parameter.Filters?.FirstOrDefault(x => x.Column == "SelectedPorgramme")?.Value;
            string BoardRegistrationNo = parameter.Filters?.FirstOrDefault(x => x.Column == "BoardRegistrationNo")?.Value;


            if (string.IsNullOrWhiteSpace(ApplicationNumber)
                || string.IsNullOrWhiteSpace(DateOfBirth)
                || string.IsNullOrWhiteSpace(SelectedPorgramme)
                || string.IsNullOrWhiteSpace(BoardRegistrationNo))
            {
                cUETRegistrationData.CUETErrors = new CUETError
                {
                    Error = new BaseError
                    {
                        Message = "All the fields are required",
                        Code = System.Net.HttpStatusCode.BadRequest,
                    }
                };
                return PartialView(cUETRegistrationData);
            }

            #region Call CUET API
            cUETRegistrationData = APIHelper.GetCandidatesRegistrationData(new CUSCUETAPIRequest
            {
                //ApplicationNo = "233510917341",
                //DOB = new DateTime(2005, 12, 20),
                ApplicationNo = ApplicationNumber,
                DOB = DateTime.Parse(DateOfBirth),
                SelectedPorgramme = (PrintProgramme)Enum.Parse(typeof(PrintProgramme), SelectedPorgramme.ToString()),
                BoardRegistrationNo = BoardRegistrationNo,

            });


            var cUETDocuments = APIHelper.GetCandidatesDocuments(new CUSCUETAPIRequest
            {

                ApplicationNo = ApplicationNumber,
                DOB = DateTime.Parse(DateOfBirth),
                SelectedPorgramme = (PrintProgramme)Enum.Parse(typeof(PrintProgramme), SelectedPorgramme.ToString()),
                BoardRegistrationNo = BoardRegistrationNo,
            });

            cUETRegistrationData.CandidateDoc = cUETDocuments.CandidateDoc;
            #endregion

            return PartialView(cUETRegistrationData);
        }

        [HttpPost]
        public ActionResult Import(CUSCUETAPIRequest CUETAPIRequest)
        {
            if (!ModelState.IsValid)
            {
                TempData["importmessage"] = "All the fields are required";
                return RedirectToAction("Search");
            }

            Tuple<bool, string> importResponse = APIHelper.ImportCUETDetails(CUETAPIRequest);

            TempData["importmessage"] = importResponse.Item2;
            return RedirectToAction("Search");
        }
    }
}