using Steamworks;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnboundLib;
using System;
using RoundsVC.UI;
namespace RoundsVC
{
    
    public class VoiceChatPeer : MonoBehaviour
    {
        public string NickName { get; internal set; }
        public static int SampleRate => RoundsVC.SampleRate;

        public SortedList<ulong, VoiceChatPacket> PacketQueue = new SortedList<ulong, VoiceChatPacket>();
        private AudioSource m_audioSource;
        private AudioReverbFilter m_audioReverbFilter;
        private AudioLowPassFilter m_audioLowPassFilter;
        private AudioHighPassFilter m_audioHighPassFilter;
        private AudioEchoFilter m_audioEchoFilter;
        private AudioDistortionFilter m_audioDistortionFilter;
        // how many packets we should collect before starting playback
        static public int PacketBuffer = 2;
        // whether or not we're currently waiting for more packets to be collected.
        public bool Buffering = true;

        // the current position of the playhead in the AudioClip
        private int m_streamPosition = 0;
        // the packet that is currently being played
        private VoiceChatPacket m_currentlyPlayingPacket;
        // our position in the packet that's being played.
        private int m_currentlyPlayingPacketSampleIndex = 0;
        // the last/current packet that was played.  if we get packets older than this, we can throw them out.
        private ulong m_lastPlayedPacketId = 0;
        
        void Start()
        {
            m_audioSource = this.gameObject.GetOrAddComponent<AudioSource>();
            m_audioSource.loop = true;
            m_audioSource.clip = AudioClip.Create("VoiceChat", SampleRate * 10, 1, SampleRate, true, OnAudioRead, OnAudioSetPosition);
            m_audioSource.Play();
            m_audioDistortionFilter = this.gameObject.GetOrAddComponent<AudioDistortionFilter>();
            m_audioReverbFilter = this.gameObject.GetOrAddComponent<AudioReverbFilter>();
            m_audioLowPassFilter = this.gameObject.GetOrAddComponent<AudioLowPassFilter>();
            m_audioHighPassFilter = this.gameObject.GetOrAddComponent<AudioHighPassFilter>();
            m_audioEchoFilter = this.gameObject.GetOrAddComponent<AudioEchoFilter>();
            this.DisableAllEffects();
            
        }
        internal void SetSpatial(bool spatial)
        {
            this.gameObject.GetOrAddComponent<AudioSource>().spatialize = spatial;
        }
        void DisableAllEffects()
        {
            this.ApplyFilters(AudioFilters.None);
        }
        void ApplyFilters(AudioFilters effects)
        {
            m_audioDistortionFilter.distortionLevel = effects.DistortionLevel;
            m_audioReverbFilter.reverbPreset = effects.Reverb;
            m_audioLowPassFilter.cutoffFrequency = effects.LowPassCutoff;
            m_audioHighPassFilter.cutoffFrequency = effects.HighPassCutoff;
            m_audioEchoFilter.dryMix = effects.EchoDryMix;
            m_audioEchoFilter.wetMix = effects.EchoWetMix;
            m_audioEchoFilter.delay = effects.EchoDelay;
            m_audioEchoFilter.decayRatio = effects.EchoDecay;
        }
        void ApplySpatial(float spatialBlend, SpatialEffects effects)
        {
            m_audioSource.spatialize = effects.Spatialize;
            m_audioSource.spatialBlend = spatialBlend;
            m_audioSource.rolloffMode = effects.RolloffMode;
            m_audioSource.minDistance = effects.MinDistance;
            m_audioSource.maxDistance = effects.MaxDistance;
            m_audioSource.dopplerLevel = effects.DopplerLevel;
            m_audioSource.spatializePostEffects = effects.SpatializePostEffects;
        }

        void Update()
        {
            // if we're buffering, we're not anymore if we've gotten enough packets.
            if (Buffering)
            {
                Buffering = PacketQueue.Count < PacketBuffer;
            }
        }

        void OnAudioRead(float[] data)
        {
            // wait til we have some packets saved up.
            if (Buffering)
            {
                // write out silence and gtfo
                int count = 0;
                while (count < data.Length)
                {
                    data[count] = 0;
                    m_streamPosition++;
                    count++;
                }
            }
            // we've got enough packets, start writing them to the buffer
            else
            {

                // if we dont' have a packet to play, try grabbing the next one.
                if (m_currentlyPlayingPacket == null)
                {
                    GrabNextPacket();
                }

                int count = 0;
                while (count < data.Length)
                {
                    // start at silence, and fill it in with the correct value.
                    float sample = 0;

                    if (m_currentlyPlayingPacket != null)
                    {
                        sample = m_currentlyPlayingPacket.DecodedData[m_currentlyPlayingPacketSampleIndex];

                        // increment our current packet's playhead now that we've just read a sample
                        m_currentlyPlayingPacketSampleIndex++;

                        // mark down the last packet that was played so that we have an idea of which incoming packets are obsolete.
                        m_lastPlayedPacketId = m_currentlyPlayingPacket.PacketID;

                        // if we've reached the end of this packet, grab the next one.
                        if (m_currentlyPlayingPacketSampleIndex >= m_currentlyPlayingPacket.DecodedData.Length)
                        {
                            GrabNextPacket();
                        }
                    }

                    // write the sample to the AudioClip & update it's position
                    data[count] = sample;
                    m_streamPosition++;
                    count++;
                }
            }
        }

        void OnAudioSetPosition(int newPosition)
        {
            m_streamPosition = newPosition;
        }

        private void GrabNextPacket()
        {
            if (PacketQueue.Count > 0)
            {
                var pair = PacketQueue.First();
                VoiceChatPacket packet = pair.Value;
                if (packet != null)
                {
                    m_currentlyPlayingPacket = packet;
                    PacketQueue.Remove(m_currentlyPlayingPacket.PacketID);
                }
                // update the volume
                m_audioSource.volume = Optionshandler.vol_Master * RoundsVC.GetPlayerOutputVolume(NickName) * RoundsVC.GlobalOutputVolume * m_currentlyPlayingPacket.RelativeVolume;
                // update the audio effects
                this.ApplyFilters(VoiceChat.VoiceChannels[m_currentlyPlayingPacket.ChannelID].AudioFilters);
                this.ApplySpatial(m_currentlyPlayingPacket.SpatialBlend, VoiceChat.VoiceChannels[m_currentlyPlayingPacket.ChannelID].SpatialEffects);
                // update UI
                VCUIHandler.PlayerTalking(m_currentlyPlayingPacket.SpeakerActorID, m_currentlyPlayingPacket.ChannelID);

            }
            else
            {
                m_currentlyPlayingPacket = null;
                Buffering = true;
            }

            // reset the index.
            m_currentlyPlayingPacketSampleIndex = 0;
        }

        public void OnNewSample(VoiceChatPacket newPacket)
        {
            // throw out duplicates. this should never happen...
            if (PacketQueue.ContainsKey(newPacket.PacketID))
            {
                Debug.LogError("already have packet " + newPacket.PacketID + ". aborting");
                return;
            }

            // throw out old packets
            if (m_lastPlayedPacketId > newPacket.PacketID)
            {
                Debug.Log("throwing out old packet " + newPacket.PacketID);
                return;
            }

            // ignore silence
            /*
            if (newPacket.IsSilence)
            {
                return;
            }*/

            // convert immediately.
            newPacket.Decode();

            // shove it into our queue.
            PacketQueue.Add(newPacket.PacketID, newPacket);
        }
    }
}
