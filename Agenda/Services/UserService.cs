using Agenda.Data;
using Microsoft.AspNetCore.Identity;

namespace Agenda.Services
{
    public class UserService
    {
        private readonly AgendaDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        public UserService(AgendaDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }
    }
}
