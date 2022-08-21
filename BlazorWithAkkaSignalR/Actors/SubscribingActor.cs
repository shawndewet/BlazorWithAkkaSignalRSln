using Akka.Actor;
using Akka.Event;
using BlazorWithAkkaSignalR.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace BlazorWithAkkaSignalR.Actors
{
    public class SubscribingActor: ReceiveActor
    {
        private readonly IHubContext<CounterHub> _hubContext;
        private ILoggingAdapter _logger = Context.GetLogger();
        
        public SubscribingActor(IHubContext<CounterHub> hubContext)
        {
            _hubContext = hubContext;

            var _mediator = Akka.Cluster.Tools.PublishSubscribe.DistributedPubSub.Get(Context.System).Mediator;
            _mediator.Tell(new Akka.Cluster.Tools.PublishSubscribe.Subscribe("counterIncremented", Self));
            Receive<Akka.Cluster.Tools.PublishSubscribe.SubscribeAck>(ack =>
            {
                _logger.Info($"Received SubscribeAck with Topic {ack.Subscribe.Topic} and ref eq self? {ack.Subscribe.Ref.Equals(Self)}");

                if (ack != null && ack.Subscribe.Topic == "counterIncremented" && ack.Subscribe.Ref.Equals(Self))
                {
                    _logger.Info($"Become Ready");

                    Become(Ready);
                }
                else
                {
                    _logger.Info($"wtf?!?");
                }
            });
        }

        public void Ready()
        {
            Receive<Shared.CounterIncremented>(async msg =>
            {
                _logger.Info("IncrementCounter");
                await _hubContext.Clients.All.SendAsync("IncrementCounter", msg.Message);
            });
        }
    }
}
