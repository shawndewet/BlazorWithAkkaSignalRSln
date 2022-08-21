using Microsoft.AspNetCore.SignalR;

namespace BlazorWithAkkaSignalR.Hubs
{
    public class CounterHub : Hub
    {
        public async Task IncrementCounter()
        {
            await Clients.All.SendAsync("IncrementCounter","incremented via SignalR");
        }
    }
}
