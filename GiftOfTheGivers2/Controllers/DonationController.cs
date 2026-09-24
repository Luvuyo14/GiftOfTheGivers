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
                string donorName = "Paballo Mentoor";
                int donationId = 1045;

                // Call custom NuGet package utility method
                string taxCertificateNumber = DonationHelper.FormatTaxCertificateNumber(donorName, donationId);
                // Result: "GOTG-TAX-PAB-001045"


                // Generate a dummy receipt/reference number
                ViewBag.ReferenceNumber = "GOTG-TAX-" + System.Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
                ViewBag.Date = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm");

                // Route to the Tax Certificate View

                return View("TaxCertificate", model);

            }


            
            return View(model);
        }

    }
}