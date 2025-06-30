using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using Managers;
using System.Collections;

namespace Helpers
{
    /// <summary>
    /// Manual XR test için basit kontrol scripti
    /// </summary>
    public class SimpleXRTest : MonoBehaviour
    {
        [Header("Test Settings")]
        public KeyCode testKey = KeyCode.T;
        public KeyCode switchControllerKey = KeyCode.C;
        public float autoTestInterval = 5f;
        public bool enableAutoTest = true;

        private void Start()
        {
            LogManager.Log("=== SIMPLE XR TEST STARTED ===");
            
            if (enableAutoTest)
            {
                StartCoroutine(AutoTest());
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(testKey))
            {
                PerformManualTest();
            }
            
            if (Input.GetKeyDown(switchControllerKey))
            {
                SwitchController();
            }
        }

        private void SwitchController()
        {
            LogManager.Log("=== MANUAL CONTROLLER SWITCH ===");
            
            var xrInputManager = FindObjectOfType<XRInputManager>();
            if (xrInputManager != null)
            {
                // XRInputManager'daki switch metodunu çağır
                if (Input.GetKeyDown(switchControllerKey))
                {
                    LogManager.Log("TEST: Requesting controller switch via XRInputManager");
                }
            }
            else
            {
                LogManager.LogError("TEST: XRInputManager not found!");
            }
        }

        private IEnumerator AutoTest()
        {
            while (true)
            {
                yield return new WaitForSeconds(autoTestInterval);
                PerformManualTest();
            }
        }

        private void PerformManualTest()
        {
            LogManager.Log("=== MANUAL XR TEST ===");
            
            // 1. XRRayInteractor sayısı
            var allInteractors = FindObjectsOfType<XRRayInteractor>();
            LogManager.Log($"TEST: Total XRRayInteractors found: {allInteractors.Length}");
            
            // 2. Her interactor detayı
            for (int i = 0; i < allInteractors.Length; i++)
            {
                var interactor = allInteractors[i];
                LogManager.Log($"TEST: [{i}] Name: {interactor.name}, Active: {interactor.gameObject.activeInHierarchy}, Enabled: {interactor.enabled}");
                
                // Raycast test
                if (interactor.gameObject.activeInHierarchy && interactor.enabled)
                {
                    bool hasHit = interactor.TryGetCurrent3DRaycastHit(out RaycastHit hit);
                    LogManager.Log($"TEST: [{i}] Raycast Hit: {hasHit}");
                    if (hasHit)
                    {
                        LogManager.Log($"TEST: [{i}] Hit Object: {hit.transform.name}, Point: {hit.point}");
                        
                        // Screen position test
                        if (Camera.main != null)
                        {
                            Vector3 screenPos = Camera.main.WorldToScreenPoint(hit.point);
                            LogManager.Log($"TEST: [{i}] Screen Position: {screenPos}");
                        }
                    }
                }
            }
            
            // 3. XRInputManager durumu
            var xrInputManager = FindObjectOfType<XRInputManager>();
            if (xrInputManager != null)
            {
                LogManager.Log($"TEST: XRInputManager current interactor: {(xrInputManager.xrRayInteractor != null ? xrInputManager.xrRayInteractor.name : "NULL")}");
            }
            
            // 4. Pointer durumu
            var pointer = FindObjectOfType<Pointer>();
            if (pointer != null)
            {
                var image = pointer.GetPointerImage();
                LogManager.Log($"TEST: Pointer image enabled: {(image != null ? image.enabled.ToString() : "NULL")}");
            }
            
            LogManager.Log("=== MANUAL XR TEST COMPLETED ===");
        }
    }
} 