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
    [HideInInspector] public XRInputManager inputManager;
    
    void Start()
    {
        inputManager = FindObjectOfType<XRInputManager>();
    }
    private void Update()
    {
        
        if (inputManager != null && inputManager.TryGetPrecisionRaycastHit(out RaycastHit hit))
            gameObject.transform.position = hit.point;
    }
}
