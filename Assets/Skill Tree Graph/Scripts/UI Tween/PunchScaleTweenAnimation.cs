using DG.Tweening;
using UnityEngine;

namespace UITweener
{
    [System.Serializable]
    public class PunchScaleTweenAnimation : UITweenAnimation
    {
        public Vector3 MinPunch = new Vector3(0.15f, 0.15f, 0f);
        public Vector3 MaxPunch = new Vector3(0.4f, 0.4f, 0f);

        [Range(1, 20)] public int Vibrato = 8;
        [Range(0f, 1f)] public float Elasticity = 0.8f;

        public override Tween Play(RectTransform target)
        {
            if (!CanPlay()) return null;

            var randomPunch = Vector3.Lerp(MinPunch, MaxPunch, Random.value);
            Tween = target.DOPunchScale(randomPunch, Duration, Vibrato, Elasticity)
                .SetEase(Ease)
                .SetDelay(Delay);

            return Tween;
        }
    }
}
