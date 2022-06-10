using HarmonyLib;
using UnityEngine;
namespace RoundsVC.Patches
{
    [HarmonyPatch(typeof(Player), "Start")]
    static class PlayerPatchStart
    {
        static void Postfix(Player __instance)
        {
            if (__instance.data.view.IsMine)
            {
                MainCam.instance.cam.transform.Find("AudioListener")?.SetParent(__instance.transform);
            }
        }
    }
}
