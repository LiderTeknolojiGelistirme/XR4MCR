using UnityEngine;
using Managers;
using System.Collections;
using RuntimeGizmos;

namespace Helpers
{
    /// <summary>
    /// VIROO runtime ortamında kritik bileşenlerin var olup olmadığını kontrol eder
    /// </summary>
    public class VIROODebugChecker : MonoBehaviour
    {
        [Header("Check Delay")]
        [SerializeField] private float checkDelay = 2f; // 2 saniye bekle ki diğer objeler initialize olsun

        private void Start()
        {
            LogManager.Log("=== VIROO DEBUG CHECKER STARTED ===");
            StartCoroutine(DelayedCheck());
        }

        private IEnumerator DelayedCheck()
        {
            yield return new WaitForSeconds(checkDelay);
            
            LogManager.Log("=== PERFORMING VIROO DEBUG CHECKS ===");
            
            CheckSystemManager();
            CheckGraphManager();
            CheckXRInputManager();
            CheckTransformGizmo();
            CheckPointer();
            CheckCamera();
            CheckXRRayInteractor();
            
            LogManager.Log("=== VIROO DEBUG CHECKS COMPLETED ===");
        }

        private void CheckSystemManager()
        {
            var systemManager = FindObjectOfType<SystemManager>();
            LogManager.Log($"VIROO CHECK: SystemManager found: {systemManager != null}");
            if (systemManager != null)
            {
                LogManager.Log($"VIROO CHECK: SystemManager name: {systemManager.gameObject.name}");
                LogManager.Log($"VIROO CHECK: SystemManager active: {systemManager.gameObject.activeInHierarchy}");
                LogManager.Log($"VIROO CHECK: SystemManager Selected3DObject: {systemManager.Selected3DObject != null}");
            }
        }

        private void CheckGraphManager()
        {
            var graphManager = FindObjectOfType<GraphManager>();
            LogManager.Log($"VIROO CHECK: GraphManager found: {graphManager != null}");
            if (graphManager != null)
            {
                LogManager.Log($"VIROO CHECK: GraphManager name: {graphManager.gameObject.name}");
                LogManager.Log($"VIROO CHECK: GraphManager active: {graphManager.gameObject.activeInHierarchy}");
                LogManager.Log($"VIROO CHECK: GraphManager Pointer: {graphManager.Pointer != null}");
                LogManager.Log($"VIROO CHECK: GraphManager Canvas: {graphManager.Canvas != null}");
            }
        }

        private void CheckXRInputManager()
        {
            var xrInputManager = FindObjectOfType<XRInputManager>();
            LogManager.Log($"VIROO CHECK: XRInputManager found: {xrInputManager != null}");
            if (xrInputManager != null)
            {
                LogManager.Log($"VIROO CHECK: XRInputManager name: {xrInputManager.gameObject.name}");
                LogManager.Log($"VIROO CHECK: XRInputManager active: {xrInputManager.gameObject.activeInHierarchy}");
                LogManager.Log($"VIROO CHECK: XRInputManager xrRayInteractor: {xrInputManager.xrRayInteractor != null}");
            }
        }

        private void CheckTransformGizmo()
        {
            var transformGizmo = FindObjectOfType<TransformGizmo>();
            LogManager.Log($"VIROO CHECK: TransformGizmo found: {transformGizmo != null}");
            if (transformGizmo != null)
            {
                LogManager.Log($"VIROO CHECK: TransformGizmo name: {transformGizmo.gameObject.name}");
                LogManager.Log($"VIROO CHECK: TransformGizmo active: {transformGizmo.gameObject.activeInHierarchy}");
                LogManager.Log($"VIROO CHECK: TransformGizmo camera: {transformGizmo.myCamera != null}");
            }
        }

        private void CheckPointer()
        {
            var pointer = FindObjectOfType<Pointer>();
            LogManager.Log($"VIROO CHECK: Pointer found: {pointer != null}");
            if (pointer != null)
            {
                LogManager.Log($"VIROO CHECK: Pointer name: {pointer.gameObject.name}");
                LogManager.Log($"VIROO CHECK: Pointer active: {pointer.gameObject.activeInHierarchy}");
                LogManager.Log($"VIROO CHECK: Pointer ImageIsActive: {pointer.ImageIsActive}");
                
                var image = pointer.GetPointerImage();
                if (image != null)
                {
                    LogManager.Log($"VIROO CHECK: Pointer Image enabled: {image.enabled}");
                    LogManager.Log($"VIROO CHECK: Pointer Image sprite: {(image.sprite != null ? image.sprite.name : "NULL")}");
                }
                else
                {
                    LogManager.LogError("VIROO CHECK: Pointer Image is NULL!");
                }
            }
        }

        private void CheckCamera()
        {
            var mainCamera = Camera.main;
            LogManager.Log($"VIROO CHECK: Camera.main found: {mainCamera != null}");
            if (mainCamera != null)
            {
                LogManager.Log($"VIROO CHECK: Camera.main name: {mainCamera.gameObject.name}");
                LogManager.Log($"VIROO CHECK: Camera.main active: {mainCamera.gameObject.activeInHierarchy}");
                LogManager.Log($"VIROO CHECK: Camera.main enabled: {mainCamera.enabled}");
            }
        }

        private void CheckXRRayInteractor()
        {
            var xrRayInteractors = FindObjectsOfType<UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor>();
            LogManager.Log($"VIROO CHECK: XRRayInteractor count: {xrRayInteractors.Length}");
            
            // Hangi interactorlar var?
            for (int i = 0; i < xrRayInteractors.Length; i++)
            {
                var rayInteractor = xrRayInteractors[i];
                LogManager.Log($"VIROO CHECK: XRRayInteractor[{i}] name: {rayInteractor.gameObject.name}");
                LogManager.Log($"VIROO CHECK: XRRayInteractor[{i}] active: {rayInteractor.gameObject.activeInHierarchy}");
                LogManager.Log($"VIROO CHECK: XRRayInteractor[{i}] enabled: {rayInteractor.enabled}");
                LogManager.Log($"VIROO CHECK: XRRayInteractor[{i}] type: {rayInteractor.GetType().Name}");
                
                // Raycast test et
                if (rayInteractor.gameObject.activeInHierarchy && rayInteractor.enabled)
                {
                    bool hasHit = rayInteractor.TryGetCurrent3DRaycastHit(out UnityEngine.RaycastHit hit);
                    LogManager.Log($"VIROO CHECK: XRRayInteractor[{i}] has raycast hit: {hasHit}");
                    if (hasHit)
                    {
                        LogManager.Log($"VIROO CHECK: XRRayInteractor[{i}] hit object: {(hit.transform != null ? hit.transform.name : "NULL")}");
                        LogManager.Log($"VIROO CHECK: XRRayInteractor[{i}] hit point: {hit.point}");
                    }
                }
            }
            
            // Hangi interactor tercih edilmeli?
            string[] preferredNames = {"Right Ray Interactor", "Left Ray Interactor", "XRRayInteractor", "Hand Ray Interactor"};
            string[] blockedNames = {"XRGazeInteractor", "Gaze"};
            
            LogManager.Log("VIROO CHECK: Analyzing best interactor choice...");
            foreach (var rayInteractor in xrRayInteractors)
            {
                bool isPreferred = false;
                bool isBlocked = false;
                
                foreach (string pref in preferredNames)
                {
                    if (rayInteractor.name.Contains(pref))
                    {
                        isPreferred = true;
                        break;
                    }
                }
                
                foreach (string blocked in blockedNames)
                {
                    if (rayInteractor.name.Contains(blocked))
                    {
                        isBlocked = true;
                        break;
                    }
                }
                
                LogManager.Log($"VIROO CHECK: {rayInteractor.name} - Preferred: {isPreferred}, Blocked: {isBlocked}");
            }
        }
    }
} 