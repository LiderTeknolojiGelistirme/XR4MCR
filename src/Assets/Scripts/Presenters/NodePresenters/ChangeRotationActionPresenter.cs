using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Helpers;
using UnityEngine;
using Models.Nodes;
using Managers;
using TMPro;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Presenters.NodePresenters
{
    public class ChangeRotationActionPresenter : ActionNodePresenter
    {
        [HideInInspector] public GameObject _simpleInteractable;
        
        // Child seçim modu (ChangePositionActionPresenter'dan exact copy)
        private bool _isChildSelectionMode = false;
        
        [SerializeField] private TMP_InputField selectObjectInputField;
        [SerializeField] private Button selectObjectButton;
        [SerializeField] private Button selectTargetButton;
        [SerializeField] private Button selectChildObjectButton;
        [SerializeField] private TMP_Text childStatusText;
        [SerializeField] private TMP_InputField durationInputField;
        [SerializeField] private Button durationIncreaseButton;
        [SerializeField] private Button durationDecreaseButton;

        private GameObject _instantiatedTargetGhostGameObject;
        private bool _holdingTarget = false;
        private int _duration = 0;

        public ChangeRotationActionNode ChangeRotationModel => Model as ChangeRotationActionNode;

        protected override void Awake()
        {
            base.Awake();
            LogManager.LogSuccess("ChangeRotationActionPresenter started: " + gameObject.name);
        }

        private void Start()
        {
            // Description'ı sadece boşsa set et (Load'dan gelen değeri korumak için)
            if (string.IsNullOrEmpty(Model.Description))
            {
                Model.Description = "Rotate the selected object to the target rotation";
            }
        }

        private void OnEnable()
        {
            selectObjectButton.onClick.AddListener(OnSelectObject);
            selectTargetButton.onClick.AddListener(OnSelectTarget);
            selectChildObjectButton?.onClick.AddListener(OnSelectChildObject);
            durationIncreaseButton.onClick.AddListener(OnIncreaseDuration);
            durationDecreaseButton.onClick.AddListener(OnDecreaseDuration);
        }

        protected override void OnDisable()
        {
            selectObjectButton.onClick.RemoveAllListeners();
            selectTargetButton.onClick.RemoveAllListeners();
            selectChildObjectButton?.onClick.RemoveAllListeners();
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
                    
                    // Target rotation'ı model'e kaydet
                    if (ChangeRotationModel != null)
                    {
                        Vector3 targetRotation = _instantiatedTargetGhostGameObject.transform.rotation.eulerAngles;
                        ChangeRotationModel.TargetRotX = targetRotation.x;
                        ChangeRotationModel.TargetRotY = targetRotation.y;
                        ChangeRotationModel.TargetRotZ = targetRotation.z;
                        ChangeRotationModel.HasTargetRotation = true;
                        LogManager.LogSuccess($"Target rotation saved: {targetRotation}");
                    }
                }
            }
        }

        protected override void PerformAction()
        {
            if (_simpleInteractable != null && _instantiatedTargetGhostGameObject != null)
            {
                GameObject objectToRotate = _simpleInteractable;
                
                // Eğer child object seçilmişse, child object'i döndür
                if (ChangeRotationModel != null && ChangeRotationModel.IsChildObjectEnabled && !string.IsNullOrEmpty(ChangeRotationModel.SelectedChildName))
                {
                    Transform childTransform = FindChildByNameRecursive(_simpleInteractable.transform, ChangeRotationModel.SelectedChildName);
                    if (childTransform != null)
                    {
                        objectToRotate = childTransform.gameObject;
                        LogManager.Log($"Rotating child object: {childTransform.name}");
                    }
                    else
                    {
                        LogManager.LogWarning($"Child object not found: {ChangeRotationModel.SelectedChildName}");
                        return; // Child bulunamazsa döndürme
                    }
                }
                else
                {
                    LogManager.Log($"Rotating parent object: {_simpleInteractable.name}");
                }

                // Model'den duration değerini al
                int duration = ChangeRotationModel?.Duration ?? _duration;
                
                Sequence sequence = DOTween.Sequence();
                sequence.Append(
                    objectToRotate.transform.DORotate(_instantiatedTargetGhostGameObject.transform.rotation.eulerAngles, duration));
                
                sequence.Play();
                
                string rotatedObjectName = ChangeRotationModel?.IsChildObjectEnabled == true ? 
                    ChangeRotationModel.SelectedChildName : _simpleInteractable.name;
                
                LogManager.LogSuccess($"Change rotation action started - Object: {rotatedObjectName}, Duration: {duration}s");
            }
            else
            {
                LogManager.LogWarning("ChangeRotation: Missing selected object or target rotation for action");
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

                // Child seçim modundaysa child seçimi yap (ChangePositionActionPresenter exact copy)
                if (_isChildSelectionMode)
                {
                    SelectChild(SystemManager.Selected3DObject);
                    return;
                }

                // Parent seçimi (ChangePositionActionPresenter exact copy)
                SetParentObject(SystemManager.Selected3DObject);
            }
            catch (Exception e)
            {
                LogManager.LogError($"Error selecting object: {e.Message}");
                Debug.LogException(e);
            }
        }

        private void SetParentObject(GameObject selectedObject)
        {
            // ObjectPresenter'ı al (VIROO nesnelerinde olması gerekir)
            var objectPresenter = selectedObject.GetComponent<ObjectPresenter>();
            if (objectPresenter == null)
            {
                LogManager.LogError("Error selecting object: Selected object does not have ObjectPresenter component");
                return;
            }

            if (_simpleInteractable == null)
            {
                _simpleInteractable = selectedObject;
            }
            else
            {
                // Eski target ghost'u temizle
                if (_instantiatedTargetGhostGameObject != null)
                {
                    Destroy(_instantiatedTargetGhostGameObject);
                    _instantiatedTargetGhostGameObject = null;
                }
                _simpleInteractable = selectedObject;
            }

            selectTargetButton.interactable = true;

            // Input field'ı güncelle
            if (selectObjectInputField != null)
            {
                selectObjectInputField.text = _simpleInteractable.name;
            }

            // Model'i güncelle - Parent nesneyi seç
            if (ChangeRotationModel != null)
            {
                ChangeRotationModel.SelectedObjectName = _simpleInteractable.name;
                ChangeRotationModel.SelectedObjectID = objectPresenter.Model.ID; // Parent'ın ID'si
                ChangeRotationModel.SelectedChildIndex = -1; // Parent seçili
                ChangeRotationModel.SelectedChildName = null; // Child name'ini temizle
                ChangeRotationModel.IsChildObjectEnabled = false; // Child devre dışı
                
                // Yeni nesne seçildiğinde eski target rotation'ını temizle
                ChangeRotationModel.HasTargetRotation = false;
                ChangeRotationModel.TargetRotX = 0f;
                ChangeRotationModel.TargetRotY = 0f;
                ChangeRotationModel.TargetRotZ = 0f;
            }

            // Target ghost nesnesini güncelle (eğer zaten varsa) - Her zaman parent nesnenin target ghost'unu kullan
            if (_instantiatedTargetGhostGameObject != null)
            {
                var interactionHelper = _simpleInteractable.GetComponent<InteractionHelper>();
                if (interactionHelper?.targetGhostPrefab != null)
                {
                    var go = Instantiate(interactionHelper.targetGhostPrefab, GameObject.Find("Root").transform);
                    go.transform.position = _instantiatedTargetGhostGameObject.transform.position;
                    go.transform.rotation = _instantiatedTargetGhostGameObject.transform.rotation;
                    go.transform.localScale = _instantiatedTargetGhostGameObject.transform.localScale;
                    Destroy(_instantiatedTargetGhostGameObject);
                    _instantiatedTargetGhostGameObject = go;
                }
            }
            
            if (selectChildObjectButton != null)
            {
                selectChildObjectButton.interactable = true;
            }

            // Child status text'ini güncelle
            UpdateChildStatusText();

            LogManager.LogSuccess($"Parent object selected: {_simpleInteractable.name} (ID: {objectPresenter.Model.ID})");
        }

        public void OnSelectChildObject()
        {
            if (_simpleInteractable == null)
            {
                LogManager.LogError("No parent object selected. Please select a parent object first.");
                return;
            }

            if (_simpleInteractable.transform.parent.name != "VIROO_PrefabContainer")
            {
                LogManager.LogError("Selected object is not under VIROO_PrefabContainer.");
                return;
            }

            // Child nesneleri kontrol et
            if (_simpleInteractable.transform.childCount == 0)
            {
                LogManager.LogError("Selected object has no child objects.");
                return;
            }

            // Child seçim modunu aktif et (ChangePositionActionPresenter exact copy)
            _isChildSelectionMode = true;

            // Parent'ın collider'ını devre dışı bırak
            var parentCollider = _simpleInteractable.GetComponent<Collider>();
            if (parentCollider != null)
            {
                parentCollider.enabled = false;
            }

            // Tüm child'ların collider'larını aktif et (seçim için)
            for (int i = 0; i < _simpleInteractable.transform.childCount; i++)
            {
                var childCollider = _simpleInteractable.transform.GetChild(i).GetComponent<Collider>();
                if (childCollider != null)
                {
                    childCollider.enabled = true;
                }
            }

            LogManager.LogInteraction("Child selection mode activated. Please select a child object.");
        }

        private void SelectChild(GameObject selectedObject)
        {
            if (_simpleInteractable == null)
            {
                LogManager.LogError("No parent object selected.");
                return;
            }

            // Parent'ın ObjectPresenter'ını al
            var parentObjectPresenter = _simpleInteractable.GetComponent<ObjectPresenter>();
            if (parentObjectPresenter == null)
            {
                LogManager.LogError("Parent object does not have ObjectPresenter component.");
                return;
            }

            // Seçilen nesnenin parent hierarchy'sinde olup olmadığını recursive olarak kontrol et (ChangePositionActionPresenter exact copy)
            Transform selectedChild = FindChildInHierarchy(_simpleInteractable.transform, selectedObject);
            
            if (selectedChild == null)
            {
                LogManager.LogError("Selected object is not found in the parent hierarchy.");
                return;
            }

            // Configuration zamanında collider'ları restore et (parent açık, child'lar edit için açık) (ChangePositionActionPresenter exact copy)
            RestoreConfigurationColliders(_simpleInteractable.transform);

            // Model'i güncelle - child bilgilerini güncelle (ChangePositionActionPresenter exact copy)
            if (ChangeRotationModel != null)
            {
                // SelectedObjectName ve SelectedObjectID parent olarak kalır, değişmez
                ChangeRotationModel.SelectedChildName = selectedChild.name; // Child name'ini kaydet
                ChangeRotationModel.IsChildObjectEnabled = true; // Child seçimi etkin
                
                // Index'i de güncelle (backward compatibility için)
                for (int i = 0; i < _simpleInteractable.transform.childCount; i++)
                {
                    if (_simpleInteractable.transform.GetChild(i) == selectedChild)
                    {
                        ChangeRotationModel.SelectedChildIndex = i;
                        break;
                    }
                }
                
                // Yeni child seçildiğinde eski target rotation'ını temizle
                ChangeRotationModel.HasTargetRotation = false;
                ChangeRotationModel.TargetRotX = 0f;
                ChangeRotationModel.TargetRotY = 0f;
                ChangeRotationModel.TargetRotZ = 0f;
            }

            // Input field'ı güncelle - parent name'i göster (ChangePositionActionPresenter exact copy)
            if (selectObjectInputField != null)
            {
                selectObjectInputField.text = $"{parentObjectPresenter.gameObject.name} -> {selectedChild.name}";
            }

            // Child seçim modunu kapat
            _isChildSelectionMode = false;

            // Child status text'ini güncelle (ChangePositionActionPresenter exact copy)  
            UpdateChildStatusText();

            LogManager.LogInteraction($"Child object selected: {selectedChild.name} (Name: {selectedChild.name}, Parent ID: {parentObjectPresenter.Model.ID})");
        }

        private void OnSelectTarget()
        {
            LogManager.LogInteraction("Select target rotation button clicked");
            
            if (_simpleInteractable == null)
            {
                LogManager.LogWarning("No object selected for rotation");
                return;
            }

            // Child seçiliyse child objesinin InteractionHelper'ını kullan
            GameObject targetObject = _simpleInteractable;
            if (ChangeRotationModel != null && ChangeRotationModel.IsChildObjectEnabled && !string.IsNullOrEmpty(ChangeRotationModel.SelectedChildName))
            {
                Transform childTransform = FindChildByNameRecursive(_simpleInteractable.transform, ChangeRotationModel.SelectedChildName);
                if (childTransform != null)
                {
                    targetObject = childTransform.gameObject;
                    LogManager.Log($"Using child object's target ghost: {childTransform.name}");
                }
                else
                {
                    LogManager.LogWarning($"Child object not found: {ChangeRotationModel.SelectedChildName}, using parent's target ghost");
                }
            }

            var interactionHelper = targetObject.GetComponent<InteractionHelper>();
            if (interactionHelper == null)
            {
                LogManager.LogError($"Selected object {targetObject.name} does not have InteractionHelper component");
                return;
            }

            if (interactionHelper.targetGhostPrefab == null)
            {
                LogManager.LogError($"InteractionHelper on {targetObject.name} does not have targetGhostPrefab assigned");
                return;
            }

            if (_instantiatedTargetGhostGameObject == null)
            {
                _instantiatedTargetGhostGameObject = Instantiate(
                    interactionHelper.targetGhostPrefab, 
                    XRInputManager.xrRayInteractor.transform);
                
                _holdingTarget = true;
                LogManager.LogSuccess($"Target rotation selection started for: {targetObject.name}");
            }
            else
            {
                _instantiatedTargetGhostGameObject.transform.SetParent(XRInputManager.xrRayInteractor.transform);
                _instantiatedTargetGhostGameObject.transform.localPosition = Vector3.zero;
                _holdingTarget = true;
                LogManager.LogSuccess($"Target rotation selection restarted for: {targetObject.name}");
            }
        }

        private void OnIncreaseDuration()
        {
            LogManager.LogInteraction("Increase duration button clicked");
            
            _duration++;
            durationInputField.text = _duration.ToString();
            
            // Model'e kaydet
            if (ChangeRotationModel != null)
            {
                ChangeRotationModel.Duration = _duration;
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
                if (ChangeRotationModel != null)
                {
                    ChangeRotationModel.Duration = _duration;
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
            
            if (ChangeRotationModel == null) return;

            // Duration'ı restore et
            _duration = ChangeRotationModel.Duration;
            if (durationInputField != null)
            {
                durationInputField.text = _duration.ToString();
            }

            // Seçili nesneyi restore et
            RestoreSelectedObject();

            // Child status text'ini güncelle
            UpdateChildStatusText();

            LogManager.LogSuccess($"ChangeRotation UI synced - Selected: {ChangeRotationModel.SelectedObjectName}, ChildName: {ChangeRotationModel.SelectedChildName}, Duration: {ChangeRotationModel.Duration}");
        }

        private void RestoreSelectedObject()
        {
            if (string.IsNullOrEmpty(ChangeRotationModel.SelectedObjectID)) return;

            GameObject selectedObject = FindObjectByID(ChangeRotationModel.SelectedObjectID);
            if (selectedObject == null)
            {
                LogManager.LogWarning($"ChangeRotation: Could not find object with ID: {ChangeRotationModel.SelectedObjectID}");
                return;
            }

            // Nesneyi ayarla
            SetSelectedObject(selectedObject);
        }

        private void SetSelectedObject(GameObject parentObject)
        {
            _simpleInteractable = parentObject;
            selectTargetButton.interactable = true;
            
            if (selectChildObjectButton != null)
            {
                selectChildObjectButton.interactable = true;
            }

            // Child seçimi varsa onu ayarla (ChangePositionActionPresenter exact copy)
            if (ChangeRotationModel.IsChildObjectEnabled && !string.IsNullOrEmpty(ChangeRotationModel.SelectedChildName))
            {
                Transform childTransform = FindChildByNameRecursive(parentObject.transform, ChangeRotationModel.SelectedChildName);
                if (childTransform != null)
                {
                    // Input field'ını güncelle - parent -> child
                    if (selectObjectInputField != null)
                    {
                        selectObjectInputField.text = $"{parentObject.name} -> {childTransform.name}";
                    }
                    
                    // Child status text'ini güncelle (ChangePositionActionPresenter exact copy)
                    UpdateChildStatusText();
                    
                    LogManager.LogSuccess($"ChangeRotation: Child object restored: {childTransform.name} (Parent ID: {ChangeRotationModel.SelectedObjectID})");
                }
                else
                {
                    LogManager.LogWarning($"ChangeRotation: Could not find child object: {ChangeRotationModel.SelectedChildName}");
                    
                    // Child bulunamazsa parent moduna geri dön
                    if (ChangeRotationModel != null)
                    {
                        ChangeRotationModel.IsChildObjectEnabled = false;
                        ChangeRotationModel.SelectedChildName = null;
                        ChangeRotationModel.SelectedChildIndex = -1;
                    }
                    
                    if (selectObjectInputField != null)
                    {
                        selectObjectInputField.text = parentObject.name;
                    }
                    
                    // Child status text'ini güncelle (ChangePositionActionPresenter exact copy)
                    UpdateChildStatusText();
                }
            }
            else
            {
                // Parent modu
                if (selectObjectInputField != null)
                {
                    selectObjectInputField.text = parentObject.name;
                }
                
                // Child status text'ini güncelle (ChangePositionActionPresenter exact copy)
                UpdateChildStatusText();
                
                LogManager.LogSuccess($"ChangeRotation: Parent object restored: {parentObject.name} (ID: {ChangeRotationModel.SelectedObjectID})");
            }

            // Target rotation'ı restore et
            if (ChangeRotationModel.HasTargetRotation)
            {
                Vector3 targetRotation = new Vector3(ChangeRotationModel.TargetRotX, ChangeRotationModel.TargetRotY, ChangeRotationModel.TargetRotZ);
                
                // Child seçiliyse child objesinin InteractionHelper'ını kullan
                GameObject targetObject = _simpleInteractable;
                if (ChangeRotationModel.IsChildObjectEnabled && !string.IsNullOrEmpty(ChangeRotationModel.SelectedChildName))
                {
                    Transform childTransform = FindChildByNameRecursive(parentObject.transform, ChangeRotationModel.SelectedChildName);
                    if (childTransform != null)
                    {
                        targetObject = childTransform.gameObject;
                        LogManager.Log($"Restoring target ghost for child object: {childTransform.name}");
                    }
                    else
                    {
                        LogManager.LogWarning($"Child object not found during restore: {ChangeRotationModel.SelectedChildName}, using parent's target ghost");
                    }
                }
                
                var interactionHelper = targetObject.GetComponent<InteractionHelper>();
                if (interactionHelper != null && interactionHelper.targetGhostPrefab != null)
                {
                    // Eski target ghost varsa temizle
                    if (_instantiatedTargetGhostGameObject != null)
                    {
                        Destroy(_instantiatedTargetGhostGameObject);
                    }
                    
                    // Yeni target ghost oluştur ve rotation'ını ayarla
                    _instantiatedTargetGhostGameObject = Instantiate(
                        interactionHelper.targetGhostPrefab,
                        GameObject.Find("Root").transform);
                    _instantiatedTargetGhostGameObject.transform.rotation = Quaternion.Euler(targetRotation);
                    
                    LogManager.LogSuccess($"ChangeRotation: Target ghost restored with rotation: {targetRotation} for object: {targetObject.name}");
                }
                else
                {
                    LogManager.LogError($"ChangeRotation: InteractionHelper or targetGhostPrefab not found on {targetObject.name}");
                }
            }
            else
            {
                LogManager.Log($"ChangeRotation: No target rotation to restore for {ChangeRotationModel.SelectedObjectName}");
            }
        }

        /// <summary>
        /// Sahne içindeki nesneleri ObjectModel.ID ile bulur
        /// </summary>
        private GameObject FindObjectByID(string objectID)
        {
            if (string.IsNullOrEmpty(objectID)) return null;

            // Sahne içindeki tüm ObjectPresenter'ları bul
            ObjectPresenter[] allObjectPresenters = FindObjectsByType<ObjectPresenter>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            
            foreach (var objectPresenter in allObjectPresenters)
            {
                if (objectPresenter.Model != null && objectPresenter.Model.ID == objectID)
                {
                    return objectPresenter.gameObject;
                }
            }
            
            return null;
        }

        // ChangePositionActionPresenter'dan entegre edilen child utility metodları (exact copy)
        private Transform FindChildInHierarchy(Transform parent, GameObject target)
        {
            // Önce direct child'lara bak
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (child.gameObject == target)
                {
                    return child;
                }
            }

            // Direct child'larda bulunamazsa recursive olarak alt seviyeye in
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                Transform result = FindChildInHierarchy(child, target);
                if (result != null)
                {
                    return result; // Alt seviyede bulunduysa onu döndür
                }
            }

            return null; // Bulunamadı
        }

        private void SetChildCollidersState(Transform parent, Transform targetChild)
        {
            // Parent'ın collider'ını devre dışı bırak
            var parentCollider = parent.GetComponent<Collider>();
            if (parentCollider != null) parentCollider.enabled = false;

            // Tüm child'ları recursive olarak tara ve sadece target child'ı aktif et
            SetCollidersRecursive(parent, targetChild);
        }

        private void SetCollidersRecursive(Transform current, Transform targetChild)
        {
            // Mevcut nesnenin collider'ını kontrol et
            var collider = current.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = (current == targetChild);
            }

            // Child'ları için recursive çağrı
            for (int i = 0; i < current.childCount; i++)
            {
                SetCollidersRecursive(current.GetChild(i), targetChild);
            }
        }

        private void RestoreConfigurationColliders(Transform parent)
        {
            // Configuration zamanında: parent açık, child'lar kapalı
            // Bu sayede karışıklık olmaz ve normal parent seçimi yapılabilir
            
            // Parent collider'ı aç
            var parentCollider = parent.GetComponent<Collider>();
            if (parentCollider != null)
            {
                parentCollider.enabled = true;
            }

            // Tüm child collider'ları kapat (configuration modu)
            DisableChildCollidersRecursive(parent);
        }

        private void EnableChildCollidersRecursive(Transform current)
        {
            // Mevcut nesnenin child'larını kontrol et
            for (int i = 0; i < current.childCount; i++)
            {
                Transform child = current.GetChild(i);
                
                // Child'ın collider'ını aç
                var childCollider = child.GetComponent<Collider>();
                if (childCollider != null)
                {
                    childCollider.enabled = true;
                }

                // Alt seviyedeki child'lar için recursive çağrı
                EnableChildCollidersRecursive(child);
            }
        }

        private void DisableChildCollidersRecursive(Transform current)
        {
            // Mevcut nesnenin child'larını kontrol et
            for (int i = 0; i < current.childCount; i++)
            {
                Transform child = current.GetChild(i);
                
                // Child'ın collider'ını kapat
                var childCollider = child.GetComponent<Collider>();
                if (childCollider != null)
                {
                    childCollider.enabled = false;
                }

                // Alt seviyedeki child'lar için recursive çağrı
                DisableChildCollidersRecursive(child);
            }
        }

        private Transform FindChildByNameRecursive(Transform parent, string childName)
        {
            // Direct child'larda ara
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (child.name == childName)
                {
                    return child;
                }
            }

            // Alt seviyede recursive ara
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                Transform result = FindChildByNameRecursive(child, childName);
                if (result != null)
                {
                    return result;
                }
            }

            return null; // Bulunamadı
        }

        private void UpdateChildStatusText()
        {
            if (childStatusText == null) return;

            if (ChangeRotationModel != null && ChangeRotationModel.IsChildObjectEnabled && !string.IsNullOrEmpty(ChangeRotationModel.SelectedChildName))
            {
                // Child seçili
                childStatusText.text = $"Selected Child: {ChangeRotationModel.SelectedChildName}";
                childStatusText.color = Color.green; // Seçili durumu vurgulamak için
            }
            else
            {
                // Parent seçili veya child yok
                childStatusText.text = "Selected Child: None";
                childStatusText.color = Color.gray; // Normal durum
            }
        }

        #region Edit Mode Functions

        /// <summary>
        /// Change rotation action node için düzenleme modunu açar.
        /// Object seçme butonları, target butonları, child seçim butonları ve duration kontrolleri gösterilir.
        /// </summary>
        public override void EditModeOn()
        {
            base.EditModeOn(); // Base class'ın keyboardDisplay'ini göster

            // Object seçme kontrollerini göster
            if (selectObjectInputField != null && selectObjectInputField.gameObject != null)
            {
                selectObjectInputField.gameObject.SetActive(true);
            }

            if (selectObjectButton != null && selectObjectButton.gameObject != null)
            {
                selectObjectButton.gameObject.SetActive(true);
            }

            // Child seçme butonunu göster
            if (selectChildObjectButton != null && selectChildObjectButton.gameObject != null)
            {
                selectChildObjectButton.gameObject.SetActive(true);
            }

            // Child status text'ini göster
            if (childStatusText != null && childStatusText.gameObject != null)
            {
                childStatusText.gameObject.SetActive(true);
            }

            // Target seçme butonunu göster
            if (selectTargetButton != null && selectTargetButton.gameObject != null)
            {
                selectTargetButton.gameObject.SetActive(true);
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

            LogManager.LogSuccess($"EditModeOn: Change rotation action node editing UI shown for: {Model.Title}");
        }

        /// <summary>
        /// Change rotation action node için düzenleme modunu kapatır.
        /// Object seçme butonları, target butonları, child seçim butonları ve duration kontrolleri gizlenir.
        /// </summary>
        public override void EditModeOff()
        {
            base.EditModeOff(); // Base class'ın keyboardDisplay'ini gizle

            // Object seçme kontrollerini gizle
            if (selectObjectInputField != null && selectObjectInputField.gameObject != null)
            {
                selectObjectInputField.gameObject.SetActive(false);
            }

            if (selectObjectButton != null && selectObjectButton.gameObject != null)
            {
                selectObjectButton.gameObject.SetActive(false);
            }

            // Child seçme butonunu gizle
            if (selectChildObjectButton != null && selectChildObjectButton.gameObject != null)
            {
                selectChildObjectButton.gameObject.SetActive(false);
            }

            // Child status text'ini gizle
            if (childStatusText != null && childStatusText.gameObject != null)
            {
                childStatusText.gameObject.SetActive(false);
            }

            // Target seçme butonunu gizle
            if (selectTargetButton != null && selectTargetButton.gameObject != null)
            {
                selectTargetButton.gameObject.SetActive(false);
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

            LogManager.LogSuccess($"EditModeOff: Change rotation action node editing UI hidden for: {Model.Title}");
        }

        #endregion
    }
}