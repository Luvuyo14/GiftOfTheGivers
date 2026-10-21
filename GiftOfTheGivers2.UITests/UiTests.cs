using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using Xunit;
using Xunit.Abstractions;

namespace GiftOfTheGivers2.UITests
{
    // Shared browser setup, helpers and screenshot capture
    public abstract class UiTestBase : IDisposable
    {
        private IWebDriver? _driver;
        protected readonly ITestOutputHelper Output;

        protected UiTestBase(ITestOutputHelper output) => Output = output;

        protected IWebDriver Driver =>
            _driver ?? throw new InvalidOperationException("Call Start() first.");

        // Set the APP_URL environment variable to test a different address (for example the live site)
        protected static string BaseUrl =>
            (Environment.GetEnvironmentVariable("APP_URL") ?? "https://localhost:7124").TrimEnd('/');

        protected void Start(string browser, int width = 1366, int height = 768)
        {
            var headless = Environment.GetEnvironmentVariable("HEADLESS") == "1";

            if (browser == "edge")
            {
                var options = new EdgeOptions { AcceptInsecureCertificates = true };
                if (headless) options.AddArgument("--headless=new");
                _driver = new EdgeDriver(options);
            }
            else
            {
                var options = new ChromeOptions { AcceptInsecureCertificates = true };
                if (headless) options.AddArgument("--headless=new");
                _driver = new ChromeDriver(options);
            }

            _driver.Manage().Window.Size = new System.Drawing.Size(width, height);
            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);
            Output.WriteLine($"Browser: {browser}, window {width}x{height}, site: {BaseUrl}");
        }

        protected void Open(string path)
        {
            Driver.Navigate().GoToUrl(BaseUrl + path);
            Output.WriteLine("Opened: " + Driver.Url);
        }

        protected void Type(string id, string text)
        {
            var element = Driver.FindElement(By.Id(id));
            element.Clear();
            element.SendKeys(text);
        }

        protected void Pick(string selectId, string optionValue) =>
            Driver.FindElement(By.CssSelector($"#{selectId} option[value='{optionValue}']")).Click();

        protected void Submit() =>
            Driver.FindElement(By.CssSelector("form button[type='submit']")).Click();

        protected bool WaitFor(Func<bool> condition, int seconds = 10)
        {
            var end = DateTime.UtcNow.AddSeconds(seconds);
            while (DateTime.UtcNow < end)
            {
                try { if (condition()) return true; } catch (WebDriverException) { }
                Thread.Sleep(250);
            }
            return false;
        }

        protected void Shot(string name)
        {
            var folder = Path.Combine(AppContext.BaseDirectory, "screenshots");
            Directory.CreateDirectory(folder);
            var path = Path.Combine(folder, name + ".png");
            File.WriteAllBytes(path, ((ITakesScreenshot)Driver).GetScreenshot().AsByteArray);
            Output.WriteLine("Screenshot saved: " + path);
        }

        public void Dispose()
        {
            _driver?.Quit();
            _driver?.Dispose();
        }
    }

    // ---------- Login and access control ----------

    [Trait("Category", "UI")]
    public class LoginUiTests : UiTestBase
    {
        public LoginUiTests(ITestOutputHelper output) : base(output) { }

        [Theory]
        [InlineData("chrome", 1366, 768)]
        [InlineData("edge", 1366, 768)]
        [InlineData("chrome", 500, 900)]
        public void Employee_CanLogin_AndSeesDashboard(string browser, int width, int height)
        {
            Start(browser, width, height);
            Open("/Account/Login");
            Type("Username", "employee");
            Type("Password", "1234");
            Submit();

            Assert.True(WaitFor(() => Driver.Url.Contains("/Employee")),
                "Expected a redirect to /Employee but the URL was " + Driver.Url);
            Assert.Contains("Employee Dashboard", Driver.PageSource);
            Shot($"employee-login-{browser}-{width}x{height}");
        }

        [Fact]
        public void Donor_CanLogin_AndLandsOnHomePage()
        {
            Start("chrome");
            Open("/Account/Login");
            Type("Username", "donor");
            Type("Password", "1234");
            Submit();

            Assert.True(WaitFor(() => !Driver.Url.Contains("/Account/Login")),
                "Still on the login page: " + Driver.Url);
            Assert.Equal("/", new Uri(Driver.Url).AbsolutePath);
            Shot("donor-login-home");
        }

        [Fact]
        public void Login_WrongPassword_ShowsErrorAndStaysOnLogin()
        {
            Start("chrome");
            Open("/Account/Login");
            Type("Username", "donor");
            Type("Password", "wrong-password");
            Submit();

            Assert.True(WaitFor(() => Driver.PageSource.Contains("Invalid username or password")));
            Assert.Contains("/Account/Login", Driver.Url);
            Shot("login-wrong-password");
        }

        [Fact]
        public void EmployeePage_WithoutLogin_RedirectsToLogin()
        {
            Start("chrome");
            Open("/Employee");

            Assert.True(WaitFor(() => Driver.Url.Contains("/Account/Login")),
                "Expected a redirect to the login page but the URL was " + Driver.Url);
            Shot("employee-page-blocked");
        }
    }

    // ---------- Donations ----------

    [Trait("Category", "UI")]
    public class DonationUiTests : UiTestBase
    {
        public DonationUiTests(ITestOutputHelper output) : base(output) { }

        [Theory]
        [InlineData("chrome", 1366, 768)]
        [InlineData("edge", 1366, 768)]
        [InlineData("chrome", 500, 900)]
        public void Donor_CanMakeAnonymousRecurringDonation_AndGetCertificate(string browser, int width, int height)
        {
            Start(browser, width, height);
            Open("/Donation");

            Type("DonorName", "Anonymous");
            Type("Email", "anonymous@example.com");
            Type("Amount", "150");
            Pick("Currency", "USD");
            Pick("DonationType", "Recurring");
            Shot($"donation-form-filled-{browser}-{width}x{height}");
            Submit();

            Assert.True(WaitFor(() => Driver.PageSource.Contains("Section 18A")),
                "The tax certificate page did not appear. URL: " + Driver.Url);
            Assert.Contains("GOTG-TAX-ANO-", Driver.PageSource);
            Assert.Contains("USD", Driver.PageSource);
            Assert.Contains("Recurring", Driver.PageSource);
            Shot($"donation-certificate-{browser}-{width}x{height}");
        }

        [Fact]
        public void Donation_EmptyNameAndNegativeAmount_IsRejected()
        {
            Start("chrome");
            Open("/Donation");

            Type("Amount", "-5");
            Submit();
            Thread.Sleep(1500);

            Assert.Empty(Driver.FindElements(By.Id("certificate")));
            Assert.Contains("/Donation", Driver.Url);
            Assert.Contains("field-validation-error", Driver.PageSource);
            Shot("donation-rejected");
        }
    }

    // ---------- Volunteers ----------

    [Trait("Category", "UI")]
    public class VolunteerUiTests : UiTestBase
    {
        public VolunteerUiTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public void Volunteer_CanRegister_AndSeesConfirmation()
        {
            Start("chrome");
            Open("/Volunteer");

            Type("FullName", "Thabo Nkosi");
            Type("Email", "thabo@example.com");
            Type("Phone", "0821234567");
            Type("Skills", "First aid and logistics");
            Pick("Availability", "Weekends");
            Submit();

            Assert.True(WaitFor(() => Driver.PageSource.Contains("registration was successful")),
                "No confirmation page. URL: " + Driver.Url);
            Assert.Contains("Thabo Nkosi", Driver.PageSource);
            Shot("volunteer-confirmation");
        }

        [Fact]
        public void Volunteer_EmptyForm_IsRejected()
        {
            Start("chrome");
            Open("/Volunteer");

            Submit();
            Thread.Sleep(1500);

            Assert.DoesNotContain("registration was successful", Driver.PageSource);
            Assert.Contains("/Volunteer", Driver.Url);
            Assert.Contains("field-validation-error", Driver.PageSource);
            Shot("volunteer-rejected");
        }
    }
}