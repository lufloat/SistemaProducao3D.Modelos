using Business_Logic.Serviços.Interfaces;
using Business_Logic.Repositories.Interfaces;
using SistemaProducao3D.Integration.Ultimaker;
using SistemaProducao3D.Modelos.Modelos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Business_Logic.Serviços
{
    /// <summary>
    /// ProducaoService - VERSÃO COM BANCO DE DADOS
    /// ✅ USA DADOS JÁ SINCRONIZADOS NO BANCO
    /// ✅ NÃO FAZ CHAMADAS HTTP DESNECESSÁRIAS
    /// </summary>
    public class ProducaoService : IProducaoService
    {
        private readonly IProducaoRepository _producaoRepository;
        private readonly IUltimakerClient _ultimakerClient;

        public ProducaoService(
            IProducaoRepository producaoRepository,
            IUltimakerClient ultimakerClient)
        {
            _producaoRepository = producaoRepository;
            _ultimakerClient = ultimakerClient;
        }

        /// <summary>
        /// ✅ OTIMIZADO: Busca produção mensal DO BANCO DE DADOS
        /// Apenas sincroniza se necessário
        /// </summary>
        public async Task<List<DetalheMensal>> ObterProducaoMensalDetalhada(
            int ano,
            int mesInicio,
            int mesFim)
        {
            Console.WriteLine($"\n📊 Buscando produção mensal DO BANCO: {ano}/{mesInicio}-{mesFim}");
            var inicio = DateTime.Now;

            var resultado = new List<DetalheMensal>();

            for (int mes = mesInicio; mes <= mesFim; mes++)
            {
                // 🔥 BUSCAR DO BANCO, NÃO DA API
                var dadosMes = await _producaoRepository.ObterPorPeriodo(mes, ano);

                var detalheMensal = new DetalheMensal
                {
                    Mes = mes,
                    MesNome = ObterNomeMes(mes),
                    Periodo = $"{ano:0000}-{mes:00}"
                };

                // ✅ CLASSIFICAR JOBS DO BANCO
                int totalProducao = 0;
                int totalPrototipo = 0;
                int totalAbortados = 0;
                int totalPerdidos = 0;

                foreach (var job in dadosMes)
                {
                    var nomeJob = job.JobName?.ToLower() ?? "";
                    bool isPrototipo = nomeJob.Contains("proto") ||
                                      nomeJob.Contains("test") ||
                                      nomeJob.Contains("sample");

                    // Verificar status
                    if (job.Status == "finished" && job.IsSucess)
                    {
                        if (isPrototipo)
                            totalPrototipo++;
                        else
                            totalProducao++;
                    }
                    else if (job.Status == "aborted")
                    {
                        totalAbortados++;
                    }
                    else if (job.Status == "failed" || !job.IsSucess)
                    {
                        totalPerdidos++;
                    }
                }

                // Atribuir valores
                detalheMensal.ProducaoPcs = totalProducao;
                detalheMensal.PrototipoPcs = totalPrototipo;
                detalheMensal.AbortadosPcs = totalAbortados;
                detalheMensal.PerdidosPcs = totalPerdidos;

                // ✅ CALCULAR PERCENTUAIS
                int totalJobs = detalheMensal.TotalMes;

                if (totalJobs > 0)
                {
                    detalheMensal.PercentualFalhas = Math.Round((decimal)totalPerdidos / totalJobs * 100, 2);
                    detalheMensal.PercentualAbortados = Math.Round((decimal)totalAbortados / totalJobs * 100, 2);
                }
                else
                {
                    detalheMensal.PercentualFalhas = 0;
                    detalheMensal.PercentualAbortados = 0;
                }

                detalheMensal.Valor = totalJobs;

                resultado.Add(detalheMensal);

                Console.WriteLine($"   ✅ {detalheMensal.MesNome}: Prod={totalProducao}, Proto={totalPrototipo}, " +
                                 $"Abort={totalAbortados}, Perd={totalPerdidos}, Total={totalJobs}");
            }

            var duracao = (DateTime.Now - inicio).TotalSeconds;
            Console.WriteLine($"✅ Processamento concluído em {duracao:F2}s");

            return resultado;
        }

        /// <summary>
        /// ✅ OTIMIZADO: Produção anual DO BANCO
        /// </summary>
        public async Task<List<ProducaoAnual>> ObterProducaoAnual(int anoInicio, int anoFim)
        {
            Console.WriteLine($"\n📊 Buscando produção anual DO BANCO: {anoInicio}-{anoFim}");
            var inicio = DateTime.Now;

            var resultado = new List<ProducaoAnual>();

            for (int ano = anoInicio; ano <= anoFim; ano++)
            {
                // Buscar todos os meses do ano
                var mesesDoAno = await ObterProducaoMensalDetalhada(ano, 1, 12);

                var producaoAnual = new ProducaoAnual
                {
                    Ano = ano,
                    ProducaoPcs = mesesDoAno.Sum(m => m.ProducaoPcs),
                    PrototipoPcs = mesesDoAno.Sum(m => m.PrototipoPcs),
                    AbortadosPcs = mesesDoAno.Sum(m => m.AbortadosPcs),
                    PerdidosPcs = mesesDoAno.Sum(m => m.PerdidosPcs)
                };

                // Calcular percentuais do ano
                int totalAno = producaoAnual.TotalAno;

                if (totalAno > 0)
                {
                    producaoAnual.PercentualFalhas = Math.Round((decimal)producaoAnual.PerdidosPcs / totalAno * 100, 2);
                    producaoAnual.PercentualAbortados = Math.Round((decimal)producaoAnual.AbortadosPcs / totalAno * 100, 2);
                }
                else
                {
                    producaoAnual.PercentualFalhas = 0;
                    producaoAnual.PercentualAbortados = 0;
                }

                resultado.Add(producaoAnual);

                Console.WriteLine($"   ✅ {ano}: Total={totalAno}, Falhas={producaoAnual.PercentualFalhas}%, " +
                                 $"Abortados={producaoAnual.PercentualAbortados}%");
            }

            var duracao = (DateTime.Now - inicio).TotalSeconds;
            Console.WriteLine($"✅ Processamento anual concluído em {duracao:F2}s");

            return resultado;
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