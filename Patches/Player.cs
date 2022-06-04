using HarmonyLib;
using System;
using UnityEngine;
using UnboundLib;

namespace RoundsVC.Patches
{
    [HarmonyPatch(typeof(Player), "Awake")]
    internal class PlayerPatchAwake
    {
        public static void Postfix(Player __instance)
        {
            //__instance.gameObject.GetOrAddComponent<VoiceChatPeer>();
        }
    }
}
