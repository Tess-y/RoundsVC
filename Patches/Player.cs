using HarmonyLib;
using UnityEngine;
using RoundsVC.UI;
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

            // try to make VC icon ahead of time so that things like LocalZoom can modify it
            Transform wobbleObjects = __instance.transform.Find("WobbleObjects");
            if (wobbleObjects is null) { return; }
            Transform playerVCIcon = wobbleObjects.Find("PlayerVCIcon");
            if (playerVCIcon is null)
            {
                playerVCIcon = GameObject.Instantiate(VCUIHandler.vcPlayerVCIconPrefab, wobbleObjects).transform;
                playerVCIcon.localPosition = new Vector3(1.25f, 1.5f, 0f);
                playerVCIcon.localScale = new Vector3(0.05f, 0.05f, 1f);
                playerVCIcon.gameObject.name = "PlayerVCIcon";
            }
            playerVCIcon.gameObject.SetActive(false);
        }
    }
}
