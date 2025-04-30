using UnityEngine;
using Virtualware.Networking.Client;

namespace VirooLab
{
    [RequireComponent(typeof(NetworkNavMeshAgent))]
    public class NetworkNavMeshAgentAnimation : MonoBehaviour
    {
        [SerializeField]
        private Animator animator = default;

        [SerializeField]
        private string animatorVariable = "IsWalking";

        private NetworkNavMeshAgent agent;
        private NetworkObject networkObject;

        protected void Awake()
        {
            agent = GetComponent<NetworkNavMeshAgent>();
            networkObject = agent.GetComponent<NetworkObject>();
            animator.SetBool(animatorVariable, value: false);
        }

        protected void Update()
        {
            if (!networkObject.IsInitialized)
            {
                return;
            }

            animator.SetBool(animatorVariable, agent.IsRunning);
        }
    }
}
