using RuntimeGizmos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class GizmoManager : MonoBehaviour
{
    Camera cam;
    GameObject go;

    // Unity Start Event Initialization
    IEnumerator Start()
    {
        // Keep looking for the camera until it's found
        while (cam == null)
        {
            cam = Camera.main;
            if (cam == null)
            {
                yield return new WaitForSeconds(0.1f);
            }
            else
            {
                
                go = cam.gameObject;

                break;
            }
        }
        
    }

    
    public void SetTransformTypeMove()
    {
        var transformGizmo = go.GetComponent<TransformGizmo>();
        if (transformGizmo != null)
        {
            transformGizmo.SetTransformTypeMove();
        }
    }

    public void SetTransformTypeRotate()
    {
        var transformGizmo = go.GetComponent<TransformGizmo>();
        if (transformGizmo != null)
        {
            transformGizmo.SetTransformTypeRotate();
        }
    }

    public void SetTransformTypeScale()
    {
        var transformGizmo = go.GetComponent<TransformGizmo>();
        if (transformGizmo != null)
        {
            transformGizmo.SetTransformTypeScale();
        }
    }
}
