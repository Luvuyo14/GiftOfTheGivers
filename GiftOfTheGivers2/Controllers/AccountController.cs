using GiftOfTheGivers2.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GiftOfTheGivers2.Controllers
{
    public class AccountController : Controller
    {
        // Login page
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // Process login
        [HttpPost]
        public IActionResult Login(LoginModel model)
        {
            if (model.Username == "donor" &&
                model.Password == "1234")
            {
                HttpContext.Session.SetString("Username", "donor");
                HttpContext.Session.SetString("Role", "Donor");

                return RedirectToAction("Index", "Home");
            }

            if (model.Username == "employee" &&
                model.Password == "1234")
            {
                HttpContext.Session.SetString("Username", "employee");
                HttpContext.Session.SetString("Role", "Employee");

                return RedirectToAction("Index", "Employee");
            }

            ModelState.AddModelError(
                "",
                "Invalid username or password."
            );

            return View(model);
        }

        // Donor registration page
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // Process donor registration
        [HttpPost]
        public IActionResult Register(RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Prototype registration
            TempData["RegistrationMessage"] =
                "Your donor account has been successfully registered.";

            return RedirectToAction("RegistrationSuccess");
        }

        // Registration success page
        public IActionResult RegistrationSuccess()
        {
            return View();
        }

        // Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Login");
        }
    }
}