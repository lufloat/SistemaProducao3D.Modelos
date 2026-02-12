using SistemaProducao3D.Modelos.Modelos;
using System.Collections.Generic;

namespace Business_Logic.Serviços.Interfaces
{
    public interface ICalculoService
    {
        decimal MaterialUsado(List<MesaProducao> dados);
        decimal TempoImpressao(List<MesaProducao> dados);
        int QuantidadeMesas(List<MesaProducao> dados);
        decimal TaxaSucesso(List<MesaProducao> dados);
        decimal MaterialPerdido(List<MesaProducao> dados);
        decimal CalcularVariacao(int atual, int anterior);
    }
}