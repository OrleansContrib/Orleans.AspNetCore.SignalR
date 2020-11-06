using Orleans.AspNetCore.SignalR.Observers;
using System.Threading.Tasks;

namespace Orleans.AspNetCore.SignalR.Grains
{
    public interface IGroupGrain : IGrainWithStringKey
    {
        /// <summary>
        /// Adds server to group
        /// </summary>
        /// <param name="server">The server</param>
        /// <returns></returns>
        Task AddServer(IServerObserver server);

        /// <summary>
        /// This method is used to confirm the server is still up. The grain implementation will
        /// expect this method to be invoked after is calls 'IServerObserver.SendToGroup'. IF this
        /// isn't called after a certain amount of time, the server will be removed from the list of
        /// servers that hold a connection to this group
        /// </summary>
        /// <param name="server"></param>
        /// <returns></returns>
        Task ConfirmConnection(IServerObserver server);

        /// <summary>
        /// Removes server from group
        /// </summary>
        /// <param name="server">the server</param>
        /// <returns></returns>
        Task RemoveServer(IServerObserver server);

        Task SendMessage(string method, params object[] args);
    }
}