using CUSrinagar.DataManagers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using CUSrinagar.Models;
using System.Web;
using CUSrinagar.Extensions;
using CUSrinagar.Enums;

namespace CUSrinagar.BusinessManagers
{
    public class AccountManager
    {
        #region AppUsers
        public AppUsers GetItem(Guid User_ID)
        {
            if (User_ID == Guid.Empty) return null;
            AccountDB accountDB = new AccountDB();
            AppUsers appUsers = accountDB.GetItem(User_ID);
            if (appUsers == null)
                return null;
            appUsers.UserRoles = accountDB.GetUserRoles(User_ID);
            return appUsers;
        }
        public Guid? SignIn(AppLogin appLogin)
        {
            if (appLogin == null) return null;
            return new AccountDB().SignIn(appLogin);
        }
        public AppUsers GetUser(string userName)
        {
            Guid User_ID = new AccountDB().GetUser_ID(userName);
            if (User_ID == Guid.Empty) return null;
            AccountDB accountDB = new AccountDB();
            AppUsers appUsers = accountDB.GetItem(User_ID);
            if (appUsers == null)
                return null;
            return appUsers;
        }

        public bool ForwardPasswordLink(string userName)
        {
            AppUsers AppUser = new AccountManager().GetUser(userName + "");
            if (AppUser != null)
            {
                string baseUrl = GetSiteUrl();
                string generatedToken = Guid.NewGuid().ToString();
                if (new AccountDB().SetToken(AppUser.User_ID, generatedToken) > 0)
                {
                    string resetPasswordLink = $"{baseUrl}/Account/ReSetPassword?generatedToken={generatedToken}";
                    string Subject = "Retrieve your password ";
                    string Message = $@"<p>
                                     You have requested a password reset, please follow the link below to reset your password.
                                    </p>
                                   <p>
                                   <a href = '{resetPasswordLink }'>
                                     Follow this link to reset your password.
                                 </a>
                                 </p> ";
                    new TaskFactory().StartNew(() => new EmailSystem().SendMail(AppUser.Email, Subject, Message, true));
                    return true;
                }
            }
            return false;
        }

        public static string GetSiteUrl()
        {
            string url = string.Empty;
            HttpRequest request = HttpContext.Current.Request;

            if (request.IsSecureConnection)
                url = "https://";
            else
                url = "http://";

            url += request["HTTP_HOST"];

            return url;
        }

        public bool ChangePassword(ForgotPassword model)
        {
            if (model != null)
            {
                if (!new AccountDB().ComparePassword(model.UserName, model.Password.Encrypt()))
                {
                    if (new AccountDB().UpdatePassword(model.UserName, model.Password.Encrypt()) > 0)
                    {
                        new AccountDB().ReSetToken(model.UserName);
                        return true;
                    }
                }
            }
            return false;
        }

        public bool ChangeUserPassword(ForgotPassword model)
        {
            if (model != null)
            {
                model.UserName = GetItem(AppUserHelper.User_ID)?.UserName;
                if (!new AccountDB().ComparePassword(model.UserName, model.Password.Encrypt()))
                {
                    if (new AccountDB().UpdatePassword(model.UserName, model.Password.Encrypt()) > 0)
                    {
                        new AccountDB().ReSetToken(model.UserName);
                        return true;
                    }
                }
            }
            return false;
        }
        public ForgotPassword GetUserByToken(string token)
        {
            AppUsers appUsers = new AccountDB().GetUserByToken(token);
            if (appUsers == null)
                return null;
            ForgotPassword appLogin = new ForgotPassword() { UserName = appUsers.UserName };

            return appLogin;
        }
        #endregion

        #region StudentZone
        public Guid SignIn(ARGReprint aRGReprint)
        {
            if (aRGReprint != null && (string.IsNullOrWhiteSpace(aRGReprint.FormNo) || aRGReprint.DOB == DateTime.MinValue))
                return Guid.Empty;
            aRGReprint.PrintProgrammeOption = new RegistrationManager().MappingTable(aRGReprint.PrintProgrammeOption);
            return new AccountDB().SignIn(aRGReprint);
        }

        public ARGPersonalInformation GetStudentByID(Guid ID, PrintProgramme printProgramme)
        {
            printProgramme = new RegistrationManager().MappingTable(printProgramme);
            ARGPersonalInformation _ARGPersonalInformation = new RegistrationDB().GetStudentByID(ID, printProgramme);
            if (_ARGPersonalInformation == null)
                return null;
            _ARGPersonalInformation.StudentAddress = new RegistrationDB().GetStudentAddress(ID, printProgramme);// new ARGStudentAddress() { };
            return _ARGPersonalInformation;
        }
        #endregion
    }
}
