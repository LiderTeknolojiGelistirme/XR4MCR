using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Virtualware.Networking.Client;

namespace VirooLab
{
    public class Bullet : MonoBehaviour
    {
        private const float DestroyTime = 10;

        private INetworkObjectsService networkObjectsService;

        private bool injectionDone;

        protected void Inject(INetworkObjectsService networkObjectsService)
        {
            this.networkObjectsService = networkObjectsService;

            Rigidbody rigid = GetComponent<Rigidbody>();
            rigid.AddForce(transform.forward * 0.25f, ForceMode.Impulse);

            injectionDone = true;
        }

#pragma warning disable S3168 // "async" methods should not return "void"
        protected async void Awake()
#pragma warning restore S3168 // "async" methods should not return "void"
        {
            this.QueueForInject();

            IgnoreCollisionWithTheGun();

            try
            {
                CancellationToken cancellationToken = this.GetCancellationTokenOnDestroy();

                await UniTask.WaitUntil(() => injectionDone, cancellationToken: cancellationToken);

                await UniTask.Delay((int)(DestroyTime * 1000), cancellationToken: cancellationToken);

                if (TryGetComponent(out NetworkObject networkObject))
                {
                    await networkObjectsService.DestroyObject(networkObject);
                }
            }
            catch (OperationCanceledException)
            {
                // Scene changed, ignore exception
            }
        }

        private void IgnoreCollisionWithTheGun()
        {
            Collider bulletCollider = GetComponent<Collider>();

            foreach (Collider col in FindObjectsOfType<VirooTag>().Where(
                virooTag => virooTag.Tag.Equals("Gun", StringComparison.OrdinalIgnoreCase))
                .SelectMany(virooTag => virooTag.GetComponentsInChildren<Collider>()))
            {
                Physics.IgnoreCollision(bulletCollider, col, ignore: true);
            }
        }
    }
}
