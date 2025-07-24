using ContainRs.Vendas.Propostas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContainRs.Vendas.Locacoes
{
    public interface ICalculadoraPrazosLocacao
    {
        DateTime CalculadoraDataPrevistaParaEntrega(Proposta proposta);
        DateTime CalculadoraDataPrevistaParaTermino(Proposta proposta);
    }

    public class CalculadoraPadraoPrazosLocacao : ICalculadoraPrazosLocacao
    {
        public DateTime CalculadoraDataPrevistaParaEntrega(Proposta proposta)
        {
            return proposta.Solicitacao
                .DataInicioOperacao
                .AddDays(-proposta.Solicitacao.DisponibilidadePrevia);
        }

        public DateTime CalculadoraDataPrevistaParaTermino(Proposta proposta)
        {
            return proposta.Solicitacao.DataInicioOperacao
               .AddDays(proposta.Solicitacao.DisponibilidadePrevia);
        }
    }

}
