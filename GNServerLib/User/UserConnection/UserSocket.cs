using log4net;
using GNPacketLib;
using System;
using System.Net.Sockets;

namespace GNServerLib.User
{
    internal class UserSocket : IDisposable
    {
        private ILog _logger = LogManager.GetLogger(nameof(UserSocket));

        private Socket _socket;
        private byte[] _recvBuffer;

        public UserConnection Connection { get; private set; }

        public UserSocket(Socket socket)
        {
            _socket = socket;
            _recvBuffer = new byte[GNPacket.RECV_BUFFER_SIZE];

            _socket.BeginReceive(_recvBuffer, 0, _recvBuffer.Length, SocketFlags.None, OnReceivedData, null);
        }

        public void Dispose()
        {
            _socket.Shutdown(SocketShutdown.Receive);
            _socket.Dispose();
            _socket = null;
        }

        public void AttachConnection(UserConnection conn)
        {
            Connection = conn;
        }

        private void OnReceivedData(IAsyncResult result)
        {
            try
            {
                var dataBytes = _socket.EndReceive(result, out var error);

                GNPacket.CheckDataBytes(dataBytes, error);

                var packet = GNPacket.FromBytes(_recvBuffer, dataBytes);

                Connection.EnqueuePacket(packet);
            }
            catch (Exception exception)
            {
                _logger.Error(exception);

                if (exception is SocketException)
                {
                    Connection.EnqueuePacket(new GNP_Disconnect());
                    Dispose();
                    return;
                }
            }
            
            _socket.BeginReceive(_recvBuffer, 0, _recvBuffer.Length, SocketFlags.None, OnReceivedData, null);
        }

        public void SendData(byte[] dataBytes)
        {
            _socket?.BeginSend(dataBytes, 0, dataBytes.Length, SocketFlags.None, OnSendedData, null);
        }

        private void OnSendedData(IAsyncResult result)
        {
            try
            {
                var dataBytes = _socket.EndSend(result, out var error);

                GNPacket.CheckDataBytes(dataBytes, error);
            }
            catch (Exception exception)
            {
                _logger.Error(exception);
            }
        }
    }
}
