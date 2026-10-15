using GiftOfTheGivers.Helpers;
using Xunit;

namespace GiftOfTheGivers2.Tests
{
    public class DonationHelperTests
    {
        [Fact]
        public void FormatTaxCertificateNumber_NormalName_ReturnsCorrectFormat()
        {
            var result = DonationHelper.FormatTaxCertificateNumber("Paballo Mentoor", 1045);

            Assert.Equal("GOTG-TAX-PAB-001045", result);
        }

        [Fact]
        public void FormatTaxCertificateNumber_ShortName_DoesNotCrash()
        {
            var result = DonationHelper.FormatTaxCertificateNumber("Al", 7);

            Assert.Equal("GOTG-TAX-AL-000007", result);
        }

        [Fact]
        public void CalculateNetDonation_Takes5PercentFee()
        {
            var net = DonationHelper.CalculateNetDonation(1000m);

            Assert.Equal(950m, net);
        }

        [Fact]
        public void CalculateNetDonation_TotalOfThreeDonations_IsCorrect()
        {
            var donations = new[] { 100m, 200m, 50m };

            var total = donations.Sum(d => DonationHelper.CalculateNetDonation(d));

            Assert.Equal(332.50m, total);
        }
    }
}