using DG.Tweening;
using UnityEngine;

namespace UITweener
{
    public class PunchMoveTweenAnimation : UITweenAnimation
    {
        public Vector3 Punch = new Vector3(15f, 0f, 0f);
        public bool randomizePunchDirection;

        [Range(1, 20)] public int Vibrato = 8;
        [Range(0f, 1f)] public float Elasticity = 0.8f;

        public override Tween Play(RectTransform target)
        {
            if (!CanPlay()) return null;

            if (randomizePunchDirection)
            {
                Punch.x *= Random.value < 0.5f ? -1f : 1f;
                Punch.y *= Random.value < 0.5f ? -1f : 1f;
                Punch.z *= Random.value < 0.5f ? -1f : 1f;
            }

            Tween = target.DOPunchPosition(Punch, Duration, Vibrato, Elasticity)
                .SetEase(Ease)
                .SetDelay(Delay);

            return Tween;
        }
    }
}