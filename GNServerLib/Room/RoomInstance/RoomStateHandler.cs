using System;
using System.Collections;

namespace GNServerLib.Room
{
    internal partial class RoomInstance
    {
        private IEnumerator OnStatement(RM_UpdateStatus message)
        {
            Info.BeginHandleState();
            Info.SetGameState(message.State);

            _logger.Info($"Room({RoomIdTag}) game state is {message.State}.");

            try
            {
                var work = HandleState(message);

                while (work.MoveNext())
                {
                    if (work.Current == null)
                        throw new Exception($"State method must end with {nameof(RM_UpdateStatus)}.");

                    if (work.Current is RM_UpdateStatus status)
                    {
                        EnqueueMessage(status);
                        break;
                    }

                    yield return work.Current;
                }
            }
            finally
            {
                Info.EndHandleState();
            }

            yield return null;
        }

        private IEnumerator HandleState(RM_UpdateStatus message)
        {
            switch (message.State)
            {
                case GameStates.Prepare:
                    return OnPrepareState();
                case GameStates.Start:
                    return OnStartState();
                default:
                    throw new Exception($"Can not handle a state message! : {message}");
            }
        }

        private IEnumerator OnPrepareState()
        {
            var roomInfo = Info.ToPacket();
            Info.BroadcastPacket(roomInfo);

            yield return new WaitForSeconds(1);

            yield return new RM_UpdateStatus(GameStates.Start);
        }

        private IEnumerator OnStartState()
        {
            yield return new WaitUntil(() => false);
        }
    }
}
