using System;
using DG.Tweening;
using UnityEngine;

namespace VirooLab
{
    public class Target : MonoBehaviour
    {
        public event EventHandler OnHit;

        [SerializeField]
        private float maxHeight = default;

        private Vector3 startPosition;
        private Quaternion startRotation;

        protected void Awake()
        {
            startPosition = transform.position;
            startRotation = transform.rotation;
        }

        public void AnimateShow()
        {
            transform.DOLocalMoveY(maxHeight, 1).OnComplete(() => transform.DOLocalRotate(new Vector3(0, 90, 90), 1));
        }

        public void AnimateHide(Action callback)
        {
            Sequence hideSequence = DOTween.Sequence();

            hideSequence.Append(transform.DORotate(new Vector3(0, 360 * 4, 90), 2, RotateMode.FastBeyond360));
            hideSequence.Append(transform.DOLocalRotate(startRotation.eulerAngles, 0.5f));
            hideSequence.Append(transform.DOMoveY(startPosition.y, 0.5f)).OnComplete(() => callback?.Invoke());

            hideSequence.Play();
        }

        protected void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out Bullet bullet))
            {
                OnHit?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
