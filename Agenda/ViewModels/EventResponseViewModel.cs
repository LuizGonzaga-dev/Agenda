using Agenda.Models;

namespace Agenda.ViewModels
{
    public class EventResponseViewModel
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<CreateEventData> Events { get; set; }
    }
}
