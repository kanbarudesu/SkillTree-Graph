using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace UITweener
{
    public enum TweenPlayMode
    {
        Replace,
        Parallel
    }

    [System.Serializable]
    public class UITweenSequence
    {
        [SerializeReference, SRPeeker]
        public List<UITweenAnimation> Animations = new();

        public TweenPlayMode PlayMode = TweenPlayMode.Replace;

        private Sequence sequence;

        public Sequence Play(RectTransform target)
        {
            if (sequence != null && sequence.IsActive() && PlayMode == TweenPlayMode.Replace)
            {
                sequence.Kill();
                sequence = null;
            }

            sequence ??= DOTween.Sequence();

            foreach (var anim in Animations)
            {
                if (anim.Tween == null) continue;

                if (PlayMode == TweenPlayMode.Parallel)
                    sequence.Join(anim.Play(target));
                else
                    sequence.Append(anim.Play(target));
            }

            sequence.Play();
            return sequence;
        }
    }
}
