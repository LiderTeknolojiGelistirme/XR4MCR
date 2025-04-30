using System.Linq;
using UnityEngine;

namespace VirooLab
{
    public class IgnoreCollision : MonoBehaviour
    {
        [SerializeField]
        private Collider target = default;

        [SerializeField]
        private Collider[] acceptedColliders = default;

        protected void OnTriggerEnter(Collider other)
        {
            if (!acceptedColliders.Contains(other))
            {
                Physics.IgnoreCollision(target, other, ignore: true);
            }
        }
    }
}
