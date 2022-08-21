using Akka.Actor;
using Akka.Cluster.Hosting;
using Akka.Hosting;
using Akka.Remote.Hosting;
using BlazorWithAkkaSignalR.Actors;
using BlazorWithAkkaSignalR.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddAkka("BlazorActorSystem", configurationBuilder =>
{

    var hostName = "localhost";
    var port = 7918;
    var seeds = new[] { "akka.tcp://BlazorActorSystem@localhost:7919" }.Select(Address.Parse).ToArray();

    configurationBuilder
    .WithRemoting(hostName, port)
    .WithClustering(new ClusterOptions()
                { Roles = new[] { "blazor" }, SeedNodes = seeds })
    .WithActors((system, registry) =>
    {
        var props = Akka.DependencyInjection.DependencyResolver.For(system).Props<CounterActor>();
        var actor = system.ActorOf(props, "counterActor");

        registry.Register<CounterActor>(actor); //this is needed so that the actor can be injected into a view and a command executed on it (Tell) from the view.

        var subscriberProps = Akka.DependencyInjection.DependencyResolver.For(system).Props<SubscribingActor>();
        system.ActorOf(subscriberProps, "subscribingActor");

    });
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapHub<BlazorWithAkkaSignalR.Hubs.CounterHub>("/counterhub");
app.MapFallbackToPage("/_Host");

app.Run();
