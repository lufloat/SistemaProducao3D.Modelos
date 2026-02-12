using SistemaProducao3D.Modelos.Modelos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Business_Logic.Serviços.Interfaces
{
    public interface IEquipamentoService
    {
        Task<List<Equipamento>> ObterEquipamentos();
    }
}