﻿
namespace Agenda.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string IdentityUserId { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string? Phone { get; set; }
        public bool IsAdm { get; set; }
        public ICollection<Event> Events { get; set; }

    }
}
