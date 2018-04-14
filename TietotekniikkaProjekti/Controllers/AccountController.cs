using System.Collections.Generic;
using System.DirectoryServices;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using TietotekniikkaProjekti.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;

namespace TietotekniikkaProjekti.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }
        
        [HttpPost]
        public async Task<IActionResult> Login(LoginModel loginModel)
        {
            AdHelper adHelper = new AdHelper();
            if (!adHelper.Authenticate(loginModel.UserName, loginModel.Password))
                return View();

            

          

            // create claims
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, loginModel.UserName),
                /*new Claim(ClaimTypes.Role, "Administrator"),*/
                new Claim(ClaimTypes.Surname, loginModel.Password)

            };

            // create identity
            ClaimsIdentity identity = new ClaimsIdentity(claims, "cookie");

            // create principal
            ClaimsPrincipal principal = new ClaimsPrincipal(identity);

            // sign-in
            await HttpContext.SignInAsync(
                    scheme: "SecurityCookie",
                    principal: principal);

            return RedirectToAction("Index", new RouteValueDictionary(
          new { controller = "Home", action = "Index",  loginModel.UserName  }));

        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                    scheme: "SecurityCookie");

            return RedirectToAction("Login");
        }
    }
}