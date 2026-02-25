using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TODOlistsystem.Models;

namespace TODOlistsystem.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            return View();
        }

        // По желание - стандартните методи
        public ActionResult About()
        {
            ViewBag.Message = "About page.";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Contact page.";
            return View();
        }
    }
}