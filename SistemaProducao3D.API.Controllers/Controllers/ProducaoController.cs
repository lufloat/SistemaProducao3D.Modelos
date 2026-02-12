using Business_Logic.Serviços.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace SistemaProducao3D.API.Controllers
{
    /// <summary>
    /// ProducaoController - VERSÃO CORRIGIDA
    /// ✅ Retorna DetalheMensal e ProducaoAnual
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ProducaoController : ControllerBase
    {
        private readonly IProducaoService _producaoService;
        private readonly IProdutoEspecificoService _produtoEspecificoService;
        private readonly IEquipamentoService _equipamentoService;

        public ProducaoController(
            IProducaoService producaoService,
            IProdutoEspecificoService produtoEspecificoService,
            IEquipamentoService equipamentoService)
        {
            _producaoService = producaoService;
            _produtoEspecificoService = produtoEspecificoService;
            _equipamentoService = equipamentoService;
        }

        /// <summary>
        /// GET /api/producao/mensal?ano=2025&mesInicio=1&mesFim=12
        /// ✅ ATUALIZADO: Retorna List<DetalheMensal>
        /// Campos: Produção, Protótipo, Abortados, Perdidos, % Falhas, % Abortados
        /// </summary>
        [HttpGet("mensal")]
        public async Task<IActionResult> ObterProducaoMensal(
            [FromQuery] int ano = 2025,
            [FromQuery] int mesInicio = 1,
            [FromQuery] int mesFim = 12)
        {
            try
            {
                Console.WriteLine($"📊 API: Requisição produção mensal {ano}/{mesInicio}-{mesFim}");

                var dados = await _producaoService.ObterProducaoMensalDetalhada(ano, mesInicio, mesFim);

                Console.WriteLine($"✅ API: Retornando {dados.Count} registros");

                return Ok(dados);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ API: Erro ao obter produção mensal: {ex.Message}");
                Console.WriteLine($"   Stack: {ex.StackTrace}");

                return StatusCode(500, new
                {
                    error = "Erro ao processar produção mensal",
                    message = ex.Message,
                    details = ex.InnerException?.Message
                });
            }
        }

        /// <summary>
        /// GET /api/producao/anual?anoInicio=2019&anoFim=2025
        /// ✅ ATUALIZADO: Retorna List<ProducaoAnual>
        /// </summary>
        [HttpGet("anual")]
        public async Task<IActionResult> ObterProducaoAnual(
            [FromQuery] int anoInicio = 2019,
            [FromQuery] int anoFim = 2025)
        {
            try
            {
                Console.WriteLine($"📊 API: Requisição produção anual {anoInicio}-{anoFim}");

                var dados = await _producaoService.ObterProducaoAnual(anoInicio, anoFim);

                Console.WriteLine($"✅ API: Retornando {dados.Count} anos");

                return Ok(dados);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro ao obter produção anual: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// GET /api/producao/recondicionados?ano=2025
        /// </summary>
        [HttpGet("recondicionados")]
        public async Task<IActionResult> ObterRecondicionados([FromQuery] int ano = 2025)
        {
            try
            {
                var dados = await _produtoEspecificoService.ObterRecondicionados(ano);
                return Ok(dados);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// GET /api/producao/placas?ano=2025
        /// </summary>
        [HttpGet("placas")]
        public async Task<IActionResult> ObterProducaoPlacas([FromQuery] int ano = 2025)
        {
            try
            {
                var dados = await _produtoEspecificoService.ObterProducaoPlacas(ano);
                return Ok(dados);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// GET /api/producao/equipamentos
        /// </summary>
        [HttpGet("equipamentos")]
        public async Task<IActionResult> ObterEquipamentos()
        {
            try
            {
                var equipamentos = await _equipamentoService.ObterEquipamentos();
                return Ok(equipamentos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}