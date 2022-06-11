using UnityEngine;
using UnboundLib.Utils;
using UnityEngine.UI;
using TMPro;
using RoundsVC.Extensions;
namespace RoundsVC.UI
{
    public class PlayerVCIconFade : MonoBehaviour
    {
        public const float StartDelay = 1f;
        public const float FadeTime = 1f;
        private TimeSince Timer;
        private Color StartColor;
        private SpriteRenderer Icon;

        private float FadePerc
        {
            get
            {
                if (this.Timer <= StartDelay) { return 1f; }
                else
                {
                    return UnityEngine.Mathf.Clamp01(1f - (this.Timer - StartDelay) / FadeTime);
                }
            }
        }
        void Start()
        {
            this.Icon = this.GetComponent<SpriteRenderer>();
            this.ResetTimer();
        }
        void OnEnable()
        {
            this.ResetTimer();
        }
        public void ResetTimer()
        {
            this.Timer = 0f;
            if (this.Icon != null) { this.StartColor = this.Icon.color; }
        }
        void Update()
        {
            if (this.Timer > StartDelay)
            {
                this.Icon.color = this.StartColor.WithOpacity(this.FadePerc * this.StartColor.a);
                if (this.Timer > StartDelay + FadeTime)
                {
                    this.gameObject.SetActive(false);
                }
            }
        }
    }
}
