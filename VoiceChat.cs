using Photon.Pun;
using Steamworks;
using System;
using System.Linq;
using UnityEngine;
using Photon.Realtime;
using UnboundLib.Networking;
using UnboundLib;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using RoundsVC.UI;
using RoundsVC.VoiceChannels;
using RoundsVC.Utils;

namespace RoundsVC
{
    public class VoiceChat : MonoBehaviour
    {
        /// <summary>
        /// This is the global voice chat manager which handles all voice inputs and RPCs
        /// It exists on the plugin object itself so that voice chat can work in lobbies
        /// 
        /// Audio sources are located on player gameobjects OR on child gameobjects of
        /// this object, which are used in the case that a player object cannot be found
        /// 
        /// A default channel exists for in-lobby global voice and in-game global voice
        /// all other channels must be made by gamemodes/other mods
        /// 
        /// the default channels can be disabled by external mods
        /// 
        /// </summary>

        public static int SampleRate => RoundsVC.SampleRate;

        public static VoiceChat Instance = null;
        public static Photon.Realtime.Player Actor => PhotonNetwork.LocalPlayer;
        private static Dictionary<int, IVoiceChannel> channels = new Dictionary<int, IVoiceChannel>();
        public static ReadOnlyDictionary<int, IVoiceChannel> VoiceChannels => new ReadOnlyDictionary<int, IVoiceChannel>(channels);

        // instantiated outside of methods for performance
        private static byte[] decompressedBuffer = new byte[SampleRate * 2];

        private ulong _packetID = 0;
        public ulong PacketID
        {
            get
            {
                this._packetID++;
                return this._packetID;
            }
        }

        private VoiceChatPeer GetPeer(bool spatial, int actorID)
        {
            Player source = PlayerManager.instance.GetPlayerWithActorID(actorID);
            if (spatial && source is null)
            {
                RoundsVC.LogError(new InvalidOperationException("Player with ActorID " + actorID + " does not exist"));
                return null;
            }

            
            if (spatial)
            {
                // check if the player has a "VoiceChat" child object, and if not, create one
                Transform voiceChat = source.transform.Find("VoiceChatPeer");
                if (voiceChat is null)
                {
                    voiceChat = new GameObject("VoiceChatPeer", typeof(VoiceChatPeer)).transform;
                    voiceChat.parent = source.transform;
                }
                VoiceChatPeer voiceChatPeer = voiceChat.GetComponent<VoiceChatPeer>();
                voiceChatPeer.SetSpatial(true);
                return voiceChatPeer;
            }
            else
            {
                // check if this object has a "VoiceChat" child object, and if not, create one
                Transform voiceChat = this.transform.Find("VoiceChatPeer");
                if (voiceChat is null)
                {
                    voiceChat = new GameObject("VoiceChatPeer").transform;
                    voiceChat.parent = this.transform;
                }
                
                // check if the voicechat object has a "<actorID>" child object, and if not, create one
                Transform playerChat = voiceChat.Find($"{actorID}");
                if (playerChat is null)
                {
                    playerChat = new GameObject($"{actorID}", typeof(VoiceChatPeer)).transform;
                    playerChat.parent = voiceChat;
                }
                VoiceChatPeer voiceChatPeer = playerChat.GetComponent<VoiceChatPeer>();
                voiceChatPeer.NickName = PhotonNetwork.CurrentRoom?.GetPlayer(actorID)?.NickName ?? "";
                voiceChatPeer.SetSpatial(false);
                return voiceChatPeer;
            }
        }

        public static void AddChannel(IVoiceChannel voiceChannel)
        {
            // add the channel if its ID doesn't already exist
            if (!channels.ContainsKey(voiceChannel.ChannelID))
            {
                channels.Add(voiceChannel.ChannelID, voiceChannel);
            }
            else
            {
                RoundsVC.LogError(new InvalidOperationException($"A Voice Channel already exists with channel ID {voiceChannel.ChannelID}: '{channels[voiceChannel.ChannelID].ChannelName}'"));
            }
        }

        private bool SteamworksAvailable = false;
                
        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            this.SteamworksAvailable = false;
            try
            {
                SteamUser.StartVoiceRecording();
                this.SteamworksAvailable = true;
            }
            catch
            {
                RoundsVC.LogError(new InvalidOperationException("SteamWorks unavailable. Voice chat will not work."));
            }
        }
        void Update()
        {
            if (!this.SteamworksAvailable || Actor is null || !VoiceControls.MicOn) { return; }

            int? speakingChannelID = null;
            try
            {
                speakingChannelID = channels.OrderByDescending(kv => kv.Value.Priority).Where(kv => kv.Value.SpeakingEnabled(PlayerManager.instance.GetLocalPlayer())).Select(kv => kv.Value.ChannelID).First();
            }
            catch
            {
                return;
            }
            if (speakingChannelID is null) { return; }
            
            EVoiceResult ret = SteamUser.GetAvailableVoice(out uint compressedBytes);
            if (ret == EVoiceResult.k_EVoiceResultOK && compressedBytes > 0)
            {
                byte[] compressedBuffer = new byte[compressedBytes];
                ret = SteamUser.GetVoice(true, compressedBuffer, compressedBytes, out uint compressedBytesWritten);
                if (ret == EVoiceResult.k_EVoiceResultOK && compressedBytesWritten > 0)
                {
                    SendData(compressedBuffer, compressedBytesWritten, (int)speakingChannelID);
                }
            }
        }

        void SendData(byte[] data, uint size, int channelID)
        {
            if (RoundsVC.DEBUG)
            {
                NetworkingManager.RPC_Unreliable(typeof(VoiceChat), nameof(PlayVoice), data, (int)size, this.PacketID.ToString(), Actor.ActorNumber, channelID);
            }
            else
            {
                VCUIHandler.PlayerTalking(Actor.ActorNumber, channelID);
                NetworkingManager.RPC_Others_Unreliable(typeof(VoiceChat), nameof(PlayVoice), data, (int)size, this.PacketID.ToString(), Actor.ActorNumber, channelID);
            }
        }

        [UnboundRPC]
        static void PlayVoice(byte[] compressedBuffer, int bytesSent, string packetID_as_string, int speakerActorID, int channelID)
        {
            if (!RoundsVC.DEBUG && speakerActorID == Actor.ActorNumber)
            {
                return;
            }
            else if (!VoiceChat.Instance.SteamworksAvailable)
            {
                VCUIHandler.PlayerTalking(speakerActorID, channelID);
                return;
            }
            ulong packetID = ulong.Parse(packetID_as_string);
            Player speaking = PlayerManager.instance.GetPlayerWithActorID(speakerActorID);
            Player listening = PlayerManager.instance.GetLocalPlayer();
            if (!channels.TryGetValue(channelID, out IVoiceChannel voiceChannel))
            {
                return;
            }
            float volume = voiceChannel.RelativeVolume(speaking, listening);
            if (volume <= 0f)
            {
                return;
            }
            float spatialBlend = voiceChannel.SpatialEffects.SpatialBlend(speaking, listening);

            EVoiceResult ret = SteamUser.DecompressVoice(compressedBuffer, (uint)bytesSent, decompressedBuffer, (uint)decompressedBuffer.Length, out uint bytesDecompressed, (uint)SampleRate);
            if (ret == EVoiceResult.k_EVoiceResultOK && bytesDecompressed > 0)
            {

                var voiceChatPacket = new VoiceChatPacket(packetID, (int)bytesDecompressed, decompressedBuffer, speakerActorID, channelID, volume, spatialBlend);
                VoiceChat.Instance.GetPeer(voiceChannel.SpatialEffects.Spatialize, speakerActorID).OnNewSample(voiceChatPacket);
            }
        }
    }
}
