using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;


namespace SoftwareFromClick.Models
{
    public class User
    {

        public int Id { get; set; }

        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;


        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // relacje
        
        public ICollection<ApiKeys> ApiKeys { get; set; } = new List<ApiKeys>(); // Jeden użytkownik może mieć wiele kluczy API
        public ICollection<Query> Queries { get; set; } = new List<Query>(); // Jeden użytkownik może mieć wiele zapytań
        public ICollection<Sessions> Sessions { get; set; } = new List<Sessions>(); // Jeden użytkownik może mieć wiele sesji
        public ICollection<EditedResults> EditedResults { get; set; } = new List<EditedResults>(); // Jeden użytkownik może edytować wiele wyników
    }
}
