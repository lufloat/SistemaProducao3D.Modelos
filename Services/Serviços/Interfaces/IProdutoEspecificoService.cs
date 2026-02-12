using SistemaProducao3D.Modelos.Modelos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Business_Logic.Serviços.Interfaces
{
    public interface IProdutoEspecificoService
    {
        Task<List<DetalheMensal>> ObterRecondicionados(int ano);
        Task<List<DetalheMensal>> ObterProducaoPlacas(int ano);
    }
}