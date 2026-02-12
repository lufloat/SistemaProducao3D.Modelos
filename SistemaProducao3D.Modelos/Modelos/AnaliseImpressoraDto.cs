public class AnaliseImpressoraDto
{
    public string ImpressoraNome { get; set; } = string.Empty;

    public decimal TaxaProducao { get; set; }
    public decimal TaxaOciosidade { get; set; }
    public decimal TaxaPausas { get; set; }

    public decimal HorasProducao { get; set; }
    public decimal HorasOciosidade { get; set; }
    public decimal HorasPausas { get; set; }

    public int JobsFinalizados { get; set; }
    public int JobsAbortados { get; set; }
}
