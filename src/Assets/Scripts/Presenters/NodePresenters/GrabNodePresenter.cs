using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Presenters.NodePresenters
{
    public class GrabNodePresenter : BaseNodePresenter
    {
        public GameObject simpleInteractable;

        public Button selectObjectButton;
        public Button selectTargetButton;
        public TMP_InputField selectObjectInputField;
        public TMP_InputField selectTargetInputField;
        public GameObject selectTargetGhostPrefab;
        private GameObject _instantiatedTargetGhostGameObject;
        
        private bool _holdingTarget = false;

        private void Awake()
        {
            selectObjectButton.onClick.AddListener(OnSelectObject);
            selectTargetButton.onClick.AddListener(OnSelectTarget);
        }

        private void OnDisable()
        {
            selectObjectButton.onClick.RemoveAllListeners();
            selectTargetButton.onClick.RemoveAllListeners();
        }

        protected override void Update()
        {
            base.Update();
            if (_holdingTarget)
            {
                if (XRInputManager.GetRawTriggerState())
                {
                    var parent = GameObject.Find("Root").transform;
                    Debug.Log(parent.name);
                    _instantiatedTargetGhostGameObject.transform.parent = parent;
                    _holdingTarget = false;
                }
            }
        }


        public override void ActivateNode()
        {
            base.ActivateNode();
        }

        public override void StartNode()
        {
            Debug.Log("Start GrabNode");
            base.StartNode();
            
        }

        public override void CompleteNode()
        {
            Debug.Log("Complete GrabNode");
            base.CompleteNode();
        }

        public override void DeactivateNode()
        {
            base.DeactivateNode();
        }

        public override void Play()
        {
            base.Play();
            
            if (Vector3.Distance(simpleInteractable.transform.position, _instantiatedTargetGhostGameObject.transform.position) < 1f)
            {
                Debug.Log("Grab and drop complete");
                CompleteNode();
            }
        }

        public void OnSelectObject()
        {
            try
            {
                simpleInteractable = SystemManager.Selected3DObject;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }
            selectObjectInputField.text = simpleInteractable.name;
        }

        public void OnSelectTarget()
        {
            if (_instantiatedTargetGhostGameObject == null)
            {
                _instantiatedTargetGhostGameObject =
                    Instantiate(selectTargetGhostPrefab, XRInputManager.xrRayInteractor.transform);
                selectTargetInputField.text = _instantiatedTargetGhostGameObject.name;
                _holdingTarget = true;
            }
            else
            {
                _instantiatedTargetGhostGameObject.transform.SetParent(XRInputManager.xrRayInteractor.transform);
                _instantiatedTargetGhostGameObject.transform.localPosition = Vector3.zero;
                _holdingTarget = true;
            }
        }
    }
}