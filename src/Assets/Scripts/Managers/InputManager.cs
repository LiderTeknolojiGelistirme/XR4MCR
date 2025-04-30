using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Managers
{
    public abstract class InputManager : MonoBehaviour
    {
        public abstract Vector3 ScreenPointerPosition { get; }
        public abstract Vector3 GetCanvasPointerPosition(GraphManager graphManager);
        public abstract bool PointerPress { get; }
        public abstract bool Aux0KeyPress { get; }
        public abstract void OnUpdate();
        public abstract UnityEvent e_OnPointerDown { get; set; }
        public abstract UnityEvent<Vector3> e_OnDrag { get; set; }
        public abstract UnityEvent e_OnPointerUp { get; set; }
        public abstract UnityEvent e_OnDelete { get; set; }
        public abstract UnityEvent e_OnPointerHover { get; set; }

        
    }
}