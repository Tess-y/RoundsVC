using UnityEngine;
namespace RoundsVC.VoiceChannels
{
    public class DefaultChannel : IVoiceChannel
    {
        public int ChannelID => 1;
        public int Priority => 1;
        public bool Directional => false;
        public string ChannelName => "Default";
        public Color ChannelColor => new Color32(230, 230, 230, 255);
        public VCAudioEffects Effects => VCAudioEffects.None;
        public bool SpeakingEnabled(Player player)
        {
            return !(player is null);
        }
        public float RelativeVolume(Player speaking, Player listening)
        {
            return (speaking is null || listening is null) ? 0f : 1f;
        }
    }
}
