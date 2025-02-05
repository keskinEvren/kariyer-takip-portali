using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AuthenticationAndAuthorization.Data;
using AuthenticationAndAuthorization.Models;
using KariyerTakip.Models;
using KariyerTakip.Services;
using Microsoft.AspNetCore.Identity;

namespace KariyerTakip.Controllers
{
    public class InternshipFormController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly FileSystemManager _fileSystemManager;

        public InternshipFormController(
            ApplicationDbContext context,
            UserManager<User> userManager,
            FileSystemManager fileSystemManager)
        {
            _context = context;
            _userManager = userManager;
            _fileSystemManager = fileSystemManager;
        }

        // GET: InternshipForm
        public async Task<IActionResult> Index()
        {
            return View(await _context.InternshipForm
                .Where(x => x.UserId == Guid.Parse(_userManager.GetUserId(User)))
                .Include(i => i.ApprovedBy)
                .Include(i => i.FinalizedBy)
                .Include(i => i.User)
                .ToListAsync());
        }

        // GET: InternshipForm/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var internshipForm = await _context.InternshipForm
                .Include(i => i.ApprovedBy)
                .Include(i => i.FinalizedBy)
                .Include(i => i.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (internshipForm == null)
            {
                return NotFound();
            }

            return View(internshipForm);
        }

        // GET: InternshipForm/Create
        public IActionResult Create()
        {
            ViewData["ApprovedById"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["FinalizedById"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: InternshipForm/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            InternshipForm internshipForm)
        {
            var userId = _userManager.GetUserId(User);
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == Guid.Parse(userId));
            if (user == null)
            {
                return NotFound();
            }

            if (
                string.IsNullOrEmpty(user.TcNo) ||
                string.IsNullOrEmpty(user.MyPhoneNumber) ||
                string.IsNullOrEmpty(user.StudentId) ||
                string.IsNullOrEmpty(user.Department) ||
                string.IsNullOrEmpty(user.Address))
            {
                ViewData["Error"] =
                    "Your profile is incomplete. Please ensure your Name, Email, and Phone are filled out.";
                ViewData["ApprovedById"] = new SelectList(_context.Users, "Id", "Id", internshipForm.ApprovedById);
                ViewData["FinalizedById"] = new SelectList(_context.Users, "Id", "Id", internshipForm.FinalizedById);
                ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", internshipForm.UserId);
                return View(internshipForm);
            }

            if (ModelState.IsValid)
            {
                internshipForm.Id = Guid.NewGuid();
                internshipForm.IsApproved = false;
                internshipForm.IsSucessfullyFinished = false;
                internshipForm.UserId = Guid.Parse(_userManager.GetUserId(User));

                _context.Add(internshipForm);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["ApprovedById"] = new SelectList(_context.Users, "Id", "Id", internshipForm.ApprovedById);
            ViewData["FinalizedById"] = new SelectList(_context.Users, "Id", "Id", internshipForm.FinalizedById);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", internshipForm.UserId);
            return View(internshipForm);
        }

        // GET: InternshipForm/Edit/5
        public async Task<IActionResult> Edit(
            Guid? id
        )
        {
            if (id == null)
            {
                return NotFound();
            }

            var internshipForm = await _context.InternshipForm.FindAsync(id);
            if (internshipForm == null)
            {
                return NotFound();
            }


            ViewData["ApprovedById"] = new SelectList(_context.Users, "Id", "Id", internshipForm.ApprovedById);
            ViewData["FinalizedById"] = new SelectList(_context.Users, "Id", "Id", internshipForm.FinalizedById);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", internshipForm.UserId);
            return View(internshipForm);
        }

        // POST: InternshipForm/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            Guid id,
            InternshipForm internshipForm,
            IFormFile? applicationForm,
            IFormFile? internshipAcceptanceForm,
            IFormFile? identificationCard,
            IFormFile? residenceCertificate,
            IFormFile? sgkEligibilityCertificate,
            IFormFile? criminalRecordCertificate,
            IFormFile? workAccidentAndOccupationalDiseaseInsuranceForm)
        {
            if (id != internshipForm.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingForm = await _context.InternshipForm.FindAsync(id);

                    if (existingForm == null)
                    {
                        return NotFound();
                    }

                    if (applicationForm != null && applicationForm.Length > 0)
                    {
                        var isApplicationFormPdf = Path.GetExtension(applicationForm.FileName)?.ToLower() == ".pdf";
                        if (!isApplicationFormPdf)
                        {
                            ViewData["Error"] = "Yüklenen her bir evrak .pdf formatında olmalıdır.";
                            ViewData["ApprovedById"] = new SelectList(_context.Users, "Id", "Id", internshipForm.ApprovedById);
                            ViewData["FinalizedById"] = new SelectList(_context.Users, "Id", "Id", internshipForm.FinalizedById);
                            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", internshipForm.UserId);
                            return View(existingForm);
                        }
                    }

                    if (internshipAcceptanceForm != null && internshipAcceptanceForm.Length > 0)
                    {
                        var isInternshipAcceptanceFormPdf =
                            Path.GetExtension(internshipAcceptanceForm.FileName)?.ToLower() == ".pdf";
                        if (!isInternshipAcceptanceFormPdf)
                        {
                            ViewData["Error"] = "Yüklenen her bir evrak .pdf formatında olmalıdır.";
                            ViewData["ApprovedById"] = new SelectList(_context.Users, "Id", "Id", internshipForm.ApprovedById);
                            ViewData["FinalizedById"] = new SelectList(_context.Users, "Id", "Id", internshipForm.FinalizedById);
                            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", internshipForm.UserId);
                            return View(existingForm);
                        }
                 
                    }

                    if (identificationCard != null && identificationCard.Length > 0)
                    {
                        var isIdentificationCardPdf = Path.GetExtension(identificationCard.FileName)?.ToLower() == ".pdf";
                        if (!isIdentificationCardPdf)
                        {
                            ViewData["Error"] = "Yüklenen her bir evrak .pdf formatında olmalıdır.";
                            ViewData["ApprovedById"] = new SelectList(_context.Users, "Id", "Id", internshipForm.ApprovedById);
                            ViewData["FinalizedById"] = new SelectList(_context.Users, "Id", "Id", internshipForm.FinalizedById);
                            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", internshipForm.UserId);
                            return View(existingForm);
                        }
                    }

                    if (residenceCertificate != null && residenceCertificate.Length > 0)
                    {
                        var isResidenceCertificatePdf =
                            Path.GetExtension(residenceCertificate.FileName)?.ToLower() == ".pdf";
                        if (!isResidenceCertificatePdf)
                        {
                            ViewData["Error"] = "Yüklenen her bir evrak .pdf formatında olmalıdır.";
                            ViewData["ApprovedById"] = new SelectList(_context.Users, "Id", "Id", internshipForm.ApprovedById);
                            ViewData["FinalizedById"] = new SelectList(_context.Users, "Id", "Id", internshipForm.FinalizedById);
                            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", internshipForm.UserId);
                            return View(existingForm);
                        }
                    }


                    if (sgkEligibilityCertificate != null && sgkEligibilityCertificate.Length > 0)
                    {
                        var isSgkEligibilityCertificatePdf =
                            Path.GetExtension(sgkEligibilityCertificate.FileName)?.ToLower() == ".pdf";
                        if (!isSgkEligibilityCertificatePdf)
                        {
                            ViewData["Error"] = "Yüklenen her bir evrak .pdf formatında olmalıdır.";
                            ViewData["ApprovedById"] = new SelectList(_context.Users, "Id", "Id", internshipForm.ApprovedById);
                            ViewData["FinalizedById"] = new SelectList(_context.Users, "Id", "Id", internshipForm.FinalizedById);
                            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", internshipForm.UserId);
                            return View(existingForm);
                        }
                    }

                    if (criminalRecordCertificate != null && criminalRecordCertificate.Length > 0)
                    {
                        var isCriminalRecordCertificatePdf =
                            Path.GetExtension(criminalRecordCertificate.FileName)?.ToLower() == ".pdf";
                        if (!isCriminalRecordCertificatePdf)
                        {
                            ViewData["Error"] = "Yüklenen her bir evrak .pdf formatında olmalıdır.";
                            ViewData["ApprovedById"] = new SelectList(_context.Users, "Id", "Id", internshipForm.ApprovedById);
                            ViewData["FinalizedById"] = new SelectList(_context.Users, "Id", "Id", internshipForm.FinalizedById);
                            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", internshipForm.UserId);
                            return View(existingForm);
                        }
                    }

                    if (workAccidentAndOccupationalDiseaseInsuranceForm != null &&
                        workAccidentAndOccupationalDiseaseInsuranceForm.Length > 0)
                    {
                        var isWorkAccidentAndOccupationalDiseaseInsuranceFormPdf =
                            Path.GetExtension(workAccidentAndOccupationalDiseaseInsuranceForm.FileName)?.ToLower() ==
                            ".pdf";
                        if (!isWorkAccidentAndOccupationalDiseaseInsuranceFormPdf)
                        {
                            ViewData["Error"] = "Yüklenen her bir evrak .pdf formatında olmalıdır.";
                            ViewData["ApprovedById"] = new SelectList(_context.Users, "Id", "Id", internshipForm.ApprovedById);
                            ViewData["FinalizedById"] = new SelectList(_context.Users, "Id", "Id", internshipForm.FinalizedById);
                            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", internshipForm.UserId);
                            return View(existingForm);
                        }
                    }

         


                    if (!(bool)existingForm.IsApproved)
                    {
                        existingForm.Company = internshipForm.Company;
                        existingForm.Description = internshipForm.Description;

                        existingForm.Industry = internshipForm.Industry;
                        existingForm.DurationInDays = internshipForm.DurationInDays;
                        existingForm.Phone = internshipForm.Phone;
                        existingForm.WebURLOfCompany = internshipForm.WebURLOfCompany;
                        existingForm.EmailOfCompany = internshipForm.EmailOfCompany;
                        existingForm.Address = internshipForm.Address;
                        existingForm.DoesCompanyWorkOnSaturday = internshipForm.DoesCompanyWorkOnSaturday;
                        existingForm.MentorFullname = internshipForm.MentorFullname;
                        existingForm.MentorTitle = internshipForm.MentorTitle;
                    }
                    else
                    {
                        if (applicationForm != null && applicationForm.Length > 0)
                        {
                            var applicationFormFolder = Path.Combine("wwwroot", "user/application-forms");
                            var applicationFormFilePath = Path.Combine(applicationFormFolder,
                                internshipForm.Id + Path.GetExtension(applicationForm.FileName));
                            await _fileSystemManager.UploadFile(applicationForm, applicationFormFolder,
                                applicationFormFilePath);
                            existingForm.ApplicationFormURL =
                                applicationFormFilePath.Replace("wwwroot", "").Replace("\\", "/");
                        }


                        if (internshipAcceptanceForm != null && internshipAcceptanceForm.Length > 0)
                        {
                            var internshipAcceptanceFormFolder =
                                Path.Combine("wwwroot", "user/internship-acceptance-forms");
                            var internshipAcceptanceFormFilePath = Path.Combine(internshipAcceptanceFormFolder,
                                internshipForm.Id + Path.GetExtension(internshipAcceptanceForm.FileName));
                            await _fileSystemManager.UploadFile(internshipAcceptanceForm,
                                internshipAcceptanceFormFolder,
                                internshipAcceptanceFormFilePath);
                            existingForm.InternshipAcceptanceFormURL = internshipAcceptanceFormFilePath
                                .Replace("wwwroot", "").Replace("\\", "/");
                        }

                        if (identificationCard != null && identificationCard.Length > 0)
                        {
                            var identificationCardFolder = Path.Combine("wwwroot", "user/identification-cards");
                            var identificationCardFilePath = Path.Combine(identificationCardFolder,
                                internshipForm.Id + Path.GetExtension(identificationCard.FileName));
                            await _fileSystemManager.UploadFile(identificationCard, identificationCardFolder,
                                identificationCardFilePath);
                            existingForm.IdentificationCardURL =
                                identificationCardFilePath.Replace("wwwroot", "").Replace("\\", "/");
                        }

                        if (residenceCertificate != null && residenceCertificate.Length > 0)
                        {
                            var residenceCertificateFolder = Path.Combine("wwwroot", "user/residence-certificates");
                            var residenceCertificateFilePath = Path.Combine(residenceCertificateFolder,
                                internshipForm.Id + Path.GetExtension(residenceCertificate.FileName));
                            await _fileSystemManager.UploadFile(residenceCertificate, residenceCertificateFolder,
                                residenceCertificateFilePath);
                            existingForm.ResidenceCertificateURL =
                                residenceCertificateFilePath.Replace("wwwroot", "").Replace("\\", "/");
                        }


                        if (sgkEligibilityCertificate != null && sgkEligibilityCertificate.Length > 0)
                        {
                            var sgkEligibilityCertificateFolder =
                                Path.Combine("wwwroot", "user/sgk-eligibility-certificates");
                            var sgkEligibilityCertificateFilePath = Path.Combine(sgkEligibilityCertificateFolder,
                                internshipForm.Id + Path.GetExtension(sgkEligibilityCertificate.FileName));
                            await _fileSystemManager.UploadFile(sgkEligibilityCertificate,
                                sgkEligibilityCertificateFolder,
                                sgkEligibilityCertificateFilePath);
                            existingForm.SgkEligibilityCertificateURL = sgkEligibilityCertificateFilePath
                                .Replace("wwwroot", "").Replace("\\", "/");
                        }


                        if (criminalRecordCertificate != null && criminalRecordCertificate.Length > 0)
                        {
                            var criminalRecordCertificateFolder =
                                Path.Combine("wwwroot", "user/criminal-record-certificates");
                            var criminalRecordCertificateFilePath = Path.Combine(criminalRecordCertificateFolder,
                                internshipForm.Id + Path.GetExtension(criminalRecordCertificate.FileName));
                            await _fileSystemManager.UploadFile(criminalRecordCertificate,
                                criminalRecordCertificateFolder,
                                criminalRecordCertificateFilePath);
                            existingForm.CriminalRecordCertificateURL = criminalRecordCertificateFilePath
                                .Replace("wwwroot", "").Replace("\\", "/");
                        }


                        if (workAccidentAndOccupationalDiseaseInsuranceForm != null &&
                            workAccidentAndOccupationalDiseaseInsuranceForm.Length > 0)
                        {
                            var workAccidentAndOccupationalDiseaseInsuranceFormFolder = Path.Combine("wwwroot",
                                "user/work-accident-and-occupational-disease-insurance-forms");
                            var workAccidentAndOccupationalDiseaseInsuranceFormFilePath = Path.Combine(
                                workAccidentAndOccupationalDiseaseInsuranceFormFolder,
                                internshipForm.Id +
                                Path.GetExtension(workAccidentAndOccupationalDiseaseInsuranceForm.FileName));
                            await _fileSystemManager.UploadFile(workAccidentAndOccupationalDiseaseInsuranceForm,
                                workAccidentAndOccupationalDiseaseInsuranceFormFolder,
                                workAccidentAndOccupationalDiseaseInsuranceFormFilePath);
                            existingForm.WorkAccidentAndOccupationalDiseaseInsuranceFormURL =
                                workAccidentAndOccupationalDiseaseInsuranceFormFilePath.Replace("wwwroot", "")
                                    .Replace("\\", "/");
                        }
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InternshipFormExists(internshipForm.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["ApprovedById"] = new SelectList(_context.Users, "Id", "Id", internshipForm.ApprovedById);
            ViewData["FinalizedById"] = new SelectList(_context.Users, "Id", "Id", internshipForm.FinalizedById);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", internshipForm.UserId);
            return View(internshipForm);
        }

        // GET: InternshipForm/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var internshipForm = await _context.InternshipForm
                .Include(i => i.ApprovedBy)
                .Include(i => i.FinalizedBy)
                .Include(i => i.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (internshipForm == null)
            {
                return NotFound();
            }

            return View(internshipForm);
        }

        // POST: InternshipForm/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var internshipForm = await _context.InternshipForm.FindAsync(id);
            if (internshipForm != null)
            {
                _context.InternshipForm.Remove(internshipForm);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool InternshipFormExists(Guid id)
        {
            return _context.InternshipForm.Any(e => e.Id == id);
        }
    }
}