
using Akka.Actor;
using Akka.Cluster.Hosting;
using Akka.Hosting;
using Akka.Remote.Hosting;
using BlazorWithAkkaSignalR.Host.Actors;
using Microsoft.Extensions.Hosting;

var builder = new HostBuilder()
    .ConfigureServices((context, services) =>
    {
        var hostName = "localhost";
        var port = 7919;
        var seeds = new[] { "akka.tcp://BlazorActorSystem@localhost:7919" }.Select(Address.Parse).ToArray();

        services.AddAkka("BlazorActorSystem", (configurationBuilder, provider) =>
        {
            configurationBuilder
                .WithRemoting(hostName, port)
                .WithClustering(new ClusterOptions()
                    { Roles = new[] { "host" }, SeedNodes = seeds })
                //.WithShardRegion<QuestMarker>("quests", s => QuestActor.GetProps(s),
                //    new QuestMessageRouter(),
                //    new ShardOptions()
                //    {
                //        RememberEntities = true,
                //        Role = QuestActorProps.SingletonActorRole,
                //        StateStoreMode = StateStoreMode.DData
                //    })
                .StartActors((system, registry) =>
                {
                    registry.TryRegister<IncrementActor>(system.ActorOf(Props.Create(() => new IncrementActor()), "incrementActor"));
                });

        });
    })
    .Build();

await builder.RunAsync();