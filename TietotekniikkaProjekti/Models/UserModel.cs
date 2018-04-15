using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TietotekniikkaProjekti.Models
{
    public class UserModel
    {
        public int Id { get; set; }
        [Required]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [StringLength(50 , MinimumLength = 7, ErrorMessage = "Password must be over 7 chars ")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,30}$", ErrorMessage = "Password must have at least one uppercase letter and one number")]
        public string Password { get; set; }
        [Required]
        public string Nimi { get; set; }
        [Required]
        public string Sukunimi { get; set; }
        [Required]
        public string Osoite { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string EmployeeType { get; set; }
        public string Group { get; set; }
        public bool Enabled { get; set; }
    }
}
