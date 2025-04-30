using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class AxisMoveWithRay : MonoBehaviour
{
    public enum Axis { X, Y, Z }
    public Axis dragAxis = Axis.X;

    private XRBaseInteractor interactor;
    private bool isDragging = false;

    public void StartDrag(BaseInteractionEventArgs args)
    {
        interactor = args.interactorObject as XRBaseInteractor;
        isDragging = true;
    }

    public void StopDrag(BaseInteractionEventArgs args)
    {
        interactor = null;
        isDragging = false;
    }

    void Update()
    {
        if (isDragging && interactor != null && transform.parent != null)
        {
            Vector3 rayOrigin = interactor.transform.position;
            Vector3 rayDir = interactor.transform.forward;
            Vector3 targetPoint = rayOrigin + rayDir * 2f;

            // Hareket ekseni (local eksende)
            Vector3 axisDirection = Vector3.zero;
            switch (dragAxis)
            {
                case Axis.X: axisDirection = transform.parent.transform.parent.right; break;
                case Axis.Y: axisDirection = transform.parent.transform.parent.up; break;
                case Axis.Z: axisDirection = transform.parent.transform.parent.forward; break;
            }

            // Objeyi local ekseni doğrultusunda sürükle
            Vector3 parentPosition = transform.parent.transform.parent.position;
            Vector3 toTarget = targetPoint - parentPosition;

            // Sadece seçilen eksendeki bileşeni al
            float distanceOnAxis = Vector3.Dot(toTarget, axisDirection);
            Vector3 newPos = parentPosition + axisDirection * distanceOnAxis;

            transform.parent.transform.parent.position = newPos;
        }
    }
}
