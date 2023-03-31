// See https://aka.ms/new-console-template for more information

using D2StatCollector.Server.Services;
using D2StatCollector.Server.StatCollector;
using D2StatCollector.Server.StatCollector.Collectors;
using DotNetBungieAPI;
using DotNetBungieAPI.Clients;
using DotNetBungieAPI.Models;
using DotNetBungieAPI.Models.Destiny.Components;
using DotNetBungieAPI.Models.GroupsV2;
using DotNetBungieAPI.Models.Queries;
using DotNetBungieAPI.Service.Abstractions;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

Console.WriteLine("Hello, World!");

// sercice collection
var services = new ServiceCollection();

services.AddSingleton<IBungieClient>(BungieApiBuilder.GetApiClient(
    bcb =>
    {
        // TODO: grab these into variables and validate them, throw exception if not set
        bcb.ClientConfiguration.ApiKey = Environment.GetEnvironmentVariable("D2SC_APIKEY");
        bcb.ClientConfiguration.ClientId = int.Parse(Environment.GetEnvironmentVariable("D2SC_CLIENT_ID")); 
        bcb.ClientConfiguration.ClientSecret = Environment.GetEnvironmentVariable("D2SC_CLIENT_SECRET");
        bcb.ClientConfiguration.CacheDefinitions = false;
    }
));
services.AddSingleton<IStatCollector, HistoricalStatsCollector>();
services.AddSingleton<IStatCollector, CharacterCraftablesCollector>();
services.AddSingleton<IStatCollector, LightLevelCollector>();
services.AddSingleton<IStatCollector, ProfileCommendationsCollector>();
services.AddSingleton<IStatCollector, ProfileArtifactCollector>();
services.AddSingleton<IStatCollector, ProfileProgressionCollector>();
services.AddSingleton<IStatCollector, ProfileRecordsCollector>();
services.AddSingleton<IStatCollector, ProfileTransitoryCollector>();
services.AddSingleton<IStatCollector, MetricsCollector>();

services.AddTransient<StatCollectorService>();

// get env variables for influxdb
var influxDbHost = Environment.GetEnvironmentVariable("INFLUXDB_HOST");
var influxDbPort = Environment.GetEnvironmentVariable("INFLUXDB_PORT");
var influxDbUser = Environment.GetEnvironmentVariable("INFLUXDB_USER");
var influxDbUserPassword = Environment.GetEnvironmentVariable("INFLUXDB_USER_PASSWORD");

services.AddSingleton<InfluxDBClient>(
    new InfluxDBClient($"http://{influxDbHost}:{influxDbPort}", influxDbUser, influxDbUserPassword));


var provider = services.BuildServiceProvider();
StatCollectorService statCollectorService = provider.GetRequiredService<StatCollectorService>();
await statCollectorService.CollectAll();

var aTimer = new System.Timers.Timer();
var interval = int.Parse(Environment.GetEnvironmentVariable("D2SC_INTERVAL_MINUTES"));
aTimer.Interval = 1000 * 60 * interval;
aTimer.Elapsed += (sender, eventArgs) => { statCollectorService.CollectAll().ConfigureAwait(false); };
aTimer.AutoReset = true;
aTimer.Enabled = true;


await Task.Delay(-1);