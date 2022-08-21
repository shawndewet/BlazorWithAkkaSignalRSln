using Akka.Actor;
using Akka.Event;
using BlazorWithAkkaSignalR.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace BlazorWithAkkaSignalR.Actors
{
    public class CounterActor : ReceiveActor
    {
        private sealed class IncrementCounter
        {
            public static readonly IncrementCounter Instance = new IncrementCounter();
            private IncrementCounter() { }
        }

        private ILoggingAdapter _logger = Context.GetLogger();
        private ICancelable? timer;
        private readonly IHubContext<CounterHub> _hubContext;

        public CounterActor(IHubContext<CounterHub> hubContext)
        {
            _hubContext = hubContext;
            Become(Started);
            _logger.Info("CounterActor constructed");
        }

        private void Started()
        {
            Receive<IncrementCounter>(async msg =>
            {
                _logger.Info("IncrementCounter");
                await _hubContext.Clients.All.SendAsync("IncrementCounter", "incremented by Akka Actor");
            });
        }

        protected override void PreStart()
        {
            //set up a timer to send IncrementCounter message every so many seconds
            var seconds = 5000;
            timer = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(seconds, seconds, Self, IncrementCounter.Instance, ActorRefs.NoSender);

            base.PreStart();
        }

        protected override void PostStop()
        {
            if (timer != null)
                timer.Cancel();
            base.PostStop();
        }
    }
}
