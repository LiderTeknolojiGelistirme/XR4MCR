using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WorldNotifierCanvas : MonoBehaviour
{
    Camera cam;
    GameObject go;

    public GameObject descriptionPanel;


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
        }
    }

    private void Update()
    {
        if (cam != null)
        {
            transform.LookAt(cam.transform);
        }
    }


    public void ShowDescriptionPanel()
    {
        if (descriptionPanel != null)
        {
            descriptionPanel.SetActive(true);
        }
    }

    public void HideDescriptionPanel()
    {
        if (descriptionPanel != null)
        {
            descriptionPanel.SetActive(false);
        }
    }

    public IEnumerator HideDescriptionAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        HideDescriptionPanel();
    }

    public void HideDescriptionPanel(float delay)
    {
        StartCoroutine(HideDescriptionAfterDelay(delay));
    }
}