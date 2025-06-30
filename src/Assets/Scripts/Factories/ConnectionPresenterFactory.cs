using Zenject;
using UnityEngine;
using Presenters;
using Managers;
using Models;
using System.Collections.Generic;
using Virtualware.Networking.Client;
using Interfaces;
using System.Threading.Tasks;
using System;

public class ConnectionPresenterFactory : MonoBehaviour
{
    [Inject] private DiContainer _container;
    [Inject] private GraphManager _graphManager;
    
    // VIROO Network Service'i
    private INetworkObjectsService _networkObjectsService;
    private ConnectionPresenter _previewConnection;
    
    // Connection prefab ID
    private const string CONNECTION_PREFAB_ID = "connection_prefab";

    private void Awake()
    {
        // VIROO injection sistemini başlat
        this.QueueForInject();
    }
    
    // VIROO injection method
    protected void Inject(INetworkObjectsService networkObjectsService)
    {
        _networkObjectsService = networkObjectsService;
    }

    public ConnectionPresenter Create()
    {
        // Senkron versiyon için async Create metodunu çağır
        var task = CreateAsync();
        task.Wait(); // Blocking call - mevcut interface'i korumak için
        return task.Result;
    }

    private async Task<ConnectionPresenter> CreateAsync()
    {
        try
        {
            if (_networkObjectsService == null)
            {
                Debug.LogError("[ConnectionPresenterFactory] NetworkObjectsService inject edilmemiş!");
                return null;
            }

            Debug.Log($"[ConnectionPresenterFactory] VIROO connection nesnesi oluşturuluyor...");
            
            // Canvas container'ı bularak o container'dan oluştur
            var canvasContainer = _graphManager.contentTransform.GetComponent<PrefabInstantiableContainer>();
            if (canvasContainer == null)
            {
                Debug.LogError("[ConnectionPresenterFactory] Canvas Content'inde PrefabInstantiableContainer bulunamadı!");
                return null;
            }
            
            // VIROO ile Canvas'da oluştur
            var createResponse = await _networkObjectsService.CreateDynamicObject(
                CONNECTION_PREFAB_ID,
                Vector3.zero,
                Quaternion.identity,
                requestAuthority: true,
                isPersistent: true,
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
            );

            if (createResponse.Success)
            {
                Debug.Log($"[ConnectionPresenterFactory] Connection nesnesi başarıyla oluşturuldu!");
                
                GameObject connectionGO = createResponse.InstantiatedObject.GameObject;
                
                // Canvas'da oluştu, transform'u düzelt
                ConfigureCanvasConnection(connectionGO);
                
                // Canvas ZenjectInjector ile inject et
                var connectionPresenter = TryInjectCanvasConnection(connectionGO);
                
                return connectionPresenter;
            }
            else
            {
                Debug.LogError($"[ConnectionPresenterFactory] Connection nesnesi oluşturulamadı!");
                return null;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[ConnectionPresenterFactory] Connection oluşturulurken hata: {e.Message}");
            return null;
        }
    }
    
    private void ConfigureCanvasConnection(GameObject connectionGO)
    {
        try
        {
            // Canvas altına ekle - "false" parametresi ile yerel pozisyonların korunmasını sağlıyoruz
            connectionGO.transform.SetParent(_graphManager.Canvas.transform, false);
            
            // RectTransform pozisyonunu ayarla
            RectTransform rectTransform = connectionGO.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = Vector2.zero;
                rectTransform.localScale = Vector3.one;
                
                // Z pozisyonunu 0'a eşitle (Canvas layer'ında olması için)
                Vector3 localPos = rectTransform.localPosition;
                rectTransform.localPosition = new Vector3(localPos.x, localPos.y, 0);
            }
            
            Debug.Log($"[ConnectionPresenterFactory] Connection konfigüre edildi");
        }
        catch (Exception e)
        {
            Debug.LogError($"[ConnectionPresenterFactory] Connection konfigüre edilirken hata: {e.Message}");
        }
    }
    
    private ConnectionPresenter TryInjectCanvasConnection(GameObject connectionGO)
    {
        try
        {
            // 1. Canvas Content'deki ZenjectInjector'ı kullan
            var canvasInjector = _graphManager.contentTransform.GetComponent<ZenjectInjector>();
            if (canvasInjector != null)
            {
                canvasInjector.InjectObject(connectionGO);
                Debug.Log($"[ConnectionPresenterFactory] {connectionGO.name} nesnesine Canvas ZenjectInjector ile injection uygulandı");
            }
            else
            {
                Debug.LogWarning("[ConnectionPresenterFactory] Canvas Content'inde ZenjectInjector bulunamadı!");
            }

            // 2. ConnectionPresenter bileşenini al veya ekle
            var connectionPresenter = connectionGO.GetComponent<ConnectionPresenter>();
            if (connectionPresenter == null)
            {
                // ConnectionPresenter bileşenini instantiate edip injection uyguluyoruz
                connectionPresenter = _container.InstantiateComponent<ConnectionPresenter>(connectionGO);
            }
            
            _container.Inject(connectionPresenter);
            
            Debug.Log($"[ConnectionPresenterFactory] {connectionGO.name} başarıyla inject edildi");
            return connectionPresenter;
        }
        catch (Exception e)
        {
            Debug.LogError($"[ConnectionPresenterFactory] Connection injection hatası: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// Gerçek bir connection (bağlantı) oluşturur.
    /// </summary>
    /// <param name="sourcePort">Bağlantının başlangıç portu</param>
    /// <param name="targetPort">Bağlantının bitiş portu</param>
    /// <returns>Oluşturulan ConnectionPresenter veya geçersizse null</returns>
    public ConnectionPresenter CreateConnection(PortPresenter firstPort, PortPresenter secondPort)
    {
        if (firstPort == null || secondPort == null)
        {
            Debug.LogError("CreateConnection failed: Source or target port is null");
            return null;
        }

        if (!IsValidConnection(firstPort, secondPort))
        {
            Debug.LogWarning($"Invalid connection attempt between ports: {firstPort.ID} -> {secondPort.ID}");
            return null;
        }

        var connectionPresenter = Create();

        if(connectionPresenter == null)
        {
            Debug.LogError("ConnectionPresenter oluşturulamadı!");
            return null;
        }

        // Port tiplerini kontrol et ve doğru source/target'ı belirle
        PortPresenter sourcePort = null;
        PortPresenter targetPort = null;
        
        if (firstPort.Polarity == NodeSystem.PolarityType.Output && secondPort.Polarity == NodeSystem.PolarityType.Input)
        {
            sourcePort = firstPort;   // Output → Input
            targetPort = secondPort;
        }
        else if (firstPort.Polarity == NodeSystem.PolarityType.Input && secondPort.Polarity == NodeSystem.PolarityType.Output)
        {
            sourcePort = secondPort;  // Input ← Output (ters çevir)
            targetPort = firstPort;
        }
        else if (firstPort.Polarity == NodeSystem.PolarityType.Bidirectional || secondPort.Polarity == NodeSystem.PolarityType.Bidirectional)
        {
            // Bidirectional durumda, Output varsa onu source yap
            if (firstPort.Polarity == NodeSystem.PolarityType.Output)
            {
                sourcePort = firstPort;
                targetPort = secondPort;
            }
            else if (secondPort.Polarity == NodeSystem.PolarityType.Output)
            {
                sourcePort = secondPort;
                targetPort = firstPort;
            }
            else
            {
                // İkisi de bidirectional veya input ise, ilk port'u source yap
                sourcePort = firstPort;
                targetPort = secondPort;
            }
        }
        else
        {
            // Default: İlk port source, ikinci port target
            sourcePort = firstPort;
            targetPort = secondPort;
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
            UnityEngine.Object.Destroy(_previewConnection.gameObject);

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
