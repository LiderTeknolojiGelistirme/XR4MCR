using System;
using Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Managers;
using Models.Nodes;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Presenters.NodePresenters
{
    public class GrabNodePresenter : BaseNodePresenter
    {
        public GameObject _simpleInteractable;

        // Child seçim modu (TouchNodePresenter'dan exact copy)
        private bool _isChildSelectionMode = false;

        public Button selectObjectButton;
        public Button selectTargetButton;
        public Button selectChildObjectButton;
        public TMP_InputField selectObjectInputField;
        public GameObject selectTargetGhostPrefab;
        public TMP_Text childStatusText;
        private GameObject _instantiatedTargetGhostGameObject;
        

        private bool _holdingTarget = false;

        public GrabNode GrabNodeModel => Model as GrabNode;

        private void Awake()
        {
            
            selectObjectButton.onClick.AddListener(OnSelectObject);
            selectTargetButton.onClick.AddListener(OnSelectTarget);
            selectChildObjectButton?.onClick.AddListener(OnSelectChildObject);
        }
        private void Start()
        {
            // Description'ı sadece boşsa set et (Load'dan gelen değeri korumak için)
            if (string.IsNullOrEmpty(Model.Description))
            {
                Model.Description = "Grab the selected object and drop it to the target position";
            }
        }

        private void OnDisable()
        {
            selectObjectButton.onClick.RemoveAllListeners();
            selectTargetButton.onClick.RemoveAllListeners();
            selectChildObjectButton?.onClick.RemoveAllListeners();
            if (_instantiatedTargetGhostGameObject != null)
            {
                Destroy(_instantiatedTargetGhostGameObject);
            }
        }

        protected override void Update()
        {
            base.Update();
            if (_holdingTarget)
            {
                if (XRInputManager.GetRawTriggerState())
                {
                    var parent = GameObject.Find("Root").transform;
                    Debug.Log(parent.name);
                    _instantiatedTargetGhostGameObject.transform.parent = parent;
                    _holdingTarget = false;
                    
                    // Target pozisyonunu model'e kaydet
                    if (GrabNodeModel != null)
                    {
                        GrabNodeModel.TargetPosX = _instantiatedTargetGhostGameObject.transform.position.x;
                        GrabNodeModel.TargetPosY = _instantiatedTargetGhostGameObject.transform.position.y;
                        GrabNodeModel.TargetPosZ = _instantiatedTargetGhostGameObject.transform.position.z;
                        GrabNodeModel.HasTargetPosition = true;  // Target pozisyonu set edildi
                        LogManager.LogSuccess($"Target position saved: {_instantiatedTargetGhostGameObject.transform.position}");
                    }
                }
            }
        }


        public override void ActivateNode()
        {
            base.ActivateNode();
        }

        public override void StartNode()
        {
            Debug.Log("Start GrabNode");
            base.StartNode();
            
            // Runtime'da child seçili ise collider'ları ayarla (TouchNodePresenter exact copy)
            if (_simpleInteractable != null && GrabNodeModel != null && GrabNodeModel.IsChildObjectEnabled)
            {
                ActivateRuntimeChildColliders();
            }
            
            LogManager.LogScenario("GrabNode started: " + gameObject.name);
        }

        public override void CompleteNode()
        {
            LogManager.LogSuccess("GrabNode completed: " + gameObject.name);
            
            // Runtime'dan configuration moduna dön (TouchNodePresenter exact copy)
            if (!string.IsNullOrEmpty(GrabNodeModel.SelectedObjectID))
            {
                GameObject parentObject = FindObjectByID(GrabNodeModel.SelectedObjectID);
                if (parentObject != null)
                {
                    RestoreConfigurationColliders(parentObject.transform);
                }
            }

            base.CompleteNode();
        }

        public override void OnSkipNode()
        {
            // Runtime'dan configuration moduna dön (TouchNodePresenter exact copy)
            if (!string.IsNullOrEmpty(GrabNodeModel.SelectedObjectID))
            {
                GameObject parentObject = FindObjectByID(GrabNodeModel.SelectedObjectID);
                if (parentObject != null)
                {
                    RestoreConfigurationColliders(parentObject.transform);
                }
            }
            
            base.OnSkipNode();
        }

        public override void Play()
        {
            base.Play();

            if (_simpleInteractable != null && _instantiatedTargetGhostGameObject != null)
            {
                GameObject objectToCheck = _simpleInteractable;
                
                // Eğer child object seçilmişse, child object'in pozisyonunu kontrol et
                if (GrabNodeModel != null && GrabNodeModel.IsChildObjectEnabled && !string.IsNullOrEmpty(GrabNodeModel.SelectedChildName))
                {
                    Transform childTransform = FindChildByNameRecursive(_simpleInteractable.transform, GrabNodeModel.SelectedChildName);
                    if (childTransform != null)
                    {
                        objectToCheck = childTransform.gameObject;
                        LogManager.Log($"Checking child object position: {childTransform.name}");
                    }
                    else
                    {
                        LogManager.LogWarning($"Child object not found: {GrabNodeModel.SelectedChildName}");
                        return; // Child bulunamazsa kontrol etme
                    }
                }
                else
                {
                    LogManager.Log($"Checking parent object position: {_simpleInteractable.name}");
                }

                // Seçili nesnenin (parent veya child) pozisyonunu target ghost ile karşılaştır
                if (Vector3.Distance(objectToCheck.transform.position,
                        _instantiatedTargetGhostGameObject.transform.position) < 1f)
                {
                    string completedObjectName = GrabNodeModel?.IsChildObjectEnabled == true ? 
                        GrabNodeModel.SelectedChildName : _simpleInteractable.name;
                    
                    LogManager.LogSuccess($"Grab and drop complete: {completedObjectName}");
                    CompleteNode();
                }
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

                // Child seçim modundaysa child seçimi yap (TouchNodePresenter exact copy)
                if (_isChildSelectionMode)
                {
                    SelectChild(SystemManager.Selected3DObject);
                    return;
                }

                // Parent seçimi (TouchNodePresenter exact copy)
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

            _simpleInteractable = selectedObject;
            selectTargetButton.interactable = true;

            // Input field'ı güncelle
            if (selectObjectInputField != null)
            {
                selectObjectInputField.text = _simpleInteractable.name;
            }

            // Model'i güncelle - Parent nesneyi seç
            if (GrabNodeModel != null)
            {
                GrabNodeModel.SelectedObjectName = _simpleInteractable.name;
                GrabNodeModel.SelectedObjectID = objectPresenter.Model.ID; // Parent'ın ID'si
                GrabNodeModel.SelectedChildIndex = -1; // Parent seçili
                GrabNodeModel.SelectedChildName = null; // Child name'ini temizle
                GrabNodeModel.IsChildObjectEnabled = false; // Child devre dışı
            }

            // Target ghost nesnesini güncelle (eğer zaten varsa)
            if (_instantiatedTargetGhostGameObject != null)
            {
                var go = Instantiate(_simpleInteractable.GetComponent<InteractionHelper>().targetGhostPrefab,
                    GameObject.Find("Root").transform);
                go.transform.position = _instantiatedTargetGhostGameObject.transform.position;
                go.transform.rotation = _instantiatedTargetGhostGameObject.transform.rotation;
                go.transform.localScale = _instantiatedTargetGhostGameObject.transform.localScale;
                Destroy(_instantiatedTargetGhostGameObject);
                _instantiatedTargetGhostGameObject = go;
            }
            selectChildObjectButton.interactable = true;

            // Child status text'ini güncelle (TouchNodePresenter exact copy)
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

            // Child seçim modunu aktif et (TouchNodePresenter exact copy)
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

            // Seçilen nesnenin parent hierarchy'sinde olup olmadığını recursive olarak kontrol et (TouchNodePresenter exact copy)
            Transform selectedChild = FindChildInHierarchy(_simpleInteractable.transform, selectedObject);
            
            if (selectedChild == null)
            {
                LogManager.LogError("Selected object is not found in the parent hierarchy.");
                return;
            }

            // Configuration zamanında collider'ları restore et (parent açık, child'lar edit için açık) (TouchNodePresenter exact copy)
            RestoreConfigurationColliders(_simpleInteractable.transform);

            // Model'i güncelle - child bilgilerini güncelle (TouchNodePresenter exact copy)
            if (GrabNodeModel != null)
            {
                // SelectedObjectName ve SelectedObjectID parent olarak kalır, değişmez
                GrabNodeModel.SelectedChildName = selectedChild.name; // Child name'ini kaydet
                GrabNodeModel.IsChildObjectEnabled = true; // Child seçimi etkin
                
                // Index'i de güncelle (backward compatibility için)
                for (int i = 0; i < _simpleInteractable.transform.childCount; i++)
                {
                    if (_simpleInteractable.transform.GetChild(i) == selectedChild)
                    {
                        GrabNodeModel.SelectedChildIndex = i;
                        break;
                    }
                }
            }

            // Input field'ı güncelle - parent name'i göster (TouchNodePresenter exact copy)
            if (selectObjectInputField != null)
            {
                selectObjectInputField.text = $"{parentObjectPresenter.gameObject.name} -> {selectedChild.name}";
            }

            // Child seçim modunu kapat
            _isChildSelectionMode = false;

            // Child status text'ini güncelle (TouchNodePresenter exact copy)  
            UpdateChildStatusText();

            LogManager.LogInteraction($"Child object selected: {selectedChild.name} (Name: {selectedChild.name}, Parent ID: {parentObjectPresenter.Model.ID})");
        }

        // TouchNodePresenter'dan entegre edilen metodlar (exact copy)
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

            if (GrabNodeModel != null && GrabNodeModel.IsChildObjectEnabled && !string.IsNullOrEmpty(GrabNodeModel.SelectedChildName))
            {
                // Child seçili
                childStatusText.text = $"Selected Child: {GrabNodeModel.SelectedChildName}";
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
            if (GrabNodeModel == null || !GrabNodeModel.IsChildObjectEnabled || string.IsNullOrEmpty(GrabNodeModel.SelectedObjectID))
                return;

            GameObject parentObject = FindObjectByID(GrabNodeModel.SelectedObjectID);
            if (parentObject == null) return;

            if (!string.IsNullOrEmpty(GrabNodeModel.SelectedChildName))
            {
                Transform childTransform = FindChildByNameRecursive(parentObject.transform, GrabNodeModel.SelectedChildName);
                if (childTransform != null)
                {
                    // Runtime'da: parent kapalı, sadece seçili child açık
                    SetChildCollidersState(parentObject.transform, childTransform);
                }
            }
        }

        private void OnSelectTarget()
        {
            LogManager.LogInteraction("Select target position button clicked");
            
            if (_simpleInteractable == null)
            {
                LogManager.LogWarning("No object selected for grab operation");
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
                // Yeni target ghost oluştur
                _instantiatedTargetGhostGameObject = Instantiate(
                    interactionHelper.targetGhostPrefab,
                    XRInputManager.xrRayInteractor.transform);

                _holdingTarget = true;
                LogManager.LogSuccess("Target ghost created and positioning started");
            }
            else
            {
                // Mevcut target ghost'u yeniden konumlandır
                _instantiatedTargetGhostGameObject.transform.SetParent(XRInputManager.xrRayInteractor.transform);
                _instantiatedTargetGhostGameObject.transform.localPosition = Vector3.zero;
                _holdingTarget = true;
                LogManager.LogSuccess("Target ghost repositioning started");
            }
        }

        /// <summary>
        /// Model'deki değerleri UI'ya aktarır (yükleme sonrası)
        /// </summary>
        public override void SyncModelToUI()
        {
            // Önce base sınıfın ortak özelliklerini sync et
            base.SyncModelToUI();
            
            if (GrabNodeModel == null) return;

            // Seçili nesneyi restore et
            RestoreSelectedObject();

            // Child status text'ini güncelle (TouchNodePresenter exact copy)
            UpdateChildStatusText();

            LogManager.LogSuccess($"GrabNode UI synced - Selected: {GrabNodeModel.SelectedObjectName}, ChildName: {GrabNodeModel.SelectedChildName}");
        }

        private void RestoreSelectedObject()
        {
            if (string.IsNullOrEmpty(GrabNodeModel.SelectedObjectID)) return;

            GameObject parentObject = FindObjectByID(GrabNodeModel.SelectedObjectID);
            if (parentObject == null)
            {
                LogManager.LogWarning($"GrabNode: Could not find parent object with ID: {GrabNodeModel.SelectedObjectID}");
                return;
            }

            // Nesneyi ayarla (parent veya child)
            SetSelectedObject(parentObject);
        }

        private void SetSelectedObject(GameObject parentObject)
        {
            _simpleInteractable = parentObject;
            selectTargetButton.interactable = true;
            selectChildObjectButton.interactable = true;

            // Child seçimi varsa onu ayarla (TouchNodePresenter exact copy)
            if (GrabNodeModel.IsChildObjectEnabled && !string.IsNullOrEmpty(GrabNodeModel.SelectedChildName))
            {
                Transform childTransform = FindChildByNameRecursive(parentObject.transform, GrabNodeModel.SelectedChildName);
                if (childTransform != null)
                {
                    // Input field'ını güncelle - parent -> child
                    if (selectObjectInputField != null)
                    {
                        selectObjectInputField.text = $"{parentObject.name} -> {childTransform.name}";
                    }
                    
                    // Child status text'ini güncelle (TouchNodePresenter exact copy)
                    UpdateChildStatusText();
                    
                    LogManager.LogSuccess($"GrabNode: Child object restored: {childTransform.name} (Parent ID: {GrabNodeModel.SelectedObjectID})");
                }
                else
                {
                    LogManager.LogWarning($"GrabNode: Could not find child object: {GrabNodeModel.SelectedChildName}");
                    
                    // Child bulunamazsa parent moduna geri dön
                    if (GrabNodeModel != null)
                    {
                        GrabNodeModel.IsChildObjectEnabled = false;
                        GrabNodeModel.SelectedChildName = null;
                        GrabNodeModel.SelectedChildIndex = -1;
                    }
                    
                    if (selectObjectInputField != null)
                    {
                        selectObjectInputField.text = parentObject.name;
                    }
                    
                    // Child status text'ini güncelle (TouchNodePresenter exact copy)
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
                
                // Child status text'ini güncelle (TouchNodePresenter exact copy)
                UpdateChildStatusText();
                
                LogManager.LogSuccess($"GrabNode: Parent object restored: {parentObject.name} (ID: {GrabNodeModel.SelectedObjectID})");
            }

            // Target pozisyonunu restore et
            if (GrabNodeModel.HasTargetPosition)
            {
                Vector3 targetPosition = new Vector3(GrabNodeModel.TargetPosX, GrabNodeModel.TargetPosY, GrabNodeModel.TargetPosZ);
                
                var interactionHelper = _simpleInteractable.GetComponent<InteractionHelper>();
                if (interactionHelper != null && interactionHelper.targetGhostPrefab != null)
                {
                    // Eski target ghost varsa temizle
                    if (_instantiatedTargetGhostGameObject != null)
                    {
                        Destroy(_instantiatedTargetGhostGameObject);
                    }
                    
                    // Yeni target ghost oluştur ve pozisyonunu ayarla
                    _instantiatedTargetGhostGameObject = Instantiate(
                        interactionHelper.targetGhostPrefab,
                        GameObject.Find("Root").transform);
                    _instantiatedTargetGhostGameObject.transform.position = targetPosition;
                    
                    LogManager.LogSuccess($"GrabNode: Target ghost restored at position: {targetPosition}");
                }
                else
                {
                    LogManager.LogError($"GrabNode: InteractionHelper or targetGhostPrefab not found on {_simpleInteractable.name}");
                }
            }
            else
            {
                LogManager.Log($"GrabNode: No target position to restore for {GrabNodeModel.SelectedObjectName}");
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
    }
}