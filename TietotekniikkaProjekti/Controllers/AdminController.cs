using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TietotekniikkaProjekti.Models;

namespace TietotekniikkaProjekti.Controllers
{
    [Authorize(Roles="Administrator")]
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();  
        }
        public IActionResult AdminCreate()
        {
            return View();
        }
        [HttpPost]
        public IActionResult AdminCreate(UserModel user)
        {
            AdHelper adHelper = new AdHelper();
            // Rename("DC = ryhma1, DC = local", login.UserName, login.Password);
            //adHelper.CreateUserAccount("DC=ryhma1,DC=local", user);
            //adHelper.AddToGroup("CN=user, OU=USERS, DC=ryhma1, DC=local ", "CN=group,OU=GROUPS,DC=ryhma1,DC=local");

            var success = adHelper.CreateUser(user);
          

            ViewBag.data = success;
            return View();
        }
    }
}