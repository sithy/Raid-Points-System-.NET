using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MvcSample.Controllers
{
    [HandleError]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            Session["language"] = Request.QueryString["language"];
            ViewData["Message"] = "Welcome to ASP.NET MVC!";
            return View();
        }
    }
}
