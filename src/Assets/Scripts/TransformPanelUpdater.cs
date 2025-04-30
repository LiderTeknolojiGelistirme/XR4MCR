using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Managers;
using Zenject;
using System;
using UnityEditor;

public class TransformPanelUpdater : MonoBehaviour
{
    public static TransformPanelUpdater Instance;

    [Header("Target Object")]
    public SelectedObjectReference selectedObjectReference;
    public SpriteReference spriteReference;
    private GameObject go;

    [Header("UI - General Info")]
    public TMP_Text selectedObjectName;
    public Image selectedObjectSprite;

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

        // Event'e abone ol
        selectedObjectReference.OnSelectedObjectChanged += HandleSelectedObjectChanged;

    }

    private void HandleSelectedObjectChanged(GameObject gameObjectRef)
    {
        go = gameObjectRef;
    }

    private void Update()
    {
        if (go == null) return;

        selectedObjectName.text = go.name;

        // Şimdilik sprite gösterimi yok → boş bırak
        selectedObjectSprite.sprite = spriteReference.GetSpriteForObject(go);

        Vector3 pos = go.transform.position;
        posX.text = pos.x.ToString("F2");
        posY.text = pos.y.ToString("F2");
        posZ.text = pos.z.ToString("F2");

        Vector3 rot = go.transform.eulerAngles;
        rotX.text = rot.x.ToString("F2");
        rotY.text = rot.y.ToString("F2");
        rotZ.text = rot.z.ToString("F2");

        Vector3 scl = go.transform.localScale;
        scaleX.text = scl.x.ToString("F2");
        scaleY.text = scl.y.ToString("F2");
        scaleZ.text = scl.z.ToString("F2");
    }


}
