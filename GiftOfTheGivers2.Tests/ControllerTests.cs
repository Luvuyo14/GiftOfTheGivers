using GiftOfTheGivers2.Controllers;
using GiftOfTheGivers2.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace GiftOfTheGivers2.Tests
{
    // A fake session so we can test code that uses HttpContext.Session
    public class TestSession : ISession
    {
        private readonly Dictionary<string, byte[]> _store = new();

        public bool IsAvailable => true;
        public string Id { get; } = Guid.NewGuid().ToString();
        public IEnumerable<string> Keys => _store.Keys;

        public void Clear() => _store.Clear();
        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Remove(string key) => _store.Remove(key);
        public void Set(string key, byte[] value) => _store[key] = value;

        public bool TryGetValue(string key, out byte[]? value)
        {
            var found = _store.TryGetValue(key, out var v);
            value = v;
            return found;
        }
    }

    public class AccountControllerTests
    {
        private static AccountController CreateController(out TestSession session)
        {
            session = new TestSession();
            var httpContext = new DefaultHttpContext { Session = session };
            return new AccountController
            {
                ControllerContext = new ControllerContext { HttpContext = httpContext }
            };
        }

        [Fact]
        public void Login_ValidDonor_SetsSessionAndRedirectsToHome()
        {
            var controller = CreateController(out var session);

            var result = controller.Login(new LoginModel { Username = "donor", Password = "1234" });

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Home", redirect.ControllerName);
            Assert.Equal("Donor", session.GetString("Role"));
        }

        [Fact]
        public void Login_ValidEmployee_SetsSessionAndRedirectsToEmployee()
        {
            var controller = CreateController(out var session);

            var result = controller.Login(new LoginModel { Username = "employee", Password = "1234" });

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Employee", redirect.ControllerName);
            Assert.Equal("Employee", session.GetString("Role"));
        }

        [Fact]
        public void Login_WrongPassword_ReturnsViewWithError()
        {
            var controller = CreateController(out var session);

            var result = controller.Login(new LoginModel { Username = "donor", Password = "wrong" });

            Assert.IsType<ViewResult>(result);
            Assert.False(controller.ModelState.IsValid);
            Assert.Null(session.GetString("Role"));
        }

        [Fact]
        public void Logout_ClearsSessionAndRedirectsToLogin()
        {
            var controller = CreateController(out var session);
            session.SetString("Role", "Donor");

            var result = controller.Logout();

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirect.ActionName);
            Assert.Null(session.GetString("Role"));
        }

        [Fact]
        public void Register_InvalidModelState_ReturnsSameView()
        {
            var controller = CreateController(out _);
            controller.ModelState.AddModelError("Email", "Required");
            var model = new RegisterModel();

            var result = controller.Register(model);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(model, view.Model);
        }
    }

    public class DonationControllerTests
    {
        [Fact]
        public void Index_ValidDonation_ShowsTaxCertificateWithReference()
        {
            var controller = new DonationController();
            var model = new DonationModel { DonorName = "Naledi", Amount = 250, Currency = "ZAR", DonationType = "One-Time" };

            var result = controller.Index(model);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Equal("TaxCertificate", view.ViewName);
            Assert.StartsWith("GOTG-TAX-", (string)controller.ViewBag.ReferenceNumber);
        }

        [Fact]
        public void Index_InvalidModelState_ReturnsFormAgain()
        {
            var controller = new DonationController();
            controller.ModelState.AddModelError("Amount", "Invalid");
            var model = new DonationModel();

            var result = controller.Index(model);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Null(view.ViewName);
            Assert.Same(model, view.Model);
        }

        [Fact]
        public void Index_ValidDonation_CertificateNumberUsesDonorNamePrefix()
        {
            var controller = new DonationController();
            var model = new DonationModel { DonorName = "Naledi", Amount = 250 };

            controller.Index(model);

            Assert.StartsWith("GOTG-TAX-NAL-", (string)controller.ViewBag.ReferenceNumber);
        }
    }

    public class VolunteerControllerTests
    {
        [Fact]
        public void Index_ValidVolunteer_ShowsConfirmation()
        {
            var controller = new GiftOfTheGivers.Controllers.VolunteerController();
            var volunteer = new VolunteerModel
            {
                FullName = "Thabo",
                Email = "t@example.com",
                Phone = "0821234567",
                Skills = "Driving",
                Availability = "Weekends"
            };

            var result = controller.Index(volunteer);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Equal("Confirmation", view.ViewName);
        }

        [Fact]
        public void Index_InvalidVolunteer_ReturnsFormAgain()
        {
            var controller = new GiftOfTheGivers.Controllers.VolunteerController();
            controller.ModelState.AddModelError("FullName", "Required");

            var result = controller.Index(new VolunteerModel());

            var view = Assert.IsType<ViewResult>(result);
            Assert.Null(view.ViewName);
        }
    }
}