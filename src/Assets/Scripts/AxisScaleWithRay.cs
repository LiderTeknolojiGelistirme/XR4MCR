using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class AxisScaleWithRay : MonoBehaviour
{
    public enum Axis { X, Y, Z }
    public Axis scaleAxis = Axis.Y;

    [Range(0.1f, 10f)]
    public float scaleSensitivity = 1f;

    private XRBaseInteractor interactor;
    private bool isScaling = false;
    private Vector3 lastInteractorPos;
    private Vector3 initialScale;

    public void StartScale(BaseInteractionEventArgs args)
    {
        interactor = args.interactorObject as XRBaseInteractor;
        if (interactor != null)
        {
            lastInteractorPos = interactor.transform.position;
            initialScale = transform.parent.localScale;
            isScaling = true;
        }
    }

    public void StopScale(BaseInteractionEventArgs args)
    {
        isScaling = false;
        interactor = null;
    }

    void Update()
    {
        if (!isScaling || interactor == null || transform.parent == null)
            return;

        Vector3 currentPos = interactor.transform.position;
        Vector3 delta = currentPos - lastInteractorPos;

        Vector3 axisVector = Vector3.zero;
        switch (scaleAxis)
        {
            case Axis.X: axisVector = Vector3.right; break;
            case Axis.Y: axisVector = Vector3.up; break;
            case Axis.Z: axisVector = Vector3.forward; break;
        }

        // Hesaplama: Ray yönü + interactor hareketi
        float scaleDelta = Vector3.Dot(delta, axisVector) * scaleSensitivity;

        Vector3 newScale = transform.parent.localScale;

        // Sadece ilgili eksende ölçek deðiþtir
        newScale += axisVector * scaleDelta;

        // Minimum ölçek kontrolü
        newScale.x = Mathf.Max(0.01f, newScale.x);
        newScale.y = Mathf.Max(0.01f, newScale.y);
        newScale.z = Mathf.Max(0.01f, newScale.z);

        transform.parent.transform.parent.localScale = newScale;

        lastInteractorPos = currentPos;
    }

    void OnDisable()
    {
        isScaling = false;
        interactor = null;
    }
}
