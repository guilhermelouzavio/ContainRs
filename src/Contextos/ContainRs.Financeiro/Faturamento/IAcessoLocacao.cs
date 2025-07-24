using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContainRs.Financeiro.Faturamento
{
    public interface IAcessoProposta
    {
        Task<Guid> BuscarIdLocacaoPorIdProposta(Guid idProposta);
    }
}
