using System.Web.Mvc;

namespace CUSrinagar.WebApp.Areas.CUSrinagarAdminPanel
{
    public class CUSrinagarAdminPanelAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "CUSrinagarAdminPanel";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "CUSrinagarAdminPanel_default",
                "CUSrinagarAdminPanel/{controller}/{action}/{id}/{id1}/{id2}",
                new { controller = "Dashboard", action = "Index", id = UrlParameter.Optional, id1 = UrlParameter.Optional, id2 = UrlParameter.Optional },
                namespaces: new[] { "CUSrinagar.WebApp.CUSrinagarAdminPanel.Controllers" }
            );
        }
    }
}