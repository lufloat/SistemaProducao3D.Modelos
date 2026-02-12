using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace SistemaProducao3D.Integration.Ultimaker
{
    /// <summary>
    /// Cliente para API Ultimaker - Versão Compatível
    /// ✅ Mantém métodos existentes + adiciona análise de eventos
    /// </summary>
    public class UltimakerClient : IUltimakerClient
    {
        private readonly HttpClient _httpClient;
        private readonly UltimakerOptions _options;

        public UltimakerClient(HttpClient httpClient, IOptions<UltimakerOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        #region Métodos Existentes (MANTIDOS SEM ALTERAÇÃO)

        /// <summary>
        /// ✅ MÉTODO EXISTENTE - Retorna UltimakerPrinterConfig (não UltimakerPrinter)
        /// </summary>
        public Task<List<UltimakerPrinterConfig>> GetPrintersAsync()
        {
            var printers = _options.Printers
                .Select(p => new UltimakerPrinterConfig
                {
                    Id = p.Id,
                    Name = p.Name,
                    IsActive = true,
                    BaseUrl = p.BaseUrl
                })
                .ToList();

            return Task.FromResult(printers);
        }

        /// <summary>
        /// ✅ MÉTODO EXISTENTE - Obtém jobs de uma impressora
        /// </summary>
        public async Task<List<UltimakerJob>> GetJobsAsync(int printerId, DateTime startDate, DateTime endDate)
        {
            var config = _options.Printers.FirstOrDefault(p => p.Id == printerId);
            if (config == null || string.IsNullOrWhiteSpace(config.BaseUrl))
            {
                Console.WriteLine($"❌ Impressora {printerId} não encontrada");
                return new List<UltimakerJob>();
            }

            var url = $"{config.BaseUrl}/api/v1/history/print_jobs";

            try
            {
                Console.WriteLine($"🔍 {config.Name}: {url}");
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"❌ HTTP {response.StatusCode}");
                    return new List<UltimakerJob>();
                }

                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<List<Dictionary<string, JsonElement>>>(json);

                if (apiResponse == null || !apiResponse.Any())
                {
                    Console.WriteLine($"   ⚠️  Sem jobs");
                    return new List<UltimakerJob>();
                }

                var jobs = apiResponse
                    .Select(jobData => MapearJob(jobData))
                    .Where(job => job != null)
                    .Cast<UltimakerJob>()
                    .ToList();

                var jobsFiltrados = jobs
                    .Where(job =>
                        job.DatetimeStarted.HasValue &&
                        job.DatetimeStarted.Value >= startDate &&
                        job.DatetimeStarted.Value <= endDate)
                    .ToList();

                return jobsFiltrados;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro: {ex.Message}");
                return new List<UltimakerJob>();
            }
        }

        /// <summary>
        /// ✅ MÉTODO EXISTENTE - Obtém job por UUID
        /// </summary>
        public async Task<UltimakerJob?> GetJobByUuidAsync(string uuid)
        {
            foreach (var printer in _options.Printers)
            {
                if (string.IsNullOrWhiteSpace(printer.BaseUrl)) continue;

                var url = $"{printer.BaseUrl}/api/v1/history/print_jobs/{uuid}";

                try
                {
                    var response = await _httpClient.GetAsync(url);
                    if (!response.IsSuccessStatusCode) continue;

                    var json = await response.Content.ReadAsStringAsync();
                    var jobData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);

                    if (jobData != null)
                        return MapearJob(jobData);
                }
                catch
                {
                    continue;
                }
            }

            return null;
        }

        #endregion

        #region ✅ NOVOS MÉTODOS - ANÁLISE DE EVENTOS

        /// <summary>
        /// Obtém eventos de uma impressora em um período
        /// </summary>
        public async Task<List<UltimakerEvent>> GetEventsAsync(int printerId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var config = _options.Printers.FirstOrDefault(p => p.Id == printerId);
            if (config == null || string.IsNullOrWhiteSpace(config.BaseUrl))
            {
                Console.WriteLine($"❌ Impressora {printerId} não encontrada");
                return new List<UltimakerEvent>();
            }

            var url = $"{config.BaseUrl}/api/v1/history/events";

            try
            {
                Console.WriteLine($"📅 {config.Name}: Buscando eventos...");
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"❌ HTTP {response.StatusCode}");
                    return new List<UltimakerEvent>();
                }

                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<List<Dictionary<string, JsonElement>>>(json);

                if (apiResponse == null || !apiResponse.Any())
                {
                    Console.WriteLine($"   ⚠️  Sem eventos");
                    return new List<UltimakerEvent>();
                }

                var eventos = apiResponse
                    .Select(eventData => MapearEvento(eventData))
                    .Where(e => e != null)
                    .Cast<UltimakerEvent>()
                    .ToList();

                // Filtrar por data se fornecido
                if (startDate.HasValue || endDate.HasValue)
                {
                    eventos = eventos.Where(e =>
                    {
                        if (startDate.HasValue && e.Time < startDate.Value) return false;
                        if (endDate.HasValue && e.Time > endDate.Value) return false;
                        return true;
                    }).ToList();
                }

                Console.WriteLine($"   ✅ {eventos.Count} eventos carregados");
                return eventos;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro ao buscar eventos: {ex.Message}");
                return new List<UltimakerEvent>();
            }
        }

        /// <summary>
        /// Obtém eventos de um job específico
        /// </summary>
        public async Task<List<UltimakerEvent>> GetEventsByJobUuidAsync(int printerId, string jobUuid)
        {
            var allEvents = await GetEventsAsync(printerId);

            return allEvents
                .Where(e => e.GetJobUuid() == jobUuid)
                .OrderBy(e => e.Time)
                .ToList();
        }

        /// <summary>
        /// Analisa eventos e calcula métricas
        /// </summary>
        public async Task<EventAnalysis> AnalyzeEventsAsync(int printerId, DateTime startDate, DateTime endDate)
        {
            var config = _options.Printers.FirstOrDefault(p => p.Id == printerId);
            if (config == null)
            {
                throw new Exception($"Impressora {printerId} não encontrada");
            }

            Console.WriteLine($"\n🔍 Analisando eventos: {config.Name}");
            Console.WriteLine($"   Período: {startDate:dd/MM/yyyy} até {endDate:dd/MM/yyyy}");

            var eventos = await GetEventsAsync(printerId, startDate, endDate);
            var jobs = await GetJobsAsync(printerId, startDate, endDate);

            var analysis = new EventAnalysis
            {
                PeriodoInicio = startDate,
                PeriodoFim = endDate,
                MachineId = printerId,
                MachineName = config.Name,
                TotalEventos = eventos.Count
            };

            // Calcular tempo total do período em minutos
            analysis.TempoTotalPeriodo = (decimal)(endDate - startDate).TotalMinutes;

            // Ordenar eventos por tempo
            var eventosOrdenados = eventos.OrderBy(e => e.Time).ToList();

            // 1. CALCULAR TEMPO DE PRODUÇÃO
            var jobsFinalizados = jobs.Where(j => j.DatetimeFinished.HasValue).ToList();
            analysis.JobsIniciados = eventos.Count(e => e.IsPrintStarted);
            analysis.JobsFinalizados = eventos.Count(e => e.IsPrintFinished);
            analysis.JobsAbortados = eventos.Count(e => e.IsPrintAborted);

            decimal tempoProducaoTotal = 0;
            foreach (var job in jobsFinalizados)
            {
                if (job.DatetimeStarted.HasValue && job.DatetimeFinished.HasValue)
                {
                    var duracao = (decimal)(job.DatetimeFinished.Value - job.DatetimeStarted.Value).TotalMinutes;
                    tempoProducaoTotal += duracao;
                }
            }
            analysis.TempoProducao = tempoProducaoTotal;

            // 2. IDENTIFICAR PAUSAS
            var pausas = new List<PauseDetail>();
            foreach (var job in jobs)
            {
                var eventosJob = eventosOrdenados
                    .Where(e => e.GetJobUuid() == job.Uuid)
                    .OrderBy(e => e.Time)
                    .ToList();

                // ✅ Regra 1: paused → resumed
                for (int i = 0; i < eventosJob.Count; i++)
                {
                    var evento = eventosJob[i];

                    if (evento.EventCategory == "Paused")
                    {
                        // Procurar o próximo resumed
                        var eventoResume = eventosJob
                            .Skip(i + 1)
                            .FirstOrDefault(e => e.EventCategory == "Resumed");

                        if (eventoResume != null)
                        {
                            // ✅ PAUSA NORMAL: paused → resumed
                            var duracao = (decimal)(eventoResume.Time - evento.Time).TotalMinutes;

                            pausas.Add(new PauseDetail
                            {
                                JobUuid = job.Uuid,
                                JobName = job.Name,
                                InicioParada = evento.Time,
                                FimParada = eventoResume.Time,
                                DuracaoMinutos = duracao,
                                Motivo = "Pausa durante impressão",
                                TipoEvento = "Paused-Resumed"
                            });
                        }
                        else
                        {
                            // ✅ Regra 2: paused → aborted
                            var eventoAbort = eventosJob
                                .Skip(i + 1)
                                .FirstOrDefault(e => e.IsPrintAborted);

                            if (eventoAbort != null)
                            {
                                // ✅ PAUSA ATÉ O ABORT
                                var duracao = (decimal)(eventoAbort.Time - evento.Time).TotalMinutes;

                                pausas.Add(new PauseDetail
                                {
                                    JobUuid = job.Uuid,
                                    JobName = job.Name,
                                    InicioParada = evento.Time,
                                    FimParada = eventoAbort.Time,
                                    DuracaoMinutos = duracao,
                                    Motivo = "Pausa seguida de cancelamento",
                                    TipoEvento = "Paused-Aborted"
                                });
                            }
                            // ✅ Regra 3: Se não encontrou nem resumed nem aborted após paused,
                            // ignora (pode ser um caso incompleto nos logs)
                        }
                    }
                }

                // ✅ Regra 4: Jobs abortados SEM evento paused = 0 minutos de pausa
                // (Nada a fazer, pois não adicionamos nada se não houver paused)
            }

            analysis.Pausas = pausas;
            analysis.TempoPausas = pausas.Sum(p => p.DuracaoMinutos);

            // 3. CALCULAR OCIOSIDADE
            var ociosidades = new List<IdleDetail>();
            var eventosStart = eventosOrdenados.Where(e => e.IsPrintStarted).ToList();
            var eventosFinish = eventosOrdenados.Where(e => e.IsPrintFinished || e.IsPrintCleared).ToList();

            for (int i = 0; i < eventosFinish.Count; i++)
            {
                var fimJob = eventosFinish[i];
                var proximoInicio = eventosStart
                    .Where(e => e.Time > fimJob.Time)
                    .OrderBy(e => e.Time)
                    .FirstOrDefault();

                if (proximoInicio != null)
                {
                    var duracaoOcio = (decimal)(proximoInicio.Time - fimJob.Time).TotalMinutes;

                    // Considerar ociosidade apenas se > 5 minutos
                    if (duracaoOcio > 5)
                    {
                        ociosidades.Add(new IdleDetail
                        {
                            Inicio = fimJob.Time,
                            Fim = proximoInicio.Time,
                            DuracaoMinutos = duracaoOcio,
                            UltimoJobUuid = fimJob.GetJobUuid(),
                            ProximoJobUuid = proximoInicio.GetJobUuid(),
                            Contexto = fimJob.IsPrintCleared ? "Após limpeza" : "Entre jobs"
                        });
                    }
                }
            }

            analysis.Ociosidades = ociosidades;
            analysis.TempoOciosidade = ociosidades.Sum(o => o.DuracaoMinutos);

            // LOG
            Console.WriteLine($"   📊 Resultados:");
            Console.WriteLine($"      • Tempo Total: {analysis.TempoTotalPeriodo:N0} min");
            Console.WriteLine($"      • Tempo Produção: {analysis.TempoProducao:N0} min ({analysis.TaxaProducao}%)");
            Console.WriteLine($"      • Tempo Ociosidade: {analysis.TempoOciosidade:N0} min ({analysis.TaxaOciosidade}%)");
            Console.WriteLine($"      • Tempo Pausas: {analysis.TempoPausas:N0} min ({analysis.TaxaPausas}%)");

            return analysis;
        }

        #endregion

        #region Métodos Auxiliares de Mapeamento

        private UltimakerEvent? MapearEvento(Dictionary<string, JsonElement> eventData)
        {
            try
            {
                var evento = new UltimakerEvent
                {
                    Message = GetString(eventData, "message") ?? string.Empty,
                    Time = GetDateTime(eventData, "time") ?? DateTime.UtcNow,
                    TypeId = GetInt(eventData, "type_id") ?? 0
                };

                if (eventData.TryGetValue("parameters", out var paramsElement) &&
                    paramsElement.ValueKind == JsonValueKind.Array)
                {
                    evento.Parameters = paramsElement.EnumerateArray()
                        .Select(p => p.GetString() ?? string.Empty)
                        .ToList();
                }

                return evento;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro ao mapear evento: {ex.Message}");
                return null;
            }
        }

        private UltimakerJob? MapearJob(Dictionary<string, JsonElement> jobData)
        {
            try
            {
                var job = new UltimakerJob
                {
                    Uuid = GetString(jobData, "uuid") ?? string.Empty,
                    Name = GetString(jobData, "name") ?? string.Empty,
                    Result = GetString(jobData, "result"),
                    DatetimeStarted = GetDateTime(jobData, "datetime_started"),
                    DatetimeFinished = GetDateTime(jobData, "datetime_finished"),
                    CreatedAt = GetDateTime(jobData, "datetime_created"),
                    TimeElapsed = GetDecimal(jobData, "time_elapsed"),
                };

                // Material 0
                job.Material0Guid = GetGuid(jobData, "material_0_guid");
                var mat0Usage = GetDecimal(jobData, "material_0_usage");
                var mat0Amount = GetDecimal(jobData, "material_0_amount");
                job.Material0Amount = mat0Usage ?? mat0Amount ?? 0;

                // Material 1
                job.Material1Guid = GetGuid(jobData, "material_1_guid");
                var mat1Usage = GetDecimal(jobData, "material_1_usage");
                var mat1Amount = GetDecimal(jobData, "material_1_amount");
                job.Material1Amount = mat1Usage ?? mat1Amount ?? 0;

                // Nomes dos materiais
                if (jobData.TryGetValue("material_0", out var mat0Obj) && mat0Obj.ValueKind == JsonValueKind.Object)
                {
                    var mat0Dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(mat0Obj.GetRawText());
                    if (mat0Dict != null)
                    {
                        job.Material0Name = GetString(mat0Dict, "name") ?? "Material 0";
                        job.Material0Brand = GetString(mat0Dict, "brand") ?? "Generic";
                    }
                }

                if (jobData.TryGetValue("material_1", out var mat1Obj) && mat1Obj.ValueKind == JsonValueKind.Object)
                {
                    var mat1Dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(mat1Obj.GetRawText());
                    if (mat1Dict != null)
                    {
                        job.Material1Name = GetString(mat1Dict, "name") ?? "Material 1";
                        job.Material1Brand = GetString(mat1Dict, "brand") ?? "Generic";
                    }
                }

                return job;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro ao mapear job: {ex.Message}");
                return null;
            }
        }

        private string? GetString(Dictionary<string, JsonElement> data, string key)
        {
            if (data.TryGetValue(key, out var value) && value.ValueKind == JsonValueKind.String)
                return value.GetString();
            return null;
        }

        private DateTime? GetDateTime(Dictionary<string, JsonElement> data, string key)
        {
            var str = GetString(data, key);
            if (DateTime.TryParse(str, out var result))
                return DateTime.SpecifyKind(result, DateTimeKind.Utc);
            return null;
        }

        private decimal? GetDecimal(Dictionary<string, JsonElement> data, string key)
        {
            if (data.TryGetValue(key, out var value))
            {
                if (value.ValueKind == JsonValueKind.Number && value.TryGetDecimal(out var dec))
                    return dec;
                if (value.ValueKind == JsonValueKind.String && decimal.TryParse(value.GetString(), out var parsed))
                    return parsed;
            }
            return null;
        }

        private int? GetInt(Dictionary<string, JsonElement> data, string key)
        {
            if (data.TryGetValue(key, out var value))
            {
                if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var num))
                    return num;
                if (value.ValueKind == JsonValueKind.String && int.TryParse(value.GetString(), out var parsed))
                    return parsed;
            }
            return null;
        }

        private Guid? GetGuid(Dictionary<string, JsonElement> data, string key)
        {
            var str = GetString(data, key);
            if (Guid.TryParse(str, out var guid))
                return guid;
            return null;
        }

        #endregion
    }
}
