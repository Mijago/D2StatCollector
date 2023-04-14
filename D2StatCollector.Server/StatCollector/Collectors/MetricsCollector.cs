using DotNetBungieAPI.Models.Destiny;
using DotNetBungieAPI.Models.Destiny.Responses;
using DotNetBungieAPI.Service.Abstractions;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;

namespace D2StatCollector.Server.StatCollector.Collectors;

public class MetricsCollector : AStatCollector
{
    public override DestinyComponentType[] RequiredComponentTypes => new DestinyComponentType[]
    {
        DestinyComponentType.Metrics
    };

    public MetricsCollector(IBungieClient bng, InfluxDBClient influxDbClient) : base(bng, influxDbClient)
    {
    }

    public override async Task Collect(DestinyProfileResponse profile, Dictionary<string, string> additionalTags)
    {
        var time = DateTime.UtcNow;
        var point = BuildDefaultPointData("user_character_metrics", additionalTags)
            .Timestamp(time, WritePrecision.Ns);
        foreach (var (key, metric) in profile.Metrics.Data.Metrics)
        {
            point = point.Field(
                metric.ObjectiveProgress.Objective.Hash.ToString(),
                metric.ObjectiveProgress.Progress
            );
        }

        WritePoint(point);
    }
}