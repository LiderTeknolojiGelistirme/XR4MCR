using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Presenters.NodePresenters
{
    public class WaitForNextNode : BaseNodePresenter
    {
        public Button increaseButton;
        public Button decreaseButton;
        public TMP_InputField inputField;
        public TMP_Text statusText;

        
        private float _waitTimeInSeconds = 5f;
        private float _intialTimeInSeconds;
        private bool _isTimerRunning = false;

        private void Awake()
        {
            increaseButton.onClick.AddListener(OnIncreaseTime);  // Artýrma butonu için dinleyici ayarla
            decreaseButton.onClick.AddListener(OnDecreaseTime);  // Azaltma butonu için dinleyici ayarla

            // Input field deðiþikliklerini dinle
            inputField.onEndEdit.AddListener(OnInputFieldChanged);
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
            
            inputField.text = Mathf.RoundToInt(_waitTimeInSeconds).ToString();
            
            
            if (_isTimerRunning)
            {
                _waitTimeInSeconds -= Time.deltaTime;

               
                float remainingTime = Mathf.Max(0, _waitTimeInSeconds);
                statusText.text = $"Waiting for next node... ({Mathf.RoundToInt(remainingTime)} sn)";

                
            }
        }

        public override void ActivateNode()
        {
            base.ActivateNode();
            _intialTimeInSeconds = Int32.Parse(inputField.text.Trim());
            _waitTimeInSeconds = _intialTimeInSeconds;
            _isTimerRunning = false;
            inputField.text = _waitTimeInSeconds.ToString();
            statusText.text = "Ready";
        }

        public override void StartNode()
        {
            base.StartNode();
            _isTimerRunning = true;
            statusText.text = $"Waiting for next node... ({Mathf.RoundToInt(_waitTimeInSeconds)} sn)";
        }

        public override void CompleteNode()
        {
            base.CompleteNode();
            statusText.text = "Node is complated";
            _isTimerRunning = false;
        }

        public override void DeactivateNode()
        {
            base.DeactivateNode();
            _isTimerRunning = false;
            statusText.text = "";
        }

        public void OnNextNode()
        {
           
            _isTimerRunning = false;
            _waitTimeInSeconds = _intialTimeInSeconds;
            CompleteNode();
            Debug.Log("Is passing next node");
            
        }

        public void OnIncreaseTime()
        {
            _waitTimeInSeconds += 1f;           
            inputField.text = Mathf.RoundToInt(_waitTimeInSeconds).ToString();
        }

        public void OnDecreaseTime()
        {
            _waitTimeInSeconds -= 1f;
            if (_waitTimeInSeconds < 1f)
            {
                _waitTimeInSeconds = 1f;
            }
            inputField.text = Mathf.RoundToInt(_waitTimeInSeconds).ToString();
        }

        private void OnInputFieldChanged(string value)
        {
            
            if (int.TryParse(value, out int seconds))
            {
                _waitTimeInSeconds = seconds;
            }
            else
            {
                inputField.text = Mathf.RoundToInt(_waitTimeInSeconds).ToString();
            }
        }


        public override void Play()
        {
            base.Play();

            if (_waitTimeInSeconds <= 0)
            {
                _isTimerRunning = false;
                 
                OnNextNode();
            }
            
           
        }
    }
}