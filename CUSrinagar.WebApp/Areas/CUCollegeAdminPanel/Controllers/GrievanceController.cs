using CUSrinagar.Enums;
using CUSrinagar.Models;
using CUSrinagar.OAuth;
using System;
using CUSrinagar.BusinessManagers;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CUSrinagar.Extensions;
using GeneralModels;

namespace CUSrinagar.WebApp.CUCollegeAdminPanel.Controllers
{
    [OAuthorize(AppRoles.University_GrievanceFeedback)]
    public class GrievanceController : CollegeAdminBaseController
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
            parameter.Filters.Add(new SearchFilter() { Column = "UserAssigned_ID", GroupOperation = LogicalOperator.AND, Operator = SQLOperator.EqualTo, Value = AppUserHelper.User_ID.ToString() });
            parameter.SortInfo = new Sort() { ColumnName = "Date", OrderBy = System.Data.SqlClient.SortOrder.Descending };
            List<GrievanceList> list = new GrievanceManager().GetGrievanceListCompact(parameter);
            return PartialView(list);
        }

        public ActionResult Detail(Guid? id)
        {
            ViewBag.LoggedInUser = AppUserHelper.User_ID;
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
            //model.UserAssigned_ID = AppUserHelper.User_ID;
            response = new GrievanceManager().SaveGrievanceReply(model,true);
            SetResponseViewBags(response);
            if (response.IsSuccess)
            {//send mail to cadidate                
                var gr = new GrievanceManager().GetGrievance(model.Grievance_ID);
                //MailGrievanceReply(gr, model);
            }
            return RedirectToAction("Detail", "Grievance", new { @area = "CUCollegeAdminPanel", @id = model.Grievance_ID });
        }

        public ActionResult AssignToMe(Guid? id)
        {
            ResponseData response = new ResponseData();
            if (id.HasValue)
            {
                response = new GrievanceManager().AssignGrievanceTo(id.Value, AppUserHelper.User_ID);
            }
            SetResponseViewBags(response);
            return RedirectToAction("Detail", "Grievance", new { @area = "CUCollegeAdminPanel", @id = id });
        }
        public ActionResult Discard(Guid? id)
        {
            ResponseData response = new ResponseData();
            if (id.HasValue)
            {
                response = new GrievanceManager().AssignGrievanceTo(id.Value, AppUserHelper.User_ID);
            }
            SetResponseViewBags(response);
            return RedirectToAction("Detail", "Grievance", new { @area = "CUCollegeAdminPanel", @id = id });
        }

        public JsonResult BatchUpdate(BatchUpdateGrievance model)
        {
            ResponseData response = response = new GrievanceManager().BatchUpdate(model);
            return Json(response);
        }

        #region HelperMethods
        void FillViewBags()
        {
            ViewBag.GrievanceCategory = Helper.GetSelectList<GrievanceCategory>().OrderBy(x => x.Text);
            ViewBag.GrievanceStatus = Helper.GetSelectList<GrievanceStatus>().OrderBy(x => x.Text);
        }
        void MailGrievanceReply(Grievance grievance, GrievanceReply grievanceReply)
        {
            //string actionUrl = Url.Action("EmailConfirmation", "Grievance");
            //string query = "id=" + grievance.Grievance_ID + "&code=" + grievance.VerificationCode.EncryptCookieAndURLSafe();
            //var url = "";// AbsoluteUrl(actionUrl, query);
            string mailSubject = $"Cluster University - Grievance Reply.";
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
                                       Your query has been answered .<strong><br/><br/>{grievanceReply.Message}.
                                    </p>
                                        <br />
                                       
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
            new EmailSystem().SendMailAsyc(grievance.Email, mailSubject, body, true);
        }
        #endregion


    }
}