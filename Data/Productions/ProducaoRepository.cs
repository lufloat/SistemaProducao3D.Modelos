// ========================================
// ProducaoRepository.cs
// Localização: Business_Logic/Repositories/ProducaoRepository.cs
// ========================================
using Business_Logic.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using SistemaProducao3D.Data.Context;
using SistemaProducao3D.Modelos.Modelos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Business_Logic.Repositories
{
    public class ProducaoRepository : IProducaoRepository
    {
        private readonly DatabaseContext _context;

        public ProducaoRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task InserirAsync(MesaProducao producao)
        {
            NormalizarDatasParaUtc(producao);
            await _context.MesasProducao.AddAsync(producao);
            await _context.SaveChangesAsync();
        }

        public async Task AtualizarAsync(MesaProducao producao)
        {
            NormalizarDatasParaUtc(producao);
            _context.MesasProducao.Update(producao);
            await _context.SaveChangesAsync();
        }

        public async Task<MesaProducao?> ObterPorUuid(string uuid)
        {
            var resultado = await _context.MesasProducao
                .FirstOrDefaultAsync(p => p.UltimakerJobUuid == uuid);

            if (resultado != null)
                NormalizarDatasParaUtc(resultado);

            return resultado;
        }

        public async Task<List<MesaProducao>> ObterPorPeriodo(int? mes, int? ano)
        {
            var query = _context.MesasProducao.AsQueryable();

            if (ano.HasValue)
                query = query.Where(p => p.DatetimeStarted.Year == ano.Value);

            if (mes.HasValue)
                query = query.Where(p => p.DatetimeStarted.Month == mes.Value);

            var resultados = await query.ToListAsync();

            foreach (var item in resultados)
                NormalizarDatasParaUtc(item);

            return resultados;
        }

        public async Task<List<MesaProducao>> ObterPorIntervalo(DateTime inicio, DateTime fim)
        {
            inicio = ForceUtc(inicio);
            fim = ForceUtc(fim);

            var resultados = await _context.MesasProducao
                .Where(p => p.DatetimeStarted >= inicio && p.DatetimeStarted <= fim)
                .ToListAsync();

            foreach (var item in resultados)
                NormalizarDatasParaUtc(item);

            return resultados;
        }

        public async Task<List<MesaProducao>> ObterMultiplosMeses(int anoInicio, int mesInicio, int quantidadeMeses)
        {
            var inicio = new DateTime(anoInicio, mesInicio, 1, 0, 0, 0, DateTimeKind.Utc);
            var fim = inicio.AddMonths(quantidadeMeses);

            var resultados = await _context.MesasProducao
                .Where(p => p.DatetimeStarted >= inicio && p.DatetimeStarted < fim)
                .ToListAsync();

            foreach (var item in resultados)
                NormalizarDatasParaUtc(item);

            return resultados;
        }

        public async Task<List<MesaProducao>> ObterPorPeriodoEImpressora(
            int? mes,
            int? ano,
            int machineId)
        {
            var query = _context.MesasProducao
                .Where(p => p.MachineId == machineId);

            if (ano.HasValue)
                query = query.Where(p => p.DatetimeStarted.Year == ano.Value);

            if (mes.HasValue)
                query = query.Where(p => p.DatetimeStarted.Month == mes.Value);

            var resultados = await query.ToListAsync();

            foreach (var item in resultados)
                NormalizarDatasParaUtc(item);

            return resultados;
        }

        // ========================================
        // NOVOS MÉTODOS PARA TIMELINE
        // ========================================

        /// <summary>
        /// Obtém todos os jobs de uma máquina em uma data específica
        /// </summary>
        public async Task<List<MesaProducao>> ObterJobsPorMaquinaEData(int machineId, DateTime data)
        {
            // Garantir que a data está em UTC
            var dataUtc = ForceUtc(data.Date);
            var proximoDia = dataUtc.AddDays(1);

            var resultados = await _context.MesasProducao
                .Where(p => p.MachineId == machineId &&
                           p.DatetimeStarted >= dataUtc &&
                           p.DatetimeStarted < proximoDia)
                .OrderBy(p => p.DatetimeStarted)
                .ToListAsync();

            foreach (var item in resultados)
                NormalizarDatasParaUtc(item);

            return resultados;
        }

        /// <summary>
        /// Obtém todos os jobs de uma máquina em um período
        /// </summary>
        public async Task<List<MesaProducao>> ObterJobsPorMaquinaEPeriodo(
            int machineId,
            DateTime dataInicio,
            DateTime dataFim)
        {
            var inicioUtc = ForceUtc(dataInicio);
            var fimUtc = ForceUtc(dataFim);

            var resultados = await _context.MesasProducao
                .Where(p => p.MachineId == machineId &&
                           p.DatetimeStarted >= inicioUtc &&
                           p.DatetimeStarted <= fimUtc)
                .OrderBy(p => p.DatetimeStarted)
                .ToListAsync();

            foreach (var item in resultados)
                NormalizarDatasParaUtc(item);

            return resultados;
        }

        // ========================================
        // MÉTODOS AUXILIARES
        // ========================================

        private static void NormalizarDatasParaUtc(MesaProducao producao)
        {
            producao.DatetimeStarted = ForceUtc(producao.DatetimeStarted);

            if (producao.DatetimeFinished.HasValue)
                producao.DatetimeFinished = ForceUtc(producao.DatetimeFinished.Value);
        }

        private static DateTime ForceUtc(DateTime dateTime)
        {
            return dateTime.Kind switch
            {
                DateTimeKind.Utc => dateTime,
                DateTimeKind.Local => dateTime.ToUniversalTime(),
                DateTimeKind.Unspecified => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc),
                _ => dateTime
            };
        }
    }
}