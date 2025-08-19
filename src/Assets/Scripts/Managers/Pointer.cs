using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using Interfaces;
using NodeSystem.Events;
using Zenject;
using System;
using Helpers.PortMatchRules;
using Presenters;
using UnityEngine.EventSystems;
using System.Collections;
using Enums;
using Unity.VisualScripting;
using NodeSystem;
using System.Threading.Tasks;
using Virtualware.Networking.Client;


namespace Managers
{
    public class Pointer : MonoBehaviour
    {
        private bool _isNodeCreationMode = false;
        private GraphManager _graphManager;
        private XRInputManager _inputManager;
        private Raycaster _raycaster;
        [Header("Optional Settings")] public Image customImage;
        private Image _pointerImage;
        private Sprite _defaultIcon;
        private Sprite _dragIcon;
        private RectTransform _rectTransform;
        private SystemManager _systemManager;
        private ConnectionPresenter _lastHoveredConnection;
        private Color _color;
        [HideInInspector] public Vector3 position;
        
        // Viroo Network Service
        private INetworkObjectsService _networkObjectsService;

        private Vector3 _lastPosition;

        public Sprite iconDefault;
        public Sprite iconOnDrag;

        public Canvas exclusiveOnDragCanvas;
        
        // Button detection flag for preventing node drag when button is clicked
        private bool _isButtonClicked = false;
        public bool IsButtonClicked => _isButtonClicked;
        
        // Persistent button interaction mode - aktif trigger release edilene kadar
        private bool _isButtonInteractionMode = false;
        public bool IsButtonInteractionMode => _isButtonInteractionMode;

        [Header("Legacy (requires Exclusive OnDrag Canvas)")]
        public bool useLegacyDragMethod = false;

        public bool ImageIsActive => customImage && customImage.IsActive();
        
        private BaseNodePresenter _nodePresenter;
        
        // Pointer görünürlük state'i
        private bool _isPointerVisible = false;
        
        public void CreateGhostNode()
        {
            
            _nodePresenter = _graphManager.CreateNodeAtPosition(_inputManager.GetCanvasPointerPosition(_graphManager), NodeType.Ghost);
        }

        public void DestroyGhostNode()
        {
            _nodePresenter.Remove();
        }
        

        [Inject]
        public void Construct(GraphManager graphManager, XRInputManager inputManager, SystemManager systemManager,
            Raycaster raycaster)
        {
            Debug.Log("ENTER: Pointer Construct");
            LogManager.Log("ENTER: Pointer Construct");
            _graphManager = graphManager;
            _inputManager = inputManager;
            _systemManager = systemManager;
            _raycaster = raycaster;
            
            // Viroo network service için inject kuyruğuna ekle
            this.QueueForInject();
        }

        // Viroo network service injection
        protected void Inject(INetworkObjectsService networkObjectsService)
        {
            _networkObjectsService = networkObjectsService;
        }

        public void Initialize(Color color, Sprite defaultSprite, Sprite dragSprite, Vector2 size)
        {
            _pointerImage = gameObject.GetComponent<Image>();
            _rectTransform = _pointerImage.rectTransform;

            _defaultIcon = defaultSprite;
            _dragIcon = dragSprite;

            _pointerImage.sprite = _defaultIcon;
            _pointerImage.color = color;
            _pointerImage.raycastTarget = false;
            _rectTransform.sizeDelta = size;

            var canvas = gameObject.GetComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingOrder = 32767;

            _lastPosition = Vector3.zero;
        }

        public void OnEnable()
        {

            _systemManager.AddToUpdate(OnUpdate);

            _inputManager.e_OnPointerDown.AddListener(OnPointerDown);
            _inputManager.e_OnDrag.AddListener(OnDrag);
            _inputManager.e_OnPointerUp.AddListener(OnPointerUp);
            _inputManager.e_OnDelete.AddListener(OnDeleteKeyPressed);
            _inputManager.e_OnPointerHover.AddListener(OnPointerHover);
            //_inputManager.e_OnCheckDragCanvas.AddListener(OnActivatePointer);


        }

        public void OnDisable()
        {

            _systemManager.RemoveFromUpdate(OnUpdate);

            _inputManager.e_OnPointerDown.RemoveListener(OnPointerDown);
            _inputManager.e_OnDrag.RemoveListener(OnDrag);
            _inputManager.e_OnPointerUp.RemoveListener(OnPointerUp);
            _inputManager.e_OnDelete.RemoveListener(OnDeleteKeyPressed);
            _inputManager.e_OnPointerHover.RemoveListener(OnPointerHover);
        }

        
        void OnUpdate()
        {
            if (_inputManager != null)
            {
                // XRRayInteractor null ise bekle
                if (_inputManager.xrRayInteractor == null)
                {
                    Hide();
                    return;
                }

                // Interactor aktif değilse bekle
                if (!_inputManager.xrRayInteractor.gameObject.activeInHierarchy)
                {
                    Hide();
                    return;
                }

                // CUSTOM PRECISION RAYCAST - TryGetCurrent3DRaycastHit yerine kendi implementasyonumuz
                if (_inputManager.TryGetPrecisionRaycastHit(out RaycastHit hit))
                {
                    if (hit.transform != null && hit.transform.gameObject != null && hit.transform.gameObject.name == "EditorMask")
                    {
                        Show();
                    }
                    else
                    {
                        Hide();
                    }
                }
                else
                {
                    Hide();
                }
            }
            else
            {
                Debug.Log("inputManager is null");
                LogManager.LogError("Pointer: inputManager is null");
            }

            // SADECE POINTER GÖRÜNÜRKEN pozisyon hesapla
            if (_isPointerVisible)
            {
                Vector3 newLocalPosition = _inputManager.GetCanvasPointerPosition(_graphManager);
                
                // COORDINATE SPACE COMPENSATION - Scroll ve Scale offset'lerini kompanse et
                if (_graphManager.scrollRect != null && _graphManager.scrollRect.content != null)
                {
                    Vector2 contentPosition = _graphManager.scrollRect.content.anchoredPosition;
                    Vector3 contentScale = _graphManager.contentTransform.localScale;
                    
                    // Scale ile pozisyonu çarp (content ne kadar scale'lendiyse pointer da o kadar çarpılmalı)
                    newLocalPosition.x *= contentScale.x;
                    newLocalPosition.y *= contentScale.y;
                    
                    // Content'in scroll offset'ini ekle
                    newLocalPosition.x += contentPosition.x;
                    newLocalPosition.y += contentPosition.y;
                }
                
                ConnectionPresenter closestConnection = _raycaster.FindClosestConnectionToPosition(newLocalPosition, _graphManager.ConnectionDetectionDistance);
                if (closestConnection != _lastHoveredConnection)
                {
                    // Eski bağlantıdan çıkış event'i tetikle
                    if (_lastHoveredConnection != null)
                    {
                        //Debug.Log($"Hover çıkıldı: {_lastHoveredConnection.Model.ID}");
                        _lastHoveredConnection.OnPointerHoverExit();
                    }

                    // Yeni bağlantıya giriş event'i tetikle
                    if (closestConnection != null)
                    {
                        //Debug.Log($"Hover edildi: {closestConnection.Model.ID}");
                        closestConnection.OnPointerHoverEnter();
                    }

                    _lastHoveredConnection = closestConnection;
                }

                if (_rectTransform != null && newLocalPosition != Vector3.zero)
                {
                    _rectTransform.anchoredPosition = newLocalPosition;
                }

                // Z değerini sıfırla
                _rectTransform.localPosition = new Vector3(_rectTransform.localPosition.x, _rectTransform.localPosition.y, 0);
            }
            else
            {
                // Pointer gizliyken connection hover'ları temizle
                if (_lastHoveredConnection != null)
                {
                    _lastHoveredConnection.OnPointerHoverExit();
                    _lastHoveredConnection = null;
                }
            }



        }

        private void UpdateConnectionHoverState(Vector3 pointerPosition)
        {
            ConnectionPresenter closestConnection = _raycaster.FindClosestConnectionToPosition(pointerPosition, _graphManager.ConnectionDetectionDistance);

            if (closestConnection != _lastHoveredConnection)
            {
                // Eski bağlantıdan çıkış event'i tetikle
                if (_lastHoveredConnection != null)
                {
                    //Debug.Log($"Hover çıkıldı: {_lastHoveredConnection.Model.ID}");
                    _lastHoveredConnection.OnPointerHoverExit();
                }

                // Yeni bağlantıya giriş event'i tetikle
                if (closestConnection != null)
                {
                    //Debug.Log($"Hover edildi: {closestConnection.Model.ID}");
                    closestConnection.OnPointerHoverEnter();
                }

                _lastHoveredConnection = closestConnection;
            }
        }


        public void ResetPointerPositionWhenScrolling()
        {
            if (_rectTransform == null || _inputManager == null || _graphManager == null)
                return;

            // Pointer görünmüyorsa pozisyon güncellemesi yapma
            if (!_isPointerVisible)
                return;

            Vector3 newLocalPosition = _inputManager.GetCanvasPointerPosition(_graphManager);
            
            // COORDINATE SPACE COMPENSATION - Scroll ve Scale offset'lerini kompanse et
            if (_graphManager.scrollRect != null && _graphManager.scrollRect.content != null)
            {
                Vector2 contentPosition = _graphManager.scrollRect.content.anchoredPosition;
                Vector3 contentScale = _graphManager.contentTransform.localScale;
                
                // Scale ile pozisyonu çarp (content ne kadar scale'lendiyse pointer da o kadar çarpılmalı)
                newLocalPosition.x *= contentScale.x;
                newLocalPosition.y *= contentScale.y;
                
                // Content'in scroll offset'ini ekle
                newLocalPosition.x += contentPosition.x;
                newLocalPosition.y += contentPosition.y;
            }

            // Check if position has changed horizontally or vertically
            if (newLocalPosition != _lastPosition)
            {
                // Update position
                _rectTransform.anchoredPosition = newLocalPosition;

                // Store new position
                _lastPosition = newLocalPosition;

                // Update connection hover state (original position'ı kullan, scroll offset olmadan)
                Vector3 originalPosition = _inputManager.GetCanvasPointerPosition(_graphManager);
                UpdateConnectionHoverState(originalPosition);

                // Z değerini sıfırla
                _rectTransform.localPosition = new Vector3(_rectTransform.localPosition.x, _rectTransform.localPosition.y, 0);

                // Store the current position for external use
                position = _rectTransform.localPosition;
            }
        }



        public void OnPointerDown()
        {
            // GUARD: Pointer gizliyse event'leri işleme
            if (!_isPointerVisible)
            {
                Debug.Log("Pointer gizli - OnPointerDown event'i ignore edildi");
                return;
            }

            // OnPointerDown event started



            Vector3 canvasLocalPosition = _inputManager.GetCanvasPointerPosition(_graphManager);
            
            // COORDINATE SPACE COMPENSATION - Connection detection için aynı compensation'ı uygula
            if (_graphManager.scrollRect != null && _graphManager.scrollRect.content != null)
            {
                Vector2 contentPosition = _graphManager.scrollRect.content.anchoredPosition;
                Vector3 contentScale = _graphManager.contentTransform.localScale;
                
                // Scale ile pozisyonu çarp (content ne kadar scale'lendiyse pointer da o kadar çarpılmalı)
                canvasLocalPosition.x *= contentScale.x;
                canvasLocalPosition.y *= contentScale.y;
                
                // Content'in scroll offset'ini ekle
                canvasLocalPosition.x += contentPosition.x;
                canvasLocalPosition.y += contentPosition.y;
            }

            ConnectionPresenter closestConnection = _raycaster.FindClosestConnectionToPosition(canvasLocalPosition, _graphManager.ConnectionDetectionDistance);

            if (closestConnection != null)
            {
                closestConnection.OnPointerDown(); // Bağlantının kendi seçme metodunu tetikler
                _systemManager.clickedElement = closestConnection;
            }

            if (_pointerImage != null)
                _pointerImage.sprite = _dragIcon;

            IElement clickedElement = GetElementCloserToPointer();
            
            if (clickedElement != null)
                _systemManager.clickedElement = clickedElement;

            if (clickedElement is ISelectable selectableElement)
            {
                _systemManager.clickedElement = (IElement)selectableElement;
            }

            if (!(_systemManager.clickedElement is IContextItem))
            {
                _graphManager.UnselectAllElements();

                if (_systemManager.clickedElement is IClickable clickable)
                {
                    clickable.OnPointerDown();
                }

                _systemManager.LTGEvents.TriggerEvent(LTGEventType.OnPointerDown, _systemManager.clickedElement);
            }
            // Context item handling (no logging needed)

            

        }




        public void SetDefaultIcon() => _pointerImage.sprite = _defaultIcon;
        public void SetDragIcon() => _pointerImage.sprite = _dragIcon;

        public void Show()
        {
            if (_pointerImage != null)
            {
                _pointerImage.enabled = true;
                _isPointerVisible = true;
            }
        }

        public void Hide()
        {
            if (_pointerImage != null)
            {
                _pointerImage.enabled = false;
                _isPointerVisible = false;
            }
        }




        public void OnDrag(Vector3 _)
        {
            // GUARD: Pointer gizliyse event'leri işleme
            if (!_isPointerVisible)
            {
                return;
            }



            if (_inputManager.TryGetPrecisionRaycastHit(out RaycastHit hit))
            {
                Vector3 localCanvasPosition = _graphManager.CanvasRectTransform.InverseTransformPoint(hit.point);

                // COORDINATE SPACE COMPENSATION - Node drag için scale kompensasyonu
                // Node'lar Content space'inde yaşar, ama pointer pozisyonu Canvas space'inde hesaplanır
                // Scale olduğunda bu iki space arasında fark oluşur
                if (_graphManager.scrollRect != null && _graphManager.scrollRect.content != null)
                {
                    Vector3 contentScale = _graphManager.contentTransform.localScale;
                    
                    // Canvas space'den Content space'e dönüşüm
                    localCanvasPosition.x /= contentScale.x;
                    localCanvasPosition.y /= contentScale.y;
                }

                if (_systemManager.clickedElement is IDraggable draggable)
                {
                    draggable.OnDrag(localCanvasPosition);
                }

                _systemManager.LTGEvents.TriggerEvent(LTGEventType.OnDrag, _systemManager.clickedElement);
            }
            
            
        }

        public void OnPointerUp()
        {
            // GUARD: Pointer gizliyse event'leri işleme
            if (!_isPointerVisible)
            {
                return;
            }

            // ✅ TRIGGER RELEASE CHECK - Persistent button mode için trigger kontrolü
            bool isTriggerReleased = !_inputManager.PointerPress; // Trigger release edildi mi?
            
            if (_isButtonInteractionMode)
            {
                if (isTriggerReleased)
                {
                    // ✅ TRIGGER RELEASE EDİLDİ - Persistent mode'u kapat
                    _isButtonInteractionMode = false;
                    _isButtonClicked = false;
                    
                    // ✅ SCROLL VIEW'I TEKRAR AKTİF ET - Persistent mode sona erdi
                    if (_graphManager != null && _graphManager.scrollRect != null)
                    {
                        _graphManager.scrollRect.enabled = true;
                    }
                }
            }
            else
            {
                // ✅ NORMAL MODE - Regular button flag reset
                _isButtonClicked = false;
            }

            if (_pointerImage != null)
                _pointerImage.sprite = _defaultIcon;



            if (_systemManager.clickedElement is PortPresenter startPort)
            {
                PortPresenter targetPort = RaycastPortOfOppositPolarity(startPort);

                if (targetPort != null)
                {
                    // Burada doğrudan hedef portun OnPointerUp'ını çağırıyoruz.
                    targetPort.OnPointerUp();
                }
                else
                {
                    startPort.OnPointerUp(); // Bağlantıyı iptal etmek için başlangıç porta haber ver
                }
            }
            if (_systemManager.clickedElement is IClickable clickable)
            {
                clickable.OnPointerUp();
            }

            _systemManager.clickedElement = null;
        }

        public async void OnDeleteKeyPressed()
        {
            // GUARD: Pointer gizliyse delete event'ini işleme
            if (!_isPointerVisible)
            {
                return;
            }

            for (int i = _systemManager.selectedElements.Count - 1; i >= 0; i--)
            {
                var element = _systemManager.selectedElements[i];
                
                // NetworkObject komponenti var mı kontrol et
                if (element is MonoBehaviour mb && mb.gameObject.TryGetComponent<NetworkObject>(out var networkObject))
                {
                    // Viroo network destroy işlemi
                    if (_networkObjectsService != null)
                    {
                        try
                        {
                            await _networkObjectsService.DestroyObject(networkObject);
                            Debug.Log($"Viroo network object destroyed: {networkObject.ObjectId}");
                        }
                        catch (System.Exception ex)
                        {
                            Debug.LogError($"Viroo network destroy failed: {ex.Message}");
                            // Network destroy başarısız olursa local remove
                            element.Remove();
                        }
                    }
                    else
                    {
                        Debug.LogWarning("NetworkObjectsService not available, using local remove");
                        element.Remove();
                    }
                }
                else
                {
                    // NetworkObject olmayan objeler için normal remove
                    element.Remove();
                }
            }

            _systemManager.LTGEvents.TriggerEvent(LTGEventType.OnDeleteKeyPressed, null);
        }

        IElement _lastHoverElement;

        public void OnPointerHover()
        {
            // GUARD: Pointer gizliyse hover event'lerini işleme
            if (!_isPointerVisible)
            {
                // Eğer pointer gizliyse ve önceden hover'da bir element varsa, hover'dan çık
                if (_lastHoverElement is IHover)
                {
                    (_lastHoverElement as IHover).OnPointerHoverExit();
                    _systemManager.LTGEvents.TriggerEvent(LTGEventType.OnPointerHoverExit, _lastHoverElement);
                    _lastHoverElement = null;
                }
                return;
            }

            IElement hoverElement = GetElementCloserToPointer();
            _systemManager.hoverElement = hoverElement;
            if (hoverElement != _lastHoverElement)
            {
                if (hoverElement is IHover)
                {
                    (hoverElement as IHover).OnPointerHoverEnter();
                    _systemManager.LTGEvents.TriggerEvent(LTGEventType.OnPointerHoverEnter, hoverElement);
                }

                if (_lastHoverElement is IHover)
                {
                    (_lastHoverElement as IHover).OnPointerHoverExit();
                    _systemManager.LTGEvents.TriggerEvent(LTGEventType.OnPointerHoverExit, _lastHoverElement);
                }
            }

            _lastHoverElement = hoverElement;
        }

        public IElement GetElementCloserToPointer()
        {
            XRInputManager inputManager = _inputManager;

            // ✅ BUTTON FLAG RESETLETip UI RAYCAST İLE KONTROL YAP
            // Persistent mode aktif değilse flag'i temizle, aktifse koru
            if (!_isButtonInteractionMode)
            {
                _isButtonClicked = false; // Normal mode'da her raycast'te temizle
            }
            
                        // UI elementleri için GraphicRaycaster kullan (collider gerektirmez)
            var uiRaycastResults = _raycaster.RaycastUIAll(inputManager.ScreenPointerPosition);
            
            // UI raycast sonuçlarında button var mı kontrol et
            foreach (var result in uiRaycastResults)
            {
                UnityEngine.UI.Button buttonComponent = result.gameObject.GetComponent<UnityEngine.UI.Button>() ?? result.gameObject.GetComponentInParent<UnityEngine.UI.Button>();
                if (buttonComponent != null && buttonComponent.interactable)
                {
                    _isButtonClicked = true;
                    _isButtonInteractionMode = true; // ✅ PERSISTENT MODE - Trigger release edilene kadar aktif
                    
                    // ✅ SCROLL VIEW'I DE ENGELLE - Button tıklaması sırasında scroll yapmasın
                    if (_graphManager != null && _graphManager.scrollRect != null)
                    {
                        _graphManager.scrollRect.enabled = false;
                    }
                    
                    break;
                }
            }

            if (!_isButtonClicked)
            {
                // ✅ BUTTON DEĞİLSE VE PERSISTENT MODE DEĞİLSE VE DRAG İŞLEMİ DEĞİLSE SCROLL VIEW AKTİF KALMALI
                bool isPortDragging = _systemManager.clickedElement is PortPresenter;
                bool isConnectionDragging = _systemManager.clickedElement is ConnectionPresenter;
                bool isDragOperation = isPortDragging || isConnectionDragging;
                
                if (!_isButtonInteractionMode && !isDragOperation && _graphManager != null && _graphManager.scrollRect != null && !_graphManager.scrollRect.enabled)
                {
                    _graphManager.scrollRect.enabled = true;
                }
            }
            
            // XR için özel raycast implementasyonu (3D objeler için)
            if (inputManager.xrRayInteractor != null && _inputManager.TryGetPrecisionRaycastHit(out RaycastHit hit))
            {
                // Hit olan obje veya parent'ında IElement arayalım
                GameObject hitObject = hit.collider.gameObject;
                
                // Önce hit edilen obje üzerinde IElement var mı kontrol et
                IElement element = hitObject.GetComponent<IElement>();
                if (element != null)
                {
                    return element;
                }
                
                // Hit edilen obje üzerinde yoksa parent hierarchy'de ara
                element = hitObject.GetComponentInParent<IElement>();
                if (element != null)
                {
                    return element;
                }
            }
            else
            {
                // XR raycast başarısız olduğunda button flag'ini temizle
                if (!_isButtonInteractionMode)
                {
                    _isButtonClicked = false;
                }
            }

            // XR raycast başarısızsa fallback olarak eski sistemi kullan
            List<IElement> orderedElementsList = _raycaster.OrderedElementsAtPosition(_graphManager,
                inputManager.ScreenPointerPosition, inputManager.GetCanvasPointerPosition(_graphManager));

            if (orderedElementsList.Count > 0)
            {
                return orderedElementsList[0];
            }

            return null;
        }

        public Image GetPointerImage()
        {
            return _pointerImage;
        }

        public PortPresenter RaycastPortOfOppositPolarity(PortPresenter draggedPort)
        {
            PortPresenter closestPort = null;
            List<RaycastResult> results = _raycaster.RaycastUIAll(_inputManager.ScreenPointerPosition);
            IElement element = null;
            foreach (RaycastResult result in results)
            {
                element = result.gameObject.GetComponent<IElement>();

                if (element != null)
                {
                    if (!(element is IClickable) || !(element as IClickable).DisableClick)
                    {
                        if (element is PortPresenter)
                        {
                            PortPresenter portPresenter = element as PortPresenter;
                            if (draggedPort != portPresenter && portPresenter.Model.HasSpots &&
                                draggedPort.Model.HasSpots)
                            {
                                Debug.Log("PORT PRESENTER: " + portPresenter.Model.baseNode.ID + "DRAGGED PORT: " +
                                          draggedPort.Model.baseNode.ID);
                                if ((portPresenter.Model.baseNode == draggedPort.Model.baseNode &&
                                     portPresenter.Model.baseNode.Model.EnableSelfConnection) ||
                                    portPresenter.Model.baseNode != draggedPort.Model.baseNode)
                                    if (portPresenter.Polarity != draggedPort.Polarity || portPresenter.Polarity ==
                                        NodeSystem.PolarityType.Bidirectional)
                                    {
                                        var matchRule = FindObjectOfType<CustomPortMatchRule>();
                                        if (matchRule != null && matchRule.ExecuteRule(draggedPort.Model, portPresenter.Model))
                                        {
                                            return portPresenter;  // Kural başarılı olduysa burada işlem yapabilirsiniz
                                        }

                                        // Eğer custom rule yoksa veya başarısızsa, normal connection kurallarını kontrol et
                                        return portPresenter;
                                    }
                            }
                        }
                    }
                }
            }

            return closestPort;
        }
    }
}