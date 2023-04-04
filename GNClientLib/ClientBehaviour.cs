using GNPacketLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GNClientLib
{
    internal class ClientBehaviour : MonoBehaviour
    {
        private static ClientBehaviour _instance;
        internal static ClientBehaviour Instance
        {
            get
            {
                if (_instance == null)
                {
                    var gameObject = new GameObject("Client");
                    DontDestroyOnLoad(gameObject);

                    var instance = gameObject.AddComponent<ClientBehaviour>();
                    instance.Initialize();
                    _instance = instance;
                }

                lock (_instance)
                    return _instance;
            }
        }

        private ClientSocket _socket;

        private Queue<GNPacket> _packetQueue;
        private NetworkBehaviour _handlingScene;

        private Queue<Action<GNPacket>> _recvProcQueue;

        private void Initialize()
        {
            _socket = new ClientSocket();
            _socket.AttachBehaviour(this);

            _packetQueue = new Queue<GNPacket>();
            _recvProcQueue = new Queue<Action<GNPacket>>();
        }

        internal void ConnectToServer(string ip, ushort port)
        {
            lock (_socket)
                _socket.ConnectToServer(ip, port);
        }

        internal void EnqueuePacket(GNPacket packet)
        {
            lock (_packetQueue)
            {
                _packetQueue.Enqueue(packet);

                Debug.Log($"<color=#00FF00>Packet Received:</color> <color=#FFFFFF>[{packet.GetType().Name}]</color>");
            }
        }

        internal void SendPacket(GNPacket packet)
        {
            lock (_socket)
            {
                _socket.SendData(packet.ToBytes());

                Debug.Log($"<color=#FFFF00>Packet Sended:</color> <color=#FFFFFF>[{packet.GetType().Name}]</color>");
            }
        }

        internal void SendAndReceive<T>(GNPacket packet, Action<T> recvProc) where T : GNPacket
        {
            SendPacket(packet);

            lock (_recvProcQueue)
            {
                Action<GNPacket> process = (GNPacket recvPacket) =>
                {
                    recvProc.Invoke((T)recvPacket);
                };
                _recvProcQueue.Enqueue(process);
            }
        }

        internal void Subscribe(NetworkBehaviour scene)
        {
            if (_handlingScene)
                return;

            _handlingScene = scene;
        }

        internal void Describe(NetworkBehaviour scene)
        {
            if (_handlingScene == scene)
                _handlingScene = null;
        }

        private void Update()
        {
            try
            {
                lock (_packetQueue)
                {
                    if (_packetQueue.Count > 0)
                    {
                        var packet = _packetQueue.Peek();
                        lock (_recvProcQueue)
                        {
                            if (_recvProcQueue.Count > 0)
                            {
                                var recvProc = _recvProcQueue.Dequeue();
                                recvProc.Invoke(packet);
                                _packetQueue.Dequeue();
                                return;
                            }
                        }

                        if (_handlingScene)
                        {
                            _handlingScene.OnPacketReceived(packet);
                            _packetQueue.Dequeue();
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Debug.LogError(exception);
            }
        }
    }
}
