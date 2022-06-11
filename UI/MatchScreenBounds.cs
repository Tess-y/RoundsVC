using UnityEngine;
using UnityEngine.UI;
namespace RoundsVC.UI
{
    internal class MatchScreenBounds : MonoBehaviour
    {
        CanvasScaler canvasScaler = null;
        RectTransform rectTransform = null;
        void Start()
        {
            this.canvasScaler = this.GetComponentInParent<CanvasScaler>();
            this.rectTransform = this.GetComponent<RectTransform>();
        }
        void Update()
        {
            this.rectTransform.sizeDelta = new Vector2(this.canvasScaler.referenceResolution.x/this.transform.localScale.x, this.canvasScaler.referenceResolution.y/this.transform.localScale.y);
        }
    }
}
