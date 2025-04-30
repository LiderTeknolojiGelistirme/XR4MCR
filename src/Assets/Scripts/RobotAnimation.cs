using UnityEngine;
using DG.Tweening;

public class RobotAnimation : MonoBehaviour
{
    public GameObject leaderRobot;
    public GameObject followerRobot;



    [Header("Leader Robot Arm Segments")]
    public Transform pivot01; // Base rotation
    public Transform pivot02; // Shoulder
    public Transform pivot03; // Elbow
    public Transform pivot04; // Wrist 1



    void Start()
    {
        StartRobotAnimation();
    }
   
    /// <summary>
    /// Initiates the continuous animation for all robot arm segments
    /// </summary>
    public void StartRobotAnimation()
    {

        leaderRobot.SetActive(true);
        followerRobot.SetActive(false);
        if (pivot01 != null)
        {
            // Base rotation around Y axis
            pivot01.DOLocalRotate(new Vector3(0, 25, 0), 2)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }

        if (pivot02 != null)
        {
            // Shoulder rotation around Z axis
            pivot02.DOLocalRotate(new Vector3(0, 0, -45), 2)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }
        
        if (pivot03 != null)
        {
            // Elbow rotation around Z axis
            pivot03.DOLocalRotate(new Vector3(0, 0, 70), 2)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }

        if (pivot04 != null)
        {
            // Wrist 1 rotation around Z axis
            pivot04.DOLocalRotate(new Vector3(0, 0, -200), 2)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }

        
    }

    // Disable RepositionRobot script
    public void StopRobotAnimation()
    {
        DOTween.KillAll();
        
        pivot01.DOLocalRotate(new Vector3(0, 0, 0), 2)
            .SetEase(Ease.InOutSine);
        pivot02.DOLocalRotate(new Vector3(0, 0, 0), 2)
            .SetEase(Ease.InOutSine);
        pivot03.DOLocalRotate(new Vector3(0, 0, 0), 2)
            .SetEase(Ease.InOutSine);

        leaderRobot.SetActive(false);
        followerRobot.SetActive(true);
    }   

    
}