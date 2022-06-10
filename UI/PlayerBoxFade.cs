using UnityEngine;
using UnboundLib.Utils;
using UnityEngine.UI;
using TMPro;
namespace RoundsVC.UI
{
    public class PlayerBoxFade : MonoBehaviour
    {
        public const float StartDelay = 1f;
        public const float FadeTime = 1f;
        private TimeSince Timer;
        private Color BoxStartColor;
        private Color TextStartColor;
        private Image Box;
        private TextMeshProUGUI Text;

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
            this.Box = this.GetComponent<Image>();
            this.Text = this.GetComponentInChildren<TextMeshProUGUI>();
            this.ResetTimer();
        }
        void OnEnable()
        {
            this.ResetTimer();
        }
        public void ResetTimer()
        {
            this.Timer = 0f;
            if (this.Box != null) { this.BoxStartColor = this.Box.color; }
            if (this.Text != null) { this.TextStartColor = this.Text.color; }
        }
        void Update()
        {
            if (this.Timer > StartDelay)
            {
                this.Box.color = new Color(this.BoxStartColor.r, this.BoxStartColor.g, this.BoxStartColor.b, this.FadePerc * this.BoxStartColor.a);
                this.Text.color = new Color(this.TextStartColor.r, this.TextStartColor.g, this.TextStartColor.b, this.FadePerc * this.TextStartColor.a);
                if (this.Timer > StartDelay + FadeTime)
                {
                    this.gameObject.SetActive(false);
                }
            }
        }
    }
}
