using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using Viroo.Interactions.Grab;
using Managers;
using Models.Nodes;

namespace Presenters.NodePresenters
{
    public class TouchNodePresenter : BaseNodePresenter
    {
        private XRBaseInteractable _selectedInteractable;
        
        // Child seçim modu (ToolTouchNodePresenter'dan exact copy)
        private bool _isChildSelectionMode = false;
        
        [Header("Object Selection")]
        public Button selectObjectButton;
        public TMP_InputField selectObjectInputField;
        public Button selectChildObjectButton;
        public TextMeshProUGUI childStatusText;
        
        public TouchNode TouchNodeModel => Model as TouchNode;

        private void Start()
        {
            // Description'ı sadece boşsa set et (Load'dan gelen değeri korumak için)
            if (string.IsNullOrEmpty(TouchNodeModel.Description))
            {
                TouchNodeModel.Description = "Touch the selected object";
            }
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
            
            LogManager.LogSuccess("TouchNodePresenter initialized: " + gameObject.name);
        }

        private void OnDisable()
        {
            if (selectObjectButton != null)
            {
                selectObjectButton.onClick.RemoveAllListeners();
            }
            
            if (selectChildObjectButton != null)
            {
                selectChildObjectButton.onClick.RemoveAllListeners();
            }
            
            LogManager.Log("TouchNodePresenter disabled: " + gameObject.name, Color.gray);
        }

        public override void Play()
        {
            base.Play();
            
            // Node zaten tamamlandıysa tekrar kontrol etme (ToolTouchNodePresenter exact copy)
            if (Model.IsCompleted)
            {
                return;
            }
            
            // Node henüz başlamamışsa kontrol etme (ToolTouchNodePresenter exact copy)
            if (!Model.IsStarted)
            {
                return;
            }
            
            if (_selectedInteractable != null)
            {
                if (_selectedInteractable.isHovered)
                {
                    LogManager.LogInteraction("Object is being hovered: " + _selectedInteractable.gameObject.name);
                    CompleteNode();
                }
            }
        }

        public void OnSelectObject()
        {
            try
            {
                if (SystemManager.Selected3DObject == null)
                {
                    LogManager.LogError("Error selecting object: No object selected");
                    return;
                }

                // Child seçim modundaysa child seçimi yap (ToolTouchNodePresenter exact copy)
                if (_isChildSelectionMode)
                {
                    SelectChild(SystemManager.Selected3DObject);
                    return;
                }

                // Parent seçimi (ToolTouchNodePresenter exact copy)
                SetParentObject(SystemManager.Selected3DObject);
            }
            catch (Exception e)
            {
                LogManager.LogError("Error selecting object: " + e.Message);
                Debug.LogError(e.Message);
            }
        }

        private void SetParentObject(GameObject selectedObject)
        {
            var interactable = selectedObject.GetComponent<XRBaseInteractable>();
            
            if (interactable == null)
            {
                LogManager.LogError("Error selecting object: Selected object does not have XRBaseInteractable component");
                return;
            }

            // ObjectPresenter'ı al (VIROO nesnelerinde olması gerekir)
            var objectPresenter = selectedObject.GetComponent<ObjectPresenter>();
            if (objectPresenter == null)
            {
                LogManager.LogError("Error selecting object: Selected object does not have ObjectPresenter component");
                return;
            }

            _selectedInteractable = interactable;
            
            // Model'i güncelle (MVP prensibi) - Parent nesneyi seç
            if (TouchNodeModel != null)
            {
                TouchNodeModel.SelectedObjectName = selectedObject.name;
                TouchNodeModel.SelectedObjectID = objectPresenter.Model.ID; // Parent'ın ID'si
                TouchNodeModel.IsChildObjectEnabled = false; // Child devre dışı
                TouchNodeModel.SelectedChildName = null; // Child name'i temizle
            }

            // SetSelectedObject metodunu çağırarak SystemManager.Selected3DObject'i de güncelle
            SetSelectedObject(selectedObject);
            
            // Child status text'ini güncelle (ToolTouchNodePresenter exact copy)
            UpdateChildStatusText();
            
            LogManager.LogInteraction($"Parent object selected: {selectedObject.name} (ID: {objectPresenter.Model.ID})");
        }
        
        public void OnSelectChildObject()
        {
            if (_selectedInteractable == null)
            {
                LogManager.LogError("No parent object selected. Please select a parent object first.");
                return;
            }

            if (_selectedInteractable.transform.parent.name != "VIROO_PrefabContainer")
            {
                LogManager.LogError("Selected object is not under VIROO_PrefabContainer.");
                return;
            }

            // Child nesneleri kontrol et
            if (_selectedInteractable.transform.childCount == 0)
            {
                LogManager.LogError("Selected object has no child objects.");
                return;
            }

            // Child seçim modunu aktif et (ToolTouchNodePresenter exact copy)
            _isChildSelectionMode = true;

            // Parent'ın collider'ını devre dışı bırak
            var parentCollider = _selectedInteractable.GetComponent<Collider>();
            if (parentCollider != null)
            {
                parentCollider.enabled = false;
            }

            // Tüm child'ların collider'larını aktif et (seçim için)
            for (int i = 0; i < _selectedInteractable.transform.childCount; i++)
            {
                var childCollider = _selectedInteractable.transform.GetChild(i).GetComponent<Collider>();
                if (childCollider != null)
                {
                    childCollider.enabled = true;
                }
            }

            LogManager.LogInteraction("Child selection mode activated. Please select a child object.");
        }

        private void SelectChild(GameObject selectedObject)
        {
            if (_selectedInteractable == null)
            {
                LogManager.LogError("No parent object selected.");
                return;
            }

            // Parent'ın ObjectPresenter'ını al
            var parentObjectPresenter = _selectedInteractable.GetComponent<ObjectPresenter>();
            if (parentObjectPresenter == null)
            {
                LogManager.LogError("Parent object does not have ObjectPresenter component.");
                return;
            }

            // Seçilen nesnenin parent hierarchy'sinde olup olmadığını recursive olarak kontrol et (ToolTouchNodePresenter exact copy)
            Transform selectedChild = FindChildInHierarchy(_selectedInteractable.transform, selectedObject);
            
            if (selectedChild == null)
            {
                LogManager.LogError("Selected object is not found in the parent hierarchy.");
                return;
            }

            // Configuration zamanında collider'ları restore et (parent açık, child'lar edit için açık) (ToolTouchNodePresenter exact copy)
            RestoreConfigurationColliders(_selectedInteractable.transform);

            // Child'ın XRBaseInteractable komponenti var mı kontrol et
            var childInteractable = selectedChild.GetComponent<XRBaseInteractable>();
            if (childInteractable != null)
            {
                _selectedInteractable = childInteractable;
            }

            // Model'i güncelle - child bilgilerini güncelle (ToolTouchNodePresenter exact copy)
            if (TouchNodeModel != null)
            {
                // SelectedObjectName ve SelectedObjectID parent olarak kalır, değişmez
                TouchNodeModel.SelectedChildName = selectedChild.name; // Child name'ini kaydet
                TouchNodeModel.IsChildObjectEnabled = true; // Child seçimi etkin
            }

            // Input field'ı güncelle - parent name'i göster (ToolTouchNodePresenter exact copy)
            if (selectObjectInputField != null)
            {
                selectObjectInputField.text = $"{parentObjectPresenter.gameObject.name} -> {selectedChild.name}";
            }

            // Child seçim modunu kapat
            _isChildSelectionMode = false;

            // Child status text'ini güncelle (ToolTouchNodePresenter exact copy)  
            UpdateChildStatusText();

            LogManager.LogInteraction($"Child object selected: {selectedChild.name} (Name: {selectedChild.name}, Parent ID: {parentObjectPresenter.Model.ID})");
        }

        // ToolTouchNodePresenter'dan entegre edilen metodlar (exact copy)
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

            if (TouchNodeModel != null && TouchNodeModel.IsChildObjectEnabled && !string.IsNullOrEmpty(TouchNodeModel.SelectedChildName))
            {
                // Child seçili
                childStatusText.text = $"Selected Child: {TouchNodeModel.SelectedChildName}";
                childStatusText.color = Color.green; // Seçili durumu vurgulamak için
            }
            else
            {
                // Parent seçili veya child yok
                childStatusText.text = "Selected Child: None";
                childStatusText.color = Color.gray; // Normal durum
            }
        }

        private void ActivateRuntimeChildColliders()
        {
            if (TouchNodeModel == null || !TouchNodeModel.IsChildObjectEnabled || string.IsNullOrEmpty(TouchNodeModel.SelectedObjectID))
                return;

            GameObject parentObject = FindObjectByID(TouchNodeModel.SelectedObjectID);
            if (parentObject == null) return;

            if (!string.IsNullOrEmpty(TouchNodeModel.SelectedChildName))
            {
                Transform childTransform = FindChildByNameRecursive(parentObject.transform, TouchNodeModel.SelectedChildName);
                if (childTransform != null)
                {
                    // Runtime'da: parent kapalı, sadece seçili child açık
                    SetChildCollidersState(parentObject.transform, childTransform);
                }
            }
        }

        /// <summary>
        /// Model'deki değerleri UI'ya aktarır (yükleme sonrası)
        /// </summary>
        public override void SyncModelToUI()
        {
            // Önce base sınıfın ortak özelliklerini sync et
            base.SyncModelToUI();
            
            if (TouchNodeModel == null) return;

            // Seçili nesneyi restore et
            RestoreSelectedObject();

            // Child status text'ini güncelle (ToolTouchNodePresenter exact copy)
            UpdateChildStatusText();

            LogManager.LogSuccess($"TouchNode UI synced - Object: {TouchNodeModel.SelectedObjectName}");
        }

        private void RestoreSelectedObject()
        {
            if (string.IsNullOrEmpty(TouchNodeModel.SelectedObjectID)) return;

            GameObject parentObject = FindObjectByID(TouchNodeModel.SelectedObjectID);
            if (parentObject == null)
            {
                LogManager.LogWarning($"TouchNode: Could not find parent object with ID: {TouchNodeModel.SelectedObjectID}");
                return;
            }

            // Nesneyi ayarla (parent veya child)
            SetSelectedObject(parentObject);
        }

        private void SetSelectedObject(GameObject parentObject)
        {
            // Parent nesneyi ayarla
            var parentInteractable = parentObject.GetComponent<XRBaseInteractable>();
            if (parentInteractable == null)
            {
                LogManager.LogError($"TouchNode: Parent object found but no XRBaseInteractable component: {parentObject.name}");
                return;
            }

            _selectedInteractable = parentInteractable;

            // Child seçimi varsa onu ayarla (ToolTouchNodePresenter exact copy)
            if (TouchNodeModel.IsChildObjectEnabled && !string.IsNullOrEmpty(TouchNodeModel.SelectedChildName))
            {
                Transform childTransform = FindChildByNameRecursive(parentObject.transform, TouchNodeModel.SelectedChildName);
                if (childTransform != null)
                {
                    // Child'ın XRBaseInteractable komponenti var mı kontrol et
                    var childInteractable = childTransform.GetComponent<XRBaseInteractable>();
                    if (childInteractable != null)
                    {
                        _selectedInteractable = childInteractable;
                        LogManager.Log($"TouchNode: Using child's XRBaseInteractable: {childTransform.name}");
                    }
                    else
                    {
                        LogManager.Log($"TouchNode: Child does not have XRBaseInteractable, using parent's: {childTransform.name}");
                    }
                    
                    // Input field'ını güncelle - parent -> child
                    if (selectObjectInputField != null)
                    {
                        selectObjectInputField.text = $"{parentObject.name} -> {childTransform.name}";
                    }
                    
                    // Child status text'ini güncelle (ToolTouchNodePresenter exact copy)
                    UpdateChildStatusText();
                    
                    LogManager.LogSuccess($"TouchNode: Child object restored: {childTransform.name} (Parent ID: {TouchNodeModel.SelectedObjectID})");
                }
                else
                {
                    LogManager.LogWarning($"TouchNode: Could not find child object: {TouchNodeModel.SelectedChildName}");
                    
                    // Child bulunamazsa parent moduna geri dön
                    if (TouchNodeModel != null)
                    {
                        TouchNodeModel.IsChildObjectEnabled = false;
                        TouchNodeModel.SelectedChildName = null;
                    }
                    
                    if (selectObjectInputField != null)
                    {
                        selectObjectInputField.text = parentObject.name;
                    }
                    
                    // Child status text'ini güncelle (ToolTouchNodePresenter exact copy)
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
                
                // Child status text'ini güncelle (ToolTouchNodePresenter exact copy)
                UpdateChildStatusText();
                
                LogManager.LogSuccess($"TouchNode: Parent object restored: {parentObject.name} (ID: {TouchNodeModel.SelectedObjectID})");
            }

            // SystemManager.Selected3DObject'i güncelle
            SystemManager.Selected3DObject = parentObject;
            
            // Child button'ı aktif et
            if (selectChildObjectButton != null)
            {
                selectChildObjectButton.interactable = true;
            }
        }

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

        public override void StartNode()
        {
            base.StartNode();
            
            // Runtime'da child seçili ise collider'ları ayarla (ToolTouchNodePresenter exact copy)
            if (_selectedInteractable != null && TouchNodeModel != null && TouchNodeModel.IsChildObjectEnabled)
            {
                ActivateRuntimeChildColliders();
            }
            
            LogManager.LogScenario("TouchNode started: " + gameObject.name);
        }

        public override void CompleteNode()
        {
            LogManager.LogSuccess("TouchNode completed: " + gameObject.name);
            
            // Runtime'dan configuration moduna dön (ToolTouchNodePresenter exact copy)
            if (!string.IsNullOrEmpty(TouchNodeModel.SelectedObjectID))
            {
                GameObject parentObject = FindObjectByID(TouchNodeModel.SelectedObjectID);
                if (parentObject != null)
                {
                    RestoreConfigurationColliders(parentObject.transform);
                }
            }

            base.CompleteNode();
        }

        public override void OnSkipNode()
        {
            // Runtime'dan configuration moduna dön (ToolTouchNodePresenter exact copy)
            if (!string.IsNullOrEmpty(TouchNodeModel.SelectedObjectID))
            {
                GameObject parentObject = FindObjectByID(TouchNodeModel.SelectedObjectID);
                if (parentObject != null)
                {
                    RestoreConfigurationColliders(parentObject.transform);
                }
            }
            
            base.OnSkipNode();
        }
    }
}