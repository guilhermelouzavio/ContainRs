
using ContainRs.Api.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text.Json;

namespace ContainRs.Api.Eventos
{
    public class EventoManager : IEventoManager
    {
        public readonly AppDbContext appDbContext;

        public EventoManager(AppDbContext appDbContext)
        {
            this.appDbContext = appDbContext;
        }

        public async Task MarcarComoLidaAsync(Guid idEvento, string tipoLeitor)
        {
            var outbox = await appDbContext.OutBox.FirstOrDefaultAsync(o => o.Id == idEvento);
            if (outbox is null) return;

            var inbox = new InboxMessage
            {
                Id = Guid.NewGuid(),
                OutboxMessageId = outbox.Id,
                Evento = outbox,
                TipoLeitor = tipoLeitor,
                DataProcessamento = DateTime.Now
            };

            await appDbContext.Inbox.AddAsync(inbox);
            await appDbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<Mensagem<T>>> RecuperarNaoLidasAsync<T>(string tipoEvento, string tipoLeitor)
        {
            var mensagens = await appDbContext.OutBox
                .FromSqlRaw(@"
                SELECT o.* 
                FROM Outbox o 
                    LEFT JOIN Inbox i on i.OutboxMessageId = o.Id 
                        AND i.TipoLeitor = {0}
                WHERE i.Id IS NULL AND o.TipoEvento = {1}", tipoLeitor, tipoEvento
                )
                .ToListAsync();

            var saida = mensagens.Select(outbox =>
                            {
                                var obj = JsonConvert.DeserializeObject<T>(outbox.InfoEvento);
                                return new Mensagem<T>() { Id = outbox.Id, Corpo = obj!};
                            }).ToList();
            return saida;
        }
    }
}
