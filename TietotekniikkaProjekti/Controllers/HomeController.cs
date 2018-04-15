using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TietotekniikkaProjekti.Models;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using TietotekniikkaProjekti.Extensions;
using System.Linq;
using System.Security.Claims;

namespace TietotekniikkaProjekti.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        AdHelper adHelper = new AdHelper();
        public IActionResult Index()
        {
            string UserName = string.Empty;
            var identity = (ClaimsIdentity)User.Identity;
            ViewBag.data = TempData["data"];
            if (identity.Name!=null)
            {
                UserName = identity.Name;
            }

            UserModel user = adHelper.GetUserDetails(UserName);
            return View(user);
        }
        [HttpGet]
        public IActionResult Edit()
        {
            string user = User.Identity.Name;
            return View(adHelper.GetUserDetails(user));
        }
        [HttpPost]
        public IActionResult Edit(UserModel user)
        {
            user.Username = User.Identity.Name;
            TempData["data"] = adHelper.EditUser(user);
           
            return Redirect("Index");
        }

        [HttpGet]
        public IActionResult EditPassword()
        {

            return View();
        }

        [HttpPost]
        public IActionResult EditPassword(string oldpassword, string newpassword)
        {
            string username = User.Identity.Name;
            string code = "";
            code = adHelper.EditPassword(username, oldpassword, newpassword);
            if(code == "OK")
            {
                TempData["data"] = "Succesfully changed!";
                return Redirect("Index");
            }
            else
            {
                TempData["data"] = "Wrong password!";
                return View();
            }
                
            
        }
    }
}
