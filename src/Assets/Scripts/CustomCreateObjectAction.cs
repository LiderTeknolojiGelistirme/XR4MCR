using System;
using System.Collections.Generic;
using UnityEngine;
using Viroo.Interactions;
using Virtualware.Networking.Client;
using Microsoft.Extensions.Logging;
using System.Linq;
using Cysharp.Threading.Tasks;

public class CustomCreateObjectAction : InternalCreateObjectAction
{
    // Sadece yaratılan nesneye erişim için
    public GameObject LastCreatedObject { get; private set; }
    private HashSet<int> existingObjectIDs = new HashSet<int>();
    
    protected override void Awake()
    {
        base.Awake();
        // Başlangıçta var olan tüm NetworkObject ID'lerini kaydet
        foreach (var no in FindObjectsOfType<NetworkObject>())
        {
            existingObjectIDs.Add(no.gameObject.GetInstanceID());
        }
        Debug.Log($"CustomCreateObjectAction: {existingObjectIDs.Count} existing NetworkObject found");
    }
    
    protected override async void LocalExecuteImplementation(string data)
    {
        Debug.Log($"CustomCreateObjectAction: LocalExecuteImplementation called with data: '{data}' and InstantiatePrefabId: '{InstantiatePrefabId}'");
        
        base.LocalExecuteImplementation(data);
        await UniTask.Delay(100);
        
        // Yeni eklenen objeyi bul
        NetworkObject[] allObjects = FindObjectsOfType<NetworkObject>();
        Debug.Log($"CustomCreateObjectAction: After base call, found {allObjects.Length} total NetworkObjects");
        
        foreach (var no in allObjects)
        {
            int id = no.gameObject.GetInstanceID();
            if (!existingObjectIDs.Contains(id))
            {
                LastCreatedObject = no.gameObject;
                existingObjectIDs.Add(id); // Listeye ekle
                Debug.Log($"CustomCreateObjectAction: Found new object: {no.gameObject.name}");
                break;
            }
        }
        
        if (LastCreatedObject == null)
        {
            Debug.LogWarning($"CustomCreateObjectAction: No new NetworkObject found after creating '{InstantiatePrefabId}'");
        }
    }
    
    // Async versiyonu - önerilen kullanım
    public async UniTask<GameObject> CreateAndGetObjectAsync()
    {
        Debug.Log($"CustomCreateObjectAction: CreateAndGetObjectAsync called for '{InstantiatePrefabId}'");
        LastCreatedObject = null;
        
        base.LocalExecuteImplementation("");
        
        // 150ms bekle - genellikle yeterli
        await UniTask.Delay(150);
        
        Debug.Log($"CustomCreateObjectAction: CreateAndGetObjectAsync result: {(LastCreatedObject != null ? LastCreatedObject.name : "NULL")}");
        return LastCreatedObject;
    }
    
    // Sync versiyonu - basit ve güvenli
    public GameObject CreateAndGetObject()
    {
        Debug.Log($"CustomCreateObjectAction: CreateAndGetObject called for '{InstantiatePrefabId}'");
        
        // Eğer async işlem zaten çalışıyorsa, sync çağrı yapmayı reddet
        try
        {
            LastCreatedObject = null;
            
            // Sync versiyonu için basit implementation
            base.LocalExecuteImplementation("");
            
            // Kısa bir süre bekle - blocking
            System.Threading.Thread.Sleep(200);
            
            // Manuel olarak yeni objeyi bul
            NetworkObject[] allObjects = FindObjectsOfType<NetworkObject>();
            Debug.Log($"CustomCreateObjectAction: After sync call, found {allObjects.Length} total NetworkObjects");
            
            foreach (var no in allObjects)
            {
                int id = no.gameObject.GetInstanceID();
                if (!existingObjectIDs.Contains(id))
                {
                    LastCreatedObject = no.gameObject;
                    existingObjectIDs.Add(id);
                    Debug.Log($"CustomCreateObjectAction: Found new sync object: {no.gameObject.name}");
                    break;
                }
            }
            
            if (LastCreatedObject == null)
            {
                Debug.LogError($"CustomCreateObjectAction: FAILED to create object with ID '{InstantiatePrefabId}'. No new NetworkObject found!");
            }
            else
            {
                Debug.Log($"CustomCreateObjectAction: SUCCESS - Created object: {LastCreatedObject.name}");
            }
            
            return LastCreatedObject;
        }
        catch (Exception ex)
        {
            Debug.LogError($"CreateAndGetObject failed: {ex.Message}");
            return null;
        }
    }
}