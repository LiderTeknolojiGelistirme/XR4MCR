using System;
using System.Collections.Generic;
using System.Linq;
using Interfaces;
using NodeSystem.Events;
using Presenters;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Samples.SpatialKeyboard;
using Zenject;
using EventType = NodeSystem.Events.LTGEventType;
using RuntimeGizmos;
using System.Collections;

namespace Managers
{
    [DefaultExecutionOrder(-20)] // En önce çalışsın
    public class SystemManager : MonoBehaviour
    {
        [Inject] NodeConfig _nodeConfig;
        [SerializeField] bool _cacheRaycasters = true;

        private Camera _cam;
        private bool _searchFinished = false;

        private GameObject _tutorialLeft, _tutorialRight;

        public TransformGizmo _transformGizmo;
        public int maxUndoCount = 100;

        // LogManager injection
        private LogManager _logManager;

        [Inject]
        public void Construct(LogManager logManager)
        {
            _logManager = logManager;
            Debug.Log("SystemManager: LogManager injected successfully");
        }

        public bool CacheRaycasters
        {
            get => _cacheRaycasters;
            set
            {
                raycasterList = new List<GraphicRaycaster>();
                if (value)
                {
                    List<GraphicRaycaster> graphicRaycasters = FindObjectsOfType<GraphicRaycaster>().ToList();
                    foreach (GraphicRaycaster graphicRaycaster in graphicRaycasters)
                    {
                        if (graphicRaycaster.GetComponent<GraphManager>() != null)
                        {
                            raycasterList.Add(graphicRaycaster);
                            break;
                        }
                    }
                }

                _cacheRaycasters = value;
            }
        }

        public List<GraphicRaycaster> raycasterList = new List<GraphicRaycaster>();

        // list of selected elements, used for single or multi selection
        public List<ISelectable> selectedElements = new List<ISelectable>();
        public IElement clickedElement;
        public IElement hoverElement;

        static EventManager<IElement> _ltgEvents;

        public EventManager<IElement> LTGEvents
        {
            get
            {
                if (_ltgEvents == null)
                {
                    _ltgEvents = new EventManager<IElement>();
                }

                return _ltgEvents;
            }
        }

        private GameObject _selected3DObject;
        [Inject] private XRKeyboard _keyboard;

        // 3D nesnesi seçimi için güvenli Property
        public GameObject Selected3DObject
        {
            get { return _selected3DObject; }
            set
            {
                // Değer değiştiğinde debug bilgisi
                if (_selected3DObject != value)
                {
                    if (value != null)
                    {
                        Debug.Log($"3D Object selected: {value.name}");
                        LogManager.Log($"SYSTEM: 3D Object selected: {value.name}");
                    }
                    else
                    {
                        Debug.Log("3D Object selection cleared");
                        LogManager.Log("SYSTEM: 3D Object selection cleared");
                    }
                }

                _selected3DObject = value;
            }
        }

        #region Log Control Methods

        /// <summary>
        /// LogManager'ın dosyaya yazmayı etkinleştir
        /// </summary>
        public void EnableFileLogging()
        {
            LogManager.EnableFileLogging();
            Debug.Log("SystemManager: File logging enabled via SystemManager");
        }

        /// <summary>
        /// LogManager'ın dosyaya yazmayı devre dışı bırak
        /// </summary>
        public void DisableFileLogging()
        {
            LogManager.DisableFileLogging();
            Debug.Log("SystemManager: File logging disabled via SystemManager");
        }

        /// <summary>
        /// LogManager'ın log dosyasını temizle
        /// </summary>
        public void ClearLogFile()
        {
            LogManager.ClearLogFile();
            Debug.Log("SystemManager: Log file cleared via SystemManager");
        }

        /// <summary>
        /// LogManager'ın dosyaya yazma durumunu döndür
        /// </summary>
        public bool IsFileLoggingEnabled()
        {
            return LogManager.enableFileLogging;
        }

        /// <summary>
        /// LogManager'ın dosyaya yazma durumunu toggle et
        /// </summary>
        public void ToggleFileLogging()
        {
            if (LogManager.enableFileLogging)
            {
                DisableFileLogging();
            }
            else
            {
                EnableFileLogging();
            }
        }
        #endregion

        void OnEnable()
        {
            CacheRaycasters = _cacheRaycasters;

            selectedElements = new List<ISelectable>();

            InputManager inputManager = FindObjectOfType<InputManager>();
            if (!inputManager)
            {
                gameObject.AddComponent<XRInputManager>();
            }
        }

        public List<GraphManager> graphManagers = new List<GraphManager>();

         IEnumerator Start()
        {
            _cam = Camera.main;
            if (_cam == null)
            {
                yield return new WaitForSeconds(0.1f);
                _cam = Camera.main; // Tekrar dene
            }
            
            _keyboard.Close(false);
            
            // TransformGizmo'yu bul
            yield return StartCoroutine(FindTransformGizmo());
        }

        IEnumerator FindTransformGizmo()
        {
            int attempts = 0;
            const int maxAttempts = 50; // 5 saniye bekle (50 * 0.1s)
            
            while (_transformGizmo == null && attempts < maxAttempts)
            {
                // Önce Camera.main'de ara
                if (_cam != null)
                {
                    _transformGizmo = _cam.GetComponent<TransformGizmo>();
                }
                
                // Eğer Camera'da yoksa, sahnede ara
                if (_transformGizmo == null)
                {
                    _transformGizmo = FindObjectOfType<TransformGizmo>();
                }
                
                if (_transformGizmo != null)
                {
                    Debug.Log($"TransformGizmo bulundu: {_transformGizmo.gameObject.name}");
                    break;
                }
                
                attempts++;
                yield return new WaitForSeconds(0.1f);
            }
            
            if (_transformGizmo == null)
            {
                Debug.LogWarning("TransformGizmo bulunamadı! Maksimum deneme sayısına ulaşıldı.");
            }
        }


        void Update()
        {
            e_OnUpdate.Invoke();
            if (!_searchFinished)
            {
                var go = GameObject.Find("RightHandAnchor");
                if (go != null)
                {
                    _tutorialRight = Instantiate(_nodeConfig.rightControllerTutorialPrefab, go.transform);
                    _tutorialLeft = Instantiate(_nodeConfig.leftControllerTutorialPrefab,
                        GameObject.Find("LeftHandAnchor").transform);
                    _tutorialLeft.SetActive(false);
                    _tutorialRight.SetActive(false);
                    _searchFinished = true;
                }
            }
        }

        UnityEvent e_OnUpdate = new UnityEvent();
        static List<UnityAction> actions = new List<UnityAction>();

        public void OpenOrCloseTutorial()
        {
            if (!_tutorialLeft.activeInHierarchy)
            {
                _tutorialLeft.SetActive(true);
                _tutorialRight.SetActive(true);
            }
            else
            {
                _tutorialLeft.SetActive(false);
                _tutorialRight.SetActive(false);
            }
        }

        public void OnClickUndo()
        {
            if (maxUndoCount != UndoRedoManager.maxUndoStored)
            {
                UndoRedoManager.maxUndoStored = maxUndoCount;
            }
            UndoRedoManager.Undo();
        }
        
        public void OnClickRedo()
        {
            UndoRedoManager.Redo();
        }


        public void AddToUpdate(UnityAction action)
        {
            if (!actions.Contains(action))
            {
                e_OnUpdate.AddListener(action);
                actions.Add(action);
            }
        }

        public void RemoveFromUpdate(UnityAction action)
        {
            if (actions.Contains(action))
            {
                e_OnUpdate.RemoveListener(action);
                actions.Remove(action);
            }
        }

         // TransformGizmo kontrol metotları
        public void SetTransformGizmoToPosition()
        {
            if (_transformGizmo != null)
            {
                _transformGizmo.SetTransformTypeMove();
                Debug.Log("Transform Gizmo Position moduna ayarlandı");
                LogManager.Log("SYSTEM: Transform Gizmo set to Position mode");
            }
            else
            {
                Debug.LogWarning("TransformGizmo referansı bulunamadı!");
                LogManager.LogError("SYSTEM: TransformGizmo reference not found!");
            }
        }

        public void SetTransformGizmoToRotation()
        {
            if (_transformGizmo != null)
            {
                _transformGizmo.SetTransformTypeRotate();
                Debug.Log("Transform Gizmo Rotation moduna ayarlandı");
                LogManager.Log("SYSTEM: Transform Gizmo set to Rotation mode");
            }
            else
            {
                Debug.LogWarning("TransformGizmo referansı bulunamadı!");
                LogManager.LogError("SYSTEM: TransformGizmo reference not found!");
            }
        }

        public void SetTransformGizmoToScale()
        {
            if (_transformGizmo != null)
            {
                _transformGizmo.SetTransformTypeScale();
                Debug.Log("Transform Gizmo Scale moduna ayarlandı");
                LogManager.Log("SYSTEM: Transform Gizmo set to Scale mode");
            }
            else
            {
                Debug.LogWarning("TransformGizmo referansı bulunamadı!");
                LogManager.LogError("SYSTEM: TransformGizmo reference not found!");
            }
        }

        public void SetSelectedObjectToScenarioArea(GameObject scenarioAreaObject)
        {
            if (scenarioAreaObject != null)
            {
                Selected3DObject = scenarioAreaObject;
                _transformGizmo.SetTransformTypeMove();
                Debug.Log($"ScenarioArea seçildi: {scenarioAreaObject.name}");
            }
            else
            {
                Debug.LogWarning("ScenarioArea GameObject referansı null!");
            }
        }
    }
}