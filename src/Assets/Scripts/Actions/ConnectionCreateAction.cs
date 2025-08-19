using System;
using System.Linq;
using UnityEngine;
using Viroo.Interactions;
using Presenters;
using Managers;
using Zenject;
using Models;

namespace Actions
{
    /// <summary>
    /// Viroo Actions sistemi kullanarak connection'ları senkronize eder
    /// </summary>
    public class ConnectionCreateAction : BroadcastObjectAction
    {
        
        [Inject] ConnectionPresenterFactory factory;
        [Inject] GraphManager graphManager;
        [HideInInspector]public ConnectionPresenter _createdConnectionPresenter;
        PortPresenter _sourcePort, _targetPort;
        
        protected override void LocalExecuteImplementation(string data)
        {
            LogManager.Log("ConnectionCreateAction Executed.");
            _createdConnectionPresenter = factory.CreateConnection(_sourcePort, _targetPort);
            if (_createdConnectionPresenter != null)
            {
                graphManager.ConnectionPresenters.Add(_createdConnectionPresenter);

                // Bilgi paneline bağlantı oluşturuldu logu ekle
                LogManager.LogInteraction(
                    $"Connection created: {_sourcePort.gameObject.name} -> {_targetPort.gameObject.name}");
            }
            else
            {
                LogManager.LogWarning("Connection creation failed - Factory returned null");

                // Bilgi paneline bağlantı başarısız logu ekle
                LogManager.LogWarning(
                    $"Connection failed: {_sourcePort.gameObject.name} -> {_targetPort.gameObject.name}");
            }
        }

        public ConnectionPresenter CreateConnection(PortPresenter sourcePort, PortPresenter targetPort)
        {
            _sourcePort = sourcePort;
            _targetPort = targetPort;
            Execute();
            return _createdConnectionPresenter;
            
        }
    }
} 