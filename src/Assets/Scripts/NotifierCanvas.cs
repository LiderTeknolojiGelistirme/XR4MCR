using JetBrains.Annotations;
using Managers;
using System.Collections;
using UnityEngine;
using Zenject;
using Managers;
using Presenters.NodePresenters;
using UnityEngine.UI;

public class NotifierCanvas : MonoBehaviour
{
    

    Camera cam;
    GameObject go;
    public GameObject tick;
    public Image locateCanvasImage;

    private AudioSource audioSource;
    // Add this field to your class
    public AudioClip achievementSound;

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
            else
            {
                go = cam.gameObject;
                this.transform.SetParent(go.transform);
                this.transform.localPosition = new Vector3(0, 0, 1);

                // Get or add the AudioSource component to the GameObject
                audioSource = go.GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = go.AddComponent<AudioSource>();
                }

                break;
            }
        }
    }

    public void DescriptionPanelPreview()
    {
        ShowDescriptionPanel();

        StartCoroutine(HideDescriptionAfterDelay(3));
    }

   

    public void ShowDescriptionPanel()
    {
        if(descriptionPanel != null)
        {
            descriptionPanel.SetActive(true);
        }
    }

    public void HideDescriptionPanel()
    {
        if(descriptionPanel != null)
        {
            descriptionPanel.SetActive(false);

        }

    }

    IEnumerator HideDescriptionAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        HideDescriptionPanel();
    }


    // Then in your PlayAchievementSound method
    void PlayAchievementSound()
    {
        if (audioSource != null)
        {
            // Assign the clip and then play
            audioSource.clip = achievementSound;
            audioSource.Play();
        }
        else
        {
            Debug.LogError("AudioSource is not assigned or found!");
        }
    }

    public void ApplyToAchievementNotification()
    {

        PlayAchievementSound();
        ShowAchievementTick();

        StartCoroutine(HideTickAfterDelay(1));

    }

    void ShowAchievementTick()
    {
        if (tick != null)
        {
            tick.SetActive(true);
        }
        else
        {
            Debug.LogError("Tick object is not assigned!");
        }
    }

    void HideAchievementTick()
    {
        if (tick != null)
        {
            tick.SetActive(false);
        }
        else
        {
            Debug.LogError("Tick object is not assigned!");
        }
    }

    IEnumerator HideTickAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        HideAchievementTick();
    }

    
}
