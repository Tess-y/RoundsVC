namespace RoundsVC
{
    public class VoiceChatPacket
    {
        public ulong PacketID;
        public int Length;
        public byte[] Data;
        public float[] DecodedData = null;
        //public bool IsSilence = false;
        public int SpeakerActorID = -1;
        public int ChannelID = -1;
        public float RelativeVolume = 1f;

        // decodes from steam's uncompressed format to the float format that unity likes
        // note that this might need to change somewhat if i mess around with the frequency.
        public void Decode()
        {
            DecodedData = new float[Length / 2];// optimization todo :: pool this.
            for (int i = 0; i < DecodedData.Length; i++)
            {
                float value = (float)System.BitConverter.ToInt16(Data, i * 2);
                DecodedData[i] = value / (float)short.MaxValue;
            }
        }

        public VoiceChatPacket(ulong PacketID, int Length, byte[] Data, int SpeakerActorID, int ChannelID, float RelativeVolume)
        {
            this.PacketID = PacketID;
            this.Length = Length;
            this.Data = Data;
            this.SpeakerActorID = SpeakerActorID;
            this.ChannelID = ChannelID;
            this.RelativeVolume = RelativeVolume;
        }
    }

}
