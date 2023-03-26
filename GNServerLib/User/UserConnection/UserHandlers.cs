using GNPacketLib;
using GNServerLib.Room;
using System;

namespace GNServerLib.User
{
    internal partial class UserConnection
    {
        private void OnConnected(GNP_Connect p)
        {
            _logger.Info($"User({UidTag}) has been connected.");
        }

        private void OnDisconnected(GNP_Disconnect p)
        {
            if (Info.Matching)
            {
                EnqueuePacket(new GNP_Match(GNP_Match.REQUESTS.CANCEL));
            }
            else if (Info.Joined)
            {
                EnqueuePacket(new GNP_UserExit());
            }

            _userManager.RemoveConnection(this);

            _logger.Info($"User({UidTag}) has been disconnected.");
        }

        private void OnLogin(GNP_Login p)
        {
            Info.SetUsername(p.Username);

            _logger.Info($"{Info.Username}(Uid:{UidTag}) has been logined");

            var result = new GNP_LoginRes(GNP_LoginRes.RESULTS.SUCCESS, Uid, Info.Username);

            SendPacket(result);
        }

        private void OnMatch(GNP_Match p)
        {
            switch (p.Request)
            {
                case GNP_Match.REQUESTS.START:
                {
                    Info.StartMatch();
                    _matchManager.AddConnection(this);
                    _logger.Info($"{Info.Username} started to wait for a match making.");
                }
                break;
                case GNP_Match.REQUESTS.CANCEL:
                {
                    Info.CancelMatch();
                    _matchManager.RemoveConnection(Uid);
                    _logger.Info($"{Info.Username} canceled to wait for a match making.");
                }
                break;
                case GNP_Match.REQUESTS.SUCCESS:
                {
                    Info.CancelMatch();
                    _logger.Info($"{Info.Username} has been succeed match making.");
                }
                break;
                default:
                {
                    throw new Exception($"Can not process request! : {p.Request}");
                }
            }

            var requestIdx = (byte)p.Request;
            var result = new GNP_MatchRes((GNP_MatchRes.RESULTS)requestIdx);

            SendPacket(result);
        }

        private void OnRoomCreate(GNP_RoomCreate p)
        {
            SendPacket(p);
        }

        private void OnUserJoin(GNP_UserJoin p)
        {
            var message = new RM_UserJoin(Uid, Info.Username);

            Info.CurrentRoom.EnqueueMessage(message);

            var result = new GNP_UserJoinRes(GNP_UserJoinRes.RESULTS.SUCCESS);

            SendPacket(result);
        }

        private void OnUserExit(GNP_UserExit p)
        {
            var message = new RM_UserExit(Uid, Info.Username);

            Info.CurrentRoom.EnqueueMessage(message);

            var result = new GNP_UserExitRes(GNP_UserExitRes.RESULTS.SUCCESS);

            SendPacket(result);
        }

        private void OnRoomInfo(GNP_RoomInfo p)
        {
            SendPacket(p);
        }
    }
}
