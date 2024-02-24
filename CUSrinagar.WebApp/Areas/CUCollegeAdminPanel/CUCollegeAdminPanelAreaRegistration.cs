using System.Web.Mvc;

namespace CUSrinagar.WebApp.Areas.CUCollegeAdminPanel
{
    public class CUCollegeAdminPanelAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "CUCollegeAdminPanel";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "CUCollegeAdminPanel_default",
                "CUCollegeAdminPanel/{controller}/{action}/{id}/{id1}/{id2}",
                new {controller= "Dashboard", action = "Index", id = UrlParameter.Optional, id1 = UrlParameter.Optional, id2 = UrlParameter.Optional },
                namespaces: new[] { "CUSrinagar.WebApp.CUCollegeAdminPanel.Controllers" }
            );
        }
    }
}