using System;
using DG.Tweening;
using UnityEngine;

namespace Helpers 
{
    public class RobotAnimationHelper : MonoBehaviour
    {
        // public GameObject objectToPickUp;
        
        Transform _objectToPickUpOldParentTransform;
        
        Vector3 _initalPosition;
        Quaternion _initalRotation;

        private void Awake()
        {
            _initalPosition = transform.position;
            _initalRotation = transform.rotation;
        }

        // public void PickUpEvent()
        // {
        //     _objectToPickUpOldParentTransform = objectToPickUp.transform.parent;
        //     objectToPickUp.transform.parent = transform;
        // }
        //
        // public void PickDownEvent()
        // {
        //     objectToPickUp.transform.parent = _objectToPickUpOldParentTransform;
        // }

        public void ResetTransform()
        {
            transform.DOMove(_initalPosition, 7f);
            transform.DORotateQuaternion(_initalRotation, 7f);
        }
        
        
    }
}