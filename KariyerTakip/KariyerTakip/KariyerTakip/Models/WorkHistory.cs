using AuthenticationAndAuthorization.Models;

namespace KariyerTakip.Models;

public class WorkHistory
{
    public Guid Id { get; set; }
    public string Company { get; set; }
    public string Description { get; set; }
    
    public Guid? InternshipFormId { get; set; }

    public Guid? UserId { get; set; }
    public User? User { get; set; }
}