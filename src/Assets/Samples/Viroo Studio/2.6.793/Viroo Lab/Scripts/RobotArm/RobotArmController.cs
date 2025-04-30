using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using Viroo.Interactions;
using Virtualware.Networking.Client;
using Virtualware.Networking.Client.Components;

namespace VirooLab
{
    public class RobotArmController : BroadcastObjectAction
    {
        private const string Take = nameof(Take);
        private const string Drop = nameof(Drop);
        private const string Start = nameof(Start);

        [SerializeField]
        private Transform objectsParent = default;

        [SerializeField]
        private NetworkObject networkObject = default;

        [SerializeField]
        private Animator animator = default;

        [SerializeField]
        private UnityEventAction activateButtonAction = default;

        private GameObject currentCube;

        protected override void Awake()
        {
            base.Awake();

            animator.enabled = false;
        }

        public void AnimateRobot(GameObject cube)
        {
            if (animator.GetBool(Start))
            {
                return;
            }

            currentCube = cube;

            if (networkObject.Authority)
            {
                animator.enabled = true;
                animator.SetBool(Start, value: true);
            }
            else
            {
                animator.enabled = false;
            }
        }

        public void TakeObject()
        {
            if (networkObject.Authority)
            {
                Execute(Take);
            }
        }

        public void DropObject()
        {
            if (networkObject.Authority)
            {
                Execute(Drop);
            }
        }

        protected override void LocalExecuteImplementation(string data)
        {
            switch (data)
            {
                case Take:
                    currentCube.transform.SetParent(objectsParent);
                    currentCube.GetComponent<NetworkTransform>().enabled = false;
                    currentCube.GetComponent<Rigidbody>().isKinematic = true;
                    currentCube.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

                    break;
                case Drop:
                    currentCube.transform.SetParent(p: null);
                    currentCube.GetComponent<NetworkTransform>().enabled = true;
                    _ = currentCube.GetComponent<NetworkTransform>()
                          .SetTransform(
                              currentCube.transform.position,
                              currentCube.transform.rotation,
                              currentCube.transform.localScale,
                              destroyCancellationToken);

                    currentCube.GetComponent<Rigidbody>().isKinematic = false;

                    if (networkObject.Authority)
                    {
                        animator.SetBool(Start, value: false);

                        activateButtonAction.Execute();
                    }

                    currentCube.GetComponent<XRGrabInteractable>().enabled = true;
                    currentCube = null;

                    break;
            }
        }
    }
}
