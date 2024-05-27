using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeodicBankAPI.Domain.Entities
{
    public partial class User
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public byte[] Password { get; set; }
        public string Email { get; set; }  
        public string Phone { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public byte[] PasswordSalt { get; set; }
        public decimal Balance { get; set; }
        public string City { get; set; }
        public string? Country { get; set; }
        public DateTime? DateOfBirth { get; set; }
    }
}
