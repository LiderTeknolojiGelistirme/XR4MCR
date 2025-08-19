using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Managers;
using Models.Nodes;
using Unity.VisualScripting;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using _3rd_Party.Outline;

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
        
        // GrabInstructorUI prefab'ı ve instance'ı
        public GameObject grabInstructorUIPrefab;
        private GameObject _instantiatedInstructorUI;
        
        private Vector3 _initialPosition;
        private Quaternion _initialRotation;
        public Toggle repeatable;
        public Toggle snapToTarget;

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
            if (_instantiatedInstructorUI != null)
            {
                Destroy(_instantiatedInstructorUI);
            }
        }

                protected override void Update()
        {
            base.Update();
            
            // Instruction UI'ı sürekli kullanıcıya baktır (billboard effect)
            UpdateInstructorUIOrientation();
            
            if (_holdingTarget)
            {
                if (XRInputManager.GetRawTriggerState())
                {
                    var parent = GameObject.Find("Root").transform;
                    Debug.Log(parent.name);
                    _instantiatedTargetGhostGameObject.transform.parent = parent;
                    _holdingTarget = false;

                    // Artık burada pozisyon kaydetmiyoruz - save butonuna basılınca kaydedilecek
                    // Instruction UI zaten ghost'un child'ı olduğu için otomatik olarak beraber hareket edecek
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
            _initialPosition = _simpleInteractable.transform.position;
            _initialRotation = _simpleInteractable.transform.rotation;

            // Seçili nesneyi grab edilebilir yap (parent veya child)
            GameObject targetObject = GetTargetObject();
            if (targetObject != null)
            {
                var grabInteractable = targetObject.GetComponent<XRGrabInteractable>();
                if (grabInteractable == null)
                {
                    grabInteractable = targetObject.AddComponent<XRGrabInteractable>();
                }

                grabInteractable.enabled = true;
            }

            // XRGrabInteractable eklendikten SONRA collider durumlarını kontrol et ve düzelt
            if (_simpleInteractable != null && GrabNodeModel != null)
            {
                if (GrabNodeModel.IsChildObjectEnabled)
                {
                    // Child seçili ise: parent collider kapalı kalmalı, sadece child açık
                    ActivateRuntimeChildColliders();
                }
                else
                {
                    // Parent seçili ise: parent collider açık olmalı
                    GameObject parentObject = FindObjectByID(GrabNodeModel.SelectedObjectID);
                    if (parentObject != null)
                    {
                        var parentCollider = parentObject.GetComponent<Collider>();
                        if (parentCollider != null)
                        {
                            parentCollider.enabled = true;
                        }
                    }
                }
            }

            // Target ghost'u da grab edilebilir yap
            if (selectTargetGhostPrefab != null)
            {
                var targetGrabInteractable = selectTargetGhostPrefab.GetComponent<XRGrabInteractable>();
                if (targetGrabInteractable != null)
                {
                    targetGrabInteractable.enabled = true;
                }
            }

            // Target ghost nesnesini görünür yap (eğer varsa)
            ShowTargetGhost();

            // Seçili objenin outline'ını aktif et
            EnableObjectOutline();
        }

        public override void CompleteNode()
        {
            // Seçili objenin outline'ını deaktif et
            DisableObjectOutline();

            // Target ghost nesnesini gizle
            HideTargetGhost();
            
            // Instructor UI'ı da gizle
            HideInstructorUI();

            // Nesneyi ilk konumuna döndür
            //if (_simpleInteractable != null)
            //{
                //if (repeatable != null && repeatable.isOn)
                //{
                //    UpdatePositionAndRotationAsInitial(_initialPosition, _initialRotation);
                //}
            //}
            //else
            //{
            //    LogManager.LogWarning("Cannot return object to initial position: _simpleInteractable is null");
            //}

            // Seçili nesnenin grab özelliğini devre dışı bırak (parent veya child)
            GameObject targetObject = GetTargetObject();
            if (targetObject != null)
            {
                var grabInteractable = targetObject.GetComponent<XRGrabInteractable>();
                if (grabInteractable != null)
                {
                    grabInteractable.enabled = false;
                }
            }

            // Runtime'dan configuration moduna dön (TouchNodePresenter exact copy)
            if (!string.IsNullOrEmpty(GrabNodeModel.SelectedObjectID))
            {
                GameObject parentObject = FindObjectByID(GrabNodeModel.SelectedObjectID);
                if (parentObject != null)
                {
                    RestoreConfigurationColliders(parentObject.transform);
                }
            }

            // Target ghost'un grab özelliğini devre dışı bırak
            if (selectTargetGhostPrefab != null)
            {
                var targetGrabInteractable = selectTargetGhostPrefab.GetComponent<XRGrabInteractable>();
                if (targetGrabInteractable != null)
                {
                    targetGrabInteractable.enabled = false;
                }
            }

            base.CompleteNode();
        }

        public override void OnSkipNode()
        {
            // Target ghost nesnesini gizle
            HideTargetGhost();
            
            // Instructor UI'ı da gizle
            HideInstructorUI();

            // Seçili nesnenin grab özelliğini devre dışı bırak (parent veya child)
            GameObject targetObject = GetTargetObject();
            if (targetObject != null)
            {
                var grabInteractable = targetObject.GetComponent<XRGrabInteractable>();
                if (grabInteractable != null)
                {
                    grabInteractable.enabled = false;
                }
            }

            // Nesneyi ilk konumuna döndür
            if (_simpleInteractable != null)
            {
                _simpleInteractable.transform.position = Vector3.zero;
            }
            else
            {
                LogManager.LogWarning("Cannot return object to initial position: _simpleInteractable is null");
            }

            // Runtime'dan configuration moduna dön (TouchNodePresenter exact copy)
            if (!string.IsNullOrEmpty(GrabNodeModel.SelectedObjectID))
            {
                GameObject parentObject = FindObjectByID(GrabNodeModel.SelectedObjectID);
                if (parentObject != null)
                {
                    RestoreConfigurationColliders(parentObject.transform);
                }
            }

            // Target ghost'un grab özelliğini devre dışı bırak
            if (selectTargetGhostPrefab != null)
            {
                var targetGrabInteractable = selectTargetGhostPrefab.GetComponent<XRGrabInteractable>();
                if (targetGrabInteractable != null)
                {
                    targetGrabInteractable.enabled = false;
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
                if (GrabNodeModel != null && GrabNodeModel.IsChildObjectEnabled &&
                    !string.IsNullOrEmpty(GrabNodeModel.SelectedChildName))
                {
                    Transform childTransform =
                        FindChildByNameRecursive(_simpleInteractable.transform, GrabNodeModel.SelectedChildName);
                    if (childTransform != null)
                    {
                        objectToCheck = childTransform.gameObject;
                    }
                    else
                    {
                        return; // Child bulunamazsa kontrol etme
                    }
                }

                // Grip tuşu basılı değilse node'u complete e
                

                // Seçili nesnenin (parent veya child) pozisyonunu target ghost ile karşılaştır
                if (Vector3.Distance(objectToCheck.transform.position,
                        _instantiatedTargetGhostGameObject.transform.position) < .35f && !XRInputManager.IsGripPressed())
                {
                    string completedObjectName = GrabNodeModel?.IsChildObjectEnabled == true
                        ? GrabNodeModel.SelectedChildName
                        : _simpleInteractable.name;

                    if (snapToTarget)
                    {
                        objectToCheck.transform.position = _instantiatedTargetGhostGameObject.transform.position;
                        objectToCheck.transform.rotation = _instantiatedTargetGhostGameObject.transform.rotation;
                    }

                    CompleteNode();
                }
            }
        }

        private void UpdatePositionAndRotationAsInitial(Vector3 _initialPosition, Quaternion _initialRotation)
        {
            _simpleInteractable.transform.position = _initialPosition;
            _simpleInteractable.transform.rotation = _initialRotation;
        }

        private void OnSelectObject()
        {
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
                // Eski pozisyon ve rotasyonu kaydet
                Vector3 oldPosition = _instantiatedTargetGhostGameObject.transform.position;
                Quaternion oldRotation = _instantiatedTargetGhostGameObject.transform.rotation;
                Vector3 oldScale = _instantiatedTargetGhostGameObject.transform.localScale;
                
                // Instruction UI'ı geçici olarak koru (ghost destroy edilmeden önce)
                GameObject tempInstructorUI = null;
                Vector3 instructorUILocalPos = Vector3.zero;
                Quaternion instructorUILocalRot = Quaternion.identity;
                Vector3 instructorUILocalScale = Vector3.one;
                bool instructorUIWasActive = false;
                
                if (_instantiatedInstructorUI != null)
                {
                    // Instruction UI'ın local transform bilgilerini kaydet
                    instructorUILocalPos = _instantiatedInstructorUI.transform.localPosition;
                    instructorUILocalRot = _instantiatedInstructorUI.transform.localRotation;
                    instructorUILocalScale = _instantiatedInstructorUI.transform.localScale;
                    instructorUIWasActive = _instantiatedInstructorUI.activeInHierarchy;
                    
                    // Instruction UI'ı geçici olarak Root'a taşı
                    _instantiatedInstructorUI.transform.SetParent(GameObject.Find("Root").transform);
                    tempInstructorUI = _instantiatedInstructorUI;
                    _instantiatedInstructorUI = null; // Referansı temizle
                }
                
                // Eski ghost'u destroy et
                Destroy(_instantiatedTargetGhostGameObject);
                
                // Yeni ghost oluştur
                var go = Instantiate(_simpleInteractable.GetComponent<InteractionHelper>().targetGhostPrefab,
                    GameObject.Find("Root").transform);
                go.transform.position = oldPosition;
                go.transform.rotation = oldRotation;
                // Yeni nesnenin kendi orijinal scale'ini koru - eski scale'i uygulamıyoruz!
                // go.transform.localScale = oldScale; // Bu satırı kaldırıyoruz
                _instantiatedTargetGhostGameObject = go;
                
                // Instruction UI'ı yeni ghost'un child'ı yap
                if (tempInstructorUI != null)
                {
                    _instantiatedInstructorUI = tempInstructorUI;
                    _instantiatedInstructorUI.transform.SetParent(_instantiatedTargetGhostGameObject.transform);
                    _instantiatedInstructorUI.transform.localPosition = instructorUILocalPos;
                    _instantiatedInstructorUI.transform.localRotation = instructorUILocalRot;
                    
                    // Yeni ghost'un scale'ine göre instruction UI scale'ini yeniden hesapla
                    Vector3 newGhostScale = _instantiatedTargetGhostGameObject.transform.localScale;
                    Vector3 newCorrectedScale = new Vector3(
                        1f / newGhostScale.x, 
                        1f / newGhostScale.y, 
                        1f / newGhostScale.z
                    );
                    _instantiatedInstructorUI.transform.localScale = newCorrectedScale;
                    _instantiatedInstructorUI.SetActive(instructorUIWasActive);
                }
            }

            selectChildObjectButton.interactable = true;

            // Child status text'ini güncelle (TouchNodePresenter exact copy)
            UpdateChildStatusText();
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

            // Target ghost'u child objenin kendi ghost'u ile güncelle
            UpdateTargetGhostForChildSelection(selectedChild.gameObject);

            // Child status text'ini güncelle (TouchNodePresenter exact copy)  
            UpdateChildStatusText();
        }

        /// <summary>
        /// Save işlemi sırasında çağrılır - target ghost pozisyon ve rotasyonunu model'e kaydeder
        /// </summary>
        public void UpdateTargetPositionForSave()
        {
            if (_instantiatedTargetGhostGameObject != null && GrabNodeModel != null)
            {
                // Pozisyon bilgilerini kaydet
                GrabNodeModel.TargetPosX = _instantiatedTargetGhostGameObject.transform.position.x;
                GrabNodeModel.TargetPosY = _instantiatedTargetGhostGameObject.transform.position.y;
                GrabNodeModel.TargetPosZ = _instantiatedTargetGhostGameObject.transform.position.z;
                GrabNodeModel.HasTargetPosition = true;
                
                // Rotasyon bilgilerini kaydet (Euler angles)
                Vector3 eulerRotation = _instantiatedTargetGhostGameObject.transform.rotation.eulerAngles;
                GrabNodeModel.TargetRotX = eulerRotation.x;
                GrabNodeModel.TargetRotY = eulerRotation.y;
                GrabNodeModel.TargetRotZ = eulerRotation.z;
                GrabNodeModel.HasTargetRotation = true;
                
                LogManager.LogSuccess($"Target position saved: {_instantiatedTargetGhostGameObject.transform.position}");
                LogManager.LogSuccess($"Target rotation saved: {eulerRotation}");
            }
        }

        /// <summary>
        /// Child object seçildiğinde target ghost'u child'ın kendi ghost'u ile günceller
        /// </summary>
        private void UpdateTargetGhostForChildSelection(GameObject childObject)
        {
            if (_instantiatedTargetGhostGameObject == null)
            {
                return;
            }

            // Mevcut ghost pozisyon ve rotation bilgilerini kaydet
            Vector3 oldPosition = _instantiatedTargetGhostGameObject.transform.position;
            Quaternion oldRotation = _instantiatedTargetGhostGameObject.transform.rotation;
            bool ghostWasActive = _instantiatedTargetGhostGameObject.activeInHierarchy;

            // Instruction UI'ı geçici olarak koru
            GameObject tempInstructorUI = null;
            Vector3 instructorUILocalPos = Vector3.zero;
            Quaternion instructorUILocalRot = Quaternion.identity;
            bool instructorUIWasActive = false;

            if (_instantiatedInstructorUI != null)
            {
                instructorUILocalPos = _instantiatedInstructorUI.transform.localPosition;
                instructorUILocalRot = _instantiatedInstructorUI.transform.localRotation;
                instructorUIWasActive = _instantiatedInstructorUI.activeInHierarchy;

                // Instruction UI'ı geçici olarak Root'a taşı
                _instantiatedInstructorUI.transform.SetParent(GameObject.Find("Root").transform);
                tempInstructorUI = _instantiatedInstructorUI;
                _instantiatedInstructorUI = null;
            }

            // Eski ghost'u destroy et
            Destroy(_instantiatedTargetGhostGameObject);

            // Child objenin InteractionHelper'ını kontrol et
            var childInteractionHelper = childObject.GetComponent<InteractionHelper>();
            GameObject ghostPrefab = null;

            if (childInteractionHelper != null && childInteractionHelper.targetGhostPrefab != null)
            {
                ghostPrefab = childInteractionHelper.targetGhostPrefab;
            }
            else if (selectTargetGhostPrefab != null)
            {
                ghostPrefab = selectTargetGhostPrefab;
                LogManager.LogWarning($"Child object {childObject.name} doesn't have InteractionHelper or targetGhostPrefab, using default ghost");
            }

            if (ghostPrefab != null)
            {
                // Yeni ghost oluştur
                _instantiatedTargetGhostGameObject = Instantiate(ghostPrefab, GameObject.Find("Root").transform);
                _instantiatedTargetGhostGameObject.transform.position = oldPosition;
                _instantiatedTargetGhostGameObject.transform.rotation = oldRotation;
                _instantiatedTargetGhostGameObject.SetActive(ghostWasActive);

                // Instruction UI'ı yeni ghost'un child'ı yap
                if (tempInstructorUI != null)
                {
                    _instantiatedInstructorUI = tempInstructorUI;
                    _instantiatedInstructorUI.transform.SetParent(_instantiatedTargetGhostGameObject.transform);
                    _instantiatedInstructorUI.transform.localPosition = instructorUILocalPos;
                    _instantiatedInstructorUI.transform.localRotation = instructorUILocalRot;

                    // Yeni ghost'un scale'ine göre instruction UI scale'ini düzelt
                    Vector3 newGhostScale = _instantiatedTargetGhostGameObject.transform.localScale;
                    Vector3 newCorrectedScale = new Vector3(
                        1f / newGhostScale.x,
                        1f / newGhostScale.y,
                        1f / newGhostScale.z
                    );
                    _instantiatedInstructorUI.transform.localScale = newCorrectedScale;
                    _instantiatedInstructorUI.SetActive(instructorUIWasActive);
                }
            }
            else
            {
                LogManager.LogError($"No ghost prefab available for child object: {childObject.name}");
            }
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

            if (GrabNodeModel != null && GrabNodeModel.IsChildObjectEnabled &&
                !string.IsNullOrEmpty(GrabNodeModel.SelectedChildName))
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
            if (GrabNodeModel == null || !GrabNodeModel.IsChildObjectEnabled ||
                string.IsNullOrEmpty(GrabNodeModel.SelectedObjectID))
                return;

            GameObject parentObject = FindObjectByID(GrabNodeModel.SelectedObjectID);
            if (parentObject == null) return;

            if (!string.IsNullOrEmpty(GrabNodeModel.SelectedChildName))
            {
                Transform childTransform =
                    FindChildByNameRecursive(parentObject.transform, GrabNodeModel.SelectedChildName);
                if (childTransform != null)
                {
                    // Runtime'da: parent kapalı, sadece seçili child açık
                    SetChildCollidersState(parentObject.transform, childTransform);
                }
            }
        }

        private void OnSelectTarget()
        {
            if (_simpleInteractable == null)
            {
                LogManager.LogWarning("No object selected for grab operation");
                return;
            }

            // Child seçiliyse child objesinin InteractionHelper'ını kullan
            GameObject targetObject = _simpleInteractable;
            bool isChildObject = false;
            
            if (GrabNodeModel != null && GrabNodeModel.IsChildObjectEnabled &&
                !string.IsNullOrEmpty(GrabNodeModel.SelectedChildName))
            {
                Transform childTransform =
                    FindChildByNameRecursive(_simpleInteractable.transform, GrabNodeModel.SelectedChildName);
                if (childTransform != null)
                {
                    targetObject = childTransform.gameObject;
                    isChildObject = true;
                }
                else
                {
                    LogManager.LogWarning(
                        $"Child object not found: {GrabNodeModel.SelectedChildName}, using default ghost");
                }
            }

            var interactionHelper = targetObject.GetComponent<InteractionHelper>();
            
            GameObject ghostPrefab = null;
            
            // Child obje için önce kendi InteractionHelper'ını kontrol et
            if (isChildObject && interactionHelper != null && interactionHelper.targetGhostPrefab != null)
            {
                ghostPrefab = interactionHelper.targetGhostPrefab;
            }
            // Child objenin kendi ghost'u yoksa veya parent obje ise fallback kullan
            else if (selectTargetGhostPrefab != null)
            {
                ghostPrefab = selectTargetGhostPrefab;
                if (isChildObject)
                {
                    LogManager.LogWarning($"Child object {targetObject.name} doesn't have InteractionHelper or targetGhostPrefab, using default ghost");
                }
            }
            else
            {
                LogManager.LogError("No ghost prefab available - neither object's InteractionHelper nor default selectTargetGhostPrefab is set");
                return;
            }

            if (_instantiatedTargetGhostGameObject == null)
            {
                // Yeni target ghost oluştur
                _instantiatedTargetGhostGameObject = Instantiate(
                    ghostPrefab,
                    XRInputManager.xrRayInteractor.transform);

                // GrabInstructorUI'ı da oluştur
                CreateInstructorUI();

                _holdingTarget = true;
            }
            else
            {
                // Mevcut target ghost'u destroy et ve yenisini oluştur (farklı prefab olabilir)
                if (_instantiatedInstructorUI != null)
                {
                    // Instruction UI'ı geçici olarak koru
                    _instantiatedInstructorUI.transform.SetParent(GameObject.Find("Root").transform);
                }
                
                Destroy(_instantiatedTargetGhostGameObject);
                
                // Yeni ghost oluştur
                _instantiatedTargetGhostGameObject = Instantiate(
                    ghostPrefab,
                    XRInputManager.xrRayInteractor.transform);
                
                // Instruction UI'ı yeni ghost'un child'ı yap
                if (_instantiatedInstructorUI != null)
                {
                    _instantiatedInstructorUI.transform.SetParent(_instantiatedTargetGhostGameObject.transform);
                    
                    // Scale düzeltmesi yap
                    Vector3 ghostScale = _instantiatedTargetGhostGameObject.transform.localScale;
                    Vector3 correctedScale = new Vector3(
                        1f / ghostScale.x, 
                        1f / ghostScale.y, 
                        1f / ghostScale.z
                    );
                    _instantiatedInstructorUI.transform.localScale = correctedScale;
                }
                
                // InstructorUI'ı tekrar göster
                ShowInstructorUI();
                
                _holdingTarget = true;
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

            // Target ghost'u load sonrasında gizle (node aktif değil)
            HideTargetGhost();

            // Child status text'ini güncelle (TouchNodePresenter exact copy)
            //UpdateChildStatusText();
        }

        private void RestoreSelectedObject()
        {
            if (string.IsNullOrEmpty(GrabNodeModel.SelectedObjectID)) return;

            GameObject parentObject = FindObjectByID(GrabNodeModel.SelectedObjectID);
            if (parentObject == null)
            {
                LogManager.LogWarning(
                    $"GrabNode: Could not find parent object with ID: {GrabNodeModel.SelectedObjectID}");
                
                // Missing object için fallback sistemi - available objelerden birini seç
                parentObject = FindAlternativeObject();
                if (parentObject != null)
                {
                    LogManager.LogWarning($"GrabNode: Using alternative object: {parentObject.name}");
                    
                    // Model'i yeni obje ile güncelle
                    var objectPresenter = parentObject.GetComponent<ObjectPresenter>();
                    if (objectPresenter != null)
                    {
                        GrabNodeModel.SelectedObjectID = objectPresenter.Model.ID;
                        GrabNodeModel.SelectedObjectName = parentObject.name;
                    }
                }
                else
                {
                    LogManager.LogError("GrabNode: No alternative objects available");
                    return;
                }
            }

            // Nesneyi ayarla (parent veya child)
            SetSelectedObject(parentObject);
        }

        /// <summary>
        /// Missing object ID için alternatif obje bulur (henüz assign edilmemiş olanları tercih eder)
        /// </summary>
        private GameObject FindAlternativeObject()
        {
            // Tüm available objelerı bul
            ObjectPresenter[] allObjectPresenters = FindObjectsByType<ObjectPresenter>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            
            if (allObjectPresenters.Length == 0) return null;

            // VIROO_PrefabContainer altındaki objelerı filtrele
            var virooObjects = allObjectPresenters
                .Where(op => op.transform.parent != null && op.transform.parent.name == "VIROO_PrefabContainer")
                .ToList();

            if (virooObjects.Count == 0) return null;

            // GraphManager'dan diğer GrabNode'ların kullandığı objeleri al
            var graphManager = FindObjectOfType<GraphManager>();
            var usedObjectIds = new HashSet<string>();
            
            if (graphManager != null)
            {
                foreach (var nodePresenter in graphManager.NodePresenters)
                {
                    if (nodePresenter is GrabNodePresenter grabPresenter && grabPresenter != this)
                    {
                        var grabModel = grabPresenter.GrabNodeModel;
                        if (grabModel != null && !string.IsNullOrEmpty(grabModel.SelectedObjectID))
                        {
                            usedObjectIds.Add(grabModel.SelectedObjectID);
                        }
                    }
                }
            }

            // Önce kullanılmamış objeleri tercih et
            var unusedObject = virooObjects.FirstOrDefault(op => !usedObjectIds.Contains(op.Model.ID));
            if (unusedObject != null)
            {
                return unusedObject.gameObject;
            }

            // Hepsi kullanılmışsa ilk uygun olanı al
            LogManager.LogWarning("All objects are used, taking first available");
            return virooObjects.First().gameObject;
        }

        private void SetSelectedObject(GameObject parentObject)
        {
            _simpleInteractable = parentObject;
            selectTargetButton.interactable = true;
            selectChildObjectButton.interactable = true;

            // Child seçimi varsa onu ayarla (TouchNodePresenter exact copy)
            if (GrabNodeModel.IsChildObjectEnabled && !string.IsNullOrEmpty(GrabNodeModel.SelectedChildName))
            {
                Transform childTransform =
                    FindChildByNameRecursive(parentObject.transform, GrabNodeModel.SelectedChildName);
                if (childTransform != null)
                {
                    // Input field'ını güncelle - parent -> child
                    if (selectObjectInputField != null)
                    {
                        selectObjectInputField.text = $"{parentObject.name} -> {childTransform.name}";
                    }

                    // Child status text'ini güncelle (TouchNodePresenter exact copy)
                    UpdateChildStatusText();
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
            }

            // Target pozisyon ve rotasyonunu restore et
            if (GrabNodeModel.HasTargetPosition)
            {
                Vector3 targetPosition = new Vector3(GrabNodeModel.TargetPosX, GrabNodeModel.TargetPosY,
                    GrabNodeModel.TargetPosZ);
                    
                // Rotasyon bilgisini de al
                Vector3 targetRotation = Vector3.zero;
                if (GrabNodeModel.HasTargetRotation)
                {
                    targetRotation = new Vector3(GrabNodeModel.TargetRotX, GrabNodeModel.TargetRotY, GrabNodeModel.TargetRotZ);
                }

                // Child seçiliyse child objesinin InteractionHelper'ını kullan
                GameObject targetObject = _simpleInteractable;
                bool isChildObject = false;
                
                if (GrabNodeModel.IsChildObjectEnabled && !string.IsNullOrEmpty(GrabNodeModel.SelectedChildName))
                {
                    Transform childTransform =
                        FindChildByNameRecursive(parentObject.transform, GrabNodeModel.SelectedChildName);
                    if (childTransform != null)
                    {
                        targetObject = childTransform.gameObject;
                        isChildObject = true;
                    }
                    else
                    {
                        LogManager.LogWarning(
                            $"Child object not found during restore: {GrabNodeModel.SelectedChildName}, using default ghost");
                    }
                }

                var interactionHelper = targetObject.GetComponent<InteractionHelper>();
                
                GameObject ghostPrefab = null;
                
                // Child obje için önce kendi InteractionHelper'ını kontrol et
                if (isChildObject && interactionHelper != null && interactionHelper.targetGhostPrefab != null)
                {
                    ghostPrefab = interactionHelper.targetGhostPrefab;
                }
                // Child objenin kendi ghost'u yoksa veya parent obje ise fallback kullan
                else if (selectTargetGhostPrefab != null)
                {
                    ghostPrefab = selectTargetGhostPrefab;
                    if (isChildObject)
                    {
                        LogManager.LogWarning($"Child object {targetObject.name} doesn't have InteractionHelper or targetGhostPrefab, using default ghost for restore");
                    }
                }
                
                if (ghostPrefab != null)
                {
                    // Eski target ghost varsa temizle
                    if (_instantiatedTargetGhostGameObject != null)
                    {
                        Destroy(_instantiatedTargetGhostGameObject);
                    }

                    // Yeni target ghost oluştur ve pozisyon/rotasyonunu ayarla
                    _instantiatedTargetGhostGameObject = Instantiate(
                        ghostPrefab,
                        GameObject.Find("Root").transform);
                    _instantiatedTargetGhostGameObject.transform.position = targetPosition;
                    
                    // Kaydedilmiş rotasyon varsa uygula (restore işleminde)
                    if (GrabNodeModel != null && GrabNodeModel.HasTargetRotation)
                    {
                        _instantiatedTargetGhostGameObject.transform.rotation = Quaternion.Euler(targetRotation);
                    }

                    // Instruction UI'ı da oluştur (restore sonrası)
                    if (grabInstructorUIPrefab != null)
                    {
                        CreateInstructorUI();
                    }
                }
                else
                {
                    LogManager.LogError($"GrabNode: No ghost prefab available for restore - object: {targetObject.name}");
                }
            }
        }

        /// <summary>
        /// InteractionHelper component'i olmayan objeler için fallback target ghost oluşturur
        /// </summary>
        private void CreateFallbackTargetGhost(GameObject targetObject, Vector3 targetPosition)
        {
            try
            {
                // selectTargetGhostPrefab field'ından default ghost kullan
                if (selectTargetGhostPrefab != null)
                {
                    // Eski target ghost varsa temizle
                    if (_instantiatedTargetGhostGameObject != null)
                    {
                        Destroy(_instantiatedTargetGhostGameObject);
                    }

                    // Default ghost prefab ile oluştur
                    _instantiatedTargetGhostGameObject = Instantiate(
                        selectTargetGhostPrefab,
                        GameObject.Find("Root").transform);
                    _instantiatedTargetGhostGameObject.transform.position = targetPosition;
                    
                    // Kaydedilmiş rotasyon varsa uygula (restore işleminde)
                    if (GrabNodeModel != null && GrabNodeModel.HasTargetRotation)
                    {
                        Vector3 fallbackTargetRotation = new Vector3(GrabNodeModel.TargetRotX, GrabNodeModel.TargetRotY, GrabNodeModel.TargetRotZ);
                        _instantiatedTargetGhostGameObject.transform.rotation = Quaternion.Euler(fallbackTargetRotation);
                    }

                    // Instruction UI'ı da oluştur
                    if (grabInstructorUIPrefab != null)
                    {
                        CreateInstructorUI();
                    }

                    LogManager.LogWarning(
                        $"GrabNode: Using fallback ghost for {targetObject.name} - InteractionHelper not found");
                }
                else
                {
                    LogManager.LogError(
                        $"GrabNode: No fallback ghost prefab available for {targetObject.name}");
                }
            }
            catch (Exception e)
            {
                LogManager.LogError($"GrabNode: Fallback ghost creation failed: {e.Message}");
            }
        }

        /// <summary>
        /// Seçili objenin outline'ını etkinleştirir
        /// </summary>
        private void EnableObjectOutline()
        {
            GameObject targetObject = GetTargetObject();
            if (targetObject != null)
            {
                _3rd_Party.Outline.Outline outline = targetObject.GetComponent<_3rd_Party.Outline.Outline>();
                if (outline != null)
                {
                    outline.enabled = true;
                }
                else
                {
                    LogManager.LogWarning($"GrabNode: No outline component found on {targetObject.name}");
                }
            }
        }

        /// <summary>
        /// Seçili objenin outline'ını devre dışı bırakır
        /// </summary>
        private void DisableObjectOutline()
        {
            GameObject targetObject = GetTargetObject();
            if (targetObject != null)
            {
                _3rd_Party.Outline.Outline outline = targetObject.GetComponent<_3rd_Party.Outline.Outline>();
                if (outline != null)
                {
                    outline.enabled = false;
                }
            }
        }

        /// <summary>
        /// Outline için hedef objeyi döndürür (parent veya child)
        /// </summary>
        private GameObject GetTargetObject()
        {
            if (GrabNodeModel == null || string.IsNullOrEmpty(GrabNodeModel.SelectedObjectID))
                return null;

            GameObject parentObject = FindObjectByID(GrabNodeModel.SelectedObjectID);
            if (parentObject == null)
                return null;

            // Child seçili ise child'ı döndür
            if (GrabNodeModel.IsChildObjectEnabled && !string.IsNullOrEmpty(GrabNodeModel.SelectedChildName))
            {
                Transform childTransform = FindChildByNameRecursive(parentObject.transform, GrabNodeModel.SelectedChildName);
                if (childTransform != null)
                {
                    return childTransform.gameObject;
                }
            }

            // Parent'ı döndür
            return parentObject;
        }

        /// <summary>
        /// VIROO_PrefabContainer altındaki nesneleri ObjectModel.ID ile bulur
        /// </summary>
        private GameObject FindObjectByID(string objectID)
        {
            if (string.IsNullOrEmpty(objectID)) return null;

            // Sahne içindeki tüm ObjectPresenter'ları bul
            ObjectPresenter[] allObjectPresenters =
                FindObjectsByType<ObjectPresenter>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (var objectPresenter in allObjectPresenters)
            {
                if (objectPresenter.Model != null)
                {
                    if (objectPresenter.Model.ID == objectID)
                    {
                        return objectPresenter.gameObject;
                    }
                }
            }

            return null;
        }

        private void ShowTargetGhost()
        {
            if (_instantiatedTargetGhostGameObject != null)
            {
                _instantiatedTargetGhostGameObject.SetActive(true);
                
                // Target ghost gösterildiğinde instructor UI'ı da göster
                ShowInstructorUI();
            }
        }

        public void HideTargetGhost()
        {
            if (_instantiatedTargetGhostGameObject != null)
            {
                _instantiatedTargetGhostGameObject.SetActive(false);
            }
            
            // Target ghost gizlendiğinde instructor UI'ı da gizle
            HideInstructorUI();
        }

        /// <summary>
        /// GrabInstructorUI prefab'ını oluşturur
        /// </summary>
        private void CreateInstructorUI()
        {
            if (grabInstructorUIPrefab == null)
            {
                LogManager.LogWarning("GrabInstructorUI prefab is not assigned!");
                return;
            }

            // Eski instructor UI varsa temizle
            if (_instantiatedInstructorUI != null)
            {
                Destroy(_instantiatedInstructorUI);
            }

            // Yeni instructor UI oluştur - ghost varsa onun child'ı yap
            Transform parentTransform = _instantiatedTargetGhostGameObject != null 
                ? _instantiatedTargetGhostGameObject.transform 
                : GameObject.Find("Root").transform;
                
            _instantiatedInstructorUI = Instantiate(grabInstructorUIPrefab, parentTransform);
            
            // Ghost'un scale'i varsa instruction UI'ın scale'ini ayarla
            if (_instantiatedTargetGhostGameObject != null)
            {
                Vector3 ghostScale = _instantiatedTargetGhostGameObject.transform.localScale;
                Vector3 correctedScale = new Vector3(
                    1f / ghostScale.x, 
                    1f / ghostScale.y, 
                    1f / ghostScale.z
                );
                _instantiatedInstructorUI.transform.localScale = correctedScale;
            }
        }

        /// <summary>
        /// InstructorUI'ı gösterir
        /// </summary>
        private void ShowInstructorUI()
        {
            if (_instantiatedInstructorUI != null)
            {
                // Eğer ghost varsa ve instruction UI ghost'un child'ı değilse, child yap
                if (_instantiatedTargetGhostGameObject != null && 
                    _instantiatedInstructorUI.transform.parent != _instantiatedTargetGhostGameObject.transform)
                {
                    _instantiatedInstructorUI.transform.SetParent(_instantiatedTargetGhostGameObject.transform);
                    
                    // Scale düzeltmesi yap
                    Vector3 ghostScale = _instantiatedTargetGhostGameObject.transform.localScale;
                    Vector3 correctedScale = new Vector3(
                        1f / ghostScale.x, 
                        1f / ghostScale.y, 
                        1f / ghostScale.z
                    );
                    _instantiatedInstructorUI.transform.localScale = correctedScale;
                }
                
                _instantiatedInstructorUI.SetActive(true);
            }
            else if (grabInstructorUIPrefab != null)
            {
                // Instructor UI yoksa oluştur
                CreateInstructorUI();
            }
        }

        /// <summary>
        /// InstructorUI'ı gizler
        /// </summary>
        private void HideInstructorUI()
        {
            if (_instantiatedInstructorUI != null)
            {
                _instantiatedInstructorUI.SetActive(false);
            }
        }

        /// <summary>
        /// Instruction UI'ı sürekli kullanıcıya baktırır (billboard effect)
        /// </summary>
        private void UpdateInstructorUIOrientation()
        {
            if (_instantiatedInstructorUI == null || !_instantiatedInstructorUI.activeInHierarchy)
                return;

            // Kamera referansını al
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                // XR ortamında main camera yoksa, XR kamerasını bul
                mainCamera = FindFirstObjectByType<Camera>();
            }

            if (mainCamera != null)
            {
                // Instruction UI'ı kameraya baktır (LookAt daha basit ve net)
                Vector3 cameraPosition = mainCamera.transform.position;
                Vector3 instructionPosition = _instantiatedInstructorUI.transform.position;
                
                // Y ekseninde sınırla (daha doğal görünüm için)
                Vector3 lookTarget = new Vector3(cameraPosition.x, instructionPosition.y, cameraPosition.z);
                
                _instantiatedInstructorUI.transform.LookAt(lookTarget);
                
                // 180 derece döndür çünkü instruction UI'ın ön yüzü Z- ekseni
                _instantiatedInstructorUI.transform.Rotate(0, 180, 0);
            }
        }
    }
}