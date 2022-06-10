using System;
using UnityEngine;
namespace RoundsVC
{
    public class SpatialEffects
    {
        public static readonly SpatialEffects None = new SpatialEffects(false, AudioRolloffMode.Logarithmic, false, float.MaxValue, float.MaxValue, 0f, (p1,p2) => 0f);

        public readonly bool Spatialize;
        public readonly AudioRolloffMode RolloffMode;
        public readonly bool SpatializePostEffects;
        public readonly float MaxDistance;
        public readonly float MinDistance;
        public readonly float DopplerLevel;
        public readonly Func<Player, Player, float> SpatialBlend;

        public static Func<Player, Player, float> LogarithmicBlend(float minDistance)
        {
            return (p1, p2) =>
            {
                float dist = Vector2.Distance(p1.data.playerVel.position, p2.data.playerVel.position);
                if (dist < minDistance) { return 0f; }
                return UnityEngine.Mathf.Clamp01(1f - minDistance / dist);
            };
        }
        public static Func<Player, Player, float> LinearBlend(float minDistance, float maxDistance)
        {
            return (p1, p2) =>
            {
                float dist = Vector2.Distance(p1.data.playerVel.position, p2.data.playerVel.position);
                if (dist < minDistance) { return 0f; }
                if (dist > maxDistance) { return 1f; }
                return UnityEngine.Mathf.Clamp01((dist - minDistance) / (maxDistance - minDistance));
            };
        }

        public SpatialEffects(bool spatialize, AudioRolloffMode rolloffMode, bool spatializePostEffects, float minDistance, float maxDistance, float dopplerLevel, Func<Player, Player, float> spatialBlend)
        {
            if ((minDistance < float.MaxValue || maxDistance < float.MaxValue) && minDistance >= maxDistance)
            {
                RoundsVC.LogError($"VoiceChannel::SpatialEffects minDistance ({minDistance}) must be less than maxDistance ({maxDistance})");
            }
            Spatialize = spatialize;
            RolloffMode = rolloffMode;
            SpatializePostEffects = spatializePostEffects;
            MaxDistance = maxDistance;
            MinDistance = minDistance;
            DopplerLevel = dopplerLevel;
            SpatialBlend = spatialBlend;
        }

    }
}
