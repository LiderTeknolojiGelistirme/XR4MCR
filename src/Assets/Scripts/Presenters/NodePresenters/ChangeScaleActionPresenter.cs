using System;
using DG.Tweening;
using Helpers;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Models.Nodes;

namespace Presenters.NodePresenters
{
    public class ChangeScaleActionPresenter : ActionNodePresenter
    {
        [HideInInspector] public GameObject _simpleInteractable;

        [SerializeField] private GameObject selectTargetGhostPrefab;
        [SerializeField] private TMP_InputField selectObjectInputField;
        [SerializeField] private Button selectObjectButton;
        [SerializeField] private Button selectTargetButton;
        [SerializeField] private TMP_InputField durationInputField;
        [SerializeField] private Button durationIncreaseButton;
        [SerializeField] private Button durationDecreaseButton;

        private GameObject _instantiatedTargetGhostGameObject;
        private bool _holdingTarget = false;
        private int _duration = 0;

        public ChangeScaleActionNode ChangeScaleModel => Model as ChangeScaleActionNode;

        protected override void Awake()
        {
            base.Awake();
            LogManager.LogSuccess("ChangeScaleActionPresenter started: " + gameObject.name);
        }

        private void Start()
        {
            // Description'ı sadece boşsa set et (Load'dan gelen değeri korumak için)
            if (string.IsNullOrEmpty(Model.Description))
            {
                Model.Description = "Change the scale of the selected object";
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
                    
                    // Target scale bilgilerini model'e kaydet
                    if (ChangeScaleModel != null)
                    {
                        ChangeScaleModel.TargetScaleX = _instantiatedTargetGhostGameObject.transform.localScale.x;
                        ChangeScaleModel.TargetScaleY = _instantiatedTargetGhostGameObject.transform.localScale.y;
                        ChangeScaleModel.TargetScaleZ = _instantiatedTargetGhostGameObject.transform.localScale.z;
                        ChangeScaleModel.HasTargetScale = true;
                        LogManager.LogSuccess($"Target scale saved: {_instantiatedTargetGhostGameObject.transform.localScale}");
                    }
                }
            }
        }

        protected override void PerformAction()
        {
            if (_simpleInteractable != null && _instantiatedTargetGhostGameObject != null)
            {
                // Model'den duration değerini al
                int duration = ChangeScaleModel?.Duration ?? _duration;
                
                Sequence sequence = DOTween.Sequence();
                sequence.Append(
                    _simpleInteractable.transform.DOScale(_instantiatedTargetGhostGameObject.transform.localScale, duration));
                sequence.Play();
                
                LogManager.LogSuccess($"Change scale action started - Duration: {duration}s");
            }
            else
            {
                LogManager.LogWarning("ChangeScale: Missing selected object or target scale for action");
            }
        }

        private void OnSelectObject()
        {
            LogManager.LogInteraction("Select object button clicked");
            
            try
            {
                // SystemManager.Selected3DObject null kontrolü
                if (SystemManager.Selected3DObject == null)
                {
                    LogManager.LogError("Error selecting object: No object selected");
                    return;
                }

                // ObjectPresenter'ı al (VIROO nesnelerinde olması gerekir)
                var objectPresenter = SystemManager.Selected3DObject.GetComponent<ObjectPresenter>();
                if (objectPresenter == null)
                {
                    LogManager.LogError("Error selecting object: Selected object does not have ObjectPresenter component");
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

                // Model'i güncelle
                if (ChangeScaleModel != null)
                {
                    ChangeScaleModel.SelectedObjectName = _simpleInteractable.name;
                    ChangeScaleModel.SelectedObjectID = objectPresenter.Model.ID;
                }

                LogManager.LogSuccess($"Object selected: {_simpleInteractable.name} (ID: {objectPresenter.Model.ID})");
            }
            catch (Exception e)
            {
                LogManager.LogError($"Error selecting object: {e.Message}");
                Debug.LogException(e);
            }
        }

        private void OnSelectTarget()
        {
            LogManager.LogInteraction("Select target scale button clicked");
            
            if (_simpleInteractable == null)
            {
                LogManager.LogWarning("No object selected for scaling");
                return;
            }

            var interactionHelper = _simpleInteractable.GetComponent<InteractionHelper>();
            if (interactionHelper == null)
            {
                LogManager.LogError($"Selected object {_simpleInteractable.name} does not have InteractionHelper component");
                return;
            }

            if (interactionHelper.targetGhostPrefab == null)
            {
                LogManager.LogError($"InteractionHelper on {_simpleInteractable.name} does not have targetGhostPrefab assigned");
                return;
            }

            if (_instantiatedTargetGhostGameObject == null)
            {
                _instantiatedTargetGhostGameObject = Instantiate(
                    interactionHelper.targetGhostPrefab, 
                    XRInputManager.xrRayInteractor.transform);
                
                _holdingTarget = true;
                LogManager.LogSuccess("Target scale selection started");
            }
            else
            {
                _instantiatedTargetGhostGameObject.transform.SetParent(XRInputManager.xrRayInteractor.transform);
                _instantiatedTargetGhostGameObject.transform.localPosition = Vector3.zero;
                _holdingTarget = true;
                LogManager.LogSuccess("Target scale selection restarted");
            }
        }

        private void OnIncreaseDuration()
        {
            LogManager.LogInteraction("Increase duration button clicked");
            
            _duration++;
            durationInputField.text = _duration.ToString();
            
            // Model'e kaydet
            if (ChangeScaleModel != null)
            {
                ChangeScaleModel.Duration = _duration;
            }
            
            LogManager.LogSuccess($"Duration increased: {_duration}");
        }

        private void OnDecreaseDuration()
        {
            LogManager.LogInteraction("Decrease duration button clicked");
            
            if (_duration > 0)
            {
                _duration--;
                durationInputField.text = _duration.ToString();
                
                // Model'e kaydet
                if (ChangeScaleModel != null)
                {
                    ChangeScaleModel.Duration = _duration;
                }
                
                LogManager.LogSuccess($"Duration decreased: {_duration}");
            }
            else
            {
                LogManager.LogWarning("Duration cannot be less than 0");
            }
        }
        
        /// <summary>
        /// Model'deki değerleri UI'ya aktarır (yükleme sonrası)
        /// </summary>
        public override void SyncModelToUI()
        {
            // Önce base sınıfın ortak özelliklerini sync et
            base.SyncModelToUI();
            
            if (ChangeScaleModel == null) return;

            // Duration'ı restore et
            _duration = ChangeScaleModel.Duration;
            if (durationInputField != null)
            {
                durationInputField.text = _duration.ToString();
            }

            // Seçili nesne ID'si varsa, VIROO_PrefabContainer'da nesneyi bul
            if (!string.IsNullOrEmpty(ChangeScaleModel.SelectedObjectID))
            {
                GameObject selectedObject = FindObjectByID(ChangeScaleModel.SelectedObjectID);
                if (selectedObject != null)
                {
                    _simpleInteractable = selectedObject;
                    selectTargetButton.interactable = true;

                    // Input field'ı güncelle
                    if (selectObjectInputField != null)
                    {
                        selectObjectInputField.text = ChangeScaleModel.SelectedObjectName;
                    }

                    LogManager.LogSuccess($"ChangeScale: Object restored: {ChangeScaleModel.SelectedObjectName} (ID: {ChangeScaleModel.SelectedObjectID})");

                    // Target scale bilgilerini restore et
                    if (ChangeScaleModel.HasTargetScale)
                    {
                        Vector3 targetScale = new Vector3(ChangeScaleModel.TargetScaleX, ChangeScaleModel.TargetScaleY, ChangeScaleModel.TargetScaleZ);
                        
                        var interactionHelper = _simpleInteractable.GetComponent<InteractionHelper>();
                        if (interactionHelper != null && interactionHelper.targetGhostPrefab != null)
                        {
                            // Eski target ghost varsa temizle
                            if (_instantiatedTargetGhostGameObject != null)
                            {
                                Destroy(_instantiatedTargetGhostGameObject);
                            }
                            
                            // Yeni target ghost oluştur ve scale'ini ayarla
                            _instantiatedTargetGhostGameObject = Instantiate(
                                interactionHelper.targetGhostPrefab,
                                GameObject.Find("Root").transform);
                            _instantiatedTargetGhostGameObject.transform.localScale = targetScale;
                            
                            LogManager.LogSuccess($"ChangeScale: Target ghost restored with scale: {targetScale}");
                        }
                        else
                        {
                            LogManager.LogError($"ChangeScale: InteractionHelper or targetGhostPrefab not found on {_simpleInteractable.name}");
                        }
                    }
                    else
                    {
                        LogManager.Log($"ChangeScale: No target scale to restore for {ChangeScaleModel.SelectedObjectName}");
                    }
                }
                else
                {
                    LogManager.LogWarning($"ChangeScale: Could not find object with ID: {ChangeScaleModel.SelectedObjectID}");
                }
            }

            LogManager.LogSuccess($"ChangeScale UI synced - Selected: {ChangeScaleModel.SelectedObjectName}, Duration: {ChangeScaleModel.Duration}");
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