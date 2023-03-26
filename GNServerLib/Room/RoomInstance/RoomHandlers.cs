using GNPacketLib;
using System.Collections;

namespace GNServerLib.Room
{
    internal partial class RoomInstance
    {
        private IEnumerator OnCreate(RM_Create message)
        {
            Info.Created(this);

            _logger.Info($"Room({RoomIdTag}) has been created.");

            yield return new WaitForSeconds(1);

            Info.BroadcastPacket(new GNP_RoomCreate());

            yield return null;
        }

        private IEnumerator OnDestroy(RM_Destroy message)
        {
            Info.BeginDestroy();
            Info.BroadcastPacket(new GNP_UserExit(), info => info.Joined);

            yield return new WaitUntil(Info.IsAllUserExited);

            Info.EndDestroy();
            _roomManager.RemoveRoom(this);

            _logger.Info($"Room({RoomIdTag}) has been destroyed.");

            yield return null;
        }

        private IEnumerator OnUserJoin(RM_UserJoin message)
        {
            Info.UserJoinRoom(message.Uid);

            _logger.Info($"User({message.Username}) has been joined to Room({RoomIdTag}).");

            if (Info.IsAllUserJoined())
            {
                var statement = new RM_UpdateStatus(GameStates.Prepare);
                EnqueueMessage(statement);
            }

            yield return null;
        }

        private IEnumerator OnUserExit(RM_UserExit message)
        {
            Info.UserExitRoom(message.Uid);

            if (!Info.IsDestroying())
            {
                EnqueueMessage(new RM_Destroy());
            }

            _logger.Info($"User({message.Username}) has been exited from Room({RoomIdTag}).");

            yield return null;
        }
    }
}
