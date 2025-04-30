using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Models.Nodes;
using Managers;
using System.Collections.Generic;

namespace Presenters.NodePresenters
{
    public class MaterialChangeNodePresenter : ActionNodePresenter
    {
        [SerializeField] private Material defaultMaterial;
        [SerializeField] private Button selectObjectButton;
        [SerializeField] private TMP_Dropdown materialDropdown;
        [SerializeField] private TMP_InputField selectedObjectInputField;
        
        private Material _materialToApply;
        
        protected override void Awake()
        {
            base.Awake();
            
            // Varsayılan materyal
            _materialToApply = defaultMaterial;
            
            // Action tipini ayarla
            SetActionType(ActionNode.ActionType.ChangeMaterial);
            
            // SelectObject butonu için listener
            if (selectObjectButton != null)
            {
                selectObjectButton.onClick.AddListener(OnSelectObject);
            }
            
            // Materyal dropdown
            if (materialDropdown != null)
            {
                materialDropdown.ClearOptions();
                
                // Proje içindeki tüm materyalleri bul
                List<string> materialOptions = FindAllMaterialsInProject();
                                               
                materialDropdown.AddOptions(materialOptions);
                materialDropdown.onValueChanged.AddListener(OnMaterialSelected);
            }
            
            // Log: MaterialChangeNodePresenter oluşturuldu
            LogManager.LogSuccess("MaterialChangeNodePresenter başlatıldı: " + gameObject.name);
        }
        
        protected override void OnDisable()
        {
            base.OnDisable();
            
            // Buton listener'ını temizle
            if (selectObjectButton != null)
            {
                selectObjectButton.onClick.RemoveAllListeners();
            }
            
            // Dropdown listener'ını temizle
            if (materialDropdown != null)
            {
                materialDropdown.onValueChanged.RemoveAllListeners();
            }
        }
        
        // Nesne seçimi
        public void OnSelectObject()
        {
            try
            {
                GameObject selectedObject = SystemManager.Selected3DObject;
                
                if (selectedObject != null)
                {
                    // Seçili nesneyi göster
                    selectedObjectInputField.text = "Seçili Nesne: " + selectedObject.name;
                    
                    // Model'e kaydet
                    SetTargetObject(selectedObject.name);
                    
                    LogManager.LogSuccess("Nesne seçildi: " + selectedObject.name);
                }
                else
                {
                    LogManager.LogWarning("Seçili nesne bulunamadı!");
                }
            }
            catch (Exception e)
            {
                LogManager.LogError("Hata: " + e.Message);
            }
        }
        
        // Materyal seçimi
        private void OnMaterialSelected(int index)
        {
            string materialName = materialDropdown.options[index].text;
            
            // Model'e kaydet
            SetParameterName("material");
            SetParameterValue(materialName);
            
            // Materyali yükle
            Debug.Log($"Materyal yüklemeye çalışılıyor: '{materialName}'");
            
            // Materyali doğrudan adıyla yüklemeyi dene
            Material selectedMaterial = Resources.Load<Material>(materialName);
            
            if (selectedMaterial != null)
            {
                _materialToApply = selectedMaterial;
                LogManager.LogSuccess($"Materyal seçildi: {materialName}");
            }
            else
            {
                // Proje içindeki tüm klasörleri tara
                _materialToApply = FindMaterialInProject(materialName);
                
                if (_materialToApply != null)
                {
                    LogManager.LogSuccess($"Materyal bulundu ve seçildi: {materialName}");
                }
                else
                {
                    _materialToApply = defaultMaterial;
                    LogManager.LogWarning($"Materyal bulunamadı: {materialName}, varsayılan kullanılacak");
                }
            }
        }
        
        // Materyal değiştirme
        protected override void PerformAction()
        {
            base.PerformAction();
            
            // Hedef nesneyi bul
            if (string.IsNullOrEmpty(ActionModel.TargetObjectID))
            {
                LogManager.LogError("Lütfen bir nesne seçin!");
                return;
            }
            
            GameObject targetObj = GameObject.Find(ActionModel.TargetObjectID);
            if (targetObj == null)
            {
                LogManager.LogError("Nesne bulunamadı: " + ActionModel.TargetObjectID);
                return;
            }
            
            // Renderer kontrolü
            Renderer renderer = targetObj.GetComponent<Renderer>();
            if (renderer == null)
            {
                LogManager.LogError("Renderer bileşeni bulunamadı!");
                return;
            }
            
            // Materyali uygula
            if (_materialToApply != null)
            {
                renderer.material = _materialToApply;
                LogManager.LogSuccess($"Materyal değiştirildi: {targetObj.name} -> {_materialToApply.name}");
            }
            else
            {
                LogManager.LogError("Materyal bulunamadı!");
            }
        }

        // Proje içindeki tüm materyalleri bul
        private List<string> FindAllMaterialsInProject()
        {
            List<string> materialNames = new List<string>();
            
            // Resources/Materials klasöründen tüm materyalleri yükle
            Material[] materials = Resources.LoadAll<Material>("Materials");
            foreach (Material mat in materials)
            {
                if (mat != null)
                {
                    materialNames.Add(mat.name);
                    Debug.Log($"Resources/Materials'da bulunan materyal: {mat.name}");
                }
            }
            
            // Varsayılan değerleri ekle
            if (materialNames.Count == 0)
            {
                Debug.LogWarning("Resources/Materials klasöründe hiç materyal bulunamadı. Varsayılan değerler kullanılıyor.");
                materialNames.Add("Varsayılan");
            }
            
            return materialNames;
        }

        // Materyali proje içinde isme göre ara
        private Material FindMaterialInProject(string materialName)
        {
            // Resources/Materials klasöründen materyali yüklemeyi dene
            Material mat = Resources.Load<Material>("Materials/" + materialName);
            
            if (mat != null)
            {
                Debug.Log($"Materyal doğrudan bulundu: Materials/{materialName}");
                return mat;
            }
            
            // Doğrudan bulunamadıysa, tüm Resources/Materials materyallerini kontrol et
            Material[] allMaterials = Resources.LoadAll<Material>("Materials");
            foreach (Material material in allMaterials)
            {
                if (material != null && material.name == materialName)
                {
                    Debug.Log($"Materyal resources taramasından bulundu: {materialName}");
                    return material;
                }
            }
            
            Debug.LogWarning($"Materyal bulunamadı: {materialName}");
            return null;
        }
    }
} 