using AuthenticationAndAuthorization.Models;

namespace KariyerTakip.Models;

public class InternshipForm
{
    public Guid Id { get; set; }
    public string Company { get; set; }
    public string Description { get; set; }
    
    public string Industry { get; set; }
    public int DurationInDays { get; set; }
    public string Phone { get; set; }
    public string WebURLOfCompany { get; set; }
    public string EmailOfCompany { get; set; }
    public string Address { get; set; }
    public bool DoesCompanyWorkOnSaturday { get; set; }
    public string MentorFullname { get; set; }
    public string MentorTitle { get; set; }
    
    public bool? IsApproved { get; set; }
    public bool? IsSucessfullyFinished { get; set; }
    
            
    public string? ApplicationFormURL { get; set; } 
    public string? InternshipAcceptanceFormURL { get; set; } 
    public string? IdentificationCardURL { get; set; } 
    public string? ResidenceCertificateURL { get; set; } 
    public string? SgkEligibilityCertificateURL { get; set; }
    public string? CriminalRecordCertificateURL { get; set; } 
    public string? WorkAccidentAndOccupationalDiseaseInsuranceFormURL { get; set; } 
    
    
    public Guid? UserId { get; set; }
    public User? User { get; set; }
    
    public User? ApprovedBy { get; set; }
    public Guid? ApprovedById { get; set; }
    
    public User? FinalizedBy { get; set; }
    public Guid? FinalizedById { get; set; }
}