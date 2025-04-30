using UnityEngine;
using Models.Nodes;
using Managers;

namespace Presenters.NodePresenters
{
    public class ToggleObjectActionPresenter : ActionNodePresenter
    {
        [SerializeField] private bool defaultState = true;
        
        private bool _targetState;
        
        protected override void Awake()
        {
            base.Awake();
            
            // Varsayılan durumu ayarla
            _targetState = defaultState;
            
            // Log: ToggleObjectActionPresenter oluşturuldu
            LogManager.LogSuccess("ToggleObjectActionPresenter başlatıldı: " + gameObject.name);
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
                Debug.LogError("Nesne açma/kapama işlemi için hedef nesne ID'si gerekli!");
                return;
            }
            
            // Parametre değerini kontrol et
            if (!string.IsNullOrEmpty(ActionModel.ParameterValue))
            {
                // "true" veya "false" olarak durum değerini al
                if (bool.TryParse(ActionModel.ParameterValue, out bool state))
                {
                    _targetState = state;
                }
                // "toggle" değeri varsa mevcut durumu tersine çevir
                else if (ActionModel.ParameterValue.ToLower() == "toggle")
                {
                    _targetState = !targetObj.activeSelf;
                }
            }
            
            // Nesneyi aç/kapat
            targetObj.SetActive(_targetState);
            
            Debug.Log($"Nesne durumu değiştirildi: {targetObj.name}, Durum={_targetState}");
            
            // Log: Durum değiştirme başarılı
            LogManager.LogSuccess($"Nesne {(_targetState ? "açıldı" : "kapatıldı")}: {targetObj.name}");
        }
    }
} 