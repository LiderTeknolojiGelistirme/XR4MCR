using System.Collections;
using _3rd_Party.Outline;
using Models;
using UnityEngine;
using Virtualware.Networking.Client;
using Zenject;
using Managers;
using Enums;

namespace Presenters
{
    public class ObjectPresenter : MonoBehaviour
    {
        // Inspector'da ayarlanabilir ObjectType
        [SerializeField] private ObjectType _objectType = ObjectType.Unknown;
        
        private ObjectModel _model;

        private Outline _outline;

        [Inject]
        public void Construct()
        {
            // Dependency injection setup
        }

        private void Awake()
        {
            // Create a new model with default values
            _model = new ObjectModel();
            
            // Serialized field'dan ObjectType'ı ata
            _model.ObjectType = _objectType;

            _outline = GetComponent<Outline>();

            //Debug.Log($"Object created with ID: {_model.ID}");
            LogManager.LogInteraction($"Object created: {gameObject.name}, Type: {_model.ObjectType}");
        }

        public ObjectModel Model
        {
            get => _model;
            set => _model = value;
        }

        public void Initialize()
        {
            Debug.Log($"Object initialized with ID: {_model.ID}");
        }

        public void Remove()
        {
            Destroy(gameObject);
        }

        private IEnumerator Start()
        {
            while (GetComponent<NetworkObject>() == null)
            {
                yield return new WaitForSeconds(0.1f);
            }

            _model.ID = GetComponent<NetworkObject>().ObjectId;
            _model.Name = gameObject.name;

            // Unity'den model verilerine aktar
            TransformToModel();
        }

        

        private void DebugTransformValues()
        {
            if (_model == null)
            {
                Debug.Log("Model is null");
                return;
            }

            string debug = $"Position: ({_model.PosX:F2}, {_model.PosY:F2}, {_model.PosZ:F2})\n" +
                           $"Rotation: ({_model.RotX:F2}, {_model.RotY:F2}, {_model.RotZ:F2})\n" +
                           $"Scale: ({_model.ScaleX:F2}, {_model.ScaleY:F2}, {_model.ScaleZ:F2})";
            Debug.Log(debug);
        }

        // Unity Transform'dan modele değer aktar
        public void TransformToModel()
        {
            var t = transform;
            _model.PosX = t.position.x;
            _model.PosY = t.position.y;
            _model.PosZ = t.position.z;

            Vector3 euler = t.eulerAngles;
            _model.RotX = euler.x;
            _model.RotY = euler.y;
            _model.RotZ = euler.z;

            _model.ScaleX = t.localScale.x;
            _model.ScaleY = t.localScale.y;
            _model.ScaleZ = t.localScale.z;
        }

        // Model verilerini Unity Transform'a uygula
        public void ModelToTransform()
        {
            transform.position = new Vector3(_model.PosX, _model.PosY, _model.PosZ);
            transform.rotation = Quaternion.Euler(_model.RotX, _model.RotY, _model.RotZ);
            transform.localScale = new Vector3(_model.ScaleX, _model.ScaleY, _model.ScaleZ);
        }

        public void EnableOutline(){
            _outline.enabled = true;
        }

        public void DisableOutline(){
            _outline.enabled = false;
        }

    }
}
