using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TietotekniikkaProjekti.Models;
using System.DirectoryServices;

namespace TietotekniikkaProjekti.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public IActionResult AdminCreate()
        {
            return View();
        }
        public IActionResult Login()
        {
           
            return View();  
        }
        [HttpPost]
        public IActionResult Login(LoginModel login)
        {
            Authenticate(login.Login, login.Password, "domaini");
            return View();
        }
        public static void Rename(string server,string userName, string password, string objectDn, string newName)
        {
            DirectoryEntry child = new DirectoryEntry("LDAP://" + server + "/" +
                objectDn, userName, password);
            child.Rename("CN=" + newName);
        }
        private bool Authenticate(string userName, string password, string domain)
        {
            bool authentic = false;
            try
            {
                DirectoryEntry entry = new DirectoryEntry("LDAP://" + domain,
                    userName, password);
                object nativeObject = entry.NativeObject;
                authentic = true;
            }
            catch (DirectoryServicesCOMException) { }
            return authentic;
        }
    }
}
