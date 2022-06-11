using System;
using BepInEx;
using HarmonyLib;
using Steamworks;
using UnboundLib;
using UnboundLib.Utils.UI;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;
using System.Collections.Generic;
using System.Collections;
using Jotunn.Utils;
using RoundsVC.UI;

namespace RoundsVC
{
    [BepInPlugin(ModID, ModName, Version)]
    [BepInProcess("Rounds.exe")]
    public class RoundsVC : BaseUnityPlugin
    {
        private const string ModID = "pykess-and-root.plugins.rounds.vc";
        private const string ModName = "RoundsVC";
        private static string CompatibilityModName = ModName.Replace(" ", "_");
        public const string Version = "0.0.0";

        public const int SampleRate = 48000; // must be between 11025 and 48000
        internal static AssetBundle Assets;
        internal static RoundsVC Instance;
        private static Coroutine OptionsMenuDemoCO = null;

#if DEBUG
        public const bool DEBUG = true;
#else
        public const bool DEBUG = false;
#endif
        public static float GlobalOutputVolume
        {
            get => PlayerPrefs.GetFloat(GetConfigKey("GlobalOutputVolume"), 1f);
            set => PlayerPrefs.SetFloat(GetConfigKey("GlobalOutputVolume"), value);
        }
        public static bool UIOnLeft
        {
            get => PlayerPrefs.GetInt(GetConfigKey("UIPositionOnLeft"), 1) == 1;
            set => PlayerPrefs.SetInt(GetConfigKey("UIPositionOnLeft"), value ? 1 : 0);
        }
        public static float UIScale
        {
            get => PlayerPrefs.GetFloat(GetConfigKey("UIScale"), 1f);
            set => PlayerPrefs.SetFloat(GetConfigKey("UIScale"), value);
        }
        public static float UIOpacity
        {
            get => PlayerPrefs.GetFloat(GetConfigKey("UIOpacity"), 1f);
            set => PlayerPrefs.SetFloat(GetConfigKey("UIOpacity"), value);
        }
        public static float GetPlayerOutputVolume(string NickName)
        {
            return PlayerPrefs.GetFloat(GetConfigKey($"Player_{NickName}_Volume"), 1f);
        }
        public static void SetPlayerOutputVolume(string NickName, float value)
        {
            PlayerPrefs.SetFloat(GetConfigKey($"Player_{NickName}_Volume"), value);
        }
        public static string GetConfigKey(string key) => $"{CompatibilityModName}_{key}";
        public static void Log(object message)
        {
            if (DEBUG)
            {
                UnityEngine.Debug.Log($"[{ModName}] {message}");
            }
        }
        internal static void LogError(object msg)
        {
            UnityEngine.Debug.LogError($"[{ModName}] {msg}");
        }

        public static float GetVolumeByActorID(int actorID)
        {
            if (PhotonNetwork.CurrentRoom is null) { return 1f; }
            var actor = PhotonNetwork.CurrentRoom.GetPlayer(actorID);
            return GetPlayerOutputVolume(actor.NickName);
        }

        void Awake()
        {
            Instance = this;
            // Use this to call any harmony patch files your Mod may have
            var harmony = new Harmony(ModID);
            harmony.PatchAll();
        }
        void Start()
        {

            try
            {
                Assets = AssetUtils.LoadAssetBundleFromResources("roundsvc", typeof(RoundsVC).Assembly);
                if (Assets == null)
                {
                    LogError("RoundsVC Assets failed to load.");
                }
            }
            catch
            {
                LogError("RoundsVC Assets failed to load.");
            }

            this.gameObject.GetOrAddComponent<VoiceChat>();
            this.gameObject.GetOrAddComponent<VCUIHandler>();

            VoiceChat.AddChannel(new VoiceChannels.LobbyChannel());
            VoiceChat.AddChannel(new VoiceChannels.DefaultChannel());

            Unbound.RegisterMenu(ModName, () =>
            {
                if (OptionsMenuDemoCO != null)
                {
                    StopCoroutine(OptionsMenuDemoCO);
                }
                OptionsMenuDemoCO = StartCoroutine(DemoUI());
            }, GUI, null, true);
        }
        private static IEnumerator DemoUI()
        {
            while (true)
            {
                VCUIHandler.DemoPlayerTalking(0);
                VCUIHandler.DemoPlayerTalking(1);
                VCUIHandler.DemoPlayerTalking(2);
                VCUIHandler.DemoPlayerTalking(3);
                yield return new WaitForSecondsRealtime(0.1f);
            }
        }
        internal static List<string> LobbyNicknames = new List<string>() { };
        private static void ReadLobby()
        {
            LobbyNicknames.Clear();
            if (PhotonNetwork.CurrentRoom is null) { return; }
            foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
            {
                if (player.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber) { continue; }
                LobbyNicknames.Add(player.NickName);
            }

        }
        private static LobbyMenuUpdater LobbyMenuUpdater = null;
        private static void GUI(GameObject menu)
        {
            MenuHandler.CreateText(ModName, menu, out TextMeshProUGUI _, 60);
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);
            MenuHandler.CreateSlider("Global Volume", menu, 30, 0f, 5f, GlobalOutputVolume, (val) => { GlobalOutputVolume = val; } , out var _, false);
            MenuHandler.CreateSlider("UI Scale", menu, 30, 0f, 5f, UIScale, (val) => { UIScale = val; VCUIHandler.UpdateVisuals(); } , out var _, false);
            MenuHandler.CreateSlider("UI Opacity", menu, 30, 0f, 1f, UIOpacity, (val) => { UIOpacity = val; VCUIHandler.UpdateVisuals(); } , out var _, false);
            MenuHandler.CreateToggle(UIOnLeft, "UI Position on Left", menu, (val) => { UIOnLeft = val; VCUIHandler.UpdateVisuals(); }, 30);
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);
            GameObject lobbyMenu = MenuHandler.CreateMenu("LOBBY VOLUME", () => { ReadLobby(); LobbyMenuUpdater.UpdateSliders(); }, menu, 60, true, true, menu.transform.parent.gameObject);
            LobbyMenuUpdater = lobbyMenu.GetOrAddComponent<LobbyMenuUpdater>();

            // Create back actions
            menu.GetComponentInChildren<GoBack>(true).goBackEvent.AddListener(() =>
            {
                if (OptionsMenuDemoCO != null)
                {
                    RoundsVC.Instance.StopCoroutine(OptionsMenuDemoCO);
                }
            });
            menu.transform.Find("Group/Back").gameObject.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (OptionsMenuDemoCO != null)
                {
                    RoundsVC.Instance.StopCoroutine(OptionsMenuDemoCO);
                }
            });
        }
    }
    class LobbyMenuUpdater : MonoBehaviour
    {
        List<Slider> sliders = new List<Slider>() { };
        internal void UpdateSliders()
        {
            this.ClearAllSliders();
            this.CreateAllSliders();
        }
        void CreateAllSliders()
        {
            foreach (string playerName in RoundsVC.LobbyNicknames)
            {
                MenuHandler.CreateSlider($"{playerName}", this.gameObject, 30, 0f, 2f, RoundsVC.GetPlayerOutputVolume(playerName), (val) => { RoundsVC.SetPlayerOutputVolume(playerName, val); }, out Slider slider, false);
                this.sliders.Add(slider);
            }
        }
        void ClearAllSliders()
        {
            foreach (Slider slider in sliders)
            {
                if (slider != null)
                {
                    GameObject.DestroyImmediate(slider.gameObject.transform.parent.parent.gameObject);
                }
            }
            sliders.Clear();
        }
    }
}
