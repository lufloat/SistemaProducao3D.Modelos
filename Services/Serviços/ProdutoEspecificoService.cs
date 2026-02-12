using Business_Logic.Repositories.Interfaces;
using Business_Logic.Serviços.Interfaces;
using SistemaProducao3D.Modelos.Modelos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Business_Logic.Serviços
{
    public class ProdutoEspecificoService : IProdutoEspecificoService
    {
        private readonly IProducaoRepository _producaoRepository;

        public ProdutoEspecificoService(IProducaoRepository repository)
        {
            _producaoRepository = repository;
        }

        public async Task<List<DetalheMensal>> ObterRecondicionados(int ano)
        {
            var resultado = new List<DetalheMensal>();

            for (int mes = 1; mes <= 12; mes++)
            {
                var dados = await _producaoRepository.ObterPorPeriodo(mes, ano);
                var recondicionados = dados.Count(d => d.IsSucess && d.JobType == "Recondicionado");

                resultado.Add(new DetalheMensal
                {
                    Periodo = new DateTime(ano, mes, 1).ToString("MMM").ToLower() + ".",
                    Valor = recondicionados
                });
            }

            return resultado;
        }

        public async Task<List<DetalheMensal>> ObterProducaoPlacas(int ano)
        {
            var resultado = new List<DetalheMensal>();

            for (int mes = 1; mes <= 12; mes++)
            {
                var dados = await _producaoRepository.ObterPorPeriodo(mes, ano);
                var producaoPlacas = dados.Count(d => d.IsSucess && d.JobType == "Placas");

                resultado.Add(new DetalheMensal
                {
                    Periodo = new DateTime(ano, mes, 1).ToString("MMM").ToLower() + ".",
                    Valor = producaoPlacas
                });
            }

            return resultado;
        }
    }
}