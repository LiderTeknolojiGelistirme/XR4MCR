using System.Collections;
using UnityEngine;
using Models.Nodes;
using Managers;

namespace Presenters.NodePresenters
{
    public class PlaySoundActionPresenter : ActionNodePresenter
    {
        [SerializeField] private AudioClip defaultSoundClip;
        [SerializeField] private float defaultVolume = 1.0f;
        
        private AudioClip _soundClip;
        private float _volume;
        
        protected override void Awake()
        {
            base.Awake();
            
            // Varsayılan ses klibini ayarla
            _soundClip = defaultSoundClip;
            _volume = defaultVolume;
            
            // Log: PlaySoundActionPresenter oluşturuldu
            LogManager.LogSuccess("PlaySoundActionPresenter başlatıldı: " + gameObject.name);
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
                }
            }
            
            // Parametre değerlerini kontrol et
            if (!string.IsNullOrEmpty(ActionModel.ParameterName))
            {
                if (ActionModel.ParameterName.ToLower() == "volume")
                {
                    if (float.TryParse(ActionModel.ParameterValue, out float volume))
                    {
                        _volume = volume;
                    }
                }
                else if (ActionModel.ParameterName.ToLower() == "clip")
                {
                    // Ses klibini yükle (Resources klasöründen)
                    _soundClip = Resources.Load<AudioClip>(ActionModel.ParameterValue);
                    if (_soundClip == null)
                    {
                        Debug.LogWarning($"Ses klibi bulunamadı: {ActionModel.ParameterValue}");
                    }
                }
            }
            
            // Ses çal
            if (_soundClip != null)
            {
                if (targetObj != null)
                {
                    // Hedef nesnede AudioSource bileşeni var mı kontrol et
                    AudioSource audioSource = targetObj.GetComponent<AudioSource>();
                    if (audioSource == null)
                    {
                        // AudioSource yoksa ekle
                        audioSource = targetObj.AddComponent<AudioSource>();
                    }
                    
                    // Ses çal
                    audioSource.PlayOneShot(_soundClip, _volume);
                    
                    Debug.Log($"Ses çalınıyor: {_soundClip.name}, Hedef: {targetObj.name}, Ses: {_volume}");
                }
                else
                {
                    // Genel ses çalma (mekânsal olmayan)
                    AudioSource.PlayClipAtPoint(_soundClip, Camera.main.transform.position, _volume);
                    
                    Debug.Log($"Genel ses çalınıyor: {_soundClip.name}, Ses: {_volume}");
                }
                
                // Log: Ses çalma başarılı
                LogManager.LogSuccess($"Ses çalındı: {_soundClip.name}");
            }
            else
            {
                Debug.LogError("Ses çalınamadı: Ses klibi bulunamadı!");
            }
        }
    }
} 