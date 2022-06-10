﻿using UnityEngine;
using UnboundLib;
using Photon.Pun;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
namespace RoundsVC.UI
{
    public class VCUIHandler : MonoBehaviour
    {
        public static VCUIHandler Instance;
        private static GameObject vcCanvasPrefab = RoundsVC.Assets.LoadAsset<GameObject>("RoundsVC");
        private static GameObject vcPlayerBoxPrefab = RoundsVC.Assets.LoadAsset<GameObject>("PlayerBox");
        private static bool Inited = false;
        public static GameObject vcCanvas;
        public static GameObject vcLayoutGroup => vcCanvas.transform.Find("Canvas").Find("LayoutGroup").gameObject;

        private static Dictionary<int,int> actorIDsTalking = new Dictionary<int, int>() { };

        private const int LocalPlayerID = -99;

        void Start()
        {
            Instance = this;
            Init();
        }
        void Update()
        {
            foreach (KeyValuePair<int, int> actorIDandChannelID in actorIDsTalking)
            {
                int actorID = actorIDandChannelID.Key;
                int channelID = actorIDandChannelID.Value;
                Transform playerBox = vcLayoutGroup.transform.Find($"{actorID}");
                if (playerBox is null)
                {
                    playerBox = GameObject.Instantiate(vcPlayerBoxPrefab, vcLayoutGroup.transform).transform;
                    playerBox.gameObject.name = $"{actorID}";
                }
                playerBox.GetComponent<Image>().color = VoiceChat.VoiceChannels[channelID].ChannelColor;
                TextMeshProUGUI Text = playerBox.GetComponentInChildren<TextMeshProUGUI>();
                Text.text = $"<smallcaps>{(actorID == LocalPlayerID ? PhotonNetwork.LocalPlayer.NickName : PhotonNetwork.CurrentRoom.Players[actorID].NickName)}{(RoundsVC.DEBUG ? $" [{VoiceChat.VoiceChannels[channelID].ChannelName}]" : "")}";
                Text.color = Color.white;
                playerBox.gameObject.GetOrAddComponent<PlayerBoxFade>().ResetTimer();
                playerBox.gameObject.SetActive(true);
                if (actorID == LocalPlayerID)
                {
                    playerBox.SetAsFirstSibling();
                }
            }
            actorIDsTalking.Clear();
        }
        internal static void Init()
        {
            if (Inited) { return; }
            try
            {
                vcCanvas = GameObject.Instantiate(vcCanvasPrefab);
                GameObject.DontDestroyOnLoad(vcCanvas);

                // remove the blank playerbox
                vcLayoutGroup.transform.Find("PlayerBox").gameObject.SetActive(false);

                Inited = true;
            }
            catch
            {
                RoundsVC.LogError("UI Initialization failed! Trying again in 1 second.");
                RoundsVC.Instance.ExecuteAfterSeconds(1f, () => Init());
            }
        }
        public static void PlayerTalking(int actorID, int channelID)
        {
            // return if there isn't a matching player
            if (PhotonNetwork.CurrentRoom is null
                || !PhotonNetwork.CurrentRoom.Players.ContainsKey(actorID)) { return; }
            if (actorID == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                // make sure local player is always bottommost (or topmost?) icon
                actorIDsTalking[LocalPlayerID] = channelID;
            }
            else
            {
                actorIDsTalking[actorID] = channelID;
            }

        }
    }
}
