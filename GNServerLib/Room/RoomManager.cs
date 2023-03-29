using log4net;
using GNServerLib.User;
using System;
using System.Collections.Generic;
using System.Threading;

namespace GNServerLib.Room
{
    internal class RoomManager : SubManager
    {
        private static readonly int MAX_THREADS = 4;

        private ILog _logger = LogManager.GetLogger(nameof(RoomManager));

        private List<RoomInstance> _rooms;
        private ulong _roomIdSeq = 0;

        private Queue<RoomProccess> _procQueue;
        private List<List<RoomWork>> _workPools;
        private List<Thread> _workers;

        public RoomManager(GameManager gameManager) : base(gameManager)
        {
            _rooms = new List<RoomInstance>();

            _procQueue = new Queue<RoomProccess>();
            _workPools = new List<List<RoomWork>>();
            _workers = new List<Thread>();
            for (var idx = 0; idx < MAX_THREADS; idx++)
            {
                _workPools.Add(new List<RoomWork>());

                var threadIdx = idx;
                var thread = new Thread(() => HandleWork(threadIdx));
                _workers.Add(thread);
            }

            _logger.Info("Successfully initialized.");
        }

        public void CreateRoom(Dictionary<ulong, UserConnection> conns)
        {
            lock (_rooms)
            {
                var room = new RoomInstance(conns, gameManager, _roomIdSeq++);
                _rooms.Add(room);
            }
        }

        public void RemoveRoom(RoomInstance room)
        {
            lock (_rooms)
                _rooms.Remove(room);
        }

        public void EnqueueProc(RoomInstance room, RoomMessage message)
        {
            lock (_procQueue)
            {
                var process = new RoomProccess(room, message);
                _procQueue.Enqueue(process);
            }
        }

        public void HandleMessage()
        {
            try
            {
                lock (_procQueue)
                {
                    if (_procQueue.Count > 0)
                    {
                        var process = _procQueue.Dequeue();
                        var subRoutine = process.Room.HandleMessage(process.Message);
                        var procWork = new RoomWork(process.Room, subRoutine);
                        LoadBalanceWork(procWork);
                    }
                }
            }
            catch (Exception exception)
            {
                _logger.Error(exception);
            }
        }

        private void LoadBalanceWork(RoomWork procWork)
        {
            var procIdx = -1;
            for (var idx = 0; idx < _workPools.Count; idx++)
            {
                lock (_workPools[idx])
                {
                    if (procIdx < 0)
                    {
                        procIdx = idx;
                        continue;
                    }

                    lock (_workPools[procIdx])
                    {
                        if (_workPools[idx].Count < _workPools[procIdx].Count)
                            procIdx = idx;
                    }
                }
            }

            lock (_workPools[procIdx])
                _workPools[procIdx].Add(procWork);
        }

        public void RunWorkers()
        {
            foreach (var thread in _workers)
                thread.Start();
        }

        private void HandleWork(int threadIdx)
        {
            while (true)
            {
                lock (_workPools[threadIdx])
                {
                    for (var idx = 0; idx < _workPools[threadIdx].Count;)
                    {
                        var work = _workPools[threadIdx][idx];

                        try
                        {
                            if (!IsWorkDone(work))
                            {
                                idx++;
                                continue;
                            }
                        }
                        catch (Exception exception)
                        {
                            _logger.Error(exception);
                        }

                        _workPools[threadIdx].Remove(work);
                    }
                }
            }
        }

        private bool IsWorkDone(RoomWork work)
        {
            if (work.Room.Info.RoomState != RoomStates.Destroyed)
            {
                if (work.Subroutine.Current is IRMCondition condition && !condition.IsFinished())
                    return false;
                
                if (work.Subroutine.MoveNext() && work.Subroutine.Current != null)
                    return false;
            }

            return true;
        }
    }
}
