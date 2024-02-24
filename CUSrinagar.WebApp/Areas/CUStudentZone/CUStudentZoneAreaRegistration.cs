using System.Web.Mvc;

namespace CUSrinagar.WebApp.Areas.CUStudentZone
{
    public class CUStudentZoneAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "CUStudentZone";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "CUStudentZone_default",
                "CUStudentZone/{controller}/{action}/{id}/{id1}/{id2}",
               new { controller = "Dashboard", action = "Index", id = UrlParameter.Optional, id1 = UrlParameter.Optional, id2 = UrlParameter.Optional },
               namespaces: new[] { "CUSrinagar.WebApp.CUStudentZone.Controllers" }
            );
        }
    }
}