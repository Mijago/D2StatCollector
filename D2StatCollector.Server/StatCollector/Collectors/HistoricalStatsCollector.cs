using DotNetBungieAPI.Models;
using DotNetBungieAPI.Models.Destiny;
using DotNetBungieAPI.Models.Destiny.HistoricalStats;
using DotNetBungieAPI.Models.Destiny.Responses;
using DotNetBungieAPI.Service.Abstractions;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;

namespace D2StatCollector.Server.StatCollector.Collectors;

public class HistoricalStatsCollector : AStatCollector
{
    public override DestinyComponentType[] RequiredComponentTypes => new DestinyComponentType[]
    {
        DestinyComponentType.Characters,
        DestinyComponentType.Profiles
    };

    public HistoricalStatsCollector(IBungieClient bng, InfluxDBClient influxDbClient) : base(bng, influxDbClient)
    {
    }

    public override async Task Collect(DestinyProfileResponse profile, Dictionary<string, string> additionalTags)
    {
        var response =
            await _bng.ApiAccess.Destiny2.GetHistoricalStatsForAccount(
                profile.Profile.Data.UserInfo.MembershipType,
                profile.Profile.Data.UserInfo.MembershipId
            );


        var time = DateTime.UtcNow;

        HandleCharacters(profile, response, time, additionalTags);
        HandleMerged(profile, response, time, additionalTags);
    }

    private void HandleCharacters(DestinyProfileResponse profile,
        BungieResponse<DestinyHistoricalStatsAccountResult> response, DateTime time,
        Dictionary<string, string> additionalTags)
    {
        List<PointData> points = new List<PointData>();

        foreach (var character in response.Response.Characters)
        {
            foreach (var (category, categoryStats) in character.Results)
            {
                var point = BuildDefaultPointData("user_historical_stats", additionalTags)
                    .Tag("character_id", character.CharacterId.ToString())
                    .Tag("category", category)
                    .Timestamp(time, WritePrecision.Ns);

                if (profile.Characters.Data.TryGetValue(character.CharacterId, out var cls))
                {
                    var characterClass = cls.ClassType;
                    point = point.Tag("character_class", characterClass.ToString());
                }
                else
                {
                    point = point.Tag("character_class", "deleted");
                }

                foreach (var (key, value) in categoryStats.AllTime)
                {
                    point = point.Field(key, value.BasicValue.Value);
                }

                points.Add(point);
            }
        }

        WritePoints(points);
    }

    private void HandleMerged(DestinyProfileResponse profile,
        BungieResponse<DestinyHistoricalStatsAccountResult> response, DateTime time,
        Dictionary<string, string> additionalTags)
    {
        List<PointData> points = new List<PointData>();

        foreach (var (category, categoryStats) in response.Response.MergedAllCharacters.Results)
        {
            var point = BuildDefaultPointData("user_historical_stats_merged", additionalTags)
                .Tag("category", category)
                .Timestamp(time, WritePrecision.Ns);
            
            foreach (var (key, value) in categoryStats.AllTime)
            {
                point = point.Field(key, value.BasicValue.Value);
            }

            points.Add(point);
        }

        WritePoints(points);
    }
}