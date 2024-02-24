using System;
using System.Web;
using CUSrinagar.Enums;
using System.Web.Mvc;

namespace CUSrinagar.Models
{
    public class AppUserHelper
    {
        private static AppUserCompact GetAppUserCompact()
        {
            try
            {
                return ((IPrincipalProvider)HttpContext.Current.User).AppUser;
            }
            catch (InvalidCastException)
            {
                UrlHelper urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
                HttpContext.Current.Response.Redirect(urlHelper.Action("Forbidden", "Account", new { area = string.Empty }), true);
                return null;
            }
            catch (NullReferenceException)
            {
                UrlHelper urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
                HttpContext.Current.Response.Redirect(urlHelper.Action("Forbidden", "Account", new { area = string.Empty }), true);
                return null;
            }
        }

        public static Guid User_ID
        {
            get
            {
                AppUserCompact appUsers = GetAppUserCompact();
                if (appUsers != null)
                    return appUsers.User_ID;
                return Guid.Empty;
            }
        }

        public static string CollegeCode
        {
            get
            {
                AppUserCompact appUsers = GetAppUserCompact();
                if (appUsers != null)
                    return appUsers.CollegeCode ?? string.Empty;
                return string.Empty;
            }
        }
        public static Guid? College_ID
        {
            get
            {
                AppUserCompact appUsers = GetAppUserCompact();
                if (appUsers != null)
                    return appUsers.College_ID;
                return null;
            }
        }
        public static AppUserCompact AppUsercompact
        {
            get
            {
                return GetAppUserCompact();
            }
        }
        public static string Designation
        {
            get
            {
                AppUserCompact appUsers = GetAppUserCompact();
                if (appUsers != null)
                    return appUsers.Designation;
                return null;
            }
        }
        public static string EmailId
        {
            get
            {
                AppUserCompact appUsers = GetAppUserCompact();
                if (appUsers != null)
                    return appUsers.EmailId;
                return null;
            }
        }

        public static PrintProgramme TableSuffix
        {
            get
            {
                AppUserCompact appUsers = GetAppUserCompact();
                if (appUsers != null)
                    return  appUsers.TableSuffix;
                return PrintProgramme.BED;
            }
        }
        public static PrintProgramme OrgPrintProgramme
        {
            get
            {
                AppUserCompact appUsers = GetAppUserCompact();
                if (appUsers != null)
                    return appUsers.OrgPrintProgramme;
                return PrintProgramme.BED;
            }
        }
    }
}
