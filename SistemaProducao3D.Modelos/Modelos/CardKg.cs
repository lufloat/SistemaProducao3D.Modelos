public class CardKg
{
    public string MesAno { get; set; }
    public decimal ProducaoKg { get; set; }
    public decimal PrototipoKg { get; set; }
    public decimal ErrosKg { get; set; }
    public decimal FailedKg { get; set; }      // ⭐ NOVO - Erros técnicos
    public decimal AbortedKg { get; set; }     // ⭐ NOVO - Jobs cancelados
    public decimal TotalKg { get; set; }
}