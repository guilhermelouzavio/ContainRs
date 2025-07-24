using ContainRs.Contracts;
using ContainRs.Financeiro.Faturamento;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContainRs.Vendas.Propostas
{
    public class AcessoPropostaPorFinanceiro : IAcessoProposta
    {
        private readonly IRepository<Proposta> _repository;

        public AcessoPropostaPorFinanceiro(IRepository<Proposta> repository)
        {
            _repository = repository;
        }

        public async Task<Guid> BuscarIdLocacaoPorIdProposta(Guid idProposta)
        {
            var proposta = await _repository
                .GetFirstAsync(c => c.Id == idProposta, c => c.Id);

            var idLocacao = proposta?.Solicitacao.Id;

            return idLocacao == null ? throw new Exception("fsdfdsfs") : (Guid)idLocacao;
        }
    }
}
