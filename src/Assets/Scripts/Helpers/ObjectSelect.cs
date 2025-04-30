using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Helpers
{
    public class ObjectSelect : MonoBehaviour
    {
        public List<GameObject> selectedObjects = new List<GameObject>();

        private XRRayInteractor xrRayInteractor;
        private bool interactorFound = false;

        void Update()
        {
            if (!interactorFound)
            {
                xrRayInteractor = FindObjectOfType<XRRayInteractor>();
                if (xrRayInteractor != null)
                {
                    interactorFound = true;
                    xrRayInteractor.selectEntered.AddListener(OnSelectEntered);
                    Debug.Log("XRRayInteractor bulundu ve event'e abone olundu.");
                }
            }
        }

        void OnDisable()
        {
        
            if (interactorFound && xrRayInteractor != null)
            {
                xrRayInteractor.selectEntered.RemoveListener(OnSelectEntered);
            }
        }
    
        void OnSelectEntered(SelectEnterEventArgs args)
        {
            GameObject selected = args.interactableObject.transform.gameObject;
            if (!selectedObjects.Contains(selected))
            {
                selectedObjects.Add(selected);
                Debug.Log("Listeye eklendi: " + selected.name);
            }
        }
    }
}