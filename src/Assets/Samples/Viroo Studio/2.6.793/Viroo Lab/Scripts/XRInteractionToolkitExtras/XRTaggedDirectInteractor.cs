using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace VirooLab
{
    public class XRTaggedDirectInteractor : XRBaseInteractor
    {
        [SerializeField]
        private new string tag = default;

        readonly Dictionary<Collider, IXRInteractable> enteredColliders = new();

        protected void OnTriggerEnter(Collider other)
        {
            if (interactionManager.TryGetInteractableForCollider(other, out IXRInteractable interactable)
                && interactable is XRTaggedSimpleInteractable taggedInteractable
                && taggedInteractable.AcceptedTags.Contains(tag, System.StringComparer.Ordinal))
            {
                enteredColliders.Add(other, interactable);
            }
        }

        protected void OnTriggerExit(Collider other)
        {
            if (enteredColliders.ContainsKey(other))
            {
                enteredColliders.Remove(other);
            }
        }

        public override void GetValidTargets(List<IXRInteractable> targets)
        {
            targets.Clear();

            targets.AddRange(enteredColliders.Values);

            base.GetValidTargets(targets);
        }
    }
}
