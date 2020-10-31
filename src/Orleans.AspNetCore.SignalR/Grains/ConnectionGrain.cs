using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.Extensions.Logging;
using Orleans.Concurrency;
using Orleans.Placement;
using Orleans.Runtime;
using System;
using System.Threading.Tasks;

namespace Orleans.AspNetCore.SignalR.Grains
{
    [PreferLocalPlacement]
    public class ConnectionGrain : Grain, IConnectionGrain
    {
        readonly ILogger _logger;
        readonly IPersistentState<ConnectionState> _storage;

        public ConnectionGrain(ILogger<ConnectionGrain> logger, [PersistentState("storage", "connectionStore")] IPersistentState<ConnectionState> storage)
        {
            _logger = logger;
            _storage = storage;
        }

        //public override Task OnActivateAsync()
        //{
        //    return base.OnActivateAsync();
        //}

        public override async Task OnDeactivateAsync()
        {
            await _storage.WriteStateAsync();

            await base.OnDeactivateAsync();
        }

        public Task Connect(string hubId)
        {
            _storage.State.HubId = hubId;

            return Task.CompletedTask;
        }

        public Task Disconnect(string reason = null)
        {
            _storage.ClearStateAsync();

#if DEBUG
            DeactivateOnIdle();
#endif
            return Task.CompletedTask;
        }

        public Task Send(Immutable<InvocationMessage> message)
        {
            if (string.IsNullOrEmpty(_storage.State.HubId)) throw new Exception("ConnectionGrain ....");

            var hubGrain = GrainFactory.GetGrain<IHubGrain>(_storage.State.HubId);
            return hubGrain.SendConnection(this.GetPrimaryKeyString(), message);
        }
    }

    public class ConnectionState
    {
        public string HubId { get; set; }
    }
}
