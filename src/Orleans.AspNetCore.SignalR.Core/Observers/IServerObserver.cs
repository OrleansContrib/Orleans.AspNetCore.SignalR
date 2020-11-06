using Microsoft.AspNetCore.SignalR.Protocol;
using Orleans.Concurrency;

namespace Orleans.AspNetCore.SignalR.Observers
{
    public interface IServerObserver : IGrainObserver
    {
        void SendHubConnection(string connectionId, Immutable<InvocationMessage> message);

        void SendToGroup(string groupId, Immutable<InvocationMessage> message);

        void SendToUser(string userId, Immutable<InvocationMessage> message);
    }
}