using System.Collections.ObjectModel;
using DotNetBungieAPI.Models.Destiny;
using DotNetBungieAPI.Models.Destiny.Components;
using DotNetBungieAPI.Models.Destiny.Responses;
using DotNetBungieAPI.Models.User;

namespace D2StatCollector.Server.StatCollector;

public interface IStatCollector
{
    // get clan members by clan id
    Task Collect(DestinyProfileResponse profile, Dictionary<string, string> additionalTags);
    
    DestinyComponentType[] RequiredComponentTypes { get; }
}