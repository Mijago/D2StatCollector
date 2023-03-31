using DotNetBungieAPI.Models.Destiny;
using DotNetBungieAPI.Models.Destiny.Responses;
using DotNetBungieAPI.Service.Abstractions;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;

namespace D2StatCollector.Server.StatCollector.Collectors;

public class ProfileArtifactCollector : AStatCollector
{
    public override DestinyComponentType[] RequiredComponentTypes => new DestinyComponentType[]
    {
        DestinyComponentType.ProfileProgression
    };


    public ProfileArtifactCollector(IBungieClient bng, InfluxDBClient influxDbClient) : base(bng, influxDbClient)
    {
    }

    public override async Task Collect(DestinyProfileResponse profile)
    {
        var PowerBonus = profile.ProfileProgression.Data.SeasonalArtifact.PowerBonus;
        var PointsAcquired = profile.ProfileProgression.Data.SeasonalArtifact.PointsAcquired;
        var xpDailyProgress = profile.ProfileProgression.Data.SeasonalArtifact.PowerBonusProgression.DailyProgress;
        var xpWeeklyProgress = profile.ProfileProgression.Data.SeasonalArtifact.PowerBonusProgression.WeeklyProgress;
        var xpCurrentProgress = profile.ProfileProgression.Data.SeasonalArtifact.PowerBonusProgression.CurrentProgress;
        var xpProgressToNextLevel = profile.ProfileProgression.Data.SeasonalArtifact.PowerBonusProgression.ProgressToNextLevel;
        
        
        var time = DateTime.UtcNow;
        var point = PointData
            .Measurement("user_profile_artifact")
            .Tag("user_membershipId", profile.Profile.Data.UserInfo.MembershipId.ToString())
            .Tag("user_membershipType", profile.Profile.Data.UserInfo.MembershipType.ToString())
            .Tag("user_displayName", profile.Profile.Data.UserInfo.BungieGlobalDisplayName+"#"+profile.Profile.Data.UserInfo.BungieGlobalDisplayNameCode)
            .Field("powerBonus", PowerBonus)
            .Field("pointsAcquired", PointsAcquired)
            .Field("xpDailyProgress", xpDailyProgress)
            .Field("xpWeeklyProgress", xpWeeklyProgress)
            .Field("xpCurrentProgress", xpCurrentProgress)
            .Field("xpProgressToNextLevel", xpProgressToNextLevel)
            .Timestamp(time, WritePrecision.Ns);

        WritePoint(point);
    }
}