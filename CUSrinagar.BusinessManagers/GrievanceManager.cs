using CUSrinagar.DataManagers;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using CUSrinagar.Models;
using GeneralModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;

namespace CUSrinagar.BusinessManagers
{
    public class GrievanceManager
    {

        public ResponseData SaveGrievance(Grievance grievance, bool IsAdmin = false)
        {
            var ValidationResponse = ValidateModel(grievance);
            if (!ValidationResponse.IsSuccess)
                return ValidationResponse;
            var response = new ResponseData();
            grievance.VerificationCode = new Random().Next(1000, 9999).ToString();
            if (IsAdmin)
                grievance.SetWorkFlow(RecordState.New);
            response.NumberOfRecordsEffected = new GrievanceDB().SaveGrievance(grievance);
            if (response.NumberOfRecordsEffected == 1)
            {
                UpdateGrievanceCount();
                SendSMSAsync(grievance.VerificationCode, grievance.Email, false);
                response.IsSuccess = true;
                response.ResponseObject = grievance;
            }
            return response;
        }

        private void UpdateGrievanceCount()
        {
            new GrievanceDB().UpdateGrievanceCount();
        }

        public ResponseData SaveGrievanceReply(GrievanceReply grievanceReply, bool IsAdmin = false)
        {
            //var ValidationResponse = ValidateModel(grievanceReply);
            //if (!ValidationResponse.IsSuccess) return ValidationResponse;
            var grievance = GetGrievance(grievanceReply.Grievance_ID);
            var response = new ResponseData();
            grievanceReply.GrievanceReply_ID = Guid.NewGuid();
            if (grievance.IsDiscarded)
                grievanceReply.Message = "Discarded:" + grievance.Message;
            if (IsAdmin)
            {
                grievanceReply.SetWorkFlow(RecordState.New);
                grievanceReply.FullName = AppUserHelper.AppUsercompact.FullName;
                grievanceReply.UserAssigned_ID = grievance.UserAssigned_ID;
                grievanceReply.MarkedAsResolved = true;
            }
            try
            {
                using (var transcopre = new TransactionScope())
                {
                    response.NumberOfRecordsEffected = new GrievanceDB().SaveGrievance(grievanceReply);
                    var status = 0;
                    if (response.NumberOfRecordsEffected == 1)
                    {
                        grievance.Status = GrievanceStatus.Resolved;
                        grievance.IsDiscarded = grievanceReply.IsDiscarded;
                        grievance.AllowViewPublic = grievanceReply.AllowViewPublic;
                        status = new GrievanceDB().UpdateGrievance(grievance);
                        response.ResponseObject = grievanceReply;
                    }
                    if (response.NumberOfRecordsEffected != 1 || status != 1)
                        throw new TransactionAbortedException();
                    response.IsSuccess = true;
                    transcopre.Complete();
                }
            }
            catch (TransactionAbortedException) { }
            if (response.IsSuccess)
            {
                MailGrievanceReply(grievance.Email, $"Your query [{grievance.Message}]<br/>Reply:[{grievanceReply.Message}]");
            }
            return response;
        }

        public Grievance GetGrievance(Guid grievance_ID)
        {
            Grievance grievance = new GrievanceDB().GetGrievance(grievance_ID);
            if (grievance != null)
                grievance.GrievanceReplies = new GrievanceDB().GetGrievanceReplies(grievance.Grievance_ID);
            return grievance;
        }

        public ResponseData AssignGrievanceTo(Guid Grievance_ID, Guid UserAssignedTo_ID)
        {
            var response = new ResponseData();
            var grievance = GetGrievance(Grievance_ID);
            var user = new UserProfileManager().GetAPUserInfoById(UserAssignedTo_ID);
            //validate grievane and user
            var grievanceReply = new GrievanceReply()
            {
                GrievanceReply_ID = Guid.NewGuid(),
                Grievance_ID = Grievance_ID,
                IsAssignmentProcess = true,
                Date = DateTime.Now,
                FullName = user.FullName,
                Message = $"This query has been forwarded to {user.FullName} ({user.Designation})",
                UserAssigned_ID = user.User_ID
            };
            try
            {
                using (var transactionScope = new TransactionScope())
                {
                    response.NumberOfRecordsEffected = new GrievanceDB().SaveGrievance(grievanceReply);
                    var status = 0;
                    if (response.NumberOfRecordsEffected == 1)
                    {
                        grievance.UserAssigned_ID = UserAssignedTo_ID;
                        grievance.Status = GrievanceStatus.Forwarded;
                        status = new GrievanceDB().UpdateGrievance(grievance);
                    }
                    if (response.NumberOfRecordsEffected != 1 || status != 1)
                    {
                        throw new TransactionAbortedException();
                    }
                    response.IsSuccess = true;
                    transactionScope.Complete();
                }
            }
            catch (TransactionAbortedException) { }
            if (response.IsSuccess)
                MailGrievanceReply(grievance.Email, $"Your query [{grievance.Message}[ has been forwarded to [{user.FullName} ({user.Designation})]");
            return response;
        }
        public ResponseData Discard(Guid Grievance_ID)
        {
            var response = new ResponseData();
            var grievance = GetGrievance(Grievance_ID);
            var grievanceReply = new GrievanceReply()
            {
                GrievanceReply_ID = Guid.NewGuid(),
                Grievance_ID = Grievance_ID,
                Date = DateTime.Now,
                FullName = AppUserHelper.AppUsercompact.FullName,
                Message = $"This query has been discarded by { AppUserHelper.AppUsercompact.FullName} ({AppUserHelper.Designation})",
                UserAssigned_ID = AppUserHelper.User_ID
            };
            grievanceReply.SetWorkFlow(RecordState.New);
            try
            {
                using (var transactionScope = new TransactionScope())
                {
                    response.NumberOfRecordsEffected = new GrievanceDB().SaveGrievance(grievanceReply);
                    var status = 0;
                    if (response.NumberOfRecordsEffected == 1)
                    {
                        grievance.UserAssigned_ID = AppUserHelper.User_ID;
                        grievance.Status = GrievanceStatus.Resolved;
                        grievance.AllowViewPublic = grievanceReply.AllowViewPublic;
                        grievance.IsDiscarded = true;
                        grievance.SetWorkFlow(RecordState.Old);
                        status = new GrievanceDB().UpdateGrievance(grievance);
                    }
                    if (response.NumberOfRecordsEffected != 1 || status != 1)
                    {
                        throw new TransactionAbortedException();
                    }
                    response.IsSuccess = true;
                    transactionScope.Complete();
                }
                MailGrievanceReply(grievance.Email, "Your query has been dicarded");
            }
            catch (TransactionAbortedException) { }
            return response;
        }

        public List<GrievanceWidgetSummary> GetGrievanceWidgetsData(Guid? college_ID, GrievanceCategory? grievanceCategory)
        {
            return new GrievanceDB().GetGrievanceWidgetsData(college_ID, grievanceCategory);
        }

        void MailGrievanceReply(string Email, string Message)
        {
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
                                       {Message}.<strong>.
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
            new EmailSystem().SendMailAsyc(Email, mailSubject, body, true);
        }

        public GrievanceGeneralWidgetSummary GetGrievanceWidgetsGeneral(Guid? college_ID, GrievanceCategory? grievanceCategory)
        {
            return new GrievanceDB().GetGrievanceWidgetsGeneral(college_ID, grievanceCategory);
        }

        public GrievanceAssignedWidgetSummary GetGrievanceWidgetsAssigned(Guid? college_ID, GrievanceCategory? grievanceCategory)
        {
            return new GrievanceDB().GetGrievanceWidgetsAssigned(college_ID, grievanceCategory);
        }

        public GrievanceVerifiedWidgetSummary GetGrievanceWidgetsVerified(Guid? college_ID, GrievanceCategory? grievanceCategory)
        {
            return new GrievanceDB().GetGrievanceWidgetsVerified(college_ID, grievanceCategory);
        }

        public GrievanceResolvedWidgetSummary GetGrievanceWidgetsResolved(Guid? college_ID, GrievanceCategory? grievanceCategory)
        {
            return new GrievanceDB().GetGrievanceWidgetsResolved(college_ID, grievanceCategory);
        }

        //void ValidateEmail(Grievance model)
        //{
        //    string actionUrl = Url.Action("EmailConfirmation", "Grievance");
        //    string query = "id=" + model.Grievance_ID + "&code=" + model.VerificationCode.EncryptCookieAndURLSafe();
        //    var url = AbsoluteUrl(actionUrl, query);
        //    //var body = $@"<a  href='{url}' target='_blank'>Click here</a>";
        //    string mailSubject = $"CLUSTER UNIVERSITY SRINAGAR - Grievance Confirmation.";
        //    string body = $@"<!DOCTYPE html>
        //                        <html>
        //                        <head>
        //                            <meta charset='utf-8' />
        //                            <title></title>
        //                        </head>
        //                        <body>
        //                            <div style='margin:0 auto;width:600px;border:1px solid grey; padding:20px'>
        //                            <div style='background-color:#dfe7e8; padding:10px;'>        
        //                                <img src='http://cusrinagar.edu.in/Content/ThemePublic/PrintImages/cuslogoEmail.png' alt='Cu Srinagar' title='Cu Srinagar' style='margin-left:50px;'>
        //                            </div>
        //                            <p style='font-size:20px'> 
        //                               You have one more step remaining to forward your query. .<strong><br/><br/>Click on the button below to verify your email address:.
        //                            </p>
        //                                <br />
        //                                <table border='0' style='font-size:20px;'>
        //                                    <tr>
        //                                     <td>Greivance ID : &nbsp;&nbsp;&nbsp;</td>
        //                                    </tr>
        //                                    <tr>                                               
        //                                         <td  style='border-radius:3px;padding:12px 20px 16px 20px;background-color:#d90007' valign='top' align='center'>
        //                                            <a href = {url} style='font-family:Helvetica,Arial,sans-serif;font-size:16px;color:#ffffff;background-color:#d90007;border-radius:3px;text-align:center;text-decoration:none;display:block;margin:0' target = '_blank'>
        //                                                Verify my email
        //                                            </a>
        //                                        </td>     
        //                                     </tr> 
        //                                      <tr>
        //                                    <td>Didn’t work? Copy the link below into your web browser: </td>
        //                                    </tr>
        //                                    <tr>
        //                                     <td>{url}</td>
        //                                    </tr>
        //                                </table>
        //                                <br/>
        //                                <hr/>
        //                        <strong>
        //                            Cluster University Srinagar
        //                        </strong>
        //                                <p>
        //                                    Zoology Block , S.P College Campus.<br/>
        //                                    M.A Road Srinagar.<br />
        //                                    Website : www.cusrinagar.edu.in
        //                                </p>
        //                                <br/>
        //                                <br />
        //                                <div style='text-align:center;padding:50px;background-color:#dfe7e8;'>
        //                                    This is a system generated Email. Please do not reply to this Email.
        //                                </div>

        //                            </div>
        //                        </body>
        //                        </html>";
        //    new EmailSystem().SendMailAsyc(model.Email, mailSubject, body, true);
        //}

        public ResponseData BatchUpdate(BatchUpdateGrievance model)
        {
            var response = new ResponseData();
            if (model == null || model.Grievance_IDs.IsNullOrEmpty())
                return null;

            foreach (var Grievance_ID in model.Grievance_IDs)
            {
                var grievance = GetGrievance(Grievance_ID);
                if (grievance.Status == GrievanceStatus.Resolved)
                    continue;
                AppUsers user = null;
                if (model.UpdateUserAssigned && user == null)
                    user = new UserProfileManager().GetAPUserInfoById(model.UserAssigned_ID);

                if (model.UpdateUserAssigned && user == null)
                    continue;

                var grievanceReply = new GrievanceReply()
                {
                    GrievanceReply_ID = Guid.NewGuid(),
                    Grievance_ID = Grievance_ID,
                    IsAssignmentProcess = true,
                    Date = DateTime.Now,
                    FullName = user.FullName,
                    Message = $"This query has been forwarded to {user.FullName} ({user.Designation})",
                    UserAssigned_ID = user.User_ID
                };
                grievanceReply.SetWorkFlow(RecordState.New);
                try
                {
                    using (var transactionScope = new TransactionScope())
                    {
                        var replystatus = new GrievanceDB().SaveGrievance(grievanceReply);
                        var greivancestatus = 0;
                        if (replystatus == 1)
                        {
                            grievance.UserAssigned_ID = user.User_ID;
                            grievance.Status = GrievanceStatus.Forwarded;
                            greivancestatus = new GrievanceDB().UpdateGrievance(grievance);
                        }
                        if (replystatus != 1 || greivancestatus != 1)
                        {
                            throw new TransactionAbortedException();
                        }
                        response.IsSuccess = true;
                        response.NumberOfRecordsEffected += 1;
                        transactionScope.Complete();
                    }
                }
                catch (TransactionAbortedException) { }
            }
            return response;
        }

        public List<GrievanceList> GetGrievanceListCompact(Parameters parameter)
        {

            if (HttpContext.Current.User is IPrincipalProvider)
            {
                if (!AppUserHelper.AppUsercompact.UserRoles.Contains(AppRoles.University))
                    parameter.Filters.Add(new SearchFilter() { Column = "UserAssigned_ID", Operator = SQLOperator.EqualTo, Value = AppUserHelper.User_ID.ToString() });
            }

            if (parameter == null)
                parameter = new Parameters();
            if (parameter.Filters.IsNullOrEmpty())
                parameter.Filters = new List<SearchFilter>();
            parameter.Filters.Add(new SearchFilter() { Column = "IsNumberVerified", GroupOperation = LogicalOperator.AND, Operator = SQLOperator.EqualTo, Value = "1" });

            if (parameter.SortInfo.ColumnName.IsNullOrEmpty())
                parameter.SortInfo = new Sort() { ColumnName = "Date", OrderBy = System.Data.SqlClient.SortOrder.Descending };

            return new GrievanceDB().GetGrievanceListCompact(parameter);
        }

        public List<GrievanceList> GetTop10GrievanceListCompact()
        {
            return new GrievanceDB().GetTop10GrievanceListCompact();
        }

        private ResponseData ValidateModel(GrievanceReply grievanceReply)
        {
            ResponseData response = new ResponseData() { IsSuccess = true };

            if (new GeneralFunctions().CheckEntityExists<Grievance>(grievanceReply.Grievance_ID.ToString(), grievanceReply.GrievanceReply_ID.ToString()))
            {
                response.IsSuccess = false;
                response.ErrorMessage = " Incorrect Grievance for GrievanceReply.";
            }
            return response;
        }
        private ResponseData ValidateModel(Grievance grievance)
        {
            ResponseData response = new ResponseData() { IsSuccess = true };

            if (string.IsNullOrWhiteSpace(grievance.FullName) || string.IsNullOrWhiteSpace(grievance.Email) || string.IsNullOrWhiteSpace(grievance.Message) || string.IsNullOrWhiteSpace(grievance.Subject))
            {
                response.IsSuccess = false;
                response.ErrorMessage = " Please check Full Name / Email / Message / Subject and Try again";
                return response;
            }
            //PersonalInformationCompact stdInfo = new StudentManager().GetStudentCompactByRegistrationNo(grievance.CUSRegistrationNo);
            //if (stdInfo == null)
            //{
            //    response.IsSuccess = false;
            //    response.ErrorMessage = "Invalid registration number";
            //    return response;
            //}
            //grievance.Student_ID = stdInfo.Student_ID;
            grievance.Date = DateTime.Now;
            grievance.Grievance_ID = Guid.NewGuid();
            grievance.GrievanceID = GetGrievanceNumber(grievance.Category);
            grievance.Status = GrievanceStatus.Received;
            return response;
        }
        private string GetGrievanceNumber(GrievanceCategory category)
        {
            Settings settings = new GeneralFunctions().GetSettings(Module.Grievance);
            return DateTime.Now.Year.ToString().Substring(2, 2) + category.ToString().ToUpper().Substring(0, 3) + (string.IsNullOrWhiteSpace(settings.Prefix) ? "" : settings.Prefix) + (settings.Count + 1) + (string.IsNullOrWhiteSpace(settings.Suffix) ? "" : settings.Suffix);
        }
        public ResponseData VerifyGrievance(Guid id, string code)
        {
            var response = new ResponseData();
            Grievance grievance = new GrievanceManager().GetGrievance(id);

            if (grievance.IsNumberVerified == true)
                return new ResponseData() { ErrorMessage = "Number already verified" };
            if (grievance.VerificationCode != code.Trim())
                return new ResponseData() { ErrorMessage = "Invalid OTP Code" };
            else
            {
                grievance.IsNumberVerified = true;
                response.NumberOfRecordsEffected = new GrievanceDB().UpdateGrievance(grievance);
            }
            if (response.NumberOfRecordsEffected == 1)
            {
                //SendSMSAsync(grievance.GrievanceID, grievance.PhoneNumber, true);
                response.IsSuccess = true;
                response.ResponseObject = grievance;
            }
            else
            {
                response.ErrorMessage = "Something went wrong";
            }
            return response;
        }

        public void SendSMSAsync(string Code, string email, bool isSuccess)
        {
            new TaskFactory().StartNew(() => SendEmailAndSMS(Code, email, isSuccess));
        }
        public void SendEmailAndSMS(string Code, string email, bool isSuccess)
        {
            if (isSuccess)
                new EmailSystem().SendMail(email, "Cluster University Srinagar GrievanceID - Student HelpDesk",
                    $"Your GrievanceID is {Code}.{Environment.NewLine} Please note this number for reference to follow up the grievance.", false);
            else
                new EmailSystem().SendMail(email, "Cluster University Srinagar OTP - Student HelpDesk",
                    $"Your OTP Code is {Code}.{Environment.NewLine} Please enter this code for submitting your grievance.", false);

        }

        public Tuple<bool, string> SaveUpdateAlomini(Alomini alomini)
        {
            if (alomini == null)
                return Tuple.Create(false, "Invalid details");

            alomini.Student_ID = AppUserHelper.User_ID;
            alomini.LastSavedOn = DateTime.Now;
            int result = new GrievanceDB().SaveUpdateAlomini(alomini);
            if (result > 0)
                return Tuple.Create(true, "Saved Successfully");
            else
                return Tuple.Create(false, "unable to save, try again later");
        }
        public Alomini GetAlomini(Guid Student_ID)
        {
            return new GrievanceDB().GetAlomini(Student_ID);
        }
    }
}
