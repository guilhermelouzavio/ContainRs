using ContainRs.Api.Eventos;
using ContainRs.DDD;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text.Json;

namespace ContainRs.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<PedidoLocacao> Pedidos { get; set; }
    public DbSet<Proposta> Propostas { get; set; }
    public DbSet<Locacao> Locacoes { get; set; }
    public DbSet<Conteiner> Conteineres { get; set; }
    public DbSet<Fatura> Faturas { get; set; }
    public DbSet<OutBoxMessage> OutBox { get; set; }

    public DbSet<InboxMessage> Inbox { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        //pega todos os eventos criados
        var domainEvents = ChangeTracker
            .Entries<IAgreggateRoot>()
            .Select(entry => entry.Entity)
            .SelectMany(entity =>
            {
                var events = entity.Events.ToList();
                entity.RemoverEventos();
                return events;
            })
            .ToList();

        //tranforma no objeto de outBox para salvar na base de dados
        var outboxMessages = domainEvents
         .Select(@event => new OutBoxMessage 
         { 
            Id = Guid.NewGuid(),
            TipoEvento = @event.GetType().Name,
            InfoEvento = JsonConvert.SerializeObject(@event),
            DataCriacao = DateTime.Now,
         
         }).ToList();

        OutBox.AddRange(outboxMessages);    

        return base.SaveChangesAsync(cancellationToken);
    }
}
