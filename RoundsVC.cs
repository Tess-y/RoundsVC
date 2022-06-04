using System;
using BepInEx;
using HarmonyLib;
using Steamworks;
using UnboundLib;

namespace RoundsVC
{
    [BepInPlugin(ModId, ModName, Version)]
    [BepInProcess("Rounds.exe")]
    public class RoundsVC : BaseUnityPlugin
    {
        private const string ModId = "root.rounds.vc";
        private const string ModName = "RoundsVC";
        public const string Version = "0.0.0";

#if DEBUG
        public const bool DEBUG = true;
#else
        public const bool DEBUG = false;
#endif

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
        }
    }


}
