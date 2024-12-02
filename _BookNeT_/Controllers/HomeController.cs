using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace _BookNeT_Project.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {            // אם המשתמש לא מחובר, הפנה אותו לדף התחברות
            if (Session["Email"] == null)
            {
                return RedirectToAction("Login", "Account"); // הפניה לדף התחברות
            }
            ViewBag.Message = "Your application description page.";

            return View();
        }
        public ActionResult Contact()
        {
            // אם המשתמש לא מחובר, הפנה אותו לדף התחברות
            if (Session["Email"] == null)
            {
                return RedirectToAction("Login", "Account"); // הפניה לדף התחברות
            }

            ViewBag.Message = "Your contact page.";
            return View();
        }
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}