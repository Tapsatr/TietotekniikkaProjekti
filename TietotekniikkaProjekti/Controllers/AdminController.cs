using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TietotekniikkaProjekti.Extensions;
using TietotekniikkaProjekti.Models;

namespace TietotekniikkaProjekti.Controllers
{
    [Authorize(Roles="Administrator")]
    public class AdminController : Controller
    {
        AdHelper adHelper = new AdHelper();
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
            
            // Rename("DC = ryhma1, DC = local", login.UserName, login.Password);
            //adHelper.CreateUserAccount("DC=ryhma1,DC=local", user);
            //adHelper.AddToGroup("CN=user, OU=USERS, DC=ryhma1, DC=local ", "CN=group,OU=GROUPS,DC=ryhma1,DC=local");

            
          
            TempData["Success"] = adHelper.CreateUser(user);
            return Redirect("UsersList");
        }
        public IActionResult UsersList()
        {
            ViewBag.Success = TempData["Success"];

            List<UserModel> usersList = adHelper.GetAllUsers();// haetaan lista käyttäjistä
            HttpContext.Session.Set("usersListSession", usersList);//tallennetaan lista sessioon
            return View(usersList);
        }

        [HttpGet]
        public IActionResult EditUser(string username)
        {
            List<UserModel> usersList = HttpContext.Session.Get<List<UserModel>>("usersListSession") as List<UserModel>;//otetaan lista sessiosta
            //etsitään käyttäjä listasta ja palautetaan edit viewiin
            UserModel userModel = new UserModel();
            userModel = usersList.Find(s => s.Username == username);
            return View(userModel);
        }
        [HttpPost]
        public IActionResult EditUser(UserModel user)
        {
            //User.IsInRole("Administrator");
            TempData["Success"] = adHelper.EditUser(user);
            return Redirect("UsersList");
        }
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}