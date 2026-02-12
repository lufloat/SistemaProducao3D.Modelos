using Business_Logic.Serviços;
using Business_Logic.Serviços.Interfaces;
using Business_Logic.Serviços.Sync;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SistemaProducao3D.Business.Services;
using SistemaProducao3D.Data.Context;
using SistemaProducao3D.Data.Repositories;
using SistemaProducao3D.Integration.Ultimaker;
using System;
using System.Threading.Tasks;

namespace SistemaProducao3D.ConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Configurar serviços
            var services = ConfigurarServicos();
            var serviceProvider = services.BuildServiceProvider();

            Console.WriteLine("==============================================");
            Console.WriteLine("   SISTEMA DE PRODUÇÃO 3D - ULTIMAKER");
            Console.WriteLine("==============================================\n");

            bool continuar = true;
            while (continuar)
            {
                MostrarMenu();
                var opcao = Console.ReadLine();

                switch (opcao)
                {
                    case "1":
                        await SincronizarDados(serviceProvider);
                        break;
                    case "2":
                        await DashboardSKU(serviceProvider);
                        break;
                    case "3":
                        await ProducaoAnual(serviceProvider);
                        break;
                    case "4":
                        await ProducaoMensal(serviceProvider);
                        break;
                    case "5":
                        await VisaoGeral(serviceProvider);
                        break;
                    case "6":
                        await CardsKg(serviceProvider);
                        break;
                    case "7":
                        await CardsCapacidade(serviceProvider);
                        break;
                    case "8":
                        await ProdutosEspecificos(serviceProvider);
                        break;
                    case "9":
                        await Equipamentos(serviceProvider);
                        break;
                    case "0":
                        continuar = false;
                        break;
                    default:
                        Console.WriteLine("Opção inválida!");
                        break;
                }

                if (continuar)
                {
                    Console.WriteLine("\nPressione qualquer tecla para continuar...");
                    Console.ReadKey();
                    Console.Clear();
                }
            }

            Console.WriteLine("\nEncerrando sistema...");
        }

        static void MostrarMenu()
        {
            Console.WriteLine("\n╔═══════════════════════════════════════╗");
            Console.WriteLine("║           MENU PRINCIPAL              ║");
            Console.WriteLine("╠═══════════════════════════════════════╣");
            Console.WriteLine("║ 1 - Sincronizar Dados Ultimaker       ║");
            Console.WriteLine("║ 2 - Dashboard SKU                     ║");
            Console.WriteLine("║ 3 - Produção Anual                    ║");
            Console.WriteLine("║ 4 - Produção Mensal Detalhada         ║");
            Console.WriteLine("║ 5 - Visão Geral                       ║");
            Console.WriteLine("║ 6 - Cards de Peso (kg)                ║");
            Console.WriteLine("║ 7 - Cards de Capacidade               ║");
            Console.WriteLine("║ 8 - Produtos Específicos              ║");
            Console.WriteLine("║ 9 - Equipamentos                      ║");
            Console.WriteLine("║ 0 - Sair                              ║");
            Console.WriteLine("╚═══════════════════════════════════════╝");
            Console.Write("\nEscolha uma opção: ");
        }

        static async Task SincronizarDados(ServiceProvider serviceProvider)
        {
            var syncService = serviceProvider.GetService<IsyncService>();

            Console.WriteLine("\n╔═══════════════════════════════════════╗");
            Console.WriteLine("║      SINCRONIZAÇÃO DE DADOS           ║");
            Console.WriteLine("╚═══════════════════════════════════════╝\n");

            Console.WriteLine("1 - Sincronizar mês específico");
            Console.WriteLine("2 - Sincronizar período");
            Console.WriteLine("3 - Sincronizar ano completo (2025)");
            Console.Write("\nEscolha: ");

            var opcao = Console.ReadLine();

            switch (opcao)
            {
                case "1":
                    Console.Write("Ano: ");
                    var ano = int.Parse(Console.ReadLine());
                    Console.Write("Mês (1-12): ");
                    var mes = int.Parse(Console.ReadLine());
                    await syncService.SincronizarMesAsync(ano, mes);
                    break;

                case "2":
                    Console.Write("Mês início (1-12): ");
                    var mesInicio = int.Parse(Console.ReadLine());
                    Console.Write("Ano início: ");
                    var anoInicio = int.Parse(Console.ReadLine());
                    Console.Write("Mês fim (1-12): ");
                    var mesFim = int.Parse(Console.ReadLine());
                    Console.Write("Ano fim: ");
                    var anoFim = int.Parse(Console.ReadLine());
                    await syncService.SincronizarPeriodoAsync(mesInicio, anoInicio);
                    await syncService.SincronizarPeriodoAsync(mesFim, anoFim);
                    break;

                case "3":
                    for (int m = 1; m <= 12; m++)
                    {
                        await syncService.SincronizarMesAsync(2025, m);
                    }
                    break;
            }
        }

        static async Task DashboardSKU(ServiceProvider serviceProvider)
        {
            var service = serviceProvider.GetService<IDashboardSKUService>();

            Console.WriteLine("\n╔═══════════════════════════════════════╗");
            Console.WriteLine("║         DASHBOARD SKU                 ║");
            Console.WriteLine("╚═══════════════════════════════════════╝\n");

            var metricas = service.ObterMetricasKPI(1, DateTime.Now.Month, 2025);

            Console.WriteLine($"SKUs Totais: {metricas.SKusTotais}");
            Console.WriteLine($"Meta SKUs: {metricas.MetaSkus}");
            Console.WriteLine($"Progresso: {metricas.ProgressoSkus:F1}%");
            Console.WriteLine($"Variação SKUs: {metricas.VariacaoSkus:F1}%");
            Console.WriteLine();
            Console.WriteLine($"Produção Total: {metricas.Producao}");
            Console.WriteLine($"Variação Produção: {metricas.VariacaoProducao:F1}%");
            Console.WriteLine();
            Console.WriteLine($"Taxa de Sucesso: {metricas.TaxaSucesso:F1}%");
            Console.WriteLine($"Variação Taxa: {metricas.VariacaoTaxaSucesso:F1}%");
            Console.WriteLine();
            Console.WriteLine($"Protótipos: {metricas.Prototipos}");
            Console.WriteLine($"Variação Protótipos: {metricas.VariacaoPrototipos:F1}%");
            Console.WriteLine();
            Console.WriteLine($"Peças: {metricas.Pecas}");
            Console.WriteLine($"Ferramentas e Diversos: {metricas.FerramentasDiversos}");
            Console.WriteLine($"Novos SKUs: {metricas.NovosSkus}");

            Console.WriteLine("\n--- Evolução de SKUs (Mensal) ---");
            var evolucao = service.ObterEvolucaoSKUs(1, DateTime.Now.Month, 2024, 2025);
            foreach (var item in evolucao)
            {
                Console.WriteLine($"{item.Periodo}: {item.Valor} SKUs");
            }
        }

        static async Task ProducaoAnual(ServiceProvider serviceProvider)
        {
            var service = serviceProvider.GetService<IProducaoService>();

            Console.WriteLine("\n╔═══════════════════════════════════════╗");
            Console.WriteLine("║        PRODUÇÃO ANUAL                 ║");
            Console.WriteLine("╚═══════════════════════════════════════╝\n");

            var producao = service.ObterProducaoAnual(2019, 2025);

            Console.WriteLine("ANO  | PRODUÇÃO | PROTÓTIPO");
            Console.WriteLine("-----|----------|----------");
            foreach (var item in producao)
            {
                Console.WriteLine($"{item.Ano} | {item.Producao,8} | {item.Prototipo,9}");
            }
        }

        static async Task ProducaoMensal(ServiceProvider serviceProvider)
        {
            var service = serviceProvider.GetService<IProducaoService>();

            Console.WriteLine("\n╔═══════════════════════════════════════╗");
            Console.WriteLine("║    PRODUÇÃO MENSAL DETALHADA          ║");
            Console.WriteLine("╚═══════════════════════════════════════╝\n");

            var producao = service.ObterProducaoMensalDetalhada(2025, 1, 12);

            Console.WriteLine("MÊS    | PROD PCS | PROD FDA | PROT PCS | PROT FDA | ERRO % | TOTAL | KG");
            Console.WriteLine("-------|----------|----------|----------|----------|--------|-------|-------");
            foreach (var item in producao)
            {
                Console.WriteLine($"{item.MesNome,-7}| {item.ProducaoPcs,8} | {item.ProducaoFda,8} | {item.PrototipoPcs,8} | {item.PrototipoFda,8} | {item.ErroPercent,5:F1}% | {item.TotalMes,5} | {item.TotalKg,5:F1}");
            }
        }

        static async Task VisaoGeral(ServiceProvider serviceProvider)
        {
            var service = serviceProvider.GetService<IVisaoGeralService>();

            Console.WriteLine("\n╔═══════════════════════════════════════╗");
            Console.WriteLine("║           VISÃO GERAL                 ║");
            Console.WriteLine("╚═══════════════════════════════════════╝\n");

            var producao = service.ObterProducaoMensal(2025, 1, 9);
            var prototipos = service.ObterPrototipoMensal(2025, 1, 9);
            var erros = service.ObterErrosMensais(2025, 1, 9);
            var peso = service.ObterPesoMensal(2025, 1, 9);

            Console.WriteLine("--- PRODUÇÃO MENSAL ---");
            foreach (var item in producao)
                Console.WriteLine($"{item.Periodo}: {item.Valor}");

            Console.WriteLine("\n--- PROTÓTIPOS MENSAL ---");
            foreach (var item in prototipos)
                Console.WriteLine($"{item.Periodo}: {item.Valor}");

            Console.WriteLine("\n--- ERROS MENSAIS ---");
            foreach (var item in erros)
                Console.WriteLine($"{item.Periodo}: {item.Valor}");

            Console.WriteLine("\n--- PESO MENSAL (kg) ---");
            foreach (var item in peso)
                Console.WriteLine($"{item.Periodo}: {item.Valor} kg");
        }

        static async Task CardsKg(ServiceProvider serviceProvider)
        {
            var service = serviceProvider.GetService<ICardService>();

            Console.WriteLine("\n╔═══════════════════════════════════════╗");
            Console.WriteLine("║       CARDS DE PESO (KG)              ║");
            Console.WriteLine("╚═══════════════════════════════════════╝\n");

            var cards = service.ObterCardsKg(2025, 1, 4);

            foreach (var card in cards)
            {
                Console.WriteLine($"╔══════════ {card.MesAno} ══════════╗");
                Console.WriteLine($"║ Produção:  {card.ProducaoKg,7:F2} kg ║");
                Console.WriteLine($"║ Protótipo: {card.PrototipoKg,7:F2} kg ║");
                Console.WriteLine($"║ Erros:     {card.ErrosKg,7:F2} kg ║");
                Console.WriteLine($"║ TOTAL:     {card.TotalKg,7:F2} kg ║");
                Console.WriteLine("╚════════════════════════════╝\n");
            }
        }

        static async Task CardsCapacidade(ServiceProvider serviceProvider)
        {
            var service = serviceProvider.GetService<ICardService>();

            Console.WriteLine("\n╔═══════════════════════════════════════╗");
            Console.WriteLine("║      CARDS DE CAPACIDADE              ║");
            Console.WriteLine("╚═══════════════════════════════════════╝\n");

            Console.Write("Número de máquinas: ");
            var numMaquinas = int.Parse(Console.ReadLine());

            var cards = service.ObterCardsCapacidade(2025, 1, 4, numMaquinas);

            foreach (var card in cards)
            {
                Console.WriteLine($"╔══════════ {card.MesAno} ══════════╗");
                Console.WriteLine($"║ Utilização:  {card.UtilizacaoPercent,5:F1}%   ║");
                Console.WriteLine($"║ Produtivas:  {card.Produtivas,7:F0}h  ║");
                Console.WriteLine($"║ Sucesso:     {card.SucessoPercent,5:F1}%   ║");
                Console.WriteLine("╚════════════════════════════╝\n");
            }
        }

        static async Task ProdutosEspecificos(ServiceProvider serviceProvider)
        {
            var service = serviceProvider.GetService<IProdutoEspecificoService>();

            Console.WriteLine("\n╔═══════════════════════════════════════╗");
            Console.WriteLine("║      PRODUTOS ESPECÍFICOS             ║");
            Console.WriteLine("╚═══════════════════════════════════════╝\n");

            Console.WriteLine("--- RECONDICIONADOS (2025) ---");
            var recond = service.ObterRecondicionados(2025);
            foreach (var item in recond.Where(x => x.Valor > 0))
                Console.WriteLine($"{item.Periodo}: {item.Valor}");

            Console.WriteLine("\n--- PLACAS (2025) ---");
            var placas = service.ObterProducaoPlacas(2025);
            foreach (var item in placas.Where(x => x.Valor > 0))
                Console.WriteLine($"{item.Periodo}: {item.Valor}");
        }

        static async Task Equipamentos(ServiceProvider serviceProvider)
        {
            var service = serviceProvider.GetService<IEquipamentoService>();

            Console.WriteLine("\n╔═══════════════════════════════════════╗");
            Console.WriteLine("║          EQUIPAMENTOS                 ║");
            Console.WriteLine("╚═══════════════════════════════════════╝\n");

            var equipamentos = service.ObterEquipamentos();

            foreach (var equip in equipamentos)
            {
                Console.WriteLine($"╔════════════════════════════════╗");
                Console.WriteLine($"║ {equip.Nome,-30} ║");
                Console.WriteLine($"║ Unidades Ativas: {equip.UnidadesAtivas,-13} ║");
                Console.WriteLine("╚════════════════════════════════╝\n");
            }
        }

        static ServiceCollection ConfigurarServicos()
        {
            var services = new ServiceCollection();

            // Configuração
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            // Database
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<DatabaseContext>(options =>
                options.UseNpgsql(connectionString));

            // Ultimaker Client
            var ultimakerUrl = configuration["Ultimaker:BaseUrl"];
            var ultimakerApiKey = configuration["Ultimaker:ApiKey"];
            services.AddSingleton<IUltimakerClient>(new UltimakerClient(ultimakerUrl, ultimakerApiKey));

            // Repositories
            services.AddScoped<IProducaoRepository, ProducaoRepository>();

            // Services
            services.AddScoped<IsyncService, SyncService>();
            services.AddScoped<ICalculoService, CalculoService>();
            services.AddScoped<ICardService, CardService>();
            services.AddScoped<IDashboardSKUService, DashboardSKUService>();
            services.AddScoped<IEquipamentoService, EquipamentoService>();
            services.AddScoped<IProducaoService, ProducaoService>();
            services.AddScoped<IProdutoEspecificoService, ProdutoEspecificoService>();
            services.AddScoped<IVisaoGeralService, VisaoGeralService>();

            return services;
        }
    }
}