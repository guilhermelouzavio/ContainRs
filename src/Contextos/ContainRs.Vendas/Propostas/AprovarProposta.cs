using ContainRs.Contracts;
using ContainRs.Vendas.Locacoes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace ContainRs.Vendas.Propostas
{
    public class AprovarProposta
    {
        public Guid IdPedido { get; set; }
        public Guid IdProposta { get; set; }

        public AprovarProposta(Guid idPedido, Guid idProposta)
        {
            IdPedido = idPedido;
            IdProposta = idProposta;
        }
    }
}
