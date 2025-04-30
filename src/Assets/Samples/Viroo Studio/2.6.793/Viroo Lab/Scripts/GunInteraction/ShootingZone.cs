using System.Collections.Generic;
using UnityEngine;
using Virtualware.Networking.Client;

namespace VirooLab
{
    public class ShootingZone : MonoBehaviour
    {
        [SerializeField]
        private TargetsController targetsController = default;

        private readonly List<VirooTag> tagsInside = new();

        protected void OnTriggerEnter(Collider other)
        {
            VirooTag virooTag = GetVirooTag(other);
            bool isGun = IsGunTriggering(virooTag);

            if (isGun && !tagsInside.Contains(virooTag))
            {
                tagsInside.Add(virooTag);

                NetworkObject network = other.GetComponentInParent<NetworkObject>();
                if (network.Authority)
                {
                    targetsController.Play();
                }
            }
        }

        protected void OnTriggerExit(Collider other)
        {
            VirooTag virooTag = GetVirooTag(other);
            bool isGun = IsGunTriggering(virooTag);

            if (isGun)
            {
                tagsInside.Remove(virooTag);
            }
        }

        private static VirooTag GetVirooTag(Collider collider)
            => collider.GetComponentInParent<VirooTag>();

        private static bool IsGunTriggering(VirooTag virooTag)
            => virooTag && virooTag.Tag.Equals("Gun", System.StringComparison.OrdinalIgnoreCase);
    }
}
