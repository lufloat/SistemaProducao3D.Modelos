using System.Collections.Generic;

namespace SistemaProducao3D.Integration.Ultimaker
{
    public class UltimakerOptions
    {
        public List<UltimakerPrinterConfig> Printers { get; set; } = new();
    }
}