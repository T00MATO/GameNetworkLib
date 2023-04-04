using GNPacketLib;
using System;
using UnityEngine;

namespace GNClientLib
{
    public abstract class NetworkBehaviour : MonoBehaviour
    {
        public abstract void OnPacketReceived(GNPacket packet);

        protected void BeginPacketReceive() => Client.Subscribe(this);

        protected void EndPacketReceive() => Client.Describe(this);

        protected void ConnectToServer(string ip, ushort port) => Client.ConnectToServer(ip, port);

        protected void SendPacket(GNPacket packet) => Client.SendPacket(packet);

        protected void SendAndReceive<T>(GNPacket packet, Action<T> recvProc) where T : GNPacket => Client.SendAndReceive<T>(packet, recvProc);
    }
}
