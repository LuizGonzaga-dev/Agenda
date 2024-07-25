using Agenda.Models;

namespace Agenda.ViewModels
{
    public class CreateEventData
    {
        public int EventId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public bool IsDeleted { get; set; } = false;
        public int UserId { get; set; }

        public CreateEventData()
        {}

        public CreateEventData(Event e)
        {
            EventId = e.EventId;
            Title = e.Title;
            Description = e.Description;
            Start = e.Start;
            End = e.End;
            IsDeleted = e.IsDeleted;
            UserId = e.UserId;
        }
    }
}
