using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AuthenticationAndAuthorization.Models;
using KariyerTakip.Models;
using Microsoft.AspNetCore.Identity;

namespace AuthenticationAndAuthorization.Data
{
    public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
        
        public DbSet<WorkHistory> WorkHistory { get; set; } = default!;
        public DbSet<InternshipForm> InternshipForm { get; set; } = default!;
    }
}