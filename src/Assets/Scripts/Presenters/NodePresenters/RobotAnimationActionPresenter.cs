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

        public RobotAnimationActionNode RobotAnimationModel => Model as RobotAnimationActionNode;

        protected override void Awake()
        {
            base.Awake();
            
            LogManager.LogSuccess("RobotAnimationActionPresenter started: " + gameObject.name);
        }

        private void Start()
        {
            // Description'ı sadece boşsa set et (Load'dan gelen değeri korumak için)
            if (string.IsNullOrEmpty(Model.Description))
            {
                Model.Description = "Move the robot to the target position in the specified duration";
            }
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
                    
                    // Target pozisyonunu model'e kaydet
                    if (RobotAnimationModel != null)
                    {
                        RobotAnimationModel.TargetPosX = _instantiatedTargetGhostGameObject.transform.position.x;
                        RobotAnimationModel.TargetPosY = _instantiatedTargetGhostGameObject.transform.position.y;
                        RobotAnimationModel.TargetPosZ = _instantiatedTargetGhostGameObject.transform.position.z;
                        RobotAnimationModel.HasTargetPosition = true;
                        LogManager.LogSuccess($"Robot target position saved: {_instantiatedTargetGhostGameObject.transform.position}");
                    }
                }
            }
        }


        protected override async void PerformAction()
        {
            if (_robotTargetFollow != null && _instantiatedTargetGhostGameObject != null)
            {
                // Model'den duration değerini al
                int duration = RobotAnimationModel?.Duration ?? 0;
                
                Sequence sequence = DOTween.Sequence();
                sequence.Append(
                    _robotTargetFollow.transform.DOMove(_instantiatedTargetGhostGameObject.transform.position, duration));
                
                sequence.Play();
                await Task.Delay((int)(sequence.Duration() * 1000));
                CompleteNode();
                
                LogManager.LogSuccess($"Robot animation action completed - Duration: {duration}s");
            }
            else
            {
                LogManager.LogWarning("RobotAnimation: Missing robot object or target position for action");
                CompleteNode();
            }
        }

        private void OnSelectObject()
        {
            LogManager.LogInteraction("Select robot object button clicked");
            
            try
            {
                // SystemManager.Selected3DObject null kontrolü
                if (SystemManager.Selected3DObject == null)
                {
                    LogManager.LogError("Error selecting robot object: No object selected");
                    return;
                }

                // ObjectPresenter'ı al (VIROO nesnelerinde olması gerekir)
                var objectPresenter = SystemManager.Selected3DObject.GetComponent<ObjectPresenter>();
                if (objectPresenter == null)
                {
                    LogManager.LogError("Error selecting robot object: Selected object does not have ObjectPresenter component");
                    return;
                }

                if (_simpleInteractable == null)
                {
                    _simpleInteractable = SystemManager.Selected3DObject;
                }
                else
                {
                    Destroy(_instantiatedTargetGhostGameObject);
                    _simpleInteractable = SystemManager.Selected3DObject;
                }
                
                // Input field'ı güncelle
                selectObjectInputField.text = _simpleInteractable.name;
                selectTargetButton.interactable = true;
                
                // TargetFollower component'ını bul
                _robotTargetFollow = _simpleInteractable.GetComponentInChildren<TargetFollower>()?.transform;
                if (_robotTargetFollow == null)
                {
                    LogManager.LogWarning($"TargetFollower component not found on {_simpleInteractable.name}");
                }

                // Model'i güncelle
                if (RobotAnimationModel != null)
                {
                    RobotAnimationModel.SelectedObjectName = _simpleInteractable.name;
                    RobotAnimationModel.SelectedObjectID = objectPresenter.Model.ID;
                }

                LogManager.LogSuccess($"Robot object selected: {_simpleInteractable.name} (ID: {objectPresenter.Model.ID})");
            }
            catch (Exception e)
            {
                LogManager.LogError($"Error selecting robot object: {e.Message}");
                Debug.LogException(e);
            }
        }


        private void OnSelectTarget()
        {
            LogManager.LogInteraction("Select robot target position button clicked");
            
            if (_simpleInteractable == null)
            {
                LogManager.LogWarning("No robot object selected for target positioning");
                return;
            }

            if (selectTargetGhostPrefab == null)
            {
                LogManager.LogError("selectTargetGhostPrefab is not assigned in RobotAnimationActionPresenter");
                return;
            }

            if (_instantiatedTargetGhostGameObject == null)
            {
                _instantiatedTargetGhostGameObject = Instantiate(selectTargetGhostPrefab, XRInputManager.xrRayInteractor.transform);
                selectTargetInputField.text = _instantiatedTargetGhostGameObject.name;
                _holdingTarget = true;
                
                LogManager.LogSuccess("Robot target position selection started");
            }
            else
            {
                _instantiatedTargetGhostGameObject.transform.SetParent(XRInputManager.xrRayInteractor.transform);
                _instantiatedTargetGhostGameObject.transform.localPosition = Vector3.zero;
                _holdingTarget = true;
                
                LogManager.LogSuccess("Robot target position selection restarted");
            }
        }

        private void OnIncreaseDuration()
        {
            LogManager.LogInteraction("Increase robot animation duration button clicked");
            
            int currentDuration = RobotAnimationModel?.Duration ?? 0;
            currentDuration++;
            durationInputField.text = currentDuration.ToString();
            
            // Model'e kaydet
            if (RobotAnimationModel != null)
            {
                RobotAnimationModel.Duration = currentDuration;
            }
            
            LogManager.LogSuccess($"Robot animation duration increased: {currentDuration}");
        }

        private void OnDecreaseDuration()
        {
            LogManager.LogInteraction("Decrease robot animation duration button clicked");
            
            int currentDuration = RobotAnimationModel?.Duration ?? 0;
            if (currentDuration > 0)
            {
                currentDuration--;
                durationInputField.text = currentDuration.ToString();
                
                // Model'e kaydet
                if (RobotAnimationModel != null)
                {
                    RobotAnimationModel.Duration = currentDuration;
                }
                
                LogManager.LogSuccess($"Robot animation duration decreased: {currentDuration}");
            }
            else
            {
                LogManager.LogWarning("Robot animation duration cannot be less than 0");
            }
        }
        
        /// <summary>
        /// Model'deki değerleri UI'ya aktarır (yükleme sonrası)
        /// </summary>
        public override void SyncModelToUI()
        {
            // Önce base sınıfın ortak özelliklerini sync et
            base.SyncModelToUI();
            
            if (RobotAnimationModel == null) return;

            // Duration'ı restore et
            if (durationInputField != null)
            {
                durationInputField.text = RobotAnimationModel.Duration.ToString();
            }

            // Seçili robot nesne ID'si varsa, VIROO_PrefabContainer'da nesneyi bul
            if (!string.IsNullOrEmpty(RobotAnimationModel.SelectedObjectID))
            {
                GameObject selectedObject = FindObjectByID(RobotAnimationModel.SelectedObjectID);
                if (selectedObject != null)
                {
                    _simpleInteractable = selectedObject;
                    selectTargetButton.interactable = true;

                    // TargetFollower component'ını bul
                    _robotTargetFollow = _simpleInteractable.GetComponentInChildren<TargetFollower>()?.transform;

                    // Input field'ı güncelle
                    if (selectObjectInputField != null)
                    {
                        selectObjectInputField.text = RobotAnimationModel.SelectedObjectName;
                    }

                    LogManager.LogSuccess($"RobotAnimation: Robot object restored: {RobotAnimationModel.SelectedObjectName} (ID: {RobotAnimationModel.SelectedObjectID})");

                    // Target pozisyonunu restore et
                    if (RobotAnimationModel.HasTargetPosition)
                    {
                        Vector3 targetPosition = new Vector3(RobotAnimationModel.TargetPosX, RobotAnimationModel.TargetPosY, RobotAnimationModel.TargetPosZ);
                        
                        if (selectTargetGhostPrefab != null)
                        {
                            // Eski target ghost varsa temizle
                            if (_instantiatedTargetGhostGameObject != null)
                            {
                                Destroy(_instantiatedTargetGhostGameObject);
                            }
                            
                            // Yeni target ghost oluştur ve pozisyonunu ayarla
                            _instantiatedTargetGhostGameObject = Instantiate(
                                selectTargetGhostPrefab,
                                GameObject.Find("Root").transform);
                            _instantiatedTargetGhostGameObject.transform.position = targetPosition;
                            
                            if (selectTargetInputField != null)
                            {
                                selectTargetInputField.text = _instantiatedTargetGhostGameObject.name;
                            }
                            
                            LogManager.LogSuccess($"RobotAnimation: Target ghost restored at position: {targetPosition}");
                        }
                        else
                        {
                            LogManager.LogError("RobotAnimation: selectTargetGhostPrefab not assigned");
                        }
                    }
                    else
                    {
                        LogManager.Log($"RobotAnimation: No target position to restore for {RobotAnimationModel.SelectedObjectName}");
                    }
                }
                else
                {
                    LogManager.LogWarning($"RobotAnimation: Could not find robot object with ID: {RobotAnimationModel.SelectedObjectID}");
                }
            }

            LogManager.LogSuccess($"RobotAnimation UI synced - Selected: {RobotAnimationModel.SelectedObjectName}, Duration: {RobotAnimationModel.Duration}");
        }

        /// <summary>
        /// VIROO_PrefabContainer altındaki nesneleri ObjectModel.ID ile bulur
        /// </summary>
        private GameObject FindObjectByID(string objectID)
        {
            Transform virooContainer = GameObject.Find("VIROO_PrefabContainer")?.transform;
            if (virooContainer == null)
            {
                LogManager.LogError("VIROO_PrefabContainer bulunamadı!");
                return null;
            }

            foreach (Transform child in virooContainer)
            {
                var objectPresenter = child.GetComponent<ObjectPresenter>();
                if (objectPresenter != null && objectPresenter.Model.ID == objectID)
                {
                    return child.gameObject;
                }
            }

            return null;
        }
    }
}