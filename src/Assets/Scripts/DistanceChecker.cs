using UnityEngine;

public class DistanceChecker : MonoBehaviour
{
    [SerializeField] private Transform trackedPoint;
    [SerializeField] private float maxAllowedDistance = 0.5f; // Metre cinsinden maksimum izin verilen uzaklık
    
    private RepositionRoboticPart repositionScript;
    private bool isRepositioning = false;

    void Start()
    {
        if (trackedPoint == null)
        {
            Debug.LogError("TrackedPoint referansı eksik!");
            return;
        }

        // RepositionRoboticPart scriptini al
        repositionScript = GetComponent<RepositionRoboticPart>();
        if (repositionScript == null)
        {
            Debug.LogError("RepositionRoboticPart scripti bulunamadı!");
        }
        
        // Başlangıçta scripti devre dışı bırak
        if (repositionScript != null)
        {
            repositionScript.enabled = false;
        }
    }

    void Update()
    {
        if (trackedPoint == null || repositionScript == null) return;

        // TrackedPoint ile nesne arasındaki mesafeyi hesapla
        float distance = Vector3.Distance(transform.position, trackedPoint.position);

        // Eğer mesafe izin verilen maksimum değeri aşarsa
        if (distance > maxAllowedDistance && !isRepositioning)
        {
            isRepositioning = true;
            repositionScript.enabled = true;
        }
        // Mesafe normale döndüğünde
        else if (distance <= maxAllowedDistance && isRepositioning)
        {
            isRepositioning = false;
            repositionScript.enabled = false;
        }
    }
} 