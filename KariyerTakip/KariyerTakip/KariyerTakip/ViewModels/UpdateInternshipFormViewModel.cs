using KariyerTakip.Models;

namespace KariyerTakip.ViewModels;

public class UpdateInternshipFormViewModel
{
    public Guid Id { get; set; }
    public InternshipForm InternshipForm { get; set; }
    public IFormFile? ApplicationForm { get; set; }
    public IFormFile? InternshipAcceptanceForm { get; set; }
    public IFormFile? IdentificationCard { get; set; }
    public IFormFile? ResidenceCertificate { get; set; }
    public IFormFile? SgkEligibilityCertificate { get; set; }
    public IFormFile? CriminalRecordCertificate { get; set; }
    public IFormFile? WorkAccidentAndOccupationalDiseaseInsuranceForm { get; set; }
}