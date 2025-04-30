using System;
using UnityEngine;
using UnityEngine.Events;
using Viroo.Input;
using Viroo.Input.XR;

namespace VirooLab
{
    public class ControllerButtonInputEvents : MonoBehaviour
    {
        private const string Pressed = nameof(Pressed);
        private const string UnPressed = nameof(UnPressed);

        [SerializeField]
        private bool isLeft = default;

        [SerializeField]
        private UnityEvent<string> onTriggerButtonPressed = default;

        [SerializeField]
        private UnityEvent<string> onGripButtonPressed = default;

        [SerializeField]
        private UnityEvent<string> onPrimaryButtonPressed = default;

        [SerializeField]
        private UnityEvent<string> onSecondaryButtonPressed = default;

        [SerializeField]
        private UnityEvent<string> onPrimary2DAxisPressed = default;

        private XRControllerInput controllerInput;

        protected void Inject(IDeviceInputProvider deviceInputProvider)
        {
            if (isLeft)
            {
                controllerInput = deviceInputProvider.LeftController;
            }
            else
            {
                controllerInput = deviceInputProvider.RightController;
            }

            controllerInput.OnTriggerPressed += OnTriggerPressed;
            controllerInput.OnGripPressed += OnGripPressed;
            controllerInput.OnPrimaryButtonPressed += OnPrimaryButtonPressed;
            controllerInput.OnSecondaryButtonPressed += OnSecondaryButtonPressed;
            controllerInput.OnPrimary2DAxisPressed += OnPrimary2DAxisClicked;

            controllerInput.OnTriggerCanceled += OnTriggerCanceled;
            controllerInput.OnGripCanceled += OnGripCancelled;
            controllerInput.OnPrimaryButtonCanceled += OnPrimaryButtonCanceled;
            controllerInput.OnSecondaryButtonCanceled += OnSecondaryButtonCanceled;
            controllerInput.OnPrimary2DAxisCanceled += OnPrimary2DAxisCanceled;
        }

        protected void Awake()
        {
            this.QueueForInject();
        }

        private void OnTriggerPressed(object sender, EventArgs e) => onTriggerButtonPressed?.Invoke(Pressed);
        private void OnGripPressed(object sender, EventArgs e) => onGripButtonPressed?.Invoke(Pressed);
        private void OnPrimaryButtonPressed(object sender, EventArgs e) => onPrimaryButtonPressed?.Invoke(Pressed);
        private void OnSecondaryButtonPressed(object sender, EventArgs e) => onSecondaryButtonPressed?.Invoke(Pressed);
        private void OnPrimary2DAxisClicked(object sender, EventArgs e) => onPrimary2DAxisPressed?.Invoke(Pressed);

        private void OnTriggerCanceled(object sender, EventArgs e) => onTriggerButtonPressed.Invoke(UnPressed);
        private void OnGripCancelled(object sender, EventArgs e) => onGripButtonPressed?.Invoke(UnPressed);
        private void OnPrimaryButtonCanceled(object sender, EventArgs e) => onPrimaryButtonPressed?.Invoke(UnPressed);
        private void OnSecondaryButtonCanceled(object sender, EventArgs e) => onSecondaryButtonPressed?.Invoke(UnPressed);
        private void OnPrimary2DAxisCanceled(object sender, EventArgs e) => onPrimary2DAxisPressed?.Invoke(UnPressed);
    }
}
