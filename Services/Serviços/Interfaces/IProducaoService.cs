using SistemaProducao3D.Modelos.Modelos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Business_Logic.Serviços.Interfaces
{
    public interface IProducaoService
    {
        Task<List<ProducaoAnual>> ObterProducaoAnual(int anoInicio, int anoFim);
        Task<List<DetalheMensal>> ObterProducaoMensalDetalhada(int ano, int mesInicio, int mesFim);
    }
}