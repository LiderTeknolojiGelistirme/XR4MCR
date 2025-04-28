using CustomGraphics;
using Managers;
using Models;
using TMPro;
using UnityEngine;

namespace Presenters
{
    public class ConnectionLabelPresenter : MonoBehaviour
    {
        private ConnectionLabel _connectionLabel;
        private TextMeshProUGUI _text;
        
        void OnEnable()
        {
            if (!_connectionLabel.TMPTextComponent)
            {
                _connectionLabel.TMPTextComponent = GetComponent<TMP_Text>() ? GetComponent<TMP_Text>() : gameObject.AddComponent<TMP_Text>();
            }
        }

        private void Awake()
        {
            _text = gameObject.AddComponent<TextMeshProUGUI>();
            _text.alignment = TextAlignmentOptions.Center;
            _text.fontSize = 12;
        }

        public void UpdateLabel(Line line)
        {
            System.Tuple<Vector2, float> pointInLine = line.LerpLine(0.5f);

            Vector3 position = transform.position;

            Vector3 pointPos = LTGUtility.ScreenToWorldPointsForRenderMode(_connectionLabel._graphManager, pointInLine.Item1);

            if (!float.IsNaN(pointPos.x))
                position.x = pointPos.x;
            if (!float.IsNaN(pointPos.y))
                position.y = pointPos.y;
            // position.z = 0;
            transform.position = position;

            Vector3 angle = transform.eulerAngles;

            if (_connectionLabel.labelAngleType == ConnectionLabel.LabelAngleType.follow)
            {
                float lineAngleDeg = (pointInLine.Item2 * Mathf.Rad2Deg);

                // v4.1 - bugfix: label flipped down when connected ports are horizontally aligned
                // calc text angle
                if ((lineAngleDeg > -90 && lineAngleDeg < 0) || (lineAngleDeg > 180 && lineAngleDeg <= 270))
                {
                    angle.z = lineAngleDeg + 90 + _connectionLabel.angleOffset;
                }
                else
                {
                    angle.z = lineAngleDeg - 90 + _connectionLabel.angleOffset;
                }
            }
            else if (_connectionLabel.labelAngleType == ConnectionLabel.LabelAngleType.fixed_)
            {
                angle.z = _connectionLabel.angleOffset;
            }

            if (!float.IsNaN(angle.z))
                transform.eulerAngles = angle;

            _connectionLabel.TMPTextComponent.text = _connectionLabel.text;

            if (_connectionLabel.adjustScaleOnUpdate)
            {
                float scale = 1 / _connectionLabel._graphManager.LineRenderer.rectScaleX;
                transform.localScale = new Vector3(scale, scale, 1);
            }
        }

        public void SetText(string text)
        {
            _connectionLabel.text = text;
            if (_text != null)
                _text.text = text;
        }
    }
}