using System;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Models.Nodes;
using Managers;
using System.Collections.Generic;
using UnityEditor;
using Unity.VisualScripting;
using UnityEngine.XR.Interaction.Toolkit.Samples.SpatialKeyboard;
using Zenject;
using Enums;

namespace Presenters.NodePresenters
{
    public class DescriptionActionNodePresenter : ActionNodePresenter
    {
        //[Inject] XRKeyboard XRKeyboard;
        [SerializeField] private TMP_InputField narrativeText;

        [SerializeField] private FlexibleColorPicker fcp_text;
        [SerializeField] private FlexibleColorPicker fcp_bg;

        [SerializeField] private Button btn_txt;
        [SerializeField] private Button btn_bg;
        [SerializeField] private TMP_InputField durationInputField;
        [SerializeField] private Button durationIncreaseButton;
        [SerializeField] private Button durationDecreaseButton;
        
        //[SerializeField] private XRKeyboardDisplay XRKeyboardDisplay;

        public NotifierCanvas nc;
        private Camera cam;
        private GameObject centerEyeAnchor;

        // Model'e kolay erişim için cast property
        private DescriptionActionNode DescriptionModel => Model as DescriptionActionNode;

        IEnumerator Start()
        {
            // Keep looking for the camera until it's found
            while (cam == null)
            {
                cam = Camera.main;
                if (cam == null)
                {
                    yield return new WaitForSeconds(0.1f);
                }
                else
                {
                    centerEyeAnchor = cam.gameObject;
                    nc = centerEyeAnchor.GetComponentInChildren<NotifierCanvas>();

                    break;
                }
            }
        }

        protected override void Awake()
        {
            base.Awake();
            //XRKeyboardDisplay.keyboard = XRKeyboard;
            SetActionType(NodeType.DescriptionActionNode);

            if (btn_txt != null)
            {
                btn_txt.onClick.AddListener(OnButtonClick_Text);
            }

            if (btn_bg != null)
            {
                btn_bg.onClick.AddListener(OnButtonClick_BackGround);
            }

            durationIncreaseButton.onClick.AddListener(OnIncreaseDuration);
            durationDecreaseButton.onClick.AddListener(OnDecreaseDuration);
            
            // MVP: UI değişikliklerini modele anında yansıt
            SetupUIToModelBinding();
        }

        private void SetupUIToModelBinding()
        {
            // Text değişikliklerini modele yansıt
            if (narrativeText != null)
            {
                narrativeText.onValueChanged.AddListener(OnNarrativeTextChanged);
            }

            // Duration input field değişikliklerini modele yansıt
            if (durationInputField != null)
            {
                durationInputField.onValueChanged.AddListener(OnDurationInputChanged);
            }

            // Color picker değişikliklerini modele yansıt
            if (fcp_text != null)
            {
                // FlexibleColorPicker'da onColorChange event'i varsa kullan
                // Yoksa Update() metodunda sürekli kontrol et
            }

            if (fcp_bg != null)
            {
                // FlexibleColorPicker'da onColorChange event'i varsa kullan
                // Yoksa Update() metodunda sürekli kontrol et
            }
        }

        protected override void Update()
        {
            base.Update();
            
            // MVP: Color picker değişikliklerini sürekli kontrol et ve modele yansıt
            if (DescriptionModel != null)
            {
                if (fcp_text != null && DescriptionModel.TextColor != fcp_text.color)
                {
                    DescriptionModel.TextColor = fcp_text.color;
                    LogManager.LogInteraction($"Text color updated: {fcp_text.color}");
                }

                if (fcp_bg != null && DescriptionModel.BackgroundColor != fcp_bg.color)
                {
                    DescriptionModel.BackgroundColor = fcp_bg.color;
                    LogManager.LogInteraction($"Background color updated: {fcp_bg.color}");
                }
            }
        }

        private void OnNarrativeTextChanged(string newText)
        {
            if (DescriptionModel != null)
            {
                DescriptionModel.MessageText = newText;
                LogManager.LogInteraction($"Narrative text updated: {newText}");
            }
        }

        private void OnDurationInputChanged(string newDurationStr)
        {
            if (DescriptionModel != null && int.TryParse(newDurationStr, out int newDuration))
            {
                if (newDuration >= 0)
                {
                    DescriptionModel.DisplayDuration = newDuration;
                    LogManager.LogInteraction($"Duration updated via input: {newDuration}");
                }
            }
        }

        /// <summary>
        /// Model verilerini UI'ya senkronize eder (Load işlemi sonrası)
        /// </summary>
        public override void SyncModelToUI()
        {
            base.SyncModelToUI(); // Base sınıfın sync metodunu çağır
            
            if (DescriptionModel == null)
            {
                LogManager.LogWarning("DescriptionActionNode model is null, cannot sync to UI");
                return;
            }

            // Text içeriğini sync et
            if (narrativeText != null)
            {
                narrativeText.text = DescriptionModel.MessageText ?? "";
            }

            // Duration'ı sync et
            if (durationInputField != null)
            {
                durationInputField.text = DescriptionModel.DisplayDuration.ToString();
            }

            // Text color'ı sync et
            if (fcp_text != null)
            {
                fcp_text.color = DescriptionModel.TextColor;
            }

            // Background color'ı sync et
            if (fcp_bg != null)
            {
                fcp_bg.color = DescriptionModel.BackgroundColor;
            }

            LogManager.LogSuccess($"DescriptionActionNode UI synced from model: Text='{DescriptionModel.MessageText}', Duration={DescriptionModel.DisplayDuration}");
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (btn_txt != null)
            {
                btn_txt.onClick.RemoveAllListeners();
            }

            if (btn_bg != null)
            {
                btn_bg.onClick.RemoveAllListeners();
            }
            durationIncreaseButton.onClick.RemoveAllListeners();
            durationDecreaseButton.onClick.RemoveAllListeners();
            
            // UI event'lerini temizle
            if (narrativeText != null)
            {
                narrativeText.onValueChanged.RemoveAllListeners();
            }
            
            if (durationInputField != null)
            {
                durationInputField.onValueChanged.RemoveAllListeners();
            }
        }

        protected override void PerformAction()
        {
            base.PerformAction();

            // Model'den değerleri al ve kullan
            if (DescriptionModel != null)
            {
                nc.descriptionPanel.GetComponentInChildren<TMP_Text>().text = DescriptionModel.MessageText;
                nc.descriptionPanel.GetComponentInChildren<TMP_Text>().color = DescriptionModel.TextColor;
                nc.descriptionPanel.GetComponent<Image>().color = DescriptionModel.BackgroundColor;

                nc.ShowDescriptionPanel();
                nc.HideDescriptionPanel(DescriptionModel.DisplayDuration);
            }
        }

        private void OnButtonClick_Text()
        {
            LogManager.LogInteraction("Text color picker button clicked");
            
            if (fcp_text.gameObject.activeSelf == true)
            {
                fcp_text.gameObject.SetActive(false);
                LogManager.LogSuccess("Text color picker closed");
            }
            else
            {
                fcp_text.gameObject.SetActive(true);
                LogManager.LogSuccess("Text color picker opened");
            }
        }

        private void OnButtonClick_BackGround()
        {
            LogManager.LogInteraction("Background color picker button clicked");
            
            if (fcp_bg.gameObject.activeSelf == true)
            {
                fcp_bg.gameObject.SetActive(false);
                LogManager.LogSuccess("Background color picker closed");
            }
            else
            {
                fcp_bg.gameObject.SetActive(true);
                LogManager.LogSuccess("Background color picker opened");
            }
        }

        public void PerformRemove()
        {
            nc.descriptionPanel.SetActive(false);
        }
        
        private void OnIncreaseDuration()
        {
            LogManager.LogInteraction("Increase duration button clicked");
            
            if (DescriptionModel != null)
            {
                DescriptionModel.DisplayDuration++;
                
                // UI'yı güncelle
                if (durationInputField != null)
                {
                    durationInputField.text = DescriptionModel.DisplayDuration.ToString();
                }
                
                LogManager.LogSuccess($"Duration increased: {DescriptionModel.DisplayDuration}");
            }
        }

        private void OnDecreaseDuration()
        {
            LogManager.LogInteraction("Decrease duration button clicked");
            
            if (DescriptionModel != null)
            {
                if (DescriptionModel.DisplayDuration > 0)
                {
                    DescriptionModel.DisplayDuration--;
                    
                    // UI'yı güncelle
                    if (durationInputField != null)
                    {
                        durationInputField.text = DescriptionModel.DisplayDuration.ToString();
                    }
                    
                    LogManager.LogSuccess($"Duration decreased: {DescriptionModel.DisplayDuration}");
                }
                else
                {
                    LogManager.LogWarning("Duration cannot be less than 0");
                }
            }
        }

        #region Edit Mode Functions

        /// <summary>
        /// Description action node için düzenleme modunu açar.
        /// Color picker'lar, duration butonları ve input field'lar gösterilir.
        /// </summary>
        public override void EditModeOn()
        {
            base.EditModeOn(); // Base class'ın keyboardDisplay'ini göster

            // Color picker'ları göster
            if (fcp_text != null && fcp_text.gameObject != null)
            {
                fcp_text.gameObject.SetActive(true);
            }

            if (fcp_bg != null && fcp_bg.gameObject != null)
            {
                fcp_bg.gameObject.SetActive(true);
            }

            // Color picker butonlarını göster
            if (btn_txt != null && btn_txt.gameObject != null)
            {
                btn_txt.gameObject.SetActive(true);
            }

            if (btn_bg != null && btn_bg.gameObject != null)
            {
                btn_bg.gameObject.SetActive(true);
            }

            // Duration kontrollerini göster
            if (durationInputField != null && durationInputField.gameObject != null)
            {
                durationInputField.gameObject.SetActive(true);
            }

            if (durationIncreaseButton != null && durationIncreaseButton.gameObject != null)
            {
                durationIncreaseButton.gameObject.SetActive(true);
            }

            if (durationDecreaseButton != null && durationDecreaseButton.gameObject != null)
            {
                durationDecreaseButton.gameObject.SetActive(true);
            }

            // Narrative text input'u göster
            if (narrativeText != null && narrativeText.gameObject != null)
            {
                narrativeText.gameObject.SetActive(true);
            }

            LogManager.LogSuccess($"EditModeOn: Description action node editing UI shown for: {Model.Title}");
        }

        /// <summary>
        /// Description action node için düzenleme modunu kapatır.
        /// Color picker'lar, duration butonları ve input field'lar gizlenir.
        /// </summary>
        public override void EditModeOff()
        {
            base.EditModeOff(); // Base class'ın keyboardDisplay'ini gizle

            // Color picker'ları gizle
            if (fcp_text != null && fcp_text.gameObject != null)
            {
                fcp_text.gameObject.SetActive(false);
            }

            if (fcp_bg != null && fcp_bg.gameObject != null)
            {
                fcp_bg.gameObject.SetActive(false);
            }

            // Color picker butonlarını gizle
            if (btn_txt != null && btn_txt.gameObject != null)
            {
                btn_txt.gameObject.SetActive(false);
            }

            if (btn_bg != null && btn_bg.gameObject != null)
            {
                btn_bg.gameObject.SetActive(false);
            }

            // Duration kontrollerini gizle
            if (durationInputField != null && durationInputField.gameObject != null)
            {
                durationInputField.gameObject.SetActive(false);
            }

            if (durationIncreaseButton != null && durationIncreaseButton.gameObject != null)
            {
                durationIncreaseButton.gameObject.SetActive(false);
            }

            if (durationDecreaseButton != null && durationDecreaseButton.gameObject != null)
            {
                durationDecreaseButton.gameObject.SetActive(false);
            }

            // Narrative text input'u gizle
            if (narrativeText != null && narrativeText.gameObject != null)
            {
                narrativeText.gameObject.SetActive(false);
            }

            LogManager.LogSuccess($"EditModeOff: Description action node editing UI hidden for: {Model.Title}");
        }

        #endregion
    }
}