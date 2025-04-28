using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace Managers
{
    public class InputManager_LegacyInputSystem : InputManager
    {
        private SystemManager _systemManager;        
        private GraphManager _graphManager;
        public KeyCode clickKey = KeyCode.Mouse0;
        public KeyCode aux0Key = KeyCode.LeftShift;
        public KeyCode deleteKey = KeyCode.Delete;

        public override Vector3 ScreenPointerPosition => Input.mousePosition;

        public override UnityEvent e_OnPointerDown { get; set; } = new UnityEvent();
        public override UnityEvent<Vector3> e_OnDrag { get; set; } = new UnityEvent<Vector3>();
        public override UnityEvent e_OnPointerUp { get; set; } = new UnityEvent();
        public override UnityEvent e_OnDelete { get; set; } = new UnityEvent();
        public override UnityEvent e_OnPointerHover { get; set; } = new UnityEvent();
        public override bool PointerPress => Input.GetKey(clickKey);
        public override bool Aux0KeyPress => Input.GetKey(aux0Key);

        Vector3 _initialMousePos;
        [Inject]
        public void Construct(SystemManager systemManager, GraphManager graphManager)
        {
            Debug.Log("ENTER: InputManager_LegacyInputSystem Construct");
            _systemManager = systemManager;
            _graphManager = graphManager;
        }
        void OnEnable()
        {
            _systemManager.AddToUpdate(OnUpdate);
        }

        void OnDisable()
        {
            _systemManager.RemoveFromUpdate(OnUpdate);
        }

        public override void OnUpdate()
        {
            if (gameObject.activeInHierarchy)
            {
                if (Input.GetKeyDown(clickKey))
                {
                    _initialMousePos = ScreenPointerPosition;
                    OnPointerDown();
                }

                if (Input.GetKey(clickKey))
                {
                    if(Vector3.Distance(_initialMousePos, ScreenPointerPosition) > 0.1f)
                    {
                        OnDrag();
                    }
                }

                if (Input.GetKeyUp(clickKey))
                {
                    OnPointerUp();
                }

                if (Input.GetKeyDown(deleteKey))
                {
                    OnDeleteKeyPressed();
                }

                OnPointerHover();
            }
        }

        public void OnPointerDown()
        {
            e_OnPointerDown.Invoke();
        }

        public void OnDrag()
        {
            //e_OnDrag.Invoke();
        }

        public void OnPointerUp()
        {
            e_OnPointerUp.Invoke();
        }

        public void OnDeleteKeyPressed()
        {
            Debug.Log("Delete pressed");
            e_OnDelete.Invoke();
        }

        public void OnPointerHover()
        {
            e_OnPointerHover.Invoke();
        }

        public override Vector3 GetCanvasPointerPosition(GraphManager graphManager)
        {
            if (graphManager.CanvasRenderMode == RenderMode.ScreenSpaceOverlay)
            {
                return ScreenPointerPosition;
            }
            else 
            {
                Camera mainCamera = Camera.main;
                var screenPoint = ScreenPointerPosition;
                screenPoint.z = graphManager.transform.position.z - mainCamera.transform.position.z; //distance of the plane from the camera
                return mainCamera.ScreenToWorldPoint(screenPoint);
            }
        }
    }
}