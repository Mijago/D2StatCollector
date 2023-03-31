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

    public override async Task Collect(DestinyProfileResponse profile)
    {
        var response =
            await _bng.ApiAccess.Destiny2.GetHistoricalStatsForAccount(
                profile.Profile.Data.UserInfo.MembershipType,
                profile.Profile.Data.UserInfo.MembershipId
            );


        var time = DateTime.UtcNow;

        HandleCharacters(profile, response, time);
        HandleMerged(profile, response, time);
    }

    private void HandleCharacters(DestinyProfileResponse profile,
        BungieResponse<DestinyHistoricalStatsAccountResult> response, DateTime time)
    {
        List<PointData> points = new List<PointData>();

        foreach (var character in response.Response.Characters)
        {
            foreach (var (category, categoryStats) in character.Results)
            {
                var point = PointData
                    .Measurement("user_historical_stats")
                    .Tag("user_membershipId", profile.Profile.Data.UserInfo.MembershipId.ToString())
                    .Tag("user_membershipType", profile.Profile.Data.UserInfo.MembershipType.ToString())
                    .Tag("user_displayName",
                        profile.Profile.Data.UserInfo.BungieGlobalDisplayName + "#" +
                        profile.Profile.Data.UserInfo.BungieGlobalDisplayNameCode)
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
        BungieResponse<DestinyHistoricalStatsAccountResult> response, DateTime time)
    {
        List<PointData> points = new List<PointData>();

        foreach (var (category, categoryStats) in response.Response.MergedAllCharacters.Results)
        {
            var point = PointData
                .Measurement("user_historical_stats_merged")
                .Tag("user_membershipId", profile.Profile.Data.UserInfo.MembershipId.ToString())
                .Tag("user_membershipType", profile.Profile.Data.UserInfo.MembershipType.ToString())
                .Tag("user_displayName",
                    profile.Profile.Data.UserInfo.BungieGlobalDisplayName + "#" +
                    profile.Profile.Data.UserInfo.BungieGlobalDisplayNameCode)
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