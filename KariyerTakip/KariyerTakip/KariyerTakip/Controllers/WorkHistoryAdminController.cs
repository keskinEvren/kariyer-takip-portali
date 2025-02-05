
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AuthenticationAndAuthorization.Data;
using KariyerTakip.Models;

namespace KariyerTakip
{
    public class WorkHistoryAdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WorkHistoryAdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: WorkHistory
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View(await _context.WorkHistory.Include(x => x.User).ToListAsync());
        }

        // GET: WorkHistory/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var workHistory = await _context.WorkHistory
                .FirstOrDefaultAsync(m => m.Id == id);
            if (workHistory == null)
            {
                return NotFound();
            }

            return View(workHistory);
        }

        // GET: WorkHistory/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: WorkHistory/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Company,Description,UserId")] WorkHistory workHistory)
        {
            if (ModelState.IsValid)
            {
                workHistory.Id = Guid.NewGuid();
                _context.Add(workHistory);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(workHistory);
        }

        // GET: WorkHistory/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var workHistory = await _context.WorkHistory.FindAsync(id);
            if (workHistory == null)
            {
                return NotFound();
            }
            return View(workHistory);
        }

        // POST: WorkHistory/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Company,Description,UserId")] WorkHistory workHistory)
        {
            if (id != workHistory.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(workHistory);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WorkHistoryExists(workHistory.Id))
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
            return View(workHistory);
        }

        // GET: WorkHistory/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var workHistory = await _context.WorkHistory
                .FirstOrDefaultAsync(m => m.Id == id);
            if (workHistory == null)
            {
                return NotFound();
            }

            return View(workHistory);
        }

        // POST: WorkHistory/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var workHistory = await _context.WorkHistory.FindAsync(id);
            if (workHistory != null)
            {
                _context.WorkHistory.Remove(workHistory);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool WorkHistoryExists(Guid id)
        {
            return _context.WorkHistory.Any(e => e.Id == id);
        }
    }
}
