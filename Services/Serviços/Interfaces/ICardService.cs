using SistemaProducao3D.Modelos.Modelos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Business_Logic.Serviços.Interfaces
{
    public interface ICardService
    {
        Task<List<CardKg>> ObterCardsKg(int ano, int mesInicio, int mesFim);
        Task<List<CardCapacidade>> ObterCardsCapacidade(int ano, int mesInicio, int mesFim, int numeroMaquinas);

        // ✅ NOVOS: Métricas por impressora
        Task<List<CardCapacidadePorImpressora>> ObterCapacidadePorImpressora(int ano, int mes);
        Task<List<CardKgPorImpressora>> ObterKgPorImpressora(int ano, int mes);
    }
}