using System;
using System.Collections;
using System.Collections.Generic;
using Managers;
using UnityEngine;
using Zenject;
using Managers;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class TrackHitPointHelper : MonoBehaviour
{
    [HideInInspector] public XRRayInteractor xrRayInteractor;
    
    void Start()
    {
        xrRayInteractor = FindObjectOfType<XRRayInteractor>();
    }
    private void Update()
    {
        
        if (xrRayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
            gameObject.transform.position = hit.point;
    }
}
