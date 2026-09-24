using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;
using GiftOfTheGivers2.Models;

namespace GiftOfTheGivers2.Pages
{
    public class TaxCertificateModelPage : PageModel
    {
        [BindProperty]
        public DonationModel DonationInput { get; set; } = new();

        public void OnGet()
        {
            // Initial page load
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(DonationInput.DonorName) || DonationInput.Amount <= 0)
            {
                ModelState.AddModelError("", "Please enter a valid donor name and amount.");
                return Page();
            }

            // Your running Azure Function local endpoint and it must be run first and the url is different from each device 
            string functionUrl = "http://localhost:7213/api/GenerateTaxCertificate";

            var payload = new
            {
                DonorName = DonationInput.DonorName,
                Amount = DonationInput.Amount,
                Currency = DonationInput.Currency ?? "ZAR",
                DonationType = DonationInput.DonationType ?? "One-Time",
                Email = DonationInput.Email
            };

            try
            {
                using var client = new HttpClient();
                var response = await client.PostAsJsonAsync(functionUrl, payload);

                if (response.IsSuccessStatusCode)
                {
                    // Read the response from the Azure Function which contains the certificate details
                    var result = await response.Content.ReadFromJsonAsync<CertificateResponseModel>();

                    if (result != null)
                    {
                        // Pass data to ViewData to match your TaxCertificate.cshtml view
                        ViewData["ReferenceNumber"] = result.CertificateNumber;
                        ViewData["Date"] = result.Date;
                    }
                }
                else
                {
                    ViewData["ReferenceNumber"] = "ERROR-GEN-FAILED";
                    ViewData["Date"] = DateTime.Now.ToString("yyyy-MM-dd");
                }
            }
            catch
            {
                ViewData["ReferenceNumber"] = "OFFLINE-MODE";
                ViewData["Date"] = DateTime.Now.ToString("yyyy-MM-dd");
            }

            // Return the model back to the view so @Model.DonorName, etc., display correctly
            return Page();
        }
    }

    // Helper model to capture the JSON response returned by your Azure Function
    public class CertificateResponseModel
    {
        public bool Success { get; set; }
        public string? CertificateNumber { get; set; }
        public string? DonorName { get; set; }
        public decimal Amount { get; set; }
        public string? Currency { get; set; }
        public string? DonationType { get; set; }
        public string? Date { get; set; }
        public string? Organisation { get; set; }
    }
}