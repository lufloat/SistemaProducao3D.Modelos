// ========================================
// IProducaoRepository.cs
// Localização: Business_Logic/Repositories/Interfaces/IProducaoRepository.cs
// ========================================
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SistemaProducao3D.Modelos.Modelos;

namespace Business_Logic.Repositories.Interfaces
{
    public interface IProducaoRepository
    {
        Task InserirAsync(MesaProducao producao);
        Task AtualizarAsync(MesaProducao producao);
        Task<MesaProducao?> ObterPorUuid(string uuid);
        Task<List<MesaProducao>> ObterPorPeriodo(int? mes, int? ano);
        Task<List<MesaProducao>> ObterPorIntervalo(DateTime inicio, DateTime fim);
        Task<List<MesaProducao>> ObterMultiplosMeses(int anoInicio, int mesInicio, int quantidadeMeses);
        Task<List<MesaProducao>> ObterPorPeriodoEImpressora(int? mes, int? ano, int machineId);

        // ========================================
        // MÉTODOS PARA TIMELINE
        // ========================================

        /// <summary>
        /// Obtém todos os jobs de uma máquina em uma data específica
        /// </summary>
        Task<List<MesaProducao>> ObterJobsPorMaquinaEData(int machineId, DateTime data);

        /// <summary>
        /// Obtém todos os jobs de uma máquina em um período
        /// </summary>
        Task<List<MesaProducao>> ObterJobsPorMaquinaEPeriodo(
            int machineId,
            DateTime dataInicio,
            DateTime dataFim);
    }
}