using Microsoft.Extensions.Options;
using SistemaProducao3D.Integration.Ultimaker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Business_Logic.Services
{
    public class UltimakerApiService
    {
        private readonly HttpClient _httpClient;
        private readonly List<UltimakerPrinterConfig> _printers;

        public UltimakerApiService(HttpClient httpClient, IOptions<UltimakerOptions> options)
        {
            _httpClient = httpClient;
            _printers = options.Value.Printers;
        }

        public async Task<decimal?> ObterDensidadeMaterial(Guid materialGuid)
        {
            foreach (var printer in _printers)
            {
                try
                {
                    var url = $"{printer.BaseUrl}/api/v1/materials";

                    Console.WriteLine($"[API] Buscando densidade em {printer.Name}: {url}");

                    var response = await _httpClient.GetAsync(url);

                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"[API]   HTTP {response.StatusCode}");
                        continue;
                    }

                    var content = await response.Content.ReadAsStringAsync();
                    var materialsXml = JsonSerializer.Deserialize<List<string>>(content);

                    if (materialsXml == null || !materialsXml.Any())
                    {
                        Console.WriteLine($"[API]   Nenhum material encontrado");
                        continue;
                    }

                    Console.WriteLine($"[API]   {materialsXml.Count} materiais na API");

                    foreach (var xmlContent in materialsXml)
                    {
                        var densidade = ExtrairDensidadeDoXml(xmlContent, materialGuid);
                        if (densidade.HasValue)
                        {
                            Console.WriteLine($"[API]   Densidade {densidade.Value} encontrada para GUID {materialGuid}");
                            return densidade.Value;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[API]   Erro ao consultar {printer.Name}: {ex.Message}");
                }
            }

            Console.WriteLine($"[API] Densidade nao encontrada para material {materialGuid} em nenhuma das 6 impressoras");
            return null;
        }

        public async Task<MaterialInfo?> ObterInformacoesMaterial(Guid materialGuid)
        {
            foreach (var printer in _printers)
            {
                try
                {
                    var url = $"{printer.BaseUrl}/api/v1/materials";
                    var response = await _httpClient.GetAsync(url);

                    if (!response.IsSuccessStatusCode)
                        continue;

                    var content = await response.Content.ReadAsStringAsync();
                    var materialsXml = JsonSerializer.Deserialize<List<string>>(content);

                    if (materialsXml == null)
                        continue;

                    foreach (var xmlContent in materialsXml)
                    {
                        var materialInfo = ExtrairInformacaoCompleta(xmlContent, materialGuid);
                        if (materialInfo != null)
                        {
                            Console.WriteLine($"[API] Material completo encontrado: {materialInfo.Nome} (densidade: {materialInfo.Densidade})");
                            return materialInfo;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[API] Erro ao consultar {printer.Name}: {ex.Message}");
                }
            }

            return null;
        }

        private decimal? ExtrairDensidadeDoXml(string xmlContent, Guid materialGuid)
        {
            try
            {
                var doc = XDocument.Parse(xmlContent);
                XNamespace ns = "http://www.ultimaker.com/material";

                var guidElement = doc.Descendants(ns + "GUID").FirstOrDefault();
                if (guidElement == null)
                    return null;

                if (!Guid.TryParse(guidElement.Value, out var xmlGuid) || xmlGuid != materialGuid)
                    return null;

                var densityElement = doc.Descendants(ns + "density").FirstOrDefault();
                if (densityElement != null &&
                    decimal.TryParse(densityElement.Value,
                                   System.Globalization.NumberStyles.Any,
                                   System.Globalization.CultureInfo.InvariantCulture,
                                   out var density))
                {
                    return density;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[API] Erro ao processar XML: {ex.Message}");
            }

            return null;
        }

        private MaterialInfo? ExtrairInformacaoCompleta(string xmlContent, Guid materialGuid)
        {
            try
            {
                var doc = XDocument.Parse(xmlContent);
                XNamespace ns = "http://www.ultimaker.com/material";

                var guidElement = doc.Descendants(ns + "GUID").FirstOrDefault();
                if (guidElement == null || !Guid.TryParse(guidElement.Value, out var xmlGuid) || xmlGuid != materialGuid)
                    return null;

                var metadata = doc.Descendants(ns + "metadata").FirstOrDefault();
                var properties = doc.Descendants(ns + "properties").FirstOrDefault();

                if (metadata == null || properties == null)
                    return null;

                var nameElement = metadata.Descendants(ns + "name").FirstOrDefault();
                var brand = nameElement?.Element(ns + "brand")?.Value ?? "Generic";
                var material = nameElement?.Element(ns + "material")?.Value ?? "Unknown";

                var densityElement = properties.Element(ns + "density");
                decimal? density = null;
                if (densityElement != null &&
                    decimal.TryParse(densityElement.Value,
                                   System.Globalization.NumberStyles.Any,
                                   System.Globalization.CultureInfo.InvariantCulture,
                                   out var d))
                {
                    density = d;
                }

                return new MaterialInfo
                {
                    Guid = xmlGuid,
                    Nome = material,
                    Fabricante = brand,
                    Densidade = density
                };
            }
            catch
            {
                return null;
            }
        }
    }

    public class MaterialInfo
    {
        public Guid Guid { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Fabricante { get; set; } = string.Empty;
        public decimal? Densidade { get; set; }
    }
}