using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SistemaProducao3D.Integration.Ultimaker
{
    /// <summary>
    /// Interface para cliente da API Ultimaker
    /// ✅ VERSÃO COMPATÍVEL - mantém métodos existentes + adiciona novos
    /// </summary>
    public interface IUltimakerClient
    {
        // ========================================
        // MÉTODOS EXISTENTES (NÃO ALTERADOS)
        // ========================================

        /// <summary>
        /// Obtém lista de impressoras configuradas
        /// </summary>
        Task<List<UltimakerPrinterConfig>> GetPrintersAsync();

        /// <summary>
        /// Obtém jobs de uma impressora em um período
        /// </summary>
        Task<List<UltimakerJob>> GetJobsAsync(int printerId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Obtém job específico por UUID
        /// </summary>
        Task<UltimakerJob?> GetJobByUuidAsync(string uuid);

        // ========================================
        // ✅ NOVOS MÉTODOS - ANÁLISE DE EVENTOS
        // ========================================

        /// <summary>
        /// Obtém todos os eventos de uma impressora em um período
        /// </summary>
        Task<List<UltimakerEvent>> GetEventsAsync(int printerId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Obtém eventos de um job específico pelo UUID
        /// </summary>
        Task<List<UltimakerEvent>> GetEventsByJobUuidAsync(int printerId, string jobUuid);

        /// <summary>
        /// Analisa eventos e calcula métricas de ociosidade e pausas
        /// </summary>
        Task<EventAnalysis> AnalyzeEventsAsync(int printerId, DateTime startDate, DateTime endDate);
    }
}
