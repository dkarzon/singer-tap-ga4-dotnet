using Google.Analytics.Data.V1Beta;
using Newtonsoft.Json;
using SingerTapGA4.Models;

namespace SingerTapGA4
{
    public class AnalyticsDataService
    {
        private readonly Config _config;
        private readonly BetaAnalyticsDataClient _client;

        public AnalyticsDataService(Config config)
        {
            _config = config;

            _client = new BetaAnalyticsDataClientBuilder
            {
                CredentialsPath = config.CredentialsJsonPath
            }.Build();
        }

        public async Task RunReportsAsync()
        {
            if (string.IsNullOrEmpty(_config.Reports))
            {
                Console.Error.WriteLine("Missing reports config property.");
                return;
            }

            List<ReportConfig>? reports = null;

            try
            {
                var reportsJson = File.ReadAllText(_config.Reports);

                reports = JsonConvert.DeserializeObject<List<ReportConfig>>(reportsJson);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Error reading reports json. " + ex.Message);
                return;
            }

            if (reports == null || reports.Count == 0)
            {
                Console.Error.WriteLine("Reports json is empty.");
                return;
            }

            foreach (var report in reports)
            {
                try
                {
                    RunReportRequest request = new RunReportRequest
                    {
                        Property = "properties/" + _config.PropertyId,
                        DateRanges = { new DateRange { StartDate = "2020-03-31", EndDate = "today" }, },
                    };

                    foreach(var metric in report.Metrics)
                    {
                        request.Metrics.Add(new Metric { Name = metric });
                    }
                    foreach (var dimension in report.Dimensions)
                    {
                        request.Dimensions.Add(new Dimension { Name = dimension });
                    }

                    RunReportResponse response = _client.RunReport(request);

                    //TODO: output SCHEMA message
                    PostMessage(BuildSchema(report));

                    foreach (Row row in response.Rows)
                    {
                        //TODO: output RECORD message
                        PostMessage(BuildRecord(row, report));
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Error running report " + report.Name + ". " + ex.Message);
                }
            }
        }

        private string ToStreamName(string name)
        {
            return name;
        }

        private SingerMessage BuildSchema(ReportConfig report)
        {
            return new SingerMessage
            {
                Stream = ToStreamName(report.Name),
                Type = SingerMessageType.SCHEMA,
                Schema = new SingerSchema
                {
                    
                }
            };
        }

        private SingerMessage BuildRecord(Row row, ReportConfig report)
        {
            return new SingerMessage
            {
                Stream = ToStreamName(report.Name),
                Type = SingerMessageType.RECORD
            };
        }

        private void PostMessage(object message)
        {
            var output = JsonConvert.SerializeObject(message);

            //Log.Information("IN {inMessage} OUT {outMessage}", s, output);
            Console.WriteLine(output);
        }
    }
}
