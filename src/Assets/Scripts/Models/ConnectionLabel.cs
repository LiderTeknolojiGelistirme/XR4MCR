using Managers;
using TMPro;
using UnityEngine;

namespace Models
{
    [System.Serializable]
    public class ConnectionLabel
    {
        [SerializeField] string _labelText;
        public string text
        {
            get => _labelText;
            set
            {
                TMPTextComponent.text = value;
                _labelText = value;
            }
        }
        public TMP_Text TMPTextComponent;
        public enum LabelAngleType { follow, fixed_ }
        public bool adjustScaleOnUpdate = true;
        public LabelAngleType labelAngleType;
        public float angleOffset;

        [SerializeField] public GraphManager _graphManager;
    }
}