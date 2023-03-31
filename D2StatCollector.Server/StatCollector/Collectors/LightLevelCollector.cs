using DotNetBungieAPI.Models.Destiny;
using DotNetBungieAPI.Models.Destiny.Responses;
using DotNetBungieAPI.Service.Abstractions;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;

namespace D2StatCollector.Server.StatCollector.Collectors;

public class LightLevelCollector : AStatCollector
{
    public override DestinyComponentType[] RequiredComponentTypes => new DestinyComponentType[]
    {
        DestinyComponentType.Characters,
        DestinyComponentType.Profiles,
    };


    public LightLevelCollector(IBungieClient bng, InfluxDBClient influxDbClient) : base(bng, influxDbClient)
    {
    }

    public override async Task Collect(DestinyProfileResponse profile)
    {
        var Light = profile.Characters.Data.Select(c => c.Value.Light).Max();
        var MinutesPlayedTotal = profile.Characters.Data.Select(c => c.Value.MinutesPlayedTotal).Max();
        var MinutesPlayedThisSession = profile.Characters.Data.Select(c => c.Value.MinutesPlayedThisSession).Max();
        var LifetimeHighestGuardianRank = profile.Profile.Data.LifetimeHighestGuardianRank;
        var CurrentGuardianRank = profile.Profile.Data.CurrentGuardianRank;
        
        var time = DateTime.UtcNow;
        var point = PointData
            .Measurement("user_profile_generic")
            .Tag("user_membershipId", profile.Profile.Data.UserInfo.MembershipId.ToString())
            .Tag("user_membershipType", profile.Profile.Data.UserInfo.MembershipType.ToString())
            .Tag("user_displayName", profile.Profile.Data.UserInfo.BungieGlobalDisplayName+"#"+profile.Profile.Data.UserInfo.BungieGlobalDisplayNameCode)
            .Field("light", Light)
            .Field("minutesPlayedTotal", MinutesPlayedTotal)
            .Field("minutesPlayedThisSession", MinutesPlayedThisSession)
            .Field("lifetimeHighestGuardianRank", LifetimeHighestGuardianRank)
            .Field("currentGuardianRank", CurrentGuardianRank)
            .Timestamp(time, WritePrecision.Ns);

        WritePoint(point);
    }
}