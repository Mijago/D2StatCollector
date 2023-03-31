using System.Collections.ObjectModel;
using D2StatCollector.Server.StatCollector;
using DotNetBungieAPI.Models;
using DotNetBungieAPI.Models.GroupsV2;
using DotNetBungieAPI.Service.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace D2StatCollector.Server.Services;

public class StatCollectorService
{
    private readonly IBungieClient _bungieClient;

    private readonly IEnumerable<IStatCollector> _collectors;
    // reference the service collection

    public StatCollectorService(IServiceProvider services, IBungieClient bungieClient)
    {
        _bungieClient = bungieClient;
        // get all registered IStatCollector
        _collectors = services.GetServices<IStatCollector>();
    }

    
    public async Task CollectAll()
    {
        // TODO: add a method to collect for individual users 
        
        // read environment variable "D2SC_CLANS". A comma separated list of clan ids.
        var clanVar = Environment.GetEnvironmentVariable("D2SC_CLANS");
        // split clanVar by "," trim spaces and convert to long
        var clans = clanVar?.Split(",").Select(e => long.Parse(e.Trim())).ToArray();

        if (clans == null || clans.Length == 0)
        {
            throw new Exception(
                "No clans to collect. Make sure to set the environment variable D2SC_CLANS as a comma separated list of clan ids.");
        }

        foreach (var clanId in clans)
        {
            Console.WriteLine("Collecting clan " + clanId);
            await CollectClan(clanId);
        }
    }

    public async Task CollectClan(long clanId)
    {
        var clanMembers = await GetClanMembers(clanId);
        // print
        foreach (var clanMember in clanMembers)
        {
            Task.Run(() =>
                CollectMember(clanMember.DestinyUserInfo.MembershipType, clanMember.DestinyUserInfo.MembershipId)
                    .ConfigureAwait(false));
        }

        Console.WriteLine("Queued all");
    }

    public async Task CollectMember(BungieMembershipType membershipType, long membershipId)
    {
        var componentTypes = _collectors.SelectMany(e => e.RequiredComponentTypes).Distinct().ToArray();

        Console.WriteLine("COLLECT MEMBER" + membershipType + " " + membershipId);
        var profile = _bungieClient.ApiAccess.Destiny2.GetProfile(
            membershipType,
            membershipId,
            componentTypes
        );

        foreach (var statCollector in _collectors)
        {
            Console.WriteLine("Collect " + statCollector.GetType().Name);


            await statCollector.Collect(profile.Result.Response);
            Console.WriteLine("Done collecting " + statCollector.GetType().Name + "");
        }
    }


    // get clan members by clan id
    public async Task<ReadOnlyCollection<GroupMember>> GetClanMembers(long groupId)
    {
        var members = await _bungieClient.ApiAccess.GroupV2.GetMembersOfGroup(groupId);

        // throw if error
        if (members.Response == null || members.ErrorCode != PlatformErrorCodes.Success)
            throw new Exception(members.ErrorStatus);

        return members.Response.Results;
    }
}