using UnityEngine;
namespace RoundsVC.Extensions
{
    internal static class ColorExtensions
    {
        public static Color WithOpacity(this Color color, float opacity)
        {
            return new Color(color.r, color.g, color.b, opacity);
        }
    }
}
