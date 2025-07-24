namespace ContainRs.Vendas.Propostas
{
    public class ComentarProposta
    {
        public Guid IdPedido { get;}
        public Guid IdProposta { get; }
        public string Mensagem { get; }
        public string Pessoa { get; }

        public ComentarProposta(Guid idPedido, Guid idProposta, string mensagem, string pessoa)
        {
            IdPedido = idPedido;
            IdProposta = idProposta;
            Mensagem = mensagem;
            Pessoa = pessoa;
        }
    }
}
