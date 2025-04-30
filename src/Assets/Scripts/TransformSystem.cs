using DG.Tweening;
using Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class TransformSystem : MonoBehaviour
{
    public SelectedObjectReference selectedObjectReference;

    private void Awake()
    {

        // Event'e abone ol
        selectedObjectReference.OnSelectedObjectChanged += HandleSelectedObjectChanged;
    }

    private void HandleSelectedObjectChanged(GameObject gameObjectRef)
    {
        MakeChildOf(gameObjectRef);
        ResetTransform();

    }


    void MakeChildOf(GameObject gameObjectRef)
    {
        this.gameObject.transform.SetParent(gameObjectRef.transform);
    }


    void ResetTransform()
    {
        this.gameObject.transform.localPosition = Vector3.zero;
        this.gameObject.transform.localRotation = Quaternion.identity;
        this.gameObject.transform.localScale = Vector3.one;
    }
}
