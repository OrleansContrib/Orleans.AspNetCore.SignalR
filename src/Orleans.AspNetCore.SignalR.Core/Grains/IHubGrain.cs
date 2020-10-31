using Microsoft.AspNetCore.SignalR.Protocol;
using Orleans.AspNetCore.SignalR.Observers;
using Orleans.Concurrency;
using System.Threading.Tasks;

namespace Orleans.AspNetCore.SignalR.Grains
{
    public interface IHubGrain : IGrainWithStringKey
    {
        Task Activate();
        Task Deactivate();

        Task Subscribe(IServerObserver observer);
        Task Unsubscribe(IServerObserver observer);
        Task SendConnection(string connectionId, Immutable<InvocationMessage> message);
    }
}
