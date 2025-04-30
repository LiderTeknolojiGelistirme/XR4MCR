using System;
using _3rd_Party.Outline;
using Interfaces;
using Managers;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using Zenject;
using Outline = _3rd_Party.Outline.Outline;

namespace Helpers
{
    public class InteractionHelper : MonoBehaviour, ISelectable
    {
        [Inject] SystemManager _systemManager;
        private XRBaseInteractable _xrBaseInteractable;
        private Outline _outline;
        public bool EnableSelect { get; set; }

        private void Awake()
        {
            _xrBaseInteractable = GetComponent<XRBaseInteractable>();
            _outline = GetComponent<Outline>();
            
            if (_xrBaseInteractable != null)
            {
                _xrBaseInteractable.selectEntered.AddListener(_ => Select());
            }
        }

        public void Remove()
        {
            throw new System.NotImplementedException();
        }

        public void Select()
        {
            Debug.Log("Select");
            
            // SystemManager kontrolü
            if (_systemManager == null)
            {
                Debug.LogWarning("SystemManager is null in InteractionHelper");
                return;
            }
            
            // Önceden seçili nesneyi temizle
            if (_systemManager.Selected3DObject != null)
            {
                try
                {
                    InteractionHelper previousHelper = _systemManager.Selected3DObject.GetComponent<InteractionHelper>();
                    if (previousHelper != null)
                    {
                        previousHelper.Unselect();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("Error while unselecting previous object: " + e.Message);
                }
            }
            
            // Bu nesneyi seçili olarak işaretle
            _systemManager.Selected3DObject = gameObject;
            
            // Outline'ı etkinleştir
            if (_outline != null)
            {
                _outline.enabled = true;
                _outline.OutlineColor = Color.white;
            }
        }

        public void Unselect()
        {
            // SystemManager kontrolü
            if (_systemManager == null)
            {
                Debug.LogWarning("SystemManager is null in InteractionHelper");
                return;
            }
            
            // Seçili nesneyi temizle
            if (_systemManager.Selected3DObject == gameObject)
            {
                _systemManager.Selected3DObject = null;
            }
            
            // Outline'ı devre dışı bırak
            if (_outline != null)
            {
                _outline.enabled = false;
                _outline.OutlineColor = Color.red;
            }
        }
    }
}