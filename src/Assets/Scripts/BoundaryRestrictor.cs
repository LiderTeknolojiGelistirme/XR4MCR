using UnityEngine;

public class BoundaryRestrictor : MonoBehaviour
{
    // Sýnýr deðerleri yerel uzayda
    private float minX = -1f, maxX = 1f;
    private float minY = 0f, maxY = 2f;
    private float minZ = -2.5f, maxZ = 2.5f;

    void Update()
    {
        // Nesnenin yerel pozisyonunu al
        Vector3 currentLocalPosition = transform.localPosition;

        // X, Y ve Z eksenlerinde pozisyonu sýnýrlayýn
        currentLocalPosition.x = Mathf.Clamp(currentLocalPosition.x, minX, maxX);
        currentLocalPosition.y = Mathf.Clamp(currentLocalPosition.y, minY, maxY);
        currentLocalPosition.z = Mathf.Clamp(currentLocalPosition.z, minZ, maxZ);

        // Güncellenmiþ yerel pozisyonu nesneye uygula
        transform.localPosition = currentLocalPosition;
    }
}
