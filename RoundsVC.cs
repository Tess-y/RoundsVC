using System;
using BepInEx;
using HarmonyLib;
using Steamworks;

namespace RoundsVC
{
    [BepInPlugin(ModId, ModName, Version)]
    [BepInProcess("Rounds.exe")]
    public class RoundsVC : BaseUnityPlugin
    {
        private const string ModId = "root.rounds.vc";
        private const string ModName = "RoundsVC";
        public const string Version = "0.0.0";

        void Awake()
        {
            // Use this to call any harmony patch files your Mod may have
            var harmony = new Harmony(ModId);
            harmony.PatchAll();
        }
    }


}
