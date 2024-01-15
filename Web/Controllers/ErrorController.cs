using avSVAW.App_Start;
using System;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace avSVAW.Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult InternalServerError()
        {
            return View();
        }

        public ActionResult NotFound()
        {
            return View();
        }
        public ActionResult Index(string exception)
        {
            ViewBag.error = exception;
            return View();
        }
    }
}
