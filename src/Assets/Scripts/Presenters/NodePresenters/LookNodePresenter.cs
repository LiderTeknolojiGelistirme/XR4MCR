using System;
using System.Linq;
using System.Net.Mime;
using Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Presenters.NodePresenters
{
    public class LookNodePresenter : BaseNodePresenter
    {
        public GameObject lookObject;
        public float lookDistance;
        public float lookDuration;
        public GameObject lookProgressCanvasPrefab;

        private RaycastHit _raycastHit;
        private float _timer;
        private float _fillAmount;
        private Image _progressImage;
        private Image _tickImage;
        private GameObject _instantiatedCanvasObject;

        public Button selectObjectButton;
        public TMP_InputField inputField;

        private void OnDisable()
        {
            selectObjectButton.onClick.RemoveAllListeners();
        }

        private void Awake()
        {
            selectObjectButton.onClick.AddListener(OnSelectObject);
        }

        public override void ActivateNode()
        {
            base.ActivateNode();
        }

        public override void StartNode()
        {
            Debug.Log("Start LookNodePresenter");
            base.StartNode();
            LocateCanvas();
            _progressImage = _instantiatedCanvasObject.GetComponent<ProgressCanvasHelper>().progressImage;
            _tickImage = _instantiatedCanvasObject.GetComponent<ProgressCanvasHelper>().tickImage;
            _tickImage.transform.rotation = Quaternion.Euler(0, 180, 0);
            _progressImage.transform.rotation = Quaternion.Euler(0, 180, 0);
        }

        public override void CompleteNode()
        {
            Debug.Log("Complete Look Node");
            _timer = 0;
            _progressImage.fillAmount = 0f;
            _tickImage.gameObject.SetActive(true);
            base.CompleteNode();
        }

        public override void DeactivateNode()
        {
            base.DeactivateNode();
        }

        public override void Play()
        {
            if (Camera.main == null)
            {
                return;
            }

            // Cast a ray from the camera to detect if the player is looking at the target object
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out _raycastHit,
                    lookDistance))
            {
                // Check if the object hit by the ray is the target object
                if (lookObject == _raycastHit.transform.parent.gameObject)
                {
                    _timer += Time.deltaTime; // Increment the timer based on time looked at the object
                    _fillAmount = _timer / lookDuration; // Calculate the progress fill amount
                    _progressImage.fillAmount = _fillAmount; // Update the UI with the progress

                    // If the player has looked long enough, complete the procedure
                    if (_fillAmount >= 1f)
                    {
                        _fillAmount = 1f;
                        CompleteNode();
                    }
                }
            }
            else
            {
                // Reset the timer and progress if the player is no longer looking at the object
                _timer = 0f;
                _progressImage.fillAmount = 0f;
            }
        }

        public void OnSelectObject()
        {
            try
            {
                lookObject = SystemManager.Selected3DObject;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }
            inputField.text = lookObject.name;
        }
        

        void LocateCanvas()
        {
            Vector3 offsetDirection = (Camera.main.transform.position - lookObject.transform.position).normalized;
            Vector3 spawnPosition = lookObject.transform.position + offsetDirection * .5f;
            _instantiatedCanvasObject = Instantiate(lookProgressCanvasPrefab, spawnPosition, Quaternion.identity,lookObject.transform);
            Vector3 tempRotation = _instantiatedCanvasObject.transform.rotation.eulerAngles;
            tempRotation.y = 180f;
            _instantiatedCanvasObject.transform.rotation = Quaternion.Euler(tempRotation);

        }
    }
}