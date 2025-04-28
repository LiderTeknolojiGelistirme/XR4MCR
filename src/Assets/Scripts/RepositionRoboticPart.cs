using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepositionRoboticPart : MonoBehaviour
{
    [SerializeField] private Transform trackedPoint; // Reference to the tracked point

    // Start is called before the first frame update
    void Start()
    {
        if (trackedPoint == null)
        {
            Debug.LogError("Tracked point reference is missing!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (trackedPoint != null)
        {
            // Match position and rotation of the tracked point
            transform.position = trackedPoint.position;
            transform.rotation = trackedPoint.rotation;
        }
    }
}
