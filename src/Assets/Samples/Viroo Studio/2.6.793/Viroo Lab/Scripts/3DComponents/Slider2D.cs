using UnityEngine;

namespace VirooLab
{
    public class Slider2D : MonoBehaviour
    {
        [SerializeField]
        private float minValue = -1;

        [SerializeField]
        private float maxValue = 1;

        [SerializeField]
        private RectTransform handle = default;

        public void SetX(float x)
        {
            float xValue = Mathf.InverseLerp(minValue, maxValue, x);

            handle.anchorMin = handle.anchorMax = new Vector2(xValue, handle.anchorMax.y);
        }

        public void SetY(float y)
        {
            float yValue = Mathf.InverseLerp(minValue, maxValue, y);

            handle.anchorMin = handle.anchorMax = new Vector2(handle.anchorMax.x, yValue);
        }
    }
}
