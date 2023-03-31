using DotNetBungieAPI.Models.Destiny;
using DotNetBungieAPI.Models.Destiny.Responses;
using DotNetBungieAPI.Service.Abstractions;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;

namespace D2StatCollector.Server.StatCollector.Collectors;

public class CharacterCraftablesCollector : AStatCollector
{
    public override DestinyComponentType[] RequiredComponentTypes => new DestinyComponentType[]
    {
        DestinyComponentType.Craftables
    };
    
    
    public CharacterCraftablesCollector(IBungieClient bng, InfluxDBClient influxDbClient) : base(bng, influxDbClient)
    {
    }

    public override async Task Collect(DestinyProfileResponse profile)
    {
        // define a dict
        var dict = new Dictionary<uint, int>();

        foreach (var (key, charCraft) in profile.CharacterCraftables.Data)
        {
            foreach (var (definitionHashPointer, craftableComponent) in charCraft.Craftables)
            {
                // not craftable, if 1 is in FailedRequirementIndexes
                var value = craftableComponent.FailedRequirementIndexes.Count > 0 ? 0 : 1;

                if (!dict.ContainsKey(definitionHashPointer.Hash.Value))
                    dict.Add(definitionHashPointer.Hash.Value, value);
                else
                    dict[definitionHashPointer.Hash.Value] =
                        (dict[definitionHashPointer.Hash.Value] == 1 || 1 == value) ? 1 : 0;
            }
        }

        var points = new List<PointData>();
        var time = DateTime.UtcNow;
        foreach (var (key, value) in dict)
        {
            var point = PointData
                .Measurement("user_character_craftables2")
                .Tag("user_membershipId", profile.Profile.Data.UserInfo.MembershipId.ToString())
                .Tag("user_membershipType", profile.Profile.Data.UserInfo.MembershipType.ToString())
                .Tag("user_displayName", profile.Profile.Data.UserInfo.BungieGlobalDisplayName+"#"+profile.Profile.Data.UserInfo.BungieGlobalDisplayNameCode)
                
                .Timestamp(time, WritePrecision.Ns);
            point = point.Tag("craftable_hash", key.ToString());
            point = point.Field("value", value);
            
            points.Add(point);
        }
        WritePoints(points);
    }
}