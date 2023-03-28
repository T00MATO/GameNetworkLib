using GNPacketLib;
using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace GNClientLib
{
    internal class ClientSocket : IDisposable
    {
        private ClientBehaviour _behaviour;

        private Socket _socket;
        private byte[] _recvBuffer;

        public ClientSocket()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _recvBuffer = new byte[GNPacket.RECV_BUFFER_SIZE];
        }

        public void AttachBehaviour(ClientBehaviour behaviour)
        {
            _behaviour = behaviour;
        }
        
        public void Dispose()
        {
            _socket.Shutdown(SocketShutdown.Receive);
            _socket.Dispose();
            _socket = null;
        }

        public void ConnectToServer(string ip, ushort port)
        {
            var address = IPAddress.Parse(ip);
            var endPoint = new IPEndPoint(address, port);

            _socket.BeginConnect(endPoint, OnConnectedToServer, null);
        }

        private void OnConnectedToServer(IAsyncResult result)
        {
            try
            {
                _socket.EndConnect(result);

                _behaviour.EnqueuePacket(new GNP_Connect());

                _socket.BeginReceive(_recvBuffer, 0, _recvBuffer.Length, SocketFlags.None, OnReceivedData, null);
            }
            catch (Exception exception)
            {
                Debug.LogError(exception);
            }
        }

        private void OnReceivedData(IAsyncResult result)
        {
            try
            {
                var dataBytes = _socket.EndReceive(result, out var error);

                GNPacket.CheckDataBytes(dataBytes, error);

                var packet = GNPacket.FromBytes(_recvBuffer, dataBytes);

                _behaviour.EnqueuePacket(packet);
            }
            catch (Exception exception)
            {
                Debug.LogError(exception);

                if (exception is SocketException)
                {
                    _behaviour.EnqueuePacket(new GNP_Disconnect());
                    Dispose();
                    return;
                }
            }

            _socket.BeginReceive(_recvBuffer, 0, _recvBuffer.Length, SocketFlags.None, OnReceivedData, null);
        }

        public void SendData(byte[] dataBytes)
        {
            _socket.BeginSend(dataBytes, 0, dataBytes.Length, SocketFlags.None, OnSendedData, null);
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
                Debug.LogError(exception);
            }
        }
    }
}
