using Factories;
using UnityEngine;
using Zenject;
using Enums;

namespace Examples
{
    public class ObjectFactoryExample : MonoBehaviour
    {
        private ObjectFactory _objectFactory;

        [Inject]
        public void Construct(ObjectFactory objectFactory)
        {
            _objectFactory = objectFactory;
        }

        // Unity Inspector'da göstermek ve test etmek için buton 
        [ContextMenu("Create Capsule")]
        public void CreateCapsule()
        {
            if (_objectFactory == null)
            {
                Debug.LogError("ObjectFactory enjekte edilmemiş! Bu hatayı görüyorsanız, GraphSceneInstaller'ı kontrol edin.");
                return;
            }

            GameObject capsule = _objectFactory.Create(ObjectType.Capsule);
            Debug.Log("Capsule oluşturuldu: " + capsule.name);
            
            // İsterseniz oluşturulan nesneye pozisyon atayabilirsiniz
            capsule.transform.position = new Vector3(0, 1, 0);
        }

        [ContextMenu("Create Cube")]
        public void CreateCube()
        {
            if (_objectFactory == null)
            {
                Debug.LogError("ObjectFactory enjekte edilmemiş!");
                return;
            }

            GameObject cube = _objectFactory.Create(ObjectType.Cube);
            Debug.Log("Cube oluşturuldu: " + cube.name);
            
            cube.transform.position = new Vector3(1, 1, 0);
        }

        // Kullanıcı arabiriminden çağrılacak metot
        public void CreateObjectAtPosition(ObjectType objectType, Vector3 position)
        {
            if (_objectFactory == null)
            {
                Debug.LogError("ObjectFactory enjekte edilmemiş!");
                return;
            }

            GameObject obj = _objectFactory.Create(objectType);
            obj.transform.position = position;
        }
    }
} 