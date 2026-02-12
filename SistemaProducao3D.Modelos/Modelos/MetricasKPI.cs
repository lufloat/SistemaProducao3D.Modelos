namespace SistemaProducao3D.Modelos.Modelos
{
    public class MetricasKPI
    {
        public int SKusTotais   { get; set; }
        public int MetaSkus    { get; set; }
        public decimal ProgressoSkus { get; set; }
        
        public decimal VariacaoSkus { get; set; }
        public decimal Producao { get; set; }

        public decimal VariacaoProducao { get; set; }
        public decimal TaxaSucesso { get; set; }

        public decimal VariacaoTaxaSucesso { get; set; }

        public int Prototipos { get; set; }
        public decimal VariacaoPrototipos { get; set; }

        public int Pecas { get; set; }
        public int FerramentasDiversos { get; set; }
        public int NovosSkus { get; set; }
    }
}
