using UnityEngine;
using RoundsVC.Extensions;
namespace RoundsVC.Utils
{
    internal class VoiceControls : MonoBehaviour
    {
        public static bool MicOn { get; private set; } = false;
        void Update()
        {
            if (Photon.Pun.PhotonNetwork.OfflineMode || Photon.Pun.PhotonNetwork.CurrentRoom is null)
            {
                MicOn = false;
                return;
            }
            Player player = PlayerManager.instance.GetLocalPlayer();
            if (player is null)
            {
                // use lobby actions
                if (MenuControllerHandler.menuControl == MenuControllerHandler.MenuControl.Controller)
                {
                    // controllers are always open mic
                    MicOn = true;
                    return;
                }
                else
                {
                    switch (RoundsVC.MicControl)
                    {
                        case RoundsVC.MicControlType.OpenMic:
                            MicOn = true;
                            return;
                        case RoundsVC.MicControlType.PushToTalk:
                            MicOn = RoundsVC.LobbyActions.PTT.IsPressed;
                            return;
                        case RoundsVC.MicControlType.PushToToggle:
                            if (RoundsVC.LobbyActions.PTT.WasPressed)
                            {
                                MicOn = !MicOn;
                            }
                            return;
                        default:
                            MicOn = false;
                            return;
                    }
                }
            }
            else
            {
                // use player actions
                if (player.data.playerActions.ActiveDevice.DeviceClass == InControl.InputDeviceClass.Controller)
                {
                    // controllers are always open mic
                    MicOn = true;
                    return;
                }
                else
                {
                    switch (RoundsVC.MicControl)
                    {
                        case RoundsVC.MicControlType.OpenMic:
                            MicOn = true;
                            return;
                        case RoundsVC.MicControlType.PushToTalk:
                            MicOn = player.data.playerActions.PTTIsPressed();
                            return;
                        case RoundsVC.MicControlType.PushToToggle:
                            if (player.data.playerActions.PTTWasPressed())
                            {
                                MicOn = !MicOn;
                            }
                            return;
                        default:
                            MicOn = false;
                            return;
                    }
                }
            }
        }
    }
}
