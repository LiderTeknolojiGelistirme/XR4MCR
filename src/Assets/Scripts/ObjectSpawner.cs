using UnityEngine;
using UnityEngine.UI;
using Enums;
using Factories;
using Zenject;

public class ObjectSpawner : MonoBehaviour
{
    [SerializeField] private ObjectType objectTypeToSpawn = ObjectType.Capsule; // Varsayılan olarak kapsül
    public Transform objectTransformPosition;
    private Transform parentTransform;
    private ObjectFactory _objectFactory;

    [Inject]
    public void Construct(ObjectFactory objectFactory)
    {
        _objectFactory = objectFactory;
    }

    private void Start()
    {
        parentTransform = objectTransformPosition.gameObject.transform.parent;
    }

    public void OnSpawnButtonClicked()
    {
        if (_objectFactory == null)
        {
            Debug.LogError("ObjectFactory enjekte edilmemiş! Zenject kurulumunu kontrol edin.");
            return;
        }

        // Spawn pozisyonunu hesapla
        Vector3 spawnPosition = objectTransformPosition.position;

        // ObjectFactory kullanarak prefabı oluştur
        GameObject spawnedObject = _objectFactory.Create(objectTypeToSpawn);
        
        if (spawnedObject != null)
        {
            // Pozisyon ve parent'ı ayarla
            spawnedObject.transform.position = spawnPosition;
            spawnedObject.transform.SetParent(parentTransform);
        }
    }

    // Farklı nesne tipleri için kullanılabilecek metodlar
    public void SpawnCapsule()
    {
        objectTypeToSpawn = ObjectType.Capsule;
        OnSpawnButtonClicked();
    }

    public void SpawnCube()
    {
        objectTypeToSpawn = ObjectType.Cube;
        OnSpawnButtonClicked();
    }
}
