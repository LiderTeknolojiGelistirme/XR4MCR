using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using Interfaces;

public class XRSelectableLtg : XRSimpleInteractable, ISelectable
{
    public bool EnableSelect { get; set; }

    public void Remove()
    {
        throw new System.NotImplementedException();
    }

    public void Select()
    {
        Debug.Log("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE " + this.gameObject.name);
    }

    public void Unselect()
    {
        throw new System.NotImplementedException();
    }


}