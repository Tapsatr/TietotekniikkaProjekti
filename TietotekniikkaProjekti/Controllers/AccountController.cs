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

namespace TietotekniikkaProjekti.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        AdHelper adHelper = new AdHelper();

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
          new { controller = "Home", action = "Index",  loginModel.UserName  }));

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
                    // var code = await _userManager.GeneratePasswordResetTokenAsync(userA);
                    Guid g = Guid.NewGuid();
                    string GuidString = Convert.ToBase64String(g.ToByteArray());
                    GuidString = GuidString.Replace("=", "");
                    GuidString = GuidString.Replace("+", "");
                    var callbackUrl = Url.ResetPasswordCallbackLink(user.Username, GuidString, Request.Scheme);

                    mail.To.Add(user.Email);
                    mail.Body = "Reset password link: " + callbackUrl;
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

            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            var result = adHelper.EditPassword(user.Username, user.Password, model.Password);
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
    }
}