using System.Globalization;
using DG.Tweening;
using UnityEngine;
using Viroo.Interactions;

namespace VirooLab
{
    public class RandomJumpAction : BroadcastObjectAction
    {
        [SerializeField]
        private float minHeight = 1;

        [SerializeField]
        private float maxHeight = 3;

        public override void Execute(string data)
        {
            float targetHeight = Random.Range(minHeight, maxHeight);

            if (DOTween.IsTweening(transform))
            {
                return;
            }

            base.Execute(targetHeight.ToString(CultureInfo.InvariantCulture));
        }

        protected override void LocalExecuteImplementation(string data)
        {
            float height = float.Parse(data, CultureInfo.InvariantCulture);

            transform.DOLocalJump(transform.localPosition, height, 1, 1);
        }
    }
}
