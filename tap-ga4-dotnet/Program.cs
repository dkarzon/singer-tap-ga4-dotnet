using CommandLine;
using Newtonsoft.Json;
using SingerTapGA4;
using SingerTapGA4.Models;


Console.WriteLine("Hello, World!");

args = new[] { "-c","ga4_config.json" };

Parser.Default
    .ParseArguments<CommandLineOptions>(args)
    .WithParsed(async o => {
        var configJson = File.ReadAllText(o.Config);

        var config = JsonConvert.DeserializeObject<Config>(configJson);

        var analyticsService = new AnalyticsDataService(config);

        await analyticsService.RunReportsAsync();

        Console.ReadLine();
    });