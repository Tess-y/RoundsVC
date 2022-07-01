using System;
using InControl;
namespace RoundsVC.Utils
{

    public class LobbyPlayerActions : PlayerActionSet
    {

        public PlayerAction PTT;
        public LobbyPlayerActions() : base()
        {
            this.PTT = base.CreatePlayerAction("PTT");
        }

        public static LobbyPlayerActions CreateWithKeyboardBindings()
        {
            LobbyPlayerActions playerActions = new LobbyPlayerActions();

            playerActions.PTT.AddDefaultBinding(Key.LeftShift);

            playerActions.ListenOptions.IncludeUnknownControllers = true;
            playerActions.ListenOptions.MaxAllowedBindings = 4U;
            playerActions.ListenOptions.UnsetDuplicateBindingsOnSet = true;
            playerActions.ListenOptions.OnBindingFound = delegate (PlayerAction action, BindingSource binding)
            {
                if (binding == new KeyBindingSource(new Key[]
                {
                    Key.Escape
                }))
                {
                    action.StopListeningForBinding();
                    return false;
                }
                return true;
            };
            BindingListenOptions listenOptions = playerActions.ListenOptions;
            listenOptions.OnBindingAdded = (Action<PlayerAction, BindingSource>)Delegate.Combine(listenOptions.OnBindingAdded, new Action<PlayerAction, BindingSource>(delegate (PlayerAction action, BindingSource binding)
            {
                Debug.Log("Binding added... " + binding.DeviceName + ": " + binding.Name);
            }));
            BindingListenOptions listenOptions2 = playerActions.ListenOptions;
            listenOptions2.OnBindingRejected = (Action<PlayerAction, BindingSource, BindingSourceRejectionType>)Delegate.Combine(listenOptions2.OnBindingRejected, new Action<PlayerAction, BindingSource, BindingSourceRejectionType>(delegate (PlayerAction action, BindingSource binding, BindingSourceRejectionType reason)
            {
                Debug.Log("Binding rejected... " + reason);
            }));
            return playerActions;
        }

        public static LobbyPlayerActions CreateWithControllerBindings()
        {
            LobbyPlayerActions playerActions = new LobbyPlayerActions();

            // no PTT binding for controller players

            playerActions.ListenOptions.IncludeUnknownControllers = true;
            playerActions.ListenOptions.MaxAllowedBindings = 4U;
            playerActions.ListenOptions.UnsetDuplicateBindingsOnSet = true;
            playerActions.ListenOptions.OnBindingFound = delegate (PlayerAction action, BindingSource binding)
            {
                if (binding == new KeyBindingSource(new Key[]
                {
                    Key.Escape
                }))
                {
                    action.StopListeningForBinding();
                    return false;
                }
                return true;
            };
            BindingListenOptions listenOptions = playerActions.ListenOptions;
            listenOptions.OnBindingAdded = (Action<PlayerAction, BindingSource>)Delegate.Combine(listenOptions.OnBindingAdded, new Action<PlayerAction, BindingSource>(delegate (PlayerAction action, BindingSource binding)
            {
                Debug.Log("Binding added... " + binding.DeviceName + ": " + binding.Name);
            }));
            BindingListenOptions listenOptions2 = playerActions.ListenOptions;
            listenOptions2.OnBindingRejected = (Action<PlayerAction, BindingSource, BindingSourceRejectionType>)Delegate.Combine(listenOptions2.OnBindingRejected, new Action<PlayerAction, BindingSource, BindingSourceRejectionType>(delegate (PlayerAction action, BindingSource binding, BindingSourceRejectionType reason)
            {
                Debug.Log("Binding rejected... " + reason);
            }));
            return playerActions;
        }
    }
}
