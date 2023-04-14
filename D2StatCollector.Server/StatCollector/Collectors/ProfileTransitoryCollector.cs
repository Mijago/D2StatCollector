using DotNetBungieAPI.Models.Destiny;
using DotNetBungieAPI.Models.Destiny.Responses;
using DotNetBungieAPI.Service.Abstractions;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;

namespace D2StatCollector.Server.StatCollector.Collectors;

public class ProfileTransitoryCollector : AStatCollector
{
    public override DestinyComponentType[] RequiredComponentTypes => new DestinyComponentType[]
    {
        DestinyComponentType.Transitory
    };


    public ProfileTransitoryCollector(IBungieClient bng, InfluxDBClient influxDbClient) : base(bng, influxDbClient)
    {
    }

    public override async Task Collect(DestinyProfileResponse profile, Dictionary<string, string> additionalTags)
    {
        if (profile.ProfileTransitoryData.Data == null)
        {
            return;
        }
        
        var partySize = profile.ProfileTransitoryData.Data.PartyMembers.Count;
        var currentActivityScore = profile.ProfileTransitoryData.Data.CurrentActivity.Score;
        var currentActivityHighestOpposingFactionScore = profile.ProfileTransitoryData.Data.CurrentActivity.HighestOpposingFactionScore;
        var currentActivityNumberOfOpponents = profile.ProfileTransitoryData.Data.CurrentActivity.NumberOfOpponents;
        var currentActivityNumberOfPlayers = profile.ProfileTransitoryData.Data.CurrentActivity.NumberOfPlayers;
        var joinAbilityOpenSlots = profile.ProfileTransitoryData.Data.JoinAbility.OpenSlots;



        var time = DateTime.UtcNow;
        var point = BuildDefaultPointData("user_profile_transitory", additionalTags)
            .Field("partySize", partySize)
            .Field("joinAbilityOpenSlots", joinAbilityOpenSlots)
            .Field("currentActivityScore", currentActivityScore)
            .Field("currentActivityHighestOpposingFactionScore", currentActivityHighestOpposingFactionScore)
            .Field("currentActivityNumberOfOpponents", currentActivityNumberOfOpponents)
            .Field("currentActivityNumberOfPlayers", currentActivityNumberOfPlayers)
            .Timestamp(time, WritePrecision.Ns);
        
        WritePoint(point);
    }
}