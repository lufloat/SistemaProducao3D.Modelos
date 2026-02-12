using SistemaProducao3D.Modelos.Modelos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Business_Logic.Repositories.Interfaces
{
    public interface IMaterialRepository
    {
        Task<Material?> ObterPorGuid(Guid materialGuid);
        Task<Material> ObterOuCriarMaterial(Guid materialGuid, string nomeMaterial, string fabricante);
        Task<List<Material>> ListarTodos();
    }
}