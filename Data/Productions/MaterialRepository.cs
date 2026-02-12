using Business_Logic.Repositories.Interfaces;
using Business_Logic.Services;
using Microsoft.EntityFrameworkCore;
using SistemaProducao3D.Data.Context;
using SistemaProducao3D.Modelos.Modelos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Business_Logic.Repositories
{
    public class MaterialRepository : IMaterialRepository
    {
        private readonly DatabaseContext _context;
        private readonly UltimakerApiService _ultimakerApi;
        private const decimal DENSIDADE_PADRAO = 1.24m;

        public MaterialRepository(DatabaseContext context, UltimakerApiService ultimakerApi)
        {
            _context = context;
            _ultimakerApi = ultimakerApi;
        }

        public async Task<Material?> ObterPorGuid(Guid materialGuid)
        {
            return await _context.Materiais
                .FirstOrDefaultAsync(m => m.UltimakerMaterialGuid == materialGuid);
        }

        public async Task<Material> ObterOuCriarMaterial(
            Guid materialGuid,
            string nomeMaterial,
            string fabricante)
        {
            var material = await ObterPorGuid(materialGuid);

            if (material != null)
                return material;

            Console.WriteLine($"[MATERIAL] Criando novo material: GUID {materialGuid}");

            var materialInfo = await _ultimakerApi.ObterInformacoesMaterial(materialGuid);

            decimal densidade = DENSIDADE_PADRAO;
            string nome = nomeMaterial ?? "Material Desconhecido";
            string fab = fabricante ?? "Generic";

            if (materialInfo != null)
            {
                if (materialInfo.Densidade.HasValue)
                {
                    densidade = materialInfo.Densidade.Value;
                    Console.WriteLine($"[MATERIAL] Densidade da API: {densidade} g/cm3");
                }

                if (!string.IsNullOrEmpty(materialInfo.Nome))
                {
                    nome = materialInfo.Nome;
                    Console.WriteLine($"[MATERIAL] Nome da API: {nome}");
                }

                if (!string.IsNullOrEmpty(materialInfo.Fabricante))
                {
                    fab = materialInfo.Fabricante;
                    Console.WriteLine($"[MATERIAL] Fabricante da API: {fab}");
                }
            }
            else
            {
                Console.WriteLine($"[MATERIAL] Informacoes completas nao encontradas, buscando apenas densidade...");
                var densidadeApi = await _ultimakerApi.ObterDensidadeMaterial(materialGuid);

                if (densidadeApi.HasValue)
                {
                    densidade = densidadeApi.Value;
                    Console.WriteLine($"[MATERIAL] Densidade encontrada: {densidade} g/cm3");
                }
                else
                {
                    Console.WriteLine($"[MATERIAL] Densidade nao encontrada na API, usando padrao: {DENSIDADE_PADRAO} g/cm3");
                }
            }

            var novoMaterial = new Material
            {
                Id = Guid.NewGuid(),
                UltimakerMaterialGuid = materialGuid,
                Nome = nome,
                Fabricante = fab,
                Densidade = densidade,
                CreatedAt = DateTime.UtcNow
            };

            _context.Materiais.Add(novoMaterial);
            await _context.SaveChangesAsync();

            Console.WriteLine($"[MATERIAL] Material criado com sucesso:");
            Console.WriteLine($"[MATERIAL]   Nome: {novoMaterial.Nome}");
            Console.WriteLine($"[MATERIAL]   Fabricante: {novoMaterial.Fabricante}");
            Console.WriteLine($"[MATERIAL]   Densidade: {novoMaterial.Densidade} g/cm3");
            Console.WriteLine($"[MATERIAL]   GUID: {novoMaterial.UltimakerMaterialGuid}");

            return novoMaterial;
        }

        public async Task<List<Material>> ListarTodos()
        {
            return await _context.Materiais
                .OrderBy(m => m.Nome)
                .ToListAsync();
        }

        public async Task<int> AtualizarDensidadesExistentes()
        {
            Console.WriteLine("[MATERIAL] Iniciando atualizacao de densidades...");

            var materiais = await _context.Materiais.ToListAsync();
            int atualizados = 0;
            int naoEncontrados = 0;

            Console.WriteLine($"[MATERIAL] {materiais.Count} materiais no banco de dados");

            foreach (var material in materiais)
            {
                Console.WriteLine($"[MATERIAL] Verificando: {material.Nome} (densidade atual: {material.Densidade})");

                var densidadeApi = await _ultimakerApi.ObterDensidadeMaterial(material.UltimakerMaterialGuid);

                if (densidadeApi.HasValue)
                {
                    if (material.Densidade != densidadeApi.Value)
                    {
                        var densidadeAntiga = material.Densidade;
                        material.Densidade = densidadeApi.Value;
                        atualizados++;

                        Console.WriteLine($"[MATERIAL]   Atualizado: {densidadeAntiga} -> {densidadeApi.Value} g/cm3");
                    }
                    else
                    {
                        Console.WriteLine($"[MATERIAL]   Densidade ja esta correta");
                    }
                }
                else
                {
                    naoEncontrados++;
                    Console.WriteLine($"[MATERIAL]   Densidade nao encontrada na API");
                }
            }

            if (atualizados > 0)
            {
                await _context.SaveChangesAsync();
                Console.WriteLine($"[MATERIAL] Atualizacao concluida: {atualizados} material(is) atualizado(s)");
            }
            else
            {
                Console.WriteLine($"[MATERIAL] Nenhum material precisou ser atualizado");
            }

            if (naoEncontrados > 0)
            {
                Console.WriteLine($"[MATERIAL] {naoEncontrados} material(is) nao encontrado(s) na API");
            }

            return atualizados;
        }
    }
}
