using UnityEngine;
namespace RoundsVC.VoiceChannels
{
    public class DefaultChannel : VoiceChannel
    {
        public override int ChannelID => 1;
        public override int Priority => 1;
        public override string ChannelName => "Default";
        public override Color ChannelColor => new Color32(230, 230, 230, 255);
        public override AudioFilters AudioFilters => AudioFilters.None;
        public override bool SpeakingEnabled(Player player)
        {
            return RoundsVC.DefaultChannelEnabled && !(player is null);
        }
        public override float RelativeVolume(Player speaking, Player listening)
        {
            return (speaking is null || listening is null) ? 0f : RoundsVC.DefaultChannelEnabled ? 1f : 0f;
        }
    }
}
