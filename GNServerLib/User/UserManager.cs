using log4net;
using GNPacketLib;
using System;
using System.Collections.Generic;

namespace GNServerLib.User
{
    internal class UserManager : SubManager
    {
        private ILog _logger = LogManager.GetLogger(nameof(UserManager));

        private List<UserConnection> _connections;
        private ulong _uidSeq = 0;

        private Queue<UserProccess> _procQueue;

        public UserManager(GameManager gameManager) : base(gameManager)
        {
            _connections = new List<UserConnection>();

            _procQueue = new Queue<UserProccess>();

            _logger.Info("Successfully initialized.");
        }

        public void CreateConnection(UserSocket socket)
        {
            lock (_connections)
            {
                var conn = new UserConnection(socket, gameManager, _uidSeq++);
                _connections.Add(conn);
            }
        }

        public void RemoveConnection(UserConnection conn)
        {
            lock (_connections)
                _connections.Remove(conn);
        }

        public void EnqueueProc(UserConnection conn, GNPacket packet)
        {
            lock (_procQueue)
            {
                var process = new UserProccess
                {
                    Connection = conn,
                    Packet = packet,
                };
                _procQueue.Enqueue(process);
            }
        }

        public void HandlePacket()
        {
            try
            {
                lock (_procQueue)
                {
                    if (_procQueue.Count > 0)
                    {
                        var process = _procQueue.Dequeue();
                        process.Connection.ProcPacket(process.Packet);
                    }
                }
            }
            catch (Exception exception)
            {
                _logger.Error(exception);
            }
        }
    }
}
