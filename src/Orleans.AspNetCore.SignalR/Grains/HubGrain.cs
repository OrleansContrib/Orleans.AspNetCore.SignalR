using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.Extensions.Logging;
using Orleans.AspNetCore.SignalR.Observers;
using Orleans.Concurrency;
using Orleans.Placement;
using Orleans.Runtime;
using System.Threading.Tasks;

namespace Orleans.AspNetCore.SignalR.Grains
{
    [PreferLocalPlacement] // We want this grain to be hosted in same silo as the signalr connections on that machine. Orleans docs: https://dotnet.github.io/orleans/docs/grains/grain_placement.html
    public class HubGrain : Grain, IHubGrain
    {
        readonly ILogger<HubGrain> _logger;
        readonly IPersistentState<ServerState> _storage;
        readonly ObserverSubscriptionManager<IServerObserver> _observers;

        public HubGrain(ILogger<HubGrain> logger, [PersistentState("storage", "serverStore")] IPersistentState<ServerState> storage)
        {
            _logger = logger;
            _storage = storage;
            _observers = new ObserverSubscriptionManager<IServerObserver>();
        }

        public override Task OnActivateAsync()
        {
            return base.OnActivateAsync();
        }

        public override async Task OnDeactivateAsync()
        {
            if (!_storage.State.IsActivated)
            {
                await _storage.ClearStateAsync();
            }

            await base.OnDeactivateAsync();
        }

        public Task Activate()
        {
            _storage.State.IsActivated = true;

            return Task.CompletedTask;
        }

        public Task Deactivate()
        {
            _storage.State.IsActivated = false;

            return Task.CompletedTask;
        }

        public Task Subscribe(IServerObserver observer)
        {
            _observers.Subscribe(observer);

            return Task.CompletedTask;
        }

        public Task Unsubscribe(IServerObserver observer)
        {
            _observers.Unsubscribe(observer);

            return Task.CompletedTask;
        }

        public Task SendConnection(string connectionId, Immutable<InvocationMessage> message)
        {
            _observers.Notify(observer => observer.SendHubConnection(connectionId, message));

            return Task.CompletedTask;
        }
    }

    public class ServerState
    {
        public bool IsActivated { get; set; }
    }
}
