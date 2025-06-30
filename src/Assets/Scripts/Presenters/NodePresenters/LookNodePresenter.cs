using System;
using System.Linq;
using System.Net.Mime;
using Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Managers;
using Models.Nodes;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Presenters.NodePresenters
{
    public class LookNodePresenter : BaseNodePresenter
    {
        [SerializeField] private GameObject lookProgressCanvasPrefab;
        [SerializeField] private Button selectObjectButton;
        [SerializeField] private Button selectChildObjectButton;
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private TMP_InputField lookDistanceField;
        [SerializeField] private TMP_InputField lookDurationField;

        private RaycastHit _raycastHit;
        private float _timer;
        private float _fillAmount;
        private Image _progressImage;
        private Image _tickImage;
        private GameObject _instantiatedCanvasObject;
        private GameObject _targetObject;

        // Model'e kolay erişim için cast property
        private LookNode LookNodeModel => Model as LookNode;

        private void OnDisable()
        {
            if (selectObjectButton != null)
                selectObjectButton.onClick.RemoveAllListeners();
                
            if (selectChildObjectButton != null)
                selectChildObjectButton.onClick.RemoveAllListeners();
                
            if (lookDistanceField != null)
                lookDistanceField.onValueChanged.RemoveAllListeners();
                
            if (lookDurationField != null)
                lookDurationField.onValueChanged.RemoveAllListeners();
        }

        private void Awake()
        {
            if (selectObjectButton != null)
            {
                selectObjectButton.onClick.AddListener(OnSelectObject);
            }

            if (selectChildObjectButton != null)
            {
                selectChildObjectButton.onClick.AddListener(OnSelectChildObject);
            }

            SetupUI();
        }

        private void SetupUI()
        {
            // Look distance input field
            if (lookDistanceField != null)
            {
                lookDistanceField.onValueChanged.AddListener(OnLookDistanceChanged);
            }

            // Look duration input field
            if (lookDurationField != null)
            {
                lookDurationField.onValueChanged.AddListener(OnLookDurationChanged);
            }
        }

        private void OnLookDistanceChanged(string value)
        {
            if (LookNodeModel != null && float.TryParse(value, out float distance))
            {
                if (distance > 0)
                {
                    LookNodeModel.LookDistance = distance;
                    LogManager.LogInteraction($"Look distance updated: {distance}");
                }
            }
        }

        private void OnLookDurationChanged(string value)
        {
            if (LookNodeModel != null && float.TryParse(value, out float duration))
            {
                if (duration > 0)
                {
                    LookNodeModel.LookDuration = duration;
                    LogManager.LogInteraction($"Look duration updated: {duration}");
                }
            }
        }

        private void Start()
        {
            // Description'ı sadece boşsa set et (Load'dan gelen değeri korumak için)
            if (string.IsNullOrEmpty(Model.Description))
            {
                Model.Description = "Look at the selected object for a specified duration";
            }
        }

        public override void ActivateNode()
        {
            base.ActivateNode();
        }

        public override void StartNode()
        {
            Debug.Log("Start LookNodePresenter");
            base.StartNode();
            
            // Model'den target object'i al
            if (LookNodeModel != null && !string.IsNullOrEmpty(LookNodeModel.SelectedObjectID))
            {
                _targetObject = FindObjectByID(LookNodeModel.SelectedObjectID);
                
                // Child seçili ise child'ı al
                if (_targetObject != null && LookNodeModel.IsChildObjectEnabled && LookNodeModel.SelectedChildIndex >= 0)
                {
                    if (LookNodeModel.SelectedChildIndex < _targetObject.transform.childCount)
                    {
                        _targetObject = _targetObject.transform.GetChild(LookNodeModel.SelectedChildIndex).gameObject;
                    }
                }
                
                if (_targetObject != null)
                {
                    LocateCanvas();
                    _progressImage = _instantiatedCanvasObject.GetComponent<ProgressCanvasHelper>().progressImage;
                    _tickImage = _instantiatedCanvasObject.GetComponent<ProgressCanvasHelper>().tickImage;
                    _tickImage.transform.rotation = Quaternion.Euler(0, 180, 0);
                    _progressImage.transform.rotation = Quaternion.Euler(0, 180, 0);
                }
                else
                {
                    LogManager.LogWarning("LookNode: Target object not found! Please select an object.");
                }
            }
            else
            {
                LogManager.LogWarning("LookNode: No target object selected! Please select an object.");
            }
        }

        public override void CompleteNode()
        {
            Debug.Log("Complete Look Node");
            _timer = 0;
            if (_progressImage != null)
            {
                _progressImage.fillAmount = 0f;
            }
            if (_tickImage != null)
            {
                _tickImage.gameObject.SetActive(true);
            }
            base.CompleteNode();
        }

        public override void Play()
        {
            if (Camera.main == null || _targetObject == null)
            {
                return;
            }

            // Cast a ray from the camera to detect if the player is looking at the target object
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out _raycastHit,
                    LookNodeModel.LookDistance))
            {
                // Check if the object hit by the ray is the target object or its parent
                GameObject hitObject = _raycastHit.transform.gameObject;
                GameObject hitParent = _raycastHit.transform.parent?.gameObject;
                
                if (hitObject == _targetObject || hitParent == _targetObject)
                {
                    _timer += Time.deltaTime; // Increment the timer based on time looked at the object
                    _fillAmount = _timer / LookNodeModel.LookDuration; // Calculate the progress fill amount
                    
                    if (_progressImage != null)
                    {
                        _progressImage.fillAmount = _fillAmount; // Update the UI with the progress
                    }

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
                if (_progressImage != null)
                {
                    _progressImage.fillAmount = 0f;
                }
            }
        }

        public void OnSelectObject()
        {
            LogManager.LogInteraction("Select look object button clicked");
            
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

                // Input field kontrolü ve güncelleme
                if (inputField != null)
                {
                    inputField.text = SystemManager.Selected3DObject.name;
                }

                // Model'i hemen güncelle (MVP prensibi) - Parent nesneyi seç
                if (LookNodeModel != null)
                {
                    LookNodeModel.SelectedObjectName = SystemManager.Selected3DObject.name;
                    LookNodeModel.SelectedObjectID = objectPresenter.Model.ID; // Parent'ın ID'si
                    LookNodeModel.SelectedChildIndex = -1; // Parent seçili
                    LookNodeModel.IsChildObjectEnabled = false; // Child devre dışı
                }

                LogManager.LogSuccess($"Look object selected: {SystemManager.Selected3DObject.name} (ID: {objectPresenter.Model.ID})");
            }
            catch (Exception e)
            {
                LogManager.LogError($"Error selecting look object: {e.Message}");
                Debug.LogException(e);
            }
        }

        public void OnSelectChildObject()
        {
            if (SystemManager.Selected3DObject == null)
            {
                LogManager.LogError("No parent object selected. Please select a parent object first.");
                return;
            }

            if (SystemManager.Selected3DObject.transform.parent.name != "VIROO_PrefabContainer")
            {
                LogManager.LogError("Selected object is not under VIROO_PrefabContainer.");
                return;
            }

            // Child nesneleri kontrol et
            if (SystemManager.Selected3DObject.transform.childCount == 0)
            {
                LogManager.LogError("Selected object has no child objects.");
                return;
            }

            // Parent'ın ObjectPresenter'ını al (ID'yi kaydetmek için)
            var parentObjectPresenter = SystemManager.Selected3DObject.GetComponent<ObjectPresenter>();
            if (parentObjectPresenter == null)
            {
                LogManager.LogError("Parent object does not have ObjectPresenter component.");
                return;
            }

            // İlk child'ı varsayılan olarak seç
            int selectedChildIndex = 0;
            Transform selectedChild = SystemManager.Selected3DObject.transform.GetChild(selectedChildIndex);

            // Model'i güncelle
            if (LookNodeModel != null)
            {
                LookNodeModel.SelectedObjectName = selectedChild.name;
                LookNodeModel.SelectedObjectID = parentObjectPresenter.Model.ID; // Parent'ın ID'sini kaydet
                LookNodeModel.SelectedChildIndex = selectedChildIndex; // Child index'ini kaydet
                LookNodeModel.IsChildObjectEnabled = true;
            }

            // Input field'ı güncelle
            if (inputField != null)
            {
                inputField.text = $"{SystemManager.Selected3DObject.name} -> {selectedChild.name}";
            }

            LogManager.LogInteraction($"Look: Child object selected: {selectedChild.name} (Index: {selectedChildIndex}, Parent ID: {LookNodeModel.SelectedObjectID})");
        }

        /// <summary>
        /// Model'deki değerleri UI'ya aktarır (yükleme sonrası)
        /// </summary>
        public override void SyncModelToUI()
        {
            // Önce base sınıfın ortak özelliklerini sync et
            base.SyncModelToUI();
            
            if (LookNodeModel == null) return;

            // Look distance'ı sync et
            if (lookDistanceField != null)
            {
                lookDistanceField.text = LookNodeModel.LookDistance.ToString("F1");
            }

            // Look duration'ı sync et
            if (lookDurationField != null)
            {
                lookDurationField.text = LookNodeModel.LookDuration.ToString("F1");
            }

            // Seçili nesne adını input field'a aktar
            if (inputField != null && !string.IsNullOrEmpty(LookNodeModel.SelectedObjectName))
            {
                if (LookNodeModel.IsChildObjectEnabled && LookNodeModel.SelectedChildIndex >= 0)
                {
                    // Child seçili - parent->child formatında göster
                    GameObject parentObject = FindObjectByID(LookNodeModel.SelectedObjectID);
                    if (parentObject != null)
                    {
                        inputField.text = $"{parentObject.name} -> {LookNodeModel.SelectedObjectName}";
                    }
                    else
                    {
                        inputField.text = LookNodeModel.SelectedObjectName;
                    }
                }
                else
                {
                    // Parent seçili
                    inputField.text = LookNodeModel.SelectedObjectName;
                }
            }

            LogManager.LogSuccess($"LookNode UI synced - Selected: {LookNodeModel.SelectedObjectName}, Distance: {LookNodeModel.LookDistance}, Duration: {LookNodeModel.LookDuration}");
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

        void LocateCanvas()
        {
            if (_targetObject == null || lookProgressCanvasPrefab == null) return;
            
            Vector3 offsetDirection = (Camera.main.transform.position - _targetObject.transform.position).normalized;
            Vector3 spawnPosition = _targetObject.transform.position + offsetDirection * .5f;
            _instantiatedCanvasObject = Instantiate(lookProgressCanvasPrefab, spawnPosition, Quaternion.identity, _targetObject.transform);
            Vector3 tempRotation = _instantiatedCanvasObject.transform.rotation.eulerAngles;
            tempRotation.y = 180f;
            _instantiatedCanvasObject.transform.rotation = Quaternion.Euler(tempRotation);
        }
    }
}