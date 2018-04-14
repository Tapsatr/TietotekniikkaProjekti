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

    }
}
