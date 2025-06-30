using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Managers;
using Models.Nodes;
using UnityEngine.XR.Interaction.Toolkit.Samples.SpatialKeyboard;

namespace Presenters.NodePresenters
{
    public class WaitForNextNodePresenter : BaseNodePresenter
    {
        public Button increaseButton;
        public Button decreaseButton;
        public TMP_InputField inputField;
        public TMP_Text statusText;

        // Model'e cast etmek için property
        private Models.Nodes.WaitForNextNode WaitForNextNodeModel
        {
            get => Model as Models.Nodes.WaitForNextNode;
        }

        private void Awake()
        {
            increaseButton.onClick.AddListener(OnIncreaseTime);  // Artırma butonu için dinleyici ayarla
            decreaseButton.onClick.AddListener(OnDecreaseTime);  // Azaltma butonu için dinleyici ayarla

            // Input field değişikliklerini dinle
            inputField.onEndEdit.AddListener(OnInputFieldChanged);
        }

        private void Start()
        {
            // Description'ı sadece boşsa set et (Load'dan gelen değeri korumak için)
            if (string.IsNullOrEmpty(Model.Description))
            {
                Model.Description = "Wait for the next node to be activated";
            }
        }

        private void OnDisable()
        {
            increaseButton.onClick.RemoveAllListeners();
            decreaseButton.onClick.RemoveAllListeners();
            inputField.onEndEdit.RemoveAllListeners();
        }

        protected override void Update()
        {
            base.Update();
            
            if (WaitForNextNodeModel != null)
            {
                inputField.text = Mathf.RoundToInt(WaitForNextNodeModel.WaitTimeInSeconds).ToString();
                
                if (WaitForNextNodeModel.IsTimerRunning)
                {
                    WaitForNextNodeModel.WaitTimeInSeconds -= Time.deltaTime;

                    float remainingTime = Mathf.Max(0, WaitForNextNodeModel.WaitTimeInSeconds);
                    statusText.text = $"Waiting for next node... ({Mathf.RoundToInt(remainingTime)} sn)";
                }
            }
        }

        public override void ActivateNode()
        {
            base.ActivateNode();
            if (WaitForNextNodeModel != null)
            {
                WaitForNextNodeModel.InitialTimeInSeconds = Int32.Parse(inputField.text.Trim());
                WaitForNextNodeModel.WaitTimeInSeconds = WaitForNextNodeModel.InitialTimeInSeconds;
                WaitForNextNodeModel.IsTimerRunning = false;
                inputField.text = WaitForNextNodeModel.WaitTimeInSeconds.ToString();
                statusText.text = "Ready";
            }
        }

        public override void StartNode()
        {
            base.StartNode();
            if (WaitForNextNodeModel != null)
            {
                WaitForNextNodeModel.IsTimerRunning = true;
                statusText.text = $"Waiting for next node... ({Mathf.RoundToInt(WaitForNextNodeModel.WaitTimeInSeconds)} sn)";
            }
        }

        public override void CompleteNode()
        {
            base.CompleteNode();
            statusText.text = "Node is completed";
            if (WaitForNextNodeModel != null)
            {
                WaitForNextNodeModel.IsTimerRunning = false;
            }
        }
        
        public void OnNextNode()
        {
            if (WaitForNextNodeModel != null)
            {
                WaitForNextNodeModel.IsTimerRunning = false;
                WaitForNextNodeModel.WaitTimeInSeconds = WaitForNextNodeModel.InitialTimeInSeconds;
            }
            CompleteNode();
            Debug.Log("Is passing next node");
        }

        public void OnIncreaseTime()
        {
            LogManager.LogInteraction("Increase wait time button clicked");
            
            if (WaitForNextNodeModel != null)
            {
                WaitForNextNodeModel.WaitTimeInSeconds += 1f;
                WaitForNextNodeModel.InitialTimeInSeconds = WaitForNextNodeModel.WaitTimeInSeconds;
                inputField.text = Mathf.RoundToInt(WaitForNextNodeModel.WaitTimeInSeconds).ToString();
                
                LogManager.LogSuccess($"Wait time increased: {WaitForNextNodeModel.WaitTimeInSeconds}s (MVP)");
            }
        }

        public void OnDecreaseTime()
        {
            LogManager.LogInteraction("Decrease wait time button clicked");
            
            if (WaitForNextNodeModel != null)
            {
                WaitForNextNodeModel.WaitTimeInSeconds -= 1f;
                if (WaitForNextNodeModel.WaitTimeInSeconds < 1f)
                {
                    WaitForNextNodeModel.WaitTimeInSeconds = 1f;
                }
                WaitForNextNodeModel.InitialTimeInSeconds = WaitForNextNodeModel.WaitTimeInSeconds;
                inputField.text = Mathf.RoundToInt(WaitForNextNodeModel.WaitTimeInSeconds).ToString();
                
                LogManager.LogSuccess($"Wait time decreased: {WaitForNextNodeModel.WaitTimeInSeconds}s (MVP)");
            }
        }

        private void OnInputFieldChanged(string value)
        {
            LogManager.LogInteraction($"Wait time input field changed: {value}");
            
            if (int.TryParse(value, out int seconds) && WaitForNextNodeModel != null)
            {
                WaitForNextNodeModel.WaitTimeInSeconds = seconds;
                WaitForNextNodeModel.InitialTimeInSeconds = seconds;
                LogManager.LogSuccess($"Wait time set to: {WaitForNextNodeModel.WaitTimeInSeconds}s (MVP - model immediately updated)");
            }
            else if (WaitForNextNodeModel != null)
            {
                inputField.text = Mathf.RoundToInt(WaitForNextNodeModel.WaitTimeInSeconds).ToString();
                LogManager.LogWarning("Invalid wait time input, reverting to previous value");
            }
        }

        public override void Play()
        {
            base.Play();

            if (WaitForNextNodeModel != null && WaitForNextNodeModel.WaitTimeInSeconds <= 0)
            {
                WaitForNextNodeModel.IsTimerRunning = false;
                OnNextNode();
            }
        }

        /// <summary>
        /// Model verilerini UI'ya senkronize eder (Load işlemi sonrası)
        /// </summary>
        public override void SyncModelToUI()
        {
            base.SyncModelToUI();
            
            if (WaitForNextNodeModel != null)
            {
                // Model'den UI'ya verileri aktar
                inputField.text = Mathf.RoundToInt(WaitForNextNodeModel.WaitTimeInSeconds).ToString();
                
                // Status text'i modele göre ayarla
                if (WaitForNextNodeModel.IsTimerRunning)
                {
                    statusText.text = $"Waiting for next node... ({Mathf.RoundToInt(WaitForNextNodeModel.WaitTimeInSeconds)} sn)";
                }
                else if (Model.IsCompleted)
                {
                    statusText.text = "Node is completed";
                }
                else
                {
                    statusText.text = "Ready";
                }
                
                LogManager.LogSuccess($"WaitForNextNode UI synced - Wait Time: {WaitForNextNodeModel.WaitTimeInSeconds}s");
            }
        }
    }
} 