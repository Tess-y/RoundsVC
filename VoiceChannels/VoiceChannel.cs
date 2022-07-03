using UnityEngine;

namespace RoundsVC.VoiceChannels
{
    public abstract class VoiceChannel : IVoiceChannel
    {
        public abstract int ChannelID { get; }
        public abstract int Priority { get; }
        public abstract string ChannelName { get; }
        public abstract Color ChannelColor { get; }
        public virtual AudioFilters AudioFilters { get; } = AudioFilters.None;
        public virtual SpatialEffects SpatialEffects { get; } = SpatialEffects.None;
        public virtual bool GlobalUIIconsEnabled { get; } = true;
        public virtual bool LocalUIIconsEnabled { get; } = true;

        public abstract float RelativeVolume(Player speaking, Player listening);

        public abstract bool SpeakingEnabled(Player player);
    }
}
