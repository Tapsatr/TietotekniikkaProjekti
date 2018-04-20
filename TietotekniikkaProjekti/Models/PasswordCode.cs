using System;
using System.ComponentModel.DataAnnotations;

namespace TietotekniikkaProjekti.Data
{
    public class PasswordCode
    {
        [Required]
        public int ID { get; set; }
        [Required]
        public string Username{ get; set; }
        [Required]
        public string Code { get; set; }
        [Required]
        public bool IsUsed { get; set; }
        [Required]
        public DateTime TimeStamp { get; set; }
    }
}