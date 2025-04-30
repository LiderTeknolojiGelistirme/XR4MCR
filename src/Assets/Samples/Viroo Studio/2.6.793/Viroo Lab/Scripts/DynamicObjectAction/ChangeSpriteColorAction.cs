using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using Viroo.Interactions;

namespace VirooLab
{
    public class ChangeSpriteColorAction : BroadcastObjectAction
    {
        [SerializeField]
        private Graphic targetGraphic = default;

        public override void Execute(string data)
        {
            Color color = Random.ColorHSV();
            string serializedColor = string.Format(CultureInfo.InvariantCulture, "{0}:{1}:{2}", color.r, color.g, color.b);

            base.Execute(serializedColor);
        }

        protected override void LocalExecuteImplementation(string data)
        {
            string[] components = data.Split(':');
            targetGraphic.color = new(
                float.Parse(components[0], CultureInfo.InvariantCulture),
                float.Parse(components[1], CultureInfo.InvariantCulture),
                float.Parse(components[2], CultureInfo.InvariantCulture));
        }

        public override void RestoreState(string data) => LocalExecuteImplementation(data);
    }
}
