using DotNetBungieAPI.Models.Destiny;
using DotNetBungieAPI.Models.Destiny.Responses;
using DotNetBungieAPI.Service.Abstractions;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;

namespace D2StatCollector.Server.StatCollector.Collectors;

public class ProfileCommendationsCollector : AStatCollector
{
    public override DestinyComponentType[] RequiredComponentTypes => new DestinyComponentType[]
    {
        DestinyComponentType.SocialCommendations
    };
    
    public ProfileCommendationsCollector(IBungieClient bng, InfluxDBClient influxDbClient) : base(bng, influxDbClient)
    {
    }

    public override async Task Collect(DestinyProfileResponse profile)
    {
        var time = DateTime.UtcNow;
        commendationScoresByHash(profile, time);
        commendationNodeScoresByHash(profile, time);
        
        var point = PointData
            .Measurement("user_profile_commendations")
            .Tag("user_membershipId", profile.Profile.Data.UserInfo.MembershipId.ToString())
            .Tag("user_membershipType", profile.Profile.Data.UserInfo.MembershipType.ToString())
            .Tag("user_displayName",
                profile.Profile.Data.UserInfo.BungieGlobalDisplayName + "#" +
                profile.Profile.Data.UserInfo.BungieGlobalDisplayNameCode)
            .Tag("category", "general")
            .Field("totalScore", profile.ProfileCommendations.Data.TotalScore)
            .Timestamp(time, WritePrecision.Ns);

        WritePoint(point);
        
        
    }

    private void commendationScoresByHash(DestinyProfileResponse profile, DateTime time)
    {
        var point = PointData
            .Measurement("user_profile_commendations")
            .Tag("user_membershipId", profile.Profile.Data.UserInfo.MembershipId.ToString())
            .Tag("user_membershipType", profile.Profile.Data.UserInfo.MembershipType.ToString())
            .Tag("user_displayName",
                profile.Profile.Data.UserInfo.BungieGlobalDisplayName + "#" +
                profile.Profile.Data.UserInfo.BungieGlobalDisplayNameCode)
            .Tag("category", "ScoresByHash")
            .Timestamp(time, WritePrecision.Ns);

        foreach (var (key, value) in profile.ProfileCommendations.Data.CommendationScoresByHash)
        {
            point = point.Field(key.ToString(), value);
        }

        WritePoint(point);
    }

    private void commendationNodeScoresByHash(DestinyProfileResponse profile, DateTime time)
    {
        var point = PointData
            .Measurement("user_profile_commendations")
            .Tag("user_membershipId", profile.Profile.Data.UserInfo.MembershipId.ToString())
            .Tag("user_membershipType", profile.Profile.Data.UserInfo.MembershipType.ToString())
            .Tag("user_displayName", profile.Profile.Data.UserInfo.BungieGlobalDisplayName+"#"+profile.Profile.Data.UserInfo.BungieGlobalDisplayNameCode)
            .Tag("category", "NodeScoresByHash")
            .Timestamp(time, WritePrecision.Ns);

        foreach (var (key, value) in profile.ProfileCommendations.Data.CommendationScoresByHash)
        {
            point = point.Field(key.Hash.ToString(), value);
        }

        WritePoint(point);
    }
}