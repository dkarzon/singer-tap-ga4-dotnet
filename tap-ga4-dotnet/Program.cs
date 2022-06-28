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

        if (config == null)
        {
            throw new NullReferenceException("Unable to deserialize config json.");
        }

        var analyticsService = new AnalyticsDataService(config);

        analyticsService.RunReports();
    });