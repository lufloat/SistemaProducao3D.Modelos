using System;

namespace SistemaProducao3D.Modelos.Modelos
{
    /// <summary>
    /// Modelo ProducaoAnual
    /// Representa produção agregada por ano
    /// </summary>
    public class ProducaoAnual
    {
        public int Ano { get; set; }
        public int ProducaoPcs { get; set; }
        public int PrototipoPcs { get; set; }
        public int AbortadosPcs { get; set; }
        public int PerdidosPcs { get; set; }
        public decimal PercentualFalhas { get; set; }
        public decimal PercentualAbortados { get; set; }
        public int TotalAno => ProducaoPcs + PrototipoPcs + AbortadosPcs + PerdidosPcs;
    }
}