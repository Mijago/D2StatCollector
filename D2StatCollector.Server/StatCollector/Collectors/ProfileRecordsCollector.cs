using DotNetBungieAPI.Models.Destiny;
using DotNetBungieAPI.Models.Destiny.Responses;
using DotNetBungieAPI.Service.Abstractions;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;

namespace D2StatCollector.Server.StatCollector.Collectors;

public class ProfileRecordsCollector : AStatCollector
{
    public override DestinyComponentType[] RequiredComponentTypes => new DestinyComponentType[]
    {
        DestinyComponentType.Records
    };


    public ProfileRecordsCollector(IBungieClient bng, InfluxDBClient influxDbClient) : base(bng, influxDbClient)
    {
    }

    public override async Task Collect(DestinyProfileResponse profile, Dictionary<string, string> additionalTags)
    {
        var score = profile.ProfileRecords.Data.Score;
        var activeScore = profile.ProfileRecords.Data.ActiveScore;
        var legacyScore = profile.ProfileRecords.Data.LegacyScore;
        var lifetimeScore = profile.ProfileRecords.Data.LifetimeScore;
        
        
        
        var time = DateTime.UtcNow;
        var point = BuildDefaultPointData("user_profile_records", additionalTags)
            .Field("score", score)
            .Field("activeScore", activeScore)
            .Field("legacyScore", legacyScore)
            .Field("lifetimeScore", lifetimeScore)
            .Timestamp(time, WritePrecision.Ns);
        
        WritePoint(point);
    }
}