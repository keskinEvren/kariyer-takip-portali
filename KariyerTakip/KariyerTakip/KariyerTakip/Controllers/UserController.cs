using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AuthenticationAndAuthorization.Data;
using AuthenticationAndAuthorization.Models;
using KariyerTakip.DTOs;
using KariyerTakip.Models;
using KariyerTakip.Services;
using KariyerTakip.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace KariyerTakip.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly FileSystemManager _fileSystemManager;

        public UserController(
            ApplicationDbContext context,
            SignInManager<User> signInManager,
            RoleManager<IdentityRole<Guid>> roleManager,
            UserManager<User> userManager,
            FileSystemManager fileSystemManager)
        {
            _context = context;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _userManager = userManager;
            _fileSystemManager = fileSystemManager;
        }

        // GET: User
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var userWithRoles = new List<UserWithRolesDto>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userWithRoles.Add(new UserWithRolesDto
                {
                    User = user,
                    Roles = roles.ToList()
                });
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (await _userManager.IsInRoleAsync(currentUser, "Admin"))
            {
                return View(users);
            }
            else if (await _userManager.IsInRoleAsync(currentUser, "Teacher"))
            {
                return View(userWithRoles
                    .Where(x=> !x.Roles.Contains("Teacher") && 
                               !x.Roles.Contains("Admin"))
                    .Where(x => x.User.Department == currentUser.Department)
                    .Select(x => x.User));
            }

            return NotFound();
        }

        // GET: User/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: User/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: User/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            User user)
        {
            if (ModelState.IsValid)
            {
                user.Id = Guid.NewGuid();
                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(user);
        }

        // GET: User/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: User/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            Guid id,
            User user,
            IFormFile? profilePicture)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingUser = await _context.Users.FindAsync(id);

                    if (existingUser == null)
                    {
                        return NotFound();
                    }

                    existingUser.FirstName = user.FirstName;
                    existingUser.LastName = user.LastName;

                    existingUser.TcNo = user.TcNo;
                    existingUser.StudentId = user.StudentId;
                    existingUser.Department = user.Department;
                    existingUser.Address = user.Address;
                    existingUser.MyPhoneNumber = user.MyPhoneNumber;

                    if (profilePicture != null && profilePicture.Length > 0)
                    {
                        var uploadsFolder = Path.Combine("wwwroot", "user/profile-pictures");
                        var filePath = Path.Combine(uploadsFolder,
                            existingUser.Id + Path.GetExtension(profilePicture.FileName));
                        await _fileSystemManager.UploadFile(profilePicture, uploadsFolder, filePath);
                        existingUser.ProfilePictureURL = filePath.Replace("wwwroot", "").Replace("\\", "/");
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
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

            return View(user);
        }

        // GET: User/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: User/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // public async Task<IActionResult> UserDetails(Guid UserId)
        // {
        //     var user = await _context.Users.Where(x => x.Id == UserId).FirstOrDefaultAsync(); 
        //     return View("Details", user);
        // }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignRole(AssignRolesViewModel request)
        {
            var user = await _userManager.FindByIdAsync(request.Id.ToString());

            var currentRoles = await _userManager.GetRolesAsync(user);

            var roles = request.Roles.Split(',');

            var rolesToRemove = currentRoles.Except(roles).ToList();

            var rolesToAdd = roles.Except(currentRoles).ToList();

            if (rolesToRemove.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            }

            if (rolesToAdd.Any())
            {
                await _userManager.AddToRolesAsync(user, rolesToAdd);
            }


            return RedirectToAction("Index", "Home");
        }

        private bool UserExists(Guid id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}