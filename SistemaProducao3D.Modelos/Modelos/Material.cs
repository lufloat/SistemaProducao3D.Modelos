using System;

namespace SistemaProducao3D.Modelos.Modelos
{
    public class Material
    {
        public Guid Id { get; set; }
        public Guid UltimakerMaterialGuid { get; set; }
        public string Nome { get; set; } = string.Empty;
        public decimal Densidade { get; set; } // g/cm³
        public string Fabricante { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Converte volume em mm³ para peso em gramas
        /// Fórmula: peso_g = (volume_mm3 / 1000) * densidade
        /// </summary>
        public decimal ConverterVolumeMm3ParaGramas(decimal volumeMm3)
        {
            if (volumeMm3 <= 0 || Densidade <= 0)
                return 0;

            // mm³ → cm³ → gramas
            decimal volumeCm3 = volumeMm3 / 1000m;
            decimal pesoGramas = volumeCm3 * Densidade;

            return Math.Round(pesoGramas, 2);
        }
    }
}