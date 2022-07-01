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
using On;
using System.Linq;
using RoundsVC.Utils;
using InControl;

namespace RoundsVC
{
    [BepInPlugin(ModID, ModName, Version)]
    [BepInProcess("Rounds.exe")]
    public class RoundsVC : BaseUnityPlugin
    {
        public enum MicControlType
        {
            /// Open Mic / Push To Talk / Push To Toggle
            /// 0        / 1 (Default)  / 2
            OpenMic = 0,
            PushToTalk = 1,
            PushToToggle = 2
        }

        private const string ModID = "pykess-and-root.plugins.rounds.vc";
        private const string ModName = "RoundsVC";
        private static string CompatibilityModName = ModName.Replace(" ", "_");
        public const string Version = "0.0.0";

        public const int SampleRate = 48000; // must be between 11025 and 48000
        internal static AssetBundle Assets;
        internal static RoundsVC Instance;
        private static Coroutine OptionsMenuDemoCO = null;
        private static GameObject LobbyMenu = null;
        private static List<Slider> LobbySliders = new List<Slider>() { };
        public static bool DefaultChannelEnabled = true;
        private static bool WarnSteamworksUnavailable = true;

        public static LobbyPlayerActions LobbyActions { get; private set; }

#if DEBUG
        public const bool DEBUG = true;
#else
        public const bool DEBUG = false;
#endif
        public static MicControlType MicControl
        {
            /// Open Mic / Push To Talk / Push To Toggle
            /// 0        / 1 (Default)  / 2

            get => (MicControlType)PlayerPrefs.GetInt(GetConfigKey("MicControlType"), (int)MicControlType.PushToTalk);
            set => PlayerPrefs.SetInt(GetConfigKey("MicControlType"), (int)value);
        }
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
            if (WarnSteamworksUnavailable && !SteamManager.Initialized)
            {
                WarnSteamworksUnavailable = false;
                LogError("Steamworks is not initialized. Please make sure you have the Steam client installed and running.");
                Unbound.BuildModal("ROUNDSVC ERROR", "Steamworks is not initialized. Please make sure you have the Steam client installed and running. ROUNDS Voice Chat will not work without it.");
            }
            On.MainMenuHandler.Awake += (orig, self) =>
            {
                this.ExecuteAfterSeconds(0.2f, () =>
                {
                    DefaultChannelEnabled = true;
                });

                orig(self);
            };
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
            this.gameObject.GetOrAddComponent<VoiceControls>();

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

            this.StartCoroutine(SetupLobbyActionsWhenReady());
        }
        private static IEnumerator SetupLobbyActionsWhenReady()
        {
            yield return new WaitUntil(() => InputManager.IsSetup);
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            LobbyActions = LobbyPlayerActions.CreateWithKeyboardBindings();
            yield break;
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
        /*
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
        */
        //private static LobbyMenuUpdater LobbyMenuUpdater = null;
        private static List<Toggle> OpenMic = new List<Toggle>();
        private static List<Toggle> PushToTalk = new List<Toggle>();
        private static List<Toggle> PushToToggle = new List<Toggle>();
        private static List<Toggle> UIOnLeftToggles = new List<Toggle>();
        private static void GUI(GameObject menu)
        {
            MenuHandler.CreateText(ModName, menu, out TextMeshProUGUI _, 60);
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);
            MenuHandler.CreateSlider("Global Volume", menu, 30, 0f, 5f, GlobalOutputVolume, (val) => { GlobalOutputVolume = val; } , out var _, false);
            LobbyMenu = MenuHandler.CreateMenu("LOBBY VOLUME",
                () =>
                {

                    foreach (Slider slider in LobbySliders.Where(s => s != null))
                    {
                        try
                        {
                            DestroyImmediate(slider.gameObject.transform.parent.parent.gameObject);
                        }
                        catch
                        { }
                    }
                    LobbySliders.Clear();
                    if (PhotonNetwork.CurrentRoom is null) { return; }
                    foreach (string nickname in PhotonNetwork.CurrentRoom.Players.Values.Select(p => p.NickName).ToList())
                    {
                        if (nickname == PhotonNetwork.LocalPlayer.NickName) { continue; }

                        MenuHandler.CreateSlider(nickname, LobbyMenu, 30, 0f, 2f, RoundsVC.GetPlayerOutputVolume(nickname), (val) => { RoundsVC.SetPlayerOutputVolume(nickname, val); }, out Slider slider, false);
                        LobbySliders.Add(slider);
                    }
                }, 
                menu,
                30,
                true,
                true,
                menu.transform.parent.gameObject);

            void ToggleOpenMic(bool val)
            {
                foreach (var toggle in OpenMic)
                {
                    if (toggle.isOn != val)
                    {
                        toggle.isOn = val;
                    }
                }
                if (!val || RoundsVC.MicControl == MicControlType.OpenMic)
                {
                    CheckControlSet();
                    return; 
                }
                RoundsVC.MicControl = MicControlType.OpenMic;
                foreach (var toggle in PushToTalk)
                {
                    toggle.isOn = false;
                }
                foreach (var toggle in PushToToggle)
                {
                    toggle.isOn = false;
                }
            }
            void TogglePushToTalk(bool val)
            {
                foreach (var toggle in PushToTalk)
                {
                    if (toggle.isOn != val)
                    {
                        toggle.isOn = val;
                    }
                }
                if (!val || RoundsVC.MicControl == MicControlType.PushToTalk)
                {
                    CheckControlSet();
                    return; 
                }
                RoundsVC.MicControl = MicControlType.PushToTalk;
                foreach (var toggle in OpenMic)
                {
                    toggle.isOn = false;
                }
                foreach (var toggle in PushToToggle)
                {
                    toggle.isOn = false;
                }
            }
            void TogglePushToToggle(bool val)
            {
                foreach (var toggle in PushToToggle)
                {
                    if (toggle.isOn != val)
                    {
                        toggle.isOn = val;
                    }
                }
                if (!val || RoundsVC.MicControl == MicControlType.PushToToggle)
                {
                    CheckControlSet();
                    return; 
                }
                RoundsVC.MicControl = MicControlType.PushToToggle;
                foreach (var toggle in OpenMic)
                {
                    toggle.isOn = false;
                }
                foreach (var toggle in PushToTalk)
                {
                    toggle.isOn = false;
                }
            }
            void CheckControlSet()
            {
                if (OpenMic.Concat(PushToTalk).Concat(PushToToggle).All(t => !t.isOn))
                {
                    // if no control set, set to default
                    TogglePushToTalk(true);
                }
            }
            
            void ChangeUIOnLeft(bool val)
            {
                UIOnLeft = val;
                foreach (Toggle toggle in UIOnLeftToggles)
                {
                    toggle.isOn = UIOnLeft;
                }
            }

            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);
            MenuHandler.CreateText("Mic Control Style (Default Keybind: [Left Shift])", menu, out TextMeshProUGUI _, 30);
            OpenMic.Add(MenuHandler.CreateToggle(MicControl == MicControlType.OpenMic, "Open", menu, ToggleOpenMic, 30).GetComponent<Toggle>());
            PushToTalk.Add(MenuHandler.CreateToggle(MicControl == MicControlType.PushToTalk, "Push To Talk", menu, TogglePushToTalk, 30).GetComponent<Toggle>());
            PushToToggle.Add(MenuHandler.CreateToggle(MicControl == MicControlType.PushToToggle, "Push To Toggle", menu, TogglePushToToggle, 30).GetComponent<Toggle>());

            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);
            MenuHandler.CreateSlider("UI Scale", menu, 30, 0f, 5f, UIScale, (val) => { UIScale = val; VCUIHandler.UpdateVisuals(); } , out var _, false);
            MenuHandler.CreateSlider("UI Opacity", menu, 30, 0f, 1f, UIOpacity, (val) => { UIOpacity = val; VCUIHandler.UpdateVisuals(); } , out var _, false);
            UIOnLeftToggles.Add(MenuHandler.CreateToggle(UIOnLeft, "UI Position on Left", menu, (val) => { ChangeUIOnLeft(val); VCUIHandler.UpdateVisuals(); }, 30).GetComponent<Toggle>());
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);
            //GameObject lobbyMenu = MenuHandler.CreateMenu("LOBBY VOLUME", () => { ReadLobby(); LobbyMenuUpdater.UpdateSliders(); }, menu, 60, true, true, menu.transform.parent.gameObject);
            //LobbyMenuUpdater = lobbyMenu.GetOrAddComponent<LobbyMenuUpdater>();

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
    /*
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
    */
}
