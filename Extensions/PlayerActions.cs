using System;
using System.Runtime.CompilerServices;
using InControl;

namespace RoundsVC.Extensions
{
    // this extension stores ONLY the data for additional player actions
    // additional actions are assigned in the PlayerActions patches
    public class PlayerActionsAdditionalData
    {
        public PlayerAction PTT; // push to talk or push to toggle
    }
    public static class PlayerActionsExtension
    {
        public static readonly ConditionalWeakTable<PlayerActions, PlayerActionsAdditionalData> data =
            new ConditionalWeakTable<PlayerActions, PlayerActionsAdditionalData>();

        public static PlayerActionsAdditionalData GetAdditionalData(this PlayerActions playerActions)
        {
            return data.GetOrCreateValue(playerActions);
        }

        public static void AddData(this PlayerActions playerActions, PlayerActionsAdditionalData value)
        {
            try
            {
                data.Add(playerActions, value);
            }
            catch (Exception) { }
        }

        public static bool IsController(this PlayerActions playerActions) => playerActions.ActiveDevice.DeviceClass != InputDeviceClass.Keyboard;
        public static bool PTTWasPressed(this PlayerActions playerActions) => playerActions.GetAdditionalData().PTT.WasPressed;
        public static bool PTTIsPressed(this PlayerActions playerActions) => playerActions.GetAdditionalData().PTT.IsPressed;
    }
}
