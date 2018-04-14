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
        public IActionResult Index(string UserName)
        {
            var identity = (ClaimsIdentity)User.Identity;
            ViewBag.VikkeGay = TempData["vikegay"];
            if (identity.Name!=null)
            {
                UserName = identity.Name;
            }

            UserModel VikkeOnHomo = adHelper.GetUserDetails(UserName);
            return View(VikkeOnHomo);
        }
        public IActionResult Edit()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Edit(UserModel user)
        {
            user.Username = User.Identity.Name;
            TempData["vikegay"] = adHelper.EditUser(user);
           
            return Redirect("Index");
        }
    }
}
