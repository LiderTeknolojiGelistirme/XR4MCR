using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using Viroo.Interactions.Grab;
using Managers;

namespace Presenters.NodePresenters
{
    public class TouchNodePresenter : BaseNodePresenter
    {
        private XRBaseInteractable _simpleInteractable;
        public Button selectObjectButton;
        public TMP_InputField selectObjectInputField;

        private void Awake()
        {
            if (selectObjectButton != null)
            {
                selectObjectButton.onClick.AddListener(OnSelectObject);
            }
            
            // Log: TouchNodePresenter oluşturuldu
            LogManager.LogSuccess("TouchNodePresenter initialized: " + gameObject.name);
        }

        private void OnDisable()
        {
            if (selectObjectButton != null)
            {
                selectObjectButton.onClick.RemoveAllListeners();
            }
            
            // Log: TouchNodePresenter devre dışı bırakıldı
            LogManager.Log("TouchNodePresenter disabled: " + gameObject.name, Color.gray);
        }

        public override void ActivateNode()
        {
            base.ActivateNode();
            
            // Log: Nod aktif edildi
            LogManager.LogScenario("TouchNode activated: " + gameObject.name);
        }

        public override void StartNode()
        {
            base.StartNode();
            
            // Log: Nod başlatıldı
            LogManager.LogScenario("TouchNode started: " + gameObject.name);
        }

        public override void CompleteNode()
        {
            // Log: Nod tamamlandı
            LogManager.LogSuccess("TouchNode completed: " + gameObject.name);
            
            base.CompleteNode();
        }

        public override void DeactivateNode()
        {
            // Log: Nod deaktive edildi
            LogManager.LogScenario("TouchNode deactivated: " + gameObject.name);
            
            base.DeactivateNode();
        }

        public override void Play()
        {
            base.Play();
            if (_simpleInteractable != null)
            {
                // Log: Hover durumunu kontrol et
                if (_simpleInteractable.isHovered)
                {
                    LogManager.LogInteraction("Object is being hovered: " + _simpleInteractable.gameObject.name);
                    Debug.Log("Selected");
                    CompleteNode();
                }
                else
                {
                    // Sadece Debug.Log ile göster, panel'i çok doldurmasın
                    //Debug.Log("Object is not hovered yet: " + _simpleInteractable.gameObject.name);
                }
            }
        }
        
        public void OnSelectObject()
        {
            try
            {
                // SystemManager.Selected3DObject null kontrolü
                if (SystemManager.Selected3DObject == null)
                {
                    LogManager.LogError("Error selecting object: No object selected");
                    return;
                }
                
                // XRBaseInteractable bileşenini al
                _simpleInteractable = SystemManager.Selected3DObject.GetComponent<XRBaseInteractable>();
                
                // XRBaseInteractable null kontrolü
                if (_simpleInteractable == null)
                {
                    LogManager.LogError("Error selecting object: Selected object does not have XRBaseInteractable component");
                    return;
                }
                
                // Input field kontrolü ve güncelleme
                if (selectObjectInputField != null)
                {
                    selectObjectInputField.text = _simpleInteractable.gameObject.name;
                }
                
                // Log: Nesne seçildi
                LogManager.LogInteraction("Object selected: " + _simpleInteractable.gameObject.name);
            }
            catch (Exception e)
            {
                // Log: Hata oluştu
                LogManager.LogError("Error selecting object: " + e.Message);
                Debug.LogError(e.Message);
            }
        }
    }
}