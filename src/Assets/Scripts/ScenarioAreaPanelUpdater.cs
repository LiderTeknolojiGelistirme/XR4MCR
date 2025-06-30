using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class ScenarioAreaPanelUpdater : MonoBehaviour
{
    public static ScenarioAreaPanelUpdater Instance;

    [Header("Target Object")]
    public GameObject targetObject;

    [Header("UI - Position Fields")]
    public TMP_InputField posX;
    public TMP_InputField posY;
    public TMP_InputField posZ;

    [Header("UI - Rotation Fields")]
    public TMP_InputField rotX;
    public TMP_InputField rotY;
    public TMP_InputField rotZ;

    [Header("UI - Scale Fields")]
    public TMP_InputField scaleX;
    public TMP_InputField scaleY;
    public TMP_InputField scaleZ;

    void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (targetObject == null) return;

        Vector3 pos = targetObject.transform.position;
        posX.text = pos.x.ToString("F4");
        posY.text = pos.y.ToString("F4");
        posZ.text = pos.z.ToString("F4");

        Vector3 rot = targetObject.transform.eulerAngles;
        rotX.text = rot.x.ToString("F4");
        rotY.text = rot.y.ToString("F4");
        rotZ.text = rot.z.ToString("F4");

        Vector3 scl = targetObject.transform.localScale;
        Debug.Log($"Scale Values - X: {scl.x}, Y: {scl.y}, Z: {scl.z}");
        Debug.Log($"Scale InputFields - X: {scaleX != null}, Y: {scaleY != null}, Z: {scaleZ != null}");
        
        if (scaleX != null && scaleY != null && scaleZ != null)
        {
            scaleX.text = scl.x.ToString("F4");
            scaleY.text = scl.y.ToString("F4");
            scaleZ.text = scl.z.ToString("F4");
        }
        else
        {
            Debug.LogWarning("Scale InputFields are not assigned!");
        }
    }
} 