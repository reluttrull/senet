using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace SenetServer.SignalR
{
    public interface IUserConnectionManager
    {
        void RegisterConnection(string userId, string connectionId);
        void UnregisterConnection(string connectionId);
        IReadOnlyCollection<string> GetConnections(string userId);
        bool HasConnections(string userId);
    }

    public sealed class UserConnectionManager : IUserConnectionManager
    {
        // userId -> set of connectionIds
        private readonly ConcurrentDictionary<string, HashSet<string>> _userConnections =
            new(StringComparer.Ordinal);

        // connectionId -> userId
        private readonly ConcurrentDictionary<string, string> _connectionToUser =
            new(StringComparer.Ordinal);

        public void RegisterConnection(string userId, string connectionId)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(connectionId)) return;

            var connections = _userConnections.GetOrAdd(userId, _ => new HashSet<string>());
            lock (connections)
            {
                connections.Add(connectionId);
            }

            _connectionToUser[connectionId] = userId;
        }

        public void UnregisterConnection(string connectionId)
        {
            if (string.IsNullOrEmpty(connectionId)) return;

            if (!_connectionToUser.TryRemove(connectionId, out var userId)) return;

            if (_userConnections.TryGetValue(userId, out var connections))
            {
                lock (connections)
                {
                    connections.Remove(connectionId);
                    if (connections.Count == 0)
                    {
                        _userConnections.TryRemove(userId, out _);
                    }
                }
            }
        }

        public IReadOnlyCollection<string> GetConnections(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return Array.Empty<string>();
            if (_userConnections.TryGetValue(userId, out var connections))
            {
                lock (connections)
                {
                    return connections.ToArray();
                }
            }
            return Array.Empty<string>();
        }

        public bool HasConnections(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return false;
            if (_userConnections.TryGetValue(userId, out var connections))
            {
                lock (connections)
                {
                    return connections.Count > 0;
                }
            }
            return false;
        }
    }
}