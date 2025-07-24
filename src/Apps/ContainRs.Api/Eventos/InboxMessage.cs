using ContainRs.Api.Data.Migrations;
using ContainRs.Contracts;
using System.Diagnostics;
using System.Runtime.Intrinsics.X86;

namespace ContainRs.Api.Eventos
{
    /// <summary>
    /// InboxMessage Por outro lado, a classe InboxMessage representa uma mensagem que já foi processada.Quando uma mensagem é marcada como lida, ela é movida da tabela de Outbox para a tabela de Inbox, onde fica registrada com informações adicionais, como o tipo de leitor que processou a mensagem e a data em que foi processada.
    /// </summary>
    public class InboxMessage
    {
        public Guid Id { get; set; }
        public required string TipoLeitor { get; set; }
        public Guid OutboxMessageId { get; set; }
        public required OutBoxMessage Evento { get; set; }
        public DateTime DataProcessamento { get; set; }
        public string? Erro { get; set; }
    }
}
