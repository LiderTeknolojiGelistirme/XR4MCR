using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class Instantiator : MonoBehaviour
{
    public AssetReferenceGameObject cubePrefab;
    public AssetReferenceGameObject spherePrefab;

    // Instantiate edilen objelerin referanslarını tutmak için
    private GameObject currentCube;
    private AsyncOperationHandle<GameObject> cubeHandle;
    private GameObject currentSphere;
    private AsyncOperationHandle<GameObject> sphereHandle;

    private static bool isApplicationQuitting = false;

    #if UNITY_EDITOR
    [UnityEditor.InitializeOnLoadMethod]
    private static void InitializeInEditor()
    {
        // Editor quit detection
        UnityEditor.EditorApplication.wantsToQuit += OnEditorWantsToQuit;
    }
    
    private static bool OnEditorWantsToQuit()
    {
        isApplicationQuitting = true;
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
    }

    void Start()
    {
        // Asset reference'ları kontrol et
        ValidateAssetReferences();
    }

    private void ValidateAssetReferences()
    {
        Debug.Log("🔍 Asset reference'lar kontrol ediliyor...");
        
        if (cubePrefab == null)
        {
            Debug.LogError("❌ Cube prefab asset reference atanmamış!");
        }
        else if (!cubePrefab.RuntimeKeyIsValid())
        {
            Debug.LogError($"❌ Cube prefab runtime key geçersiz: {cubePrefab.AssetGUID}");
        }
        else
        {
            Debug.Log($"✅ Cube prefab OK: {cubePrefab.AssetGUID}");
        }

        if (spherePrefab == null)
        {
            Debug.LogError("❌ Sphere prefab asset reference atanmamış!");
        }
        else if (!spherePrefab.RuntimeKeyIsValid())
        {
            Debug.LogError($"❌ Sphere prefab runtime key geçersiz: {spherePrefab.AssetGUID}");
        }
        else
        {
            Debug.Log($"✅ Sphere prefab OK: {spherePrefab.AssetGUID}");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            InstantiateCube();
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            InstantiateSphere();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ReleaseCube();
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            ReleaseSphere();
        }
    }

    public async void InstantiateCube()
    {
        if (isApplicationQuitting) return;

        // Eğer zaten bir cube varsa önce onu release et
        if (currentCube != null)
        {
            ReleaseCube();
        }

        Debug.Log("🧊 Cube instantiate başlatılıyor...");

        // Prefab kontrolü
        if (cubePrefab == null)
        {
            Debug.LogError("❌ Cube prefab referansı null!");
            return;
        }

        if (!cubePrefab.RuntimeKeyIsValid())
        {
            Debug.LogError($"❌ Cube prefab runtime key geçersiz: {cubePrefab.AssetGUID}");
            return;
        }

        try
        {
            Debug.Log($"📦 Cube instantiate ediliyor - Key: {cubePrefab.AssetGUID}");
            
            var handle = cubePrefab.InstantiateAsync(Vector3.zero, Quaternion.identity);
            var gameObject = await handle.Task;
            
            // Task tamamlandıktan sonra application quitting kontrolü
            if (isApplicationQuitting)
            {
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
                return;
            }
            
            if (gameObject != null)
            {
                // Referansları sakla
                currentCube = gameObject;
                cubeHandle = handle;
                
                // WebDAV downloader tracking (opsiyonel)
                TryTrackWithWebDAV(gameObject, handle);
                
                Debug.Log($"✅ Cube başarıyla instantiate edildi: {gameObject.name}");
            }
            else
            {
                Debug.LogError("❌ Cube instantiate edilemedi - GameObject null");
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Cube instantiate hatası: {e.Message}");
            Debug.LogError($"Stack trace: {e.StackTrace}");
        }
    }

    public async void InstantiateSphere()
    {
        if (isApplicationQuitting) return;

        // Eğer zaten bir sphere varsa önce onu release et
        if (currentSphere != null)
        {
            ReleaseSphere();
        }

        Debug.Log("🔵 Sphere instantiate başlatılıyor...");

        // Prefab kontrolü
        if (spherePrefab == null)
        {
            Debug.LogError("❌ Sphere prefab referansı null!");
            return;
        }

        if (!spherePrefab.RuntimeKeyIsValid())
        {
            Debug.LogError($"❌ Sphere prefab runtime key geçersiz: {spherePrefab.AssetGUID}");
            return;
        }

        try
        {
            Debug.Log($"📦 Sphere instantiate ediliyor - Key: {spherePrefab.AssetGUID}");
            
            var handle = spherePrefab.InstantiateAsync(Vector3.zero, Quaternion.identity);
            var gameObject = await handle.Task;
            
            // Task tamamlandıktan sonra application quitting kontrolü
            if (isApplicationQuitting)
            {
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
                return;
            }
            
            if (gameObject != null)
            {
                // Referansları sakla
                currentSphere = gameObject;
                sphereHandle = handle;
                
                // WebDAV downloader tracking (opsiyonel)
                TryTrackWithWebDAV(gameObject, handle);
                
                Debug.Log($"✅ Sphere başarıyla instantiate edildi: {gameObject.name}");
            }
            else
            {
                Debug.LogError("❌ Sphere instantiate edilemedi - GameObject null");
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Sphere instantiate hatası: {e.Message}");
            Debug.LogError($"Stack trace: {e.StackTrace}");
        }
    }

    private void TryTrackWithWebDAV(GameObject obj, AsyncOperationHandle<GameObject> handle)
    {
        try
        {
            // WebDAV downloader varsa tracking sistemine kaydet
            WebDAVAddressablesDownloader.TrackInstantiatedObject(obj, handle);
        }
        catch (System.Exception e)
        {
            // WebDAV downloader yoksa veya hata varsa sessizce geç
            Debug.Log($"WebDAV tracking atlandı: {e.Message}");
        }
    }

    private void TryUntrackWithWebDAV(GameObject obj)
    {
        try
        {
            // WebDAV downloader varsa tracking sisteminden çıkar
            WebDAVAddressablesDownloader.UntrackObject(obj);
        }
        catch (System.Exception e)
        {
            // WebDAV downloader yoksa veya hata varsa sessizce geç
            Debug.Log($"WebDAV untracking atlandı: {e.Message}");
        }
    }

    public void ReleaseCube()
    {
        if (currentCube != null)
        {
            Debug.Log("🗑️ Cube release ediliyor...");
            try
            {
                // Tracking sisteminden çıkar (opsiyonel)
                TryUntrackWithWebDAV(currentCube);
                
                // GameObject'i yok et
                if (currentCube != null && !isApplicationQuitting)
                {
                    Destroy(currentCube);
                }
                
                // Handle'ı release et
                if (cubeHandle.IsValid())
                {
                    Addressables.Release(cubeHandle);
                }
                
                // Referansları temizle
                currentCube = null;
                cubeHandle = default;
                
                Debug.Log("✅ Cube başarıyla release edildi");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Cube release hatası: {e.Message}");
            }
        }
        else
        {
            Debug.Log("⚠️ Release edilecek cube bulunamadı");
        }
    }

    public void ReleaseSphere()
    {
        if (currentSphere != null)
        {
            Debug.Log("🗑️ Sphere release ediliyor...");
            try
            {
                // Tracking sisteminden çıkar (opsiyonel)
                TryUntrackWithWebDAV(currentSphere);
                
                // GameObject'i yok et
                if (currentSphere != null && !isApplicationQuitting)
                {
                    Destroy(currentSphere);
                }
                
                // Handle'ı release et
                if (sphereHandle.IsValid())
                {
                    Addressables.Release(sphereHandle);
                }
                
                // Referansları temizle
                currentSphere = null;
                sphereHandle = default;
                
                Debug.Log("✅ Sphere başarıyla release edildi");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Sphere release hatası: {e.Message}");
            }
        }
        else
        {
            Debug.Log("⚠️ Release edilecek sphere bulunamadı");
        }
    }

    void OnDestroy()
    {
        // Component destroy edildiğinde tüm referansları temizle
        if (!isApplicationQuitting)
        {
            ReleaseCube();
            ReleaseSphere();
        }
    }

    void OnApplicationPause(bool pauseStatus)
    {
        // Mobile platformlarda uygulama pause olduğunda güvenlik için
        if (pauseStatus)
        {
            Debug.Log("🔄 Application paused - Resources korunuyor");
        }
    }
}