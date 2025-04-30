using System;
using RuntimeGizmos;
using UnityEngine;
using Viroo.Interactions;
using Zenject;

namespace Actions
{
    public class TriggerPressedAction : BroadcastObjectAction
    {
        private bool _isFound = false;
        private TransformGizmo _transformGizmo;
        private void FixedUpdate()
        {
            if (!_isFound)
            {
                _transformGizmo = FindObjectOfType<TransformGizmo>();
                if (_transformGizmo != null)
                {
                    _isFound = true;
                }
            }
        }

        protected override void LocalExecuteImplementation(string data)
        {
            // _transformGizmo henüz bulunmamış olabilir
            if (_transformGizmo == null)
            {
                _transformGizmo = FindObjectOfType<TransformGizmo>();
                if (_transformGizmo == null)
                {
                    Debug.Log("TriggerPressedAction: TransformGizmo bulunamadı!");
                    return;
                }
                _isFound = true;
            }
            
            _transformGizmo.isTriggerPressed = true;
            _transformGizmo.isTriggerReleased = false;
        }
    }
}
