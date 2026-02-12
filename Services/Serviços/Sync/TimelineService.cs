// ========================================
// TimelineService.cs - VERSÃO CORRIGIDA
// CORREÇÕES APLICADAS:
// 1. Detecção de pausas via TypeId (65537, 65536)
// 2. Cálculo real de espera operador (evento cleared)
// 3. Detecção de manutenção via TypeId
// 4. Dia sem jobs retorna ociosidade 100%
// 5. Logs de debug para troubleshooting
// ========================================

using Business_Logic.Repositories;
using Business_Logic.Repositories.Interfaces;
using Business_Logic.Serviços.Interfaces;
using SistemaProducao3D.Integration.Ultimaker;
using SistemaProducao3D.Modelos.Modelos;
using SistemaProducao3D.Modelos.Timeline;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Business_Logic.Serviços
{
    public class TimelineService : ITimelineService
    {
        private readonly IProducaoRepository _producaoRepository;
        private readonly IUltimakerClient _ultimakerClient;

        public TimelineService(
            IProducaoRepository producaoRepository,
            IUltimakerClient ultimakerClient)
        {
            _producaoRepository = producaoRepository;
            _ultimakerClient = ultimakerClient;
        }

        // ========================================
        // MÉTODOS PÚBLICOS - BÁSICOS
        // ========================================

        public async Task<List<BlocoTimeline>> ObterTimelineDiaAsync(int maquinaId, DateTime data)
        {
            Console.WriteLine($"📅 [TIMELINE] Obtendo timeline: M{maquinaId} - {data:yyyy-MM-dd}");

            var jobs = await _producaoRepository.ObterJobsPorMaquinaEData(maquinaId, data);

            Console.WriteLine($"   - Jobs encontrados: {jobs.Count}");

            var timeline = await ConstruirTimelineCompleta(jobs, data, maquinaId);

            Console.WriteLine($"   - Blocos criados: {timeline.Count}");
            Console.WriteLine($"   - Total minutos: {timeline.Sum(b => b.DuracaoMinutos)}");

            return timeline;
        }

        public async Task<List<BlocoTimeline>> ObterTimelineEnriquecidaAsync(
            int maquinaId,
            DateTime data,
            bool incluirEventos = true)
        {
            var timeline = await ObterTimelineDiaAsync(maquinaId, data);

            if (incluirEventos && timeline.Any())
            {
                try
                {
                    var inicioDia = new DateTime(data.Year, data.Month, data.Day, 0, 0, 0, DateTimeKind.Utc);
                    var fimDia = inicioDia.AddDays(1);

                    var eventos = await _ultimakerClient.GetEventsAsync(maquinaId, inicioDia, fimDia);

                    Console.WriteLine($"   - Eventos API: {eventos.Count}");

                    EnriquecerComEventos(timeline, eventos);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️  Erro ao enriquecer eventos: {ex.Message}");
                }
            }

            return timeline;
        }

        public async Task<Dictionary<DateTime, List<BlocoTimeline>>> ObterTimelinePeriodoAsync(
            int maquinaId,
            DateTime dataInicio,
            DateTime dataFim)
        {
            var resultado = new Dictionary<DateTime, List<BlocoTimeline>>();
            var dataAtual = dataInicio.Date;

            while (dataAtual <= dataFim.Date)
            {
                var timeline = await ObterTimelineDiaAsync(maquinaId, dataAtual);
                resultado[dataAtual] = timeline;
                dataAtual = dataAtual.AddDays(1);
            }

            return resultado;
        }

        public ResultadoValidacao ValidarCobertura(List<BlocoTimeline> blocos, DateTime data)
        {
            var resultado = new ResultadoValidacao
            {
                MinutosTotais = blocos.Sum(b => b.DuracaoMinutos)
            };

            resultado.Valido = resultado.MinutosTotais == 1440;

            if (!resultado.Valido)
            {
                resultado.Problemas.Add($"Total: {resultado.MinutosTotais} min (esperado: 1440)");
            }

            return resultado;
        }

        // ========================================
        // MÉTODOS PÚBLICOS - ANÁLISE EM CAMADAS
        // ========================================

        public async Task<ResumoConsolidado> ObterResumoConsolidado(int ano, int mes)
        {
            Console.WriteLine($"📊 [CONSOLIDADO] Gerando resumo: {mes:D2}/{ano}");

            var printers = await _ultimakerClient.GetPrintersAsync();
            var cultureInfo = new CultureInfo("pt-BR");

            var resumo = new ResumoConsolidado
            {
                Ano = ano,
                Mes = mes,
                Periodo = $"{cultureInfo.DateTimeFormat.GetAbbreviatedMonthName(mes).ToUpper()}/{ano.ToString().Substring(2)}"
            };

            decimal tempoTotalProducao = 0;
            decimal tempoTotalPausas = 0;
            decimal tempoTotalOciosidade = 0;
            decimal tempoTotalEsperaOperador = 0;
            decimal tempoTotalManutencao = 0;
            int totalJobsFinalizados = 0;
            int totalJobsAbortados = 0;

            var todosMotivos = new List<MotivoConsolidado>();

            foreach (var printer in printers.Where(p => p.IsActive))
            {
                try
                {
                    Console.WriteLine($"   🖨️  Processando: {printer.Name} (ID: {printer.Id})");

                    var resumoImpressora = await ObterResumoMensal(printer.Id, ano, mes);

                    tempoTotalProducao += resumoImpressora.TempoProducao;
                    tempoTotalPausas += resumoImpressora.TempoPausas;
                    tempoTotalOciosidade += resumoImpressora.TempoOciosidade;
                    tempoTotalEsperaOperador += resumoImpressora.TempoEsperaOperador;
                    tempoTotalManutencao += resumoImpressora.TempoManutencao;
                    totalJobsFinalizados += resumoImpressora.JobsFinalizados;
                    totalJobsAbortados += resumoImpressora.JobsAbortados;

                    todosMotivos.AddRange(resumoImpressora.Motivos);
                    resumo.Impressoras.Add(resumoImpressora);

                    Console.WriteLine($"      ✅ {resumoImpressora.MachineName}: {resumoImpressora.TempoProducao}h prod");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"      ❌ Erro: {ex.Message}");
                }
            }

            var tempoTotal = tempoTotalProducao + tempoTotalPausas + tempoTotalOciosidade +
                           tempoTotalEsperaOperador + tempoTotalManutencao;

            resumo.TempoTotalDisponivel = tempoTotal;
            resumo.TempoProducaoTotal = tempoTotalProducao;
            resumo.TempoPausasTotal = tempoTotalPausas;
            resumo.TempoOciosidadeTotal = tempoTotalOciosidade;
            resumo.TempoEsperaOperadorTotal = tempoTotalEsperaOperador;
            resumo.TempoManutencaoTotal = tempoTotalManutencao;

            if (tempoTotal > 0)
            {
                resumo.TaxaProducao = Math.Round((tempoTotalProducao / tempoTotal) * 100, 2);
                resumo.TaxaPausas = Math.Round((tempoTotalPausas / tempoTotal) * 100, 2);
                resumo.TaxaOciosidade = Math.Round((tempoTotalOciosidade / tempoTotal) * 100, 2);
                resumo.TaxaEsperaOperador = Math.Round((tempoTotalEsperaOperador / tempoTotal) * 100, 2);
                resumo.TaxaManutencao = Math.Round((tempoTotalManutencao / tempoTotal) * 100, 2);

                // Garantir 100%
                var somaAtual = resumo.TaxaProducao + resumo.TaxaPausas + resumo.TaxaOciosidade +
                               resumo.TaxaEsperaOperador + resumo.TaxaManutencao;

                if (Math.Abs(somaAtual - 100) > 0.01m)
                {
                    Console.WriteLine($"   ⚠️  Ajustando taxas: {somaAtual} → 100%");
                    resumo.TaxaProducao += (100 - somaAtual);
                }
            }

            resumo.HorasProdutivas = tempoTotalProducao;
            resumo.Utilizacao = resumo.TaxaProducao;
            resumo.TaxaSucesso = totalJobsFinalizados + totalJobsAbortados > 0
                ? Math.Round((decimal)totalJobsFinalizados / (totalJobsFinalizados + totalJobsAbortados) * 100, 1)
                : 0;

            // Consolidar top motivos
            resumo.TopMotivosPausas = todosMotivos
                .Where(m => m.Status == StatusMaquina.Pausa)
                .GroupBy(m => new { m.Status, m.Motivo, m.MotivoDescricao })
                .Select(g => new MotivoConsolidado
                {
                    Status = g.Key.Status,
                    Motivo = g.Key.Motivo,
                    MotivoDescricao = g.Key.MotivoDescricao,
                    TempoTotal = g.Sum(m => m.TempoTotal),
                    Ocorrencias = g.Sum(m => m.Ocorrencias),
                    Percentual = tempoTotalPausas > 0
                        ? Math.Round((decimal)g.Sum(m => m.TempoTotal) / (decimal)(tempoTotalPausas * 60) * 100, 1)
                        : 0
                })
                .OrderByDescending(m => m.TempoTotal)
                .Take(5)
                .ToList();

            resumo.TopMotivosOciosidade = todosMotivos
                .Where(m => m.Status == StatusMaquina.Ociosidade)
                .GroupBy(m => new { m.Status, m.Motivo, m.MotivoDescricao })
                .Select(g => new MotivoConsolidado
                {
                    Status = g.Key.Status,
                    Motivo = g.Key.Motivo,
                    MotivoDescricao = g.Key.MotivoDescricao,
                    TempoTotal = g.Sum(m => m.TempoTotal),
                    Ocorrencias = g.Sum(m => m.Ocorrencias),
                    Percentual = tempoTotalOciosidade > 0
                        ? Math.Round((decimal)g.Sum(m => m.TempoTotal) / (decimal)(tempoTotalOciosidade * 60) * 100, 1)
                        : 0
                })
                .OrderByDescending(m => m.TempoTotal)
                .Take(5)
                .ToList();

            resumo.TopMotivosEsperaOperador = todosMotivos
                .Where(m => m.Status == StatusMaquina.EsperaOperador)
                .GroupBy(m => new { m.Status, m.Motivo, m.MotivoDescricao })
                .Select(g => new MotivoConsolidado
                {
                    Status = g.Key.Status,
                    Motivo = g.Key.Motivo,
                    MotivoDescricao = g.Key.MotivoDescricao,
                    TempoTotal = g.Sum(m => m.TempoTotal),
                    Ocorrencias = g.Sum(m => m.Ocorrencias),
                    Percentual = tempoTotalEsperaOperador > 0
                        ? Math.Round((decimal)g.Sum(m => m.TempoTotal) / (decimal)(tempoTotalEsperaOperador * 60) * 100, 1)
                        : 0
                })
                .OrderByDescending(m => m.TempoTotal)
                .Take(5)
                .ToList();

            Console.WriteLine($"   ✅ Consolidado: {resumo.Impressoras.Count} impressoras");
            Console.WriteLine($"      Total: {tempoTotal}h | Prod: {resumo.TaxaProducao}% | Ocio: {resumo.TaxaOciosidade}%");

            return resumo;
        }

        public async Task<ResumoMensal> ObterResumoMensal(int maquinaId, int ano, int mes)
        {
            var printer = (await _ultimakerClient.GetPrintersAsync())
                .FirstOrDefault(p => p.Id == maquinaId);

            var cultureInfo = new CultureInfo("pt-BR");

            var resumo = new ResumoMensal
            {
                MachineId = maquinaId,
                MachineName = printer?.Name ?? $"M{maquinaId}",
                Ano = ano,
                Mes = mes,
                MesNome = cultureInfo.DateTimeFormat.GetMonthName(mes)
            };

            var totalDias = DateTime.DaysInMonth(ano, mes);
            decimal tempoTotalProducao = 0;
            decimal tempoTotalPausas = 0;
            decimal tempoTotalOciosidade = 0;
            decimal tempoTotalEsperaOperador = 0;
            decimal tempoTotalManutencao = 0;

            var todosMotivos = new Dictionary<string, MotivoConsolidado>();

            for (int dia = 1; dia <= totalDias; dia++)
            {
                var data = new DateTime(ano, mes, dia);

                try
                {
                    var timeline = await ObterTimelineDiaAsync(maquinaId, data);

                    var producaoDia = timeline.Where(b => b.Status == StatusMaquina.Producao).Sum(b => b.DuracaoMinutos);
                    var pausasDia = timeline.Where(b => b.Status == StatusMaquina.Pausa).Sum(b => b.DuracaoMinutos);
                    var ociosidadeDia = timeline.Where(b => b.Status == StatusMaquina.Ociosidade).Sum(b => b.DuracaoMinutos);
                    var esperaDia = timeline.Where(b => b.Status == StatusMaquina.EsperaOperador).Sum(b => b.DuracaoMinutos);
                    var manutencaoDia = timeline.Where(b => b.Status == StatusMaquina.Manutencao).Sum(b => b.DuracaoMinutos);

                    tempoTotalProducao += producaoDia;
                    tempoTotalPausas += pausasDia;
                    tempoTotalOciosidade += ociosidadeDia;
                    tempoTotalEsperaOperador += esperaDia;
                    tempoTotalManutencao += manutencaoDia;

                    foreach (var bloco in timeline)
                    {
                        var chave = $"{bloco.Status}_{bloco.Motivo}";
                        if (!todosMotivos.ContainsKey(chave))
                        {
                            todosMotivos[chave] = new MotivoConsolidado
                            {
                                Status = bloco.Status,
                                Motivo = bloco.Motivo,
                                MotivoDescricao = bloco.Mensagem,
                                TempoTotal = 0,
                                Ocorrencias = 0
                            };
                        }
                        todosMotivos[chave].TempoTotal += bloco.DuracaoMinutos;
                        todosMotivos[chave].Ocorrencias++;
                    }

                    var resumoDia = new ResumoDiario
                    {
                        Data = data,
                        MachineId = maquinaId,
                        MachineName = resumo.MachineName,
                        TempoProducao = producaoDia,
                        TempoPausas = pausasDia,
                        TempoOciosidade = ociosidadeDia,
                        TempoEsperaOperador = esperaDia,
                        TempoManutencao = manutencaoDia,
                        Timeline = timeline
                    };

                    resumo.Dias.Add(resumoDia);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Erro dia {data:yyyy-MM-dd}: {ex.Message}");
                }
            }

            // Converter minutos → horas
            resumo.TempoProducao = Math.Round(tempoTotalProducao / 60, 1);
            resumo.TempoPausas = Math.Round(tempoTotalPausas / 60, 1);
            resumo.TempoOciosidade = Math.Round(tempoTotalOciosidade / 60, 1);
            resumo.TempoEsperaOperador = Math.Round(tempoTotalEsperaOperador / 60, 1);
            resumo.TempoManutencao = Math.Round(tempoTotalManutencao / 60, 1);
            resumo.TempoTotal = resumo.TempoProducao + resumo.TempoPausas + resumo.TempoOciosidade +
                               resumo.TempoEsperaOperador + resumo.TempoManutencao;

            // Calcular percentuais
            foreach (var motivo in todosMotivos.Values)
            {
                var tempoTotalStatus = motivo.Status switch
                {
                    StatusMaquina.Producao => tempoTotalProducao,
                    StatusMaquina.Pausa => tempoTotalPausas,
                    StatusMaquina.Ociosidade => tempoTotalOciosidade,
                    StatusMaquina.EsperaOperador => tempoTotalEsperaOperador,
                    StatusMaquina.Manutencao => tempoTotalManutencao,
                    _ => 1
                };

                motivo.Percentual = tempoTotalStatus > 0
                    ? Math.Round((decimal)motivo.TempoTotal / tempoTotalStatus * 100, 1)
                    : 0;
            }

            resumo.Motivos = todosMotivos.Values
                .OrderByDescending(m => m.TempoTotal)
                .ToList();

            // Jobs do período
            var jobs = await _producaoRepository.ObterJobsPorMaquinaEPeriodo(
                maquinaId,
                new DateTime(ano, mes, 1),
                new DateTime(ano, mes, totalDias, 23, 59, 59));

            resumo.JobsFinalizados = jobs.Count(j => j.IsSucess);
            resumo.JobsAbortados = jobs.Count(j => !j.IsSucess);

            return resumo;
        }

        public async Task<ResumoDiario> ObterResumoDiario(int maquinaId, DateTime data)
        {
            var printer = (await _ultimakerClient.GetPrintersAsync())
                .FirstOrDefault(p => p.Id == maquinaId);

            var timeline = await ObterTimelineEnriquecidaAsync(maquinaId, data, true);

            var resumo = new ResumoDiario
            {
                MachineId = maquinaId,
                MachineName = printer?.Name ?? $"M{maquinaId}",
                Data = data,
                Timeline = timeline
            };

            var producao = timeline.Where(b => b.Status == StatusMaquina.Producao).Sum(b => b.DuracaoMinutos);
            var pausas = timeline.Where(b => b.Status == StatusMaquina.Pausa).Sum(b => b.DuracaoMinutos);
            var ociosidade = timeline.Where(b => b.Status == StatusMaquina.Ociosidade).Sum(b => b.DuracaoMinutos);
            var espera = timeline.Where(b => b.Status == StatusMaquina.EsperaOperador).Sum(b => b.DuracaoMinutos);
            var manutencao = timeline.Where(b => b.Status == StatusMaquina.Manutencao).Sum(b => b.DuracaoMinutos);

            resumo.TempoProducao = producao;
            resumo.TempoPausas = pausas;
            resumo.TempoOciosidade = ociosidade;
            resumo.TempoEsperaOperador = espera;
            resumo.TempoManutencao = manutencao;

            // Consolidar motivos
            var motivos = timeline
                .GroupBy(b => new { b.Status, b.Motivo, b.Mensagem })
                .Select(g =>
                {
                    var tempoTotal = g.Sum(b => b.DuracaoMinutos);
                    var tempoTotalStatus = g.Key.Status switch
                    {
                        StatusMaquina.Producao => producao,
                        StatusMaquina.Pausa => pausas,
                        StatusMaquina.Ociosidade => ociosidade,
                        StatusMaquina.EsperaOperador => espera,
                        StatusMaquina.Manutencao => manutencao,
                        _ => 1
                    };

                    return new MotivoConsolidado
                    {
                        Status = g.Key.Status,
                        Motivo = g.Key.Motivo,
                        MotivoDescricao = g.Key.Mensagem,
                        TempoTotal = tempoTotal,
                        Ocorrencias = g.Count(),
                        Percentual = tempoTotalStatus > 0
                            ? Math.Round((decimal)tempoTotal / tempoTotalStatus * 100, 1)
                            : 0
                    };
                })
                .OrderByDescending(m => m.TempoTotal)
                .ToList();

            resumo.Motivos = motivos;

            return resumo;
        }

        // ========================================
        // CONSTRUÇÃO DA TIMELINE - VERSÃO CORRIGIDA
        // ========================================

        private async Task<List<BlocoTimeline>> ConstruirTimelineCompleta(
            List<MesaProducao> jobs,
            DateTime data,
            int maquinaId)
        {
            var blocos = new List<BlocoTimeline>();
            var inicioDia = new DateTime(data.Year, data.Month, data.Day, 0, 0, 0, DateTimeKind.Utc);
            var fimDia = inicioDia.AddDays(1);

            // Dia sem jobs = 100% ociosidade
            if (!jobs.Any())
            {
                blocos.Add(new BlocoTimeline
                {
                    Inicio = inicioDia,
                    Fim = fimDia,
                    DuracaoMinutos = 1440,
                    Status = StatusMaquina.Ociosidade,
                    Motivo = MotivoStatus.FaltaJob,
                    Mensagem = "Sem jobs programados no dia"
                });
                return blocos;
            }

            // Preencher início
            if (jobs.First().DatetimeStarted > inicioDia)
            {
                blocos.Add(new BlocoTimeline
                {
                    Inicio = inicioDia,
                    Fim = jobs.First().DatetimeStarted,
                    DuracaoMinutos = (int)(jobs.First().DatetimeStarted - inicioDia).TotalMinutes,
                    Status = StatusMaquina.Ociosidade,
                    Motivo = MotivoStatus.FimExpediente,
                    Mensagem = "Período noturno/sem expediente"
                });
            }

            // Processar jobs
            for (int i = 0; i < jobs.Count; i++)
            {
                var job = jobs[i];

                if (!job.DatetimeFinished.HasValue)
                {
                    blocos.Add(new BlocoTimeline
                    {
                        Inicio = job.DatetimeStarted,
                        Fim = job.DatetimeStarted.AddMinutes(1),
                        DuracaoMinutos = 1,
                        Status = StatusMaquina.Pausa,
                        Motivo = MotivoStatus.FalhaTemporaria,
                        Mensagem = $"Job {job.Status} sem conclusão",
                        JobUuid = job.UltimakerJobUuid,
                        JobName = job.JobName
                    });
                    continue;
                }

                var duracaoProducao = (int)(job.DatetimeFinished.Value - job.DatetimeStarted).TotalMinutes;

                if (duracaoProducao < 0 || duracaoProducao > 2880)
                {
                    Console.WriteLine($"⚠️  Job {job.UltimakerJobUuid}: duração inválida ({duracaoProducao} min)");
                    continue;
                }

                // PRODUÇÃO
                blocos.Add(new BlocoTimeline
                {
                    Inicio = job.DatetimeStarted,
                    Fim = job.DatetimeFinished.Value,
                    DuracaoMinutos = duracaoProducao,
                    Status = StatusMaquina.Producao,
                    Motivo = MotivoStatus.ProducaoNormal,
                    Mensagem = job.IsSucess ? "Produção concluída com sucesso" : "Produção com problemas",
                    JobUuid = job.UltimakerJobUuid,
                    JobName = job.JobName
                });

                // 🔧 CORREÇÃO: ESPERA OPERADOR REAL
                DateTime fimEspera = job.DatetimeFinished.Value.AddMinutes(5); // Default

                try
                {
                    var eventos = await _ultimakerClient.GetEventsByJobUuidAsync(maquinaId, job.UltimakerJobUuid);
                    var eventoCleared = eventos.FirstOrDefault(e =>
                        e.TypeId == 131077 && e.Message.Contains("cleared"));

                    if (eventoCleared != null)
                    {
                        fimEspera = eventoCleared.Time;
                        Console.WriteLine($"   ✅ Espera real: {job.JobName} → {(fimEspera - job.DatetimeFinished.Value).TotalMinutes:F1} min");
                    }
                }
                catch
                {
                    // Usar fallback
                }

                // Ajustar se próximo job começar antes
                if (i + 1 < jobs.Count && jobs[i + 1].DatetimeStarted < fimEspera)
                {
                    fimEspera = jobs[i + 1].DatetimeStarted;
                }
                else if (fimEspera > fimDia)
                {
                    fimEspera = fimDia;
                }

                var duracaoEspera = (int)(fimEspera - job.DatetimeFinished.Value).TotalMinutes;

                if (duracaoEspera > 1)
                {
                    blocos.Add(new BlocoTimeline
                    {
                        Inicio = job.DatetimeFinished.Value,
                        Fim = fimEspera,
                        DuracaoMinutos = duracaoEspera,
                        Status = StatusMaquina.EsperaOperador,
                        Motivo = MotivoStatus.EsperaRemocaoPeca,
                        Mensagem = "Aguardando remoção de peça da mesa",
                        JobUuid = job.UltimakerJobUuid,
                        JobName = job.JobName
                    });
                }

                // OCIOSIDADE
                if (i + 1 < jobs.Count)
                {
                    var proximoJob = jobs[i + 1];
                    var duracaoOciosidade = (int)(proximoJob.DatetimeStarted - fimEspera).TotalMinutes;

                    if (duracaoOciosidade > 1)
                    {
                        blocos.Add(new BlocoTimeline
                        {
                            Inicio = fimEspera,
                            Fim = proximoJob.DatetimeStarted,
                            DuracaoMinutos = duracaoOciosidade,
                            Status = StatusMaquina.Ociosidade,
                            Motivo = duracaoOciosidade >= 120 ? MotivoStatus.FaltaJob : MotivoStatus.AguardandoAprovacao,
                            Mensagem = duracaoOciosidade >= 120 ? "Aguardando novo job" : "Intervalo entre jobs"
                        });
                    }
                }
            }

            // Preencher fim
            if (blocos.Any())
            {
                var ultimoBloco = blocos.Last();
                if (ultimoBloco.Fim < fimDia)
                {
                    blocos.Add(new BlocoTimeline
                    {
                        Inicio = ultimoBloco.Fim,
                        Fim = fimDia,
                        DuracaoMinutos = (int)(fimDia - ultimoBloco.Fim).TotalMinutes,
                        Status = StatusMaquina.Ociosidade,
                        Motivo = MotivoStatus.FimExpediente,
                        Mensagem = "Período noturno/fim do expediente"
                    });
                }
            }

            // VALIDAÇÃO FINAL
            var totalMinutos = blocos.Sum(b => b.DuracaoMinutos);

            if (totalMinutos != 1440)
            {
                Console.WriteLine($"⚠️  Total minutos: {totalMinutos} (ajustando...)");

                if (blocos.Any())
                {
                    var diferenca = 1440 - totalMinutos;
                    blocos.Last().DuracaoMinutos += diferenca;
                    blocos.Last().Fim = blocos.Last().Fim.AddMinutes(diferenca);
                }
            }

            return blocos.OrderBy(b => b.Inicio).ToList();
        }

        // 🔧 CORREÇÃO: ENRIQUECIMENTO VIA TYPE_ID
        private void EnriquecerComEventos(List<BlocoTimeline> timeline, List<UltimakerEvent> eventos)
        {
            foreach (var bloco in timeline)
            {
                var eventosBloco = eventos.Where(e =>
                    e.Time >= bloco.Inicio && e.Time <= bloco.Fim).ToList();

                if (!eventosBloco.Any())
                    continue;

                foreach (var evento in eventosBloco)
                {
                    // 🔧 Material changed (65537) = PAUSA
                    if (evento.TypeId == 65537)
                    {
                        bloco.Status = StatusMaquina.Pausa;
                        bloco.Motivo = MotivoStatus.TrocaMaterial;
                        bloco.Mensagem = "Troca de material detectada";
                        bloco.TypeId = 65537;
                        break;
                    }

                    // 🔧 Hotend changed (65536) = MANUTENÇÃO
                    if (evento.TypeId == 65536)
                    {
                        bloco.Status = StatusMaquina.Manutencao;
                        bloco.Motivo = MotivoStatus.ManutencaoPreventiva;
                        bloco.Mensagem = $"Manutenção: {evento.Message}";
                        bloco.TypeId = 65536;
                        break;
                    }

                    // 🔧 Print paused (131073) = PAUSA
                    if (evento.TypeId == 131073)
                    {
                        bloco.Status = StatusMaquina.Pausa;
                        bloco.Motivo = MotivoStatus.AjusteImpressao;
                        bloco.Mensagem = "Impressão pausada manualmente";
                        bloco.TypeId = 131073;
                        break;
                    }

                    // 🔧 System started (1) = possível manutenção corretiva
                    if (evento.TypeId == 1 && evento.Message.Contains("System started"))
                    {
                        bloco.Status = StatusMaquina.Manutencao;
                        bloco.Motivo = MotivoStatus.ManutencaoCorretiva;
                        bloco.Mensagem = "Manutenção corretiva (sistema reiniciado)";
                        bloco.TypeId = 1;
                        break;
                    }
                }
            }
        }
    }
}