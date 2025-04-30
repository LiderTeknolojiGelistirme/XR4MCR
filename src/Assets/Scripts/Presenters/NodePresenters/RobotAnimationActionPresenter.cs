using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Helpers;
using UnityEngine;
using Models.Nodes;
using Managers;
using Preliy.Flange;
using TMPro;
using UnityEngine.Serialization;
using UnityEngine.UI;
using System.Threading.Tasks;

namespace Presenters.NodePresenters
{
    public class RobotAnimationActionPresenter : ActionNodePresenter
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
        private Transform _robotTargetFollow;
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
            if (_instantiatedTargetGhostGameObject != null)
            {
                Destroy(_instantiatedTargetGhostGameObject);
            }

            
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


        protected override async void PerformAction()
        {
            Sequence sequence = DOTween.Sequence();
            sequence.Append(
                _robotTargetFollow.transform.DOMove(_instantiatedTargetGhostGameObject.transform.position, _duration));
            
             sequence.Play();
            await Task.Delay((int)(sequence.Duration() * 1000));
            CompleteNode();

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
            _robotTargetFollow = _simpleInteractable.GetComponentInChildren<TargetFollower>().transform;
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