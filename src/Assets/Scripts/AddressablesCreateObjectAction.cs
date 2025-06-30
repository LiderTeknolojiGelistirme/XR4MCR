using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;
using Viroo.Interactions;
using System.Collections.Generic;
using Virtualware.Networking.Client;
using Microsoft.Extensions.Logging;
using Virtualware.Networking;
using Networking.Messages;
using System.Linq;

public class AddressablesCreateObjectAction : InternalCreateObjectAction
{
    // Addressables Settings
    [SerializeField] private bool useAddressablesInsteadOfRegistry = true;
    
    // Addressables handle tracking
    private static Dictionary<string, AsyncOperationHandle<GameObject>> loadedPrefabs = 
        new Dictionary<string, AsyncOperationHandle<GameObject>>();
    
    private static bool isApplicationQuitting = false;

    #if UNITY_EDITOR
    [UnityEditor.InitializeOnLoadMethod]
    private static void InitializeInEditor()
    {
        // Editörde domain reload sonrası cleanup
        CleanupInvalidHandles();
        
        // Application quit detection
        UnityEditor.EditorApplication.wantsToQuit += OnEditorWantsToQuit;
    }
    
    private static bool OnEditorWantsToQuit()
    {
        CleanupAllPrefabs();
        return true;
    }
    #endif

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeInRuntime()
    {
        Application.quitting += OnApplicationQuitting;
    }
    
    private static void OnApplicationQuitting()
    {
        isApplicationQuitting = true;
        CleanupAllPrefabs();
    }

    protected override async void LocalExecuteImplementation(string data)
    {
        Debug.Log($"🔥 ADDRESSABLES: Başladı - ID: {InstantiatePrefabId}");
        
        if (useAddressablesInsteadOfRegistry && !string.IsNullOrEmpty(InstantiatePrefabId))
        {
            try
            {
                await LoadPrefabFromAddressables();
                Debug.Log($"✅ ADDRESSABLES: Prefab yüklendi - {InstantiatePrefabId}");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"⚠️ ADDRESSABLES: Hata, normal registry kullanılacak - {e.Message}");
            }
        }
        
        // Normal flow'u devam ettir
        base.LocalExecuteImplementation(data);
    }

    private async UniTask LoadPrefabFromAddressables()
    {
        Debug.Log($"🔥 ADDRESSABLES: Yükleniyor - {InstantiatePrefabId}");
        
        // Geçersiz handle'ları temizle
        CleanupInvalidHandles();
        
        // Zaten yüklü mü kontrol et
        if (loadedPrefabs.ContainsKey(InstantiatePrefabId))
        {
            var existingHandle = loadedPrefabs[InstantiatePrefabId];
            if (existingHandle.IsValid() && existingHandle.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log($"✅ ADDRESSABLES: Cache'den alındı - {InstantiatePrefabId}");
                return;
            }
            else
            {
                // Geçersiz handle'ı temizle
                loadedPrefabs.Remove(InstantiatePrefabId);
            }
        }
        
        // Addressables'dan yükle
        var handle = Addressables.LoadAssetAsync<GameObject>(InstantiatePrefabId);
        var prefab = await handle.ToUniTask();
        
        if (prefab != null)
        {
            // Başarılı yükleme - handle'ı sakla
            loadedPrefabs[InstantiatePrefabId] = handle;
            Debug.Log($"✅ ADDRESSABLES: Yüklendi - {prefab.name}");
        }
        else
        {
            // Başarısız yükleme - handle'ı release et
            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }
            throw new System.Exception($"Prefab yüklenemedi: {InstantiatePrefabId}");
        }
    }

    private static void CleanupInvalidHandles()
    {
        if (loadedPrefabs == null) return;
        
        var keysToRemove = new List<string>();
        foreach (var kvp in loadedPrefabs)
        {
            if (!kvp.Value.IsValid() || kvp.Value.Status == AsyncOperationStatus.Failed)
            {
                keysToRemove.Add(kvp.Key);
            }
        }
        
        foreach (var key in keysToRemove)
        {
            loadedPrefabs.Remove(key);
        }
    }

    // Cleanup
    void OnDestroy()
    {
        if (!isApplicationQuitting)
        {
            // Sadece bu instance ile ilgili cleanup yapabiliriz
            // Static dictionary'yi boşaltmıyoruz çünkü başka instance'lar kullanıyor olabilir
        }
    }

    private static void CleanupAllPrefabs()
    {
        if (loadedPrefabs == null) return;
        
        Debug.Log($"🧹 ADDRESSABLES: Cleanup - {loadedPrefabs.Count} item");
        
        foreach (var kvp in loadedPrefabs)
        {
            if (kvp.Value.IsValid())
            {
                Addressables.Release(kvp.Value);
                Debug.Log($"🧹 Released: {kvp.Key}");
            }
        }
        loadedPrefabs.Clear();
    }
}