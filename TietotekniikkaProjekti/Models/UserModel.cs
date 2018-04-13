﻿using System;
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
        public string Password { get; set; }
        [Required]
        public string Nimi { get; set; }
        public string Osoite { get; set; }
        public string Email { get; set; }
        public string EmployeeType { get; set; }
    }
}
