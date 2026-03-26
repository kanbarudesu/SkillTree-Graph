using DG.Tweening;
using UnityEngine;

namespace UITweener
{
    [System.Serializable]
    public class PunchRotateTweenAnimation : UITweenAnimation
    {
        public Vector3 Punch = new Vector3(0, 0, 15f);
        public bool randomizePunchDirection;

        [Range(1, 20)] public int Vibrato = 10;
        [Range(0f, 1f)] public float Elasticity = 1f;

        public override Tween Play(RectTransform target)
        {
            if (!CanPlay()) return null;

            if (randomizePunchDirection)
                Punch *= Random.value < 0.5f ? -1f : 1f;

            Tween = target.DOPunchRotation(Punch, Duration, Vibrato, Elasticity)
                .SetEase(Ease)
                .SetDelay(Delay);

            return Tween;
        }
    }
}
