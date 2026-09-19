using System.ComponentModel.DataAnnotations;

namespace GiftOfTheGivers2.Models
{
    public class DonationModel
{
    [Required]
    [Display(Name = "Donor Name")]
    public string DonorName { get; set; } = "";

    [Required]
    [Range(1, 1000000)]
    [Display(Name = "Donation Amount")]
    public decimal Amount { get; set; }

    [Required]
    [Display(Name = "Currency")]
    public string Currency { get; set; } = "ZAR";

    [Required]
    [Display(Name = "Donation Type")]
    public string DonationType { get; set; } = "One-Time";

    [Display(Name = "Email")]
    [EmailAddress]
    public string Email { get; set; } = "";

    public DateTime DonationDate { get; set; } = DateTime.Now;

    public string TaxCertificate { get; set; } = "";
}
}