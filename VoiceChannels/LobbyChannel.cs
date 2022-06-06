using UnityEngine;
namespace RoundsVC.VoiceChannels
{
    public class LobbyChannel : IVoiceChannel
    {
        public int ChannelID => 0;
        public int Priority => 0;
        public bool Directional => false;
        public string ChannelName => "Lobby";
        public Color ChannelColor => new Color32(230, 230, 230, 255);
        public VCAudioEffects Effects => VCAudioEffects.None;
        public bool SpeakingEnabled(Player player)
        {
            return (player is null);
        }
        public float RelativeVolume(Player speaking, Player listening)
        {
            return (speaking is null && listening is null) ? 1f : 0f;
        }
    }
}
