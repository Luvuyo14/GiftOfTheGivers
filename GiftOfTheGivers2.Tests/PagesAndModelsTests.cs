using giftofthegivers2.function.Models;
using GiftOfTheGivers2.Models;
using GiftOfTheGivers2.Pages;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Xunit;

namespace GiftOfTheGivers2.Tests
{
    public class EmployeeUpdateModelTests
    {
        [Fact]
        public void EmployeeUpdateModel_StoresAllValues()
        {
            var model = new EmployeeUpdateModel
            {
                EmployeeId = "EMP-001",
                EmployeeName = "Thabo Nkosi",
                Department = "Logistics",
                UpdateText = "Delivered 200 blankets"
            };

            Assert.Equal("EMP-001", model.EmployeeId);
            Assert.Equal("Thabo Nkosi", model.EmployeeName);
            Assert.Equal("Logistics", model.Department);
            Assert.Equal("Delivered 200 blankets", model.UpdateText);
        }

        [Fact]
        public void EmployeeUpdateModel_TimestampDefaultsToNowUtc()
        {
            var before = DateTime.UtcNow.AddSeconds(-5);

            var model = new EmployeeUpdateModel();

            Assert.True(model.Timestamp >= before);
            Assert.True(model.Timestamp <= DateTime.UtcNow.AddSeconds(5));
        }
    }

    public class CertificateResponseModelTests
    {
        [Fact]
        public void CertificateResponseModel_StoresAllValues()
        {
            var model = new CertificateResponseModel
            {
                Success = true,
                CertificateNumber = "GOTG-TAX-NAL-000001",
                DonorName = "Naledi",
                Amount = 250m,
                Currency = "ZAR",
                DonationType = "One-Time",
                Date = "2026-10-15",
                Organisation = "Gift of the Givers"
            };

            Assert.True(model.Success);
            Assert.Equal("GOTG-TAX-NAL-000001", model.CertificateNumber);
            Assert.Equal("Naledi", model.DonorName);
            Assert.Equal(250m, model.Amount);
            Assert.Equal("ZAR", model.Currency);
            Assert.Equal("One-Time", model.DonationType);
            Assert.Equal("2026-10-15", model.Date);
            Assert.Equal("Gift of the Givers", model.Organisation);
        }
    }

    public class TaxCertificatePageTests
    {
        private static TaxCertificateModelPage CreatePage()
        {
            var page = new TaxCertificateModelPage();
            var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary());
            page.PageContext = new PageContext { ViewData = viewData };
            return page;
        }

        [Fact]
        public void OnGet_DoesNotThrow_AndStartsWithEmptyDonation()
        {
            var page = CreatePage();

            page.OnGet();

            Assert.NotNull(page.DonationInput);
        }

        [Fact]
        public async Task OnPostAsync_EmptyDonorName_ReturnsPageWithError()
        {
            var page = CreatePage();
            page.DonationInput = new DonationModel { DonorName = "", Amount = 100 };

            var result = await page.OnPostAsync();

            Assert.IsType<PageResult>(result);
            Assert.False(page.ModelState.IsValid);
        }

        [Fact]
        public async Task OnPostAsync_ZeroAmount_ReturnsPageWithError()
        {
            var page = CreatePage();
            page.DonationInput = new DonationModel { DonorName = "Naledi", Amount = 0 };

            var result = await page.OnPostAsync();

            Assert.IsType<PageResult>(result);
            Assert.False(page.ModelState.IsValid);
        }

        // Note: stop your Azure Function if it is running locally, otherwise this test
        // reaches the real function instead of the "offline" path.
        [Fact]
        public async Task OnPostAsync_FunctionNotRunning_ShowsOfflineReference()
        {
            var page = CreatePage();
            page.DonationInput = new DonationModel { DonorName = "Naledi", Amount = 100 };

            var result = await page.OnPostAsync();

            Assert.IsType<PageResult>(result);
            Assert.Equal("OFFLINE-MODE", page.ViewData["ReferenceNumber"]);
        }
    }
}