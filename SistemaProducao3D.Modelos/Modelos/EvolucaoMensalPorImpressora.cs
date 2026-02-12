namespace SistemaProducao3D.Modelos.Modelos
{
    public class EvolucaoMensalPorImpressora
    {
        public int MachineId { get; set; }
        public string NomeImpressora { get; set; } = string.Empty;
        public string Periodo { get; set; } = string.Empty;
        public int Valor { get; set; }
    }
}