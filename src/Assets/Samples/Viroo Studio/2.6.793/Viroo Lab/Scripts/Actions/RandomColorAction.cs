using UnityEngine;
using Viroo.Interactions;

namespace VirooLab.Actions
{
    public class RandomColorAction : BroadcastObjectAction
    {
        [SerializeField]
        private Renderer cubeRenderer = default;

        [SerializeField]
        private Color color = default;

        protected override void LocalExecuteImplementation(string data)
        {
            color = Random.ColorHSV();
            cubeRenderer.material.color = color;
        }
    }
}
