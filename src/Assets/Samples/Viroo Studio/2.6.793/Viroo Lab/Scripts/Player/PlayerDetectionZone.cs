using UnityEngine;
using UnityEngine.Events;
using Viroo.Networking;

namespace VirooLab
{
    public class PlayerDetectionZone : MonoBehaviour
    {
        [SerializeField]
        private UnityEvent enter = default;

        [SerializeField]
        private UnityEvent exit = default;

        protected void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<IPlayer>(out _))
            {
                enter?.Invoke();
            }
        }

        protected void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent<IPlayer>(out _))
            {
                exit?.Invoke();
            }
        }
    }
}
