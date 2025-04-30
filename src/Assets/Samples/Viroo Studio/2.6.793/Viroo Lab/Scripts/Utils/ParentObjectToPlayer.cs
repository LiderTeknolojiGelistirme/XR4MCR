using Microsoft.Extensions.Logging;
using Networking.Messages;
using UnityEngine;
using UnityEngine.Events;
using Viroo.Context;
using Virtualware.Networking.Client;
using Virtualware.Networking.Client.Components;
using Virtualware.Networking.Client.SceneManagement;

namespace VirooLab.Actions
{
    public class ParentObjectToPlayer : MonoBehaviour
    {
        private enum TransformTypes
        {
            Player = 0,
            Camera = 1,
            LeftHand = 2,
            RightHand = 3,
        }

        [SerializeField]
        private Transform target = default;

        [SerializeField]
        private TransformTypes attachTo = default;

        [SerializeField]
        private Vector3 positionOffset = Vector3.zero;

        [SerializeField]
        private Vector3 rotationOffset = Vector3.zero;

        [SerializeField]
        private bool parentObjectOnStart = false;

        [SerializeField]
        private UnityEvent onParented = default;

        [SerializeField]
        private UnityEvent onUnParented = default;

        private Transform defaultParent;

        private IContextProvider contextProvider;
        private INetworkObjectsService networkObjectsService;
        private ILogger<ParentObjectToPlayer> logger;

        protected void Inject(IContextProvider contextProvider, INetworkScenesService networkScenesService,
            INetworkObjectsService networkObjectsService, ILogger<ParentObjectToPlayer> logger)
        {
            this.contextProvider = contextProvider;
            this.networkObjectsService = networkObjectsService;
            this.logger = logger;

            if (target == null)
            {
                target = transform;
            }

            defaultParent = target.parent;

            if (parentObjectOnStart)
            {
                SetParent();
            }

            networkScenesService.OnLocalClientSceneUnloadStarted += OnLocalSceneUnloadStarted;
        }

        protected void Awake()
        {
            this.QueueForInject();
        }

        private void OnLocalSceneUnloadStarted(object sender, NetworkSceneEventArgs e)
        {
            (sender as INetworkScenesService)!.OnLocalClientSceneUnloadStarted -= OnLocalSceneUnloadStarted;

            SetUnparent();
        }

#pragma warning disable S3168 // "async" methods should not return "void"
        public async void SetParent()
#pragma warning restore S3168 // "async" methods should not return "void"
        {
            Transform attachTransform = GetAttachTransform(attachTo);

            bool isParentingAllowed = true;

            if (target.TryGetComponent(out NetworkTransform networkTransform))
            {
                NetworkObject networkObject = target.GetComponent<NetworkObject>();
                SessionObjectAuthorityResponse response = await networkObjectsService.RequestObjectAuthority(networkObject);

                isParentingAllowed = response.Success;

                if (!response.Success)
                {
                    logger.LogWarning(
                        "Unable to get Authority on {NetworkObject} - Error: {Error}",
                        networkObject.ObjectId,
                        (SessionObjectAuthorityResponse.ErrorTypes)response.ErrorCode);
                }
            }

            if (isParentingAllowed)
            {
                target.SetParent(attachTransform);

                target.SetLocalPositionAndRotation(positionOffset, Quaternion.Euler(rotationOffset));

                if (networkTransform)
                {
                    await networkTransform.SetTransform(target.position, target.rotation, target.localScale, destroyCancellationToken);
                }
            }

            onParented?.Invoke();
        }

        public void SetUnparent()
        {
            if (target)
            {
                target.SetParent(defaultParent);

                onUnParented?.Invoke();
            }
        }

        private Transform GetAttachTransform(TransformTypes transformType)
        {
            return transformType switch
            {
                TransformTypes.Player => contextProvider.Player.transform,
                TransformTypes.Camera => contextProvider.VirooCamera.transform,
                TransformTypes.LeftHand => contextProvider.LeftHandAttachPoint.transform,
                TransformTypes.RightHand => contextProvider.RightHandAttachPoint.transform,
                _ => contextProvider.Player.transform,
            };
        }
    }
}
