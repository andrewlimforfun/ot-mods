using System.Text;
using PurrNet;
using TMPro;
using UnityEngine;

namespace Alpha.Core.Util
{
    public class PlayerDetail
    {
        public readonly string SteamID;
        public readonly PlayerID PlayerID;
        public readonly PlayerIDInfo PlayerIDInfo;
        public readonly NetworkTransform PlayerTransform;
        public TextMeshProUGUI UserNameTMP => PlayerTransform.GetComponent<PlayerController>().PlayerNameText;
        public string UserName => UserNameTMP.text;
        public string SteamPersonaName => SteamUtils.GetSteamPersonaName(SteamID);
        public Vector3 Position => PlayerTransform.position;


        public PlayerDetail(string steamID, PlayerID playerID, PlayerIDInfo playerIDInfo, NetworkTransform playerTransform)
        {
            SteamID = steamID;
            PlayerID = playerID;
            PlayerIDInfo = playerIDInfo;
            PlayerTransform = playerTransform;
        }
    }
    
}
