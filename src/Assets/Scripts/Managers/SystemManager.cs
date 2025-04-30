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
using Zenject;
using EventType = NodeSystem.Events.LTGEventType;

namespace Managers
{
    [DefaultExecutionOrder(-20)]  // En önce çalışsın
    public class SystemManager : MonoBehaviour
    {
        [SerializeField] bool _cacheRaycasters = true;
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
        public  List<ISelectable> selectedElements = new List<ISelectable>();
        public  IElement clickedElement;
        public  IElement hoverElement;

        static EventManager<IElement> _ltgEvents;
        public  EventManager<IElement> LTGEvents
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
                    }
                    else
                    {
                        Debug.Log("3D Object selection cleared");
                    }
                }
                
                _selected3DObject = value;
            }
        }

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

        public  List<GraphManager> graphManagers = new List<GraphManager>();

        void Start()
        {

        }

        void Update()
        {
            e_OnUpdate.Invoke();
        }

        UnityEvent e_OnUpdate = new UnityEvent();
        static List<UnityAction> actions = new List<UnityAction>();

        public  void AddToUpdate(UnityAction action)
        {
            if (!actions.Contains(action))
            {
                e_OnUpdate.AddListener(action);
                actions.Add(action);
            }
        }
        public  void RemoveFromUpdate(UnityAction action)
        {
            if (actions.Contains(action))
            {
                e_OnUpdate.RemoveListener(action);
                actions.Remove(action);
            }
        }
    }
} 