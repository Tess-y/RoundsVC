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

namespace RoundsVC
{
    public class VoiceChat : MonoBehaviour
    {
        /// <summary>
        /// This is the global voice chat manager which handles all voice inputs and outputs
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

        public const int SampleRate = 44100; // must be between 11025 and 48000
        public const int Threshold = 512;

        public static VoiceChat Instance = null;
        public static Photon.Realtime.Player Actor => PhotonNetwork.LocalPlayer;
        private static Dictionary<int, IVoiceChannel> channels = new Dictionary<int, IVoiceChannel>();
        public static ReadOnlyDictionary<int, IVoiceChannel> VoiceChannels => new ReadOnlyDictionary<int, IVoiceChannel>(channels);

        public static void AddChannel(IVoiceChannel voiceChannel)
        {
            // add the channel if its ID doesn't already exist
            if (!channels.ContainsKey(voiceChannel.ChannelID))
            {
                channels.Add(voiceChannel.ChannelID, voiceChannel);
            }
            else
            {
                RoundsVC.LogError(new InvalidOperationException("A channel with that Channel ID already exists"));
            }
        }

        private AudioSource GetAudioSource(bool directional, int actorID)
        {
            Player source = PlayerManager.instance.GetPlayerWithActorID(actorID);
            if (directional && source is null)
            {
                RoundsVC.LogError(new InvalidOperationException("Player with ActorID " + actorID + " does not exist"));
                return null;
            }

            
            if (directional)
            {
                // check if the player has a "VoiceChat" child object, and if not, create one
                Transform voiceChat = source.transform.Find("VoiceChat");
                if (voiceChat is null)
                {
                    voiceChat = new GameObject("VoiceChat", typeof(AudioSource)).transform;
                    voiceChat.parent = source.transform;
                }
                return voiceChat.GetComponent<AudioSource>();
            }
            else
            {
                // check if this object has a "VoiceChat" child object, and if not, create one
                Transform voiceChat = this.transform.Find("VoiceChat");
                if (voiceChat is null)
                {
                    voiceChat = new GameObject("VoiceChat").transform;
                    voiceChat.parent = this.transform;
                }
                
                // check if the voicechat object has a "<actorID>" child object, and if not, create one
                Transform playerChat = voiceChat.Find($"{actorID}");
                if (playerChat is null)
                {
                    playerChat = new GameObject($"{actorID}", typeof(AudioSource)).transform;
                    playerChat.parent = voiceChat;
                }
                return playerChat.GetComponent<AudioSource>();
            }
        }
        
        void Awake()
        {
            Instance = this;
        }

        void Start()
        { 
            SteamUser.StartVoiceRecording();
        }
        void Update()
        {
            if (Actor is null) { return; }

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
            
            EVoiceResult ret = SteamUser.GetAvailableVoice(out uint Compressed);
            if (ret == EVoiceResult.k_EVoiceResultOK && Compressed > Threshold)
            {
                byte[] DestBuffer = new byte[1024];
                ret = SteamUser.GetVoice(true, DestBuffer, 1024, out uint BytesWritten);
                if (ret == EVoiceResult.k_EVoiceResultOK && BytesWritten > 0)
                {
                    SendData(DestBuffer, BytesWritten, (int)speakingChannelID);
                }
            }
        }

        void SendData(byte[] data, uint size, int channelID)
        {
            if (RoundsVC.DEBUG)
            {
                NetworkingManager.RPC(typeof(VoiceChat), nameof(PlayVoice), data, (int)size, Actor.ActorNumber, channelID);
            }
            else
            {
                NetworkingManager.RPC_Others(typeof(VoiceChat), nameof(PlayVoice), data, (int)size, Actor.ActorNumber, channelID);
            }
        }

        [UnboundRPC]
        static void PlayVoice(byte[] DestBuffer, int BytesWritten, int speakerActorID, int channelID)
        {
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

            byte[] DestBuffer2 = new byte[SampleRate * 2];
            EVoiceResult ret = SteamUser.DecompressVoice(DestBuffer, (uint)BytesWritten, DestBuffer2, (uint)DestBuffer2.Length, out uint BytesWritten2, SampleRate);
            if (ret == EVoiceResult.k_EVoiceResultOK && BytesWritten2 > 0)
            {
                AudioSource audioSource = Instance.GetAudioSource(voiceChannel.Directional, speakerActorID);
                audioSource.clip = AudioClip.Create(Guid.NewGuid().ToString(), SampleRate, 1, SampleRate, false);

                float[] test = new float[SampleRate];
                for (int i = 0; i < test.Length; ++i)
                {
                    test[i] = (short)(DestBuffer2[i * 2] | DestBuffer2[i * 2 + 1] << 8) / 32768.0f;
                }
                audioSource.clip.SetData(test, 0);
                audioSource.volume = volume;
                audioSource.Play();
            }
        }
    }
}
