using System;
using DG.Tweening;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Presenters.NodePresenters
{
    public class ChangeScaleActionPresenter : ActionNodePresenter
    {
        [HideInInspector] public GameObject _simpleInteractable;

        [SerializeField] private GameObject selectTargetGhostPrefab;
        [SerializeField] private TMP_InputField selectObjectInputField;
        [SerializeField] private Button selectObjectButton;
        [SerializeField] private TMP_InputField selectTargetInputField;
        [SerializeField] private Button selectTargetButton;
        [SerializeField] private TMP_InputField durationInputField;
        [SerializeField] private Button durationIncreaseButton;
        [SerializeField] private Button durationDecreaseButton;

        private GameObject _instantiatedTargetGhostGameObject;
        private bool _holdingTarget = false;
        private int _duration = 0;


        protected override void Awake()
        {
            LogManager.LogSuccess("ChangePositionActionPresenter başlatıldı: " + gameObject.name);
        }

        private void OnEnable()
        {
            selectObjectButton.onClick.AddListener(OnSelectObject);
            selectTargetButton.onClick.AddListener(OnSelectTarget);
            durationIncreaseButton.onClick.AddListener(OnIncreaseDuration);
            durationDecreaseButton.onClick.AddListener(OnDecreaseDuration);
        }

        protected override void OnDisable()
        {
            selectObjectButton.onClick.RemoveAllListeners();
            selectTargetButton.onClick.RemoveAllListeners();
            durationIncreaseButton.onClick.RemoveAllListeners();
            durationDecreaseButton.onClick.RemoveAllListeners();   
        }


        protected override void Update()
        {
            if (_holdingTarget)
            {
                if (XRInputManager.GetRawTriggerState())
                {
                    var parent = GameObject.Find("Root").transform;
                    Debug.Log(parent.name);
                    _instantiatedTargetGhostGameObject.transform.parent = parent;
                    _holdingTarget = false;
                }
            }
        }


        protected override void PerformAction()
        {
            Sequence sequence = DOTween.Sequence();
            sequence.Append(
                _simpleInteractable.transform.DOScale(_instantiatedTargetGhostGameObject.transform.localScale, _duration));
            sequence.Play();
        }

        private void OnSelectObject()
        {
            try
            {
                _simpleInteractable = SystemManager.Selected3DObject;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }

            selectObjectInputField.text = _simpleInteractable.name;
        }


        private void OnSelectTarget()
        {
            if (_instantiatedTargetGhostGameObject == null)
            {
                _instantiatedTargetGhostGameObject =
                    Instantiate(selectTargetGhostPrefab, XRInputManager.xrRayInteractor.transform);
                selectTargetInputField.text = _instantiatedTargetGhostGameObject.name;
                _holdingTarget = true;
            }
            else
            {
                _instantiatedTargetGhostGameObject.transform.SetParent(XRInputManager.xrRayInteractor.transform);
                _instantiatedTargetGhostGameObject.transform.localPosition = Vector3.zero;
                _holdingTarget = true;
            }
        }

        private void OnIncreaseDuration()
        {
            _duration++;
            durationInputField.text = _duration.ToString();
        }

        private void OnDecreaseDuration()
        {
            if (_duration > 0)
            {
                _duration--;
                durationInputField.text = _duration.ToString();
            }
            
        }
        
    }
}