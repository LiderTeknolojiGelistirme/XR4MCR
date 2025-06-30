using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Bu bileşeni bir nesneye eklediğinizde, diğer tüm gerekli bileşenler otomatik olarak eklenecektir.
/// </summary>
public class AutoGrabComponentAdder : MonoBehaviour
{
    private void Reset()
    {
        // Bu metod, component ilk eklendiğinde veya Reset butonuna basıldığında çalışır
        AddAllComponents();
    }

    private void AddAllComponents()
    {
        GameObject obj = this.gameObject;
        
        // Rigidbody ekle veya düzenle
        Rigidbody rb;
        if (!obj.GetComponent<Rigidbody>())
        {
            // Rigidbody yoksa ekle
            rb = obj.AddComponent<Rigidbody>();
        }
        else
        {
            // Zaten varsa al ve düzenle
            rb = obj.GetComponent<Rigidbody>();
        }
            
        // Her durumda Rigidbody ayarlarını düzenle
        rb.useGravity = true;
        rb.isKinematic = false;
        // Tüm konum ve rotasyon eksenlerini dondur
        rb.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;

        // Box Collider ekle ve nesnenin sınırlarına göre ayarla
        if (!obj.GetComponent<BoxCollider>())
        {
            BoxCollider boxCollider = obj.AddComponent<BoxCollider>();
            AdjustBoxColliderToMeshBounds(obj, boxCollider);
        }
        else
        {
            // Var olan Box Collider'ı nesnenin sınırlarına göre ayarla
            BoxCollider existingCollider = obj.GetComponent<BoxCollider>();
            AdjustBoxColliderToMeshBounds(obj, existingCollider);
        }

        // Boundary Restrictor ekle
        if (!obj.GetComponent<BoundaryRestrictor>())
        {
            obj.AddComponent<BoundaryRestrictor>();
        }

        // Interactable With Gizmo ekle
        if (!obj.GetComponent<InteractableWithGizmo>())
        {
            obj.AddComponent<InteractableWithGizmo>();
        }

        // Unity XR Interaction Toolkit - XRGrabInteractable
        TryAddComponent("UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable");
        
        // Network bileşenleri
        TryAddComponent("Virtualware.Networking.Client.NetworkObject");
        TryAddComponent("Virtualware.Networking.Client.Components.NetworkTransformSettings");
        TryAddComponent("Virtualware.Networking.Client.Components.NetworkTransform");
        
        // Viroo Lab özel bileşenleri
        TryAddComponent("Viroo.Interactions.Grab.VirooXRGrabInteractable");

        var xrSimpleInteractable = obj.GetComponent<XRSimpleInteractable>();
        if (xrSimpleInteractable != null)
        {
            DestroyImmediate(xrSimpleInteractable);
        }
        // Diğer bileşenler
        TryAddComponent("_3rd_Party.Outline.Outline");
        TryAddComponent("Helpers.InteractionHelper");

        // NetworkObject bileşenini al
        var networkObject = obj.GetComponent(typeof(Virtualware.Networking.Client.NetworkObject));
        if (networkObject != null)
        {
            // Scene bilgisini al
            var scene = obj.scene;
            // GenerateRandomId fonksiyonunu çağır
            var generateRandomIdMethod = typeof(Virtualware.Networking.Client.NetworkObject).GetMethod("GenerateRandomId", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            string objectIdValue = null;
            if (generateRandomIdMethod != null)
            {
                objectIdValue = (string)generateRandomIdMethod.Invoke(null, new object[] { scene });
            }
            else
            {
                // Fallback: elle oluştur
                objectIdValue = obj.name + "-" + System.Guid.NewGuid().ToString();
            }

            // ObjectId property'sine değeri ata
            var objectIdProp = networkObject.GetType().GetProperty("ObjectId");
            if (objectIdProp != null && objectIdProp.CanWrite)
            {
                objectIdProp.SetValue(networkObject, objectIdValue);
            }
        }
        
        // Editor'deyse bileşenleri sırala
#if UNITY_EDITOR
        if (UnityEditor.EditorApplication.isPlaying == false)
        {
            // Bileşen eklendikten sonra sıralamayı düzenle
            UnityEditor.EditorApplication.delayCall += () => 
            {
                if (this != null && gameObject != null)
                {
                    // GameObject'i tekrar seç
                    UnityEditor.Selection.activeGameObject = gameObject;
                    
                    // Sıralama işlemini Editor API aracılığıyla yap
                    UnityEditor.EditorApplication.ExecuteMenuItem("GameObject/3D Nesnesi Bileşenleri/Bileşenleri Sırala");
                }
            };
        }
#endif

        // İşlem tamamlandıktan sonra bu bileşeni kaldır
        // Ancak bu işlemi bir frame sonra yap, aksi halde Unity hata verebilir
        StartCoroutine(RemoveSelfNextFrame());
    }

    // Box Collider'ı nesnenin sınırlarına göre ayarlayan metod
    private void AdjustBoxColliderToMeshBounds(GameObject obj, BoxCollider boxCollider)
    {
        // MeshRenderer veya SkinnedMeshRenderer bul
        Renderer renderer = obj.GetComponent<MeshRenderer>();
        if (renderer == null)
        {
            renderer = obj.GetComponent<SkinnedMeshRenderer>();
        }

        // Alt nesnelerdeki tüm renderer'ları topla
        if (renderer == null)
        {
            // Tüm alt nesnelerdeki renderer'ları topla
            Renderer[] childRenderers = obj.GetComponentsInChildren<Renderer>();
            if (childRenderers.Length > 0)
            {
                // İlk renderer'ı bounds hesaplamak için başlangıç noktası olarak kullan
                Bounds combinedBounds = childRenderers[0].bounds;
                
                // Diğer tüm renderer'ların bounds'larını birleştir
                for (int i = 1; i < childRenderers.Length; i++)
                {
                    combinedBounds.Encapsulate(childRenderers[i].bounds);
                }
                
                // Box Collider'ı birleştirilmiş bounds'a göre ayarla
                // Local koordinatlara çevir (renderer bounds'ları world space'de, collider local space'de)
                boxCollider.center = obj.transform.InverseTransformPoint(combinedBounds.center);
                
                // Size hesaplaması için lokal ölçekleri dikkate al
                Vector3 worldSize = combinedBounds.size;
                Vector3 localSize = new Vector3(
                    worldSize.x / obj.transform.lossyScale.x,
                    worldSize.y / obj.transform.lossyScale.y,
                    worldSize.z / obj.transform.lossyScale.z
                );
                boxCollider.size = localSize;
                
                return;
            }
        }
        
        // Renderer bulunduysa bounds'a göre ayarla
        if (renderer != null)
        {
            Bounds bounds = renderer.bounds;
            
            // Local koordinatlara çevir (renderer bounds'ları world space'de, collider local space'de)
            boxCollider.center = obj.transform.InverseTransformPoint(bounds.center);
            
            // Size hesaplaması için lokal ölçekleri dikkate al
            Vector3 worldSize = bounds.size;
            Vector3 localSize = new Vector3(
                worldSize.x / obj.transform.lossyScale.x,
                worldSize.y / obj.transform.lossyScale.y,
                worldSize.z / obj.transform.lossyScale.z
            );
            boxCollider.size = localSize;
            
            return;
        }
        
        // Renderer yoksa MeshFilter'a bak
        MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
        if (meshFilter != null && meshFilter.sharedMesh != null)
        {
            Bounds bounds = meshFilter.sharedMesh.bounds;
            boxCollider.center = bounds.center;
            boxCollider.size = bounds.size;
            return;
        }
        
        // Hiçbir mesh bulunamadıysa varsayılan olarak bırak
        Debug.Log($"{obj.name} nesnesinde mesh bulunamadı, box collider varsayılan değerlerle kalacak.");
    }

    private System.Collections.IEnumerator RemoveSelfNextFrame()
    {
        yield return null; // Bir frame bekle
        DestroyImmediate(this); // Bu bileşeni kaldır
    }

    private void TryAddComponent(string componentTypeName)
    {
        System.Type componentType = System.Type.GetType(componentTypeName);
        
        // Tam ad bulunamadıysa, tüm assembly'leri kontrol edelim
        if (componentType == null)
        {
            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if (type.Name == componentTypeName || type.FullName == componentTypeName)
                        {
                            componentType = type;
                            break;
                        }
                    }
                
                    if (componentType != null)
                        break;
                }
                catch (System.Exception)
                {
                    // Bazı assembly'ler GetTypes() çağrısında hata verebilir, bunları atlayalım
                    continue;
                }
            }
        }

        if (componentType != null)
        {
            if (gameObject.GetComponent(componentType) == null)
            {
                gameObject.AddComponent(componentType);
            }
        }
    }
} 