using Agenda.Data;
using Agenda.Models;
using Agenda.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Agenda.Services
{
    public class EventService
    {
        private readonly AgendaDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        public EventService(AgendaDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<EventResponseViewModel> CreateAsync([FromBody] CreateEventData data, int UserId)
        {
            try
            {
                if(IsStartGreaterThanEnd(data.Start, data.End))
                {
                    return new EventResponseViewModel
                    {
                        Success = false,
                        Message = "Erro: data final menor que ou igual a data inicial!"
                    };
                }

                var user = await _db.Users.FirstOrDefaultAsync(x => x.UserId == UserId);

                if (user is null)
                {
                    return new EventResponseViewModel
                    {
                        Success = false,
                        Message = "Usuário não existe"
                    };
                };

                data.UserId = UserId;

                Event ev = new Event(data);

                await _db.Events.AddAsync(ev);
                await _db.SaveChangesAsync();

                data.EventId = ev.EventId;

                return new EventResponseViewModel
                {
                    Success = true,
                    Message = "Usuário criado com sucesso!",
                    Events = new List<CreateEventData>() { data }
                };
            }
            catch (Exception ex)
            {
                return new EventResponseViewModel
                {
                    Success = false,
                    Message = "Não foi possível criar o usuário!",
                };
            }
        }

        public async Task<EventResponseViewModel> DeleteAsync(int eventId)
        {
            try
            {
                var ev = await _db.Events.FirstOrDefaultAsync(x => x.EventId == eventId);

                if (ev is null) 
                {
                    return new EventResponseViewModel
                    {
                        Success = false,
                        Message = "O evento não existe no banco de dados!"
                    };
                }

                ev.IsDeleted = true;
                _db.Events.Update(ev);
                await _db.SaveChangesAsync();

                return new EventResponseViewModel
                {
                    Success = true,
                    Message = "O evento foi removido com sucesso!"
                };
            }
            catch (Exception ex)
            {
                return new EventResponseViewModel
                {
                    Success = false,
                    Message = "Não foi possível deletar o usuário"
                };
            }
        }

        public async Task<EventResponseViewModel> EditAsync([FromBody] CreateEventData data)
        {
            try
            {
                var ev = await _db.Events.FirstOrDefaultAsync(x => x.EventId == data.EventId);

                if (ev is null)
                {
                    return new EventResponseViewModel
                    {
                        Success = false,
                        Message = "Evento não encontrado!"
                    };
                }

                if (IsStartGreaterThanEnd(data.Start, data.End))
                {
                    return new EventResponseViewModel
                    {
                        Success = false,
                        Message = "Erro: data final menor que ou igual a data inicial!"
                    };
                }

                ev.Start = data.Start;
                ev.End = data.End;
                ev.Description = data.Description;
                ev.Title = data.Title;
                 

                _db.Events.Update(ev);
                await _db.SaveChangesAsync();

                return new EventResponseViewModel
                {
                    Success = true,
                    Message = "Evento editado com sucesso",
                    Events = new List<CreateEventData>() { data }
                };
            }
            catch (Exception ex) 
            {
                return new EventResponseViewModel
                {
                    Success = false,
                    Message = "Não foi possível editar o evento!"
                };
            }
        }

        public async Task<EventResponseViewModel> GetAllAsync(int userId)
        {
            try
            {
                var user = await _db.Users.FirstOrDefaultAsync(x => x.UserId == userId);

                if(user is null)
                {
                    return new EventResponseViewModel
                    {
                        Success = false,
                        Message = "Usuário não encontrado"
                    };
                }

                var eventsData = await _db.Events.Where(x => x.UserId == user.UserId && x.IsDeleted != true)?.Select(u => new CreateEventData(u)).ToListAsync();

                return new EventResponseViewModel
                {
                    Success = true,
                    Events = eventsData
                };

            }
            catch (Exception ex) 
            {
                return new EventResponseViewModel
                {
                    Success = false,
                    Message = "Ocorreu um erro!"
                };
            }
        }
    
        public async Task<EventResponseViewModel> GetAsync(int eventId)
        {
            try
            {
                var ev = await _db.Events.FirstOrDefaultAsync(e => e.EventId == eventId);

                if(ev is null)
                {
                    return new EventResponseViewModel
                    {
                        Success = false,
                        Message = "O evento não existe!"
                    };
                }

                return new EventResponseViewModel
                {
                    Success = true,
                    Events = new List<CreateEventData> { new CreateEventData(ev) }
                };
            }
            catch (Exception ex) 
            {
                return new EventResponseViewModel
                {
                    Success = false,
                    Message = "Ocorreu um erro"
                };
            }
        }

        private static bool IsStartGreaterThanEnd(DateTime start, DateTime end)
        {
            return end <= start;
        }
    }
}
