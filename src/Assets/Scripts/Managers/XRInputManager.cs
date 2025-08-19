using MeadowGames.UINodeConnect4;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using Viroo.Cameras;
using Zenject;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Presenters;
using Interfaces;

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
        private bool lastTriggerState = false;

        [Header("Dynamic Detection")]
        [SerializeField] private bool isSearchingForCorrectInteractor = true;
        [SerializeField] private float searchInterval = 0.5f;
        [SerializeField] private string[] preferredInteractorNames = {"RightXRRayInteractor", "Right Ray Interactor", "LeftXRRayInteractor", "Left Ray Interactor", "XRRayInteractor", "Hand Ray Interactor"};
        [SerializeField] private string[] blockedInteractorNames = {"XRGazeInteractor", "Gaze"};
        
        [Header("Manual Control")]
        [SerializeField] private bool forceRightController = true;
        [SerializeField] private KeyCode switchControllerKey = KeyCode.C;

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

        void Update()
        {
            // Manuel controller değiştirme
            if (Input.GetKeyDown(switchControllerKey))
            {
                SwitchToSpecificController();
            }
        }

        void Start()
        {
            // Dinamik olarak doğru XRRayInteractor'ı bul
            StartCoroutine(FindCorrectXRRayInteractor());
        }

        private IEnumerator FindCorrectXRRayInteractor()
        {
            LogManager.LogInput("Starting dynamic XRRayInteractor search...");
            
            while (isSearchingForCorrectInteractor)
            {
                XRRayInteractor foundInteractor = FindBestXRRayInteractor();
                
                if (foundInteractor != null && IsValidInteractor(foundInteractor))
                {
                    // Eski interactor'dan farklıysa değiştir
                    if (xrRayInteractor != foundInteractor)
                    {
                        xrRayInteractor = foundInteractor;
                        LogManager.LogInput($"Found CORRECT XRRayInteractor: {xrRayInteractor.name}");
                        LogManager.LogInput($"XRRayInteractor active: {xrRayInteractor.gameObject.activeInHierarchy}");
                        LogManager.LogInput($"XRRayInteractor enabled: {xrRayInteractor.enabled}");
                        
                        // Eğer hand controller bulundu ise aramayı durdur
                        if (IsHandController(foundInteractor))
                        {
                            isSearchingForCorrectInteractor = false;
                            LogManager.LogInput("Hand controller found! Stopping search.");
                        }
                    }
                }
                else
                {
                    LogManager.LogInput("No valid XRRayInteractor found, continuing search...");
                }
                
                yield return new WaitForSeconds(searchInterval);
            }
        }

        private XRRayInteractor FindBestXRRayInteractor()
        {
            XRRayInteractor[] allInteractors = FindObjectsOfType<XRRayInteractor>();
            LogManager.LogInput($"Found {allInteractors.Length} total XRRayInteractors");
            
            // Manuel force right controller kontrolü
            if (forceRightController)
            {
                foreach (XRRayInteractor interactor in allInteractors)
                {
                    if ((interactor.name.Contains("Right") || interactor.name.Contains("RightXR")) && 
                        interactor.gameObject.activeInHierarchy && !IsBlockedInteractor(interactor))
                    {
                        LogManager.LogInput($"FORCED RIGHT controller: {interactor.name}");
                        return interactor;
                    }
                }
            }
            
            // 1. Önce preferred isimli olanları ara
            foreach (string preferredName in preferredInteractorNames)
            {
                foreach (XRRayInteractor interactor in allInteractors)
                {
                    if (interactor.name.Contains(preferredName) && interactor.gameObject.activeInHierarchy)
                    {
                        LogManager.LogInput($"Found preferred interactor: {interactor.name}");
                        return interactor;
                    }
                }
            }
            
            // 2. Sonra blocked olmayan herhangi bir aktif interactor
            foreach (XRRayInteractor interactor in allInteractors)
            {
                if (interactor.gameObject.activeInHierarchy && !IsBlockedInteractor(interactor))
                {
                    LogManager.LogInput($"Found fallback interactor: {interactor.name}");
                    return interactor;
                }
            }
            
            return null;
        }

        private void SwitchToSpecificController()
        {
            LogManager.LogInput("Manual controller switch requested");
            
            XRRayInteractor[] allInteractors = FindObjectsOfType<XRRayInteractor>();
            
            // Şu anki interactor'ı logla
            if (xrRayInteractor != null)
            {
                LogManager.LogInput($"Current interactor: {xrRayInteractor.name}");
            }
            
            // Mevcut tüm interactorları listele
            LogManager.LogInput("Available interactors:");
            for (int i = 0; i < allInteractors.Length; i++)
            {
                var interactor = allInteractors[i];
                LogManager.LogInput($"[{i}] {interactor.name} - Active: {interactor.gameObject.activeInHierarchy}");
            }
            
            // forceRightController toggle et
            forceRightController = !forceRightController;
            LogManager.LogInput($"Force right controller toggled to: {forceRightController}");
            
            // Yeniden ara
            isSearchingForCorrectInteractor = true;
            StartCoroutine(FindCorrectXRRayInteractor());
        }

        private bool IsValidInteractor(XRRayInteractor interactor)
        {
            if (interactor == null) return false;
            if (!interactor.gameObject.activeInHierarchy) return false;
            if (!interactor.enabled) return false;
            
            return !IsBlockedInteractor(interactor);
        }

        private bool IsHandController(XRRayInteractor interactor)
        {
            foreach (string preferredName in preferredInteractorNames)
            {
                if (interactor.name.Contains(preferredName))
                    return true;
            }
            return false;
        }

        private bool IsBlockedInteractor(XRRayInteractor interactor)
        {
            foreach (string blockedName in blockedInteractorNames)
            {
                if (interactor.name.Contains(blockedName))
                {
                    LogManager.LogInput($"Blocking interactor: {interactor.name} (contains '{blockedName}')");
                    return true;
                }
            }
            return false;
        }

        public override Vector3 ScreenPointerPosition
        {
            get
            {
                if (xrRayInteractor != null && xrRayInteractor.gameObject.activeInHierarchy)
                {
                    if (TryGetPrecisionRaycastHit(out RaycastHit hit))
                    {
                        if (Camera.main == null)
                        {
                            LogManager.LogError("XRInputManager: Camera.main is null");
                        }
                        return Camera.main.WorldToScreenPoint(hit.point);
                        
                    }
                }

                return Vector3.zero;
            }
        }

        // PRECISION RAYCAST - Sphere cast yerine ince raycast kullanır
        public override bool TryGetPrecisionRaycastHit(out RaycastHit hit)
        {
            hit = default(RaycastHit);
            
            if (xrRayInteractor == null || xrRayInteractor.rayOriginTransform == null)
                return false;
            
            // Ray origin ve direction al
            Transform rayOrigin = xrRayInteractor.rayOriginTransform;
            Vector3 origin = rayOrigin.position;
            Vector3 direction = rayOrigin.forward;
            
            // Max distance
            float maxDistance = 10f;
            
            // LayerMask - UI ve RayInteraction layer'larını dahil et
            LayerMask layerMask = (1 << LayerMask.NameToLayer("UI")) | (1 << LayerMask.NameToLayer("RayInteraction")) | (1 << LayerMask.NameToLayer("Default"));
            
            // PRECISION PHYSICS RAYCAST - İnce çizgi raycast
            bool hasHit = Physics.Raycast(origin, direction, out hit, maxDistance, layerMask, QueryTriggerInteraction.Collide);
            
            return hasHit;
        }

        public override Vector3 GetCanvasPointerPosition(GraphManager graphManager)
        {
            if (xrRayInteractor == null)
            {
                xrRayInteractor = FindObjectOfType<XRRayInteractor>();
                if (xrRayInteractor == null)
                {
                    Debug.LogWarning("XRRayInteractor sahnede bulunamadı.");
                    LogManager.LogError("XRInputManager: XRRayInteractor sahnede bulunamadi.");
                    return Vector3.zero;
                }
            }

            if (TryGetPrecisionRaycastHit(out RaycastHit hit))
            {
                Vector3 worldHitPoint = hit.point;

                // ÖNEMLİ: Direkt olarak Canvas'ın RectTransform'una göre inverse transform et
                RectTransform canvasRect = graphManager.contentTransform;

                // Doğru dönüşüm budur:
                Vector3 localPoint = canvasRect.InverseTransformPoint(worldHitPoint);
                
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
                // LogManager.LogError("XRInputManager: xrRayInteractor is null.");
                xrRayInteractor = FindObjectOfType<XRRayInteractor>();
                return;
            }

            if (!xrRayInteractor.gameObject.activeInHierarchy)
            {
                // LogManager.LogError("XRInputManager: xrRayInteractor is not active in hierarchy");
                return;
            }

            VisualizeRay(); // <-- Raycast'i görselleştirme ekledim

            if (XRTriggerPressed())
            {
                wasTriggerPressed = true;
                e_OnPointerDown.Invoke();
            }
            else if (XRDragging())
            {
                if (TryGetPrecisionRaycastHit(out RaycastHit hit))
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
                bool isPressed = result > 0.1f;
                // Sadece ilk kez değiştiğinde logla (debug için)
                if (lastTriggerState != isPressed)
                {
                    lastTriggerState = isPressed;
                }
                return isPressed;
            }

            return false;
        }

        bool XRTriggerPressed()
        {
            bool current = GetRawTriggerState();
            if (current && !wasTriggerPressed)
            {
                pointerDownPosition = ScreenPointerPosition;
                LogManager.LogInput("Trigger PRESSED - OnPointerDown will fire");
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
            bool released = !current && wasTriggerPressed;
            if (released)
            {
                LogManager.LogInput("Trigger RELEASED - OnPointerUp will fire");
            }
            return released;
        }

        bool XRAux0Pressed()
        {
            return false;
        }

        public void OnDeleteKeyPressed()
        {
            Debug.Log("OnDeleteKeyPressed çağrıldı!");
            LogManager.LogInput("OnDeleteKeyPressed çağrıldı!");
            
            bool hasDeletedSomething = false;
            
            // 1. Önce SystemManager'daki selectedElements'ı kontrol et (UI Node'lar için)
            if (_systemManager != null)
            {
                Debug.Log($"Seçili eleman sayısı: {_systemManager.selectedElements.Count}");
                LogManager.LogInput($"Seçili eleman sayısı: {_systemManager.selectedElements.Count}");
                
                // ÖNEMLI: Seçili elemanların kopyasını al
                var selectedElementsCopy = _systemManager.selectedElements.ToList();
                
                for (int i = 0; i < selectedElementsCopy.Count; i++)
                {
                    var element = selectedElementsCopy[i];
                    Debug.Log($"Seçili eleman {i}: {element?.GetType().Name} - {(element as BaseNodePresenter)?.Model?.Title}");
                    LogManager.LogInput($"Seçili eleman {i}: {element?.GetType().Name} - {(element as BaseNodePresenter)?.Model?.Title}");
                }
                
                // Eğer kopyada eleman varsa direkt sil
                if (selectedElementsCopy.Count > 0)
                {
                    Debug.Log("Seçili elemanlar bulundu, direkt silme işlemi başlatılıyor...");
                    LogManager.LogInput("Seçili elemanlar bulundu, direkt silme işlemi başlatılıyor...");
                    
                    // Direkt silme işlemini burada yap
                    for (int i = selectedElementsCopy.Count - 1; i >= 0; i--)
                    {
                        var element = selectedElementsCopy[i];
                        Debug.Log($"Siliniyor: {element?.GetType().Name}");
                        LogManager.LogInput($"Siliniyor: {element?.GetType().Name}");
                        element.Remove();
                    }
                    
                    Debug.Log("Direkt silme işlemi tamamlandı.");
                    LogManager.LogInput("Direkt silme işlemi tamamlandı.");
                    hasDeletedSomething = true;
                }
            }
            else
            {
                Debug.Log("SystemManager null!");
                LogManager.LogError("SystemManager null!");
            }
            
            // 2. SystemManager'daki Selected3DObject'i kontrol et (TransformGizmo ile seçilen nesneler için)
            if (_systemManager != null && _systemManager.Selected3DObject != null)
            {
                var selectedObject = _systemManager.Selected3DObject;
                Debug.Log($"Seçili 3D obje bulundu: {selectedObject.name}");
                LogManager.LogInput($"Seçili 3D obje bulundu: {selectedObject.name}");
                
                // ObjectPresenter'ını bul ve Remove() çağır
                var objectPresenter = selectedObject.GetComponent<ObjectPresenter>();
                if (objectPresenter != null)
                {
                    Debug.Log($"ObjectPresenter bulundu, siliniyor: {selectedObject.name}");
                    LogManager.LogInput($"ObjectPresenter bulundu, siliniyor: {selectedObject.name}");
                    objectPresenter.Remove();
                    hasDeletedSomething = true;
                    
                    // TransformGizmo'nun seçimlerini temizle
                    if (_systemManager._transformGizmo != null)
                    {
                        _systemManager._transformGizmo.ClearTargets();
                        Debug.Log("TransformGizmo seçimleri temizlendi.");
                        LogManager.LogInput("TransformGizmo seçimleri temizlendi.");
                    }
                    
                    // Selected3DObject'i temizle
                    _systemManager.Selected3DObject = null;
                }
                else
                {
                    Debug.Log($"ObjectPresenter bulunamadı: {selectedObject.name}");
                    LogManager.LogInput($"ObjectPresenter bulunamadı: {selectedObject.name}");
                }
            }
            else
            {
                Debug.Log("Seçili 3D obje yok.");
                LogManager.LogInput("Seçili 3D obje yok.");
            }
            
            // 3. Eğer hiçbir şey silinmediyse fallback olarak event'i tetikle
            if (!hasDeletedSomething)
            {
                Debug.Log("Hiçbir şey silinmedi, e_OnDelete.Invoke() çağrılıyor...");
                LogManager.LogInput("Hiçbir şey silinmedi, e_OnDelete.Invoke() çağrılıyor...");
                e_OnDelete.Invoke();
                Debug.Log("e_OnDelete.Invoke() tamamlandı.");
                LogManager.LogInput("e_OnDelete.Invoke() tamamlandı.");
            }
        }

        /// <summary>
        /// XRRayInteractor'ın rayını ve hit noktasını görselleştiren metot
        /// </summary>
        private void VisualizeRay()
        {
            if (TryGetPrecisionRaycastHit(out RaycastHit hit))
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

        public bool IsGripPressed()
        {
            // Sağ el kontrol cihazındaki grip button kullanılıyor.
            InputDevice rightHandDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            bool isPressed = false;
            if (rightHandDevice.TryGetFeatureValue(CommonUsages.gripButton, out isPressed))
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