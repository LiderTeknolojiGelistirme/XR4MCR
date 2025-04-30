using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class AxisRotateWithRay : MonoBehaviour
{
    public enum Axis { X, Y, Z }
    public Axis rotateAxis = Axis.Y;

    private XRBaseInteractor interactor;
    private bool isRotating = false;
    private Vector3 lastInteractorPos;

    public void StartRotation(BaseInteractionEventArgs args)
    {
        interactor = args.interactorObject as XRBaseInteractor;
        if (interactor != null)
        {
            lastInteractorPos = interactor.transform.position;
            isRotating = true;
        }
    }

    public void StopRotation(BaseInteractionEventArgs args)
    {
        interactor = null;
        isRotating = false;
    }

    void Update()
    {
        if (isRotating && interactor != null && transform.parent != null)
        {
            Vector3 currentPos = interactor.transform.position;
            Vector3 movement = currentPos - lastInteractorPos;

            Vector3 axis = Vector3.zero;
            switch (rotateAxis)
            {
                case Axis.X: axis = Vector3.right; break;
                case Axis.Y: axis = Vector3.up; break;
                case Axis.Z: axis = Vector3.forward; break;
            }

            // Hareketin o eksene g�re ne kadar d�nd�rmesi gerekti�ini bul
            float rotationAmount = Vector3.Dot(movement, Vector3.Cross(axis, interactor.transform.forward)) * 300f;

            transform.parent.transform.parent.Rotate(axis, rotationAmount, Space.World);
            transform.parent.rotation = transform.parent.transform.parent.rotation;

            lastInteractorPos = currentPos;
        }
    }
}
