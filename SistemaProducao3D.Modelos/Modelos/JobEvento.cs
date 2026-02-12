public class JobEvento
{
    public int Id { get; set; }
    public string ImpressoraNome { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime Inicio { get; set; }
    public DateTime Fim { get; set; }
}