using UnityEngine;
namespace RoundsVC.VoiceChannels
{
    public class LobbyChannel : VoiceChannel
    {
        public override int ChannelID => 0;
        public override int Priority => 0;
        public override string ChannelName => "Lobby";
        public override Color ChannelColor => new Color32(230, 230, 230, 255);
        public override AudioFilters AudioFilters => AudioFilters.None;
        public override bool SpeakingEnabled(Player player)
        {
            return (player is null);
        }
        public override float RelativeVolume(Player speaking, Player listening)
        {
            return (speaking is null && listening is null) ? 1f : 0f;
        }
    }
}
