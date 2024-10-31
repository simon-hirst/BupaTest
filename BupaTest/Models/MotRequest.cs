using System.ComponentModel.DataAnnotations;

namespace BupaTest.Models
{
    public class MotRequest
    {
        [Required(AllowEmptyStrings = false)]
        [RegularExpression(@"^[A-Z]{2}[0-9]{2}[A-Z]{3}$", ErrorMessage = "Please enter a valid UK registration number.")]
        public string RegistrationNumber { get; set; } = string.Empty;
    }
}