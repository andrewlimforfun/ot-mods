using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Alpha.Core.Util;
using PurrNet;
using UnityEngine;

namespace Echo.Core
{
    public static class MoveUtil
    {
        public static void Teleport(UnityEngine.Vector3 targetPosition)
        {
            // var steamId= PlayerUtils.FindPlayerBySteamID();
            // var playerDetail = PlayersListUtil.GetPlayerBySteamID(steamId);
            // if (playerDetail == null)
            // {
            //     return;
            // }

            // var playerTransform = playerDetail.PlayerTransform;
            // var cc = playerTransform.GetComponent<CharacterController>();
            // cc.enabled = false;
            // playerTransform.position = targetPosition;
            // cc.enabled = true;
            // NetworkSingleton<TextChannelManager>.I.MainPlayer.position = playerTransform.position;
        }

    }

}
