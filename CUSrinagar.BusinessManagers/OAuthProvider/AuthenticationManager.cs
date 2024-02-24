using CUSrinagar.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI.WebControls;
using CUSrinagar.Enums;
using System.Web.Script.Serialization;
using CUSrinagar.Extensions;
using CUSrinagar.DataManagers;
using System.Threading.Tasks;

namespace CUSrinagar.BusinessManagers
{
    public class AuthenticationManager
    {
        #region General OAuthRoutines
        public Tuple<SignInStatus, string> SignIn(AppLogin appLogin)
        {
            if (appLogin == null || string.IsNullOrWhiteSpace(appLogin.UserName) || string.IsNullOrWhiteSpace(appLogin.Password))
                return Tuple.Create(SignInStatus.Failed, "Please provide your login credentials");

            string _message = string.Empty;
            SignInStatus _SignInStatus = SignInStatus.Failed;

            Guid? User_ID = new AccountManager().SignIn(appLogin);

            if (User_ID != null && User_ID != Guid.Empty)
            {
                #region on Success login
                if (IsAccountDisabled((Guid)User_ID))
                    return Tuple.Create(SignInStatus.Disabled, $"Your account with Username/Email {appLogin.UserName} has been disabled");

                if (IsAccountLocked((Guid)User_ID))
                    return Tuple.Create(SignInStatus.LockedOut, $"Your account with Username/Email {appLogin.UserName} has been locked for {AuthCookie.MaxLockHours} hours.");

                new AccountDB().RemoveAccountLockout((Guid)User_ID);
                AppUserCompact serializeModel = FillAppUserCompact((Guid)User_ID);

                var userData = new JavaScriptSerializer().Serialize(serializeModel);
                DateTime _expiryDate = appLogin.RememberMe ? DateTime.UtcNow.AddDays(2) : DateTime.MinValue;
                var authTicket = new FormsAuthenticationTicket(
                                2,
                                serializeModel.FullName,
                                DateTime.UtcNow,
                                _expiryDate,
                                false,//appLogin.RememberMe, // for security reasons set to false
                                userData.EncryptCookieAndURLSafe());//custom data

                string encTicket = FormsAuthentication.Encrypt(authTicket);
                HttpCookie authCookie = new HttpCookie(AuthCookie.Name, encTicket);
                authCookie.HttpOnly = true;
                if (appLogin.RememberMe)
                {
                    //authCookie.Expires = _expiryDate; // code commented for security reasons 
                }
                HttpContext.Current.Response.Cookies.Add(authCookie);
                _SignInStatus = SignInStatus.Success;
                if (serializeModel.UserRoles?.Any(x => (x.ToString().IndexOf(AppRoles.College.ToString(), StringComparison.InvariantCultureIgnoreCase) > -1)) ?? false)
                    _message = AppRoles.College.ToString();
                else if (serializeModel.UserRoles?.Any(x => (x.ToString().IndexOf(AppRoles.University.ToString(), StringComparison.InvariantCultureIgnoreCase) > -1)) ?? false)
                    _message = AppRoles.University.ToString();
                #endregion
            }
            else
            {
                #region invalid account
                //check for lockout and attempts or unrecognized account
                _SignInStatus = SignInStatus.Failed;

                bool userExists = UserNameExists(appLogin.UserName);
                if (userExists)
                {
                    if (IsAccountDisabled(GetUser_ID(appLogin.UserName)))
                        return Tuple.Create(SignInStatus.Disabled, $"Your account with Username/Email {appLogin.UserName} has been disabled");

                    int AccessFailedCount = GetAccessFailedCount(appLogin.UserName);
                    if (AccessFailedCount >= AuthCookie.MaxLoginAttempts)
                    {
                        if (IsAccountLocked(GetUser_ID(appLogin.UserName)))
                            return Tuple.Create(SignInStatus.LockedOut, $"Your account with Username/Email {appLogin.UserName} has been locked for {AuthCookie.MaxLockHours} hours.");
                        AccessFailedCount = GetAccessFailedCount(appLogin.UserName);
                    }
                    if (IsAccountLockedEnabled(appLogin.UserName))
                    {
                        UpdateAccessFailedCount(appLogin.UserName);
                        //max attempts allowed
                        if ((AccessFailedCount + 1) >= AuthCookie.MaxLoginAttempts)
                        {
                            UpdateLockoutEndDateUtc(appLogin.UserName);
                            return Tuple.Create(SignInStatus.LockedOut, $"Your account with Username/Email {appLogin.UserName} has been locked for {AuthCookie.MaxLockHours} hours.");
                        }
                        else
                            _message = $"Wrong Username or Password. Attempts left {AuthCookie.MaxLoginAttempts - (AccessFailedCount + 1)}.";
                    }
                    else
                    {
                        _message = $"Wrong Username or Password.";
                    }
                }
                else
                {
                    _message = $"Wrong Username or Password.";
                }
                #endregion
            }
            return Tuple.Create(_SignInStatus, _message);
        }

        public void SignOut()
        {
            HttpCookie authCookie = new HttpCookie(AuthCookie.Name);
            if (authCookie != null)
            {
                authCookie.Expires = DateTime.UtcNow.AddDays(-7);
                HttpContext.Current.Response.Cookies.Add(authCookie);
            }
        }

        private AppUserCompact FillAppUserCompact(Guid User_ID)
        {
            AppUserCompact serializeModel = new AppUserCompact();
            AppUsers appUsers = new AccountManager().GetItem(User_ID);
            if (appUsers.College_ID != null && appUsers.College_ID != Guid.Empty)
            {
                ADMCollegeMaster aDMCollegeMaster = new CollegeManager().GetItem((Guid)appUsers.College_ID);
                serializeModel.CollegeCode = aDMCollegeMaster.CollegeCode;
            }
            serializeModel.FullName = appUsers.FullName;
            serializeModel.College_ID = appUsers.College_ID;
            serializeModel.User_ID = appUsers.User_ID;
            serializeModel.SecurityStamp = appUsers.SecurityStamp;
            serializeModel.EmailId = appUsers.Email;
            serializeModel.Designation = appUsers.Designation;
            serializeModel.LastPasswordResetDate = appUsers.LastPasswordResetDate;
            serializeModel.UserRoles = appUsers.UserRoles?.Select(x => x.RoleID).ToList();
            return serializeModel;
        }

        public bool LogMeOutOfOtherDevices(Guid User_ID, Guid securityStamp)
        {
            Guid DBSecurityStamp = new AccountDB().LogMeOutOfOtherDevices(User_ID);
            return !(securityStamp == DBSecurityStamp);
        }
        public bool LogMeOutOfOtherDevices(Guid User_ID, Guid securityStamp, PrintProgramme printProgramme)
        {
            Guid DBSecurityStamp = new AccountDB().LogMeOutOfOtherDevices(User_ID, printProgramme);
            return !(securityStamp == DBSecurityStamp);
        }

        public bool IsAccountDisabled(Guid User_ID)
        {
            return new AccountDB().IsAccountDisabled(User_ID);
        }
        public bool IsAccountLocked(Guid User_ID)
        {
            //enable after 5 hours
            DateTime lockeddateTime = new AccountDB().IsAccountLocked(User_ID);
            if (lockeddateTime == DateTime.MinValue)
                return false;

            if ((DateTime.UtcNow - lockeddateTime).TotalHours >= 0)
            {
                new AccountDB().RemoveAccountLockout(User_ID);
                return false;
            }
            else
                return true;

        }
        public int GetAccessFailedCount(string UserName)
        {
            return new AccountDB().GetAccessFailedCount(UserName);
        }
        public int UpdateAccessFailedCount(string UserName)
        {
            return new AccountDB().UpdateAccessFailedCount(UserName);
        }
        public int UpdateLockoutEndDateUtc(string UserName)
        {
            return new AccountDB().UpdateLockoutEndDateUtc(UserName, DateTime.UtcNow.AddHours(AuthCookie.MaxLockHours));
        }
        public bool UserNameExists(string UserName)
        {
            return new AccountDB().UserNameExists(UserName);
        }
        public bool IsAccountLockedEnabled(string UserName)
        {
            return new AccountDB().IsAccountLockedEnabled(UserName);
        }
        public Guid GetUser_ID(string UserName)
        {
            return new AccountDB().GetUser_ID(UserName);
        }
        #endregion


        #region StudentZoneRoutines
        public Tuple<SignInStatus, string> SignIn(ARGReprint appLogin)
        {
            if (appLogin == null || string.IsNullOrWhiteSpace(appLogin.FormNo) || appLogin.DOB == DateTime.MinValue)
                return Tuple.Create(SignInStatus.Failed, "Please provide your login cridentials");

            PrintProgramme printProgramme = appLogin.PrintProgrammeOption;
            string _message = string.Empty;
            SignInStatus _SignInStatus = SignInStatus.Failed;

            Guid? User_ID = new AccountManager().SignIn(appLogin);

            if (User_ID != null && User_ID != Guid.Empty)
            {
                if (new AccountDB().IsStudentPassout((Guid)User_ID, appLogin.PrintProgrammeOption))
                {
                    return Tuple.Create(SignInStatus.Failed, "It seems that you are passed out from University. Best wishes for your future endeavours.");
                }

                #region on Success login
                AppUserCompact serializeModel = FillAppUserCompactStudentZone((Guid)User_ID, printProgramme);
                var userData = new JavaScriptSerializer().Serialize(serializeModel);
                DateTime _expiryDate = DateTime.MinValue;
                var authTicket = new FormsAuthenticationTicket(
                                2,
                                serializeModel.FullName,
                                DateTime.UtcNow,
                                _expiryDate,
                                false,
                                userData.EncryptCookieAndURLSafe());//custom data

                string encTicket = FormsAuthentication.Encrypt(authTicket);
                HttpCookie authCookie = new HttpCookie(AuthCookie.Name, encTicket);
                authCookie.HttpOnly = true;
                authCookie.Expires = _expiryDate;
                HttpContext.Current.Response.Cookies.Add(authCookie);
                _SignInStatus = SignInStatus.Success;
                _message = AppRoles.Student.ToString();
                #endregion
            }
            else
            {
                #region invalid account
                FormStatus? formStatus = new AccountDB().SignInStatus(appLogin);
                _SignInStatus = SignInStatus.Failed;
                if (formStatus == null)
                {
                    _message = $"Details provided are not valid.";
                }
                else
                {
                    _message = $"Login not allowed ({formStatus?.GetEnumDescription()}).";
                }
                #endregion
            }
            return Tuple.Create(_SignInStatus, _message);
        }

        private AppUserCompact FillAppUserCompactStudentZone(Guid Student_ID, PrintProgramme printProgramme)
        {
            AppUserCompact serializeModel = new AppUserCompact();
            ARGPersonalInformation appUsers = new AccountManager().GetStudentByID(Student_ID, printProgramme);

            serializeModel.FullName = appUsers.FullName;
            serializeModel.User_ID = appUsers.Student_ID;
            serializeModel.SecurityStamp = appUsers.CreatedBy ?? Guid.Empty;
            serializeModel.EmailId = appUsers.StudentAddress.Email;
            serializeModel.Designation = "Student";
            serializeModel.TableSuffix = new RegistrationManager().MappingTable(printProgramme);
            serializeModel.OrgPrintProgramme = printProgramme;
            serializeModel.UserRoles = new List<AppRoles> { AppRoles.Student };
            serializeModel.College_ID = appUsers.AcceptCollege_ID;
            serializeModel.CollegeCode = appUsers.CollegeCode;
            serializeModel.OrgBatch = appUsers.Batch;
            return serializeModel;
        }
        #endregion

        /// <summary>
        /// Very important function for OnlinePayments
        /// </summary>
        /// <param name="BDInputStream"></param>
        /// <returns></returns>
        public string GetCustomerID(string BDInputStream)
        {
            string[] BillDeskResponse = BDInputStream.Split('|');
            string CustomerID = "";

            if (BillDeskResponse.IsNotNullOrEmpty() && BillDeskResponse.Length > 1)
            {
                CustomerID = new PaymentDB().GetoAuthTokenValue(BillDeskResponse[1]);
                if (!string.IsNullOrWhiteSpace(CustomerID))
                {
                    new PaymentDB().DeleteoAuthTokenValue(BillDeskResponse[1]);
                }
                return CustomerID;
            }
            else
                return null;
        }
    }
}