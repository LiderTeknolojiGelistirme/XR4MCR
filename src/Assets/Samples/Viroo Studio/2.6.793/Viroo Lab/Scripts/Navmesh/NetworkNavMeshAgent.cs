using System;
using System.Collections.Generic;
using Networking.Messages.Senders;
using UnityEngine;
using UnityEngine.AI;
using Viroo.Networking;
using Virtualware.Networking;
using Virtualware.Networking.Client;
using Virtualware.Networking.Client.Components;
using Virtualware.Networking.Client.Variables;

namespace VirooLab
{
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(NetworkAuthorityEnsurer))]
    public class NetworkNavMeshAgent : NetworkBehaviour
    {
        public event EventHandler<EventArgs> OnDestinationReached;

        private IForwardMessageSender forwardMessageSender;
        private IScenePersistenceAwareElementRegistry persistenceAwareElementRegistry;
        private IPlayerProvider playerProvider;

        private NavMeshAgent agent;

        private NetworkVariable<Vector3> destination;
        private NetworkVariable<bool> isRunning;

        private string networkObjectId;

        public string NetworkObjectId
        {
            get
            {
                if (string.IsNullOrEmpty(networkObjectId))
                {
                    networkObjectId = GetComponent<NetworkObject>().ObjectId;
                }

                return networkObjectId;
            }
        }

        public bool IsRunning => isRunning?.Value == true;

        private bool arrivalNotified = false;

        protected void Inject(
            IForwardMessageSender forwardMessageSender,
            IScenePersistenceAwareElementRegistry persistenceAwareElementRegistry,
            IPlayerProvider playerProvider,
            NetworkVariableSynchronizer variableSynchronizer)
        {
            this.forwardMessageSender = forwardMessageSender;
            this.persistenceAwareElementRegistry = persistenceAwareElementRegistry;
            this.playerProvider = playerProvider;

            agent = GetComponent<NavMeshAgent>();

            destination = new(variableSynchronizer, NetworkObjectId + "_AgentDestination", defaultValue: Vector3.zero);
            isRunning = new(variableSynchronizer, NetworkObjectId + "_AgentIsRunning", defaultValue: false);

            persistenceAwareElementRegistry.Register(destination);
            persistenceAwareElementRegistry.Register(isRunning);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            persistenceAwareElementRegistry.Unregister(destination);
            persistenceAwareElementRegistry.Unregister(isRunning);
            playerProvider.OnPlayerRegistered -= OnPlayerRegistered;
        }

        protected override void OnBehaviourInitializationCompleted()
        {
            base.OnBehaviourInitializationCompleted();
            playerProvider.OnPlayerRegistered += OnPlayerRegistered;
        }

        private void OnPlayerRegistered(object sender, PlayerRegisteredEventArgs args)
        {
            if (!HasAuthority)
            {
                return;
            }

            SetTransformMessage message = new(IntId, transform.position, transform.rotation, transform.localScale);
            _ = forwardMessageSender.Forward(message, new List<string> { args.Player.ClientId });
        }

        protected override void RegisterMessageCallbacks()
        {
            base.RegisterMessageCallbacks();

            RegisterMessageCallback<SetTransformMessage>(OnInitialPositionReceived);
        }

        protected override void UnregisterMessageCallbacks()
        {
            base.UnregisterMessageCallbacks();

            UnregisterMessageCallback<SetTransformMessage>(OnInitialPositionReceived);
        }

        private void OnInitialPositionReceived(SetTransformMessage message)
        {
            agent.Warp(message.Position);
        }

        public void SetDestination(Vector3 destination)
        {
            arrivalNotified = false;

            if (!HasAuthority)
            {
                return;
            }

            this.destination.Value = destination;
            isRunning.Value = true;
        }

        public void Stop()
        {
            if (!HasAuthority)
            {
                return;
            }

            isRunning.Value = false;
        }

        protected void Update()
        {
#pragma warning disable RCS1146 // Use conditional access
            if (NetworkObject == null || !NetworkObject.IsInitialized)
#pragma warning restore RCS1146 // Use conditional access
            {
                return;
            }

            if (isRunning?.Value == true)
            {
                agent.SetDestination(destination);

                CheckArrivedToDestination();
            }
        }

        private void CheckArrivedToDestination()
        {
            if (!arrivalNotified && agent.hasPath && agent.remainingDistance <= agent.stoppingDistance + 0.2f)
            {
                arrivalNotified = true;

                agent.ResetPath();

                OnDestinationReached?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
