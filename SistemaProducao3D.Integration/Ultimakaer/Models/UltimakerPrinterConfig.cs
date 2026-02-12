using System;

namespace SistemaProducao3D.Integration.Ultimaker
{
    /// <summary>
    /// Configuração de impressora Ultimaker
    /// ✅ MODELO EXISTENTE - mantido para compatibilidade
    /// </summary>
    public class UltimakerPrinterConfig
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string? BaseUrl { get; set; }
    }
}
