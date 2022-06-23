using CommandLine;
using Newtonsoft.Json;
using SingerTapGA4;
using SingerTapGA4.Models;

//args = new[] { "-c","ga4_config.json" };

Parser.Default
    .ParseArguments<CommandLineOptions>(args)
    .WithParsed(o => {
        var configJson = File.ReadAllText(o.Config);

        var config = JsonConvert.DeserializeObject<Config>(configJson);

        var analyticsService = new AnalyticsDataService(config);

        analyticsService.RunReports();

        Console.ReadLine();
    });