using Akka.Actor;
using Akka.Event;
using BlazorWithAkkaSignalR.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace BlazorWithAkkaSignalR.Actors
{
    public class CounterActor : ReceiveActor
    {
        public sealed class IncrementCounter
        {
            public IncrementCounter(string message)
            {
                Message = message;
            }

            public string Message { get; }
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
                await _hubContext.Clients.All.SendAsync("IncrementCounter", msg.Message);
            });
        }

        protected override void PreStart()
        {
            var ms = 5000;
            timer = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(ms, ms, Self, new IncrementCounter("via Timer"), ActorRefs.NoSender);

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
