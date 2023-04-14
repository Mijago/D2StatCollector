using DotNetBungieAPI.Models.Destiny;
using DotNetBungieAPI.Models.Destiny.Responses;
using DotNetBungieAPI.Service.Abstractions;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;

namespace D2StatCollector.Server.StatCollector.Collectors;

public class ProfileProgressionCollector : AStatCollector
{
    public override DestinyComponentType[] RequiredComponentTypes => new DestinyComponentType[]
    {
        DestinyComponentType.ProfileProgression
    };


    public ProfileProgressionCollector(IBungieClient bng, InfluxDBClient influxDbClient) : base(bng, influxDbClient)
    {
    }

    public override async Task Collect(DestinyProfileResponse profile, Dictionary<string, string> additionalTags)
    {
        var points = new List<PointData>();
        
        foreach (var (key, value) in profile.ProfileProgression.Data.Checklists)
        {
            var time = DateTime.UtcNow;
            var point = BuildDefaultPointData("user_profile_progression", additionalTags)
                .Tag("checklist_id", key.Hash.ToString())
                .Timestamp(time, WritePrecision.Ns);

            var sum = 0;
            var count = 0;
            foreach (var (key2, value2) in value)
            {
                count += 1;
                sum += value2 ? 1 : 0;
                point = point.Field(key2.ToString(), value2);
            }

            point = point.Field("sum", sum);
            point = point.Field("percentage", (float)sum / count);


            points.Add(point);
        }
        WritePoints(points);
    }
}