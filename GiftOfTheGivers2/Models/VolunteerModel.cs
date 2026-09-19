using System.ComponentModel.DataAnnotations;

namespace GiftOfTheGivers2.Models
{
    public class VolunteerModel
    {
        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = "";

        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        [Phone]
        public string Phone { get; set; } = "";

        [Required]
        [Display(Name = "Skills")]
        public string Skills { get; set; } = "";

        [Required]
        [Display(Name = "Availability")]
        public string Availability { get; set; } = "";

        public DateTime RegistrationDate { get; set; } = DateTime.Now;
    }
}