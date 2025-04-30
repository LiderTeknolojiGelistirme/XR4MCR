using TMPro;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using Viroo.Input.XR;
using Viroo.Input;
using System.Globalization;
using System.Collections;
using UnityEngine.Serialization;

public class CanvasControllerXR : MonoBehaviour
{
    public GameObject CanvasParent;
    public NotifierCanvas notifierCanvas;
    public float locateCanvasDelay = 1f;
    
    private float _timer = 0f;
    private float _fillAmount = 0f;
    int i;

    private GameObject SelectedCanvas;
    //selectedobject se�ilemiyor

    /// <summary>
    /// The scene camera used for positioning the main panel.
    /// </summary>
    public Camera sceneCamera;

    /// <summary>
    /// The target position for the main panel.
    /// </summary>
    private Vector3 targetPosition;

    /// <summary>
    /// The target rotation for the main panel.
    /// </summary>
    private Quaternion targetRotation;

    /// <summary>
    /// The step value used for smooth animation.
    /// </summary>
    private float step;

    public bool held;

    /// <summary>
    /// Sets the initial position of the main panel in front of the user.
    /// </summary>
    /// 

    IEnumerator Start()
    {

        held = false;

        sceneCamera = Camera.main;
        SelectedCanvas = CanvasParent;
        notifierCanvas = sceneCamera.transform.GetComponentInChildren<NotifierCanvas>();
        // Set initial main panel position in front of the user
        //SelectedCanvas.transform.position = sceneCamera.transform.position + sceneCamera.transform.forward * 3.0f + Vector3.up * 1.5f;
        //SelectedCanvas.transform.rotation = sceneCamera.transform.rotation;

        // Define step value for smooth movement
        step = 500.0f * Time.deltaTime;

        

        yield return null;

        
    }

    
    private void Update()
    {


        //Debug.Log(SelectedCanvas.name);
        if (held)
        {
            _timer += Time.deltaTime; // Increment the timer based on time looked at the object
            _fillAmount = _timer / locateCanvasDelay; // Calculate the progress fill amount
            notifierCanvas.locateCanvasImage.fillAmount = _fillAmount; // Update the UI with the progress

            // If the player has looked long enough, complete the procedure
            if (_fillAmount >= 1f)
            {
                _fillAmount = 1f;
                centerPanel();
            }
        }
        else
        {
            _timer = 0f;
            notifierCanvas.locateCanvasImage.fillAmount = 0f;
        }
        
        
    }

    public void UpdateSelectedCanvas(GameObject go)
    {
        SelectedCanvas = go;
    }

    /// <summary>
    /// Places the main panel smoothly at the center of the user's viewport and rotates it to face the camera.
    /// </summary>
    public void centerPanel()
    {
       
        
        // Position the canvas 1 meter in front of the camera
        targetPosition = sceneCamera.transform.position + sceneCamera.transform.forward * 3.0f;

        // Make canvas face the same direction as the camera
        targetRotation = sceneCamera.transform.rotation;

    
        // Smoothly interpolate position and rotation
        SelectedCanvas.transform.position = Vector3.Lerp(SelectedCanvas.transform.position, targetPosition, step);
        SelectedCanvas.transform.rotation = Quaternion.Slerp(SelectedCanvas.transform.rotation, targetRotation, step);


        
    }

    public void HoldCanvas()
    {
        held = true;
    }

    public void ReleaseCanvas()
    {
        held = false;
    }

}
