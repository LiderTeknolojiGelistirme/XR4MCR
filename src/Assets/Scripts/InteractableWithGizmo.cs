using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableWithGizmo : MonoBehaviour
{
    public bool interactableWithGizmo = true;

    public void EnableInteract()
    {
        interactableWithGizmo=true;
    }

    public void DisableInteract()
    {
        interactableWithGizmo = false;
    }
}
