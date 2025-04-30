using Zenject;
using UnityEngine;
using Presenters;
using Managers;
using Models;
using System.Collections.Generic;

public class ConnectionPresenterFactory : PlaceholderFactory<ConnectionPresenter>
{
    private readonly DiContainer _container;
    private readonly GraphManager _graphManager;
    private ConnectionPresenter _previewConnection;

    public ConnectionPresenterFactory(DiContainer container, GraphManager graphManager)
    {
        _container = container;
        _graphManager = graphManager;
    }

    public override ConnectionPresenter Create()
    {
        // Yeni GameObject oluşturup, canvas altına ekliyoruz.
        var connectionGO = new GameObject("Connection");
        // "false" parametresi ile yerel pozisyonların korunmasını sağlıyoruz.
        connectionGO.transform.SetParent(_graphManager.Canvas.transform, false);
        
        // ConnectionPresenter bileşenini instantiate edip injection uyguluyoruz.
        var connectionPresenter = _container.InstantiateComponent<ConnectionPresenter>(connectionGO);
        _container.Inject(connectionPresenter);
        
        return connectionPresenter;
    }

    /// <summary>
    /// Gerçek bir connection (bağlantı) oluşturur.
    /// </summary>
    /// <param name="sourcePort">Bağlantının başlangıç portu</param>
    /// <param name="targetPort">Bağlantının bitiş portu</param>
    /// <returns>Oluşturulan ConnectionPresenter veya geçersizse null</returns>
    public ConnectionPresenter CreateConnection(PortPresenter sourcePort, PortPresenter targetPort)
    {
        if (sourcePort == null || targetPort == null)
        {
            Debug.LogError("CreateConnection failed: Source or target port is null");
            return null;
        }

        if (!IsValidConnection(sourcePort, targetPort))
        {
            Debug.LogWarning($"Invalid connection attempt between ports: {sourcePort.ID} -> {targetPort.ID}");
            return null;
        }

        var connectionPresenter = Create();

        if(connectionPresenter == null)
        {
            Debug.LogError("ConnectionPresenter oluşturulamadı!");
            return null;
        }
    
        var connection = new Connection(sourcePort, targetPort);
        connectionPresenter.Initialize(connection);
    
        Debug.Log($"ConnectionPresenter başarıyla oluşturuldu: {sourcePort.ID} -> {targetPort.ID}");
    
        return connectionPresenter;
    }


    /// <summary>
    /// Preview (geçici) bağlantı oluşturur.
    /// </summary>
    /// <param name="startPort">Bağlantının başlangıç portu</param>
    /// <returns>Oluşturulan preview ConnectionPresenter</returns>
    public ConnectionPresenter CreatePreviewConnection(PortPresenter startPort)
    {
        if (_previewConnection != null)
            Object.Destroy(_previewConnection.gameObject);

        _previewConnection = Create();
        
        // Geçici bir connection model'i oluşturuyoruz
        var tempConnection = new Connection(
            startPort,
            null
        );
        
        _previewConnection.Initialize(tempConnection);
        return _previewConnection;
    }

    // public void UpdatePreviewConnection(Vector2 endPosition)
    // {
    //     if (_previewConnection != null)
    //     {
    //         _previewConnection.UpdatePreviewPosition(endPosition);
    //     }
    // }

    /// <summary>
    /// Portlar arası bağlantının geçerli olup olmadığını kontrol eder.
    /// </summary>
    private bool IsValidConnection(PortPresenter output, PortPresenter input)
    {
        if (output == null || input == null)
            return false;

        // Aynı node üzerindeki portların bağlanmaması
        if (output == input)
            return false;

        // Port tiplerinin uyumluluğu (detaylandırılabilir)
        if (!ArePortTypesCompatible(output.Model, input.Model))
            return false;

        // Eğer giriş portu zaten bir bağlantıya sahipse
        if (HasExistingConnection(input))
            return false;

        return true;
    }

    /// <summary>
    /// Portların tiplerinin uyumunu kontrol eder.
    /// Şu an için her durumda true döndürülüyor, ihtiyaç halinde detaylandırılabilir.
    /// </summary>
    private bool ArePortTypesCompatible(Port outputPort, Port inputPort)
    {
        return true;
    }

    /// <summary>
    /// Belirtilen port üzerinde mevcut bir bağlantı olup olmadığını kontrol eder.
    /// Şu an için her durumda false döndürülüyor, ihtiyaç halinde kontrol eklenebilir.
    /// </summary>
    private bool HasExistingConnection(PortPresenter port)
    {
        return false;
    }

    
}
