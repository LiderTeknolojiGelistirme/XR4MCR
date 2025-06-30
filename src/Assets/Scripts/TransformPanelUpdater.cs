using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Managers;
using Zenject;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TransformPanelUpdater : MonoBehaviour
{
    public static TransformPanelUpdater Instance;

    [Header("Target Object")]
    public SelectedObjectReference selectedObjectReference;
    private GameObject go;

    [Header("UI - General Info")]
    public TMP_Text selectedObjectName;
    public Image selectedObjectSprite;

    [Header("Default Sprite")]
    public Sprite defaultCubeSprite; // Inspector'dan Unity küpü sprite'ını ata

    [Header("UI - Position Fields")]
    public TMP_InputField posX;
    public TMP_InputField posY;
    public TMP_InputField posZ;

    [Header("UI - Rotation Fields")]
    public TMP_InputField rotX;
    public TMP_InputField rotY;
    public TMP_InputField rotZ;

    [Header("UI - Scale Fields")]
    public TMP_InputField scaleX;
    public TMP_InputField scaleY;
    public TMP_InputField scaleZ;

    void Awake()
    {
        Instance = this;
        selectedObjectReference.OnSelectedObjectChanged += HandleSelectedObjectChanged;
        
        // Eğer default sprite atanmamışsa, runtime'da oluştur
        if (defaultCubeSprite == null)
        {
            CreateDefaultCubeSprite();
        }
    }

    private void HandleSelectedObjectChanged(GameObject gameObjectRef)
    {
        go = gameObjectRef;
    }

    private void Update()
    {
        if (go == null) 
        {
            // Seçili obje yoksa default değerleri göster
            selectedObjectName.text = "No Object Selected";
            selectedObjectSprite.sprite = defaultCubeSprite;
            
            // Transform değerlerini sıfırla
            posX.text = "0.00";
            posY.text = "0.00";
            posZ.text = "0.00";
            
            rotX.text = "0.00";
            rotY.text = "0.00";
            rotZ.text = "0.00";
            
            scaleX.text = "1.00";
            scaleY.text = "1.00";
            scaleZ.text = "1.00";
            
            return;
        }

        int cloneIndex = go.name.IndexOf("(Clone)");
        string displayName = cloneIndex > 0 ? go.name.Substring(0, cloneIndex).Trim() : go.name;
        selectedObjectName.text = displayName;
        
        // Sprite'ı al, bulamazsa default küp kullan
        //Sprite sprite = GetObjectSprite(go);
        //selectedObjectSprite.sprite = sprite != null ? sprite : defaultCubeSprite;
        selectedObjectSprite.sprite = defaultCubeSprite;

        Vector3 pos = go.transform.position;
        posX.text = pos.x.ToString("F2");
        posY.text = pos.y.ToString("F2");
        posZ.text = pos.z.ToString("F2");

        Vector3 rot = go.transform.eulerAngles;
        rotX.text = rot.x.ToString("F2");
        rotY.text = rot.y.ToString("F2");
        rotZ.text = rot.z.ToString("F2");

        Vector3 scl = go.transform.localScale;
        scaleX.text = scl.x.ToString("F2");
        scaleY.text = scl.y.ToString("F2");
        scaleZ.text = scl.z.ToString("F2");
    }

    private void CreateDefaultCubeSprite()
    {
        // Unity GameObject ikonu için 64x64 piksel texture oluştur
        Texture2D iconTexture = new Texture2D(64, 64, TextureFormat.RGBA32, false);
        
        Color[] pixels = new Color[64 * 64];
        
        // Unity GameObject ikonu renkleri
        Color topFace = new Color(0.95f, 0.95f, 0.95f, 1f);     // Beyaz (üst yüz)
        Color leftFace = new Color(0.75f, 0.75f, 0.75f, 1f);    // Açık gri (sol yüz)
        Color rightFace = new Color(0.45f, 0.45f, 0.45f, 1f);   // Koyu gri (sağ yüz)
        Color outline = new Color(0.15f, 0.15f, 0.15f, 1f);     // Siyah kontur
        Color transparent = new Color(0f, 0f, 0f, 0f);           // Şeffaf
        
        // Tüm pikselleri şeffaf yap
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = transparent;
        }
        
        // Unity GameObject ikonunu çiz
        for (int y = 0; y < 64; y++)
        {
            for (int x = 0; x < 64; x++)
            {
                int index = y * 64 + x;
                
                // Üst yüz (eğik paralel kenar - daha geniş)
                if (IsInTopFace(x, y))
                {
                    if (IsTopFaceOutline(x, y))
                        pixels[index] = outline;
                    else
                        pixels[index] = topFace;
                }
                // Sol yüz (dikey yüz)
                else if (IsInLeftFace(x, y))
                {
                    if (IsLeftFaceOutline(x, y))
                        pixels[index] = outline;
                    else
                        pixels[index] = leftFace;
                }
                // Sağ yüz (eğik yüz)
                else if (IsInRightFace(x, y))
                {
                    if (IsRightFaceOutline(x, y))
                        pixels[index] = outline;
                    else
                        pixels[index] = rightFace;
                }
            }
        }
        
        iconTexture.SetPixels(pixels);
        iconTexture.Apply();
        
        // Sprite oluştur
        defaultCubeSprite = Sprite.Create(iconTexture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));
        defaultCubeSprite.name = "UnityGameObjectIcon";
        
        Debug.Log("Unity GameObject ikonu oluşturuldu!");
    }

    // Üst yüz - geniş eğik paralel kenar
    private bool IsInTopFace(int x, int y)
    {
        if (y < 42 || y > 58) return false;
        
        int leftEdge = 4 + (y - 42) / 2;      // Sol kenar hafif eğik
        int rightEdge = 52 + (y - 42) / 2;    // Sağ kenar hafif eğik
        
        return x >= leftEdge && x <= rightEdge;
    }

    private bool IsTopFaceOutline(int x, int y)
    {
        if (y < 42 || y > 58) return false;
        
        int leftEdge = 4 + (y - 42) / 2;
        int rightEdge = 52 + (y - 42) / 2;
        
        return x == leftEdge || x == rightEdge || y == 42 || y == 58;
    }

    // Sol yüz - dikey dikdörtgen
    private bool IsInLeftFace(int x, int y)
    {
        return x >= 4 && x <= 20 && y >= 6 && y <= 42;
    }

    private bool IsLeftFaceOutline(int x, int y)
    {
        return (x >= 4 && x <= 20 && y >= 6 && y <= 42) &&
               (x == 4 || x == 20 || y == 6 || y == 42);
    }

    // Sağ yüz - eğik dikdörtgen (daha geniş)
    private bool IsInRightFace(int x, int y)
    {
        if (y < 6 || y > 42) return false;
        
        int leftEdge = 20;                           // Sol kenar düz
        int rightEdge = 52 + (42 - y) / 2;          // Sağ kenar eğik
        
        return x >= leftEdge && x <= rightEdge;
    }

    private bool IsRightFaceOutline(int x, int y)
    {
        if (y < 6 || y > 42) return false;
        
        int leftEdge = 20;
        int rightEdge = 52 + (42 - y) / 2;
        
        return x == leftEdge || x == rightEdge || y == 6 || y == 42;
    }

    public Sprite GetObjectSprite(GameObject obj)
    {
        Debug.Log($"Sprite arıyor: {obj.name}");

        // 1. SpriteRenderer kontrol et
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
        {
            Debug.Log("SpriteRenderer'dan sprite bulundu!");
            return sr.sprite;
        }

        // 2. MeshRenderer kontrol et
        MeshRenderer mr = obj.GetComponent<MeshRenderer>();
        if (mr != null)
        {
            Debug.Log($"MeshRenderer bulundu. Material sayısı: {mr.materials.Length}");
            
            foreach (Material mat in mr.materials)
            {
                if (mat != null && mat.mainTexture != null)
                {
                    Debug.Log($"Material texture bulundu: {mat.mainTexture.name}");
                    
                    Texture2D tex = mat.mainTexture as Texture2D;
                    if (tex != null)
                    {
                        Debug.Log("Texture2D'ye çevrildi, sprite oluşturuluyor!");
                        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                    }
                    else
                    {
                        Debug.LogWarning("Texture, Texture2D'ye çevrilemedi!");
                    }
                }
            }
        }

        // 3. Child objelerde ara
        for (int i = 0; i < obj.transform.childCount; i++)
        {
            Transform child = obj.transform.GetChild(i);
            SpriteRenderer childSr = child.GetComponent<SpriteRenderer>();
            if (childSr != null && childSr.sprite != null)
            {
                Debug.Log("Child'da SpriteRenderer bulundu!");
                return childSr.sprite;
            }

            MeshRenderer childMr = child.GetComponent<MeshRenderer>();
            if (childMr != null && childMr.material != null && childMr.material.mainTexture != null)
            {
                Texture2D tex = childMr.material.mainTexture as Texture2D;
                if (tex != null)
                {
                    Debug.Log("Child'da MeshRenderer texture bulundu!");
                    return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                }
            }
        }

#if UNITY_EDITOR
        // 4. Editor preview dene
        Texture2D preview = AssetPreview.GetAssetPreview(obj);
        if (preview != null)
        {
            Debug.Log("AssetPreview bulundu!");
            return Sprite.Create(preview, new Rect(0, 0, preview.width, preview.height), new Vector2(0.5f, 0.5f));
        }
#endif

        Debug.LogWarning($"Hiçbir sprite bulunamadı: {obj.name}");
        return null; // null döndür, Update'te default sprite kullanılacak
    }
}
