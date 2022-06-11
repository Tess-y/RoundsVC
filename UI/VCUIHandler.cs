using UnityEngine;
using UnboundLib;
using Photon.Pun;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using RoundsVC.Extensions;
namespace RoundsVC.UI
{
    public class VCUIHandler : MonoBehaviour
    {
        public static VCUIHandler Instance;
        private static GameObject vcCanvasPrefab = RoundsVC.Assets.LoadAsset<GameObject>("RoundsVC");
        private static GameObject vcPlayerBoxPrefab = RoundsVC.Assets.LoadAsset<GameObject>("PlayerBox");
        private static GameObject vcPlayerVCIconPrefab = RoundsVC.Assets.LoadAsset<GameObject>("PlayerVCIcon");
        private static bool Inited = false;
        public static GameObject vcCanvas;
        public static GameObject vcLayoutGroup => vcCanvas.transform.Find("Canvas").Find("LayoutGroup").gameObject;

        private static Dictionary<int,int> actorIDsTalking = new Dictionary<int, int>() { };

        private static float UIScale => RoundsVC.UIScale;
        private static float UIOpacity => RoundsVC.UIOpacity;
        private static bool UIOnLeft => RoundsVC.UIOnLeft;

        private const int LocalPlayerID = -99;
        private const int DemoPlayerID1 = -100;
        private const int DemoPlayerID2 = -101;
        private const int DemoPlayerID3 = -102;
        private const int DemoPlayerID4 = -103;

        internal static void UpdateVisuals()
        {
            vcLayoutGroup.transform.localScale = new Vector3(UIScale, UIScale, 1);
            vcLayoutGroup.GetComponent<VerticalLayoutGroup>().childAlignment = UIOnLeft ? TextAnchor.LowerLeft : TextAnchor.LowerRight;
        }

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
                playerBox.GetComponent<Image>().color = VoiceChat.VoiceChannels[channelID].ChannelColor.WithOpacity(UIOpacity);
                TextMeshProUGUI Text = playerBox.GetComponentInChildren<TextMeshProUGUI>();
                if (actorID > DemoPlayerID1) { Text.text = $"<smallcaps>{(actorID == LocalPlayerID ? PhotonNetwork.LocalPlayer.NickName : PhotonNetwork.CurrentRoom.Players[actorID].NickName)}{(RoundsVC.DEBUG ? $" [{VoiceChat.VoiceChannels[channelID].ChannelName}]" : "")}"; }
                else
                {
                    Text.text = $"<smallcaps>Player{-actorID + DemoPlayerID1 + 1}";
                }
                Text.color = Color.white.WithOpacity(UIOpacity);
                playerBox.gameObject.GetOrAddComponent<PlayerBoxFade>().ResetTimer();
                playerBox.gameObject.SetActive(true);
                if (actorID == LocalPlayerID)
                {
                    playerBox.SetAsFirstSibling();
                }
                Player player = actorID == LocalPlayerID ? PlayerManager.instance.GetLocalPlayer() : PlayerManager.instance.GetPlayerWithActorID(actorID);
                if (player is null) { continue; }
                Transform wobbleObjects = player.transform.Find("WobbleObjects");
                if (wobbleObjects is null) { continue; }
                Transform playerVCIcon = wobbleObjects.Find("PlayerVCIcon");
                if (playerVCIcon is null)
                {
                    playerVCIcon = GameObject.Instantiate(vcPlayerVCIconPrefab, wobbleObjects).transform;
                    playerVCIcon.localPosition = new Vector3(1.25f, 1.5f, 0f);
                    playerVCIcon.localScale = new Vector3(0.05f, 0.05f, 1f);
                    playerVCIcon.gameObject.name = "PlayerVCIcon";
                }
                playerVCIcon.GetComponent<SpriteRenderer>().color = VoiceChat.VoiceChannels[channelID].ChannelColor.WithOpacity(UIOpacity);
                playerVCIcon.gameObject.GetOrAddComponent<PlayerVCIconFade>().ResetTimer();
                playerVCIcon.gameObject.SetActive(true);
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
                vcLayoutGroup.GetOrAddComponent<MatchScreenBounds>();

                Inited = true;
            }
            catch
            {
                RoundsVC.LogError("UI Initialization failed! Trying again in 1 second.");
                RoundsVC.Instance.ExecuteAfterSeconds(1f, () => Init());
            }
            UpdateVisuals();
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
        internal static void DemoPlayerTalking(int demoPlayer)
        {
            switch (demoPlayer)
            {
                case 0:
                    actorIDsTalking[DemoPlayerID1] = 0;
                    break;
                case 1:
                    actorIDsTalking[DemoPlayerID2] = 0;
                    break;
                case 2:
                    actorIDsTalking[DemoPlayerID3] = 0;
                    break;
                case 3:
                    actorIDsTalking[DemoPlayerID4] = 0;
                    break;
                default:
                    break;
            }

        }
    }
}
