using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Models.Nodes;
using Managers;
using Enums;

namespace Presenters.NodePresenters
{
    public class MaterialChangeNodePresenter : ActionNodePresenter
    {
        [SerializeField] private Material defaultMaterial;
        
        // UI Elements (TouchNode pattern)
        [SerializeField] private Button selectObjectButton;
        [SerializeField] private TMP_InputField selectedObjectInputField;
        [SerializeField] private Button selectChildObjectButton;
        
        // UI Elements (AudioActionNode pattern)
        [SerializeField] private TMP_Dropdown materialDropdown;
        
        // ChangeMaterialActionNode model'ine kolay erişim için cast property
        public ChangeMaterialActionNode ChangeMaterialNodeModel => Model as ChangeMaterialActionNode;
        
        protected override void Awake()
        {
            base.Awake();
            
            // SetActionType çağrısı (node tipini belirlemek için)
            SetActionType(NodeType.ChangeMaterialAction);
            
            // TouchNode pattern - Object selection button listeners
            if (selectObjectButton != null)
            {
                selectObjectButton.onClick.AddListener(OnSelectObject);
            }
            if (selectChildObjectButton != null)
            {
                selectChildObjectButton.onClick.AddListener(OnSelectChildObject);
            }
            
            // AudioActionNode pattern - Material dropdown
            if (materialDropdown != null)
            {
                materialDropdown.ClearOptions();
                
                // Proje içindeki tüm material'ları bul
                List<string> materialOptions = FindAllMaterialsInProject();
                
                materialDropdown.AddOptions(materialOptions);
                materialDropdown.onValueChanged.AddListener(OnMaterialSelected);
                
                // İlk değeri model'e kaydet
                if (ChangeMaterialNodeModel != null && materialOptions.Count > 0)
                {
                    ChangeMaterialNodeModel.SelectedMaterialName = materialOptions[0];
                    ChangeMaterialNodeModel.SelectedMaterialIndex = 0;
                    LogManager.Log($"ChangeMaterialNode: Initial selection set - {materialOptions[0]}");
                }
            }
            else
            {
                LogManager.LogWarning("MaterialChangeNodePresenter: materialDropdown is null! UI will not work properly.");
            }
            
            // Log: MaterialChangeNodePresenter oluşturuldu
            LogManager.LogSuccess("MaterialChangeNodePresenter started: " + gameObject.name);
        }
        
        protected override void OnDisable()
        {
            base.OnDisable();
            
            // TouchNode pattern - Clean up listeners
            if (selectObjectButton != null)
            {
                selectObjectButton.onClick.RemoveAllListeners();
            }
            if (selectChildObjectButton != null)
            {
                selectChildObjectButton.onClick.RemoveAllListeners();
            }
            
            // AudioActionNode pattern - Clean up dropdown listener
            if (materialDropdown != null)
            {
                materialDropdown.onValueChanged.RemoveAllListeners();
            }
        }
        
        // TouchNode pattern - Object selection methods
        public void OnSelectObject()
        {
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
                if (selectedObjectInputField != null)
                {
                    selectedObjectInputField.text = SystemManager.Selected3DObject.name;
                }

                // Model'i hemen güncelle (MVP prensibi) - Parent nesneyi seç
                if (ChangeMaterialNodeModel != null)
                {
                    ChangeMaterialNodeModel.SelectedObjectName = SystemManager.Selected3DObject.name;
                    ChangeMaterialNodeModel.SelectedObjectID = objectPresenter.Model.ID; // Parent'ın ID'si
                    ChangeMaterialNodeModel.SelectedChildIndex = -1; // Parent seçili
                    ChangeMaterialNodeModel.IsChildObjectEnabled = false; // Child devre dışı
                }

                // Log: Parent nesne seçildi
                LogManager.LogInteraction($"Parent object selected: {SystemManager.Selected3DObject.name} (ID: {objectPresenter.Model.ID})");
            }
            catch (Exception e)
            {
                // Log: Hata oluştu
                LogManager.LogError("Error selecting object: " + e.Message);
                Debug.LogError(e.Message);
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

            // İlk child'ı varsayılan olarak seç (Unity'de UI eklenerek kullanıcı seçimi yapılabilir)
            int selectedChildIndex = 0; // Şimdilik ilk child
            Transform selectedChild = SystemManager.Selected3DObject.transform.GetChild(selectedChildIndex);

            // Model'i güncelle - Parent ID'sini doğru şekilde kaydet
            if (ChangeMaterialNodeModel != null)
            {
                ChangeMaterialNodeModel.SelectedObjectName = selectedChild.name;
                ChangeMaterialNodeModel.SelectedObjectID = parentObjectPresenter.Model.ID; // Parent'ın ID'sini kaydet
                ChangeMaterialNodeModel.SelectedChildIndex = selectedChildIndex; // Child index'ini kaydet
                ChangeMaterialNodeModel.IsChildObjectEnabled = true;
            }

            // Input field'ı güncelle
            if (selectedObjectInputField != null)
            {
                selectedObjectInputField.text = $"{SystemManager.Selected3DObject.name} -> {selectedChild.name}";
            }

            LogManager.LogInteraction($"Child object selected: {selectedChild.name} (Index: {selectedChildIndex}, Parent ID: {ChangeMaterialNodeModel.SelectedObjectID})");
        }
        
        // AudioActionNode pattern - Material dropdown methods
        private void OnMaterialSelected(int index)
        {
            string materialName = materialDropdown.options[index].text;

            // Model'e dropdown bilgilerini kaydet - ChangeMaterialActionNode özelliklerini kullan
            if (ChangeMaterialNodeModel != null)
            {
                ChangeMaterialNodeModel.SelectedMaterialName = materialName;
                ChangeMaterialNodeModel.SelectedMaterialIndex = index;
                
                // DropdownItems listesini de güncel tutmak için
                if (ChangeMaterialNodeModel.DropdownItems == null)
                {
                    ChangeMaterialNodeModel.DropdownItems = new List<string>();
                }
                
                // Dropdown items'ları güncelle (sadece boşsa)
                if (ChangeMaterialNodeModel.DropdownItems.Count == 0)
                {
                    foreach (var option in materialDropdown.options)
                    {
                        ChangeMaterialNodeModel.DropdownItems.Add(option.text);
                    }
                }
                
                // Eski property'leri de güncelle (geriye dönük uyumluluk)
                ChangeMaterialNodeModel.MaterialName = materialName;
                ChangeMaterialNodeModel.MaterialPath = "Materials/" + materialName;
            }

            // Eski parameter sistemini de koruyalım (geriye dönük uyumluluk için)
            SetParameterName("material");
            SetParameterValue(materialName);

            LogManager.LogSuccess($"Material selected: {materialName} (Index: {index})");
        }
        
        private List<string> FindAllMaterialsInProject()
        {
            List<string> materialNames = new List<string>();

            Material[] materials = Resources.LoadAll<Material>("Materials");
            foreach (Material mat in materials)
            {
                if (mat != null)
                {
                    materialNames.Add(mat.name);
                    Debug.Log($"Resources/Materials'da bulunan materyal: {mat.name}");
                }
            }

            if (materialNames.Count == 0)
            {
                Debug.LogWarning("Resources/Materials klasöründe hiç materyal bulunamadı. Varsayılan değerler kullanılıyor.");
                materialNames.Add("Default-Material");
            }

            // Dropdown items'ları model'e kaydet
            if (ChangeMaterialNodeModel != null)
            {
                ChangeMaterialNodeModel.DropdownItems = new List<string>(materialNames);
                LogManager.LogSuccess($"ChangeMaterialNode: Dropdown items saved to model ({materialNames.Count} items)");
            }

            return materialNames;
        }
        
        protected override void PerformAction()
        {
            base.PerformAction();
            
            // Hedef objeyi bul - yeni object selection sistemini kullan
            GameObject targetObj = null;
            if (!string.IsNullOrEmpty(ChangeMaterialNodeModel.SelectedObjectID))
            {
                targetObj = FindObjectByID(ChangeMaterialNodeModel.SelectedObjectID);
                if (targetObj == null)
                {
                    Debug.LogWarning($"Hedef nesne bulunamadı: {ChangeMaterialNodeModel.SelectedObjectID}");
                    return;
                }
                
                // Child object seçiliyse, child'ı hedef al
                if (ChangeMaterialNodeModel.IsChildObjectEnabled && ChangeMaterialNodeModel.SelectedChildIndex >= 0)
                {
                    if (ChangeMaterialNodeModel.SelectedChildIndex < targetObj.transform.childCount)
                    {
                        targetObj = targetObj.transform.GetChild(ChangeMaterialNodeModel.SelectedChildIndex).gameObject;
                    }
                    else
                    {
                        Debug.LogWarning($"Invalid child index: {ChangeMaterialNodeModel.SelectedChildIndex}");
                        return;
                    }
                }
            }
            else
            {
                Debug.LogError("Materyal değiştirme işlemi için hedef nesne ID'si gerekli!");
                return;
            }
            
            // Renderer bileşenini kontrol et
            Renderer renderer = targetObj.GetComponent<Renderer>();
            if (renderer == null)
            {
                Debug.LogError($"Hedef nesnede ({targetObj.name}) Renderer bileşeni bulunamadı!");
                return;
            }
            
            // Seçili materyali yükle
            Material targetMaterial = null;
            if (!string.IsNullOrEmpty(ChangeMaterialNodeModel.SelectedMaterialName))
            {
                targetMaterial = Resources.Load<Material>("Materials/" + ChangeMaterialNodeModel.SelectedMaterialName);
                if (targetMaterial == null)
                {
                    Debug.LogWarning($"Materyal bulunamadı: {ChangeMaterialNodeModel.SelectedMaterialName}");
                    targetMaterial = defaultMaterial; // Varsayılan kullan
                }
            }
            else
            {
                targetMaterial = defaultMaterial;
            }
            
            // Materyali değiştir
            if (targetMaterial != null)
            {
                renderer.material = targetMaterial;
                Debug.Log($"Materyal değiştirildi: {targetObj.name}, Materyal={targetMaterial.name}");
                
                // Log: Materyal değiştirme başarılı
                LogManager.LogSuccess($"Material changed: {targetObj.name} -> {targetMaterial.name}");
            }
            else
            {
                Debug.LogError("Materyal değiştirilemedi: Materyal bulunamadı!");
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
        
        /// <summary>
        /// Model'deki değerleri UI'ya aktarır (yükleme sonrası)
        /// </summary>
        public override void SyncModelToUI()
        {
            // Debug: SyncModelToUI çağrıldı
            LogManager.LogSuccess("MaterialChangeNodePresenter: SyncModelToUI called!");
            
            // Önce base sınıfın ortak özelliklerini sync et
            base.SyncModelToUI();
            
            if (ChangeMaterialNodeModel == null) 
            {
                LogManager.LogError("MaterialChangeNodePresenter: ChangeMaterialNodeModel is null!");
                return;
            }

            // TouchNode pattern - Object selection restore
            // Seçili nesne adını input field'a aktar
            if (selectedObjectInputField != null && !string.IsNullOrEmpty(ChangeMaterialNodeModel.SelectedObjectName))
            {
                selectedObjectInputField.text = ChangeMaterialNodeModel.SelectedObjectName;
            }

            // Eğer seçili nesne ID'si varsa, VIROO_PrefabContainer'da parent'ı bul
            if (!string.IsNullOrEmpty(ChangeMaterialNodeModel.SelectedObjectID))
            {
                GameObject parentObject = FindObjectByID(ChangeMaterialNodeModel.SelectedObjectID);
                if (parentObject != null)
                {
                    // Parent mı child mı seçili?
                    if (ChangeMaterialNodeModel.SelectedChildIndex == -1)
                    {
                        // Parent seçili
                        LogManager.LogSuccess($"MaterialChangeNodePresenter: Parent object restored: {ChangeMaterialNodeModel.SelectedObjectName} (ID: {ChangeMaterialNodeModel.SelectedObjectID})");
                    }
                    else
                    {
                        // Child seçili
                        if (ChangeMaterialNodeModel.SelectedChildIndex >= 0 && ChangeMaterialNodeModel.SelectedChildIndex < parentObject.transform.childCount)
                        {
                            Transform childTransform = parentObject.transform.GetChild(ChangeMaterialNodeModel.SelectedChildIndex);
                            
                            // Input field'ı güncelle
                            if (selectedObjectInputField != null)
                            {
                                selectedObjectInputField.text = $"{parentObject.name} -> {childTransform.name}";
                            }
                            
                            LogManager.LogSuccess($"MaterialChangeNodePresenter: Child object restored: {childTransform.name} (Index: {ChangeMaterialNodeModel.SelectedChildIndex}, Parent ID: {ChangeMaterialNodeModel.SelectedObjectID})");
                        }
                        else
                        {
                            LogManager.LogError($"MaterialChangeNodePresenter: Invalid child index {ChangeMaterialNodeModel.SelectedChildIndex} for parent {parentObject.name}");
                        }
                    }
                }
                else
                {
                    LogManager.LogWarning($"MaterialChangeNodePresenter: Could not find parent object with ID: {ChangeMaterialNodeModel.SelectedObjectID}");
                }
            }

            // AudioActionNode pattern - Dropdown restore
            // DropdownItems'ları restore et
            if (ChangeMaterialNodeModel.DropdownItems != null && ChangeMaterialNodeModel.DropdownItems.Count > 0)
            {
                if (materialDropdown != null)
                {
                    materialDropdown.ClearOptions();
                    materialDropdown.AddOptions(ChangeMaterialNodeModel.DropdownItems);
                    LogManager.LogSuccess($"MaterialChangeNodePresenter: Dropdown items restored ({ChangeMaterialNodeModel.DropdownItems.Count} items)");
                }
            }

            // Seçili material index'ini restore et
            if (materialDropdown != null && ChangeMaterialNodeModel.SelectedMaterialIndex >= 0 && 
                ChangeMaterialNodeModel.SelectedMaterialIndex < materialDropdown.options.Count)
            {
                // Dropdown listener'ını geçici olarak kaldır (OnMaterialSelected tetiklenmesini engelle)
                materialDropdown.onValueChanged.RemoveListener(OnMaterialSelected);
                
                // Dropdown value'yu set et
                materialDropdown.value = ChangeMaterialNodeModel.SelectedMaterialIndex;
                
                // Listener'ı geri ekle
                materialDropdown.onValueChanged.AddListener(OnMaterialSelected);
                
                LogManager.LogSuccess($"MaterialChangeNodePresenter: Material dropdown restored: {ChangeMaterialNodeModel.SelectedMaterialName}");
            }

            LogManager.LogSuccess($"MaterialChangeNodePresenter UI synced - Selected Object: {ChangeMaterialNodeModel.SelectedObjectName}, Material: {ChangeMaterialNodeModel.SelectedMaterialName}, ChildIndex: {ChangeMaterialNodeModel.SelectedChildIndex}");
        }
    }
} 