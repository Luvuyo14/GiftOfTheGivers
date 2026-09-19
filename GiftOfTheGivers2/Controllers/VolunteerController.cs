using GiftOfTheGivers2.Models;
using Microsoft.AspNetCore.Mvc;

namespace GiftOfTheGivers.Controllers
{
    public class VolunteerController : Controller
    {
        // Display volunteer registration form
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // Process volunteer registration
        [HttpPost]
        public IActionResult Index(VolunteerModel volunteer)
        {
            if (ModelState.IsValid)
            {
                volunteer.RegistrationDate = DateTime.Now;

                return View("Confirmation", volunteer);
            }

            return View(volunteer);
        }
    }
}