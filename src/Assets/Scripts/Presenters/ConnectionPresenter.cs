using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Models;
using Interfaces;
using CustomGraphics;
using Zenject;
using Managers;
using UnityEngine.Assertions.Must;

namespace Presenters
{
    [RequireComponent(typeof(RectTransform))]
    public class ConnectionPresenter : MonoBehaviour, IGraphElement, ISelectable, IClickable, IDraggable, IHover
    {
        public bool EnableSelect { get; set; } = true;
        public bool DisableClick => false;

        private Color _defaultColor = Color.white;
        private Color _selectedColor = new Color(1f, 0.7f, 0f);

        private Connection _model;

        public Connection Model
        {
            get => _model;
            set => _model = value;
        }

        private RectTransform _rectTransform;
        private PortPresenter _inputPortPresenter;
        private PortPresenter _outputPortPresenter;
        private NodeConfig _config;
        private GraphManager _graphManager;
        private SystemManager _systemManager;
        private bool _isConstructed = false;
        private Connection _pendingModel = null;
        private ConnectionLabelPresenter _label;
        private Vector2 _dragOffset;


        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }
        

        [Inject]
        public void Construct(SystemManager systemManager, NodeConfig config, GraphManager graphManager)
        {
            Debug.Log("ENTER: ConnectionPresenter Construct");
            _systemManager = systemManager;
            _config = config;
            _graphManager = graphManager;
            _isConstructed = true;
            if (_pendingModel != null)
            {
                Initialize(_pendingModel);
                _pendingModel = null;
            }
        }

        public void Initialize(Connection model)
        {
            if (!_isConstructed)
            {
                _pendingModel = model;
                Debug.LogWarning("ConnectionPresenter not constructed yet, setting pending model");
                return;
            }
    
            _model = model;
            // Eğer _model.line henüz oluşturulmamışsa oluşturun.
            if (_model.line == null)
            {
                _model.line = new Line();
                Debug.Log("Yeni Line nesnesi oluşturuldu.");
            }
    
            Debug.Log($"Connection initialized with model ID: {model.ID}");
            // UpdateLine(); // Eğer gerekliyse burada çağırabilirsiniz.
        }

        private void Update()
        {
            if (!_isConstructed) return;
            UpdateLine();
            _model.line.Draw(_graphManager.LineRenderer);
        }
        



        public void UpdateLine()
        {
    
            // Eğer tüm referanslar null değilse, devam edebilirsiniz.
            if (_graphManager != null)
            {
                Vector3[] linePoints = LTGUtility.WorldToScreenPointsForRenderMode(_graphManager, new Vector3[]
                {
                    _model.SourcePort.transform.position,
                    _model.SourcePort.Model.ControlPoint.Position,
                    _model.TargetPort.Model.ControlPoint.Position,
                    _model.TargetPort.transform.position
                });
    
                Vector2[] newPoints =
                    LineUtils.ConvertLinePointsToCurve(linePoints, Connection.CurveStyleType.Soft_Z_Shape);
    
                Model.line.SetPoints(newPoints);
            }
        }






        private PortPresenter FindPortPresenter(Port port)
        {
            foreach (var node in FindObjectsOfType<BaseNodePresenter>())
            {
                var portPresenter = node.GetPortPresenterByModel(port);
                if (portPresenter != null)
                    return portPresenter;
            }

            return null;
        }

        public void Select()
        {
            if (EnableSelect)
            {
                _model.line.color = _config.selectedColor;
                if (!_systemManager.selectedElements.Contains(this))
                {
                    _systemManager.selectedElements.Add(this);
                    _systemManager.LTGEvents.TriggerEvent(NodeSystem.Events.LTGEventType.OnElementSelected, this);
                }
            }
        }

        public void Unselect()
        {
            if (EnableSelect)
            {
                _model.line.color = _config.defaultNodeColor;
                if (_systemManager.selectedElements.Contains(this))
                {
                    _systemManager.selectedElements.Remove(this);
                    _systemManager.LTGEvents.TriggerEvent(NodeSystem.Events.LTGEventType.OnElementUnselected, this);
                }
            }
        }

        public void OnPointerDown()
        {
            if (!_systemManager.selectedElements.Contains(this))
            {
                Select();
            }
            else
            {
                Unselect();
            }
        }

        public void OnPointerUp()
        {
            
        }

        public string ID { get; set; }
        public int Priority { get; }
        public void Remove()
        { 
            Unselect();
            _graphManager.ConnectionPresenters.Remove(this);
            Model.SourcePort.UpdateIcon();
            Model.TargetPort.UpdateIcon();

            if (_systemManager.clickedElement == this as IElement)
            {
                _systemManager.clickedElement = null;
            }
            Destroy(gameObject);
        }

        public bool EnableDrag { get; set; } = true;
        private bool _dragStart = true;
        public void OnDrag(Vector2 position)
        {
            if (EnableDrag)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(_graphManager.CanvasRectTransform, position, null, out var mousePos);
                transform.localPosition = mousePos + _dragOffset;
            }
        }
        public void OnBeginDrag()
        {
            if (EnableDrag)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(_graphManager.CanvasRectTransform, Input.mousePosition, null, out var mousePos);
                _dragOffset = (Vector2)transform.localPosition - mousePos;
                Select();
            }
        }
        public void OnEndDrag() => throw new System.NotImplementedException();
        public bool EnableHover { get; set; } = true;
        public void OnPointerHoverEnter()
        {
            if (EnableHover)
            {
                _model.line.color = _config.hoverColor;
            }
        }
        public void OnPointerHoverExit()
        {
            if (EnableHover)
            {
                if (_systemManager.selectedElements.Contains(this))
                {
                    _model.line.color = _selectedColor;
                }
                else
                {
                    _model.line.color = _config.defaultNodeColor;
                }
            }
        }

        public void SetLabel(string text)
        {
            if (_label == null)
            {
                var labelGO = new GameObject("Connection Label");
                labelGO.transform.SetParent(transform);
                _label = labelGO.AddComponent<ConnectionLabelPresenter>();
            }

            _label.SetText(text);
            UpdateLine();
        }

        public void RemoveLabel()
        {
            if (_label != null)
            {
                Destroy(_label.gameObject);
                _label = null;
            }
        }
    }
}