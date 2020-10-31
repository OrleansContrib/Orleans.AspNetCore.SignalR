using Microsoft.AspNetCore.SignalR.Protocol;
using Orleans.Concurrency;

namespace Orleans.AspNetCore.SignalR.Observers
{
    public interface IServerObserver : IGrainObserver
    {
        void SendHubConnection(string connectionId, Immutable<InvocationMessage> message);
    }
}
