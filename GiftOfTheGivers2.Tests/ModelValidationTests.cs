using System.ComponentModel.DataAnnotations;
using GiftOfTheGivers2.Models;
using Xunit;

namespace GiftOfTheGivers2.Tests
{
    // Runs the same validation rules MVC uses when a form is submitted
    public static class ModelValidation
    {
        public static List<ValidationResult> Validate(object model)
        {
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(model, new ValidationContext(model), results, true);
            return results;
        }
    }

    public class DonationModelTests
    {
        private static DonationModel ValidDonation() => new DonationModel
        {
            DonorName = "Naledi",
            Amount = 500,
            Currency = "ZAR",
            DonationType = "One-Time",
            Email = "naledi@example.com"
        };

        [Fact]
        public void ValidDonation_PassesValidation()
        {
            Assert.Empty(ModelValidation.Validate(ValidDonation()));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        [InlineData(1000001)]
        public void Amount_OutsideAllowedRange_FailsValidation(decimal amount)
        {
            var model = ValidDonation();
            model.Amount = amount;

            Assert.Contains(ModelValidation.Validate(model), r => r.MemberNames.Contains("Amount"));
        }

        [Fact]
        public void MissingDonorName_FailsValidation()
        {
            var model = ValidDonation();
            model.DonorName = "";

            Assert.Contains(ModelValidation.Validate(model), r => r.MemberNames.Contains("DonorName"));
        }

        [Fact]
        public void InvalidEmail_FailsValidation()
        {
            var model = ValidDonation();
            model.Email = "not-an-email";

            Assert.Contains(ModelValidation.Validate(model), r => r.MemberNames.Contains("Email"));
        }
    }

    public class VolunteerModelTests
    {
        private static VolunteerModel ValidVolunteer() => new VolunteerModel
        {
            FullName = "Thabo Nkosi",
            Email = "thabo@example.com",
            Phone = "0821234567",
            Skills = "First aid",
            Availability = "Weekends"
        };

        [Fact]
        public void ValidVolunteer_PassesValidation()
        {
            Assert.Empty(ModelValidation.Validate(ValidVolunteer()));
        }

        [Fact]
        public void MissingSkills_FailsValidation()
        {
            var model = ValidVolunteer();
            model.Skills = "";

            Assert.Contains(ModelValidation.Validate(model), r => r.MemberNames.Contains("Skills"));
        }

        [Fact]
        public void InvalidEmail_FailsValidation()
        {
            var model = ValidVolunteer();
            model.Email = "thabo-at-example";

            Assert.Contains(ModelValidation.Validate(model), r => r.MemberNames.Contains("Email"));
        }
    }

    public class RegisterModelTests
    {
        [Fact]
        public void PasswordsThatDoNotMatch_FailValidation()
        {
            var model = new RegisterModel
            {
                FullName = "Donor One",
                Email = "donor@example.com",
                Phone = "0821234567",
                Username = "donor1",
                Password = "Secret123",
                ConfirmPassword = "Different123"
            };

            Assert.Contains(ModelValidation.Validate(model), r => r.MemberNames.Contains("ConfirmPassword"));
        }
    }
}