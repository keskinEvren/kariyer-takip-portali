
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AuthenticationAndAuthorization.Data;
using AuthenticationAndAuthorization.Models;
using KariyerTakip.Models;
using Microsoft.AspNetCore.Identity;

namespace KariyerTakip.Controllers
{
    public class InternshipFormAdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public InternshipFormAdminController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: InternshipForm
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            
            if (await _userManager.IsInRoleAsync(currentUser, "Admin"))
            {
                var internshipForms = await _context.InternshipForm
                    .Include(x => x.User)
                    .Include(x => x.ApprovedBy)
                    .Include(x => x.FinalizedBy)
                    .ToListAsync();
                return View(internshipForms);

            }
            else if (await _userManager.IsInRoleAsync(currentUser, "Teacher"))
            {
                var internshipForms = await _context.InternshipForm
                    .Include(x => x.User)
                    .Include(x => x.ApprovedBy)
                    .Include(x => x.FinalizedBy)
                    .Where(x => x.User.Department == currentUser.Department)
                    .ToListAsync();
                return View(internshipForms);

            }
            return NotFound();
        }

        // GET: InternshipForm/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var internshipForm = await _context.InternshipForm
                .Include(x => x.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (internshipForm == null)
            {
                return NotFound();
            }

            return View(internshipForm);
        }

        // GET: InternshipForm/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
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
            return View(internshipForm);
        }

        // POST: InternshipForm/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Company,Description,IsApproved,IsSucessfullyFinished,UserId")] InternshipForm internshipForm)
        {
            if (id != internshipForm.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(internshipForm);
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
                .Include(x => x.User)
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
        
        // file cloud'da degilse, path tutulmaz xd
        
        [HttpPost]
        [Route("/[controller]/ToggleApproval/{internshipId:guid}")]
        public IActionResult ToggleApproval(
            [FromRoute] Guid internshipId, 
            [FromBody] BoolBody boolBody)
        {
            var internshipForm = _context.InternshipForm.Find(internshipId);
            if (internshipForm == null)
            {
                return NotFound();
            }

            internshipForm.IsApproved = boolBody.ApprovedStatus;
            internshipForm.ApprovedById = Guid.Parse(_userManager.GetUserId(User));
            _context.SaveChanges();

            return RedirectToAction(nameof(Details), new { internshipId });
        }
        
              
        [HttpPost]
        [Route("/[controller]/ToggleFinish/{internshipId:guid}")]
        public  IActionResult ToggleFinish(
            [FromRoute] Guid internshipId, 
            [FromBody] BoolBody boolBody)
        {
            var internshipForm =  _context.InternshipForm.Find(internshipId);
            if (internshipForm == null)
            {
                return NotFound();
            }

            internshipForm.IsSucessfullyFinished = boolBody.ApprovedStatus; // Toggle the value
            internshipForm.FinalizedById = Guid.Parse(_userManager.GetUserId(User));

            var workHistoryOfInternship =  _context.WorkHistory.FirstOrDefault(x => x.InternshipFormId == internshipForm.Id);
            if (workHistoryOfInternship is null)
            {
                var workHistory = new WorkHistory
                {
                    Id = Guid.NewGuid(),
                    Company = internshipForm.Company,
                    Description = internshipForm.Description,
                    UserId = internshipForm.UserId,
                    InternshipFormId = internshipForm.Id,
                };
                 _context.WorkHistory.Add(workHistory);
            }
            _context.SaveChanges();

            return RedirectToAction(nameof(Details), new { internshipId });
        }

        public class BoolBody
        {
            public bool ApprovedStatus { get; set; }
        }
        

        private bool InternshipFormExists(Guid id)
        {
            return _context.InternshipForm.Any(e => e.Id == id);
        }
    }
}
