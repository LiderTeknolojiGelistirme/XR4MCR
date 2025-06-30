using System;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Models.Nodes;
using Managers;
using System.Collections.Generic;
using UnityEditor;
using System.Threading.Tasks;
using Presenters;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Presenters.NodePresenters
{
    public class HighlightObjectActionNodePresenter : ActionNodePresenter
    {
        [SerializeField] private TMP_InputField selectedObjectInputField;
        [SerializeField] private Button selectObjectButton;
        [SerializeField] private Button selectChildObjectButton;
        [SerializeField] private TMP_InputField durationInputField;
        [SerializeField] private Button increaseButton;
        [SerializeField] private Button decreaseButton;
        [SerializeField] private Toggle toggleDuration;
        [SerializeField] private TMP_Text childStatusText; // Child status göstermek için

        private GameObject _highlightedObject;
        private ObjectPresenter _objectPresenter;
        private bool _isHighlightActive = false;
        private XRBaseInteractable _targetInteractable;

        // Child seçim modu (GrabNodePresenter'dan alındı)
        private bool _isChildSelectionMode = false;
        private GameObject _selectedParentObject; // Child seçim modunda parent'ı saklamak için

        // Model'e kolay erişim için cast property
        private HighlightObjectActionNode HighlightModel => Model as HighlightObjectActionNode;

        protected override void Awake()
        {
            base.Awake();
            SetActionType(Enums.NodeType.HighlightObjectActionNode);

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
            if (durationInputField != null)
            {
                durationInputField.onValueChanged.AddListener(OnDurationInputChanged);
            }

            if (increaseButton != null)
            {
                increaseButton.onClick.AddListener(OnIncreaseTime);
            }

            if (decreaseButton != null)
            {
                decreaseButton.onClick.AddListener(OnDecreaseTime);
            }

            if (toggleDuration != null)
            {
                toggleDuration.onValueChanged.AddListener(OnToggleValueChanged);
            }
        }

        private void OnDurationInputChanged(string value)
        {
            if (HighlightModel != null && float.TryParse(value, out float duration))
            {
                HighlightModel.Duration = duration;
                LogManager.LogInteraction($"Highlight duration updated via input: {duration}");
            }
        }

        private void OnIncreaseTime()
        {
            LogManager.LogInteraction("Increase highlight time button clicked");
            
            if (HighlightModel != null)
            {
                HighlightModel.Duration += 1f;
                
                // UI'yı güncelle
                if (durationInputField != null)
                {
                    durationInputField.text = Mathf.RoundToInt(HighlightModel.Duration).ToString();
                }
                
                LogManager.LogSuccess($"Highlight duration increased: {HighlightModel.Duration}");
            }
        }

        private void OnDecreaseTime()
        {
            LogManager.LogInteraction("Decrease highlight time button clicked");
            
            if (HighlightModel != null)
            {
                HighlightModel.Duration -= 1f;
                if (HighlightModel.Duration < 1f)
                {
                    HighlightModel.Duration = 1f;
                }
                
                // UI'yı güncelle
                if (durationInputField != null)
                {
                    durationInputField.text = Mathf.RoundToInt(HighlightModel.Duration).ToString();
                }
                
                LogManager.LogSuccess($"Highlight duration decreased: {HighlightModel.Duration}");
            }
        }

        private void OnToggleValueChanged(bool isOn)
        {
            LogManager.LogInteraction($"Duration toggle changed: {isOn}");
            
            if (HighlightModel != null)
            {
                HighlightModel.UseDuration = isOn;
            }
            
            // UI kontrolleri
            if (durationInputField != null) durationInputField.interactable = isOn;
            if (increaseButton != null) increaseButton.interactable = isOn;
            if (decreaseButton != null) decreaseButton.interactable = isOn;
            
            LogManager.LogSuccess($"Duration settings {(isOn ? "enabled" : "disabled")}");
        }

        public void OnSelectObject()
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

                // Child seçim modundaysa child seçimi yap (GrabNodePresenter'dan alındı)
                if (_isChildSelectionMode)
                {
                    SelectChild(SystemManager.Selected3DObject);
                    return;
                }

                // Parent seçimi (GrabNodePresenter'dan alındı)
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

            // XRBaseInteractable bileşenini al (opsiyonel)
            _targetInteractable = selectedObject.GetComponent<XRBaseInteractable>();

            // Input field kontrolü ve güncelleme
            if (selectedObjectInputField != null)
            {
                selectedObjectInputField.text = selectedObject.name;
            }

            // Model'i hemen güncelle (MVP prensibi) - Parent nesneyi seç
            if (HighlightModel != null)
            {
                HighlightModel.SelectedObjectName = selectedObject.name;
                HighlightModel.SelectedObjectID = objectPresenter.Model.ID; // Parent'ın ID'si
                HighlightModel.SelectedChildIndex = -1; // Parent seçili
                HighlightModel.SelectedChildName = null; // Child name'ini temizle
                HighlightModel.IsChildObjectEnabled = false; // Child devre dışı
            }

            // Child object button'unu aktif et
            if (selectChildObjectButton != null)
            {
                selectChildObjectButton.interactable = true;
            }

            // Child status text'ini güncelle
            UpdateChildStatusText();

            // Log: Parent nesne seçildi
            LogManager.LogInteraction($"Highlight: Parent object selected: {selectedObject.name} (ID: {objectPresenter.Model.ID})");
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

            // Parent nesneyi sakla (child seçim modunda kaybolmaması için)
            _selectedParentObject = SystemManager.Selected3DObject;

            // Child seçim modunu aktif et (GrabNodePresenter'dan alındı)
            _isChildSelectionMode = true;

            // Parent'ın collider'ını devre dışı bırak
            var parentCollider = _selectedParentObject.GetComponent<Collider>();
            if (parentCollider != null)
            {
                parentCollider.enabled = false;
            }

            // Tüm child'ların collider'larını aktif et (seçim için)
            for (int i = 0; i < _selectedParentObject.transform.childCount; i++)
            {
                var childCollider = _selectedParentObject.transform.GetChild(i).GetComponent<Collider>();
                if (childCollider != null)
                {
                    childCollider.enabled = true;
                }
            }

            LogManager.LogInteraction("Child selection mode activated. Please select a child object.");
        }

        private void SelectChild(GameObject selectedObject)
        {
            if (_selectedParentObject == null)
            {
                LogManager.LogError("No parent object selected.");
                return;
            }

            // Parent'ın ObjectPresenter'ını al
            var parentObjectPresenter = _selectedParentObject.GetComponent<ObjectPresenter>();
            if (parentObjectPresenter == null)
            {
                LogManager.LogError("Parent object does not have ObjectPresenter component.");
                return;
            }

            // Seçilen nesnenin parent hierarchy'sinde olup olmadığını recursive olarak kontrol et (GrabNodePresenter'dan alındı)
            Transform selectedChild = FindChildInHierarchy(_selectedParentObject.transform, selectedObject);
            
            if (selectedChild == null)
            {
                LogManager.LogError("Selected object is not found in the parent hierarchy.");
                return;
            }

            // Configuration zamanında collider'ları restore et (parent açık, child'lar edit için açık) (GrabNodePresenter'dan alındı)
            RestoreConfigurationColliders(_selectedParentObject.transform);

            // Model'i güncelle - child bilgilerini güncelle (GrabNodePresenter'dan alındı)
            if (HighlightModel != null)
            {
                // SelectedObjectName ve SelectedObjectID parent olarak kalır, değişmez
                HighlightModel.SelectedChildName = selectedChild.name; // Child name'ini kaydet
                HighlightModel.IsChildObjectEnabled = true; // Child seçimi etkin
                
                // Index'i de güncelle (backward compatibility için)
                for (int i = 0; i < _selectedParentObject.transform.childCount; i++)
                {
                    if (_selectedParentObject.transform.GetChild(i) == selectedChild)
                    {
                        HighlightModel.SelectedChildIndex = i;
                        break;
                    }
                }
            }

            // Input field'ı güncelle - parent name'i göster (GrabNodePresenter'dan alındı)
            if (selectedObjectInputField != null)
            {
                selectedObjectInputField.text = $"{parentObjectPresenter.gameObject.name} -> {selectedChild.name}";
            }

            // Child seçim modunu kapat
            _isChildSelectionMode = false;
            _selectedParentObject = null; // Parent referansını temizle

            // Child status text'ini güncelle
            UpdateChildStatusText();

            LogManager.LogInteraction($"Highlight: Child object selected: {selectedChild.name} (Name: {selectedChild.name}, Parent ID: {parentObjectPresenter.Model.ID})");
        }

        // GrabNodePresenter'dan alınan child hierarchy metodları
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

            if (HighlightModel != null && HighlightModel.IsChildObjectEnabled && !string.IsNullOrEmpty(HighlightModel.SelectedChildName))
            {
                // Child seçili
                childStatusText.text = $"Selected Child: {HighlightModel.SelectedChildName}";
                childStatusText.color = Color.green; // Seçili durumu vurgulamak için
            }
            else
            {
                // Parent seçili veya child yok
                childStatusText.text = "Selected Child: None";
                childStatusText.color = Color.gray; // Normal durum
            }
        }

        /// <summary>
        /// Model'deki değerleri UI'ya aktarır (yükleme sonrası)
        /// </summary>
        public override void SyncModelToUI()
        {
            // Önce base sınıfın ortak özelliklerini sync et
            base.SyncModelToUI();
            
            if (HighlightModel == null) return;

            // Duration'ı sync et
            if (durationInputField != null)
            {
                durationInputField.text = Mathf.RoundToInt(HighlightModel.Duration).ToString();
            }

            // Toggle'ı sync et
            if (toggleDuration != null)
            {
                toggleDuration.isOn = HighlightModel.UseDuration;
                OnToggleValueChanged(HighlightModel.UseDuration); // UI kontrollerini güncelle
            }

            // Seçili nesneyi restore et
            RestoreSelectedObject();

            // Child status text'ini güncelle
            UpdateChildStatusText();

            LogManager.LogSuccess($"HighlightObjectActionNode UI synced - Selected: {HighlightModel.SelectedObjectName}, ChildName: {HighlightModel.SelectedChildName}, Duration: {HighlightModel.Duration}, UseDuration: {HighlightModel.UseDuration}");
        }

        private void RestoreSelectedObject()
        {
            if (string.IsNullOrEmpty(HighlightModel.SelectedObjectID)) return;

            GameObject parentObject = FindObjectByID(HighlightModel.SelectedObjectID);
            if (parentObject == null)
            {
                LogManager.LogWarning($"Highlight: Could not find parent object with ID: {HighlightModel.SelectedObjectID}");
                return;
            }

            // Nesneyi ayarla (parent veya child)
            SetSelectedObject(parentObject);
        }

        private void SetSelectedObject(GameObject parentObject)
        {
            // selectChildObjectButton'unu aktif et
            if (selectChildObjectButton != null)
            {
                selectChildObjectButton.interactable = true;
            }

            // Child seçimi varsa onu ayarla (GrabNodePresenter'dan alındı)
            if (HighlightModel.IsChildObjectEnabled && !string.IsNullOrEmpty(HighlightModel.SelectedChildName))
            {
                Transform childTransform = FindChildByNameRecursive(parentObject.transform, HighlightModel.SelectedChildName);
                if (childTransform != null)
                {
                    // Input field'ını güncelle - parent -> child
                    if (selectedObjectInputField != null)
                    {
                        selectedObjectInputField.text = $"{parentObject.name} -> {childTransform.name}";
                    }
                    
                    // Child status text'ini güncelle
                    UpdateChildStatusText();
                    
                    LogManager.LogSuccess($"Highlight: Child object restored: {childTransform.name} (Parent ID: {HighlightModel.SelectedObjectID})");
                }
                else
                {
                    LogManager.LogWarning($"Highlight: Could not find child object: {HighlightModel.SelectedChildName}");
                    
                    // Child bulunamazsa parent moduna geri dön
                    if (HighlightModel != null)
                    {
                        HighlightModel.IsChildObjectEnabled = false;
                        HighlightModel.SelectedChildName = null;
                        HighlightModel.SelectedChildIndex = -1;
                    }
                    
                    if (selectedObjectInputField != null)
                    {
                        selectedObjectInputField.text = parentObject.name;
                    }
                    
                    // Child status text'ini güncelle
                    UpdateChildStatusText();
                }
            }
            else
            {
                // Parent modu
                if (selectedObjectInputField != null)
                {
                    selectedObjectInputField.text = parentObject.name;
                }
                
                // Child status text'ini güncelle
                UpdateChildStatusText();
                
                LogManager.LogSuccess($"Highlight: Parent object restored: {parentObject.name} (ID: {HighlightModel.SelectedObjectID})");
            }
        }

        /// <summary>
        /// VIROO_PrefabContainer altındaki nesneleri ObjectModel.ID ile bulur
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

        protected override async Task PerformActionAsync()
        {
            try
            {
                GameObject targetObject = null;

                // Model'den hedef nesneyi bul
                if (HighlightModel != null && !string.IsNullOrEmpty(HighlightModel.SelectedObjectID))
                {
                    targetObject = FindObjectByID(HighlightModel.SelectedObjectID);
                    
                    // Child seçili ise child'ı al (GrabNodePresenter'dan alındı)
                    if (targetObject != null && HighlightModel.IsChildObjectEnabled && !string.IsNullOrEmpty(HighlightModel.SelectedChildName))
                    {
                        Transform childTransform = FindChildByNameRecursive(targetObject.transform, HighlightModel.SelectedChildName);
                        if (childTransform != null)
                        {
                            targetObject = childTransform.gameObject;
                        }
                    }
                }

                if (targetObject != null)
                {
                    _highlightedObject = targetObject;
                    _objectPresenter = _highlightedObject.GetComponent<ObjectPresenter>();

                    if (_objectPresenter != null)
                    {
                        // Highlight işlemini başlat
                        _isHighlightActive = true;
                        _objectPresenter.EnableOutline();
                        LogManager.LogSuccess($"Highlight started for object: {targetObject.name}");

                        if (HighlightModel.UseDuration)
                        {
                            await Task.Delay(Mathf.RoundToInt(HighlightModel.Duration * 1000));

                            // Süre sonunda highlight'ı kaldır (eğer hala aktifse)
                            if (_isHighlightActive && _objectPresenter != null)
                            {
                                _objectPresenter.DisableOutline();
                                _isHighlightActive = false;
                                LogManager.LogSuccess($"Highlight ended for object: {targetObject.name}");
                            }
                        }
                    }
                    else
                    {
                        LogManager.LogWarning("ObjectPresenter component not found on target object!");
                    }
                }
                else
                {
                    LogManager.LogWarning("Target object not found! Please select an object first.");
                }
            }
            catch (Exception e)
            {
                LogManager.LogError("Error during highlight operation: " + e.Message);
            }
        }

        public override void StopAction()
        {
            base.StopAction();

            if (_highlightedObject != null && _objectPresenter != null)
            {
                _objectPresenter.DisableOutline();
                _isHighlightActive = false;
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (selectObjectButton != null)
                selectObjectButton.onClick.RemoveAllListeners();
                
            if (selectChildObjectButton != null)
                selectChildObjectButton.onClick.RemoveAllListeners();

            if (durationInputField != null)
                durationInputField.onValueChanged.RemoveAllListeners();

            if (increaseButton != null)
                increaseButton.onClick.RemoveAllListeners();

            if (decreaseButton != null)
                decreaseButton.onClick.RemoveAllListeners();

            if (toggleDuration != null)
                toggleDuration.onValueChanged.RemoveAllListeners();

            // Eğer highlight hala aktifse kaldır
            if (_isHighlightActive && _objectPresenter != null)
            {
                _objectPresenter.DisableOutline();
                _isHighlightActive = false;
            }
        }
    }
}
