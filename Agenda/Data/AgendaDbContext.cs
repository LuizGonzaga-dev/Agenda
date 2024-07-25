using Agenda.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Agenda.Data
{
    public class AgendaDbContext : IdentityDbContext<IdentityUser>
    {
        public AgendaDbContext(DbContextOptions<AgendaDbContext> options) : base(options) 
        { 
        
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Event> Events { get; set; }
    }
}
