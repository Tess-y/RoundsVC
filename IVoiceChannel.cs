using UnityEngine;
namespace RoundsVC
{
    public interface IVoiceChannel
    {
        /// the interface for voice channels
       
        int ChannelID { get; } // must be unique
        int Priority { get; } // only the highest priority channel that is enabled for the player will be used when they are speaking
        string ChannelName { get; } // what the user would see
        Color ChannelColor { get; } // channel color
        bool Directional { get; } // whether or not the channel is directional (audio is emitted from the player)
        VCAudioEffects Effects { get; }
        bool SpeakingEnabled(Player player); // whether or not the player can speak in this channel right now
        float RelativeVolume(Player speaking, Player listening); // the relative volume (0 to 1) of chat in this channel
    }
}
