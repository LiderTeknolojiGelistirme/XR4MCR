using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Models
{
    public class PortControlPoint : MonoBehaviour
    {
        [SerializeField] private float _defaultDistance = 50f;
        [SerializeField] private float _defaultAngle = 180f;

        Transform _transform;
        public Transform Transform
        {
            get
            {
                if (!_transform)
                    _transform = transform;
                return _transform;
            }
        }
        public Vector3 Position
        {
            get => Transform.position;
            set => Transform.position = value;
        }
        public Vector3 LocalPosition
        {
            get => Transform.localPosition;
            set => Transform.localPosition = value;
        }

        public void SetupDefaultPosition()
        {
            SetPosition(_defaultDistance, _defaultAngle);
        }

        public void SetPosition(float distance, float angle)
        {
            var x = distance * Mathf.Cos(angle * Mathf.Deg2Rad);
            var y = distance * Mathf.Sin(angle * Mathf.Deg2Rad);
            Transform.localPosition = new Vector3(x, y, 0);
        }
    }
}