public class CardKgPorImpressora
{
    public string MachineId { get; set; }
    public string NomeImpressora { get; set; }
    public decimal ProducaoKg { get; set; }
    public decimal PrototipoKg { get; set; }
    public decimal ErrosKg { get; set; }
    public decimal FailedKg { get; set; }      // ⭐ NOVO
    public decimal AbortedKg { get; set; }     // ⭐ NOVO
    public decimal TotalKg { get; set; }
}