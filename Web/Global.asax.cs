using System;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using avSVAW.App_Start;
using avSVAW.Controllers;
using System.Web.Http;
using System.Data.SqlClient;

namespace avSVAW
{
    public class MvcApplication : HttpApplication
    {
        string conStr = System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString;

        protected void Application_Start()
        {
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            //SqlDependency.Start(conStr);

        }
        protected void Session_Start(object sender, EventArgs e)
        {
            //Monitoring NC = new Monitoring();
            //var currentTime = DateTime.Now;
            ////HttpContext.Current.Session["LastUpdated"] = currentTime;
            //NC.RegisterMonitoring(currentTime);
        }
        protected void Application_End()
        {
            //here we will stop Sql Dependency
            //SqlDependency.Stop(conStr);
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var exception = Server.GetLastError();
            var httpContext = ((HttpApplication)sender).Context;
            httpContext.Response.Clear();
            httpContext.ClearError();

            if (new HttpRequestWrapper(httpContext.Request).IsAjaxRequest())
            {
                return;
            }

            ExecuteErrorController(httpContext, exception as HttpException);
        }

        private void ExecuteErrorController(HttpContext httpContext, HttpException exception)
        {
            var routeData = new RouteData();
            routeData.Values["controller"] = "Error";

            if (exception != null && exception.GetHttpCode() == (int)HttpStatusCode.NotFound)
            {
                routeData.Values["action"] = "NotFound";
            }
            else
            {
                routeData.Values["action"] = "InternalServerError";
            }

            using (Controller controller = new ErrorController())
            {
               ((IController)controller).Execute(new RequestContext(new HttpContextWrapper(httpContext), routeData));
            }
        }
    }
}