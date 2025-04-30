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
using Unity.VisualScripting;
using NodeSystem;

namespace Managers
{
    public class Pointer : MonoBehaviour
    {
        public NoDragScrollRect noDragScrollRect;
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

        public Sprite iconDefault;
        public Sprite iconOnDrag;

        public Canvas exclusiveOnDragCanvas;

        [Header("Legacy (requires Exclusive OnDrag Canvas)")]
        public bool useLegacyDragMethod = false;

        public bool ImageIsActive => customImage && customImage.IsActive();

        [Inject]
        public void Construct(GraphManager graphManager, XRInputManager inputManager, SystemManager systemManager,
            Raycaster raycaster)
        {
            Debug.Log("ENTER: Pointer Construct");
            _graphManager = graphManager;
            _inputManager = inputManager;
            _systemManager = systemManager;
            _raycaster = raycaster;
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
                if (_inputManager.xrRayInteractor != null && _inputManager.xrRayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
                {
                    if (hit.transform != null && hit.transform.gameObject != null && hit.transform.gameObject.name == "Plane")
                        Show();
                    else
                        Hide();
                }
                else
                {
                    Debug.Log("xrRayInteractor is null or TryGetCurrent3DRaycastHit failed");
                }
            }
            else
            {
                Debug.Log("inputManager is null");
            }

             Vector3 newLocalPosition = _inputManager.GetCanvasPointerPosition(_graphManager);
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

        //public void ResetPointerPosition()
        //{
            
        //}



        public void OnPointerDown()
        {
            Vector3 canvasLocalPosition = _inputManager.GetCanvasPointerPosition(_graphManager);

            ConnectionPresenter closestConnection = _raycaster.FindClosestConnectionToPosition(canvasLocalPosition, _graphManager.ConnectionDetectionDistance);

            if (closestConnection != null)
            {
                Debug.Log($"Bağlantıya tıklandı: {closestConnection.Model.ID}");
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
        }

       


        public void SetDefaultIcon() => _pointerImage.sprite = _defaultIcon;
        public void SetDragIcon() => _pointerImage.sprite = _dragIcon;

        public void Show()
        {
            _pointerImage.enabled = true;
        }

        public void Hide()
        {
            _pointerImage.enabled = false;

        }




        public void OnDrag(Vector3 _)
        {
            if (_inputManager.xrRayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
            {
                Vector3 localCanvasPosition = _graphManager.CanvasRectTransform.InverseTransformPoint(hit.point);


                // Canvas'ın scale'ini hesaba katmak zorundaysan bunu yap:
                Vector3 scale = _graphManager.CanvasRectTransform.localScale;


                if (_systemManager.clickedElement is IDraggable draggable)
                {
                    draggable.OnDrag(localCanvasPosition);
                }

                _systemManager.LTGEvents.TriggerEvent(LTGEventType.OnDrag, _systemManager.clickedElement);
            }
        }

        public void OnPointerUp()
        {
            if (_pointerImage != null)
                _pointerImage.sprite = _defaultIcon;

            if (_systemManager.clickedElement is PortPresenter startPort)
            {
                PortPresenter targetPort = RaycastPortOfOppositPolarity(startPort);

                if (targetPort != null)
                {
                    Debug.Log($"Hedef port bulundu: {targetPort.ID}");

                    // Burada doğrudan hedef portun OnPointerUp'ını çağırıyoruz.
                    targetPort.OnPointerUp();
                }
                else
                {
                    Debug.LogWarning("Hedef port bulunamadı.");
                    startPort.OnPointerUp(); // Bağlantıyı iptal etmek için başlangıç porta haber ver
                }
            }

            _systemManager.clickedElement = null;
        }

        public void OnDeleteKeyPressed()
        {
            for (int i = _systemManager.selectedElements.Count - 1; i >= 0; i--)
            {
                _systemManager.selectedElements[i].Remove();
            }

            _systemManager.LTGEvents.TriggerEvent(LTGEventType.OnDeleteKeyPressed, null);
        }

        IElement _lastHoverElement;

        public void OnPointerHover()
        {
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

                                        return null;


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