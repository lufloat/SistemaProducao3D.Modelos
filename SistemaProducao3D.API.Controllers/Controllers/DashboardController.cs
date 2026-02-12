using Business_Logic.Serviços.Interfaces;
using Business_Logic.Repositories.Interfaces;
using Business_Logic.Serviços.Sync;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Business_Logic.Repositories;
using SistemaProducao3D.Integration.Ultimaker;
using SistemaProducao3D.Modelos.Timeline;
using Business_Logic.Serviços;

namespace SistemaProducao3D.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardSKUService _dashboardService;
        private readonly IVisaoGeralService _visaoGeralService;
        private readonly ICardService _cardService;
        private readonly ISyncService _syncService;
        private readonly IProducaoRepository _producaoRepository;
        private readonly IMaterialRepository _materialRepository;
        private readonly IUltimakerClient _ultimakerClient;
        private readonly ITimelineService _timelineService; // ✅ NOVO

        public DashboardController(
            IDashboardSKUService dashboardService,
            IVisaoGeralService visaoGeralService,
            ICardService cardService,
            ISyncService syncService,
            IProducaoRepository producaoRepository,
            IMaterialRepository materialRepository,
            IUltimakerClient ultimakerClient,
            ITimelineService timelineService) // ✅ NOVO
        {
            _dashboardService = dashboardService;
            _visaoGeralService = visaoGeralService;
            _cardService = cardService;
            _syncService = syncService;
            _producaoRepository = producaoRepository;
            _materialRepository = materialRepository;
            _ultimakerClient = ultimakerClient;
            _timelineService = timelineService; // ✅ NOVO
        }

        // ========================================
        // KPIs e SKUs (EXISTENTES)
        // ========================================

        [HttpGet("kpis")]
        public async Task<IActionResult> ObterKPIs(
            [FromQuery] int? ano = null,
            [FromQuery] int mesInicio = 1,
            [FromQuery] int? mesFim = null)
        {
            try
            {
                var kpis = await _dashboardService.ObterMetricasKPI(ano, mesInicio, mesFim);
                return Ok(kpis);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("evolucao-skus")]
        public async Task<IActionResult> ObterEvolucaoSKUs(
            [FromQuery] int? anoInicio = null,
            [FromQuery] int mesInicio = 1,
            [FromQuery] int? anoFim = null,
            [FromQuery] int mesFim = 12)
        {
            try
            {
                var evolucao = await _dashboardService.ObterEvolucaoSKUs(anoInicio, mesInicio, anoFim, mesFim);
                return Ok(evolucao);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // ========================================
        // VISAO GERAL - CONSOLIDADO MENSAL (EXISTENTES)
        // ========================================

        [HttpGet("visao-geral/producao")]
        public async Task<IActionResult> ObterProducaoMensal(
            [FromQuery] int ano = 2025,
            [FromQuery] int mesInicio = 1,
            [FromQuery] int mesFim = 12)
        {
            try
            {
                var dados = await _visaoGeralService.ObterProducaoMensal(ano, mesInicio, mesFim);
                return Ok(dados);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("visao-geral/prototipos")]
        public async Task<IActionResult> ObterPrototiposMensal(
            [FromQuery] int ano = 2025,
            [FromQuery] int mesInicio = 1,
            [FromQuery] int mesFim = 12)
        {
            try
            {
                var dados = await _visaoGeralService.ObterPrototipoMensal(ano, mesInicio, mesFim);
                return Ok(dados);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("visao-geral/erros")]
        public async Task<IActionResult> ObterErrosMensal(
            [FromQuery] int ano = 2025,
            [FromQuery] int mesInicio = 1,
            [FromQuery] int mesFim = 12)
        {
            try
            {
                var dados = await _visaoGeralService.ObterErrosMensais(ano, mesInicio, mesFim);
                return Ok(dados);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("visao-geral/peso")]
        public async Task<IActionResult> ObterPesoMensal(
            [FromQuery] int ano = 2025,
            [FromQuery] int mesInicio = 1,
            [FromQuery] int mesFim = 12)
        {
            try
            {
                var dados = await _visaoGeralService.ObterPesoMensal(ano, mesInicio, mesFim);
                return Ok(dados);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("visao-geral/failed")]
        public async Task<IActionResult> ObterFailedMensal(
            [FromQuery] int ano = 2025,
            [FromQuery] int mesInicio = 1,
            [FromQuery] int mesFim = 12)
        {
            try
            {
                var dados = await _visaoGeralService.ObterFailedMensais(ano, mesInicio, mesFim);
                return Ok(dados);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("visao-geral/aborted")]
        public async Task<IActionResult> ObterAbortedMensal(
            [FromQuery] int ano = 2025,
            [FromQuery] int mesInicio = 1,
            [FromQuery] int mesFim = 12)
        {
            try
            {
                var dados = await _visaoGeralService.ObterAbortedMensais(ano, mesInicio, mesFim);
                return Ok(dados);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // ========================================
        // VISAO GERAL - POR IMPRESSORA (ANUAL) (EXISTENTES)
        // ========================================

        [HttpGet("visao-geral/producao/impressora/anual")]
        public async Task<IActionResult> ObterProducaoPorImpressoraAnual(
            [FromQuery] int ano = 2026,
            [FromQuery] int mesInicio = 1,
            [FromQuery] int mesFim = 12)
        {
            try
            {
                var dados = await _visaoGeralService.ObterProducaoPorImpressoraAnual(ano, mesInicio, mesFim);
                return Ok(dados);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("visao-geral/prototipos/impressora/anual")]
        public async Task<IActionResult> ObterPrototiposPorImpressoraAnual(
            [FromQuery] int ano = 2026,
            [FromQuery] int mesInicio = 1,
            [FromQuery] int mesFim = 12)
        {
            try
            {
                var dados = await _visaoGeralService.ObterPrototiposPorImpressoraAnual(ano, mesInicio, mesFim);
                return Ok(dados);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("visao-geral/erros/impressora/anual")]
        public async Task<IActionResult> ObterErrosPorImpressoraAnual(
            [FromQuery] int ano = 2026,
            [FromQuery] int mesInicio = 1,
            [FromQuery] int mesFim = 12)
        {
            try
            {
                var dados = await _visaoGeralService.ObterErrosPorImpressoraAnual(ano, mesInicio, mesFim);
                return Ok(dados);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("visao-geral/peso/impressora/anual")]
        public async Task<IActionResult> ObterPesoPorImpressoraAnual(
            [FromQuery] int ano = 2026,
            [FromQuery] int mesInicio = 1,
            [FromQuery] int mesFim = 12)
        {
            try
            {
                var dados = await _visaoGeralService.ObterPesoPorImpressoraAnual(ano, mesInicio, mesFim);
                return Ok(dados);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("visao-geral/failed/impressora/anual")]
        public async Task<IActionResult> ObterFailedPorImpressoraAnual(
            [FromQuery] int ano = 2026,
            [FromQuery] int mesInicio = 1,
            [FromQuery] int mesFim = 12)
        {
            try
            {
                var dados = await _visaoGeralService.ObterFailedPorImpressoraAnual(ano, mesInicio, mesFim);
                return Ok(dados);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("visao-geral/aborted/impressora/anual")]
        public async Task<IActionResult> ObterAbortedPorImpressoraAnual(
            [FromQuery] int ano = 2026,
            [FromQuery] int mesInicio = 1,
            [FromQuery] int mesFim = 12)
        {
            try
            {
                var dados = await _visaoGeralService.ObterAbortedPorImpressoraAnual(ano, mesInicio, mesFim);
                return Ok(dados);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // ========================================
        // CARDS - KG E CAPACIDADE (CONSOLIDADO) (EXISTENTES)
        // ========================================

        [HttpGet("cards/kg")]
        public async Task<IActionResult> ObterCardsKg(
            [FromQuery] int ano = 2025,
            [FromQuery] int mesInicio = 1,
            [FromQuery] int mesFim = 12)
        {
            try
            {
                var cards = await _cardService.ObterCardsKg(ano, mesInicio, mesFim);
                return Ok(cards);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("cards/capacidade")]
        public async Task<IActionResult> ObterCardsCapacidade(
            [FromQuery] int ano = 2025,
            [FromQuery] int mesInicio = 1,
            [FromQuery] int mesFim = 12,
            [FromQuery] int numeroMaquinas = 6)
        {
            try
            {
                var cards = await _cardService.ObterCardsCapacidade(ano, mesInicio, mesFim, numeroMaquinas);
                return Ok(cards);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // ========================================
        // CARDS - POR IMPRESSORA (EXISTENTES)
        // ========================================

        [HttpGet("cards/capacidade/impressora")]
        public async Task<IActionResult> ObterCapacidadePorImpressora(
            [FromQuery] int ano = 2026,
            [FromQuery] int mes = 1)
        {
            try
            {
                var cards = await _cardService.ObterCapacidadePorImpressora(ano, mes);
                return Ok(cards);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("cards/kg/impressora")]
        public async Task<IActionResult> ObterKgPorImpressora(
            [FromQuery] int ano = 2026,
            [FromQuery] int mes = 1)
        {
            try
            {
                var cards = await _cardService.ObterKgPorImpressora(ano, mes);
                return Ok(cards);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // ========================================
        // ENDPOINTS DE TESTE (EXISTENTES)
        // ========================================

        [HttpPost("test/sincronizar-mes")]
        public async Task<IActionResult> SincronizarMes(
            [FromQuery] int ano = 2026,
            [FromQuery] int mes = 1)
        {
            try
            {
                await _syncService.SincronizarMesAsync(ano, mes);
                return Ok(new
                {
                    sucesso = true,
                    mensagem = $"Sincronizacao de {mes:D2}/{ano} concluida com sucesso"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    sucesso = false,
                    erro = ex.Message,
                    detalhes = ex.InnerException?.Message
                });
            }
        }

        [HttpGet("test/job/{uuid}")]
        public async Task<IActionResult> VerJob(string uuid)
        {
            try
            {
                var job = await _producaoRepository.ObterPorUuid(uuid);

                if (job == null)
                    return NotFound(new { mensagem = "Job nao encontrado" });

                var material0 = job.Material0Guid.HasValue
                    ? await _materialRepository.ObterPorGuid(job.Material0Guid.Value)
                    : null;

                var material1 = job.Material1Guid.HasValue
                    ? await _materialRepository.ObterPorGuid(job.Material1Guid.Value)
                    : null;

                return Ok(new
                {
                    job.JobName,
                    job.Status,
                    job.DatetimeStarted,
                    job.DatetimeFinished,

                    material0 = new
                    {
                        guid = job.Material0Guid,
                        nome = material0?.Nome,
                        densidade = material0?.Densidade,
                        volume_mm3 = job.Material0Amount,
                        peso_g = job.Material0WeightG,
                        peso_kg = Math.Round(job.Material0WeightG / 1000m, 3)
                    },

                    material1 = new
                    {
                        guid = job.Material1Guid,
                        nome = material1?.Nome,
                        densidade = material1?.Densidade,
                        volume_mm3 = job.Material1Amount,
                        peso_g = job.Material1WeightG,
                        peso_kg = Math.Round(job.Material1WeightG / 1000m, 3)
                    },

                    total = new
                    {
                        peso_g = job.MaterialTotal,
                        peso_kg = Math.Round(job.MaterialTotalKg, 3)
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        [HttpGet("test/materiais")]
        public async Task<IActionResult> ListarMateriais()
        {
            try
            {
                var materiais = await _materialRepository.ListarTodos();

                return Ok(materiais.Select(m => new
                {
                    guid = m.UltimakerMaterialGuid,
                    m.Nome,
                    densidade_g_cm3 = m.Densidade,
                    m.Fabricante,
                    criado_em = m.CreatedAt
                }).OrderBy(m => m.Nome));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // ========================================
        // MATERIAIS (EXISTENTES)
        // ========================================

        [HttpPost("materiais/atualizar-densidades")]
        public async Task<IActionResult> AtualizarDensidadesMateriais()
        {
            try
            {
                Console.WriteLine("[DASHBOARD] Iniciando atualizacao de densidades via API...");

                if (_materialRepository is MaterialRepository repo)
                {
                    var quantidadeAtualizada = await repo.AtualizarDensidadesExistentes();

                    return Ok(new
                    {
                        sucesso = true,
                        mensagem = "Densidades atualizadas com sucesso",
                        materiaisAtualizados = quantidadeAtualizada
                    });
                }

                return BadRequest(new { sucesso = false, mensagem = "MaterialRepository nao disponivel" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    sucesso = false,
                    erro = ex.Message,
                    detalhes = ex.InnerException?.Message
                });
            }
        }

        [HttpGet("materiais/verificar-densidades")]
        public async Task<IActionResult> VerificarDensidades()
        {
            try
            {
                var materiais = await _materialRepository.ListarTodos();

                var materiaisComDensidadePadrao = materiais
                    .Where(m => m.Densidade == 1.24m)
                    .Select(m => new
                    {
                        guid = m.UltimakerMaterialGuid,
                        nome = m.Nome,
                        fabricante = m.Fabricante,
                        densidade = m.Densidade,
                        criadoEm = m.CreatedAt
                    })
                    .OrderBy(m => m.nome)
                    .ToList();

                var materiaisComDensidadeDiferente = materiais
                    .Where(m => m.Densidade != 1.24m)
                    .Select(m => new
                    {
                        guid = m.UltimakerMaterialGuid,
                        nome = m.Nome,
                        fabricante = m.Fabricante,
                        densidade = m.Densidade,
                        criadoEm = m.CreatedAt
                    })
                    .OrderBy(m => m.nome)
                    .ToList();

                return Ok(new
                {
                    resumo = new
                    {
                        totalMateriais = materiais.Count,
                        comDensidadePadrao = materiaisComDensidadePadrao.Count,
                        comDensidadeReal = materiaisComDensidadeDiferente.Count,
                        percentualPadrao = materiais.Count > 0
                        ? Math.Round((decimal)materiaisComDensidadePadrao.Count / materiais.Count * 100, 1)
                        : 0
                    },
                    materiaisComDensidadePadrao,
                    materiaisComDensidadeReal = materiaisComDensidadeDiferente
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // ========================================
        // ANÁLISE DE EVENTOS (EXISTENTES)
        // ========================================

        /// <summary>
        /// GET /api/dashboard/eventos/impressora/{machineId}?dataInicio=2026-01-01&dataFim=2026-01-31
        /// Retorna todos os eventos de uma impressora em um período
        /// </summary>
        [HttpGet("eventos/impressora/{machineId}")]
        public async Task<IActionResult> ObterEventosImpressora(
            int machineId,
            [FromQuery] DateTime dataInicio,
            [FromQuery] DateTime dataFim)
        {
            try
            {
                var eventos = await _ultimakerClient.GetEventsAsync(machineId, dataInicio, dataFim);

                return Ok(new
                {
                    impressoraId = machineId,
                    periodo = new { inicio = dataInicio, fim = dataFim },
                    totalEventos = eventos.Count,
                    eventos = eventos.Select(e => new
                    {
                        timestamp = e.Time,
                        mensagem = e.Message,
                        tipo = e.EventCategory,
                        typeId = e.TypeId,
                        jobUuid = e.GetJobUuid(),
                        parametros = e.Parameters
                    }).OrderBy(e => e.timestamp)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        /// <summary>
        /// GET /api/dashboard/analise/impressora/{machineId}?dataInicio=2026-01-01&dataFim=2026-01-31
        /// Analisa eventos e retorna taxas de ociosidade, pausas e produção
        /// </summary>
        [HttpGet("analise/impressora/{machineId}")]
        public async Task<IActionResult> AnalisarImpressora(
            int machineId,
            [FromQuery] DateTime dataInicio,
            [FromQuery] DateTime dataFim)
        {
            try
            {
                var analise = await _ultimakerClient.AnalyzeEventsAsync(machineId, dataInicio, dataFim);

                return Ok(new
                {
                    impressora = new
                    {
                        id = analise.MachineId,
                        nome = analise.MachineName
                    },
                    periodo = new
                    {
                        inicio = analise.PeriodoInicio,
                        fim = analise.PeriodoFim,
                        totalMinutos = Math.Round(analise.TempoTotalPeriodo, 0),
                        totalHoras = Math.Round(analise.TempoTotalPeriodo / 60, 1)
                    },
                    jobs = new
                    {
                        iniciados = analise.JobsIniciados,
                        finalizados = analise.JobsFinalizados,
                        abortados = analise.JobsAbortados
                    },
                    tempos = new
                    {
                        producao = new
                        {
                            minutos = Math.Round(analise.TempoProducao, 0),
                            horas = Math.Round(analise.TempoProducao / 60, 1),
                            taxa = analise.TaxaProducao
                        },
                        ociosidade = new
                        {
                            minutos = Math.Round(analise.TempoOciosidade, 0),
                            horas = Math.Round(analise.TempoOciosidade / 60, 1),
                            taxa = analise.TaxaOciosidade,
                            periodos = analise.Ociosidades.Count
                        },
                        pausas = new
                        {
                            minutos = Math.Round(analise.TempoPausas, 0),
                            horas = Math.Round(analise.TempoPausas / 60, 1),
                            taxa = analise.TaxaPausas,
                            quantidade = analise.Pausas.Count
                        }
                    },
                    detalhesPausas = analise.Pausas.Select(p => new
                    {
                        jobUuid = p.JobUuid,
                        jobName = p.JobName,
                        inicio = p.InicioParada,
                        fim = p.FimParada,
                        duracaoMinutos = Math.Round(p.DuracaoMinutos, 1),
                        motivo = p.Motivo,
                        tipo = p.TipoEvento
                    }),
                    detalhesOciosidade = analise.Ociosidades.Select(o => new
                    {
                        inicio = o.Inicio,
                        fim = o.Fim,
                        duracaoMinutos = Math.Round(o.DuracaoMinutos, 1),
                        duracaoHoras = Math.Round(o.DuracaoMinutos / 60, 1),
                        contexto = o.Contexto,
                        ultimoJob = o.UltimoJobUuid,
                        proximoJob = o.ProximoJobUuid
                    })
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        /// <summary>
        /// GET /api/dashboard/analise/todas-impressoras?dataInicio=2026-01-01&dataFim=2026-01-31
        /// Analisa todas as impressoras e retorna comparativo
        /// </summary>
        [HttpGet("analise/todas-impressoras")]
        public async Task<IActionResult> AnalisarTodasImpressoras(
            [FromQuery] DateTime dataInicio,
            [FromQuery] DateTime dataFim)
        {
            try
            {
                var printers = await _ultimakerClient.GetPrintersAsync();
                var analises = new List<object>();

                foreach (var printer in printers)
                {
                    try
                    {
                        var analise = await _ultimakerClient.AnalyzeEventsAsync(printer.Id, dataInicio, dataFim);

                        analises.Add(new
                        {
                            impressoraId = analise.MachineId,
                            impressoraNome = analise.MachineName,
                            taxaProducao = analise.TaxaProducao,
                            taxaOciosidade = analise.TaxaOciosidade,
                            taxaPausas = analise.TaxaPausas,
                            horasProducao = Math.Round(analise.TempoProducao / 60, 1),
                            horasOciosidade = Math.Round(analise.TempoOciosidade / 60, 1),
                            horasPausas = Math.Round(analise.TempoPausas / 60, 1),
                            jobsFinalizados = analise.JobsFinalizados,
                            jobsAbortados = analise.JobsAbortados
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro ao analisar impressora {printer.Name}: {ex.Message}");
                    }
                }

                var totais = new
                {
                    taxaProducaoMedia = analises.Any()
                        ? Math.Round(analises.Average(a => (decimal)((dynamic)a).taxaProducao), 1)
                        : 0,
                    taxaOciosidadeMedia = analises.Any()
                        ? Math.Round(analises.Average(a => (decimal)((dynamic)a).taxaOciosidade), 1)
                        : 0,
                    taxaPausasMedia = analises.Any()
                        ? Math.Round(analises.Average(a => (decimal)((dynamic)a).taxaPausas), 1)
                        : 0
                };

                return Ok(new
                {
                    periodo = new { inicio = dataInicio, fim = dataFim },
                    totalImpressoras = analises.Count,
                    medias = totais,
                    impressoras = analises.OrderByDescending(a => ((dynamic)a).taxaProducao)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        /// <summary>
        /// GET /api/dashboard/eventos/job/{jobUuid}?machineId=1
        /// Retorna eventos específicos de um job
        /// </summary>
        [HttpGet("eventos/job/{jobUuid}")]
        public async Task<IActionResult> ObterEventosJob(
            string jobUuid,
            [FromQuery] int machineId)
        {
            try
            {
                var eventos = await _ultimakerClient.GetEventsByJobUuidAsync(machineId, jobUuid);

                return Ok(new
                {
                    jobUuid,
                    impressoraId = machineId,
                    totalEventos = eventos.Count,
                    eventos = eventos.Select(e => new
                    {
                        timestamp = e.Time,
                        mensagem = e.Message,
                        categoria = e.EventCategory,
                        parametros = e.Parameters
                    }).OrderBy(e => e.timestamp)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // ========================================
        // DEBUG (EXISTENTES)
        // ========================================

        [HttpGet("debug/impressoras")]
        public async Task<IActionResult> DebugImpressoras()
        {
            try
            {
                var printers = await _ultimakerClient.GetPrintersAsync();

                return Ok(new
                {
                    totalImpressoras = printers.Count,
                    impressoras = printers.Select(p => new
                    {
                        id = p.Id,
                        nome = p.Name,
                        ativa = p.IsActive,
                        hostname = p.BaseUrl
                    })
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        [HttpGet("debug/analise/{machineId}")]
        public async Task<IActionResult> DebugAnaliseImpressora(
            int machineId,
            [FromQuery] DateTime dataInicio,
            [FromQuery] DateTime dataFim)
        {
            try
            {
                Console.WriteLine($"🔍 DEBUG: Analisando impressora {machineId}");
                Console.WriteLine($"📅 Período: {dataInicio:yyyy-MM-dd} até {dataFim:yyyy-MM-dd}");

                var analise = await _ultimakerClient.AnalyzeEventsAsync(machineId, dataInicio, dataFim);

                return Ok(new
                {
                    debug = true,
                    machineId,
                    periodo = new { dataInicio, dataFim },
                    resultado = new
                    {
                        impressoraId = analise.MachineId,
                        impressoraNome = analise.MachineName,
                        taxaProducao = analise.TaxaProducao,
                        taxaOciosidade = analise.TaxaOciosidade,
                        taxaPausas = analise.TaxaPausas,
                        horasProducao = Math.Round(analise.TempoProducao / 60, 1),
                        horasOciosidade = Math.Round(analise.TempoOciosidade / 60, 1),
                        horasPausas = Math.Round(analise.TempoPausas / 60, 1),
                        jobsFinalizados = analise.JobsFinalizados,
                        jobsAbortados = analise.JobsAbortados
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    erro = ex.Message,
                    detalhes = ex.InnerException?.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        // ========================================
        // 📅 NOVOS ENDPOINTS - TIMELINE
        // ========================================

        /// <summary>
        /// GET /api/dashboard/timeline/mes-consolidado?ano=2026&mes=1
        /// Retorna resumo executivo do mês (todas impressoras)
        /// </summary>
        [HttpGet("timeline/mes-consolidado")]
        public async Task<IActionResult> ObterResumoMensalConsolidado(
            [FromQuery] int ano,
            [FromQuery] int mes)
        {
            try
            {
                var resumo = await _timelineService.ObterResumoConsolidado(ano, mes);

                return Ok(new
                {
                    periodo = resumo.Periodo,
                    // Métricas principais (compatível com card existente)
                    metricas = new
                    {
                        utilizacao = resumo.Utilizacao,
                        horasProdutivas = resumo.HorasProdutivas,
                        taxaSucesso = resumo.TaxaSucesso
                    },
                    // Distribuição do tempo (GARANTE 100%)
                    distribuicao = new
                    {
                        producao = new
                        {
                            horas = Math.Round(resumo.TempoProducaoTotal, 1),
                            taxa = resumo.TaxaProducao
                        },
                        pausas = new
                        {
                            horas = Math.Round(resumo.TempoPausasTotal, 1),
                            taxa = resumo.TaxaPausas
                        },
                        ociosidade = new
                        {
                            horas = Math.Round(resumo.TempoOciosidadeTotal, 1),
                            taxa = resumo.TaxaOciosidade
                        },
                        esperaOperador = new
                        {
                            horas = Math.Round(resumo.TempoEsperaOperadorTotal, 1),
                            taxa = resumo.TaxaEsperaOperador
                        },
                        manutencao = new
                        {
                            horas = Math.Round(resumo.TempoManutencaoTotal, 1),
                            taxa = resumo.TaxaManutencao
                        },
                        // ✅ Verificação de soma
                        totalTaxas = resumo.TaxaProducao + resumo.TaxaPausas + resumo.TaxaOciosidade +
                                    resumo.TaxaEsperaOperador + resumo.TaxaManutencao
                    },
                    // Top motivos consolidados
                    motivos = new
                    {
                        pausas = resumo.TopMotivosPausas.Select(m => new
                        {
                            motivo = m.MotivoDescricao,
                            horas = Math.Round((decimal)m.TempoTotal / 60, 1),
                            taxa = m.Percentual,
                            ocorrencias = m.Ocorrencias
                        }),
                        ociosidade = resumo.TopMotivosOciosidade.Select(m => new
                        {
                            motivo = m.MotivoDescricao,
                            horas = Math.Round((decimal)m.TempoTotal / 60, 1),
                            taxa = m.Percentual,
                            ocorrencias = m.Ocorrencias
                        }),
                        esperaOperador = resumo.TopMotivosEsperaOperador.Select(m => new
                        {
                            motivo = m.MotivoDescricao,
                            horas = Math.Round((decimal)m.TempoTotal / 60, 1),
                            taxa = m.Percentual,
                            ocorrencias = m.Ocorrencias
                        })
                    },
                    // ✅ ADICIONADO: Lista de impressoras para drill-down
                    impressoras = resumo.Impressoras.Select(imp => new
                    {
                        impressoraId = imp.MachineId,
                        impressoraNome = imp.MachineName,
                        taxaProducao = imp.TaxaProducao,
                        taxaPausas = imp.TaxaPausas,
                        taxaOciosidade = imp.TaxaOciosidade,
                        taxaEsperaOperador = imp.TaxaEsperaOperador,
                        taxaManutencao = imp.TaxaManutencao,
                        horasProducao = Math.Round(imp.TempoProducao / 60, 1),
                        horasPausas = Math.Round(imp.TempoPausas / 60, 1),
                        horasOciosidade = Math.Round(imp.TempoOciosidade / 60, 1),
                        jobsFinalizados = imp.JobsFinalizados,
                        jobsAbortados = imp.JobsAbortados,
                        taxaSucesso = imp.TaxaSucesso
                    }),
                    // Resumo por impressora (para drill-down)
                    totalImpressoras = resumo.Impressoras.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        /// <summary>
        /// GET /api/dashboard/timeline/mes-impressora/{machineId}?ano=2026&mes=1
        /// Retorna análise mensal de uma impressora específica
        /// </summary>
        [HttpGet("timeline/mes-impressora/{machineId}")]
        public async Task<IActionResult> ObterResumoMensalImpressora(
            int machineId,
            [FromQuery] int ano,
            [FromQuery] int mes)
        {
            try
            {
                var resumo = await _timelineService.ObterResumoMensal(machineId, ano, mes);

                return Ok(new
                {
                    impressora = new
                    {
                        id = resumo.MachineId,
                        nome = resumo.MachineName
                    },
                    periodo = new
                    {
                        ano = resumo.Ano,
                        mes = resumo.Mes,
                        mesNome = resumo.MesNome
                    },
                    // Tempos e taxas (GARANTE 100%)
                    metricas = new
                    {
                        producao = new
                        {
                            horas = Math.Round(resumo.TempoProducao, 1),
                            taxa = resumo.TaxaProducao
                        },
                        pausas = new
                        {
                            horas = Math.Round(resumo.TempoPausas, 1),
                            taxa = resumo.TaxaPausas
                        },
                        ociosidade = new
                        {
                            horas = Math.Round(resumo.TempoOciosidade, 1),
                            taxa = resumo.TaxaOciosidade
                        },
                        esperaOperador = new
                        {
                            horas = Math.Round(resumo.TempoEsperaOperador, 1),
                            taxa = resumo.TaxaEsperaOperador
                        },
                        manutencao = new
                        {
                            horas = Math.Round(resumo.TempoManutencao, 1),
                            taxa = resumo.TaxaManutencao
                        },
                        totalTaxas = resumo.TaxaProducao + resumo.TaxaPausas + resumo.TaxaOciosidade +
                                    resumo.TaxaEsperaOperador + resumo.TaxaManutencao
                    },
                    // Jobs
                    jobs = new
                    {
                        finalizados = resumo.JobsFinalizados,
                        abortados = resumo.JobsAbortados,
                        taxaSucesso = resumo.TaxaSucesso
                    },
                    // Motivos do mês
                    motivos = resumo.Motivos
                        .OrderByDescending(m => m.TempoTotal)
                        .Select(m => new
                        {
                            status = m.Status.ToString(),
                            motivo = m.MotivoDescricao,
                            horas = Math.Round((decimal)m.TempoTotal / 60, 1),
                            taxa = m.Percentual,
                            ocorrencias = m.Ocorrencias
                        }),
                    // Calendário do mês (resumos diários)
                    calendario = resumo.Dias.Select(d => new
                    {
                        data = d.Data.ToString("yyyy-MM-dd"),
                        dia = d.Data.Day,
                        producao = new
                        {
                            minutos = Math.Round(d.TempoProducao, 0),
                            taxa = d.TaxaProducao
                        },
                        pausas = new
                        {
                            minutos = Math.Round(d.TempoPausas, 0),
                            taxa = d.TaxaPausas
                        },
                        ociosidade = new
                        {
                            minutos = Math.Round(d.TempoOciosidade, 0),
                            taxa = d.TaxaOciosidade
                        },
                        esperaOperador = new
                        {
                            minutos = Math.Round(d.TempoEsperaOperador, 0),
                            taxa = d.TaxaEsperaOperador
                        },
                        manutencao = new
                        {
                            minutos = Math.Round(d.TempoManutencao, 0),
                            taxa = d.TaxaManutencao
                        }
                    })
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        /// <summary>
        /// GET /api/dashboard/timeline/dia/{machineId}?data=2026-01-15
        /// Retorna resumo
        ///     /// <summary>
        /// GET /api/dashboard/timeline/dia/{machineId}?data=2026-01-15
        /// Retorna resumo de um dia específico
        /// </summary>
        [HttpGet("timeline/dia/{machineId}")]
        public async Task<IActionResult> ObterResumoDiario(
            int machineId,
            [FromQuery] DateTime data)
        {
            try
            {
                var resumo = await _timelineService.ObterResumoDiario(machineId, data);

                return Ok(new
                {
                    impressora = new
                    {
                        id = resumo.MachineId,
                        nome = resumo.MachineName
                    },
                    data = resumo.Data.ToString("yyyy-MM-dd"),
                    diaSemana = resumo.Data.ToString("dddd"),
                    // Resumo do dia (GARANTE 100%)
                    resumo = new
                    {
                        producao = new
                        {
                            minutos = Math.Round(resumo.TempoProducao, 0),
                            horas = Math.Round(resumo.TempoProducao / 60, 1),
                            taxa = resumo.TaxaProducao
                        },
                        pausas = new
                        {
                            minutos = Math.Round(resumo.TempoPausas, 0),
                            horas = Math.Round(resumo.TempoPausas / 60, 1),
                            taxa = resumo.TaxaPausas
                        },
                        ociosidade = new
                        {
                            minutos = Math.Round(resumo.TempoOciosidade, 0),
                            horas = Math.Round(resumo.TempoOciosidade / 60, 1),
                            taxa = resumo.TaxaOciosidade
                        },
                        esperaOperador = new
                        {
                            minutos = Math.Round(resumo.TempoEsperaOperador, 0),
                            horas = Math.Round(resumo.TempoEsperaOperador / 60, 1),
                            taxa = resumo.TaxaEsperaOperador
                        },
                        manutencao = new
                        {
                            minutos = Math.Round(resumo.TempoManutencao, 0),
                            horas = Math.Round(resumo.TempoManutencao / 60, 1),
                            taxa = resumo.TaxaManutencao
                        },
                        totalTaxas = resumo.TaxaProducao + resumo.TaxaPausas + resumo.TaxaOciosidade +
                                    resumo.TaxaEsperaOperador + resumo.TaxaManutencao
                    },
                    // Motivos do dia
                    motivos = resumo.Motivos.Select(m => new
                    {
                        status = m.Status.ToString(),
                        motivo = m.MotivoDescricao,
                        minutos = Math.Round((decimal)m.TempoTotal, 0),
                        horas = Math.Round((decimal)m.TempoTotal / 60, 1),
                        taxa = m.Percentual,
                        ocorrencias = m.Ocorrencias
                    }),
                    // Link para timeline horária
                    temTimeline = resumo.Timeline.Any()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        /// <summary>
        /// GET /api/dashboard/timeline/horaria/{machineId}?data=2026-01-15
        /// Retorna timeline hora a hora (estilo Teams)
        /// </summary>
        [HttpGet("timeline/horaria/{machineId}")]
        public async Task<IActionResult> ObterTimelineHoraria(
            int machineId,
            [FromQuery] DateTime data)
        {
            try
            {
                var resumo = await _timelineService.ObterResumoDiario(machineId, data);

                return Ok(new
                {
                    impressora = new
                    {
                        id = resumo.MachineId,
                        nome = resumo.MachineName
                    },
                    data = resumo.Data.ToString("yyyy-MM-dd"),
                    // Timeline bloco a bloco
                    timeline = resumo.Timeline.Select(b => new
                    {
                        inicio = b.Inicio.ToString("HH:mm"),
                        fim = b.Fim.ToString("HH:mm"),
                        duracaoMinutos = b.DuracaoMinutos,
                        status = b.Status.ToString(),
                        statusCor = ObterCorStatus(b.Status),
                        motivo = b.Motivo.ToString(),
                        motivoDescricao = b.Mensagem,
                        jobUuid = b.JobUuid,
                        jobName = b.JobName
                    }).OrderBy(b => b.inicio),
                    // Resumo consolidado do dia
                    resumoDia = new
                    {
                        producao = new
                        {
                            horas = Math.Round(resumo.TempoProducao / 60, 1),
                            taxa = resumo.TaxaProducao
                        },
                        pausas = new
                        {
                            horas = Math.Round(resumo.TempoPausas / 60, 1),
                            taxa = resumo.TaxaPausas
                        },
                        ociosidade = new
                        {
                            horas = Math.Round(resumo.TempoOciosidade / 60, 1),
                            taxa = resumo.TaxaOciosidade
                        },
                        esperaOperador = new
                        {
                            horas = Math.Round(resumo.TempoEsperaOperador / 60, 1),
                            taxa = resumo.TaxaEsperaOperador
                        },
                        manutencao = new
                        {
                            horas = Math.Round(resumo.TempoManutencao / 60, 1),
                            taxa = resumo.TaxaManutencao
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // ========================================
        // MÉTODO AUXILIAR PARA CORES
        // ========================================

        private string ObterCorStatus(StatusMaquina status)
        {
            return status switch
            {
                StatusMaquina.Producao => "green",       // 🟢
                StatusMaquina.Pausa => "yellow",         // 🟡
                StatusMaquina.Ociosidade => "red",       // 🔴
                StatusMaquina.EsperaOperador => "orange",// 🟠
                StatusMaquina.Manutencao => "blue",      // 🔵
                _ => "gray"
            };
        }
    }
}