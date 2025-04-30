using MeadowGames.UINodeConnect4;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using Zenject;

namespace Managers
{
    public class XRInputManager : InputManager
    {
        private SystemManager _systemManager;
        private GraphManager _graphManager;
        [HideInInspector] public XRRayInteractor xrRayInteractor;
        public bool wasTriggerPressed = false;
        private const float dragThreshold = .1f;
        private Vector3 pointerDownPosition = Vector3.zero;

        [Inject]
        public void Construct(SystemManager systemManager, GraphManager graphManager)
        {
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

        void Start()
        {
            xrRayInteractor = FindObjectOfType<XRRayInteractor>();
        }

        public override Vector3 ScreenPointerPosition
        {
            get
            {
                if (xrRayInteractor != null && xrRayInteractor.gameObject.activeInHierarchy)
                {
                    if (xrRayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
                    {
                        return Camera.main.WorldToScreenPoint(hit.point);
                        
                    }
                }

                return Vector3.zero;
            }
        }

        //public override Vector3 GetCanvasPointerPosition(GraphManager graphManager)
        //{
        //    if (xrRayInteractor == null)
        //    {
        //        xrRayInteractor = FindObjectOfType<XRRayInteractor>();
        //        if (xrRayInteractor == null)
        //        {
        //            Debug.LogWarning("XRRayInteractor sahnede bulunamadı.");
        //            return Vector3.zero;
        //        }
        //    }

        //    // Raycast hit kontrolü yapıyoruz
        //    if (xrRayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        //    {
        //        if (hit.collider != null) // Raycast bir collider'a çarptıysa
        //        {
        //            // Raycast'in çarptığı dünya pozisyonunu alıyoruz
        //            Vector3 worldHitPoint = hit.point;



        //            // Canvas'ın RectTransform'unu al
        //            RectTransform canvasRect = graphManager.contentTransform;
        //            if (canvasRect == null) return Vector3.zero;


        //            Vector3 localPoint = canvasRect.InverseTransformPoint(worldHitPoint);

        //            Vector3 contentSize = canvasRect.sizeDelta;

        //            localPoint = new Vector3(
        //                Mathf.Round(localPoint.x / contentSize.x) * contentSize.x,
        //                Mathf.Round(localPoint.y / contentSize.y) * contentSize.y,
        //                0 // Z değerini 0 yapıyoruz
        //            );


        //        }
        //        else
        //        {
        //            // Eğer raycast bir collider'a çarpmadıysa
        //            Debug.Log("Raycast didn't hit anything.");
        //            return Vector3.zero;
        //        }
        //    }

        //    return Vector3.zero;
        //}

        public override Vector3 GetCanvasPointerPosition(GraphManager graphManager)
        {
            if (xrRayInteractor == null)
            {
                xrRayInteractor = FindObjectOfType<XRRayInteractor>();
                if (xrRayInteractor == null)
                {
                    Debug.LogWarning("XRRayInteractor sahnede bulunamadı.");
                    return Vector3.zero;
                }
            }

            if (xrRayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
            {
                Vector3 worldHitPoint = hit.point;

                // ÖNEMLİ: Direkt olarak Canvas'ın RectTransform'una göre inverse transform et
                RectTransform canvasRect = graphManager.contentTransform;

                // Doğru dönüşüm budur:
                Vector3 localPoint = canvasRect.InverseTransformPoint(worldHitPoint);
                
                // Canvas'ın scale değerini hesaba kat - scale ile ÇARPIYORUZ
                Vector3 scale = canvasRect.localScale;
                localPoint = new Vector3(
                    localPoint.x * scale.x,
                    localPoint.y * scale.y,
                    0  // Z değeri her zaman 0
                );

                return localPoint;
            }

            return Vector3.zero;
        }





        public override bool PointerPress => XRTriggerPressed();
        public override bool Aux0KeyPress => XRAux0Pressed();

        public override UnityEvent e_OnPointerDown { get; set; } = new UnityEvent();
        public override UnityEvent<Vector3> e_OnDrag { get; set; } = new UnityEvent<Vector3>();
        public override UnityEvent e_OnPointerUp { get; set; } = new UnityEvent();
        public override UnityEvent e_OnDelete { get; set; } = new UnityEvent();
        public override UnityEvent e_OnPointerHover { get; set; } = new UnityEvent();

        

        public override void OnUpdate()
        {
           

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                _graphManager.ScaleUpGraph();
            }

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                _graphManager.ScaleDownGraph();
            }
            if (xrRayInteractor == null)
            {
                xrRayInteractor = FindObjectOfType<XRRayInteractor>();
                return;
            }

            if (!xrRayInteractor.gameObject.activeInHierarchy)
                return;

            VisualizeRay(); // <-- Raycast'i görselleştirme ekledim

            if (XRTriggerPressed())
            {
                wasTriggerPressed = true;
                e_OnPointerDown.Invoke();
            }
            else if (XRDragging())
            {
                if (xrRayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
                {
                    e_OnDrag.Invoke(hit.point);
                }
            }
            else if (XRTriggerReleased())
            {
                wasTriggerPressed = false;
                e_OnPointerUp.Invoke();
            }

            e_OnPointerHover.Invoke();

            
        }

        public void GetRawPrimaryButtonState()
        {
            if (xrRayInteractor != null && xrRayInteractor)
            {
                
            }
        }

        public bool GetRawTriggerState()
        {
            float result = 0f;
            if (xrRayInteractor != null && xrRayInteractor.uiPressInput.TryReadValue(out result))
            {
                return result > 0.1f;
            }

            return false;
        }

        bool XRTriggerPressed()
        {
            bool current = GetRawTriggerState();
            if (current && !wasTriggerPressed)
            {
                pointerDownPosition = ScreenPointerPosition;
                return true;
            }

            return false;
        }

        bool XRDragging()
        {
            bool current = GetRawTriggerState();
            if (current && wasTriggerPressed)
            {
                float distance = Vector3.Distance(ScreenPointerPosition, pointerDownPosition);
                return distance > dragThreshold;
            }

            return false;
        }

        bool XRTriggerReleased()
        {
            bool current = GetRawTriggerState();
            return !current && wasTriggerPressed;
        }

        bool XRAux0Pressed()
        {
            return false;
        }

        public void OnDeleteKeyPressed()
        {
            Debug.Log("DDeleteButtonPRESEEDD");
            e_OnDelete.Invoke();
        }

        /// <summary>
        /// XRRayInteractor'ın rayını ve hit noktasını görselleştiren metot
        /// </summary>
        private void VisualizeRay()
        {
            if (xrRayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
            {
                Vector3 origin = xrRayInteractor.rayOriginTransform.position;
                Vector3 direction = hit.point - origin;

                // Canvas'a veya UI elemanlarına çarpan nokta için küçük bir küre çiz
                Debug.DrawLine(origin, hit.point, Color.green);
                Debug.DrawRay(hit.point, hit.normal * 0.05f, Color.yellow);
                Debug.DrawRay(hit.point, Vector3.up * 0.05f, Color.red);
                Debug.DrawRay(hit.point, Vector3.right * 0.05f, Color.blue);
            }
            else
            {
                // Eğer hiçbir yere çarpmıyorsa, default uzunlukta mavi ray çiz
                Vector3 origin = xrRayInteractor.rayOriginTransform.position;
                Vector3 forward = xrRayInteractor.rayOriginTransform.forward;
                Debug.DrawRay(origin, forward * 10f, Color.cyan);
            }
        }

        public bool IsSnapPressed()
        {
            // Sağ el kontrol cihazındaki primary button (örneğin "A" butonu) kullanılıyor.
            InputDevice rightHandDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            bool isPressed = false;
            if (rightHandDevice.TryGetFeatureValue(CommonUsages.primaryButton, out isPressed))
            {
                return isPressed;
            }
            return false;
        }

        public void SetSelected3DObject(GameObject selectedObject)
        {
            _systemManager.Selected3DObject = selectedObject;
        }

        public void ResetSelected3DObject()
        {
            _systemManager.Selected3DObject = null;
        }

       
    }
}