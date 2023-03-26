using log4net;
using GNServerLib.Room;
using GNServerLib.User;
using System;
using System.Collections.Generic;
using GNPacketLib;

namespace GNServerLib.Match
{
    internal class MatchManager : SubManager
    {
        private ILog _logger = LogManager.GetLogger(nameof(MatchManager));

        private List<UserConnection> _connections;

        public MatchManager(GameManager gameManager) : base(gameManager)
        {
            _connections = new List<UserConnection>();

            _logger.Info("Successfully initialized.");
        }

        public void AddConnection(UserConnection conn)
        {
            lock (_connections)
                _connections.Add(conn);
        }

        public void RemoveConnection(ulong uid)
        {
            lock (_connections)
            {
                foreach (var conn in _connections)
                {
                    if (conn.Uid == uid)
                    {
                        _connections.Remove(conn);
                        return;
                    }
                }
            }
        }

        public void HandleMatch()
        {
            try
            {
                lock (_connections)
                {
                    if (_connections.Count > 1)
                        MatchConnections();
                }
            }
            catch (Exception exception)
            {
                _logger.Error(exception);
            }
        }

        private void MatchConnections()
        {
            var res = new GNP_Match(GNP_Match.REQUESTS.SUCCESS);

            var conns = new Dictionary<ulong, UserConnection>();
            for (var idx = 0; idx < RoomInfo.MAX_USER; idx++)
            {
                var conn = _connections[0];
                conn.EnqueuePacket(res);

                _connections.Remove(conn);
                conns.Add(conn.Uid, conn);
            }

            roomManager.CreateRoom(conns);
        }
    }
}
