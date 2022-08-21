using Akka.Actor;
using Akka.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorWithAkkaSignalR.Host.Actors
{
    public class IncrementActor : ReceiveActor
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
        private readonly IActorRef _mediator;

        public IncrementActor()
        {
            _mediator = Akka.Cluster.Tools.PublishSubscribe.DistributedPubSub.Get(Context.System).Mediator;
            Become(Started);
            _logger.Info("IncrementActor constructed");
        }

        private void Started()
        {
            Receive<IncrementCounter>(msg =>
            {
                _logger.Info("IncrementCounter");
                _mediator.Tell(new Akka.Cluster.Tools.PublishSubscribe.Publish("counterIncremented", new Shared.CounterIncremented("via pubsub")));
            });
        }

        protected override void PreStart()
        {
            var ms = 5000;
            timer = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(10000, ms, Self, new IncrementCounter("via Timer on Host"), ActorRefs.NoSender);

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
