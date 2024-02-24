using CUSrinagar.BusinessManagers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http.Filters;

namespace CUSrinagar.API
{
    public class OopsExceptionHandler : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            #if (!DEBUG)
                if (actionExecutedContext.Exception != null)
                    new EmailSystem().LogErrorMail(actionExecutedContext.Exception);
                var response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent($@"Oops! Sorry! Something went wrong. {Environment.NewLine}An unhandled exception was thrown by service. {Environment.NewLine}Detailed report has been send to CUS Developers and will be fixed soon.")
                };
                actionExecutedContext.Response = response;
            #endif
        }
    }
}