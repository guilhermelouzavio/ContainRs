using ContainRs.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContainRs.Financeiro.Faturamento
{
    public class EmissorDeFaturas
    {

        private readonly IRepository<Fatura> _repository;
        private readonly IAcessoProposta _acessoLocacao;
        private readonly IEventoManager eventoManager;

        public EmissorDeFaturas(IRepository<Fatura> repository, IAcessoProposta acessoLocacao, IEventoManager eventoManager)
        {
            _repository = repository;
            _acessoLocacao = acessoLocacao;
            this.eventoManager = eventoManager;
        }

        public async Task ExecutarAsync()
        {
            var mensagens = await eventoManager.RecuperarNaoLidasAsync<PropostaAprovada>("PropostaAprovada", nameof(EmissorDeFaturas));

            foreach (var mensagem in mensagens)
            {
                Fatura fatura = new()
                {
                    Id = Guid.NewGuid(),
                    DataEmissao = DateTime.Now,
                    DataVencimento = DateTime.Now.AddDays(5),
                    Numero = "1546546",
                    Total = mensagem.Corpo.ValorProposta,
                    LocacaoId = await _acessoLocacao.BuscarIdLocacaoPorIdProposta(mensagem.Corpo.IdProposta) //CAMADA ANTI CORRUPÇÂO

                };

                //persistir a fatura
                await _repository.AddAsync(fatura);

                //marcar mensagem como lida
                await eventoManager.MarcarComoLidaAsync(mensagem.Id, nameof(EmissorDeFaturas));
            }

        }
               
    }
}
