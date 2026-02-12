// ========================================
// TimelineService.cs - MÉTODO ATUALIZADO
// ========================================
// Este código substitui o método ConstruirTimelineCompleta() existente
// Adiciona cálculo de Espera do Operador e garante 100% de cobertura

using System;
using System.Collections.Generic;
using System.Linq;
using SistemaProducao3D.Modelos.Timeline;
using SistemaProducao3D.Modelos.Modelos;

namespace Business_Logic.Serviços
{
    public partial class TimelineService
    {
        /// <summary>
        /// Constrói timeline completa do dia - GARANTE 100% das 24 horas preenchidas
        /// </summary>
        private List<BlocoTimeline> ConstruirTimelineCompleta(List<MesaProducao> jobs, DateTime data)
        {
            var blocos = new List<BlocoTimeline>();
            var inicioDia = new DateTime(data.Year, data.Month, data.Day, 0, 0, 0, DateTimeKind.Utc);
            var fimDia = inicioDia.AddDays(1);

            // ========================================
            // 1. PREENCHER INÍCIO DO DIA (se necessário)
            // ========================================
            if (jobs.Any() && jobs.First().DatetimeStarted > inicioDia)
            {
                blocos.Add(new BlocoTimeline
                {
                    Inicio = inicioDia,
                    Fim = jobs.First().DatetimeStarted,
                    DuracaoMinutos = (int)(jobs.First().DatetimeStarted - inicioDia).TotalMinutes,
                    Status = StatusMaquina.Ociosidade,
                    Motivo = DeterminarMotivoOciosidadeInicio(jobs.First().DatetimeStarted),
                    Mensagem = "Período noturno/sem expediente"
                });
            }
            else if (!jobs.Any())
            {
                // Dia inteiro sem jobs
                blocos.Add(new BlocoTimeline
                {
                    Inicio = inicioDia,
                    Fim = fimDia,
                    DuracaoMinutos = 1440,
                    Status = StatusMaquina.Ociosidade,
                    Motivo = MotivoStatus.FaltaJob,
                    Mensagem = "Sem jobs programados no dia"
                });
                return blocos; // Retorna logo se não tem jobs
            }

            // ========================================
            // 2. PROCESSAR CADA JOB
            // ========================================
            for (int i = 0; i < jobs.Count; i++)
            {
                var job = jobs[i];

                // ----------------------------------------
                // 2.1. BLOCO DE PRODUÇÃO (started ? finished)
                // ----------------------------------------
                if (job.DatetimeFinished.HasValue)
                {
                    var duracaoProducao = (int)(job.DatetimeFinished.Value - job.DatetimeStarted).TotalMinutes;

                    // ? Validação: Ignorar jobs com duração negativa ou muito longa
                    if (duracaoProducao < 0 || duracaoProducao > 2880) // Max 2 dias
                    {
                        Console.WriteLine($"?? Job {job.UltimakerJobUuid} com duração inválida: {duracaoProducao} min");
                        continue;
                    }

                    blocos.Add(new BlocoTimeline
                    {
                        Inicio = job.DatetimeStarted,
                        Fim = job.DatetimeFinished.Value,
                        DuracaoMinutos = duracaoProducao,
                        Status = StatusMaquina.Producao,
                        Motivo = MotivoStatus.ProducaoNormal,
                        Mensagem = job.IsSucess
                            ? "Produção concluída com sucesso"
                            : "Produção com problemas",
                        JobUuid = job.UltimakerJobUuid,
                        JobName = job.JobName,
                        TypeId = null
                    });

                    // ----------------------------------------
                    // 2.2. ? NOVO: ESPERA OPERADOR (finished ? cleaned)
                    // ----------------------------------------
                    // Tempo estimado entre finished e cleaned (quando operador remove a peça)
                    // Ajuste conforme observado na prática da sua operação
                    var tempoEsperaOperadorPadrao = TimeSpan.FromMinutes(5);

                    // Calcula quando seria o "cleaned" estimado
                    var fimEsperaEstimado = job.DatetimeFinished.Value.Add(tempoEsperaOperadorPadrao);

                    // Determina o limite real da espera
                    DateTime limiteEspera = fimEsperaEstimado;

                    // Se há próximo job, o limite é o início dele (ou a estimativa, o que vier primeiro)
                    if (i + 1 < jobs.Count)
                    {
                        var proximoJob = jobs[i + 1];
                        limiteEspera = proximoJob.DatetimeStarted < fimEsperaEstimado
                            ? proximoJob.DatetimeStarted
                            : fimEsperaEstimado;
                    }
                    // Se é o último job do dia, o limite é o fim do dia (ou estimativa, o que vier primeiro)
                    else
                    {
                        limiteEspera = fimEsperaEstimado < fimDia
                            ? fimEsperaEstimado
                            : fimDia;
                    }

                    // Só adiciona bloco de espera se houver tempo significativo (> 1 min)
                    var duracaoEspera = (int)(limiteEspera - job.DatetimeFinished.Value).TotalMinutes;
                    if (duracaoEspera > 1)
                    {
                        blocos.Add(new BlocoTimeline
                        {
                            Inicio = job.DatetimeFinished.Value,
                            Fim = limiteEspera,
                            DuracaoMinutos = duracaoEspera,
                            Status = StatusMaquina.EsperaOperador,
                            Motivo = MotivoStatus.EsperaRemocaoPeca,
                            Mensagem = "Aguardando remoção de peça da mesa",
                            JobUuid = job.UltimakerJobUuid,
                            JobName = job.JobName
                        });
                    }

                    // ----------------------------------------
                    // 2.3. OCIOSIDADE (cleaned ? próximo job)
                    // ----------------------------------------
                    if (i + 1 < jobs.Count)
                    {
                        var proximoJob = jobs[i + 1];

                        // Só adiciona bloco de ociosidade se houver intervalo significativo
                        var duracaoOciosidade = (int)(proximoJob.DatetimeStarted - limiteEspera).TotalMinutes;

                        if (duracaoOciosidade > 1)
                        {
                            blocos.Add(new BlocoTimeline
                            {
                                Inicio = limiteEspera,
                                Fim = proximoJob.DatetimeStarted,
                                DuracaoMinutos = duracaoOciosidade,
                                Status = StatusMaquina.Ociosidade,
                                Motivo = DeterminarMotivoOciosidade(TimeSpan.FromMinutes(duracaoOciosidade)),
                                Mensagem = DeterminarMensagemOciosidade(duracaoOciosidade)
                            });
                        }
                    }
                }
                else
                {
                    // Job SEM datetime_finished (failed logo no início ou em andamento)
                    // Considera como falha rápida
                    blocos.Add(new BlocoTimeline
                    {
                        Inicio = job.DatetimeStarted,
                        Fim = job.DatetimeStarted.AddMinutes(1), // Duração mínima
                        DuracaoMinutos = 1,
                        Status = StatusMaquina.Pausa,
                        Motivo = MotivoStatus.FalhaTemporaria,
                        Mensagem = $"Job {job.Status} sem conclusão",
                        JobUuid = job.UltimakerJobUuid,
                        JobName = job.JobName
                    });
                }
            }

            // ========================================
            // 3. PREENCHER FIM DO DIA (se necessário)
            // ========================================
            if (blocos.Any())
            {
                var ultimoBloco = blocos.Last();

                // Se o último bloco termina antes do fim do dia, preenche
                if (ultimoBloco.Fim < fimDia)
                {
                    var duracaoFinal = (int)(fimDia - ultimoBloco.Fim).TotalMinutes;

                    blocos.Add(new BlocoTimeline
                    {
                        Inicio = ultimoBloco.Fim,
                        Fim = fimDia,
                        DuracaoMinutos = duracaoFinal,
                        Status = StatusMaquina.Ociosidade,
                        Motivo = DeterminarMotivoOciosidadeFim(ultimoBloco.Fim),
                        Mensagem = "Período noturno/fim do expediente"
                    });
                }
            }

            // ========================================
            // 4. VALIDAÇÃO FINAL - GARANTE 100%
            // ========================================
            var totalMinutos = blocos.Sum(b => b.DuracaoMinutos);

            if (totalMinutos != 1440)
            {
                Console.WriteLine($"?? ATENÇÃO: Total de minutos diferente de 1440!");
                Console.WriteLine($"   Data: {data:yyyy-MM-dd}");
                Console.WriteLine($"   Total calculado: {totalMinutos} min");
                Console.WriteLine($"   Esperado: 1440 min");
                Console.WriteLine($"   Blocos criados: {blocos.Count}");

                // ?? CORREÇÃO AUTOMÁTICA: Ajusta último bloco
                if (totalMinutos < 1440 && blocos.Any())
                {
                    var diferenca = 1440 - totalMinutos;
                    var ultimoBloco = blocos.Last();

                    Console.WriteLine($"   ? Ajustando último bloco em +{diferenca} min");

                    ultimoBloco.Fim = ultimoBloco.Fim.AddMinutes(diferenca);
                    ultimoBloco.DuracaoMinutos += diferenca;
                }
                else if (totalMinutos > 1440 && blocos.Any())
                {
                    var diferenca = totalMinutos - 1440;
                    var ultimoBloco = blocos.Last();

                    Console.WriteLine($"   ? Ajustando último bloco em -{diferenca} min");

                    ultimoBloco.Fim = ultimoBloco.Fim.AddMinutes(-diferenca);
                    ultimoBloco.DuracaoMinutos -= diferenca;
                }
            }

            return blocos.OrderBy(b => b.Inicio).ToList();
        }

        // ========================================
        // MÉTODOS AUXILIARES
        // ========================================

        /// <summary>
        /// Determina motivo de ociosidade baseado na duração
        /// </summary>
        private MotivoStatus DeterminarMotivoOciosidade(TimeSpan duracao)
        {
            if (duracao.TotalHours >= 2)
                return MotivoStatus.FaltaJob; // Ociosidade longa = falta de job

            if (duracao.TotalMinutes >= 30)
                return MotivoStatus.AguardandoAprovacao; // Média = aguardando aprovação

            return MotivoStatus.FimExpediente; // Curta = intervalo normal
        }

        /// <summary>
        /// Determina motivo de ociosidade no início do dia
        /// </summary>
        private MotivoStatus DeterminarMotivoOciosidadeInicio(DateTime primeiroJob)
        {
            var hora = primeiroJob.Hour;

            // Se primeiro job é após 8h, provavelmente é início normal do expediente
            if (hora >= 8 && hora < 12)
                return MotivoStatus.FimExpediente;

            // Se é muito tarde (após 12h), pode ser falta de job
            if (hora >= 12)
                return MotivoStatus.FaltaJob;

            return MotivoStatus.FimExpediente;
        }

        /// <summary>
        /// Determina motivo de ociosidade no fim do dia
        /// </summary>
        private MotivoStatus DeterminarMotivoOciosidadeFim(DateTime ultimoEvento)
        {
            var hora = ultimoEvento.Hour;

            // Se termina antes das 18h, pode ser falta de job
            if (hora < 18)
                return MotivoStatus.FaltaJob;

            // Se termina após 18h, provavelmente é fim normal do expediente
            return MotivoStatus.FimExpediente;
        }

        /// <summary>
        /// Gera mensagem descritiva para ociosidade
        /// </summary>
        private string DeterminarMensagemOciosidade(int minutos)
        {
            if (minutos >= 120) // 2h+
                return "Aguardando novo job";

            if (minutos >= 30)
                return "Intervalo entre jobs";

            return "Setup/preparação";
        }

        /// <summary>
        /// Enriquece blocos com dados de eventos da API Ultimaker
        /// Detecta pausas, manutenção, etc.
        /// </summary>
        private void EnriquecerComEventos(List<BlocoTimeline> timeline, List<UltimakerEvent> eventos)
        {
            foreach (var bloco in timeline)
            {
                // Buscar eventos no período do bloco
                var eventosBloco = eventos.Where(e =>
                    e.Time >= bloco.Inicio && e.Time <= bloco.Fim).ToList();

                if (!eventosBloco.Any())
                    continue;

                // ----------------------------------------
                // DETECTAR PAUSAS
                // ----------------------------------------

                // Troca de material (type_id = 65537)
                if (eventosBloco.Any(e => e.IsMaterialChanged))
                {
                    bloco.Status = StatusMaquina.Pausa;
                    bloco.Motivo = MotivoStatus.TrocaMaterial;
                    bloco.Mensagem = "Troca de material detectada via API";
                    bloco.TypeId = 65537;
                    continue;
                }

                // Troca de hotend (type_id = 65536)
                if (eventosBloco.Any(e => e.IsHotendChanged))
                {
                    bloco.Status = StatusMaquina.Pausa;
                    bloco.Motivo = MotivoStatus.TrocaHotend;
                    bloco.Mensagem = "Troca de hotend detectada via API";
                    bloco.TypeId = 65536;
                    continue;
                }

                // Pausado durante impressão (type_id = 131073)
                if (eventosBloco.Any(e => e.IsPrintPaused))
                {
                    bloco.Status = StatusMaquina.Pausa;
                    bloco.Motivo = MotivoStatus.AjusteImpressao;
                    bloco.Mensagem = "Impressão pausada";
                    bloco.TypeId = 131073;
                    continue;
                }

                // ----------------------------------------
                // ? NOVO: DETECTAR MANUTENÇÃO
                // ----------------------------------------

                var mensagensBloco = string.Join(" ", eventosBloco.Select(e => e.Message.ToLower()));

                // Detectar por palavras-chave
                if (mensagensBloco.Contains("maintenance") ||
                    mensagensBloco.Contains("manutenção") ||
                    mensagensBloco.Contains("manutenção"))
                {
                    bloco.Status = StatusMaquina.Manutencao;
                    bloco.Motivo = DeterminarTipoManutencao(eventosBloco);
                    bloco.Mensagem = "Manutenção detectada via API";
                    continue;
                }

                // Detectar calibração
                if (mensagensBloco.Contains("calibr") ||
                    mensagensBloco.Contains("calibration"))
                {
                    bloco.Status = StatusMaquina.Manutencao;
                    bloco.Motivo = MotivoStatus.Calibracao;
                    bloco.Mensagem = "Calibração em andamento";
                    continue;
                }
            }
        }

        /// <summary>
        /// Determina tipo específico de manutenção
        /// </summary>
        private MotivoStatus DeterminarTipoManutencao(List<UltimakerEvent> eventos)
        {
            var mensagens = string.Join(" ", eventos.Select(e => e.Message.ToLower()));

            if (mensagens.Contains("preventiva") || mensagens.Contains("preventive"))
                return MotivoStatus.ManutencaoPreventiva;

            if (mensagens.Contains("calibr"))
                return MotivoStatus.Calibracao;

            return MotivoStatus.ManutencaoCorretiva;
        }
    }
}