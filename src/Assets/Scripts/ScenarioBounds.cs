using UnityEngine;
using ModestTree;

public class ScenarioBounds : MonoBehaviour
{
    // Sınırlar için referanslar
    public Transform frontBoundary;
    public Transform backBoundary;
    public Transform leftBoundary;
    public Transform rightBoundary;
    public Transform topBoundary;
    public Transform bottomBoundary;
    
    // Sınırların pozisyonlarını sakla
    public Vector3 minBounds { get; private set; }
    public Vector3 maxBounds { get; private set; }
    
    void Awake()
    {
        // Editor'de atanmamış ise otomatik bul
        FindBoundaries();
        
        // Sınırlardan bounds hesapla
        CalculateBounds();
    }
    
    void FindBoundaries()
    {
        if (frontBoundary == null)
            frontBoundary = transform.Find("ScenarioBoundary_Front");
        if (backBoundary == null)
            backBoundary = transform.Find("ScenarioBoundary_Back");
        if (leftBoundary == null)
            leftBoundary = transform.Find("ScenarioBoundary_Left");
        if (rightBoundary == null)
            rightBoundary = transform.Find("ScenarioBoundary_Right");
        if (topBoundary == null)
            topBoundary = transform.Find("ScenarioBoundary_Top");
        if (bottomBoundary == null)
            bottomBoundary = transform.Find("ScenarioBoundary_Bottom");
        
        // Eğer hala bulunamadıysa, Cube isimlerine göre arama yap (geçici çözüm)
        if (frontBoundary == null) Log.Error("frontBoundary not found");
        if (backBoundary == null) Log.Error("backBoundary not found");
        if (leftBoundary == null) Log.Error("leftBoundary not found");
        if (rightBoundary == null) Log.Error("rightBoundary not found");
        if (topBoundary == null) Log.Error("topBoundary not found");
        if (bottomBoundary == null) Log.Error("bottomBoundary not found");
    }
    
    void CalculateBounds()
    {
        // Minimize et/Maksimize et yaklaşımıyla sınırları belirle
        minBounds = new Vector3(
            leftBoundary ? leftBoundary.localPosition.x : -1f,
            bottomBoundary ? bottomBoundary.localPosition.y : 0f,
            backBoundary ? backBoundary.localPosition.z : -2.5f
        );
        
        maxBounds = new Vector3(
            rightBoundary ? rightBoundary.localPosition.x : 1f,
            topBoundary ? topBoundary.localPosition.y : 2f,
            frontBoundary ? frontBoundary.localPosition.z : 2.5f
        );
    }
    
    // Verilen dünya pozisyonunu sınırlar içinde tut
    public Vector3 ClampPosition(Vector3 worldPosition)
    {
        // Dünya -> yerel dönüşümü
        Vector3 localPosition = transform.InverseTransformPoint(worldPosition);
        
        // Sınırla
        localPosition.x = Mathf.Clamp(localPosition.x, minBounds.x, maxBounds.x);
        localPosition.y = Mathf.Clamp(localPosition.y, minBounds.y, maxBounds.y);
        localPosition.z = Mathf.Clamp(localPosition.z, minBounds.z, maxBounds.z);
        
        // Yerel -> dünya dönüşümü
        return transform.TransformPoint(localPosition);
    }
} 