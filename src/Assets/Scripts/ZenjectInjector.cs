using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

/// <summary>
/// Parent altındaki tüm child nesneleri Zenject'e kaydeden yardımcı bileşen.
/// </summary>
public class ZenjectInjector : MonoBehaviour
{
    [Inject]
    private DiContainer _container;
    
    [Tooltip("Sadece en üst seviye child'ları inject et, alt seviye child'ları işleme")]
    [SerializeField]
    private bool _onlyDirectChildren = true;
    
    [Tooltip("Sadece belirli tag'e sahip nesneler mi işlensin?")]
    [SerializeField]
    private string _targetTag = "";
    
    // İşlenmiş nesneleri takip etmek için
    private List<GameObject> _processedObjects = new List<GameObject>();
    
    private void Awake()
    {
        // Eğer Inject ile DiContainer alınamadıysa, SceneContext'den almaya çalış
        if (_container == null)
        {
            var sceneContext = GameObject.FindObjectOfType<SceneContext>();
            if (sceneContext != null)
            {
                _container = sceneContext.Container;
                Debug.Log("[ZenjectInjector] DiContainer SceneContext'den alındı");
            }
            else
            {
                Debug.LogError("[ZenjectInjector] DiContainer bulunamadı! ZenjectInjector çalışmayacak.");
            }
        }
    }
    
    /// <summary>
    /// DiContainer'ı manuel olarak ayarlar (eğer Zenject sistemde kayıtlı değilse)
    /// </summary>
    public void SetDiContainer(DiContainer container)
    {
        _container = container;
        Debug.Log("[ZenjectInjector] DiContainer manuel olarak ayarlandı");
    }
    
    /// <summary>
    /// Manuel olarak child nesneleri inject etmek için çağrılır
    /// </summary>
    public void InjectAllChildren()
    {
        if (_container == null)
        {
            Debug.LogError("[ZenjectInjector] DiContainer null! İşlem yapılamıyor.");
            return;
        }
        
        if (_onlyDirectChildren)
        {
            // Sadece direkt (en üst seviye) child'ları işle
            InjectDirectChildren();
        }
        else
        {
            // Tüm alt nesneleri (tüm seviyeler) işle
            InjectAllChildrenRecursive();
        }
    }
    
    /// <summary>
    /// Sadece direkt child'ları inject eder, alt seviye child'ları işlemez
    /// </summary>
    private void InjectDirectChildren()
    {
        // Sadece ilk seviye child'ları al
        int childCount = transform.childCount;
        
        for (int i = 0; i < childCount; i++)
        {
            Transform child = transform.GetChild(i);
            
            // Tag kontrolü
            if (!string.IsNullOrEmpty(_targetTag) && !child.CompareTag(_targetTag))
            {
                continue;
            }
            
            // Bu nesne daha önce işlendi mi?
            if (_processedObjects.Contains(child.gameObject))
            {
                continue;
            }
            
            // Nesneyi Zenject'e tanıt
            ProcessGameObject(child.gameObject);
        }
        
        Debug.Log($"[ZenjectInjector] {childCount} adet direkt child işlendi");
    }
    
    /// <summary>
    /// Tüm child'ları ve alt seviye child'ları dahil inject eder
    /// </summary>
    private void InjectAllChildrenRecursive()
    {
        // Tüm alt nesneleri bul (parent dahil değil)
        Transform[] allChildren = GetComponentsInChildren<Transform>(true);
        int processedCount = 0;
        
        foreach (Transform child in allChildren)
        {
            // Parent'ı dahil etme
            if (child == transform)
            {
                continue;
            }
            
            // Tag kontrolü
            if (!string.IsNullOrEmpty(_targetTag) && !child.CompareTag(_targetTag))
            {
                continue;
            }
            
            // Bu nesne daha önce işlendi mi?
            if (_processedObjects.Contains(child.gameObject))
            {
                continue;
            }
            
            // Nesneyi Zenject'e tanıt
            ProcessGameObject(child.gameObject);
            processedCount++;
        }
        
        Debug.Log($"[ZenjectInjector] {processedCount} adet nesne işlendi (tüm seviyeler)");
    }
    
    private void ProcessGameObject(GameObject obj)
    {
        try
        {
            // Zenject DI uygula
            _container.InjectGameObject(obj);
            
            // İşlenmiş nesneler listesine ekle
            _processedObjects.Add(obj);
            
            Debug.Log($"[ZenjectInjector] {obj.name} nesnesine Zenject dependency injection uygulandı");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[ZenjectInjector] {obj.name} nesnesine injection uygulanırken hata: {e.Message}");
        }
    }
    
    /// <summary>
    /// Belirli bir nesneyi manuel olarak inject etmek için
    /// </summary>
    /// <param name="obj">İnject edilecek GameObject</param>
    public void InjectObject(GameObject obj)
    {
        if (_container == null)
        {
            Debug.LogError("[ZenjectInjector] DiContainer null! İşlem yapılamıyor.");
            return;
        }
        
        if (!_processedObjects.Contains(obj))
        {
            ProcessGameObject(obj);
        }
    }
    
    /// <summary>
    /// İşlenmiş nesneler listesini temizler
    /// </summary>
    public void ClearProcessedObjects()
    {
        _processedObjects.Clear();
        Debug.Log("[ZenjectInjector] İşlenmiş nesneler listesi temizlendi");
    }
}
