using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR;
using Managers;
using UnityEngine.UI;


    public class CustomScrollRect : ScrollRect
    {
        int maximumTouchCount = 2;

        private XRInputManager _xrInputManager;

        // Get the touch position from XR controller
        public Vector2 MultiTouchPosition
        {
            get
            {
                Vector2 position = Vector2.zero;
                // Get pointer position from the XRInputManager
                position = _xrInputManager.ScreenPointerPosition;
                return position;
            }
        }

        // Inject XRInputManager
        public void Construct(XRInputManager xrInputManager)
        {
            _xrInputManager = xrInputManager;
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            if (_xrInputManager != null)
            {
                eventData.position = MultiTouchPosition;
                base.OnBeginDrag(eventData);
            }
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            if (_xrInputManager != null)
            {
                eventData.position = MultiTouchPosition;
                base.OnEndDrag(eventData);
            }
        }

        public override void OnDrag(PointerEventData eventData)
        {
            if (_xrInputManager != null)
            {
                eventData.position = MultiTouchPosition;
                base.OnDrag(eventData);
            }
        }
    }
