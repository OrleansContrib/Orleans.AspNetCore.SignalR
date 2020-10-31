using Microsoft.AspNetCore.SignalR.Protocol;
using Orleans.Concurrency;
using System.Threading.Tasks;

namespace Orleans.AspNetCore.SignalR.Grains
{
    public interface IConnectionGrain : IGrainWithStringKey
    {
        Task Connect(string hubId);
        Task Disconnect(string reason = null);

        Task Send(Immutable<InvocationMessage> message);
    }
}
