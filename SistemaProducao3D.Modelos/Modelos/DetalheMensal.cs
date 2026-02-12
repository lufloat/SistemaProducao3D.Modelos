using System;

namespace SistemaProducao3D.Modelos.Modelos
{
    /// <summary>
    /// Modelo DetalheMensal - VERSÃO ATUALIZADA
    /// ✅ Compatível com IProducaoService
    /// Campos: Produção, Protótipo, Abortados, Perdidos, % Falhas, % Abortados
    /// </summary>
    public class DetalheMensal
    {
        public int Mes { get; set; }
        public string MesNome { get; set; } = string.Empty;

        // ✅ NOVOS CAMPOS - Quantidades em peças
        public int ProducaoPcs { get; set; }
        public int PrototipoPcs { get; set; }
        public int AbortadosPcs { get; set; }
        public int PerdidosPcs { get; set; }

        // ✅ NOVOS CAMPOS - Percentuais
        public decimal PercentualFalhas { get; set; }
        public decimal PercentualAbortados { get; set; }

        // Total do mês (soma de tudo)
        public int TotalMes => ProducaoPcs + PrototipoPcs + AbortadosPcs + PerdidosPcs;

        // ✅ CAMPOS ANTIGOS MANTIDOS PARA COMPATIBILIDADE (se existirem em outros lugares)
        public string? Periodo { get; set; }  // Formato "2025-01"
        public int Valor { get; set; }        // Valor genérico (pode ser SKUs, por exemplo)

        // Construtor
        public DetalheMensal()
        {
            MesNome = ObterNomeMes(Mes);
            Periodo = $"{DateTime.Now.Year:0000}-{Mes:00}";
        }

        private string ObterNomeMes(int mes)
        {
            return mes switch
            {
                1 => "Janeiro",
                2 => "Fevereiro",
                3 => "Março",
                4 => "Abril",
                5 => "Maio",
                6 => "Junho",
                7 => "Julho",
                8 => "Agosto",
                9 => "Setembro",
                10 => "Outubro",
                11 => "Novembro",
                12 => "Dezembro",
                _ => $"Mês {mes}"
            };
        }
    }
}