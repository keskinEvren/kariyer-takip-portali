using KariyerTakip.Models;
using Microsoft.AspNetCore.Identity;

namespace AuthenticationAndAuthorization.Models
{
    public class User : IdentityUser<Guid>
    {
        public override Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string TcNo { get; set; } = string.Empty;
        public string MyPhoneNumber { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        
        public string Department { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string ProfilePictureURL { get; set; } = string.Empty;

    }
}