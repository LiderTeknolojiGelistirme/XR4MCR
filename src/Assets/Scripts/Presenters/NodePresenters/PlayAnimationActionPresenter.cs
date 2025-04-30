using UnityEngine;
using Models.Nodes;
using Managers;

namespace Presenters.NodePresenters
{
    public class PlayAnimationActionPresenter : ActionNodePresenter
    {
        [SerializeField] private string defaultAnimationName = "Idle";
        [SerializeField] private bool defaultCrossFade = false;
        [SerializeField] private float defaultCrossFadeDuration = 0.3f;
        
        private string _animationName;
        private bool _useCrossFade;
        private float _crossFadeDuration;
        
        protected override void Awake()
        {
            base.Awake();
            
            // Varsayılan değerleri ayarla
            _animationName = defaultAnimationName;
            _useCrossFade = defaultCrossFade;
            _crossFadeDuration = defaultCrossFadeDuration;
            
            // Log: PlayAnimationActionPresenter oluşturuldu
            LogManager.LogSuccess("PlayAnimationActionPresenter başlatıldı: " + gameObject.name);
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
                Debug.LogError("Animasyon oynatma işlemi için hedef nesne ID'si gerekli!");
                return;
            }
            
            // Animator bileşenini kontrol et
            Animator animator = targetObj.GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogError($"Hedef nesnede ({targetObj.name}) Animator bileşeni bulunamadı!");
                return;
            }
            
            // Parametre değerlerini kontrol et
            if (!string.IsNullOrEmpty(ActionModel.ParameterName))
            {
                if (ActionModel.ParameterName.ToLower() == "animation")
                {
                    _animationName = ActionModel.ParameterValue;
                }
                else if (ActionModel.ParameterName.ToLower() == "crossfade")
                {
                    if (bool.TryParse(ActionModel.ParameterValue, out bool crossFade))
                    {
                        _useCrossFade = crossFade;
                    }
                }
                else if (ActionModel.ParameterName.ToLower() == "duration")
                {
                    if (float.TryParse(ActionModel.ParameterValue, out float duration))
                    {
                        _crossFadeDuration = duration;
                    }
                }
            }
            
            // Animasyonu oynat
            if (_useCrossFade)
            {
                animator.CrossFade(_animationName, _crossFadeDuration);
                Debug.Log($"Animasyon geçişli oynatılıyor: {targetObj.name}, Animasyon={_animationName}, Süre={_crossFadeDuration}s");
            }
            else
            {
                animator.Play(_animationName);
                Debug.Log($"Animasyon oynatılıyor: {targetObj.name}, Animasyon={_animationName}");
            }
            
            // Log: Animasyon oynatma başarılı
            LogManager.LogSuccess($"Animasyon oynatıldı: {targetObj.name} -> {_animationName}");
        }
    }
} 