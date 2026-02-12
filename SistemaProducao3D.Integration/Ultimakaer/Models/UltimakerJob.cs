using System;

namespace SistemaProducao3D.Integration.Ultimaker
{
    public class UltimakerJob
    {
        public string Uuid { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Result { get; set; }

        public DateTime? DatetimeStarted { get; set; }
        public DateTime? DatetimeFinished { get; set; }
        public DateTime? CreatedAt { get; set; }

        public decimal? TimeElapsed { get; set; }

        // ✅ Volume em mm³ (vem da API)
        public decimal? Material0Amount { get; set; }
        public decimal? Material1Amount { get; set; }

        // ✅ NOVO: Informações dos materiais (vêm da API)
        public Guid? Material0Guid { get; set; }
        public string? Material0Name { get; set; }
        public string? Material0Brand { get; set; }

        public Guid? Material1Guid { get; set; }
        public string? Material1Name { get; set; }
        public string? Material1Brand { get; set; }
    }
}