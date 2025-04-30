using System.Collections.Generic;
using UnityEngine;
using Virtualware.Networking;
using Virtualware.Networking.Client;
using Virtualware.Networking.Client.Variables;

namespace VirooLab
{
    public class TargetsController : MonoBehaviour
    {
        [SerializeField]
        private List<Target> targets = default;

        [SerializeField]
        private Score score = default;

        private NetworkVariable<bool> isPlaying;
        private NetworkVariable<int> currentTargetIndex;
        private NetworkObject networkObject;

        private IScenePersistenceAwareElementRegistry elementRegistry;

        private bool HasAuthority => networkObject.Authority;

        protected void Inject(IScenePersistenceAwareElementRegistry elementRegistry, NetworkVariableSynchronizer variableSynchronizer)
        {
            this.elementRegistry = elementRegistry;

            networkObject = GetComponent<NetworkObject>();

            isPlaying = new NetworkVariable<bool>(variableSynchronizer, "TargetsControllerPlaying", defaultValue: false);

            currentTargetIndex = new NetworkVariable<int>(variableSynchronizer, "TargetsControllerIndex", defaultValue: -1);
            currentTargetIndex.OnInitialized += OnCurrentIndexChanged;
            currentTargetIndex.OnValueChanged += OnCurrentIndexChanged;

            elementRegistry.Register(isPlaying);
            elementRegistry.Register(currentTargetIndex);
        }

        protected void Awake()
        {
            this.QueueForInject();
        }

        protected void OnDestroy()
        {
            currentTargetIndex.OnInitialized -= OnCurrentIndexChanged;
            currentTargetIndex.OnValueChanged -= OnCurrentIndexChanged;

            elementRegistry.Unregister(isPlaying);
            elementRegistry.Unregister(currentTargetIndex);
        }

        private void OnHit(object sender, System.EventArgs e)
        {
            score.Increment();

            Target target = (sender as Target)!;
            target.OnHit -= OnHit;
            target.AnimateHide(SetTarget);
        }

        public void Play()
        {
            if (isPlaying.Value)
            {
                return;
            }

            isPlaying.Value = true;

            SetTarget();
        }

        private void SetTarget()
        {
            if (!HasAuthority)
            {
                return;
            }

            int randomIndex;
            do
            {
                randomIndex = Random.Range(0, targets.Count);
            }
            while (randomIndex == currentTargetIndex.Value);

            currentTargetIndex.Value = randomIndex;
        }

        private void OnCurrentIndexChanged(object sender, int value)
        {
            if (value < 0)
            {
                return;
            }

            Target target = targets[currentTargetIndex.Value];

            target.AnimateShow();
            target.OnHit += OnHit;
        }
    }
}
