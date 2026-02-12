using Business_Logic.Serviços.Sync;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace SistemaProducao3D.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SyncController : ControllerBase
    {
        private readonly ISyncService _syncService;

        public SyncController(ISyncService syncService)
        {
            _syncService = syncService;
        }

        /// <summary>
        /// Sincronizar um mês específico
        /// POST /api/sync/mes?ano=2025&mes=12
        /// </summary>
        [HttpPost("mes")]
        public async Task<IActionResult> SincronizarMes(
            [FromQuery] int ano,
            [FromQuery] int mes)
        {
            try
            {
                if (mes < 1 || mes > 12)
                    return BadRequest(new { erro = "Mês deve estar entre 1 e 12" });

                await _syncService.SincronizarMesAsync(ano, mes);

                return Ok(new
                {
                    mensagem = $"Mês {mes:00}/{ano} sincronizado com sucesso!",
                    ano = ano,
                    mes = mes
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    erro = ex.Message,
                    detalhes = ex.InnerException?.Message
                });
            }
        }

        /// <summary>
        /// Sincronizar um período específico
        /// POST /api/sync/periodo?anoInicio=2025&mesInicio=1&anoFim=2025&mesFim=12
        /// </summary>
        [HttpPost("periodo")]
        public async Task<IActionResult> SincronizarPeriodo(
            [FromQuery] int anoInicio,
            [FromQuery] int mesInicio,
            [FromQuery] int anoFim,
            [FromQuery] int mesFim)
        {
            try
            {
                if (mesInicio < 1 || mesInicio > 12)
                    return BadRequest(new { erro = "Mês inicial deve estar entre 1 e 12" });

                if (mesFim < 1 || mesFim > 12)
                    return BadRequest(new { erro = "Mês final deve estar entre 1 e 12" });

                await _syncService.SincronizarPeriodoAsync(anoInicio, mesInicio, anoFim, mesFim);

                return Ok(new
                {
                    mensagem = $"Período sincronizado: {mesInicio:00}/{anoInicio} até {mesFim:00}/{anoFim}",
                    inicio = new { ano = anoInicio, mes = mesInicio },
                    fim = new { ano = anoFim, mes = mesFim }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    erro = ex.Message,
                    detalhes = ex.InnerException?.Message
                });
            }
        }

        /// <summary>
        /// Sincronizar um ano completo
        /// POST /api/sync/ano?ano=2025
        /// </summary>
        [HttpPost("ano")]
        public async Task<IActionResult> SincronizarAno([FromQuery] int ano)
        {
            try
            {
                await _syncService.SincronizarPeriodoAsync(ano, 1, ano, 12);

                return Ok(new
                {
                    mensagem = $"Ano {ano} sincronizado com sucesso!",
                    ano = ano
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    erro = ex.Message,
                    detalhes = ex.InnerException?.Message
                });
            }
        }

        /// <summary>
        /// Sincronizar job específico por UUID
        /// POST /api/sync/job/{uuid}
        /// </summary>
        [HttpPost("job/{uuid}")]
        public async Task<IActionResult> SincronizarJob(string uuid)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(uuid))
                    return BadRequest(new { erro = "UUID inválido" });

                await _syncService.SincronizarJobAsync(uuid);

                return Ok(new
                {
                    mensagem = $"Job {uuid} sincronizado com sucesso!",
                    uuid = uuid
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    erro = ex.Message,
                    detalhes = ex.InnerException?.Message
                });
            }
        }

        /// <summary>
        /// Sincronizar últimos 3 meses
        /// POST /api/sync/ultimos-meses
        /// </summary>
        [HttpPost("ultimos-meses")]
        public async Task<IActionResult> SincronizarUltimosMeses()
        {
            try
            {
                var hoje = DateTime.Now;
                var mesAtual = hoje.Month;
                var anoAtual = hoje.Year;

                // Calcula 3 meses atrás
                var inicio = hoje.AddMonths(-3);
                var mesInicio = inicio.Month;
                var anoInicio = inicio.Year;

                await _syncService.SincronizarPeriodoAsync(anoInicio, mesInicio, anoAtual, mesAtual);

                return Ok(new
                {
                    mensagem = "Últimos 3 meses sincronizados com sucesso!",
                    inicio = new { ano = anoInicio, mes = mesInicio },
                    fim = new { ano = anoAtual, mes = mesAtual }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    erro = ex.Message,
                    detalhes = ex.InnerException?.Message
                });
            }
        }
    }
}