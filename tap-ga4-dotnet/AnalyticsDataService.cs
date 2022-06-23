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

        public void RunReports()
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
                        DateRanges = {
                            new DateRange
                            {
                                StartDate = _config.StartDate,
                                EndDate = _config.EndDate ?? "today"
                            }
                        },
                    };

                    foreach(var metric in report.Metrics)
                    {
                        request.Metrics.Add(new Metric { Name = metric });
                    }
                    foreach (var dimension in report.Dimensions)
                    {
                        request.Dimensions.Add(new Dimension { Name = dimension });
                    }

                    // Async method didn't seem to work here
                    RunReportResponse response = _client.RunReport(request);

                    // output SCHEMA message
                    var schemaMessage = BuildSchema(report);
                    PostMessage(schemaMessage);

                    foreach (Row row in response.Rows)
                    {
                        //output RECORD message
                        PostMessage(BuildRecord(row, report, schemaMessage.Schema));
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Error running report " + report.Name + ". " + ex.Message);
                }
            }
        }

        private string ToCleanName(string name)
        {
            return name.Replace(":", "_");
        }

        private List<string> LookupMetricDataType(string metric)
        {
            var dataType = new List<string>
            {
                "null"
            };
            // This is a hack for now until I come up with a better way to do it.
            var lowerMetric = metric.ToLower();
            if (lowerMetric.Contains("rate") || lowerMetric.Contains("avg") || lowerMetric.Contains("duration"))
            {
                dataType.Add("number");
            }
            else
            {
                dataType.Add("integer");
            }
            return dataType;
        }

        private SingerMessage BuildSchema(ReportConfig report)
        {
            var schemaProps = new Dictionary<string, SingerSchemaProperty>();
            foreach (var d in report.Dimensions)
            {
                schemaProps.Add(ToCleanName(d), new SingerSchemaProperty
                {
                    Type = new List<string> { "string" }
                });
            }
            foreach (var m in report.Metrics)
            {
                schemaProps.Add(ToCleanName(m), new SingerSchemaProperty
                {
                    Type = LookupMetricDataType(m)
                });
            }

            return new SingerMessage
            {
                Stream = ToCleanName(report.Name),
                Type = SingerMessageType.SCHEMA,
                Schema = new SingerSchema
                {
                    Type = new List<string> // Hardcoding since we always return a row object
                    {
                        "null",
                        "object"
                    },
                    Properties = schemaProps
                },
                KeyProperties = report.Dimensions.Select(d => ToCleanName(d)).ToList()
            };
        }

        private SingerMessage BuildRecord(Row row, ReportConfig report, SingerSchema schema)
        {
            var record = new Dictionary<string, object>();
            for (var i = 0; i < report.Dimensions.Count; i++)
            {
                // Always a string
                record.Add(ToCleanName(report.Dimensions[i]), row.DimensionValues[i].Value);
            }
            for (var i = 0; i < report.Metrics.Count; i++)
            {
                // Check what type we are set in the schema and convert to that type
                var cleanName = ToCleanName(report.Metrics[i]);
                var value = ConvertMetricValue(cleanName, row.MetricValues[i].Value, schema);
                record.Add(cleanName, value);
            }

            return new SingerMessage
            {
                Stream = ToCleanName(report.Name),
                Type = SingerMessageType.RECORD,
                Record = record
            };
        }

        private object ConvertMetricValue(string cleanName, string value, SingerSchema schema)
        {
            var thisSchema = schema.Properties[cleanName].Type.First(t => t != "null");
            if (thisSchema == "integer")
            {
                return Convert.ToInt32(value);
            }
            else if (thisSchema == "number")
            {
                return Convert.ToDouble(value);
            }
            return value;
        }

        private void PostMessage(object message)
        {
            var output = JsonConvert.SerializeObject(message);

            //Log.Information("IN {inMessage} OUT {outMessage}", s, output);
            Console.WriteLine(output);
        }
    }
}
