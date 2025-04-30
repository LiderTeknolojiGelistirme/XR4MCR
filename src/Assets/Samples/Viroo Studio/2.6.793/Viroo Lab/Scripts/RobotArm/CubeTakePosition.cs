using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace VirooLab
{
    public class CubeTakePosition : MonoBehaviour
    {
        [SerializeField]
        private RobotArmController robotArmController = default;

        protected void OnTriggerEnter(Collider other)
        {
            VirooTag virooTag = other.GetComponent<VirooTag>();

            if (virooTag && virooTag.Tag.Equals("RobotArmGrabbableCube", StringComparison.Ordinal))
            {
                XRGrabInteractable grabInteractable = other.GetComponent<XRGrabInteractable>();
                bool wasGrabbed = grabInteractable.isSelected;
                grabInteractable.enabled = false;

                if (wasGrabbed)
                {
                    grabInteractable.transform.SetPositionAndRotation(transform.position, Quaternion.identity);
                }

                robotArmController.AnimateRobot(other.gameObject);
            }
        }
    }
}
