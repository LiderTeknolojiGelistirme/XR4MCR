using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace VirooLab
{
    public class XRTaggedGrabInteractable : XRGrabInteractable
    {
        [SerializeField]
        private new string tag = default;

        public string Tag => tag;
    }
}
