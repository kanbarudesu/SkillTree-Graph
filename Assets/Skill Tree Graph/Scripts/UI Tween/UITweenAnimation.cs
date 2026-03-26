using DG.Tweening;
using UnityEngine;

namespace UITweener
{
    [System.Serializable]
    public abstract class UITweenAnimation
    {
        public float Duration = 0.2f;
        public Ease Ease = Ease.OutQuad;
        public float Delay = 0f;
        public Tween Tween { get; protected set; }

        public abstract Tween Play(RectTransform target);
        public virtual bool CanPlay()
        {
            return Tween == null || !Tween.IsActive();
        }
    }
}
