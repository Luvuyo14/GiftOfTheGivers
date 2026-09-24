using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtownsoft.Json;

namespace GiftOfTheGivers2.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _functionBaseUrl;

        public EmployeeController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _functionBaseUrl = configuration["AzureSettings:FunctionBaseUrl"];
        }

        public IActionResult Index()
        {
            string role = HttpContext.Session.GetString("Role");

            if (role != "Employee")
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> PostUpdate(string projectName, string update)
        {
            string role = HttpContext.Session.GetString("Role");


            if(role !="Employee")
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

            // Send POST request to your Azure Function endpoint
            string targetUrl = $"{_functionBaseUrl}employee/update";
            var response = await _httpClient.PostAsync(targetUrl, jsonContent);

            if (response.IsSuccessStatusCode)
            {
                ViewBag.Message = "Project update posted successfully and logged to Azure Storage!";
            }
            else
            {
                ViewBag.Message = "Project update posted, but failed to log to Azure Function.";
            }

            return View("Index");
        }
    }
}