using Microsoft.AspNetCore.Mvc;
using GiftOfTheGivers2.Models;
using GiftOfTheGivers.Helpers;

namespace GiftOfTheGivers2.Controllers
{
    public class DonationController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(DonationModel model)
        {
            if (ModelState.IsValid)
            {
                // Prototype donation id (no database yet)
                int donationId = Random.Shared.Next(1, 999999);

                // Call custom NuGet package utility method using the donor from the form
                string taxCertificateNumber =
                    DonationHelper.FormatTaxCertificateNumber(model.DonorName, donationId);

                model.TaxCertificate = taxCertificateNumber;
                ViewBag.ReferenceNumber = taxCertificateNumber;
                ViewBag.Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm");

                // Route to the Tax Certificate View
                return View("TaxCertificate", model);
            }

            return View(model);
        }
    }
}