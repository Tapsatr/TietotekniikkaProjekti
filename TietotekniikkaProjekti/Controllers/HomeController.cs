using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TietotekniikkaProjekti.Models;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using TietotekniikkaProjekti.Extensions;
using System.Linq;

namespace TietotekniikkaProjekti.Controllers
{
    [Authorize]//ota pois jos et halua kirjautua sisään kokoajan
    public class HomeController : Controller
    {
        AdHelper adHelper = new AdHelper();
        public IActionResult Index(string UserName)
        {
            AdHelper adHelper = new AdHelper();
            string data = adHelper.GetUserDetails(UserName);

            // var data = adHelper.AttributeValuesSingleString("streetAddress", $" CN=Tapio Riihimäki, CN=Users, DC=ryhma1, DC=local ");
            //CN = group, OU = GROUPS, DC = contoso, DC = com
            var data2 = adHelper.GetGroup(UserName);
            string gay = "";
            foreach (var val in data2)
            {
                gay += val;
            }
            ViewBag.VikkeOnHomo = gay;


            if (Request.Cookies["cookie"] != null)
            {
                var value = Request.Cookies["cookie"].ToString();

            }

            return View();
        }

        public IActionResult UsersList()
        {
            List<UserModel> usersList= adHelper.GetAllUsers();// haetaan lista käyttäjistä
            HttpContext.Session.Set("usersListSession", usersList);//tallennetaan lista sessioon
            return View(usersList);
        }

        public IActionResult AddUser()
        {

            return View();
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
            adHelper.EditUser(user);
            return View("UsersList");
        }
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}
