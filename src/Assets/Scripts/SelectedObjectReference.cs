using Managers;
using System;
using UnityEngine;
using Zenject;

public class SelectedObjectReference : MonoBehaviour
{
    [Inject] private SystemManager SystemManager;
    public event Action<GameObject> OnSelectedObjectChanged;

    private GameObject _selectedObject;
    public GameObject selectedObject
    {
        get => _selectedObject;
        private set
        {
            if (_selectedObject != value)
            {
                _selectedObject = value;
                // Nesne deðiþtiðinde event'i tetikle
                OnSelectedObjectChanged?.Invoke(_selectedObject);
            }
        }
    }

    private void Update()
    {
        selectedObject = SystemManager.Selected3DObject;
    }
}