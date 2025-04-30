using UnityEngine;

namespace Helpers
{
    public class CanvasScaleHelper : MonoBehaviour
    {
        public Transform canvasTransform;

        public void OnScaleUp()
        {
            canvasTransform.localScale *= 1.1f;
        }

        public void OnScaleDown()
        {
            canvasTransform.localScale /= 1.1f;
        }
    }
}