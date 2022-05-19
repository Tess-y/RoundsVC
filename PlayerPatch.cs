using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoundsVC
{
    [Serializable]
    [HarmonyPatch(typeof(Player), "Awake")]
    internal class PlayerPatch
    {
        public static void Prefix(Player __instance)
        {
            __instance.gameObject.AddComponent<VoiceChat>();
        }
    }
}
