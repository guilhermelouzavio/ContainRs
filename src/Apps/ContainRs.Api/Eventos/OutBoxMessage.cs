namespace ContainRs.Api.Eventos
{
    /// <summary>
    /// A classe OutboxMessage representa uma mensagem que foi enviada, mas ainda não foi processada por um leitor. Ela geralmente contém informações sobre o evento que ocorreu, como o tipo de evento e os dados associados a ele. Essa classe é utilizada para armazenar mensagens que estão aguardando para serem lidas ou processadas.
    /// </summary>
    public class OutBoxMessage
    {
        public Guid Id { get; set; }
        public required string TipoEvento { get; set; }
        public required string InfoEvento { get; set; }
        public DateTime DataCriacao { get; set; }
    }
}
