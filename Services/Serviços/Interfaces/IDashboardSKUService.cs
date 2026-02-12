using SistemaProducao3D.Modelos.Modelos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Business_Logic.Serviços.Interfaces
{
    public interface IDashboardSKUService
    {
        Task<MetricasKPI> ObterMetricasKPI(int? ano, int mesInicio, int? mesFim);
        Task<List<DetalheMensal>> ObterEvolucaoSKUs(int? anoInicio, int mesInicio, int? anoFim, int mesFim);
    }
}