using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace VirooLab
{
    public class XRTaggedSocketInteractor : XRSocketInteractor
    {
        [SerializeField]
        private List<string> acceptedTags = default;

        public override bool CanHover(IXRHoverInteractable interactable)
        {
            return base.CanHover(interactable) && IsValidInteractable(interactable as MonoBehaviour);
        }

        public override bool CanSelect(IXRSelectInteractable interactable)
        {
            return base.CanSelect(interactable) && IsValidInteractable(interactable as MonoBehaviour);
        }

        private bool IsValidInteractable(MonoBehaviour interactable)
        {
            XRTaggedGrabInteractable taggedGrabInteractable = interactable.GetComponent<XRTaggedGrabInteractable>();

            return taggedGrabInteractable != null && acceptedTags.Contains(taggedGrabInteractable.Tag, System.StringComparer.Ordinal);
        }
    }
}
