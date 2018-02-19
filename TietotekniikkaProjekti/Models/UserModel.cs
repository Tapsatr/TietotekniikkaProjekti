using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TietotekniikkaProjekti.Models
{
    public class UserModel
    {
        public int Id { get; set; }
        public string Nimi { get; set; }
        public string Osoite { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public string EmployeeType { get; set; }
    }
}
