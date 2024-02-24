using CUSrinagar.Enums;
using CUSrinagar.Models;
using System;
using CUSrinagar.BusinessManagers;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CaptchaMvc.HtmlHelpers;
using CUSrinagar.Extensions;
using GeneralModels;

namespace CUSrinagar.WebApp.Controllers
{
    public class GrievanceController : WebAppBaseController
    {
        public ActionResult Grievance()
        {
            FillViewBags();
            return View();
        }
        public ActionResult GrievanceList(Parameters parameter)
        {
            if (parameter == null) parameter = new Parameters();
            if (parameter.Filters.IsNullOrEmpty()) parameter.Filters = new List<SearchFilter>();
            parameter.Filters.Add(new SearchFilter() { Column = "AllowViewPublic", GroupOperation = LogicalOperator.AND, Operator = SQLOperator.EqualTo, Value = "1" });
            parameter.SortInfo = new Sort() { ColumnName = "Date", OrderBy = System.Data.SqlClient.SortOrder.Descending };
            List<GrievanceList> list = new GrievanceManager().GetGrievanceListCompact(parameter);
            return PartialView(list);
        }
        public ActionResult GrievanceForm()
        {
            Grievance model = (Grievance)TempData["model"] ?? new Grievance();
            FillViewBags();
            GetResponseViewBags();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GrievanceForm(Grievance model)
        {
            ResponseData response = new ResponseData();
            if (!(this?.IsCaptchaValid("") ?? true))
            {
                TempData["ErrorMessage"] = "Invalid Captcha";
                TempData["model"] = model;
                return RedirectToAction("GrievanceForm");
            }
            if (ModelState.IsValid)
            {
                model.Date = DateTime.Now;
                response = new GrievanceManager().SaveGrievance(model);
            }
            else
            {
                TempData["model"] = model;
                response.ErrorMessage = string.Join(", ", ModelState.Values.SelectMany(s => s.Errors).Select(y => y.ErrorMessage).ToArray());
                //string invalidDataErrorMessage = ViewData.ModelState.Values.SelectMany(modelState => modelState.Errors).Aggregate("", (current, error) => current + error.ErrorMessage);
            }
            SetResponseViewBags(response);
            if (!response.IsSuccess)
            {
                TempData["model"] = model;
                return RedirectToAction("GrievanceForm");
            }
            TempData["model"] = response.ResponseObject;
            return RedirectToAction("OTPConfirmation", new { @id =model.Grievance_ID });
        }

        [HttpGet]
        public ActionResult OTPConfirmation(string id)
        {
            Guid Grievance_ID;
            Guid.TryParse(id, out Grievance_ID);
            if (Grievance_ID != Guid.Empty)
            {
                var grievance = (Grievance)TempData["model"];
                if (grievance == null)
                    grievance = new GrievanceManager().GetGrievance(Grievance_ID);
                ViewData.Model = grievance;
                GetResponseViewBags();
                return View();
            }
            else
            {
                return RedirectToAction("Grievance");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult OTPConfirmation(Guid? Grievance_ID, string OTP)
        {
            ResponseData response = new ResponseData() { ErrorMessage = "Invalid request" };
            if (Grievance_ID.HasValue && !string.IsNullOrEmpty(OTP))
            {
                response = new GrievanceManager().VerifyGrievance(Grievance_ID.Value, OTP);
                SetResponseViewBags(response);
                if (response.IsSuccess)
                {
                    var GrievanceID = ((Grievance)response.ResponseObject).GrievanceID.ToUpper();
                    return RedirectToAction("GrievanceConfirmation", new { @GrievanceID = GrievanceID, @IsSuccess = true });
                }
                else
                {
                    return RedirectToAction("OTPConfirmation", new { @id = Grievance_ID });
                }
            }
            else
            {
                return RedirectToAction("Grievance");
            }
        }
        public ActionResult GrievanceConfirmation(string GrievanceID, bool IsSuccess)
        {
            if (!string.IsNullOrEmpty(GrievanceID))
            {
                ViewBag.GrievanceID = GrievanceID;
                ViewBag.IsVerified = IsSuccess;
                GetResponseViewBags();
                return View();
            }
            else
            {
                return RedirectToAction("Grievance");
            }
        }

        public ActionResult Detail(Guid? id)
        {
            if (id.HasValue)
            {
                GetResponseViewBags();
                ViewData.Model = new GrievanceManager().GetGrievance(id.Value);
            }
            return View();
        }
        public ActionResult PostGrievanceReply(GrievanceReply model)
        {
            ResponseData response = new ResponseData();
            model.Date = DateTime.Now;
            response = new GrievanceManager().SaveGrievanceReply(model);
            SetResponseViewBags(response);
            if (response.IsSuccess)
            {//send mail to cadidate                
                var gr = new GrievanceManager().GetGrievance(model.Grievance_ID);
                //MailGrievanceReply(gr, model);
            }
            return RedirectToAction("Detail", "Grievance", new { @id = model.Grievance_ID });
        }

        #region HelperMethods
        void FillViewBags()
        {
            ViewBag.GrievanceCategory = Helper.GetSelectList<GrievanceCategory>();
           // ViewBag.GrievanceStatus = Helper.GetSelectList<GrievanceStatus>().OrderBy(x => x.Text);
           // ViewBag.Colleges = new CollegeManager().GetADMCollegeMasterList();
        }
        void ValidateEmail(Grievance model)
        {
            string actionUrl = Url.Action("EmailConfirmation", "Grievance");
            string query = "id=" + model.Grievance_ID + "&code=" + model.VerificationCode.EncryptCookieAndURLSafe();
            var url = AbsoluteUrl(actionUrl, query);
            //var body = $@"<a  href='{url}' target='_blank'>Click here</a>";
            string mailSubject = $"CLUSTER UNIVERSITY SRINAGAR - Grievance Confirmation.";
            string body = $@"<!DOCTYPE html>
                                <html>
                                <head>
                                    <meta charset='utf-8' />
                                    <title></title>
                                </head>
                                <body>
                                    <div style='margin:0 auto;width:600px;border:1px solid grey; padding:20px'>
                                    <div style='background-color:#dfe7e8; padding:10px;'>        
                                        <img src='http://cusrinagar.edu.in/Content/ThemePublic/PrintImages/cuslogoEmail.png' alt='Cu Srinagar' title='Cu Srinagar' style='margin-left:50px;'>
                                    </div>
                                    <p style='font-size:20px'> 
                                       You have one more step remaining to forward your query. .<strong><br/><br/>Click on the button below to verify your email address:.
                                    </p>
                                        <br />
                                        <table border='0' style='font-size:20px;'>
                                            <tr>
                                             <td>Greivance ID : &nbsp;&nbsp;&nbsp;</td>
                                            </tr>
                                            <tr>                                               
                                                 <td  style='border-radius:3px;padding:12px 20px 16px 20px;background-color:#d90007' valign='top' align='center'>
                                                    <a href = {url} style='font-family:Helvetica,Arial,sans-serif;font-size:16px;color:#ffffff;background-color:#d90007;border-radius:3px;text-align:center;text-decoration:none;display:block;margin:0' target = '_blank'>
                                                        Verify my email
                                                    </a>
                                                </td>     
                                             </tr> 
                                              <tr>
                                            <td>Didn’t work? Copy the link below into your web browser: </td>
                                            </tr>
                                            <tr>
                                             <td>{url}</td>
                                            </tr>
                                        </table>
                                        <br/>
                                        <hr/>
                                <strong>
                                    Cluster University Srinagar
                                </strong>
                                        <p>
                                            Zoology Block , S.P College Campus.<br/>
                                            M.A Road Srinagar.<br />
                                            Website : www.cusrinagar.edu.in
                                        </p>
                                        <br/>
                                        <br />
                                        <div style='text-align:center;padding:50px;background-color:#dfe7e8;'>
                                            This is a system generated Email. Please do not reply to this Email.
                                        </div>

                                    </div>
                                </body>
                                </html>";
            new EmailSystem().SendMailAsyc(model.Email, mailSubject, body, true);
        }
        #endregion


    }
}