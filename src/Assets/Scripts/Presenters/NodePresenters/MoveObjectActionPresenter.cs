using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Models.Nodes;
using Managers;
using TMPro;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Presenters.NodePresenters
{
    public class MoveObjectActionPresenter : ActionNodePresenter
    {
        [SerializeField] private TMP_Dropdown targetObjectsDropdown;
        [SerializeField] private Button addButton;
        [SerializeField] private Button removeButton;
        [SerializeField] private Button selectButton;

        [SerializeField] private GameObject targetGhostGameObject;
        private List<GameObject> _gameObjects = new List<GameObject>();
        private bool _holdingTarget = false;
        private GameObject _selectedTargetGameObject;

        private void Start()
        {
            //Model.Description = "Move the selected object to the target position";
        }


        protected override void Awake()
        {
            base.Awake();
            
            // Log: MoveObjectActionPresenter oluşturuldu
            LogManager.LogSuccess("MoveObjectActionPresenter started: " + gameObject.name);
        }

        private void OnEnable()
        {
            addButton.onClick.AddListener(OnAddButtonClicked);
            removeButton.onClick.AddListener(OnRemoveButtonClicked);
            targetObjectsDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
            selectButton.onClick.AddListener(OnSelectButtonClicked);
        }

        

        protected override void Update()
        {
            if (_holdingTarget)
            {
                if (XRInputManager.GetRawTriggerState())
                {
                    var parent = GameObject.Find("Root").transform;
                    Debug.Log(parent.name);
                    _selectedTargetGameObject.transform.parent = parent;
                    _holdingTarget = false;
                }
            }
        }


        protected override void PerformAction()
        {
        }
        private void OnSelectButtonClicked()
        {
            LogManager.LogInteraction("Select target position button clicked");
            
            _selectedTargetGameObject.transform.SetParent(XRInputManager.xrRayInteractor.transform);
            _selectedTargetGameObject.transform.localPosition = Vector3.zero;
            _holdingTarget = true;
            
            LogManager.LogSuccess("Target position selection started");
        }
        private void OnRemoveButtonClicked()
        {
            LogManager.LogInteraction("Remove target button clicked");
            
            int selectedIndex = targetObjectsDropdown.value;
            
            if(selectedIndex >= 0 && selectedIndex < _gameObjects.Count)
            {
                GameObject go = _gameObjects[selectedIndex];
                Destroy(go.gameObject);
                _gameObjects.RemoveAt(selectedIndex);
                UpdateDropdownOptions();
            
                Debug.Log("Removed target index: " + selectedIndex);
                LogManager.LogSuccess($"Target removed: index {selectedIndex}");
            }
            else
            {
                Debug.LogWarning("No valid target selected for removal!");
                LogManager.LogWarning("No valid target selected for removal");
            }
        }

        private void OnAddButtonClicked()
        {
            LogManager.LogInteraction("Add target button clicked");
            
            if (targetGhostGameObject != null)
            {
                _selectedTargetGameObject = Instantiate(targetGhostGameObject,XRInputManager.xrRayInteractor.transform);
                _gameObjects.Add(_selectedTargetGameObject);
                _selectedTargetGameObject.name = _selectedTargetGameObject.name + " " +
                                                 _gameObjects.IndexOf(_selectedTargetGameObject);
                _holdingTarget = true;
                
                LogManager.LogSuccess($"New target added: {_selectedTargetGameObject.name}");
            }
            else
            {
                LogManager.LogWarning("Target ghost prefab not assigned");
            }

            UpdateDropdownOptions();
        }

        private void UpdateDropdownOptions()
        {
            targetObjectsDropdown.ClearOptions();
            List<string> options = new List<string>();

            foreach (GameObject g in _gameObjects)
            {
                options.Add(g.name);
            }

            targetObjectsDropdown.AddOptions(options);
            targetObjectsDropdown.value = _gameObjects.IndexOf(_selectedTargetGameObject);
        }

        private void OnDropdownValueChanged(int selectedIndex)
        {
            LogManager.LogInteraction($"Target dropdown selection changed: index {selectedIndex}");
            
            if (selectedIndex >= 0 && selectedIndex < _gameObjects.Count)
            {
                GameObject selectedGameObject = _gameObjects[selectedIndex];
                Debug.Log("Selected target: " + selectedGameObject.name + " - Position: " +
                          selectedGameObject.transform.position);
                _selectedTargetGameObject = selectedGameObject;
                
                LogManager.LogSuccess($"Target selected: {selectedGameObject.name}");
            }
            else
            {
                Debug.LogWarning("Selected index is not valid!");
                LogManager.LogWarning("Invalid target index selected");
            }
        }
    }
}