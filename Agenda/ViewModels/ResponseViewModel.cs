using Agenda.Models;

namespace Agenda.ViewModels
{
    public class ResponseViewModel
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public TokenViewModel? TokenInfo { get; set; }
        public List<Event>? Events { get; set; }
    }
}
