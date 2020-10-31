using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.Extensions.Logging;
using Orleans.AspNetCore.SignalR.Grains;
using Orleans.AspNetCore.SignalR.Observers;
using Orleans.Concurrency;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Orleans.AspNetCore.SignalR
{
    public sealed class OrleansHubLifetimeManager<THub> : HubLifetimeManager<THub>, IServerObserver, IDisposable where THub : Hub
    {
        readonly HubConnectionStore _connections;

        readonly ILogger _logger;
        readonly IClusterClient _cluster;
        readonly string _hubId;

        public OrleansHubLifetimeManager(ILogger<OrleansHubLifetimeManager<THub>> logger, IClusterClient cluster, string hubId = null)
        {
            _logger = logger;
            _connections = new HubConnectionStore();

            _cluster = cluster;
            _hubId = hubId ?? typeof(THub).Name;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public override Task OnConnectedAsync(HubConnectionContext connection)
        {
            try
            {
                _connections.Add(connection);

                var connectionGrain = _cluster.GetGrain<IConnectionGrain>(connection.ConnectionId);
                return connectionGrain.Connect(_hubId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OrleansHubLifetimeManager.OnConnectedAsync() failed");
                throw ex;
            }
        }

        public override Task OnDisconnectedAsync(HubConnectionContext connection)
        {
            try
            {
                _connections.Remove(connection);

                var connectionGrain = _cluster.GetGrain<IConnectionGrain>(connection.ConnectionId);
                return connectionGrain.Disconnect();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OrleansHubLifetimeManager.OnDisconnectedAsync() failed");
            }

            return Task.CompletedTask;
        }

        // IServerObserver
        public void SendHubConnection(string connectionId, Immutable<InvocationMessage> message)
        {
            _ = SendConnectionAsync(connectionId, message.Value?.Target, message.Value?.Arguments);
        }

        // Connections
        public override Task SendAllAsync(string methodName, object[] args, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task SendAllExceptAsync(string methodName, object[] args, IReadOnlyList<string> excludedConnectionIds, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task SendConnectionAsync(string connectionId, string methodName, object[] args, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionId)) throw new ArgumentNullException(nameof(connectionId));
            if (string.IsNullOrWhiteSpace(methodName)) throw new ArgumentNullException(nameof(methodName));

            var message = new InvocationMessage(methodName, args);

            var connection = _connections[connectionId];
            if (connection != null)
                return SendLocal(connection, message, cancellationToken);
            else
                return SendExternal(connectionId, message);
        }

        public override Task SendConnectionsAsync(IReadOnlyList<string> connectionIds, string methodName, object[] args, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        // Groups
        public override Task AddToGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task RemoveFromGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task SendGroupAsync(string groupName, string methodName, object[] args, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task SendGroupExceptAsync(string groupName, string methodName, object[] args, IReadOnlyList<string> excludedConnectionIds, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task SendGroupsAsync(IReadOnlyList<string> groupNames, string methodName, object[] args, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        // Users
        public override Task SendUserAsync(string userId, string methodName, object[] args, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task SendUsersAsync(IReadOnlyList<string> userIds, string methodName, object[] args, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        // Internals
        Task SendLocal(HubConnectionContext connection, HubInvocationMessage hubMessage, CancellationToken cancellationToken = default)
        {
#if DEBUG
            _logger.LogDebug($"SendLocal(connection:{connection}, hubMessage:{hubMessage}, cancellationToken:{cancellationToken}");
#endif
            return connection.WriteAsync(hubMessage, cancellationToken).AsTask();
        }

        Task SendExternal(string connectionId, InvocationMessage hubMessage)
        {
#if DEBUG
            _logger.LogDebug($"SendExternal(connectionId:{connectionId}, hubMessage:{hubMessage}");
#endif
            var connectionGrain = _cluster.GetGrain<IConnectionGrain>(connectionId);
            return connectionGrain.Send(hubMessage.AsImmutable());
        }
    }
}
