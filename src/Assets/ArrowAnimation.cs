using UnityEngine;
using DG.Tweening;

public class ArrowAnimation : MonoBehaviour
{
    /// <summary>
    /// The transform of the arrow to be animated.
    /// </summary>
    public Transform arrowTransform;

    /// <summary>
    /// The distance the arrow will move up and down.
    /// </summary>
    public float moveDistance = 0.1f;

    /// <summary>
    /// The duration of the arrow animation.
    /// </summary>
    public float animationDuration = 1f;

    /// <summary>
    /// Reference to the Tween object for controlling the arrow animation.
    /// </summary>
    private Tween arrowTween;

    /// <summary>
    /// Previous moveDistance value to detect changes
    /// </summary>
    private float previousMoveDistance;

    /// <summary>
    /// Called when the object becomes active, starting the arrow animation.
    /// </summary>
    void OnEnable()
    {
        StartArrowAnimation();
    }

    /// <summary>
    /// Update method to detect moveDistance changes in runtime
    /// </summary>
    void Update()
    {
        // Check if moveDistance has changed
        if (Mathf.Abs(moveDistance - previousMoveDistance) > 0.001f)
        {
            RestartAnimation();
            previousMoveDistance = moveDistance;
        }
    }

    /// <summary>
    /// Starts the arrow animation
    /// </summary>
    private void StartArrowAnimation()
    {
        if (arrowTransform == null) return;

        // Store initial moveDistance
        previousMoveDistance = moveDistance;

        // Use DOLocalMoveY for relative movement (better with moving parents)
        Vector3 startPos = arrowTransform.localPosition;
        arrowTween = arrowTransform.DOLocalMoveY(startPos.y + moveDistance, animationDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    /// <summary>
    /// Restarts the arrow animation with new moveDistance value
    /// </summary>
    public void RestartAnimation()
    {
        StopAnimation();
        StartArrowAnimation();
    }

    /// <summary>
    /// Stops the current animation
    /// </summary>
    private void StopAnimation()
    {
        if (arrowTween != null)
        {
            arrowTween.Kill();
            arrowTween = null;
        }
    }

    /// <summary>
    /// Called when the object becomes inactive, stopping and clearing the animation.
    /// </summary>
    void OnDisable()
    {
        StopAnimation();
    }
}
