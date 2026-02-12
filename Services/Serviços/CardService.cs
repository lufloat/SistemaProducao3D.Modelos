using Business_Logic.Repositories.Interfaces;
using Business_Logic.Serviços.Interfaces;
using Microsoft.Extensions.Options;
using SistemaProducao3D.Integration.Ultimaker;
using SistemaProducao3D.Modelos.Modelos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Business_Logic.Serviços
{
    public class CardService : ICardService
    {
        private readonly IProducaoRepository _repository;
        private readonly ICalculoService _calculo;
        private readonly IOptions<UltimakerOptions> _options;

        public CardService(
            IProducaoRepository repository,
            ICalculoService calculo,
            IOptions<UltimakerOptions> options)
        {
            _repository = repository;
            _calculo = calculo;
            _options = options;
        }

        public async Task<List<CardKg>> ObterCardsKg(int ano, int mesInicio, int mesFim)
        {
            var resultado = new List<CardKg>();

            for (int mes = mesInicio; mes <= mesFim; mes++)
            {
                var dados = await _repository.ObterPorPeriodo(mes, ano);
                var dadosUnicos = RemoverDuplicatas(dados);
                var jobsFinalizados = dadosUnicos.Where(d => d.IsFinished).ToList();

                var dadosProducao = jobsFinalizados.Where(d => d.IsSucess && !d.IsPrototype).ToList();
                var dadosPrototipo = jobsFinalizados.Where(d => d.IsSucess && d.IsPrototype).ToList();

                // ✅ SEPARAÇÃO DE ERROS
                var dadosFailed = jobsFinalizados.Where(d => d.IsFailed).ToList();
                var dadosAborted = jobsFinalizados.Where(d => d.IsAborted).ToList();

                var producaoKg = _calculo.MaterialUsado(dadosProducao) / 1000m;
                var prototipoKg = _calculo.MaterialUsado(dadosPrototipo) / 1000m;
                var failedKg = _calculo.MaterialUsado(dadosFailed) / 1000m;
                var abortedKg = _calculo.MaterialUsado(dadosAborted) / 1000m;
                var errosKg = failedKg + abortedKg; // Total de erros

                resultado.Add(new CardKg
                {
                    MesAno = new DateTime(ano, mes, 1).ToString("MMM/yy").ToUpper(),
                    ProducaoKg = Math.Round(producaoKg, 2),
                    PrototipoKg = Math.Round(prototipoKg, 2),
                    ErrosKg = Math.Round(errosKg, 2),
                    FailedKg = Math.Round(failedKg, 2),      // ⭐ NOVO
                    AbortedKg = Math.Round(abortedKg, 2),    // ⭐ NOVO
                    TotalKg = Math.Round(producaoKg + prototipoKg + errosKg, 2)
                });
            }

            return resultado;
        }

        public async Task<List<CardCapacidade>> ObterCardsCapacidade(
            int ano, int mesInicio, int mesFim, int numeroMaquinas)
        {
            var resultado = new List<CardCapacidade>();

            for (int mes = mesInicio; mes <= mesFim; mes++)
            {
                var dados = await _repository.ObterPorPeriodo(mes, ano);
                var dadosUnicos = RemoverDuplicatas(dados);
                var jobsFinalizados = dadosUnicos.Where(d => d.IsFinished).ToList();
                var dadosSucesso = jobsFinalizados.Where(d => d.IsSucess).ToList();

                var horasProdutivas = _calculo.TempoImpressao(dadosSucesso) / 3600m;

                var diasNoMes = DateTime.DaysInMonth(ano, mes);
                var capacidadeTotalHoras = numeroMaquinas * 24m * diasNoMes;

                var utilizacao = capacidadeTotalHoras > 0
                    ? (horasProdutivas / capacidadeTotalHoras) * 100
                    : 0;

                var taxaSucesso = jobsFinalizados.Any()
                    ? ((decimal)dadosSucesso.Count / jobsFinalizados.Count) * 100
                    : 0;

                resultado.Add(new CardCapacidade
                {
                    MesAno = new DateTime(ano, mes, 1).ToString("MMM/yy").ToUpper(),
                    UtilizacaoPercent = Math.Round(utilizacao, 1),
                    Produtivas = Math.Round(horasProdutivas, 0),
                    SucessoPercent = Math.Round(taxaSucesso, 1)
                });
            }

            return resultado;
        }

        public async Task<List<CardCapacidadePorImpressora>> ObterCapacidadePorImpressora(
            int ano, int mes)
        {
            var printers = ObterImpressorasAtivas();
            var resultado = new List<CardCapacidadePorImpressora>();

            var diasNoMes = DateTime.DaysInMonth(ano, mes);
            var capacidadePorImpressora = 24m * diasNoMes;

            foreach (var printer in printers)
            {
                var dados = await _repository.ObterPorPeriodoEImpressora(mes, ano, printer.Id);
                var dadosUnicos = RemoverDuplicatas(dados);
                var jobsFinalizados = dadosUnicos.Where(d => d.IsFinished).ToList();
                var dadosSucesso = jobsFinalizados.Where(d => d.IsSucess).ToList();

                var horasProdutivas = _calculo.TempoImpressao(dadosSucesso) / 3600m;
                var utilizacao = (horasProdutivas / capacidadePorImpressora) * 100;
                var taxaSucesso = jobsFinalizados.Any()
                    ? ((decimal)dadosSucesso.Count / jobsFinalizados.Count) * 100
                    : 0;

                resultado.Add(new CardCapacidadePorImpressora
                {
                    MachineId = printer.Id,
                    NomeImpressora = printer.Name,
                    UtilizacaoPercent = Math.Round(utilizacao, 1),
                    HorasProdutivas = Math.Round(horasProdutivas, 1),
                    TaxaSucessoPercent = Math.Round(taxaSucesso, 1)
                });
            }

            return resultado;
        }

        public async Task<List<CardKgPorImpressora>> ObterKgPorImpressora(
            int ano, int mes)
        {
            var printers = ObterImpressorasAtivas();
            var resultado = new List<CardKgPorImpressora>();

            foreach (var printer in printers)
            {
                var dados = await _repository.ObterPorPeriodoEImpressora(mes, ano, printer.Id);
                var dadosUnicos = RemoverDuplicatas(dados);
                var jobsFinalizados = dadosUnicos.Where(d => d.IsFinished).ToList();

                var dadosProducao = jobsFinalizados.Where(d => d.IsSucess && !d.IsPrototype).ToList();
                var dadosPrototipo = jobsFinalizados.Where(d => d.IsSucess && d.IsPrototype).ToList();

                // ✅ SEPARAÇÃO DE ERROS POR IMPRESSORA
                var dadosFailed = jobsFinalizados.Where(d => d.IsFailed).ToList();
                var dadosAborted = jobsFinalizados.Where(d => d.IsAborted).ToList();

                var producaoKg = _calculo.MaterialUsado(dadosProducao) / 1000m;
                var prototipoKg = _calculo.MaterialUsado(dadosPrototipo) / 1000m;
                var failedKg = _calculo.MaterialUsado(dadosFailed) / 1000m;
                var abortedKg = _calculo.MaterialUsado(dadosAborted) / 1000m;
                var errosKg = failedKg + abortedKg;

                resultado.Add(new CardKgPorImpressora
                {
                    MachineId = printer.Id.ToString(),
                    NomeImpressora = printer.Name,
                    ProducaoKg = Math.Round(producaoKg, 2),
                    PrototipoKg = Math.Round(prototipoKg, 2),
                    ErrosKg = Math.Round(errosKg, 2),
                    FailedKg = Math.Round(failedKg, 2),      // ⭐ NOVO
                    AbortedKg = Math.Round(abortedKg, 2),    // ⭐ NOVO
                    TotalKg = Math.Round(producaoKg + prototipoKg + errosKg, 2)
                });
            }

            return resultado;
        }

        private List<MesaProducao> RemoverDuplicatas(List<MesaProducao> dados)
        {
            var dadosUnicos = dados
                .GroupBy(d => d.UltimakerJobUuid)
                .Select(g => g.First())
                .ToList();

            if (dados.Count != dadosUnicos.Count)
            {
                Console.WriteLine($"⚠️ DUPLICATAS REMOVIDAS: {dados.Count} → {dadosUnicos.Count} registros únicos");
            }

            var jobsEmProgresso = dadosUnicos.Count(d => d.IsInProgress);
            if (jobsEmProgresso > 0)
            {
                Console.WriteLine($"ℹ️ Jobs em progresso (ignorados nos cálculos): {jobsEmProgresso}");
            }

            return dadosUnicos;
        }

        private List<UltimakerPrinterConfig> ObterImpressorasAtivas()
        {
            return _options.Value.Printers;
        }
    }
}