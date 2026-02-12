using Business_Logic.Repositories.Interfaces;
using Business_Logic.Serviços.Interfaces;
using SistemaProducao3D.Modelos.Modelos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Business_Logic.Serviços
{
    /// <summary>
    /// DashboardSKUService - VERSÃO CORRIGIDA
    /// ✅ Retorna DetalheMensal ao invés de ProducaoMensalDTO
    /// </summary>
    public class DashboardSKUService : IDashboardSKUService
    {
        private readonly IProducaoRepository _producaoRepository;
        private readonly ICalculoService _calculoService;

        public DashboardSKUService(IProducaoRepository producaoRepository, ICalculoService calculoService)
        {
            _producaoRepository = producaoRepository;
            _calculoService = calculoService;
        }

        public async Task<MetricasKPI> ObterMetricasKPI(int? ano, int mesInicio, int? mesFim)
        {
            int anoAtual = ano ?? DateTime.Now.Year;
            int mesFimAtual = mesFim ?? DateTime.Now.Month;
            int quantidadeMeses = mesFimAtual - mesInicio + 1;

            var dadosAtual = await _producaoRepository.ObterMultiplosMeses(anoAtual, mesInicio, quantidadeMeses);
            var dadosAnterior = await _producaoRepository.ObterMultiplosMeses(anoAtual - 1, mesInicio, quantidadeMeses);

            var skusTotais = dadosAtual
                .Where(d => d.IsSucess && !string.IsNullOrWhiteSpace(d.JobName))
                .Select(d => d.JobName.Trim())
                .Distinct()
                .Count();

            var skusTotaisAnterior = dadosAnterior
                .Where(d => d.IsSucess && !string.IsNullOrWhiteSpace(d.JobName))
                .Select(d => d.JobName.Trim())
                .Distinct()
                .Count();

            var producao = dadosAtual.Count(d => d.IsSucess && d.JobType == "Producao");
            var producaoAnterior = dadosAnterior.Count(d => d.IsSucess && d.JobType == "Producao");

            var prototipos = dadosAtual.Count(d => d.IsSucess && d.JobType == "Prototipo");
            var prototiposAnterior = dadosAnterior.Count(d => d.IsSucess && d.JobType == "Prototipo");

            var pecas = dadosAtual.Count(d => d.IsSucess && d.JobType == "Pecas");
            var ferramentas = dadosAtual.Count(d => d.IsSucess && d.JobType == "FerramentasDiversos");

            var taxaSucesso = _calculoService.TaxaSucesso(dadosAtual);
            var taxaSucessoAnterior = _calculoService.TaxaSucesso(dadosAnterior);

            return new MetricasKPI
            {
                SKusTotais = skusTotais,
                MetaSkus = 450,
                ProgressoSkus = skusTotais > 0 ? (decimal)skusTotais / 450 * 100 : 0,
                VariacaoSkus = _calculoService.CalcularVariacao(skusTotais, skusTotaisAnterior),

                Producao = producao,
                VariacaoProducao = _calculoService.CalcularVariacao(producao, producaoAnterior),

                Prototipos = prototipos,
                VariacaoPrototipos = _calculoService.CalcularVariacao(prototipos, prototiposAnterior),

                Pecas = pecas,
                FerramentasDiversos = ferramentas,

                TaxaSucesso = taxaSucesso,
                VariacaoTaxaSucesso = _calculoService.CalcularVariacao((int)taxaSucesso, (int)taxaSucessoAnterior),

                NovosSkus = dadosAtual
                    .Where(d => d.IsSucess && !string.IsNullOrWhiteSpace(d.JobName))
                    .Select(d => d.JobName.Trim())
                    .Distinct()
                    .Except(
                        dadosAnterior
                            .Where(d => d.IsSucess && !string.IsNullOrWhiteSpace(d.JobName))
                            .Select(d => d.JobName.Trim())
                            .Distinct()
                    )
                    .Count()
            };
        }

        /// <summary>
        /// ✅ CORRIGIDO: Retorna List<DetalheMensal> ao invés de List<ProducaoMensalDTO>
        /// </summary>
        public async Task<List<DetalheMensal>> ObterEvolucaoSKUs(int? anoInicio, int mesInicio, int? anoFim, int mesFim)
        {
            var resultado = new List<DetalheMensal>();

            int anoInicioAtual = anoInicio ?? DateTime.Now.Year;
            int anoFimAtual = anoFim ?? DateTime.Now.Year;

            var dataInicio = new DateTime(anoInicioAtual, mesInicio, 1);
            var dataFim = new DateTime(anoFimAtual, mesFim, 1);
            var dataAtual = dataInicio;

            while (dataAtual <= dataFim)
            {
                var dados = await _producaoRepository.ObterPorPeriodo(dataAtual.Month, dataAtual.Year);

                var skusTotais = dados
                    .Where(d => d.IsSucess && !string.IsNullOrWhiteSpace(d.JobName))
                    .Select(d => d.JobName.Trim())
                    .Distinct()
                    .Count();

                resultado.Add(new DetalheMensal
                {
                    Mes = dataAtual.Month,
                    MesNome = ObterNomeMes(dataAtual.Month),
                    Periodo = dataAtual.ToString("yyyy-MM"),
                    Valor = skusTotais
                });

                dataAtual = dataAtual.AddMonths(1);
            }

            return resultado;
        }

        /// <summary>
        /// Método auxiliar para obter nome do mês
        /// </summary>
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