using DotNetBungieAPI.Models.Destiny;
using DotNetBungieAPI.Models.Destiny.Responses;
using DotNetBungieAPI.Service.Abstractions;
using InfluxDB.Client;
using InfluxDB.Client.Writes;

namespace D2StatCollector.Server.StatCollector;

public abstract class AStatCollector : IStatCollector
{
    protected readonly IBungieClient _bng;
    protected readonly InfluxDBClient _influxDbClient;
    private IStatCollector _statCollectorImplementation;

    protected AStatCollector(IBungieClient bng, InfluxDBClient influxDbClient)
    {
        _bng = bng;
        _influxDbClient = influxDbClient;
    }

    protected async void WritePoint(PointData data)
    {
        using var writeApi = _influxDbClient.GetWriteApi();
        var influxDbBucket = Environment.GetEnvironmentVariable("INFLUXDB_DB");
        var influxDbOrg = Environment.GetEnvironmentVariable("D2SC_INFLUXDB_ORG");
        writeApi.WritePoint(data, influxDbBucket, influxDbOrg);
    }

    protected async void WritePoints(List<PointData> data)
    {
        using var writeApi = _influxDbClient.GetWriteApi();
        var influxDbBucket = Environment.GetEnvironmentVariable("INFLUXDB_DB");
        var influxDbOrg = Environment.GetEnvironmentVariable("D2SC_INFLUXDB_ORG");
        writeApi.WritePoints(data, influxDbBucket, influxDbOrg);
    }

    // get clan members by clan id
    public abstract Task Collect(DestinyProfileResponse profile);
    public abstract DestinyComponentType[] RequiredComponentTypes { get; }
}