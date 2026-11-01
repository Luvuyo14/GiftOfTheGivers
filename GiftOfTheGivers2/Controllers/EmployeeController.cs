using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GiftOfTheGivers2.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string? _functionBaseUrl;
        private readonly string? _functionKey;

        public EmployeeController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _functionBaseUrl = configuration["AzureSettings:FunctionBaseUrl"];
            _functionKey = configuration["AzureSettings:FunctionKey"];
        }

        public IActionResult Index()
        {
            string? role = HttpContext.Session.GetString("Role");

            if (role != "Employee")
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> PostUpdate(string projectName, string update)
        {
            string? role = HttpContext.Session.GetString("Role");

            if (role != "Employee")
            {
                return RedirectToAction("Login", "Account");
            }

            // Build the payload for the Azure Function
            var payload = new
            {
                EmployeeId = HttpContext.Session.GetString("EmployeeId") ?? "EMP-001",
                EmployeeName = HttpContext.Session.GetString("EmployeeName") ?? "Staff Member",
                Department = projectName,
                UpdateText = update
            };

            var jsonContent = new StringContent(
                JsonConvert.SerializeObject(payload),
                Encoding.UTF8,
                "application/json"
            );

            // Send POST request to the Azure Function endpoint
            string targetUrl = $"{_functionBaseUrl}employee/update";
            bool succeeded = false;

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, targetUrl) { Content = jsonContent };
                if (!string.IsNullOrEmpty(_functionKey))
                {
                    request.Headers.Add("x-functions-key", _functionKey);
                }

                var response = await _httpClient.SendAsync(request);
                succeeded = response.IsSuccessStatusCode;
            }
            catch (HttpRequestException)
            {
                succeeded = false;
            }

            if (succeeded)
            {
                ViewBag.Message = "Project update posted successfully and recorded by the Azure Function.";
            }
            else
            {
                ViewBag.Message = "Project update posted, but failed to log to Azure Function.";
            }

            return View("Index");
        }
    }
}