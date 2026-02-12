using System;
using System.Collections.Generic;

namespace SistemaProducao3D.Integration.Ultimaker
{
    /// <summary>
    /// Representa um evento capturado da API /api/v1/history/events
    /// </summary>
    public class UltimakerEvent
    {
        public string Message { get; set; } = string.Empty;
        public List<string> Parameters { get; set; } = new();
        public DateTime Time { get; set; }
        public int TypeId { get; set; }

        // ✅ Propriedades calculadas para facilitar análise
        public bool IsPrintStarted => TypeId == 131072;
        public bool IsPrintPaused => TypeId == 131073;   // ✅ ADICIONADO
        public bool IsPrintResumed => TypeId == 131074;  // ✅ ADICIONADO
        public bool IsPrintAborted => TypeId == 131075;
        public bool IsPrintFinished => TypeId == 131076;
        public bool IsPrintCleared => TypeId == 131077;
        public bool IsSystemStarted => TypeId == 1;
        public bool IsMaterialChanged => TypeId == 65537;
        public bool IsHotendChanged => TypeId == 65536;

        /// <summary>
        /// Extrai UUID do job do evento (quando aplicável)
        /// </summary>
        public string? GetJobUuid()
        {
            if (Parameters != null && Parameters.Count > 0)
            {
                // Para eventos de print, o primeiro parâmetro geralmente é o UUID
                if (IsPrintStarted || IsPrintPaused || IsPrintResumed ||
                    IsPrintFinished || IsPrintAborted || IsPrintCleared)
                {
                    return Parameters[0];
                }
            }
            return null;
        }

        /// <summary>
        /// Extrai nome do arquivo do evento de início de impressão
        /// </summary>
        public string? GetJobName()
        {
            if (IsPrintStarted && Parameters != null && Parameters.Count > 1)
            {
                return Parameters[1];
            }
            return null;
        }

        /// <summary>
        /// Determina se é um evento relacionado a parada/interrupção
        /// </summary>
        public bool IsInterruptionEvent =>
            IsPrintAborted ||
            IsPrintPaused ||
            Message.Contains("paused", StringComparison.OrdinalIgnoreCase) ||
            Message.Contains("stopped", StringComparison.OrdinalIgnoreCase) ||
            Message.Contains("error", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Obtém categoria do evento para análise
        /// </summary>
        public string EventCategory
        {
            get
            {
                if (IsPrintStarted) return "PRINT_START";
                if (IsPrintPaused) return "Paused";      // ✅ ADICIONADO
                if (IsPrintResumed) return "Resumed";    // ✅ ADICIONADO
                if (IsPrintFinished) return "PRINT_FINISH";
                if (IsPrintAborted) return "PRINT_ABORT";
                if (IsPrintCleared) return "PRINT_CLEAR";
                if (IsMaterialChanged) return "MATERIAL_CHANGE";
                if (IsHotendChanged) return "HOTEND_CHANGE";
                if (IsSystemStarted) return "SYSTEM_START";
                return "OTHER";
            }
        }
    }

    /// <summary>
    /// Análise de eventos agregados para calcular métricas
    /// </summary>
    public class EventAnalysis
    {
        public DateTime PeriodoInicio { get; set; }
        public DateTime PeriodoFim { get; set; }
        public int MachineId { get; set; }
        public string MachineName { get; set; } = string.Empty;

        // Métricas principais
        public int TotalEventos { get; set; }
        public int JobsIniciados { get; set; }
        public int JobsFinalizados { get; set; }
        public int JobsAbortados { get; set; }

        // Tempos em minutos
        public decimal TempoTotalPeriodo { get; set; }
        public decimal TempoOciosidade { get; set; }
        public decimal TempoPausas { get; set; }
        public decimal TempoProducao { get; set; }

        // Taxas percentuais
        public decimal TaxaOciosidade => TempoTotalPeriodo > 0
            ? Math.Round((TempoOciosidade / TempoTotalPeriodo) * 100, 2)
            : 0;

        public decimal TaxaPausas => TempoTotalPeriodo > 0
            ? Math.Round((TempoPausas / TempoTotalPeriodo) * 100, 2)
            : 0;

        public decimal TaxaProducao => TempoTotalPeriodo > 0
            ? Math.Round((TempoProducao / TempoTotalPeriodo) * 100, 2)
            : 0;

        // Detalhes das pausas
        public List<PauseDetail> Pausas { get; set; } = new();
        public List<IdleDetail> Ociosidades { get; set; } = new();
    }

    /// <summary>
    /// Detalhe de uma pausa durante impressão
    /// </summary>
    public class PauseDetail
    {
        public string JobUuid { get; set; } = string.Empty;
        public string JobName { get; set; } = string.Empty;
        public DateTime InicioParada { get; set; }
        public DateTime? FimParada { get; set; }
        public decimal DuracaoMinutos { get; set; }
        public string Motivo { get; set; } = string.Empty;
        public string TipoEvento { get; set; } = string.Empty; // "Paused-Resumed", "Paused-Aborted"
    }

    /// <summary>
    /// Detalhe de um período de ociosidade
    /// </summary>
    public class IdleDetail
    {
        public DateTime Inicio { get; set; }
        public DateTime Fim { get; set; }
        public decimal DuracaoMinutos { get; set; }
        public string? UltimoJobUuid { get; set; }
        public string? ProximoJobUuid { get; set; }
        public string Contexto { get; set; } = string.Empty; // ex: "Entre jobs", "Após limpeza"
    }
}