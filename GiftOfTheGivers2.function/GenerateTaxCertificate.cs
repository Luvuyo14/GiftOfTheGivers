using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text.Json;

namespace GiftOfTheGivers2.function;

public class GenerateTaxCertificate
{
    private readonly ILogger<GenerateTaxCertificate> _logger;

    public GenerateTaxCertificate(ILogger<GenerateTaxCertificate> logger)
    {
        _logger = logger;
    }

    [Function("GenerateTaxCertificate")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request to generate a tax certificate.");

        string donorName = req.Query["Name"];
        decimal amount = 0;
        string currency = "ZAR";
        string donationType = "One-Time";

       
        if (req.Method == "POST")
        {
            try
            {
                using var reader = new StreamReader(req.Body);
                var bodyJson = await reader.ReadToEndAsync();

                using var doc = JsonDocument.Parse(bodyJson);
                var root = doc.RootElement;

                if (root.TryGetProperty("DonorName", out var nameProp))
                    donorName = nameProp.GetString();

                if (root.TryGetProperty("Amount", out var amountProp))
                    amount = amountProp.GetDecimal();

                if (root.TryGetProperty("Currency", out var currProp))
                    currency = currProp.GetString() ?? "ZAR";

                if (root.TryGetProperty("DonationType", out var typeProp))
                    donationType = typeProp.GetString() ?? "One-Time";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error parsing request body: {ex.Message}");
            }
        }

        // Generate a unique certificate number and response object matching your frontend expectations
        var certificateResponse = new
        {
            Success = true,
            CertificateNumber = $"GOTG-TAX-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
            DonorName = string.IsNullOrEmpty(donorName) ? "Valued Donor" : donorName,
            Amount = amount > 0 ? amount : 1000.00m,
            Currency = currency,
            DonationType = donationType,
            Date = DateTime.Now.ToString("yyyy-MM-dd"),
            Organisation = Environment.GetEnvironmentVariable("ORGANISATION_NAME") ?? "Gift of the Givers Foundation"

        };

        return new OkObjectResult(certificateResponse);
    }
}