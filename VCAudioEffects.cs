using UnityEngine;
namespace RoundsVC
{
    public class VCAudioEffects
    {
        public static readonly VCAudioEffects None = new VCAudioEffects();

        public readonly AudioReverbPreset Reverb = AudioReverbPreset.Off;
        public readonly int LowPassCutoff = int.MaxValue;
        public readonly int HighPassCutoff = int.MinValue;
        public readonly float EchoDecay = 0f;
        public readonly float EchoDelay = 0f;
        public readonly float EchoDryMix = 1f;
        public readonly float EchoWetMix = 0f;
        public readonly float DistortionLevel = 0f;
        
        public VCAudioEffects()
        {
            this.Reverb = AudioReverbPreset.Off;
            this.LowPassCutoff = int.MaxValue;
            this.HighPassCutoff = int.MinValue;
            this.EchoDecay = 0f;
            this.EchoDelay = 0f;
            this.EchoDryMix = 1f;
            this.EchoWetMix = 0f;
            this.DistortionLevel = 0f;
        }
        public VCAudioEffects(AudioReverbPreset reverb)
        {
            this.Reverb = reverb;
            this.LowPassCutoff = int.MaxValue;
            this.HighPassCutoff = int.MinValue;
            this.EchoDecay = 0f;
            this.EchoDelay = 0f;
            this.EchoDryMix = 1f;
            this.EchoWetMix = 0f;
            this.DistortionLevel = 0f;
        }
        public VCAudioEffects(float distortion)
        {
            this.Reverb = AudioReverbPreset.Off;
            this.LowPassCutoff = int.MaxValue;
            this.HighPassCutoff = int.MinValue;
            this.EchoDecay = 0f;
            this.EchoDelay = 0f;
            this.EchoDryMix = 1f;
            this.EchoWetMix = 0f;
            this.DistortionLevel = distortion;
        }
        public VCAudioEffects(int lowPassCutoff, int highPassCutoff)
        {
            this.Reverb = AudioReverbPreset.Off;
            this.LowPassCutoff = lowPassCutoff;
            this.HighPassCutoff = highPassCutoff;
            this.EchoDecay = 0f;
            this.EchoDelay = 0f;
            this.EchoDryMix = 1f;
            this.EchoWetMix = 0f;
            this.DistortionLevel = 0f;
        }
        public VCAudioEffects(float echoDecay, float echoDelay, float echoDryMix, float echoWetMix)
        {
            this.Reverb = AudioReverbPreset.Off;
            this.LowPassCutoff = int.MaxValue;
            this.HighPassCutoff = int.MinValue;
            this.EchoDecay = echoDecay;
            this.EchoDelay = echoDelay;
            this.EchoDryMix = echoDryMix;
            this.EchoWetMix = echoWetMix;
            this.DistortionLevel = 0f;
        }
        public VCAudioEffects(AudioReverbPreset reverb = AudioReverbPreset.Off, int lowPassCutoff = int.MaxValue, int highPassCutoff = int.MinValue, float echoDecay = 0f, float echoDelay = 0f, float echoDryMix = 1f, float echoWetMix = 0f, float distortion = 0f)
        {
            this.Reverb = reverb;
            this.LowPassCutoff = lowPassCutoff;
            this.HighPassCutoff = highPassCutoff;
            this.EchoDecay = echoDecay;
            this.EchoDelay = echoDelay;
            this.EchoDryMix = echoDryMix;
            this.EchoWetMix = echoWetMix;
            this.DistortionLevel = distortion;
        }
    }
}
