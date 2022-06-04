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

namespace RoundsVC
{
    [BepInPlugin(ModId, ModName, Version)]
    [BepInProcess("Rounds.exe")]
    public class RoundsVC : BaseUnityPlugin
    {
        private const string ModId = "root.rounds.vc";
        private const string ModName = "RoundsVC";
        private static string CompatibilityModName = ModName.Replace(" ", "_");
        public const string Version = "0.0.0";

        public const int SampleRate = 11025; // must be between 11025 and 48000

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
        public static float InputGain
        {
            get => PlayerPrefs.GetFloat(GetConfigKey("InputGain"), 0f);
            set => PlayerPrefs.SetFloat(GetConfigKey("InputGain"), value);
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
            // Use this to call any harmony patch files your Mod may have
            var harmony = new Harmony(ModId);
            harmony.PatchAll();
        }
        void Start()
        {
            this.gameObject.GetOrAddComponent<VoiceChat>();

            VoiceChat.AddChannel(new VoiceChannels.LobbyChannel());
            VoiceChat.AddChannel(new VoiceChannels.DefaultChannel());

            Unbound.RegisterMenu(ModName, () =>
            {
            }, GUI, null, true);
        }
        private static void GUI(GameObject menu)
        {
            MenuHandler.CreateText(ModName, menu, out TextMeshProUGUI _, 60);
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);
            MenuHandler.CreateSlider("Global Volume", menu, 30, 0f, 2f, GlobalOutputVolume, (val) => { GlobalOutputVolume = val; } , out var _, false);
            //MenuHandler.CreateSlider("Input Gain", menu, 30, 0f, 1f, InputGain, (val) => { InputGain = val; } , out var _, false);
            GameObject lobbyMenu = MenuHandler.CreateMenu("LOBBY VOLUME", () => { }, menu, 60, true, true, menu.transform.parent.gameObject);
            lobbyMenu.GetOrAddComponent<LobbyMenuUpdater>();
        }
    }
    class LobbyMenuUpdater : MonoBehaviour
    {
        bool inLobby = false;
        int numPlayers = -1;
        List<Slider> sliders = new List<Slider>() { };
        void Update()
        {
            if (inLobby && PhotonNetwork.CurrentRoom == null)
            {
                ClearAllSliders();
                inLobby = false;
                return;
            }
            if (!inLobby && PhotonNetwork.CurrentRoom != null)
            {
                ClearAllSliders();
                CreateAllSliders();
                inLobby = true;
                return;
            }
            if (inLobby && numPlayers != PhotonNetwork.CurrentRoom.Players.Count)
            {
                ClearAllSliders();
                CreateAllSliders();
                numPlayers = PhotonNetwork.CurrentRoom.Players.Count;
                return;
            }
        }
        void CreateAllSliders()
        {
            foreach (Photon.Realtime.Player player in PhotonNetwork.CurrentRoom.Players.Values)
            {
                MenuHandler.CreateSlider($"{player.NickName}", this.gameObject, 30, 0f, 2f, RoundsVC.GetPlayerOutputVolume(player.NickName), (val) => { RoundsVC.SetPlayerOutputVolume(player.NickName, val); }, out var _, false);
            }
            numPlayers = PhotonNetwork.CurrentRoom.Players.Count;
        }
        void ClearAllSliders()
        {
            foreach (Slider slider in sliders)
            {
                if (slider != null)
                {
                    GameObject.DestroyImmediate(slider.gameObject);
                }
            }
            sliders.Clear();
        }
    }
}
