using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GiftOfTheGivers2.Tests
{
    // Stand-in for the Azure Function: records each request and returns the status we choose
    public class StubFunctionHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _status;
        public List<(string Url, string Body)> Requests { get; } = new();

        public StubFunctionHandler(HttpStatusCode status) => _status = status;

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var body = request.Content == null ? "" : await request.Content.ReadAsStringAsync(cancellationToken);
            Requests.Add((request.RequestUri!.ToString(), body));
            return new HttpResponseMessage(_status);
        }
    }

    // Starts the real web app in memory and talks to it over HTTP, like a browser
    public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public IntegrationTests(WebApplicationFactory<Program> factory) => _factory = factory;

        private HttpClient CreateClient(StubFunctionHandler? stub = null)
        {
            var factory = _factory.WithWebHostBuilder(builder =>
            {
                builder.UseSetting("AzureSettings:FunctionBaseUrl", "http://fake-function/api/");
                builder.ConfigureTestServices(services =>
                {
                    if (stub != null)
                    {
                        services.ConfigureHttpClientDefaults(b =>
                            b.ConfigurePrimaryHttpMessageHandler(() => stub));
                    }
                });
            });

            return factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                HandleCookies = true
            });
        }

        private static Task<HttpResponseMessage> Login(HttpClient client, string user, string password) =>
            client.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["Username"] = user,
                ["Password"] = password
            }));

        private static Task<HttpResponseMessage> Donate(HttpClient client, string name, string amount,
            string currency = "ZAR", string type = "One-Time") =>
            client.PostAsync("/Donation", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["DonorName"] = name,
                ["Amount"] = amount,
                ["Currency"] = currency,
                ["DonationType"] = type,
                ["Email"] = "donor@example.com"
            }));

        // ---------- Donor flow ----------

        [Fact]
        public async Task Donation_ValidForm_ShowsTaxCertificate()
        {
            var client = CreateClient();

            var form = await client.GetAsync("/Donation");
            Assert.Equal(HttpStatusCode.OK, form.StatusCode);

            var response = await Donate(client, "Naledi", "250");
            var html = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("GOTG-TAX-NAL-", html);
            Assert.Contains("Naledi", html);
            Assert.Contains("Section 18A", html);
        }

        [Theory]
        [InlineData("ZAR", "One-Time")]
        [InlineData("USD", "Monthly")]
        [InlineData("EUR", "Monthly")]
        public async Task Donation_DifferentCurrenciesAndTypes_AppearOnCertificate(string currency, string type)
        {
            var client = CreateClient();

            var response = await Donate(client, "Thabo", "100", currency, type);
            var html = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains(currency, html);
            Assert.Contains(type, html);
            Assert.Contains("GOTG-TAX-THA-", html);
        }

        [Fact]
        public async Task Donation_ZeroAmount_IsRejectedAndNoCertificateIssued()
        {
            var client = CreateClient();

            var response = await Donate(client, "Naledi", "0");
            var html = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.DoesNotContain("GOTG-TAX-", html);
        }

        [Fact]
        public async Task Donation_MissingDonorName_IsRejectedAndNoCertificateIssued()
        {
            var client = CreateClient();

            var response = await Donate(client, "", "100");
            var html = await response.Content.ReadAsStringAsync();

            Assert.DoesNotContain("GOTG-TAX-", html);
        }

        // ---------- Login ----------

        [Fact]
        public async Task Login_WrongPassword_ShowsErrorMessage()
        {
            var client = CreateClient();

            var response = await Login(client, "donor", "wrong");
            var html = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("Invalid username or password", html);
        }

        // ---------- Employee flow (Azure Function is stubbed) ----------

        [Fact]
        public async Task Employee_LoginThenPostUpdate_SendsUpdateToFunction()
        {
            var stub = new StubFunctionHandler(HttpStatusCode.OK);
            var client = CreateClient(stub);

            var login = await Login(client, "employee", "1234");
            Assert.Equal(HttpStatusCode.Redirect, login.StatusCode);
            Assert.Contains("Employee", login.Headers.Location!.OriginalString);

            var dashboard = await client.GetAsync("/Employee");
            Assert.Equal(HttpStatusCode.OK, dashboard.StatusCode);

            var response = await client.PostAsync("/Employee/PostUpdate", new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    ["projectName"] = "Flood Relief",
                    ["update"] = "Delivered 200 blankets"
                }));
            var html = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("successfully", html);
            Assert.Single(stub.Requests);
            Assert.EndsWith("employee/update", stub.Requests[0].Url);
            Assert.Contains("Delivered 200 blankets", stub.Requests[0].Body);
        }

        [Fact]
        public async Task Employee_FunctionReturnsError_ShowsFailureMessage()
        {
            var stub = new StubFunctionHandler(HttpStatusCode.InternalServerError);
            var client = CreateClient(stub);
            await Login(client, "employee", "1234");

            var response = await client.PostAsync("/Employee/PostUpdate", new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    ["projectName"] = "Flood Relief",
                    ["update"] = "Update text"
                }));
            var html = await response.Content.ReadAsStringAsync();

            Assert.Contains("failed", html);
        }

        // ---------- Access control ----------

        [Fact]
        public async Task EmployeePage_WithoutLogin_RedirectsToLogin()
        {
            var client = CreateClient();

            var response = await client.GetAsync("/Employee");

            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Contains("/Account/Login", response.Headers.Location!.OriginalString);
        }

        [Fact]
        public async Task EmployeePage_LoggedInAsDonor_RedirectsToLogin()
        {
            var client = CreateClient();
            await Login(client, "donor", "1234");

            var response = await client.GetAsync("/Employee");

            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Contains("/Account/Login", response.Headers.Location!.OriginalString);
        }
    }
}