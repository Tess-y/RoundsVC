﻿using UnityEngine;
using UnboundLib.Utils;
using UnityEngine.UI;
using TMPro;
using RoundsVC.Extensions;
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
                this.Box.color = this.BoxStartColor.WithOpacity(this.FadePerc * this.BoxStartColor.a);
                this.Text.color = this.TextStartColor.WithOpacity(this.FadePerc * this.TextStartColor.a);
                if (this.Timer > StartDelay + FadeTime)
                {
                    this.gameObject.SetActive(false);
                }
            }
        }
    }
}
