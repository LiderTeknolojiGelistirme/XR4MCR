using System.Collections;
using UnityEngine;
using Models.Nodes;
using Managers;

namespace Presenters.NodePresenters
{
    public class ChangeMaterialActionPresenter : ActionNodePresenter
    {
        [SerializeField] private Material defaultMaterial;
        
        private Material _targetMaterial;
        
        protected override void Awake()
        {
            base.Awake();
            
            // Varsayılan değerleri ayarla
            _targetMaterial = defaultMaterial;
            
            // Log: ChangeMaterialActionPresenter oluşturuldu
            LogManager.LogSuccess("ChangeMaterialActionPresenter başlatıldı: " + gameObject.name);
        }
        
        protected override void PerformAction()
        {
            base.PerformAction();
            
            // Hedef objeyi bul
            GameObject targetObj = null;
            if (!string.IsNullOrEmpty(ActionModel.TargetObjectID))
            {
                targetObj = GameObject.Find(ActionModel.TargetObjectID);
                if (targetObj == null)
                {
                    Debug.LogWarning($"Hedef nesne bulunamadı: {ActionModel.TargetObjectID}");
                    return;
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
            
            // Parametre değerlerini kontrol et
            if (!string.IsNullOrEmpty(ActionModel.ParameterName))
            {
                if (ActionModel.ParameterName.ToLower() == "material")
                {
                    // Materyal adıyla Resources klasöründen yükleme
                    Material loadedMaterial = Resources.Load<Material>(ActionModel.ParameterValue);
                    if (loadedMaterial != null)
                    {
                        _targetMaterial = loadedMaterial;
                    }
                    else
                    {
                        Debug.LogWarning($"Materyal bulunamadı: {ActionModel.ParameterValue}");
                    }
                }
            }
            
            // Materyali değiştir
            if (_targetMaterial != null)
            {
                renderer.material = _targetMaterial;
                Debug.Log($"Materyal değiştirildi: {targetObj.name}, Materyal={_targetMaterial.name}");
                
                // Log: Materyal değiştirme başarılı
                LogManager.LogSuccess($"Materyal değiştirildi: {targetObj.name} -> {_targetMaterial.name}");
            }
            else
            {
                Debug.LogError("Materyal değiştirilemedi: Materyal bulunamadı!");
            }
        }
    }
} 