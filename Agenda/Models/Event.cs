using Agenda.ViewModels;

namespace Agenda.Models
{
    public class Event
    {
        public int EventId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public bool IsDeleted { get; set; } = false;
        public int UserId { get; set; }
        public virtual User User { get; set; }

        public Event() {}

        public Event(CreateEventData data)
        {
            Title = data.Title;
            Description = data.Description;
            CreatedAt = DateTime.UtcNow;
            Start = data.Start;
            End = data.End;
            IsDeleted = data.IsDeleted;
            UserId = data.UserId;
        }
    }
}
