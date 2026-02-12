using System.Collections.Generic;
using System.Threading.Tasks;

namespace Business_Logic.Serviços.Interfaces
{
    public interface IVisaoGeralService
    {
        // ==========================================
        // CONSOLIDADO MENSAL (SEM DISTINÇÃO DE IMPRESSORA)
        // ==========================================
        Task<List<object>> ObterProducaoMensal(int ano, int mesInicio, int mesFim);
        Task<List<object>> ObterPrototipoMensal(int ano, int mesInicio, int mesFim);
        Task<List<object>> ObterErrosMensais(int ano, int mesInicio, int mesFim);
        Task<List<object>> ObterPesoMensal(int ano, int mesInicio, int mesFim);

        // ✅ NOVOS MÉTODOS - Failed e Aborted separados
        Task<List<object>> ObterFailedMensais(int ano, int mesInicio, int mesFim);
        Task<List<object>> ObterAbortedMensais(int ano, int mesInicio, int mesFim);

        // ==========================================
        // POR IMPRESSORA - ANO COMPLETO
        // ==========================================
        Task<List<object>> ObterProducaoPorImpressoraAnual(int ano, int mesInicio, int mesFim);
        Task<List<object>> ObterPrototiposPorImpressoraAnual(int ano, int mesInicio, int mesFim);
        Task<List<object>> ObterErrosPorImpressoraAnual(int ano, int mesInicio, int mesFim);
        Task<List<object>> ObterPesoPorImpressoraAnual(int ano, int mesInicio, int mesFim);

        // ✅ NOVOS MÉTODOS - Failed e Aborted por impressora
        Task<List<object>> ObterFailedPorImpressoraAnual(int ano, int mesInicio, int mesFim);
        Task<List<object>> ObterAbortedPorImpressoraAnual(int ano, int mesInicio, int mesFim);
    }
}