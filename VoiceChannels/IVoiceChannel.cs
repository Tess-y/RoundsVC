using UnityEngine;
namespace RoundsVC.VoiceChannels
{
    public interface IVoiceChannel
    {
        /// the interface for voice channels
       
        int ChannelID { get; } // must be unique
        int Priority { get; } // only the highest priority channel that is enabled for the player will be used when they are speaking
        string ChannelName { get; } // what the user would see
        Color ChannelColor { get; } // channel color
        AudioFilters AudioFilters { get; } // audio filter options
        SpatialEffects SpatialEffects { get; } // spatialized audio options
        bool GlobalUIIconsEnabled { get; } // show channel player speaking icons in the global UI 
        bool LocalUIIconsEnabled { get; } // show channel player speaking icons in the local UI
        bool SpeakingEnabled(Player player); // whether or not the player can speak in this channel right now
        float RelativeVolume(Player speaking, Player listening); // the relative volume (0 to 1) of chat in this channel

    }
}
