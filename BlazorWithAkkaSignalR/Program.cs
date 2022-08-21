using Akka.Hosting;
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
    configurationBuilder
    .WithActors((system, registry) =>
    {
        var props = Akka.DependencyInjection.DependencyResolver.For(system).Props<CounterActor>();
        var actor = system.ActorOf(props, "counterActor");

        registry.Register<CounterActor>(actor);
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
