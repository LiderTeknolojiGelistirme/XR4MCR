using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Virtualware.Networking;
using Virtualware.Networking.Client;
using Virtualware.Networking.Client.Variables;

namespace VirooLab
{
    public class NetworkNavMeshAgentDestinations : MonoBehaviour
    {
        [SerializeField]
        private NetworkNavMeshAgent agent = default;

        [SerializeField]
        private List<Transform> destinations = default;

        private NetworkVariable<int> index;

        private IScenePersistenceAwareElementRegistry persistenceAwareElementRegistry;

        protected async void Inject(
            IScenePersistenceAwareElementRegistry persistenceAwareElementRegistry,
            NetworkVariableSynchronizer variableSynchronizer)
        {
            this.persistenceAwareElementRegistry = persistenceAwareElementRegistry;

            index = new NetworkVariable<int>(variableSynchronizer, agent.NetworkObjectId + "_AgentDestinationIndex", defaultValue: 0);
            index.OnInitialized += OnIndexChanged;
            index.OnValueChanged += OnIndexChanged;
            persistenceAwareElementRegistry.Register(index);

            await UniTask.WaitUntil(() => !agent.ReceivingInitState, cancellationToken: destroyCancellationToken);

            agent.OnDestinationReached += OnDestinationReached;

            NetworkAuthorityEnsurer ensurer = agent.GetComponent<NetworkAuthorityEnsurer>();
            ensurer.OnInitialized += OnEnsurerInitialized;

            agent.NetworkObject.OnOwnershipDataChanged += OnOwnershipDataChanged;
        }

        protected void Awake()
        {
            this.QueueForInject();
        }

        protected void OnDestroy()
        {
            index.OnInitialized -= OnIndexChanged;
            index.OnValueChanged -= OnIndexChanged;
            persistenceAwareElementRegistry.Unregister(index);
            agent.NetworkObject.OnOwnershipDataChanged -= OnOwnershipDataChanged;
        }

        private void OnEnsurerInitialized(object sender, EventArgs args)
        {
            if (sender is not NetworkAuthorityEnsurer ensurer)
            {
                return;
            }

            ensurer.OnInitialized -= OnEnsurerInitialized;

            GoToDestination(index.Value);
        }

        private void OnOwnershipDataChanged(object sender, EventArgs args) => GoToDestination(index.Value);

        private void OnIndexChanged(object sender, int value) => GoToDestination(index);

        private void SetNextDestination()
        {
            if (!agent.NetworkObject.Authority)
            {
                return;
            }

            index.Value = (index.Value + 1) % destinations.Count;
        }

        private void GoToDestination(int index)
        {
            if (!agent.NetworkObject.Authority)
            {
                return;
            }

            agent.SetDestination(destinations[index].position);
        }

        private void OnDestinationReached(object sender, EventArgs e) => SetNextDestination();
    }
}
