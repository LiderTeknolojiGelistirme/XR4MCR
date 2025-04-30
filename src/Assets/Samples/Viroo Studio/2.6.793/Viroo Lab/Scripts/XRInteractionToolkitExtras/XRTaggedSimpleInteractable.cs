using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace VirooLab
{
    public class XRTaggedSimpleInteractable : XRSimpleInteractable
    {
        [SerializeField]
        private List<string> acceptedTags = default;

        public IEnumerable<string> AcceptedTags => acceptedTags;
    }
}
