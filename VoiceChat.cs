using Photon.Pun;
using Steamworks;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace RoundsVC
{
    public class VoiceChat : NetworkBehaviour
    {
        internal AudioSource audioSource;
        internal PhotonView view;
        internal Player _player;

        public int channel = 0;
        public int[] liseningTo = new int[] { 0 };
        public float falloffStartDistence = int.MaxValue;
        public float maxDistence = int.MaxValue;

        void Start() { SteamUser.StartVoiceRecording(); _player = gameObject.GetComponent<Player>(); view = _player.data.view; audioSource = gameObject.AddComponent<AudioSource>();  }
        void Update()
        {
            if (view.IsMine && Input.GetKeyUp(KeyCode.V))
                SteamUser.StartVoiceRecording();
            else if (view.IsMine && Input.GetKeyDown(KeyCode.V))
                SteamUser.StopVoiceRecording();


            if (view.IsMine)
            {
                uint Compressed;
                EVoiceResult ret = SteamUser.GetAvailableVoice(out Compressed);
                if (ret == EVoiceResult.k_EVoiceResultOK && Compressed > 1024)
                {
                    UnityEngine.Debug.Log(Compressed);
                    byte[] DestBuffer = new byte[1024];
                    uint BytesWritten;
                    ret = SteamUser.GetVoice(true, DestBuffer, 1024, out BytesWritten);
                    if (ret == EVoiceResult.k_EVoiceResultOK && BytesWritten > 0)
                    {
                        Cmd_SendData(DestBuffer, BytesWritten);
                    }
                }
            }
        }

        [Command(channel = 2)]
        void Cmd_SendData(byte[] data, uint size)
        {
            foreach(Player player in PlayerManager.instance.players.Where(p => p.playerID != _player.playerID))
            {
                if(player.gameObject.GetComponent<VoiceChat>().liseningTo.Contains(channel))
                typeof(PhotonNetwork).GetMethod("RPC", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic, null,
                    new Type[] { typeof(PhotonView), typeof(string), typeof(Photon.Realtime.Player), typeof(bool), typeof(object[]) }, null)
                    .Invoke(null, new object[] { view, nameof(Target_PlaySound), player.data.view.Owner, false, new object[] { data, (int)size, player.playerID} });
            }
        }

        [PunRPC]
        void Target_PlaySound(byte[] DestBuffer, int BytesWritten, int id)
        {
            byte[] DestBuffer2 = new byte[22050 * 2];
            uint BytesWritten2;
            EVoiceResult ret = SteamUser.DecompressVoice(DestBuffer, (uint)BytesWritten, DestBuffer2, (uint)DestBuffer2.Length, out BytesWritten2, 22050);
            if (ret == EVoiceResult.k_EVoiceResultOK && BytesWritten2 > 0)
            {
                audioSource.clip = AudioClip.Create(Guid.NewGuid().ToString(), 22050, 1, 22050, false);

                float[] test = new float[22050];
                for (int i = 0; i < test.Length; ++i)
                {
                    test[i] = (short)(DestBuffer2[i * 2] | DestBuffer2[i * 2 + 1] << 8) / 32768.0f;
                }
                audioSource.clip.SetData(test, 0);
                Player target = PlayerManager.instance.players.Find(p => p.playerID == id);
                float distance = Vector2.Distance(transform.position, target.transform.position);
                if (distance <= falloffStartDistence) audioSource.volume = 1;
                else if(distance > maxDistence) audioSource.volume = 0;
                else
                {
                    audioSource.volume = 1-(distance - falloffStartDistence)/(maxDistence - falloffStartDistence);
                }
                audioSource.Play();
            }
        }
    }
}
