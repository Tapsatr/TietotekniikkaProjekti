using System.Collections.Generic;
using System.DirectoryServices;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using TietotekniikkaProjekti.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Identity;
using TietotekniikkaProjekti.Services;
using Microsoft.Extensions.Logging;
using System.Net.Mail;
using System;
using TietotekniikkaProjekti.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;

namespace TietotekniikkaProjekti.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {


        private readonly PassWordContext _context;
        private const int PASSWORD_RESET_CODE_ALIVE_TIME = 12;
        static private IConfiguration _Configuration;
         private AdHelper adHelper;
        public AccountController(PassWordContext context, IConfiguration Configuration)
        {
            _context = context;
            _Configuration = Configuration;
           
             adHelper = new AdHelper(_Configuration);
        }
        
      

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel loginModel)
        {

            if (!adHelper.Authenticate(loginModel.UserName, loginModel.Password))
                return View();



            string role = "User";
            if (adHelper.isAdmin(loginModel.UserName))
            {
                role = "Administrator";
            }
            else if (adHelper.IsHR(loginModel.UserName))
            {
                role = "HR";
            }
            // create claims
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, loginModel.UserName),
                new Claim(ClaimTypes.Role, role)

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
          new { controller = "Home", action = "Index", loginModel.UserName }));

        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                    scheme: "SecurityCookie");

            return RedirectToAction("Login");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPw()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ForgotPw(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {

                var ad = adHelper.GetAllUsers();

                var user = ad.Find(x => x.Email.ToLower() == model.Email.ToLower());

                if (user == null)
                {
                    return RedirectToAction(nameof(ForgotPasswordConfirmation));
                }
                else
                {
                    MailMessage mail = new MailMessage

                    {
                        Subject = "mokkamoi!!",

                        From = new MailAddress("vikke94@hotmail.com")

                    };
                    ApplicationUser userA = new ApplicationUser();

                    userA.UserName = user.Username;
                    Guid g = Guid.NewGuid();
                    string GuidString = Convert.ToBase64String(g.ToByteArray());
                    GuidString = GuidString.Replace("=", "");
                    GuidString = GuidString.Replace("+", "");
                    var callbackUrl = Url.ResetPasswordCallbackLink(user.Username, GuidString, Request.Scheme);

                    try
                    {
                        PasswordCode result = (from p in _context.PasswordCode
                                               where p.Username == user.Username
                                               select p).SingleOrDefault();
                        if (result == null)//jos käyttäjä ei ole ennen tehnyt salasananpalautusta tehdään uusi passwordcode
                        {
                            result = new PasswordCode
                            {
                                Username = user.Username
                            };
                        }
                        result.Code = GuidString;// tallennetaan käyttäjälle salasanan palautus tiedot
                        result.IsUsed = false;
                        result.TimeStamp = DateTime.Now;
                        _context.Update(result);

                        _context.SaveChanges();
                    }
                    catch (Exception) { }

                    mail.To.Add(user.Email);
                    mail.Body = $"Reset password link: <a href=\"{callbackUrl}\">{callbackUrl}</a>";
                    mail.IsBodyHtml = true;

                    adHelper.SendMail(mail, user.Email);
                }

                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }

            // If we got this far, something failed, redisplay form
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string code = null)
        {
            if (code == null)
            {
                throw new ApplicationException("A code must be supplied for password reset.");
            }
            var model = new ResetPasswordViewModel { Code = code };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // var user = await _userManager.FindByEmailAsync(model.Email);

            var ad = adHelper.GetAllUsers();

            var user = ad.Find(x => x.Email.ToLower() == model.Email.ToLower());

            if (user == null || !PasswordCodeValid(model.Code, user.Username))
            {
                // Don't reveal that the user does not exist
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            var result = adHelper.EditPassword(user.Username, model.Password);//???
            if (result.Equals("OK"))
            {
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }
            //AddErrors(result);
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }


        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
        private bool PasswordCodeValid(string code, string username)
        {
            TimeSpan aliveTime = TimeSpan.FromHours(PASSWORD_RESET_CODE_ALIVE_TIME);
            try
            {
                PasswordCode result = (from p in _context.PasswordCode
                                       where p.Username == username
                                       select p).SingleOrDefault();
                if (result != null)
                {
                    if (result.Code == code && result.Username == username && result.IsUsed == false && DateTime.Now - result.TimeStamp < aliveTime)// oikea palautus koodi, username, voimassaolokoodi ja aika aloitusaika - nyt aika < olemassaoloaika
                    {
                        //var time = DateTime.Now - result.TimeStamp;
                        result.IsUsed = true;//koodi käytetty
                        _context.SaveChanges();
                        return true;
                    }
                }
            }
            catch (Exception) { }
            return false;
        }
    }
}