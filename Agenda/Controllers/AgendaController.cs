using Agenda.Models;
using Agenda.Services;
using Agenda.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Agenda.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AgendaController : ControllerBase
    {
        private readonly EventService _eventService;
        public AgendaController(EventService eventService) 
        {
            _eventService = eventService;
        }

        [HttpGet]
        [Route("index")]
        public async Task<IActionResult> Index(int userId)
        {
            var response = await _eventService.GetAllAsync(userId);
            return Ok(response);
        }

        [HttpGet]
        [Route("get")]
        public async Task<IActionResult> Get(int eventId)
        {
            var response = await _eventService.GetAsync(eventId);
            return Ok(response);
        }

        [HttpPut]
        [Route("create")]
        public async Task<IActionResult> Create([FromBody] CreateEventData data)
        {
            var response = await _eventService.CreateAsync(data, data.UserId);
            return Ok(response);
        }

        [HttpDelete]
        [Route("delete")]
        public async Task<IActionResult> Delete(int eventId)
        {
            var response = await _eventService.DeleteAsync(eventId);
            return Ok(response);
        }

        [HttpPut]
        [Route("edit")]
        public async Task<IActionResult> Edit([FromBody] CreateEventData data)
        {
            var response = await _eventService.EditAsync(data);
            return Ok(response);
        }
    }
}
