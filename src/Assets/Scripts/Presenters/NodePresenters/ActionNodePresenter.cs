using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Models.Nodes;
using Managers;
using System.Threading.Tasks;
using System;

namespace Presenters.NodePresenters
{
    public class ActionNodePresenter : BaseNodePresenter
    {
        protected ActionNode ActionModel => Model as ActionNode;
        
        protected virtual void Awake()
        {
            // Alt sınıfların kendi yapılandırmaları için boş bırakıldı
        }
        
        protected virtual void OnDisable()
        {
            // Alt sınıfların temizleme işlemleri için boş bırakıldı
        }
        
        // BaseNodePresenter'daki Update metodunu override ediyoruz
        // böylece ActionNode'lar sadece event port tetiklendiğinde çalışacak
        protected override void Update()
        {
            // Boş bırakıyoruz, böylece Play() otomatik çağrılmayacak
            // ActionNode'lar sadece event tetiklendiğinde çalışacak
        }
        
        public override void Play()
        {
            Debug.Log($"ActionNodePresenter.Play çalıştırıldı: {Model.Title}");
            
            // Action'ı yürüt
            PerformAction();
        }
        
        protected virtual async void PerformAction()
        {
            // Alt sınıflar tarafından uygulanacak
            Debug.Log($"ActionNodePresenter.PerformAction çağrıldı: Tip={ActionModel.Type}, Hedef={ActionModel.TargetObjectID}");

            await PerformActionAsync();

            CompleteNode();
        }

        protected virtual async Task PerformActionAsync()
        {
            await Task.Delay( 1000 ); //default
        }

        protected void SetTargetObject(string value)
        {
            if (ActionModel != null)
            {
                ActionModel.TargetObjectID = value;
                Debug.Log($"Hedef nesne değiştirildi: {ActionModel.TargetObjectID}");
            }
        }
        
        // Model'de parametre adını günceller
        protected void SetParameterName(string value)
        {
            if (ActionModel != null)
            {
                ActionModel.ParameterName = value;
                Debug.Log($"Parametre adı güncellendi: {ActionModel.ParameterName}");
            }
        }
        
        // Model'de parametre değerini günceller
        protected void SetParameterValue(string value)
        {
            if (ActionModel != null)
            {
                ActionModel.ParameterValue = value;
                Debug.Log($"Parametre değeri güncellendi: {ActionModel.ParameterValue}");
            }
        }
        
        // Action tipini günceller
        protected void SetActionType(ActionNode.ActionType type)
        {
            if (ActionModel != null)
            {
                ActionModel.Type = type;
                Debug.Log($"Action tipi güncellendi: {ActionModel.Type}");
            }
        }
        
        // CompleteNode metodunu override et
        public override void CompleteNode()
        {
            base.CompleteNode();
            Model.IsCompleted = true;
            Debug.Log($"ActionNodePresenter.CompleteNode çağrıldı: {Model.Title}");
        }
    }
} 