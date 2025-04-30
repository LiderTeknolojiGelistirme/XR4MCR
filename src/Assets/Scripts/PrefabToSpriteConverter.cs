//using UnityEngine;
//using System.IO;
//using System.Collections;
//#if UNITY_EDITOR
//using UnityEditor;
//#endif
//using System.Linq;

//public class PrefabToSpriteConverter : MonoBehaviour
//{
//    public string prefabsFolderPath = "Assets/Objects/Prefabs";
//    public string spritesFolderPath = "Assets/Objects/Sprites";
//    public int textureSize = 512;
//    public bool autoSave = true;
//    public float cameraDistance = 2f; // Kamera mesafesi

//    private void Start()
//    {
//        if (autoSave)
//        {
//            StartCoroutine(ConvertAllPrefabsInFolder());
//        }
//    }

//    /// <summary>
//    /// Prefab klasöründeki tüm prefab'leri tarayıp sprite'ları oluşturur.
//    /// </summary>
//    public IEnumerator ConvertAllPrefabsInFolder()
//    {
//#if UNITY_EDITOR
//        // Prefab'lerin olduğu klasörü kontrol et
//        string[] prefabPaths = Directory.GetFiles(prefabsFolderPath, "*.prefab", SearchOption.AllDirectories);

//        if (prefabPaths.Length == 0)
//        {
//            Debug.LogWarning($"No prefabs found in folder: {prefabsFolderPath}");
//            yield break;
//        }

//        // Sprite klasörü yoksa oluştur
//        if (!AssetDatabase.IsValidFolder(spritesFolderPath))
//        {
//            Debug.Log($"Creating Sprites folder at: {spritesFolderPath}");
//            string parentFolder = Path.GetDirectoryName(spritesFolderPath);
//            string folderName = Path.GetFileName(spritesFolderPath);
//            AssetDatabase.CreateFolder(parentFolder, folderName);
//        }

//        foreach (string prefabPath in prefabPaths)
//        {
//            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
//            if (prefab != null)
//            {
//                yield return StartCoroutine(ConvertPrefabToSprite(prefab, prefabPath));
//            }
//        }
//#endif
//    }

//    /// <summary>
//    /// Prefab'in sprite'ını oluştur ve kaydet.
//    /// </summary>
//    public IEnumerator ConvertPrefabToSprite(GameObject prefab, string prefabPath)
//    {
//#if UNITY_EDITOR
//        // Prefab'in adını al
//        string prefabName = prefab.name;

//        // Sprite kaydedilecek yol
//        string spritePath = Path.Combine(spritesFolderPath, prefabName + "_Sprite.png").Replace("\\", "/");

//        // Eğer sprite zaten varsa güncellenir
//        if (File.Exists(spritePath))
//        {
//            Debug.Log($"Updating sprite for {prefabName}...");
//        }
//        else
//        {
//            Debug.Log($"Creating new sprite for {prefabName}...");
//        }

//        // Prefab'ten geçici bir instance oluştur
//        GameObject instance = Instantiate(prefab);

//        // Scene view kamera referansı al
//        SceneView sceneView = SceneView.lastActiveSceneView;
//        if (sceneView == null)
//        {
//            Debug.LogError("No active Scene view found!");
//            Destroy(instance);
//            yield break;
//        }

//        // Nesneyi kameraya göre konumlandır
//        PositionCameraForObject(sceneView.camera, instance);

//        // Bir frame bekle, ayarlar otursun
//        yield return null;

//        // Render texture oluştur
//        RenderTexture renderTexture = new RenderTexture(textureSize, textureSize, 24);
//        sceneView.camera.targetTexture = renderTexture;

//        // Objeyi render et
//        sceneView.camera.Render();

//        // Render'ı Texture2D'ye aktar
//        Texture2D texture = new Texture2D(textureSize, textureSize);
//        RenderTexture.active = renderTexture;
//        texture.ReadPixels(new Rect(0, 0, textureSize, textureSize), 0, 0);
//        texture.Apply();

//        // PNG olarak sprite'ı kaydet
//        File.WriteAllBytes(spritePath, texture.EncodeToPNG());

//        // AssetDatabase'i güncelle
//        AssetDatabase.Refresh();

//        // Temizlik
//        RenderTexture.active = null;
//        sceneView.camera.targetTexture = null;
//        Destroy(renderTexture);
//        Destroy(texture);
//        Destroy(instance);

//        Debug.Log($"Sprite for {prefab.name} saved at {spritePath}");
//#endif
//    }

//    /// <summary>
//    /// Nesneyi kameraya göre konumlandırır.
//    /// </summary>
//    private void PositionCameraForObject(Camera camera, GameObject obj)
//    {
//        // Nesnenin sınırlarını al
//        Bounds bounds = new Bounds();
//        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();

//        if (renderers.Length > 0)
//        {
//            bounds = renderers[0].bounds;
//            for (int i = 1; i < renderers.Length; i++)
//            {
//                bounds.Encapsulate(renderers[i].bounds);
//            }
//        }
//        else
//        {
//            bounds.center = obj.transform.position;
//            bounds.size = Vector3.one;
//        }

//        // Kamera mesafesini ayarla
//        float objectSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
//        float distance = objectSize * cameraDistance;

//        // Kamerayı objenin önüne yerleştir
//        camera.transform.position = bounds.center - camera.transform.forward * distance;
//        camera.transform.LookAt(bounds.center);
//    }

//    /// <summary>
//    /// Tek bir prefab'in sprite'ını oluşturur.
//    /// </summary>
//    public void ConvertSinglePrefab(GameObject prefab)
//    {
//        StartCoroutine(ConvertPrefabToSprite(prefab, AssetDatabase.GetAssetPath(prefab)));
//    }
//}
