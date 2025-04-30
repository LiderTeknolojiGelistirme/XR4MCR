using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Test : MonoBehaviour
{
    public XRSimpleInteractable simpleInteractable;
    public Transform targetTransform;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        if (Vector3.Distance(simpleInteractable.transform.position, targetTransform.position) < 0.1f)
        {
            Debug.Log("Grab and drop complete");
            
        }
    }
}
