using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using Virtualware.Networking.Client;

namespace VirooLab
{
    public class XRGrabInteractableCreator : MonoBehaviour, IRecipient<NetworkObjectInstantiatedData>
    {
        [SerializeField]
        private string objectId = default;

        [SerializeField]
        private XRGrabInteractable grabInteractable = default;

        private INetworkObjectsService networkObjectsService;

        private IMessenger messenger;

        protected void Inject(INetworkObjectsService networkObjectsService, IMessenger messenger)
        {
            this.networkObjectsService = networkObjectsService;
            this.messenger = messenger;

            messenger.RegisterAll(this);
        }

        protected void Awake()
        {
            this.QueueForInject();
        }

        protected void Start()
        {
            grabInteractable.selectEntered.AddListener(OnSelectEntered);
        }

        protected void OnDestroy()
        {
            messenger.UnregisterAll(this);
        }

        public void Receive(NetworkObjectInstantiatedData message)
        {
            if (!message.ResourceIdentifier.Equals(objectId, StringComparison.Ordinal))
            {
                return;
            }

            XRBaseInteractable interactable = message.GameObject.GetComponent<XRBaseInteractable>();
            interactable.selectEntered.AddListener(OnSelectEntered);
        }

        private async void OnSelectEntered(SelectEnterEventArgs selectEnteredEventArgs)
        {
            XRBaseInteractable interactable = selectEnteredEventArgs.interactableObject as XRBaseInteractable;
            interactable.selectEntered.RemoveListener(OnSelectEntered);

            NetworkObject networkObject = interactable.gameObject.GetComponent<NetworkObject>();

            if (networkObject.Authority)
            {
                await CreateInteractable();
            }
        }

        private async Task CreateInteractable()
        {
            await networkObjectsService.CreateDynamicObject(
                    objectId,
                    transform.position,
                    transform.rotation,
                    requestAuthority: true,
                    isPersistent: true,
                    SceneManager.GetActiveScene().name);
        }
    }
}
