using System;
using UnityEngine;
using Virtualware.Networking.Client;

namespace VirooLab
{
    public class CubeDestroyPosition : MonoBehaviour
    {
        [SerializeField]
        private ParticleSystem fire = default;

        private INetworkObjectsService networkObjectsService;

        protected void Inject(INetworkObjectsService networkObjectsService)
        {
            this.networkObjectsService = networkObjectsService;
        }

        protected void Awake()
        {
            this.QueueForInject();
        }

        protected async void OnTriggerEnter(Collider other)
        {
            VirooTag virooTag = other.GetComponent<VirooTag>();

            if (virooTag && virooTag.Tag.Equals("RobotArmGrabbableCube", StringComparison.Ordinal))
            {
                fire.Play();

                if (other.GetComponent<NetworkObject>().Authority)
                {
                    NetworkObject networkObject = other.GetComponent<NetworkObject>();
                    await networkObjectsService.DestroyObject(networkObject);
                }
            }
        }
    }
}
