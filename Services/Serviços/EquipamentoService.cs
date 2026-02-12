using Business_Logic.Serviços.Interfaces;
using SistemaProducao3D.Integration.Ultimaker;
using SistemaProducao3D.Modelos.Modelos;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Business_Logic.Serviços
{
    public class EquipamentoService : IEquipamentoService
    {
        private readonly IUltimakerClient _ultimakerClient;

        public EquipamentoService(IUltimakerClient ultimakerClient)
        {
            _ultimakerClient = ultimakerClient;
        }

        public async Task<List<Equipamento>> ObterEquipamentos()
        {
            var printers = await _ultimakerClient.GetPrintersAsync();

            return printers
                .GroupBy(p => "Ultimaker S5")
                .Select(g => new Equipamento
                {
                    Nome = g.Key,
                    UnidadesAtivas = g.Count(p => p.IsActive)
                })
                .ToList();
        }
    }
}