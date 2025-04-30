using Enums;
using Helpers;
using Managers;
using UnityEngine;
using Zenject;

namespace Factories
{
    public class ObjectFactory : PlaceholderFactory<ObjectType, GameObject>
    {
        private readonly DiContainer _container;
        private readonly NodeConfig _config;
        private readonly SystemManager _systemManager;

        public ObjectFactory(DiContainer container, NodeConfig config, SystemManager systemManager)
        {
            _container = container;
            _config = config;
            _systemManager = systemManager;
        }

        public override GameObject Create(ObjectType objectType)
        {
            GameObject prefabToInstantiate;
            
            // Nesne tipine göre prefabı seç
            switch (objectType)
            {
                case ObjectType.Capsule:
                    prefabToInstantiate = _config.capsulePrefab;
                    break;
                case ObjectType.Cube:
                    prefabToInstantiate = _config.cubePrefab;
                    break;
                case ObjectType.Cylinder:
                    prefabToInstantiate = _config.cylinderPrefab;
                    break;
                case ObjectType.Sphere:
                    prefabToInstantiate = _config.spherePrefab;
                    break;
                case ObjectType.Robot:
                    prefabToInstantiate = _config.robotPrefab;
                    break;
                case ObjectType.Barrier:
                    prefabToInstantiate = _config.barrierPrefab;
                    break;
                case ObjectType.BrownDesk:
                    prefabToInstantiate = _config.browndeskPrefab;
                    break;
                case ObjectType.EmergencyButton:
                    prefabToInstantiate = _config.emergencybuttonPrefab;
                    break;
                case ObjectType.Chair:
                    prefabToInstantiate = _config.chairPrefab;
                    break;
                case ObjectType.Chassis:
                    prefabToInstantiate = _config.chassisPrefab;
                    break;
                case ObjectType.Glasses:
                    prefabToInstantiate = _config.glassesPrefab;
                    break;
                case ObjectType.Gloves:
                    prefabToInstantiate = _config.glovesPrefab;
                    break;
                case ObjectType.Helmet:
                    prefabToInstantiate = _config.helmetPrefab;
                    break;
                case ObjectType.Kabinet:
                    prefabToInstantiate = _config.kabinetPrefab;
                    break;
                case ObjectType.Kawasaki:
                    prefabToInstantiate = _config.kawasakiPrefab;
                    break;
                case ObjectType.NightStand:
                    prefabToInstantiate = _config.nightstandPrefab;
                    break;
                case ObjectType.WhiteDesk:
                    prefabToInstantiate = _config.whitedeskPrefab;
                    break;
                case ObjectType.YellowLine:
                    prefabToInstantiate = _config.yellowlinePrefab;
                    break;
                default:
                    Debug.LogError("Bilinmeyen ObjectType: " + objectType);
                    return null;
            }
            
            // Prefab null kontrolü
            if (prefabToInstantiate == null)
            {
                Debug.LogError($"{objectType} tipi için prefab NodeConfig'te tanımlı değil!");
                return null;
            }

            // DiContainer üzerinden instantiate et (enjeksiyonların çalışması için)
            GameObject instance = _container.InstantiatePrefab(prefabToInstantiate);
            
            // Nesne oluşturma sonrası işlemler
            var interactionHelper = instance.GetComponent<InteractionHelper>();
            if (interactionHelper == null)
            {
                Debug.LogWarning($"{objectType} prefabında InteractionHelper bileşeni bulunamadı!");
            }
            
            return instance;
        }
    }
} 