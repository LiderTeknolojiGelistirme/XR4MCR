using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Text;
using System.Xml;
using TMPro;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceLocations;

#if UNITY_EDITOR
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
#endif

public class WebDAVAddressablesDownloader : MonoBehaviour
{
    [Header("WebDAV Configuration")]
    public string webdavUrl = "https://kai.nl.tab.digital/remote.php/dav/files/username/";
    public string username = "your_username";
    public string password = "your_password";

    [Header("Status Display")]
    public TMP_Text statusText;

    // Doğru klasör yolları
    private string streamingAssetsPath;
    private string addressablesPath;
    private string logFilePath;
    private string catalogPath;
    private string settingsPath;

    // Instantiate edilen objeleri takip etmek için
    private static Dictionary<GameObject, UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<GameObject>> trackedObjects 
        = new Dictionary<GameObject, UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<GameObject>>();

    // Asset handle'larını takip etmek için
    private static Dictionary<string, UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<GameObject>> trackedAssets 
        = new Dictionary<string, UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<GameObject>>();

    private static bool isApplicationQuitting = false;

    #if UNITY_EDITOR
    [UnityEditor.InitializeOnLoadMethod]
    private static void InitializeInEditor()
    {
        // Editörde domain reload sonrası cleanup
        CleanupInvalidTrackedObjects();
        CleanupInvalidTrackedAssets();
        
        // Application quit detection
        UnityEditor.EditorApplication.wantsToQuit += OnEditorWantsToQuit;
    }
    
    private static bool OnEditorWantsToQuit()
    {
        CleanupAllResources();
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
        CleanupAllResources();
    }

    void Start()
    {
        SetupPaths();
        LogToFile("=== WebDAV Addressables Downloader Started ===");
        LogToFile($"Persistent Data Path: {Application.persistentDataPath}");
        LogToFile($"Streaming Assets Path: {streamingAssetsPath}");
        LogToFile($"Addressables Path: {addressablesPath}");
        LogToFile($"WebDAV URL: {webdavUrl}");

        UpdateStatus("WebDAV Addressables Downloader hazır.");
    }

    private void SetupPaths()
    {
        #if UNITY_EDITOR
        // Editörde Library klasörüne indir (Addressables'ın okuduğu yer)
        string libraryPath = Path.GetDirectoryName(Application.dataPath); // Project root
        string platformName = UnityEditor.EditorUserBuildSettings.activeBuildTarget.ToString();
        streamingAssetsPath = Path.Combine(libraryPath, "Library", "com.unity.addressables");
        addressablesPath = Path.Combine(streamingAssetsPath, "aa", platformName);
        #else
        // Build'de StreamingAssets klasöründen oku
        streamingAssetsPath = Path.Combine(Application.streamingAssetsPath, "aa");
        addressablesPath = streamingAssetsPath;
        #endif
        
        // Catalog ve settings dosyaları
        catalogPath = Path.Combine(addressablesPath, "catalog.json");
        settingsPath = Path.Combine(addressablesPath, "settings.json");
        
        // Log dosyası yolu
        logFilePath = Path.Combine(Application.persistentDataPath, "webdav_debug.log");

        // Log dosyasını temizle
        if (File.Exists(logFilePath))
        {
            File.Delete(logFilePath);
        }

        // Gerekli klasörleri oluştur
        EnsureDirectoryExists(streamingAssetsPath);
        EnsureDirectoryExists(addressablesPath);
        
        LogToFile($"Editor Mode: {Application.isEditor}");
        LogToFile($"Target Path: {addressablesPath}");
        LogToFile($"Catalog Path: {catalogPath}");
        LogToFile($"Settings Path: {settingsPath}");
    }

    private void EnsureDirectoryExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            LogToFile($"Klasör oluşturuldu: {path}");
        }
    }

    private void LogToFile(string message)
    {
        try
        {
            string logEntry = $"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
            File.AppendAllText(logFilePath, logEntry + "\n");
            Debug.Log(message); // Unity Console'a da yazdır
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Log yazma hatası: {e.Message}");
        }
    }

    private static void CleanupInvalidTrackedObjects()
    {
        if (trackedObjects == null) return;
        
        var keysToRemove = new List<GameObject>();
        foreach (var kvp in trackedObjects)
        {
            if (kvp.Key == null || !kvp.Value.IsValid())
            {
                keysToRemove.Add(kvp.Key);
            }
        }
        
        foreach (var key in keysToRemove)
        {
            trackedObjects.Remove(key);
        }
    }

    private static void CleanupInvalidTrackedAssets()
    {
        if (trackedAssets == null) return;
        
        var keysToRemove = new List<string>();
        foreach (var kvp in trackedAssets)
        {
            if (!kvp.Value.IsValid())
            {
                keysToRemove.Add(kvp.Key);
            }
        }
        
        foreach (var key in keysToRemove)
        {
            trackedAssets.Remove(key);
        }
    }

    private static void CleanupAllResources()
    {
        if (trackedObjects != null)
        {
            Debug.Log($"🧹 Cleaning up {trackedObjects.Count} tracked objects");
            foreach (var kvp in trackedObjects)
            {
                if (kvp.Key != null && kvp.Value.IsValid())
                {
                    UnityEngine.AddressableAssets.Addressables.Release(kvp.Value);
                    if (kvp.Key != null) Destroy(kvp.Key);
                }
            }
            trackedObjects.Clear();
        }

        if (trackedAssets != null)
        {
            Debug.Log($"🧹 Cleaning up {trackedAssets.Count} tracked assets");
            foreach (var kvp in trackedAssets)
            {
                if (kvp.Value.IsValid())
                {
                    UnityEngine.AddressableAssets.Addressables.Release(kvp.Value);
                }
            }
            trackedAssets.Clear();
        }
    }



    // YENİ METOT: Addressables Cache'ini Temizle
    public void ClearAddressablesCache()
    {
        StartCoroutine(ClearAddressablesCacheCoroutine());
    }

    private IEnumerator ClearAddressablesCacheCoroutine()
    {
        UpdateStatus("🧹 Addressables cache temizleniyor...");
        LogToFile("ClearAddressablesCache başladı");

        // 1. Editor'da ise Addressables build cache'ini temizle
        #if UNITY_EDITOR
        LogToFile("Editor modunda - Addressables build cache temizleniyor");
        try
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings != null)
            {
                AddressableAssetSettings.CleanPlayerContent(settings.ActivePlayerDataBuilder);
                LogToFile("✅ Editor Addressables build cache temizlendi");
            }
        }
        catch (System.Exception e)
        {
            LogToFile($"⚠️ Editor cache cleanup hatası: {e.Message}");
        }
        #endif

        // 2. Addressables cache klasörünü temizle
        if (Directory.Exists(addressablesPath))
        {
            LogToFile($"Addressables cache klasörü temizleniyor: {addressablesPath}");
            
            string[] files = Directory.GetFiles(addressablesPath, "*", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                if (TryDeleteFile(file))
                {
                    LogToFile($"Silindi: {Path.GetFileName(file)}");
                }
                yield return null;
            }

            string[] directories = Directory.GetDirectories(addressablesPath, "*", SearchOption.AllDirectories);
            System.Array.Reverse(directories);
            
            foreach (string dir in directories)
            {
                if (TryDeleteEmptyDirectory(dir))
                {
                    LogToFile($"Boş klasör silindi: {Path.GetFileName(dir)}");
                }
                yield return null;
            }

            LogToFile("✅ Addressables cache klasörü temizlendi");
        }

        // 3. Persistent Data Path'teki Unity Addressables cache'ini temizle
        string persistentCachePath = Path.Combine(Application.persistentDataPath, "com.unity.addressables");
        if (Directory.Exists(persistentCachePath))
        {
            LogToFile($"Unity persistent cache temizleniyor: {persistentCachePath}");
            if (TryDeleteDirectory(persistentCachePath))
            {
                LogToFile("✅ Unity persistent Addressables cache temizlendi");
            }
        }

        // 4. Unity'nin geçici cache klasörlerini temizle
        string tempCachePath = Path.Combine(Application.temporaryCachePath, "Addressables");
        if (Directory.Exists(tempCachePath))
        {
            LogToFile($"Unity temporary cache temizleniyor: {tempCachePath}");
            if (TryDeleteDirectory(tempCachePath))
            {
                LogToFile("✅ Unity temporary Addressables cache temizlendi");
            }
        }

        // 5. Klasörleri yeniden oluştur
        EnsureDirectoryExists(addressablesPath);

        UpdateStatus("✅ Addressables cache başarıyla temizlendi!");
        LogToFile("ClearAddressablesCache tamamlandı");
    }

    // Yardımcı metodlar
    private bool TryDeleteFile(string filePath)
    {
        try
        {
            File.Delete(filePath);
            return true;
        }
        catch (System.Exception e)
        {
            LogToFile($"❌ Dosya silme hatası {Path.GetFileName(filePath)}: {e.Message}");
            return false;
        }
    }

    private bool TryDeleteEmptyDirectory(string dirPath)
    {
        try
        {
            if (Directory.GetFiles(dirPath).Length == 0 && Directory.GetDirectories(dirPath).Length == 0)
            {
                Directory.Delete(dirPath);
                return true;
            }
            return false;
        }
        catch (System.Exception e)
        {
            LogToFile($"❌ Klasör silme hatası {Path.GetFileName(dirPath)}: {e.Message}");
            return false;
        }
    }

    private bool TryDeleteDirectory(string dirPath)
    {
        try
        {
            Directory.Delete(dirPath, true);
            return true;
        }
        catch (System.Exception e)
        {
            LogToFile($"❌ Klasör silme hatası {Path.GetFileName(dirPath)}: {e.Message}");
            return false;
        }
    }

    // Ana senkronizasyon fonksiyonu
    public void StartWebDAVSync()
    {
        LogToFile("=== StartWebDAVSync çağrıldı ===");
        LogToFile($"WebDAV URL: '{webdavUrl}'");
        LogToFile($"Username: '{username}'");
        LogToFile($"Password set: {!string.IsNullOrEmpty(password)}");

        if (string.IsNullOrEmpty(webdavUrl) || string.IsNullOrEmpty(username))
        {
            LogToFile("❌ WebDAV URL veya kullanıcı adı boş!");
            UpdateStatus("❌ WebDAV URL ve kullanıcı adı gerekli!");
            return;
        }

        UpdateStatus("🔄 WebDAV senkronizasyonu başlatılıyor...");
        LogToFile("StartWebDAVSync - Coroutine başlatılıyor");
        StartCoroutine(SyncAddressablesFromWebDAV());
    }

    // Cache temizleyip ardından sync yapan birleşik metot
    public void ClearCacheAndSync()
    {
        StartCoroutine(ClearCacheAndSyncCoroutine());
    }

    private IEnumerator ClearCacheAndSyncCoroutine()
    {
        // Önce cache'i temizle
        yield return StartCoroutine(ClearAddressablesCacheCoroutine());
        
        // Sonra sync yap
        if (!string.IsNullOrEmpty(webdavUrl) && !string.IsNullOrEmpty(username))
        {
            yield return StartCoroutine(SyncAddressablesFromWebDAV());
        }
    }

    private IEnumerator SyncAddressablesFromWebDAV()
    {
        LogToFile("SyncAddressablesFromWebDAV başladı");
        
        // Addressables klasörüne sync yap
        yield return StartCoroutine(ListAndDownloadDirectory("", addressablesPath));
        
        UpdateStatus("✅ Senkronizasyon tamamlandı!");
        LogToFile("Senkronizasyon tamamlandı");
        LogToFile($"Log dosyası konumu: {logFilePath}");
    }

    private IEnumerator ListAndDownloadDirectory(string remotePath, string localPath)
    {
        string fullUrl = webdavUrl.TrimEnd('/') + "/" + remotePath.TrimStart('/');

        LogToFile($"=== ListAndDownloadDirectory başladı ===");
        LogToFile($"Remote Path: '{remotePath}'");
        LogToFile($"Local Path: '{localPath}'");
        LogToFile($"Full URL: '{fullUrl}'");

        // Local klasör var mı kontrol et
        EnsureDirectoryExists(localPath);

        // PROPFIND request ile klasör içeriğini listele
        UnityWebRequest request = new UnityWebRequest(fullUrl, "PROPFIND");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(GetPropfindXml()));
        request.downloadHandler = new DownloadHandlerBuffer();

        // WebDAV headers
        request.SetRequestHeader("Depth", "1");
        request.SetRequestHeader("Content-Type", "application/xml");
        request.SetRequestHeader("Authorization", GetBasicAuth());

        UpdateStatus($"📁 Listeleniyor: {remotePath}");
        LogToFile($"PROPFIND request gönderiliyor: {fullUrl}");

        yield return request.SendWebRequest();

        LogToFile($"Request tamamlandı - Result: {request.result}");
        LogToFile($"Response Code: {request.responseCode}");

        if (request.result == UnityWebRequest.Result.Success)
        {
            LogToFile($"✅ Request başarılı - Response Code: {request.responseCode}");

            // XML response'u parse et
            List<WebDAVItem> items = ParseWebDAVResponse(request.downloadHandler.text, remotePath);

            LogToFile($"Parse sonucu: {items.Count} item bulundu");

            foreach (var item in items)
            {
                LogToFile($"İşleniyor: {item.name} (IsDir: {item.isDirectory})");

                if (item.isDirectory)
                {
                    // Alt klasör - recursive olarak işle
                    string subLocalPath = Path.Combine(localPath, item.name);
                    EnsureDirectoryExists(subLocalPath);

                    string subRemotePath = remotePath + "/" + item.name;
                    LogToFile($"Recursively calling for: {subRemotePath}");
                    yield return StartCoroutine(ListAndDownloadDirectory(subRemotePath, subLocalPath));
                }
                else
                {
                    // Dosya - indir
                    string fileLocalPath = Path.Combine(localPath, item.name);
                    string fileRemotePath = remotePath + "/" + item.name;
                    LogToFile($"Dosya indirilecek: {fileRemotePath} -> {fileLocalPath}");
                    yield return StartCoroutine(DownloadFile(fileRemotePath, fileLocalPath));
                }
            }
        }
        else
        {
            LogToFile($"❌ Request hatası: {request.error}");
            LogToFile($"HTTP Kod: {request.responseCode}");
            LogToFile($"Response Text: {request.downloadHandler.text}");
            UpdateStatus($"❌ Klasör listeleme hatası: {remotePath}");
        }

        request.Dispose();
    }

    private IEnumerator DownloadFile(string remotePath, string localPath)
    {
        string fullUrl = webdavUrl.TrimEnd('/') + "/" + remotePath.TrimStart('/');
        LogToFile($"DownloadFile - Remote: {remotePath}, Local: {localPath}, URL: {fullUrl}");

        // Dosya zaten varsa ve aynı boyuttaysa atla
        if (File.Exists(localPath))
        {
            // Size kontrolü için HEAD request
            UnityWebRequest headRequest = UnityWebRequest.Head(fullUrl);
            headRequest.SetRequestHeader("Authorization", GetBasicAuth());
            yield return headRequest.SendWebRequest();

            if (headRequest.result == UnityWebRequest.Result.Success)
            {
                string contentLength = headRequest.GetResponseHeader("Content-Length");
                if (!string.IsNullOrEmpty(contentLength))
                {
                    long remoteSize = long.Parse(contentLength);
                    long localSize = new FileInfo(localPath).Length;

                    if (remoteSize == localSize)
                    {
                        LogToFile($"⏭️ Dosya zaten güncel: {Path.GetFileName(localPath)}");
                        headRequest.Dispose();
                        yield break;
                    }
                }
            }
            headRequest.Dispose();
        }

        // Dosyayı indir
        UnityWebRequest request = UnityWebRequest.Get(fullUrl);
        request.SetRequestHeader("Authorization", GetBasicAuth());

        UpdateStatus($"⬇️ İndiriliyor: {Path.GetFileName(localPath)}");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            byte[] data = request.downloadHandler.data;

            if (data != null && data.Length > 0)
            {
                try
                {
                    // Klasör yoksa oluştur
                    string directory = Path.GetDirectoryName(localPath);
                    EnsureDirectoryExists(directory);

                    File.WriteAllBytes(localPath, data);
                    LogToFile($"✅ İndirildi: {localPath} ({data.Length} bytes)");
                }
                catch (System.Exception e)
                {
                    LogToFile($"❌ Dosya yazma hatası: {e.Message}");
                }
            }
        }
        else
        {
            LogToFile($"❌ Dosya indirme hatası: {request.error}");
            LogToFile($"HTTP Kod: {request.responseCode}");
        }

        request.Dispose();
    }

    private string GetPropfindXml()
    {
        return @"<?xml version=""1.0""?>
                <d:propfind xmlns:d=""DAV:"">
                    <d:prop>
                        <d:displayname/>
                        <d:getcontentlength/>
                        <d:getcontenttype/>
                        <d:resourcetype/>
                        <d:getlastmodified/>
                    </d:prop>
                </d:propfind>";
    }

    private string GetBasicAuth()
    {
        string credentials = $"{username}:{password}";
        string encodedCredentials = System.Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));
        return $"Basic {encodedCredentials}";
    }

    private List<WebDAVItem> ParseWebDAVResponse(string xmlResponse, string currentRemotePath = "")
    {
        List<WebDAVItem> items = new List<WebDAVItem>();

        try
        {
            LogToFile($"ParseWebDAVResponse başladı - Current Remote Path: '{currentRemotePath}'");

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlResponse);

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("d", "DAV:");

            XmlNodeList responses = doc.SelectNodes("//d:response", nsmgr);
            LogToFile($"XML'de {responses.Count} response bulundu");

            foreach (XmlNode response in responses)
            {
                XmlNode hrefNode = response.SelectSingleNode("d:href", nsmgr);
                XmlNode resourceTypeNode = response.SelectSingleNode(".//d:resourcetype", nsmgr);
                XmlNode displayNameNode = response.SelectSingleNode(".//d:displayname", nsmgr);

                if (hrefNode != null)
                {
                    string href = hrefNode.InnerText.Trim();
                    string displayName = displayNameNode?.InnerText?.Trim();
                    bool isDirectory = resourceTypeNode?.SelectSingleNode("d:collection", nsmgr) != null;

                    LogToFile($"Raw Item - HREF: '{href}', DisplayName: '{displayName}', IsDir: {isDirectory}");

                    // Base URL'den sonraki kısmı al
                    string relativePath = href;
                    if (href.StartsWith(webdavUrl))
                    {
                        relativePath = href.Substring(webdavUrl.Length).TrimStart('/');
                    }
                    else if (href.Contains("/remote.php/dav/files/"))
                    {
                        // NextCloud formatı için alternatif parsing
                        int startIndex = href.IndexOf("/remote.php/dav/files/");
                        if (startIndex >= 0)
                        {
                            string afterDav = href.Substring(startIndex + "/remote.php/dav/files/".Length);
                            if (afterDav.Contains("/"))
                            {
                                int userEndIndex = afterDav.IndexOf("/");
                                relativePath = afterDav.Substring(userEndIndex + 1);
                            }
                        }
                    }

                    LogToFile($"Processed - Relative Path: '{relativePath}'");

                    // Root klasörü atla
                    if (string.IsNullOrEmpty(relativePath) || relativePath == "/" || relativePath.Trim() == "")
                    {
                        LogToFile("Skipping: Empty/root path");
                        continue;
                    }

                    // Şu anda sorguladığımız klasörün kendisini atla
                    string normalizedCurrentPath = currentRemotePath.Trim('/');
                    string normalizedRelativePath = relativePath.Trim('/');

                    if (normalizedRelativePath == normalizedCurrentPath && isDirectory)
                    {
                        LogToFile($"Skipping: Current directory '{normalizedRelativePath}' == '{normalizedCurrentPath}'");
                        continue;
                    }

                    // Dosya/klasör adını çıkar
                    string name = displayName;
                    if (string.IsNullOrEmpty(name))
                    {
                        name = Path.GetFileName(relativePath.TrimEnd('/'));
                    }

                    if (!string.IsNullOrEmpty(name))
                    {
                        LogToFile($"Adding Item: '{name}' ({(isDirectory ? "Directory" : "File")})");
                        items.Add(new WebDAVItem
                        {
                            name = name,
                            href = href,
                            isDirectory = isDirectory
                        });
                    }
                    else
                    {
                        LogToFile($"Skipping: Empty name for path '{relativePath}'");
                    }
                }
            }

            LogToFile($"Parse tamamlandı - Toplam {items.Count} item eklendi");
        }
        catch (System.Exception e)
        {
            LogToFile($"❌ XML parse hatası: {e.Message}");
            LogToFile($"Stack trace: {e.StackTrace}");
        }

        return items;
    }

    public static void TrackInstantiatedObject(GameObject obj, UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<GameObject> handle)
    {
        if (obj != null && handle.IsValid())
        {
            // Geçersiz handle'ları temizle
            CleanupInvalidTrackedObjects();
            
            trackedObjects[obj] = handle;
            Debug.Log($"🔍 Tracking object: {obj.name}");
        }
    }

    public static void UntrackObject(GameObject obj)
    {
        if (obj != null && trackedObjects.ContainsKey(obj))
        {
            trackedObjects.Remove(obj);
            Debug.Log($"🗑️ Stopped tracking object: {obj.name}");
        }
    }

    public static void ClearAllTrackedObjects()
    {
        if (!isApplicationQuitting)
        {
            foreach (var kvp in trackedObjects)
            {
                if (kvp.Key != null)
                {
                    UnityEngine.AddressableAssets.Addressables.Release(kvp.Value);
                    Destroy(kvp.Key);
                }
            }
        }
        trackedObjects.Clear();
        Debug.Log("🧹 All tracked objects cleared");
    }

    public static void TrackLoadedAsset(string assetId, UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<GameObject> handle)
    {
        if (!string.IsNullOrEmpty(assetId) && handle.IsValid())
        {
            // Geçersiz handle'ları temizle
            CleanupInvalidTrackedAssets();
            
            trackedAssets[assetId] = handle;
            Debug.Log($"🔍 Tracking loaded asset: {assetId}");
        }
    }

    public static void ReleaseTrackedAsset(string assetId)
    {
        if (trackedAssets.ContainsKey(assetId))
        {
            var handle = trackedAssets[assetId];
            if (handle.IsValid())
            {
                UnityEngine.AddressableAssets.Addressables.Release(handle);
            }
            trackedAssets.Remove(assetId);
            Debug.Log($"🗑️ Released tracked asset: {assetId}");
        }
    }

    public static void ClearAllTrackedAssets()
    {
        foreach (var kvp in trackedAssets)
        {
            if (kvp.Value.IsValid())
            {
                UnityEngine.AddressableAssets.Addressables.Release(kvp.Value);
            }
        }
        trackedAssets.Clear();
        Debug.Log("🧹 All tracked assets cleared");
    }

    void OnDestroy()
    {
        if (!isApplicationQuitting)
        {
            // Sadece bu instance'ın coroutine'lerini durdur
            StopAllCoroutines();
        }
    }

    private void UpdateStatus(string message)
    {
        LogToFile($"STATUS: {message}");
        if (statusText != null)
        {
            statusText.text = message;
        }
    }

    // Test fonksiyonu - WebDAV bağlantısını test et
    public void TestConnection()
    {
        StartCoroutine(TestWebDAVConnection());
    }

    private IEnumerator TestWebDAVConnection()
    {
        UpdateStatus("🔍 Bağlantı test ediliyor...");
        LogToFile("=== WebDAV Connection Test başladı ===");
        LogToFile($"Test URL: '{webdavUrl}'");
        LogToFile($"Username: '{username}'");
        LogToFile($"Password length: {password?.Length ?? 0}");

        UnityWebRequest request = new UnityWebRequest(webdavUrl, "PROPFIND");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(GetPropfindXml()));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Depth", "0");
        request.SetRequestHeader("Content-Type", "application/xml");
        request.SetRequestHeader("Authorization", GetBasicAuth());

        LogToFile("Test request gönderiliyor...");
        yield return request.SendWebRequest();

        LogToFile($"Test sonucu - Result: {request.result}");
        LogToFile($"Response Code: {request.responseCode}");

        if (request.result == UnityWebRequest.Result.Success)
        {
            LogToFile("✅ WebDAV bağlantı testi başarılı!");
            LogToFile($"Response: {request.downloadHandler.text}");
            UpdateStatus("✅ WebDAV bağlantısı başarılı!");
        }
        else
        {
            LogToFile($"❌ WebDAV bağlantı testi başarısız!");
            LogToFile($"Error: {request.error}");
            LogToFile($"Response: {request.downloadHandler.text}");
            UpdateStatus($"❌ Bağlantı hatası: {request.responseCode} - {request.error}");
        }

        request.Dispose();
        LogToFile("=== WebDAV Connection Test tamamlandı ===");
    }
}

[System.Serializable]
public class WebDAVItem
{
    public string name;
    public string href;
    public bool isDirectory;
}