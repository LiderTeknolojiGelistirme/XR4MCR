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
    public class ToolTouchNodePresenter : BaseNodePresenter
    {
        private XRBaseInteractable _toolInteractable;
        private XRBaseInteractable _targetInteractable;
        
        // Child seçim modu (sadece target için)
        private bool _isTargetChildSelectionMode = false;
        
        [Header("Tool Selection")]
        public Button selectToolButton;
        public TMP_InputField selectToolInputField;
        
        [Header("Target Selection")]
        public Button selectTargetButton;
        public TMP_InputField selectTargetInputField;
        public Button selectTargetChildButton;
        public TextMeshProUGUI targetChildStatusText;
        
        public ToolTouchNode ToolTouchNodeModel => Model as ToolTouchNode;

        private void Start()
        {
            // Description'ı sadece boşsa set et (Load'dan gelen değeri korumak için)
            if (string.IsNullOrEmpty(ToolTouchNodeModel.Description))
            {
                ToolTouchNodeModel.Description = "Touch the target object with the selected tool";
            }
        }

        private void Awake()
        {
            if (selectToolButton != null)
            {
                selectToolButton.onClick.AddListener(OnSelectTool);
            }
            
            if (selectTargetButton != null)
            {
                selectTargetButton.onClick.AddListener(OnSelectTarget);
            }
            
            if (selectTargetChildButton != null)
            {
                selectTargetChildButton.onClick.AddListener(OnSelectTargetChildObject);
            }
            
            LogManager.LogSuccess("ToolTouchNodePresenter initialized: " + gameObject.name);
        }

        private void OnDisable()
        {
            if (selectToolButton != null)
            {
                selectToolButton.onClick.RemoveAllListeners();
            }
            
            if (selectTargetButton != null)
            {
                selectTargetButton.onClick.RemoveAllListeners();
            }
            
            if (selectTargetChildButton != null)
            {
                selectTargetChildButton.onClick.RemoveAllListeners();
            }
            
            LogManager.Log("ToolTouchNodePresenter disabled: " + gameObject.name, Color.gray);
        }

        public override void Play()
        {
            base.Play();
            
            // Node zaten tamamlandıysa tekrar kontrol etme
            if (Model.IsCompleted)
            {
                return;
            }
            
            // Node henüz başlamamışsa kontrol etme
            if (!Model.IsStarted)
            {
                return;
            }
            
            if (_toolInteractable != null && _targetInteractable != null)
            {
                // Tool grabbed ise ve hedef nesneye dokunuyorsa
                if (IsToolTouchingTarget())
                {
                    LogManager.LogInteraction($"Tool {_toolInteractable.gameObject.name} is touching target {_targetInteractable.gameObject.name}");
                    LogManager.LogWarning($"About to call CompleteNode - IsStarted: {Model.IsStarted}, IsCompleted: {Model.IsCompleted}");
                    CompleteNode();
                }
            }
        }

        private bool IsToolTouchingTarget()
        {
            if (_toolInteractable == null || _targetInteractable == null)
                return false;

            // Tool ve hedef nesnenin collider'larını al
            Collider toolCollider = _toolInteractable.GetComponent<Collider>();
            Collider targetCollider = _targetInteractable.GetComponent<Collider>();

            if (toolCollider == null || targetCollider == null)
                return false;

            // Collider'ların çarpışıp çarpışmadığını kontrol et
            return toolCollider.bounds.Intersects(targetCollider.bounds);
        }

                public void OnSelectTool()
        {
            try
            {
                if (SystemManager.Selected3DObject == null)
                {
                    LogManager.LogError("Error selecting tool: No object selected");
                    return;
                }

                // Tool seçimi - SetToolParentObject metodunu kullan
                SetToolParentObject(SystemManager.Selected3DObject);
            }
            catch (Exception e)
            {
                LogManager.LogError("Error selecting tool: " + e.Message);
                Debug.LogError(e.Message);
            }
        }

        private void SetToolParentObject(GameObject selectedObject)
        {
            var toolInteractable = selectedObject.GetComponent<XRBaseInteractable>();
            
            if (toolInteractable == null)
            {
                LogManager.LogError("Error selecting tool: Selected object does not have XRBaseInteractable component");
                return;
            }

            // ObjectPresenter'ı al (VIROO nesnelerinde olması gerekir)
            var objectPresenter = selectedObject.GetComponent<ObjectPresenter>();
            if (objectPresenter == null)
            {
                LogManager.LogError("Error selecting tool: Selected object does not have ObjectPresenter component");
                return;
            }

            _toolInteractable = toolInteractable;
            
            // Model'i güncelle (MVP prensibi) - Parent nesneyi seç
            if (ToolTouchNodeModel != null)
            {
                ToolTouchNodeModel.ToolObjectName = selectedObject.name;
                ToolTouchNodeModel.ToolObjectID = objectPresenter.Model.ID; // Parent'ın ID'si
                ToolTouchNodeModel.IsToolChildEnabled = false; // Child devre dışı
            }

            // SetToolObject metodunu çağırarak SystemManager.Selected3DObject'i de güncelle
            SetToolObject(selectedObject);
            
            LogManager.LogInteraction($"Tool selected: {selectedObject.name} (ID: {objectPresenter.Model.ID})");
        }

        public void OnSelectTarget()
        {
            try
            {
                if (SystemManager.Selected3DObject == null)
                {
                    LogManager.LogError("Error selecting target: No object selected");
                    return;
                }

                // Child seçim modundaysa child seçimi yap
                if (_isTargetChildSelectionMode)
                {
                    SelectTargetChild(SystemManager.Selected3DObject);
                    return;
                }

                // Parent seçimi - SetTargetParentObject metodunu kullan
                SetTargetParentObject(SystemManager.Selected3DObject);
            }
            catch (Exception e)
            {
                LogManager.LogError("Error selecting target: " + e.Message);
                Debug.LogError(e.Message);
            }
        }

        private void SetTargetParentObject(GameObject selectedObject)
        {
            var targetInteractable = selectedObject.GetComponent<XRBaseInteractable>();
            
            if (targetInteractable == null)
            {
                LogManager.LogError("Error selecting target: Selected object does not have XRBaseInteractable component");
                return;
            }

            // ObjectPresenter'ı al (VIROO nesnelerinde olması gerekir)
            var objectPresenter = selectedObject.GetComponent<ObjectPresenter>();
            if (objectPresenter == null)
            {
                LogManager.LogError("Error selecting target: Selected object does not have ObjectPresenter component");
                return;
            }

            _targetInteractable = targetInteractable;
            
            // Model'i güncelle (MVP prensibi) - Parent nesneyi seç
            if (ToolTouchNodeModel != null)
            {
                ToolTouchNodeModel.TargetObjectName = selectedObject.name;
                ToolTouchNodeModel.TargetObjectID = objectPresenter.Model.ID; // Parent'ın ID'si
                ToolTouchNodeModel.IsTargetChildEnabled = false; // Child devre dışı
                ToolTouchNodeModel.TargetChildName = null; // Child name'i temizle
            }

            // SetTargetObject metodunu çağırarak SystemManager.Selected3DObject'i de güncelle
            SetTargetObject(selectedObject);
            
            // Child status text'ini güncelle
            UpdateTargetChildStatusText();
            
            LogManager.LogInteraction($"Target parent selected: {selectedObject.name} (ID: {objectPresenter.Model.ID})");
        }
        

        
        public void OnSelectTargetChildObject()
        {
            if (_targetInteractable == null)
            {
                LogManager.LogError("No target parent object selected. Please select a target parent object first.");
                return;
            }

            if (_targetInteractable.transform.parent.name != "VIROO_PrefabContainer")
            {
                LogManager.LogError("Selected target object is not under VIROO_PrefabContainer.");
                return;
            }

            // Child nesneleri kontrol et
            if (_targetInteractable.transform.childCount == 0)
            {
                LogManager.LogError("Selected target object has no child objects.");
                return;
            }

            // Child seçim modunu aktif et
            _isTargetChildSelectionMode = true;

            // Parent'ın collider'ını devre dışı bırak
            var parentCollider = _targetInteractable.GetComponent<Collider>();
            if (parentCollider != null)
            {
                parentCollider.enabled = false;
            }

            // Tüm child'ların collider'larını aktif et (seçim için)
            for (int i = 0; i < _targetInteractable.transform.childCount; i++)
            {
                var childCollider = _targetInteractable.transform.GetChild(i).GetComponent<Collider>();
                if (childCollider != null)
                {
                    childCollider.enabled = true;
                }
            }

            LogManager.LogInteraction("Target child selection mode activated. Please select a child object.");
        }

        private void SelectTargetChild(GameObject selectedObject)
        {
            if (_targetInteractable == null)
            {
                LogManager.LogError("No target parent object selected.");
                return;
            }

            // Parent'ın ObjectPresenter'ını al
            var parentObjectPresenter = _targetInteractable.GetComponent<ObjectPresenter>();
            if (parentObjectPresenter == null)
            {
                LogManager.LogError("Target parent object does not have ObjectPresenter component.");
                return;
            }

            // Seçilen nesnenin parent hierarchy'sinde olup olmadığını recursive olarak kontrol et
            Transform selectedChild = FindChildInHierarchy(_targetInteractable.transform, selectedObject);
            
            if (selectedChild == null)
            {
                LogManager.LogError("Selected object is not found in the target parent hierarchy.");
                return;
            }

            // Configuration zamanında collider'ları restore et (parent açık, child'lar edit için açık)
            RestoreConfigurationColliders(_targetInteractable.transform);

            // Child'ın XRBaseInteractable komponenti var mı kontrol et
            var childInteractable = selectedChild.GetComponent<XRBaseInteractable>();
            if (childInteractable != null)
            {
                _targetInteractable = childInteractable;
            }

            // Model'i güncelle - sadece child bilgilerini güncelle, parent bilgileri aynı kalır
            if (ToolTouchNodeModel != null)
            {
                // TargetObjectName ve TargetObjectID parent olarak kalır, değişmez
                ToolTouchNodeModel.TargetChildName = selectedChild.name; // Child name'ini kaydet
                ToolTouchNodeModel.IsTargetChildEnabled = true; // Child seçimi etkin
            }

            // Input field'ı güncelle - parent name'i göster
            if (selectTargetInputField != null)
            {
                selectTargetInputField.text = $"{parentObjectPresenter.gameObject.name} -> {selectedChild.name}";
            }

            // Child seçim modunu kapat
            _isTargetChildSelectionMode = false;

            // Child status text'ini güncelle
            UpdateTargetChildStatusText();

            LogManager.LogInteraction($"Target child object selected: {selectedChild.name} (Name: {selectedChild.name}, Parent ID: {parentObjectPresenter.Model.ID})");
        }

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

        private void UpdateTargetChildStatusText()
        {
            if (targetChildStatusText == null) return;

            if (ToolTouchNodeModel != null && ToolTouchNodeModel.IsTargetChildEnabled && !string.IsNullOrEmpty(ToolTouchNodeModel.TargetChildName))
            {
                // Child seçili
                targetChildStatusText.text = $"Selected Child: {ToolTouchNodeModel.TargetChildName}";
                targetChildStatusText.color = Color.green; // Seçili durumu vurgulamak için
            }
            else
            {
                // Parent seçili veya child yok
                targetChildStatusText.text = "Selected Child: None";
                targetChildStatusText.color = Color.gray; // Normal durum
            }
        }

        /// <summary>
        /// Model'deki değerleri UI'ya aktarır (yükleme sonrası)
        /// </summary>
        public override void SyncModelToUI()
        {
            // Önce base sınıfın ortak özelliklerini sync et
            base.SyncModelToUI();
            
            if (ToolTouchNodeModel == null) return;

            // Tool nesnesini restore et
            RestoreToolObject();
            
            // Target nesnesini restore et
            RestoreTargetObject();

            // Child status text'ini güncelle
            UpdateTargetChildStatusText();

            LogManager.LogSuccess($"ToolTouchNode UI synced - Tool: {ToolTouchNodeModel.ToolObjectName}, Target: {ToolTouchNodeModel.TargetObjectName}");
        }

        private void RestoreToolObject()
        {
            if (string.IsNullOrEmpty(ToolTouchNodeModel.ToolObjectID)) return;

            GameObject parentObject = FindObjectByID(ToolTouchNodeModel.ToolObjectID);
            if (parentObject == null)
            {
                LogManager.LogWarning($"ToolTouchNode: Could not find tool parent object with ID: {ToolTouchNodeModel.ToolObjectID}");
                return;
            }

            // Tool nesnesini ayarla
            SetToolObject(parentObject);
        }

        private void SetToolObject(GameObject parentObject)
        {
            // Tool için sadece parent seçimi var (child yok)
            var parentInteractable = parentObject.GetComponent<XRBaseInteractable>();
            if (parentInteractable == null)
            {
                LogManager.LogError($"ToolTouchNode: Tool parent object found but no XRBaseInteractable component: {parentObject.name}");
                return;
            }

            _toolInteractable = parentInteractable;

            // SystemManager.Selected3DObject'i güncelle
            SystemManager.Selected3DObject = parentObject;
            
            if (selectToolInputField != null)
            {
                selectToolInputField.text = parentObject.name;
            }
            
            LogManager.LogSuccess($"ToolTouchNode: Tool object set: {parentObject.name} (ID: {ToolTouchNodeModel.ToolObjectID})");
        }

        private void RestoreTargetObject()
        {
            if (string.IsNullOrEmpty(ToolTouchNodeModel.TargetObjectID)) return;

            GameObject parentObject = FindObjectByID(ToolTouchNodeModel.TargetObjectID);
            if (parentObject == null)
            {
                LogManager.LogWarning($"ToolTouchNode: Could not find target parent object with ID: {ToolTouchNodeModel.TargetObjectID}");
                return;
            }

            // Target nesnesini ayarla (parent veya child)
            SetTargetObject(parentObject);
        }

        private void SetTargetObject(GameObject parentObject)
        {
            // Parent interactable'ı ayarla
            var parentInteractable = parentObject.GetComponent<XRBaseInteractable>();
            if (parentInteractable == null)
            {
                LogManager.LogError($"ToolTouchNode: Target parent object found but no XRBaseInteractable component: {parentObject.name}");
                return;
            }

            _targetInteractable = parentInteractable;

            // IsTargetChildEnabled kontrolü
            if (ToolTouchNodeModel.IsTargetChildEnabled)
            {
                // Child restore - önce name ile ara, bulamazsa index ile ara
                Transform childTransform = null;
                
                // 1. Öncelik: TargetChildName ile ara (recursive)
                if (!string.IsNullOrEmpty(ToolTouchNodeModel.TargetChildName))
                {
                    childTransform = FindChildByNameRecursive(parentObject.transform, ToolTouchNodeModel.TargetChildName);
                    if (childTransform != null)
                    {
                        LogManager.Log($"ToolTouchNode: Target child found by name: {childTransform.name}");
                    }
                }
                

                
                if (childTransform != null)
                {
                    // Configuration restore: tüm collider'lar açık
                    RestoreConfigurationColliders(parentObject.transform);

                    // Child'ın XRBaseInteractable komponenti var mı kontrol et
                    var childInteractable = childTransform.GetComponent<XRBaseInteractable>();
                    if (childInteractable != null)
                    {
                        _targetInteractable = childInteractable;
                        LogManager.Log($"ToolTouchNode: Target child has its own XRBaseInteractable: {childTransform.name}");
                    }
                    else
                    {
                        LogManager.Log($"ToolTouchNode: Target child does not have XRBaseInteractable, using parent's: {childTransform.name}");
                    }

                    // SystemManager.Selected3DObject'i child ile güncelle
                    SystemManager.Selected3DObject = childTransform.gameObject;
                    
                    if (selectTargetInputField != null)
                    {
                        selectTargetInputField.text = $"{parentObject.name} -> {childTransform.name}";
                    }
                    
                    LogManager.LogSuccess($"ToolTouchNode: Target child object set: {childTransform.name} (Name: {ToolTouchNodeModel.TargetChildName}, Parent: {parentObject.name})");
                }
                else
                {
                    LogManager.LogError($"ToolTouchNode: Could not find target child '{ToolTouchNodeModel.TargetChildName}' in parent {parentObject.name}");
                    // Child bulunamazsa parent'ı kullan
                    SystemManager.Selected3DObject = parentObject;
                    
                    if (selectTargetInputField != null)
                    {
                        selectTargetInputField.text = $"{parentObject.name} (child not found)";
                    }
                }
                
                // Child status text'ini güncelle
                UpdateTargetChildStatusText();
            }
            else
            {
                // Parent seçimi
                // SystemManager.Selected3DObject'i parent ile güncelle
                SystemManager.Selected3DObject = parentObject;
                
                if (selectTargetInputField != null)
                {
                    selectTargetInputField.text = parentObject.name;
                }
                
                // Child status text'ini güncelle
                UpdateTargetChildStatusText();
                
                LogManager.LogSuccess($"ToolTouchNode: Target parent object set: {parentObject.name} (ID: {ToolTouchNodeModel.TargetObjectID})");
            }
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

        #region Runtime Collider Management

        public override void StartNode()
        {
            base.StartNode();
            
            // Runtime'da child seçili ise collider'ları ayarla
            if (_targetInteractable != null && ToolTouchNodeModel != null && ToolTouchNodeModel.IsTargetChildEnabled)
            {
                ActivateRuntimeChildColliders();
            }
            
            LogManager.LogSuccess($"ToolTouchNode StartNode - IsStarted: {Model.IsStarted}, IsCompleted: {Model.IsCompleted}");
        }

        public override void CompleteNode()
        {
            LogManager.LogSuccess($"ToolTouchNode CompleteNode called - IsCompleted: {Model.IsCompleted}");
            
            // Runtime'dan configuration moduna dön - Model'den parent nesneyi bul
            if (!string.IsNullOrEmpty(ToolTouchNodeModel.TargetObjectID))
            {
                GameObject parentObject = FindObjectByID(ToolTouchNodeModel.TargetObjectID);
                if (parentObject != null)
                {
                    RestoreConfigurationColliders(parentObject.transform);
                    LogManager.LogInteraction($"CompleteNode: Restored configuration colliders - Parent '{parentObject.name}' activated, children deactivated");
                }
            }
            
            base.CompleteNode();
            
            LogManager.LogSuccess($"ToolTouchNode CompleteNode finished - IsStarted: {Model.IsStarted}, IsCompleted: {Model.IsCompleted}");
        }

        public override void OnSkipNode()
        {
            // Runtime'dan configuration moduna dön - Model'den parent nesneyi bul
            if (!string.IsNullOrEmpty(ToolTouchNodeModel.TargetObjectID))
            {
                GameObject parentObject = FindObjectByID(ToolTouchNodeModel.TargetObjectID);
                if (parentObject != null)
                {
                    RestoreConfigurationColliders(parentObject.transform);
                    LogManager.LogInteraction($"OnSkipNode: Restored configuration colliders - Parent '{parentObject.name}' activated, children deactivated");
                }
            }
            
            base.OnSkipNode();
        }

        private void ActivateRuntimeChildColliders()
        {
            // Model'den parent nesneyi bul (TargetObjectID her zaman parent'ın ID'si)
            GameObject parentObject = FindObjectByID(ToolTouchNodeModel.TargetObjectID);
            if (parentObject == null)
            {
                LogManager.LogError($"Runtime: Could not find parent object with ID: {ToolTouchNodeModel.TargetObjectID}");
                return;
            }

            // Parent collider'ı deaktif et
            var parentCollider = parentObject.GetComponent<Collider>();
            if (parentCollider != null)
            {
                parentCollider.enabled = false;
                LogManager.Log($"Runtime: Parent collider deactivated for {parentObject.name}");
            }

            // Tüm child collider'ları aktif et (parent'tan child'lara geçiş için)
            EnableChildCollidersRecursive(parentObject.transform);
            
            LogManager.LogInteraction($"Runtime: All child colliders activated, parent '{parentObject.name}' deactivated");
        }

        #endregion
    }
}