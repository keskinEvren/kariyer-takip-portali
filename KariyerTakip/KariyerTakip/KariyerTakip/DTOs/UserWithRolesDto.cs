using AuthenticationAndAuthorization.Models;

namespace KariyerTakip.DTOs;

public class UserWithRolesDto
{
    public User User { get; set; }
    public List<string> Roles { get; set; }
}