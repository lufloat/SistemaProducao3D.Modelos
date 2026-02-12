using Business_Logic.Serviços.Interfaces;
using SistemaProducao3D.Modelos.Modelos;
using System.Collections.Generic;
using System.Linq;

namespace Business_Logic.Serviços
{
    public class CalculoService : ICalculoService
    {
        // ✅ Retorna material em GRAMAS (MaterialTotal já está em gramas)
        public decimal MaterialUsado(List<MesaProducao> dados)
        {
            return dados.Sum(d => d.MaterialTotal);
        }

        // ✅ Retorna tempo em SEGUNDOS (PrintTime já está em segundos)
        public decimal TempoImpressao(List<MesaProducao> dados)
        {
            return dados.Sum(d => d.PrintTime);
        }

        public int QuantidadeMesas(List<MesaProducao> dados)
        {
            return dados.Select(d => d.MesaId).Distinct().Count();
        }

        public decimal TaxaSucesso(List<MesaProducao> dados)
        {
            if (!dados.Any()) return 0;
            var sucesso = dados.Count(d => d.IsSucess);
            return (decimal)sucesso / dados.Count * 100;
        }

        // ✅ Material perdido APENAS em falhas técnicas (failed)
        public decimal MaterialPerdido(List<MesaProducao> dados)
        {
            return dados
                .Where(d => d.IsFailed)
                .Sum(d => d.MaterialTotal);
        }

        // ✅ NOVO: Material perdido em jobs abortados
        public decimal MaterialAbortado(List<MesaProducao> dados)
        {
            return dados
                .Where(d => d.IsAborted)
                .Sum(d => d.MaterialTotal);
        }

        public decimal CalcularVariacao(int atual, int anterior)
        {
            if (anterior == 0) return 0;
            return ((decimal)(atual - anterior) / anterior) * 100;
        }
    }
}