using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Bytescout.Spreadsheet;
using TietotekniikkaProjekti.Models;
using TietotekniikkaProjekti.Extensions;

namespace TietotekniikkaProjekti.Controllers
{
    public class HRController : Controller
    {
        AdHelper adHelper = new AdHelper();
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult AddMultipleUsers()
        {

            return View();
        }
        [HttpPost]
        public IActionResult AddMultipleUsers(Microsoft.AspNetCore.Http.IFormFile files)
        {
            Spreadsheet document = new Spreadsheet();
            document.LoadFromFile(files.FileName);
            Worksheet worksheet = document.Workbook.Worksheets.ByName("Taul1");
            int STARTING_ROW = 2;

            List<UserModel> usersToBeAddedList = new List<UserModel>();
            Cell username, password, nimi, sukunimi, osoite, email, employeetype, enabled;
            while (true)
            {
                // Set current cell
                username = worksheet.Cell(STARTING_ROW, 0);
                password = worksheet.Cell(STARTING_ROW, 1);
                nimi = worksheet.Cell(STARTING_ROW, 2);
                sukunimi = worksheet.Cell(STARTING_ROW, 3);
                osoite = worksheet.Cell(STARTING_ROW, 4);
                email = worksheet.Cell(STARTING_ROW, 5);
                employeetype = worksheet.Cell(STARTING_ROW, 6);
                enabled = worksheet.Cell(STARTING_ROW, 7);
                if (username.ValueAsString.Equals(""))
                    break;
                UserModel userModel = new UserModel
                {
                    Username = username.ValueAsString,
                    Nimi = nimi.ValueAsString,
                    Sukunimi = sukunimi.ValueAsString,
                    Password = password.ValueAsString,
                    Osoite = osoite.ValueAsString,
                    Email = email.ValueAsString,
                    EmployeeType = employeetype.ValueAsString,
                    Enabled = enabled.ValueAsBoolean
                };
                usersToBeAddedList.Add(userModel);
                STARTING_ROW += 1;//next line
            }

            // Close document
            document.Close();

            HttpContext.Session.Set("NewUsersList", usersToBeAddedList);

            return View(usersToBeAddedList);
        }
        
        public IActionResult AddUsers()
        {
            List<string> successList = new List<string>();
            List<UserModel> NewUsersList = HttpContext.Session.Get<List<UserModel>>("NewUsersList") as List<UserModel>;
           if(NewUsersList == null)
            {
                ViewData["info"] = "Failed to add!";
            }
           foreach(var user in NewUsersList)
            {
                string tmp = "";
                tmp = adHelper.CreateUser(user);//Ei toimi koska käyttäjät ovat jo olemassa
                successList.Add($"Username: {user.Username} Email: {user.Email} Password: {user.Password} ----  {tmp}");
            }
            ViewData["info"] = successList;
            return View("AddMultipleUsers");
        }
    }
}