using GNPacketLib;
using System;
using UnityEngine;

namespace GNClientLib
{
    public static class Client
    {
        private static ClientBehaviour _client => ClientBehaviour.Instance;

        public static void ConnectToServer(string ip, ushort port) => _client.ConnectToServer(ip, port);

        public static void SendPacket(GNPacket packet) => _client.SendPacket(packet);
        
        public static void SendAndReceive<T>(GNPacket packet, Action<T> recvProc) where T : GNPacket => _client.SendAndReceive(packet, recvProc);
        
        public static void Subscribe(NetworkBehaviour network) => _client.Subscribe(network);
        
        public static void Describe(NetworkBehaviour network) => _client.Describe(network);
    }
}
