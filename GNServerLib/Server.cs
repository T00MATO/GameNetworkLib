using log4net;
using GNServerLib.User;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace GNServerLib
{
    internal class Server : IDisposable
    {
        private ILog _logger = LogManager.GetLogger(nameof(Server));

        private GameManager _gameManager;

        private Socket _socket;

        public Server(ushort port, GameManager gameManager)
        {
            _gameManager = gameManager;

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            var address = IPAddress.Any;
            var endPoint = new IPEndPoint(address, port);
            _socket.Bind(endPoint);
            _socket.Listen((int)SocketOptionName.MaxConnections);

            _logger.Info("Successfully initialized.");
        }

        public void Dispose()
        {
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Dispose();
            _socket = null;

            _logger.Info("Successfully disposed.");
        }

        public void StartAcceptSocket()
        {
            _logger.Info("Started to accept client sockets.");

            _socket.BeginAccept(OnAcceptedSocket, null);
        }

        private void OnAcceptedSocket(IAsyncResult result)
        {
            try
            {
                var connSocket = _socket.EndAccept(result);
                var userSocket = new UserSocket(connSocket);
                _gameManager.UserManager.CreateConnection(userSocket);
            }
            catch (Exception exception)
            {
                _logger.Error(exception);
            }

            _socket.BeginAccept(OnAcceptedSocket, null);
        }
    }
}
